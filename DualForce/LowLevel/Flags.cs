using System;

namespace DualForce.LowLevel
{
    [Flags]
    internal enum Flags1 : byte
    {
        MainMotors1 = 0x01,
        MainMotors2 = 0x02,
        RightTrigger = 0x04,
        LeftTrigger = 0x08,
    }

    [Flags]
    internal enum Flags2 : byte
    {
        MicLed = 0x01,
        SetLightBarColor = 0x04,
        PlayerLed = 0x10,
    }

    internal enum InternalMicLedState : byte
    {
        Off = 0x00,
        On = 0x01,
        Pulsating = 0x02,
    }

    [Flags]
    internal enum LedFlags : byte
    {
        PlayerLedBrightness = 0x01,
        LightBarFade        = 0x02,
    }

    internal enum InternalPlayerLedBrightness : byte
    {
        High = 0x0,
        Medium = 0x1,
        Low = 0x2,
    }
}