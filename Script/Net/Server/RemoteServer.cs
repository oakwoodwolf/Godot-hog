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
    public class RemoteServer : IServer
    {
        // Multiplayer API
        private MultiplayerApi _multiplayerApi;

        // Remote server
        public RemoteServer(MultiplayerApi multiplayerApi, string ip, int port)
        {
            // Connect multiplayer peer
            var peer = new ENetMultiplayerPeer();
            peer.CreateClient(ip, port);

            // Setup multiplayer API
            peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
            _multiplayerApi = multiplayerApi;
            _multiplayerApi.MultiplayerPeer = peer;
            GD.Print("Joining Remote Server: " + ip + ":" + port);
        }

        // Returns your own peer ID
        public int GetPeerId() => _multiplayerApi.GetUniqueId();

        // Returns all peer IDs
        public int[] GetPeerIds() => _multiplayerApi.GetPeers();

        // Returns the peer ID coming from the server
        public int GetRemotePeerId() => _multiplayerApi.GetRemoteSenderId();

        // Send RPC to the server
        public void Rpc(Node node, string name, params Variant[] args)
        {
            // Forward RPC to the server root
            Variant[] forward = { Root.Singleton().GetPathTo(node), name, new Godot.Collections.Array(args) };
            _multiplayerApi.Rpc(1, Root.Singleton(), nameof(Root.Rpc_ServerForward), new Godot.Collections.Array(forward));
        }

        // Disconnect
        public void Disconnect()
        {
            // Disconnect from the server
            _multiplayerApi.MultiplayerPeer.Close();

            // Remove multiplayer peer
            _multiplayerApi.MultiplayerPeer = null;
            _multiplayerApi = null;
        }
    }
}
