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

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace CrypTool.PRESENT
{
    public class CubeBuilder
    {
        private Color _color;

        public CubeBuilder(Color color)
        {
            _color = color;
        }

        public GeometryModel3D Create()
        {
            return Create(1, 1, 1, new Point3D(0, 0, 0));
        }

        public GeometryModel3D Create(Point3D Center)
        {
            return Create(1, 1, 1, Center);
        }

        public GeometryModel3D Create(double SizeAll)
        {
            return Create(SizeAll, SizeAll, SizeAll, new Point3D(0, 0, 0));
        }

        public GeometryModel3D Create(double SizeAll, Point3D Center)
        {
            return Create(SizeAll, SizeAll, SizeAll, Center);
        }

        public GeometryModel3D Create(double SizeX, double SizeY, double SizeZ, Point3D Center)
        {
            double x = (SizeX / 2);
            double y = (SizeY / 2);
            double z = (SizeZ / 2);
            double cx = Center.X;
            double cy = Center.Y;
            double cz = Center.Z;
            GeometryModel3D cube = new GeometryModel3D()
            {
                Geometry = new MeshGeometry3D()
                {
                    Positions = new Point3DCollection() {
                        new Point3D(cx - x, cy + y, cz + z), //0            5---------4
                        new Point3D(cx + x, cy + y, cz + z), //1           /|        /|
                        new Point3D(cx - x, cy - y, cz + z), //2          / |       / |
                        new Point3D(cx + x, cy - y, cz + z), //3         0---------1  |
                        new Point3D(cx + x, cy + y, cz - z), //4         |  7--- --|--6
                        new Point3D(cx - x, cy + y, cz - z), //5         | /       | /
                        new Point3D(cx + x, cy - y, cz - z), //6         |/        |/
                        new Point3D(cx - x, cy - y, cz - z), //7         2---------3
                        new Point3D(cx - x, cy + y, cz - z), //0+8
                        new Point3D(cx - x, cy + y, cz + z), //1+8
                        new Point3D(cx - x, cy - y, cz - z), //2+8
                        new Point3D(cx - x, cy - y, cz + z), //3+8
                        new Point3D(cx + x, cy + y, cz + z), //4+8
                        new Point3D(cx + x, cy + y, cz - z), //5+8
                        new Point3D(cx + x, cy - y, cz + z), //6+8
                        new Point3D(cx + x, cy - y, cz - z), //7+8
                        new Point3D(cx - x, cy + y, cz - z), //0+16
                        new Point3D(cx + x, cy + y, cz - z), //1+16
                        new Point3D(cx - x, cy + y, cz + z), //2+16
                        new Point3D(cx + x, cy + y, cz + z), //3+16
                        new Point3D(cx + x, cy - y, cz - z), //4+16
                        new Point3D(cx - x, cy - y, cz - z), //5+16
                        new Point3D(cx + x, cy - y, cz + z), //6+16
                        new Point3D(cx - x, cy - y, cz + z)  //7+16
                    },
                    TriangleIndices = new Int32Collection() {
                        0,    2,    1,     1,    2,    3,    //front
                        4,    6,    5,     5,    6,    7,    //back
                        0+8,  2+8,  1+8,   1+8,  2+8,  3+8,  //left
                        4+8,  6+8,  5+8,   5+8,  6+8,  7+8,  //right 
                        0+16, 2+16, 1+16,  1+16, 2+16, 3+16, //top
                        4+16, 6+16, 5+16,  5+16, 6+16, 7+16  //bottom
                    },
                    TextureCoordinates = new PointCollection() {
                        new Point(0, 0), new Point(1, 0), new Point(0, 1), new Point(1, 1), //x front
                        new Point(0, 0), new Point(1, 0), new Point(0, 1), new Point(1, 1), //back
                        new Point(0, 0), new Point(1, 0), new Point(0, 1), new Point(1, 1), //left
                        new Point(1, 0), new Point(1, 1), new Point(0, 0), new Point(0, 1), //x right
                        new Point(0, 1), new Point(0, 0), new Point(1, 1), new Point(1, 0), //x top
                        new Point(0, 0), new Point(1, 0), new Point(0, 1), new Point(1, 1)  //bottom
                    }
                },
                Material = new DiffuseMaterial(new SolidColorBrush(_color))
            };
            return cube;
        }
    }

    public class AnimationBuilder
    {
        private readonly double _speed;

        public AnimationBuilder(double Speed)
        {
            _speed = Speed;
        }

        public Transform3DCollection OutDownFrontRight(Point3D SourcePosition, Point3D TargetPosition, double Wait, double Duration)
        {
            Transform3DCollection tc = new Transform3DCollection();

            DoubleAnimation xa = new DoubleAnimation(SourcePosition.X, TargetPosition.X, new Duration(TimeSpan.FromSeconds(Duration / 2))) { BeginTime = TimeSpan.FromSeconds(0) };
            DoubleAnimation ya = new DoubleAnimation(SourcePosition.Y, TargetPosition.Y, new Duration(TimeSpan.FromSeconds(Duration / 2))) { BeginTime = TimeSpan.FromSeconds(Duration / 2) };
            DoubleAnimation za = new DoubleAnimation(SourcePosition.Z, TargetPosition.Z, new Duration(TimeSpan.FromSeconds(Duration / 2))) { BeginTime = TimeSpan.FromSeconds(Duration / 2) };
            TranslateTransform3D xt = new TranslateTransform3D(); xt.BeginAnimation(TranslateTransform3D.OffsetXProperty, xa);
            TranslateTransform3D yt = new TranslateTransform3D(); yt.BeginAnimation(TranslateTransform3D.OffsetYProperty, ya);
            TranslateTransform3D zt = new TranslateTransform3D(); zt.BeginAnimation(TranslateTransform3D.OffsetZProperty, za);

            tc.Add(xt);
            tc.Add(yt);
            tc.Add(zt);
            return tc;
        }
    }
}
