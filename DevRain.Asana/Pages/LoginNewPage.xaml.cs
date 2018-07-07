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
using DevRain.Asana.API.Storage;
using DevRain.Asana.ViewModels;
using DevRain.WP.Common.Extensions;
using DevRain.WP.Core.Extensions;
using DevRain.WP.Core.MVVM.Core;
using HuntersWP.Db;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace DevRain.Asana.Pages
{
	public partial class LoginNewPage : PhoneApplicationPage
	{
		private string _website = "devrain.com";

		public LoginNewPage()
		{
			InitializeComponent();
		}

		LoginViewModel Model
		{
			get { return (LoginViewModel)DataContext; }
		}
		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			browser.Navigated += browser_Navigated;
			browser.Navigating += browser_Navigating;
			browser.IsScriptEnabled = true;

			ExNavigationService.ClearNavigationHistory();
			//if (AsanaStateService.IsSetAuthToken)
			//{
			//	ExNavigationService.Navigate<MainPage>();
			//	return;
			//}
			DataContext = new LoginViewModel
			{
				IsBusy = false,
				ProgressText = "Logging in",


			};

		    if (AsanaStateService.IsSetAuthToken)
		    {
                btnLoginWithOAuth_Click_1(this, new RoutedEventArgs());
		    }
		}

		async void browser_Navigating(object sender, NavigatingEventArgs e)
		{
			Model.IsBusy = true;
			try
			{
				if (e.Uri.Host.Contains(_website))
				{
					string[] data = e.Uri.Fragment.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);

					var token = data[0].Replace("#access_token=", "");

					token = HttpUtility.UrlDecode(token);

					AsanaStateService.SetAuthToken(token);

				    var response = await new AsanaRespository().GetUser("me");

					if(AsanaClient.ProcessResponse(response))
					{
						if(response.Data.id != SettingsService.LoggedUserId)
						{
							await Bootstrapper.Current.Container.Resolve<DbService>().ClearDb();
						}

						SettingsService.LoggedUserId = response.Data.id;
						SettingsService.CurrentUserId = response.Data.id;
						ExNavigationService.Navigate<MainPage>();
					}

					
					

				}
			}
			catch (Exception exc)
			{
				Debug.WriteLine(exc.Message);
				
			}
			
			
		}

		void browser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
		{
			browser.IsScriptEnabled = true;
			Model.IsBusy = false;
		}

		private void btnLoginWithOAuth_Click_1(object sender, RoutedEventArgs e)
		{
			ContentPanel.Visibility = Visibility.Collapsed;
			MainPanel.Visibility = Visibility.Collapsed;
			browserPanel.Visibility = Visibility.Visible;
			browser.Navigate(new Uri("https://app.asana.com/-/oauth_authorize?client_id=4423278272782&redirect_uri=http://{0}&response_type=token".FormatWith(_website)));
			Model.IsBusy = true;
		}
	}
}