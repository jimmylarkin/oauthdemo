using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace LiveIdOfflineClient
{
    class Program
    {
        static void Main(string[] args)
        {
            string token = "EwA4Aq1DBAAUGCCXc8wU/zFu9QnLdZXy+YnElFkAAWfVq6mk+yT6kwg/Jnw7ddLl12w7AtMUe+ltMnnYr7eSlM1x6VzJ7QYb8WbZ6XcDWutpOqvV1fDotVIXolRIaSN7EB7+5uiajzcAoSqd/fkdQL0PtP/KPEBkC0R0i4tK/2jZa2CHs0viqokMxSqRoa5iFzH/EDvBfwcvz8nG1wSIvOex8SaYprENCLkDCCjYITu36rfihJbtqjwhF/egX8yID/NsKaepB5BfvC7b3rE5+6lv9zhJE/BQdNMLrbydIQ9eK5sOkW/L8l+AGGXfBqCikbjDICH7AsUWil7YtW65AIewiV2LFhhKUv5PkPX1rg/DBTxr6f/4iVWfTnBqxBEDZgAACB5eIPhSsFyfCAHvKyFOIJtFbiHo8BYkSnonvukrTzjqs0pHpEXAncbcP8UJ1s2O8o8Ac3VtKFCruBiANMg+FmKWSG7DdTttcYHm5pLr0WLGDRWjMIV4i17EIM2QULwXgwFbNmH2+Q2CbokucyO+bD/DNN26BSC8JX9DE+kvuOUtnya/jul+JI6l9+Sh+6S74zlvJgPuRLBkt32NmMbXD3auPkB/Gw6BHeNdXV9rbbbFT5xd8oJ952l0lI7W7+m/9bswPrIQ+dHbS+B+u3kFG98JxLzHBs1dpzNlnxAE2YdLWk5rCB4s+Ucnim2VSOfg1SqjprrRdHz7ApvjNLIBv6cn1FKaap2mI2AGitzWXiS+dpsAAA==";
            string refreshToken = "CodiHo6nN*TU6DZxLP**GYtY2rFCqgxLbmDi3To*WWUwFADJqI5Redy*jarIdxuq5qIx60AorWdHE76SMu5eYdCU7c49IR6ro2yE8pUzEseu0JvI!QxEt05Qxph9xBbCn1uOXnuCpqQ!!*U8DkVrUWrVAAH464YsmuQF*DJGibDe!16O6gHTciqxpPbw42K!KebEvXWwgMO0ZX2bTqkKMSafnh7yU4V4g0crHGVxzACD1t5PjpEMI3R2eeB!e62NZneqxskZcVaNbNslmg1UIwdaBodUeUyopFSo!mh!aPAG5ICNCiataZzRGxn7Q2RDcTowKc8aGBQTH8PVAu2oFzCFOhmbAdsciAN!b2AJv6Xbvl5LWs7AgF3xHrGgiknfhw$$";
            
            try
            {
                GetUserData(token);
            }
            catch (WebException ex)
            {
                if (((System.Net.HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.Unauthorized)
                {
                    var response = RefreshToken(refreshToken);
                    if (response != null)
                    {
                        token = response.Token;
                        refreshToken = response.RefreshToken;
                        GetUserData(token);
                    }
                }
            }
            Console.ReadLine();
        }

        private static void GetUserData(string token)
        {
            WebClient client = new WebClient();
            byte[] responseBytes = client.DownloadData("https://apis.live.net/v5.0/me?access_token=" + token);
            string responseString = Encoding.ASCII.GetString(responseBytes);
            Console.Write(responseString.Replace("\r", "\n"));
        }

        private static OAuthTokenResponse RefreshToken(string refreshToken)
        {
            WebClient client = new WebClient();
            NameValueCollection form = new NameValueCollection();
            form.Add("client_id", "000000004810217E");
            form.Add("redirect_uri", "http://demo.my/Home/AuthorizationCodeResponse");
            form.Add("client_secret", "ynlv5klzgQFjHy4sK3XXRdyYw-oA-Fev");
            form.Add("refresh_token", refreshToken);
            form.Add("grant_type", "refresh_token");

            client.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");

            try
            {
                byte[] responseBytes = client.UploadValues("https://login.live.com/oauth20_token.srf", "POST", form);
                string responseString = Encoding.ASCII.GetString(responseBytes);
                var response = JsonConvert.DeserializeObject<OAuthTokenResponse>(responseString);
                return response;
            }
            catch (WebException ex)
            {
                byte[] buffer = new byte[(int)ex.Response.ContentLength];
                ex.Response.GetResponseStream().Read(buffer, 0, (int)ex.Response.ContentLength);
                Console.WriteLine("Error:");
                Console.WriteLine(Encoding.ASCII.GetString(buffer));
            }
            return null;
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
}
