using Godot;
using Godot.Collections;
using System;

namespace SonicGodot
{
	public partial class Menu : Control
	{
		[ExportGroup("Options")]
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
		OptionButton _resolutionButton;
		[Export]
		Dictionary<string, float> _fsrScaling = new Dictionary<string, float> {
			{"Performance: 50%", 0.5f},
			{"Balanced: 59%", 0.59f},
			{"Quality: 67%", 0.67f},
			{"Ultra: 77%", 0.77f},
			{"Off: 100%", 1f},
		};
		[Export]
		OptionButton _fsrButton;
		public AudioStreamPlayer SoundPlayer;
		[Export]
		Dictionary<string, AudioStream> _menuSounds = new Dictionary<string, AudioStream>();
        public enum MenuPage
        {
            Title,
            Options,
            Mode,
            Stage,
            Online,
            Host,
            Join,
            Credits,

        }

        [Export]
        public Dictionary<MenuPage, NodePath> MenuValuePairs = new Dictionary<MenuPage, NodePath>();
   
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			int index = 0;

			SoundPlayer = GetNode<AudioStreamPlayer>("Sounds");
			GetNode<Label>("%VersionTagTitle").Text = ProjectSettings.GetSetting("application/config/name").AsString();
			GetNode<Button>("%PlayButton").GrabFocus();
			foreach (var (resolution, vec2) in _resolutionOptions)
			{
				_resolutionButton.AddItem(resolution, index);
				index++;
			}
			foreach (var fsrPreset in _fsrScaling)
			{
				_fsrButton.AddItem(fsrPreset.Key);
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
            SwitchMenu(MenuPage.Mode);
			GetNode<Button>("ModeMenu/VBoxContainer/SoloButton").GrabFocus();
		}
        public void SwitchMenu(MenuPage page)
        {
            PlaySound("accept");
            foreach (var menu in MenuValuePairs)
            {
                GD.Print(menu);
                if (menu.Key == page)
                { 
                    GetNode<Control>(menu.Value).Show();
                }
                else
                {
                    GetNode<Control>(menu.Value).Hide(); 
                }
                

            }
        }
		private void OnOptionsPressed()
		{
            SwitchMenu(MenuPage.Options);
			_resolutionButton.GrabFocus();
		}
		private void OnQuitPressed()
		{
			PlaySound("accept");
			GetTree().Quit();
		}
		//Play Menu
		private void OnOnlinePressed()
		{
            SwitchMenu(MenuPage.Online);
			GetNode<Button>("PlayMenu/Bar/StartButton").GrabFocus();
		}
		//Options
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
		
		private void OnFSRButtonItemSelected(int index)
		{

			ProjectSettings.SetSetting("rendering/scaling_3d/scale", _fsrScaling[_fsrButton.GetItemText(index)]);
			GD.Print(ProjectSettings.GetSetting("rendering/scaling_3d/scale"));
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
            SwitchMenu(MenuPage.Title);
            GetNode<Button>("%PlayButton").GrabFocus();
			Root.SaveSetting();
		}
		private void OnReturnPressed()
		{
            SwitchMenu(MenuPage.Title);
            GetNode<Control>("ModeMenu").Visible = false;
			GetNode<Button>("%PlayButton").GrabFocus();
		}
		private void OnFocusExit()
		{
			PlaySound("choose");
		}
	}
}
