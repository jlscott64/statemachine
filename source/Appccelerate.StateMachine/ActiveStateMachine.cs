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
    using Appccelerate.StateMachine.Machine.Events;
    using Appccelerate.StateMachine.Machine;
    using Appccelerate.StateMachine.Persistence;
    using Appccelerate.StateMachine.Syntax;

    /// <summary>
    /// An active state machine.
    /// This state machine reacts to events on its own worker thread and the <see cref="Fire(TEvent,object)"/> or
    /// <see cref="FirePriority(TEvent,object)"/> methods return immediately back to the caller.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public class ActiveStateMachine<TState, TEvent> : IStateMachine<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly StateMachine<TState, TEvent> stateMachine;

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
        {
            var activeExecuter = new ActiveExecuter<TState, TEvent>();
            name = StateMachine<TState,TEvent>.NameOrDefault(this.GetType(), name);
            stateMachine = new StateMachine<TState, TEvent>(name, factory, activeExecuter);
        }

        public override string ToString()
        {
            return stateMachine.Name;
        }

        public event EventHandler<TransitionEventArgs<TState, TEvent>> TransitionDeclined
        {
            add { stateMachine.TransitionDeclined += value; }
            remove { stateMachine.TransitionDeclined -= value; }
        }

        public event EventHandler<TransitionExceptionEventArgs<TState, TEvent>> TransitionExceptionThrown
        {
            add { stateMachine.TransitionExceptionThrown += value; }
            remove { stateMachine.TransitionExceptionThrown -= value; }
        }

        public event EventHandler<TransitionEventArgs<TState, TEvent>> TransitionBegin
        {
            add { stateMachine.TransitionBegin += value; }
            remove { stateMachine.TransitionBegin -= value; }
        }

        public event EventHandler<TransitionCompletedEventArgs<TState, TEvent>> TransitionCompleted
        {
            add { stateMachine.TransitionCompleted += value; }
            remove { stateMachine.TransitionCompleted -= value; }
        }

        public bool IsRunning
        {
            get { return stateMachine.IsRunning; }
        }

        public IEntryActionSyntax<TState, TEvent> In(TState state)
        {
            return stateMachine.In(state);
        }

        public IHierarchySyntax<TState> DefineHierarchyOn(TState superStateId)
        {
            return stateMachine.DefineHierarchyOn(superStateId);
        }

        public IInitialSubStateSyntax<TState> DefineRegionOn(TState stateId)
        {
            return stateMachine.DefineRegionOn(stateId);
        }

        public void Fire(TEvent eventId)
        {
            stateMachine.Fire(eventId);
        }

        public void Fire(TEvent eventId, object eventArgument)
        {
            stateMachine.Fire(eventId, eventArgument);
        }

        public void FirePriority(TEvent eventId)
        {
            stateMachine.FirePriority(eventId);
        }

        public void FirePriority(TEvent eventId, object eventArgument)
        {
            stateMachine.FirePriority(eventId, eventArgument);
        }

        public void Initialize(TState initialState)
        {
            stateMachine.Initialize(initialState);
        }

        public void Start()
        {
            stateMachine.Start();
        }

        public void Stop()
        {
            stateMachine.Stop();
        }

        public void AddExtension(IExtension<TState, TEvent> extension)
        {
            stateMachine.AddExtension(extension);
        }

        public void ClearExtensions()
        {
            stateMachine.ClearExtensions();
        }

        public void Report(IStateMachineReport<TState, TEvent> reportGenerator)
        {
            stateMachine.Report(reportGenerator);
        }

        public void Save(IStateMachineSaver<TState> stateMachineSaver)
        {
            stateMachine.Save(stateMachineSaver);
        }

        public void Load(IStateMachineLoader<TState> stateMachineLoader)
        {
            stateMachine.Load(stateMachineLoader);
        }
    }
}