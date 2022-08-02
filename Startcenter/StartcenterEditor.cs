/*
   Copyright 2008-2022 CrypTool Team

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
using CrypTool.Core;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Attributes;
using CrypTool.PluginBase.Editor;
using Startcenter.Controls;
using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace Startcenter
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

        public ISettings Settings => null;

        private readonly StartcenterControl _startcenter = new StartcenterControl();
        public UserControl Presentation => _startcenter;

        public void Execute()
        {

        }

        public void Stop()
        {

        }

        public void Initialize()
        {
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
            OnFileLoaded?.Invoke(this, fileName);
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
            set => _samplesDir = value;
        }

        public bool ReadOnly { get; set; }

        public void AddText()
        {
            //do nothing
        }

        public void AddImage()
        {
            //do nothing
        }
    }
}
