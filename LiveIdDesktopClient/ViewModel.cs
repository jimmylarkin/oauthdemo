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
  public class MainWindowViewModel : INotifyPropertyChanged
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
}
