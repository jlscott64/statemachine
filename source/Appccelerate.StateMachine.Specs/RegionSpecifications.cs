//-------------------------------------------------------------------------------
// <copyright file="HierarchicalTransitionSpecification.cs" company="Appccelerate">
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

using Appccelerate.StateMachine.Machine;
using Appccelerate.StateMachine.Machine.States;
using FakeItEasy;

namespace Appccelerate.StateMachine
{
    using System;
    using System.Globalization;

    using FluentAssertions;
    using global::Machine.Specifications;

    public class RegionSpecification
    {
        protected static PassiveStateMachine<S, E> machine;
        protected static IExtension<S, E> extension;

        public enum S
        {
            A,

            // State A, region R
            Ar1,
            Ar2,

            // State A, Region Q
            Aq1,
            Aq2,
        }

        public enum E
        {
            E1,
        }

        Establish context = () =>
        {
            machine = new PassiveStateMachine<S, E>();

            extension = A.Fake<IExtension<S, E>>();
            machine.AddExtension(extension);

            machine.DefineHierarchyOn(S.A)
                .WithHistoryType(HistoryType.None)
                .WithInitialSubState(S.Ar1)
                .WithSubState(S.Ar2);

            machine.DefineRegionOn(S.A)
                .WithInitialSubState(S.Aq1)
                .WithSubState(S.Aq2);

            machine.In(S.Ar1).On(E.E1).Goto(S.Ar2);
            machine.In(S.Aq1).On(E.E1).Goto(S.Aq2);

            machine.Initialize(S.A);
            machine.Start();
        };

        protected static IState<S, E> anyState
        {
            get { return A<IState<S, E>>._; }
        }

        protected static IStateMachineInformation<S, E> anyMachine
        {
            get { return A<IStateMachineInformation<S, E>>._; }
        }

        protected static IState<S, E> State(S id)
        {
            return A<IState<S, E>>.That.Matches(s => s.Id == id);
        }
    }

    [Subject(Concern.Initialization)]
    public class When_a_state_with_two_regions_is_initialized : RegionSpecification
    {
        It should_be_in_the_first_regions_intial_state = () =>
            A.CallTo(() => extension.SwitchedState(anyMachine, anyState, State(S.Ar1))).MustHaveHappened();

        It should_be_in_the_second_regions_intial_state = () =>
            A.CallTo(() => extension.SwitchedState(anyMachine, anyState, State(S.Aq1))).MustHaveHappened();
    }
}