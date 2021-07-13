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
            ReplacementTableViewBox.Child = ShowPlayText();
            CharacterMappingTableTap.Content = new Viewbox
            {
                Child = ShowPlayText()
            };
            ReplacementTableTab.Header = Properties.Resources.ReplacementTableTab;
            CharacterMappingTableTap.Header = Properties.Resources.CharacterMappingTableTap;
        }

        private static Label ShowPlayText()
        {
            var label = new Label {Content = Properties.Resources.ShowPlayText };
            return label;
        }

        public void BuildReplacementTable(DataTable tableContent)
        {
            ReplacementTable.DataContext = tableContent.DefaultView;
            ReplacementTable.CanUserAddRows = false;
            ReplacementTable.CanUserDeleteRows = false;
            ReplacementTable.CanUserReorderColumns = false;
            ReplacementTable.CanUserResizeColumns = false;
            ReplacementTable.CanUserResizeRows = false;
            ReplacementTable.CanUserSortColumns = false;
            ReplacementTable.IsReadOnly = true;
            ReplacementTableViewBox.Child = ReplacementTable;
        }

        public void BuildCharacterMappingTable(DataTable tableContent)
        {
            CharacterMapping.DataContext = tableContent.DefaultView;
            CharacterMapping.CanUserAddRows = false;
            CharacterMapping.CanUserDeleteRows = false;
            CharacterMapping.CanUserReorderColumns = false;
            CharacterMapping.CanUserResizeColumns = false;
            CharacterMapping.CanUserResizeRows = false;
            CharacterMapping.CanUserSortColumns = false;
            CharacterMapping.IsReadOnly = true;
            CharacterMappingTableTap.Content = new ScrollViewer
            {
                Content = CharacterMapping
            };
        }
    }
}
