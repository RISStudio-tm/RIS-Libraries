// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace RIS.Reflection.Mapping
{
    public class MethodMap<TInstance>
    {
        public event EventHandler<RInformationEventArgs> Information;
        public event EventHandler<RWarningEventArgs> Warning;
        public event EventHandler<RErrorEventArgs> Error;



        private readonly Type _instanceType;
        private readonly Type[] _targetArgsTypes;
        private readonly Type _targetReturnType;
        private readonly Dictionary<string, MethodInfo> _mappings;

        private TInstance _instance;
        public TInstance Instance
        {
            get
            {
                return _instance;
            }
            set
            {
                if (value == null)
                {
                    var exception = new ArgumentException($"{nameof(value)} cannot be null", nameof(value));
                    Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                    OnError(new RErrorEventArgs(exception, exception.Message));
                    throw exception;
                }

                _instance = value;
            }
        }
        public ReadOnlyDictionary<string, MethodInfo> Mappings { get; }



        public MethodMap(TInstance instance, Delegate target)
        {
            _instanceType = typeof(TInstance);
            _targetArgsTypes = target.Method
                .GetParameters()
                .Select(parameter => parameter.ParameterType)
                .ToArray();
            _targetReturnType = target.Method
                .ReturnType;
            _mappings = new Dictionary<string, MethodInfo>();

            Instance = instance;

            CreateMappings();

            Mappings = new ReadOnlyDictionary<string, MethodInfo>(
                _mappings);
        }
        public MethodMap(TInstance instance, Type[] argsTypes)
            : this(instance, argsTypes, typeof(void))
        {

        }
        public MethodMap(TInstance instance, Type[] argsTypes, Type returnType)
        {
            _instanceType = typeof(TInstance);
            _targetArgsTypes = argsTypes;
            _targetReturnType = returnType;
            _mappings = new Dictionary<string, MethodInfo>();

            Instance = instance;

            CreateMappings();

            Mappings = new ReadOnlyDictionary<string, MethodInfo>(
                _mappings);
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



        // ReSharper disable RedundantJumpStatement
        private void CreateMappings()
        {
            foreach (var method in _instanceType.GetMethods(BindingFlags.NonPublic
                                                            | BindingFlags.Public
                                                            | BindingFlags.Instance
                                                            | BindingFlags.Static))
            {
                var mappedAttributes = method
                    .GetCustomAttributes<MappedMethodAttribute>();

                foreach (var mappedAttribute in mappedAttributes)
                {
                    var name = method.Name;

                    if (!string.IsNullOrEmpty(mappedAttribute.Name))
                        name = mappedAttribute.Name;

                    var parameters = method
                        .GetParameters();

                    if (parameters.Length != _targetArgsTypes.Length)
                        goto NotEqualToTarget;

                    for (int i = 0; i < parameters.Length; ++i)
                    {
                        ref var parameter = ref parameters[i];
                        ref var targetParameterType = ref _targetArgsTypes[i];

                        if (parameter.ParameterType != targetParameterType)
                            goto NotEqualToTarget;
                        if (parameter.IsOut)
                            goto NotEqualToTarget;
                    }

                    if (method.ReturnType != _targetReturnType)
                        goto NotEqualToTarget;

                    _mappings.Add(name, method);

                    continue;

                    // Label
                    NotEqualToTarget:



                    continue;
                }
            }
        }
        // ReSharper restore RedundantJumpStatement



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
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return (T)result;
        }
        public object Invoke(string name, params object[] args)
        {
            if (string.IsNullOrEmpty(name))
            {
                var exception = new ArgumentException($"{nameof(name)} cannot be empty or null", nameof(name));
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            if (!_mappings.ContainsKey(name))
            {
                var exception = new KeyNotFoundException($"Method with mapped name '{name}' not found");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            var method = _mappings[name];

            return method.Invoke(
                !method.IsStatic
                    ? _instance
                    : (object)null,
                args);
        }
    }
}
