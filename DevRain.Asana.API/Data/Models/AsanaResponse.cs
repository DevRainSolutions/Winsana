using RestSharp;

namespace DevRain.Asana.API.Data.Models
{
	public class AsanaResponse<T> : RestResponse, IRestResponse where T:class 
	{
        


		public T Data { get; set; }


		public AsanaErrors Errors { get; set; }

	}

	public class AsanaError
	{
		public string message { get; set; }
	}
}