using Godot;
using Godot.Collections;
using System;

namespace SonicGodot
{
	public partial class Menu : Control
	{
		public AudioStreamPlayer soundPlayer;
		[Export]
		Dictionary<string, AudioStream> menuSounds = new Dictionary<string, AudioStream>();
		OptionButton resolutionButton;
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			soundPlayer = GetNode<AudioStreamPlayer>("Sounds");
			GetNode<Label>("%VersionTagTitle").Text = ProjectSettings.GetSetting("application/config/name").AsString();
			GetNode<Button>("%PlayButton").GrabFocus();
			resolutionButton = GetNode<OptionButton>("%ResolutionButton");
			OptionButton windowModeButton = GetNode<OptionButton>("%WindowModeButton");
			windowModeButton.Select(CheckWindowMode((UInt16)DisplayServer.WindowGetMode()));
			GD.Print(DisplayServer.WindowGetMode());
			resolutionButton.Select(CheckResolution(DisplayServer.WindowGetSize()));
			GetNode<Slider>("%MasterSlider").Value = Root.DecibelToLinear(AudioServer.GetBusVolumeDb(0));
			GetNode<Slider>("%MusicSlider").Value = Root.DecibelToLinear(AudioServer.GetBusVolumeDb(1));
			GetNode<Slider>("%SoundSlider").Value = Root.DecibelToLinear(AudioServer.GetBusVolumeDb(2));
			GetNode<Slider>("%VoiceSlider").Value = Root.DecibelToLinear(AudioServer.GetBusVolumeDb(3));

		}

		public void PlaySound(string name)
		{
			soundPlayer.Stream = menuSounds[name];
			soundPlayer.Play();
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
		}
		// Main Menu
		private void _on_play_pressed()
		{
			PlaySound("accept");
			GetNode<Control>("PlayMenu").Visible = true;
			GetNode<Button>("PlayMenu/Bar/StartButton").GrabFocus();
			GetNode<Control>("TitleScreen").Visible = false;
		}
		private void _on_options_pressed()
		{
			PlaySound("accept");
			GetNode<Control>("OptionsMenu").Visible = true;
			resolutionButton.GrabFocus();
			GetNode<Control>("TitleScreen").Visible = false;
		}
		private void _on_quit_pressed()
		{
			PlaySound("accept");
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
			switch (res)
			{
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
			PlaySound("choose");
			DisplayServer.WindowSetSize(GetResolution(index));
		}
		private void _on_window_mode_button_item_selected(Int16 index)
		{
			PlaySound("choose");
			DisplayServer.WindowSetMode((DisplayServer.WindowMode)Root.windowModes[index]);
		}
		private void _on_master_slider_value_changed(double value)
		{
			AudioServer.SetBusVolumeDb(0, Root.LinearToDecibel(value));
		}
		private void _on_music_slider_value_changed(double value)
		{
			AudioServer.SetBusVolumeDb(1, Root.LinearToDecibel(value));
		}
		private void _on_sound_slider_value_changed(double value)
		{
			AudioServer.SetBusVolumeDb(2, Root.LinearToDecibel(value));
		}
		private void _on_voice_slider_value_changed(double value)
		{
			AudioServer.SetBusVolumeDb(3, Root.LinearToDecibel(value));
		}
		private void _on_slider_drag_ended(bool value_changed)
		{
			PlaySound("accept");
		}
		private void _on_voice_slider_drag_ended(bool value_changed)
		{
			GetNode<AudioStreamPlayer>("Voice").Play();
		}
		private void _on_apply_pressed()
		{
			PlaySound("accept");
			GetNode<Control>("OptionsMenu").Visible = false;
			GetNode<Control>("TitleScreen").Visible = true;
			GetNode<Button>("%PlayButton").GrabFocus();
			Root.SaveSetting();
		}
		private void _on_return_pressed()
		{
			PlaySound("accept");
			GetNode<Control>("OptionsMenu").Visible = false;
			GetNode<Control>("TitleScreen").Visible = true;
			GetNode<Button>("%PlayButton").GrabFocus();
		}
		private void OnFocusExit()
		{
			PlaySound("choose");
		}
	}
}
