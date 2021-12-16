using CrypTool.Plugins.ChaCha.ViewModel.Components;

namespace CrypTool.Plugins.ChaCha.ViewModel
{
    internal class StartViewModel : ViewModelBase, INavigation
    {
        public StartViewModel()
        {
            Name = "Start";
        }

        #region INavigation

        private string _name; public string Name
        {
            get
            {
                if (_name == null)
                {
                    _name = "";
                }

                return _name;
            }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public void Setup()
        {
        }

        public void Teardown()
        {
        }

        #endregion INavigation
    }
}