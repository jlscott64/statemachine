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

    [Subject(Concern.Initialization)]
    public class When_a_state_with_two_regions_is_initialized
    {
        public enum S
        {
            A,
            Ar1,
            Aq1,
        }

        public enum E
        {
            
        }

        static PassiveStateMachine<S, E> machine;

        static IExtension<S, E> extension;

        Establish context = () =>
            {
                machine = new PassiveStateMachine<S, E>();

                extension = A.Fake<IExtension<S, E>>();
                machine.AddExtension(extension);
                A.CallTo(() => extension.SwitchedState(anyMachine, anyState, State(S.Ar1))).Invokes(() =>
                {
                    machine.GetHashCode();
                });


                machine.DefineHierarchyOn(S.A)
                    .WithHistoryType(HistoryType.None)
                    .WithInitialSubState(S.Ar1);
                
                machine.DefineRegionOn(S.A)
                    .WithInitialSubState(S.Aq1);

                machine.Initialize(S.A);
            };

        Because of = () => machine.Start();

        It should_be_in_the_first_regions_intial_state = () =>
            A.CallTo(() => extension.SwitchedState(anyMachine, anyState, State(S.Ar1))).MustHaveHappened();

        It should_be_in_the_second_regions_intial_state = () =>
            A.CallTo(() => extension.SwitchedState(anyMachine, anyState, State(S.Aq1))).MustHaveHappened();

        static IState<S, E> anyState
        {
            get { return A<IState<S, E>>._; }
        }

        static IStateMachineInformation<S, E> anyMachine
        {
            get { return A<IStateMachineInformation<S, E>>._; }
        }

        static IState<S, E> State(S id)
        {
            return A<IState<S, E>>.That.Matches(s => s.Id == id);
        }

    }
}