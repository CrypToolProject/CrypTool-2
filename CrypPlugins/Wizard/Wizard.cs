using System;
using System.ComponentModel;
using System.Xml.Linq;
using System.Windows.Controls;
using CrypTool.Core;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Attributes;
using CrypTool.PluginBase.Editor;

namespace Wizard
{
    [TabColor("royalblue")]
    [EditorInfo("wizard", true, false, false, true, false, false)]
    [Author("Simone Sauer", "sauer@CrypTool.org", "Universität Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("Wizard.Properties.Resources", "PluginCaption", "PluginTooltip", "Wizard/DetailedDescription/doc.xml", "Wizard/wizard.png")]
    public class Wizard : IEditor
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event SelectedPluginChangedHandler OnSelectedPluginChanged;
        public event ProjectTitleChangedHandler OnProjectTitleChanged;
        public event OpenProjectFileHandler OnOpenProjectFile;
        public event OpenTabHandler OnOpenTab;
        public event OpenEditorHandler OnOpenEditor;
        public event FileLoadedHandler OnFileLoaded;

        public Wizard()
        {
            wizardControl.OnOpenEditor += (editor, info) => OnOpenEditor(editor, info);
            wizardControl.OnOpenTab += (content, title, parent) => OnOpenTab(content, title, parent);
            wizardControl.OnGuiLogNotificationOccured += (sender, args) => OnGuiLogNotificationOccured(this, new GuiLogEventArgs(args.Message, this, args.NotificationLevel));
            Presentation.ToolTip = Properties.Resources.PluginTooltip;
        }

        public ISettings Settings
        {
            get { return null; }
        }

        private WizardControl wizardControl = new WizardControl();
        public UserControl Presentation
        {
            get { return wizardControl; }
        }

        public XElement WizardConfigXML()
        {
            return ((WizardControl)Presentation).WizardConfigXML;
        }

        public void Initialize()
        {
            wizardControl.Initialize();
        }

        public void Dispose()
        {
            wizardControl.StopCurrentWorkspaceManager();
        }

        public void ShowSelectedPluginDescription()
        {
        }

        public void Execute()
        {
            wizardControl.ExecuteCurrentWorkspaceManager();
        }

        public void Stop()
        {
            wizardControl.StopCurrentWorkspaceManager();
        }

        public bool CanExecute
        {
            get { return wizardControl.WizardCanExecute(); }
        }

        public bool CanStop
        {
            get { return wizardControl.WizardCanStop(); }
        }

        #region unused methods

        public void New()
        {
            
        }

        public bool HasBeenClosed { get; set; }

        public PluginManager PluginManager
        { get; set; }

        public void Open(string fileName)
        {
            if (OnFileLoaded != null)
            {
                OnFileLoaded(this, fileName);
            }
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
            set { wizardControl.SamplesDir = value; }
        }

        public bool ReadOnly
        {
            get { return false; }
            set {  }
        }

        #endregion

        public void AddText()
        {
            throw new NotImplementedException();
        }

        public void AddImage()
        {
            throw new NotImplementedException();
        }


        public void ShowSelectedEntityHelp()
        {
            throw new NotImplementedException();
        }
    }
}
