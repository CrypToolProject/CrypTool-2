/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System;
using System.ComponentModel;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

namespace CrypTool.Plugins.Zufallsgeneratoren{

    public class ZufallsgeneratorenSettings : ISettings
    {
        public void Initialize()
        {

        }

        #region Private Variables

        private int someParameter = 0;

        #endregion

        #region TaskPane Settings

        private int selectedRNG = 0;

        [PropertySaveOrder(1)]
        [TaskPane("SelectedRNgCaption", "SelectedRNgTooltip", null, 1, false, ControlType.ComboBox, new string[] { "borosh13", "cmrg", "coveyou", "fishman18", "fishman20", "fishman2x", "gfsr4", "knuthran", "knuthran2", "knuthran2002", "lecuyer21", "minstd", "mrg", "mt19937", "mt19937_1999", "mt19937_1998", "r250", "ran0", "ran1", "ran2 ", "ran3", "rand ", "rand48 ", "random128-bsd", "random128-glibc2", "random128-libc5 ", "random256-bsd", "random256-glibc2", "random256-libc5", "random32-bsd", "random32-glibc2", "random32-libc5", "random64-bsd", "random64-glibc2", "random64-libc5", "random8-bsd", "random8-glibc2", "random8-libc5", "random-bsd", "random-glibc2", "random-libc5", "randu", "ranf", "ranlux", "ranlux389", "ranlxd1", "ranlxd2", "ranlxs0", "ranlxs1", "ranlxs2", "ranmar", "slatec", "taus", "taus2", "taus113", "transputer", "tt800", "uni", "uni32", "vax", "waterman14", "zuf", "ca", "uvag", "AES_OFB", "kiss", "superkiss", "R_wichmann_hill", "R_marsaglia_multic", "R_super_duper", "R_mersenne_twister", "R_knuth_taocp", "R_knuth_taocp2" })]
        public int SelectedRNg
        {
            get
            {
                return this.selectedRNG;
            }
            set
            {
                if (value != selectedRNG)
                {
                    this.selectedRNG = value;
                    OnPropertyChanged("Action");
                }
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}
