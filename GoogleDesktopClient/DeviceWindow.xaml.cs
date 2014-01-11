using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GoogleApiDesktopClient
{
    /// <summary>
    /// Interaction logic for DeviceWindow.xaml
    /// </summary>
    public partial class DeviceWindow : Window
    {
        MainWindow mainWindow;
        string deviceCode;
        private System.Timers.Timer timer = new System.Timers.Timer() { Enabled = false };

        public DeviceWindow(MainWindow parent)
        {
            mainWindow = parent;
            InitializeComponent();
            StartAuthProcess();
            timer.Elapsed += timer_Elapsed;
            timer.Start();
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Stop();
            string result = CheckForAuthCode();
            timer.Start();
        }

        private void StartAuthProcess()
        {
            string scope = "https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/userinfo.email https://www.googleapis.com/auth/plus.me";
            try
            {
                WebClient client = new WebClient();
                NameValueCollection form = new NameValueCollection();
                form.Add("client_id", App.ClientId);
                form.Add("scope", scope);
                client.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");

                Dictionary<string, string> tokenData = new Dictionary<string, string>();
                byte[] responseBytes = client.UploadValues("https://accounts.google.com/o/oauth2/device/code", "POST", form);
                string data = Encoding.ASCII.GetString(responseBytes);
                /* Response format
                        {
                          "device_code" : "4/WKNrvQ52UMnvPjvcr3mUZ-uCG1Gg",
                          "user_code" : "iiirrjga",
                          "verification_url" : "http://www.google.com/device",
                          "expires_in" : 1800,
                          "interval" : 5
                        }
                 */
                tokenData = MainWindow.DeserializeJson(data);
                link.Text = tokenData["verification_url"];
                code.Text = tokenData["user_code"];
                deviceCode = tokenData["device_code"];
                timer.Interval = int.Parse(tokenData["interval"]) * 1000;
            }
            catch (Exception ex)
            {
                var stream = ((System.Net.WebException)(ex)).Response.GetResponseStream();
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, (int)stream.Length);
                string response = Encoding.UTF8.GetString(buffer);
                MessageBox.Show("Unable to obtain device code: " + response);
            }
        }

        private string CheckForAuthCode()
        {
            WebClient client = new WebClient();
            NameValueCollection form = new NameValueCollection();
            form.Add("client_id", App.ClientId);
            form.Add("client_secret", App.ClientSecret);
            form.Add("code", deviceCode);
            form.Add("grant_type", "http://oauth.net/grant_type/device/1.0");
            client.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");

            Dictionary<string, string> tokenData = new Dictionary<string, string>();
            byte[] responseBytes = client.UploadValues(" https://accounts.google.com/o/oauth2/token", "POST", form);
            string data = Encoding.ASCII.GetString(responseBytes);

            tokenData = MainWindow.DeserializeJson(data);
            if (tokenData.ContainsKey("error"))
            {
                if (tokenData["error"] == "authorization_pending")
                {
                    return string.Empty;
                }
                if (tokenData["error"] == "slow_down")
                {
                    timer.Interval = timer.Interval + 1000;
                    return string.Empty;
                }
            } else {
                mainWindow.GetTokenFromResponse(tokenData);
                if (this.Dispatcher.CheckAccess())
                {
                    this.Close();
                }
                else
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new ThreadStart(this.Close));
                }
            }
            return string.Empty;
        }

    }
}
