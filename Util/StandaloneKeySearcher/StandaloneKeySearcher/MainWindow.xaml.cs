/*
   Copyright 2019 Nils Kopal <Nils.Kopal<AT>cryptool.org>

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
using CrypCloud.Core;
using CrypTool.Core;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Editor;
using System;
using System.ComponentModel;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using VoluntLib2.ManagementLayer;
using WellKnownPeer;
using WorkspaceManager.Execution;
using WorkspaceManager.Model;

namespace StandaloneKeySearcher
{    
    public partial class MainWindow : Window, IEditor
    {
        private VoluntLib2.Tools.Logger _logger;
        private Thread _workerThread;
        private Job _job;
        private BigInteger _jobID;
        private WorkspaceModel _jobWorkspaceModel;
        private readonly CrypCloudCore crypCloudCore = CrypCloudCore.Instance;
        private KeySearcher.KeySearcher _keySearcher;
        private ExecutionEngine _executionEngine;
        private FileLogger _fileLogger;

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            _jobID = new BigInteger(HexStringToArray(Properties.Settings.Default.JobID));
            ConnectToCrypCloud();
            _workerThread = new Thread(WorkerThreadMethod);
            _workerThread.IsBackground = true;
            _workerThread.Start();
        }

        /// <summary>
        /// Converts a hex string to a byte array
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        private byte[] HexStringToArray(string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        /// <summary>
        /// Connects to crypcloud
        /// </summary>
        public void ConnectToCrypCloud()
        {
            try
            {
                _fileLogger = new FileLogger();
                CrypCloudCore.Instance.LogLevel = "Info";
                CrypCloudCore.Instance.EnableOpenCL = false;
                CrypCloudCore.Instance.AmountOfWorker = Properties.Settings.Default.Workers;
                _logger = VoluntLib2.Tools.Logger.GetLogger();                                
                _logger.LoggOccured += _logger_LoggOccured;
                if (CrypCloudCore.Instance.Login(LoadAnonymousCertificate("anonymous")))
                {
                    CrypCloudCore.Instance.RefreshJobList();
                }
            }
            catch (Exception ex)
            {
                _logger_LoggOccured(this, new VoluntLib2.Tools.LogEventArgs(VoluntLib2.Tools.Logtype.Error, string.Format("Exception during connecting to CrypCloud: {0}", ex.Message)));
            }

        }

        private void _logger_LoggOccured1(object sender, VoluntLib2.Tools.LogEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The connected thread is responsible for downloading the job, starting it, and showing the KeySearcher user interface
        /// </summary>
        private void WorkerThreadMethod()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(1000);
                    //1. Continuesly check if we are connected. If not we connect
                    if (CrypCloudCore.Instance.IsRunning == false)
                    {
                        _logger_LoggOccured(this, new VoluntLib2.Tools.LogEventArgs(VoluntLib2.Tools.Logtype.Info, "We are not connected to CrypCloud. Trying to connect."));
                        ConnectToCrypCloud();
                        Thread.Sleep(10000);
                        continue;
                    }

                    //2. Check, if we downloaded the needed job. If not, download it
                    if (_job == null)
                    {
                        _job = CrypCloudCore.Instance.GetJobById(_jobID);
                        if (_job == null || _job.HasPayload == false)
                        {
                            _logger_LoggOccured(this, new VoluntLib2.Tools.LogEventArgs(VoluntLib2.Tools.Logtype.Info, "We do not have the job. Trying to get it. This may take a while..."));
                            CrypCloudCore.Instance.RefreshJobList();
                            Thread.Sleep(5000);
                            continue;
                        }
                    }

                    //3. Get jobWorkspaceModel
                    if (_jobWorkspaceModel == null)
                    {
                        _logger_LoggOccured(this, new VoluntLib2.Tools.LogEventArgs(VoluntLib2.Tools.Logtype.Info, "We do not have the job workspace model. Trying to get it. This may take a while..."));
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                _jobWorkspaceModel = crypCloudCore.GetWorkspaceOfJob(_job.JobId);
                            }
                            catch (Exception ex)
                            {
                                _logger_LoggOccured(this, new VoluntLib2.Tools.LogEventArgs(VoluntLib2.Tools.Logtype.Error, string.Format("Exception while getting workspace of job: {0}", ex.Message)));
                            }
                        }));
                        Thread.Sleep(5000);
                        continue;
                    }

                    //4. Show KeySearcher
                    if (_keySearcher == null)
                    {
                        _logger_LoggOccured(this, new VoluntLib2.Tools.LogEventArgs(VoluntLib2.Tools.Logtype.Info, "Showing KeySearcher user interface now."));
                        foreach (var plugin in _jobWorkspaceModel.GetAllPluginModels())
                        {
                            if (plugin.Plugin is KeySearcher.KeySearcher)
                            {
                                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    try
                                    {
                                        _keySearcher = (KeySearcher.KeySearcher)plugin.Plugin;
                                        Grid.Children.Clear();
                                        Grid.Children.Add(_keySearcher.Presentation);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger_LoggOccured(this, new VoluntLib2.Tools.LogEventArgs(VoluntLib2.Tools.Logtype.Error, string.Format("Exception while showing KeySearcher ui: {0}", ex.Message)));
                                    }
                                }));
                            }
                        }
                    }

                    //5. Start execution
                    if (_executionEngine == null)
                    {
                        _logger_LoggOccured(this, new VoluntLib2.Tools.LogEventArgs(VoluntLib2.Tools.Logtype.Info, "Creating ExecutionEngine now."));
                        _executionEngine = new ExecutionEngine(this);
                        _logger_LoggOccured(this, new VoluntLib2.Tools.LogEventArgs(VoluntLib2.Tools.Logtype.Info, "Starting ExecutionEngine now."));
                        _executionEngine.Execute(_jobWorkspaceModel, false);
                    }
                }
                catch(Exception ex)
                {
                    _logger_LoggOccured(this, new VoluntLib2.Tools.LogEventArgs(VoluntLib2.Tools.Logtype.Error, string.Format("Exception in worker thread: {0}", ex.Message)));
                }
            }
        }

        /// <summary>
        /// Connects to CrypCloud using the anonymous certificate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _logger_LoggOccured(object sender, VoluntLib2.Tools.LogEventArgs e)
        {
            try
            {
                _fileLogger.OnLoggOccured(sender, e);
            }
            catch (Exception)
            {
                //do nothing
            }
            string log = DateTime.Now.ToLocalTime() + " " + e.Logtype + " " + e.Message;
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    Logg.Text = Logg.Text + log + Environment.NewLine;
                    Logg.ScrollToEnd();
                }
                catch (Exception)
                {
                    //do nothing
                }
            }));
        }

        /// <summary>
        /// Loads the anonymous certificate using the given password
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static X509Certificate2 LoadAnonymousCertificate(string password)
        {
            try
            {
                return new X509Certificate2(Properties.Resources.anonymous, password);
            }
            catch (Exception)
            {
                return null;
            }
        }

        #region IEditor

        public void New()
        {

        }

        public void Open(string fileName)
        {

        }

        public void Save(string fileName)
        {

        }

        public void Add(Type type)
        {

        }

        public void Undo()
        {

        }

        public void Redo()
        {

        }

        public void Cut()
        {

        }

        public void Copy()
        {

        }

        public void Paste()
        {

        }

        public void Remove()
        {

        }

        public void Print()
        {

        }

        public void AddText()
        {

        }

        public void AddImage()
        {

        }

        public void ShowSelectedEntityHelp()
        {

        }

        public void Execute()
        {

        }

        public void Stop()
        {

        }

        public void Initialize()
        {

        }

        public void Dispose()
        {

        }


        public ISettings Settings
        {
            get { return null; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public bool CanUndo
        {
            get
            {
                return false;
            }
        }

        public bool CanRedo
        {
            get
            {
                return false;
            }
        }
        public bool CanCut
        {
            get
            {
                return false;
            }
        }
        public bool CanCopy
        {
            get
            {
                return false;
            }
        }
        public bool CanPaste
        {
            get
            {
                return false;
            }
        }
        public bool CanRemove
        {
            get
            {
                return false;
            }
        }
        public bool CanExecute
        {
            get
            {
                return false;
            }
        }
        public bool CanStop
        {
            get
            {
                return false;
            }
        }
        public bool HasChanges
        {
            get
            {
                return false;
            }
        }
        public bool CanPrint
        {
            get
            {
                return false;
            }
        }

        public bool CanSave
        {
            get
            {
                return false;
            }
        }

        public string CurrentFile
        {
            get
            {
                return null;
            }
        }

        public string SamplesDir
        {
            get;
            set;
        }
        public bool ReadOnly
        {
            get;
            set;
        }
        public bool HasBeenClosed
        {
            get;
            set;
        }
        public PluginManager PluginManager
        {
            get;
            set;
        }

        public event SelectedPluginChangedHandler OnSelectedPluginChanged;
        public event ProjectTitleChangedHandler OnProjectTitleChanged;
        public event OpenProjectFileHandler OnOpenProjectFile;
        public event OpenTabHandler OnOpenTab;
        public event OpenEditorHandler OnOpenEditor;
        public event FileLoadedHandler OnFileLoaded;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                if (_executionEngine != null && _executionEngine.IsRunning())
                {
                    _executionEngine.Stop();
                }

            }
            catch (Exception)
            {
                //do nothing
            }
            try
            {
                CrypCloud.Core.CrypCloudCore.Instance.Logout();
            }
            catch (Exception)
            {
                //do nothing
            }
        }
    }
}
