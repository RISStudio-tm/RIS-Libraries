// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RIS.Reflection.Mapping
{
    public class MethodMap<TInstance>
    {
        public event EventHandler<RInformationEventArgs> Information;
        public event EventHandler<RWarningEventArgs> Warning;
        public event EventHandler<RErrorEventArgs> Error;

        private readonly Type _instanceType;
        private readonly TInstance _instance;
        private readonly Type _targetType;
        private readonly Dictionary<string, Delegate> _map;

        public MethodMap(TInstance instance, Delegate target)
        {
            _instanceType = typeof(TInstance);
            _instance = instance;
            _targetType = target.GetType();
            _map = new Dictionary<string, Delegate>();

            CreateMapping();
        }
        public MethodMap(TInstance instance, Type[] argsTypes)
            : this(instance, argsTypes, typeof(void))
        {

        }
        public MethodMap(TInstance instance, Type[] argsTypes, Type returnType)
        {
            _instanceType = typeof(TInstance);
            _instance = instance;

            Type[] argsTypesCopy = new Type[argsTypes.Length + 1];

            argsTypes.CopyTo(argsTypesCopy, 0);
            argsTypesCopy[argsTypes.Length] = returnType;

            _targetType = Expression.GetDelegateType(argsTypesCopy);
            _map = new Dictionary<string, Delegate>();

            CreateMapping();
        }

        public void OnInformation(RInformationEventArgs e)
        {
            OnInformation(this, e);
        }
        public void OnInformation(object sender, RInformationEventArgs e)
        {
            Information?.Invoke(sender, e);
        }

        public void OnWarning(RWarningEventArgs e)
        {
            OnWarning(this, e);
        }
        public void OnWarning(object sender, RWarningEventArgs e)
        {
            Warning?.Invoke(sender, e);
        }

        public void OnError(RErrorEventArgs e)
        {
            OnError(this, e);
        }
        public void OnError(object sender, RErrorEventArgs e)
        {
            Error?.Invoke(sender, e);
        }



        private void CreateMapping()
        {
            foreach (MethodInfo method in _instanceType.GetMethods())
            {
                var mappedAttribute = method.GetCustomAttribute<MappedMethodAttribute>();

                if (mappedAttribute == null)
                    continue;

                string name = method.Name;

                if (!string.IsNullOrEmpty(mappedAttribute.Name))
                    name = mappedAttribute.Name;

                Delegate methodDelegate;

                if (method.IsStatic)
                {
                    methodDelegate = Delegate.CreateDelegate(
                        _targetType,
                        null,
                        method,
                        false);
                }
                else
                {
                    methodDelegate = Delegate.CreateDelegate(
                        _targetType,
                        _instance,
                        method,
                        false);
                }

                if (methodDelegate == null)
                    continue;

                _map.Add(name, methodDelegate);
            }
        }

        public string[] GetMappedNames()
        {
            return _map.Keys
                .ToArray();
        }

        public void InvokeVoid(string name, params object[] args)
        {
            Invoke(name, args);
        }

        public T Invoke<T>(string name, params object[] args)
        {
            var result = Invoke(name, args);

            if (!(result is T))
            {
                var exception = new InvalidCastException($"Result of method invocation cannot be cast to type '{typeof(T)}'");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            return (T)result;
        }
        public object Invoke(string name, params object[] args)
        {
            if (string.IsNullOrEmpty(name))
            {
                var exception = new ArgumentException($"{nameof(name)} cannot be empty or null", nameof(name));
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            if (!_map.ContainsKey(name))
            {
                var exception = new KeyNotFoundException($"Method with name '{nameof(name)}' not found");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            return _map[name].DynamicInvoke(args);
        }
    }
}
