using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevRain.Asana.API.Services.API;
using DevRain.Winsana.Core.Models;

namespace DevRain.Winsana.Services
{
    public class OffineSyncTaskService
    {
        private static bool synced = false;

        public static async Task Execute(AsanaViewModel model, Action actionAfterSuccess)
        {
            if (synced) return;
            synced = true;

            if (await model.CheckInternetConnection(false))
            {
              //  model.AddOperation();
                var response = await new LoadDataService(false).SyncTasks();
                //model.RemoveOperation();
                if (response)
                {
                    actionAfterSuccess();
                }

            }
        }
    }
}
