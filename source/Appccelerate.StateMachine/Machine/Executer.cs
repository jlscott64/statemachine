using System;
using System.Collections.Concurrent;

namespace Appccelerate.StateMachine.Machine
{
    public abstract class Executer
    {
        private readonly ConcurrentQueue<Action> eventActionQueue;
        private readonly ConcurrentQueue<Action> highPriortyEventActionQueue;

        protected Executer()
        {
            this.eventActionQueue = new ConcurrentQueue<Action>();
            this.highPriortyEventActionQueue = new ConcurrentQueue<Action>();
        }

        public void Enqueue(Action action)
        {
            this.eventActionQueue.Enqueue(action);
        }

        public void PriorityEnqueue(Action action)
        {
            this.highPriortyEventActionQueue.Enqueue(action);
        }

        public int QueuedEventCount
        {
            get { return this.eventActionQueue.Count + this.highPriortyEventActionQueue.Count; }
        }

        public virtual bool IsStopping { get { return false; } }
        public abstract bool IsRunning { get; }

        public abstract void DoStart();
        public abstract void DoStop();
        public abstract void Execute();

        /// <summary>
        /// Starts the state machine. Events will be processed.
        /// If the state machine is not started then the events will be queued until the state machine is started.
        /// Already queued events are processed.
        /// </summary>
        public bool Start()
        {
            if (this.IsRunning)
            {
                return false;
            }

            this.DoStart();
            this.Execute();

            return true;
        }

        /// <summary>
        /// Stops the state machine. Events will be queued until the state machine is started.
        /// </summary>
        public bool Stop()
        {
            if (!this.IsRunning || this.IsStopping)
            {
                return false;
            }

            DoStop();

            return true;
        }

        public Action GetNextEventAction()
        {
            Action eventAction;

            // Check the high priority queue first
            if (this.highPriortyEventActionQueue.TryDequeue(out eventAction))
            {
                return eventAction;
            }

            // Then look for a non-priority
            if (this.eventActionQueue.TryDequeue(out eventAction))
            {
                return eventAction;
            }

            return null;
        }

        public void PumpEvents()
        {
            Action eventAction;

            while (this.IsRunning
                   && !this.IsStopping
                   && (eventAction = this.GetNextEventAction()) != null)
            {
                eventAction();
            }
        }
    }
}