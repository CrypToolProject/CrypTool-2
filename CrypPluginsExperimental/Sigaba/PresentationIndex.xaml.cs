using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sigaba
{
    /// <summary>
    /// Interaction logic for PresentationIndex.xaml
    /// </summary>
    public partial class PresentationIndex : UserControl
    {
        private GradientStop GradientStop1;
        private GradientStop GradientStop2;
        private GradientStop GradientStop3;

        public Boolean Reversed;

        public int Position;
        public int Index;

        public PresentationIndex(int type)
        {
            Index = type;

            InitializeComponent();

            LinearGradientBrush myBrush = new LinearGradientBrush();
            GradientStop1 = new GradientStop(Colors.Transparent, 0.0);
            GradientStop2 = new GradientStop(Colors.Black, 0.1);
            GradientStop3 = new GradientStop(Colors.Transparent, 0.2);

            myBrush.GradientStops.Add(GradientStop1);
            myBrush.GradientStops.Add(GradientStop2);
            myBrush.GradientStops.Add(GradientStop3);

            Rotor0815.OpacityMask = myBrush;

            for(int i=0;i<18;i++)
            {
                TextBlock tb = new TextBlock();
                tb.Text = (i+7)%10+ 10*type +"";
                tb.Height = 16;
                Rotor0815.Children.Add(tb);
            }

        }


        public void SetType(int t)
        {
            for (int i = 0; i < Rotor0815.Children.Count; i++)
            {
                TextBlock tb = (TextBlock) Rotor0815.Children[i];
                tb.Text = (i + 7) % 10 + 10 * (t) + "";
            }

            Index = t;
        }

        public void SetPosition(int c)
        {
            double height = Rotor0815.Children[0].RenderSize.Height;
            if(!Reversed)
            {
                Canvas.SetTop(Rotor0815, -(c * 16));
            }
            else
            {
                Canvas.SetTop(Rotor0815, (c * 16)+113);
            }

            GradientStop1.Offset = ((c + 0) / 18.0);
            GradientStop2.Offset = ((c + 3) / 18.0);
            GradientStop3.Offset = ((c + 7) / 18.0);

            Position = c;

        }

        public void Reverse(Boolean b)
        {

            if (this.Reversed == false && b)
            {
                Rotor0815.RenderTransform = new RotateTransform(180);
                Canvas.SetTop(Rotor0815, ((17 - (10 - Position )) * 16));
                Canvas.SetLeft(Rotor0815, 13);
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
