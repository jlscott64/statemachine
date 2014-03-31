//-------------------------------------------------------------------------------
// <copyright file="StateMachine.cs" company="Appccelerate">
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


using System.Text;

namespace Appccelerate.StateMachine.Machine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Appccelerate.StateMachine.Machine.Events;
    using Appccelerate.StateMachine.Syntax;

    public class StateMachine<TState, TEvent>
        : IStateMachine<TState, TEvent>,
            INotifier<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly string name;

        private readonly IFactory<TState, TEvent> factory;
        private readonly Executer executer;

        private readonly IStateDictionary<TState, TEvent> states;
        private readonly IList<IState<TState, TEvent>> currentStates;
        private readonly Initializable<TState> initialStateId;

        private bool initialized;

        public StateMachine(string name, IFactory<TState, TEvent> factory, Executer executer)
        {
            this.name = name;

            this.factory = factory ?? new StandardFactory<TState, TEvent>(this);
            this.executer = executer;

            this.states = new StateDictionary<TState, TEvent>(this.factory);
            this.currentStates = new List<IState<TState, TEvent>>();
            this.initialStateId = new Initializable<TState>();
        }

        /// <summary>
        /// Occurs when no transition could be executed.
        /// </summary>
        public event EventHandler<TransitionEventArgs<TState, TEvent>> TransitionDeclined;

        /// <summary>
        /// Occurs when an exception was thrown inside a transition of the state machine.
        /// </summary>
        public event EventHandler<TransitionExceptionEventArgs<TState, TEvent>> TransitionExceptionThrown;

        /// <summary>
        /// Occurs when a transition begins.
        /// </summary>
        public event EventHandler<TransitionEventArgs<TState, TEvent>> TransitionBegin;

        /// <summary>
        /// Occurs when a transition completed.
        /// </summary>
        public event EventHandler<TransitionCompletedEventArgs<TState, TEvent>> TransitionCompleted;

        public bool IsRunning
        {
            get { return executer.IsRunning; }
        }

        /// <summary>
        /// Define the behavior of a state.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns>Syntax to build state behavior.</returns>
        public IEntryActionSyntax<TState, TEvent> In(TState state)
        {
            var newState = this.states[state];
            newState.Completed += OnStateCompleted;
            return new StateBuilder<TState, TEvent>(newState, this.states, this.factory);
        }

        /// <summary>
        /// Defines the hierarchy on.
        /// </summary>
        /// <param name="superStateId">The super state id.</param>
        /// <returns>Syntax to build a state hierarchy.</returns>
        public IHierarchySyntax<TState> DefineHierarchyOn(TState superStateId)
        {
            return new HierarchyBuilder<TState, TEvent>(this.states, superStateId);
        }

        /// <summary>
        /// Defines a region on a state.
        /// </summary>
        /// <param name="stateId">The state id.</param>
        /// <returns>Syntax to build hierarchy.</returns>
        public IInitialSubStateSyntax<TState> DefineRegionOn(TState stateId)
        {
            return new HierarchyBuilder<TState, TEvent>(this.states, stateId);
        }

        /// <summary>
        /// Fires the specified event.
        /// </summary>
        /// <param name="eventId">The event.</param>
        public void Fire(TEvent eventId)
        {
            this.Fire(eventId, null);
        }

        /// <summary>
        /// Fires the specified event.
        /// </summary>
        /// <param name="eventId">The event.</param>
        /// <param name="eventArgument">The event argument.</param>
        public virtual void Fire(TEvent eventId, object eventArgument)
        {
            this.AssertThatStateMachineIsInitialized();

            executer.Enqueue(() => this.DoFire(eventId, eventArgument));

            this.Execute();
        }

        /// <summary>
        /// Fires the specified priority event. The event will be handled before any already queued event.
        /// </summary>
        /// <param name="eventId">The event.</param>
        public void FirePriority(TEvent eventId)
        {
            this.FirePriority(eventId, null);
        }

        /// <summary>
        /// Fires the specified priority event. The event will be handled before any already queued event.
        /// </summary>
        /// <param name="eventId">The event.</param>
        /// <param name="eventArgument">The event argument.</param>
        public virtual void FirePriority(TEvent eventId, object eventArgument)
        {
            this.AssertThatStateMachineIsInitialized();

            executer.PriorityEnqueue(() => this.DoFire(eventId, eventArgument));

            this.Execute();
        }

        /// <summary>
        /// Initializes the state machine to the specified initial state.
        /// </summary>
        /// <param name="initialState">The state to which the state machine is initialized.</param>
        public void Initialize(TState initialState)
        {
            this.AssertThatStateMachineIsNotAlreadyInitialized();

            this.initialStateId.Value = this.states[initialState].Id;
            executer.PriorityEnqueue(this.EnterInitialState);

            this.initialized = true;
        }

        public IState<TState, TEvent> LastActiveState { get; set; }

        protected int QueuedEventCount
        {
            get { return executer.QueuedEventCount; }
        }

        public void OnExceptionThrown(ITransitionContext<TState, TEvent> context, Exception exception)
        {
            RethrowExceptionIfNoHandlerRegistered(exception, this.TransitionExceptionThrown);

            this.RaiseEvent(this.TransitionExceptionThrown,
                new TransitionExceptionEventArgs<TState, TEvent>(context, exception), context, false);
        }

        /// <summary>
        /// Gets the ids of the current states.
        /// </summary>
        /// <value>The ids of the current states.</value>
        public IEnumerable<TState> CurrentStates
        {
            get { return this.currentStates.Select(s => s.Id).ToArray(); }
        }

        /// <summary>
        /// Gets the name of this instance.
        /// </summary>
        /// <value>The name of this instance.</value>
        public string Name
        {
            get { return this.name; }
        }

        private void AssertThatStateMachineIsInitialized()
        {
            if (!this.initialized)
            {
                throw new InvalidOperationException(ExceptionMessages.StateMachineNotInitialized);
            }
        }

        private void AssertThatStateMachineIsNotAlreadyInitialized()
        {
            if (this.initialized)
            {
                throw new InvalidOperationException(ExceptionMessages.StateMachineIsAlreadyInitialized);
            }
        }

        private void ChangeState(IState<TState, TEvent> oldState, IState<TState, TEvent> newState)
        {
            var indexOf = currentStates.IndexOf(oldState);
            if (indexOf == -1)
            {
                currentStates.Add(newState);
            }
            else
            {
                currentStates[indexOf] = newState;
            }
        }

        private void ChangeStates(IState<TState, TEvent> oldState, IEnumerable<IState<TState, TEvent>> newStates)
        {
            foreach (var newState in newStates)
                this.ChangeState(oldState, newState);
        }

        protected void DoFire(TEvent eventId, object eventArgument)
        {
            this.AssertThatStateMachineIsInitialized();

            var fired = false;
            foreach (var pair in GetTransitionsToFire(eventId, eventArgument))
            {
                var transition = pair.Item1;
                var context = pair.Item2;

                DoFire(transition, context);

                fired = true;
            }

            if (!fired)
            {
                var missableEventId = new Missable<TEvent>(eventId);

                foreach (
                    var context in
                        this.currentStates.Select(
                            state => this.factory.CreateTransitionContext(state, missableEventId, eventArgument)))
                {
                    this.OnTransitionDeclined(context);
                }
            }
        }

        private void DoFire(ITransition<TState, TEvent> transition, ITransitionContext<TState, TEvent> context)
        {
            this.OnTransitionBegin(context);

            var result = transition.Fire(context);
            this.ChangeStates(context.SourceState, result.NewStates);

            this.OnTransitionCompleted(context, result.NewStates.First().Id);
        }

        /// <summary>
        /// Enters the initial state that was previously set with <see cref="Initialize(TState)"/>.
        /// </summary>
        private void EnterInitialState()
        {
            this.AssertThatStateMachineIsInitialized();
            var stateId = this.initialStateId.Value;

            var context = this.factory.CreateTransitionContext(null, new Missable<TEvent>(), Missing.Value);
            var initializer = this.factory.CreateStateMachineInitializer(this.states[stateId], context);
            this.ChangeStates(null, initializer.EnterInitialStates());
        }

        protected void Execute()
        {
            executer.Execute();
        }

        /// <summary>
        /// Starts the state machine. Events will be processed.
        /// If the state machine is not started then the events will be queued until the state machine is started.
        /// Already queued events are processed.
        /// </summary>
        public void Start()
        {
            this.AssertThatStateMachineIsInitialized();
            if (executer.Start())
            {
            }
        }

        /// <summary>
        /// Stops the state machine. Events will be queued until the state machine is started.
        /// </summary>
        public void Stop()
        {
            if (executer.Stop())
            {
            }
        }

        private ITransition<TState, TEvent> GetTransitionToFire(IState<TState, TEvent> state,
            ITransitionContext<TState, TEvent> context)
        {
            ITransition<TState, TEvent> result = null;

            var transitionsForEvent = state.GetTransitions(context.EventId.Value);

            foreach (ITransition<TState, TEvent> transition in transitionsForEvent)
            {
                if (transition.WillFire(context))
                {
                    result = transition;
                    break;
                }
            }

            return result;
        }

        private IEnumerable<Tuple<ITransition<TState, TEvent>, ITransitionContext<TState, TEvent>>> GetTransitionsToFire
            (TEvent eventId, object eventArgument)
        {
            var missableEventId = new Missable<TEvent>(eventId);

            var stateArray = this.currentStates.OrderByDescending(s => s.Level).ThenBy(s => s.Id).ToArray();
            var levels = stateArray.Select(s => s == null ? 0 : s.Level).ToArray();
            var eventConsumed = stateArray.Select(s => false).ToArray();
            var contexts =
                stateArray.Select(s => this.factory.CreateTransitionContext(s, missableEventId, eventArgument))
                    .ToArray();

            var start = 0;
            var end = stateArray.Count() - 1;

            for (var targetLevel = levels[start] - 1;
                start <= end && targetLevel >= 0;
                targetLevel--)
            {
                // Invariant: At this point, the states list, from index "start",
                // is ordered by descending level and then by state id.

                // Delete duplicates at the start of the list.
                while (start < end && stateArray[start] == stateArray[start + 1])
                {
                    eventConsumed[start + 1] = eventConsumed[start] || eventConsumed[start + 1];
                    start++;
                }

                // Invariant: At this point, the states list, from index "start",
                // is still ordered by descending level and then by state id.

                // For the first state and all the states at the same level,
                // visit the state and replace it in the list with its superstate.
                for (var index = start;
                    index <= end && levels[index] == targetLevel + 1;
                    index++)
                {
                    var currentState = stateArray[index];

                    if (!eventConsumed[index])
                    {
                        var newTransition = GetTransitionToFire(currentState, contexts[index]);
                        if (newTransition != null)
                        {
                            eventConsumed[index] = true;
                            yield return Tuple.Create(newTransition, contexts[index]);
                        }
                    }

                    stateArray[index] = currentState.SuperState;
                    levels[index]--;
                }

                // Invariant: At this point, the states list, from index "start",
                // is ordered by descending level and then by state id.
            }
        }

        private void OnStateCompleted(object sender, StateCompletedEventArgs args)
        {
            var state = (IState<TState, TEvent>) sender;
            var context = this.factory.CreateTransitionContext(state, new Missable<TEvent>(), null);
            var transition = state.CompletionTransitions.SingleOrDefault(t => t.WillFire(context));

            if (transition != null)
            {
                executer.PriorityEnqueue(() => this.DoFire(transition, context));

                this.Execute();
            }
        }

        /// <summary>
        /// Fires the <see cref="TransitionBegin"/> event.
        /// </summary>
        /// <param name="transitionContext">The transition context.</param>
        private void OnTransitionBegin(ITransitionContext<TState, TEvent> transitionContext)
        {
            this.RaiseEvent(this.TransitionBegin, new TransitionEventArgs<TState, TEvent>(transitionContext),
                transitionContext, true);
        }

        /// <summary>
        /// Fires the <see cref="TransitionCompleted"/> event.
        /// </summary>
        /// <param name="transitionContext">The transition event context.</param>
        /// <param name="currentStateId"></param>
        private void OnTransitionCompleted(ITransitionContext<TState, TEvent> transitionContext, TState currentStateId)
        {
            this.RaiseEvent(this.TransitionCompleted,
                new TransitionCompletedEventArgs<TState, TEvent>(currentStateId, transitionContext), transitionContext,
                true);
        }

        /// <summary>
        /// Fires the <see cref="TransitionDeclined"/> event.
        /// </summary>
        /// <param name="transitionContext">The transition event context.</param>
        private void OnTransitionDeclined(ITransitionContext<TState, TEvent> transitionContext)
        {
            this.RaiseEvent(this.TransitionDeclined, new TransitionEventArgs<TState, TEvent>(transitionContext),
                transitionContext, true);
        }

        private void RaiseEvent<T>(EventHandler<T> eventHandler, T arguments, ITransitionContext<TState, TEvent> context,
            bool raiseEventOnException) where T : EventArgs
        {
            try
            {
                if (eventHandler == null)
                {
                    return;
                }

                eventHandler(this, arguments);
            }
            catch (Exception e)
            {
                if (!raiseEventOnException)
                {
                    throw;
                }

                ((INotifier<TState, TEvent>) this).OnExceptionThrown(context, e);
            }
        }

        // ReSharper disable once UnusedParameter.Local
        private static void RethrowExceptionIfNoHandlerRegistered<T>(Exception exception,
            EventHandler<T> exceptionHandler) where T : EventArgs
        {
            if (exceptionHandler == null)
            {
                throw new StateMachineException("No exception listener is registered. Exception: ", exception);
            }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return this.Name ?? this.GetType().FullName;
        }

        public static string NameOrDefault(Type containerType, string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }

            var stringBuilder = new StringBuilder();
            BuildTypeName(containerType, stringBuilder);

            return stringBuilder.ToString();
        }

        private static void BuildTypeName(Type type, StringBuilder stringBuilder)
        {   
            if (type.IsGenericTypeDefinition)
            {
                var genericTypeName = type.FullName;
                var length = genericTypeName.LastIndexOf('`');
                length = length != -1 ? length : genericTypeName.Length;
                stringBuilder.Append(genericTypeName.Substring(0, length));
            }
            else if (type.IsGenericType)
            {
                BuildTypeName(type.GetGenericTypeDefinition(), stringBuilder);
                stringBuilder.Append("<");

                var delimiter = "";
                foreach (var typeParameter in type.GetGenericArguments())
                {
                    stringBuilder.Append(delimiter);
                    delimiter = ",";

                    BuildTypeName(typeParameter, stringBuilder);
                }

                stringBuilder.Append(">");
            }
            else
            {
                stringBuilder.Append(type.FullName);
            }
        }
    }
}