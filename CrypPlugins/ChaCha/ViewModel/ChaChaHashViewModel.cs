using CrypTool.Plugins.ChaCha.Helper;
using CrypTool.Plugins.ChaCha.Helper.Validation;
using CrypTool.Plugins.ChaCha.Model;
using CrypTool.Plugins.ChaCha.ViewModel.Components;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Input;

namespace CrypTool.Plugins.ChaCha.ViewModel
{
    internal class ChaChaHashViewModel : ActionViewModelBase, INavigation, ITitle, IDiffusion
    {
        public ChaChaHashViewModel(ChaChaPresentationViewModel chachaPresentationViewModel) : base(chachaPresentationViewModel)
        {
            Name = this["ChaChaHashName"];
            Title = this["ChaChaHashTitle"];

            QRIO = new QRIOActionCreator(this);
            QRAdd = new QRAdditionActionCreator(this);
            QRXOR = new QRXORActionCreator(this);
            QRShift = new QRShiftActionCreator(this);
            StateActionCreator = new StateActionCreator(this);
        }

        #region ActionViewModelBase

        private QRIOActionCreator QRIO { get; set; }

        private QRAdditionActionCreator QRAdd { get; set; }
        private QRXORActionCreator QRXOR { get; set; }

        private QRShiftActionCreator QRShift { get; set; }

        private StateActionCreator StateActionCreator { get; set; }

        protected override void InitActions()
        {
            // ChaCha Hash sequence
            ActionCreator.StartSequence();

            ExtendLastAction(() => { CurrentKeystreamBlockIndex = null; });
            ExtendLastAction(() => { RoundsStep = true; });
            for (int keystreamBlock = 0; keystreamBlock < ChaCha.TotalKeystreamBlocks; ++keystreamBlock)
            {
                // Keystream Block sequence
                ActionCreator.StartSequence();

                // The very first action which is empty was added by ActionViewModelBase.
                // Thus the first action of every next keystream block, we add a empty action ourselves.
                // We also set the the correct original state as the base extension for all following actions
                // in this action sequence.
                int localKeystreamBlock = keystreamBlock; // fix for https://stackoverflow.com/questions/271440/captured-variable-in-a-loop-in-c-sharp
                // Set base extension for this keystream block sequence.
                ActionCreator.Sequential(() =>
                {
                    CurrentKeystreamBlockIndex = localKeystreamBlock;
                    RoundsStep = true;
                });
                // *** Make state insertion as last sequential action such that this action will get overriden by ReplaceLast call. ***
                ActionCreator.Sequential(StateActionCreator.InsertOriginalState(localKeystreamBlock));
                // Add empty action if this is not the very first action because ActionViewModelBase
                // has already added an empty action at the very beginning.
                // This empty action added here will be extended by the base extension we just added.
                if (keystreamBlock != 0)
                {
                    Seq(() => { });
                }

                // Extend the first action which was added by the base class.
                ExtendLastAction(() => { CurrentRoundIndex = null; });
                ExtendLastAction(() => { CurrentQRIndex = null; });

                for (int round = 0; round < Settings.Rounds; ++round)
                {
                    // round sequence
                    ActionCreator.StartSequence();
                    // Set a sequence base extension for round sequence.
                    // All further actions will now extend this action.
                    int localRound = round; // fix for https://stackoverflow.com/questions/271440/captured-variable-in-a-loop-in-c-sharp
                    ActionCreator.Sequential(() => { CurrentRoundIndex = localRound; });

                    for (int qr = 0; qr < 4; ++qr)
                    {
                        //  Quarterround sequence
                        ActionCreator.StartSequence();
                        // Set a sequence base extension for quarterround sequence.
                        // All further actions will now extend this action.
                        int localQr = qr; // fix for https://stackoverflow.com/questions/271440/captured-variable-in-a-loop-in-c-sharp
                        ActionCreator.Sequential(() => { CurrentQRIndex = localQr; });

                        // Copy from state into qr input
                        ActionCreator.StartSequence();
                        Seq(QRIO.MarkState(round, qr));
                        if (qr == 0)
                        {
                            TagRoundStartAction(keystreamBlock, round);
                            if (round == 0)
                            {
                                TagKeystreamBlockStartAction(keystreamBlock);
                            }
                        }
                        TagQRStartAction(keystreamBlock, round, qr);

                        Seq(QRIO.InsertQRInputs(keystreamBlock, round, qr).Extend(QRIO.MarkQRInputs));
                        ActionCreator.EndSequence();

                        // Keep inserted qr input and marked state entries for the rest of the qr sequence
                        Seq(QRIO.InsertQRInputs(keystreamBlock, round, qr).Extend(QRIO.MarkState(round, qr)));

                        // Run quarterround steps
                        for (int qrStep = 0; qrStep < 4; ++qrStep)
                        {
                            // Execute addition
                            ActionCreator.StartSequence();
                            Seq(QRAdd.MarkInputs(qrStep));
                            Seq(QRAdd.Insert(keystreamBlock, round, qr, qrStep).Extend(QRAdd.Mark(qrStep)));
                            ActionCreator.EndSequence();

                            // Keep addition values
                            Seq(QRAdd.Insert(keystreamBlock, round, qr, qrStep));

                            // Execute XOR
                            ActionCreator.StartSequence();
                            Seq(QRXOR.MarkInputs(qrStep));
                            Seq(QRXOR.Insert(keystreamBlock, round, qr, qrStep).Extend(QRXOR.Mark(qrStep)));
                            ActionCreator.EndSequence();

                            // Keep XOR values
                            Seq(QRXOR.Insert(keystreamBlock, round, qr, qrStep));

                            // Execute shift
                            ActionCreator.StartSequence();
                            Seq(QRShift.MarkInputs(qrStep));
                            Seq(QRShift.Insert(keystreamBlock, round, qr, qrStep).Extend(QRShift.Mark(qrStep)));
                            ActionCreator.EndSequence();

                            // Keep shift values
                            Seq(QRShift.Insert(keystreamBlock, round, qr, qrStep));
                        }

                        // Fill quarterround output
                        ActionCreator.StartSequence();
                        Seq(QRIO.MarkQROutputPaths);
                        Seq(QRIO.InsertQROutputs(keystreamBlock, round, qr).Extend(QRIO.MarkQROutputs));
                        ActionCreator.EndSequence();

                        // Keep qr output values
                        Seq(QRIO.InsertQROutputs(keystreamBlock, round, qr));

                        // Copy from qr output to state
                        ActionCreator.StartSequence();
                        Seq(QRIO.MarkQROutputs);

                        Seq(QRIO.UpdateState(keystreamBlock, round, qr).Extend(QRIO.MarkState(round, qr)));
                        if (qr == 3)
                        {
                            TagRoundEndStartAction(keystreamBlock, round);
                        }

                        TagQREndAction(keystreamBlock, round, qr);

                        ActionCreator.EndSequence();

                        // End quarterround sequence
                        ActionCreator.EndSequence();

                        // Keep state update for rest of round sequence
                        // FIXME There is a bug that the state update order is not as expected.
                        //   We need to apply previous state updates ( from the previous round ) and then the new state update.
                        //   But it for some reasons first applies the latest state and then the state updates fro last round in the correct order.
                        //   So basically like this: 4, 0, 3, 2, 1.
                        //   This leads to an overwrite of the diagonal.
                        Seq(QRIO.UpdateState(keystreamBlock, round, qr));
                    }
                    // End round sequence
                    ActionCreator.EndSequence();

                    // Replace previous state update with the state updates from all quarterrounds of the last round for rest of ChaCha hash sequence.
                    // The previous action should be the the original state insertion. See comment above wrapped with ***.
                    // We do this because the complete state will be modified in every round anyway and thus we would just "overdraw" if we apply all state updates from each round
                    // in a sequence.
                    // TODO(refactor) This can probably be handled way more elegant.
                    ActionCreator.ReplaceLast(QRIO.UpdateState(keystreamBlock, round, 3)
                        .Extend(QRIO.UpdateState(keystreamBlock, round, 2), QRIO.UpdateState(keystreamBlock, round, 1), QRIO.UpdateState(keystreamBlock, round, 0)));
                }

                // Addition + little-endian step
                ActionCreator.StartSequence();
                // Base extension.
                ActionCreator.Sequential(() =>
                {
                    StateActionCreator.HideOriginalState();
                    StateActionCreator.HideAdditionResult();
                    StateActionCreator.HideLittleEndian();
                });

                Seq(() =>
                {
                    RoundsStep = false;
                    CurrentRoundIndex = Settings.Rounds - 1;
                    CurrentQRIndex = 3;
                });
                TagMatrixStartAction(keystreamBlock);
                Seq(StateActionCreator.ShowOriginalState(localKeystreamBlock));
                Seq(StateActionCreator.ShowAdditionResult(localKeystreamBlock));
                Seq(StateActionCreator.ShowLittleEndian(localKeystreamBlock));
                TagMatrixEndAction(keystreamBlock);

                ActionCreator.EndSequence();

                // End keystream block sequence
                ActionCreator.EndSequence();
            }

            // End ChaCha Hash sequence
            ActionCreator.EndSequence();
        }

        public override void Reset()
        {
            StateActionCreator.ClearStateMatrix();
            StateActionCreator.InsertFirstOriginalState();
            QRIO.ResetQuarterroundValues();
        }

        #endregion ActionViewModelBase

        #region ChaCha Hash Navigation Bar

        #region Keystream Block input

        private ICommand _nextKeystreamBlockCommand; public ICommand NextKeystreamBlockCommand
        {
            get
            {
                if (_nextKeystreamBlockCommand == null)
                {
                    _nextKeystreamBlockCommand = new RelayCommand((arg) => NextKeystreamBlock(), (arg) => CanNextKeystreamBlock);
                }

                return _nextKeystreamBlockCommand;
            }
        }

        public bool CanNextKeystreamBlock => CurrentKeystreamBlockIndex == null || CurrentKeystreamBlockIndex < ChaCha.TotalKeystreamBlocks - 1;

        private ICommand _prevKeystreamBlockCommand; public ICommand PrevKeystreamBlockCommand
        {
            get
            {
                if (_prevKeystreamBlockCommand == null)
                {
                    _prevKeystreamBlockCommand = new RelayCommand((arg) => PrevKeystreamBlock(), (arg) => CanPrevKeystreamBlock);
                }

                return _prevKeystreamBlockCommand;
            }
        }

        public bool CanPrevKeystreamBlock => CurrentKeystreamBlockIndex != null && CurrentKeystreamBlockIndex != 0;

        private void NextKeystreamBlock()
        {
            int nextKeystreamBlockIndex = CurrentKeystreamBlockIndex == null ? 0 : (int)CurrentKeystreamBlockIndex + 1;
            int nextKeystreamBlockActionIndex = GetTaggedActionIndex(KeystreamBlockStartTag(nextKeystreamBlockIndex));
            MoveToAction(nextKeystreamBlockActionIndex);
        }

        private void PrevKeystreamBlock()
        {
            int prevKeystreamBlockIndex = (int)CurrentKeystreamBlockIndex - 1;
            int prevKeystreamBlockActionIndex = GetTaggedActionIndex(KeystreamBlockStartTag(prevKeystreamBlockIndex));
            MoveToAction(prevKeystreamBlockActionIndex);
        }

        private ValidationRule _keystreamBlockInputRule; private ValidationRule KeystreamBlockInputRule
        {
            get
            {
                if (_keystreamBlockInputRule == null)
                {
                    _keystreamBlockInputRule = new UserInputValidationRule(1, ChaCha.TotalKeystreamBlocks);
                }

                return _keystreamBlockInputRule;
            }
        }

        private KeyEventHandler _keystreamBlockInputHandler; public KeyEventHandler KeystreamBlockInputHandler
        {
            get
            {
                if (_keystreamBlockInputHandler == null)
                {
                    _keystreamBlockInputHandler = UserInputHandler(KeystreamBlockInputRule, GoToKeystreamBlock);
                }

                return _keystreamBlockInputHandler;
            }
        }

        /// <summary>
        /// Go to given keystream block.
        /// </summary>
        /// <param name="keystreamBlock">One-based keystreamBlock index.</param>
        private void GoToKeystreamBlock(int keystreamBlock)
        {
            // Value comes from user. Map to zero-based round index.
            int keystreamBlockIndex = keystreamBlock - 1;
            int keystreamBlockActionIndex = GetTaggedActionIndex(KeystreamBlockStartTag(keystreamBlockIndex));
            MoveToAction(keystreamBlockActionIndex);
        }

        #endregion Keystream Block input

        #region Round input

        private ICommand _nextRoundCommand; public ICommand NextRoundCommand
        {
            get
            {
                if (_nextRoundCommand == null)
                {
                    _nextRoundCommand = new RelayCommand((arg) => NextRound(), (arg) => CanNextRound);
                }

                return _nextRoundCommand;
            }
        }

        public bool CanNextRound => CurrentRoundIndex == null || CurrentRoundIndex < Settings.Rounds - 1;

        private ICommand _prevRoundCommand; public ICommand PrevRoundCommand
        {
            get
            {
                if (_prevRoundCommand == null)
                {
                    _prevRoundCommand = new RelayCommand((arg) => PrevRound(), (arg) => CanPrevRound);
                }

                return _prevRoundCommand;
            }
        }

        public bool CanPrevRound => CurrentRoundIndex != null && CurrentRoundIndex != 0;

        private void NextRound()
        {
            // TODO check for round overflow where we have to increase keystream block index
            int keystreamBlockIndex = CurrentKeystreamBlockIndex ?? 0;
            int nextRoundIndex = CurrentRoundIndex == null ? 0 : (int)CurrentRoundIndex + 1;
            int nextRoundActionIndex = GetTaggedActionIndex(RoundStartTag(keystreamBlockIndex, nextRoundIndex));
            MoveToAction(nextRoundActionIndex);
        }

        private void PrevRound()
        {
            if (CurrentRoundIndex == null || CurrentRoundIndex == 0)
            {
                throw new InvalidOperationException("CurrentRoundIndex was null or zero in PrevRound.");
            }
            // TODO check for round underflow where we have to decrease keystream block index
            int keystreamBlockIndex = CurrentKeystreamBlockIndex ?? 0;
            int currentRoundStartIndex = GetTaggedActionIndex(RoundStartTag(keystreamBlockIndex, (int)CurrentRoundIndex));
            // only go back to start of previous round if we are on the start of a round
            // else go to start of current round
            if (CurrentActionIndex == currentRoundStartIndex)
            {
                int prevRoundIndex = (int)CurrentRoundIndex - 1;
                int prevRoundActionIndex = GetTaggedActionIndex(RoundStartTag(keystreamBlockIndex, prevRoundIndex));
                MoveToAction(prevRoundActionIndex);
            }
            else
            {
                MoveToAction(currentRoundStartIndex);
            }
        }

        private ValidationRule _roundInputRule; private ValidationRule RoundInputRule
        {
            get
            {
                if (_roundInputRule == null)
                {
                    _roundInputRule = new UserInputValidationRule(1, Settings.Rounds);
                }

                return _roundInputRule;
            }
        }

        private KeyEventHandler _roundInputHandler; public KeyEventHandler RoundInputHandler
        {
            get
            {
                if (_roundInputHandler == null)
                {
                    _roundInputHandler = UserInputHandler(RoundInputRule, GoToRound);
                }

                return _roundInputHandler;
            }
        }

        /// <summary>
        /// Go to given round.
        /// </summary>
        /// <param name="round">One-based round index.</param>
        private void GoToRound(int round)
        {
            int keystreamBlockIndex = CurrentKeystreamBlockIndex ?? 0;
            // Value comes from user. Map to zero-based round index.
            int roundIndex = round - 1;
            int roundActionIndex = GetTaggedActionIndex(RoundStartTag(keystreamBlockIndex, roundIndex));
            MoveToAction(roundActionIndex);
        }

        #endregion Round input

        #region Quarterround input

        private ICommand _nextQRCommand; public ICommand NextQRCommand
        {
            get
            {
                if (_nextQRCommand == null)
                {
                    _nextQRCommand = new RelayCommand((arg) => NextQR(), (arg) => CanNextQR);
                }

                return _nextQRCommand;
            }
        }

        public bool CanNextQR => CanNextRound || CurrentQRIndex == null || CurrentQRIndex < 3;

        private ICommand _prevQRCommand; public ICommand PrevQRCommand
        {
            get
            {
                if (_prevQRCommand == null)
                {
                    _prevQRCommand = new RelayCommand((arg) => PrevQR(), (arg) => CanPrevQR);
                }

                return _prevQRCommand;
            }
        }

        public bool CanPrevQR => CanPrevRound || (CurrentQRIndex != null && CurrentQRIndex != 0);

        private void NextQR()
        {
            // TODO check for qr overflow which results in round overflow where we have to increase keystream block index
            int keystreamBlockIndex = CurrentKeystreamBlockIndex ?? 0;
            int roundIndex = CurrentRoundIndex ?? 0;
            int nextQRIndex = CurrentQRIndex == null ? 0 : ((int)(CurrentQRIndex + 1) % 4);
            if (CurrentQRIndex != null && CurrentQRIndex == 3)
            {
                roundIndex += 1;
            }

            int nextQRActionIndex = GetTaggedActionIndex(QRStartTag(keystreamBlockIndex, roundIndex, nextQRIndex));
            MoveToAction(nextQRActionIndex);
        }

        private void PrevQR()
        {
            if (CurrentQRIndex == null)
            {
                throw new InvalidOperationException("CurrentQRIndex was null in PrevQR.");
            }
            // TODO check for qr underflow which results in round underflow where we have to increase keystream block index
            int keystreamBlockIndex = CurrentKeystreamBlockIndex ?? 0;
            int currentRoundIndex = CurrentRoundIndex ?? 0;
            int currentQRIndex = CurrentQRIndex ?? 0;
            int currentQRStartIndex = GetTaggedActionIndex(QRStartTag(keystreamBlockIndex, currentRoundIndex, currentQRIndex));
            // only go back to start of previous qr if we are on the start of a qr
            // else go to start of current qr
            if (CurrentActionIndex == currentQRStartIndex)
            {
                // check if we would "underflow" and thus have to go to the last quarterround of previous round
                int prevQRIndex = currentQRIndex == 0 ? 3 : currentQRIndex - 1;
                int roundIndexForPrevQR = currentQRIndex == 0 ? currentRoundIndex - 1 : currentRoundIndex;
                int prevRoundActionIndex = GetTaggedActionIndex(QRStartTag(keystreamBlockIndex, roundIndexForPrevQR, prevQRIndex));
                MoveToAction(prevRoundActionIndex);
            }
            else
            {
                MoveToAction(currentQRStartIndex);
            }
        }

        private ICommand _quarterroundStartCommand; public ICommand QuarterroundStartCommand
        {
            get
            {
                if (_quarterroundStartCommand == null)
                {
                    _quarterroundStartCommand = new RelayCommand((arg) => GoToQRStart(int.Parse((string)arg)));
                }

                return _quarterroundStartCommand;
            }
        }

        private ICommand _quarterroundEndCommand; public ICommand QuarterroundEndCommand
        {
            get
            {
                if (_quarterroundEndCommand == null)
                {
                    _quarterroundEndCommand = new RelayCommand((arg) => GoToQREnd(int.Parse((string)arg)));
                }

                return _quarterroundEndCommand;
            }
        }

        /// <summary>
        /// Go to the quarterround start of the given qr index of the current round.
        /// </summary>
        /// <param name="qr">Zero-based qr index.</param>
        private void GoToQRStart(int qr)
        {
            int keystreamBlockIndex = CurrentKeystreamBlockIndex ?? 0;
            int round = CurrentRoundIndex ?? 0;
            int qrStartActionIndex = GetTaggedActionIndex(QRStartTag(keystreamBlockIndex, round, qr));
            MoveToAction(qrStartActionIndex);
        }

        /// <summary>
        /// Go to the quarterround end of the given qr index of the current round.
        /// </summary>
        /// <param name="qr">Zero-based qr index.</param>
        private void GoToQREnd(int qr)
        {
            int keystreamBlockIndex = CurrentKeystreamBlockIndex ?? 0;
            int round = CurrentRoundIndex ?? 0;
            int qrStartActionIndex = GetTaggedActionIndex(QREndTag(keystreamBlockIndex, round, qr));
            MoveToAction(qrStartActionIndex);
        }

        private ValidationRule _qrInputRule; private ValidationRule QRInputRule
        {
            get
            {
                if (_qrInputRule == null)
                {
                    _qrInputRule = new UserInputValidationRule(1, 4);
                }

                return _qrInputRule;
            }
        }

        private KeyEventHandler _qrInputHandler; public KeyEventHandler QRInputHandler
        {
            get
            {
                if (_qrInputHandler == null)
                {
                    _qrInputHandler = UserInputHandler(QRInputRule, GoToQR);
                }

                return _qrInputHandler;
            }
        }

        /// <summary>
        /// Go to given quarterround.
        /// </summary>
        /// <param name="qr">One-based quarterround index.</param>
        private void GoToQR(int qr)
        {
            int keystreamBlockIndex = CurrentKeystreamBlockIndex ?? 0;
            // Value comes from user. Map to zero-based round index.
            int qrIndex = qr - 1;
            int round = CurrentRoundIndex ?? 0;
            int qrActionIndex = GetTaggedActionIndex(QRStartTag(keystreamBlockIndex, round, qrIndex));
            MoveToAction(qrActionIndex);
        }

        #endregion Quarterround input

        #region Matrix

        private ICommand _matrixStartCommand; public ICommand MatrixStartCommand
        {
            get
            {
                if (_matrixStartCommand == null)
                {
                    _matrixStartCommand = new RelayCommand((arg) => GoToMatrixStart());
                }

                return _matrixStartCommand;
            }
        }

        private ICommand _matrixEndCommand; public ICommand MatrixEndCommand
        {
            get
            {
                if (_matrixEndCommand == null)
                {
                    _matrixEndCommand = new RelayCommand((arg) => GoToMatrixEnd());
                }

                return _matrixEndCommand;
            }
        }

        /// <summary>
        /// Go to the start of the addition step of the given keystream block.
        /// </summary>
        /// <param name="keystreamBlock">Zero-based keystream block index.</param>
        private void GoToMatrixStart()
        {
            int keystreamBlockIndex = CurrentKeystreamBlockIndex ?? 0;
            int matrixStartActionIndex = GetTaggedActionIndex(MatrixStartTag(keystreamBlockIndex));
            MoveToAction(matrixStartActionIndex);
        }

        /// <summary>
        /// Go to the end of the addition step of the given keystream block.
        /// </summary>
        /// <param name="keystreamBlock">Zero-based keystream block index.</param>
        private void GoToMatrixEnd()
        {
            int keystreamBlockIndex = CurrentKeystreamBlockIndex ?? 0;
            int matrixEndActionIndex = GetTaggedActionIndex(MatrixEndTag(keystreamBlockIndex));
            MoveToAction(matrixEndActionIndex);
        }

        #endregion Matrix

        #endregion ChaCha Hash Navigation Bar

        #region Binding Properties

        private ObservableCollection<StateValue> _stateValues; public ObservableCollection<StateValue> StateValues
        {
            get
            {
                if (_stateValues == null)
                {
                    _stateValues = new ObservableCollection<StateValue>();
                }

                return _stateValues;
            }
            private set
            {
                if (_stateValues != value)
                {
                    _stateValues = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<VisualQRStep> _qrStep; public ObservableCollection<VisualQRStep> QRStep
        {
            get
            {
                if (_qrStep == null)
                {
                    _qrStep = new ObservableCollection<VisualQRStep>();
                }

                return _qrStep;
            }
            private set
            {
                if (_qrStep != value)
                {
                    _qrStep = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<StateValue> _originalState; public ObservableCollection<StateValue> OriginalState
        {
            get
            {
                if (_originalState == null)
                {
                    _originalState = new ObservableCollection<StateValue>();
                }

                return _originalState;
            }
            private set
            {
                if (_originalState != value)
                {
                    _originalState = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<StateValue> _additionResultState; public ObservableCollection<StateValue> AdditionResultState
        {
            get
            {
                if (_additionResultState == null)
                {
                    _additionResultState = new ObservableCollection<StateValue>();
                }

                return _additionResultState;
            }
            private set
            {
                if (_additionResultState != value)
                {
                    _additionResultState = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<StateValue> _littleEndianState; public ObservableCollection<StateValue> LittleEndianState
        {
            get
            {
                if (_littleEndianState == null)
                {
                    _littleEndianState = new ObservableCollection<StateValue>();
                }

                return _littleEndianState;
            }
            private set
            {
                if (_littleEndianState != value)
                {
                    _littleEndianState = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _currentKeystreamBlockIndex = 0; public int? CurrentKeystreamBlockIndex
        {
            get => _currentKeystreamBlockIndex;
            set
            {
                if (_currentKeystreamBlockIndex != value)
                {
                    _currentKeystreamBlockIndex = value;
                    CurrentUserKeystreamBlockIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Same purpose as property 'CurrentUserActionIndex'.
        /// See its documentation for further information.
        /// </summary>
        private int? _currentUserKeystreamBlockIndex = null; public int? CurrentUserKeystreamBlockIndex

        {
            get => _currentUserKeystreamBlockIndex;
            set
            {
                if (_currentUserKeystreamBlockIndex != value)
                {
                    _currentUserKeystreamBlockIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _currentRoundIndex = null; public int? CurrentRoundIndex
        {
            get => _currentRoundIndex;
            set
            {
                if (_currentRoundIndex != value)
                {
                    _currentRoundIndex = value;
                    CurrentUserRoundIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Same purpose as property 'CurrentUserActionIndex'.
        /// See its documentation for further information.
        /// </summary>
        private int? _currentUserRoundIndex = null; public int? CurrentUserRoundIndex

        {
            get => _currentUserRoundIndex;
            set
            {
                if (_currentUserRoundIndex != value)
                {
                    _currentUserRoundIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _currentQRIndex = null; public int? CurrentQRIndex
        {
            get => _currentQRIndex;
            set
            {
                if (_currentQRIndex != value)
                {
                    _currentQRIndex = value;
                    CurrentUserQRIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Same purpose as property 'CurrentUserActionIndex'.
        /// See its documentation for further information.
        /// </summary>
        private int? _currentUserQRIndex = null; public int? CurrentUserQRIndex

        {
            get => _currentUserQRIndex;
            set
            {
                if (_currentUserQRIndex != value)
                {
                    _currentUserQRIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        #region QRInX

        private QRValue _qrInA; public QRValue QRInA
        {
            get
            {
                if (_qrInA == null)
                {
                    _qrInA = new QRValue();
                }

                return _qrInA;
            }
            set
            {
                _qrInA = value;
                OnPropertyChanged();
            }
        }

        private QRValue _qrInB; public QRValue QRInB
        {
            get
            {
                if (_qrInB == null)
                {
                    _qrInB = new QRValue();
                }

                return _qrInB;
            }
            set
            {
                _qrInB = value;
                OnPropertyChanged();
            }
        }

        private QRValue _qrInC; public QRValue QRInC
        {
            get
            {
                if (_qrInC == null)
                {
                    _qrInC = new QRValue();
                }

                return _qrInC;
            }
            set
            {
                _qrInC = value;
                OnPropertyChanged();
            }
        }

        private QRValue _qrInD; public QRValue QRInD
        {
            get
            {
                if (_qrInD == null)
                {
                    _qrInD = new QRValue();
                }

                return _qrInD;
            }
            set
            {
                _qrInD = value;
                OnPropertyChanged();
            }
        }

        #endregion QRInX

        #region QROutX

        private QRValue _qrOutA; public QRValue QROutA
        {
            get
            {
                if (_qrOutA == null)
                {
                    _qrOutA = new QRValue();
                }

                return _qrOutA;
            }
            set
            {
                _qrOutA = value;
                OnPropertyChanged();
            }
        }

        private QRValue _qrOutB; public QRValue QROutB
        {
            get
            {
                if (_qrOutB == null)
                {
                    _qrOutB = new QRValue();
                }

                return _qrOutB;
            }
            set
            {
                _qrOutB = value;
                OnPropertyChanged();
            }
        }

        private QRValue _qrOutC; public QRValue QROutC
        {
            get
            {
                if (_qrOutC == null)
                {
                    _qrOutC = new QRValue();
                }

                return _qrOutC;
            }
            set
            {
                _qrOutC = value;
                OnPropertyChanged();
            }
        }

        private QRValue _qrOutD; public QRValue QROutD
        {
            get
            {
                if (_qrOutD == null)
                {
                    _qrOutD = new QRValue();
                }

                return _qrOutD;
            }
            set
            {
                _qrOutD = value;
                OnPropertyChanged();
            }
        }

        #endregion QROutX

        /// <summary>
        /// Indicates if we are currently in the rounds step or in the addition / little-endian step.
        /// </summary>
        private bool _roundsStep = true; public bool RoundsStep

        {
            get => _roundsStep;
            set
            {
                if (_roundsStep != value)
                {
                    _roundsStep = value;
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

        public int DiffusionFlippedBits => BitFlips.FlippedBits(DiffusionStateValues.Select(sv => sv.Value).ToArray(), StateValues.Select(sv => sv.Value).ToArray());

        public int TotalBits => 512;

        public double DiffusionFlippedBitsPercentage => (double)DiffusionFlippedBits / TotalBits;

        private ObservableCollection<StateValue> _diffusionStateValues; public ObservableCollection<StateValue> DiffusionStateValues
        {
            get
            {
                if (_diffusionStateValues == null)
                {
                    _diffusionStateValues = new ObservableCollection<StateValue>();
                }

                return _diffusionStateValues;
            }
            private set
            {
                if (_diffusionStateValues != value)
                {
                    _diffusionStateValues = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<VisualQRStep> _diffusionQrStep; public ObservableCollection<VisualQRStep> DiffusionQRStep
        {
            get
            {
                if (_diffusionQrStep == null)
                {
                    _diffusionQrStep = new ObservableCollection<VisualQRStep>();
                }

                return _diffusionQrStep;
            }
            private set
            {
                if (_diffusionQrStep != value)
                {
                    _diffusionQrStep = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<StateValue> _diffusionOriginalState; public ObservableCollection<StateValue> DiffusionOriginalState
        {
            get
            {
                if (_diffusionOriginalState == null)
                {
                    _diffusionOriginalState = new ObservableCollection<StateValue>();
                }

                return _diffusionOriginalState;
            }
            private set
            {
                if (_diffusionOriginalState != value)
                {
                    _diffusionOriginalState = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<StateValue> _diffusionAdditionResultState; public ObservableCollection<StateValue> DiffusionAdditionResultState
        {
            get
            {
                if (_diffusionAdditionResultState == null)
                {
                    _diffusionAdditionResultState = new ObservableCollection<StateValue>();
                }

                return _diffusionAdditionResultState;
            }
            private set
            {
                if (_diffusionAdditionResultState != value)
                {
                    _diffusionAdditionResultState = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<StateValue> _diffusionLittleEndianState; public ObservableCollection<StateValue> DiffusionLittleEndianState
        {
            get
            {
                if (_diffusionLittleEndianState == null)
                {
                    _diffusionLittleEndianState = new ObservableCollection<StateValue>();
                }

                return _diffusionLittleEndianState;
            }
            private set
            {
                if (_diffusionLittleEndianState != value)
                {
                    _diffusionLittleEndianState = value;
                    OnPropertyChanged();
                }
            }
        }

        #region DiffusionQRInX

        private QRValue _diffusionQrInA; public QRValue DiffusionQRInA
        {
            get
            {
                if (_diffusionQrInA == null)
                {
                    _diffusionQrInA = new QRValue();
                }

                return _diffusionQrInA;
            }
            set
            {
                _diffusionQrInA = value;
                OnPropertyChanged();
            }
        }

        private QRValue _diffusionQrInB; public QRValue DiffusionQRInB
        {
            get
            {
                if (_diffusionQrInB == null)
                {
                    _diffusionQrInB = new QRValue();
                }

                return _diffusionQrInB;
            }
            set
            {
                _diffusionQrInB = value;
                OnPropertyChanged();
            }
        }

        private QRValue _diffusionQrInC; public QRValue DiffusionQRInC
        {
            get
            {
                if (_diffusionQrInC == null)
                {
                    _diffusionQrInC = new QRValue();
                }

                return _diffusionQrInC;
            }
            set
            {
                _diffusionQrInC = value;
                OnPropertyChanged();
            }
        }

        private QRValue _diffusionQrInD; public QRValue DiffusionQRInD
        {
            get
            {
                if (_diffusionQrInD == null)
                {
                    _diffusionQrInD = new QRValue();
                }

                return _diffusionQrInD;
            }
            set
            {
                _diffusionQrInD = value;
                OnPropertyChanged();
            }
        }

        #endregion DiffusionQRInX

        #region DiffusionQROutX

        private QRValue _diffusionQrOutA; public QRValue DiffusionQROutA
        {
            get
            {
                if (_diffusionQrOutA == null)
                {
                    _diffusionQrOutA = new QRValue();
                }

                return _diffusionQrOutA;
            }
            set
            {
                _diffusionQrOutA = value;
                OnPropertyChanged();
            }
        }

        private QRValue _diffusionqrOutB; public QRValue DiffusionQROutB
        {
            get
            {
                if (_diffusionqrOutB == null)
                {
                    _diffusionqrOutB = new QRValue();
                }

                return _diffusionqrOutB;
            }
            set
            {
                _diffusionqrOutB = value;
                OnPropertyChanged();
            }
        }

        private QRValue _diffusionQrOutC; public QRValue DiffusionQROutC
        {
            get
            {
                if (_diffusionQrOutC == null)
                {
                    _diffusionQrOutC = new QRValue();
                }

                return _diffusionQrOutC;
            }
            set
            {
                _diffusionQrOutC = value;
                OnPropertyChanged();
            }
        }

        private QRValue _diffusionQrOutD; public QRValue DiffusionQROutD
        {
            get
            {
                if (_diffusionQrOutD == null)
                {
                    _diffusionQrOutD = new QRValue();
                }

                return _diffusionQrOutD;
            }
            set
            {
                _diffusionQrOutD = value;
                OnPropertyChanged();
            }
        }

        #endregion DiffusionQROutX

        #endregion Binding Properties (Diffusion)

        #region IActionTag

        /// <summary>
        /// Tag the last added action as the start of the given round.
        /// </summary>
        /// <param name="keystreamBlock">Zero-based keystream block index.</param>
        private void TagKeystreamBlockStartAction(int keystreamBlock)
        {
            TagLastAction(KeystreamBlockStartTag(keystreamBlock));
        }

        /// <summary>
        /// Tag the last added action as the start of the given round.
        /// </summary>
        /// <param name="keystreamBlock">Zero-based keystream block index.</param>
        /// <param name="round">Zero-based round index.</param>
        private void TagRoundStartAction(int keystreamBlock, int round)
        {
            TagLastAction(RoundStartTag(keystreamBlock, round));
        }

        /// <summary>
        /// Tag the last added action as the end of the given round.
        /// </summary>
        /// <param name="keystreamBlock">Zero-based keystream block index.</param>
        /// <param name="round">Zero-based round index.</param>
        private void TagRoundEndStartAction(int keystreamBlock, int round)
        {
            TagLastAction(RoundEndTag(keystreamBlock, round));
        }

        /// <summary>
        /// Tag the last added action as the start of the given quarterround of the given round.
        /// </summary>
        /// <param name="keystreamBlock">Zero-based keystream block index.</param>
        /// <param name="round">Zero-based round index.</param>
        /// <param name="qr">Zero-based qr index.</param>
        private void TagQRStartAction(int keystreamBlock, int round, int qr)
        {
            TagLastAction(QRStartTag(keystreamBlock, round, qr));
        }

        /// <summary>
        /// Tag the last added action as the end of the given quarterround of the given round.
        /// </summary>
        /// <param name="keystreamBlock">Zero-based keystream block index.</param>
        /// <param name="round">Zero-based round index.</param>
        /// <param name="qr">Zero-based qr index.</param>
        private void TagQREndAction(int keystreamBlock, int round, int qr)
        {
            TagLastAction(QREndTag(keystreamBlock, round, qr));
        }

        /// <summary>
        /// Tag the last added action as the start of the addition step of the given keystream block.
        /// </summary>
        /// <param name="keystreamBlock">Zero-based keystream block index.</param>
        private void TagMatrixStartAction(int keystreamBlock)
        {
            TagLastAction(MatrixStartTag(keystreamBlock));
        }

        /// <summary>
        /// Tag the last added action as the end of the keystream block sequence.
        /// </summary>
        /// <param name="keystreamBlock">Zero-based keystream block index.</param>
        private void TagMatrixEndAction(int keystreamBlock)
        {
            TagLastAction(MatrixEndTag(keystreamBlock));
        }

        /// <summary>
        /// Extend the action at the given action index with the given action.
        /// </summary>
        private void ExtendAction(int actionIndex, Action toExtend)
        {
            Action action = Actions[actionIndex];
            Action updated = action.Extend(toExtend);
            Actions[actionIndex] = updated;
        }

        /// <summary>
        /// Extend the last added action with the given action.
        /// </summary>
        private void ExtendLastAction(Action toExtend)
        {
            ExtendAction(ActionIndex, toExtend);
        }

        /// <summary>
        /// Check if keystream block input for tag is in valid range.
        /// </summary>
        /// <param name="keystreamBlock">Zero-based keystream block index.</param>
        private void AssertKeystreamBlockTagInput(int keystreamBlock)
        {
            int maxKeystreamBlock = ChaCha.TotalKeystreamBlocks - 1;
            if (keystreamBlock < 0 || keystreamBlock > maxKeystreamBlock)
            {
                throw new ArgumentOutOfRangeException("keystreamBlock", $"Keystream Block must be between 0 and {maxKeystreamBlock}. Received: {keystreamBlock}");
            }
        }

        /// <summary>
        /// Check if round input for tag is in valid range.
        /// </summary>
        /// <param name="round">Zero-based round index.</param>
        private void AssertRoundTagInput(int round)
        {
            int maxRoundIndex = Settings.Rounds - 1;
            if (round < 0 || round > maxRoundIndex)
            {
                throw new ArgumentOutOfRangeException("round", $"Round must be between 0 and {maxRoundIndex}. Received: {round}");
            }
        }

        /// <summary>
        /// Check if qr input for tag is in valid range.
        /// </summary>
        /// <param name="qr">Zero-based qr index.</param>
        private void AssertQRTagInput(int qr)
        {
            if (qr < 0 || qr > 3)
            {
                throw new ArgumentOutOfRangeException("qr", $"Quarterround must be between 0 and 3. Received {qr}");
            }
        }

        /// <summary>
        /// Create the keystream block start tag.
        /// </summary>
        /// <param name="keystreamBlock">Zero-based keystream block index.</param>
        private string KeystreamBlockStartTag(int keystreamBlock)
        {
            AssertKeystreamBlockTagInput(keystreamBlock);
            return $"KEYSTREAM_BLOCK_START_{keystreamBlock}";
        }

        /// <summary>
        /// Create the round start tag.
        /// </summary>
        /// <param name="keystreamBlock">Zero-based keystream block index.</param>
        /// <param name="round">Zero-based round index.</param>
        private string RoundStartTag(int keystreamBlock, int round)
        {
            AssertKeystreamBlockTagInput(keystreamBlock);
            AssertRoundTagInput(round);
            return $"KEYSTREAM_BLOCK_{keystreamBlock}_ROUND_START_{round}";
        }

        /// <summary>
        /// Create the round end tag.
        /// </summary>
        /// <param name="keystreamBlock">Zero-based keystream block index.</param>
        /// <param name="round">Zero-based round index.</param>
        private string RoundEndTag(int keystreamBlock, int round)
        {
            AssertKeystreamBlockTagInput(keystreamBlock);
            AssertRoundTagInput(round);
            return $"KEYSTREAM_BLOCK_{keystreamBlock}_ROUND_END_{round}";
        }

        /// <summary>
        /// Create the quarterround start tag.
        /// </summary>
        /// <param name="keystreamBlock">Zero-based keystream block index.</param>
        /// <param name="round">Zero-based round index.</param>
        /// <param name="qr">Zero-based qr index.</param>
        private string QRStartTag(int keystreamBlock, int round, int qr)
        {
            AssertKeystreamBlockTagInput(keystreamBlock);
            AssertRoundTagInput(round);
            AssertQRTagInput(qr);
            return $"KEYSTREAM_BLOCK_{keystreamBlock}_ROUND_{round}_QR_START_{qr}";
        }

        /// <summary>
        /// Create the quarterround end tag.
        /// </summary>
        /// <param name="keystreamBlock">Zero-based keystream block index.</param>
        /// <param name="round">Zero-based round index.</param>
        /// <param name="qr">Zero-based qr index.</param>
        private string QREndTag(int keystreamBlock, int round, int qr)
        {
            AssertKeystreamBlockTagInput(keystreamBlock);
            AssertRoundTagInput(round);
            AssertQRTagInput(qr);
            return $"KEYSTREAM_BLOCK_{keystreamBlock}_ROUND_{round}_QR_END_{qr}";
        }

        /// <summary>
        /// Create the addition start tag.
        /// </summary>
        /// <param name="keystreamBlock">Zero-based keystream block index.</param>
        private string MatrixStartTag(int keystreamBlock)
        {
            AssertKeystreamBlockTagInput(keystreamBlock);
            return $"KEYSTREAM_BLOCK_{keystreamBlock}_MATRIX_START";
        }

        /// <summary>
        /// Create the keystream end tag.
        /// </summary>
        /// <param name="keystreamBlock">Zero-based keystream block index.</param>
        private string MatrixEndTag(int keystreamBlock)
        {
            AssertKeystreamBlockTagInput(keystreamBlock);
            return $"KEYSTREAM_BLOCK_{keystreamBlock}_MATRIX_END";
        }

        #endregion IActionTag

        #region INavigation

        private void AssertEmptyAndInitialize<T>(ObservableCollection<T> list, string name, int count = 0) where T : new()
        {
            Debug.Assert(list.Count == 0, $"{name} should be empty during ChaCha hash setup.");
            for (int i = 0; i < count; ++i)
            {
                list.Add(new T());
            }
        }

        public override void Setup()
        {
            Debug.Assert(ChaCha.OriginalState.Count == ChaCha.TotalKeystreamBlocks,
                $"Count of OriginalState was not equal to TotalKeystreamBlocks. Expected: {ChaCha.TotalKeystreamBlocks}. Actual: {ChaCha.OriginalState.Count}");
            AssertEmptyAndInitialize(StateValues, "StateValues");
            AssertEmptyAndInitialize(DiffusionStateValues, "DiffusionStateValues", 16);
            AssertEmptyAndInitialize(OriginalState, "OriginalState", 16);
            AssertEmptyAndInitialize(DiffusionOriginalState, "DiffusionOriginalState", 16);
            AssertEmptyAndInitialize(AdditionResultState, "AdditionResultState", 16);
            AssertEmptyAndInitialize(DiffusionAdditionResultState, "DiffusionAdditionResultState", 16);
            AssertEmptyAndInitialize(LittleEndianState, "LittleEndianState", 16);
            AssertEmptyAndInitialize(DiffusionLittleEndianState, "DiffusionLittleEndianState", 16);
            AssertEmptyAndInitialize(QRStep, "QRStep", 4);
            AssertEmptyAndInitialize(DiffusionQRStep, "DiffusionQRStep", 4);
            uint[] state = ChaCha.OriginalState[0];
            for (int i = 0; i < 16; ++i)
            {
                StateValues.Add(new StateValue(state[i]));
            }
            // First setup page, then call base setup because action buffer handler may depend on things being setup already.
            base.Setup();
        }

        public override void Teardown()
        {
            base.Teardown();
            // Clear lists to undo Setup.
            StateValues.Clear();
            DiffusionStateValues.Clear();
            QRStep.Clear();
            DiffusionQRStep.Clear();
            OriginalState.Clear();
            DiffusionOriginalState.Clear();
            AdditionResultState.Clear();
            DiffusionAdditionResultState.Clear();
            LittleEndianState.Clear();
            DiffusionLittleEndianState.Clear();
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