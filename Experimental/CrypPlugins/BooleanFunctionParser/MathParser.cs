/* Copyright (C) 2005 <Paratrooper> paratrooper666@gmx.net
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  any later version.
 */

using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace CrypTool.MathParser
{
    public enum Mode { RAD, DEG, GRAD };

    public class Parser
    {
        private ArrayList FunctionList = new ArrayList(new string[] { "abs", "acos", "asin", "atan", "ceil", "cos", "cosh", "exp", "floor", "ln", "log", "sign", "sin", "sinh", "sqrt", "tan", "tanh" });
        private double Value;
        private double Factor;
        private Mode mode;

        // Constructor
        public Parser()
        {
            this.Mode = Mode.RAD;
        }
        public Parser(Mode mode)
        {
            this.Mode = mode;
        }

        // Getter & Setter
        public double Result
        {
            get { return this.Value; }
        }
        public Mode Mode
        {
            get { return this.mode; }
            set
            {
                this.mode = value;
                switch (value)
                {
                    case Mode.RAD:
                        this.Factor = 1.0;
                        break;
                    case Mode.DEG:
                        this.Factor = 2.0 * Math.PI / 360.0;
                        break;
                    case Mode.GRAD:
                        this.Factor = 2.0 * Math.PI / 400.0;
                        break;
                }
            }
        }

        public bool Evaluate(string Expression)
        {
            try
            {
                // ****************************************************************************************
                // ** MathParser in action:                                                              **
                // ** Expression = "-(5 - 10)^(-1)  ( 3 + 2(    cos( 3 Pi )+( 2+ ln( exp(1) ) )    ^3))" **
                // ****************************************************************************************
                //
                //
                // ----------
                // - Step 1 -
                // ----------
                // Remove blank.
                //
                // -(5 - 10)^(-1)  ( 3 + 2(    cos( 3 Pi )+( 2+ ln( exp(1) ) )    ^3)) -> -(5-10)^(-1)(3+2(cos(3Pi)+(2+ln(exp(1)))^3))
                //
                Expression = Expression.Replace(" ", "");
                //
                // ----------
                // - Step 2 -
                // ----------
                // Insert '*' if necessary.
                //
                //                                                             _    _      _
                // -(5-10)^(-1)(3+2(cos(3Pi)+(2+ln(exp(1)))^3)) -> -(5-10)^(-1)*(3+2*(cos(3*Pi)+(2+ln(exp(1)))^3))
                //             |   |     |
                //
                Regex regEx = new Regex(@"(?<=[\d\)])(?=[a-df-z\(])|(?<=pi)(?=[^\+\-\*\/\\^!)])|(?<=\))(?=\d)|(?<=[^\/\*\+\-])(?=exp)", RegexOptions.IgnoreCase);
                Expression = regEx.Replace(Expression, "*");
                //
                // ----------
                // - Step 4 -
                // ----------
                // Search for parentheses an solve the expression between it.
                //
                /*                                                      _____
				* -(5-10)^(-1)*(3+2*(cos(3*3,14)+(2+ln(exp(1)))^3)) -> -{-5}^(-1)*(3+2*(cos(3*3,14)+(2+ln(exp(1)))^3))
				*  |_____|
				*                                                          __
				* -{-5}^(-1)*(3+2*(cos(3*3,14)+(2+ln(exp(1)))^3)) -> -{-5}^-1*(3+2*(cos(3*3,14)+(2+ln(exp(1)))^3))
				*       |__|
				*                                                                    ____
				* -{-5}^-1*(3+2*(cos(3*3,14)+(2+ln(exp(1)))^3)) -> -{-5}^-1*(3+2*(cos9,42+(2+ln(exp(1)))^3))
				*                   |______|
				*                                                                              _
				* -{-5}^-1*(3+2*(cos9,72+(2+ln(exp(1)))^3)) -> -{-5}^-1*(3+2*(cos9,72+(2+ln(exp1))^3))
				*                                 |_|
				*                                                                        ____
				* -{-5}^-1*(3+2*(cos9,72+(2+ln(exp1))^3)) -> -{-5}^-1*(3+2*(cos9,72+(2+ln2,71)^3))
				*                             |____|
				*                                                                 ____
				* -{-5}^-1*(3+2*(cos9,72+(2+ln2,71)^3)) -> -{-5}^-1*(3+2*(cos9,72+{3}^3))
				*                        |_________|
				*                                                 __
				* -{-5}^-1*(3+2*(cos9,72+{3}^3)) -> -{-5}^-1*(3+2*26)
				*               |_____________|
				*                               __
				* -{-5}^-1*(3+2*26) -> -{-5}^-1*55
				*          |______|
				*/
                regEx = new Regex(@"([a-z]*)\(([^\(\)]+)\)(\^|!?)", RegexOptions.IgnoreCase);
                Match m = regEx.Match(Expression);
                while (m.Success)
                {
                    if (m.Groups[3].Value.Length > 0) Expression = Expression.Replace(m.Value, "{" + m.Groups[1].Value + this.Solve(m.Groups[2].Value) + "}" + m.Groups[3].Value);
                    else Expression = Expression.Replace(m.Value, m.Groups[1].Value + this.Solve(m.Groups[2].Value));
                    m = regEx.Match(Expression);
                }
                //
                // ----------
                // - Step 5 -
                // ----------
                // There are no more parentheses. Solve the expression and convert it to double.
                //                __
                // -{-5}^-1*55 => 11
                // |_________|
                //
                this.Value = Convert.ToDouble(this.Solve(Expression));
                return true;
            }
            catch
            {
                // Shit!
                return false;
            }
        }

        private string Solve(string Expression)
        {
            Regex regEx;
            Match m;

            // Solve AND
            regEx = new Regex(@"([\+-]?\d+,*\d*[eE][\+-]?\d+|[\-\+]?\d+,*\d*)([\/\*])(-?\d+,*\d*[eE][\+-]?\d+|-?\d+,*\d*)");
            m = regEx.Match(Expression, 0);
            while (m.Success)
            {
                double result;
                //switch( m.Groups[2].Value )
                //{
                //	case "*" :
                result = Convert.ToDouble(Convert.ToInt32(m.Groups[1].Value) & Convert.ToInt32(m.Groups[3].Value));
                if (m.Index == 0) Expression = regEx.Replace(Expression, result.ToString(), 1);
                else Expression = Expression.Replace(m.Value, "+" + result);
                m = regEx.Match(Expression);
                //		continue;
                //}
            }
            // Solve XOR.
            regEx = new Regex(@"([\+-]?\d+,*\d*[eE][\+-]?\d+|[\+-]?\d+,*\d*)([\+-])(-?\d+,*\d*[eE][\+-]?\d+|-?\d+,*\d*)");
            m = regEx.Match(Expression, 0);
            while (m.Success)
            {
                double result;
                //switch( m.Groups[2].Value )
                //{
                //	case "+" :
                result = result = Convert.ToDouble(Convert.ToInt32(m.Groups[1].Value) ^ Convert.ToInt32(m.Groups[3].Value));
                if (m.Index == 0) Expression = regEx.Replace(Expression, result.ToString(), 1);
                else Expression = regEx.Replace(Expression, "+" + result, 1);
                m = regEx.Match(Expression);
                //		continue;
                //}
            }
            //if( Expression.StartsWith( "--" ) ) Expression = Expression.Substring(2);
            return Expression;
        }
    }
}
