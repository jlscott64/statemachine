//-------------------------------------------------------------------------------
// <copyright file="IState.cs" company="Appccelerate">
//   Copyright (c) 2008-2013
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
//-------------------------------------------------------------------------------


namespace Appccelerate.StateMachine.Machine
{
    using System;
    using System.Collections.Generic;
    using Appccelerate.StateMachine.Machine.States;
    using Appccelerate.StateMachine.Machine.ActionHolders;
    
    /// <summary>
    /// Represents a state of the state machine.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public interface IState<TState, TEvent> where TState : IComparable
        where TEvent : IComparable
    {
        /// <summary>
        /// Gets the id of this state.
        /// </summary>
        /// <value>The id of this state.</value>
        TState Id { get; }

        /// <summary>
        /// DEPRECATED. Gets the initial sub-state. Null if this state has no sub-states.
        /// </summary>
        /// <returns>The initial sub-state. Null if this state has no sub-states.</returns>
        IState<TState, TEvent> GetInitialState();

        /// <summary>
        /// Gets the initial sub-states. Empty if this state has no sub-states.
        /// </summary>
        /// <value>The initial sub-states. Empty if this state has no sub-states.</value>
        IEnumerable<IState<TState, TEvent>> InitialStates { get; }

        /// <summary>
        /// Gets or sets the super-state. Null if this is a root state.
        /// </summary>
        /// <value>The super-state.</value>
        IState<TState, TEvent> SuperState { get; set; }

        /// <summary>
        /// Gets or sets the region the state belongs to. Null if this is a simple state.
        /// </summary>
        /// <value>The region.</value>
        IRegion<TState, TEvent> Region { get; set; }

        /// <summary>
        /// Gets the sub-states.
        /// </summary>
        /// <value>The sub-states.</value>
        IEnumerable<IState<TState, TEvent>> SubStates { get; }

        /// <summary>
        /// Gets the transitions.
        /// </summary>
        /// <value>The transitions.</value>
        ITransitionDictionary<TState, TEvent> Transitions { get; }

        /// <summary>
        /// Gets or sets the level in the hierarchy.
        /// </summary>
        /// <value>The level in the hierarchy.</value>
        int Level { get; set; }

        /// <summary>
        /// Gets the current active states of this state.
        /// </summary>
        /// <value>The current active states.</value>
        IEnumerable<IState<TState, TEvent>> ActiveStates { get; }

        /// <summary>
        /// Gets or sets the current active state of this state.
        /// </summary>
        /// <value>The current state of the active.</value>
        IState<TState, TEvent> ActiveState { get; set; }

        /// <summary>
        /// Gets the current active states of this state.
        /// </summary>
        /// <value>The last active states.</value>
        IEnumerable<IState<TState, TEvent>> LastActiveStates { get; }

        /// <summary>
        /// Gets or sets the last active state of this state.
        /// </summary>
        /// <value>The last state of the active.</value>
        IState<TState, TEvent> LastActiveState { get; set; }

        /// <summary>
        /// Gets the entry actions.
        /// </summary>
        /// <value>The entry actions.</value>
        IList<IActionHolder> EntryActions { get; }

        /// <summary>
        /// Gets the exit actions.
        /// </summary>
        /// <value>The exit actions.</value>
        IList<IActionHolder> ExitActions { get; }

        /// <summary>
        /// Gets or sets the history type of this state.
        /// </summary>
        /// <value>The type of the history.</value>
        HistoryType HistoryType { get; set; }

        /// <summary>
        /// Give the event context, returns the transition to be fired by this state.
        /// </summary>
        /// <param name="context">The event context.</param>
        /// <returns>The transition to be fired or null.</returns>
        ITransition<TState, TEvent> GetTransitionToFire(ITransitionContext<TState, TEvent> context);

        void Entry(ITransitionContext<TState, TEvent> context);

        void Exit(ITransitionContext<TState, TEvent> context);

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        string ToString();

        void AddSubState(IState<TState, TEvent> subState);

        void AddInitialState(IState<TState, TEvent> initialState);

        bool HasInitialState();
        IRegion<TState, TEvent> AddRegion();
    }
}