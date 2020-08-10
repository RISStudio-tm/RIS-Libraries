using System;
using System.Linq.Expressions;
using System.Reflection;

namespace RIS.Reflection.Call
{
    public static class Calls
    {
        public static event EventHandler<RInformationEventArgs> Information;
		public static event EventHandler<RWarningEventArgs> Warning;
		public static event EventHandler<RErrorEventArgs> Error;

        public static void OnInformation(RInformationEventArgs e)
        {
            OnInformation(null, e);
        }
        public static void OnInformation(object sender, RInformationEventArgs e)
        {
            Information?.Invoke(sender, e);
        }

        public static void OnWarning(RWarningEventArgs e)
        {
            OnWarning(null, e);
        }
        public static void OnWarning(object sender, RWarningEventArgs e)
        {
            Warning?.Invoke(sender, e);
        }

        public static void OnError(RErrorEventArgs e)
        {
            OnError(null, e);
        }
        public static void OnError(object sender, RErrorEventArgs e)
        {
            Error?.Invoke(sender, e);
        }


        public static void CallVoidMethod<TInstance>(string nameMethod)
            where TInstance : class, new()
        {
            try
            {
                var instanceType = typeof(TInstance);

                if (string.IsNullOrEmpty(nameMethod))
                    throw new ArgumentNullException(nameof(nameMethod));

                var methodInfo = instanceType.GetMethod(nameMethod,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static,
                    null,
                    Array.Empty<Type>(),
                    null);

                if (methodInfo == null)
                {
                    var exception = new Exception($"Метод с указанным именем и параметрами не найден");
                    Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                var instance = Expression.New(instanceType);
                var methodCall = Expression.Call(instance, methodInfo);
                var func = Expression.Lambda<Action>(methodCall).Compile();

                func();
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }
        public static void CallVoidMethod<TInstance>(string nameMethod, string argsMethod)
            where TInstance : class, new()
        {
            CallVoidMethod<TInstance, string>(nameMethod, argsMethod);
        }
        public static void CallVoidMethod<TInstance>(string nameMethod, string[] argsMethod)
            where TInstance : class, new()
        {
            CallVoidMethod<TInstance, string[]>(nameMethod, argsMethod);
        }
        public static void CallVoidMethod<TInstance, TParam>(string nameMethod, TParam argsMethod)
            where TInstance : class, new()
        {
            try
            {
                var instanceType = typeof(TInstance);
                var argsType = typeof(TParam);

                if (string.IsNullOrEmpty(nameMethod))
                    throw new ArgumentNullException(nameof(nameMethod));

                var methodInfo = instanceType.GetMethod(nameMethod,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new Type[] { argsType },
                    null);

                if (methodInfo == null)
                {
                    var exception = new Exception($"Метод с указанным именем и параметрами не найден");
                    Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                var param = Expression.Parameter(argsType, "args");
                var instance = Expression.New(instanceType);
                var methodCall = Expression.Call(instance, methodInfo, param);
                var func = Expression.Lambda<Action<TParam>>(methodCall, param).Compile();

                func(argsMethod);
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }

        public static bool CallMethod<TInstance>(string nameMethod)
            where TInstance : class, new()
        {
            return CallMethod<TInstance, bool>(nameMethod);
        }
        public static TResult CallMethod<TInstance, TResult>(string nameMethod)
            where TInstance : class, new()
        {
            try
            {
                var instanceType = typeof(TInstance);

                if (string.IsNullOrEmpty(nameMethod))
                    throw new ArgumentNullException(nameof(nameMethod));

                var methodInfo = instanceType.GetMethod(nameMethod,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static,
                    null,
                    Array.Empty<Type>(),
                    null);

                if (methodInfo == null)
                {
                    var exception = new Exception($"Метод с указанным именем и параметрами не найден");
                    Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                var instance = Expression.New(instanceType);
                var methodCall = Expression.Call(instance, methodInfo);
                var func = Expression.Lambda<Func<TResult>>(methodCall).Compile();

                return func();
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }
        public static bool CallMethod<TInstance>(string nameMethod, string argsMethod)
            where TInstance : class, new()
        {
            return CallMethod<TInstance, string, bool>(nameMethod, argsMethod);
        }
        public static bool CallMethod<TInstance>(string nameMethod, string[] argsMethod)
            where TInstance : class, new()
        {
            return CallMethod<TInstance, string[], bool>(nameMethod, argsMethod);
        }
        public static TResult CallMethod<TInstance, TResult>(string nameMethod, string argsMethod)
            where TInstance : class, new()
        {
            return CallMethod<TInstance, string, TResult>(nameMethod, argsMethod);
        }
        public static TResult CallMethod<TInstance, TResult>(string nameMethod, string[] argsMethod)
            where TInstance : class, new()
        {
            return CallMethod<TInstance, string[], TResult>(nameMethod, argsMethod);
        }
        public static bool CallMethod<TInstance, TParam>(string nameMethod, TParam argsMethod)
            where TInstance : class, new()
        {
            return CallMethod<TInstance, TParam, bool>(nameMethod, argsMethod);
        }
        public static TResult CallMethod<TInstance, TParam, TResult>(string nameMethod, TParam argsMethod)
            where TInstance : class, new()
        {
            try
            {
                var instanceType = typeof(TInstance);
                var argsType = typeof(TParam);

                if (string.IsNullOrEmpty(nameMethod))
                    throw new ArgumentNullException(nameof(nameMethod));

                var methodInfo = instanceType.GetMethod(nameMethod,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new Type[] { argsType },
                    null);

                if (methodInfo == null)
                {
                    var exception = new Exception($"Метод с указанным именем и параметрами не найден");
                    Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                var param = Expression.Parameter(argsType, "args");
                var instance = Expression.New(instanceType);
                var methodCall = Expression.Call(instance, methodInfo, param);
                var func = Expression.Lambda<Func<TParam, TResult>>(methodCall, param).Compile();

                return func(argsMethod);
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }


        public static void CallVoidStaticMethod<TInstance>(string nameMethod)
            where TInstance : class, new()
        {
            try
            {
                var instanceType = typeof(TInstance);

                if (string.IsNullOrEmpty(nameMethod))
                    throw new ArgumentNullException(nameof(nameMethod));

                var methodInfo = instanceType.GetMethod(nameMethod,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static,
                    null,
                    Array.Empty<Type>(),
                    null);

                if (methodInfo == null)
                {
                    var exception = new Exception($"Метод с указанным именем и параметрами не найден");
                    Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                var methodCall = Expression.Call(methodInfo);
                var func = Expression.Lambda<Action>(methodCall).Compile();

                func();
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }
        public static void CallVoidStaticMethod<TInstance>(string nameMethod, string argsMethod)
            where TInstance : class, new()
        {
            CallVoidStaticMethod<TInstance, string>(nameMethod, argsMethod);
        }
        public static void CallVoidStaticMethod<TInstance>(string nameMethod, string[] argsMethod)
            where TInstance : class, new()
        {
            CallVoidStaticMethod<TInstance, string[]>(nameMethod, argsMethod);
        }
        public static void CallVoidStaticMethod<TInstance, TParam>(string nameMethod, TParam argsMethod)
            where TInstance : class, new()
        {
            try
            {
                var instanceType = typeof(TInstance);
                var argsType = typeof(TParam);

                if (string.IsNullOrEmpty(nameMethod))
                    throw new ArgumentNullException(nameof(nameMethod));

                var methodInfo = instanceType.GetMethod(nameMethod,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static,
                    null,
                    new Type[] { argsType },
                    null);

                if (methodInfo == null)
                {
                    var exception = new Exception($"Метод с указанным именем и параметрами не найден");
                    Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                var param = Expression.Parameter(argsType, "args");
                var methodCall = Expression.Call(methodInfo, param);
                var func = Expression.Lambda<Action<TParam>>(methodCall, param).Compile();

                func(argsMethod);
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }

        public static bool CallStaticMethod<TInstance>(string nameMethod)
            where TInstance : class, new()
        {
            return CallStaticMethod<TInstance, bool>(nameMethod);
        }
        public static TResult CallStaticMethod<TInstance, TResult>(string nameMethod)
            where TInstance : class, new()
        {
            try
            {
                var instanceType = typeof(TInstance);

                if (string.IsNullOrEmpty(nameMethod))
                    throw new ArgumentNullException(nameof(nameMethod));

                var methodInfo = instanceType.GetMethod(nameMethod,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static,
                    null,
                    Array.Empty<Type>(),
                    null);

                if (methodInfo == null)
                {
                    var exception = new Exception($"Метод с указанным именем и параметрами не найден");
                    Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                var methodCall = Expression.Call(methodInfo);
                var func = Expression.Lambda<Func<TResult>>(methodCall).Compile();

                return func();
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }
        public static bool CallStaticMethod<TInstance>(string nameMethod, string argsMethod)
            where TInstance : class, new()
        {
            return CallStaticMethod<TInstance, string, bool>(nameMethod, argsMethod);
        }
        public static bool CallStaticMethod<TInstance>(string nameMethod, string[] argsMethod)
            where TInstance : class, new()
        {
            return CallStaticMethod<TInstance, string[], bool>(nameMethod, argsMethod);
        }
        public static TResult CallStaticMethod<TInstance, TResult>(string nameMethod, string argsMethod)
            where TInstance : class, new()
        {
            return CallStaticMethod<TInstance, string, TResult>(nameMethod, argsMethod);
        }
        public static TResult CallStaticMethod<TInstance, TResult>(string nameMethod, string[] argsMethod)
            where TInstance : class, new()
        {
            return CallStaticMethod<TInstance, string[], TResult>(nameMethod, argsMethod);
        }
        public static bool CallStaticMethod<TInstance, TParam>(string nameMethod, TParam argsMethod)
            where TInstance : class, new()
        {
            return CallStaticMethod<TInstance, TParam, bool>(nameMethod, argsMethod);
        }
        public static TResult CallStaticMethod<TInstance, TParam, TResult>(string nameMethod, TParam argsMethod)
            where TInstance : class, new()
        {
            try
            {
                var instanceType = typeof(TInstance);
                var argsType = typeof(TParam);

                if (string.IsNullOrEmpty(nameMethod))
                    throw new ArgumentNullException(nameof(nameMethod));

                var methodInfo = instanceType.GetMethod(nameMethod,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static,
                    null,
                    new Type[] { argsType },
                    null);

                if (methodInfo == null)
                {
                    var exception = new Exception($"Метод с указанным именем и параметрами не найден");
                    Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                var param = Expression.Parameter(argsType, "args");
                var methodCall = Expression.Call(methodInfo, param);
                var func = Expression.Lambda<Func<TParam, TResult>>(methodCall, param).Compile();

                return func(argsMethod);
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }
    }

    public sealed class Calls<TInstance>
        where TInstance : class, new()
    {
        public event EventHandler<RInformationEventArgs> Information;
		public event EventHandler<RWarningEventArgs> Warning;
		public event EventHandler<RErrorEventArgs> Error;

        private readonly Type _instanceType;
        private readonly NewExpression _instance;

        public Calls()
        {
            _instanceType = typeof(TInstance);
            _instance = Expression.New(_instanceType);
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


        public void CallVoidMethod(string nameMethod)
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
                    var exception = new Exception($"Метод с указанным именем и параметрами не найден");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                var methodCall = Expression.Call(_instance, methodInfo);
                var func = Expression.Lambda<Action>(methodCall).Compile();

                func();
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }
        public void CallVoidMethod(string nameMethod, string argsMethod)
        {
            CallVoidMethod<string>(nameMethod, argsMethod);
        }
        public void CallVoidMethod(string nameMethod, string[] argsMethod)
        {
            CallVoidMethod<string[]>(nameMethod, argsMethod);
        }
        public void CallVoidMethod<TParam>(string nameMethod, TParam argsMethod)
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
                    var exception = new Exception($"Метод с указанным именем и параметрами не найден");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                var param = Expression.Parameter(argsType, "args");
                var methodCall = Expression.Call(_instance, methodInfo, param);
                var func = Expression.Lambda<Action<TParam>>(methodCall, param).Compile();

                func(argsMethod);
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }

        public bool CallMethod(string nameMethod)
        {
            return CallMethod<bool>(nameMethod);
        }
        public TResult CallMethod<TResult>(string nameMethod)
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
                    var exception = new Exception($"Метод с указанным именем и параметрами не найден");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                var methodCall = Expression.Call(_instance, methodInfo);
                var func = Expression.Lambda<Func<TResult>>(methodCall).Compile();

                return func();
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }
        public bool CallMethod(string nameMethod, string argsMethod)
        {
            return CallMethod<string, bool>(nameMethod, argsMethod);
        }
        public bool CallMethod(string nameMethod, string[] argsMethod)
        {
            return CallMethod<string[], bool>(nameMethod, argsMethod);
        }
        public TResult CallMethod<TResult>(string nameMethod, string argsMethod)
        {
            return CallMethod<string, TResult>(nameMethod, argsMethod);
        }
        public TResult CallMethod<TResult>(string nameMethod, string[] argsMethod)
        {
            return CallMethod<string[], TResult>(nameMethod, argsMethod);
        }
        public bool CallMethod<TParam>(string nameMethod, TParam argsMethod)
        {
            return CallMethod<TParam, bool>(nameMethod, argsMethod);
        }
        public TResult CallMethod<TParam, TResult>(string nameMethod, TParam argsMethod)
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
                    var exception = new Exception($"Метод с указанным именем и параметрами не найден");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                var param = Expression.Parameter(argsType, "args");
                var methodCall = Expression.Call(_instance, methodInfo, param);
                var func = Expression.Lambda<Func<TParam, TResult>>(methodCall, param).Compile();

                return func(argsMethod);
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }


        public void CallVoidStaticMethod(string nameMethod)
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
                    var exception = new Exception($"Метод с указанным именем и параметрами не найден");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                var methodCall = Expression.Call(methodInfo);
                var func = Expression.Lambda<Action>(methodCall).Compile();

                func();
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }
        public void CallVoidStaticMethod(string nameMethod, string argsMethod)
        {
            CallVoidStaticMethod<string>(nameMethod, argsMethod);
        }
        public void CallVoidStaticMethod(string nameMethod, string[] argsMethod)
        {
            CallVoidStaticMethod<string[]>(nameMethod, argsMethod);
        }
        public void CallVoidStaticMethod<TParam>(string nameMethod, TParam argsMethod)
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
                    var exception = new Exception($"Метод с указанным именем и параметрами не найден");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                var param = Expression.Parameter(argsType, "args");
                var methodCall = Expression.Call(methodInfo, param);
                var func = Expression.Lambda<Action<TParam>>(methodCall, param).Compile();

                func(argsMethod);
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }

        public bool CallStaticMethod(string nameMethod)
        {
            return CallStaticMethod<bool>(nameMethod);
        }
        public TResult CallStaticMethod<TResult>(string nameMethod)
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
                    var exception = new Exception($"Метод с указанным именем и параметрами не найден");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                var methodCall = Expression.Call(methodInfo);
                var func = Expression.Lambda<Func<TResult>>(methodCall).Compile();

                return func();
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }
        public bool CallStaticMethod(string nameMethod, string argsMethod)
        {
            return CallStaticMethod<string, bool>(nameMethod, argsMethod);
        }
        public bool CallStaticMethod(string nameMethod, string[] argsMethod)
        {
            return CallStaticMethod<string[], bool>(nameMethod, argsMethod);
        }
        public TResult CallStaticMethod<TResult>(string nameMethod, string argsMethod)
        {
            return CallStaticMethod<string, TResult>(nameMethod, argsMethod);
        }
        public TResult CallStaticMethod<TResult>(string nameMethod, string[] argsMethod)
        {
            return CallStaticMethod<string[], TResult>(nameMethod, argsMethod);
        }
        public bool CallStaticMethod<TParam>(string nameMethod, TParam argsMethod)
        {
            return CallStaticMethod<TParam, bool>(nameMethod, argsMethod);
        }
        public TResult CallStaticMethod<TParam, TResult>(string nameMethod, TParam argsMethod)
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
                    var exception = new Exception($"Метод с указанным именем и параметрами не найден");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                var param = Expression.Parameter(argsType, "args");
                var methodCall = Expression.Call(methodInfo, param);
                var func = Expression.Lambda<Func<TParam, TResult>>(methodCall, param).Compile();

                return func(argsMethod);
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }
    }
}
