using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using DevRain.Asana.API;
using DevRain.Asana.API.Data.Models;
using DevRain.Asana.Models;
using DevRain.Asana.Services;
using DevRain.Asana.ViewModels;
using DevRain.WP.Common.Extensions;
using DevRain.WP.Core.Extensions;
using DevRain.WP.Core.MVVM.Core;
using DevRain.WP.Core.MVVM.Messaging;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;

namespace DevRain.Asana.Pages
{
    public partial class TaskDetails : PhoneApplicationPage, IMessageReceiver<FocusListMessage>, IMessageReceiver<GoToFirstPivotItemMessage>
    {

        

        public TaskDetails()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            pivot.SelectedIndex = 1;
            Bootstrapper.Current.Container.Resolve<IMessagePublisher>().Register<GoToFirstPivotItemMessage>(this);
            Bootstrapper.Current.Container.Resolve<IMessagePublisher>().Register<FocusListMessage>(this);
            grdNeedSyncing.Visibility = Visibility.Visible;
        }



        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Bootstrapper.Current.Container.Resolve<IMessagePublisher>().Unregister<FocusListMessage>(this);
            Bootstrapper.Current.Container.Resolve<IMessagePublisher>().Unregister<GoToFirstPivotItemMessage>(this);

        }
      

        private void BtnEdit_OnClick(object sender, EventArgs e)
        {
            var model = DataContext as TaskDetailsViewModel;
            model.GoToEditTask(model.Id);
        }

      
        private async void BtnComplete_OnClick(object sender, EventArgs e)
        {
            var model = DataContext as TaskDetailsViewModel;
            await model.SetCompleted(true);

        }

        private async void BtnNotComplete_OnClick(object sender, EventArgs e)
        {
            var model = DataContext as TaskDetailsViewModel;
            await model.SetCompleted(false);

        }

        private void Pivot_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            var model = DataContext as TaskDetailsViewModel;

            var pivotItem = pivot.SelectedItem as PivotItem;

            model.IsTaskInfoActive = pivotItem.Name == taskDetailsPivotItem.Name;
            model.IsCommentsInfoActive = pivotItem.Name == commentsPivotItem.Name;
            model.IsSubtasksActive = pivotItem.Name == subtasksPivotItem.Name;
        }


  

        public void OnReceive(FocusListMessage message)
        {
            if(message.IsSubtasks)
            {
                lstSubtasks.Focus();
                return;
            }

            lstComments.Focus();
            tbComment.Text = "";
            lstComments.UpdateLayout();
            var scroll = lstComments.Descendents().OfType<ScrollViewer>().FirstOrDefault();
            if (scroll != null)
            {
                scroll.ScrollToVerticalOffset(0);
            }
        }

        private void BtnAddSubtask_OnClick(object sender, EventArgs e)
        {
            var model = DataContext as TaskDetailsViewModel;
            model.AddSubtask();

        }

        private void LstSubtasks_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            var task = e.AddedItems[0] as AsanaTask;
            var model = DataContext as TaskDetailsViewModel;
            model.EditSubtask(task.id);


        }

        public void OnReceive(GoToFirstPivotItemMessage message)
        {
            pivot.SelectedIndex = 1;
        }


        private void TbNewSubTask_OnKeyUp(object sender, KeyEventArgs e)
        {
            
            if(e.Key == Key.Enter)
            {
                var model = DataContext as TaskDetailsViewModel;
                model.AddNewSubTask();
            }
        }
    }
}