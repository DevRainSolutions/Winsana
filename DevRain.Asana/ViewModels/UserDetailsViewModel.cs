using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;
using DevRain.Asana.API.Data.Models;
using DevRain.Asana.API.Services;
using DevRain.Asana.API.Services.API;
using DevRain.Asana.API.Services.Db;
using DevRain.Asana.API.Storage;
using DevRain.Asana.Models;
using DevRain.Asana.Services;
using DevRain.WP.Core.Extensions;
using DevRain.WP.Core.Helpers;
using DevRain.WP.Core.MVVM.Commands;
using DevRain.WP.Core.MVVM.Messaging;
using DevRain.WP.Core.Models;
using DevRain.WP.Core.MVVM.State;

namespace DevRain.Asana.ViewModels
{
    public class UserDetailsViewModel : AsanaViewModel, IMessageReceiver<TaskStatusCompletedMessage>
    {

		[Tombstoned]
		public long UserId { get; set; }

		private AsanaUser user;
			[Tombstoned]
		public AsanaUser User
		{
			get { return user; }
			set
			{
				user = value;
				NotifyOfPropertyChange("User");
			}
		}


            //TODO: LOAD  Only after navigate
			[Tombstoned]
		public ExObservableCollection<AsanaTask> Tasks { get; set; }

		public List<AsanaTask> ActiveTasks
		{
			get { return Tasks.Where(x => !x.completed).ToList(); }
		}

        //TODO: LOAD  Only after navigate
		public List<AsanaTask> CompletedTasks
		{
			get { return Tasks.Where(x => x.completed).ToList(); }
		}

		public ICommand PinToStartCommand { get; set; }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            Container.Resolve<IMessagePublisher>().Register(this);
            var userId = long.Parse(NavigationManager.GetQueryParameter("id"));

            UserId = userId;
			IsPinned = PinService.Exists(PinService.GetUserDetailsUri(userId));
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Container.Resolve<IMessagePublisher>().Unregister(this);
        }

        protected override void OnCreate()
        {
            Tasks = new ExObservableCollection<AsanaTask>();
			PinToStartCommand = new RelayCommand(PinToStart);
        }



	

        public void NotifyAll()
        {
            NotifyOfPropertyChange("ActiveTasks");
            NotifyOfPropertyChange("CompletedTasks");

        }

        protected override void OnLoad()
        {
            LoadData();
        }

       async Task LoadTasksFromDb()
        {

            var tasks = await GetStorageService().GetTasksByUser(UserId);

            foreach (var asanaTask in tasks)
            {
                FillTaskCommands(asanaTask);
                await MapperService.FillTaskInfo(asanaTask);
                await MapperService.FillSubtasksInfo(asanaTask);
            }

            Tasks.Clear();
            Tasks.AddRange(tasks);
            NotifyAll();

        }

        public async void LoadData(bool isUserAction = false)
        {
            IsBusy = true;
            User = await new StorageService().Find<AsanaUser>(UserId);

            await LoadTasksFromDb();

            IsBusy = false;

	        if (NavigationEventArgs.IsBackOrReset())
	        {
		        NotifyAll();
		        return;
	        }

            if (CheckInternetConnection(false))
            {

                AddOperation();
                


                var tasks = new List<System.Threading.Tasks.Task>();
                foreach (var w in User.GetAvailableWorkspaces())
                {
                    tasks.Add(LoadTasks(w, isUserAction));
                }

                
                System.Threading.Tasks.Task.Factory.ContinueWhenAll(tasks.ToArray(), x =>
	                                                                                     {
		                                                                                     User.TasksCount =
			                                                                                     ActiveTasks.Count;

																							 if (IsPinned)
																							 {
																								 PinService.CreateUpdateStartTileAsync(User.name, User.TasksCountText, PinService.GetUserDetailsUri(UserId), false);
																							 }

                                                                                             RemoveOperation();
                                                                                         });


            }
			await OffineSyncTaskService.Execute(this, () => { });

        }

        async System.Threading.Tasks.Task LoadTasks(long workspaceId, bool isUserAction = false)
        {
            await new LoadDataService(isUserAction).LoadUserTasks(UserId, workspaceId);

        }

        public void OnReceive(TaskStatusCompletedMessage message)
        {
            var task = Tasks.FirstOrDefault(x => x.id == message.Id);
            if (task == null) return;

            task.IsCompleted = message.IsCompleted;
        }

		void PinToStart(object sender)
		{
			if (IsPinned)
			{
				PinService.RemoveTile(PinService.GetUserDetailsUri(UserId));
				IsPinned = false;
			}
			else
			{
				PinService.CreateUpdateStartTile(User.name, User.TasksCountText, PinService.GetUserDetailsUri(UserId), true);
				IsPinned = true;
			}


		}
    }
}
