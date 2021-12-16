using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using WorkspaceManager.View.VisualComponents.CryptoLineView;
using WorkspaceManager.View.Visuals;

namespace WorkspaceManager.View.Base
{
    public static class Util
    {

        public static T TryFindParent<T>(this DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = GetParentObject(child);

            //we've reached the end of the tree
            if (parentObject == null)
            {
                return null;
            }

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                //use recursion to proceed with next level
                return TryFindParent<T>(parentObject);
            }
        }

        public static DependencyObject GetParentObject(this DependencyObject child)
        {
            if (child == null)
            {
                return null;
            }

            //handle content elements separately
            ContentElement contentElement = child as ContentElement;
            if (contentElement != null)
            {
                DependencyObject parent = ContentOperations.GetParent(contentElement);
                if (parent != null)
                {
                    return parent;
                }

                FrameworkContentElement fce = contentElement as FrameworkContentElement;
                return fce != null ? fce.Parent : null;
            }

            //also try searching for parent in framework elements (such as DockPanel, etc)
            FrameworkElement frameworkElement = child as FrameworkElement;
            if (frameworkElement != null)
            {
                DependencyObject parent = frameworkElement.Parent;
                if (parent != null)
                {
                    return parent;
                }
            }

            //if it's not a ContentElement/FrameworkElement, rely on VisualTreeHelper
            return VisualTreeHelper.GetParent(child);
        }

        public static MultiBinding CreateConnectorBinding(ConnectorVisual connectable, CryptoLineView link)
        {
            MultiBinding multiBinding = new MultiBinding
            {
                Converter = new BinConnectorVisualBindingConverter(),
                ConverterParameter = connectable
            };

            Binding binding = new Binding
            {
                Source = connectable.WindowParent,
                Path = new PropertyPath(ComponentVisual.PositionProperty)
            };
            multiBinding.Bindings.Add(binding);

            binding = new Binding
            {
                Source = link.Line,
                Path = new PropertyPath(InternalCryptoLineView.StrokeThicknessProperty)
            };
            multiBinding.Bindings.Add(binding);

            binding = new Binding
            {
                Source = connectable.WindowParent.West,
                Path = new PropertyPath(FrameworkElement.ActualHeightProperty)
            };
            multiBinding.Bindings.Add(binding);

            binding = new Binding
            {
                Source = connectable.WindowParent.East,
                Path = new PropertyPath(FrameworkElement.ActualHeightProperty)
            };
            multiBinding.Bindings.Add(binding);

            binding = new Binding
            {
                Source = connectable.WindowParent.North,
                Path = new PropertyPath(FrameworkElement.ActualWidthProperty)
            };
            multiBinding.Bindings.Add(binding);

            binding = new Binding
            {
                Source = connectable.WindowParent.South,
                Path = new PropertyPath(FrameworkElement.ActualWidthProperty)
            };
            multiBinding.Bindings.Add(binding);

            binding = new Binding
            {
                Source = connectable,
                Path = new PropertyPath(ConnectorVisual.PositionProperty)
            };
            multiBinding.Bindings.Add(binding);

            return multiBinding;
        }

        public static MultiBinding CreateIsDraggingBinding(object[] value)
        {
            MultiBinding multiBinding = new MultiBinding
            {
                Converter = new IsDraggingConverter()
            };

            Binding binding;
            Type valueType = value.GetType();
            if (valueType.IsArray && typeof(Thumb).IsAssignableFrom(valueType.GetElementType()))
            {

                foreach (Thumb t in value.Cast<Thumb>())
                {
                    binding = new Binding
                    {
                        Source = t,
                        Path = new PropertyPath(Thumb.IsDraggingProperty)
                    };
                    multiBinding.Bindings.Add(binding);
                }
            }

            if (valueType.IsArray && typeof(ComponentVisual).IsAssignableFrom(valueType.GetElementType()))
            {

                foreach (ComponentVisual b in value.Cast<ComponentVisual>())
                {
                    binding = new Binding
                    {
                        Source = b,
                        Path = new PropertyPath(ComponentVisual.IsDraggingProperty)
                    };
                    multiBinding.Bindings.Add(binding);
                }
            }

            return multiBinding;
        }

        public static class MouseUtilities
        {
            public static Point CorrectGetPosition(Visual relativeTo)
            {
                Win32Point w32Mouse = new Win32Point();
                GetCursorPos(ref w32Mouse);
                return relativeTo.PointFromScreen(new Point(w32Mouse.X, w32Mouse.Y));
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct Win32Point
            {
                public int X;
                public int Y;
            };

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool GetCursorPos(ref Win32Point pt);
        }
    }
}
