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
using CrypTool.Core;
using System;

namespace CrypTool.PluginBase.Editor
{
    public interface IEditor : IPlugin
    {
        event SelectedPluginChangedHandler OnSelectedPluginChanged;
        event ProjectTitleChangedHandler OnProjectTitleChanged;
        event OpenProjectFileHandler OnOpenProjectFile;
        event OpenTabHandler OnOpenTab;
        event OpenEditorHandler OnOpenEditor;
        event FileLoadedHandler OnFileLoaded;

        void New();
        void Open(string fileName);
        void Save(string fileName);

        void Add(Type type);
        void Undo();
        void Redo();
        void Cut();
        void Copy();
        void Paste();
        void Remove();
        void Print();
        void AddText();
        void AddImage();

        /// <summary>
        /// Used to display a plugin specific description button in settings pane. 
        /// </summary>
        void ShowSelectedEntityHelp();

        bool CanUndo { get; }
        bool CanRedo { get; }
        bool CanCut { get; }
        bool CanCopy { get; }
        bool CanPaste { get; }
        bool CanRemove { get; }
        bool CanExecute { get; }
        bool CanStop { get; }
        bool HasChanges { get; }
        bool CanPrint { get; }
        bool CanSave { get; }

        string CurrentFile { get; }

        string SamplesDir { set; }

        /// <summary>
        /// Gets or sets the readOnly propability of an editor i.e. if something on the workspace can be changed.
        /// </summary>
        bool ReadOnly { get; set; }

        bool HasBeenClosed { get; set; }

        PluginManager PluginManager { get; set; }
    }
}
