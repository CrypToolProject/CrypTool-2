using CrypCloud.Manager.Services;
using CrypCloud.Manager.ViewModels.Helper;
using CrypTool.CertificateLibrary.Network;
using CrypTool.CertificateLibrary.Util;
using System.Security;

namespace CrypCloud.Manager.ViewModels
{
    public class ResetPasswordVM : VerificationBaseVM
    {
        public string Name { get; set; }
        public string Email { get; set; }

        public string VerificationCode { get; set; }
        public SecureString Password { get; set; }
        public SecureString PasswordConfirm { get; set; }

        public ResetPasswordVM()
        {
            RequestCommand = new RelayCommand(it => ResetPassword());
            VerificationCommand = new RelayCommand(it => VerifyReset());
        }

        private void ResetPassword()
        {
            bool validName = Verification.IsValidAvatar(Name);
            bool validEmail = Verification.IsValidEmailAddress(Email);
            if (!validName && !validEmail)
            {
                ErrorMessage = "Please enter ether a Name or an Email";
                return;
            }

            PasswordReset request = new PasswordReset();
            if (validName)
            {
                request.Avatar = Name;
            }

            if (validEmail)
            {
                request.Email = Email;
            }

            CAServerHelper.ResetPassword(request, ShowVerification, HandleRequestProcessingError, ErrorHandler);

            ShowInputDialog = false;
            ShowWaitDialog = true;
        }

        private void VerifyReset()
        {
            if (!ValidatePassword())
            {
                return;
            }

            PasswordResetVerification request = new PasswordResetVerification(Password.ToUnsecuredString(), VerificationCode);
            CAServerHelper.VerifyPasswordReset(request, OnCertificateReceived, HandleVerificationProcessingError, ErrorHandler);

            ShowVerificationDialog = false;
            ShowWaitDialog = true;
        }

        private void OnCertificateReceived(CertificateReceivedEventArgs arg)
        {
            CertificateHelper.StoreCertificate(arg.Certificate, arg.Password, arg.Certificate.Avatar);
            ShowSuccessMessage();
        }

        private bool ValidatePassword()
        {
            if (!Verification.IsValidPassword(Password.ToUnsecuredString()))
            {
                ShowMessageBox("Invalid Password.");
                return false;
            }

            if (!Password.IsEqualTo(PasswordConfirm))
            {
                ShowMessageBox("Passwords are not equal");
                return false;
            }

            return true;
        }


        private void HandleRequestProcessingError(ProcessingErrorEventArgs error)
        {
            string errorMessage = "Invalid request: " + error.Type;

            if (error.Type.Equals(ErrorType.NoCertificateFound))
            {
                errorMessage = "No such user found";
            }

            ErrorHandler(errorMessage);
        }

        private void HandleVerificationProcessingError(ProcessingErrorEventArgs error)
        {
            string errorMessage = "Invalid request: " + error.Type;

            if (error.Type.Equals(ErrorType.NoCertificateFound))
            {
                errorMessage = "Verification code is ether expired or wrong";
            }

            ErrorHandler(errorMessage);
        }


    }
}
