using CrypCloud.Manager.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace CrypCloud.Manager.Screens
{

    [CrypTool.PluginBase.Attributes.Localization("CrypCloud.Manager.Properties.Resources")]
    public partial class Login : UserControl

    {
        public Login()
        {
            InitializeComponent();

            string rememberedUsername = Settings.Default.rememberedUsername;
            if (!string.IsNullOrEmpty(rememberedUsername))
            {
                SecredPasswordInput.Password = "**********";
            }
            else
            {
                ClearPasswordBox();
            }

            if (DataContext is LoginVM)
            {
                LoginVM loginVM = DataContext as LoginVM;
                loginVM.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName.Equals("Password"))
                    {
                        if (loginVM.Password.Length == 0)
                        {
                            SecredPasswordInput.Clear();
                        }
                    }
                };
            }
        }

        //prevents memory leaking of the sequre password imput
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null && !SecredPasswordInput.Password.Equals("**********"))
            {
                ((LoginVM)DataContext).Password = ((PasswordBox)sender).SecurePassword;
            }
        }

        public void ClearPasswordBox()
        {
            SecredPasswordInput.Clear();
        }
    }
}
