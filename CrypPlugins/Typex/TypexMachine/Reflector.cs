/*
   Copyright 2023 Nils Kopal <kopal<AT>cryptool.org>

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
namespace CrypTool.Typex.TypexMachine
{
    public class Reflector : Rotor
    {
        /// <summary>
        /// Creates a reflector
        /// </summary>
        /// <param name="rotor"></param>
        /// <param name="notches"></param>
        /// <param name="ringPosition"></param>
        /// <param name="rotation"></param>
        public Reflector(int[] rotor, int[] notches, int ringPosition, int rotation) : base(rotor, notches, ringPosition, rotation)
        {
        }

        public override void Step()
        {
        }

        public override bool IsAtNotchPosition()
        {
            return false;
        }

        public override bool WillBeAtNotchPosition()
        {
            return false;
        }
    }
}
