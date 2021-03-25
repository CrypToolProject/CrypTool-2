/* 
   Copyright 2009 Holger Pretzsch, University of Duisburg-Essen

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

namespace CrypTool.MD5.Algorithm
{
    /// <summary>
    /// Options for description of <see cref="PresentableMD5"/> algorithm's current state
    /// </summary>
    /// <seealso cref="PresentableMD5"/>
    /// <seealso cref="PresentableMD5State"/>
    public enum MD5StateDescription
    {
        UNINITIALIZED, INITIALIZED,
        READING_DATA, READ_DATA,
        STARTING_PADDING, ADDING_PADDING_BYTES, ADDED_PADDING_BYTES, ADDING_LENGTH, ADDED_LENGTH, FINISHED_PADDING,
        STARTING_COMPRESSION, STARTING_ROUND, STARTING_ROUND_STEP, FINISHED_ROUND_STEP, FINISHED_ROUND, FINISHING_COMPRESSION, FINISHED_COMPRESSION,
        FINISHED
    }
}
