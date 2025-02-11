﻿/*
   Copyright 2019 Christian Bender christian1.bender@student.uni-siegen.de

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

namespace DCAPathFinder.UI.Tutorial3
{
    /// <summary>
    /// Interaction logic for AttackKeyRound3.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("DCAPathFinder.Properties.Resources")]
    public partial class AttackKeyRound3 : UserControl
    {
        public event EventHandler<EventArgs> SelectionChanged;

        public AttackKeyRound3()
        {
            InitializeComponent();

            //round5
            SBox4Round5.AlreadyAttacked = true;
            SBox4Round5.IsClickable = false;

            SBox3Round5.AlreadyAttacked = true;
            SBox3Round5.IsClickable = false;

            SBox2Round5.AlreadyAttacked = true;
            SBox2Round5.IsClickable = false;

            SBox1Round5.AlreadyAttacked = true;
            SBox1Round5.IsClickable = false;

            //round4
            SBox4Round4.AlreadyAttacked = true;
            SBox4Round4.IsClickable = false;

            SBox3Round4.AlreadyAttacked = true;
            SBox3Round4.IsClickable = false;

            SBox2Round4.AlreadyAttacked = true;
            SBox2Round4.IsClickable = false;

            SBox1Round4.AlreadyAttacked = true;
            SBox1Round4.IsClickable = false;

            //round3
            SBox4Round3.SelectionChanged += SBoxSelectionChanged;
            SBox3Round3.SelectionChanged += SBoxSelectionChanged;
            SBox2Round3.SelectionChanged += SBoxSelectionChanged;
            SBox1Round3.SelectionChanged += SBoxSelectionChanged;

            //round2
            SBox4Round2.AlreadyAttacked = false;
            SBox4Round2.IsClickable = false;

            SBox3Round2.AlreadyAttacked = false;
            SBox3Round2.IsClickable = false;

            SBox2Round2.AlreadyAttacked = false;
            SBox2Round2.IsClickable = false;

            SBox1Round2.AlreadyAttacked = false;
            SBox1Round2.IsClickable = false;

            //round1
            SBox4Round1.AlreadyAttacked = false;
            SBox4Round1.IsClickable = false;

            SBox3Round1.AlreadyAttacked = false;
            SBox3Round1.IsClickable = false;

            SBox2Round1.AlreadyAttacked = false;
            SBox2Round1.IsClickable = false;

            SBox1Round1.AlreadyAttacked = false;
            SBox1Round1.IsClickable = false;

            //Round3
            scrollviewerCipher.ScrollToVerticalOffset(200);
        }

        /// <summary>
        /// Listener to handle change of selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SBoxSelectionChanged(object sender, EventArgs e)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged.Invoke(sender, e);
            }
        }
    }
}