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
/// This file implements the InstructionManager singleton, which coordinates the persistence 
/// lifecycle of LLM instructional directives. It merges authoritative systemic catalogs 
/// with local user adaptations into a unified runtime dictionary.
/// </summary>

using CrypTool.PluginBase.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CrypTool.CrypLLM.AgentInstructions
{
    /// <summary>
    /// Manages loading, saving and accessing persisted agent instructions.
    /// </summary>
    internal sealed class InstructionManager
    {
        /// <summary>
        /// Singleton instance (lazy, thread-safe).
        /// </summary>
        private static readonly Lazy<InstructionManager> _instance =
            new(() => new InstructionManager());

        /// <summary>
        /// Full path of the JSON file storing all instructions.
        /// </summary>
        private readonly string _instructionsPath;

        /// <summary>
        /// Backing list for <see cref="Entries"/>.
        /// </summary>
        private List<InstructionEntry> _entries = new();

        /// <summary>
        /// Global singleton instance.
        /// </summary>
        public static InstructionManager Instance => _instance.Value;

        /// <summary>
        /// Current in-memory instruction entries.
        /// </summary>
        public IReadOnlyList<InstructionEntry> Entries => _entries;

        /// <summary>
        /// Sets up storage path and loads or initializes instructions.
        /// </summary>
        private InstructionManager()
        {
            string dir = Path.Combine(DirectoryHelper.DirectoryLocal, "LLM");
            Directory.CreateDirectory(dir);
            _instructionsPath = Path.Combine(dir, "AgentInstructions.json");

            LoadOrInitialize();
        }

        /// <summary>
        /// Saves all entries to disk as JSON.
        /// </summary>
        public void SaveToDisk()
        {
            try
            {
                if (_entries == null)
                {
                    return;
                }

                string json = JsonConvert.SerializeObject(_entries, Formatting.Indented);
                File.WriteAllText(_instructionsPath, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Log.Warning($"Saving AgentInstructions.json failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Returns the entry for the given key (case-insensitive).
        /// Uses "Default" when key is empty.
        /// </summary>
        private InstructionEntry GetEntry(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                key = "Default";
            }

            InstructionEntry entry = _entries
                .FirstOrDefault(e => string.Equals(e.Key, key, StringComparison.OrdinalIgnoreCase));

            if (entry != null)
            {
                return entry;
            }

            return null;
        }

        /// <summary>
        /// Returns all known instruction keys.
        /// </summary>
        public IEnumerable<string> GetAllKeys()
        {
            if (_entries == null || _entries.Count == 0)
            {
                LoadOrInitialize();
            }

            return _entries.Select(e => e.Key).ToArray();
        }

        /// <summary>
        /// Loads entries from disk or falls back to defaults.
        /// </summary>
        private void LoadOrInitialize()
        {
            List<InstructionEntry> entryListFromFile = null;
            try
            {
                if (!File.Exists(_instructionsPath))
                {
                    // A new profile still needs the embedded defaults before its first request.
                    LoadFromSaveFile();
                    return;
                }

                string json = File.ReadAllText(_instructionsPath, Encoding.UTF8);
                entryListFromFile = JsonConvert.DeserializeObject<List<InstructionEntry>>(json);
            }
            catch (Exception ex)
            {
                Log.Warning($"Loading AgentInstructions.json failed: {ex.Message}. Reinitializing from defaults.");
            }

            LoadFromSaveFile(entryListFromFile);
        }

        /// <summary>
        /// Hybridizes default catalog definitions alongside historically stored local alterations.
        /// Unrecognized or obsolete entries from the disk cache are safely ignored, ensuring strict adherence to the authoritative schema.
        /// </summary>
        /// <param name="entryListFromFile">Previously persisted JSON structures.</param>
        private void LoadFromSaveFile(List<InstructionEntry> entryListFromFile = null)
        {
            // Retain absolute schema compliance by projecting solely from the embedded catalog.
            List<InstructionEntry> entryCatalog = InstructionCatalog.AllEntrys.ToList();
            List<InstructionEntry> newList = new();

            foreach (InstructionEntry catalogEntry in entryCatalog)
            {
                // Attempt to match local overrides structurally against the catalog reference point.
                InstructionEntry entryFromFile = entryListFromFile
                    ?.FirstOrDefault(e => string.Equals(e.Key, catalogEntry.Key, StringComparison.OrdinalIgnoreCase));


                InstructionEntry newEntry = catalogEntry.Clone();
                if (entryFromFile == null)
                {
                    newEntry.LoadDefaultText();
                }
                else if (entryFromFile.Text != null)
                {
                    newEntry.Text = entryFromFile.Text;
                }
                else
                {
                    newEntry = catalogEntry.Clone();
                }

                newList.Add(newEntry);
            }

            _entries = newList;
            SaveToDisk();
        }

        /// <summary>
        /// Extracts and safely truncates instructional guidance corresponding to a query taxonomy context.
        /// Neutralizes disruptive formatting structures (escaped characters/line breaks) native to JSON.
        /// </summary>
        /// <param name="key">Unique systemic prompt key to deploy.</param>
        /// <returns>A formatted, condensed directive ready to be interpreted by Semantic Kernel configurations.</returns>
        public string GetInstructionText(string key)
        {
            InstructionEntry entry = GetEntry(key);
            if (entry == null)
            {
                return string.Empty;
            }

            // Text may intentionally be empty and must remain empty to signal intentional passiveness.
            string effective = entry.Text ?? string.Empty;

            if (string.IsNullOrEmpty(effective))
            {
                return string.Empty;
            }

            effective = effective.Trim();

            // Neutralize erratic escapements across heterogeneous JSON deserialization engines.
            effective = effective
                .Replace("\\r\\n", "\n")
                .Replace("\\n", "\n")
                .Replace("\\t", "\t")
                .Replace("\r\n", "\n")
                .Replace("\r", "\n");

            // Condense directives into unified block logic string required by certain underlying Chat completion APIs.
            effective = effective.Replace("\n", string.Empty);

            return effective;
        }

        /// <summary>
        /// Updates text of a user-adjustable entry and saves it.
        /// </summary>
        public void UpdateEntryText(string key, string newText)
        {
            InstructionEntry entry = _entries
                .FirstOrDefault(e => string.Equals(e.Key, key, StringComparison.OrdinalIgnoreCase));

            if (entry == null)
            {
                return;
            }

            if (!entry.IsUserAdjustable)
            {
                return;
            }

            entry.Text = newText ?? string.Empty;
            SaveToDisk();
        }

        /// <summary>
        /// Full path of the backing JSON file.
        /// </summary>
        public string InstructionsPath => _instructionsPath;
    }
}
