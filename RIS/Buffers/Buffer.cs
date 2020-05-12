using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RIS.Buffers
{
    public static class Buffer
    {
        private const int BufferBlockCopyThreshold = 1024;
        private const int UnmanagedThreshold = 128;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] CopyBytes(this byte[] data)
        {
            return data.DeepCopy();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyBytes(this byte[] src, byte[] dst)
        {
            src.DeepCopy(dst);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyBytes(this byte[] src, byte[] dst, int length)
        {
            src.DeepCopy(0, dst, 0, length);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyBytes(this byte[] src, int srcOffset, byte[] dst, int dstOffset, int length)
        {
            src.DeepCopy(srcOffset, dst, dstOffset, length);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyBytesNoChecks(this byte[] src, int srcOffset, byte[] dst, int dstOffset, int length)
        {
            src.DeepCopyNoChecks(srcOffset, dst, dstOffset, length);
        }


        public static T[] DeepCopy<T>(this T[] data)
            where T : struct
        {
            if (data == null)
                return null;

            var dst = new T[data.Length];

            data.DeepCopyNoChecks(0, dst, 0, data.Length);

            return dst;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeepCopy<T>(this T[] src, T[] dst)
            where T : struct
        {
            ThrowOnInvalidArgument(src, dst, src.Length);
            DeepCopyNoChecks(src, 0, dst, 0, src.Length);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeepCopy<T>(this T[] src, int srcOffset, T[] dst, int dstOffset, int length)
            where T : struct
        {
            ThrowOnInvalidArgument(src, dst, length, srcOffset, dstOffset);
            DeepCopyNoChecks(src, srcOffset, dst, dstOffset, length);
        }
        public static void DeepCopyNoChecks<T>(this T[] src, int srcOffset, T[] dst, int dstOffset, int length)
            where T : struct
        {
            int sizeT = (int) Environment.GetSize<T>();

            int unmanagedLimit = UnmanagedThreshold / sizeT;

            if (length >= unmanagedLimit)
            {
                unsafe
                {
                    GCHandle srcPtr = GCHandle.Alloc(src[srcOffset], GCHandleType.Pinned);
                    GCHandle dstPtr = GCHandle.Alloc(dst[dstOffset], GCHandleType.Pinned);

                    try
                    {
                        CopyMemory((byte*) srcPtr.AddrOfPinnedObject(),
                            (byte*) dstPtr.AddrOfPinnedObject(),
                            length * sizeT);
                    }
                    finally
                    {
                        srcPtr.Free();
                        dstPtr.Free();
                    }
                }
            }
            else
            {
                int bufferBlockCopyLimit = BufferBlockCopyThreshold / sizeT;

                if (length >= bufferBlockCopyLimit)
                {
                    System.Buffer.BlockCopy(src, srcOffset, dst, dstOffset, length * sizeT);
                }
                else
                {
                    Array.Copy(src, srcOffset, dst, dstOffset, length);
                }
            }
        }


        public static sbyte[] DeepCopy(this sbyte[] data)
        {
            if (data == null)
                return null;

            var dst = new sbyte[data.Length];

            data.DeepCopyNoChecks(0, dst, 0, data.Length);

            return dst;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeepCopy(this sbyte[] src, sbyte[] dst)
        {
            ThrowOnInvalidArgument(src, dst, src.Length);
            DeepCopyNoChecks(src, 0, dst, 0, src.Length);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeepCopy(this sbyte[] src, int srcOffset, sbyte[] dst, int dstOffset, int length)
        {
            ThrowOnInvalidArgument(src, dst, length, srcOffset, dstOffset);
            DeepCopyNoChecks(src, srcOffset, dst, dstOffset, length);
        }
        public static void DeepCopyNoChecks(this sbyte[] src, int srcOffset, sbyte[] dst, int dstOffset, int length)
        {
            if (length >= UnmanagedThreshold)
            {
                unsafe {
                    fixed (sbyte* srcPtr = &src[srcOffset]) {
                        fixed (sbyte* dstPtr = &dst[dstOffset]) {
                            CopyMemory((byte*) srcPtr, (byte*) dstPtr, length);
                        }
                    }
                }
            }
            else
            {
                if (length >= BufferBlockCopyThreshold)
                {
                    System.Buffer.BlockCopy(src, srcOffset, dst, dstOffset, length);
                }
                else
                {
                    Array.Copy(src, srcOffset, dst, dstOffset, length);
                }
            }
        }


        public static byte[] DeepCopy(this byte[] data)
        {
            if (data == null)
                return null;

            var dst = new byte[data.Length];

            data.DeepCopyNoChecks(0, dst, 0, data.Length);

            return dst;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeepCopy(this byte[] src, byte[] dst)
        {
            ThrowOnInvalidArgument(src, dst, src.Length);
            DeepCopyNoChecks(src, 0, dst, 0, src.Length);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeepCopy(this byte[] src, int srcOffset, byte[] dst, int dstOffset, int length)
        {
            ThrowOnInvalidArgument(src, dst, length, srcOffset, dstOffset);
            DeepCopyNoChecks(src, srcOffset, dst, dstOffset, length);
        }
        public static void DeepCopyNoChecks(this byte[] src, int srcOffset, byte[] dst, int dstOffset, int length)
        {
            if (length >= UnmanagedThreshold) {
                unsafe {
                    fixed (byte* srcPtr = &src[srcOffset]) {
                        fixed (byte* dstPtr = &dst[dstOffset]) {
                            CopyMemory(srcPtr, dstPtr, length);
                        }
                    }
                }
            }
            else
            {
                if (length >= BufferBlockCopyThreshold)
                {
                    System.Buffer.BlockCopy(src, srcOffset, dst, dstOffset, length);
                }
                else
                {
                    Array.Copy(src, srcOffset, dst, dstOffset, length);
                }
            }
        }


        public static char[] DeepCopy(this char[] data)
        {
            if (data == null)
                return null;

            var dst = new char[data.Length];

            data.DeepCopyNoChecks(0, dst, 0, data.Length);

            return dst;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeepCopy(this char[] src, char[] dst)
        {
            ThrowOnInvalidArgument(src, dst, src.Length);
            DeepCopyNoChecks(src, 0, dst, 0, src.Length);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeepCopy(this char[] src, int srcOffset, char[] dst, int dstOffset, int length)
        {
            ThrowOnInvalidArgument(src, dst, length, srcOffset, dstOffset);
            DeepCopyNoChecks(src, srcOffset, dst, dstOffset, length);
        }
        public static void DeepCopyNoChecks(this char[] src, int srcOffset, char[] dst, int dstOffset, int length)
        {
            const int unmanagedLimit = UnmanagedThreshold / sizeof(char);

            if (length >= unmanagedLimit) {
                unsafe {
                    fixed (char* srcPtr = &src[srcOffset]) {
                        fixed (char* dstPtr = &dst[dstOffset]) {
                            CopyMemory((byte*) srcPtr, (byte*) dstPtr, length * sizeof(char));
                        }
                    }
                }
            }
            else
            {
                const int bufferBlockCopyLimit = BufferBlockCopyThreshold / sizeof(char);

                if (length >= bufferBlockCopyLimit)
                {
                    System.Buffer.BlockCopy(src, srcOffset, dst, dstOffset, length * sizeof(char));
                }
                else
                {
                    Array.Copy(src, srcOffset, dst, dstOffset, length);
                }
            }
        }


        public static short[] DeepCopy(this short[] data)
        {
            if (data == null)
                return null;

            var dst = new short[data.Length];

            data.DeepCopyNoChecks(0, dst, 0, data.Length);

            return dst;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeepCopy(this short[] src, short[] dst)
        {
            ThrowOnInvalidArgument(src, dst, src.Length);
            DeepCopyNoChecks(src, 0, dst, 0, src.Length);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeepCopy(this short[] src, int srcOffset, short[] dst, int dstOffset, int length)
        {
            ThrowOnInvalidArgument(src, dst, length, srcOffset, dstOffset);
            DeepCopyNoChecks(src, srcOffset, dst, dstOffset, length);
        }
        public static void DeepCopyNoChecks(this short[] src, int srcOffset, short[] dst, int dstOffset, int length)
        {
            const int unmanagedLimit = UnmanagedThreshold / sizeof(short);

            if (length >= unmanagedLimit) {
                unsafe {
                    fixed (short* srcPtr = &src[srcOffset]) {
                        fixed (short* dstPtr = &dst[dstOffset]) {
                            CopyMemory((byte*) srcPtr, (byte*) dstPtr, length * sizeof(short));
                        }
                    }
                }
            }
            else
            {
                const int bufferBlockCopyLimit = BufferBlockCopyThreshold / sizeof(short);

                if (length >= bufferBlockCopyLimit)
                {
                    System.Buffer.BlockCopy(src, srcOffset, dst, dstOffset, length * sizeof(short));
                }
                else
                {
                    Array.Copy(src, srcOffset, dst, dstOffset, length);
                }
            }
        }


        public static ushort[] DeepCopy(this ushort[] data)
        {
            if (data == null)
                return null;

            var dst = new ushort[data.Length];

            data.DeepCopyNoChecks(0, dst, 0, data.Length);

            return dst;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeepCopy(this ushort[] src, ushort[] dst)
        {
            ThrowOnInvalidArgument(src, dst, src.Length);
            DeepCopyNoChecks(src, 0, dst, 0, src.Length);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeepCopy(this ushort[] src, int srcOffset, ushort[] dst, int dstOffset, int length)
        {
            ThrowOnInvalidArgument(src, dst, length, srcOffset, dstOffset);
            DeepCopyNoChecks(src, srcOffset, dst, dstOffset, length);
        }
        public static void DeepCopyNoChecks(this ushort[] src, int srcOffset, ushort[] dst, int dstOffset, int length)
        {
            const int unmanagedLimit = UnmanagedThreshold / sizeof(uint);

            if (length >= unmanagedLimit) {
                unsafe {
                    fixed (ushort* srcPtr = &src[srcOffset]) {
                        fixed (ushort* dstPtr = &dst[dstOffset]) {
                            CopyMemory((byte*) srcPtr, (byte*) dstPtr, length * sizeof(ushort));
                        }
                    }
                }
            }
            else
            {
                const int bufferBlockCopyLimit = BufferBlockCopyThreshold / sizeof(ushort);

                if (length >= bufferBlockCopyLimit)
                {
                    System.Buffer.BlockCopy(src, srcOffset, dst, dstOffset, length * sizeof(ushort));
                }
                else
                {
                    Array.Copy(src, srcOffset, dst, dstOffset, length);
                }
            }
        }


        public static int[] DeepCopy(this int[] data)
        {
            if (data == null)
                return null;

            var dst = new int[data.Length];

            data.DeepCopyNoChecks(0, dst, 0, data.Length);

            return dst;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeepCopy(this int[] src, int[] dst)
        {
            ThrowOnInvalidArgument(src, dst, src.Length);
            DeepCopyNoChecks(src, 0, dst, 0, src.Length);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeepCopy(this int[] src, int srcOffset, int[] dst, int dstOffset, int length)
        {
            ThrowOnInvalidArgument(src, dst, length, srcOffset, dstOffset);
            DeepCopyNoChecks(src, srcOffset, dst, dstOffset, length);
        }
        public static void DeepCopyNoChecks(this int[] src, int srcOffset, int[] dst, int dstOffset, int length)
        {
            const int unmanagedLimit = UnmanagedThreshold / sizeof(int);

            if (length >= unmanagedLimit) {
                unsafe {
                    fixed (int* srcPtr = &src[srcOffset]) {
                        fixed (int* dstPtr = &dst[dstOffset]) {
                            CopyMemory((byte*) srcPtr, (byte*) dstPtr, length * sizeof(int));
                        }
                    }
                }
            }
            else
            {
                const int bufferBlockCopyLimit = BufferBlockCopyThreshold / sizeof(int);

                if (length >= bufferBlockCopyLimit)
                {
                    System.Buffer.BlockCopy(src, srcOffset, dst, dstOffset, length * sizeof(int));
                }
                else
                {
                    Array.Copy(src, srcOffset, dst, dstOffset, length);
                }
            }
        }


        public static uint[] DeepCopy(this uint[] data)
        {
            if (data == null)
                return null;

            var dst = new uint[data.Length];

            data.DeepCopyNoChecks(0, dst, 0, data.Length);

            return dst;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeepCopy(this uint[] src, uint[] dst)
        {
            ThrowOnInvalidArgument(src, dst, src.Length);
            DeepCopyNoChecks(src, 0, dst, 0, src.Length);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeepCopy(this uint[] src, int srcOffset, uint[] dst, int dstOffset, int length)
        {
            ThrowOnInvalidArgument(src, dst, length, srcOffset, dstOffset);
            DeepCopyNoChecks(src, srcOffset, dst, dstOffset, length);
        }
        public static void DeepCopyNoChecks(this uint[] src, int srcOffset, uint[] dst, int dstOffset, int length)
        {
            const int unmanagedLimit = UnmanagedThreshold / sizeof(uint);

            if (length >= unmanagedLimit) {
                unsafe {
                    fixed (uint* srcPtr = &src[srcOffset]) {
                        fixed (uint* dstPtr = &dst[dstOffset]) {
                            CopyMemory((byte*) srcPtr, (byte*) dstPtr, length * sizeof(uint));
                        }
                    }
                }
            }
            else
            {
                const int bufferBlockCopyLimit = BufferBlockCopyThreshold / sizeof(uint);

                if (length >= bufferBlockCopyLimit)
                {
                    System.Buffer.BlockCopy(src, srcOffset, dst, dstOffset, length * sizeof(uint));
                }
                else
                {
                    Array.Copy(src, srcOffset, dst, dstOffset, length);
                }
            }
        }


        public static long[] DeepCopy(this long[] data)
        {
            if (data == null)
                return null;

            var dst = new long[data.Length];

            data.DeepCopyNoChecks(0, dst, 0, data.Length);

            return dst;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeepCopy(this long[] src, long[] dst)
        {
            ThrowOnInvalidArgument(src, dst, src.Length);
            DeepCopyNoChecks(src, 0, dst, 0, src.Length);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeepCopy(this long[] src, int srcOffset, long[] dst, int dstOffset, int length)
        {
            ThrowOnInvalidArgument(src, dst, length, srcOffset, dstOffset);
            DeepCopyNoChecks(src, srcOffset, dst, dstOffset, length);
        }
        public static void DeepCopyNoChecks(this long[] src, int srcOffset, long[] dst, int dstOffset, int length)
        {
            const int unmanagedLimit = UnmanagedThreshold / sizeof(long);

            if (length >= unmanagedLimit) {
                unsafe {
                    fixed (long* srcPtr = &src[srcOffset]) {
                        fixed (long* dstPtr = &dst[dstOffset]) {
                            CopyMemory((byte*) srcPtr, (byte*) dstPtr, length * sizeof(long));
                        }
                    }
                }
            }
            else
            {
                const int bufferBlockCopyLimit = BufferBlockCopyThreshold / sizeof(long);

                if (length >= bufferBlockCopyLimit)
                {
                    System.Buffer.BlockCopy(src, srcOffset, dst, dstOffset, length * sizeof(long));
                }
                else
                {
                    Array.Copy(src, srcOffset, dst, dstOffset, length);
                }
            }
        }


        public static ulong[] DeepCopy(this ulong[] data)
        {
            if (data == null)
                return null;

            var dst = new ulong[data.Length];

            data.DeepCopyNoChecks(0, dst, 0, data.Length);

            return dst;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeepCopy(this ulong[] src, ulong[] dst)
        {
            ThrowOnInvalidArgument(src, dst, src.Length);
            DeepCopyNoChecks(src, 0, dst, 0, src.Length);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeepCopy(this ulong[] src, int srcOffset, ulong[] dst, int dstOffset, int length)
        {
            ThrowOnInvalidArgument(src, dst, length, srcOffset, dstOffset);
            DeepCopyNoChecks(src, srcOffset, dst, dstOffset, length);
        }
        public static void DeepCopyNoChecks(this ulong[] src, int srcOffset, ulong[] dst, int dstOffset, int length)
        {
            const int unmanagedLimit = UnmanagedThreshold / sizeof(ulong);

            if (length >= unmanagedLimit) {
                unsafe {
                    fixed (ulong* srcPtr = &src[srcOffset]) {
                        fixed (ulong* dstPtr = &dst[dstOffset]) {
                            CopyMemory((byte*) srcPtr, (byte*) dstPtr, length * sizeof(ulong));
                        }
                    }
                }
            }
            else
            {
                const int bufferBlockCopyLimit = BufferBlockCopyThreshold / sizeof(ulong);

                if (length >= bufferBlockCopyLimit)
                {
                    System.Buffer.BlockCopy(src, srcOffset, dst, dstOffset, length * sizeof(ulong));
                }
                else
                {
                    Array.Copy(src, srcOffset, dst, dstOffset, length);
                }
            }
        }


        public static unsafe void CopyMemory(byte* srcPtr, byte* dstPtr, int length)
        {
            const int u32Size = sizeof(uint);
            const int u64Size = sizeof(ulong);

            byte* srcEndPtr = srcPtr + length;

            if (Environment.PlatformWordSize == u32Size) {
                // 32-bit
                while (srcPtr + u64Size <= srcEndPtr) {
                    *(uint*) dstPtr = *(uint*) srcPtr;
                    dstPtr += u32Size;
                    srcPtr += u32Size;
                    *(uint*) dstPtr = *(uint*) srcPtr;
                    dstPtr += u32Size;
                    srcPtr += u32Size;
                }
            } else if (Environment.PlatformWordSize == u64Size) {
                // 64-bit            
                const int u128Size = sizeof(ulong) * 2;
                while (srcPtr + u128Size <= srcEndPtr) {
                    *(ulong*) dstPtr = *(ulong*) srcPtr;
                    dstPtr += u64Size;
                    srcPtr += u64Size;
                    *(ulong*) dstPtr = *(ulong*) srcPtr;
                    dstPtr += u64Size;
                    srcPtr += u64Size;
                }
                if (srcPtr + u64Size <= srcEndPtr) {
                    *(ulong*) dstPtr ^= *(ulong*) srcPtr;
                    dstPtr += u64Size;
                    srcPtr += u64Size;
                }
            }

            if (srcPtr + u32Size <= srcEndPtr) {
                *(uint*) dstPtr = *(uint*) srcPtr;
                dstPtr += u32Size;
                srcPtr += u32Size;
            }

            if (srcPtr + sizeof(ushort) <= srcEndPtr) {
                *(ushort*) dstPtr = *(ushort*) srcPtr;
                dstPtr += sizeof(ushort);
                srcPtr += sizeof(ushort);
            }

            if (srcPtr + 1 <= srcEndPtr) {
                *dstPtr = *srcPtr;
            }
        }

        internal static void ThrowOnInvalidArgument<T>(
            T[] src, T[] dst, int length, int srcOffset = 0, int dstOffset = 0,
            string srcName = null, string dstName = null, string lengthName = null, string srcOffsetName = null, string dstOffsetName = null)
            where T : struct
        {
            if (src == null)
            {
                throw new ArgumentNullException(srcName ?? "src");
            }

            int srcLength = src.Length;

            if (src.Length < 0)
            {
                throw new ArgumentException($"{srcName ?? "src"}.Length < 0 : {srcLength} < 0", srcName ?? "src");
            }

            if (dst == null)
            {
                throw new ArgumentNullException(dstName ?? "dst");
            }

            int dstLength = dst.Length;

            if (dst.Length < 0)
            {
                throw new ArgumentException($"{dstName ?? "dst"}.Length < 0 : {dstLength} < 0", dstName ?? "dst");
            }

            if (srcOffset != 0 || dstOffset != 0 || length != srcLength)
            {
                if (length < 0)
                {
                    throw new ArgumentOutOfRangeException(lengthName ?? "length",
                        $"{lengthName ?? "length"} < 0 : {length} < 0");
                }

                if (srcOffset + length > srcLength)
                {
                    if (srcOffset >= srcLength)
                    {
                        throw new ArgumentException(
                            $"{srcOffsetName ?? "srcOffset"} >= {srcName ?? "src"}.Length : {srcOffset} >= {srcLength}");
                    }
                    else if (length > srcLength)
                    {
                        throw new ArgumentOutOfRangeException(lengthName ?? "length",
                            $"{lengthName ?? "length"} > {srcName ?? "src"}.Length : {length} > {srcLength}");
                    }
                    else
                    {
                        throw new ArgumentException(
                            $"{srcOffsetName ?? "srcOffset"} + {lengthName ?? "length"} > {srcName ?? "src"}.Length : {srcOffset} + {length} > {srcLength}");
                    }
                }
                else if (srcOffset < 0)
                {
                    throw new ArgumentOutOfRangeException(srcOffsetName ?? "srcOffset",
                        $"{srcOffsetName ?? "srcOffset"} < 0 : {srcOffset} < 0");
                }

                if (dstOffset + length > dstLength)
                {
                    if (dstOffset >= dstLength)
                    {
                        throw new ArgumentException(
                            $"{dstOffsetName ?? "dstOffset"} >= {dstName ?? "dst"} : {dstOffset} >= {dstLength}");
                    }
                    else if (length > dstLength)
                    {
                        throw new ArgumentOutOfRangeException(lengthName ?? "length",
                            $"{lengthName ?? "length"} > {dstName ?? "dst"}.Length : {length} > {dstLength}");
                    }
                    else
                    {
                        throw new ArgumentException(
                            $"{dstOffsetName ?? "dstOffset"} + {lengthName ?? "length"} > {dstName ?? "dst"}.Length : {dstOffset} + {length} > {dstLength}");
                    }
                }
                else if (dstOffset < 0)
                {
                    throw new ArgumentOutOfRangeException(dstOffsetName ?? "dstOffset",
                        $"{dstOffsetName ?? "dstOffset"} < 0 : {dstOffset} < 0");
                }
            }
        }
    }
}
