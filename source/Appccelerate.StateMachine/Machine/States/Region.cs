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

    public class Region<TState, TEvent> : IRegion<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        /// <summary>
        /// Collection of the states of this region.
        /// </summary>
        readonly List<IState<TState, TEvent>> states = new List<IState<TState, TEvent>>();

        public Region(IState<TState, TEvent> owner)
        {
            this.Owner = owner;
        }

        /// <summary>
        /// The owner of this region.
        /// </summary>
        public IState<TState, TEvent> Owner { get; private set; }

        /// <summary>
        /// Gets the initial state of the region.
        /// </summary>
        /// <returns>The initial state of the region.</returns>
        public IState<TState, TEvent> InitialState { get; private set; }

        public IState<TState, TEvent> LastActiveState { get; set; }

        public IState<TState, TEvent> ActiveState { get; set; }

        /// <summary>
        /// Collection of the states of this region.
        /// </summary>
        public IEnumerable<IState<TState, TEvent>> States
        {
            get { return states; }
        }

        public void AddState(IState<TState, TEvent> state)
        {
            states.Add(state);
        }

        public void SetInitialState(IState<TState, TEvent> initialState)
        {
            InitialState = initialState;
        }
    }
}