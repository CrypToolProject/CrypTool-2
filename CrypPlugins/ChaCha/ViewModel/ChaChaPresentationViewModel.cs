/*
   Copyright (C) CrypTool 2 Team

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using CrypTool.Plugins.ChaCha.Helper;
using CrypTool.Plugins.ChaCha.ViewModel.Components;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Windows.Input;

namespace CrypTool.Plugins.ChaCha.ViewModel
{
    internal class ChaChaPresentationViewModel : ViewModelBase, IChaCha
    {
        public static string RESET_VISUALIZATION = "RESET_VISUALIZATION";

        public ChaChaPresentationViewModel(ChaCha chaCha)
        {
            ChaCha = chaCha;
            ChaCha.PropertyChanged += new PropertyChangedEventHandler(OnPluginPropertyChanged);
            // Add available pages
            Pages.Add(new StartViewModel());
            Pages.Add(new OverviewViewModel(this));
            Pages.Add(new DiffusionViewModel(this));
            Pages.Add(new StateMatrixInitViewModel(this));
            Pages.Add(new ChaChaHashViewModel(this));
            Pages.Add(new XorViewModel(this));

            // Set starting page
            CurrentPage = Pages[0];
        }

        /// <summary>
        /// This is called if a property on the plugin has changed.
        /// </summary>
        private void OnPluginPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == RESET_VISUALIZATION)
            {
                ShowXOR = false;
                ChangePage(Pages[0]);
            }
            OnPropertyChanged("NavigationEnabled");
        }

        #region Commands

        private ICommand _changePageCommand; public ICommand ChangePageCommand
        {
            get
            {
                if (_changePageCommand == null)
                {
                    _changePageCommand = new RelayCommand(
                        p => ChangePage((INavigation)p),
                        p => p is INavigation);
                }

                return _changePageCommand;
            }
        }

        private List<INavigation> _pages; public List<INavigation> Pages
        {
            get
            {
                if (_pages == null)
                {
                    _pages = new List<INavigation>();
                }

                return _pages;
            }
        }

        private INavigation _currentPage; public INavigation CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion Commands

        #region Binding Properties

        public bool NavigationEnabled => ChaCha.ExecutionFinished && ChaCha.IsValid;

        #endregion Binding Properties

        #region Binding Properties (Diffusion)

        private DiffusionViewModel DiffusionViewModel => (DiffusionViewModel)Pages[2];

        public byte[] DiffusionInputKey => DiffusionViewModel.DiffusionKey;

        public byte[] DiffusionInputIV => DiffusionViewModel.DiffusionIV;

        public BigInteger DiffusionInitialCounter => DiffusionViewModel.DiffusionInitialCounter;

        public bool DiffusionActive => DiffusionViewModel.DiffusionActive;

        private bool _showXOR; public bool ShowXOR
        {
            get => _showXOR;
            set
            {
                _showXOR = value;
                OnPropertyChanged();
            }
        }

        #endregion Binding Properties (Diffusion)

        #region Methods

        private void ChangePage(INavigation viewModel)
        {
            CurrentPage.Teardown();
            if (!Pages.Contains(viewModel))
            {
                Pages.Add(viewModel);
            }

            CurrentPage = Pages
                .FirstOrDefault(vm => vm == viewModel);
            CurrentPage.Setup();
        }

        #endregion Methods

        #region IChaCha

        public ChaChaPresentationViewModel PresentationViewModel => this;
        public ChaCha ChaCha { get; set; }
        public ChaChaSettings Settings => (ChaChaSettings)ChaCha.Settings;

        #endregion IChaCha
    }
}