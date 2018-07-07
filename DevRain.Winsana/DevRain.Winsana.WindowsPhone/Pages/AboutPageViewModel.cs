using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DevRain.Asana.API.Data;
using DevRain.Windows.WinRT.Common.Helpers;
using DevRain.Windows.WinRT.Common.MVVM;
using DevRain.Winsana.Core.Models;
using DevRain.Winsana.Models;

namespace DevRain.Winsana.Pages
{
    public class AboutPageViewModel : WPAsanaViewModel
    {
        public string Version { get; set; }

        public ICommand FeedbackCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    DeviceHelper.SendEmail(AsanaConstants.FeedbackEmail,AsanaConstants.AppTitle);
                });
            }
        }


        public ICommand RateAppCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    //TODO:RateAppCommand
                });
            }
        }

        public ICommand VisitWebsiteCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    DeviceHelper.OpenInBrowser("http://devrain.com/");
                });
            }
        }

        public ICommand ShowOtherAppsCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    //TODO:ShowOtherAppsCommand
                });
            }
        }

        protected override void OnNavigatedTo()
        {

            Version = string.Format("Version: {0}", DeviceHelper.GetAppPackageVersion(true).ToString());
            RaisePropertyChanged(()=>Version);
        }
    }
}
