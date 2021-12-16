using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CrypTool.Plugins.BB84PhotonEncoder
{
    public partial class BB84PhotonEncoderPresentation : UserControl
    {
        private System.Windows.Threading.DispatcherTimer frameTimer;
        private string keyString;
        private string photonString;
        private string baseString;
        public int animationRepeats;
        public bool hasFinished;
        public int Progress;
        public event EventHandler UpdateProgess;
        public double SpeedFactor;


        public BB84PhotonEncoderPresentation()
        {
            hasFinished = true;
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
            resetRepeats();
            InitializeComponent();
            mainCanvas.Visibility = Visibility.Hidden;
            SizeChanged += sizeChanged;

        }

        public void StartPresentation(string givenKeyString, string givenPhotonString, string givenBaseString)
        {
            hasFinished = false;

            mainCanvas.Visibility = Visibility.Visible;

            resetRepeats();

            setSpeed();

            initializeStrings(givenKeyString, givenPhotonString, givenBaseString);

            initializeBitQueues();

            initializeFirstImages();

            startFullAnimation();


        }

        private void setSpeed()
        {
            ((Storyboard)Resources["movementLeft"]).SpeedRatio = 0.6 * SpeedFactor;
            ((Storyboard)Resources["movementRight"]).SpeedRatio = 0.6 * SpeedFactor;
            ((Storyboard)Resources["movementBottom"]).SpeedRatio = 1.3 * SpeedFactor;
            ((Storyboard)Resources["fadePlus"]).SpeedRatio = SpeedFactor;
            ((Storyboard)Resources["fadeCross"]).SpeedRatio = SpeedFactor;
            ((Storyboard)Resources["fadeLeftOne"]).SpeedRatio = SpeedFactor;
            ((Storyboard)Resources["fadeLeftZero"]).SpeedRatio = SpeedFactor;
            ((Storyboard)Resources["scalingBottom"]).SpeedRatio = 1.3 * SpeedFactor;
            ((Storyboard)Resources["scalingLeft"]).SpeedRatio = 0.6 * SpeedFactor;
            ((Storyboard)Resources["moveZeroBitQueues"]).SpeedRatio = 0.6 * SpeedFactor;
            ((Storyboard)Resources["moveOneBitQueues"]).SpeedRatio = 0.6 * SpeedFactor;
            ((Storyboard)Resources["moveCrossBaseQueues"]).SpeedRatio = 0.6 * SpeedFactor;
            ((Storyboard)Resources["movePlusBaseQueues"]).SpeedRatio = 0.6 * SpeedFactor;
        }

        private void resetRepeats()
        {
            animationRepeats = 0;
        }

        private void initializeStrings(string givenKeyString, string givenPhotonString, string givenBaseString)
        {
            keyString = givenKeyString;
            photonString = givenPhotonString;
            baseString = givenBaseString;
        }

        private void initializeFirstImages()
        {
            rightHorizontal.X = 0;
            rightVertical.X = 0;
            rightTopLeft.X = 0;
            rightTopRight.X = 0;
            imageRightHorizontal.Visibility = Visibility.Hidden;
            imageRightTopLeft.Visibility = Visibility.Hidden;
            imageRightTopRight.Visibility = Visibility.Hidden;
            imageRightVertical.Visibility = Visibility.Hidden;

            ((Storyboard)Resources["fadePlus"]).Stop();

            ((Storyboard)Resources["fadeCross"]).Stop();

            if (animationRepeats < keyString.Length)
            {
                if (keyString.ElementAt(animationRepeats).Equals('0'))
                {
                    imageLeftOne.Visibility = Visibility.Hidden;
                    imageLeftZero.Visibility = Visibility.Visible;
                }
                else
                {
                    imageLeftOne.Visibility = Visibility.Visible;
                    imageLeftZero.Visibility = Visibility.Hidden;
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
            }
        }

        private void startFullAnimation()
        {
            if (animationRepeats < keyString.Length)
            {
                frameTimer.Start();
                animationPhaseOne();
            }
        }

        private void animationPhaseOne()
        {


            initializeFirstImages();
            ((Storyboard)Resources["movementLeft"]).Stop();
            ((Storyboard)Resources["fadeLeftOne"]).Stop();
            ((Storyboard)Resources["fadeLeftZero"]).Stop();
            ((Storyboard)Resources["fadePlus"]).Stop();
            ((Storyboard)Resources["fadeCross"]).Stop();
            ((Storyboard)Resources["movementLeft"]).Begin();
            ((Storyboard)Resources["movementBottom"]).Begin();
            ((Storyboard)Resources["scalingLeft"]).Begin();
            ((Storyboard)Resources["scalingBottom"]).Begin();

            bitQueueOne1.Visibility = bitQueueZero1.Visibility = Visibility.Hidden;
            baseQueueCross1.Visibility = baseQueuePlus1.Visibility = Visibility.Hidden;
        }

        private void completedMovementLeft(object sender, EventArgs e)
        {
            animationPhaseTwo();
        }

        private void animationPhaseTwo()
        {
            initializeSecondImages();

            ((Storyboard)Resources["movementRight"]).Stop();
            ((Storyboard)Resources["movementBottom"]).Stop();

            ((Storyboard)Resources["fadePlus"]).Begin();
            ((Storyboard)Resources["fadeCross"]).Begin();
            ((Storyboard)Resources["fadeLeftOne"]).Begin();
            ((Storyboard)Resources["fadeLeftZero"]).Begin();
            ((Storyboard)Resources["moveZeroBitQueues"]).Begin();
            ((Storyboard)Resources["moveOneBitQueues"]).Begin();
            ((Storyboard)Resources["moveCrossBaseQueues"]).Begin();
            ((Storyboard)Resources["movePlusBaseQueues"]).Begin();

            updateFirstTwoBitQueues();
        }

        private void initializeBitQueues()
        {
            char[] keyStringChars = keyString.ToCharArray();
            char[] baseStringChars = baseString.ToCharArray();

            if (keyStringChars.Length >= 2 && baseStringChars.Length >= 2)
            {
                if (keyStringChars[1].Equals('0'))
                {
                    bitQueueOne2.Visibility = Visibility.Hidden;
                    bitQueueZero2.Visibility = Visibility.Visible;
                }
                else
                {
                    bitQueueOne2.Visibility = Visibility.Visible;
                    bitQueueZero2.Visibility = Visibility.Hidden;
                }
                if (baseStringChars[1].Equals('+'))
                {
                    baseQueueCross2.Visibility = Visibility.Hidden;
                    baseQueuePlus2.Visibility = Visibility.Visible;
                }
                else
                {
                    baseQueueCross2.Visibility = Visibility.Visible;
                    baseQueuePlus2.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                bitQueueOne2.Visibility = Visibility.Hidden;
                bitQueueZero2.Visibility = Visibility.Hidden;
                baseQueueCross2.Visibility = Visibility.Hidden;
                baseQueuePlus2.Visibility = Visibility.Hidden;
            }

            if (keyStringChars.Length >= 3 && baseStringChars.Length >= 3)
            {
                if (keyStringChars[2].Equals('0'))
                {
                    bitQueueOne3.Visibility = Visibility.Hidden;
                    bitQueueZero3.Visibility = Visibility.Visible;
                }
                else
                {
                    bitQueueOne3.Visibility = Visibility.Visible;
                    bitQueueZero3.Visibility = Visibility.Hidden;
                }
                if (baseStringChars[2].Equals('+'))
                {
                    baseQueueCross3.Visibility = Visibility.Hidden;
                    baseQueuePlus3.Visibility = Visibility.Visible;
                }
                else
                {
                    baseQueueCross3.Visibility = Visibility.Visible;
                    baseQueuePlus3.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                bitQueueOne3.Visibility = Visibility.Hidden;
                bitQueueZero3.Visibility = Visibility.Hidden;
                baseQueueCross3.Visibility = Visibility.Hidden;
                baseQueuePlus3.Visibility = Visibility.Hidden;
            }
        }

        private void updateFirstTwoBitQueues()
        {
            char[] keyStringChars = keyString.ToCharArray();
            char[] baseStringChars = baseString.ToCharArray();
            if (animationRepeats + 1 < keyStringChars.Length && animationRepeats + 1 < baseStringChars.Length)
            {
                if (keyStringChars[animationRepeats + 1].Equals('0'))
                {
                    bitQueueOne1.Visibility = Visibility.Hidden;
                    bitQueueZero1.Visibility = Visibility.Visible;

                }
                else
                {
                    bitQueueOne1.Visibility = Visibility.Visible;
                    bitQueueZero1.Visibility = Visibility.Hidden;
                }

                if (baseStringChars[animationRepeats + 1].Equals('+'))
                {
                    baseQueueCross1.Visibility = Visibility.Hidden;
                    baseQueuePlus1.Visibility = Visibility.Visible;
                }
                else
                {
                    baseQueueCross1.Visibility = Visibility.Visible;
                    baseQueuePlus1.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                bitQueueOne1.Visibility = Visibility.Hidden;
                bitQueueZero1.Visibility = Visibility.Hidden;
                baseQueueCross1.Visibility = Visibility.Hidden;
                baseQueuePlus1.Visibility = Visibility.Hidden;
            }

            if (animationRepeats + 2 < keyStringChars.Length && animationRepeats + 2 < baseStringChars.Length)
            {
                if (keyStringChars[animationRepeats + 2].Equals('0'))
                {
                    bitQueueOne2.Visibility = Visibility.Hidden;
                    bitQueueZero2.Visibility = Visibility.Visible;
                }
                else
                {
                    bitQueueOne2.Visibility = Visibility.Visible;
                    bitQueueZero2.Visibility = Visibility.Hidden;
                }
                if (baseStringChars[animationRepeats + 2].Equals('+'))
                {
                    baseQueueCross2.Visibility = Visibility.Hidden;
                    baseQueuePlus2.Visibility = Visibility.Visible;
                }
                else
                {
                    baseQueueCross2.Visibility = Visibility.Visible;
                    baseQueuePlus2.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                bitQueueOne2.Visibility = Visibility.Hidden;
                bitQueueZero2.Visibility = Visibility.Hidden;
                baseQueueCross2.Visibility = Visibility.Hidden;
                baseQueuePlus2.Visibility = Visibility.Hidden;
            }

            bitQueueOne3.Visibility = Visibility.Hidden;
            bitQueueZero3.Visibility = Visibility.Hidden;
            baseQueueCross3.Visibility = Visibility.Hidden;
            baseQueuePlus3.Visibility = Visibility.Hidden;
        }

        private void completedFadePlus(object sender, EventArgs e)
        {
            animationPhaseThree();
        }

        private void animationPhaseThree()
        {



            ((Storyboard)Resources["movementRight"]).Begin();
        }

        private void initializeSecondImages()
        {
            if (animationRepeats < keyString.Length && animationRepeats < baseString.Length && animationRepeats < photonString.Length)
            {
                if (baseString.ElementAt(animationRepeats).Equals('+'))
                {
                    imageCross.Visibility = Visibility.Hidden;
                }
                else
                {
                    imagePlus.Visibility = Visibility.Hidden;
                }


                if (photonString.ElementAt(animationRepeats).Equals('|'))
                {
                    imageRightHorizontal.Visibility = Visibility.Hidden;
                    imageRightTopLeft.Visibility = Visibility.Hidden;
                    imageRightTopRight.Visibility = Visibility.Hidden;
                    imageRightVertical.Visibility = Visibility.Visible;
                }
                else if (photonString.ElementAt(animationRepeats).Equals('-'))
                {
                    imageRightHorizontal.Visibility = Visibility.Visible;
                    imageRightTopLeft.Visibility = Visibility.Hidden;
                    imageRightTopRight.Visibility = Visibility.Hidden;
                    imageRightVertical.Visibility = Visibility.Hidden;
                }
                else if (photonString.ElementAt(animationRepeats).Equals('\\'))
                {
                    imageRightHorizontal.Visibility = Visibility.Hidden;
                    imageRightTopLeft.Visibility = Visibility.Visible;
                    imageRightTopRight.Visibility = Visibility.Hidden;
                    imageRightVertical.Visibility = Visibility.Hidden;
                }
                else
                {
                    imageRightHorizontal.Visibility = Visibility.Hidden;
                    imageRightTopLeft.Visibility = Visibility.Hidden;
                    imageRightTopRight.Visibility = Visibility.Visible;
                    imageRightVertical.Visibility = Visibility.Hidden;
                }
            }
        }

        private void completedMovementRight(object sender, EventArgs e)
        {
            animationRepeats++;

            if (animationRepeats < photonString.Length)
            {
                animationPhaseOne();
            }
            else
            {
                StopPresentation();
            }
        }

        private void sizeChanged(object sender, EventArgs eventArgs)
        {
            allCanvas.RenderTransform = new ScaleTransform(ActualWidth / allCanvas.ActualWidth, ActualHeight / allCanvas.ActualHeight);
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







        private void completedMovementBottom(object sender, EventArgs e)
        {

        }






        private void completedFadeCross(object sender, EventArgs e)
        {

        }

        private void completedBitQueueMovement(object sender, EventArgs e)
        {
            updateThirdBitQueue();

        }

        private void updateThirdBitQueue()
        {
            char[] keyStringChars = keyString.ToCharArray();
            char[] baseStringChars = baseString.ToCharArray();

            if (animationRepeats + 3 < keyStringChars.Length && animationRepeats + 3 < baseStringChars.Length)
            {
                if (keyStringChars[animationRepeats + 3].Equals('0'))
                {
                    bitQueueOne3.Visibility = Visibility.Hidden;
                    bitQueueZero3.Visibility = Visibility.Visible;
                }
                else
                {
                    bitQueueOne3.Visibility = Visibility.Visible;
                    bitQueueZero3.Visibility = Visibility.Hidden;
                }
                if (baseStringChars[animationRepeats + 3].Equals('+'))
                {
                    baseQueueCross3.Visibility = Visibility.Hidden;
                    baseQueuePlus3.Visibility = Visibility.Visible;
                }
                else
                {
                    baseQueueCross3.Visibility = Visibility.Visible;
                    baseQueuePlus3.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                bitQueueOne3.Visibility = Visibility.Hidden;
                bitQueueZero3.Visibility = Visibility.Hidden;
                baseQueueCross3.Visibility = Visibility.Hidden;
                baseQueuePlus3.Visibility = Visibility.Hidden;
            }
        }

        internal void StopPresentation()
        {
            stopAllStoryboards();
            resetRepeats();
            mainCanvas.Visibility = Visibility.Hidden;
            hasFinished = true;

            if (frameTimer != null)
            { frameTimer.Stop(); }


        }

        private void stopAllStoryboards()
        {
            ((Storyboard)Resources["movementLeft"]).Stop();
            ((Storyboard)Resources["movementRight"]).Stop();
            ((Storyboard)Resources["movementBottom"]).Stop();
            ((Storyboard)Resources["fadePlus"]).Stop();
            ((Storyboard)Resources["fadeCross"]).Stop();
            ((Storyboard)Resources["scalingLeft"]).Stop();
            ((Storyboard)Resources["scalingBottom"]).Stop();
            ((Storyboard)Resources["fadeLeftOne"]).Stop();
            ((Storyboard)Resources["fadeLeftZero"]).Stop();
            ((Storyboard)Resources["scalingBottom"]).Stop();
            ((Storyboard)Resources["scalingLeft"]).Stop();
        }
    }
}
