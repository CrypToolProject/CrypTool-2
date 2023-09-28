using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using CrypTool.Plugins.RAPPOR.ViewModel;
using RAPPOR.Helper;
using RAPPOR.Helper.ArrayDrawer;

namespace RAPPOR.ViewModel
{
    /// <summary>
    /// This class is used to create the view of the randomized response of the rr view tab. 
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.Plugins.RAPPOR.Properties.Resources")]
    public class RandomizedResponsesViewModel : ObservableObject, IViewModelBase
    {
        /// <summary>
        /// The different array drawers which are being utilized.
        /// </summary>
        private readonly ArrayDrawerRR arrayDrawerRR;
        private readonly ArrayDrawerHeatMaps arrayDrawerHM;
        /// <summary>
        /// Instance of the current rappor class.
        /// </summary>
        public CrypTool.Plugins.RAPPOR.RAPPOR _rappor;

        /// <summary>
        /// Sets up the current RandomizedResponseViewModel. 
        /// </summary>
        /// <param name="rappor">The current rappor instance</param>
        public RandomizedResponsesViewModel(CrypTool.Plugins.RAPPOR.RAPPOR rappor)
        {
            _rappor = rappor;
            arrayDrawerRR = new ArrayDrawerRR();
            arrayDrawerHM = new ArrayDrawerHeatMaps();
            RandomizedResponsesCanvas = new Canvas();
            DrawCanvas();
            OnPropertyChanged("RandomizedResponsesCanvas");
        }
        /// <summary>
        /// This class is used to create the randomized response view model of the component.
        /// </summary>
        public void DrawCanvas()
        {
            RandomizedResponsesCanvas.Children.Clear();
            _rappor.RunRappor();
            //Drawing boxes
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddRectangle(10, 10, 180, 185, "#F2F2F2"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddRectangle(10, 205, 180, 185, "#F2F2F2"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddRectangle(200, 10, 400, 380, "#FFFFFF"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddRectangle(610, 100, 180, 140, "#F2F2F2"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddRectangle(610, 250, 180, 140, "#F2F2F2"));

            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddRectangle(610, 10, 85, 35, "#F2F2F2"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddRectangle(705, 10, 85, 35, "#F2F2F2"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddRectangle(610, 55, 85, 35, "#F2F2F2"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddRectangle(705, 55, 85, 35, "#F2F2F2"));

            //Text for the variables
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(615, 15, 17, "h: " + _rappor.GetRAPPORSettings().GetAmountOfHashFunctions(), "#000000"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(710, 15, 17, "f: " + _rappor.GetRAPPORSettings().GetFVariable() + " %", "#000000"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(615, 60, 17, "q: " + _rappor.GetRAPPORSettings().GetQVariable() + " %", "#000000"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(710, 60, 17, "p: " + _rappor.GetRAPPORSettings().GetPVariable() + " %", "#000000"));


            //Drawing divider lines
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddStrokedLine(200, 137, 600, 137, 2, "#808080"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddStrokedLine(200, 274, 600, 274, 2, "#808080"));

            //Drawing Tree
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddLine(400, 50, 320, 182));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddLine(400, 50, 440, 227));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddLine(400, 50, 520, 227));

            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddLine(320, 182, 280, 227));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddLine(320, 182, 360, 227));

            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddLine(280, 227, 245, 350));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddLine(280, 227, 290, 350));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddLine(360, 227, 335, 350));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddLine(360, 227, 380, 350));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddLine(440, 227, 425, 350));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddLine(440, 227, 470, 350));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddLine(520, 227, 515, 350));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddLine(520, 227, 560, 350));

            int radius = 3;
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddCircle(400, 50, radius, "#808080"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddCircle(320, 182, radius, "#808080"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddCircle(440, 227, radius, "#808080"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddCircle(520, 227, radius, "#808080"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddCircle(280, 227, radius, "#808080"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddCircle(360, 227, radius, "#808080"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddCircle(245, 350, radius, "#808080"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddCircle(290, 350, radius, "#808080"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddCircle(335, 350, radius, "#808080"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddCircle(380, 350, radius, "#808080"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddCircle(425, 350, radius, "#808080"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddCircle(470, 350, radius, "#808080"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddCircle(515, 350, radius, "#808080"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddCircle(560, 350, radius, "#808080"));

            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(202, 115, 20, "B"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(202, 252, 20, "B'"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(202, 368, 20, "S"));


            //Adding text to tree //Right side
            //20230122 Increased size by 50%
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(403, 38, 15, "B", "#000000"));//400,50//400
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(414, 44, 6, "i", "#000000"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(362, 215, 15, "B", "#000000"));//360,227//365
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(373, 221, 6, "i", "#000000"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(377, 215, 15, "= 1", "#000000"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(445, 215, 15, "0", "#000000"));//440,227//445
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(525, 215, 15, "1", "#000000"));//520,227

            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(295, 338, 15, "1", "#000000"));//290,350
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(383, 338, 15, "B", "#000000"));//380,350//380
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(394, 344, 6, "i", "#000000"));//393
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(475, 338, 15, "1", "#000000"));//470,350
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(565, 338, 15, "1", "#000000"));//560,350

            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(425, 138, 15, "f / 2", "#000000"));//420,139
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(475, 138, 15, "f / 2", "#000000"));//470,139
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(290, 287, 15, "p", "#000000"));//285,289
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(375, 287, 15, "q", "#000000"));//370,289
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(460, 287, 15, "p", "#000000"));//455,289
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(545, 287, 15, "q", "#000000"));//540,289

            //Left side
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(300, 170, 15, "B", "#000000"));//320,182//305
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(311, 176, 6, "i", "#000000"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(259, 215, 15, "=0", "#000000"));//280,227//260
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(256, 221, 6, "i", "#000000"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(245, 215, 15, "B", "#000000"));//248
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(237, 344, 6, "i", "#000000"));//245,350
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(226, 338, 15, "B", "#000000"));//229
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(324, 338, 15, "0", "#000000"));//335,350//325
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(414, 338, 15, "0", "#000000"));//425,350//415
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(504, 338, 15, "0", "#000000"));//515,350//505

            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(313, 138, 15, "1 - f", "#000000"));//360,116//320
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(231, 287, 15, "1 - p", "#000000"));//263,289//233
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(316, 287, 15, "1 - q", "#000000"));//348,289//318
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(401, 287, 15, "1 - p", "#000000"));//433,289//403
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(486, 287, 15, "1 - q", "#000000"));//518,289//488

            //Adding Images
            //Better quality with this perhaps: https://stackoverflow.com/questions/87753/resizing-an-image-without-losing-any-quality
            Image prrImage = new Image
            {
                Source = new BitmapImage(new Uri("..\\Images\\PermanentRandomizedResponse.png", UriKind.Relative)),
                Width = 166
            };//10, 10, 180, 185//901,288
            Canvas.SetLeft(prrImage, 17);
            Canvas.SetTop(prrImage, 41);

            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddRectangle(15, 36, 170, 53 + 4 + 4, "#FFFFFF"));
            RandomizedResponsesCanvas.Children.Add(prrImage);
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(15, 16, 16, "B \u2192 B'"));

            Image irrImage = new Image
            {
                Source = new BitmapImage(new Uri("..\\Images\\InstantaneousRandomizedResponse.png", UriKind.Relative)),
                Width = 166
            };//10, 103, 180, 92,//865,247
            Canvas.SetLeft(irrImage, 17);
            Canvas.SetTop(irrImage, 139);

            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddRectangle(15, 134, 170, 47 + 4 + 4, "#FFFFFF"));
            RandomizedResponsesCanvas.Children.Add(irrImage);
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(15, 114, 16, "B' \u2192 S"));

            int y = 90;
            Image eInfImage = new Image
            {
                Source = new BitmapImage(new Uri("..\\Images\\epsilonInfinity.png", UriKind.Relative)),
                Width = 166
            }; //610, 10, 180, 185,//237,105
            Canvas.SetLeft(eInfImage, 617);//image placement x
            Canvas.SetTop(eInfImage, 41 + y);//image placement y

            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddRectangle(615, 36 + y, 170, 74 + 4 + 4, "#FFFFFF"));//image rectangle
            RandomizedResponsesCanvas.Children.Add(eInfImage);
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(615, 16 + y, 14, CrypTool.Plugins.RAPPOR.Properties.Resources.DifferentialPrivacyLevel + " " + "\u03B5")); //top text
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(765, 22 + y, 10, " \u221E")); //top text infinity symbol #20230122 Added blanks after epsilon and after infinity

            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(615, 123 + y, 20, "\u03B5   =  " + string.Format("{0:0.##########}", CalculateEpsilonInfinity(_rappor.GetRAPPORSettings().GetAmountOfHashFunctions(), (double)_rappor.GetRAPPORSettings().GetFVariable() / 100))));//bottom calc
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(623, 130 + y, 14, " \u221E "));//bottom epsilon

            y = y - 45;

            Image epsilonOne = new Image
            {
                Source = new BitmapImage(new Uri("..\\Images\\epsilonOne.png", UriKind.Relative)),
                Width = 166
            };//610, 205, 180, 185,//262,105
            Canvas.SetLeft(epsilonOne, 617);//image placement x
            Canvas.SetTop(epsilonOne, 232 + y);//image placement y

            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddRectangle(615, 230 + y, 170, 67 + 4, "#FFFFFF"));//image rectangle
            RandomizedResponsesCanvas.Children.Add(epsilonOne);
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(615, 212 + y, 14, CrypTool.Plugins.RAPPOR.Properties.Resources.DifferentialPrivacyLevel + " \u03B5\u2081"));//top text
            //RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(615, 306, 20, "\u03B5\u2081: " + calculateEpsilonOne((double)rappor.GetRAPPORSettings().GetAmountOfHashFunctions(), (double)rappor.GetRAPPORSettings().GetFVariable() / 100, (double)rappor.GetRAPPORSettings().GetQVariable() / 100, (double)rappor.GetRAPPORSettings().GetPVariable() / 100)));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(615, 306 + y, 20, "\u03B5\u2081 = " + string.Format("{0:0.##########}", CalculateEpsilonOne(_rappor.GetRAPPORSettings().GetAmountOfHashFunctions(), (double)_rappor.GetRAPPORSettings().GetFVariable() / 100, (double)_rappor.GetRAPPORSettings().GetQVariable() / 100, (double)_rappor.GetRAPPORSettings().GetPVariable() / 100)))); //bottom text

            //10, 205, 180, 185,
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(15, 210, 14, CrypTool.Plugins.RAPPOR.Properties.Resources.qStarText));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(15, 272, 14, "q* = " + string.Format("{0:0.##########}", CalculateQStar((double)_rappor.GetRAPPORSettings().GetFVariable() / 100, (double)_rappor.GetRAPPORSettings().GetQVariable() / 100, (double)_rappor.GetRAPPORSettings().GetPVariable() / 100))));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(15, 300, 14, CrypTool.Plugins.RAPPOR.Properties.Resources.pStarText));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(15, 362, 14, "p* = " + string.Format("{0:0.##########}", CalculatePStar((double)_rappor.GetRAPPORSettings().GetFVariable() / 100, (double)_rappor.GetRAPPORSettings().GetQVariable() / 100, (double)_rappor.GetRAPPORSettings().GetPVariable() / 100))));

            OnPropertyChanged("RandomizedResponsesCanvas");
        }
        /// <summary>
        /// Probability of observing 1 given that the underlying Bloom filer bit was set is given by this formular.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="q"></param>
        /// <param name="p"></param>
        /// <returns>The q star value</returns>
        public double CalculateQStar(double f, double q, double p)
        {
            return 0.5 * f * (p + q) + (1 - f) * q;
        }
        /// <summary>
        /// Probability of observing 1 given that the underlying Bloom filer bit was not set is given by this formular.
        /// </summary>
        /// <param name="f">User tunable parameter f</param>
        /// <param name="q">User tunable parameter q</param>
        /// <param name="p">User tunable parameter p</param>
        /// <returns>The p star value</returns>
        public double CalculatePStar(double f, double q, double p)
        {
            return 0.5 * f * (p + q) + (1 - f) * p;
        }
        /// <summary>
        /// Provided differential privacy value against an attack who is able to collect all instantaneous randomized responses or the permanent randomized response.
        /// </summary>
        /// <param name="h">User tunable parameter h</param>
        /// <param name="f">User tunable parameter f</param>
        /// <returns>Epsilon infinity value</returns>
        public double CalculateEpsilonInfinity(double h, double f)
        {
            if (f == 0)
            {
                return double.NaN;
            }
            //Math.Log is log with base e.
            return 2 * h * Math.Log((1 - 0.5 * f) / (0.5 * f));
        }
        /// <summary>
        /// Provided differential privacy value against an attack who is able to collect one instantaneous randomized response.
        /// </summary>
        /// <param name="h">User tunable parameter  h</param>
        /// <param name="f">User tunable parameter  f</param>
        /// <param name="q">User tunable parameter  q</param>
        /// <param name="p">User tunable parameter  p</param>
        /// <returns>epsilon one value</returns>
        public double CalculateEpsilonOne(double h, double f, double q, double p)
        {
            double qStar = CalculateQStar(f, q, p);
            double pStar = CalculatePStar(f, q, p);
            return Math.Abs(h * Math.Log(qStar * (1 - pStar) / (pStar * (1 - qStar)), 2));
        }

        /// <summary>
        /// The randomized response canvas.
        /// </summary>
        private Canvas _randomizedResponsesCanvas;

        /// <summary>
        /// Getter and setter for the randomized response canvas
        /// </summary>
        public Canvas RandomizedResponsesCanvas
        {
            get
            {
                if (_randomizedResponsesCanvas == null)
                {
                    return null;
                }

                return _randomizedResponsesCanvas;
            }
            set
            {
                _randomizedResponsesCanvas = value;
                OnPropertyChanged("RandomizedResponsesCanvas");
            }
        }
        public new void ChangeButton(Boolean ru)
        {
        }
        public void CreateHeatMapViewText(int a)
        {
        }
    }
}
