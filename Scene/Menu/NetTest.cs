// SonicOnset.Scene.NetTest.NetTest
using Godot;
using System.Reflection;

namespace SonicGodot.Scene
{
	partial class NetTest : Control
	{
		private TextEdit _ipEdit;

		private TextEdit _portEdit;

		private TextEdit _maxEdit;
		private TextEdit _stageEdit;
		private Control _loadDrop;
		private Label _loadLabel;

		private Button _playButton;

		private Button _hostButton;

		private Button _joinButton;
		private CheckBox _upnpCheckbox;
		private bool _upnp;

		public override void _Ready()
		{
			_ipEdit = GetNode<TextEdit>("Bar/IpEdit");
			_portEdit = GetNode<TextEdit>("Bar/PortEdit");
			_maxEdit = GetNode<TextEdit>("Bar/MaxEdit");
			_stageEdit = GetNode<TextEdit>("Bar/StageEdit");
			_loadDrop = GetNode<Control>("Loading");
			_upnpCheckbox = GetNode<CheckBox>("Bar/CheckBox");


			_playButton = GetNode<Button>("Bar/StartButton");
			_hostButton = GetNode<Button>("Bar/HostButton");
			_joinButton = GetNode<Button>("Bar/JoinButton");
			_playButton.Connect("pressed", new Callable(this, "OnPlayButtonPressed"));
			_hostButton.Connect("pressed", new Callable(this, "OnHostButtonPressed"));
			_joinButton.Connect("pressed", new Callable(this, "OnJoinButtonPressed"));
			base._Ready();
		}

		private void OnPlayButtonPressed()
		{
			Root.StartLocalServer();
			LoadStagePack(_stageEdit.Text);

		}



		private void OnHostButtonPressed()
		{
			_loadDrop.Visible = true;
			int port = int.Parse(_portEdit.Text);
			int max_players = int.Parse(_maxEdit.Text);
			Root.StartHostServer(port, max_players, _upnp);
			LoadStagePack(_stageEdit.Text);
		}

		private void OnJoinButtonPressed()
		{
			_loadDrop.Visible = true;
			int port = int.Parse(_portEdit.Text);
            Root.JoinServer(_ipEdit.Text, port);
        }
        /// <summary>
        /// Loads a Stage.pck file. Its name must match the name of both the pck and the folder packed inside.
        /// </summary>
        /// <param name="name">The name of the stage's pck file, excluding the file extension.</param>
		private static void LoadStagePack(string name)
		{
            var data = (StageData) ResourceLoader.Load("res://Stages/" + name + "/Stage.tres");
            Root.GetHostServer().RpcAll(Root.Singleton(), nameof(Root.Rpc_SetScene), "res://Stages/" + name + "/Stage.tscn", data);
		}
		private void OnCheckboxToggled(bool button_pressed)
		{
			GD.Print("toggled " + button_pressed);
			_upnp = button_pressed;
		}

	}
}



