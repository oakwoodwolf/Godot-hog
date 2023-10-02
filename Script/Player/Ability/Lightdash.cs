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
using System.Diagnostics;

namespace SonicOnset
{
	public partial class Player
	{
		public partial class Ability
		{
			public partial class Lightdash : Ability
			{
				// Homing attack ability
				public Lightdash(Player player)
				{
					// Set parent player
					m_parent = player;
				}
                private bool GetRingTarget()
                {
                    // Query in radial area
                    Godot.Collections.Array<Node3D> radial_nodes = m_parent.m_radial_trigger.QueryIntersections();

                    // Check for nodes
                    float target_distance = float.PositiveInfinity;

                    foreach (Node3D node in radial_nodes)
                    {
                        // Get homing attackable node
                        IObject node_object = node as IObject;
                        if (node_object == null || !node_object.CanLightDash())
                            continue;

                        // Get offset from our global transform
                        Vector3 offset = node.GlobalTransform.Origin - m_parent.GlobalTransform.Origin;

                        // Check if too far above
                        float y_offset = offset.Dot(m_parent.GetUp());
                        if (y_offset > 20.0f)
                            continue;

                        // Get distance and dot value
                        float distance = offset.Length() / 3;
                        float dot = offset.Normalized().Dot(m_parent.GetLook());
                        if (dot < 0.3825f)
                            continue;

                        float dot_value = distance / dot;
                        if (dot_value < target_distance)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                internal override bool CheckJumpAbility()
				{
					// Check if we're already dashing
					if (m_parent.m_state is Player.Lightdash)
						return false;

					// Check jump button
					if (m_parent.m_input_tertiary.m_pressed)
					{
                        bool i = GetRingTarget();
						// Switch to ringdash state
                        if (i == true)
                        {
                            m_parent.SetState(new Player.Lightdash(m_parent));
                            return true;
                        }

                    }
					return false;
				}
                internal override bool CheckSpinAbility()
                {
                    // Check if we're already dashing
                    if (m_parent.m_state is Player.Lightdash)
                        return false;

                    // Check jump button
                    if (m_parent.m_input_tertiary.m_pressed)
                    {
                        bool i = GetRingTarget();
                        // Switch to ringdash state
                        if (i == true)
                        {
                            m_parent.SetState(new Player.Lightdash(m_parent));
                            return true;
                        }
                    }
                    return false;
                }
            }
		}
	}
}
