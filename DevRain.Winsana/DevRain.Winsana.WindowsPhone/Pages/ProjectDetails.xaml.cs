using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556
using DevRain.Asana.API.Data.Models;

namespace DevRain.Winsana.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProjectDetails : Page
    {
        public ProjectDetails()
        {
            this.InitializeComponent();
        }

        private void LstActiveTasks_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            var task = e.AddedItems[0] as AsanaTask;

            if (task.IsPriorityHeading) return;

        //    ExNavigationService.Navigate<TaskDetails>("id", task.id);

        }

        private void BtnAddTask_OnClick(object sender, EventArgs e)
        {
            var model = DataContext as ProjectDetailsViewModel;
            model.AddTask();

        }

        private void BtnEditProject_OnClick(object sender, EventArgs e)
        {
            var model = DataContext as ProjectDetailsViewModel;
           // ExNavigationService.Navigate<AddEditProject>("projectId", model.Id);

        }

        private async void BtnRefresh_OnClick(object sender, EventArgs e)
        {


            var model = DataContext as ProjectDetailsViewModel;
            if (model.IsBusy) return;
            if (!await model.CheckInternetConnection(true)) return;

            model.IsBusy = true;
            model.LoadData(true);
        }
    }
}
