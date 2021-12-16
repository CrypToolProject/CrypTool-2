using CrypTool.Plugins.ChaCha.Helper.Converter;
using CrypTool.Plugins.ChaCha.Model;
using CrypTool.Plugins.ChaCha.ViewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CrypTool.Plugins.ChaCha.View
{
    /// <summary>
    /// Interaction logic for ChaChaHash.xaml
    /// </summary>
    [PluginBase.Attributes.Localization("CrypTool.Plugins.ChaCha.Properties.Resources")]
    public partial class ChaChaHash : UserControl
    {
        public ChaChaHash()
        {
            InitializeComponent();
            ActionViewBase.LoadLocaleResources(this);
            DataContextChanged += OnDataContextChanged;
        }

        private ChaChaHashViewModel ViewModel { get; set; }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ViewModel = (ChaChaHashViewModel)e.NewValue;
            if (ViewModel != null)
            {
                // On page enter, the real DOM and virtual DOM should be in sync.
                DomSync();

                ViewModel.PropertyChanged += new PropertyChangedEventHandler(OnViewModelPropertyChange);
                ActionViewBase.AddEventHandlers(ViewModel, Root);

                InitKeystreamBlockInput();
                InitRoundInput();
                InitQRInput();
            }
        }

        #region Diffusion - Virtual DOM

        /**
         * These variables implement the "Real DOM" part of the "Virtual DOM"
         * system. This should improve peformance during diffusion.
         *
         * This design is inspired by the virtual DOM used in ReactJS.
         * See https://www.codecademy.com/articles/react-virtual-dom.
         *
         * The variables here represent the current state of the real DOM
         * whereas the variables in the ViewModel represent the virtual DOM.
         *
         * During the execution of an action, the virtual DOM changes.
         * At the end of an action execution, MOVE_ACTION_FINISHED is dispatched.
         *
         * This tells the view that it should now compare the real DOM with the virtual DOM.
         *
         * Only if something is different, the appropriate dispatch is done and thus
         * resulting in an actual change in the real DOM the user can see.
         *
         * Therefore, after every MOVE_ACTION_FINISHED, the real DOM and virtual DOM
         * are in sync.
         */

        private readonly uint?[] DiffusionState = new uint?[16];

        private readonly uint?[] DiffusionOriginalState = new uint?[16];

        private readonly uint?[] DiffusionAdditionResultState = new uint?[16];

        private readonly uint?[] DiffusionLittleEndianState = new uint?[16];

        private uint? DiffusionQRInA_, DiffusionQRInB_, DiffusionQRInC_, DiffusionQRInD_;

        private uint? DiffusionQROutA_, DiffusionQROutB_, DiffusionQROutC_, DiffusionQROutD_;

        private readonly uint?[,] DiffusionQRStep = new uint?[4, 3];

        private void OnViewModelPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            Dispatcher.Invoke(delegate
            {
                // Seems like WPF somehow calls this function but without a ViewModel even though it is attached to one?
                // Mhhh... we'll ignore it for now and just return, if this happens.
                // Maybe some async issues?
                if (ViewModel == null)
                {
                    return;
                }

                if (!e.PropertyName.Equals(ActionViewModelBase.MOVE_ACTION_FINISHED))
                {
                    return;
                }

                DomSync();
            });
        }

        private void DomSync()
        {
            // State matrices
            DomSync(DiffusionState, ViewModel.DiffusionStateValues, ViewModel.StateValues,
                (i) => $"DiffusionState{i}", (i) => $"DiffusionStateXOR{i}");
            DomSync(DiffusionOriginalState, ViewModel.DiffusionOriginalState, ViewModel.OriginalState,
                (i) => $"DiffusionOriginalState{i}", (i) => $"DiffusionOriginalStateXOR{i}");
            DomSync(DiffusionAdditionResultState, ViewModel.DiffusionAdditionResultState, ViewModel.AdditionResultState,
                (i) => $"DiffusionAdditionResultState{i}", (i) => $"DiffusionAdditionResultStateXOR{i}");
            DomSync(DiffusionLittleEndianState, ViewModel.DiffusionLittleEndianState, ViewModel.LittleEndianState,
                (i) => $"DiffusionLittleEndianState{i}", (i) => $"DiffusionLittleEndianStateXOR{i}");

            // QR Input
            DomSync(ref DiffusionQRInA_, ViewModel.DiffusionQRInA, ViewModel.QRInA, "DiffusionQRInA", "DiffusionQRInAXOR");
            DomSync(ref DiffusionQRInB_, ViewModel.DiffusionQRInB, ViewModel.QRInB, "DiffusionQRInB", "DiffusionQRInBXOR");
            DomSync(ref DiffusionQRInC_, ViewModel.DiffusionQRInC, ViewModel.QRInC, "DiffusionQRInC", "DiffusionQRInCXOR");
            DomSync(ref DiffusionQRInD_, ViewModel.DiffusionQRInD, ViewModel.QRInD, "DiffusionQRInD", "DiffusionQRInDXOR");

            // QR Output
            DomSync(ref DiffusionQROutA_, ViewModel.DiffusionQROutA, ViewModel.QROutA, "DiffusionQROutA", "DiffusionQROutAXOR");
            DomSync(ref DiffusionQROutB_, ViewModel.DiffusionQROutB, ViewModel.QROutB, "DiffusionQROutB", "DiffusionQROutBXOR");
            DomSync(ref DiffusionQROutC_, ViewModel.DiffusionQROutC, ViewModel.QROutC, "DiffusionQROutC", "DiffusionQROutCXOR");
            DomSync(ref DiffusionQROutD_, ViewModel.DiffusionQROutD, ViewModel.QROutD, "DiffusionQROutD", "DiffusionQROutDXOR");

            // QR Step
            for (int i = 0; i < 4; ++i)
            {
                DomSync(ref DiffusionQRStep[i, 0], ViewModel.DiffusionQRStep[i].Add, ViewModel.QRStep[i].Add, $"QRValueAddDiffusion_{i}", $"QRValueAddDiffusionXOR_{i}");
                DomSync(ref DiffusionQRStep[i, 1], ViewModel.DiffusionQRStep[i].XOR, ViewModel.QRStep[i].XOR, $"QRValueXORDiffusion_{i}", $"QRValueXORDiffusionXOR_{i}");
                DomSync(ref DiffusionQRStep[i, 2], ViewModel.DiffusionQRStep[i].Shift, ViewModel.QRStep[i].Shift, $"QRValueShiftDiffusion_{i}", $"QRValueShiftDiffusionXOR_{i}");
            }
        }

        private delegate string IndexToNameMapper(int i);

        private void DomSync(uint?[] real, uint?[] virtual_, uint?[] primary, IndexToNameMapper domDiffusionName, IndexToNameMapper domDiffusionXorName)
        {
            Debug.Assert(real.Length == virtual_.Length, "real and virtual_ length must be equal");
            for (int i = 0; i < real.Length; ++i)
            {
                uint? v = virtual_[i];
                uint? p = primary[i];
                DomSync(ref real[i], v, p, domDiffusionName(i), domDiffusionXorName(i));
            }
        }

        private void DomSync(uint?[] real, ObservableCollection<StateValue> virtual_, ObservableCollection<StateValue> primary, IndexToNameMapper domDiffusionName, IndexToNameMapper domDiffusionXorName)
        {
            DomSync(real, virtual_.Select(sv => sv.Value).ToArray(), primary.Select(sv => sv.Value).ToArray(), domDiffusionName, domDiffusionXorName);
        }

        private void DomSync(ref uint? r, uint? v, uint? p, string domDiffusionName, string domDiffusionXorName)
        {
            if (r.HasValue ^ v.HasValue || (r.HasValue && v.HasValue && r.Value != v.Value))
            {
                // Update real DOM
                RichTextBox rtb = (RichTextBox)FindName(domDiffusionName);
                RichTextBox rtbXor = (RichTextBox)FindName(domDiffusionXorName);
                InitOrClearDiffusionValue(rtb, v, p);
                InitOrClearXorValue(rtbXor, v, p);
                r = v;
            }
        }

        private void DomSync(ref uint? r, QRValue v, QRValue p, string domDiffusionName, string domDiffusionXorName)
        {
            DomSync(ref r, v.Value, p.Value, domDiffusionName, domDiffusionXorName);
        }

        private void InitOrClearDiffusionValue(RichTextBox rtb, uint? diffusionStateValue, uint? stateValue)
        {
            if (diffusionStateValue != null && stateValue != null)
            {
                Plugins.ChaCha.ViewModel.Components.Diffusion.InitDiffusionValue(rtb, (uint)diffusionStateValue,
                    (uint)stateValue);
            }
            else
            {
                rtb.Document.Blocks.Clear();
            }
        }

        private void InitOrClearXorValue(RichTextBox rtb, uint? diffusionValue, uint? stateValue)
        {
            if (diffusionValue != null && stateValue != null)
            {
                Plugins.ChaCha.ViewModel.Components.Diffusion.InitXORValue(rtb, (uint)diffusionValue,
                    (uint)stateValue);
            }
            else
            {
                rtb.Document.Blocks.Clear();
            }
        }

        #endregion Diffusion - Virtual DOM

        #region User Input

        private void InitKeystreamBlockInput()
        {
            TextBox keystreamBlockInput = (TextBox)FindName("KeystreamBlockInput");
            int maxKeystreamBlock = ViewModel.ChaCha.TotalKeystreamBlocks;
            Binding binding = new Binding("CurrentKeystreamBlockIndex")
            {
                Mode = BindingMode.TwoWay,
                Converter = new ZeroBasedIndexToOneBasedIndex(),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            };
            ActionViewBase.InitUserInputField(keystreamBlockInput, binding, 1, maxKeystreamBlock, ViewModel.KeystreamBlockInputHandler);
        }

        private void InitRoundInput()
        {
            TextBox roundInputTextBox = (TextBox)FindName("RoundInput");
            int maxRound = ViewModel.Settings.Rounds;
            Binding binding = new Binding("CurrentUserRoundIndex")
            {
                Mode = BindingMode.TwoWay,
                Converter = new ZeroBasedIndexToOneBasedIndex(),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            };
            ActionViewBase.InitUserInputField(roundInputTextBox, binding, 1, maxRound, ViewModel.RoundInputHandler);
        }

        private void InitQRInput()
        {
            TextBox qrInputTextBox = (TextBox)FindName("QRInput");
            Binding binding = new Binding("CurrentUserQRIndex")
            {
                Mode = BindingMode.TwoWay,
                Converter = new ZeroBasedIndexToOneBasedIndex(),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            };
            ActionViewBase.InitUserInputField(qrInputTextBox, binding, 1, 4, ViewModel.QRInputHandler);
        }

        #endregion User Input
    }
}