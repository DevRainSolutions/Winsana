using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.Storage;
using DevRain.Asana.API.Data.Models;
using DevRain.Asana.API.Services;
using SQLite;


namespace HuntersWP.Db
{


    public class DbService
    {
        public static string DB_PATH = Path.Combine(Path.Combine(ApplicationData.Current.LocalFolder.Path, "db2.sqlite"));

        public SQLiteConnection GetConnection()
        {
            string dbRootPath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            return new SQLiteConnection(Path.Combine(dbRootPath, DB_PATH));
        }

        public SQLiteAsyncConnection GetAsyncConnection()
        {
            string dbRootPath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            return new SQLiteAsyncConnection(Path.Combine(dbRootPath, DB_PATH), openFlags: SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache | SQLiteOpenFlags.NoMutex, storeDateTimeAsTicks: true);
        }

      


        public  async Task ClearDb()
        {
            using (var c = GetConnection())
            {
                c.DropTable<AsanaWorkspace>();
                c.DropTable<AsanaTask>();
                c.DropTable<AsanaProject>();
                c.DropTable<AsanaUser>();
                c.DropTable<AsanaTag>();
                c.DropTable<AsanaTagTask>();
                c.DropTable<AsanaStory>();


                
            }

            Initialize();

            AsanaStateService.NeedToSyncData = true;

            

        }

     

        public void Initialize()
        {
            using (var c = GetConnection())
            {
                
                SQLite3.Config(SQLite3.ConfigOption.MultiThread);

                c.CreateTable<AsanaWorkspace>();
                c.CreateTable<AsanaTask>();
                c.CreateTable<AsanaProject>();
                c.CreateTable<AsanaUser>();
                c.CreateTable<AsanaTag>();
                c.CreateTable<AsanaTagTask>();
                c.CreateTable<AsanaStory>();

            }
        }
    }
}
