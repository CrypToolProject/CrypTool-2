
using CrypTool.PluginBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace CrypTool.Enigma
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class EnigmaPresentation : UserControl
    {
        #region Variables

        private readonly Enigma Enigma;

        private int test = 0;

        public AutoResetEvent ars;

        private readonly EnigmaSettings settings;

        private TextBlock tb;
        public DisabledBool PresentationDisabled;

        private readonly Storyboard storyboard1;
        private readonly Storyboard storyboard;


        public string output;
        public event EventHandler fireLetters;
        public event EventHandler newInput;

        private string newinput = "";
        private int merken = -1;
        private Button temp = new Button();
        private bool access = true;
        private readonly DispatcherTimer dispo;
        private bool playbool = false;
        private int inputcounter = 0;
        private char outputchar;
        private string input;
        private readonly Button[] bList = new Button[26];
        private readonly Button[] tList = new Button[26];
        private readonly TextBlock[] inputList = new TextBlock[26];
        private readonly TextBlock[] outputList = new TextBlock[26];
        private readonly int[] switchlist = new int[26];
        private readonly int[] umkehrlist = Walze.getWalzeAsInt(3, 1);
        private int aktuell = -1;
        private readonly Rectangle[] leuchtenList = new Rectangle[26];
        private Canvas dummycanvas;
        private readonly Line[,] umlList = new Line[26, 3];
        private readonly List<Line[]> schalterlist = new List<Line[]>();
        private readonly List<Line[]> schalterlist2 = new List<Line[]>();
        private readonly Rectangle[] dummyrec = new Rectangle[4];
        private bool stop = false;
        private readonly bool off = false;
        private bool statoron = false;

        private double speed;
        private readonly Line[,] frombat = new Line[9, 3];
        private readonly Line[,] frombat2 = new Line[3, 2];
        private readonly Line[,] toBat = new Line[26, 6];
        private readonly Line[,] tobat1 = new Line[9, 3];
        private readonly Line[,] tobat2 = new Line[3, 2];
        private readonly Canvas rotorlocks1;
        private readonly Canvas rotorlocks2;
        private readonly Canvas rotorlocks3;
        private Canvas rotorlocks4;
        private List<TextBlock> inputtebo;
        private readonly List<TextBlock> outputtebo = new List<TextBlock>();
        private TextBlock[] textBlocksToAnimate = new TextBlock[6];
        private readonly Line[] drawLines = new Line[22];
        private readonly Rotor2[] rotorarray = new Rotor2[4];
        private Stator stator;
        private Walze walze;
        private readonly Image[] rotorimgs = new Image[8];
        private readonly Image[] walzeimgs = new Image[3];
        private Point startPoint;
        private readonly Line[] lList = new Line[26];
        private readonly List<Canvas> schalterlist3 = new List<Canvas>();
        private readonly List<Canvas> leuchtenlist3 = new List<Canvas>();
        private readonly List<UIElement> linesToAnimate = new List<UIElement>();
        private readonly List<UIElement> linesToAnimate2 = new List<UIElement>();
        private readonly List<Line> linesToThrowAway = new List<Line>();
        private readonly DoubleAnimation fadeIn = new DoubleAnimation();
        private readonly DoubleAnimation fadeOut = new DoubleAnimation();
        private readonly DoubleAnimation nop = new DoubleAnimation();
        private DoubleAnimation mydouble;
        private double timecounter = 0;
        private bool b = true;
        private bool suc = false;

        #endregion

        #region CrypTool coomunication
        private void sizeChanged(object sender, EventArgs eventArgs)
        {
            mainmainmain.RenderTransform = new ScaleTransform(ActualWidth / mainmainmain.ActualWidth,
                                                            ActualHeight / mainmainmain.ActualHeight);


        }

        private void sliderValueChanged(object sender, EventArgs args)
        {
            double tut = slider1.Value;

        }

        private void setReflector()
        {
            deleteImage(false, settings.Reflector);

            if (walze != null)
            {
                if (walzenarea.Children.Contains(walze))
                {
                    walzenarea.Children.Remove(walze);
                }
            }
            walze = new Walze(settings.Model, settings.Reflector + 1, Width, Height);
            //walze1.fast = speed * 80;
            Canvas.SetLeft(walze, 0);
            Canvas.SetTop(walze, 60);
            walzenarea.Children.Add(walze);
            walze.Cursor = Cursors.Hand;
            walze.PreviewMouseLeftButtonDown += List_PreviewMouseLeftButtonDown;
            walze.PreviewMouseMove += new MouseEventHandler(Walze_MouseMove);


        }

        private void deleteStator()
        {
            if (stator != null)
            {
                if (rotorarea.Children.Contains(stator))

                {
                    rotorarea.Children.Remove(stator);
                }
            }



            if (rotorlocks4 != null)
            {
                if (rotorarea.Children.Contains(rotorlocks4))
                {
                    rotorarea.Children.Remove(rotorlocks4);
                }
            }

            textBlocksToAnimate = new TextBlock[6];
            statoron = false;
        }


        private void setStator()
        {
            stator = new Stator(settings.Model, Width, Height);
            //walze1.fast = speed * 80;
            Canvas.SetLeft(stator, 3 * 230);
            Canvas.SetTop(stator, 0);
            rotorarea.Children.Add(stator);

            rotorlocks4 = rotorlocks();

            Canvas.SetLeft(rotorlocks4, 890);



            rotorarea.Children.Add(rotorlocks4);
            textBlocksToAnimate = new TextBlock[8];
            statoron = true;
        }

        private void setRotor(int position)
        {
            if (settings.Model == 2 || settings.Model == 3)
            {

                if (rotorarray[position] != null)
                {
                    if (rotorarea.Children.Contains(rotorarray[position]))
                    {
                        rotorarea.Children.Remove(rotorarray[position]);
                    }
                }
                Array.Clear(rotorarray, position, 0);
                switch (position)
                {
                    case 0:
                        rotorarray[position] = new Rotor2(settings.Model, settings.Rotor3 + 1, Width, Height,
                                                          settings.InitialRotorPos.ToUpper()[0] - 65, settings.Ring3);
                        //Console.WriteLine(position + "  " + settings.Rotor3 + 1 + "   " + settings.InitialRotorPos.ToUpper());
                        break;
                    case 1:
                        rotorarray[position] = new Rotor2(settings.Model, settings.Rotor2 + 1, Width, Height,
                                                          settings.InitialRotorPos.ToUpper()[1] - 65, settings.Ring2);
                        break;
                    case 2:
                        rotorarray[position] = new Rotor2(settings.Model, settings.Rotor1 + 1, Width, Height,
                                                          settings.InitialRotorPos.ToUpper()[2] - 65, settings.Ring1);
                        break;


                }

                rotorarray[position].updone += changeSettings;
                rotorarray[position].downdone += changeSettings;
                rotorarray[position].up1done += changeSettings;
                rotorarray[position].down1done += changeSettings;

                rotorarray[position].Cursor = Cursors.Hand;
                rotorarray[position].PreviewMouseLeftButtonDown += List_PreviewMouseLeftButtonDown;

                Canvas.SetLeft(rotorarray[position], position * 230);
                rotorarea.Children.Add(rotorarray[position]);
                rotorarray[position].PreviewMouseMove += new MouseEventHandler(Rotor_MouseMove);
            }
        }

        private void setImage(bool rotor, int position)
        {
            if (position > -1)
            {
                Image img1 = new Image
                {
                    Uid = position + 1 + "",
                    Height = 100,
                    Width = 50
                };
                BitmapImage bi11 = new BitmapImage();
                bi11.BeginInit();

                for (int i = 0; i < EnigmaCore.rotors.GetLength(1); i++)
                {
                    if (EnigmaCore.rotors[settings.Model, i] == "")
                    {
                        deleteImage(true, i);
                    }
                }

                for (int i = 0; i < EnigmaCore.reflectors.GetLength(1); i++)
                {
                    if (EnigmaCore.reflectors[settings.Model, i] == "")
                    {
                        deleteImage(false, i);
                    }
                }

                switch (rotor)
                {
                    case true:

                        if (dropBoxCanvas.Children.Contains(rotorimgs[position]))
                        {
                            dropBoxCanvas.Children.Remove(rotorimgs[position]);
                            Array.Clear(rotorimgs, position, 0);
                        }



                        switch (position)
                        {
                            case 0:
                                bi11.UriSource = new Uri("Images/rotor1.png", UriKind.Relative);
                                break;
                            case 1:
                                bi11.UriSource = new Uri("Images/rotor2.png", UriKind.Relative);
                                break;
                            case 2:
                                bi11.UriSource = new Uri("Images/rotor3.png", UriKind.Relative);
                                break;
                            case 3:
                                bi11.UriSource = new Uri("Images/rotor4.png", UriKind.Relative);
                                break;
                            case 4:
                                bi11.UriSource = new Uri("Images/rotor5.png", UriKind.Relative);
                                break;
                            case 5:
                                bi11.UriSource = new Uri("Images/rotor6.png", UriKind.Relative);
                                break;
                            case 6:
                                bi11.UriSource = new Uri("Images/rotor7.png", UriKind.Relative);
                                break;
                            case 7:
                                bi11.UriSource = new Uri("Images/rotor8.png", UriKind.Relative);
                                break;
                        }
                        bi11.EndInit();
                        img1.Source = bi11;
                        dropBoxCanvas.Children.Add(img1);
                        rotorimgs[position] = img1;
                        rotorimgs[position].PreviewMouseMove += Rotor_MouseMove1;
                        break;
                    case false:

                        if (dropBoxCanvasWalze.Children.Contains(walzeimgs[position]))
                        {
                            dropBoxCanvas.Children.Remove(walzeimgs[position]);
                            Array.Clear(rotorimgs, position, 0);
                        }
                        switch (position)
                        {
                            case 0:
                                bi11.UriSource = new Uri("Images/Walze1.png", UriKind.Relative);
                                break;
                            case 1:
                                bi11.UriSource = new Uri("Images/Walze2.png", UriKind.Relative);
                                break;
                            case 2:
                                bi11.UriSource = new Uri("Images/Walze3.png", UriKind.Relative);
                                break;
                        }
                        bi11.EndInit();
                        img1.Source = bi11;
                        dropBoxCanvasWalze.Children.Add(img1);
                        walzeimgs[position] = img1;
                        walzeimgs[position].PreviewMouseMove += Walze_MouseMove1;
                        break;
                }


                Canvas.SetLeft(img1, 50 * (position + 1));
                img1.Cursor = Cursors.Hand;
            }
        }

        private void deleteImage(bool rotor, int position)
        {
            if (rotor)
            {
                if (dropBoxCanvas.Children.Contains(rotorimgs[position]))
                {
                    dropBoxCanvas.Children.Remove(rotorimgs[position]);
                }
            }
            else
            {
                if (dropBoxCanvasWalze.Children.Contains(walzeimgs[position]))
                {
                    dropBoxCanvasWalze.Children.Remove(walzeimgs[position]);
                }
            }
        }

        public void settings_OnPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            EnigmaSettings settings = sender as EnigmaSettings;



            if (e.PropertyName == "Model")
            {
                if (settings.Model == 2)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {

                        mainCanvas.Visibility = Visibility.Visible;
                        for (int i = 0; i < 3; i++)
                        {
                            setRotor(i);
                        }

                        steckerbrett.IsEnabled = false;

                        setReflector();
                        setStator();

                        for (int i = 0; i < EnigmaCore.rotors.GetLength(1); i++)
                        {
                            if (EnigmaCore.rotors[2, i] == "")
                            {
                                deleteImage(true, i);
                            }
                        }

                        for (int i = 0; i < EnigmaCore.reflectors.GetLength(1); i++)
                        {
                            if (EnigmaCore.reflectors[2, i] == "")
                            {
                                deleteImage(false, i);
                            }
                        }


                    }, null);
                }
                else if (settings.Model == 3)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        mainCanvas.Visibility = Visibility.Visible;
                        for (int i = 0; i < 3; i++)
                        {
                            setRotor(i);
                        }

                        for (int i = 0; i < EnigmaCore.rotors.GetLength(1); i++)
                        {
                            if (EnigmaCore.rotors[3, i] != null)
                            {
                                if (i != settings.Rotor1 && i != settings.Rotor2 && i != settings.Rotor3)
                                {
                                    setImage(true, i);
                                }
                            }
                        }

                        for (int i = 0; i < EnigmaCore.reflectors.GetLength(1); i++)
                        {
                            if (EnigmaCore.rotors[3, i] != null)
                            {
                                if (i != settings.Reflector)
                                {
                                    setImage(false, i);
                                }
                            }
                        }

                        deleteStator();
                        setReflector();
                        steckerbrett.IsEnabled = true;
                    }, null);
                }
                else
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        mainCanvas.Visibility = Visibility.Hidden;
                    }, null);
                }

                for (int i = 0; i < EnigmaCore.rotors.GetLength(1); i++)
                {
                    if (EnigmaCore.rotors[3, i] == "")
                    {
                        deleteImage(true, i);
                    }
                }

                for (int i = 0; i < EnigmaCore.reflectors.GetLength(1); i++)
                {
                    if (EnigmaCore.reflectors[3, i] == "")
                    {
                        deleteImage(false, i);
                    }
                }
            }

            if (e.PropertyName == "PresentationSpeed")
            {

                Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    Debug.Text = "" + settings.PresentationSpeed;
                    speed = settings.PresentationSpeed;
                    storyboard1.Pause();
                    storyboard.Pause();

                    storyboard1.SetSpeedRatio(speed);
                    storyboard.SetSpeedRatio(speed);

                    storyboard1.Resume();
                    storyboard.Resume();


                }, null);

            }

            if (settings.Model == 3 || settings.Model == 2)
            {

                if (e.PropertyName == "Key" && justme && !playbool)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        setRotor(0);
                        setRotor(1);
                        setRotor(2);
                    }, null);
                }

                if (rotorarray[2] != null)
                {
                    if (e.PropertyName == "Ring1down" && justme)
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            rotorarray[2].upperclick1(null, EventArgs.Empty);
                        }, null);
                    }

                    if (e.PropertyName == "Ring1up" && justme)
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            rotorarray[2].downerclick1(null, EventArgs.Empty);
                        }, null);
                    }

                    if (e.PropertyName == "Ring1NewValue" && justme)
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            //rotorarray[2].changeoffset(settings.Key.ToUpper()[0] - 65, settings.Ring1);
                            setRotor(2);
                        }, null);
                    }
                }
                if (rotorarray[1] != null)
                {
                    if (e.PropertyName == "Ring2down" && justme)
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            rotorarray[1].upperclick1(null, EventArgs.Empty);
                        }, null);
                    }

                    if (e.PropertyName == "Ring2up" && justme)
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            rotorarray[1].downerclick1(null, EventArgs.Empty);
                        }, null);
                    }

                    if (e.PropertyName == "Ring2NewValue" && justme)
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            //rotorarray[1].changeoffset(settings.Key.ToUpper()[1] - 65, settings.Ring2);
                            setRotor(1);
                        }, null);
                    }
                }
                if (rotorarray[0] != null)
                {
                    if (e.PropertyName == "Ring3down" && justme)
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            rotorarray[0].upperclick1(null, EventArgs.Empty);
                        }, null);
                    }

                    if (e.PropertyName == "Ring3up" && justme)
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            rotorarray[0].downerclick1(null, EventArgs.Empty);
                        }, null);
                    }

                    if (e.PropertyName == "Ring3NewValue" && justme)
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            //rotorarray[0].changeoffset(settings.Key.ToUpper()[2] - 65, settings.Ring3);
                            setRotor(0);
                        }, null);
                    }
                }

                if (e.PropertyName == "Remove all Plugs" && justme)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        for (int i = 0; i < bList.Length; i++)
                        {
                            if (!bList[i].Uid.Equals(bList[i].Content.ToString()))
                            { switchbuttons(int.Parse(bList[i].Content.ToString()), int.Parse(bList[i].Uid)); }
                        }
                    }, null);
                }

                if (e.PropertyName == "PlugBoard")
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {

                        /*
                        for (int i = 0; i < bList.Length; i++)
                        {
                            if (!bList[i].Uid.Equals(bList[i].Content.ToString()))
                            {
                                switchbuttons(Int32.Parse(bList[i].Content.ToString()), Int32.Parse(bList[i].Uid));
                            }
                        }
                        for (int ix = 0; ix < bList.Length; ix++)
                        for (int i = 0; i < bList.Length; i++)
                        {
                            if (Int32.Parse(bList[i].Content + "") != settings.PlugBoard[i] - 65)
                            {
                                switchbuttons(i, settings.PlugBoard[i] - 65);
                            }
                        }*/

                        for (int i = 0; i < bList.Length; i++)
                        {



                            if (int.Parse(bList[i].Content.ToString()) != int.Parse(settings.PlugBoard[i] - 65 + ""))
                            {
                                for (int ix = 0; ix < i; ix++)
                                {
                                    if (int.Parse(bList[ix].Content.ToString()) != int.Parse(settings.PlugBoard[ix] - 65 + ""))
                                    {
                                        switchbuttons(int.Parse(bList[ix].Content.ToString()),
                                                      int.Parse(bList[ix].Uid));
                                    }
                                }
                                //switchbuttons(Int32.Parse(bList[i].Content.ToString()),Int32.Parse(bList[i].Uid));
                                switchbuttons(int.Parse(bList[i].Uid), int.Parse(bList[settings.PlugBoard[i] - 65].Uid));
                            }
                        }


                    }, null);
                }

                if (e.PropertyName == "Reflector")
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        if (walze != null)
                        {
                            setImage(false, walze.typ - 1);
                        }
                        setReflector();

                    }, null);


                }

                if (e.PropertyName == "Rotor1")
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {

                        if (rotorarray[2] != null)
                        {
                            setImage(true, rotorarray[2].map - 1);
                        }
                        setRotor(2);
                    }, null);
                }

                if (e.PropertyName == "Rotor2")
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        if (rotorarray[1] != null)
                        {
                            setImage(true, rotorarray[1].map - 1);
                        }
                        setRotor(1);
                    }, null);
                }

                if (e.PropertyName == "Rotor3")
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        if (rotorarray[0] != null)
                        {
                            setImage(true, rotorarray[0].map - 1);
                        }
                        setRotor(0);
                    }, null);
                }

                if (justme)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                   {
                       if (walze != null)
                       {
                           dropBoxCanvasWalze.Children.Remove(walzeimgs[settings.Reflector]);
                       }

                       if (rotorarray[0] != null)
                       {
                           dropBoxCanvas.Children.Remove(rotorimgs[settings.Rotor3]);
                       }

                       if (rotorarray[1] != null)
                       {
                           dropBoxCanvas.Children.Remove(rotorimgs[settings.Rotor2]);
                       }

                       if (rotorarray[2] != null)
                       {
                           dropBoxCanvas.Children.Remove(rotorimgs[settings.Rotor1]);
                       }
                   }, null);
                }
                else if (e.PropertyName == "Rotor3" || e.PropertyName == "Rotor2" || e.PropertyName == "Rotor1" || e.PropertyName == "Reflector" || e.PropertyName == "Key" || e.PropertyName == "Ring3" || e.PropertyName == "Rotor1")
                {
                    justme = true;
                }
            }
        }

        #endregion

        #region Constructor
        public EnigmaPresentation(Enigma facade)
        {
            Enigma = facade;
            ars = new AutoResetEvent(false);
            storyboard = new Storyboard();
            storyboard.Completed += tasteClick2;


            PresentationDisabled = new DisabledBool();



            storyboard1 = new Storyboard();
            storyboard1.Completed += prefadeout1;

            settings = (EnigmaSettings)facade.Settings;
            speed = settings.PresentationSpeed;
            InitializeComponent();

            dropBoxCanvasWalze.AllowDrop = false;
            rotorarea.AllowDrop = false;
            walzenarea.AllowDrop = false;
            maingrid.AllowDrop = false;

            SizeChanged += sizeChanged;
            int x = 30;
            int y = 50;

            int modx = 0;
            int mody = 0;
            int modz = 0;



            for (int i = 0; i < 26; i++)
            {

                switchlist[i] = i;
                Button b = new Button();
                b.PreviewMouseLeftButtonDown += List_PreviewMouseLeftButtonDown;
                b.Click += List_MouseLeftButtonDown;
                b.Background = Brushes.LightBlue;
                b.Height = y;
                b.Width = x;
                b.PreviewMouseMove += new MouseEventHandler(List_MouseMove);
                b.Drop += List_Drop;
                b.Uid = "" + i;
                b.Content = Convert.ToChar(i + 80) + "";
                b.Content = i;
                b.Cursor = Cursors.Hand;
                b.Opacity = 1;


                steckerbrett.Children.Add(b);
                bList[i] = b;

                Line l = new Line
                {
                    Name = "l" + i,
                    X1 = x / 2 + i * x,
                    Y1 = 0,
                    X2 = x / 2 + i * x,
                    Y2 = 200
                };

                SolidColorBrush redBrush = new SolidColorBrush
                {
                    Color = Colors.Black
                };


                redBrush.Color = Colors.Black;

                TextBlock t = new TextBlock
                {
                    Text = "" + Convert.ToChar(i + 65),
                    Height = x,
                    Width = x,
                    Background = Brushes.SkyBlue,
                    TextAlignment = TextAlignment.Center
                };
                if (i % 2 == 0)
                {
                    t.Background = Brushes.LightSeaGreen;
                }

                alpha.Children.Add(t);
                TextBlock t1 = new TextBlock
                {
                    Text = Convert.ToChar(i + 65) + "",
                    Height = x,
                    Width = x
                };
                t.FontSize = 20;
                t1.FontSize = 20;
                t1.Background = Brushes.SkyBlue;
                t1.TextAlignment = TextAlignment.Center;
                if (i % 2 == 0)
                {
                    t1.Background = Brushes.LightSeaGreen;
                }

                alpha2.Children.Add(t1);
                inputList[i] = t;
                outputList[i] = t1;

                l.StrokeThickness = 1;

                l.Stroke = redBrush;

                Line l2 = new Line
                {
                    X1 = x / 2 + i * x,
                    Y1 = 0,
                    X2 = x / 2 + i * x,
                    Y2 = 20 + umkehrlist[i] * 10,

                    StrokeThickness = 1
                };

                umlList[i, 0] = l2;
                if (umkehrlist[i] < i)
                {
                    maingrid2.Children.Add(l2);
                }

                Line l3 = new Line
                {
                    X1 = x / 2 + umkehrlist[i] * x,
                    Y1 = 20 + umkehrlist[i] * 10,
                    X2 = x / 2 + i * x,
                    Y2 = 20 + umkehrlist[i] * 10,

                    StrokeThickness = 1
                };

                umlList[i, 1] = l3;
                if (umkehrlist[i] < i)
                {
                    maingrid2.Children.Add(l3);
                }

                lList[i] = l;
                maingrid.Children.Add(l);

                NameScope.GetNameScope(this).RegisterName("l" + i, l);


                Line l4 = new Line
                {
                    X1 = x / 2 + umkehrlist[i] * x,
                    Y1 = 0,
                    X2 = x / 2 + umkehrlist[i] * x,
                    Y2 = 20 + umkehrlist[i] * 10,

                    StrokeThickness = 1
                };

                umlList[i, 2] = l4;
                if (umkehrlist[i] < i)
                {
                    maingrid2.Children.Add(l4);
                }

                Button taste = new Button
                {
                    Height = 39,
                    Width = 39,
                    Uid = i + "",
                    Content = "" + Convert.ToChar(i + 65)
                };
                taste.Click += tasteClick;
                taste.FontSize = 25;
                Canvas.SetLeft(taste, 50 * i);

                if (i % 3 == 0)
                {
                    Canvas hschalter = schalter(0, i);
                    Canvas hleuchte = leuchte(0, i);

                    Canvas.SetLeft(taste, 90 * modx - 5);
                    Canvas.SetLeft(hschalter, 90 * modx - 10);
                    Canvas.SetTop(hschalter, 0);
                    Canvas.SetLeft(hleuchte, 90 * modx - 5);

                    tastaturrow0.Children.Add(taste);
                    schalterrow0.Children.Add(hschalter);
                    schalterlist3.Add(hschalter);
                    leuchtenlist3.Add(hleuchte);

                    leuchten.Children.Add(hleuchte);
                    modx++;
                }

                if (i % 3 == 1)
                {
                    Canvas hschalter = schalter(1, i);
                    Canvas hleuchte = leuchte(1, i);

                    Canvas.SetLeft(taste, 90 * mody + 25);
                    Canvas.SetLeft(hschalter, 90 * mody + 20);
                    Canvas.SetLeft(hleuchte, 90 * mody + 25);

                    Canvas.SetTop(hschalter, 0);
                    Canvas.SetTop(hleuchte, 58);

                    tastaturrow1.Children.Add(taste);
                    schalterrow1.Children.Add(hschalter);
                    schalterlist3.Add(hschalter);
                    leuchtenlist3.Add(hleuchte);
                    leuchten.Children.Add(hleuchte);

                    mody++;

                }
                if (i % 3 == 2)
                {
                    Canvas hschalter = schalter(2, i);
                    Canvas hleuchte = leuchte(2, i);

                    Canvas.SetLeft(taste, 90 * modz + 55);
                    Canvas.SetLeft(hschalter, 90 * modz + 50);
                    Canvas.SetLeft(hleuchte, 90 * modz + 55);

                    Canvas.SetTop(hleuchte, 118);

                    leuchten.Children.Add(hleuchte);

                    tastaturrow2.Children.Add(taste);
                    schalterrow2.Children.Add(hschalter);
                    leuchtenlist3.Add(hleuchte);
                    schalterlist3.Add(hschalter);
                    modz++;
                }
                tList[i] = taste;
            }

            batterieMalen();

            rotorlocks1 = rotorlocks();
            rotorlocks2 = rotorlocks();
            rotorlocks3 = rotorlocks();

            Canvas.SetLeft(rotorlocks1, 200);
            Canvas.SetLeft(rotorlocks2, 430);
            Canvas.SetLeft(rotorlocks3, 660);

            rotorarea.Children.Add(rotorlocks1);
            rotorarea.Children.Add(rotorlocks2);
            rotorarea.Children.Add(rotorlocks3);

            dropBox.Drop += List_Drop2;
            dropBoxWalze.Drop += List_Drop21;

            for (int i = 0; i < 3; i++)
            {
                setRotor(i);
            }

            setReflector();


            for (int i = 0; i < 8; i++)
            {
                setImage(true, i);
            }

            for (int i = 1; i < 3; i++)
            {
                setImage(false, i);
            }

            fadeIn.From = 0.0;
            fadeIn.To = 1.0;
            fadeIn.Duration = new Duration(TimeSpan.FromMilliseconds((1000)));

            fadeOut.From = 1.0;
            fadeOut.To = 0.0;
            fadeOut.Duration = new Duration(TimeSpan.FromMilliseconds((1000)));

            dummycanvas = new Canvas();

            mainmainmain.Children.Remove(tb);
            IsEnabled = true;
            input = "";
            bList[0].Focus();


        }

        #endregion

        #region Animation starting

        public bool checkReady()
        {
            bool b = !(rotorarray[0] == null || rotorarray[1] == null || rotorarray[2] == null || walze == null);
            return b;
        }

        public void giveFeedbackAndDie()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {

                   if (playbool)
                   {
                       stopclick(this,
                               EventArgs.Empty);
                       object[] carrier =
                           new object[3];
                       carrier[0] =
                           output.Substring(0,
                                           output.
                                               Length);
                       carrier[1] = output.Length;
                       carrier[2] = output.Length;
                       fireLetters(carrier,
                                   EventArgs.Empty);
                   }
               }, null);
        }

        public void disablePresentation(bool isrunning, bool isvisible)
        {

            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {

                if (isrunning)
                {
                    stopclick(this, null);
                    walzenarea.IsEnabled = false;
                    rotorarea.IsEnabled = false;
                    mainmain.IsEnabled = false;

                    if (!IsVisible && !mainmainmain.Children.Contains(tb))
                    {
                        tb = new TextBlock
                        {
                            TextWrapping = TextWrapping.Wrap,
                            Width = 2200,
                            FontSize = 180,
                            Text = Properties.Resources.Please_stop_the_Workspace_for_Presentation
                        };
                        mainmainmain.Children.Add(tb);
                    }
                }
                else
                {
                    if (mainmainmain.Children.Contains(tb))
                    {
                        mainmainmain.Children.Remove(tb);
                    }

                    IsEnabled = true;
                    walzenarea.IsEnabled = true;
                    rotorarea.IsEnabled = true;
                    mainmain.IsEnabled = true;
                }
            }, null);
        }

        private void t1_Tick()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {

                if ((input.Length + 1 != newinput.Length && !playbool && newinput != "") || newinput.Length == 1)
                {
                    stopclick(this, EventArgs.Empty);
                    input = newinput;
                    inputtebo = new List<TextBlock>();

                    for (int i = 0; i < newinput.Length; i++)
                    {
                        TextBlock t = new TextBlock
                        {
                            FontFamily = new FontFamily("Courier New"),
                            Text = newinput[i] + "",
                            FontSize = 41
                        };
                        inputPanel.Children.Add(t);
                        inputtebo.Add(t);

                    }

                    playClick(null, EventArgs.Empty);
                }


                if (newinput == "")
                {
                    stopclick(null, EventArgs.Empty);
                    input = newinput;
                }

                if (input.Length < newinput.Length)
                {
                    int pos = newinput.Length - 1;
                    for (int i = 0; i < input.Length - 1; i++)
                    {
                        if (input[i] != newinput[i])
                        {
                            pos = i;
                            break;
                        }
                    }

                    if (pos > inputcounter && input.Length == newinput.Length - 1)
                    {


                        input = newinput;
                        if (inputtebo == null)
                        {
                            inputtebo = new List<TextBlock>();
                        }
                        int foo = pos + pos / 5 - 1;

                        TextBlock t = new TextBlock
                        {
                            FontFamily = new FontFamily("Courier New"),
                            Text = newinput[pos] + "",
                            FontSize = 41
                        };

                        inputPanel.Children.Insert(pos, t);
                        inputtebo.Insert(pos, t);

                    }
                }


                if (input.Length > newinput.Length)
                {
                    int pos = input.Length - 1;

                    for (int i = 0; i < newinput.Length - 1; i++)
                    {
                        if (input[i] != newinput[i])
                        {
                            pos = i;
                            break;
                        }
                    }

                    if (pos > inputcounter && input.Length - 1 == newinput.Length)
                    {
                        input = newinput;
                        if (inputtebo == null)
                        {
                            inputtebo = new List<TextBlock>();
                        }

                        int foo = pos + pos / 5 - 1;

                        inputPanel.Children.RemoveAt(pos);
                        inputtebo.RemoveAt(pos);

                    }
                    else
                    {
                        stopclick(null, EventArgs.Empty);
                    }
                }


            }, null);
        }

        public void setinput(string input)
        {
            newinput = input;
            t1_Tick();
        }

        #endregion

        #region GUI drawing
        private Canvas rotorlocks()
        {
            Canvas temp = new Canvas();
            StackPanel stack = new StackPanel();
            Canvas.SetTop(stack, 60);
            stack.Orientation = Orientation.Vertical;
            for (int i = 0; i < 26; i++)
            {
                TextBlock t = new TextBlock
                {
                    Text = "" + Convert.ToChar(i + 65),
                    Width = 30.0,
                    Height = 29.4,


                    FontSize = 20,
                    Background = Brushes.SkyBlue,
                    TextAlignment = TextAlignment.Center
                };
                if (i % 2 == 0)
                {
                    t.Background = Brushes.LightSeaGreen;
                }

                stack.Children.Add(t);
            }
            temp.Children.Add(stack);
            return temp;
        }

        private void batterieMalen()
        {
            Line l = new Line
            {
                Stroke = Brushes.Black,
                X1 = 80,
                Y2 = 10,
                X2 = 80,
                Y1 = 68,
                Opacity = 0.3
            };

            tobat2[0, 0] = l;
            batterie.Children.Add(l);

            Line l01 = new Line
            {
                Stroke = Brushes.Black,
                X1 = 80,
                Y2 = 68,
                X2 = 80,
                Y1 = 128,
                Opacity = 0.3
            };

            tobat2[1, 0] = l01;
            batterie.Children.Add(l01);

            Line l02 = new Line
            {
                Stroke = Brushes.Black,
                X1 = 80,
                Y2 = 128,
                X2 = 80,
                Y1 = 180,
                Opacity = 0.3
            };

            tobat2[2, 0] = l02;
            batterie.Children.Add(l02);


            Line l1 = new Line
            {
                Stroke = Brushes.Black,
                X2 = 77,
                Y1 = 10,
                X1 = 80,
                Y2 = 10,
                Opacity = 0.3
            };

            tobat2[0, 1] = l1;
            batterie.Children.Add(l1);

            Line l2 = new Line
            {
                Stroke = Brushes.Black,
                X1 = 80,
                Y1 = 68,
                X2 = 80,
                Y2 = 68,
                Opacity = 0.3
            };

            tobat2[1, 1] = l2;
            batterie.Children.Add(l2);


            Line l3 = new Line
            {
                Stroke = Brushes.Black,
                X2 = 47,
                Y1 = 128,
                X1 = 80,
                Y2 = 128,
                Opacity = 0.3
            };

            tobat2[2, 1] = l3;
            batterie.Children.Add(l3);


            Line l4 = new Line
            {
                Stroke = Brushes.Black,
                X1 = 50,
                Y1 = 180,
                X2 = 110,
                Y2 = 180,
                StrokeThickness = 3
            };


            batterie.Children.Add(l4);

            Line l5 = new Line
            {
                Stroke = Brushes.Black,
                X1 = 65,
                Y1 = 190,
                X2 = 95,
                Y2 = 190,
                StrokeThickness = 3
            };


            batterie.Children.Add(l5);


            Line l6 = new Line
            {
                Stroke = Brushes.Black,
                X1 = 80,
                Y1 = 190,
                X2 = 80,
                Y2 = 274,
                Opacity = 0.3
            };

            frombat2[0, 0] = l6;
            batterie.Children.Add(l6);

            Line l61 = new Line
            {
                Stroke = Brushes.Black,
                X1 = 80,
                Y1 = 274,
                X2 = 80,
                Y2 = 363,
                Opacity = 0.3
            };

            frombat2[1, 0] = l61;
            batterie.Children.Add(l61);

            Line l62 = new Line
            {
                Stroke = Brushes.Black,
                X1 = 80,
                Y1 = 363,
                X2 = 80,
                Y2 = 452,
                Opacity = 0.3
            };

            frombat2[2, 0] = l62;
            batterie.Children.Add(l62);

            Line l70 = new Line
            {
                Stroke = Brushes.Black,
                X1 = 80,
                Y1 = 274,
                X2 = 80,
                Y2 = 274,
                Opacity = 0.3
            };

            frombat2[0, 1] = l70;
            batterie.Children.Add(l70);

            Line l71 = new Line
            {
                Stroke = Brushes.Black,
                X1 = 80,
                Y1 = 363,
                X2 = 80,
                Y2 = 363,
                Opacity = 0.3
            };

            frombat2[1, 1] = l71;
            batterie.Children.Add(l71);


            Line l72 = new Line
            {
                Stroke = Brushes.Black,
                X1 = 80,
                Y1 = 452,
                X2 = 34,
                Y2 = 452,
                Opacity = 0.3
            };

            frombat2[2, 1] = l72;
            batterie.Children.Add(l72);


        }

        private Canvas leuchte(int swint, int i)
        {
            Canvas myCanvas = new Canvas();

            Rectangle k = new Rectangle
            {
                RadiusX = 20,
                RadiusY = 20,
                Height = 40,
                Width = 40,
                Stroke = Brushes.Black
            };

            Canvas.SetTop(k, 20);
            Canvas.SetLeft(k, 3);

            leuchtenList[i] = k;
            myCanvas.Children.Add(k);

            TextBlock b = new TextBlock
            {
                Text = Convert.ToChar(i + 65) + "",
                FontSize = 25,
                Width = 40,
                TextAlignment = TextAlignment.Center
            };

            Canvas.SetTop(b, 22);
            Canvas.SetLeft(b, 3);

            myCanvas.Children.Add(b);

            Line l = new Line
            {
                Stroke = Brushes.Black,
                X1 = 23,
                Y1 = 10,
                X2 = 23,
                Y2 = 20,
                Opacity = 0.3
            };

            toBat[i, 5] = l;
            myCanvas.Children.Add(l);

            Line l1 = new Line
            {
                Stroke = Brushes.Black,
                X2 = 23,
                Y1 = 10,
                X1 = 112,
                Y2 = 10,
                Opacity = 0.3
            };

            switch (swint)
            {
                case 0:
                    tobat1[i / 3, swint] = l1;
                    break;
                case 1:
                    tobat1[i / 3, swint] = l1;
                    break;
                case 2:
                    tobat1[i / 3, swint] = l1;
                    break;
            }

            if (i == 25)
            {
                l1.X1 = 86;
            }

            myCanvas.Children.Add(l1);


            return myCanvas;
        }

        private Canvas schalter(int swint, int i)
        {
            Line[] line = new Line[2];
            Line[] line2 = new Line[6];

            Canvas myCanvas = new Canvas
            {
                Height = 40,
                Width = 40
            };
            Line l = new Line
            {
                Stroke = Brushes.Black,
                X1 = 40,
                Y1 = 5,
                X2 = 40,
                Y2 = 18
            };

            myCanvas.Children.Add(l);

            line[0] = l;
            Line ln0 = new Line
            {
                Stroke = Brushes.Black,
                X1 = 40,
                Y1 = 5,
                X2 = 25,
                Y2 = 5
            };
            myCanvas.Children.Add(ln0);

            Line ln1 = new Line
            {
                Stroke = Brushes.Black,
                X1 = 25,
                Y1 = 5,
                X2 = 25,
                Y2 = 0
            };
            myCanvas.Children.Add(ln1);

            Line l2 = new Line
            {
                Stroke = Brushes.Black,
                X1 = 40,
                Y1 = 18,
                X2 = 5,
                Y2 = 23
            };
            myCanvas.Children.Add(l2);

            line[1] = l2;
            line2[0] = l2;
            schalterlist.Add(line);


            Rectangle r = new Rectangle
            {
                Stroke = Brushes.Black,
                RadiusX = 3,
                RadiusY = 3
            };
            Canvas.SetLeft(r, 25);
            Canvas.SetTop(r, 13);
            r.Height = 7;
            r.Width = 7;


            myCanvas.Children.Add(r);



            Line l4 = new Line
            {
                Stroke = Brushes.Black,
                X2 = 28,
                Y1 = 10,
                X1 = 50,
                Y2 = 10
            };

            toBat[i, 0] = l4;
            myCanvas.Children.Add(l4);

            Line ln4 = new Line
            {
                Stroke = Brushes.Black,
                X1 = 29,
                Y2 = 14,
                X2 = 29,
                Y1 = 10
            };

            toBat[i, 1] = ln4;
            myCanvas.Children.Add(ln4);

            Line ln41 = new Line
            {
                Stroke = Brushes.Black,
                X1 = 50,
                Y2 = 10,
                X2 = 50,
                Y1 = -45
            };

            toBat[i, 2] = ln41;
            myCanvas.Children.Add(ln41);

            Line ln42 = new Line
            {
                Stroke = Brushes.Black,
                X2 = 50,
                Y2 = -45,
                X1 = 28,
                Y1 = -45
            };

            toBat[i, 3] = ln42;
            myCanvas.Children.Add(ln42);

            Line ln43 = new Line
            {
                Stroke = Brushes.Black,
                X1 = 28,
                Y2 = -45,
                X2 = 28
            };
            switch (swint)
            {
                case 0:
                    ln43.Y1 = -180;
                    break;
                case 1:
                    ln43.Y1 = -210;
                    break;
                case 2:
                    ln43.Y1 = -240;
                    break;

            }

            ln43.Opacity = 0.7;
            toBat[i, 4] = ln43;
            myCanvas.Children.Add(ln43);


            Rectangle r1 = new Rectangle
            {
                Stroke = Brushes.Black,
                RadiusX = 3,
                RadiusY = 3
            };
            Canvas.SetLeft(r1, 25);
            Canvas.SetTop(r1, 25);
            r1.Height = 7;
            r1.Width = 7;


            myCanvas.Children.Add(r1);

            Line l5 = new Line
            {
                Stroke = Brushes.Black,
                X1 = 14,
                Y1 = 28,
                X2 = 25,
                Y2 = 28
            };
            myCanvas.Children.Add(l5);
            line2[2] = l5;

            Line l6 = new Line
            {
                Stroke = Brushes.Black,
                X1 = 14,
                Y1 = 35,
                X2 = 14,
                Y2 = 28
            };
            myCanvas.Children.Add(l6);

            line2[3] = l6;
            schalterlist2.Add(line2);


            Line ln6 = new Line
            {
                Stroke = Brushes.Black,
                X1 = 104,
                Y1 = 35,
                X2 = 14,
                Y2 = 35,
                Opacity = 0.3
            };
            myCanvas.Children.Add(ln6);

            switch (swint)
            {
                case 0:
                    frombat[i / 3, swint] = ln6;
                    break;
                case 1:
                    frombat[i / 3, swint] = ln6;
                    break;
                case 2:
                    frombat[i / 3, swint] = ln6;
                    break;
            }


            if (i == 24)
            {
                ln6.X1 = 120;
            }

            if (i == 25)
            {
                ln6.X1 = 90;
            }

            Line l7 = new Line
            {
                Stroke = Brushes.Black,
                X1 = 5,
                Y1 = 23,
                X2 = 5,
                Y2 = 45
            };
            myCanvas.Children.Add(l7);
            line2[5] = l7;

            Line l8 = new Line
            {
                Stroke = Brushes.Black,
                X1 = 5,
                Y1 = 45,
                X2 = 25,
                Y2 = 45
            };
            myCanvas.Children.Add(l8);
            line2[4] = l8;

            Line l9 = new Line
            {
                Stroke = Brushes.Black,
                X1 = 25,
                Y1 = 45,
                X2 = 25
            };
            switch (swint)
            {
                case 0:
                    l9.Y2 = 250;
                    break;
                case 1:
                    l9.Y2 = 140;
                    break;
                case 2:
                    l9.Y2 = 50;
                    break;
            }
            myCanvas.Children.Add(l9);
            line2[1] = l9;
            return myCanvas;
        }

        private int DrawLines2(int fromboard)
        {
            if (!stop)
            {
                int m4 = 0;
                if (statoron)
                {
                    m4 = 228;
                }

                Line l = new Line
                {
                    Stroke = Brushes.Green,
                    X1 = fromboard * 30 + 15,
                    Y1 = -5,
                    X2 = fromboard * 30 + 15,
                    Y2 = 30.5
                };
                maingrid2.Children.Add(l);
                linesToAnimate.Add(l);
                drawLines[0] = l;


                Line l1 = new Line
                {
                    Stroke = Brushes.Green,
                    X1 = fromboard * 30 + 15,
                    Y1 = 30.5,
                    X2 = 0,
                    Y2 = 30.5
                };
                maingrid2.Children.Add(l1);


                drawLines[1] = l1;
                linesToAnimate.Add(l1);
                Line l2 = new Line
                {
                    Stroke = Brushes.Green,
                    X1 = 1300,
                    Y1 = 812,
                    X2 = 990 + m4,
                    Y2 = 812
                };
                mainmainmain.Children.Add(l2);

                drawLines[2] = l2;
                linesToAnimate.Add(l2);

                Line l3 = new Line
                {
                    Stroke = Brushes.Green,
                    X1 = 990 + m4,
                    Y1 = 812,
                    X2 = 990 + m4,
                    Y2 = 74 + rotorarray[2].maparray[fromboard, 0] * 29.5
                };
                mainmainmain.Children.Add(l3);
                linesToAnimate.Add(l3);

                drawLines[3] = l3;

                Line l4 = new Line
                {
                    Stroke = Brushes.Green,
                    X1 = 990 + m4,
                    Y1 = 74 + rotorarray[2].maparray[fromboard, 0] * 29.5,
                    X2 = 950 + m4,
                    Y2 = 74 + rotorarray[2].maparray[fromboard, 0] * 29.5
                };
                mainmainmain.Children.Add(l4);
                linesToAnimate.Add(l4);
                drawLines[4] = l4;

                int fromboardhelp = fromboard;

                StackPanel dummyLock1 = (StackPanel)rotorlocks3.Children[0];
                int ein = 0;
                if (statoron)
                {
                    dummyLock1 = (StackPanel)rotorlocks4.Children[0];
                    textBlocksToAnimate[6] = (TextBlock)dummyLock1.Children[fromboard];

                    int dummyfromboard = 0;
                    dummyfromboard = stator.mapto(fromboard);
                    fromboard = dummyfromboard;

                }
                dummyLock1 = (StackPanel)rotorlocks3.Children[0];
                textBlocksToAnimate[0] = (TextBlock)dummyLock1.Children[fromboard];
                ein = rotorarray[2].mapto(fromboard);
                dummyLock1 = (StackPanel)rotorlocks2.Children[0];
                textBlocksToAnimate[1] = (TextBlock)dummyLock1.Children[ein];

                int ein1 = 0;
                ein1 = rotorarray[1].mapto(ein);
                dummyLock1 = (StackPanel)rotorlocks1.Children[0];
                textBlocksToAnimate[2] = (TextBlock)dummyLock1.Children[ein1];

                int ein2 = 0;
                ein2 = rotorarray[0].mapto(ein1);
                int ein3 = walze.umkehrlist0(ein2, off);

                int ein4 = rotorarray[0].maptoreverse(ein3);
                dummyLock1 = (StackPanel)rotorlocks1.Children[0];
                textBlocksToAnimate[3] = (TextBlock)dummyLock1.Children[ein4];

                int ein5 = rotorarray[1].maptoreverse(ein4);

                dummyLock1 = (StackPanel)rotorlocks2.Children[0];
                textBlocksToAnimate[4] = (TextBlock)dummyLock1.Children[ein5];

                int ein6 = rotorarray[2].maptoreverse(ein5);

                dummyLock1 = (StackPanel)rotorlocks3.Children[0];
                textBlocksToAnimate[5] = (TextBlock)dummyLock1.Children[ein6];

                if (statoron)
                {
                    int dummyein6 = 0;
                    dummyein6 = stator.maptoreverse(ein6);
                    ein6 = dummyein6;

                    dummyLock1 = (StackPanel)rotorlocks4.Children[0];
                    textBlocksToAnimate[7] = (TextBlock)dummyLock1.Children[ein6];
                }


                if (ein6 > fromboardhelp)
                {
                    Line l41 = new Line
                    {
                        Stroke = Brushes.Red,
                        X2 = 950 + m4,
                        Y1 = 74 + ein6 * 29.5,
                        X1 = 970 + m4,
                        Y2 = 74 + ein6 * 29.5
                    };
                    mainmainmain.Children.Add(l41);
                    linesToAnimate2.Add(l41);
                    drawLines[17] = l41;

                    Line l31 = new Line
                    {
                        Stroke = Brushes.Red,
                        X2 = 970 + m4,
                        Y2 = 74 + ein6 * 29.5,
                        X1 = 970 + m4,
                        Y1 = 821.5
                    };
                    mainmainmain.Children.Add(l31);
                    linesToAnimate2.Add(l31);
                    drawLines[18] = l31;

                    Line l21 = new Line
                    {
                        Stroke = Brushes.Red,
                        X2 = 970 + m4,
                        Y1 = 821.5,
                        X1 = 1300,
                        Y2 = 821.5
                    };
                    mainmainmain.Children.Add(l21);
                    linesToAnimate2.Add(l21);
                    drawLines[19] = l21;

                    Line l11 = new Line
                    {
                        Stroke = Brushes.Red,
                        X2 = 0,
                        Y1 = 40.5,
                        X1 = ein6 * 30 + 15,
                        Y2 = 40.5
                    };
                    maingrid2.Children.Add(l11);
                    linesToAnimate2.Add(l11);
                    drawLines[20] = l11;

                    Line ln1 = new Line
                    {
                        Stroke = Brushes.Red,
                        X2 = ein6 * 30 + 15,
                        Y2 = 40.5,
                        X1 = ein6 * 30 + 15,
                        Y1 = -5
                    };
                    maingrid2.Children.Add(ln1);
                    linesToAnimate2.Add(ln1);
                    drawLines[21] = ln1;
                }
                else
                {
                    Line l41 = new Line
                    {
                        Stroke = Brushes.Red,
                        X1 = 1050 + m4,
                        Y1 = 74 + ein6 * 29.5,
                        X2 = 950 + m4,
                        Y2 = 74 + ein6 * 29.5
                    };
                    mainmainmain.Children.Add(l41);
                    linesToAnimate2.Add(l41);
                    drawLines[17] = l41;

                    Line l31 = new Line
                    {
                        Stroke = Brushes.Red,
                        X2 = 1050 + m4,
                        Y2 = 74 + ein6 * 29.5,
                        X1 = 1050 + m4,
                        Y1 = 800.5
                    };
                    mainmainmain.Children.Add(l31);
                    linesToAnimate2.Add(l31);
                    drawLines[18] = l31;

                    Line l21 = new Line
                    {
                        Stroke = Brushes.Red,
                        X2 = 1050 + m4,
                        Y1 = 800.5,
                        X1 = 1300,
                        Y2 = 800.5
                    };
                    mainmainmain.Children.Add(l21);
                    linesToAnimate2.Add(l21);
                    drawLines[19] = l21;

                    Line l11 = new Line
                    {
                        Stroke = Brushes.Red,
                        X2 = 0,
                        Y1 = 19.5,
                        X1 = ein6 * 30 + 15,
                        Y2 = 19.5
                    };
                    maingrid2.Children.Add(l11);
                    linesToAnimate2.Add(l11);
                    drawLines[20] = l11;

                    Line ln1 = new Line
                    {
                        Stroke = Brushes.Red,
                        X1 = ein6 * 30 + 15,
                        Y2 = 19.5,
                        X2 = ein6 * 30 + 15,
                        Y1 = -5
                    };
                    maingrid2.Children.Add(ln1);
                    linesToAnimate2.Add(ln1);
                    drawLines[21] = ln1;
                }

                foreach (UIElement l0815 in drawLines)
                {
                    if (l0815 != null)
                    {
                        l0815.Opacity = 0.0;
                    }
                }



                return ein6;
            }
            else
            {
                return 5;
            }
        }

        #endregion

        #region reset

        private void everythingblack()
        {
            storyboard.Children.Clear();
            storyboard1.Children.Clear();
            StackPanel dummyLock1 = (StackPanel)rotorlocks3.Children[0];

            foreach (TextBlock t in dummyLock1.Children)
            {
                t.Background = Brushes.LightSeaGreen;
                string s = t.Text;
                if (s[0] % 2 == 0)
                {
                    t.Background = Brushes.SkyBlue;
                }
            }
            dummyLock1 = (StackPanel)rotorlocks2.Children[0];

            foreach (TextBlock t in dummyLock1.Children)
            {
                t.Background = Brushes.LightSeaGreen;
                string s = t.Text;
                if (s[0] % 2 == 0)
                {
                    t.Background = Brushes.SkyBlue;
                }
            }
            dummyLock1 = (StackPanel)rotorlocks1.Children[0];

            foreach (TextBlock t in dummyLock1.Children)
            {
                t.Background = Brushes.LightSeaGreen;
                string s = t.Text;
                if (s[0] % 2 == 0)
                {
                    t.Background = Brushes.SkyBlue;
                }
            }

            if (rotorlocks4 != null)
            {
                dummyLock1 = (StackPanel)rotorlocks4.Children[0];

                foreach (TextBlock t in dummyLock1.Children)
                {
                    t.Background = Brushes.LightSeaGreen;
                    string s = t.Text;
                    if (s[0] % 2 == 0)
                    {
                        t.Background = Brushes.SkyBlue;
                    }
                }
            }

            foreach (Line l in linesToThrowAway)
            {

                foreach (Canvas c in leuchtenlist3)
                {
                    if (c == l.Parent)
                    {
                        c.Children.Remove(l);
                    }
                }

                if (batterie == l.Parent)
                {
                    batterie.Children.Remove(l);
                }

                foreach (Canvas c in schalterlist3)
                {
                    if (c == l.Parent)
                    {
                        c.Children.Remove(l);
                    }
                }
                if (mainmainmain == l.Parent)
                {
                    mainmainmain.Children.Remove(l);
                }
                if (maingrid == l.Parent)
                {
                    maingrid.Children.Remove(l);
                }
                if (maingrid2 == l.Parent)
                {
                    maingrid2.Children.Remove(l);
                }

                //System.GC.Collect();


            }
            linesToThrowAway.Clear();
            linesToAnimate.Clear();
            linesToAnimate2.Clear();

            if (walze != null)
            {
                walze.resetColors();
            }

            foreach (Rotor2 r in rotorarray)
            {
                if (r != null)
                {
                    r.resetColors();
                }
            }

            if (stator != null)
            {
                stator.resetColors();
            }
            if (drawLines != null)
            {
                foreach (Line l in drawLines)
                {

                    mainmainmain.Children.Remove(l);
                    maingrid2.Children.Remove(l);
                }
            }



            foreach (Rectangle r in leuchtenList)
            {
                r.Fill = Brushes.White;
            }

            foreach (Line l in toBat)
            {
                l.Stroke = Brushes.Black;
                l.StrokeThickness = 1.0;
            }

            foreach (Line l in frombat2)
            {
                l.Stroke = Brushes.Black;
                l.StrokeThickness = 1.0;
            }

            foreach (Line[] l in schalterlist2)
            {
                l[0].Stroke = Brushes.Black;
                l[1].Stroke = Brushes.Black;
                l[2].Stroke = Brushes.Black;
                l[3].Stroke = Brushes.Black;
                l[4].Stroke = Brushes.Black;
                l[5].Stroke = Brushes.Black;
                l[0].StrokeThickness = 1.0;
                l[1].StrokeThickness = 1.0;
                l[2].StrokeThickness = 1.0;
                l[3].StrokeThickness = 1.0;
                l[4].StrokeThickness = 1.0;
                l[5].StrokeThickness = 1.0;


            }

            foreach (Line l in frombat)
            {
                if (l != null)
                {
                    l.Stroke = Brushes.Black;
                    l.StrokeThickness = 1.0;
                }
            }
            foreach (Line l in tobat1)
            {
                if (l != null)
                {
                    l.Stroke = Brushes.Black;
                    l.StrokeThickness = 1.0;
                }
            }

            foreach (Line l in tobat2)
            {
                if (l != null)
                {
                    l.Stroke = Brushes.Black;
                    l.StrokeThickness = 1.0;
                }
            }


            foreach (Line[] l in schalterlist)
            {
                DoubleAnimation animax = new DoubleAnimation
                {
                    Duration = new Duration(TimeSpan.FromMilliseconds((0))),
                    From = l[0].Y2,
                    To = 18
                };
                l[0].BeginAnimation(Line.Y2Property, animax);
                l[1].BeginAnimation(Line.Y1Property, animax);
                l[0].Y2 = 18;
                l[1].Y1 = 18;
                l[0].StrokeThickness = 1.0;
                l[1].StrokeThickness = 1.0;
            }
            for (int i = 0; i < outputList.Length; i++)
            {
                outputList[i].Background = Brushes.SkyBlue;
                if (i % 2 == 0)
                {
                    outputList[i].Background = Brushes.LightSeaGreen;
                }
            }
            for (int i = 0; i < inputList.Length; i++)
            {
                inputList[i].Background = Brushes.SkyBlue;
                if (i % 2 == 0)
                {
                    inputList[i].Background = Brushes.LightSeaGreen;
                }
            }

            foreach (Line t in lList)
            {
                t.Stroke = Brushes.Black;
                t.StrokeThickness = 1.0;
            }
            foreach (Button t in bList)
            {
                t.Background = Brushes.LightBlue;
            }
        }

        #endregion

        #region buttonevents

        public void resetkey()
        {
            setRotor(0);
            setRotor(1);
            setRotor(2);

            /*
            if (rotorarray[2] != null)
            {
                if (settings.Key.ToUpper()[2] - 65>0)
                rotorarray[2].changeoffset(settings.Key.ToUpper()[2] - 65, settings.Ring1);
                
            }
            if (rotorarray[1] != null)
            {
               if (settings.Key.ToUpper()[1] - 65 > 0)
               rotorarray[1].changeoffset(settings.Key.ToUpper()[1] - 65, settings.Ring2);
             
            }
            if (rotorarray[0] != null)
            {
                if (settings.Key.ToUpper()[0] - 65 > 0)
                rotorarray[0].changeoffset(settings.Key.ToUpper()[0] - 65, settings.Ring3);
             
            }*/
        }

        public void stopclick(object sender, EventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    storyboard1.Stop();
                    storyboard.Stop();

                    resetkey();
                    stop = true;
                    inputPanel.Children.Clear();
                    outputPanel.Children.Clear();
                    inputcounter = 0;
                    everythingblack();
                    access = true;
                    input = "";
                    playbool = false;
                    newInput(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    Enigma.LogMessage(string.Format("Exception occured during stopclick-method: {0}", ex.Message), NotificationLevel.Warning);
                }
            }, null);
        }

        private void m4onClick(object sender, EventArgs e) //not functional
        {
            statoron = true;
            everythingblack();


            /* Rotor2 rotor4 = new Rotor2(settings.Model, 2, this.Width, this.Height, 0, 0);
             rotor4.Cursor = Cursors.Hand;

             Canvas.SetLeft(rotor4, 688);
             rotorarea.Children.Add(rotor4);



             rotorarray[3] = rotor4;

             Canvas.SetLeft(rotorarray[3], 688);



             */
            textBlocksToAnimate = new TextBlock[8];
        }

        private void List_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Store the mouse position
            startPoint = e.GetPosition(null);
        }

        public void tastedruecken(object sender, KeyEventArgs e)
        {
            Key k = e.Key;
            string s = k.ToString();
            int x = s[0] - 65;
            Debug.Text = s;
            if (!playbool && s.Length < 2 && k != Key.Space)
            {
                tasteClick(bList[x], EventArgs.Empty);
            }

            Debug.Text = s;
        }

        public void playClick(object sender, EventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                if (!playbool)
                {
                    timecounter = 0.0;

                    dummycanvas = new Canvas
                    {
                        Height = 1000,
                        Width = 2200,
                        Opacity = 1.0,
                        Cursor = Cursors.No,
                        Background = Brushes.Transparent
                    };

                    everythingblack();
                    ColorAnimation colorani = new ColorAnimation
                    {
                        From = Colors.Transparent,
                        To = Colors.Orange,
                        Duration = new Duration(TimeSpan.FromMilliseconds(1000)),
                        BeginTime = TimeSpan.FromMilliseconds(timecounter)
                    };
                    timecounter += 1000;
                    inputtebo[0].Background = Brushes.Transparent;
                    storyboard.Children.Add(colorani);
                    Storyboard.SetTarget(colorani, inputtebo[0]);
                    Storyboard.SetTargetProperty(colorani, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));

                    rotorarray[0].stop = false;
                    rotorarray[1].stop = false;
                    rotorarray[2].stop = false;
                    walze.stop = false;
                    stop = false;
                    int x = Convert.ToInt32(inputtebo[0].Text[0]) - 65;

                    playbool = true;
                    letterInput(bList[x], EventArgs.Empty);

                }
            }, null);

        }

        private void tasteClick(object sender, EventArgs e)
        {
            if (access)
            {

                dummycanvas = new Canvas
                {
                    Height = 1250,
                    Width = 2200,
                    Opacity = 1.0,
                    Cursor = Cursors.No,
                    Background = Brushes.Transparent
                };

                IsEnabled = false;

                timecounter = 0.0;
                stop = false;
                everythingblack();
                playbool = false;
                letterInput(sender, EventArgs.Empty);
            }
        }

        #endregion

        #region Storyboard creating

        private void prefadeout1(object sender, EventArgs e)
        {
            access = true;
            if (playbool)
            {
                if (inputtebo.Count > inputcounter && !stop)
                {
                    object[] carrier = new object[3];
                    carrier[0] = output.Substring(0, inputcounter);
                    carrier[1] = inputcounter;
                    carrier[2] = output.Length;
                    fireLetters(carrier, EventArgs.Empty);

                    everythingblack();
                    storyboard.Children.Clear();
                    timecounter = 0.0;
                    int x = Convert.ToInt32(inputtebo[inputcounter].Text[0]) - 65;
                    letterInput(bList[x], EventArgs.Empty);
                }
                if (inputtebo.Count == inputcounter && !stop)
                {
                    object[] carrier = new object[3];
                    carrier[0] = output.Substring(0, inputcounter);
                    carrier[1] = inputcounter;
                    carrier[2] = output.Length;
                    fireLetters(carrier, EventArgs.Empty);

                    everythingblack();
                    storyboard.Children.Clear();
                    timecounter = 0.0;

                }
            }
            if (!playbool)
            {
                mainmainmain.Children.Remove(tb);
                IsEnabled = true;
            }
        }

        private void letterInput(object sender, EventArgs e)
        {

            if (access)
            {
                if (true)
                {
                    if (rotorarray[0] != null && rotorarray[1] != null && rotorarray[2] != null && walze != null)
                    {

                        stop = false;
                        access = false;
                        temp = sender as Button;

                        Storyboard sb = rotorarray[2].upperclicksb(false);

                        sb.BeginTime = TimeSpan.FromMilliseconds(timecounter);

                        storyboard.Children.Add(sb);

                        if (rotorarray[2].next)
                        {
                            Storyboard sb3 = rotorarray[1].upperclicksb(false);
                            sb3.BeginTime = TimeSpan.FromMilliseconds(timecounter);
                            storyboard.Children.Add(sb3);

                        }

                        if (rotorarray[1].next)
                        {
                            if (!rotorarray[2].next)
                            {
                                Storyboard sb5 = rotorarray[1].upperclicksb(false);
                                sb5.BeginTime = TimeSpan.FromMilliseconds(timecounter);
                                storyboard.Children.Add(sb5);

                            }
                            Storyboard sb7 = rotorarray[0].upperclicksb(false);

                            sb7.BeginTime = TimeSpan.FromMilliseconds(timecounter);

                            storyboard.Children.Add(sb7);

                        }

                        Line l = new Line();
                        mainmainmain.Children.Add(l);

                        DoubleAnimation animax = new DoubleAnimation();
                        DoubleAnimation animax2 = new DoubleAnimation();

                        Line[] line = schalterlist[int.Parse(temp.Uid)];

                        animax.From = line[0].Y2;
                        animax.To = 27;

                        animax2.From = line[0].Y1;
                        animax2.To = 27;

                        animax.Duration = new Duration(TimeSpan.FromMilliseconds(1000));
                        animax2.Duration = new Duration(TimeSpan.FromMilliseconds(1000));
                        animax.BeginTime = TimeSpan.FromMilliseconds(timecounter);
                        animax2.BeginTime = TimeSpan.FromMilliseconds(timecounter);

                        storyboard.Children.Add(animax);
                        storyboard.Children.Add(animax2);

                        Storyboard.SetTarget(animax, line[0]);
                        Storyboard.SetTarget(animax2, line[1]);

                        Storyboard.SetTargetProperty(animax, new PropertyPath("(Y2)"));
                        Storyboard.SetTargetProperty(animax2, new PropertyPath("(Y1)"));


                        storyboard.Begin();
                        storyboard.SetSpeedRatio(speed);
                    }
                }
                else
                {
                    if (rotorarray[0] != null && rotorarray[1] != null && rotorarray[2] != null &&
                        rotorarray[3] != null && walze != null)
                    {
                        access = false;
                        temp = sender as Button;
                        everythingblack();
                        rotorarray[3].upperclick(sender, EventArgs.Empty);
                        if (rotorarray[3].next)
                        {

                            rotorarray[2].upperclick(sender, EventArgs.Empty);
                        }

                        if (rotorarray[2].next)
                        {
                            rotorarray[1].upperclick(sender, EventArgs.Empty);
                        }

                        if (rotorarray[1].next)
                        {
                            rotorarray[1].upperclick(sender, EventArgs.Empty);
                            rotorarray[0].upperclick(sender, EventArgs.Empty);

                        }

                        Line l = new Line();
                        mainmainmain.Children.Add(l);

                        DoubleAnimation animax = new DoubleAnimation();
                        DoubleAnimation animax2 = new DoubleAnimation();

                        Line[] line = schalterlist[int.Parse(temp.Uid)];

                        animax.From = line[0].Y2;
                        animax.To = 27;

                        animax2.From = line[0].Y1;
                        animax2.To = 27;

                        animax.Duration = new Duration(TimeSpan.FromMilliseconds((speed * 80)));
                        line[0].BeginAnimation(Line.Y2Property, animax);

                        animax2.Duration = new Duration(TimeSpan.FromMilliseconds((speed * 80)));
                        line[1].BeginAnimation(Line.Y1Property, animax);

                    }
                }
            }

        }

        private void tasteClick2(object sender, EventArgs e)
        {
            if (!stop)
            {

                Debug.Text = "" + rotorarray[2].next;



                if (rotorarray[1].next)
                {
                    if (!rotorarray[2].next)
                    {
                        Storyboard sb6 = rotorarray[1].helpupperclicksb(false);
                        sb6.Begin();
                    }

                    Storyboard sb8 = rotorarray[0].helpupperclicksb(false);
                    sb8.Begin();
                }

                if (rotorarray[2].next)
                {
                    Storyboard sb4 = rotorarray[1].helpupperclicksb(false);
                    sb4.Begin();
                }

                Storyboard sb2 = rotorarray[2].helpupperclicksb(false);
                sb2.Begin();

                Button button = temp as Button;


                for (int i = 0; i <= int.Parse(button.Uid) % 3; i++)
                {
                    frombat2[i, 0].Stroke = Brushes.Green;
                    linesToAnimate.Add(frombat2[i, 0]);
                    frombat2[i, 0].Opacity = 1.0;
                }



                frombat2[int.Parse(button.Uid) % 3, 1].Stroke = Brushes.Green;
                linesToAnimate.Add(frombat2[int.Parse(button.Uid) % 3, 1]);

                if (int.Parse(button.Uid) % 3 != 2)
                {
                    frombat[8, int.Parse(button.Uid) % 3].Stroke = Brushes.Green;
                    linesToAnimate.Add(frombat[8, int.Parse(button.Uid) % 3]);
                }

                for (int i = 7; i >= int.Parse(button.Uid) / 3; i--)
                {
                    frombat[i, int.Parse(button.Uid) % 3].Stroke = Brushes.Green;
                    linesToAnimate.Add(frombat[i, int.Parse(button.Uid) % 3]);
                }
                Line[] line = schalterlist[int.Parse(button.Uid)];
                line[0].Y2 = 27;
                line[1].Y1 = 27;

                Line[] line2 = schalterlist2[int.Parse(button.Uid)];
                line2[0].Stroke = Brushes.Green;
                line2[1].Stroke = Brushes.Green;
                line2[2].Stroke = Brushes.Green;
                line2[3].Stroke = Brushes.Green;
                line2[4].Stroke = Brushes.Green;
                line2[5].Stroke = Brushes.Green;

                linesToAnimate.Add(line2[3]);
                linesToAnimate.Add(line2[2]);
                linesToAnimate.Add(line2[0]);



                linesToAnimate.Add(line2[5]);

                linesToAnimate.Add(line2[4]);
                linesToAnimate.Add(line2[1]);

                linesToAnimate.Add(inputList[int.Parse(button.Uid)]);
                lList[switchlist[int.Parse(button.Uid)]].Stroke = Brushes.Green;
                linesToAnimate.Add(lList[switchlist[int.Parse(button.Uid)]]);

                linesToAnimate.Add(bList[switchlist[int.Parse(button.Uid)]]);

                linesToAnimate.Add(outputList[switchlist[int.Parse(button.Uid)]]);


                int aus = DrawLines2(switchlist[int.Parse(button.Uid)]);



                linesToAnimate2.Add(outputList[aus]);
                linesToAnimate2.Add(bList[aus]);


                linesToAnimate2.Add(lList[aus]);
                linesToAnimate2.Add(inputList[switchlist[aus]]);



                Line[] line3 = schalterlist2[switchlist[aus]];
                line3[0].Stroke = Brushes.Red;
                line3[1].Stroke = Brushes.Red;
                line3[4].Stroke = Brushes.Red;
                line3[5].Stroke = Brushes.Red;

                linesToAnimate2.Add(line3[1]);
                linesToAnimate2.Add(line3[4]);
                linesToAnimate2.Add(line3[5]);
                linesToAnimate2.Add(line3[0]);


                toBat[switchlist[aus], 0].Stroke = Brushes.Red;
                toBat[switchlist[aus], 1].Stroke = Brushes.Red;
                toBat[switchlist[aus], 2].Stroke = Brushes.Red;
                toBat[switchlist[aus], 3].Stroke = Brushes.Red;
                toBat[switchlist[aus], 4].Stroke = Brushes.Red;
                toBat[switchlist[aus], 5].Stroke = Brushes.Red;

                linesToAnimate2.Add(toBat[switchlist[aus], 1]);
                linesToAnimate2.Add(toBat[switchlist[aus], 0]);
                linesToAnimate2.Add(toBat[switchlist[aus], 2]);
                linesToAnimate2.Add(toBat[switchlist[aus], 3]);
                linesToAnimate2.Add(toBat[switchlist[aus], 4]);

                linesToAnimate2.Add(leuchtenList[switchlist[aus]]);
                linesToAnimate2.Add(toBat[switchlist[aus], 5]);

                outputchar = Convert.ToChar(switchlist[aus] + 65);


                List<Line> dummy = new List<Line>();


                for (int i = 7; i >= switchlist[aus] / 3; i--)
                {
                    tobat1[i, switchlist[aus] % 3].Stroke = Brushes.Red;
                    dummy.Add(tobat1[i, switchlist[aus] % 3]);

                }
                for (int i = dummy.Count - 1; i >= 0; i--)
                {
                    linesToAnimate2.Add(dummy[i]);
                }

                if (switchlist[aus] % 3 != 2)
                {
                    tobat1[8, switchlist[aus] % 3].Stroke = Brushes.Red;
                    linesToAnimate2.Add(tobat1[8, switchlist[aus] % 3]);
                }
                linesToAnimate2.Add(tobat2[switchlist[aus] % 3, 1]);
                List<Line> dummy2 = new List<Line>();
                for (int i = 2; i >= switchlist[aus] % 3; i--)
                {
                    tobat2[i, 0].Stroke = Brushes.Red;
                    dummy2.Add(tobat2[i, 0]);
                    tobat2[i, 0].Opacity = 1.0;
                }


                for (int i = dummy2.Count - 1; i >= 0; i--)
                {
                    linesToAnimate2.Add(dummy2[i]);
                }

                tobat2[switchlist[aus] % 3, 1].Stroke = Brushes.Red;


                lList[aus].Stroke = Brushes.Red;



                animatelines();


            }
        }

        private void animatelines()
        {
            storyboard1.Children.Clear();
            timecounter = 0;

            for (int i = 0; i < linesToAnimate.Count; i++)
            {
                if (linesToAnimate[i] is Line)
                {
                    animateThisLine((Line)linesToAnimate[i]);
                }
                if (linesToAnimate[i] is TextBlock)
                {
                    animateThisTebo2((TextBlock)linesToAnimate[i], true);
                }
                if (linesToAnimate[i] is Button)
                {
                    animateThisTebo2((Button)linesToAnimate[i], true);
                }

            }

            if (statoron)
            {
                animateThisTebo(textBlocksToAnimate[6], true);

                Storyboard sb7 = stator.startAnimation();
                sb7.BeginTime = TimeSpan.FromMilliseconds(timecounter);
                storyboard1.Children.Add(sb7);
                timecounter += 3000;

            }

            animateThisTebo(textBlocksToAnimate[0], true);

            Storyboard sb = rotorarray[2].startAnimation();
            sb.BeginTime = TimeSpan.FromMilliseconds(timecounter);
            storyboard1.Children.Add(sb);
            timecounter += 3000;

            animateThisTebo(textBlocksToAnimate[1], true);

            Storyboard sb1 = rotorarray[1].startAnimation();
            sb1.BeginTime = TimeSpan.FromMilliseconds(timecounter);
            storyboard1.Children.Add(sb1);
            timecounter += 3000;

            animateThisTebo(textBlocksToAnimate[2], true);

            Storyboard sb2 = rotorarray[0].startAnimation();
            sb2.BeginTime = TimeSpan.FromMilliseconds(timecounter);
            storyboard1.Children.Add(sb2);
            timecounter += 3000;

            Storyboard sb3 = walze.startanimation();
            sb3.BeginTime = TimeSpan.FromMilliseconds(timecounter);
            storyboard1.Children.Add(sb3);
            timecounter += 5000;

            Storyboard sb4 = rotorarray[0].startAnimationReverse();
            sb4.BeginTime = TimeSpan.FromMilliseconds(timecounter);
            storyboard1.Children.Add(sb4);
            timecounter += 3000;

            animateThisTebo(textBlocksToAnimate[3], false);

            Storyboard sb5 = rotorarray[1].startAnimationReverse();
            sb5.BeginTime = TimeSpan.FromMilliseconds(timecounter);
            storyboard1.Children.Add(sb5);
            timecounter += 3000;

            animateThisTebo(textBlocksToAnimate[4], false);

            Storyboard sb6 = rotorarray[2].startAnimationReverse();
            sb6.BeginTime = TimeSpan.FromMilliseconds(timecounter);
            storyboard1.Children.Add(sb6);
            timecounter += 3000;

            animateThisTebo(textBlocksToAnimate[5], false);

            if (statoron)
            {


                Storyboard sb8 = stator.startAnimationReverse();
                sb8.BeginTime = TimeSpan.FromMilliseconds(timecounter);
                storyboard1.Children.Add(sb8);
                timecounter += 3000;

                animateThisTebo(textBlocksToAnimate[7], false);
            }

            for (int i = 0; i < linesToAnimate2.Count; i++)
            {
                if (linesToAnimate2[i] is Line)
                {
                    animateThisLine2((Line)linesToAnimate2[i]);
                }
                if (linesToAnimate2[i] is TextBlock)
                {
                    animateThisTebo2((TextBlock)linesToAnimate2[i], false);
                }
                if (linesToAnimate2[i] is Button)
                {
                    animateThisTebo2((Button)linesToAnimate2[i], false);
                }
                if (linesToAnimate2[i] is Rectangle)
                {
                    animateThisTebo2((Rectangle)linesToAnimate2[i], false);
                }

            }

            if (playbool)
            {
                Storyboard np = nextPlease();
                np.BeginTime = TimeSpan.FromMilliseconds(timecounter);
                storyboard1.Children.Add(np);
            }

            storyboard1.Begin();
            storyboard1.SetSpeedRatio(speed);

        }

        private void animateThisLine2(Line l)
        {

            if (!stop)
            {

                Line l1 = new Line();

                Canvas.SetLeft(l, Canvas.GetLeft(l));

                Canvas.SetTop(l, Canvas.GetTop(l));

                l1.StrokeThickness = 5.0;
                l1.Stroke = Brushes.Tomato;


                l1.X1 = l.X2;
                l1.X2 = l.X2;

                l1.Y1 = l.Y2;
                l1.Y2 = l.Y2;

                double abst = Math.Sqrt(Math.Pow(l.X2 - l.X1, 2) + Math.Pow(l.Y2 - l.Y1, 2));
                if (abst == 0)
                {
                    abst = 1;
                }

                abst += 300;
                mydouble = new DoubleAnimation
                {
                    From = l.X2,
                    To = l.X1,
                    Duration = new Duration(TimeSpan.FromMilliseconds(abst)),
                    BeginTime = TimeSpan.FromMilliseconds(timecounter)
                };


                DoubleAnimation mydouble1 = new DoubleAnimation
                {
                    From = l.Y2,
                    To = l.Y1,
                    Duration = new Duration(TimeSpan.FromMilliseconds(abst)),
                    BeginTime = TimeSpan.FromMilliseconds(timecounter)
                };
                storyboard1.Children.Add(mydouble);
                storyboard1.Children.Add(mydouble1);

                Storyboard.SetTarget(mydouble, l1);
                Storyboard.SetTarget(mydouble1, l1);

                Storyboard.SetTargetProperty(mydouble, new PropertyPath("(X2)"));
                Storyboard.SetTargetProperty(mydouble1, new PropertyPath("(Y2)"));

                timecounter += abst;


                foreach (Canvas c in schalterlist3)
                {
                    if (c == l.Parent)
                    {
                        c.Children.Add(l1);
                    }
                }

                foreach (Canvas c in leuchtenlist3)
                {
                    if (c == l.Parent)
                    {
                        c.Children.Add(l1);
                    }
                }

                if (mainmainmain == l.Parent)
                {
                    mainmainmain.Children.Add(l1);
                }
                if (maingrid == l.Parent)
                {
                    maingrid.Children.Add(l1);
                }
                if (maingrid2 == l.Parent)
                {
                    maingrid2.Children.Add(l1);
                }

                if (batterie == l.Parent)
                {
                    batterie.Children.Add(l1);
                }

                linesToThrowAway.Add(l1);
            }
        }

        private void animateThisLine(Line l)
        {



            Line l1 = new Line();

            Canvas.SetLeft(l, Canvas.GetLeft(l));

            Canvas.SetTop(l, Canvas.GetTop(l));

            l1.StrokeThickness = 5.0;
            l1.Stroke = Brushes.LawnGreen;


            l1.X1 = l.X1;
            l1.X2 = l.X1;

            l1.Y1 = l.Y1;
            l1.Y2 = l.Y1;

            DoubleAnimation mydouble = new DoubleAnimation
            {
                From = l.X1,
                To = l.X2
            };
            Debug.Text = "" + Math.Sqrt(Math.Pow(l.X2 - l.X1, 2) + Math.Pow(l.Y2 - l.Y1, 2));
            double abst = Math.Sqrt(Math.Pow(l.X2 - l.X1, 2) + Math.Pow(l.Y2 - l.Y1, 2));
            if (abst == 0)
            {
                abst = 1;
            }

            abst += 300;

            mydouble.Duration = new Duration(TimeSpan.FromMilliseconds(abst));
            mydouble.BeginTime = TimeSpan.FromMilliseconds(timecounter);


            DoubleAnimation mydouble1 = new DoubleAnimation
            {
                From = l.Y1,
                To = l.Y2,
                Duration = new Duration(TimeSpan.FromMilliseconds(abst)),
                BeginTime = TimeSpan.FromMilliseconds(timecounter)
            };

            storyboard1.Children.Add(mydouble);
            storyboard1.Children.Add(mydouble1);

            Storyboard.SetTarget(mydouble, l1);
            Storyboard.SetTarget(mydouble1, l1);

            Storyboard.SetTargetProperty(mydouble, new PropertyPath("(X2)"));
            Storyboard.SetTargetProperty(mydouble1, new PropertyPath("(Y2)"));

            timecounter += abst;

            foreach (Canvas c in schalterlist3)
            {
                if (c == l.Parent)
                {
                    c.Children.Add(l1);
                }
            }

            foreach (Canvas c in leuchtenlist3)
            {
                if (c == l.Parent)
                {
                    c.Children.Add(l1);
                }
            }

            if (mainmainmain == l.Parent)
            {
                mainmainmain.Children.Add(l1);
            }
            if (maingrid == l.Parent)
            {
                maingrid.Children.Add(l1);
            }
            if (maingrid2 == l.Parent)
            {
                maingrid2.Children.Add(l1);
            }

            if (batterie == l.Parent)
            {
                batterie.Children.Add(l1);
            }

            linesToThrowAway.Add(l1);


        }

        private void animateThisTebo2(Rectangle tebo, bool c)
        {
            if (!stop)
            {

                ColorAnimation colorAni = new ColorAnimation
                {
                    From = Colors.White,
                    To = Colors.Yellow,

                    Duration = new Duration(TimeSpan.FromMilliseconds(1000)),
                    BeginTime = TimeSpan.FromMilliseconds(timecounter)
                };

                Storyboard.SetTarget(colorAni, tebo);

                Storyboard.SetTargetProperty(colorAni, new PropertyPath("(Shape.Fill).(SolidColorBrush.Color)"));

                storyboard1.Children.Add(colorAni);

                timecounter += 1000;
            }
        }

        private void animateThisTebo2(Button tebo, bool c)
        {
            if (!stop)
            {

                ColorAnimation colorAni = new ColorAnimation
                {
                    From = Colors.SkyBlue
                };
                if (tebo.Background == Brushes.LightSeaGreen)
                {
                    colorAni.From = Colors.LightSeaGreen;
                }

                if (c)
                {
                    colorAni.To = Colors.YellowGreen;
                }
                else
                {
                    colorAni.To = Colors.Tomato;
                }

                colorAni.Duration = new Duration(TimeSpan.FromMilliseconds(1000));
                colorAni.BeginTime = TimeSpan.FromMilliseconds(timecounter);

                Storyboard.SetTarget(colorAni, tebo);

                Storyboard.SetTargetProperty(colorAni, new PropertyPath("(Button.Background).(SolidColorBrush.Color)"));

                storyboard1.Children.Add(colorAni);

                timecounter += 1000;


            }
        }

        private void animateThisTebo2(TextBlock tebo, bool c)
        {
            if (!stop)
            {

                ColorAnimation colorAni = new ColorAnimation
                {
                    From = Colors.SkyBlue
                };
                if (tebo.Background == Brushes.LightSeaGreen)
                {
                    colorAni.From = Colors.LightSeaGreen;
                }

                if (c)
                {
                    colorAni.To = Colors.YellowGreen;
                }
                else
                {
                    colorAni.To = Colors.Tomato;
                }

                colorAni.Duration = new Duration(TimeSpan.FromMilliseconds(1000));
                colorAni.BeginTime = TimeSpan.FromMilliseconds(timecounter);

                Storyboard.SetTarget(colorAni, tebo);


                Storyboard.SetTargetProperty(colorAni, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));

                storyboard1.Children.Add(colorAni);


                timecounter += 1000;

            }
        }

        private void animateThisTebo(TextBlock tebo, bool c)
        {
            if (!stop)
            {
                ColorAnimation colorAni = new ColorAnimation
                {
                    From = Colors.SkyBlue
                };
                if (tebo.Background == Brushes.LightSeaGreen)
                {
                    colorAni.From = Colors.LightSeaGreen;
                }

                if (c)
                {
                    colorAni.To = Colors.YellowGreen;
                }
                else
                {
                    colorAni.To = Colors.Tomato;
                }

                colorAni.Duration = new Duration(TimeSpan.FromMilliseconds(1000));
                colorAni.BeginTime = TimeSpan.FromMilliseconds(timecounter);


                Storyboard.SetTarget(colorAni, tebo);

                Storyboard.SetTargetProperty(colorAni, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));

                storyboard1.Children.Add(colorAni);

                timecounter += 1000;
            }
        }

        private Storyboard nextPlease()
        {
            Storyboard sbret = new Storyboard();
            int timecounterint = 0;

            Debug.Text = "" + outputchar;

            if (inputtebo.Count > inputcounter && !stop)
            {


                ColorAnimation colorani0 = new ColorAnimation
                {
                    From = Colors.Orange,
                    To = Colors.Transparent,
                    Duration = new Duration(TimeSpan.FromMilliseconds(1000)),
                    BeginTime = TimeSpan.FromMilliseconds(timecounterint)
                };
                timecounterint += 1000;

                Storyboard.SetTarget(colorani0, inputtebo[inputcounter]);
                Storyboard.SetTargetProperty(colorani0, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));

                sbret.Children.Add(colorani0);

                if ((inputcounter) % 5 == 0 && inputcounter != 0)
                {
                    TextBlock t1 = new TextBlock
                    {
                        Text = " ",
                        FontSize = 40
                    };
                    outputPanel.Children.Add(t1);

                }

                TextBlock t = new TextBlock
                {
                    Background = Brushes.Orange,
                    Opacity = 0.0,
                    Text = outputchar + "",
                    FontSize = 42,
                    FontFamily = new FontFamily("Courier New")
                };

                outputPanel.Children.Add(t);
                outputtebo.Add(t);
                DoubleAnimation fadeIn2 = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(1000)),
                    BeginTime = TimeSpan.FromMilliseconds(timecounterint)
                };
                timecounterint += 1000;

                Storyboard.SetTarget(fadeIn2, t);

                Storyboard.SetTargetProperty(fadeIn2, new PropertyPath("(Opacity)"));

                sbret.Children.Add(fadeIn2);

                ColorAnimation colorani = new ColorAnimation
                {
                    From = Colors.Orange,
                    To = Colors.Transparent,
                    Duration = new Duration(TimeSpan.FromMilliseconds(1000)),
                    BeginTime = TimeSpan.FromMilliseconds(timecounterint)
                };
                timecounterint += 1000;

                Storyboard.SetTarget(colorani, outputtebo[outputtebo.Count - 1]);
                Storyboard.SetTargetProperty(colorani, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));

                sbret.Children.Add(colorani);

                DoubleAnimation fadeOut2 = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.5,
                    Duration = new Duration(TimeSpan.FromMilliseconds(1000)),
                    BeginTime = TimeSpan.FromMilliseconds(timecounterint)
                };

                timecounterint += 1000;

                Storyboard.SetTarget(fadeOut2, inputtebo[inputcounter]);
                Storyboard.SetTargetProperty(fadeOut2, new PropertyPath("(Opacity)"));

                sbret.Children.Add(fadeOut2);
                if (inputtebo.Count > inputcounter + 1 && !stop)
                {
                    inputtebo[inputcounter + 1].Background = Brushes.Transparent;
                    ColorAnimation colorani2 = new ColorAnimation
                    {
                        From = Colors.Transparent,
                        To = Colors.Orange,
                        Duration = new Duration(TimeSpan.FromMilliseconds(1000))
                    };
                    timecounterint += 1000;

                    Storyboard.SetTarget(colorani2, inputtebo[inputcounter + 1]);
                    Storyboard.SetTargetProperty(colorani2, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));

                    sbret.Children.Add(colorani2);
                }

                inputcounter++;

            }

            return sbret;
        }

        #endregion

        #region drag and drop

        private void List_MouseLeftButtonDown(object sender, EventArgs e)
        {
            try
            {
                // Debug.Text = "test";
                everythingblack();
                if (merken == -1)
                {
                    Button button = sender as Button;
                    merken = int.Parse(button.Uid);
                    if (!button.Uid.Equals(button.Content.ToString()))
                    {
                        switchbuttons(int.Parse(button.Content.ToString()), int.Parse(button.Uid));
                    }

                    button.Background = Brushes.LawnGreen;
                }

                else
                {

                    Button button = sender as Button;
                    bList[merken].Background = Brushes.LightBlue;
                    if ((button.Content.ToString().Equals(button.Uid) ||
                         int.Parse(button.Content.ToString()).Equals(merken)))
                    {
                        switchbuttons(int.Parse(button.Uid), merken);
                        Debug.Text = "test1";
                    }

                    else
                    {
                        Debug.Text = "test1";
                        switchbuttons(int.Parse(button.Content.ToString()), merken);
                        switchbuttons(int.Parse(button.Uid), int.Parse(button.Content.ToString()));
                    }

                    //switchbuttons(Int32.Parse(button.Uid), merken);
                    merken = -1;
                }

                syncPluboardSettings();
            }
            catch (Exception ex)
            {
                Enigma.LogMessage(string.Format("Exception in List_MouseLeftButtonDown: {0}", ex.Message), NotificationLevel.Warning);
            }
        }

        private bool justme = true;
        private void List_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                // Get the current mouse position
                Point mousePos = e.GetPosition(null);
                Vector diff = startPoint - mousePos;
                Button button = sender as Button;
                Button button2 = new Button
                {
                    Width = button.Width,
                    Height = button.Height,
                    Opacity = 0.0
                };



                if (e.LeftButton == MouseButtonState.Pressed &&
                    Math.Abs(diff.X) + 4 > SystemParameters.MinimumHorizontalDragDistance &&
                    Math.Abs(diff.Y) + 4 > SystemParameters.MinimumVerticalDragDistance && button.IsPressed)
                {
                    // Get the dragged ListViewItem

                    everythingblack();
                    //lList[Int32.Parse(button.Uid)].X2 = mousePos.X;
                    // Find the data behind the ListViewItem

                    // Let's define our DragScope .. In this case it is every thing inside our main window .. 
                    DragScope = Application.Current.MainWindow.Content as FrameworkElement;
                    System.Diagnostics.Debug.Assert(DragScope != null);

                    // We enable Drag & Drop in our scope ...  We are not implementing Drop, so it is OK, but this allows us to get DragOver 
                    bool previousDrop = DragScope.AllowDrop;
                    //DragScope.AllowDrop = true;
                    maingrid.AllowDrop = true;


                    // Let's wire our usual events.. 
                    // GiveFeedback just tells it to use no standard cursors..  

                    //GiveFeedbackEventHandler feedbackhandler = new GiveFeedbackEventHandler(DragSource_GiveFeedback);
                    //this.DragSource.GiveFeedback += feedbackhandler;

                    // The DragOver event ... 
                    DragEventHandler draghandler = new DragEventHandler(Window1_DragOver);
                    DragScope.PreviewDragOver += draghandler;
                    DragScope.PreviewMouseLeftButtonUp += aktuellupdate;

                    // Drag Leave is optional, but write up explains why I like it .. 
                    //DragEventHandler dragleavehandler = new DragEventHandler(DragScope_DragLeave);
                    //DragScope.DragLeave += dragleavehandler;

                    // QueryContinue Drag goes with drag leave... 
                    //QueryContinueDragEventHandler queryhandler = new QueryContinueDragEventHandler(DragScope_QueryContinueDrag);
                    //DragScope.QueryContinueDrag += queryhandler;
                    steckerbrett.Children.Remove(button);
                    steckerbrett.Children.Insert(int.Parse(button.Uid), button2);

                    //Here we create our adorner.. 
                    _adorner = new DragAdorner(DragScope, button, true, 1, ActualWidth,
                        ActualHeight);

                    _layer = AdornerLayer.GetAdornerLayer(DragScope as Visual);
                    _layer.Add(_adorner);


                    IsDragging = true;
                    //_dragHasLeftScope = false;
                    //Finally lets drag drop 
                    if (!button.Uid.Equals(button.Content.ToString()))
                    {
                        switchbuttons(int.Parse(button.Content.ToString()), int.Parse(button.Uid));
                    }

                    aktuell = int.Parse(button.Content.ToString());

                    DataObject data = new DataObject("myFormat", button.Uid);
                    DragDropEffects de = DragDrop.DoDragDrop(maingrid, data, DragDropEffects.Move);
                    maingrid.AllowDrop = false;
                    steckerbrett.Children.Remove(button2);
                    steckerbrett.Children.Insert(int.Parse(button.Uid), button);
                    // Clean up our mess :) 
                    //DragScope.AllowDrop = previousDrop;
                    if (_adorner != null)
                    {
                        AdornerLayer.GetAdornerLayer(DragScope).Remove(_adorner);
                    }

                    _adorner = null;
                    lList[int.Parse(button.Content.ToString())].X2 = 15 + int.Parse(button.Content.ToString()) * 30;
                    lList[int.Parse(button.Content.ToString())].Y2 = 200;
                    //           DragSource.GiveFeedback -= feedbackhandler;
                    //         DragScope.DragLeave -= dragleavehandler;
                    //       DragScope.QueryContinueDrag -= queryhandler;
                    DragScope.PreviewDragOver -= draghandler;

                    syncPluboardSettings();

                    //IsDragging = false;


                    // Initialize the drag & drop operation
                    //DataObject dragData = new DataObject("myFormat", button.Uid);

                    //DragDrop.DoDragDrop(button, dragData , DragDropEffects.Move);
                }
            }
            catch (Exception ex)
            {
                Enigma.LogMessage(string.Format("Exception in List_MouseMove: {0}", ex.Message), NotificationLevel.Warning);
            }
        }

        private void Rotor_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                // Get the current mouse position
                Point mousePos = e.GetPosition(null);
                Vector diff = startPoint - mousePos;
                Rotor2 rotor = sender as Rotor2;



                if (e.LeftButton == MouseButtonState.Pressed &&
                    Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance &&
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    // Get the dragged ListViewItem

                    everythingblack();
                    //lList[Int32.Parse(button.Uid)].X2 = mousePos.X;
                    // Find the data behind the ListViewItem

                    // Let's define our DragScope .. In this case it is every thing inside our main window .. 
                    //DragScope = dropBox as FrameworkElement;
                    DragScope = Application.Current.MainWindow.Content as FrameworkElement;
                    System.Diagnostics.Debug.Assert(DragScope != null);

                    // We enable Drag & Drop in our scope ...  We are not implementing Drop, so it is OK, but this allows us to get DragOver 
                    bool previousDrop = DragScope.AllowDrop;
                    //DragScope.AllowDrop = true;

                    // Let's wire our usual events.. 
                    // GiveFeedback just tells it to use no standard cursors..  

                    //GiveFeedbackEventHandler feedbackhandler = new GiveFeedbackEventHandler(DragSource_GiveFeedback);
                    //this.DragSource.GiveFeedback += feedbackhandler;

                    // The DragOver event ... 
                    DragEventHandler draghandler = new DragEventHandler(Window1_DragOver2);
                    DragScope.PreviewDragOver += draghandler;
                    DragScope.PreviewMouseLeftButtonUp += aktuellupdate;

                    // Drag Leave is optional, but write up explains why I like it .. 
                    //DragEventHandler dragleavehandler = new DragEventHandler(DragScope_DragLeave);
                    //DragScope.DragLeave += dragleavehandler;

                    // QueryContinue Drag goes with drag leave... 
                    //QueryContinueDragEventHandler queryhandler = new QueryContinueDragEventHandler(DragScope_QueryContinueDrag);
                    //DragScope.QueryContinueDrag += queryhandler;
                    rotorarea.Children.Remove(rotor);
                    Rectangle dummy = new Rectangle
                    {
                        Width = 200,
                        Height = 900,
                        Opacity = 0.0,
                        Stroke = Brushes.Green,
                        StrokeThickness = 5,
                        Fill = Brushes.LawnGreen
                    };
                    //dummy.AllowDrop = true;
                    rotorarea.AllowDrop = true;
                    dropBoxCanvas.AllowDrop = true;

                    Canvas.SetLeft(dummy, Canvas.GetLeft(rotor));
                    int helpint = 5;
                    rotor.PreviewMouseLeftButtonDown -= List_PreviewMouseLeftButtonDown;
                    rotorarea.Children.Add(dummy);

                    if (rotorarray[0] == rotor)
                    {
                        dummy.Drop += List_Drop30;
                        dummyrec[0] = dummy;
                        rotorarray[0] = null;
                        helpint = 0;
                    }


                    if (rotorarray[1] == rotor)
                    {
                        dummy.Drop += List_Drop31;

                        dummyrec[1] = dummy;
                        rotorarray[1] = null;
                        helpint = 1;
                    }

                    if (rotorarray[2] == rotor)
                    {
                        dummy.Drop += List_Drop32;
                        dummyrec[2] = dummy;
                        rotorarray[2] = null;
                        helpint = 2;
                    }

                    if (rotorarray[3] == rotor)
                    {
                        dummy.Drop += List_Drop33;
                        dummyrec[3] = dummy;
                        rotorarray[3] = null;
                        helpint = 3;
                    }

                    //steckerbrett.Children.Insert(Int32.Parse(button.Uid), button2);

                    //Here we create our adorner.. 
                    _adorner = new DragAdorner(DragScope, rotor.iAm, true, 1, ActualWidth,
                        ActualHeight);

                    _layer = AdornerLayer.GetAdornerLayer(DragScope as Visual);
                    _layer.Add(_adorner);

                    suc = false;
                    IsDragging = true;

                    //_dragHasLeftScope = false;
                    //Finally lets drag drop 
                    //if (!button.Uid.Equals(button.Content.ToString()))
                    //  switchbuttons(Int32.Parse(button.Content.ToString()), Int32.Parse(button.Uid));


                    DataObject data = new DataObject("myFormat", rotor.returnMap() + "");
                    justme = false;
                    DragDropEffects de = DragDrop.DoDragDrop(rotorarea, data, DragDropEffects.Move);
                    justme = true;

                    Debug.Text += "k";



                    if (!suc)
                    {
                        if (0 == helpint)
                        {
                            dummy.Drop -= List_Drop30;
                            dummyrec[0] = null;
                            rotorarray[0] = rotor;
                        }

                        if (1 == helpint)
                        {
                            dummy.Drop -= List_Drop31;
                            dummyrec[1] = null;
                            rotorarray[1] = rotor;

                        }

                        if (2 == helpint)
                        {
                            dummy.Drop -= List_Drop32;
                            dummyrec[2] = null;
                            rotorarray[2] = rotor;
                        }

                        rotorarea.Children.Remove(dummy);
                        rotorarea.Children.Add(rotor);

                        dropBoxCanvas.AllowDrop = false;
                    }

                    rotorarea.AllowDrop = false;

                    // Clean up our mess :) 
                    //DragScope.AllowDrop = previousDrop;
                    if (_adorner != null)
                    {
                        AdornerLayer.GetAdornerLayer(DragScope).Remove(_adorner);
                    }

                    _adorner = null;

                    //           DragSource.GiveFeedback -= feedbackhandler;
                    //         DragScope.DragLeave -= dragleavehandler;
                    //       DragScope.QueryContinueDrag -= queryhandler;
                    DragScope.PreviewDragOver -= draghandler;

                    //IsDragging = false;


                    // Initialize the drag & drop operation
                    //DataObject dragData = new DataObject("myFormat", button.Uid);

                    //DragDrop.DoDragDrop(button, dragData , DragDropEffects.Move);

                }
            }
            catch (Exception ex)
            {
                Enigma.LogMessage(string.Format("Exception in Rotor_MouseMove: {0}", ex.Message), NotificationLevel.Warning);
            }
        }

        private void Walze_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                // Get the current mouse position
                Point mousePos = e.GetPosition(null);
                Vector diff = startPoint - mousePos;
                Walze rotor = sender as Walze;


                if (e.LeftButton == MouseButtonState.Pressed &&
                    Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance &&
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    // Get the dragged ListViewItem

                    everythingblack();
                    //lList[Int32.Parse(button.Uid)].X2 = mousePos.X;
                    // Find the data behind the ListViewItem

                    // Let's define our DragScope .. In this case it is every thing inside our main window .. 
                    DragScope = Application.Current.MainWindow.Content as FrameworkElement;
                    System.Diagnostics.Debug.Assert(DragScope != null);

                    // We enable Drag & Drop in our scope ...  We are not implementing Drop, so it is OK, but this allows us to get DragOver 
                    bool previousDrop = DragScope.AllowDrop;
                    //DragScope.AllowDrop = true;

                    // Let's wire our usual events.. 
                    // GiveFeedback just tells it to use no standard cursors..  

                    //GiveFeedbackEventHandler feedbackhandler = new GiveFeedbackEventHandler(DragSource_GiveFeedback);
                    //this.DragSource.GiveFeedback += feedbackhandler;

                    // The DragOver event ... 
                    DragEventHandler draghandler = new DragEventHandler(Window1_DragOver2);
                    DragScope.PreviewDragOver += draghandler;
                    DragScope.PreviewMouseLeftButtonUp += aktuellupdate;

                    // Drag Leave is optional, but write up explains why I like it .. 
                    //DragEventHandler dragleavehandler = new DragEventHandler(DragScope_DragLeave);
                    //DragScope.DragLeave += dragleavehandler;

                    // QueryContinue Drag goes with drag leave... 
                    //QueryContinueDragEventHandler queryhandler = new QueryContinueDragEventHandler(DragScope_QueryContinueDrag);
                    //DragScope.QueryContinueDrag += queryhandler;
                    walzenarea.Children.Remove(rotor);
                    Rectangle dummy = new Rectangle
                    {
                        Width = 260,
                        Height = 900,
                        Opacity = 0.0,
                        Stroke = Brushes.Green,
                        StrokeThickness = 5,
                        Fill = Brushes.LawnGreen
                    };
                    //dummy.AllowDrop = true;
                    walzenarea.AllowDrop = true;
                    dropBoxCanvasWalze.AllowDrop = true;

                    Canvas.SetLeft(dummy, Canvas.GetLeft(rotor));

                    rotor.PreviewMouseLeftButtonDown -= List_PreviewMouseLeftButtonDown;
                    walzenarea.Children.Add(dummy);

                    dummy.Drop += List_Drop4;
                    walze = null;

                    dummyrec[3] = dummy;

                    //Here we create our adorner.. 
                    _adorner = new DragAdorner(DragScope, rotor.iAm, true, 1, ActualWidth,
                        ActualHeight);

                    _layer = AdornerLayer.GetAdornerLayer(DragScope as Visual);
                    _layer.Add(_adorner);

                    suc = false;
                    IsDragging = true;

                    //_dragHasLeftScope = false;
                    //Finally lets drag drop 

                    DataObject data = new DataObject("myFormat", rotor.typ + "");
                    DragDropEffects de = DragDrop.DoDragDrop(rotorarea, data, DragDropEffects.Move);
                    Debug.Text += "k";



                    if (!suc)
                    {
                        dummyrec[3] = null;
                        dummy.Drop -= List_Drop4;
                        walzenarea.Children.Remove(dummy);
                        walzenarea.Children.Add(rotor);
                        dropBoxCanvas.AllowDrop = false;
                        walze = rotor;
                    }

                    // Clean up our mess :) 
                    //DragScope.AllowDrop = previousDrop;
                    if (_adorner != null)
                    {
                        AdornerLayer.GetAdornerLayer(DragScope).Remove(_adorner);
                    }

                    _adorner = null;

                    DragScope.PreviewDragOver -= draghandler;

                    //IsDragging = false;


                    // Initialize the drag & drop operation
                    //DataObject dragData = new DataObject("myFormat", button.Uid);

                    //DragDrop.DoDragDrop(button, dragData , DragDropEffects.Move);

                }
            }
            catch (Exception ex)
            {
                Enigma.LogMessage(string.Format("Exception in Walze_MouseMove: {0}", ex.Message), NotificationLevel.Warning);
            }
        }

        private void Walze_MouseMove1(object sender, MouseEventArgs e)
        {
            try
            {
                // Get the current mouse position
                Point mousePos = e.GetPosition(null);
                Vector diff = startPoint - mousePos;
                Image rotor = sender as Image;


                if (e.LeftButton == MouseButtonState.Pressed &&
                    Math.Abs(diff.X) - 4 > SystemParameters.MinimumHorizontalDragDistance &&
                    Math.Abs(diff.Y) - 4 > SystemParameters.MinimumVerticalDragDistance)
                {
                    // Get the dragged ListViewItem

                    everythingblack();
                    //lList[Int32.Parse(button.Uid)].X2 = mousePos.X;
                    // Find the data behind the ListViewItem

                    // Let's define our DragScope .. In this case it is every thing inside our main window .. 
                    DragScope = mainCanvas as FrameworkElement;
                    System.Diagnostics.Debug.Assert(DragScope != null);

                    // We enable Drag & Drop in our scope ...  We are not implementing Drop, so it is OK, but this allows us to get DragOver 
                    bool previousDrop = DragScope.AllowDrop;
                    walzenarea.AllowDrop = true;
                    dropBoxCanvasWalze.AllowDrop = true;
                    //DragScope.AllowDrop = true;

                    // Let's wire our usual events.. 
                    // GiveFeedback just tells it to use no standard cursors..  

                    //GiveFeedbackEventHandler feedbackhandler = new GiveFeedbackEventHandler(DragSource_GiveFeedback);
                    //this.DragSource.GiveFeedback += feedbackhandler;

                    // The DragOver event ... 
                    DragEventHandler draghandler = new DragEventHandler(Window1_DragOver2);
                    DragScope.PreviewDragOver += draghandler;
                    DragScope.PreviewMouseLeftButtonUp += aktuellupdate;

                    // Drag Leave is optional, but write up explains why I like it .. 
                    //DragEventHandler dragleavehandler = new DragEventHandler(DragScope_DragLeave);
                    //DragScope.DragLeave += dragleavehandler;

                    // QueryContinue Drag goes with drag leave... 
                    //QueryContinueDragEventHandler queryhandler = new QueryContinueDragEventHandler(DragScope_QueryContinueDrag);
                    //DragScope.QueryContinueDrag += queryhandler;
                    dropBoxCanvasWalze.Children.Remove(rotor);

                    //steckerbrett.Children.Insert(Int32.Parse(button.Uid), button2);

                    //Here we create our adorner.. 
                    _adorner = new DragAdorner(DragScope, rotor, true, 1, ActualWidth,
                        ActualHeight);

                    _layer = AdornerLayer.GetAdornerLayer(DragScope as Visual);
                    _layer.Add(_adorner);


                    suc = false;
                    IsDragging = true;
                    //_dragHasLeftScope = false;
                    //Finally lets drag drop 
                    //if (!button.Uid.Equals(button.Content.ToString()))
                    //  switchbuttons(Int32.Parse(button.Content.ToString()), Int32.Parse(button.Uid));



                    DataObject data = new DataObject("myFormat", rotor.Uid);
                    DragDropEffects de = DragDrop.DoDragDrop(mainmainmain, data, DragDropEffects.Move);


                    if (!suc)
                    {
                        dropBoxCanvasWalze.Children.Add(rotor);
                        rotorarea.AllowDrop = false;
                        dropBoxCanvasWalze.AllowDrop = false;
                    }

                    // Clean up our mess :) 
                    //DragScope.AllowDrop = previousDrop;
                    if (_adorner != null)
                    {
                        AdornerLayer.GetAdornerLayer(DragScope).Remove(_adorner);
                    }

                    _adorner = null;

                    //           DragSource.GiveFeedback -= feedbackhandler;
                    //         DragScope.DragLeave -= dragleavehandler;
                    //       DragScope.QueryContinueDrag -= queryhandler;
                    DragScope.PreviewDragOver -= draghandler;

                    IsDragging = false;


                    // Initialize the drag & drop operation
                    //DataObject dragData = new DataObject("myFormat", button.Uid);

                    //DragDrop.DoDragDrop(button, dragData , DragDropEffects.Move);

                }
            }
            catch (Exception ex)
            {
                Enigma.LogMessage(string.Format("Exception in Walze_MouseMove1: {0}", ex.Message),
                    NotificationLevel.Warning);
            }
        }

        private void Rotor_MouseMove1(object sender, MouseEventArgs e)
        {
            try
            {
                // Get the current mouse position
                Point mousePos = e.GetPosition(null);
                Vector diff = startPoint - mousePos;
                Image rotor = sender as Image;


                if (e.LeftButton == MouseButtonState.Pressed &&
                    Math.Abs(diff.X) - 4 > SystemParameters.MinimumHorizontalDragDistance &&
                    Math.Abs(diff.Y) - 4 > SystemParameters.MinimumVerticalDragDistance)
                {
                    // Get the dragged ListViewItem

                    everythingblack();
                    //lList[Int32.Parse(button.Uid)].X2 = mousePos.X;
                    // Find the data behind the ListViewItem

                    // Let's define our DragScope .. In this case it is every thing inside our main window .. 
                    //DragScope = rotorarea as FrameworkElement;
                    DragScope = mainCanvas as FrameworkElement;

                    System.Diagnostics.Debug.Assert(DragScope != null);

                    // We enable Drag & Drop in our scope ...  We are not implementing Drop, so it is OK, but this allows us to get DragOver 
                    bool previousDrop = DragScope.AllowDrop;
                    rotorarea.AllowDrop = true;
                    dropBoxCanvas.AllowDrop = true;
                    //DragScope.AllowDrop = true;

                    // Let's wire our usual events.. 
                    // GiveFeedback just tells it to use no standard cursors..  

                    //GiveFeedbackEventHandler feedbackhandler = new GiveFeedbackEventHandler(DragSource_GiveFeedback);
                    //this.DragSource.GiveFeedback += feedbackhandler;

                    // The DragOver event ... 
                    //DragEventHandler draghandler = new DragEventHandler(Window1_DragOver);
                    //DragScope.PreviewDragOver += draghandler;

                    DragEventHandler draghandler = new DragEventHandler(Window1_DragOver2);
                    DragScope.PreviewDragOver += draghandler;
                    DragScope.PreviewMouseLeftButtonUp += aktuellupdate;

                    // Drag Leave is optional, but write up explains why I like it .. 
                    //DragEventHandler dragleavehandler = new DragEventHandler(DragScope_DragLeave);
                    //DragScope.DragLeave += dragleavehandler;

                    // QueryContinue Drag goes with drag leave... 
                    //QueryContinueDragEventHandler queryhandler = new QueryContinueDragEventHandler(DragScope_QueryContinueDrag);
                    //DragScope.QueryContinueDrag += queryhandler;
                    dropBoxCanvas.Children.Remove(rotor);

                    //steckerbrett.Children.Insert(Int32.Parse(button.Uid), button2);

                    //Here we create our adorner.. 
                    _adorner = new DragAdorner(DragScope, rotor, true, 1, ActualWidth,
                        ActualHeight);

                    _layer = AdornerLayer.GetAdornerLayer(DragScope as Visual);
                    _layer.Add(_adorner);

                    //maingrid.AllowDrop = true;

                    suc = false;
                    IsDragging = true;
                    //_dragHasLeftScope = false;
                    //Finally lets drag drop 
                    //if (!button.Uid.Equals(button.Content.ToString()))
                    //  switchbuttons(Int32.Parse(button.Content.ToString()), Int32.Parse(button.Uid));


                    DataObject data = new DataObject("myFormat", rotor.Uid);
                    justme = false;
                    DragDropEffects de = DragDrop.DoDragDrop(mainmainmain, data, DragDropEffects.Move);



                    if (!suc)
                    {
                        dropBoxCanvas.Children.Add(rotor);
                        rotorarea.AllowDrop = false;
                        dropBoxCanvas.AllowDrop = false;
                    }

                    // Clean up our mess :) 
                    DragScope.AllowDrop = previousDrop;
                    if (_adorner != null)
                    {
                        AdornerLayer.GetAdornerLayer(DragScope).Remove(_adorner);
                    }

                    _adorner = null;

                    //           DragSource.GiveFeedback -= feedbackhandler;
                    //         DragScope.DragLeave -= dragleavehandler;
                    //       DragScope.QueryContinueDrag -= queryhandler;
                    DragScope.PreviewDragOver -= draghandler;

                    justme = true;
                    //IsDragging = false;


                    // Initialize the drag & drop operation
                    //DataObject dragData = new DataObject("myFormat", button.Uid);

                    //DragDrop.DoDragDrop(button, dragData , DragDropEffects.Move);

                }
            }
            catch (Exception ex)
            {
                Enigma.LogMessage(string.Format("Exception in Rotor_MouseMove1: {0}", ex.Message),
                    NotificationLevel.Warning);
            }
        }

        private void List_Drop(object sender, DragEventArgs e)
        {
            try
            {


                maingrid.AllowDrop = false;

                aktuell = -1;
                Button dummy = new Button();

                string uID = e.Data.GetData("myFormat") as string;
                Button button = sender as Button;
                int myInteger1 = int.Parse(uID);
                int myInteger2 = int.Parse(button.Uid);

                if (b && (button.Content.ToString().Equals(button.Uid) ||
                          int.Parse(button.Content.ToString()).Equals(myInteger1)))
                {
                    switchbuttons(myInteger1, myInteger2);
                }

                else if (b && !button.Content.ToString().Equals(button.Uid))
                {
                    switchbuttons(int.Parse(button.Content.ToString()), myInteger2);
                    switchbuttons(myInteger1, myInteger2);
                }


                else
                {
                    b = true;
                }

                syncPluboardSettings();
            }
            catch (Exception ex)
            {
                Enigma.LogMessage(string.Format("Exception in List_Drop: {0}", ex.Message), NotificationLevel.Warning);
            }
        }

        private void List_Drop2(object sender, DragEventArgs e)
        {
            try
            {
                suc = true;
                string uID = e.Data.GetData("myFormat") as string;
                dropBoxCanvas.AllowDrop = false;

                int urint = int.Parse(uID);
                setImage(true, urint - 1);
            }
            catch (Exception ex)
            {
                Enigma.LogMessage(string.Format("Exception in List_Drop2: {0}", ex.Message), NotificationLevel.Warning);
            }

        }

        private void List_Drop21(object sender, DragEventArgs e)
        {
            try
            {
                suc = true;

                string uID = e.Data.GetData("myFormat") as string;

                dropBoxCanvasWalze.AllowDrop = false;
                walzenarea.AllowDrop = false;
                int urint = int.Parse(uID);

                setImage(false, urint - 1);
            }
            catch (Exception ex)
            {
                Enigma.LogMessage(string.Format("Exception in List_Drop21: {0}", ex.Message), NotificationLevel.Warning);
            }
        }

        private void List_Drop31(object sender, DragEventArgs e)
        {
            try
            {
                suc = true;
                rotorarea.AllowDrop = false;

                dropBoxCanvas.AllowDrop = false;
                string uID = e.Data.GetData("myFormat") as string;

                int urint = int.Parse(uID);

                rotorarea.Children.Remove(dummyrec[1]);
                if (settings.Rotor2 == urint - 1)
                {
                    setRotor(1);
                }
                else
                {
                    settings.Rotor2 = urint - 1;
                }
            }
            catch (Exception ex)
            {
                Enigma.LogMessage(string.Format("Exception in List_Drop31: {0}", ex.Message), NotificationLevel.Warning);
            }

        }

        private void List_Drop32(object sender, DragEventArgs e)
        {
            try
            {
                suc = true;
                rotorarea.AllowDrop = false;
                dropBoxCanvas.AllowDrop = false;
                string uID = e.Data.GetData("myFormat") as string;

                int urint = int.Parse(uID);

                rotorarea.Children.Remove(dummyrec[2]);
                if (settings.Rotor1 == urint - 1)
                {
                    setRotor(2);
                }
                else
                {
                    settings.Rotor1 = urint - 1;
                }
            }
            catch (Exception ex)
            {
                Enigma.LogMessage(string.Format("Exception in List_Drop32: {0}", ex.Message), NotificationLevel.Warning);
            }
        }

        private void List_Drop33(object sender, DragEventArgs e)
        {
            try
            {
                suc = true;
                rotorarea.AllowDrop = false;
                dropBoxCanvas.AllowDrop = false;
                string uID = e.Data.GetData("myFormat") as string;

                int urint = int.Parse(uID);

                rotorarea.Children.Remove(dummyrec[3]);
                settings.Rotor4 = urint - 1;
            }
            catch (Exception ex)
            {
                Enigma.LogMessage(string.Format("Exception in List_Drop33: {0}", ex.Message), NotificationLevel.Warning);
            }
        }

        private void List_Drop30(object sender, DragEventArgs e)
        {
            try
            {
                suc = true;
                rotorarea.AllowDrop = false;
                dropBoxCanvas.AllowDrop = false;
                string uID = e.Data.GetData("myFormat") as string;

                int urint = int.Parse(uID);
                rotorarea.Children.Remove(dummyrec[0]);
                if (settings.Rotor3 == urint - 1)
                {
                    setRotor(0);
                }
                else
                {
                    settings.Rotor3 = urint - 1;
                }
            }
            catch (Exception ex)
            {
                Enigma.LogMessage(string.Format("Exception in List_Drop30: {0}", ex.Message), NotificationLevel.Warning);
            }
        }

        private void List_Drop4(object sender, DragEventArgs e)
        {
            try
            {
                suc = true;
                walzenarea.AllowDrop = false;
                dropBoxCanvasWalze.AllowDrop = false;
                string uID = e.Data.GetData("myFormat") as string;
                int urint = int.Parse(uID);
                walzenarea.Children.Remove(dummyrec[3]);
                if (settings.Reflector == urint - 1)
                {
                    setReflector();
                }
                else
                {
                    settings.Reflector = urint - 1;
                }
            }
            catch (Exception ex)
            {
                Enigma.LogMessage(string.Format("Exception in List_Drop4: {0}", ex.Message), NotificationLevel.Warning);
            }
        }


        private void switchbuttons(int button1, int button2)
        {
            Button dummy = new Button();
            double dummyl;

            lList[int.Parse(bList[button1].Content.ToString())].BeginAnimation(OpacityProperty, fadeOut);

            int help = switchlist[button1];
            switchlist[button1] = switchlist[button2];
            switchlist[button2] = help;

            dummyl = lList[int.Parse(bList[button1].Content.ToString())].X1;
            dummy.Content = bList[button1].Content;

            lList[int.Parse(bList[button1].Content.ToString())].X1 = lList[int.Parse(bList[button2].Content.ToString())].X1;
            lList[int.Parse(bList[button1].Content.ToString())].X2 = 15 + int.Parse(bList[button1].Content.ToString()) * 30;
            lList[int.Parse(bList[button1].Content.ToString())].Y2 = 200;
            bList[button1].Content = bList[button2].Content;

            lList[int.Parse(bList[button2].Content.ToString())].X1 = dummyl;
            lList[int.Parse(bList[button2].Content.ToString())].X2 = 15 + int.Parse(bList[button2].Content.ToString()) * 30;
            lList[int.Parse(bList[button2].Content.ToString())].Y2 = 200;
            bList[button2].Content = dummy.Content;

            lList[int.Parse(bList[button2].Content.ToString())].BeginAnimation(OpacityProperty, fadeIn);

            bList[int.Parse(bList[button2].Content.ToString())].BeginAnimation(OpacityProperty, fadeIn);

            b = true;
        }

        private void syncPluboardSettings()
        {
            test = 0;
            justme = false;
            for (int i = 0; i < switchlist.Length; i++)
            {
                switch (i)
                {
                    case 0: settings.PlugBoardA = switchlist[i]; break;
                    case 1: settings.PlugBoardB = switchlist[i]; break;
                    case 2: settings.PlugBoardC = switchlist[i]; break;
                    case 3: settings.PlugBoardD = switchlist[i]; break;
                    case 4: settings.PlugBoardE = switchlist[i]; break;
                    case 5: settings.PlugBoardF = switchlist[i]; break;
                    case 6: settings.PlugBoardG = switchlist[i]; break;
                    case 7: settings.PlugBoardH = switchlist[i]; break;
                    case 8: settings.PlugBoardI = switchlist[i]; break;
                    case 9: settings.PlugBoardJ = switchlist[i]; break;
                    case 10: settings.PlugBoardK = switchlist[i]; break;
                    case 11: settings.PlugBoardL = switchlist[i]; break;
                    case 12: settings.PlugBoardM = switchlist[i]; break;
                    case 13: settings.PlugBoardN = switchlist[i]; break;
                    case 14: settings.PlugBoardO = switchlist[i]; break;
                    case 15: settings.PlugBoardP = switchlist[i]; break;
                    case 16: settings.PlugBoardQ = switchlist[i]; break;
                    case 17: settings.PlugBoardR = switchlist[i]; break;
                    case 18: settings.PlugBoardS = switchlist[i]; break;
                    case 19: settings.PlugBoardT = switchlist[i]; break;
                    case 20: settings.PlugBoardU = switchlist[i]; break;
                    case 21: settings.PlugBoardV = switchlist[i]; break;
                    case 22: settings.PlugBoardW = switchlist[i]; break;
                    case 23: settings.PlugBoardX = switchlist[i]; break;
                    case 24: settings.PlugBoardY = switchlist[i]; break;
                    case 25: settings.PlugBoardZ = switchlist[i]; break;
                }
            }



            // workaround for race-condition should be fixed soon
            DispatcherTimer t = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 0, 20)
            };
            ;
            t.Tick += delegate (object o, System.EventArgs e)
            { t.Stop(); justme = true; };

            t.Start();
        }

        private void changeSettings(object sender, EventArgs e)
        {
            justme = false;
            Button dummy = sender as Button;

            if (rotorarray[0] != null)
            {
                if (dummy == rotorarray[0].up)
                {
                    settings.InitialRotorPos = rotorarray[0].custom.Text + "" + settings.InitialRotorPos[1] + "" + settings.InitialRotorPos[2];

                }
            }

            if (rotorarray[1] != null)
            {
                if (dummy == rotorarray[1].up)
                {
                    settings.InitialRotorPos = settings.InitialRotorPos[0] + "" + rotorarray[1].custom.Text + "" + settings.InitialRotorPos[2];
                }
            }

            if (rotorarray[2] != null)
            {
                if (dummy == rotorarray[2].up)
                {
                    settings.InitialRotorPos = settings.InitialRotorPos[0] + "" + settings.InitialRotorPos[1] + "" + rotorarray[2].custom.Text;
                }
            }

            if (rotorarray[0] != null)
            {
                if (dummy == rotorarray[0].down)
                {
                    settings.InitialRotorPos = rotorarray[0].custom.Text + "" + settings.InitialRotorPos[1] + "" + settings.InitialRotorPos[2];

                }
            }

            if (rotorarray[1] != null)
            {
                if (dummy == rotorarray[1].down)
                {
                    settings.InitialRotorPos = settings.InitialRotorPos[0] + "" + rotorarray[1].custom.Text + "" + settings.InitialRotorPos[2];
                }
            }

            if (rotorarray[2] != null)
            {
                if (dummy == rotorarray[2].down)
                {
                    settings.InitialRotorPos = settings.InitialRotorPos[0] + "" + settings.InitialRotorPos[1] + "" + rotorarray[2].custom.Text;
                }
            }

            if (rotorarray[2] != null)
            {
                if (dummy == rotorarray[2].up1)
                {

                    settings.Ring1 = int.Parse(rotorarray[2].custom2.Text);
                }
            }

            if (rotorarray[1] != null)
            {
                if (dummy == rotorarray[1].up1)
                {
                    settings.Ring2 = int.Parse(rotorarray[1].custom2.Text);
                }
            }

            if (rotorarray[0] != null)
            {
                if (dummy == rotorarray[0].up1)
                {
                    settings.Ring3 = int.Parse(rotorarray[0].custom2.Text);
                }
            }

            if (rotorarray[2] != null)
            {
                if (dummy == rotorarray[2].down1)
                {
                    settings.Ring1 = int.Parse(rotorarray[2].custom2.Text);
                }
            }

            if (rotorarray[1] != null)
            {
                if (dummy == rotorarray[1].down1)
                {
                    settings.Ring2 = int.Parse(rotorarray[1].custom2.Text);
                }
            }

            if (rotorarray[0] != null)
            {
                if (dummy == rotorarray[0].down1)
                {
                    settings.Ring3 = int.Parse(rotorarray[0].custom2.Text);
                }
            }
        }

        private Point _startPoint;
        private bool _isDragging;
        private FrameworkElement _dragScope;
        private DragAdorner _adorner = null;
        private AdornerLayer _layer;

        public FrameworkElement DragScope
        {
            get => _dragScope;
            set => _dragScope = value;
        }

        private bool IsDragging
        {
            get => _isDragging;
            set => _isDragging = value;
        }

        private void Window1_DragOver(object sender, DragEventArgs args)
        {
            try
            {
                if (_adorner != null)
                {
                    if (aktuell != -1)
                    {
                        lList[aktuell].X2 = args.GetPosition(mainmain).X * 800 / mainmain.ActualWidth;
                        lList[aktuell].Y2 =
                            args.GetPosition(mainmain).Y * 1000 / mainmain.ActualHeight -
                            520; /* 1250 / this.ActualHeight - 380 * 1250 / this.ActualHeight */
                        ;
                    }

                    _adorner.LeftOffset = args.GetPosition(DragScope).X /* - _startPoint.X */;
                    _adorner.TopOffset = args.GetPosition(DragScope).Y /* - _startPoint.Y */;

                }
            }
            catch (Exception ex)
            {
                Enigma.LogMessage(string.Format("Exception in Window1_DragOver: {0}", ex.Message), NotificationLevel.Warning);
            }
        }

        private void Window1_DragOver2(object sender, DragEventArgs args)
        {
            try
            {
                if (_adorner != null)
                {

                    _adorner.LeftOffset = args.GetPosition(DragScope).X /* - _startPoint.X */;
                    _adorner.TopOffset = args.GetPosition(DragScope).Y /* - _startPoint.Y */;

                }
            }
            catch (Exception ex)
            {
                Enigma.LogMessage(string.Format("Exception in Window1_DragOver2: {0}", ex.Message), NotificationLevel.Warning);
            }
        }

        private void aktuellupdate(object sender, MouseButtonEventArgs args)
        {
            aktuell = -1;
        }

        #endregion

    }
    public class DisabledBool : INotifyPropertyChanged
    {
        public bool disabledBoolProperty;

        public DisabledBool() { }

        public bool DisabledBoolProperty
        {
            get => disabledBoolProperty;
            set
            {
                disabledBoolProperty = value;
                OnPropertyChanged("DisabledBoolProperty");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
