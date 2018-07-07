using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using DevRain.Windows.WinRT.Common.Helpers;
using SQLite;


namespace DevRain.Asana.API.Data.Models
{
  
    public class AsanaTagTask : BaseAsanaDbEntity
    {
        public int Order { get; set; }
        
        public AsanaTagTask()
        {

        }
        


        public long TaskId { get; set; }

        public long TagId { get; set; }
    }



    public class AsanaTag:BaseAsanaDbEntity
    {
        public AsanaTag()
		{
			
			this.followers = new List<AsanaFollower>();
		}




        public DateTime? created_at { get; set; }

                [Ignore]
        public List<AsanaFollower> followers { get; set; }


        public string name { get; set; }


        public string notes { get; set; }
                [Ignore]
        public AsanaWorkspace workspace { get; set; }


        public Int64 workspaceid { get; set; }

                [Ignore]
        public decimal TasksCount { get; set; }
                [Ignore]
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
