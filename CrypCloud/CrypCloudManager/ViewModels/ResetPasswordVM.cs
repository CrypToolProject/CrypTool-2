using System;
using System.Security;
using CrypCloud.Manager.Services;
using CrypCloud.Manager.ViewModels.Helper;
using CrypTool.CertificateLibrary.Network;
using CrypTool.CertificateLibrary.Util;

namespace CrypCloud.Manager.ViewModels
{
    public class ResetPasswordVM : VerificationBaseVM
    {
        public String Name { get; set; }
        public String Email { get; set; }

        public String VerificationCode { get; set; } 
        public SecureString Password { get; set; }
        public SecureString PasswordConfirm { get; set; } 

        public ResetPasswordVM()
        {
            RequestCommand = new RelayCommand(it => ResetPassword());
            VerificationCommand = new RelayCommand(it => VerifyReset()); 
        }

        private void ResetPassword()
        {
            var validName = Verification.IsValidAvatar(Name);
            var validEmail = Verification.IsValidEmailAddress(Email);
            if (!validName && !validEmail)
            {
                ErrorMessage = "Please enter ether a Name or an Email";
                return;
            }

            var request = new PasswordReset();
            if (validName)
                request.Avatar = Name;

            if (validEmail)
                request.Email = Email;

            CAServerHelper.ResetPassword(request, ShowVerification, HandleRequestProcessingError, ErrorHandler);

            ShowInputDialog = false;
            ShowWaitDialog = true;
        }

        private void VerifyReset()
        {
            if ( ! ValidatePassword()) 
                return;

            var request = new PasswordResetVerification(Password.ToUnsecuredString(), VerificationCode);
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
            if ( ! Verification.IsValidPassword(Password.ToUnsecuredString()))
            {
                ShowMessageBox("Invalid Password.");
                return false;
            }

            if ( ! Password.IsEqualTo(PasswordConfirm))
            {
                ShowMessageBox("Passwords are not equal");
                return false;
            }

            return true;
        }


        private void HandleRequestProcessingError(ProcessingErrorEventArgs error)
        {
            var errorMessage = "Invalid request: " + error.Type;

            if (error.Type.Equals(ErrorType.NoCertificateFound))
                errorMessage = "No such user found";

            ErrorHandler(errorMessage);
        }

        private void HandleVerificationProcessingError(ProcessingErrorEventArgs error)
        {
            var errorMessage = "Invalid request: " + error.Type;

            if (error.Type.Equals(ErrorType.NoCertificateFound))
                errorMessage = "Verification code is ether expired or wrong";

            ErrorHandler(errorMessage);
        }

  
    }
}
