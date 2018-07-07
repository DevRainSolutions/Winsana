using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml.Navigation;
using DevRain.Asana.API.Data;
using DevRain.Asana.API.Data.Models;
using DevRain.Asana.API.Services;
using DevRain.Asana.API.Services.API;
using DevRain.Asana.API.Services.Db;
using DevRain.Windows.WinRT.Common.Models;
using DevRain.Windows.WinRT.Common.MVVM;
using DevRain.Windows.WinRT.Common.Services;
using DevRain.Winsana.Core.Models;
using DevRain.Winsana.Models;
using DevRain.Winsana.Services;

namespace DevRain.Winsana.Pages
{
    public class MainPageViewModel : WPAsanaViewModel
    {
    
        public ExObservableCollection<AsanaWorkspace> Workspaces { get; set; }

      
        public ExObservableCollection<AsanaUser> Users { get; set; }

       
        public ExObservableCollection<AsanaTask> MyTasks { get; set; }

  
        public ExObservableCollection<AsanaTask> DueTodayTasks { get; set; }

        public ExObservableCollection<AsanaTask> DueTomorrowTasks { get; set; }

        public ExObservableCollection<AsanaTask> DueWeekendTasks { get; set; }




        public ICommand RefreshCommand { get; set; }
        public ICommand AddTaskCommand { get; set; }

        protected override void OnCreate()
        {
            Workspaces = new ExObservableCollection<AsanaWorkspace>();
            Users = new ExObservableCollection<AsanaUser>();
            MyTasks = new ExObservableCollection<AsanaTask>();
            DueTodayTasks = new ExObservableCollection<AsanaTask>();
            DueTomorrowTasks = new ExObservableCollection<AsanaTask>();
            DueWeekendTasks = new ExObservableCollection<AsanaTask>();

            RefreshCommand = new DelegateCommand(Refresh);
            AddTaskCommand = new DelegateCommand(AddTask);
            IsBusy = true;



        }

        async Task LoadUserId()
        {
            if (SettingsService.LoggedUserId > 0) return;

            var response = await new AsanaApiRepository().GetUser("me");

            if (AsanaClient.ProcessResponse(response, true))
            {
                SettingsService.LoggedUserId = response.Data.id;
                SettingsService.CurrentUserId = response.Data.id;
            }
        }

        protected override async void OnNavigatedTo()
        {

            NavigationService.ClearHistory();
            IsBusy = true;

            await LoadUserId();

            if (!AsanaStateService.NeedToSyncData.HasValue)
            {
                AsanaStateService.NeedToSyncData = await new StorageService().CountWorkspaces() == 0;

            }
            //FIRST LOAD
            if (AsanaStateService.NeedToSyncData.Value)
            {

                if (await CheckInternetConnection(true))
                    LoadDataOnFirstLoad();
                else
                {
                    IsBusy = false;
                }
            }
            else
            {



                await LoadData();

                TileService.UpdateMainTile();

            }

            //		RatingNotificationService.Run(Container.Resolve<IApplicationSettingsService>(), "Rate our application, we'd like to know what you think about Winsana!",
            //5,10, "Winsana");

        }

        async void LoadDataOnFirstLoad()
        {
            IsBlockingProgress = true;
            IsBusy = false;

          //  PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            var loadService = new LoadDataService();

            await loadService.Execute();

            Dispatcher.RunAsync(async () =>
            {
                //PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Enabled;
                IsBlockingProgress = false;
                IsBusy = true;
                await LoadWorkspacesFromDb();
                await LoadUsersFromDb();
                await LoadTasksFromDb();
                IsBusy = false;

                TileService.UpdateMainTile();

            });
        }



        async Task LoadData(bool isUserAction = false)
        {

            await LoadWorkspacesFromDb();
            await LoadTasksFromDb();
            await LoadUsersFromDb();

           // ApplicationCheckUpdateService.Check(AsanaConstants.AppId);

            //if (NavigationEventArgs.IsBackOrReset())
            //{
            //    NotifyOfPropertyChange(() => Workspaces);
            //    NotifyOfPropertyChange(() => DueTodayTasks);
            //    NotifyOfPropertyChange(() => DueTomorrowTasks);
            //    NotifyOfPropertyChange(() => DueWeekendTasks);
            //    NotifyOfPropertyChange(() => MyTasks);
            //    NotifyOfPropertyChange(() => Users);
            //}


            IsBusy = false;

            await OffineSyncTaskService.Execute(this, () =>
            {
                Dispatcher.RunAsync(async () => await LoadTasksFromDb());

            });


            await LoadWorkspaces(isUserAction);



            await LoadUsers(isUserAction);

            if (NavigationEventArgs.NavigationMode != NavigationMode.Back || isUserAction)
            {
                LoadMyTasks(isUserAction);
            }

        }

        async void LoadMyTasks(bool isUserAction = false)
        {
            if (!SettingsService.CurrentUserId.HasValue)
            {
                return;
            }
            if (!await CheckInternetConnection(false)) return;

         //   AddOperation();
            var user = await new StorageService().Find<AsanaUser>(SettingsService.CurrentUserId.Value);

            if (user != null)
            {
                var dataService = new LoadDataService(false, 0);
                foreach (var availableWorkspace in user.GetAvailableWorkspaces())
                {
                    await dataService.LoadUserTasks(user.id, availableWorkspace);
                }

                await LoadTasksFromDb(true);
               // RemoveOperation();
            }
            else
            {
                //TODO: how to handle?
            }



        }




        async Task LoadUsers(bool isUserAction = false)
        {
            if (!await CheckInternetConnection(false)) return;

            //AddOperation();

            await new LoadDataService(false).LoadUsers(false);

            await LoadUsersFromDb();


        //    RemoveOperation();
        }

        async Task LoadTasksFromDb(bool onlyMyTasks = false)
        {
            
            if (SettingsService.CurrentUserId.HasValue)
            {
                var myTasks = await new StorageService().GetActiveTasksByUser(SettingsService.CurrentUserId.Value);
                foreach (var asanaTask in myTasks)
                {
                    FillTaskCommands(asanaTask);
                    await MapperService.FillTaskInfo(asanaTask);
                }

                Dispatcher.RunAsync(() =>
                {
                    MyTasks.Clear();
                    MyTasks.AddRange(myTasks);
                });



            }

            if (!onlyMyTasks)
            {

                var dueTodayTasks = await new StorageService().GetDueTodayTasks();

                foreach (var asanaTask in dueTodayTasks)
                {
                    FillTaskCommands(asanaTask);
                    await MapperService.FillTaskInfo(asanaTask);

                    if (asanaTask.assigneeid > 0)
                    {
                        var user = await new StorageService().Find<AsanaUser>(asanaTask.assigneeid);

                        if (user != null)
                            asanaTask.UserName = user.name;
                    }
                }


                Dispatcher.RunAsync(() =>
                {
                    DueTodayTasks.Clear();
                    DueTodayTasks.AddRange(dueTodayTasks);
                });

                var tomorrowTasks = await new StorageService().GetDueTomorrowTasks();

                foreach (var asanaTask in tomorrowTasks)
                {
                    FillTaskCommands(asanaTask);
                    await MapperService.FillTaskInfo(asanaTask);

                    if (asanaTask.assigneeid > 0)
                    {
                        var user = await new StorageService().Find<AsanaUser>(asanaTask.assigneeid);

                        if (user != null)
                            asanaTask.UserName = user.name;
                    }
                }


                Dispatcher.RunAsync(() =>
                {
                    DueTomorrowTasks.Clear();
                    DueTomorrowTasks.AddRange(tomorrowTasks);
                });

                var weekendTasks = await new StorageService().GetDueWeekendTasks();

                foreach (var asanaTask in weekendTasks)
                {
                    FillTaskCommands(asanaTask);
                    await MapperService.FillTaskInfo(asanaTask);

                    if (asanaTask.assigneeid > 0)
                    {
                        var user = await new StorageService().Find<AsanaUser>(asanaTask.assigneeid);
                        if (user != null)
                            asanaTask.UserName = user.name;
                    }
                }


                Dispatcher.RunAsync(() =>
                {
                    DueWeekendTasks.Clear();
                    DueWeekendTasks.AddRange(weekendTasks);
                });


            }



        }

        async Task LoadWorkspacesFromDb()
        {
            var dbWorkspaces = await new StorageService().GetWorkspaces();

            foreach (var asanaWorkspace in dbWorkspaces)
            {
                asanaWorkspace.ProjectsCount =
                    await GetStorageService().GetAsyncConnection().Table<AsanaProject>().Where(
                        x => x.workspaceid == asanaWorkspace.id && x.archived == false).CountAsync();
            }
            AsanaStateService.NeedToSyncData = !dbWorkspaces.Any();

            Dispatcher.RunAsync(() =>
            {
                Workspaces.Clear();
                Workspaces.AddRange(dbWorkspaces);
               // IsAppBarVisible = true;
            });


        }

        async Task LoadUsersFromDb()
        {

            var users = await GetStorageService().GetUsers();

            foreach (var asanaUser in users)
            {
                asanaUser.TasksCount = await GetStorageService().GetAsyncConnection().Table<AsanaTask>().Where(x => x.assigneeid == asanaUser.id && x.completed == false).CountAsync();
            }

            Dispatcher.RunAsync(() =>
            {
                Users.Clear();
                Users.AddRange(users);
            });


        }

        async Task LoadWorkspaces(bool isUserAction = false)
        {

            if (!await CheckInternetConnection(false)) return;


         //   AddOperation();


            await new LoadDataService(isUserAction).LoadWorkspaces(false);
            await LoadWorkspacesFromDb();

         //   RemoveOperation();

        }



        private async void Refresh(object sender)
        {

            if (IsBusy || IsBlockingProgress) return;
            if (!await CheckInternetConnection(true)) return;

            IsBusy = true;
            await LoadData(true);
        }

        private void AddTask(object sender)
        {
            if (IsBlockingProgress) return;

            if (Workspaces.Count == 0)
            {
                new MessageDialog("No workspaces - refresh data").ShowAsync();
                return;
            }

            AsanaWorkspace workspace = null;
            if (SettingsService.DefaultWorkspaceId.HasValue)
            {
                workspace = Workspaces.FirstOrDefault(x => x.id == SettingsService.DefaultWorkspaceId.Value);
            }
            if (workspace == null)
            {
                workspace = Workspaces.FirstOrDefault();
            }

            if (workspace == null) return;

            //ExNavigationService.Navigate<AddEditTask>("workspaceId", workspace.id);

        }

        protected override void OnNavigatedFrom()
        {
        //    Container.Resolve<IMessagePublisher>().Unregister(this);
        }

        //protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        //{
        //   // Container.Resolve<IMessagePublisher>().Register(this);


        //    //if (NavigationEventArgs.IsBackOrReset())
        //    //{
        //    //    NotifyOfPropertyChange(() => Workspaces);
        //    //    NotifyOfPropertyChange(() => DueTodayTasks);
        //    //    NotifyOfPropertyChange(() => MyTasks);
        //    //    NotifyOfPropertyChange(() => Users);
        //    //    NotifyOfPropertyChange(() => DueTomorrowTasks);
        //    //    NotifyOfPropertyChange(() => DueWeekendTasks);
        //    //}


        //   // RatingNotificationService.Run(new ApplicationSettingsService(), "Rate our application, we'd like to know what you think about Winsana!", 1, 7, "Winsana", "ok", "cancel");
        //}


        //public void OnReceive(TaskStatusCompletedMessage message)
        //{
        //    var task = MyTasks.FirstOrDefault(x => x.id == message.Id);
        //    if (task != null)
        //    {
        //        MyTasks.Remove(task);
        //    }

        //    task = DueTodayTasks.FirstOrDefault(x => x.id == message.Id);
        //    if (task != null)
        //    {
        //        DueTodayTasks.Remove(task);
        //    }

        //    task = DueTomorrowTasks.FirstOrDefault(x => x.id == message.Id);
        //    if (task != null)
        //    {
        //        DueTomorrowTasks.Remove(task);
        //    }

        //    task = DueWeekendTasks.FirstOrDefault(x => x.id == message.Id);
        //    if (task != null)
        //    {
        //        DueWeekendTasks.Remove(task);
        //    }


        //}

    }
}
