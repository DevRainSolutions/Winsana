using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using DevRain.Asana.API;
using DevRain.Asana.API.Data;
using DevRain.Asana.API.Data.Models;
using DevRain.Asana.API.Data.Models.DTO;
using DevRain.Asana.API.Services;
using DevRain.Asana.API.Services.API;
using DevRain.Asana.API.Services.Db;
using DevRain.Asana.Models;
using DevRain.Asana.Pages;
using DevRain.Asana.Services;
using DevRain.WP.Common.Extensions;
using DevRain.WP.Common.Helpers;
using DevRain.WP.Common.Models;
using DevRain.WP.Core.Extensions;
using DevRain.WP.Core.Helpers;
using DevRain.WP.Core.MVVM.Commands;
using DevRain.WP.Core.MVVM.Messaging;
using DevRain.WP.Core.Models;
using DevRain.WP.Core.Models.Messages;
using System.Threading.Tasks;

using DevRain.WP.Core.MVVM.State;

namespace DevRain.Asana.ViewModels
{
    public class TaskDetailsViewModel : AsanaViewModel, IMessageReceiver<TaskStatusCompletedMessage>
    {
		[Tombstoned]
        public long Id { get; set; }
        public ICommand SendCommentCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand ShareCommand { get; set; }
        public ICommand SetForTodayCommand { get; set; }
        public ICommand SetForTomorrowCommand { get; set; }



        private string newSubTaskText;
		[Tombstoned]
        public string NewSubTaskText
        {
            get { return newSubTaskText; }
            set
            {
                newSubTaskText = value;
                NotifyOfPropertyChange("NewSubTaskText");
            }
        }

        private string commentText;
		[Tombstoned]
        public string CommentText
        {
            get { return commentText; }
            set
            {
                commentText = value;
                NotifyOfPropertyChange("CommentText");
            }
        }


        private TaskInfo taskInfo;
		[Tombstoned]
        public TaskInfo Task
        {
            get { return taskInfo; }
            set { 
                taskInfo = value;
                NotifyOfPropertyChange("Task");
                NotifyOfPropertyChange(() => CanNotCompleteTask);
                NotifyOfPropertyChange(() => CanCompleteTask);
            }
        }

         private bool isCommentsInfoActive;
		[Tombstoned]
         public bool IsCommentsInfoActive
        {
            get { return isCommentsInfoActive; }
            set { isCommentsInfoActive = value;
            NotifyOfPropertyChange(()=>IsCommentsInfoActive);
            }
        }

        private bool isTaskInfoActive;
		[Tombstoned]
        public bool IsTaskInfoActive
        {
            get { return isTaskInfoActive; }
            set { 
                
                isTaskInfoActive = value;
                NotifyOfPropertyChange(()=>IsTaskInfoActive);
                NotifyOfPropertyChange(() => CanNotCompleteTask);
                NotifyOfPropertyChange(() => CanCompleteTask);
            }
        }

        private bool isSubtasksActive;
		[Tombstoned]
        public bool IsSubtasksActive
        {
            get { return isSubtasksActive; }
            set
            {
                isSubtasksActive = value;
                NotifyOfPropertyChange(() => IsSubtasksActive);
            }
        }

        public bool CanNotCompleteTask
        {
            get
            {
                if (!IsTaskInfoActive) return false;
                if (taskInfo == null) return false;
                return taskInfo.IsCompleted;
            }
        }


        public bool CanCompleteTask
        {
            get
            {
                if (!IsTaskInfoActive) return false;
                if (taskInfo == null) return false;
                return !taskInfo.IsCompleted;
            }
        }

		[Tombstoned]
        public ExObservableCollection<AsanaStory> Comments { get; set; }
		[Tombstoned]
        public ExObservableCollection<AsanaStory> Histories { get; set; }
		[Tombstoned]
        public ExObservableCollection<AsanaTask> Subtasks { get; set; }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            Container.Resolve<IMessagePublisher>().Register(this);
            var id = long.Parse(NavigationManager.GetQueryParameter("id"));
            Id = id;
           
        }

        protected override void OnCreate()
        {
            Comments = new ExObservableCollection<AsanaStory>();
            Subtasks = new ExObservableCollection<AsanaTask>();
            Histories = new ExObservableCollection<AsanaStory>();
            SendCommentCommand = new RelayCommand(o=>SendSomment());
            RefreshCommand = new RelayCommand(async o => await RefreshData());
            ShareCommand = new RelayCommand(o=>Share());
            SetForTodayCommand = new RelayCommand(async o=>
                                                      {
                                                          await UpdateTask(false, DateTime.Today);
                                                      });
            SetForTomorrowCommand = new RelayCommand(async o =>
            {
                await UpdateTask(false, DateTime.Today.AddDays(1));
            });
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Container.Resolve<IMessagePublisher>().Unregister(this);
        }



        protected override async void OnLoad()
        {
	        if (NavigationEventArgs.IsBackOrReset())
	        {
				await LoadOnBack();
		        return;
	        }


            Sync();
        }

	    async Task LoadOnBack()
	    {
			IsBusy = true;
			var task = await new StorageService().Find<AsanaTask>(Id);
			await SetTaskInfo(task);
			
			await LoadSubtasksFromDb();

			await LoadStoriesFromDb();


			Container.Resolve<IMessagePublisher>().Publish(new RefreshApplicationBarMessage(this));
			IsBusy = false;
	    }

        async void Sync()
        {
            IsBusy = true;
            var task = await new StorageService().Find<AsanaTask>(Id);

            if (task == null)
            {
                MessageBox.Show("Task is not found");
                NavigationManager.GoBack();
                return;
            }

            if (CheckInternetConnection(false))
            {
                if (task.IsForSync)
                {
                    MessageBox.Show("Task is needed to be synced with Asana", "Warning", MessageBoxButton.OK);

                    var syncedTask = await new LoadDataService().SyncTask(task);
                    if (syncedTask.Item1 != null)
                    {
                        await GetStorageService().Delete(task);
                        await GetStorageService().Insert(syncedTask.Item1);
                        
                        Id = syncedTask.Item1.id;
                        NavigationManager.SetQueryParamter("id", Id);

                    }
                    else
                    {
                        MessageBox.Show("Sync error");
                    }
                }
            }

            await LoadData(task);
        }


        async Task LoadData(AsanaTask task)
        {

            await SetTaskInfo(task);

            
            await SetTags();

await             LoadTaskTags(true);
            await LoadStories(false);
            await LoadSubtasks();

            IsBusy = false;

            if (IsFromCache)
            {
                Container.Resolve<IMessagePublisher>().Publish(new RefreshApplicationBarMessage(this));
            }
        }

        async Task SetTaskInfo(AsanaTask task)
        {
            var info = new TaskInfo();

            info.Name = task.DisplayName;
            info.Notes = task.notes;
            info.IsCompleted = task.completed;

            var project = await new StorageService().Find<AsanaProject>(task.projectid);

	        string projectName = "";
            if (project != null)
            {
                projectName = project.name;
            }
            else
            {
				projectName = "Unknown";
            }
	        info.ProjectName = string.Format(@"{0} / {1}", projectName, (await new StorageService().Find<AsanaWorkspace>(task.workspaceid)).name);
            if (task.assigneeid > 0)
            {
                info.HasAssignee = true;
                var assignee = await new StorageService().Find<AsanaUser>(task.assigneeid);
                info.AssigneeName = assignee != null ? assignee.name : "Unknown";
            }
            if (task.due_on.HasValue)
            {
                info.DueDate = task.DueOn;
                info.HasDueDate = true;
            }

            info.HasStatus = !string.IsNullOrEmpty(task.assignee_status);
            info.Status = task.assignee_status;
            info.IsForSync = task.IsForSync;
            info.IsOverdue = task.IsOverDue;

            if(task.parentId != null)
            {
                var parent = await new StorageService().Find<AsanaTask>(task.parentId.Value);
                if(parent != null)
                {
                    info.HasParent = true;
                    info.ParentName = parent.DisplayName;
                }
            }

            var followersId = task.GetFollowers();
            info.HasFollowers = followersId.Any();
            info.Followers = new List<AsanaUser>();

            foreach (var l in followersId)
            {
                var user = await new StorageService().Find<AsanaUser>(l);

                if (user != null)
                {
                    info.Followers.Add(user);
                }
                else
                {
                    info.Followers.Add(new AsanaUser() { name = "Unknown" });
                }
            }

            info.NotifyFollowers();

            Task = info;
        }


        async Task SetTags()
        {
            Task.Tags = await new StorageService().GetTagsByTask(Id);
            Task.NotifyTags();
        }

        async Task RefreshData()
        {
            if (IsBusy) return;

            if (!CheckInternetConnection(true)) return;

            IsBusy = true;
            var response = await new AsanaRespository().GetTask(Id);

            if (AsanaClient.ProcessResponse(response, false, async r =>
            {
                return await ProcessNotFoundTask (r);
            }))
            {

                var task = await new StorageService().Find<AsanaTask>(Id);
                MapperService.Map(response.Data, task.workspaceid);

                await GetStorageService().Save(response.Data);


                DispatcherHelper.OnUi(async () =>
                {
                   await SetTaskInfo(response.Data);

                });


                await LoadStories(false);
                await LoadSubtasks(false);
                await LoadTaskTags(false);


            }

            DispatcherHelper.OnUi(() =>
            {
                IsBusy = false;
            });
        }


        async Task LoadSubtasks(bool isUserAction = false)
        {
            await LoadSubtasksFromDb();



                      if (!CheckInternetConnection(false))
                      {
                          return;
                      }

                      AddOperation();

                      await
                           new LoadDataService(isUserAction).LoadSubtasks(Id, false);


                      RemoveOperation();

                      await LoadSubtasksFromDb();
        }

        async Task LoadSubtasksFromDb()
        {

           var subtasks = await new StorageService().GetSubTasks(Id);

                                                                 foreach (var asanaTask in subtasks)
                                                                 {
                                                                     asanaTask.CannotEdit = true;
                                                                     FillTaskCommands(asanaTask);
                                                                     asanaTask.EditTaskCommand = new RelayCommand(x=>EditSubtask((long)x));
                                                                 }

                                                                 DispatcherHelper.OnUi(() =>
                                                                                           {
                                                                                               Subtasks.Clear();
                                                                                               Subtasks.AddRange(subtasks);
                                                                                           });

        }

         async Task<bool> ProcessNotFoundTask(AsanaResponse<AsanaTask> r)
        {
              var errors = r.Errors;

                    if (errors == null || !errors.IsUnknownObject) return false;

                    var res = MessageBox.Show("Task was not found - seems like it was deleted in Asana service. Delete it?", "Warning", MessageBoxButton.OKCancel);

                    if (res == MessageBoxResult.OK)
                    {
                        var task = await  GetStorageService().Find<AsanaTask>(Id);

                        await new LoadDataService().DeleteTask(task);
                        NavigationManager.GoBack();
                    }

                    return true;
        }

        async Task LoadTaskTags(bool isUserAction = false)
        {
            if (CheckInternetConnection(false))
            {
                var response = await new AsanaRespository().GetTask(Id);

                if (AsanaClient.ProcessResponse(response, !isUserAction, async r =>
                                                                             {

                                                                                 return await ProcessNotFoundTask(r);



                                                                             }))
                {

                    var dbTags = await new StorageService().GetTaskTagsByTask(Id);



                    if (response.Data.tags != null)
                    {
                        foreach (var tag in response.Data.tags)
                        {
                            var dbTag = dbTags.FirstOrDefault(x => x.TagId == tag.id);

                            if (dbTag != null)
                            {
                                dbTag.IsFoundInDb = true;
                            }
                            else
                            {
                                await GetStorageService().Insert(new AsanaTagTask() { TagId = tag.id, TaskId = Id, id = Guid.NewGuid().GetHashCode() });
                            }
                        }
                    }


                    foreach (var asanaTagTask in dbTags.Where(x => !x.IsFoundInDb))
                    {
                        await GetStorageService().Delete(asanaTagTask);
                    }



                    DispatcherHelper.OnUi(async () =>
                    {
                        await SetTags();
                    });

       
                }



            }
        }

        async System.Threading.Tasks.Task LoadStories(bool isUserAction = false)
        {




            if (CheckInternetConnection(false))
            {
                AddOperation();
                var stories =
                    await
                        new AsanaRespository().GetStoriesByTask(Id);

                if (AsanaClient.ProcessResponse(stories, !isUserAction))
                {


                    var dbStories = await GetStorageService().GetStories(Id);
                    foreach (var story in stories.Data)
                    {
                        var dbStory =
                            dbStories.FirstOrDefault(
                                x => x.id == story.id);
                        if (dbStory != null)
                        {
                            dbStory.IsFoundInDb = true;
                        }


                        story.userId = story.created_by.id;
                        story.targetId = Id;
                        await GetStorageService().Save(story);

                    }

                    foreach (var story in dbStories.Where(x => !x.IsFoundInDb))
                    {
                        await GetStorageService().Delete(story);
                    }

                }


                await LoadStoriesFromDb();


            }

            RemoveOperation();


        }

	    async Task LoadStoriesFromDb()
	    {
		                          var allStories = await new StorageService().GetStories(Id);

			                                                        DispatcherHelper.OnUi(() =>
			                                                                              {
				                                                                              Comments.Clear();
				                                                                              Histories.Clear();


				                                                                              Comments.AddRange(
					                                                                              allStories.Where(x => x.IsComment));
				                                                                              Histories.AddRange(
					                                                                              allStories.Where(x => !x.IsComment));


			                                                                              });
	    }

        public async Task UpdateTask(bool isCompleted, DateTime? dueOn)
        {


            await UpdateTask(Id, isCompleted, dueOn, (task) =>
                                                         {

                                                             Task.IsCompleted = isCompleted;
                                                             Task.DueDate = task.DueOn;
                                                             Task.IsOverdue = task.IsOverDue;
                                                             TileService.UpdateMainTile();

                                                             NotifyOfPropertyChange(() => CanNotCompleteTask);
                                                             NotifyOfPropertyChange(() => CanCompleteTask);

                                                             Container.Resolve<IMessagePublisher>().Publish(new RefreshApplicationBarMessage(this));

                                                             Container.Resolve<IMessagePublisher>().Publish(new GoToFirstPivotItemMessage(this));
                                                         });
      


        }

        public async System.Threading.Tasks.Task SetCompleted(bool isCompleted)
        {
            await UpdateTask(isCompleted,null);


        }

        async void SendSomment()
        {
            if (String.IsNullOrEmpty(CommentText)) return;

            if (IsBusy) return;

            IsBusy = true;
            ProgressText = "Processing";


            var response = await new AsanaRespository().CreateTaskStory(Id, new AsanaStory() { text = CommentText });

            if (AsanaClient.ProcessResponse(response))
            {

                response.Data.userId = response.Data.created_by.id;
                response.Data.targetId = Id;
                await GetStorageService().Insert(response.Data);

                Comments.Insert(0,response.Data);

                Container.Resolve<IMessagePublisher>().Publish(new FocusListMessage(this));
                CommentText = "";
            }
            IsBusy = false;
            SetDefaultProgressText();


        }


        void Share()
        {
            var name = string.Format("Task from Asana: {0}", Task.Name);
            if(Task.HasDueDate)
            {
                name += ", due to " + Task.DueDate;
            }

	        var notes = new StringBuilder();
	        notes.AppendLine("Status: {0}".FormatWith(Task.IsCompleted  ? "completed" :"not completed"));
			notes.AppendLine("Project/Workspace: {0}".FormatWith(Task.ProjectName));
			if (Task.HasParent)
			{
				notes.AppendLine("Parent: {0}".FormatWith(Task.ParentName));
			}
			if(Task.HasAssignee)
			{
				notes.AppendLine("Assignee: {0}".FormatWith(Task.AssigneeName));
			}
			if (Task.HasTags)
			{
				notes.AppendLine("Tags: {0}".FormatWith(Task.TagsText));
			}
			if(Task.HasFollowers)
			{
				notes.AppendLine("Followers: {0}".FormatWith(Task.FollowersText));
			}
			if(Task.HasNotes)
			{
				notes.AppendLine("");
				notes.AppendLine(Task.Notes);
			}


            Tasks.ShowEmailComposeTask("",name,notes.ToString());
        }

        public void AddSubtask()
        {
            if(Task.IsForSync)
            {
                MessageBox.Show("Can't add subtask to unsynced task");
                return;
            }


            ExNavigationService.Navigate<AddEditTask>("parentId", Id);
        }

           public void EditSubtask(long id)
        {
            ExNavigationService.Navigate<AddEditTask>("id", id, "parentId", Id);
        }

        public void OnReceive(TaskStatusCompletedMessage message)
        {
            var task = Subtasks.FirstOrDefault(x => x.id == message.Id);
            if (task == null) return;

            task.IsCompleted = message.IsCompleted;

        }

        public async void AddNewSubTask()
        {
            if (String.IsNullOrEmpty(NewSubTaskText)) return;

            if (!CheckInternetConnection(true)) return;

            IsBusy = true;
            var task = await new StorageService().Find<AsanaTask>(Id);

            

            var dto = MapperService.CreateTaskDTO(0, NewSubTaskText, "", 0, "later", 0, false, null, null);

            var response = await new AsanaRespository().CreateTask(task.workspaceid, dto, Id);

             if (AsanaClient.ProcessResponse(response))
             {
                 MapperService.Map(response.Data, task.projectid, task.workspaceid);
                 await new StorageService().Insert(response.Data);


                 Subtasks.Add(response.Data);
                 NewSubTaskText = "";

                 Container.Resolve<IMessagePublisher>().Publish(new FocusListMessage(this){IsSubtasks = true});

             }

            IsBusy = false;

        }
    }



    public class TaskInfo : BaseObject
    {
        public string Name { get; set; }
        public string Notes { get; set; }
        public string ProjectName { get; set; }
        public string AssigneeName { get; set; }

        public string ParentName { get; set; }

        public bool HasParent { get; set; }

        private string dueDate;
        public string DueDate
        {
            get { return dueDate; }
            set
            {
                if(dueDate != value)
                {
                    dueDate = value;
                    NotifyOfPropertyChange(()=>DueDate);

                    HasDueDate = !string.IsNullOrEmpty(dueDate);
                    NotifyOfPropertyChange(()=>HasDueDate);

                    
                }
            }
        }
        public string Status { get; set; }

        private bool isOverDue;
        public bool IsOverdue
        {
            get { return isOverDue; }
            set
            {
                if(value != isOverDue)
                {
                    isOverDue = value;
                    NotifyOfPropertyChange(()=>IsOverdue);
                }
            }
        }

        private bool isCompleted;
        public bool IsCompleted
        {
            get { return isCompleted; }
            set
            {
                isCompleted = value;
                NotifyOfPropertyChange(() => IsCompleted);
            }
        }


        public List<AsanaTag> Tags { get; set; }
        public string TagsText
        {
            get
            {
                if (Tags == null) return "";
                return string.Join(" | ", Tags.Select(c => c.name)).Trim();
            }
        }

        public bool HasTags
        {
            get
            {
                if (Tags == null) return false;
                return Tags.Any();
            }
        }

        public void NotifyTags()
        {
            NotifyOfPropertyChange("TagsText");
            NotifyOfPropertyChange("HasTags");
        }

        public bool IsForSync { get; set; }
        public bool HasStatus { get; set; }

        public bool HasDueDate { get; set; }
        public bool HasAssignee { get; set; }

        public bool HasNotes
        {
            get { return !string.IsNullOrEmpty(Notes); }
        }

        public bool HasFollowers { get; set; }

        public List<AsanaUser> Followers { get; set; }


        public string FollowersText
        {
            get
            {
                if (Followers == null) return "";
                return string.Join(" | ", Followers.Select(c => c.name)).Trim();
            }
        }

        public void NotifyFollowers()
        {
            NotifyOfPropertyChange("FollowersText");
            NotifyOfPropertyChange("HasFollowers");
        }

    }
}
