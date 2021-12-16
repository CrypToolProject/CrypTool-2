using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace WorkspaceManager.View.VisualComponents
{
    public enum DirSort
    {
        X_DESC,
        Y_DESC,
        X_ASC,
        Y_ASC
    };

    public enum FromToMeta
    {
        HasStartPoint,
        HasEndpoint,
        HasEndStartPoint,
        None,
    }

    public class FromTo : INotifyPropertyChanged
    {
        private Point from, to;

        public DirSort DirSort { get; private set; }
        public bool IsXDir => DirSort == DirSort.X_ASC || DirSort == DirSort.X_DESC;
        public FromToMeta MetaData
        {
            get;
            private set;
        }

        public Point To
        {
            get => to;
            set
            {
                to = value;
                checkDirSort(from, to);
                OnPropertyChanged("To");
            }
        }

        public Point From
        {
            get => from;
            set
            {
                from = value;
                checkDirSort(from, to);
                OnPropertyChanged("From");
            }
        }
        public SortedSet<IntersectPoint> Intersection { get; private set; }

        public FromTo(Point from, Point to, FromToMeta meta = FromToMeta.None)
        {
            initFromTo(from, to);
            MetaData = meta;
        }

        private void initFromTo(Point from, Point to)
        {
            this.from = from;
            this.to = to;
            checkDirSort(from, to);
            Intersection = new SortedSet<IntersectPoint>(new InLineSorter(DirSort));
        }

        private void checkDirSort(Point from, Point to)
        {
            if (From.X == To.X)
            {
                if (From.Y > To.Y)
                {
                    DirSort = DirSort.Y_DESC;
                }
                else
                {
                    DirSort = DirSort.Y_ASC;
                }

            }
            else if (From.Y == To.Y)
            {
                if (From.X > To.X)
                {
                    DirSort = DirSort.X_DESC;
                }
                else
                {
                    DirSort = DirSort.X_ASC;
                }

            }
        }

        public void Update()
        {
            checkDirSort(From, To);
        }

        public System.Drawing.RectangleF GetRectangle(double stroke = 1)
        {
            float x = 0, y = 0, sizeY = 0, sizeX = 0;
            switch (DirSort)
            {
                case DirSort.X_ASC:
                    x = (float)(From.X);
                    y = (float)(From.Y - (stroke / 2));
                    sizeX = (float)(To.X - From.X);
                    sizeY = (float)(stroke);
                    break;
                case DirSort.X_DESC:
                    x = (float)(To.X);
                    y = (float)(To.Y - (stroke / 2));
                    sizeX = (float)(From.X - To.X);
                    sizeY = (float)(stroke);
                    break;
                case DirSort.Y_ASC:
                    y = (float)(From.Y);
                    x = (float)(From.X - (stroke / 2));
                    sizeY = (float)(To.Y - From.Y);
                    sizeX = (float)(stroke);
                    break;
                case DirSort.Y_DESC:
                    y = (float)(To.Y);
                    x = (float)(To.X - (stroke / 2));
                    sizeY = (float)(From.Y - To.Y);
                    sizeX = (float)(stroke);
                    break;
            }

            return new System.Drawing.RectangleF(x, y, sizeX, sizeY);
        }

        public override string ToString()
        {
            return "From" + From.ToString() + " " + "To" + To.ToString();
        }


        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class InLineSorter : IComparer<IntersectPoint>
    {
        private readonly DirSort dirSort;

        public InLineSorter(DirSort dirsort)
        {
            dirSort = dirsort;
        }

        // Returns:
        //     Value               Meaning
        //  Less than zero      a is less than b
        //  Zero                a equals b
        //  Greater than zero   a is greater than b
        public int Compare(IntersectPoint a, IntersectPoint b)
        {
            switch (dirSort)
            {
                case DirSort.Y_ASC:
                    return a.Point.Y.CompareTo(b.Point.Y);
                case DirSort.Y_DESC:
                    return b.Point.Y.CompareTo(a.Point.Y);
                case DirSort.X_ASC:
                    return a.Point.X.CompareTo(b.Point.X);
                case DirSort.X_DESC:
                    return b.Point.X.CompareTo(a.Point.X);
                default:
                    throw new Exception("error");
            }
        }
    }
}
