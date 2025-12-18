/*
   Copyright 2025 Adrián Lutišan 

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
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using CrypTool.Plugins.Ubchi.Properties;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Linq;

namespace CrypTool.Plugins.Ubchi
{
    /// <summary>
    /// Main plugin class implementing the Übchi cipher - a German WWI double columnar transposition cipher
    /// with frequency-based null characters inserted between transpositions
    /// </summary>
    [Author("Adrián Lutišan", "adrian.lutisan@gmail.com", "Institute of Computer Science and Mathematics, Faculty of Electrical Engineering and Information Technology, Slovak University of Technology", "https://uim.fei.stuba.sk")]
    [PluginInfo("CrypTool.Plugins.Ubchi.Properties.Resources", "Ubchi_PluginCaption", "Ubchi_PluginTooltip", "Ubchi/userdoc.xml", new[] { "Ubchi/Images/ubchi.png" })]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class Ubchi : ICrypComponent
    {
        #region Private Fields

        private readonly UbchiSettings _settings = new UbchiSettings();
        private UbchiPresentation _presentation;

        // English letter frequencies for generating realistic null characters
        // Values represent percentage frequency of each letter in English text
        private readonly Dictionary<char, double> englishFrequencies = new Dictionary<char, double>
        {
            {'A', 6.554}, {'B', 1.196}, {'C', 2.087}, {'D', 3.663}, {'E', 10.485},
            {'F', 1.918}, {'G', 1.566}, {'H', 5.505}, {'I', 5.420}, {'J', 0.109},
            {'K', 0.524}, {'L', 3.090}, {'M', 2.041}, {'N', 5.545}, {'O', 6.106},
            {'P', 1.359}, {'Q', 0.102}, {'R', 4.908}, {'S', 5.155}, {'T', 7.409},
            {'U', 2.254}, {'V', 0.816}, {'W', 1.869}, {'X', 0.132}, {'Y', 1.403},
            {'Z', 0.054}
        };

        private readonly Random random = new Random();

        #endregion

        #region Public Properties (Plugin Inputs/Outputs)

        /// <summary>
        /// Input text to be encrypted or decrypted
        /// </summary>
        [PropertyInfo(Direction.InputData, "Ubchi_InputText_Name", "Ubchi_InputText_Description")]
        public string InputText { get; set; }

        /// <summary>
        /// Primary encryption key - determines column permutation for first transposition
        /// </summary>
        [PropertyInfo(Direction.InputData, "Ubchi_Key1_Name", "Ubchi_Key1_Description")]
        public string Key1 { get; set; }

        /// <summary>
        /// Optional secondary key for dual-key mode - determines column permutation for second transposition
        /// </summary>
        [PropertyInfo(Direction.InputData, "Ubchi_Key2_Name", "Ubchi_Key2_Description", false)]
        public string Key2 { get; set; }

        /// <summary>
        /// Output text after encryption or decryption
        /// </summary>
        [PropertyInfo(Direction.OutputData, "Ubchi_OutputText_Name", "Ubchi_OutputText_Description")]
        public string OutputText { get; set; }

        #endregion

        #region Plugin Interface Implementation

        public ISettings Settings => _settings;

        public UserControl Presentation
        {
            get
            {
                if (_presentation == null)
                    _presentation = new UbchiPresentation();
                return _presentation;
            }
        }

        public void PreExecution() { }
        public void PostExecution() { }
        public void Stop() { }
        public void Initialize() { }
        public void Dispose() { }

        #endregion

        #region Main Execution Logic

        /// <summary>
        /// Main execution method - validates inputs and performs encryption or decryption
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 100);

            // Validate required inputs
            if (string.IsNullOrEmpty(InputText) || string.IsNullOrEmpty(Key1))
            {
                GuiLogMessage(Resources.Ubchi_Error_InputAndKeyRequired, NotificationLevel.Error);
                _presentation?.ClearPresentation();
                return;
            }

            try
            {
                // Validate key lengths based on settings
                if (!ValidateKeys())
                {
                    _presentation?.ClearPresentation();
                    return;
                }

                // Count words in keys to determine number of null characters
                int wordsInKey1 = CountWords(Key1);
                int wordsInKey2 = CountWords(Key2);
                int totalWords = _settings.UseDualKey && !string.IsNullOrEmpty(Key2) ? wordsInKey1 + wordsInKey2 : wordsInKey1;

                GuiLogMessage(string.Format(Resources.Ubchi_Log_ActionWordsKey1, GetLocalizedActionVerb(_settings.Action), wordsInKey1), NotificationLevel.Info);
                if (_settings.UseDualKey && !string.IsNullOrEmpty(Key2))
                {
                    GuiLogMessage(string.Format(Resources.Ubchi_Log_WordsKey2TotalNulls, wordsInKey2, totalWords), NotificationLevel.Info);
                }

                // Process the text (encrypt or decrypt)
                string result = ProcessText(InputText, Key1, Key2, totalWords,
                    _settings.Action == UbchiAction.Encrypt, _settings.UseDualKey);

                OutputText = result;
                OnPropertyChanged("OutputText");
                GuiLogMessage(string.Format(Resources.Ubchi_Log_Completed, GetLocalizedActionVerb(_settings.Action)), NotificationLevel.Info);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format(Resources.Ubchi_Log_ErrorPrefix, ex.Message), NotificationLevel.Error);
                _presentation?.ClearPresentation();
            }

            ProgressChanged(100, 100);
        }

        #endregion

        #region Key Validation

        /// <summary>
        /// Validates key lengths based on user settings
        /// Returns true if keys are valid, false otherwise
        /// </summary>
        private bool ValidateKeys()
        {
            // If custom key length is not allowed, enforce 12-30 character restriction
            if (!_settings.UseCustomKeyLength)
            {
                string cleanKey1 = Key1.Trim().Replace(" ", "");
                if (cleanKey1.Length < 12 || cleanKey1.Length > 30)
                {
                    GuiLogMessage(string.Format(Resources.Ubchi_Error_Key1Length, cleanKey1.Length, Resources.UbchiSettings_UseCustomKeyLength_Caption), NotificationLevel.Error);
                    return false;
                }

                if (_settings.UseDualKey && !string.IsNullOrEmpty(Key2))
                {
                    string cleanKey2 = Key2.Trim().Replace(" ", "");
                    if (cleanKey2.Length < 12 || cleanKey2.Length > 30)
                    {
                        GuiLogMessage(string.Format(Resources.Ubchi_Error_Key2Length, cleanKey2.Length, Resources.UbchiSettings_UseCustomKeyLength_Caption), NotificationLevel.Error);
                        return false;
                    }
                }
            }

            // Always check for empty keys
            if (string.IsNullOrWhiteSpace(Key1.Replace(" ", "")))
            {
                GuiLogMessage(Resources.Ubchi_Error_Key1Empty, NotificationLevel.Error);
                return false;
            }

            if (_settings.UseDualKey && string.IsNullOrWhiteSpace(Key2?.Replace(" ", "")))
            {
                GuiLogMessage(Resources.Ubchi_Error_Key2Required, NotificationLevel.Error);
                return false;
            }

            return true;
        }

        #endregion

        #region Main Processing Methods

        /// <summary>
        /// Main text processing method - orchestrates encryption or decryption
        /// Prepares keys and text, then calls appropriate encryption/decryption method
        /// </summary>
        private string ProcessText(string text, string key1, string key2, int nullCount, bool encrypt, bool dualKey)
        {
            // Clean text (remove non-alphabetic characters, convert to uppercase)
            string cleanText = PrepareText(text);
            string cleanKey1 = key1.Trim().Replace(" ", "").ToUpper();
            string cleanKey2 = dualKey && !string.IsNullOrEmpty(key2) ? key2.Trim().Replace(" ", "").ToUpper() : "";

            string result;

            if (encrypt)
            {
                // Choose single or dual key encryption
                result = dualKey && !string.IsNullOrEmpty(cleanKey2)
                    ? EncryptDual(cleanText, cleanKey1, cleanKey2, nullCount, key1, key2)
                    : EncryptSingle(cleanText, cleanKey1, nullCount, key1);
            }
            else
            {
                // Choose single or dual key decryption
                result = dualKey && !string.IsNullOrEmpty(cleanKey2)
                    ? DecryptDual(cleanText, cleanKey1, cleanKey2, nullCount, key1, key2)
                    : DecryptSingle(cleanText, cleanKey1, nullCount, key1);
            }

            return result;
        }

        #endregion

        #region Single-Key Encryption/Decryption

        /// <summary>
        /// Single-key encryption process:
        /// 1. First columnar transposition
        /// 2. Add frequency-based null characters
        /// 3. Second columnar transposition (using same key)
        /// Updates presentation with visualization of each step
        /// </summary>
        private string EncryptSingle(string text, string key, int nullCount, string originalKey)
        {
            int[] perm = CreatePermutation(key);

            // Step 1: First transposition (fill by rows, read by columns)
            string step1 = Transpose(text, perm, true);
            string grid1 = CreateGridVisualization(text, key.Length, true, perm);

            // Step 2: Add frequency-based null characters at the end
            string step2 = step1 + GenerateFrequencyBasedNulls(nullCount);

            // Step 3: Second transposition (fill by rows, read by columns)
            string step3 = Transpose(step2, perm, true);
            string grid2 = CreateGridVisualization(step2, key.Length, true, perm);

            // Update presentation with all steps
            UpdatePresentationSimple(text, originalKey, true, perm, grid1, step1, step2, grid2, step3);

            return FormatOutput(step3);
        }

        /// <summary>
        /// Single-key decryption process (reverse of encryption):
        /// 1. Reverse second transposition
        /// 2. Remove null characters from end
        /// 3. Reverse first transposition
        /// Updates presentation with visualization of each step
        /// </summary>
        private string DecryptSingle(string text, string key, int nullCount, string originalKey)
        {
            int[] perm = CreatePermutation(key);

            // Step 1: Reverse second transposition (fill by columns, read by rows)
            string step1 = Transpose(text, perm, false);
            string grid1 = CreateGridVisualization(text, key.Length, false, perm);

            // Step 2: Remove null characters from the end
            string step2 = RemoveNulls(step1, nullCount);

            // Step 3: Reverse first transposition (fill by columns, read by rows)
            string result = Transpose(step2, perm, false);
            string grid2 = CreateGridVisualization(step2, key.Length, false, perm);

            // Update presentation with all steps
            UpdatePresentationSimple(text, originalKey, false, perm, grid1, step1, step2, grid2, result);

            return result;
        }

        #endregion

        #region Dual-Key Encryption/Decryption

        /// <summary>
        /// Dual-key encryption process:
        /// 1. First transposition using Key 1
        /// 2. Add frequency-based null characters
        /// 3. Second transposition using Key 2 (different from Key 1)
        /// Provides stronger security by using different permutations for each step
        /// </summary>
        private string EncryptDual(string text, string key1, string key2, int nullCount, string origKey1, string origKey2)
        {
            int[] perm1 = CreatePermutation(key1);
            int[] perm2 = CreatePermutation(key2);

            // Step 1: First transposition with Key 1
            string step1 = Transpose(text, perm1, true);
            
            // Step 2: Add nulls
            string step2 = step1 + GenerateFrequencyBasedNulls(nullCount);
            
            // Step 3: Second transposition with Key 2
            string step3 = Transpose(step2, perm2, true);

            // Create combined key display for presentation
            string combinedKey = origKey1 + " / " + origKey2;
            string grid1 = CreateGridVisualization(text, key1.Length, true, perm1);
            string grid2 = CreateGridVisualization(step2, key2.Length, true, perm2);

            UpdatePresentationSimple(text, combinedKey, true, perm1, grid1, step1, step2, grid2, step3);

            return FormatOutput(step3);
        }

        /// <summary>
        /// Dual-key decryption process (reverse of dual encryption):
        /// 1. Reverse second transposition using Key 2
        /// 2. Remove null characters
        /// 3. Reverse first transposition using Key 1
        /// </summary>
        private string DecryptDual(string text, string key1, string key2, int nullCount, string origKey1, string origKey2)
        {
            int[] perm1 = CreatePermutation(key1);
            int[] perm2 = CreatePermutation(key2);

            // Step 1: Reverse second transposition (Key 2)
            string step1 = Transpose(text, perm2, false);
            
            // Step 2: Remove nulls
            string step2 = RemoveNulls(step1, nullCount);
            
            // Step 3: Reverse first transposition (Key 1)
            string result = Transpose(step2, perm1, false);

            // Create combined key display for presentation
            string combinedKey = origKey1 + " / " + origKey2;
            string grid1 = CreateGridVisualization(text, key2.Length, false, perm2);
            string grid2 = CreateGridVisualization(step2, key1.Length, false, perm1);

            UpdatePresentationSimple(text, combinedKey, false, perm2, grid1, step1, step2, grid2, result);

            return result;
        }

        #endregion

        #region Transposition Logic

        /// <summary>
        /// Universal transposition function - handles both encryption and decryption
        /// Encryption mode: Fill grid by rows, read by columns according to permutation
        /// Decryption mode: Fill grid by columns (according to permutation), read by rows
        /// </summary>
        private string Transpose(string text, int[] permutation, bool encryptMode)
        {
            int numCols = permutation.Length;
            int numRows = (text.Length + numCols - 1) / numCols; // Calculate required rows

            if (encryptMode)
            {
                // ENCRYPTION: Fill by rows, read by columns
                char[,] grid = new char[numRows, numCols];
                int idx = 0;

                // Fill grid row by row
                for (int r = 0; r < numRows; r++)
                    for (int c = 0; c < numCols; c++)
                        grid[r, c] = idx < text.Length ? text[idx++] : (char)0;

                // Create mapping: permutation value -> column index
                int[] orderToCol = new int[numCols];
                for (int c = 0; c < numCols; c++)
                    orderToCol[permutation[c] - 1] = c;

                // Read columns in permutation order
                StringBuilder result = new StringBuilder();
                for (int order = 0; order < numCols; order++)
                {
                    int colIdx = orderToCol[order];
                    for (int r = 0; r < numRows; r++)
                        if (grid[r, colIdx] != (char)0)
                            result.Append(grid[r, colIdx]);
                }
                return result.ToString();
            }
            else
            {
                // DECRYPTION: Fill by columns (in permutation order), read by rows
                
                // Create mapping: permutation value -> column index
                int[] orderToCol = new int[numCols];
                for (int c = 0; c < numCols; c++)
                    orderToCol[permutation[c] - 1] = c;

                // Calculate column lengths (some columns may have one less character)
                int baseLen = text.Length / numCols;
                int extraChars = text.Length % numCols;
                int[] colLengths = new int[numCols];
                for (int i = 0; i < numCols; i++)
                    colLengths[i] = baseLen + (orderToCol[i] < extraChars ? 1 : 0);

                // Extract columns from input text
                string[] columns = new string[numCols];
                int pos = 0;
                for (int order = 0; order < numCols; order++)
                {
                    int length = colLengths[order];
                    columns[order] = pos + length <= text.Length ? text.Substring(pos, length) : text.Substring(pos);
                    pos += length;
                }

                // Fill grid column by column
                char[,] grid = new char[numRows, numCols];
                for (int order = 0; order < numCols; order++)
                {
                    int colIdx = orderToCol[order];
                    string colData = columns[order];
                    for (int r = 0; r < colData.Length && r < numRows; r++)
                        grid[r, colIdx] = colData[r];
                }

                // Read grid row by row
                StringBuilder result = new StringBuilder();
                for (int r = 0; r < numRows; r++)
                    for (int c = 0; c < numCols; c++)
                        if (grid[r, c] != (char)0)
                            result.Append(grid[r, c]);

                return result.ToString();
            }
        }

        #endregion

        #region Null Character Generation

        /// <summary>
        /// Generates null characters based on English letter frequency distribution
        /// This makes the nulls look like natural text, making cryptanalysis harder
        /// Uses weighted random selection based on actual letter frequencies in English
        /// </summary>
        private string GenerateFrequencyBasedNulls(int count)
        {
            if (count <= 0) return "";

            // Create cumulative probability distribution
            var letters = englishFrequencies.Keys.ToArray();
            var cumulative = new double[letters.Length];
            double total = 0;

            for (int i = 0; i < letters.Length; i++)
            {
                total += englishFrequencies[letters[i]];
                cumulative[i] = total;
            }

            StringBuilder nulls = new StringBuilder();

            // Generate each null character using weighted random selection
            for (int i = 0; i < count; i++)
            {
                double rand = random.NextDouble() * total;

                // Find the letter corresponding to this random value
                for (int j = 0; j < cumulative.Length; j++)
                {
                    if (rand <= cumulative[j])
                    {
                        nulls.Append(letters[j]);
                        break;
                    }
                }
            }

            return nulls.ToString();
        }

        /// <summary>
        /// Removes specified number of null characters from the end of text
        /// Used during decryption to remove the frequency-based nulls added during encryption
        /// </summary>
        private string RemoveNulls(string text, int count) =>
            text.Length >= count ? text.Substring(0, text.Length - count) : text;

        #endregion

        #region Permutation Creation

        /// <summary>
        /// Creates a column permutation from a key string
        /// Sorts characters alphabetically, assigning numbers 1-n based on alphabetical order
        /// If duplicate characters exist, they are numbered by position (left to right)
        /// Example: "ZEBRA" -> [5,2,1,4,3] (alphabetical order: B=1, E=2, R=3, A(last)=4, Z=5)
        /// </summary>
        private int[] CreatePermutation(string key)
        {
            var pairs = new List<KeyValuePair<char, int>>();
            for (int i = 0; i < key.Length; i++)
                pairs.Add(new KeyValuePair<char, int>(key[i], i));

            // Sort by character, then by position (for duplicate handling)
            pairs.Sort((x, y) => x.Key != y.Key ? x.Key.CompareTo(y.Key) : x.Value.CompareTo(y.Value));

            int[] perm = new int[key.Length];
            for (int i = 0; i < pairs.Count; i++)
                perm[pairs[i].Value] = i + 1;
            return perm;
        }

        #endregion

        #region Visualization and Presentation

        /// <summary>
        /// Creates a text-based grid visualization of the transposition table
        /// Shows the permutation order at top and text arranged in rows/columns
        /// Handles both encryption (fill by rows) and decryption (fill by columns) modes
        /// </summary>
        private string CreateGridVisualization(string text, int numCols, bool fillByRows, int[] permutation)
        {
            try
            {
                StringBuilder result = new StringBuilder();

                // Add permutation header row
                result.Append("   ");
                for (int i = 0; i < numCols; i++)
                {
                    result.Append(permutation[i].ToString().PadLeft(2) + " ");
                }
                result.AppendLine();

                // Add separator line
                result.Append("   ");
                for (int i = 0; i < numCols; i++)
                {
                    result.Append("-- ");
                }
                result.AppendLine();

                int numRows = (text.Length + numCols - 1) / numCols;
                char[,] grid = new char[numRows, numCols];

                if (fillByRows)
                {
                    // Fill by rows - normal mode for encryption display
                    int idx = 0;
                    for (int r = 0; r < numRows; r++)
                    {
                        for (int c = 0; c < numCols; c++)
                        {
                            if (idx < text.Length)
                            {
                                grid[r, c] = text[idx++];
                            }
                        }
                    }
                }
                else
                {
                    // Fill by columns according to permutation order for decryption display
                    int[] orderToCol = new int[numCols];
                    for (int c = 0; c < numCols; c++)
                        orderToCol[permutation[c] - 1] = c;

                    // Calculate column lengths properly
                    int baseLen = text.Length / numCols;
                    int extraChars = text.Length % numCols;

                    int pos = 0;
                    for (int order = 0; order < numCols; order++)
                    {
                        int colIdx = orderToCol[order];
                        int colLength = baseLen + (order < extraChars ? 1 : 0);

                        for (int r = 0; r < colLength && pos < text.Length; r++)
                        {
                            grid[r, colIdx] = text[pos++];
                        }
                    }
                }

                // Display the grid - only show rows with content
                for (int r = 0; r < numRows; r++)
                {
                    bool hasContent = false;
                    for (int c = 0; c < numCols; c++)
                    {
                        if (grid[r, c] != '\0')
                        {
                            hasContent = true;
                            break;
                        }
                    }

                    if (hasContent)
                    {
                        result.Append((r + 1).ToString().PadLeft(2) + " ");
                        for (int c = 0; c < numCols; c++)
                        {
                            if (grid[r, c] != '\0')
                            {
                                result.Append(grid[r, c] + "  ");
                            }
                            else
                            {
                                result.Append("   ");
                            }
                        }
                        result.AppendLine();
                    }
                }

                return result.ToString();
            }
            catch
            {
                return Resources.Ubchi_GridVisualizationError;
            }
        }

        /// <summary>
        /// Updates the presentation window with step-by-step cipher process visualization
        /// Shows permutation, grids, intermediate results, and which nulls were added/removed
        /// </summary>
        private void UpdatePresentationSimple(string inputText, string key, bool isEncryption,
            int[] permutation, string firstGrid, string firstResult, string withNulls,
            string secondGrid, string finalResult)
        {
            if (_presentation == null) return;

            try
            {
                string permText = "[" + string.Join(",", permutation) + "]";

                // Show which null characters were used
                string nullCharsInfo = "";
                if (isEncryption && withNulls.Length > firstResult.Length)
                {
                    string addedNulls = withNulls.Substring(firstResult.Length);
                    nullCharsInfo = string.Format(Resources.Ubchi_NullCharsAdded, addedNulls);
                }
                else if (!isEncryption && firstResult.Length > withNulls.Length)
                {
                    string removedNulls = firstResult.Substring(withNulls.Length);
                    nullCharsInfo = string.Format(Resources.Ubchi_NullCharsRemoved, removedNulls);
                }

                _presentation.UpdatePresentation(inputText, key, isEncryption, permText,
                    firstGrid, firstResult, withNulls, secondGrid, finalResult, nullCharsInfo);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format(Resources.Ubchi_Error_Presentation, ex.Message), NotificationLevel.Warning);
            }
        }

        #endregion

        #region Helper Methods


        /// <summary>
        /// Returns a localized verb for the current action (Encrypt/Decrypt)
        /// </summary>
        private string GetLocalizedActionVerb(UbchiAction action) =>
            action == UbchiAction.Encrypt ? Resources.UbchiSettings_Action_Encrypt : Resources.UbchiSettings_Action_Decrypt;

        /// <summary>
        /// Counts the number of words in a key string
        /// Words are separated by spaces; consecutive spaces are treated as single separator
        /// This count determines how many null characters will be added
        /// </summary>
        private int CountWords(string key) =>
            string.IsNullOrEmpty(key) ? 0 : key.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;

        /// <summary>
        /// Prepares text for encryption/decryption:
        /// - Converts to uppercase
        /// - Removes all characters except A-Z
        /// - Removes all spaces
        /// </summary>
        private string PrepareText(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";

            StringBuilder result = new StringBuilder();
            foreach (char c in text.ToUpper())
                if (c >= 'A' && c <= 'Z') result.Append(c);
            return result.ToString();
        }

        /// <summary>
        /// Formats output text by grouping characters in groups of 5
        /// Makes the ciphertext more readable and easier to transmit
        /// Example: "ABCDEFGHIJK" -> "ABCDE FGHIJK"
        /// </summary>
        private string FormatOutput(string text)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                if (i > 0 && i % 5 == 0) result.Append(' ');
                result.Append(text[i]);
            }
            return result.ToString();
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Logs a message to the CrypTool GUI log window
        /// </summary>
        private void GuiLogMessage(string message, NotificationLevel level) =>
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, level));

        /// <summary>
        /// Notifies that a property has changed (triggers output update)
        /// </summary>
        private void OnPropertyChanged(string name) =>
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));

        /// <summary>
        /// Updates the progress bar in CrypTool interface
        /// </summary>
        private void ProgressChanged(double value, double max) =>
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));

        #endregion
    }
}