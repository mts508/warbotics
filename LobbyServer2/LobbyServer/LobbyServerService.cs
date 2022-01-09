﻿using Warbotic.LobbyServer.Account;
using Warbotic.LobbyServer.Friend;
using Warbotic.LobbyServer.Session;
using Warbotic.LobbyServer.Store;
using EvoS.Framework.Constants.Enums;
using EvoS.Framework.Logging;
using EvoS.Framework.Network.NetworkMessages;
using EvoS.Framework.Network.Static;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using WebSocketSharp;

namespace Warbotic.LobbyServer
{
    public class LobbyServerService : LobbyServerProtocolBase
    {
        /// <summary>
        /// Register all the callbacks that will handle client requests
        /// </summary>
        protected override void OnOpen()
        {
            RegisterHandler(new EvosMessageDelegate<RegisterGameClientRequest>(HandleRegisterGame));
            RegisterHandler(new EvosMessageDelegate<OptionsNotification>(HandleOptionsNotification));
            RegisterHandler(new EvosMessageDelegate<CustomKeyBindNotification>(HandleCustomKeyBindNotification));
            RegisterHandler(new EvosMessageDelegate<PricesRequest>(HandlePricesRequest));
            RegisterHandler(new EvosMessageDelegate<PlayerUpdateStatusRequest>(HandlePlayerUpdateStatusRequest));
            RegisterHandler(new EvosMessageDelegate<PlayerMatchDataRequest>(HandlePlayerMatchDataRequest));
            RegisterHandler(new EvosMessageDelegate<SetGameSubTypeRequest>(HandleSetGameSubTypeRequest));
            RegisterHandler(new EvosMessageDelegate<PlayerInfoUpdateRequest>(HandlePlayerInfoUpdateRequest));
            RegisterHandler(new EvosMessageDelegate<CheckAccountStatusRequest>(HandleCheckAccountStatusRequest));
            RegisterHandler(new EvosMessageDelegate<CheckRAFStatusRequest>(HandleCheckRAFStatusRequest));
            RegisterHandler(new EvosMessageDelegate<ClientErrorSummary>(HandleClientErrorSummary));
            RegisterHandler(new EvosMessageDelegate<PreviousGameInfoRequest>(HandlePreviousGameInfoRequest));
            RegisterHandler(new EvosMessageDelegate<PurchaseTintRequest>(HandlePurchaseTintRequest));
            RegisterHandler(new EvosMessageDelegate<JoinMatchmakingQueueRequest>(HandleJoinMatchMakingQueueRequest));

            /*
            RegisterHandler(new EvosMessageDelegate<PurchaseModResponse>(HandlePurchaseModRequest));
            RegisterHandler(new EvosMessageDelegate<PurchaseTauntRequest>(HandlePurchaseTauntRequest));
            RegisterHandler(new EvosMessageDelegate<PurchaseBannerBackgroundRequest>(HandlePurchaseBannerRequest));
            RegisterHandler(new EvosMessageDelegate<PurchaseBannerForegroundRequest>(HandlePurchaseEmblemRequest));
            RegisterHandler(new EvosMessageDelegate<PurchaseChatEmojiRequest>(HandlePurchaseChatEmoji));
            RegisterHandler(new EvosMessageDelegate<PurchaseLoadoutSlotRequest>(HandlePurchaseLoadoutSlot));
            */

        }

        protected override void OnClose(CloseEventArgs e)
        {
            LobbyPlayerInfo playerInfo = SessionManager.GetPlayerInfo(this.AccountId);
            if (playerInfo != null)
            {
                EvoS.Framework.Logging.Log.Print(LogType.Lobby, string.Format(Config.Messages.PlayerDisconnected, this.UserName));
                SessionManager.OnPlayerDisconnect(this);
            }
        }

        public void HandleRegisterGame(RegisterGameClientRequest request)
        {
            try
            {
                LobbyPlayerInfo playerInfo = SessionManager.OnPlayerConnect(this, request);

                if (playerInfo != null)
                {
                    EvoS.Framework.Logging.Log.Print(LogType.Lobby, string.Format(Config.Messages.LoginSuccess, this.UserName));
                    RegisterGameClientResponse response = new RegisterGameClientResponse
                    {
                        AuthInfo = request.AuthInfo,
                        SessionInfo = request.SessionInfo,
                        ResponseId = request.RequestId
                    };

                    Send(response);

                    SendLobbyServerReadyNotification();
                }
                else
                {
                    SendErrorResponse(new RegisterGameClientResponse(), request.RequestId, Config.Messages.LoginFailed);
                }
            }
            catch (Exception e)
            {
                SendErrorResponse(new RegisterGameClientResponse(), request.RequestId, e);
            }
        }

        public void HandleOptionsNotification(OptionsNotification notification) {}
        public void HandleCustomKeyBindNotification(CustomKeyBindNotification notification) { }
        public void HandlePricesRequest(PricesRequest request)
        {
            PricesResponse response = StoreManager.GetPricesResponse();
            response.ResponseId = request.RequestId;
            Send(response);
        }

        public void HandlePlayerUpdateStatusRequest(PlayerUpdateStatusRequest request)
        {
            EvoS.Framework.Logging.Log.Print(LogType.Lobby, $"{this.UserName} is now {request.StatusString}");
            PlayerUpdateStatusResponse response = FriendManager.OnPlayerUpdateStatusRequest(this, request);

            Send(response);
        }

        public void HandlePlayerMatchDataRequest(PlayerMatchDataRequest request)
        {
            PlayerMatchDataResponse response = new PlayerMatchDataResponse()
            {
                MatchData = new List<EvoS.Framework.Network.Static.PersistedCharacterMatchData>(),
                ResponseId = request.RequestId
            };

            Send(response);
        }

        public void HandleSetGameSubTypeRequest(SetGameSubTypeRequest request)
        {
            this.SelectedSubTypeMask = request.SubTypeMask;
            SetGameSubTypeResponse response = new SetGameSubTypeResponse() { ResponseId = request.RequestId };
            Send(response);
        }

        public void HandlePlayerInfoUpdateRequest(PlayerInfoUpdateRequest request)
        {
            LobbyPlayerInfoUpdate playerInfoUpdate = request.PlayerInfoUpdate;
            

            if (request.GameType != null && request.GameType.HasValue)
                SetGameType(request.GameType.Value);

            if (playerInfoUpdate.CharacterType != null && playerInfoUpdate.CharacterType.HasValue)
            {
                SetCharacterType(playerInfoUpdate.CharacterType.Value);
                LobbyPlayerInfo playerInfo = SessionManager.GetPlayerInfo(this.AccountId);

                PersistedAccountData accountData = AccountManager.GetPersistedAccountData(this.AccountId);
                // should be automatic when account gets its data from database, but for now we modify the needed things here
                accountData.AccountComponent.LastCharacter = playerInfo.CharacterInfo.CharacterType;
                accountData.AccountComponent.SelectedBackgroundBannerID = playerInfo.BannerID;
                accountData.AccountComponent.SelectedForegroundBannerID = playerInfo.EmblemID;
                accountData.AccountComponent.SelectedRibbonID = playerInfo.RibbonID;
                accountData.AccountComponent.SelectedTitleID = playerInfo.TitleID;
                // end "should be automatic"
                
                PlayerAccountDataUpdateNotification updateNotification = new PlayerAccountDataUpdateNotification()
                {
                    AccountData = accountData
                };
                Send(updateNotification);

                PlayerInfoUpdateResponse response = new PlayerInfoUpdateResponse()
                {
                    PlayerInfo = playerInfo,
                    CharacterInfo = playerInfo.CharacterInfo,
                    OriginalPlayerInfoUpdate = request.PlayerInfoUpdate,
                    ResponseId = request.RequestId
                };
                Send(response);
            }

            if (playerInfoUpdate.AllyDifficulty != null && playerInfoUpdate.AllyDifficulty.HasValue)
                SetAllyDifficulty(playerInfoUpdate.AllyDifficulty.Value);
            if (playerInfoUpdate.CharacterAbilityVfxSwaps != null && playerInfoUpdate.CharacterAbilityVfxSwaps.HasValue)
                SetCharacterAbilityVfxSwaps(playerInfoUpdate.CharacterAbilityVfxSwaps.Value);
            if (playerInfoUpdate.CharacterCards != null && playerInfoUpdate.CharacterCards.HasValue)
                SetCharacterCards(playerInfoUpdate.CharacterCards.Value);
            if (playerInfoUpdate.CharacterLoadoutChanges != null && playerInfoUpdate.CharacterLoadoutChanges.HasValue)
                SetCharacterLoadoutChanges(playerInfoUpdate.CharacterLoadoutChanges.Value);
            if (playerInfoUpdate.CharacterMods != null && playerInfoUpdate.CharacterMods.HasValue)
                SetCharacterMods(playerInfoUpdate.CharacterMods.Value);
            if (playerInfoUpdate.CharacterSkin!= null && playerInfoUpdate.CharacterSkin.HasValue)
                SetCharacterSkin(playerInfoUpdate.CharacterSkin.Value);
            
            if (playerInfoUpdate.ContextualReadyState != null && playerInfoUpdate.ContextualReadyState.HasValue)
                SetContextualReadyState(playerInfoUpdate.ContextualReadyState.Value);
            if (playerInfoUpdate.EnemyDifficulty != null && playerInfoUpdate.EnemyDifficulty.HasValue)
                SetEnemyDifficulty(playerInfoUpdate.EnemyDifficulty.Value);
            if (playerInfoUpdate.LastSelectedLoadout != null && playerInfoUpdate.LastSelectedLoadout.HasValue)
                SetLastSelectedLoadout(playerInfoUpdate.LastSelectedLoadout.Value);

            //Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
        }

        public void HandleCheckAccountStatusRequest(CheckAccountStatusRequest request)
        {
            CheckAccountStatusResponse response = new CheckAccountStatusResponse()
            {
                QuestOffers = new QuestOfferNotification() { OfferDailyQuest = false },
                ResponseId = request.RequestId
            };
            Send(response);
        }

        public void HandleCheckRAFStatusRequest(CheckRAFStatusRequest request)
        {
            CheckRAFStatusResponse response = new CheckRAFStatusResponse()
            {
                ReferralCode = "sampletext",
                ResponseId = request.RequestId
            };
            Send(response);
        }

        public void HandleClientErrorSummary(ClientErrorSummary request)
        {
        }

        public void HandlePreviousGameInfoRequest(PreviousGameInfoRequest request)
        {
            PreviousGameInfoResponse response = new PreviousGameInfoResponse()
            {
                PreviousGameInfo =  null,
                ResponseId = request.RequestId
            };
            Send(response);
        }

        public void HandlePurchaseTintRequest(PurchaseTintRequest request)
        {
            Console.WriteLine("PurchaseTintRequest " + JsonConvert.SerializeObject(request));

            PurchaseTintResponse response = new PurchaseTintResponse()
            {
                Result = PurchaseResult.Success,
                CurrencyType = request.CurrencyType,
                CharacterType = request.CharacterType,
                SkinId = request.SkinId,
                TextureId = request.TextureId,
                TintId = request.TintId,
                ResponseId = request.RequestId
            };
            Send(response);

            Character.SkinHelper sk = new Character.SkinHelper();
            sk.AddSkin(request.CharacterType, request.SkinId, request.TextureId, request.TintId);
            sk.Save();
        }

        public void HandleJoinMatchMakingQueueRequest(JoinMatchmakingQueueRequest request)
        {
            JoinMatchmakingQueueResponse response = new JoinMatchmakingQueueResponse()
            {
                LocalizedFailure = null,
                ResponseId = request.RequestId
            };

            Send(response);
        }

        


    }
}
