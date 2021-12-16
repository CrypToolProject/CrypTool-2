/*
   Copyright 2008 Timo Eckhardt, University of Siegen

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

using Microsoft.Win32;
using Primes.Properties;
using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Primes.Options
{
    /// <summary>
    /// Interaction logic for OptionLi.xaml
    /// </summary>
    public partial class OptionLi : UserControl, IOption
    {
        public const string usePari = "usePari";
        public const string useLi = "useLi";
        public const string gpexe = "gpexe";

        public OptionLi()
        {
            InitializeComponent();
            lblError.Visibility = Visibility.Visible;
        }

        private Settings m_Setting;
        public ApplicationSettingsBase Settings
        {
            set
            {
                m_Setting = value as Settings;
                rbUsePari.IsChecked = m_Setting.usePari;
                rbUseSimpson.IsChecked = m_Setting.useSimpson;
                if (!string.IsNullOrEmpty(m_Setting.gpexe))
                {
                    tbGpExe.Text = m_Setting.gpexe;
                }
                else
                {
                    string paripath = Registry.GetValue(@"HKEY_CURRENT_USER\Software\PARI", "", "") as string;
                    if (!string.IsNullOrEmpty(paripath))
                    {
                        tbGpExe.Text = paripath + @"\gp.exe";
                    }
                }
            }
        }

        #region IOption Members

        public bool Save()
        {
            bool result = true;

            if (rbUsePari.IsChecked.Value)
            {
                if (string.IsNullOrEmpty(tbGpExe.Text) || !File.Exists(tbGpExe.Text))
                {
                    lblError.Visibility = Visibility.Visible;
                    lblError.Text = "Bitte wählen geben Sie den Pfad zur Programm \"gp.exe\" an. Es befindet sich im Pari Installationsverzeichnis.";
                    result = false;
                }
            }

            if (result)
            {
                m_Setting.usePari = rbUsePari.IsChecked.Value;
                m_Setting.useSimpson = rbUseSimpson.IsChecked.Value;
                m_Setting.gpexe = tbGpExe.Text;
            }

            return result;
        }

        #endregion

        private void btnChooseGpexe_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "gp.exe|gp.exe",
                Multiselect = false
            };

            if (File.Exists(m_Setting.gpexe))
            {
                ofd.InitialDirectory = m_Setting.gpexe;
            }
            else
            {
                ofd.InitialDirectory = string.Empty;
            }

            if (ofd.ShowDialog() == true)
            {
                if (File.Exists(ofd.FileName))
                {
                    tbGpExe.Text = ofd.FileName;
                }
            }
        }

        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Pari_Lin);
            e.Handled = true;
            OnlineHelp.OnlineHelpAccess.Activate();
        }

        private void Label_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Simpson_Lin);
            e.Handled = true;
            OnlineHelp.OnlineHelpAccess.Activate();
        }
    }
}
