using System.Net;
using System.Web.Mvc;

namespace FamilyTree.Models
{
    public class StatusContentResult : ContentResult
    {
        private readonly HttpStatusCode statusCode;

        public StatusContentResult(string content, string contentType, HttpStatusCode statusCode)
        {
            Content = content;
            ContentType = contentType;
            this.statusCode = statusCode;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var response = context.RequestContext.HttpContext.Response;
            response.StatusCode = (int)statusCode;

            base.ExecuteResult(context);
        }
    }
}