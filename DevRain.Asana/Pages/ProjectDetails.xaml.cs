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
using DevRain.Asana.Services;
using DevRain.Asana.ViewModels;

using Microsoft.Phone.Controls;

namespace DevRain.Asana.Pages
{
    public partial class ProjectDetails : PhoneApplicationPage
    {
        public ProjectDetails()
        {
            InitializeComponent();

        }


     
        //async void LoadStories(long id)
        //{
            
        //    var model = DataContext as ProjectDetailsViewModel;

        //    model.AddOperation();
        //    var stories = await AsanaClient.SendRequest(() => new AsanaRespository().GetStoriesByProject(id));

        //    if (AsanaClient.ProcessResponse(stories))
        //    {
        //        model.Stories.AddRange(stories.Data);
        //    }
        //    model.RemoveOperation();
        //}

       

        private void LstActiveTasks_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            var task = e.AddedItems[0] as AsanaTask;

            if(task.IsPriorityHeading) return;
            
            ExNavigationService.Navigate<TaskDetails>("id", task.id);

        }

        private void BtnAddTask_OnClick(object sender, EventArgs e)
        {
            var model = DataContext as ProjectDetailsViewModel;
            model.AddTask();
            //var newProject = new AsanaProject() {name = "new proj"};
            //Scheduler.Default.Schedule(() =>
            //                               {
            //                                   var result =
            //                                       new AsanaRespository().CreateProject(
            //                                           AsanaStateService.CurrentWorkspace, newProject);
            //                               });

        }

        private void BtnEditProject_OnClick(object sender, EventArgs e)
        {
            var model = DataContext as ProjectDetailsViewModel;
            ExNavigationService.Navigate<AddEditProject>("projectId", model.Id);

        }

        private void BtnRefresh_OnClick(object sender, EventArgs e)
        {


            var model = DataContext as ProjectDetailsViewModel;
            if (model.IsBusy) return;
            if (!model.CheckInternetConnection(true)) return;

            model.IsBusy = true;
            model.LoadData(true);
        }
    }
}