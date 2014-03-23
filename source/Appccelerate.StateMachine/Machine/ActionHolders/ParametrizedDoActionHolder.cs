// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParametrizedActionHolder{T}.cs" company="Appccelerate">
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
// --------------------------------------------------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace Appccelerate.StateMachine.Machine.ActionHolders
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public class ParametrizedDoActionHolder<T> : DoActionHolderBase
    {
        private readonly Action<T, CancellationToken> action;

        private readonly T parameter;

        public ParametrizedDoActionHolder(Action<T, CancellationToken> action, T parameter)
        {
            this.action = action;
            this.parameter = parameter;
        }

        protected override MethodInfo GetMethodInfo()
        {
            return this.action.GetMethodInfo();
        }

        protected override Action GetActionCall(object argument, CancellationToken cancellation)
        {
            return () => this.action(this.parameter, cancellation);
        }
    }
}