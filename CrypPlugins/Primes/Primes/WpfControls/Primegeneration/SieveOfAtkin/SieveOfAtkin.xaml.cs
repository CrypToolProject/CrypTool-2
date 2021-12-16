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
using Primes.Library;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;

namespace Primes.WpfControls.Primegeneration.SieveOfAtkin
{
    /// <summary>
    /// Interaction logic for SieveOfAtkin.xaml
    /// </summary>
    public partial class SieveOfAtkin : UserControl
    {
        private Thread m_SieveThread;
        private PrimesBigInteger m_Value;

        public SieveOfAtkin()
        {
            InitializeComponent();
        }

        #region Thread

        public void startThread()
        {
            m_SieveThread = new Thread(new ThreadStart(Sieve))
            {
                CurrentCulture = Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture
            };

            m_SieveThread.Start();
        }

        public bool IsRunning()
        {
            return (m_SieveThread != null) && m_SieveThread.IsAlive;
        }

        public void CancelSieve()
        {
            FireCancelEvent();
            CancelThread();
        }

        private void CancelThread()
        {
            if (m_SieveThread != null)
            {
                m_SieveThread.Abort();
                m_SieveThread = null;
            }
        }

        #endregion

        #region events

        public event VoidDelegate Start;
        public event VoidDelegate Stop;
        public event VoidDelegate Cancel;

        private void FireStartEvent()
        {
            if (Start != null)
            {
                Start();
            }
        }

        private void FireStopEvent()
        {
            if (Stop != null)
            {
                Stop();
            }
        }

        private void FireCancelEvent()
        {
            if (Cancel != null)
            {
                Cancel();
            }
        }

        #endregion

        public void Execute(PrimesBigInteger value)
        {
            if (IsRunning())
            {
                return;
            }

            log.Clear();
            log.Columns = 1;

            m_Value = value;
            numbergrid.Limit = m_Value;
            startThread();
        }

        public void Sieve()
        {
            FireStartEvent();

            int limit = m_Value.IntValue;
            bool[] list = new bool[limit + 1];
            int sqrt = (int)Math.Sqrt(limit);

            log.Info(string.Format(Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.soa_initvsieve, m_Value.ToString("D")));

            IList<PrimesBigInteger> result = new List<PrimesBigInteger>
            {
                PrimesBigInteger.Two,
                PrimesBigInteger.Three,
                PrimesBigInteger.Five
            };
            numbergrid.MarkNumber(PrimesBigInteger.Two, Brushes.LightBlue);
            numbergrid.MarkNumber(PrimesBigInteger.Three, Brushes.LightBlue);
            numbergrid.MarkNumber(PrimesBigInteger.Five, Brushes.LightBlue);

            log.Info(Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.soa_startsieve);

            for (int i = 1; i < list.Length; i++)
            {
                log.Info(string.Format(Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.soa_actualnumber, i.ToString()));

                if (i % 2 == 0)
                {
                    continue;
                }

                if (i % 3 == 0)
                {
                    continue;
                }

                if (i % 5 == 0)
                {
                    continue;
                }

                int mod = i % 60;

                if (mod == 1 || mod == 13 || mod == 17 || mod == 29 || mod == 37 || mod == 41 || mod == 49 || mod == 53)
                {
                    log.Info(string.Format(Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.soa_firstifmatch, new object[] { i.ToString(), mod.ToString(), i.ToString() }));

                    for (int j = 1; j <= sqrt; j++)
                    {
                        for (int k = 1; k <= sqrt; k++)
                        {
                            if (4 * j * j + k * k == i)
                            {
                                list[i] = !list[i];

                                log.Info(string.Format(Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.soa_firstsolutionfound, new object[] { j.ToString(), k.ToString(), i.ToString(), i.ToString(),
                    (list[i]) ? Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.soa_isprime : Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.soa_isnotprime }));

                                numbergrid.MarkNumber(PrimesBigInteger.ValueOf(i), list[i] ? Brushes.LightBlue : Brushes.Transparent);
                            }
                        }
                    }
                }
                else if (mod == 7 || mod == 19 || mod == 31 || mod == 43)
                {
                    log.Info(string.Format(Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.soa_secondifmatch, new object[] { i.ToString(), mod.ToString(), i.ToString() }));

                    for (int j = 1; j <= sqrt; j++)
                    {
                        for (int k = 1; k <= sqrt; k++)
                        {
                            if (3 * j * j + k * k == i)
                            {
                                list[i] = !list[i];

                                log.Info(string.Format(Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.soa_secondsolutionfound, new object[] { j.ToString(), k.ToString(), i.ToString(), i.ToString(),
                    (list[i]) ? Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.soa_isprime : Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.soa_isnotprime }));

                                numbergrid.MarkNumber(PrimesBigInteger.ValueOf(i), list[i] ? Brushes.LightBlue : Brushes.Transparent);
                            }
                        }
                    }
                }
                else if (mod == 11 || mod == 23 || mod == 47 || mod == 59)
                {
                    log.Info(string.Format(Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.soa_thirdifmatch, new object[] { i.ToString(), mod.ToString(), i.ToString() }));

                    for (int j = 1; j <= sqrt; j++)
                    {
                        for (int k = 1; k <= sqrt; k++)
                        {
                            if (3 * j * j - k * k == i && j > k)
                            {
                                list[i] = !list[i];

                                log.Info(string.Format(Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.soa_thirdsolutionfound, new object[] { j.ToString(), k.ToString(), i.ToString(), i.ToString(),
                    (list[i]) ? Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.soa_isprime : Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.soa_isnotprime }));

                                numbergrid.MarkNumber(PrimesBigInteger.ValueOf(i), list[i] ? Brushes.LightBlue : Brushes.Transparent);
                            }
                        }
                    }
                }

                //Thread.Sleep(10);
            }

            list[2] = true;
            list[3] = true;
            list[5] = true;

            for (int i = 7; i <= sqrt; i++)
            {
                if (list[i])
                {
                    numbergrid.MarkNumber(PrimesBigInteger.ValueOf(i), Brushes.LightBlue);

                    int i2 = i * i;

                    log.Info(string.Format(Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.soa_fourthsolutionfound, new object[] { i.ToString(), i2.ToString() }));

                    for (int j = i2; j < list.Length; j += i2)
                    {
                        if (list[j])
                        {
                            list[j] = false;
                            numbergrid.MarkNumber(PrimesBigInteger.ValueOf(j), Brushes.Transparent);
                            log.Info(string.Format(Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.soa_fithsolutionfound, new object[] { j.ToString(), i.ToString() }));
                        }
                    }
                }
            }

            numbergrid.Sieved = list;

            FireStopEvent();
        }
    }
}
