using System;
using System.Net;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using System.Windows;
using DevRain.Asana.API.Data.Models;
using DevRain.WP.Common.Helpers;
using DevRain.WP.Common.Services;


namespace DevRain.Asana.API.Services.API
{

   

   public  class AsanaClient
   {



           public static bool ProcessResponse<T>(AsanaResponse<T> response, bool onlySuccess = false,
               Func<AsanaResponse<T>, Task<bool>> processError = null) where T : class
           {
               return AppBootstrapperStateService.Container.Resolve<IAsanaResponseProcessor>()
                   .ProcessResponse(response, onlySuccess, processError);

           }

       
   }
}
