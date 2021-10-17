using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Data.SqlClient;

namespace CrypTool.Plugins.Blockchain
{
    /// <summary>
    /// Interaktionslogik für TransactionPresentation.xaml
    /// </summary>
    public partial class TransactionPresentation : UserControl
    {

        DataTable dt;

        public TransactionPresentation()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)

        {

            dt = new DataTable("emp");

            DataColumn dc1 = new DataColumn("From", typeof(string));

            DataColumn dc2 = new DataColumn("To", typeof(string));

            DataColumn dc3 = new DataColumn("Amount", typeof(string));

            DataColumn dc4 = new DataColumn("Signature", typeof(string));

            DataColumn dc5 = new DataColumn("Hash", typeof(string));

            DataColumn dc6 = new DataColumn("PrevHash", typeof(string));

            dt.Columns.Add(dc1);

            dt.Columns.Add(dc2);

            dt.Columns.Add(dc3);

            dt.Columns.Add(dc4);

            dt.Columns.Add(dc5);

            dt.Columns.Add(dc6);

            dataGrid1.ItemsSource = dt.DefaultView;

        }

        DataRow dr;
        private void addRow(string from, string to, string amount, string signature, string hash, string prevHash)

        {

            dr = dt.NewRow();

            dr[0] = from;

            dr[1] = to;

            dr[2] = amount;

            dr[3] = signature;

            dr[4] = hash;

            dr[5] = prevHash;

            dt.Rows.Add(dr);

            dataGrid1.ItemsSource = dt.DefaultView;

        }

    }
}
