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
using Primes.WpfControls.Components;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Primes.WpfControls.Primetest.Numbergrid
{
    /// <summary>
    /// Interaction logic for Numbergrid.xaml
    /// </summary>
    public delegate void NumberButtonClickDelegate(NumberButton value);

    public partial class Numbergrid : UserControl
    {
        private const short MAX = 20;
        private const short MIN = 2;
        //private IDictionary<long, bool> m_Sieved;
        private long[] m_Sieved;

        #region Constructors

        public Numbergrid()
        {
            InitializeComponent();
            Rows = 20;
            Columns = 20;
            numbergrid.Children.Clear();
            border.BorderThickness = new Thickness(0);
            m_RemovedNumbers = new List<PrimesBigInteger>();
            m_RemovedMods = new List<PrimesBigInteger>();
            //m_Sieved = new Dictionary<long, bool>();
            if (m_Limit != null)
            {
                DrawGrid();
            }
        }

        #endregion

        #region Properties

        private int m_Rows;

        public int Rows
        {
            get => m_Rows;
            set => m_Rows = Math.Min(Math.Max(value, MIN), MAX);
        }

        private int m_Columns;

        public int Columns
        {
            get => m_Columns;
            set => m_Columns = Math.Min(Math.Max(value, MIN), MAX);
        }

        private PrimesBigInteger m_Limit;

        public PrimesBigInteger Limit
        {
            get => m_Limit;
            set
            {
                m_Limit = value;
                m_Sieved = new long[m_Limit.LongValue + 1];
                for (long i = 0; i <= m_Limit.LongValue; i++)
                {
                    m_Sieved[i] = i;
                }
                m_ButtonColor = Brushes.White;
                m_RemovedMods.Clear();
                InitButtons();
                RemoveNumber(PrimesBigInteger.One);
            }
        }

        #endregion

        #region DeleteNumbers

        private readonly IList<PrimesBigInteger> m_RemovedNumbers;
        private readonly IList<PrimesBigInteger> m_RemovedMods;

        public void RemoveNumber(PrimesBigInteger value)
        {
            m_RemovedNumbers.Add(value);

            if (m_Limit != null)
            {
                RedrawButtons();
            }
        }

        public void RemoveMulipleOf(PrimesBigInteger value)
        {
            for (PrimesBigInteger i = value * 2; i <= m_Limit; i = i + value)
            {
                m_Sieved[i.LongValue] = -1;
            }

            m_RemovedMods.Add(value);

            if (value.Pow(2) <= GetMaxVisibleValue())
            {
                RedrawButtons();
            }
        }

        private void RedrawButtons()
        {
            ScrollGrid(PrimesBigInteger.Zero, false);
        }

        public void ClearRemovedNumbers(bool redraw)
        {
            m_RemovedNumbers.Clear();
            m_RemovedMods.Clear();

            if (redraw)
            {
                RedrawButtons();
            }
        }

        public IList<PrimesBigInteger> Remainders
        {
            get
            {
                List<PrimesBigInteger> result = new List<PrimesBigInteger>();
                return result;
            }
        }

        public void Reset()
        {
            if (m_Limit != null)
            {
                m_ButtonColor = null;
                ClearRemovedNumbers(true);
                SetButtonStatus();
            }
        }

        #endregion

        #region Events

        public event NumberButtonClickDelegate NumberButtonClick;

        #endregion

        #region Drawing

        private void DrawGrid()
        {
            border.BorderThickness = new Thickness(1);
            numbergrid.RowDefinitions.Clear();
            numbergrid.ColumnDefinitions.Clear();

            for (int i = 0; i < Rows + Rows - 1; i++)
            {
                RowDefinition rd = new RowDefinition();
                if (i % 2 == 0)
                {
                    rd.Height = new GridLength(0.1, GridUnitType.Star);
                }
                else
                {
                    rd.Height = new GridLength(1, GridUnitType.Pixel);
                    Rectangle rect = new Rectangle
                    {
                        Height = 1.0,
                        Fill = Brushes.Black
                    };
                    Grid.SetColumnSpan(rect, Columns + Columns - 1);
                    Grid.SetRow(rect, i);
                    numbergrid.Children.Add(rect);
                }
                numbergrid.RowDefinitions.Add(rd);
            }

            for (int i = 0; i < Columns + Columns - 1; i++)
            {
                ColumnDefinition cd = new ColumnDefinition();
                if (i % 2 == 0)
                {
                    cd.Width = new GridLength(0.1, GridUnitType.Star);
                }
                else
                {
                    cd.Width = new GridLength(1, GridUnitType.Pixel);
                    Rectangle rect = new Rectangle
                    {
                        Width = 1.0,
                        Fill = Brushes.Black
                    };
                    Grid.SetRowSpan(rect, Rows + Rows - 1);
                    Grid.SetColumn(rect, i);
                    numbergrid.Children.Add(rect);
                }

                numbergrid.ColumnDefinitions.Add(cd);
            }
        }

        private Brush m_ButtonColor = null;

        public void MarkNumbers(Brush color)
        {
            m_ButtonColor = color;
            ScrollGrid(PrimesBigInteger.Zero);
        }

        #endregion

        #region Buttons

        private void InitButtons()
        {
            btnBack.Visibility = btnCompleteBack.Visibility = Visibility.Visible;
            btnForward.Visibility = btnCompleteForward.Visibility = Visibility.Visible;

            if (numbergrid.RowDefinitions.Count >= MIN)
            {
                numbergrid.Children.Clear();
                DrawGrid();
            }

            PrimesBigInteger counter = 1;

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    NumberButton btn = new NumberButton
                    {
                        NumberButtonStyle = Primes.WpfControls.Components.NumberButtonStyle.Button.ToString(),

                        ShowContent = true,
                        Number = (counter).ToString()
                    };
                    btn.Click += new RoutedEventHandler(NumberButton_Click);
                    Grid.SetColumn(btn, 2 * j);
                    Grid.SetRow(btn, 2 * i);

                    btn.Background = Brushes.White;

                    numbergrid.Children.Add(btn);
                    if (counter.CompareTo(m_Limit) > 0)
                    {
                        btn.Visibility = Visibility.Hidden;
                    }

                    if (m_RemovedNumbers.Contains(btn.BINumber))
                    {
                        btn.Visibility = Visibility.Hidden;
                    }

                    counter = counter.Add(PrimesBigInteger.ValueOf(1));
                }
            }

            SetButtonStatus();
        }

        private void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null && sender.GetType() == typeof(NumberButton))
            {
                if (NumberButtonClick != null)
                {
                    NumberButtonClick(sender as NumberButton);
                }
            }
        }

        #endregion

        #region Forward/Back Button

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            PrimesBigInteger amount = PrimesBigInteger.ValueOf(Rows * -1);
            if (sender == btnCompleteBack)
            {
                amount = PrimesBigInteger.One.Subtract((numbergrid.Children[Rows + Columns - 2] as NumberButton).BINumber);
            }

            ScrollGrid(amount, false);
        }

        private void btnForward_Click(object sender, RoutedEventArgs e)
        {
            PrimesBigInteger rows = PrimesBigInteger.ValueOf(Rows);
            PrimesBigInteger amount = rows;
            if (sender == btnCompleteForward)
            {
                amount =
                  m_Limit.Subtract((numbergrid.Children[Rows + Columns - 2 + (Columns * Rows) - 1] as NumberButton).BINumber);
                amount = amount.Add(rows.Subtract(amount.Mod(rows)));
            }

            ScrollGrid(amount, false);
        }

        private delegate void ParameterDelegate(object o);

        private void ScrollGrid(PrimesBigInteger amount, bool AsThread)
        {
            if (AsThread)
            {
                Thread t = new Thread(new ParameterizedThreadStart(new ParameterDelegate(ScrollGrid)))
                {
                    CurrentCulture = Thread.CurrentThread.CurrentCulture,
                    CurrentUICulture = Thread.CurrentThread.CurrentUICulture
                };
                t.Start(amount);
            }
            else
            {
                ScrollGrid(amount);
            }
        }

        private void DoScrollGrid(PrimesBigInteger amount)
        {
            bool keepColor = true;
            int counter = 0;
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    NumberButton btn = GetNumberButton(Rows + Columns - 2 + counter);
                    if (btn != null)
                    {
                        PrimesBigInteger newVal = btn.BINumber.Add(amount);
                        btn.BINumber = newVal;
                        if (newVal.CompareTo(m_Limit) > 0 || newVal.CompareTo(PrimesBigInteger.One) < 0)
                        {
                            if (m_Limit.CompareTo(PrimesBigInteger.ValueOf(Rows * Columns)) < 0)
                            {
                                return;
                            }

                            ControlHandler.SetButtonVisibility(btn, Visibility.Hidden);
                        }
                        else
                        {
                            // Color the Buttons
                            if (m_ButtonColor != null)
                            {
                                ControlHandler.SetPropertyValue(btn, "Background", m_ButtonColor);
                            }
                            else
                            {
                                if (!keepColor)
                                {
                                    btn.Background = Brushes.White;
                                }
                            }
                            bool isMultipleOfRemovedNumbers = m_Sieved[btn.BINumber.LongValue] == -1;
                            if (
                              isMultipleOfRemovedNumbers
                              || m_RemovedNumbers.Contains(btn.BINumber))
                            {
                                ControlHandler.SetButtonVisibility(btn, Visibility.Hidden);
                            }
                            else
                            {
                                ControlHandler.SetButtonVisibility(btn, Visibility.Visible);
                            }
                        }
                    }
                    counter++;
                }
            }

            SetButtonStatus();
        }

        private void ScrollGrid(object o)
        {
            if (o != null && o.GetType() == typeof(PrimesBigInteger))
            {
                DoScrollGrid(o as PrimesBigInteger);
            }
        }

        private NumberButton GetNumberButton(int index)
        {
            UIElementCollection buttons =
              ControlHandler.GetPropertyValue(numbergrid, "Children") as UIElementCollection;
            return buttons[index] as NumberButton;
        }

        private PrimesBigInteger GetMaxVisibleValue()
        {
            NumberButton nb = GetNumberButton(Rows + Columns - 2 + (Columns * Rows) - 1);
            if (nb != null)
            {
                return nb.BINumber;
            }
            else
            {
                return PrimesBigInteger.Zero;
            }
        }

        private PrimesBigInteger GetMinVisibleValue()
        {
            NumberButton nb = GetNumberButton(Rows + Columns - 2);
            if (nb != null)
            {
                return nb.BINumber;
            }
            else
            {
                return PrimesBigInteger.Zero;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if ((e.Key == Key.Down || e.Key == Key.PageDown) && ButtonForwardEnabled)
            {
                ScrollGrid(BiRows, true);
            }
            else if ((e.Key == Key.Up || e.Key == Key.PageUp) && ButtonBackEnabled)
            {
                ScrollGrid(BiRows.Multiply(PrimesBigInteger.ValueOf(-1)), true);
            }
        }

        private void SetButtonStatus()
        {
            SetButtonStatus(ButtonBackEnabled, ButtonForwardEnabled);
        }

        public void SetButtonStatus(bool backEnabled, bool forwardEnabled)
        {
            ControlHandler.SetButtonEnabled(btnForward, forwardEnabled);
            ControlHandler.SetButtonEnabled(btnCompleteForward, forwardEnabled);
            ControlHandler.SetButtonEnabled(btnBack, backEnabled);
            ControlHandler.SetButtonEnabled(btnCompleteBack, backEnabled);
        }

        private bool ButtonBackEnabled
        {
            get
            {
                NumberButton nb = GetNumberButton(Rows + Columns - 2); ;
                if (nb != null)
                {
                    return nb.BINumber.CompareTo(PrimesBigInteger.One) >= 1;
                }
                else
                {
                    return false;
                }
            }
        }

        private bool ButtonForwardEnabled
        {
            get
            {
                NumberButton nb = GetNumberButton(Rows + Columns - 2 + (Columns * Rows) - 1);
                if (nb != null)
                {
                    return nb.BINumber.CompareTo(m_Limit) <= 0;
                }
                else
                {
                    return false;
                }
            }
        }

        private PrimesBigInteger BiRows => PrimesBigInteger.ValueOf(Rows);

        private void numbergrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int scrollfactor = (e.Delta > 0) ? -2 : 2;
            if (scrollfactor > 0 && ButtonForwardEnabled)
            {
                ScrollGrid(PrimesBigInteger.ValueOf(Rows * scrollfactor), false);
            }
            else if (scrollfactor < 0 && ButtonBackEnabled)
            {
                ScrollGrid(PrimesBigInteger.ValueOf(Rows * scrollfactor), false);
            }
        }

        #endregion
    }
}
