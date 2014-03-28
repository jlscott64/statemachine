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

using Appccelerate.StateMachine.Machine;

namespace Appccelerate.StateMachine
{
    using System;
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
    public class ActiveStateMachine<TState, TEvent> : StateMachine<TState, TEvent>, IStateMachine<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
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

            this.ForEach(extension => extension.StartedStateMachine(this));
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

            this.ForEach(extension => extension.StoppedStateMachine(this));
        }

        protected override void Execute()
        {
            this.eventActionQueued.Set();
        }

        private void ProcessQueuedEvents(CancellationToken cancellationToken)
        {
            this.InitializeStateMachineIfInitializationIsPending();

            var signals = new[] {this.eventActionQueued, cancellationToken.WaitHandle};

            while (!cancellationToken.IsCancellationRequested)
            {
                Action eventAction = GetNextEventAction();

                while (eventAction != null && !cancellationToken.IsCancellationRequested)
                {
                    eventAction();
                    eventAction = GetNextEventAction();
                }
                
                WaitHandle.WaitAny(signals);
            }
        }
    }
}