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

namespace SonicGodot.UI.LoadUI
{
	public partial class LoadUI : Node
	{
		// Load UI nodes
		private Label _load_label;
		[Export]
		public StageData Data;
		[Export]
		private Label _loadingText;
		[Export]
		private TextureRect _loadingBg;
		[Export]
		public AnimationPlayer Player;

		// Load UI node
		public override void _Ready()
		{
			// Get nodes
			_load_label = GetNode<Label>("UI/LoadUI/LoadRoot/LoadLabel");

			// Setup base
			base._Ready();
		}
		public void SetUp(StageData data)
		{
			if (data != null)
			{
				Data = data;
				_loadingText.Text = Data.StageName;
				GD.Print(Data.StageBackground);
				_loadingBg.Texture = Data.StageBackground;
				GD.Print(_loadingBg.Texture);
				Player.Play("LoadStart");
			}
		}
		private void OnLoadStartFinished(StringName anim_name)
		{
			if (anim_name == "LoadStart") {
				Player.Play("Load");
			} if (anim_name == "LoadEnd")
			{
				Root.FreeLoader();
				QueueFree();
			}
			
		}
		// Update progress
		public void SetProgress(float progress)
		{
			// Update progress
			_load_label.Text = string.Format("Loading... {0:0}%", progress * 100.0f);
		}


	}
}



