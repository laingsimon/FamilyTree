﻿using System.Diagnostics;
using System.Xml.Serialization;

namespace FamilyTree.Models
{
	[DebuggerDisplay("{Date,nq}")]
	public class Event
	{
		[XmlAttribute]
		public string Date { get; set; }

		[XmlAttribute]
		public string Location { get; set; }
	}
}