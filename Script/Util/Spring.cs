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

namespace SonicGodot.Util
{
    public class Spring
    {
        public float Freq = 0.0f;
        public float Goal = 0.0f;
        public float Pos = 0.0f;
        public float Vel = 0.0f;

        public Spring(float freq, float pos = 0.0f)
        {
            Freq = freq;
            Goal = pos;
            Pos = pos;
            Vel = 0.0f;
        }

        public float Step(float dt)
        {
            float f = Freq * 2.0f * Godot.Mathf.Pi;
            float g = Goal;
            float p0 = Pos;
            float v0 = Vel;

            float offset = p0 - g;
            float decay = Godot.Mathf.Exp(-f * dt);

            float p1 = (offset * (1.0f + f * dt) + v0 * dt) * decay + g;
            float v1 = (v0 * (1.0f - f * dt) - offset * (f * f * dt)) * decay;

            Pos = p1;
            Vel = v1;

            return p1;
        }
    }

    public class Spring3D
    {
        private Spring _x;
        private Spring _y;
        private Spring _z;

        public float Freq
        {
            get { return _x.Freq; }
            set
            {
                _x.Freq = value;
                _y.Freq = value;
                _z.Freq = value;
            }
        }

        public Godot.Vector3 Goal
        {
            get { return new Godot.Vector3(_x.Goal, _y.Goal, _z.Goal); }
            set
            {
                _x.Goal = value.X;
                _y.Goal = value.Y;
                _z.Goal = value.Z;
            }
        }

        public Godot.Vector3 Pos
        {
            get { return new Godot.Vector3(_x.Pos, _y.Pos, _z.Pos); }
            set
            {
                _x.Pos = value.X;
                _y.Pos = value.Y;
                _z.Pos = value.Z;
            }
        }

        public Godot.Vector3 Vel
        {
            get { return new Godot.Vector3(_x.Vel, _y.Vel, _z.Vel); }
            set
            {
                _x.Vel = value.X;
                _y.Vel = value.Y;
                _z.Vel = value.Z;
            }
        }

        public Spring3D(float freq, Godot.Vector3 pos = new Godot.Vector3())
        {
            _x = new Spring(freq, pos.X);
            _y = new Spring(freq, pos.Y);
            _z = new Spring(freq, pos.Z);
        }
    }
}
