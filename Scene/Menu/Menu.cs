using Godot;
using SonicOnset;
using System;

namespace SonicOnset
{
	public partial class Menu : Control
	{
		OptionButton resolutionButton;
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			GetNode<Label>("%VersionTagTitle").Text = ProjectSettings.GetSetting("application/config/name").AsString();
			GetNode<Button>("%PlayButton").GrabFocus();
			resolutionButton = GetNode<OptionButton>("%ResolutionButton");
			OptionButton windowModeButton = GetNode<OptionButton>("%WindowModeButton");
			windowModeButton.Select(CheckWindowMode((UInt16)DisplayServer.WindowGetMode()));
			GD.Print(DisplayServer.WindowGetMode());
			resolutionButton.Select(CheckResolution(DisplayServer.WindowGetSize()));
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
		}
		// Main Menu
		private void _on_play_pressed()
		{
			GetNode<Control>("PlayMenu").Visible = true;
            GetNode<Button>("PlayMenu/Bar/StartButton").GrabFocus();
            GetNode<Control>("TitleScreen").Visible = false;
		}
		private void _on_options_pressed()
		{
			GetNode<Control>("OptionsMenu").Visible = true;
			resolutionButton.GrabFocus();
			GetNode<Control>("TitleScreen").Visible = false;
		}
		private void _on_quit_pressed()
		{
			GetTree().Quit();
		}
		//Resolution management
		private Vector2I GetResolution(int index)
		{
			var resolutionArray = resolutionButton.GetItemText(index).Split("x");
			return new Vector2I(Int16.Parse(resolutionArray[0]), Int16.Parse(resolutionArray[1]));
		}
		private int CheckResolution(Vector2I res)
		{
			for (int i = 0; i < resolutionButton.ItemCount; i++)
			{
				if (GetResolution(i) == res) return i;

			}
			return -1;
		}
		private int CheckWindowMode(UInt16 res)
		{
			int i = 0;
			switch (res) {
				case (UInt16)DisplayServer.WindowMode.Fullscreen:
					i = 1;
                    break;
                case (UInt16)DisplayServer.WindowMode.ExclusiveFullscreen:
                    i = 2;
                    break;
                default:

                    break;

            }
            return i;
        }
		private void _on_resolution_button_item_selected(Int16 index)
		{

			DisplayServer.WindowSetSize(GetResolution(index));
		}
		private void _on_window_mode_button_item_selected(Int16 index)
		{
			DisplayServer.WindowSetMode((DisplayServer.WindowMode)Root.windowModes[index]);
		}
		private void _on_apply_pressed()
		{
			GetNode<Control>("OptionsMenu").Visible = false;
			GetNode<Control>("TitleScreen").Visible = true;
            GetNode<Button>("%PlayButton").GrabFocus();
            Root.SaveSetting();
		}
		private void _on_return_pressed()
		{
			GetNode<Control>("OptionsMenu").Visible = false;
			GetNode<Control>("TitleScreen").Visible = true;
            GetNode<Button>("%PlayButton").GrabFocus();
        }

	}
}
