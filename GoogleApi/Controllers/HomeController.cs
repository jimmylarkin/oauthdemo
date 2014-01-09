using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Threading;
using LiveIdScopes.Models;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Plus.v1;
using Google.Apis.Services;

namespace GoogleApi.Controllers
{
    public class HomeController : AsyncController 
    {
        public async Task<ActionResult> IndexAsync(CancellationToken cancellationToken)
        {
            return View();
        }

        public async Task<ActionResult> UserDataAsync(CancellationToken cancellationToken)
        {
            var result = await new AuthorizationCodeMvcApp(this, new AppFlowMetadata()).AuthorizeAsync(cancellationToken);

            if (result.Credential != null)
            {
                var service = new Google.Apis.Oauth2.v1.Oauth2Service(new BaseClientService.Initializer
                {
                    HttpClientInitializer = result.Credential,
                    ApplicationName = "oauthdemo"
                });

                var profileRequest = service.Userinfo.V2.Me.Get();
                var profile = profileRequest.Execute();
                return View(profile);
            }
            else
            {
                return new RedirectResult(result.RedirectUri);
            }
        }
    }
}