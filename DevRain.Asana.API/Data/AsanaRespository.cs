using System.Threading;
using System.Threading.Tasks;
using DevRain.Asana.API.Data.Models;
using DevRain.Asana.API.Data.Models.DTO;
using DevRain.Asana.API.Services;
using DevRain.WP.Common.Helpers;
using Microsoft.Phone.Info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using RestSharp;

namespace DevRain.Asana.API.Data
{
	public class AsanaRespository
	{
		#region Constants

		#endregion

		#region Static Fields

		private static readonly string _asanaBaseUrl;

		#endregion

		#region Constructors and Destructors

		static AsanaRespository()
		{
			_asanaBaseUrl = "https://app.asana.com/api/1.0/";
		}

		#endregion

		#region Public Methods and Operators

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

		public async Task<AsanaResponse<List<AsanaStory>>> GetStoriesByProject(long id)
		{
			return await UseRequest<List<AsanaStory>>(GetProjectStoriesUrl(id), request =>
				{

				});

		}


		public  async Task<AsanaResponse<List<AsanaStory>>> GetStoriesByTask(long id)
		{
			return await UseRequest<List<AsanaStory>>(GetTaskStoriesUrl(id), request =>
			{

			});

		}

		public  async Task<AsanaResponse<AsanaProject>> CreateProject(long workspaceId, AsanaProject newProject)
		{
			return await UseRequest<AsanaProject>(
				GetWorkspaceProjectsUrl(workspaceId),
				request =>
				{
					newProject.workspaceid = workspaceId;
					request.Method = Method.POST;

					var fields = new List<string> { "name", "notes", "archived" };

					request.AddObject(newProject, fields.ToArray());
				});
		}

		public  async Task<AsanaResponse<AsanaTask>> GetTask(long id)
		{
			return await UseRequest<AsanaTask>(
				string.Format("tasks/{0}", id),
				request =>
				{
					request.Method = Method.GET;

				});
		}

		public  async Task<AsanaResponse<AsanaProject>> GetProject(long id)
		{
			return await  UseRequest<AsanaProject>(
				string.Format("projects/{0}", id),
				request =>
				{
					request.Method = Method.GET;
				});
		}

		public  async Task<AsanaResponse<AsanaTask>> CreateTask(long workspaceId, AsanaTaskDTO newTask, long? parentId)
		{
			var fields = new List<string> { "name", "notes", "completed", "due_on", "assignee_status" };

			if (newTask.projects > 0 && parentId == null)
			{
				fields.Add("projects");
			}

			if (newTask.due_on != null)
			{
				fields.Add("due_on");
			}



			fields.Add("assignee");


			var url = parentId.HasValue ? GetSubTasksUrl(parentId.Value) : GetWorkspaceTasksUrl(workspaceId);


			return await UseRequest<AsanaTask>(url,
request =>
{
	request.Method = Method.POST;

	if (newTask.followers != null)
	{
		foreach (var asanaFollower in newTask.followers)
		{
			request.AddParameter(string.Format("followers[{0}]", newTask.followers.IndexOf(asanaFollower)), asanaFollower.id);
		}
	}
	request.AddObject(newTask, fields.ToArray());
});


		}


		public  async Task<AsanaResponse<AsanaStory>> CreateTaskStory(long taskId, AsanaStory story)
		{
			return await UseRequest<AsanaStory>(
				GetTaskStoriesUrl(taskId),
				request =>
				{

					var fields = new List<string> { "text" };

					request.Method = Method.POST;
					request.AddObject(story, fields.ToArray());
				});
		}




		public  async Task<AsanaResponse<AsanaTask>> UpdateTask(AsanaTaskDTO newTask)
		{
			return await UseRequest<AsanaTask>(
				string.Format("tasks/{0}", newTask.id),
				request =>
				{

					var fields = new List<string> { "name", "notes", "completed", "assignee_status" };

					fields.Add("due_on");

					fields.Add("assignee");


					//if(newTask.followers.Any())
					//{
					//    fields.Add("followers");  
					//}


					request.Method = Method.PUT;
					request.AddObject(newTask, fields.ToArray());
				});
		}

		public  async Task<AsanaResponse<AsanaTask>> DeleteTask(long id)
		{
			return await UseRequest<AsanaTask>(
				string.Format("tasks/{0}", id),
				request =>
				{

					request.Method = Method.DELETE;

				});
		}




		public  async Task<AsanaResponse<AsanaTag>> CreateTag(long workspaceId, AsanaTag newTag)
		{
			return await UseRequest<AsanaTag>(
				GetWorkspaceTagsUrl(workspaceId),
				request =>
				{

					request.Method = Method.POST;

					var fields = new List<string>();
					fields.Add("name");

					request.AddObject(newTag, fields.ToArray());
				});
		}

		public  async Task<AsanaResponse<BaseEntity>> AddTag(long taskId, long tagId)
		{
			return await UseRequest<BaseEntity>(string.Format("tasks/{0}/addTag", taskId), request =>
			{
				request.Method = Method.POST;

				request.AddParameter("tag", tagId);


			});

		}

		public  async Task<AsanaResponse<BaseEntity>> RemoveTag(long taskId, long tagId)
		{
			return await UseRequest<BaseEntity>(string.Format("tasks/{0}/removeTag", taskId), request =>
			{
				request.Method = Method.POST;

				request.AddParameter("tag", tagId);


			});

		}

		public  async Task<AsanaResponse<BaseEntity>> AddFollowers(long taskId, List<long> followers)
		{
			return await UseRequest<BaseEntity>(string.Format("tasks/{0}/addFollowers", taskId), request =>
			{
				request.Method = Method.POST;

				for (int i = 0; i < followers.Count; i++)
				{
					request.AddParameter(
			        string.Format("followers[{0}]", i), followers[i]);
				}


			});

		}

		public  async Task<AsanaResponse<BaseEntity>> RemoveFollowers(long taskId, List<long> followers)
		{
			return await UseRequest<BaseEntity>(string.Format("tasks/{0}/removeFollowers", taskId), request =>
			{
				request.Method = Method.POST;

				for (int i = 0; i < followers.Count; i++)
				{
					request.AddParameter(
			string.Format("followers[{0}]", i), followers[i]);
				}


			});

		}



		public  async Task<AsanaResponse<BaseEntity>> ChangeTaskProject(long taskId, long oldProjectId, long newProjectId)
		{
			AsanaResponse<BaseEntity> result = null;

			if (oldProjectId > 0)
			{
				result = await UseRequest<BaseEntity>(string.Format("tasks/{0}/removeProject", taskId), request =>
																									  {
																										  request.Method = Method.POST;
																										  request.AddParameter(
																											  "project", oldProjectId);
																									  });
			}


			if (newProjectId > 0 && (result == null || result.Errors == null))
			{
				return await UseRequest<BaseEntity>(string.Format("tasks/{0}/addProject", taskId), request =>
																								 {
																									 request.Method = Method.POST;
																									 request.AddParameter(
																										 "project", newProjectId);
																								 });

			}
			else
			{
				return result;
			}
		}


		public  async Task<AsanaResponse<AsanaProject>> UpdateProject(AsanaProject projectToUpdate)
		{
			return await UseRequest<AsanaProject>(
				string.Format("projects/{0}", projectToUpdate.id),
				request =>
				{
					var fields = new List<string> { "name", "notes", "archived" };

					request.AddObject(projectToUpdate, fields.ToArray());
					request.Method = Method.PUT;
				});
		}




		public  async Task<AsanaResponse<AsanaTag>> UpdateTag(AsanaTag tagToUpdate)
		{
			return await UseRequest<AsanaTag>(
				string.Format("tags/{0}", tagToUpdate.id),
				request =>
				{
					request.AddObject(tagToUpdate);
					request.Method = Method.PUT;
				});
		}




        //public List<AsanaNavigation> GetAsanaNav()
        //{
        //    var workspaces = this.GetWorkspaces();
        //    var results =
        //        workspaces.Data.Select(
        //            p => new AsanaNavigation { Name = p.name, Id = p.id, Projects = this.GetProjects(p.id, null).Data }).ToList();

        //    return results;
        //}

		public  async Task<AsanaResponse<List<AsanaProject>>> GetProjects(long workspaceId, List<string> optFields)
		{
			var url = GetWorkspaceProjectsUrl(workspaceId, optFields);

			return await UseRequest<List<AsanaProject>>(url, request =>
				{

				});
		}

		public  async Task<AsanaResponse<List<AsanaTag>>> GetTags(long workspaceId, List<string> optFields = null)
		{
			var url = GetWorkspaceTagsUrl(workspaceId, optFields);

			return await UseRequest<List<AsanaTag>>(url, request =>
			{

			});
		}

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



		public  async Task<AsanaResponse<List<AsanaTask>>> GetTasksByTag(long tagId, List<string> optFields)
		{
			return await GetData<List<AsanaTask>>(this.GetTagTasksUrl(tagId, optFields));
		}


		public  async Task<AsanaResponse<List<AsanaTask>>> GetTasks(long projectId, List<string> optFields)
		{
			return await GetData<List<AsanaTask>>(this.GetProjectTasksUrl(projectId, optFields));
		}


		public  async Task<AsanaResponse<List<AsanaTask>>> GetSubtasks(long parentId, List<string> optFields)
		{
			return await GetData<List<AsanaTask>>(GetUrl(string.Format("/tasks/{0}/subtasks", parentId.ToString()), optFields));
		}

		public  async Task<AsanaResponse<List<AsanaTask>>> GetUserTasks(long userId, long workspaceId, List<string> optFields)
		{
			return await GetData<List<AsanaTask>>(GetUrl(string.Format("/tasks?assignee={0}&workspace={1}", userId, workspaceId), optFields, false));
		}


		public  async Task<AsanaResponse<List<AsanaTask>>> GetMyTasks(long workspaceId, List<string> optFields)
		{
			return await GetData<List<AsanaTask>>(GetUrl("/tasks?assignee=me&workspace=" + workspaceId, optFields, false));
		}



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

		public  async Task<AsanaResponse<AsanaUser>> GetUser(long userId, List<string> optFields = null)
		{
			return await GetUser(userId.ToString(), optFields);
		}

		public  async Task<AsanaResponse<AsanaUser>> GetUser(string userId, List<string> optFields = null)
		{
			if (optFields == null)
			{
				optFields = new List<string>();
			}
			optFields.Add("name");

			return await GetData<AsanaUser>(GetUrl("users/" + userId, optFields));
		}

		public  async Task<AsanaResponse<List<AsanaUser>>> GetUsers(List<string> optFields = null)
		{
			if (optFields == null)
			{
				optFields = new List<string>();
			}
			optFields.Add("name");

			return await GetData<List<AsanaUser>>(GetUrl("users", optFields));
		}

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

        //public  async Task<List<AsanaProject>> GetWorkspaceProjectsAndTasks(long id)
        //{
        //    var optFields = new List<string>() { "created_at", "name", "archived", "modified_at", "notes", "workspaceid", "archived", "followers" };
        //    var projects = (await this.GetProjects(id, optFields)).Data;

        //    projects.ForEach(p => p.Tasks = this.GetTasks(p.id, new List<string> { "created_at", "modified_at", "due_on", "completed", "followers", "name" }).Data);
        //    return projects;
        //}


		public async Task<AsanaResponse<List<AsanaWorkspace>>> GetWorkspaces()
		{
			return await GetData<List<AsanaWorkspace>>("workspaces");
		}

		public static string GetDeviceModel()
		{
			string model = null;
			object theModel;

			if (DeviceExtendedProperties.TryGetValue("DeviceName", out theModel))
			{
				model = theModel as string;
			}
			return model;
		}

		#endregion

		#region Methods

		private static async Task<AsanaResponse<T>> GetData<T>(string resource) where T : class,new()
		{
			return await UseRequest<T>(resource, request => { });

		}

		private static async Task<AsanaResponse<T>> GetResponse<T>(string resource, Action<RestRequest> action) where T : class,new()
		{
			var client = new RestClient(_asanaBaseUrl);
			client.Timeout = 19000;
			client.UserAgent = string.Format("Mozilla/5.0 (WindowsPhone {0}; {1}) Gecko/20100101", Environment.OSVersion.Version, GetDeviceModel());
			var request = new RestRequest(resource);
			if(AsanaStateService.IsSetApiKey)
			{
				client.Authenticator = new HttpBasicAuthenticator(AsanaStateService.ApiKey, string.Empty);
			}
			else if (AsanaStateService.IsSetAuthToken)
			{
				request.AddHeader(System.Net.HttpRequestHeader.Authorization.ToString(),
				                  string.Format("Bearer {0}", AsanaStateService.AuthToken));
			}
		
			request.AddHeader("Accept-Encoding", "gzip");
			action(request);

		    AsanaResponse<T> response = null;

		    try
		    {
                response = await client.MyExecuteAsync(request, (content) =>
                {
                    return new AsanaResponse<T>
                    {
                        Content = content.Content,
                        ContentEncoding = content.ContentEncoding,
                        ContentLength = content.ContentLength,
                        ContentType = content.ContentType,
                        ErrorException = content.ErrorException,
                        ErrorMessage = content.ErrorMessage,
                        RawBytes = content.RawBytes,
                        Request = content.Request,
                        ResponseStatus = content.ResponseStatus,
                        ResponseUri = content.ResponseUri,
                        Server = content.Server,
                        StatusCode = content.StatusCode,
                        StatusDescription = content.StatusDescription
                    };


                });
		    }
		    catch (Exception e)
		    {
		        response = new AsanaResponse<T>(){ErrorException =e,ErrorMessage = e.Message,StatusCode = HttpStatusCode.NotFound};
		    }

            
			return response;


		}




		private static async Task<AsanaResponse<T>> UseRequest<T>(string resource, Action<RestRequest> action) where T : class,new()
		{
			AsanaResponse<T> response = null;
			try
			{
				response = await GetResponse<T>(resource, action);
			
				switch (response.StatusCode)
				{
					case HttpStatusCode.ServiceUnavailable:
						response.StatusDescription = "Asana service is not available. Try later please";
						break;
                    case HttpStatusCode.BadGateway:
                        response.StatusDescription = "Asana service is not available. Try later please";
                        break;
					case HttpStatusCode.BadRequest:
						response.Errors = JsonConvert.DeserializeObject<AsanaErrors>(response.Content);
						break;
					case HttpStatusCode.OK:
					case HttpStatusCode.Created:
						response.Data = JsonConvert.DeserializeObject<DataClass<T>>(response.Content).data;
						break;

					case HttpStatusCode.Unauthorized:
						break;

					case HttpStatusCode.Forbidden:
						response.StatusDescription = "Forbidden request to Asana service";
						break;
					case (HttpStatusCode)429:
						response.StatusDescription = "Asana service is not available. Try later please";
						break;
					case 0:
					case HttpStatusCode.NotFound:
						response.StatusDescription = "Asana service is not available. Try later please";

						if (!string.IsNullOrEmpty(response.Content))
						{
							ActionHelper.SafeExecute(() =>
														 {
															 response.Errors =
																 JsonConvert.DeserializeObject<AsanaErrors>(
																	 response.Content);
														 });
						}

						break;
					default:
						throw new Exception(string.Format("Unhandled status code {0}", response.StatusCode));
				}


				return response;
			}
			catch (Exception e)
			{
				throw;
			}



		}

		#endregion
	}

    public static class RestClientExtensions
    {
        public static Task<T> MyExecuteAsync<T>(this RestClient client, IRestRequest request, Func<IRestResponse, T> selector)
        {
            var tcs = new TaskCompletionSource<T>();
            var loginResponse = client.ExecuteAsync(request, r =>
            {
                if (r.ErrorException == null)
                {
                    tcs.SetResult(selector(r));
                }
                else
                {
                    tcs.SetException(r.ErrorException);
                }
            });
            return tcs.Task;
        }


    }
}