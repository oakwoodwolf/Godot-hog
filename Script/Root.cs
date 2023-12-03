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
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SonicGodot
{
	public partial class Root : Node
	{
		// Game constants
		public const uint TickRate = 60; // Tick rate for physics calculations

		// Global clock
		private ulong _clock = 0;
		// Scene loader
		[Export]
		private PackedScene _loadScene;
		private static UI.LoadUI.LoadUI LoadSceneNode = null;

		private string _loadingScene = null;
		private string _nextScene = null;
		public StageData StageData = null;

		private Node _scene = null;

		private Node _players = null;

		// Net server
		private MultiplayerApi _multiplayerApi = null;

		public static List<string> stageList = new();

		private Net.IServer _server = null;
		private Net.NetSync _netsync = null;
		public static ConfigFile settingsFile = new ConfigFile();
		public static Godot.Collections.Dictionary<int, int> windowModes = new Godot.Collections.Dictionary<int, int> { { 0, (int)DisplayServer.WindowMode.Windowed }, { 1, (int)DisplayServer.WindowMode.Fullscreen }, { 2, (int)DisplayServer.WindowMode.ExclusiveFullscreen } };
		// Root singleton
		public override void _EnterTree()
		{
			// Create multiplayer API
			_multiplayerApi = Godot.MultiplayerApi.CreateDefaultInterface();
			GetTree().SetMultiplayer(_multiplayerApi);

			// Connect to players joining
			_multiplayerApi.Connect("peer_connected", new Callable(this, "Rpc_PeerConnected"));
			_multiplayerApi.Connect("peer_disconnected", new Callable(this, "Rpc_PeerDisconnected"));

			// Register singleton
			ProcessPriority = (int)Enum.Priority.Root;
			Engine.RegisterSingleton("Root", this);
		}

		public override void _ExitTree() => DisconnectServer();

        internal static Root Singleton() => (Root) Engine.GetSingleton("Root");

        // Load scene
        private void LoadScene(string scene_path, StageData data = null)
		{
			// Begin loading new scene
			StageData = data;
			_nextScene = scene_path;

		}

		private void SpawnPlayer()
		{
			// Instantiate player
			var player_scene = (PackedScene)ResourceLoader.Load("res://Prefab/Character/Sonic/Player.tscn");
			Node player = player_scene.Instantiate();
			player.Name = _server.GetPeerId().ToString();

			// Add player to scene
			_players.AddChild(player);
		}

		private void SpawnPeer(int peer_id, string name = "Player")
		{
			// Instantiate player
			var player_scene = (PackedScene)ResourceLoader.Load("res://Prefab/Character/Sonic/NetPlayer.tscn");
			var player = (NetPlayer)player_scene.Instantiate();
			player.Name = peer_id.ToString();
			Label3D label = player.GetNode<Label3D>("ModelRoot/Nametag");
			label.Text = peer_id.ToString();
			// Add player to scene
			_players.AddChild(player);
		}

		private void SpawnPeers()
		{
			// Go through all peers
			int client_id = _server.GetPeerId();
			foreach (int peer_id in _server.GetPeerIds())
            {
                if (peer_id != client_id)
                {
                    SpawnPeer(peer_id);
                }
            }
        }

		private void DespawnPeer(int peer_id)
		{
			if (_players != null)
			{
				// Remove player
				Node player = _players.GetNodeOrNull(peer_id.ToString());
				if (player != null)
					player.QueueFree();
			}
		}

		private void SetupScene()
		{
			// Check if scene contains a players node
			_players = _scene.GetNodeOrNull("Players");

			// Spawn players
			if (_players != null)
			{
				SpawnPlayer();
				SpawnPeers();
			}
		}

		// Root node
		public override void _Ready()
		{
			LoadSettings();
			// Load scene
			LoadAllStagePcks();

			LoadScene("res://Scene/Menu/NetTest.tscn");
		}

		public override void _PhysicsProcess(double delta)
		{
			// Increment clock
			_clock++;
		}
		public static void LoadAllStagePcks()
		{
			
			string workingDirectory;
			if (OS.HasFeature("editor"))
			{
				workingDirectory = ProjectSettings.GlobalizePath("res://");
			} else
			{
				workingDirectory = OS.GetExecutablePath().GetBaseDir();
			}
			
			string[] resourcePacks = Directory.GetFiles(workingDirectory, "*.pck");
			resourcePacks = resourcePacks.Where(pack => !Path.GetFileName(pack).Contains("SonicGodot.pck", StringComparison.OrdinalIgnoreCase)).ToArray();
			for (int i = 0; i < resourcePacks.Length; i++)
			{
				resourcePacks[i] = Path.GetFileNameWithoutExtension(resourcePacks[i]);
				var success = ProjectSettings.LoadResourcePack("res://" + resourcePacks[i] + ".pck");
				if (success)
				{
					if (Godot.FileAccess.FileExists("res://" + resourcePacks[i] + "/"))
				   { Assembly.LoadFile(resourcePacks[i] + ".dll"); }
					stageList.Add(resourcePacks[i]);
				}
			}
			
		}
		public override void _Process(double delta)
		{
			while (_nextScene != null || _loadingScene != null)
			{
				// Check if we should start loading a scene
				if (_nextScene != null && _loadingScene == null)
				{
					// Unload scene
					if (_scene != null)
					{
						_scene.QueueFree();
						_scene = null;
					}

					// Load UI
					LoadSceneNode = (UI.LoadUI.LoadUI)_loadScene.Instantiate();
					AddChild(LoadSceneNode);
					if (StageData != null)
					{
						LoadSceneNode.SetUp(StageData);
					}

					// Begin loading scene
					_loadingScene = _nextScene;
					_nextScene = null;
					Godot.ResourceLoader.LoadThreadedRequest(_loadingScene);
				}

				// Check if we are loading a scene
				if (_loadingScene != null)
				{
					// Check if scene is loaded
					var progress = new Godot.Collections.Array();
					switch (ResourceLoader.LoadThreadedGetStatus(_loadingScene, progress))
					{
						case ResourceLoader.ThreadLoadStatus.InProgress:
							// Update progress
							LoadSceneNode.SetProgress((float)progress[0]);
							return;
						case ResourceLoader.ThreadLoadStatus.Loaded:
							// Get scene
							var packedScene = (PackedScene)ResourceLoader.LoadThreadedGet(_loadingScene);

							_loadingScene = null;

							if (_nextScene == null)
							{
								// Unload UI


								// Instantiate scene
								_scene = packedScene.Instantiate();
								AddChild(_scene);
								LoadSceneNode.Player.Play("LoadEnd");
								
							   
								// Setup scene
								SetupScene();
								return;
							}
							break;
						default:
							// Failed to load scene
							Godot.GD.PushError("Failed to load scene: " + _loadingScene);
							_loadingScene = null;
							return;
					}
				}
			}
		}
		public static void FreeLoader()
		{
			LoadSceneNode = null;
		}
		public void LoadSettings()
		{
			if (settingsFile.Load("res://settings.cfg") != Error.Ok)
			{
				SetupSettings();
			}
			else
			{
				AudioServer.SetBusVolumeDb(0, Mathf.LinearToDb(settingsFile.GetValue("AUDIO", "Master").AsSingle()));
				AudioServer.SetBusVolumeDb(1, Mathf.LinearToDb(settingsFile.GetValue("AUDIO", "Music").AsSingle()));
				AudioServer.SetBusVolumeDb(2, Mathf.LinearToDb(settingsFile.GetValue("AUDIO", "Sound").AsSingle()));
				AudioServer.SetBusVolumeDb(3, Mathf.LinearToDb(settingsFile.GetValue("AUDIO", "Voice").AsSingle()));
				DisplayServer.WindowSetSize(settingsFile.GetValue("VIDEO", "Resolution").AsVector2I());
				DisplayServer.WindowSetMode((DisplayServer.WindowMode)(settingsFile.GetValue("VIDEO", "WindowMode").AsUInt16()));
				ProjectSettings.SetSetting("rendering/scaling_3d/scale", settingsFile.GetValue("VIDEO", "Scaling"));
				CenterWindow();
			};


		}

		public void SetupSettings()
		{
			//Auto-detect Window size
			DisplayServer.WindowSetSize(DisplayServer.ScreenGetSize());
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Maximized);
			DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Enabled);
			CenterWindow();
			//Write to config file
			settingsFile.SetValue("VIDEO", "Resolution", DisplayServer.WindowGetSize().X.ToString() + "x" + DisplayServer.WindowGetSize().Y.ToString());
			settingsFile.SetValue("VIDEO", "Vsync", 1);
			settingsFile.SetValue("VIDEO", "Scaling", ProjectSettings.GetSetting("rendering/scaling_3d/scale"));
			settingsFile.SetValue("VIDEO", "WindowMode", 0);
			settingsFile.SetValue("VIDEO", "Graphics", 0);
			settingsFile.SetValue("VIDEO", "ColourBlind", 0);
			SaveAudioSettings();

			settingsFile.Save("res://settings.cfg");

		}

		private static void SaveAudioSettings()
		{
			settingsFile.SetValue("AUDIO", "Master", Mathf.DbToLinear(AudioServer.GetBusVolumeDb(0)));
			settingsFile.SetValue("AUDIO", "Music", Mathf.DbToLinear(AudioServer.GetBusVolumeDb(1)));
			settingsFile.SetValue("AUDIO", "Sound", Mathf.DbToLinear(AudioServer.GetBusVolumeDb(2)));
			settingsFile.SetValue("AUDIO", "Voice", Mathf.DbToLinear(AudioServer.GetBusVolumeDb(3)));
		}

		public static void SaveSetting()
		{
			settingsFile.SetValue("VIDEO", "Resolution", DisplayServer.WindowGetSize());
			settingsFile.SetValue("VIDEO", "Scaling", ProjectSettings.GetSetting("rendering/scaling_3d/scale"));
			settingsFile.SetValue("VIDEO", "WindowMode", (int)DisplayServer.WindowGetMode());
			SaveAudioSettings();
			settingsFile.Save("res://settings.cfg");
		}

		public static void CenterWindow()
		{
			Vector2I center = DisplayServer.ScreenGetPosition() + (DisplayServer.ScreenGetSize() / 2);
			Vector2I windowSize = DisplayServer.WindowGetSizeWithDecorations();
			DisplayServer.WindowSetPosition(center - windowSize / 2);
		}
		// Get clock
		public static ulong GetClock()
		{
			return Singleton()._clock;
		}

		// Net server classes
		public static Net.IServer GetServer()
		{
			return Singleton()._server;
		}

		/// <summary>
		/// Gets the Host of the Multiplayer Game
		/// </summary>
		/// <returns>The HostServer Singleton</returns>
		public static Net.IHostServer GetHostServer()
		{
			return Singleton()._server as Net.IHostServer;
		}

		public static Net.NetSync GetNetSync()
		{
			return Singleton()._netsync;
		}

		public static void Rpc(Godot.Node node, string name, params Godot.Variant[] args) => GetServer().Rpc(node, name, args);

		// Net server connection
		public static void StartLocalServer()
		{
			// Start local server
			DisconnectServer();
			Singleton()._server = new Net.LocalServer();

			// Create net sync
			Singleton()._netsync = new Net.NetSync();
		}

		public static void StartHostServer(int port, int max_clients, bool upnp)
		{
			// Start host server
			DisconnectServer();
			Singleton()._server = new Net.HostServer(Singleton()._multiplayerApi, port, max_clients, upnp);

			// Create net sync
			Singleton()._netsync = new Net.NetSync();
		}

		public static void JoinServer(string ip, int port)
		{
			// Start remote server
			DisconnectServer();
			Singleton()._server = new Net.RemoteServer(Singleton()._multiplayerApi, ip, port);
		}

		public static void DisconnectServer()
		{
			if (Singleton()._server != null)
			{
				// Disconnect server
				Singleton()._server.Disconnect();
				Singleton()._server = null;

				// Destroy net sync
				if (Singleton()._netsync != null)
                {
                    Singleton()._netsync = null;
                }

                // Send us back to the main menu
                Singleton().LoadScene("res://Scene/Menu/NetTest.tscn");
			}
		}

		// RPC methods
		private void Rpc_PeerConnected(int id)
		{
			GD.Print("Peer Connection called: " + id);

			// Check if we're the host
			Net.IHostServer host_server = GetHostServer();
			if (host_server != null)
			{
				// Sync peer
				Net.NetSync net_sync = GetNetSync();
				net_sync.SyncPeer(id);
			}

			// Spawn peer if not client
			if (_players != null)
			{
				if (id != _server.GetPeerId())
                {
                    SpawnPeer(id);
                }
            }
		}

		private void Rpc_PeerDisconnected(int id)
		{
			// If this peer is the host, disconnect
			if (id == 1)
            {
                DisconnectServer();
            }

            // Remove peer
            if (_players != null)
            {
                DespawnPeer(id);
            }
        }

        [Rpc(MultiplayerApi.RpcMode.Authority)]
        public void Rpc_SetScene(string scene, StageData data = null) => LoadScene(scene, data);

        // RPC forwarding
        // We avoid using RPC on nodes directly, as this can cause issues with RPC sync
        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
		public void Rpc_ServerForward(NodePath path, string name, Godot.Collections.Array args)
		{
			// Call method
			Node node = GetNodeOrNull(path);
			if (node != null && node.HasMethod(name))
            {
                node.Callv(name, args);
            }
        }

		[Rpc(MultiplayerApi.RpcMode.Authority)]
		public void Rpc_ClientForward(NodePath path, string name, Godot.Collections.Array args)
		{
			// Call method
			Node node = GetNodeOrNull(path);
			if (node != null && node.HasMethod(name))
            {
                GD.Print(node.HasMethod(name) + " " + name);
                node.Callv(name, args);
            } 
        }
	}
}
