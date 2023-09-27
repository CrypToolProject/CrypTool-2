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
using M209AnalyzerLib.Common;
using M209AnalyzerLib.Enums;
using System;
using System.Linq;
using System.Text;

namespace M209AnalyzerLib.M209
{
    public class Key
    {
        public static readonly int WHEELS = 6;
        public static int BARS = 27;
        public static readonly int LUGS_PER_BAR = 2;
        public static readonly string[] WHEEL_LETTERS = {
            "????????????????????????????",
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
            //"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvxyz",
            //   ZYXWVUTSRQPONMLKJIHGFEDCBA
            "ABCDEFGHIJKLMNOPQRSTUVXYZ",
            //   ZYXWVUTSRQPONMLKJIHGFEDCBA
            "ABCDEFGHIJKLMNOPQRSTUVX",
            //   ZYXWVUTSRQPONMLKJIHGFEDCBA
            "ABCDEFGHIJKLMNOPQRSTU",
            //   ZYXWVUTSRQPONMLKJIHGFEDCBA
            "ABCDEFGHIJKLMNOPQRS",
            //   ZYXWVUTSRQPONMLKJIHGFEDCBA
            "ABCDEFGHIJKLMNOPQ",
            //   ZYXWVUTSRQPONMLKJIHGFEDCBA

        };

        //38 41 42 43 46 47
        public static readonly string[] WHEEL_LETTERS_C52 = {
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ6789012345[]{}<>@#$&*",
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ6789012345[]{}<>@#$&*",
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ6789012345[]{}<>@#$&",
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ6789012345[]{}<>@",
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ6789012345[]{}<>",
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ6789012345[]{}<",
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ6789012345[]",
        };

        public static readonly int[] WHEELS_SIZE = {
            -1, WHEEL_LETTERS[1].Length, WHEEL_LETTERS[2].Length,
            WHEEL_LETTERS[3].Length, WHEEL_LETTERS[4].Length,
            WHEEL_LETTERS[5].Length, WHEEL_LETTERS[6].Length
        };

        public static readonly string Ax6 = "AAAAAA";
        public static readonly string Ax26 = "AAAAAAAAAAAAAAAAAAAAAAAAAA";
        public static readonly char[] WHEELS_ACTIVE_PINS = "?PONMLK".ToCharArray();
        public static readonly string NULL_INDICATOR = "LLKJIH";
        private static int SPACE = Common.Utils.Z;
        private readonly static int S1 = WHEELS_SIZE[1];
        private readonly static int S2 = WHEELS_SIZE[2];
        private readonly static int S3 = WHEELS_SIZE[3];
        private readonly static int S4 = WHEELS_SIZE[4];
        private readonly static int S5 = WHEELS_SIZE[5];
        private readonly static int S6 = WHEELS_SIZE[6];

        private readonly static int M1 = 0x1;
        private readonly static int M2 = M1 << 1;
        private readonly static int M3 = M2 << 1;
        private readonly static int M4 = M3 << 1;
        private readonly static int M5 = M4 << 1;
        private readonly static int M6 = M5 << 1;

        public Pins Pins;
        public Lugs Lugs;
        public int Slide = 0;
        private int slidePlus25 = (0 + 25) % 26;

        public int[] Decryption { get => decryption; }

        private int[] decryption = null;
        private bool decryptionValid = false;
        private int[] decryptionFrequency = new int[26];

        public string CipherText
        {
            get => cipherText; set => SetCipherText(value);
        }
        private string cipherText = "";
        public string Crib { get; set; } = "";
        public int[] CipherArray { get; set; }
        public int[] CribArray { get; set; }

        private Key _originalKey;
        public Key OriginalKey { get => _originalKey; }
        public double OriginalScore;

        public static long evaluations = 0;

        public Key(string[] pinsString, string indicator, string lugsString)
        {
            Pins = new Pins(this, pinsString, indicator);
            if ((lugsString == null) || string.IsNullOrEmpty(lugsString))
            {
                Lugs = new Lugs(this);
            }
            else
            {
                Lugs = new Lugs(this, lugsString);
            }
            InvalidateDecryption();
        }

        public Key(string[] pinsString, string lugsString) : this(pinsString, lugsString, null) { }

        public Key() : this(null, NULL_INDICATOR, null) { }

        public Key(Key key) : this(key.Pins.AbsolutePinsStringArray(), key.Pins.Indicator, key.Lugs.GetLugsString())
        {
            this.CipherArray = key.CipherArray;
            this.CribArray = key.CribArray;
            this.Slide = key.Slide;
            this.Crib = key.Crib;
            this.CipherText = key.CipherText;
            this._originalKey = key._originalKey;
            this.OriginalScore = key.OriginalScore;
        }

        public string EncryptDecrypt(string In, bool encrypt)
        {

            StringBuilder s = new StringBuilder();
            for (int pos = 0; pos < In.Length; pos++)
            {
                char c = In.ElementAt(pos);
                if (c == '?')
                {
                    s.Append('?');
                }
                else
                {
                    int cin;
                    if (encrypt && (c == ' '))
                    {
                        cin = SPACE;
                    }
                    else
                    {
                        cin = c - 'A';
                    }
                    int disp = CalcDisplacement(pos);
                    int cout = EncryptSymbol(cin, disp);
                    if (!encrypt && (cout == SPACE))
                    {
                        s.Append(' ');
                    }
                    else
                    {
                        s.Append((char)('A' + cout));
                    }
                }
            }

            return s.ToString();
        }

        private int CalcDisplacement(int pos)
        {

            return Lugs.DisplacementVector[(Pins.WheelPins1[pos % S1] ? M1 : 0)
                    + (Pins.WheelPins2[pos % S2] ? M2 : 0)
                    + (Pins.WheelPins3[pos % S3] ? M3 : 0)
                    + (Pins.WheelPins4[pos % S4] ? M4 : 0)
                    + (Pins.WheelPins5[pos % S5] ? M5 : 0)
                    + (Pins.WheelPins6[pos % S6] ? M6 : 0)];

        }

        public int EvalMono()
        {
            UpdateDecryptionIfInvalid();

            int mono = 0;
            for (int i = 0; i < 26; i++)
            {
                int f = decryptionFrequency[i];
                mono += (int)Stats.MonogramStats[i] * f;
                //mono += Stats.monogramStatsLinear[i] * f;
            }
            return mono / CipherArray.Length;
        }

        private int EvalADE()
        {

            UpdateDecryptionIfInvalid();

            int sumScore = 0;
            int missing = 0;

            int actual;
            int expected;

            for (int cribIndex = 0; cribIndex < CribArray.Length; cribIndex++)
            {

                expected = CribArray[cribIndex];
                if (expected == -1)
                {
                    missing++;
                    continue;
                }
                actual = decryption[cribIndex];

                int dist1 = Math.Abs(expected - actual);
                int dist2 = Math.Abs(expected + 26 - actual);
                sumScore += 26 - Math.Min(dist1, dist2);
            }

            return 5000 * sumScore / (CribArray.Length - missing);

        }

        public int Eval(EvalType evalType)
        {

            evaluations++;

            switch (evalType)
            {
                case EvalType.CRIB:
                    return EvalADE();
                case EvalType.MONO:
                    return EvalMono();
                default:
                    Console.WriteLine($"Unsupported eval type {evalType}");
                    return 0;
            }

        }

        public static void PrintCounter()
        {
            Console.WriteLine($"Number of evaluations: {evaluations}\n");
        }

        public static void ResetCounter()
        {
            evaluations = 0;
        }

        public static bool GetWheelBit(int bitmap, int w)
        {
            return ((bitmap >> (w - 1)) & 0x1) == 0x1;
        }

        public void InvalidateDecryption()
        {
            decryptionValid = false;
        }

        public void UpdateDecryptionIfInvalid()
        {
            if (!decryptionValid)
            {
                UpdateDecryption();
            }
        }

        private void UpdateDecryption()
        {

            if (CipherArray == null)
            {
                return;
            }
            if ((decryption == null) || (decryption.Length != CipherArray.Length))
            {
                decryption = new int[CipherArray.Length];
            }

            /*final*/
            int len = (CribArray != null) ? CribArray.Length : CipherArray.Length;
            int pos1 = 0, pos2 = 0, pos3 = 0, pos4 = 0, pos5 = 0, pos6 = 0;
            int vector;
            int symbol;
            for (int pos = 0; pos < len; pos++)
            {
                vector = 0;
                if (Pins.WheelPins1[pos1]) vector += M1;
                if (Pins.WheelPins2[pos2]) vector += M2;
                if (Pins.WheelPins3[pos3]) vector += M3;
                if (Pins.WheelPins4[pos4]) vector += M4;
                if (Pins.WheelPins5[pos5]) vector += M5;
                if (Pins.WheelPins6[pos6]) vector += M6;


                symbol = slidePlus25 - CipherArray[pos] + Lugs.DisplacementVector[vector];
                symbol = symbol % 26;

                //displacement = lugs.displacementVector[vector];
                //symbol = encryptSymbol(cipherArray[pos], displacement);

                decryption[pos] = symbol;

                if (++pos1 == Key.S1) pos1 = 0;
                if (++pos2 == Key.S2) pos2 = 0;
                if (++pos3 == Key.S3) pos3 = 0;
                if (++pos4 == Key.S4) pos4 = 0;
                if (++pos5 == Key.S5) pos5 = 0;
                if (++pos6 == Key.S6) pos6 = 0;
            }

            decryptionValid = true;

        }

        public void UpdateDecryption(int w1, int p1)
        {

            if (!decryptionValid)
            {
                UpdateDecryption();
                return;
            }

            if (CipherArray == null)
            {
                return;
            }
            if ((decryption == null) || (decryption.Length != CipherArray.Length))
            {
                UpdateDecryption();
                return;
            }

            UpdateDecryptionSelectedPin(w1, p1);

        }

        public void UpdateDecryption(int w, int p1, int p2)
        {

            if (!decryptionValid)
            {
                UpdateDecryption();
                return;
            }

            if (CipherArray == null)
            {
                return;
            }
            if ((decryption == null) || (decryption.Length != CipherArray.Length))
            {
                decryption = new int[CipherArray.Length];
            }

            UpdateDecryptionSelectedPin(w, p1);
            UpdateDecryptionSelectedPin(w, p2);

        }

        private void UpdateDecryptionSelectedPin(int wheelNr, int p)
        {
            /*final*/
            int len = (CribArray != null) ? CribArray.Length : CipherArray.Length;
            int wheelSize = WHEELS_SIZE[wheelNr];
            int vector;
            int symbol;
            int pos1 = p, pos2 = p, pos3 = p, pos4 = p, pos5 = p, pos6 = p;
            while (pos1 >= Key.S1)
            {
                pos1 -= Key.S1;
            }
            while (pos2 >= Key.S2)
            {
                pos2 -= Key.S2;
            }
            while (pos3 >= Key.S3)
            {
                pos3 -= Key.S3;
            }
            while (pos4 >= Key.S4)
            {
                pos4 -= Key.S4;
            }
            while (pos5 >= Key.S5)
            {
                pos5 -= Key.S5;
            }
            while (pos6 >= Key.S6)
            {
                pos6 -= Key.S6;
            }

            // Performance improvement because of using local copys ? https://blog.tedd.no/2020/06/01/faster-c-array-access/
            var pwp1 = Pins.WheelPins1;
            var pwp2 = Pins.WheelPins2;
            var pwp3 = Pins.WheelPins3;
            var pwp4 = Pins.WheelPins4;
            var pwp5 = Pins.WheelPins5;
            var pwp6 = Pins.WheelPins6;

            var ca = CipherArray;
            var ldv = Lugs.DisplacementVector;
            for (int pos = p; pos < len; pos += wheelSize)
            {

                vector = 0;
                if (pwp1[pos1]) vector += M1;
                if (pwp2[pos2]) vector += M2;
                if (pwp3[pos3]) vector += M3;
                if (pwp4[pos4]) vector += M4;
                if (pwp5[pos5]) vector += M5;
                if (pwp6[pos6]) vector += M6;
                //displacement = lugs.displacementVector[vector];

                //symbol = encryptSymbol(cipherArray[pos], displacement);
                symbol = slidePlus25 - ca[pos] + ldv[vector];
                if (symbol >= 26)
                {
                    symbol -= 26;
                }

                decryption[pos] = symbol;

                for (pos1 += wheelSize; pos1 >= Key.S1; pos1 -= Key.S1) ;
                for (pos2 += wheelSize; pos2 >= Key.S2; pos2 -= Key.S2) ;
                for (pos3 += wheelSize; pos3 >= Key.S3; pos3 -= Key.S3) ;
                for (pos4 += wheelSize; pos4 >= Key.S4; pos4 -= Key.S4) ;
                for (pos5 += wheelSize; pos5 >= Key.S5; pos5 -= Key.S5) ;
                for (pos6 += wheelSize; pos6 >= Key.S6; pos6 -= Key.S6) ;
            }
        }

        public void SetCipherTextAndCrib(string cipher, string crib)
        {
            InvalidateDecryption();
            SetCipherText(cipher);

            this.Crib = crib;
            CribArray = new int[crib.Length];
            for (int pos = 0; pos < crib.Length; pos++)
            {
                int ccrib = crib.ElementAt(pos) - 'A';
                if ((ccrib < 0) || (ccrib > 25))
                {
                    ccrib = -1;
                }
                CribArray[pos] = ccrib;
            }
        }

        public void SetCipherText(string cipher)
        {
            InvalidateDecryption();
            this.cipherText = cipher;
            CipherArray = new int[cipher.Length];
            for (int pos = 0; pos < cipher.Length; pos++)
            {
                int ccipher = cipher.ElementAt(pos) - 'A';
                if ((ccipher < 0) || (ccipher > 25))
                {
                    ccipher = -1;
                }
                CipherArray[pos] = ccipher;
            }
        }

        public void setOriginalKey(Key originalKey)
        {
            _originalKey = new Key(originalKey);
        }

        public void setOriginalScore(double originalScore)
        {
            OriginalScore = originalScore;
        }

        public int GetCountIncorrectPins()
        {
            if (_originalKey == null)
            {
                return 0;
            }
            int total = 0;
            for (int w = 1; w <= Key.WHEELS; w++)
            {
                total += GetCountIncorrectPins(w);
            }
            return total;
        }

        public int GetCountIncorrectPins(int w1)
        {
            if (_originalKey == null)
            {
                return 0;
            }
            int incorrect = 0;

            for (int i = 0; i < Key.WHEELS_SIZE[w1]; i++)
            {
                if (Pins.IsoPins[w1][i] != _originalKey.Pins.IsoPins[w1][i])
                {
                    incorrect++;
                }
            }

            return Math.Min(incorrect, Key.WHEELS_SIZE[w1] - incorrect);
        }

        public int GetCountIncorrectLugs()
        {
            if (_originalKey == null)
            {
                return 0;
            }
            int incorrect = 0;
            int[] typeCount = Lugs.CreateTypeCountCopy();
            for (int i = 0; i < Lugs.TYPE_COUNT_ARRAY_SIZE; i++)
            {
                int error = Math.Abs(_originalKey.Lugs.TypeCount[i] - typeCount[i]);
                incorrect += error;
            }
            return incorrect / 2;
        }


        private int EncryptSymbol(int input, int disp)
        {
            int val;
            for (val = 25 - input + Slide + disp; val >= 26; val -= 26) ;
            return val;
        }

        public override string ToString()
        {
            return $"[Slide {Slide}] [{Lugs.GetLugsString()}] [{Pins.AbsolutePinStringAll01()}]";
        }

    }
}
