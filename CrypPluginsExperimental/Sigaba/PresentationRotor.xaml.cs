using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sigaba
{
    /// <summary>
    /// Interaction logic for PresentationRotor.xaml
    /// </summary>
    public partial class PresentationRotor : UserControl
    {
        private GradientStop GradientStop1;
        private GradientStop GradientStop2;
        private GradientStop GradientStop3;

        public Boolean Reversed = false;
        public char Position = 'A';

        public int Index;

        public PresentationRotor()
        {
            InitializeComponent();
            
            var myBrush = new LinearGradientBrush();
            GradientStop1 = new GradientStop(Colors.Transparent, 0.0);
            GradientStop2 = new GradientStop(Colors.Black, 0.1);
            GradientStop3 = new GradientStop(Colors.Transparent, 0.2);
            
            myBrush.GradientStops.Add(GradientStop1);
            myBrush.GradientStops.Add(GradientStop2);
            myBrush.GradientStops.Add(GradientStop3);

            Rotor0815.OpacityMask = myBrush;
        }

        public void SetPosition(char c)
        {
            double height = Rotor0815.Children[0].RenderSize.Height;

            if(!Reversed)
            {
                Canvas.SetTop(Rotor0815,-( ( c-65 )  * 16 ));
            }
            else
            {
                Canvas.SetTop(Rotor0815, ((c - 65) * 16) + 113);
            } 
            GradientStop1.Offset = ((c - 65 + 0) / 33.0);
            GradientStop2.Offset = ((c - 65 + 3) / 33.0) ;
            GradientStop3.Offset = (( c -65 + 7) / 33.0) ;
            Position = c;
                                                                                       
        }

        public void Reverse(Boolean b)
        {
            if(this.Reversed == false && b)
            {
                Rotor0815.RenderTransform = new RotateTransform(180);
                Canvas.SetTop(Rotor0815, ((33 - (26-(Position - 65))) * 16));
                Canvas.SetLeft(Rotor0815, 12);
            }

            if (this.Reversed == true && !b)
            {
                Rotor0815.RenderTransform = new RotateTransform(0);
                SetPosition(Position);
                Canvas.SetLeft(Rotor0815, 0);
            }

            this.Reversed = b;
            

        }
    }
}
