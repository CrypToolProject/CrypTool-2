using System.Collections;
using System.Configuration;

namespace CrypTool.PluginBase.Properties
{
    public sealed partial class Settings
    {
        [UserScopedSetting()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [SettingsSerializeAs(SettingsSerializeAs.Binary)]
        public ArrayList Wizard_Storage
        {
            get => ((ArrayList)(this["Wizard_Storage"]));
            set => this["Wizard_Storage"] = value;
        }

        [UserScopedSetting()]
        public System.Windows.Media.FontFamily FontFamily
        {
            get => ((System.Windows.Media.FontFamily)(this["FontFamily"]));
            set => this["FontFamily"] = value;
        }

        [UserScopedSetting()]
        [global::System.Configuration.DefaultSettingValueAttribute("12")]
        public double FontSize
        {
            get => ((double)(this["FontSize"]));
            set => this["FontSize"] = value;
        }
    }
}
