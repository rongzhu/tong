using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace tongbro
{
    public class NonLocalRequireHttpsAttribute : RequireHttpsAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext != null && filterContext.HttpContext.Request.IsLocal)
            {
                return;     //local is fine
            }
            
            base.OnAuthorization(filterContext);        //now requires https
        }
    }
}