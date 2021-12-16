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
using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Primes.WpfControls.Components
{
    /// <summary>
    /// Interaction logic for LogControl.xaml
    /// </summary>
    public partial class LogControl : UserControl
    {
        #region Constants

        private const int counterwidth = 15;

        #endregion

        #region Properties

        private readonly SelectableTextBlock m_TextBlock;
        private readonly TextStyle m_TextStyle;
        private int m_CurrentRow;
        private int m_FormerRow;

        public object Title
        {
            get
            {
                if (gbHeader != null)
                {
                    return gbHeader.Header;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                if (gbHeader != null)
                {
                    gbHeader.Header = value;
                }
            }
        }

        private int m_Columns;

        public int Columns
        {
            get => m_Columns;
            set
            {
                m_Columns = value + 1;
                ColumnDefinitionCollection columnDefinitions =
                  ControlHandler.GetPropertyValue(gridMessages, "ColumnDefinitions") as ColumnDefinitionCollection;
                if (m_Columns < columnDefinitions.Count)
                {
                    Clear();
                }
                for (int i = 0; i < m_Columns; i++)
                {
                    if (i >= columnDefinitions.Count)
                    {
                        ColumnDefinition cd = ControlHandler.CreateObject(typeof(ColumnDefinition)) as ColumnDefinition;
                        ControlHandler.ExecuteMethod(columnDefinitions, "Add", new object[] { cd });
                    }
                    double width = counterwidth;
                    GridUnitType unittype = GridUnitType.Auto;
                    if (i > 0)
                    {
                        width = Math.Max((ActualWidth / Columns) - counterwidth, 0);
                        unittype = GridUnitType.Star;
                    }
                    GridLength gl = (GridLength)ControlHandler.CreateObject(typeof(GridLength), new Type[] { typeof(double), typeof(GridUnitType) }, new object[] { width, unittype });
                    ControlHandler.SetPropertyValue(columnDefinitions[i], "Width", gl);
                }
            }
        }

        private bool m_ShowCounter;

        public bool ShowCounter
        {
            get => m_ShowCounter;
            set => m_ShowCounter = value;
        }

        private bool m_OverrideText;
        public bool OverrideText { get => m_OverrideText; set => m_OverrideText = value; }

        //private double m_Widht = double.NaN;
        //public new double Width
        //{
        //  get { return m_Widht; }
        //  set { m_Widht = value; }
        //}

        public new double Width
        {
            get => gridMessages.ActualWidth;
            set
            {
                gridMessages.Width = value;
                foreach (UIElement e in gridMessages.Children)
                {
                    if (e.GetType() == typeof(SelectableTextBlock))
                    {
                        (e as SelectableTextBlock).Width = value - 50;
                    }
                    else if (e.GetType() == typeof(Rectangle))
                    {
                        (e as Rectangle).Width = value - 50;
                    }
                }
            }
        }

        private int counter;

        #endregion

        #region Constructors

        private readonly bool m_Initialized = false;
        private readonly object logobjext = null;

        public LogControl()
        {
            InitializeComponent();
            m_CurrentRow = -1;
            m_FormerRow = -1;
            counter = 1;
            m_Initialized = false;
            logobjext = new object();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (m_Initialized)
            {
                gridMessages.Width = sizeInfo.NewSize.Width;
            }
        }

        #endregion

        #region Events

        public event ExecuteIntegerDelegate RowMouseOver;
        private void FireRowMouseOverEvent(int value)
        {
            if (RowMouseOver != null)
            {
                RowMouseOver(PrimesBigInteger.ValueOf(value).Divide(PrimesBigInteger.Two));
            }
        }

        #endregion

        #region Messaging

        public void Info(string message)
        {
            int row = NewLine();
            Info(message, 0, row);
        }

        public void Info(string message, int column, int row)
        {
            AddMessage(message, TextStyle.InfoStyle.Foreground, column, row);
        }

        public void Error(string message)
        {
            int row = NewLine();
            Error(message, 0, row);
        }

        public void Error(string message, int column, int row)
        {
            AddMessage(message, TextStyle.ErrorStyle.Foreground, column, row);
        }

        private SelectableTextBlock Get(int colum, int row)
        {
            UIElementCollection childs = ControlHandler.GetPropertyValue(gridMessages, "Children") as UIElementCollection;

            if (childs != null)
            {
                IEnumerator _enum = ControlHandler.ExecuteMethod(childs, "GetEnumerator") as IEnumerator;
                while ((bool)ControlHandler.ExecuteMethod(_enum, "MoveNext"))
                {
                    UIElement element = ControlHandler.GetPropertyValue(_enum, "Current") as UIElement;
                    if (element.GetType() == typeof(SelectableTextBlock))
                    {
                        int _row = (int)ControlHandler.ExecuteMethod(gridMessages, "GetRow", new object[] { element });
                        int _col = (int)ControlHandler.ExecuteMethod(gridMessages, "GetColumn", new object[] { element });
                        if (_row == row && _col == colum)
                        {
                            return element as SelectableTextBlock;
                        }
                    }
                }
            }

            return null;
        }

        public void _AddMessage(string message, Brush color, int column, int row)
        {
            lock (logobjext)
            {
                if (message != null)
                {
                    UIElementCollection childs = ControlHandler.GetPropertyValue(gridMessages, "Children") as UIElementCollection;
                    column++;
                    SelectableTextBlock tb = null;
                    if (m_OverrideText)
                    {
                        SelectableTextBlock _tb = Get(column, row);
                        if (_tb != null)
                        {
                            tb = _tb;
                            gridMessages.Children.Remove(tb);
                        }
                        else
                        {
                            tb = ControlHandler.CreateObject(typeof(SelectableTextBlock)) as SelectableTextBlock;
                        }
                    }
                    else
                    {
                        tb = ControlHandler.CreateObject(typeof(SelectableTextBlock)) as SelectableTextBlock;
                    }
                    ControlHandler.SetPropertyValue(tb, "TextWrapping", TextWrapping.Wrap);
                    ControlHandler.SetPropertyValue(tb, "Text", message);
                    ControlHandler.SetPropertyValue(tb, "Foreground", color);
                    //ControlHandler.SetPropertyValue(tb, "Width", Math.Max(this.ActualWidth - 100, 50));
                    ControlHandler.SetPropertyValue(tb, "FontSize", 12);
                    ControlHandler.SetPropertyValue(tb, "HorizontalAlignment", HorizontalAlignment.Left);
                    //ControlHandler.SetPropertyValue(tb, "IsReadOnly", true);
                    tb.Padding = new Thickness(10, 1, 10, 1);

                    Grid.SetColumn(tb, column);
                    Grid.SetRow(tb, row);
                    gridMessages.Children.Add(tb);
                    //ControlHandler.ExecuteMethod(gridMessages, "SetColumn", new object[] { tb, column });
                    //ControlHandler.ExecuteMethod(gridMessages, "SetRow", new object[] { tb, row });
                    //ControlHandler.AddChild(tb, gridMessages);
                    //ControlHandler.ExecuteMethod(childs, "Add", new object[] { tb });
                    if (m_FormerRow != m_CurrentRow)
                    {
                        NewLine();
                        if (m_ShowCounter)
                        {
                            SelectableTextBlock tb1 = ControlHandler.CreateObject(typeof(SelectableTextBlock)) as SelectableTextBlock;
                            ControlHandler.SetPropertyValue(tb1, "TextWrapping", TextWrapping.Wrap);
                            ControlHandler.SetPropertyValue(tb1, "Text", counter.ToString() + ". ");
                            ControlHandler.SetPropertyValue(tb1, "Foreground", color);
                            ControlHandler.SetPropertyValue(tb1, "FontSize", 12);
                            ControlHandler.SetPropertyValue(tb1, "HorizontalAlignment", HorizontalAlignment.Left);

                            Grid.SetColumn(tb1, 0);
                            Grid.SetRow(tb1, row);
                            gridMessages.Children.Add(tb1);

                            //ControlHandler.ExecuteMethod(gridMessages, "SetColumn", new object[] { tb1, 0 });
                            //ControlHandler.ExecuteMethod(gridMessages, "SetRow", new object[] { tb1, row });
                            //ControlHandler.AddChild(tb1, gridMessages);
                        }
                        Rectangle rec = (Rectangle)ControlHandler.CreateObject(typeof(Rectangle));
                        ControlHandler.SetPropertyValue(rec, "Width", Math.Max(ActualWidth - 100, 50));
                        ControlHandler.SetPropertyValue(rec, "Fill", Brushes.LightGray);
                        ControlHandler.SetPropertyValue(rec, "Height", 1.0);
                        ControlHandler.SetPropertyValue(rec, "HorizontalAlignment", HorizontalAlignment.Left);
                        ControlHandler.SetPropertyValue(rec, "VerticalAlignment", VerticalAlignment.Bottom);

                        if (m_Columns > 0)
                        {
                            Grid.SetColumnSpan(rec, m_Columns);
                        }

                        Grid.SetRow(rec, m_CurrentRow);
                        gridMessages.Children.Add(rec);

                        //ControlHandler.ExecuteMethod(gridMessages, "SetColumnSpan", new object[] { rec, m_Columns });
                        //ControlHandler.ExecuteMethod(gridMessages, "SetRow", new object[] { rec, m_CurrentRow });
                        //ControlHandler.AddChild(rec, gridMessages);
                        counter++;
                        m_FormerRow = m_CurrentRow;
                    }

                    ControlHandler.ExecuteMethod(scroller, "ScrollToEnd");
                }
            }
        }

        private void AddMessage(string message, Brush color, int column, int row)
        {
            ControlHandler.ExecuteMethod(this, "_AddMessage", new object[] { message, color, column, row });
        }

        public void Clear()
        {
            ColumnDefinitionCollection columnDefinitions =
              ControlHandler.GetPropertyValue(gridMessages, "ColumnDefinitions") as ColumnDefinitionCollection;
            ControlHandler.ExecuteMethod(columnDefinitions, "Clear");

            RowDefinitionCollection rowDefinitions =
              ControlHandler.GetPropertyValue(gridMessages, "RowDefinitions") as RowDefinitionCollection;
            ControlHandler.ExecuteMethod(rowDefinitions, "Clear");

            UIElementCollection childs =
              ControlHandler.GetPropertyValue(gridMessages, "Children") as UIElementCollection;
            ControlHandler.ExecuteMethod(childs, "Clear");

            m_CurrentRow = m_FormerRow = -1;

            counter = 1;
        }

        public int NewLine()
        {
            m_CurrentRow++;
            RowDefinitionCollection rowDefinitions =
              ControlHandler.GetPropertyValue(gridMessages, "RowDefinitions") as RowDefinitionCollection;
            RowDefinition rd = ControlHandler.CreateObject(typeof(RowDefinition)) as RowDefinition;

            GridLength gl = (GridLength)ControlHandler.CreateObject(typeof(GridLength), new Type[] { typeof(double), typeof(GridUnitType) }, new object[] { 1, GridUnitType.Auto });
            ControlHandler.SetPropertyValue(rd, "Height", gl);

            ControlHandler.ExecuteMethod(rowDefinitions, "Add", new object[] { rd });
            return m_CurrentRow;
        }

        #endregion

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null && sender.GetType() == typeof(MenuItem))
                {
                    MenuItem mi = sender as MenuItem;
                    if (mi == miCopyAll)
                    {
                        int row = -1;
                        StringBuilder result = new StringBuilder();
                        string[] msg = new string[m_Columns];

                        void AppendRow()
                        {
                            string rowString = string.Join("\t", msg.Where(s => !string.IsNullOrEmpty(s)));
                            result.AppendLine(rowString);
                        }

                        foreach (UIElement element in gridMessages.Children)
                        {
                            if (element.GetType() == typeof(SelectableTextBlock))
                            {
                                if (row != Grid.GetRow(element))
                                {
                                    if (row > -1)
                                    {
                                        AppendRow();
                                    }
                                    row = Grid.GetRow(element);
                                }
                                int column = Grid.GetColumn(element);
                                if (column > -1 && column < msg.Length)
                                {
                                    msg[column] = (element as SelectableTextBlock)?.Text;
                                }
                            }
                        }
                        AppendRow();

                        Clipboard.SetText(result.ToString(), TextDataFormat.Text);
                    }
                }
                gridMessages.ContextMenu.IsOpen = false;
            }
            catch (Exception)
            {
            }
        }

        #region Class TextStyle

        private class TextStyle
        {
            private static TextStyle m_InfoStyle;
            private static TextStyle m_ErrorStye;

            private Brush m_Foreground;

            public Brush Foreground
            {
                get => m_Foreground;
                set => m_Foreground = (value == null) ? m_Foreground = Brushes.Green : value;
            }

            private Brush m_Background;

            public Brush Background
            {
                get => m_Background;
                set => m_Background = (value == null) ? m_Background = Brushes.Transparent : value;
            }

            public TextStyle(Brush foreground, Brush background)
            {
                Foreground = foreground;
                Background = background;
            }

            public override bool Equals(object obj)
            {
                if (obj != null && obj.GetType() == typeof(TextStyle))
                {
                    return (obj as TextStyle).Foreground == m_Foreground && (obj as TextStyle).Background == m_Background;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public static TextStyle InfoStyle
            {
                get
                {
                    if (m_InfoStyle == null)
                    {
                        m_InfoStyle = new TextStyle(Brushes.Blue, Brushes.Transparent);
                    }

                    return m_InfoStyle;
                }
            }

            public static TextStyle ErrorStyle
            {
                get
                {
                    if (m_ErrorStye == null)
                    {
                        m_ErrorStye = new TextStyle(Brushes.Red, Brushes.Transparent);
                    }

                    return m_ErrorStye;
                }
            }
        }

        #endregion

        private void scroller_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
        }
    }
}
