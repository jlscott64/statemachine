using System;
using System.Threading;

namespace Appccelerate.StateMachine.Syntax
{
    /// <summary>
    /// Defines the do action syntax.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public interface IDoActionSyntax<TState, TEvent> : IExitActionSyntax<TState, TEvent>
    {
        /// <summary>
        /// Defines a do action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>Do action syntax.</returns>
        IDoActionSyntax<TState, TEvent> ExecuteWhileActive(Action<CancellationToken> action);
    }
}