using FamilyTree.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace FamilyTree.ViewModels
{
	public class PersonViewModel
	{
		private readonly Person _person;
		private readonly Person _directParent;
		private readonly MarriageViewModel _marriage;
		private readonly int _siblings;
		private readonly int _position;

		public PersonViewModel(Person person, int position, int siblings = 0, Person directParent = null, MarriageViewModel marriage = null)
		{
			_position = position;
			_siblings = siblings;
			_person = person;
			_marriage = marriage;
			_directParent = directParent;
		}

		public int Position
		{
			get { return _position; }
		}

		public int Siblings
		{
			get { return _siblings; }
		}

		public Event Birth
		{
			get { return _person.Birth; }
		}

		public Event Death
		{
			get { return _person.Death; }
		}

		public Gender Gender
		{
			get { return _person.Gender; }
		}

		public Name Name
		{
			get { return _person.Name; }
		}

		public IReadOnlyCollection<PersonViewModel> DirectChildren
		{
			get { return _person.Children.People.Select((c, i) => new PersonViewModel(c, position: i, siblings: _person.Children.People.Count - 1, directParent: _person)).ToArray(); }
		}

		public IReadOnlyCollection<MarriageViewModel> Marriages
		{
			get { return _person.Marriages.Select((m, i) => new MarriageViewModel(this, marriage: m, position: i)).ToArray(); }
		}

		public string Handle
		{
			get
			{
				var fullName = _GetFullName(includeNickname: false).Replace(" ", "-");

				if (Birth == null || string.IsNullOrEmpty(Birth.Date))
					return fullName;

				return fullName + "_" + Birth.Date.Replace("/", "");
			}
		}

		private string _GetFullName(bool includeNickname = true)
		{
			var stringBuilder = new StringBuilder();
			if (!string.IsNullOrEmpty(Name.Title))
				stringBuilder.Append(Name.Title + " ");

			stringBuilder.Append(Name.First);

			if (includeNickname && !string.IsNullOrEmpty(Name.Nickname))
				stringBuilder.AppendFormat(" '{0}'", Name.Nickname);

			stringBuilder.Append(" ");

			if (!string.IsNullOrEmpty(Name.Middle))
				stringBuilder.Append(Name.Middle + " ");

			stringBuilder.Append(Name.Last);

			return stringBuilder.ToString();
		}
	}
}