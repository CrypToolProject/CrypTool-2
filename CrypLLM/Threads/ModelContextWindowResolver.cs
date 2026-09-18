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
using CrypTool.CrypLLM.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CrypTool.CrypLLM.Threads
{
    /// <summary>
    /// Module-level resolver for model-specific context-window limits.
    /// </summary>
    /// <remarks>
    /// This component centralizes lookup and persistence of token-window metadata used by
    /// runtime budgeting. Values are read from two sources: developer defaults (shipped with
    /// the plugin) and user overrides (stored in local settings). Overrides take precedence.
    /// The accepted mapping format is line-based <c>modelId=tokens</c>, where malformed lines
    /// are ignored to keep startup resilient against manual configuration errors.
    /// </remarks>
    /// <summary>
    /// Resolves context-window limits for model ids.
    /// User values override developer defaults.
    /// </summary>
    internal static class ModelContextWindowResolver
    {
        private const char PairSeparator = '=';

        private static readonly Lazy<Dictionary<string, int>> DeveloperModelContextWindows =
            new Lazy<Dictionary<string, int>>(() => ParseModelMap(Settings.Default.developerModelContextWindows), isThreadSafe: true);

        /// <summary>
        /// Resolves the effective context-window size for a model id.
        /// </summary>
        /// <param name="modelId">Model identifier as selected in the runtime configuration.</param>
        /// <param name="contextWindowTokens">Resolved context-window size in tokens when available.</param>
        /// <returns>
        /// <see langword="true"/> if a valid mapping exists; otherwise <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// Resolution order is deterministic: user settings first, then developer defaults.
        /// </remarks>
        public static bool TryResolveContextWindowTokens(string modelId, out int contextWindowTokens)
        {
            // User-configured values should override developer defaults for the same model id.
            return TryGetUserConfiguredContextWindowTokens(modelId, out contextWindowTokens) ||
                   TryGetDeveloperContextWindowTokens(modelId, out contextWindowTokens);
        }

        /// <summary>
        /// Attempts to resolve context-window tokens from developer-provided defaults.
        /// </summary>
        /// <param name="modelId">Model identifier to resolve.</param>
        /// <param name="contextWindowTokens">Resolved token window if present and valid.</param>
        /// <returns>
        /// <see langword="true"/> when a positive mapping exists in developer defaults.
        /// </returns>
        public static bool TryGetDeveloperContextWindowTokens(string modelId, out int contextWindowTokens)
        {
            contextWindowTokens = 0;
            string normalizedModelId = NormalizeModelId(modelId);
            if (string.IsNullOrWhiteSpace(normalizedModelId))
            {
                return false;
            }

            return DeveloperModelContextWindows.Value.TryGetValue(normalizedModelId, out contextWindowTokens) && contextWindowTokens > 0;
        }

        /// <summary>
        /// Attempts to resolve context-window tokens from user-specific overrides.
        /// </summary>
        /// <param name="modelId">Model identifier to resolve.</param>
        /// <param name="contextWindowTokens">Resolved token window if present and valid.</param>
        /// <returns>
        /// <see langword="true"/> when a positive mapping exists in user settings.
        /// </returns>
        public static bool TryGetUserConfiguredContextWindowTokens(string modelId, out int contextWindowTokens)
        {
            contextWindowTokens = 0;
            string normalizedModelId = NormalizeModelId(modelId);
            if (string.IsNullOrWhiteSpace(normalizedModelId))
            {
                return false;
            }

            Dictionary<string, int> userMap = ParseUserMap(Settings.Default.userModelContextWindows);
            return userMap.TryGetValue(normalizedModelId, out contextWindowTokens) && contextWindowTokens > 0;
        }

        /// <summary>
        /// Persists or updates a user override for a model-specific context window.
        /// </summary>
        /// <param name="modelId">Model identifier to update.</param>
        /// <param name="contextWindowTokens">Token window to persist.</param>
        /// <remarks>
        /// Values below 4096 are rejected to avoid unrealistic or accidental low-budget
        /// configurations. The settings store is only written when a semantic change occurred.
        /// </remarks>
        public static void SetUserConfiguredContextWindowTokens(string modelId, int contextWindowTokens)
        {
            // Guard against empty model ids and values below the minimum supported window.
            string normalizedModelId = NormalizeModelId(modelId);
            if (string.IsNullOrWhiteSpace(normalizedModelId) || contextWindowTokens < 4096)
            {
                return;
            }

            // Merge/update the override map and serialize into canonical storage format.
            Dictionary<string, int> userMap = ParseUserMap(Settings.Default.userModelContextWindows);
            userMap[normalizedModelId] = contextWindowTokens;

            string serialized = SerializeUserMap(userMap);
            // Avoid unnecessary disk writes and settings churn when no effective change exists.
            if (string.Equals(Settings.Default.userModelContextWindows ?? string.Empty, serialized, StringComparison.Ordinal))
            {
                return;
            }

            Settings.Default.userModelContextWindows = serialized;
            Settings.Default.Save();

            // Log both override and baseline context to support reproducibility in diagnostics.
            if (TryGetDeveloperContextWindowTokens(normalizedModelId, out int developerTokens))
            {
                Log.Info($"Model context window override saved: model={normalizedModelId}, userTokens={contextWindowTokens}, developerDefaultTokens={developerTokens}");
            }
            else
            {
                Log.Info($"Model context window saved: model={normalizedModelId}, userTokens={contextWindowTokens}");
            }
        }

        private static Dictionary<string, int> ParseUserMap(string raw)
        {
            return ParseModelMap(raw);
        }

        /// <summary>
        /// Parses a line-based model-to-token mapping.
        /// </summary>
        /// <param name="raw">Raw mapping text from settings.</param>
        /// <returns>
        /// A normalized dictionary keyed by model id (case-insensitive).
        /// </returns>
        /// <remarks>
        /// Expected line syntax is <c>modelId=tokens</c>. Empty lines, comment lines beginning
        /// with <c>#</c>, malformed pairs, non-integer values, and values below 4096 are ignored.
        /// For duplicate model ids, the last valid occurrence wins.
        /// </remarks>
        private static Dictionary<string, int> ParseModelMap(string raw)
        {
            Dictionary<string, int> map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return map;
            }

            // Parse line-wise so partial corruption does not invalidate the full mapping.
            string[] lines = raw.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                if (trimmed.Length == 0 || trimmed.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                int separatorIndex = trimmed.IndexOf(PairSeparator);
                if (separatorIndex <= 0 || separatorIndex >= trimmed.Length - 1)
                {
                    continue;
                }

                // Normalize key/value pair and accept only meaningful token budgets.
                string modelId = NormalizeModelId(trimmed.Substring(0, separatorIndex));
                string tokenRaw = trimmed.Substring(separatorIndex + 1).Trim();
                if (string.IsNullOrWhiteSpace(modelId))
                {
                    continue;
                }

                if (!int.TryParse(tokenRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int tokens) || tokens < 4096)
                {
                    continue;
                }

                map[modelId] = tokens;
            }

            return map;
        }

        /// <summary>
        /// Serializes user overrides into deterministic storage format.
        /// </summary>
        /// <param name="map">Mapping to serialize.</param>
        /// <returns>
        /// Line-based <c>modelId=tokens</c> text with ordinal-ignore-case ordering.
        /// </returns>
        /// <remarks>
        /// Deterministic ordering minimizes noisy settings diffs and improves reproducibility.
        /// </remarks>
        private static string SerializeUserMap(Dictionary<string, int> map)
        {
            if (map == null || map.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, int> pair in map.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(pair.Key) || pair.Value <= 0)
                {
                    continue;
                }

                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }

                sb.Append(pair.Key);
                sb.Append(PairSeparator);
                sb.Append(pair.Value.ToString(CultureInfo.InvariantCulture));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Normalizes model identifiers used as mapping keys.
        /// </summary>
        /// <param name="modelId">Raw model id as provided by caller or settings.</param>
        /// <returns>A trimmed identifier, or an empty string for null/whitespace input.</returns>
        private static string NormalizeModelId(string modelId)
        {
            return string.IsNullOrWhiteSpace(modelId) ? string.Empty : modelId.Trim();
        }
    }
}
