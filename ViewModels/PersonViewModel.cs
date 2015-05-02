using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace FamilyTree.ViewModels
{
	public class PersonViewModel
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string MiddleName { get; set; }
		public string Title { get; set; }
		public string Nickname { get; set; }

		public EventViewModel Birth { get; set; }
		public EventViewModel Death { get; set; }

		/// <summary>
		/// Children born out of wed-lock
		/// </summary>
		public PersonViewModel[] Children { get; set; }

		/// <summary>
		/// Marriages of this person to another
		/// </summary>
		public MarriageViewModel[] Marriages { get; set; }

		public string GetHandle()
		{
			var fullName = _FullName(false).Replace(" ", "-");
			var birthDate = _BirthDate();

			if (string.IsNullOrEmpty(birthDate))
				return fullName;

			return fullName + "_" + birthDate;
		}

		private string _BirthDate()
		{
			if (Birth == null || string.IsNullOrEmpty(Birth.DateFormatted))
				return null;

			return Birth.DateFormatted.Replace("/", "");
		}

		private string _FullName(bool includeNick = true)
		{
			var builder = new StringBuilder();
			if (!string.IsNullOrEmpty(Title))
				builder.Append(Title + " ");

			builder.Append(FirstName);

			if (includeNick && !string.IsNullOrEmpty(Nickname))
				builder.AppendFormat(" '{0}'", Nickname);

			builder.Append(" ");

			if (!string.IsNullOrEmpty(MiddleName))
				builder.Append(MiddleName + " ");

			builder.Append(LastName);

			return builder.ToString();
		}
	}
}