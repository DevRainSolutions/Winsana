using System;
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
using DevRain.Asana.API.Data.Models;
using DevRain.Asana.API.Services.API;
using DevRain.Asana.API.Services.Db;
using DevRain.Asana.Controls;
using DevRain.Asana.Pages;
using DevRain.Asana.Services;
using DevRain.WP.Common.Helpers;
using DevRain.WP.Core.Extensions;
using DevRain.WP.Core.Helpers;
using DevRain.WP.Core.MVVM.Commands;
using DevRain.WP.Core.Models;
using System.Threading.Tasks;

using DevRain.WP.Core.MVVM.State;

using Microsoft.Phone.Controls;

namespace DevRain.Asana.ViewModels
{
	public class WorkspaceDetailsViewModel : AsanaViewModel
	{
		public ICommand PinToStartCommand { get; set; }
		public ICommand WorkspacesCommand { get; set; }
		public ICommand RefreshCommand { get; set; }

		[Tombstoned]
		public long Id { get; set; }

		[Tombstoned]
		public ExObservableCollection<AsanaProject> Projects { get; set; }
		//public ExtendedObservableCollection<AsanaProject> ArchiveProjects { get; set; }

		[Tombstoned]
		public ExObservableCollection<AsanaTag> Tags { get; set; }


		private AsanaWorkspace _workspace;
		[Tombstoned]
		public AsanaWorkspace Workspace
		{
			get { return _workspace; }
			set
			{
				_workspace = value;
				NotifyOfPropertyChange("Workspace");
			}
		}

		protected override void OnCreate()
		{
			Projects = new ExObservableCollection<AsanaProject>();
			// ArchiveProjects = new ExtendedObservableCollection<AsanaProject>();
			Tags = new ExObservableCollection<AsanaTag>();
			RefreshCommand = new RelayCommand<object>(async (o)=>await this.Refresh(null));
			WorkspacesCommand = new RelayCommand<object>(ChangeWorkspace);
			PinToStartCommand = new RelayCommand(PinToStart);

		}

		protected override async void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			this.Id = long.Parse(NavigationManager.GetQueryParameter("id"));
			var workspace = await new StorageService().Find<AsanaWorkspace>(Id);

			Workspace = workspace;
			IsPinned = PinService.Exists(PinService.GetWorkspaceDetailsUri(Id));

		}

		protected override async void OnLoad()
		{
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


            DispatcherHelper.OnUi(() =>
            {
                //model.ArchiveProjects.Clear();
                Projects.Clear();
                Projects.AddRange(projects);
                Workspace.ProjectsCount = projects.Count(x => !x.archived);

                if (IsPinned)
                {
                    PinService.CreateUpdateStartTileAsync(Workspace.name, Workspace.ProjectsCountText, PinService.GetWorkspaceDetailsUri(Id), false);
                }

                // model.ArchiveProjects.AddRange(projects.Where(x => x.archived));
            });

		}

		async Task<MyTuple<int, int>> SetProjectInfo(AsanaProject project)
		{
            //TODO: move to class
			int count = await GetStorageService().GetAsyncConnection().Table<AsanaTask>().Where(x => x.projectid == project.id && x.completed ==false && x.parentId == null && x.IsPriorityHeading == false).CountAsync();
            int overdue = await GetStorageService().GetAsyncConnection().Table<AsanaTask>().Where(x => x.projectid == project.id && x.completed == false && x.parentId == null && x.due_on != null && x.due_on <= DateTime.Today && x.IsPriorityHeading == false).CountAsync();

            return new MyTuple<int, int>(count, overdue);
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
            DispatcherHelper.OnUi(() =>
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

			if (NavigationEventArgs.IsBackOrReset())
			{
				NotifyOfPropertyChange(()=>Tags);
				NotifyOfPropertyChange(() => Projects);
				return;
			}

			await LoadProjects(isUserAction);
			await LoadTags(isUserAction);


			await OffineSyncTaskService.Execute(this, () => { });


		}

		async Task LoadTags(bool isUserAction = false)
		{

			if (!CheckInternetConnection(false)) return;

			AddOperation();

			await new LoadDataService(isUserAction).LoadWorkspaceTags(Id, false);

			await LoadTagsFromDb();
			RemoveOperation();
		}

		async Task LoadProjects(bool isUserAction = false)
		{

			if (!CheckInternetConnection(false)) return;

			AddOperation();


			await new LoadDataService(isUserAction).LoadWorkspaceProjects(Id, false);
			await LoadProjectsFromDb();
			RemoveOperation();

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

                        DispatcherHelper.OnUi(() =>
                        {
                            project.TasksCount = tasksCount.Item1;
                            project.OverdueTasksCount = tasksCount.Item2;
                        });
                    
                    }
				}

			}
		}


		private async Task Refresh(object sender)
		{

			if (IsBusy) return;
			if (!CheckInternetConnection(true)) return;

			IsBusy = true;
			await LoadData(true);
		}


		void ChangeWorkspace(object sender)
		{
			var popup = new WorkspacesListPopup();
			popup.ShowWorkspaces(w =>
						   {
							   if (w.id != Id)
							   {
								   ExNavigationService.Navigate<WorkspaceDetails>("id", w.id);
							   }
						   });
		}

		void PinToStart(object sender)
		{
			if (IsPinned)
			{
				PinService.RemoveTile(PinService.GetWorkspaceDetailsUri(Id));
				IsPinned = false;
			}
			else
			{
				PinService.CreateUpdateStartTile(Workspace.name, Workspace.ProjectsCountText, PinService.GetWorkspaceDetailsUri(Id), true);
				IsPinned = true;
			}


		}

	}
}
