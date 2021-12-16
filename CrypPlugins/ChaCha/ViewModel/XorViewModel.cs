using CrypTool.Plugins.ChaCha.ViewModel.Components;

namespace CrypTool.Plugins.ChaCha.ViewModel
{
    internal class XorViewModel : ViewModelBase, INavigation, ITitle, IDiffusion
    {
        public XorViewModel(ChaChaPresentationViewModel chachaPresentationViewModel)
        {
            PresentationViewModel = chachaPresentationViewModel;
            Name = "XOR";
            Title = "XOR";
        }

        #region Binding Properties (Diffusion)

        public bool DiffusionActive => PresentationViewModel.DiffusionActive;

        #endregion Binding Properties (Diffusion)

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

        #region IDiffusion

        public bool ShowToggleButton => DiffusionActive;

        #endregion IDiffusion

        #region IChaCha

        public ChaChaPresentationViewModel PresentationViewModel { get; private set; }
        public ChaCha ChaCha => PresentationViewModel.ChaCha;
        public ChaChaSettings Settings => (ChaChaSettings)ChaCha.Settings;

        #endregion IChaCha
    }
}