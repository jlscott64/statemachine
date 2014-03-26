using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Appccelerate.StateMachine.Machine.ActionHolders
{
    public abstract class DoActionHolderBase : IDoActionHolder
    {

        public Task Start(object argument, CancellationToken cancellation)
        {
            var actionCall = GetActionCall(argument, cancellation);
            return Task.Factory.StartNew(actionCall, cancellation);
        }

        public string Describe()
        {
            var methodInfo = GetMethodInfo();
            return methodInfo.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any() ? "anonymous" : methodInfo.Name;
        }

        protected abstract MethodInfo GetMethodInfo();

        protected abstract Action GetActionCall(object argument, CancellationToken cancellation);
    }
}