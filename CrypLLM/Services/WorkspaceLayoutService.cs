/* Copyright 2026 CrypTool Project. Licensed under the Apache License, Version 2.0. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WorkspaceManager.Model;

namespace CrypTool.CrypLLM.Services
{
    /// <summary>Checks canvas geometry and measures formatted memo contents on the UI thread.</summary>
    internal static class WorkspaceLayoutService
    {
        internal static RichTextBox MemoBox(TextModel memo) => (memo.UpdateableView as FrameworkElement)?.FindName("mainRTB") as RichTextBox;

        internal static double MeasureMemoHeight(TextModel memo, double width)
        {
            RichTextBox live = MemoBox(memo);
            var probe = new RichTextBox
            {
                Width = width, VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Padding = live?.Padding ?? new Thickness(0), BorderThickness = live?.BorderThickness ?? new Thickness(0),
                FontFamily = live?.FontFamily ?? SystemFonts.MessageFontFamily, FontSize = live?.FontSize ?? SystemFonts.MessageFontSize
            };
            memo.loadRTB(probe);
            probe.Document.PageHeight = double.NaN;
            probe.Document.PageWidth = double.NaN;
            probe.Measure(new Size(width, double.PositiveInfinity));
            probe.Arrange(new Rect(0, 0, width, Math.Max(1, probe.DesiredSize.Height)));
            probe.UpdateLayout();
            return Math.Max(probe.DesiredSize.Height, probe.ExtentHeight);
        }

        internal static LayoutReport Check(WorkspaceModel model, double minimumGap = 40)
        {
            var report = new LayoutReport();
            var elements = model.GetAllPluginModels().Cast<VisualElementModel>().Concat(model.GetAllTextModels()).Concat(model.GetAllImageModels()).ToList();
            var boxes = elements.ToDictionary(e => e, e =>
            {
                ElementSize size = WorkspaceElementGeometry.Measure(e);
                return new Rect(e.GetPosition(), new Size(size.Width, size.Height));
            });
            report.elementCount = boxes.Count;
            for (int i = 0; i < elements.Count; i++)
                for (int j = i + 1; j < elements.Count; j++)
                {
                    Rect a = boxes[elements[i]], b = boxes[elements[j]], intersection = Rect.Intersect(a, b);
                    double dx = Math.Max(0, Math.Max(a.Left - b.Right, b.Left - a.Right));
                    double dy = Math.Max(0, Math.Max(a.Top - b.Bottom, b.Top - a.Bottom));
                    if (!intersection.IsEmpty && intersection.Width > 0.01 && intersection.Height > 0.01)
                        report.Add("error", "element_overlap", Id(elements[i]), Id(elements[j]));
                    else if (Math.Sqrt(dx * dx + dy * dy) + 0.5 < minimumGap)
                        report.Add("warning", "insufficient_spacing", Id(elements[i]), Id(elements[j]));
                }
            foreach (TextModel memo in model.GetAllTextModels())
            {
                RichTextBox box = MemoBox(memo);
                box?.UpdateLayout();
                if (box == null || box.ActualWidth <= 0 || box.ActualHeight <= 0)
                {
                    report.uncheckedMemos++;
                    report.Add("warning", "memo_visibility_unavailable", Id(memo));
                    continue;
                }
                report.checkedMemos++;
                double required = MeasureMemoHeight(memo, box.ActualWidth);
                if (required > box.ActualHeight + 1 || box.ExtentWidth > box.ViewportWidth + 1)
                    report.Add("error", "memo_content_clipped", Id(memo), null, required);
            }
            var segments = new List<WireSegment>();
            foreach (ConnectionModel wire in model.GetAllConnectionModels())
            {
                List<Point> points = wire.PointList;
                if (points == null || points.Count < 2) { report.uncheckedConnections++; continue; }
                report.checkedConnections++;
                if (Enumerable.Range(1, points.Count - 1).Any(i => Math.Abs(points[i].X - points[i - 1].X) > 0.01 && Math.Abs(points[i].Y - points[i - 1].Y) > 0.01))
                {
                    report.uncheckedConnections++;
                    report.Add("warning", "non_orthogonal_route_not_fully_checked", Id(wire));
                }
                for (int i = 1; i < points.Count; i++)
                {
                    var s = new WireSegment { Wire = wire, A = points[i - 1], B = points[i] };
                    if ((s.B - s.A).Length < 0.01) continue;
                    segments.Add(s);
                    foreach (var entry in boxes)
                    {
                        if (ReferenceEquals(entry.Key, wire.From?.PluginModel) || ReferenceEquals(entry.Key, wire.To?.PluginModel)) continue;
                        Rect r = entry.Value;
                        r.Inflate(-0.5, -0.5);
                        if ((Horizontal(s) && s.A.Y > r.Top && s.A.Y < r.Bottom && Math.Max(s.A.X, s.B.X) > r.Left && Math.Min(s.A.X, s.B.X) < r.Right) ||
                            (Vertical(s) && s.A.X > r.Left && s.A.X < r.Right && Math.Max(s.A.Y, s.B.Y) > r.Top && Math.Min(s.A.Y, s.B.Y) < r.Bottom))
                            report.Add("warning", "wire_through_element", Id(wire), Id(entry.Key));
                    }
                }
            }
            for (int i = 0; i < segments.Count; i++)
                for (int j = i + 1; j < segments.Count; j++)
                {
                    WireSegment a = segments[i], b = segments[j];
                    if (ReferenceEquals(a.Wire, b.Wire)) continue;
                    string collision = Collision(a, b);
                    if (collision != null) report.Add("warning", collision, Id(a.Wire), Id(b.Wire));
                }
            return report;
        }

        private sealed class WireSegment { public ConnectionModel Wire; public Point A; public Point B; }
        private static bool Horizontal(WireSegment s) => Math.Abs(s.A.Y - s.B.Y) < 0.01;
        private static bool Vertical(WireSegment s) => Math.Abs(s.A.X - s.B.X) < 0.01;
        private static string Collision(WireSegment a, WireSegment b)
        {
            if (Horizontal(a) && Horizontal(b) && Math.Abs(a.A.Y - b.A.Y) < 0.5 &&
                Math.Min(Math.Max(a.A.X, a.B.X), Math.Max(b.A.X, b.B.X)) - Math.Max(Math.Min(a.A.X, a.B.X), Math.Min(b.A.X, b.B.X)) > 1) return "overlaid_wires";
            if (Vertical(a) && Vertical(b) && Math.Abs(a.A.X - b.A.X) < 0.5 &&
                Math.Min(Math.Max(a.A.Y, a.B.Y), Math.Max(b.A.Y, b.B.Y)) - Math.Max(Math.Min(a.A.Y, a.B.Y), Math.Min(b.A.Y, b.B.Y)) > 1) return "overlaid_wires";
            if (Vertical(a) && Horizontal(b)) { WireSegment temp = a; a = b; b = temp; }
            if (Horizontal(a) && Vertical(b) && b.A.X >= Math.Min(a.A.X, a.B.X) - 0.01 && b.A.X <= Math.Max(a.A.X, a.B.X) + 0.01 &&
                a.A.Y >= Math.Min(b.A.Y, b.B.Y) - 0.01 && a.A.Y <= Math.Max(b.A.Y, b.B.Y) + 0.01)
            {
                Point p = new Point(b.A.X, a.A.Y);
                bool sharedEndpoint = (p - a.A).Length < 0.01 || (p - a.B).Length < 0.01;
                sharedEndpoint &= (p - b.A).Length < 0.01 || (p - b.B).Length < 0.01;
                bool sharedConnector = ReferenceEquals(a.Wire.From, b.Wire.From) || ReferenceEquals(a.Wire.To, b.Wire.To);
                if (!sharedEndpoint || !sharedConnector) return "crossing_wires";
            }
            return null;
        }
        private static string Id(object element) => element.GetHashCode().ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    internal sealed class LayoutReport
    {
        public int elementCount, checkedMemos, uncheckedMemos, checkedConnections, uncheckedConnections, errorCount, warningCount;
        public bool passed => errorCount == 0;
        public bool complete => uncheckedMemos == 0 && uncheckedConnections == 0;
        public List<object> issues { get; } = new List<object>();
        private readonly HashSet<string> seen = new HashSet<string>();
        internal void Add(string severity, string code, string elementId, string otherId = null, double? requiredHeight = null)
        {
            string pair = string.CompareOrdinal(elementId, otherId) <= 0 ? elementId + ":" + otherId : otherId + ":" + elementId;
            if (!seen.Add(code + ":" + pair)) return;
            if (severity == "error") errorCount++; else warningCount++;
            if (issues.Count < 50) issues.Add(new { severity, code, elementId, otherId, requiredContentHeight = requiredHeight });
        }
    }
}
