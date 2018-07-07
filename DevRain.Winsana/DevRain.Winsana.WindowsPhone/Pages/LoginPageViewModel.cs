using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using DevRain.Asana.API.Data;
using DevRain.Asana.API.Services;
using DevRain.Asana.API.Services.API;
using DevRain.Windows.WinRT.Common.Core;
using DevRain.Windows.WinRT.Common.Extensions;
using DevRain.Windows.WinRT.Common.MVVM;
using DevRain.Winsana.Core.Models;
using DevRain.Winsana.Core.Services.Db;
using DevRain.Winsana.Models;


namespace DevRain.Winsana.Pages
{
    public class LoginPageViewModel : WPAsanaViewModel
    {
        private string _website = "devrain.com";

        public ICommand LoginWithOAuthCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    Page.ContentPanel.Visibility = Visibility.Collapsed;
                    Page.MainPanel.Visibility = Visibility.Collapsed;
                    Page.browserPanel.Visibility = Visibility.Visible;
                    Page.browser.Navigate(
                        new Uri(
                            "https://app.asana.com/-/oauth_authorize?client_id=4423278272782&redirect_uri=http://{0}&response_type=token"
                                .FormatWith(_website)));
                    IsBusy = true;
                });
            }
        }


        LoginPage Page
        {
            get { return View as LoginPage; }
        }
        protected override void OnNavigatedTo()
        {

            DependencyResolverContainer.Resolve<NavigationService>().ClearHistory();

            var browser = Page.browser;

            browser.NavigationCompleted += BrowserOnNavigationCompleted;
            browser.NavigationStarting += browser_NavigationStarting;


            if (AsanaStateService.IsSetAuthToken)
            {
                LoginWithOAuthCommand.Execute(null);
            }
        }

        async void browser_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {

            IsBusy = true;
            try
            {
                if (args.Uri.Host.Contains(_website))
                {
                    string[] data = args.Uri.Fragment.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);

                    var token = data[0].Replace("#access_token=", "");

                    token = WebUtility.UrlDecode(token);

                    AsanaStateService.SetAuthToken(token);

                    var response = await new AsanaApiRepository().GetUser("me");

                    if (AsanaClient.ProcessResponse(response))
                    {
                        if (response.Data.id != SettingsService.LoggedUserId)
                        {
                            await DependencyResolverContainer.Resolve<DbService>().ClearDb();


                        }

                        SettingsService.LoggedUserId = response.Data.id;
                        SettingsService.CurrentUserId = response.Data.id;

                        StateService.DependencyResolverContainer.Resolve<NavigationService>().Navigate<MainPage>();
                    }




                }
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc.Message);

            }

        }

        private void BrowserOnNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {


        }


    }
}
