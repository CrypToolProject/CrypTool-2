using CrypCloud.Core;
using CrypCloud.Manager.Properties;
using CrypCloud.Manager.ViewModels.Helper;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using VoluntLib2.ConnectionLayer;
using VoluntLib2.ManagementLayer;
using MessageBox = System.Windows.MessageBox;

namespace CrypCloud.Manager.ViewModels
{
    public class JobListVM : BaseViewModel
    {
        private readonly CrypCloudCore crypCloudCore = CrypCloudCore.Instance;
        public CrypCloudManager Manager { get; set; }

        public ObservableCollection<Job> RunningJobs { get; set; }

        public ObservableCollection<Contact> Contacts { get; set; }

        private Job selectedJob;
        public Job SelectedJob
        {
            get => selectedJob;
            set
            {
                if (selectedJob == value)
                {
                    return;
                }
                selectedJob = value;
            }
        }
        public RelayCommand RefreshJobListCommand { get; set; }
        public RelayCommand CreateNewJobCommand { get; set; }
        public RelayCommand OpenJobCommand { get; set; }
        public RelayCommand DeleteJobCommand { get; set; }
        public RelayCommand DownloadWorkspaceCommand { get; set; }
        public RelayCommand LogOutCommand { get; set; }

        private string _username;
        public string Username
        {
            get => Resources._JobList_LoggedInAs + " " + _username;
            set => _username = value;
        }

        public JobListVM()
        {
            CreateNewJobCommand = new RelayCommand(it => OpenJobCreation());
            RefreshJobListCommand = new RelayCommand(it => RefreshJobs());
            LogOutCommand = new RelayCommand(it => Logout());
            OpenJobCommand = new RelayCommand(OpenJob);
            DeleteJobCommand = new RelayCommand(DeleteJob);
            DownloadWorkspaceCommand = new RelayCommand(DownloadJob);
        }

        protected override void HasBeenActivated()
        {
            base.HasBeenActivated();
            RunningJobs = crypCloudCore.GetJoblist();
            RunningJobs.CollectionChanged += RunningJobs_CollectionChanged;
            RaisePropertyChanged("RunningJobs");
            if (RunningJobs.Count > 0)
            {
                selectedJob = RunningJobs.First();
                RaisePropertyChanged("SelectedJob");
            }
            Contacts = crypCloudCore.GetContacts();
            RaisePropertyChanged("Contacts");
            Username = CrypCloudCore.Instance.GetUsername();
            RaisePropertyChanged("Username");
            RaisePropertyChanged("UserCanCreateJob");
        }

        private void OpenJobCreation()
        {
            Navigator.ShowScreenWithPath(ScreenPaths.JobCreation);
        }

        private void RefreshJobs()
        {
            crypCloudCore.RefreshJobList();
        }

        private void Logout()
        {
            try
            {
                if (crypCloudCore.IsPartizipationOnJob())
                {
                    ErrorMessage = CrypCloud.Manager.Properties.Resources.Stop_Running_Jobs_Before_Logout;
                    return;
                }

                ErrorMessage = "";
                crypCloudCore.Logout();
                Navigator.ShowScreenWithPath(ScreenPaths.Login);
            }
            catch (Exception ex)
            {
                ErrorMessage = string.Format("Exception while logging out: {0}", ex.Message);
            }
        }

        #region job handling

        private void DownloadJob(object obj)
        {
            try
            {
                Job job = obj as Job;
                if (job == null)
                {
                    return;
                }

                crypCloudCore.DownloadWorkspaceOfJob(job.JobId);
            }
            catch (Exception ex)
            {
                ErrorMessage = string.Format("Exception while downloading job: {0}", ex.Message);
            }
        }

        private void OpenJob(object it)
        {
            try
            {
                Job job = it as Job;
                if (job == null)
                {
                    return; // shoudnt happen anyways
                }

                if (!job.HasPayload)
                {
                    ErrorMessage = "Cannot open job without downloading it before";
                    return;
                }

                WorkspaceManager.Model.WorkspaceModel workspaceModel = crypCloudCore.GetWorkspaceOfJob(job.JobId);
                CrypTool.PluginBase.Editor.IEditor workspaceEditor = workspaceModel.MyEditor;
                if (workspaceEditor == null || workspaceEditor.HasBeenClosed)
                {
                    UiContext.StartNew(() =>
                    {
                        try
                        {
                            Manager.OpenWorkspaceInNewTab(workspaceModel, job.JobId, job.JobName);
                        }
                        catch (Exception ex)
                        {
                            ErrorMessage = string.Format("Exception while opening new tab: {0}", ex.Message);
                        }

                    });
                }
                else
                {
                    ErrorMessage = "Workspace is already open.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = string.Format("Exception while opening job: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Returns "true" for everyone except the anonymous user, who is not allowed to create jobs.
        /// Thus, he does not need to see the CreateJob button
        /// </summary>
        /// <returns></returns>
        public bool UserCanCreateJob
        {
            get
            {
                try
                {
                    if (CrypCloudCore.Instance.GetUsername().ToLower().Equals("anonymous"))
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Returns true, if the user can delete a job
        /// </summary>
        public bool UserCanDeleteJob
        {
            get
            {
                try
                {
                    if (CrypCloudCore.Instance.GetUsername().ToLower().Equals("anonymous"))
                    {
                        return false;
                    }
                    foreach (Job job in RunningJobs)
                    {
                        if (job.UserCanDeleteJob)
                        {
                            return true;
                        }
                    }
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Updates the "Delete Job" column of JobList in case there is a change of the RunningJobsCollection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunningJobs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("UserCanDeleteJob");
        }

        #endregion

        private void DeleteJob(object it)
        {
            try
            {
                Job job = it as Job;
                if (job == null)
                {
                    return; // shoudnt happen anyways
                }

                MessageBoxResult confirmResult = MessageBox.Show(Resources._Confirm_Job_Deletion_Text, Resources._Confirm_Job_Deletion_Title, MessageBoxButton.YesNo);
                if (confirmResult == MessageBoxResult.Yes)
                {
                    crypCloudCore.DeleteJob(job.JobId);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = string.Format("Exception while deleting job: {0}", ex.Message);
            }
        }
    }
}
