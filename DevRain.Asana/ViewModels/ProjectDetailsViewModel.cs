using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
using DevRain.Asana.API.Data.Models;
using DevRain.Asana.API.Services;
using DevRain.Asana.API.Services.API;
using DevRain.Asana.API.Services.Db;
using DevRain.Asana.API.Storage;
using DevRain.Asana.Controls;
using DevRain.Asana.Models;
using DevRain.Asana.Pages;
using DevRain.Asana.Services;
using DevRain.WP.Common.Helpers;
using DevRain.WP.Core.Extensions;
using DevRain.WP.Core.Helpers;
using DevRain.WP.Core.MVVM.Commands;
using DevRain.WP.Core.MVVM.Messaging;
using DevRain.WP.Core.Models;
using DevRain.WP.Core.MVVM.State;

namespace DevRain.Asana.ViewModels
{
    public class ProjectDetailsViewModel : AsanaViewModel, IMessageReceiver<TaskStatusCompletedMessage>
    {
		public ICommand PinToStartCommand { get; set; }
        public ICommand ProjectsCommand { get; set; }
		public ICommand ShowProjectDescriptionCommand { get; set; }

		[Tombstoned]
        public long Id { get; set; }


        private AsanaProject _project;
		[Tombstoned]
        public AsanaProject Project
        {
            get { return _project; }
            set { _project = value;
            NotifyOfPropertyChange("Project");
            }
        }

        //TODO: LOAD  Only after navigate
		[Tombstoned]
        public ExObservableCollection<AsanaTask> Tasks { get; set; }
		[Tombstoned]
        ExObservableCollection<AsanaStory> Stories { get; set; }

        public List<AsanaTask> ActiveTasks
        {
            get { return Tasks.Where(x => !x.completed).ToList(); }
        }

        //TODO: LOAD  Only after navigate
        public List<AsanaTask> CompletedTasks
        {
            get { return Tasks.Where(x => x.completed).ToList(); }
        }

        public void NotifyAll()
        {
            NotifyOfPropertyChange("ActiveTasks");
            NotifyOfPropertyChange("CompletedTasks");
            
        }

        protected override void OnCreate()
        {
            Tasks = new ExObservableCollection<AsanaTask>();
            Stories = new ExObservableCollection<AsanaStory>();
            ProjectsCommand = new RelayCommand(ChangeProject);
			PinToStartCommand = new RelayCommand(PinToStart);
			ShowProjectDescriptionCommand = new RelayCommand(o =>
			                                                 {
				                                                 var w = new ProjectDescriptionPopup();
																 w.Initialize(Project.notes);
																 w.Show();
			                                                 });
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Container.Resolve<IMessagePublisher>().Unregister(this);
        }

        protected override async void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            Container.Resolve<IMessagePublisher>().Register(this);

            var id = long.Parse(NavigationManager.GetQueryParameter("id"));
            Id = id;
            var project = await new StorageService().Find<AsanaProject>(Id);

			//project.notes = TextHelper.SafeTruncate(project.notes, 100);
	        Project = project;

			IsPinned = PinService.Exists(PinService.GetProjectDetailsUri(Id));

        }

        protected override void OnLoad()
        {
           LoadData();
        }

        private async Task LoadTasksFromDb()
        {

            var tasks = await new StorageService().GetTasks(Id);


            foreach (var asanaTask in tasks)
            {
                FillTaskCommands(asanaTask);
                await MapperService.FillSubtasksInfo(asanaTask);
            }

            Tasks.Clear();
            Tasks.AddRange(tasks);






            Project.TasksCount = ActiveTasks.Count;

            if (IsPinned)
            {
                PinService.CreateUpdateStartTileAsync(Project.name, Project.TasksCountText,
                    PinService.GetProjectDetailsUri(Id), false);
            }



            NotifyAll();

        }



        public async void LoadData(bool isUserAction = false)
        {
            IsBusy = true;


            await LoadTasksFromDb();

            IsBusy = false;


	        if (NavigationEventArgs.IsBackOrReset())
	        {
		        NotifyAll();
		        return;
	        }


            await DownloadTasks(isUserAction);

			await OffineSyncTaskService.Execute(this, () => { });

        }

        async Task DownloadTasks(bool isUserAction = false)
        {
        
            if (!CheckInternetConnection(false)) return;

            AddOperation();

            var service = new LoadTasksService(isUserAction);
            service.SubTasksLoaded += service_SubTasksLoaded;
            service.SubTasksLoadStarted += service_SubTasksLoadStarted;
            service.LoadCompleted +=   ServiceOnLoadCompleted;
            await  service.LoadProjectTasks(Project.workspaceid, Project.id);

        }

        void service_SubTasksLoadStarted(object sender, EventArgs e)
        {
            AddOperation();
        }

        async void service_SubTasksLoaded(object sender, EventArgs e)
        {
            var task = Tasks.FirstOrDefault(x => x.id == (long) sender);
            if (task == null) return;
            RemoveOperation();


            var subtasks = await new StorageService().GetAllSubTasks(task.id);
            DispatcherHelper.OnUi(() =>
            {
                MapperService.FillSubtasksInfo(task, subtasks);

            });

  
            
        }

        private void ServiceOnLoadCompleted(object sender, EventArgs eventArgs)
        {
            DispatcherHelper.OnUi(async ()=>
                                      {
                                          await LoadTasksFromDb();
                                        
                                      });
            RemoveOperation();

        }

        public void AddTask()
        {
            ExNavigationService.Navigate<AddEditTask>("projectId", Id);
        }

        public void OnReceive(TaskStatusCompletedMessage message)
        {
            var task = Tasks.FirstOrDefault(x => x.id == message.Id);
            if(task == null) return;

            task.IsCompleted = message.IsCompleted;





        }

        void ChangeProject(object o)
        {
            var popup = new WorkspacesListPopup();
            popup.ShowProjects(w =>
            {
                if (w.id != Id)
                {
                    ExNavigationService.Navigate<ProjectDetails>("id", w.id);
                }
            },Project.workspaceid);
        }

		void PinToStart(object sender)
		{
			if (IsPinned)
			{
				PinService.RemoveTile(PinService.GetProjectDetailsUri(Id));
				IsPinned = false;
			}
			else
			{
				PinService.CreateUpdateStartTile(Project.name, Project.TasksCountText, PinService.GetProjectDetailsUri(Id), true);
				IsPinned = true;
			}


		}
    }
}
