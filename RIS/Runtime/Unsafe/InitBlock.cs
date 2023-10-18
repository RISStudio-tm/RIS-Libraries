// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Runtime.InteropServices;

namespace RIS.Runtime.Unsafe
{
    public static class InitBlock
    {

        public static void Set(byte[] array,
            int startIndex, uint count, byte value = 0)
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
        public static unsafe void Set(nint pointer,
            int startIndex, uint count, byte value = 0)
        {
            var address = pointer + startIndex;

            System.Runtime.CompilerServices.Unsafe.InitBlock(
                address.ToPointer(), value, count);

        }
    }
}
