using System;
using System.Collections.Generic;

namespace CrypTool.Plugins.SZ42
{
    /// <summary>
    /// Class that represents a SZ42 machine cipher
    /// </summary>
    [Serializable]
    public class SZ42
    {
        #region Private Variables

        private char basicMotor, totalMotor, limitation;
        private bool theresLimitation;

        //Chi wheels of the SZ42
        private Wheel[] chiWheels;

        //Psi wheels of the SZ42
        private Wheel[] psiWheels;

        //Mu wheels of the SZ42
        private Wheel[] muWheels;

        //This dictionary contains the badout code with 
        //the corresponding character e.g. T -> ...x
        private readonly Dictionary<char, string> baudotCode = new Dictionary<char, string>();

        //This dictionary contains character with 
        //the corresponding badout code e.g. ...x -> T
        private readonly Dictionary<string, char> charCode = new Dictionary<string, char>();

        //This dictionary contains figures with
        //the corresponding letters e.g.  + -> Z
        private readonly Dictionary<char, char> charToFigures = new Dictionary<char, char>();

        //This dictionary contains figures with
        //the corresponding letters e.g.  Z -> +
        private readonly Dictionary<char, char> figuresToChar = new Dictionary<char, char>();

        //List that contains a trace of the encrypt
        private List<string> trace;

        #endregion

        public Wheel[] ChiWheels
        {
            get => chiWheels;
            set => chiWheels = value;
        }

        public Wheel[] PsiWheels
        {
            get => psiWheels;
            set => psiWheels = value;
        }

        public Wheel[] MuWheels
        {
            get => muWheels;
            set => muWheels = value;
        }

        public Dictionary<char, string> BaudotCode => baudotCode;

        public Dictionary<string, char> CharCode => charCode;

        /// <summary>
        /// Public property of the trace
        /// </summary>
        public List<string> Trace
        {
            get => trace;
            set => trace = value;
        }

        public bool TheresLimitation
        {
            get => theresLimitation;
            set => theresLimitation = value;
        }

        /// <summary>
        /// The constructor where the 
        /// badout code's dictionary is
        /// initialized
        /// </summary>
        public SZ42()
        {
            trace = new List<string>();

            #region Baudot Code

            baudotCode.Add('/', ".....");
            baudotCode.Add('.', "..x..");
            baudotCode.Add('H', "..x.x");
            baudotCode.Add('T', "....x");
            baudotCode.Add('O', "...xx");
            baudotCode.Add('M', "..xxx");
            baudotCode.Add('N', "..xx.");
            baudotCode.Add('3', "...x.");
            baudotCode.Add('R', ".x.x.");
            baudotCode.Add('C', ".xxx.");
            baudotCode.Add('V', ".xxxx");
            baudotCode.Add('G', ".x.xx");
            baudotCode.Add('L', ".x..x");
            baudotCode.Add('P', ".xx.x");
            baudotCode.Add('I', ".xx..");
            baudotCode.Add('4', ".x...");
            baudotCode.Add('A', "xx...");
            baudotCode.Add('U', "xxx..");
            baudotCode.Add('Q', "xxx.x");
            baudotCode.Add('W', "xx..x");
            baudotCode.Add('+', "xx.xx");
            baudotCode.Add('-', "xxxxx");
            baudotCode.Add('K', "xxxx.");
            baudotCode.Add('J', "xx.x.");
            baudotCode.Add('D', "x..x.");
            baudotCode.Add('F', "x.xx.");
            baudotCode.Add('X', "x.xxx");
            baudotCode.Add('B', "x..xx");
            baudotCode.Add('Z', "x...x");
            baudotCode.Add('Y', "x.x.x");
            baudotCode.Add('S', "x.x..");
            baudotCode.Add('E', "x....");

            #endregion

            #region Characters Code

            charCode.Add(".....", '/');
            charCode.Add("..x..", '.');
            charCode.Add("..x.x", 'H');
            charCode.Add("....x", 'T');
            charCode.Add("...xx", 'O');
            charCode.Add("..xxx", 'M');
            charCode.Add("..xx.", 'N');
            charCode.Add("...x.", '3');
            charCode.Add(".x.x.", 'R');
            charCode.Add(".xxx.", 'C');
            charCode.Add(".xxxx", 'V');
            charCode.Add(".x.xx", 'G');
            charCode.Add(".x..x", 'L');
            charCode.Add(".xx.x", 'P');
            charCode.Add(".xx..", 'I');
            charCode.Add(".x...", '4');
            charCode.Add("xx...", 'A');
            charCode.Add("xxx..", 'U');
            charCode.Add("xxx.x", 'Q');
            charCode.Add("xx..x", 'W');
            charCode.Add("xx.xx", '+');
            charCode.Add("xxxxx", '-');
            charCode.Add("xxxx.", 'K');
            charCode.Add("xx.x.", 'J');
            charCode.Add("x..x.", 'D');
            charCode.Add("x.xx.", 'F');
            charCode.Add("x.xxx", 'X');
            charCode.Add("x..xx", 'B');
            charCode.Add("x...x", 'Z');
            charCode.Add("x.x.x", 'Y');
            charCode.Add("x.x..", 'S');
            charCode.Add("x....", 'E');

            #endregion

            #region Char To Figures

            charToFigures.Add('ε', 'H');
            charToFigures.Add('5', 'T');
            charToFigures.Add('9', 'O');
            charToFigures.Add('.', 'M');
            charToFigures.Add(',', 'N');
            charToFigures.Add('4', 'R');
            charToFigures.Add(':', 'C');
            charToFigures.Add('=', 'V');
            charToFigures.Add('@', 'G');
            charToFigures.Add(')', 'L');
            charToFigures.Add('0', 'P');
            charToFigures.Add('8', 'I');
            charToFigures.Add('-', 'A');
            charToFigures.Add('7', 'U');
            charToFigures.Add('1', 'Q');
            charToFigures.Add('2', 'W');
            charToFigures.Add('(', 'k');
            //figures.Add('ring bell', 'J');
            //figures.Add('Who are you?', 'D');
            charToFigures.Add('%', 'F');
            charToFigures.Add('/', 'X');
            charToFigures.Add('?', 'B');
            charToFigures.Add('+', 'Z');
            charToFigures.Add('6', 'Y');
            charToFigures.Add('\'', 'S');
            charToFigures.Add('3', 'E');

            #endregion

            #region Figures To Char

            figuresToChar.Add('H', 'ε');
            figuresToChar.Add('T', '5');
            figuresToChar.Add('O', '9');
            figuresToChar.Add('M', '.');
            figuresToChar.Add('N', ',');
            figuresToChar.Add('R', '4');
            figuresToChar.Add('C', ':');
            figuresToChar.Add('V', '=');
            figuresToChar.Add('G', '@');
            figuresToChar.Add('L', ')');
            figuresToChar.Add('P', '0');
            figuresToChar.Add('I', '8');
            figuresToChar.Add('A', '-');
            figuresToChar.Add('U', '7');
            figuresToChar.Add('Q', '1');
            figuresToChar.Add('W', '2');
            figuresToChar.Add('k', '(');
            //figuresToChar('ring bell', 'J');
            //figuresToChar('Who are you?', 'D');                    
            figuresToChar.Add('F', '%');
            figuresToChar.Add('X', '/');
            figuresToChar.Add('B', '?');
            figuresToChar.Add('Z', '+');
            figuresToChar.Add('Y', '6');
            figuresToChar.Add('S', '\'');
            figuresToChar.Add('E', '3');

            #endregion

            #region Wheels

            chiWheels = new Wheel[] {
                                        new Wheel ("χ1", 41),
                                        new Wheel ("χ2", 31),
                                        new Wheel ("χ3", 29),
                                        new Wheel ("χ4", 26),
                                        new Wheel ("χ5", 23),
                                    };

            psiWheels = new Wheel[] {
                                        new Wheel ("Ψ1", 43),
                                        new Wheel ("Ψ2", 47),
                                        new Wheel ("Ψ3", 51),
                                        new Wheel ("Ψ4", 53),
                                        new Wheel ("Ψ5", 59)
                                    };

            muWheels = new Wheel[] {
                                        new Wheel ("μ61", 61),
                                        new Wheel ("μ37", 37)
                                   };
            #endregion

            theresLimitation = true;

            //Chi2 ONE BACK for the limitation
            limitation = chiWheels[1].ActiveState;

            //the earlier active position of the mu37 
            //for the basic motor character
            basicMotor = muWheels[1].ActiveState;
        }

        public string ParseOutput(string raw)
        {
            bool beginFig = false;
            int i = 0;

            raw = raw.Replace('.', ' ').Replace('3', '\r').Replace('4', '\n');

            while (i < raw.Length)
            {
                if (raw[i] == '+')
                {
                    raw = raw.Remove(i, 1);
                    beginFig = true;
                }
                else if (raw[i] == '-')
                {
                    beginFig = false;
                    raw = raw.Remove(i, 1);
                }

                if (beginFig)
                {
                    raw = raw.Insert(i, figuresToChar[raw[i]].ToString());
                    raw = raw.Remove(i + 1, 1);
                }

                i++;
            }

            raw = raw.ToLower();

            return raw;
        }

        /// <summary>
        /// Parse the input
        /// </summary>
        public string ParseInput(string raw)
        {
            bool beginFig = true;
            int i = 0;
            char temp;

            while (i < raw.Length)
            {
                if (charToFigures.ContainsKey(raw[i]))
                {
                    if (beginFig)
                    {
                        raw = raw.Insert(i, "+");
                        i++;
                        beginFig = false;
                    }

                    raw = raw.Insert(i, charToFigures[raw[i]].ToString());
                    raw = raw.Remove(i + 1, 1);

                    if (i == raw.Length - 1)
                    {
                        temp = raw[i];
                        raw = raw.Remove(i, 1);
                        raw = raw.Insert(i, temp + "-");
                        i++;
                    }
                }

                if (!(i == raw.Length - 1))
                {
                    if (!beginFig && !charToFigures.ContainsKey(raw[i + 1]))
                    {
                        temp = raw[i];
                        raw = raw.Remove(i, 1);
                        raw = raw.Insert(i, temp + "-");
                        i++;
                        beginFig = true;
                    }
                }
                i++;
            }

            raw = raw.ToUpper().Replace(' ', '.').Replace('\r', '3').Replace('\n', '4');

            return raw;
        }

        /// <summary>
        /// Encrypt or Decrypt the string p
        /// </summary>
        public string ActionMachine(string p)
        {
            string z = "";

            foreach (char c in p)
            {
                if (baudotCode.ContainsKey(c))
                {
                    z += SumBaudot(baudotCode[c], Character(chiWheels), Character(psiWheels));

                    MoveWheels();
                }
                else
                {
                    z += c;
                }
            }

            return z;
        }

        /// <summary>
        /// with char1 and char2 returns the character
        /// that is generated by the XOR
        /// </summary>
        public char SumXOR(char char1, char char2)
        {
            char c;
            string b = "", string1, string2;

            string1 = baudotCode[char1];
            string2 = baudotCode[char2];

            for (int i = 0; i < 5; i++)
            {
                b += XOR(string1[i], string2[i]);
            }

            c = charCode[b];

            return c;
        }

        /// <summary>
        /// Controls the movement of the wheels
        /// in every step (when a each p character is enciphered)
        /// </summary>
        private void MoveWheels()
        {
            if (theresLimitation)
            {
                //Chi2 ONE BACK for the limitation
                limitation = chiWheels[1].ActiveState;
            }

            //Chiwheel move once
            foreach (Wheel chi in chiWheels)
            {
                chi.MoveOnce();
            }

            if (muWheels[0].ActiveState == 'x')
            {
                muWheels[1].MoveOnce();
            }

            //Mu61 move once
            muWheels[0].MoveOnce();

            //Determine if the psiwheels step once
            if (totalMotor == 'x')
            {
                foreach (Wheel psi in psiWheels)
                {
                    psi.MoveOnce();
                }
            }

            basicMotor = muWheels[1].ActiveState;

            //Find the Total Motor
            if (theresLimitation)
            {
                totalMotor = Conjunction(basicMotor, limitation);
            }
            else
            {
                totalMotor = basicMotor;
            }
        }

        private char Conjunction(char bm, char lim)
        {
            if (bm == '.' & lim == 'x')
            {
                return '.';
            }
            else
            {
                return 'x';
            }
        }

        /// <summary>
        /// returns the character corresponding 
        /// to active position of a set of wheels
        /// for example chiwheels
        /// </summary>
        private string Character(Wheel[] currentWheels)
        {
            string badout = "";

            foreach (Wheel wheel in currentWheels)
            {
                badout += wheel.ActiveState;
            }

            return badout;
        }

        /// <summary>
        /// Returns a character corresponding to z
        /// from the sum of p, chi and psi
        /// </summary>
        private char SumBaudot(string pstream, string chistream, string psistream)
        {
            char c;
            string b = "";

            for (int i = 0; i < pstream.Length; i++)
            {
                b += XOR(pstream[i], XOR(chistream[i], psistream[i]));
            }

            c = charCode[b];

            trace.Add(charCode[pstream] + "    +    " + SumXOR(charCode[chistream], charCode[psistream]) + "    ->    " + c);

            return c;
        }

        /// <summary>
        /// Returns the results of to apply 
        /// XOR to two characters
        /// </summary>
        private char XOR(char op1, char op2)
        {
            if (op1 == '.' && op2 == '.' || op1 == 'x' && op2 == 'x')
            {
                return '.';
            }

            if (op1 == '.' && op2 == 'x' || op2 == '.' && op1 == 'x')
            {
                return 'x';
            }

            return ' ';
        }
    }
}
