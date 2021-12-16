using CrypCloud.Manager.Services;
using CrypCloud.Manager.ViewModels.Helper;
using CrypTool.CertificateLibrary.Network;
using CrypTool.CertificateLibrary.Util;
using System.Security;

namespace CrypCloud.Manager.ViewModels
{
    public class CreateAccountVM : VerificationBaseVM
    {
        public SecureString PasswordConfirm { get; set; }
        public SecureString Password { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string VerificationCode { get; set; }


        public CreateAccountVM()
        {
            RequestCommand = new RelayCommand(it => CreateAccount());
            VerificationCommand = new RelayCommand(it => VerifyAccount());
        }

        private void CreateAccount()
        {
            if (!ValidateModel())
            {
                return;
            }

            CertificateRegistration request = new CertificateRegistration
            {
                Avatar = Username,
                Email = Email,
                Password = Password.ToUnsecuredString(),
                World = "CrypTool"
            };

            CAServerHelper.RegisterCertificate(request, ShowVerification, HandleProcessingError, ErrorHandler);

            ShowInputDialog = false;
            ShowWaitDialog = true;
        }

        private void VerifyAccount()
        {
            CAServerHelper.VerifyEmail(VerificationCode, ShowSuccessMessage, HandleProcessingError, ErrorHandler);

            ShowVerificationDialog = false;
            ShowWaitDialog = true;
        }

        private bool ValidateModel()
        {
            if (!Verification.IsValidAvatar(Username))
            {
                ShowMessageBox("Invalid Username");
                return false;
            }

            if (!Verification.IsValidEmailAddress(Email))
            {
                ShowMessageBox("Invalid Email.");
                return false;
            }

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

        private void HandleProcessingError(ProcessingErrorEventArgs error)
        {
            string errorMessage;
            switch (error.Type)
            {
                case ErrorType.AvatarAlreadyExists:
                    errorMessage = "The username already exists. Please chose another one.";
                    break;
                case ErrorType.EmailAlreadyExists:
                    errorMessage = "The email already exists. Please chose another ones.";
                    break;
                case ErrorType.AlreadyVerified:
                    errorMessage = "Email already verified.";
                    break;
                case ErrorType.WrongCode:
                    errorMessage = "Verification code is not correct.";
                    break;
                default:
                    errorMessage = "Invalid certificate request: " + error.Type;
                    break;
            }
            ErrorHandler(errorMessage);
        }
    }
}
