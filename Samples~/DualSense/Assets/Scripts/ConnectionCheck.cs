using TMPro;
using DualForce;
using UnityEngine;
using UnityEngine.InputSystem;

public class ConnectionCheck : MonoBehaviour
{
    [SerializeField] private TMP_Text _connectionText;

    private void InputSystemOnonDeviceChange(InputDevice device, InputDeviceChange change)
    {
        _connectionText.text += $"[{change}] {device}:" +
                                $"    {device.description.manufacturer}" +
                                $"    {device.description.product}" +
                                $"    {device.description.capabilities}" +
                                $"";
    }

    
    
    public void Update()
    {
        if (DualSenseGamepadHID.FindCurrent() is { } gamepad)
        {
            _connectionText.text = gamepad.L2.value.ToString("0.00000") + " / " + gamepad.L2.pressPoint;
            _connectionText.text += gamepad.Accelerometer.value.ToString("0.00")
                                   + " = " + gamepad.Accelerometer.value.magnitude.ToString("0.00")
                                   + " " + Vector3.SignedAngle(gamepad.Accelerometer.value.normalized, Vector3.up,  Vector3.right).ToString("0.00")
                                   + "\n";
            _connectionText.text += gamepad.Gyroscope.value.ToString("0.00") + "\n";
            _connectionText.text += gamepad.Gyroscope.value.ToString("0.00") + "\n";
            _connectionText.text += gamepad.touch0Active.isPressed + ">"
                                    + gamepad.touch0Id.value
                                    + gamepad.touch0Position.value + "\n";

        }
    }

    public void Command()
    {
        if (DualSenseGamepadHID.FindCurrent() is { } gamepad)
        {
            var leftTrigger = new DualSenseTriggerState();
            DualSenseTriggerEffectGenerator.Feedback(
                leftTrigger.AsSpan(),
                0,
                4,
                5);

            // DualSenseTriggerEffectGenerator.Off(
            //     leftTrigger.AsSpan(),
            //     0);
            
            // DualSenseTriggerEffectGenerator.Bow(
            //     leftTrigger.AsSpan(),
            //     0,
            //     2,
            //     5,
            //     4,
            //     8);
            var rightTrigger = new DualSenseTriggerState()
            {
                EffectType = DualSenseTriggerEffectType.EffectEx,
                EffectEx = new DualSenseEffectExProperties()
                {
                    BeginForce = 255,
                    EndForce = 255,
                    Frequency = 5,
                    KeepEffect =  true,
                    MiddleForce = 255,
                    StartPosition = 0,
                }
            };
            // DualSenseTriggerEffectGenerator.MultiplePositionVibration(
            //     rightTrigger.AsSpan(),
            //     0,
            //     10,
            //     stackalloc  byte[10] { 1, 2, 3, 4, 5, 6, 7, 8, 8, 8});
            // DualSenseTriggerEffectGenerator.Vibration(
            //     rightTrigger.AsSpan(),
            //     0,
            //     8,
            //     1,
            //     10
            // );
            
            gamepad.SetGamepadState(new DualSenseGamepadState()
            {
                LightBarColor = Color.magenta,
                 LeftTrigger = leftTrigger,
                 RightTrigger = rightTrigger,
                PlayerLed = new PlayerLedState(0),
                //PlayerLedBrightness =  PlayerLedBrightness.Low,
                //MicLed = DualSenseMicLedState.Off
            });
            //gamepad.SetMotorSpeeds(0.5f, 0.5f);
            //gamepad.SetLightBarColor(Color.red);
        }
    }
}
