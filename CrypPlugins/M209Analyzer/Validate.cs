using M209Analyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Cryptool.Plugins.M209Analyzer
{
    internal class Validate
    {
        public static string Version;

        public static bool FROM_TABLE;
        public static bool ONLY_TABLE_GROUP_A;
        public static bool ONLY_TABLE_GROUP_B;

        public static int MAX_OVERLAP;
        public static int MIN_OVERLAP;
        public static int MAX_KICK;
        public static int MAX_KICK_REPETITION_64;
        public static bool COVERAGE_27;
        public static bool SAME_SUCCESSOR_ALLOWED;
        public static bool EVEN_3;
        public static bool EVEN_2_3_4;
        public static int MAX_CONSECUTIVE_SAME_PINS;
        public static int MAX_SAME_OVERLAP;
        public static int MIN_INVOLVED_WHEELS;
        public static bool OVERLAPS_SIDEBYSIDE_SEPARATED;
        public static bool OVERLAPS_EVENLY;
        public static int MAX_TOTAL_OVERLAP;
        public static int MAX_LOWEST_KICK;
        public static int MIN_KICK;
        public static int MIN_PERCENT_ACTIVE_PINS;
        public static int MAX_PERCENT_ACTIVE_PINS;

        public static void initVersion(string version)
        {
            Version = version;

            if (Version == "V1942" || Version == "V1943" || Version == "V1944" || Version == "V1947" || Version == "V1953")
            {
                ONLY_TABLE_GROUP_A = false;
                ONLY_TABLE_GROUP_B = false;
                FROM_TABLE = (Version == "V1944") || (Version == "V1947");
                MAX_OVERLAP = 12;
                MIN_OVERLAP = ((Version == "V1944") || (Version == "V1947")) ? 1 : 2;
                MIN_KICK = 1;
                MAX_KICK = (Version == "V1953") ? 14 : 13;
                MAX_KICK_REPETITION_64 = (Version == "V1953") ? 5 : int.MaxValue;
                MAX_LOWEST_KICK = 1;
                SAME_SUCCESSOR_ALLOWED = Version != "V1942";
                EVEN_3 = Version == "V1942";
                EVEN_2_3_4 = Version == "V1943";
                MAX_CONSECUTIVE_SAME_PINS = (Version == "V1944") || (Version == "V1947") || (Version == "SWEDISH") ? 6 : int.MaxValue;
                MAX_SAME_OVERLAP = ((Version == "V1942") || (Version == "V1943")) ? int.MaxValue : 4;
                MIN_INVOLVED_WHEELS = (Version != "V1942") ? 4 : 0;
                OVERLAPS_SIDEBYSIDE_SEPARATED = (Version != "V1942");
                OVERLAPS_EVENLY = (Version != "V1942");
                MAX_TOTAL_OVERLAP = ((Version == "V1947") || (Version == "V1953")) ? 1 : int.MaxValue;
                COVERAGE_27 = true;
                MAX_PERCENT_ACTIVE_PINS = 70;
                MIN_PERCENT_ACTIVE_PINS = 30;
            }
            else if (version == "SWEDISH")
            {
                FROM_TABLE = false;
                MAX_OVERLAP = 15;
                MIN_OVERLAP = 0;
                MAX_KICK = 27;
                MAX_KICK_REPETITION_64 = 100;
                SAME_SUCCESSOR_ALLOWED = true;
                EVEN_3 = false;
                EVEN_2_3_4 = false;

                MAX_SAME_OVERLAP = 100;
                MIN_INVOLVED_WHEELS = 0;
                OVERLAPS_SIDEBYSIDE_SEPARATED = false;
                OVERLAPS_EVENLY = false;
                MAX_TOTAL_OVERLAP = 27;
                MIN_KICK = 0;
                MAX_LOWEST_KICK = MAX_KICK;
                COVERAGE_27 = false;
            }
            else if (version == "UNRESTRICTED")
            {
                FROM_TABLE = false;
                MAX_OVERLAP = 27;
                MIN_OVERLAP = 0;
                MAX_KICK = 27;
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
                MAX_TOTAL_OVERLAP = 27;
                MIN_KICK = 0;
                MAX_LOWEST_KICK = MAX_KICK;
                COVERAGE_27 = false;
            }
            else if (version == "NO_OVERLAP")
            {
                FROM_TABLE = false;
                MAX_OVERLAP = 0;
                MIN_OVERLAP = 0;
                MAX_KICK = 27;
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
        }
        public static bool LugSettings(LugSettings lugSettings, int barNr)
        {
            int count = lugSettings.GetLugCount(barNr);

            if (count < 0 || count > MAX_KICK)
            {
                return false;
            }

            int overlaps = lugSettings.GetOverlaps();
            if (overlaps > MAX_OVERLAP || overlaps < MIN_OVERLAP)
            {
                return false;
            }
            return true;
        }
    }
}
