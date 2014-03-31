//-------------------------------------------------------------------------------
// <copyright file="StateMachineInitializer.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Machine
{
    using System;
    using Appccelerate.StateMachine.Machine.Transitions;

    /// <summary>
    /// Responsible for entering the initial state of the state machine. 
    /// All states up in the hierarchy are entered, too.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public class StateMachineInitializer<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly IState<TState, TEvent> initialState;

        private readonly ITransitionContext<TState, TEvent> context;

        public StateMachineInitializer(IState<TState, TEvent> initialState, ITransitionContext<TState, TEvent> context)
        {
            this.initialState = initialState;
            this.context = context;
        }

        public IEnumerable<IState<TState, TEvent>> EnterInitialStates()
        {
            var traversal = new Traversal<TState, TEvent>();
            return traversal.ExecuteTraversal(context, 
                sourceState: null, 
                targetState: this.initialState, 
                transitionAction: null);
        }
    }
}