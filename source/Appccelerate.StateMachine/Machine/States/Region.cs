//-------------------------------------------------------------------------------
// <copyright file="State.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Machine.States
{
    using System;
    using System.Collections.Generic;

    using Appccelerate.StateMachine.Machine.ActionHolders;
    using Appccelerate.StateMachine.Machine.Transitions;

    /// <summary>
    /// A region in a state of the state machine.
    /// A state with more than one region is an "orthogonal" state
    /// in UML terminology.  Each region can be active simultaneously
    /// meaning that the state can have more than one current substate
    /// at a time.
    /// </summary>
    /// <typeparam name="TState">The type of the state id.</typeparam>
    /// <typeparam name="TEvent">The type of the event id.</typeparam>
    public class Region<TState, TEvent> 
        : IRegion<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        /// <summary>
        /// Collection of the sub-states of this region.
        /// </summary>
        private readonly List<IState<TState, TEvent>> subStates;

        /// <summary>
        /// Collection of transitions that start in this region (<see cref="ITransition{TState,TEvent}.Source"/> is equal to this state).
        /// </summary>
        private readonly TransitionDictionary<TState, TEvent> transitions;

        private readonly IStateMachineInformation<TState, TEvent> stateMachineInformation;

        /// <summary>
        /// The state to which this region belongs.
        /// </summary>
        private readonly IState<TState, TEvent> owningState;

        /// <summary>
        /// The initial sub-state of this region.
        /// </summary>
        private IState<TState, TEvent> initialState;

        /// <summary>
        /// Initializes a new instance of the <see cref="State&lt;TState, TEvent&gt;"/> class.
        /// </summary>
        /// <param name="owningState">The state to which this region belongs.</param>
        public Region(IState<TState, TEvent> owningState)
        {
            this.owningState = owningState;
            this.subStates = new List<IState<TState, TEvent>>();
            this.transitions = new TransitionDictionary<TState, TEvent>(owningState);
        }

        /// <summary>
        /// Gets or sets the last active state of this state.
        /// </summary>
        /// <value>The last state of the active.</value>
        public IState<TState, TEvent> LastActiveState { get; set; }

        /// <summary>
        /// Gets or sets the initial sub state of this state.
        /// </summary>
        /// <value>The initial sub state of this state.</value>
        public IState<TState, TEvent> InitialState
        {
            get
            {
                return this.initialState;
            }

            set
            {
                this.CheckInitialStateIsASubState(value);
                this.initialState = this.LastActiveState = value;
            }
        }

        /// <summary>
        /// Gets the state to which this region belongs.
        /// </summary>
        /// <value>The owing state of this region.</value>
        public IState<TState, TEvent> Owner
        {
            get
            {
                return this.owningState;
            }
        }

        /// <summary>
        /// Gets the sub-states of this region.
        /// </summary>
        /// <value>The sub-states of this region.</value>
        public ICollection<IState<TState, TEvent>> SubStates 
        { 
            get { return this.subStates; }
        }

        /// <summary>
        /// Gets the transitions that start in this region.
        /// </summary>
        /// <value>The transitions.</value>
        public ITransitionDictionary<TState, TEvent> Transitions
        {
            get { return this.transitions; }
        }

        /// <summary>
        /// Goes recursively up the state hierarchy until a state is found that can handle the event.
        /// </summary>
        /// <param name="context">The event context.</param>
        /// <returns>The result of the transition.</returns>
        public ITransitionResult<TState, TEvent> Fire(ITransitionContext<TState, TEvent> context)
        {
            Ensure.ArgumentNotNull(context, "context");

            ITransitionResult<TState, TEvent> result = TransitionResult<TState, TEvent>.NotFired;

            var transitionsForEvent = this.transitions[context.EventId.Value];
            if (transitionsForEvent != null)
            {
                foreach (ITransition<TState, TEvent> transition in transitionsForEvent)
                {
                    result = transition.Fire(context);
                    if (result.Fired)
                    {
                        return result;
                    }
                }
            }

            return result;
        }

        public void AddSubState(IState<TState, TEvent> subState)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Throws an exception if the new initial state is not a sub-state of this instance.
        /// </summary>
        /// <param name="value">The value.</param>
        private void CheckInitialStateIsASubState(IState<TState, TEvent> value)
        {
            if (value.ContainingRegion != this)
            {
                throw new ArgumentException(StatesExceptionMessages.StateCannotBeTheInitialStateOfSuperStateBecauseItIsNotADirectSubState(value.ToString(), this.ToString()));
            }
        }
    }
}