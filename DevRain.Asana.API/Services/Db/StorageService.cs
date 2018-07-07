using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Navigation;
using DevRain.Asana.API.Data.Models;

using DevRain.WP.Common.Helpers;
using HuntersWP.Db;
using SQLite;

namespace DevRain.Asana.API.Services.Db
{
    public class StorageService
    {
        DbService _dbService = new DbService();

        public SQLiteAsyncConnection GetAsyncConnection()
        {
            return _dbService.GetAsyncConnection();
        }

        public async Task Insert<T>(T db)
        {
            await GetAsyncConnection().InsertAsync(db);
        }

        static object _locker = new object();

        public async Task Save<T>(T dbEntity) where T:BaseAsanaDbEntity,new()
        {
            var c = _dbService.GetAsyncConnection();

            if ((await c.FindAsync<T>(dbEntity.id) != null))
            {
                await c.UpdateAsync(dbEntity);
            }
            else
            {
                await c.InsertAsync(dbEntity);
            }
          
      
        }

        public async Task<T> Find<T>(object id) where T : BaseAsanaDbEntity, new()
        {
            return await GetAsyncConnection().FindAsync<T>(id);
        }

        public async Task Delete<T>(T dbEntity)
        {
            await GetAsyncConnection().DeleteAsync(dbEntity);
        }

        public async Task<int> CountWorkspaces()
        {
            return await GetAsyncConnection().Table<AsanaWorkspace>().CountAsync();
        }

        public async Task<long> GetLocalTaskId()
        {

            var r = await GetAsyncConnection().Table<AsanaTask>().OrderBy(x => x.id).FirstOrDefaultAsync();


            if (r != null)
            {
                return r.id - 1;
            }
            return 0;

        }


        public async Task<List<AsanaWorkspace>> GetWorkspaces()
        {
            return await GetAsyncConnection().Table<AsanaWorkspace>().ToListAsync();
        }


        public async Task<List<AsanaUser>> GetUsers()
        {
            return await GetAsyncConnection().Table<AsanaUser>().ToListAsync();

        }



        public async Task<List<AsanaUser>> GetUsers(long workspaceId)
        {
            var allUsers = await GetUsers();
            var users = new List<AsanaUser>();


            foreach (var asanaUser in allUsers)
            {
                if (asanaUser.GetAvailableWorkspaces().Contains(workspaceId))
                {
                    users.Add(asanaUser);
                }
            }
            return users;
        }


        public async Task<List<AsanaProject>> GetActiveProjects(long workspaceId)
        {
            return await GetAsyncConnection().Table<AsanaProject>().Where(x => x.workspaceid == workspaceId && x.archived == false).OrderBy(x => x.Order).ToListAsync();

        }



        public async Task<List<AsanaProject>> GetProjects(long workspaceId)
        {
            return (await GetAsyncConnection().Table<AsanaProject>().Where(x => x.workspaceid == workspaceId).ToListAsync()).OrderByDescending(x => x.archived == false).ThenBy(x => x.Order).ToList();

        }



        public async Task<List<AsanaTag>> GetTags(long id)
        {

            return await GetAsyncConnection().Table<AsanaTag>().Where(x => x.workspaceid == id).OrderByDescending(x => x.created_at).ToListAsync();

        }

        public async Task<List<AsanaProject>> GetProjectsForRefresh()
        {
            return await GetAsyncConnection().Table<AsanaProject>().OrderBy(x => x.LastLoadDate).Take(4).ToListAsync();

        }



        public async Task<List<AsanaTagTask>> GetTaskTagsByTask(long taskId)
        {

            return await GetAsyncConnection().Table<AsanaTagTask>().Where(x => x.TaskId == taskId).ToListAsync();

        }

        public async Task<List<AsanaTask>> GetTasksForSync()
        {
            return await GetAsyncConnection().Table<AsanaTask>().Where(x => x.IsForSync).ToListAsync();

       }


        public async Task<List<AsanaTask>> GetTasks(long projectId)
        {

            return await GetAsyncConnection().Table<AsanaTask>().Where(x => x.projectid == projectId && x.parentId == null)
                  .OrderBy(x => x.Order).ToListAsync();

        }

        public async Task<List<AsanaTask>> GetSubTasks(long taskId)
        {

            return
                (await GetAsyncConnection().Table<AsanaTask>().Where(x => x.parentId == taskId).ToListAsync())
                    .OrderByDescending(x => x.completed == false)
                    .ThenBy(x => x.Order).ToList();

        }

        public async Task<List<AsanaStory>> GetStories(long id)
        {
            var stories = await GetAsyncConnection().Table<AsanaStory>().Where(x => x.targetId == id).ToListAsync();

            foreach (var asanaStorey in stories)
            {
                asanaStorey.created_by = await Find<AsanaUser>(asanaStorey.userId);
            }

            return stories.OrderByDescending(x => x.created_at).ToList();
        }



        public async Task<List<AsanaTagTask>> GetTaskTagsByTag(long tagId)
        {
            return await GetAsyncConnection().Table<AsanaTagTask>().Where(x => x.TagId == tagId).ToListAsync();

        }

        public async Task<List<AsanaTask>> GetAllSubTasks(long taskId)
        {
            return await GetAsyncConnection().Table<AsanaTask>().Where(x => x.parentId == taskId).ToListAsync();


        }

        public async Task<List<AsanaTask>> GetActiveTasksByUser(long userId)
        {

            return await GetAsyncConnection().Table<AsanaTask>().Where(x => x.assigneeid == userId && x.completed == false && x.IsPriorityHeading ==false).OrderByDescending(x => x.modified_at).ToListAsync();

        }


        public async Task< List<AsanaTask>> GetActiveWorkspacesTasks(long workspaceId)
        {
            return await GetAsyncConnection().Table<AsanaTask>().Where(x => x.workspaceid == workspaceId && x.IsPriorityHeading == false && x.completed == false)
      .OrderBy(x => x.Order).ToListAsync();

        }

        public async Task<List<AsanaTask>> GetTasksByUser(long userId)
        {
            return await GetAsyncConnection().Table<AsanaTask>().Where(x => x.assigneeid == userId && x.IsPriorityHeading == false)
      .OrderBy(x => x.Order).ToListAsync();

        }


        public async Task<List<AsanaTask>> GetDueTomorrowTasks()
        {
            DateTime givenDate = DateTime.Today;
            var tomorrow = givenDate.AddDays(1);
            return
                (await
                    GetAsyncConnection()
                        .Table<AsanaTask>()
                        .Where(
                            x =>
                                x.due_on != null && x.due_on > givenDate && x.due_on <= tomorrow && x.completed == false &&
                                x.IsPriorityHeading == false)
                        .ToListAsync())
                    .OrderBy(x => x.due_on).ThenBy(x => x.Order).ToList();


        }





        AsyncTableQuery<AsanaTask>  GetDueTodayTasksQuery()
        {
            return GetAsyncConnection().Table<AsanaTask>().Where(
                    x => x.due_on != null && x.due_on <= DateTime.Today && x.completed == false && x.IsPriorityHeading == false);
        }


        public async Task<List<AsanaTask>> GetDueTodayTasks()
        {
            return await GetDueTodayTasksQuery().OrderBy(x => x.due_on).OrderBy(x => x.Order).ToListAsync();


        }


        public async Task<List<AsanaTask>> GetDueWeekendTasks()
        {
            DateTime givenDate = DateTime.Today;
            DateTime dueDate = DateTimeHelper.GetEndOfWeek(givenDate);

            var tomorrow = givenDate.AddDays(1);

            return (await GetAsyncConnection().Table<AsanaTask>().Where(x => x.due_on != null && x.due_on > givenDate && x.due_on > tomorrow && x.due_on <= dueDate && x.completed == false && x.IsPriorityHeading == false).ToListAsync())
                     .OrderBy(x => x.due_on).ThenBy(x => x.Order).ToList();
        }


        public async Task<List<AsanaTask>> GetTasksByTag(long tagId)
        {

            var c = GetAsyncConnection();

            var r = (await c.Table<AsanaTagTask>().Where(x => x.TagId == tagId).ToListAsync()).Join(
                    (await c.Table<AsanaTask>().Where(x => x.IsPriorityHeading == false).ToListAsync()), x => x.TaskId, x => x.id, (x, y) => y);

            return r.OrderByDescending(x => x.modified_at).ToList();

        }

        public async Task<List<AsanaTag>> GetTagsByTask(long taskId)
        {

            var c = GetAsyncConnection();

            var r =
                (await c.Table<AsanaTagTask>().Where(x => x.TaskId == taskId).ToListAsync()).Join(
                    (await c.Table<AsanaTag>().ToListAsync()), x => x.TagId, x => x.id, (x, y) => y).ToList();

            return r;


        }
        


        public async Task<int> GetDueTodayTasksCount()
        {
            return await GetDueTodayTasksQuery().CountAsync();
        }

    }
}
