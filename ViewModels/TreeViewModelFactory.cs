using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FamilyTree.Models;
using FamilyTree.Models.DTO;

namespace FamilyTree.ViewModels
{
	public class TreeViewModelFactory
	{
		private readonly TreeFactory _treeFactory;
		private readonly TreeParser _treeParser;

		public TreeViewModelFactory(TreeFactory treeFactory, TreeParser treeParser)
		{
			_treeFactory = treeFactory;
			_treeParser = treeParser;
		}

		public TreeViewModel Create(Tree tree)
		{
			return new TreeViewModel
			{
				Name = tree.Family,
				Root = _BuildPerson(tree, tree.Person)
			};
		}

		private PersonViewModel _BuildPerson(Tree tree, Person person)
		{
			var viewModel = new PersonViewModel
			{
				FirstName = person.Name.First,
				LastName = person.Name.Last,
				MiddleName = person.Name.Middle,
				Nickname = person.Name.Nickname,
				Gender = person.Gender,
				Title = person.Name.Title,
				Birth = _BuildEvent(person.Birth),
				Death = _BuildEvent(person.Death),
				Children = _BuildChildren(tree, person.Children).ToArray(),
				HasOtherTree = _treeFactory.OtherTreeExists(person.Name.Last)
			};

			viewModel.Marriages = _BuildMarriages(tree, viewModel, person.Marriages).ToArray();

			return viewModel;
		}

		private IEnumerable<MarriageViewModel> _BuildMarriages(Tree tree, PersonViewModel person, Marriage[] marriages)
		{
			if (marriages == null || !marriages.Any())
				yield break;

			foreach (var marriage in marriages)
			{
				var toPersonViewModel = _BuildPerson(tree, marriage.To.Person);

				var marriageAndChildrenDetail = _BuildChildrenForMarriage(tree, marriage.Children, person, toPersonViewModel);
				var wedding = _OtherTreeMarriage.GetWeddingViewModel(marriageAndChildrenDetail.Marriage ?? marriage);

				yield return new MarriageViewModel(person, toPersonViewModel)
				{
					Wedding = wedding,
					Status = marriage.Status,
					Children = marriageAndChildrenDetail.Children.ToArray(),
				};
			}
		}

		private _OtherTreeMarriage _BuildChildrenForMarriage(
			Tree tree,
			MarriageChildren marriageChildren,
			PersonViewModel from,
			PersonViewModel to)
		{
			if (marriageChildren == null)
				return new _OtherTreeMarriage
				{
					Children = new PersonViewModel[0]
				};

			if (!string.IsNullOrEmpty(marriageChildren.SeeOtherTree))
				return _BuildChildrenFromOtherTree(marriageChildren.SeeOtherTree, from, to);

			var children = _BuildChildrenFromSameTree(tree, marriageChildren);
			return new _OtherTreeMarriage
			{
				Children = children
			};
		}

		private IEnumerable<PersonViewModel> _BuildChildren(
			Tree tree,
			MarriageChildren marriageChildren)
		{
			if (marriageChildren == null || marriageChildren.People == null || !marriageChildren.People.Any())
				return new PersonViewModel[0];

			return _BuildChildrenFromSameTree(tree, marriageChildren);
		}

		private IEnumerable<PersonViewModel> _BuildChildrenFromSameTree(Tree tree, MarriageChildren marriageChildren)
		{
			if (marriageChildren == null || marriageChildren.People == null)
				yield break;

			foreach (var child in marriageChildren.People)
				yield return _BuildPerson(tree, child);
		}

		private class _OtherTreeMarriage
		{
			public Marriage Marriage { get; set; }
			public IEnumerable<PersonViewModel> Children { get; set; }

			public static EventViewModel GetWeddingViewModel(Marriage marriage)
			{
				if (marriage == null)
					return null;

				return new EventViewModel
				{
					Date = _ParseDate(marriage.Date),
					Location = marriage.Location
				};
			}
		}

		private _OtherTreeMarriage _BuildChildrenFromOtherTree(string otherTree, PersonViewModel from, PersonViewModel to)
		{
			var subTree = _treeFactory.Load(otherTree);
			var marriage = _FindMarriage(subTree, from, to)
				?? _FindMarriage(subTree, to, from);

			if (marriage == null)
			{
				//couldn't find entry point
				return new _OtherTreeMarriage
				{
					Children = new PersonViewModel[0]
				};
			}

			return new _OtherTreeMarriage
			{
				Children = _BuildChildrenFromSameTree(subTree, marriage.Children),
				Marriage = marriage
			};
		}

		private Marriage _FindMarriage(Tree tree, PersonViewModel from, PersonViewModel to)
		{
			var fromHandle = from.GetHandle(useRawBirthDate: true);
			var toHandle = to.GetHandle(useRawBirthDate: true);

			var findEntryPoint = string.Format("{0}+{1}", fromHandle, toHandle);

			var allMarriages = _treeParser.GetAllMarriages(tree).Where(m => m.Children != null);
			return allMarriages.SingleOrDefault(m => m.Children.EntryPoint == findEntryPoint);
		}

		private static EventViewModel _BuildEvent(Event @event)
		{
			if (@event == null)
				return null;

			return new EventViewModel
			{
				Location = @event.Location,
				Date = _ParseDate(@event.Date),
				RawDate = @event.Date
			};
		}

		private static DateTime? _ParseDate(string dateString)
		{
			DateTime date;
			if (DateTime.TryParseExact(dateString, "dd/MM/yyyy", null, DateTimeStyles.None, out date))
				return date;

			return null;
		}
	}
}