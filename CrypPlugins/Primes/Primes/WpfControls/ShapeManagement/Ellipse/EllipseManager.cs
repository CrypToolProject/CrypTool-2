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

using Primes.Library.FactorTree;
using System.Collections.Generic;

namespace Primes.WpfControls.ShapeManagement.Ellipse
{
    public class EllipseManager
    {
        private readonly int m_ItemSize = 30;

        public int ItemSize => m_ItemSize;

        private double m_Width;

        public double Width
        {
            get => m_Width;
            set => m_Width = value;
        }

        private double m_Height;

        public double Height
        {
            get => m_Height;
            set => m_Height = value;
        }

        private GmpFactorTree m_FactorTree = null;

        public EllipseManager() { }

        public GmpFactorTree FactorTree
        {
            get => m_FactorTree;
            set { m_FactorTree = value; CalculateFactorTree(); }
        }

        private IDictionary<GmpFactorTreeNode, EllipseItem> m_EllipseItems = null;

        public IDictionary<GmpFactorTreeNode, EllipseItem> EllipseItems => m_EllipseItems;

        private void CalculateFactorTree()
        {
            if (m_FactorTree != null)
            {
                if (m_EllipseItems == null)
                {
                    m_EllipseItems = new Dictionary<GmpFactorTreeNode, EllipseItem>();
                }

                GmpFactorTreeNode root = FactorTree.Root;

                double y = ItemSize / 2;
                double xRoot = 80;

                EllipseItem item;
                if (m_EllipseItems.ContainsKey(root))
                {
                    item = m_EllipseItems[root];
                }
                else
                {
                    item = CreateEllipseItem(root, root.Value, ItemSize, ItemSize, xRoot, y);
                }

                item.IsRoot = true;
                EllipseItem parent = item;
                item.IsPrime = root.IsPrime;
                GmpFactorTreeNode node = root.LeftChild;
                double xleft = 10;
                double xright = 100;
                bool even = true;

                while (node != null)
                {
                    EllipseItem i = null;
                    if (m_EllipseItems.ContainsKey(node))
                    {
                        i = m_EllipseItems[node];
                    }
                    else
                    {
                        i = new EllipseItem(node);
                    }

                    i.Parent = parent;
                    i.Allign = EllipseAllign.Right;
                    double xItem = xright;
                    if (even)
                    {
                        i.Allign = EllipseAllign.Left;
                        xItem = xleft;
                        y += ItemSize + item.Height / 2;
                    }
                    else
                    {
                        //y += ItemSize/2;
                    }
                    i.Value = node.Value;
                    i.Height = ItemSize;
                    i.Width = ItemSize;
                    i.X = xItem;
                    i.Y = y;
                    i.IsPrime = node.IsPrime;

                    if (!m_EllipseItems.ContainsKey(node))
                    {
                        m_EllipseItems.Add(node, i);
                    }
                    even = !even;
                    xright += 10;
                    xleft += 10;

                    if (node.RightSibling != null)
                    {
                        i.IsPrime = true;
                        node = node.RightSibling;
                        continue;
                    }

                    parent = i;
                    node = node.LeftChild;
                }
                if (!m_EllipseItems.ContainsKey(root))
                {
                    m_EllipseItems.Add(root, item);
                }
            }
        }

        private EllipseItem CreateEllipseItem(GmpFactorTreeNode node, object value, double width, double height, double x, double y)
        {
            EllipseItem result = new EllipseItem(node)
            {
                Value = value,
                Width = width,
                Height = height,
                Y = y,
                X = x
            };
            return result;
        }

        public void Clear()
        {
            if (m_EllipseItems != null)
            {
                m_EllipseItems.Clear();
            }

            if (m_FactorTree != null)
            {
                m_FactorTree.Clear();
            }
        }
    }
}
