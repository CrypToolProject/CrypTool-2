using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace BB84KeyGenerator
{
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.Plugins.BB84KeyGenerator.Properties.Resources")]
    public partial class BB84KeyGeneratorPresentation : UserControl
    {
        private string firstBases;
        private string secondBases;
        private string givenKey;
        private bool commonBases;
        public event EventHandler UpdateProgess;
        public int Progress;
        private readonly int maxLengthStrings;
        public double speed;
        public bool hasFinished;


        public BB84KeyGeneratorPresentation()
        {
            InitializeComponent();
            maxLengthStrings = 15;
            firstBases = "";
            secondBases = "";
            givenKey = "";
            hasFinished = true;
            SizeChanged += sizeChanged;
        }

        internal void StartPresentation(string inputBasesFirst, string inputBasesSecond, string inputKey)
        {
            commonBases = false;
            messageBox.Visibility = Visibility.Hidden;

            setSpeed();

            hasFinished = false;



            initializeStrings(inputBasesFirst, inputBasesSecond, inputKey);

            hideAndStopEverything();

            animationPhaseOne();
        }

        private void hideAndStopEverything()
        {
            inputKeyBox.Visibility = Visibility.Hidden;
            firstBasesBox.Visibility = Visibility.Hidden;
            secondBasesBox.Visibility = Visibility.Hidden;
            commonKeyBox.Visibility = Visibility.Hidden;

            for (int i = 0; i < overlayCanvas.Children.Count; i++)
            {
                overlayCanvas.Children[i].Visibility = Visibility.Hidden;
            }

            ((Storyboard)Resources["fadeInInputKeyBox"]).Stop();
            ((Storyboard)Resources["fadeInFirstBasesBox"]).Stop();
            ((Storyboard)Resources["fadeInSecondBasesBox"]).Stop();
            ((Storyboard)Resources["fadeInOverlayBoxes"]).Stop();
            ((Storyboard)Resources["fadeInCommonKeyBox"]).Stop();
        }




        private void initializeStrings(string inputBasesFirst, string inputBasesSecond, string inputKey)
        {
            if (inputBasesFirst.Length > maxLengthStrings && inputBasesSecond.Length > maxLengthStrings && inputKey.Length > maxLengthStrings)
            {
                firstBases = inputBasesFirst.Substring(0, maxLengthStrings);
                secondBases = inputBasesSecond.Substring(0, maxLengthStrings);
                givenKey = inputKey.Substring(0, maxLengthStrings);
            }
            else
            {
                firstBases = inputBasesFirst;
                secondBases = inputBasesSecond;
                givenKey = inputKey;
            }


            StringBuilder commonKeyBuilder = new StringBuilder();
            for (int i = 0; i < firstBases.Length; i++)
            {
                if (givenKey.Length > i && firstBases.Length > i && secondBases.Length > i)
                {
                    if (firstBases[i].Equals(secondBases[i]))
                    {
                        commonKeyBuilder.Append(givenKey[i]);
                    }
                    else
                    {
                        commonKeyBuilder.Append(" ");
                    }
                }
            }



            if (inputBasesFirst.Length > maxLengthStrings)
            {
                firstBasesBox.Text = firstBases + " ...";
                secondBasesBox.Text = secondBases + " ...";
                inputKeyBox.Text = givenKey + " ...";
                commonKeyBox.Text = commonKeyBuilder.ToString() + " ...";
            }
            else
            {
                firstBasesBox.Text = firstBases;
                secondBasesBox.Text = secondBases;
                inputKeyBox.Text = givenKey;
                commonKeyBox.Text = commonKeyBuilder.ToString();
            }
        }

        private void animationPhaseOne()
        {


            inputKeyBox.Visibility = Visibility.Visible;
            ((Storyboard)Resources["fadeInInputKeyBox"]).Begin();
        }

        private void completedFadeInInputKeyBox(object sender, EventArgs e)
        {
            firstBasesBox.Visibility = Visibility.Visible;
            ((Storyboard)Resources["fadeInFirstBasesBox"]).Begin();
        }

        private void completedFadeInFirstBasesBox(object sender, EventArgs e)
        {
            secondBasesBox.Visibility = Visibility.Visible;
            ((Storyboard)Resources["fadeInSecondBasesBox"]).Begin();
        }

        private void completedFadeInSecondBasesBox(object sender, EventArgs e)
        {
            overlayCanvas.Visibility = Visibility.Visible;
            for (int i = 0; i < firstBases.Length; i++)
            {
                if (secondBases.Length > i)
                {
                    if (firstBases[i].Equals(secondBases[i]))
                    {
                        overlayCanvas.Children[i].Visibility = Visibility.Visible;
                        commonBases = true;
                    }
                }
            }


            ((Storyboard)Resources["fadeInOverlayBoxes"]).Begin();
        }

        private void completedFadeOverlayBoxes(object sender, EventArgs e)
        {
            if (commonBases)
            {
                commonKeyBox.Visibility = Visibility.Visible;
                ((Storyboard)Resources["fadeInCommonKeyBox"]).Begin();
            }
            else
            {
                messageBox.Visibility = Visibility.Visible;
            }
        }

        private void completedFadeInCommonKeyBox(object sender, EventArgs e)
        {
            hasFinished = true;
        }

        internal bool StopPresentation()
        {
            hideAndStopEverything();
            hasFinished = true;
            return hasFinished;
        }

        private void sizeChanged(object sender, EventArgs eventArgs)
        {
            allCanvas.RenderTransform = new ScaleTransform(ActualWidth / allCanvas.ActualWidth, ActualHeight / allCanvas.ActualHeight);
        }

        private void setSpeed()
        {
            ((Storyboard)Resources["fadeInInputKeyBox"]).SpeedRatio = speed;
            ((Storyboard)Resources["fadeInFirstBasesBox"]).SpeedRatio = speed;
            ((Storyboard)Resources["fadeInSecondBasesBox"]).SpeedRatio = speed;
            ((Storyboard)Resources["fadeInOverlayBoxes"]).SpeedRatio = speed;
            ((Storyboard)Resources["fadeInCommonKeyBox"]).SpeedRatio = speed;
        }








    }
}
