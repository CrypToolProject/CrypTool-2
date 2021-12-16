/*
   Copyright 2011 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;

namespace CrypTool.Plugins.SpanishStripCipherAnalysis
{
    // HOWTO: Change author name, email address, organization and URL.
    [Author("Luis Alberto Benthin Sanguino", "Luis.BenthinSanguino@rub.de", "Ruhr-Universität Bochum - Chair for Embedded Security", "http://www.emsec.rub.de/chair/home/")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("SpanishStripCipherAnalysis.Properties.Resources", "PluginCaption", "PluginTooltip", "SpanishStripCipherAnalysis/DetailedDescription/doc.xml", new[] { "SpanishStripCipherAnalysis/Images/SpanishStripCipher.png" })]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class SpanishStripCipherAnalysis : ICrypComponent
    {
        #region Private Variables
        private readonly List<string> ciphertTextArray = new List<string>();
        private readonly List<string> usedHomophones = new List<string>();
        private readonly List<List<string>> candidates = new List<List<string>>();
        private readonly List<List<string>> foundHomphonesColumns = new List<List<string>>();
        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        private readonly SpanishStripCipherAnalysisSettings settings = new SpanishStripCipherAnalysisSettings();

        #endregion

        #region Data Properties

        /// <summary>
        /// HOWTO: Input interface to read the input data. 
        /// You can add more input properties of other type if needed.
        /// </summary>
        [PropertyInfo(Direction.InputData, "input", "InputTooltip")]
        public string SomeInput
        {
            get;
            set;
        }

        /// <summary>
        /// HOWTO: Output interface to write the output data.
        /// You can add more output properties ot other type if needed.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "output", "OutputTooltip")]
        public string SomeOutput
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings => settings;
        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation => null;

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            int errorType = 0;
            // HOWTO: Use this to show the progress of a plugin algorithm execution in the editor.
            ProgressChanged(0, 1);
            if (string.IsNullOrEmpty(SomeInput))
            {
                errorType = 1;
            }
            switch (errorType)
            {
                case 0:
                    StringToArray(SomeInput);
                    FindCandidates();
                    CleanCandidates();
                    while (AnlyzeCandidates())
                    {
                    }
                    int columnsCounter = 1;
                    for (int i = 0; i < foundHomphonesColumns.Count; i++)
                    {
                        SomeOutput = SomeOutput + "Column " + columnsCounter + ": {";
                        for (int j = 0; j < foundHomphonesColumns[i].Count; j++)
                        {
                            SomeOutput = SomeOutput + foundHomphonesColumns[i][j] + " ";
                        }
                        SomeOutput = SomeOutput + "} ";
                        columnsCounter++;
                    }
                    break;
                case 1:
                    break;
            }
            // HOWTO: After you have changed an output property, make sure you announce the name of the changed property to the CT2 core.
            //SomeOutput = SomeInput - settings.SomeParameter;
            OnPropertyChanged("SomeOutput");

            // HOWTO: You can pass error, warning, info or debug messages to the CT2 main window.
            /*if (settings.SomeParameter < 0)
                GuiLogMessage("SomeParameter is negative", NotificationLevel.Debug);*/

            // HOWTO: Make sure the progress bar is at maximum when your Execute() finished successfully.
            ProgressChanged(1, 1);
        }
        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
        }
        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
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
        public void StringToArray(string cipherText)
        {
            int j = 0;
            string homophone = "";
            for (int i = 0; i < cipherText.Length; i++)
            {
                if (j == 2)
                {
                    ciphertTextArray.Add(homophone);
                    homophone = "";
                    homophone = homophone + cipherText[i];
                    j = 1;
                }
                else
                {
                    homophone = homophone + cipherText[i];
                    j++;
                }
            }
            ciphertTextArray.Add(homophone);
        }
        public void FindCandidates()
        {
            List<List<string>> subsetsS_n = new List<List<string>>();
            List<string> subsetS_1 = new List<string>();
            for (int i = 0; i < ciphertTextArray.Count; i++)
            {
                if (!usedHomophones.Contains(ciphertTextArray[i]))
                {
                    for (int j = i + 1; j < ciphertTextArray.Count; j++)
                    {
                        if (!ciphertTextArray[i].Equals(ciphertTextArray[j]))
                        {
                            subsetS_1.Add(ciphertTextArray[j]);
                        }
                        else
                        {
                            List<string> subsetS_2 = new List<string>(subsetS_1);
                            subsetsS_n.Add(subsetS_2);
                            subsetS_1.Clear();
                        }
                    }
                    usedHomophones.Add(ciphertTextArray[i]);
                    if (subsetsS_n.Count > 0)
                    {
                        Intersection(subsetsS_n, ciphertTextArray[i]);
                    }
                    subsetsS_n.Clear();
                    subsetS_1.Clear();
                }
            }
        }
        public void Intersection(List<List<string>> subsetsS, string homophone)
        {
            List<List<string>> subsetsS_n = new List<List<string>>(subsetsS);
            HashSet<string> intersection = new HashSet<string>(subsetsS_n[0]);
            for (int i = 1; i < subsetsS_n.Count; i++)
            {
                intersection.IntersectWith(subsetsS_n[i]);
            }
            List<string> S = new List<string>(intersection);
            S.Insert(0, homophone);
            candidates.Add(S);
        }
        public void CleanCandidates()
        {
            for (int i = 0; i <= candidates.Count - 1; i++)
            {
                for (int j = 0; j < candidates.Count - 1; j++)
                {
                    if (i != j)
                    {
                        while (candidates[i].Contains(candidates[j][0]) && !candidates[j].Contains(candidates[i][0]))
                        {
                            candidates[i].Remove(candidates[j][0]);
                        }
                    }
                }
            }
        }
        public bool AnlyzeCandidates()
        {
            bool homophoneFound = false;
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i].Count == 3)
                {
                    Case1(candidates[i]);
                    homophoneFound = true;
                    break;
                }
                else if (candidates[i].Count == 4)
                {
                    if (Case3b(candidates[i]))
                    {
                        homophoneFound = true;
                        break;
                    }
                    else if (Case2(candidates[i]))
                    {
                        homophoneFound = true;
                        break;
                    }
                }
            }
            return homophoneFound;
        }
        public void Case1(List<string> candidate)
        {
            foundHomphonesColumns.Add(candidate);
            DeleteFoundColumn(candidate);
        }
        public bool Case2(List<string> candidate)
        {
            int counterHomophones;
            int counterCandidates = 0;
            bool homophoneFound = false;
            for (int i = 0; i < candidates.Count; i++)
            {
                counterHomophones = 0;
                if (candidates[i].Contains(candidate[0]) && candidate.Contains(candidates[i][0]))
                {
                    for (int j = 0; j < candidate.Count; j++)
                    {
                        if (candidates[i].Contains(candidate[j]))
                        {
                            counterHomophones++;
                        }
                    }
                }
                if (counterHomophones == 4)
                {
                    counterCandidates++;
                }
                if (counterCandidates == 4)
                {
                    homophoneFound = true;
                    foundHomphonesColumns.Add(candidate);
                    DeleteFoundColumn(candidate);
                    break;
                }
            }
            return homophoneFound;
        }
        public bool Case3b(List<string> candidate)
        {
            int counter;
            bool foundHomophone = false;
            List<List<string>> hypothesis = new List<List<string>>(GetHypothesis3(candidate));
            List<List<string>> otherHypothesis = new List<List<string>>();
            List<List<string>> foundCandidate = new List<List<string>>();
            for (int i = 1; i < candidate.Count; i++)
            {
                otherHypothesis.AddRange(GetHypothesis3(GetCandidate(candidate[i])));
            }
            //System.out.println(otherHypothesis);
            for (int i = 0; i < hypothesis.Count; i++)
            {
                counter = 1;
                for (int j = 0; j < otherHypothesis.Count; j++)
                {
                    if (ContainsAllItems(otherHypothesis[j], hypothesis[i]))
                    {
                        counter++;
                    }
                }
                if (counter == 3)
                {
                    foundCandidate.Add(hypothesis[i]);
                }
            }
            if (foundCandidate.Count == 1)
            {
                foundHomophone = true;
                foundHomphonesColumns.Add(foundCandidate[0]);
                DeleteFoundColumn(foundCandidate[0]);
            }
            return foundHomophone;
        }
        public List<List<string>> GetHypothesis3(List<string> set)
        {
            List<List<string>> allHypothesis = new List<List<string>>();
            for (int i = 1; i < set.Count - 1; i++)
            {
                for (int j = i + 1; j < set.Count - 0; j++)
                {
                    List<string> oneHypothesis = new List<string>
                    {
                        set[0],
                        set[i],
                        set[j]
                    };
                    allHypothesis.Add(oneHypothesis);
                }
            }
            return allHypothesis;
        }
        public bool ContainsAllItems(List<string> a, List<string> b)
        {
            bool flag = true;
            for (int i = 0; i < a.Count; i++)
            {
                if (!b.Contains(a[i]))
                {
                    flag = false;
                    break;
                }
            }
            return flag;
        }
        public List<string> GetCandidate(string homophone)
        {
            List<string> candidate = new List<string>();
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i][0].Equals(homophone))
                {
                    candidate = candidates[i];
                    break;
                }
            }
            return candidate;
        }
        public void DeleteFoundColumn(List<string> foundColumn)
        {
            for (int i = 0; i < candidates.Count; i++)
            {
                if (foundColumn.Contains(candidates[i][0]))
                {
                    candidates.RemoveAt(i);
                    i--;
                }
                else
                {
                    for (int j = 0; j < foundColumn.Count; j++)
                    {
                        if (candidates[i].Contains(foundColumn[j]))
                        {
                            candidates[i].Remove(foundColumn[j]);
                        }
                    }
                }
            }
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