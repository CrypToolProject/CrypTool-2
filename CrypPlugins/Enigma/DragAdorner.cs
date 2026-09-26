/*
   Copyright (C) CrypTool 2 Team

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
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;


namespace CrypTool.Enigma
{
    internal class DragAdorner : Adorner
    {
        protected UIElement _child;
        protected UIElement _owner;
        protected double XCenter;
        protected double YCenter;

        public DragAdorner(UIElement owner) : base(owner) { }

        public DragAdorner(UIElement owner, UIElement adornElement, bool useVisualBrush, double opacity, double mainwidth, double mainheight)
            : base(owner)
        {
            System.Diagnostics.Debug.Assert(owner != null);
            System.Diagnostics.Debug.Assert(adornElement != null);
            _owner = owner;

            if (useVisualBrush)
            {
                VisualBrush _brush = new VisualBrush(adornElement)
                {
                    Opacity = opacity
                };
                Rectangle r = new Rectangle
                {
                    RadiusX = 3,
                    RadiusY = 3,

                    //TODO: questioning DesiredSize vs. Actual 
                    Width = adornElement.DesiredSize.Width / 2200 * mainwidth,
                    Height = adornElement.DesiredSize.Height / 1250 * mainheight
                };

                XCenter = adornElement.DesiredSize.Width / 2200 * mainwidth / 2;
                YCenter = adornElement.DesiredSize.Height / 1250 * mainheight / 2;

                r.Fill = _brush;
                _child = r;

            }
            else
            {
                _child = adornElement;
            }
        }


        private double _leftOffset;
        public double LeftOffset
        {
            get => _leftOffset;
            set
            {
                _leftOffset = value - XCenter;
                UpdatePosition();
            }
        }

        private double _topOffset;
        public double TopOffset
        {
            get => _topOffset;
            set
            {
                _topOffset = value - YCenter;

                UpdatePosition();
            }
        }

        private void UpdatePosition()
        {
            AdornerLayer adorner = (AdornerLayer)Parent;
            if (adorner != null)
            {
                adorner.Update(AdornedElement);
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            return _child;
        }

        protected override int VisualChildrenCount => 1;


        protected override Size MeasureOverride(Size finalSize)
        {
            _child.Measure(finalSize);
            return _child.DesiredSize;
        }
        protected override Size ArrangeOverride(Size finalSize)
        {

            _child.Arrange(new Rect(_child.DesiredSize));
            return finalSize;
        }

        public double scale = 1.0;
        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            GeneralTransformGroup result = new GeneralTransformGroup();

            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(_leftOffset, _topOffset));
            return result;
        }
    }


}

