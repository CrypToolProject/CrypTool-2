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
    public partial class FileInfoCtrl : UserControl
    {
        public FileInfoCtrl()
        {
            InitializeComponent();
        }

        public string FileName 
        {
            set 
            { 
                textBox1.Text = value;
            } 
        }
    }
}
