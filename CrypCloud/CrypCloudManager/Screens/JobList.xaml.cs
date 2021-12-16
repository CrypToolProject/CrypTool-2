/*
   Copyright 2019 CrypTool 2 Team <ct2contact@CrypTool.org>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using CrypCloud.Manager.ViewModels;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CrypCloud.Manager.Screens
{
    [CrypTool.PluginBase.Attributes.Localization("CrypCloud.Manager.Properties.Resources")]
    public partial class JobList : UserControl
    {
        public JobList()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Copies job information to the clipboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyJobInfo_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (DataContext == null)
                {
                    return;
                }
                JobListVM jobListVM = (JobListVM)DataContext;
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("CrypCloud – Job from " + DateTime.Now);
                stringBuilder.AppendLine("- Job ID: " + ByteArrayToString(jobListVM.SelectedJob.JobId.ToByteArray()));
                stringBuilder.AppendLine("- Job name: " + jobListVM.SelectedJob.JobName);
                stringBuilder.AppendLine("- Job size (bytes): " + jobListVM.SelectedJob.JobSize);
                stringBuilder.AppendLine("- Creation date: " + jobListVM.SelectedJob.CreationDate.ToLocalTime().ToString("g"));
                stringBuilder.AppendLine("- Creator name: " + jobListVM.SelectedJob.CreatorName);
                stringBuilder.AppendLine("- Job description: " + jobListVM.SelectedJob.JobDescription);
                stringBuilder.AppendLine("- Number of blocks: " + jobListVM.SelectedJob.NumberOfBlocks);
                stringBuilder.AppendLine("- Number of calculated blocks: " + jobListVM.SelectedJob.NumberOfCalculatedBlocks);
                Clipboard.SetText(stringBuilder.ToString());
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        /// <summary>
        /// Copies the instructions to the clipboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyInstructions_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(Properties.Resources._JobList_instructions);
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        /// <summary>
        /// Copies the list of contacts to the clipboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyConnectedContacts_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                JobListVM jobListVM = (JobListVM)DataContext;
                stringBuilder.AppendLine("CrypCloud – Contactlist from " + DateTime.Now);
                foreach (VoluntLib2.ConnectionLayer.Contact element in jobListVM.Contacts)
                {
                    stringBuilder.AppendLine("Contact:");
                    stringBuilder.AppendLine("- PeerId:" + ByteArrayToString(element.PeerId));
                    stringBuilder.AppendLine("- IP:" + element.IPAddress);
                    stringBuilder.AppendLine("- Port:" + element.Port);
                    stringBuilder.AppendLine("- Last seen:" + element.LastSeen.ToLocalTime().ToString("g"));

                }
                Clipboard.SetText(stringBuilder.ToString());
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        /// <summary>
        /// Copies the list of jobs to the clipboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyJobList_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                JobListVM jobListVM = (JobListVM)DataContext;
                stringBuilder.AppendLine("CrypCloud – Joblist from " + DateTime.Now);
                foreach (VoluntLib2.ManagementLayer.Job job in jobListVM.RunningJobs)
                {
                    stringBuilder.AppendLine("Job:");
                    stringBuilder.AppendLine("- ID: " + ByteArrayToString(job.JobId.ToByteArray()));
                    stringBuilder.AppendLine("- Name: " + job.JobName);
                    stringBuilder.AppendLine("- Size (bytes): " + job.JobSize);
                    stringBuilder.AppendLine("- Creation date: " + job.CreationDate.ToLocalTime().ToString("g"));
                    stringBuilder.AppendLine("- Creator name: " + job.CreatorName);
                    stringBuilder.AppendLine("- Job description: " + job.JobDescription);
                    stringBuilder.AppendLine("- Number of blocks: " + job.NumberOfBlocks);
                    stringBuilder.AppendLine("- Number of calculated blocks: " + job.NumberOfCalculatedBlocks);
                }
                Clipboard.SetText(stringBuilder.ToString());
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        /// <summary>
        /// Converts the byte array to a hex-string
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        private static string ByteArrayToString(byte[] array)
        {
            return BitConverter.ToString(array).Replace("-", "");
        }

    }
}
