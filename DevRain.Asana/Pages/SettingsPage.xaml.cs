using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Concurrency;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using DevRain.Asana.API;
using DevRain.Asana.API.Data.Models;
using DevRain.Asana.API.Services;
using DevRain.Asana.API.Services.Db;
using DevRain.Asana.API.Storage;
using DevRain.Asana.Services;
using DevRain.Asana.ViewModels;
using DevRain.WP.Common.Helpers;
using DevRain.WP.Core.Controls.Progress;
using DevRain.WP.Core.Helpers;
using DevRain.WP.Core.MVVM.Core;
using HuntersWP.Db;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Threading.Tasks;

namespace DevRain.Asana.Pages
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            this.Loaded += SettingsPage_Loaded;
        }

        void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            (DataContext as AsanaViewModel).IsBusy = true;
            LoadData();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            
        }

       async void LoadData()
       {
           var workspaces = await new StorageService().GetWorkspaces();


           DispatcherHelper.OnUi(() =>
           {
               lstWorkspaces.ItemsSource = workspaces;


               if (SettingsService.DefaultWorkspaceId != null)
               {
                   var item = workspaces.FirstOrDefault(x => x.id == SettingsService.DefaultWorkspaceId.Value);
                   lstWorkspaces.SelectedItem = item;
               }
               else
               {
                   SettingsService.DefaultWorkspaceId = workspaces.First().id;
               }
               lstWorkspaces.SelectionChanged += LstWorkspaces_OnSelectionChanged;
           });





           var users = await new StorageService().GetUsers();
           users.Insert(0, new AsanaUser() { name = "None", id = -1 });

           DispatcherHelper.OnUi(() =>
           {
               lstUsers.ItemsSource = users;
               if (SettingsService.CurrentUserId != null)
               {
                   var user = users.FirstOrDefault(x => x.id == SettingsService.CurrentUserId.Value);
                   lstUsers.SelectedItem = user;

               }
               lstUsers.SelectionChanged += LstUsersOnSelectionChanged;
           });







           chkIsUpdateMainTile.IsChecked = SettingsService.UpdateMainTile;


           (DataContext as AsanaViewModel).IsBusy = false;


       }

        private void LstUsersOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            var user = e.AddedItems[0] as AsanaUser;

            if(user.id > 0)
            {
                SettingsService.CurrentUserId = user.id;
            }
            else
            {
                SettingsService.CurrentUserId = null;
            }

  

        }

        private async void BtnClearLocalCache_OnClick(object sender, RoutedEventArgs e)
        {
            var model = DataContext as AsanaViewModel;
            if (model.IsBlockingProgress) return;

            if (MessageBox.Show("Are you sure you want to clear local data?", "Warning", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                return;



            model.IsBlockingProgress = true;
            Bootstrapper.Current.Container.Resolve<IProgressIndicatorService>().Show("Processing");

            await Bootstrapper.Current.Container.Resolve<DbService>().ClearDb();

            DispatcherHelper.OnUi(() =>
            {
                (DataContext as AsanaViewModel).IsBlockingProgress = false;

                Bootstrapper.Current.Container.Resolve<IProgressIndicatorService>().Hide();

                MessageBox.Show("Cleared local data", "Completed", MessageBoxButton.OK);

                ExNavigationService.Navigate<MainPage>();


            });

        }

        private void LstWorkspaces_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            var workspace = e.AddedItems[0] as AsanaWorkspace;

            SettingsService.DefaultWorkspaceId = workspace.id;

        }

        private async void BtnLogout_OnClick(object sender, RoutedEventArgs e)
        {
            var model = DataContext as AsanaViewModel;
            if (model.IsBlockingProgress) return;
            
            if(MessageBox.Show("Are you sure you want to logout? All local data will be removed.","Warning",MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                return;

            model.IsBlockingProgress = true;
            Bootstrapper.Current.Container.Resolve<IProgressIndicatorService>().Show("Processing");

            await Bootstrapper.Current.Container.Resolve<DbService>().ClearDb();
            DispatcherHelper.OnUi(() =>
            {
                (DataContext as AsanaViewModel).IsBlockingProgress = false;

                Bootstrapper.Current.Container.Resolve<IProgressIndicatorService>().Hide();

                AsanaStateService.ResetAuthData();
                ExNavigationService.ClearNavigationHistory();
                ExNavigationService.Navigate<LoginNewPage>();


            });
           


        }

        private void ChkIsUpdateMainTile_OnChecked(object sender, RoutedEventArgs e)
        {
            SettingsService.UpdateMainTile = chkIsUpdateMainTile.IsChecked.Value;

            if(SettingsService.UpdateMainTile)
            {
                TileService.UpdateMainTile();
            }
            else
            {
                TileService.ClearMainTile();
            }
        }

       
    }
}