using System;
using System.ComponentModel;
using System.Windows.Controls;
using CrypTool.Core;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Attributes;
using CrypTool.PluginBase.Editor;

namespace StartCenter
{
    [TabColor("LightSkyBlue")]
    [EditorInfo("startcenter", false, true, false, true, false, false)]
    [Author("Sven Rech", "rech@CrypTool.org", "Universität Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("Startcenter.Properties.Resources", "PluginCaption", "PluginTooltip", null, "Startcenter/images/startcenter.png")]
    public class StartcenterEditor : IEditor
    {
        private string _samplesDir;

        public event PropertyChangedEventHandler PropertyChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public delegate void StartupBehaviourChangedHandler(bool showOnStartup);
        public static event StartupBehaviourChangedHandler StartupBehaviourChanged;

        public bool ShowOnStartup
        {
            set { _startcenter.StartupCheckbox.IsChecked = value; }
        }

        public ISettings Settings
        {
            get { return null; }
        }

        private readonly Startcenter.Startcenter _startcenter = new Startcenter.Startcenter();
        public UserControl Presentation
        {
            get { return _startcenter; }
        }

        public void Execute()
        {
            
        }

        public void Stop()
        {
            
        }

        public void Initialize()
        {
            _startcenter.StartupBehaviourChanged += (showOnStartup) => StartupBehaviourChanged(showOnStartup);
            _startcenter.OnOpenEditor += (content, info) => OnOpenEditor(content, info);
            _startcenter.OnOpenTab += (content, title, parent) => OnOpenTab(content, title, parent);
            _startcenter.TemplatesDir = _samplesDir;
            OnProjectTitleChanged(this, "Startcenter");
            Presentation.ToolTip = Startcenter.Properties.Resources.PluginTooltip;
        }

        public void Dispose()
        {
            
        }

        public bool HasBeenClosed { get; set; }
        public PluginManager PluginManager { get; set; }

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

        public void ShowSelectedEntityHelp()
        {
            _startcenter.ShowHelp();
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
            set { _samplesDir = value; }
        }

        public bool ReadOnly { get; set; }


        public void AddText()
        {
            throw new NotImplementedException();
        }

        public void AddImage()
        {
            throw new NotImplementedException();
        }
    }
}
