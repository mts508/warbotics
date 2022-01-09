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

            WebSocketServer server = new WebSocketServer(6060);
            server.AddWebSocketService<LobbyServer.LobbyServerService>("/LobbyGameClientSessionManager");
            server.AddWebSocketService<BridgeServer.BridgeServerProtocol>("/BridgeServer");
            server.Log.Level = LogLevel.Debug;

            Thread t = new Thread(() => DirectoryServer.DirectoryServer.RunServer());
            t.Start();


            server.Start();
            Console.WriteLine("Lobby server started");
            Console.ReadLine();
            //Console.ReadKey();
            //server.Stop();
        }
    }
}
