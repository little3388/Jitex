﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Jitex.Exceptions;
using Jitex.JIT.Context;
using Jitex.Runtime;
using Jitex.Utils;
using Jitex.Utils.Comparer;

namespace Jitex.Intercept
{
    public class InterceptHandler
    {
        public delegate void InterceptorHandler(CallContext context);
    }

    public class InterceptManager
    {
        private static InterceptManager? _instance;

        private readonly ConcurrentBag<InterceptContext> _interceptedMethods = new ConcurrentBag<InterceptContext>();

        private InterceptHandler.InterceptorHandler? _interceptors;

        private InterceptManager()
        {
        }

        internal void AddIntercept(InterceptContext detourContext)
        {
            _interceptedMethods.Add(detourContext);
            EnableIntercept(detourContext.Method);
        }

        internal void EnableIntercept(MethodBase method)
        {
            InterceptContext? interceptContext = GetInterceptContext(method);

            if (interceptContext == null) throw new InterceptNotFound(method);

            interceptContext.WriteDetour();
        }

        internal void RemoveIntercept(MethodBase method)
        {
            InterceptContext? interceptContext = GetInterceptContext(method);

            if (interceptContext == null) throw new InterceptNotFound(method);

            interceptContext.RemoveDetour();
        }

        private InterceptContext? GetInterceptContext(MethodBase method)
        {
            return _interceptedMethods.FirstOrDefault(w => MethodEqualityComparer.Instance.Equals(w.Method, method));
        }

        internal void AddCallInterceptor(InterceptHandler.InterceptorHandler inteceptor) => _interceptors += inteceptor;

        internal void RemoveCallInterceptor(InterceptHandler.InterceptorHandler inteceptor) => _interceptors -= inteceptor;

        internal bool HasCallInteceptor(InterceptHandler.InterceptorHandler inteceptor) => _interceptors != null && _interceptors.GetInvocationList().Any(del => del.Method == inteceptor.Method);

        public object? InterceptCall(long handle, object[] parameters, Type[]? genericTypeArguments, Type[]? genericMethodArguments)
        {
            MethodBase? method = RuntimeMethodCache.GetMethodFromHandle(new IntPtr(handle));

            if (method == null) throw new MethodNotFound(handle);

            InterceptContext? interceptContext = GetInterceptContext(method);

            if (interceptContext == null) throw new InterceptNotFound(method);

            Delegate del = DelegateHelper.CreateDelegate(interceptContext.SecondaryNativeAddress, method);

            if (genericTypeArguments != null)
            {
                Type type = method.DeclaringType.GetGenericTypeDefinition().MakeGenericType(genericTypeArguments);
                method = type.GetMethods((BindingFlags)(-1)).First(w => w.MetadataToken == method.MetadataToken);
            }

            if (genericMethodArguments != null)
                method = ((MethodInfo)method).GetGenericMethodDefinition().MakeGenericMethod(genericMethodArguments);

            CallContext context = new CallContext(method, del, parameters);

            if (_interceptors != null)
            {
                Delegate[] interceptors = _interceptors.GetInvocationList();

                foreach (InterceptHandler.InterceptorHandler interceptor in interceptors)
                {
                    interceptor(context);
                }
            }

            if (!context.IsReturnSetted)
                context.ContinueFlow();

            return context.ReturnValue;
        }

        public static InterceptManager GetInstance()
        {
            return _instance ??= new InterceptManager();
        }
    }
}