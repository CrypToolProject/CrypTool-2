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
using Primes.WpfControls.Validation;
using Primes.WpfControls.Validation.Validator;
using System;
using System.Collections;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Primes.WpfControls.PrimesDistribution.NumberRectangle
{
    /// <summary>
    /// Interaction logic for NumberRectangleControl.xaml
    /// </summary>
    public partial class NumberRectangleControl : UserControl, IPrimeDistribution
    {
        private Thread m_DrawThread;
        private readonly ArrayList m_Buttons;

        public NumberRectangleControl()
        {
            InitializeComponent();
            Rows = 10;
            Columns = 10;
            m_Buttons = new ArrayList();
            InputValidator<PrimesBigInteger> ivHeight = new InputValidator<PrimesBigInteger>
            {
                Validator = new BigIntegerMinValueMaxValueValidator(null, PrimesBigInteger.ValueOf(10), PrimesBigInteger.ValueOf(500))
            };
            iscHeight.AddInputValidator(InputSingleControl.Free, ivHeight);
            iscHeight.Execute += new ExecuteSingleDelegate(isc_Execute);
            iscHeight.OnInfoError += new MessageDelegate(iscHeight_OnInfoError);
            iscHeight.KeyDown += new ExecuteSingleDelegate(iscWidth_KeyDown);

            InputValidator<PrimesBigInteger> ivWidth = new InputValidator<PrimesBigInteger>
            {
                Validator = new BigIntegerMinValueMaxValueValidator(null, PrimesBigInteger.ValueOf(10), PrimesBigInteger.ValueOf(500))
            };
            iscWidth.AddInputValidator(InputSingleControl.Free, ivWidth);
            iscWidth.Execute += new ExecuteSingleDelegate(isc_Execute);
            iscWidth.OnInfoError += new MessageDelegate(iscHeight_OnInfoError);
            iscWidth.KeyDown += new ExecuteSingleDelegate(iscWidth_KeyDown);

            InputValidator<PrimesBigInteger> ivStart = new InputValidator<PrimesBigInteger>
            {
                Validator = new BigIntegerMinValueValidator(null, PrimesBigInteger.ValueOf(1))
            };
            iscStart.AddInputValidator(InputSingleControl.Free, ivStart);
            iscStart.Execute += new ExecuteSingleDelegate(isc_Execute);
            iscStart.OnInfoError += new MessageDelegate(iscHeight_OnInfoError);
            iscStart.KeyDown += new ExecuteSingleDelegate(iscWidth_KeyDown);

            OnDrawStart += new VoidDelegate(NumberRectangleControl_OnDrawStart);
            OnDrawStop += new VoidDelegate(NumberRectangleControl_OnDrawStop);

            m_start = PrimesBigInteger.One;
        }

        private void iscWidth_KeyDown(PrimesBigInteger value)
        {
            lblInfoError.Text = "";
        }

        private void iscHeight_OnInfoError(string message)
        {
            lblInfoError.Visibility = Visibility.Visible;
            lblInfoError.Text = message;
        }

        #region Properties

        private const short MAX = 1000;
        private const short MIN = 2;
        private const double SIZE = 12.0;
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

        private PrimesBigInteger m_start;

        #endregion

        #region events

        private event VoidDelegate OnDrawStart;
        private event VoidDelegate OnDrawStop;

        #endregion

        #region drawing

        private void DrawGrid()
        {
            UIElementCollection childs = ControlHandler.GetPropertyValue(NumberRectangle, "Children") as UIElementCollection;
            ControlHandler.ExecuteMethod(childs, "Clear");

            RowDefinitionCollection rowdefs = ControlHandler.GetPropertyValue(NumberRectangle, "RowDefinitions") as RowDefinitionCollection;
            ControlHandler.ExecuteMethod(rowdefs, "Clear");

            ColumnDefinitionCollection coldefs = ControlHandler.GetPropertyValue(NumberRectangle, "ColumnDefinitions") as ColumnDefinitionCollection;
            ControlHandler.ExecuteMethod(coldefs, "Clear");

            for (int i = 0; i < Rows + Rows - 1; i++)
            {
                GridLength gl = GridLength.Auto;
                if (i % 2 == 0)
                {
                    gl = (GridLength)ControlHandler.CreateObject(typeof(GridLength), new Type[] { typeof(double), typeof(GridUnitType) }, new object[] { 1.0 / Rows, GridUnitType.Star });
                }
                else
                {

                    gl = (GridLength)ControlHandler.CreateObject(typeof(GridLength), new Type[] { typeof(double), typeof(GridUnitType) }, new object[] { 1, GridUnitType.Pixel });
                    //rd.Height = new GridLength();
                    //Rectangle rect = new Rectangle();
                    //rect.Height = 1.0;
                    //rect.Fill = Brushes.Black;
                    //Grid.SetColumnSpan(rect, this.Columns + this.Columns - 1);
                    //Grid.SetRow(rect, i);
                    //this.NumberRectangle.Children.Add(rect);
                }
                RowDefinition rd = ControlHandler.CreateObject(typeof(RowDefinition)) as RowDefinition;
                ControlHandler.SetPropertyValue(rd, "Height", gl);
                ControlHandler.ExecuteMethod(rowdefs, "Add", new object[] { rd });

                //this.NumberRectangle.RowDefinitions.Add(rd);
            }

            for (int i = 0; i < Columns + Columns - 1; i++)
            {
                GridLength gl = GridLength.Auto;
                if (i % 2 == 0)
                {
                    gl = (GridLength)ControlHandler.CreateObject(typeof(GridLength), new Type[] { typeof(double), typeof(GridUnitType) }, new object[] { 1.0 / Columns, GridUnitType.Star });
                }
                else
                {
                    gl = (GridLength)ControlHandler.CreateObject(typeof(GridLength), new Type[] { typeof(double), typeof(GridUnitType) }, new object[] { 1, GridUnitType.Pixel });
                    //Rectangle rect = new Rectangle();
                    //rect.Width = 1.0;
                    //rect.Fill = Brushes.Black;
                    //Grid.SetRowSpan(rect, this.Rows + this.Rows - 1);
                    //Grid.SetColumn(rect, i);
                    //this.NumberRectangle.Children.Add(rect);
                }
                ColumnDefinition cd = ControlHandler.CreateObject(typeof(ColumnDefinition)) as ColumnDefinition;
                ControlHandler.SetPropertyValue(cd, "Width", gl);

                ControlHandler.ExecuteMethod(coldefs, "Add", new object[] { cd });
            }
        }

        //private void DrawButtons()
        //{
        //  DateTime start = DateTime.Now;
        //  m_Buttons.Clear();
        //  PrimesBigInteger counter = m_start;
        //  NumberRectangle.Width = this.Columns * SIZE;
        //  NumberRectangle.Height = this.Rows * SIZE;
        //  NumberRectangle.Background = Brushes.Transparent;
        //  UIElementCollection childs = ControlHandler.GetPropertyValue(this.NumberRectangle, "Children") as UIElementCollection;
        //  ControlHandler.ExecuteMethod(childs, "Clear");
        //  for (int i = 0; i < this.Rows; i++)
        //  {
        //    for (int j = 0; j < this.Columns; j++)
        //    {
        //        Rectangle btn = ControlHandler.CreateObject(typeof(Rectangle)) as Rectangle;
        //        ControlHandler.SetPropertyValue(btn, "Tag", counter);

        //        ControlHandler.SetPropertyValue(btn, "Height", 10);
        //        ControlHandler.SetPropertyValue(btn, "Width", 10);
        //        ControlHandler.SetPropertyValue(btn, "Fill", (!counter.IsPrime(10)) ? Brushes.White : Brushes.Red);
        //        ToolTip tt = ControlHandler.CreateObject(typeof(ToolTip)) as ToolTip;
        //        ControlHandler.SetPropertyValue(tt, "Content", counter.ToString());
        //        ControlHandler.SetPropertyValue(btn, "ToolTip", tt);

        //        //btn.MouseLeave += new MouseEventHandler(btn_MouseLeave);

        //        //NumberButton btn = ControlHandler.CreateObject(typeof(NumberButton)) as NumberButton;
        //        //ControlHandler.SetPropertyValue(btn, "ShowContent", false);

        //        //ControlHandler.SetPropertyValue(btn, "Number", (counter).ToString());
        //        //ControlHandler.SetPropertyValue(btn, "Background", (counter.IsPrime(20)) ? Brushes.Red : Brushes.White);

        //        ControlHandler.ExecuteMethod(NumberRectangle, "SetTop", new object[] { btn, i * SIZE });
        //        ControlHandler.ExecuteMethod(NumberRectangle, "SetLeft", new object[] { btn, j * SIZE });

        //        ControlHandler.ExecuteMethod(childs, "Add", new object[] { btn });
        //        m_Buttons.Add(btn);
        //        counter = counter.Add(PrimesBigInteger.ValueOf(1));
        //    }
        //  }
        //  TimeSpan diff = DateTime.Now - start;
        //  Debug.WriteLine(string.Format("Create: Std={0}, Min={1}, sek={2}, MS={3}", new object[] { diff.Hours, diff.Minutes, diff.Seconds, diff.Milliseconds }));
        //}

        private void DrawButtons()
        {
            Cursor = Cursors.Wait;
            //DateTime start = DateTime.Now;

            //PrimesBigInteger counter = m_start;
            long counter = m_start.LongValue;
            //DesignerItem.Width = this.Columns * SIZE;
            //DesignerItem.Height = this.Rows * SIZE;
            NumberRectangle.Width = Columns * SIZE;
            NumberRectangle.Height = Rows * SIZE;
            DesignerItem.Width = Columns * SIZE; ;
            DesignerItem.Height = Rows * SIZE;

            NumberRectangle.Background = Brushes.Transparent;
            NumberRectangle.Children.Clear();
            //UIElementCollection childs = ControlHandler.GetPropertyValue(this.NumberRectangle, "Children") as UIElementCollection;
            //ControlHandler.ExecuteMethod(childs, "Clear");
            Rectangle btn;
            int c = 1;
            for (int i = 0; i < Rows; i++)
            {
                TextBlock lblRow = new TextBlock();
                lblRow.SizeChanged += new SizeChangedEventHandler(lblRow_SizeChanged);
                lblRow.Text = counter.ToString();
                Canvas.SetLeft(lblRow, 0);
                Canvas.SetTop(lblRow, i * SIZE);
                NumberRectangle.Children.Add(lblRow);

                for (int j = 0; j < Columns; j++)
                {
                    bool isPrime = PrimesBigInteger.ValueOf(counter).IsPrime(10);
                    if (c % 2 == 0 || isPrime)
                    {
                        btn = new Rectangle
                        {
                            Height = SIZE,
                            Width = SIZE
                        };

                        //btn = ControlHandler.CreateObject(typeof(Rectangle)) as Rectangle;
                        //ControlHandler.SetPropertyValue(btn, "Height", SIZE);
                        //ControlHandler.SetPropertyValue(btn, "Width", SIZE);
                        if (isPrime)
                        {
                            btn.Fill = Brushes.Red;
                            //ControlHandler.SetPropertyValue(btn, "Fill", Brushes.Red);
                        }
                        else
                        {
                            if (c % 2 == 0)
                            {
                                btn.Fill = Brushes.WhiteSmoke;
                                //ControlHandler.SetPropertyValue(btn, "Fill", Brushes.WhiteSmoke);
                            }
                        }
                        btn.Stroke = Brushes.White;
                        btn.StrokeThickness = 1.0;
                        Canvas.SetTop(btn, i * SIZE);
                        Canvas.SetLeft(btn, j * SIZE);
                        NumberRectangle.Children.Add(btn);
                        //ControlHandler.SetPropertyValue(btn, "Stroke", Brushes.White);
                        //ControlHandler.SetPropertyValue(btn, "StrokeThickness", 1.0);
                        //ControlHandler.ExecuteMethod(NumberRectangle, "SetTop", new object[] { btn, i * SIZE });
                        //ControlHandler.ExecuteMethod(NumberRectangle, "SetLeft", new object[] { btn, j * SIZE });
                        //ControlHandler.ExecuteMethod(childs, "Add", new object[] { btn });
                    }
                    //counter = counter.Add(PrimesBigInteger.ValueOf(1));
                    counter++;
                    c++;
                }
            }
            Cursor = Cursors.Arrow;

            //TimeSpan diff = DateTime.Now - start;
            //Debug.WriteLine(string.Format("Create: Std={0}, Min={1}, sek={2}, MS={3}", new object[] { diff.Hours, diff.Minutes, diff.Seconds, diff.Milliseconds }));
        }

        private void lblRow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Canvas.SetLeft(sender as TextBlock, (e.NewSize.Width + 5) * -1);
        }

        //private void Scroll(int factor)
        //{
        //  DateTime start = DateTime.Now;

        //  foreach (Rectangle nb in m_Buttons)
        //  {
        //    PrimesBigInteger c = nb.Tag as PrimesBigInteger;
        //    c = c.Add(PrimesBigInteger.ValueOf(this.Columns * factor));
        //    if (c.IsPrime(10))
        //    {
        //      nb.Fill = Brushes.Red;
        //    }
        //    else
        //    {
        //      nb.Fill = Brushes.White;
        //    }

        //    nb.Tag = c;
        //    (nb.ToolTip as ToolTip).Content = c.ToString();
        //  }
        //  TimeSpan diff = DateTime.Now - start;
        //  Debug.WriteLine(string.Format("Scroll: Std={0}, Min={1}, sek={2}, MS={3}", new object[] { diff.Hours, diff.Minutes, diff.Seconds, diff.Milliseconds }));
        //}

        private void Scroll(int factor)
        {
            //DateTime start = DateTime.Now;
            PrimesBigInteger sa = PrimesBigInteger.ValueOf(Columns * factor);
            m_start = PrimesBigInteger.Max(m_start.Add(sa), PrimesBigInteger.One);
            DrawButtons();
            //TimeSpan diff = DateTime.Now - start;
            //Debug.WriteLine(string.Format("Scroll: Std={0}, Min={1}, sek={2}, MS={3}", new object[] { diff.Hours, diff.Minutes, diff.Seconds, diff.Milliseconds }));
        }

        //private void Scroll(int factor)
        //{
        //  PrimesBigInteger sa = PrimesBigInteger.ValueOf(this.Columns * factor);
        //  foreach (Rectangle nb in m_Buttons)
        //  {
        //    PrimesBigInteger c = nb.Tag as PrimesBigInteger;
        //    c = c.Add(sa);
        //    if (c.IsPrime(10) && c.CompareTo(PrimesBigInteger.Zero) >= 0)
        //    {
        //      nb.Fill = Brushes.Red;
        //    }
        //    else
        //    {
        //      nb.Fill = Brushes.White;
        //    }

        //    nb.Tag = c;
        //    (nb.ToolTip as ToolTip).Content = c.ToString();
        //  }
        //}
        //
        //private void Scroll(int factor)
        //{
        //  PrimesBigInteger sa = PrimesBigInteger.ValueOf(this.Columns * factor);

        //  foreach (Rectangle nb in m_Buttons)
        //  {
        //    nb.BINumber = nb.BINumber.Add(sa);
        //    if (nb.BINumber.IsPrime(10) && nb.BINumber.CompareTo(PrimesBigInteger.Zero) >= 0)
        //    {
        //      nb.Background = Brushes.Red;
        //    }
        //    else
        //    {
        //      nb.Background = Brushes.White;
        //    }
        //  }
        //}

        private static long GCD(long a, long b)
        {
            long Remainder;

            while (b != 0)
            {
                Remainder = a % b;
                a = b;
                b = Remainder;
            }

            return a;
        }

        private bool isPrime(long n)
        {
            return PrimesBigInteger.ValueOf(n).IsPrime(10);
            //if (n == 2 || n == 3 || n == 5 || n == 7) return true;
            //if (n < 2) return false;
            //else if (n > 2 && n % 2 == 0) return false;
            //else if (n > 3 && n % 3 == 0) return false;
            //else if (n > 5 && n % 5 == 0) return false;
            //else if (n > 7 && n % 7 == 0) return false;

            //bool result = true;
            //Random r = new Random();
            //for (int i = 0; i < 10 && result; i++)
            //{
            //  long a = (r.Next(2,(int)(n%int.MaxValue)-1));
            //  result = GCD(a, n) == 1;
            //  if(!result)
            //    result = (Math.Pow(a, n - 1) % n == 1);
            //}
            //return result;
        }

        private void Draw()
        {
            //CancelDrawThread();
            //m_DrawThread = new Thread(new ThreadStart(DoDraw));
            //m_DrawThread.Start();

            DoDraw();
        }

        private void DoDraw()
        {
            Cursor = Cursors.Wait;
            if (OnDrawStart != null)
            {
                OnDrawStart();
            }
            //DrawGrid();
            DrawButtons();
            if (OnDrawStop != null)
            {
                OnDrawStop();
            }

            Cursor = Cursors.Arrow;
        }

        private void CancelDrawThread()
        {
            if (m_DrawThread != null)
            {
                if (OnDrawStop != null)
                {
                    OnDrawStop();
                }

                m_DrawThread.Abort();
                m_DrawThread = null;
            }
        }

        private void NumberRectangleControl_OnDrawStop()
        {
            SetScrollButtonsEnabled(true);
        }

        private void NumberRectangleControl_OnDrawStart()
        {
            SetScrollButtonsEnabled(false);
        }

        private void SetScrollButtonsEnabled(bool enabled)
        {
            ControlHandler.SetButtonEnabled(btnDown, enabled);
            ControlHandler.SetButtonEnabled(btnFastDown, enabled);
            ControlHandler.SetButtonEnabled(btnUp, enabled);
            ControlHandler.SetButtonEnabled(btnFastUp, enabled);
        }

        #endregion

        #region IPrimeDistribution Members

        public void Init()
        {
            DoDraw();
        }

        public void Dispose()
        {
            CancelDrawThread();
        }

        #endregion

        #region Recognizing User Input

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            PrimesBigInteger rows = iscHeight.GetValue();
            PrimesBigInteger cols = iscWidth.GetValue();
            if (rows != null && cols != null)
            {
                Rows = rows.IntValue;
                Columns = cols.IntValue;
                Draw();
            }
        }

        //void iscWidth_Execute(PrimesBigInteger value)
        //{
        //  Columns = value.IntValue;
        //  PrimesBigInteger rows = iscHeight.GetValue();
        //  if (rows != null) this.Rows = rows.IntValue;
        //  Draw();
        //}

        //void iscHeight_Execute(PrimesBigInteger value)
        //{
        //  Rows = value.IntValue;
        //  PrimesBigInteger cols = iscWidth.GetValue();
        //  if (cols != null) this.Columns = cols.IntValue;
        //  Draw();
        //}

        private void isc_Execute(PrimesBigInteger value)
        {
            Execute();
        }

        private void Execute()
        {
            PrimesBigInteger cols = iscWidth.GetValue();
            PrimesBigInteger rows = iscHeight.GetValue();
            PrimesBigInteger start = iscStart.GetValue();
            if (rows != null && cols != null && start != null)
            {
                Rows = rows.IntValue;
                Columns = cols.IntValue;
                m_start = start;
                Draw();
            }
        }

        #endregion

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnScroll_Click(object sender, RoutedEventArgs e)
        {
            int factor = 0;
            if (sender == btnUp)
            {
                factor = -1;
            }
            else if (sender == btnFastUp)
            {
                factor = -2;
            }
            else if (sender == btnDown)
            {
                factor = 1;
            }
            else if (sender == btnFastDown)
            {
                factor = 2;
            }

            Scroll(factor);
        }

        private void DesignerItem_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int width = (int)(e.NewSize.Width / SIZE);
            int height = (int)(e.NewSize.Height / SIZE);
            iscHeight.SetText(InputSingleControl.Free, height.ToString());
            iscWidth.SetText(InputSingleControl.Free, width.ToString());
            double sliderValue = silderScale.Value;
            if (e.NewSize.Width * sliderValue > ActualWidth)
            {
                silderScale.Value = ActualWidth / (e.NewSize.Width * sliderValue);
            }
            if (e.NewSize.Height * sliderValue > ActualHeight)
            {
                silderScale.Value = ActualHeight / (e.NewSize.Height * sliderValue);
            }
        }

        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {
            Execute();
        }

        private void ResizeThumb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int width = (int)(DesignerItem.ActualWidth / SIZE);
            int height = (int)(DesignerItem.ActualHeight / SIZE);
            iscHeight.SetText(InputSingleControl.Free, height.ToString());
            iscWidth.SetText(InputSingleControl.Free, width.ToString());
            Execute();
        }

        private void ResizeThumb_LeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NumberRectangle.Children.Clear();
        }

        private void NumberRectangle_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(NumberRectangle);
            int row = (int)Math.Floor(p.Y / SIZE);
            int col = (int)Math.Floor(p.X / SIZE);
            lblActualNumber.Text = m_start.Add(PrimesBigInteger.ValueOf(Columns * row)).Add(PrimesBigInteger.ValueOf(col)).ToString("D");
            lblActualNumberInfo.Visibility = Visibility.Visible;
        }

        private void NumberRectangle_MouseLeave(object sender, MouseEventArgs e)
        {
            lblActualNumberInfo.Visibility = Visibility.Hidden;
        }

        private void NumberRectangle_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        private void DesignerItem_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
        }
    }
}
