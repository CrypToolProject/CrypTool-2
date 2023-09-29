/*
   Copyright 2019 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using CrypCloud.Core;
using CrypTool.PluginBase.Attributes;
using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
namespace CrypCloud.Manager
{
    /// <summary>
    /// </summary>
    [Localization("CrypCloud.Manager.Properties.Resources")]
    [SettingsTab("CrypCloudSettings", "/MainSettings/")]
    public partial class CrypCloudSettingsTab : UserControl
    {
        private readonly int minvalue = 1;
        private readonly int maxvalue = Environment.ProcessorCount;
        private readonly int startvalue;
        public CrypCloudSettingsTab(Style settingsStyle)
        {
            InitializeComponent();
            try
            {
                startvalue = Settings.Default.amountOfWorker;
                CrypCloudCore.Instance.AmountOfWorker = startvalue;
            }
            catch (Exception)
            {
                startvalue = 1;
                CrypCloudCore.Instance.AmountOfWorker = 1;
            }

            try
            {
                CrypCloudCore.Instance.LogLevel = Settings.Default.LogLevel;
                if (CrypCloudCore.Instance.LogLevel != null)
                {
                    for (int i = 0; i < LogLevelComboBox.Items.Count; i++)
                    {
                        if (((ComboBoxItem)LogLevelComboBox.Items[i]).Content.ToString().Equals(CrypCloudCore.Instance.LogLevel))
                        {
                            LogLevelComboBox.SelectedIndex = i;
                        }
                    }
                }
            }
            catch (Exception)
            {
                CrypCloudCore.Instance.LogLevel = Settings.Default.LogLevel;
            }

            Resources.Add("settingsStyle", settingsStyle);
            try
            {
                NUDTextBox.Text = startvalue.ToString();
                CrypCloudCore.Instance.WritePerformanceLog = Settings.Default.writePerformanceLog;
            }
            catch (Exception)
            {
                CrypCloudCore.Instance.WritePerformanceLog = false;
            }
        }

        private ObservableCollection<string> devicesAvailable = new ObservableCollection<string>();
        public ObservableCollection<string> DevicesAvailable
        {
            get => devicesAvailable;
            set
            {
                if (value != devicesAvailable)
                {
                    devicesAvailable = value;
                }
            }
        }    

        private void NUDButtonUP_Click(object sender, RoutedEventArgs e)
        {
            int number = NUDTextBox.Text != "" ? Convert.ToInt32(NUDTextBox.Text) : 0;
            if (number < maxvalue)
            {
                NUDTextBox.Text = Convert.ToString(number + 1);
            }
        }

        private void NUDButtonDown_Click(object sender, RoutedEventArgs e)
        {
            int number;
            if (NUDTextBox.Text != "")
            {
                number = Convert.ToInt32(NUDTextBox.Text);
            }
            else
            {
                number = 0;
            }

            if (number > minvalue)
            {
                NUDTextBox.Text = Convert.ToString(number - 1);
            }
        }

        private void NUDTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Up)
            {
                NUDButtonUP.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonUP, new object[] { true });
            }


            if (e.Key == Key.Down)
            {
                NUDButtonDown.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonDown, new object[] { true });
            }
        }

        private void NUDTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonUP, new object[] { false });
            }

            if (e.Key == Key.Down)
            {
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonDown, new object[] { false });
            }
        }

        private void NUDTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int number = 0;
            if (NUDTextBox.Text != "")
            {
                if (!int.TryParse(NUDTextBox.Text, out number))
                {
                    NUDTextBox.Text = startvalue.ToString();
                }
            }

            if (number > maxvalue)
            {
                NUDTextBox.Text = maxvalue.ToString();
            }

            if (number < minvalue)
            {
                NUDTextBox.Text = minvalue.ToString();
            }

            NUDTextBox.SelectionStart = NUDTextBox.Text.Length;

            Settings.Default.amountOfWorker = number;
            Settings.Default.Save();
            CrypCloudCore.Instance.AmountOfWorker = number;
        }

        private void LogLevelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LogLevelComboBox.SelectedItem == null)
            {
                return;
            }
            Settings.Default.LogLevel = ((ComboBoxItem)LogLevelComboBox.SelectedItem).Content.ToString();
            Settings.Default.Save();
            CrypCloudCore.Instance.LogLevel = Settings.Default.LogLevel;
        }
    }
}
