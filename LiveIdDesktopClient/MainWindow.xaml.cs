using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Web.Script.Serialization;
using System.Net;
using System.IO;
using System.ComponentModel;
using System.Collections.Specialized;

namespace LiveIdDesktopClient
{
    public partial class MainWindow : Window
    {
        public MainWindowViewModel Model { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Model = new MainWindowViewModel();

            var accessToken = ReadToken("access");
            var refreshToken = ReadToken("refresh");
            if (!string.IsNullOrEmpty(accessToken))
            {
                Model.AccessToken = accessToken;
            }
            if (!string.IsNullOrEmpty(refreshToken))
            {
                Model.RefreshToken = refreshToken;
            }
            DataContext = Model;
            TryGetUserInfo();
        }

        private void btnSignIn_Click(object sender, RoutedEventArgs e)
        {
            BrowserWindow browser = new BrowserWindow(this);
            browser.Closed += new EventHandler(browser_Closed);
            browser.Show();
        }

        void browser_Closed(object sender, EventArgs e)
        {
            GetAccessToken();
            GetUserInfo();
        }

        private void SaveToken(string tokenName, string token)
        {
            File.WriteAllText("..\\..\\tokens\\" + tokenName + ".txt", token);
        }

        private string ReadToken(string tokenName)
        {
            if (File.Exists("..\\..\\tokens\\" + tokenName + ".txt"))
            {
                var token = File.ReadAllText("..\\..\\tokens\\" + tokenName + ".txt");
                return token;
            }
            return null;
        }

        private Dictionary<string, string> DeserializeJson(string json)
        {
            var jss = new JavaScriptSerializer();
            var d = jss.Deserialize<Dictionary<string, string>>(json);
            return d;
        }

        private void TryGetUserInfo()
        {
            try
            {
                GetUserInfo();
            }
            catch (WebException ex)
            {
                if (((System.Net.HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.Unauthorized)
                {
                    RefreshToken();
                }
                GetUserInfo();
            }
        }

        private void GetUserInfo()
        {
            if (!string.IsNullOrEmpty(Model.AccessToken))
            {
                string apiUrl = @"https://apis.live.net/v5.0/";
                WebClient client = new WebClient();
                Model.UserData = client.DownloadString(new Uri(apiUrl + "me?access_token=" + Model.AccessToken));
                btnSignIn.Visibility = Visibility.Collapsed;

                string imgUrl = apiUrl + "me/picture?access_token=" + Model.AccessToken;
                Model.Image = new BitmapImage(new Uri(imgUrl, UriKind.RelativeOrAbsolute));
            }
        }


        private void GetAccessToken()
        {
            if (!string.IsNullOrEmpty(Model.AuthCode))
            {
                try
                {
                    WebClient client = new WebClient();
                    NameValueCollection form = new NameValueCollection();
                    form.Add("client_id", App.ClientId);
                    form.Add("redirect_uri", "https://login.live.com/oauth20_desktop.srf");
                    form.Add("client_secret", App.ClientSecret);
                    form.Add("code", Model.AuthCode);
                    form.Add("grant_type", "authorization_code");
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");

                    Dictionary<string, string> tokenData = new Dictionary<string, string>();
                    byte[] responseBytes = client.UploadValues("https://login.live.com/oauth20_token.srf", "POST", form);
                    string data = Encoding.ASCII.GetString(responseBytes);
                    tokenData = DeserializeJson(data);
                    GetTokenFromResponse(tokenData);
                }
                catch (Exception ex)
                {
                    var stream = ((System.Net.WebException)(ex)).Response.GetResponseStream();
                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, (int)stream.Length);
                    string response = Encoding.UTF8.GetString(buffer);
                    MessageBox.Show("Error: " + response);
                    throw;
                }
            }
        }

        private void RefreshToken()
        {
            WebClient client = new WebClient();
            NameValueCollection form = new NameValueCollection();
            form.Add("client_id", App.ClientId);
            form.Add("redirect_uri", "http://demo.my/Home/AuthorizationCodeResponse");
            form.Add("client_secret", App.ClientSecret);
            form.Add("refresh_token", Model.RefreshToken);
            form.Add("grant_type", "refresh_token");
            client.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");

            Dictionary<string, string> tokenData = new Dictionary<string, string>();
            byte[] responseBytes = client.UploadValues("https://login.live.com/oauth20_token.srf", "POST", form);
            string data = Encoding.ASCII.GetString(responseBytes);
            tokenData = DeserializeJson(data);
            GetTokenFromResponse(tokenData);
        }

        private void GetTokenFromResponse(Dictionary<string, string> tokenData)
        {
            string accessToken = tokenData["access_token"];
            SaveToken("access", accessToken);
            Model.AccessToken = accessToken;

            string refreshToken = tokenData["refresh_token"];
            SaveToken("refresh", refreshToken);
            Model.RefreshToken = refreshToken;
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
