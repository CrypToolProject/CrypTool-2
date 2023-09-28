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
using M209AnalyzerLib.M209;

namespace M209AnalyzerLib.Common
{
    public class BestListResult
    {
        public double Score { get; set; }
        public Key Key { get; set; }
        public string KeyString { get; set; }
        public string PlaintextString { get; set; }
        public BestListResult(double score, Key key, string plaintextString)
        {
            Set(score, key, plaintextString);
        }
        public void Set(double score, Key key, string plaintextString)
        {
            Score = score;
            Key = key;
            PlaintextString = plaintextString;
            KeyString = key.ToString();
        }
        public string ToString(int rank)
        {
            return $"{rank};[{Score}] ->{PlaintextString}\n";
        }
    }
}
