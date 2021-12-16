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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CrypTool.PRESENT
{
    public class ElementBuilder
    {
        private readonly Material _material;

        public ElementBuilder(Material material)
        {
            _material = material;
        }

        #region CreateCube
        public GeometryModel3D CreateCube()
        {
            return CreateCube(1, 1, 1, new Point3D(0, 0, 0), _material);
        }

        public GeometryModel3D CreateCube(Point3D Center)
        {
            return CreateCube(1, 1, 1, Center, _material);
        }

        public GeometryModel3D CreateCube(double SizeAll)
        {
            return CreateCube(SizeAll, SizeAll, SizeAll, new Point3D(0, 0, 0), _material);
        }

        public GeometryModel3D CreateCube(double SizeAll, Point3D Center)
        {
            return CreateCube(SizeAll, SizeAll, SizeAll, Center, _material);
        }

        public GeometryModel3D CreateCube(double SizeAll, Point3D Center, Material material)
        {
            return CreateCube(SizeAll, SizeAll, SizeAll, Center, material);
        }

        public GeometryModel3D CreateCube(double SizeX, double SizeY, double SizeZ, Point3D Center, Material material)
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
                        //new Point(0, 0), new Point(1, 0), new Point(0, 1), new Point(1, 1), //back
                        new Point(0,0),new Point(0,0),new Point(0,0),new Point(0,0),
                        //new Point(0, 0), new Point(1, 0), new Point(0, 1), new Point(1, 1), //left
                        new Point(0,0),new Point(0,0),new Point(0,0),new Point(0,0),
                        new Point(1, 0), new Point(1, 1), new Point(0, 0), new Point(0, 1), //x right
                        new Point(0, 1), new Point(0, 0), new Point(1, 1), new Point(1, 0), //x top
                        //new Point(0, 0), new Point(1, 0), new Point(0, 1), new Point(1, 1)  //bottom
                        new Point(0,0),new Point(0,0),new Point(0,0),new Point(0,0),
                    }
                },
                Material = material
            };
            return cube;
        }
        #endregion

        #region CreateLabel
        public GeometryModel3D CreateLabel(double SizeX, double SizeY, Point3D Center, Color Background, string LabelText, Color TextColor, double Opacity)
        {
            double cx = Center.X;
            double cy = Center.Y;
            double cz = Center.Z;
            MaterialCollection material = new MaterialCollection
            {
                new DiffuseMaterial(new SolidColorBrush(Background) { Opacity = Opacity }),
                new DiffuseMaterial(new VisualBrush(new TextBlock() { Text = LabelText, Background = new SolidColorBrush(Background), Foreground = new SolidColorBrush(TextColor), Padding = new Thickness(0, 0, 2, 0), Opacity = Opacity, HorizontalAlignment = HorizontalAlignment.Center }) { Stretch = Stretch.Uniform, Opacity = Opacity })
            };
            GeometryModel3D label = new GeometryModel3D()
            {
                Geometry = new MeshGeometry3D()
                {
                    Positions = new Point3DCollection() {
                        new Point3D(cx, cy, cz),
                        new Point3D(cx+SizeX, cy, cz),
                        new Point3D(cx,cy-SizeY, cz),
                        new Point3D(cx+SizeX,cy-SizeY, cz)
                    },
                    TriangleIndices = new Int32Collection() {
                        0,    2,    1,     1,    2,    3    //square
                    },
                    TextureCoordinates = new PointCollection() {
                        new Point(0, 0), new Point(1, 0), new Point(0, 1), new Point(1, 1) //square
                    }
                },
                Material = new MaterialGroup() { Children = material }
            };
            return label;
        }
        #endregion

        public GeometryModel3D FlatLabel(double SizeX, double SizeZ, double SizeY, Point3D Center, Color Background, string LabelText, Color TextColor, double Opacity)
        {
            double cx = Center.X;
            double cy = Center.Y;
            double cz = Center.Z;
            MaterialCollection material = new MaterialCollection
            {
                new DiffuseMaterial(new SolidColorBrush(Background) { Opacity = Opacity }),
                new DiffuseMaterial(new VisualBrush(new TextBlock() { Text = LabelText, Background = new SolidColorBrush(Background), Foreground = new SolidColorBrush(TextColor), Opacity = Opacity, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Bottom }) { Stretch = Stretch.Uniform, Opacity = Opacity })
            };
            GeometryModel3D label = new GeometryModel3D()
            {
                Geometry = new MeshGeometry3D()
                {
                    Positions = new Point3DCollection() {
                        new Point3D(cx-SizeX/2, cy, cz+SizeZ/2),
                        new Point3D(cx+SizeX/2, cy, cz+SizeZ/2),
                        new Point3D(cx-SizeX/2, cy-SizeY, cz+SizeZ/2),
                        new Point3D(cx+SizeX/2, cy-SizeY, cz+SizeZ/2),

                        new Point3D(cx-SizeX/2, cy, cz-SizeZ/2),
                        new Point3D(cx+SizeX/2, cy, cz-SizeZ/2),
                        new Point3D(cx-SizeX/2, cy, cz+SizeZ/2),
                        new Point3D(cx+SizeX/2, cy, cz+SizeZ/2),
                    },
                    TriangleIndices = new Int32Collection() {
                        0,    2,    1,     1,    2,    3,    //square
                        4,    6,    5,     5,    6,    7    //square
                    },
                    TextureCoordinates = new PointCollection() {
                        new Point(0, 0), new Point(1, 0), new Point(0, 1), new Point(1, 1) //square
                    }
                },
                Material = new MaterialGroup() { Children = material }
            };
            return label;
        }

        #region CreateArrowRight
        public GeometryModel3D CreateArrowRight(double SizeX, double SizeY, Point3D Arrowhead, Color ArrowColor, double Opacity)
        {
            double cx = Arrowhead.X;
            double cy = Arrowhead.Y;
            double cz = Arrowhead.Z;
            Material material = new DiffuseMaterial(new SolidColorBrush(ArrowColor) { Opacity = Opacity });
            GeometryModel3D label = new GeometryModel3D()
            {
                Geometry = new MeshGeometry3D()
                {
                    Positions = new Point3DCollection() {
                        new Point3D(cx - SizeY/2 - SizeX, cy + SizeY/2, cz),         //0      0-------1\
                        new Point3D(cx - SizeY/2, cy + SizeY/2, cz),                 //1      |       | \4
                        new Point3D(cx - SizeY/2 - SizeX, cy - SizeY/2, cz),         //2      |       | /
                        new Point3D(cx - SizeY/2, cy - SizeY/2, cz),                 //3      2-------3/
                        new Point3D(cx, cy, cz) //4
                    },
                    TriangleIndices = new Int32Collection() {
                        0,    2,    1,     1,    2,    3,    //square
                        1,    3,    4                        //arrow
                    },
                    TextureCoordinates = new PointCollection() {
                        new Point(0, 0), new Point(1, 0), new Point(0, 1), new Point(1, 1) //square
                    }
                },
                Material = material
            };
            return label;
        }
        #endregion

        public GeometryModel3D CreateCubeLabel(double SizeAll, Point3D Center, Material material)
        {
            double x = (SizeAll / 2);
            double y = (SizeAll / 2);
            double z = (SizeAll / 2);
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
                    },
                    TriangleIndices = new Int32Collection() {
                        0,    2,    1,     1,    2,    3,    //front
                    },
                    TextureCoordinates = new PointCollection() {
                        new Point(0, 0), new Point(1, 0), new Point(0, 1), new Point(1, 1), //x front
                    }
                },
                Material = material
            };
            return cube;
        }
    }
}
