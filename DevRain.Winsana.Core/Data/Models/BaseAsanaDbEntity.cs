using System;
using System.ComponentModel;
using SQLite;


namespace DevRain.Asana.API.Data.Models
{

    public class BaseAsanaDbEntity : INotifyPropertyChanged
    {
        [PrimaryKey]
        public long id { get; set; }

        public bool IsFoundInDb { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));

        }
    }

    public class TaskStatus : BaseAsanaDbEntity
    {
        public string name { get; set; }


    }

    public class Assignee : BaseAsanaDbEntity
    {

    }

    public class ApiEntity
    {
        public string name { get; set; }


        public long id { get; set; }
    }
}
