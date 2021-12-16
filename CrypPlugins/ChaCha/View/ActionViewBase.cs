using CrypTool.Plugins.ChaCha.Helper;
using CrypTool.Plugins.ChaCha.Helper.Validation;
using CrypTool.Plugins.ChaCha.ViewModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace CrypTool.Plugins.ChaCha.View
{
    /// <summary>
    /// Class with helper methods which all views whose view model implements IActionNavigation use.
    /// Implements the event handler for the slider and the action input textbox.
    /// </summary>
    internal static class ActionViewBase
    {
        /// <summary>
        /// Attaches the event handler from the view model to the slider and textbox which are inside a template.
        /// </summary>
        /// <param name="root">The ContentControl which has the slider and textbox in its template.</param>
        public static void AddEventHandlers(ActionViewModelBase viewModel, ContentControl root)
        {
            // --- ACTION SLIDER ---
            // Add value changed event handler to action slider
            root.ApplyTemplate();
            Slider actionSlider = (Slider)root.Template.FindName("ActionSlider", root);
            actionSlider.ValueChanged += viewModel.HandleActionSliderValueChange;

            // --- USER ACTION INPUT ---
            TextBox actionInputTextbox = (TextBox)root.Template.FindName("ActionInputTextBox", root);
            int maxAction = viewModel.TotalActions - 1;
            InitUserInputField(actionInputTextbox, "CurrentUserActionIndex", 0, maxAction, viewModel.HandleUserActionInput);
        }

        /// <summary>
        /// Initialize the TextBox to support user input including validation.
        /// It adds a two-way binding to the property at the specified path and adds a validation rule
        /// such that only the minimum and maximum value are valid.
        /// It also adds the given handler to the 'KeyDown' event and sets the maximum length
        /// to the amount of digits the maximum value has.
        /// (It was not possible to do this in pure XAML because the ValidationRule
        /// needs an argument and ValidationRule is not a DependencyObject thus no data binding available
        /// to pass in the argument.)
        /// </summary>
        /// <param name="tb">The TextBox we want to setup.</param>
        /// <param name="bindingPath">Path to property the TextBox should bind to.</param>
        /// <param name="min">Minimum valid user value.</param>
        /// <param name="max">Maximum valid user value.</param>
        /// <param name="handleUserInput">The function which handles the user input.</param>
        public static void InitUserInputField(TextBox tb, string bindingPath, int min, int max, KeyEventHandler handleUserInput)
        {
            Binding binding = new Binding(bindingPath)
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            InitUserInputField(tb, binding, min, max, handleUserInput);
        }

        /// <summary>
        /// Initialize the TextBox to support user input including validation.
        /// Wrapper method with predefined binding. Can be used to use converters with the binding.
        /// </summary>
        /// <param name="tb">The TextBox we want to setup.</param>
        /// <param name="binding">The binding we want to use with the Text property of the TextBox.</param>
        /// <param name="min">Minimum valid user value.</param>
        /// <param name="max">Maximum valid user value.</param>
        /// <param name="handleUserInput">The function which handles the user input.</param>
        public static void InitUserInputField(TextBox tb, Binding binding, int min, int max, KeyEventHandler handleUserInput)
        {
            tb.KeyDown += handleUserInput;
            binding.ValidationRules.Add(new UserInputValidationRule(min, max));
            tb.SetBinding(TextBox.TextProperty, binding);
            tb.MaxLength = Digits.GetAmountOfDigits(max);
        }

        /// <summary>
        /// Load the resource dictionaries for the current locale setting into the resources of the UserControl.
        /// </summary>
        public static void LoadLocaleResources(UserControl uc)
        {
            System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.CurrentCulture;
            if (culture.Name == "en-US")
            {
                uc.Resources.MergedDictionaries.Add(new ContentTemplate.Localization.en_US());
            }
            else if (culture.Name == "de-DE")
            {
                uc.Resources.MergedDictionaries.Add(new ContentTemplate.Localization.de_DE());
            }
        }
    }
}