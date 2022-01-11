using System;
using System.Threading;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Warbotic
{
    public class WarboticServer
    {
        public static void Main(string[] args)
        {
            Banner.PrintBanner();

            // Directory Server
            Thread t = new Thread(() => DirectoryServer.DirectoryServer.RunServer());
            t.Start();

            WebSocketServer server = new WebSocketServer(6060);

            // Lobby
            server.AddWebSocketService<LobbyServer.LobbyClientConnection>("/LobbyGameClientSessionManager");

            // Bridge
            server.AddWebSocketService<BridgeServer.BridgeServerProtocol>("/BridgeServer");


            server.Log.Level = LogLevel.Debug;
            server.Start();
            Console.WriteLine("Lobby server started");
            Console.ReadLine();
            //Console.ReadKey();
            //server.Stop();
        }
    }
}
