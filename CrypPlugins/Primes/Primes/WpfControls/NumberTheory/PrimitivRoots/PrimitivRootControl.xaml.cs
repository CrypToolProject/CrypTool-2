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
using Primes.WpfControls.Validation;
using Primes.WpfControls.Validation.Validator;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using rsc = Primes.Resources.lang.Numbertheory.Numbertheory;

namespace Primes.WpfControls.NumberTheory.PrimitivRoots
{
    /// <summary>
    /// Interaction logic for PrimitivRootControl.xaml
    /// </summary>
    public partial class PrimitivRootControl : UserControl, IPrimeMethodDivision
    {
        private Thread m_ThreadCalculatePrimitiveRoots;
        private static readonly int[] mersenneseed = new int[] { 3, 5, 7, 13 };

        public PrimitivRootControl()
        {
            InitializeComponent();
            OnStart += new VoidDelegate(PrimitivRootControl_OnStart);
            OnStop += new VoidDelegate(PrimitivRootControl_OnStop);
            validator = new BigIntegerMinValueMaxValueValidator(null, MIN, MAX);
            log.OverrideText = true;
            int mersenneexp = mersenneseed[new Random().Next(mersenneseed.Length - 1)];
            tbInput.Text = PrimesBigInteger.Random(2).Add(PrimesBigInteger.Three).NextProbablePrime().ToString();
            tbInput.Text += ", 2^" + mersenneexp + "-1";
            PrimesBigInteger rangeval = PrimesBigInteger.Random(2).Add(PrimesBigInteger.Three);

            tbInput.Text += ", " + rangeval.ToString() + ":" + rangeval.Add(PrimesBigInteger.Ten).ToString();

            rndGenerate = new Random((int)(DateTime.Now.Ticks % int.MaxValue));
            m_JumpLockObject = new object();
        }

        private void btnPrimitivRootInput_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.PrimitivRoot_Input);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            StopThread();
            log.Info(rsc.proot_skip);
        }

        private void ClearLog()
        {
            log.Clear();
            log.Columns = 1;
        }

        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {
            Execute(true);
        }

        private void Execute(bool doExecute)
        {
            ClearLog();
            intervals.Clear();

            string _input = tbInput.Text;
            if (!string.IsNullOrEmpty(_input))
            {
                string[] input = _input.Trim().Split(',');
                if (input != null && input.Length > 0)
                {
                    foreach (string s in input)
                    {
                        if (!string.IsNullOrEmpty(s))
                        {
                            string[] _inputrange = s.Split(':');
                            if (_inputrange.Length == 1)
                            {
                                PrimesBigInteger ipt = null;
                                validator.Value = s;
                                Primes.WpfControls.Validation.ValidationResult res = validator.Validate(ref ipt);
                                if (res == Primes.WpfControls.Validation.ValidationResult.OK)
                                {
                                    if (ipt.IsPrime(10))
                                    {
                                        intervals.Add(new List<PrimesBigInteger> { ipt, ipt });
                                        if (ipt.CompareTo(MAX) > 0)
                                        {
                                            log.Info(string.Format(rsc.proot_warningbignumber, s));
                                        }
                                    }
                                    else
                                    {
                                        log.Info(string.Format(rsc.proot_noprime, s));
                                    }
                                }
                                else
                                {
                                    log.Info(string.Format(rsc.proot_novalidnumber, new object[] { s, MIN.ToString(), MAX.ToString() }));
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(_inputrange[0]) && string.IsNullOrEmpty(_inputrange[1]))
                                {
                                    log.Info(rsc.proot_rangeboth);
                                }
                                else if (string.IsNullOrEmpty(_inputrange[0]))
                                {
                                    log.Info(string.Format(rsc.proot_rangeupper, _inputrange[1]));
                                }
                                else if (string.IsNullOrEmpty(_inputrange[1]))
                                {
                                    log.Info(string.Format(rsc.proot_rangedown, _inputrange[0]));
                                }
                                else
                                {
                                    PrimesBigInteger i1 = IsGmpBigInteger(_inputrange[0]);
                                    PrimesBigInteger i2 = IsGmpBigInteger(_inputrange[1]);
                                    if (i1 != null && i2 != null)
                                    {
                                        if (i1.CompareTo(i2) >= 0)
                                        {
                                            log.Info(string.Format(rsc.proot_wronginterval, s));
                                        }
                                        else
                                        {
                                            intervals.Add(new List<PrimesBigInteger> { i1, i2 });
                                            if (i2.CompareTo(MAXWARN) > 0)
                                            {
                                                log.Info(string.Format(rsc.proot_warningbiginterval, s));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (doExecute && intervals.Count > 0)
                {
                    StartThread();
                }
            }
            else
            {
                Info(rsc.proot_insert);
            }
        }

        private PrimesBigInteger IsGmpBigInteger(string s)
        {
            PrimesBigInteger ipt = null;
            validator.Value = s;
            Primes.WpfControls.Validation.ValidationResult res = validator.Validate(ref ipt);
            if (res != Primes.WpfControls.Validation.ValidationResult.OK)
            {
                log.Info(string.Format(rsc.proot_novalidnumber, new object[] { s, MIN.ToString(), MAX.ToString() }));
                return null;
            }
            return ipt;
        }

        private string[] GetInput()
        {
            string _input = tbInput.Text;
            if (!string.IsNullOrEmpty(_input))
            {
                return _input.Split(',');
            }
            return null;
        }

        #region Constants

        private static readonly PrimesBigInteger MIN = PrimesBigInteger.ValueOf(1);
        private static readonly PrimesBigInteger MAX = PrimesBigInteger.ValueOf(uint.MaxValue);
        private static readonly PrimesBigInteger MAXWARN = PrimesBigInteger.ValueOf(1000000);

        #endregion

        #region Properites

        private readonly List<List<PrimesBigInteger>> intervals = new List<List<PrimesBigInteger>>();
        private readonly IValidator<PrimesBigInteger> validator;
        private readonly Random rndGenerate;

        #endregion

        #region events

        private event VoidDelegate OnStart;
        private event VoidDelegate OnStop;

        private void FireOnStop()
        {
            if (OnStop != null)
            {
                OnStop();
            }
        }

        private void FireOnStart()
        {
            if (OnStart != null)
            {
                OnStart();
            }
        }

        #endregion

        #region CalculatePrimitiveRoots

        private void PrimitivRootControl_OnStop()
        {
            ControlHandler.SetPropertyValue(log, "Title", rsc.proot_result);
            ControlHandler.SetButtonEnabled(btnExecute, true);
            ControlHandler.SetButtonEnabled(btnCancel, false);
            ControlHandler.SetPropertyValue(btnJump, "Visibility", Visibility.Hidden);
            ControlHandler.SetPropertyValue(tbInput, "IsEnabled", true);
        }

        private void PrimitivRootControl_OnStart()
        {
            ControlHandler.SetPropertyValue(log, "Title", rsc.proot_progress);
            ControlHandler.SetButtonEnabled(btnExecute, false);
            ControlHandler.SetButtonEnabled(btnCancel, true);
            ControlHandler.SetPropertyValue(btnJump, "Visibility", Visibility.Visible);
            ControlHandler.SetPropertyValue(tbInput, "IsEnabled", false);
        }

        private void StartThread()
        {
            m_ThreadCalculatePrimitiveRoots = new Thread(new ThreadStart(DoCalculatePrimitiveRoots))
            {
                CurrentCulture = Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture
            };
            m_ThreadCalculatePrimitiveRoots.Start();
        }

        private void StopThread()
        {
            FireOnStop();
            CancelThread();
        }

        private void CancelThread()
        {
            if (m_ThreadCalculatePrimitiveRoots != null)
            {
                m_ThreadCalculatePrimitiveRoots.Abort();
                m_ThreadCalculatePrimitiveRoots = null;
            }
        }

        private void DoCalculatePrimitiveRoots()
        {
            try
            {
                DateTime start = DateTime.Now;

                FireOnStart();

                m_Jump = false;

                int numberOfPrimes = 0;

                foreach (List<PrimesBigInteger> interval in intervals)
                {
                    PrimesBigInteger prime = interval[0];
                    if (!prime.IsPrime(10))
                    {
                        prime = prime.NextProbablePrime();
                    }

                    for (; prime.CompareTo(interval[1]) <= 0; prime = prime.NextProbablePrime())
                    {
                        numberOfPrimes++;

                        int row1 = log.NewLine();
                        int row2 = log.NewLine();

                        log.Info(string.Format(rsc.proot_calculating, prime.ToString()), 0, row1);

                        PrimesBigInteger primeMinus1 = prime.Subtract(PrimesBigInteger.One);
                        PrimesBigInteger numroots = primeMinus1.Phi();

                        string fmt = numroots.CompareTo(PrimesBigInteger.One) == 0 ? rsc.proot_resultcalc : rsc.proot_resultscalc;
                        string result = string.Format(fmt, prime.ToString(), numroots.ToString());
                        log.Info(result + ". " + rsc.proot_calculating, 0, row1);

                        PrimesBigInteger primitiveroot = PrimesBigInteger.One;
                        while (primitiveroot.CompareTo(prime) < 0)
                        {
                            if (m_Jump)
                            {
                                break;
                            }

                            if (IsPrimitiveRoot(primitiveroot, prime))
                            {
                                break;
                            }

                            primitiveroot = primitiveroot.Add(PrimesBigInteger.One);
                        }

                        List<PrimesBigInteger> roots = new List<PrimesBigInteger>();

                        PrimesBigInteger i = PrimesBigInteger.One;
                        bool skipped = false;

                        while (i.CompareTo(prime) < 0)
                        {
                            lock (m_JumpLockObject)
                            {
                                if (m_Jump)
                                {
                                    m_Jump = false;
                                    skipped = true;
                                    break;
                                }
                            }
                            if (PrimesBigInteger.GCD(i, primeMinus1).Equals(PrimesBigInteger.One))
                            {
                                roots.Add(primitiveroot.ModPow(i, prime));
                            }
                            i = i.Add(PrimesBigInteger.One);
                        }

                        if (skipped)
                        {
                            log.Info(result + ". " + rsc.proot_skip, 0, row1);
                        }
                        else
                        {
                            log.Info(result + ". " + rsc.proot_printing, 0, row1);
                            roots.Sort(PrimesBigInteger.Compare);
                            //string numbers = string.Join(" ", roots.ToArray().Select(x => x.ToString()));
                            StringBuilder sb = new StringBuilder();
                            foreach (PrimesBigInteger r in roots)
                            {
                                lock (m_JumpLockObject)
                                {
                                    if (m_Jump)
                                    {
                                        m_Jump = false;
                                        skipped = true;
                                        break;
                                    }
                                }
                                sb.Append(r.ToString() + " ");
                            }
                            if (skipped)
                            {
                                log.Info(result + ". " + rsc.proot_skip, 0, row1);
                            }
                            else
                            {
                                string numbers = sb.ToString();
                                log.Info(numbers, 0, row2);
                                log.Info(result + ":", 0, row1);
                            }
                        }

                        log.NewLine();
                    }
                }

                if (numberOfPrimes == 0)
                {
                    log.Info(rsc.proot_noprimes);
                }

                TimeSpan diff = DateTime.Now - start;

                StopThread();
            }
            catch (Exception)
            {
            }
        }

        private bool IsPrimitiveRoot(PrimesBigInteger root, PrimesBigInteger prime)
        {
            if (!PrimesBigInteger.GCD(root, prime).Equals(PrimesBigInteger.One))
            {
                return false;
            }

            PrimesBigInteger primeMinus1 = prime.Subtract(PrimesBigInteger.One);
            PrimesBigInteger k = PrimesBigInteger.One;
            while (k.CompareTo(primeMinus1) < 0)
            {
                if (m_Jump)
                {
                    return false;
                }

                if (root.ModPow(k, prime).Equals(PrimesBigInteger.One))
                {
                    return false;
                }

                k = k.Add(PrimesBigInteger.One);
            }

            return true;
        }

        #endregion

        #region InfoError

        private void Info(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                tbInput.Background = Brushes.LightBlue;
                lblInfo.Text = message;
                lblInfo.Foreground = Brushes.Blue;
            }
        }

        private void HideInfo()
        {
            tbInput.Background = Brushes.White;
            lblInfo.Text = "";
            lblInfo.Foreground = Brushes.Blue;
        }

        #endregion

        #region IPrimeUserControl Members

        public void Dispose()
        {
            StopThread();
        }

        #endregion

        private void tbInput_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void tbInput_KeyUp(object sender, KeyEventArgs e)
        {
            HideInfo();
            if (!string.IsNullOrEmpty(tbInput.Text))
            {
                Execute(e.Key == Key.Enter);
            }
            else
            {
                Info(rsc.proot_insert);
            }
        }

        private void miHeader_Click(object sender, RoutedEventArgs e)
        {
            int rndNumber = rndGenerate.Next(950);
            PrimesBigInteger prime = PrimesBigInteger.ValueOf(rndNumber).NextProbablePrime();
            if (!string.IsNullOrEmpty(tbInput.Text))
            {
                tbInput.Text += ", ";
            }
            else
            {
                HideInfo();
            }
            tbInput.Text += prime.ToString();
        }

        private readonly object m_JumpLockObject;
        private bool m_Jump;

        private void btnJump_Click(object sender, RoutedEventArgs e)
        {
            lock (m_JumpLockObject)
            {
                m_Jump = true;
            }
        }

        #region IPrimeUserControl Members

        public void SetTab(int i)
        {
        }

        #endregion

        #region IPrimeUserControl Members

        public void Init()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
