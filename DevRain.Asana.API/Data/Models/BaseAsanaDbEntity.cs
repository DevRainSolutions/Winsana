using System;
using System.ComponentModel;
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
