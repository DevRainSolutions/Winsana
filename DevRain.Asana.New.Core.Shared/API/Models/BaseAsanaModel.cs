using System;
using System.ComponentModel;


namespace DevRain.Asana.API.Data.Models
{
 
    public class BaseAsanaModel:INotifyPropertyChanged
    {
        public bool IsFoundInDb { get; set; }

        public long id { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));

        }
    }

    public class TaskStatus:BaseAsanaModel
    {
        public string name { get; set; }


        public long id { get; set; }
    }

    public class Assignee:BaseAsanaModel
    {
        public long id { get; set; }
    }

    public class AEntity
    {
        public string name { get; set; }
        public long id { get; set; }
    }
}
