/*
   Copyright 2008 Timm Korte, University of Siegen

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

using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.PRESENT
{
    /// <summary>
    /// Interaktionslogik für Window1.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.PRESENT.Properties.Resources")]
    public partial class PRESENTAnimation : UserControl
    {
        private readonly KS_Animation ks_animation;
        private readonly EC_Animation ec_animation;
        private readonly PresentTracer tracer;
        public Present cipher;
        private readonly byte[] byte_key = new byte[10];
        private readonly byte[] byte_data = new byte[8];

        public PRESENTAnimation()
        {
            try
            {
                InitializeComponent();
                cipher = new Present();
                tracer = new PresentTracer();
                Assign_Click(this, null);
                ec_animation = new EC_Animation(this, this.EC_Model3DGroup);
                ec_animation.InitStart();
                ks_animation = new KS_Animation(this, this.KS_Model3DGroup);
                ks_animation.InitStart();
            }
            catch (Exception)
            {
                //Console.WriteLine(e.ToString());
            }
        }

        public void Assign_Values(byte[] key, byte[] data)
        {
            txt_Data.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate { txt_Data.Text = BitConverter.ToString(data).Replace("-", ""); }, null);
            txt_Key.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate { txt_Key.Text = BitConverter.ToString(key).Replace("-", ""); }, null);
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate { Assign_Click(this, null); }, null);
            //this.Assign_Click(this, null);
        }

        private void Assign_Click(object sender, RoutedEventArgs e)
        {
            HexString2ByteArray(txt_Key.Text, byte_key);
            HexString2ByteArray(txt_Data.Text, byte_data);
            cipher.keyschedule(byte_key);
            cipher.execute(byte_data, true);
            txtTrace.Text = tracer.Trace(cipher);
            if (ks_animation != null)
            {
                ks_animation.InitStart();
            }

            if (ec_animation != null)
            {
                ec_animation.InitStart();
            }
        }

        #region Key Schedule Centric

        private void KS_Nav_Click(object sender, RoutedEventArgs e)
        {
            string action = (sender as Button).Name;
            switch (action)
            {
                case "btn_KS_PrevRound":
                    ks_animation.Control(-2);
                    break;
                case "btn_KS_PrevStep":
                    ks_animation.Control(-1);
                    break;
                case "btn_KS_Pause":
                    ks_animation.Control(0);
                    break;
                case "btn_KS_NextStep":
                    ks_animation.Control(1);
                    break;
                case "btn_KS_NextRound":
                    ks_animation.Control(2);
                    break;
            };
        }

        #endregion

        #region Encryption Centric

        private void EC_Nav_Click(object sender, RoutedEventArgs e)
        {
            string action = (sender as Button).Name;
            switch (action)
            {
                case "btn_EC_PrevRound":
                    ec_animation.Control(-2);
                    break;
                case "btn_EC_PrevStep":
                    ec_animation.Control(-1);
                    break;
                case "btn_EC_Pause":
                    ec_animation.Control(0);
                    break;
                case "btn_EC_NextStep":
                    ec_animation.Control(1);
                    break;
                case "btn_EC_NextRound":
                    ec_animation.Control(2);
                    break;
            };
        }

        #endregion

        #region Helper Functions

        private void HexString2ByteArray(string str, byte[] arr)
        {
            str = "00000000000000000000" + str;
            str = str.Substring(str.Length - (arr.Length * 2));
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = Convert.ToByte(str.Substring((i * 2), 2), 16);
            }
        }

        #endregion
    }
}
