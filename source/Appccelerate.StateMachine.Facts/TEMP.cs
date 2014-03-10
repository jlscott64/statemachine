using System;
using System.Collections.Generic;
using System.Linq;
using Appccelerate.StateMachine.Machine;
using FakeItEasy;
using Xunit;

namespace Appccelerate.StateMachine
{
    public class TEMP
    {
        public enum S
        {
            A,
                A1,
            B,
                B1
        }

        [Fact]
        public void Test()
        {
            var fakeStates = Enum.GetValues(typeof (S)).Cast<S>().Select(MakeFakeState).ToArray();
            var fakeStatesDictionary = fakeStates.ToDictionary(s => Enum.GetName(typeof(S), s.Id));
            AssignSuperStates(fakeStatesDictionary);


        }

        static void AssignSuperStates(Dictionary<string, IState<S, int>> fakeStatesDictionary)
        {
            foreach (var pair in fakeStatesDictionary)
            {
                KeyValuePair<string, IState<S, int>> pair1 = pair;
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

        IState<S, int> MakeFakeState(S s)
        {
            var fakeState = A.Fake<IState<S, int>>();
            A.CallTo(() => fakeState.Id).Returns(s);

            return fakeState;
        }

        private void TraverseActiveStateTree(IEnumerable<IState<S, int>> currentStates)
        {
            var stateChains = currentStates.Select(GetStateChain);

        }

        private void TraverseActiveStateTree(IEnumerable<IEnumerable<IState<S, int>>> stateChains)
        {
            IEnumerable<IEnumerable<IState<S, int>>> a = stateChains;

        }

        static IEnumerable<IState<S, int>> GetStateChain(IState<S, int> state)
        {
            yield return state;
            while (state.SuperState != null)
            {
                state = state.SuperState;
                yield return state;
            }
        }
    }
}
