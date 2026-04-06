using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DualForce.LowLevel
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct DualSenseHIDMinimalInputReport
    {
        public static int ExpectedSize1 = 10;
        public static int ExpectedSize2 = 78;

        [FieldOffset(0)] public byte reportId;
        [FieldOffset(1)] public byte leftStickX;
        [FieldOffset(2)] public byte leftStickY;
        [FieldOffset(3)] public byte rightStickX;
        [FieldOffset(4)] public byte rightStickY;
        [FieldOffset(5)] public byte buttons1;
        [FieldOffset(6)] public byte buttons2;
        [FieldOffset(7)] public byte buttons3;
        [FieldOffset(8)] public byte leftTrigger;
        [FieldOffset(9)] public byte rightTrigger;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void ToHIDInputReport(DualSenseHIDInputReport* btReport)
        {
            btReport->reportId = 0x31;
            btReport->leftStickX = leftStickX;
            btReport->leftStickY = leftStickY;
            btReport->rightStickX = rightStickX;
            btReport->rightStickY = rightStickY;
            btReport->leftTrigger = leftTrigger;
            btReport->rightTrigger = rightTrigger;
            btReport->buttons1 = buttons1;
            btReport->buttons2 = buttons2;
            btReport->buttons3 = (byte)(buttons3 &
                                        0x03); // higher bits seem to contain random data, and mic button is not supported
            btReport->accelX = 0;
            btReport->accelY = 0;
            btReport->accelZ = 0;
            btReport->gyroPitch = 0;
            btReport->gyroRoll = 0;
            btReport->gyroYaw = 0;
            btReport->touch0Contact = 0x80;
            btReport->touch1Contact = 0x80;
            btReport->batteryInfo1 = 0;
            btReport->batteryInfo2 = 0xFF;
        }
    }
}