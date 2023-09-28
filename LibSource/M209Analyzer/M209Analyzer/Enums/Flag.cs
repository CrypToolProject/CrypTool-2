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
namespace M209AnalyzerLib.Enums
{
    public enum Flag
    {
        // https://stackoverflow.com/questions/8588384/how-to-define-an-enum-with-string-value
        INDICATORS_FILE = 'd',
        STRENGTH = 'e',
        SCENARIO_PATH = 'f',
        LANGUAGE = 'g',
        HC_SA_CYCLES = 'h', HEXA_FILE = 'h',
        CIPHERTEXT = 'i',
        CRIB_POSITION = 'j',
        KEY = 'k',
        SIMULATION_TEXT_LENGTH = 'l',
        MODEL = 'm',
        CYCLES = 'n',
        OFFSET = 'o', // multiplex
        SIMULATION_OVERLAPS = 'o', // M209
        MODE = 'o', // Enigma
        CRIB = 'p',
        RESOURCE_PATH = 'r',
        SIMULATION = 's',
        THREADS = 't',
        VERBOSE = 'u',
        HELP = 'v',
        MESSAGE_INDICATOR = 'w',
        VERSION = 'y', // M209
        MIDDLE_RING_SCOPE = 'y', // Enigma
        RIGHT_RING_SAMPLING = 'x',
        SCENARIO = 'z'
    }
}
