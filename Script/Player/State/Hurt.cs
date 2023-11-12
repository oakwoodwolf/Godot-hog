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
	public partial class Player
	{
		public partial class Hurt : State
		{
			// Hurt state
			int m_nocon = 60;
			bool animPlayed = false;
			// Fall state
			internal Hurt(Player parent)
			{
				// Set parent
				m_parent = parent;
                // Set animation
                m_parent.PlaySound("Damage");

                if (!m_parent.m_status.m_dead)
				{
					m_parent.m_input_stop.Set((ulong)Mathf.Abs(m_nocon));
                    m_parent.ClearAnimation();
                    m_parent.PlayAnimation("Hurt");
                    m_parent.PlaySound("VoiceHurt");
                    if (m_parent.m_rings < 50)
                    {
                        m_parent.m_rings = 0;
                    }
                    else
                    {
                        m_parent.m_rings -= 50;
                    }
                } else
				{
                    m_parent.PlaySound("VoiceDead");

                }


;
			}
			
			internal override void Process()
			{
		
					// Fall to gravity
					m_parent.Velocity += m_parent.m_gravity * m_parent.m_param.m_gravity * Root.c_tick_rate;
				// Physics
				float y_speed = m_parent.GetSpeedY();
				m_parent.PhysicsMove();
				m_parent.CheckGrip();

				if (m_parent.m_status.m_grounded)
				{
					if (!m_parent.m_status.m_dead) { m_parent.m_status.m_hurt = true; m_parent.SetStateLand(y_speed);
                    }

                    m_parent.m_modelroot.Visible = true;
                }
                

				// Check if nocon expired
				if (m_nocon < 0)
					m_nocon++;
				else
					m_nocon--;
				if (m_parent.m_status.m_dead)
				{

					if (m_parent.m_status.m_grounded)
					{
                        m_parent.BrakeMovement();
						if (!animPlayed)
						{
							animPlayed = true;
                            m_parent.PlayAnimation("Dead");				
                        }
                    }
                }
			}
			// State overrides
			internal override bool CanDynamicPose()
			{
				return false;
			}

        }
	}
}
