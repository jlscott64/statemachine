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
    using Appccelerate.StateMachine.Syntax;
    
    public class RegionBuilder<TState, TEvent> : IInitialSubStateSyntax<TState>,
        ISubStateSyntax<TState>
        where TState : IComparable
        where TEvent : IComparable
    {
        public RegionBuilder(IStateDictionary<TState, TEvent> states, TState orthogonalStateId)

        {
            
        }

        public ISubStateSyntax<TState> WithInitialSubState(TState stateId)
        {
            throw new NotImplementedException();
        }

        public ISubStateSyntax<TState> WithSubState(TState stateId)
        {
            throw new NotImplementedException();
        }
    }
}