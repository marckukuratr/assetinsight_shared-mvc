using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace ThomsonReuters.Shared.Web
{
	public enum BundleType
	{
		Script,
		Style
	}

	public class BundleMap
	{
		public BundleType Type { get; internal set; }
		public string ReferenceName { get; internal set; }
		public List<string> Paths { get; internal set; }
	}

	public static class BundleMapper
	{
		public const string DEF_CONFIG_FILE = "BundleConfig.xml";

		public const string TABLE_SCRIPT = "ScriptBundles";
		public const string TABLE_STYLE = "StyleBundles";

		public const string REL_SCRIPT_ITEM = "ScriptBundles_Item";
		public const string REL_STYLE_ITEM = "StyleBundles_Item";

		public const string COL_REF = "ref";
		public const string COL_PATH = "path";

		public const string MARKUP_VERSION = "{VERSION}";


		public static bool IsBundleAndMinify
		{
			get
			{
				var minify = SharedWebConfigHelper.IsMinifyResources;
				var ret = minify.HasValue == true &&
				(minify.Value == MinificationType.ScriptStyles || minify.Value == MinificationType.HtmlScriptStyles);
				return ret;
			}
		}

		public static Dictionary<BundleType, List<BundleMap>> LoadBundleConfiguration(string filename = DEF_CONFIG_FILE, string version = null)
		{
			var path = HttpContext.Current.Server.MapPath("~/" + filename);
			DataSet ds = new DataSet();
			ds.ReadXml(path);

			DataTable dtScripts = ds.Tables[TABLE_SCRIPT];
			DataTable dtStyles = ds.Tables[TABLE_STYLE];

			var scripts = Extract(BundleType.Script, dtScripts, version);
			var styles = Extract(BundleType.Style, dtStyles, version);

			Dictionary<BundleType, List<BundleMap>> ret = new Dictionary<BundleType, List<BundleMap>>();
			ret.Add(BundleType.Script, scripts);
			ret.Add(BundleType.Style, styles);

			return ret;
		}

		private static List<BundleMap> Extract(BundleType type, DataTable dt, string verson)
		{
			List<BundleMap> ret = new List<BundleMap>();

			if (dt != null)
			{
				foreach (DataRow row in dt.Rows)
				{
					var map = CreateBundleMap(type, row, verson);
					ret.Add(map);
				}
			}

			return ret;
		}

		private static BundleMap CreateBundleMap(BundleType type, DataRow row, string verson)
		{
			var hasVersion = !string.IsNullOrWhiteSpace(verson);
			var versionLen = MARKUP_VERSION.Length;

			var rel = type == BundleType.Script ? REL_SCRIPT_ITEM : REL_STYLE_ITEM;
			var ret = new BundleMap();

			ret.Type = type;
			ret.ReferenceName = (string)row[COL_REF];
			ret.Paths = new List<string>();

			var itemRows = row.GetChildRows(rel);

			foreach (var item in itemRows)
			{
				var path = (string)item[COL_PATH];

				if (hasVersion)
				{
					path = Regex.Replace(path, MARKUP_VERSION, verson, RegexOptions.IgnoreCase);
				}

				ret.Paths.Add(path);
			}

			return ret;
		}
	}
}
