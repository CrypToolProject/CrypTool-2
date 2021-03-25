using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CrypCloud.Manager.Services;
using System.Windows;

namespace CrypCloud.Manager.ViewModels
{
    public class ScreenViewModel : INotifyPropertyChanged
    {
        public ScreenNavigator Navigator { get; set; }
        protected TaskFactory UiContext;

        #region viewProperties

        private bool isActive;
        public bool IsActive
        {
            get { return isActive; }
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
            get { return errorMessage; }
            set 
            { 
                errorMessage = value;
                RaisePropertyChanged("ErrorMessage");
            }
        } 

        #endregion

        public ScreenViewModel()
        {
            UiContext = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
        }

        protected virtual void HasBeenActivated()
        {
        }


        protected Action RunInUiContext(Action action)
        {
            return () => UiContext.StartNew(action);
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