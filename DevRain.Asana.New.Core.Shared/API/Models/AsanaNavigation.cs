using System.Collections.Generic;

namespace DevRain.Asana.API.Data.Models
{
	public class ANavigation
	{
		public string Name { get; set; }

		public List<AProject> Projects { get; set; }

		public long Id { get; set; }
	}
}