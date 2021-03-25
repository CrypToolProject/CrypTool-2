using System;
using System.Collections.Generic;
using CrypTool.PluginBase.Editor;

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

            public bool ShowLogPanel { get { return showLogPanel; } set { showLogPanel = value & CanShowLogPanel; } }
            public bool ShowSettingsPanel { get { return showSettingsPanel; } 
                set { 
                    showSettingsPanel = value & CanShowSettingsPanel;
                } 
            }
            public bool ShowComponentPanel { get { return showComponentPanel; } set { showComponentPanel = value & CanShowComponentPanel; } }

            public bool CanShowLogPanel { get; private set; }
            public bool CanShowSettingsPanel { get; private set; }
            public bool CanShowComponentPanel { get; private set; }

            public bool ShowMaximized { get; set; }

            public bool IsMaximized
            {
                get
                {
                    return !(ShowLogPanel || ShowComponentPanel);
                }
            }

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
                if (IsMaximized) showLogPanel = true;
            }
        }

        private Dictionary<Type, EditorTypePanelProperties> _editorTypeToPanelPropertiesMap = new Dictionary<Type, EditorTypePanelProperties>();

        public EditorTypePanelProperties GetEditorTypePanelProperties(Type editorType)
        {
            if (!_editorTypeToPanelPropertiesMap.ContainsKey(editorType))
            {
                var editorSettings = editorType.GetEditorInfoAttribute();
                if (editorSettings == null) return null;

                EditorTypePanelProperties prop = new EditorTypePanelProperties(editorSettings.CanShowLogPanel, editorSettings.CanShowSettingsPanel, editorSettings.CanShowComponentPanel);
                prop.ShowLogPanel = editorSettings.ShowLogPanel;
                prop.ShowSettingsPanel = editorSettings.ShowSettingsPanel;
                prop.ShowComponentPanel = editorSettings.ShowComponentPanel;
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
