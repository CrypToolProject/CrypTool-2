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
using CrypTool.Plugins.DECRYPTTools.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace CrypTool.Plugins.DECRYPTTools
{
    /// <summary>
    /// Interaktionslogik für DECRYPTHeatmapPresentation.xaml
    /// </summary>
    public partial class DECRYPTSymbolHeatmapPresentation : UserControl
    {
        public DECRYPTSymbolHeatmapPresentation()
        {
            InitializeComponent();
        }

        public void GenerateNewHeatmap(TextDocument textDocument, List<Token> alphabetTokens, DECRYPTSymbolHeatmapSettings settings)
        {
            int firstGramsCount = (int)settings.FirstGrams + 1;
            int secondGramsCount = (int)settings.SecondGrams + 1;

            //we use a dictionary of token,token to count the combinations
            Dictionary<Tuple<Token, Token>, HeatmapEntry> dictionary = new Dictionary<Tuple<Token, Token>, HeatmapEntry>();

            //convert document to plain symbol list
            List<Token> tokenList = textDocument.ToList();
            List<Symbol> symbolList = new List<Symbol>();
            foreach (Token token in tokenList)
            {
                if (token.TokenType != TokenType.RegularElement)
                {
                    continue;
                }
                foreach (Symbol symbol in token.Symbols)
                {
                    symbolList.Add(symbol);
                }
            }

            //generate dictionary with all combinations of symbols (and axis values)
            int firstCombinationsCount = (int)Math.Pow(alphabetTokens.Count, firstGramsCount);
            int secondCombinationsCount = (int)Math.Pow(alphabetTokens.Count, secondGramsCount);

            int axisTokenCounter = int.MaxValue; //we need this "hack" with the counter to generate unique keys for the dictionary

            dictionary.Add(new Tuple<Token, Token>(new Token(null, "" + axisTokenCounter), new Token(null, "" + (axisTokenCounter - 1))), new HeatmapEntry() { IsAxisValue = true });
            axisTokenCounter -= 2;

            //generate x-axis
            for (int i = 0; i < secondCombinationsCount; i++)
            {
                Token token = GenerateSymbolCombination(i, alphabetTokens, secondGramsCount);
                string axistext = "";
                foreach (Symbol symbol in token.Symbols)
                {
                    axistext += symbol;
                }
                dictionary.Add(new Tuple<Token, Token>(new Token(null, "" + axisTokenCounter), new Token(null, "" + (axisTokenCounter - 1))), new HeatmapEntry() { IsAxisValue = true, Value = axistext });
                axisTokenCounter -= 2;
            }

            //generate entries
            for (int y = 0; y < firstCombinationsCount; y++)
            {
                Token firstToken = GenerateSymbolCombination(y, alphabetTokens, firstGramsCount);

                //this here adds the y-axis value in front of every other value
                string axistext = "";
                foreach (Symbol symbol in firstToken.Symbols)
                {
                    axistext += symbol;
                }
                dictionary.Add(new Tuple<Token, Token>(new Token(null, "" + axisTokenCounter), new Token(null, "" + (axisTokenCounter - 1))), new HeatmapEntry() { IsAxisValue = true, Value = axistext });
                axisTokenCounter -= 2;

                //this loop generates the actual entries of the heatmap
                for (int x = 0; x < secondCombinationsCount; x++)
                {
                    Token secondToken = GenerateSymbolCombination(x, alphabetTokens, secondGramsCount);
                    dictionary.Add(new Tuple<Token, Token>(firstToken, secondToken), new HeatmapEntry() { ToolTip = firstToken.ToString() + secondToken.ToString() });
                }
            }

            int maxvalue = 0;
            //count <token, token> combinations
            for (int position = 0; position <= symbolList.Count - (firstGramsCount + secondGramsCount); position++)
            {
                Token leftToken = new Token(null)
                {
                    Symbols = symbolList.GetRange(position, firstGramsCount)
                };
                Token rightToken = new Token(null)
                {
                    Symbols = symbolList.GetRange(position + firstGramsCount, secondGramsCount)
                };

                Tuple<Token, Token> tuple = new Tuple<Token, Token>(leftToken, rightToken);
                if (!dictionary.ContainsKey(tuple) || dictionary[tuple].IsAxisValue)
                {
                    continue;
                }

                dictionary[tuple].Count = dictionary[tuple].Count + 1;

                if (maxvalue < dictionary[tuple].Count)
                {
                    maxvalue = dictionary[tuple].Count;
                }
            }

            List<HeatmapEntry> entries = dictionary.Values.ToList();

            int averageValue = 0;
            int number = 0;
            //compute average value
            foreach (HeatmapEntry entry in entries)
            {
                if (!entry.IsAxisValue && entry.Count > 0)
                {
                    averageValue += entry.Count;
                    number++;
                }
            }
            averageValue = (int)(averageValue / (float)number);

            //compute colors based on (percentaged) distance to maxvalue
            //also set output strings
            Exception exception = null;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    Color coldColor = Colors.DodgerBlue;
                    Color averageColor = Colors.LightGreen;
                    Color hotColor = Colors.Tomato;

                    foreach (HeatmapEntry entry in entries)
                    {
                        if (entry.IsAxisValue)
                        {
                            continue;
                        }

                        if (entry.Count > averageValue)
                        {
                            float distance = 1;
                            if (maxvalue > 0)
                            {
                                distance = ((maxvalue - averageValue) - (entry.Count - averageValue)) / (float)(maxvalue - averageValue);
                            }

                            byte r = (byte)(averageColor.R * distance + hotColor.R * (1 - distance));
                            byte g = (byte)(averageColor.G * distance + hotColor.G * (1 - distance));
                            byte b = (byte)(averageColor.B * distance + hotColor.B * (1 - distance));

                            Color entryColor = Color.FromRgb(r, g, b);
                            entry.Color = new SolidColorBrush(entryColor);
                            entry.Value = string.Format("{0}", entry.Count);
                        }
                        else
                        {
                            float distance = 1;
                            if (averageValue > 0)
                            {
                                distance = ((averageValue) - entry.Count) / (float)(averageValue);
                            }

                            byte r = (byte)(coldColor.R * distance + averageColor.R * (1 - distance));
                            byte g = (byte)(coldColor.G * distance + averageColor.G * (1 - distance));
                            byte b = (byte)(coldColor.B * distance + averageColor.B * (1 - distance));

                            Color entryColor = Color.FromRgb(r, g, b);
                            entry.Color = new SolidColorBrush(entryColor);
                            entry.Value = string.Format("{0}", entry.Count);
                        }
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            }, null);
            if (exception != null)
            {
                throw exception;
            }

            //output data to user interface
            List<List<HeatmapEntry>> lists = new List<List<HeatmapEntry>>();
            int counter = 0;
            for (int y = 0; y < firstCombinationsCount + 1; y++)
            {
                List<HeatmapEntry> list = new List<HeatmapEntry>();
                lists.Add(list);
                for (int x = 0; x < secondCombinationsCount + 1; x++)
                {
                    list.Add(entries[counter]);
                    counter++;
                }
            }

            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                HeatmapGrid.ItemsSource = lists;
            }, null);

        }

        private Token GenerateSymbolCombination(int i, List<Token> alphabetTokens, int size)
        {
            Token token = new Token(null);
            do
            {
                int j = i % alphabetTokens.Count;
                i = i / alphabetTokens.Count;

                token.Symbols.Insert(0, alphabetTokens[j].Symbols[0]);
            } while (i != 0);
            while (token.Symbols.Count < size)
            {
                token.Symbols.Insert(0, alphabetTokens[0].Symbols[0]);
            }
            return token;
        }
    }

    public class HeatmapEntry
    {
        public HeatmapEntry()
        {
            Value = string.Empty;
            Color = Brushes.White;
            IsAxisValue = false;
        }

        public int Count
        {
            get;
            set;
        }

        public string Value
        {
            get;
            set;
        }

        public Brush Color
        {
            get;
            set;
        }

        public bool IsAxisValue
        {
            get;
            set;
        }

        public string ToolTip
        {
            get; set;
        }
    }
}
