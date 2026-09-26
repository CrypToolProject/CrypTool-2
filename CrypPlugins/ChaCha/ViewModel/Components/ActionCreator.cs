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
using System.Collections.Generic;
using System.Linq;

namespace CrypTool.Plugins.ChaCha.ViewModel.Components
{
    public class ActionCreator : IActionCreator
    {
        /// <summary>
        /// Data structure to store sequences.
        /// </summary>
        private class Sequence : Stack<Action>, IEnumerable<Action>
        {
            public Action AggregatedAction
            {
                get
                {
                    if (Count == 0)
                    {
                        return () => { };
                    }
                    // Reverse to traverse the stack from bottom up
                    return this.Reverse().Aggregate((Action acc, Action curr) => curr.Extend(acc));
                }
            }
        }

        private Action BaseAction => Sequences
                .Select(s => s.AggregatedAction)
                // Reverse such that the aggregated action of the latest sequence comes indeed last due to reversed stack iteration order
                .Reverse()
                .Aggregate((Action acc, Action curr) => curr.Extend(acc));

        private Stack<Sequence> Sequences { get; set; } = new Stack<Sequence>();

        private Sequence CurrentSequence => Sequences.Peek();

        public void StartSequence()
        {
            Sequences.Push(new Sequence());
        }

        public void EndSequence()
        {
            if (Sequences.Count == 0)
            {
                throw new InvalidOperationException("Cannot end sequence because there is none.");
            }

            Sequences.Pop();
        }

        public Action Pop()
        {
            return CurrentSequence.Pop();
        }

        public void ReplaceLast(Action action)
        {
            if (CurrentSequence.Count > 0)
            {
                Pop();
            }

            Action extendedAction = action.Extend(BaseAction);
            CurrentSequence.Push(action);
        }

        public void ExtendLast(Action action)
        {
            if (CurrentSequence.Count == 0)
            {
                throw new InvalidOperationException("Cannot extend last action because there is no sequence. Please call `StartSequence` first.");
            }

            Action last = Pop();
            CurrentSequence.Push(last.Extend(action));
        }

        public Action Sequential(Action action)
        {
            if (Sequences.Count == 0)
            {
                throw new InvalidOperationException("Cannot create sequential action because there has no sequence been started. Please call `StartSequence` first.");
            }

            Action extendedAction = action.Extend(BaseAction);
            CurrentSequence.Push(action);
            return extendedAction;
        }
    }

    internal static class ActionExtensions
    {
        /// <summary>
        /// Extension method to extend actions.
        /// The last parameter is the action from which we want to derive a new, extended action.
        /// This is why we call it first in the new returned action.
        /// </summary>
        public static Action Extend(this Action action, Action toExtend)
        {
            return () =>
            {
                toExtend.Invoke();
                action.Invoke();
            };
        }

        /// <summary>
        /// Syntactic sugar for Extend chaining.
        /// In other words: Action.Extend(x, y) is equivalent to Action.Extend(x).Extend(y)
        /// </summary>
        public static Action Extend(this Action action, params Action[] toExtend)
        {
            return toExtend.Aggregate(action, (acc, curr) => acc.Extend(curr));
        }
    }
}