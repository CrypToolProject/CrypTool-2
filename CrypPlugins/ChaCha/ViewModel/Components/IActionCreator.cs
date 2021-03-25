using System;

namespace CrypTool.Plugins.ChaCha.ViewModel.Components
{
    /// <summary>
    /// Interface to streamline the development process of action creation.
    ///
    /// Since we are using a centralized navigation system, we need to define the complete page state in every action.
    /// This interface documents and defines helper methods for a high-level action creation API.
    ///
    /// The basic idea behind it is to define "sequences" of actions.
    /// For example, consider this code:
    /// <code>
    ///   > ActionCreator ac = new ActionCreator(); <br/>
    ///   > ac.StartSequence(); <br/>
    ///   > var a1 = ac.CreateAction(() => Console.WriteLine("a1")); <br/>
    ///   > var b1 = ac.CreateAction(() => Console.WriteLine("b1")); <br/>
    ///   > ac.StartSequence(); <br/>
    ///   > var a2 = ac.CreateAction(() => Console.WriteLine("a2")); <br/>
    ///   > var b2 = ac.CreateAction(() => Console.WriteLine("b2")); <br/>
    ///   > ac.EndSequence(); <br/>
    ///   > var d1 = ac.CreateAction(() => Console.WriteLine("c1")); <br/>
    ///   > ac.EndSequence(); <br/>
    ///   > a1(); b1(); a2(); b2(); c2(); <br/>
    /// </code>
    /// <br/>
    /// This will output:
    /// <code>
    ///   > a1 <br/>
    ///   > a1 b1 <br/>
    ///   > a1 b1 a2 <br/>
    ///   > a1 b1 a2 b2 <br/>
    ///   > a1 b1 c1 <br/>
    /// </code>
    /// <br/>
    /// As we can see, when we start a sequence all following actions will contain all previous actions from all sequences and we can nest sequences inside other sequences.
    /// This works by a concept which I called "extending". If Action A extends Action b, this means that Action A contains all statements from Action b or in other words, A is a superset of b.
    /// </summary>

    internal interface IActionCreator
    {
        /// <summary>
        /// Start a new sequence. All following actions using `Sequential` will be a superset of the previous ones.
        /// </summary>
        void StartSequence();

        /// <summary>
        /// End the latest sequence. All following actions using `Sequential` will no longer be a superset of the previous ones in the latest sequence.
        /// </summary>
        void EndSequence();

        /// <summary>
        /// Pop the last action from the current sequence.
        /// </summary>
        /// <returns></returns>
        Action Pop();

        /// <summary>
        /// Replace the last action from the current sequence with the given action.
        /// The action will be inserted extended.
        /// If there is no last action in the current sequence, it will NOT throw an error and just insert the action at the beginning.
        /// </summary>
        void ReplaceLast(Action action);

        /// <summary>
        /// Extend the last action from the current sequence with the given action.
        /// </summary>
        void ExtendLast(Action action);

        /// <summary>
        /// Create a extended action using all previous sequential actions.
        /// </summary>
        /// <param name="action">The action we want to extend</param>
        /// <returns>The extended action</returns>
        Action Sequential(Action action);
    }
}