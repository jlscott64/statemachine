using System;
using System.Threading;
using FakeItEasy;
using FluentAssertions;
using Machine.Specifications;

namespace Appccelerate.StateMachine
{
    [Subject(Concern.DoActions)]
    public class When_starting_in_a_state_with_a_do_action
    {
        public enum S { A }
        public enum E { }

        static PassiveStateMachine<S, E> machine;
        static ManualResetEvent doActionStarted;

        Establish context = () =>
        {
            Action<CancellationToken> doAction = cancellationToken => doActionStarted.Set();

            machine = new PassiveStateMachine<S, E>();

            machine
                .In(S.A)
                .ExecuteWhileActive(doAction);

            machine.Initialize(S.A);

            doActionStarted = new ManualResetEvent(false);
        };

        Because of = () => machine.Start();

        It should_start_the_do_action = () => doActionStarted.WaitOne(TimeSpan.FromMilliseconds(20)).Should().BeTrue();
    }

    [Subject(Concern.DoActions)]
    public class When_leaving_a_state_with_a_do_action
    {
        public enum S { A, B }
        public enum E { A_to_B }

        static PassiveStateMachine<S, E> machine;
        static ManualResetEvent doActionCancelled;

        Establish context = () =>
        {
            Action<CancellationToken> doAction = cancellationToken => cancellationToken.Register(() => doActionCancelled.Set());

            machine = new PassiveStateMachine<S, E>();

            machine
                .In(S.A)
                .ExecuteWhileActive(doAction)
                .On(E.A_to_B)
                .Goto(S.B);

            machine.Initialize(S.A);

            doActionCancelled = new ManualResetEvent(false);
            machine.Start();
        };

        Because of = () => machine.Fire(E.A_to_B);

        It should_cancel_the_do_action = () => doActionCancelled.WaitOne(TimeSpan.FromMilliseconds(100)).Should().BeTrue();
    }


    [Subject(Concern.DoActions)]
    public class When_completing_a_states_do_actions
    {
        public enum S { A, B }
        public enum E { A_to_B }

        static ActiveStateMachine<S, E> machine;
        static ManualResetEvent completed;
        static ManualResetEvent stateBEntered;

        static void ExecuteUntil(WaitHandle completed)
        {
            completed.WaitOne();
        }

        Establish context = () =>
        {
            completed = new ManualResetEvent(false);
            stateBEntered = new ManualResetEvent(false);
            
            machine = new ActiveStateMachine<S, E>();

            machine
                .In(S.A)
                .ExecuteWhileActive(_ => ExecuteUntil(completed))
                .OnCompletion.Goto(S.B);

            machine
                .In(S.B)
                .ExecuteOnEntry(() => stateBEntered.Set());

            machine.Initialize(S.A);
            machine.Start();
        };

        Because of = () => completed.Set();

        It should_fire_the_completion_transition = () => stateBEntered.WaitOne(TimeSpan.FromMilliseconds(20)).Should().BeTrue();

        Cleanup after = () => machine.Stop();
    }
}