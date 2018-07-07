using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using DevRain.Asana.API.Data.Models;
using DevRain.Asana.API.Services;
using DevRain.Asana.API.Services.API;
using DevRain.Asana.API.Services.Db;
using DevRain.Asana.API.Storage;
using DevRain.Asana.Models;
using DevRain.Asana.Services;
using DevRain.WP.Core.Extensions;
using DevRain.WP.Core.MVVM.Messaging;
using DevRain.WP.Core.Models;
using DevRain.WP.Core.MVVM.State;

namespace DevRain.Asana.ViewModels
{
    public class TagDetailsViewModel : AsanaViewModel, IMessageReceiver<TaskStatusCompletedMessage>
    {
		[Tombstoned]
        public AsanaTag Tag { get; set; }

        //TODO: load after navigate
		[Tombstoned]
        public ExObservableCollection<AsanaTask> Tasks { get; set; }

        public List<AsanaTask> ActiveTasks
        {
            get { return Tasks.Where(x => !x.completed).ToList(); }
        }

        //TODO: load after navigate
        public List<AsanaTask> CompletedTasks
        {
            get { return Tasks.Where(x => x.completed).ToList(); }
        }

        public void NotifyAll()
        {
			NotifyOfPropertyChange(() => ActiveTasks);
			NotifyOfPropertyChange(() => CompletedTasks);

        }

        protected override async void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            Container.Resolve<IMessagePublisher>().Register(this);
            var id = long.Parse(NavigationManager.GetQueryParameter("id"));
      
            Tag = await new StorageService().Find<AsanaTag>(id);
            NotifyOfPropertyChange(()=>Tag);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Container.Resolve<IMessagePublisher>().Unregister(this);
        }   

        protected override void OnCreate()
        {
            Tasks = new ExObservableCollection<AsanaTask>();
        }

        protected override async void OnLoad()
        {
            IsBusy = true;

			await LoadTasksFromDb();
			IsBusy = false;
	        if (NavigationEventArgs.IsBackOrReset())
	        {
		        NotifyAll();
				return;
	        }


            LoadTasks();
        }

        public async void LoadTasks(bool isUserAction = false)
        {
            if (!CheckInternetConnection(false)) return;

            AddOperation();

            await new LoadDataService(isUserAction).LoadTagTasks(Tag.id,false);


            await LoadTasksFromDb();

            RemoveOperation();
        }


        async Task LoadTasksFromDb()
        {
            var tasks = await new StorageService().GetTasksByTag(Tag.id);

            foreach (var asanaTask in tasks)
            {
                FillTaskCommands(asanaTask);
                await MapperService.FillTaskInfo(asanaTask);
                await MapperService.FillSubtasksInfo(asanaTask);
            }


            Tasks.Clear();
            Tasks.AddRange(tasks);

            NotifyAll();
            IsBusy = false;

        }

        public void OnReceive(TaskStatusCompletedMessage message)
        {
            var task = Tasks.FirstOrDefault(x => x.id == message.Id);
            if (task == null) return;

            task.IsCompleted = message.IsCompleted;
        }

        
    }
}
