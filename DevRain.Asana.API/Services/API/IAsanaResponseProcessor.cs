using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DevRain.Asana.API.Data.Models;
using DevRain.WP.Common.Helpers;

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
}
