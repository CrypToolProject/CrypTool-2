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
using M209AnalyzerLib.Enums;

namespace M209AnalyzerLib.M209
{
    public static class Global
    {
        public static MachineVersion VERSION = initVersion(MachineVersion.V1947);

        public static bool FROM_TABLE;
        public static bool ONLY_TABLE_GROUP_A;
        public static bool ONLY_TABLE_GROUP_B;

        public static int MAX_OVERLAP;
        public static int MIN_OVERLAP;
        public static int MAX_SAME_OVERLAP;
        public static int MAX_TOTAL_OVERLAP;

        public static int MAX_KICK;
        public static int MAX_KICK_REPETITION_64;
        public static int MAX_LOWEST_KICK;
        public static int MIN_KICK;

        public static bool COVERAGE_27;
        public static bool SAME_SUCCESSOR_ALLOWED;
        public static bool EVEN_3;
        public static bool EVEN_2_3_4;
        public static int MAX_CONSECUTIVE_SAME_PINS;
        public static int MIN_INVOLVED_WHEELS;
        public static bool OVERLAPS_SIDEBYSIDE_SEPARATED;
        public static bool OVERLAPS_EVENLY;
        public static int MIN_PERCENT_ACTIVE_PINS;
        public static int MAX_PERCENT_ACTIVE_PINS;

        public static int WHEELS = 6;
        public static int BARS = 27;

        public static MachineVersion initVersion(MachineVersion version)
        {
            return initVersion(version, false, false);
        }

        private static MachineVersion initVersion(MachineVersion version, bool onlyA, bool onlyB)
        {
            VERSION = version;

            if ((VERSION == MachineVersion.V1942) || (VERSION == MachineVersion.V1943) || (VERSION == MachineVersion.V1944) || (VERSION == MachineVersion.V1947) || (VERSION == MachineVersion.V1953))
            {
                ONLY_TABLE_GROUP_A = onlyA;
                ONLY_TABLE_GROUP_B = onlyB;
                FROM_TABLE = (VERSION == MachineVersion.V1944) || (VERSION == MachineVersion.V1947);
                MAX_OVERLAP = 12;
                MIN_OVERLAP = ((VERSION == MachineVersion.V1943) || (VERSION == MachineVersion.V1944)) ? 1 : 2;
                MIN_KICK = 1;
                MAX_KICK = (VERSION == MachineVersion.V1953) ? 14 : 13;
                MAX_KICK_REPETITION_64 = (VERSION == MachineVersion.V1953) ? 5 : int.MaxValue;
                MAX_LOWEST_KICK = 1;
                SAME_SUCCESSOR_ALLOWED = VERSION != MachineVersion.V1942;
                EVEN_3 = VERSION == MachineVersion.V1942;
                EVEN_2_3_4 = VERSION == MachineVersion.V1943;
                MAX_CONSECUTIVE_SAME_PINS = (VERSION == MachineVersion.V1944) || (VERSION == MachineVersion.V1947) || (VERSION == MachineVersion.SWEDISH) ? 6 : int.MaxValue;
                MAX_SAME_OVERLAP = ((VERSION == MachineVersion.V1942) || (VERSION == MachineVersion.V1943)) ? int.MaxValue : 4;
                MIN_INVOLVED_WHEELS = (VERSION != MachineVersion.V1942) ? 4 : 0;
                OVERLAPS_SIDEBYSIDE_SEPARATED = (VERSION != MachineVersion.V1942);
                OVERLAPS_EVENLY = (VERSION != MachineVersion.V1942);
                MAX_TOTAL_OVERLAP = ((VERSION == MachineVersion.V1947) || (VERSION == MachineVersion.V1953)) ? 1 : int.MaxValue;
                COVERAGE_27 = true;
                MAX_PERCENT_ACTIVE_PINS = 70;
                MIN_PERCENT_ACTIVE_PINS = 30;

            }
            else if (version == MachineVersion.SWEDISH)
            {
                FROM_TABLE = false;
                MAX_OVERLAP = 15;
                MIN_OVERLAP = 0;
                MAX_KICK = BARS;
                MAX_KICK_REPETITION_64 = 100;
                SAME_SUCCESSOR_ALLOWED = true;
                EVEN_3 = false;
                EVEN_2_3_4 = false;

                MAX_SAME_OVERLAP = 100;
                MIN_INVOLVED_WHEELS = 0;
                OVERLAPS_SIDEBYSIDE_SEPARATED = false;
                OVERLAPS_EVENLY = false;
                MAX_TOTAL_OVERLAP = BARS;
                MIN_KICK = 0;
                MAX_LOWEST_KICK = MAX_KICK;
                COVERAGE_27 = false;
            }
            else if (version == MachineVersion.UNRESTRICTED)
            {
                FROM_TABLE = false;
                MAX_OVERLAP = BARS;
                MIN_OVERLAP = 0;
                MAX_KICK = BARS;
                MAX_KICK_REPETITION_64 = 100;
                SAME_SUCCESSOR_ALLOWED = true;
                EVEN_3 = false;
                EVEN_2_3_4 = false;
                MAX_CONSECUTIVE_SAME_PINS = 100;
                MAX_PERCENT_ACTIVE_PINS = 100;
                MIN_PERCENT_ACTIVE_PINS = 0;
                MAX_SAME_OVERLAP = 100;
                MIN_INVOLVED_WHEELS = 0;
                OVERLAPS_SIDEBYSIDE_SEPARATED = false;
                OVERLAPS_EVENLY = false;
                MAX_TOTAL_OVERLAP = BARS;
                MIN_KICK = 0;
                MAX_LOWEST_KICK = MAX_KICK;
                COVERAGE_27 = false;
            }
            else if (version == MachineVersion.NO_OVERLAP)
            {
                FROM_TABLE = false;
                MAX_OVERLAP = 0;
                MIN_OVERLAP = 0;
                MAX_KICK = BARS;
                MAX_KICK_REPETITION_64 = 100;
                SAME_SUCCESSOR_ALLOWED = true;
                EVEN_3 = false;
                EVEN_2_3_4 = false;
                MAX_CONSECUTIVE_SAME_PINS = 6;
                MAX_PERCENT_ACTIVE_PINS = 80;
                MIN_PERCENT_ACTIVE_PINS = 20;
                MAX_SAME_OVERLAP = 100;
                MIN_INVOLVED_WHEELS = 0;
                OVERLAPS_SIDEBYSIDE_SEPARATED = false;
                OVERLAPS_EVENLY = false;
                MAX_TOTAL_OVERLAP = 0;
                MIN_KICK = 0;
                MAX_LOWEST_KICK = MAX_KICK;
                COVERAGE_27 = true;
            }

            // LugsRules.createValidLugCountSequences();

            return VERSION;

        }
    }
}
