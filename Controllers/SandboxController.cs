using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace tongbro.Controllers
{
    public class SandboxController : ApiController
    {
		[HttpPost]
		[Route("api/101")]
		public string SaveTempStash([FromBody] string data)
		{
			System.Web.HttpContext.Current.Application["TempStash"] = data;
			System.Web.HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
			return "data saved: " + (string)System.Web.HttpContext.Current.Application["TempStash"];
		}

		[Route("api/102")]
		public string GetTempStash()
		{
			return (string)System.Web.HttpContext.Current.Application["TempStash"];
		}
	}
}
