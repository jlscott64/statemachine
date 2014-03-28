//-------------------------------------------------------------------------------
// <copyright file="StateMachine.cs" company="Appccelerate">
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
    using System.Linq;
    using System.Reflection;

    using Appccelerate.StateMachine.Machine.Events;
    using Appccelerate.StateMachine.Persistence;
    using Appccelerate.StateMachine.Syntax;

    /// <summary>
    /// Base implementation of a state machine.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public class StateMachine<TState, TEvent> : PassiveStateMachine<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateMachine{TState,TEvent}"/> class.
        /// </summary>
        public StateMachine()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateMachine{TState,TEvent}"/> class.
        /// </summary>
        /// <param name="name">The name of this state machine used in log messages.</param>
        public StateMachine(string name)
            : this(name, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateMachine{TState,TEvent}"/> class.
        /// </summary>
        /// <param name="name">The name of this state machine used in log messages.</param>
        /// <param name="factory">The factory used to create internal instances.</param>
        public StateMachine(string name, IFactory<TState, TEvent> factory)
            :base(name, factory)
        {
        }
    }
}