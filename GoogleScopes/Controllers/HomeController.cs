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

namespace GoogleScopes.Controllers
{
  public class HomeController : Controller
  {
    public ActionResult Index()
    {
      return View();
    }

    [HttpPost]
    public ActionResult UserinfoProfileScope()
    {
      string url = string.Format("https://accounts.google.com/o/oauth2/auth?client_id={0}&scope={1}&response_type=code&redirect_uri={2}",
          "71806529788.apps.googleusercontent.com",
          "https://www.googleapis.com/auth/userinfo.profile",
          HttpUtility.UrlEncode("http://demo.my/Home/AuthorizationCodeResponse"));
      return Redirect(url);
    }

    public ActionResult AuthorizationCodeResponse(string code)
    {
      WebClient client = new WebClient();
      NameValueCollection form = new NameValueCollection();
      form.Add("client_id", "71806529788.apps.googleusercontent.com");
      form.Add("redirect_uri", "http://demo.my/Home/AuthorizationCodeResponse");
      form.Add("client_secret", "MKCCduYe50nZoOD0DiQJRosE");
      form.Add("code", code);
      form.Add("grant_type", "authorization_code");

      byte[] responseBytes = client.UploadValues("https://accounts.google.com/o/oauth2/token", "POST", form);
      string responseString = Encoding.ASCII.GetString(responseBytes);

      var response = JsonConvert.DeserializeObject<OAuthTokenResponse>(responseString);
      response.RawResponse = responseString;

      ResponseViewModel model = new ResponseViewModel();
      model.Code = code;
      model.TokenResponse = response;

      Session["token"] = response.Token;

      return View(model);
    }

    public ActionResult UserData()
    {
      string token = (string)Session["token"];
      WebClient client = new WebClient();
      byte[] responseBytes = client.DownloadData("https://www.googleapis.com/oauth2/v2/userinfo?access_token=" + token);
      ViewBag.Response = Encoding.ASCII.GetString(responseBytes);
      return View();
    }
  }

  public class OAuthTokenResponse
  {
    [JsonIgnore]
    public string RawResponse { get; set; }

    [JsonProperty("token_type")]
    public string TokenType { get; set; }

    [JsonProperty("scope")]
    public string Scope { get; set; }

    [JsonProperty("access_token")]
    public string Token { get; set; }

    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; }

    [JsonProperty("expires_in")]
    public int Expiry { get; set; }
  }

  public class ResponseViewModel
  {
    public string Code { get; set; }
    public OAuthTokenResponse TokenResponse { get; set; }
  }
}
