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
using System;

namespace CrypTool.PluginBase.Editor
{
    /// <summary>
    /// The default file-extension for the editor used by CrypWin to display 
    /// Open/Save FileDialog with correct filter.
    /// </summary>
    public class EditorInfoAttribute : Attribute
    {
        public bool Singleton;
        public bool ShowAsNewButton;
        public string DefaultExtension;
        public bool CanEdit;

        public bool CanShowLogPanel, CanShowSettingsPanel, CanShowComponentPanel;
        public bool ShowLogPanel, ShowSettingsPanel, ShowComponentPanel;

        // wander 2011-12-13: showSettingsPanel defaults to false in favor of WorkspaceManager parameter panel
        public EditorInfoAttribute(
            string defaultExtension, bool showAsNewButton = true, bool singleton = false, bool canEdit = false,
            bool canShowLogPanel = true, bool canShowSettingsPanel = false, bool canShowComponentPanel = false,
            bool showLogPanel = false, bool showSettingsPanel = false, bool showComponentPanel = false
            )
        {
            Singleton = singleton;
            ShowAsNewButton = showAsNewButton;
            DefaultExtension = defaultExtension;

            CanShowLogPanel = canShowLogPanel;
            CanShowSettingsPanel = canShowSettingsPanel;
            CanShowComponentPanel = canShowComponentPanel;

            ShowLogPanel = showLogPanel & canShowLogPanel;
            ShowSettingsPanel = showSettingsPanel & canShowSettingsPanel;
            ShowComponentPanel = showComponentPanel & canShowComponentPanel;

            CanEdit = canEdit;
        }
    }
}
