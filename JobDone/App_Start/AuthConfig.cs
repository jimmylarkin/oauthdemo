using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.WebPages.OAuth;
using JobDone.Models;

namespace JobDone
{
  public static class AuthConfig
  {
    public static void RegisterAuth()
    {
      OAuthWebSecurity.RegisterMicrosoftClient(
          clientId: "00000000400F8C05",
          clientSecret: "bll3xF4lvpaWQdgrtgL7BP6VpKkzXuD1");
    }
  }
}
