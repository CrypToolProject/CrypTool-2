using CrypCloud.Manager.Services;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace CrypCloud.Manager.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public ScreenNavigator Navigator { get; set; }
        protected TaskFactory UiContext;
        #region viewProperties

        private bool isActive;
        public bool IsActive
        {
            get => isActive;
            set
            {
                isActive = value;
                if (value)
                {
                    HasBeenActivated();
                }
                RaisePropertyChanged("IsActive");
            }
        }


        private string errorMessage;
        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                errorMessage = value;
                RaisePropertyChanged("ErrorMessage");
            }
        }

        #endregion

        public BaseViewModel()
        {
            UiContext = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
        }

        protected virtual void HasBeenActivated()
        {
        }



        protected void RunInUiContext(Action action)
        {
            UiContext.StartNew(action);
        }

        #region messageBox

        protected static EventHandler<T> GetDelegate<T>(Action action) where T : EventArgs
        {
            return (delegate { action(); });
        }

        protected static EventHandler<T> GetShowMessageDelegate<T>(string msg) where T : EventArgs
        {
            return (delegate { ShowMessageBox(msg); });
        }

        protected static void ShowMessageBox(string msg, string title = "")
        {
            MessageBox.Show(msg, title);
        }

        #endregion

        #region INotifyPropertyChanged

        internal void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}