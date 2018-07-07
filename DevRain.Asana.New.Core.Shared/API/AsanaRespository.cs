using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using DevRain.Asana.API.Data.Models;
using DevRain.Asana.API.Data.Models.DTO;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace DevRain.Asana.API.Data
{
	public class AsanaRespository
	{
		private static readonly string _asanaBaseUrl;
        


		static AsanaRespository()
		{
			_asanaBaseUrl = "https://app.asana.com/api/1.0/";
		}



		public static List<string> SelectProjectsFields()
		{
			return new List<string> { "id", "name", "notes", "archived", "created_at", "modified_at" };
		}

		public static List<string> SelectTasksFields(bool includeTags = false, bool includeFollowers = false)
		{
			var fields = new List<string>()
                       {
                           "id",
                           "name",
                           "notes",
                           "completed",
                           "due_on",
                           "created_at",
                           "completed_at",
                           "assignee",
                           "assignee_status",
                           "modified_at",
                           "projects"
                       };

			if (includeFollowers)
				fields.Add("followers");

			if (includeTags)
				fields.Add("tags");

			fields.Add("parent");

			return fields;
		}

		public async Task<AStoriesResponse> GetStoriesByProject(long id)
		{
		    return await SendRequest<AStoriesResponse>(GetProjectStoriesUrl(id));
		}


        public async Task<AStoriesResponse> GetStoriesByTask(long id)
		{
            return await SendRequest<AStoriesResponse>(GetTaskStoriesUrl(id));
		}

//        public AsanaResponse<AsanaProject> CreateProject(long workspaceId, AsanaProject newProject)
//        {
//            return UseRequest<AsanaProject>(
//                GetWorkspaceProjectsUrl(workspaceId),
//                request =>
//                {
//                    newProject.workspaceid = workspaceId;
//                    request.Method = Method.POST;

//                    var fields = new List<string> { "name", "notes", "archived" };

//                    request.AddObject(newProject, fields.ToArray());
//                });
//        }

//        public AsanaResponse<AsanaTask> GetTask(long id)
//        {
//            return UseRequest<AsanaTask>(
//                string.Format("tasks/{0}", id),
//                request =>
//                {
//                    request.Method = Method.GET;

//                });
//        }

//        public AsanaResponse<AsanaProject> GetProject(long id)
//        {
//            return UseRequest<AsanaProject>(
//                string.Format("projects/{0}", id),
//                request =>
//                {
//                    request.Method = Method.GET;
//                });
//        }

//        public AsanaResponse<AsanaTask> CreateTask(long workspaceId, AsanaTaskDTO newTask, long? parentId)
//        {
//            var fields = new List<string> { "name", "notes", "completed", "due_on", "assignee_status" };

//            if (newTask.projects > 0 && parentId == null)
//            {
//                fields.Add("projects");
//            }

//            if (newTask.due_on != null)
//            {
//                fields.Add("due_on");
//            }



//            fields.Add("assignee");


//            var url = parentId.HasValue ? GetSubTasksUrl(parentId.Value) : GetWorkspaceTasksUrl(workspaceId);


//            return UseRequest<AsanaTask>(url,
//request =>
//{
//    request.Method = Method.POST;

//    if (newTask.followers != null)
//    {
//        foreach (var asanaFollower in newTask.followers)
//        {
//            request.AddParameter(string.Format("followers[{0}]", newTask.followers.IndexOf(asanaFollower)), asanaFollower.id);
//        }
//    }
//    request.AddObject(newTask, fields.ToArray());
//});


//        }


//        public AsanaResponse<AStory> CreateTaskStory(long taskId, AStory story)
//        {
//            return UseRequest<AStory>(
//                GetTaskStoriesUrl(taskId),
//                request =>
//                {

//                    var fields = new List<string> { "text" };

//                    request.Method = Method.POST;
//                    request.AddObject(story, fields.ToArray());
//                });
//        }




//        public AsanaResponse<AsanaTask> UpdateTask(AsanaTaskDTO newTask)
//        {
//            return UseRequest<AsanaTask>(
//                string.Format("tasks/{0}", newTask.id),
//                request =>
//                {

//                    var fields = new List<string> { "name", "notes", "completed", "assignee_status" };

//                    fields.Add("due_on");

//                    fields.Add("assignee");

//                    request.Method = Method.PUT;
//                    request.AddObject(newTask, fields.ToArray());
//                });
//        }

//        public AsanaResponse<AsanaTask> DeleteTask(long id)
//        {
//            return UseRequest<AsanaTask>(
//                string.Format("tasks/{0}", id),
//                request =>
//                {

//                    request.Method = Method.DELETE;

//                });
//        }




//        public AsanaResponse<AsanaTag> CreateTag(long workspaceId, AsanaTag newTag)
//        {
//            return UseRequest<AsanaTag>(
//                GetWorkspaceTagsUrl(workspaceId),
//                request =>
//                {

//                    request.Method = Method.POST;

//                    var fields = new List<string>();
//                    fields.Add("name");

//                    request.AddObject(newTag, fields.ToArray());
//                });
//        }

//        public AsanaResponse<BaseEntity> AddTag(long taskId, long tagId)
//        {
//            return UseRequest<BaseEntity>(string.Format("tasks/{0}/addTag", taskId), request =>
//            {
//                request.Method = Method.POST;

//                request.AddParameter("tag", tagId);


//            });

//        }

//        public AsanaResponse<BaseEntity> RemoveTag(long taskId, long tagId)
//        {
//            return UseRequest<BaseEntity>(string.Format("tasks/{0}/removeTag", taskId), request =>
//            {
//                request.Method = Method.POST;

//                request.AddParameter("tag", tagId);


//            });

//        }

//        public AsanaResponse<BaseEntity> AddFollowers(long taskId, List<long> followers)
//        {
//            return UseRequest<BaseEntity>(string.Format("tasks/{0}/addFollowers", taskId), request =>
//            {
//                request.Method = Method.POST;

//                for (int i = 0; i < followers.Count; i++)
//                {
//                    request.AddParameter(
//            string.Format("followers[{0}]", i), followers[i]);
//                }


//            });

//        }

//        public AsanaResponse<BaseEntity> RemoveFollowers(long taskId, List<long> followers)
//        {
//            return UseRequest<BaseEntity>(string.Format("tasks/{0}/removeFollowers", taskId), request =>
//            {
//                request.Method = Method.POST;

//                for (int i = 0; i < followers.Count; i++)
//                {
//                    request.AddParameter(
//            string.Format("followers[{0}]", i), followers[i]);
//                }


//            });

//        }



//        public AsanaResponse<BaseEntity> ChangeTaskProject(long taskId, long oldProjectId, long newProjectId)
//        {
//            AsanaResponse<BaseEntity> result = null;

//            if (oldProjectId > 0)
//            {
//                result = UseRequest<BaseEntity>(string.Format("tasks/{0}/removeProject", taskId), request =>
//                                                                                                      {
//                                                                                                          request.Method = Method.POST;
//                                                                                                          request.AddParameter(
//                                                                                                              "project", oldProjectId);
//                                                                                                      });
//            }


//            if (newProjectId > 0 && (result == null || result.Errors == null))
//            {
//                return UseRequest<BaseEntity>(string.Format("tasks/{0}/addProject", taskId), request =>
//                                                                                                 {
//                                                                                                     request.Method = Method.POST;
//                                                                                                     request.AddParameter(
//                                                                                                         "project", newProjectId);
//                                                                                                 });

//            }
//            else
//            {
//                return result;
//            }
//        }


//        public AsanaResponse<AsanaProject> UpdateProject(AsanaProject projectToUpdate)
//        {
//            return UseRequest<AsanaProject>(
//                string.Format("projects/{0}", projectToUpdate.id),
//                request =>
//                {
//                    var fields = new List<string> { "name", "notes", "archived" };

//                    request.AddObject(projectToUpdate, fields.ToArray());
//                    request.Method = Method.PUT;
//                });
//        }




//        public AsanaResponse<AsanaTag> UpdateTag(AsanaTag tagToUpdate)
//        {
//            return UseRequest<AsanaTag>(
//                string.Format("tags/{0}", tagToUpdate.id),
//                request =>
//                {
//                    request.AddObject(tagToUpdate);
//                    request.Method = Method.PUT;
//                });
//        }




//        public List<AsanaNavigation> GetAsanaNav()
//        {
//            var workspaces = this.GetWorkspaces();
//            var results =
//                workspaces.Data.Select(
//                    p => new AsanaNavigation { Name = p.name, Id = p.id, Projects = this.GetProjects(p.id, null).Data }).ToList();

//            return results;
//        }

//        public AsanaResponse<List<AsanaProject>> GetProjects(long workspaceId, List<string> optFields)
//        {
//            var url = GetWorkspaceProjectsUrl(workspaceId, optFields);

//            return UseRequest<List<AsanaProject>>(url, request =>
//                {

//                });
//        }

//        public AsanaResponse<List<AsanaTag>> GetTags(long workspaceId, List<string> optFields = null)
//        {
//            var url = GetWorkspaceTagsUrl(workspaceId, optFields);

//            return UseRequest<List<AsanaTag>>(url, request =>
//            {

//            });
//        }

		private static string GetProjectStoriesUrl(long projectId, List<string> optFields = null)
		{
			var url = GetUrl(string.Format("projects/{0}/stories", projectId.ToString()), optFields);

			return url;
		}

		private static string GetTaskStoriesUrl(long taskId, List<string> optFields = null)
		{
			var url = GetUrl(string.Format("tasks/{0}/stories", taskId.ToString()), optFields);

			return url;
		}


		private static string GetWorkspaceProjectsUrl(long workspaceId, List<string> optFields = null)
		{
			var url = GetUrl(string.Format("workspaces/{0}/projects", workspaceId.ToString()), optFields);

			return url;
		}


		private static string GetWorkspaceTasksUrl(long workspaceId, List<string> optFields = null)
		{
			var url = GetUrl(string.Format("workspaces/{0}/tasks", workspaceId.ToString()), optFields);

			return url;
		}

		private static string GetSubTasksUrl(long parentId, List<string> optFields = null)
		{
			var url = GetUrl(string.Format("tasks/{0}/subtasks", parentId.ToString()), optFields);

			return url;
		}

		private static string GetWorkspaceTagsUrl(long workspaceId, List<string> optFields = null)
		{
			var url = GetUrl(string.Format("workspaces/{0}/tags", workspaceId.ToString()), optFields);

			return url;
		}

		private static string GetUrl(string url, List<string> optFields, bool addEnd = true)
		{
			if (optFields != null)
			{
				if (addEnd)
				{
					url += "?";
				}
				else
				{
					url += "&";
				}
				url += "opt_fields=" + string.Join(",", optFields);
			}

			return url;
		}



        //public AsanaResponse<List<AsanaTask>> GetTasksByTag(long tagId, List<string> optFields)
        //{
        //    return GetData<List<AsanaTask>>(this.GetTagTasksUrl(tagId, optFields));
        //}


        //public AsanaResponse<List<AsanaTask>> GetTasks(long projectId, List<string> optFields)
        //{
        //    return GetData<List<AsanaTask>>(this.GetProjectTasksUrl(projectId, optFields));
        //}


        //public AsanaResponse<List<AsanaTask>> GetSubtasks(long parentId, List<string> optFields)
        //{
        //    return GetData<List<AsanaTask>>(GetUrl(string.Format("/tasks/{0}/subtasks", parentId.ToString()), optFields));
        //}

        //public AsanaResponse<List<AsanaTask>> GetUserTasks(long userId, long workspaceId, List<string> optFields)
        //{
        //    return GetData<List<AsanaTask>>(GetUrl(string.Format("/tasks?assignee={0}&workspace={1}", userId, workspaceId), optFields, false));
        //}


        //public AsanaResponse<List<AsanaTask>> GetMyTasks(long workspaceId, List<string> optFields)
        //{
        //    return GetData<List<AsanaTask>>(GetUrl("/tasks?assignee=me&workspace=" + workspaceId, optFields, false));
        //}



		private string GetProjectTasksUrl(long id)
		{
			return this.GetProjectTasksUrl(id, null);
		}


		private string GetTagTasksUrl(long tagId, List<string> optFields)
		{
			return GetUrl(string.Format("/tags/{0}/tasks", tagId.ToString()), optFields);

		}

		private string GetProjectTasksUrl(long projectId, List<string> optFields)
		{
			return GetUrl(string.Format("/projects/{0}/tasks", projectId.ToString()), optFields);

		}

        //public AsanaResponse<AsanaUser> GetUser(long userId, List<string> optFields = null)
        //{
        //    return GetUser(userId.ToString(), optFields);
        //}

        //public AsanaResponse<AsanaUser> GetUser(string userId, List<string> optFields = null)
        //{
        //    if (optFields == null)
        //    {
        //        optFields = new List<string>();
        //    }
        //    optFields.Add("name");

        //    return GetData<AsanaUser>(GetUrl("users/" + userId, optFields));
        //}

        //public AsanaResponse<List<AsanaUser>> GetUsers(List<string> optFields = null)
        //{
        //    if (optFields == null)
        //    {
        //        optFields = new List<string>();
        //    }
        //    optFields.Add("name");

        //    return GetData<List<AsanaUser>>(GetUrl("users", optFields));
        //}

        //public WorkspaceStats GetWorkspaceStats(long id)
        //{
        //    var projects = this.GetWorkspaceProjectsAndTasks(id);

        //    var results = new WorkspaceStats();

        //    var stats =
        //        projects.Where(f => !f.archived).Select(
        //            project =>
        //            {
        //                var workplaceStat = new WorkplaceStat();
        //                workplaceStat.archived = project.archived;
        //                workplaceStat.created_at = project.created_at;
        //                workplaceStat.id = project.id;
        //                workplaceStat.modified_at = project.modified_at;
        //                workplaceStat.name = project.name;
        //                workplaceStat.notes = project.notes;
        //                workplaceStat.workspaceid = project.workspaceid;
        //                workplaceStat.TotalTasks = project.Tasks.Count;
        //                workplaceStat.TasksCompleted = project.Tasks.Count(p => p.completed);
        //                workplaceStat.OldestTask = project.Tasks.OrderByDescending(p => p.created_at).Select(p => p.created_at).FirstOrDefault();
        //                workplaceStat.NewestTask = project.Tasks.OrderBy(p => p.created_at).Select(p => p.created_at).FirstOrDefault();
        //                workplaceStat.PastDueTasks = project.Tasks.Count(p => p.due_on.HasValue && p.due_on.Value <= DateTime.Now);
        //                workplaceStat.TaskFollowers = project.Tasks.SelectMany(p => p.followers).Select(f => f.id).Distinct().Count();
        //                workplaceStat.ProjectFollowers = project.followers.Count();
        //                workplaceStat.DueToday = project.Tasks.Count(p => p.due_on.HasValue && p.due_on.Value.Date == DateTime.Now.Date);
        //                workplaceStat.NotDueYet = project.Tasks.Count(p => p.due_on.HasValue && p.due_on.Value >= DateTime.Now);
        //                workplaceStat.NeverDue = project.Tasks.Count(p => !p.due_on.HasValue);
        //                workplaceStat.LastModified = project.Tasks.OrderByDescending(p => p.modified_at).Select(p => p.modified_at).FirstOrDefault();
        //                workplaceStat.PastDueDays = project.Tasks.Where(p => p.due_on.HasValue && p.due_on.Value <= DateTime.Now).Sum(p => (DateTime.Now - p.due_on).Value.TotalDays);
        //                return workplaceStat;
        //            });

        //    results.Stats = stats.ToList();
        //    return results;
        //}

        //public List<AsanaProject> GetWorkspaceProjectsAndTasks(long id)
        //{
        //    var optFields = new List<string>() { "created_at", "name", "archived", "modified_at", "notes", "workspaceid", "archived", "followers" };
        //    var projects = this.GetProjects(id, optFields).Data;

        //    projects.ForEach(p => p.Tasks = this.GetTasks(p.id, new List<string> { "created_at", "modified_at", "due_on", "completed", "followers", "name" }).Data);
        //    return projects;
        //}


		public async Task<AWorkspacesResponse> GetWorkspaces()
		{
			return await SendRequest<AWorkspacesResponse>("workspaces");
		}

		public static string GetDeviceModel()
		{
            //TODO: fix
		    return "test";
            //string model = null;
            //object theModel;

            //if (DeviceExtendedProperties.TryGetValue("DeviceName", out theModel))
            //{
            //    model = theModel as string;
            //}
            //return model;
		}



		private static async Task<T> SendRequest<T>(string resource, HttpMethod method = null, RequestParametersContainer parameters = null) where T : AResponse,new()
		{

            var client = new HttpClient(new HttpClientHandler(){AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip});
            client.Timeout = TimeSpan.FromSeconds(20);
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            //TODO:user agent
            //client.UserAgent = string.Format("Mozilla/5.0 (WindowsPhone {0}; {1}) Gecko/20100101", Environment.OSVersion.Version, GetDeviceModel());

		    var request = new HttpRequestMessage();

		    if (method == null)
		    {
		        method = HttpMethod.Get;
		    }

		    request.Method = method;


		    if (parameters != null)
		    {
		        if (method == HttpMethod.Post)
		        {
		            request.Content = new FormUrlEncodedContent(parameters);
		        }
		        if (method == HttpMethod.Get)
		        {
		            resource += "?" + parameters.ToStringData();
		        }
		    }
		    request.RequestUri = new Uri(_asanaBaseUrl + resource);
		    

		    //TODO: key
            request.Headers.Authorization = new AuthenticationHeaderValue(AsanaConstants.ApiKeys.TestAccount1);

		    try
		    {
                var response = await client.SendAsync(request);

		        var content = await response.Content.ReadAsStringAsync();

		        var data = JsonConvert.DeserializeObject<T>(content);

                //TODO: process errors

		        return data;
		    }
		    catch (Exception)
		    {
                //TODO process
		    }

		    return null;


		    //var request = new RestRequest(resource);
		    //if(AsanaStateService.IsSetApiKey)
		    //{
		    //    client.Authenticator = new HttpBasicAuthenticator(AsanaStateService.ApiKey, string.Empty);
		    //}
		    //else if (AsanaStateService.IsSetAuthToken)
		    //{
		    //    request.AddHeader(System.Net.HttpRequestHeader.Authorization.ToString(),
		    //                      string.Format("Bearer {0}", AsanaStateService.AuthToken));
		    //}



		}




        //private static AsanaResponse<T> UseRequest<T>(string resource, Action<RestRequest> action) where T : class,new()
        //{
        //    AsanaResponse<T> response = null;
        //    try
        //    {
        //        response = GetResponse<T>(resource, action);

        //        if (response == null)
        //        {
        //            response = new AsanaResponse<T>();
        //            response.StatusCode = HttpStatusCode.RequestTimeout;
        //            response.StatusDescription = "Request to Asana service failed due to timeout. Try again";
        //            return response;
        //        }

				
        //        switch (response.StatusCode)
        //        {
        //            case HttpStatusCode.ServiceUnavailable:
        //                response.StatusDescription = "Asana service is not available. Try later please";
        //                break;
        //            case HttpStatusCode.BadGateway:
        //                response.StatusDescription = "Asana service is not available. Try later please";
        //                break;
        //            case HttpStatusCode.BadRequest:
        //                response.Errors = JsonConvert.DeserializeObject<AErrors>(response.Content);
        //                break;
        //            case HttpStatusCode.OK:
        //            case HttpStatusCode.Created:
        //                response.Data = JsonConvert.DeserializeObject<DataClass<T>>(response.Content).data;
        //                break;

        //            case HttpStatusCode.Unauthorized:
        //                break;

        //            case HttpStatusCode.Forbidden:
        //                response.StatusDescription = "Forbidden request to Asana service";
        //                break;
        //            case (HttpStatusCode)429:
        //                response.StatusDescription = "Asana service is not available. Try later please";
        //                break;
        //            case 0:
        //            case HttpStatusCode.NotFound:
        //                response.StatusDescription = "Asana service is not available. Try later please";

        //                if (!string.IsNullOrEmpty(response.Content))
        //                {
        //                    ActionHelper.SafeExecute(() =>
        //                                                 {
        //                                                     response.Errors =
        //                                                         JsonConvert.DeserializeObject<AErrors>(
        //                                                             response.Content);
        //                                                 });
        //                }

        //                break;
        //            default:
        //                throw new Exception(string.Format("Unhandled status code {0}", response.StatusCode));
        //        }


        //        return response;
        //    }
        //    catch (Exception e)
        //    {
        //        throw;
        //        //response = new AsanaResponse<T>();
        //        //response.StatusCode = HttpStatusCode.ServiceUnavailable;
        //        //response.StatusDescription = "Asana service is not available";
        //        //return response;
        //    }



        //}

	}

    public class RequestParametersContainer : List<KeyValuePair<string, string>>
    {



        private Dictionary<string, KeyValuePair<string, byte[]>> _binaryData =
            new Dictionary<string, KeyValuePair<string, byte[]>>();

        public Dictionary<string, KeyValuePair<string, byte[]>> BinaryData
        {
            get { return _binaryData; }
        }

        public string PropertyName { get; set; }

        public JObject JsonData { get; set; }

        public JsonSerializerSettings  JsonSerializerSettings { get; set; }

        public void Add(object key, IEnumerable<string> value)
        {
            foreach (var v in value)
            {
                Add(key, v);
            }
        }

        public void Add(object key, IEnumerable<int> value)
        {
            Add(key, value.Select(x => x.ToString()));
        }

        public void Add<T>(object key, Nullable<T> value) where T : struct
        {
            if (value.HasValue)
            {
                Add(key, value.Value);
            }
        }

        public void Add(object key, object value, bool addAnyWay = false)
        {
            string v = "";
            if (value != null || addAnyWay)
            {
                if (value is bool)
                {
                    v = ((bool)value) ? "true" : "false";
                }
                else
                {
                    v = value.ToString();
                }
                this.Add(new KeyValuePair<string, string>(key.ToString(), v));
            }
        }

        public void Add(object key, string fileName, byte[] data)
        {
            _binaryData.Add(key.ToString(), new KeyValuePair<string, byte[]>(fileName, data));
        }


        public bool HasBinaryContents()
        {
            return _binaryData.Any();
        }

        public void Merge(RequestParametersContainer pairs)
        {
            foreach (KeyValuePair<string, string> keyValuePair in pairs)
            {
                Add(keyValuePair);
            }
        }

        public string ToStringData()
        {

            return string.Join("&", this.Select(x => string.Format("{0}={1}",x.Key, x.Value)));
        }
    }

}