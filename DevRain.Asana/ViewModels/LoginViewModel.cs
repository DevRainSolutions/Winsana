using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DevRain.WP.Core.MVVM.State;

namespace DevRain.Asana.ViewModels
{
    public class LoginViewModel:AsanaViewModel
    {
        private string apiKey;
		[Tombstoned]
        public string ApiKey
        {
            get { return apiKey; }
            set
            {
                apiKey = value;
                NotifyOfPropertyChange("ApiKey");
            }
        }
    }
}
