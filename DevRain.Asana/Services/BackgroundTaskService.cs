using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Phone.Scheduler;

namespace DevRain.Asana.Services
{
	public class BackgroundTaskService
	{
		/// <summary>
		/// Synchronization object (singletone pattern).
		/// </summary>
		private static object _syncObject = new object();
		/// <summary>
		/// Instance of the service.
		/// </summary>
		private static BackgroundTaskService _Instance = null;

		/// <summary>
		/// Gets the instance of service.
		/// </summary>
		public static BackgroundTaskService Instance
		{
			get
			{
				if (_Instance == null)
				{
					lock (_syncObject)
					{
						if (_Instance == null)
							_Instance = new BackgroundTaskService();
					}
				}
				return _Instance;
			}
		}

		/// <summary>
		/// Attention! Private constructor, instance creation via static property.
		/// </summary>
		private BackgroundTaskService()
		{
		}

		public const string TASK_AGENT_NAME = "WinsanaBackground";

		#region Methods

		public void UpdateBackgroundTasks()
		{
	

			ScheduledAction task = null;

			task = ScheduledActionService.Find(TASK_AGENT_NAME);

			if(task != null)
			{
				ScheduledActionService.Remove(TASK_AGENT_NAME);
			}
			task = AddBackgroundTask();
			

			if (task != null)
				task.ExpirationTime = DateTime.Now + new TimeSpan(14, 0, 0, 0, 0);


#if DEBUG
			// If we're debugging, attempt to start the task immediately
			//ScheduledActionService.LaunchForTest(TASK_AGENT_NAME, new TimeSpan(0, 0, 1));

			
#endif
		}

		#endregion

		#region Private Methods

		private ScheduledAction AddBackgroundTask()
		{
			// Start background agent 
			PeriodicTask periodicTask = new PeriodicTask(TASK_AGENT_NAME);
			periodicTask.Description = "Winsana background task";

			// Place the call to Add in a try block in case the user has disabled agents.
			try
			{
				// only can be called when application is running in foreground.
				ScheduledActionService.Add(periodicTask);

				return periodicTask;
			}
			catch (InvalidOperationException ex)
			{
				if (ex.Message.Contains("BNS Error: The action is disabled"))
				{
					Debug.WriteLine("Unable to start service agent");
				}
				if (ex.Message.Contains("BNS Error: The maximum number of ScheduledActions of this type have already been added."))
				{
					Debug.WriteLine("Unable to start service agent");
				}
			}
			catch (SchedulerServiceException ex)
			{
					Debug.WriteLine("Unable to start service agent");
			}

			return null;
		}

		#endregion
	}
}
