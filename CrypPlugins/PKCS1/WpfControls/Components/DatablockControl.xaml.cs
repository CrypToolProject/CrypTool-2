using Microsoft.Win32;
using PKCS1.Library;
using PKCS1.Resources.lang.Gui;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;


namespace PKCS1.WpfControls.Components
{
    /// <summary>
    /// Interaktionslogik für HwControl.xaml
    /// </summary>
    public partial class DatablockControl : UserControl
    {
        private string fileName = string.Empty;

        public DatablockControl()
        {
            InitializeComponent();
            Initialize();
        }

        public void Initialize()
        {
            // ComboBox befüllen
            this.cbHashFunc.Items.Add(HashFuncIdentHandler.SHA1);
            this.cbHashFunc.Items.Add(HashFuncIdentHandler.SHA256);
            this.cbHashFunc.Items.Add(HashFuncIdentHandler.SHA384);
            this.cbHashFunc.Items.Add(HashFuncIdentHandler.SHA512);
            this.cbHashFunc.Items.Add(HashFuncIdentHandler.MD2);
            this.cbHashFunc.Items.Add(HashFuncIdentHandler.MD5);
            this.cbHashFunc.SelectedIndex = 0;

            this.rbTextFromBox.IsChecked = true;

            Datablock.getInstance().RaiseParamChangedEvent += handleParamChanged;
        }

        #region Eventhandlng

        private void handleParamChanged(ParameterChangeType type)
        {
            if (ParameterChangeType.Message == type)
            {
                if (true == this.tbInputText.IsEnabled)
                {
                    this.tbInputText.Text = Encoding.ASCII.GetString(Datablock.getInstance().Message);
                }
                this.tbHashDigest.Text = Datablock.getInstance().GetHashDigestToHexString();
            }
        }

        public event ParamChanged RaiseDataBlockGenerated;

        private void OnRaiseDataBlockGenerated(ParameterChangeType type)
        {
            if (null != RaiseDataBlockGenerated)
            {
                RaiseDataBlockGenerated(type);
            }
        }

        #endregion

        // fired when Checkbox Selection changed 
        // Hashfunction IdentificationTextbox filled & selected Hashfunction is set
        private void cbHashFunc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Datablock.getInstance().HashFunctionIdent = (HashFunctionIdent)this.cbHashFunc.SelectedValue;
            this.tbHashIdent.Text = Datablock.getInstance().HashFunctionIdent.DERIdent;

            // HashDigest Textboxen leeren; werden bei Execute befüllt
            this.tbHashDigest.Text = string.Empty;
            this.lblHashDigestLength.Text = string.Empty;
        }

        private void bGenerate_Click(object sender, RoutedEventArgs e)
        {
            byte[] byteToHash;

            if (this.rbTextFromBox.IsChecked == true)
            {
                byteToHash = Encoding.ASCII.GetBytes(this.tbInputText.Text);
            }
            else if (this.rbTextfromFile.IsChecked == true)
            {
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                byteToHash = new byte[fs.Length];
                fs.Read(byteToHash, 0, System.Convert.ToInt32(fs.Length));
                fs.Close();
            }
            else
            {
                byteToHash = Encoding.ASCII.GetBytes("Error!");
            }

            // Text setzen, Hash wird automatisch generiert, da in Datablock das Event getriggert wird und hier im Handling Hashgenerierung auslöst
            Datablock.getInstance().Message = byteToHash;
            OnRaiseDataBlockGenerated(ParameterChangeType.DataBlock);
        }

        private void tbHashIdent_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.tbHashIdent.Text != string.Empty)
            {
                this.lblHashIdentLength.Text = string.Format(Common.length, this.tbHashIdent.Text.Length * 4);
            }
        }

        private void tbHashDigest_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.tbHashDigest.Text != string.Empty)
            {
                this.lblHashDigestLength.Text = string.Format(Common.length, this.tbHashDigest.Text.Length * 4);
            }
        }

        private void rbTextfromFile_Checked(object sender, RoutedEventArgs e)
        {
            this.tbInputFile.IsEnabled = true;
            this.bOpenFile.IsEnabled = true;
            this.tbInputText.IsEnabled = false;
        }

        private void rbTextFromBox_Checked(object sender, RoutedEventArgs e)
        {
            this.tbInputFile.IsEnabled = false;
            this.bOpenFile.IsEnabled = false;
            this.tbInputText.IsEnabled = true;
        }

        private void bOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                DefaultExt = ".*"
            };

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                fileName = dlg.FileName;
                this.tbInputFile.Text = fileName;
            }
        }
    }
}
