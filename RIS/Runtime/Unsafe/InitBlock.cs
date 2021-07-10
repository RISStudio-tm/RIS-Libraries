// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
#if NETFRAMEWORK
using System.Reflection;
using System.Reflection.Emit;
#endif
using System.Runtime.InteropServices;
#if NETFRAMEWORK
using GrEmit;
#endif

namespace RIS.Runtime.Unsafe
{
    public static class InitBlock
    {

#if NETFRAMEWORK

        private delegate void SetMemoryCallback(IntPtr array, byte value, uint count);

        private static readonly SetMemoryCallback SetMemoryHandler;

        static InitBlock()
        {
            SetMemoryHandler = CreateSetMemoryHandler();
        }

        private static SetMemoryCallback CreateSetMemoryHandler()
        {
            var method = new DynamicMethod(
                "call_initblk",
                MethodAttributes.Public | MethodAttributes.Static,
                CallingConventions.Standard,
                typeof(void),
                new[] { typeof(IntPtr), typeof(byte), typeof(uint) },
                typeof(InitBlock),
                false);

            using (var il = new GroboIL(method))
            {
                il.Ldarg(0);   // address
                il.Ldarg(1);   // initialization value
                il.Ldarg(2);   // number of bytes
                il.Initblk();  // block init
                il.Ret();      // return
            }

            return (SetMemoryCallback)method.CreateDelegate(typeof(SetMemoryCallback));
        }

#endif

        public static void Set(byte[] array, int startIndex, uint count, byte value = 0)
        {
            GCHandle handle = default;

            try
            {
                handle = GCHandle.Alloc(array,
                    GCHandleType.Pinned);

                Set(handle.AddrOfPinnedObject(),
                    startIndex, count, value);
            }
            finally
            {
                if (handle.IsAllocated)
                    handle.Free();
            }
        }
        public static unsafe void Set(IntPtr pointer, int startIndex, uint count, byte value = 0)
        {
            IntPtr address = pointer + startIndex;

#if NETCOREAPP

            System.Runtime.CompilerServices.Unsafe.InitBlock(address.ToPointer(), value, count);

#elif NETFRAMEWORK

            SetMemoryHandler(address, value, count);

#endif

        }
    }
}
