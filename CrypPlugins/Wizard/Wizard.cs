using CrypTool.Core;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Attributes;
using CrypTool.PluginBase.Editor;
using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Xml.Linq;

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

        public ISettings Settings => null;

        private readonly WizardControl wizardControl = new WizardControl();
        public UserControl Presentation => wizardControl;

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

        public bool CanExecute => wizardControl.WizardCanExecute();

        public bool CanStop => wizardControl.WizardCanStop();

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

        public bool CanUndo => false;

        public bool CanRedo => false;

        public bool CanCut => false;

        public bool CanCopy => false;

        public bool CanPaste => false;

        public bool CanRemove => false;

        public bool HasChanges => false;

        public bool CanPrint => false;

        public bool CanSave => false;

        public string CurrentFile => null;

        public string SamplesDir
        {
            set => wizardControl.SamplesDir = value;
        }

        public bool ReadOnly
        {
            get => false;
            set { }
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
