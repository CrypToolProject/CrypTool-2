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
using CrypTool.PluginBase.Editor;
using System;
using System.Collections.Generic;

namespace CrypTool.CrypWin.Helper
{
    public class EditorTypePanelManager
    {
        public class EditorTypePanelProperties
        {
            private bool showLogPanel = false;
            private bool showSettingsPanel = false;
            private bool showComponentPanel = false;

            private bool saveShowLogPanel = false;
            private bool saveShowSettingsPanel = false;
            private bool saveShowComponentPanel = false;

            public EditorTypePanelProperties(bool CanShowLogPanel, bool CanShowSettingsPanel, bool CanShowComponentPanel)
            {
                this.CanShowLogPanel = CanShowLogPanel;
                this.CanShowSettingsPanel = CanShowSettingsPanel;
                this.CanShowComponentPanel = CanShowComponentPanel;
            }

            public bool ShowLogPanel { get => showLogPanel; set => showLogPanel = value & CanShowLogPanel; }
            public bool ShowSettingsPanel
            {
                get => showSettingsPanel;
                set => showSettingsPanel = value & CanShowSettingsPanel;
            }
            public bool ShowComponentPanel { get => showComponentPanel; set => showComponentPanel = value & CanShowComponentPanel; }

            public bool CanShowLogPanel { get; private set; }
            public bool CanShowSettingsPanel { get; private set; }
            public bool CanShowComponentPanel { get; private set; }

            public bool ShowMaximized { get; set; }

            public bool IsMaximized => !(ShowLogPanel || ShowComponentPanel);

            public void Maximize()
            {
                saveShowLogPanel = showLogPanel;
                saveShowSettingsPanel = showSettingsPanel;
                saveShowComponentPanel = showComponentPanel;

                ShowLogPanel = false;
                ShowSettingsPanel = false;
                ShowComponentPanel = false;
            }

            public void Minimize()
            {
                ShowLogPanel = saveShowLogPanel;
                ShowSettingsPanel = saveShowSettingsPanel;
                //ShowComponentPanel = saveShowComponentPanel;
                ShowComponentPanel = true;
                if (IsMaximized)
                {
                    showLogPanel = true;
                }
            }
        }

        private readonly Dictionary<Type, EditorTypePanelProperties> _editorTypeToPanelPropertiesMap = new Dictionary<Type, EditorTypePanelProperties>();

        public EditorTypePanelProperties GetEditorTypePanelProperties(Type editorType)
        {
            if (!_editorTypeToPanelPropertiesMap.ContainsKey(editorType))
            {
                EditorInfoAttribute editorSettings = editorType.GetEditorInfoAttribute();
                if (editorSettings == null)
                {
                    return null;
                }

                EditorTypePanelProperties prop = new EditorTypePanelProperties(editorSettings.CanShowLogPanel, editorSettings.CanShowSettingsPanel, editorSettings.CanShowComponentPanel)
                {
                    ShowLogPanel = editorSettings.ShowLogPanel,
                    ShowSettingsPanel = editorSettings.ShowSettingsPanel,
                    ShowComponentPanel = editorSettings.ShowComponentPanel
                };
                //{
                //    //CanShowComponentPanel = editorSettings.CanShowComponentPanel,
                //    //CanShowLogPanel = editorSettings.CanShowLogPanel,
                //    //CanShowSettingsPanel = editorSettings.CanShowSettingsPanel,
                //    ShowComponentPanel = editorSettings.ShowComponentPanel,
                //    ShowLogPanel = editorSettings.ShowLogPanel,
                //    ShowSettingsPanel = editorSettings.ShowSettingsPanel,
                //    ShowMaximized = false
                //};

                _editorTypeToPanelPropertiesMap.Add(editorType, prop);
            }

            return _editorTypeToPanelPropertiesMap[editorType];
        }

        public void SetEditorTypePanelProperties(Type editorType, EditorTypePanelProperties panelProperties)
        {
            if (!_editorTypeToPanelPropertiesMap.ContainsKey(editorType))
            {
                _editorTypeToPanelPropertiesMap.Add(editorType, panelProperties);
            }
            else
            {
                _editorTypeToPanelPropertiesMap[editorType] = panelProperties;
            }
        }
    }
}
