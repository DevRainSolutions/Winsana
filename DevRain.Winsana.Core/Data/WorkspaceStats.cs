using System.Collections.Generic;

namespace DevRain.Asana.API.Data
{
	public class WorkspaceStats
	{
		public WorkspaceStats()
		{
		}

		public List<WorkplaceStat> Stats { get; set; }
	}
}