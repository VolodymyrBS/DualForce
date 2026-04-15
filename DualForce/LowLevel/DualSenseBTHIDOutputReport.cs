using System;
using System.Collections.Generic;
using System.IO.Hashing;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace DualForce.LowLevel
{
    [StructLayout(LayoutKind.Explicit, Size = kSize)]
    internal unsafe struct DualSenseBTHIDOutputReport : IInputDeviceCommandInfo
    {
        public static FourCC Type => new FourCC('H', 'I', 'D', 'O');

        internal const int PayloadSize = 74;
        internal const int CrcSize = 4;
        internal const int kSize = InputDeviceCommand.BaseCommandSize + PayloadSize + CrcSize;
        internal const int kTriggerParamSize = 9;
        internal const int kReportId = 2;

        [FieldOffset(0)] public InputDeviceCommand baseCommand;

        [FieldOffset(InputDeviceCommand.BaseCommandSize + 0)] public byte reportId;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 1)] public byte seq_tag;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 2)] public byte tag;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 3)] public Flags1 flags1;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 4)] public Flags2 flags2;

        [FieldOffset(InputDeviceCommand.BaseCommandSize + 5)] public byte lowFrequencyMotorSpeed;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 6)] public byte highFrequencyMotorSpeed;

        [FieldOffset(InputDeviceCommand.BaseCommandSize + 11)] public InternalMicLedState micLedState;
        
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 13)] public byte rightTriggerMode;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 14)] public fixed byte rightTriggerParams[kTriggerParamSize];
        
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 24)] public byte leftTriggerMode;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 25)] public fixed byte leftTriggerParams[kTriggerParamSize];
        
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 39)] public byte powerReduction;
        
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 41)] public LedFlags ledFlags;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 44)] public byte ledPulseOption;

        [FieldOffset(InputDeviceCommand.BaseCommandSize + 45)] public InternalPlayerLedBrightness playerLedBrightness;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 46)] public byte playerLedState;

        [FieldOffset(InputDeviceCommand.BaseCommandSize + 47)] public byte lightBarRed;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 48)] public byte lightBarGreen;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 49)] public byte lightBarBlue;
        
        [FieldOffset(kSize - 4)] public uint crc32;

        public FourCC typeStatic => Type;

        public static unsafe DualSenseBTHIDOutputReport FromUSBReport(ref byte seq, DualSenseUSBHIDOutputReport report)
        {
            if (seq == 16)
                seq = 0;
            
            var btReport = new DualSenseBTHIDOutputReport
            {
                baseCommand = new InputDeviceCommand(Type, kSize),
                reportId = 0x31,
                seq_tag = (byte)((seq << 4) & 0xF0),
                tag = 0x10,
                flags1 = report.flags1,
                flags2 = report.flags2,
                lowFrequencyMotorSpeed = report.lowFrequencyMotorSpeed,
                highFrequencyMotorSpeed = report.highFrequencyMotorSpeed,
                micLedState = report.micLedState,
                rightTriggerMode = report.rightTriggerMode,
                leftTriggerMode = report.leftTriggerMode,
                powerReduction = report.powerReduction,
                ledFlags = report.ledFlags,
                ledPulseOption = report.ledPulseOption,
                playerLedBrightness = report.playerLedBrightness,
                playerLedState = report.playerLedState,
                lightBarRed = report.lightBarRed,
                lightBarGreen = report.lightBarGreen,
                lightBarBlue = report.lightBarBlue,
            };
            seq++;

            for (int i = 0; i < kTriggerParamSize; i++)
            {
                btReport.leftTriggerParams[i] = report.leftTriggerParams[i];
                btReport.rightTriggerParams[i] = report.rightTriggerParams[i];
            }
            
            var crc = Crc32LE.Calculate(~0U, stackalloc byte[] { 0xA2 });
            crc = ~Crc32LE.Calculate(crc, new Span<byte>(&btReport.reportId, PayloadSize));
            btReport.crc32 = crc;

            return btReport;
        }
    }
}