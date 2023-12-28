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
	public partial class DroppedRing : RigidBody3D, IObject
	{
		private float flickerCounter;
		private Node3D m_modelroot;
		public override void _Ready()
		{
			m_modelroot = GetNode<Node3D>("Ring");
			GetNode<Ring>("Ring").LightDashable = false;
			base._Ready();
		}
		public override void _Process(double delta)
		{
			SkinFlicker();
		}
		void SkinFlicker()
		{
			flickerCounter += 2;
			if (flickerCounter > 0)
			{
				m_modelroot.Visible = true;
			}
			else
			{
				m_modelroot.Visible = false;
			}
			if (flickerCounter > 10)
			{
				flickerCounter = -10;
			}

		}
		public bool CanLightDash() { return false; }
		private void _on_timer_timeout()
		{
			QueueFree();
		}
	}
}





