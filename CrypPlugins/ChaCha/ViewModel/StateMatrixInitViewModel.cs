using CrypTool.Plugins.ChaCha.Helper;
using CrypTool.Plugins.ChaCha.ViewModel.Components;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Windows.Input;

namespace CrypTool.Plugins.ChaCha.ViewModel
{
    internal class StateMatrixInitViewModel : ActionViewModelBase, INavigation, ITitle, IDiffusion
    {
        public StateMatrixInitViewModel(ChaChaPresentationViewModel chachaPresentationViewModel) : base(chachaPresentationViewModel)
        {
            Name = this["StateMatrixName"];
            Title = this["StateMatrixTitle"];
        }

        #region ActionViewModelBase

        private readonly string CONSTANTS_ENCODING_START = "CONSTANTS_ENCODING_START";
        private readonly string CONSTANTS_ENCODING_END = "CONSTANTS_ENCODING_END";

        private readonly string KEY_ENCODING_START = "KEY_ENCODING_START";
        private readonly string KEY_ENCODING_END = "KEY_ENCODING_END";

        private readonly string COUNTER_ENCODING_START = "COUNTER_ENCODING_START";
        private readonly string COUNTER_ENCODING_END = "COUNTER_ENCODING_END";

        private readonly string IV_ENCODING_START = "IV_ENCODING_START";
        private readonly string IV_ENCODING_END = "IV_ENCODING_END";

        protected override void InitActions()
        {
            ActionCreator.StartSequence();

            #region Constants

            Seq(() => { Description[0] = true; });

            ActionCreator.StartSequence();

            Seq(() => { ConstantsEncoding = true; });
            TagLastAction(CONSTANTS_ENCODING_START);
            Seq(() => { ConstantsEncodingInput = true; });
            Seq(() => { ConstantsEncodingASCII = true; });
            Seq(() => { ConstantsEncodingChunkify = true; });
            Seq(() => { ConstantsEncodingLittleEndian = true; });
            Seq(() => { ConstantsMatrix = true; });
            TagLastAction(CONSTANTS_ENCODING_END);

            ActionCreator.EndSequence();

            #endregion Constants

            #region Key

            Seq(() => { ConstantsMatrix = true; Description[1] = true; });

            ActionCreator.StartSequence();

            Seq(() => { KeyEncoding = true; });
            TagLastAction(KEY_ENCODING_START);
            Seq(() => { KeyEncodingInput = true; });
            Seq(() => { KeyEncodingChunkify = true; });
            Seq(() => { KeyEncodingLittleEndian = true; });
            Seq(() => { KeyMatrix = true; });
            TagLastAction(KEY_ENCODING_END);

            ActionCreator.EndSequence();

            #endregion Key

            #region Counter

            Seq(() => { KeyMatrix = true; Description[2] = true; });

            ActionCreator.StartSequence();

            Seq(() => { CounterEncoding = true; });
            TagLastAction(COUNTER_ENCODING_START);
            Seq(() => { CounterEncodingInput = true; });
            Seq(() => { CounterEncodingReverse = true; });
            Seq(() => { CounterEncodingChunkify = true; });
            Seq(() => { CounterEncodingLittleEndian = true; });
            Seq(() => { CounterMatrix = true; if (Settings.Version.CounterBits == 64) { State13Matrix = true; } });
            TagLastAction(COUNTER_ENCODING_END);

            ActionCreator.EndSequence();

            #endregion Counter

            #region IV

            Seq(() => { CounterMatrix = true; if (Settings.Version.CounterBits == 64) { State13Matrix = true; } Description[3] = true; });

            ActionCreator.StartSequence();

            Seq(() => { IVEncoding = true; });
            TagLastAction(IV_ENCODING_START);
            Seq(() => { IVEncodingInput = true; });
            Seq(() => { IVEncodingChunkify = true; });
            Seq(() => { IVEncodingLittleEndian = true; });
            Seq(() => { IVMatrix = true; if (Settings.Version.CounterBits == 32) { State13Matrix = true; } });

            #endregion IV

            Seq(() => { Description[4] = true; });
            TagLastAction(IV_ENCODING_END);

            ActionCreator.EndSequence();

            ActionCreator.EndSequence();
        }

        public override void Reset()
        {
            HideDescriptions();
            HideEncoding();
            HideState();
        }

        private void HideDescriptions()
        {
            for (int i = 0; i < Description.Count; ++i)
            {
                Description[i] = false;
            }
        }

        private void HideState()
        {
            // Hide state values.
            ConstantsMatrix = false;
            KeyMatrix = false;
            CounterMatrix = false;
            State13Matrix = false;
            IVMatrix = false;
        }

        private void HideEncoding()
        {
            ConstantsEncoding = false;
            ConstantsEncodingInput = false;
            ConstantsEncodingASCII = false;
            ConstantsEncodingChunkify = false;
            ConstantsEncodingLittleEndian = false;

            KeyEncoding = false;
            KeyEncodingInput = false;
            KeyEncodingChunkify = false;
            KeyEncodingLittleEndian = false;

            CounterEncoding = false;
            CounterEncodingInput = false;
            CounterEncodingReverse = false;
            CounterEncodingChunkify = false;
            CounterEncodingLittleEndian = false;

            IVEncoding = false;
            IVEncodingInput = false;
            IVEncodingChunkify = false;
            IVEncodingLittleEndian = false;
        }

        #endregion ActionViewModelBase

        #region Navigation Bar

        #region Constants

        private ICommand _goToConstantsEncodingStart; public ICommand GoToConstantsEncodingStartCommand
        {
            get
            {
                if (_goToConstantsEncodingStart == null)
                {
                    _goToConstantsEncodingStart = new RelayCommand((arg) => MoveToTaggedAction(CONSTANTS_ENCODING_START));
                }

                return _goToConstantsEncodingStart;
            }
        }

        private ICommand _goToConstantsEncodingEnd; public ICommand GoToConstantsEncodingEndCommand
        {
            get
            {
                if (_goToConstantsEncodingEnd == null)
                {
                    _goToConstantsEncodingEnd = new RelayCommand((arg) => MoveToTaggedAction(CONSTANTS_ENCODING_END));
                }

                return _goToConstantsEncodingEnd;
            }
        }

        #endregion Constants

        #region Key

        private ICommand _goToKeyEncodingStart; public ICommand GoToKeyEncodingStartCommand
        {
            get
            {
                if (_goToKeyEncodingStart == null)
                {
                    _goToKeyEncodingStart = new RelayCommand((arg) => MoveToTaggedAction(KEY_ENCODING_START));
                }

                return _goToKeyEncodingStart;
            }
        }

        private ICommand _goToKeyEncodingEnd; public ICommand GoToKeyEncodingEndCommand
        {
            get
            {
                if (_goToKeyEncodingEnd == null)
                {
                    _goToKeyEncodingEnd = new RelayCommand((arg) => MoveToTaggedAction(KEY_ENCODING_END));
                }

                return _goToKeyEncodingEnd;
            }
        }

        #endregion Key

        #region Counter

        private ICommand _goToCounterEncodingStart; public ICommand GoToCounterEncodingStartCommand
        {
            get
            {
                if (_goToCounterEncodingStart == null)
                {
                    _goToCounterEncodingStart = new RelayCommand((arg) => MoveToTaggedAction(COUNTER_ENCODING_START));
                }

                return _goToCounterEncodingStart;
            }
        }

        private ICommand _goToCounterEncodingEnd; public ICommand GoToCounterEncodingEndCommand
        {
            get
            {
                if (_goToCounterEncodingEnd == null)
                {
                    _goToCounterEncodingEnd = new RelayCommand((arg) => MoveToTaggedAction(COUNTER_ENCODING_END));
                }

                return _goToCounterEncodingEnd;
            }
        }

        #endregion Counter

        #region IV

        private ICommand _goToIVEncodingStart; public ICommand GoToIVEncodingStartCommand
        {
            get
            {
                if (_goToIVEncodingStart == null)
                {
                    _goToIVEncodingStart = new RelayCommand((arg) => MoveToTaggedAction(IV_ENCODING_START));
                }

                return _goToIVEncodingStart;
            }
        }

        private ICommand _goToIVEncodingEnd; public ICommand GoToIVEncodingEndCommand
        {
            get
            {
                if (_goToIVEncodingEnd == null)
                {
                    _goToIVEncodingEnd = new RelayCommand((arg) => MoveToTaggedAction(IV_ENCODING_END));
                }

                return _goToIVEncodingEnd;
            }
        }

        #endregion IV

        #endregion Navigation Bar

        #region Binding Properties

        private ObservableCollection<uint> _stateMatrixValues; public ObservableCollection<uint> StateMatrixValues
        {
            get
            {
                if (_stateMatrixValues == null)
                {
                    _stateMatrixValues = new ObservableCollection<uint>();
                }

                return _stateMatrixValues;
            }
            private set
            {
                if (_stateMatrixValues != value)
                {
                    _stateMatrixValues = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<bool> _description; public ObservableCollection<bool> Description
        {
            get
            {
                if (_description == null)
                {
                    _description = new ObservableCollection<bool>(Enumerable.Repeat(false, 5).ToList());
                }

                return _description;
            }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion Binding Properties

        #region Binding Properties (Diffusion)

        public byte[] DiffusionInputKey => PresentationViewModel.DiffusionInputKey;

        public byte[] DiffusionInputIV => PresentationViewModel.DiffusionInputIV;

        public BigInteger DiffusionInitialCounter => PresentationViewModel.DiffusionInitialCounter;

        public bool DiffusionActive => PresentationViewModel.DiffusionActive;

        #endregion Binding Properties (Diffusion)

        #region Binding Properties (Constants)

        public string ASCIIConstants => ChaCha.InputKey.Length == 16 ? "expand 16-byte k" : "expand 32-byte k";

        private bool _constantsEncoding; public bool ConstantsEncoding
        {
            get => _constantsEncoding;
            set
            {
                _constantsEncoding = value;
                OnPropertyChanged();
            }
        }

        private bool _constantsEncodingInput; public bool ConstantsEncodingInput
        {
            get => _constantsEncodingInput;
            set
            {
                _constantsEncodingInput = value;
                OnPropertyChanged();
            }
        }

        private bool _constantsEncodingASCII; public bool ConstantsEncodingASCII
        {
            get => _constantsEncodingASCII;
            set
            {
                _constantsEncodingASCII = value;
                OnPropertyChanged();
            }
        }

        private bool _constantsEncodingChunkify; public bool ConstantsEncodingChunkify
        {
            get => _constantsEncodingChunkify;
            set
            {
                _constantsEncodingChunkify = value;
                OnPropertyChanged();
            }
        }

        public bool _constantsEncodingLittleEndian; public bool ConstantsEncodingLittleEndian
        {
            get => _constantsEncodingLittleEndian;
            set
            {
                _constantsEncodingLittleEndian = value;
                OnPropertyChanged();
            }
        }

        private bool _constantsMatrix; public bool ConstantsMatrix
        {
            get => _constantsMatrix;
            set
            {
                _constantsMatrix = value;
                OnPropertyChanged();
            }
        }

        #endregion Binding Properties (Constants)

        #region Binding Properties (Key)

        private bool _keyEncoding; public bool KeyEncoding
        {
            get => _keyEncoding;
            set
            {
                _keyEncoding = value;
                OnPropertyChanged();
            }
        }

        private bool _keyEncodingInput; public bool KeyEncodingInput
        {
            get => _keyEncodingInput;
            set
            {
                _keyEncodingInput = value;
                OnPropertyChanged();
            }
        }

        private bool _keyEncodingChunkify; public bool KeyEncodingChunkify
        {
            get => _keyEncodingChunkify;
            set
            {
                _keyEncodingChunkify = value;
                OnPropertyChanged();
            }
        }

        private bool _keyEncodingLittleEndian; public bool KeyEncodingLittleEndian
        {
            get => _keyEncodingLittleEndian;
            set
            {
                _keyEncodingLittleEndian = value;
                OnPropertyChanged();
            }
        }

        private bool _keyMatrix; public bool KeyMatrix
        {
            get => _keyMatrix;
            set
            {
                _keyMatrix = value;
                OnPropertyChanged();
            }
        }

        #endregion Binding Properties (Key)

        #region Binding Properties (Counter)

        private bool _counterEncoding; public bool CounterEncoding
        {
            get => _counterEncoding;
            set
            {
                _counterEncoding = value;
                OnPropertyChanged();
            }
        }

        private bool _counterEncodingInput; public bool CounterEncodingInput
        {
            get => _counterEncodingInput;
            set
            {
                _counterEncodingInput = value;
                OnPropertyChanged();
            }
        }

        private bool _counterEncodingReverse; public bool CounterEncodingReverse
        {
            get => _counterEncodingReverse;
            set
            {
                _counterEncodingReverse = value;
                OnPropertyChanged();
            }
        }

        private bool _counterEncodingChunkify; public bool CounterEncodingChunkify
        {
            get => _counterEncodingChunkify;
            set
            {
                _counterEncodingChunkify = value;
                OnPropertyChanged();
            }
        }

        private bool _counterEncodingLittleEndian; public bool CounterEncodingLittleEndian
        {
            get => _counterEncodingLittleEndian;
            set
            {
                _counterEncodingLittleEndian = value;
                OnPropertyChanged();
            }
        }

        private bool _counterMatrix; public bool CounterMatrix
        {
            get => _counterMatrix;
            set
            {
                _counterMatrix = value;
                OnPropertyChanged();
            }
        }

        private bool _state13Matrix; public bool State13Matrix
        {
            get => _state13Matrix;
            set
            {
                _state13Matrix = value;
                OnPropertyChanged();
            }
        }

        #endregion Binding Properties (Counter)

        #region Binding Properties (IV)

        private bool _ivEncoding; public bool IVEncoding
        {
            get => _ivEncoding;
            set
            {
                _ivEncoding = value;
                OnPropertyChanged();
            }
        }

        private bool _ivEncodingInput; public bool IVEncodingInput
        {
            get => _ivEncodingInput;
            set
            {
                _ivEncodingInput = value;
                OnPropertyChanged();
            }
        }

        private bool _ivEncodingChunkify; public bool IVEncodingChunkify
        {
            get => _ivEncodingChunkify;
            set
            {
                _ivEncodingChunkify = value;
                OnPropertyChanged();
            }
        }

        private bool _ivEncodingLittleEndian; public bool IVEncodingLittleEndian
        {
            get => _ivEncodingLittleEndian;
            set
            {
                _ivEncodingLittleEndian = value;
                OnPropertyChanged();
            }
        }

        private bool _ivMatrix; public bool IVMatrix
        {
            get => _ivMatrix;
            set
            {
                _ivMatrix = value;
                OnPropertyChanged();
            }
        }

        #endregion Binding Properties (IV)

        #region INavigation

        public override void Setup()
        {
            InitStateMatrixValues();
            base.Setup();
        }

        public override void Teardown()
        {
            base.Teardown();
        }

        private void InitStateMatrixValues()
        {
            uint[] state = ChaCha.OriginalState[0];
            StateMatrixValues.Clear();
            for (int i = 0; i < state.Length; ++i)
            {
                StateMatrixValues.Add(state[i]);
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

        #region IDiffusion

        public bool ShowToggleButton => DiffusionActive;

        #endregion IDiffusion
    }
}