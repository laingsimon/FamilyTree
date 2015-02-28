using FamilyTree.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FamilyTree.Controllers
{
    public class CacheController : Controller
    {
        public ActionResult Index()
        {
            return Content(
				string.Format(@"Assemly: {0:yyyy-MM-dd HH:mm:ss}
ft.xsl {1:yyyy-MM-dd HH:mm:ss}
Laing.xml: {2:yyyy-MM-dd HH:mm:ss}",
				ETagHelper.GetAssemblyDate(),
				System.IO.File.GetLastWriteTimeUtc(Server.MapPath("~/Xsl/ft.xsl")),
				System.IO.File.GetLastWriteTimeUtc(Server.MapPath("~/Data/Laing.xml"))
			), "text/plain");
        }
    }
}
