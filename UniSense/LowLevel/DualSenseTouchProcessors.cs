using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting;

namespace DualForce.LowLevel
{
    [Preserve]
    public class DualSenseTouchActiveProcessor : InputProcessor<float>
    {
        public override float Process(float value, InputControl control)
        {
            return 1 - value;
        }
    }
    
    [Preserve]
    public class DualSenseTouchProcessor : InputProcessor<Vector2>
    {
        public override Vector2 Process(Vector2 value, InputControl control)
        {
            return new Vector2(value.x * 4095, value.y * 4095);
        }
    }

}
