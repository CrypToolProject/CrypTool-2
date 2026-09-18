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
/// This file implements the InstructionEntry data model, representing individual configuration 
/// nodes for the LLM systemic instructions. By isolating default references from active modifications, 
/// the agent's behavior can be customized by users while maintaining a reproducible factory reset state.
/// </summary>

using Newtonsoft.Json;
using System;

namespace CrypTool.CrypLLM.AgentInstructions
{
    /// <summary>
    /// Represents a single configurable instruction entry used by the LLM agent.
    /// 
    /// Each entry has:
    /// - a unique <see cref="Key"/> used by the code and persistence layer,
    /// - a flag indicating whether it is user-editable (<see cref="IsUserAdjustable"/>),
    /// - a built-in default text (immutable, see <see cref="_defaultText"/>),
    /// - and the current editable text (<see cref="Text"/>).
    ///
    /// Default content is provided and managed by <see cref="InstructionCatalog"/>.
    /// </summary>
    public sealed class InstructionEntry
    {
        /// <summary>
        /// Unique key used by the application logic and persistence to identify this entry,
        /// e.g. "Default", "GermanUser" or similar identifiers.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Human-readable label for GUI representation. If omitted, falls back to the <see cref="Key"/>.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Indicates whether this entry can be edited by the user in the UI.
        /// If <c>false</c>, the UI should present the text as read-only.
        /// </summary>
        public bool IsUserAdjustable { get; set; }

        /// <summary>
        /// Built-in default text associated with this instruction.
        ///
        /// This value:
        /// - is immutable after construction,
        /// - is used to initialize new instances,
        /// - can be reapplied via <see cref="LoadDefaultText"/> to reset user changes,
        /// - acts as the authoritative reference text for this entry.
        /// </summary>
        private readonly string _defaultText;

        private string _text;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstructionEntry"/> class.
        /// </summary>
        /// <param name="key">
        /// The unique key for this instruction entry. Must not be reused for other entries.
        /// </param>
        /// <param name="isUserAdjustable">
        /// Specifies whether the user is allowed to modify the <see cref="Text"/> in the UI.
        /// </param>
        /// <param name="defaultText">
        /// The built-in default text. If <c>null</c> is passed, it is normalized to <see cref="string.Empty"/>.
        /// </param>
        /// <param name="text">
        /// Optional initial value for <see cref="Text"/> (e.g. a previously persisted value).
        /// If <c>null</c>, the entry is created with an empty editable text.
        /// </param>
        public InstructionEntry(string key, bool isUserAdjustable, string defaultText, string text = null)
            : this(key, key, isUserAdjustable, defaultText, text)
        {
        }

        [JsonConstructor]
        public InstructionEntry(string key, string name, bool isUserAdjustable, string defaultText, string text = null)
        {
            Key = key;
            Name = string.IsNullOrWhiteSpace(name) ? key : name;
            IsUserAdjustable = isUserAdjustable;
            _defaultText = defaultText ?? string.Empty;
            _text = text;
        }

        /// <summary>
        /// Computes a composite display label for configuration UI views, prefixing specialized entries
        /// (e.g., 'UiText_') to delineate Tool UI strings from core AI directives.
        /// </summary>
        public string DisplayName
        {
            get
            {
                // Fallback resolution for structural safety.
                string label = string.IsNullOrWhiteSpace(Name) ? Key : Name;

                // Explicit metadata prefix formatting for internal tool-text categories.
                if (!string.IsNullOrWhiteSpace(Key) && Key.StartsWith("UiText_", StringComparison.OrdinalIgnoreCase))
                {
                    return $"Tool(text_ui) -> {label}";
                }

                return string.Equals(Key, label, StringComparison.OrdinalIgnoreCase)
                    ? label
                    : $"{label} ({Key})";
            }
        }

        /// <summary>
        /// Gets or sets the current user-adjustable text for this instruction.
        ///
        /// Semantics:
        /// - Intentionally empty text is represented by <see cref="string.Empty"/>.
        /// - If the setter receives <c>null</c> or a value containing only whitespace,
        ///   the value is normalized to <see cref="string.Empty"/>.
        /// - Non-empty values are stored trimmed (leading and trailing whitespace removed).
        /// </summary>
        public string Text
        {
            get { return _text; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _text = string.Empty;
                }
                else
                {
                    _text = value.Trim();
                }
            }
        }

        /// <summary>
        /// Computes equality between the active modified state and the authoritative default text constraint. 
        /// Assists the UI in rendering indicators (e.g., reset buttons) to distinguish altered states.
        /// </summary>
        public bool IsUsingDefaultText
        {
            get
            {
                // Tolerates whitespace anomalies during state matching context.
                string current = string.IsNullOrWhiteSpace(Text) ? string.Empty : Text.Trim();
                string defaultText = string.IsNullOrWhiteSpace(_defaultText) ? string.Empty : _defaultText.Trim();
                return string.Equals(current, defaultText, StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// Replaces the current <see cref="Text"/> with the built-in default text.
        ///
        /// This is typically used when the user requests a reset to factory settings
        /// or when repairing inconsistent persisted data.
        /// </summary>
        /// <returns>
        /// The applied default text (same as the new value of <see cref="Text"/>).
        /// </returns>
        public string LoadDefaultText()
        {
            Text = _defaultText;
            return Text;
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="InstructionEntry"/>.
        ///
        /// The clone:
        /// - has the same <see cref="Key"/>,
        /// - has the same <see cref="IsUserAdjustable"/> state,
        /// - shares the same built-in default text,
        /// - copies the current <see cref="Text"/> value.
        /// </summary>
        /// <returns>A new <see cref="InstructionEntry"/> with the same data.</returns>
        public InstructionEntry Clone()
        {
            return new InstructionEntry(Key, Name, IsUserAdjustable, _defaultText, Text);
        }
    }
}
