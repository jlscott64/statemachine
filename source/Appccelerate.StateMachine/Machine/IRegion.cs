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

using System;
using System.Collections.Generic;
using Appccelerate.StateMachine.Machine.ActionHolders;

namespace Appccelerate.StateMachine.Machine
{
    /// <summary>
    /// A region in a state of the state machine.
    /// A state with more than one region is an "orthogonal" state
    /// in UML terminology.  Each region can be active simultaneously
    /// meaning that the state can have more than one current substate
    /// at a time.
    /// </summary>
    /// <typeparam name="TState">The type of the state id.</typeparam>
    /// <typeparam name="TEvent">The type of the event id.</typeparam>
    public interface IRegion<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        /// <summary>
        /// Gets or sets the initial sub-state.
        /// </summary>
        /// <value>The initial sub-state.</value>
        IState<TState, TEvent> InitialState { get; set; }

        /// <summary>
        /// Gets the state to which this region belongs.
        /// </summary>
        /// <value>The owning state of this region.</value>
        IState<TState, TEvent> Owner { get; }

        /// <summary>
        /// Gets the sub-states.
        /// </summary>
        /// <value>The sub-states.</value>
        ICollection<IState<TState, TEvent>> SubStates { get; }

        /// <summary>
        /// Gets the transitions.
        /// </summary>
        /// <value>The transitions.</value>
        ITransitionDictionary<TState, TEvent> Transitions { get; }

        /// <summary>
        /// Gets or sets the last active state of this state.
        /// </summary>
        /// <value>The last state of the active.</value>
        IState<TState, TEvent> LastActiveState { get; set; }

        /// <summary>
        /// Fires the specified event id on this state.
        /// </summary>
        /// <param name="context">The event context.</param>
        /// <returns>The result of the transition.</returns>
        ITransitionResult<TState, TEvent> Fire(ITransitionContext<TState, TEvent> context);

        void AddSubState(IState<TState, TEvent> subState);
    }
}