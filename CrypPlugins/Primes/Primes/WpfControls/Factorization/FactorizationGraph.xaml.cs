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
using Primes.Library.FactorTree;
using Primes.WpfControls.ShapeManagement.Ellipse;
using Primes.WpfControls.Validation.Validator;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Primes.WpfControls.Factorization
{
    /// <summary>
    /// Interaction logic for FactorizationGraph.xaml
    /// </summary>
    public delegate void UpdatedFactorTree(GmpFactorTree factortree);
    public delegate void FactorTreeDelegate();

    public partial class FactorizationGraph : UserControl, IFactorizer
    {
        internal delegate void OnReset();

        private readonly GmpFactorTree m_FactorTree;
        private readonly EllipseManager m_Manager;
        private bool m_Running = false;
        private const int MAXSCALE = 10;
        private const int MINSCALE = 1;
        private DateTime m_Start;
        private TimeSpan m_Needs;

        private readonly IDictionary<Ellipse, EllipseItem> m_MapEllipseItems;

        public FactorizationGraph()
        {
            InitializeComponent();
            m_FactorTree = new GmpFactorTree();
            m_Manager = new EllipseManager
            {
                Height = ActualHeight,
                Width = ActualWidth
            };
            m_FactorTree.OnFactorFound += FactorFound;
            m_FactorTree.OnStart += OnFactorizationStart;
            m_FactorTree.OnStop += OnFactorizationStop;
            m_FactorTree.OnCancel += OnFactorizationCancel;
            m_FactorTree.OnActualDivisorChanged += new GmpBigIntegerParameterDelegate(m_FactorTree_OnActualDivisorChanged);
            //this.PaintArea.MouseWheel += new MouseWheelEventHandler(PaintArea_MouseWheel);
            m_MapEllipseItems = new Dictionary<Ellipse, EllipseItem>();
            items = new List<object>();
        }

        public bool isRunning => m_FactorTree.isRunning;

        private void m_FactorTree_OnActualDivisorChanged(PrimesBigInteger value)
        {
            ControlHandler.SetPropertyValue(lblActualDivisor, "Text", Primes.Resources.lang.WpfControls.Factorization.Factorization.bf_actualdiv + value.ToString("D"));
        }

        private readonly object test = new object();

        public void Execute(PrimesBigInteger from, PrimesBigInteger to)
        {
        }

        public void Execute(PrimesBigInteger value)
        {
            m_Start = DateTime.Now;
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new OnReset(Reset));
            try
            {
                m_FactorTree.Factorize(value);
            }
            catch (Library.FactorTree.Exceptions.TrialDivisionException)
            {
                MessageBox.Show("Nach 100.000 Versuchen wurde kein Faktor gefunden, das Verfahren wird abgebrochen");
            }
        }

        public void CancelExecute()
        {
            CancelFactorization();
        }

        public TimeSpan Needs => m_Needs;

        public void CancelFactorization()
        {
            m_Needs = DateTime.Now - m_Start;
            ControlHandler.SetPropertyValue(lblActualDivisor, "Text", "");
            m_FactorTree.CancelFactorize();

            if (Cancel != null)
            {
                Cancel();
            }
        }

        public void Reset()
        {
            items.Clear();
            m_MapEllipseItems.Clear();
            m_FactorTree.Clear();
            m_Manager.Clear();
            UIElementCollection childs = ControlHandler.GetPropertyValue(PaintArea, "Children") as UIElementCollection;
            ControlHandler.ExecuteMethod(childs, "Clear");
            ControlHandler.SetPropertyValue(PaintArea, "Height", 0);
            ControlHandler.SetPropertyValue(PaintArea, "Width", 0);
            ControlHandler.SetPropertyValue(lblActualDivisor, "Text", "");
            //sv.ScrollToRightEnd();
            //sv.ScrollToBottom();
        }

        private void FactorFound()
        {
            //this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new OnReset(Reset));
            if (FoundFactor != null)
            {
                FoundFactor(m_FactorTree);
            }
            m_Manager.FactorTree = m_FactorTree;
            foreach (EllipseItem item in m_Manager.EllipseItems.Values)
            {
                AddEllipseItem(item);
            }
            //foreach (EllipseConnector conn in m_Manager.Connectors)
            //{
            //  AddConnector(conn);
            //}
        }

        private delegate void AddConnectorDelegate(EllipseConnector conn);

        private void AddConnector(EllipseConnector conn)
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new AddConnectorDelegate(InvokeAddConnector), conn);
        }

        private void InvokeAddConnector(EllipseConnector conn)
        {
            PaintArea.Children.Add(CreateConnector(conn));
        }

        private Line CreateConnector(EllipseConnector conn)
        {

            Line result = new Line
            {
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                X1 = conn.X1,
                Y1 = conn.Y1,
                X2 = conn.X2,
                Y2 = conn.Y2
            };
            return result;
        }

        internal delegate void AddEllipsDelegate(EllipseItem item);

        private void AddEllipseItem(EllipseItem item)
        {
            Dispatcher.Invoke(
              System.Windows.Threading.DispatcherPriority.Send,
              new AddEllipsDelegate(InvokeAddEllipseItem),
              item);
        }

        private readonly IList<object> items;
        private readonly object itemslistlockobject = new object();

        private void InvokeAddEllipseItem(EllipseItem item)
        {
            if (!items.Contains(item))
            {
                ToolTip tt = new ToolTip
                {
                    Content = StringFormat.FormatString(item.Value.ToString(), 80)
                };

                items.Add(item);

                Ellipse result = new Ellipse
                {
                    ToolTip = tt,
                    Width = item.Width,
                    Height = item.Height,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Fill = Brushes.LightCyan
                };
                if (item.IsPrime)
                {
                    result.Fill = Brushes.LightGreen;
                }

                Canvas.SetLeft(result, item.X);
                Canvas.SetTop(result, item.Y);
                if (item.Y > PaintArea.Height - item.Height)
                {
                    PaintArea.Height = item.Y + m_Manager.ItemSize * 2;
                }
                if (item.X > PaintArea.Width - item.Width)
                {
                    PaintArea.Width = item.X + m_Manager.ItemSize * 10;
                }
                m_MapEllipseItems.Add(result, item);
                PaintArea.Children.Add(result);

                Label l = new Label();
                string lContent = item.Value.ToString();
                if (lContent.Length > 7)
                {
                    lContent = lContent.Substring(0, 6);
                    lContent += "...";
                }
                l.Content = lContent;
                l.ToolTip = tt;
                Canvas.SetLeft(l, item.X + item.Width);
                Canvas.SetTop(l, item.Y);
                PaintArea.Children.Add(l);

                if (item.Parent != null)
                {
                    AddConnector(new EllipseConnector(item.Parent, item));
                }
            }
        }

        public void OnFactorizationStart()
        {
            m_Running = true;
            ControlHandler.SetCursor(this, Cursors.Wait);
            if (Start != null)
            {
                Start();
            }
        }

        public void OnFactorizationStop()
        {
            m_Running = false;
            items.Clear();
            m_MapEllipseItems.Clear();
            m_Manager.Clear();
            m_Needs = DateTime.Now - m_Start;
            ControlHandler.SetPropertyValue(lblActualDivisor, "Text", "");

            ControlHandler.SetCursor(this, Cursors.Arrow);

            if (Stop != null)
            {
                Stop();
            }
        }

        public void OnFactorizationCancel()
        {
            m_Running = false;
            m_Needs = DateTime.Now - m_Start;
            ControlHandler.SetCursor(this, Cursors.Arrow);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (m_Running)
            {
                Cursor = Cursors.Wait;
            }
            else
            {
                Cursor = Cursors.Arrow;
            }
        }

        #region IFactorizer Members

        public event FoundFactor FoundFactor;

        public event VoidDelegate Start;

        public event VoidDelegate Stop;

        public event VoidDelegate Cancel;

        public event GmpBigIntegerParameterDelegate OnActualDivisorChange;

        private void FireOnActualDivisorChange()
        {
            OnActualDivisorChange(null);
        }

        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        #region IPrimeVisualization Members

        public event CallbackDelegateGetInteger ForceGetInteger;

        private void FireForceGetInteger()
        {
            ForceGetInteger(null);
        }

        #endregion

        #region IPrimeVisualization Members

        public event CallbackDelegateGetInteger ForceGetIntegerInterval;

        private void FireForceGetIntegerInterval()
        {
            ForceGetIntegerInterval(null);
        }

        #endregion

        #region IFactorizer Members

        public Primes.WpfControls.Validation.IValidator<PrimesBigInteger> Validator => new BigIntegerMinValueValidator(null, PrimesBigInteger.Three);

        #endregion
    }
}
