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
    public class HostServer : IHostServer
    {
        // Multiplayer API
        private MultiplayerApi _multiplayerApi;

        // Remote peer ID override
        private int m_remote_peer_id = 0;
        private Upnp upnp;
        private int port;

        // Remote server
        public HostServer(MultiplayerApi multiplayer_api, int port, int max_clients, bool upnp = false)
        {
            this.port = port;

            // Connect multiplayer peer
            ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
            peer.CreateServer(port, max_clients);

            // Setup multiplayer API
            peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);

            _multiplayerApi = multiplayer_api;
            _multiplayerApi.MultiplayerPeer = peer;

            if (upnp) UpnpSetup();
        }
        public void UpnpSetup()
        {
            upnp = new();
            int discoverResult = upnp.Discover();
            if (discoverResult == 0)
            {
                if (upnp.GetGateway() != null && upnp.GetGateway().IsValidGateway())
                {
                    int mapResultUDP = upnp.AddPortMapping(port, port, ProjectSettings.GetSetting("application/config/name").ToString(), "UDP", 0);
                    int mapResultTCP = upnp.AddPortMapping(port, port, ProjectSettings.GetSetting("application/config/name").ToString(), "TCP", 0);

                    if (mapResultUDP != 0) upnp.AddPortMapping(port, port, "", "UDP");
                    if (mapResultTCP != 0) upnp.AddPortMapping(port, port, "", "TCP");
                    GD.Print(mapResultTCP + " UDP: " + mapResultUDP);
                }
                string extIP = upnp.QueryExternalAddress();
                GD.Print(extIP + "\nPort: " + port);
            }

            upnp.DeletePortMapping(port, "UDP");
            upnp.DeletePortMapping(port, "TCP");
        }
        // Returns your own peer ID
        public int GetPeerId() => _multiplayerApi.GetUniqueId();

        // Returns all peer IDs
        public int[] GetPeerIds() => _multiplayerApi.GetPeers();

        // Returns the peer ID coming from the server
        public int GetRemotePeerId()
        {
            return (m_remote_peer_id != 0) ? m_remote_peer_id : _multiplayerApi.GetRemoteSenderId();
        }

        // Send RPC to the server
        public void Rpc(Node node, string name, params Variant[] args)
        {
            // Forward RPC to the server root
            Root.Singleton().Rpc_ServerForward(Root.Singleton().GetPathTo(node), name, new Godot.Collections.Array(args));
        }

        // Send RPC to all peers
        public void RpcAll(Node node, string name, params Variant[] args)
        {
            // Send RPC to the server
            RpcId(1, node, name, args);

            // Send RPC to all peers
            int[] peers = GetPeerIds();
            foreach (int peer in peers)
            {
                RpcId(peer, node, name, args);
            }
        }
        // Send RPC to others
        public void RpcOthers(Node node, string name, params Variant[] args)
        {
            // Send RPC to the server
            RpcId(1, node, name, args);

            // Send RPC to all peers
            int[] peers = GetPeerIds();
            int you = GetPeerId();
            foreach (int peer in peers)
            {
                if (peer != you)
                {
                    RpcId(peer, node, name, args);
                }
            }
        }

        // Send RPC to a specific peer
        public void RpcId(int peer_id, Node node, string name, params Variant[] args)
        {
            if (peer_id == 1)
            {
                // Call RPC locally
                m_remote_peer_id = 1;
                node.Call(name, args);
                m_remote_peer_id = 0;
            }
            else
            {
                // Forward RPC to the client root
                Variant[] forward = { Root.Singleton().GetPathTo(node), name, new Godot.Collections.Array(args) };
                _multiplayerApi.Rpc(peer_id, Root.Singleton(), "Rpc_ClientForward", new Godot.Collections.Array(forward));
            }
        }

        // Disconnect from the server
        public void Disconnect()
        {
            // Stop accepting new connections
            _multiplayerApi.MultiplayerPeer.RefuseNewConnections = true;

            // Disconnect all peers
            int[] peers = GetPeerIds();
            foreach (int peer in peers)
            {
                _multiplayerApi.MultiplayerPeer.DisconnectPeer(peer, true);
            }
            // Close server
            _multiplayerApi.MultiplayerPeer.Close();

            // Remove multiplayer peer
            _multiplayerApi.MultiplayerPeer = null;
            _multiplayerApi = null;
        }
    }
}
