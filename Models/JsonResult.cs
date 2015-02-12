using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FamilyTree.Models
{
	public class JsonResult : ContentResult
	{
		private static readonly JsonSerializerSettings _settings
			= new JsonSerializerSettings
			{
				Formatting = Formatting.Indented,
				NullValueHandling = NullValueHandling.Ignore,
				Converters =
				{
					new StringEnumConverter()
				}
			};

		public JsonResult(object data)
		{
			ContentType = "application/json";
			Content = JsonConvert.SerializeObject(data, _settings);
		}
	}
}