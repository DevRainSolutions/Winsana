 using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;

using DevRain.WP.Common.Helpers;

using System;
 using SQLite;

namespace DevRain.Asana.API.Data.Models
{
	
	public class AsanaUser:BaseAsanaDbEntity
	{
        
        public int Order { get; set; }

        
        public string name { get; set; }

        /// <summary>
        /// SEPARATED BY ;
        /// </summary>
        public string WorkspacesIds { get; set; }

        public List<long> GetAvailableWorkspaces()
        {
            var l = WorkspacesIds.Split(new string[] {AsanaConstants.Utils.WORKSPACES_SEPARATOR},StringSplitOptions.RemoveEmptyEntries);

            return l.Select(x => long.Parse(x)).ToList();

        }

        public void SetAvailableWorkspaces(List<AsanaWorkspace>  workspaces)
        {
            WorkspacesIds = String.Join(AsanaConstants.Utils.WORKSPACES_SEPARATOR, workspaces.Select(x => x.id.ToString()));
        }

        [Ignore]
        public List<AsanaWorkspace> workspaces { get; set; }

            [Ignore]
        public decimal TasksCount { get; set; }

            [Ignore]
        public string TasksCountText
        {
            get { return TextHelper.GetText(TasksCount, "task", "tasks", "tasks", "no tasks"); }
        }

        public override string ToString()
        {
            return name;
        }
	}
}