using DevRain.Windows.WinRT.Common.Core;
using DevRain.Windows.WinRT.Common.Services;



namespace DevRain.Asana.API.Services
{
    public class SettingsService
    {

		static IApplicationSettingsService GetSettingsService()
		{
		    return StateService.DependencyResolverContainer.Resolve<IApplicationSettingsService>();
		}

        public static long? DefaultWorkspaceId
        {
            get {

                var res = GetSettingsService().GetValue<long>("DefaultWorkspaceId", 0);

                if (res != 0) return res;
                return null;

            }
            set
            {
                GetSettingsService().SetValue("DefaultWorkspaceId",value);
                
            }
        }

        public static bool UpdateMainTile
        {
            get
            {
                return GetSettingsService().GetValue<bool>("UpdateMainTile", true);

            }
            set
            {
                GetSettingsService().SetValue("UpdateMainTile", value);
            }
        }


        public static long? CurrentUserId
        {
            get
            {
                var res = GetSettingsService().GetValue<long>("CurrentUserId", 0);

                if (res != 0) return res;
                return null;
                

            }
            set
            {
                GetSettingsService().SetValue("CurrentUserId", value);
            }
        }

		public static long LoggedUserId
		{
            get { return GetSettingsService().GetValue<long>("LoggedUserId", 0); }
			set
			{
				GetSettingsService().SetValue("LoggedUserId", value);
			}
		}

    }
}
