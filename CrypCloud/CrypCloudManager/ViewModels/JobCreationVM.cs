using CrypCloud.Core;
using CrypCloud.Manager.Services;
using CrypCloud.Manager.ViewModels.Helper;
using System;
using VoluntLib2.Tools;
using WorkspaceManager.Model;

namespace CrypCloud.Manager.ViewModels
{
    public class JobCreationVM : BaseViewModel
    {
        private readonly CrypCloudCore crypCloudCore = CrypCloudCore.Instance;

        public string Name { get; set; }
        public string LocalFilePath { get; set; }
        public string Description { get; set; }

        public RelayCommand BackToListCmd { get; set; }
        public RelayCommand CreateNewJobCmd { get; set; }
        public RelayCommand SelectWorkspaceFromFilesystemCmd { get; set; }

        public JobCreationVM()
        {
            Description = "";
            Name = "";

            BackToListCmd = new RelayCommand(it => ShowListView());
            CreateNewJobCmd = new RelayCommand(it => CreateNewJob());
            SelectWorkspaceFromFilesystemCmd = new RelayCommand(it => SelectWorkspaceFromFilesystem());
        }

        private void ShowListView()
        {
            Navigator.ShowScreenWithPath(ScreenPaths.JobList);
        }

        private void SelectWorkspaceFromFilesystem()
        {
            LocalFilePath = WorkspaceHelper.OpenFilePickerAndReturnPath();
            RaisePropertyChanged("LocalFilePath");
        }

        public void CreateNewJob()
        {
            WorkspaceModel workspaceModel = TryDeserializeWorkspace(LocalFilePath);
            if (workspaceModel == null)
            {
                ErrorMessage = "Cannot load workspace from file";
                return;
            }

            try
            {
                bool jobHasBeenCreated = crypCloudCore.CreateJob("CrypToolJob", Name, Description, workspaceModel);
                if (jobHasBeenCreated)
                {
                    Navigator.ShowScreenWithPath(ScreenPaths.JobList);
                }
                else
                {
                    ErrorMessage = "Workspace does not contain a cloud component";
                }
            }
            catch (JobPayloadTooBigException)
            {
                ErrorMessage = "Selected cwm file is too big. Only 20KiB allowed!";
            }
            catch (Exception ex)
            {
                ErrorMessage = string.Format("Exception while creating job: {0}", ex.Message);
            }
        }

        private WorkspaceModel TryDeserializeWorkspace(string filePath)
        {
            try
            {
                return new ModelPersistance().loadModel(filePath);
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
