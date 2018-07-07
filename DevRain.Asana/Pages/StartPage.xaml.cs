using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using DevRain.Asana.API;
using DevRain.Asana.API.Data;
using DevRain.Asana.API.Services;
using DevRain.Asana.API.Services.API;
using DevRain.WP.Common.Services;
using DevRain.WP.Core.Helpers;
using DevRain.WP.Core.MVVM.Core;

using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace DevRain.Asana.Pages
{
    public partial class StartPage : PhoneApplicationPage
    {
        public StartPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            SmallProfiler.PrintLine("StartPage - OnNavigatedTo");


#if DEBUG
			Bootstrapper.Current.Container.Resolve<IApplicationSettingsService>().SetSetting("ApiKey", "2IPsBtr.EBOluTYUi2mwwz86RkWKNHr6");
			AsanaStateService.Initialize();
#endif


			if(AsanaStateService.IsSetApiKey)
			{
				ExNavigationService.Navigate<MainPage>();
				return;
			}

            if (AsanaStateService.IsSetAuthToken)
            {
				if (NetworkHelper.IsNetworkAvailableNew())
				{
				    var response = await new AsanaRespository().GetUser("me");

					if (AsanaClient.ProcessResponse(response, true))
					{
						ExNavigationService.Navigate<MainPage>();
					}
					else
					{
						ExNavigationService.Navigate<LoginNewPage>();
					}
				}
				else
				{
					ExNavigationService.Navigate<MainPage>();
				}
            }
            else
            {
                ExNavigationService.Navigate<LoginNewPage>();
            }
        }
    }
}