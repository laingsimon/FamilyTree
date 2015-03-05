using FamilyTree.Models.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace FamilyTree.ViewModels
{
	public class MarriageViewModel
	{
		private readonly PersonViewModel _from;
		private readonly Marriage _marriage;
		private readonly int _position;

		public MarriageViewModel(PersonViewModel from, Marriage marriage, int position)
		{
			_position = position;
			_from = from;
			_marriage = marriage;
		}

		public IReadOnlyCollection<PersonViewModel> Children
		{
			get { return _marriage.Children.People.Select((c, i) => new PersonViewModel(c, position: i, siblings: _marriage.Children.People.Count - 1, marriage: this)).ToArray(); }
		}

		public PersonViewModel To
		{
			get { return new PersonViewModel(_marriage.To.Person, 0); }
		}

		public PersonViewModel From
		{
			get { return _from; }
		}

		public string Date
		{
			get { return _marriage.Date; }
		}

		public int Position
		{
			get { return _position;  }
		}

		public string Location
		{
			get { return _marriage.Location; }
		}

		public string Status
		{
			get 
			{ 
				return _marriage.Status == MarriageStatus.Married
					? ""
					: _marriage.Status.ToString();
			}
		}

		public string SeeOtherTree
		{
			get { return _marriage.Children.SeeOtherTree; }
		}
	}
}