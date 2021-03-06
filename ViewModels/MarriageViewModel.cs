﻿using FamilyTree.Models.DTO;
using Newtonsoft.Json;

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
		// ReSharper disable MemberCanBePrivate.Global
		public MarriageStatus Status { get; set; }
		// ReSharper restore MemberCanBePrivate.Global
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