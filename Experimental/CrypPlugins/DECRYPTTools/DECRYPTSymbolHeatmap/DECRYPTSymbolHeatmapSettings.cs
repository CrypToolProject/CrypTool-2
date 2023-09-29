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
using System.ComponentModel;

namespace CrypTool.Plugins.DECRYPTTools
{
    public enum NGramsType
    {
        Unigrams = 0,
        Bigrams = 1,
        Trigrams = 2,
        Tetragrams = 3,
    }
    public class DECRYPTSymbolHeatmapSettings : ISettings
    {
        private NGramsType _firstGrams = NGramsType.Unigrams;
        private NGramsType _secondGrams = NGramsType.Unigrams;

        public event PropertyChangedEventHandler PropertyChanged;

        [TaskPane("FirstGramsCaption", "FirstGramsTooltip", null, 1, false, ControlType.ComboBox, new string[]
        {
            "Unigrams",
            "Bigrams",
            "Trigrams",
            "Tetragrams",
            "Pentagrams",
            "Hexagrams",
            "Heptagrams",
            "Octograms"
        })]
        public NGramsType FirstGrams
        {
            get => _firstGrams;
            set
            {
                if ((value) != _firstGrams)
                {
                    _firstGrams = value;
                    OnPropertyChanged("FirstGrams");
                }
            }
        }

        [TaskPane("SecondGramsCaption", "SecondGramsTooltip", null, 2, false, ControlType.ComboBox, new string[]
        {
            "Unigrams",
            "Bigrams",
            "Trigrams",
            "Tetragrams",
            "Pentagrams",
            "Hexagrams",
            "Heptagrams",
            "Octograms"
        })]
        public NGramsType SecondGrams
        {
            get => _secondGrams;
            set
            {
                if ((value) != _secondGrams)
                {
                    _secondGrams = value;
                    OnPropertyChanged("SecondGrams");
                }
            }
        }

        public void Initialize()
        {

        }

        protected void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }
    }
}
