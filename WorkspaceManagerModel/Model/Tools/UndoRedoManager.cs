/*                              
   Copyright 2010 Nils Kopal

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
using System.Collections.Generic;
using System.Linq;
using WorkspaceManagerModel.Model.Operations;
using WorkspaceManagerModel.Model.Tools;

namespace WorkspaceManager.Model.Tools
{
    public class UndoRedoManager
    {
        private readonly WorkspaceModel _workspaceModel = null;
        private readonly Stack<Operation> _undoStack = new Stack<Operation>();
        private readonly Stack<Operation> _redoStack = new Stack<Operation>();
        [ThreadStatic] private static object recordingOwner;
        private readonly Dictionary<Operation, object> operationOwners = new Dictionary<Operation, object>();
        public event EventHandler HistoryChanged;
        public int Revision { get; private set; }

        /// <summary>Tag only synchronous tool edits, leaving intervening user edits independent.</summary>
        public static IDisposable PushOperationOwner(object owner)
        {
            object previous = recordingOwner;
            recordingOwner = owner;
            return new OwnerScope(() => recordingOwner = previous);
        }

        private sealed class OwnerScope : IDisposable
        {
            private Action restore;
            public OwnerScope(Action restore) { this.restore = restore; }
            public void Dispose() { Action action = restore; restore = null; action?.Invoke(); }
        }

        /// <summary>Collapse a contiguous tool-owned suffix. Never absorb manual edits or saved intermediate states.</summary>
        public Operation GroupOwnedOperations(object owner)
        {
            var owned = _undoStack.TakeWhile(op => operationOwners.TryGetValue(op, out object value) && ReferenceEquals(value, owner)).ToList();
            bool fragmented = _undoStack.Skip(owned.Count).Any(op => operationOwners.TryGetValue(op, out object value) && ReferenceEquals(value, owner));
            foreach (Operation op in operationOwners.Where(pair => ReferenceEquals(pair.Value, owner)).Select(pair => pair.Key).ToList())
                operationOwners.Remove(op);
            if (owned.Count == 0 || fragmented || owned.Skip(1).Any(op => op.SavedHere)) return null;
            foreach (Operation op in owned) _undoStack.Pop();
            bool savedHere = owned[0].SavedHere;
            owned.Reverse();
            var group = new GroupedOperation(owned) { SavedHere = savedHere };
            _undoStack.Push(group);
            HistoryChanged?.Invoke(this, EventArgs.Empty);
            return group;
        }

        public bool CanUndo(Operation expected) => !IsCurrentlyWorking && _undoStack.Count > 0 && ReferenceEquals(_undoStack.Peek(), expected);
        public bool TryUndo(Operation expected)
        {
            if (!CanUndo(expected)) return false;
            Undo();
            return true;
        }

        internal UndoRedoManager(WorkspaceModel workspaceModel)
        {
            _workspaceModel = workspaceModel;
            SettingsManager = new SettingsManager(workspaceModel);
        }

        public SettingsManager SettingsManager
        {
            get;
            private set;
        }

        /// <summary>
        /// Is an undo-operation possible?
        /// </summary>
        /// <returns></returns>
        public bool CanUndo()
        {
            return _undoStack.Count > 0;
        }

        /// <summary>
        /// Is a redo-operation possible?
        /// </summary>
        /// <returns></returns>
        public bool CanRedo()
        {
            return _redoStack.Count > 0;
        }

        /// <summary>
        /// Do undo now
        /// </summary>
        public void Undo()
        {
            if (!CanUndo())
            {
                return;
            }

            IsCurrentlyWorking = true;
            try
            {
                Operation op = _undoStack.Pop();
                op.Undo(_workspaceModel);
                _redoStack.Push(op);

                Operation nextOp = null;
                while (_undoStack.Count > 0 &&
                    op.GetType().Equals(_undoStack.Peek().GetType()) &&
                    _undoStack.Peek().Identifier == op.Identifier &&
                    (_undoStack.Peek() is MoveModelElementOperation ||
                    _undoStack.Peek() is ResizeModelElementOperation ||
                    _undoStack.Peek() is MultiOperation))
                {
                    nextOp = _undoStack.Pop();
                    _redoStack.Push(nextOp);
                }

                if (nextOp != null)
                {
                    nextOp.Undo(_workspaceModel);
                }
            }
            finally
            {
                IsCurrentlyWorking = false;
                HistoryChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Do redo now
        /// </summary>
        public void Redo()
        {
            if (!CanRedo())
            {
                return;
            }

            IsCurrentlyWorking = true;
            try
            {
                Operation op = _redoStack.Pop();
                op.Execute(_workspaceModel);
                _undoStack.Push(op);

                Operation nextOp = null;
                while (_redoStack.Count > 0 &&
                    op.GetType().Equals(_redoStack.Peek().GetType()) &&
                    _redoStack.Peek().Identifier == op.Identifier &&
                    (_redoStack.Peek() is MoveModelElementOperation ||
                    _redoStack.Peek() is ResizeModelElementOperation ||
                    _redoStack.Peek() is MultiOperation))
                {
                    nextOp = _redoStack.Pop();
                    _undoStack.Push(nextOp);
                }

                if (nextOp != null)
                {
                    nextOp.Execute(_workspaceModel);
                }
            }
            finally
            {
                IsCurrentlyWorking = false;
                HistoryChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Clears undo and redo stacks
        /// </summary>
        public void ClearStacks()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            operationOwners.Clear();
            Revision++;
            HistoryChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Tell the UndoRedoManager that an operation occured
        /// </summary>
        /// <param name="op"></param>
        public void DidOperation(Operation op)
        {
            //we do not notice any operation if we are currently working 
            //(means we undo or redo at this moment)
            if (IsCurrentlyWorking)
            {
                return;
            }

            if (_redoStack.Count > 0)
            {
                _redoStack.Clear();
            }
            _undoStack.Push(op);
            if (recordingOwner != null) operationOwners[op] = recordingOwner;
            Revision++;
            HistoryChanged?.Invoke(this, EventArgs.Empty);
        }

        internal bool SavedHere
        {
            set
            {
                foreach (Operation operation in _undoStack)
                {
                    operation.SavedHere = false;
                }
                foreach (Operation operation in _redoStack)
                {
                    operation.SavedHere = false;
                }
                if (_undoStack.Count > 0)
                {
                    _undoStack.Peek().SavedHere = value;
                }
            }
            get => _undoStack.Peek().SavedHere;
        }

        internal bool HasUnsavedChanges()
        {
            if (CanUndo() && SavedHere == false)
            {
                return true;
            }

            if (CanRedo())
            {
                return _redoStack.Any(operation => operation.SavedHere);
            }

            return false;
        }

        public bool IsCurrentlyWorking
        {
            get;
            private set;
        }
    }
}
