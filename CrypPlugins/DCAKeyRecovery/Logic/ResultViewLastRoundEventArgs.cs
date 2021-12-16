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

namespace DCAKeyRecovery.Logic
{
    public class ResultViewLastRoundEventArgs : EventArgs
    {
        public DateTime startTime;
        public DateTime endTime;
        public string currentPlainText;
        public string currentCipherText;
        public string currentKeyCandidate;
        public string expectedDifference;
        public int round;
        public int currentKeysToTestThisRound;
        public int remainingKeyCandidates;
        public int examinedPairCount;

        /// <summary>
        /// Constructor
        /// </summary>
        public ResultViewLastRoundEventArgs()
        {

        }
    }
}
