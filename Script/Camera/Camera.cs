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

namespace SonicGodot
{

    public partial class Camera : Camera3D
    {
        public enum CameraMode
        {
            Normal,
            Frozen,
        }
        // Target node
        [Export]
        public Node3D TargetNode;
        [Export]
        public float LerpFactor = 0.1f;
        [Export]
        public CameraMode Mode = CameraMode.Normal;
        private float _x = 0.0f;
        private float _y = -0.2f;

        private bool _locked = false;
        private bool _rightDown = false;

        private Util.Spring _zoom = new(2.0f, 1.0f);

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            // Capture our mouse
            SetLocked(true, false);
            if (OS.GetName() == "Android")
            {
                SetLocked(true,true);
            }
        }

        // Input
        private void SetLocked(bool locked, bool right_down)
        {
            _locked = locked;
            _rightDown = right_down;

            if (_locked || _rightDown)
                Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Captured;
            else
                Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Visible;
        }

        public override void _Input(InputEvent motionUnknown)
        {
            InputEventMouseMotion motion = motionUnknown as InputEventMouseMotion;
            if (motion != null)
            {
                if (_locked || _rightDown)
                {
                    _x += motion.Relative.X * -0.008f;
                    _y += motion.Relative.Y * -0.005f;
                }
            }

            InputEventMouseButton button = motionUnknown as InputEventMouseButton;
            if (button != null)
            {
                if (button.IsPressed())
                {
                    if (button.ButtonIndex == MouseButton.WheelUp)
                        _zoom.Goal -= 0.1f;
                    if (button.ButtonIndex == MouseButton.WheelDown)
                        _zoom.Goal += 0.1f;

                    if (button.ButtonIndex == MouseButton.Middle)
                        SetLocked(!_locked, _rightDown);
                    if (button.ButtonIndex == MouseButton.Right)
                        SetLocked(_locked, true);
                }
                else
                {
                    if (button.ButtonIndex == MouseButton.Right)
                        SetLocked(_locked, false);
                }

                _zoom.Goal = Mathf.Clamp(_zoom.Goal, 0.4f, 1.2f);
            }
        }

        // Update
        public override void _Process(double delta)
        {
            switch (Mode)
            {
                case CameraMode.Frozen:
                    Transform = Transform.LookingAt(TargetNode.GlobalPosition);
                    break;
                default:
                    ProcessCamera(delta);
                    break;

            }



            // Move camera by rotate vector
            Vector2 rotate_vector = Input.Server.GetLookVector();
            _x += rotate_vector.X * -4.0f * (float)delta;
            _y += rotate_vector.Y * 3.0f * (float)delta;

            // Limit camera
            _x %= Mathf.Pi * 2.0f;
            _y = Mathf.Clamp(_y, Mathf.Pi * -0.499f, Mathf.Pi * 0.499f);

        }

        private void ProcessCamera(double delta)
        {
            // Zoom camera
            _zoom.Step((float)delta);
            // Move behind the target node
            var transformDestination = TargetNode.GlobalTransform;
            transformDestination.Basis = new Basis(new Vector3(0.0f, 1.0f, 0.0f), _x) * new Basis(new Vector3(1.0f, 0.0f, 0.0f), _y);
            transformDestination.Origin += transformDestination.Basis.Z * 15.0f * _zoom.Pos;
            transformDestination.Origin.Y += 3.5f * _zoom.Pos;
            Transform = Transform.InterpolateWith(transformDestination, LerpFactor);
        }
    }
}
