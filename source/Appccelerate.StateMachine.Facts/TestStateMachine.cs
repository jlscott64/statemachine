using System;
using Appccelerate.StateMachine.Machine;

namespace Appccelerate.StateMachine
{
    public class TestStateMachine<TStates, TEvents> : StateMachine<TStates, TEvents> 
        where TStates : IComparable
        where TEvents : IComparable
    {
        bool isRunning;

        public TestStateMachine(string name)
            : base(name)
        {
        }

        public TestStateMachine()
        {
        }

        public override bool IsRunning
        {
            get { return isRunning; }
        }

        protected override void DoStart()
        {
            this.isRunning = true;
        }

        protected override void DoStop()
        {
            this.isRunning = false;
        }

        protected override void Execute()
        {
            PumpEvents();
        }
    }
}