using System;

namespace FamilyTree.ViewModels
{
	public class EventViewModel
	{
		public string RawDate { get; set; }
		public DateTime? Date { get; set; }
		public string Location { get; set; }

		public string DateFormatted
		{
			get
			{
				if (Date == null)
					return RawDate;

				return Date.Value.ToString("MMM yyyy");
			}
		}
	}
}