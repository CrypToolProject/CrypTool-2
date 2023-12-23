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
namespace M209AnalyzerLib.M209
{
    public class LocalState
    {
        public double BestScore { get; set; }
        public bool Improved { get; set; }
        public int[] BestTypeCount { get; set; }
        public bool[][] BestPins { get; set; }
        public int TaskId { get; set; }
        public int CurrentCycle { get; set; } = 0;

        public int Restarts { get; set; }
        public bool SingleIteration { get; set; }
        public bool Quick { get; set; }

        public LocalState(int taskId)
        {
            TaskId = taskId;
        }
    }
}
