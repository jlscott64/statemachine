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

using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace Appccelerate.StateMachine.Machine.States
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Appccelerate.StateMachine.Machine.ActionHolders;
    using Appccelerate.StateMachine.Machine.Transitions;

    /// <summary>
    /// A state of the state machine.
    /// A state can be a sub-state or super-state of another state.
    /// </summary>
    /// <typeparam name="TState">The type of the state id.</typeparam>
    /// <typeparam name="TEvent">The type of the event id.</typeparam>
    public class State<TState, TEvent> 
        : IState<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        /// <summary>
        /// Collection of the regions of this state.
        /// </summary>
        private readonly List<IRegion<TState, TEvent>> regions;

        /// <summary>
        /// Collection of transitions that start in this state (<see cref="ITransition{TState,TEvent}.Source"/> is equal to this state).
        /// </summary>
        private readonly TransitionDictionary<TState, TEvent> transitions;

        private readonly IList<ITransition<TState, TEvent>> completionTransitions; 

        private readonly IStateMachineInformation<TState, TEvent> stateMachineInformation;

        private readonly IExtensionHost<TState, TEvent> extensionHost;

        /// <summary>
        /// The level of this state within the state hierarchy [1..maxLevel]
        /// </summary>
        private int level;

        /// <summary>
        /// The super-state of this state. Null for states with <see cref="level"/> equal to 1.
        /// </summary>
        private IState<TState, TEvent> superState;

        /// <summary>
        /// The <see cref="HistoryType"/> of this state.
        /// </summary>
        private HistoryType historyType = HistoryType.None;

        readonly INotifier<TState, TEvent> notifier;
        CancellationTokenSource cancellation;

        /// <summary>
        /// Initializes a new instance of the <see cref="State&lt;TState, TEvent&gt;"/> class.
        /// </summary>
        /// <param name="id">The unique id of this state.</param>
        /// <param name="stateMachineInformation">The state machine information.</param>
        /// <param name="notifier"></param>
        /// <param name="extensionHost">The extension host.</param>
        public State(TState id, IStateMachineInformation<TState, TEvent> stateMachineInformation, INotifier<TState, TEvent> notifier, IExtensionHost<TState, TEvent> extensionHost)
        {
            this.Id = id;
            this.level = 1;
            this.stateMachineInformation = stateMachineInformation;
            this.notifier = notifier;
            this.extensionHost = extensionHost;

            this.regions = new List<IRegion<TState, TEvent>>();
            this.transitions = new TransitionDictionary<TState, TEvent>(this);
            this.completionTransitions = new List<ITransition<TState, TEvent>>();

            this.EntryActions = new List<IActionHolder>();
            this.DoActions = new List<IDoActionHolder>();
            this.ExitActions = new List<IActionHolder>();
        }

        /// <summary>
        /// Gets or sets the current active state of this state.
        /// </summary>
        /// <value>The current state of the active.</value>
        public IState<TState, TEvent> ActiveState
        {
            get { return (this.regions.Any()) ? this.regions.First().ActiveState : null; }
            set { this.regions.First().ActiveState = value; }
        }

        /// <summary>
        /// Gets the current active states of this state.
        /// </summary>
        /// <value>The current active states.</value>
        public IEnumerable<IState<TState, TEvent>> ActiveStates
        {
            get { return this.regions.Select(r => r.ActiveState).Where(r => r != null).ToArray(); }
        }

        /// <summary>
        /// Gets or sets the last active state of this state.
        /// </summary>
        /// <value>The last state of the active.</value>
        public IState<TState, TEvent> LastActiveState
        {
            get { return (this.regions.Any()) ? this.regions.First().LastActiveState : null; }
            set { this.regions.First().LastActiveState = value; }
        }

        /// <summary>
        /// Gets the last active states of this state.
        /// </summary>
        /// <value>The last active states.</value>
        public IEnumerable<IState<TState, TEvent>> LastActiveStates
        {
            get { return this.regions.Select(r => r.LastActiveState).ToArray(); }
        }

        /// <summary>
        /// Gets the unique id of this state.
        /// </summary>
        /// <value>The id of this state.</value>
        public TState Id { get; private set; }

        /// <summary>
        /// Gets the entry actions.
        /// </summary>
        /// <value>The entry actions.</value>
        public IList<IActionHolder> EntryActions { get; private set; }

        /// <summary>
        /// Gets the do actions.
        /// </summary>
        /// <value>The do actiona.</value>
        public IList<IDoActionHolder> DoActions { get; private set; }

        /// <summary>
        /// Gets the exit actions.
        /// </summary>
        /// <value>The exit actions.</value>
        public IList<IActionHolder> ExitActions { get; private set; }

        /// <summary>
        /// DEPRECATED. Gets the initial sub-state. Null if this state has no sub-states.
        /// </summary>
        /// <returns>The initial sub-state. Null if this state has no sub-states.</returns>
        public IState<TState, TEvent> GetInitialState()
        {
            return regions.Any() ? this.regions.First().InitialState : null;
        }

        public void AddSubState(IState<TState, TEvent> subState)
        {
            throw new NotImplementedException("Getting rid of this function.");
        }

        public void AddInitialState(IState<TState, TEvent> initialState)
        {
            throw new NotImplementedException("Getting rid of this function.");
        }

        /// <summary>
        /// Gets the initial sub-states. Empty if this state has no sub-states.
        /// </summary>
        /// <value>The initial sub-states. Empty if this state has no sub-states.</value>
        public IEnumerable<IState<TState, TEvent>> InitialStates
        {
            get { return regions.Select(r => r.InitialState); }
        }

        /// <summary>
        /// Gets or sets the super-state of this state.
        /// </summary>
        /// <remarks>
        /// The <see cref="Level"/> of this state is changed accordingly to the super-state.
        /// </remarks>
        /// <value>The super-state of this super.</value>
        public IState<TState, TEvent> SuperState
        {
            get
            {
                return this.superState;
            }

            set
            {
                this.CheckSuperStateIsNotThisInstance(value);

                this.superState = value;

                this.SetInitialLevel();
            }
        }

        /// <summary>
        /// Gets or sets the level of this state in the state hierarchy.
        /// When set then the levels of all sub-states are changed accordingly.
        /// </summary>
        /// <value>The level.</value>
        public int Level
        {
            get
            {
                return this.level;
            }
            
            set
            {
                this.level = value;

                this.SetLevelOfSubStates();
            }
        }

        /// <summary>
        /// Gets or sets the history type of this state.
        /// </summary>
        /// <value>The type of the history.</value>
        public HistoryType HistoryType
        {
            get { return this.historyType; } 
            set { this.historyType = value; }
        }

        public IRegion<TState, TEvent> Region { get; set; }

        /// <summary>
        /// Gets the sub-states of this state.
        /// </summary>
        /// <value>The sub-states of this state.</value>
        public IEnumerable<IState<TState, TEvent>> SubStates 
        {
            get { return this.regions.SelectMany(r => r.States); }
        }

        /// <summary>
        /// Gets the transitions that start in this state.
        /// </summary>
        /// <value>The transitions.</value>
        public ITransitionDictionary<TState, TEvent> Transitions
        {
            get { return this.transitions; }
        }

        /// <summary>
        /// Gets the completion transitions.
        /// </summary>
        /// <value>The transitions.</value>
        public IList<ITransition<TState, TEvent>> CompletionTransitions
        {
            get { return this.completionTransitions; }
        }

        /// <summary>
        /// Give the event context, returns the transition to be fired by this state.
        /// </summary>
        /// <param name="context">The event context.</param>
        /// <returns>The transition to be fired or null.</returns>
        public ITransition<TState, TEvent> GetTransitionToFire(ITransitionContext<TState, TEvent> context)
        {
            Ensure.ArgumentNotNull(context, "context");
            ITransition<TState, TEvent> result = null;

            var transitionsForEvent = this.transitions[context.EventId.Value].NotNull();

            foreach (ITransition<TState, TEvent> transition in transitionsForEvent)
            {
                if (transition.WillFire(context))
                {
                    result = transition;
                    break;
                }
                else
                {
                    var transition1 = transition;

                    this.extensionHost.ForEach(extension => extension.SkippedTransition(
                        this.stateMachineInformation,
                        transition1,
                        context));
                }
            }

            return result;
        }

        public void Entry(ITransitionContext<TState, TEvent> context)
        {
            Ensure.ArgumentNotNull(context, "context");

            this.SetThisStateAsActiveStateOfRegion();
            this.ExecuteEntryActions(context);
            this.StartDoActions(context);
        }

        public void Exit(ITransitionContext<TState, TEvent> context)
        {
            Ensure.ArgumentNotNull(context, "context");

            this.StopDoActions(context);
            this.ExecuteExitActions(context);
            this.SetThisStateAsLastStateOfRegion();
        }

        /// <summary>
        /// Sets this instance as the last state of this instance's region.
        /// </summary>
        private void SetThisStateAsActiveStateOfRegion()
        {
            if (this.Region != null)
            {
                this.Region.ActiveState = this;
                //this.Region.LastActiveState = null;
            }
        }

        /// <summary>
        /// Sets this instance as the last state of this instance's region.
        /// </summary>
        private void SetThisStateAsLastStateOfRegion()
        {
            if (this.Region != null)
            {
                this.Region.ActiveState = null;
                this.Region.LastActiveState = this;
            }
        }

        public override string ToString()
        {
            return this.Id.ToString();
        }

        private void HandleException(Exception exception, ITransitionContext<TState,TEvent> context)
        {
            notifier.OnExceptionThrown(context, exception);
        }

        /// <summary>
        /// Sets the initial level depending on the level of the super state of this instance.
        /// </summary>
        private void SetInitialLevel()
        {
            this.Level = this.superState != null ? this.superState.Level + 1 : 1;
        }

        /// <summary>
        /// Sets the level of all sub states.
        /// </summary>
        private void SetLevelOfSubStates()
        {
            foreach (var state in this.SubStates)
            {
                state.Level = this.level + 1;
            }
        }

        private void ExecuteEntryActions(ITransitionContext<TState, TEvent> context)
        {
            foreach (var actionHolder in this.EntryActions)
            {
                this.ExecuteEntryAction(actionHolder, context);
            }
        }

        private void ExecuteEntryAction(IActionHolder actionHolder, ITransitionContext<TState, TEvent> context)
        {
            try
            {
                actionHolder.Execute(context.EventArgument);
            }
            catch (Exception exception)
            {
                this.HandleEntryActionException(context, exception);
            }
        }

        private void HandleEntryActionException(ITransitionContext<TState, TEvent> context, Exception exception)
        {
            this.extensionHost.ForEach(
                extension =>
                extension.HandlingEntryActionException(
                    this.stateMachineInformation, this, context, ref exception));

            HandleException(exception, context);

            this.extensionHost.ForEach(
                extension =>
                extension.HandledEntryActionException(
                    this.stateMachineInformation, this, context, exception));
        }

        private void StartDoActions(ITransitionContext<TState, TEvent> context)
        {
            cancellation = new CancellationTokenSource();
            var cancellationToken = cancellation.Token;

            var doActionTasks = this.DoActions.Select(actionHolder => this.StartDoAction(actionHolder, context, cancellationToken));
            Task.WhenAll(doActionTasks).ContinueWith(t => this.DoActionsCompleted(), TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private void DoActionsCompleted()
        {
            OnCompleted(new StateCompletedEventArgs());
        }

        private Task StartDoAction(IDoActionHolder actionHolder, ITransitionContext<TState, TEvent> context, CancellationToken cancellationToken)
        {
            var doActionTask = actionHolder.Start(context.EventArgument, cancellationToken);
            doActionTask.ContinueWith(t => DoActionFailed(t, actionHolder, context), cancellationToken);
            return doActionTask;
        }

        private void DoActionFailed(Task doActionTask, IDoActionHolder actionHolder, ITransitionContext<TState, TEvent> context)
        {
            this.HandleDoActionException(context, doActionTask.Exception);
        }

        private void HandleDoActionException(ITransitionContext<TState, TEvent> context, Exception exception)
        {
            this.extensionHost.ForEach(
                extension =>
                extension.HandlingDoActionException(
                    this.stateMachineInformation, this, context, ref exception));

            HandleException(exception, context);

            this.extensionHost.ForEach(
                extension =>
                extension.HandledDoActionException(
                    this.stateMachineInformation, this, context, exception));
        }

        private void StopDoActions(ITransitionContext<TState, TEvent> context)
        {
            cancellation.Cancel();
        }

        private void ExecuteExitActions(ITransitionContext<TState, TEvent> context)
        {
            foreach (var actionHolder in this.ExitActions)
            {
                this.ExecuteExitAction(actionHolder, context);
            }
        }

        private void ExecuteExitAction(IActionHolder actionHolder, ITransitionContext<TState, TEvent> context)
        {
            try
            {
                actionHolder.Execute(context.EventArgument);
            }
            catch (Exception exception)
            {
                this.HandleExitActionException(context, exception);
            }
        }

        private void HandleExitActionException(ITransitionContext<TState, TEvent> context, Exception exception)
        {
            this.extensionHost.ForEach(
                extension =>
                extension.HandlingExitActionException(
                    this.stateMachineInformation, this, context, ref exception));

            HandleException(exception, context);

            this.extensionHost.ForEach(
                extension =>
                extension.HandledExitActionException(
                    this.stateMachineInformation, this, context, exception));
        }

        public bool HasInitialState()
        {
            return this.InitialStates.Any();
        }

        public IRegion<TState, TEvent> AddRegion()
        {
            var region = new Region<TState, TEvent>(this);
            this.regions.Add(region);
            return region;
        }

        public event EventHandler<StateCompletedEventArgs> Completed;

        protected virtual void OnCompleted(StateCompletedEventArgs e)
        {
            var handler = Completed;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Throws an exception if the new super state is this instance.
        /// </summary>
        /// <param name="newSuperState">The value.</param>
        // ReSharper disable once UnusedParameter.Local
        private void CheckSuperStateIsNotThisInstance(IState<TState, TEvent> newSuperState)
        {
            if (this == newSuperState)
            {
                throw new ArgumentException(StatesExceptionMessages.StateCannotBeItsOwnSuperState(this.ToString()));
            }
        }

        /// <summary>
        /// Throws an exception if the new initial state is this instance.
        /// </summary>
        /// <param name="newInitialState">The value.</param>
        // ReSharper disable once UnusedParameter.Local
        private void CheckInitialStateIsNotThisInstance(IState<TState, TEvent> newInitialState)
        {
            if (this == newInitialState)
            {
                throw new ArgumentException(StatesExceptionMessages.StateCannotBeTheInitialSubStateToItself(this.ToString()));
            }
        }

        /// <summary>
        /// Throws an exception if the new initial state is not a sub-state of this instance.
        /// </summary>
        /// <param name="value">The value.</param>
        private void CheckInitialStateIsASubState(IState<TState, TEvent> value)
        {
            if (value.SuperState != this)
            {
                throw new ArgumentException(StatesExceptionMessages.StateCannotBeTheInitialStateOfSuperStateBecauseItIsNotADirectSubState(value.ToString(), this.ToString()));
            }
        }
    }
}