/*
   Copyright 2026 CrypTool Project
   Licensed under the Apache License, Version 2.0.
   http://www.apache.org/licenses/LICENSE-2.0
*/
using System;
using System.Windows;
using System.Windows.Controls;
using WorkspaceManager.Model;
using WorkspaceManager.View.Visuals;

namespace CrypTool.CrypLLM.Services
{
    /// <summary>
    /// Resolves workspace sizes in device-independent canvas units, independent of zoom.
    /// Auto-sized elements can store zero dimensions even though their visual has a real size.
    /// </summary>
    internal static class WorkspaceElementGeometry
    {
        internal static FrameworkElement GetWindow(VisualElementModel element)
        {
            if (element.UpdateableView is ComponentVisual component)
                return component.FindName("Window") as FrameworkElement;
            if (element.UpdateableView is UserControl control)
                return control.Content as FrameworkElement ?? control;
            return element.UpdateableView as FrameworkElement;
        }

        internal static ElementSize Measure(VisualElementModel element)
        {
            FrameworkElement window = GetWindow(element);
            window?.UpdateLayout();
            double minWidth = window?.MinWidth ?? (element is TextModel ? 150 : element.GetMinWidth());
            double minHeight = window?.MinHeight ?? (element is TextModel ? 100 : element.GetMinHeight());
            double width = window?.ActualWidth ?? 0;
            double height = window?.ActualHeight ?? 0;
            bool measured = IsPositiveFinite(width) && IsPositiveFinite(height);
            if (!IsPositiveFinite(width)) width = IsPositiveFinite(element.GetWidth()) ? element.GetWidth() : Math.Max(1, minWidth);
            if (!IsPositiveFinite(height)) height = IsPositiveFinite(element.GetHeight()) ? element.GetHeight() : Math.Max(1, minHeight);
            return new ElementSize
            {
                Width = width, Height = height, MinWidth = minWidth, MinHeight = minHeight,
                MaxWidth = FiniteMaximum(window?.MaxWidth), MaxHeight = FiniteMaximum(window?.MaxHeight),
                SizeSource = measured ? "visual" :
                    IsPositiveFinite(element.GetWidth()) && IsPositiveFinite(element.GetHeight()) ? "model" : "minimum-fallback"
            };
        }

        internal static bool IsPositiveFinite(double value) => value > 0 && !double.IsNaN(value) && !double.IsInfinity(value);
        private static double? FiniteMaximum(double? value) => value.HasValue && IsPositiveFinite(value.Value) ? value : null;

        /// <summary>
        /// The model position anchors the entire control, including the connector rails.
        /// Its inner Window dimensions alone do not describe the actual box location.
        /// </summary>
        internal static Rect GetBodyBounds(VisualElementModel element)
        {
            FrameworkElement view = element.UpdateableView as FrameworkElement;
            FrameworkElement window = GetWindow(element);
            FrameworkElement body = element is PluginModel ? window?.Parent as FrameworkElement ?? window : window;
            body?.UpdateLayout();
            if (view != null && body != null && IsPositiveFinite(body.ActualWidth) && IsPositiveFinite(body.ActualHeight))
            {
                Point offset = ReferenceEquals(body, view) ? new Point() : body.TransformToAncestor(view).Transform(new Point());
                Point anchor = element.GetPosition();
                return new Rect(new Point(anchor.X + offset.X, anchor.Y + offset.Y), new Size(body.ActualWidth, body.ActualHeight));
            }
            ElementSize size = Measure(element);
            return new Rect(element.GetPosition(), new Size(size.Width, size.Height));
        }

        /// <summary>Occupied bounds also include connector rails and the caption below a component.</summary>
        internal static Rect GetOccupiedBounds(VisualElementModel element)
        {
            if (element.UpdateableView is FrameworkElement view)
            {
                view.UpdateLayout();
                if (IsPositiveFinite(view.ActualWidth) && IsPositiveFinite(view.ActualHeight))
                    return new Rect(element.GetPosition(), new Size(view.ActualWidth, view.ActualHeight));
            }
            return GetBodyBounds(element);
        }

        internal static object Project(Rect rect) => new { x = rect.X, y = rect.Y, width = rect.Width, height = rect.Height };

        /// <summary>Small bounds projection for the main workspace model tool.</summary>
        internal static object CreateBounds(VisualElementModel element)
        {
            ElementSize size = Measure(element);
            Point position = element.GetPosition();
            return new
            {
                x = position.X, y = position.Y, width = size.Width, height = size.Height, sizeSource = size.SizeSource,
                bodyBounds = Project(GetBodyBounds(element)), occupiedBounds = Project(GetOccupiedBounds(element))
            };
        }
    }

    internal sealed class ElementSize
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public double MinWidth { get; set; }
        public double MinHeight { get; set; }
        public double? MaxWidth { get; set; }
        public double? MaxHeight { get; set; }
        public string SizeSource { get; set; }
    }
}
