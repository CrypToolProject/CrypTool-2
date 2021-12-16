using System;
using System.Windows;
using WorkspaceManager.View.Visuals;

namespace WorkspaceManager.View.Base.Interfaces
{
    internal interface IRouting
    {
        event EventHandler<PositionDeltaChangedArgs> PositionDeltaChanged;
        ObjectSize ObjectSize { get; }
        Point Position { get; set; }
        Point GetRoutingPoint(int routPoint);
    }

    public class ObjectSize
    {
        public double X { get; set; }
        public double Y { get; set; }

        public ObjectSize(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }
    }
}
