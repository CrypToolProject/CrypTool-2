using CrypTool.CrypWin.Helper;
using System.Collections.Generic;
using System.Configuration;

namespace CrypTool.CrypWin.Properties
{
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase
    {
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [SettingsSerializeAs(SettingsSerializeAs.Binary)]
        public global::System.Collections.ArrayList DisabledPlugins
        {
            get => ((global::System.Collections.ArrayList)(this["DisabledPlugins"]));
            set => this["DisabledPlugins"] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [SettingsSerializeAs(SettingsSerializeAs.Binary)]
        public List<StoredTab> LastOpenedTabs
        {
            get => ((List<StoredTab>)(this["LastOpenedTabs"]));
            set => this["LastOpenedTabs"] = value;
        }
    }
}
