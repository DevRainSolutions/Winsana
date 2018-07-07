using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Concurrency;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using DevRain.Asana.API;
using DevRain.Asana.Services;
using DevRain.Asana.ViewModels;

using Microsoft.Phone.Controls;

namespace DevRain.Asana.Pages
{
    public partial class AddEditProject : PhoneApplicationPage
    {
        public AddEditProject()
        {
            InitializeComponent();

        }



 


        //void DeleteProject()
        //{
        //    var model = DataContext as AddEditProjectViewModel;

        //    if (!model.Id.HasValue)
        //    {
        //        NavigationService.GoBack();
        //        return;
        //    }
        //}
    }
}