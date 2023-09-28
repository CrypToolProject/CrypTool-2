/*
   Copyright 2008 - 2022 CrypTool Team

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
using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Management;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CrypTool.Core
{
    /// <summary>
    /// Interaction logic for UnhandledExceptionDialog.xaml
    /// </summary>
    public partial class ReportErrorDialog : Window
    {
        private readonly Exception _exception;
        private readonly Version _version;
        private readonly string _installationType;
        private readonly string _buildType;
        private readonly string _productName;
        private readonly string _systemInfos;

        public ReportErrorDialog(Exception exception, Version version, string installationType, string buildType, string productName)
        {
            _exception = exception;
            _version = version;
            _installationType = installationType;
            _buildType = buildType;
            _productName = productName;
            _systemInfos = GetSystemInfos();

            InitializeComponent();
            UpdateSendInformationsBox();
        }

        private void SendButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            // User clicked on "report" and then "send" at this point. Attempt to send mail.
            try
            {
                Mailer.SendMailToCoreDevs(Mailer.ACTION_TICKET, "Crash report", SendInformations.Text);
                MessageBox.Show("The error has been reported. Thank you!", "Reporting done");
                Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("Reporting failed: {0}. Please try again later.", e.Message), "Reporting failed");
            }
        }

        private void CancelButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            Close();
        }

        private string GetSystemInfos()
        {
            WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            bool hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);
            StringBuilder sb = new StringBuilder();
            //get windows information from system registry
            try
            {
                RegistryKey localKey;
                if (Environment.Is64BitOperatingSystem)
                {
                    localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                }
                else
                {
                    localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                }

                RegistryKey reg = localKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                string productName = (string)reg.GetValue("ProductName");
                int currentMajorVersionNumber = (int)reg.GetValue("CurrentMajorVersionNumber");
                int currentMinorVersionNumber = (int)reg.GetValue("CurrentMinorVersionNumber");
                string currentBuildNumber = (string)reg.GetValue("CurrentBuildNumber");
                if (currentBuildNumber.Equals("22000"))
                {
                    //hack, to replace Windows 10 with Windows 11 in the display
                    productName = "Windows 11";
                }
                string windowsVersionString = string.Format("{0} ({1}.{2}.{3})", productName, currentMajorVersionNumber, currentMinorVersionNumber, currentBuildNumber);
                sb.AppendLine(string.Format("Operating System: {0}", windowsVersionString));
            }
            catch (Exception)
            {
                //show fallback if its not possible to read from registration
                sb.AppendLine(string.Format("Operating System: {0}", System.Environment.OSVersion.ToString()));
            }
            sb.AppendLine(string.Format("Processorname: {0}", GetProcessorName()));
            sb.AppendLine(string.Format("Processors: {0}", System.Environment.ProcessorCount));
            sb.AppendLine(string.Format("Administrative rights: {0}", hasAdministrativeRight));
            sb.AppendLine(string.Format("Current culture: {0}", CultureInfo.CurrentUICulture.Name));
            sb.AppendLine(string.Format("CrypTool version: {0}", _version));
            sb.AppendLine(string.Format("Installation type: {0}", _installationType));
            sb.AppendLine(string.Format("Build type: {0}", _buildType));
            sb.AppendLine(string.Format("Build time: {0}", File.GetLastWriteTime(Assembly.GetEntryAssembly().Location)));
            sb.AppendLine(string.Format("Product name: {0}", _productName));
            sb.AppendLine(string.Format("Common language runtime version: {0}", Environment.Version.ToString()));
            sb.AppendLine(string.Format("System time: {0}", DateTime.Now.ToShortTimeString()));
            return sb.ToString();
        }

        public static void ShowModalDialog(Exception e, Version version, string installationType, string buildType, string productName)
        {
            UnhandledExceptionDialog unhandledExceptionDialog = new UnhandledExceptionDialog(e, version, installationType, buildType, productName);
            unhandledExceptionDialog.ShowDialog();
        }

        private void UserMessage_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateSendInformationsBox();
        }

        private void UpdateSendInformationsBox()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("Exception at {0} (UTC time).", DateTime.UtcNow));
            sb.AppendLine("User message:");
            sb.AppendLine(UserMessage.Text);
            sb.AppendLine("-");
            sb.AppendLine("Exception:");
            sb.AppendLine(_exception.ToString());
            sb.AppendLine("");
            //here, we append possible inner exceptions
            Exception exception = _exception.InnerException;
            while (exception != null)
            {
                sb.AppendLine("Inner Exception:");
                sb.AppendLine(exception.ToString());
                exception = exception.InnerException;
                sb.AppendLine("");
            }
            sb.AppendLine("-");
            sb.AppendLine("System infos:");
            sb.AppendLine(_systemInfos);

            SendInformations.Text = sb.ToString();
        }

        /// <summary>
        /// Returns the concatenated names of all processors
        /// </summary>
        /// <returns></returns>
        private string GetProcessorName()
        {
            try
            {
                ManagementObjectSearcher query = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                string name = string.Empty;
                ManagementObjectCollection collection = query.Get();
                int i = 1;
                foreach (ManagementBaseObject processor in collection)
                {
                    if (processor["name"] != null)
                    {
                        name += processor["name"].ToString();
                        if (i < collection.Count)
                        {
                            name += ", ";
                        }
                    }
                    i++;
                }
                return name;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
