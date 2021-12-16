using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;

namespace CrypUpdater
{
    public partial class App : Application
    {
        public const string ZipInstallLogHeader = "CrypTool 2 Zip Installation";

        private bool mayRestart = false;
        private static string CrypToolExePath;
        private string filePath;
        private string CrypToolFolderPath;

        private readonly string tempPath;
        private readonly string logfilePath;
        private int CrypToolProcessID;
        private Process p;
        private List<Process> unwantedProcesses = new List<Process>();

        private App()
        {
            tempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CrypTool2", "Temp");
            logfilePath = Path.Combine(tempPath, "install.txt");
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // flomar, 04/08/2011: we don't want users to use the auto updater as stand-alone 
            // application; therefore we exit with an error message if the application was 
            // started without any additional arguments (such as CT2 folder path...)
            if (e.Args.Length <= 1)
            {
                MessageBox.Show("You shouldn't run this program.\n\nIt is part of CrypTool 2, and it is invoked automatically during updates.");
                return;
            }

            try
            {
                filePath = e.Args[0];
                CrypToolFolderPath = e.Args[1];
                CrypToolExePath = e.Args[2];
                CrypToolProcessID = Convert.ToInt32(e.Args[3]);
                try
                {
                    p = Process.GetProcessById(CrypToolProcessID);
                }
                catch (Exception)
                {
                    p = null;
                }

                if (e.Args[4].ToLower() == "-justrestart")
                {
                    if ((p == null) || p.WaitForExit(1000 * 30))
                    {
                        RestartCrypTool();
                    }
                    else
                    {
                        p.Kill();
                        p.WaitForExit();
                        RestartCrypTool();
                    }
                    return;
                }
                else
                {
                    mayRestart = Convert.ToBoolean(e.Args[4]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error: {0}", ex.Message));
                return;
            }

            try
            {
                if ((p == null) || p.WaitForExit(1000 * 30))
                {
                    StartUpdateProcess();
                }
                else
                {
                    MessageBoxButton b = MessageBoxButton.OKCancel;
                    string caption = "Timeout error";
                    MessageBoxResult result;
                    result = MessageBox.Show("CrypTool 2 failed to shut down. Kill the process to proceed?", caption, b);
                    if (result == MessageBoxResult.OK)
                    {
                        try
                        {
                            p.Kill();
                            p.WaitForExit();
                            StartUpdateProcess();
                        }
                        catch (Exception)
                        {
                            StartUpdateProcess();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Update failed. CrypTool 2 will be restarted.");
                    }
                }

            }
            catch (IndexOutOfRangeException) // parameter not set
            {
                if (CrypToolExePath != null)
                {
                    MessageBox.Show("Update failed. CrypTool 2 will be restarted.", "Error");
                }
                else
                {
                    UpdateFailure();
                }
            }
            catch (FormatException) // no id or mayrestart was parsable 
            {
                UpdateFailure();
            }
            catch (ArgumentException) // the invoking process has already exited (no such process with this id exists)
            {
                StartUpdateProcess();
            }

            if (mayRestart)
            {
                File.Delete(filePath);
                RestartCrypTool();
            }
            else
            {
                App.Current.Shutdown();
            }
        }

        private void StartUpdateProcess()
        {
            // make sure we have a valid temp path
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            unwantedProcesses = FindCrypToolProcesses();
            if (unwantedProcesses.Count == 0)
            {
                if (filePath.EndsWith("msi"))
                {
                    StartMSI();
                }
                else if (filePath.EndsWith("exe"))
                {
                    StartNSIS();
                }
                else
                {
                    UnpackZip(filePath, CrypToolFolderPath);
                }
            }
            else
            {
                AskForLicenseToKill();
            }
        }

        private void StartMSI()
        {
            // flomar, 04/01/2011: from now on, whenever someone wants to upgrade to an MSI installation 
            // we warn the user that he should switch to an NSIS installation (TODO: this could be i18n-ed)
            MessageBox.Show("You are about to install an MSI-based installation of CrypTool 2. MSI-based installations will soon no longer be supported. We suggest you uninstall your existing MSI-based installation and upgrade to the new NSIS-based installation instead (visit the CrypTool 2 download page).", "Warning");

            try
            {
                DirectorySecurity ds = Directory.GetAccessControl(CrypToolFolderPath);

                Process p = new Process();
                p.StartInfo.FileName = "msiexec.exe";
                p.StartInfo.Arguments = "/i \"" + filePath + "\" /qb /l* " + logfilePath + " INSTALLDIR=\"" + CrypToolFolderPath + "\"";
                p.Start();
                p.WaitForExit();
                if (p.ExitCode != 0)
                {
                    MessageBox.Show("The exit code is not equal to zero. See log file for more information. CrypTool 2 will be restarted.", "Error");
                }
            }
            catch (UnauthorizedAccessException)
            {
                WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());

                if (!pricipal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    Process p = new Process();
                    p.StartInfo.FileName = "msiexec.exe";
                    p.StartInfo.Arguments = "/i \"" + filePath + "\" /qb /l* " + logfilePath + " INSTALLDIR=\"" + CrypToolFolderPath + "\"";
                    p.StartInfo.UseShellExecute = true;
                    p.StartInfo.Verb = "runas";
                    p.Start();
                    p.WaitForExit();
                    if (p.ExitCode != 0)
                    {
                        MessageBox.Show("The exit code is not equal to zero. See log file for more information. CrypTool 2 will be restarted.", "Error");
                    }
                }
                else
                {
                    MessageBox.Show("MSI update failed: CrypTool 2 will be restarted.", "Error");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("MSI update failed: " + e.Message + "\n" + "CrypTool 2 will be restarted.", "Error");
            }
        }

        private void StartNSIS()
        {
            try
            {
                // flomar, 05/09/2011: if the about-to-be-installed NSIS update executable is older than the current version,
                // we silently delete the old update executable; executables are considered "old" if (a) their version is smaller 
                // than the current version, or if (b) the new version cannot be determined (which implicitly makes it an old version)
                FileVersionInfo oldExecutableFileVersionInfo = FileVersionInfo.GetVersionInfo(filePath);
                FileVersionInfo newExecutableFileVersionInfo = FileVersionInfo.GetVersionInfo(CrypToolExePath);
                // if the version cannot be determined (i.e. versions weren't set up until a recent fix), null is returned
                string oldVersion = oldExecutableFileVersionInfo.ProductVersion;
                string newVersion = newExecutableFileVersionInfo.ProductVersion;

                bool isUpdateExecutableOutOfDate = false;

                // the update is out of date if its version cannot be determined
                if (newVersion == null)
                {
                    isUpdateExecutableOutOfDate = true;
                }
                else
                {
                    // the update is out of date if its version string has zero length
                    if (newVersion.Length == 0)
                    {
                        isUpdateExecutableOutOfDate = true;
                    }
                    else
                    {
                        // split the product version into separate strings (MAJOR.MINOR.REVISION.SVNREVISION)
                        string[] arrayNewVersion = newVersion.Split('.');
                        // if something's wrong here, the update is out of date
                        if (arrayNewVersion.Length != 4)
                        {
                            isUpdateExecutableOutOfDate = true;
                        }
                        else
                        {
                            // the actual version comparison will be in effect only for "valid" old versions;
                            // for "invalid" old versions every new version is considered a valid update
                            if (oldVersion != null)
                            {
                                // split the product version into separate strings (MAJOR.MINOR.REVISION.SVNREVISION)
                                string[] arrayOldVersion = oldVersion.Split('.');

                                if (arrayOldVersion.Length == 4)
                                {
                                    // transform version strings to integer values
                                    int oldMajor = int.Parse(arrayOldVersion.ElementAt(0));
                                    int oldMinor = int.Parse(arrayOldVersion.ElementAt(1));
                                    int oldRevision = int.Parse(arrayOldVersion.ElementAt(2));
                                    int oldSvnRevision = int.Parse(arrayOldVersion.ElementAt(3));
                                    int newMajor = int.Parse(arrayNewVersion.ElementAt(0));
                                    int newMinor = int.Parse(arrayNewVersion.ElementAt(1));
                                    int newRevision = int.Parse(arrayNewVersion.ElementAt(2));
                                    int newSvnRevision = int.Parse(arrayNewVersion.ElementAt(3));
                                    // compare old and new version
                                    if (oldMajor > newMajor)
                                    {
                                        isUpdateExecutableOutOfDate = true;
                                    }

                                    if (oldMajor >= newMajor && oldMinor > newMinor)
                                    {
                                        isUpdateExecutableOutOfDate = true;
                                    }

                                    if (oldMajor >= newMajor && oldMinor >= newMinor && oldRevision > newRevision)
                                    {
                                        isUpdateExecutableOutOfDate = true;
                                    }

                                    if (oldMajor >= newMajor && oldMinor >= newMinor && oldRevision >= newRevision && oldSvnRevision > newSvnRevision)
                                    {
                                        isUpdateExecutableOutOfDate = true;
                                    }
                                }
                            }
                        }
                    }
                }

                if (isUpdateExecutableOutOfDate)
                {
                    File.Delete(filePath);
                }
                else
                {
                    DirectorySecurity ds = Directory.GetAccessControl(CrypToolFolderPath);

                    Process p = new Process();
                    p.StartInfo.FileName = filePath;
                    p.StartInfo.Arguments = "/S >" + logfilePath;
                    p.Start();
                    p.WaitForExit();
                    if (p.ExitCode != 0)
                    {
                        MessageBox.Show("The exit code is not equal to zero. See log file for more information. CrypTool 2 will be restarted.", "Error");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());

                if (!pricipal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    Process p = new Process();
                    p.StartInfo.FileName = filePath;
                    p.StartInfo.Arguments = "/S >" + logfilePath;
                    p.StartInfo.UseShellExecute = true;
                    p.StartInfo.Verb = "runas";
                    p.Start();
                    p.WaitForExit();
                    if (p.ExitCode != 0)
                    {
                        MessageBox.Show("The exit code is not equal to zero. See log file for more information. CrypTool 2 will be restarted.", "Error");
                    }
                }
                else
                {
                    MessageBox.Show("NSIS update failed: CrypTool 2 will be restarted.", "Error");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("NSIS update failed: " + e.Message + "\n" + "CrypTool 2 will be restarted.", "Error");
            }
        }

        private void AskForLicenseToKill()
        {
            MessageBoxButton mbb = MessageBoxButton.YesNo;
            string caption = "Error";
            string messagePart1;
            string messagePart2;
            string messagePart3;
            if (unwantedProcesses.Count > 1)
            {
                messagePart1 = "Several instances";
                messagePart2 = "are";
                messagePart3 = "these processes";
            }
            else
            {
                messagePart1 = "Another instance";
                messagePart2 = "is";
                messagePart3 = "this process";
            }
            MessageBoxResult result;
            result = MessageBox.Show(messagePart1 + " of CrypTool 2 using the same resources " + messagePart2 + " still running. Kill " + messagePart3 + " to proceed?", caption, mbb);
            if (result == MessageBoxResult.Yes)
            {
                KillOtherProcesses();
                StartUpdateProcess();
            }
            else
            {
                MessageBox.Show("Update failed. CrypTool 2 will be restarted.");
            }
        }

        private void KillOtherProcesses()
        {
            try
            {
                foreach (Process pr in unwantedProcesses)
                {
                    if (!pr.HasExited)
                    {
                        pr.Kill();
                        pr.WaitForExit();
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Update failed. Not able to remove remaining CrypTool 2 instances.", "Error");
            }
        }

        private void UpdateFailure()
        {
            MessageBox.Show("Update failed, wrong parameters!", "Error");
            Application.Current.Shutdown();
        }

        private void RestartCrypTool()
        {
            try
            {
                // flomar, 05/09/2011: restart CrypTool (and don't forget to set the working directory!)
                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.WorkingDirectory = CrypToolFolderPath;
                p.StartInfo.FileName = CrypToolExePath;
                p.Start();
                Application.Current.Shutdown();
            }
            catch (Exception)
            {
                MessageBoxButton bu = MessageBoxButton.OK;
                string caption2 = "Error";
                MessageBoxResult res = MessageBox.Show("CrypTool 2 could not be restarted! Try again later.", caption2, bu);
                if (res == MessageBoxResult.OK)
                {
                    Application.Current.Shutdown();
                }
            }
        }

        private void UnpackZip(string ZipFilePath, string CrypToolFolderPath)
        {
            MainWindow loadDialog = new CrypUpdater.MainWindow();
            Task extractionTask = Task.Run(() => InternalUnpackZip(ZipFilePath, CrypToolFolderPath));
            extractionTask.ContinueWith(_ => loadDialog.CloseOnFinish());   //Close dialog after extraction finished.

            loadDialog.ShowDialog();    //This call will wait in the UI event loop until the dialog gets closed.
        }

        private void InternalUnpackZip(string ZipFilePath, string CrypToolFolderPath)
        {
            try
            {
                DirectorySecurity ds = Directory.GetAccessControl(CrypToolFolderPath);

                using (ZipFile zip = ZipFile.Read(ZipFilePath))
                {
                    // flomar, 10/27/2011: check whether we have a "ZipInstall.log" file in our
                    // CrypToolFolderPath-- if so, read in all listed files and delete them
                    string fileInstalledFiles = Path.Combine(CrypToolFolderPath, "ZipInstall.log");
                    if (File.Exists(fileInstalledFiles))
                    {

                        StreamReader reader = File.OpenText(fileInstalledFiles);

                        string header = reader.ReadLine(); // read first line
                        // attempt deletion only if file header matches
                        if (header == ZipInstallLogHeader)
                        {
                            // this list stores all files that we couldn't delete during uninstall (for whatever reason),
                            // and we're going to use this list to notify the user after the update process (if necessary)
                            List<string> listUndeletedFiles = new List<string>();

                            string filename = null;
                            while ((filename = reader.ReadLine()) != null)
                            {
                                // ignore empty or relative entries
                                if (string.IsNullOrWhiteSpace(filename) || filename.Contains(".."))
                                {
                                    continue;
                                }

                                try
                                {
                                    // delete files only, not directories (on purpose)
                                    File.Delete(Path.Combine(CrypToolFolderPath, filename));
                                }
                                catch (Exception)
                                {
                                    listUndeletedFiles.Add(filename);
                                }
                            }
                            reader.Close();

                            // now, if any errors occured, inform the user
                            if (listUndeletedFiles.Count > 0)
                            {
                                MessageBox.Show("One or more previously installed files could not be deleted.\n" +
                                "Proceeding with unpack anyway.", "Error", MessageBoxButton.OK);
                            }
                        }
                    }

                    // flomar, 10/27/2011: now extract the archive as usual, regardless of errors during uninstall
                    foreach (ZipEntry e in zip)
                    {
                        e.Extract(CrypToolFolderPath, ExtractExistingFileAction.OverwriteSilently);
                    }
                }

            }
            catch (UnauthorizedAccessException)
            {
                WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());

                if (!pricipal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    ProcessStartInfo psi = new ProcessStartInfo("CrypUpdater.exe", "\"" + ZipFilePath + "\" " + "\"" + CrypToolFolderPath + "\" " + "\"" + CrypToolExePath + "\" " + "\"" + CrypToolProcessID + "\" \"" + bool.FalseString + "\"")
                    {
                        UseShellExecute = true,
                        Verb = "runas",
                        WorkingDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)
                    };
                    Process cu = Process.Start(psi);
                    cu.WaitForExit();
                }
                else
                {
                    MessageBox.Show("Extraction failed: CrypTool 2 will be restarted.", "Error");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Extraction failed: " + e.Message + "\n" + "CrypTool 2 will be restarted.", "Error");
            }

        }


        private List<Process> FindCrypToolProcesses()
        {
            List<Process> processList = new List<Process>();

            try
            {
                Process[] p2 = Process.GetProcessesByName("CrypWin");
                foreach (Process p in p2)
                {
                    if (Path.GetDirectoryName(p.MainModule.FileName) == CrypToolFolderPath)
                    {
                        processList.Add(p);
                    }
                }
            }
            catch (Exception)
            {
                //32 bit updater cannot check for 64 bit processes
            }

            return processList;
        }

    }
}
