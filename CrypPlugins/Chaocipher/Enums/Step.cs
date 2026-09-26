/*
   Copyright (C) CrypTool 2 Team

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
namespace CrypTool.Chaocipher.Enums
{
    public enum Step
    {
        Begin = 0,
        BringCharToZenith = 1,
        BringCipherCharToZenith = 2,
        CharBroughtToZenith = 10,
        CipherCharBroughtToZenith = 11,
        CharCanNotBeEnciphered = 12,
        CharCanNotBeDeciphered = 13,
        PermutateLeftDisk = 100,
        PermutateLeftDiskRemoveChar = 110,
        PermutateLeftDiskMoveChars = 120,
        PermutateLeftDiskInsertChar = 130,
        PermutateRightDisk = 200,
        PermutateRightDiskRemoveChar = 210,
        PermutateRightDiskMoveByOne = 211,
        PermutateRightDiskMoveChars = 220,
        PermutateRightDiskInsertChar = 230,
        NotPermutateOnLastChar = 300,
        End = 10000,
    }
}