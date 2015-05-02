using FamilyTree.Models;
using FamilyTree.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
				Title = person.Name.Title,
				Birth = _BuildEvent(person.Birth),
				Death = _BuildEvent(person.Death),
				Children = _BuildChildren(tree, person.Children).ToArray()
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

				yield return new MarriageViewModel(person, toPersonViewModel)
				{
					Wedding = new EventViewModel
					{
						Date = _ParseDate(marriage.Date),
						Location = marriage.Location
					},
					Status = marriage.Status.ToString(), //TODO: Use the enum here?
					Children = _BuildChildren(tree, marriage.Children, person, toPersonViewModel).ToArray(),
				};
			}
		}

		private IEnumerable<PersonViewModel> _BuildChildren(
			Tree tree,
			MarriageChildren marriageChildren,
			PersonViewModel from,
			PersonViewModel to)
		{
			if (marriageChildren == null)
				return new PersonViewModel[0];

			if (!string.IsNullOrEmpty(marriageChildren.SeeOtherTree))
				return _BuildChildrenFromOtherTree(marriageChildren.SeeOtherTree, from, to);

			return _BuildChildrenFromSameTree(tree, marriageChildren);
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

		private IEnumerable<PersonViewModel> _BuildChildrenFromOtherTree(string otherTree, PersonViewModel from, PersonViewModel to)
		{
			var subTree = _treeFactory.Load(otherTree);
			var marriage = _FindMarriage(subTree, from, to)
				?? _FindMarriage(subTree, to, from);

			if (marriage == null)
			{
				//couldn't find entry point
				return new PersonViewModel[0];
			}

			return _BuildChildrenFromSameTree(subTree, marriage.Children);
		}

		private Marriage _FindMarriage(Tree tree, PersonViewModel from, PersonViewModel to)
		{
			var fromHandle = from.GetHandle();
			var toHandle = to.GetHandle();

			var findEntryPoint = string.Format("{0}+{1}", fromHandle, toHandle);

			var allMarriages = _treeParser.GetAllMarriages(tree).Where(m => m.Children != null);
			return allMarriages.SingleOrDefault(m => m.Children.EntryPoint == findEntryPoint);
		}

		private EventViewModel _BuildEvent(Event @event)
		{
			if (@event == null)
				return null;

			return new EventViewModel
			{
				Location = @event.Location,
				Date = _ParseDate(@event.Date)
			};
		}

		private DateTime? _ParseDate(string dateString)
		{
			DateTime date;
			if (DateTime.TryParseExact(dateString, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out date))
				return date;

			return null;
		}
	}
}