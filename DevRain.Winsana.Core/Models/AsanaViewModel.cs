using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DevRain.Asana.API.Data;
using DevRain.Asana.API.Data.Models;
using DevRain.Asana.API.Services;
using DevRain.Asana.API.Services.API;
using DevRain.Asana.API.Services.Db;
using DevRain.Windows.WinRT.Common.Models;
using DevRain.Windows.WinRT.Common.MVVM;

namespace DevRain.Winsana.Core.Models
{
    public class AsanaViewModel:ViewModel
    {


        protected StorageService GetStorageService()
        {
            return new StorageService();
        }

        private bool isPinned;
        public bool IsPinned
        {
            get { return isPinned; }
            set
            {
                isPinned = value;
                RaisePropertyChanged(() => PinButtonText);
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
                    RaisePropertyChanged(() => IsAppBarVisible);
                }
            }
        }

        private string pageTitleText;

        public string PageTitleText
        {
            get { return pageTitleText; }
            set
            {
                if (pageTitleText != value)
                {
                    this.pageTitleText = value;
                    RaisePropertyChanged(()=>PageTitleText);
                }
            }
        }

        public ICommand GoToAllTasksPageCommand { get; set; }
        public ICommand GoToMainPageCommand { get; set; }
        public ICommand GoToSettingsPageCommand { get; set; }
        public ICommand GoToAboutPageCommand { get; set; }





        public void GoToEditTask(long id)
        {
            //NavigationService.Navigate<AddEditTask>("id", id);
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
                                          task.assigneeid, isCompleted, dueOn, null);

            var response = await new AsanaApiRepository().UpdateTask(dto);

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
            await UpdateTask(id, true, null, (task) =>
            {
                //Container.Resolve<IMessagePublisher>().Publish(new TaskStatusCompletedMessage(this) { Id = task.id, IsCompleted = task.IsCompleted });
            });
        }

        public async Task UncompleteTask(long id)
        {
            await UpdateTask(id, false, null, (task) =>
            {
              //  Container.Resolve<IMessagePublisher>().Publish(new TaskStatusCompletedMessage(this) { Id = task.id, IsCompleted = task.IsCompleted });
            });
        }

        protected void FillTaskCommands(AsanaTask asanaTask)
        {
            asanaTask.EditTaskCommand = new DelegateCommand(o => GoToEditTask((long)o));
            asanaTask.CompleteTaskCommand = new DelegateCommand(async o => await CompleteTask((long)o));
            asanaTask.UncompleteTaskCommand = new DelegateCommand(async o => await UncompleteTask((long)o));
        }
    }
}
