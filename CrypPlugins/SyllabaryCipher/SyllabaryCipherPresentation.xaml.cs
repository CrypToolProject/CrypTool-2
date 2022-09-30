/*
   Copyright 2022 Nils Kopal <kopal<AT>cryptool.org>

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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CrypTool.SyllabaryCipher
{
    
    /// <summary>
    /// Interaktionslogik für SyllabaryCipherPresentation.xaml
    /// </summary>
    public partial class SyllabaryCipherPresentation : UserControl
    {
        private readonly Label[] _labels = new Label[121];

        public SyllabaryCipherPresentation()
        {
            InitializeComponent();

            //Create labels for displaying the substitution texts in the table
            for (int x = 0; x < 11; x++)
            {
                for (int y = 0; y < 11; y++)
                {
                    Border border = new Border();
                    TableGrid.Children.Add(border);
                    border.BorderThickness = new Thickness(1, 1, y == 10 ? 1 : 0, x == 10 ? 1 : 0);
                    if (x == 0 || y == 0)
                    {
                        border.Background = Brushes.LightYellow;
                    }
                    border.BorderBrush = Brushes.Black;
                    Grid.SetRow(border, x);
                    Grid.SetColumn(border, y);                                        

                    _labels[x + y * 11] = new Label();
                    TableGrid.Children.Add(_labels[x + y * 11]);
                    Grid.SetRow(_labels[x + y * 11], x);
                    Grid.SetColumn(_labels[x + y * 11], y);
                    _labels[x + y * 11].VerticalAlignment = VerticalAlignment.Center;
                    _labels[x + y * 11].HorizontalAlignment = HorizontalAlignment.Center;
                    _labels[x + y * 11].FontSize = 14;                                        
                }
            }
        }

        /// <summary>
        /// Fills the table with the coordinates defined by the key and with the mapping table
        /// </summary>
        /// <param name="key"></param>
        /// <param name="table"></param>
        public void FillTable(string key, string[,] table)
        {
            for (int i = 0; i < 10; i++)
            {
                _labels[(i + 1) * 11].Content = key[i];
                _labels[(i + 1) * 11].ToolTip = key[i];
                _labels[1 + i].Content = key[i + 10];
                _labels[1 + i].ToolTip = key[i + 10];
                
            }
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    _labels[x + 1 + (y + 1) * 11].Content = table[x, y];
                    _labels[x + 1 + (y + 1) * 11].ToolTip = string.Format("{0}→{1}{2}", table[x, y], _labels[x + 1].Content, _labels[11 + y * 11].Content);
                }
            }
        }
    }
}