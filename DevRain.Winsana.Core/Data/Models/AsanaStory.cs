using System;

using System.Net;
using System.Windows;

using SQLite;

namespace DevRain.Asana.API.Data.Models
{

    public class AsanaStory:BaseAsanaDbEntity
    {
      
        public int Order { get; set; }




        public DateTime created_at { get; set; }

               [Ignore]
        public AsanaUser created_by { get; set; }


        public string text { get; set; }

 
        public long userId { get; set; }
                [Ignore]
        public ApiEntity target { get; set; }


        public long? targetId { get; set; }

        public string source { get; set; }

   
        public string type { get; set; }

                [Ignore]
        public bool IsComment
        {
            get { return type == "comment"; }
        }
                [Ignore]
        public string CreatedAt
        {
            get { return created_at.ToString(); }
        }
    }
}
