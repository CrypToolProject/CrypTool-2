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
using System.Collections.Generic;

namespace SevenZ.Calculator
{
    public partial class Calculator
    {
        public delegate void CalcVariableDelegate(object sender, EventArgs e);
        public event CalcVariableDelegate OnVariableStore;

        private Dictionary<string, PrimesBigInteger> variables;

        public const string AnswerVar = "r";

        private void LoadConstants()
        {
            variables = new Dictionary<string, PrimesBigInteger>();
            //variables.Add("pi", Math.PI);
            //variables.Add("e", Math.E);
            //variables.Add(AnswerVar, 0);

            if (OnVariableStore != null)
            {
                OnVariableStore(this, new EventArgs());
            }
        }

        public Dictionary<string, PrimesBigInteger> Variables => variables;

        public void SetVariable(string name, PrimesBigInteger val)
        {
            if (variables.ContainsKey(name))
            {
                variables[name] = val;
            }
            else
            {
                variables.Add(name, val);
            }

            if (OnVariableStore != null)
            {
                OnVariableStore(this, new EventArgs());
            }
        }

        public PrimesBigInteger GetVariable(string name)
        {  // return variable's value // if variable ha push default value, 0
            return variables.ContainsKey(name) ? variables[name] : PrimesBigInteger.Zero;
        }
    }
}
