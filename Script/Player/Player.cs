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

// Big TODOs:
// - Just clean up the code in general, this is a mess

using Godot;
using System.Collections.Generic;
namespace SonicGodot
{
	// Player controller class
	public partial class Player : CharacterBody3D, IObject
	{
		// Player nodes
		[Export]
		internal Camera m_camera_node;
		[Export]
		public ColorRect SpeedLines;
		public Tween SpeedTween;

		private string currentAnim;
		internal CollisionShape3D m_main_colshape_node;
		internal CollisionShape3D m_roll_colshape_node;

		internal PlayerParam m_param_node;

		internal ObjectTriggerInterest m_radial_trigger;
		private ObjectTriggerInterest m_attack_trigger;

		[Signal]
		public delegate void onPlayerHurtSignalEventHandler();
		[Signal]
		public delegate void onPlayerDeathEventHandler();


		internal Character.ModelRoot m_modelroot;
		internal Transform3D m_modelroot_offset;
		private float m_hurt_counter, m_flicker_counter, m_dead_counter, m_rings_to_release;
		private bool m_releasing_rings = false;
		public Node3D releaseDirection;

		// Debug context
		// private UI.DebugUI.DebugUI.DebugUIContext m_debug_context = UI.DebugUI.DebugUI.AcquireContext("Player");

		// Player physics state
		[Export]
		public Vector3 m_gravity = -Vector3.Up;

		internal Param m_param = new Param();

		private PhysicsDirectSpaceState3D m_directspace;
		private PhysicsRayQueryParameters3D m_ray_query;

		// Player game state
		public ulong m_score { get; private set; } = 0;
		public ulong m_time { get; private set; } = 0;
		public uint m_rings { get; private set; } = 0;
		public bool hasHomed = false;
		public void AddScore(ulong score)
		{
			m_score += score;
		}

		public void AddRings(uint rings)
		{
			m_rings += rings;
		}
		public void SetRings(uint rings)
		{
			m_rings = rings;
		}

		// Player status
		public struct Status
		{
			public bool m_grounded = true;
			public bool m_hurt = false;
			public bool m_dead = false;
			public bool m_invincible = false;

			public Status() { }
		}
		public Status m_status = new Status();

		// Player input state
		public Input.Stick m_input_stick = new Input.Stick();
		public Input.Button m_input_jump = new Input.Button();
		public Input.Button m_input_spin = new Input.Button();
		public Input.Button m_input_secondary = new Input.Button();
		public Input.Button m_input_tertiary = new Input.Button();
		public Input.Button m_input_quaternary = new Input.Button();
		public Input.Button m_input_debug_respawn = new Input.Button();


		public Debounce m_input_stop = new Debounce();
		public Debounce m_input_speed = new Debounce();

		// Player ability
		public partial class Ability
		{
			// Parent player
			internal Player _parent;

			// Ability interface
			virtual internal bool CheckJump() { return false; }
			virtual internal bool CheckJumpAbility() { return false; }
			virtual internal bool CheckFallAbility() { return false; }
			virtual internal bool CheckSpinAbility() { return false; }
			virtual internal bool CheckLandAbility() { return false; }
			virtual internal bool CheckHurtAbility() { return false; }


			virtual internal void FlagHitBounce() { } // When you bounce off an object, for special abilities like Air Kick and Chaos Snap
			virtual internal void ClearHitBounce() { } // Performing an ability should clear the bounce flag

			// Ability list class
			public partial class AbilityList : Ability
			{
				// Abilities
				internal System.Collections.Generic.List<Ability> m_abilities = new System.Collections.Generic.List<Ability>();

				// Ability list interface
				internal override bool CheckJump()
				{
					foreach (Ability ability in m_abilities)
					{
						if (ability.CheckJump())
						{
							ClearHitBounce();
							return true;
						}
					}
					return false;
				}

				internal override bool CheckJumpAbility()
				{
					foreach (Ability ability in m_abilities)
					{
						if (ability.CheckJumpAbility())
						{
							ClearHitBounce();
							return true;
						}
					}
					return false;
				}

				internal override bool CheckFallAbility()
				{
					foreach (Ability ability in m_abilities)
					{
						if (ability.CheckFallAbility())
						{
							ClearHitBounce();
							return true;
						}
					}
					return false;
				}

				internal override bool CheckSpinAbility()
				{
					foreach (Ability ability in m_abilities)
					{
						if (ability.CheckSpinAbility())
						{
							ClearHitBounce();
							return true;
						}
					}
					return false;
				}

				internal override bool CheckLandAbility()
				{
					foreach (Ability ability in m_abilities)
					{
						if (ability.CheckLandAbility())
						{
							ClearHitBounce();
							return true;
						}
					}
					return false;
				}

				internal override void FlagHitBounce()
				{
					foreach (Ability ability in m_abilities)

						ability.FlagHitBounce();
				}

				internal override void ClearHitBounce()
				{
					foreach (Ability ability in m_abilities)
						ability.ClearHitBounce();
				}
			}
		}

		public Ability m_ability { get; internal set; }

		// Player state
		public partial class State
		{
			// Parent player
			internal Player m_parent;

			// State interface
			virtual internal void Ready() { }
			virtual internal void Stop() { }
			/// <summary>
			/// Called every frame the state is active.
			/// </summary>
			virtual internal void AbilityProcess() { }
			virtual internal void Process() { }

			virtual internal void Debug(List<string> debugs) { }

			// State overrides
			/// <summary>
			/// Allows Dynamic bones to be usable while in this state.
			/// </summary>
			/// <returns></returns>
			virtual internal bool CanDynamicPose()
			{
				return true;
			}

			virtual internal float GetTilt()
			{
				// Get our turning proportional to our speed
				float turn = m_parent.m_input_stick.m_turn;
				turn = Mathf.Clamp(turn, Mathf.Pi * -0.3f, Mathf.Pi * 0.3f);

				float speed = m_parent.GetAbsSpeedX();
				float tilt = turn * (0.5f + Mathf.Clamp(speed / 3.0f, 0.0f, 1.8f));
				return tilt;
			}

			virtual internal Transform3D GetShear()
			{
				// No shearing
				return Transform3D.Identity;
			}

			virtual internal void HitWall(Vector3 wall_normal)
			{
				if (m_parent.m_status.m_grounded)
				{
					// Project velocity onto wall
					m_parent.Velocity = Util.Vector3.NormalProject(m_parent.Velocity, wall_normal);
				}
				else
				{
					// Try to preserve our Y velocity
					Vector3 pre_velocity = m_parent.Velocity;
					m_parent.Velocity = Util.Vector3.NormalProject(m_parent.Velocity, wall_normal);

					// Check if the wall is too steep for us to stand on
					if (wall_normal.Dot(m_parent.GetUp()) < Mathf.Cos(m_parent.FloorMaxAngle))
					{
						// If the projection inverted our velocity, cancel preservation
						if (m_parent.Velocity.Dot(pre_velocity) < 0.0f)
							return;

						// Get our Y velocity before and after the projection
						float pre_y = Mathf.Abs(pre_velocity.Dot(m_parent.GetUp()));
						float post_y = Mathf.Abs(m_parent.Velocity.Dot(m_parent.GetUp()));
						if (post_y < Mathf.Epsilon || pre_y < Mathf.Epsilon)
							return;

						// If we're losing Y velocity, cancel preservation
						// As that would cause us to gain extreme XZ velocity
						if (pre_y > post_y)
							return;

						// Preserve our Y velocity
						m_parent.Velocity *= pre_y / post_y;
					}
				}
			}

			virtual public bool HitObject(Node3D node) { return true; }
		}

		public State m_state { get; internal set; }

		public void SetState(State state)
		{
			// Stop the current state
			if (m_state != null)
			{
				m_state.Stop();
				m_state = null;
			}

			// Start the new state
			m_state = state;
			m_state.Ready();
		}

		// Common states
		internal void SetStateLand(float y_speed = 0.0f)
		{
			// Check for land ability
			if (m_ability.CheckLandAbility())
				return;

			// Play landing sound
			float audio_db = Util.Audio.MultiplierToDb(Mathf.Clamp(0.2f + -y_speed * 0.17f, 0.0f, 1.0f));
			if (!m_status.m_hurt)
			{
				PlaySound("Land", audio_db);
			}
			else
			{
				PlaySound("DamageLand", audio_db);
			}

			// Check if we have enough speed to run
			if (GetAbsSpeedX() < m_param.m_jog_speed)
				SetState(new Idle(this, y_speed < -2.5f));
			else
				SetState(new Run(this));
		}
		internal void SetStateHurt()
		{
			if (!m_status.m_invincible)
			{
				if (m_rings < 1)
				{
					SetStateDead();

				}
				else
				{
					m_status.m_invincible = true;
					SetState(new Player.Hurt(this));
					m_releasing_rings = true;
					m_rings_to_release = m_rings;
					Vector3 speed = this.ToSpeed(this.Velocity);
					if (m_param.m_reset_speed_on_hit)
					{
						speed.X = -1;
					}
					else
					{
						speed.X = speed.X / 2;
						speed.Z = speed.Z / 2;
					}
					if (this.m_status.m_grounded)
					{
						speed.Y = 1.25f;
					}
					this.Velocity = this.FromSpeed(speed);
				}

			}
			return;
		}

		private void SetStateDead()
		{
			m_status.m_dead = true;
			m_status.m_invincible = true;
			SetState(new Player.Hurt(this));
		}

		// Coordinate systems
		internal Vector3 GetLook()
		{
			return -GlobalTransform.Basis.Z;
		}

		internal Vector3 GetUp()
		{
			return GlobalTransform.Basis.Y;
		}

		internal Vector3 GetRight()
		{
			return GlobalTransform.Basis.X;
		}

		internal Vector3 ToSpeed(Vector3 vector)
		{
			Vector3 speed = GlobalTransform.Basis.Inverse() * vector;
			return new Vector3(-speed.Z, speed.Y, speed.X) / Root.TickRate;
		}

		internal Vector3 FromSpeed(Vector3 speed)
		{
			Vector3 vector = GlobalTransform.Basis * new Vector3(speed.Z, speed.Y, -speed.X);
			return vector * Root.TickRate;
		}

		// Rotation functions
		internal void OriginBasis(Basis basis, float inertia = 1.0f)
		{
			Vector3 prev_speed = ToSpeed(Velocity);
			Basis = (basis * Basis).Orthonormalized();
			Velocity = Velocity * inertia + FromSpeed(prev_speed) * (1.0f - inertia);
		}

		internal void OriginRotate(Vector3 axis, float angle, float inertia = 1.0f)
		{
			OriginBasis(new Basis(axis, angle), inertia);
		}

		internal void CenterBasis(Basis basis, float inertia = 1.0f)
		{
			Vector3 prev_speed = ToSpeed(Velocity);
			GlobalTranslate(GetUp() * m_param.m_center_height);
			Basis = (basis * Basis).Orthonormalized();
			GlobalTranslate(GetUp() * -m_param.m_center_height);
			Velocity = Velocity * inertia + FromSpeed(prev_speed) * (1.0f - inertia);
		}

		internal void CenterRotate(Vector3 axis, float angle, float inertia = 1.0f)
		{
			CenterBasis(new Basis(axis, angle), inertia);
		}

		internal Transform3D GetCenterTransform()
		{
			return GlobalTransform.Translated(Vector3.Up * m_param.m_center_height);
		}

		// Common values
		internal float GetSpeedX()
		{
			return GetLook().Dot(Velocity) / Root.TickRate;
		}
		internal float GetAbsSpeedX()
		{
			return Mathf.Abs(GetSpeedX());
		}

		internal float GetSpeedY()
		{
			return GetUp().Dot(Velocity) / Root.TickRate;
		}
		internal float GetAbsSpeedY()
		{
			return Mathf.Abs(GetSpeedY());
		}

		internal float GetSpeedZ()
		{
			return GetRight().Dot(Velocity) / Root.TickRate;
		}
		internal float GetAbsSpeedZ()
		{
			return Mathf.Abs(GetSpeedZ());
		}

		internal float GetDotp()
		{
			return -GetUp().Dot(m_gravity);
		}

		// Sound functions
		internal Util.IAudioStreamPlayer GetSound(string name)
		{
			// Get sound node
			return Util.IAudioStreamPlayer.FromNode(GetNode("Sound/" + name));
		}

		internal void PlaySound(string name, float db = 0.0f, float speed = 1.0f)
		{
			// Get sound node
			Util.IAudioStreamPlayer sound_node = GetSound(name);

			// Set sound volume and speed
			sound_node.VolumeDb = db;
			sound_node.PitchScale = speed;

			// Play sound
			sound_node.Stop();
			sound_node.Play();
		}

		internal void StopSound(string name)
		{
			// Get sound node
			Util.IAudioStreamPlayer sound_node = GetSound(name);

			// Stop sound
			sound_node.Stop();
		}

		internal void UpdateSound(float db = 1.0f, float speed = 1.0f)
		{
			// Get sound node
			Util.IAudioStreamPlayer sound_node = GetSound("Footstep");

			// Set sound volume and speed
			sound_node.VolumeDb = db;
			sound_node.PitchScale = speed;
		}

		// Animation functions
		internal void ClearAnimation() => m_modelroot.ClearAnimation();
		internal void PlayAnimation(string name, double speed = 1.0f) => m_modelroot.PlayAnimation(name, speed);

		// Godot methods
		public override void _Ready()
		{
			// Get nodes
			m_main_colshape_node = GetNode<CollisionShape3D>("MainColShape");
			m_roll_colshape_node = GetNode<CollisionShape3D>("RollColShape");

			m_param_node = GetNode<PlayerParam>("PlayerParam");

			m_radial_trigger = GetNode<ObjectTriggerInterest>("RadialTrigger");
			m_attack_trigger = GetNode<ObjectTriggerInterest>("AttackTrigger");

			m_modelroot = GetNode<Character.ModelRoot>("ModelRoot");
			m_modelroot_offset = GlobalTransform.Inverse() * m_modelroot.GlobalTransform;

			// Initialize collision
			m_directspace = GetWorld3D().DirectSpaceState;

			m_ray_query = new PhysicsRayQueryParameters3D();
			m_ray_query.CollisionMask = CollisionMask;
			m_ray_query.Exclude.Add(GetRid());
			m_ray_query.HitBackFaces = false;
			m_rings_to_release = m_param.m_rings_to_release;
			// Initialize player state
			SetState(new Idle(this));

			// Setup base
			base._Ready();
		}

		public override void _PhysicsProcess(double delta)
        {
            // Reset if too low

            if (m_input_debug_respawn.m_released)
            {
                Respawn();
                /*Godot.PackedScene player_scene = (Godot.PackedScene) Godot.ResourceLoader.Load("res://Prefab/Character/Sonic/Player.tscn");
                BotPlayer player = (BotPlayer)player_scene.Instantiate();
                player.leader = (Player)this;
                GetParent().AddChild(player);*/
            }
            if (GetAbsSpeedX() > m_param.m_crash_speed)
            { m_camera_node.Fov = Mathf.Lerp(m_camera_node.Fov, 90, 0.1f); }
            else
            {
                m_camera_node.Fov = Mathf.Lerp(m_camera_node.Fov, 75, 0.1f);
            }

            // Update player parameters
            m_param = m_param_node.m_param;
            currentAnim = m_modelroot.m_current_anim;

            ProcessInput();

            // Process state
            m_state.AbilityProcess();
            m_state.Process();

            // Update model root
            m_modelroot.SetTransform(GlobalTransform * m_modelroot_offset);

            if (m_state.CanDynamicPose())
            {
                m_modelroot.SetTilt(m_state.GetTilt());
                m_modelroot.SetPointOfInterest(null);
            }
            else
            {
                m_modelroot.SetTilt(0.0f);
                m_modelroot.SetPointOfInterest(null);
            }

            m_modelroot.SetShear(m_state.GetShear());

            // Send RPC update
            Root.Rpc(this, nameof(HostRpc_Update), GlobalTransform, currentAnim);
            // Increment time
            m_time++;

            // Update debug context
#if DEBUG
            List<string> debugs = new List<string>();

            debugs.Add(string.Format("= Physics ="));

            Vector3 position = GlobalPosition;
            debugs.Add(string.Format("Position ({0:0.00}, {1:0.00}, {2:0.00})", position.X, position.Y, position.Z));

            Vector3 velocity = Velocity / Root.TickRate;
            debugs.Add(string.Format("Velocity ({0:0.00}, {1:0.00}, {2:0.00})", velocity.X, velocity.Y, velocity.Z));

            Vector3 speed = ToSpeed(Velocity);
            debugs.Add(string.Format("Speed ({0:0.00}, {1:0.00}, {2:0.00})", speed.X, speed.Y, speed.Z));

            Vector3 rotation = GlobalRotation * 180.0f / Mathf.Pi;
            debugs.Add(string.Format("Rotation ({0:0.00}, {1:0.00}, {2:0.00})", rotation.X, rotation.Y, rotation.Z));

            debugs.Add(string.Format("= State {0} =", m_state));
            m_state.Debug(debugs);

            // m_debug_context.SetItems(debugs);
#endif
            //Handle Damage
            if (m_status.m_hurt)
                HandleDamage();
            if (m_status.m_dead)
                handleDeath();
        }

        internal virtual void ProcessInput()
        {
            // Update input state

            Vector2 input_stick = Input.Server.GetMoveVector();
            bool input_jump = Input.Server.GetButton("move_jump");
            bool input_spin = Input.Server.GetButton("move_spin");
            bool input_secondary = Input.Server.GetButton("move_secondary");
            bool input_tertiary = Input.Server.GetButton("move_tertiary");
            bool input_quaternary = Input.Server.GetButton("move_quaternary");
            bool input_debug_respawn = Input.Server.GetButton("debug_respawn");

            m_input_stick.Update(input_stick, GlobalTransform, m_camera_node.GlobalTransform, -m_gravity);
            m_input_jump.Update(input_jump);
            m_input_spin.Update(input_spin);
            m_input_secondary.Update(input_secondary);
            m_input_tertiary.Update(input_tertiary);
            m_input_quaternary.Update(input_quaternary);
            m_input_debug_respawn.Update(input_debug_respawn);
            if (!m_input_stop.Check())
            {
                m_input_stick.m_x = 0.0f;
                m_input_stick.m_y = 0.0f;
                m_input_stick.m_turn = 0.0f;
                m_input_stick.m_length = 0.0f;
                m_input_stick.m_angle = 0.0f;
            }

            if (!m_input_speed.Check())
            {
                m_input_stick.m_x = 0.0f;
                m_input_stick.m_y = 1.0f;
                m_input_stick.m_turn = 0.0f;
                m_input_stick.m_length = 1.0f;
                m_input_stick.m_angle = 0.0f;
            }
        }

        private void Respawn()
		{
			GlobalTransform = GetParent<Node3D>().GlobalTransform;
			Velocity = Vector3.Zero;
		}

		public void HandleDamage()
		{
			m_hurt_counter++;
			if (m_hurt_counter < m_param.m_invincibility_timer)
			{
				m_status.m_invincible = true;
				SkinFlicker();
			}
			else
			{
				m_status.m_invincible = false;
				m_status.m_hurt = false;
				m_hurt_counter = 0;
				m_modelroot.Visible = true;

			}
			if (m_releasing_rings)
			{
				if (m_rings_to_release > m_param.m_rings_to_release) { m_rings_to_release = m_param.m_rings_to_release; }
				//RingLoss();
			}
		}
		void SkinFlicker()
		{
			m_flicker_counter += m_param.m_flicker_timer;
			if (m_flicker_counter > 0)
			{
				m_modelroot.Visible = true;
			}
			else
			{
				m_modelroot.Visible = false;
			}
			if (m_flicker_counter > 10)
			{
				m_flicker_counter = -10;
			}

		}
		void RingLoss()
		{
			m_rings = 0;
			if (m_rings_to_release > 0)
			{
				Vector3 pos = GlobalPosition;
				pos.Y++;
				var movingRing = ResourceLoader.Load<PackedScene>("res://Prefab/Objects/Ring/ringdropped.res").Instantiate() as DroppedRing;
				GetParent().GetParent().AddChild(this);
				movingRing.ApplyForce(Transform.Basis.Z, pos);
				releaseDirection.Rotate(GlobalPosition, 30f);
				m_rings_to_release--;
			}
			else
			{
				m_releasing_rings = false;
			}

		}
		public void handleDeath()
		{
			m_camera_node.mode = Camera.CameraMode.Frozen;

			m_input_stop.Set((ulong)Mathf.Abs(m_param.m_dead_timer));
			m_dead_counter++;
			if (m_dead_counter > m_param.m_dead_timer)
			{
				m_input_stop.Set((ulong)Mathf.Abs(0));
				m_dead_counter = 0;
				Respawn();
				m_camera_node.mode = Camera.CameraMode.Normal;
				m_rings = 0;
				m_status.m_invincible = false;
				m_status.m_dead = false;
			}
		}
		// RPC methods
		private void HostRpc_Update(Transform3D transform, string anim)
		{
			// Forward the RPC to all clients
			Root.GetHostServer().RpcOthers(this, "Rpc_Update", transform, anim);
		}
	}
}
