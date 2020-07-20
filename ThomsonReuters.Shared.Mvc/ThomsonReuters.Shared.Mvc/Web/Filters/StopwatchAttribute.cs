using System.Web.Mvc;
using System.Diagnostics;
using System.Collections.Generic;
using System;

namespace ThomsonReuters.Shared.Web.Filters
{
	public class StopwatchAttribute : ActionFilterAttribute
	{
		public enum OutputType
		{
			Debug,
			ResponseHeader,
		}

		private enum Stages
		{
			Global,
			Action,
			Intermediate,
			Result,
		}


		private const string CONTEXT_KEY = "Stopwatch";
		private const string CONTEXT_HEADER_KEY = "StopwatchControllerAction";

		public OutputType Output { get; set; }


		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
#if DEBUG
			StartWatch(filterContext);

			WatchStart(filterContext, Stages.Global);
			WatchStart(filterContext, Stages.Action);
#endif
		}

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
#if DEBUG
			WatchStop(filterContext, Stages.Action);

			ProcessStopped(filterContext, Stages.Action);

			WatchStart(filterContext, Stages.Intermediate);
#endif
		}

		public override void OnResultExecuting(ResultExecutingContext filterContext)
		{
#if DEBUG
			WatchStop(filterContext, Stages.Intermediate);

			ProcessStopped(filterContext, Stages.Intermediate);

			WatchStart(filterContext, Stages.Result);
#endif
		}

		public override void OnResultExecuted(ResultExecutedContext filterContext)
		{
#if DEBUG
			WatchStop(filterContext, Stages.Result);
			WatchStop(filterContext, Stages.Global);

			ProcessStopped(filterContext, Stages.Result);
			ProcessStopped(filterContext, Stages.Global);
#endif
		}


		private static void StartWatch(ActionExecutingContext context)
		{
			Dictionary<Stages, Stopwatch> dic = new Dictionary<Stages, Stopwatch>();

			dic.Add(Stages.Global, new Stopwatch());
			dic.Add(Stages.Action, new Stopwatch());
			dic.Add(Stages.Intermediate, new Stopwatch());
			dic.Add(Stages.Result, new Stopwatch());

			context.HttpContext.Items[CONTEXT_KEY] = dic;
			context.HttpContext.Items[CONTEXT_HEADER_KEY] = string.Format("{0}.{1}", context.ActionDescriptor.ControllerDescriptor.ControllerName, context.ActionDescriptor.ActionName);

		}

		private void ProcessStopped(ControllerContext context, Stages stage)
		{
			var watch = GetWatch(context, stage);

			if (watch != null)
			{
				var head = GetHeaderName(context, stage);
				var elapsed = GetTimeFormat(watch.Elapsed);

				switch (Output)
				{
					case OutputType.Debug:
						Debug.WriteLine(head + " => " + elapsed);
						break;
					case OutputType.ResponseHeader:
						context.HttpContext.Response.AddHeader(head, elapsed);
						break;
				}
			}
		}

		private static void WatchStart(ControllerContext context, Stages stage)
		{
			var watch = GetWatch(context, stage);

			if (watch != null)
			{
				watch.Start();
			}
		}

		private static void WatchStop(ControllerContext context, Stages stage)
		{
			var watch = GetWatch(context, stage);

			if (watch != null)
			{
				watch.Stop();
			}
		}


		private static Stopwatch GetWatch(ControllerContext context, Stages stage)
		{
			var cache = (Dictionary<Stages, Stopwatch>)context.HttpContext.Items[CONTEXT_KEY];

			Stopwatch ret = null;

			if (cache != null)
			{
				ret = cache[stage];
			}

			return ret;
		}

		private static string GetTimeFormat(TimeSpan ts)
		{
			var ret = string.Format("{0}:{1}:{2}", ts.Minutes, ts.Seconds, ts.Milliseconds);
			return ret;
		}

		private static string GetHeaderName(ControllerContext context, Stages stage)
		{
			var contrAction = context.HttpContext.Items[CONTEXT_HEADER_KEY];
			var ret = string.Format("Profiling:{0} ({1})", contrAction, stage);
			return ret;
		}
	}
}
