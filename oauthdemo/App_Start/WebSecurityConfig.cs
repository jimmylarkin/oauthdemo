using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebMatrix.WebData;

namespace oauthdemo
{
  public class WebSecurityConfig
  {
    public static void RegisterWebSecurityProvider()
    {
      WebSecurity.InitializeDatabaseConnection("DefaultConnection", "users", "id", "name", true);
    }
  }
}