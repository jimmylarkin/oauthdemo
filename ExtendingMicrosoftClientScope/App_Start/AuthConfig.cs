using Microsoft.Web.WebPages.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExtendingMicrosoftClientScope
{
  public class AuthConfig
  {
    public static void RegisterAuth()
    {
      OAuthWebSecurity.RegisterClient(new MyMicrosoftClient("000000004810217E", "ynlv5klzgQFjHy4sK3XXRdyYw-oA-Fev", "wl.basic wl.emails"));
      //OAuthWebSecurity.RegisterMicrosoftClient(
      //    clientId: "000000004810217E",
      //    clientSecret: "ynlv5klzgQFjHy4sK3XXRdyYw-oA-Fev");
    }
  }
}