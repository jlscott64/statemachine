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


using Appccelerate.StateMachine.Machine.Events;

namespace Appccelerate.StateMachine.Machine
{
    using System;
    using Appccelerate.StateMachine.Syntax;
    using Appccelerate.StateMachine.Machine.States;
    
    public class RegionBuilder<TState, TEvent> : IInitialSubStateSyntax<TState>,
        ISubStateSyntax<TState>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly IStateDictionary<TState, TEvent> states;
        private readonly IRegion<TState, TEvent> region;

        public RegionBuilder(IFactory<TState,TEvent> factory, IStateDictionary<TState, TEvent> states, TState owningStateId)
        {
            this.states = states;

            var owningState = this.states[owningStateId];

            region = factory.CreateRegion(owningState);
            owningState.AddRegion(region);
        }

        public ISubStateSyntax<TState> WithInitialSubState(TState stateId)
        {
            var subState = this.states[stateId];
            region.InitialState = subState;
            return this;
        }

        public ISubStateSyntax<TState> WithSubState(TState stateId)
        {
            var subState = this.states[stateId];
            region.AddSubState(subState);
            return this;
        }
    }
}