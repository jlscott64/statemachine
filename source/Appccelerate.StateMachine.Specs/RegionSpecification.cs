//-------------------------------------------------------------------------------
// <copyright file="HierarchicalStateMachineInitializationSpecification.cs" company="Appccelerate">
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
using FakeItEasy;

namespace Appccelerate.StateMachine
{
    using FluentAssertions;
    using global::Machine.Specifications;

    [Subject(Concern.OrthogonalStates)]
    public class When_the_state_machine_is_started : RegionSpecification
    {
        Because of = () =>
        {
            machine.Initialize(State.S1);
            machine.Start();
        };

        It should_enter_the_initial_state_of_the_first_region = () =>
        {
            A.CallTo(() => testExtension.SwitchedState(SomeStateMachineInfo, NoState, TheStateWithId(State.S1_R1_1))).MustHaveHappened();
        };

        It should_enter_the_initial_state_of_the_second_region = () =>
        {
            A.CallTo(() => testExtension.SwitchedState(SomeStateMachineInfo, NoState, TheStateWithId(State.S1_R2_1))).MustHaveHappened();
        };
    }

    [Subject(Concern.OrthogonalStates)]
    public class RegionSpecification
    {
        static protected readonly IState<State, int> NoState = null;
        static protected readonly IStateMachineInformation<State, int> SomeStateMachineInfo = A<IStateMachineInformation<State, int>>.Ignored;
        static protected readonly ITransitionContext<State, int> SomeTransitionContext = A<ITransitionContext<State, int>>.Ignored;

        static protected IState<State, int> TheStateWithId(State stateId)
        {
            return A<IState<State, int>>.That.Matches(s => s.Id == stateId);
        }

        protected static PassiveStateMachine<State, int> machine;

        protected static IExtension<State, int> testExtension;

        Establish context = () =>
        {
            testExtension = A.Fake<IExtension<State, int>>();

            machine = new PassiveStateMachine<State, int>();

            machine.AddExtension(testExtension);

            machine.DefineRegionOn(State.S1)
                .WithInitialSubState(State.S1_R1_1);

            machine.DefineRegionOn(State.S1)
                .WithInitialSubState(State.S1_R2_1);
        };
    }

    public enum State
    {
        S1,
            S1_R1_1,
            S1_R2_1,
    }
}