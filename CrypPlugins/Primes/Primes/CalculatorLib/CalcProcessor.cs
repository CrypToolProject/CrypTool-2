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

using Primes.Bignum;
using System;

namespace SevenZ.Calculator
{
    public partial class Calculator
    {
        /// <summary>
        /// Calculates binary expressions and pushes the result into the operands stack
        /// </summary>
        /// <param name="op">Binary operator</param>
        /// <param name="operand1">First operand</param>
        /// <param name="operand2">Second operand</param>
        private void Calculate(string op, PrimesBigInteger operand1, PrimesBigInteger operand2)
        {
            PrimesBigInteger res = PrimesBigInteger.Zero;
            try
            {
                switch (op)
                {
                    case Token.Add: res = operand1.Add(operand2); break;
                    case Token.Subtract: res = operand1.Subtract(operand2); break;
                    case Token.Multiply: res = operand1.Multiply(operand2); break;
                    case Token.Divide: res = operand1.Divide(operand2); break;
                    case Token.Mod: res = operand1.Mod(operand2); break;
                    case Token.Power: res = operand1.Pow(operand2.IntValue); break;
                    case Token.Log: res = PrimesBigInteger.Zero; break;
                    case Token.Root: res = PrimesBigInteger.Zero; break;
                }

                operands.Push(PostProcess(res));
            }
            catch (Exception e)
            {
                ThrowException(e.Message);
            }
        }

        /// <summary>
        /// Calculates unary expressions and pushes the result into the operands stack
        /// </summary>
        /// <param name="op">Unary operator</param>
        /// <param name="operand">Operand</param>
        private void Calculate(string op, PrimesBigInteger operand)
        {
            PrimesBigInteger res = PrimesBigInteger.One;

            try
            {
                switch (op)
                {
                    case Token.UnaryMinus: res = operand.Multiply(PrimesBigInteger.NegativeOne); break;
                    //case Token.Abs:        res = Math.Abs(operand); break;
                    //case Token.ACosine:    res = Math.Acos(operand); break;
                    //case Token.ASine:      res = Math.Asin(operand); break;
                    //case Token.ATangent:   res = Math.Atan(operand); break;
                    //case Token.Cosine:     res = Math.Cos(operand); break;
                    //case Token.Ln:         res = Math.Log(operand); break;
                    //case Token.Log10:      res = Math.Log10(operand); break;
                    //case Token.Sine:       res = Math.Sin(operand); break;
                    case Token.Sqrt: res = operand.SquareRoot(); break;
                        //case Token.Tangent:    res = Math.Tan(operand); break;
                        //case Token.Exp:        res = Math.Exp(operand); break;
                        //case Token.Factorial:  for (int i = 2; i <= (int)operand; res *= i++) ;
                        //break;
                }

                operands.Push(PostProcess(res));
            }
            catch (Exception e)
            {
                ThrowException(e.Message);
            }
        }

        /// <summary>
        /// Result post-processing
        /// </summary>
        private PrimesBigInteger PostProcess(PrimesBigInteger result)
        {
            return result;
        }
    }
}
