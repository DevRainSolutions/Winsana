using System.Collections.Generic;

namespace DevRain.Asana.API.Data.Models
{
	public class AsanaNavigation
	{
		public string Name { get; set; }

		public List<AsanaProject> Projects { get; set; }

		public long Id { get; set; }
	}
}