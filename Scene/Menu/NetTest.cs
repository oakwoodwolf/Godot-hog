// SonicOnset.Scene.NetTest.NetTest
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using SonicOnset;

namespace SonicOnset.Scene
{
	partial class NetTest : Control
	{
		private TextEdit m_ip_edit;

		private TextEdit m_port_edit;

		private TextEdit m_max_edit;
		private TextEdit m_stage_edit;
		private Control m_load_drop;
		private Label m_load_label;

		private Button m_play_button;

		private Button m_host_button;

		private Button m_join_button;

		public override void _Ready()
		{
			m_ip_edit = GetNode<TextEdit>("Bar/IpEdit");
			m_port_edit = GetNode<TextEdit>("Bar/PortEdit");
			m_max_edit = GetNode<TextEdit>("Bar/MaxEdit");
			m_stage_edit = GetNode<TextEdit>("Bar/StageEdit");
			m_load_drop = GetNode<Control>("Loading");

			m_play_button = GetNode<Button>("Bar/PlayButton");
			m_host_button = GetNode<Button>("Bar/HostButton");
			m_join_button = GetNode<Button>("Bar/JoinButton");
			m_play_button.Connect("pressed", new Callable(this, "OnPlayButtonPressed"));
			m_host_button.Connect("pressed", new Callable(this, "OnHostButtonPressed"));
			m_join_button.Connect("pressed", new Callable(this, "OnJoinButtonPressed"));
			base._Ready();
		}

		private void OnPlayButtonPressed()
		{
			Root.StartLocalServer();
			LoadStagePack(m_stage_edit.Text);

		}



		private void OnHostButtonPressed()
		{
			m_load_drop.Visible = true;
			int port = int.Parse(m_port_edit.Text);
			int max_players = int.Parse(m_max_edit.Text);
			Root.StartHostServer(port, max_players);
			var success = ProjectSettings.LoadResourcePack("res://Stage.pck");
			LoadStagePack(m_stage_edit.Text);
		}

		private void OnJoinButtonPressed()
		{
			m_load_drop.Visible = true;
			int port = int.Parse(m_port_edit.Text);
			var success = ProjectSettings.LoadResourcePack("res://" + m_stage_edit.Text + ".pck");
			if (success)
			{
				Root.JoinServer(m_ip_edit.Text, port);
			}
			


		}

		private static void LoadStagePack(string name)
		{
			var success = ProjectSettings.LoadResourcePack("res://" + name + ".pck");
			if (success)
			{
				if (FileAccess.FileExists("res://"+ name +"/") )
					{ Assembly.LoadFile(name + ".dll"); }

				Root.GetHostServer().RpcAll(Root.Singleton(), "Rpc_SetScene", "res://Stages/" + name + "/Stage.tscn");
			}
		}


	}
}
