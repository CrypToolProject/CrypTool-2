/*
   Copyright 2022 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrypTool.Plugins.SymbolCipher.CipherImplementations;

namespace CrypTool.Plugins.SymbolCipher
{
    public class PigpenCipher : ASymbolCipher
    {
        private const string KEY_PIGPEN = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public override Image Encrypt(string plaintext, string key = null)
        {
            if(string.IsNullOrEmpty(key))
            {
                key = KEY_PIGPEN;
            }

            return EncryptPigpen(plaintext, key);
        }

        private Image EncryptPigpen(string plaintext, string key)
        {
            //we only work on uppercase letters
            plaintext = plaintext.ToUpper();         

            Image image = new Image((int)Math.Round(A4_WIDTH) * _dpi, (int)Math.Round(A4_HEIGHT) * _dpi); // we loose some pixels due to the casts here...
            image.ClearImage(0xFF, 0xFF, 0xFF);

            //draw notes
            int staffNo = 0;
            int noteNumber = 0;
            foreach (char symbol in plaintext)
            {
                int index = key.IndexOf(symbol);
                if (index == -1) // symbol is not part of the alphabet
                {
                    if (symbol == '\n') // line break
                    {
                        staffNo++;
                        noteNumber = -1;
                    }
                    else if (symbol == '|') // bar line
                    {
                        DrawNote(image, staffNo, 2, noteNumber, NoteType.Barline);
                    }
                    else if (symbol == ' ') // space
                    {
                        //empty note; do nothing
                    }
                    else //unkown symbol we can't handle
                    {
                        //move one back, so we don't draw and move at all
                        noteNumber--;
                        if (noteNumber == -1)
                        {
                            noteNumber = 37;
                            staffNo--;
                        }
                    }
                }
                else if (index >= 0 && index < 11)
                {
                    //notes first go up
                    DrawNote(image, staffNo, index, noteNumber, NoteType.Rhombos);
                }
                else if (index >= 11 && index < 22)
                {
                    //the notes go down
                    index = 21 - index;
                    DrawNote(image, staffNo, index, noteNumber, NoteType.Square);
                }
                else if (index >= 22 && index < 33)
                {
                    //and finally they go up again
                    index = index - 21;
                    DrawNote(image, staffNo, index, noteNumber, NoteType.SquareWithLine);
                }

                noteNumber++;
                if (noteNumber == 38) // we can display a maximum of 38 notes
                {
                    noteNumber = 0;
                    staffNo++;
                }
            }

            return image;
        }

        private void DrawNote(Image image, int staffNo, int v, int noteNumber, NoteType barline)
        {
            
        }

        public override Image GenerateKeyImage(string key)
        {
            return null;
        }
    }
}
