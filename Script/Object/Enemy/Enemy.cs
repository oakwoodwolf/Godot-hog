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
	public partial class Enemy : ObjectTriggerInterest, IObject
	{
		// Spring parameters
		[Export]
		float m_power = 6.0f;
		[Export]
		int m_nocon = 60;
		[Export]
		bool m_roll = false;

		// Debounce
		Debounce m_debounce = new Debounce();

		// Nodes
		private AnimationPlayer m_animation_player;

		private Util.IAudioStreamPlayer m_enemy_sound;

		// Node setup
		public override void _Ready()
		{
			// Get nodes
			m_animation_player = GetNode<AnimationPlayer>("Model/AnimationPlayer");
			m_shape_node = GetNode<CollisionShape3D>("ColShape");
			m_listener_node = GetNode<StaticBody3D>(".");
			m_enemy_sound = Util.IAudioStreamPlayer.FromNode(GetNode("BounceSound"));

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
				// Check debounce
				if (!m_debounce.Check())
					return;
				m_debounce.Set(10);

				

				// Launch player
				if (player.m_state.HitObject(this) && (!player.m_status.m_invincible && !player.m_status.m_hurt))
				{
                    m_enemy_sound.Play();
                    m_animation_player.Play("SpringBounce");

                    // Set player state
                    player.SetStateHurt();

					player.m_ability.FlagHitBounce();

					player.m_status.m_grounded = false;
				}
			}
		}

		// Object properties
		public bool CanHomingAttack() { return true; }
	}
}
