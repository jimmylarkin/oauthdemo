
using DotNetOpenAuth.AspNet.Clients;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace CustomGoogleClient
{
  public class MyGoogleClient : OAuth2Client
  {
    private readonly string appId;
    private readonly string appSecret;
    private readonly string scope;

    public MyGoogleClient(string appId, string appSecret, string scope)
        : base("google")
    {
        this.scope = scope;
        this.appId = appId;
        this.appSecret = appSecret;
    }

    protected override Uri GetServiceLoginUrl(Uri returnUrl)
    {
        string uri = string.Format("https://accounts.google.com/o/oauth2/auth?client_id={0}&scope={1}&response_type=code&redirect_uri={2}",
        this.appId, this.scope, HttpUtility.UrlEncode(returnUrl.AbsoluteUri));
      return new Uri(uri);
    }

    protected override string QueryAccessToken(Uri returnUrl, string authorizationCode)
    {
        UriBuilder builder = new UriBuilder("https://accounts.google.com/o/oauth2/token");
      builder.Query = string.Format("client_id={0}&client_secret={1}&code={2}&redirect_uri={3}&grant_type=authorization_code",
        appId,
        appSecret,
        authorizationCode,
        HttpUtility.UrlEncode(returnUrl.AbsoluteUri)
        );
      WebRequest request = WebRequest.Create(builder.Uri.AbsoluteUri);
      request.Method = "GET";
      HttpWebResponse response = (HttpWebResponse)request.GetResponse();
      if (response.StatusCode == HttpStatusCode.OK)
      {
        using (Stream responseStream = response.GetResponseStream())
        {
          DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(OAuth2AccessTokenData));
          OAuth2AccessTokenData data = serializer.ReadObject(responseStream) as OAuth2AccessTokenData;
          if (data != null)
          {
            return data.AccessToken;
          }
        }
      }
      return null;
    }

    protected override IDictionary<string, string> GetUserData(string accessToken)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      using (WebResponse response = WebRequest.Create("https://www.googleapis.com/oauth2/v2/userinfo?access_token=" + Uri.EscapeDataString(accessToken)).GetResponse())
      {
        Stream stream = response.GetResponseStream();
        StreamReader reader = new StreamReader(stream, Encoding.UTF8);
        Char[] buffer = new Char[1024];
        int count = reader.Read(buffer, 0, 1024);
        StringBuilder json = new StringBuilder();
        while (count > 0)
        {
          json.Append(new String(buffer, 0, count));
          count = reader.Read(buffer, 0, 256);
        }
        var jss = new JavaScriptSerializer();
        dynamic userData = jss.Deserialize<dynamic>(json.ToString());
        Dictionary<string, object> userDataDictionary = userData as Dictionary<string, object>;
        GetFlatUserDataProperties(dictionary, userDataDictionary);
      }
      return dictionary;
    }

    private static void GetFlatUserDataProperties(Dictionary<string, string> flatDictionary, Dictionary<string, object> userDataDictionary, string parentPropertyName = "")
    {
      foreach (KeyValuePair<string, object> kvp in userDataDictionary)
      {
        Dictionary<string, object> dict = kvp.Value as Dictionary<string, object>;
        if (dict == null)
        {
          string value = kvp.Value == null ? string.Empty : kvp.Value.ToString();
          flatDictionary.Add(parentPropertyName + kvp.Key, value);
        }
        else
        {
          GetFlatUserDataProperties(flatDictionary, dict, kvp.Key + ".");
        }
      }
    }
  }
}