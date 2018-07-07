using System;
using System.Collections.Generic;
using DevRain.Asana.New.Core.Shared;


namespace DevRain.Asana.API.Data.Models
{

    public class ATagTask : BaseAsanaModel
    {

    
        public int Order { get; set; }




        public long TaskId { get; set; }


        public long TagId { get; set; }
    }



    public class ATag:BaseAsanaModel
    {
        public ATag()
		{
			
			this.followers = new List<AFollower>();
		}




        public DateTime? created_at { get; set; }

        public List<AFollower> followers { get; set; }


        public string name { get; set; }


        public string notes { get; set; }

        public AWorkspace workspace { get; set; }

        
        public Int64 workspaceid { get; set; }


        public decimal TasksCount { get; set; }
        public string TasksCountText
        {
            get { return TextHelper.GetText(TasksCount, "task", "tasks", "tasks", "no tasks"); }
        }

        public override string ToString()
        {
            return name;
        }
    }
}
