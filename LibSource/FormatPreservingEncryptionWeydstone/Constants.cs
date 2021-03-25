/**
 * Format-Preserving Encryption
 * 
 * Copyright (c) 2016 Weydstone LLC dba Sutton Abinger
 * 
 * See the NOTICE file distributed with this work for additional information
 * regarding copyright ownership. Sutton Abinger licenses this file to you under
 * the Apache License, Version 2.0 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of the License
 * at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations under
 * the License.
 */
/*
* Converted and modified by Alexander Hirsch <hirsch@CrypTool.org>
*/

namespace FormatPreservingEncryptionWeydstone
{
    /**
     * Constants used by FF1, FF3 and common functions.
     * 
     * @author Kai Johnson
     *
     */
    public class Constants
    {
        /**
         * Determines whether the methods will produce output for conformance
         * testing against the NIST sample data.
         * <p>
         * Modify the source code to set this field to true to enable output of
         * intermediate results to the console.
         */
        public static bool CONFORMANCE_OUTPUT = true;

        /**
	     * Minimum length of an input string for the FF1 functions.
	     */
        public static int MINLEN = 2;

        /**
         * Maximum length of an input string for the FF1 function.
         */
        public static int MAXLEN = 4096;
        /*
         * NIST SP 800-38G permits MAXLEN to be in the range
         * MINLEN+1..2<sup>32</sup>. We limit MAXLEN to 4096 to keep performance
         * reasonable.
         * 
         * I found FF1 performance to be unacceptable for inputs longer than 4096
         * symbols, but your mileage may vary. Go as big as you dare.
         * 
         * public static final int MAXLEN = Integer.MAX_VALUE - 1;
         */

        /**
         * Minimum radix for FF1 and FF3.
         */
        public static int MINRADIX = 2;

        /**
         * Maximum radix for FF1 and FF3.
         */
        public static int MAXRADIX = 65536;

        /**
        * Minimum radix for FF2 and DFF.
       */
        public static int MINRADIX_FF2 = 2;

        /**
         * Maximum radix for FF2 and DFF.
         */
        public static int MAXRADIX_FF2 = 256;

        /**
         * Non-instantiable class.
         */
        private Constants()
        {
        }
    }
}
