using System;
using DevRain.Asana.API;
using DevRain.Asana.API.Services;
using DevRain.Asana.API.Storage;
using DevRain.Asana.Pages;
using DevRain.Asana.Services;
using DevRain.Asana.ViewModels;
using DevRain.WP.Common.Logging;
using DevRain.WP.Core.Helpers;
using DevRain.WP.Core.MVVM.Core;
using Microsoft.Phone.Shell;
using Winsana.Phone.Tests.Infrastructure;

namespace Winsana.Phone.Tests
{
    public class TestBootstrapper : Bootstrapper
    {
        protected override void Init()
        {
            Container.Register<ILogger, DebugLogger>();
            Container.Register<IDbService, TestDbService>();
            SmallProfiler.Start();

            Container.Resolve<IDbService>().Initialize();

        

            AsanaStateService.Initialize();

            ViewModelMap.Register<DevRain.Asana.Pages.MainPage, MainViewModel>();
            ViewModelMap.Register<WorkspaceDetails, WorkspaceDetailsViewModel>();
            ViewModelMap.Register<UserDetails, UserDetailsViewModel>();
            ViewModelMap.Register<AddEditProject, AddEditProjectViewModel>();
            ViewModelMap.Register<TagDetails, TagDetailsViewModel>();
            ViewModelMap.Register<TaskDetails, TaskDetailsViewModel>();
            ViewModelMap.Register<AddEditTask, AddEditTaskViewModel>();
            ViewModelMap.Register<ProjectDetails, ProjectDetailsViewModel>();


            ViewModelCache.Register<WorkspaceDetailsViewModel>();
            ViewModelCache.Register<ProjectDetailsViewModel>();
            ViewModelCache.Register<UserDetailsViewModel>();
            ViewModelCache.Register<TagDetailsViewModel>();
            ViewModelCache.Register<TaskDetailsViewModel>();
            //Container.Resolve<ILogger>().Debug("Init complete");



        }



        protected override void OnApplicationUnhandledException(System.Windows.ApplicationUnhandledExceptionEventArgs e)
        {
            ProcessError("OnApplicationUnhandledException", e.ExceptionObject);
        }



        void ProcessError(string message, Exception e)
        {




        }

        protected override void OnApplicationLaunching(object sender, Microsoft.Phone.Shell.LaunchingEventArgs e)
        {
          
        }

        protected override void OnApplicationClosing(object sender, Microsoft.Phone.Shell.ClosingEventArgs e)
        {
          
        }

        protected override void OnApplicationActivated(ActivatedEventArgs e)
        {
          
        }

        protected override void OnApplicationDeactivated()
        {
  
        }

       
       
    }
}
