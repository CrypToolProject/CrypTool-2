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

namespace DCAPathFinder.Logic.Cipher3
{
    public class Cipher3Characteristic : Characteristic
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Cipher3Characteristic()
        {
            InputDifferentials = new ushort[5];
            OutputDifferentials = new ushort[4];

            for (int i = 0; i < InputDifferentials.Length; i++)
            {
                InputDifferentials[i] = 0;
            }

            for (int i = 0; i < OutputDifferentials.Length; i++)
            {
                OutputDifferentials[i] = 0;
            }
        }

        /// <summary>
        /// override of Clone()
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            Characteristic obj = new Cipher3Characteristic
            {
                InputDifferentials = (ushort[])InputDifferentials.Clone(),
                OutputDifferentials = (ushort[])OutputDifferentials.Clone(),
                Probability = Probability
            };

            return obj;
        }

        /// <summary>
        /// override of ToString()
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Prob: " + Probability + " InputDiffRound5: " + InputDifferentials[4] + " OutputDiffRound4: " +
                   OutputDifferentials[3] + " InputDiffRound4: " + InputDifferentials[3] + " OutputDiffRound3: " +
                   OutputDifferentials[2] + " InputDiffRound3: " + InputDifferentials[2] + " OutputDiffRound2: " +
                   OutputDifferentials[1] + " InputDiffRound2: " + InputDifferentials[1] + " OutputDiffRound1: " +
                   OutputDifferentials[0] + " InputDiffRound1: " + InputDifferentials[0];
        }
    }
}