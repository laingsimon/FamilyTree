using System.Collections.Generic;
using System.Linq;
using FamilyTree.Models.DTO;

namespace FamilyTree.Models
{
	public class TreeParser
	{
		public IEnumerable<Marriage> GetAllMarriages(Tree tree)
		{
			return from person in _GetAllPeople(tree)
				   where person.Marriages != null
				   from marriage in person.Marriages
				   select marriage;
		}

		private IEnumerable<Person> _GetAllPeople(Tree tree)
		{
			if (tree == null || tree.Person == null)
				yield break;


			yield return tree.Person;

			foreach (var person in _FlattenTree(tree.Person))
				yield return person;
		}

		private IEnumerable<Person> _FlattenTree(Person person)
		{
			foreach (var directChild in _EnumerateChildren(person.Children))
				yield return directChild;

			if (person.Marriages != null)
			{
				foreach (var marriage in person.Marriages)
				{
					yield return marriage.To.Person;

					foreach (var child in _EnumerateChildren(marriage.Children))
						yield return child;
				}
			}
		}

		private IEnumerable<Person> _EnumerateChildren(MarriageChildren children)
		{
			if (children == null || children.People == null)
				yield break;

			foreach (var child in children.People)
			{
				yield return child;

				foreach (var grandChild in _FlattenTree(child))
					yield return grandChild;
			}
		}
	}
}