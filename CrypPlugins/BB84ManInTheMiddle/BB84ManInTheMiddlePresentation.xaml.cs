using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace BB84ManInTheMiddle
{
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.Plugins.BB84ManInTheMiddle.Properties.Resources")]
    public partial class BB84ManInTheMiddlePresentation : UserControl
    {
        public bool listened;

        private System.Windows.Threading.DispatcherTimer frameTimer;
        private string receivedPhotonString;
        private string baseString;
        private string photonOutputString;
        public int animationRepeats;
        public int Progress;
        public bool hasFinished;
        public event EventHandler UpdateProgress;
        public double SpeedFactor;


        public BB84ManInTheMiddlePresentation()
        {
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
            resetRepeats();
            InitializeComponent();
            SizeChanged += sizeChanged;
            hideEverything();
            hasFinished = true;
        }

        private void setSpeed()
        {
            ((Storyboard)Resources["fadePlus"]).SpeedRatio = SpeedFactor;
            ((Storyboard)Resources["fadeCross"]).SpeedRatio = SpeedFactor;
            ((Storyboard)Resources["fadeLeftVertical"]).SpeedRatio = SpeedFactor;
            ((Storyboard)Resources["fadeLeftHorizontal"]).SpeedRatio = SpeedFactor;
            ((Storyboard)Resources["fadeLeftTopLeftDiagonal"]).SpeedRatio = SpeedFactor;
            ((Storyboard)Resources["fadeLeftTopRightDiagonal"]).SpeedRatio = SpeedFactor;
            ((Storyboard)Resources["movementLeft"]).SpeedRatio = 0.6 * SpeedFactor;
            ((Storyboard)Resources["movementRight"]).SpeedRatio = 0.6 * SpeedFactor;
            ((Storyboard)Resources["movementTop"]).SpeedRatio = 0.6 * SpeedFactor;
            ((Storyboard)Resources["movementThrough"]).SpeedRatio = 0.23 * SpeedFactor;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            frameTimer = new System.Windows.Threading.DispatcherTimer();
            frameTimer.Tick += OnFrame;
            frameTimer.Interval = TimeSpan.FromSeconds(1.0 / 50.0);

        }

        private void OnFrame(object sender, EventArgs e)
        {

        }

        private void resetRepeats()
        {
            animationRepeats = 0;
        }

        private void sizeChanged(object sender, EventArgs eventArgs)
        {
            allCanvas.RenderTransform = new ScaleTransform(ActualWidth / allCanvas.ActualWidth, ActualHeight / allCanvas.ActualHeight);
        }





        public void StartPresentation(string givenReceivedPhotonString, string givenBaseString, string givenPhotonOutputString, bool givenListened)
        {
            listened = givenListened;
            sleepMessage.Visibility = Visibility.Hidden;
            resetRepeats();
            setSpeed();
            hasFinished = false;
            receivedPhotonString = givenReceivedPhotonString;
            baseString = givenBaseString;
            photonOutputString = givenPhotonOutputString;
            if (animationRepeats < receivedPhotonString.Length)
            {

                animationPhaseOne();
            }
        }

        private void animationPhaseOne()
        {
            if (receivedPhotonString.Length > animationRepeats)
            {
                if (!receivedPhotonString[animationRepeats].Equals('W'))
                {
                    mainCanvas.Visibility = Visibility.Visible;
                }
            }

            initializeFirstImages();

            if (listened)
            {
                sleepMessage.Visibility = Visibility.Hidden;
                ((Storyboard)Resources["movementRight"]).Stop();
                ((Storyboard)Resources["movementTop"]).Stop();
                ((Storyboard)Resources["movementLeft"]).Stop();
                ((Storyboard)Resources["movementLeft"]).Begin();
            }
            else
            {
                sleepMessage.Visibility = Visibility.Visible;
                ((Storyboard)Resources["movementRight"]).Stop();
                ((Storyboard)Resources["movementTop"]).Stop();
                ((Storyboard)Resources["movementLeft"]).Stop();
                ((Storyboard)Resources["movementThrough"]).Begin();
            }
        }

        private void initializeFirstImages()
        {
            imageRightHorizontal.Visibility = Visibility.Hidden;
            imageRightTopLeft.Visibility = Visibility.Hidden;
            imageRightTopRight.Visibility = Visibility.Hidden;
            imageRightVertical.Visibility = Visibility.Hidden;
            imageTopHorizontal.Visibility = Visibility.Hidden;
            imageTopTopLeft.Visibility = Visibility.Hidden;
            imageTopTopRight.Visibility = Visibility.Hidden;
            imageTopVertical.Visibility = Visibility.Hidden;


            if (animationRepeats < receivedPhotonString.Length && animationRepeats < baseString.Length)
            {
                if (receivedPhotonString.ElementAt(animationRepeats).Equals('/'))
                {
                    imageLeftTopRightDiagonal.Visibility = Visibility.Visible;
                    imageLeftHorizontal.Visibility = Visibility.Hidden;
                    imageLeftTopLeftDiagonal.Visibility = Visibility.Hidden;
                    imageLeftVertical.Visibility = Visibility.Hidden;
                }
                else if (receivedPhotonString.ElementAt(animationRepeats).Equals('\\'))
                {
                    imageLeftTopLeftDiagonal.Visibility = Visibility.Visible;
                    imageLeftHorizontal.Visibility = Visibility.Hidden;
                    imageLeftTopRightDiagonal.Visibility = Visibility.Hidden;
                    imageLeftVertical.Visibility = Visibility.Hidden;
                }
                else if (receivedPhotonString.ElementAt(animationRepeats).Equals('|'))
                {
                    imageLeftVertical.Visibility = Visibility.Visible;
                    imageLeftTopLeftDiagonal.Visibility = Visibility.Hidden;
                    imageLeftHorizontal.Visibility = Visibility.Hidden;
                    imageLeftTopRightDiagonal.Visibility = Visibility.Hidden;
                }
                else
                {
                    imageLeftHorizontal.Visibility = Visibility.Visible;
                    imageLeftVertical.Visibility = Visibility.Hidden;
                    imageLeftTopLeftDiagonal.Visibility = Visibility.Hidden;
                    imageLeftTopRightDiagonal.Visibility = Visibility.Hidden;
                }

                if (baseString.ElementAt(animationRepeats).Equals('+'))
                {
                    imagePlus.Visibility = Visibility.Visible;
                    imageCross.Visibility = Visibility.Hidden;
                }
                else
                {
                    imagePlus.Visibility = Visibility.Hidden;
                    imageCross.Visibility = Visibility.Visible;
                }

                if (!listened)
                {
                    imagePlus.Visibility = Visibility.Hidden;
                    imageCross.Visibility = Visibility.Hidden;
                }

            }


        }

        public void StopPresentation()
        {
            stopAllStoryboards();
            hideEverything();
            sleepMessage.Visibility = Visibility.Visible;

            hasFinished = true;

            if (frameTimer != null)
            { frameTimer.Stop(); }
        }

        private void completedMovementLeft(object sender, EventArgs e)
        {
            animationPhaseTwo();
        }

        private void completedFadePlus(object sender, EventArgs e)
        {
            animationPhaseThree();
        }

        private void completedMovementRight(object sender, EventArgs e)
        {
            animationRepeats++;

            if (animationRepeats < receivedPhotonString.Length)
            {
                animationPhaseOne();
            }
            else
            {
                StopPresentation();
            }

        }



        private void animationPhaseTwo()
        {
            initializeSecondImages();
            ((Storyboard)Resources["fadePlus"]).Begin();
            ((Storyboard)Resources["fadeCross"]).Begin();
            ((Storyboard)Resources["fadeLeftVertical"]).Begin();
            ((Storyboard)Resources["fadeLeftHorizontal"]).Begin();
            ((Storyboard)Resources["fadeLeftTopLeftDiagonal"]).Begin();
            ((Storyboard)Resources["fadeLeftTopRightDiagonal"]).Begin();
        }

        private void initializeSecondImages()
        {
            if (animationRepeats < receivedPhotonString.Length && photonOutputString.Length > animationRepeats)
            {
                if (photonOutputString[animationRepeats].Equals('|'))
                {
                    imageTopVertical.Visibility = Visibility.Visible;
                    imageTopHorizontal.Visibility = Visibility.Hidden;
                    imageTopTopLeft.Visibility = Visibility.Hidden;
                    imageTopTopRight.Visibility = Visibility.Hidden;

                }
                else if (photonOutputString[animationRepeats].Equals('-'))
                {
                    imageTopHorizontal.Visibility = Visibility.Visible;
                    imageTopVertical.Visibility = Visibility.Hidden;
                    imageTopTopLeft.Visibility = Visibility.Hidden;
                    imageTopTopRight.Visibility = Visibility.Hidden;
                }
                else if (photonOutputString[animationRepeats].Equals('/'))
                {
                    imageTopTopRight.Visibility = Visibility.Visible;
                    imageTopHorizontal.Visibility = Visibility.Hidden;
                    imageTopVertical.Visibility = Visibility.Hidden;
                    imageTopTopLeft.Visibility = Visibility.Hidden;
                }
                else
                {
                    imageTopTopLeft.Visibility = Visibility.Visible;
                    imageTopTopRight.Visibility = Visibility.Hidden;
                    imageTopHorizontal.Visibility = Visibility.Hidden;
                    imageTopVertical.Visibility = Visibility.Hidden;
                }

            }

        }

        private void animationPhaseThree()
        {
            initializeThirdImages();
            imageLeftHorizontal.Visibility = Visibility.Hidden;
            imageLeftTopLeftDiagonal.Visibility = Visibility.Hidden;
            imageLeftTopRightDiagonal.Visibility = Visibility.Hidden;
            imageLeftVertical.Visibility = Visibility.Hidden;
            imageCross.Visibility = Visibility.Hidden;
            imagePlus.Visibility = Visibility.Hidden;

            ((Storyboard)Resources["fadePlus"]).Stop();
            ((Storyboard)Resources["fadeCross"]).Stop();
            ((Storyboard)Resources["fadeLeftVertical"]).Stop();
            ((Storyboard)Resources["fadeLeftHorizontal"]).Stop();
            ((Storyboard)Resources["fadeLeftTopLeftDiagonal"]).Stop();
            ((Storyboard)Resources["fadeLeftTopRightDiagonal"]).Stop();


            ((Storyboard)Resources["movementRight"]).Begin();
            ((Storyboard)Resources["movementTop"]).Begin();
        }

        private void initializeThirdImages()
        {
            if (animationRepeats < receivedPhotonString.Length && animationRepeats < photonOutputString.Length)
            {
                if (photonOutputString[animationRepeats].Equals('|'))
                {
                    imageRightVertical.Visibility = Visibility.Visible;
                    imageRightTopLeft.Visibility = Visibility.Hidden;
                    imageRightTopRight.Visibility = Visibility.Hidden;
                    imageRightHorizontal.Visibility = Visibility.Hidden;

                }
                else if (photonOutputString[animationRepeats].Equals('-'))
                {
                    imageRightHorizontal.Visibility = Visibility.Visible;
                    imageRightTopLeft.Visibility = Visibility.Hidden;
                    imageRightTopRight.Visibility = Visibility.Hidden;
                    imageRightVertical.Visibility = Visibility.Hidden;
                }
                else if (photonOutputString[animationRepeats].Equals('/'))
                {
                    imageRightTopRight.Visibility = Visibility.Visible;
                    imageRightTopLeft.Visibility = Visibility.Hidden;
                    imageRightVertical.Visibility = Visibility.Hidden;
                    imageRightHorizontal.Visibility = Visibility.Hidden;
                }
                else
                {
                    imageRightTopLeft.Visibility = Visibility.Visible;
                    imageRightTopRight.Visibility = Visibility.Hidden;
                    imageRightVertical.Visibility = Visibility.Hidden;
                    imageRightHorizontal.Visibility = Visibility.Hidden;
                }

            }
        }



        private void stopAllStoryboards()
        {
            ((Storyboard)Resources["fadePlus"]).Stop();
            ((Storyboard)Resources["fadeCross"]).Stop();
            ((Storyboard)Resources["fadeLeftVertical"]).Stop();
            ((Storyboard)Resources["fadeLeftHorizontal"]).Stop();
            ((Storyboard)Resources["fadeLeftTopLeftDiagonal"]).Stop();
            ((Storyboard)Resources["fadeLeftTopRightDiagonal"]).Stop();
            ((Storyboard)Resources["movementLeft"]).Stop();
            ((Storyboard)Resources["movementRight"]).Stop();
            ((Storyboard)Resources["movementTop"]).Stop();
            ((Storyboard)Resources["movementThrough"]).Stop();
        }

        private void hideEverything()
        {
            mainCanvas.Visibility = Visibility.Hidden;
        }



    }
}
