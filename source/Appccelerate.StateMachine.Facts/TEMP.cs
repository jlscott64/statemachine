using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Appccelerate.StateMachine.Machine;
using Appccelerate.StateMachine.Machine.States;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Appccelerate.StateMachine
{
    public class TEMP
    {
        string[] ids =
        {
            "A",
                "A1",
                    "A1A",
                "A2",
                    "A2A",
                        "A2A1",
                    "A2B",
                        "A2B1",
                            "A2B1A",
            "B",
                "B1",
            "C",
                "C1",
        };

        IList<string> visitList = new List<string>();

        [Fact]
        public void Test2()
        {
            var states = ids.Select(MakeFakeState).ToArray();
            var dict = states.ToDictionary(s => s.Id);
            AssignSuperStates(dict);

            var currentStates = new[] { "B", "A2B1A", "A1A", "C1", "A2A1" }.Select(s => dict[s]).ToArray();

            TraverseActiveStateTree2(currentStates);

            var visits = string.Join(", ", visitList);

            var ord = visitList.Select((s, i) => new { S = s, I = i }).ToDictionary(t => t.S, t => t.I);

            ord["A1"].Should().BeLessThan(ord["A"]);
            ord["A2"].Should().BeLessThan(ord["A"]);
            ord["A1A"].Should().BeLessThan(ord["A1"]);
            ord["A2A"].Should().BeLessThan(ord["A2"]);
            ord["A2A1"].Should().BeLessThan(ord["A2A"]);
            ord["A2B"].Should().BeLessThan(ord["A2"]);
            ord["A2B1"].Should().BeLessThan(ord["A2B"]);
            ord["A2B1A"].Should().BeLessThan(ord["A2B1"]);
            //ord["B1"].Should().BeLessThan(ord["B"]);
            ord["C1"].Should().BeLessThan(ord["C"]);
        }

        void TraverseActiveStateTree2(IEnumerable<IState<string, int>> currentStates)
        {
            var states = currentStates.OrderByDescending(s => s.Level).ThenBy(s => s.Id).ToArray();
            var levels = states.Select(s => s == null ? 0 : s.Level).ToArray();
            var eventConsumed = states.Select(s => false).ToArray();
 
            var start = 0;
            var end = states.Count() - 1;

            for (var targetLevel = levels[start] - 1; 
                start <= end && targetLevel >= 0; 
                targetLevel--)
            {
                // Invariant: At this point, the states list, from index "start",
                // is ordered by descending level and then by state id.

                // Delete duplicates at the start of the list.
                while (start < end && states[start] == states[start + 1])
                {
                    eventConsumed[start + 1] = eventConsumed[start] || eventConsumed[start + 1];
                    start++;
                }

                // Invariant: At this point, the states list, from index "start",
                // is still ordered by descending level and then by state id.

                // For the first state and all the states at the same level,
                // visit the state and replace it in the list with its superstate.
                for (var index = start;
                    index <= end && levels[index] == targetLevel + 1;
                    index++)
                {
                        Visit(states[index]);
                        states[index] = states[index].SuperState;
                        levels[index]--;
                }

                // Invariant: At this point, the states list, from index "start",
                // is ordered by descending level and then by state id.
            }
        }

        void Visit(IState<string, int> state)
        {
            visitList.Add(state.Id);
        }

        [Fact]
        public void Test()
        {
            var states = ids.Select(MakeFakeState).ToArray();
            var dict = states.ToDictionary(s => s.Id);
            AssignSuperStates(dict);

            var currentStates = new[] {"A1A", "A2B1", "B"}.Select(s => dict[s]).ToArray();

            var stateChains = currentStates.Select(GetStateChain);

            TraverseActiveStateTree(stateChains);

            var ord = visitList.Select((s, i) => new { S = s, I = i }).ToDictionary(t => t.S, t => t.I);

            ord["A2B1"].Should().BeLessThan(ord["A2B"]);
            ord["A2B"].Should().BeLessThan(ord["A2"]);
            ord["A2"].Should().BeLessThan(ord["A"]);
            ord["A1A"].Should().BeLessThan(ord["A1"]);
            ord["A1"].Should().BeLessThan(ord["A"]);
        }

        private void TraverseActiveStateTree(IEnumerable<Stack<IState<string, int>>> stateChains, IState<string, int> commonRootState = null)
        {
            var groups = stateChains.GroupBy(sc => sc.FirstOrDefault(), r => { if (r.Any()) r.Pop(); return r; });
            foreach (var g in groups)
            {
                if (g.Key != null)
                {
                    TraverseActiveStateTree(g, g.Key);
                }
                else
                {
                    // Base case
                }
            }

            if (commonRootState != null)
            {
                visitList.Add(commonRootState.Id);
            }
        }

        static Stack<IState<string, int>> GetStateChain(IState<string, int> state)
        {
            var stack = new Stack<IState<string, int>>();
            
            while (state != null)
            {
                stack.Push(state);
                state = state.SuperState;      
            }

            return stack;
        }

        static void AssignSuperStates(Dictionary<string, IState<string, int>> fakeStatesDictionary)
        {
            foreach (var pair in fakeStatesDictionary)
            {
                var name = pair.Key;
                var state = pair.Value;

                if (name.Length == 1)
                {
                    A.CallTo(() => state.SuperState).Returns(null);       
                }
                else
                {
                    var superStateName = name.Substring(0, name.Length - 1);
                    var superState = fakeStatesDictionary[superStateName];

                    A.CallTo(() => state.SuperState).Returns(superState);
                }
            }
        }

        IState<string, int> MakeFakeState(string id)
        {
            var fakeState = A.Fake<IState<string, int>>();
            A.CallTo(() => fakeState.Id).Returns(id);
            A.CallTo(() => fakeState.Level).Returns(id.Length);

            return fakeState;
        }
    }
}
