using Godot;
using System;
using System.Collections.Generic;

namespace SonicGodot
{
	public partial class BotPlayer : Player
	{
		[Export]
		public Player leader { get; set; }
		public override void _Ready()
		{
			GD.Print(leader.Name);
		}
		public override void _PhysicsProcess(double delta)
		{
		  

			// Update player parameters
			m_param = leader.m_param_node.m_param;

			// Update input state
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


			
		}

		internal override void ProcessInput()
		{
			m_input_stick.Update(new Vector2(1, 0.1f), GlobalTransform, GlobalTransform, -m_gravity);
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
		}
	}
}

