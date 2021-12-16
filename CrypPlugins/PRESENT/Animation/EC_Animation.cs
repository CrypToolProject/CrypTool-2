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
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace CrypTool.PRESENT
{
    internal class EC_Animation
    {
        private readonly PRESENTAnimation parent;
        private readonly Model3DGroup mdl3D_main;

        public EC_Animation(PRESENTAnimation Parent, Model3DGroup Main3D)
        {
            parent = Parent;
            mdl3D_main = Main3D;
        }

        private int c_round, c_group, c_step;
        private readonly int[] c_sel = new int[4];

        private Model3DGroup mdl3D_sbox = new Model3DGroup();
        private Model3DGroup mdl3D_state = new Model3DGroup();
        private Model3DGroup mdl3D_roundkeys = new Model3DGroup();
        private readonly Model3DGroup[] mdl3D_roundkey = new Model3DGroup[32];
        private readonly Model3DCollection[] mdc3D_roundkeys = new Model3DCollection[32];

        private readonly ElementBuilder eBuilder = new ElementBuilder(null);

        private readonly ImageBrush[,] brush = new ImageBrush[2, 4];
        private readonly Material[,] material = new Material[2, 4];

        private readonly GeometryModel3D[] gma3D_state = new GeometryModel3D[64];
        private readonly GeometryModel3D[] gma3D_statetemp = new GeometryModel3D[64];
        private readonly GeometryModel3D[,] gma3D_roundkeys = new GeometryModel3D[32, 64];
        private readonly GeometryModel3D[,] gma3D_sbox = new GeometryModel3D[16, 8];

        private readonly double opacity_default = 0.7;
        private readonly double opacity_high = 1;
        private readonly double opacity_low = 0.3;

        private string resumeName = "";
        private bool pause = true;

        private readonly string[] steps = new string[3] { "Add_Roundkey", "S_Box", "Permutation" };

        private readonly SolidColorBrush tb = new SolidColorBrush();
        private DoubleAnimation ta;

        private void CreateObjects()
        {
            parent.EC_Cam.BeginAnimation(PerspectiveCamera.FieldOfViewProperty, null);
            parent.EC_Cam.BeginAnimation(PerspectiveCamera.LookDirectionProperty, null);
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

            mdl3D_state = new Model3DGroup
            {
                Children = Create_State()
            };
            mdl3D_main.Children.Add(mdl3D_state);
        }

        private void PositionInit()
        {
            c_group = 0;
            CreateObjects();
            Color_Sbox(0);
            Color_State(c_round, c_step, 0);
            int d = (c_step == 0) ? c_round - 1 : c_round;

            for (int r = 31; r > d; r--)
            {
                Color_RoundKeyCube(r, 0);
                Point3D pos = GetRoundKeyPosition(r);
                mdl3D_roundkey[r].Transform = new TranslateTransform3D(pos.X, pos.Y, pos.Z);
            }
            parent.lbl_EC_Round.Content = string.Format("{0:d2}", c_round);
            parent.lbl_EC_Step.Content = typeof(PRESENT).GetPluginStringResource(steps[c_step]);
            resumeName = string.Format("Start_{0:d1}", c_step);
        }

        public void InitStart()
        {
            c_round = 0; c_group = 0; c_step = 0;
            resumeName = "Start_0";
            PositionInit();
            parent.lbl_EC_Text.Content = string.Format(Properties.Resources.Encryption_Data, parent.cipher.States[0, 0]);
            parent.lbl_EC_Step.Content = typeof(PRESENT).GetPluginStringResource(steps[c_step]);
            pause = true;
        }

        public void Zoom()
        {
            DoubleAnimation da = new DoubleAnimation(20, TimeSpan.FromSeconds(2));
            Vector3D d = new Point3D(0, 0, 0) - parent.EC_Cam.Position;
            Vector3DAnimation va = new Vector3DAnimation(d, TimeSpan.FromSeconds(2));
            parent.EC_Cam.BeginAnimation(PerspectiveCamera.LookDirectionProperty, va);
            parent.EC_Cam.BeginAnimation(PerspectiveCamera.FieldOfViewProperty, da);
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
                        if (c_round > 0) { c_round--; c_step = 2; }
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
                    if (c_step >= 2)
                    {
                        if (c_round < 31) { c_round++; c_step = 0; }
                    }
                    else
                    {
                        if (c_round < 31)
                        {
                            c_step++;
                        }
                        else
                        {
                            c_step = 0;
                        }
                    }
                    PositionInit();
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
                        parent.tabControl.SelectedIndex = 5; //Trace
                    }
                    AnimationFSM(this, null);
                    break;
            }
        }

        private void countdown(double seconds, string name, double speedratio)
        {
            tb.BeginAnimation(Brush.OpacityProperty, null);
            ta = new DoubleAnimation(0, 0, TimeSpan.FromSeconds(seconds)) { SpeedRatio = speedratio };
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

        private Point3D GetCubePos(int x)
        {
            double offsetX, offsetY, offsetZ;
            offsetX = 1.5 - (x % 4);
            offsetY = 1.5 - (x % 16) / 4;
            offsetZ = 1.5 - (x / 16);
            return new Point3D(offsetX, offsetY, offsetZ);
        }

        private Model3DCollection Create_State()
        {
            Model3DCollection col = new Model3DCollection();
            for (int i = 63; i >= 0; i--)
            {
                gma3D_state[i] = eBuilder.CreateCube(0.8, GetCubePos(i));
                gma3D_statetemp[i] = eBuilder.CreateCube(0.8, GetCubePos(i), null);
                col.Add(gma3D_statetemp[i]);
                col.Add(gma3D_state[i]);
            }
            return col;
        }

        private void Color_State(int round, int step, int index)
        {
            int x;
            for (int i = 0; i < 64; i++)
            {
                x = (int)(parent.cipher.States[round, step] >> i) & 1;
                gma3D_state[i].Material = material[x, index];
                gma3D_statetemp[i].Material = null;
            }
        }

        #endregion

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
                        StateKeyGet();
                        break;
                    case "Start_1":
                        StateSboxExtract();
                        break;
                    case "Start_2":
                        StatePLayer();
                        break;

                    case "StateKeyGet":
                        StateKeyXORHighlight();
                        break;
                    case "StateKeyXORHighlight":
                        StateKeyXORMerge();
                        break;
                    case "StateKeyXORMerge":
                        StateKeyXORClean();
                        break;
                    case "StateKeyXORClean":
                        StateKeyXORSet(c_group);
                        c_group++;
                        if (c_group < 16)
                        {
                            StateKeyXORHighlight();
                        }
                        else if (c_round < 31)
                        {
                            c_group = 0;
                            Color_Sbox(0);
                            Color_State(c_round, 1, 0);
                            StateSboxExtract();
                        }
                        else
                        {
                            Color_Sbox(0);
                            Color_State(c_round, 1, 0);
                            parent.lbl_EC_Text.Content = string.Format(Properties.Resources.Encrypted_to, parent.cipher.States[0, 0], parent.cipher.States[31, 1]);
                            parent.lbl_EC_Step.Content = Properties.Resources.Done;
                            resumeName = "Done";
                            Zoom();
                        }
                        break;
                    case "StateSboxExtract":
                        StateSboxIntegrate();
                        break;
                    case "StateSboxIntegrate":
                        c_group++;
                        if (c_group < 16)
                        {
                            Color_Sbox(0);
                            StateSboxExtract();
                        }
                        else
                        {
                            Color_Sbox(0);
                            Color_State(c_round, 2, 0);
                            StatePLayer();
                        }
                        break;
                    case "StatePLayer":
                        mdl3D_state.Transform = null;
                        Color_State(c_round, 3, 0);      //Init everything to unselected
                        c_group = 0;         //Start again at first block
                        c_round++;
                        if (c_round < 32)
                        {
                            Color_State(c_round, 0, 0);
                            StateKeyGet();
                        }
                        else
                        {
                            c_round = 31;
                        }
                        break;
                    #endregion
                    case "Done":
                        break;
                    default:
                        throw new Exception(string.Concat("Unhandled Function Name: '", name, "'"));
                }
            }
        }

        #region Encryption Animations

        #region Key XOR

        private void StateKeyGet()
        {
            double speedratio = parent.sld_EC_Speed.Value;
            c_step = 0;
            parent.lbl_EC_Text.Content = string.Format(Properties.Resources.Key_XOR_, c_round, parent.cipher.States[c_round, 0], parent.cipher.States[c_round, 1]);
            parent.lbl_EC_Step.Content = typeof(PRESENT).GetPluginStringResource(steps[0]);

            Point3D pos = GetRoundKeyPosition(c_round);
            Point3D des = new Point3D(-6, 0, 0);
            TranslateTransform3D tt = new TranslateTransform3D();
            DoubleAnimation dx = new DoubleAnimation(pos.X, des.X, TimeSpan.FromSeconds(2)) { SpeedRatio = speedratio };
            DoubleAnimation dy = new DoubleAnimation(pos.Y, des.Y, TimeSpan.FromSeconds(2)) { SpeedRatio = speedratio };
            DoubleAnimation dz = new DoubleAnimation(pos.Z, des.Z, TimeSpan.FromSeconds(2)) { SpeedRatio = speedratio };
            mdl3D_roundkey[c_round].Transform = tt;
            Color_RoundKeyCube(c_round, 0);
            Color_State(c_round, 0, 0);
            tt.BeginAnimation(TranslateTransform3D.OffsetXProperty, dx);
            tt.BeginAnimation(TranslateTransform3D.OffsetYProperty, dy);
            tt.BeginAnimation(TranslateTransform3D.OffsetZProperty, dz);

            countdown(2, MethodBase.GetCurrentMethod().Name, speedratio);
        }

        private void StateKeyXORHighlight()
        {
            double speedratio = parent.sld_EC_Speed.Value;
            int p, x, z, a, b, c;
            p = c_group * 4;
            a = (int)(parent.cipher.RoundKeys[c_round] >> p) & 0x0f;
            b = (int)(parent.cipher.States[c_round, 0] >> p) & 0x0f;
            c = (int)(parent.cipher.States[c_round, 1] >> p) & 0x0f;
            parent.lbl_EC_Text.Content = string.Format(Properties.Resources.bit_group_xor, c_group + 1, p + 3, p + 2, p + 1, p, Int2BinaryString(a, 4), Int2BinaryString(b, 4), Int2BinaryString(c, 4));

            for (int i = Math.Max(0, p - 4); i < p; i++)
            {
                z = (int)(parent.cipher.States[c_round, 1] >> i) & 1;
                gma3D_roundkeys[c_round, i].Material = null;
                gma3D_state[i].Material = material[z, 0];
            }

            for (int i = p; i < p + 4; i++)
            {
                x = (int)(parent.cipher.RoundKeys[c_round] >> i) & 1;
                z = (int)(parent.cipher.States[c_round, 0] >> i) & 1;
                gma3D_roundkeys[c_round, i].Material = material[x, 1];
                gma3D_state[i].Material = null;
                gma3D_statetemp[i].Material = material[z, 1];
            }

            if (c_group == 0)
            {
                DoubleAnimation da = new DoubleAnimation(opacity_default, opacity_high, TimeSpan.FromSeconds(1)) { SpeedRatio = speedratio };
                DoubleAnimation db = new DoubleAnimation(opacity_default, opacity_high, TimeSpan.FromSeconds(1)) { SpeedRatio = speedratio };
                brush[0, 1].BeginAnimation(Brush.OpacityProperty, da);
                brush[1, 1].BeginAnimation(Brush.OpacityProperty, db);
                DoubleAnimation dc = new DoubleAnimation(opacity_default, opacity_low, TimeSpan.FromSeconds(1)) { SpeedRatio = speedratio };
                brush[0, 0].BeginAnimation(Brush.OpacityProperty, dc);
                brush[1, 0].BeginAnimation(Brush.OpacityProperty, dc);
            }
            else
            {
                DoubleAnimation da = new DoubleAnimation(opacity_low, opacity_high, TimeSpan.FromSeconds(1)) { SpeedRatio = speedratio };
                DoubleAnimation db = new DoubleAnimation(opacity_low, opacity_high, TimeSpan.FromSeconds(1)) { SpeedRatio = speedratio };
                brush[0, 1].BeginAnimation(Brush.OpacityProperty, da);
                brush[1, 1].BeginAnimation(Brush.OpacityProperty, db);
            }
            countdown(1, MethodBase.GetCurrentMethod().Name, speedratio);
        }

        private void StateKeyXORMerge()
        {
            double speedratio = parent.sld_EC_Speed.Value;

            int p, z;
            p = c_group * 4;

            for (int i = Math.Max(0, p - 4); i < p; i++)
            {
                z = (int)(parent.cipher.States[c_round, 1] >> i) & 1;
                gma3D_roundkeys[c_round, i].Material = null;
                gma3D_state[i].Material = material[z, 0];
            }

            for (int i = p; i < p + 4; i++)
            {
                z = (int)(parent.cipher.States[c_round, 1] >> i) & 1;
                gma3D_state[i].Material = material[z, 2];
            }

            DoubleAnimation da = new DoubleAnimation(0, TimeSpan.FromSeconds(2)) { SpeedRatio = speedratio };
            DoubleAnimation db = new DoubleAnimation(0, TimeSpan.FromSeconds(2)) { SpeedRatio = speedratio };
            brush[0, 1].BeginAnimation(Brush.OpacityProperty, da);
            brush[1, 1].BeginAnimation(Brush.OpacityProperty, db);

            DoubleAnimation dc = new DoubleAnimation(opacity_high, TimeSpan.FromSeconds(2)) { SpeedRatio = speedratio };
            brush[0, 2].BeginAnimation(Brush.OpacityProperty, dc);
            brush[1, 2].BeginAnimation(Brush.OpacityProperty, dc);

            countdown(2, MethodBase.GetCurrentMethod().Name, speedratio);
        }

        private void StateKeyXORClean()
        {
            double speedratio = parent.sld_EC_Speed.Value;

            if (c_group == 15)
            {
                DoubleAnimation da = new DoubleAnimation(opacity_default, TimeSpan.FromSeconds(1)) { SpeedRatio = speedratio };
                DoubleAnimation db = new DoubleAnimation(opacity_default, TimeSpan.FromSeconds(1)) { SpeedRatio = speedratio };
                brush[0, 2].BeginAnimation(Brush.OpacityProperty, da);
                brush[1, 2].BeginAnimation(Brush.OpacityProperty, db);
                DoubleAnimation dc = new DoubleAnimation(opacity_default, TimeSpan.FromSeconds(1)) { SpeedRatio = speedratio };
                brush[0, 0].BeginAnimation(Brush.OpacityProperty, dc);
                brush[1, 0].BeginAnimation(Brush.OpacityProperty, dc);
            }
            else
            {
                DoubleAnimation da = new DoubleAnimation(opacity_low, TimeSpan.FromSeconds(1)) { SpeedRatio = speedratio };
                DoubleAnimation db = new DoubleAnimation(opacity_low, TimeSpan.FromSeconds(1)) { SpeedRatio = speedratio };
                brush[0, 2].BeginAnimation(Brush.OpacityProperty, da);
                brush[1, 2].BeginAnimation(Brush.OpacityProperty, db);
            }
            countdown(1, MethodBase.GetCurrentMethod().Name, speedratio);
        }

        private void StateKeyXORSet(int x)
        {
            int z, p = x * 4;
            if (c_round < 32 && p < 64)
            {
                for (int i = p; i < p + 4; i++)
                {
                    z = (int)(parent.cipher.States[c_round, 1] >> i) & 1;
                    gma3D_state[i].Material = material[z, 0];
                    gma3D_roundkeys[c_round, i].Material = null;
                    gma3D_statetemp[i].Material = null;
                }
            }
        }

        #endregion

        #region SBox

        private void StateSboxExtract()
        {
            double speedratio = parent.sld_EC_Speed.Value;
            c_step = 1;
            //parent.lbl_EC_Text.Content = String.Format("S-Box (0x{1:x16} -> 0x{2:x16})", c_round, parent.cipher.States[c_round, 1], parent.cipher.States[c_round, 2]);
            parent.lbl_EC_Step.Content = typeof(PRESENT).GetPluginStringResource(steps[c_step]);

            int p, x, r, s;
            TranslateTransform3D tt = new TranslateTransform3D();

            p = c_group * 4;
            r = (int)(parent.cipher.States[c_round, 1] >> p) & 0x0f;
            s = parent.cipher.Sbox4[r];
            parent.lbl_EC_Text.Content = string.Format(Properties.Resources.bit_group_sbox, c_group + 1, p + 3, p + 2, p + 1, p, Int2BinaryString(r, 4), Int2BinaryString(s, 4));

            for (int i = 0; i < 8; i++)
            {
                if (i < 4)
                {
                    x = (r >> (3 - i)) & 1;
                }
                else
                {
                    x = (s >> (7 - i)) & 1;
                }

                gma3D_sbox[r, i].Material = material[x, 1];
            }

            for (int i = Math.Max(0, p - 4); i < p; i++)
            {
                x = (int)(parent.cipher.States[c_round, 2] >> i) & 1;
                gma3D_state[i].Material = material[x, 0];
                gma3D_state[i].Transform = null;
            }

            for (int i = 3; i >= 0; i--)
            {
                p = c_group * 4 + i;
                x = (int)(parent.cipher.States[c_round, 1] >> p) & 1;
                gma3D_state[p].Material = material[x, 1];
                gma3D_state[p].Transform = tt;
                c_sel[i] = p;
            }

            CosDoubleAnimation dax = new CosDoubleAnimation() { From = Math.PI * 3 / 2, To = Math.PI * 2, Scale = 4, Duration = TimeSpan.FromSeconds(1), SpeedRatio = speedratio };

            DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames() { Duration = TimeSpan.FromSeconds(1), SpeedRatio = speedratio };
            da.KeyFrames.Add(new LinearDoubleKeyFrame(opacity_high, KeyTime.FromPercent(.2)));
            da.KeyFrames.Add(new LinearDoubleKeyFrame(opacity_high, KeyTime.FromPercent(1)));
            brush[0, 1].BeginAnimation(Brush.OpacityProperty, da);
            brush[1, 1].BeginAnimation(Brush.OpacityProperty, da);

            if (c_group == 0)
            {
                DoubleAnimation db = new DoubleAnimation(opacity_low, TimeSpan.FromSeconds(1)) { SpeedRatio = speedratio };
                brush[0, 0].BeginAnimation(Brush.OpacityProperty, db);
                brush[1, 0].BeginAnimation(Brush.OpacityProperty, db);
            }

            tt.BeginAnimation(TranslateTransform3D.OffsetXProperty, dax);

            countdown(2, MethodBase.GetCurrentMethod().Name, speedratio);
        }

        private void StateSboxIntegrate()
        {
            double speedratio = parent.sld_EC_Speed.Value;

            int p, x;
            TranslateTransform3D tt = new TranslateTransform3D();

            p = c_group * 4;

            for (int i = 3; i >= 0; i--)
            {
                p = c_group * 4 + i;
                x = (int)(parent.cipher.States[c_round, 2] >> p) & 1;
                gma3D_state[p].Material = material[x, 1];
                gma3D_state[p].Transform = tt;
                c_sel[i] = p;
            }

            CosDoubleAnimation dax = new CosDoubleAnimation() { From = Math.PI * 2, To = Math.PI * 3 / 2, Scale = 4, Duration = TimeSpan.FromSeconds(1), SpeedRatio = speedratio };

            if (c_group == 15)
            {
                DoubleAnimation da = new DoubleAnimation(opacity_default, TimeSpan.FromSeconds(1)) { SpeedRatio = speedratio };
                brush[0, 1].BeginAnimation(Brush.OpacityProperty, da);
                brush[1, 1].BeginAnimation(Brush.OpacityProperty, da);

                DoubleAnimation db = new DoubleAnimation(opacity_default, TimeSpan.FromSeconds(1)) { SpeedRatio = speedratio };
                brush[0, 0].BeginAnimation(Brush.OpacityProperty, db);
                brush[1, 0].BeginAnimation(Brush.OpacityProperty, db);
            }
            else
            {
                DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames() { Duration = TimeSpan.FromSeconds(1), SpeedRatio = speedratio };
                da.KeyFrames.Add(new LinearDoubleKeyFrame(opacity_high, KeyTime.FromPercent(.8)));
                da.KeyFrames.Add(new LinearDoubleKeyFrame(opacity_low, KeyTime.FromPercent(1)));
                brush[0, 1].BeginAnimation(Brush.OpacityProperty, da);
                brush[1, 1].BeginAnimation(Brush.OpacityProperty, da);
            }

            tt.BeginAnimation(TranslateTransform3D.OffsetXProperty, dax);

            countdown(1.5, MethodBase.GetCurrentMethod().Name, speedratio);
        }

        #endregion

        #region P-Layer
        private void StatePLayer()
        {
            double speedratio = parent.sld_EC_Speed.Value;

            c_step = 2;
            parent.lbl_EC_Step.Content = typeof(PRESENT).GetPluginStringResource(steps[c_step]);
            parent.lbl_EC_Text.Content = string.Format("pLayer (0x{0:x16} -> {1:x16})", parent.cipher.States[c_round, 2], parent.cipher.States[c_round, 3]);
            DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames() { Duration = new Duration(TimeSpan.FromSeconds(3)), SpeedRatio = speedratio };
            da.KeyFrames.Add(new LinearDoubleKeyFrame() { Value = opacity_high, KeyTime = KeyTime.FromPercent(0.25) });
            da.KeyFrames.Add(new LinearDoubleKeyFrame() { Value = opacity_high, KeyTime = KeyTime.FromPercent(0.75) });
            da.KeyFrames.Add(new LinearDoubleKeyFrame() { Value = opacity_default, KeyTime = KeyTime.FromPercent(1) });
            brush[0, 0].BeginAnimation(Brush.OpacityProperty, da);
            brush[1, 0].BeginAnimation(Brush.OpacityProperty, da);
            RotateTransform3D myRotateTransform3D = new RotateTransform3D()
            {
                Rotation = new AxisAngleRotation3D(new Vector3D(1, 1, 1), 120)
            };
            mdl3D_state.Transform = myRotateTransform3D;

            DoubleAnimation a = new DoubleAnimation()
            {
                Duration = new Duration(TimeSpan.FromSeconds(2)),
                From = 0,
                To = -120,
                AccelerationRatio = 0.5,
                DecelerationRatio = 0.3,
                SpeedRatio = speedratio
            };
            myRotateTransform3D.Rotation.BeginAnimation(AxisAngleRotation3D.AngleProperty, a);

            countdown(3, MethodBase.GetCurrentMethod().Name, speedratio);
        }

        #endregion

        #endregion

        private Point3D GetRoundKeyPosition(int r)
        {
            double size = 4.5;
            double offsetX, offsetY, offsetZ;
            offsetX = -25 - (r % 4) * size;
            offsetY = -.5 - ((r % 16) / 4) * size;
            offsetZ = -50 - (r / 16) * size;
            return new Point3D(offsetX, offsetY, offsetZ);
        }

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
