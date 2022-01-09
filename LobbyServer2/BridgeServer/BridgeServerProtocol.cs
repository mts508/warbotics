﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;
using EvoS.Framework.Network;
using EvoS.Framework.Logging;
using EvoS.Framework.Network.Static;
using EvoS.Framework.Constants.Enums;
using Newtonsoft.Json;
using Warbotic.LobbyServer.Session;

namespace Warbotic.BridgeServer
{
    public class BridgeServerProtocol : WebSocketBehavior
    {
        public string Address;
        public int Port;
        private LobbyGameInfo GameInfo;
        private LobbyTeamInfo TeamInfo;
        private GameStatus GameStatus = GameStatus.Stopped;

        public enum BridgeMessageType
        {
            InitialConfig,
            SetLobbyGameInfo,
            SetTeamInfo,
            Start,
            Stop,
            GameStatusChange
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            MemoryStream stream = new MemoryStream(e.RawData);
            string data = "";

            BridgeMessageType messageType;
            
            using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
            {
                messageType = (BridgeMessageType)reader.Read();
                data = reader.ReadToEnd();
            }

            switch (messageType)
            {
                case BridgeMessageType.InitialConfig:
                    Address = data.Split(":")[0];
                    Port = Convert.ToInt32(data.Split(":")[1]);
                    ServerManager.AddServer(this);
                    break;
                default:
                    EvoS.Framework.Logging.Log.Print(LogType.Game, "Received unhandled message type");
                    break;
            }
        }

        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);
            ServerManager.RemoveServer(this.ID);
        }

        public bool IsAvailable()
        {
            return GameStatus == GameStatus.Stopped;
        }

        public void StartGame(LobbyGameInfo gameInfo, LobbyTeamInfo teamInfo)
        {
            GameInfo = gameInfo;
            TeamInfo = teamInfo;
            GameStatus = GameStatus.Assembling;

            SendGameInfo();
            SendTeamInfo();
            SendStartNotification();
        }

        private ReadOnlySpan<byte> GetBytesSpan(string str)
        {
            return new ReadOnlySpan<byte>(Encoding.GetEncoding("UTF-8").GetBytes(str));
        }

        public void SendGameInfo()
        {
            MemoryStream stream = new MemoryStream();
            stream.WriteByte((byte) BridgeMessageType.SetLobbyGameInfo);
            string jsonData = JsonConvert.SerializeObject(GameInfo);
            stream.Write(GetBytesSpan(jsonData));
            Send(stream.ToArray());
            EvoS.Framework.Logging.Log.Print(LogType.Game, "Setting Game Info");
        }

        public void SendTeamInfo()
        {
            MemoryStream stream = new MemoryStream();
            stream.WriteByte((byte)BridgeMessageType.SetTeamInfo);
            string jsonData = JsonConvert.SerializeObject(TeamInfo);
            stream.Write(GetBytesSpan(jsonData));
            Send(stream.ToArray());
            EvoS.Framework.Logging.Log.Print(LogType.Game, "Setting Team Info");
        }

        public void SendStartNotification()
        {
            MemoryStream stream = new MemoryStream();
            stream.WriteByte((byte)BridgeMessageType.Start);
            Send(stream.ToArray());
            EvoS.Framework.Logging.Log.Print(LogType.Game, "Starting Game Server");
        }
    }
}
