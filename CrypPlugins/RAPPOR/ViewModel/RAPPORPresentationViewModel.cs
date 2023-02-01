using RAPPOR;
using RAPPOR.ViewModel;
using System;

namespace CrypTool.Plugins.RAPPOR.ViewModel
{
    /// <summary>
    /// This class represents the instance of the rappor presentation view model.
    /// It is used for calculating the ui logic of the rappor presentation view.
    /// </summary>
    public class RAPPORPresentationViewModel : ObservableObject
    {
        /// <summary>
        /// An array of all views which are placed in the content type.
        /// </summary>
        public IViewModelBase[] viewArray;

        /// <summary>
        /// Instance of the RAPPOR class.
        /// </summary>
        public RAPPOR rappor;
        /// <summary>
        /// Instance of the overview View model.
        /// </summary>
        private OverviewViewModel overviewViewModel;

        private int selectedViewInteger;

        /// <summary>
        /// Initializes the RAPPOR presentation view model.
        /// </summary>
        /// <param name="rAPPOR">Instance of the RAPPOR class</param>
        public RAPPORPresentationViewModel(RAPPOR rAPPOR)
        {
            rappor = rAPPOR;
            setUp();
            selectedViewInteger = 0;

        }
        /// <summary>
        /// Initializes all views which are placed in the content type.
        /// </summary>
        private void setUp()
        {
            viewArray = new IViewModelBase[5];
            viewArray[0] = new StartViewModel();
            viewArray[1] = overviewViewModel = new OverviewViewModel(rappor);
            viewArray[2] = new BloomFilterViewModel(rappor);
            viewArray[3] = new RandomizedResponsesViewModel(rappor);
            viewArray[4] = new HeatMapsViewModel(rappor);
        }
        /// <summary>
        /// Binding property of the name.
        /// </summary>
        private string _name;
        /// <summary>
        /// Getter and setter for the binding string property
        /// </summary>
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                {
                    return "nameMissing";
                }

                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }
        /// <summary>
        /// Getter for the view array.
        /// </summary>
        /// <returns>The array of all views as an IViewModelBase array.</returns>
        public IViewModelBase[] GetViewArray()
        {
            return viewArray;
        }
        /// <summary>
        /// Getter for the overview view model
        /// </summary>
        /// <returns><The overviewviewmodel./returns>
        public OverviewViewModel getOverviewViewModel()
        {
            return overviewViewModel;
        }
        /// <summary>
        /// Returns the current rappor instance.
        /// </summary>
        /// <returns>returns the current rappor instance.</returns>
        public RAPPOR GetRAPPOR()
        {
            return RAPPOR;
        }
        /// <summary>
        /// Returns the selected view integer
        /// </summary>
        /// <returns>Returns the selected view integer</returns>
        public int GetSelectedViewInteger()
        {
            return selectedViewInteger;
        }
        /// <summary>
        /// Sets the selected view integer
        /// </summary>
        /// <param name="selection">Chosen selection</param>
        public void SetSelectedViewInteger(int selection)
        {
            selectedViewInteger = selection;
        }

        #region IRAPPOR
        /// <summary>
        /// Getter and setter for the presentation view model.
        /// </summary>
        public RAPPORPresentationViewModel PresentationViewModel => this;
        /// <summary>
        /// Getter and setter for the rappor instance.
        /// </summary>
        public RAPPOR RAPPOR { get; set; }
        /// <summary>
        /// Getter and setter for the rappor settings.
        /// </summary>
        public RAPPORSettings Settings => (RAPPORSettings)RAPPOR.Settings;

        #endregion IRAPPOR

        public IViewModelBase[] GetviewArray()
        {
            return viewArray;
        }
    }
}