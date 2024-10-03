using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PraneshKing_Cabs
{
    public class Authorization:ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string controller = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            var actionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            string role = (string)filterContext.HttpContext.Session["RoleName"];
            bool IsAuthorized = false;
            if (role != null && role.ToLower() == "admin")
            {
                    IsAuthorized = true;
            }
            else if (role != null && role.ToLower() == "user")
            {
                IsAuthorized = false;
            }

            if (!IsAuthorized)
            {
                filterContext.Result = new RedirectResult("/Login/LogOut");
            }
        }
    }
}