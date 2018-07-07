using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Linq;
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
using DevRain.Asana.API.Data;
using DevRain.Asana.API.Data.Models;
using DevRain.Asana.API.Services;
using DevRain.Asana.API.Services.API;
using DevRain.Asana.API.Services.Db;
using DevRain.Asana.API.Storage;
using DevRain.Asana.Pages;
using DevRain.Asana.Services;
using DevRain.WP.Core.MVVM.Commands;
using DevRain.WP.Core.Models;
using DevRain.WP.Core.MVVM.State;

using Telerik.Windows.Controls;
using TaskStatus = DevRain.Asana.API.Data.Models.TaskStatus;

namespace DevRain.Asana.ViewModels
{
	public class AddEditTaskViewModel : AsanaViewModel
	{
		public ICommand AddTagCommand { get; set; }

		AddEditTask Page
		{
			get { return RootElement as AddEditTask; }
		}

		public bool IsEditMode
		{
			get { return Id.HasValue; }
		}

		[Tombstoned]
		public AsanaTask Task { get; set; }

		private bool isForSync;
		[Tombstoned]
		public bool IsForSync
		{
			get { return isForSync; }
			set
			{
				if (isForSync != value)
				{
					isForSync = value;

					NotifyOfPropertyChange("IsForSync");
				}
			}
		}

		[Tombstoned]
		public long? ParentId { get; set; }
		[Tombstoned]
		public long WorkspaceId { get; set; }
		[Tombstoned]
		public long? ProjectId { get; set; }
		[Tombstoned]
		public long? Id { get; set; }



		private string appTitle;
		[Tombstoned]
		public string AppTitle
		{
			get { return appTitle; }
			set
			{
				appTitle = value;
				NotifyOfPropertyChange("AppTitle");
			}
		}


		private string name;
		[Tombstoned]
		public string Name
		{
			get { return name; }
			set
			{
				name = value;
				NotifyOfPropertyChange("Name");
			}
		}

		private string notes;
		[Tombstoned]
		public string Notes
		{
			get { return notes; }
			set
			{
				notes = value;
				NotifyOfPropertyChange("Notes");
			}
		}

		private bool isCompleted;
		[Tombstoned]
		public bool IsCompleted
		{
			get { return isCompleted; }
			set
			{
				isCompleted = value;
				NotifyOfPropertyChange("IsCompleted");
			}
		}

		private AsanaProject project;
		[Tombstoned]
		public AsanaProject Project
		{
			get { return project; }
			set
			{
				project = value;
				NotifyOfPropertyChange("Project");
			}
		}

		[Tombstoned]
		public long OldProjectId { get; set; }

		public ExObservableCollection<AsanaProject> Projects { get; set; }
		public ExObservableCollection<AsanaUser> Followers { get; set; }
		public ExObservableCollection<AsanaTag> Tags { get; set; }

		private List<long> OldFollowersIds;
		public List<AsanaUser> SelectedFollowers
		{
			get { return Page.lstFollowers.SelectedItems.Cast<AsanaUser>().ToList(); }
			set
			{
				Page.lstFollowers.SelectedItems.Clear();

				foreach (var asanaUser in value)
				{
					Page.lstFollowers.SelectedItems.Add(asanaUser);
				}


			}
		}



		private List<long> OldTagsIds;
		public List<AsanaTag> SelectedTags
		{
			get { return Page.lstTags.SelectedItems.Cast<AsanaTag>().ToList(); }
			set
			{
				Page.lstTags.SelectedItems.Clear();

				foreach (var tag in value)
				{
					Page.lstTags.SelectedItems.Add(tag);
				}


			}
		}

		private bool canEditProject = true;
		[Tombstoned]
		public bool CanEditProject
		{
			get { return canEditProject; }
			set
			{
				canEditProject = value;
				NotifyOfPropertyChange(() => CanEditProject);
			}
		}

		private bool isUseDueDate;
		[Tombstoned]
		public bool IsUseDueDate
		{
			get { return isUseDueDate; }
			set
			{
				isUseDueDate = value;
				NotifyOfPropertyChange("IsUseDueDate");
			}
		}



		private DateTime dueDate;
		[Tombstoned]
		public DateTime DueDate
		{
			get { return dueDate; }
			set
			{
				dueDate = value;
				NotifyOfPropertyChange("DueDate");
			}
		}

		[Tombstoned]
		public long? UserId { get; set; }
		[Tombstoned]
		public ExObservableCollection<AsanaUser> Users { get; set; }
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
		[Tombstoned]
		public ExObservableCollection<TaskStatus> Statuses { get; set; }
		private TaskStatus status;
		[Tombstoned]
		public TaskStatus Status
		{
			get { return status; }
			set
			{
				status = value;
				NotifyOfPropertyChange("Status");
			}
		}



		public ICommand SaveCommand { get; set; }

		protected override void OnCreate()
		{
			SaveCommand = new RelayCommand(o => SaveTask());
			AddTagCommand = new RelayCommand(async o => await AddTag());

			Projects = new ExObservableCollection<AsanaProject>();
			Users = new ExObservableCollection<AsanaUser>();
			Followers = new ExObservableCollection<AsanaUser>();
			Tags = new ExObservableCollection<AsanaTag>();
			OldTagsIds = new List<long>();
			OldFollowersIds = new List<long>();


			Statuses = new ExObservableCollection<TaskStatus>()
                           {
                               new TaskStatus() {name = EAsanaTaskStatus.inbox.ToString()},
                               new TaskStatus() {name = EAsanaTaskStatus.later.ToString()},
                               new TaskStatus() {name = EAsanaTaskStatus.today.ToString()},
                               new TaskStatus() {name = EAsanaTaskStatus.upcoming.ToString()}
                           };
			IsBusy = true;

		}

		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			if (NavigationManager.GetQueryParameter("workspaceId") != null)
			{
				WorkspaceId = long.Parse(NavigationManager.GetQueryParameter("workspaceId"));
			}

			if (NavigationManager.GetQueryParameter("parentId") != null)
			{
				ParentId = long.Parse(NavigationManager.GetQueryParameter("parentId"));
			}

			if (NavigationManager.GetQueryParameter("projectId") != null)
			{
				ProjectId = long.Parse(NavigationManager.GetQueryParameter("projectId"));
			}


			if (NavigationManager.GetQueryParameter("id") != null)
			{
				Id = long.Parse(NavigationManager.GetQueryParameter("id"));
				PageTitleText = ParentId.HasValue ? "edit subtask" : "edit task";
			}
			else
			{
				PageTitleText = ParentId.HasValue ? "create subtask" : "create task";
			}


		}

		protected override void OnLoad()
		{
			LoadData();
		}

		async Task SetData(AsanaTask task)
		{
			Task = task;
			IsForSync = task.IsForSync;
			Status = Statuses.FirstOrDefault(x => x.name == task.assignee_status);
			Name = task.name;
			Notes = task.notes;
			IsUseDueDate = task.due_on.HasValue;
			DueDate = task.due_on.HasValue ? task.due_on.Value : DateTime.Today;
			IsUseDueDate = task.due_on.HasValue;
			IsCompleted = task.completed;
			ProjectId = task.projectid;
			if (task.assigneeid > 0)
				UserId = task.assigneeid;

			SelectedFollowers = Followers.Where(x => task.GetFollowers().Contains(x.id)).ToList();
			OldFollowersIds = SelectedFollowers.Select(x => x.id).ToList();


			var tags = await new StorageService().GetTagsByTask(task.id);
			OldTagsIds = tags.Select(x => x.id).ToList();
			SelectedTags = Tags.Where(x => OldTagsIds.Contains(x.id)).ToList();


		}

		async void LoadData()
		{

				var storageService = new StorageService();

				AddOperation();

				if (WorkspaceId == 0)
				{
					if (ProjectId.HasValue)
					{
						var project = await storageService.Find<AsanaProject>(ProjectId.Value);
						WorkspaceId = project.workspaceid;
					}
					else if (Id.HasValue)
					{
						var task = await storageService.Find<AsanaTask>(Id.Value);

						if (task == null)
						{
							MessageBox.Show("Task is not found");
							NavigationManager.GoBack();
							return;
						}

						WorkspaceId = task.workspaceid;
					}
					else if (ParentId.HasValue)
					{
                        var task = await storageService.Find<AsanaTask>(ParentId.Value);

						if (task == null)
						{
							MessageBox.Show("Task is not found");
							NavigationManager.GoBack();
							return;
						}

						WorkspaceId = task.workspaceid;
						ProjectId = task.projectid;

						CanEditProject = false;
					}


				}



				var followers = await storageService.GetUsers(WorkspaceId);
				Followers.AddRange(followers);

				AppTitle = (await storageService.Find<AsanaWorkspace>(WorkspaceId)).name;

				var tags = await storageService.GetTags(WorkspaceId);
				Tags.AddRange(tags);

				if (IsEditMode)
				{
					var task = await storageService.Find<AsanaTask>(Id.Value);


					if (task == null)
					{
						MessageBox.Show("Task is not found");
						NavigationManager.GoBack();
						return;
					}

					var r = await CheckSyncStatus(task);

					if (r != null)
					{
						task = r;
					}

					await SetData(task);


					if (task.parentId.HasValue)
					{
						CanEditProject = false;
					}




					//      if (model.CheckInternetConnection(false))
					//      {
					//          var response =
					//await AsanaClient.SendRequest(() => new AsanaRespository().GetTask(model.Id.Value));

					//          if (AsanaClient.ProcessResponse(response))
					//          {
					//              response.Data.projectid = response.Data.projects.Any() ? response.Data.projects.First().id : 0;
					//              response.Data.assigneeid = response.Data.assignee != null ? response.Data.assignee.id : 0;

					//              SetData(response.Data);

					//              using (var db = new DbTransaction())
					//              {
					//                  db.InsertOrUpdate(response.Data);
					//                  db.Commit();
					//              }
					//          }



					//      }




				}
				else
				{
					DueDate = DateTime.Today;

				}

				var projects = await storageService.GetActiveProjects(WorkspaceId);
				Projects.Insert(0, new AsanaProject
								   {
									   id = 0,
									   name = "none",
									   workspaceid = WorkspaceId
								   });
				Projects.AddRange(projects);



				Project = ProjectId.HasValue
							  ? Projects.FirstOrDefault(x => x.id == ProjectId.Value)
							  : Projects.FirstOrDefault(x => x.id > 0);
				if (Project != null)
					OldProjectId = Project.id;

				if (Project == null)
				{
					Project = Projects.FirstOrDefault(x => x.id > 0);
				}



				if (Status == null)
				{
					Status = Statuses.FirstOrDefault(x => x.name == EAsanaTaskStatus.upcoming.ToString());
				}




				var users = await storageService.GetUsers(WorkspaceId);


				Users.Insert(0, new AsanaUser()
								{
									id = -1,
									name = "none"
								});
				Users.AddRange(users);

				User = UserId.HasValue ? Users.FirstOrDefault(x => x.id == UserId.Value) : Users.FirstOrDefault();

				if (User == null)
				{
					User = Users.FirstOrDefault();
				}



				RemoveOperation();
			

		}

		async System.Threading.Tasks.Task<AsanaTask> CheckSyncStatus(AsanaTask task)
		{
			AsanaTask r = null;
			//CHECK FOR ASYNC
			if (CheckInternetConnection(false))
			{
				if (task.IsForSync)
				{
					MessageBox.Show("Task is needed to be synced with Asana", "Warning", MessageBoxButton.OK);

					var syncedTask = await new LoadDataService().SyncTask(task);
					if (syncedTask.Item1 != null)
					{
                        await new StorageService().Delete(task);
                        await new StorageService().Insert(syncedTask.Item1);
                        
                        Id = syncedTask.Item1.id;
                        r = syncedTask.Item1;
                        NavigationManager.SetQueryParamter("id", Id);

					}
					else
					{
						MessageBox.Show("Sync error");
					}
				}
			}
			return r;
		}

		async void SaveTask()
		{


			if (IsBusy || IsBlockingProgress) return;

			if (string.IsNullOrEmpty(Name))
			{
				MessageBox.Show("Specify name");
				return;
			}



			if (Project == null)
			{
				MessageBox.Show("Specify project");
				return;
			}

			IsBlockingProgress = true;

			DateTime? dueDate = null;
			if (IsUseDueDate)
			{
				dueDate = DueDate;
			}

			var dto = MapperService.CreateTaskDTO(Id.GetValueOrDefault(0), Name,
												  Notes,
												  !IsEditMode ? Project.id : 0,
												  Status.name,
												  User != null && User.id > 0 ? User.id : -1,
												  IsCompleted, dueDate, SelectedFollowers.Select(x => x.id).ToList());


			//var task = new AsanaTaskDTO() {
			//    name = model.Name,
			//    notes = model.Notes,
			//    completed = model.IsCompleted, 
			//    id = model.Id.GetValueOrDefault(0),
			//    assignee = -1, 
			//    assignee_status = model.Status.name };


			//if (model.IsUseDueDate)
			//{
			//    task.due_on = model.DueDate.ToString("yyyy-MM-dd");
			//}
			//else
			//{
			//    task.due_on = "null";
			//}

			//if (!model.IsEditMode)
			//{
			//    task.projects = model.Project.id;
			//}
			//if (model.User != null && model.User.id > 0)
			//{
			//    task.assignee = model.User.id;
			//}

			//save task for syncing later
			if (!CheckInternetConnection(false))
			{
				if (IsForSync || !IsEditMode)
				{
					AsanaTask dbTask;
					if (IsEditMode)
					{
						dbTask = Task;
					}
					else
					{

						dbTask = new AsanaTask()
						{
							id = await new StorageService().GetLocalTaskId(),
							workspaceid = Project.workspaceid,
							modified_at = DateTime.Now,
							created_at = DateTime.Now,
							IsForSync = true
						};
					}

					dbTask.name = dto.name;
					dbTask.notes = dto.notes;
					dbTask.completed = dto.completed;
					dbTask.assignee_status = dto.assignee_status;
					dbTask.assigneeid = dto.assignee == AsanaConstants.Utils.NULL_VALUE ? -1 : long.Parse(dto.assignee);
					dbTask.projectid = Project.id;
					dbTask.parentId = ParentId;
					dbTask.SetFollowers(dto.followers.Select(X => X.id).ToList());

					if (IsUseDueDate)
					{
						dbTask.due_on = DueDate;
					}
					else
					{
						dbTask.due_on = null;
					}


                    await new StorageService().Save(dbTask);


					if (!IsEditMode)
					{
						MessageBox.Show("Task saved for syncing when network will be available");
					}

					IsBlockingProgress = false;

					NavigationManager.GoBack();

					return;
				}
			}

			if (IsForSync && IsEditMode)
			{

				var task = await new StorageService().Find<AsanaTask>(Id.Value);

				await CheckSyncStatus(task);
				IsForSync = false;
				IsBlockingProgress = false;
				return;
			}

			//normal save
			var response =  !IsEditMode ? await new AsanaRespository().CreateTask(WorkspaceId, dto, ParentId) : await new AsanaRespository().UpdateTask(dto);

			if (AsanaClient.ProcessResponse(response))
			{

				if (IsEditMode && OldProjectId != Project.id)
				{
                    var newResponse = await new AsanaRespository().ChangeTaskProject(Id.Value, OldProjectId, Project.id);


					if (AsanaClient.ProcessResponse(newResponse))
					{

						await PrepareAndSaveTaskToDb(response.Data);
						await ProcessFollowers(response.Data);
						await ProcessTags(response.Data);
					}

				}
				else
				{
					await PrepareAndSaveTaskToDb(response.Data);
					if (IsEditMode)
					{
						await ProcessFollowers(response.Data);
					}
					await ProcessTags(response.Data);
				}



				NavigationManager.GoBack();
				return;


			}

			IsBlockingProgress = false;



		}

		async System.Threading.Tasks.Task ProcessTags(AsanaTask task)
		{
			var currentTags = SelectedTags.Select(x => x.id).ToList();

			var tagsToAdd = new List<long>();
			var tagsToRemove = new List<long>();

			foreach (var tag in currentTags)
			{
				if (!OldTagsIds.Contains(tag))
				{
					tagsToAdd.Add(tag);
				}
			}

			foreach (var tag in OldTagsIds)
			{
				if (!currentTags.Contains(tag))
				{
					tagsToRemove.Add(tag);
				}
			}

			var taskTags = await new StorageService().GetTaskTagsByTask(task.id);


			var forInsert = new List<AsanaTagTask>();
			var forDelete = new List<AsanaTagTask>();

			foreach (var l in tagsToAdd)
			{
                var response = await new AsanaRespository().AddTag(task.id, l);

				if (AsanaClient.ProcessResponse(response, true))
				{
					var link = new AsanaTagTask() { TagId = l, TaskId = task.id,id = Guid.NewGuid().GetHashCode() };
					forInsert.Add(link);
				}

			}


			foreach (var l in tagsToRemove)
			{
                var response = await new AsanaRespository().RemoveTag(task.id, l);

				if (AsanaClient.ProcessResponse(response, true))
				{
					var link = taskTags.FirstOrDefault(x => x.TagId == l);
					if (link != null)
					{
						forDelete.Add(link);
					}
				}

			}

            foreach (var asanaTagTask in forInsert)
            {
                await new StorageService().Insert(asanaTagTask);
            }
            foreach (var asanaTagTask in forDelete)
            {
                await new StorageService().Delete(asanaTagTask);
            }


		}

		async System.Threading.Tasks.Task ProcessFollowers(AsanaTask task)
		{
			var currentFollowers = SelectedFollowers.Select(x => x.id).ToList();

			var followersToAdd = new List<long>();
			var followersToRemove = new List<long>();

			foreach (var currentFollower in currentFollowers)
			{
				if (!OldFollowersIds.Contains(currentFollower))
				{
					followersToAdd.Add(currentFollower);
				}
			}

			foreach (var oldFollowersId in OldFollowersIds)
			{
				if (!currentFollowers.Contains(oldFollowersId))
				{
					followersToRemove.Add(oldFollowersId);
				}
			}

			var success = false;
			if (followersToAdd.Any())
			{
                var response = await new AsanaRespository().AddFollowers(task.id,
                                                                                                            followersToAdd);

				if (AsanaClient.ProcessResponse(response, true))
				{
					success = true;
				}
				else
				{
					return;
				}
			}


			if (followersToRemove.Any())
			{
                var response = await new AsanaRespository().RemoveFollowers(task.id,
                                                               followersToRemove);

				if (AsanaClient.ProcessResponse(response, true))
				{
					success = true;
				}
				else
				{
					return;
				}
			}

			if (success)
			{
				task.SetFollowers(currentFollowers);
			}

			task.followers = null;
			if (followersToAdd.Any() || followersToRemove.Any())
			{
				await PrepareAndSaveTaskToDb(task);
			}
		}

		async Task PrepareAndSaveTaskToDb(AsanaTask task)
		{
			if (!task.completed && task.due_on.HasValue)
			{
                TileService.UpdateMainTile();
			}


			MapperService.Map(task, Project.id, WorkspaceId);

		    await new StorageService().Save(task);


		}

		async Task AddTag()
		{
			RadInputPrompt.Show("New tag", message: "Enter the name for the new tag", buttons: MessageBoxButtons.OKCancel, closedHandler: async (args) =>
		   {
			   if (args.Result == DialogResult.OK)
			   {

				   await AddTag(args.Text);
			   }
		   });


		}

		async Task AddTag(string name)
		{
			if (String.IsNullOrEmpty(name)) return;

			var tag = new AsanaTag() { name = name };
			IsBusy = true;
            var response = await new AsanaRespository().CreateTag(WorkspaceId, tag);

			if (AsanaClient.ProcessResponse(response))
			{
				response.Data.workspaceid = WorkspaceId;
                await new StorageService().Insert(response.Data);
           
				Tags.Add(response.Data);

				var selectedTags = SelectedTags;
				selectedTags.Add(response.Data);

				SelectedTags = selectedTags;

			}
			IsBusy = false;
		}



	}
}
