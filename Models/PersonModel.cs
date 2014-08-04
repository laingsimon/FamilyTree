using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Linq;

namespace FamilyTree.Models
{
    public class PersonModel
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Nickname { get; set; }

        public PersonModel()
        { }

        public PersonModel(XElement person)
        {
            var name = person.Element("Name");
            FirstName = name.Attribute("First").Value;
            MiddleName = name.Attribute("Middle").Value;
            Nickname = name.Attribute("Nickname").Value;

            var dob = person.Element("Birth");

            DateTime dobValue;
            DateTime.TryParseExact(
                dob.Attribute("Date").Value,
                "dd/MM/yyyy",
                null,
                System.Globalization.DateTimeStyles.AssumeLocal,
                out dobValue);
            DateOfBirth = dobValue;
        }

        public HttpStatusCode Update(XElement personNode)
        {
            var isNew = !personNode.HasElements;

            var name = personNode.Element("Name") ?? _CreateElement(personNode, "Name");
            var dob = personNode.Element("Birth") ?? _CreateElement(personNode, "Birth");

            _SetAttribute(name, "First", FirstName);
            _SetAttribute(name, "Middle", MiddleName);
            _SetAttribute(name, "Nickname", Nickname);

            _SetAttribute(dob, "Date", DateOfBirth.ToString("dd/MM/yyyy"));

            return isNew
                ? HttpStatusCode.Created
                : HttpStatusCode.ResetContent;
        }

        private void _SetAttribute(XElement element, string name, string value)
        {
            element.ReplaceAttributes(new XAttribute(name, value));
        }

        private XElement _CreateElement(XElement personNode, string name)
        {
            var element = new XElement(name);
            personNode.Add(element);
            return element;
        }
    }
}