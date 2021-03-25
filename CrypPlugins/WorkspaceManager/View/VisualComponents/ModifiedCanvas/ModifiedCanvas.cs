using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using WorkspaceManager.View.Visuals;
using WorkspaceManager.View.Base.Interfaces;

namespace WorkspaceManager.View.VisualComponents
{
    public class ModifiedCanvas : Canvas
    {
        public enum ZPaneRequest
        {
            top,
            bot,
            up,
            down
        };

        private LinkedList<IZOrdering> zPaneOrderCollection = new LinkedList<IZOrdering>();

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
            if (visualAdded is IRouting)
            {
                var bin = (IRouting)visualAdded;
                bin.PositionDeltaChanged += new EventHandler<PositionDeltaChangedArgs>(PositionDeltaChanged);
            }
            if (visualRemoved is IRouting)
            {
                var bin = (IRouting)visualRemoved;
                bin.PositionDeltaChanged -= new EventHandler<PositionDeltaChangedArgs>(PositionDeltaChanged);
            }

            if (visualAdded is IZOrdering)
            {
                var bin = (IZOrdering)visualAdded;
                zPaneOrderCollection.AddLast(bin);
                //zPaneOrderCollection = new LinkedList<IZOrdering>(zPaneOrderCollection.OrderBy(x => x.ZIndex));

            }
            if (visualRemoved is IZOrdering)
            {
                var bin = (IZOrdering)visualRemoved;
                zPaneOrderCollection.Remove(bin);
                //zPaneOrderCollection = new LinkedList<IZOrdering>(zPaneOrderCollection.OrderBy(x => x.ZIndex));
            }
        }

        void PositionDeltaChanged(object sender, PositionDeltaChangedArgs e)
        {
            this.InvalidateMeasure();
            //foreach (CryptoLineView.CryptoLineView element in base.InternalChildren.OfType<CryptoLineView.CryptoLineView>())
            //{
            //    element.Line.InvalidateVisual();
            //}
        }

        public static void RequestZIndexModification(ModifiedCanvas panel,IZOrdering obj, ZPaneRequest req)
        {
            if (panel == null || panel.zPaneOrderCollection.Find(obj) == null)
                return;
            var node = panel.zPaneOrderCollection.Find(obj);
            var next = node.Next;
            var prev = node.Previous;
            panel.zPaneOrderCollection.Remove(obj);
            switch (req)
            {
                case ZPaneRequest.bot:
                    panel.zPaneOrderCollection.AddFirst(obj);
                    break;

                case ZPaneRequest.top:
                    panel.zPaneOrderCollection.AddLast(obj);
                    break;

                case ZPaneRequest.up:
                    if (next != null)
                        panel.zPaneOrderCollection.AddAfter(next, node);
                    else
                        panel.zPaneOrderCollection.AddLast(obj);
                    break;

                case ZPaneRequest.down:
                    if (prev != null)
                        panel.zPaneOrderCollection.AddBefore(prev, node);
                    else
                        panel.zPaneOrderCollection.AddFirst(obj);
                    break;
            }

            panel.InvalidateMeasure();
        }

        protected override Size MeasureOverride(Size constraint)
        {
            base.MeasureOverride(constraint);
            Size availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
            double maxHeight = 0;
            double maxWidth = 0;
            double left;
            double top;

            foreach (UIElement element in base.InternalChildren)
            {
                if (element != null)
                {
                    element.Measure(availableSize);
                    if (element is IRouting)
                    {
                        IRouting b = (IRouting)element;

                        left = b.Position.X;
                        top = b.Position.Y;
                        left += element.DesiredSize.Width;
                        top += element.DesiredSize.Height;

                        maxWidth = maxWidth < left ? left : maxWidth;
                        maxHeight = maxHeight < top ? top : maxHeight;

                        if (element is TextVisual || element is ImageVisual)
                            Canvas.SetZIndex(element, -2);
                    }
                    else
                    {
                        left = element.DesiredSize.Width;
                        top = element.DesiredSize.Height;
                        maxWidth = maxWidth < left ? left : maxWidth;
                        maxHeight = maxHeight < top ? top : maxHeight;
                    }
                    //element.InvalidateArrange();
                }
            }

            //Possible performance improvement
            var list = zPaneOrderCollection.ToList();
            foreach (var order in list)
            {
                int i = order.ZIndex = list.IndexOf(order);
                Panel.SetZIndex((UIElement)order, i);
            }

            return new Size { Height = maxHeight, Width = maxWidth };
        }
    }
}
