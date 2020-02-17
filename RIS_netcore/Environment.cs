using System;
using System.Runtime.InteropServices;

namespace RIS
{
    public static class Environment
    {
        public static uint LargeObjectHeapEntrySizeLimit { get; } = 84000;

        public static uint GetElementSize<T>()
        {
            Type typeT = typeof(T);
            int size;
            if (typeT.IsValueType)
            {
                if (typeT.IsGenericType)
                {
                    var defaultT = default(T);
                    size = Marshal.SizeOf(defaultT);
                }
                else
                {
                    size = Marshal.SizeOf(typeT);
                }
            }
            else
            {
                size = IntPtr.Size;
            }
            return (uint)size;
        }
    }
}
