using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Appccelerate.StateMachine.Machine;
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
                    "A2B",
                        "A2B1",
            "B",
                "B1",
        };

        IList<string> visitList = new List<string>();

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
