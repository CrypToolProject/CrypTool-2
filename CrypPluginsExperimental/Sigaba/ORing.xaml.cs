using System.Windows.Controls;
using System.Windows.Shapes;

namespace Sigaba
{
    /// <summary>
    /// Interaction logic for ORing.xaml
    /// </summary>
    public partial class ORing : UserControl
    {
        private int[] _oring;

        public ORing(int[] oring)
        {
            _oring = oring;
            InitializeComponent();
            Line[] lineArray = new Line[26];
            for (int i = 0; i < LineCanvas.Children.Count; i++)
            {
                lineArray[i] = (Line)LineCanvas.Children[i];
            }
            Connections(lineArray);
        }

        private void Connections(Line[] lineArray)
        {

            for (int i = 0; i < lineArray.Length; i++)
            {
                //lineArray[i].Y1 = (SigabaConstants.ControlCipherRotors[Type][i]-65)*33 - i*33 + 16;
                //int j = (i+2)%26;

                lineArray[i].X1 = (_oring[i]) * 33 - i * 33 + 16 + 231;
            }



        }
    }
}
