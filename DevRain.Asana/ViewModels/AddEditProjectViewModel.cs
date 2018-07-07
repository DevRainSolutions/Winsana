using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Navigation;
using DevRain.Asana.API;
using DevRain.Asana.API.Data;
using DevRain.Asana.API.Data.Models;
using DevRain.Asana.API.Services.API;
using DevRain.Asana.API.Services.Db;
using DevRain.Asana.API.Storage;
using DevRain.Asana.Services;
using DevRain.WP.Core.MVVM.Commands;
using DevRain.WP.Core.MVVM.State;

namespace DevRain.Asana.ViewModels
{
    public class AddEditProjectViewModel : AsanaViewModel
    {
        protected override void OnCreate()
        {
            SaveProjectCommand = new RelayCommand(o=>SaveProject());
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {

            if (NavigationManager.GetQueryParameter("workspaceId") != null)
            {
                WorkspaceId = long.Parse(NavigationManager.GetQueryParameter("workspaceId"));

            }

            if (NavigationManager.GetQueryParameter("projectId") != null)
            {
                Id = long.Parse(NavigationManager.GetQueryParameter("projectId"));
                PageTitleText = "edit project";
            }
            else
            {
                PageTitleText = "create project";
            }

        }

        protected override void OnLoad()
        {
            LoadData();
        }


        void SetData(AsanaProject project)
        {
           
            IsArchived = project.archived;
            Name = project.name;
            Notes = project.notes;
        }

        private async void LoadData()
        {

            IsBusy = true;


            if (IsEditMode)
            {
                var project = await new StorageService().Find<AsanaProject>(Id.Value);
                WorkspaceId = project.workspaceid;

                SetData(project);

                //if(model.CheckInternetConnection(false))
                //{
                //    var response = await AsanaClient.SendRequest(() => new AsanaRespository().GetProject(model.Id.Value));

                //    if(AsanaClient.ProcessResponse(response))
                //    {
                //        response.Data.workspaceid = response.Data.workspace.id;
                //        SetData(response.Data);

                //        using (var db = new DbTransaction())
                //        {
                //            db.InsertOrUpdate(response.Data);
                //            db.Commit();
                //        }
                //    }




                //}






            }
            IsBusy = false;
        }


        async void SaveProject()
        {

            if (IsBusy) return;


            if (string.IsNullOrEmpty(Name))
            {
                return;
            }
            IsBlockingProgress = true;
            var project = new AsanaProject { name = Name, notes = Notes, archived = IsArchived };




            if (IsEditMode)
            {
                project.id = Id.Value;
            }

            var response = !IsEditMode
                                ? await new AsanaRespository().CreateProject(WorkspaceId, project)
                                : await new AsanaRespository().UpdateProject(project);

            if (AsanaClient.ProcessResponse(response))
            {


                response.Data.workspaceid = response.Data.workspace.id;
                await new StorageService().Save(response.Data);


                    NavigationManager.GoBack();
                    return;
                


            }
            IsBlockingProgress = false;


        }


        public ICommand SaveProjectCommand { get; set; }

        public bool IsEditMode
        {
            get { return Id.HasValue; }
        }

		[Tombstoned]
        public long WorkspaceId { get; set; }

		[Tombstoned]
        public long? Id { get; set; }


        private bool isArchived;

				[Tombstoned]
        public bool IsArchived
        {
            get { return isArchived; }
            set
            {
                isArchived = value;
                NotifyOfPropertyChange("IsArchived");
            }
        }

        private string name;
			[Tombstoned]
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                NotifyOfPropertyChange("Name");
            }
        }

        private string notes;
			[Tombstoned]
        public string Notes
        {
            get { return notes; }
            set
            {
                notes = value;
                NotifyOfPropertyChange("Notes");
            }
        }

    }
}
