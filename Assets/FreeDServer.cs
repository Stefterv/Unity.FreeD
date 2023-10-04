using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FreeD { 
    public class FreeDServer
    {
        static Dictionary<int, FreeDServer> servers = new Dictionary<int, FreeDServer>();
        public static FreeDServer Get(int port)
        {
            FreeDServer server;
            var exists = servers.TryGetValue(port, out server);
            if (!exists)
            {
                server = new FreeDServer();
                server.listenPort = port;
                Task.Run(server.Start);
                servers.Add(port, server);
            }
            return server;
        }

        public int listenPort = 40_000;
        public delegate void PacketReceived(Packet packet);
        public PacketReceived received;

       
        public async void Start() {
            UdpClient listener = new UdpClient(listenPort);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);
            try
            {
                while (true)
                {
                    var result = await listener.ReceiveAsync();

                    try
                    {
                        var packet = Packet.Decode(result.Buffer);
                        if(received != null) received(packet);
                    }catch(Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }
            catch (SocketException e)
            {
                Debug.LogError(e);
            }
            finally
            {
                listener.Close();
            }
        }
    }
}