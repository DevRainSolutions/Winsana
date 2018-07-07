using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DevRain.Asana.API;
using DevRain.Asana.API.Data;
using DevRain.Asana.API.Services;
using DevRain.Asana.API.Services.API;

using DevRain.Asana.Pages;
using DevRain.Asana.Services;
using DevRain.Asana.ViewModels;
using DevRain.WP.Common.Helpers;
using DevRain.WP.Common.Logging;
using DevRain.WP.Core.Controls;
using DevRain.WP.Core.Controls.Feedback;
using DevRain.WP.Core.Helpers;
using DevRain.WP.Core.MVVM.Core;
using FlurryWP8SDK;
using HuntersWP.Db;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;
using Yandex.Metrica;

namespace DevRain.Asana
{
    public class AsanaBootstrapper : WP8Bootstrapper
	{
		protected override Microsoft.Phone.Controls.PhoneApplicationFrame CreateRootFrame()
		{
			return new AnimatedFrame();
		}

        public override string StartPage
        {
            get { return typeof (StartPage).Name; }
        }

        protected override void Init()
		{
			Container.Register<ILogger, DebugLogger>();
            Container.Register<DbService, DbService>();
            Container.RegisterInstance<IAsanaResponseProcessor, AsanaResponseProcessor>();
			SmallProfiler.Start();

            Container.Resolve<DbService>().Initialize();

			InteractionEffectManager.AllowedTypes.Add(typeof(RadDataBoundListBoxItem));


            Counter.Start(10669);

			RadDiagnostics diagnostics = new RadDiagnostics();
			diagnostics.EmailTo = AsanaConstants.FeedbackEmail;
			diagnostics.EmailSubject = AsanaConstants.AppTitle;
			diagnostics.ApplicationVersion = DeviceHelper.GetAppVersion().ToString();
			//diagnostics.MessageBoxInfo.Title = "";
			diagnostics.MessageBoxInfo.Content = "Would you to like to send information about error to developers?";

			diagnostics.Init();
			Telerik.Windows.Controls.ApplicationUsageHelper.Init(DeviceHelper.GetAppVersion().ToString());
			/// ///

			AsanaStateService.Initialize();

		    
			//ApplicationRatingNotificationServiceFromNokia.Initialize("Winsana","support@devrain.com");


			ViewModelMap.Register<MainPage, MainViewModel>();
			ViewModelMap.Register<WorkspaceDetails, WorkspaceDetailsViewModel>();
			ViewModelMap.Register<UserDetails, UserDetailsViewModel>();
			ViewModelMap.Register<AddEditProject, AddEditProjectViewModel>();
			ViewModelMap.Register<TagDetails, TagDetailsViewModel>();
			ViewModelMap.Register<TaskDetails, TaskDetailsViewModel>();
			ViewModelMap.Register<AddEditTask, AddEditTaskViewModel>();
			ViewModelMap.Register<ProjectDetails, ProjectDetailsViewModel>();
			ViewModelMap.Register<AllTasks, AllTasksViewModel>();
            ViewModelMap.Register<SettingsPage, SettingsViewModel>();

			ViewModelCache.Register<WorkspaceDetailsViewModel>();
			ViewModelCache.Register<ProjectDetailsViewModel>();
			ViewModelCache.Register<UserDetailsViewModel>();
			ViewModelCache.Register<TagDetailsViewModel>();
			ViewModelCache.Register<TaskDetailsViewModel>();
	


			System.Threading.Tasks.TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
		}

		void TaskScheduler_UnobservedTaskException(object sender, System.Threading.Tasks.UnobservedTaskExceptionEventArgs e)
		{



			var flatten = e.Exception.Flatten();

			foreach (var innerException in flatten.InnerExceptions)
			{
				ProcessError("async", innerException, true);
			}

		}

		protected override void OnApplicationUnhandledException(System.Windows.ApplicationUnhandledExceptionEventArgs e)
		{
			ProcessError("OnApplicationUnhandledException", e.ExceptionObject);
		}



		void ProcessError(string message, Exception e, bool async = false)
		{
			if (System.Diagnostics.Debugger.IsAttached)
			{
				// A navigation has failed; break into the debugger
				System.Diagnostics.Debugger.Break();
			}

#if DEBUG
			return;
#endif

			if (async)
			{
				BugSense.BugSenseHandler.Instance.LogError(e, "async");
			}

			ActionHelper.SafeExecute(() =>
			{
				Api.LogError(string.Format("{0};{1};{2}", e.Message, message, e.GetType()), e);
			});
		}

		protected override void OnApplicationLaunching(object sender, Microsoft.Phone.Shell.LaunchingEventArgs e)
		{
			StartAnalytics();
			BackgroundTaskService.Instance.UpdateBackgroundTasks();
		}

		protected override void OnApplicationClosing(object sender, Microsoft.Phone.Shell.ClosingEventArgs e)
		{
			StopAnalytics();
		}

        protected override void OnApplicationActivated(ActivatedEventArgs e)
		{
			StartAnalytics();
		}

		protected override void OnApplicationDeactivated()
		{
			StopAnalytics();
		}

		private void StopAnalytics()
		{
#if DEBUG
			return;
#endif


			ActionHelper.SafeExecute(() =>
										 {
											 Api.EndSession();
										 });
		}

		private string FLURRY_KEY = "W7338QGZQBZ6JSYB6HND";
		void StartAnalytics()
		{

#if DEBUG
			return;
#endif

			ActionHelper.SafeExecute(() =>
										 {
											 Api.StartSession(FLURRY_KEY);

											 string ver = DeviceHelper.GetAppVersion().ToString();

											 Api.SetVersion(ver);
										 });
		}

	}
}
