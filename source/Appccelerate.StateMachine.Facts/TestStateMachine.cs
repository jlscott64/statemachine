using System;
using System.Collections.Generic;
using Appccelerate.StateMachine.Machine;
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
            var passiveExecuter = new PassiveExecuter<TState, TEvent>();
            stateMachine = new StateMachine<TState, TEvent>(name ?? this.GetType().FullName, null, passiveExecuter);
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