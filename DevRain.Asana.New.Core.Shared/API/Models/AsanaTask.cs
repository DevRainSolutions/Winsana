
using System.Runtime.Serialization;
using System.Linq;
using System;
using System.Collections.Generic;

namespace DevRain.Asana.API.Data.Models
{

	public class ATask:BaseAsanaModel
	{
		public ATask()
		{
			this.followers = new List<AFollower>();
		}

      
        public int Order { get; set; }

        /// <summary>
        /// SEPARATED BY ;
        /// </summary>
    
        public string FollowersIds { get; set; }

        public List<long> GetFollowers()
        {
            if(String.IsNullOrEmpty(FollowersIds)) return new List<long>();

            var l = FollowersIds.Split(new string[] { AsanaConstants.Utils.FOLLOWERS_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);

            return l.Select(x => long.Parse(x)).ToList();

        }

        public void SetFollowers(List<long> followers)
        {
            FollowersIds = String.Join(AsanaConstants.Utils.FOLLOWERS_SEPARATOR, followers.Select(x => x.ToString()));
        }



        public Assignee assignee { get; set; }

 
        public long assigneeid { get; set; }

  
		public string assignee_status { get; set; }

   
        public long? parentId { get; set; }

    
        public long projectid { get; set; }
        

		public DateTime created_at { get; set; }

   
        public string name { get; set; }



  
		public bool completed { get; set; }

      
		public DateTime? completed_at { get; set; }

        
		public DateTime? due_on { get; set; }


		public List<AFollower> followers { get; set; }


		public DateTime modified_at { get; set; }
		
        
		public string notes { get; set; }

        
        public long workspaceid { get; set; }

        
        public bool IsForSync { get; set; }

        
        public bool IsPriorityHeading { get; set; }

        public string DisplayName
        {
            get
            {
                if(string.IsNullOrEmpty(name))
                {
                    return "No text";
                }

                return name;
            }
        }

        public string ProjectName { get; set; }
		public string JumpListProjectName { get; set; }
        
        public bool HasProjectName
        {
            get { return !string.IsNullOrEmpty(ProjectName); }
        }

        public string UserName { get; set; }

        public bool HasUserName
        {
            get { return !string.IsNullOrEmpty(UserName); }
        }

        public List<ATag> tags { get; set; }
		public List<AProject> projects { get; set; }
		public BaseAsanaModel workspace { get; set; }
        public AEntity parent { get; set; }




        public bool DisplayTasksCount { get; set; }




        //private decimal tasksCount;
        //public decimal TasksCount
        //{
        //    get { return tasksCount; }
        //    set
        //    {
        //        if (tasksCount != value)
        //        {
        //            tasksCount = value;
        //            OnPropertyChanged("TasksCount");
        //            OnPropertyChanged("TasksCountText");
        //        }
        //    }
        //}

        public string TasksCountText { get; set; }

        public string DueOnText
        {
            get
            {
                if (!due_on.HasValue) return string.Empty;
                return string.Format("due to {0}", DueOn);
            }
        }

	    public string DueOn
	    {
	        get
	        {
	            var text = "";
                if (!due_on.HasValue) return string.Empty;

	            var today = DateTime.Today;
	            var yesterday = today.AddDays(-1);
	            var tomorrow = today.AddDays(1);

	            var value = due_on.Value;
                if (value.Date == today)  return  "today";
                else if (value.Date == yesterday) return "yesterday";
                else if (value.Date == tomorrow) return"tomorrow";
                else
                {
                    //TODO: fix
                    return value.Date.ToString();
                }


	   
	        }
	    }

	    public bool IsOverDue
	    {
	        get
	        {
	           // if (completed) return false;
	            
                return due_on.HasValue && due_on.Value <= DateTime.Today;
	        }
	    }

	    public decimal Opacity
	    {
	        get { return completed ? 0.7m : 1.0m; }
	    }

	    public bool IsCompleted
	    {
	        get { return completed; }
            set
            {
                if (completed != value)
                {
                    completed = value;
                    OnPropertyChanged("IsCompleted");
                    OnPropertyChanged("completed");
                    OnPropertyChanged("Opacity");
                }
            }
	    }

        public bool CannotEdit { get; set; }
        //[IgnoreDataMember]
        //public ICommand UncompleteTaskCommand { get; set; }
        //[IgnoreDataMember]
        //public ICommand CompleteTaskCommand { get; set; }
        //[IgnoreDataMember]
        //public ICommand EditTaskCommand { get; set; }


	}

    public enum EAsanaTaskStatus
    {
        upcoming = 0,
        inbox,
        later,
        today
    }

}