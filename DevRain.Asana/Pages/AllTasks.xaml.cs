using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using DevRain.Asana.API;
using DevRain.Asana.API.Data.Models;
using DevRain.Asana.API.Storage;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;

namespace DevRain.Asana.Pages
{
	public partial class AllTasks : PhoneApplicationPage
	{
		public AllTasks()
		{
			InitializeComponent();

		}



		

		private void JumpList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count() == 0) return;

			var task = e.AddedItems[0] as AsanaTask;
			
			ExNavigationService.Navigate<TaskDetails>("id", task.id);

		}
	}
}