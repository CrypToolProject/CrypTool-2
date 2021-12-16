/*
   Copyright 2016, Eugen Antal and Tomáš Sovič, FEI STU Bratislava

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

namespace CrypTool.Fialka
{
    public class FialkaEnums
    {
        /// <summary>
        ///  PROTON_I - rotor with blocking pins, PROTON_II - newer version of rotors with adjustable core
        /// </summary>
        public enum rotorTypes { PROTON_I = 0, PROTON_II = 1 };
        /// <summary>
        /// 1K : Russia, 3K : Poland, 4K : DDR (East-Germany), 5K : Hungary, 6K : Czechslovakia, 
        /// 0K : Common wiring for all Warsaw Pact countries in case of war. 
        /// Only 3K, 5K and 6K series are available.
        /// </summary>
        public enum rotorSeries { K3 = 0, K5 = 1, K6 = 2 };
        /// <summary>
        /// M125 - first version, M125-3 - newer version with additional modes (includes NumLock and TextMode)
        /// </summary>
        public enum machineModel { M125 = 0, M125_3 = 1 };
        /// <summary>
        /// Because of cycle 3 on reflector, different operation modes are required. Fialka can be used also in "plain text" mode. 
        /// </summary>
        public enum operationMode { Plain = 0, Encrypt = 1, Decrypt = 2 };

        /// <summary>
        /// Available only in M125-3. While the NumLock10 is enabled, only 10 (numerical) inputs are available. The encryption is repeated, 
        /// until the result is not numeric. 
        /// </summary>
        public enum numLockType { NumLock10 = 0, NumLock30 = 1 };
        /// <summary>
        /// These modes are available only in M125-3. For M-125 the Letters (text) mode should be set for compatibility reasons. 
        /// Letters mode can be used only in NumLock30 (this is the default mode for M-125 machine) where the print head specify the output.
        /// Numbers mode is available only for Cyrillic print head, also the 10 numlock should be set, where only the number keys are used.
        /// Mixed mode allow numbers, letters and punctuation marks to be used simultaneously. 
        /// Two keys are given up for Letter-shift (alpha key A) and Number-shift (number key 1) - the printHeadShift enum holds this value.
        /// </summary>
        public enum textOperationmMode { Letters = 0, Mixed = 1, Numbers = 2 };
        /// <summary>
        /// Determines the input and output mapping. In fact on real fialka the Cyrillic/Latin keyboard mapping represents the same input,
        /// but the output can be printed differently based on a selected print head. Also country specific for Latin.
        /// PrintHeadShift enum helps to specify if the Letter part of the print head is used or the Number (special) part of the print head is used.
        /// </summary>
        public enum printHeadMapping { Cyrillic = 0, Latin = 1 };
        /// <summary>
        /// Determines witch part of the print head will be used.
        /// </summary>
        public enum printHeadShift { LetterShift = 0, NumberShift = 1 };
        /// <summary>
        /// Country specific keyboard layout, determines the input encoding (positions of keys). Also specify the output layout (together with the print head).
        /// Available only: Poland, GDR, Czechoslovakia.
        /// </summary>
        public enum countryLayout { Poland = 0, GDR = 1, Czechoslovakia = 2 };


        #region Input handler
        /// <summary>
        /// How to handle invalid inputs.
        /// </summary>
        public enum handleInvalidInput { Remove = 0, KeepAndMark = 1, KeepUnchanged = 2 }
        #endregion

    }
}
