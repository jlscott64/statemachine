//-------------------------------------------------------------------------------
// <copyright file="HierarchyBuilder.cs" company="Appccelerate">
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


namespace Appccelerate.StateMachine
{
    using System;
    
    using Appccelerate.StateMachine.Machine;
    using Appccelerate.StateMachine.Machine.States;
    using Appccelerate.StateMachine.Syntax;

    public class HierarchyBuilder<TState, TEvent> : 
        IHierarchySyntax<TState>, 
        IInitialSubStateSyntax<TState>,
        ISubStateSyntax<TState>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly IStateDictionary<TState, TEvent> states;

        private readonly IState<TState, TEvent> superState;
        private readonly IRegion<TState, TEvent> region;

        public HierarchyBuilder(IStateDictionary<TState, TEvent> states, TState superStateId)
        {
            Ensure.ArgumentNotNull(states, "states");

            this.states = states;
            this.superState = this.states[superStateId];
            this.region = this.superState.AddRegion();
        }

        public IInitialSubStateSyntax<TState> WithHistoryType(HistoryType historyType)
        {
            this.superState.SetHistoryType(historyType);

            return this;
        }

        public ISubStateSyntax<TState> WithInitialSubState(TState stateId)
        {
            var subState = this.states[stateId];

            this.WithSubState(subState);
            this.region.SetInitialState(subState);

            return this;
        }

        public ISubStateSyntax<TState> WithSubState(TState stateId)
        {
            var subState = this.states[stateId];
            return WithSubState(subState);
        }

        ISubStateSyntax<TState> WithSubState(IState<TState, TEvent> subState)
        {
            this.CheckThatStateHasNotAlreadyASuperState(subState);
            CheckSuperStateIsNotItself(subState, this.superState);

            subState.SetSuperState(this.superState);
            SetLevel(subState, this.superState.Level + 1);
            subState.SetRegion(this.region);
            this.region.AddState(subState);

            return this;
        }

        private void CheckThatStateHasNotAlreadyASuperState(IState<TState, TEvent> subState)
        {
            Ensure.ArgumentNotNull(subState, "subState");

            if (subState.SuperState != null)
            {
                throw new InvalidOperationException(
                    ExceptionMessages.CannotSetStateAsASuperStateBecauseASuperStateIsAlreadySet(
                        this.superState.Id, 
                        subState));
            }
        }

        private static void SetLevel(IState<TState, TEvent> state, int level)
        {
            state.SetLevel(level);
            foreach (var subState in state.SubStates)
            {
                SetLevel(subState, level + 1);
            }
        }

        // ReSharper disable once UnusedParameter.Local
        private static void CheckSuperStateIsNotItself(IState<TState, TEvent> state, IState<TState, TEvent> newSuperState)
        {
            if (state == newSuperState)
            {
                throw new ArgumentException(StatesExceptionMessages.StateCannotBeItsOwnSuperState(state.ToString()));
            }
        }
    }
}