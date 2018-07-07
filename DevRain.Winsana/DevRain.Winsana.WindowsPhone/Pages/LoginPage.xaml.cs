using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556
using DevRain.Asana.API.Data;
using DevRain.Asana.API.Services;
using DevRain.Asana.API.Services.API;
using DevRain.Windows.WinRT.Common.Core;
using DevRain.Windows.WinRT.Common.Extensions;
using DevRain.Windows.WinRT.Common.MVVM;

namespace DevRain.Winsana.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {


        public LoginPage()
        {
            this.InitializeComponent();
        }


     
    }
}
