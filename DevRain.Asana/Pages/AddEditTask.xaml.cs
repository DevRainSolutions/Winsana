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
using DevRain.Asana.API.Data.Models;
using DevRain.Asana.API.Data.Models.DTO;
using DevRain.Asana.Services;
using DevRain.Asana.ViewModels;
using DevRain.WP.Core.Models;
using Microsoft.Phone.Controls;

namespace DevRain.Asana.Pages
{
    public partial class AddEditTask : PhoneApplicationPage
    {
        public AddEditTask()
        {
            InitializeComponent();
            

        }

      

      

        //private void BtnSave_OnClick(object sender, EventArgs e)
        //{

        //    SaveTask();
        //}

        //private void BtnDelete_OnClick(object sender, EventArgs e)
        //{

        //    DeleteTask();
        //}

        //void DeleteTask()
        //{
        //    //var model = DataContext as AddEditTaskViewModel;

        //    //if(!model.Id.HasValue)
        //    //{
        //    //    NavigationService.GoBack();
        //    //    return;
        //    //}

        //    //Scheduler.Default.Schedule(() =>
        //    //    {
        //    //        var result = new AsanaRespository().DeleteTask(model.Id.Value);

        //    //        if(result.Errors == null)
        //    //        {
        //    //            Dispatcher.BeginInvoke(() =>
        //    //            {
        //    //                NavigationService.GoBack();
        //    //            });      
        //    //        }


        //    //    });
        //}

    }
}