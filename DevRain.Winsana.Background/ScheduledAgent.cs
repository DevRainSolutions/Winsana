using System;
using System.Diagnostics;
using System.Windows;
using DevRain.Asana.API;
using DevRain.Asana.API.Data;
using DevRain.Asana.API.Services;
using DevRain.Asana.API.Services.API;
using DevRain.Asana.API.Services.Db;
using DevRain.WP.Common.IoC;
using DevRain.WP.Common.Logging;
using DevRain.WP.Common.Services;
using HuntersWP.Db;
using Microsoft.Phone.Scheduler;

namespace WinsanaBackground
{
	public class ScheduledAgent : ScheduledTaskAgent
	{
		private static volatile bool _classInitialized;

		/// <remarks>
		/// ScheduledAgent constructor, initializes the UnhandledException handler
		/// </remarks>
		public ScheduledAgent()
		{
			if (!_classInitialized)
			{
				_classInitialized = true;
				// Subscribe to the managed exception handler
				Deployment.Current.Dispatcher.BeginInvoke(delegate
				{
					Application.Current.UnhandledException += ScheduledAgent_UnhandledException;
				});
                //var b = new BackgroundBootstrapper();
                //b.InitializeForBackgroundTask();
                AsanaStateService.Initialize();
                AppBootstrapperStateService.Container = new IocContainer();
                AppBootstrapperStateService.Container.Register<ILogger, DebugLogger>();
                AppBootstrapperStateService.Container.Register<DbService, DbService>();
                AppBootstrapperStateService.Container.RegisterInstance<IApplicationSettingsService, ApplicationSettingsService>();
                AppBootstrapperStateService.Container.RegisterInstance<IAsanaResponseProcessor, NonUiAsanaResponseProcessor>();
                //AppBootstrapperStateService.Container.Resolve<DbService>().Initialize();
			}
		}

		/// Code to execute on Unhandled Exceptions
		private void ScheduledAgent_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
		{
			if (System.Diagnostics.Debugger.IsAttached)
			{
				// An unhandled exception has occurred; break into the debugger
				System.Diagnostics.Debugger.Break();
			}

			Debug.WriteLine("Error: " + e.ExceptionObject.Message);

		}

		/// <summary>
		/// Agent that runs a scheduled task
		/// </summary>
		/// <param name="task">
		/// The invoked task
		/// </param>
		/// <remarks>
		/// This method is called when a periodic or resource intensive task is invoked
		/// </remarks>
		protected override async void OnInvoke(ScheduledTask task)
		{
			Debug.WriteLine("OnInvoke");

			if (!AsanaStateService.IsSetAuthToken && !AsanaStateService.IsSetApiKey)
			{
				Debug.WriteLine("Complete - no auth");
				NotifyComplete();
				return;
			}


		    var response = await new AsanaRespository().GetWorkspaces();

			if (!AsanaClient.ProcessResponse(response, true))
			{
				Debug.WriteLine("Complete - no connection");
				NotifyComplete();
				return;
			}


			var service = new LoadDataService(false);
			service.IsBackgroundAgent = true;

			var projects = await new StorageService().GetProjectsForRefresh();

			foreach (var project in projects)
			{
				await service.LoadProjectTasks(project.workspaceid, project.id, false);
			}


			TileService.UpdateMainTile();

			Debug.WriteLine("Complete - OK");
			NotifyComplete();


		}


	}
}