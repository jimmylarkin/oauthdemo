using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.WebPages.OAuth;
using oauthdemo.Models;

namespace oauthdemo
{
  public static class AuthConfig
  {
    public static void RegisterAuth()
    {
      OAuthWebSecurity.RegisterMicrosoftClient(
          clientId: "00000000400F8C05",
          clientSecret: "bll3xF4lvpaWQdgrtgL7BP6VpKkzXuD1");

      OAuthWebSecurity.RegisterTwitterClient(
          consumerKey: "Z13QcmG1IPgfIyNH4Q84Zg",
          consumerSecret: "vF1NO5EWbULRRV6Hxnl9NVV86tdPloq8uNDfGdxfA");

      OAuthWebSecurity.RegisterFacebookClient(
          appId: "153997748117499",
          appSecret: "1bc14a00d95031a38cbf688f87de61a2");

      OAuthWebSecurity.RegisterGoogleClient();
    }
  }
}
