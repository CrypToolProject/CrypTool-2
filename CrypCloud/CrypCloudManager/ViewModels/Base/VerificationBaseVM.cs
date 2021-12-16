
using CrypCloud.Manager.ViewModels.Helper;
using System;

namespace CrypCloud.Manager.ViewModels
{
    public class VerificationBaseVM : BaseViewModel
    {
        public RelayCommand RequestCommand { get; set; }
        public RelayCommand VerificationCommand { get; set; }
        public RelayCommand GoToVerification { get; set; }
        public RelayCommand BackCommand { get; set; }

        protected readonly Action<string> ErrorHandler;

        public VerificationBaseVM()
        {
            GoToVerification = new RelayCommand(it => ShowVerification());
            BackCommand = new RelayCommand(it => Navigator.ShowScreenWithPath(ScreenPaths.Login));

            ErrorHandler = msg => UiContext.StartNew(() => ShowErrorMessage(msg));
        }

        protected override void HasBeenActivated()
        {
            HideAll();
            ShowInputDialog = true;
        }

        #region dialog properties

        private bool successDialogVisible;
        private bool inputDialogVisible;
        private bool waitDialogVisible;
        private bool errorDialogVisible;
        private bool verificationDialogVisible;

        public bool ShowSuccessDialog
        {
            get => successDialogVisible;
            set
            {
                successDialogVisible = value;
                RaisePropertyChanged("ShowSuccessDialog");
            }
        }

        public bool ShowInputDialog
        {
            get => inputDialogVisible;
            set
            {
                inputDialogVisible = value;
                RaisePropertyChanged("ShowInputDialog");
            }
        }

        public bool ShowWaitDialog
        {
            get => waitDialogVisible;
            set
            {
                waitDialogVisible = value;
                RaisePropertyChanged("ShowWaitDialog");
            }
        }

        public bool ShowErrorDialog
        {
            get => errorDialogVisible;
            set
            {
                errorDialogVisible = value;
                RaisePropertyChanged("ShowErrorDialog");
            }
        }

        public bool ShowVerificationDialog
        {
            get => verificationDialogVisible;
            set
            {
                verificationDialogVisible = value;
                RaisePropertyChanged("ShowVerificationDialog");
            }
        }

        #endregion

        #region dialog transistions

        protected void ShowSuccessMessage()
        {
            HideAll();
            ShowSuccessDialog = true;
        }

        protected void ShowErrorMessage(string errorMesssage)
        {
            HideAll();
            ShowErrorDialog = true;
            ErrorMessage = errorMesssage;
        }

        protected void ShowVerification()
        {
            HideAll();
            ShowVerificationDialog = true;
        }

        protected void HideAll()
        {
            ShowSuccessDialog = false;
            ShowInputDialog = false;
            ShowWaitDialog = false;
            ShowErrorDialog = false;
            ShowVerificationDialog = false;
        }

        #endregion
    }
}