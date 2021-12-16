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
using Primes.Resources.lang.Numbertheory;
using Primes.WpfControls.Components;
using Primes.WpfControls.Components.Arrows;
using Primes.WpfControls.Validation;
using Primes.WpfControls.Validation.Validator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Primes.WpfControls.NumberTheory.PowerMod
{
    /// <summary>
    /// Power Mod tutorial which iterates the exponent.
    /// </summary>
    public partial class PowerModControl : UserControl
    {
        #region initializing

        private Thread m_Thread;
        private readonly bool m_Initialized;
        private bool m_Resume = false;

        private const double Margin = 25.0;
        private const double PointRadius = 3.0;

        public struct IterationLogEntry
        {
            public int Iteration { get; set; }
            public string Formula { get; set; }
            public string LogEntry { get; set; }
        }

        private readonly ObservableCollection<IterationLogEntry> iterationLogEntries = new ObservableCollection<IterationLogEntry>();

        public PowerModControl()
        {
            InitializeComponent();
            initBindings();
            iterationLog.ItemsSource = iterationLogEntries;
            ConfigureIntegerInputs();
            m_Points = new Dictionary<int, Point>();
            m_SourceDestination = new List<Pair<Ellipse, Ellipse>>();
            m_CirclesSource = new Dictionary<Ellipse, Polyline>();
            m_ArrowsWithSourceAndDestination = new Dictionary<Pair<Ellipse, Ellipse>, ArrowLine>();
            m_ArrowsMark = new Dictionary<PrimesBigInteger, ArrowLine>();
            m_CirclesMark = new Dictionary<PrimesBigInteger, Polyline>();
            m_RunningLockObject = new object();
            m_Initialized = true;
            m_StepWiseEvent = new ManualResetEvent(false);
        }

        private void LogEntry_MouseMove(object sender, MouseEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is IterationLogEntry log)
            {
                MarkPath(log.Iteration - 1);
            }
        }

        private void CopyLogSelection_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu contextMenu = (sender as MenuItem)?.Parent as ContextMenu;
            if (contextMenu?.DataContext is IterationLogEntry)
            {
                IterationLogEntry logEntry = (IterationLogEntry)contextMenu.DataContext;
                try
                {
                    Clipboard.SetText($"{logEntry.LogEntry}\n", TextDataFormat.Text);
                }
                catch { /* Ignore */}
            }
        }

        private void CopyLogAll_Click(object sender, RoutedEventArgs e)
        {
            string log = string.Join("\n", iterationLogEntries.Select(entry => entry.LogEntry));
            try
            {
                Clipboard.SetText($"{log}\n", TextDataFormat.Text);
            }
            catch { /* Ignore */}
        }

        public void MarkPath(PrimesBigInteger iteration)
        {
            int normalizedIteration = GetNormalizedIterationIndex(iteration.IntValue);

            foreach (ArrowLine currentArrow in m_ArrowsWithSourceAndDestination.Values)
            {
                currentArrow.Stroke = Brushes.LightGray;
            }

            for (int i = IterationStart.IntValue; i < normalizedIteration; i++)
            {
                if (m_ArrowsMark.TryGetValue(i, out ArrowLine iterationArrowLine))
                {
                    iterationArrowLine.Stroke = Brushes.Black;
                }
            }

            if (m_ArrowsMark.TryGetValue(normalizedIteration, out ArrowLine targetArrowLine))
            {
                targetArrowLine.Stroke = Brushes.Red;
            }

            foreach (Polyline currentCircle in m_CirclesSource.Values)
            {
                currentCircle.Stroke = Brushes.LightGray;
            }

            for (int i = IterationStart.IntValue; i < normalizedIteration; i++)
            {
                if (m_CirclesMark.TryGetValue(i, out Polyline iterationCircle))
                {
                    iterationCircle.Stroke = Brushes.Black;
                }
            }

            if (m_CirclesMark.TryGetValue(normalizedIteration, out Polyline targetCircle))
            {
                targetCircle.Stroke = Brushes.Red;
            }
        }

        /// <summary>
        /// Transforms an interation index into the smallest equivalent index when resolving the cycle.
        /// The returned index is meant to be used for visualization purposes, 
        /// in order to show the iteration part until the "normalized" iteration index.
        /// </summary>
        protected virtual int GetNormalizedIterationIndex(int index)
        {
            if (index < 1)
            {
                return index;
            }
            (int cycleStartIndex, int cycleEndIndex, HashSet<int> _) = FindCycle();
            int cycleLength = cycleEndIndex - cycleStartIndex;
            int normalizedIndex = (index - cycleStartIndex) % cycleLength + cycleStartIndex;
            if (normalizedIndex == 0)
            {
                //Avoid iteration 0, because there is no arrow for it:
                normalizedIndex += cycleLength;
            }
            if (cycleLength == 1 && normalizedIndex >= cycleStartIndex && normalizedIndex < index)
            {
                //Special case: If cycle length is 1 and index is within the cycle, take the next index for visualisation purposes.
                normalizedIndex++;
            }
            return normalizedIndex;
        }

        protected virtual InputSingleControl ActiveExpControl => iscMaxExp;

        protected virtual InputSingleControl ActiveBaseControl => iscBase;

        protected virtual PrimesBigInteger DefaultExp => 28;
        protected virtual PrimesBigInteger DefaultBase => 2;
        protected virtual PrimesBigInteger DefaultMod => 13;

        private void ConfigureIntegerInputs()
        {
            InputSingleControl baseControl = ActiveBaseControl;
            baseControl.Visibility = Visibility.Visible;
            baseControl.Execute += new ExecuteSingleDelegate(iscBase_Execute);
            baseControl.KeyDown += new ExecuteSingleDelegate(iscBase_KeyDown);
            baseControl.SetText(InputSingleControl.Free, DefaultBase.ToString());
            InputValidator<PrimesBigInteger> ivBase = new InputValidator<PrimesBigInteger>
            {
                Validator = new BigIntegerMinValueValidator(null, PrimesBigInteger.Two)
            };
            baseControl.AddInputValidator(InputSingleControl.Free, ivBase);

            InputSingleControl expControl = ActiveExpControl;
            expControl.Visibility = Visibility.Visible;
            expControl.Execute += new ExecuteSingleDelegate(iscExp_Execute);
            expControl.KeyDown += new ExecuteSingleDelegate(iscExp_KeyDown);
            expControl.SetText(InputSingleControl.Free, DefaultExp.ToString());
            InputValidator<PrimesBigInteger> ivExp = new InputValidator<PrimesBigInteger>
            {
                Validator = new BigIntegerMinValueValidator(null, PrimesBigInteger.One)
            };
            expControl.AddInputValidator(InputSingleControl.Free, ivExp);

            iscMod.Execute += new ExecuteSingleDelegate(iscMod_Execute);
            iscMod.KeyDown += new ExecuteSingleDelegate(iscMod_KeyDown);
            iscMod.SetText(InputSingleControl.Free, DefaultMod.ToString());
            InputValidator<PrimesBigInteger> ivMod = new InputValidator<PrimesBigInteger>
            {
                Validator = new BigIntegerMinValueMaxValueValidator(null, PrimesBigInteger.Two, PrimesBigInteger.ValueOf(150))
            };
            iscMod.AddInputValidator(InputSingleControl.Free, ivMod);

            Exp = DefaultExp;
            Base = DefaultBase;
            Mod = DefaultMod;

            Start += new VoidDelegate(PowerModControl_Start);
            Stop += new VoidDelegate(PowerModControl_Stop);
            Cancel += new VoidDelegate(PowerModControl_Cancel);
        }

        private void iscMod_KeyDown(PrimesBigInteger value)
        {
            if (value != null)
            {
                Mod = value;
            }
        }

        private void iscMod_Execute(PrimesBigInteger value)
        {
            Mod = value;
            StartThread();
        }

        private void slidermodulus_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Mod = PrimesBigInteger.ValueOf((long)e.NewValue);
        }

        private void iscExp_KeyDown(PrimesBigInteger value)
        {
            if (value != null)
            {
                Exp = value;
            }
        }

        private void iscExp_Execute(PrimesBigInteger value)
        {
            Exp = value;
            StartThread();
        }

        private void iscBase_KeyDown(PrimesBigInteger value)
        {
            if (value != null)
            {
                Base = value;
            }
        }

        private void iscBase_Execute(PrimesBigInteger value)
        {
            Base = value;
            StartThread();
        }

        #endregion

        #region Properties

        private readonly object m_RunningLockObject;
        private bool m_Running;
        private readonly IDictionary<int, Point> m_Points;
        private readonly IList<Pair<Ellipse, Ellipse>> m_SourceDestination;
        private readonly IDictionary<Ellipse, Polyline> m_CirclesSource;
        private readonly IDictionary<PrimesBigInteger, Polyline> m_CirclesMark;
        private readonly IDictionary<Pair<Ellipse, Ellipse>, ArrowLine> m_ArrowsWithSourceAndDestination;
        private readonly IDictionary<PrimesBigInteger, ArrowLine> m_ArrowsMark;
        private double offset = 0;

        private PrimesBigInteger m_Base;

        public PrimesBigInteger Base
        {
            get => m_Base;
            set
            {
                if (m_Base != value)
                {
                    m_Base = value;
                    ResetProcessView();
                }
            }
        }

        private PrimesBigInteger m_Exp;

        public PrimesBigInteger Exp
        {
            get => m_Exp;
            set
            {
                if (m_Exp != value)
                {
                    m_Exp = value;
                    ResetProcessView();
                }
            }
        }

        private PrimesBigInteger m_Mod;

        public PrimesBigInteger Mod
        {
            get => m_Mod;
            set
            {
                m_Mod = value;
                if (m_Mod != null)
                {
                    if (m_Mod.Equals(iscMod.GetValue()))
                    {
                        slidermodulus.Value = m_Mod.DoubleValue;
                    }
                    else
                    {
                        iscMod.SetText(InputSingleControl.Free, m_Mod.ToString());
                    }
                    ResetProcessView();
                    if (m_Initialized)
                    {
                        Reset();
                        CreatePoints();
                    }
                }
            }
        }

        #endregion

        #region painting the points

        private void Reset()
        {
            m_Points.Clear();
            m_SourceDestination.Clear();
            m_CirclesSource.Clear();
            m_ArrowsWithSourceAndDestination.Clear();
            m_ArrowsMark.Clear();
            m_CirclesMark.Clear();
            ControlHandler.ClearChildren(PaintArea);
            ControlHandler.ClearChildren(ArrowArea);
            ControlHandler.ClearChildren(LabelArea);
        }

        public void GetCoords(int value, out double x, out double y)
        {
            double angle = 2 * Math.PI * (value / (double)m_Mod.IntValue + offset / 360.0) * (m_SortAsc ? 1 : -1);
            x = (Math.Sin(angle) + 1) * Radius + Margin;
            y = (Math.Cos(angle) + 1) * Radius + Margin;
        }

        private void CreatePoint(int value)
        {
            GetCoords(value, out double x, out double y);
            y -= PointRadius;
            x -= PointRadius;

            Ellipse point = m_Points.ContainsKey(value) ? GetEllipseAt(m_Points[value]) : null;

            if (point == null)
            {
                point = ControlHandler.CreateObject(typeof(Ellipse)) as Ellipse;
                point.ToolTip = new ToolTip();
                (point.ToolTip as ToolTip).Content = value.ToString();
                ControlHandler.SetPropertyValue(point, "Width", PointRadius * 2);
                ControlHandler.SetPropertyValue(point, "Height", PointRadius * 2);
                ControlHandler.SetPropertyValue(point, "Fill", Brushes.Black);
                ControlHandler.SetPropertyValue(point, "Stroke", Brushes.Black);
                ControlHandler.SetPropertyValue(point, "StrokeThickness", 1.0);
                ControlHandler.AddChild(point, PaintArea);
            }

            ControlHandler.ExecuteMethod(PaintArea, "SetLeft", new object[] { point, x });
            ControlHandler.ExecuteMethod(PaintArea, "SetTop", new object[] { point, y });

            if (m_CirclesSource.ContainsKey(point))
            {
                MoveCircle(point);
            }

            if (m_Points.ContainsKey(value))
            {
                m_Points[value] = new Point(x, y);
            }
            else
            {
                m_Points.Add(value, new Point(x, y));
            }

            Label lbl = ControlHandler.CreateLabel(value.ToString(), null);
            double _left = x - 5;
            double _top = y;
            _left += (x < Radius) ? -7 + (-2 * (int)Math.Log(value)) : 2;
            _top += (y < Radius) ? -20 : 0;
            ControlHandler.ExecuteMethod(PaintArea, "SetLeft", new object[] { lbl, _left });
            ControlHandler.ExecuteMethod(PaintArea, "SetTop", new object[] { lbl, _top });
            ControlHandler.AddChild(lbl, LabelArea);
        }

        #endregion

        #region Painting the Ellipse

        private void Paint()
        {
            Ellipse el = ControlHandler.CreateObject(typeof(Ellipse)) as Ellipse;
            ControlHandler.SetPropertyValue(el, "Width", Aperture);
            ControlHandler.SetPropertyValue(el, "Height", Aperture);
            ControlHandler.ExecuteMethod(CircleArea, "SetTop", new object[] { el, Margin });
            ControlHandler.ExecuteMethod(CircleArea, "SetLeft", new object[] { el, Margin });
            ControlHandler.SetPropertyValue(el, "StrokeThickness", 1.0);
            ControlHandler.SetPropertyValue(el, "Stroke", Brushes.Black);
            ControlHandler.SetPropertyValue(el, "Name", "dontremove");
            ControlHandler.AddChild(el, CircleArea);
        }

        private double Aperture => Math.Max(Math.Min(PaintArea.Width, PaintArea.Height) - 2 * Margin, 0);

        private double _radius = -1;
        private double Radius
        {
            get => Aperture / 2.0;
            set => _radius = value;
        }

        private double Perimeter => (2 * Math.PI * Aperture);

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            try
            {
                if (m_Initialized)
                {
                    //ClearArrows();
                    iterationLog.RenderSize = sizeInfo.NewSize;
                    ArrowArea.Width = PaintArea.Width = CircleArea.Width = LabelArea.Width = ContentArea.Width = Math.Max(0, PaintPanel.ActualWidth - 20);
                    ArrowArea.Height = PaintArea.Height = CircleArea.Height = ContentArea.Height = LabelArea.Height = Math.Max(0, PaintPanel.ActualHeight - spslider.ActualHeight - 30);
                    //ContentArea.Width = PaintPanel.ActualWidth;

                    //ContentArea.Height = PaintPanel.ActualHeight - spslider.ActualHeight;

                    //if (log.ActualWidth < 100)
                    //{
                    //  log.Width = this.ActualWidth - 5;
                    //}
                    if (sizeInfo.NewSize.Height != double.NaN && sizeInfo.NewSize.Height > 0 &&
                      sizeInfo.NewSize.Width != double.NaN && sizeInfo.NewSize.Width > 0)
                    {
                        if (!m_Running)
                        {
                            if (CircleArea.Children.Count != 0)
                            {
                                CircleArea.Children.Clear();
                            }

                            Paint();
                            if (m_Mod != null)
                            {
                                CreatePoints();
                                foreach (Ellipse e in m_CirclesSource.Keys)
                                {
                                    MoveCircle(e);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void ClearPoints()
        {
            UIElementCollection childs = ControlHandler.GetPropertyValue(PaintArea, "Children") as UIElementCollection;
            ControlHandler.ExecuteMethod(childs, "Clear");
        }

        private void ClearLabels()
        {
            UIElementCollection childs = ControlHandler.GetPropertyValue(LabelArea, "Children") as UIElementCollection;
            ControlHandler.ExecuteMethod(childs, "Clear");
        }

        private void ClearArrows()
        {
            UIElementCollection childs = ControlHandler.GetPropertyValue(ArrowArea, "Children") as UIElementCollection;
            ControlHandler.ExecuteMethod(childs, "Clear");
            //m_Arrows.Clear();
            //m_Circles.Clear();
            m_ArrowsWithSourceAndDestination.Clear();
            m_CirclesSource.Clear();
            m_ArrowsMark.Clear();
            m_CirclesMark.Clear();
        }

        #endregion

        #region Events

        private event VoidDelegate Start;

        private void FireStartEvent()
        {
            if (Start != null)
            {
                Start();
            }
        }

        private event VoidDelegate Stop;

        private void FireStopEvent()
        {
            if (Stop != null)
            {
                Stop();
            }
        }

        private event VoidDelegate Cancel;

        private void FireCancelEvent()
        {
            if (Cancel != null)
            {
                Cancel();
            }
        }

        private void PowerModControl_Cancel()
        {
            CancelThread();
            SetupStop();
        }

        private void PowerModControl_Stop()
        {
            SetupStop();
        }

        private void PowerModControl_Start()
        {
            SetupStart();
        }

        #endregion

        #region SettingUpButtons

        private void SetupStart()
        {
            ControlHandler.ExecuteMethod(this, "_SetupStart");
        }

        public void _SetupStart()
        {
            btnCancel.IsEnabled = true;
            iscBase.LockControls();
            iscMaxBase.LockControls();
            iscExp.LockControls();
            iscMaxExp.LockControls();
            iscMod.LockControls();
            slidermodulus.IsEnabled = false;
        }

        private void SetupStop()
        {
            ControlHandler.ExecuteMethod(this, "_SetupStop");
        }

        public void _SetupStop()
        {
            btnCancel.IsEnabled = false;
            btnNextStep.IsEnabled = true;
            btnResumeAutomatic.IsEnabled = true;
            m_Resume = false;
            iscBase.UnLockControls();
            iscMaxBase.UnLockControls();
            iscExp.UnLockControls();
            iscMaxExp.UnLockControls();
            iscMod.UnLockControls();
            slidermodulus.IsEnabled = true;
        }

        #endregion

        #region Execute

        private void CancelThread()
        {
            if (m_Thread != null)
            {
                lock (m_RunningLockObject)
                {
                    m_Running = false;
                }

                m_Thread.Abort();
                m_Thread = null;
            }
        }

        private void StartThread()
        {
            m_Base = ActiveBaseControl.GetValue();
            m_Exp = ActiveExpControl.GetValue();
            m_Mod = iscMod.GetValue();
            if (m_Base != null && m_Exp != null && m_Mod != null)
            {
                m_Thread = new Thread(new ThreadStart(DoExecuteGraphic))
                {
                    CurrentCulture = Thread.CurrentThread.CurrentCulture,
                    CurrentUICulture = Thread.CurrentThread.CurrentUICulture
                };
                m_Thread.Start();
                //rteDoExecuteGraphic();
            }
        }

        private bool m_SortAsc = true;

        private void CreatePoints()
        {
            //ClearArrows();
            //ClearPoints();
            ClearLabels();
            for (int i = 0; i < m_Mod.IntValue; i++)
            {
                CreatePoint(i);
            }
        }

        private readonly ManualResetEvent m_StepWiseEvent;

        private void WaitStepWise()
        {
            if (m_StepWiseEvent != null && !m_Resume)
            {
                m_StepWiseEvent.Reset();
                m_StepWiseEvent.WaitOne();
            }
        }

        private (int cycleStartIndex, int cycleEndIndex, HashSet<int> allValues) FindCycle()
        {
            PrimesBigInteger value = Base.Mod(Mod);
            Dictionary<int, int> previousValues = new Dictionary<int, int>();
            int counter = 0;
            do
            {
                previousValues.Add(value.IntValue, counter);
                value = (value * Base).Mod(Mod);
                counter++;
                Debug.Assert(counter < Mod);
            } while (!previousValues.ContainsKey(value.IntValue));

            return (previousValues[value.IntValue], counter, previousValues.Keys.ToHashSet());
        }

        public virtual void SetCycleInfo()
        {
            (int cycleStartIndex, int cycleEndIndex, HashSet<int> allValues) = FindCycle();

            int cycleLength = cycleEndIndex - cycleStartIndex;
            CycleInfo1.Text = string.Format(Numbertheory.powermod_cycle_length, cycleLength);
            CycleInfo2.Text = string.Empty;
            CycleInfo2.ToolTip = null;

            //Get all values which are not contained in the iteration:
            List<int> missedValues = Enumerable.Range(1, Mod.IntValue - 1)
                .Where(val => !allValues.Contains(val)).ToList();

            if (missedValues.Any())
            {
                CycleInfo1.ToolTip = string.Format(Numbertheory.powermod_missing_values, string.Join(", ", missedValues));
            }
            else
            {
                CycleInfo1.ToolTip = Numbertheory.powermod_no_missing_values;
            }
        }

        public virtual void SetFormula()
        {
            Formula.Formula = $"{Base}^i \\text{{ mod }} {Mod} \\text{{          }} i = 1,\\ldots,{Exp}";
        }

        protected virtual PrimesBigInteger MaxIteration => Exp;

        protected virtual PrimesBigInteger IterationStart => 1;

        public void AddIterationLogEntry(int iteration, string formula, string logEntry)
        {
            ControlHandler.ExecuteMethod(this, nameof(_AddIterationLogEntry), new object[] {
                new IterationLogEntry
                {
                    Iteration = iteration,
                    Formula = formula,
                    LogEntry = logEntry
                }
            });
        }

        public void _AddIterationLogEntry(IterationLogEntry iterationLogEntry)
        {
            iterationLogEntries.Add(iterationLogEntry);
            iterationLogScroll.ScrollToBottom();
        }

        public void _ClearIterationLogEntries()
        {
            iterationLogEntries.Clear();
        }

        protected virtual PrimesBigInteger DoIterationStep(PrimesBigInteger lastResult, PrimesBigInteger iteration)
        {
            PrimesBigInteger result;
            if (lastResult == null)
            {
                result = Base.Mod(Mod);
            }
            else
            {
                result = (lastResult * Base).Mod(Mod);
            }

            AddIterationLogEntry(iteration.IntValue,
                $"{Base}^{{{iteration}}} \\text{{ mod }} {Mod} = {result}",
                string.Format(Numbertheory.powermod_execution, iteration, Base, iteration, Mod, result));

            return result;
        }

        private void ResetProcessView()
        {
            if (m_Initialized)
            {
                ClearArrows();
                m_SourceDestination.Clear();
                ControlHandler.ExecuteMethod(this, nameof(_ClearIterationLogEntries), new object[0]);
                ResetPointEllipseColor();

                ControlHandler.SetPropertyValue(CycleInfo1, "Text", string.Empty);
                ControlHandler.SetPropertyValue(CycleInfo1, nameof(CycleInfo1.ToolTip), null);
                ControlHandler.SetPropertyValue(CycleInfo2, "Text", string.Empty);
                ControlHandler.SetPropertyValue(CycleInfo2, nameof(CycleInfo2.ToolTip), null);
                ControlHandler.SetPropertyValue(Formula, "Formula", string.Empty);
            }
        }

        private void DoExecuteGraphic()
        {
            ResetProcessView();
            FireStartEvent();

            lock (m_RunningLockObject)
            {
                m_Running = true;
            }

            Point lastPoint = new Point(-1, -1);
            Ellipse lastEllipse = null;
            PrimesBigInteger result = null;

            ControlHandler.ExecuteMethod(this, nameof(SetCycleInfo), new object[0]);
            ControlHandler.ExecuteMethod(this, nameof(SetFormula), new object[0]);

            PrimesBigInteger i = IterationStart;
            while (i <= MaxIteration)
            {
                Thread.Sleep(100);

                result = DoIterationStep(result, i);

                if (lastPoint.X == -1 && lastPoint.Y == -1)
                {
                    lastPoint = m_Points[result.IntValue];
                    //First point, mark ellipse red:
                    Ellipse e = GetEllipseAt(lastPoint);
                    if (e != null)
                    {
                        ControlHandler.SetPropertyValue(e, "Fill", Brushes.Red);
                        ControlHandler.SetPropertyValue(e, "Stroke", Brushes.Red);
                    }
                    lastEllipse = e;
                }
                else
                {
                    Ellipse e = GetEllipseAt(m_Points[result.IntValue]);
                    Point newPoint = m_Points[result.IntValue];
                    CreateArrow(i - 1, lastEllipse, e);
                    lastPoint = newPoint;
                    lastEllipse = e;
                }

                i += 1;
                if (i > IterationStart + 1)
                {
                    WaitStepWise();
                }
            }

            lock (m_RunningLockObject)
            {
                m_Running = false;
            }

            FireStopEvent();
        }

        private void ResetPointEllipseColor()
        {
            foreach (Point point in m_Points.Values)
            {
                Ellipse e = GetEllipseAt(point);
                if (e != null)
                {
                    ControlHandler.SetPropertyValue(e, "Fill", Brushes.Black);
                    ControlHandler.SetPropertyValue(e, "Stroke", Brushes.Black);
                }
            }
        }

        private Ellipse GetEllipseAt(Point p)
        {
            return (Ellipse)ControlHandler.ExecuteMethod(this, "_GetEllipseAt", new object[] { p });
        }

        public Ellipse _GetEllipseAt(Point p)
        {
            foreach (UIElement e in PaintArea.Children)
            {
                if (e.GetType() == typeof(Ellipse))
                {
                    if (Canvas.GetTop(e) == p.Y && Canvas.GetLeft(e) == p.X)
                    {
                        return e as Ellipse;
                    }
                }
            }

            return null;
        }

        private void CreateArrow(PrimesBigInteger counter, Ellipse from, Ellipse to)
        {
            if (from == to)
            {
                AddCircle(counter, from);
            }
            else
            {
                ArrowLine l = ControlHandler.CreateObject(typeof(ArrowLine)) as ArrowLine;
                ControlHandler.SetPropertyValue(l, "Stroke", Brushes.Black);
                ControlHandler.SetPropertyValue(l, "StrokeThickness", 1.5);
                ArrowPositionConverter arrowPositionConverter = new ArrowPositionConverter(PointRadius);
                ControlHandler.ExecuteMethod(l, "SetBinding", new object[] { ArrowLine.X1Property, new Binding("(Canvas.Left)") { Source = from, Converter = arrowPositionConverter }, new Type[] { typeof(DependencyProperty), typeof(BindingBase) } });
                ControlHandler.ExecuteMethod(l, "SetBinding", new object[] { ArrowLine.Y1Property, new Binding("(Canvas.Top)") { Source = from, Converter = arrowPositionConverter }, new Type[] { typeof(DependencyProperty), typeof(BindingBase) } });
                ControlHandler.ExecuteMethod(l, "SetBinding", new object[] { ArrowLine.X2Property, new Binding("(Canvas.Left)") { Source = to, Converter = arrowPositionConverter }, new Type[] { typeof(DependencyProperty), typeof(BindingBase) } });
                ControlHandler.ExecuteMethod(l, "SetBinding", new object[] { ArrowLine.Y2Property, new Binding("(Canvas.Top)") { Source = to, Converter = arrowPositionConverter }, new Type[] { typeof(DependencyProperty), typeof(BindingBase) } });

                Pair<Ellipse, Ellipse> pair = new Pair<Ellipse, Ellipse>(from, to);
                if (!m_SourceDestination.Contains(pair))
                {
                    m_SourceDestination.Add(pair);
                    m_ArrowsWithSourceAndDestination.Add(pair, l);
                    ControlHandler.AddChild(l, ArrowArea);
                    m_ArrowsMark.Add(counter, l);
                }
                else if (m_ArrowsWithSourceAndDestination.ContainsKey(pair))
                {
                    l = m_ArrowsWithSourceAndDestination[pair];
                    ResetLine(counter, l);
                }
            }

            ControlHandler.ExecuteMethod(this, nameof(MarkPath), new object[] { counter });
        }

        private class ArrowPositionConverter : IValueConverter
        {
            private readonly double offset;

            public ArrowPositionConverter(double offset)
            {
                this.offset = offset;
            }

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                //Correct for the fact that arrow target positions are not pointing exactly at the middle of target ellipses:
                return (double)value + offset;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }

        private void ResetLine(PrimesBigInteger counter, ArrowLine l)
        {
            if (l != null)
            {
                m_ArrowsMark.Add(counter, l);
                UIElementCollection children = ControlHandler.GetPropertyValue(ArrowArea, "Children") as UIElementCollection;
                ControlHandler.ExecuteMethod(children, "Remove", new object[] { l });
                ControlHandler.ExecuteMethod(children, "Add", new object[] { l });
            }
        }

        public void AddCircle(PrimesBigInteger counter, Ellipse source)
        {
            Polyline p = ControlHandler.CreateObject(typeof(Polyline)) as Polyline;
            ControlHandler.SetPropertyValue(p, "Stroke", Brushes.Black);
            ControlHandler.SetPropertyValue(p, "StrokeThickness", 1.5);
            PointCollection pc = ControlHandler.GetPropertyValue(p, "Points") as PointCollection;
            int c = 16;
            double radius = 25;
            double x = (double)ControlHandler.ExecuteMethod(PaintArea, "GetLeft", source);
            x += (PointRadius - radius) / 2;
            double y = (double)ControlHandler.ExecuteMethod(PaintArea, "GetTop", source);
            y += (PointRadius - radius) / 2;

            for (int value = 0; value <= c; value++)
            {
                double angle = (360.0 / c) * value;
                double top = radius / 2 + (Math.Sin((angle * 2 * Math.PI) / 360.0) * radius / 2);
                double left = radius / 2 + (Math.Cos((angle * 2 * Math.PI) / 360.0) * radius / 2);
                ControlHandler.ExecuteMethod(pc, "Add", new object[] { new Point(top, left) });
            }
            if (!m_CirclesSource.ContainsKey(source))
            {
                m_CirclesSource.Add(source, p);
                m_CirclesMark.Add(counter, p);
                ControlHandler.ExecuteMethod(ArrowArea, "SetLeft", new object[] { p, x });
                ControlHandler.ExecuteMethod(ArrowArea, "SetTop", new object[] { p, y });
                ControlHandler.AddChild(p, ArrowArea);
            }
            else
            {
                p = m_CirclesSource[source];
                ResetCircle(counter, p);
            }
        }

        public void MoveCircle(Ellipse source)
        {
            Polyline pl = m_CirclesSource[source];
            double x = (double)ControlHandler.ExecuteMethod(PaintArea, "GetLeft", source);
            double y = (double)ControlHandler.ExecuteMethod(PaintArea, "GetTop", source);
            ControlHandler.ExecuteMethod(ArrowArea, "SetLeft", new object[] { pl, x });
            ControlHandler.ExecuteMethod(ArrowArea, "SetTop", new object[] { pl, y });
        }

        private void ResetCircle(PrimesBigInteger counter, Polyline p)
        {
            if (p != null)
            {
                m_CirclesMark.Add(counter, p);

                UIElementCollection children = ControlHandler.GetPropertyValue(ArrowArea, "Children") as UIElementCollection;
                ControlHandler.ExecuteMethod(children, "Remove", new object[] { p });
                Thread.Sleep(100);
                ControlHandler.ExecuteMethod(children, "Add", new object[] { p });
            }
        }

        #endregion

        private void btnNextStep_Click(object sender, RoutedEventArgs e)
        {
            if (!m_Running)
            {
                StartThread();
            }
            else
            {
                m_StepWiseEvent.Set();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            FireCancelEvent();
            CancelThread();
        }

        public void CleanUp()
        {
            CancelThread();
        }

        private void btnResumeAutomatic_Click(object sender, RoutedEventArgs e)
        {
            if (!m_Running)
            {
                StartThread();
            }

            m_Resume = true;
            m_StepWiseEvent.Set();
            ControlHandler.SetPropertyValue(btnNextStep, "IsEnabled", false);
            ControlHandler.SetPropertyValue(btnResumeAutomatic, "IsEnabled", false);
        }

        private void ContentArea_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ArrowArea.Width = PaintArea.Width = CircleArea.Width = e.NewSize.Width;
            ArrowArea.Height = PaintArea.Height = CircleArea.Height = e.NewSize.Height;
        }

        private bool rotate = false;

        private void PaintArea_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine(e.Source.ToString());
            if (e.Source != null && e.Source == PaintArea)
            {
                rotate = true;
            }
            Cursor = Cursors.ScrollAll;
        }

        private void ArrowArea_MouseLeave(object sender, MouseEventArgs e)
        {
            releaseRotate();
        }

        private void ArrowArea_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            releaseRotate();
        }

        private void releaseRotate()
        {
            lock (m_lockobject)
            {
                Cursor = Cursors.Arrow;
                rotate = false;
                diff = 0;
            }
        }

        private void ArrowArea_PreviewDragOver(object sender, DragEventArgs e)
        {
        }

        private readonly object m_lockobject = new object();
        private int diff = 0;
        private Point previous = new Point(-10000, -10000);

        private void ArrowArea_MouseMove(object sender, MouseEventArgs e)
        {
            lock (m_lockobject)
            {
                if (rotate)
                {
                    double centerY = Aperture / 2;
                    double centerX = Aperture / 2;
                    double diffx = 0;
                    double diffy = 0;
                    if (previous.X == -10000 && previous.Y == -10000)
                    {
                        previous = e.GetPosition(PaintArea);
                    }
                    else
                    {
                        Point actual = e.GetPosition(PaintArea);
                        diffx = previous.X - actual.X;
                        diffy = centerY - previous.Y;
                        previous = actual;
                    }
                    Point currentPosition = e.GetPosition(CircleArea);
                    double diffXp = Math.Abs(((centerX - currentPosition.X)) / 120);
                    if ((diffx < 0 && diffy <= 0) || (diffx >= 0 && diffy >= 0))
                    {
                        diffXp *= -1;
                    }
                    diffXp *= (!m_SortAsc) ? -1 : 1;
                    offset += diffXp;
                    CreatePoints();
                }
            }
        }
    }
}
