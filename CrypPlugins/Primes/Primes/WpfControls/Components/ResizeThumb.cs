/*
   Copyright 2008 Timo Eckhardt, University of Siegen

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

using Primes.Library;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Primes.WpfControls.Components
{
    public class ResizeThumb : Thumb
    {
        private ResizerPosition position;

        public ResizerPosition Position
        {
            get => position;
            set => position = value;
        }

        public ResizeThumb()
        {
            base.DragDelta += new DragDeltaEventHandler(ResizeThumb_DragDelta);
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            ContentControl item = DataContext as ContentControl;

            if (item != null)
            {
                switch (Position)
                {
                    case ResizerPosition.Top:
                        item.Height = Math.Max(25, item.ActualHeight - e.VerticalChange);
                        break;
                    case ResizerPosition.Bottom:
                        item.Height = Math.Max(25, item.ActualHeight + e.VerticalChange);
                        break;
                    case ResizerPosition.Left:
                        item.Width = Math.Max(25, item.ActualWidth - e.HorizontalChange);
                        break;
                    case ResizerPosition.Right:
                        item.Width = Math.Max(25, item.ActualWidth + e.HorizontalChange);
                        break;
                    case ResizerPosition.TopLeft:
                        item.Height = Math.Max(25, item.ActualHeight - e.VerticalChange);
                        item.Width = Math.Max(25, item.ActualWidth - e.HorizontalChange);
                        break;
                    case ResizerPosition.TopRight:
                        item.Height = Math.Max(25, item.ActualHeight - e.VerticalChange);
                        item.Width = Math.Max(25, item.ActualWidth + e.HorizontalChange);
                        break;
                    case ResizerPosition.BottomLeft:
                        item.Height = Math.Max(25, item.ActualHeight + e.VerticalChange);
                        item.Width = Math.Max(25, item.ActualWidth - e.HorizontalChange);
                        break;
                    case ResizerPosition.BottomRight:
                        item.Height = Math.Max(25, item.ActualHeight + e.VerticalChange);
                        item.Width = Math.Max(25, item.ActualWidth + e.HorizontalChange);
                        break;
                    default:
                        break;
                }
                if (OnSizeChanged != null)
                {
                    OnSizeChanged(new Size(item.Width, item.Height));
                }
            }
            e.Handled = true;
        }

        public event DoubleParameterDelegate OnSizeChanged;
        public event MouseButtonEventHandler LeftButtonUp;
        public event MouseButtonEventHandler LeftButtonDown;

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (LeftButtonUp != null)
            {
                LeftButtonUp(this, e);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (LeftButtonDown != null)
            {
                LeftButtonDown(this, e);
            }
        }
    }

    public enum ResizerPosition
    {
        None,
        Top,
        Bottom,
        Left,
        Right,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
}
