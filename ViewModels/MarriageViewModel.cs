using FamilyTree.Models.DTO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FamilyTree.ViewModels
{
	public class MarriageViewModel
	{
		public MarriageViewModel(PersonViewModel from, PersonViewModel to)
		{
			From = from;
			To = to;
		}

		[JsonIgnore]
		public PersonViewModel From { get; private set; }
		public PersonViewModel To { get; private set; }
		public MarriageStatus Status { get; set; }
		public EventViewModel Wedding { get; set; }

		public PersonViewModel[] Children { get; set; }

		public string MarriageSymbol
		{
			get
			{
				switch (Status)
				{
					case MarriageStatus.CommonLaw:
						return "-";
					case MarriageStatus.Divorced:
					case MarriageStatus.Widowed:
						return "&#8800;";
					default:
						return "=";
				}
			}
		}
	}
}