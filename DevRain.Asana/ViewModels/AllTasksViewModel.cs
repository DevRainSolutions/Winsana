using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevRain.Asana.API.Data.Models;
using DevRain.Asana.API.Services.Db;
using DevRain.Asana.API.Storage;
using DevRain.Asana.Pages;
using DevRain.WP.Common.Helpers;
using DevRain.WP.Core.Extensions;
using DevRain.WP.Core.Helpers;
using DevRain.WP.Core.MVVM.State;

using Telerik.Windows.Data;

namespace DevRain.Asana.ViewModels
{

    public class AsanaWorkspaceForDisplay:AsanaWorkspace
    {
        public List<DataDescriptor> DataDescriptors { get; set; }
    }

	public class AllTasksViewModel : AsanaViewModel
	{
        public List<AsanaWorkspaceForDisplay> WorkspacesList { get; set; }

		protected override void OnCreate()
		{
            WorkspacesList = new List<AsanaWorkspaceForDisplay>();
		}

		protected override async void OnLoad()
		{
			if (NavigationEventArgs.IsBack())
			{
				NotifyOfPropertyChange(() => WorkspacesList);
				return;
			}


			IsBusy = true;
            await LoadData();

		}

		private async Task LoadData()
		{
			GenericGroupDescriptor<AsanaTask, string> descriptor =
				new GenericGroupDescriptor<AsanaTask, string>(movie => movie.JumpListProjectName);

			Dictionary<long, AsanaProject> projects = new Dictionary<long, AsanaProject>();
			projects.Add(0,new AsanaProject(){name = "Unknown"});

		    var service = new StorageService();
		
				var workspaces = await service.GetWorkspaces();

				foreach (var w in workspaces)
				{

				    var wd = new AsanaWorkspaceForDisplay() {name = w.name};

					wd.Tasks = new List<AsanaTask>();
					wd.DataDescriptors = new List<DataDescriptor>();
					wd.DataDescriptors.Add(descriptor);

					var tasks = await service.GetActiveWorkspacesTasks(w.id);

					foreach (var asanaTask in tasks)
					{
						AsanaProject project;

						if(projects.ContainsKey(asanaTask.projectid))
						{
							project = projects[asanaTask.projectid];
						}
						else
						{
							var pr = await service.Find<AsanaProject>(asanaTask.projectid);
							project = pr;
							projects.Add(pr.id,pr);
						}
						
						asanaTask.JumpListProjectName = project.name;
						wd.Tasks.Add(asanaTask);
					}

					WorkspacesList.Add(wd);

				

			}


			DispatcherHelper.OnUi(()=>
				                      {
					                      (RootElement as AllTasks).pivot.ItemsSource = WorkspacesList;
					                      IsBusy = false;
				                      });
		}
	}
}
