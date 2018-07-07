
using System;
using System.Collections.Generic;
using DevRain.Asana.New.Core.Shared;


namespace DevRain.Asana.API.Data.Models
{
	public class AProject:BaseAsanaModel
	{
		public AProject()
		{
			this.Tasks = new List<ATask>();
			this.followers = new List<AFollower>();
		}

     public int Order { get; set; }


     public Int64 workspaceid { get; set; }

        
		public string name { get; set; }

        
		

        
		public DateTime? created_at { get; set; }

		
		public DateTime? LastLoadDate { get; set; }

        
		public DateTime? modified_at { get; set; }

        
		public string notes { get; set; }

        
		public bool archived { get; set; }


		public AWorkspace workspace { get; set; }
		public List<AFollower> followers { get; set; }
        public List<ATask> Tasks { get; set; }

        private decimal tasksCount;
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

		public bool HasOverDueTasks
		{
			get { return OverdueTasksCount > 0; }
		}


	}
}