using System.Linq;
using System.Windows.Controls;
using DevRain.Asana.API;
using DevRain.Asana.API.Data.Models;
using DevRain.Asana.API.Services;
using DevRain.WP.Core.Controls.Feedback;
using DevRain.WP.Core.Helpers;
using Microsoft.Phone.Controls;

namespace DevRain.Asana.Pages
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

        }


        
        private static PivotItem myTasksPivotItemBackup;


        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
			ExNavigationService.ClearNavigationHistory();

            
            base.OnNavigatedTo(e);


            SmallProfiler.PrintLine("MainPage - OnNavigatedTo");

            if (!SettingsService.CurrentUserId.HasValue)
            {
                var item = pivot.Items.Cast<PivotItem>().FirstOrDefault(x => x.Name == "myTasksPivotItem");
                if(item != null)
                {
                    myTasksPivotItemBackup = item;
                    pivot.Items.Remove(item);
                }
            }
            else
            {
                var item = pivot.Items.Cast<PivotItem>().FirstOrDefault(x => x.Name == "myTasksPivotItem");
                 if (item == null)
                 {
                     pivot.Items.Insert(1, myTasksPivotItemBackup);
                 }
            }



			//ApplicationRatingNotificationServiceFromNokia.Show(LayoutRoot);

            //MainViewModel model = ViewModelCache.GetModel<MainViewModel>("MainPage");
            //DataContext = model;




            //if(e.NavigationMode == NavigationMode.Back)
            //{
            //    MainPage_Loaded(this, new RoutedEventArgs());
            //}



        }

        private void LstWorkspaces_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (e.AddedItems.Count == 0) return;

            var w = e.AddedItems[0] as AsanaWorkspace;

            if (w == null) return;

            ExNavigationService.Navigate<WorkspaceDetails>("id", w.id);

        }


      

        private void LstMyTasks_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            var task = e.AddedItems[0] as AsanaTask;
            ExNavigationService.Navigate<TaskDetails>("id", task.id);

        }

        private void LstUsers_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            var user = e.AddedItems[0] as AsanaUser;
            ExNavigationService.Navigate<UserDetails>("id", user.id);


        }



       
     
    }
}