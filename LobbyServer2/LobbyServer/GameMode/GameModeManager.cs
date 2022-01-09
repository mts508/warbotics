﻿using EvoS.Framework.Constants.Enums;
using EvoS.Framework.Network.NetworkMessages;
using EvoS.Framework.Network.Static;
using Warbotic.LobbyServer.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace Warbotic.LobbyServer.Gamemode
{
    class GameModeManager
    {
        public static Dictionary<GameType, GameTypeAvailability> GetGameTypeAvailabilities()
        {
            Dictionary<GameType, GameTypeAvailability> gameTypes = new Dictionary<GameType, GameTypeAvailability>();

            gameTypes.Add(GameType.Practice, GetPracticeGameTypeAvailability());
            gameTypes.Add(GameType.Coop, GetCoopGameTypeAvailability());
            gameTypes.Add(GameType.PvP, GetPvPGameTypeAvailability());
            gameTypes.Add(GameType.Ranked, GetRankedGameTypeAvailability());
            gameTypes.Add(GameType.Custom, GetCustomGameTypeAvailability());

            return gameTypes;
        }

        private static GameTypeAvailability GetPracticeGameTypeAvailability()
        {
            GameTypeAvailability type = new GameTypeAvailability();
            type.MaxWillFillPerTeam = 4;
            type.IsActive = ConfigManager.GameTypePracticeAvailable;
            type.QueueableGroupSizes = new Dictionary<int, RequirementCollection> { { 1, null } };
            type.TeamAPlayers = 1;
            type.TeamBBots = 2;

            type.SubTypes = new List<GameSubType>()
            {
                new GameSubType {
                    LocalizedName = "GenericPractice@SubTypes",
                    GameMapConfigs = new List<GameMapConfig>{ new GameMapConfig(Maps.VR_Practice, true) },
                    RewardBucket = GameBalanceVars.GameRewardBucketType.NoRewards,
                    PersistedStatBucket = PersistedStatBucket.DoNotPersist,
                    TeamAPlayers = 1,
                    TeamABots = 0,
                    TeamBPlayers = 0,
                    TeamBBots = 2,
                    Mods = new List<GameSubType.SubTypeMods>
                    {
                        GameSubType.SubTypeMods.AllowPlayingLockedCharacters,
                        GameSubType.SubTypeMods.HumansHaveFirstSlots
                    },
                    TeamComposition = new TeamCompositionRules
                    {
                        Rules = new Dictionary<TeamCompositionRules.SlotTypes, FreelancerSet>
                        {
                            {
                                TeamCompositionRules.SlotTypes.A1, new FreelancerSet
                                {
                                    Roles = new List<CharacterRole>
                                    {
                                        CharacterRole.Tank,
                                        CharacterRole.Assassin,
                                        CharacterRole.Support
                                    }
                                }
                            }, {
                                TeamCompositionRules.SlotTypes.B1, new FreelancerSet{Types = new List<CharacterType> {CharacterType.PunchingDummy}}
                            }, {
                                TeamCompositionRules.SlotTypes.B2, new FreelancerSet{Types = new List<CharacterType> {CharacterType.PunchingDummy}}
                            }
                        }
                    }
                }
            };

            return type;
        }

        private static GameTypeAvailability GetCoopGameTypeAvailability()
        {
            GameTypeAvailability type = new GameTypeAvailability();
            type.IsActive = ConfigManager.GameTypeCoopAvailable;
            type.MaxWillFillPerTeam = 0;
            type.SubTypes = new List<GameSubType>() {
                new GameSubType() {
                    TeamAPlayers = 4,
                    TeamABots = 0,
                    TeamBPlayers = 0,
                    TeamBBots = 4,

                    DuplicationRule = FreelancerDuplicationRuleTypes.noneInTeam,
                    GameMapConfigs = GameMapConfig.GetDeatmatchMaps(),
                    InstructionsToDisplay = GameSubType.GameLoadScreenInstructions.Default,

                }
            };
            return type;
        }

        private static GameTypeAvailability GetPvPGameTypeAvailability()
        {
            GameTypeAvailability type = new GameTypeAvailability();
            type.IsActive = ConfigManager.GameTypePvPAvailable;
            type.MaxWillFillPerTeam = 0;
            type.SubTypes = new List<GameSubType>()
            {
                new GameSubType
                {
                    LocalizedName = "GenericPvP@SubTypes",
                    GameMapConfigs = new List<GameMapConfig>{ new GameMapConfig(Maps.VR_Practice, true) },
                    RewardBucket = GameBalanceVars.GameRewardBucketType.NoRewards,
                    PersistedStatBucket = PersistedStatBucket.DoNotPersist,
                    TeamAPlayers = 1,
                    TeamABots = 0,
                    TeamBPlayers = 1,
                    TeamBBots = 0,
                    Mods = new List<GameSubType.SubTypeMods>
                    {
                        GameSubType.SubTypeMods.AllowPlayingLockedCharacters,
                        GameSubType.SubTypeMods.HumansHaveFirstSlots
                    },
                    TeamComposition = new TeamCompositionRules
                    {
                        Rules = new Dictionary<TeamCompositionRules.SlotTypes, FreelancerSet>
                        {
                            {
                                TeamCompositionRules.SlotTypes.A1, new FreelancerSet
                                {
                                    Roles = new List<CharacterRole>
                                    {
                                        CharacterRole.Tank,
                                        CharacterRole.Assassin,
                                        CharacterRole.Support
                                    }
                                }
                            }, {
                                TeamCompositionRules.SlotTypes.B1, new FreelancerSet
                                {
                                    Roles = new List<CharacterRole>
                                    {
                                        CharacterRole.Tank,
                                        CharacterRole.Assassin,
                                        CharacterRole.Support
                                    }
                                }
                            }
                        }
                    }
                }
            };
            return type;
        }

        private static GameTypeAvailability GetRankedGameTypeAvailability()
        {
            GameTypeAvailability type = new GameTypeAvailability();
            type.IsActive = ConfigManager.GameTypeRankedAvailable;
            type.MaxWillFillPerTeam = 0;
            type.SubTypes = new List<GameSubType>();
            return type;
        }

        private static GameTypeAvailability GetCustomGameTypeAvailability()
        {
            GameTypeAvailability type = new GameTypeAvailability();
            type.IsActive = ConfigManager.GameTypeCustomAvailable;
            type.MaxWillFillPerTeam = 0;
            type.SubTypes = new List<GameSubType>();
            return type;
        }

    }
}
