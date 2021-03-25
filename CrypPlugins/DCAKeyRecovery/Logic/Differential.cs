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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAKeyRecovery.Logic
{
    public class Differential: ICloneable
    {
        public ushort InputDifferential;
        public ushort OutputDifferential;
        public ushort Count = 0;
        public double Probability;

        /// <summary>
        /// Constructor
        /// </summary>
        public Differential()
        {
            InputDifferential = 0;
            OutputDifferential = 0;
            Count = 0;
        }

        /// <summary>
        /// Implementation of ICloneable
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var clone = new Differential
            {
                InputDifferential = this.InputDifferential,
                OutputDifferential = this.OutputDifferential,
                Count = this.Count,
                Probability = this.Probability
            };

            return clone;
        }
    }
}
