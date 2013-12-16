using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace GoogleApiDesktopClient
{
    public partial class BrowserWindow : Window
    {
        private bool isInOobMode;
        static string scope = "https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/userinfo.email";
        MainWindow mainWindow;

        public BrowserWindow(MainWindow parent, bool inOobMode)
        {
            isInOobMode = inOobMode;
            mainWindow = parent;
            InitializeComponent();
            string redirectUri = "http://localhost";
            if (isInOobMode)
            {
                redirectUri = "urn:ietf:wg:oauth:2.0:oob";
            }

            Uri signInUrl = new Uri(String.Format(@"https://accounts.google.com/o/oauth2/auth?client_id={0}&redirect_uri={1}&scope={2}&response_type=code", 
                App.ClientId,
                redirectUri,
                scope));
            webBrowser.Navigate(signInUrl);
        }

        private void webBrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (isInOobMode)
            {
                //dynamic doc = webBrowser.Document;
                //if (doc.Title.Contains("code="))
                //{
                //    string auth_code = Regex.Split(doc.Title, "code=")[1];
                //    mainWindow.Model.AuthCode = auth_code;
                //    this.Close();
                //}
            }
            else
            {
                if (e.Uri.AbsoluteUri.Contains("code="))
                {
                    string auth_code = Regex.Split(e.Uri.AbsoluteUri, "code=")[1];
                    mainWindow.Model.AuthCode = auth_code;
                    this.Close();
                }
            }
        }
    }
}
