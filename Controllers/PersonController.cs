public class PersonController : Controller
{
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
				return HttpNotFound();
				
			data.Update(personNode);

			xml.Save(fileName);
			File.Copy(fileName, Path.ChangeExtension(fileName, string.Format("{0:yyyy-MM-dd@HH-mm-ss}", DateTime.UtcNow));
			
			return new HttpStatusCodeResult(204);
		}
		catch (Exception exc)
		{
			Response.StatusCode = 500;
			Response.AddHeader("Exception", exc.GetType().Name);
			return Content(exc.Message);
		}
	}
	
	public class PersonModel
	{
		public string FirstName { get; set; }
		public string MiddleName { get; set; }
		public DateTime DateOfBirth { get; set; }
		public string Nickname { get; set; }
		
		public static PersonModel(XElement person)
		{
			var name = person.Element("Name");
			FirstName = name.Attribute("First");
			MiddleName = name.Attribute("Middle");
			Nickname = name.Attribute("Nickname");
			
			var dob = person.Element("Birth");
			
			DateTime.TryParseExact(dob.Attribute("Date"), "dd/MM/yyyy", null, DateOfBirth);
		}
	}
}