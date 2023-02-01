using System;
using System.Windows.Controls;
using System.Windows.Media;
using CrypTool.Plugins.RAPPOR.ViewModel;
using RAPPOR.Helper;
using RAPPOR.Helper.ArrayDrawer;

namespace RAPPOR.ViewModel
{

    public class HeatMapsViewModel : ObservableObject, IViewModelBase
    {
        public CrypTool.Plugins.RAPPOR.RAPPOR rappor;
        public ArrayDrawerHeatMaps arrayDrawerHeatMaps;
        public ArrayDrawer arrayDrawer;

        public HeatMapsViewModel(CrypTool.Plugins.RAPPOR.RAPPOR rAPPOR)
        {
            rappor = rAPPOR;

            arrayDrawerHeatMaps = new ArrayDrawerHeatMaps();
            Canvas canvas = new Canvas();
            HeatMapsCanvas = canvas;
            DrawCanvas();
            OnPropertyChanged("HeatMapsViewModel");
        }

        /// <summary>
        /// This method is used to draw the canvas of the heat map.
        /// </summary>
        public void DrawCanvas()
        {
            rappor.RunRappor();

            HeatMapsCanvas.Children.Clear();

            //Drawing the rectangles of the canvas. 
            HeatMapsCanvas.Children.Add(arrayDrawerHeatMaps.AddRectangle(170, 10, 50, 120, "#f2f2f2"));
            HeatMapsCanvas.Children.Add(arrayDrawerHeatMaps.AddRectangle(170, 140, 50, 120, "#f2f2f2"));
            HeatMapsCanvas.Children.Add(arrayDrawerHeatMaps.AddRectangle(170, 270, 50, 120, "#f2f2f2"));

            HeatMapsCanvas.Children.Add(arrayDrawerHeatMaps.AddRectangle(230, 10, 560, 120, "#f2f2f2"));
            HeatMapsCanvas.Children.Add(arrayDrawerHeatMaps.AddRectangle(230, 140, 560, 120, "#f2f2f2"));
            HeatMapsCanvas.Children.Add(arrayDrawerHeatMaps.AddRectangle(230, 270, 560, 120, "#f2f2f2"));

            //Drawing the text for the canvas
            HeatMapsCanvas.Children.Add(arrayDrawerHeatMaps.AddText(180, 70, 16,
                CrypTool.Plugins.RAPPOR.Properties.Resources.BF));
            HeatMapsCanvas.Children.Add(arrayDrawerHeatMaps.AddText(180, 190, 16,
                CrypTool.Plugins.RAPPOR.Properties.Resources.PRR));
            HeatMapsCanvas.Children.Add(arrayDrawerHeatMaps.AddText(180, 310, 16,
                CrypTool.Plugins.RAPPOR.Properties.Resources.IRR));

            HeatMapsCanvas.Children.Add(arrayDrawerHeatMaps.AddBloomFilter(235, 10, 545, 120, 5,
                rappor.GetBloomFilter().GetBoolArray(), true));
            HeatMapsCanvas.Children.Add(arrayDrawerHeatMaps.AddBloomFilter(235, 10, 545, 120, 5,
                rappor.GetBloomFilter().GetBoolArray(), false));

            int[] prrArray = new int[rappor.GetBloomFilter().GetBoolArray().Length];
            int[] irrArray = new int[rappor.GetBloomFilter().GetBoolArray().Length];
            for (int i = 0; i < rappor.GetBloomFilter().GetBoolArray().Length; i++)
            {
                prrArray[i] = 0;
                irrArray[i] = 0;
            }

            int f = rappor.GetRAPPORSettings().GetFVariable();
            int p = rappor.GetRAPPORSettings().GetPVariable();
            int q = rappor.GetRAPPORSettings().GetQVariable();
            bool[][] b = new bool[rappor.GetRAPPORSettings().GetIterations()][];

            for (int i = 0; i < rappor.GetRAPPORSettings().GetIterations(); i++)
            {
                b[i] = new bool[rappor.GetPermanentRandomizedResponse().GetBoolArray().Length];
                for (int j = 0; j < rappor.GetPermanentRandomizedResponse().GetBoolArray().Length; j++)
                {
                    double x = rappor.GetRandom().NextDouble() * 100;

                    if (x < (f / 2))
                    {
                        b[i][j] = false;
                    }
                    else if (x < f)
                    {
                        prrArray[j]++;
                        b[i][j] = true;
                    }
                    else
                    {
                        if (rappor.GetBloomFilter().GetBoolArray()[j])
                        {
                            prrArray[j]++;
                            b[i][j] = true;
                        }
                    }
                }
            }

            for (int i = 0; i < rappor.GetRAPPORSettings().GetIterations(); i++)
            {
                for (int j = 0; j < rappor.GetPermanentRandomizedResponse().GetBoolArray().Length; j++)
                {
                    double x = rappor.GetRandom().NextDouble() * 100;

                    if (b[i][j])
                    {
                        if (x < q)
                        {
                            irrArray[j]++;
                        }
                    }
                    else
                    {
                        if (x < p)
                        {
                            irrArray[j]++;
                        }

                    }

                }
            }

            double minPRR = rappor.GetRAPPORSettings().GetIterations();
            double maxPRR = 0;
            double minIRR = rappor.GetRAPPORSettings().GetIterations();
            double maxIRR = 0;
            double maxTotal = 0;
            double minTotal = rappor.GetRAPPORSettings().GetIterations();
            for (int i = 0; i < prrArray.Length; i++)
            {
                if (minPRR > prrArray[i])
                {
                    minPRR = prrArray[i];
                }

                if (maxPRR < prrArray[i])
                {
                    maxPRR = prrArray[i];
                }

                if (minIRR > irrArray[i])
                {
                    minIRR = irrArray[i];
                }

                if (maxIRR > irrArray[i])
                {
                    maxIRR = irrArray[i];
                }
            }

            if (maxPRR > maxIRR)
            {
                maxTotal = maxPRR;
            }
            else
            {
                maxTotal = maxIRR;
            }

            if (minPRR < minIRR)
            {
                minTotal = minPRR;
            }
            else
            {
                minTotal = minIRR;
            }

            double minTotalDouble = minTotal;
            double maxTotalDouble = maxTotal;
            double minPercentageDouble =
                Math.Truncate((minTotalDouble / rappor.GetRAPPORSettings().GetIterations()));
            string minPercentageString = string.Format("{0:N2}%", minPercentageDouble);
            double maxPercentageDouble =
                Math.Truncate((maxTotalDouble / rappor.GetRAPPORSettings().GetIterations()));
            string maxPercentageString = string.Format("{0:N2}%", maxPercentageDouble);
            double midPercentageDouble = Math.Truncate((minPercentageDouble + maxPercentageDouble) / 2);
            string midPercentageString = string.Format("{0:N2}%", midPercentageDouble);

            GradientStopCollection gradCol = new GradientStopCollection(6)
            {
                new GradientStop(Colors.Black, 0),
                new GradientStop(Colors.Blue, .1666),
                new GradientStop(Colors.Green, .3333),
                new GradientStop(Colors.Yellow, .5),
                new GradientStop(Colors.Orange, .6666),
                new GradientStop(Colors.OrangeRed, .8333),
                //This is rather simular to (Color)new BrushConverter().ConvertFrom("#F01F2B")
                new GradientStop(Colors.Red, 1)
            };
            for (int i = 0; i < rappor.GetBloomFilter().GetBoolArray().Length; i++)
            {
                HeatMapsCanvas.Children.Add(arrayDrawerHeatMaps.AddRandomizedResponseStroke(235, 200, 545, 60, 5,
                    rappor.GetRAPPORSettings().GetIterations(), i, prrArray[i], prrArray.Length, gradCol,
                    rappor.GetRAPPORSettings().GetIterations(), 0));
                HeatMapsCanvas.Children.Add(arrayDrawerHeatMaps.AddRandomizedResponseStroke(235, 330, 545, 60, 5,
                    rappor.GetRAPPORSettings().GetIterations(), i, irrArray[i], irrArray.Length, gradCol,
                    rappor.GetRAPPORSettings().GetIterations(), 0));
            }

            HeatMapsCanvas.Children.Add(arrayDrawerHeatMaps.HeatMapLegend(60, 10, 100, 380));

            HeatMapsCanvas.Children.Add(arrayDrawerHeatMaps.HeatMapLegendText(10, 10, 380,
                "0" + "/" + rappor.GetRAPPORSettings().GetIterations(),
                (rappor.GetRAPPORSettings().GetIterations() / 2).ToString() + "/" +
                rappor.GetRAPPORSettings().GetIterations(),
                rappor.GetRAPPORSettings().GetIterations() + "/" + rappor.GetRAPPORSettings().GetIterations()));
            OnPropertyChanged("HeatMapsCanvas");

        }


        //The canvas containing the heatmaps view
        private Canvas _heatMapsCanvas;

        /// <summary>
        /// Getter and setter of the heat maps canvas.
        /// </summary>
        public Canvas HeatMapsCanvas
        {
            get
            {
                if (_heatMapsCanvas == null)
                {
                    return null;
                }

                return _heatMapsCanvas;
            }
            set
            {
                _heatMapsCanvas = value;
                OnPropertyChanged("HeatMapsCanvas");
            }
        }

        public void CreateHeatMapViewText(int it)
        {
            HeatMapViewText =  CrypTool.Plugins.RAPPOR.Properties.Resources.HeatMapViewText1 + it + CrypTool.Plugins.RAPPOR.Properties.Resources.HeatMapViewText2 + it + CrypTool.Plugins.RAPPOR.Properties.Resources.HeatMapViewText3 + it/2 + CrypTool.Plugins.RAPPOR.Properties.Resources.HeatMapViewText4;
        }

        private string _heatMapViewText;

        /// <summary>
        /// Getter and setter of the heat maps canvas.
        /// </summary>
        public string HeatMapViewText
        {
            get
            {
                if (_heatMapViewText == null)
                {
                    return null;
                }

                return _heatMapViewText;
            }
            set
            {
                _heatMapViewText = value;
                OnPropertyChanged("HeatMapViewText");
            }
        }

        public new void ChangeButton(Boolean ru)
        {
        }
    }


}
