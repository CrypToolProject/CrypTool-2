/*
   Copyright 2020 George Lasry
   Converted in 2020 from Java to C# by Nils Kopal

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

namespace EnigmaAnalyzerLib.Common
{
    public enum Flag
    {
        INDICATORS_FILE,// = 'd',
        STRENGTH,// = 'e',
        SCENARIO_PATH,// = 'f',
        LANGUAGE,// = 'g',
        HC_SA_CYCLES,// = 'h',
        CIPHERTEXT,// = 'i',
        CRIB_POSITION,// = 'j',
        KEY,// = 'k',
        SIMULATION_TEXT_LENGTH,// = 'l',
        MODEL,// = 'm',
        CYCLES,// = 'n',
        OFFSET,// = 'o' /* multiplex*/, 
        SIMULATION_OVERLAPS,// = 'o' /* M209 */, 
        MODE,// = 'o' /* Enigma*/,
        CRIB,// = 'p',
        RESOURCE_PATH,// = 'r',
        SIMULATION,// = 's',
        THREADS,// = 't',
        VERBOSE,// = 'u',
        HELP,// = 'v',
        MESSAGE_INDICATOR,// = 'w',
        VERSION,// = 'y' /* M209 */, 
        MIDDLE_RING_SCOPE,// = 'y' /* Enigma */,
        RIGHT_RING_SAMPLING,// = 'x', 
        HEXA_FILE,// = 'h',
        SCENARIO,// = 'z'
    }

    public static class FlagHelper
    {
        public static string GetFlagString(Flag flag)
        {
            switch (flag)
            {
                case Flag.INDICATORS_FILE:
                    return "INDICATORS_FILE";
                case Flag.STRENGTH:
                    return "STRENGTH";
                case Flag.SCENARIO_PATH:
                    return "SCENARIO_PATH";
                case Flag.LANGUAGE:
                    return "LANGUAGE";
                case Flag.HC_SA_CYCLES:
                    return "HC_SA_CYCLES";
                case Flag.CIPHERTEXT:
                    return "CIPHERTEXT";
                case Flag.CRIB_POSITION:
                    return "CRIB_POSITION";
                case Flag.KEY:
                    return "KEY";
                case Flag.SIMULATION_TEXT_LENGTH:
                    return "SIMULATION_TEXT_LENGTH";
                case Flag.MODEL:
                    return "MODEL";
                case Flag.CYCLES:
                    return "CYCLES";
                case Flag.OFFSET:
                    return "OFFSET";
                case Flag.SIMULATION_OVERLAPS:
                    return "SIMULATION_OVERLAPS";
                case Flag.MODE:
                    return "MODE";
                case Flag.CRIB:
                    return "CRIB";
                case Flag.RESOURCE_PATH:
                    return "RESOURCE_PATH";
                case Flag.SIMULATION:
                    return "SIMULATION";
                case Flag.THREADS:
                    return "THREADS";
                case Flag.VERBOSE:
                    return "VERBOSE";
                case Flag.HELP:
                    return "HELP";
                case Flag.MESSAGE_INDICATOR:
                    return "MESSAGE_INDICATOR";
                case Flag.VERSION:
                    return "VERSION";
                case Flag.MIDDLE_RING_SCOPE:
                    return "MIDDLE_RING_SCOPE";
                case Flag.RIGHT_RING_SAMPLING:
                    return "RIGHT_RING_SAMPLING";
                case Flag.HEXA_FILE:
                    return "HEXA_FILE";
                case Flag.SCENARIO:
                    return "SCENARIO";
                default:
                    return string.Empty;
            }
        }
    }
}