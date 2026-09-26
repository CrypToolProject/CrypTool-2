/*
   Copyright (C) CrypTool 2 Team

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
using System.Collections.Generic;
using System.Data;
using System.Windows.Controls;
using Label = System.Windows.Controls.Label;
using UserControl = System.Windows.Controls.UserControl;

namespace CrypTool.StraddlingCheckerboard
{
    /// <summary>
    /// Interaction logic for StraddlingCheckerboardPresentation.xaml
    /// </summary>
    public partial class StraddlingCheckerboardPresentation : UserControl
    {
        public Canvas Canvas { get; set; }
        public List<int?> Rows { get; set; }
        public DataTable DataTable { get; set; }
        public StraddlingCheckerboardPresentation()
        {
            InitializeComponent();
            ClearCheckerboard();
        }

        public Label ShowPlayText()
        {
            Label label = new Label { Content = Properties.Resources.ShowPlayText };
            return label;
        }

        public void ShowCheckerboard(DataTable tableContent, List<int?> checkerboardRows)
        {
            DataGrid.IsEnabled = true;
            DataGrid.LoadingRow += DataGrid_OnLoadingRow;
            Rows = checkerboardRows;
            DataTable = tableContent;
            DataGrid.DataContext = tableContent.DefaultView;
            DataGrid.CanUserAddRows = false;
            DataGrid.CanUserDeleteRows = false;
            DataGrid.CanUserReorderColumns = false;
            DataGrid.CanUserResizeColumns = false;
            DataGrid.CanUserResizeRows = false;
            DataGrid.CanUserSortColumns = false;
            DataGrid.IsReadOnly = true;
            Viewbox.Child = DataGrid;
        }

        private void DataGrid_OnLoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = Rows[e.Row.GetIndex()] == null ? "" : Rows[e.Row.GetIndex()].ToString();
        }

        public void ClearCheckerboard()
        {
            Viewbox.Child = ShowPlayText();
        }

        public void DisableCheckerboard()
        {
            DataGrid.IsEnabled = false;
        }
    }
}
