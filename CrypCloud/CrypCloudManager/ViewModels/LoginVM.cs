using CrypCloud.Core;
using CrypCloud.Manager.Services;
using CrypCloud.Manager.ViewModels.Helper;
using CrypTool.CertificateLibrary.Network;
using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace CrypCloud.Manager.ViewModels
{
    public class LoginVM : BaseViewModel
    {
        public List<string> AvailableCertificates { get; set; }
        public string Username { get; set; }
        public SecureString Password { get; set; }

        public RelayCommand LoginCommand { get; set; }
        public RelayCommand CreateNewAccountCommand { get; set; }
        public RelayCommand ResetPasswordCommand { get; set; }

        private bool rememberPassword = false;
        public bool RememberPassword
        {
            get => rememberPassword;
            set
            {
                rememberPassword = value;
                if (!value)
                {
                    Settings.Default.rememberedUsername = "";
                    Settings.Default.rememberedPassword = "";
                    Settings.Default.Save();
                }

                RaisePropertyChanged("RememberPassword");
            }
        }


        public LoginVM()
        {
            AvailableCertificates = new List<string>(CertificateHelper.GetNamesOfKnownCertificates());
            CreateNewAccountCommand = new RelayCommand(it => Navigator.ShowScreenWithPath(ScreenPaths.CreateAccount));
            ResetPasswordCommand = new RelayCommand(it => Navigator.ShowScreenWithPath(ScreenPaths.ResetPassword));
            LoginCommand = new RelayCommand(it => GetCertificateAndLogin());
        }

        protected override void HasBeenActivated()
        {
            string rememberedUsername = Settings.Default.rememberedUsername;
            RememberPassword = !string.IsNullOrEmpty(rememberedUsername);
            if (RememberPassword)
            {
                Username = rememberedUsername;
                Password = LoadPassword();
            }
            else
            {
                Username = "";
                Password = new SecureString().FromString("");
            }

            RaisePropertyChanged("Username");
            RaisePropertyChanged("Password");
        }

        #region remember me

        private static SecureString LoadPassword()
        {
            byte[] userData = Convert.FromBase64String(Settings.Default.rememberedPassword);
            byte[] unprotect = ProtectedData.Unprotect(userData, new byte[0], DataProtectionScope.CurrentUser);
            return new SecureString().FromString(Encoding.UTF8.GetString(unprotect));
        }
        private void RememberUserData()
        {
            Settings.Default.rememberedUsername = Username;
            byte[] bytes = Encoding.UTF8.GetBytes(Password.ToUnsecuredString());
            byte[] protectedPassword = ProtectedData.Protect(bytes, new byte[0], DataProtectionScope.CurrentUser);
            Settings.Default.rememberedPassword = Convert.ToBase64String(protectedPassword);
            Settings.Default.Save();
        }

        #endregion

        /// <summary>
        ///     Is called when the user clicks the login button
        /// </summary>
        private void GetCertificateAndLogin()
        {
            if (Username.ToLower().Equals("anonymous"))
            {
                //1. case: anonymous user
                LoadLocalCertificateAndLogin(true);
            }
            else if (CertificateHelper.UserCertificateIsUnknown(Username))
            {
                //2. case: we don't know the user, thus, we try to download a certificate
                LoadRemoteCertificateAndLogin();
            }
            else
            {
                //3. case: we know the user and open the local certificate
                LoadLocalCertificateAndLogin();
            }
        }

        #region local certificate

        private void LoadLocalCertificateAndLogin(bool anonymous = false)
        {
            X509Certificate2 certificate = null;
            if (anonymous)
            {
                certificate = CertificateHelper.LoadAnonymousCertificate(Password);
            }
            else
            {
                certificate = CertificateHelper.LoadPrivateCertificate(Username, Password);
            }
            if (certificate == null)
            {
                ErrorMessage = "Unable to open certificate of " + Username;
                return;
            }

            if (RememberPassword)
            {
                RememberUserData();
            }

            if (CrypCloudCore.Instance.IsBannedCertificate(certificate))
            {
                ErrorMessage = "Your Certificate has been banned";
                return;
            }

            if (CrypCloudCore.Instance.Login(certificate))
            {
                CrypCloudCore.Instance.RefreshJobList();
                Navigator.ShowScreenWithPath(ScreenPaths.JobList);
            }


            ErrorMessage = "";
        }

        #endregion

        #region remote certificate

        private void LoadRemoteCertificateAndLogin()
        {
            Action<string> errorAction = new Action<string>(msg => ErrorMessage = msg);
            CT2CertificateRequest request = new CT2CertificateRequest(Username, null, Password.ToUnsecuredString());

            CAServerHelper.RequestCertificate(request, OnCertificateReceived, HandleProcessingError, errorAction);
        }

        private void OnCertificateReceived(CertificateReceivedEventArgs arg)
        {
            CertificateHelper.StoreCertificate(arg.Certificate, arg.Password, arg.Certificate.Avatar);
            LoadLocalCertificateAndLogin();
        }

        private void HandleProcessingError(ProcessingErrorEventArgs arg)
        {
            if (arg.Type.Equals(ErrorType.CertificateNotYetAuthorized))
            {
                ErrorMessage = "Certificate has not been authorized. Please try again later";
            }
            else
            {
                ErrorMessage = "Unable to get Certificate for user: " + Username;
            }
        }

        #endregion
    }
}