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
using DevRain.WP.Core.Extensions;
using DevRain.WP.Core.Helpers;

using Microsoft.Phone.Controls;
using System.Threading.Tasks;

namespace DevRain.Asana.Pages
{
    public partial class WorkspaceDetails : PhoneApplicationPage
    {
        public WorkspaceDetails()
        {
            InitializeComponent();
        }




        private void lstProjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;

            var project = e.AddedItems[0] as AsanaProject;
            
            ExNavigationService.Navigate<ProjectDetails>("id",project.id);
            
        }

        private void BtnAddTask_OnClick(object sender, EventArgs e)
        {
            var model = this.ViewModel<WorkspaceDetailsViewModel>();
            ExNavigationService.Navigate<AddEditTask>("workspaceId", model.Id);

            
        }

        private void BtnAddProject_OnClick(object sender, EventArgs e)
        {
            var model = this.ViewModel<WorkspaceDetailsViewModel>();
            ExNavigationService.Navigate<AddEditProject>("workspaceId",model.Id);

        }

     

        private void lstTags_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;

            var tag = e.AddedItems[0] as AsanaTag;


            ExNavigationService.Navigate<TagDetails>("id", tag.id);
        }
    }
}