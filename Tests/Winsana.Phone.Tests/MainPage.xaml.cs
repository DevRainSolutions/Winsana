using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Silverlight.Testing;

namespace Winsana.Phone.Tests
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPageLoaded;
        }

        void MainPageLoaded(object sender, RoutedEventArgs e)
        {
            SystemTray.IsVisible = false;
            var testPage = UnitTestSystem.CreateTestPage() as IMobileTestPage;            
            BackKeyPress += (o, a) => a.Cancel = testPage.NavigateBack();       
            (Application.Current.RootVisual as PhoneApplicationFrame).Content = testPage;
        }
    }
}