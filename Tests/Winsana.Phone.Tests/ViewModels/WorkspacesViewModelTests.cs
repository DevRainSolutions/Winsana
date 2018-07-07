using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Navigation;
using DevRain.Asana.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Winsana.Phone.Tests.Infrastructure;

namespace Winsana.Phone.Tests.ViewModels
{
    [TestClass]
    public class WorkspacesViewModelTests : AsanaTest
    {

        MainViewModel PrepareModel()
        {
            var v = new MainViewModel();
            v.IsBlockingProgress = false;
            v.FireOnCreate();
            v.FireOnNavigatedTo(new NavigationEventArgs(this, null));
            Thread.Sleep(2000);

            v.FireOnLoad();
            Thread.Sleep(2000);

            return v;
        }

        [TestMethod]
        public void OnNavigatedTo()
        {
            var v = new MainViewModel();
            v.FireOnCreate();
            v.FireOnNavigatedTo(new NavigationEventArgs(this, null));

        }

        [TestMethod]
        public void OnLoad()
        {
            PrepareModel();

        }

        [TestMethod]
        public void OnUnload()
        {
            var model = PrepareModel();
            model.FireOnUnload();

        }
        [TestMethod]
        public void Refresh()
        {
            var model = PrepareModel();

            model.RefreshCommand.Execute(null);

        }


    }
}
