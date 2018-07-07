using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevRain.Asana.Controls.Tiles;
using DevRain.Asana.Pages;
using DevRain.WP.Common.Extensions;
using DevRain.WP.Common.Helpers;
using DevRain.WP.Core.Extensions;
using DevRain.WP.Core.Helpers;
using Telerik.Windows.Controls;

namespace DevRain.Asana.Services
{
	public static class PinService
	{
		static string _detailsUri = "/Pages/{0}.xaml?id={1}";

		public static string GetWorkspaceDetailsUri(long id)
		{
			return _detailsUri.FormatWith(typeof (WorkspaceDetails).Name, id);
		}
		public static string GetProjectDetailsUri(long id)
		{
			return _detailsUri.FormatWith(typeof(ProjectDetails).Name, id);
		}
		public static string GetUserDetailsUri(long id)
		{
			return _detailsUri.FormatWith(typeof(UserDetails).Name, id);
		}


		public static bool Exists(string uri)
		{
			return LiveTileHelper.GetTile(new Uri(uri, UriKind.RelativeOrAbsolute)) != null;
		}

		public static void RemoveTile(string url)
		{
			var uri = new Uri(url, UriKind.RelativeOrAbsolute);

			var tile = LiveTileHelper.GetTile(uri);
			if (tile != null)
			{
				tile.Delete();
			}

		}

		public static void CreateUpdateStartTileAsync(string name, string summary, string url, bool remove)
		{
			System.Threading.Tasks.Task.Factory.StartNew(() =>
				                                             {
																 DispatcherHelper.OnUi(()=>
																	                       {
																							   CreateUpdateStartTile(name, summary, url, remove);
																	                       });
				                                             });
		}

		public static void CreateUpdateStartTile(string name, string summary,string url, bool remove)
		{
			if(remove)
				RemoveTile(url);

			var control = new ItemTileControl();
			control.Initialize(name,summary);
			
			 RadExtendedTileData extendedData = new RadExtendedTileData()
            {
                VisualElement = control,
				Title = ApplicationManifestHelper.Read().Title,
			
            };


			 LiveTileHelper.CreateOrUpdateTile(extendedData, new Uri(url, UriKind.RelativeOrAbsolute));
			
		}

	}
}
