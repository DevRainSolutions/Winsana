using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using DevRain.Asana.API;
using DevRain.Asana.API.Data;
using DevRain.Asana.API.Data.Models;
using DevRain.Asana.API.Services;
using DevRain.Asana.API.Services.API;
using DevRain.Asana.Services;
using DevRain.Asana.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using RestSharp;

namespace DevRain.Asana.Pages
{
    public partial class LoginPage : PhoneApplicationPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            tbApiKey.IsReadOnly = false;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ExNavigationService.ClearNavigationHistory();
            if (AsanaStateService.IsSetAuthToken)
            {
                ExNavigationService.Navigate<MainPage>();
                return;
            }
            DataContext = new LoginViewModel
            {
                IsBusy = false,
                ProgressText = "Logging in",
                
            
            };


            ContentPanel.Visibility = Visibility.Visible;

        }

        private async void BtnLogin_OnClick(object sender, EventArgs e)
        {
            var model = DataContext as LoginViewModel;

            if (string.IsNullOrEmpty(model.ApiKey))
            {
                MessageBox.Show("Set your Asana API key", "Error", MessageBoxButton.OK);
                return;
            }

            btnLogin.IsEnabled = false;
            AsanaStateService.SetAuthToken(model.ApiKey);
            model.IsBusy = true;
            var response = await new AsanaRespository().GetWorkspaces();

            btnLogin.IsEnabled = true;
         

            if(AsanaClient.ProcessResponse(response,true))
            {
                AsanaStateService.SetAuthToken(model.ApiKey);
                ExNavigationService.Navigate<MainPage>();
            }
            else
            {
                AsanaStateService.SetAuthToken("");
                OnError(response);
            }
            model.IsBusy = false;
        }

        void OnError(AsanaResponse<List<AsanaWorkspace>> response)
        {
          
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                MessageBox.Show("Wrong API key", "Authorization failed",MessageBoxButton.OK);
            }
            else
            {
               MessageBox.Show(new AsanaResponseProcessor().GetErrorMessage(response), "Error", MessageBoxButton.OK);
               
            }
        }

        private void TextBox_KeyUp_1(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == Key.Enter && !string.IsNullOrEmpty(tbApiKey.Text))
            {
                BtnLogin_OnClick(sender, EventArgs.Empty);
            }
        }

        void LoginWithTestAccount()
        {
            var model = DataContext as LoginViewModel;

            model.ApiKey = AsanaConstants.ApiKeys.TestAccount1;

            BtnLogin_OnClick(this,EventArgs.Empty);
        }

        private void BtnTestLogin_OnClick(object sender, EventArgs e)
        {
            if (MessageBox.Show("You will be logged with test account. Continue?", "Warning",MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {

                LoginWithTestAccount();
            }

        }
    }
}