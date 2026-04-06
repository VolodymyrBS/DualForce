using System;
using System.Runtime.CompilerServices;
using DualForce.LowLevel;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.HID;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Scripting;

namespace DualForce
{
    [InputControlLayout(
        stateType = typeof(DualSenseHIDInputReport),
        displayName = "PS5 Controller")]
    [Preserve]
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class DualSenseGamepadHID : DualShockGamepad, IInputStateCallbackReceiver
    {
        public ButtonControl leftTriggerButton { get; protected set; }
        public ButtonControl rightTriggerButton { get; protected set; }
        public ButtonControl playStationButton { get; protected set; }
        public ButtonControl micMuteButton { get; protected set; }

        public Vector3Control Gyroscope { get; protected set; }
        public Vector3Control Accelerometer { get; protected set; }

        public ButtonControl touch0Active { get; protected set; }
        public IntegerControl touch0Id { get; protected set; }
        public Vector2Control touch0Position { get; protected set; }
        public ButtonControl touch1Active { get; protected set; }
        public IntegerControl touch1Id { get; protected set; }
        public Vector2Control touch1Position { get; protected set; }
        
        private byte _btSeqNum = 0;
        private bool _isBt;
#if UNITY_EDITOR
        static DualSenseGamepadHID()
        {
            Initialize();
        }
#endif

        /// <summary>
        /// Finds the first DualSense connected by the player or <c>null</c> if 
        /// there is no one connected to the system.
        /// </summary>
        /// <returns>A DualSenseGamepadHID instance or <c>null</c>.</returns>
        public static DualSenseGamepadHID FindFirst()
        {
            foreach (var gamepad in all)
            {
                var isDualSenseGamepad = gamepad is DualSenseGamepadHID;
                if (isDualSenseGamepad) return gamepad as DualSenseGamepadHID;
            }

            return null;
        }

        /// <summary>
        /// Finds the DualSense last used/connected by the player or <c>null</c> if 
        /// there is no one connected to the system.
        /// </summary>
        /// <returns>A DualSenseGamepadHID instance or <c>null</c>.</returns>
        public static DualSenseGamepadHID FindCurrent() => Gamepad.current as DualSenseGamepadHID;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            InputSystem.RegisterProcessor<DualSenseTouchProcessor>();
            InputSystem.RegisterProcessor<DualSenseTouchActiveProcessor>();
            
            InputSystem.RegisterLayout<DualSenseGamepadHID>(
                matches: new InputDeviceMatcher()
                    .WithInterface("HID")
                    .WithCapability("vendorId", 0x54C)
                    .WithCapability("productId", 0xCE6)
                );
        }

        protected override void FinishSetup()
        {
            leftTriggerButton = GetChildControl<ButtonControl>("leftTriggerButton");
            rightTriggerButton = GetChildControl<ButtonControl>("rightTriggerButton");
            playStationButton = GetChildControl<ButtonControl>("systemButton");
            micMuteButton = GetChildControl<ButtonControl>("micMuteButton");
            Gyroscope = GetChildControl<Vector3Control>("gyro");
            Accelerometer = GetChildControl<Vector3Control>("accel");

            touch0Active = GetChildControl<ButtonControl>("touch0Active");
            touch0Id = GetChildControl<IntegerControl>("touch0Id");
            touch0Position = GetChildControl<Vector2Control>("touch0Position");
            touch1Active = GetChildControl<ButtonControl>("touch1Active");
            touch1Id = GetChildControl<IntegerControl>("touch1Id");
            touch1Position = GetChildControl<Vector2Control>("touch1Position");

            var hidDeviceDescriptor = HID.HIDDeviceDescriptor.FromJson(description.capabilities);
            _isBt = hidDeviceDescriptor.inputReportSize > 64;
            
            base.FinishSetup();
        }

        private bool MotorHasValue => m_LowFrequencyMotorSpeed.HasValue || m_HighFrequenceyMotorSpeed.HasValue;
        private bool LeftTriggerHasValue => m_leftTriggerState.HasValue;
        private bool RightTriggerHasValue => m_rightTriggerState.HasValue;

        public override void PauseHaptics()
        {
            if (!MotorHasValue && !LeftTriggerHasValue && !RightTriggerHasValue)
                return;

            var command = DualSenseUSBHIDOutputReport.Create();
            command.ResetMotorSpeeds();
            command.SetLeftTriggerState(new DualSenseTriggerState());
            command.SetRightTriggerState(new DualSenseTriggerState());

            PrecessAndExecuteCommand(ref command);
        }

        public override void ResetHaptics()
        {
            if (!MotorHasValue && !LeftTriggerHasValue && !RightTriggerHasValue)
                return;

            var command = DualSenseUSBHIDOutputReport.Create();
            command.ResetMotorSpeeds();
            command.SetLeftTriggerState(new DualSenseTriggerState());
            command.SetRightTriggerState(new DualSenseTriggerState());

            PrecessAndExecuteCommand(ref command);

            m_HighFrequenceyMotorSpeed = null;
            m_LowFrequencyMotorSpeed = null;
        }

        public void ResetMotorSpeeds() => SetMotorSpeeds(0f, 0f);

        public void ResetLightBarColor() => SetLightBarColor(Color.black);

        public void ResetTriggersState()
        {
            var command = DualSenseUSBHIDOutputReport.Create();
            command.SetRightTriggerState(m_rightTriggerState.Value);
            command.SetLeftTriggerState(m_leftTriggerState.Value);

            PrecessAndExecuteCommand(ref command);
        }

        public void Reset()
        {
            ResetHaptics();
            ResetMotorSpeeds();
            ResetLightBarColor();
            ResetTriggersState();
        }

        public override void ResumeHaptics()
        {
            if (!MotorHasValue && !LeftTriggerHasValue && !RightTriggerHasValue)
                return;

            var command = DualSenseUSBHIDOutputReport.Create();
            if (MotorHasValue) command.SetMotorSpeeds(m_LowFrequencyMotorSpeed.Value, m_HighFrequenceyMotorSpeed.Value);
            if (LeftTriggerHasValue) command.SetLeftTriggerState(m_leftTriggerState.Value);
            if (RightTriggerHasValue) command.SetRightTriggerState(m_rightTriggerState.Value);

            PrecessAndExecuteCommand(ref command);
        }

        public override void SetLightBarColor(Color color)
        {
            var command = DualSenseUSBHIDOutputReport.Create();
            command.SetLightBarColor(color);

            PrecessAndExecuteCommand(ref command);
        }

        public override void SetMotorSpeeds(float lowFrequency, float highFrequency)
        {
            var command = DualSenseUSBHIDOutputReport.Create();
            command.SetMotorSpeeds(lowFrequency, highFrequency);

            PrecessAndExecuteCommand(ref command);

            m_LowFrequencyMotorSpeed = lowFrequency;
            m_HighFrequenceyMotorSpeed = highFrequency;
        }

        public void SetGamepadState(DualSenseGamepadState state)
        {
            var command = DualSenseUSBHIDOutputReport.Create();

            if (state.LightBarColor.HasValue)
            {
                var lightBarColor = state.LightBarColor.Value;
                command.SetLightBarColor(lightBarColor);
            }

            if (state.Motor.HasValue)
            {
                var motor = state.Motor.Value;
                command.SetMotorSpeeds(motor.LowFrequencyMotorSpeed, motor.HighFrequenceyMotorSpeed);
                m_LowFrequencyMotorSpeed = motor.LowFrequencyMotorSpeed;
                m_HighFrequenceyMotorSpeed = motor.HighFrequenceyMotorSpeed;
            }

            if (state.MicLed.HasValue)
            {
                var micLed = state.MicLed.Value;
                command.SetMicLedState(micLed);
            }

            if (state.RightTrigger.HasValue)
            {
                var rightTriggerState = state.RightTrigger.Value;
                command.SetRightTriggerState(rightTriggerState);
                m_rightTriggerState = rightTriggerState;
            }

            if (state.LeftTrigger.HasValue)
            {
                var leftTriggerState = state.LeftTrigger.Value;
                command.SetLeftTriggerState(leftTriggerState);
                m_leftTriggerState = leftTriggerState;
            }

            if (state.PlayerLedBrightness.HasValue)
            {
                var playerLedBrightness = state.PlayerLedBrightness.Value;
                command.SetPlayerLedBrightness(playerLedBrightness);
            }

            if (state.PlayerLed.HasValue)
            {
                var playerLed = state.PlayerLed.Value;
                command.SetPlayerLedState(playerLed);
            }

            PrecessAndExecuteCommand(ref command);
        }
        
        

        private protected virtual void PrecessAndExecuteCommand(ref DualSenseUSBHIDOutputReport command)
        {
            if (!_isBt)
            {
                ExecuteCommand(ref command);
                return;
            }
            
            var btCommand = DualSenseBTHIDOutputReport.FromUSBReport(ref _btSeqNum, command);
            unsafe
            {
                var s = new Span<byte>(&btCommand, sizeof(DualSenseBTHIDOutputReport));
                var l = "";
                foreach (var i in s)
                {
                    l += "0x" + i.ToString("X2") + " ";
                }

                Debug.Log(l);
            }

            ExecuteCommand(ref btCommand);
        }
        
        public void OnNextUpdate()
        { }

        public unsafe void OnStateEvent(InputEventPtr eventPtr)
        {
            if (eventPtr.type != StateEvent.Type)
            {
                InputState.Change(this, eventPtr);
                return;
            }
            
            var stateEventPtr = (StateEvent*)eventPtr.data;

            var reportId = *(byte*)stateEventPtr->state;

            if (_isBt)
            {
                switch (reportId)
                {
                    case 0x01:
                    {
                        var minimalHid = *(DualSenseHIDMinimalInputReport*)stateEventPtr->state;
                        minimalHid.ToHIDInputReport((DualSenseHIDInputReport*)stateEventPtr->state);
                        break;
                    }
                    case 0x31 when !ValidateCrc(stateEventPtr):
                        return;
                    case 0x31:
                    {
                        var hidReport = *(DualSenseHIDInputReport*)((byte*)stateEventPtr->state + 1);
                        hidReport.reportId = reportId;
                        Unsafe.AsRef<DualSenseHIDInputReport>(stateEventPtr->state) = hidReport;
                        break;
                    }
                }
            }
            
            InputState.Change(this, eventPtr);
        }

        private static unsafe bool ValidateCrc(StateEvent* stateEventPtr)
        {
            var crcCalculated = Crc32LE.Calculate(uint.MaxValue, stackalloc byte[] { 0xA1 });
            crcCalculated = ~Crc32LE.Calculate(crcCalculated, new Span<byte>(stateEventPtr->state, (int)(stateEventPtr->stateSizeInBytes - 4)));
            var crcReceived = *(uint*)((byte*)stateEventPtr->state + stateEventPtr->stateSizeInBytes - 4);
            if (crcCalculated == crcReceived)
                return true;
            Debug.LogAssertionFormat("Crc validation failed. Expected {0}, got {1}", crcCalculated, crcReceived);
            return false;

        }

        public bool GetStateOffsetForEvent(InputControl control, InputEventPtr eventPtr, ref uint offset)
        {
            return false;
        }

        private float? m_LowFrequencyMotorSpeed;
        private float? m_HighFrequenceyMotorSpeed;
        private DualSenseTriggerState? m_rightTriggerState;
        private DualSenseTriggerState? m_leftTriggerState;
    }
}