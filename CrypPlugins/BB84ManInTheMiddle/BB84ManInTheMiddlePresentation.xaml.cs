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
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            resetRepeats();
            InitializeComponent();
            SizeChanged += sizeChanged;
            hideEverything();
            hasFinished = true;
        }

        private void setSpeed()
        {
            ((Storyboard)this.Resources["fadePlus"]).SpeedRatio = SpeedFactor;
            ((Storyboard)this.Resources["fadeCross"]).SpeedRatio = SpeedFactor;
            ((Storyboard)this.Resources["fadeLeftVertical"]).SpeedRatio = SpeedFactor;
            ((Storyboard)this.Resources["fadeLeftHorizontal"]).SpeedRatio = SpeedFactor;
            ((Storyboard)this.Resources["fadeLeftTopLeftDiagonal"]).SpeedRatio = SpeedFactor;
            ((Storyboard)this.Resources["fadeLeftTopRightDiagonal"]).SpeedRatio = SpeedFactor;
            ((Storyboard)this.Resources["movementLeft"]).SpeedRatio = 0.6 * SpeedFactor;
            ((Storyboard)this.Resources["movementRight"]).SpeedRatio = 0.6 * SpeedFactor;
            ((Storyboard)this.Resources["movementTop"]).SpeedRatio = 0.6 * SpeedFactor;
            ((Storyboard)this.Resources["movementThrough"]).SpeedRatio = 0.23 * SpeedFactor;
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

        private void sizeChanged(Object sender, EventArgs eventArgs)
        {
            allCanvas.RenderTransform = new ScaleTransform(this.ActualWidth / allCanvas.ActualWidth, this.ActualHeight / allCanvas.ActualHeight);
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
                ((Storyboard)this.Resources["movementRight"]).Stop();
                ((Storyboard)this.Resources["movementTop"]).Stop();
                ((Storyboard)this.Resources["movementLeft"]).Stop();
                ((Storyboard)this.Resources["movementLeft"]).Begin();
            }
            else
            {
                sleepMessage.Visibility = Visibility.Visible;
                ((Storyboard)this.Resources["movementRight"]).Stop();
                ((Storyboard)this.Resources["movementTop"]).Stop();
                ((Storyboard)this.Resources["movementLeft"]).Stop();
                ((Storyboard)this.Resources["movementThrough"]).Begin();
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
            ((Storyboard)this.Resources["fadePlus"]).Begin();
            ((Storyboard)this.Resources["fadeCross"]).Begin();
            ((Storyboard)this.Resources["fadeLeftVertical"]).Begin();
            ((Storyboard)this.Resources["fadeLeftHorizontal"]).Begin();
            ((Storyboard)this.Resources["fadeLeftTopLeftDiagonal"]).Begin();
            ((Storyboard)this.Resources["fadeLeftTopRightDiagonal"]).Begin();
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

            ((Storyboard)this.Resources["fadePlus"]).Stop();
            ((Storyboard)this.Resources["fadeCross"]).Stop();
            ((Storyboard)this.Resources["fadeLeftVertical"]).Stop();
            ((Storyboard)this.Resources["fadeLeftHorizontal"]).Stop();
            ((Storyboard)this.Resources["fadeLeftTopLeftDiagonal"]).Stop();
            ((Storyboard)this.Resources["fadeLeftTopRightDiagonal"]).Stop();
            

            ((Storyboard)this.Resources["movementRight"]).Begin();
            ((Storyboard)this.Resources["movementTop"]).Begin();
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
            ((Storyboard)this.Resources["fadePlus"]).Stop();
            ((Storyboard)this.Resources["fadeCross"]).Stop();
            ((Storyboard)this.Resources["fadeLeftVertical"]).Stop();
            ((Storyboard)this.Resources["fadeLeftHorizontal"]).Stop();
            ((Storyboard)this.Resources["fadeLeftTopLeftDiagonal"]).Stop();
            ((Storyboard)this.Resources["fadeLeftTopRightDiagonal"]).Stop();
            ((Storyboard)this.Resources["movementLeft"]).Stop();
            ((Storyboard)this.Resources["movementRight"]).Stop();
            ((Storyboard)this.Resources["movementTop"]).Stop();
            ((Storyboard)this.Resources["movementThrough"]).Stop();
        }

        private void hideEverything()
        {
            mainCanvas.Visibility = Visibility.Hidden;
        }
        

        
    }
}
