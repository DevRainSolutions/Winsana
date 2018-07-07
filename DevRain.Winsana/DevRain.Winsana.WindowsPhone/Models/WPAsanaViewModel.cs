using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevRain.Windows.WinRT.Common.MVVM;
using DevRain.Winsana.Core.Models;
using DevRain.Winsana.Pages;

namespace DevRain.Winsana.Models
{
    public class WPAsanaViewModel : AsanaViewModel
    {

        public WPAsanaViewModel()
        {
            GoToSettingsPageCommand = new DelegateCommand((o) =>
            {
                NavigationService.Navigate<SettingsPage>();

            });

            GoToMainPageCommand = new DelegateCommand((o) =>
            {
                NavigationService.Navigate<MainPage>();
            });

            GoToAllTasksPageCommand = new DelegateCommand((o) =>
            {
               // ExNavigationService.Navigate<AllTasks>();
            });

            GoToAboutPageCommand = new DelegateCommand((o) =>
            {
              NavigationService.Navigate<AboutPage>();

            });
            SetDefaultProgressText();
        }

    }
}
