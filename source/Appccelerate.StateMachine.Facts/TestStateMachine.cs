using System;
using System.Collections.Generic;
using Appccelerate.StateMachine.Machine;
using Appccelerate.StateMachine.Machine.Events;
using Appccelerate.StateMachine.Syntax;

namespace Appccelerate.StateMachine
{
    public class TestStateMachine<TState, TEvent> :
        IStateMachine<TState, TEvent>,
        INotifier<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly StateMachine<TState, TEvent> stateMachine;

        public TestStateMachine()
            : this(default(string))
        {
        }

        public TestStateMachine(string name)
        {
            var passiveExecuter = new PassiveExecuter();
            stateMachine = new StateMachine<TState, TEvent>(name ?? this.GetType().FullName, null, passiveExecuter);
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

        public IEnumerable<TState> CurrentStates
        {
            get { return stateMachine.CurrentStates; }
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

        public string Name
        {
            get { return stateMachine.Name; }
        }

        public void OnExceptionThrown(ITransitionContext<TState, TEvent> context, Exception exception)
        {
            stateMachine.OnExceptionThrown(context, exception);
        }
    }
}