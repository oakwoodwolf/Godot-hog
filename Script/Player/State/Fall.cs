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
    public partial class Player
    {
        public partial class Fall : State
        {
            int _fallTimer;
            // Fall state
            internal Fall(Player parent)
            {
                // Set parent
                _fallTimer = 15;
                m_parent = parent;
            }

            internal override void AbilityProcess()
            {
                _fallTimer--;
                if (_fallTimer > 0 )
                {
                    if (m_parent.m_ability.CheckJump())
                    {
                        GD.Print(_fallTimer);
                        return;
                    }
                       
                }
                
                // Check fall abilities
                if (m_parent.m_ability.CheckFallAbility())
                    return;
            }

            internal override void Process()
            {
                // Movement
                m_parent.RotateToGravity();
                m_parent.AirMovement();

                // Physics
                float y_speed = m_parent.GetSpeedY();
                float x_speed = m_parent.GetSpeedX();
                m_parent.PhysicsMove();
                m_parent.CheckGrip();

                if (m_parent.m_status.m_grounded)
                    m_parent.SetStateLand(y_speed);

                // Set animation
                if (x_speed < 4)
                {
                    if (y_speed <= 1)
                    {
                        m_parent.PlayAnimation("Fall");
                    }
                    else
                    {
                        m_parent.PlayAnimation("Up");
                    }
                }
                else
                {
                    m_parent.PlayAnimation("FallFast");
                }


            }
        }
    }
}
