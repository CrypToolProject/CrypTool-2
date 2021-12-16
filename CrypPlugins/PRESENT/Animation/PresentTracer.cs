/*
   Copyright 2008 Timm Korte, University of Siegen

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

namespace CrypTool.PRESENT
{
    internal class PresentTracer
    {
        public PresentTracer()
        {
        }

        public string Trace(Present cipher)
        {
            string txtOut;
            txtOut = "*** PRESENT Key Schedule ******************************\n";
            txtOut += "*** (c) 2008 by Timm Korte, CrypTool@easycrypt.de******\n\n";
            txtOut += string.Format("Input Key:  {0:x16} {1:x4}\n", cipher.KeyRegs[0, 1, 3], cipher.KeyRegs[0, 0, 3]);
            txtOut += string.Format("Subkey Round 1: >>{0:x16}<<\n\n", cipher.RoundKeys[0]);

            for (int r = 1; r < 32; r++)
            {
                txtOut += string.Format("...after Shift: {0:x16} {1:x4}\n", cipher.KeyRegs[r, 1, 1], cipher.KeyRegs[r, 0, 1]);
                txtOut += string.Format("...after S-Box: {0:x16} {1:x4}\n", cipher.KeyRegs[r, 1, 2], cipher.KeyRegs[r, 0, 2]);
                txtOut += string.Format("Subkey Round {2} (after Salt): >>{0:x16}<< {1:x4}\n\n", cipher.KeyRegs[r, 1, 3], cipher.KeyRegs[r, 0, 3], r + 1);
            }
            txtOut += "*** PRESENT Encryption ********************************\n\n";
            txtOut += string.Format("Input Key:  {0:x16} {1:x4}\n", cipher.KeyRegs[0, 1, 3], cipher.KeyRegs[0, 0, 3]);
            txtOut += string.Format("Input Data: {0:x16}\n\n", cipher.States[0, 0]);

            for (int r = 0; r < 31; r++)
            {
                txtOut += string.Format("Round {0}\n", r + 1);
                txtOut += string.Format("  Subkey: >>{0:x16}<<\n", cipher.RoundKeys[r]);
                txtOut += "Text after...\n";
                txtOut += string.Format("...Key XOR: {0:x16}\n", cipher.States[r, 1]);
                txtOut += string.Format("...  S-Box: {0:x16}\n", cipher.States[r, 2]);
                txtOut += string.Format("...P-Layer: {0:x16}\n\n", cipher.States[r, 3]);
            }

            txtOut += "Final Round (32):\n";
            txtOut += string.Format("  Subkey: >>{0:x16}<<\n", cipher.RoundKeys[31]);
            txtOut += "Text after...\n";
            txtOut += string.Format("...Key XOR: {0:x16}\n\n", cipher.States[31, 1]);
            txtOut += "Input:\n";
            txtOut += string.Format("       Key: {0:x16} {1:x4}\n", cipher.KeyRegs[0, 1, 2], cipher.KeyRegs[0, 0, 2]);
            txtOut += string.Format("      Data: {0:x16}\n", cipher.States[0, 0]);
            txtOut += "Output:\n";
            txtOut += string.Format("    Cipher: {0:x16}\n", cipher.States[31, 1]);
            return txtOut;
        }
    }
}
