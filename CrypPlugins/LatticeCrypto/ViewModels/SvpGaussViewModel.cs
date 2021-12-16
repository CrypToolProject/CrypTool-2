using CrypTool.PluginBase.Miscellaneous;
using LatticeCrypto.Models;
using LatticeCrypto.Properties;
using LatticeCrypto.Utilities;
using LatticeCrypto.Utilities.Arrows;
using LatticeCrypto.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace LatticeCrypto.ViewModels
{
    public class SvpGaussViewModel : BaseViewModel
    {
        public LatticeND Lattice { get; set; }
        public Canvas canvas;
        public RichTextBox History { get; set; }
        public BigInteger? TargetVectorX { get; set; }
        public BigInteger? TargetVectorY { get; set; }
        public ReductionMethods ReductionMethod { get; set; }
        protected Point zeroPoint;
        protected ArrowLine x_line;
        protected ArrowLine y_line;
        protected double scalingFactorX;
        protected double scalingFactorY;
        protected ArrowLine closestVectorArrow;
        protected Ellipse closestVector;
        protected List<Polygon> listPolygons = new List<Polygon>();
        private readonly List<LatticePoint> listLatticePoints = new List<LatticePoint>();
        private readonly List<Ellipse> listLatticeBasisPoints = new List<Ellipse>();
        private readonly List<Ellipse> listLatticeReducedPoints = new List<Ellipse>();
        private readonly List<ArrowLine> listAxes = new List<ArrowLine>();
        private readonly List<TextBlock> listAxesTextBlocks = new List<TextBlock>();
        private readonly List<ArrowLine> listVectors = new List<ArrowLine>();
        private readonly List<Ellipse> listSpheres = new List<Ellipse>();
        private Ellipse hermiteCircle;
        private BigInteger maxVectorValueX;
        private BigInteger maxVectorValueY;
        private Ellipse selectedPoint;
        private int selectedPointTag;
        private double maxPixelsPerPoint;
        private Transform lastTransform = new TranslateTransform(0, 0);
        private static readonly double hermiteConstant = 2 / Math.Sqrt(3);

        protected double pixelsPerPoint = 20;
        public double PixelsPerPoint
        {
            get => pixelsPerPoint;
            set
            {
                pixelsPerPoint = value > 0.5 ? value : 0.5;

                NotifyPropertyChanged("PixelsPerPoint");
                ZoomInCommand.RaiseCanExecuteChanged();
                ZoomOutCommand.RaiseCanExecuteChanged();
            }
        }

        private bool isBlinking;
        public bool IsBlinking
        {
            get => isBlinking;
            set
            {
                isBlinking = value;
                RefreshBlinking();
            }
        }

        public SvpGaussViewModel()
        {
            ReductionMethod = ReductionMethods.reduceGauss;
        }

        public void GenerateNewLattice(int n, int m, BigInteger codomainStart, BigInteger codomainEnd)
        {
            UiServices.SetBusyState();

            LatticeND newLattice = new LatticeND(n, m, false);

            //Zur Generierung von kritischen Gittern
            //while (Math.Round(newLattice.AngleReducedVectors, 0) != 60)
            //{
            newLattice.GenerateRandomVectors(ReductionMethod == ReductionMethods.reduceGauss, codomainStart, codomainEnd);

            switch (ReductionMethod)
            {
                case ReductionMethods.reduceGauss:
                    newLattice.GaussianReduce();
                    break;
                default:
                    newLattice.LLLReduce();
                    break;
            }
            //}
            Lattice = newLattice;

            WriteHistoryForNewLattice(Languages.buttonGenerateNewLattice);

            NotifyPropertyChanged("Lattice");
        }

        public void SetInitialNDLattice()
        {
            SetLatticeManually(new LatticeND(2, 2, false) { Vectors = new[] { new VectorND(new BigInteger[] { 27, 21 }), new VectorND(new BigInteger[] { 19, 4 }) } });
        }

        public void WriteHistoryForNewLattice(string firstLine)
        {
            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + firstLine + " **\r\n"))));

            paragraph.Inlines.Add(new Bold(new Run(Languages.labelLatticeBasis + ":")));
            paragraph.Inlines.Add(" " + Lattice.LatticeToString() + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run("   " + Languages.labelLengthBasisVectors)));
            paragraph.Inlines.Add(" " + Lattice.VectorLengthToString() + "\r\n");

            if (Lattice.N == 2 && Lattice.M == 2)
            {
                paragraph.Inlines.Add(new Bold(new Run("   " + Languages.labelAngleBasisVectors)));
                paragraph.Inlines.Add(" " + Util.FormatDoubleLog(Lattice.AngleBasisVectors) + "\r\n");

                paragraph.Inlines.Add(new Bold(new Run("   " + Languages.labelEstimationR)));
                paragraph.Inlines.Add(" " + Util.FormatDoubleLog(Math.Sqrt(hermiteConstant * (double)Lattice.Determinant)) + "\r\n");
            }

            paragraph.Inlines.Add(new Bold(new Run(Languages.labelReducedLatticeBasis + ":")));
            paragraph.Inlines.Add(" " + Lattice.LatticeReducedToString() + "\r\n");

            List<string> reductionSteps = Lattice.LatticeReductionStepsToString();
            for (int i = 1; i <= reductionSteps.Count; i++)
            {
                paragraph.Inlines.Add("    " + string.Format(Languages.labelReductionStep, i, reductionSteps.Count) + " " + reductionSteps[i - 1] + "\r\n");
            }

            paragraph.Inlines.Add(new Bold(new Run("   " + Languages.labelSuccessiveMinima)));
            paragraph.Inlines.Add(" " + Lattice.VectorReducedLengthToString() + "\r\n");

            if (Lattice.N == 2 && Lattice.M == 2)
            {
                paragraph.Inlines.Add(new Bold(new Run("   " + Languages.labelAngleReducedVectors)));
                paragraph.Inlines.Add(" " + Util.FormatDoubleLog(Lattice.AngleReducedVectors) + "\r\n");
                paragraph.Inlines.Add(new Bold(new Run("   " + Languages.labelDensity)));
                paragraph.Inlines.Add(" " + Util.FormatDoubleToPercentageLog(Lattice.Density) + " / " + Util.FormatDoubleToPercentageLog(Lattice.DensityRelToOptimum) + "\r\n");
            }

            paragraph.Inlines.Add(new Bold(new Run("   " + Languages.labelMinimalVector)));
            paragraph.Inlines.Add(" " + Lattice.GetMinimalReducedVector() + "\r\n");

            paragraph.Inlines.Add(new Bold(new Run(Languages.labelUnimodularTransformationMatrix)));
            paragraph.Inlines.Add(" " + Lattice.LatticeTransformationToString() + "\r\n");

            if (Lattice.N == Lattice.M)
            {
                paragraph.Inlines.Add(new Bold(new Run(Languages.labelDeterminant)));
                paragraph.Inlines.Add(" " + Lattice.Determinant + "\r\n");
            }

            if (History.Document.Blocks.FirstBlock != null)
            {
                History.Document.Blocks.InsertBefore(History.Document.Blocks.FirstBlock, paragraph);
            }
            else
            {
                History.Document.Blocks.Add(paragraph);
            }
        }

        public void SetLatticeManually(LatticeND newLattice)
        {
            UiServices.SetBusyState();

            Lattice = new LatticeND(newLattice.Vectors, newLattice.UseRowVectors);

            switch (ReductionMethod)
            {
                case ReductionMethods.reduceGauss:
                    Lattice.GaussianReduce();
                    break;
                default:
                    Lattice.LLLReduce();
                    break;
            }

            WriteHistoryForNewLattice(Languages.buttonDefineNewLattice);

            NotifyPropertyChanged("Lattice");
        }

        private RelayCommand saveToClipboardCommand;
        public RelayCommand SaveToClipboardCommand
        {
            get
            {
                if (saveToClipboardCommand != null)
                {
                    return saveToClipboardCommand;
                }

                saveToClipboardCommand = new RelayCommand(
                    parameter1 =>
                    {
                        LatticeCopyOrSaveSelection selectionView = new LatticeCopyOrSaveSelection();
                        if (selectionView.ShowDialog() == false)
                        {
                            return;
                        }

                        string latticeInfos;
                        switch (selectionView.selection)
                        {
                            default:
                                latticeInfos = Lattice.LatticeToString();
                                break;
                            case 1:
                                latticeInfos = Lattice.LatticeReducedToString();
                                break;
                            case 2:
                                latticeInfos = "";
                                List<string> latticeInfosList = Lattice.GetAllLatticeInfosAsStringList();

                                for (int i = 0; i < latticeInfosList.Count; i++)
                                {
                                    latticeInfos += latticeInfosList[i];
                                    if (i != latticeInfosList.Count - 1)
                                    {
                                        latticeInfos += Environment.NewLine;
                                    }
                                }

                                break;
                        }

                        Clipboard.SetText(latticeInfos);
                    });
                return saveToClipboardCommand;
            }
        }

        private RelayCommand saveToFileCommand;
        public RelayCommand SaveToFileCommand
        {
            get
            {
                if (saveToFileCommand != null)
                {
                    return saveToFileCommand;
                }

                saveToFileCommand = new RelayCommand(
                    parameter1 =>
                    {
                        LatticeCopyOrSaveSelection selectionView = new LatticeCopyOrSaveSelection();
                        if (selectionView.ShowDialog() == false)
                        {
                            return;
                        }

                        SaveFileDialog saveFileDialog = new SaveFileDialog { Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*" };
                        if (saveFileDialog.ShowDialog() == false)
                        {
                            return;
                        }

                        try
                        {
                            string[] latticeInfos;
                            switch (selectionView.selection)
                            {
                                default:
                                    latticeInfos = new[] { Lattice.LatticeToString() };
                                    break;
                                case 1:
                                    latticeInfos = new[] { Lattice.LatticeReducedToString() };
                                    break;
                                case 2:
                                    latticeInfos = Lattice.GetAllLatticeInfosAsStringList().ToArray();
                                    break;
                            }

                            File.WriteAllLines(saveFileDialog.FileName, latticeInfos);
                        }
                        catch (IOException)
                        {
                            MessageBox.Show(Languages.errorSavingFile, Languages.error, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    });
                return saveToFileCommand;
            }
        }

        private RelayCommand zoomInCommand;
        public RelayCommand ZoomInCommand
        {
            get
            {
                if (zoomInCommand != null)
                {
                    return zoomInCommand;
                }

                zoomInCommand = new RelayCommand(
                    parameter1 =>
                    {
                        PixelsPerPoint = PixelsPerPoint * 1.2;
                        GenerateLatticePoints(false, false);
                        if (this is CvpViewModel)
                        {
                            ((CvpViewModel)this).FindClosestVector(false);
                        }

                        UpdateCanvas();
                    },
                    parameter2 => PixelsPerPoint <= maxPixelsPerPoint);
                return zoomInCommand;
            }
        }

        private RelayCommand zoomOutCommand;
        public RelayCommand ZoomOutCommand
        {
            get
            {
                if (zoomOutCommand != null)
                {
                    return zoomOutCommand;
                }

                zoomOutCommand = new RelayCommand(
                    parameter1 =>
                    {
                        PixelsPerPoint = PixelsPerPoint / 1.2;
                        GenerateLatticePoints(false, false);
                        if (this is CvpViewModel)
                        {
                            ((CvpViewModel)this).FindClosestVector(false);
                        }

                        UpdateCanvas();
                    },
                    parameter2 => PixelsPerPoint > 0.1);
                return zoomOutCommand;
            }
        }

        public bool IsPointBasisLatticeVector(Point point)
        {
            selectedPoint = null;
            selectedPointTag = 0;
            VisualTreeHelper.HitTest(canvas, null, HitTestCallback, new PointHitTestParameters(point));
            return selectedPoint != null;
        }

        public HitTestResultBehavior HitTestCallback(HitTestResult htrResult)
        {
            if (!(htrResult.VisualHit is Ellipse) || !listLatticeBasisPoints.Contains((Ellipse)htrResult.VisualHit))
            {
                return HitTestResultBehavior.Continue;
            }

            selectedPoint = (Ellipse)htrResult.VisualHit;
            selectedPointTag = int.Parse(((Ellipse)htrResult.VisualHit).Tag.ToString());
            return HitTestResultBehavior.Stop;
        }

        public void RefreshBlinking()
        {
            if (IsBlinking)
            {
                BeginnLatticeBasisPointsBlink();
            }
            else
            {
                StopLatticeBasisPointsBlink();
            }
        }

        public void BeginnLatticeBasisPointsBlink()
        {
            if (listLatticeBasisPoints == null)
            {
                return;
            }

            foreach (Ellipse basisPoint in listLatticeBasisPoints)
            {
                basisPoint.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0.75, 1, new Duration(new TimeSpan(0, 0, 0, 0, 600))) { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever });
                basisPoint.BeginAnimation(FrameworkElement.WidthProperty, new DoubleAnimation(10 + PixelsPerPoint / 5, 10 + PixelsPerPoint / 1.5, new Duration(new TimeSpan(0, 0, 0, 0, 600))) { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever });
                basisPoint.BeginAnimation(FrameworkElement.HeightProperty, new DoubleAnimation(10 + PixelsPerPoint / 5, 10 + PixelsPerPoint / 1.5, new Duration(new TimeSpan(0, 0, 0, 0, 600))) { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever });
            }
        }

        public void StopLatticeBasisPointsBlink()
        {
            if (listLatticeBasisPoints == null)
            {
                return;
            }

            foreach (Ellipse basisPoint in listLatticeBasisPoints)
            {
                basisPoint.BeginAnimation(UIElement.OpacityProperty, null);
                basisPoint.BeginAnimation(FrameworkElement.WidthProperty, null);
                basisPoint.BeginAnimation(FrameworkElement.HeightProperty, null);
            }
        }


        public void ChangeVectorToSelectedPoint(Point point)
        {
            BigInteger tempVectorX = new BigInteger(Math.Round((point.X - y_line.X1) / PixelsPerPoint / scalingFactorX));
            BigInteger tempVectorY = new BigInteger(Math.Round((x_line.Y1 - point.Y) / PixelsPerPoint / scalingFactorY));
            VectorND tempNewVector = new VectorND(new[] { tempVectorX, tempVectorY });

            //Bevor geändert werden kann, muss auf lineare Unabhängigkeit geprüft werden
            LatticeND tempLattice = selectedPointTag == 1 ? new LatticeND(new[] { tempNewVector, Lattice.Vectors[1] }, false) : new LatticeND(new[] { Lattice.Vectors[0], tempNewVector }, false);
            if (tempLattice.Determinant == 0)
            {
                return;
            }

            Lattice = tempLattice;
            Lattice.GaussianReduce();

            NotifyPropertyChanged("Lattice");
            AddCoordinateLinesAndBasisVectorsToCanvas();
            UpdateCanvas();
        }


        public void CalculatePixelsPerPoint()
        {
            VectorND[] vectors = this is CvpViewModel ? Lattice.ReducedVectors : Lattice.Vectors;

            maxVectorValueX = BigInteger.Max(BigInteger.Abs(vectors[0].values[0]), BigInteger.Abs(vectors[1].values[0]));
            maxVectorValueY = BigInteger.Max(BigInteger.Abs(vectors[0].values[1]), BigInteger.Abs(vectors[1].values[1]));

            if (canvas.ActualHeight == 0 || canvas.ActualWidth == 0)
            {
                return;
            }

            //Der Divisor 262/9 ist nicht fest hergeleitet. Ein "normalgroßes" Gitter soll den scalingFactor ~= 1 bekommen
            const double normalizationDiv = 262 / 9;
            scalingFactorX = canvas.ActualWidth / (double)maxVectorValueX / normalizationDiv;
            scalingFactorY = canvas.ActualHeight / (double)maxVectorValueY / normalizationDiv;

            maxPixelsPerPoint = Math.Max(canvas.ActualWidth / scalingFactorX / (double)maxVectorValueX, canvas.ActualHeight / scalingFactorY / (double)maxVectorValueY);
            PixelsPerPoint = maxPixelsPerPoint / 4;

            if (!Settings.Default.useSameScalingForBothAxes)
            {
                return;
            }

            double scalingFactor = Math.Min(scalingFactorX, scalingFactorY);
            scalingFactorX = scalingFactor;
            scalingFactorY = scalingFactor;
        }

        public void AddBasisVectorsAndReducedVectors()
        {
            double transX = canvas.RenderTransform.Value.OffsetX;
            double transY = canvas.RenderTransform.Value.OffsetY;

            for (int i = 0; i <= 1; i++)
            {
                VectorND[] vectors = i == 0 ? Lattice.Vectors : Lattice.ReducedVectors;

                for (int j = 0; j <= 1; j++)
                {
                    for (int k = 0; k <= 1; k++)
                    {
                        if (j == k)
                        {
                            continue;
                        }

                        BigInteger logicX = j * vectors[0].values[0] + k * vectors[1].values[0];
                        BigInteger logicY = j * vectors[0].values[1] + k * vectors[1].values[1];

                        double x = y_line.X1 + (double)logicX * PixelsPerPoint * scalingFactorX;
                        double y = x_line.Y1 + (double)logicY * PixelsPerPoint * scalingFactorY;

                        //Nur so viele Gitterpunkte zeichnen wie ins Bild passen
                        if (x < -transX || y < transY || x > canvas.ActualWidth - transX || y > canvas.ActualHeight + transY)
                        {
                            continue;
                        }

                        string logicXString = Util.FormatBigInt(logicX);
                        string logicYString = Util.FormatBigInt(logicY);

                        Ellipse ellipse = new Ellipse
                        {
                            Width = 10 + PixelsPerPoint / 5,
                            Height = 10 + PixelsPerPoint / 5,
                            Fill = Brushes.Black,
                            ToolTip = "X = " + logicXString + ", Y = " + logicYString
                        };

                        Canvas.SetLeft(ellipse, x - ellipse.Width / 2);
                        Canvas.SetBottom(ellipse, y - ellipse.Height / 2);

                        if (i == 0 && !(this is CvpViewModel))
                        {
                            if (j == 1 && k == 0)
                            {
                                ellipse.Tag = 1;
                                listLatticeBasisPoints.Add(ellipse);
                            }
                            else if (j == 0 && k == 1)
                            {
                                ellipse.Tag = 2;
                                listLatticeBasisPoints.Add(ellipse);
                            }
                        }
                        else if (i == 1)
                        {
                            listLatticeReducedPoints.Add(ellipse);
                        }
                    }
                }
            }
        }

        public void GenerateLatticePoints(bool clearCurrentList, bool updateCanvas)
        {
            listLatticeBasisPoints.Clear();
            listLatticeReducedPoints.Clear();
            listPolygons.Clear();
            listSpheres.Clear();
            closestVector = null;
            closestVectorArrow = null;
            hermiteCircle = null;

            AddCoordinateLinesAndBasisVectorsToCanvas();
            AddBasisVectorsAndReducedVectors();
            if (!(this is CvpViewModel) && Settings.Default.showHermiteCircle)
            {
                AddHermiteCircle();
            }

            //Verschiebungen der Graphik
            double transX = canvas.RenderTransform.Value.OffsetX;
            double transY = canvas.RenderTransform.Value.OffsetY;

            //Sicherheitszuschläge für die Ränder, damit wirklich alle(!) Parallelogramme angezeigt werden
            double secureX = (double)BigInteger.Max(BigInteger.Abs(Lattice.ReducedVectors[0].values[0]), BigInteger.Abs(Lattice.ReducedVectors[1].values[0])) * PixelsPerPoint * scalingFactorX;
            double secureY = (double)BigInteger.Max(BigInteger.Abs(Lattice.ReducedVectors[0].values[1]), BigInteger.Abs(Lattice.ReducedVectors[1].values[1])) * PixelsPerPoint * scalingFactorY;

            //Temporäre Liste, entweder mit den letzten Gitterpunkten oder initial mit dem Nullpunkt
            List<LatticePoint> tempLatticePoints = new List<LatticePoint>();
            if (listLatticePoints.Count == 0 || clearCurrentList)
            {
                tempLatticePoints.Add(new LatticePoint(0, 0));
            }
            else
            {
                tempLatticePoints.AddRange(listLatticePoints);
            }

            listLatticePoints.Clear();

            bool tooManyPolygons = false;
            bool tooManyPoints = false;

            while (tempLatticePoints.Count > 0 && !tooManyPoints)
            {
                LatticePoint currentPoint = tempLatticePoints[0];
                tempLatticePoints.Remove(currentPoint);

                if (listLatticePoints.Exists(z => z.logicX == currentPoint.logicX && z.logicY == currentPoint.logicY))
                {
                    continue;
                }

                double x = y_line.X1 + (double)currentPoint.logicX * PixelsPerPoint * scalingFactorX;
                double y = x_line.Y1 + (double)currentPoint.logicY * PixelsPerPoint * scalingFactorY;

                //Nur so viele Gitterpunkte zeichnen wie ins Bild passen
                if (x < -transX - secureX || y < transY - secureY || x > canvas.ActualWidth - transX + secureX || y > canvas.ActualHeight + transY + secureY)
                {
                    continue;
                }

                //Neue Ellipse erzeugen
                string logicXString = Util.FormatBigInt(currentPoint.logicX);
                string logicYString = Util.FormatBigInt(currentPoint.logicY);

                Ellipse ellipse = new Ellipse
                {
                    Width = 10 + PixelsPerPoint / 5,
                    Height = 10 + PixelsPerPoint / 5,
                    Fill = Brushes.Black,
                    ToolTip = "X = " + logicXString + ", Y = " + logicYString
                };

                Canvas.SetLeft(ellipse, x - ellipse.Width / 2);
                Canvas.SetBottom(ellipse, y - ellipse.Height / 2);

                currentPoint.ellipse = ellipse;
                listLatticePoints.Add(currentPoint);

                if (listLatticePoints.Count == Settings.Default.maxCountLatticePoints)
                {
                    tooManyPoints = true;
                    listLatticePoints.Clear();
                    listSpheres.Clear();
                    continue;
                }

                //Aus dem aktuellen Gitterpunkt neue generieren
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (i == 0 && j == 0)
                        {
                            continue;
                        }

                        BigInteger logicX = currentPoint.logicX + i * Lattice.ReducedVectors[0].values[0] + j * Lattice.ReducedVectors[1].values[0];
                        BigInteger logicY = currentPoint.logicY + i * Lattice.ReducedVectors[0].values[1] + j * Lattice.ReducedVectors[1].values[1];

                        tempLatticePoints.Add(new LatticePoint(logicX, logicY));
                    }
                }

                //Einen Kreis um den Gitterpunkt für die Gitterdichte
                if (!(this is CvpViewModel) && Settings.Default.showSpherePacking)
                {
                    double width = Lattice.ReducedVectors[0].Length * pixelsPerPoint * scalingFactorX;
                    double height = Lattice.ReducedVectors[0].Length * pixelsPerPoint * scalingFactorY;

                    Ellipse sphere = new Ellipse
                    {
                        Width = width,
                        Height = height,
                        Stroke = Brushes.Magenta,
                        StrokeThickness = 3,
                    };

                    Canvas.SetLeft(sphere, x - sphere.Width / 2);
                    Canvas.SetBottom(sphere, y - sphere.Height / 2);
                    listSpheres.Add(sphere);
                }

                //Polygone generieren
                if (tooManyPolygons)
                {
                    continue;
                }

                y = canvas.ActualHeight - y;
                double scaledVector1X = (double)Lattice.ReducedVectors[0].values[0] * pixelsPerPoint * scalingFactorX;
                double scaledVector1Y = (double)Lattice.ReducedVectors[0].values[1] * pixelsPerPoint * scalingFactorY;
                double scaledVector2X = (double)Lattice.ReducedVectors[1].values[0] * pixelsPerPoint * scalingFactorX;
                double scaledVector2Y = (double)Lattice.ReducedVectors[1].values[1] * pixelsPerPoint * scalingFactorY;

                //Die Parallelepipede müssen in zwei entgegengesetzte Richtungen erzeugt werden. Die anderen beiden Richtungen sind im Prinzip nicht nötig (?!)
                Polygon parallelepiped1 = new Polygon { Stroke = Brushes.DarkBlue, StrokeThickness = 2 };
                parallelepiped1.Points.Add(new Point(x, y));
                parallelepiped1.Points.Add(new Point(x + scaledVector1X, y - scaledVector1Y));
                parallelepiped1.Points.Add(new Point(x + scaledVector1X + scaledVector2X, y - scaledVector1Y - scaledVector2Y));
                parallelepiped1.Points.Add(new Point(x + scaledVector2X, y - scaledVector2Y));

                Polygon parallelepiped2 = new Polygon { Stroke = Brushes.DarkBlue, StrokeThickness = 2 };
                parallelepiped2.Points.Add(new Point(x, y));
                parallelepiped2.Points.Add(new Point(x - scaledVector1X, y + scaledVector1Y));
                parallelepiped2.Points.Add(new Point(x - scaledVector1X - scaledVector2X, y + scaledVector1Y + scaledVector2Y));
                parallelepiped2.Points.Add(new Point(x - scaledVector2X, y + scaledVector2Y));

                //Polygon parallelepiped3 = new Polygon { Stroke = Brushes.DarkBlue, StrokeThickness = 2 };
                //parallelepiped3.Points.Add(new Point(x, y));
                //parallelepiped3.Points.Add(new Point(x + scaledVector1X, y - scaledVector1Y));
                //parallelepiped3.Points.Add(new Point(x + (scaledVector1X - scaledVector2X), y + (scaledVector2Y - scaledVector1Y)));
                //parallelepiped3.Points.Add(new Point(x - scaledVector2X, y + scaledVector2Y));

                //Polygon parallelepiped4 = new Polygon { Stroke = Brushes.DarkBlue, StrokeThickness = 2 };
                //parallelepiped4.Points.Add(new Point(x, y));
                //parallelepiped4.Points.Add(new Point(x + scaledVector2X, y - scaledVector2Y));
                //parallelepiped4.Points.Add(new Point(x + (scaledVector2X - scaledVector1X), y + (scaledVector1Y - scaledVector2Y)));
                //parallelepiped4.Points.Add(new Point(x - scaledVector1X, y + scaledVector1Y));

                if (!listPolygons.Contains(parallelepiped1))
                {
                    listPolygons.Add(parallelepiped1);
                }

                if (!listPolygons.Contains(parallelepiped2))
                {
                    listPolygons.Add(parallelepiped2);
                }
                //if (!listPolygons.Contains(parallelepiped3))
                //    listPolygons.Add(parallelepiped3);
                //if (!listPolygons.Contains(parallelepiped4))
                //    listPolygons.Add(parallelepiped4);

                //Prüfen, ob schon zuviele Polygone existieren.
                //Da die Polygone auch über den Rand hinausragen können, wird noch ein Aufschlag von 1/2 gegeben
                if (listPolygons.Count <= Settings.Default.maxCountPolygons * 1.5)
                {
                    continue;
                }

                tooManyPolygons = true;
                listPolygons.Clear();
            }

            if (updateCanvas)
            {
                UpdateCanvas();
            }

            RefreshBlinking();
        }

        public void UpdateCanvas()
        {
            canvas.Children.Clear();

            //Zuerst das Koordinatensystem
            foreach (ArrowLine element in listAxes)
            {
                canvas.Children.Add(element);
            }

            foreach (TextBlock element in listAxesTextBlocks)
            {
                canvas.Children.Add(element);
            }
            //Dann die Parallelogramme
            foreach (Polygon element in listPolygons)
            {
                canvas.Children.Add(element);
            }

            //Parallelogramme, die sich nicht innerhalb der Canvas befinden, können wieder gelöscht werden
            Rect canvasRect = new Rect(canvas.RenderSize);
            for (int i = 0; i < canvas.Children.Count; i++)
            {
                UIElement child = canvas.Children[i];
                Polygon polygon = child as Polygon;
                if (polygon == null)
                {
                    continue;
                }

                Rect polygonRect = polygon.RenderedGeometry.Bounds;
                bool intersects = canvasRect.IntersectsWith(polygonRect);
                if (intersects)
                {
                    continue;
                }

                canvas.Children.Remove(child);
                listPolygons.Remove(polygon);
            }

            //Für Masterarbeit
            //canvas.Children.Add(listPolygons[0]);
            //canvas.Children.Add(listPolygons[1]);

            //Dann die Vectoren
            foreach (ArrowLine element in listVectors)
            {
                canvas.Children.Add(element);
            }
            //Dann den Hermite Kreis
            if (hermiteCircle != null)
            {
                canvas.Children.Add(hermiteCircle);
            }
            //Dann die Kreise
            foreach (Ellipse element in listSpheres)
            {
                canvas.Children.Add(element);
            }
            //Dann die Ellipsen
            foreach (LatticePoint element in listLatticePoints)
            {
                canvas.Children.Add(element.ellipse);
            }

            foreach (Ellipse element in listLatticeBasisPoints)
            {
                canvas.Children.Add(element);
            }

            foreach (Ellipse element in listLatticeReducedPoints)
            {
                canvas.Children.Add(element);
            }
            //Und evtl. den Closest Vector
            if (closestVector != null && closestVectorArrow != null)
            {
                canvas.Children.Add(closestVector);
                canvas.Children.Add(closestVectorArrow);
            }
            ZoomOutCommand.RaiseCanExecuteChanged();
        }

        public void AddHermiteCircle()
        {
            double width = Math.Sqrt(hermiteConstant * (double)Lattice.Determinant) * PixelsPerPoint * scalingFactorX * 2;
            double height = Math.Sqrt(hermiteConstant * (double)Lattice.Determinant) * PixelsPerPoint * scalingFactorY * 2;

            hermiteCircle = new Ellipse
            {
                Width = width,
                Height = height,
                Stroke = Brushes.Purple,
                StrokeThickness = 3,
                ToolTip = Languages.tooltipHermiteUpperBound
            };

            Canvas.SetLeft(hermiteCircle, y_line.X1 - hermiteCircle.Width / 2);
            Canvas.SetBottom(hermiteCircle, x_line.Y1 - hermiteCircle.Height / 2);
        }

        public void AddCoordinateLinesAndBasisVectorsToCanvas()
        {
            listAxes.Clear();
            listVectors.Clear();
            listAxesTextBlocks.Clear();

            //Verschiebungen der Graphik
            double transX = canvas.RenderTransform.Value.OffsetX;
            double transY = canvas.RenderTransform.Value.OffsetY;

            zeroPoint = new Point(canvas.ActualWidth / 2, canvas.ActualHeight / 2);

            x_line = new ArrowLine
            {
                X1 = -transX,
                X2 = canvas.ActualWidth - transX,
                Y1 = zeroPoint.Y,
                Y2 = zeroPoint.Y,
                Stroke = Brushes.Black,
            };

            y_line = new ArrowLine
            {
                X1 = zeroPoint.X,
                X2 = zeroPoint.X,
                Y1 = canvas.ActualHeight - transY,
                Y2 = -transY,
                Stroke = Brushes.Black,
            };

            //Ceiling statt Round?
            int max10baseX = (int)Math.Round(BigInteger.Log10(maxVectorValueX * 2));
            int max10baseY = (int)Math.Round(BigInteger.Log10(maxVectorValueY * 2));

            //Wenn die beiden Max-Werte nicht zu sehr voneinander abweichen, sollen sie gleich gemacht werden
            if (Math.Abs(max10baseX - max10baseY) == 1)
            {
                int max = Math.Max(max10baseX, max10baseY);
                max10baseX = max;
                max10baseY = max;
            }

            BigInteger intervalX = ComputeInterval(max10baseX);
            BigInteger intervalY = ComputeInterval(max10baseY);

            double scalingStepX = pixelsPerPoint * scalingFactorX * (double)intervalX;
            double scalingStepY = pixelsPerPoint * scalingFactorY * (double)intervalY;

            BigInteger currentStep;

            if (scalingStepX > 5)
            {
                //Zahlen an der X-Achse
                currentStep = 0;
                for (double x = zeroPoint.X - scalingStepX; x > -transX; x -= scalingStepX)
                {
                    currentStep -= intervalX;
                    string text = Util.FormatBigInt(currentStep);
                    TextBlock textBlock = new SelectableTextBlock { Text = text.PadLeft(4) };
                    Canvas.SetLeft(textBlock, x + PixelsPerPoint / 3.0 - 20);
                    Canvas.SetBottom(textBlock, zeroPoint.Y - 15);
                    listAxesTextBlocks.Add(textBlock);
                }
                currentStep = 0;
                for (double x = zeroPoint.X + scalingStepX; x < canvas.ActualWidth - transX; x += scalingStepX)
                {
                    currentStep += intervalX;
                    string text = Util.FormatBigInt(currentStep);
                    TextBlock textBlock = new SelectableTextBlock { Text = text.PadLeft(4) };
                    Canvas.SetLeft(textBlock, x + PixelsPerPoint / 3.0 - 20);
                    Canvas.SetBottom(textBlock, zeroPoint.Y - 15);
                    listAxesTextBlocks.Add(textBlock);
                }
            }

            if (scalingStepY > 5)
            {
                //Zahlen an der Y-Achse
                currentStep = 0;
                for (double y = canvas.ActualHeight / 2 - scalingStepY; y > transY; y -= scalingStepY)
                {
                    currentStep -= intervalY;
                    string text = Util.FormatBigInt(currentStep);
                    TextBlock textBlock = new SelectableTextBlock { Text = text.PadLeft(4) };
                    Canvas.SetBottom(textBlock, y - PixelsPerPoint / 3.0);
                    Canvas.SetLeft(textBlock, zeroPoint.X - 25);
                    listAxesTextBlocks.Add(textBlock);
                }
                currentStep = 0;
                for (double y = canvas.ActualHeight / 2 + scalingStepY; y < canvas.ActualHeight + transY; y += scalingStepY)
                {
                    currentStep += intervalY;
                    string text = Util.FormatBigInt(currentStep);
                    TextBlock textBlock = new SelectableTextBlock { Text = text.PadLeft(4) };
                    Canvas.SetBottom(textBlock, y - PixelsPerPoint / 3.0);
                    Canvas.SetLeft(textBlock, zeroPoint.X - 25);
                    listAxesTextBlocks.Add(textBlock);
                }
            }

            listAxes.Add(x_line);
            listAxes.Add(y_line);

            if (!(this is CvpViewModel))
            {
                ArrowLine basisVector1Line = new ArrowLine
                {

                    X1 = y_line.X1,
                    X2 = y_line.X1 + (double)Lattice.Vectors[0].values[0] * pixelsPerPoint * scalingFactorX,
                    Y1 = x_line.Y1,
                    Y2 = x_line.Y1 - (double)Lattice.Vectors[0].values[1] * pixelsPerPoint * scalingFactorY,
                    Stroke = Brushes.Crimson,
                    StrokeThickness = 8,
                    ArrowAngle = 65,
                    ArrowLength = 25 + pixelsPerPoint / 10
                };

                ArrowLine basisVector2Line = new ArrowLine
                {

                    X1 = y_line.X1,
                    X2 = y_line.X1 + (double)Lattice.Vectors[1].values[0] * pixelsPerPoint * scalingFactorX,
                    Y1 = x_line.Y1,
                    Y2 = x_line.Y1 - (double)Lattice.Vectors[1].values[1] * pixelsPerPoint * scalingFactorY,
                    Stroke = Brushes.Crimson,
                    StrokeThickness = 8,
                    ArrowAngle = 65,
                    ArrowLength = 25 + pixelsPerPoint / 10
                };
                listVectors.Add(basisVector1Line);
                listVectors.Add(basisVector2Line);
            }

            ArrowLine reducedBasisVector1Line = new ArrowLine
            {

                X1 = y_line.X1,
                X2 = y_line.X1 + (double)Lattice.ReducedVectors[0].values[0] * pixelsPerPoint * scalingFactorX,
                Y1 = x_line.Y1,
                Y2 = x_line.Y1 - (double)Lattice.ReducedVectors[0].values[1] * pixelsPerPoint * scalingFactorY,
                Stroke = Brushes.Green,
                StrokeThickness = 8,
                ArrowAngle = 65,
                ArrowLength = 25 + pixelsPerPoint / 10
            };

            ArrowLine reducedBasisVector2Line = new ArrowLine
            {

                X1 = y_line.X1,
                X2 = y_line.X1 + (double)Lattice.ReducedVectors[1].values[0] * pixelsPerPoint * scalingFactorX,
                Y1 = x_line.Y1,
                Y2 = x_line.Y1 - (double)Lattice.ReducedVectors[1].values[1] * pixelsPerPoint * scalingFactorY,
                Stroke = Brushes.Green,
                StrokeThickness = 8,
                ArrowAngle = 65,
                ArrowLength = 25 + pixelsPerPoint / 10
            };

            listVectors.Add(reducedBasisVector1Line);
            listVectors.Add(reducedBasisVector2Line);
        }

        public BigInteger ComputeInterval(int max10base)
        {
            /*
             * Folgende Verbesserung wäre möglich:
             * ->Zoomen anhand der aktuellen Zehnerpotenz, currentZoomX und currentZoomY
             * ->Anfangen mit maxValueX und maxValueY (nächste Zehnerpotenz oder auch Dezimalzahlen zulassen, vermutlich besser)
             * ->PixelsPerPoint nun abhängig von currentZoomX und currentZoomY
             * ->Zur Berechnung der Skalen: Breite/Höhe teilen durch Durchschnittliche Länge der aktuellen Zehnerpotenz 
             *  und dann schauen, ob 10, 4, 2 oder 1 passt
             */

            BigInteger interval = BigInteger.Pow(10, max10base);

            double adjustFactor = 1;
            if (max10base > 3)
            {
                adjustFactor = 0.75;
            }
            else if (max10base > 5)
            {
                adjustFactor = 0.7;
            }
            else if (max10base > 7)
            {
                adjustFactor = 0.65;
            }

            if (pixelsPerPoint * adjustFactor > 25)
            {
                interval /= 20;
            }
            else if (pixelsPerPoint * adjustFactor > 12)
            {
                interval /= 10;
            }
            else if (pixelsPerPoint * adjustFactor > 6)
            {
                interval /= 4;
            }
            else if (pixelsPerPoint * adjustFactor > 3)
            {
                interval /= 2;
            }
            else if (pixelsPerPoint * adjustFactor < 1)
            {
                interval *= 4;
            }
            else if (pixelsPerPoint * adjustFactor < 1.3)
            {
                interval *= 2;
            }

            if (interval == 0)
            {
                interval = 1;
            }

            return interval;
        }

        public void ResetCanvasPosition()
        {
            canvas.RenderTransform = new TranslateTransform(0, 0);
            lastTransform = canvas.RenderTransform;
        }

        public void SetCanvasPosition(Point lastPoint, Point currentPoint)
        {
            double x = lastPoint.X - lastTransform.Value.OffsetX - currentPoint.X;
            double y = lastPoint.Y - lastTransform.Value.OffsetY - currentPoint.Y;
            canvas.RenderTransform = new TranslateTransform(-x, -y);
        }

        public void SetCanvasTransform(Transform transform)
        {
            lastTransform = transform;
        }
    }
}
