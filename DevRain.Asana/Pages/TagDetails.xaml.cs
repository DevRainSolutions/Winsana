using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using DevRain.Asana.API;
using DevRain.Asana.API.Data.Models;
using DevRain.Asana.Services;
using DevRain.Asana.ViewModels;

using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace DevRain.Asana.Pages
{
    public partial class TagDetails : PhoneApplicationPage
    {
        public TagDetails()
        {
            InitializeComponent();

        }


        
     

        private void LstActiveTasks_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            var task = e.AddedItems[0] as AsanaTask;


            ExNavigationService.Navigate<TaskDetails>("id", task.id);

        }

  
        private void BtnRefresh_OnClick(object sender, EventArgs e)
        {
            
            var model = DataContext as TagDetailsViewModel;

            if (!model.CheckInternetConnection(false)) return;

            model.LoadTasks(true);
        }
    }
}