

using DevRain.WP.Common.Helpers;
using System;
using System.Collections.Generic;
using SQLite;


namespace DevRain.Asana.API.Data.Models
{
	public class AsanaProject:BaseAsanaDbEntity
	{
		public AsanaProject()
		{
			this.Tasks = new List<AsanaTask>();
			this.followers = new List<AsanaFollower>();
		}

        
        public int Order { get; set; }


        
		public Int64 workspaceid { get; set; }


		public string name { get; set; }


		public DateTime? created_at { get; set; }

		
		public DateTime? LastLoadDate { get; set; }

        
		public DateTime? modified_at { get; set; }

        
		public string notes { get; set; }

        
		public bool archived { get; set; }

            [Ignore]
		public AsanaWorkspace workspace { get; set; }
            [Ignore]
		public List<AsanaFollower> followers { get; set; }
            [Ignore]
        public List<AsanaTask> Tasks { get; set; }

        private decimal tasksCount;

        [Ignore]
        public decimal TasksCount
        {
            get { return tasksCount; }
            set
            {
                if(tasksCount != value)
                {
                    tasksCount = value;
                    OnPropertyChanged("TasksCount");
                    OnPropertyChanged("TasksCountText");
					OnPropertyChanged("HasOverDueTasks");
                }
            }
        }
                [Ignore]
        public string TasksCountText
        {
            get
            {
	            var r =  TextHelper.GetText(TasksCount, "task", "tasks", "tasks", "no tasks");

				if (HasOverDueTasks)
				{
					r += string.Format(", {0} overdue", OverdueTasksCount);
				}

	            return r;
            }
        }

		private decimal overdueTasksCount;
                [Ignore]
		public decimal OverdueTasksCount
		{
			get { return overdueTasksCount; }
			set
			{
				if (overdueTasksCount != value)
				{
					overdueTasksCount = value;
					OnPropertyChanged("OverdueTasksCount");
					OnPropertyChanged("TasksCountText");
					OnPropertyChanged("HasOverDueTasks");
				}
			}
		}
                [Ignore]
		public bool HasOverDueTasks
		{
			get { return OverdueTasksCount > 0; }
		}


	}
}