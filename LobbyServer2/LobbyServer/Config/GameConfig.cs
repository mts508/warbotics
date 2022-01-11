using EvoS.Framework.Constants.Enums;
using EvoS.Framework.Network.Static;
using System;
using System.Collections.Generic;
using System.Text;
using Warbotic.LobbyServer.Gamemode;

namespace Warbotic.LobbyServer.Config
{
    internal class GameConfig
    {
        private static GameConfig Instance = null;
        private GameConfig() { }
        
        public static GameConfig Get()
        {
            if (Instance == null)
            {
                Instance = new GameConfig();
            }
            return Instance;
        }
        public static string GetServerHost()
        {
            return null; // Didn't find any reference of use in the client code
        }

        public string GetServerAddress()
        {
            return "127.0.0.1:5000";
        }

        public string GetGameServerProcessCode()
        {
            return "0a101c3b-5ae5-c022"; // Example from old logs, can have any value later
        }

        public GameStatus GetInitialGameStatus()
        {
            return GameStatus.Assembling;
        }

        private string GetMap()
        {
            return Maps.Skyway_Deathmatch;
        }

        public LobbyGameConfig CreateGameConfig(GameType gameType)
        {
            GameTypeAvailability pvpSubType = GameModeManager.GetGameTypeAvailabilities()[GameType.PvP];

            return new LobbyGameConfig()
            {
                GameOptionFlags = GameOptionFlag.AllowDuplicateCharacters,
                GameType = gameType,
                IsActive = true,
                SubTypes = pvpSubType.SubTypes,
                Map = GetMap()
            };
        }

        public LobbyGameInfo CreateGameInfo(LobbyGameConfig gameConfig)
        {
            return new LobbyGameInfo
            {
                AcceptedPlayers = 1, // Total Accepted players for this match to start???
                AcceptTimeout = new TimeSpan(0, 0, 5), // TODO: what does this do???
                AccountIdToOverconIdToCount = new Dictionary<long, Dictionary<int, int>>(),
                ActiveHumanPlayers = 0, // Active humans in this game queue???
                ActivePlayers = 0, // Active humans + bots in this game queue???
                ActiveSpectators = 0,

                CreateTimestamp = DateTime.Now.Ticks,
                GameConfig = gameConfig,
                GameResult = GameResult.NoResult,
                GameServerAddress = GetServerAddress(),
                GameServerHost = GetServerHost(),
                GameServerProcessCode = GetGameServerProcessCode(),
                GameStatus = GetInitialGameStatus(),
                ggPackUsedAccountIDs = new Dictionary<long, int>(),
                IsActive = true,
                SelectedBotSkillTeamA = BotDifficulty.Medium,
                SelectedBotSkillTeamB = BotDifficulty.Medium,

                LoadoutSelectionStartTimestamp = 0,
                LoadoutSelectTimeout = TimeSpan.FromSeconds(30), // Time to select loadout(mods, skin, catalyst) before the match starts

                SelectionStartTimestamp = DateTime.Now.Ticks,
                SelectionSubPhase = FreelancerResolutionPhaseSubType.UNDEFINED,
                SelectionSubPhaseStartTimestamp = 0,

                SelectSubPhaseBan1Timeout = TimeSpan.Zero,
                SelectSubPhaseBan2Timeout = TimeSpan.Zero,
                SelectSubPhaseFreelancerSelectTimeout = TimeSpan.FromSeconds(30),
                SelectSubPhaseTradeTimeout = TimeSpan.FromSeconds(30),
                SelectTimeout = TimeSpan.FromSeconds(15),
                UpdateTimestamp = DateTime.Now.Ticks,
                
            };
        }
    }
}
