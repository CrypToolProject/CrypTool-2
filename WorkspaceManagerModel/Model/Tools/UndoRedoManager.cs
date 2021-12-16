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
            }
        }

        /// <summary>
        /// Clears undo and redo stacks
        /// </summary>
        public void ClearStacks()
        {
            _undoStack.Clear();
            _redoStack.Clear();
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
