//-------------------------------------------------------------------------------
// <copyright file="ArgumentActionHolder.cs" company="Appccelerate">
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

using System.Threading;

namespace Appccelerate.StateMachine.Machine.ActionHolders
{
    using System;
    using System.Reflection;

    public class ArgumentDoActionHolder<T> : DoActionHolderBase
    {
        private readonly Action<T, CancellationToken> action;

        public ArgumentDoActionHolder(Action<T, CancellationToken> action)
        {
            this.action = action;
        }

        protected override Action GetActionCall(object argument, CancellationToken cancellation)
        {
            T castArgument = default(T);

            if (argument != Missing.Value && !(argument is T))
            {
                throw new ArgumentException(ActionHoldersExceptionMessages.CannotCastArgumentToActionArgument(argument, this.Describe()));
            }

            if (argument != Missing.Value)
            {
                castArgument = (T) argument;
            }

            return () => this.action(castArgument, cancellation);
        }

        protected override MethodInfo GetMethodInfo()
        {
            return this.action.GetMethodInfo();
        }
    }
}