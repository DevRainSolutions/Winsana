using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.UI.Popups;
using DevRain.Asana.API.Data.Models;
using DevRain.Windows.WinRT.Common.Core;
using DevRain.Windows.WinRT.Common.Services;


namespace DevRain.Asana.API.Services.API
{
   public interface IAsanaResponseProcessor
   {
       bool ProcessResponse<T>(AsanaResponse<T> response, bool onlySuccess = false,
           Func<AsanaResponse<T>, Task<bool>> processError = null) where T : class;
   }

   public class NonUiAsanaResponseProcessor : IAsanaResponseProcessor
    {

        public bool ProcessResponse<T>(AsanaResponse<T> response, bool onlySuccess = false, Func<AsanaResponse<T>, Task<bool>> processError = null) where T : class
        {
            var success = (response.StatusCode == HttpStatusCode.OK ||
                         response.StatusCode == HttpStatusCode.Created) && response.Data != null;


            return success;


        }
    }

   public class AsanaResponseProcessor : IAsanaResponseProcessor
   {
       public bool ProcessResponse<T>(AsanaResponse<T> response, bool onlySuccess = false, Func<AsanaResponse<T>, Task<bool>> processError = null) where T : class
       {
           var success = (response.StatusCode == HttpStatusCode.OK ||
                        response.StatusCode == HttpStatusCode.Created) && response.Data != null;


           if (!success && !onlySuccess)
           {
               StateService.DependencyResolverContainer.Resolve<IDispatcher>().RunAsync(async () =>
               {
                   if (processError != null)
                   {
                       var r = await processError(response);
                       if (r) return;
                   }

                   var dialog = new MessageDialog(GetErrorMessage(response),"Error");
                   await dialog.ShowAsync();

                   
               });

           }

           return success;


       }

       public string GetErrorMessage<T>(AsanaResponse<T> response) where T : class
       {
           if (response.Errors != null && response.Errors.IsUnknownObject)
           {
               return "Object is not found - seems like it was deleted in Asana service. Sync please";
           }

           return response.StatusDescription != null ? response.StatusDescription : response.StatusCode.ToString();
       }
   }
}
