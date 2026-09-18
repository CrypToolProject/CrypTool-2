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
            var occupied = elements.ToDictionary(e => e, WorkspaceElementGeometry.GetOccupiedBounds);
            var boxes = elements.ToDictionary(e => e, WorkspaceElementGeometry.GetBodyBounds);
            report.elementCount = boxes.Count;
            for (int i = 0; i < elements.Count; i++)
                for (int j = i + 1; j < elements.Count; j++)
                {
                    Rect a = occupied[elements[i]], b = occupied[elements[j]], intersection = Rect.Intersect(a, b);
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
                object routingAdvice = CreateRoutingAdvice(wire, boxes);
                if (routingAdvice != null)
                    report.Add("warning", "connector_sides_need_detour", Id(wire), null, null, routingAdvice);
                // Read the invalidated live shape so a completed route can be inspected before
                // the next asynchronous render. Do not substitute an old cached route for a missing one.
                if (wire.UpdateableView is WorkspaceManager.View.VisualComponents.CryptoLineView.CryptoLineView live &&
                    live.Line != null && !live.Line.HasComputed)
                    _ = live.Line.RenderedGeometry;
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
                        bool attached = ReferenceEquals(entry.Key, wire.From?.PluginModel) || ReferenceEquals(entry.Key, wire.To?.PluginModel);
                        Rect r = attached ? entry.Value : occupied[entry.Key];
                        r.Inflate(-0.5, -0.5);
                        // Connector attachment at the boundary is valid. Penetrating ANY body,
                        // including the source or target body, is a defect rather than a warning.
                        if (TouchesBox(s, r))
                            report.Add("error", "wire_through_element", Id(wire), Id(entry.Key), null, new
                            {
                                elementRole = ReferenceEquals(entry.Key, wire.From?.PluginModel) ? "source" : ReferenceEquals(entry.Key, wire.To?.PluginModel) ? "target" : "obstacle",
                                segment = new { from = new { x = s.A.X, y = s.A.Y }, to = new { x = s.B.X, y = s.B.Y } },
                                source = new { componentId = Id(wire.From.PluginModel), connector = wire.From.PropertyName, orientation = Orientation(wire.From).ToString() },
                                target = new { componentId = Id(wire.To.PluginModel), connector = wire.To.PropertyName, orientation = Orientation(wire.To).ToString() },
                                routingAdvice
                            });
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
        private static bool TouchesBox(WireSegment s, Rect r)
        {
            if (r.IsEmpty) return false;
            if (Horizontal(s)) return s.A.Y > r.Top && s.A.Y < r.Bottom && Math.Max(s.A.X, s.B.X) > r.Left && Math.Min(s.A.X, s.B.X) < r.Right;
            if (Vertical(s)) return s.A.X > r.Left && s.A.X < r.Right && Math.Max(s.A.Y, s.B.Y) > r.Top && Math.Min(s.A.Y, s.B.Y) < r.Bottom;
            double lo = 0, hi = 1, dx = s.B.X - s.A.X, dy = s.B.Y - s.A.Y;
            return Clip(-dx, s.A.X - r.Left, ref lo, ref hi) && Clip(dx, r.Right - s.A.X, ref lo, ref hi) &&
                Clip(-dy, s.A.Y - r.Top, ref lo, ref hi) && Clip(dy, r.Bottom - s.A.Y, ref lo, ref hi);
        }
        private static bool Clip(double p, double q, ref double lo, ref double hi)
        {
            if (Math.Abs(p) < 0.0001) return q >= 0;
            double t = q / p;
            if (p < 0) lo = Math.Max(lo, t); else hi = Math.Min(hi, t);
            return lo <= hi;
        }

        private static ConnectorOrientation Orientation(ConnectorModel connector) => connector.Orientation == ConnectorOrientation.Unset
            ? connector.Outgoing ? ConnectorOrientation.East : ConnectorOrientation.West : connector.Orientation;

        /// <summary>Offer conditional alternatives; one shared output must keep a single side for every branch.</summary>
        private static object CreateRoutingAdvice(ConnectionModel wire, Dictionary<VisualElementModel, Rect> boxes)
        {
            if (wire.From?.PluginModel == null || wire.To?.PluginModel == null ||
                !boxes.TryGetValue(wire.From.PluginModel, out Rect source) || !boxes.TryGetValue(wire.To.PluginModel, out Rect target)) return null;
            ConnectorOrientation from, to;
            if (source.Right <= target.Left) { from = ConnectorOrientation.East; to = ConnectorOrientation.West; }
            else if (target.Right <= source.Left) { from = ConnectorOrientation.West; to = ConnectorOrientation.East; }
            else if (source.Bottom <= target.Top) { from = ConnectorOrientation.South; to = ConnectorOrientation.North; }
            else if (target.Bottom <= source.Top) { from = ConnectorOrientation.North; to = ConnectorOrientation.South; }
            else return null;
            if (Orientation(wire.From) == from && Orientation(wire.To) == to) return null;
            return new
            {
                sourceComponentId = Id(wire.From.PluginModel), sourceConnector = wire.From.PropertyName,
                targetComponentId = Id(wire.To.PluginModel), targetConnector = wire.To.PropertyName,
                suggestedSourceOrientation = from.ToString(), suggestedTargetOrientation = to.ToString(),
                sharedOutputConnections = wire.From.GetOutputConnections().Count,
                alternative = "Move the receiver beyond the source's output side with a clear corridor; for East-to-West data flow keep the whole source box left of the receiver. Re-check all branches before changing a shared connector's side."
            };
        }
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
        internal void Add(string severity, string code, string elementId, string otherId = null, double? requiredHeight = null, object details = null)
        {
            string pair = string.CompareOrdinal(elementId, otherId) <= 0 ? elementId + ":" + otherId : otherId + ":" + elementId;
            if (!seen.Add(code + ":" + pair)) return;
            if (severity == "error") errorCount++; else warningCount++;
            if (issues.Count < 50) issues.Add(new { severity, code, elementId, otherId, requiredContentHeight = requiredHeight, details });
        }
    }
}
