using System;

/* Original Project can be found at https://code.google.com/p/dct-watermark/
* Ported to C# to be used within CrypTool 2 by Nils Rehwald
* Thanks to cgaffa, ZXing and everyone else who worked on the original Project for making the original Java sources available publicly
* Thanks to Nils Kopal for Support and Bugfixing 
 * 
* Copyright 2007 ZXing authors Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
* file except in compliance with the License. You may obtain a copy of the License at
* http://www.apache.org/licenses/LICENSE-2.0 Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
* either express or implied. See the License for the specific language governing permissions and limitations under the
* License.
*/

namespace com.google.zxing.common.reedsolomon
{

    /// <summary>
    /// <para>
    /// Thrown when an exception occurs during Reed-Solomon decoding, such as when there are too many errors to correct.
    /// </para>
    /// 
    /// @author Sean Owen, ported to C# by Nils Rehwald
    /// </summary>
    public sealed class ReedSolomonException : Exception
    {

        public ReedSolomonException(string message) : base(message)
        {
        }

    }

}