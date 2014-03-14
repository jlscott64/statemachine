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
    using System.Linq;

    public class Traversal<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        public IState<TState, TEvent> ExecuteTraversal(ITransitionContext<TState, TEvent> context, IState<TState, TEvent> currentFromState, IState<TState, TEvent> currentToState,
            IState<TState, TEvent> destinationState, Action<ITransitionContext<TState, TEvent>> transitionAction)
        {
            var exitedStates = new List<IState<TState, TEvent>>();
            var enteredStates = new List<IState<TState, TEvent>>();

            GetStateChanges(currentFromState, currentToState, destinationState, exitedStates, enteredStates);
            Foobar(enteredStates, currentToState);
            
            exitedStates.ForEach(s => s.Exit(context));
            transitionAction(context);
            enteredStates.ForEach(s => s.Entry(context));

            return enteredStates.Last();
        }

        /// <summary>
        /// Recursively traverses the state hierarchy from the source to the target,
        /// On the way down, it collects the states to be exited, and on the way up,
        /// it collects the states to be entered.
        /// </summary>
        /// <remarks>
        /// There exist the following transition scenarios:
        /// 0. there is no target state (internal transition)
        ///    --> handled outside this method.
        /// 1. The source and target state are the same (self transition)
        ///    --> perform the transition directly:
        ///        Exit source state, and enter target state
        /// 2. The target state is a direct or indirect sub-state of the source state
        ///    --> then traverse the hierarchy 
        ///        from the source state down to the target state,
        ///        entering each state along the way.
        ///        No state is exited.
        /// 3. The source state is a sub-state of the target state
        ///    --> traverse the hierarchy from the source up to the target, 
        ///        exiting each state along the way. 
        ///        Finally enter the target state.
        /// 4. The source and target state share the same super-state
        /// 5. All other scenarios:
        ///    a. The source and target states reside at the same level in the hierarchy 
        ///       but do not share the same direct super-state
        ///    --> exit the source state, move up the hierarchy on both sides and enter the target state
        ///    b. The source state is lower in the hierarchy than the target state
        ///    --> exit the source state and move up the hierarchy on the source state side
        ///    c. The target state is lower in the hierarchy than the source state
        ///    --> move up the hierarchy on the target state side, afterward enter target state
        /// </remarks>
        /// <param name="currentFromState">The source state.</param>
        /// <param name="currentToState">The target state.</param>
        /// <param name="finalDestinationState"></param>
        /// <param name="exitedStates">The list of states to be exited.</param>
        /// <param name="enteredStates">The list of states to be entered.</param>
        public void GetStateChanges(IState<TState, TEvent> currentFromState, IState<TState, TEvent> currentToState, IState<TState, TEvent> finalDestinationState, IList<IState<TState, TEvent>> exitedStates, IList<IState<TState, TEvent>> enteredStates)
        {
            if (currentFromState == finalDestinationState)
            {
                // Handles 1.
                // Handles 3. after traversing from the source to the target.
                exitedStates.Add(currentFromState);
                enteredStates.Add(currentFromState);
            }
            else if (currentFromState == currentToState)
            {
                // Handles 2. after traversing from the target to the source.
                // Do nothing.
            }
            else if (currentFromState.SuperState == currentToState.SuperState)
            {
                //// Handles 4.
                //// Handles 5a. after traversing the hierarchy until a common ancestor if found.
                exitedStates.Add(currentFromState);
                enteredStates.Add(currentToState);
            }
            else
            {
                // traverses the hierarchy until one of the above scenarios is met.

                // Handles 3.
                // Handles 5b.
                if (currentFromState.Level > currentToState.Level)
                {
                    exitedStates.Add(currentFromState);
                    GetStateChanges(currentFromState.SuperState, currentToState, finalDestinationState, exitedStates, enteredStates);
                }
                else if (currentFromState.Level < currentToState.Level)
                {
                    // Handles 2.
                    // Handles 5c.
                    GetStateChanges(currentFromState, currentToState.SuperState, finalDestinationState, exitedStates, enteredStates);
                    enteredStates.Add(currentToState);
                }
                else
                {
                    // Handles 5a.
                    exitedStates.Add(currentFromState);
                    GetStateChanges(currentFromState.SuperState, currentToState.SuperState, finalDestinationState, exitedStates, enteredStates);
                    enteredStates.Add(currentToState);
                }
            }
        }

        static void Foobar(ICollection<IState<TState, TEvent>> states, IState<TState, TEvent> state)
        {
            switch (state.HistoryType)
            {
                case HistoryType.None:
                    FoobarInitial(states, state);
                    break;

                case HistoryType.Shallow:
                    FoobarShallow(states, state);
                    break;

                case HistoryType.Deep:
                    FoobarDeep(states, state);
                    break;
            }
        }

        static void FoobarInitial(ICollection<IState<TState, TEvent>> states, IState<TState, TEvent> state)
        {
            if (state.HasInitialState())
            {
                var nextState = state.InitialStates.First();
                states.Add(nextState);
                FoobarInitial(states, nextState);
            }
        }

        static void FoobarShallow(ICollection<IState<TState, TEvent>> states, IState<TState, TEvent> state)
        {
            if (state.LastActiveState != null)
            {
                var nextState = state.LastActiveState;
                states.Add(nextState);
                FoobarInitial(states, nextState);
            }
            else
            {
                FoobarInitial(states, state);
            }
        }

        static void FoobarDeep(ICollection<IState<TState, TEvent>> states, IState<TState, TEvent> state)
        {
            if (state.LastActiveState != null)
            {
                var nextState = state.LastActiveState;
                states.Add(nextState);
                FoobarDeep(states, nextState);
            }
            else
            {
                FoobarInitial(states, state);
            }
        }
    }
}