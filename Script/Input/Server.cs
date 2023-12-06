/*
 * [ Sonic Onset Adventure]
 * Copyright (c) 2023 Regan "CKDEV" Green
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
*/

using Godot;
using System.Collections.Generic;
using System.Linq;

namespace SonicGodot.Input
{
    // Input server singleton
    // Due to some bugs in Godot's input mapping, we have to implement our own input server for processing the InputMap
    public partial class Server : Godot.Node
    {
        // Input state
        private Dictionary<Key, bool> _key = new Dictionary<Key, bool>();
        private Dictionary<MouseButton, bool> _mouseButton = new Dictionary<MouseButton, bool>();

        private float[] _joyAxis = new float[(int)JoyAxis.Max];
        private bool[] _joyButton = new bool[(int)JoyButton.Max];

        // Cached mapped input state
        private struct CachedAction
        {
            // Action information
            public Godot.Collections.Array<InputEvent> Events;
            public float Deadzone;

            // Action state
            public float Value;

            public float RawValue;
            public float JoyValue;
        }
        private Dictionary<StringName, CachedAction> _cachedActions = new();

        // Input vectors
        private Vector2 _moveVector = new Vector2();
        private Vector2 _lookVector = new Vector2();

        // Singleton
        public override void _Ready()
        {
            // Cache actions
            CacheActions();

            // Register singleton
            ProcessPriority = (int)Enum.Priority.PreProcess;
            Engine.RegisterSingleton("InputServer", this);
        }

        internal static Server Singleton()
        {
            // Get singleton
            return (Server)Engine.GetSingleton("InputServer");
        }

        // Input state event
        public override void _Input(InputEvent input_event)
        {
            // Process keys
            if (input_event is InputEventKey key)
            {
                _key[key.PhysicalKeycode] = key.Pressed;
            }

            // Process mouse buttons
            if (input_event is InputEventMouseButton mouseButton)
            {
                _mouseButton[mouseButton.ButtonIndex] = mouseButton.Pressed;
            }

            // Process joypad axes
            if (input_event is InputEventJoypadMotion joypadMotion)
            {
                _joyAxis[(int)joypadMotion.Axis] = joypadMotion.AxisValue;
            }

            // Process joypad buttons
            if (input_event is InputEventJoypadButton joypadButton)
            {
                _joyButton[(int)joypadButton.ButtonIndex] = joypadButton.Pressed;
            }
        }

        // Input processing
        public override void _Process(double delta)
        {
            // Process actions
            ProcessActions();
        }

        // Internal input functions
        public void CacheActions()
        {
            // Get all action names
            Godot.Collections.Array<StringName> actions = InputMap.GetActions();

            // Reset cached actions
            _cachedActions.Clear();

            // Setup new actions
            foreach (var action_name in actions)
            {
                // Setup cached
                CachedAction cached = new CachedAction();

                cached.Events = InputMap.ActionGetEvents(action_name);
                cached.Deadzone = InputMap.ActionGetDeadzone(action_name);

                cached.Value = 0.0f;

                _cachedActions[action_name] = cached;
            }

            // Process actions so that they are updated
            ProcessActions();
        }

        private void ProcessActions()
        {
            // Process cached actions
            foreach (var key in _cachedActions.Keys.ToList())
            {
                // Get cached action
                CachedAction cached = _cachedActions[key];

                // Process events
                float final_value = 0.0f;

                float raw_value = 0.0f;
                float joy_value = 0.0f;

                float deadzone = cached.Deadzone;

                foreach (var input_event in cached.Events)
                {
                    // Get value
                    float value = input_event switch
                    {
                        InputEventJoypadMotion joypad_motion => _joyAxis[(int)joypad_motion.Axis] * joypad_motion.AxisValue,
                        _ => GetEventValue(input_event)
                    };
                    {
                        if (input_event is InputEventJoypadMotion joypad_motion)
                        {
                            joy_value += _joyAxis[(int)joypad_motion.Axis] * joypad_motion.AxisValue;
                        }
                        else
                        {
                            raw_value += GetEventValue(input_event);
                        }
                    }

                    // Apply deadzone and add
                    if (value > deadzone)
                    {
                        final_value += (value - deadzone) / (1.0f - deadzone);
                    }
                }

                // Set value
                cached.Value = Mathf.Min(final_value, 1.0f);

                cached.RawValue = raw_value;
                cached.JoyValue = joy_value;

                // Set cached action
                _cachedActions[key] = cached;
            }

            // Get vectors
            _moveVector = GetVectorValue("move_left", "move_right", "move_up", "move_down");
            _lookVector = GetVectorValue("look_left", "look_right", "look_up", "look_down");
        }

        private float GetEventValue(Godot.InputEvent input_event)
        {
            if (input_event is InputEventKey key)
                return _key.GetValueOrDefault(key.PhysicalKeycode) ? 1.0f : 0.0f;
            if (input_event is InputEventMouseButton mouse_button)
                return _mouseButton.GetValueOrDefault(mouse_button.ButtonIndex) ? 1.0f : 0.0f;
            if (input_event is InputEventJoypadMotion joypad_motion)
                return _joyAxis[(int)joypad_motion.Axis];
            if (input_event is InputEventJoypadButton joypad_button)
                return _joyButton[(int)joypad_button.ButtonIndex] ? 1.0f : 0.0f;
            return 0.0f;
        }

        private static Vector2 DeadzoneVector(Vector2 vector, float deadzone)
        {
            float length = vector.Length();
            if (length < deadzone)
            {
                return Vector2.Zero;
            }
            else
            {
                return length < 1.0f ? vector * ((length - deadzone) / (1.0f - deadzone)) : vector.Normalized();
            }
        }

        private Vector2 GetVectorValue(string left_name, string right_name, string up_name, string down_name)
        {
            // Get raw and joy vectors
            CachedAction cached_left = _cachedActions[left_name];
            CachedAction cached_right = _cachedActions[right_name];
            CachedAction cached_up = _cachedActions[up_name];
            CachedAction cached_down = _cachedActions[down_name];
            float deadzone = cached_left.Deadzone;

            Vector2 raw_vector = DeadzoneVector(new Vector2(
                cached_right.RawValue - cached_left.RawValue,
                cached_up.RawValue - cached_down.RawValue
            ), deadzone);
            Vector2 joy_vector = DeadzoneVector(new Vector2(
                cached_right.JoyValue - cached_left.JoyValue,
                cached_up.JoyValue - cached_down.JoyValue
            ), deadzone);

            // Get final vector
            Vector2 vector = raw_vector + joy_vector;
            if (vector.LengthSquared() > 1.0f)
            {
                vector = vector.Normalized();
            }

            return vector;
        }

        // Input functions
        public static Vector2 GetMoveVector()
        {
            // Return move vector
            return Singleton()._moveVector;
        }
        public static void SetMoveVector(Vector2 vector)
        {
            // Return move vector
            Singleton()._moveVector = vector;
        }

        public static Vector2 GetLookVector()
        {
            // Return look vector
            return Singleton()._lookVector;
        }

        public static bool GetButton(string name)
        {
            // Return mapped button
            return Singleton()._cachedActions[name].Value > 0.0f;
        }
    }
}
