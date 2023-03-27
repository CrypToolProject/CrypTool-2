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
using CrypTool.PluginBase.Attributes;
using CrypTool.PluginBase.Miscellaneous;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CrypTool.CrypWin
{
    /// <summary>
    /// Interaction logic for SystemInfos.xaml
    /// </summary>
    [TabColor("pink")]
    [Localization("CrypTool.CrypWin.Properties.Resources")]
    public partial class SystemInfos : UserControl
    {
        /// <summary>
        /// Entry of the info list
        /// </summary>
        private class Info
        {
            public Info(int id)
            {
                Id = id;
            }

            public int Id { get; set; }
            public string Description { get; set; }
            public string Value { get; set; }
        }

        private readonly List<Info> informations = new List<Info>();
        private int timeIndex;

        public SystemInfos()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Updates the info list
        /// </summary>
        private void UpdateInfos()
        {
            int idcounter = 0;
            informations.Clear();
            WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            bool hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);
            //get windows information from system registry
            try
            {
                RegistryKey localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
                    Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);

                RegistryKey reg = localKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                string productName = (string)reg.GetValue("ProductName");
                int currentMajorVersionNumber = (int)reg.GetValue("CurrentMajorVersionNumber");
                int currentMinorVersionNumber = (int)reg.GetValue("CurrentMinorVersionNumber");
                string currentBuildNumber = (string)reg.GetValue("CurrentBuildNumber");
                //hack, to replace Windows 10 with Windows 11 in the display
                if (int.Parse(currentBuildNumber) >= 22621)
                {
                    
                    productName = "Windows 11 (2022 Update)";
                }
                else if (int.Parse(currentBuildNumber) >= 22000)
                {
                    productName = "Windows 11";
                }
                string windowsVersionString = string.Format("{0} ({1}.{2}.{3})", productName, currentMajorVersionNumber, currentMinorVersionNumber, currentBuildNumber);
                informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_Operating_System, Value = windowsVersionString });
            }
            catch (Exception)
            {
                //show fallback if its not possible to read from registration
                informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_Operating_System, Value = System.Environment.OSVersion.ToString() });
            }
            informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_System_Type, Value = System.Environment.Is64BitOperatingSystem ? Properties.Resources.SI_System_Type_64 : Properties.Resources.SI_System_Type_32 });
            //informations.Add(new Info(idcounter++) { Description = "Platform", Value = Environment.OSVersion.Platform.ToString() }); // always Win32NT
            //informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_Machine_Name, Value = System.Environment.MachineName });      //personal information
            informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_Processor_Name, Value = GetProcessorName() });
            informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_Processors, Value = System.Environment.ProcessorCount.ToString() });
            informations.Add(new Info(idcounter++) { Description = Properties.Resources.ProcessType, Value = (System.Environment.Is64BitProcess ? "64 Bit" : "32 Bit") });
            informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_Administrative_Rights, Value = hasAdministrativeRight ? Properties.Resources.SI_yes : Properties.Resources.SI_no });
            //informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_Unique_Identifier, Value = uniqueID });       //personal information
            //informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_Host_Name, Value = UniqueIdentifier.GetHostName() });     //personal information
            informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_Current_Culture, Value = CultureInfo.CurrentUICulture.Name });
            informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_CrypTool_Version, Value = AssemblyHelper.Version.ToString() });
            informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_Installation_Type, Value = AssemblyHelper.InstallationType.ToString() });
            informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_Build_Type, Value = AssemblyHelper.BuildType.ToString() });
            informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_Build_Time, Value = File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location).ToString(CultureInfo.CurrentUICulture.DateTimeFormat) });
            informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_Product_Name, Value = AssemblyHelper.ProductName });
            informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_Common_Language_Runtime_Version, Value = GetDotNetVersion.Get45PlusFromRegistry() + " (" + Environment.Version.ToString() + ")" });
            informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_Runtime_Path, Value = AppDomain.CurrentDomain.BaseDirectory });
            informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_CommandLine, Value = Environment.CommandLine });
            informations.Add(new Info(idcounter++) { Description = Properties.Resources.Java_Version, Value = GetJavaVersion() });

            // system time row with hacky workaround to update time when tab becomes visible
            timeIndex = informations.Count;
            informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_System_Time, Value = DateTime.Now.ToShortTimeString() });
            InfoGrid.IsVisibleChanged += UpdateTime;

            InfoGrid.DataContext = informations;

            Tag = FindResource("Icon");

            InfoGrid.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, new ExecutedRoutedEventHandler(MyExecutedRoutedEventHandler), new CanExecuteRoutedEventHandler(CanExecuteRoutedEventHandler)));

            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                string exe = asm.Location;
                X509Certificate2 executingCert = new X509Certificate2(X509Certificate2.CreateFromSignedFile(exe));

                if (executingCert != null)
                {
                    informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_IsSigned, Value = Properties.Resources.SI_yes });
                    informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_ValidCertificate, Value = (executingCert.Verify() ? Properties.Resources.SI_yes : Properties.Resources.SI_no) });
                    informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_Subject, Value = executingCert.Subject });
                    informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_IssuerName, Value = executingCert.Issuer });
                    informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_KeyAlgorithm, Value = executingCert.GetKeyAlgorithmParametersString() });
                    informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_PublicKey, Value = executingCert.GetPublicKeyString() });
                    informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_SerialNumber, Value = executingCert.GetSerialNumberString() });
                    informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_CertHash, Value = executingCert.GetCertHashString() });
                    informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_EffectiveDate, Value = executingCert.GetEffectiveDateString() });
                    informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_ExpirationDate, Value = executingCert.GetExpirationDateString() });
                }
                else
                {
                    informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_IsSigned, Value = Properties.Resources.SI_no });
                    informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_ValidCertificate, Value = Properties.Resources.SI_no });
                    informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_Subject, Value = Properties.Resources.SI_no });
                    informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_IssuerName, Value = Properties.Resources.SI_no });
                    informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_KeyAlgorithm, Value = Properties.Resources.SI_no });
                    informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_PublicKey, Value = Properties.Resources.SI_no });
                    informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_SerialNumber, Value = Properties.Resources.SI_no });
                    informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_CertHash, Value = Properties.Resources.SI_no });
                    informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_EffectiveDate, Value = Properties.Resources.SI_no });
                    informations.Add(new Info(idcounter++) { Description = Properties.Resources.SI_ExpirationDate, Value = Properties.Resources.SI_no });
                }
                AnalyzeLoadedAndReferencedAssemblies(ref idcounter);
            }
            catch (Exception)
            {
                //wtf?
            }
        }

        /// <summary>
        /// Puts all loaded or referenced assemblies in the list
        /// </summary>
        private void AnalyzeLoadedAndReferencedAssemblies(ref int idcounter)
        {
            List<string> referencedAssemblyNames = new List<string>();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                if (!referencedAssemblyNames.Contains(assembly.FullName))
                {
                    referencedAssemblyNames.Add(assembly.FullName);
                }
                foreach (AssemblyName reference in assembly.GetReferencedAssemblies())
                {
                    if (!referencedAssemblyNames.Contains(reference.FullName))
                    {
                        referencedAssemblyNames.Add(reference.FullName);
                    }
                }

            }
            referencedAssemblyNames.Sort((x, y) => x.CompareTo(y));
            foreach (string reference in referencedAssemblyNames)
            {
                informations.Add(new Info(idcounter++) { Description = Properties.Resources.LoadedReferencedAssembly, Value = reference });
            }
        }

        /// <summary>
        /// Returns the text output of java.exe -version
        /// </summary>
        /// <returns></returns>
        private string GetJavaVersion()
        {
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = "java.exe",
                    Arguments = " -version",
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                };
                Process process = Process.Start(processStartInfo);
                return process.StandardError.ReadLine().Split(' ')[2].Replace("\"", "");
            }
            catch (Exception)
            {
                return Properties.Resources.Java_could_not;
            }
        }

        public static string ByteArrayToHexString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex;
        }

        private void CanExecuteRoutedEventHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (InfoGrid.SelectedIndex >= 0);
        }

        /// <summary>
        /// Copy a single line
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyExecutedRoutedEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            Info info = (Info)InfoGrid.SelectedItem;
            string msg = info.Id + ": " + info.Description + ": " + info.Value;
            Clipboard.SetDataObject(msg);
        }

        /// <summary>
        /// Copy all lines
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            string msg = string.Join(Environment.NewLine, informations.Select(i => i.Id + ": " + i.Description + ": " + i.Value));
            Clipboard.SetDataObject(msg);
        }

        // update system time row, refresh grid
        private void UpdateTime(object sender, DependencyPropertyChangedEventArgs args)
        {
            // sanity checks
            if (args.Property == null || args.Property.Name != "IsVisible" || !(args.OldValue is bool) || !(args.NewValue is bool) || timeIndex > informations.Count)
            {
                return;
            }

            if (!(bool)args.OldValue && (bool)args.NewValue) // was false, becomes true
            {
                informations[timeIndex] = new Info(timeIndex) { Description = Properties.Resources.SI_System_Time, Value = DateTime.Now.ToShortTimeString() };
                InfoGrid.Items.Refresh();
            }
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
                ManagementObjectCollection collection = query.Get();
                List<ManagementBaseObject> l = new List<ManagementBaseObject>();
                foreach (ManagementBaseObject processor in collection)
                {
                    l.Add(processor);
                }

                return string.Join(", ", l.Where(p => p["Name"] != null).Select(p => p["Name"].ToString()));
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// When SystemInfos are visible, we update the infos
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="dependencyPropertyChangedEventArgs"></param>
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if ((bool)(dependencyPropertyChangedEventArgs.NewValue) == true)
            {
                UpdateInfos();
            }
        }
    }

    /// <summary>
    /// Obtained and adapted code from https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed
    /// </summary>
    public class GetDotNetVersion
    {
        public static string Get45PlusFromRegistry()
        {
            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

            using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
            {
                if (ndpKey != null && ndpKey.GetValue("Release") != null)
                {
                    return string.Format(CheckFor45PlusVersion((int)ndpKey.GetValue("Release")));
                }
                else
                {
                    return string.Format(".NET Framework Version 4.5 or later is not detected.");
                }
            }

            // Checking the version using >= enables forward compatibility.
            string CheckFor45PlusVersion(int releaseKey)
            {
                if (releaseKey >= 528040)
                {
                    return "4.8 " + Properties.Resources.SI_Or_newer;
                }
                if (releaseKey >= 461808)
                {
                    return "4.7.2";
                }
                if (releaseKey >= 461308)
                {
                    return "4.7.1";
                }
                if (releaseKey >= 460798)
                {
                    return "4.7";
                }
                if (releaseKey >= 394802)
                {
                    return "4.6.2";
                }
                if (releaseKey >= 394254)
                {
                    return "4.6.1";
                }
                if (releaseKey >= 393295)
                {
                    return "4.6";
                }
                if (releaseKey >= 379893)
                {
                    return "4.5.2";
                }
                if (releaseKey >= 378675)
                {
                    return "4.5.1";
                }
                if (releaseKey >= 378389)
                {
                    return "4.5";
                }
                // This code should never execute. A non-null release key should mean
                // that 4.5 or later is installed.
                return "No 4.5 or later version detected";
            }
        }
    }
}
