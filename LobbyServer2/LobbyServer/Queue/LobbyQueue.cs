using EvoS.Framework.Constants.Enums;
using EvoS.Framework.Logging;
using EvoS.Framework.Network.NetworkMessages;
using EvoS.Framework.Network.Static;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Warbotic.LobbyServer.Config;
using Warbotic.LobbyServer.Session;

namespace Warbotic.LobbyServer.Queue
{
    internal class LobbyQueue
    {
        LobbyMatchmakingQueueInfo QueueInfo = new LobbyMatchmakingQueueInfo();
        LobbyGameInfo CurrentGameInfo;
        public LobbyQueue(GameType gameType)
        {
            QueueInfo.AverageWaitTime = TimeSpan.FromSeconds(8);
            QueueInfo.GameConfig = GameConfig.Get().CreateGameConfig(gameType);
            QueueInfo.PlayersPerMinute = 1;
            QueueInfo.QueuedPlayers = 0;
            QueueInfo.QueueStatus = QueueStatus.WaitingForHumans;
            QueueInfo.ShowQueueSize = true;

            CurrentGameInfo = GameConfig.Get().CreateGameInfo(QueueInfo.GameConfig);
        }

        public void AddPlayer(LobbyClientConnection client)
        {
            SessionManager.GetPlayerInfo(client.AccountId).ReadyState = ReadyState.Ready;

            NotifyQueueAssignment(client);

            Task.Delay(5000).ContinueWith(o =>
            {
                Log.Print(LogType.Debug, "SENDING GAME ASSIGNMENT NOTIFICATION");
                NotifyQueueUnassignment(client);

                // Assembling
                //CurrentGameInfo.GameStatus = GameStatus.Assembling;
                //NotifyGameAssignment(client);

                // FreelancerSelecting, SetGameStatus is probably called with anoter notification rather than GameAssignment
                CurrentGameInfo.GameStatus = GameStatus.FreelancerSelecting;
                NotifyGameAssignment(client);

                // --------------------------------------------------
                // A bunch of 'Received Game Info Notification' here
                // --------------------------------------------------

                //Loadout Selecting
                CurrentGameInfo.GameStatus = GameStatus.LoadoutSelecting;
                SendGameInfoNotification(client);

                // after 30 seconds
                /*
                CurrentGameInfo.GameStatus = GameStatus.Launching;
                SendGameInfoNotification(client);
                */




            });
            
            Log.Print(LogType.Debug, "ASSIGNED TO MATCHMAKING QUEUE");
            


        }

        private void NotifyQueueAssignment(LobbyClientConnectionBase client)
        {
            // Sending this two times can cause the client to think that it has been unassigned from the queue
            MatchmakingQueueAssignmentNotification notification = new MatchmakingQueueAssignmentNotification()
            {
                MatchmakingQueueInfo = QueueInfo
            };
            client.Send(notification);
        }

        private void NotifyQueueUnassignment(LobbyClientConnectionBase client)
        {
            MatchmakingQueueAssignmentNotification notification = new MatchmakingQueueAssignmentNotification()
            {
                MatchmakingQueueInfo = null
            };
            client.Send(notification);
        }

        private void NotifyGameAssignment(LobbyClientConnectionBase client)
        {
            LobbyPlayerInfo playerInfo = SessionManager.GetPlayerInfo(client.AccountId);
            playerInfo.ReadyState = ReadyState.Ready;

            GameAssignmentNotification notification = new GameAssignmentNotification()
            {
                GameInfo = CurrentGameInfo,
                PlayerInfo = playerInfo,
                GameResult = GameResult.NoResult,
                Reconnection = false,
                Observer = false
            };
            client.Send(notification);
        }

        /// <summary>
        /// Notification sent to notify about changes in the game status and team
        /// </summary>
        /// <param name="client"></param>
        private void SendGameInfoNotification(LobbyClientConnectionBase client)
        {
            LobbyPlayerInfo playerInfo = SessionManager.GetPlayerInfo(client.AccountId);
            if (CurrentGameInfo.GameStatus == GameStatus.LoadoutSelecting)
            {
                // By marking the player info as ready, we stop showing the "Change Freelancer" button in the loadout select screen
                playerInfo.ReadyState = ReadyState.Ready;
            }
            

            GameInfoNotification notification = new GameInfoNotification()
            {
                GameInfo = CurrentGameInfo,
                PlayerInfo = playerInfo,
                TeamInfo = new LobbyTeamInfo
                {
                    TeamPlayerInfo = new List<LobbyPlayerInfo> { playerInfo }
                },
            };

            client.Send(notification);
        }

        
    }
}
