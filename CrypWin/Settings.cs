using System.Collections.Generic;
using System.Configuration;
using CrypTool.CrypWin.Helper;

namespace CrypTool.CrypWin.Properties
{
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase
    {
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [SettingsSerializeAs(SettingsSerializeAs.Binary)]
        public global::System.Collections.ArrayList DisabledPlugins
        {
            get
            {
                return ((global::System.Collections.ArrayList)(this["DisabledPlugins"]));
            }
            set
            {
                this["DisabledPlugins"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [SettingsSerializeAs(SettingsSerializeAs.Binary)]
        public List<StoredTab> LastOpenedTabs
        {
            get
            {
                return ((List<StoredTab>)(this["LastOpenedTabs"]));
            }
            set
            {
                this["LastOpenedTabs"] = value;
            }
        }
    }
}
