using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace System.Web.Mvc
{
    public static class UrlHelpers
    {
        public static string GetNavClass(this UrlHelper url, string action, string controller)
        {
            if (url.RequestContext.RouteData.Values["controller"].ToString().Equals(controller, StringComparison.OrdinalIgnoreCase)
                               && url.RequestContext.RouteData.Values["action"].ToString().Equals(action, StringComparison.OrdinalIgnoreCase))
            {
                return "class=\"active\"";
            }
            return string.Empty;
        }
    }
}