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

using System.Collections.Generic;
using System.Text;

namespace DCAKeyRecovery.Logic
{
    public class DifferentialAttackRoundConfiguration
    {
        public int Round;
        public bool[] ActiveSBoxes;
        public bool IsFirst;
        public bool IsLast;
        public bool IsBeforeLast;
        public double Probability;
        public int InputDifference;
        public int ExpectedDifference;
        public List<Characteristic> Characteristics;
        public List<Pair> UnfilteredPairList;
        public List<Pair> FilteredPairList;
        public List<Pair> EncrypedPairList;
        public SearchPolicy SearchPolicy;
        public AbortingPolicy AbortingPolicy;
        public Algorithms SelectedAlgorithm;

        /// <summary>
        /// default Constructor
        /// </summary>
        public DifferentialAttackRoundConfiguration()
        {
            Round = -1;
            ActiveSBoxes = null;
            InputDifference = -1;
            ExpectedDifference = -1;
            IsFirst = false;
            IsLast = false;
            IsBeforeLast = false;
            Characteristics = new List<Characteristic>();
            UnfilteredPairList = new List<Pair>();
            FilteredPairList = new List<Pair>();
            EncrypedPairList = new List<Pair>();
        }

        /// <summary>
        /// returns binary representation of the SBox configuration
        /// </summary>
        /// <returns></returns>
        public string GetActiveSBoxes()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = (ActiveSBoxes.Length - 1); i >= 0; i--)
            {
                if (ActiveSBoxes[i] == true)
                {
                    sb.Append("1");
                }
                else
                {
                    sb.Append("0");
                }
            }

            return sb.ToString();
        }
    }
}
