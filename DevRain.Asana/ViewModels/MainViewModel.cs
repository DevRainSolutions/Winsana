using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Linq;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DevRain.Asana.API;
using DevRain.Asana.API.Data;
using DevRain.Asana.API.Data.Models;
using DevRain.Asana.API.Services;
using DevRain.Asana.API.Services.API;
using DevRain.Asana.API.Services.Db;
using DevRain.Asana.API.Storage;
using DevRain.Asana.Models;
using DevRain.Asana.Pages;
using DevRain.Asana.Services;
using DevRain.WP.Common.Helpers;
using DevRain.WP.Common.Services;
using DevRain.WP.Core.Extensions;
using DevRain.WP.Core.Helpers;
using DevRain.WP.Core.MVVM.Commands;
using DevRain.WP.Core.MVVM.Messaging;
using DevRain.WP.Core.Models;
using DevRain.WP.Core.MVVM.State;
using DevRain.WP.Core.Services;
using DevRain.WP.Core.Services.ApplicationUpdateChecker;
using HuntersWP.Db;
using Microsoft.Phone.Shell;
using System.Threading.Tasks;
namespace DevRain.Asana.ViewModels
{
    public class MainViewModel : AsanaViewModel, IMessageReceiver<TaskStatusCompletedMessage>
    {
	    [Tombstoned]
        public ExObservableCollection<AsanaWorkspace> Workspaces { get; set; }

		[Tombstoned]
        public ExObservableCollection<AsanaUser> Users { get; set; }

		[Tombstoned]
        public ExObservableCollection<AsanaTask> MyTasks { get; set; }

		[Tombstoned]
        public ExObservableCollection<AsanaTask> DueTodayTasks { get; set; }

        		[Tombstoned]
        public ExObservableCollection<AsanaTask> DueTomorrowTasks { get; set; }

        		[Tombstoned]
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

            RefreshCommand = new RelayCommand<object>(Refresh);
            AddTaskCommand = new RelayCommand<object>(AddTask);
            IsBusy = true;

            

        }

		async void LoadUserId()
		{
			if (SettingsService.LoggedUserId > 0) return;

		    var response = await new AsanaRespository().GetUser("me");

			if(AsanaClient.ProcessResponse(response,true))
			{
				SettingsService.LoggedUserId = response.Data.id;
				SettingsService.CurrentUserId = response.Data.id;
			}
		}

        protected override async void OnLoad()
        {
           IsBusy = true;

			LoadUserId();

            if (!AsanaStateService.NeedToSyncData.HasValue)
            {
                AsanaStateService.NeedToSyncData = await new StorageService().CountWorkspaces() == 0;

            }
            //FIRST LOAD
            if (AsanaStateService.NeedToSyncData.Value)
            {

                if (CheckInternetConnection(true))
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

            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            var loadService = new LoadDataService();

            await loadService.Execute();

            Dispatcher.BeginInvoke(async () =>
            {
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Enabled;
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

			ApplicationCheckUpdateService.Check(AsanaConstants.AppId);

			if (NavigationEventArgs.IsBackOrReset())
			{
				NotifyOfPropertyChange(() => Workspaces);
				NotifyOfPropertyChange(() => DueTodayTasks);
                NotifyOfPropertyChange(() => DueTomorrowTasks);
                NotifyOfPropertyChange(() => DueWeekendTasks);
				NotifyOfPropertyChange(() => MyTasks);
				NotifyOfPropertyChange(() => Users);
			}


            IsBusy = false;

			await OffineSyncTaskService.Execute(this,()=>
				                                   {
					                                   DispatcherHelper.OnUi(async () => await LoadTasksFromDb());
													   
				                                   });

			//if (CheckInternetConnection(false))
			//{
			//	AddOperation();
			//	Task.Factory.StartNew(async () =>
			//	{
			//		Debug.WriteLine("sync" + Dispatcher.CheckAccess());
			//		await (new LoadDataService(false).SyncTasks()).ContinueWith(async response =>
			//		{
			//			var result = await response;
			//			RemoveOperation();
			//			if (result)
			//			{
			//				DispatcherHelper.OnUi(() => LoadTasksFromDb());
			//			}
			//		});
			//	});


			//}

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
            if (!CheckInternetConnection(false)) return;

            AddOperation();
            var user = await new StorageService().Find<AsanaUser>(SettingsService.CurrentUserId.Value);

            if (user != null)
            {
                var dataService = new LoadDataService(false, 0);
                foreach (var availableWorkspace in user.GetAvailableWorkspaces())
                {
                    await dataService.LoadUserTasks(user.id, availableWorkspace);
                }

                await LoadTasksFromDb(true);
                RemoveOperation();
            }
            else
            {
                //TODO: how to handle?
            }



        }


        

        async Task LoadUsers(bool isUserAction = false)
        {
            if (!CheckInternetConnection(false)) return;

            AddOperation();

            await new LoadDataService(false).LoadUsers(false);

           await LoadUsersFromDb();


            RemoveOperation();
        }

        async Task LoadTasksFromDb(bool onlyMyTasks = false)
        {
            Debug.WriteLine("LoadTasksFromDb");

          
                if (SettingsService.CurrentUserId.HasValue)
                {
                    var myTasks = await new StorageService().GetActiveTasksByUser(SettingsService.CurrentUserId.Value);
                    foreach (var asanaTask in myTasks)
                    {
                        FillTaskCommands(asanaTask);
                        await MapperService.FillTaskInfo(asanaTask);
                    }

                    DispatcherHelper.OnUi(() =>
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


                    DispatcherHelper.OnUi(() =>
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


                    DispatcherHelper.OnUi(() =>
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


                    DispatcherHelper.OnUi(() =>
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

            DispatcherHelper.OnUi(() =>
            {
                Workspaces.Clear();
                Workspaces.AddRange(dbWorkspaces);
                IsAppBarVisible = true;
            });


        }

        async Task LoadUsersFromDb()
        {

            var users = await GetStorageService().GetUsers();

            foreach (var asanaUser in users)
            {
                asanaUser.TasksCount = await GetStorageService().GetAsyncConnection().Table<AsanaTask>().Where(x => x.assigneeid == asanaUser.id && x.completed == false).CountAsync();
            }

            DispatcherHelper.OnUi(() =>
            {
                Users.Clear();
                Users.AddRange(users);
            });


        }

        async Task LoadWorkspaces(bool isUserAction = false)
        {

            if (!CheckInternetConnection(false)) return;


            AddOperation();


            await new LoadDataService(isUserAction).LoadWorkspaces(false);
            await LoadWorkspacesFromDb();

            RemoveOperation();
     
        }

      

        private async void Refresh(object sender)
        {

            if (IsBusy || IsBlockingProgress) return;
            if (!CheckInternetConnection(true)) return;

            IsBusy = true;
            await LoadData(true);
        }

        private void AddTask(object sender)
        {
            if (IsBlockingProgress) return;

            if (Workspaces.Count == 0)
            {
                MessageBox.Show("No workspaces - refresh data");
                return;
            }

            AsanaWorkspace workspace = null;
            if (SettingsService.DefaultWorkspaceId.HasValue)
            {
                workspace = Workspaces.FirstOrDefault(x => x.id == SettingsService.DefaultWorkspaceId.Value);
            }
            if(workspace == null)
            {
	            workspace = Workspaces.FirstOrDefault();
            }

            if (workspace == null) return;

            ExNavigationService.Navigate<AddEditTask>("workspaceId", workspace.id);

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Container.Resolve<IMessagePublisher>().Unregister(this);
        }   

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            Container.Resolve<IMessagePublisher>().Register(this);
            ExNavigationService.ClearNavigationHistory();

	        if (NavigationEventArgs.IsBackOrReset())
	        {
		        NotifyOfPropertyChange(()=>Workspaces);
				NotifyOfPropertyChange(() => DueTodayTasks);
				NotifyOfPropertyChange(() => MyTasks);
				NotifyOfPropertyChange(() => Users);
                NotifyOfPropertyChange(() => DueTomorrowTasks);
                NotifyOfPropertyChange(() => DueWeekendTasks);
	        }


            RatingNotificationService.Run(new ApplicationSettingsService(), "Rate our application, we'd like to know what you think about Winsana!", 1, 7, "Winsana","ok","cancel");
        }


        public void OnReceive(TaskStatusCompletedMessage message)
        {
            var task = MyTasks.FirstOrDefault(x => x.id == message.Id);
            if (task != null)
            {
                MyTasks.Remove(task);
            }
            
            task = DueTodayTasks.FirstOrDefault(x => x.id == message.Id);
            if (task != null)
            {
                DueTodayTasks.Remove(task);
            }

            task = DueTomorrowTasks.FirstOrDefault(x => x.id == message.Id);
            if (task != null)
            {
                DueTomorrowTasks.Remove(task);
            }

            task = DueWeekendTasks.FirstOrDefault(x => x.id == message.Id);
            if (task != null)
            {
                DueWeekendTasks.Remove(task);
            }


        }
    }
}
