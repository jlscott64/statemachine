using System;
using Appccelerate.StateMachine.Machine.Events;
using FakeItEasy;
using FluentAssertions;
using Xunit;
using Xunit.Extensions;
using E = System.Int32;
using S = System.String;

namespace Appccelerate.StateMachine.Machine
{
    public class RegionBuilderTest
    {
        private const string OwningStateId = "OwningState";

        private readonly RegionBuilder<S, E> testee;

        private readonly IStateDictionary<S, E> states;

        private readonly IState<S, E> owningState;

        private readonly IRegion<S, E> region;

        private readonly IFactory<S, E> factory;

        public RegionBuilderTest()
        {
            this.owningState = A.Fake<IState<S, E>>();
            A.CallTo(() => this.owningState.Id).Returns(OwningStateId);

            this.states = A.Fake<IStateDictionary<S, E>>();
            A.CallTo(() => this.states[OwningStateId]).Returns(this.owningState);

            this.region = A.Fake<IRegion<S, E>>();

            factory = A.Fake<IFactory<S, E>>();
            A.CallTo(() => factory.CreateRegion(this.owningState)).Returns(this.region);

            this.testee = new RegionBuilder<S, E>(factory, this.states, OwningStateId);
        }

        [Fact]
        public void PassOwningStateOfRegionToFactory()
        {
            A.CallTo(() => factory.CreateRegion(this.owningState))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void AddsRegionToOwningState()
        {
            A.CallTo(() =>
                this.owningState.AddRegion(
                    A<IRegion<S, E>>.That.Not.IsNull()))
                .MustHaveHappened();
        }


        [Fact]
        public void SetsInitialSubStateOfRegion()
        {
            const string SubState = "SubState";
            var subState = A.Fake<IState<S, E>>();
            subState.SuperState = null;
            A.CallTo(() => this.states[SubState]).Returns(subState);

            this.testee
                .WithInitialSubState(SubState);

            this.region.InitialState
                .Should().BeSameAs(subState);
        }

        [Fact]
        public void AddsInitialSubStateToSuperState()
        {
            const string SubState = "SubState";
            var subState = A.Fake<IState<string, int>>();
            subState.SuperState = null;
            A.CallTo(() => this.states[SubState]).Returns(subState);

            this.testee
                .WithInitialSubState(SubState);

            A.CallTo(() => this.owningState.SubStates.Add(subState)).MustHaveHappened();
        }

        [Fact]
        public void AddsSubStatesToOwnerState()
        {
            const string AnotherSubState = "AnotherSubState";
            var anotherSubState = A.Fake<IState<S, E>>();
            A.CallTo(() => this.states[AnotherSubState]).Returns(anotherSubState);

            this.testee
                .WithSubState(AnotherSubState);

            A.CallTo(() => this.owningState.SubStates.Add(anotherSubState)).MustHaveHappened();
        }

        [Fact]
        public void ThrowsExceptionIfSubStateAlreadyHasASuperState()
        {
            const string SubState = "SubState";
            var subState = A.Fake<IState<S, E>>();
            subState.SuperState = A.Fake<IState<S, E>>(); 
            A.CallTo(() => this.states[SubState]).Returns(subState);

            this.testee.Invoking(t => t.WithInitialSubState(SubState))
                .ShouldThrow<InvalidOperationException>()
                .WithMessage(ExceptionMessages.CannotSetStateAsASuperStateBecauseASuperStateIsAlreadySet(
                    OwningStateId,
                    subState));
        }
    }
}