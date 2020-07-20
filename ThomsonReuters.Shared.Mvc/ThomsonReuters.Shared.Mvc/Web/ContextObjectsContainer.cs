using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;
using ThomsonReuters.Configuration;
using ThomsonReuters.Utilities;

namespace ThomsonReuters.Shared.Web
{
	/// <summary>
	/// Work only in web context.
	/// </summary>
	public class ContextObjectsContainer<TContextObjectsContainer>
		where TContextObjectsContainer : ContextObjectsContainer<TContextObjectsContainer>, new()
	{
		private const string CONTEXT_KEY = "ContextObjectsContainer";
		private const string CONFIG_SECTION = "contextObjectsMappings";
		private const int GARBAGE_TRUCK_POLLING_INTERVAL = 5 * 1000;
		//private const bool USE_GARBAGE_TRUCK_ALGORITHM = false;

		private static Timer _timerGarbageTruck = null;

		[ThreadStatic]
		private static TContextObjectsContainer _failsafeContainer;
		private static Dictionary<int, Tuple<TContextObjectsContainer, Thread>> _failsafeContainers = new Dictionary<int, Tuple<TContextObjectsContainer, Thread>>();

		private static Dictionary<Type, Type> _bindings = new Dictionary<Type, Type>();
		private Dictionary<Type, object> _contextObjects = new Dictionary<Type, object>();

		private static Dictionary<bool, int> _instanceCounterStore = new Dictionary<bool, int>();

		private static object _lockObj_Static = new object();
		private static object _lockObj_Static_2 = new object();
		private object _lockObj_Instance = new object();


		static ContextObjectsContainer()
		{
			RefreshConfigMappings();

			//if (USE_GARBAGE_TRUCK_ALGORITHM)
			//{
			//	_timerGarbageTruck = new Timer(new TimerCallback(HandleGarbageTruckArrived), null, GARBAGE_TRUCK_POLLING_INTERVAL, GARBAGE_TRUCK_POLLING_INTERVAL);
			//}
		}

		public ContextObjectsContainer()
		{
			IsInHttpContext = HttpContext.Current != null;

			UpdateInstanceRef(this, true);
		}


		public static TContextObjectsContainer Current
		{
			get
			{
				try
				{
					Monitor.Enter(_lockObj_Static);

					if (HttpContext.Current != null)
					{
						var curr = HttpContext.Current.Items[CONTEXT_KEY] as TContextObjectsContainer;
						if (curr == null)
						{
							curr = CreateContainer();
							HttpContext.Current.Items[CONTEXT_KEY] = curr;
						}
						return curr;
					}
					else
					{
						if (_failsafeContainer == null)
						{
							_failsafeContainer = CreateContainer();
						}

						return _failsafeContainer;

						//if (USE_GARBAGE_TRUCK_ALGORITHM)
						//{
						//	var thread = Thread.CurrentThread;
						//	var tup = default(Tuple<TContextObjectsContainer, Thread>);

						//	if (!_failsafeContainers.TryGetValue(thread.ManagedThreadId, out tup))
						//	{
						//		var newContainer = CreateContainer();

						//		tup = Tuple.Create(newContainer, thread);

						//		_failsafeContainers.Add(thread.ManagedThreadId, tup);
						//	}

						//	return tup.Item1;
						//}
						//else
						//{
						//	if (_failsafeContainer == null)
						//	{
						//		_failsafeContainer = CreateContainer();
						//	}

						//	return _failsafeContainer;
						//}
					}
				}
				finally
				{
					Monitor.Exit(_lockObj_Static);
				}
			}
		}


		public static void RefreshConfigMappings()
		{
			var cMappings = LoadConfigMappings();

			if (cMappings.Mappings.HasAny())
			{
				foreach (var map in cMappings.Mappings)
				{
					var typeSource = Type.GetType(map.Source);
					var typeTarget = Type.GetType(map.Target);

					Bind(typeSource, typeTarget);
				}
			}
		}

		public static void Bind<TAbstraction, TImplementation>()
			where TImplementation : TAbstraction
		{
			var typeAbstract = typeof(TAbstraction);
			var typeImplement = typeof(TImplementation);

			Bind(typeAbstract, typeImplement);
		}

		public static void Bind(Type typeAbstraction, Type typeImplementation)
		{
			if (_bindings.ContainsKey(typeAbstraction))
			{
				_bindings.Remove(typeAbstraction);
			}

			_bindings.Add(typeAbstraction, typeImplementation);
		}


		public bool IsInHttpContext { get; private set; }


		public T Get<T>()
		{
			return (T)Get(typeof(T));
		}

		public T Get<T>(Type type)
		{
			return (T)Get(type);
		}

		public object Get(Type type)
		{
			var ret = default(object);

			try
			{
				Monitor.Enter(_lockObj_Instance);

				if (!_contextObjects.TryGetValue(type, out ret))
				{
					ret = CreateObject(type, true);
				}
			}
			finally
			{
				Monitor.Exit(_lockObj_Instance);
			}

			return ret;
		}

		public void Clear()
		{
			_contextObjects.Clear();
		}

		public void Refresh<T>()
		{
			Refresh(typeof(T));
		}

		public void Refresh(Type type)
		{
			try
			{
				Monitor.Enter(_lockObj_Instance);

				CreateObject(type, true);
			}
			finally
			{
				Monitor.Exit(_lockObj_Instance);
			}
		}

		public T New<T>()
		{
			var ret = (T)New(typeof(T));
			return ret;
		}

		public object New(Type type)
		{
			var ret = CreateObject(type, false);
			return ret;
		}


		private static TContextObjectsContainer CreateContainer()
		{
			var ret = new TContextObjectsContainer();
			return ret;
		}

		private object CreateObject(Type type, bool updateContext)
		{
			if (!type.IsClass && !type.IsValueType && !_bindings.ContainsKey(type))
			{
				var msg = string.Format("Cannot create {0}. For a non class or non struct type, use 'Bind' function.", type.ToString());
				throw new ApplicationException(msg);
			}

			var createType = _bindings.ContainsKey(type) ? _bindings[type] : type;
			var ret = default(object);

			if (updateContext)
			{
				if (_contextObjects.ContainsKey(type))
				{
					_contextObjects.Remove(type);
				}

				ret = Activator.CreateInstance(createType, false);

				_contextObjects.Add(type, ret);
			}
			else
			{
				ret = Activator.CreateInstance(createType, false);
			}

			return ret;
		}


		private static void HandleGarbageTruckArrived(object state)
		{
			try
			{
				Monitor.Enter(_lockObj_Static);

				var keys = _failsafeContainers.Keys.ToArray();

				foreach (var key in keys)
				{
					var tup = _failsafeContainers[key];

					if (!tup.Item2.IsAlive)
					{
						tup.Item1._contextObjects.Clear();

						_failsafeContainers.Remove(key);
					}
				}
			}
			finally
			{
				Monitor.Exit(_lockObj_Static);
			}
		}

		private static void UpdateInstanceRef(ContextObjectsContainer<TContextObjectsContainer> container, bool add)
		{
			lock (_lockObj_Static_2)
			{
				if (!_instanceCounterStore.ContainsKey(true))
				{
					_instanceCounterStore.Add(true, 0);
				}

				if (!_instanceCounterStore.ContainsKey(false))
				{
					_instanceCounterStore.Add(false, 0);
				}

				var isInHttpContext = container.IsInHttpContext;
				var count = _instanceCounterStore[isInHttpContext];

				count = add ? (count + 1) : (count - 1);

				_instanceCounterStore[isInHttpContext] = count;
			}
		}


		private static ContextObjectMappings LoadConfigMappings()
		{
			ConfigurationManager.RefreshSection(CONFIG_SECTION);

			var configSection = (InnerXmlConfigSection)ConfigurationManager.GetSection(CONFIG_SECTION);

			if (configSection != null)
			{
				var xmlString = configSection.Data.Content;

				var ret = SerializationUtilities.XMLDeserialize<ContextObjectMappings>(xmlString);
				return ret;
			}

			return new ContextObjectMappings { Mappings = new ContextObjectMapping[0] };
		}


		~ContextObjectsContainer()
		{
			UpdateInstanceRef(this, false);
		}


		[XmlType(AnonymousType = true)]
		[XmlRoot(Namespace = "", IsNullable = false, ElementName = "Mappings")]
		public class ContextObjectMappings
		{
			[XmlElement("Mapping")]
			public ContextObjectMapping[] Mappings { get; set; }
		}

		[XmlType(AnonymousType = true)]
		public class ContextObjectMapping
		{
			[XmlAttribute("Source")]
			public string Source { get; set; }
			[XmlAttribute("Target")]
			public string Target { get; set; }
		}
	}
}
