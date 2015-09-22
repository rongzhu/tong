using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Configuration;
using System.Web.Security;
using tongbro.Models;

namespace tongbro.Controllers
{
    public class HomeController : Controller
    {
        private string _root;

        private string ToWeb(string path)
        {
            if (!_root.HasContent()) _root = Server.MapPath("~/content/images").ToLower();
            return path.ToLower().Replace(_root, "/content/images").Replace('\\', '/');
        }

        public ActionResult Default()
        {
            return View(Directory.GetFiles(Server.MapPath("~/content/images/960").ToLower()).Select(f => ToWeb(f)).ToArray());
        }

        public ActionResult Gallery()
        {
            var cats = new List<ProductCategory>();
            string[] dirs = Directory.GetDirectories(Server.MapPath("~/content/images/800").ToLower());
            foreach (string d in dirs)
                cats.Add(FillCategory(d));

            return View(cats);
        }

        private ProductCategory FillCategory(string dir)
        {
            var cat = new ProductCategory();
            cat.Name = Path.GetFileName(dir);
            int n = cat.Name.IndexOf('.');
            if (n >= 0) cat.Name = cat.Name.Substring(n + 1).Trim();
            cat.SeoName = cat.Name.ToLower().Replace(" ", "-");

            string[] subdirs = Directory.GetDirectories(dir);
            if (subdirs.Length > 0)
            {
                foreach (string d in subdirs)
                    cat.Subcategories.Add(FillCategory(d));
            }
            else
            {
                string[] imgs = Directory.GetFiles(dir);

                foreach(string img in imgs)
                {
                    ProductImage pi = new ProductImage();
                    pi.Path = ToWeb(img);
                    pi.Thumbnail = pi.Path.Replace("/images/800", "/images/200");
                    pi.Sku = Path.GetFileNameWithoutExtension(pi.Path);
                    cat.Images.Add(pi);
                }
            }

            return cat;
        }

        public ActionResult ContactUs()
        {
            return View();
        }

        public ActionResult ThankYou()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string password, string returnUrl = null)
        {
			if (password == ConfigurationManager.AppSettings["password"])
			{
                FormsAuthentication.SetAuthCookie("rongzhu", false);

                return Redirect(returnUrl.HasContent() && returnUrl.StartsWith("/") ? returnUrl : "/ng/index.html");
			}
			else
			{
				ViewBag.Message = "Bummer!";
				return View();
			}
        }

    }
}
