using System;
using System.Collections.Generic;
using System.Text;

namespace Warbotic.LobbyServer.Queue
{
    internal class LobbyQueueManager
    {
        private static Dictionary<GameType, LobbyQueue> queues = new Dictionary<GameType, LobbyQueue>();

        public static LobbyQueue GetQueue(GameType gameType) {
            if (!queues.ContainsKey(gameType))
            {
                queues[gameType] = new LobbyQueue(gameType);
            }

            return queues[gameType];
        }
    }
}
