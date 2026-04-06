using UnityEngine;

namespace DualSenseSample.Inputs
{
    /// <summary>
    /// Component to set the DualSense rumble, aka motor speeds.
    /// <para>Set the <see cref="LeftRumble"/> and/or <see cref="RightRumble"/> Properties.</para>
    /// </summary>
    public class DualSenseRumble : AbstractDualSenseBehaviour
    {
        /// <summary>
        /// Speed of the low-frequency (left) motor. 
        /// Normalized [0..1] value with 1 indicating maximum speed 
        /// and 0 indicating the motor is turned off.
        /// </summary>
        public float LeftRumble { get; set; }

        private float _oldLeftRumble = -1;

        /// <summary>
        /// Speed of the high-frequency (right) motor. 
        /// Normalized [0..1] value with 1 indicating maximum speed 
        /// and 0 indicating the motor is turned off.
        /// </summary>
        public float RightRumble { get; set; }

        private float _oldRightRumble = -1;

        private void Update() => UpdateMotorSpeeds();

        private void UpdateMotorSpeeds()
        {
            if (!Mathf.Approximately(_oldRightRumble, RightRumble) || !Mathf.Approximately(_oldLeftRumble, LeftRumble))
            {
                Debug.Log($"Set: {LeftRumble}, {RightRumble}");
                DualSense?.SetMotorSpeeds(LeftRumble, RightRumble);
                DualSense?.ResumeHaptics();
                _oldLeftRumble = LeftRumble;
                _oldRightRumble = RightRumble;
            }
        }
    }
}
