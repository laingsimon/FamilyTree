using System;
using System.Web.Mvc;
using System.Xml.Linq;
using System.IO;
using System.Xml.XPath;
using FamilyTree.Models;
using System.Net;

public class PersonController : Controller
{
    [HttpPost]
	public ActionResult Update(string family, string path, PersonModel data)
	{
		var fileName = Server.MapPath(string.Format("~/Data/{0}.xml", family));
		
		Response.AddHeader("FileName", fileName);
		if (!System.IO.File.Exists(fileName))
			return HttpNotFound();
		
		try
		{
			var xml = XDocument.Load(fileName);
			var personNode = xml.XPathSelectElement(path);

            if (personNode == null)
            {
                personNode = new XElement("Person");
                var parentPath = _GetParentPath(path);
                var parentNode = xml.XPathSelectElement(parentPath);

                if (parentNode == null)
                    return new HttpStatusCodeResult((int)HttpStatusCode.NotFound);

                parentNode.Add(personNode);
            }

			var result = data.Update(personNode);

            if (result != HttpStatusCode.NotModified)
            {
                xml.Save(fileName);
                System.IO.File.Copy(
                    fileName,
                    Path.ChangeExtension(
                        fileName,
                        string.Format("{0:yyyy-MM-dd@HH-mm-ss}", DateTime.UtcNow)));
            }
			
			return new HttpStatusCodeResult(result);
		}
		catch (Exception exc)
		{
			Response.StatusCode = 500;
			Response.AddHeader("Exception", exc.GetType().Name);
			return Content(exc.ToString(), "text/plain");
		}
	}

    private string _GetParentPath(string path)
    {
        return path + "/../../Children/"; //TODO: What if the path points to the top-most person?
    }
}