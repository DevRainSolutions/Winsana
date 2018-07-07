using System;
using System.Collections.Generic;
using DevRain.Asana.New.Core.Shared;


namespace DevRain.Asana.API.Data.Models
{
    public class AWorkspacesResponse : AResponse
    {
        public List<AWorkspace> Data { get; set; }
    }

	public class AWorkspace : BaseAsanaModel
	{
       
        public int Order { get; set; }


        public string name { get; set; }


		public decimal ProjectsCount { get; set; }
		public string ProjectsCountText
		{
			get { return TextHelper.GetText(ProjectsCount, "project", "projects", "projects", "no projects"); }
		}

		public List<ATask> Tasks { get; set; }

		/// <summary>
		/// FOR ALL TASKS PIVOT
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return name.ToLower();
		}




		private ATask _selectedTask;
		public ATask SelectedTask
		{
			get { return _selectedTask; }
			set
			{
				_selectedTask = value;
				OnPropertyChanged("SelectedTask");
			}
		}
	}
}