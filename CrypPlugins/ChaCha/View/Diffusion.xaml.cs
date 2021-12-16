using CrypTool.Plugins.ChaCha.Helper;
using CrypTool.Plugins.ChaCha.Helper.Converter;
using CrypTool.Plugins.ChaCha.Helper.Validation;
using CrypTool.Plugins.ChaCha.ViewModel;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace CrypTool.Plugins.ChaCha.View
{
    /// <summary>
    /// Interaction logic for Diffusion.xaml
    /// </summary>
    [PluginBase.Attributes.Localization("CrypTool.Plugins.ChaCha.Properties.Resources")]
    public partial class Diffusion : UserControl
    {
        public Diffusion()
        {
            InitializeComponent();
            ActionViewBase.LoadLocaleResources(this);
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DiffusionViewModel ViewModel = (DiffusionViewModel)e.NewValue;
            if (ViewModel != null)
            {
                Version v = ViewModel.Settings.Version;

                InputDiffusionKeyExplicit.Text = Formatter.HexString(ViewModel.ChaCha.InputKey);
                int keyLength = ViewModel.ChaCha.InputKey.Length;
                ValidationRule keyRule = new DiffusionInputValidationRule(keyLength);
                IValueConverter keyConverter = new DiffusionBytesConverter(keyLength);
                InitDiffusionInputField(InputDiffusionKeyExplicit, keyRule, keyConverter, "DiffusionKeyExplicit", ViewModel.DiffusionKeyExplicitInputHandler(keyRule));
                InitDiffusionInputField(InputDiffusionKeyXOR, keyRule, keyConverter, "DiffusionKeyXOR", ViewModel.DiffusionKeyXORInputHandler(keyRule));

                InputDiffusionIVExplicit.Text = Formatter.HexString(ViewModel.ChaCha.InputIV);
                int ivLength = ViewModel.ChaCha.InputIV.Length;
                ValidationRule ivRule = new DiffusionInputValidationRule(ivLength);
                IValueConverter ivConverter = new DiffusionBytesConverter(ivLength);
                InitDiffusionInputField(InputDiffusionIVExplicit, ivRule, ivConverter, "DiffusionIVExplicit", ViewModel.DiffusionIVExplicitInputHandler(ivRule));
                InitDiffusionInputField(InputDiffusionIVXOR, ivRule, ivConverter, "DiffusionIVXOR", ViewModel.DiffusionIVXORInputHandler(ivRule));

                BigInteger initialCounter = ViewModel.ChaCha.InitialCounter;
                InputDiffusionInitialCounterExplicit.Text = Formatter.HexString(v == Version.DJB ? (ulong)initialCounter : (uint)initialCounter);
                int counterLength = (int)v.CounterBits / 8;
                ValidationRule counterRule = new DiffusionInputValidationRule(counterLength);
                IValueConverter counterConverter = new DiffusionCounterConverter(counterLength);
                InitDiffusionInputField(InputDiffusionInitialCounterExplicit, counterRule, counterConverter, "DiffusionInitialCounterExplicit", ViewModel.DiffusionInitialCounterExplicitInputHandler(counterRule));
                InitDiffusionInputField(InputDiffusionInitialCounterXOR, counterRule, counterConverter, "DiffusionInitialCounterXOR", ViewModel.DiffusionInitialCounterXORInputHandler(counterRule));
            }
        }

        private void InitDiffusionInputField(TextBox inputField, ValidationRule validationRule, IValueConverter converter, string bindingPath)
        {
            Binding binding = new Binding(bindingPath) { Mode = BindingMode.TwoWay, Converter = converter };
            binding.ValidationRules.Add(validationRule);
            inputField.SetBinding(TextBox.TextProperty, binding);
            EditingCommands.ToggleInsert.Execute(null, inputField);
        }

        private void InitDiffusionInputField(TextBox inputField, ValidationRule validationRule, IValueConverter converter, string bindingPath, TextChangedEventHandler handleUserInput)
        {
            InitDiffusionInputField(inputField, validationRule, converter, bindingPath);
            inputField.TextChanged += handleUserInput;
        }
    }
}