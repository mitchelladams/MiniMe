using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace MiniMe.Models
{
    /// <summary>
    /// This will return a JSONP formatted string. Used for cross domain requests.
    /// </summary> 
    public class JsonpResult : JsonResult
    {
        object data = null;

        public JsonpResult() { }

        public JsonpResult(object data)
        {
            this.data = data;
        }

        public override void ExecuteResult(ControllerContext controllerContext)
        {
            if (controllerContext != null)
            {
                HttpResponseBase Response = controllerContext.HttpContext.Response;
                HttpRequestBase Request = controllerContext.HttpContext.Request;

                string callbackfunction = Request["callback"];

                if (string.IsNullOrEmpty(callbackfunction))
                {
                    throw new Exception("Callback function name must be provided in request.");
                }
                Response.ContentType = "application/x-javascript";

                if (data != null)
                {
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    Response.Write(string.Format("{0}({1});", callbackfunction, serializer.Serialize(data)));
                }
            }
        }
    }
}