//-------------------------------------------------------------------------------
// <copyright file="State.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Machine.States
{
    using System;
    using System.Collections.Generic;

    public interface IRegion<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        /// <summary>
        /// The owner of this region.
        /// </summary>
        IState<TState, TEvent> Owner { get; }

        /// <summary>
        /// Collection of the states of this region.
        /// </summary>
        IEnumerable<IState<TState, TEvent>> States { get; }

        /// <summary>
        /// Gets the initial state of the region.
        /// </summary>
        /// <returns>The initial state of the region.</returns>
        IState<TState, TEvent> IntialState { get; }

        /// <summary>
        /// Gets or sets the last active state of this state.
        /// </summary>
        /// <value>The last state of the active.</value>
        IState<TState, TEvent> LastActiveState { get; set; }

        void AddState(IState<TState, TEvent> state);
        void SetInitialState(IState<TState, TEvent> initialState);
    }
}