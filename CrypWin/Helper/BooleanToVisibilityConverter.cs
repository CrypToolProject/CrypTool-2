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
using DevComponents.WpfDock;
using System;
using System.Windows;
using System.Windows.Data;

namespace CrypTool.CrypWin.Helper
{
    internal class BooleanToVisibilityConverter : IMultiValueConverter
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
                    {
                        ((DockWindow)((DockWindowGroup)split.Children[0]).Items[0]).Open();
                    }
                }
                else
                {
                    v = Visibility.Collapsed;
                    if (split.Children.Count > 0)
                    {
                        ((DockWindow)((DockWindowGroup)split.Children[0]).Items[0]).Close();
                    }
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
