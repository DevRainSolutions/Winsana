using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DevRain.Asana.API.Services.Db;


namespace DevRain.Asana.API.Services.API
{
    public class LoadTasksService
    {
         private bool _isUserActionRequest = false;

        public LoadTasksService()
        {
            
        }

        public LoadTasksService(bool isUserActionRequest)
        {
            _isUserActionRequest = isUserActionRequest;

        }
        public event EventHandler<EventArgs> SubTasksLoadStarted= delegate { };
        public event EventHandler<EventArgs> SubTasksLoaded = delegate { };
        public event EventHandler<EventArgs> LoadCompleted = delegate { };

        public async Task LoadProjectTasks(long workspaceId, long projectId)
        {
            var service = new LoadDataService(_isUserActionRequest);
            await service.LoadProjectTasks(workspaceId, projectId, false);
            service.SubTasksLoaded += service_SubTasksLoaded;

            LoadCompleted(this, EventArgs.Empty);
            
            var dbTasks = (await new StorageService().GetTasks(projectId)).Where(x => !x.IsCompleted);

            foreach (var task in dbTasks)
            {
                SubTasksLoadStarted(task.id, EventArgs.Empty);
                await service.LoadSubtasks(task.id, false);
            }




        }

        void service_SubTasksLoaded(object sender, EventArgs e)
        {
            SubTasksLoaded(sender, e);
        }
    }
}
