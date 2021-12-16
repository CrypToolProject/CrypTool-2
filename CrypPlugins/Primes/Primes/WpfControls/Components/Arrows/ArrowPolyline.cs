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

//----------------------------------------------
// ArrowPolyline.cs (c) 2007 by Charles Petzold
//----------------------------------------------

using System.Windows;
using System.Windows.Media;

namespace Primes.WpfControls.Components.Arrows
{
    /// <summary>
    ///     Draws a series of connected straight lines with
    ///     optional arrows on the ends.
    /// </summary>
    public class ArrowPolyline : ArrowLineBase
    {
        /// <summary>
        ///     Identifies the Points dependency property.
        /// </summary>
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points",
                typeof(PointCollection), typeof(ArrowPolyline),
                new FrameworkPropertyMetadata(null,
                        FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     Gets or sets a collection that contains the
        ///     vertex points of the ArrowPolyline.
        /// </summary>
        public PointCollection Points
        {
            set => SetValue(PointsProperty, value);
            get => (PointCollection)GetValue(PointsProperty);
        }

        /// <summary>
        ///     Initializes a new instance of the ArrowPolyline class.
        /// </summary>
        public ArrowPolyline()
        {
            Points = new PointCollection();
        }

        /// <summary>
        ///     Gets a value that represents the Geometry of the ArrowPolyline.
        /// </summary>
        protected override Geometry DefiningGeometry
        {
            get
            {
                // Clear out the PathGeometry.
                pathgeo.Figures.Clear();

                //// Try to avoid unnecessary indexing exceptions.
                //if (Points.Count > 0)
                //{
                //    // Define a PathFigure containing the points.
                //    pathfigLine.StartPoint = Points[0];
                //    polysegLine.Points.Clear();

                //    for (int i = 1; i < Points.Count; i++)
                //        polysegLine.Points.Add(Points[i]);

                //    pathgeo.Figures.Add(pathfigLine);
                //}

                // Call the base property to add arrows on the ends.
                return base.DefiningGeometry;
            }
        }
    }
}
