using System.Collections.Generic;

using System.Linq;



using System;
using DevRain.Asana.New.Core.Shared;

namespace DevRain.Asana.API.Data.Models
{

	public class AsanaUser:BaseAsanaModel
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

        public void SetAvailableWorkspaces(List<AWorkspace>  workspaces)
        {
            WorkspacesIds = String.Join(AsanaConstants.Utils.WORKSPACES_SEPARATOR, workspaces.Select(x => x.id.ToString()));
        }


        public List<AWorkspace> workspaces { get; set; }

        public decimal TasksCount { get; set; }
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