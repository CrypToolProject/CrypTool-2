/*
   Copyright 2023 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using CrypTool.PluginBase.Utils;
using LanguageStatisticsLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Controls;

namespace CrypTool.Plugins.HomophonicSubstitutionAnalyzer
{
    [Author("Nils Kopal", "nils.kopal@CrypTool.org", "CrypTool 2 Team", "https://www.CrypTool.org")]
    [PluginInfo("CrypTool.Plugins.HomophonicSubstitutionAnalyzer.Properties.Resources", "PluginCaption", "PluginTooltip", "HomophonicSubstitutionAnalyzer/userdoc.xml", "HomophonicSubstitutionAnalyzer/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class HomophonicSubstitutionAnalyzer : ICrypComponent
    {
        #region Private Variables

        private readonly HomophonicSubstitutionAnalyzerSettings _settings = new HomophonicSubstitutionAnalyzerSettings();
        private readonly HomophoneSubstitutionAnalyzerPresentation _presentation = new HomophoneSubstitutionAnalyzerPresentation();
        private bool _running = true;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public HomophonicSubstitutionAnalyzer()
        {
            _presentation.NewBestValue += PresentationOnNewBestValue;
            _presentation.UserChangedText += PresentationOnUserChangedText;
            _presentation.Progress += PresentationOnProgress;
            _presentation.LetterLimitsChanged += PresentationOnLetterLimitsChanged;
            _settings.PropertyChanged += SettingsOnPropertyChanged;
        }

        #region Data Properties

        [PropertyInfo(Direction.InputData, "CiphertextCaption", "CiphertextTooltip", true)]
        public string Ciphertext
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "DictionaryCaption", "DictionaryTooltip", false)]
        public string[] Dictionary
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "PlaintextCaption", "PlaintextTooltip")]
        public string Plaintext
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "KeyCaption", "KeyTooltip")]
        public string Key
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "StartKeyCaption", "StartKeyTooltip", false)]
        public string StartKey
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "FoundWordsCaption", "FoundWordsTooltip")]
        public string FoundWords
        {
            get;
            set;
        }
        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings => _settings;

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation => _presentation;

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            Dictionary = null;
            Ciphertext = null;
            StartKey = null;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);

            //set separator for ciphertext letter separation
            char separator;
            switch (_settings.Separator)
            {
                case Separator.Comma:
                    separator = ',';
                    break;
                case Separator.FullStop:
                    separator = '.';
                    break;
                case Separator.Semicolon:
                    separator = ';';
                    break;
                default:
                    separator = ' ';
                    break;
            }
            string ciphertext = HandleLinebreaks(Ciphertext, out List<int> linebreakPositions);
            _presentation.LoadLangStatistics(_settings.Language, _settings.UseSpaces, _settings.UseNulls);
            _presentation.AddCiphertext(ciphertext, _settings.CiphertextFormat, separator, _settings.StartTemperature, _settings.UseNulls);
            _presentation.AnalyzerConfiguration.PlaintextAlphabet = LanguageStatistics.Alphabet(LanguageStatistics.LanguageCode(_settings.Language), _settings.UseSpaces);
            if (_settings.UseNulls)
            {
                //add symbol for null
                _presentation.AnalyzerConfiguration.PlaintextAlphabet += "#";
            }
            _presentation.AnalyzerConfiguration.WordCountToFind = _settings.WordCountToFind;
            _presentation.AnalyzerConfiguration.MinWordLength = _settings.MinWordLength;
            _presentation.AnalyzerConfiguration.MaxWordLength = _settings.MaxWordLength;
            _presentation.AnalyzerConfiguration.NomenclatureElementsThreshold = _settings.NomenclatureElementsThreshold;
            _presentation.AnalyzerConfiguration.Steps = _settings.Steps;
            _presentation.AnalyzerConfiguration.AnalysisMode = _settings.AnalysisMode;
            _presentation.AnalyzerConfiguration.Restarts = _settings.Restarts;
            _presentation.AnalyzerConfiguration.UseNulls = _settings.UseNulls;
            _presentation.AnalyzerConfiguration.LinebreakPositions = linebreakPositions;
            _presentation.AnalyzerConfiguration.KeepLinebreaks = _settings.KeepLinebreaks;
            if (string.IsNullOrWhiteSpace(_settings.LetterLimits))
            {
                GenerateLetterLimits();
            }
            else
            {
                DeserializeLetterLimits();
            }
            _presentation.AddDictionary(Dictionary);
            _presentation.GenerateGrids();
            _running = true;
            try
            {
                if (!string.IsNullOrEmpty(StartKey))
                {
                    _presentation.ApplyStartKey(StartKey);
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Could not apply key: {0}", ex.Message), NotificationLevel.Warning);
            }

            _presentation.ReplaceCiphertextLabelsByUnicodeChars();

            _presentation.EnableUI();
            if (_settings.AnalysisMode == AnalysisMode.FullAutomatic)
            {
                _presentation.StartAnalysis();
            }

            while (_running)
            {
                Thread.Sleep(100);
            }

            _presentation.DisableUIAndStop();

            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Deserialize LetterLimits stored in the settings
        /// </summary>
        private void DeserializeLetterLimits()
        {
            _presentation.AnalyzerConfiguration.KeyLetterLimits.Clear();
            using (StringReader reader = new StringReader(_settings.LetterLimits))
            {
                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    try
                    {
                        string[] splits = line.Split(';');
                        if (splits.Length == 3)
                        {
                            int letter = int.Parse(splits[0]);
                            int minValue = int.Parse(splits[1]);
                            int maxValue = int.Parse(splits[2]);
                            _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = letter, MinValue = minValue, MaxValue = maxValue });
                        }
                        else
                        {
                            throw new Exception(string.Format("Invalid line in LetterLimits: {0}", line));
                        }

                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage(string.Format("Error while deserializing LetterLimits: {0}", ex.Message), NotificationLevel.Error);
                    }
                }
            }
            _presentation.GenerateKeyLetterLimitsListView();
        }

        /// <summary>
        /// Serializes and stores LetterLimits in the settings
        /// </summary>
        private void SerializeLetterLimits()
        {
            StringBuilder builder = new StringBuilder();
            foreach (LetterLimits letterLimit in _presentation.AnalyzerConfiguration.KeyLetterLimits)
            {
                builder.AppendLine(string.Format("{0};{1};{2}", letterLimit.Letter, letterLimit.MinValue, letterLimit.MaxValue));
            }
            _settings.LetterLimits = builder.ToString();
        }

        /// <summary>
        /// Removes the line breaks from the ciphertext
        /// Also, returns a list of all linebreak positions
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <returns></returns>
        private string HandleLinebreaks(string ciphertext, out List<int> linebreakPositions)
        {
            //here, we memorize all linebreak positions 
            ciphertext = ciphertext.Replace(Environment.NewLine, "\0");
            ciphertext = ciphertext.Replace("\r", "\0");
            ciphertext = ciphertext.Replace("\n", "\0");
            linebreakPositions = new List<int>();
            for (int i = 0; i < ciphertext.Length; i++)
            {
                if (ciphertext[i] == '\0')
                {
                    linebreakPositions.Add(i - linebreakPositions.Count); //add the count since we remove the linebreaks
                }
            }
            //here, we remove all linebreak placeholders from the string
            ciphertext = ciphertext.Replace("\0", "");
            return ciphertext;
        }

        /// <summary>
        /// Generate the letter limits list based on language
        /// </summary>
        private void GenerateLetterLimits()
        {
            int languageId = _settings.Language;
            string languageCode = LanguageStatistics.SupportedLanguagesCodes[languageId];
            _presentation.AnalyzerConfiguration.KeyLetterLimits.Clear();
            string alphabet = LanguageStatistics.Alphabets[languageCode];

            double[] unigrams;

            if (_settings.KeyLetterDistributionType == KeyLetterDistributionType.LanguageBasted)
            {
                if (LanguageStatistics.Unigrams.ContainsKey(languageCode))
                {
                    unigrams = LanguageStatistics.Unigrams[languageCode];
                }
                else
                {
                    //if we have no unigram stats, we just equally distribute the letters 
                    unigrams = new double[alphabet.Length];
                    for (int i = 0; i < alphabet.Length; i++)
                    {
                        unigrams[i] = 1.0 / alphabet.Length;
                    }
                }
                for (int i = 0; i < alphabet.Length; i++)
                {
                    int minvalue = 1;
                    int maxvalue = 2;
                    if (i < unigrams.Length)
                    {
                        minvalue = (int)Math.Ceiling(unigrams[i] * alphabet.Length);
                        maxvalue = minvalue * 2;
                    }
                    _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = i, MinValue = minvalue, MaxValue = maxvalue });
                }
            }
            else //here we just have uniformly distributed min and max values equal to set homophonicity
            {
                for (int i = 0; i < alphabet.Length; i++)
                {
                    int minvalue = _settings.Homophonicity;
                    int maxvalue = _settings.Homophonicity;                    
                    _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = i, MinValue = minvalue, MaxValue = maxvalue });
                }
            }

            if (_settings.UseSpaces)
            {
                _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = Tools.MapIntoNumberSpace(" ", _presentation.AnalyzerConfiguration.PlaintextAlphabet)[0], MinValue = 2, MaxValue = 3 });   //SPACE
            }
            if (_settings.UseNulls)
            {
                _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = Tools.MapIntoNumberSpace("#", _presentation.AnalyzerConfiguration.PlaintextAlphabet)[0], MinValue = 1, MaxValue = 2 });   //NULLS
            }
            _presentation.GenerateKeyLetterLimitsListView();
        }

        /// <summary>
        /// Progress of analyzer changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="progressChangedEventArgs"></param>
        private void PresentationOnProgress(object sender, ProgressChangedEventArgs progressChangedEventArgs)
        {
            if (!_running)
            {
                return;
            }
            ProgressChanged(progressChangedEventArgs.Percentage, 1);
        }

        private void PresentationOnLetterLimitsChanged(object sender, TextChangedEventArgs e)
        {
            _presentation.UpdateKeyLetterLimits();
            SerializeLetterLimits();
        }

        /// <summary>
        /// Analyzer found a new best value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="newBestValueEventArgs"></param>
        private void PresentationOnNewBestValue(object sender, NewBestValueEventArgs newBestValueEventArgs)
        {
            if (!_running && !newBestValueEventArgs.ForceOutput)
            {
                return;
            }
            if (newBestValueEventArgs.NewTopEntry || newBestValueEventArgs.ForceOutput)
            {
                Plaintext = AddLinebreaksToPlaintext(newBestValueEventArgs.Plaintext);
                OnPropertyChanged("Plaintext");
                if (newBestValueEventArgs.FoundWords != null && newBestValueEventArgs.FoundWords.Count > 0)
                {
                    StringBuilder wordBuilder = new StringBuilder();
                    foreach (string word in newBestValueEventArgs.FoundWords)
                    {
                        wordBuilder.AppendLine(word);
                    }
                    FoundWords = wordBuilder.ToString();
                    OnPropertyChanged("FoundWords");
                }
                if (!string.IsNullOrWhiteSpace(newBestValueEventArgs.SubstitutionKey))
                {
                    Key = newBestValueEventArgs.SubstitutionKey;
                    OnPropertyChanged("Key");
                }
            }
        }

        /// <summary>
        /// Adds original line breaks to plaintext
        /// </summary>
        /// <param name="plaintext"></param>
        /// <returns></returns>
        private string AddLinebreaksToPlaintext(string text)
        {
            StringBuilder builder = new StringBuilder();
            int lastposition = 0;
            if (_presentation.AnalyzerConfiguration.LinebreakPositions != null)
            {
                foreach (int position in _presentation.AnalyzerConfiguration.LinebreakPositions)
                {
                    try
                    {
                        builder.AppendLine(text.Substring(lastposition, position - lastposition));
                        lastposition = position;
                    }
                    catch (Exception)
                    {
                        //wtf?
                    }
                }
            }
            builder.AppendLine(text.Substring(lastposition, text.Length - lastposition));
            return builder.ToString();
        }

        /// <summary>
        /// User changed a homophone plaintext mapping
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="userChangedTextEventArgs"></param>
        private void PresentationOnUserChangedText(object sender, UserChangedTextEventArgs userChangedTextEventArgs)
        {
            Plaintext = userChangedTextEventArgs.Plaintext;
            OnPropertyChanged("Plaintext");
            Key = userChangedTextEventArgs.SubstitutionKey;
            OnPropertyChanged("Key");
        }

        /// <summary>
        /// Settings property was changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_presentation.AnalyzerConfiguration != null &&
               (e.PropertyName.Equals("Language") || 
               e.PropertyName.Equals("UseSpaces") || 
               e.PropertyName.Equals("UseNulls") ||
               e.PropertyName.Equals("KeyLetterDistributionType") ||
               e.PropertyName.Equals("Homophonicity")))
            {
                _presentation.AnalyzerConfiguration.PlaintextAlphabet = LanguageStatistics.Alphabet(LanguageStatistics.LanguageCode(_settings.Language), _settings.UseSpaces);
                if (_settings.UseNulls)
                {
                    //add symbol for null
                    _presentation.AnalyzerConfiguration.PlaintextAlphabet += "#";
                }
                //update letter limits based on language
                GenerateLetterLimits();
                SerializeLetterLimits();
            }
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            _presentation.DisableUIAndStop();
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            _running = false;
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}
