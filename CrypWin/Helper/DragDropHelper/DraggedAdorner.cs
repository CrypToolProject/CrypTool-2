/*
   Copyright 2008 - 2022 CrypTool Team

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
using System.Windows.Documents;
using System.Windows.Media;

namespace DragDropListBox
{
    public class DraggedAdorner : Adorner
    {
        private readonly ContentPresenter contentPresenter;
        private double left;
        private double top;
        private readonly AdornerLayer adornerLayer;

        public DraggedAdorner(object dragDropData, DataTemplate dragDropTemplate, UIElement adornedElement, AdornerLayer adornerLayer)
            : base(adornedElement)
        {
            this.adornerLayer = adornerLayer;

            contentPresenter = new ContentPresenter
            {
                Content = dragDropData,
                ContentTemplate = dragDropTemplate,
                Opacity = 0.7
            };

            this.adornerLayer.Add(this);
        }

        public void SetPosition(double left, double top)
        {
            // -1 and +13 align the dragged adorner with the dashed rectangle that shows up
            // near the mouse cursor when dragging.
            this.left = left - 1;
            this.top = top + 13;
            if (adornerLayer != null)
            {
                adornerLayer.Update(AdornedElement);
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            contentPresenter.Measure(constraint);
            return contentPresenter.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            contentPresenter.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            return contentPresenter;
        }

        protected override int VisualChildrenCount => 1;

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            GeneralTransformGroup result = new GeneralTransformGroup();
            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(left, top));

            return result;
        }

        public void Detach()
        {
            adornerLayer.Remove(this);
        }

    }
}
