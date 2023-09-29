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
using CrypTool.Core;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Windows;

namespace CrypTool.CrypWin
{
    /// <summary>
    /// Interaction logic for ContactDevelopersDialog.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.CrypWin.Properties.Resources")]
    public partial class ContactDevelopersDialog : Window
    {
        public ContactDevelopersDialog()
        {
            InitializeComponent();
            Subject.Focus();
        }

        private void SendButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (string.IsNullOrWhiteSpace(Message.Text) || string.IsNullOrWhiteSpace(Subject.Text))
            {
                MessageBox.Show(Properties.Resources.Please_fill_in_all_required_fields, Properties.Resources.Input_failure, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Question;
            MessageBoxResult res = MessageBox.Show(Properties.Resources.Are_you_sure_you_want_to_send_this_message_to_the_CrypToo_developers_, Properties.Resources.Are_you_sure_, button, icon);
            if (res == MessageBoxResult.Yes)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("A user sent a contact message.");
                sb.AppendLine(string.Format("Subject: {0}", Subject.Text));
                sb.AppendLine(string.Format("E-Mail: {0}", EMail.Text));
                sb.AppendLine("Message:");
                sb.AppendLine(Message.Text);
                sb.AppendLine("");
                sb.AppendLine("-");
                sb.AppendLine("System infos:");
                sb.AppendLine(GetSystemInfos());

                try
                {
                    Mailer.SendMailToCoreDevs(Mailer.ACTION_DEVMAIL, string.Format("User contact message: {0}", Subject.Text), sb.ToString(), EMail.Text);
                    MessageBox.Show(Properties.Resources.Your_message_has_been_delivered, Properties.Resources.Sending_done);
                    Close();
                }
                catch (Exception e)
                {
                    MessageBox.Show(string.Format(Properties.Resources.Error_trying_to_send_the_message, e.Message), Properties.Resources.Sending_failed);
                }
            }
        }

        private void CancelButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            Close();
        }

        private string GetSystemInfos()
        {
            WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            bool hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("Operating System: {0}", System.Environment.OSVersion.ToString()));
            sb.AppendLine(string.Format("Plattform: {0}", Environment.OSVersion.Platform));
            sb.AppendLine(string.Format("Processors: {0}", System.Environment.ProcessorCount));
            sb.AppendLine(string.Format("Administrative Rights: {0}", hasAdministrativeRight));
            sb.AppendLine(string.Format("Current culture: {0}", CultureInfo.CurrentUICulture.Name));
            sb.AppendLine(string.Format("CrypTool version: {0}", AssemblyHelper.Version));
            sb.AppendLine(string.Format("Build type: {0}", AssemblyHelper.BuildType));
            sb.AppendLine(string.Format("Build time: {0}", File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location)));
            sb.AppendLine(string.Format("Product name: {0}", AssemblyHelper.ProductName));
            sb.AppendLine(string.Format("Common language runtime version: {0}", Environment.Version));
            return sb.ToString();
        }

        public static void ShowModalDialog()
        {
            ContactDevelopersDialog contactDevelopersDialog = new ContactDevelopersDialog();
            contactDevelopersDialog.ShowDialog();
        }
    }
}
