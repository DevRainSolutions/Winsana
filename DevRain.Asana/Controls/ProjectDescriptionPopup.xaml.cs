using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using DevRain.WP.Core.Helpers;

using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace DevRain.Asana.Controls
{
	public partial class ProjectDescriptionPopup
	{
		public ProjectDescriptionPopup()
		{
			InitializeComponent();
		}

		public void Initialize(string text)
		{
			this.Width = DeviceHelper.GetCurrentApplicationFrame().ActualWidth;
			this.Height = DeviceHelper.GetCurrentApplicationFrame().ActualHeight;
			SystemTray.IsVisible = false;
			tbDescription.Text = text;

		}

		protected override void OnWindowClosed(Telerik.Windows.Controls.WindowCloseReason reason)
		{
			base.OnWindowClosed(reason);
			SystemTray.IsVisible = true;
		}
	}
}
