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
	public partial class Ring : ObjectTriggerInterest, IObject
	{
		// Node setup
		[Export]
		public PackedScene m_ring_particle;


		public override void _Ready()
		{
			// Play ring spinning animation
			GetNode<AnimationPlayer>("Ring/AnimationPlayer").Play("RingSpin");
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
				if (player._state.HitObject(this))
				{
					// Add ring to player
					player.AddRings(1);
					player.AddScore(10);
					Root.Rpc(this, nameof(SendDeleteCall));
					
					DeleteRing();
				}
			}
		}
        private void SendDeleteCall() => Root.GetHostServer().RpcAll(this, nameof(DeleteRing));
        private void DeleteRing()
		{
			var sparkles = ResourceLoader.Load<PackedScene>("res://Particles/RingParticle.res").Instantiate() as GpuParticles3D;
			// Add the node as a child of the node the script is attached to.
			GetParent().AddChild(sparkles);
			sparkles.GlobalTransform = GlobalTransform;
			// Delete self
			QueueFree();
		}
		public bool CanLightDash() { return true; }

	}
}
