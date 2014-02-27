//-------------------------------------------------------------------------------
// <copyright file="TransitionResult.cs" company="Appccelerate">
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

using System.Collections.Generic;
using System.Linq;

namespace Appccelerate.StateMachine.Machine.Transitions
{
    using System;

    public class TransitionResult<TState, TEvent>
        : ITransitionResult<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        public static readonly ITransitionResult<TState, TEvent> NotFired = new TransitionResult<TState, TEvent>();
        private readonly List<IState<TState, TEvent>> newStates = new List<IState<TState, TEvent>>();

        TransitionResult()
        {
            this.Fired = false;
        }

        public TransitionResult(bool fired, IState<TState, TEvent> newState)
        {
            if (newState == null) throw new ArgumentNullException("newState");

            this.Fired = fired;
            this.newStates.Add(newState);
        }

        public TransitionResult(bool fired, IEnumerable<IState<TState, TEvent>> newStates)
        {
            if (newStates == null) throw new ArgumentNullException("newStates");

            this.Fired = fired;
            this.newStates.AddRange(newStates);
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ITransitionResult{TState, TEvent}"/> is fired.
        /// </summary>
        /// <value><c>true</c> if fired; otherwise, <c>false</c>.</value>
        public bool Fired { get; private set; }

        /// <summary>
        /// Gets the new state the state machine is in.
        /// </summary>
        /// <value>The new state.</value>
        public IState<TState, TEvent> NewState { get { return this.newStates.FirstOrDefault(); } }

        /// <summary>
        /// Gets the new states the state machine is in.
        /// </summary>
        /// <value>The new states.</value>
        public IEnumerable<IState<TState, TEvent>> NewStates { get { return this.newStates; } }
    }
}