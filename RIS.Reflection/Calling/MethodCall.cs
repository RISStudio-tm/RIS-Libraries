// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace RIS.Reflection.Calling
{
    public sealed class MethodCall<TInstance>
    {
        public event EventHandler<RInformationEventArgs> Information;
        public event EventHandler<RWarningEventArgs> Warning;
        public event EventHandler<RErrorEventArgs> Error;

        private readonly Type _instanceType;
        private readonly Expression _instance;

        public MethodCall()
        {
            _instanceType = typeof(TInstance);
            _instance = Expression.New(_instanceType);
        }
        public MethodCall(TInstance instance)
        {
            _instanceType = typeof(TInstance);
            _instance = Expression.Constant(instance);
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



        public void CallVoid(string nameMethod)
        {
            try
            {
                if (string.IsNullOrEmpty(nameMethod))
                    throw new ArgumentNullException(nameof(nameMethod));

                var methodInfo = _instanceType.GetMethod(nameMethod,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static,
                    null,
                    Array.Empty<Type>(),
                    null);

                if (methodInfo == null)
                {
                    var exception = new Exception($"Метод с указанным именем[{nameMethod}] и параметрами не найден");
                    Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                    throw exception;
                }

                var methodCall = Expression.Call(_instance, methodInfo);
                var func = Expression.Lambda<Action>(methodCall).Compile();

                func();
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                throw;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                throw;
            }
        }
        public void CallVoid(string nameMethod, string argsMethod)
        {
            CallVoid<string>(nameMethod, argsMethod);
        }
        public void CallVoid(string nameMethod, string[] argsMethod)
        {
            CallVoid<string[]>(nameMethod, argsMethod);
        }
        public void CallVoid<TParam>(string nameMethod, TParam argsMethod)
        {
            try
            {
                var argsType = typeof(TParam);

                if (string.IsNullOrEmpty(nameMethod))
                    throw new ArgumentNullException(nameof(nameMethod));

                var methodInfo = _instanceType.GetMethod(nameMethod,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new Type[] { argsType },
                    null);

                if (methodInfo == null)
                {
                    var exception = new Exception($"Метод с указанным именем[{nameMethod}] и параметрами не найден");
                    Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                    throw exception;
                }

                var param = Expression.Parameter(argsType, "args");
                var methodCall = Expression.Call(_instance, methodInfo, param);
                var func = Expression.Lambda<Action<TParam>>(methodCall, param).Compile();

                func(argsMethod);
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                throw;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                throw;
            }
        }


        public bool Call(string nameMethod)
        {
            return Call<bool>(nameMethod);
        }
        public TResult Call<TResult>(string nameMethod)
        {
            try
            {
                if (string.IsNullOrEmpty(nameMethod))
                    throw new ArgumentNullException(nameof(nameMethod));

                var methodInfo = _instanceType.GetMethod(nameMethod,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static,
                    null,
                    Array.Empty<Type>(),
                    null);

                if (methodInfo == null)
                {
                    var exception = new Exception($"Метод с указанным именем[{nameMethod}] и параметрами не найден");
                    Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                    throw exception;
                }

                var methodCall = Expression.Call(_instance, methodInfo);
                var func = Expression.Lambda<Func<TResult>>(methodCall).Compile();

                return func();
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                throw;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                throw;
            }
        }
        public bool Call(string nameMethod, string argsMethod)
        {
            return Call<string, bool>(nameMethod, argsMethod);
        }
        public bool Call(string nameMethod, string[] argsMethod)
        {
            return Call<string[], bool>(nameMethod, argsMethod);
        }
        public TResult Call<TResult>(string nameMethod, string argsMethod)
        {
            return Call<string, TResult>(nameMethod, argsMethod);
        }
        public TResult Call<TResult>(string nameMethod, string[] argsMethod)
        {
            return Call<string[], TResult>(nameMethod, argsMethod);
        }
        public bool Call<TParam>(string nameMethod, TParam argsMethod)
        {
            return Call<TParam, bool>(nameMethod, argsMethod);
        }
        public TResult Call<TParam, TResult>(string nameMethod, TParam argsMethod)
        {
            try
            {
                var argsType = typeof(TParam);

                if (string.IsNullOrEmpty(nameMethod))
                    throw new ArgumentNullException(nameof(nameMethod));

                var methodInfo = _instanceType.GetMethod(nameMethod,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new Type[] { argsType },
                    null);

                if (methodInfo == null)
                {
                    var exception = new Exception($"Метод с указанным именем[{nameMethod}] и параметрами не найден");
                    Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                    throw exception;
                }

                var param = Expression.Parameter(argsType, "args");
                var methodCall = Expression.Call(_instance, methodInfo, param);
                var func = Expression.Lambda<Func<TParam, TResult>>(methodCall, param).Compile();

                return func(argsMethod);
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                throw;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                throw;
            }
        }



        public void CallStaticVoid(string nameMethod)
        {
            try
            {
                if (string.IsNullOrEmpty(nameMethod))
                    throw new ArgumentNullException(nameof(nameMethod));

                var methodInfo = _instanceType.GetMethod(nameMethod,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static,
                    null,
                    Array.Empty<Type>(),
                    null);

                if (methodInfo == null)
                {
                    var exception = new Exception($"Метод с указанным именем[{nameMethod}] и параметрами не найден");
                    Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                    throw exception;
                }

                var methodCall = Expression.Call(methodInfo);
                var func = Expression.Lambda<Action>(methodCall).Compile();

                func();
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                throw;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                throw;
            }
        }
        public void CallStaticVoid(string nameMethod, string argsMethod)
        {
            CallStaticVoid<string>(nameMethod, argsMethod);
        }
        public void CallStaticVoid(string nameMethod, string[] argsMethod)
        {
            CallStaticVoid<string[]>(nameMethod, argsMethod);
        }
        public void CallStaticVoid<TParam>(string nameMethod, TParam argsMethod)
        {
            try
            {
                var argsType = typeof(TParam);

                if (string.IsNullOrEmpty(nameMethod))
                    throw new ArgumentNullException(nameof(nameMethod));

                var methodInfo = _instanceType.GetMethod(nameMethod,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static,
                    null,
                    new Type[] { argsType },
                    null);

                if (methodInfo == null)
                {
                    var exception = new Exception($"Метод с указанным именем[{nameMethod}] и параметрами не найден");
                    Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                    throw exception;
                }

                var param = Expression.Parameter(argsType, "args");
                var methodCall = Expression.Call(methodInfo, param);
                var func = Expression.Lambda<Action<TParam>>(methodCall, param).Compile();

                func(argsMethod);
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                throw;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                throw;
            }
        }


        public bool CallStatic(string nameMethod)
        {
            return CallStatic<bool>(nameMethod);
        }
        public TResult CallStatic<TResult>(string nameMethod)
        {
            try
            {
                if (string.IsNullOrEmpty(nameMethod))
                    throw new ArgumentNullException(nameof(nameMethod));

                var methodInfo = _instanceType.GetMethod(nameMethod,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static,
                    null,
                    Array.Empty<Type>(),
                    null);

                if (methodInfo == null)
                {
                    var exception = new Exception($"Метод с указанным именем[{nameMethod}] и параметрами не найден");
                    Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                    throw exception;
                }

                var methodCall = Expression.Call(methodInfo);
                var func = Expression.Lambda<Func<TResult>>(methodCall).Compile();

                return func();
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                throw;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                throw;
            }
        }
        public bool CallStatic(string nameMethod, string argsMethod)
        {
            return CallStatic<string, bool>(nameMethod, argsMethod);
        }
        public bool CallStatic(string nameMethod, string[] argsMethod)
        {
            return CallStatic<string[], bool>(nameMethod, argsMethod);
        }
        public TResult CallStatic<TResult>(string nameMethod, string argsMethod)
        {
            return CallStatic<string, TResult>(nameMethod, argsMethod);
        }
        public TResult CallStatic<TResult>(string nameMethod, string[] argsMethod)
        {
            return CallStatic<string[], TResult>(nameMethod, argsMethod);
        }
        public bool CallStatic<TParam>(string nameMethod, TParam argsMethod)
        {
            return CallStatic<TParam, bool>(nameMethod, argsMethod);
        }
        public TResult CallStatic<TParam, TResult>(string nameMethod, TParam argsMethod)
        {
            try
            {
                var argsType = typeof(TParam);

                if (string.IsNullOrEmpty(nameMethod))
                    throw new ArgumentNullException(nameof(nameMethod));

                var methodInfo = _instanceType.GetMethod(nameMethod,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static,
                    null,
                    new Type[] { argsType },
                    null);

                if (methodInfo == null)
                {
                    var exception = new Exception($"Метод с указанным именем[{nameMethod}] и параметрами не найден");
                    Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                    throw exception;
                }

                var param = Expression.Parameter(argsType, "args");
                var methodCall = Expression.Call(methodInfo, param);
                var func = Expression.Lambda<Func<TParam, TResult>>(methodCall, param).Compile();

                return func(argsMethod);
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                throw;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                throw;
            }
        }
    }
}
