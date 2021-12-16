/*
   Copyright 2011 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using CrypCloud.Manager.Services;
using CrypCloud.Manager.ViewModels;
using CrypTool.Core;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Attributes;
using CrypTool.PluginBase.Editor;
using System;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Controls;
using VoluntLib2.Tools;
using WorkspaceManager.Model;

namespace CrypCloud.Manager
{
    public enum ScreenPaths { Login, JobList, JobCreation, ResetPassword, CreateAccount, ConfirmAccount };

    [TabColor("white")]
    [EditorInfo("CrypCloud", false, true, false, true, false, false)]
    [Author("Christopher Konze", "c.konze@uni.de", "Universität Kassel", "")]
    [PluginInfo("CrypCloud.Manager.Properties.Resources", "PluginCaption", "PluginTooltip", "CrypCloudManager/doc.xml", "CrypCloudManager/images/icon.png")]
    public class CrypCloudManager : IEditor
    {
        private readonly ScreenNavigator screenNavigator = new ScreenNavigator();
        public const string DefaultTabName = "CrypCloud";

        public CrypCloudManager()
        {
            CrypCloudPresentation crypCloudPresentation = new CrypCloudPresentation();
            Presentation = crypCloudPresentation;
            AddScreensToNavigator(crypCloudPresentation);

            JobListVM jobListVM = (JobListVM)crypCloudPresentation.JobList.DataContext;
            jobListVM.Manager = this;
            Logger logger = Logger.GetLogger();
            logger.LoggOccured += logger_Logged;
        }

        private void logger_Logged(object sender, LogEventArgs logEventArgs)
        {
            switch (logEventArgs.Logtype)
            {
                case Logtype.Debug:
                    GuiLogMessage(logEventArgs.Message, NotificationLevel.Debug);
                    break;
                case Logtype.Info:
                    GuiLogMessage(logEventArgs.Message, NotificationLevel.Info);
                    break;
                case Logtype.Warning:
                    GuiLogMessage(logEventArgs.Message, NotificationLevel.Warning);
                    break;
                case Logtype.Error:
                    GuiLogMessage(logEventArgs.Message, NotificationLevel.Error);
                    break;
            }
        }

        private void AddScreensToNavigator(CrypCloudPresentation crypCloudPresentation)
        {
            //viewmodels are created in the xaml file due to autocomplete and typesafty reasons
            BaseViewModel loginVm = (BaseViewModel)crypCloudPresentation.Login.DataContext;
            screenNavigator.AddScreenWithPath(loginVm, ScreenPaths.Login);

            BaseViewModel jobListVm = (BaseViewModel)crypCloudPresentation.JobList.DataContext;
            screenNavigator.AddScreenWithPath(jobListVm, ScreenPaths.JobList);

            BaseViewModel jobCreateVm = (BaseViewModel)crypCloudPresentation.JobCreation.DataContext;
            screenNavigator.AddScreenWithPath(jobCreateVm, ScreenPaths.JobCreation);

            BaseViewModel createAccountVm = (BaseViewModel)crypCloudPresentation.CreateAccount.DataContext;
            screenNavigator.AddScreenWithPath(createAccountVm, ScreenPaths.CreateAccount);

            BaseViewModel resetPasswordVm = (BaseViewModel)crypCloudPresentation.ResetPassword.DataContext;
            screenNavigator.AddScreenWithPath(resetPasswordVm, ScreenPaths.ResetPassword);
        }

        public void OpenWorkspaceInNewTab(WorkspaceModel model, BigInteger jobId, string tabTitle = null)
        {
            if (OnOpenEditor == null)
            {
                return; // cant open tab 
            }

            TabInfo tabInfo = new TabInfo()
            {
                Title = tabTitle != null ? tabTitle : DefaultTabName
            };

            IEditor currentManager = OnOpenEditor(typeof(WorkspaceManager.WorkspaceManagerClass), tabInfo);
            ((WorkspaceManager.WorkspaceManagerClass)currentManager).Open(model);
        }

        public void New()
        {
            if (!CertificateHelper.DoesDirectoryExists())
            {
                CertificateHelper.CreateDirectory();
            }

            screenNavigator.ShowScreenWithPath(CrypCloudCore.Instance.IsRunning ? ScreenPaths.JobList
                                                                                : ScreenPaths.Login);
        }

        public void GuiLogMessage(string message, NotificationLevel notificationLevel)
        {
            if (OnGuiLogNotificationOccured == null)
            {
                return;
            }

            GuiLogEventArgs guiLogEvent = new GuiLogEventArgs(message, this, notificationLevel) { Title = "-" };
            OnGuiLogNotificationOccured(this, guiLogEvent);
        }

        public void Open(string fileName)
        {
            GuiLogMessage("CryptCloudManager: OpenFileDialog(" + fileName + ")", NotificationLevel.Debug);
            if (OnFileLoaded != null)
            {
                OnFileLoaded(this, fileName);
            }
        }

        public void SendOpenProjectFileEvent(string filename)
        {
            if (OnOpenProjectFile != null)
            {
                OnOpenProjectFile(this, filename);
            }
        }

        #region not utilized IEditor Members

        public event SelectedPluginChangedHandler OnSelectedPluginChanged;

        public event ProjectTitleChangedHandler OnProjectTitleChanged;

        public event OpenProjectFileHandler OnOpenProjectFile;

        public event FileLoadedHandler OnFileLoaded;


        public event OpenTabHandler OnOpenTab;
        public event OpenEditorHandler OnOpenEditor;

        public void AddText()
        {
            throw new NotImplementedException();
        }

        public void AddImage()
        {
            throw new NotImplementedException();
        }

        public void Save(string fileName)
        {
            GuiLogMessage("CryptCloudManager: Save(" + fileName + ")", NotificationLevel.Debug);
        }

        public void Add(Type type)
        {
            GuiLogMessage("CryptCloudManager: Add(" + type + ")", NotificationLevel.Debug);
        }

        public void Undo()
        {
            GuiLogMessage("CryptCloudManager: Undo()", NotificationLevel.Debug);
        }

        public void Redo()
        {
            GuiLogMessage("CryptCloudManager: Redo()", NotificationLevel.Debug);
        }

        public void Cut()
        {
            throw new NotImplementedException();
        }

        public void Copy()
        {
            throw new NotImplementedException();
        }

        public void Paste()
        {
            throw new NotImplementedException();
        }

        public void Remove()
        {
            throw new NotImplementedException();
        }

        public void Print()
        {
            throw new NotImplementedException();
        }

        public void ShowSelectedEntityHelp()
        {
        }

        public bool CanUndo => false;

        public bool CanRedo => false;

        public bool CanCut => false;

        public bool CanCopy => false;

        public bool CanPaste => false;

        public bool CanRemove => false;

        public bool CanExecute => false;

        public bool CanStop => false;

        public bool HasChanges => false;

        public bool CanPrint => false;

        public bool CanSave => false;

        public string CurrentFile => null;

        public string SamplesDir
        {
            set { }
        }

        public bool ReadOnly { get; set; }
        public bool HasBeenClosed { get; set; }

        public PluginManager PluginManager
        {
            get => null;
            set { }
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public ISettings Settings { get; private set; }

        public UserControl Presentation { get; private set; }

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
            Logger logger = Logger.GetLogger();
            logger.LoggOccured -= logger_Logged;
        }

        public event PropertyChangedEventHandler PropertyChanged;


        #endregion

    }

}
