using System;
using System.Collections.Generic;
using DevRain.Windows.WinRT.Common.Helpers;
using SQLite;


namespace DevRain.Asana.API.Data.Models
{
	public class AsanaWorkspace : BaseAsanaDbEntity
	{

        public int Order { get; set; }




		public string name { get; set; }

                [Ignore]
		public decimal ProjectsCount { get; set; }
                [Ignore]
		public string ProjectsCountText
		{
			get { return TextHelper.GetText(ProjectsCount, "project", "projects", "projects", "no projects"); }
		}
                [Ignore]
		public List<AsanaTask> Tasks { get; set; }

		/// <summary>
		/// FOR ALL TASKS PIVOT
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return name.ToLower();
		}




		private AsanaTask _selectedTask;
                [Ignore]
		public AsanaTask SelectedTask
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