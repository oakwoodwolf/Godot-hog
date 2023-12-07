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

namespace SonicGodot.Character.Sonic
{
	public partial class Player : SonicGodot.Player
	{
		// Player abilities
		public partial class AbilityList : Ability.AbilityList
		{
			// Ability list
			internal AbilityList(Player player)
			{
				// Set parent player
				_parent = player;

				// Create abilities
				_abilities.Add(new Jump(player));
				_abilities.Add(new Lightdash(player));
				_abilities.Add(new Spindash(player));
				_abilities.Add(new HomingAttack(player));
				_abilities.Add(new Bounce(player));
				_abilities.Add(new AirKick(player));
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
	}
}
