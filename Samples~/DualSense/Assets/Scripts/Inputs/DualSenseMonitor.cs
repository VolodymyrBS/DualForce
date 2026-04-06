using System.Collections.Generic;
using System.Linq;
using DualForce;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DualSenseSample.Inputs
{
    /// <summary>
    /// Device monitor for DualSense gamepad.
    /// <para>
    /// Notifies all listeners about a DualSense connection or disconnection and 
    /// resets a DualSense instance when disabled.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DualSenseMonitor : MonoBehaviour
    {
        [SerializeField] private Text _controllerType;
        
        private AbstractDualSenseBehaviour[] listeners;

        private void Start()
        {
            _controllerType.text = Gamepad.current?.GetType().FullName ?? "<None>";
            
            listeners = GetComponentsInChildren<AbstractDualSenseBehaviour>();
            var dualSense = DualSenseGamepadHID.FindCurrent();
            var isDualSenseConected = dualSense != null;
            if (isDualSenseConected) NotifyConnection(dualSense);
            else NotifyDisconnection();
        }

        private void OnEnable() => InputSystem.onDeviceChange += OnDeviceChange;

        private void OnDisable()
        {
            InputSystem.onDeviceChange -= OnDeviceChange;
            var dualSense = DualSenseGamepadHID.FindCurrent();
            dualSense?.Reset();
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (device is Gamepad gamepad)
                _controllerType.text = gamepad.GetType().FullName;
            
            var isNotDualSense = !(device is DualSenseGamepadHID);
            if (isNotDualSense) return;

            switch (change)
            {
                case InputDeviceChange.Added:
                    NotifyConnection(device as DualSenseGamepadHID);
                    break;
                case InputDeviceChange.Reconnected:
                    NotifyConnection(device as DualSenseGamepadHID);
                    break;
                case InputDeviceChange.Disconnected:
                    NotifyDisconnection();
                    break;
            }
        }

        private void NotifyConnection(DualSenseGamepadHID dualSense)
        {
            var controlls = new List<InputControl>();
            dualSense.FindControlsRecursive(controlls, t => true);
            
            controlls.ForEach(c => Debug.Log($"{c.device}: {c.path}; {c.GetType()}"));
            Debug.LogError(controlls.OfType<UnityEngine.InputSystem.Controls.AxisControl>().First(x => x.path.Contains("batteryLevel"))
                .value);
            
            foreach (var listener in listeners)
            {
                listener.OnConnect(dualSense);
            }
        }

        private void NotifyDisconnection()
        {
            foreach (var listener in listeners)
            {
                listener.OnDisconnect();
            }
        }
    }
}
