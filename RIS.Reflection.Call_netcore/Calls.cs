using System;
using System.Linq.Expressions;
using System.Reflection;

namespace RIS.Reflection.Call
{
    public static class Calls
    {
        public static event RMessageHandler ShowMessage;
        public static event RErrorHandler ShowError;

        public static bool CallMethod<TInstance>(string nameMethod) where TInstance : class
        {
            try
            {
                var instanceType = typeof(TInstance);

                if (nameMethod == string.Empty)
                    return false;

                var methodInfo = instanceType.GetMethod(nameMethod,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static,
                    null,
                    new Type[0],
                    null);

                if (methodInfo == null)
                {
                    var exception = new Exception($"Метод с указанным именем и параметрами не найден");
                    Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                var instance = Expression.New(instanceType);
                var methodCall = Expression.Call(instance, methodInfo);
                var func = Expression.Lambda<Func<bool>>(methodCall).Compile();

                return func();
            }
            catch (ArgumentNullException ex)
            {
                Events.DShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                return false;
            }
            catch (Exception ex)
            {
                Events.DShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }
        public static bool CallMethod<TInstance>(string nameMethod, string argsMethod) where TInstance : class
        {
            return CallMethod<TInstance, string>(nameMethod, argsMethod);
        }
        public static bool CallMethod<TInstance, TParam>(string nameMethod, TParam argsMethod) where TInstance : class
        {
            try
            {
                var instanceType = typeof(TInstance);
                var argsType = typeof(TParam);

                if (nameMethod == string.Empty)
                    return false;

                var methodInfo = instanceType.GetMethod(nameMethod,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new Type[] { argsType },
                    null);

                if (methodInfo == null)
                {
                    var exception = new Exception($"Метод с указанным именем и параметрами не найден");
                    Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                var param = Expression.Parameter(argsType, "args");
                var instance = Expression.New(instanceType);
                var methodCall = Expression.Call(instance, methodInfo, param);
                var func = Expression.Lambda<Func<TParam, bool>>(methodCall, param).Compile();

                return func(argsMethod);
            }
            catch (ArgumentNullException ex)
            {
                Events.DShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                return false;
            }
            catch (Exception ex)
            {
                Events.DShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }

        public static bool CallStaticMethod<TInstance>(string nameMethod) where TInstance : class
        {
            try
            {
                var instanceType = typeof(TInstance);

                if (nameMethod == string.Empty)
                    return false;

                var methodInfo = instanceType.GetMethod(nameMethod,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static,
                    null,
                    new Type[0],
                    null);

                if (methodInfo == null)
                {
                    var exception = new Exception($"Метод с указанным именем и параметрами не найден");
                    Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                var methodCall = Expression.Call(methodInfo);
                var func = Expression.Lambda<Func<bool>>(methodCall).Compile();

                return func();
            }
            catch (ArgumentNullException ex)
            {
                Events.DShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                return false;
            }
            catch (Exception ex)
            {
                Events.DShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }
        public static bool CallStaticMethod<TInstance>(string nameMethod, string argsMethod) where TInstance : class
        {
            return CallStaticMethod<TInstance, string>(nameMethod, argsMethod);
        }
        public static bool CallStaticMethod<TInstance, TParam>(string nameMethod, TParam argsMethod) where TInstance : class
        {
            try
            {
                var instanceType = typeof(TInstance);
                var argsType = typeof(TParam);

                if (nameMethod == string.Empty)
                    return false;

                var methodInfo = instanceType.GetMethod(nameMethod,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static,
                    null,
                    new Type[] { argsType },
                    null);

                if (methodInfo == null)
                {
                    var exception = new Exception($"Метод с указанным именем и параметрами не найден");
                    Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                var param = Expression.Parameter(argsType, "args");
                var methodCall = Expression.Call(methodInfo, param);
                var func = Expression.Lambda<Func<TParam, bool>>(methodCall, param).Compile();

                return func(argsMethod);
            }
            catch (ArgumentNullException ex)
            {
                Events.DShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                return false;
            }
            catch (Exception ex)
            {
                Events.DShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }
    }

    public sealed class Calls<TInstance> where TInstance : class
    {
        public event RMessageHandler ShowMessage;
        public event RErrorHandler ShowError;

        public bool CallMethod(string nameMethod)
        {
            try
            {
                var instanceType = typeof(TInstance);

                if (nameMethod == string.Empty)
                    return false;

                var methodInfo = instanceType.GetMethod(nameMethod,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static,
                    null,
                    new Type[0],
                    null);

                if (methodInfo == null)
                {
                    var exception = new Exception($"Метод с указанным именем и параметрами не найден");
                    Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                var instance = Expression.New(instanceType);
                var methodCall = Expression.Call(instance, methodInfo);
                var func = Expression.Lambda<Func<bool>>(methodCall).Compile();

                return func();
            }
            catch (ArgumentNullException ex)
            {
                Events.DShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                return false;
            }
            catch (Exception ex)
            {
                Events.DShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }
        public bool CallMethod(string nameMethod, string argsMethod)
        {
            return CallMethod<string>(nameMethod, argsMethod);
        }
        public bool CallMethod<TParam>(string nameMethod, TParam argsMethod)
        {
            try
            {
                var instanceType = typeof(TInstance);
                var argsType = typeof(TParam);

                if (nameMethod == string.Empty)
                    return false;

                var methodInfo = instanceType.GetMethod(nameMethod,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new Type[] { argsType },
                    null);

                if (methodInfo == null)
                {
                    var exception = new Exception($"Метод с указанным именем и параметрами не найден");
                    Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                var param = Expression.Parameter(argsType, "args");
                var instance = Expression.New(instanceType);
                var methodCall = Expression.Call(instance, methodInfo, param);
                var func = Expression.Lambda<Func<TParam, bool>>(methodCall, param).Compile();

                return func(argsMethod);
            }
            catch (ArgumentNullException ex)
            {
                Events.DShowError?.Invoke(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                return false;
            }
            catch (Exception ex)
            {
                Events.DShowError?.Invoke(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }

        public bool CallStaticMethod(string nameMethod)
        {
            try
            {
                var instanceType = typeof(TInstance);

                if (nameMethod == string.Empty)
                    return false;

                var methodInfo = instanceType.GetMethod(nameMethod,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static,
                    null,
                    new Type[0],
                    null);

                if (methodInfo == null)
                {
                    var exception = new Exception($"Метод с указанным именем и параметрами не найден");
                    Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                var methodCall = Expression.Call(methodInfo);
                var func = Expression.Lambda<Func<bool>>(methodCall).Compile();

                return func();
            }
            catch (ArgumentNullException ex)
            {
                Events.DShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                return false;
            }
            catch (Exception ex)
            {
                Events.DShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }
        public bool CallStaticMethod(string nameMethod, string argsMethod)
        {
            return CallStaticMethod<string>(nameMethod, argsMethod);
        }
        public bool CallStaticMethod<TParam>(string nameMethod, TParam argsMethod)
        {
            try
            {
                var instanceType = typeof(TInstance);
                var argsType = typeof(TParam);

                if (nameMethod == string.Empty)
                    return false;

                var methodInfo = instanceType.GetMethod(nameMethod,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static,
                    null,
                    new Type[] { argsType },
                    null);

                if (methodInfo == null)
                {
                    var exception = new Exception($"Метод с указанным именем и параметрами не найден");
                    Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                var param = Expression.Parameter(argsType, "args");
                var methodCall = Expression.Call(methodInfo, param);
                var func = Expression.Lambda<Func<TParam, bool>>(methodCall, param).Compile();

                return func(argsMethod);
            }
            catch (ArgumentNullException ex)
            {
                Events.DShowError?.Invoke(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                return false;
            }
            catch (Exception ex)
            {
                Events.DShowError?.Invoke(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }
    }
}
