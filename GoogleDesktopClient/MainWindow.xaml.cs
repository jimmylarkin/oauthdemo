﻿using System;
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
using Google.Apis.Auth.OAuth2;
using System.Threading;
using Google.Apis.Util.Store;
using Google.Apis.Services;

namespace GoogleApiDesktopClient
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

        private void btnSignInCustom_Click(object sender, RoutedEventArgs e)
        {
            Model.IsOutOfBrowserMode = false;
            BrowserWindow browser = new BrowserWindow(this);
            browser.Closed += new EventHandler(DialogWindowClosed);
            browser.ShowDialog();
        }

        private void btnSignInCustomOob_Click(object sender, RoutedEventArgs e)
        {
            string scope = "https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/userinfo.email https://www.googleapis.com/auth/plus.me https://www.googleapis.com/auth/plus.login";
            string redirectUri = "urn:ietf:wg:oauth:2.0:oob";
            Uri signInUrl = new Uri(String.Format(@"https://accounts.google.com/o/oauth2/auth?client_id={0}&redirect_uri={1}&scope={2}&response_type=code",
                App.ClientId,
                redirectUri,
                scope));

            System.Diagnostics.Process.Start("iexplore.exe", signInUrl.AbsoluteUri);
            Model.IsOutOfBrowserMode = true;
        }

        private void UseCodeClick(object sender, RoutedEventArgs e)
        {
            Model.AuthCode = CodeTextBox.Text;
            GetAccessToken();
            GetUserInfo();
            CodeTextBox.Text = "";
        }

        private void deviceSignIn_Click(object sender, RoutedEventArgs e)
        {
            DeviceWindow deviceWindow = new DeviceWindow(this);
            deviceWindow.Closed += new EventHandler(DialogWindowClosed);
            deviceWindow.ShowDialog();
        }

        private void btnSignInApi_Click(object sender, RoutedEventArgs e)
        {
            var result = SignInWithGoogleApi().Result;
            var service = new Google.Apis.Plus.v1.PlusService(new BaseClientService.Initializer
            {
                HttpClientInitializer = result,
                ApplicationName = "oauthdemo"
            });
            var profileRequest = service.People.Get("me");
            var profile = profileRequest.Execute();

            var jss = new JavaScriptSerializer();
            Model.UserData = jss.Serialize(profile);
            Model.Image = new BitmapImage(new Uri(profile.Image.Url, UriKind.RelativeOrAbsolute));
        }

        private async Task<UserCredential> SignInWithGoogleApi()
        {
            var task = await GoogleWebAuthorizationBroker.AuthorizeAsync(
               new ClientSecrets
               {
                   ClientId = App.ClientId,
                   ClientSecret = App.ClientSecret
               },
               new[] { "https://www.googleapis.com/auth/userinfo.profile", "https://www.googleapis.com/auth/userinfo.email", "https://www.googleapis.com/auth/plus.me", "https://www.googleapis.com/auth/plus.login" },
               "user", CancellationToken.None, new FileDataStore("DesktopClient"));

            // first run Install-Package -Id Microsoft.Bcl.Async -Version 1.0.16
            return task;
        }

        void DialogWindowClosed(object sender, EventArgs e)
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

        public static Dictionary<string, string> DeserializeJson(string json)
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
                WebClient client = new WebClient();
                Model.UserData = client.DownloadString(new Uri("https://www.googleapis.com/plus/v1/people/me?access_token=" + Model.AccessToken));
                buttonsPanel.Visibility = Visibility.Collapsed;

                var jss = new JavaScriptSerializer();
                dynamic userData = jss.Deserialize<dynamic>(Model.UserData);
                var userDataDictionary = userData as Dictionary<string, object>;
                var image = userDataDictionary["image"] as Dictionary<string, object>;
                string imgUrl = image["url"].ToString();
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
                    form.Add("redirect_uri", Model.IsOutOfBrowserMode ? "urn:ietf:wg:oauth:2.0:oob" : "http://localhost");
                    form.Add("client_secret", App.ClientSecret);
                    form.Add("code", Model.AuthCode);
                    form.Add("grant_type", "authorization_code");
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");

                    Dictionary<string, string> tokenData = new Dictionary<string, string>();
                    byte[] responseBytes = client.UploadValues("https://accounts.google.com/o/oauth2/token", "POST", form);
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
                }
            }
        }

        private void RefreshToken()
        {
            try
            {
                WebClient client = new WebClient();
                NameValueCollection form = new NameValueCollection();
                form.Add("client_id", App.ClientId);
                form.Add("client_secret", App.ClientSecret);
                form.Add("refresh_token", Model.RefreshToken);
                form.Add("grant_type", "refresh_token");
                client.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");

                Dictionary<string, string> tokenData = new Dictionary<string, string>();
                byte[] responseBytes = client.UploadValues("https://accounts.google.com/o/oauth2/token", "POST", form);
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
                MessageBox.Show("Unable to refresh the token: " + response);
            }
        }

        public void GetTokenFromResponse(Dictionary<string, string> tokenData)
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
