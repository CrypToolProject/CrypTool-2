/*
   Copyright 2008 Timo Eckhardt, University of Siegen

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

using System.Text.RegularExpressions;

namespace SevenZ.Calculator
{
    public partial class Calculator
    {
        public enum CalcMode { Numeric, Logic };

        public static class Token
        {
            public const string PRight = ")", PLeft = "(", Power = "^", Divide = "/",
                                Multiply = "*", UnaryMinus = "_", Add = "+", Subtract = "-",
                                Factorial = "!", Mod = "%",
                                Sentinel = "#", End = ";", Store = "=", None = " ",
                                Separator = ",";

            public const string Sine = "sin", Cosine = "cos", Tangent = "tan",
                       ASine = "asin", ACosine = "acos", ATangent = "atan",
                       Log = "log", Log10 = "log10", Ln = "ln", Exp = "exp",
                       Abs = "abs", Sqrt = "sqrt", Root = "rt";

            //BoolAnd = "&", BoolNot = "!", BoolOr = "|", BoolImp = ">", BoolXor = "^",

            private static readonly string[] binaryOperators = new string[] { Multiply, Divide, Subtract, Add,
                                                          Power, Log, Root, Mod };
            private static readonly string[] unaryOperators = new string[] { Subtract, Sine, Cosine, Tangent, ASine,
                                                         ACosine, ATangent, Log10, Ln, Exp,
                                                         Abs, Sqrt};
            private static readonly string[] specialOperators = new string[] { Sentinel, End, Store, None, Separator, PRight };
            private static readonly string[] rightSideOperators = new string[] { Factorial };
            private static readonly string[] FunctionList = new string[] { Sine, Cosine, Tangent, ASine, ACosine,
                                                       ATangent, Log, Log10, Ln, Exp, Abs,
                                                       Sqrt, Root };
            private static readonly string[] lastProcessedOperators = new string[] { Power }; // 2^3^4 = 2^(3^4)

            private static int Precedence(string op)
            {
                if (Token.IsFunction(op))
                {
                    return 64;
                }

                switch (op)
                {
                    case Subtract: return 4;
                    case Add: return 4;
                    case UnaryMinus: return 8;
                    case Power: return 16;
                    case Multiply: return 24;
                    case Divide: return 24;
                    case Mod: return 32;
                    case Factorial: return 48;
                    case PLeft: return 64;
                    case PRight: return 64;

                    default: return 0; //operators END, Sentinel, Store
                }
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="op1"></param>
            /// <param name="op2"></param>
            /// <returns></returns>
            public static int Compare(string op1, string op2)
            {
                if (op1 == op2 && Contains(op1, lastProcessedOperators))
                {
                    return -1;
                }
                else
                {
                    return Precedence(op1) >= Precedence(op2) ? 1 : -1;
                }
            }

            #region Is... Functions

            public static bool IsBinary(string op)
            {
                return Contains(op, binaryOperators);
            }

            public static bool IsUnary(string op)
            {
                return Contains(op, unaryOperators);
            }

            public static bool IsRightSide(string op)
            {
                return Contains(op, rightSideOperators);
            }

            public static bool IsSpecial(string op)
            {
                return Contains(op, specialOperators);
            }

            public static bool IsName(string token)
            {
                return Regex.IsMatch(token, @"[a-zA-Z0-9]");
            }

            public static bool IsDigit(string token)
            {
                return Regex.IsMatch(token, @"\d|\.");
            }

            public static bool IsFunction(string op)
            {
                return Contains(op, FunctionList);
            }

            #endregion

            /// <summary>
            /// Converts operator from expression to driver-comprehensible mode
            /// </summary>
            /// <param name="op">Unary operator</param>
            /// <returns>Converted operator</returns>
            public static string ConvertOperator(string op)
            {
                switch (op)
                {
                    case "-": return "_";
                    default: return op;
                }
            }

            public static string ToString(string op)
            {
                switch (op)
                {
                    case End: return "END";
                    default: return op.ToString();
                }
            }
        }

        private static bool Contains(string token, string[] array)
        {
            foreach (string s in array)
            {
                if (s == token)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
