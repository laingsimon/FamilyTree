using System;

namespace FamilyTree.ViewModels
{
	public class EventViewModel
	{
        private readonly string dateFormat;

        public string RawDate { get; set; }
		public DateTime? Date { get; set; }
		public string Location { get; set; }

        public EventViewModel(string dateFormat)
        {
            this.dateFormat = dateFormat;
        }

		public string DateFormatted
		{
			get
			{
				if (Date == null)
					return RawDate;

				return Date.Value.ToString(dateFormat);
			}
		}
	}
}