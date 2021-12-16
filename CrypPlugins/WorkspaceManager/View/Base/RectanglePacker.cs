#region CPL License
/*
Nuclex Framework
Copyright (C) 2002-2011 Nuclex Development Labs

This library is free software; you can redistribute it and/or
modify it under the terms of the IBM Common Public License as
published by the IBM Corporation; either version 1.0 of the
License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
IBM Common Public License for more details.

You should have received a copy of the IBM Common Public
License along with this library
*/
#endregion
using System;
using System.Collections.Generic;
using System.Windows;

namespace WorkspaceManager.Base.Sort
{
    public abstract class RectanglePacker
    {
        /// <summary>Initializes a new rectangle packer</summary>
        /// <param name="packingAreaWidth">Width of the packing area</param>
        /// <param name="packingAreaHeight">Height of the packing area</param>
        protected RectanglePacker(double packingAreaWidth, double packingAreaHeight)
        {
            this.packingAreaWidth = packingAreaWidth;
            this.packingAreaHeight = packingAreaHeight;
        }

        /// <summary>Allocates space for a rectangle in the packing area</summary>
        /// <param name="rectangleWidth">Width of the rectangle to allocate</param>
        /// <param name="rectangleHeight">Height of the rectangle to allocate</param>
        /// <returns>The location at which the rectangle has been placed</returns>
        public virtual Point Pack(double rectangleWidth, double rectangleHeight)
        {

            if (!TryPack(rectangleWidth, rectangleHeight, out Point point))
            {
                throw new Exception("Rectangle does not fit in packing area");
            }

            return point;
        }

        /// <summary>Tries to allocate space for a rectangle in the packing area</summary>
        /// <param name="rectangleWidth">Width of the rectangle to allocate</param>
        /// <param name="rectangleHeight">Height of the rectangle to allocate</param>
        /// <param name="placement">Output parameter receiving the rectangle's placement</param>
        /// <returns>True if space for the rectangle could be allocated</returns>
        public abstract bool TryPack(
          double rectangleWidth, double rectangleHeight, out Point placement
        );

        /// <summary>Maximum width the packing area is allowed to have</summary>
        protected double PackingAreaWidth => packingAreaWidth;

        /// <summary>Maximum height the packing area is allowed to have</summary>
        protected double PackingAreaHeight => packingAreaHeight;

        /// <summary>Maximum allowed width of the packing area</summary>
        private readonly double packingAreaWidth;
        /// <summary>Maximum allowed height of the packing area</summary>
        private readonly double packingAreaHeight;

    }

    public class ArevaloRectanglePacker : RectanglePacker
    {

        #region class AnchorRankComparer

        /// <summary>Compares the 'rank' of anchoring points</summary>
        /// <remarks>
        ///   Anchoring points are potential locations for the placement of new rectangles.
        ///   Each time a rectangle is inserted, an anchor point is generated on its upper
        ///   right end and another one at its lower left end. The anchor points are kept
        ///   in a list that is ordered by their closeness to the upper left corner of the
        ///   packing area (their 'rank') so the packer favors positions that are closer to
        ///   the upper left for new rectangles.
        /// </remarks>
        private class AnchorRankComparer : IComparer<Point>
        {

            /// <summary>Provides a default instance for the anchor rank comparer</summary>
            public static AnchorRankComparer Default = new AnchorRankComparer();

            /// <summary>Compares the rank of two anchors against each other</summary>
            /// <param name="left">Left anchor point that will be compared</param>
            /// <param name="right">Right anchor point that will be compared</param>
            /// <returns>The relation of the two anchor point's ranks to each other</returns>
            public int Compare(Point left, Point right)
            {
                //return Math.Min(left.X, left.Y) - Math.Min(right.X, right.Y);
                return Convert.ToInt32((left.X + left.Y) - (right.X + right.Y));
            }

        }

        #endregion

        /// <summary>Initializes a new rectangle packer</summary>
        /// <param name="packingAreaWidth">Maximum width of the packing area</param>
        /// <param name="packingAreaHeight">Maximum height of the packing area</param>
        public ArevaloRectanglePacker(double packingAreaWidth, double packingAreaHeight) :
            base(packingAreaWidth, packingAreaHeight)
        {

            packedRectangles = new List<Rect>();
            anchors = new List<Point>
            {
                new Point(0, 0)
            };

            actualPackingAreaWidth = 1;
            actualPackingAreaHeight = 1;
        }

        /// <summary>Tries to allocate space for a rectangle in the packing area</summary>
        /// <param name="rectangleWidth">Width of the rectangle to allocate</param>
        /// <param name="rectangleHeight">Height of the rectangle to allocate</param>
        /// <param name="placement">Output parameter receiving the rectangle's placement</param>
        /// <returns>True if space for the rectangle could be allocated</returns>
        public override bool TryPack(
          double rectangleWidth, double rectangleHeight, out Point placement)
        {

            // Try to find an anchor where the rectangle fits in, enlarging the packing
            // area and repeating the search recursively until it fits or the
            // maximum allowed size is exceeded.
            int anchorIndex = selectAnchorRecursive(
              rectangleWidth, rectangleHeight,
              actualPackingAreaWidth, actualPackingAreaHeight
            );

            // No anchor could be found at which the rectangle did fit in
            if (anchorIndex == -1)
            {
                placement = new Point(0, 0);
                return false;
            }

            placement = anchors[anchorIndex];

            // Move the rectangle either to the left or to the top until it collides with
            // a neightbouring rectangle. This is done to combat the effect of lining up
            // rectangles with gaps to the left or top of them because the anchor that
            // would allow placement there has been blocked by another rectangle
            optimizePlacement(ref placement, rectangleWidth, rectangleHeight);

            // Remove the used anchor and add new anchors at the upper right and lower left
            // positions of the new rectangle
            {
                // The anchor is only removed if the placement optimization didn't
                // move the rectangle so far that the anchor isn't blocked anymore
                bool blocksAnchor =
                  ((placement.X + rectangleWidth) > anchors[anchorIndex].X) &&
                  ((placement.Y + rectangleHeight) > anchors[anchorIndex].Y);

                if (blocksAnchor)
                {
                    anchors.RemoveAt(anchorIndex);
                }

                // Add new anchors at the upper right and lower left coordinates of the rectangle
                insertAnchor(new Point(placement.X + rectangleWidth, placement.Y));
                insertAnchor(new Point(placement.X, placement.Y + rectangleHeight));
            }

            // Finally, we can add the rectangle to our packed rectangles list
            packedRectangles.Add(
              new Rect(placement.X, placement.Y, rectangleWidth, rectangleHeight)
            );

            return true;

        }

        /// <summary>
        ///   Optimizes the rectangle's placement by moving it either left or up to fill
        ///   any gaps resulting from rectangles blocking the anchors of the most optimal
        ///   placements.
        /// </summary>
        /// <param name="placement">Placement to be optimized</param>
        /// <param name="rectangleWidth">Width of the rectangle to be optimized</param>
        /// <param name="rectangleHeight">Height of the rectangle to be optimized</param>
        private void optimizePlacement(ref Point placement, double rectangleWidth, double rectangleHeight)
        {
            Rect rectangle = new Rect
            (
              placement.X, placement.Y, rectangleWidth, rectangleHeight
            );

            // Try to move the rectangle to the left as far as possible
            double leftMost = placement.X;
            while (isFree(ref rectangle, PackingAreaWidth, PackingAreaHeight))
            {
                leftMost = rectangle.X;
                --rectangle.X;
            }

            // Reset rectangle to original position
            rectangle.X = placement.X;

            // Try to move the rectangle upwards as far as possible
            double topMost = placement.Y;
            while (isFree(ref rectangle, PackingAreaWidth, PackingAreaHeight))
            {
                topMost = rectangle.Y;
                --rectangle.Y;
            }

            // Use the dimension in which the rectangle could be moved farther
            if ((placement.X - leftMost) > (placement.Y - topMost))
            {
                placement.X = leftMost;
            }
            else
            {
                placement.Y = topMost;
            }
        }

        /// <summary>
        ///   Searches for a free anchor and recursively enlarges the packing area
        ///   if none can be found.
        /// </summary>
        /// <param name="rectangleWidth">Width of the rectangle to be placed</param>
        /// <param name="rectangleHeight">Height of the rectangle to be placed</param>
        /// <param name="testedPackingAreaWidth">Width of the tested packing area</param>
        /// <param name="testedPackingAreaHeight">Height of the tested packing area</param>
        /// <returns>
        ///   Index of the anchor the rectangle is to be placed at or -1 if the rectangle
        ///   does not fit in the packing area anymore.
        /// </returns>
        private int selectAnchorRecursive(
          double rectangleWidth, double rectangleHeight,
          double testedPackingAreaWidth, double testedPackingAreaHeight
        )
        {

            // Try to locate an anchor point where the rectangle fits in
            int freeAnchorIndex = findFirstFreeAnchor(
              rectangleWidth, rectangleHeight, testedPackingAreaWidth, testedPackingAreaHeight
            );

            // If a the rectangle fits without resizing packing area (any further in case
            // of a recursive call), take over the new packing area size and return the
            // anchor at which the rectangle can be placed.
            if (freeAnchorIndex != -1)
            {
                actualPackingAreaWidth = testedPackingAreaWidth;
                actualPackingAreaHeight = testedPackingAreaHeight;

                return freeAnchorIndex;
            }

            //
            // If we reach this point, the rectangle did not fit in the current packing
            // area and our only choice is to try and enlarge the packing area.
            //

            // For readability, determine whether the packing area can be enlarged
            // any further in its width and in its height
            bool canEnlargeWidth = (testedPackingAreaWidth < PackingAreaWidth);
            bool canEnlargeHeight = (testedPackingAreaHeight < PackingAreaHeight);
            bool shouldEnlargeHeight =
              (!canEnlargeWidth) ||
              (testedPackingAreaHeight < testedPackingAreaWidth);

            // Try to enlarge the smaller of the two dimensions first (unless the smaller
            // dimension is already at its maximum size). 'shouldEnlargeHeight' is true
            // when the height was the smaller dimension or when the width is maxed out.
            if (canEnlargeHeight && shouldEnlargeHeight)
            {

                // Try to double the height of the packing area
                return selectAnchorRecursive(
                  rectangleWidth, rectangleHeight,
                  testedPackingAreaWidth, Math.Min(testedPackingAreaHeight * 2, PackingAreaHeight)
                );

            }
            else if (canEnlargeWidth)
            {

                // Try to double the width of the packing area
                return selectAnchorRecursive(
                  rectangleWidth, rectangleHeight,
                  Math.Min(testedPackingAreaWidth * 2, PackingAreaWidth), testedPackingAreaHeight
                );

            }
            else
            {

                // Both dimensions are at their maximum sizes and the rectangle still
                // didn't fit. We give up!
                return -1;

            }

        }

        /// <summary>Locates the first free anchor at which the rectangle fits</summary>
        /// <param name="rectangleWidth">Width of the rectangle to be placed</param>
        /// <param name="rectangleHeight">Height of the rectangle to be placed</param>
        /// <param name="testedPackingAreaWidth">Total width of the packing area</param>
        /// <param name="testedPackingAreaHeight">Total height of the packing area</param>
        /// <returns>The index of the first free anchor or -1 if none is found</returns>
        private int findFirstFreeAnchor(
          double rectangleWidth, double rectangleHeight,
          double testedPackingAreaWidth, double testedPackingAreaHeight
        )
        {
            Rect potentialLocation = new Rect(
              0, 0, rectangleWidth, rectangleHeight
            );

            // Walk over all anchors (which are ordered by their distance to the
            // upper left corner of the packing area) until one is discovered that
            // can house the new rectangle.
            for (int index = 0; index < anchors.Count; ++index)
            {
                potentialLocation.X = anchors[index].X;
                potentialLocation.Y = anchors[index].Y;

                // See if the rectangle would fit in at this anchor point
                if (isFree(ref potentialLocation, testedPackingAreaWidth, testedPackingAreaHeight))
                {
                    return index;
                }
            }

            // No anchor points were found where the rectangle would fit in
            return -1;
        }

        /// <summary>
        ///   Determines whether the rectangle can be placed in the packing area
        ///   at its current location.
        /// </summary>
        /// <param name="rectangle">Rectangle whose position to check</param>
        /// <param name="testedPackingAreaWidth">Total width of the packing area</param>
        /// <param name="testedPackingAreaHeight">Total height of the packing area</param>
        /// <returns>True if the rectangle can be placed at its current position</returns>
        private bool isFree(ref Rect rectangle, double testedPackingAreaWidth, double testedPackingAreaHeight)
        {

            // If the rectangle is partially or completely outside of the packing
            // area, it can't be placed at its current location
            bool leavesPackingArea =
              (rectangle.X < 0) ||
              (rectangle.Y < 0) ||
              (rectangle.Right > testedPackingAreaWidth) ||
              (rectangle.Bottom > testedPackingAreaHeight);

            if (leavesPackingArea)
            {
                return false;
            }

            rectangle.X += 1;
            rectangle.Y += 1;

            // Brute-force search whether the rectangle touches any of the other
            // rectangles already in the packing area
            for (int index = 0; index < packedRectangles.Count; ++index)
            {

                if (packedRectangles[index].IntersectsWith(rectangle))
                {
                    return false;
                }
            }

            // Success! The rectangle is inside the packing area and doesn't overlap
            // with any other rectangles that have already been packed.
            return true;

        }

        /// <summary>Inserts a new anchor point into the anchor list</summary>
        /// <param name="anchor">Anchor point that will be inserted</param>
        /// <remarks>
        ///   This method tries to keep the anchor list ordered by ranking the anchors
        ///   depending on the distance from the top left corner in the packing area.
        /// </remarks>
        private void insertAnchor(Point anchor)
        {

            // Find out where to insert the new anchor based on its rank (which is
            // calculated based on the anchor's distance to the top left corner of
            // the packing area).
            //
            // From MSDN on BinarySearch():
            //   "If the List does not contain the specified value, the method returns
            //    a negative integer. You can apply the bitwise complement operation (~) to
            //    this negative integer to get the index of the first element that is
            //    larger than the search value."
            int insertIndex = anchors.BinarySearch(anchor, AnchorRankComparer.Default);
            if (insertIndex < 0)
            {
                insertIndex = ~insertIndex;
            }

            // Insert the anchor at the index matching its rank
            anchors.Insert(insertIndex, anchor);

        }

        /// <summary>Current width of the packing area</summary>
        private double actualPackingAreaWidth;
        /// <summary>Current height of the packing area</summary>
        private double actualPackingAreaHeight;
        /// <summary>Rectangles contained in the packing area</summary>
        private readonly List<Rect> packedRectangles;
        /// <summary>Anchoring points where new rectangles can potentially be placed</summary>
        private readonly List<Point> anchors;

    }

} // namespace Nuclex.Game.Packing
