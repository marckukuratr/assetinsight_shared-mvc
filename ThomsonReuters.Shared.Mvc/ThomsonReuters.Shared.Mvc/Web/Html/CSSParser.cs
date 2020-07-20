using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using iTextSharp.text.html.simpleparser;
using ThomsonReuters.Utilities;

namespace ThomsonReuters.Shared.Web.Html
{
	/// <summary>
	/// Object used to parse CSS Files.
	/// This can also be used to minify a CSS file though I
	/// doubt this will pass all the same tests as YUI compressor
	/// or some other tool
	/// </summary>
	[Serializable]
	public partial class CSSParser : List<KeyValuePair<string, List<KeyValuePair<string, string>>>>, ICSSParser
	{
		private const string STYLE_BEGIN = "<style";
		private const string STYLE_END = "</style>";
		private const string SelectorKey = "selector";
		private const string NameKey = "name";
		private const string ValueKey = "value";

		/// <summary>
		/// Regular expression to parse the Stylesheet
		/// </summary>
		[NonSerialized]
		private readonly Regex rStyles = new Regex(RegularExpressionLibrary.CSSGroups, RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private string stylesheet = string.Empty;
		private Dictionary<string, Dictionary<string, string>> classes;
		private Dictionary<string, Dictionary<string, string>> elements;


		/// <summary>
		/// Original Style Sheet loaded
		/// </summary>
		public string StyleSheet
		{
			get
			{
				return this.stylesheet;
			}
			set
			{
				//If the style sheet changes we will clean out any dependant data
				this.stylesheet = value;
				this.Clear();
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CascadingStyleSheet"/> class.
		/// </summary>
		public CSSParser()
		{
			this.StyleSheet = string.Empty;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CascadingStyleSheet"/> class.
		/// </summary>
		/// <param name="CascadingStyleSheet">The cascading style sheet.</param>
		public CSSParser(string CascadingStyleSheet)
		{
			this.Read(CascadingStyleSheet);
		}

		/// <summary>
		/// Reads the CSS file.
		/// </summary>
		/// <param name="Path">The path.</param>
		public void ReadCSSFile(string Path)
		{
			this.StyleSheet = File.ReadAllText(Path);
			this.Read(StyleSheet);
		}

		/// <summary>
		/// Reads the specified cascading style sheet.
		/// </summary>
		/// <param name="CascadingStyleSheet">The cascading style sheet.</param>
		public void Read(string CascadingStyleSheet)
		{
			this.StyleSheet = CascadingStyleSheet;

			if (!string.IsNullOrEmpty(CascadingStyleSheet))
			{
				//Remove comments before parsing the CSS. Don't want any comments in the collection. Don't know how iTextSharp would react to CSS Comments
				MatchCollection MatchList = rStyles.Matches(Regex.Replace(CascadingStyleSheet, RegularExpressionLibrary.CSSComments, string.Empty));
				foreach (Match item in MatchList)
				{
					//Check for nulls
					if (item != null && item.Groups != null && item.Groups[SelectorKey] != null && item.Groups[SelectorKey].Captures != null && item.Groups[SelectorKey].Captures[0] != null && !string.IsNullOrEmpty(item.Groups[SelectorKey].Value))
					{
						string strSelector = item.Groups[SelectorKey].Captures[0].Value.Trim();
						var style = new List<KeyValuePair<string, string>>();

						for (int i = 0; i < item.Groups[NameKey].Captures.Count; i++)
						{
							string className = item.Groups[NameKey].Captures[i].Value;
							string value = item.Groups[ValueKey].Captures[i].Value;
							//Check for null values in the properies
							if (!string.IsNullOrEmpty(className) && !string.IsNullOrEmpty(value))
							{
								className = TrimWhiteSpace(className);
								value = TrimWhiteSpace(value);
								//One more check to be sure we are only pulling valid css values
								if (!string.IsNullOrEmpty(className) && !string.IsNullOrEmpty(value))
								{
									style.Add(new KeyValuePair<string, string>(className, value));
								}
							}
						}
						this.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>(strSelector, style));
					}
				}
			}
		}

		/// <summary>
		/// Gets the CSS classes.
		/// </summary>
		public Dictionary<string, Dictionary<string, string>> Classes
		{
			get
			{
				if (classes == null || classes.Count == 0)
				{
					this.classes = this.Where(cl => cl.Key.StartsWith(".")).ToDictionary(cl => cl.Key.Trim(new Char[] { '.' }), cl => cl.Value.ToDictionary(p => p.Key, p => p.Value));
				}

				return classes;
			}
		}

		/// <summary>
		/// Gets the elements.
		/// </summary>
		public Dictionary<string, Dictionary<string, string>> Elements
		{
			get
			{
				if (elements == null || elements.Count == 0)
				{
					elements = this.Where(el => !el.Key.StartsWith(".")).ToDictionary(el => el.Key, el => el.Value.ToDictionary(p => p.Key, p => p.Value));
				}
				return elements;
			}
		}

		/// <summary>
		/// Gets all styles in an Immutable collection
		/// </summary>
		public IEnumerable<KeyValuePair<string, List<KeyValuePair<string, string>>>> Styles
		{
			get
			{
				return this.ToArray();
			}
		}

		/// <summary>
		/// Removes all elements from the <see cref="CSSParser"></see>.
		/// </summary>
		new public void Clear()
		{
			base.Clear();
			this.classes = null;
			this.elements = null;
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> the CSS that was entered as it is stored internally.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			StringBuilder strb = new StringBuilder(this.StyleSheet.Length);
			foreach (var item in this)
			{
				strb.Append(item.Key).Append("{");
				foreach (var property in item.Value)
				{
					strb.Append(property.Key).Append(":").Append(property.Value).Append(";");
				}
				strb.Append("}");
			}



			return strb.ToString();
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return StyleSheet == null ? 0 : StyleSheet.GetHashCode();
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="T:System.NullReferenceException">
		/// The <paramref name="obj"/> parameter is null.
		///   </exception>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj == null)
			{
				return false;
			}

			CSSParser o = obj as CSSParser;
			return this.StyleSheet.Equals(o.StyleSheet);
		}

		public void ApplyStyles(StyleSheet styles)
		{
			Action<string, string, string> loadTag = new Action<string, string, string>(styles.LoadTagStyle);
			Action<string, string, string> loadClass = new Action<string, string, string>(styles.LoadStyle);

			var stylesTags = this.Elements;
			var stylesClass = this.Classes;

			ApplyStyles(stylesTags, loadTag);
			ApplyStyles(stylesClass, loadClass);
		}


		private static void ApplyStyles(Dictionary<string, Dictionary<string, string>> items, Action<string, string, string> load)
		{
			var length = items.Count;
			var keys = items.Keys.ToArray();
			var vals = items.Values.ToArray();

			for (int i = 0; i < length; i++)
			{
				var key = keys[i];
				var props = vals[i];

				var propLength = props.Count;
				var propKeys = props.Keys.ToArray();
				var propVals = props.Values.ToArray();

				for (int x = 0; x < propLength; x++)
				{
					var propKey = propKeys[x];
					var propVal = propVals[x];

					load(key, propKey, propVal);
				}
			}
		}

		public static void SeparateHtmlAndCss(string input, out string html, out string css)
		{

			var ixStart = 0;
			var ixEnd = input.Length - 1;

			StringBuilder sbHtml = new StringBuilder(input);
			StringBuilder sbCSS = new StringBuilder();

			do
			{
				var tempData = sbHtml.ToString();
				ixStart = tempData.IndexOf(STYLE_BEGIN, ixStart, StringComparison.OrdinalIgnoreCase);

				if (ixStart >= 0)
				{
					ixEnd = tempData.IndexOf(STYLE_END, ixStart + 1, StringComparison.OrdinalIgnoreCase);

					if (ixEnd > 0)
					{
						ixEnd = ixEnd + STYLE_END.Length;
						var cssLength = ixEnd - ixStart + 1;

						var strCss = tempData.Substring(ixStart, cssLength);
						strCss = RemoveStyleTags(strCss);

						sbCSS.AppendLine(strCss);
						sbHtml.Remove(ixStart, cssLength);
					}
					else
					{
						throw new ApplicationException("CSS not well formatted");
					}
				}
				else
				{
					break;
				}
			}
			while (true);

			html = sbHtml.ToString();
			css = sbCSS.ToString();
		}

		private static string RemoveStyleTags(string input)
		{
			var ret = input;
			var ixStart = input.IndexOf(STYLE_BEGIN, StringComparison.OrdinalIgnoreCase);
			var ixEnd = input.LastIndexOf(STYLE_END, StringComparison.OrdinalIgnoreCase);

			if (ixStart >= 0 && ixEnd >= 0)
			{
				ixStart = input.IndexOf(">", ixStart, StringComparison.OrdinalIgnoreCase);

				if (ixStart >= 0)
				{
					ixStart++;
					ret = input.Substring(ixStart, ixEnd - ixStart).Trim();
				}
				else
				{
					throw new ApplicationException("CSS not well formatted");
				}
			}
			else if (ixStart >= 0 || ixEnd >= 0)
			{
				throw new ApplicationException("CSS not well formatted");
			}

			return ret;
		}


		private static string TrimWhiteSpace(string str)
		{
			if (str == null)
			{
				return null;
			}
			Char[] whiteSpace = { '\r', '\n', '\f', '\t', '\v' };
			return str.Trim(whiteSpace).Trim();
		}
	}
}
