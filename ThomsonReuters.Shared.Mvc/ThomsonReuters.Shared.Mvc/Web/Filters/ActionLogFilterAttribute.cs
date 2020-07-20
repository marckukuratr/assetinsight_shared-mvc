using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using ThomsonReuters.Logging;
using ThomsonReuters.Shared.Model;
using ThomsonReuters.Shared.Repositories;

namespace ThomsonReuters.Shared.Web.Filters
{
	/// <summary>
	/// IMPORTANT! : Filters are cached since MVC 3. Be carefull to maintain states inside the filters
	/// </summary>
	public class ActionLogFilterAttribute : ActionFilterAttribute
	{
		private static XmlWriterSettings _writerSettings;

		public IActionLogFilterSettings Settings { get; set; }


		public ActionLogFilterAttribute()
		{
		}

		public ActionLogFilterAttribute(Type settingsType)
		{
			if (settingsType != null)
			{
				Settings = Activator.CreateInstance(settingsType) as IActionLogFilterSettings;
				IsActionLogDisabled = false;
			}
			else
			{
				IsActionLogDisabled = true;
			}
		}


		public bool IsActionLogDisabled { get; set; }

		public object EventType { get; set; }

		protected static XmlWriterSettings WriterSettings
		{
			get
			{
				if (_writerSettings == null)
				{
					_writerSettings = new XmlWriterSettings
					{
						CheckCharacters = true,
						CloseOutput = false,
						ConformanceLevel = ConformanceLevel.Document,
						Encoding = Encoding.Unicode,
						Indent = false,
						IndentChars = " ",
						NamespaceHandling = NamespaceHandling.Default,
						NewLineChars = "\r\n",
						NewLineHandling = NewLineHandling.Replace,
						NewLineOnAttributes = false,
						OmitXmlDeclaration = true
					};
				}
				return _writerSettings;
			}
		}


		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			try
			{
				if (!IsActionLogDisabled)
				{
					if (Settings.Repository == null)
					{
						throw new ApplicationException("No repository specified to log actions.");
					}

					var log = BuildActionLog(filterContext);
					Settings.Repository.AddToActionLogs(log, true);
				}
			}
			catch (Exception ex)
			{
				// Dont let the action logging affect user experience.
				TREventLog.Log(ex.ToString(), LogType.Error);
			}
			finally
			{
				base.OnActionExecuting(filterContext);
			}
		}


		protected IActionLog NewActionLog()
		{
			return Settings.NewActionLog();
		}

		protected virtual IActionLog BuildActionLog(ActionExecutingContext filterContext)
		{
			var log = NewActionLog();
			log.SessionId = filterContext.HttpContext.Session.SessionID;
			log.User = filterContext.HttpContext.User.Identity.Name;
			log.IP = filterContext.HttpContext.Request.UserHostAddress;
			log.Area = filterContext.RouteData.DataTokens.Keys.Contains("area") ? filterContext.RouteData.DataTokens["area"].ToString() : null;
			log.Controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
			log.Action = filterContext.ActionDescriptor.ActionName;
			log.Url = HttpHelper.ToAbsoluteUrl(filterContext.HttpContext.Request.Url.AbsoluteUri);
			log.UrlReferrer = filterContext.HttpContext.Request.UrlReferrer == null ? null : filterContext.HttpContext.Request.UrlReferrer.ToString();
			log.HttpMethod = filterContext.HttpContext.Request.HttpMethod != null ?
				(filterContext.HttpContext.Request.HttpMethod.Length > 6 ? filterContext.HttpContext.Request.HttpMethod.Substring(0, 6) : filterContext.HttpContext.Request.HttpMethod) : null;
			log.DateTime = DateTime.UtcNow;
			log.EventType = EventType == null ? null : EventType.ToString();
			log.Data = BuildData(filterContext);

			return log;
		}

		protected virtual string BuildData(ActionExecutingContext filterContext)
		{
			string ret = null;

			if (filterContext.ActionParameters.Count > 0 ||
				filterContext.HttpContext.Request.QueryString.Count > 0 ||
				filterContext.HttpContext.Request.Form.Count > 0)
			{
				StringBuilder sb = new StringBuilder();
				using (var wr = XmlTextWriter.Create(sb, WriterSettings))
				{
					wr.WriteStartElement("Data");

					// Action parameters
					WriteData_ActionParameters(filterContext, wr);
					// Query strings
					WriteData_QueryStrings(filterContext, wr);
					// Form data
					WriteData_Form(filterContext, wr);

					wr.WriteEndElement();
					wr.Flush();
				}

				ret = sb.ToString();
			}

			return ret;
		}


		protected void WriteData_HttpHeaders(ActionExecutingContext filterContext, XmlWriter writer, bool writeHeaderElement = false)
		{
			NameValueCollection col = filterContext.HttpContext.Request.Headers;
			WriteKeyValueFromCollection(col, writer, writeHeaderElement ? "HttpHeader" : null);
		}

		protected void WriteData_ActionParameters(ActionExecutingContext filterContext, XmlWriter writer, bool writeHeaderElement = false)
		{
			var col = filterContext.ActionParameters;
			WriteKeyValueFromCollection(col, writer, writeHeaderElement ? "ActionParameters" : null);
		}

		protected void WriteData_QueryStrings(ActionExecutingContext filterContext, XmlWriter writer, bool writeHeaderElement = false)
		{
			NameValueCollection col = filterContext.HttpContext.Request.QueryString;
			WriteKeyValueFromCollection(col, writer, writeHeaderElement ? "Query" : null);
		}

		protected void WriteData_Form(ActionExecutingContext filterContext, XmlWriter writer, bool writeHeaderElement = false)
		{
			NameValueCollection col = filterContext.HttpContext.Request.Form;
			WriteKeyValueFromCollection(col, writer, writeHeaderElement ? "Form" : null);
		}


		protected string GetFieldValues(string fieldNamesCsv, ActionExecutingContext filterContext, bool ignoreCase)
		{
			string ret = null;

			if (!string.IsNullOrWhiteSpace(fieldNamesCsv))
			{
				var fieldNames = fieldNamesCsv.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				if (fieldNames.Length > 0)
				{
					StringBuilder sb = new StringBuilder();

					foreach (var fname in fieldNames)
					{
						var fval = GetFieldValue(fname, filterContext, ignoreCase);
						string data = string.Format("{0}={1};", fname, fval);
						sb.Append(data);
					}

					ret = sb.Remove(sb.Length - 1, 1).ToString();
				}
			}

			return ret;
		}

		protected string GetFieldValue(string fieldName, ActionExecutingContext filterContext, bool ignoreCase)
		{
			string foundVal = null;

			if (!string.IsNullOrWhiteSpace(fieldName))
			{
				foundVal = GetMatchedFieldValue(filterContext.HttpContext.Request.QueryString, fieldName, ignoreCase);

				if (foundVal == null)
				{
					foundVal = GetMatchedFieldValue(filterContext.HttpContext.Request.Form, fieldName, ignoreCase);
				}
			}

			return foundVal;
		}

		private string GetMatchedFieldValue(NameValueCollection keyValueCollection, string match, bool ignoreCase)
		{
			if (ignoreCase)
			{
				match = match.ToUpper();
			}

			string ret = null;
			int lenMatch = match.Length;

			foreach (string key in keyValueCollection.Keys)
			{
				string name = ignoreCase ? key.ToUpper() : key;
				int ix = name.IndexOf(match);

				if (ix >= 0)
				{
					bool leftOk = ix == 0;

					if (!leftOk)
					{
						leftOk = name[ix - 1] == '.';
					}

					int rightIx = ix + lenMatch;
					bool rightOk = name.Length == rightIx;

					if (!rightOk)
					{
						rightOk = name[rightIx] == '.' || name[rightIx] == '[';
					}


					if (leftOk && rightOk)
					{
						ret = keyValueCollection[name];
						break;
					}
				}
			}

			return ret;
		}


		protected void WriteKeyValueFromCollection(NameValueCollection col, XmlWriter wr, string newElementName = null)
		{
			if (col.Count > 0)
			{
				var writeNewElement = !string.IsNullOrWhiteSpace(newElementName);
				var enmr = col.GetEnumerator();
				enmr.Reset();

				if (writeNewElement)
				{
					wr.WriteStartElement(newElementName);
				}

				while (enmr.MoveNext())
				{
					try
					{
						var key = (string)enmr.Current;
						var val = col[key];

						wr.WriteStartElement("Item");
						WriteXmlAttributeSafe(wr, "key", key);
						WriteXmlAttributeSafe(wr, "value", val);
						wr.WriteEndElement();
					}
					catch (Exception ex)
					{
						try
						{
							var exMsg = ex.Message;
							if (ex.Message.StartsWith("A potentially dangerous "))
							{
								int ix = exMsg.IndexOf('(');

								if (ix > -1)
								{
									exMsg = exMsg.Remove(0, ix + 1).TrimEnd('.', ')');

									var ixEqual = exMsg.IndexOf('=');

									if (ixEqual > -1)
									{
										var key = exMsg.Substring(0, ixEqual);
										var val = exMsg.Remove(0, ixEqual + 1).Trim('\"');

										wr.WriteStartElement("Item");
										WriteXmlAttributeSafe(wr, "key", key);
										WriteXmlAttributeSafe(wr, "value", val);
										wr.WriteEndElement();
									}
								}
							}
							else
							{
								wr.WriteStartElement("Error");
								wr.WriteCData(ex.ToString());
								wr.WriteEndElement();
							}
						}
						catch (Exception ex2)
						{
							wr.WriteStartElement("Error");
							wr.WriteStartElement("Primary");
							wr.WriteCData(ex.ToString());
							wr.WriteEndElement();
							wr.WriteStartElement("Secondary");
							wr.WriteCData(ex2.ToString());
							wr.WriteEndElement();
							wr.WriteEndElement();
						}
					}
				}

				if (writeNewElement)
				{
					wr.WriteEndElement();
				}
			}
		}

		protected void WriteKeyValueFromCollection(IDictionary<string, object> col, XmlWriter wr, string newElementName = null)
		{
			if (col.Count > 0)
			{
				var writeNewElement = !string.IsNullOrWhiteSpace(newElementName);

				if (writeNewElement)
				{
					wr.WriteStartElement(newElementName);
				}

				foreach (var objKey in col.Keys)
				{
					var key = (string)objKey;
					var val = col[key] == null ? null : col[key].ToString();

					wr.WriteStartElement("Item");
					WriteXmlAttributeSafe(wr, "key", key);
					WriteXmlAttributeSafe(wr, "value", val);
					wr.WriteEndElement();
				}

				if (writeNewElement)
				{
					wr.WriteEndElement();
				}
			}
		}

		protected void WriteXmlAttributeSafe(XmlWriter wr, string keyName, string val)
		{
			var goodValue = SecurityElement.Escape(val);
			wr.WriteAttributeString(keyName, goodValue);
		}
	}
}
