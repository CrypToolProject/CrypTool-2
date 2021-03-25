using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CrypTool.CrypWin.Properties;
using CrypTool.PluginBase.Attributes;
using CrypTool.PluginBase.Miscellaneous;

namespace CrypTool.CrypWin.SettingsTabs
{
    /// <summary>
    /// Interaction logic for PluginsSettingsTab.xaml
    /// </summary>
    [Localization("CrypTool.CrypWin.SettingsTabs.Resources.res")]
    [SettingsTab("PluginSettings", "/", 0.5)]
    public partial class PluginsSettingsTab : UserControl
    {
        public static readonly DependencyProperty IsVisibleProperty = 
            DependencyProperty.Register("IsVisible", typeof(Boolean), typeof(PluginsSettingsTab), new PropertyMetadata(false));

        public Boolean IsVisible
        {
            get { return (Boolean)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        public PluginsSettingsTab(Style settingsStyle)
        {
            Resources.Add("settingsStyle", settingsStyle);
            InitializeComponent();
            if (AssemblyHelper.BuildType == Ct2BuildType.Developer)
            {
                IsVisible = true;
            }

            PluginListBox.DataContext = PluginList.AllPlugins;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var assemblyToPluginMap = new Dictionary<string, List<PluginInformation>>();
            foreach (var plugin in PluginList.AllPlugins)
            {
                if (assemblyToPluginMap.ContainsKey(plugin.Assemblyname))
                    assemblyToPluginMap[plugin.Assemblyname].Add(plugin);
                else
                    assemblyToPluginMap.Add(plugin.Assemblyname, new List<PluginInformation>() {plugin});
            }

            Settings.Default.DisabledPlugins = new ArrayList();
            foreach (var plugin in PluginList.AllPlugins)
            {
                if (plugin.Disabled)
                {
                    bool canBeDisabled = assemblyToPluginMap[plugin.Assemblyname].All(brotherPlugin => brotherPlugin.Disabled);
                    if (canBeDisabled)
                        Settings.Default.DisabledPlugins.Add(plugin);
                }
            }

            Settings.Default.Save();
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var plugin in PluginList.AllPlugins)
            {
                plugin.Disabled = true;
            }
            PluginListBox.Items.Refresh();
            CheckBox_Checked(null, null);
        }

        private void DeselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var plugin in PluginList.AllPlugins)
            {
                plugin.Disabled = false;
            }
            PluginListBox.Items.Refresh();
            CheckBox_Checked(null, null);
        }

        private void Invert_Click(object sender, RoutedEventArgs e)
        {
            foreach (var plugin in PluginList.AllPlugins)
            {
                plugin.Disabled = !plugin.Disabled;
            }
            PluginListBox.Items.Refresh();
            CheckBox_Checked(null, null);
        }

        private void ContextMenuHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem menu = (MenuItem)((RoutedEventArgs)e).Source;
                PluginInformation entry = (PluginInformation)menu.CommandParameter;
                if (entry == null) return;
                string tag = (string)menu.Tag;

                if (tag == "copy_line")
                {
                    string s = entry.Pluginname + "\t" + entry.Plugindescription + "\t" + entry.Assemblyname;
                    Clipboard.SetText(s);
                }
                else if (tag == "copy_all")
                {
                    List<string> lines = new List<string>();
                    foreach (PluginInformation x in PluginListBox.Items) lines.Add(x.Pluginname + "\t" + x.Plugindescription + "\t" + x.Assemblyname);
                    Clipboard.SetText(String.Join("\n", lines));
                }
            }
            catch (Exception ex)
            {
                Clipboard.SetText("");
            }
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class TrueToVisibleConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
                              CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
                throw new InvalidOperationException("The target must be of Visibility");

            if ((bool)value)
            {
                return Visibility.Visible;
            }

            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
