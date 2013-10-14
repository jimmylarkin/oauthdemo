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
      OAuthWebSecurity.RegisterMicrosoftClient(
          clientId: "000000004810217E",
          clientSecret: "ynlv5klzgQFjHy4sK3XXRdyYw-oA-Fev");
    }
  }
}