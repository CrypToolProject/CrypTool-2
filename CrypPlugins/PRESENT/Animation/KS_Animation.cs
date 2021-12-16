/*
   Copyright 2008 Timm Korte, University of Siegen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using CrypTool.PluginBase;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace CrypTool.PRESENT
{
    internal class KS_Animation
    {
        private readonly PRESENTAnimation parent;
        private readonly Model3DGroup mdl3D_main;

        public KS_Animation(PRESENTAnimation Parent, Model3DGroup Main3D)
        {
            parent = Parent;
            mdl3D_main = Main3D;
        }

        private int c_round, c_step, c_group;
        private readonly int[] c_sel = new int[4];

        private Model3DGroup mdl3D_sbox = new Model3DGroup();
        private Model3DGroup mdl3D_keyreg = new Model3DGroup();
        private Model3DGroup mdl3D_keyreglabel = new Model3DGroup();
        private Model3DGroup mdl3D_roundkeys = new Model3DGroup();
        private Model3DGroup mdl3D_roundcounter = new Model3DGroup();
        private readonly Model3DGroup[] mdl3D_roundkey = new Model3DGroup[32];
        private readonly Model3DCollection[] mdc3D_roundkeys = new Model3DCollection[32];

        private readonly ElementBuilder eBuilder = new ElementBuilder(null);
        private readonly ImageBrush[,] brush = new ImageBrush[2, 4];
        private readonly Material[,] material = new Material[2, 4];

        private readonly GeometryModel3D[] gma3D_keyreg = new GeometryModel3D[80];
        private readonly GeometryModel3D[] gma3D_roundcounter = new GeometryModel3D[5];
        private readonly GeometryModel3D[] gma3D_roundcountertemp = new GeometryModel3D[5];
        private readonly GeometryModel3D[,] gma3D_roundkeys = new GeometryModel3D[32, 64];
        private readonly GeometryModel3D[,] gma3D_sbox = new GeometryModel3D[16, 8];

        private readonly double opacity_default = 0.7;
        private readonly double opacity_high = 1;
        private readonly double opacity_low = 0.3;

        private string resumeName = "";
        private bool pause = true;
        private readonly string[] steps = new string[4] { "Key_Shift", "S_Box", "Roundcounter_XOR", "Key_Extraction" };

        private readonly SolidColorBrush tb = new SolidColorBrush();
        private DoubleAnimation ta;

        private void CreateObjects()
        {
            parent.KS_Cam.BeginAnimation(PerspectiveCamera.FieldOfViewProperty, null);
            parent.KS_Cam.BeginAnimation(PerspectiveCamera.LookDirectionProperty, null);
            parent.KS_Cam.BeginAnimation(PerspectiveCamera.PositionProperty, null);
            mdl3D_main.Children.Clear();

            for (int i = 0; i < 4; i++)
            {
                brush[0, i] = new ImageBrush() { ImageSource = BitmapFrame.Create(new Uri(@"pack://application:,,,/PRESENT;component/Animation/resources/img0.png")), Opacity = opacity_default };
                brush[1, i] = new ImageBrush() { ImageSource = BitmapFrame.Create(new Uri(@"pack://application:,,,/PRESENT;component/Animation/resources/img1.png")), Opacity = opacity_default };
                material[0, i] = new DiffuseMaterial(brush[0, i]);
                material[1, i] = new DiffuseMaterial(brush[1, i]);
            }

            mdl3D_sbox = new Model3DGroup
            {
                Children = Create_Sbox()
            };
            mdl3D_main.Children.Add(mdl3D_sbox);

            mdl3D_roundkeys = new Model3DGroup
            {
                Children = Create_RoundKeyCubes()
            };
            mdl3D_main.Children.Add(mdl3D_roundkeys);

            mdl3D_roundcounter = new Model3DGroup
            {
                Children = Create_RoundCounter()
            };
            mdl3D_main.Children.Add(mdl3D_roundcounter);

            mdl3D_keyreg = new Model3DGroup
            {
                Children = Create_Ring()
            };
            mdl3D_main.Children.Add(mdl3D_keyreg);

            mdl3D_keyreglabel = new Model3DGroup
            {
                Children = Create_Label()
            };
            mdl3D_main.Children.Add(mdl3D_keyreglabel);
        }

        private void PositionInit()
        {
            c_group = 0;
            CreateObjects();
            Color_Sbox(0);
            Color_Ring(c_round, c_step, 0);
            Color_RoundCounter(0);
            for (int r = 0; r < c_round; r++)
            {
                Color_RoundKeyCube(r, 0);
                Point3D pos = GetRoundKeyPosition(r);
                mdl3D_roundkey[r].Transform = new TranslateTransform3D(pos.X, pos.Y, pos.Z);
            }
            parent.lbl_KS_Round.Content = string.Format("{0:d2}", c_round);
            parent.lbl_KS_Step.Content = typeof(PRESENT).GetPluginStringResource(steps[c_step]);
            resumeName = string.Format("Start_{0:d1}", c_step);
        }

        public void InitStart()
        {
            c_round = 0; c_group = 0; c_step = 3;
            resumeName = "Start_3";
            PositionInit();
            parent.lbl_KS_Text.Content = string.Format(Properties.Resources.Key_Schedule_, parent.cipher.KeyRegs[0, 1, 2], parent.cipher.KeyRegs[0, 0, 2]);
            parent.lbl_KS_Step.Content = typeof(PRESENT).GetPluginStringResource(steps[c_step]);
            pause = true;
        }

        public void Control(int action)
        {
            tb.BeginAnimation(Brush.OpacityProperty, null);

            switch (action)
            {
                case -2:
                    if (c_round > 0)
                    {
                        c_round--;
                        c_step = 0;
                        PositionInit();
                        AnimationFSM(this, null);
                    }
                    break;
                case -1:
                    if (c_step <= 0)
                    {
                        if (c_round > 0) { c_round--; c_step = 3; }
                    }
                    else
                    {
                        c_step--;
                    }
                    PositionInit();
                    AnimationFSM(this, null);
                    break;
                case 0:
                    pause = !pause;
                    AnimationFSM(this, null);
                    break;
                case 1:
                    if (c_step >= 3)
                    {
                        if (c_round < 31) { c_round++; c_step = 0; PositionInit(); }
                    }
                    else
                    {
                        c_step++;
                        PositionInit();
                    }
                    AnimationFSM(this, null);
                    break;
                case 2:
                    if (c_round < 31)
                    {
                        c_round++;
                        c_step = 0;
                        PositionInit();
                        AnimationFSM(this, null);
                    }
                    else
                    {
                        parent.tabControl.SelectedIndex = 4; //Encryption
                    }
                    AnimationFSM(this, null);
                    break;
            }
        }


        #region Main Handler

        private void AnimationFSM(object sender, EventArgs e)
        {
            string name = "";
            if (typeof(AnimationClock).Equals(sender.GetType()))
            {
                name = (sender as AnimationClock).Timeline.Name;
                resumeName = name;
            }
            else
            {
                name = resumeName;
            }
            if (!pause)
            {
                switch (name)
                {
                    case "Start_0":
                        KeyShift();
                        break;
                    case "Start_1":
                        KeySbox();
                        break;
                    case "Start_2":
                        KeyCounterAdd();
                        break;
                    case "Start_3":
                        KeyGetRoundKeyPrepare();
                        break;

                    case "KeyGetRoundKeyPrepare":
                        c_group = 0;
                        KeyGetRoundKeyGroup();
                        break;
                    case "KeyGetRoundKeyGroup":
                        KeyGetRoundKeyGroupCompleted();
                        break;
                    case "KeyGetRoundKeyGroupCompleted":
                        RoundKeyMove(c_round);
                        c_round++;
                        if (c_round < 32)
                        {
                            parent.lbl_KS_Round.Content = string.Format("{0:d2}", c_round);
                            Color_RoundCounter(0);
                            KeyShift();
                        }
                        else
                        {
                            c_round = 31;
                            resumeName = "Done";
                            Zoom();
                        }
                        break;
                    case "KeyShift":
                        KeyShiftCompleted();
                        break;
                    case "KeySboxExtract":
                        KeySboxExtractCompleted();
                        break;
                    case "KeySboxIntegrate":
                        KeySboxIntegrateCompleted();
                        break;
                    case "KeyCounterAdd":
                        KeyCounterAddCompleted();
                        break;
                    case "Done":
                        break;
                    case "Nothing":
                        break;
                    default:
                        throw new Exception(string.Concat("Unhandled Function Name: '", name, "'"));
                }
            }
        }
        #endregion

        private void countdown(double seconds, string name)
        {
            tb.BeginAnimation(Brush.OpacityProperty, null);
            ta = new DoubleAnimation(0, 0, TimeSpan.FromSeconds(seconds)) { SpeedRatio = parent.sld_KS_Speed.Value };
            ta.Name = name;
            ta.Completed += new EventHandler(AnimationFSM);
            tb.BeginAnimation(Brush.OpacityProperty, ta);
        }

        #region Create and Color

        private Model3DCollection Create_Sbox()
        {
            Model3DCollection col = new Model3DCollection();
            double offsetX = 7, offsetY = 3, offsetZ = -7, scale = 1.9;
            for (int r = 0; r < 16; r++)
            {
                for (int a = 0; a < 4; a++)
                {
                    gma3D_sbox[r, a] = eBuilder.CreateCube(0.5, new Point3D(offsetX + a / scale, offsetY - r / scale, offsetZ));
                    col.Add(gma3D_sbox[r, a]);
                }

                for (int b = 0; b < 4; b++)
                {
                    gma3D_sbox[r, b + 4] = eBuilder.CreateCube(0.5, new Point3D(offsetX + (b + 5) / scale, offsetY - r / scale, offsetZ));
                    col.Add(gma3D_sbox[r, b + 4]);
                }
                GeometryModel3D ar = eBuilder.CreateArrowRight(0.3, 0.2, new Point3D(offsetX + 4.35 / scale, offsetY - r / scale, offsetZ), Colors.Blue, 0.6);
                col.Add(ar);
            }
            GeometryModel3D lblsbox = eBuilder.CreateLabel(4, 2, new Point3D(offsetX, offsetY + 2, offsetZ), Colors.Transparent, "S-Box", Colors.Black, 1);
            col.Add(lblsbox);
            return col;
        }

        private void Color_Sbox(int index)
        {
            int x;
            for (int r = 0; r < 16; r++)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (i < 4)
                    {
                        x = (r >> (3 - i)) & 1;
                    }
                    else
                    {
                        x = (parent.cipher.Sbox4[r] >> (7 - i)) & 1;
                    }

                    gma3D_sbox[r, i].Material = material[x, index];
                }
            }
        }

        private Model3DCollection Create_RoundCounter()
        {
            Model3DCollection col = new Model3DCollection();
            Point3D pos;
            for (int i = 0; i < 5; i++)
            {
                pos = GetRingPos(i + 15);
                gma3D_roundcountertemp[i] = eBuilder.CreateCube(0.3, pos, null);
                col.Add(gma3D_roundcountertemp[i]);
                pos.Y += 1.2;
                gma3D_roundcounter[i] = eBuilder.CreateCube(0.3, pos);
                col.Add(gma3D_roundcounter[i]);
            }
            return col;
        }

        private void Color_RoundCounter(int index)
        {
            int x;
            for (int i = 0; i < 5; i++)
            {
                x = (c_round >> i) & 1;
                gma3D_roundcounter[i].Material = material[x, index];
            }
        }

        private Model3DCollection Create_Ring()
        {
            Model3DCollection col = new Model3DCollection();
            double size = 0.3;
            for (int i = 0; i < 80; i++)
            {
                gma3D_keyreg[i] = eBuilder.CreateCube(size, GetRingPos(i));
                col.Add(gma3D_keyreg[i]);
            }
            return col;
        }

        private void Color_Ring(int round, int step, int index)
        {
            int x;
            for (int i = 0; i < 16; i++)
            {
                x = (int)(parent.cipher.KeyRegs[round, 0, step] >> i) & 1;
                gma3D_keyreg[i].Material = material[x, index];
            }
            for (int i = 0; i < 64; i++)
            {
                x = (int)(parent.cipher.KeyRegs[round, 1, step] >> i) & 1;
                gma3D_keyreg[i + 16].Material = material[x, index];
            }
        }
        private Model3DCollection Create_RoundKeyCubes()
        {
            Model3DCollection col = new Model3DCollection();
            for (int r = 31; r >= 0; r--)
            {
                mdc3D_roundkeys[r] = new Model3DCollection();
                mdl3D_roundkey[r] = new Model3DGroup();
                for (int i = 63; i >= 0; i--)
                {
                    gma3D_roundkeys[r, i] = eBuilder.CreateCube(0.8, GetCubePos(i), null);
                    mdc3D_roundkeys[r].Add(gma3D_roundkeys[r, i]);
                }
                mdl3D_roundkey[r].Children = mdc3D_roundkeys[r];
                col.Add(mdl3D_roundkey[r]);
            }
            return col;
        }

        private void Color_RoundKeyCube(int round, int index)
        {
            int x;
            for (int i = 0; i < 64; i++)
            {
                x = (int)(parent.cipher.RoundKeys[round] >> i) & 1;
                gma3D_roundkeys[round, i].Material = material[x, index];
            }
        }

        private Model3DCollection Create_Label()
        {
            Point3D pos;
            Material mat;
            GeometryModel3D num;
            Model3DCollection col = new Model3DCollection();
            for (int i = 0; i < 80; i++)
            {
                pos = GetRingPos(i);
                pos.Y -= 0.3;
                mat = new DiffuseMaterial(new VisualBrush(new TextBlock() { Text = i.ToString(), Background = new SolidColorBrush(Colors.Transparent), Foreground = new SolidColorBrush(Colors.Black), Padding = new Thickness(1) }) { Stretch = Stretch.Uniform });
                num = eBuilder.CreateCubeLabel(0.3, pos, mat);
                double a = Math.Atan2((parent.KS_Cam.Position.X - pos.X), (parent.KS_Cam.Position.Z - pos.Z)) / Math.PI * 180;
                RotateTransform3D rt = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), a), pos);
                num.Transform = rt;
                col.Add(num);
            }
            return col;
        }

        private Point3D GetRingPos(int x)
        {
            double r = 6, h = -1.5;
            double offsetX, offsetY, offsetZ;
            offsetX = Math.Cos(2 * Math.PI * (x / 80.0)) * r;
            offsetZ = Math.Sin(2 * Math.PI * (x / 80.0)) * r;
            offsetY = Math.Sin(2 * Math.PI * (x / 80.0)) * h;
            return new Point3D(offsetX, offsetY, offsetZ);
        }

        private Point3D GetCubePos(int x)
        {
            double offsetX, offsetY, offsetZ;
            offsetX = 1.5 - (x % 4);
            offsetY = 1.5 - (x % 16) / 4;
            offsetZ = 1.5 - (x / 16);
            return new Point3D(offsetX, offsetY, offsetZ);
        }

        #endregion

        #region Key Schedule Animations
        #region Key Shift

        private void KeyShift()
        {
            c_step = 0;
            parent.lbl_KS_Text.Content = string.Format(Properties.Resources.rotate_key_register_61, parent.cipher.KeyRegs[c_round, 1, 0], parent.cipher.KeyRegs[c_round, 0, 0], parent.cipher.KeyRegs[c_round, 1, 1], parent.cipher.KeyRegs[c_round, 0, 1]);
            parent.lbl_KS_Step.Content = typeof(PRESENT).GetPluginStringResource(steps[c_step]);

            double a, b, r = 6, h = -1.5;
            RoundCosDoubleAnimation rax = null;
            RoundSinDoubleAnimation ray = null, raz = null;
            TranslateTransform3D tt;
            for (int i = 0; i < 80; i++)
            {
                a = (2 * Math.PI * (i / 80.0));
                b = (2 * Math.PI * ((i - 19) % 80) / 80.0);
                if (a > b)
                {
                    b += 2 * Math.PI;
                }

                rax = new RoundCosDoubleAnimation() { From = a, To = b, Scale = r, Duration = TimeSpan.FromSeconds(4), SpeedRatio = parent.sld_KS_Speed.Value };
                raz = new RoundSinDoubleAnimation() { From = a, To = b, Scale = r, Duration = TimeSpan.FromSeconds(4), SpeedRatio = parent.sld_KS_Speed.Value };
                ray = new RoundSinDoubleAnimation() { From = a, To = b, Scale = h, Duration = TimeSpan.FromSeconds(4), SpeedRatio = parent.sld_KS_Speed.Value };
                tt = new TranslateTransform3D();
                gma3D_keyreg[i].Transform = tt;
                tt.BeginAnimation(TranslateTransform3D.OffsetXProperty, rax);
                tt.BeginAnimation(TranslateTransform3D.OffsetYProperty, ray);
                tt.BeginAnimation(TranslateTransform3D.OffsetZProperty, raz);
            }
            countdown(4, MethodBase.GetCurrentMethod().Name);
        }

        private void KeyShiftCompleted()
        {
            for (int i = 0; i < 80; i++)
            {
                gma3D_keyreg[i].Transform = null;
            }

            Color_Ring(c_round, 1, 0);
            KeySbox();
        }

        #endregion

        #region Key Sbox

        private void KeySbox()
        {
            c_step = 1;
            int x = (int)(parent.cipher.KeyRegs[c_round, 1, 1] >> 60) & 0x0f;
            int r = (int)(parent.cipher.KeyRegs[c_round, 1, 2] >> 60) & 0x0f;
            parent.lbl_KS_Text.Content = string.Format(Properties.Resources.passing_leftmost_4_bits, Int2BinaryString(x, 4), Int2BinaryString(r, 4));
            parent.lbl_KS_Step.Content = typeof(PRESENT).GetPluginStringResource(steps[c_step]);
            KeySboxExtract();
        }

        private void KeySboxExtract()
        {
            int x, r;

            TranslateTransform3D tt = new TranslateTransform3D();
            r = (int)(parent.cipher.KeyRegs[c_round, 1, 1] >> 60) & 0x0f;

            for (int i = 0; i < 8; i++)
            {
                if (i < 4)
                {
                    x = (r >> (3 - i)) & 1;
                }
                else
                {
                    x = (parent.cipher.Sbox4[r] >> (7 - i)) & 1;
                }

                gma3D_sbox[r, i].Material = material[x, 3];
            }
            for (int i = 76; i < 80; i++)
            {
                x = (r >> (i - 76)) & 1;
                gma3D_keyreg[i].Material = material[x, 3];
                gma3D_keyreg[i].Transform = tt;
            }
            CosDoubleAnimation dax = new CosDoubleAnimation() { From = Math.PI * 3 / 2, To = Math.PI * 2, Scale = 0.6, Duration = TimeSpan.FromSeconds(1), SpeedRatio = parent.sld_KS_Speed.Value };
            DoubleAnimation da = new DoubleAnimation(opacity_default, opacity_high, TimeSpan.FromSeconds(1)) { SpeedRatio = parent.sld_KS_Speed.Value };

            brush[0, 3].BeginAnimation(Brush.OpacityProperty, da);
            brush[1, 3].BeginAnimation(Brush.OpacityProperty, da);
            tt.BeginAnimation(TranslateTransform3D.OffsetXProperty, dax);

            countdown(1, MethodBase.GetCurrentMethod().Name);
        }

        private void KeySboxExtractCompleted()
        {
            int x;
            for (int i = 76; i < 80; i++)
            {
                x = (int)(parent.cipher.KeyRegs[c_round, 1, 2] >> (i - 16)) & 1;
                gma3D_keyreg[i].Material = material[x, 3];
            }
            KeySboxIntegrate();
        }

        private void KeySboxIntegrate()
        {
            TranslateTransform3D tt = new TranslateTransform3D();
            for (int i = 76; i < 80; i++)
            {
                gma3D_keyreg[i].Transform = tt;
            }

            CosDoubleAnimation dax = new CosDoubleAnimation() { From = Math.PI * 2, To = Math.PI * 3 / 2, Scale = 0.6, Duration = TimeSpan.FromSeconds(1), SpeedRatio = parent.sld_KS_Speed.Value };
            DoubleAnimation da = new DoubleAnimation(opacity_high, opacity_default, TimeSpan.FromSeconds(1)) { SpeedRatio = parent.sld_KS_Speed.Value };
            brush[0, 3].BeginAnimation(Brush.OpacityProperty, da);
            brush[1, 3].BeginAnimation(Brush.OpacityProperty, da);
            tt.BeginAnimation(TranslateTransform3D.OffsetXProperty, dax);

            countdown(1, MethodBase.GetCurrentMethod().Name);
        }

        private void KeySboxIntegrateCompleted()
        {
            Color_Sbox(0);
            Color_Ring(c_round, 2, 0);
            KeyCounterAdd();
        }

        #endregion

        #region Add RoundCounter
        private void KeyCounterAdd()
        {
            c_step = 2;
            parent.lbl_KS_Step.Content = typeof(PRESENT).GetPluginStringResource(steps[c_step]);
            int a, b, r, x;
            a = (int)(parent.cipher.KeyRegs[c_round, 1, 3] & 0xf) << 1;
            b = (int)(parent.cipher.KeyRegs[c_round, 0, 3] >> 15) & 1;
            r = a ^ b;
            parent.lbl_KS_Text.Content = string.Format(Properties.Resources.xor_round_counter, Int2BinaryString(a, 5), Int2BinaryString(b, 5), Int2BinaryString(r, 5));
            TranslateTransform3D tt = new TranslateTransform3D();
            for (int i = 0; i < 5; i++)
            {
                x = (r >> i) & 1;
                gma3D_roundcountertemp[i].Material = gma3D_keyreg[i + 15].Material;
                gma3D_roundcountertemp[i].Transform = tt;
                gma3D_keyreg[i + 15].Material = material[x, 3];
            }
            CosDoubleAnimation day = new CosDoubleAnimation() { From = Math.PI * 3 / 2, To = Math.PI * 2, Scale = 0.6, Duration = TimeSpan.FromSeconds(2), SpeedRatio = parent.sld_KS_Speed.Value };
            DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames() { Duration = TimeSpan.FromSeconds(4), SpeedRatio = parent.sld_KS_Speed.Value };
            da.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(0)));
            da.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(0.5)));
            da.KeyFrames.Add(new LinearDoubleKeyFrame(opacity_default, KeyTime.FromPercent(1)));
            DoubleAnimationUsingKeyFrames db = da.Clone();
            brush[0, 3].BeginAnimation(Brush.OpacityProperty, da);
            brush[1, 3].BeginAnimation(Brush.OpacityProperty, db);
            tt.BeginAnimation(TranslateTransform3D.OffsetYProperty, day);

            countdown(4, MethodBase.GetCurrentMethod().Name);
        }

        private void KeyCounterAddCompleted()
        {
            Color_Ring(c_round, 3, 0);
            for (int i = 0; i < 5; i++)
            {
                gma3D_roundcountertemp[i].Transform = null;
                gma3D_roundcountertemp[i].Material = null;
            }
            KeyGetRoundKeyPrepare();
        }
        #endregion

        #region Get Current Roundkey

        private void KeyGetRoundKeyPrepare()
        {
            c_step = 3;
            parent.lbl_KS_Text.Content = string.Format(Properties.Resources.picking_the_64_left_most_bits, c_round, parent.cipher.RoundKeys[c_round]);
            parent.lbl_KS_Step.Content = typeof(PRESENT).GetPluginStringResource(steps[3]);
            Color_Ring(c_round, 3, 0); //Set Ring to Color Index 0
            DoubleAnimation oa = new DoubleAnimation() { Duration = new Duration(TimeSpan.FromSeconds(0.5)), To = opacity_low, SpeedRatio = parent.sld_KS_Speed.Value };
            DoubleAnimation ob = new DoubleAnimation() { Duration = new Duration(TimeSpan.FromSeconds(0.5)), To = opacity_low, SpeedRatio = parent.sld_KS_Speed.Value };
            brush[0, 0].BeginAnimation(Brush.OpacityProperty, oa);
            brush[1, 0].BeginAnimation(Brush.OpacityProperty, ob);
            brush[0, 2].Opacity = opacity_default;
            brush[1, 2].Opacity = opacity_default;
            countdown(0.5, MethodBase.GetCurrentMethod().Name);
        }

        private void KeyGetRoundKeyGroup()
        {
            int x;
            for (int i = 0; i < 64; i++)
            {
                x = (int)(parent.cipher.KeyRegs[c_round, 1, 3] >> i) & 1;
                if (i >= (c_group * 4) && i < ((c_group + 1) * 4))
                {
                    gma3D_roundkeys[c_round, i].Material = material[x, 1];
                    gma3D_keyreg[i + 16].Material = material[x, 1];
                }
                else
                {
                    if (i < (c_group * 4))
                    {
                        gma3D_roundkeys[c_round, i].Material = material[x, 0];
                        gma3D_keyreg[i + 16].Material = material[x, 2];
                    }
                }
            }
            DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames() { Duration = TimeSpan.FromSeconds(1), SpeedRatio = parent.sld_KS_Speed.Value };
            da.KeyFrames.Add(new LinearDoubleKeyFrame(opacity_low, KeyTime.FromPercent(0)));
            da.KeyFrames.Add(new LinearDoubleKeyFrame(opacity_high, KeyTime.FromPercent(0.25)));
            da.KeyFrames.Add(new LinearDoubleKeyFrame(opacity_high, KeyTime.FromPercent(0.75)));
            da.KeyFrames.Add(new LinearDoubleKeyFrame(opacity_low, KeyTime.FromPercent(1)));
            DoubleAnimationUsingKeyFrames db = da.Clone();
            brush[0, 1].BeginAnimation(Brush.OpacityProperty, da);
            brush[1, 1].BeginAnimation(Brush.OpacityProperty, db);

            countdown(1, MethodBase.GetCurrentMethod().Name);
        }

        private void KeyGetRoundKeyGroupCompleted()
        {
            c_group++;
            if (c_group < 16)
            {
                KeyGetRoundKeyGroup();
            }
            else
            {
                Color_RoundKeyCube(c_round, 0);
                Color_Ring(c_round, 3, 0);
                DoubleAnimation da = new DoubleAnimation(opacity_default, TimeSpan.FromSeconds(0.5)) { SpeedRatio = parent.sld_KS_Speed.Value };
                DoubleAnimation db = da.Clone();
                brush[0, 0].BeginAnimation(Brush.OpacityProperty, da);
                brush[1, 0].BeginAnimation(Brush.OpacityProperty, db);
                countdown(0.5, MethodBase.GetCurrentMethod().Name);
            }
        }

        #endregion

        #region Move RoundKey

        private void RoundKeyMove(int r)
        {
            //this.txtBoxStep.Text = "";
            Point3D pos = GetRoundKeyPosition(r);
            TranslateTransform3D tt = new TranslateTransform3D();
            DoubleAnimation dx = new DoubleAnimation(0, pos.X, TimeSpan.FromSeconds(2)) { SpeedRatio = parent.sld_KS_Speed.Value };
            DoubleAnimation dy = new DoubleAnimation(0, pos.Y, TimeSpan.FromSeconds(2)) { SpeedRatio = parent.sld_KS_Speed.Value };
            DoubleAnimation dz = new DoubleAnimation(0, pos.Z, TimeSpan.FromSeconds(2)) { SpeedRatio = parent.sld_KS_Speed.Value };
            mdl3D_roundkey[r].Transform = tt;
            tt.BeginAnimation(TranslateTransform3D.OffsetXProperty, dx);
            tt.BeginAnimation(TranslateTransform3D.OffsetYProperty, dy);
            tt.BeginAnimation(TranslateTransform3D.OffsetZProperty, dz);
        }

        #endregion

        public void Zoom()
        {
            Vector3D d = new Vector3D(0, 0, -1);
            Vector3DAnimation va = new Vector3DAnimation(d, TimeSpan.FromSeconds(3));
            Point3D p = GetRoundKeyPosition(0);
            p.Z += 30;
            p.X -= 7;
            p.Y -= 7;
            Point3DAnimation pa = new Point3DAnimation(p, TimeSpan.FromSeconds(3));
            DoubleAnimation da = new DoubleAnimation(opacity_high, TimeSpan.FromSeconds(2));
            parent.KS_Cam.BeginAnimation(PerspectiveCamera.PositionProperty, pa);
            parent.KS_Cam.BeginAnimation(PerspectiveCamera.LookDirectionProperty, va);
            brush[0, 0].BeginAnimation(Brush.OpacityProperty, da);
            brush[1, 0].BeginAnimation(Brush.OpacityProperty, da);
            parent.lbl_KS_Text.Content = Properties.Resources.Finished_generating_32_roundkeys;
            parent.lbl_KS_Step.Content = Properties.Resources.Finished;
        }

        private Point3D GetRoundKeyPosition(int r)
        {
            double size = 4.5;
            double offsetX, offsetY, offsetZ;
            offsetX = -25 - (r % 4) * size;
            offsetY = -.5 - ((r % 16) / 4) * size;
            offsetZ = -50 - (r / 16) * size;
            return new Point3D(offsetX, offsetY, offsetZ);
        }

        #endregion

        private string Int2BinaryString(int x, int numbits)
        {
            string binstr = "";
            for (int i = numbits - 1; i >= 0; i--)
            {
                if (((x >> i) & 1) == 1)
                {
                    binstr += "1";
                }
                else
                {
                    binstr += "0";
                }
            }
            return binstr;
        }
    }
}
