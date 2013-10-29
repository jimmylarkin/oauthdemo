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

namespace LiveIdDesktopClient
{
  public class ViewModel : INotifyPropertyChanged
  {
    private string accessToken;
    private string refreshToken;
    private BitmapImage image;
    private string userData;

    public string AccessToken
    {
      get
      {
        return accessToken;
      }
      set
      {
        accessToken = value;
        RaisePropertyChanged("AccessToken");
      }
    }

    public string RefreshToken
    {
      get
      {
        return refreshToken;
      }
      set
      {
        refreshToken = value;
        RaisePropertyChanged("RefreshToken");
      }
    }
    public string UserData
    {
      get
      {
        return userData;
      }
      set
      {
        userData = value;
        RaisePropertyChanged("UserData");
      }
    }

    public string AuthCode { get; set; }

    public BitmapImage Image
    {
      get
      {
        return image;
      }
      set
      {
        image = value;
        RaisePropertyChanged("Image");
      }
    }

    private void RaisePropertyChanged(string propName)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(propName));
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }

  public partial class MainWindow : Window
  {
    static string client_id = "000000004010B428";
    static string client_secret = "wj6BDdBBKhj81pdi7LMBsLzZHiuWyIwf";
    static string accessTokenUrl = String.Format(@"https://login.live.com/oauth20_token.srf?client_id={0}&client_secret={1}&redirect_uri=https://login.live.com/oauth20_desktop.srf&grant_type=authorization_code&code=", client_id, client_secret);
    static string apiUrl = @"https://apis.live.net/v5.0/";

    public ViewModel Model { get; set; }

    public MainWindow()
    {
      InitializeComponent();
      Model = new ViewModel();

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

    private void GetAccessToken()
    {
      if (!string.IsNullOrEmpty(Model.AuthCode))
      {
        WebClient wc = new WebClient();
        string data = wc.DownloadString(new Uri(accessTokenUrl + Model.AuthCode));
        Dictionary<string, string> tokenData = new Dictionary<string, string>();
        tokenData = DeserializeJson(data);

        string accessToken = tokenData["access_token"];
        SaveToken("access", accessToken);
        Model.AccessToken = accessToken;

        string refreshToken = tokenData["refresh_token"];
        SaveToken("refresh", refreshToken);
        Model.RefreshToken = refreshToken;
      }
    }

    private void SaveToken(string tokenName, string token)
    {
      File.WriteAllText(tokenName + ".txt", token);
    }

    private string ReadToken(string tokenName)
    {
      if (File.Exists(tokenName + ".txt"))
      {
        var token = File.ReadAllText(tokenName + ".txt");
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

    private void GetUserInfo()
    {
      if (!string.IsNullOrEmpty(Model.AccessToken))
      {
        WebClient wc = new WebClient();
        Model.UserData = wc.DownloadString(new Uri(apiUrl + "me?access_token=" + Model.AccessToken));
        btnSignIn.Visibility = Visibility.Collapsed;
        string imgUrl = apiUrl + "me/picture?access_token=" + Model.AccessToken;
        Model.Image = new BitmapImage(new Uri(imgUrl, UriKind.RelativeOrAbsolute));
      }
    }

    private void Window_Unloaded(object sender, RoutedEventArgs e)
    {
      Application.Current.Shutdown();
    }
  }
}
