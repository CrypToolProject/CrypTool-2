using System;
using System.Globalization;
using System.IO;
using System.Management;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace CrypTool.Core
{
    /// <summary>
    /// Interaction logic for UnhandledExceptionDialog.xaml
    /// </summary>
    public partial class ReportErrorDialog : Window
    {
        private readonly Exception _e;
        private readonly Version _version;
        private readonly string _installationType;
        private readonly string _buildType;
        private readonly string _productName;
        private string _systemInfos;

        public ReportErrorDialog(Exception e, Version version, string installationType, string buildType, string productName)
        {
            _e = e;
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
            var pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            var hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);
            var sb = new StringBuilder();
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

                var reg = localKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                var productName = (string)reg.GetValue("ProductName");
                var csdVersion = (string)reg.GetValue("CSDVersion");
                var currentVersion = (string)reg.GetValue("CurrentVersion");
                var currentBuildNumber = (string)reg.GetValue("CurrentBuildNumber");
                var windowsVersionString = productName + " " + csdVersion + " (" + currentVersion + "." + currentBuildNumber + ")";
                sb.AppendLine(string.Format("Operating System: {0}", windowsVersionString));
            }
            catch (Exception ex)
            {
                //show fallback if its not possible to read from registration
                sb.AppendLine(string.Format("Operating System: {0}", System.Environment.OSVersion.ToString()));
            }            
            //sb.AppendLine(string.Format("Plattform: {0}", Environment.OSVersion.Platform)); // always Win32NT
            sb.AppendLine(string.Format("Processorname: {0}", GetProcessorName()));
            sb.AppendLine(string.Format("Processors: {0}", System.Environment.ProcessorCount));
            //sb.AppendLine(string.Format("Process Info: {0}", (System.Environment.Is64BitProcess ? "64 Bit" : "32 Bit"))); // always 32 Bit
            sb.AppendLine(string.Format("Administrative Rights: {0}", hasAdministrativeRight));
            sb.AppendLine(string.Format("Current culture: {0}", CultureInfo.CurrentUICulture.Name));
            sb.AppendLine(string.Format("CrypTool version: {0}", _version));
            sb.AppendLine(string.Format("Installation type: {0}", _installationType));
            sb.AppendLine(string.Format("Build type: {0}", _buildType));
            sb.AppendLine(string.Format("Build time: {0}", File.GetLastWriteTime(Assembly.GetEntryAssembly().Location)));
            sb.AppendLine(string.Format("Product name: {0}", _productName));
            sb.AppendLine(string.Format("Common language runtime version: {0}", Environment.Version.ToString()));
            sb.AppendLine(string.Format("System time: {0}", DateTime.Now.ToShortTimeString()));
            sb.AppendLine(string.Format("Command line: {0}", Environment.CommandLine));

            return sb.ToString();
        }

        public static void ShowModalDialog(Exception e, Version version, string installationType, string buildType, string productName)
        {
            var unhandledExceptionDialog = new UnhandledExceptionDialog(e, version, installationType, buildType, productName);
            unhandledExceptionDialog.ShowDialog();
        }

        private void UserMessage_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateSendInformationsBox();
        }

        private void UpdateSendInformationsBox()
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("Exception at {0} (UTC time).", DateTime.UtcNow));
            sb.AppendLine("User message:");
            sb.AppendLine(UserMessage.Text);
            sb.AppendLine("-");
            sb.AppendLine("Exception:");
            sb.AppendLine(_e.ToString());
            sb.AppendLine("");
            //here, we append possible inner exceptions
            var e = _e.InnerException;
            while(e != null)
            {
                sb.AppendLine("Inner Exception:");
                sb.AppendLine(e.ToString());
                e = e.InnerException;
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
                var query = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                string name = String.Empty;
                var collection = query.Get();
                var i = 1;
                foreach (var processor in collection)
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
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
