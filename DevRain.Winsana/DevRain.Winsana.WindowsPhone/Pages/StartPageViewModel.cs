using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using DevRain.Asana.API.Data;
using DevRain.Asana.API.Services;
using DevRain.Asana.API.Services.API;
using DevRain.Windows.WinRT.Common.Core;
using DevRain.Windows.WinRT.Common.Helpers;
using DevRain.Windows.WinRT.Common.Models;
using DevRain.Windows.WinRT.Common.MVVM;
using DevRain.Windows.WinRT.Common.Services;
using DevRain.Winsana.Core.Models;
using DevRain.Winsana.Models;

namespace DevRain.Winsana.Pages
{
    public class StartPageViewModel : WPAsanaViewModel
    {
        protected override async void OnNavigatedTo()
        {

            
            //if(AsanaStateService.IsSetApiKey)
            //{
            //    ExNavigationService.Navigate<MainPage>();
            //    return;
            //}

            if (AsanaStateService.IsSetAuthToken)
            {
                if (await NetworkHelper.IsNetworkAvailableViaRequest(3000))
				{
				    var response = await new AsanaApiRepository().GetUser("me");

					if (AsanaClient.ProcessResponse(response, true))
					{
						NavigationService.Navigate<MainPage>();
					}
					else
					{
                        NavigationService.Navigate<LoginPage>();
					}
				}
				else
				{
                    NavigationService.Navigate<MainPage>();
				}
            }
            else
            {
                NavigationService.Navigate<LoginPage>();

            }
         



        }
    }
}
