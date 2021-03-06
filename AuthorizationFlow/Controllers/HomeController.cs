﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using AuthorizationFlow.Models;

namespace AuthorizationFlow.Controllers
{
  public class HomeController : Controller
  {
    public ActionResult Index()
    {
      string implicitUrl = string.Format("https://login.live.com/oauth20_authorize.srf?client_id={0}&scope={1}&response_type=token&redirect_uri={2}&state=12345testme",
        "000000004810217E",
        "wl.basic",
        HttpUtility.UrlEncode("http://demo.my/Home/ImplicitResponse"));
      ViewBag.ImplicitUrl = implicitUrl;
      return View();
    }

    [HttpPost]
    public ActionResult StartAuthorizationCode()
    {
        string url = string.Format("https://login.live.com/oauth20_authorize.srf?client_id={0}&scope={1}&response_type=code&redirect_uri={2}&state=12345testme",
        "000000004810217E",
        "wl.basic",
        HttpUtility.UrlEncode("http://demo.my/Home/AuthorizationCodeResponse"));
      return Redirect(url);
    }

    public ActionResult AuthorizationCodeResponse(string code)
    {
      WebClient client = new WebClient();
      NameValueCollection form = new NameValueCollection();
      form.Add("client_id", "000000004810217E");
      form.Add("redirect_uri", "http://demo.my/Home/AuthorizationCodeResponse");
      form.Add("client_secret", "ynlv5klzgQFjHy4sK3XXRdyYw-oA-Fev");
      form.Add("code", code);
      form.Add("grant_type", "authorization_code");

      byte[] responseBytes = client.UploadValues("https://login.live.com/oauth20_token.srf", "POST", form);
      string responseString = Encoding.ASCII.GetString(responseBytes);

      var response = JsonConvert.DeserializeObject<OAuthTokenResponse>(responseString);
      response.RawResponse = responseString;

      ResponseViewModel model = new ResponseViewModel();
      model.Code = code;
      model.TokenResponse = response;

      Session["token"] = response.Token;

      return View(model);
    }

    public ActionResult ImplicitResponse()
    {
      return View();
    }

    [HttpPost]
    public ActionResult ImplicitResponse(string token)
    {
      Session["token"] = token;
      return new EmptyResult();
    }

    public ActionResult UserData()
    {
      string token = (string)Session["token"];
      WebClient client = new WebClient();
      byte[] responseBytes = client.DownloadData("https://apis.live.net/v5.0/me?access_token=" + token);
      ViewBag.Response = Encoding.ASCII.GetString(responseBytes);
      return View();
    }
  }
}