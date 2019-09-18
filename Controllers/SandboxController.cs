using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using System.Net.Mail;

namespace tongbro.Controllers
{
	public class LogPostModel
	{
		public string keyword { get; set; }
		public string queryid { get; set; }
	}

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

		[HttpPost]
		[Route("api/103")]
		public string LogPost(LogPostModel model)
		{
			var req = HttpContext.Current.Request;
			MailMessage message = new MailMessage("admin@tongbrothers.com", "postmaster@tongbrothers.com", "Log Post",
				string.Format("View attempted: {0} from {1} (query {2}) referer {3} at UTC {4}", model.keyword, req.UserHostAddress,
				model.queryid, req.UrlReferrer, DateTime.UtcNow.ToString()));

			SmtpClient client = new SmtpClient("mail.tongbrothers.com");
			int n = 2222;
			NetworkCredential Credentials = new NetworkCredential("postmaster@tongbrothers.com", "winhost" + (n + 1122));
			client.Credentials = Credentials;
			client.Send(message);

			HttpContext.Current.Response.Redirect("/content/messagepage.html");
			return "";
		}

	}
}
