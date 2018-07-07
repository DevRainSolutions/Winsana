
// -----------------------------------------------------------------------

using System.Linq;
using System.Collections.Generic;

namespace DevRain.Asana.API.Data.Models
{
	public class AsanaErrors
	{
		public List<AsanaError> errors { get; set; } 
        
	    public bool IsUnknownObject
	    {
	        get { return errors.Any(x => x.message.Contains("Unknown object")); }
	    }

	}
}
