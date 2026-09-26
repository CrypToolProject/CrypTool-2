/*
   Copyright (C) CrypTool 2 Team

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
using System;
using System.Numerics;
using KeySearcher.Helper;
using KeySearcher.P2P.Exceptions;
using KeySearcher.P2P.Storage;
using KeySearcher.Properties;

namespace KeySearcher.P2P.Tree
{
    class Node : NodeBase
    {
        internal bool LeftChildFinished;
        internal bool RightChildFinished;
        internal bool LeftChildIntegrated = false;
        internal bool RightChildIntegrated = false;

        internal NodeBase leftChild;
        internal NodeBase rightChild;
        internal bool leftChildReserved;
        internal bool rightChildReserved;

        public Node(KeyQualityHelper keyQualityHelper, Node parentNode, BigInteger @from, BigInteger to, string distributedJobIdentifier)
            : base(keyQualityHelper, parentNode, @from, to, distributedJobIdentifier)
        {
        }

        private void LoadOrUpdateChildNodes()
        {
           
        }

        public override bool IsCalculated()
        {
            return LeftChildFinished && RightChildFinished;
        }

        public override void Reset()
        {
            leftChild = null;
            rightChild = null;
            LeftChildFinished = false;
            RightChildFinished = false;
            LeftChildIntegrated = false;
            RightChildIntegrated = false;
            Result.Clear();
            Activity.Clear();
            UpdateCache(); 
        }

        public void ClearChildsLocal()
        {
            leftChild = null;
            rightChild = null;
        }

        public override void UpdateCache()
        {
            LoadOrUpdateChildNodes();

            UpdateChildrenReservationIndicators();
        }

        private void UpdateChildrenReservationIndicators()
        {
            leftChildReserved = LeftChildFinished || leftChild.IsReserved();
            rightChildReserved = RightChildFinished || (rightChild != null && rightChild.IsReserved());
        }

        public override Leaf CalculatableLeaf(bool useReservedNodes)
        {
            // Left child not finished and not reserved (or reserved leafs are allowed)
            if (!LeftChildFinished && (!leftChildReserved || useReservedNodes))
            {
                return leftChild.CalculatableLeaf(useReservedNodes);
            }

            if (rightChild == null)
            {
                Reset();
                return leftChild.CalculatableLeaf(useReservedNodes);
            }

            return rightChild.CalculatableLeaf(useReservedNodes);
        }

        public void ChildFinished(NodeBase childNode)
        {
            if (childNode == leftChild)
            {
                LeftChildFinished = true;
                leftChild = null;
                UpdateCache();
                return;
            }

            if (childNode == rightChild)
            {
                RightChildFinished = true;
                rightChild = null;
                UpdateCache();
                return;
            }
        }

        public override bool IsReserved()
        {
            if (LeftChildFinished && !RightChildFinished)
            {
                return rightChildReserved;
            }

            if (!LeftChildFinished && RightChildFinished)
            {
                return leftChildReserved;
            }

            if (!LeftChildFinished && !RightChildFinished && rightChildReserved)
            {
                return leftChildReserved;
            }

            return rightChildReserved;
        }

        public override string ToString()
        {
            return base.ToString() + Resources.__LeftChildFinished_ + LeftChildFinished + Resources.___RightChildFinished_ +
                   RightChildFinished;
        }

        //Updates also the right children (if necessary)
        public void UpdateAll()
        {
             
        }
    }
}
