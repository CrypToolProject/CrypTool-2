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
/// This file implements the InstructionCatalog, responsible for loading and supplying 
/// the default systemic prompt instructions for the LLM agent. 
/// Using embedded JSON resource extraction ensures robustness across deployment environments.
/// </summary>

using CrypTool.PluginBase.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CrypTool.CrypLLM.AgentInstructions
{
    /// <summary>
    /// Provides all built‑in default instruction entries.
    /// This catalog is used to initialize or repair the JSON storage file.
    /// </summary>
    internal static class InstructionCatalog
    {
        /// <summary>
        /// Gets the list of all built‑in instruction entries.
        /// The caller should treat this as read‑only and not modify its contents.
        /// </summary>
        public static IReadOnlyList<InstructionEntry> AllEntrys => _allEntrys.Value;

        private static readonly Lazy<IReadOnlyList<InstructionEntry>> _allEntrys =
            new Lazy<IReadOnlyList<InstructionEntry>>(LoadFromEmbeddedJson, true);

        /// <summary>
        /// Reads instruction parameters from a JSON configuration.
        /// Prioritizes extracting from an embedded resource to limit filesystem dependency failures.
        /// If extraction fails, attempts a secondary fallback directory read.
        /// </summary>
        /// <returns>A deterministically sorted read-only collection of agent instructions, or an empty array on structural failure.</returns>
        private static IReadOnlyList<InstructionEntry> LoadFromEmbeddedJson()
        {
            try
            {
                // Prefer embedded resource (robust for deployment)
                Assembly assembly = typeof(InstructionCatalog).Assembly;
                string resourceName = assembly
                    .GetManifestResourceNames()
                    .FirstOrDefault(n => n.EndsWith("Instructions.json", StringComparison.OrdinalIgnoreCase));

                string json;
                if (!string.IsNullOrEmpty(resourceName))
                {
                    using Stream stream = assembly.GetManifestResourceStream(resourceName);
                    using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                    json = reader.ReadToEnd();
                }
                else
                {
                    // Fallback: load from file next to assembly / base directory
                    string baseDir = DirectoryHelper.BaseDirectory;
                    string path = Path.Combine(baseDir, "CrypLLM", "AgentInstructions", "Instructions.json");
                    json = File.ReadAllText(path, Encoding.UTF8);
                }

                // Deserializes strict instructions mapping; prevents unpredictable behavior if JSON schema is breached.
                InstructionsJsonRoot root = JsonConvert.DeserializeObject<InstructionsJsonRoot>(json);
                if (root?.Instructions == null || root.Instructions.Count == 0)
                {
                    return Array.Empty<InstructionEntry>();
                }

                // Deterministic order: by key (ordinal comparison) ensuring behavior continuity between sessions.
                return root.Instructions
                    .OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase)
                    .Select(kvp => new InstructionEntry(
                        kvp.Key,
                        string.IsNullOrWhiteSpace(kvp.Value.Name) ? kvp.Key : kvp.Value.Name,
                        !kvp.Value.Mandatory,
                        kvp.Value.Text ?? string.Empty))
                    .ToList();
            }
            catch
            {
                // Silently swallow exceptions to prevent application startup from crashing. Returns an empty catalog.
                return Array.Empty<InstructionEntry>();
            }
        }

        /// <summary>
        /// Data structure mapping for JSON deserialization representation.
        /// </summary>
        private sealed class InstructionsJsonRoot
        {
            public int InstructionsVersion { get; set; }
            public string Description { get; set; }
            public Dictionary<string, InstructionsJsonEntry> Instructions { get; set; }
        }

        /// <summary>
        /// Represents a singular systemic target setting parsed from configuration.
        /// </summary>
        private sealed class InstructionsJsonEntry
        {
            public string Name { get; set; }
            public string Text { get; set; }
            public bool Mandatory { get; set; }
        }
    }
}