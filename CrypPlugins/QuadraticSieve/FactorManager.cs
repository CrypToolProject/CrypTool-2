/*                              
   Copyright 2010 Sven Rech, Uni Duisburg-Essen

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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace CrypTool.Plugins.QuadraticSieve
{
    [Serializable]
    internal class FactorManager
    {
        private List<BigInteger> primeFactors = new List<BigInteger>();
        private List<BigInteger> compositeFactors = new List<BigInteger>();
        private BigInteger number;

        [NonSerialized]
        private readonly MethodInfo getPrimeFactorsMethod;
        [NonSerialized]
        private readonly MethodInfo getCompositeFactorsMethod;

        public delegate void FactorsChangedHandler(List<BigInteger> primeFactors, List<BigInteger> compositeFactors);
        [field: NonSerialized]
        public event FactorsChangedHandler FactorsChanged;

        public FactorManager(MethodInfo getPrimeFactors, MethodInfo getCompositeFactors, BigInteger number)
        {
            getPrimeFactorsMethod = getPrimeFactors;
            getCompositeFactorsMethod = getCompositeFactors;
            this.number = number;
        }

        #region public

        /// <summary>
        /// For debug purposes: Calculates the number from the factors.
        /// </summary>
        public BigInteger CalculateNumber()
        {
            BigInteger n = 1;
            foreach (BigInteger p in primeFactors)
            {
                n *= p;
            }

            foreach (BigInteger p in compositeFactors)
            {
                n *= p;
            }

            return n;
        }

        /// <summary>
        /// adds the factors from the msieve internal factorList to the factor lists of the FactorManager.
        /// </summary>
        public void AddFactors(IntPtr factorList)
        {
            AddFactorsWithoutFiringEvent(factorList);
            FactorsChanged(primeFactors, compositeFactors);
        }

        /// <summary>
        /// uses the informations from the factorManager parameter to transform some composite factors to prime factors.
        /// returns true if this factorManager has more informations than the parameter factorManager.
        /// </summary>
        public bool Synchronize(FactorManager factorManager)
        {
            Debug.Assert(factorManager.CalculateNumber() == CalculateNumber());
            if (SameFactorization(factorManager))
            {
                return false;
            }

            //check if we can gain information from factorManager for our FactorList (and put these informations in our list)
            foreach (BigInteger comp in compositeFactors)
            {
                if (!factorManager.compositeFactors.Contains(comp))
                {
                    List<BigInteger> primeFactorsForComp = new List<BigInteger>();
                    List<BigInteger> compositeFactorsForComp = new List<BigInteger>();

                    //Let's check whether factorManager already has a factorization for comp:
                    foreach (BigInteger p in factorManager.primeFactors)
                    {
                        if (comp % p == 0)
                        {
                            primeFactorsForComp.Add(p);
                        }
                    }

                    foreach (BigInteger c in factorManager.compositeFactors)
                    {
                        if (comp != c && comp % c == 0)
                        {
                            compositeFactorsForComp.Add(c);
                        }
                    }

                    if (primeFactorsForComp.Count != 0 || compositeFactorsForComp.Count != 0)
                    {
                        ReplaceCompositeByFactors(comp, primeFactorsForComp, compositeFactorsForComp);
                        return Synchronize(factorManager);
                    }
                }
            }

            //now check if our FactorList has more informations than factorManager:
            return !SameFactorization(factorManager);
        }

        private bool SameFactorization(FactorManager factorManager)
        {
            bool equalPrimes = primeFactors.Intersect(factorManager.primeFactors).Count() == 0;
            bool equalComposites = compositeFactors.Intersect(factorManager.compositeFactors).Count() == 0;
            return (equalPrimes && equalComposites);
        }

        /// <summary>
        /// Returns true if the composite list contains the param.
        /// </summary>
        public bool ContainsComposite(BigInteger composite)
        {
            foreach (BigInteger c in compositeFactors)
            {
                if (c == composite)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// replaces the composite factor "composite" by the factors of the parameters "primeFactors" and "compositeFactors".
        /// of course, the factors have to multiply up to composite.
        /// </summary>
        public void ReplaceCompositeByFactors(BigInteger composite, List<BigInteger> primeFactors, List<BigInteger> compositeFactors)
        {
            //Some debug stuff:
            BigInteger comp = 1;
            foreach (BigInteger p in primeFactors)
            {
                comp *= p;
            }

            foreach (BigInteger c in compositeFactors)
            {
                comp *= c;
            }

            Debug.Assert(comp == composite);

            //Add:
            this.primeFactors.AddRange(primeFactors);
            this.compositeFactors.AddRange(compositeFactors);
            normalizeLists();

            Debug.Assert(CalculateNumber() == number);
            FactorsChanged(this.primeFactors, this.compositeFactors);
        }

        /// <summary>
        /// replaces the composite factor "composite" by the factors of factorList.
        /// of course, factorList has to multiply up to composite.
        /// </summary>
        public void ReplaceCompositeByFactors(BigInteger composite, IntPtr factorList)
        {
            //Some debug stuff:
            FactorManager debugFactorManager = new FactorManager(getPrimeFactorsMethod, getCompositeFactorsMethod, composite);
            debugFactorManager.AddFactorsWithoutFiringEvent(factorList);
            Debug.Assert(debugFactorManager.CalculateNumber() == composite);

            //Add:
            AddFactorsWithoutFiringEvent(factorList);
            normalizeLists();

            Debug.Assert(CalculateNumber() == number);
            FactorsChanged(primeFactors, compositeFactors);
        }

        /// <summary>
        /// Returns a single composite factor (or 0, if no composite factors are left).
        /// </summary>
        public BigInteger GetCompositeFactor()
        {
            if (compositeFactors.Count == 0)
            {
                return 0;
            }

            return compositeFactors[0];
        }

        /// <summary>
        /// Returns if we only have prime factors. True means, that factorizing is finished.
        /// </summary>
        public bool OnlyPrimes()
        {
            return (compositeFactors.Count == 0);
        }

        /// <summary>
        /// Returns all prime factors in a sorted fashion.
        /// </summary>
        public BigInteger[] getPrimeFactors()
        {
            primeFactors.Sort();
            return primeFactors.ToArray();
        }

        #endregion

        #region private

        /// <summary>
        /// See AddFactors.
        /// </summary>
        private void AddFactorsWithoutFiringEvent(IntPtr factorList)
        {
            ArrayList pf = (ArrayList)(getPrimeFactorsMethod.Invoke(null, new object[] { factorList }));
            foreach (object o in pf)
            {
                primeFactors.Add(BigInteger.Parse((string)o));
            }

            ArrayList cf = (ArrayList)(getCompositeFactorsMethod.Invoke(null, new object[] { factorList }));
            foreach (object o in cf)
            {
                compositeFactors.Add(BigInteger.Parse((string)o));
            }

            normalizeLists();
        }

        /// <summary>
        /// Normalizes the prime and composite factor lists, i.e. after calling this method, N is the product
        /// of the elements from both the prime and the composite factor list.
        /// </summary>        
        private void normalizeLists()
        {
            primeFactors.Sort();
            compositeFactors.Sort();

            BigInteger N = number;
            List<BigInteger> pf = new List<BigInteger>();
            List<BigInteger> cf = new List<BigInteger>();

            foreach (BigInteger p in primeFactors)
            {
                while (N % p == 0)  //while N is divisible by p...
                {
                    pf.Add(p);
                    N = N / p;
                }
            }

            foreach (BigInteger c in compositeFactors)
            {
                while (N % c == 0)  //while N is divisible by c...
                {
                    cf.Add(c);
                    N = N / c;
                }
            }

            primeFactors = pf;
            compositeFactors = cf;

            Debug.Assert(N == 1);
        }

        #endregion
    }
}