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

namespace SonicGodot.Net
{
    public class NetSync
    {
        // Net sync state
        private string _scenePath = "TestStage";

        // Net sync functions
        public void SetScene(string scene)
        {
            // Set scene path
            _scenePath = scene;

            // Load scene globally
            Net.IHostServer host_server = Root.GetHostServer();
            var success = ProjectSettings.LoadResourcePack("res://" + _scenePath + ".pck");
            if (success)
            {
                var data = (StageData) ResourceLoader.Load("res://Stages/" + _scenePath + "/Stage.tres");
                host_server.RpcAll(Root.Singleton(), nameof(Root.Rpc_SetScene), "res://Stages/" + _scenePath + "/Stage.tscn", data);
            }
        }

        // Net sync join syncing
        public void SyncPeer(int peer)
        {

            // Bring peer to current scene
            Net.IHostServer hostServer = Root.GetHostServer();
            GD.Print("HostServer " + _scenePath);
            //var data = (StageData)ResourceLoader.Load("res://Stages/" + _scenePath + "/Stage.tres");
            StageData data = null;
            //GD.Print(data.StageName + ": Data");
            hostServer.RpcId(peer, Root.Singleton(), nameof(Root.Rpc_SetScene), "res://Stages/" + _scenePath + "/Stage.tscn", data);
        }
    }
}

