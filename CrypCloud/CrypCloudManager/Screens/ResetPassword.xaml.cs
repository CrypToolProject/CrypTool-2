using CrypCloud.Manager.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace CrypCloud.Manager.Screens
{

    [CrypTool.PluginBase.Attributes.Localization("CrypCloud.Manager.Properties.Resources")]
    public partial class ResetPassword : UserControl
    {
        public ResetPassword()
        {
            InitializeComponent();
        }


        //prevents memory leaking of the secure password input
        private void PasswordBoxChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                ((ResetPasswordVM)DataContext).Password = ((PasswordBox)sender).SecurePassword;
            }
        }

        //prevents memory leaking of the secure password input
        private void PasswordConfirmBoxChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                ((ResetPasswordVM)DataContext).PasswordConfirm = ((PasswordBox)sender).SecurePassword;
            }
        }
    }
}
