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

namespace DCAKeyRecovery.Logic
{
    public class DifferentialAttackLastRoundResult
    {
        public ushort SubKey0 = 0;
        public ushort SubKey1 = 0;
        public int DecryptionCounter = 0;
        public int KeyCounter = 0;
    }
}
