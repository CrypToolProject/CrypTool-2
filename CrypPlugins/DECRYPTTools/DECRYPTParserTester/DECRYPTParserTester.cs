/*
   Copyright 2019 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using CrypTool.Plugins.DECRYPTTools.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Controls;

namespace CrypTool.Plugins.DECRYPTTools
{
    [Author("Nils Kopal", "nils.kopal@CrypTool.org", "CrypTool 2 Team", "https://www.CrypTool.org")]
    [PluginInfo("CrypTool.Plugins.DECRYPTTools.Properties.Resources", "DecodeParserTesterCaption", "DecodeParserTesterTooltip", "DECRYPTTools/userdoc.xml", "DECRYPTTools/icon.png")]
    [ComponentCategory(ComponentCategory.DECRYPTProjectComponent)]
    public class DECRYPTParserTester : ICrypComponent
    {
        private readonly DECRYPTParserTestPresentation _presentation = new DECRYPTParserTestPresentation();
        private readonly DECRYPTParserTesterSettings _settings = new DECRYPTParserTesterSettings();
        private bool _running = false;
        private int _maximumNumberOfNulls = 2;

        [PropertyInfo(Direction.InputData, "ClusterCaption", "ClusterTooltip", true)]
        public string Cluster
        {
            get;
            set;
        }

        public ISettings Settings => _settings;

        public UserControl Presentation => _presentation;

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PropertyChangedEventHandler PropertyChanged;

        public void Dispose()
        {

        }

        public void Execute()
        {
            _maximumNumberOfNulls = _settings.MaximumNumberOfNulls;
            _running = true;
            _presentation.ClearBestlist();
            List<Type> parserTypes = GetAllParsers();

            //here, we count the number of usable parser:
            int totalParsers = 0;
            foreach (Type parserType in parserTypes)
            {
                if (GetVoidConstructorInfo(parserType) == null)
                {
                    continue;
                }
                Parser parser = (Parser)Activator.CreateInstance(parserType);

                PossibleParserParameters possibleParserParameters = parser.GetPossibleParserParameters(_maximumNumberOfNulls);
                if (possibleParserParameters == null ||
                    (possibleParserParameters.PossiblePrefixes.Count == 0 && possibleParserParameters.PossibleNulls.Count == 0))
                {
                    continue;
                }
                totalParsers++;
            }

            ///Convert cluster string into parsed single Token Parser
            SimpleSingleTokenParser simpleSingleTokenParser = new SimpleSingleTokenParser
            {
                DECRYPTTextDocument = Cluster
            };
            TextDocument textDocument = simpleSingleTokenParser.GetTextDocument();
            simpleSingleTokenParser.CleanupDocument(textDocument);

            //here, we do the actual testing of each parser
            int parserCounter = 0;
            foreach (Type parserType in parserTypes)
            {
                if (!_running)
                {
                    return;
                }
                if (GetVoidConstructorInfo(parserType) == null)
                {
                    continue;
                }
                Parser parser = (Parser)Activator.CreateInstance(parserType);

                PossibleParserParameters possibleParserParameters = parser.GetPossibleParserParameters(_maximumNumberOfNulls);

                if (possibleParserParameters.PossibleNulls.Count > 10 && possibleParserParameters.MaximumNumberOfNulls > 0)
                {
                    //we reduce the testing right now to 1 null for parsers with more than 10 possible nulls
                    possibleParserParameters.MaximumNumberOfNulls = 1;
                }

                if (possibleParserParameters == null ||
                    (possibleParserParameters.PossiblePrefixes.Count == 0 && possibleParserParameters.PossibleNulls.Count == 0))
                {
                    continue;
                }

                //test all settings of the parser
                GuiLogMessage(string.Format("Testing all {0} parser setting combinations of {1}", possibleParserParameters.GetNumberOfSettingCombinations(), parser.ParserName), NotificationLevel.Info);
                DateTime startDateTime = DateTime.Now;
                try
                {
                    //we clone the textdocument to avoid double parsing with SingleSimpleTokenParser
                    TextDocument clone = (TextDocument)textDocument.Clone();
                    TestParser(clone, parser, possibleParserParameters);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format("Exception occured during parser test of {0}:", parser.ParserName, ex.Message), NotificationLevel.Error);
                    continue;
                }
                GuiLogMessage(string.Format("Tested all parser setting combinations of {0} done in {1} seconds!", parser.ParserName, (DateTime.Now - startDateTime).TotalMilliseconds / 1000), NotificationLevel.Info);
                parserCounter++;
                ProgressChanged(parserCounter, totalParsers);
            }
            ProgressChanged(1, 1);
            _running = false;
        }

        /// <summary>
        /// Tests all possible settings of the given parser
        /// </summary>
        /// <param name="parser"></param>
        private void TestParser(TextDocument textDocument, Parser parser, PossibleParserParameters possibleParserParameters)
        {
            int combinations = possibleParserParameters.GetNumberOfSettingCombinations();

            List<BestListEntry> bestList = new List<BestListEntry>();
            BlockingCollection<Parameters> alreadyTestedParamters = new BlockingCollection<Parameters>();

            for (int i = 0; i < combinations; i++)
            {
                if (!_running)
                {
                    continue;
                }
                Parameters parameters = possibleParserParameters.GetParameters(i);
                if (parameters == null)
                {
                    //GetParameters returns null if it would generate a setting with a null twice, e.g. "8, 8"
                    continue;
                }
                if (alreadyTestedParamters.Contains(parameters))
                {
                    //The GetParameters method also returns "8,1" and "1,8" for nulls
                    //thus, we do not check these again
                    //we can compare since the GetSettings sorts the result, e.g. it returns "1,8" twice
                    continue;
                }
                alreadyTestedParamters.Add(parameters);

                parser.Prefixes = parameters.Prefixes;
                parser.Nulls = parameters.Nulls;
                TextDocument myParsedTextDocument = parser.GetTextDocument((TextDocument)textDocument.Clone());

                double entropyValue = TextDocument.CalculateEntropy(myParsedTextDocument);

                BestListEntry bestListEntry = new BestListEntry
                {
                    ParserName = parser.ParserName,
                    Nulls = parser.Nulls,
                    Prefixes = parser.Prefixes,
                    EntropyValue = entropyValue
                };
                bestList.Add(bestListEntry);
            }

            bestList.Sort();

            if (bestList.Count > 1)
            {
                if (Math.Abs(1 - (bestList[0].EntropyValue / bestList[1].EntropyValue)) > 0.045)
                {
                    bestList[0].Significant = true;
                }
                _presentation.AddNewBestlistEntry(bestList[0]);
                _presentation.AddNewBestlistEntry(bestList[1]);
            }
            else if (bestList.Count == 1)
            {
                _presentation.AddNewBestlistEntry(bestList[0]);
            }
        }

        public void Initialize()
        {

        }

        public void PostExecution()
        {

        }

        public void PreExecution()
        {

        }

        public void Stop()
        {
            _running = false;
        }

        /// <summary>
        /// Returns types of all parsers implemented in CrypTool.Plugins.DECRYPTTools.Util
        /// which derive from SimpleSingleTokenParser and are not the KeyAsPlaintextParser
        /// </summary>
        /// <returns></returns>
        public List<Type> GetAllParsers()
        {
            List<Parser> parsers = new List<Parser>();
            IEnumerable<Type> query = from t in Assembly.GetExecutingAssembly().GetTypes()
                                      where t.IsClass &&
                                      t.Namespace.Equals("CrypTool.Plugins.DECRYPTTools.Util") &&
                                      t.BaseType.Name.Equals("SimpleSingleTokenParser") &&
                                      !t.Name.Equals("KeyAsPlaintextParser")
                                      select t;
            return query.ToList();
        }

        /// <summary>
        /// Returns the ConstructorInfo of the void constructor of the given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ConstructorInfo GetVoidConstructorInfo(Type type)
        {
            foreach (ConstructorInfo constructorInfo in type.GetConstructors())
            {
                ParameterInfo[] parameters = constructorInfo.GetParameters();
                if (parameters.Length == 0)
                {
                    return constructorInfo;
                }
            }
            return null;
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }
    }

    /// <summary>
    /// BestListEntry for best list of parsers
    /// </summary>
    public class BestListEntry : IComparable
    {
        public BestListEntry()
        {
            ParserName = string.Empty;
            EntropyValue = 0;
            Significant = false;
        }

        public string ParserName { get; set; }
        public double EntropyValue { get; set; }

        public List<Token> Nulls = new List<Token>();
        public List<Token> Prefixes = new List<Token>();

        public string EntropyAsString => "" + Math.Round(EntropyValue, 2);

        public string PrefixesAsString
        {
            get
            {
                if (Prefixes.Count == 0)
                {
                    return string.Empty;
                }
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < Prefixes.Count; i++)
                {
                    stringBuilder.Append(Prefixes[i]);
                    if (i < Prefixes.Count - 1)
                    {
                        stringBuilder.Append(", ");
                    }
                }
                return stringBuilder.ToString();
            }
        }

        public string NullsAsString
        {
            get
            {
                if (Nulls.Count == 0)
                {
                    return string.Empty;
                }
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < Nulls.Count; i++)
                {
                    stringBuilder.Append(Nulls[i]);
                    if (i < Nulls.Count - 1)
                    {
                        stringBuilder.Append(", ");
                    }
                }
                return stringBuilder.ToString();
            }
        }

        /// <summary>
        /// Is this entry significant, i.e. is the value below it at most 5% different?
        /// </summary>
        public bool Significant
        {
            get;
            set;
        }

        public int CompareTo(object obj)
        {
            if (obj is BestListEntry)
            {
                return EntropyValue.CompareTo(((BestListEntry)obj).EntropyValue);
            }
            else
            {
                return 0;
            }
        }
    }
}
