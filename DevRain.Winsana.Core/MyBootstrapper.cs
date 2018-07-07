using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevRain.Asana.API.Services.API;
using DevRain.Windows.WinRT.Common.Core;
using DevRain.Windows.WinRT.Common.MVVM;
using DevRain.Winsana.Core.Services.Db;

namespace DevRain.Winsana.Core
{
    public class MyBootstrapper : Bootstrapper
    {
        protected override async Task OnInitialize()
        {
            using (var registration = StateService.DependencyResolverContainer.Registration())
            {
                registration.Register<DbService>().AsSingleton();
                registration.Register<IAsanaResponseProcessor>().As<AsanaResponseProcessor>();
            }



            new DbService().Initialize();
        }
    }
}
