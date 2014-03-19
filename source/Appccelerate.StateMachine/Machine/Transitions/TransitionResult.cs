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
        readonly IEnumerable<IState<TState, TEvent>> newStates;

        public TransitionResult(bool fired, IState<TState, TEvent> newState)
            : this(fired, new[] { newState })
        {
        }

        public TransitionResult(bool fired, IEnumerable<IState<TState, TEvent>> newStates)
        {
            this.Fired = fired;
            this.newStates = newStates.ToArray();
            this.NewState = NewStates.FirstOrDefault();
        }

        private TransitionResult()
            : this(false, new IState<TState, TEvent>[0])
        {
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
        public IState<TState, TEvent> NewState { get; private set; }

        /// <summary>
        /// Gets the new states the state machine is in.
        /// </summary>
        /// <value>The new states.</value>
        public IEnumerable<IState<TState, TEvent>> NewStates
        {
            get { return newStates; }
        }
    }
}