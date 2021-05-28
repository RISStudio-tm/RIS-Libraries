// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;
using RIS.Extensions;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace RIS.Reflection.Conversion
{
    public static class ConversionHelper
    {
        private static readonly ConversionCache ExplicitCastCache;
        private static readonly ConversionCache ImplicitCastCache;

        static ConversionHelper()
        {
            ExplicitCastCache = new ConversionCache();
            ImplicitCastCache = new ConversionCache();
        }



        private static void AttemptExplicitCast<TFrom, TTo>()
        {
            // based on the IL generated from
            // var x = (TTo)(dynamic)default(TFrom);

            var binder = Binder.Convert(
                CSharpBinderFlags.ConvertExplicit,
                typeof(TTo),
                typeof(ConversionHelper));
            var callSite = CallSite<Func<CallSite, TFrom, TTo>>
                .Create(binder);

            callSite.Target(callSite, default(TFrom));
        }

        // ReSharper disable PossibleNullReferenceException
        private static bool CanExplicitCastNonValueType(Type from, Type to)
        {
            if ((to.IsInterface && !from.IsSealed)
                || (from.IsInterface && !to.IsSealed))
            {
                // any non-sealed type can be cast to any interface since the runtime type MIGHT implement
                // that interface. The reverse is also true; we can cast to any non-sealed type from any interface
                // since the runtime type that implements the interface might be a derived type of to.
                return true;
            }

            // arrays are complex because of array covariance 
            // (see http://msmvps.com/blogs/jon_skeet/archive/2013/06/22/array-covariance-not-just-ugly-but-slow-too.aspx).
            // Thus, we have to allow for things like var x = (IEnumerable<string>)new object[0];
            // and var x = (object[])default(IEnumerable<string>);
            var arrayType = from.IsArray && !from.GetElementType().IsValueType
                ? from
                : to.IsArray && !to.GetElementType().IsValueType
                    ? to
                    : null;

            if (arrayType != null)
            {
                var genericInterfaceType = from.IsInterface && from.IsGenericType
                    ? from
                    : to.IsInterface && to.IsGenericType
                        ? to
                        : null;

                if (genericInterfaceType != null)
                {
                    return arrayType.GetInterfaces()
                        .Any(i =>
                            i.IsGenericType
                            && i.GetGenericTypeDefinition() == genericInterfaceType.GetGenericTypeDefinition()
                            && i.GetGenericArguments().Zip(
                                to.GetGenericArguments(),
                                (ia, ta) =>
                                    ta.IsAssignableFrom(ia) || ia.IsAssignableFrom(ta)).All(b => b));
                }
            }

            // look for conversion operators. Even though we already checked for implicit conversions, we have to look
            // for operators of both types because, for example, if a class defines an implicit conversion to int then it can be explicitly
            // cast to uint
            const BindingFlags conversionFlags = BindingFlags.Public
                                                 | BindingFlags.Static
                                                 | BindingFlags.FlattenHierarchy;

            var conversionMethods = from
                .GetMethods(conversionFlags)
                .Concat(to.GetMethods(conversionFlags))
                .Where(m => (m.Name == "op_Explicit" || m.Name == "op_Implicit")
                    && m.Attributes.HasFlag(MethodAttributes.SpecialName)
                    && m.GetParameters().Length == 1
                    && ( // the from argument of the conversion function can be an indirect match to from in
                         // either direction. For example, if we have A : B and Foo defines a conversion from B => Foo,
                         // then C# allows A to be cast to Foo
                         m.GetParameters()[0].ParameterType.IsAssignableFrom(from)
                         || from.IsAssignableFrom(m.GetParameters()[0].ParameterType))
                );

            if (to.IsPrimitive && typeof(IConvertible).IsAssignableFrom(to))
            {
                // as mentioned above, primitive convertible types (i. e. not IntPtr) get special 
                // treatment in the sense that if you can convert from Foo => int, you can convert
                // from Foo => double as well
                return conversionMethods.Any(m => m.ReturnType.IsExplicitlyCastableTo(to));
            }

            return conversionMethods.Any(m => m.ReturnType == to);
        }
        // ReSharper enable PossibleNullReferenceException

        public static bool CanExplicitCast(Type from, Type to)
        {
            // explicit conversion always works if there's implicit conversion
            if (CanImplicitCast(from, to))
                return true;

            var key = new KeyValuePair<Type, Type>(from, to);

            if (ExplicitCastCache.TryGetValue(key, out var cachedValue))
                return cachedValue;

            // for nullable types, we can simply strip off the nullability and evaluate the underyling types
            var underlyingFrom = Nullable.GetUnderlyingType(from);
            var underlyingTo = Nullable.GetUnderlyingType(to);

            if (underlyingFrom != null
                || underlyingTo != null)
            {
                return (underlyingFrom ?? from)
                    .IsExplicitlyCastableTo(underlyingTo ?? to);
            }

            bool result;

            if (from.IsValueType)
            {
                try
                {
                    ReflectionHelper.GetMethod(() => AttemptExplicitCast<object, object>())
                        .GetGenericMethodDefinition()
                        .MakeGenericMethod(from, to)
                        .Invoke(null, Array.Empty<object>());

                    result = true;
                }
                catch (TargetInvocationException ex)
                {
                    result = !(ex.InnerException is RuntimeBinderException);
                }
            }
            else
            {
                // if the from type is null, the dynamic logic above won't be of any help because 
                // either both types are nullable and thus a runtime cast of null => null will 
                // succeed OR we get a runtime failure related to the inability to cast null to 
                // the desired type, which may or may not indicate an actual issue. thus, we do 
                // the work manually
                result = CanExplicitCastNonValueType(from, to);
            }

            ExplicitCastCache.SetValue(key, result);

            return result;
        }



        private static void AttemptImplicitCast<TFrom, TTo>()
        {
            // based on the IL produced by:
            // dynamic list = new List<TTo>();
            // list.Add(Get<TFrom>());

            // We can't use the above code because it will mimic a cast in a generic method
            // which doesn't have the same semantics as a cast in a non-generic method

            var list = new List<TTo>(0);
            var binder = Binder.InvokeMember(
                CSharpBinderFlags.ResultDiscarded,
                "Add",
                null,
                typeof(ConversionHelper),
                new[]
                {
                    CSharpArgumentInfo.Create(
                        CSharpArgumentInfoFlags.None,
                        null),
                    CSharpArgumentInfo.Create(
                        CSharpArgumentInfoFlags.UseCompileTimeType,
                        null),
                }
            );

            var callSite = CallSite<Action<CallSite, object, TFrom>>
                .Create(binder);

            callSite.Target(callSite, list, default(TFrom));
        }

        public static bool CanImplicitCast(Type from, Type to)
        {
            if (to.IsAssignableFrom(from))
                return true;

            var key = new KeyValuePair<Type, Type>(from, to);

            if (ImplicitCastCache.TryGetValue(key, out var cachedValue))
                return cachedValue;

            bool result;

            try
            {
                ReflectionHelper.GetMethod(() => AttemptImplicitCast<object, object>())
                    .GetGenericMethodDefinition()
                    .MakeGenericMethod(from, to)
                    .Invoke(null, Array.Empty<object>());

                result = true;
            }
            catch (TargetInvocationException ex)
            {
                result = !(ex.InnerException is RuntimeBinderException);
            }

            ImplicitCastCache.SetValue(key, result);

            return result;
        }
    }
}
