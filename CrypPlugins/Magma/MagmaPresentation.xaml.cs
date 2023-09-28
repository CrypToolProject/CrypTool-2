/*
   Copyright 2022 Nils Kopal, CrypTool project

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
using System.Windows.Controls;

namespace CrypTool.Plugins.Magma
{
    /// <summary>
    /// Interaktionslogik für MagmaPresentation.xaml
    /// </summary>
    [PluginBase.Attributes.Localization("CrypTool.Plugins.Magma.Properties.Resources")]
    public partial class MagmaPresentation : UserControl
    {
        public MagmaPresentation()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Visualizes the complete data in the ui for encryption
        /// </summary>
        /// <param name="data"></param>
        public void VisualizeCompleteDataEncryption(MagmaCompleteData data)
        {
            EncryptGrid.Visibility = System.Windows.Visibility.Visible;
            DecryptGrid.Visibility = System.Windows.Visibility.Hidden;

            //set all round keys
            EncryptRound1.RoundKey = data.RoundKey[0].ToUpper();
            EncryptRound2.RoundKey = data.RoundKey[1].ToUpper();
            EncryptRound3.RoundKey = data.RoundKey[2].ToUpper();
            EncryptRound4.RoundKey = data.RoundKey[3].ToUpper();
            EncryptRound5.RoundKey = data.RoundKey[4].ToUpper();
            EncryptRound6.RoundKey = data.RoundKey[5].ToUpper();
            EncryptRound7.RoundKey = data.RoundKey[6].ToUpper();
            EncryptRound8.RoundKey = data.RoundKey[7].ToUpper();
            EncryptRound9.RoundKey = data.RoundKey[8].ToUpper();
            EncryptRound10.RoundKey = data.RoundKey[9].ToUpper();
            EncryptRound11.RoundKey = data.RoundKey[10].ToUpper();
            EncryptRound12.RoundKey = data.RoundKey[11].ToUpper();
            EncryptRound13.RoundKey = data.RoundKey[12].ToUpper();
            EncryptRound14.RoundKey = data.RoundKey[13].ToUpper();
            EncryptRound15.RoundKey = data.RoundKey[14].ToUpper();
            EncryptRound16.RoundKey = data.RoundKey[15].ToUpper();
            EncryptRound17.RoundKey = data.RoundKey[16].ToUpper();
            EncryptRound18.RoundKey = data.RoundKey[17].ToUpper();
            EncryptRound19.RoundKey = data.RoundKey[18].ToUpper();
            EncryptRound20.RoundKey = data.RoundKey[19].ToUpper();
            EncryptRound21.RoundKey = data.RoundKey[20].ToUpper();
            EncryptRound22.RoundKey = data.RoundKey[21].ToUpper();
            EncryptRound23.RoundKey = data.RoundKey[22].ToUpper();
            EncryptRound24.RoundKey = data.RoundKey[23].ToUpper();
            EncryptRound25.RoundKey = data.RoundKey[24].ToUpper();
            EncryptRound26.RoundKey = data.RoundKey[25].ToUpper();
            EncryptRound27.RoundKey = data.RoundKey[26].ToUpper();
            EncryptRound28.RoundKey = data.RoundKey[27].ToUpper();
            EncryptRound29.RoundKey = data.RoundKey[28].ToUpper();
            EncryptRound30.RoundKey = data.RoundKey[29].ToUpper();
            EncryptRound31.RoundKey = data.RoundKey[30].ToUpper();
            EncryptRound32.RoundKey = data.RoundKey[31].ToUpper();

            //set all left in/out and right in/out
            EncryptRound1.LeftIn = data.LeftIn[0].ToUpper();
            EncryptRound2.LeftIn = data.LeftIn[1].ToUpper();
            EncryptRound3.LeftIn = data.LeftIn[2].ToUpper();
            EncryptRound4.LeftIn = data.LeftIn[3].ToUpper();
            EncryptRound5.LeftIn = data.LeftIn[4].ToUpper();
            EncryptRound6.LeftIn = data.LeftIn[5].ToUpper();
            EncryptRound7.LeftIn = data.LeftIn[6].ToUpper();
            EncryptRound8.LeftIn = data.LeftIn[7].ToUpper();
            EncryptRound9.LeftIn = data.LeftIn[8].ToUpper();
            EncryptRound10.LeftIn = data.LeftIn[9].ToUpper();
            EncryptRound11.LeftIn = data.LeftIn[10].ToUpper();
            EncryptRound12.LeftIn = data.LeftIn[11].ToUpper();
            EncryptRound13.LeftIn = data.LeftIn[12].ToUpper();
            EncryptRound14.LeftIn = data.LeftIn[13].ToUpper();
            EncryptRound15.LeftIn = data.LeftIn[14].ToUpper();
            EncryptRound16.LeftIn = data.LeftIn[15].ToUpper();
            EncryptRound17.LeftIn = data.LeftIn[16].ToUpper();
            EncryptRound18.LeftIn = data.LeftIn[17].ToUpper();
            EncryptRound19.LeftIn = data.LeftIn[18].ToUpper();
            EncryptRound20.LeftIn = data.LeftIn[19].ToUpper();
            EncryptRound21.LeftIn = data.LeftIn[20].ToUpper();
            EncryptRound22.LeftIn = data.LeftIn[21].ToUpper();
            EncryptRound23.LeftIn = data.LeftIn[22].ToUpper();
            EncryptRound24.LeftIn = data.LeftIn[23].ToUpper();
            EncryptRound25.LeftIn = data.LeftIn[24].ToUpper();
            EncryptRound26.LeftIn = data.LeftIn[25].ToUpper();
            EncryptRound27.LeftIn = data.LeftIn[26].ToUpper();
            EncryptRound28.LeftIn = data.LeftIn[27].ToUpper();
            EncryptRound29.LeftIn = data.LeftIn[28].ToUpper();
            EncryptRound30.LeftIn = data.LeftIn[29].ToUpper();
            EncryptRound31.LeftIn = data.LeftIn[30].ToUpper();
            EncryptRound32.LeftIn = data.LeftIn[31].ToUpper();

            EncryptRound1.LeftOut = data.LeftOut[0].ToUpper();
            EncryptRound2.LeftOut = data.LeftOut[1].ToUpper();
            EncryptRound3.LeftOut = data.LeftOut[2].ToUpper();
            EncryptRound4.LeftOut = data.LeftOut[3].ToUpper();
            EncryptRound5.LeftOut = data.LeftOut[4].ToUpper();
            EncryptRound6.LeftOut = data.LeftOut[5].ToUpper();
            EncryptRound7.LeftOut = data.LeftOut[6].ToUpper();
            EncryptRound8.LeftOut = data.LeftOut[7].ToUpper();
            EncryptRound9.LeftOut = data.LeftOut[8].ToUpper();
            EncryptRound10.LeftOut = data.LeftOut[9].ToUpper();
            EncryptRound11.LeftOut = data.LeftOut[10].ToUpper();
            EncryptRound12.LeftOut = data.LeftOut[11].ToUpper();
            EncryptRound13.LeftOut = data.LeftOut[12].ToUpper();
            EncryptRound14.LeftOut = data.LeftOut[13].ToUpper();
            EncryptRound15.LeftOut = data.LeftOut[14].ToUpper();
            EncryptRound16.LeftOut = data.LeftOut[15].ToUpper();
            EncryptRound17.LeftOut = data.LeftOut[16].ToUpper();
            EncryptRound18.LeftOut = data.LeftOut[17].ToUpper();
            EncryptRound19.LeftOut = data.LeftOut[18].ToUpper();
            EncryptRound20.LeftOut = data.LeftOut[19].ToUpper();
            EncryptRound21.LeftOut = data.LeftOut[20].ToUpper();
            EncryptRound22.LeftOut = data.LeftOut[21].ToUpper();
            EncryptRound23.LeftOut = data.LeftOut[22].ToUpper();
            EncryptRound24.LeftOut = data.LeftOut[23].ToUpper();
            EncryptRound25.LeftOut = data.LeftOut[24].ToUpper();
            EncryptRound26.LeftOut = data.LeftOut[25].ToUpper();
            EncryptRound27.LeftOut = data.LeftOut[26].ToUpper();
            EncryptRound28.LeftOut = data.LeftOut[27].ToUpper();
            EncryptRound29.LeftOut = data.LeftOut[28].ToUpper();
            EncryptRound30.LeftOut = data.LeftOut[29].ToUpper();
            EncryptRound31.LeftOut = data.LeftOut[30].ToUpper();
            EncryptRound32.LeftOut = data.LeftOut[31].ToUpper();

            EncryptRound1.RightIn = data.RightIn[0].ToUpper();
            EncryptRound2.RightIn = data.RightIn[1].ToUpper();
            EncryptRound3.RightIn = data.RightIn[2].ToUpper();
            EncryptRound4.RightIn = data.RightIn[3].ToUpper();
            EncryptRound5.RightIn = data.RightIn[4].ToUpper();
            EncryptRound6.RightIn = data.RightIn[5].ToUpper();
            EncryptRound7.RightIn = data.RightIn[6].ToUpper();
            EncryptRound8.RightIn = data.RightIn[7].ToUpper();
            EncryptRound9.RightIn = data.RightIn[8].ToUpper();
            EncryptRound10.RightIn = data.RightIn[9].ToUpper();
            EncryptRound11.RightIn = data.RightIn[10].ToUpper();
            EncryptRound12.RightIn = data.RightIn[11].ToUpper();
            EncryptRound13.RightIn = data.RightIn[12].ToUpper();
            EncryptRound14.RightIn = data.RightIn[13].ToUpper();
            EncryptRound15.RightIn = data.RightIn[14].ToUpper();
            EncryptRound16.RightIn = data.RightIn[15].ToUpper();
            EncryptRound17.RightIn = data.RightIn[16].ToUpper();
            EncryptRound18.RightIn = data.RightIn[17].ToUpper();
            EncryptRound19.RightIn = data.RightIn[18].ToUpper();
            EncryptRound20.RightIn = data.RightIn[19].ToUpper();
            EncryptRound21.RightIn = data.RightIn[20].ToUpper();
            EncryptRound22.RightIn = data.RightIn[21].ToUpper();
            EncryptRound23.RightIn = data.RightIn[22].ToUpper();
            EncryptRound24.RightIn = data.RightIn[23].ToUpper();
            EncryptRound25.RightIn = data.RightIn[24].ToUpper();
            EncryptRound26.RightIn = data.RightIn[25].ToUpper();
            EncryptRound27.RightIn = data.RightIn[26].ToUpper();
            EncryptRound28.RightIn = data.RightIn[27].ToUpper();
            EncryptRound29.RightIn = data.RightIn[28].ToUpper();
            EncryptRound30.RightIn = data.RightIn[29].ToUpper();
            EncryptRound31.RightIn = data.RightIn[30].ToUpper();
            EncryptRound32.RightIn = data.RightIn[31].ToUpper();

            EncryptRound1.RightOut = data.RightOut[0].ToUpper();
            EncryptRound2.RightOut = data.RightOut[1].ToUpper();
            EncryptRound3.RightOut = data.RightOut[2].ToUpper();
            EncryptRound4.RightOut = data.RightOut[3].ToUpper();
            EncryptRound5.RightOut = data.RightOut[4].ToUpper();
            EncryptRound6.RightOut = data.RightOut[5].ToUpper();
            EncryptRound7.RightOut = data.RightOut[6].ToUpper();
            EncryptRound8.RightOut = data.RightOut[7].ToUpper();
            EncryptRound9.RightOut = data.RightOut[8].ToUpper();
            EncryptRound10.RightOut = data.RightOut[9].ToUpper();
            EncryptRound11.RightOut = data.RightOut[10].ToUpper();
            EncryptRound12.RightOut = data.RightOut[11].ToUpper();
            EncryptRound13.RightOut = data.RightOut[12].ToUpper();
            EncryptRound14.RightOut = data.RightOut[13].ToUpper();
            EncryptRound15.RightOut = data.RightOut[14].ToUpper();
            EncryptRound16.RightOut = data.RightOut[15].ToUpper();
            EncryptRound17.RightOut = data.RightOut[16].ToUpper();
            EncryptRound18.RightOut = data.RightOut[17].ToUpper();
            EncryptRound19.RightOut = data.RightOut[18].ToUpper();
            EncryptRound20.RightOut = data.RightOut[19].ToUpper();
            EncryptRound21.RightOut = data.RightOut[20].ToUpper();
            EncryptRound22.RightOut = data.RightOut[21].ToUpper();
            EncryptRound23.RightOut = data.RightOut[22].ToUpper();
            EncryptRound24.RightOut = data.RightOut[23].ToUpper();
            EncryptRound25.RightOut = data.RightOut[24].ToUpper();
            EncryptRound26.RightOut = data.RightOut[25].ToUpper();
            EncryptRound27.RightOut = data.RightOut[26].ToUpper();
            EncryptRound28.RightOut = data.RightOut[27].ToUpper();
            EncryptRound29.RightOut = data.RightOut[28].ToUpper();
            EncryptRound30.RightOut = data.RightOut[29].ToUpper();
            EncryptRound31.RightOut = data.RightOut[30].ToUpper();
            EncryptRound32.RightOut = data.RightOut[31].ToUpper();
        }

        /// <summary>
        /// Visualizes the complete data in the ui for decryption
        /// </summary>
        /// <param name="data"></param>
        public void VisualizeCompleteDataDecryption(MagmaCompleteData data)
        {
            EncryptGrid.Visibility = System.Windows.Visibility.Hidden;
            DecryptGrid.Visibility = System.Windows.Visibility.Visible;

            //set all round keys
            DecryptRound1.RoundKey = data.RoundKey[0].ToUpper();
            DecryptRound2.RoundKey = data.RoundKey[1].ToUpper();
            DecryptRound3.RoundKey = data.RoundKey[2].ToUpper();
            DecryptRound4.RoundKey = data.RoundKey[3].ToUpper();
            DecryptRound5.RoundKey = data.RoundKey[4].ToUpper();
            DecryptRound6.RoundKey = data.RoundKey[5].ToUpper();
            DecryptRound7.RoundKey = data.RoundKey[6].ToUpper();
            DecryptRound8.RoundKey = data.RoundKey[7].ToUpper();
            DecryptRound9.RoundKey = data.RoundKey[8].ToUpper();
            DecryptRound10.RoundKey = data.RoundKey[9].ToUpper();
            DecryptRound11.RoundKey = data.RoundKey[10].ToUpper();
            DecryptRound12.RoundKey = data.RoundKey[11].ToUpper();
            DecryptRound13.RoundKey = data.RoundKey[12].ToUpper();
            DecryptRound14.RoundKey = data.RoundKey[13].ToUpper();
            DecryptRound15.RoundKey = data.RoundKey[14].ToUpper();
            DecryptRound16.RoundKey = data.RoundKey[15].ToUpper();
            DecryptRound17.RoundKey = data.RoundKey[16].ToUpper();
            DecryptRound18.RoundKey = data.RoundKey[17].ToUpper();
            DecryptRound19.RoundKey = data.RoundKey[18].ToUpper();
            DecryptRound20.RoundKey = data.RoundKey[19].ToUpper();
            DecryptRound21.RoundKey = data.RoundKey[20].ToUpper();
            DecryptRound22.RoundKey = data.RoundKey[21].ToUpper();
            DecryptRound23.RoundKey = data.RoundKey[22].ToUpper();
            DecryptRound24.RoundKey = data.RoundKey[23].ToUpper();
            DecryptRound25.RoundKey = data.RoundKey[24].ToUpper();
            DecryptRound26.RoundKey = data.RoundKey[25].ToUpper();
            DecryptRound27.RoundKey = data.RoundKey[26].ToUpper();
            DecryptRound28.RoundKey = data.RoundKey[27].ToUpper();
            DecryptRound29.RoundKey = data.RoundKey[28].ToUpper();
            DecryptRound30.RoundKey = data.RoundKey[29].ToUpper();
            DecryptRound31.RoundKey = data.RoundKey[30].ToUpper();
            DecryptRound32.RoundKey = data.RoundKey[31].ToUpper();

            //set all left in/out and right in/out
            DecryptRound1.LeftIn = data.LeftIn[0].ToUpper();
            DecryptRound2.LeftIn = data.LeftIn[1].ToUpper();
            DecryptRound3.LeftIn = data.LeftIn[2].ToUpper();
            DecryptRound4.LeftIn = data.LeftIn[3].ToUpper();
            DecryptRound5.LeftIn = data.LeftIn[4].ToUpper();
            DecryptRound6.LeftIn = data.LeftIn[5].ToUpper();
            DecryptRound7.LeftIn = data.LeftIn[6].ToUpper();
            DecryptRound8.LeftIn = data.LeftIn[7].ToUpper();
            DecryptRound9.LeftIn = data.LeftIn[8].ToUpper();
            DecryptRound10.LeftIn = data.LeftIn[9].ToUpper();
            DecryptRound11.LeftIn = data.LeftIn[10].ToUpper();
            DecryptRound12.LeftIn = data.LeftIn[11].ToUpper();
            DecryptRound13.LeftIn = data.LeftIn[12].ToUpper();
            DecryptRound14.LeftIn = data.LeftIn[13].ToUpper();
            DecryptRound15.LeftIn = data.LeftIn[14].ToUpper();
            DecryptRound16.LeftIn = data.LeftIn[15].ToUpper();
            DecryptRound17.LeftIn = data.LeftIn[16].ToUpper();
            DecryptRound18.LeftIn = data.LeftIn[17].ToUpper();
            DecryptRound19.LeftIn = data.LeftIn[18].ToUpper();
            DecryptRound20.LeftIn = data.LeftIn[19].ToUpper();
            DecryptRound21.LeftIn = data.LeftIn[20].ToUpper();
            DecryptRound22.LeftIn = data.LeftIn[21].ToUpper();
            DecryptRound23.LeftIn = data.LeftIn[22].ToUpper();
            DecryptRound24.LeftIn = data.LeftIn[23].ToUpper();
            DecryptRound25.LeftIn = data.LeftIn[24].ToUpper();
            DecryptRound26.LeftIn = data.LeftIn[25].ToUpper();
            DecryptRound27.LeftIn = data.LeftIn[26].ToUpper();
            DecryptRound28.LeftIn = data.LeftIn[27].ToUpper();
            DecryptRound29.LeftIn = data.LeftIn[28].ToUpper();
            DecryptRound30.LeftIn = data.LeftIn[29].ToUpper();
            DecryptRound31.LeftIn = data.LeftIn[30].ToUpper();
            DecryptRound32.LeftIn = data.LeftIn[31].ToUpper();

            DecryptRound1.LeftOut = data.LeftOut[0].ToUpper();
            DecryptRound2.LeftOut = data.LeftOut[1].ToUpper();
            DecryptRound3.LeftOut = data.LeftOut[2].ToUpper();
            DecryptRound4.LeftOut = data.LeftOut[3].ToUpper();
            DecryptRound5.LeftOut = data.LeftOut[4].ToUpper();
            DecryptRound6.LeftOut = data.LeftOut[5].ToUpper();
            DecryptRound7.LeftOut = data.LeftOut[6].ToUpper();
            DecryptRound8.LeftOut = data.LeftOut[7].ToUpper();
            DecryptRound9.LeftOut = data.LeftOut[8].ToUpper();
            DecryptRound10.LeftOut = data.LeftOut[9].ToUpper();
            DecryptRound11.LeftOut = data.LeftOut[10].ToUpper();
            DecryptRound12.LeftOut = data.LeftOut[11].ToUpper();
            DecryptRound13.LeftOut = data.LeftOut[12].ToUpper();
            DecryptRound14.LeftOut = data.LeftOut[13].ToUpper();
            DecryptRound15.LeftOut = data.LeftOut[14].ToUpper();
            DecryptRound16.LeftOut = data.LeftOut[15].ToUpper();
            DecryptRound17.LeftOut = data.LeftOut[16].ToUpper();
            DecryptRound18.LeftOut = data.LeftOut[17].ToUpper();
            DecryptRound19.LeftOut = data.LeftOut[18].ToUpper();
            DecryptRound20.LeftOut = data.LeftOut[19].ToUpper();
            DecryptRound21.LeftOut = data.LeftOut[20].ToUpper();
            DecryptRound22.LeftOut = data.LeftOut[21].ToUpper();
            DecryptRound23.LeftOut = data.LeftOut[22].ToUpper();
            DecryptRound24.LeftOut = data.LeftOut[23].ToUpper();
            DecryptRound25.LeftOut = data.LeftOut[24].ToUpper();
            DecryptRound26.LeftOut = data.LeftOut[25].ToUpper();
            DecryptRound27.LeftOut = data.LeftOut[26].ToUpper();
            DecryptRound28.LeftOut = data.LeftOut[27].ToUpper();
            DecryptRound29.LeftOut = data.LeftOut[28].ToUpper();
            DecryptRound30.LeftOut = data.LeftOut[29].ToUpper();
            DecryptRound31.LeftOut = data.LeftOut[30].ToUpper();
            DecryptRound32.LeftOut = data.LeftOut[31].ToUpper();

            DecryptRound1.RightIn = data.RightIn[0].ToUpper();
            DecryptRound2.RightIn = data.RightIn[1].ToUpper();
            DecryptRound3.RightIn = data.RightIn[2].ToUpper();
            DecryptRound4.RightIn = data.RightIn[3].ToUpper();
            DecryptRound5.RightIn = data.RightIn[4].ToUpper();
            DecryptRound6.RightIn = data.RightIn[5].ToUpper();
            DecryptRound7.RightIn = data.RightIn[6].ToUpper();
            DecryptRound8.RightIn = data.RightIn[7].ToUpper();
            DecryptRound9.RightIn = data.RightIn[8].ToUpper();
            DecryptRound10.RightIn = data.RightIn[9].ToUpper();
            DecryptRound11.RightIn = data.RightIn[10].ToUpper();
            DecryptRound12.RightIn = data.RightIn[11].ToUpper();
            DecryptRound13.RightIn = data.RightIn[12].ToUpper();
            DecryptRound14.RightIn = data.RightIn[13].ToUpper();
            DecryptRound15.RightIn = data.RightIn[14].ToUpper();
            DecryptRound16.RightIn = data.RightIn[15].ToUpper();
            DecryptRound17.RightIn = data.RightIn[16].ToUpper();
            DecryptRound18.RightIn = data.RightIn[17].ToUpper();
            DecryptRound19.RightIn = data.RightIn[18].ToUpper();
            DecryptRound20.RightIn = data.RightIn[19].ToUpper();
            DecryptRound21.RightIn = data.RightIn[20].ToUpper();
            DecryptRound22.RightIn = data.RightIn[21].ToUpper();
            DecryptRound23.RightIn = data.RightIn[22].ToUpper();
            DecryptRound24.RightIn = data.RightIn[23].ToUpper();
            DecryptRound25.RightIn = data.RightIn[24].ToUpper();
            DecryptRound26.RightIn = data.RightIn[25].ToUpper();
            DecryptRound27.RightIn = data.RightIn[26].ToUpper();
            DecryptRound28.RightIn = data.RightIn[27].ToUpper();
            DecryptRound29.RightIn = data.RightIn[28].ToUpper();
            DecryptRound30.RightIn = data.RightIn[29].ToUpper();
            DecryptRound31.RightIn = data.RightIn[30].ToUpper();
            DecryptRound32.RightIn = data.RightIn[31].ToUpper();

            DecryptRound1.RightOut = data.RightOut[0].ToUpper();
            DecryptRound2.RightOut = data.RightOut[1].ToUpper();
            DecryptRound3.RightOut = data.RightOut[2].ToUpper();
            DecryptRound4.RightOut = data.RightOut[3].ToUpper();
            DecryptRound5.RightOut = data.RightOut[4].ToUpper();
            DecryptRound6.RightOut = data.RightOut[5].ToUpper();
            DecryptRound7.RightOut = data.RightOut[6].ToUpper();
            DecryptRound8.RightOut = data.RightOut[7].ToUpper();
            DecryptRound9.RightOut = data.RightOut[8].ToUpper();
            DecryptRound10.RightOut = data.RightOut[9].ToUpper();
            DecryptRound11.RightOut = data.RightOut[10].ToUpper();
            DecryptRound12.RightOut = data.RightOut[11].ToUpper();
            DecryptRound13.RightOut = data.RightOut[12].ToUpper();
            DecryptRound14.RightOut = data.RightOut[13].ToUpper();
            DecryptRound15.RightOut = data.RightOut[14].ToUpper();
            DecryptRound16.RightOut = data.RightOut[15].ToUpper();
            DecryptRound17.RightOut = data.RightOut[16].ToUpper();
            DecryptRound18.RightOut = data.RightOut[17].ToUpper();
            DecryptRound19.RightOut = data.RightOut[18].ToUpper();
            DecryptRound20.RightOut = data.RightOut[19].ToUpper();
            DecryptRound21.RightOut = data.RightOut[20].ToUpper();
            DecryptRound22.RightOut = data.RightOut[21].ToUpper();
            DecryptRound23.RightOut = data.RightOut[22].ToUpper();
            DecryptRound24.RightOut = data.RightOut[23].ToUpper();
            DecryptRound25.RightOut = data.RightOut[24].ToUpper();
            DecryptRound26.RightOut = data.RightOut[25].ToUpper();
            DecryptRound27.RightOut = data.RightOut[26].ToUpper();
            DecryptRound28.RightOut = data.RightOut[27].ToUpper();
            DecryptRound29.RightOut = data.RightOut[28].ToUpper();
            DecryptRound30.RightOut = data.RightOut[29].ToUpper();
            DecryptRound31.RightOut = data.RightOut[30].ToUpper();
            DecryptRound32.RightOut = data.RightOut[31].ToUpper();
        }
    }
}
