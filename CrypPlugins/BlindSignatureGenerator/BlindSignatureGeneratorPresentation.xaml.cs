/*
   Copyright 2019 Axel Wehage

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
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace BlindSignatureGenerator
{
    /// <summary>
    /// Interaktionslogik für BlindSignatureGeneratorPresentation.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("BlindSignatureGenerator.Properties.Resources")]
    public partial class BlindSignatureGeneratorPresentation : UserControl
    {
        public bool presentationEnabled = false;
        public int current_step;
        public string message;
        public byte[] hash;
        public BigInteger blindingfactor;
        public BigInteger publicKey;
        public BigInteger privateKey;
        public BigInteger modulo;
        public byte[] signature;
        public BigInteger signatureNumber;
        public BigInteger[] signaturepaillier;
        public BigInteger blindedmessage;
        private readonly StringBuilder DescriptionBuilder = new StringBuilder();
        private readonly StringBuilder ValuesBuilder = new StringBuilder();

        public BlindSignatureGeneratorPresentation()
        {
            InitializeComponent();
            current_step = 0;
            update();
        }
        public static string ByteArrayToHexString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex;
        }

        private void BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            current_step--;
            update();
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            current_step++;
            update();
        }

        private void update()
        {
            if (!presentationEnabled)
            {
                current_step = 0;
            }
            DescriptionBuilder.Clear();
            ValuesBuilder.Clear();
            if (current_step >= 5)
            {
                current_step = 4;

            }
            if (current_step < 0)
            {
                current_step = 0;
            }
            switch (current_step)
            {
                case 0:
                    {
                        LabelStepText.Content = "0";
                        //DescriptionBuilder.AppendLine("This is the presentation of the Blind Signature Generator. Blind Signatures are a special type of digital signatures which allow a high degree of anonymity and yet can be used for verification of messages.");
                        DescriptionBuilder.AppendLine(Properties.Resources.Slide0Description);
                        DescriptionTextBlock.Text = DescriptionBuilder.ToString();
                        LabelValues.Content = "";
                        ValuesBuilder.AppendLine(Properties.Resources.Slide0Values);
                        //ValuesBuilder.AppendLine("Please run the generator and then click through the steps to be shown the internal calculations of the component.");
                        ValuesTextBlock.Text = ValuesBuilder.ToString();
                        break;
                    }
                case 1:
                    {
                        LabelStepText.Content = "1";
                        DescriptionBuilder.AppendLine(Properties.Resources.Slide1Description);
                        DescriptionTextBlock.Text = DescriptionBuilder.ToString();
                        LabelValues.Content = Properties.Resources.Slide1Values0;
                        ValuesBuilder.Append(Properties.Resources.Slide1Values1);
                        ValuesBuilder.AppendLine(message);
                        ValuesBuilder.Append(Properties.Resources.Slide1Values2);
                        ValuesBuilder.AppendLine(modulo.ToString());
                        ValuesBuilder.Append(Properties.Resources.Slide1Values3);
                        ValuesBuilder.AppendLine(publicKey.ToString());
                        ValuesBuilder.Append(Properties.Resources.Slide1Values4);
                        ValuesBuilder.AppendLine(privateKey.ToString());
                        ValuesTextBlock.Text = ValuesBuilder.ToString();
                        break;
                    }
                case 2:
                    {
                        LabelStepText.Content = "2";
                        DescriptionBuilder.AppendLine(Properties.Resources.Slide2Description);
                        DescriptionTextBlock.Text = DescriptionBuilder.ToString();
                        ValuesBuilder.Append(Properties.Resources.Slide2Values);
                        ValuesBuilder.AppendLine(ByteArrayToHexString(hash).Replace("-", " "));
                        ValuesTextBlock.Text = ValuesBuilder.ToString();
                        break;
                    }
                case 3:
                    {
                        LabelStepText.Content = "3";
                        if (signaturepaillier != null)
                        {
                            DescriptionBuilder.AppendLine(Properties.Resources.Slide3DescriptionPaillier1);
                            DescriptionBuilder.AppendLine(Properties.Resources.Slide3DescriptionPaillier2);
                            ValuesBuilder.AppendLine(Properties.Resources.Slide3ValuesPaillier1 + blindingfactor.ToString());
                        }
                        else
                        {
                            DescriptionBuilder.AppendLine(Properties.Resources.Slide3DescriptionRSA1);
                            DescriptionBuilder.AppendLine(Properties.Resources.Slide3DescriptionRSA2);
                            ValuesBuilder.AppendLine(Properties.Resources.Slide3ValuesRSA1 + blindingfactor.ToString());
                            ValuesBuilder.AppendLine(Properties.Resources.Slide3ValuesRSA2 + blindedmessage.ToString());
                        }
                        DescriptionTextBlock.Text = DescriptionBuilder.ToString();
                        ValuesTextBlock.Text = ValuesBuilder.ToString();
                        break;
                    }
                case 4:
                    {
                        LabelStepText.Content = "4";
                        if (signaturepaillier != null)
                        {
                            DescriptionBuilder.AppendLine(Properties.Resources.Slide4DescriptionPaillier1);
                            DescriptionBuilder.AppendLine(Properties.Resources.Slide4DescriptionPaillier2);
                            DescriptionBuilder.AppendLine(Properties.Resources.Slide4DescriptionPaillier3);
                            DescriptionBuilder.AppendLine(Properties.Resources.Slide4DescriptionPaillier4);
                            DescriptionBuilder.AppendLine(Properties.Resources.Slide4DescriptionPaillier5);
                            ValuesBuilder.Append(Properties.Resources.Slide4ValuesPaillier1);
                            ValuesBuilder.AppendLine(signaturepaillier[0].ToString());
                            ValuesBuilder.Append(Properties.Resources.Slide4ValuesPaillier2);
                            ValuesBuilder.AppendLine(signaturepaillier[1].ToString());
                        }
                        else
                        {
                            DescriptionBuilder.AppendLine(Properties.Resources.Slide4DescriptionRSA1);
                            DescriptionBuilder.AppendLine(Properties.Resources.Slide4DescriptionRSA2);
                            DescriptionBuilder.AppendLine(Properties.Resources.Slide4DescriptionRSA3);
                            ValuesBuilder.AppendLine(Properties.Resources.Slide4ValuesRSA1 + blindingfactor.ToString());
                            ValuesBuilder.Append(Properties.Resources.Slide4ValuesRSA2);
                            ValuesBuilder.AppendLine(ByteArrayToHexString(signature).Replace("-", " "));
                        }
                        DescriptionTextBlock.Text = DescriptionBuilder.ToString();
                        ValuesTextBlock.Text = ValuesBuilder.ToString();
                        break;
                    }
            }
        }
    }
}
