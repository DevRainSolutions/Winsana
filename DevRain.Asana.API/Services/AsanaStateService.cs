using System.IO.IsolatedStorage;
using DevRain.WP.Common.IoC;
using DevRain.WP.Common.Services;


namespace DevRain.Asana.API.Services
{
	public class AsanaStateService
	{
		static IApplicationSettingsService GetSettingsService()
		{
			return new ApplicationSettingsService();
		}


		//  public static string ApiKey = "Frr7pZb.RlkvRhuJEv3ATp46NaKa9nUt";
		public static string AuthToken { get; private set; }
		public static string ApiKey { get; private set; }

		// public static AsanaWorkspace CurrentWorkspace { get; set; }
		//public static AsanaProject CurrentProject { get; set; }
		//public static long UserId { get; set; }

		public static void Initialize()
		{
			//#if DEBUG
			//            ApiKey = "2IPsBtr.EBOluTYUi2mwwz86RkWKNHr6";
			//            return;
			//#endif
			AuthToken = GetSettingsService().GetSetting("AuthToken", "");
			ApiKey = GetSettingsService().GetSetting("ApiKey", "");

		}


		public static void SetAuthToken(string key)
		{
			AuthToken = key;

			GetSettingsService().SetSetting("AuthToken", key);
		}

		public static void ResetAuthData()
		{
			GetSettingsService().RemoveSetting("AuthToken");
			GetSettingsService().RemoveSetting("ApiKey");
			SettingsService.LoggedUserId = 0;
			AuthToken = null;
			ApiKey = null;
		}

	

		public static bool IsSetAuthToken
		{
			get
			{

				return !string.IsNullOrEmpty(AuthToken);
			}
		}

		public static bool IsSetApiKey
		{
			get
			{

				return !string.IsNullOrEmpty(ApiKey);
			}
		}


		public static bool? NeedToSyncData { get; set; }

	}
}
