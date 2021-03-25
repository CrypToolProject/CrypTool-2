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
using Microsoft.Win32;

namespace HexBox
{
    /// <summary>
    /// Interaction logic for HexBox.xaml
    /// </summary>
    /// 

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

        private Window window;
        private readonly HexText _ht;
        private readonly TextBlock _info = new TextBlock();
        private readonly long[] _mark;
        private readonly StretchText _st;
        private readonly Encoding enc = new ASCIIEncoding();
        private int _cell;
        private int _cellText;
        private long _cursorposition;
        private long _cursorpositionText;
        private DynamicFileByteProvider _dyfipro;
        private Point _falloff = new Point(0, 0);
        private double _lastUpdate;
        private Boolean _markedBackwards;
        private double _offset;
        private Boolean _unicorn = true;

        #endregion

        #region Properties

        

        public Boolean InReadOnlyMode = false;              // Option for checking the mode of the Hexbox   
        public string Pfad = string.Empty;                  // Path of the file
        public event EventHandler OnFileChanged;            // Event fired when file changes data

        #endregion

        #region Constructor

        

        public HexBox()
        {
            InitializeComponent();

            _st = new StretchText();
            _ht = new HexText();

            _ht.mark[0] = -1;
            _ht.mark[1] = -1;
            _st.mark[1] = -1;
            _st.mark[0] = -1;

            _st.removemarks = true;
            _ht.removemarks = true;

            canvas1.MouseLeftButtonDown += ht_MouseDown;
            canvas1.MouseLeftButtonUp += ht_MouseUp;
            canvas1.MouseMove += ht_MouseMove;
            canvas2.MouseLeftButtonDown += st_MouseDown;
            canvas2.MouseMove += st_MouseMove;
            canvas2.MouseLeftButtonUp += st_MouseUp;

            canvas1.Cursor = Cursors.IBeam;

            canvas2.Cursor = Cursors.IBeam;

            _st.FontFamily = new FontFamily("Consolas");

            _st.Width = 100;

            var myBinding = new Binding("ByteContent");
            myBinding.Source = _st;

            myBinding.Mode = BindingMode.TwoWay;
            _ht.SetBinding(HexText.ByteProperty, myBinding);

            canvas2.Children.Add(_st);
            canvas1.Children.Add(_ht);

            MouseWheel += MainWindow_MouseWheel;

            _mark = new long[2];

            _mark[0] = -1;

            _mark[1] = -1;

            cursor2.Focus();
            for (int j = 0; j < 32; j++)
            {
                var id = new TextBlock();
                id.TextAlignment = TextAlignment.Right;
                id.VerticalAlignment = VerticalAlignment.Center;
                id.FontFamily = new FontFamily("Consolas");

                id.Height = 20;
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


            var sb = new Storyboard();

            

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

            _dyfipro = new DynamicFileByteProvider(s);
            _dyfipro.LengthChanged += dyfipro_LengthChanged;

            mainPanel.Loaded += HexBox_Loaded;                                // something has to throw this event in order to calculate the offset of the HexBox

        }

        void HexBox_Loaded(object sender, RoutedEventArgs e)
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
            if (!_dyfipro.ReadOnly)
            {
                this.makeUnFocused(this.IsMouseOver);
            }
        }

        #endregion

        #region Mouse interaction and events

        private void setcursor(Point p)
        {
            p.X = _cell/2%16*(_ht.CharWidth*3);
            p.Y = _cell/2/16*20 + 2;

            Canvas.SetLeft(cursor, p.X);

            Canvas.SetTop(cursor, p.Y);
            //cursor.Focus();
        }

        private void setcursor2(Point p)
        {
            p.X = _cellText%16*(_st.CharWidth);
            p.Y = _cellText/16*20 - 2;

            Canvas.SetLeft(cursor2, p.X);

            Canvas.SetTop(cursor2, p.Y);
            //cursor2.Focus();
        }

        private void st_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(canvas2);

            _falloff = p;

            _cellText = (((int) Math.Round((p.Y - 10)/20.0))*16 + (int) Math.Round((p.X)/(_st.CharWidth)));

            if (_cellText + (long) fileSlider.Value*16 > _dyfipro.Length)
            {
                _cellText = (int) (_dyfipro.Length - (long) fileSlider.Maximum*16);
            }

            if (_cellText > Constants._numberOfCells)
            {
                _cellText = Constants._numberOfCells;
            }

            if (_cellText < 0)
            {
                _cellText = 0;
            }

            _st.mark[0] = _cellText;
            _ht.mark[0] = _st.mark[0]*2;

            _mark[0] = _cellText + (long) fileSlider.Value*16;

            setPosition(_cellText*2);
            setPositionText(_cellText);
            setcursor2(p);

            cursor2.Focus();

            _st.removemarks = true;
            _ht.removemarks = true;

            updateUI((long) fileSlider.Value);
            e.Handled = true;
        }

        private void ht_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(canvas1);

            _falloff = p;
            _cell = (((int) Math.Round((p.Y - 10)/20.0))*16 + (int) Math.Round((p.X)/(_ht.CharWidth*3)))*2;


            if (_cell/2 + (long) fileSlider.Value*16 > _dyfipro.Length)
            {
                _cell = (int) (_dyfipro.Length - (long) fileSlider.Maximum*16)*2;
            }

            _ht.mark[0] = _cell;

            _ht.removemarks = true;
            _st.removemarks = true;

            if (_cell > Constants._helpNumberOfCells1)
            {
                _cell = Constants._helpNumberOfCells1;
            }

            _mark[0] = _cell/2 + (long) fileSlider.Value*16;


            setPosition(_cell);
            setPositionText(_cell/2);
            setcursor(p);

            cursor.Focus();
            updateUI((long) fileSlider.Value);
            e.Handled = true;
        }

        private void st_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (10 < Math.Abs(e.GetPosition(canvas2).X - _falloff.X) ||
                    10 < Math.Abs(e.GetPosition(canvas2).Y - _falloff.Y))
                {
                    _st.removemarks = false;
                    _ht.removemarks = false;
                }

                Point p = e.GetPosition(canvas2);

                _cellText = (((int) Math.Round((p.Y - 10)/20.0))*16 + (int) Math.Round((p.X)/(_st.CharWidth)));

                if (_cellText + (long) fileSlider.Value*16 > _dyfipro.Length)
                {
                    _cellText = (int) (_dyfipro.Length - (long) fileSlider.Maximum*16);
                }

                if (_cellText > Constants._numberOfCells)
                {
                    _cellText = Constants._numberOfCells;
                }

                if (_mark[0] > _cellText + (long) fileSlider.Value*16)
                {
                    _mark[1] = _cellText + (long) fileSlider.Value*16;
                    _markedBackwards = true;
                    _st.mark[1] = (int) (_mark[1] - fileSlider.Value*16);
                }

                else
                {
                    _mark[1] = _cellText + (long) fileSlider.Value*16;
                    _st.mark[1] = (int) (_mark[1] - fileSlider.Value*16);
                }

                updateUI((long) fileSlider.Value);
            }
        }

        private void ht_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (10 < Math.Abs(e.GetPosition(canvas1).X - _falloff.X) ||
                    10 < Math.Abs(e.GetPosition(canvas1).Y - _falloff.Y))
                {
                    _ht.removemarks = false;
                    _st.removemarks = false;
                }

                Point p = e.GetPosition(canvas1);

                _cell = (((int) Math.Round((p.Y - 10)/20.0))*16 + (int) Math.Round((p.X)/(_ht.CharWidth*3)))*2;

                if (_cell/2 + (long) fileSlider.Value*16 > _dyfipro.Length)
                {
                    _cell = (int) (_dyfipro.Length - (long) fileSlider.Maximum)*2 - 2;
                }

                if (_cell > Constants._helpNumberOfCells1)
                {
                    _cell = Constants._helpNumberOfCells1;
                }

                if (_mark[0] > _cell/2 + (long) fileSlider.Value*16)
                {
                    _mark[1] = _cell/2 + (long) fileSlider.Value*16;
                    _markedBackwards = true;
                    _ht.mark[1] = (_cell);
                }

                else
                {
                    _mark[1] = _cell/2 + (long) fileSlider.Value*16;
                    _ht.mark[1] = (_cell);
                }

                updateUI((long) fileSlider.Value);
            }
        }

        private void st_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(canvas2);

            _cellText = (((int) Math.Round((p.Y - 10)/20.0))*16 + (int) Math.Round((p.X)/(_st.CharWidth)));

            if (_cellText + (long) fileSlider.Value*16 > _dyfipro.Length)
            {
                _cellText = (int) (_dyfipro.Length - (long) fileSlider.Maximum*16);
            }

            if (_cellText > Constants._numberOfCells-1)
            {
                _cellText = Constants._numberOfCells - 1;
            }
            if (_cellText < 0)
            {
                _cellText = 0;
            }
            setPositionText(_cellText);
            setPosition(_cellText*2);

            setcursor2(p);

            if (_mark[1] < _mark[0])
            {
                _markedBackwards = true;
                long help = _mark[1];
                _mark[1] = _mark[0];
                _mark[0] = help;
            }
            _cursorpositionText = _cellText + (long) fileSlider.Value*16;
            _cursorposition = _cellText*2 + (long) fileSlider.Value*32;

            updateUI((long) fileSlider.Value);
            e.Handled = true;
        }

        private void ht_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(canvas1);
            _cell = (((int) Math.Round((p.Y - 10)/20.0))*16 + (int) Math.Round((p.X)/(_ht.CharWidth*3)))*2;

            if (_cell/2 + (long) fileSlider.Value*16 > _dyfipro.Length)
            {
                _cell = (int) (_dyfipro.Length - (long) fileSlider.Maximum*16)*2;
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

            _cursorpositionText = _cell/2 + (long) fileSlider.Value*16;

            _cursorposition = _cell + (long) fileSlider.Value*32;

            updateUI((long) fileSlider.Value);
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
            if (cellText + (long) fileSlider.Value*16 > _dyfipro.Length)
            {
                cellText = (int) (_dyfipro.Length - (long) fileSlider.Value*16);
            }
            var p = new Point();

            p.X = cellText%16*_st.CharWidth;
            p.Y = cellText/16*20-2;

            Canvas.SetLeft(cursor2, p.X);

            Canvas.SetTop(cursor2, p.Y);

            _cellText = cellText;
        }

        private void setPosition(int cell)
        {
            if (cell/2 + (long) fileSlider.Value*16 > _dyfipro.Length)
            {
                cell = (int) (_dyfipro.Length - (long) fileSlider.Value*16)*2;
            }

            var p = new Point();

            p.X = cell%32*(_ht.CharWidth*3)/2;
            p.Y = cell/32*20 -2;

            if (cell%2 == 1)
            {
                p.X = p.X - _ht.CharWidth/2;
            }

            Canvas.SetLeft(cursor, p.X);

            Canvas.SetTop(cursor, p.Y);

            _cell = cell;
        }

        private void KeyInputHexField(object sender, KeyEventArgs e)
        {

            if (e.Key != Key.LeftCtrl && e.Key != Key.RightCtrl)
            {
                if (_cursorposition > fileSlider.Value*32 + Constants._numberOfCells || _cursorposition < fileSlider.Value*32)
                {
                    fileSlider.Value = _cursorposition/32 - 1;
                    fileSlider.Value = Math.Round(fileSlider.Value);
                }
            }


            if (Pfad != "" && Pfad != " ")
            {
                _cell = _cell;
                Key k = e.Key;

                Boolean releasemark = true;

                Boolean controlKey = false;

                string s = k.ToString();

                if (e.Key == Key.Right)
                {
                    if (_cell/2 + (long) fileSlider.Value*16 < _dyfipro.Length)
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
                    if (_cell/2 + (long) fileSlider.Value*16 + 15 < _dyfipro.Length)
                    {
                        if (_cell < Constants._helpNumberOfCells2 * 31  && _cell / 32 < _offset - 2)
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
                            _dyfipro.DeleteBytes(_cell/2 + (long) fileSlider.Value*16 - 1, 1);

                            if (_cell%2 == 1)
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
                    if (Constants._numberOfCells > _dyfipro.Length)
                    {
                        row = (_dyfipro.Length/16)*32;
                    }


                    if (fileSlider.Value != fileSlider.Maximum || _cell < row)
                    {
                        _cell = ((_cell/32) + 1)*32 - 2;
                    }
                    else
                    {
                        _cell = (int) (row + (int) (_dyfipro.Length%16)*2);
                    }

                    e.Handled = true;
                }


                else if (e.Key == Key.Home)
                {
                    _cell = ((_cell/32))*32;
                    controlKey = true;
                    e.Handled = true;
                }


                else if (e.Key == Key.Return)
                {
                    e.Handled = true;
                }

                _cursorposition = _cell + (long) fileSlider.Value*32;
                _cursorpositionText = _cursorposition/2;

                if (e.Key == Key.Tab)
                {
                    cursor2.Focus();
                }


                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    releasemark = false;
                    if (_mark[0] == -1 || _mark[1] == -1)
                    {
                        _mark[0] = _cell/2 + (long) fileSlider.Value*16;
                        _mark[1] = _cell/2 + (long) fileSlider.Value*16;
                    }

                    long help = -1;

                    if (_cell/2 + (long) fileSlider.Value*16 < _mark[0])
                    {
                        if (!_markedBackwards)
                        {
                            help = _mark[0];
                        }
                        _markedBackwards = true;
                    }

                    if (_cell/2 + (long) fileSlider.Value*16 > _mark[1])
                    {
                        if (_markedBackwards)
                        {
                            help = _mark[1];
                        }
                        _markedBackwards = false;
                    }

                    if (_cell/2 + (long) fileSlider.Value*16 <= _mark[1] &&
                        _cell/2 + (long) fileSlider.Value*16 >= _mark[0] &&
                        !_markedBackwards)
                    {
                        _mark[1] = _cell/2 + (long) fileSlider.Value*16;
                    }

                    if (_cell/2 + (long) fileSlider.Value*16 <= _mark[1] &&
                        _cell/2 + (long) fileSlider.Value*16 >= _mark[0] &&
                        _markedBackwards)
                    {
                        _mark[0] = _cell/2 + (long) fileSlider.Value*16;
                    }


                    if (_cell/2 + (long) fileSlider.Value*16 < _mark[0])
                    {
                        _mark[0] = _cell/2 + (long) fileSlider.Value*16;
                        if (help != -1)
                        {
                            _mark[1] = help;
                        }
                    }

                    if (_cell/2 + (long) fileSlider.Value*16 > _mark[1])
                    {
                        _mark[1] = _cell/2 + (long) fileSlider.Value*16;
                        if (help != -1)
                        {
                            _mark[0] = help;
                        }
                    }
                    if (controlKey)
                    {
                        _ht.removemarks = false;
                        _st.removemarks = false;
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
                        _ht.mark[0] = -1;
                        _ht.mark[1] = -1;
                        _ht.removemarks = true;
                        _st.removemarks = true;
                    }
                }

                if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.A || Keyboard.IsKeyDown(Key.RightCtrl) && e.Key == Key.A)
                {
                    _mark[0] = 0;
                    _mark[1] = _dyfipro.Length;

                    _st.removemarks = false;
                    _ht.removemarks = false;
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

                        _dyfipro.ApplyChanges();
                    }
                    catch(Exception exp)
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


            updateUI((long) fileSlider.Value);
        }

        private void KeyInputASCIIField(object sender, KeyEventArgs e)
        {
            if (_cursorpositionText > fileSlider.Value*16 + Constants._numberOfCells || _cursorpositionText < fileSlider.Value*16)
            {
                fileSlider.Value = _cursorpositionText/16 - 1;
                fileSlider.Value = Math.Round(fileSlider.Value);
            }

            if (Pfad != "" && Pfad != " ")
            {
                Key k = e.Key;
                string s = k.ToString();
                Boolean controlKey = false;
                if (e.Key == Key.Right)
                {
                    if (_cellText + (long) fileSlider.Value*16 < _dyfipro.Length)
                    {
                        if (_cellText < Constants._numberOfCells-1 && (_cellText + 1) / 16 < _offset - 1)
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
                    if (_cellText + (long) fileSlider.Value*16 + 15 < _dyfipro.Length)
                    {
                        if (_cellText < Constants._helpNumberOfCells2*15 && _cellText/16 < _offset - 2)
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
                        _dyfipro.DeleteBytes(_cellText + (long) fileSlider.Value*16 - 1, 1);

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
                    if (fileSlider.Value < fileSlider.Maximum-16)
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
                    if (Constants._numberOfCells > _dyfipro.Length)
                    {
                        row = (_dyfipro.Length/16)*16;
                    }

                    if (fileSlider.Value != fileSlider.Maximum || _cellText < row)
                    {
                        _cellText = ((_cellText/16) + 1)*16 - 1;
                    }
                    else
                    {
                        _cellText = (int) (row + _dyfipro.Length%16);
                    }
                    controlKey = true;
                    e.Handled = true;
                }


                else if (e.Key == Key.Home)
                {
                    _cellText = ((_cellText/16))*16;
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

                _cursorpositionText = _cellText + (long) fileSlider.Value*16;
                _cursorposition = _cursorpositionText*2;

                //setPosition(cellText*2);
                //setPositionText(cellText);

                Boolean releasemark = true;


                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    releasemark = false;

                    if (_mark[0] == -1 || _mark[1] == -1)
                    {
                        _mark[0] = _cellText + (long) fileSlider.Value*16;
                        _mark[1] = _cellText + (long) fileSlider.Value*16;
                    }

                    long help = -1;

                    if (_cellText + (long) fileSlider.Value*16 < _mark[0])
                    {
                        if (!_markedBackwards)
                        {
                            help = _mark[0];
                        }
                        _markedBackwards = true;
                    }

                    if (_cellText + (long) fileSlider.Value*16 > _mark[1])
                    {
                        if (_markedBackwards)
                        {
                            help = _mark[1];
                        }
                        _markedBackwards = false;
                    }

                    if (_cellText + (long) fileSlider.Value*16 <= _mark[1] &&
                        _cellText + (long) fileSlider.Value*16 >= _mark[0] &&
                        !_markedBackwards)
                    {
                        _mark[1] = _cellText + (long) fileSlider.Value*16;
                    }

                    if (_cellText + (long) fileSlider.Value*16 <= _mark[1] &&
                        _cellText + (long) fileSlider.Value*16 >= _mark[0] &&
                        _markedBackwards)
                    {
                        _mark[0] = _cellText + (long) fileSlider.Value*16;
                    }


                    if (_cellText + (long) fileSlider.Value*16 < _mark[0])
                    {
                        _mark[0] = _cellText + (long) fileSlider.Value*16;
                        if (help != -1)
                        {
                            _mark[1] = help;
                        }
                    }

                    if (_cellText + (long) fileSlider.Value*16 > _mark[1])
                    {
                        _mark[1] = _cellText + (long) fileSlider.Value*16;
                        if (help != -1)
                        {
                            _mark[0] = help;
                        }
                    }
                    if (controlKey)
                    {
                        _st.removemarks = false;
                        _ht.removemarks = false;
                    }
                }
                if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    if (releasemark)
                    {
                        _mark[0] = -1;
                        _mark[1] = -1;

                        _ht.removemarks = true;
                        _st.removemarks = true;
                    }
                }

                if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && e.Key == Key.A)
                {
                    _mark[0] = 0;
                    _mark[1] = _dyfipro.Length;

                    _st.removemarks = false;
                    _ht.removemarks = false;
                }

                if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && e.Key == Key.C)
                {
                    Copy_ASCIIFild();
                    e.Handled = true;
                }

                if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && e.Key == Key.V)
                {
                    ASCIIField_TextInput_Help((String) Clipboard.GetData(DataFormats.Text));
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

            updateUI((long) fileSlider.Value);
        }

        private void backASCIIField()
        {
            Boolean b = true;

            if (Canvas.GetLeft(cursor) > 10)
            {
                Canvas.SetLeft(cursor, Canvas.GetLeft(cursor) - _ht.CharWidth);
                b = false;
            }
            else if (Canvas.GetTop(cursor) > 10)
            {
                Canvas.SetTop(cursor, Canvas.GetTop(cursor) - 20);
                Canvas.SetLeft(cursor, 328.7);
            }


            if (Canvas.GetLeft(cursor2) > 0)
                Canvas.SetLeft(cursor2, Canvas.GetLeft(cursor2) - _st.CharWidth);

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

        private void ASCIIField_TextInput_Help(String e)
        {
            for (int ix = 0; ix < e.Length; ix++)
            {
                if (insertCheck.IsChecked == false)
                {
                    if (_cellText + (long) fileSlider.Value*16 < _dyfipro.Length)
                    {
                        _dyfipro.WriteByte(_cellText + (long) fileSlider.Value*16,
                                           Encoding.GetEncoding(1252).GetBytes(e[ix] + "")[0]);
                    }
                    else
                    {
                        Byte[] dummyArray = {Encoding.GetEncoding(1252).GetBytes(e[ix] + "")[0]};
                        _dyfipro.InsertBytes(_cellText + (long) fileSlider.Value*16,
                                             dummyArray);
                    }
                }
                else
                {
                    Byte[] dummyArray = {Encoding.GetEncoding(1252).GetBytes(e[ix] + "")[0]};


                    _dyfipro.InsertBytes(_cellText + (long) fileSlider.Value*16, dummyArray);
                }

                //nextASCIIField();

                _cursorpositionText++;
                _cellText++;
                if (_cellText == 16*16)
                {
                    if (fileSlider.Value < fileSlider.Maximum)
                    {
                        fileSlider.Value += 1;
                        _cursorpositionText++;
                    }
                }

                if ((_cellText)/16 > _offset - 1 && _cellText != 16*16 - 1)
                {
                    if (fileSlider.Value < fileSlider.Maximum)
                    {
                        fileSlider.Value += 1;
                        //this.cursorposition++;
                    }
                }
                if (_cellText == 16*16 - 1)
                {
                    if (fileSlider.Value < fileSlider.Maximum)
                    {
                        fileSlider.Value += 1;
                        //this.cursorposition++;
                    }
                }
            }

            _cursorposition = _cursorpositionText*2;

            //setPosition(cellText * 2);
            //setPositionText(cellText);

            updateUI((long) fileSlider.Value);
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

        private void HexBox_TextInput_Help(String e)
        {
            String validCharTest = e.ToLower();
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
                    Byte[] dummyArray = {Encoding.GetEncoding(1252).GetBytes(e)[ix]};

                    var b = new byte();

                    string s = "00";

                    char c = e[ix];

                    if (_cell/2 + (long) fileSlider.Value*16 < _dyfipro.Length)
                    {

                        try
                        {
                            s = String.Format("{0:X2}", _dyfipro.ReadByte(_cell/2 + (long) fileSlider.Value*16));
                        }
                        catch(Exception exp)
                        {
                            _dyfipro.Dispose();
                            ErrorOccured(this, new GUIErrorEventArgs(exp.Message + "DynamicFileByteProvider cannot read from file"));
                        }
                    }

                    if (insertCheck.IsChecked == false)
                    {
                        if (_cell%2 == 0)
                        {
                            int i = e[ix];

                            if (e[0] > 96 && e[0] < 103 || e[0] > 47 && e[0] < 58)
                            {
                                s = c + "" + s[1];
                            }

                            if (_cell/2 + (long) fileSlider.Value*16 < _dyfipro.Length)
                            {
                                _dyfipro.WriteByte(_cell/2 + (long) fileSlider.Value*16,
                                                   (byte) Convert.ToInt32(s, 16));
                            }
                            else
                            {
                                s = s[0] + "" + c;
                                dummyArray[0] = (byte) Convert.ToInt32(s, 16);
                                try
                                {
                                    _dyfipro.InsertBytes(_cell/2 + (long) fileSlider.Value*16, dummyArray);
                                }
                                catch (Exception exp)
                                {
                                    ErrorOccured(this, new GUIErrorEventArgs(exp.Message));
                                }
                            }
                        }
                        if (_cell%2 == 1)
                        {
                            if (e[0] > 96 && e[0] < 103 || e[0] > 47 && e[0] < 58)
                            {
                                s = s[0] + "" + c;
                            }
                            if (_cell/2 + (long) fileSlider.Value*16 < _dyfipro.Length)
                            {
                                _dyfipro.WriteByte(_cell/2 + (long) fileSlider.Value*16,
                                                   (byte) Convert.ToInt32(s, 16));
                            }
                        }
                    }
                    else
                    {
                        if (_cell%2 == 0)
                        {
                            int i = e[0];


                            if (e[0] > 96 && e[0] < 103 || e[0] > 47 && e[0] < 58)
                            {
                                s = c + "0";
                            }
                            dummyArray[0] = (byte) Convert.ToInt32(s, 16);
                            _dyfipro.InsertBytes(_cell/2 + (long) fileSlider.Value*16, dummyArray);
                        }
                        if (_cell%2 == 1)
                        {
                            if (e[0] > 96 && e[0] < 103 || e[0] > 47 && e[0] < 58)
                            {
                                s = s[0] + "" + c;
                            }
                            if (_cell/2 + (long) fileSlider.Value*16 < _dyfipro.Length)
                            {
                                _dyfipro.WriteByte(_cell/2 + (long) fileSlider.Value*16, (byte) Convert.ToInt32(s, 16));
                            }
                        }
                    }

                    _cursorposition++;
                    _cell++;
                    if ((_cell)/32 > _offset - 1 && _cell != 32*16 - 1)
                    {
                        if (fileSlider.Value < fileSlider.Maximum)
                        {
                            fileSlider.Value += 1;
                            //this.cursorposition++;
                        }
                    }
                    if (_cell == 32*16 - 1)
                    {
                        if (fileSlider.Value < fileSlider.Maximum)
                        {
                            fileSlider.Value += 1;
                            //this.cursorposition++;
                        }
                    }
                }
            }

            _cursorpositionText = _cursorposition/2;

            //setPositionText(cell / 2);
            //setPosition(cell);

            updateUI((long) fileSlider.Value);
        }

        #endregion

        #region Public Methods

        public AsyncCallback callback()
        {
            return null;
        }

        public void dispose() //Disposes File See IDisposable for further information
        {

            window = null;
            _dyfipro.Dispose();
        }

        private void updateUI(long position) // Updates UI
        {
            Column.Text = (int) (_cursorpositionText%16 + 1) + "";
            Line.Text = _cursorpositionText/16 + 1 + "";

            long end = _dyfipro.Length - position*16;
            if (end < 0)
                end = 0;

            int max = Constants._numberOfCells;

            _st.Text = "";

            Byte[] help2;
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
                            help2[i] = _dyfipro.ReadByte(i + position * 16);
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                _dyfipro.Dispose();
                ErrorOccured(this, new GUIErrorEventArgs(exp.Message + "DynamicFileByteProvider cannot read from file"));
            }

            _ht.ByteContent = help2;

            if (_cursorpositionText - fileSlider.Value*16 < Constants._numberOfCells && _cursorpositionText - fileSlider.Value*16 >= 0)
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
                var id = gridid.Children[j] as TextBlock;

                long s = (position + j)*16;
                id.Text = "";
                for (int x = 8 - s.ToString("X").Length; x > 0; x--)
                {
                    id.Text += "0";
                }
                id.Text += s.ToString("X");
            }

            _ht.mark[0] = (int) (_mark[0] - position*16)*2;
            _ht.mark[1] = (int) (_mark[1] - position*16)*2;

            _st.mark[0] = (int) (_mark[0] - position*16);
            _st.mark[1] = (int) (_mark[1] - position*16);

            setPositionText((int) (_cursorpositionText - fileSlider.Value*16));
            setPosition((int) (_cursorposition - fileSlider.Value*32));

            _lastUpdate = position;
            _unicorn = true;
        }

        private void dyfipro_LengthChanged(object sender, EventArgs e) // occures when length of file changed 
        {
            double old = fileSlider.Maximum;

            if (_offset <= Constants._helpNumberOfCells2)
            {
                fileSlider.Maximum = (_dyfipro.Length - Constants._numberOfCells) / 16 + 2 + Constants._helpNumberOfCells2 - _offset;
                fileSlider.Maximum = Math.Round(fileSlider.Maximum);
               
            }
            else
            {
                fileSlider.Maximum = (_dyfipro.Length - Constants._numberOfCells)/16 + 2;
                fileSlider.Maximum = Math.Round(fileSlider.Maximum);
               
            }

            if ((long) old > (long) fileSlider.Maximum && fileSlider.Value == fileSlider.Maximum)
            {
                //_cellText += 16;
                //_cell += 32;
            }

            if ((long) old < (long) fileSlider.Maximum && fileSlider.Value == fileSlider.Maximum)
            {
                _cellText -= 16;
                _cell -= 32;
            }
        }

        public void openFile(String fileName, Boolean canRead) // opens file 
        {
            _dyfipro.Dispose();

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
                Pfad = fileName;
                try
                {
                    _dyfipro = new DynamicFileByteProvider(Pfad, canRead);
                    makeUnAccesable(true);
                }
                catch (IOException ioe)
                {
                    _dyfipro = new DynamicFileByteProvider(Pfad, true);
                    makeUnAccesable(false);
                    ErrorOccured(this, new GUIErrorEventArgs(ioe.Message + "File will be opened in ReadOnlyMode"));
                }


                _dyfipro.LengthChanged += dyfipro_LengthChanged;

                fileSlider.Minimum = 0;
                fileSlider.Maximum = (_dyfipro.Length - Constants._numberOfCells) / 16 + Constants._helpNumberOfCells2 - 1;
                fileSlider.Maximum = Math.Round(fileSlider.Maximum);
                fileSlider.ViewportSize = Constants._helpNumberOfCells2 ;

                _info.Text = _dyfipro.Length/Constants._numberOfCells + "";

                fileSlider.SmallChange = 1;
                fileSlider.LargeChange = 1;

                setPosition(0);
                setPositionText(0);


                updateUI(0);
            }
        }

        public void closeFile(Boolean clear) // closes file
        {
            _dyfipro.Dispose();
            


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

            this.Loaded -= new RoutedEventHandler(HexBox_Loaded);
            
                
        }

        public Boolean saveData(Boolean ask, Boolean saveas) // saves changed data to file
        {
            try
            {
                if (_dyfipro.Length != 0)
                    if (_dyfipro.HasChanges() || saveas)
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

                                var saveFileDialog1 = new SaveFileDialog();
                                saveFileDialog1.Title = "Save Data";
                                saveFileDialog1.FileName = Pfad;
                                saveFileDialog1.ShowDialog();


                                // If the file name is not an empty string open it for saving.
                                if (saveFileDialog1.FileName != "")
                                {
                                    // Saves the Image via a FileStream created by the OpenFile method.

                                    if (saveFileDialog1.FileName != Pfad)
                                    {
                                        var fs = (FileStream) saveFileDialog1.OpenFile();


                                        for (long i = 0; i < _dyfipro.Length; i++)
                                        {
                                            fs.WriteByte(_dyfipro.ReadByte(i));
                                        }
                                        FileName.Text = saveFileDialog1.FileName;
                                        Pfad = saveFileDialog1.FileName;
                                        fs.Close();
                                    }
                                    else
                                    {
                                        _dyfipro.ApplyChanges();
                                    }
                                }
                                OnFileChanged(this, EventArgs.Empty);
                                break;
                            case MessageBoxResult.No:
                                _dyfipro.ApplyChanges();
                                break;
                            case MessageBoxResult.Cancel:
                                // User pressed Cancel button
                                // ...
                                break;
                        }
                    }
            }
            catch (Exception e)
            {
                _dyfipro.Dispose();
                ErrorOccured(this, new GUIErrorEventArgs(e.Message));
            }

            return true;
        }

        public void collapseControl(Boolean b) // changes visibility of user controls, when HexBox is not visible
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

        private void makeUnFocused(Boolean b) // allows or doesn't allow manipulation of data
        {
            
            if (b&&this.IsEnabled)
            {
                _st.brush = Brushes.Orange;
                _ht.brush = Brushes.Orange;
                cursor.Visibility = Visibility.Visible;
                cursor2.Visibility = Visibility.Visible;
            }
            else
            {
                _ht.brush = Brushes.DarkGray;
                _st.brush = Brushes.DarkGray;
                cursor.Visibility = Visibility.Collapsed;
                cursor2.Visibility = Visibility.Collapsed;

            }
            updateUI((long)fileSlider.Value);
        }


        public void makeUnAccesable(Boolean b) // allows or doesn't allows manipulation of data
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
            _dyfipro.Dispose();

            try
            {
                var openFileDialog = new OpenFileDialog();
                //openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog().Value)
                {
                    Pfad = openFileDialog.FileName;
                }
                if (Pfad != "" && File.Exists(Pfad))
                {
                    FileName.Text = Pfad;
                    try
                    {
                        _dyfipro = new DynamicFileByteProvider(Pfad, false);
                        makeUnAccesable(true);
                    }
                    catch (IOException ioe)
                    {
                        _dyfipro = new DynamicFileByteProvider(Pfad, true);
                        makeUnAccesable(false);
                        InReadOnlyMode = true;
                        ErrorOccured(this, new GUIErrorEventArgs(ioe.Message + "File will be opened in ReadOnlyMode"));
                    }

                    _dyfipro.LengthChanged += dyfipro_LengthChanged;

                    fileSlider.Minimum = 0;
                    fileSlider.Maximum = (_dyfipro.Length - Constants._numberOfCells) / 16 + Constants._helpNumberOfCells2-1;
                    fileSlider.Maximum = Math.Round(fileSlider.Maximum);
                    fileSlider.ViewportSize = Constants._helpNumberOfCells2;

                    _info.Text = _dyfipro.Length/Constants._numberOfCells + "";

                    fileSlider.SmallChange = 1;
                    fileSlider.LargeChange = 1;

                    reset();

                    updateUI(0);

                    OnFileChanged(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                _dyfipro.Dispose();
                ErrorOccured(this, new GUIErrorEventArgs(ex.Message));
            }
        }

        private void New_Button_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Title = "New File";
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                _dyfipro.Dispose();
                // Saves the Image via a FileStream created by the OpenFile method.
                try
                {
                    var fs = (FileStream) saveFileDialog1.OpenFile();

                    fs.Dispose();
                    fs.Close();

                    Pfad = saveFileDialog1.FileName;

                    FileName.Text = Pfad;

                    _dyfipro = new DynamicFileByteProvider(Pfad, false);
                    _dyfipro.LengthChanged += dyfipro_LengthChanged;

                    fileSlider.Minimum = 0;
                    fileSlider.Maximum = (_dyfipro.Length - Constants._numberOfCells) / 16 + Constants._helpNumberOfCells2 - 1;
                    fileSlider.Maximum = Math.Round(fileSlider.Maximum);
                    fileSlider.ViewportSize = Constants._helpNumberOfCells2;

                    _info.Text = _dyfipro.Length/Constants._numberOfCells + "";

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
            _ht.removemarks = true;
            _st.removemarks = true;
        }

        private void help_copy_Hexbox()
        {
            var clipBoardString = new StringBuilder();
            if (_mark[1] > _dyfipro.Length)
                _mark[1] = _dyfipro.Length;


            for (long i = _mark[0]; i < _mark[1]; i++)
            {
                try
                {
                    clipBoardString.Append(String.Format("{0:X2}", _dyfipro.ReadByte(i)));
                }
                catch(Exception exp)
                {
                    _dyfipro.Dispose();
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

            _ht.removemarks = true;
            updateUI((long) fileSlider.Value);
        }

        private void deletion_HexBoxField()
        {
            int celltemp = _cell;
            if (_cell > 0)
            {
                if (celltemp/2 + (long) fileSlider.Value*16 - 2 < _dyfipro.Length)
                {
                    if (_mark[1] - _mark[0] == 0)
                    {
                        if (celltemp/2 + (int) fileSlider.Value*16 - 1 > -1)
                        {
                            if (celltemp / 2 + (long)fileSlider.Value * 16 > -1)
                            _dyfipro.DeleteBytes(celltemp/2 + (long) fileSlider.Value*16, 1);
                        }
                    }
                    else
                    {
                        if (_markedBackwards)
                        {
                            if(_mark[0] >-1 && _mark[1] - _mark[0]>-1)
                            _dyfipro.DeleteBytes(_mark[0], _mark[1] - _mark[0]);
                        }
                        else
                        {
                            if (_mark[0] > -1 && _mark[1] - _mark[0] > -1)
                            _dyfipro.DeleteBytes(_mark[0], _mark[1] - _mark[0]);

                            if (_mark[1] - _mark[0] > celltemp)
                            {
                                fileSlider.Value = _mark[0]/16;
                                fileSlider.Value = Math.Round(fileSlider.Value);
                            }

                            _cell = (int) (_mark[0]*2 - (long) fileSlider.Value*32);
                            Canvas.SetLeft(cursor, _mark[0]%16*_ht.CharWidth*3);
                        }
                    }

                    _cursorposition = _mark[0]*2;
                    _cursorpositionText = _mark[0];

                    _mark[1] = -1;
                    _mark[0] = -1;

                    _ht.removemarks = true;
                    _st.removemarks = true;
                    updateUI((long) fileSlider.Value);
                }
            }
        }

        private void deletion_ASCIIField()
        {
            if (_cellText > 0)
            {
                if (_cellText + (int) fileSlider.Value*16 - 2 < _dyfipro.Length)
                {
                    if (_mark[1] - _mark[0] == 0)
                    {
                        if (_cellText + (int) fileSlider.Value*16 - 1 > -1)
                        {
                            _dyfipro.DeleteBytes(_cellText + (long) fileSlider.Value*16 - 1, 1);
                            backASCIIField();
                        }
                    }
                    else
                    {
                        if (_markedBackwards)
                        {
                            if (_mark[0] > -1 && _mark[1] - _mark[0] > -1)
                                _dyfipro.DeleteBytes(_mark[0], _mark[1] - _mark[0]);
                        }
                        else
                        {
                            if (_mark[0] > -1 && _mark[1] - _mark[0] > -1)
                                _dyfipro.DeleteBytes(_mark[0], _mark[1] - _mark[0]);
                            if (_mark[1] - _mark[0] > _cellText)
                            {
                                fileSlider.Value = _mark[0]/16;
                                fileSlider.Value = Math.Round(fileSlider.Value);
                            }

                            _cellText = (int) (_mark[0] - (long) fileSlider.Value*16);
                        }
                    }


                    _ht.removemarks = true;
                    _st.removemarks = true;


                    _cursorposition = _mark[0]*2;
                    _cursorpositionText = _mark[0];


                    _mark[1] = -1;
                    _mark[0] = -1;
                    updateUI((long) fileSlider.Value);
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
            var clipBoardString = new StringBuilder();
            try
            {
            for (long i = _mark[0]; i < _mark[1]; i++)
            {
                if (_dyfipro.ReadByte(i) > 34 && _dyfipro.ReadByte(i) < 128)
                {
                    clipBoardString.Append((char) _dyfipro.ReadByte(i));
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
                _dyfipro.Dispose();
                ErrorOccured(this, new GUIErrorEventArgs(exp.Message));
            }
            _mark[1] = -1;
            _mark[0] = -1;

            updateUI((long) fileSlider.Value);
        }

        private void Paste_ASCIIFild()
        {
            if (_cellText + (int) fileSlider.Value*16 < _dyfipro.Length)
            {
                if (_mark[1] - _mark[0] != 0)
                {
                    if (_markedBackwards)
                    {
                        try
                        {
                            _dyfipro.DeleteBytes(_mark[0] + 1, _mark[1] - _mark[0]);
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
                            _dyfipro.DeleteBytes(_mark[0], _mark[1] - _mark[0]);
                        }
                        catch (Exception e)
                        {
                            ErrorOccured(this, new GUIErrorEventArgs(e.Message));
                        }

                        if (_mark[1] - _mark[0] > _cell)
                        {
                            fileSlider.Value = _mark[0]/16;
                            fileSlider.Value = Math.Round(fileSlider.Value);
                        }
                    }
                }


                _mark[1] = -1;
                _mark[0] = -1;
                updateUI((long) fileSlider.Value);
            }

            if (_markedBackwards)
                _cellText = (int) (Canvas.GetTop(cursor)/20*16 + Canvas.GetLeft(cursor)/10 + 2);
            else
                _cellText = (int) (Canvas.GetTop(cursor)/20*16 + Canvas.GetLeft(cursor)/10);

            var text = (String) Clipboard.GetData(DataFormats.Text);

            try
            {
                _dyfipro.InsertBytes(_cellText + (int) fileSlider.Value*16, enc.GetBytes(text));
            }
            catch (Exception e)
            {
                ErrorOccured(this, new GUIErrorEventArgs(e.Message));
            }

            updateUI((long) fileSlider.Value);
        }

        private void help_copy_ASCIIField()
        {
            var clipBoardString = new StringBuilder();
            if (_mark[1] > _dyfipro.Length)
                _mark[1] = _dyfipro.Length;

            try
            {
            for (long i = _mark[0]; i < _mark[1]; i++)
            {
                if (_dyfipro.ReadByte(i) > 34 && _dyfipro.ReadByte(i) < 128)
                {
                    clipBoardString.Append((char) _dyfipro.ReadByte(i));
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
                _dyfipro.Dispose();
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
                _dyfipro.ApplyChanges();
            }
            catch(Exception exp)
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
            fileSlider.Value -= e.Delta/10;


            e.Handled = true;
        }

        

        private void MyManipulationCompleteEvent(object sender, EventArgs e)
        {
            fileSlider.Value = Math.Round(fileSlider.Value);


            if (_lastUpdate != fileSlider.Value && _unicorn)
            {
                _unicorn = false;
                updateUI((long) fileSlider.Value);
            }

            _info.Text = (long) fileSlider.Value + "" + Math.Round(fileSlider.Value*16, 0) + fileSlider.Value;
        }

        private void scoview_MouseDown(object sender, MouseButtonEventArgs e)
        {
            canvas1.Focus();
        }

        private void scoview_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            scoview.ScrollToTop();
            _offset = scoview.ViewportHeight/20;
            if(_offset < 2 )
            {
                _offset = 2;
            }

            if (_offset < Constants._helpNumberOfCells2)
            {
                fileSlider.Maximum = _dyfipro.Length/16 + 2 - _offset;
                fileSlider.Maximum = Math.Round(fileSlider.Maximum);
            }
            else
            {
                fileSlider.Maximum = _dyfipro.Length/16 - Constants._helpNumberOfCells2 - 1;
                fileSlider.Maximum = Math.Round(fileSlider.Maximum);
            }
        }

        #endregion

        public void Clear()
        {
            fileSlider.Minimum = 0;
            fileSlider.Maximum = (_dyfipro.Length - Constants._numberOfCells)/16 + 1;
            fileSlider.Maximum = Math.Round(fileSlider.Maximum);
            fileSlider.SmallChange = 1;
            fileSlider.LargeChange = 1;

            FileName.Text = "";

            closeFile(false);
            Stream s = new MemoryStream();
            _dyfipro = new DynamicFileByteProvider(s);
            _dyfipro.LengthChanged += dyfipro_LengthChanged;
            OnFileChanged(this, EventArgs.Empty);
            reset();
            updateUI(0);
        }
        #region Events
        public delegate void GUIErrorEventHandler(object sender, GUIErrorEventArgs ge);

        // Now, create a public event "FireEvent" whose type is our FireEventHandler delegate. 


        public event GUIErrorEventHandler ErrorOccured;

        #endregion

    }

    public class GUIErrorEventArgs : EventArgs
    {
        public GUIErrorEventArgs(string message)
        {
            this.message = message;

        }

        public string message;

    }	
}