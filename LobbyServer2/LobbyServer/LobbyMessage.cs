using EvoS.Framework.Network.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace Warbotic.LobbyServer
{
    public delegate void LobbyMessage<T>(LobbyClientConnectionBase client,T message) where T : WebSocketMessage;
}
