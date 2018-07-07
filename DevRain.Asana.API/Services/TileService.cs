using System.Linq;
using DevRain.Asana.API.Services.Db;

using Microsoft.Phone.Shell;
using System.Threading.Tasks;

namespace DevRain.Asana.API.Services
{
    public class TileService
    {

        public static async void UpdateMainTile()
        {
            if (!SettingsService.UpdateMainTile) return;

            ShellTile tile = ShellTile.ActiveTiles.First();
            if (tile != null)
            {
                StandardTileData tileData = new StandardTileData
                {
                    Count= await new StorageService().GetDueTodayTasksCount()
                };

                tile.Update(tileData);
            }
        }

        public static void ClearMainTile()
        {
            Task.Factory.StartNew(() =>
                                      {
                                          ShellTile tile = ShellTile.ActiveTiles.First();
                                          if (tile != null)
                                          {
                                              tile.Update(new StandardTileData(){Count = 0});
                                          }
                                      });
        }
    }
}
