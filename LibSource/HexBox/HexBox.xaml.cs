using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace HexBox
{    
    public static class Constants
    {
        public static readonly int _numberOfCells = 768; // ATTENTION: huge numbers can slow the application
        public static readonly int _helpNumberOfCells1 = (Constants._numberOfCells - 2) * 2; // Number of cells in hex
        public static readonly int _helpNumberOfCells2 = Constants._numberOfCells / 16; // Number of Cells per Row
        public static readonly double _heightOfNOR = Constants._numberOfCells / 16 * 20; // Static Resource of height for XAML Code
    }

    public partial class HexBox : UserControl
    {

        #region Private variables

        private readonly HexText _hexText;
        private readonly TextBlock _info = new TextBlock();
        private readonly long[] _mark;
        private readonly StretchText _stretchText;
        private readonly Encoding _encoding = new ASCIIEncoding();
        private int _cell;
        private int _cellText;
        private long _cursorposition;
        private long _cursorpositionText;
        private DynamicFileByteProvider _dynamicFileByteProvider;
        private Point _falloff = new Point(0, 0);
        private double _lastUpdate;
        private bool _markedBackwards;
        private double _offset;
        private bool _unicorn = true;

        #endregion

        #region Properties

        public bool InReadOnlyMode
        {
            get; 
            set;
        } = false;

        public string Path
        {
            get; 
            set;

        } = string.Empty;

        #endregion

        public event EventHandler OnFileChanged;

        #region Constructor

        public HexBox()
        {
            InitializeComponent();

            _stretchText = new StretchText();
            _hexText = new HexText();

            _hexText.mark[0] = -1;
            _hexText.mark[1] = -1;
            _stretchText.mark[1] = -1;
            _stretchText.mark[0] = -1;

            _stretchText.removemarks = true;
            _hexText.removemarks = true;

            canvas1.MouseLeftButtonDown += ht_MouseDown;
            canvas1.MouseLeftButtonUp += ht_MouseUp;
            canvas1.MouseMove += ht_MouseMove;
            canvas2.MouseLeftButtonDown += st_MouseDown;
            canvas2.MouseMove += st_MouseMove;
            canvas2.MouseLeftButtonUp += st_MouseUp;

            canvas1.Cursor = Cursors.IBeam;

            canvas2.Cursor = Cursors.IBeam;

            _stretchText.FontFamily = new FontFamily("Consolas");

            _stretchText.Width = 100;

            Binding myBinding = new Binding("ByteContent")
            {
                Source = _stretchText,
                Mode = BindingMode.TwoWay
            };
            _hexText.SetBinding(HexText.ByteProperty, myBinding);

            canvas2.Children.Add(_stretchText);
            canvas1.Children.Add(_hexText);

            MouseWheel += MainWindow_MouseWheel;

            _mark = new long[2];

            _mark[0] = -1;

            _mark[1] = -1;

            cursor2.Focus();
            for (int j = 0; j < 32; j++)
            {
                TextBlock id = new TextBlock
                {
                    TextAlignment = TextAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontFamily = new FontFamily("Consolas"),

                    Height = 20
                };
                long hex = (j) * 16;
                id.Text = "";
                for (int x = 8 - hex.ToString("X").Length; x > 0; x--)
                {
                    id.Text += "0";
                }
                id.Text += hex.ToString("X");

                id.FontSize = 13;
                Grid.SetRow(id, j);
                gridid.Children.Add(id);
            }

            Storyboard storyBoard = new Storyboard();

            canvas1.MouseLeftButtonDown += canvas1_MouseDown;
            canvas2.MouseLeftButtonDown += canvas2_MouseDown;

            cursor.PreviewKeyDown += KeyInputHexField;
            cursor2.PreviewKeyDown += KeyInputASCIIField;

            cursor2.TextInput += ASCIIField_TextInput;
            cursor.TextInput += HexBox_TextInput;

            scoview.ScrollChanged += scoview_ScrollChanged;
            fileSlider.ValueChanged += MyManipulationCompleteEvent;

            mainPanel.MouseWheel += MainWindow_MouseWheel;
            Stream s = new MemoryStream();

            _dynamicFileByteProvider = new DynamicFileByteProvider(s);
            _dynamicFileByteProvider.LengthChanged += dyfipro_LengthChanged;

            mainPanel.Loaded += HexBox_Loaded;                                // something has to throw this event in order to calculate the offset of the HexBox

        }

        private void HexBox_Loaded(object sender, RoutedEventArgs e)
        {
            scoview_ScrollChanged(this, null);

            /*window = Window.GetWindow(this); //old code to catch mouse button events in window mode, was later fixed by core
            if (window == null)
            {
                return;
            }

            window.PreviewMouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(window_MouseLeftButtonDown);*/
        }

        private void window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_dynamicFileByteProvider.ReadOnly)
            {
                makeUnFocused(IsMouseOver);
            }
        }

        #endregion

        #region Mouse interaction and events

        private void setcursor(Point p)
        {
            p.X = _cell / 2 % 16 * (_hexText.CharWidth * 3);
            p.Y = _cell / 2 / 16 * 20 + 2;

            Canvas.SetLeft(cursor, p.X);

            Canvas.SetTop(cursor, p.Y);
            //cursor.Focus();
        }

        private void setcursor2(Point p)
        {
            p.X = _cellText % 16 * (_stretchText.CharWidth);
            p.Y = _cellText / 16 * 20 - 2;

            Canvas.SetLeft(cursor2, p.X);

            Canvas.SetTop(cursor2, p.Y);
            //cursor2.Focus();
        }

        private void st_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(canvas2);

            _falloff = p;

            _cellText = (((int)Math.Round((p.Y - 10) / 20.0)) * 16 + (int)Math.Round((p.X) / (_stretchText.CharWidth)));

            if (_cellText + (long)fileSlider.Value * 16 > _dynamicFileByteProvider.Length)
            {
                _cellText = (int)(_dynamicFileByteProvider.Length - (long)fileSlider.Maximum * 16);
            }

            if (_cellText > Constants._numberOfCells)
            {
                _cellText = Constants._numberOfCells;
            }

            if (_cellText < 0)
            {
                _cellText = 0;
            }

            _stretchText.mark[0] = _cellText;
            _hexText.mark[0] = _stretchText.mark[0] * 2;

            _mark[0] = _cellText + (long)fileSlider.Value * 16;

            setPosition(_cellText * 2);
            setPositionText(_cellText);
            setcursor2(p);

            cursor2.Focus();

            _stretchText.removemarks = true;
            _hexText.removemarks = true;

            updateUI((long)fileSlider.Value);
            e.Handled = true;
        }

        private void ht_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(canvas1);

            _falloff = p;
            _cell = (((int)Math.Round((p.Y - 10) / 20.0)) * 16 + (int)Math.Round((p.X) / (_hexText.CharWidth * 3))) * 2;


            if (_cell / 2 + (long)fileSlider.Value * 16 > _dynamicFileByteProvider.Length)
            {
                _cell = (int)(_dynamicFileByteProvider.Length - (long)fileSlider.Maximum * 16) * 2;
            }

            _hexText.mark[0] = _cell;

            _hexText.removemarks = true;
            _stretchText.removemarks = true;

            if (_cell > Constants._helpNumberOfCells1)
            {
                _cell = Constants._helpNumberOfCells1;
            }

            _mark[0] = _cell / 2 + (long)fileSlider.Value * 16;


            setPosition(_cell);
            setPositionText(_cell / 2);
            setcursor(p);

            cursor.Focus();
            updateUI((long)fileSlider.Value);
            e.Handled = true;
        }

        private void st_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (10 < Math.Abs(e.GetPosition(canvas2).X - _falloff.X) ||
                    10 < Math.Abs(e.GetPosition(canvas2).Y - _falloff.Y))
                {
                    _stretchText.removemarks = false;
                    _hexText.removemarks = false;
                }

                Point p = e.GetPosition(canvas2);

                _cellText = (((int)Math.Round((p.Y - 10) / 20.0)) * 16 + (int)Math.Round((p.X) / (_stretchText.CharWidth)));

                if (_cellText + (long)fileSlider.Value * 16 > _dynamicFileByteProvider.Length)
                {
                    _cellText = (int)(_dynamicFileByteProvider.Length - (long)fileSlider.Maximum * 16);
                }

                if (_cellText > Constants._numberOfCells)
                {
                    _cellText = Constants._numberOfCells;
                }

                if (_mark[0] > _cellText + (long)fileSlider.Value * 16)
                {
                    _mark[1] = _cellText + (long)fileSlider.Value * 16;
                    _markedBackwards = true;
                    _stretchText.mark[1] = (int)(_mark[1] - fileSlider.Value * 16);
                }

                else
                {
                    _mark[1] = _cellText + (long)fileSlider.Value * 16;
                    _stretchText.mark[1] = (int)(_mark[1] - fileSlider.Value * 16);
                }

                updateUI((long)fileSlider.Value);
            }
        }

        private void ht_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (10 < Math.Abs(e.GetPosition(canvas1).X - _falloff.X) ||
                    10 < Math.Abs(e.GetPosition(canvas1).Y - _falloff.Y))
                {
                    _hexText.removemarks = false;
                    _stretchText.removemarks = false;
                }

                Point p = e.GetPosition(canvas1);

                _cell = (((int)Math.Round((p.Y - 10) / 20.0)) * 16 + (int)Math.Round((p.X) / (_hexText.CharWidth * 3))) * 2;

                if (_cell / 2 + (long)fileSlider.Value * 16 > _dynamicFileByteProvider.Length)
                {
                    _cell = (int)(_dynamicFileByteProvider.Length - (long)fileSlider.Maximum) * 2 - 2;
                }

                if (_cell > Constants._helpNumberOfCells1)
                {
                    _cell = Constants._helpNumberOfCells1;
                }

                if (_mark[0] > _cell / 2 + (long)fileSlider.Value * 16)
                {
                    _mark[1] = _cell / 2 + (long)fileSlider.Value * 16;
                    _markedBackwards = true;
                    _hexText.mark[1] = (_cell);
                }

                else
                {
                    _mark[1] = _cell / 2 + (long)fileSlider.Value * 16;
                    _hexText.mark[1] = (_cell);
                }

                updateUI((long)fileSlider.Value);
            }
        }

        private void st_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(canvas2);

            _cellText = (((int)Math.Round((p.Y - 10) / 20.0)) * 16 + (int)Math.Round((p.X) / (_stretchText.CharWidth)));

            if (_cellText + (long)fileSlider.Value * 16 > _dynamicFileByteProvider.Length)
            {
                _cellText = (int)(_dynamicFileByteProvider.Length - (long)fileSlider.Maximum * 16);
            }

            if (_cellText > Constants._numberOfCells - 1)
            {
                _cellText = Constants._numberOfCells - 1;
            }
            if (_cellText < 0)
            {
                _cellText = 0;
            }
            setPositionText(_cellText);
            setPosition(_cellText * 2);

            setcursor2(p);

            if (_mark[1] < _mark[0])
            {
                _markedBackwards = true;
                long help = _mark[1];
                _mark[1] = _mark[0];
                _mark[0] = help;
            }
            _cursorpositionText = _cellText + (long)fileSlider.Value * 16;
            _cursorposition = _cellText * 2 + (long)fileSlider.Value * 32;

            updateUI((long)fileSlider.Value);
            e.Handled = true;
        }

        private void ht_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(canvas1);
            _cell = (((int)Math.Round((p.Y - 10) / 20.0)) * 16 + (int)Math.Round((p.X) / (_hexText.CharWidth * 3))) * 2;

            if (_cell / 2 + (long)fileSlider.Value * 16 > _dynamicFileByteProvider.Length)
            {
                _cell = (int)(_dynamicFileByteProvider.Length - (long)fileSlider.Maximum * 16) * 2;
            }

            if (_cell > Constants._helpNumberOfCells1)
            {
                _cell = Constants._helpNumberOfCells1;
            }


            //setPosition(cell);
            //setPositionText(cell / 2);
            //setcursor(p);

            if (_mark[1] < _mark[0])
            {
                _markedBackwards = true;
                long help = _mark[1];
                _mark[1] = _mark[0];
                _mark[0] = help;
            }

            _cursorpositionText = _cell / 2 + (long)fileSlider.Value * 16;

            _cursorposition = _cell + (long)fileSlider.Value * 32;

            updateUI((long)fileSlider.Value);
            e.Handled = true;
        }

        private void canvas1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            cursor.Focus();

            //e.Handled = true;
        }

        private void canvas2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            cursor2.Focus();

            //e.Handled = true;
        }

        #endregion

        #region Keyinput

        private void setPositionText(int cellText)
        {
            if (cellText + (long)fileSlider.Value * 16 > _dynamicFileByteProvider.Length)
            {
                cellText = (int)(_dynamicFileByteProvider.Length - (long)fileSlider.Value * 16);
            }
            Point p = new Point
            {
                X = cellText % 16 * _stretchText.CharWidth,
                Y = cellText / 16 * 20 - 2
            };

            Canvas.SetLeft(cursor2, p.X);

            Canvas.SetTop(cursor2, p.Y);

            _cellText = cellText;
        }

        private void setPosition(int cell)
        {
            if (cell / 2 + (long)fileSlider.Value * 16 > _dynamicFileByteProvider.Length)
            {
                cell = (int)(_dynamicFileByteProvider.Length - (long)fileSlider.Value * 16) * 2;
            }

            Point p = new Point
            {
                X = cell % 32 * (_hexText.CharWidth * 3) / 2,
                Y = cell / 32 * 20 - 2
            };

            if (cell % 2 == 1)
            {
                p.X = p.X - _hexText.CharWidth / 2;
            }

            Canvas.SetLeft(cursor, p.X);

            Canvas.SetTop(cursor, p.Y);

            _cell = cell;
        }

        private void KeyInputHexField(object sender, KeyEventArgs e)
        {

            if (e.Key != Key.LeftCtrl && e.Key != Key.RightCtrl)
            {
                if (_cursorposition > fileSlider.Value * 32 + Constants._numberOfCells || _cursorposition < fileSlider.Value * 32)
                {
                    fileSlider.Value = _cursorposition / 32 - 1;
                    fileSlider.Value = Math.Round(fileSlider.Value);
                }
            }


            if (Path != "" && Path != " ")
            {
                _cell = _cell;
                Key k = e.Key;

                bool releasemark = true;

                bool controlKey = false;

                string s = k.ToString();

                if (e.Key == Key.Right)
                {
                    if (_cell / 2 + (long)fileSlider.Value * 16 < _dynamicFileByteProvider.Length)
                    {
                        if (_cell < 32 * Constants._helpNumberOfCells2 - 1 && (_cell + 1) / 32 < _offset - 1)
                        {
                            _cell++;
                        }
                        else
                        {
                            if (fileSlider.Value < fileSlider.Maximum)
                            {
                                fileSlider.Value += 1;
                                _cell++;
                                //this.cell = 32*16-32;
                            }
                        }
                    }
                    controlKey = true;
                    e.Handled = true;
                }

                else if (e.Key == Key.Left)
                {
                    if (_cell > 0)
                    {
                        _cell--;
                    }
                    else
                    {
                        if (fileSlider.Value > 0)
                        {
                            fileSlider.Value -= 1;
                            _cell = 31;
                        }
                    }
                    controlKey = true;
                    e.Handled = true;
                }

                else if (e.Key == Key.Down)
                {
                    if (_cell / 2 + (long)fileSlider.Value * 16 + 15 < _dynamicFileByteProvider.Length)
                    {
                        if (_cell < Constants._helpNumberOfCells2 * 31 && _cell / 32 < _offset - 2)
                        {
                            _cell += 32;
                        }
                        else
                        {
                            if (fileSlider.Value < fileSlider.Maximum)
                            {
                                fileSlider.Value += 1;
                                _cell += 32;
                            }
                        }
                    }


                    controlKey = true;
                    e.Handled = true;
                }

                else if (e.Key == Key.Up)
                {
                    if (_cell > 31)
                    {
                        _cell -= 32;
                    }
                    else
                    {
                        if (fileSlider.Value > 0)
                        {
                            fileSlider.Value -= 1;
                            _cell -= 32;
                        }
                    }
                    controlKey = true;
                    e.Handled = true;
                }

                else if (e.Key == Key.Back)
                {
                    if (_mark[1] - _mark[0] == 0)
                    {
                        if (_cell > 0)
                        {
                            _dynamicFileByteProvider.DeleteBytes(_cell / 2 + (long)fileSlider.Value * 16 - 1, 1);

                            if (_cell % 2 == 1)
                            {
                                _cell--;
                            }
                            _cell--;
                            _cell--;
                        }
                        else
                        {
                            if (fileSlider.Value > 0)
                            {
                                fileSlider.Value -= 1;

                                _cell--;
                            }
                        }
                    }

                    else
                    {
                        deletion_HexBoxField();
                    }

                    e.Handled = true;
                }

                if (e.Key == Key.PageDown)
                {
                    if (fileSlider.Value < fileSlider.Maximum - 16)
                    {
                        fileSlider.Value += 16;
                        _cell += Constants._numberOfCells;
                    }

                    controlKey = true;
                    e.Handled = true;
                }

                else if (e.Key == Key.PageUp)
                {
                    if (fileSlider.Value > 16)
                    {
                        fileSlider.Value -= 16;
                        _cell -= Constants._numberOfCells;
                    }

                    controlKey = true;
                    e.Handled = true;
                }

                else if (e.Key == Key.End)
                {
                    controlKey = true;

                    long row = 480;
                    if (Constants._numberOfCells > _dynamicFileByteProvider.Length)
                    {
                        row = (_dynamicFileByteProvider.Length / 16) * 32;
                    }


                    if (fileSlider.Value != fileSlider.Maximum || _cell < row)
                    {
                        _cell = ((_cell / 32) + 1) * 32 - 2;
                    }
                    else
                    {
                        _cell = (int)(row + (int)(_dynamicFileByteProvider.Length % 16) * 2);
                    }

                    e.Handled = true;
                }


                else if (e.Key == Key.Home)
                {
                    _cell = ((_cell / 32)) * 32;
                    controlKey = true;
                    e.Handled = true;
                }


                else if (e.Key == Key.Return)
                {
                    e.Handled = true;
                }

                _cursorposition = _cell + (long)fileSlider.Value * 32;
                _cursorpositionText = _cursorposition / 2;

                if (e.Key == Key.Tab)
                {
                    cursor2.Focus();
                }


                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    releasemark = false;
                    if (_mark[0] == -1 || _mark[1] == -1)
                    {
                        _mark[0] = _cell / 2 + (long)fileSlider.Value * 16;
                        _mark[1] = _cell / 2 + (long)fileSlider.Value * 16;
                    }

                    long help = -1;

                    if (_cell / 2 + (long)fileSlider.Value * 16 < _mark[0])
                    {
                        if (!_markedBackwards)
                        {
                            help = _mark[0];
                        }
                        _markedBackwards = true;
                    }

                    if (_cell / 2 + (long)fileSlider.Value * 16 > _mark[1])
                    {
                        if (_markedBackwards)
                        {
                            help = _mark[1];
                        }
                        _markedBackwards = false;
                    }

                    if (_cell / 2 + (long)fileSlider.Value * 16 <= _mark[1] &&
                        _cell / 2 + (long)fileSlider.Value * 16 >= _mark[0] &&
                        !_markedBackwards)
                    {
                        _mark[1] = _cell / 2 + (long)fileSlider.Value * 16;
                    }

                    if (_cell / 2 + (long)fileSlider.Value * 16 <= _mark[1] &&
                        _cell / 2 + (long)fileSlider.Value * 16 >= _mark[0] &&
                        _markedBackwards)
                    {
                        _mark[0] = _cell / 2 + (long)fileSlider.Value * 16;
                    }


                    if (_cell / 2 + (long)fileSlider.Value * 16 < _mark[0])
                    {
                        _mark[0] = _cell / 2 + (long)fileSlider.Value * 16;
                        if (help != -1)
                        {
                            _mark[1] = help;
                        }
                    }

                    if (_cell / 2 + (long)fileSlider.Value * 16 > _mark[1])
                    {
                        _mark[1] = _cell / 2 + (long)fileSlider.Value * 16;
                        if (help != -1)
                        {
                            _mark[0] = help;
                        }
                    }
                    if (controlKey)
                    {
                        _hexText.removemarks = false;
                        _stretchText.removemarks = false;
                    }
                }
                else
                {
                }

                if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    if (releasemark)
                    {
                        _mark[0] = -1;
                        _mark[1] = -1;
                        _hexText.mark[0] = -1;
                        _hexText.mark[1] = -1;
                        _hexText.removemarks = true;
                        _stretchText.removemarks = true;
                    }
                }

                if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.A || Keyboard.IsKeyDown(Key.RightCtrl) && e.Key == Key.A)
                {
                    _mark[0] = 0;
                    _mark[1] = _dynamicFileByteProvider.Length;

                    _stretchText.removemarks = false;
                    _hexText.removemarks = false;
                    e.Handled = true;
                }


                if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.C || Keyboard.IsKeyDown(Key.RightCtrl) && e.Key == Key.C)
                {
                    Copy_HexBoxField();
                    e.Handled = true;
                }

                if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.V || Keyboard.IsKeyDown(Key.RightCtrl) && e.Key == Key.V)
                {
                    HexBox_TextInput_Help(Clipboard.GetText());
                    e.Handled = true;
                }

                if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.X || Keyboard.IsKeyDown(Key.RightCtrl) && e.Key == Key.X)
                {
                    Cut_HexBoxField();
                    e.Handled = true;
                }

                if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.S || Keyboard.IsKeyDown(Key.RightCtrl) && e.Key == Key.S)
                {
                    try
                    {

                        _dynamicFileByteProvider.ApplyChanges();
                    }
                    catch (Exception exp)
                    {
                        ErrorOccured(this, new GUIErrorEventArgs(exp.Message));
                    }
                    e.Handled = true;
                }
            }
            else
            {
                e.Handled = true;
            }


            updateUI((long)fileSlider.Value);
        }

        private void KeyInputASCIIField(object sender, KeyEventArgs e)
        {
            if (_cursorpositionText > fileSlider.Value * 16 + Constants._numberOfCells || _cursorpositionText < fileSlider.Value * 16)
            {
                fileSlider.Value = _cursorpositionText / 16 - 1;
                fileSlider.Value = Math.Round(fileSlider.Value);
            }

            if (Path != "" && Path != " ")
            {
                Key k = e.Key;
                string s = k.ToString();
                bool controlKey = false;
                if (e.Key == Key.Right)
                {
                    if (_cellText + (long)fileSlider.Value * 16 < _dynamicFileByteProvider.Length)
                    {
                        if (_cellText < Constants._numberOfCells - 1 && (_cellText + 1) / 16 < _offset - 1)
                        {
                            _cellText++;
                        }
                        else
                        {
                            if (fileSlider.Value < fileSlider.Maximum)
                            {
                                fileSlider.Value += 1;
                                _cellText++;
                            }
                        }
                    }
                    controlKey = true;
                    e.Handled = true;
                }

                else if (e.Key == Key.Left)
                {
                    if (_cellText > 0)
                    {
                        _cellText--;
                    }
                    else
                    {
                        if (fileSlider.Value > 0)
                        {
                            fileSlider.Value -= 1;
                            _cellText = 15;
                        }
                    }

                    controlKey = true;
                    e.Handled = true;
                }

                else if (e.Key == Key.Down)
                {
                    if (_cellText + (long)fileSlider.Value * 16 + 15 < _dynamicFileByteProvider.Length)
                    {
                        if (_cellText < Constants._helpNumberOfCells2 * 15 && _cellText / 16 < _offset - 2)
                        {
                            _cellText += 16;
                        }
                        else
                        {
                            if (fileSlider.Value < fileSlider.Maximum)
                            {
                                fileSlider.Value += 1;
                                _cellText += 16;
                            }
                        }
                    }
                    controlKey = true;
                    e.Handled = true;
                }

                else if (e.Key == Key.Up)
                {
                    if (_cellText > 15)
                    {
                        _cellText -= 16;
                    }
                    else
                    {
                        if (fileSlider.Value > 0)
                        {
                            fileSlider.Value -= 1;
                            _cellText -= 16;
                        }
                    }
                    controlKey = true;
                    e.Handled = true;
                }

                else if (e.Key == Key.Back)
                {
                    if (_mark[1] - _mark[0] == 0)
                    {
                        _dynamicFileByteProvider.DeleteBytes(_cellText + (long)fileSlider.Value * 16 - 1, 1);

                        if (_cellText > 0)
                        {
                            _cellText--;
                        }
                        else
                        {
                            if (fileSlider.Value > 0)
                            {
                                fileSlider.Value -= 1;

                                _cellText = 30;
                            }
                        }
                    }
                    else
                    {
                        deletion_ASCIIField();
                    }

                    e.Handled = true;
                }


                if (e.Key == Key.PageDown)
                {
                    if (fileSlider.Value < fileSlider.Maximum - 16)
                    {
                        fileSlider.Value += 16;
                        _cellText += Constants._numberOfCells;
                    }
                    controlKey = true;
                    e.Handled = true;
                }

                else if (e.Key == Key.PageUp)
                {
                    if (fileSlider.Value > 16)
                    {
                        fileSlider.Value -= 16;
                        _cellText -= Constants._numberOfCells;
                    }
                    controlKey = true;
                    e.Handled = true;
                }

                else if (e.Key == Key.End)
                {
                    long row = 240;
                    if (Constants._numberOfCells > _dynamicFileByteProvider.Length)
                    {
                        row = (_dynamicFileByteProvider.Length / 16) * 16;
                    }

                    if (fileSlider.Value != fileSlider.Maximum || _cellText < row)
                    {
                        _cellText = ((_cellText / 16) + 1) * 16 - 1;
                    }
                    else
                    {
                        _cellText = (int)(row + _dynamicFileByteProvider.Length % 16);
                    }
                    controlKey = true;
                    e.Handled = true;
                }


                else if (e.Key == Key.Home)
                {
                    _cellText = ((_cellText / 16)) * 16;
                    controlKey = true;
                    e.Handled = true;
                }


                else if (e.Key == Key.Return)
                {
                    e.Handled = true;
                }

                else if (e.Key == Key.Tab)
                {
                    cursor.Focus();
                    e.Handled = true;
                }

                _cursorpositionText = _cellText + (long)fileSlider.Value * 16;
                _cursorposition = _cursorpositionText * 2;

                //setPosition(cellText*2);
                //setPositionText(cellText);

                bool releasemark = true;


                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    releasemark = false;

                    if (_mark[0] == -1 || _mark[1] == -1)
                    {
                        _mark[0] = _cellText + (long)fileSlider.Value * 16;
                        _mark[1] = _cellText + (long)fileSlider.Value * 16;
                    }

                    long help = -1;

                    if (_cellText + (long)fileSlider.Value * 16 < _mark[0])
                    {
                        if (!_markedBackwards)
                        {
                            help = _mark[0];
                        }
                        _markedBackwards = true;
                    }

                    if (_cellText + (long)fileSlider.Value * 16 > _mark[1])
                    {
                        if (_markedBackwards)
                        {
                            help = _mark[1];
                        }
                        _markedBackwards = false;
                    }

                    if (_cellText + (long)fileSlider.Value * 16 <= _mark[1] &&
                        _cellText + (long)fileSlider.Value * 16 >= _mark[0] &&
                        !_markedBackwards)
                    {
                        _mark[1] = _cellText + (long)fileSlider.Value * 16;
                    }

                    if (_cellText + (long)fileSlider.Value * 16 <= _mark[1] &&
                        _cellText + (long)fileSlider.Value * 16 >= _mark[0] &&
                        _markedBackwards)
                    {
                        _mark[0] = _cellText + (long)fileSlider.Value * 16;
                    }


                    if (_cellText + (long)fileSlider.Value * 16 < _mark[0])
                    {
                        _mark[0] = _cellText + (long)fileSlider.Value * 16;
                        if (help != -1)
                        {
                            _mark[1] = help;
                        }
                    }

                    if (_cellText + (long)fileSlider.Value * 16 > _mark[1])
                    {
                        _mark[1] = _cellText + (long)fileSlider.Value * 16;
                        if (help != -1)
                        {
                            _mark[0] = help;
                        }
                    }
                    if (controlKey)
                    {
                        _stretchText.removemarks = false;
                        _hexText.removemarks = false;
                    }
                }
                if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    if (releasemark)
                    {
                        _mark[0] = -1;
                        _mark[1] = -1;

                        _hexText.removemarks = true;
                        _stretchText.removemarks = true;
                    }
                }

                if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && e.Key == Key.A)
                {
                    _mark[0] = 0;
                    _mark[1] = _dynamicFileByteProvider.Length;

                    _stretchText.removemarks = false;
                    _hexText.removemarks = false;
                }

                if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && e.Key == Key.C)
                {
                    Copy_ASCIIFild();
                    e.Handled = true;
                }

                if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && e.Key == Key.V)
                {
                    ASCIIField_TextInput_Help((string)Clipboard.GetData(DataFormats.Text));
                    e.Handled = true;
                }

                if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && e.Key == Key.X)
                {
                    Cut_ASCIIFild();
                    e.Handled = true;
                }
            }
            else
            {
                e.Handled = true;
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                e.Handled = true;
            }

            updateUI((long)fileSlider.Value);
        }

        private void backASCIIField()
        {
            bool b = true;

            if (Canvas.GetLeft(cursor) > 10)
            {
                Canvas.SetLeft(cursor, Canvas.GetLeft(cursor) - _hexText.CharWidth);
                b = false;
            }
            else if (Canvas.GetTop(cursor) > 10)
            {
                Canvas.SetTop(cursor, Canvas.GetTop(cursor) - 20);
                Canvas.SetLeft(cursor, 328.7);
            }


            if (Canvas.GetLeft(cursor2) > 0)
            {
                Canvas.SetLeft(cursor2, Canvas.GetLeft(cursor2) - _stretchText.CharWidth);
            }
            else if (Canvas.GetTop(cursor2) > 0)
            {
                Canvas.SetTop(cursor2, Canvas.GetTop(cursor2) - 20);
                Canvas.SetLeft(cursor2, 107);
            }


            if (fileSlider.Value != fileSlider.Minimum && Canvas.GetLeft(cursor2) == 0 &&
                Canvas.GetTop(cursor2) == 0 && b)
            {
                Canvas.SetLeft(cursor, 328.7);
                Canvas.SetLeft(cursor2, 107);
                fileSlider.Value -= 1;
            }

            _cellText--;
        }

        private void ASCIIField_TextInput(object sender, TextCompositionEventArgs e)
        {
            ASCIIField_TextInput_Help(e.Text);
            e.Handled = true;
        }

        private void ASCIIField_TextInput_Help(string e)
        {
            for (int ix = 0; ix < e.Length; ix++)
            {
                if (insertCheck.IsChecked == false)
                {
                    if (_cellText + (long)fileSlider.Value * 16 < _dynamicFileByteProvider.Length)
                    {
                        _dynamicFileByteProvider.WriteByte(_cellText + (long)fileSlider.Value * 16,
                                           Encoding.GetEncoding(1252).GetBytes(e[ix] + "")[0]);
                    }
                    else
                    {
                        byte[] dummyArray = { Encoding.GetEncoding(1252).GetBytes(e[ix] + "")[0] };
                        _dynamicFileByteProvider.InsertBytes(_cellText + (long)fileSlider.Value * 16,
                                             dummyArray);
                    }
                }
                else
                {
                    byte[] dummyArray = { Encoding.GetEncoding(1252).GetBytes(e[ix] + "")[0] };


                    _dynamicFileByteProvider.InsertBytes(_cellText + (long)fileSlider.Value * 16, dummyArray);
                }

                //nextASCIIField();

                _cursorpositionText++;
                _cellText++;
                if (_cellText == 16 * 16)
                {
                    if (fileSlider.Value < fileSlider.Maximum)
                    {
                        fileSlider.Value += 1;
                        _cursorpositionText++;
                    }
                }

                if ((_cellText) / 16 > _offset - 1 && _cellText != 16 * 16 - 1)
                {
                    if (fileSlider.Value < fileSlider.Maximum)
                    {
                        fileSlider.Value += 1;
                        //this.cursorposition++;
                    }
                }
                if (_cellText == 16 * 16 - 1)
                {
                    if (fileSlider.Value < fileSlider.Maximum)
                    {
                        fileSlider.Value += 1;
                        //this.cursorposition++;
                    }
                }
            }

            _cursorposition = _cursorpositionText * 2;

            //setPosition(cellText * 2);
            //setPositionText(cellText);

            updateUI((long)fileSlider.Value);
        }

        private void HexBox_TextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                HexBox_TextInput_Help(e.Text);
            }
            catch (Exception exp)
            {
                ErrorOccured(this, new GUIErrorEventArgs(exp.Message));
            }

            e.Handled = true;
        }

        private void HexBox_TextInput_Help(string e)
        {
            string validCharTest = e.ToLower();
            validCharTest = validCharTest.Replace("a", "");
            validCharTest = validCharTest.Replace("b", "");
            validCharTest = validCharTest.Replace("c", "");
            validCharTest = validCharTest.Replace("d", "");
            validCharTest = validCharTest.Replace("e", "");
            validCharTest = validCharTest.Replace("f", "");
            validCharTest = validCharTest.Replace("1", "");
            validCharTest = validCharTest.Replace("2", "");
            validCharTest = validCharTest.Replace("3", "");
            validCharTest = validCharTest.Replace("4", "");
            validCharTest = validCharTest.Replace("5", "");
            validCharTest = validCharTest.Replace("6", "");
            validCharTest = validCharTest.Replace("7", "");
            validCharTest = validCharTest.Replace("8", "");
            validCharTest = validCharTest.Replace("9", "");
            validCharTest = validCharTest.Replace("0", "");

            if (validCharTest.Length == 0)
            {
                for (int ix = 0; ix < e.Length; ix++)
                {
                    byte[] dummyArray = { Encoding.GetEncoding(1252).GetBytes(e)[ix] };

                    string s = "00";

                    char c = e[ix];

                    if (_cell / 2 + (long)fileSlider.Value * 16 < _dynamicFileByteProvider.Length)
                    {

                        try
                        {
                            s = string.Format("{0:X2}", _dynamicFileByteProvider.ReadByte(_cell / 2 + (long)fileSlider.Value * 16));
                        }
                        catch (Exception exp)
                        {
                            _dynamicFileByteProvider.Dispose();
                            ErrorOccured(this, new GUIErrorEventArgs(exp.Message + "DynamicFileByteProvider cannot read from file"));
                        }
                    }

                    if (insertCheck.IsChecked == false)
                    {
                        if (_cell % 2 == 0)
                        {
                            int i = e[ix];

                            if (e[0] > 96 && e[0] < 103 || e[0] > 47 && e[0] < 58)
                            {
                                s = c + "" + s[1];
                            }

                            if (_cell / 2 + (long)fileSlider.Value * 16 < _dynamicFileByteProvider.Length)
                            {
                                _dynamicFileByteProvider.WriteByte(_cell / 2 + (long)fileSlider.Value * 16,
                                                   (byte)Convert.ToInt32(s, 16));
                            }
                            else
                            {
                                s = s[0] + "" + c;
                                dummyArray[0] = (byte)Convert.ToInt32(s, 16);
                                try
                                {
                                    _dynamicFileByteProvider.InsertBytes(_cell / 2 + (long)fileSlider.Value * 16, dummyArray);
                                }
                                catch (Exception exp)
                                {
                                    ErrorOccured(this, new GUIErrorEventArgs(exp.Message));
                                }
                            }
                        }
                        if (_cell % 2 == 1)
                        {
                            if (e[0] > 96 && e[0] < 103 || e[0] > 47 && e[0] < 58)
                            {
                                s = s[0] + "" + c;
                            }
                            if (_cell / 2 + (long)fileSlider.Value * 16 < _dynamicFileByteProvider.Length)
                            {
                                _dynamicFileByteProvider.WriteByte(_cell / 2 + (long)fileSlider.Value * 16,
                                                   (byte)Convert.ToInt32(s, 16));
                            }
                        }
                    }
                    else
                    {
                        if (_cell % 2 == 0)
                        {
                            int i = e[0];


                            if (e[0] > 96 && e[0] < 103 || e[0] > 47 && e[0] < 58)
                            {
                                s = c + "0";
                            }
                            dummyArray[0] = (byte)Convert.ToInt32(s, 16);
                            _dynamicFileByteProvider.InsertBytes(_cell / 2 + (long)fileSlider.Value * 16, dummyArray);
                        }
                        if (_cell % 2 == 1)
                        {
                            if (e[0] > 96 && e[0] < 103 || e[0] > 47 && e[0] < 58)
                            {
                                s = s[0] + "" + c;
                            }
                            if (_cell / 2 + (long)fileSlider.Value * 16 < _dynamicFileByteProvider.Length)
                            {
                                _dynamicFileByteProvider.WriteByte(_cell / 2 + (long)fileSlider.Value * 16, (byte)Convert.ToInt32(s, 16));
                            }
                        }
                    }

                    _cursorposition++;
                    _cell++;
                    if ((_cell) / 32 > _offset - 1 && _cell != 32 * 16 - 1)
                    {
                        if (fileSlider.Value < fileSlider.Maximum)
                        {
                            fileSlider.Value += 1;
                            //this.cursorposition++;
                        }
                    }
                    if (_cell == 32 * 16 - 1)
                    {
                        if (fileSlider.Value < fileSlider.Maximum)
                        {
                            fileSlider.Value += 1;
                            //this.cursorposition++;
                        }
                    }
                }
            }

            _cursorpositionText = _cursorposition / 2;

            //setPositionText(cell / 2);
            //setPosition(cell);

            updateUI((long)fileSlider.Value);
        }

        #endregion

        #region Public Methods

        public AsyncCallback callback()
        {
            return null;
        }

        public void dispose() //Disposes File See IDisposable for further information
        {

            _dynamicFileByteProvider.Dispose();
        }

        private void updateUI(long position) // Updates UI
        {
            Column.Text = (int)(_cursorpositionText % 16 + 1) + "";
            Line.Text = _cursorpositionText / 16 + 1 + "";

            long end = _dynamicFileByteProvider.Length - position * 16;
            if (end < 0)
            {
                end = 0;
            }

            int max = Constants._numberOfCells;

            _stretchText.Text = "";

            byte[] help2;
            if (end < Constants._numberOfCells)
            {
                help2 = new byte[end];
            }
            else
            {
                help2 = new byte[Constants._numberOfCells];
            }

            try
            {
                for (int i = 0; i < help2.Count(); i++)
                {
                    if (i <= max)
                    {
                        if (i < end)
                        {
                            help2[i] = _dynamicFileByteProvider.ReadByte(i + position * 16);
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                _dynamicFileByteProvider.Dispose();
                ErrorOccured(this, new GUIErrorEventArgs(exp.Message + "DynamicFileByteProvider cannot read from file"));
            }

            _hexText.ByteContent = help2;

            if (_cursorpositionText - fileSlider.Value * 16 < Constants._numberOfCells && _cursorpositionText - fileSlider.Value * 16 >= 0)
            {
                cursor.Opacity = 1.0;
                cursor.Width = 10;
                cursor.Height = 20;
                cursor2.Width = 10;
                cursor2.Height = 20;
                cursor2.Opacity = 1.0;
            }
            else
            {
                cursor.Width = 0;
                cursor.Height = 0;
                cursor2.Width = 0;
                cursor2.Height = 0;
                cursor.Opacity = 0.0;
                cursor2.Opacity = 0.0;
            }

            for (int j = 0; j < 32; j++)
            {
                TextBlock id = gridid.Children[j] as TextBlock;

                long s = (position + j) * 16;
                id.Text = "";
                for (int x = 8 - s.ToString("X").Length; x > 0; x--)
                {
                    id.Text += "0";
                }
                id.Text += s.ToString("X");
            }

            _hexText.mark[0] = (int)(_mark[0] - position * 16) * 2;
            _hexText.mark[1] = (int)(_mark[1] - position * 16) * 2;

            _stretchText.mark[0] = (int)(_mark[0] - position * 16);
            _stretchText.mark[1] = (int)(_mark[1] - position * 16);

            setPositionText((int)(_cursorpositionText - fileSlider.Value * 16));
            setPosition((int)(_cursorposition - fileSlider.Value * 32));

            _lastUpdate = position;
            _unicorn = true;
        }

        private void dyfipro_LengthChanged(object sender, EventArgs e) // occures when length of file changed 
        {
            double old = fileSlider.Maximum;

            if (_offset <= Constants._helpNumberOfCells2)
            {
                fileSlider.Maximum = (_dynamicFileByteProvider.Length - Constants._numberOfCells) / 16 + 2 + Constants._helpNumberOfCells2 - _offset;
                fileSlider.Maximum = Math.Round(fileSlider.Maximum);

            }
            else
            {
                fileSlider.Maximum = (_dynamicFileByteProvider.Length - Constants._numberOfCells) / 16 + 2;
                fileSlider.Maximum = Math.Round(fileSlider.Maximum);

            }

            if ((long)old > (long)fileSlider.Maximum && fileSlider.Value == fileSlider.Maximum)
            {
                //_cellText += 16;
                //_cell += 32;
            }

            if ((long)old < (long)fileSlider.Maximum && fileSlider.Value == fileSlider.Maximum)
            {
                _cellText -= 16;
                _cell -= 32;
            }
        }

        public void openFile(string fileName, bool canRead) // opens file 
        {
            _dynamicFileByteProvider.Dispose();

            canvas1.MouseLeftButtonDown += ht_MouseDown;
            canvas1.MouseLeftButtonUp += ht_MouseUp;
            canvas1.MouseMove += ht_MouseMove;
            canvas2.MouseLeftButtonDown += st_MouseDown;
            canvas2.MouseMove += st_MouseMove;
            canvas2.MouseLeftButtonUp += st_MouseUp;


            canvas1.MouseLeftButtonDown += canvas1_MouseDown;
            canvas2.MouseLeftButtonDown += canvas2_MouseDown;

            cursor.PreviewKeyDown += KeyInputHexField;
            cursor2.PreviewKeyDown += KeyInputASCIIField;

            cursor2.TextInput += ASCIIField_TextInput;
            cursor.TextInput += HexBox_TextInput;

            scoview.ScrollChanged += scoview_ScrollChanged;
            fileSlider.ValueChanged += MyManipulationCompleteEvent;

            if (fileName != "" && fileName != " " && File.Exists(fileName))
            {
                FileName.Text = fileName;
                Path = fileName;
                try
                {
                    _dynamicFileByteProvider = new DynamicFileByteProvider(Path, canRead);
                    makeUnAccesable(true);
                }
                catch (IOException ioe)
                {
                    _dynamicFileByteProvider = new DynamicFileByteProvider(Path, true);
                    makeUnAccesable(false);
                    ErrorOccured(this, new GUIErrorEventArgs(ioe.Message + "File will be opened in ReadOnlyMode"));
                }


                _dynamicFileByteProvider.LengthChanged += dyfipro_LengthChanged;

                fileSlider.Minimum = 0;
                fileSlider.Maximum = (_dynamicFileByteProvider.Length - Constants._numberOfCells) / 16 + Constants._helpNumberOfCells2 - 1;
                fileSlider.Maximum = Math.Round(fileSlider.Maximum);
                fileSlider.ViewportSize = Constants._helpNumberOfCells2;

                _info.Text = _dynamicFileByteProvider.Length / Constants._numberOfCells + "";

                fileSlider.SmallChange = 1;
                fileSlider.LargeChange = 1;

                setPosition(0);
                setPositionText(0);


                updateUI(0);
            }
        }

        public void closeFile(bool clear) // closes file
        {
            _dynamicFileByteProvider.Dispose();



            canvas1.MouseLeftButtonDown -= ht_MouseDown;
            canvas1.MouseLeftButtonUp -= ht_MouseUp;
            canvas1.MouseMove -= ht_MouseMove;
            canvas2.MouseLeftButtonDown -= st_MouseDown;
            canvas2.MouseMove -= st_MouseMove;
            canvas2.MouseLeftButtonUp -= st_MouseUp;


            canvas1.MouseLeftButtonDown -= canvas1_MouseDown;
            canvas2.MouseLeftButtonDown -= canvas2_MouseDown;

            cursor.PreviewKeyDown -= KeyInputHexField;
            cursor2.PreviewKeyDown -= KeyInputASCIIField;

            cursor2.TextInput -= ASCIIField_TextInput;
            cursor.TextInput -= HexBox_TextInput;

            scoview.ScrollChanged -= scoview_ScrollChanged;
            fileSlider.ValueChanged -= MyManipulationCompleteEvent;

            Loaded -= new RoutedEventHandler(HexBox_Loaded);


        }

        public bool saveData(bool ask, bool saveas) // saves changed data to file
        {
            try
            {
                if (_dynamicFileByteProvider.Length != 0)
                {
                    if (_dynamicFileByteProvider.HasChanges() || saveas)
                    {
                        MessageBoxResult result;
                        if (ask)
                        {
                            string messageBoxText =
                                "Do you want to save the changes in a new file? (If you click no, changes will be saved permanently.)";
                            string caption = "FileInput";
                            MessageBoxButton button = MessageBoxButton.YesNoCancel;

                            MessageBoxImage icon = MessageBoxImage.Warning;


                            result = MessageBox.Show(messageBoxText, caption, button, icon);
                        }
                        else
                        {
                            result = MessageBoxResult.Yes;
                        }
                        // Process message box results
                        switch (result)
                        {
                            case MessageBoxResult.Yes:
                                // User pressed Yes button
                                // ...

                                SaveFileDialog saveFileDialog1 = new SaveFileDialog
                                {
                                    Title = "Save Data",
                                    FileName = Path
                                };
                                saveFileDialog1.ShowDialog();


                                // If the file name is not an empty string open it for saving.
                                if (saveFileDialog1.FileName != "")
                                {
                                    // Saves the Image via a FileStream created by the OpenFile method.

                                    if (saveFileDialog1.FileName != Path)
                                    {
                                        FileStream fs = (FileStream)saveFileDialog1.OpenFile();


                                        for (long i = 0; i < _dynamicFileByteProvider.Length; i++)
                                        {
                                            fs.WriteByte(_dynamicFileByteProvider.ReadByte(i));
                                        }
                                        FileName.Text = saveFileDialog1.FileName;
                                        Path = saveFileDialog1.FileName;
                                        fs.Close();
                                    }
                                    else
                                    {
                                        _dynamicFileByteProvider.ApplyChanges();
                                    }
                                }
                                OnFileChanged(this, EventArgs.Empty);
                                break;
                            case MessageBoxResult.No:
                                _dynamicFileByteProvider.ApplyChanges();
                                break;
                            case MessageBoxResult.Cancel:
                                // User pressed Cancel button
                                // ...
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _dynamicFileByteProvider.Dispose();
                ErrorOccured(this, new GUIErrorEventArgs(e.Message));
            }

            return true;
        }

        public void collapseControl(bool b) // changes visibility of user controls, when HexBox is not visible
        {
            //grid1.IsEnabled = b;
            //grid2.IsEnabled = b;

            if (b)
            {
                cursor.Visibility = Visibility.Visible;
                cursor2.Visibility = Visibility.Visible;
                saveAs.Visibility = Visibility.Visible;
                save.Visibility = Visibility.Visible;
                newFile.Visibility = Visibility.Visible;
                openFileButton.Visibility = Visibility.Visible;
                insertCheck.Visibility = Visibility.Visible;
                toolbar.Visibility = Visibility.Visible;
            }
            else
            {
                cursor.Visibility = Visibility.Collapsed;
                cursor2.Visibility = Visibility.Collapsed;
                saveAs.Visibility = Visibility.Collapsed;
                save.Visibility = Visibility.Collapsed;
                newFile.Visibility = Visibility.Collapsed;
                openFileButton.Visibility = Visibility.Collapsed;
                insertCheck.Visibility = Visibility.Collapsed;
                toolbar.Visibility = Visibility.Collapsed;
            }
        }

        private void makeUnFocused(bool b) // allows or doesn't allow manipulation of data
        {

            if (b && IsEnabled)
            {
                _stretchText.brush = Brushes.Orange;
                _hexText.brush = Brushes.Orange;
                cursor.Visibility = Visibility.Visible;
                cursor2.Visibility = Visibility.Visible;
            }
            else
            {
                _hexText.brush = Brushes.DarkGray;
                _stretchText.brush = Brushes.DarkGray;
                cursor.Visibility = Visibility.Collapsed;
                cursor2.Visibility = Visibility.Collapsed;

            }
            updateUI((long)fileSlider.Value);
        }


        public void makeUnAccesable(bool b) // allows or doesn't allows manipulation of data
        {
            //grid1.IsEnabled = b;
            //grid2.IsEnabled = b;
            //Console.WriteLine("--------------------------hallo----------------------------");
            canvas1.IsEnabled = b;
            canvas2.IsEnabled = b;
            saveAs.IsEnabled = b;
            save.IsEnabled = b;

            InReadOnlyMode = !b;

            if (b)
            {
                cursor.Visibility = Visibility.Visible;
                cursor2.Visibility = Visibility.Visible;
            }
            else
            {
                cursor.Visibility = Visibility.Collapsed;
                cursor2.Visibility = Visibility.Collapsed;
            }

        }

        #endregion

        #region Buttons

        private void Open_Button_Click(object sender, RoutedEventArgs e)
        {
            _dynamicFileByteProvider.Dispose();

            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                //openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog().Value)
                {
                    Path = openFileDialog.FileName;
                }
                if (Path != "" && File.Exists(Path))
                {
                    FileName.Text = Path;
                    try
                    {
                        _dynamicFileByteProvider = new DynamicFileByteProvider(Path, false);
                        makeUnAccesable(true);
                    }
                    catch (IOException ioe)
                    {
                        _dynamicFileByteProvider = new DynamicFileByteProvider(Path, true);
                        makeUnAccesable(false);
                        InReadOnlyMode = true;
                        ErrorOccured(this, new GUIErrorEventArgs(ioe.Message + "File will be opened in ReadOnlyMode"));
                    }

                    _dynamicFileByteProvider.LengthChanged += dyfipro_LengthChanged;

                    fileSlider.Minimum = 0;
                    fileSlider.Maximum = (_dynamicFileByteProvider.Length - Constants._numberOfCells) / 16 + Constants._helpNumberOfCells2 - 1;
                    fileSlider.Maximum = Math.Round(fileSlider.Maximum);
                    fileSlider.ViewportSize = Constants._helpNumberOfCells2;

                    _info.Text = _dynamicFileByteProvider.Length / Constants._numberOfCells + "";

                    fileSlider.SmallChange = 1;
                    fileSlider.LargeChange = 1;

                    reset();

                    updateUI(0);

                    OnFileChanged(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                _dynamicFileByteProvider.Dispose();
                ErrorOccured(this, new GUIErrorEventArgs(ex.Message));
            }
        }

        private void New_Button_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog
            {
                Title = "New File"
            };
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                _dynamicFileByteProvider.Dispose();
                // Saves the Image via a FileStream created by the OpenFile method.
                try
                {
                    FileStream fs = (FileStream)saveFileDialog1.OpenFile();

                    fs.Dispose();
                    fs.Close();

                    Path = saveFileDialog1.FileName;

                    FileName.Text = Path;

                    _dynamicFileByteProvider = new DynamicFileByteProvider(Path, false);
                    _dynamicFileByteProvider.LengthChanged += dyfipro_LengthChanged;

                    fileSlider.Minimum = 0;
                    fileSlider.Maximum = (_dynamicFileByteProvider.Length - Constants._numberOfCells) / 16 + Constants._helpNumberOfCells2 - 1;
                    fileSlider.Maximum = Math.Round(fileSlider.Maximum);
                    fileSlider.ViewportSize = Constants._helpNumberOfCells2;

                    _info.Text = _dynamicFileByteProvider.Length / Constants._numberOfCells + "";

                    //fileSlider.ValueChanged += MyManipulationCompleteEvent;
                    fileSlider.SmallChange = 1;
                    fileSlider.LargeChange = 1;

                    OnFileChanged(this, EventArgs.Empty);

                    reset();
                    updateUI(0);
                }
                catch (Exception exp)
                {
                    ErrorOccured(this, new GUIErrorEventArgs(exp.Message));
                }
            }
        }

        private void reset()
        {
            setPosition(0);
            setPositionText(0);
            _mark[1] = -1;
            _mark[0] = -1;
            _hexText.removemarks = true;
            _stretchText.removemarks = true;
        }

        private void help_copy_Hexbox()
        {
            StringBuilder clipBoardString = new StringBuilder();
            if (_mark[1] > _dynamicFileByteProvider.Length)
            {
                _mark[1] = _dynamicFileByteProvider.Length;
            }

            for (long i = _mark[0]; i < _mark[1]; i++)
            {
                try
                {
                    clipBoardString.Append(string.Format("{0:X2}", _dynamicFileByteProvider.ReadByte(i)));
                }
                catch (Exception exp)
                {
                    _dynamicFileByteProvider.Dispose();
                    ErrorOccured(this, new GUIErrorEventArgs(exp.Message + "DynamicFileByteProvider cannot read from file"));
                }
            }

            try
            {
                Clipboard.SetText(clipBoardString.ToString());
            }
            catch (Exception exp)
            {
                ErrorOccured(this, new GUIErrorEventArgs(exp.Message));
            }
        }

        private void Copy_HexBoxField()
        {
            help_copy_Hexbox();

            _mark[1] = -1;
            _mark[0] = -1;

            _hexText.removemarks = true;
            updateUI((long)fileSlider.Value);
        }

        private void deletion_HexBoxField()
        {
            int celltemp = _cell;
            if (_cell > 0)
            {
                if (celltemp / 2 + (long)fileSlider.Value * 16 - 2 < _dynamicFileByteProvider.Length)
                {
                    if (_mark[1] - _mark[0] == 0)
                    {
                        if (celltemp / 2 + (int)fileSlider.Value * 16 - 1 > -1)
                        {
                            if (celltemp / 2 + (long)fileSlider.Value * 16 > -1)
                            {
                                _dynamicFileByteProvider.DeleteBytes(celltemp / 2 + (long)fileSlider.Value * 16, 1);
                            }
                        }
                    }
                    else
                    {
                        if (_markedBackwards)
                        {
                            if (_mark[0] > -1 && _mark[1] - _mark[0] > -1)
                            {
                                _dynamicFileByteProvider.DeleteBytes(_mark[0], _mark[1] - _mark[0]);
                            }
                        }
                        else
                        {
                            if (_mark[0] > -1 && _mark[1] - _mark[0] > -1)
                            {
                                _dynamicFileByteProvider.DeleteBytes(_mark[0], _mark[1] - _mark[0]);
                            }

                            if (_mark[1] - _mark[0] > celltemp)
                            {
                                fileSlider.Value = _mark[0] / 16;
                                fileSlider.Value = Math.Round(fileSlider.Value);
                            }

                            _cell = (int)(_mark[0] * 2 - (long)fileSlider.Value * 32);
                            Canvas.SetLeft(cursor, _mark[0] % 16 * _hexText.CharWidth * 3);
                        }
                    }

                    _cursorposition = _mark[0] * 2;
                    _cursorpositionText = _mark[0];

                    _mark[1] = -1;
                    _mark[0] = -1;

                    _hexText.removemarks = true;
                    _stretchText.removemarks = true;
                    updateUI((long)fileSlider.Value);
                }
            }
        }

        private void deletion_ASCIIField()
        {
            if (_cellText > 0)
            {
                if (_cellText + (int)fileSlider.Value * 16 - 2 < _dynamicFileByteProvider.Length)
                {
                    if (_mark[1] - _mark[0] == 0)
                    {
                        if (_cellText + (int)fileSlider.Value * 16 - 1 > -1)
                        {
                            _dynamicFileByteProvider.DeleteBytes(_cellText + (long)fileSlider.Value * 16 - 1, 1);
                            backASCIIField();
                        }
                    }
                    else
                    {
                        if (_markedBackwards)
                        {
                            if (_mark[0] > -1 && _mark[1] - _mark[0] > -1)
                            {
                                _dynamicFileByteProvider.DeleteBytes(_mark[0], _mark[1] - _mark[0]);
                            }
                        }
                        else
                        {
                            if (_mark[0] > -1 && _mark[1] - _mark[0] > -1)
                            {
                                _dynamicFileByteProvider.DeleteBytes(_mark[0], _mark[1] - _mark[0]);
                            }

                            if (_mark[1] - _mark[0] > _cellText)
                            {
                                fileSlider.Value = _mark[0] / 16;
                                fileSlider.Value = Math.Round(fileSlider.Value);
                            }

                            _cellText = (int)(_mark[0] - (long)fileSlider.Value * 16);
                        }
                    }


                    _hexText.removemarks = true;
                    _stretchText.removemarks = true;


                    _cursorposition = _mark[0] * 2;
                    _cursorpositionText = _mark[0];


                    _mark[1] = -1;
                    _mark[0] = -1;
                    updateUI((long)fileSlider.Value);
                }
            }
        }

        private void Cut_HexBoxField()
        {
            help_copy_Hexbox();
            deletion_HexBoxField();
        }

        private void Copy_ASCIIFild()
        {
            StringBuilder clipBoardString = new StringBuilder();
            try
            {
                for (long i = _mark[0]; i < _mark[1]; i++)
                {
                    if (_dynamicFileByteProvider.ReadByte(i) > 34 && _dynamicFileByteProvider.ReadByte(i) < 128)
                    {
                        clipBoardString.Append((char)_dynamicFileByteProvider.ReadByte(i));
                    }
                    else
                    {
                        clipBoardString.Append('.');
                    }
                }


                Clipboard.SetText(clipBoardString.ToString());
            }
            catch (Exception exp)
            {
                _dynamicFileByteProvider.Dispose();
                ErrorOccured(this, new GUIErrorEventArgs(exp.Message));
            }
            _mark[1] = -1;
            _mark[0] = -1;

            updateUI((long)fileSlider.Value);
        }

        private void Paste_ASCIIFild()
        {
            if (_cellText + (int)fileSlider.Value * 16 < _dynamicFileByteProvider.Length)
            {
                if (_mark[1] - _mark[0] != 0)
                {
                    if (_markedBackwards)
                    {
                        try
                        {
                            _dynamicFileByteProvider.DeleteBytes(_mark[0] + 1, _mark[1] - _mark[0]);
                        }
                        catch (Exception e)
                        {
                            ErrorOccured(this, new GUIErrorEventArgs(e.Message));
                        }
                    }
                    else
                    {
                        try
                        {
                            _dynamicFileByteProvider.DeleteBytes(_mark[0], _mark[1] - _mark[0]);
                        }
                        catch (Exception e)
                        {
                            ErrorOccured(this, new GUIErrorEventArgs(e.Message));
                        }

                        if (_mark[1] - _mark[0] > _cell)
                        {
                            fileSlider.Value = _mark[0] / 16;
                            fileSlider.Value = Math.Round(fileSlider.Value);
                        }
                    }
                }


                _mark[1] = -1;
                _mark[0] = -1;
                updateUI((long)fileSlider.Value);
            }

            if (_markedBackwards)
            {
                _cellText = (int)(Canvas.GetTop(cursor) / 20 * 16 + Canvas.GetLeft(cursor) / 10 + 2);
            }
            else
            {
                _cellText = (int)(Canvas.GetTop(cursor) / 20 * 16 + Canvas.GetLeft(cursor) / 10);
            }

            string text = (string)Clipboard.GetData(DataFormats.Text);

            try
            {
                _dynamicFileByteProvider.InsertBytes(_cellText + (int)fileSlider.Value * 16, _encoding.GetBytes(text));
            }
            catch (Exception e)
            {
                ErrorOccured(this, new GUIErrorEventArgs(e.Message));
            }

            updateUI((long)fileSlider.Value);
        }

        private void help_copy_ASCIIField()
        {
            StringBuilder clipBoardString = new StringBuilder();
            if (_mark[1] > _dynamicFileByteProvider.Length)
            {
                _mark[1] = _dynamicFileByteProvider.Length;
            }

            try
            {
                for (long i = _mark[0]; i < _mark[1]; i++)
                {
                    if (_dynamicFileByteProvider.ReadByte(i) > 34 && _dynamicFileByteProvider.ReadByte(i) < 128)
                    {
                        clipBoardString.Append((char)_dynamicFileByteProvider.ReadByte(i));
                    }
                    else
                    {
                        clipBoardString.Append('.');
                    }
                }

                Clipboard.SetText(clipBoardString.ToString());
            }
            catch (Exception exp)
            {
                _dynamicFileByteProvider.Dispose();
                ErrorOccured(this, new GUIErrorEventArgs(exp.Message));
            }
        }

        private void Cut_ASCIIFild()
        {
            help_copy_ASCIIField();
            deletion_ASCIIField();
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _dynamicFileByteProvider.ApplyChanges();
            }
            catch (Exception exp)
            {
                ErrorOccured(this, new GUIErrorEventArgs(exp.Message));
            }
        }

        private void Save_As_Button_Click(object sender, RoutedEventArgs e)
        {
            saveData(false, true);
        }

        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            fileSlider.Value -= e.Delta / 10;


            e.Handled = true;
        }



        private void MyManipulationCompleteEvent(object sender, EventArgs e)
        {
            fileSlider.Value = Math.Round(fileSlider.Value);


            if (_lastUpdate != fileSlider.Value && _unicorn)
            {
                _unicorn = false;
                updateUI((long)fileSlider.Value);
            }

            _info.Text = (long)fileSlider.Value + "" + Math.Round(fileSlider.Value * 16, 0) + fileSlider.Value;
        }

        private void scoview_MouseDown(object sender, MouseButtonEventArgs e)
        {
            canvas1.Focus();
        }

        private void scoview_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            scoview.ScrollToTop();
            _offset = scoview.ViewportHeight / 20;
            if (_offset < 2)
            {
                _offset = 2;
            }

            if (_offset < Constants._helpNumberOfCells2)
            {
                fileSlider.Maximum = _dynamicFileByteProvider.Length / 16 + 2 - _offset;
                fileSlider.Maximum = Math.Round(fileSlider.Maximum);
            }
            else
            {
                fileSlider.Maximum = _dynamicFileByteProvider.Length / 16 - Constants._helpNumberOfCells2 - 1;
                fileSlider.Maximum = Math.Round(fileSlider.Maximum);
            }
        }

        #endregion

        public void Clear()
        {
            fileSlider.Minimum = 0;
            fileSlider.Maximum = (_dynamicFileByteProvider.Length - Constants._numberOfCells) / 16 + 1;
            fileSlider.Maximum = Math.Round(fileSlider.Maximum);
            fileSlider.SmallChange = 1;
            fileSlider.LargeChange = 1;

            FileName.Text = string.Empty;

            closeFile(false);
            Stream memoryStream = new MemoryStream();
            _dynamicFileByteProvider = new DynamicFileByteProvider(memoryStream);
            _dynamicFileByteProvider.LengthChanged += dyfipro_LengthChanged;
            OnFileChanged(this, EventArgs.Empty);
            reset();
            updateUI(0);
        }
        #region Events

        public delegate void GUIErrorEventHandler(object sender, GUIErrorEventArgs ge);
        public event GUIErrorEventHandler ErrorOccured;

        #endregion

    }

    public class GUIErrorEventArgs : EventArgs
    {
        public string Message
        {
            get; 
            set;
        }

        public GUIErrorEventArgs(string message)
        {
            Message = message;
        }        
    }
}