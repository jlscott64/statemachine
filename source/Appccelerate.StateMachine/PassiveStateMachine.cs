//-------------------------------------------------------------------------------
// <copyright file="PassiveStateMachine.cs" company="Appccelerate">
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

    using Appccelerate.StateMachine.Machine.Events;

    /// <summary>
    /// A passive state machine.
    /// This state machine reacts to events on the current thread.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public class PassiveStateMachine<TState, TEvent> : StateMachine<TState, TEvent>, IStateMachine<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        /// <summary>
        /// Whether this state machine is executing an event. Allows that events can be added while executing.
        /// </summary>
        private bool executing;


        bool isRunning;

        /// <summary>
        /// Initializes a new instance of the <see cref="PassiveStateMachine&lt;TState, TEvent&gt;"/> class.
        /// </summary>
        public PassiveStateMachine()
            : this(default(string))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PassiveStateMachine{TState, TEvent}"/> class.
        /// </summary>
        /// <param name="name">The name of the state machine. Used in log messages.</param>
        public PassiveStateMachine(string name)
            : this(name, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PassiveStateMachine{TState, TEvent}"/> class.
        /// </summary>
        /// <param name="name">The name of the state machine. Used in log messages.</param>
        /// <param name="factory">The factory used to build up internals. Pass your own factory to change the behavior of the state machine.</param>
        public PassiveStateMachine(string name, IFactory<TState, TEvent> factory)
            :base(name, factory)
        {
        }

        /// <summary>
        /// Gets a value indicating whether this instance is running. The state machine is running if if was started and not yet stopped.
        /// </summary>
        /// <value><c>true</c> if this instance is running; otherwise, <c>false</c>.</value>
        public override bool IsRunning
        {
            get { return isRunning; }
        }

        protected override void DoStart()
        {
            this.isRunning = true;
            this.Execute();
        }

        protected override void DoStop()
        {
            this.isRunning = false;

            this.ForEach(extension => extension.StoppedStateMachine(this));
        }

        /// <summary>
        /// Executes all queued events.
        /// </summary>
        protected override void Execute()
        {
            if (this.executing || !this.IsRunning)
            {
                return;
            }

            this.executing = true;
            try
            {
                this.ProcessQueuedEvents();
            }
            finally
            {
                this.executing = false;
            }
        }

        private void ProcessQueuedEvents()
        {
            while (QueuedEventCount > 0)
            {
                var eventAction = GetNextEventAction();
                eventAction();
            }
        }
    }
}