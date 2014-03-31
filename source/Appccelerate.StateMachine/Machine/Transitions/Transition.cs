//-------------------------------------------------------------------------------
// <copyright file="Transition.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Machine.Transitions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using Appccelerate.StateMachine.Machine.ActionHolders;
    using Appccelerate.StateMachine.Machine.GuardHolders;

    public class Transition<TState, TEvent>
        : ITransition<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly List<IActionHolder> actions;
        private readonly IExtensionHost<TState, TEvent> extensionHost;
        private readonly IStateMachineInformation<TState, TEvent> stateMachineInformation;
        private readonly INotifier<TState, TEvent> notifier;

        public Transition(IStateMachineInformation<TState, TEvent> stateMachineInformation, INotifier<TState, TEvent> notifier, IExtensionHost<TState, TEvent> extensionHost)
        {
            this.stateMachineInformation = stateMachineInformation;
            this.notifier = notifier;
            this.extensionHost = extensionHost;

            this.actions = new List<IActionHolder>();
        }

        public IState<TState, TEvent> Source { get; set; }

        public IState<TState, TEvent> Target { get; set; }

        public IGuardHolder Guard { get; set; }

        public ICollection<IActionHolder> Actions
        {
            get { return this.actions; }
        }

        private bool IsInternalTransition
        {
            get { return this.Target == null; }
        }

        public ITransitionResult<TState, TEvent> Fire(ITransitionContext<TState, TEvent> context)
        {
            Ensure.ArgumentNotNull(context, "context");

            IEnumerable<IState<TState, TEvent>> newStates;

            Action<ITransitionContext<TState, TEvent>> transitionAction = this.PerformActions;
            if (!this.IsInternalTransition)
            {
                var sourceState = context.SourceState;
                var targetState = this.Target;

                var traversal = new Traversal<TState, TEvent>();

                newStates = traversal.ExecuteTraversal(context, sourceState, targetState, transitionAction);
            }
            else
            {
                transitionAction(context);
                newStates = new[] {context.SourceState};
            }

            this.extensionHost.ForEach(extension => extension.ExecutedTransition(
                this.stateMachineInformation, 
                this,
                context));

            return new TransitionResult<TState, TEvent>(true, newStates);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Transition from state {0} to state {1}.", this.Source, this.Target);
        }

        private void HandleException(Exception exception, ITransitionContext<TState, TEvent> context)
        {
            notifier.OnExceptionThrown(context, exception);
        }

        /// <summary>
        /// Indicates if the transition will fire in the given context.
        /// </summary>
        /// <param name="context">The event context.</param>
        /// <returns>True if the transition will fire.</returns>
        public bool WillFire(ITransitionContext<TState, TEvent> context)
        {
            try
            {
                return this.Guard == null || this.Guard.Execute(context.EventArgument);
            }
            catch (Exception exception)
            {
                this.extensionHost.ForEach(extention => extention.HandlingGuardException(this.stateMachineInformation, this, context, ref exception));
                
                HandleException(exception, context);

                this.extensionHost.ForEach(extention => extention.HandledGuardException(this.stateMachineInformation, this, context, exception));

                return false;
            }
        }

        private void PerformActions(ITransitionContext<TState, TEvent> context)
        {
            foreach (IActionHolder action in this.actions)
            {
                try
                {
                    action.Execute(context.EventArgument);
                }
                catch (Exception exception)
                {
                    this.extensionHost.ForEach(extension => extension.HandlingTransitionException(this.stateMachineInformation, this, context, ref exception));
                    
                    HandleException(exception, context);

                    this.extensionHost.ForEach(extension => extension.HandledTransitionException(this.stateMachineInformation, this, context, exception));
                }
            }
        }
    }
}