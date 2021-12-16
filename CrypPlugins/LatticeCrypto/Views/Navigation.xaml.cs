using LatticeCrypto.Utilities;
using System.Windows;

namespace LatticeCrypto.Views
{
    /// <summary>
    /// Interaktionslogik für Navigation.xaml
    /// </summary>
    public partial class Navigation
    {
        public event Navigate OnNavigate;
        public Navigation()
        {
            InitializeComponent();
        }

        private void link_Click(object sender, RoutedEventArgs e)
        {
            if (null == OnNavigate)
            {
                return;
            }

            NavigationCommandType commandtype = NavigationCommandType.None;

            if (Equals(sender, link_Start))
            {
                commandtype = NavigationCommandType.Start;
            }
            else if (Equals(sender, link_Gauss))
            {
                commandtype = NavigationCommandType.Gauss;
            }
            else if (Equals(sender, link_LLL))
            {
                commandtype = NavigationCommandType.LLL;
            }
            else if (Equals(sender, link_CVP))
            {
                commandtype = NavigationCommandType.CVP;
            }
            else if (Equals(sender, link_MerkleHellman))
            {
                commandtype = NavigationCommandType.MerkleHellman;
            }
            else if (Equals(sender, link_RSA))
            {
                commandtype = NavigationCommandType.RSA;
            }
            else if (Equals(sender, link_GGH))
            {
                commandtype = NavigationCommandType.GGH;
            }
            else if (Equals(sender, link_LWE))
            {
                commandtype = NavigationCommandType.LWE;
            }

            OnNavigate(commandtype);
        }
    }
}
