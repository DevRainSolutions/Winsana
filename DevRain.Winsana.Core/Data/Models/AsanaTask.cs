using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Input;
using System;
using System.Collections.Generic;
using DevRain.Windows.WinRT.Common.Extensions;
using SQLite;

namespace DevRain.Asana.API.Data.Models
{

	public class AsanaTask:BaseAsanaDbEntity
	{
		public AsanaTask()
		{
			this.followers = new List<AsanaFollower>();
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


                [Ignore]
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

                [Ignore]
		public List<AsanaFollower> followers { get; set; }


		public DateTime modified_at { get; set; }
		

		public string notes { get; set; }


        public long workspaceid { get; set; }

 
        public bool IsForSync { get; set; }


        public bool IsPriorityHeading { get; set; }

                [Ignore]
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
                [Ignore]
        public string ProjectName { get; set; }
                [Ignore]
		public string JumpListProjectName { get; set; }
                [Ignore]
        public bool HasProjectName
        {
            get { return !string.IsNullOrEmpty(ProjectName); }
        }
                [Ignore]
        public string UserName { get; set; }
                [Ignore]
        public bool HasUserName
        {
            get { return !string.IsNullOrEmpty(UserName); }
        }
                [Ignore]
        public List<AsanaTag> tags { get; set; }
                [Ignore]
		public List<AsanaProject> projects { get; set; }
                [Ignore]
		public BaseAsanaDbEntity workspace { get; set; }
                [Ignore]
        public ApiEntity parent { get; set; }



                [Ignore]
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
                [Ignore]
        public string TasksCountText { get; set; }
                [Ignore]
        public string DueOnText
        {
            get
            {
                if (!due_on.HasValue) return string.Empty;
                return string.Format("due to {0}", DueOn);
            }
        }
                [Ignore]
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
                    return value.Date.ToShortDateString(); 
                }


	   
	        }
	    }
                [Ignore]
	    public bool IsOverDue
	    {
	        get
	        {
	           // if (completed) return false;
	            
                return due_on.HasValue && due_on.Value <= DateTime.Today;
	        }
	    }
                [Ignore]
	    public decimal Opacity
	    {
	        get { return completed ? 0.7m : 1.0m; }
	    }
                [Ignore]
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

                [Ignore]
        public bool CannotEdit { get; set; }
		[IgnoreDataMember]
        [Ignore]
        public ICommand UncompleteTaskCommand { get; set; }
		[IgnoreDataMember]
        [Ignore]
        public ICommand CompleteTaskCommand { get; set; }
		[IgnoreDataMember]
        [Ignore]
        public ICommand EditTaskCommand { get; set; }


	}

    public enum EAsanaTaskStatus
    {
        upcoming = 0,
        inbox,
        later,
        today
    }

}