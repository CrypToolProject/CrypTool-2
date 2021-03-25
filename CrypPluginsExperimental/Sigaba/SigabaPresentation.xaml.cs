using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace Sigaba
{
    /// <summary>
    /// Interaction logic for SigabaPresentation.xaml
    /// </summary>
    public partial class SigabaPresentation
    {
        #region rotors
        
        private PresentationRotor _pr0;
        private PresentationRotor _pr1;
        private PresentationRotor _pr2;
        private PresentationRotor _pr3;
        private PresentationRotor _pr4;

        private PresentationRotor _pr10;
        private PresentationRotor _pr11;
        private PresentationRotor _pr12;
        private PresentationRotor _pr13;
        private PresentationRotor _pr14;

        private PresentationIndex _pri0;
        private PresentationIndex _pri1;
        private PresentationIndex _pri2;
        private PresentationIndex _pri3;
        private PresentationIndex _pri4;

        private Rectangle[] _rarray = new Rectangle[5];

        #endregion

        #region variables
        private Rotor3 _toro;
        private Rotor3Index _toro2;
        private ORing _oro;
        private ORing2 _oro2;
        private SigabaSettings _settings;

        public Storyboard St = new Storyboard();
        private double _time = 0;
        public double SpeedRatio = 40;

        public Boolean stopPresentation;

        public Boolean Callback;

        #endregion

        #region constructor

        public SigabaPresentation(Sigaba facade, SigabaSettings settings)
        {
            InitializeComponent();
            _settings = settings;
            SizeChanged += sizeChanged; 
            AddRotors(settings);
            ClearPresentation();
            _rarray[0] = mark1;
            _rarray[1] = mark2;
            _rarray[2] = mark3;
            _rarray[3] = mark4;
            _rarray[4] = mark5;
        }

        #endregion

        #region storyboard

        private void FillStoryBoard(char c, TextBlock t, Boolean timer)
        {
            StringAnimationUsingKeyFrames stas = new StringAnimationUsingKeyFrames();

            DiscreteStringKeyFrame desc = new DiscreteStringKeyFrame("", TimeSpan.FromMilliseconds(_time));
            DiscreteStringKeyFrame desc2 = new DiscreteStringKeyFrame(c + "", TimeSpan.FromMilliseconds(_time + 300));
            DiscreteStringKeyFrame desc3 = new DiscreteStringKeyFrame("", TimeSpan.FromMilliseconds(400 * 19));

            stas.KeyFrames.Add(desc);
            stas.KeyFrames.Add(desc2);
            stas.KeyFrames.Add(desc3);

            Storyboard.SetTarget(stas, t);
            Storyboard.SetTargetProperty(stas, new PropertyPath("(Text)"));
            if(timer)
            {
                _time += 400;
            }
            St.Children.Add(stas);
        }

        private void FillStoryBoard4(int time)
        {
            DoubleAnimation db = new DoubleAnimation() { BeginTime = TimeSpan.FromMilliseconds(_time), Duration = TimeSpan.FromMilliseconds(time), From = 0, To = 1 };

            Storyboard.SetTarget(db, rectangle1);
            Storyboard.SetTargetProperty(db, new PropertyPath("(Opacity)"));
            St.Children.Add(db);

        }

        private void FillStoryBoard3(Rectangle t)
        {
            DoubleAnimation db = new DoubleAnimation() { BeginTime = TimeSpan.FromMilliseconds(_time), Duration = TimeSpan.FromMilliseconds(100),From = 0,To = 1};
            DoubleAnimation db2 = new DoubleAnimation() { BeginTime = TimeSpan.FromMilliseconds(400*22), Duration = TimeSpan.FromMilliseconds(0), From = 1, To = 0 };
            
            Console.Write(t.Name);

            Storyboard.SetTarget(db, t);
            Storyboard.SetTargetProperty(db, new PropertyPath("(Opacity)"));

            Storyboard.SetTarget(db2, t);
            Storyboard.SetTargetProperty(db2, new PropertyPath("(Opacity)"));

            //_time += 400;
            St.Children.Add(db);
            St.Children.Add(db2);
        }

        private void FillStoryBoard2(char c, TextBlock t)
        {
            ColorAnimation colani = new ColorAnimation() { BeginTime = TimeSpan.FromMilliseconds(400*19), From = Colors.LightBlue, To = Colors.Transparent, Duration = TimeSpan.FromMilliseconds(300) };
            SolidColorBrush s = new SolidColorBrush(Colors.LightBlue);

            ColorAnimation colani2 = new ColorAnimation() { BeginTime = TimeSpan.FromMilliseconds(400 * 19), From = Colors.LightBlue, To = Colors.Transparent, Duration = TimeSpan.FromMilliseconds(300) };
            SolidColorBrush s2 = new SolidColorBrush(Colors.LightBlue);

            StringAnimationUsingKeyFrames stas = new StringAnimationUsingKeyFrames();

            DiscreteStringKeyFrame desc = new DiscreteStringKeyFrame("", TimeSpan.FromMilliseconds(_time));
            DiscreteStringKeyFrame desc2 = new DiscreteStringKeyFrame(c + "", TimeSpan.FromMilliseconds(_time + 300));
            
            stas.KeyFrames.Add(desc);
            stas.KeyFrames.Add(desc2);

            t.Background = s;

            if(_settings.Action==0)
                ((TextBlock)LeftPanel.Children[RightPanel.Children.Count-1]).Background = s2;
            else
                ((TextBlock)RightPanel.Children[LeftPanel.Children.Count - 1]).Background = s2;

            Storyboard.SetTarget(stas, t);
            Storyboard.SetTargetProperty(stas, new PropertyPath("(Text)"));
            if (_settings.Action == 0)
            {
                Storyboard.SetTarget(colani2, (TextBlock) LeftPanel.Children[RightPanel.Children.Count - 1]);
                Storyboard.SetTargetProperty(colani2, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
            }

            else
            {
                Storyboard.SetTarget(colani2, (TextBlock)RightPanel.Children[LeftPanel.Children.Count - 1]);
                Storyboard.SetTargetProperty(colani2, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));

            }
            Storyboard.SetTarget(colani, t);
            Storyboard.SetTargetProperty(colani, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));

            _time += 400;
            St.Children.Add(stas);
            St.Children.Add(colani);
            St.Children.Add(colani2);
        }

        private void FillStoryBoard(int c, TextBlock t, Boolean timer)
        {
            StringAnimationUsingKeyFrames stas = new StringAnimationUsingKeyFrames();

            DiscreteStringKeyFrame desc = new DiscreteStringKeyFrame("", TimeSpan.FromMilliseconds(_time));
            DiscreteStringKeyFrame desc2 = new DiscreteStringKeyFrame(c + "", TimeSpan.FromMilliseconds(_time + 300));
            DiscreteStringKeyFrame desc3 = new DiscreteStringKeyFrame("", TimeSpan.FromMilliseconds(400*19));

            stas.KeyFrames.Add(desc);
            stas.KeyFrames.Add(desc2);
            stas.KeyFrames.Add(desc3);

            Storyboard.SetTarget(stas, t);
            Storyboard.SetTargetProperty(stas, new PropertyPath("(Text)"));

            if (timer)
            {
                _time += 400;
            }
            St.Children.Add(stas);
        }
 
        void StCompleted(object sender, EventArgs e)
        {
            Callback = false;
            ClearPresentation();
        }
        #endregion

        #region public methods

        public void ClearPresentation()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                
                textBlock6.Text = "";
                textBlock1.Text = "";
                textBlock3.Text = "";
                textBlock4.Text = "";
                textBlock5.Text = "";
                textBlock7.Text = "";
                textBlock56.Text = "";
                textBlock57.Text = "";
                textBlock58.Text = "";
                textBlock59.Text = "";
                textBlock52.Text = "";
                textBlock53.Text = "";
                textBlock54.Text = "";
                textBlock55.Text = "";
                textBlock48.Text = "";
                textBlock49.Text = "";
                textBlock50.Text = "";
                textBlock51.Text = "";
                textBlock44.Text = "";
                textBlock45.Text = "";
                textBlock46.Text = "";
                textBlock47.Text = "";
                textBlock40.Text = "";
                textBlock41.Text = "";
                textBlock42.Text = "";
                textBlock43.Text = "";
                textBlock36.Text = "";
                textBlock37.Text = "";
                textBlock38.Text = "";
                textBlock39.Text = "";
                textBlock8.Text = "";
                textBlock9.Text = "";
                textBlock10.Text = "";
                textBlock11.Text = "";
                textBlock12.Text = "";
                textBlock13.Text = "";
                textBlock14.Text = "";
                textBlock15.Text = "";
                textBlock16.Text = "";
                textBlock17.Text = "";
                textBlock18.Text = "";
                textBlock19.Text = "";
                textBlock20.Text = "";
                textBlock21.Text = "";
                textBlock22.Text = "";
                textBlock23.Text = "";
                textBlock24.Text = "";
                textBlock25.Text = "";
                textBlock26.Text = "";
                textBlock27.Text = "";
                textBlock28.Text = "";
                textBlock29.Text = "";
                textBlock30.Text = "";
                textBlock31.Text = "";
               

            }, null);
        }

        public void SetCipher(String cipher)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate
                                                                                       {
                                                                                           if (_settings.Action == 1)
                                                                                           {
                                                                                               foreach (char c in cipher)
                                                                                               {
                                                                                                   RightPanel.Children.Add(new TextBlock(){Text=c +"", FontSize = 24});
                                                                                               }
                                                                                           }
                                                                                           else
                                                                                           {
                                                                                               foreach (char c in cipher)
                                                                                               {
                                                                                                   LeftPanel.Children.Add(new TextBlock(){Text=c +"", FontSize = 24 });
                                                                                               }
                                                                                           }
                                                                                       }, null);
        }

        public void FillPresentation(int[,] s)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate
                                                                                       {
                                                                                           
                                                                                           _time = 0;
                                                                                           St = new Storyboard{SpeedRatio = SpeedRatio };
                                                                                           St.Completed += StCompleted;
                                                                                           
                                                                                           if(_settings.Action == 0)
                                                                                           {
                                                                                               FillStoryBoard((char)(s[4, 5] + 65), textBlock7,true);
                                                                                               FillStoryBoard((char)(s[4, 4] + 65), textBlock5, true);
                                                                                               FillStoryBoard((char)(s[4, 3] + 65), textBlock4, true);
                                                                                               FillStoryBoard((char)(s[4, 2] + 65), textBlock3, true);
                                                                                               FillStoryBoard((char)(s[4, 1] + 65), textBlock1, true);
                                                                                               FillStoryBoard((char)(s[4, 0] + 65), textBlock6, true);
                                                                                           }

                                                                                           else
                                                                                           {
                                                                                               FillStoryBoard((char)(s[4, 0] + 65), textBlock6, true);
                                                                                               FillStoryBoard((char)(s[4, 5] + 65), textBlock1, true);
                                                                                               FillStoryBoard((char)(s[4, 4] + 65), textBlock3, true);
                                                                                               FillStoryBoard((char)(s[4, 3] + 65), textBlock4, true);
                                                                                               FillStoryBoard((char)(s[4, 2] + 65), textBlock5, true);
                                                                                               FillStoryBoard((char)(s[4, 1] + 65), textBlock7, true);
                                                                                           }

                                                                                           if (_settings.Action == 0)
                                                                                           {
                                                                                                TextBlock t = new TextBlock() { FontSize = 24 };
                                                                                                RightPanel.Children.Add(t);
                                                                                                FillStoryBoard2((char)(s[4, 0] + 65), t);
                                                                                           }
                                                                                           else
                                                                                           {
                                                                                               TextBlock t = new TextBlock() { FontSize = 24 };
                                                                                               LeftPanel.Children.Add(t);
                                                                                               FillStoryBoard2((char)(s[4, 1] + 65), t);
                                                                                           }

                                                                                           FillStoryBoard((char)(s[0, 1] + 65), textBlock36, false);
                                                                                           FillStoryBoard((char)(s[1, 1] + 65), textBlock37, false);
                                                                                           FillStoryBoard((char)(s[2, 1] + 65), textBlock38, false);
                                                                                           FillStoryBoard((char)(s[3, 1] + 65), textBlock39, true);
                                                                                           
                                                                                           FillStoryBoard((char)(s[0, 2] + 65), textBlock40,false);
                                                                                           FillStoryBoard((char)(s[1, 2] + 65), textBlock41, false);
                                                                                           FillStoryBoard((char)(s[2, 2] + 65), textBlock42, false);
                                                                                           FillStoryBoard((char)(s[3, 2] + 65), textBlock43, true);
                                                                                           
                                                                                           FillStoryBoard((char)(s[0, 3] + 65), textBlock44,false);
                                                                                           FillStoryBoard((char)(s[1, 3] + 65), textBlock45, false);
                                                                                           FillStoryBoard((char)(s[2, 3] + 65), textBlock46, false);
                                                                                           FillStoryBoard((char)(s[3, 3] + 65), textBlock47, true);
                                                                                           
                                                                                           FillStoryBoard((char)(s[0, 4] + 65), textBlock48, false);
                                                                                           FillStoryBoard((char)(s[1, 4] + 65), textBlock49, false);
                                                                                           FillStoryBoard((char)(s[2, 4] + 65), textBlock50, false);
                                                                                           FillStoryBoard((char)(s[3, 4] + 65), textBlock51, true);

                                                                                           FillStoryBoard((char)(s[0, 5] + 65), textBlock52, false);
                                                                                           FillStoryBoard((char)(s[1, 5] + 65), textBlock53, false);
                                                                                           FillStoryBoard((char)(s[2, 5] + 65), textBlock54, false);
                                                                                           FillStoryBoard((char)(s[3, 5] + 65), textBlock55, true);

                                                                                           FillStoryBoard(s[0, 7], textBlock8,false);
                                                                                           FillStoryBoard(s[1, 7], textBlock9, false);
                                                                                           FillStoryBoard(s[2, 7], textBlock10, false);
                                                                                           FillStoryBoard(s[3, 7], textBlock11, true);
                                                                                           FillStoryBoard(s[0, 8], textBlock12, false);
                                                                                           FillStoryBoard(s[1, 8], textBlock13, false);
                                                                                           FillStoryBoard(s[2, 8], textBlock14, false);
                                                                                           FillStoryBoard(s[3, 8], textBlock15, true);
                                                                                           FillStoryBoard(s[0, 9], textBlock16, false);
                                                                                           FillStoryBoard(s[1, 9], textBlock17, false);
                                                                                           FillStoryBoard(s[2, 9], textBlock18, false);
                                                                                           FillStoryBoard(s[3, 9], textBlock19, true);

                                                                                           FillStoryBoard(s[0, 10], textBlock20, false);
                                                                                           FillStoryBoard(s[1, 10], textBlock21, false);
                                                                                           FillStoryBoard(s[2, 10], textBlock22, false);
                                                                                           FillStoryBoard(s[3, 10], textBlock23, true);
                                                                                           FillStoryBoard(s[0, 11], textBlock24, false);
                                                                                           FillStoryBoard(s[1, 11], textBlock25, false);
                                                                                           FillStoryBoard(s[2, 11], textBlock26, false);
                                                                                           FillStoryBoard(s[3, 11], textBlock27, true);
                                                                                           FillStoryBoard(s[0, 12], textBlock28, false);
                                                                                           FillStoryBoard(s[1, 12], textBlock29, false);
                                                                                           FillStoryBoard(s[2, 12], textBlock30, false);
                                                                                           FillStoryBoard(s[3, 12], textBlock31, true);

                                                                                           FillStoryBoard(s[3, 14], textBlock59, false);
                                                                                           FillStoryBoard(s[2, 14], textBlock58, false);
                                                                                           FillStoryBoard(s[1, 14], textBlock57, false);
                                                                                           FillStoryBoard(s[0, 14], textBlock56, true);

                                                                                           String sdummy = "";
                                                                                           String sdummy2 = "";

                                                                                           for (int ix = 0; ix < s.GetLength(0)-1; ix++)
                                                                                           {
                                                                                               sdummy += s[ix, 14];
                                                                                               Console.Write(s[ix, 14]);
                                                                                           }

                                                                                           sdummy2 = new String(sdummy.Distinct().ToArray());

                                                                                           foreach (char num in sdummy2)
                                                                                           {
                                                                                               FillStoryBoard3(_rarray[num - 48]);
                                                                                           }
                                                                                           _time += 400;
                                                                                           FillStoryBoard4(400);

                                                                                           if(!stopPresentation)
                                                                                           St.Begin();
                                                                                           Console.WriteLine("");
                                                                                       }, null);
        }

        public void SetInAndOutput()
        {

        }

        public void SetRotors()
        {

        }

        public void settings_OnPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            var settings = sender as SigabaSettings;

            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate
                                                                                       {
                                                                                           if (settings != null)
                                                                                           {
                                                                                               _pri0.SetType(settings.IndexRotor1);
                                                                                               _pri1.SetType(settings.IndexRotor2);
                                                                                               _pri2.SetType(settings.IndexRotor3);
                                                                                               _pri3.SetType(settings.IndexRotor4);
                                                                                               _pri4.SetType(settings.IndexRotor5);

                                                                                               _pr0.Index = settings.CipherRotor1;
                                                                                               _pr1.Index = settings.CipherRotor2;
                                                                                               _pr2.Index = settings.CipherRotor3;
                                                                                               _pr3.Index = settings.CipherRotor4;
                                                                                               _pr4.Index = settings.CipherRotor5;

                                                                                               _pr10.Index = settings.ControlRotor1;
                                                                                               _pr11.Index = settings.ControlRotor2;
                                                                                               _pr12.Index = settings.ControlRotor3;
                                                                                               _pr13.Index = settings.ControlRotor4;
                                                                                               _pr14.Index = settings.ControlRotor5;

                                                                                               _pr0.Reverse(settings.CipherRotor1Reverse);
                                                                                               _pr1.Reverse(settings.CipherRotor2Reverse);
                                                                                               _pr2.Reverse(settings.CipherRotor3Reverse);
                                                                                               _pr3.Reverse(settings.CipherRotor4Reverse);
                                                                                               _pr4.Reverse(settings.CipherRotor5Reverse);

                                                                                               _pr14.Reverse(settings.ControlRotor5Reverse);
                                                                                               _pr13.Reverse(settings.ControlRotor4Reverse);
                                                                                               _pr12.Reverse(settings.ControlRotor3Reverse);
                                                                                               _pr11.Reverse(settings.ControlRotor2Reverse);
                                                                                               _pr10.Reverse(settings.ControlRotor1Reverse);

                                                                                               _pri0.Reverse(settings.IndexRotor1Reverse);
                                                                                               _pri1.Reverse(settings.IndexRotor2Reverse);
                                                                                               _pri2.Reverse(settings.IndexRotor3Reverse);
                                                                                               _pri3.Reverse(settings.IndexRotor4Reverse);
                                                                                               _pri4.Reverse(settings.IndexRotor5Reverse);

                                                                                               _pr0.SetPosition(
                                                                                                   settings.CipherKey[0]);

                                                                                               _pr1.SetPosition(
                                                                                                   settings.CipherKey[1]);
                                                                                               _pr2.SetPosition(
                                                                                                   settings.CipherKey[2]);
                                                                                               _pr3.SetPosition(
                                                                                                   settings.CipherKey[3]);
                                                                                               _pr4.SetPosition(
                                                                                                   settings.CipherKey[4]);

                                                                                               _pr10.SetPosition(
                                                                                                   settings.ControlKey[4]);
                                                                                               _pr11.SetPosition(
                                                                                                   settings.ControlKey[3]);
                                                                                               _pr12.SetPosition(
                                                                                                   settings.ControlKey[2]);
                                                                                               _pr13.SetPosition(
                                                                                                   settings.ControlKey[1]);
                                                                                               _pr14.SetPosition(
                                                                                                   settings.ControlKey[0]);

                                                                                               _pri0.SetPosition(
                                                                                                   settings.IndexKey[0] - 48);
                                                                                               _pri1.SetPosition(
                                                                                                   settings.IndexKey[1] - 48);
                                                                                               _pri2.SetPosition(
                                                                                                   settings.IndexKey[2] - 48);
                                                                                               _pri3.SetPosition(
                                                                                                   settings.IndexKey[3] - 48);
                                                                                               _pri4.SetPosition(
                                                                                                   settings.IndexKey[4] - 48);
                                                                                           }
                                                                                       }, null);


        }
        
        public void Stop()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate
                                                                                       {
                                                                                           stopPresentation = true;
                                                                                           St.Stop();
                                                                                           St.Freeze();
                                                                                           Callback = false;
                                                                                           LeftPanel.Children.Clear();
                                                                                           RightPanel.Children.Clear();
                                                                                       }, null);
        }

        #endregion

        #region misc

        private void sizeChanged(Object sender, EventArgs eventArgs)
        {
            if(this.ActualWidth<this.ActualHeight)
                this.MainCanvas.RenderTransform = new ScaleTransform(this.ActualWidth / 650, this.ActualWidth / 650);
            else
                this.MainCanvas.RenderTransform = new ScaleTransform(this.ActualHeight / 650, this.ActualHeight / 650);

        }

        private void SetSettings(SigabaSettings settings)
        {

            _pri0.SetType(settings.IndexRotor1);
            _pri1.SetType(settings.IndexRotor2);
            _pri2.SetType(settings.IndexRotor3);
            _pri3.SetType(settings.IndexRotor4);
            _pri4.SetType(settings.IndexRotor5);

            _pr0.Index = settings.CipherRotor1;
            _pr1.Index = settings.CipherRotor2;
            _pr2.Index = settings.CipherRotor3;
            _pr3.Index = settings.CipherRotor4;
            _pr4.Index = settings.CipherRotor5;

            _pr10.Index = settings.ControlRotor1;
            _pr11.Index = settings.ControlRotor2;
            _pr12.Index = settings.ControlRotor3;
            _pr13.Index = settings.ControlRotor4;
            _pr14.Index = settings.ControlRotor5;

            _pr0.Reverse(settings.CipherRotor1Reverse);
            _pr1.Reverse(settings.CipherRotor2Reverse);
            _pr2.Reverse(settings.CipherRotor3Reverse);
            _pr3.Reverse(settings.CipherRotor4Reverse);
            _pr4.Reverse(settings.CipherRotor5Reverse);

            _pr10.Reverse(settings.ControlRotor5Reverse);
            _pr11.Reverse(settings.ControlRotor4Reverse);
            _pr12.Reverse(settings.ControlRotor3Reverse);
            _pr13.Reverse(settings.ControlRotor2Reverse);
            _pr14.Reverse(settings.ControlRotor1Reverse);

            _pri0.Reverse(settings.IndexRotor1Reverse);
            _pri1.Reverse(settings.IndexRotor2Reverse);
            _pri2.Reverse(settings.IndexRotor3Reverse);
            _pri3.Reverse(settings.IndexRotor4Reverse);
            _pri4.Reverse(settings.IndexRotor5Reverse);

            _pr0.SetPosition(settings.CipherKey[0]);
            _pr1.SetPosition(settings.CipherKey[1]);
            _pr2.SetPosition(settings.CipherKey[2]);
            _pr3.SetPosition(settings.CipherKey[3]);
            _pr4.SetPosition(settings.CipherKey[4]);

            _pr14.SetPosition(settings.ControlKey[4]);
            _pr13.SetPosition(settings.ControlKey[3]);
            _pr12.SetPosition(settings.ControlKey[2]);
            _pr11.SetPosition(settings.ControlKey[1]);
            _pr10.SetPosition(settings.ControlKey[0]);

            _pri0.SetPosition(settings.IndexKey[0] - 48);
            _pri1.SetPosition(settings.IndexKey[1] - 48);
            _pri2.SetPosition(settings.IndexKey[2] - 48);
            _pri3.SetPosition(settings.IndexKey[3] - 48);
            _pri4.SetPosition(settings.IndexKey[4] - 48);
        }

        private void AddRotors(SigabaSettings settings)
        {
            _pr0 = new PresentationRotor();
            _pr1 = new PresentationRotor();
            _pr2 = new PresentationRotor();
            _pr3 = new PresentationRotor();
            _pr4 = new PresentationRotor();

            canvas1.Children.Add(_pr0);
            canvas2.Children.Add(_pr1);
            canvas3.Children.Add(_pr2);
            canvas4.Children.Add(_pr3);
            canvas5.Children.Add(_pr4);

            canvas1.MouseLeftButtonDown += Pr0MouseLeftButtonDown;
            canvas2.MouseLeftButtonDown += Pr0MouseLeftButtonDown;
            canvas3.MouseLeftButtonDown += Pr0MouseLeftButtonDown;
            canvas4.MouseLeftButtonDown += Pr0MouseLeftButtonDown;
            canvas5.MouseLeftButtonDown += Pr0MouseLeftButtonDown;
            
            textBlock60.MouseLeftButtonDown += Oring0MouseLeftButtonDown;
            textBlock61.MouseLeftButtonDown += Oring1MouseLeftButtonDown;
            
            Canvas.SetLeft(_pr0, 7);
            Canvas.SetLeft(_pr1, 7);
            Canvas.SetLeft(_pr2, 7);
            Canvas.SetLeft(_pr3, 7);
            Canvas.SetLeft(_pr4, 7);

            _pr0.RenderTransform = new ScaleTransform(1.2, 1.2);
            _pr1.RenderTransform = new ScaleTransform(1.2, 1.2);
            _pr2.RenderTransform = new ScaleTransform(1.2, 1.2);
            _pr3.RenderTransform = new ScaleTransform(1.2, 1.2);
            _pr4.RenderTransform = new ScaleTransform(1.2, 1.2);

            _pri0 = new PresentationIndex(1);
            _pri1 = new PresentationIndex(2);
            _pri2 = new PresentationIndex(3);
            _pri3 = new PresentationIndex(4);
            _pri4 = new PresentationIndex(5);

            canvas7.MouseLeftButtonDown += Pri0MouseLeftButtonDown;
            canvas8.MouseLeftButtonDown += Pri0MouseLeftButtonDown;
            canvas9.MouseLeftButtonDown += Pri0MouseLeftButtonDown;
            canvas10.MouseLeftButtonDown += Pri0MouseLeftButtonDown;
            canvas11.MouseLeftButtonDown += Pri0MouseLeftButtonDown;
            
            Canvas.SetLeft(_pri0, 6);
            Canvas.SetLeft(_pri1, 6);
            Canvas.SetLeft(_pri2, 6);
            Canvas.SetLeft(_pri3, 6);
            Canvas.SetLeft(_pri4, 6);

            _pri0.RenderTransform = new ScaleTransform(1.2, 1.2);
            _pri1.RenderTransform = new ScaleTransform(1.2, 1.2);
            _pri2.RenderTransform = new ScaleTransform(1.2, 1.2);
            _pri3.RenderTransform = new ScaleTransform(1.2, 1.2);
            _pri4.RenderTransform = new ScaleTransform(1.2, 1.2);
            
            canvas7.Children.Add(_pri0);
            canvas8.Children.Add(_pri1);
            canvas9.Children.Add(_pri2);
            canvas10.Children.Add(_pri3);
            canvas11.Children.Add(_pri4);

            _pr10 = new PresentationRotor();
            _pr11 = new PresentationRotor();
            _pr12 = new PresentationRotor();
            _pr13 = new PresentationRotor();
            _pr14 = new PresentationRotor();

            canvas6.MouseLeftButtonDown += Pr0MouseLeftButtonDown;
            canvas12.MouseLeftButtonDown += Pr0MouseLeftButtonDown;
            canvas13.MouseLeftButtonDown += Pr0MouseLeftButtonDown;
            canvas14.MouseLeftButtonDown += Pr0MouseLeftButtonDown;
            canvas15.MouseLeftButtonDown += Pr0MouseLeftButtonDown;
            
            canvas6.Children.Add(_pr14);
            canvas12.Children.Add(_pr13);
            canvas13.Children.Add(_pr12);
            canvas14.Children.Add(_pr11);
            canvas15.Children.Add(_pr10);
            
            _pr10.RenderTransform = new ScaleTransform(1.2, 1.2);
            Canvas.SetLeft(_pr10, 7);
            _pr11.RenderTransform = new ScaleTransform(1.2, 1.2);
            Canvas.SetLeft(_pr11, 7);
            _pr12.RenderTransform = new ScaleTransform(1.2, 1.2);
            Canvas.SetLeft(_pr12, 7);
            _pr13.RenderTransform = new ScaleTransform(1.2, 1.2);
            Canvas.SetLeft(_pr13, 7);
            _pr14.RenderTransform = new ScaleTransform(1.2, 1.2);
            Canvas.SetLeft(_pr14, 7);

            SetSettings(settings);
        }

        #endregion 

        #region mousebuttonevents

        void Pr0MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var canv = sender as Canvas;

            var pr = new PresentationRotor();
            if (canv != null)
                foreach (UIElement ui in canv.Children)
                {
                    if(ui is PresentationRotor)
                    {
                        pr = ui as PresentationRotor;
                    }
                }

            if(!MainCanvas.Children.Contains(_toro))
            {
                _toro = new Rotor3(new Rotor(SigabaConstants.ControlCipherRotors[pr.Index], pr.Position - 65, pr.Reversed),true);
                _toro.RenderTransform = new ScaleTransform(0.6, 0.6);
                MainCanvas.Children.Add(_toro);
            }

            else
            {
                MainCanvas.Children.Remove(_toro);
                _toro = new Rotor3(new Rotor(SigabaConstants.ControlCipherRotors[pr.Index], pr.Position - 65, pr.Reversed), true);
                _toro.RenderTransform = new ScaleTransform(0.6, 0.6);
                MainCanvas.Children.Add(_toro);
            }

            Canvas.SetRight(_toro, 525);
            Canvas.SetTop(_toro, 20);

            if (MainCanvas.Children.Contains(_toro2))
            {
                MainCanvas.Children.Remove(_toro2);
            }

            if (MainCanvas.Children.Contains(_oro))
            {
                MainCanvas.Children.Remove(_oro);
            }

            if (MainCanvas.Children.Contains(_oro2))
            {
                MainCanvas.Children.Remove(_oro2);
            }

            _toro.MouseLeftButtonUp += ToroMouseLeftButtonUp;
        }

        void OroMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainCanvas.Children.Remove(_oro);
        }

        void Oro2MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainCanvas.Children.Remove(_oro2);
        }

        void ToroMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainCanvas.Children.Remove(_toro);  
        }

        void Toro2MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainCanvas.Children.Remove(_toro2);
        }

        void Oring0MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!MainCanvas.Children.Contains(_oro))
            {
                _oro = new ORing(SigabaConstants.Transform[_settings.Type]) { RenderTransform = new ScaleTransform(0.6, 0.6) };
                MainCanvas.Children.Add(_oro);
            }

            else
            {
                MainCanvas.Children.Remove(_oro);
                _oro = new ORing(SigabaConstants.Transform[_settings.Type]) { RenderTransform = new ScaleTransform(0.6, 0.6) };
                MainCanvas.Children.Add(_oro);
            }

            if (MainCanvas.Children.Contains(_toro))
            {
                MainCanvas.Children.Remove(_toro);
            }

            if (MainCanvas.Children.Contains(_toro2))
            {
                MainCanvas.Children.Remove(_toro2);
            }

            if (MainCanvas.Children.Contains(_oro2))
            {
                MainCanvas.Children.Remove(_oro2);
            }

            Canvas.SetRight(_oro, 525);
            Canvas.SetTop(_oro, 20);

            _oro.MouseLeftButtonUp += OroMouseLeftButtonUp;
        } 

        void Oring1MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!MainCanvas.Children.Contains(_oro2))
            {
                _oro2 = new ORing2(SigabaConstants.Transform2) {RenderTransform = new ScaleTransform(0.6, 0.6)};
                MainCanvas.Children.Add(_oro2);
            }

            else
            {
                MainCanvas.Children.Remove(_oro2);
                _oro2 = new ORing2(SigabaConstants.Transform2) {RenderTransform = new ScaleTransform(0.6, 0.6)};
                MainCanvas.Children.Add(_oro2);
            }

            if (MainCanvas.Children.Contains(_toro))
            {
                MainCanvas.Children.Remove(_toro);
            }

            if (MainCanvas.Children.Contains(_toro2))
            {
                MainCanvas.Children.Remove(_toro2);
            }

            if (MainCanvas.Children.Contains(_oro))
            {
                MainCanvas.Children.Remove(_oro);
            }

            Canvas.SetRight(_oro2, 525);
            Canvas.SetTop(_oro2, 20);

            _oro2.MouseLeftButtonUp += Oro2MouseLeftButtonUp;
        }

        void Pri0MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var canv = sender as Canvas;

            var pr = new PresentationIndex(0);
            if (canv != null)
                foreach (UIElement ui in canv.Children)
                {
                    if (ui is PresentationIndex)
                    {
                        pr = ui as PresentationIndex;
                    }
                }

            if (!MainCanvas.Children.Contains(_toro2))
            {
                _toro2 = new Rotor3Index(new Rotor(SigabaConstants.IndexRotors[pr.Index], pr.Position, pr.Reversed))
                             {RenderTransform = new ScaleTransform(0.6, 0.6)};
                MainCanvas.Children.Add(_toro2);
            }

            else
            {
                MainCanvas.Children.Remove(_toro2);
                _toro2 = new Rotor3Index(new Rotor(SigabaConstants.IndexRotors[pr.Index], pr.Position, pr.Reversed))
                             {RenderTransform = new ScaleTransform(0.6, 0.6)};
                MainCanvas.Children.Add(_toro2);
            }

            if (MainCanvas.Children.Contains(_toro))
            {
                MainCanvas.Children.Remove(_toro);
            }

            if (MainCanvas.Children.Contains(_oro))
            {
                MainCanvas.Children.Remove(_oro);
            }

            if (MainCanvas.Children.Contains(_oro2))
            {
                MainCanvas.Children.Remove(_oro2);
            }


            Canvas.SetRight(_toro2, 525);
            Canvas.SetTop(_toro2, 20);

            _toro2.MouseLeftButtonUp += Toro2MouseLeftButtonUp;
        }

        #endregion

        
    }
}