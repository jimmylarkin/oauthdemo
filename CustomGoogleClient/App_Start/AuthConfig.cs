using Microsoft.Web.WebPages.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustomGoogleClient
{
  public class AuthConfig
  {
    public static void RegisterAuth()
    {
      OAuthWebSecurity.RegisterClient(
          new MyGoogleClient("71806529788.apps.googleusercontent.com",
              "MKCCduYe50nZoOD0DiQJRosE", 
              "https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/userinfo.email"));
    }
  }
}