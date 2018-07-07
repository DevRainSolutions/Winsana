using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using DevRain.Asana.API.Services.API;
using DevRain.Asana.ViewModels;
using DevRain.WP.Core.Helpers;
using System.Threading.Tasks;

namespace DevRain.Asana.Services
{
	public class OffineSyncTaskService
	{
		private static bool synced = false;

		public static async Task Execute(AsanaViewModel model,Action actionAfterSuccess)
		{
			if (synced) return;
			synced = true;

			if (model.CheckInternetConnection(false))
			{
				model.AddOperation();
			    var response = await new LoadDataService(false).SyncTasks();
			       model.RemoveOperation();
                   if (response)
                    {
                        actionAfterSuccess();
                    }

			}
		}
	}
}
