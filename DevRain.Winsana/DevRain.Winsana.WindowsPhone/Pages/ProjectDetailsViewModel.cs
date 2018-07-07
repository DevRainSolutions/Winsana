using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DevRain.Asana.API.Data.Models;
using DevRain.Asana.API.Services;
using DevRain.Asana.API.Services.API;
using DevRain.Asana.API.Services.Db;
using DevRain.Windows.WinRT.Common.Models;
using DevRain.Windows.WinRT.Common.MVVM;
using DevRain.Winsana.Models;
using DevRain.Winsana.Services;

namespace DevRain.Winsana.Pages
{
    public class ProjectDetailsViewModel:WPAsanaViewModel
    {
        public ICommand PinToStartCommand { get; set; }
        public ICommand ProjectsCommand { get; set; }
        public ICommand ShowProjectDescriptionCommand { get; set; }

        public ICommand RefreshCommand { get; set; }

        public ICommand AddTaskCommand { get; set; }

        public ICommand EditProjectCommand { get; set; }


        public long Id { get; set; }


        private AsanaProject _project;
        
        public AsanaProject Project
        {
            get { return _project; }
            set { SetValue(ref _project, value); }
        }

        public ExObservableCollection<AsanaTask> Tasks { get; set; }

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
            RaisePropertyChanged(() => ActiveTasks);
            RaisePropertyChanged(() => CompletedTasks);


        }

        protected override void OnCreate()
        {
            Tasks = new ExObservableCollection<AsanaTask>();
            Stories = new ExObservableCollection<AsanaStory>();
            ProjectsCommand = new DelegateCommand(ChangeProject);
            PinToStartCommand = new DelegateCommand(PinToStart);
            ShowProjectDescriptionCommand = new DelegateCommand(o =>
            {
                //var w = new ProjectDescriptionPopup();
                //w.Initialize(Project.notes);
                //w.Show();
            });
        }

        protected override void OnNavigatedFrom()
        {

        }

        protected override async void OnNavigatedTo()
        {

            var id = (long) NavigationEventArgs.Parameter;
            Id = id;
            var project = await new StorageService().Find<AsanaProject>(Id);

            
            Project = project;

           // IsPinned = PinService.Exists(PinService.GetProjectDetailsUri(Id));

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
                //PinService.CreateUpdateStartTileAsync(Project.name, Project.TasksCountText,
                //    PinService.GetProjectDetailsUri(Id), false);
            }



            NotifyAll();

        }



        public async void LoadData(bool isUserAction = false)
        {
            IsBusy = true;


            await LoadTasksFromDb();

            IsBusy = false;


            //if (NavigationEventArgs.IsBackOrReset())
            //{
            //    NotifyAll();
            //    return;
            //}


            await DownloadTasks(isUserAction);

            await OffineSyncTaskService.Execute(this, () => { });

        }

        async Task DownloadTasks(bool isUserAction = false)
        {

            if (!await CheckInternetConnection(false)) return;

            //AddOperation();

            var service = new LoadTasksService(isUserAction);
            service.SubTasksLoaded += service_SubTasksLoaded;
            service.SubTasksLoadStarted += service_SubTasksLoadStarted;
            service.LoadCompleted += ServiceOnLoadCompleted;
            await service.LoadProjectTasks(Project.workspaceid, Project.id);

        }

        void service_SubTasksLoadStarted(object sender, EventArgs e)
        {
          //  AddOperation();
        }

        async void service_SubTasksLoaded(object sender, EventArgs e)
        {
            var task = Tasks.FirstOrDefault(x => x.id == (long)sender);
            if (task == null) return;
          //  RemoveOperation();


            var subtasks = await new StorageService().GetAllSubTasks(task.id);
            Dispatcher.RunAsync(() =>
            {
                MapperService.FillSubtasksInfo(task, subtasks);

            });



        }

        private void ServiceOnLoadCompleted(object sender, EventArgs eventArgs)
        {
            Dispatcher.RunAsync(async () =>
            {
                await LoadTasksFromDb();

            });
         //   RemoveOperation();

        }

        public void AddTask()
        {
          //  ExNavigationService.Navigate<AddEditTask>("projectId", Id);
        }

        //public void OnReceive(TaskStatusCompletedMessage message)
        //{
        //    var task = Tasks.FirstOrDefault(x => x.id == message.Id);
        //    if (task == null) return;

        //    task.IsCompleted = message.IsCompleted;





        //}

        void ChangeProject(object o)
        {
            //var popup = new WorkspacesListPopup();
            //popup.ShowProjects(w =>
            //{
            //    if (w.id != Id)
            //    {
            //        ExNavigationService.Navigate<ProjectDetails>("id", w.id);
            //    }
            //}, Project.workspaceid);
        }

        void PinToStart(object sender)
        {
            //if (IsPinned)
            //{
            //    PinService.RemoveTile(PinService.GetProjectDetailsUri(Id));
            //    IsPinned = false;
            //}
            //else
            //{
            //    PinService.CreateUpdateStartTile(Project.name, Project.TasksCountText, PinService.GetProjectDetailsUri(Id), true);
            //    IsPinned = true;
            //}


        }
    }
}
