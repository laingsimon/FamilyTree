using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FamilyTree.ViewModels
{
	public class OtherTreeViewModel
	{
		private readonly string _fromHandle;
		private readonly string _toHandle;
		private readonly string _path;

		public OtherTreeViewModel(string fromHandle, string toHandle, string path)
		{
			_fromHandle = fromHandle;
			_toHandle = toHandle;
			_path = path;
		}

		public string Path
		{
			get { return _path; }
		}

		public string EntryPoint
		{
			get { return string.Format("{0}+{1}", _fromHandle, _toHandle); }
		}

		public string EntryPointReversed
		{
			get { return string.Format("{0}+{1}", _toHandle, _fromHandle); }
		}
	}
}