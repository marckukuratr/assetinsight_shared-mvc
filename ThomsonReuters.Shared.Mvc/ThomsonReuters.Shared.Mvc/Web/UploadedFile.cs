using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThomsonReuters.Shared.Web
{
	public class UploadedFile
	{
		private byte[] _data = null;

		public string FileName { get; set; }
		public string ContentType { get; set; }
		public int Length { get; set; }
		public MemoryStream DataStream { get; set; }

		public byte[] Data
		{
			get
			{
				if (_data == null && DataStream != null)
				{
					_data = DataStream.ToArray();
				}
				return _data;
			}
			set
			{
				_data = value;
			}
		}
	}
}
