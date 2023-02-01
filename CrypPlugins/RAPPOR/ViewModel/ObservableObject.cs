using System;
using System.ComponentModel;

namespace RAPPOR
{
    /// <summary>
    /// This class is used for all view models to inherit from.
    /// </summary>
    public class ObservableObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Property Changed event to act whenver a property has been changed in the view.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Method which will inform the viewmodel whenever a property in te view is changed.
        /// </summary>
        /// <param name="name">Name of the property which has been changed.</param>
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }


    }
}
