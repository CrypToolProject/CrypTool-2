using System.Windows.Controls;
using System.Windows.Shapes;

namespace Sigaba
{
    /// <summary>
    /// Interaction logic for ORing2.xaml
    /// </summary>
    public partial class ORing2 : UserControl
    {   
        private int[] _oring;

        public ORing2(int[] oring)
        {
            _oring = oring;
            InitializeComponent();
            Line[] lineArray = new Line[10];
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
                lineArray[i].X1 = (_oring[i]) * 33 - i * 33 + 16 + 87;
            }



        }
    }
}
