/*
   Copyright 2022 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using System.Data;
using System.Windows.Controls;

namespace CrypTool.JosseCipher
{
    /// <summary>
    /// Interaction logic for JosseCipherPresentation.xaml
    /// </summary>
    public partial class JosseCipherPresentation : UserControl
    {
        public JosseCipherPresentation()
        {
            InitializeComponent();
            RectangleTableViewBox.Child = ShowPlayText();
            CharacterMappingViewBox.Child = ShowPlayText();
            RectangleTableTab.Header = Properties.Resources.Rectangle;
            CharacterMappingTableTap.Header = Properties.Resources.CharacterMapping;
        }

        private Label ShowPlayText()
        {
            Label label = new Label { Content = Properties.Resources.ShowPlayText };
            return label;
        }

        public void BuildRectangleTable(DataTable dataTable)
        {
            RectangleTable.DataContext = dataTable.DefaultView;
            RectangleTable.CanUserAddRows = false;
            RectangleTable.CanUserDeleteRows = false;
            RectangleTable.CanUserReorderColumns = false;
            RectangleTable.CanUserResizeColumns = false;
            RectangleTable.CanUserResizeRows = false;
            RectangleTable.CanUserSortColumns = false;
            RectangleTable.IsReadOnly = true;
            RectangleTableViewBox.Child = RectangleTable;
        }

        public void BuildCharacterMappingTable(DataTable dataTable)
        {
            CharacterMappingTable.DataContext = dataTable.DefaultView;
            CharacterMappingTable.CanUserAddRows = false;
            CharacterMappingTable.CanUserDeleteRows = false;
            CharacterMappingTable.CanUserReorderColumns = false;
            CharacterMappingTable.CanUserResizeColumns = false;
            CharacterMappingTable.CanUserResizeRows = false;
            CharacterMappingTable.CanUserSortColumns = false;
            CharacterMappingTable.IsReadOnly = true;
            CharacterMappingViewBox.Child = CharacterMappingTable;
        }
    }
}