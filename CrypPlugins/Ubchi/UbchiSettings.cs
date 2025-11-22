/*
   Copyright CrypTool 2 Team <ct2contact@cryptool.org>
   Licensed under the Apache License, Version 2.0
*/
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System.ComponentModel;

namespace CrypTool.Plugins.Ubchi
{
    /// <summary>
    /// Enumeration for cipher action - Encrypt or Decrypt
    /// </summary>
    public enum UbchiAction { Encrypt = 0, Decrypt = 1 }

    /// <summary>
    /// Settings class for the Übchi cipher plugin
    /// Provides user-configurable options in the CrypTool task pane
    /// </summary>
    public class UbchiSettings : ISettings
    {
        #region Private Fields

        private UbchiAction action = UbchiAction.Encrypt;
        private bool useDualKey = false;
        private bool useCustomKeyLength = false;

        #endregion

        #region Public Properties (Task Pane Settings)

        /// <summary>
        /// Determines whether to encrypt or decrypt the input text
        /// Default: Encrypt
        /// </summary>
        [TaskPane("Action", "Choose whether to encrypt or decrypt the text", null, 1, false, ControlType.ComboBox, new string[] { "Encrypt", "Decrypt" })]
        public UbchiAction Action
        {
            get => action;
            set { if (action != value) { action = value; OnPropertyChanged("Action"); } }
        }

        /// <summary>
        /// Enables dual-key mode: use Key 1 for first transposition and Key 2 for second transposition
        /// When disabled, Key 1 is used for both transpositions
        /// Default: false (single key mode)
        /// Requires Key 2 input when enabled
        /// </summary>
        [TaskPane("Use Dual Key", "Use different keys for first and second transposition (Key 2 required)", null, 2, false, ControlType.CheckBox)]
        public bool UseDualKey
        {
            get => useDualKey;
            set { if (useDualKey != value) { useDualKey = value; OnPropertyChanged("UseDualKey"); } }
        }

        /// <summary>
        /// Allows keys of any length when checked
        /// When unchecked, keys must be 12-30 characters long (historical cipher constraint)
        /// Default: false (enforces 12-30 character limit)
        /// </summary>
        [TaskPane("Allow Custom Key Length", "Allow keys of any length (if unchecked, requires 12-30 characters)", null, 3, false, ControlType.CheckBox)]
        public bool UseCustomKeyLength
        {
            get => useCustomKeyLength;
            set { if (useCustomKeyLength != value) { useCustomKeyLength = value; OnPropertyChanged("UseCustomKeyLength"); } }
        }

        #endregion

        #region ISettings Implementation

        public event PropertyChangedEventHandler PropertyChanged;
        
        /// <summary>
        /// Notifies that a setting property has changed
        /// Triggers UI updates in CrypTool
        /// </summary>
        private void OnPropertyChanged(string propertyName) => EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        
        /// <summary>
        /// Called when the plugin is initialized
        /// No initialization needed for these settings
        /// </summary>
        public void Initialize() { }

        #endregion
    }
}