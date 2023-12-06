using Godot;
using System;
using System.Collections.Generic;

namespace SonicGodot
{
	public partial class BotPlayer : Player
	{
        public Node3D Leader { get; set; }
        public partial class AbilityList : Ability.AbilityList
        {
            // Ability list
            internal AbilityList(Player player)
            {
                // Set parent player
                _parent = player;

                // Create abilities
                m_abilities.Add(new Jump(player));
                m_abilities.Add(new Spindash(player));
            }
        }

        // Player node
        public override void _Ready()
        {
            // Set ability
            m_ability = new AbilityList(this);
            // Ready base
            base._Ready();
        }
        internal override void ProcessInput()
        {
            // Update input state

            Vector2 input_stick = Vector2.One;
            bool input_jump = Input.Server.GetButton("move_jump");
            bool input_spin = Input.Server.GetButton("move_spin");


            m_input_stick.Update(input_stick, GlobalTransform, Leader.GlobalTransform, -m_gravity);
            GD.Print(input_stick);
            m_input_jump.Update(input_jump);
            m_input_spin.Update(input_spin);

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
    }
}

