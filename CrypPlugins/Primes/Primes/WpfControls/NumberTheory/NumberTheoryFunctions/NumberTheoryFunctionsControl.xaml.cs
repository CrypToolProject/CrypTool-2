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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;

namespace Primes.WpfControls.NumberTheory.NumberTheoryFunctions
{
    /// <summary>
    /// Interaction logic for NumberTheoryFunctionsControl.xaml
    /// </summary>

    public partial class NumberTheoryFunctionsControl : System.Windows.Controls.UserControl, IPrimeMethodDivision
    {
        private NTFunctions m_SourceFunctions;
        private NTFunctions m_DestinationFunctions;
        private readonly DataGridView m_DgvFunctions;
        private readonly DataTable m_DataTable;
        private readonly IDictionary<INTFunction, DataColumn> m_ColumnsDict;
        private readonly DataColumn m_DcN;
        private bool initialized = false;

        public NumberTheoryFunctionsControl()
        {
            InitializeComponent();

            InitFunctions();
            m_ColumnsDict = new Dictionary<INTFunction, DataColumn>();
            m_DataTable = new DataTable();
            m_DcN = new DataColumn("n");

            m_DataTable.Columns.Add(m_DcN);
            m_DgvFunctions = new DataGridView
            {
                AllowUserToOrderColumns = true,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                RowHeadersVisible = false,
                DataSource = m_DataTable,
                AllowUserToResizeColumns = true
            };
            DataGridViewHost.Child = m_DgvFunctions;
            AddGridMenu();
            irc.Execute += new Primes.WpfControls.Components.ExecuteDelegate(irc_Execute);
            irc.Cancel += new VoidDelegate(irc_Cancel);

            irc.IntervalSizeCanBeZero = true;

            InputValidator<PrimesBigInteger> iv = new InputValidator<PrimesBigInteger>
            {
                Validator = new PositiveBigIntegerValidator()
            };
            irc.AddInputValidator(InputRangeControl.SecondParameter, iv);
            irc.AddInputValidator(InputRangeControl.FreeFrom, iv);
            irc.AddInputValidator(InputRangeControl.CalcFromFactor, iv);
            irc.AddInputValidator(InputRangeControl.CalcFromBase, iv);
            irc.AddInputValidator(InputRangeControl.CalcFromExp, iv);

            //irc.SecondParameterPresent = NeedsSecondParameter;
            irc.pnlSecondParameter.IsEnabled = NeedsSecondParameter;

            SourceFunctionView.SortDescriptions.Add(new SortDescription("Description", ListSortDirection.Ascending));
            DestinationFunctionView.SortDescriptions.Add(new SortDescription("Description", ListSortDirection.Ascending));
        }

        #region Menu

        private void AddGridMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            m_DgvFunctions.ContextMenuStrip = menu;
            menu.Items.Add(Primes.Resources.lang.Numbertheory.Numbertheory.copy_selectedrow, null, new EventHandler(MenuItemClick_CopySelectionToClipboard));
            menu.Items.Add(Primes.Resources.lang.Numbertheory.Numbertheory.copy_table, null, new EventHandler(MenuItemClick_CopyAllToClipboard));
            menu.Items.Add(Primes.Resources.lang.Numbertheory.Numbertheory.copy_savetable, null, new EventHandler(MenuItemClick_CopyAllToFile));
        }

        private void MenuItemClick_CopySelectionToClipboard(object sender, EventArgs e)
        {
            DataGridViewRow row = m_DgvFunctions.CurrentRow;
            if (row != null)
            {
                string header = GetHeader('\t');
                string[] content = new string[m_DgvFunctions.Columns.Count];
                foreach (DataGridViewColumn c in m_DgvFunctions.Columns)
                {
                    content[c.DisplayIndex] = row.Cells[c.Index].FormattedValue.ToString();
                }
                string _content = string.Empty;
                foreach (string c in content)
                {
                    if (!string.IsNullOrEmpty(_content))
                    {
                        _content += "\t";
                    }

                    _content += c;
                }
                System.Windows.Clipboard.SetText(header + "\r\n" + _content);
            }
        }

        private void MenuItemClick_CopyAllToClipboard(object sender, EventArgs e)
        {
            if (m_DgvFunctions.Rows.Count > 1)
            {
                string header = GetHeader('\t');
                string content = GetAllGridContent('\t');
                System.Windows.Clipboard.SetText(header + "\r\n" + content);
            }
        }

        private void MenuItemClick_CopyAllToFile(object sender, EventArgs e)
        {
            if (m_DgvFunctions.Rows.Count > 1)
            {
                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "CSV-Datei|*.csv"
                };
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    string header = GetHeader(';');
                    string content = GetAllGridContent(';');
                    string filecontent = header + "\r\n" + content;
                    File.WriteAllText(sfd.FileName, filecontent);
                }
            }
        }

        private string GetHeader(char separator)
        {
            string result = string.Empty;
            string[] header = new string[m_DgvFunctions.Columns.Count];

            foreach (DataGridViewColumn c in m_DgvFunctions.Columns)
            {
                header[c.DisplayIndex] = c.HeaderText;
            }

            foreach (string h in header)
            {
                if (!string.IsNullOrEmpty(result))
                {
                    result += separator;
                }

                result += h;
            }

            return result;
        }

        private string GetAllGridContent(char separator)
        {
            string result = string.Empty;
            if (m_DgvFunctions.Rows.Count > 1)
            {
                string header = GetHeader('\t');

                string[][] content = new string[m_DgvFunctions.Rows.Count][];
                foreach (DataGridViewRow row in m_DgvFunctions.Rows)
                {
                    content[row.Index] = new string[m_DgvFunctions.Columns.Count];
                    foreach (DataGridViewColumn c in m_DgvFunctions.Columns)
                    {
                        content[row.Index][c.DisplayIndex] = row.Cells[c.Index].FormattedValue.ToString();
                    }
                }

                foreach (string[] _s in content)
                {
                    if (!string.IsNullOrEmpty(result))
                    {
                        result += "\r\n";
                    }

                    string innercontent = string.Empty;
                    foreach (string _s2 in _s)
                    {
                        if (!string.IsNullOrEmpty(innercontent))
                        {
                            innercontent += separator;
                        }

                        innercontent += _s2;
                    }
                    result += innercontent;
                }
            }

            return result;
        }

        #endregion

        private void InitFunctions()
        {
            m_SourceFunctions = new NTFunctions();
            m_DestinationFunctions = new NTFunctions();

            INTFunction f1 = new EulerPhi();
            f1.Message += new NumberTheoryMessageDelegate(f1_Message);
            f1.OnStop += new VoidDelegate(f1_OnStop);
            m_SourceFunctions.Add(f1);

            INTFunction f3 = new EulerPhiValues();
            f3.Message += new NumberTheoryMessageDelegate(f1_Message);
            f3.OnStop += new VoidDelegate(f1_OnStop);
            m_SourceFunctions.Add(f3);

            INTFunction f2 = new Tau();
            f2.Message += new NumberTheoryMessageDelegate(f1_Message);
            f2.OnStop += new VoidDelegate(f1_OnStop);
            m_SourceFunctions.Add(f2);

            INTFunction f4 = new Rho();
            f4.Message += new NumberTheoryMessageDelegate(f1_Message);
            f4.OnStop += new VoidDelegate(f1_OnStop);
            m_SourceFunctions.Add(f4);

            INTFunction f5 = new EulerPhiSum();
            f5.Message += new NumberTheoryMessageDelegate(f1_Message);
            f5.OnStop += new VoidDelegate(f1_OnStop);
            m_SourceFunctions.Add(f5);

            INTFunction f6 = new TauValues();
            f6.Message += new NumberTheoryMessageDelegate(f1_Message);
            f6.OnStop += new VoidDelegate(f1_OnStop);
            m_SourceFunctions.Add(f6);

            INTFunction f7 = new PiX();
            f7.Message += new NumberTheoryMessageDelegate(f1_Message);
            f7.OnStop += new VoidDelegate(f1_OnStop);
            m_SourceFunctions.Add(f7);

            INTFunction f8 = new GCD();
            f8.Message += new NumberTheoryMessageDelegate(f1_Message);
            f8.OnStop += new VoidDelegate(f1_OnStop);
            m_SourceFunctions.Add(f8);

            INTFunction f9 = new LCM();
            f9.Message += new NumberTheoryMessageDelegate(f1_Message);
            f9.OnStop += new VoidDelegate(f1_OnStop);
            m_SourceFunctions.Add(f9);

            INTFunction f10 = new ModInv();
            f10.Message += new NumberTheoryMessageDelegate(f1_Message);
            f10.OnStop += new VoidDelegate(f1_OnStop);
            m_SourceFunctions.Add(f10);

            INTFunction f11 = new ExtEuclid();
            f11.Message += new NumberTheoryMessageDelegate(f1_Message);
            f11.OnStop += new VoidDelegate(f1_OnStop);
            m_SourceFunctions.Add(f11);

            lbDestination.DataContext = m_DestinationFunctions;
            lbSource.DataContext = m_SourceFunctions;
        }

        private readonly object stopobject = new object();

        private void f1_OnStop()
        {
            lock (stopobject)
            {
                bool allstopped = true;
                foreach (INTFunction f in m_DestinationFunctions)
                {
                    allstopped &= !f.IsRunnung;
                }
                if (allstopped)
                {
                    irc.UnLockControls();
                    ControlHandler.SetPropertyValue(gridChooseFunctions, "IsEnabled", true);
                }
            }
        }

        private void irc_Cancel()
        {
            foreach (INTFunction f in m_DestinationFunctions)
            {
                f.Stop();
            }

            irc.UnLockControls();
            gridChooseFunctions.IsEnabled = true;
        }

        private readonly object locksetdata = new object();

        private void f1_Message(INTFunction function, PrimesBigInteger value, string message)
        {
            lock (locksetdata)
            {
                if (m_ColumnsDict.ContainsKey(function))
                {
                    DataRow[] rows = m_DataTable.Select("n = '" + value.ToString() + "'");
                    if (rows != null && rows.Length == 1)
                    {
                        DataColumn dc = m_ColumnsDict[function];
                        int col = m_DataTable.Columns.IndexOf(dc);
                        int row = m_DataTable.Rows.IndexOf(rows[0]);
                        SetData(col, row, message);
                    }
                }
            }
        }

        private bool NeedsSecondParameter
        {
            get
            {
                foreach (INTFunction f in m_DestinationFunctions)
                {
                    if (f.NeedsSecondParameter)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private delegate void SetDataDelegate(int column, int row, string data);

        private void SetData(int column, int row, string data)
        {
            if (m_DgvFunctions.InvokeRequired)
            {
                m_DgvFunctions.Invoke(new SetDataDelegate(SetData), new object[] { column, row, data });
            }
            else
            {
                if (column >= 0 && column < m_DataTable.Columns.Count)
                {
                    if (row >= 0)
                    {
                        DataRow dr = null;

                        if (row >= m_DataTable.Rows.Count)
                        {
                            dr = m_DataTable.NewRow();
                            m_DataTable.Rows.Add(dr);
                        }
                        else
                        {
                            dr = m_DataTable.Rows[row];
                        }

                        if (dr != null)
                        {
                            dr[column] = data;
                        }
                    }
                }
            }
        }

        private void irc_Execute(PrimesBigInteger from, PrimesBigInteger to, PrimesBigInteger second)
        {
            m_DataTable.Clear();

            int column = m_DataTable.Columns.IndexOf(m_DcN);

            for (int x = 0; x <= (to - from).IntValue; x++)
            {
                SetData(column, x, (from + x).ToString());
            }

            if (m_DestinationFunctions.Count > 0)
            {
                gridChooseFunctions.IsEnabled = false;
                foreach (INTFunction f in m_DestinationFunctions)
                {
                    if (!f.NeedsSecondParameter)
                    {
                        f.Start(from, to, second);
                    }
                    else
                    {
                        if (irc.ValidateSecondInput(ref second))
                        {
                            f.Start(from, to, second);
                        }
                        else
                        {
                            f1_OnStop();
                        }
                    }
                }
            }
            else
            {
                irc.UnLockControls();
            }
        }

        #region IPrimeUserControl Members

        public void Dispose()
        {
        }

        public void SetTab(int i)
        {
        }

        #endregion

        private ICollectionView SourceFunctionView => CollectionViewSource.GetDefaultView(m_SourceFunctions);

        private ICollectionView DestinationFunctionView => CollectionViewSource.GetDefaultView(m_DestinationFunctions);

        private void btnToExec_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<object> selectd = lbSource.SelectedItems as ObservableCollection<object>;
            object[] objs = selectd.ToArray<object>();
            foreach (INTFunction fun in objs)
            {
                m_SourceFunctions.Remove(fun);
                m_DestinationFunctions.Add(fun);
                AddFunction(fun);
            }
        }

        private void AddFunction(INTFunction function)
        {
            DataColumn dc = new DataColumn(function.Description);

            m_ColumnsDict.Add(function, dc);
            //m_DataTable.Clear();
            m_DataTable.Columns.Add(dc);

            //irc.SecondParameterPresent = NeedsSecondParameter;
            irc.pnlSecondParameter.IsEnabled = NeedsSecondParameter;
            irc.lblInfoSecond.Text = NeedsSecondParameter ? Primes.Resources.lang.Numbertheory.Numbertheory.secondparametermissing : "";
        }

        private void RemoveFunction(INTFunction function)
        {
            if (m_ColumnsDict.ContainsKey(function))
            {
                //m_DataTable.Clear();
                m_DataTable.Columns.Remove(m_ColumnsDict[function]);
                m_ColumnsDict.Remove(function);

                //irc.SecondParameterPresent = NeedsSecondParameter;
                irc.pnlSecondParameter.IsEnabled = NeedsSecondParameter;
                irc.lblInfoSecond.Text = NeedsSecondParameter ? Primes.Resources.lang.Numbertheory.Numbertheory.secondparametermissing : "";
            }
        }

        private void btnToDontExec_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<object> selectd = lbDestination.SelectedItems as ObservableCollection<object>;
            object[] objs = selectd.ToArray<object>();
            foreach (INTFunction fun in objs)
            {
                m_DestinationFunctions.Remove(fun);
                m_SourceFunctions.Add(fun);
                RemoveFunction(fun);
            }
        }

        private void lbSource_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.ListBox lb = null;
            if (sender is System.Windows.Controls.ListBox)
            {
                lb = sender as System.Windows.Controls.ListBox;
            }
            else if (sender is ListBoxItem)
            {
                lb = ((ListBoxItem)sender).Parent as System.Windows.Controls.ListBox;
            }

            if (lb != null)
            {
                IList selected = (sender as System.Windows.Controls.ListBox).SelectedItems;
                if (selected != null && selected.Count > 0)
                {
                    if (sender == lbSource)
                    {
                        lbDestination.AllowDrop = true;
                        lbSource.AllowDrop = false;
                    }
                    else
                    {
                        lbDestination.AllowDrop = false;
                        lbSource.AllowDrop = true;
                    }
                    DragDrop.DoDragDrop(sender as System.Windows.Controls.ListBox, selected, System.Windows.DragDropEffects.Move);
                }
            }
        }

        private void lbSource_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem)
            {
                btnToExec_Click(sender, e);
            }
        }

        private void lbDestination_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem)
            {
                btnToDontExec_Click(sender, e);
            }
        }

        private void lbDestination_Drop(object sender, System.Windows.DragEventArgs e)
        {
            IList selected = (sender as System.Windows.Controls.ListBox).SelectedItems;

            ObservableCollection<object> dropped;
            NTFunctions dest = null;
            NTFunctions source = null;

            if (sender == lbDestination)
            {
                source = m_SourceFunctions;
                dest = m_DestinationFunctions;
            }
            else
            {
                dest = m_SourceFunctions;
                source = m_DestinationFunctions;
            }

            try
            {
                dropped = e.Data.GetData("System.Windows.Controls.SelectedItemCollection") as ObservableCollection<object>;
                object[] objs = dropped.ToArray<object>();

                foreach (object o in objs)
                {
                    if (o.GetType().GetInterface(typeof(INTFunction).ToString()) != null)
                    {
                        source.Remove(o as INTFunction);
                    }

                    dest.Add(o as INTFunction);

                    if (sender == lbDestination)
                    {
                        AddFunction(o as INTFunction);
                    }
                    else
                    {
                        RemoveFunction(o as INTFunction);
                    }
                }
            }
            catch { }
        }

        #region IPrimeUserControl Members

        public void Init()
        {
            throw new NotImplementedException();
        }

        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!initialized)
            {
                irc.SetText(InputRangeControl.FreeFrom, "2");
                irc.SetText(InputRangeControl.FreeTo, "42");
                irc.SetText(InputRangeControl.CalcFromFactor, "1");
                irc.SetText(InputRangeControl.CalcFromBase, "2");
                irc.SetText(InputRangeControl.CalcFromExp, "1");
                irc.SetText(InputRangeControl.CalcFromSum, "0");
                irc.SetText(InputRangeControl.CalcToFactor, "1");
                irc.SetText(InputRangeControl.CalcToBase, "42");
                irc.SetText(InputRangeControl.CalcToExp, "1");
                irc.SetText(InputRangeControl.CalcToSum, "0");

                initialized = true;
            }
        }
    }
}
