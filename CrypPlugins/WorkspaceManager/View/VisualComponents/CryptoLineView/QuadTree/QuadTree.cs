/*                              
   Copyright 208 Michael Coyle 

   Licensed under the Code Project Open License (CPOL) 1.02
*/

using System.Collections.Generic;
using System.Drawing;

namespace QuadTreeLib
{
    /// <summary>
    /// A Quadtree is a structure designed to partition space so
    /// that it's faster to find out what is inside or outside a given 
    /// area. See http://en.wikipedia.org/wiki/Quadtree
    /// This QuadTree contains items that have an area (RectangleF)
    /// it will store a reference to the item in the quad 
    /// that is just big enough to hold it. Each quad has a bucket that 
    /// contain multiple items.
    /// </summary>
    public class QuadTree<T> where T : IHasRect
    {
        /// <summary>
        /// The root QuadTreeNode
        /// </summary>
        private readonly QuadTreeNode<T> m_root;

        /// <summary>
        /// The bounds of this QuadTree
        /// </summary>
        private RectangleF m_rectangle;

        /// <summary>
        /// An delegate that performs an action on a QuadTreeNode
        /// </summary>
        /// <param name="obj"></param>
        public delegate void QTAction(QuadTreeNode<T> obj);

        /// <summary>
        /// Constructs a Quadtree from -100000,-100000 to 100000, 100000
        /// </summary>
        public QuadTree() : this(new RectangleF(-100000, -100000, 200000, 200000))
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rectangle"></param>
        public QuadTree(RectangleF rectangle)
        {
            m_rectangle = rectangle;
            m_root = new QuadTreeNode<T>(m_rectangle);
        }

        /// <summary>
        /// Get the count of items in the QuadTree
        /// </summary>
        public int Count => m_root.Count;

        /// <summary>
        /// Insert the feature into the QuadTree
        /// </summary>
        /// <param name="item"></param>
        public void Insert(T item)
        {
            m_root.Insert(item);
        }

        /// <summary>
        /// Query the QuadTree, returning the items that are in the given area
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        public List<T> Query(RectangleF area)
        {
            return m_root.Query(area);
        }

        /// <summary>
        /// Do the specified action for each item in the quadtree
        /// </summary>
        /// <param name="action"></param>
        public void ForEach(QTAction action)
        {
            m_root.ForEach(action);
        }


        internal bool QueryAny(RectangleF queryRect)
        {
            return m_root.QueryAny(queryRect);
        }
    }

}
