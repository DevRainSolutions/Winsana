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
using DevRain.Windows.WinRT.Common.Core;
using DevRain.Windows.WinRT.Common.MVVM;

namespace DevRain.Winsana.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WorkspaceDetails : Page
    {
        public WorkspaceDetails()
        {
            this.InitializeComponent();
        }

        private void lstProjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;

            var project = e.AddedItems[0] as AsanaProject;

            StateService.DependencyResolverContainer.Resolve<NavigationService>().Navigate<ProjectDetails>(project.id);

        }






        private void lstTags_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;

            var tag = e.AddedItems[0] as AsanaTag;


           // ExNavigationService.Navigate<TagDetails>("id", tag.id);
        }
    }
}
