using System;
using System.Windows.Data;
using System.Windows;
using DevComponents.WpfDock;

namespace CrypTool.CrypWin.Helper
{
    class BooleanToVisibilityConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool b = (bool)values[0], bb = (bool)values[1];
            DockWindow dock = (DockWindow)values[2];
            SplitPanel split = (SplitPanel)values[3];

            string s = (string)parameter;
            if (b)
            {
                Visibility v;
                if (bb)
                {
                    v = Visibility.Visible;
                    if (split.Children.Count > 0)
                        ((DockWindow)((DockWindowGroup)split.Children[0]).Items[0]).Open();
                }
                else
                {
                    v = Visibility.Collapsed;
                    if (split.Children.Count > 0)
                        ((DockWindow)((DockWindowGroup)split.Children[0]).Items[0]).Close();
                }

                switch (s.ToLower())
                {
                    case "log":
                        Properties.Settings.Default.LogVisibility = v.ToString();
                        break;
                    case "plugin":
                        Properties.Settings.Default.PluginVisibility = v.ToString();
                        break;
                    case "setting":
                        Properties.Settings.Default.SettingVisibility = v.ToString();
                        break;
                }
                dock.IsAutoHide = false;
                CrypTool.PluginBase.Miscellaneous.ApplicationSettingsHelper.SaveApplicationsSettings();
                return v;
            }
            else
            {
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
