﻿using System;
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
using System.Web;

namespace GoogleApiDesktopClient
{
    public partial class BrowserWindow : Window
    {
        private bool isInOobMode;
        static string scope = "https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/userinfo.email https://www.googleapis.com/auth/plus.me https://www.googleapis.com/auth/plus.login";
        MainWindow mainWindow;

        public BrowserWindow(MainWindow parent)
        {
            mainWindow = parent;
            isInOobMode = mainWindow.Model.IsOutOfBrowserMode;
            InitializeComponent();
            string redirectUri = "http://localhost";
            Uri signInUrl = new Uri(String.Format(@"https://accounts.google.com/o/oauth2/auth?client_id={0}&redirect_uri={1}&scope={2}&response_type=code",
                App.ClientId,
                redirectUri,
                scope));
            webBrowser.Navigate(signInUrl);
        }

        private void webBrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Uri.AbsoluteUri.Contains("code="))
            {
                var elements = HttpUtility.ParseQueryString(e.Uri.Query);
                string auth_code = elements["code"];
                mainWindow.Model.AuthCode = auth_code;
                this.Close();
            }
        }
    }
}
