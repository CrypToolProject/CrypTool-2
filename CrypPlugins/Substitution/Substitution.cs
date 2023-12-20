/*
   Copyright 2022 Nils Kopal, CrypTool Team

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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CrypTool.Substitution
{
    [Author("Nils Kopal", "Nils.Kopal@cryptool.org", "CrypTool Team", "http://www.cryptool.org")]
    [PluginInfo("CrypTool.Substitution.Properties.Resources", "PluginCaption", "PluginTooltip", "Substitution/DetailedDescription/doc.xml",
      new[] { "Substitution/Images/icon.png", "Substitution/Images/encrypt.png", "Substitution/Images/decrypt.png" })]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class Substitution : ICrypComponent
    {
        private readonly SubstitutionPresentation _presentation = new SubstitutionPresentation();
        private readonly SubstitutionSettings _settings = new SubstitutionSettings();
        private bool _stopped;

        [PropertyInfo(Direction.InputData, "InputStringCaption", "InputStringTooltip", true)]
        public string InputString
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", false)]
        public string OutputString
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "SourceAlphabetCaption", "SourceAlphabetTooltip", true)]
        public string SourceAlphabet
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "DestinationAlphabetCaption", "DestinationAlphabetTooltip", true)]
        public string DestinationAlphabet
        {
            get;
            set;
        }

        public void PreExecution()
        {
            _stopped = false;
        }

        public void PostExecution()
        {
            _stopped = true;
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        public ISettings Settings => _settings;

        public UserControl Presentation => _presentation;

        public void Execute()
        {
            ProgressChanged(0, 1);

            Dictionary<string, string> dict;
            if (_settings.Action == 0) // Encrypt
            {
                dict = GenerateSubstitutionDictionary(SourceAlphabet.Replace(Environment.NewLine, string.Empty), DestinationAlphabet.Replace(Environment.NewLine, string.Empty), ref _stopped);
            }
            else //Decrypt
            {
                dict = GenerateSubstitutionDictionary(DestinationAlphabet.Replace(Environment.NewLine, string.Empty), SourceAlphabet.Replace(Environment.NewLine, string.Empty), ref _stopped);
            }

            if (_stopped)
            {
                return;
            }

            GeneratePresentationMapping(dict);

            if (((SubstitutionSettings)Settings).SymbolChoice == SymbolChoice.Random)
            {
                if (!string.IsNullOrEmpty(((SubstitutionSettings)Settings).InputSeparatorSymbol))
                {
                    OutputString = Substitute(InputString, dict, ProcessEscapeSymbols(((SubstitutionSettings)Settings).InputSeparatorSymbol));
                }
                else
                {
                    OutputString = Substitute(InputString, dict);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(((SubstitutionSettings)Settings).InputSeparatorSymbol))
                {
                    OutputString = Substitute(InputString, dict, ProcessEscapeSymbols(((SubstitutionSettings)Settings).InputSeparatorSymbol), false);
                }
                else
                {
                    OutputString = Substitute(InputString, dict, false);
                }

            }
            if (!_stopped)
            {
                OnPropertyChanged("OutputString");
                ProgressChanged(1, 1);
            }
        }

        /// <summary>
        /// Generates the graphical presentation
        /// </summary>
        /// <param name="dict"></param>
        private void GeneratePresentationMapping(Dictionary<string, string> dict)
        {
            int counter = 0;
            _presentation.Dispatcher.Invoke(DispatcherPriority.Background, (SendOrPostCallback)delegate
            {
                try
                {
                    _presentation.Stackpanel.Children.Clear();
                    foreach (KeyValuePair<string, string> entry in dict)
                    {
                        string[] froms = entry.Key.Split('|');
                        string[] tos = entry.Value.Split('|');

                        Grid grid = new Grid();
                        grid.RowDefinitions.Add(new RowDefinition());
                        grid.RowDefinitions.Add(new RowDefinition());
                        grid.RowDefinitions.Add(new RowDefinition());
                        grid.Width = Math.Max(55 * froms.Length, 55 * tos.Length);
                        grid.Height = 155;

                        for (int i = 0; i < Math.Max(froms.Length, tos.Length); i++)
                        {
                            grid.ColumnDefinitions.Add(new ColumnDefinition());
                        }

                        int fromColumnCounter = 0;
                        foreach (string from in froms)
                        {
                            string displayFrom = from;
                            //handling of newlines
                            if (displayFrom.Equals(Environment.NewLine))
                            {
                                displayFrom = @"\n";
                            }
                            Rectangle fromRectangle = new Rectangle();
                            SolidColorBrush fromsolidBrushColor = new SolidColorBrush
                            {
                                Color = Color.FromArgb(255, 100, 255, 100)
                            };
                            fromRectangle.ToolTip = displayFrom;
                            fromRectangle.Fill = fromsolidBrushColor;
                            fromRectangle.StrokeThickness = 2;
                            fromRectangle.Stroke = Brushes.Black;
                            fromRectangle.Width = 50;
                            fromRectangle.Height = 50;
                            fromRectangle.SetValue(Grid.RowProperty, 0);
                            fromRectangle.SetValue(Grid.ColumnProperty, fromColumnCounter);
                            TextBlock fromText = new TextBlock
                            {
                                Text = displayFrom.Length >= 8 ? displayFrom.Substring(0, 5 % displayFrom.Length) + "..." : displayFrom,
                                ToolTip = displayFrom,
                                FontSize = displayFrom.Length > 4 ? 12 : 20,
                                VerticalAlignment = VerticalAlignment.Center,
                                HorizontalAlignment = HorizontalAlignment.Center
                            };
                            fromText.SetValue(Grid.RowProperty, 0);
                            fromText.SetValue(Grid.ColumnProperty, fromColumnCounter);
                            grid.Children.Add(fromRectangle);
                            grid.Children.Add(fromText);
                            fromColumnCounter++;
                        }

                        int toColumnCounter = 0;
                        foreach (string to in tos)
                        {
                            string displayTo = to;
                            if (displayTo.Equals(Environment.NewLine))
                            {
                                displayTo = @"\n";
                            }
                            Rectangle toRectangle = new Rectangle();
                            SolidColorBrush tosolidBrushColor = new SolidColorBrush
                            {
                                Color = Color.FromArgb(255, 255, 100, 100)
                            };
                            toRectangle.ToolTip = displayTo;
                            toRectangle.Fill = tosolidBrushColor;
                            toRectangle.StrokeThickness = 2;
                            toRectangle.Stroke = Brushes.Black;
                            toRectangle.Width = 50;
                            toRectangle.Height = 50;
                            toRectangle.SetValue(Grid.RowProperty, 2);
                            toRectangle.SetValue(Grid.ColumnProperty, toColumnCounter);
                            TextBlock toText = new TextBlock
                            {
                                Text = displayTo.Length >= 8 ? displayTo.Substring(0, 5 % displayTo.Length) + "..." : displayTo,
                                ToolTip = displayTo,
                                FontSize = displayTo.Length > 4 ? 12 : 20,
                                VerticalAlignment = VerticalAlignment.Center,
                                HorizontalAlignment = HorizontalAlignment.Center
                            };
                            toText.SetValue(Grid.RowProperty, 2);
                            toText.SetValue(Grid.ColumnProperty, toColumnCounter);
                            grid.Children.Add(toRectangle);
                            grid.Children.Add(toText);

                            int fromcolumncounter = 0;
                            foreach (string from in froms)
                            {
                                Line line = new Line
                                {
                                    X1 = 25 + 55 * fromcolumncounter,
                                    Y1 = -5,
                                    X2 = 25 + 55 * toColumnCounter,
                                    Y2 = 55
                                };
                                SolidColorBrush lineSolidBrushColor = new SolidColorBrush
                                {
                                    Color = Colors.Black
                                };
                                line.Fill = lineSolidBrushColor;
                                line.Stroke = lineSolidBrushColor;
                                line.StrokeThickness = 2;
                                line.HorizontalAlignment = HorizontalAlignment.Left;
                                line.SetValue(Grid.RowProperty, 1);
                                line.SetValue(Grid.ColumnProperty, 0);
                                line.SetValue(Grid.ColumnSpanProperty, Math.Max(froms.Length, tos.Length));
                                grid.Children.Add(line);
                                fromcolumncounter++;
                            }
                            toColumnCounter++;
                        }

                        _presentation.Stackpanel.Children.Add(grid);
                        counter++;
                        if (counter == _settings.MaxMappingsShownInPresentation)
                        {
                            grid = new Grid();
                            Rectangle fromRectangle = new Rectangle();
                            SolidColorBrush fromsolidBrushColor = new SolidColorBrush
                            {
                                Color = Color.FromArgb(255, 100, 255, 100)
                            };
                            fromRectangle.ToolTip = "...";
                            fromRectangle.Fill = fromsolidBrushColor;
                            fromRectangle.StrokeThickness = 2;
                            fromRectangle.Stroke = Brushes.Black;
                            fromRectangle.Width = 50;
                            fromRectangle.Height = 50;
                            fromRectangle.SetValue(Grid.RowProperty, 0);
                            fromRectangle.SetValue(Grid.ColumnProperty, fromColumnCounter);
                            TextBlock fromText = new TextBlock
                            {
                                Text = "...",
                                ToolTip = "...",
                                FontSize = 11,
                                VerticalAlignment = VerticalAlignment.Center,
                                HorizontalAlignment = HorizontalAlignment.Center
                            };
                            fromText.SetValue(Grid.RowProperty, 0);
                            fromText.SetValue(Grid.ColumnProperty, fromColumnCounter);
                            grid.Children.Add(fromRectangle);
                            grid.Children.Add(fromText);
                            Rectangle toRectangle = new Rectangle();
                            SolidColorBrush tosolidBrushColor = new SolidColorBrush
                            {
                                Color = Color.FromArgb(255, 255, 100, 100)
                            };
                            toRectangle.ToolTip = "...";
                            toRectangle.Fill = tosolidBrushColor;
                            toRectangle.StrokeThickness = 2;
                            toRectangle.Stroke = Brushes.Black;
                            toRectangle.Width = 50;
                            toRectangle.Height = 50;
                            toRectangle.SetValue(Grid.RowProperty, 2);
                            toRectangle.SetValue(Grid.ColumnProperty, toColumnCounter);
                            TextBlock toText = new TextBlock
                            {
                                Text = "...",
                                ToolTip = "...",
                                FontSize = 11,
                                VerticalAlignment = VerticalAlignment.Center,
                                HorizontalAlignment = HorizontalAlignment.Center
                            };
                            toText.SetValue(Grid.RowProperty, 2);
                            toText.SetValue(Grid.ColumnProperty, toColumnCounter);
                            grid.Children.Add(toRectangle);
                            grid.Children.Add(toText);
                            Line line = new Line
                            {
                                X1 = 25,
                                Y1 = -5,
                                X2 = 25,
                                Y2 = 55
                            };
                            SolidColorBrush lineSolidBrushColor = new SolidColorBrush
                            {
                                Color = Colors.Black
                            };
                            line.Fill = lineSolidBrushColor;
                            line.Stroke = lineSolidBrushColor;
                            line.StrokeThickness = 2;
                            line.HorizontalAlignment = HorizontalAlignment.Left;
                            line.SetValue(Grid.RowProperty, 1);
                            line.SetValue(Grid.ColumnProperty, 0);
                            line.SetValue(Grid.ColumnSpanProperty, Math.Max(froms.Length, tos.Length));
                            grid.Children.Add(line);
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format("Error creating substitution presentation: {0}", ex.Message), NotificationLevel.Error);
                }
            }, null);

        }

        public void Stop()
        {
            _stopped = true;
        }

        public void Initialize()
        {
            ((SubstitutionSettings)Settings).UpdateTaskPaneVisibility();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public void Dispose()
        {

        }

        /// <summary>
        /// Generates a dictionary for substitution using a source alphabet and a destination alphabet
        /// </summary>
        /// <param name="sourceAlphabet"></param>
        /// <param name="destinationAlphabet"></param>
        /// <returns></returns>
        private Dictionary<string, string> GenerateSubstitutionDictionary(string sourceAlphabet, string destinationAlphabet, ref bool stop)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            int duplicateSubstitution = 0;

            for (int sourceIndex = 0, destinationIndex = 0; sourceIndex < sourceAlphabet.Length && destinationIndex < destinationAlphabet.Length; sourceIndex++)
            {                
                string sourceCharacter = "";
                string destinationCharacter = "";
                //1. Find next source character (a "character" may be one or more chars in the string)
                if (sourceAlphabet[sourceIndex] == '[')
                {
                    for (sourceIndex++; sourceIndex < sourceAlphabet.Length; sourceIndex++)
                    {
                        if (sourceAlphabet[sourceIndex] != ']')
                        {
                            sourceCharacter += sourceAlphabet[sourceIndex];
                        }
                        else
                        {
                            break;
                        }
                        if (stop)
                        {
                            return dictionary;
                        }
                    }
                }
                else
                {
                    sourceCharacter = new string(sourceAlphabet[sourceIndex], 1);
                }
                //2. Find next destination character (a "character" may be one or more chars in the string)
                if (destinationAlphabet[destinationIndex] == '[')
                {
                    for (destinationIndex++; destinationIndex < destinationAlphabet.Length; destinationIndex++)
                    {
                        if (destinationAlphabet[destinationIndex] != ']')
                        {
                            destinationCharacter += destinationAlphabet[destinationIndex];
                        }
                        else
                        {
                            break;
                        }
                        if (stop)
                        {
                            return dictionary;
                        }
                    }
                }
                else
                {
                    destinationCharacter = new string(destinationAlphabet[destinationIndex], 1);
                }
                destinationIndex++;

                //3. Add Substitution rule to our dictionary
                if (!dictionary.ContainsKey(sourceCharacter))
                {
                    //handling of linebreaks
                    if (sourceCharacter.Equals(@"\n"))
                    {
                        sourceCharacter = Environment.NewLine;
                    }
                    if (destinationCharacter.Equals(@"\n"))
                    {
                        destinationCharacter = Environment.NewLine;
                    }

                    dictionary.Add(sourceCharacter, destinationCharacter);
                }
                else
                {
                    if (duplicateSubstitution < 10)
                    {
                        GuiLogMessage(string.Format(Properties.Resources.SubstitutionAlreadyExists, sourceCharacter, destinationCharacter, dictionary[sourceCharacter]), NotificationLevel.Warning);                        
                    }
                    duplicateSubstitution++;
                }
            }

            if (duplicateSubstitution >= 10)
            {
                GuiLogMessage(String.Format(Properties.Resources.TooManyDuplicateSubstitutions, duplicateSubstitution), NotificationLevel.Warning);
            }

            return dictionary;
        }

        /// <summary>
        /// Substitute a given Text using a dictionary which contains the mapping
        /// </summary>
        /// <param name="text"></param>
        /// <param name="substitutionDictionary"></param>
        /// <param name="randomDistribution"></param>
        /// <returns></returns>
        private string Substitute(string text, Dictionary<string, string> substitutionDictionary, bool randomDistribution = true)
        {
            StringBuilder substitution = new StringBuilder();
            Random random = new Random();

            //we search for the "longest" source character
            int maxLength = substitutionDictionary.Keys.Select(key => key.Length).Concat(new[] { 0 }).Max();

            string currentCharacter = String.Empty;

            //this dictionary is used when we do not want a randomDistribution at poly alphabetic substitution (for example a->[1|2|3])
            //it stores the actual index in the [1|2|3] array
            Dictionary<string, int> polyCounterDictionary = new Dictionary<string, int>();
            for (int position = 0; position < text.Length;)
            {
                if (_stopped)
                {
                    return string.Empty;
                }

                for (int lengthCurrentCharacter = Math.Min(maxLength, (text.Length - position)); lengthCurrentCharacter >= 0; lengthCurrentCharacter--)
                {
                    currentCharacter = text.Substring(position, lengthCurrentCharacter);

                    if (lengthCurrentCharacter == 0)
                    {
                        currentCharacter = text.Substring(position, 1);
                        position++;
                        switch (((SubstitutionSettings)Settings).UnknownSymbolHandling)
                        {
                            case UnknownSymbolHandling.LeaveAsIs:
                                substitution.Append(currentCharacter);
                                if (!string.IsNullOrEmpty(((SubstitutionSettings)Settings).OutputSeparatorSymbol) && position < text.Length)
                                {
                                    substitution.Append(ProcessEscapeSymbols(((SubstitutionSettings)Settings).OutputSeparatorSymbol));
                                }
                                break;
                            case UnknownSymbolHandling.Replace:
                                substitution.Append(((SubstitutionSettings)Settings).ReplacementSymbol);
                                if (!string.IsNullOrEmpty(((SubstitutionSettings)Settings).OutputSeparatorSymbol) && position < text.Length)
                                {
                                    substitution.Append(ProcessEscapeSymbols(((SubstitutionSettings)Settings).OutputSeparatorSymbol));
                                }
                                break;
                            case UnknownSymbolHandling.Remove:
                                break;
                        }
                    }
                    else if (ExistsSubstitionMapping(substitutionDictionary, currentCharacter))
                    {
                        position += lengthCurrentCharacter;
                        string substitutionCharacter = GetSubstitutionValue(substitutionDictionary, currentCharacter);
                        if (substitutionCharacter.Contains("|"))
                        {
                            string[] substitutionCharacters = substitutionCharacter.Split(new[] { '|', '[', ']' });
                            //choose a random character from the substitution array
                            if (randomDistribution)
                            {
                                int randomCharacterNumber = random.Next(substitutionCharacters.Length);
                                substitutionCharacter = substitutionCharacters[randomCharacterNumber];
                            }
                            else
                            //choose the next character from the substitution array
                            {
                                if (polyCounterDictionary.ContainsKey(currentCharacter))
                                {
                                    polyCounterDictionary[currentCharacter] = (polyCounterDictionary[currentCharacter] + 1) % substitutionCharacters.Length;
                                }
                                else
                                {
                                    polyCounterDictionary.Add(currentCharacter, 0);
                                }
                                substitutionCharacter = substitutionCharacters[polyCounterDictionary[currentCharacter]];
                            }
                        }

                        //handle end of lines
                        if (currentCharacter.Equals(@"\n"))
                        {
                            currentCharacter = Environment.NewLine;
                        }

                        substitution.Append(substitutionCharacter);
                        if (!string.IsNullOrEmpty(((SubstitutionSettings)Settings).OutputSeparatorSymbol) && position < text.Length)
                        {
                            substitution.Append(ProcessEscapeSymbols(((SubstitutionSettings)Settings).OutputSeparatorSymbol));
                        }
                        break;
                    }
                }
                ProgressChanged(position, text.Length);
            }
            return substitution.ToString();
        }


        /// <summary>
        /// Substitute a given Text using a dictionary which contains the mapping
        /// this method also uses a input separator which makes the substitution easier and faster
        /// </summary>
        /// <param name="text"></param>
        /// <param name="substitutionDictionary"></param>
        /// <param name="randomDistribution"></param>
        /// <returns></returns>
        private string Substitute(string text, Dictionary<string, string> substitutionDictionary, string inputSeparator, bool randomDistribution = true)
        {
            StringBuilder substitution = new StringBuilder();
            Random random = new Random();

            string[] tokens = text.Split(inputSeparator.ToArray(), StringSplitOptions.RemoveEmptyEntries);
            int counter = 0;

            Dictionary<string, int> polyCounterDictionary = new Dictionary<string, int>();
            foreach (string token in tokens)
            {
                if (_stopped)
                {
                    return string.Empty;
                }

                string substitutionCharacter = GetSubstitutionValue(substitutionDictionary, token);

                if (substitutionCharacter.Contains("|"))
                {
                    string[] substitutionCharacters = substitutionCharacter.Split(new[] { '|', '[', ']' });
                    //choose a random character from the substitution array
                    if (randomDistribution)
                    {
                        int randomCharacterNumber = random.Next(substitutionCharacters.Length);
                        substitutionCharacter = substitutionCharacters[randomCharacterNumber];
                    }
                    else
                    //choose the next character from the substitution array
                    {
                        if (polyCounterDictionary.ContainsKey(token))
                        {
                            polyCounterDictionary[token] = (polyCounterDictionary[token] + 1) %
                                                                     substitutionCharacters.Length;
                        }
                        else
                        {
                            polyCounterDictionary.Add(token, 0);
                        }
                        substitutionCharacter = substitutionCharacters[polyCounterDictionary[token]];
                    }
                }

                if (!ExistsSubstitionMapping(substitutionDictionary, token))
                {
                    switch (((SubstitutionSettings)Settings).UnknownSymbolHandling)
                    {
                        case UnknownSymbolHandling.LeaveAsIs:
                            substitution.Append(token);
                            break;
                        case UnknownSymbolHandling.Replace:
                            substitution.Append(((SubstitutionSettings)Settings).ReplacementSymbol);
                            break;
                        case UnknownSymbolHandling.Remove:
                            break;
                    }
                }
                else
                {
                    substitution.Append(substitutionCharacter);
                }

                if (!string.IsNullOrEmpty(((SubstitutionSettings)Settings).OutputSeparatorSymbol) && counter < tokens.Length)
                {
                    substitution.Append(ProcessEscapeSymbols(((SubstitutionSettings)Settings).OutputSeparatorSymbol));
                }

                counter++;
                ProgressChanged(counter, tokens.Length);               
            }
            return substitution.ToString();
        }

        /// <summary>
        /// Get the substitution value from a dictionary for the given substitution key
        /// </summary>
        /// <param name="substitutionDictionary"></param>
        /// <param name="substitutionKey"></param>
        /// <returns></returns>
        private string GetSubstitutionValue(Dictionary<string, string> substitutionDictionary, string substitutionKey)
        {
            //It can be that the key stands without [], so we can find it with ContainstKey and return it
            if (substitutionDictionary.ContainsKey(substitutionKey))
            {
                return substitutionDictionary[substitutionKey];
            }
            //We did not find it, so we have to search all Keys
            foreach (string key in substitutionDictionary.Keys)
            {
                //we only have to check keys which are arrays
                if (key.Contains("|"))
                {
                    string[] keys = key.Split(new[] { '|', '[', ']' });
                    if (keys.Any(arraykey => arraykey.Equals(substitutionKey)))
                    {
                        return substitutionDictionary[key];
                    }
                }
            }
            return "?";
        }

        /// <summary>
        /// Makes a lookup in a substitution dictionary wether a possible substitution exists or not
        /// </summary>
        /// <param name="substitutionDictionary"></param>
        /// <param name="substitutionKey"></param>
        /// <returns></returns>
        private bool ExistsSubstitionMapping(Dictionary<string, string> substitutionDictionary, string substitutionKey)
        {
            //It can be that the key stands without [], so we can find it with ContainstKey and return true
            if (substitutionDictionary.ContainsKey(substitutionKey))
            {
                return true;
            }

            //We did not find it, so we have to search all Keys, but only if we want to process polypartit
            if (!_settings.ProcessPolypartit)
            {
                return false;
            }
            foreach (string key in substitutionDictionary.Keys)
            {
                //we only have to check keys which are arrays
                if (key.Contains("|"))
                {
                    string[] keys = key.Split(new[] { '|', '[', ']' });
                    if (keys.Any(arraykey => arraykey.Equals(substitutionKey)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Replaces the escape symbols of a string a user entered with "real" escape symbols
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private string ProcessEscapeSymbols(string p)
        {
            return p.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\b", "\b").Replace("\\t", "\t").Replace("\\v", "\v").Replace("\\", "\\");
        }
    }
}
