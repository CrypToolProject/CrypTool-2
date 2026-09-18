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
/// This file provides conversion utilities to map raw configuration strings 
/// to the internal LLM provider enumeration. It ensures safe deserialization 
/// and enforces structural fallbacks to prevent runtime errors caused by malformed settings.
/// </summary>

using CrypTool.CrypLLM.Helper;
using System;

namespace CrypTool.CrypLLM.Converter
{
    /// <summary>
    /// Utility class facilitating bi-directional conversion between the <see cref="SupportedProviders"/> 
    /// enumeration and string representations. Essential for mapping user settings or external configurations.
    /// </summary>
    public class ProviderToStringConverter
    {
        /// <summary>
        /// Parses a raw string input into a <see cref="SupportedProviders"/> enum value.
        /// Incorporates fault tolerance to ensure a valid baseline provider is always returned,
        /// even under conditions involving missing or corrupted configuration data.
        /// </summary>
        /// <param name="raw">The raw string identifier for the provider, typically from dynamic textual configurations.</param>
        /// <returns>The corresponding provider enum, defaulting to OpenAI upon parsing failure.</returns>
        public static SupportedProviders Convert(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return SupportedProviders.OpenAI;
            }

            SupportedProviders result;
            if (Enum.TryParse(raw, ignoreCase: true, out result))
            {
                return result;
            }

            // Fallback assumption: To prevent systematic failures on corrupted, outdated, or 
            // unrecognized configuration inputs, the algorithm unconditionally falls back to the primary operational provider.
            return SupportedProviders.OpenAI;
        }

        /// <summary>
        /// Serializes the provider enumeration into its nominal string representation.
        /// Employed to write internal system states back into textual configurations or propagate it to UI string bindings.
        /// </summary>
        /// <param name="provider">The active provider state object.</param>
        /// <returns>The strict textual equivalent of the provider identity.</returns>
        public static string Convert(SupportedProviders provider)
        {
            return provider.ToString();
        }
    }
}
