using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.ExchangeActiveSyncProvisioning;
using DevRain.Asana.API.Data.Models;
using DevRain.Asana.API.Data.Models.DTO;
using DevRain.Asana.API.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using DevRain.Windows.WinRT.Common.Helpers;
using DevRain.Windows.WinRT.Common.Models.API;
using Newtonsoft.Json;


namespace DevRain.Asana.API.Data
{
	public class AsanaApiRepository
	{
		#region Constants

		#endregion

		#region Static Fields

		private static readonly string _asanaBaseUrl;

		#endregion

		#region Constructors and Destructors

		static AsanaApiRepository()
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
			return await UseRequest<List<AsanaStory>>(GetProjectStoriesUrl(id));

		}


		public  async Task<AsanaResponse<List<AsanaStory>>> GetStoriesByTask(long id)
		{
			return await UseRequest<List<AsanaStory>>(GetTaskStoriesUrl(id));

		}

		public  async Task<AsanaResponse<AsanaProject>> CreateProject(long workspaceId, AsanaProject newProject)
		{
			return await UseRequest<AsanaProject>(
				GetWorkspaceProjectsUrl(workspaceId),
				(request,container) =>
				{
					request.Method = HttpMethod.Post;
                    
                    container.Add("name",newProject.name);
                    container.Add("notes", newProject.notes);
                    container.Add("archived", newProject.archived);
				});
		}

		public  async Task<AsanaResponse<AsanaTask>> GetTask(long id)
		{
			return await UseRequest<AsanaTask>(
				string.Format("tasks/{0}", id));
		}

		public  async Task<AsanaResponse<AsanaProject>> GetProject(long id)
		{
			return await  UseRequest<AsanaProject>(
				string.Format("projects/{0}", id));
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
            (request,container )=>
            {
                request.Method = HttpMethod.Post;

	
                container.AddObject(newTask, fields.ToArray());

                if (newTask.followers != null)
                {
                    foreach (var asanaFollower in newTask.followers)
                    {
                        container.JsonData.Add(string.Format("followers[{0}]", newTask.followers.IndexOf(asanaFollower)), asanaFollower.id);
                    }
                }
            });


		}


		public  async Task<AsanaResponse<AsanaStory>> CreateTaskStory(long taskId, AsanaStory story)
		{
			return await UseRequest<AsanaStory>(
				GetTaskStoriesUrl(taskId),
				(request,container) =>
				{
                    var fields = new List<string> { "text" };

				    request.Method = HttpMethod.Post;
                    container.AddObject(story, fields.ToArray());
				});
		}




		public  async Task<AsanaResponse<AsanaTask>> UpdateTask(AsanaTaskDTO newTask)
		{
			return await UseRequest<AsanaTask>(
				string.Format("tasks/{0}", newTask.id),
				(request,container) =>
				{

					var fields = new List<string> { "name", "notes", "completed", "assignee_status" };

					fields.Add("due_on");

					fields.Add("assignee");


				    request.Method = HttpMethod.Put;
                    container.AddObject(newTask, fields.ToArray());
				});
		}

		public  async Task<AsanaResponse<AsanaTask>> DeleteTask(long id)
		{
			return await UseRequest<AsanaTask>(
				string.Format("tasks/{0}", id),
				(request,container) =>
				{

				    request.Method = HttpMethod.Delete;

				});
		}




		public  async Task<AsanaResponse<AsanaTag>> CreateTag(long workspaceId, AsanaTag newTag)
		{
			return await UseRequest<AsanaTag>(
				GetWorkspaceTagsUrl(workspaceId),
                    (request, container) =>
                    {

                        request.Method = HttpMethod.Post;

					var fields = new List<string>();
					fields.Add("name");

                    container.AddObject(newTag, fields.ToArray());
				});
		}

		public  async Task<AsanaResponse<BaseEntity>> AddTag(long taskId, long tagId)
		{
            return await UseRequest<BaseEntity>(string.Format("tasks/{0}/addTag", taskId), (request, container) =>
            {
                request.Method = HttpMethod.Post;

                container.Add("tag", tagId);


			});

		}

		public  async Task<AsanaResponse<BaseEntity>> RemoveTag(long taskId, long tagId)
		{
            return await UseRequest<BaseEntity>(string.Format("tasks/{0}/removeTag", taskId), (request, container) =>
			{
                request.Method = HttpMethod.Post;

                container.Add("tag", tagId);


			});

		}

		public  async Task<AsanaResponse<BaseEntity>> AddFollowers(long taskId, List<long> followers)
		{
            return await UseRequest<BaseEntity>(string.Format("tasks/{0}/addFollowers", taskId), (request, container) =>
			{
				request.Method =HttpMethod.Post;

				for (int i = 0; i < followers.Count; i++)
				{
                    container.Add(string.Format("followers[{0}]", i), followers[i]);
				}


			});

		}

		public  async Task<AsanaResponse<BaseEntity>> RemoveFollowers(long taskId, List<long> followers)
		{
            return await UseRequest<BaseEntity>(string.Format("tasks/{0}/removeFollowers", taskId), (request, container) =>
			{
                request.Method = HttpMethod.Post;

				for (int i = 0; i < followers.Count; i++)
				{
                    container.Add(
			string.Format("followers[{0}]", i), followers[i]);
				}


			});

		}



		public  async Task<AsanaResponse<BaseEntity>> ChangeTaskProject(long taskId, long oldProjectId, long newProjectId)
		{
			AsanaResponse<BaseEntity> result = null;

			if (oldProjectId > 0)
			{
                result = await UseRequest<BaseEntity>(string.Format("tasks/{0}/removeProject", taskId), (request, container) =>
																									  {
                                                                                                          request.Method = HttpMethod.Post;
                                                                                                          container.Add(
																											  "project", oldProjectId);
																									  });
			}


			if (newProjectId > 0 && (result == null || result.Errors == null))
			{
                return await UseRequest<BaseEntity>(string.Format("tasks/{0}/addProject", taskId), (request, container) =>
																								 {
                                                                                                     request.Method = HttpMethod.Post;
                                                                                                     container.Add(
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
                  (request, container) =>
				{
					var fields = new List<string> { "name", "notes", "archived" };

					container.AddObject(projectToUpdate, fields.ToArray());
				    request.Method = HttpMethod.Put;
				});
		}




		public  async Task<AsanaResponse<AsanaTag>> UpdateTag(AsanaTag tagToUpdate)
		{
			return await UseRequest<AsanaTag>(
				string.Format("tags/{0}", tagToUpdate.id),
				(request,container) =>
				{
					container.AddObject(tagToUpdate);
				    request.Method = HttpMethod.Put;
				});
		}




		public  async Task<AsanaResponse<List<AsanaProject>>> GetProjects(long workspaceId, List<string> optFields)
		{
			var url = GetWorkspaceProjectsUrl(workspaceId, optFields);

            return await UseRequest<List<AsanaProject>>(url, (request, container) =>
				{

				});
		}

		public  async Task<AsanaResponse<List<AsanaTag>>> GetTags(long workspaceId, List<string> optFields = null)
		{
			var url = GetWorkspaceTagsUrl(workspaceId, optFields);

			return await UseRequest<List<AsanaTag>>(url);
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

 
		public async Task<AsanaResponse<List<AsanaWorkspace>>> GetWorkspaces()
		{
			return await GetData<List<AsanaWorkspace>>("workspaces");
		}



		#endregion

		#region Methods

		private static async Task<AsanaResponse<T>> GetData<T>(string resource) where T : class,new()
		{
			return await UseRequest<T>(resource);

		}

		private static async Task<AsanaResponse<T>> GetResponse<T>(string resource, Action<HttpRequestMessage,RequestParametersContainer> action = null) where T : class,new()
		{
            var handler = new HttpClientHandler()
		    {
		        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
		    };

			var client = new HttpClient(handler);
            client.BaseAddress = new Uri(_asanaBaseUrl);

            var userAgent = string.Format("Mozilla/5.0 (WindowsPhone {0}; {1} {2}) Gecko/20100101 CCW/1.0",
            DeviceHelper.GetOSVersion(),
            DeviceHelper.GetSystemManufacturer(),
            DeviceHelper.GetSystemProductName());

          //  client.DefaultRequestHeaders.UserAgent.ParseAdd(string.Format("Mozilla/5.0 (WindowsPhone {0}; {1}) Gecko/20100101", DeviceHelper.GetOSVersion(), DeviceHelper.GetModelName()));

            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

		    var request = new HttpRequestMessage(HttpMethod.Get, resource);

            
			if (AsanaStateService.IsSetAuthToken)
			{
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AsanaStateService.AuthToken);

			}

		    var container = new RequestParametersContainer();


		    if (action != null)
		    {
		        action(request, container);
		    }

		    HttpResponseMessage response = null;

		    try
		    {
		        response = await client.SendAsync(request);
                
                    return new AsanaResponse<T>
                    {
                        Content = await response.Content.ReadAsStringAsync(),
                        StatusCode = response.StatusCode,
                        RequestUri = resource,
                    };


              
		    }
		    catch (Exception e)
		    {
		        return new AsanaResponse<T>(){ErrorException =e,ErrorMessage = e.Message,StatusCode = HttpStatusCode.NotFound};
		    }

            
			


		}




        private static async Task<AsanaResponse<T>> UseRequest<T>(string resource, Action<HttpRequestMessage,RequestParametersContainer> action = null) where T : class,new()
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
						ActionHelper.SafeExecute(() =>
                        {
                            response.Errors =
                                JsonConvert.DeserializeObject<AsanaErrors>(
                                    response.Content);
                        });

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

 
}