using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CrypTool.Resource
{
    public partial class TabContentCtrl : UserControl
    {
        private bool handleTextChangedEvents = true;
        private int selectedIndex;
        private DataTable dataSource;
        private ListViewItem selectedItem = null;

        public int SelectedIndex
        {
            get { return this.selectedIndex; }
            set
            {
                if (resList.Items.Count > value)
                {
                    this.selectedIndex = value;
                    resList.Items[this.selectedIndex].Selected = true;
                }
            }
        }

        public DataTable DataSource
        {
            get { return this.dataSource; }
            set 
            {
                if (this.dataSource != value)
                {
                    this.dataSource = value;
                    DataBind();
                }
            }
        }

        public TabContentCtrl()
        {
            InitializeComponent();
        }

        private void DataBind()
        {
            this.resList.Items.Clear();
            foreach (DataRow dr in this.dataSource.Rows)
            {
                ListViewItem item = new ListViewItem(new string[] { dr["Key"].ToString(), dr["Original"].ToString(), dr["Translation"].ToString() });
                resList.Items.Add(item);
            }
            if (resList.Items.Count > 0)
                resList.SelectedIndices.Add(0);
        }

        private void resList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (resList.SelectedIndices.Count == 1)
            {
                this.selectedItem = resList.SelectedItems[0];
                this.selectedIndex = resList.SelectedIndices[0];
                this.handleTextChangedEvents = false;
                originalTextBox.Text = this.selectedItem.SubItems[1].Text;
                translationTextBox.Text = this.selectedItem.SubItems[2].Text;
                this.handleTextChangedEvents = true;
            }
        }

        private void translationTextBox_TextChanged(object sender, EventArgs e)
        {
            if ((this.handleTextChangedEvents == true) && (this.selectedItem != null))
            {
                this.selectedItem.SubItems[2].Text = this.translationTextBox.Text;
                this.dataSource.Rows[selectedIndex]["Translation"] = this.translationTextBox.Text;
            }
        }
    }
}
