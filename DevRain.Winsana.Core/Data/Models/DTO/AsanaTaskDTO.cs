using System;
using System.Collections.Generic;

using System.Net;
using System.Windows;


namespace DevRain.Asana.API.Data.Models.DTO
{
    public class AsanaTaskDTO:BaseAsanaDbEntity
    {
        public string name { get; set; }
        public Int64 id { get; set; }

        public string notes { get; set; }

        public long projects { get; set; }



        public bool completed { get; set; }

        public string due_on { get; set; }

        public string assignee { get; set; }

        public string assignee_status { get; set; }

        public List<AsanaFollower> followers { get; set; }
        
    }
}
