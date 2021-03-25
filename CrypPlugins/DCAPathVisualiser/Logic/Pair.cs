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

namespace DCAPathVisualiser.Logic
{
    public class Pair : ICloneable
    {
        public UInt16 LeftMember;
        public UInt16 RightMember;

        /// <summary>
        /// default Constructor
        /// </summary>
        public Pair()
        {
            LeftMember = 0;
            RightMember = 0;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="leftMember"></param>
        /// <param name="rightMember"></param>
        public Pair(UInt16 leftMember, UInt16 rightMember)
        {
            LeftMember = leftMember;
            RightMember = rightMember;
        }

        /// <summary>
        /// IClonable
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var clone = new Pair()
            {
                LeftMember = LeftMember,
                RightMember = RightMember
            };
            return clone;
        }

        /// <summary>
        /// ToString method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ("LeftMember = " + LeftMember + " RightMember = " + RightMember);
        }
    }
}
