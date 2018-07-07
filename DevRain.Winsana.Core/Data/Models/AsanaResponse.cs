

using System;
using System.Net;
using System.Runtime.Serialization;

namespace DevRain.Asana.API.Data.Models
{
	public class AsanaResponse<T> 
	{
        public string StatusDescription { get; set; }

	    public Exception ErrorException { get; set; }

	    public string ErrorMessage { get; set; }

        public string RequestUri { get; set; }

	    public HttpStatusCode StatusCode { get; set; }
        public string Content { get; set; }

		public T Data { get; set; }


		public AsanaErrors Errors { get; set; }

	}

	public class AsanaError
	{
		public string message { get; set; }
	}
}