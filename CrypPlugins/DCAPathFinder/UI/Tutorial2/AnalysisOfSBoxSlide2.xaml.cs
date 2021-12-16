/*
   Copyright 2019 Christian Bender christian1.bender@student.uni-siegen.de

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

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;
using UserControl = System.Windows.Controls.UserControl;

namespace DCAPathFinder.UI.Tutorial2
{
    /// <summary>
    /// Interaktionslogik für AnalysisOfSBoxSlide2.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("DCAPathFinder.Properties.Resources")]
    public partial class AnalysisOfSBoxSlide2 : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<DifferenceDistribution> _differenceDistributionData = null;

        public AnalysisOfSBoxSlide2()
        {
            _differenceDistributionData = new ObservableCollection<DifferenceDistribution>();
            fillTable();

            DataContext = this;
            InitializeComponent();
        }

        /// <summary>
        /// Property for table data
        /// </summary>
        public ObservableCollection<DifferenceDistribution> DifferenceDistributionData
        {
            get => _differenceDistributionData;
            set => _differenceDistributionData = value;
        }

        /// <summary>
        /// Generates the data to display
        /// </summary>
        private void fillTable()
        {
            DifferenceDistribution header = new DifferenceDistribution()
            {
                InVal = "",
                ZeroOutVal = 0.ToString("X"),
                OneOutVal = 1.ToString("X"),
                TwoOutVal = 2.ToString("X"),
                ThreeOutVal = 3.ToString("X"),
                FourOutVal = 4.ToString("X"),
                FiveOutVal = 5.ToString("X"),
                SixOutVal = 6.ToString("X"),
                SevenOutVal = 7.ToString("X"),
                EightOutVal = 8.ToString("X"),
                NineOutVal = 9.ToString("X"),
                TenOutVal = 0.ToString("X"),
                ElevenOutVal = 11.ToString("X"),
                TwelveOutVal = 12.ToString("X"),
                ThirteenOutVal = 13.ToString("X"),
                FourteenOutVal = 14.ToString("X"),
                FifteenOutVal = 15.ToString("X")
            };

            DifferenceDistributionData.Add(header);

            int[,] ddt = new int[16, 16];

            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    int inputDiff = i ^ j;
                    int outputDiff = DCAPathFinder.Logic.Cipher2.Cipher2Configuration.SBOX[i] ^
                                     DCAPathFinder.Logic.Cipher2.Cipher2Configuration.SBOX[j];
                    ddt[inputDiff, outputDiff]++;
                }
            }

            for (int i = 0; i < 16; i++)
            {
                DifferenceDistribution line = new DifferenceDistribution()
                {
                    InVal = i.ToString("X"),
                    ZeroOutVal = ddt[i, 0].ToString(),
                    OneOutVal = ddt[i, 1].ToString(),
                    TwoOutVal = ddt[i, 2].ToString(),
                    ThreeOutVal = ddt[i, 3].ToString(),
                    FourOutVal = ddt[i, 4].ToString(),
                    FiveOutVal = ddt[i, 5].ToString(),
                    SixOutVal = ddt[i, 6].ToString(),
                    SevenOutVal = ddt[i, 7].ToString(),
                    EightOutVal = ddt[i, 8].ToString(),
                    NineOutVal = ddt[i, 9].ToString(),
                    TenOutVal = ddt[i, 10].ToString(),
                    ElevenOutVal = ddt[i, 11].ToString(),
                    TwelveOutVal = ddt[i, 12].ToString(),
                    ThirteenOutVal = ddt[i, 13].ToString(),
                    FourteenOutVal = ddt[i, 14].ToString(),
                    FifteenOutVal = ddt[i, 15].ToString()
                };

                DifferenceDistributionData.Add(line);
            }
        }

        /// <summary>
        /// Event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Method to call if data changes
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void Grid_OnLoadingRow(object sender, DataGridRowEventArgs e)
        {
            DifferenceDistribution item = e.Row.Item as DifferenceDistribution;
            if (item != null && item.FifteenOutVal == "F")
            {
                e.Row.Background = new SolidColorBrush(Colors.LightGray);
            }
        }
    }
}