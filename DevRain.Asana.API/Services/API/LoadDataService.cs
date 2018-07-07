using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DevRain.Asana.API.Data;
using DevRain.Asana.API.Data.Models;
using DevRain.Asana.API.Services.Db;

using DevRain.WP.Common.Helpers;
using HuntersWP.Db;


namespace DevRain.Asana.API.Services.API
{
    public class LoadDataService
    {

        StorageService getDb()
        {
            return new StorageService();
        }

        private object _locker = new object();
        private int _pendingOperations = 2;

        public void AddOperation()
        {
            lock (_locker)
            {
                _pendingOperations++;
            }

        }

        public void RemoveOperation()
        {
            lock (_locker)
            {
                _pendingOperations--;

                if (_pendingOperations == 0)
                {
                    //Completed(this, EventArgs.Empty);
                }
            }

        }

        private bool _isUserActionRequest = false;
        public bool IsBackgroundAgent { get; set; }
        public LoadDataService()
        {

        }

        public LoadDataService(bool isUserActionRequest, int? initOperationsCount = null)
        {
            _isUserActionRequest = isUserActionRequest;
            if (initOperationsCount.HasValue)
            {
                _pendingOperations = initOperationsCount.Value;
            }
        }


        public event EventHandler<EventArgs> ProjectTasksLoaded = delegate { };

        public event EventHandler<EventArgs> SubTasksLoaded = delegate { };




        void ExecuteWithComplete(Action action, Action onComplete)
        {
            ActionHelper.ExecuteWithComplete(action, onComplete);
        }

        public async Task Execute()
        {
            await LoadWorkspaces();
            await LoadUsers();


        }

        public async Task LoadWorkspaceProjects(long id, bool firstLoad = true)
        {

            if (firstLoad)
                AddOperation();

            var projects = await new AsanaRespository().GetProjects(id, AsanaRespository.SelectProjectsFields());



            ExecuteWithComplete(async () =>
            {

                if (AsanaClient.ProcessResponse(projects, !_isUserActionRequest))
                {
                    var db = new StorageService();


                    int i = 0;
                    var dbProjects = firstLoad ? await db.GetProjects(id) : await db.GetActiveProjects(id);

                    foreach (var project in projects.Data)
                    {
                        project.Order = i;
                        var dbProject = dbProjects.FirstOrDefault(x => x.id == project.id);
                        if (dbProject != null)
                        {
                            dbProject.IsFoundInDb = true;
                        }

                        project.workspaceid = id;

                        if (firstLoad && !IsBackgroundAgent)
                        {
                            await db.Insert(project);
                        }
                        else
                        {
                            await db.Save(project);
                        }
                        i++;
                    }

                    foreach (var project in dbProjects.Where(x => !x.IsFoundInDb))
                    {
                        await DeleteProject(project);
                    }



                    if (firstLoad)
                    {
                        foreach (var project in projects.Data)
                        {
                            await LoadProjectTasks(id, project.id);
                        }
                    }

                }
            }, () =>
            {
                if (firstLoad)
                    RemoveOperation();
            });


        }

        public async Task LoadTagTasks(long tagId, bool isFirstLoad = true)
        {
            if (isFirstLoad)
                AddOperation();

            var tasks = await new AsanaRespository().GetTasksByTag(tagId, AsanaRespository.SelectTasksFields(true));

               ExecuteWithComplete(async () =>
                                                                        {
                                                                            if (AsanaClient.ProcessResponse(tasks, !_isUserActionRequest))
                                                                            {

                                                                                var dbTasks = await getDb().GetTaskTagsByTag(tagId);

                                                                                foreach (var task in tasks.Data)
                                                                                {

                                                                                    var dbTask = dbTasks.FirstOrDefault(x => x.TaskId == task.id);

                                                                                    if (dbTask != null)
                                                                                    {
                                                                                        dbTask.IsFoundInDb = true;
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        await getDb().Insert(new AsanaTagTask { TaskId = task.id, TagId = tagId,id = Guid.NewGuid().GetHashCode()});
                                                                                    }

                                                                                }


                                                                                foreach (var asanaTagTask in dbTasks.Where(x => !x.IsFoundInDb))
                                                                                {
                                                                                    await getDb().Delete(asanaTagTask);
                                                                                }


                                                                            }
                                                                        }, () =>
                                                                              {
                                                                                  if (isFirstLoad)
                                                                                      RemoveOperation();
                                                                              });


        }

        public async Task LoadProjectTasks(long workspaceId, long id, bool firstLoad = true)
        {

            if (firstLoad)
                AddOperation();

            var tasks = await new AsanaRespository().GetTasks(id, AsanaRespository.SelectTasksFields(false, true));

            ExecuteWithComplete(async () =>
            {
                if (AsanaClient.ProcessResponse(tasks, !_isUserActionRequest))
                {


                    int i = 0;

                    var project = await getDb().Find<AsanaProject>(id);
                    var dbTasks = await getDb().GetTasks(id);
                    foreach (var task in tasks.Data)
                    {
                        task.Order = i;
                        var dbTask = dbTasks.FirstOrDefault(x => x.id == task.id);
                        if (dbTask != null)
                        {
                            dbTask.IsFoundInDb = true;
                        }

                        MapperService.Map(task, id, workspaceId);

                        await getDb().Save(task);


                        i++;
                    }


                    foreach (var task in dbTasks.Where(x => !x.IsFoundInDb && !x.IsForSync))
                    {
                        await DeleteTask(task);
                    }

                    project.LastLoadDate = DateTime.Now;



                    if (firstLoad)
                    {
                        var projectTasks = (await getDb().GetTasks(id)).Where(x => !x.IsCompleted);


                        foreach (var task in projectTasks)
                        {
                            await LoadSubtasks(task.id, firstLoad);
                        }
                    }

                }
                }, () =>
                {
                    if (firstLoad)
                        RemoveOperation();


                    ProjectTasksLoaded(id, EventArgs.Empty);
                });

        }

        public async Task LoadSubtasks(long id, bool firstLoad = true)
        {

            if (firstLoad)
                AddOperation();

            var response = await new AsanaRespository().GetSubtasks(id, AsanaRespository.SelectTasksFields(false));


            ExecuteWithComplete(async () =>
            {
                if (AsanaClient.ProcessResponse(response, true))
                {
                    var DbSubtasks = await getDb().GetSubTasks(id);

                    var i = 0;
                    foreach (var subtask in response.Data)
                    {
                        subtask.Order = i;
                        var dbTask =
                            DbSubtasks.FirstOrDefault(
                                x => x.id == subtask.id);

                        if (dbTask != null)
                        {
                            dbTask.IsFoundInDb = true;
                        }

                        var task = await new StorageService().Find<AsanaTask>(id);
                        MapperService.Map(subtask,
                                          task.projectid,
                                          task.workspaceid);
                        subtask.parentId = task.id;


                        await getDb().Save(subtask);
                        i++;

                    }

                    foreach (var subtask in DbSubtasks.Where(x => !x.IsFoundInDb && !x.IsForSync))
                    {
                        await getDb().Delete(subtask);
                    }




                }
            }, () =>
            {
                if (firstLoad)
                    RemoveOperation();

                SubTasksLoaded(id, EventArgs.Empty);
            });
        }

        public async Task LoadWorkspaceTags(long id, bool firstLoad = true)
        {
            if (firstLoad)
                AddOperation();
            var tags = await new AsanaRespository().GetTags(id, new List<string> {"id", "name"});
            ;
            ;

                            ExecuteWithComplete(async () =>
                                                                                                                                                                                 {

                                                                                                                                                                                     if (AsanaClient.ProcessResponse(tags, !_isUserActionRequest))
                                                                                                                                                                                     {

                                                                                                                                                                                         var dbTgs = await getDb().GetTags(id);

                                                                                                                                                                                         foreach (var p in tags.Data)
                                                                                                                                                                                         {

                                                                                                                                                                                             var dbTag = dbTgs.FirstOrDefault(x => x.id == p.id);
                                                                                                                                                                                             if (dbTag != null)
                                                                                                                                                                                             {
                                                                                                                                                                                                 dbTag.IsFoundInDb = true;
                                                                                                                                                                                             }
                                                                                                                                                                                             p.workspaceid = id;

                                                                                                                                                                                             await getDb().Save(p);



                                                                                                                                                                                         }


                                                                                                                                                                                         foreach (var tag in dbTgs.Where(x => !x.IsFoundInDb))
                                                                                                                                                                                         {
                                                                                                                                                                                             await DeleteTag(tag);
                                                                                                                                                                                         }


                                                                                                                                                                                         if (firstLoad)
                                                                                                                                                                                         {
                                                                                                                                                                                             var currentTags = await new StorageService().GetTags(id);
                                                                                                                                                                                             foreach (var currentTag in currentTags)
                                                                                                                                                                                             {
                                                                                                                                                                                                 await LoadTagTasks(currentTag.id, true);
                                                                                                                                                                                             }
                                                                                                                                                                                         }

                                                                                                                                                                                     }
                                                                                                                                                                                 }, () =>
                                                                                                                                                                                       {
                                                                                                                                                                                           if (firstLoad)
                                                                                                                                                                                               RemoveOperation();
                                                                                                                                                                                       });




        }

        async Task DeleteTag(AsanaTag tag)
        {
            var links = await getDb().GetAsyncConnection().Table<AsanaTagTask>().Where(x => x.TagId == tag.id).ToListAsync();

            foreach (var asanaTagTask in links)
            {
                await getDb().Delete(asanaTagTask);
            }
            await getDb().Delete(tag);
        }

        public async Task DeleteTask(AsanaTask task)
        {
            var stories = await new StorageService().GetStories(task.id);

            foreach (var asanaStorey in stories)
            {
                await new StorageService().Delete(asanaStorey);
            }


            var subtasks = await new StorageService().GetSubTasks(task.id);
            foreach (var t in subtasks)
            {
                await getDb().Delete(t);
            }


            await getDb().Delete(task);


        }

        async Task DeleteProject(AsanaProject project)
        {
            var tasks = await getDb().GetTasks(project.id);

            foreach (var task in tasks)
            {
                await DeleteTask(task);
            }
            await getDb().Delete(project);
        }

        async Task DeleteWorkspace(AsanaWorkspace workspace)
        {
            var projects = await getDb().GetProjects(workspace.id);

            foreach (var asanaProject in projects)
            {
                await DeleteProject(asanaProject);
            }

            await getDb().Delete(workspace);


        }

        public async Task LoadWorkspaces(bool firstLoad = true)
        {
            var workspaces = await new AsanaRespository().GetWorkspaces();
            Debug.WriteLine("LoadWorkspaces1" + Deployment.Current.Dispatcher.CheckAccess());

            ExecuteWithComplete(async () =>
            {
                if (AsanaClient.ProcessResponse(workspaces, !_isUserActionRequest))
                {


                    Debug.WriteLine("LoadWorkspaces2" + Deployment.Current.Dispatcher.CheckAccess());

                    int i = 0;
                    var dbWorkspaces = await getDb().GetWorkspaces();
                    foreach (var workspace in workspaces.Data)
                    {
                        workspace .Order=i;
                        var dbWorkspace = dbWorkspaces.FirstOrDefault(x => x.id == workspace.id);
                        if (dbWorkspace != null)
                        {
                            dbWorkspace.IsFoundInDb = true;
                        }



                        await getDb().Save(workspace);
                        i++;

                    }

                    foreach (var workkspace in dbWorkspaces.Where(x => !x.IsFoundInDb))
                    {
                        await DeleteWorkspace(workkspace);
                    }




                    if (firstLoad)
                    {
                        foreach (var asanaWorkspace in workspaces.Data)
                        {
                            await LoadWorkspaceProjects(asanaWorkspace.id);
                            await LoadWorkspaceTags(asanaWorkspace.id);
                        }
                    }
                }
            }, () =>
            {
                if (firstLoad)
                    RemoveOperation();
            });


        }

        public async Task LoadUsers(bool firstLoad = true)
        {
            var users = await new AsanaRespository().GetUsers(new List<string>() {"workspaces"});

            ExecuteWithComplete(async () =>
            {
                if (AsanaClient.ProcessResponse(users, !_isUserActionRequest))
                {
                    Debug.WriteLine(Deployment.Current.Dispatcher.CheckAccess());



                    var dbUsers = await getDb().GetUsers();

                    foreach (var w in users.Data)
                    {
                        w.SetAvailableWorkspaces(w.workspaces);


                        var dbUser = dbUsers.FirstOrDefault(x => x.id == w.id);
                        if (dbUser != null)
                        {
                            dbUser.IsFoundInDb = true;
                        }

                        await getDb().Save(w);


                    }

                    foreach (var user in dbUsers.Where(x => !x.IsFoundInDb))
                    {
                        await getDb().Delete(user);
                    }


                }
            }, () =>
            {
                if (firstLoad)
                    RemoveOperation();
            });






        }

        public async Task LoadUserTasks(long userId, long workspaceId)
        {

            var fields = AsanaRespository.SelectTasksFields();
            fields.Add("projects");
            AddOperation();

            var tasks = await new AsanaRespository().GetUserTasks(userId, workspaceId, fields);

            ExecuteWithComplete(async () =>
            {


                if (AsanaClient.ProcessResponse(tasks, !_isUserActionRequest))
                {

                    var userTasks = (await getDb().GetTasksByUser(userId)).Where(x => x.workspaceid == workspaceId);
                    foreach (var u in tasks.Data)
                    {
                        var dbTask = userTasks.FirstOrDefault(x => x.id == u.id);
                        if (dbTask != null)
                        {
                            dbTask.IsFoundInDb = true;
                        }

                        MapperService.Map(u, workspaceId);
                        await getDb().Save(u);

                    }

                    foreach (var task in userTasks.Where(x => !x.IsFoundInDb))
                    {
                        task.assigneeid = 0;
                        await getDb().Save(task);
                    }


                }
            }, () =>
            {
                RemoveOperation();
            });


        }

        public async Task<Tuple<AsanaTask, bool>> SyncTask(AsanaTask task)
        {
            var dto = MapperService.CreateTaskDTO(0, task.name, task.notes, task.projectid, task.assignee_status,
                                                  task.assigneeid, task.completed, task.due_on, task.GetFollowers());


            var response = await new AsanaRespository().CreateTask(task.workspaceid, dto, task.parentId);
            

            if (AsanaClient.ProcessResponse(response, !_isUserActionRequest))
            {

                MapperService.Map(response.Data, task.projectid, task.workspaceid);


                return new Tuple<AsanaTask, bool>(response.Data, true);
            }
            return new Tuple<AsanaTask, bool>(null, false);
        }

        public async Task<bool> SyncTasks()
        {

            var tasks = await new StorageService().GetTasksForSync();

            if (tasks.Count == 0) return false;
            var newTasks = new List<AsanaTask>();




            foreach (var task in tasks)
            {
                var dbTask = await SyncTask(task);

                if (dbTask.Item2)
                {
                    await getDb().Delete(task);
                }

                if (dbTask.Item1 != null)
                {
                    newTasks.Add(dbTask.Item1);
                }



            }


            foreach (var asanaTask in newTasks)
            {
                await getDb().Insert(asanaTask);
            }

            return true;

        }
    }
}
