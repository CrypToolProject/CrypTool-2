using System;
using System.ComponentModel;
using System.Windows.Controls;
using CrypTool.Core;
using CrypTool.CrypTutorials.Properties;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Attributes;
using CrypTool.PluginBase.Editor;
using Vlc.DotNet.Core;

namespace CrypTool.CrypTutorials
{
    [TabColor("Black")]
    [EditorInfo("cryptutorials", true, true, false, true, false, false)]
    [Author("Nils Kopal", "kopal@cryptool.org", "Universität Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("CrypTool.CrypTutorials.Properties.Resources", "PluginCaption", "PluginTooltip", "CrypTutorials/DetailedDescription/doc.xml",
        "CrypTutorials/icon.png")]
    public class CrypTutorials : IEditor
    {
        private readonly CrypTutorialsPresentation _presentation;

        public CrypTutorials()
        {
            _presentation = new CrypTutorialsPresentation();
        }

        #region IEditor Members

        public event SelectedPluginChangedHandler OnSelectedPluginChanged;
        public event ProjectTitleChangedHandler OnProjectTitleChanged;
        public event OpenProjectFileHandler OnOpenProjectFile;
        public event OpenTabHandler OnOpenTab;
        public event OpenEditorHandler OnOpenEditor;
        public event FileLoadedHandler OnFileLoaded;

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

        public bool CanUndo
        {
            get { return false; }
        }

        public bool CanRedo
        {
            get { return false; }
        }

        public bool CanCut
        {
            get { return false; }
        }

        public bool CanCopy
        {
            get { return false; }
        }

        public bool CanPaste
        {
            get { return false; }
        }

        public bool CanRemove
        {
            get { return false; }
        }

        public bool CanExecute
        {
            get { return false; }
        }

        public bool CanStop
        {
            get { return false; }
        }

        public bool HasChanges
        {
            get { return false; }
        }

        public bool CanPrint
        {
            get { return false; }
        }

        public bool CanSave
        {
            get { return false; }
        }

        public string CurrentFile
        {
            get { return null; }
        }

        public string SamplesDir
        {
            set { }
        }

        public bool ReadOnly
        {
            get { return false; }
            set { }
        }

        public bool HasBeenClosed { get; set; }

        public PluginManager PluginManager
        {
            get { return null; }
            set { }
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public ISettings Settings
        {
            get { return null; }
        }

        public UserControl Presentation
        {
            get { return _presentation; }
        }

        public void Execute()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
            OnProjectTitleChanged(this, Resources.PluginCaption);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Dispose()
        {
            if (!VlcContext.IsInitialized)
                return;
            ((CrypTutorialsPresentation)Presentation).Player.Close();
        }


        public void ShowSelectedEntityHelp()
        {
        }

        #endregion

        public void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, logLevel));
            }
        }

        public void ShowSelectedPluginDescription()
        {
        }
    }
}