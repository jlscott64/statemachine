//-------------------------------------------------------------------------------
// <copyright file="HierarchyBuilderTest.cs" company="Appccelerate">
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

using Appccelerate.StateMachine.Machine.States;

namespace Appccelerate.StateMachine.Machine
{
    using System;

    using FakeItEasy;

    using FluentAssertions;

    using Xunit;
    using Xunit.Extensions;

    public class HierarchyBuilderTest
    {
        private const string SuperState = "SuperState";

        private readonly HierarchyBuilder<string, int> testee;

        private readonly IStateDictionary<string, int> states;

        private readonly IState<string, int> superState;
        private readonly IRegion<string, int> region;

        public HierarchyBuilderTest()
        {
            this.region = A.Fake<IRegion<string, int>>();

            this.superState = A.Fake<IState<string, int>>();
            A.CallTo(() => this.superState.Id).Returns(SuperState);
            A.CallTo(() => this.superState.AddRegion()).Returns(this.region).Once();

            this.states = A.Fake<IStateDictionary<string, int>>();
            A.CallTo(() => this.states[SuperState]).Returns(this.superState);

            this.testee = new HierarchyBuilder<string, int>(this.states, SuperState);
        }

        [Theory]
        [InlineData(HistoryType.Deep)]
        [InlineData(HistoryType.Shallow)]
        [InlineData(HistoryType.None)]
        public void SetsHistoryTypeOfSuperState(HistoryType historyType)
        {
            this.testee.WithHistoryType(historyType);

            A.CallTo(() => this.superState.SetHistoryType(historyType)).MustHaveHappened();
        }

        [Fact]
        public void SetsInitialStateOfRegion()
        {
            const string SubState = "SubState";
            var subState = A.Fake<IState<string, int>>();

            A.CallTo(() => subState.SuperState).Returns(null);
            A.CallTo(() => this.states[SubState]).Returns(subState);

            this.testee.WithInitialSubState(SubState);

            A.CallTo(() => this.region.SetInitialState(subState)).MustHaveHappened();
        }

        [Fact]
        public void AddsSubStatesToRegion()
        {
            const string AnotherSubState = "AnotherSubState";
            var anotherSubState = A.Fake<IState<string, int>>();

            A.CallTo(() => anotherSubState.SuperState).Returns(null);
            A.CallTo(() => this.states[AnotherSubState]).Returns(anotherSubState);

            this.testee
                .WithSubState(AnotherSubState);

            A.CallTo(() => this.region.AddState(anotherSubState)).MustHaveHappened();
        }

        [Fact]
        public void ThrowsExceptionIfSubStateAlreadyHasASuperState()
        {
            const string SubState = "SubState";
            var subState = A.Fake<IState<string, int>>();
            A.CallTo(() => subState.SuperState).Returns(A.Fake<IState<string, int>>()); 
            A.CallTo(() => this.states[SubState]).Returns(subState);

            this.testee.Invoking(t => t.WithInitialSubState(SubState))
                .ShouldThrow<InvalidOperationException>()
                .WithMessage(ExceptionMessages.CannotSetStateAsASuperStateBecauseASuperStateIsAlreadySet(
                    SuperState,
                    subState));
        }

        [Fact]
        public void ThrowsExceptionIfSuperStateAddedAsItsOwnSubState()
        {
            A.CallTo(() => this.superState.SuperState).Returns(null);

            this.testee.Invoking(t => t.WithSubState(this.superState.Id))
                .ShouldThrow<ArgumentException>()
                .WithMessage(ExceptionMessages.StateCannotBeItsOwnSuperState(this.superState.ToString()));
        }

        [Fact]
        public void HierarchyWhenSettingLevelThenTheLevelOfAllChildrenIsUpdated()
        {
            const int Level = 2;
            A.CallTo(() => this.superState.Level).Returns(Level);

            const string SubState = "SubState";
            var subState = A.Fake<IState<string, int>>();

            A.CallTo(() => subState.SuperState).Returns(null);
            A.CallTo(() => this.states[SubState]).Returns(subState);

            this.testee.WithSubState(SubState);

            A.CallTo(() => subState.SetLevel(Level + 1)).MustHaveHappened();
        }
    }
}