/*
   Copyright 2008 - 2022 CrypTool Team

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
using CrypTool.PluginBase.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Threading;

namespace CrypTool.PluginBase.Miscellaneous
{
    /// <summary>
    /// This class is responsible for loading the msieve dll and offers some functions
    /// for factorizing.
    /// </summary>
    public class Msieve
    {
        private static Assembly msieveDLL = null;
        private static readonly Mutex msieveMutex = new Mutex();

        /// <summary>
        /// A single factor
        /// </summary>
        public struct Factor
        {
            public BigInteger factor;
            public bool prime;
            public int count;

            public Factor(BigInteger factor, bool prime, int count)
            {
                this.factor = factor;
                this.prime = prime;
                this.count = count;
            }
        }

        /// <summary>
        /// This method is mainly a singleton.
        /// </summary>
        /// <returns>The (only) msieve assembly</returns>
        public static Assembly GetMsieveDLL()
        {
            msieveMutex.WaitOne();

            if (msieveDLL == null)
            {
                msieveDLL = Assembly.LoadFile(DirectoryHelper.BaseDirectory + "\\Lib\\msieve64.dll");
            }

            msieveMutex.ReleaseMutex();

            return msieveDLL;
        }

        /// <summary>
        /// This method factorizes the parameter "number" by using the "trivial" (i.e. very fast) methods that are available in msieve.
        /// This means, that the factorization doesn't take very long, but on the other hand, you can end up having some 
        /// composite factors left, because they can't be factorized efficiently.
        /// </summary>
        /// <param name="number">the number to factorize</param>
        /// <returns>A list of factors</returns>
        public static List<Factor> TrivialFactorization(BigInteger number)
        {
            msieveMutex.WaitOne();

            Type msieve = GetMsieveDLL().GetType("Msieve.msieve");

            //init msieve with callbacks:
            MethodInfo initMsieve = msieve.GetMethod("initMsieve");
            object callback_struct = Activator.CreateInstance(msieveDLL.GetType("Msieve.callback_struct"));
            FieldInfo putTrivialFactorlistField = msieveDLL.GetType("Msieve.callback_struct").GetField("putTrivialFactorlist");
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static;
            MethodInfo putTrivialFactorlistMethodInfo = typeof(Msieve).GetMethod("putTrivialFactorlist", flags);
            Delegate putTrivialFactorlistDel = MulticastDelegate.CreateDelegate(msieveDLL.GetType("Msieve.putTrivialFactorlistDelegate"), putTrivialFactorlistMethodInfo);
            putTrivialFactorlistField.SetValue(callback_struct, putTrivialFactorlistDel);
            initMsieve.Invoke(null, new object[1] { callback_struct });

            //start msieve:
            currentNumber = number;
            MethodInfo start = msieve.GetMethod("start");
            start.Invoke(null, new object[] { number.ToString(), null });

            msieveMutex.ReleaseMutex();
            return factorlist;
        }

        #region private

        private static List<Factor> factorlist;
        private static BigInteger currentNumber;

        private static void putTrivialFactorlist(IntPtr list, IntPtr obj)
        {
            factorlist = new List<Factor>();

            Type msieve = GetMsieveDLL().GetType("Msieve.msieve");
            MethodInfo getPrimeFactorsMethod = msieve.GetMethod("getPrimeFactors");
            MethodInfo getCompositeFactorsMethod = msieve.GetMethod("getCompositeFactors");

            ArrayList pf = (ArrayList)(getPrimeFactorsMethod.Invoke(null, new object[] { list }));
            foreach (object o in pf)
            {
                AddToFactorlist(BigInteger.Parse((string)o), true);
            }

            ArrayList cf = (ArrayList)(getCompositeFactorsMethod.Invoke(null, new object[] { list }));
            foreach (object o in cf)
            {
                AddToFactorlist(BigInteger.Parse((string)o), false);
            }

            Debug.Assert(currentNumber == 1);
        }

        private static void AddToFactorlist(BigInteger factor, bool prime)
        {
            //Check if factor already in factorlist:
            foreach (Factor f in factorlist)
            {
                if (f.factor == factor)
                {
                    return;
                }
            }

            //Add to factorlist:
            int count = 0;
            while (currentNumber % factor == 0)
            {
                count++;
                currentNumber /= factor;
            }
            Debug.Assert(count != 0);
            factorlist.Add(new Factor(factor, prime, count));
        }

        #endregion

    }
}
