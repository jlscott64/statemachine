//-------------------------------------------------------------------------------
// <copyright file="ActiveStateMachine.cs" company="Appccelerate">
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
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    using Appccelerate.StateMachine.Machine.Events;

    /// <summary>
    /// An active state machine.
    /// This state machine reacts to events on its own worker thread and the <see cref="Fire(TEvent,object)"/> or
    /// <see cref="FirePriority(TEvent,object)"/> methods return immediately back to the caller.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public class ActiveStateMachine<TState, TEvent> : StateMachineBase<TState, TEvent>, IStateMachine<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly ConcurrentQueue<Action> eventActionQueue;
        private readonly ConcurrentQueue<Action> highPriortyEventActionQueue;
        private readonly AutoResetEvent eventActionQueued;

        private CancellationTokenSource stopToken;
        private Task worker;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveStateMachine{TState, TEvent}"/> class.
        /// </summary>
        public ActiveStateMachine()
            : this(default(string))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveStateMachine{TState, TEvent}"/> class.
        /// </summary>
        /// <param name="name">The name of the state machine. Used in log messages.</param>
        public ActiveStateMachine(string name)
            : this(name, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveStateMachine{TState, TEvent}"/> class.
        /// </summary>
        /// <param name="name">The name of the state machine. Used in log messages.</param>
        /// <param name="factory">The factory used to build up internals. Pass your own factory to change the behavior of the state machine.</param>
        public ActiveStateMachine(string name, IFactory<TState, TEvent> factory)
            :base(name, factory)
        {
            this.eventActionQueue = new ConcurrentQueue<Action>();
            this.highPriortyEventActionQueue = new ConcurrentQueue<Action>();
            this.eventActionQueued = new AutoResetEvent(false);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is running. The state machine is running if if was started and not yet stopped.
        /// </summary>
        /// <value><c>true</c> if this instance is running; otherwise, <c>false</c>.</value>
        public override bool IsRunning
        {
            get { return this.worker != null && !this.worker.IsCompleted; }
        }

        /// <summary>
        /// Fires the specified event.
        /// </summary>
        /// <param name="eventId">The event.</param>
        /// <param name="eventArgument">The event argument.</param>
        public override void Fire(TEvent eventId, object eventArgument)
        {
            this.eventActionQueue.Enqueue(() => this.stateMachine.Fire(eventId, eventArgument));
            this.eventActionQueued.Set();

            this.stateMachine.ForEach(extension => extension.EventQueued(this.stateMachine, eventId, eventArgument));
        }

        /// <summary>
        /// Fires the specified priority event. The event will be handled before any already queued event.
        /// </summary>
        /// <param name="eventId">The event.</param>
        /// <param name="eventArgument">The event argument.</param>
        public override void FirePriority(TEvent eventId, object eventArgument)
        {
            this.highPriortyEventActionQueue.Enqueue(() => this.stateMachine.Fire(eventId, eventArgument));
            this.eventActionQueued.Set();

            this.stateMachine.ForEach(extension => extension.EventQueuedWithPriority(this.stateMachine, eventId, eventArgument));
        }

        /// <summary>
        /// Starts the state machine. Events will be processed.
        /// If the state machine is not started then the events will be queued until the state machine is started.
        /// Already queued events are processed.
        /// </summary>
        public override void Start()
        {
            this.CheckThatStateMachineIsInitialized();

            if (this.IsRunning)
            {
                return;
            }

            this.stopToken = new CancellationTokenSource();
            this.worker = Task.Factory.StartNew(
                () => this.ProcessQueuedEvents(this.stopToken.Token),
                this.stopToken.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            this.stateMachine.ForEach(extension => extension.StartedStateMachine(this.stateMachine));
        }

        /// <summary>
        /// Stops the state machine. Events will be queued until the state machine is started.
        /// </summary>
        public override void Stop()
        {
            if (!this.IsRunning || this.stopToken.IsCancellationRequested)
            {
                return;
            }
            
            this.stopToken.Cancel();
            
            try
            {
                this.worker.Wait();
            }
            catch (AggregateException)
            {
                // in case the task was stopped before it could actually start, it will be canceled.
                if (this.worker.IsFaulted)
                {
                    throw;
                }
            }

            this.stateMachine.ForEach(extension => extension.StoppedStateMachine(this.stateMachine));
        }

        private void ProcessQueuedEvents(CancellationToken cancellationToken)
        {
            this.InitializeStateMachineIfInitializationIsPending();

            var signals = new[] {this.eventActionQueued, cancellationToken.WaitHandle};

            while (!cancellationToken.IsCancellationRequested)
            {
                Action eventAction;

                // Empty the high priority queue first
                while (this.highPriortyEventActionQueue.TryDequeue(out eventAction))
                {
                    eventAction();
                }

                // Then look for a non-priority
                if (this.eventActionQueue.TryDequeue(out eventAction))
                {
                    eventAction();
                }

                WaitHandle.WaitAny(signals);
            }
        }
    }
}