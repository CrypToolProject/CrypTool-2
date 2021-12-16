using CrypTool.Plugins.ChaCha.Helper;
using CrypTool.Plugins.ChaCha.ViewModel.Components;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Windows.Controls;

namespace CrypTool.Plugins.ChaCha.ViewModel
{
    internal class DiffusionViewModel : ViewModelBase, INavigation, ITitle, IChaCha, IDiffusion
    {
        public DiffusionViewModel(ChaChaPresentationViewModel chachaPresentationViewModel)
        {
            PresentationViewModel = chachaPresentationViewModel;
            ChaCha.PropertyChanged += new PropertyChangedEventHandler(PluginInputChanged);
            Name = "Diffusion";
            Title = "Diffusion";
            InitHandlers();
        }

        private void PluginInputChanged(object sender, PropertyChangedEventArgs e)
        {
            DiffusionKey = ChaCha.InputKey;
            DiffusionKeyExplicit = ChaCha.InputKey;
            DiffusionKeyXOR = ByteUtil.XOR(DiffusionKey, ChaCha.InputKey);

            DiffusionIV = ChaCha.InputIV;
            DiffusionIVExplicit = ChaCha.InputIV;
            DiffusionIVXOR = ByteUtil.XOR(DiffusionIV, ChaCha.InputIV);

            DiffusionInitialCounter = ChaCha.InitialCounter;
            DiffusionInitialCounterExplicit = DiffusionInitialCounter;
            DiffusionInitialCounterXOR = DiffusionInitialCounter ^ ChaCha.InitialCounter;
        }

        #region Input Handlers

        public delegate TextChangedEventHandler ValidatedTextChangedEventHandler(ValidationRule validationRule);

        public static ValidatedTextChangedEventHandler ValidateInputBeforeHandle(Action<string> handle)
        {
            return (ValidationRule validationRule) =>
            {
                return (object sender, TextChangedEventArgs e) =>
                {
                    string value = ((TextBox)sender).Text;
                    ValidationResult result = validationRule.Validate(value, CultureInfo.CurrentCulture);
                    if (result == ValidationResult.ValidResult)
                    {
                        handle(value);
                    }
                };
            };
        }

        private void InitHandlers()
        {
            DiffusionKeyExplicitInputHandler = ValidateInputBeforeHandle((string value) =>
            {
                DiffusionKey = Formatter.Bytes(value);
                DiffusionKeyXOR = ByteUtil.XOR(DiffusionKey, ChaCha.InputKey);
            });
            DiffusionKeyXORInputHandler = ValidateInputBeforeHandle((string value) =>
            {
                byte[] input = Formatter.Bytes(value);
                DiffusionKey = ByteUtil.XOR(input, ChaCha.InputKey);
                DiffusionKeyExplicit = DiffusionKey;
            });

            DiffusionIVExplicitInputHandler = ValidateInputBeforeHandle((string value) =>
            {
                DiffusionIV = Formatter.Bytes(value);
                DiffusionIVXOR = ByteUtil.XOR(DiffusionIV, ChaCha.InputIV);
            });
            DiffusionIVXORInputHandler = ValidateInputBeforeHandle((string value) =>
            {
                byte[] input = Formatter.Bytes(value);
                DiffusionIV = ByteUtil.XOR(input, ChaCha.InputIV);
                DiffusionIVExplicit = DiffusionIV;
            });

            DiffusionInitialCounterExplicitInputHandler = ValidateInputBeforeHandle((string value) =>
            {
                BigInteger input = Formatter.BigInteger(value);
                DiffusionInitialCounter = input;
                BigInteger xor;
                if (Settings.Version.CounterBits == 64)
                {
                    xor = (ulong)input ^ (ulong)ChaCha.InitialCounter;
                }
                else
                {
                    xor = (uint)input ^ (uint)ChaCha.InitialCounter;
                }
                DiffusionInitialCounterXOR = xor;
            });
            DiffusionInitialCounterXORInputHandler = ValidateInputBeforeHandle((string value) =>
            {
                BigInteger input = Formatter.BigInteger(value);
                if (Settings.Version.CounterBits == 64)
                {
                    DiffusionInitialCounter = (ulong)input ^ (ulong)ChaCha.InitialCounter;
                }
                else
                {
                    DiffusionInitialCounter = (uint)input ^ (uint)ChaCha.InitialCounter;
                }
                DiffusionInitialCounterExplicit = DiffusionInitialCounter;
            });
        }

        public ValidatedTextChangedEventHandler DiffusionKeyExplicitInputHandler;

        public ValidatedTextChangedEventHandler DiffusionKeyXORInputHandler;

        public ValidatedTextChangedEventHandler DiffusionIVExplicitInputHandler;

        public ValidatedTextChangedEventHandler DiffusionIVXORInputHandler;

        public ValidatedTextChangedEventHandler DiffusionInitialCounterExplicitInputHandler;

        public ValidatedTextChangedEventHandler DiffusionInitialCounterXORInputHandler;

        #endregion Input Handlers

        #region Binding Properties (Key)

        /// <summary>
        /// The value which is shown in the diffusion key input box.
        /// </summary>
        public byte[] _diffusionKeyExplicit; public byte[] DiffusionKeyExplicit

        {
            get => _diffusionKeyExplicit;
            set
            {
                if (_diffusionKeyExplicit != value)
                {
                    _diffusionKeyExplicit = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The value which is shown in the diffusion XOR key input box.
        /// </summary>
        public byte[] _diffusionKeyXOR; public byte[] DiffusionKeyXOR

        {
            get => _diffusionKeyXOR;
            set
            {
                if (_diffusionKeyXOR != value)
                {
                    _diffusionKeyXOR = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The actual diffusion key which will be used for cipher execution.
        /// </summary>
        private byte[] _diffusionKey; public byte[] DiffusionKey

        {
            get => _diffusionKey;
            set
            {
                if (_diffusionKey != value)
                {
                    _diffusionKey = value;
                    OnPropertyChanged();
                    OnPropertyChanged("DiffusionActive");
                    OnPropertyChanged("FlippedBits");
                    OnPropertyChanged("FlippedBitsPercentage");
                }
            }
        }

        #endregion Binding Properties (Key)

        #region Binding Properties (IV)

        /// <summary>
        /// The value which is shown in the diffusion IV input box.
        /// </summary>
        public byte[] _diffusionIVExplicit; public byte[] DiffusionIVExplicit

        {
            get => _diffusionIVExplicit;
            set
            {
                if (_diffusionIVExplicit != value)
                {
                    _diffusionIVExplicit = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The value which is shown in the diffusion XOR IV input box.
        /// </summary>
        public byte[] _diffusionIVXOR; public byte[] DiffusionIVXOR

        {
            get => _diffusionIVXOR;
            set
            {
                if (_diffusionIVXOR != value)
                {
                    _diffusionIVXOR = value;
                    OnPropertyChanged();
                }
            }
        }

        private byte[] _diffusionIV; public byte[] DiffusionIV
        {
            get => _diffusionIV;
            set
            {
                if (_diffusionIV != value)
                {
                    _diffusionIV = value;
                    OnPropertyChanged();
                    OnPropertyChanged("DiffusionActive");
                    OnPropertyChanged("FlippedBits");
                    OnPropertyChanged("FlippedBitsPercentage");
                }
            }
        }

        #endregion Binding Properties (IV)

        #region Binding Properties (Initial Counter)

        /// <summary>
        /// The value which is shown in the diffusion Initial Counter input box.
        /// </summary>
        public BigInteger _diffusionInitialCounterExplicit; public BigInteger DiffusionInitialCounterExplicit

        {
            get => _diffusionInitialCounterExplicit;
            set
            {
                if (_diffusionInitialCounterExplicit != value)
                {
                    _diffusionInitialCounterExplicit = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The value which is shown in the diffusion XOR Initial Counter input box.
        /// </summary>
        public BigInteger _diffusionInitialCounterXOR; public BigInteger DiffusionInitialCounterXOR

        {
            get => _diffusionInitialCounterXOR;
            set
            {
                if (_diffusionInitialCounterXOR != value)
                {
                    _diffusionInitialCounterXOR = value;
                    OnPropertyChanged();
                }
            }
        }

        private BigInteger _diffusionInitialCounter; public BigInteger DiffusionInitialCounter
        {
            get => _diffusionInitialCounter;
            set
            {
                if (_diffusionInitialCounter != value)
                {
                    _diffusionInitialCounter = value;
                    OnPropertyChanged();
                    OnPropertyChanged("DiffusionActive");
                    OnPropertyChanged("FlippedBits");
                    OnPropertyChanged("FlippedBitsPercentage");
                }
            }
        }

        #endregion Binding Properties (Initial Counter)

        #region Binding Properties (Status)

        public int FlippedBits => BitFlips.FlippedBits(DiffusionKey, ChaCha.InputKey) + BitFlips.FlippedBits(DiffusionIV, ChaCha.InputIV) + BitFlips.FlippedBits((ulong)DiffusionInitialCounter, (ulong)ChaCha.InitialCounter);

        public int TotalBits => DiffusionKey.Length * 8 + DiffusionIV.Length * 8 + (int)Settings.Version.CounterBits;

        public double FlippedBitsPercentage => (double)FlippedBits / TotalBits;

        public bool DiffusionActive => !(DiffusionKey.SequenceEqual(ChaCha.InputKey) && DiffusionIV.SequenceEqual(ChaCha.InputIV) && DiffusionInitialCounter == ChaCha.InitialCounter);

        #endregion Binding Properties (Status)

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
            if (DiffusionActive)
            {
                // Execute ChaCha with Diffusion values.
                ChaCha.ExecuteDiffusion(DiffusionKey, DiffusionIV, (ulong)DiffusionInitialCounter);
            }
            else
            {
                // If diffusion is inactive, there is no toggle button thus the value of the toggle button should be reset.
                PresentationViewModel.ShowXOR = false;
            }
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

        #region IDiffusion

        public bool ShowToggleButton => true;

        #endregion IDiffusion
    }
}