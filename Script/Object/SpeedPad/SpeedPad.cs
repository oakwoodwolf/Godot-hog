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
	public partial class SpeedPad : ObjectTriggerInterest, IObject
	{
		// Spring parameters
		[Export]
		float m_power = 6.0f;
		[Export]
		uint m_nocon = 60;

		// Debounce
		Debounce m_debounce = new Debounce();

		// Nodes
		private Util.IAudioStreamPlayer m_panel_sound;

		// Node setup
		public override void _Ready()
		{
			// Get nodes
			m_panel_sound = Util.IAudioStreamPlayer.FromNode(GetNode("PanelSound"));
			
			m_shape_node = GetNode<CollisionShape3D>("ColShape");
			m_listener_node = GetNode<StaticBody3D>(".");


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
				// Check if player is on ground
				if (!player.m_status.m_grounded)
					return;

				// Check debounce
				if (!m_debounce.Check())
					return;
				m_debounce.Set(10);

				// Play sound
				m_panel_sound.Play();

				// Launch player
				if (player.m_state.HitObject(this))
				{
					// Launch player velocity
					player.GlobalTransform = new Transform3D(GlobalTransform.Basis, player.GlobalTransform.Origin);
					player.Velocity = (GlobalTransform.Basis.Z * -(m_power * Root.c_tick_rate)) + (GlobalTransform.Basis.Y * player.Velocity.Dot(GlobalTransform.Basis.Y));
					player.m_input_speed.Set(m_nocon);
				}
			}
		}
	}
}
