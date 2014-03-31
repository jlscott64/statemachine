using System;
using System.Threading;
using System.Threading.Tasks;
using Appccelerate.StateMachine.Machine;

namespace Appccelerate.StateMachine
{
    public class ActiveExecuter : Executer
    {
        private readonly AutoResetEvent eventActionQueued;
        private CancellationTokenSource stopToken;
        private Task worker;

        public ActiveExecuter()
        {
            this.eventActionQueued = new AutoResetEvent(false);
        }

        public override bool IsRunning
        {
            get { return this.worker != null && !this.worker.IsCompleted; }
        }

        public override bool IsStopping
        {
            get { return this.IsRunning && this.stopToken.IsCancellationRequested; }
        }

        public override void DoStart()
        {
            this.stopToken = new CancellationTokenSource();
            this.worker = Task.Factory.StartNew(
                () => this.ProcessQueuedEvents(this.stopToken.Token),
                this.stopToken.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        public override void DoStop()
        {
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
        }

        public override void Execute()
        {
            this.eventActionQueued.Set();
        }

        private void ProcessQueuedEvents(CancellationToken cancellationToken)
        {
            var signals = new[] {this.eventActionQueued, cancellationToken.WaitHandle};

            while (!cancellationToken.IsCancellationRequested)
            {
                this.PumpEvents();

                WaitHandle.WaitAny(signals);
            }
        }
    }
}