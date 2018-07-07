using System;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
using DevRain.WP.Core.Helpers;
using DevRain.WP.Core.MVVM.Commands;
using DevRain.WP.Core.MVVM.Core;
using DevRain.WP.Core.MVVM.Messaging;
using DevRain.WP.Core.MVVM.State;
using System.Threading.Tasks;
using DevRain.WP.Core.Models.Messages;


namespace DevRain.Asana.ViewModels
{
    public class AsanaViewModel : ViewModel
    {
        public AsanaViewModel()
        {
            SettingsCommand = new RelayCommand((o) =>
                                                   {
                                                      ExNavigationService.Navigate<SettingsPage>();

                                                   });

            MainPageCommand = new RelayCommand((o)=>
                                                   {
                                                       ExNavigationService.Navigate<MainPage>();
                                                   });

			AllTasksCommand = new RelayCommand((o) =>
			{
				ExNavigationService.Navigate<AllTasks>();
			});

            AboutCommand = new RelayCommand((o) =>
            {
              ExNavigationService.Navigate<AboutPage>();

            });
            SetDefaultProgressText();
        }


        protected StorageService GetStorageService()
        {
            return new StorageService();
        }

	    private bool isPinned;
		[Tombstoned]
	    public bool IsPinned
	    {
		    get { return isPinned; }
			set { isPinned = value;
				NotifyOfPropertyChange(()=>PinButtonText);
			}
	    }

	    public string PinButtonText
	    {
		    get
		    {
			    return IsPinned ? "unpin from start" : "pin to start";
		    }
	    }

        private bool isAppBarVisible;
		[Tombstoned]
        public bool IsAppBarVisible
        {
            get
            {
                return this.isAppBarVisible;
            }

            set
            {
                if (isAppBarVisible != value)
                {
                    this.isAppBarVisible = value;
                    NotifyOfPropertyChange("IsAppBarVisible");
                }
            }
        }

        private string pageTitleText;
		[Tombstoned]
        public string PageTitleText
        {
            get { return pageTitleText; }
            set
            {
                if (pageTitleText != value)
                {
                    this.pageTitleText = value;
                    NotifyOfPropertyChange("PageTitleText");
                }
            }
        }

		public ICommand AllTasksCommand { get; set; }
        public ICommand MainPageCommand { get; set; }
        public ICommand SettingsCommand { get; set; }
        public ICommand AboutCommand { get; set; }


 


        public void GoToEditTask(long id)
        {
            ExNavigationService.Navigate<AddEditTask>("id", id);
        }

        public async Task UpdateTask(long id, bool isCompleted, DateTime? dueOn, Action<AsanaTask> action)
        {
            if (IsBusy) return;

            ProgressText = "Syncing";
            IsBusy = true;
          
            var task = await new StorageService().Find<AsanaTask>(id);

            if (!dueOn.HasValue)
            {
                dueOn = task.due_on;
            }

            var dto = MapperService.CreateTaskDTO(id, task.name, task.notes, task.projectid, task.assignee_status,
                                          task.assigneeid, isCompleted, dueOn,null);

            var response = await new AsanaRespository().UpdateTask(dto);

            if (AsanaClient.ProcessResponse(response))
            {
                task.due_on = dueOn;
                task.completed = isCompleted;

                await new StorageService().Save(task);

                if (action != null)
                {
                    action(task);
                }

            }
           
            IsBusy = false;
            SetDefaultProgressText();


        }

        public async Task CompleteTask(long id)
        {
            await UpdateTask(id, true, null, (task)=>
                                           {
                                               Container.Resolve<IMessagePublisher>().Publish(new TaskStatusCompletedMessage(this){Id=task.id,IsCompleted = task.IsCompleted});
                                           });
        }

        public async Task UncompleteTask(long id)
        {
            await UpdateTask(id, false, null, (task) =>
            {
                Container.Resolve<IMessagePublisher>().Publish(new TaskStatusCompletedMessage(this) { Id = task.id, IsCompleted = task.IsCompleted });
            });
        }

        protected void FillTaskCommands(AsanaTask asanaTask)
        {
            asanaTask.EditTaskCommand = new RelayCommand(o => GoToEditTask((long)o));
            asanaTask.CompleteTaskCommand = new RelayCommand(async o => await CompleteTask((long)o));
            asanaTask.UncompleteTaskCommand = new RelayCommand(async o => await UncompleteTask((long)o));
        }
    }
}
