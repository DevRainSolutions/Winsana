using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DevRain.Asana.API.Data.Models;
using DevRain.Asana.API.Services.API;
using DevRain.WP.Common.Helpers;
using DevRain.WP.Core.Helpers;

namespace DevRain.Asana.Services
{
    public class AsanaResponseProcessor:IAsanaResponseProcessor
    {
        public bool ProcessResponse<T>(AsanaResponse<T> response, bool onlySuccess = false, Func<AsanaResponse<T>, Task<bool>> processError = null) where T : class
        {
            var success = (response.StatusCode == HttpStatusCode.OK ||
                         response.StatusCode == HttpStatusCode.Created) && response.Data != null;


            if (!success && !onlySuccess)
            {


                DispatcherHelper.OnUi(async () =>
                {
                    if (processError != null)
                    {
                        var r = await processError(response);
                        if (r) return;
                    }


                    MessageBox.Show(GetErrorMessage(response), "Error", MessageBoxButton.OK);
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
