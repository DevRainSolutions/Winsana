using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using DevRain.WP.Core.MVVM.Core;
using Microsoft.Phone.Controls;

namespace DevRain.Asana.API
{

    public class ExNavigationService
    {
        private static readonly PhoneApplicationFrame frame = Application.Current.RootVisual as PhoneApplicationFrame;



        public static void GoBack()
        {
            Bootstrapper.Current.NavigationManager.GoBack();
        }


        public static void Navigate<T>()
        {
            Navigate<T>(null);
        }







        public static void ClearNavigationHistory()
        {
            Bootstrapper.Current.NavigationManager.ClearNavigationHistory();
        }

        public static void Navigate(string page,params object[] parameters)
        {
            var s = "";
            if (parameters != null)
            {
                for (int i = 0; i < parameters.Count(); i += 2)
                {
                    if (parameters[i + 1] == null) continue;

                    s += parameters[i] + "=" + parameters[i + 1] + "&";
                }
            }
            Bootstrapper.Current.NavigationManager.Navigate(string.Format("/Pages/{0}.xaml?" + s, page));
        }

        public static void Navigate<T>(params object[] parameters)
        {
            Navigate(typeof (T).Name, parameters);
        }
    }
}
