using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Popups;
using DevRain.Asana.API.Data.Models;
using DevRain.Asana.API.Services;
using DevRain.Asana.API.Services.Db;
using DevRain.Windows.WinRT.Common.Controls;
using DevRain.Windows.WinRT.Common.Models;
using DevRain.Windows.WinRT.Common.MVVM;
using DevRain.Windows.WinRT.Common.Services;
using DevRain.Winsana.Core.Models;
using DevRain.Winsana.Core.Services.Db;
using DevRain.Winsana.Models;

namespace DevRain.Winsana.Pages
{
    public class SettingsPageViewModel : WPAsanaViewModel
    {


        public ExObservableCollection<AsanaWorkspace> Workspaces { get; set; }

        public ExObservableCollection<AsanaUser> Users { get; set; }

        private AsanaWorkspace _selectedWorkspace;
        public AsanaWorkspace SelectedWorkspace
        {
            get { return _selectedWorkspace; }
            set
            {
                if (SetValue(ref _selectedWorkspace, value))
                {
                    SettingsService.DefaultWorkspaceId = value.id;
                }
            }
        }

        private AsanaUser _selectedUser;
        public AsanaUser SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                if (SetValue(ref _selectedUser, value))
                {
                    if (value.id > 0)
                    {
                        SettingsService.CurrentUserId = value.id;
                    }
                    else
                    {
                        SettingsService.CurrentUserId = null;
                    }

                }
            }
        }


        private bool _isUpdateMainTile;
        public bool IsUpdateMainTile
        {
            get { return _isUpdateMainTile; }
            set
            {
                SetValue(ref _isUpdateMainTile, value);
                SettingsService.UpdateMainTile = value;

                if (SettingsService.UpdateMainTile)
                {
                    TileService.UpdateMainTile();
                }
                else
                {
                    TileService.ClearMainTile();
                }
            }
        }


        public ICommand ClearCacheCommand
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    if (IsBlockingProgress) return;


                    MessageBoxService.Show("Are you sure you want to clear local data?", "Warning", async (c) =>
                  {
                      IsBlockingProgress = true;
                      DependencyResolverContainer.Resolve<IProgressIndicatorService>().Show("Processing");

                      await DependencyResolverContainer.Resolve<DbService>().ClearDb();

                      IsBlockingProgress = false;

                      DependencyResolverContainer.Resolve<IProgressIndicatorService>().Hide();

                      MessageBoxService.Show("Cleared local data", "Completed");

                      NavigationService.Navigate<MainPage>();
                  });




                });
            }
        }

        public ICommand LogoutCommand
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    if (IsBlockingProgress) return;


                    MessageBoxService.Show("Are you sure you want to logout? All local data will be removed.", "Warning",
                        async c =>
                        {
                            IsBlockingProgress = true;
                            DependencyResolverContainer.Resolve<IProgressIndicatorService>().Show("Processing");

                            await DependencyResolverContainer.Resolve<DbService>().ClearDb();
                            IsBlockingProgress = false;

                            DependencyResolverContainer.Resolve<IProgressIndicatorService>().Hide();

                            AsanaStateService.ResetAuthData();
                            NavigationService.ClearHistory();
                            NavigationService.Navigate<LoginPage>();
                        });


                });

            }
        }

        protected override void OnCreate()
        {
            Workspaces = new ExObservableCollection<AsanaWorkspace>();
            Users = new ExObservableCollection<AsanaUser>();
        }

        protected override void OnNavigatedTo()
        {


            IsBusy = true;
            LoadData();
        }

        async void LoadData()
        {
            var workspaces = await new StorageService().GetWorkspaces();


            Dispatcher.RunAsync(() =>
            {
                Workspaces.AddRange(workspaces);


                if (SettingsService.DefaultWorkspaceId != null)
                {
                    var item = workspaces.FirstOrDefault(x => x.id == SettingsService.DefaultWorkspaceId.Value);

                    SelectedWorkspace = item;

                }
                else
                {
                    SettingsService.DefaultWorkspaceId = workspaces.First().id;
                    SelectedWorkspace = workspaces.FirstOrDefault();
                }




            });





            var users = await new StorageService().GetUsers();
            users.Insert(0, new AsanaUser() { name = "None", id = -1 });

            Dispatcher.RunAsync(() =>
            {

                Users.AddRange(users);

                if (SettingsService.CurrentUserId != null)
                {
                    var user = users.FirstOrDefault(x => x.id == SettingsService.CurrentUserId.Value);
                    SelectedUser = user;

                }

            });







            IsUpdateMainTile = SettingsService.UpdateMainTile;


            IsBusy = false;


        }









    }
}
