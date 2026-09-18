/*
   Copyright 2026 CrypTool Project
   Licensed under the Apache License, Version 2.0.
   http://www.apache.org/licenses/LICENSE-2.0
*/
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CrypTool.CrypLLM.Threads
{
    /// <summary>
    /// Captures only workspace visuals and transports their PNG pixels as a multimodal
    /// user message after the complete tool-result group. The installed connector accepts
    /// text tool results, so the image envelope remains in history and is expanded at HTTP send.
    /// </summary>
    internal static class WorkspaceScreenshotContent
    {
        internal const int EstimatedImageTokens = 2048;
        private const string EnvelopeType = "workspace_screenshot";

        /// <summary>Renders the visible workspace viewport without changing zoom or selection.</summary>
        internal static string Capture(FrameworkElement viewport)
        {
            double width = viewport.ActualWidth;
            double height = viewport.ActualHeight;
            if (width < 1 || height < 1 || !viewport.IsVisible)
            {
                return JsonConvert.SerializeObject(new { error = "The workspace viewport is not visible. Open the workspace tab before requesting a screenshot." });
            }

            double scale = Math.Min(1.0, 1024.0 / Math.Max(width, height));
            int pixelWidth = Math.Max(1, (int)Math.Ceiling(width * scale));
            int pixelHeight = Math.Max(1, (int)Math.Ceiling(height * scale));
            var drawing = new DrawingVisual();
            using (DrawingContext context = drawing.RenderOpen())
            {
                context.DrawRectangle(Brushes.White, null, new Rect(0, 0, pixelWidth, pixelHeight));
                var brush = new VisualBrush(viewport)
                {
                    ViewboxUnits = BrushMappingMode.Absolute,
                    Viewbox = new Rect(0, 0, width, height),
                    Stretch = Stretch.Fill
                };
                context.DrawRectangle(brush, null, new Rect(0, 0, pixelWidth, pixelHeight));
            }
            var bitmap = new RenderTargetBitmap(pixelWidth, pixelHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(drawing);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                return JsonConvert.SerializeObject(new
                {
                    type = EnvelopeType, width = pixelWidth, height = pixelHeight,
                    description = "Visible workspace viewport at the current zoom and scroll position.",
                    imageDataUri = "data:image/png;base64," + Convert.ToBase64String(stream.ToArray())
                });
            }
        }

        /// <summary>Recognizes bounded, locally generated screenshot envelopes.</summary>
        internal static bool TryParse(string content, out JObject screenshot)
        {
            screenshot = null;
            if (string.IsNullOrEmpty(content) || content.Length > 8 * 1024 * 1024 ||
                content.IndexOf(EnvelopeType, StringComparison.Ordinal) < 0) return false;
            try
            {
                JObject parsed = JObject.Parse(content);
                string dataUri = (string)parsed["imageDataUri"];
                int width = (int?)parsed["width"] ?? 0;
                int height = (int?)parsed["height"] ?? 0;
                if ((string)parsed["type"] != EnvelopeType ||
                    dataUri == null || !dataUri.StartsWith("data:image/png;base64,", StringComparison.Ordinal) ||
                    width < 1 || height < 1 || width > 1024 || height > 1024) return false;
                screenshot = parsed;
                return true;
            }
            catch (JsonException) { return false; }
            catch (FormatException) { return false; }
            catch (OverflowException) { return false; }
            catch (InvalidCastException) { return false; }
            catch (ArgumentException) { return false; }
        }

        /// <summary>
        /// Replaces screenshot tool envelopes with compact metadata and appends image inputs
        /// only after every adjacent tool result, preserving OpenAI tool-call ordering.
        /// The original history is untouched so saved conversations retain the image.
        /// </summary>
        internal static string ExpandRequest(string json)
        {
            if (string.IsNullOrEmpty(json) || json.IndexOf(EnvelopeType, StringComparison.Ordinal) < 0) return json;
            JObject request = JObject.Parse(json);
            if (!(request["messages"] is JArray messages)) return json;
            var expanded = new JArray();
            var pendingImages = new List<JObject>();
            bool changed = false;
            foreach (JObject message in messages)
            {
                bool isTool = (string)message["role"] == "tool";
                if (!isTool) AppendImages(expanded, pendingImages);
                expanded.Add(message);
                if (isTool && message["content"]?.Type == JTokenType.String &&
                    TryParse((string)message["content"], out JObject screenshot))
                {
                    pendingImages.Add(screenshot);
                    message["content"] = JsonConvert.SerializeObject(new
                    {
                        width = (int)screenshot["width"], height = (int)screenshot["height"],
                        description = "Workspace screenshot follows as an image input. Requires a model with vision support."
                    });
                    changed = true;
                }
            }
            AppendImages(expanded, pendingImages);
            if (!changed) return json;
            request["messages"] = expanded;
            return request.ToString(Formatting.None);
        }

        private static void AppendImages(JArray messages, List<JObject> screenshots)
        {
            if (screenshots.Count == 0) return;
            var content = new JArray(new JObject
            {
                ["type"] = "text", ["text"] = "Workspace screenshots returned by the preceding tools. Inspect the visible components, connectors and layout."
            });
            foreach (JObject screenshot in screenshots)
            {
                content.Add(new JObject
                {
                    ["type"] = "image_url",
                    ["image_url"] = new JObject { ["url"] = screenshot["imageDataUri"], ["detail"] = "high" }
                });
            }
            messages.Add(new JObject { ["role"] = "user", ["content"] = content });
            screenshots.Clear();
        }
    }
}
