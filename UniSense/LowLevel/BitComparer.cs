using System;
using System.Runtime.CompilerServices;

namespace DualForce.LowLevel
{
    public class BitComparer
    {
        public static bool Equals<T>(ref T a1, ref T a2)
            where T : struct
        {
            var size = Unsafe.SizeOf<T>();
            ref byte s1 = ref Unsafe.As<T, byte>(ref a1);
            ref byte s2 = ref Unsafe.As<T, byte>(ref a2);
            int readSize = IntPtr.Size;
            while (size > 0)
            {
                while (readSize > size)
                    readSize /= 2;

                Console.WriteLine(readSize + " " + size);
                switch (readSize)
                {
                    case 8:
                        if (
                            Unsafe.As<byte, long>(ref s1) !=
                            Unsafe.As<byte, long>(ref s2)
                        )
                            return false;
                        break;
                    case 4:
                        if (
                            Unsafe.As<byte, int>(ref s1) !=
                            Unsafe.As<byte, int>(ref s2)
                        )
                            return false;
                        break;
                    case 2:
                        if (
                            Unsafe.As<byte, short>(ref s1) !=
                            Unsafe.As<byte, short>(ref s2)
                        )
                            return false;
                        break;
                    case 1:
                        if (s1 != s2)
                            return false;
                        break;
                }

                size -= readSize;
                s1 = ref Unsafe.Add(ref s1, readSize);
                s2 = ref Unsafe.Add(ref s2, readSize);

            }
            return true;
        }
    }
}