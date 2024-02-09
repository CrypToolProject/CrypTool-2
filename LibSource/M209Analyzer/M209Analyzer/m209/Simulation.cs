/*
   Copyright CrypTool 2 Team josef.matwich@gmail.com

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
using M209AnalyzerLib.Common;

namespace M209AnalyzerLib.M209
{
    public class Simulation
    {
        public static Key CreateSimulationValues(M209AttackManager attackManager)
        {

            Key simulationKey = attackManager.SimulationKey;
            simulationKey.Pins.Randomize();
            simulationKey.Lugs.Randomize(attackManager.SimulationOverlaps);

            string plainText = Utils.ReadPlaintextSegmentFromResource(attackManager.Language, -1, attackManager.SimulationTextLength, true).Substring(0, attackManager.SimulationTextLength);
            string cipherText = simulationKey.EncryptDecrypt(plainText, true);

            simulationKey.SetCipherTextAndCrib(cipherText, plainText);

            return simulationKey;
        }
    }
}
