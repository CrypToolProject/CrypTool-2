using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace BB84PhotonDecoder
{
    public partial class BB84PhotonDecoderPresentation : UserControl
    {
        private System.Windows.Threading.DispatcherTimer frameTimer;
        private string keyString;
        private string photonString;
        private string baseString;
        public int animationRepeats;
        public int Progress;
        public bool hasFinished;
        private bool firstRepeat;
        public event EventHandler UpdateProgess;
        public double SpeedFactor;

        public BB84PhotonDecoderPresentation()
        {
            
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
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

            initializeBaseQueue();

            initializeFirstImages();

            startFullAnimation();

        }

        private void initializeBaseQueue()
        {
            char[] photonStringChars = photonString.ToCharArray();
            char[] baseStringChars = baseString.ToCharArray();

            if (baseStringChars.Length >= 2)
            {
                
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
                baseQueueCross2.Visibility = Visibility.Hidden;
                baseQueuePlus2.Visibility = Visibility.Hidden;
            }

            if (baseStringChars.Length >= 3)
            {
                
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
               
                baseQueueCross3.Visibility = Visibility.Hidden;
                baseQueuePlus3.Visibility = Visibility.Hidden;
            }
        
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
            
            imageRightOne.Visibility = Visibility.Hidden;
            imageRightZero.Visibility = Visibility.Hidden;
            imageError.Visibility = Visibility.Hidden;

            ((Storyboard)this.Resources["fadePlus"]).Stop();

            ((Storyboard)this.Resources["fadeCross"]).Stop();
            ((Storyboard)this.Resources["fadeInRightZero"]).Stop();
            ((Storyboard)this.Resources["fadeInRightOne"]).Stop();

            if (animationRepeats < photonString.Length)
            {
                if (photonString.ElementAt(animationRepeats).Equals('|'))
                {
                    imageLeftVertical.Visibility = Visibility.Visible;
                    imageLeftHorizontal.Visibility = Visibility.Hidden;
                    imageLeftTopLeftDiagonal.Visibility = Visibility.Hidden;
                    imageLeftTopRightDiagonal.Visibility = Visibility.Hidden;
                }
                else if (photonString.ElementAt(animationRepeats).Equals('-'))
                {
                    imageLeftVertical.Visibility = Visibility.Hidden;
                    imageLeftHorizontal.Visibility = Visibility.Visible;
                    imageLeftTopLeftDiagonal.Visibility = Visibility.Hidden;
                    imageLeftTopRightDiagonal.Visibility = Visibility.Hidden;
                }
                else if (photonString.ElementAt(animationRepeats).Equals('\\'))
                {
                    imageLeftVertical.Visibility = Visibility.Hidden;
                    imageLeftHorizontal.Visibility = Visibility.Hidden;
                    imageLeftTopLeftDiagonal.Visibility = Visibility.Visible;
                    imageLeftTopRightDiagonal.Visibility = Visibility.Hidden;
                }
                else if (photonString.ElementAt(animationRepeats).Equals('/'))
                {
                    imageLeftVertical.Visibility = Visibility.Hidden;
                    imageLeftHorizontal.Visibility = Visibility.Hidden;
                    imageLeftTopLeftDiagonal.Visibility = Visibility.Hidden;
                    imageLeftTopRightDiagonal.Visibility = Visibility.Visible;
                }
                
            }
            if (animationRepeats < baseString.Length){

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
            if (animationRepeats < photonString.Length)
            {
                if (photonString[animationRepeats].Equals('W'))
                {
                    mainCanvas.Visibility = Visibility.Hidden;
                }
                else
                {
                    mainCanvas.Visibility = Visibility.Visible;
                }
                frameTimer.Start();
                animationPhaseOne();
            }
        }

        private void animationPhaseOne()
        {

            initializeFirstImages();
            ((Storyboard)this.Resources["movementLeft"]).Stop();
            ((Storyboard)this.Resources["fadeLeftVertical"]).Stop();
            ((Storyboard)this.Resources["fadeLeftHorizontal"]).Stop();
            ((Storyboard)this.Resources["fadeLeftTopLeftDiagonal"]).Stop();
            ((Storyboard)this.Resources["fadeLeftTopRightDiagonal"]).Stop();
            ((Storyboard)this.Resources["fadePlus"]).Stop();
            ((Storyboard)this.Resources["fadeCross"]).Stop();
            ((Storyboard)this.Resources["movementLeft"]).Begin();
            ((Storyboard)this.Resources["movementBottom"]).Begin();
           // ((Storyboard)this.Resources["scalingLeft"]).Begin();
            ((Storyboard)this.Resources["scalingBottom"]).Begin();

            baseQueueCross1.Visibility = baseQueuePlus1.Visibility = Visibility.Hidden;
        }

        private void completedMovementLeft(object sender, EventArgs e)
        {
            animationPhaseTwo();
        }

        private void animationPhaseTwo()
        {
            initializeSecondImages();

            ((Storyboard)this.Resources["movementRight"]).Stop();
            ((Storyboard)this.Resources["movementBottom"]).Stop();

            ((Storyboard)this.Resources["fadePlus"]).Begin();
            ((Storyboard)this.Resources["fadeCross"]).Begin();
            ((Storyboard)this.Resources["fadeLeftVertical"]).Begin();
            ((Storyboard)this.Resources["fadeLeftHorizontal"]).Begin();
            ((Storyboard)this.Resources["fadeLeftTopLeftDiagonal"]).Begin();
            ((Storyboard)this.Resources["fadeLeftTopRightDiagonal"]).Begin();
            ((Storyboard)this.Resources["moveCrossBaseQueues"]).Begin();
            ((Storyboard)this.Resources["movePlusBaseQueues"]).Begin();
            ((Storyboard)this.Resources["fadeInRightZero"]).Begin();
            ((Storyboard)this.Resources["fadeInRightOne"]).Begin();
            ((Storyboard)this.Resources["lightning"]).Begin();

            updateQueues();
        }

        private void updateQueues()
        {

            char[] baseStringChars = baseString.ToCharArray();
            if ( animationRepeats + 1 < baseStringChars.Length)
            {

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
                baseQueueCross1.Visibility = Visibility.Hidden;
                baseQueuePlus1.Visibility = Visibility.Hidden;
            }

            if (animationRepeats + 2 < baseStringChars.Length)
            {   
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
                
                baseQueueCross2.Visibility = Visibility.Hidden;
                baseQueuePlus2.Visibility = Visibility.Hidden;
            }

            baseQueueCross3.Visibility = Visibility.Hidden;
            baseQueuePlus3.Visibility = Visibility.Hidden;
        }

        private void initializeSecondImages()
        {
 
            
            if (animationRepeats < photonString.Length && animationRepeats < baseString.Length)
            {

                if (baseString.ElementAt(animationRepeats).Equals('+'))
                {
                    imageCross.Visibility = Visibility.Hidden;
                    if (photonString.ElementAt(animationRepeats).Equals('/') || photonString.ElementAt(animationRepeats).Equals('\\'))
                    {
                        imageError.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    imagePlus.Visibility = Visibility.Hidden;
                    if (photonString.ElementAt(animationRepeats).Equals('|') || photonString.ElementAt(animationRepeats).Equals('-'))
                    {
                        imageError.Visibility = Visibility.Visible;
                    }
                }
            }

            if (animationRepeats < keyString.Length){
                if (keyString.ElementAt(animationRepeats).Equals('0'))
                {
                    imageRightZero.Visibility = Visibility.Visible;
                    imageRightOne.Visibility = Visibility.Hidden;
                }
                else
                {
                    imageRightZero.Visibility = Visibility.Hidden;
                    imageRightOne.Visibility = Visibility.Visible;
                }
                
                
            }
        }

        private void completedFadePlus(object sender, EventArgs e)
        {
            animationPhaseThree();
        }

        private void animationPhaseThree()
        {
            //imageError.Visibility = Visibility.Hidden;
            ((Storyboard)this.Resources["movementRight"]).Begin();
            
        }

        private void completedMovementRight(object sender, EventArgs e)
        {
            animationRepeats++;
            firstRepeat = false;
            if (animationRepeats < photonString.Length)
            {
                if (photonString[animationRepeats].Equals('W'))
                {
                    mainCanvas.Visibility = Visibility.Hidden;
                }
                else
                {
                    mainCanvas.Visibility = Visibility.Visible;
                }
                animationPhaseOne();
            }
            else
            {
                StopPresentation();
            }
        }

        private void sizeChanged(Object sender, EventArgs eventArgs)
        {
            allCanvas.RenderTransform = new ScaleTransform(this.ActualWidth / allCanvas.ActualWidth, this.ActualHeight / allCanvas.ActualHeight);
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
            ((Storyboard)this.Resources["movementLeft"]).Stop();
            ((Storyboard)this.Resources["movementRight"]).Stop();
            ((Storyboard)this.Resources["movementBottom"]).Stop();
            ((Storyboard)this.Resources["fadePlus"]).Stop();
            ((Storyboard)this.Resources["fadeCross"]).Stop();
            ((Storyboard)this.Resources["scalingBottom"]).Stop();
            ((Storyboard)this.Resources["fadeLeftVertical"]).Stop();
            ((Storyboard)this.Resources["fadeLeftHorizontal"]).Stop();
            ((Storyboard)this.Resources["fadeLeftTopLeftDiagonal"]).Stop();
            ((Storyboard)this.Resources["fadeLeftTopRightDiagonal"]).Stop();
            ((Storyboard)this.Resources["lightning"]).Stop();
            ((Storyboard)this.Resources["fadeInRightZero"]).Stop();
            ((Storyboard)this.Resources["fadeInRightOne"]).Stop();
            ((Storyboard)this.Resources["moveCrossBaseQueues"]).Stop();
            ((Storyboard)this.Resources["movePlusBaseQueues"]).Stop();


        }

        private void completedQueueMovement(object sender, EventArgs e)
        {
            updateThirdQueue();
        }

        private void updateThirdQueue()
        {
            char[] baseStringChars = baseString.ToCharArray();

            if (animationRepeats+3 < baseString.Length)
            { 
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
                baseQueueCross3.Visibility = Visibility.Hidden;
                baseQueuePlus3.Visibility = Visibility.Hidden;
            }
        }

        public void setSpeed()
        {
            ((Storyboard)this.Resources["movementLeft"]).SpeedRatio = 0.6*SpeedFactor;
            ((Storyboard)this.Resources["movementRight"]).SpeedRatio = 0.6* SpeedFactor;
            ((Storyboard)this.Resources["movementBottom"]).SpeedRatio = 1.3* SpeedFactor;
            ((Storyboard)this.Resources["fadePlus"]).SpeedRatio = SpeedFactor;
            ((Storyboard)this.Resources["fadeCross"]).SpeedRatio = SpeedFactor;
            ((Storyboard)this.Resources["scalingLeft"]).SpeedRatio = 0.6* SpeedFactor;
            ((Storyboard)this.Resources["scalingBottom"]).SpeedRatio = 1.3 * SpeedFactor;
            ((Storyboard)this.Resources["fadeLeftVertical"]).SpeedRatio = SpeedFactor;
            ((Storyboard)this.Resources["fadeLeftHorizontal"]).SpeedRatio = SpeedFactor;
            ((Storyboard)this.Resources["fadeLeftTopLeftDiagonal"]).SpeedRatio = SpeedFactor;
            ((Storyboard)this.Resources["fadeLeftTopRightDiagonal"]).SpeedRatio = SpeedFactor;
            ((Storyboard)this.Resources["lightning"]).SpeedRatio = SpeedFactor;
            ((Storyboard)this.Resources["fadeInRightZero"]).SpeedRatio = SpeedFactor;
            ((Storyboard)this.Resources["fadeInRightOne"]).SpeedRatio = SpeedFactor;
            ((Storyboard)this.Resources["moveCrossBaseQueues"]).SpeedRatio = 0.6* SpeedFactor;
            ((Storyboard)this.Resources["movePlusBaseQueues"]).SpeedRatio =0.6* SpeedFactor;
        }

       
    }
}
