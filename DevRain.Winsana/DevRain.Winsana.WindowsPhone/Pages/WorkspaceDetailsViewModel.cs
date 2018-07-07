using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Navigation;
using DevRain.Asana.API.Data.Models;
using DevRain.Asana.API.Services.API;
using DevRain.Asana.API.Services.Db;
using DevRain.Windows.WinRT.Common.Models;
using DevRain.Windows.WinRT.Common.MVVM;
using DevRain.Winsana.Models;

namespace DevRain.Winsana.Pages
{
    public class WorkspaceDetailsViewModel : WPAsanaViewModel
    {
        public ICommand PinToStartCommand { get; set; }

        public ICommand RefreshCommand { get; set; }

        public ICommand AddTaskCommand { get; set; }

        public ICommand SelectWorkspaceCommand { get; set; }

        public ICommand AddProjectCommand { get; set; }

        public long Id { get; set; }

        public ExObservableCollection<AsanaProject> Projects { get; set; }
        
        public ExObservableCollection<AsanaTag> Tags { get; set; }


        private AsanaWorkspace _workspace;
        public AsanaWorkspace Workspace
        {
            get { return _workspace; }
            set { SetValue(ref _workspace, value); }
        }

        protected override void OnCreate()
        {
            Projects = new ExObservableCollection<AsanaProject>();
            
            Tags = new ExObservableCollection<AsanaTag>();

            RefreshCommand = new DelegateCommand(async (o) => await this.Refresh(null));
            PinToStartCommand = new DelegateCommand(PinToStart);
            AddTaskCommand = new DelegateCommand(AddTask);
            SelectWorkspaceCommand = new DelegateCommand(SelectWorkspaces);

            AddProjectCommand = new DelegateCommand(AddProject);

        }

        protected override async void OnNavigatedTo()
        {
            this.Id = (long)NavigationEventArgs.Parameter;
            var workspace = await new StorageService().Find<AsanaWorkspace>(Id);

            Workspace = workspace;
            //IsPinned = PinService.Exists(PinService.GetWorkspaceDetailsUri(Id));

            await LoadData();

        }


        async Task LoadProjectsFromDb()
        {

            var projects = await GetStorageService().GetProjects(Id);

            foreach (var asanaProject in projects)
            {
                var tasksCount = await SetProjectInfo(asanaProject);
                asanaProject.TasksCount = tasksCount.Item1;
                asanaProject.OverdueTasksCount = tasksCount.Item2;
            }


            Dispatcher.RunAsync(() =>
            {
                Projects.Clear();
                Projects.AddRange(projects);
                Workspace.ProjectsCount = projects.Count(x => !x.archived);

                if (IsPinned)
                {
                    //PinService.CreateUpdateStartTileAsync(Workspace.name, Workspace.ProjectsCountText, PinService.GetWorkspaceDetailsUri(Id), false);
                }

            });

        }

        async Task<Tuple<int, int>> SetProjectInfo(AsanaProject project)
        {
            //TODO: move to class
            int count = await GetStorageService().GetAsyncConnection().Table<AsanaTask>().Where(x => x.projectid == project.id && x.completed == false && x.parentId == null && x.IsPriorityHeading == false).CountAsync();
            int overdue = await GetStorageService().GetAsyncConnection().Table<AsanaTask>().Where(x => x.projectid == project.id && x.completed == false && x.parentId == null && x.due_on != null && x.due_on <= DateTime.Today && x.IsPriorityHeading == false).CountAsync();

            return new Tuple<int, int>(count, overdue);
        }



        async Task LoadTagsFromDb()
        {

            var tags = await GetStorageService().GetTags(Id);

            foreach (var tag in tags)
            {
                tag.TasksCount =
                    (await GetStorageService().GetAsyncConnection().Table<AsanaTagTask>().Where(t => t.TagId == tag.id).ToListAsync())
                    .Join(await GetStorageService().GetAsyncConnection().Table<AsanaTask>().Where(x => x.completed == false).ToListAsync(),
                                                                    x => x.TaskId,
                                                                    x => x.id, (x, y) => y).Count();
            }
            Dispatcher.RunAsync(() =>
            {
                Tags.Clear();
                Tags.AddRange(tags);
            });



        }


        async Task LoadData(bool isUserAction = false)
        {
            IsBusy = true;

            await LoadProjectsFromDb();
            await LoadTagsFromDb();
            IsBusy = false;

            //if (NavigationEventArgs.IsBackOrReset())
            //{
            //    NotifyOfPropertyChange(() => Tags);
            //    NotifyOfPropertyChange(() => Projects);
            //    return;
            //}

            await LoadProjects(isUserAction);
            await LoadTags(isUserAction);


          //  await OffineSyncTaskService.Execute(this, () => { });


        }

        async Task LoadTags(bool isUserAction = false)
        {

            if (!await CheckInternetConnection(false)) return;

            //AddOperation();

            await new LoadDataService(isUserAction).LoadWorkspaceTags(Id, false);

            await LoadTagsFromDb();
            ///RemoveOperation();
        }

        async Task LoadProjects(bool isUserAction = false)
        {

            if (! await CheckInternetConnection(false)) return;

         //   AddOperation();


            await new LoadDataService(isUserAction).LoadWorkspaceProjects(Id, false);
            await LoadProjectsFromDb();
            //RemoveOperation();

            if (NavigationEventArgs.NavigationMode != NavigationMode.Back || isUserAction)
            {
                var projects = Projects;

                var dataService = new LoadDataService(isUserAction);

                foreach (var asanaProject in projects)
                {

                    await dataService.LoadProjectTasks(Id, asanaProject.id, false);

                    var id = asanaProject.id;

                    var project = Projects.FirstOrDefault(x => x.id == id);


                    if (project != null)
                    {

                        var tasksCount = await SetProjectInfo(project);

                        Dispatcher.RunAsync(() =>
                        {
                            project.TasksCount = tasksCount.Item1;
                            project.OverdueTasksCount = tasksCount.Item2;
                        });

                    }
                }

            }
        }

        async void AddProject()
        {
//             NavigationService.Navigate<AddEditProject>("workspaceId", model.Id);
        }

        async void SelectWorkspaces()
        {
            //var popup = new WorkspacesListPopup();
            //popup.ShowWorkspaces(w =>
            //{
            //    if (w.id != Id)
            //    {
            //        ExNavigationService.Navigate<WorkspaceDetails>("id", w.id);
            //    }
            //});
        }

        async void AddTask()
        {
           // NavigationService.Navigate<AddEditTask>("workspaceId", Id);
            
        }
        private async Task Refresh(object sender)
        {

            if (IsBusy) return;
            if (!await CheckInternetConnection(true)) return;

            IsBusy = true;
            await LoadData(true);
        }




        void PinToStart(object sender)
        {
            //if (IsPinned)
            //{
            //    PinService.RemoveTile(PinService.GetWorkspaceDetailsUri(Id));
            //    IsPinned = false;
            //}
            //else
            //{
            //    PinService.CreateUpdateStartTile(Workspace.name, Workspace.ProjectsCountText, PinService.GetWorkspaceDetailsUri(Id), true);
            //    IsPinned = true;
            //}


        }
    }
}
