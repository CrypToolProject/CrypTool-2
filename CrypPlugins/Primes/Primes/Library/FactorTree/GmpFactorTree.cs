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
using System.Threading;

namespace Primes.Library.FactorTree
{
    public enum FactorizingMethod { TrialDivision, FastTrialDivision }
    public delegate void GmpFactorTreeDelegate();

    public class GmpFactorTree
    {
        private Thread m_FactorThread;
        private GmpFactorTreeNode m_Root;
        private readonly object m_LockObject = null;
        private IDictionary<string, PrimesBigInteger> m_Factors;

        private PrimesBigInteger m_Remainder;

        public GmpFactorTreeNode Root => m_Root;

        public GmpFactorTree()
        {
            m_LockObject = new object();
        }

        public PrimesBigInteger GetFactorCount(string factor)
        {
            if (m_Factors.ContainsKey(factor))
            {
                return m_Factors[factor];
            }
            else
            {
                return null;
            }
        }

        public ICollection<string> Factors
        {
            get
            {
                if (m_Factors != null)
                {
                    return m_Factors.Keys;
                }
                else
                {
                    return null;
                }
            }
        }

        public PrimesBigInteger Remainder => m_Remainder;

        #region events

        public event GmpFactorTreeDelegate OnFactorFound;
        public event GmpFactorTreeDelegate OnStart;
        public event GmpFactorTreeDelegate OnStop;
        public event GmpFactorTreeDelegate OnCancel;

        public event GmpBigIntegerParameterDelegate OnActualDivisorChanged;

        public void FireOnActualDivisorChanged(PrimesBigInteger i)
        {
            if (OnActualDivisorChanged != null)
            {
                OnActualDivisorChanged(i);
            }
        }

        #endregion

        public void Factorize(PrimesBigInteger value)
        {
            m_Factors = new Dictionary<string, PrimesBigInteger>();
            m_FactorThread = new Thread(new ParameterizedThreadStart(DoFactorize))
            {
                CurrentCulture = Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture
            };
            m_Root = new GmpFactorTreeNode(value);
            if (OnStart != null)
            {
                OnStart();
            }

            m_FactorThread.Start(FactorizingMethod.TrialDivision);
        }

        public void CancelFactorize()
        {
            if (OnCancel != null)
            {
                OnCancel();
            }

            if (m_FactorThread != null)
            {
                m_FactorThread.Abort();
                m_FactorThread = null;
            }
        }

        public bool isRunning
        {
            get
            {
                if (m_FactorThread == null)
                {
                    return false;
                }

                return m_FactorThread.IsAlive;
            }
        }

        private int m_Height;

        public int Height => m_Height;

        private void DoFactorize(object method)
        {
            if (method.GetType() == typeof(FactorizingMethod))
            {
                FactorizingMethod _method = (FactorizingMethod)method;
                switch (_method)
                {
                    case FactorizingMethod.TrialDivision:
                        TrialDivision();
                        break;
                    case FactorizingMethod.FastTrialDivision:
                        TrialDivision();
                        break;
                }
                CancelFactorize();
            }
        }

        private void TrialDivision()
        {
            m_Height = 0;
            PrimesBigInteger divisor = PrimesBigInteger.Two;
            FireOnActualDivisorChanged(divisor);
            PrimesBigInteger value = new PrimesBigInteger(m_Root.Value);
            GmpFactorTreeNode node = m_Root;
            while (!value.IsProbablePrime(10))
            {
                //int counter = 0;
                while (value.Mod(divisor).CompareTo(PrimesBigInteger.Zero) != 0)
                {
                    divisor = divisor.NextProbablePrime();
                    FireOnActualDivisorChanged(divisor);
                    //counter++;
                    //if (counter == 1000000)
                    //{
                    //  if (OnCanceled != null) OnCanceled("Nach 100.000 Versuchen wurde kein weiterer Faktor gefunden, das Verfahren wird abgebrochen.");
                    //  CancelFactorize();
                    //}
                }
                value = value.Divide(divisor);
                m_Remainder = value;
                GmpFactorTreeNode primeNodeTmp = new GmpFactorTreeNode(divisor)
                {
                    IsPrime = true
                };
                node.AddChild(primeNodeTmp);
                node = node.AddChild(new GmpFactorTreeNode(value));
                node.IsPrime = value.IsProbablePrime(20);
                m_Height++;
                AddFactor(divisor);
            }
            m_Remainder = null;

            node.IsPrime = true;
            AddFactor(node.Value);
            if (OnStop != null)
            {
                OnStop();
            }
        }

        private void TrialDivision2()
        {
        }

        //  private void FastTrialDivision()
        //  {
        //      m_Height = 0;
        //  PrimesBigInteger m_factor = PrimesBigInteger.Two;
        //      int mod30 = m_factor.Mod(PrimesBigInteger.ValueOf(30)).IntValue;
        //      PrimesBigInteger value = new PrimesBigInteger(this.m_Root.Value);
        //      PrimesBigInteger max = value.SquareRoot();
        //      GmpFactorTreeNode node = this.m_Root;
        //bool run = true;
        //if (value.IsProbablePrime(10))
        //  run = false;

        //      while (run)
        //      {
        //          while (m_factor.CompareTo(max) <= 0)
        //          {
        //              if (!(mod30 == 1 ||
        //               mod30 == 7 ||
        //               mod30 == 11 ||
        //               mod30 == 13 ||
        //               mod30 == 17 ||
        //               mod30 == 19 ||
        //               mod30 == 23 ||
        //               mod30 == 29 ||
        //               m_factor.CompareTo(PrimesBigInteger.Two) == 0 ||
        //               m_factor.CompareTo(PrimesBigInteger.ValueOf(3)) == 0 ||
        //               m_factor.CompareTo(PrimesBigInteger.ValueOf(5)) == 0))
        //              {
        //                  m_factor = m_factor.Add(PrimesBigInteger.One);
        //                  mod30++;
        //                  (mod30) %= 30;
        //                  continue;
        //              }
        //              if (value.Mod(m_factor).CompareTo(PrimesBigInteger.Zero)==0)
        //              {
        //                  value = value.Divide(m_factor);
        //      m_Remainder = value;

        //                  max = value.SquareRoot();
        //                  GmpFactorTreeNode primeNodeTmp = new GmpFactorTreeNode(m_factor);
        //      primeNodeTmp.IsPrime = m_factor.IsPrime(10);
        //      m_Height++;
        //      AddFactor(m_factor);
        //      node.AddChild(primeNodeTmp);
        //                  node = node.AddChild(new GmpFactorTreeNode(value));
        //                  continue;
        //              }
        //              m_factor = m_factor.Add(PrimesBigInteger.One);
        //              mod30++;
        //              (mod30) %= 30;
        //              run = false;
        //          }
        //          run = false;
        //      }
        //m_Remainder = null;
        //      node.IsPrime = true;
        //      AddFactor(value);
        //  }

        private void AddFactor(PrimesBigInteger value)
        {
            if (m_Factors.ContainsKey(value.ToString()))
            {
                m_Factors[value.ToString()] = m_Factors[value.ToString()].Add(PrimesBigInteger.One);
            }
            else
            {
                m_Factors.Add(value.ToString(), PrimesBigInteger.One);
            }

            if (OnFactorFound != null)
            {
                OnFactorFound();
            }
        }

        public int Width = 2;

        public void Clear()
        {
            GmpFactorTreeNode node = Root;
            if (node != null)
            {
                while (node.LeftChild != null)
                {
                    node = node.LeftChild;
                    if (node.RightSibling != null)
                    {
                        node = node.RightSibling;
                    }
                }
                while (node.LeftSibling != null)
                {
                    node = node.LeftSibling;
                    node = node.Parent;
                }
                m_Root = null;
            }
            if (m_Factors != null)
            {
                m_Factors.Clear();
            }
        }
    }

    public class GmpFactorTreeNode
    {
        public GmpFactorTreeNode(PrimesBigInteger value)
        {
            m_Value = new PrimesBigInteger(value);
        }

        private bool m_IsPrime;

        public bool IsPrime
        {
            get => m_IsPrime;
            set => m_IsPrime = value;
        }

        private PrimesBigInteger m_Value = null;

        public PrimesBigInteger Value
        {
            get => m_Value;
            set => m_Value = value;
        }

        private GmpFactorTreeNode m_Parent = null;

        public GmpFactorTreeNode Parent
        {
            get => m_Parent;
            set => m_Parent = value;
        }

        private GmpFactorTreeNode m_LeftChild = null;

        public GmpFactorTreeNode LeftChild => m_LeftChild;

        private GmpFactorTreeNode m_RightSibling = null;

        public GmpFactorTreeNode RightSibling => m_RightSibling;

        private GmpFactorTreeNode m_LeftSibling = null;

        public GmpFactorTreeNode LeftSibling => m_LeftSibling;

        public GmpFactorTreeNode AddChild(GmpFactorTreeNode child)
        {
            if (child == null)
            {
                throw new ArgumentNullException("child");
            }

            child.Parent = this;

            if (m_LeftChild == null)
            {
                m_LeftChild = child;
            }
            else
            {
                GmpFactorTreeNode tmpchild = m_LeftChild;
                while (tmpchild.m_RightSibling != null)
                {
                    tmpchild = child.m_RightSibling;
                }
                tmpchild.m_RightSibling = child;
                child.m_LeftSibling = tmpchild;
            }

            return child;
        }
    }
}
