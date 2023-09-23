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

namespace SonicOnset
{
	public partial class Ring : ObjectTriggerInterest, IObject
	{
		// Node setup
		public override void _Ready()
		{
			// Play ring spinning animation
			GetNode<AnimationPlayer>("Ring/AnimationPlayer").Play("RingSpin");

			// Setup base
			base._Ready();
		}

		// Trigger listener
		public void Touch(Node3D other)
		{
			// Check if player
			Player player = other as Player;
			if (player != null)
			{
				if (player.m_state.HitObject(this))
				{
					// Add ring to player
					player.AddRings(1);
					player.AddScore(10);

					// Delete self
					QueueFree();
				}
			}
		}
	}
}
