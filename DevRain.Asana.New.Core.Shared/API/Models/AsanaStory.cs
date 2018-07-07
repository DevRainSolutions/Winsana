using System;
using System.Collections.Generic;


namespace DevRain.Asana.API.Data.Models
{
    public class AStoriesResponse : AResponse
    {
        public List<AStory> Data { get; set; }
    }

    public class AStory:BaseAsanaModel
    {
        public int Order { get; set; }
        

        public DateTime created_at { get; set; }

       
        public AsanaUser created_by { get; set; }
        public string text { get; set; }

        public long userId { get; set; }

        public AEntity target { get; set; }

        public long? targetId { get; set; }

       public string source { get; set; }

     
        public string type { get; set; }

        public bool IsComment
        {
            get { return type == "comment"; }
        }

        public string CreatedAt
        {
            get { return created_at.ToString(); }
        }
    }
}
