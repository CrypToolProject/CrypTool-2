/*                              
   Copyright 2026 Marc Philipp Kray, CrypTool Project

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

/// <summary>
/// This file implements a WPF data binding converter mapping boolean logic states to 
/// UI element visibility states. It facilitates dynamic component rendering decoupled from code-behind logic.
/// </summary>

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CrypTool.CrypLLM.Converter
{
    /// <summary>
    /// Converts boolean values to corresponding <see cref="Visibility"/> enumeration values.
    /// Exposes configurable states to support diverse UI layouts (e.g., using Collapsed vs. Hidden).
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// The visibility state returned when the bound boolean evaluates to true.
        /// </summary>
        public Visibility TrueValue { get; set; } = Visibility.Visible;

        /// <summary>
        /// The visibility state returned when the bound boolean evaluates to false.
        /// </summary>
        public Visibility FalseValue { get; set; } = Visibility.Collapsed;

        /// <summary>
        /// Transforms a boolean data-bound value into a UI visibility state.
        /// </summary>
        /// <param name="value">The data-bound value, expected to be convertible to a boolean.</param>
        /// <param name="targetType">The expected target type, typically <see cref="Visibility"/>.</param>
        /// <param name="parameter">An optional converter parameter (unused).</param>
        /// <param name="culture">The regional culture context for value conversion.</param>
        /// <returns>The corresponding <see cref="Visibility"/> state based on the boolean evaluation.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag;
            try
            {
                // Ensures robustness against unexpected binding value types.
                flag = System.Convert.ToBoolean(value, culture);
            }
            catch
            {
                // Failsafe state fallback to prevent unhandled UI binding exceptions.
                flag = false;
            }

            return flag ? TrueValue : FalseValue;
        }

        /// <summary>
        /// Resolves the reverse transformation from a UI visibility state back to a boolean data model.
        /// </summary>
        /// <param name="value">The current <see cref="Visibility"/> state provided by the UI element.</param>
        /// <param name="targetType">The expected target type (must be boolean).</param>
        /// <param name="parameter">An optional converter parameter (unused).</param>
        /// <param name="culture">The regional culture context for value conversion.</param>
        /// <returns>True if the visibility matches <see cref="TrueValue"/>; otherwise, false.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the binding mapping improperly targets a non-boolean.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool))
            {
                throw new InvalidOperationException("Target type must be bool.");
            }

            if (Equals(value, TrueValue))
            {
                return true;
            }

            if (Equals(value, FalseValue))
            {
                return false;
            }

            // Fallback assumption resolves ambiguous or unconfigured mapping states to false.
            return false;
        }
    }
}