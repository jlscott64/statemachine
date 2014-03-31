using Appccelerate.StateMachine.Machine;

namespace Appccelerate.StateMachine
{
    public class PassiveExecuter : Executer
    {
        /// <summary>
        /// Whether this state machine is executing an event. Allows that events can be added while executing.
        /// </summary>
        private bool executing;
        private bool isRunning;

        public override bool IsRunning
        {
            get { return isRunning; }
        }

        public override void DoStart()
        {
            this.isRunning = true;
            this.Execute();
        }

        public override void DoStop()
        {
            this.isRunning = false;
        }

        public override void Execute()
        {
            ProcessQueuedEvents();
        }

        private void ProcessQueuedEvents()
        {
            if (this.executing || !this.IsRunning)
            {
                return;
            }

            this.executing = true;
            try
            {
                this.PumpEvents();
            }
            finally
            {
                this.executing = false;
            }
        }
    }
}