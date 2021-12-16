using CrypTool.Plugins.ChaCha.ViewModel.Components;

namespace CrypTool.Plugins.ChaCha.ViewModel
{
    [PluginBase.Attributes.Localization("CrypTool.Plugins.ChaCha.Properties.Resources")]
    internal class OverviewViewModel : ViewModelBase, INavigation, ITitle, IChaCha
    {
        public OverviewViewModel(ChaChaPresentationViewModel chachaPresentationViewModel)
        {
            PresentationViewModel = chachaPresentationViewModel;
            Name = this["OverviewName"];
            Title = this["OverviewTitle"];
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

        #region ITitle

        private string _title; public string Title
        {
            get
            {
                if (_title == null)
                {
                    _title = "";
                }

                return _title;
            }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion ITitle

        #region IChaCha

        public ChaChaPresentationViewModel PresentationViewModel { get; private set; }
        public ChaCha ChaCha => PresentationViewModel.ChaCha;
        public ChaChaSettings Settings => (ChaChaSettings)ChaCha.Settings;

        #endregion IChaCha
    }
}