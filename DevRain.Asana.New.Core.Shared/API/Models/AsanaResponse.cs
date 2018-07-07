

namespace DevRain.Asana.API.Data.Models
{
	public class AResponse
	{
        public AErrors Errors { get; set; }

	}

	public class AError
	{
		public string message { get; set; }
	}
}