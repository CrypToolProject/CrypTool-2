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
