//-------------------------------------------------------------------------------
// <copyright file="OrthogonalStatesSpecification.cs" company="Appccelerate">
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
    using FluentAssertions;
    using global::Machine.Specifications;


    [Subject(Concern.Initialization)]
    public class OrthogonalStatesSpecification
    {
        
        const int One = 0;
        const int One_RegionOne_SubStateOne = 1;
        
        protected static PassiveStateMachine<int, int> machine;
        protected static CurrentStateExtension testExtension;

        Establish context = () =>
        {
            testExtension = new CurrentStateExtension();

            machine = new PassiveStateMachine<int, int>();

            machine.AddExtension(testExtension);

            machine.DefineRegionOn(One)
                .WithInitialSubState(One_RegionOne_SubStateOne);
        };

        Because of = () =>
        {
            machine.Initialize(One);
            machine.Start();
        };

        It should_set_current_states_of_state_machine_to_initial_states_of_the_regions = () =>
        {
            testExtension.CurrentState.Should().Be(One_RegionOne_SubStateOne);
        };
    }
}