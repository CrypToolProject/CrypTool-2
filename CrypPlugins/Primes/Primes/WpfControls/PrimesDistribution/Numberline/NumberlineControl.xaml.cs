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

using CrypTool.PluginBase.Miscellaneous;
using Primes.Bignum;
using Primes.Library;
using Primes.Library.Function;
using Primes.Resources.lang.WpfControls.Distribution;
using Primes.WpfControls.Components;
using Primes.WpfControls.Validation;
using Primes.WpfControls.Validation.Validator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Primes.WpfControls.PrimesDistribution.Numberline
{
    /// <summary>
    /// Interaction logic for NumberlineControl.xaml
    /// </summary>
    public partial class NumberlineControl : UserControl, IPrimeDistribution
    {
        private const int PADDINGLEFT = 10;
        private const int PADDINGRIGHT = 10;
        private const double POINTERWIDTH = 15;

        private static readonly PrimesBigInteger MIN = PrimesBigInteger.Two;
        private static readonly PrimesBigInteger MAX = PrimesBigInteger.ValueOf(1000000000);

        private readonly IList<NumberButton> m_Buttons;
        private readonly IDictionary<PrimesBigInteger, NumberButton> m_ButtonsDict;
        private readonly IList<NumberButton> markedNumbers;
        private Thread m_FactorizeThread;
        private Thread m_GoldbachThread;
        private Thread m_CountPrimesThread;

        private readonly INTFunction m_EulerPhi;
        private readonly INTFunction m_Tau;
        private readonly INTFunction m_Rho;
        private readonly INTFunction m_DivSum;

        private double m_UnitSize;
        private double m_ButtonScale;
        private PrimesBigInteger m_Start;
        private PrimesBigInteger m_End;
        private PrimesBigInteger m_ActualNumber;
        private bool m_Initialized;
        private bool goldbachIsOpen = true;
        private bool infoGroupBoxInitialized = false;

        public NumberlineControl()
        {
            InitializeComponent();

            m_Buttons = new List<NumberButton>();
            markedNumbers = new List<NumberButton>();
            m_ButtonsDict = new Dictionary<PrimesBigInteger, NumberButton>();

            m_ButtonScale = 45.0;
            m_Start = MIN;
            m_End = m_Start.Add(PrimesBigInteger.ValueOf((long)m_ButtonScale - 1));

            iscTo.Execute += new ExecuteSingleDelegate(iscTo_Execute);
            iscTo.KeyDown += new ExecuteSingleDelegate(iscTo_Execute);
            iscFrom.Execute += new ExecuteSingleDelegate(iscFrom_Execute);
            iscFrom.KeyDown += new ExecuteSingleDelegate(iscFrom_Execute);
            iscFrom.OnInfoError += new MessageDelegate(iscFrom_OnInfoError);
            iscFrom.NoMargin = true;
            iscTo.NoMargin = true;

            iscFrom.SetBorderBrush(Brushes.Blue);
            iscTo.SetBorderBrush(Brushes.Violet);

            iscTo.OnInfoError += new MessageDelegate(iscFrom_OnInfoError);
            iscTo.KeyDownNoValidation += new MessageDelegate(iscTo_KeyDownNoValidation);
            iscFrom.KeyDownNoValidation += new MessageDelegate(iscTo_KeyDownNoValidation);
            IValidator<PrimesBigInteger> validatefrom = new BigIntegerMinValueValidator(null, MIN);
            InputValidator<PrimesBigInteger> inputvalidatefrom = new InputValidator<PrimesBigInteger>
            {
                DefaultValue = "2",
                Validator = validatefrom
            };
            iscFrom.AddInputValidator(InputSingleControl.Free, inputvalidatefrom);
            SetInputValidator();

            FactorizationDone += new VoidDelegate(NumberlineControl_FactorizationDone);
            GoldbachDone += new VoidDelegate(NumberlineControl_GoldbachDone);

            m_EulerPhi = new EulerPhi(logEulerPhi, lblCalcEulerPhiInfo);
            m_EulerPhi.OnStop += new VoidDelegate(m_EulerPhi_OnStop);

            m_Tau = new Tau(logTau, lblCalcTauInfo);
            m_Tau.OnStop += new VoidDelegate(m_Tau_OnStop);

            m_Rho = new Rho(logRho, lblCalcRhoInfo);
            m_Rho.OnStop += new VoidDelegate(m_Rho_OnStop);

            m_DivSum = new EulerPhiSum(logDivSum, lblCalcDividerSum);
            m_DivSum.OnStop += new VoidDelegate(m_DivSum_OnStop);
        }

        private void iscFrom_OnInfoError(string message)
        {
            tbInfoError.Text = message;
            tbInfoError.Visibility = string.IsNullOrWhiteSpace(message) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void SetInputValidator()
        {
            IValidator<PrimesBigInteger> validateto = new BigIntegerMinValueMaxValueValidator(null, PrimesBigInteger.ValueOf((int)m_ButtonScale + 1), MAX);
            InputValidator<PrimesBigInteger> inputvalidateto = new InputValidator<PrimesBigInteger>
            {
                DefaultValue = ((int)m_ButtonScale + 1).ToString(),
                Validator = validateto
            };
            iscTo.AddInputValidator(InputSingleControl.Free, inputvalidateto);
        }

        #region IPrimeUserControl Members

        public void Close()
        {
            CancelThreads();
        }

        private void CancelThreads()
        {
            CancelFactorization();
            CancelGoldbach();
            CancelCountPrimes();
            m_EulerPhi.Stop();
            m_Tau.Stop();
            m_Rho.Stop();
            m_DivSum.Stop();
            ControlHandler.SetButtonEnabled(btnCancelAll, false);
        }

        #endregion

        #region Properties

        private PrimesBigInteger ButtonScale => PrimesBigInteger.ValueOf((int)m_ButtonScale);

        private PrimesBigInteger ButtonScaleMinusOne => PrimesBigInteger.ValueOf((int)m_ButtonScale - 1);

        #endregion

        #region Drawing

        private void DrawButtons()
        {
            double x1 = 0;
            double x2 = 8;

            if (m_Buttons.Count > 0)
            {
                x1 = (double)ControlHandler.ExecuteMethod(PaintArea, "GetLeft", new object[] { m_Buttons[0] });
                x2 = (double)ControlHandler.ExecuteMethod(PaintArea, "GetLeft", new object[] { m_Buttons[1] });
            }

            UIElementCollection children = ControlHandler.GetPropertyValue(PaintArea, "Children") as UIElementCollection;
            ControlHandler.ExecuteMethod(children, "Clear");
            m_Buttons.Clear();
            m_ButtonsDict.Clear();

            double width = (double)ControlHandler.GetPropertyValue(LineArea, "ActualWidth");
            double len = width - (PADDINGLEFT + PADDINGRIGHT);
            if (len < 0)
            {
                len = 100;
            }

            m_UnitSize = len / (m_ButtonScale - 1);

            ControlHandler.ExecuteMethod(LineArea, "SetLeft", new object[] { numberline, PADDINGLEFT });
            ControlHandler.SetPropertyValue(numberline, "Width", len);

            double y = (double)ControlHandler.ExecuteMethod(LineArea, "GetTop", new object[] { numberline }) + numberline.Height / 2;

            int buttonsize = Math.Max(3, Math.Min(20, (int)(len / m_ButtonScale)));

            for (int i = 0; i < m_ButtonScale; i++)
            {
                double x = i * m_UnitSize + PADDINGLEFT;
                DrawNumberButton(m_Start.Add(PrimesBigInteger.ValueOf(i)), x, y, buttonsize, buttonsize);
            }

            if (m_Buttons != null && m_Buttons.Count > 0)
            {
                NumberButton nb = m_Buttons[m_Buttons.Count - 1];
                Canvas.SetLeft(pToNumber, Canvas.GetLeft(nb) - 6);
                lblCountPoints.Text = m_Buttons.Count.ToString();
                SetEdgeButtonColor();
            }
        }

        private void DrawNumberButton(PrimesBigInteger value, double x, double y, double width, double height)
        {
            NumberButton nb = ControlHandler.CreateObject(typeof(NumberButton)) as NumberButton;
            nb.MouseEnter += new MouseEventHandler(nb_MouseMove);
            //nb.MouseLeave += new MouseEventHandler(nb_MouseLeave);
            nb.Cursor = Cursors.Hand;
            ControlHandler.SetPropertyValue(nb, "NumberButtonStyle", NumberButtonStyle.Ellipse.ToString());
            ControlHandler.SetPropertyValue(nb, "BINumber", value);
            ControlHandler.SetPropertyValue(nb, "Width", width);
            ControlHandler.SetPropertyValue(nb, "Height", height);
            ControlHandler.SetPropertyValue(nb, "BorderBrush", Brushes.Black);
            SetButtonColor(nb);

            ControlHandler.ExecuteMethod(PaintArea, "SetTop", new object[] { nb, y - height / 2 });
            ControlHandler.ExecuteMethod(PaintArea, "SetLeft", new object[] { nb, x - width / 2 });
            ControlHandler.AddChild(nb, PaintArea);
            m_Buttons.Add(nb);
            m_ButtonsDict.Add(value, nb);
        }

        private void SetButtonColor(NumberButton btn)
        {
            PrimesBigInteger number = ControlHandler.GetPropertyValue(btn, "BINumber") as PrimesBigInteger;

            if (number.IsProbablePrime(10))
            {
                ControlHandler.SetPropertyValue(btn, "Background", Brushes.LightBlue);
            }
            else
            {
                ControlHandler.SetPropertyValue(btn, "Background", Brushes.Black);
            }
        }

        private void SetEdgeButtonColor()
        {
            if (m_Buttons != null && m_Buttons.Count > 0)
            {
                NumberButton nbFirst = m_Buttons[0];
                NumberButton nbLast = m_Buttons[m_Buttons.Count - 1];
                ControlHandler.SetPropertyValue(nbFirst, "Background", Brushes.Blue);
                ControlHandler.SetPropertyValue(nbLast, "Background", Brushes.Violet);
            }
        }

        #endregion

        #region scrolling

        private void ButtonScrollLeftClick(object sender, MouseButtonEventArgs e)
        {
            foreach (NumberButton btn in PaintArea.Children)
            {
                if (btn.BINumber.CompareTo(PrimesBigInteger.Two) <= 0)
                {
                    break;
                }

                btn.BINumber = btn.BINumber.Subtract(PrimesBigInteger.One);
                SetButtonColor(btn);
            }

            SetEdgeButtonColor();
        }

        private void DoAtomicScroll(PrimesBigInteger amount)
        {
            PrimesBigInteger len = PrimesBigInteger.ValueOf(m_Buttons.Count - 1);
            PrimesBigInteger newStart = m_Start.Add(amount);

            if (newStart.Add(len).CompareTo(MAX) > 0)
            {
                newStart = MAX.Subtract(len);
            }
            else if (newStart.CompareTo(MIN) < 0)
            {
                newStart = MIN;
            }

            amount = newStart.Subtract(m_Start);
            m_Start = newStart;
            m_End = m_Start.Add(len);

            m_ButtonsDict.Clear();

            foreach (NumberButton btn in m_Buttons)
            {
                PrimesBigInteger number = (ControlHandler.GetPropertyValue(btn, "BINumber") as PrimesBigInteger).Add(amount);
                ControlHandler.SetPropertyValue(btn, "BINumber", number);
                m_ButtonsDict.Add(number, btn);
                SetButtonColor(btn);
            }

            if (m_ActualNumber.CompareTo(m_Start) < 0)
            {
                m_ActualNumber = m_Start;
            }

            if (m_ActualNumber.CompareTo(m_End) > 0)
            {
                m_ActualNumber = m_End;
            }

            MarkNumberWithOutThreads(m_ActualNumber);

            SetFromTo();
            SetCountPrimes();
        }

        private void SetFromTo()
        {
            ControlHandler.SetPropertyValue(iscFrom, "FreeText", m_Start.ToString());
            ControlHandler.SetPropertyValue(iscTo, "FreeText", m_End.ToString());
            ControlHandler.SetPropertyValue(lblInfoCountPrimesInterval, "Text", string.Format(Distribution.numberline_numberofprimeinterval, m_Start, m_End));
            SetEdgeButtonColor();
        }

        private void btnScroll_MouseClick(object sender, RoutedEventArgs e)
        {
            int amount = 1;
            if (sender == btnScrollRight_Fast || sender == btnScrollLeft_Fast)
            {
                amount = 10;
            }

            if (sender == btnScrollLeft || sender == btnScrollLeft_Fast)
            {
                amount *= -1;
            }

            DoAtomicScroll(PrimesBigInteger.ValueOf(amount));
        }

        private void iscFrom_Execute(PrimesBigInteger value)
        {
            EnableInput();
            iscTo_Execute(value.Add(ButtonScaleMinusOne));
            MarkNumberWithOutThreads(m_Start);
        }

        private void iscTo_Execute(PrimesBigInteger value)
        {
            if (value.CompareTo(PrimesBigInteger.Two.Add(ButtonScaleMinusOne)) >= 0)
            {
                iscFrom.ResetMessages();
                iscTo.ResetMessages();
                EnableInput();
                PrimesBigInteger diff = value.Subtract(m_Start.Add(ButtonScaleMinusOne));
                DoAtomicScroll(diff);
                MarkNumberWithOutThreads(value);
            }
        }

        private void iscTo_KeyDownNoValidation(string message)
        {
            DisableInput();
        }

        private void DisableInput()
        {
            pnlContent.IsEnabled = false;
            pnlContent.Opacity = 0.5;
            pnlScrollButtons.IsEnabled = false;
            pnlScrollButtons.Opacity = 0.5;
        }

        private void EnableInput()
        {
            pnlContent.IsEnabled = true;
            pnlContent.Opacity = 1.0;
            pnlScrollButtons.IsEnabled = true;
            pnlScrollButtons.Opacity = 1.0;
            iscFrom_OnInfoError("");
        }

        #endregion

        #region Scaling

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (m_Initialized && e.NewValue != m_ButtonScale)
            {
                StartScale((int)e.NewValue);
            }
        }

        private void StartScale(int value)
        {
            ScaleNumberline(value);

            SetCountPrimes();

            if (m_ActualNumber != null)
            {
                SetPointerActualNumber(m_ActualNumber);
            }
        }

        private readonly object scalelockobject = new object();

        private void ScaleNumberline(int value)
        {
            lock (scalelockobject)
            {
                m_ButtonScale = value;
                DrawButtons();
                SetFromTo();
                SetInputValidator();
                DoAtomicScroll(PrimesBigInteger.Zero);
            }
        }

        private void btnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            if ((m_ButtonScale - 1) >= 10)
            {
                StartScale((int)(m_ButtonScale - 1));
            }

            slider.Value = m_ButtonScale;
        }

        private void btnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if ((m_ButtonScale + 1) <= slider.Maximum)
            {
                StartScale((int)(m_ButtonScale + 1));
            }

            slider.Value = m_ButtonScale;
        }

        #endregion

        private void nb_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender.GetType() == typeof(NumberButton))
            {
                MarkNumber((sender as NumberButton).BINumber);
            }
        }

        private void nb_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender != null && sender.GetType() == typeof(NumberButton))
            {
                SetButtonColor(sender as NumberButton);
            }

            UnmarkAllNumbers();
            //CancelThreads();
        }

        #region Show Number Information

        private void MarkNumber(PrimesBigInteger value)
        {
            if (m_ActualNumber != null && m_ActualNumber.Equals(value))
            {
                return;
            }

            UnmarkAllNumbers();
            CancelThreads();
            MarkNumberWithOutThreads(value);

            Dictionary<PrimesBigInteger, long> factors = value.Factorize();

            Factorize(factors);
            CalculateGoldbach(value);
            CountPrimes(value);
            m_EulerPhi.Start(value, factors);
            m_Tau.Start(value, factors);
            m_Rho.Start(value, factors);
            m_DivSum.Start(value, factors);

            ControlHandler.SetButtonEnabled(btnCancelAll, true);
        }

        private void MarkNumberWithOutThreads(PrimesBigInteger value)
        {
            NumberButton sender = null;
            try { sender = m_ButtonsDict[value]; }
            catch { }
            if (sender != null)
            {
                ControlHandler.SetPropertyValue((sender as NumberButton), "Background", Brushes.Yellow);
            }

            m_ActualNumber = value;
            HideInfoPanels();
            if (!infoGroupBoxInitialized)
            {
                HideInfoGroupBoxes();
                infoGroupBoxInitialized = true;
            }
            SetPointerActualNumber(value);
            SetInfoActualNumber(value);
            SetNeighborPrimes(value);
            SetTwinPrimes(value);
            SetSextupletPrimes(value);
            SetQuadrupletPrimes(value);
            SetEdgeButtonColor();
        }

        private void SetPointerActualNumber(PrimesBigInteger value)
        {
            if (m_ButtonsDict.ContainsKey(value))
            {
                NumberButton btn = m_ButtonsDict[value];
                double left = Canvas.GetLeft(btn) + (btn.Width - POINTERWIDTH) / 2;
                Canvas.SetLeft(pActualNumber, left);
                SetInfoActualNumber(value);
            }
        }

        private void HideInfoPanels()
        {
            //lblCalcGoldbachInfo.Text = "";
            pnlQuadrupletPrimes.Visibility = Visibility.Collapsed;
            lblTwinPrimes.Visibility = Visibility.Collapsed;
            pnlSixTupletPrimes.Visibility = Visibility.Collapsed;
            lblCalcDividerSum.Visibility = Visibility.Collapsed;
            lblCalcRhoInfo.Visibility = Visibility.Collapsed;
            lblCalcTauInfo.Visibility = Visibility.Collapsed;
            lblCalcEulerPhiInfo.Visibility = Visibility.Collapsed;
        }

        private void SetInfoActualNumber(PrimesBigInteger value)
        {
            setActualNumberText(value);
            string info = string.Empty;
            if (value.IsProbablePrime(10))
            {
                lblActualNumber.Foreground = Brushes.Red;
                info = Distribution.numberline_isprime;
            }
            else
            {
                lblActualNumber.Foreground = Brushes.Black;
            }
            lblActualNumberInfo.Text = info;
        }

        private void setActualNumberText(PrimesBigInteger value)
        {
            string text = value.ToString("D");
            lblActualNumber.Text = text;
            //if (m_Start.Add(m_End).Divide(PrimesBigInteger.Two).CompareTo(value) > 0)
            //{
            //    textActualNumberleft.Visibility = Visibility.Collapsed;
            //    textActualNumberright.Visibility = Visibility.Visible;
            //    textActualNumberright.Text = text;
            //}
            //else
            //{
            //    textActualNumberright.Visibility = Visibility.Collapsed;
            //    textActualNumberleft.Visibility = Visibility.Visible;
            //    textActualNumberleft.Text = text;
            //}

            textActualNumber.Text = text;
        }

        private void SetNeighborPrimes(PrimesBigInteger value)
        {
            lblNextPrime.Text = value.NextProbablePrime().ToString("D");
            lblPriorPrime.Text = value.PriorProbablePrime(true).ToString("D");
        }

        private void SetCountPrimes()
        {
            ControlHandler.SetPropertyValue(lblCountPrimesPi, "Text", string.Format(Distribution.numberline_countprimespin, CountPrimesPi.ToString()));
            ControlHandler.SetPropertyValue(lblCountPrimesGauss, "Text", string.Format(Distribution.numberline_countprimesgauss, CountPrimesGauss.ToString("N")));
        }

        private int CountPrimesPi
        {
            get
            {
                int result = 0;
                foreach (PrimesBigInteger key in m_ButtonsDict.Keys)
                {
                    result += (key.IsPrime(10)) ? 1 : 0;
                }
                return result;
            }
        }

        private double CountPrimesGauss
        {
            get
            {
                double a = double.Parse(m_Buttons[0].BINumber.ToString());
                double b = double.Parse(m_Buttons[m_Buttons.Count - 1].BINumber.ToString());
                double result = (b / (Math.Log(b))) - (a / (Math.Log(a)));
                return result;
            }
        }

        private void SetTwinPrimes(PrimesBigInteger value)
        {
            PrimesBigInteger twin1 = value.Subtract(PrimesBigInteger.One);
            PrimesBigInteger twin2 = value.Add(PrimesBigInteger.One);
            PrimesBigInteger tmp = null;

            if (twin1.IsPrime(20) && twin2.IsPrime(20))
            {
                lblTwinPrimes.Text = string.Format(Distribution.numberline_insidetwinprime, value, twin1, twin2);
                lblTwinPrimes.Visibility = Visibility.Visible;
            }
            else if (value.IsTwinPrime(ref tmp))
            {
                twin1 = PrimesBigInteger.Min(value, tmp);
                twin2 = PrimesBigInteger.Max(value, tmp);
                if (m_ButtonsDict.ContainsKey(twin1))
                {
                    MarkNumber(m_ButtonsDict[twin1]);
                }

                if (m_ButtonsDict.ContainsKey(twin2))
                {
                    MarkNumber(m_ButtonsDict[twin2]);
                }

                lblTwinPrimes.Text = string.Format(Distribution.numberline_istwinprime, twin1, twin2);
                lblTwinPrimes.Visibility = Visibility.Visible;
            }

            PrimesBigInteger a = null;
            PrimesBigInteger b = null;
            string text = "";
            twin1.PriorTwinPrime(ref a, ref b);
            if (a.CompareTo(twin1) < 0)
            {
                text = string.Format(Distribution.numberline_priortwinprime, a, b) + " ";
            }

            twin1.Add(PrimesBigInteger.One).NextTwinPrime(ref a, ref b);
            text += string.Format(Distribution.numberline_nexttwinprime, a, b);

            lblTwinPrimes2.Text = text;
        }

        private void SetQuadrupletPrimes(PrimesBigInteger value)
        {
            PrimesBigInteger first = null;

            if (IsQuadrupletPrime(value, ref first))
            {
                string text = MarkQuadrupletPrimes(first);
                if (value.Equals(PrimesBigInteger.ValueOf(11)) || value.Equals(PrimesBigInteger.ValueOf(13)))
                {
                    text = MarkQuadrupletPrimes(PrimesBigInteger.Five) + " " + text;
                }

                lblQuadrupletPrimes.Text = text;
                pnlQuadrupletPrimes.Visibility = Visibility.Visible;
            }
        }

        private string MarkQuadrupletPrimes(PrimesBigInteger first)
        {
            List<int> diffs = new List<int> { 0, 2, 6, 8 };
            List<PrimesBigInteger> l = new List<PrimesBigInteger>();

            foreach (int d in diffs)
            {
                PrimesBigInteger p = first.Add(PrimesBigInteger.ValueOf(d));
                l.Add(p);
                if (m_ButtonsDict.ContainsKey(p))
                {
                    MarkNumber(m_ButtonsDict[p]);
                }
            }

            return string.Format(Distribution.numberline_isquadtrupletprime, l[0], l[1], l[2], l[3]);
        }

        private bool IsQuadrupletPrime(PrimesBigInteger value, ref PrimesBigInteger first)
        {
            PrimesBigInteger twin = null;

            if (!value.IsTwinPrime(ref twin))
            {
                return false;
            }

            PrimesBigInteger twin1 = PrimesBigInteger.Min(value, twin);

            if ((twin1 + 6).IsTwinPrime(ref twin))
            {
                first = twin1;
            }
            else if (twin1 > 6 && (twin1 - 6).IsTwinPrime(ref twin))
            {
                first = twin1 - 6;
            }
            else
            {
                return false;
            }

            return true;
        }

        private void SetSextupletPrimes(PrimesBigInteger value)
        {
            if (!value.IsProbablePrime(10))
            {
                return;
            }

            PrimesBigInteger first = null;

            if (value.Equals(PrimesBigInteger.Seven))
            {
                first = PrimesBigInteger.ValueOf(11);  // 7 is the only prime that doesn't match the pattern, so handle this case separately
            }
            else if (!IsQuadrupletPrime(value, ref first) && !IsQuadrupletPrime(value.Add(PrimesBigInteger.Four), ref first) && !IsQuadrupletPrime(value.Subtract(PrimesBigInteger.Four), ref first))
            {
                return;
            }

            first = first.Subtract(PrimesBigInteger.Four);
            if (!first.IsPrime(10))
            {
                return;
            }

            if (!first.Add(PrimesBigInteger.ValueOf(16)).IsPrime(10))
            {
                return;
            }

            List<int> diffs = new List<int> { 0, 4, 6, 10, 12, 16 };
            List<PrimesBigInteger> l = new List<PrimesBigInteger>();

            foreach (int d in diffs)
            {
                PrimesBigInteger p = first.Add(PrimesBigInteger.ValueOf(d));
                l.Add(p);
                if (m_ButtonsDict.ContainsKey(p))
                {
                    MarkNumber(m_ButtonsDict[p]);
                }
            }

            lblSixTupletPrimes.Text = string.Format(Distribution.numberline_issixtupletprime, l[0], l[1], l[2], l[3], l[4], l[5]);
            pnlSixTupletPrimes.Visibility = Visibility.Visible;
        }

        #endregion

        #region Factorization

        private event VoidDelegate FactorizationDone;

        private void NumberlineControl_FactorizationDone()
        {
            ControlHandler.SetPropertyValue(lblCalcFactorizationInfo, "Text", "(fertig)");
            ControlHandler.SetPropertyValue(lblCalcFactorization, "Text", "");

            CancelFactorization();
        }

        private void Factorize(object value)
        {
            CancelFactorization();

            m_FactorizeThread = new Thread(new ParameterizedThreadStart(new ObjectParameterDelegate(DoFactorize)))
            {
                CurrentCulture = Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture,
                Priority = ThreadPriority.Normal
            };
            m_FactorizeThread.Start(value);
        }

        private void DoFactorize(object o)
        {
            if (o == null)
            {
                return;
            }

            Dictionary<PrimesBigInteger, long> factors = null;
            PrimesBigInteger value = null;

            if (o.GetType() == typeof(PrimesBigInteger))
            {
                ControlHandler.SetPropertyValue(lblCalcFactorizationInfo, "Text", Distribution.numberline_factorizationcalculating);
                value = o as PrimesBigInteger;
                factors = value.Factorize();
            }
            else if (o.GetType() == typeof(Dictionary<PrimesBigInteger, long>))
            {
                factors = o as Dictionary<PrimesBigInteger, long>;
                value = PrimesBigInteger.Refactor(factors);
            }

            if (factors != null)
            {
                string s = value.ToString();
                if (!value.IsPrime(20) && !value.Equals(PrimesBigInteger.One))
                {
                    s += " = " + string.Join(" * ", factors.Keys.Select(i => i + ((factors[i] > 1) ? "^" + factors[i] : "")).ToArray());
                }

                ControlHandler.SetPropertyValue(lblFactors, "Visibility", Visibility.Visible);
                ControlHandler.SetPropertyValue(lblFactors, "Text", s);
            }

            if (FactorizationDone != null)
            {
                FactorizationDone();
            }
        }

        private void DoFactorizeInfo()
        {
            ControlHandler.SetPropertyValue(lblCalcFactorizationInfo, "Visibility", Visibility.Visible);
            while (m_FactorizeThread != null && m_FactorizeThread.ThreadState == System.Threading.ThreadState.Running)
            {
                string text = ControlHandler.GetPropertyValue(lblCalcFactorization, "Text") as string;
                text += ".";

                if (text.Length > 3)
                {
                    text = "";
                }

                ControlHandler.SetPropertyValue(lblCalcFactorization, "Text", text);
                Thread.Sleep(500);
            }
        }

        private void CancelFactorization()
        {
            if (m_FactorizeThread != null)
            {
                m_FactorizeThread.Abort();
                m_FactorizeThread = null;
            }
            setCancelAllEnabled();
            ControlHandler.SetPropertyValue(lblCalcFactorizationInfo, "Visibility", Visibility.Hidden);
        }

        #endregion

        #region Goldbach

        private event VoidDelegate GoldbachDone;

        private void NumberlineControl_GoldbachDone()
        {
            //CancelGoldbach();
            ControlHandler.SetButtonEnabled(btnCancelAll, false);
        }

        private void CalculateGoldbach(PrimesBigInteger value)
        {
            CancelGoldbach();

            logGoldbach.Clear();
            logGoldbach.Columns = 1;
            m_GoldbachThread = new Thread(new ParameterizedThreadStart(new ObjectParameterDelegate(DoCalculateGoldbach)))
            {
                CurrentCulture = Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture,
                Priority = ThreadPriority.Normal
            };
            m_GoldbachThread.Start(value);
        }

        private void DoCalculateGoldbach(object o)
        {
            if (o == null || o.GetType() != typeof(PrimesBigInteger))
            {
                return;
            }

            PrimesBigInteger value = o as PrimesBigInteger;

            if (value.Mod(PrimesBigInteger.Two).Equals(PrimesBigInteger.One)) // value is odd
            {
                ControlHandler.SetPropertyValue(lblGoldbachInfoCalc, "Text", string.Format(Distribution.numberline_isodd, value));
                ControlHandler.SetPropertyValue(gbGoldbach, "Visibility", Visibility.Collapsed);
            }
            else if (value.Equals(PrimesBigInteger.Two))  // value = 2
            {
                ControlHandler.SetPropertyValue(lblGoldbachInfoCalc, "Text", Distribution.numberline_istwo);
                ControlHandler.SetPropertyValue(gbGoldbach, "Visibility", Visibility.Collapsed);
            }
            else // value is even and not prime
            {
                int counter = 0;
                int maxlines = 1000;

                if (!value.IsProbablePrime(10))
                {
                    long x = value.LongValue;
                    int i = 0;
                    long sum1 = PrimeNumbers.primes[i];
                    while (sum1 <= x / 2)
                    {
                        long sum2 = x - sum1;
                        if (BigIntegerHelper.IsProbablePrime(sum2))
                        //if (PrimeNumbers.isprime.Contains(sum2))
                        {
                            counter++;

                            if (counter < maxlines)
                            {
                                logGoldbach.Info(string.Format("{0} + {1}   ", sum1, sum2));
                            }
                            else if (counter == maxlines)
                            {
                                logGoldbach.Info(string.Format(Distribution.numberline_goldbachmaxlines, maxlines));
                            }

                            if (counter % 50 == 0)
                            {
                                string fmt = (counter == 1) ? Distribution.numberline_goldbachfoundsum : Distribution.numberline_goldbachfoundsums;
                                ControlHandler.SetPropertyValue(lblGoldbachInfoCalc, "Text", string.Format(fmt, counter, value));
                            }
                        }
                        sum1 = (++i < PrimeNumbers.primes.Length) ? PrimeNumbers.primes[i] : (long)BigIntegerHelper.NextProbablePrime(sum1 + 1);
                    }

                    string fmt1 = (counter == 1) ? Distribution.numberline_goldbachfoundsum : Distribution.numberline_goldbachfoundsums;
                    ControlHandler.SetPropertyValue(lblGoldbachInfoCalc, "Text", string.Format(fmt1, counter, value));
                    ControlHandler.SetPropertyValue(gbGoldbach, "Visibility", goldbachIsOpen ? Visibility.Visible : Visibility.Collapsed);
                }
            }

            if (GoldbachDone != null)
            {
                GoldbachDone();
            }
        }

        private void CancelGoldbach()
        {
            if (m_GoldbachThread != null)
            {
                m_GoldbachThread.Abort();
                m_GoldbachThread = null;
            }
            setCancelAllEnabled();
        }

        #endregion

        #region Counting Primes

        public void CountPrimes(PrimesBigInteger value)
        {
            CancelCountPrimes();
            m_CountPrimesThread = new Thread(new ParameterizedThreadStart(new ObjectParameterDelegate(DoCountPrimes)))
            {
                CurrentCulture = Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture,
                Priority = ThreadPriority.Normal
            };
            m_CountPrimesThread.Start(value);
        }

        public void DoCountPrimes(object o)
        {
            if (o != null && o.GetType() == typeof(PrimesBigInteger))
            {
                FunctionPiX func = new FunctionPiX
                {
                    ShowIntermediateResult = true
                };
                m_refreshcount = 0;
                func.Executed += new ObjectParameterDelegate(func_Executed);
                double erg = func.Execute((o as PrimesBigInteger).DoubleValue);
                ControlHandler.SetPropertyValue(lblInfoCountPrimes, "Text", StringFormat.FormatDoubleToIntString(erg));
            }
        }

        private int m_refreshcount;

        private void func_Executed(object obj)
        {
            m_refreshcount++;
            if (m_refreshcount == 1000)
            {
                m_refreshcount = 0;
                ControlHandler.SetPropertyValue(lblInfoCountPrimes, "Text", obj.ToString());
            }
        }

        public void CancelCountPrimes()
        {
            if (m_CountPrimesThread != null)
            {
                m_CountPrimesThread.Abort();
                m_CountPrimesThread = null;
            }
            setCancelAllEnabled();
        }

        #endregion

        #region Euler-Phi

        private void m_EulerPhi_OnStop()
        {
            m_EulerPhi.Stop();
            setCancelAllEnabled();
        }

        #endregion

        #region Tau

        private void m_Tau_OnStop()
        {
            m_Tau.Stop();
            setCancelAllEnabled();
        }

        #endregion

        #region Rho

        private void m_Rho_OnStop()
        {
            m_Rho.Stop();
            setCancelAllEnabled();
        }

        #endregion

        #region EulerPhiSum

        private void m_DivSum_OnStop()
        {
            m_DivSum.Stop();
            setCancelAllEnabled();
        }

        #endregion

        #region Misc

        private void MarkNumber(NumberButton nb)
        {
            if (!markedNumbers.Contains(nb))
            {
                markedNumbers.Add(nb);
            }

            ControlHandler.SetPropertyValue(nb, "Background", Brushes.Red);
        }

        private void UnmarkAllNumbers()
        {
            foreach (NumberButton nb in m_Buttons)
            {
                SetButtonColor(nb);
            }

            markedNumbers.Clear();
        }

        #endregion

        private void btnHelp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnlineHelp.OnlineHelpActions action = Primes.OnlineHelp.OnlineHelpActions.None;
            if (sender == btnHelpCountPrimes)
            {
                action = Primes.OnlineHelp.OnlineHelpActions.Graph_PrimesCount;
            }
            else if (sender == btnHelpFactorize)
            {
                action = Primes.OnlineHelp.OnlineHelpActions.Factorization_Factorization;
            }
            else if (sender == btnHelpGoldbach)
            {
                action = Primes.OnlineHelp.OnlineHelpActions.Distribution_Goldbach;
            }
            else if (sender == btnHelpTwinPrimes)
            {
                action = Primes.OnlineHelp.OnlineHelpActions.Distribution_TwinPrimes;
            }
            else if (sender == btnHelpQuadrupletPrimes)
            {
                action = Primes.OnlineHelp.OnlineHelpActions.Distribution_QuadrupletPrimes;
            }
            else if (sender == btnHelpSixTupletPrimes)
            {
                action = Primes.OnlineHelp.OnlineHelpActions.Distribution_SixTupletPrimes;
            }
            else if (sender == btnHelpEulerPhi)
            {
                action = Primes.OnlineHelp.OnlineHelpActions.Distribution_EulerPhi;
            }
            else if (sender == btnHelpTau)
            {
                action = Primes.OnlineHelp.OnlineHelpActions.Distribution_Tau;
            }
            else if (sender == btnHelpRho)
            {
                action = Primes.OnlineHelp.OnlineHelpActions.Distribution_Sigma;
            }
            else if (sender == btnHelpDivSum)
            {
                action = Primes.OnlineHelp.OnlineHelpActions.Distribution_EulerPhiSum;
            }

            if (action != Primes.OnlineHelp.OnlineHelpActions.None)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(action);
            }

            e.Handled = true;
        }

        #region IPrimeDistribution Members

        public void Init()
        {
            if (!m_Initialized)
            {
                DrawButtons();
                SetCountPrimes();
                m_Initialized = true;
                m_ActualNumber = MIN;
                SetPointerActualNumber(m_ActualNumber);
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            DrawButtons();

            Canvas.SetLeft(pFromNumber, 5);
            PrimesBigInteger key = m_Start.Add(PrimesBigInteger.ValueOf(m_ButtonsDict.Count - 1));
            if (m_ButtonsDict.ContainsKey(key))
            {
                NumberButton nb = m_ButtonsDict[key];
                Canvas.SetLeft(pToNumber, Canvas.GetLeft(nb) - 5);
            }
        }

        public void Dispose()
        {
            CancelThreads();
        }

        #endregion

        private void HideInfoGroupBoxes()
        {
            gbEulerPhi.Visibility = Visibility.Collapsed;
            gbTau.Visibility = Visibility.Collapsed;
            gbRho.Visibility = Visibility.Collapsed;
            gbDivSum.Visibility = Visibility.Collapsed;
            gbGoldbach.Visibility = Visibility.Collapsed;
        }

        private void lblCalcInfo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GroupBox gb = null;
            TextBlock _sender = sender as TextBlock;

            if (_sender == lblCalcEulerPhiInfo)
            {
                gb = gbEulerPhi;
            }
            else if (_sender == lblCalcTauInfo)
            {
                gb = gbTau;
            }
            else if (_sender == lblCalcRhoInfo)
            {
                gb = gbRho;
            }
            else if (_sender == lblCalcDividerSum)
            {
                gb = gbDivSum;
            }
            else if (_sender == lblGoldbachInfoCalc)
            {
                gb = gbGoldbach;
            }

            if (gb != null)
            {
                gb.Visibility = (gb.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
            }

            goldbachIsOpen = (gbGoldbach.Visibility == Visibility.Visible);
        }

        private void ActuallNumberButtonArea_MouseMove(object sender, MouseEventArgs e)
        {
            double x = e.GetPosition(ActuallNumberButtonArea).X - PADDINGLEFT;
            int div = (int)(x / m_UnitSize + 0.5);
            Canvas.SetLeft(pActualNumber, div * m_UnitSize + PADDINGLEFT - POINTERWIDTH / 2);
            PrimesBigInteger val = null;
            try { val = m_Buttons[div].BINumber; }
            catch { val = m_Start; }
            if (val != null && !val.Equals(m_ActualNumber))
            {
                MarkNumber(val);
            }
        }

        #region events

        public event VoidDelegate Execute;

        public void FireExecute()
        {
            if (Execute != null)
            {
                Execute();
            }
        }

        public event VoidDelegate Stop;

        public void FireStop()
        {
            if (Stop != null)
            {
                Stop();
            }
        }

        #endregion

        private void btnCancelAll_Click(object sender, RoutedEventArgs e)
        {
            UnmarkAllNumbers();
            CancelThreads();
        }

        private void setCancelAllEnabled()
        {
            bool enabled = false;
            enabled =
              (m_FactorizeThread != null && m_FactorizeThread.ThreadState == System.Threading.ThreadState.Running) ||
              (m_GoldbachThread != null && m_GoldbachThread.ThreadState == System.Threading.ThreadState.Running) ||
              (m_CountPrimesThread != null && m_CountPrimesThread.ThreadState == System.Threading.ThreadState.Running) ||
              m_Tau.IsRunning ||
              m_Rho.IsRunning ||
              m_EulerPhi.IsRunning ||
              m_DivSum.IsRunning;
            ControlHandler.SetButtonEnabled(btnCancelAll, enabled);
        }

        private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (TextBlock child in GetVisibleChildren(pnlContent).OfType<TextBlock>())
            {
                if (child.Inlines.Any())
                {
                    TextRange range = new TextRange(child.Inlines.FirstInline.ContentStart, child.Inlines.LastInline.ContentEnd);
                    sb.AppendLine(range.Text);
                }
                else
                {
                    sb.AppendLine(child.Text);
                }
            }
            Clipboard.SetText(sb.ToString());
        }

        private IEnumerable<DependencyObject> GetVisibleChildren(DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if ((child as UIElement)?.IsVisible ?? true)
                {
                    yield return child;
                    foreach (DependencyObject subChild in GetVisibleChildren(child))
                    {
                        yield return subChild;
                    }
                }
            }
        }
    }
}
