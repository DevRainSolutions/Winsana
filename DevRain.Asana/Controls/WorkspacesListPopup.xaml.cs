using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using DevRain.Asana.API.Data.Models;
using DevRain.Asana.API.Services.Db;
using DevRain.Asana.API.Storage;
using DevRain.WP.Core.Models;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;

namespace DevRain.Asana.Controls
{
    public partial class WorkspacesListPopup
    {
        public WorkspacesListPopup()
        {
            InitializeComponent();
        }

        private Action<AsanaWorkspace> _workspaceActions;
        public async void ShowWorkspaces(Action<AsanaWorkspace> action)
        {
            _workspaceActions = action;
            
            var workspaces = await new StorageService().GetWorkspaces();

            lstWorkspaces.ItemsSource = workspaces;
            tbTitle.Text = "Select workspace";
            SystemTray.IsVisible = false;
			Show();
            this.WindowClosed += WorkspacesListPopup_WindowClosed;
        }


        private Action<AsanaProject> _projectsAction;
        public async void ShowProjects(Action<AsanaProject> action, long id)
        {
            _projectsAction = action;

            var projects = await new StorageService().GetProjects(id);

            lstWorkspaces.ItemsSource = projects;

            tbTitle.Text = "Select project";
            SystemTray.IsVisible = false;
            Show();
            this.WindowClosed += WorkspacesListPopup_WindowClosed;
        }

        void WorkspacesListPopup_WindowClosed(object sender, WindowClosedEventArgs e)
        {
            this.WindowClosed -= WorkspacesListPopup_WindowClosed;
            SystemTray.IsVisible = true;
        }

        private void LstWorkspaces_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            var item = e.AddedItems[0] as BaseAsanaDbEntity;

            IsOpen = false;

            if (item is AsanaWorkspace && _workspaceActions != null)
            {
                _workspaceActions(item as AsanaWorkspace);
            }
            else if (item is AsanaProject && _projectsAction != null)
            {
                _projectsAction(item as AsanaProject);
            }



        }
    }
}
