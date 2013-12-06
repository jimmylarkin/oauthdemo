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

namespace AuthorizationFlow.Models
{
    public class ResponseViewModel
    {
        public string Code { get; set; }
        public OAuthTokenResponse TokenResponse { get; set; }
    }
}
