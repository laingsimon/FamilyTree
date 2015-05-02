using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FamilyTree.ViewModels
{
	public class EventViewModel
	{
		public DateTime? Date { get; set; }
		public string Location { get; set; }

		public string DateFormatted
		{
			get
			{
				if (Date == null)
					return null;

				return Date.Value.ToString("dd/MM/yyyy");
			}
		}
	}
}