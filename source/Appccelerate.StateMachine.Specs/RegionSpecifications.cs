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

using System.Collections;
using System.Collections.Generic;
using Appccelerate.StateMachine.Machine;
using Appccelerate.StateMachine.Machine.States;
using FakeItEasy;

namespace Appccelerate.StateMachine
{
// ReSharper disable InconsistentNaming
    
    using System;
    using System.Globalization;

    using FluentAssertions;
    using global::Machine.Specifications;

    public class RegionSpecification
    {
        protected static PassiveStateMachine<S, E> machine;
        protected static IExtension<S, E> extension;
        protected static IList<S> currentStates;

        public enum S
        {
            // State A
            A,

            // State A, region X
            Ax1,
            Ax2,

            // State A, Region Y
            Ay1,
            Ay2,

            // State B
            B,
        }

        public enum E
        {
            Ax1_to_Ax2_and_Ay1_to_Ay2,
            Ay1_to_Ay2,
            Ax1_to_B,
            B_to_A
        }

        Establish context = () =>
        {
            machine = new PassiveStateMachine<S, E>();
            currentStates = new List<S>();

            extension = A.Fake<IExtension<S, E>>();
            machine.AddExtension(extension);

            machine.DefineHierarchyOn(S.A)
                .WithHistoryType(HistoryType.None)
                .WithInitialSubState(S.Ax1)
                .WithSubState(S.Ax2);

            machine.DefineRegionOn(S.A)
                .WithInitialSubState(S.Ay1)
                .WithSubState(S.Ay2);

            machine.In(S.Ax1).On(E.Ax1_to_Ax2_and_Ay1_to_Ay2).Goto(S.Ax2);
            machine.In(S.Ay1).On(E.Ax1_to_Ax2_and_Ay1_to_Ay2).Goto(S.Ay2);
            machine.In(S.Ay1).On(E.Ay1_to_Ay2).Goto(S.Ay2);
            machine.In(S.Ax1).On(E.Ax1_to_B).Goto(S.B);
            machine.In(S.B).On(E.B_to_A).Goto(S.A);

            foreach(S id in Enum.GetValues(typeof(S)))
            {
                machine.In(id)
                    .ExecuteOnEntryParametrized(currentStates.Add, id)
                    .ExecuteOnExitParametrized(s => currentStates.Remove(s), id);
            }
        };
    }

    public class Starting_in_A : RegionSpecification
    {
        Establish context = () =>
        {
            machine.Initialize(S.A);
            machine.Start();
        };
    }

    public class Starting_in_B : RegionSpecification
    {
        Establish context = () =>
        {
            machine.Initialize(S.B);
            machine.Start();
        };
    }

    [Subject(Concern.Initialization)]
    public class When_a_state_with_two_regions_is_initialized : RegionSpecification
    {
        Because of = () =>
        {
            machine.Initialize(S.A);
            machine.Start();
        };

        It should_be_in_the_both_regions_intial_states = () =>
            currentStates.Should().BeEquivalentTo(S.A, S.Ax1, S.Ay1);
    }

    [Subject(Concern.Transition)]
    public class When_an_event_has_transitions_one_region_of_a_state : Starting_in_A
    {
        Because of = () => { machine.Fire(E.Ay1_to_Ay2); };

        It should_be_in_that_regions_new_state = () =>
            currentStates.Should().BeEquivalentTo(S.A, S.Ax1, S.Ay2);
    }

    [Subject(Concern.Transition)]
    public class When_an_event_has_transitions_two_regions_of_a_state : Starting_in_A
    {
        Because of = () => machine.Fire(E.Ax1_to_Ax2_and_Ay1_to_Ay2);

        It should_be_in_both_regions_new_states = () =>
            currentStates.Should().BeEquivalentTo(S.A, S.Ax2, S.Ay2);
    }

    [Subject(Concern.Transition)]
    public class When_an_event_transitions_out_of_a_state_with_two_regions : Starting_in_A
    {
        Because of = () => machine.Fire(E.Ax1_to_B);

        It should_exit_both_regions = () =>
            currentStates.Should().BeEquivalentTo(S.B);
    }

    [Subject(Concern.Transition)]
    public class When_an_event_transitions_to_a_state_with_two_regions : Starting_in_B
    {
        Because of = () => machine.Fire(E.B_to_A);

        It should_enter_both_regions_initialStates = () =>
            currentStates.Should().BeEquivalentTo(S.A, S.Ax1, S.Ay1);
    }
}