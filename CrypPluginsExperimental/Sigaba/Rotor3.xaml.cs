using System;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Sigaba
{
    /// <summary>
    /// Interaction logic for Rotor3.xaml
    /// </summary>
    public partial class Rotor3 : UserControl
    {

        public int Type;

        private Rotor _rotor;
        private Boolean Ciph;

        public Rotor3(Rotor rotor,Boolean ciph)
        {
            Ciph = ciph;
            _rotor = rotor;
            InitializeComponent();
            Line[] lineArray = new Line[26];
            for(int i=0;i<LineCanvas.Children.Count;i++)
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
                   /* if(Ciph)
                        lineArray[i].Y1 = (_rotor.Ciph(i))*33 - i*33 + 16;
                    else*/
                        lineArray[i].Y1 = (_rotor.DeCiph(i)) * 33 - i * 33 + 16;
                }
            
            

        }

    }
}
