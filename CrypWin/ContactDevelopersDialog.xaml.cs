using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Windows;
using CrypTool.Core;
using CrypTool.PluginBase.Miscellaneous;

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

            var button = MessageBoxButton.YesNo;
            var icon = MessageBoxImage.Question;
            var res = MessageBox.Show(Properties.Resources.Are_you_sure_you_want_to_send_this_message_to_the_CrypToo_developers_, Properties.Resources.Are_you_sure_, button, icon);
            if (res == MessageBoxResult.Yes)
            {
                var sb = new StringBuilder();
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
            var pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            var hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);
            var sb = new StringBuilder();
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
            var contactDevelopersDialog = new ContactDevelopersDialog();
            contactDevelopersDialog.ShowDialog();
        }
    }
}
