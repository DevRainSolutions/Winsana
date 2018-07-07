using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevRain.Asana.API.Data;
using DevRain.Asana.API.Data.Models;
using DevRain.Asana.API.Data.Models.DTO;
using DevRain.Asana.API.Services.Db;

using DevRain.WP.Common.Helpers;


namespace DevRain.Asana.API.Services
{
	public class MapperService
	{
		public static void Map(AsanaTask task, long workspaceId)
		{
			Map(task, task.projects.Any() ? task.projects.First().id : 0, workspaceId);
		}

		public static void Map(AsanaTask task, long projectId, long workspaceId)
		{
			if (task.assignee != null)
			{
				task.assigneeid = task.assignee.id;
			}

			task.projectid = projectId;
			task.workspaceid = workspaceId;
			task.IsPriorityHeading = task.name.EndsWith(":");
            

			if (task.parent != null)
			{
				task.parentId = task.parent.id;
			}

			if (task.followers != null)
			{
				task.SetFollowers(task.followers.Select(x => x.id).ToList());
			}
		}

		public static AsanaTaskDTO CreateTaskDTO(long id, string name, string notes, long projectId, string assigneeStatus, long assigneeId, bool completed, DateTime? dueDate, List<long> followers)
		{
			var dto = new AsanaTaskDTO();
			dto.id = id;
			dto.assignee_status = assigneeStatus;
			dto.assignee = assigneeId > 0 ? assigneeId.ToString() : AsanaConstants.Utils.NULL_VALUE;
			dto.completed = completed;

			if (dueDate.HasValue)
			{
				dto.due_on = dueDate.Value.ToString("yyyy-MM-dd");
			}
			else
			{
				dto.due_on = AsanaConstants.Utils.NULL_VALUE;
			}
			dto.name = name;
			dto.notes = notes;
			dto.projects = projectId;

			if (followers != null)
			{
				dto.followers = followers.Select(c => new AsanaFollower() { id = c }).ToList();
			}


			return dto;
		}

		public static async Task FillTaskInfo(AsanaTask task)
		{
		    var project = await new StorageService().Find<AsanaProject>(task.projectid);
			if (project != null)
			{
				task.ProjectName = project.name;
			}
		}

		public static async Task FillSubtasksInfo(AsanaTask task)
		{
			var subtasks = await new StorageService().GetAllSubTasks(task.id);

			FillSubtasksInfo(task, subtasks);
		}

		public static void FillSubtasksInfo(AsanaTask task, List<AsanaTask> subtasks)
		{

			task.TasksCountText = string.Format("active: {0}, total: {1}", subtasks.Count(x => !x.completed),
													 TextHelper.GetText(subtasks.Count, "task", "tasks", "tasks"));
			task.DisplayTasksCount = subtasks.Any();
		}

	}
}
