
using DevRain.Windows.WinRT.Common.Core;
using DevRain.Windows.WinRT.Common.Services;



namespace DevRain.Asana.API.Services
{
	public class AsanaStateService
	{
		static IApplicationSettingsService GetSettingsService()
		{
		    return StateService.DependencyResolverContainer.Resolve<IApplicationSettingsService>();
		}


		//  public static string ApiKey = "Frr7pZb.RlkvRhuJEv3ATp46NaKa9nUt";
	    public static string AuthToken
	    {
	        get
	        {
                return GetSettingsService().GetValue("AuthToken", "");
	        }
	        set
	        {
                GetSettingsService().SetValue("AuthToken","value");
	        }
	    }


		// public static AsanaWorkspace CurrentWorkspace { get; set; }
		//public static AsanaProject CurrentProject { get; set; }
		//public static long UserId { get; set; }




		public static void SetAuthToken(string key)
		{
			AuthToken = key;

			GetSettingsService().SetValue("AuthToken", key);
		}

		public static void ResetAuthData()
		{
			SettingsService.LoggedUserId = 0;
			AuthToken = null;
		}

	

		public static bool IsSetAuthToken
		{
			get
			{

				return !string.IsNullOrEmpty(AuthToken);
			}
		}




		public static bool? NeedToSyncData { get; set; }

	}
}
