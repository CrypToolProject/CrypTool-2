using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace CrypTool.Internet_frame_generator
{
    /// <summary>
    /// Interaktionslogik für Internet_frame_generator_Presentation.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("Internet_frame_generator.Properties.Resources")]
    public partial class Internet_frame_generator_Presentation : UserControl
    {
        #region Private variables

        /// <summary>
        /// Values of a typical IP packet (v4).
        /// </summary>
        private static readonly byte[] iPPacket = { 0x00, 0x12, 0xBF, 0xDC, 0x4E, 0x7A, 0x00, 0xA0,
                                                      0xD1, 0x25, 0xB9, 0xEC, 0x08, 0x00, 0x45, 0x00,
                                                      0x00, 0x3C, 0x11, 0x2B, 0x40, 0x00, 0x40, 0x06,
                                                      0x97, 0xF8, 0xC0, 0xA8, 0x02, 0x66, 0xC3, 0x47,
                                                      0x0B, 0x43, 0xE5, 0xD3, 0x00, 0x50, 0x83, 0x74,
                                                      0xA4, 0xB9, 0x00, 0x00, 0x00, 0x00, 0xA0, 0x02,
                                                      0x16, 0xD0, 0xBD, 0x58, 0x00, 0x00, 0x02, 0x04,
                                                      0x05, 0xB4, 0x04, 0x02, 0x08, 0x0A, 0x00, 0x5B,
                                                      0xB5, 0x92, 0x00, 0x00, 0x00, 0x00, 0x01, 0x03,
                                                      0x03, 0x06 };

        /// <summary>
        /// Values of a typical ARP packet.
        /// </summary>
        private static readonly byte[] aRPPacket = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x12,
                                                       0xBF, 0xDB, 0x5F, 0x7A, 0x08, 0x06, 0x00, 0x01,
                                                       0x08, 0x00, 0x06, 0x04, 0x00, 0x01, 0x21, 0x21,
                                                       0x21, 0x21, 0x21, 0x21, 0xDC, 0x9C, 0x49, 0xC6,
                                                       0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x4B, 0xB2,
                                                       0x2E, 0xD7 };

        /// <summary>
        /// Contains all pathes for the IP packets.
        /// </summary>
        private readonly Path[,] pathesIP = new Path[10, 8];

        /// <summary>
        /// Contains all pathes for the ARP packets.
        /// </summary>
        private readonly Path[,] pathesARP = new Path[10, 8];

        private readonly Storyboard[,] myStoryboardInArray = new Storyboard[2, 74];
        private readonly Storyboard[,] myStoryboardOutArray = new Storyboard[2, 74];

        #endregion

        #region Private help methods

        /// <summary>
        /// Fills the <see cref="Path"/>[] with IP and ARP pathes for the <see cref="GeometryRectangle"/>s
        /// (defined in XAML file).
        /// </summary>
        private void fillPathArray()
        {
            pathesIP[0, 0] = pathIP1_1; pathesIP[1, 0] = pathIP2_1;
            pathesIP[0, 1] = pathIP1_2; pathesIP[1, 1] = pathIP2_2;
            pathesIP[0, 2] = pathIP1_3; pathesIP[1, 2] = pathIP2_3;
            pathesIP[0, 3] = pathIP1_4; pathesIP[1, 3] = pathIP2_4;
            pathesIP[0, 4] = pathIP1_5; pathesIP[1, 4] = pathIP2_5;
            pathesIP[0, 5] = pathIP1_6; pathesIP[1, 5] = pathIP2_6;
            pathesIP[0, 6] = pathIP1_7; pathesIP[1, 6] = pathIP2_7;
            pathesIP[0, 7] = pathIP1_8; pathesIP[1, 7] = pathIP2_8;

            pathesIP[2, 0] = pathIP3_1; pathesIP[3, 0] = pathIP4_1;
            pathesIP[2, 1] = pathIP3_2; pathesIP[3, 1] = pathIP4_2;
            pathesIP[2, 2] = pathIP3_3; pathesIP[3, 2] = pathIP4_3;
            pathesIP[2, 3] = pathIP3_4; pathesIP[3, 3] = pathIP4_4;
            pathesIP[2, 4] = pathIP3_5; pathesIP[3, 4] = pathIP4_5;
            pathesIP[2, 5] = pathIP3_6; pathesIP[3, 5] = pathIP4_6;
            pathesIP[2, 6] = pathIP3_7; pathesIP[3, 6] = pathIP4_7;
            pathesIP[2, 7] = pathIP3_8; pathesIP[3, 7] = pathIP4_8;

            pathesIP[4, 0] = pathIP5_1; pathesIP[5, 0] = pathIP6_1;
            pathesIP[4, 1] = pathIP5_2; pathesIP[5, 1] = pathIP6_2;
            pathesIP[4, 2] = pathIP5_3; pathesIP[5, 2] = pathIP6_3;
            pathesIP[4, 3] = pathIP5_4; pathesIP[5, 3] = pathIP6_4;
            pathesIP[4, 4] = pathIP5_5; pathesIP[5, 4] = pathIP6_5;
            pathesIP[4, 5] = pathIP5_6; pathesIP[5, 5] = pathIP6_6;
            pathesIP[4, 6] = pathIP5_7; pathesIP[5, 6] = pathIP6_7;
            pathesIP[4, 7] = pathIP5_8; pathesIP[5, 7] = pathIP6_8;

            pathesIP[6, 0] = pathIP7_1; pathesIP[7, 0] = pathIP8_1;
            pathesIP[6, 1] = pathIP7_2; pathesIP[7, 1] = pathIP8_2;
            pathesIP[6, 2] = pathIP7_3; pathesIP[7, 2] = pathIP8_3;
            pathesIP[6, 3] = pathIP7_4; pathesIP[7, 3] = pathIP8_4;
            pathesIP[6, 4] = pathIP7_5; pathesIP[7, 4] = pathIP8_5;
            pathesIP[6, 5] = pathIP7_6; pathesIP[7, 5] = pathIP8_6;
            pathesIP[6, 6] = pathIP7_7; pathesIP[7, 6] = pathIP8_7;
            pathesIP[6, 7] = pathIP7_8; pathesIP[7, 7] = pathIP8_8;

            pathesIP[8, 0] = pathIP9_1; pathesIP[9, 0] = pathIP10_1;
            pathesIP[8, 1] = pathIP9_2; pathesIP[9, 1] = pathIP10_2;
            pathesIP[8, 2] = pathIP9_3; pathesIP[9, 2] = pathIP10_3;
            pathesIP[8, 3] = pathIP9_4; pathesIP[9, 3] = pathIP10_4;
            pathesIP[8, 4] = pathIP9_5; pathesIP[9, 4] = pathIP10_5;
            pathesIP[8, 5] = pathIP9_6; pathesIP[9, 5] = pathIP10_6;
            pathesIP[8, 6] = pathIP9_7; pathesIP[9, 6] = pathIP10_7;
            pathesIP[8, 7] = pathIP9_8; pathesIP[9, 7] = pathIP10_8;

            pathesARP[0, 0] = pathARP1_1; pathesARP[1, 0] = pathARP2_1;
            pathesARP[0, 1] = pathARP1_2; pathesARP[1, 1] = pathARP2_2;
            pathesARP[0, 2] = pathARP1_3; pathesARP[1, 2] = pathARP2_3;
            pathesARP[0, 3] = pathARP1_4; pathesARP[1, 3] = pathARP2_4;
            pathesARP[0, 4] = pathARP1_5; pathesARP[1, 4] = pathARP2_5;
            pathesARP[0, 5] = pathARP1_6; pathesARP[1, 5] = pathARP2_6;
            pathesARP[0, 6] = pathARP1_7; pathesARP[1, 6] = pathARP2_7;
            pathesARP[0, 7] = pathARP1_8; pathesARP[1, 7] = pathARP2_8;

            pathesARP[2, 0] = pathARP3_1; pathesARP[3, 0] = pathARP4_1;
            pathesARP[2, 1] = pathARP3_2; pathesARP[3, 1] = pathARP4_2;
            pathesARP[2, 2] = pathARP3_3; pathesARP[3, 2] = pathARP4_3;
            pathesARP[2, 3] = pathARP3_4; pathesARP[3, 3] = pathARP4_4;
            pathesARP[2, 4] = pathARP3_5; pathesARP[3, 4] = pathARP4_5;
            pathesARP[2, 5] = pathARP3_6; pathesARP[3, 5] = pathARP4_6;
            pathesARP[2, 6] = pathARP3_7; pathesARP[3, 6] = pathARP4_7;
            pathesARP[2, 7] = pathARP3_8; pathesARP[3, 7] = pathARP4_8;

            pathesARP[4, 0] = pathARP5_1; pathesARP[5, 0] = pathARP6_1;
            pathesARP[4, 1] = pathARP5_2; pathesARP[5, 1] = pathARP6_2;
            pathesARP[4, 2] = pathARP5_3; pathesARP[5, 2] = pathARP6_3;
            pathesARP[4, 3] = pathARP5_4; pathesARP[5, 3] = pathARP6_4;
            pathesARP[4, 4] = pathARP5_5; pathesARP[5, 4] = pathARP6_5;
            pathesARP[4, 5] = pathARP5_6; pathesARP[5, 5] = pathARP6_6;
            pathesARP[4, 6] = pathARP5_7; pathesARP[5, 6] = pathARP6_7;
            pathesARP[4, 7] = pathARP5_8; pathesARP[5, 7] = pathARP6_8;

            pathesARP[6, 0] = pathARP7_1; pathesARP[7, 0] = pathARP8_1;
            pathesARP[6, 1] = pathARP7_2; pathesARP[7, 1] = pathARP8_2;
            pathesARP[6, 2] = pathARP7_3; pathesARP[7, 2] = pathARP8_3;
            pathesARP[6, 3] = pathARP7_4; pathesARP[7, 3] = pathARP8_4;
            pathesARP[6, 4] = pathARP7_5; pathesARP[7, 4] = pathARP8_5;
            pathesARP[6, 5] = pathARP7_6; pathesARP[7, 5] = pathARP8_6;
            pathesARP[6, 6] = pathARP7_7; pathesARP[7, 6] = pathARP8_7;
            pathesARP[6, 7] = pathARP7_8; pathesARP[7, 7] = pathARP8_8;

            pathesARP[8, 0] = pathARP9_1; pathesARP[9, 0] = pathARP10_1;
            pathesARP[8, 1] = pathARP9_2; pathesARP[9, 1] = pathARP10_2;
            pathesARP[8, 2] = pathARP9_3; pathesARP[9, 2] = pathARP10_3;
            pathesARP[8, 3] = pathARP9_4; pathesARP[9, 3] = pathARP10_4;
            pathesARP[8, 4] = pathARP9_5; pathesARP[9, 4] = pathARP10_5;
            pathesARP[8, 5] = pathARP9_6; pathesARP[9, 5] = pathARP10_6;
            pathesARP[8, 6] = pathARP9_7; pathesARP[9, 6] = pathARP10_7;
            pathesARP[8, 7] = pathARP9_8; pathesARP[9, 7] = pathARP10_8;
        }

        /// <summary>
        /// Creates the <see cref="GeometryRectangles"/> with <see cref="TextBlock"/>s in them,
        /// which contains the value of the packet at the special place.
        /// </summary>
        private void createPacketCubes()
        {
            int i = 0;

            VisualBrush visualBrush;

            StackPanel stackPanel;

            DrawingBrush drawingBrush;

            GeometryGroup geometryGroup;

            RadialGradientBrush ragrbr;

            GeometryDrawing gedr;

            TextBlock textBlock;

            Path temp;

            for (int j = 1; j <= 12; j++)
            {
                for (int k = 0; k < 8; k++)
                {
                    if (i < 74)
                    {
                        temp = pathesIP[j - 1, k];
                        temp.Data = new RectangleGeometry(new Rect(k, j, 30, 30));

                        drawingBrush = new DrawingBrush();
                        geometryGroup = new GeometryGroup();
                        geometryGroup.Children.Add(new RectangleGeometry(new Rect(k, j, 30, 30)));

                        ragrbr = new RadialGradientBrush();
                        ragrbr.GradientStops.Add(new GradientStop(Colors.LightGray, 0.0));

                        gedr = new GeometryDrawing(ragrbr, null, geometryGroup);

                        drawingBrush.Drawing = gedr;
                        stackPanel = new StackPanel
                        {
                            Background = drawingBrush
                        };

                        textBlock = new TextBlock
                        {
                            Text = string.Format("{0:X2}", iPPacket[i])
                        };
                        FontSizeConverter fsc = new FontSizeConverter();
                        textBlock.FontSize = (double)fsc.ConvertFromString("12pt");
                        textBlock.Margin = new Thickness(10);

                        stackPanel.Children.Add(textBlock);
                        visualBrush = new VisualBrush
                        {
                            Visual = stackPanel
                        };
                        temp.Fill = visualBrush;

                        temp.Opacity = 0.5;

                        DoubleAnimation myDoubleAnimationIn = new DoubleAnimation
                        {
                            From = 0.5,
                            To = 1.0,
                            Duration = new Duration(TimeSpan.FromSeconds(1)),
                            AutoReverse = false
                        };

                        DoubleAnimation myDoubleAnimationOut = new DoubleAnimation
                        {
                            From = 1.0,
                            To = 0.5,
                            Duration = new Duration(TimeSpan.FromSeconds(1)),
                            AutoReverse = false
                        };

                        Storyboard myStoryboardIn = new Storyboard();
                        myStoryboardIn.Children.Add(myDoubleAnimationIn);
                        Storyboard.SetTargetName(myDoubleAnimationIn, temp.Name);
                        Storyboard.SetTargetProperty(myDoubleAnimationIn, new PropertyPath(Path.OpacityProperty));

                        Storyboard myStoryboardOut = new Storyboard();
                        myStoryboardOut.Children.Add(myDoubleAnimationOut);
                        Storyboard.SetTargetName(myDoubleAnimationOut, temp.Name);
                        Storyboard.SetTargetProperty(myDoubleAnimationOut, new PropertyPath(Path.OpacityProperty));

                        temp.MouseEnter += new MouseEventHandler(mouseEnter);
                        temp.MouseLeave += new MouseEventHandler(mouseLeave);

                        myStoryboardInArray[0, i] = myStoryboardIn;
                        myStoryboardOutArray[0, i] = myStoryboardOut;

                        pathesIP[j - 1, k].Data = temp.Data;
                    }
                    i++;
                }
            }
            i = 0;
            for (int j = 1; j <= 8; j += 1)
            {
                for (int k = 0; k < 8; k += 1)
                {
                    if (i < 42)
                    {
                        temp = pathesARP[j - 1, k];
                        temp.Data = new RectangleGeometry(new Rect(k, j, 30, 30));

                        drawingBrush = new DrawingBrush();
                        geometryGroup = new GeometryGroup();
                        geometryGroup.Children.Add(new RectangleGeometry(new Rect(k, j, 30, 30)));

                        ragrbr = new RadialGradientBrush();
                        ragrbr.GradientStops.Add(new GradientStop(Colors.LightGray, 0.0));

                        gedr = new GeometryDrawing(ragrbr, null, geometryGroup);

                        drawingBrush.Drawing = gedr;
                        stackPanel = new StackPanel
                        {
                            Background = drawingBrush
                        };

                        textBlock = new TextBlock
                        {
                            Text = string.Format("{0:X2}", aRPPacket[i])
                        };
                        FontSizeConverter fsc = new FontSizeConverter();
                        textBlock.FontSize = (double)fsc.ConvertFromString("12pt");
                        textBlock.Margin = new Thickness(10);

                        stackPanel.Children.Add(textBlock);
                        visualBrush = new VisualBrush
                        {
                            Visual = stackPanel
                        };
                        temp.Fill = visualBrush;

                        temp.Opacity = 0.5;

                        DoubleAnimation myDoubleAnimationIn = new DoubleAnimation
                        {
                            From = 0.5,
                            To = 1.0,
                            Duration = new Duration(TimeSpan.FromSeconds(1)),
                            AutoReverse = false
                        };

                        DoubleAnimation myDoubleAnimationOut = new DoubleAnimation
                        {
                            From = 1.0,
                            To = 0.5,
                            Duration = new Duration(TimeSpan.FromSeconds(1)),
                            AutoReverse = false
                        };

                        Storyboard myStoryboardIn = new Storyboard();
                        myStoryboardIn.Children.Add(myDoubleAnimationIn);
                        Storyboard.SetTargetName(myDoubleAnimationIn, temp.Name);
                        Storyboard.SetTargetProperty(myDoubleAnimationIn, new PropertyPath(Path.OpacityProperty));

                        Storyboard myStoryboardOut = new Storyboard();
                        myStoryboardOut.Children.Add(myDoubleAnimationOut);
                        Storyboard.SetTargetName(myDoubleAnimationOut, temp.Name);
                        Storyboard.SetTargetProperty(myDoubleAnimationOut, new PropertyPath(Path.OpacityProperty));

                        temp.MouseEnter += new MouseEventHandler(mouseEnter);
                        temp.MouseLeave += new MouseEventHandler(mouseLeave);

                        myStoryboardInArray[1, i] = myStoryboardIn;
                        myStoryboardOutArray[1, i] = myStoryboardOut;

                        pathesARP[j - 1, k].Data = temp.Data;
                    }
                    i++;
                }
            }
        }

        /// <summary>
        /// Converts a given <see cref="byte[]"/>structure into an int value.
        /// </summary>
        /// <param name="value">The <see cref="byte[]"/> value to be converted.</param>
        /// <returns>The int value representing the byte[] values.</returns>
        private int getIntFromTwoBytes(byte[] value)
        {
            return (value[1] & 0xff) << 8 | (value[0] & 0xff);
        }

        /// <summary>
        /// Converts a given <see cref="byte[]"/>structure into an int value.
        /// </summary>
        /// <param name="value">The <see cref="byte[]"/> value to be converted.</param>
        /// <returns>The int value representing the byte[] values</returns>
        private int getIntFromFourBytes(byte[] value)
        {
            return value[3] << 24 | (value[2] & 0xff) << 16 | (value[1] & 0xff) << 8 | (value[0] & 0xff);
        }

        #endregion

        #region Navigation / main control

        /// <summary>
        /// Organizes the main structure of the presentation.
        /// The <see cref="Path"/> arrays are filled with the Pathes defined in the XAML file and
        /// the <see cref="GeometryRectangle"/>s are created.
        /// </summary>
        private void mainControl()
        {
            fillPathArray();
            createPacketCubes();
        }

        #endregion

        #region Public methods & constructor

        /// <summary>
        /// Simple constructor. Without further words...
        /// </summary>
        public Internet_frame_generator_Presentation()
        {
            try
            {
                InitializeComponent();
                mainControl();
            }
            catch (Exception exc)
            {
                textBlockARP.Text = exc.ToString();
                textBlockIP.Text = exc.ToString();
            }
        }

        #endregion

        #region Animation

        /// <summary>
        /// Makes some preparation of data for the animation going to be done.
        /// </summary>
        /// <param name="valueType">Indicates which data parts of the packet is concerned (MAC address e.g.).</param>
        /// <param name="packetType">Indicates what kind of packet is concerned ("IP" or "ARP").</param>
        private void prepareAnimation(int valueType, string packetType, string inOrOut)
        {
            if (packetType.Equals("IP"))
            {
                if (valueType < 6)
                {
                    organiseAnimation("DestinationAddressMAC_IP", inOrOut);
                }

                if ((valueType >= 6) && (valueType < 11))
                {
                    organiseAnimation("SourceAdressMAC_IP", inOrOut);
                }

                if ((valueType >= 11) && (valueType < 14))
                {
                    organiseAnimation("TypeIP", inOrOut);
                }

                if (valueType == 14)
                {
                    organiseAnimation("HeaderLength", inOrOut);
                }

                if (valueType == 15)
                {
                    organiseAnimation("DifferentiatedServicesField", inOrOut);
                }

                if ((valueType >= 16) && (valueType < 18))
                {
                    organiseAnimation("TotalLength", inOrOut);
                }

                if ((valueType >= 18) && (valueType < 20))
                {
                    organiseAnimation("Identification", inOrOut);
                }

                if ((valueType >= 20) && (valueType < 22))
                {
                    organiseAnimation("FragmentOffset", inOrOut);
                }

                if (valueType == 22)
                {
                    organiseAnimation("TimeToLive", inOrOut);
                }

                if (valueType == 23)
                {
                    organiseAnimation("TypeTCP", inOrOut);
                }

                if ((valueType >= 24) && (valueType < 26))
                {
                    organiseAnimation("TCPHeaderChecksum", inOrOut);
                }

                if ((valueType >= 26) && (valueType < 30))
                {
                    organiseAnimation("TCPSourceIP", inOrOut);
                }

                if ((valueType >= 30) && (valueType < 34))
                {
                    organiseAnimation("TCPDestinationIP", inOrOut);
                }

                if (valueType >= 34)
                {
                    organiseAnimation("TCPData", inOrOut);
                }
            }
            if (packetType.Equals("ARP"))
            {
                if (valueType < 6)
                {
                    organiseAnimation("DestinationAddressMAC_ARP", inOrOut);
                }

                if ((valueType >= 6) && (valueType < 12))
                {
                    organiseAnimation("SourceAdressMAC_ARP", inOrOut);
                }

                if ((valueType >= 12) && (valueType < 14))
                {
                    organiseAnimation("TypeARP", inOrOut);
                }

                if (valueType >= 14)
                {
                    organiseAnimation("ARPData", inOrOut);
                }
            }
        }

        /// <summary>
        /// Manages the animation.
        /// </summary>
        /// <param name="a">Indicates which rectangles have to be animated.</param>
        private void organiseAnimation(string a, string inOrOut)
        {
            if (a.Equals("DestinationAddressMAC_IP"))
            {
                if (inOrOut.Equals("in"))
                {
                    for (int i = 0; i < 6; i++) { myStoryboardInArray[0, i].Begin(this); }
                    textBlockIP.Text = "DestinationAddressMAC_IP";
                }
                if (inOrOut.Equals("out"))
                {
                    for (int i = 0; i < 6; i++) { myStoryboardOutArray[0, i].Begin(this); }
                    textBlockIP.Text = string.Empty;
                }
            }

            if (a.Equals("SourceAdressMAC_IP"))
            {
                if (inOrOut.Equals("in"))
                {
                    for (int i = 6; i < 12; i++) { myStoryboardInArray[0, i].Begin(this); }
                    textBlockIP.Text = "SourceAdressMAC_IP";
                }
                if (inOrOut.Equals("out"))
                {
                    for (int i = 6; i < 12; i++) { myStoryboardOutArray[0, i].Begin(this); }
                    textBlockIP.Text = string.Empty;
                }
            }

            if (a.Equals("TypeIP"))
            {
                if (inOrOut.Equals("in"))
                {
                    myStoryboardInArray[0, 12].Begin(this);
                    myStoryboardInArray[0, 13].Begin(this);
                    textBlockIP.Text = "TypeIP";
                }
                if (inOrOut.Equals("out"))
                {
                    myStoryboardOutArray[0, 12].Begin(this);
                    myStoryboardOutArray[0, 13].Begin(this);
                    textBlockIP.Text = string.Empty;
                }
            }

            if (a.Equals("HeaderLength"))
            {
                if (inOrOut.Equals("in"))
                {
                    myStoryboardInArray[0, 14].Begin(this);
                    textBlockIP.Text = "HeaderLength";
                }
                if (inOrOut.Equals("out"))
                {
                    myStoryboardOutArray[0, 14].Begin(this);
                    textBlockIP.Text = string.Empty;
                }
            }

            if (a.Equals("DifferentiatedServicesField"))
            {
                if (inOrOut.Equals("in"))
                {
                    myStoryboardInArray[0, 15].Begin(this);
                    textBlockIP.Text = "DifferentiatedServicesField";
                }
                if (inOrOut.Equals("out"))
                {
                    myStoryboardOutArray[0, 15].Begin(this);
                    textBlockIP.Text = string.Empty;
                }
            }

            if (a.Equals("TotalLength"))
            {
                if (inOrOut.Equals("in"))
                {
                    myStoryboardInArray[0, 16].Begin(this);
                    myStoryboardInArray[0, 17].Begin(this);
                    textBlockIP.Text = "TotalLength";
                }
                if (inOrOut.Equals("out"))
                {
                    myStoryboardOutArray[0, 16].Begin(this);
                    myStoryboardOutArray[0, 17].Begin(this);
                    textBlockIP.Text = string.Empty;
                }
            }

            if (a.Equals("Identification"))
            {
                if (inOrOut.Equals("in"))
                {
                    myStoryboardInArray[0, 18].Begin(this);
                    myStoryboardInArray[0, 19].Begin(this);
                    textBlockIP.Text = "Identification";
                }
                if (inOrOut.Equals("out"))
                {
                    myStoryboardOutArray[0, 18].Begin(this);
                    myStoryboardOutArray[0, 19].Begin(this);
                    textBlockIP.Text = string.Empty;
                }
            }

            if (a.Equals("FragmentOffset"))
            {
                if (inOrOut.Equals("in"))
                {
                    myStoryboardInArray[0, 20].Begin(this);
                    myStoryboardInArray[0, 21].Begin(this);
                    textBlockIP.Text = "FragmentOffset";
                }
                if (inOrOut.Equals("out"))
                {
                    myStoryboardOutArray[0, 20].Begin(this);
                    myStoryboardOutArray[0, 21].Begin(this);
                    textBlockIP.Text = string.Empty;
                }
            }

            if (a.Equals("TimeToLive"))
            {
                if (inOrOut.Equals("in"))
                {
                    myStoryboardInArray[0, 22].Begin(this);
                    textBlockIP.Text = "TimeToLive";
                }
                if (inOrOut.Equals("out"))
                {
                    myStoryboardOutArray[0, 22].Begin(this);
                    textBlockIP.Text = string.Empty;
                }
            }

            if (a.Equals("TypeTCP"))
            {
                if (inOrOut.Equals("in"))
                {
                    myStoryboardInArray[0, 23].Begin(this);
                    textBlockIP.Text = "TypeTCP";
                }
                if (inOrOut.Equals("out"))
                {
                    myStoryboardOutArray[0, 23].Begin(this);
                    textBlockIP.Text = string.Empty;
                }
            }

            if (a.Equals("TCPHeaderChecksum"))
            {
                if (inOrOut.Equals("in"))
                {
                    myStoryboardInArray[0, 24].Begin(this);
                    myStoryboardInArray[0, 25].Begin(this);
                    textBlockIP.Text = "TCPHeaderChecksum";
                }
                if (inOrOut.Equals("out"))
                {
                    myStoryboardOutArray[0, 24].Begin(this);
                    myStoryboardOutArray[0, 25].Begin(this);
                    textBlockIP.Text = string.Empty;
                }
            }

            if (a.Equals("TCPSourceIP"))
            {
                if (inOrOut.Equals("in"))
                {
                    for (int i = 26; i < 30; i++) { myStoryboardInArray[0, i].Begin(this); }
                    textBlockIP.Text = "TCPSourceIP";
                }
                if (inOrOut.Equals("out"))
                {
                    for (int i = 26; i < 30; i++) { myStoryboardOutArray[0, i].Begin(this); }
                    textBlockIP.Text = string.Empty;
                }
            }

            if (a.Equals("TCPDestinationIP"))
            {
                if (inOrOut.Equals("in"))
                {
                    for (int i = 30; i < 34; i++) { myStoryboardInArray[0, i].Begin(this); }
                    textBlockIP.Text = "TCPDestinationIP";
                }
                if (inOrOut.Equals("out"))
                {
                    for (int i = 30; i < 34; i++) { myStoryboardOutArray[0, i].Begin(this); }
                    textBlockIP.Text = string.Empty;
                }
            }

            if (a.Equals("TCPData"))
            {
                if (inOrOut.Equals("in"))
                {
                    for (int i = 34; i < 74; i++) { myStoryboardInArray[0, i].Begin(this); }
                    textBlockIP.Text = "TCPData";
                }
                if (inOrOut.Equals("out"))
                {
                    for (int i = 34; i < 74; i++) { myStoryboardOutArray[0, i].Begin(this); }
                    textBlockIP.Text = string.Empty;
                }
            }

            if (a.Equals("DestinationAddressMAC_ARP"))
            {
                if (inOrOut.Equals("in"))
                {
                    for (int i = 0; i < 6; i++) { myStoryboardInArray[1, i].Begin(this); }
                    textBlockARP.Text = "DestinationAddressMAC_ARP";
                }
                if (inOrOut.Equals("out"))
                {
                    for (int i = 0; i < 6; i++) { myStoryboardOutArray[1, i].Begin(this); }
                    textBlockARP.Text = string.Empty;
                }
            }

            if (a.Equals("SourceAdressMAC_ARP"))
            {
                if (inOrOut.Equals("in"))
                {
                    for (int i = 6; i < 12; i++) { myStoryboardInArray[1, i].Begin(this); }
                    textBlockARP.Text = "SourceAdressMAC_ARP";
                }
                if (inOrOut.Equals("out"))
                {
                    for (int i = 6; i < 12; i++) { myStoryboardOutArray[1, i].Begin(this); }
                    textBlockARP.Text = string.Empty;
                }
            }

            if (a.Equals("TypeARP"))
            {
                if (inOrOut.Equals("in"))
                {
                    myStoryboardInArray[1, 12].Begin(this);
                    myStoryboardInArray[1, 13].Begin(this);
                    textBlockARP.Text = "TypeARP";
                }
                if (inOrOut.Equals("out"))
                {
                    myStoryboardOutArray[1, 12].Begin(this);
                    myStoryboardOutArray[1, 13].Begin(this);
                    textBlockARP.Text = string.Empty;
                }
            }

            if (a.Equals("ARPData"))
            {
                if (inOrOut.Equals("in"))
                {
                    for (int i = 14; i < 42; i++) { myStoryboardInArray[1, i].Begin(this); }
                    textBlockARP.Text = "ARPData";
                }
                if (inOrOut.Equals("out"))
                {
                    for (int i = 14; i < 42; i++) { myStoryboardOutArray[1, i].Begin(this); }
                    textBlockARP.Text = string.Empty;
                }
            }
        }

        #endregion

        #region EventHandler

        /// <summary>
        /// EventHandler for "mouse in rectangle event".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mouseEnter(object sender, MouseEventArgs e)
        {
            int i = 0;
            for (int j = 0; j < 12; j++)
            {
                for (int k = 0; k < 8; k++)
                {
                    if ((i < 74) && (sender as Path).Name == pathesIP[j, k].Name)
                    {
                        prepareAnimation(i, "IP", "in");
                    }
                    i++;
                }
            }

            i = 0;

            for (int j = 0; j < 12; j++)
            {
                for (int k = 0; k < 8; k++)
                {
                    if ((i < 74) && (sender as Path).Name == pathesARP[j, k].Name)
                    {
                        prepareAnimation(i, "ARP", "in");
                    }
                    i++;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mouseLeave(object sender, MouseEventArgs e)
        {
            int i = 0;
            for (int j = 0; j < 12; j++)
            {
                for (int k = 0; k < 8; k++)
                {
                    if ((i < 74) && (sender as Path).Name == pathesIP[j, k].Name)
                    {
                        prepareAnimation(i, "IP", "out");
                    }
                    i++;
                }
            }

            i = 0;

            for (int j = 0; j < 12; j++)
            {
                for (int k = 0; k < 8; k++)
                {
                    if ((i < 74) && (sender as Path).Name == pathesARP[j, k].Name)
                    {
                        prepareAnimation(i, "ARP", "out");
                    }
                    i++;
                }
            }
        }

        #endregion
    }
}
