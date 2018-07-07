using DevRain.WP.Common.Services;


namespace DevRain.Asana.API.Services
{
    public class SettingsService
    {

		static IApplicationSettingsService GetSettingsService()
		{
			return new ApplicationSettingsService();
		}

        public static long? DefaultWorkspaceId
        {
            get {

                var res = GetSettingsService().GetSetting<long>("DefaultWorkspaceId", 0);

                if (res != 0) return res;
                return null;

            }
            set
            {
                GetSettingsService().SetSetting("DefaultWorkspaceId",value);
                
            }
        }

        public static bool UpdateMainTile
        {
            get
            {
                return GetSettingsService().GetSetting<bool>("UpdateMainTile", true);

            }
            set
            {
                GetSettingsService().SetSetting("UpdateMainTile", value);
            }
        }


        public static long? CurrentUserId
        {
            get
            {
                var res = GetSettingsService().GetSetting<long>("CurrentUserId", 0);

                if (res != 0) return res;
                return null;
                

            }
            set
            {
                GetSettingsService().SetSetting("CurrentUserId", value);
            }
        }

		public static long LoggedUserId
		{
			get { return GetSettingsService().GetSetting<long>("LoggedUserId", 0); }
			set
			{
				GetSettingsService().SetSetting("LoggedUserId", value);
			}
		}

    }
}
