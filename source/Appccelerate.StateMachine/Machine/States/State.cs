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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Appccelerate.StateMachine.Machine.ActionHolders;
    using Appccelerate.StateMachine.Machine.Transitions;

    /// <summary>
    /// A state of the state machine.
    /// A state can be a sub-state or super-state of another state.
    /// </summary>
    /// <typeparam name="TState">The type of the state id.</typeparam>
    /// <typeparam name="TEvent">The type of the event id.</typeparam>
    public class State<TState, TEvent> : IState<TState, TEvent>
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

        readonly IList<IActionHolder> entryActions;
        readonly IList<IDoActionHolder> doActions;
        readonly IList<IActionHolder> exitActions;

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

            this.entryActions = new List<IActionHolder>();
            this.doActions = new List<IDoActionHolder>();
            this.exitActions = new List<IActionHolder>();
        }

        /// <summary>
        /// Gets or sets the current active state of this state.
        /// </summary>
        /// <value>The current state of the active.</value>
        public IState<TState, TEvent> ActiveState
        {
            get { return (this.regions.Any()) ? this.regions.First().ActiveState : null; }
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
        }

        public void SetLastActiveState(IState<TState, TEvent> value)
        {
            this.regions.First().LastActiveState = value;
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
        /// Gets descriptions of the entry actions.
        /// </summary>
        /// <value>The entry actions.</value>
        public IEnumerable<string> EntryActionDescriptions
        {
            get { return entryActions.Select(action => action.Describe()); }

        }

        /// <summary>
        /// Gets descriptions of the do actions.
        /// </summary>
        /// <value>The do actiona.</value>
        public IEnumerable<string> DoActionDescriptions
        {
            get { return doActions.Select(action => action.Describe()); }
        }

        /// <summary>
        /// Gets descriptions of the exit actions.
        /// </summary>
        /// <value>The exit actions.</value>
        public IEnumerable<string> ExitActionDescriptions
        {
            get { return exitActions.Select(action => action.Describe()); }
        }

        /// <summary>
        /// DEPRECATED. Gets the initial sub-state. Null if this state has no sub-states.
        /// </summary>
        /// <returns>The initial sub-state. Null if this state has no sub-states.</returns>
        public IState<TState, TEvent> GetInitialState()
        {
            return regions.Any() ? this.regions.First().InitialState : null;
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
            get { return this.superState; }
        }

        /// <summary>
        /// Gets or sets the level of this state in the state hierarchy.
        /// When set then the levels of all sub-states are changed accordingly.
        /// </summary>
        /// <value>The level.</value>
        public int Level
        {
            get { return this.level; }
        }

        /// <summary>
        /// Gets or sets the history type of this state.
        /// </summary>
        /// <value>The type of the history.</value>
        public HistoryType HistoryType
        {
            get { return this.historyType; } 
        }

        public IRegion<TState, TEvent> Region { get; private set; }

        /// <summary>
        /// Gets the sub-states of this state.
        /// </summary>
        /// <value>The sub-states of this state.</value>
        public IEnumerable<IState<TState, TEvent>> SubStates 
        {
            get { return this.regions.SelectMany(r => r.States); }
        }

        /// <summary>
        /// Gets the completion transitions.
        /// </summary>
        /// <value>The transitions.</value>
        public IList<ITransition<TState, TEvent>> CompletionTransitions
        {
            get { return this.completionTransitions; }
        }

        public IEnumerable<ITransition<TState, TEvent>> GetTransitions(TEvent eventId)
        {
            return this.transitions[eventId].NotNull();
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

            this.StopDoActions();
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

        private void ExecuteEntryActions(ITransitionContext<TState, TEvent> context)
        {
            foreach (var actionHolder in this.entryActions)
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

            var doActionTasks = this.doActions.Select(actionHolder => this.StartDoAction(actionHolder, context, cancellationToken));
            Task.WhenAll(doActionTasks).ContinueWith(t => this.DoActionsCompleted(), TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private void DoActionsCompleted()
        {
            OnCompleted(new StateCompletedEventArgs());
        }

        private Task StartDoAction(IDoActionHolder actionHolder, ITransitionContext<TState, TEvent> context, CancellationToken cancellationToken)
        {
            var doActionTask = actionHolder.Start(context.EventArgument, cancellationToken);
            doActionTask.ContinueWith(t => DoActionFailed(t, context), cancellationToken);
            return doActionTask;
        }

        private void DoActionFailed(Task doActionTask, ITransitionContext<TState, TEvent> context)
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

        private void StopDoActions()
        {
            cancellation.Cancel();
        }

        private void ExecuteExitActions(ITransitionContext<TState, TEvent> context)
        {
            foreach (var actionHolder in this.exitActions)
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

        public IEnumerable<TransitionInfo<TState, TEvent>> GetTransitions()
        {
            return this.transitions.GetTransitions();
        }

        public event EventHandler<StateCompletedEventArgs> Completed;

        public void AddEntryAction(IActionHolder action)
        {
            this.entryActions.Add(action);
        }

        public void AddDoAction(IDoActionHolder action)
        {
            this.doActions.Add(action);
        }

        public void AddExitAction(IActionHolder action)
        {
            this.exitActions.Add(action);
        }

        public void AddTransition(TEvent eventId, ITransition<TState, TEvent> transition)
        {
            this.transitions.Add(eventId, transition);
        }

        public void AddCompletionTransition(ITransition<TState, TEvent> transition)
        {
            this.completionTransitions.Add(transition);
        }

        public void SetHistoryType(HistoryType value)
        {
            this.historyType = value;
        }

        public void SetSuperState(IState<TState, TEvent> value)
        {
            this.superState = value;
        }

        public void SetId(TState value)
        {
            this.Id = value;
        }

        public void SetLevel(int value)
        {
            this.level = value;
        }

        public IRegion<TState, TEvent> AddRegion()
        {
            var region = new Region<TState, TEvent>(this);
            this.regions.Add(region);
            return region;
        }

        public void SetRegion(IRegion<TState, TEvent> value)
        {
            this.Region = value;
        }

        protected virtual void OnCompleted(StateCompletedEventArgs e)
        {
            var handler = Completed;
            if (handler != null) handler(this, e);
        }
    }
}