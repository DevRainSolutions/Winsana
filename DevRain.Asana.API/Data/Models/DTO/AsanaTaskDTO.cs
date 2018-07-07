using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

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
