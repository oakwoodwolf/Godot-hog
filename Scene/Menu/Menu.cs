using Godot;
using Godot.Collections;
using System;

namespace SonicGodot
{
	public partial class Menu : Control
	{
		public AudioStreamPlayer SoundPlayer;
		[Export]
		Dictionary<string, Vector2I> _resolutionOptions = new Dictionary<string, Vector2I> {
			{"3840x2160", new Vector2I(3840, 2160)},
			{"2560x1440", new Vector2I(2560,1080)},
			{"1920x1080", new Vector2I(1920,1080)},
			{"1366x768", new Vector2I(1366,768)},
			{"1536x864", new Vector2I(1536,864)},
			{"1280x720", new Vector2I(1280,720)},
			{"1440x900", new Vector2I(1440,900)},
			{"1600x900", new Vector2I(1600,900)},
			{"1024x600", new Vector2I(1024,600)},
			{"800x600", new Vector2I(800,600)}};
		[Export]
		Dictionary<string, AudioStream> _menuSounds = new Dictionary<string, AudioStream>();
		OptionButton _resolutionButton;
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			int index = 0;

			SoundPlayer = GetNode<AudioStreamPlayer>("Sounds");
			GetNode<Label>("%VersionTagTitle").Text = ProjectSettings.GetSetting("application/config/name").AsString();
			GetNode<Button>("%PlayButton").GrabFocus();
			_resolutionButton = GetNode<OptionButton>("%ResolutionButton");
			foreach (var (resolution, vec2) in _resolutionOptions)
			{
				_resolutionButton.AddItem(resolution, index);
				index++;
			}
			SetResolutionText();
			OptionButton windowModeButton = GetNode<OptionButton>("%WindowModeButton");
			windowModeButton.Select(CheckWindowMode((UInt16)DisplayServer.WindowGetMode()));
			GetNode<Slider>("%MasterSlider").Value = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(0));
			GetNode<Slider>("%MusicSlider").Value = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(1));
			GetNode<Slider>("%SoundSlider").Value = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(2));
			GetNode<Slider>("%VoiceSlider").Value = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(3));

		}

		private void SetResolutionText()
		{
			_resolutionButton.Text = DisplayServer.WindowGetSize().X + "x" + DisplayServer.WindowGetSize().Y;
		}

		public void PlaySound(string name)
		{
			SoundPlayer.Stream = _menuSounds[name];
			SoundPlayer.Play();
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
		}
		// Main Menu
		private void OnPlayPressed()
		{
			PlaySound("accept");
			GetNode<Control>("PlayMenu").Visible = true;
			GetNode<Button>("PlayMenu/Bar/StartButton").GrabFocus();
			GetNode<Control>("TitleScreen").Visible = false;
		}
		private void OnOptionsPressed()
		{
			PlaySound("accept");
			GetNode<Control>("OptionsMenu").Visible = true;
			_resolutionButton.GrabFocus();
			GetNode<Control>("TitleScreen").Visible = false;
		}
		private void OnQuitPressed()
		{
			PlaySound("accept");
			GetTree().Quit();
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
		private void OnResolutionButtonItemSelected(Int16 index)
		{	
			string res = _resolutionButton.GetItemText(index);
			DisplayServer.WindowSetSize(_resolutionOptions[res]);
			Root.CenterWindow();
			PlaySound("choose");
		}
		private void OnWindowModeButtonItemSelected(Int16 index)
		{
			PlaySound("choose");
			DisplayServer.WindowSetMode((DisplayServer.WindowMode)Root.windowModes[index]);
			SetResolutionText();
		}
		private void OnMasterSliderValueChanged(double value)
		{
			AudioServer.SetBusVolumeDb(0, (float)Mathf.LinearToDb(value)/2);
		}
		private void OnMusicSliderValueChanged(double value)
		{
			AudioServer.SetBusVolumeDb(1, (float)Mathf.LinearToDb(value));
		}
		private void OnSoundSliderValueChanged(double value)
		{
			AudioServer.SetBusVolumeDb(2, (float)Mathf.LinearToDb(value));
		}
		private void OnVoiceSliderValueChanged(double value)
		{
			AudioServer.SetBusVolumeDb(3, (float)Mathf.LinearToDb(value));
		}
		private void OnSliderDragEnded(bool value_changed)
		{
			PlaySound("accept");
		}
		private void OnVoiceSliderDragEnded(bool value_changed)
		{
			GetNode<AudioStreamPlayer>("Voice").Play();
		}
		private void OnApplyPressed()
		{
			PlaySound("accept");
			GetNode<Control>("OptionsMenu").Visible = false;
			GetNode<Control>("TitleScreen").Visible = true;
			GetNode<Button>("%PlayButton").GrabFocus();
			Root.SaveSetting();
		}
		private void OnReturnPressed()
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
