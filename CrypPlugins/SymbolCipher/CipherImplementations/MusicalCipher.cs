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

namespace CrypTool.Plugins.SymbolCipher.CipherImplementations
{
    /// <summary>
    /// Definitions based on ciphers shown in Klaus Schmeh's blog article: 
    /// https://scienceblogs.de/klausis-krypto-kolumne/2020/10/14/a-musical-cryptogram-from-1854/
    /// </summary>
    public enum MusicalCipherType
    {
        DanielSchwenter,
        JohnWilkins/*,
        GustavusSelenus*/
    }

    public enum NoteType
    {
        Barline,
        Ellipse,
        FilledEllipse,
        Rhombos,
        Square,
        SquareWithLine,
        Ellipse90Degrees,
        Ellipse90DegreesWithLine
    }

    public class MusicalCipher : ASymbolCipher
    {
        private readonly MusicalCipherType _musicalCipherType;
        private readonly double _staffAndBarLineThickness;
        private readonly double _NoteLineThickness;

        public const int NUMBER_OF_STAFFS = 8;
        public const int NOTE_ANGLE = 45;

        private const string KEY_DANIEL_SCHWENTER   = "BACDEFGHIKLMNOYZRSTUWXQP";
        private const string KEY_JOHN_WILKINS       = "ABCDEIGHFLMNOPRSUTWXYZ";

        /// <summary>
        /// Generates a Musical Cipher which allows to encrypt text into notes
        /// </summary>
        /// <param name="musicalCipherType"></param>
        /// <param name="dpi"></param>
        public MusicalCipher(MusicalCipherType musicalCipherType, int dpi = 300) : base(dpi)
        {
            _musicalCipherType = musicalCipherType;
            _staffAndBarLineThickness = 0.01 * _dpi;
            _NoteLineThickness = 0.025 * _dpi;            
        }

        /// <summary>
        /// Encrypts the given plaintext using the 
        /// </summary>
        /// <param name="plaintext"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override Image Encrypt(string plaintext, string key = null)
        {
            switch (_musicalCipherType)
            {
                case MusicalCipherType.DanielSchwenter:
                    return EncryptDanielSchwenter(plaintext, key);
                case MusicalCipherType.JohnWilkins:
                    return EncryptJohnWilkins(plaintext, key);
                /*case MusicalCipherType.GustavusSelenus:
                    return EncryptGustavusSelenus(plaintext, key);*/
                default:
                    throw new NotImplementedException(string.Format("MusicalCipherType {0} not implemented", _musicalCipherType));
            }
        }

        /// <summary>
        /// Generates the key image of this musical cipher
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override Image GenerateKeyImage(string key)
        {
            switch (_musicalCipherType)
            {
                case MusicalCipherType.DanielSchwenter:
                    return GenerateKeyImageDanielSchwenter(key);
                case MusicalCipherType.JohnWilkins:
                    return GenerateKeyImageJohnWilkins(key);
                /*case MusicalCipherType.GustavusSelenus:
                    return GenerateKeyImageGustavusSelenus(key);*/
                default:
                    throw new NotImplementedException(string.Format("MusicalCipherType {0} not implemented", _musicalCipherType));
            }
        }

        /// <summary>
        /// Generates a bitmap using the "Daniel Schwenter encryption scheme". It is also possible to provide an own key (alphabet)
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private Image EncryptDanielSchwenter(string plaintext, string key = null)
        {
            //we only work on uppercase letters
            plaintext = plaintext.ToUpper();

            //if key is empty, we use the original Daniel Schwenter scheme:
            if (string.IsNullOrEmpty(key))
            {
                key = KEY_DANIEL_SCHWENTER;
                //we have U=V and I=J
                plaintext = plaintext.Replace("V", "U").Replace("J", "I");
            }

            Image image = new Image((int)Math.Round(A4_WIDTH) * _dpi, (int)Math.Round(A4_HEIGHT) * _dpi); // we loose some pixels due to the casts here...
            image.ClearImage(0xFF, 0xFF, 0xFF);

            //draw all staffs
            for (int staffNumber = 0; staffNumber < NUMBER_OF_STAFFS; staffNumber++)
            {
                DrawStaff(image, staffNumber);
            }

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

        private Image GenerateKeyImageDanielSchwenter(string key)
        {
            return null;
        }

        /// <summary>
        /// Generates a bitmap using the "John Wilkins encryption scheme". It is also possible to provide an own key (alphabet)
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private Image EncryptJohnWilkins(string plaintext, string key = null)
        {
            //we only work on uppercase letters
            plaintext = plaintext.ToUpper();

            //if key is empty, we use the original Daniel Schwenter scheme:
            if (string.IsNullOrEmpty(key))
            {
                key = KEY_JOHN_WILKINS;
                //we have U=V, I=J, K=C, Q=O
                plaintext = plaintext.Replace("V", "U").Replace("J", "I").Replace("K","C").Replace("Q","O");
            }

            Image image = new Image((int)Math.Round(A4_WIDTH) * _dpi, (int)Math.Round(A4_HEIGHT) * _dpi); // we loose some pixels due to the casts here...
            image.ClearImage(0xFF, 0xFF, 0xFF);

            //draw all staffs
            for (int staffNumber = 0; staffNumber < NUMBER_OF_STAFFS; staffNumber++)
            {
                DrawStaff(image, staffNumber);
            }

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
                else if (index >= 0 && index < 22)
                {
                    if(index % 2 == 0)
                    {
                        DrawNote(image, staffNo, 9 - index / 2, noteNumber, NoteType.Ellipse90Degrees);
                    }
                    else
                    {
                        DrawNote(image, staffNo, 9 - index / 2, noteNumber, NoteType.Ellipse90DegreesWithLine);
                    }                    
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

        private Image GenerateKeyImageJohnWilkins(string key)
        {
            return null;
        }

        /*private Image EncryptGustavusSelenus(string plaintext, string key = null)
        {
            return null;
        }

        private Image GenerateKeyImageGustavusSelenus(string key)
        {
            return null;
        }*/

        /// <summary>
        /// Draws the staff identified by staffNumber with 5 lines 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="staffNumber"></param>
        private void DrawStaff(Image image, int staffNumber)
        {
            if(staffNumber > 7)
            {
                return;
            }
            int staffTop = (int)(1.4 * staffNumber * _dpi + 0.8 * _dpi);
            int lineSpace = (int)(0.16 * _dpi);
            int spacingLeftRight = (int)(0.2 * _dpi);

            for (int lineNumber = 0; lineNumber < 5; lineNumber++)
            {
                image.DrawLine(spacingLeftRight, staffTop + lineNumber * lineSpace, image.Width - spacingLeftRight, staffTop + lineNumber * lineSpace, (int)_staffAndBarLineThickness, 0x00, 0x00, 0x00);
            }
        }

        /// <summary>
        /// Draws a note of noteType at the position defined by staffNumber, lineNumber, and noteNumber
        /// </summary>
        /// <param name="image"></param>
        /// <param name="staffNumber"></param>
        /// <param name="halfLineNumber"></param>
        /// <param name="noteNumber"></param>
        /// <param name="noteType"></param>
        private void DrawNote(Image image, int staffNumber, int halfLineNumber, int noteNumber, NoteType noteType)
        {
            if(staffNumber > 7)
            {
                return;
            }
            //our lowest note starts at the bottom
            halfLineNumber = 10 - halfLineNumber;

            //Calculate position of the note
            int staffTop = (int)(1.4 * staffNumber * _dpi + (0.8 - 0.16) * _dpi);
            int lineSpace = (int)(0.16 * _dpi);
            int spacingLeftRight = (int)(0.4 * _dpi);
            int noteX = spacingLeftRight + (int)(noteNumber * 0.2 * _dpi);
            int noteY = staffTop + (halfLineNumber + 1) * lineSpace / 2;         

            double radius = 0.07 * _dpi;
            switch (noteType)
            {
                case NoteType.Barline: // of course this is not a "note" but we still draw it here :-)
                    {
                        (int x, int y) top = (noteX, staffTop + lineSpace);
                        (int x, int y) bottom = (noteX, staffTop + 5 * lineSpace);
                        image.DrawLine(top.x, top.y, bottom.x, bottom.y, (int)(_staffAndBarLineThickness), 0x00, 0x00, 0x00);
                    }
                    break;
                case NoteType.Ellipse:
                    //we draw the ellipse with a specific thickness using this loop here                    
                    image.DrawEllipse(noteX, noteY, radius * 1.2, radius * 1, NOTE_ANGLE, (int)_NoteLineThickness, 0x00, 0x00, 0x00);
                    break;
                case NoteType.FilledEllipse:
                    image.DrawFilledEllipse(noteX, noteY, radius * 1.2, radius, NOTE_ANGLE, 0x00, 0x00, 0x00);
                    break;
                case NoteType.Rhombos:
                    {
                        (int x, int y) top = (noteX, noteY - (int)radius);
                        (int x, int y) bottom = (noteX, noteY + (int)radius);
                        (int x, int y) left = (noteX - (int)radius, noteY);
                        (int x, int y) right = (noteX + (int)radius, noteY);
                        image.DrawLine(top.x, top.y, left.x, left.y, (int)(_NoteLineThickness / 1.5), 0x00, 0x00, 0x00);
                        image.DrawLine(top.x, top.y, right.x, right.y, (int)_NoteLineThickness, 0x00, 0x00, 0x00);
                        image.DrawLine(left.x, left.y, bottom.x, bottom.y, (int)_NoteLineThickness, 0x00, 0x00, 0x00);
                        image.DrawLine(right.x, right.y, bottom.x, bottom.y, (int)(_NoteLineThickness / 1.5), 0x00, 0x00, 0x00);
                    }
                    break;
                case NoteType.SquareWithLine:
                case NoteType.Square:
                    (int x, int y) topleft = (noteX - (int)radius, noteY - (int)radius);
                    (int x, int y) topright = (noteX + (int)radius, noteY - (int)radius);
                    (int x, int y) bottomleft = (noteX - (int)radius, noteY + (int)radius);
                    (int x, int y) bottomright = (noteX + (int)radius, noteY + (int)radius);
                    image.DrawLine(topleft.x, topleft.y, topright.x, topright.y, (int)_NoteLineThickness, 0x00, 0x00, 0x00);
                    image.DrawLine(bottomleft.x, bottomleft.y, bottomright.x, bottomright.y, (int)_NoteLineThickness, 0x00, 0x00, 0x00);
                    image.DrawLine(topleft.x, topleft.y - (int)(radius * 0.3), bottomleft.x, bottomleft.y + (int)(radius * 0.3), (int)(_NoteLineThickness / 1.5), 0x00, 0x00, 0x00);
                    image.DrawLine(topright.x, topright.y - (int)(radius * 0.3), bottomright.x, bottomright.y + (int)(radius * 0.3), (int)(_NoteLineThickness / 1.5), 0x00, 0x00, 0x00);
                    if (noteType == NoteType.SquareWithLine)
                    {
                        if (halfLineNumber >= 6)
                        {
                            //line goes up
                            image.DrawLine(topright.x, topright.y - (int)(radius * 4.5), bottomright.x, bottomright.y + (int)(radius * 0.3), (int)(_NoteLineThickness / 1.5), 0x00, 0x00, 0x00);
                        }
                        else
                        {
                            //line goes doen
                            image.DrawLine(bottomright.x, bottomright.y + (int)(radius * 4.5), topright.x, topright.y + (int)(radius * 0.3), (int)(_NoteLineThickness / 1.5), 0x00, 0x00, 0x00);
                        }
                    }
                    break;
                case NoteType.Ellipse90Degrees: //needed for John Wilkins
                case NoteType.Ellipse90DegreesWithLine: //needed for John Wilkins            
                    {                                                
                        image.DrawEllipse(noteX, noteY, radius * 1.2, radius * 1, 90, (int)_NoteLineThickness, 0x00, 0x00, 0x00);
                        if(halfLineNumber > 10)
                        {
                            (int x, int y) left = (noteX - (int)(3 * radius), noteY);
                            (int x, int y) right = (noteX + (int)(3 * radius), noteY);
                            image.DrawLine(left.x, left.y, right.x, right.y, (int)(_staffAndBarLineThickness), 0x00, 0x00, 0x00);
                        }
                        if(noteType == NoteType.Ellipse90DegreesWithLine)
                        {
                            if (halfLineNumber > 4)
                            {
                                (int x, int y) top = (noteX, noteY - (int)(5 * radius));
                                (int x, int y) bottom = (noteX, noteY - (int)(1 * radius));
                                image.DrawLine(top.x, top.y, bottom.x, bottom.y, (int)(_NoteLineThickness), 0x00, 0x00, 0x00);
                            }
                            else
                            {
                                (int x, int y) top = (noteX, noteY + (int)(5 * radius));
                                (int x, int y) bottom = (noteX, noteY + (int)(1 * radius));
                                image.DrawLine(top.x, top.y, bottom.x, bottom.y, (int)(_NoteLineThickness), 0x00, 0x00, 0x00);
                            }
                        }                        
                    }
                    break;
            }
        }
    }
}
