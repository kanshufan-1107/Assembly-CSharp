using System;
using System.Collections.Generic;
using Blizzard.Commerce;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Network;
using BobNetProto;
using Hearthstone;
using PegasusGame;
using PegasusLettuce;
using PegasusLuckyDraw;
using PegasusShared;
using PegasusUtil;

namespace Networking;

public class ConnectAPI
{
	private readonly IDispatcher dispatcherImpl;

	private readonly DebugConnectionManager m_debugConnectionManager;

	public ConnectAPI(IDispatcher dispatcher, DebugConnectionManager debugConnectionManager)
	{
		dispatcherImpl = dispatcher;
		m_debugConnectionManager = debugConnectionManager;
	}

	public void AbortBlizzardPurchase(string deviceId, bool isAutoCanceled, CancelPurchase.CancelReason? reason, string error)
	{
		CancelPurchase packet = new CancelPurchase
		{
			IsAutoCancel = isAutoCanceled,
			DeviceId = deviceId,
			ErrorMessage = error
		};
		if (reason.HasValue)
		{
			packet.Reason = reason.Value;
		}
		SendUtilPacket(274, UtilSystemId.BATTLEPAY, packet);
	}

	public void AckAchieveProgress(int achievementId, int ackProgress)
	{
		SendUtilPacket(243, UtilSystemId.CLIENT, new AckAchieveProgress
		{
			Id = achievementId,
			AckProgress = ackProgress
		});
	}

	public void AckQuest(int questId)
	{
		SendUtilPacket(604, UtilSystemId.CLIENT, new AckQuest
		{
			QuestId = questId
		});
	}

	public void CheckForNewQuests()
	{
		SendUtilPacket(605, UtilSystemId.CLIENT, new CheckForNewQuests());
	}

	public void CheckForExpiredQuests()
	{
		SendUtilPacket(654, UtilSystemId.CLIENT, new CheckForExpiredQuests());
	}

	public void RerollQuest(int questId)
	{
		SendUtilPacket(606, UtilSystemId.CLIENT, new RerollQuest
		{
			QuestId = questId
		});
	}

	public void AbandonQuest(int questId)
	{
		SendUtilPacket(620, UtilSystemId.CLIENT, new AbandonQuest
		{
			QuestId = questId
		});
	}

	public void AckAchievement(int achievementId)
	{
		SendUtilPacket(612, UtilSystemId.CLIENT, new AckAchievement
		{
			AchievementId = achievementId
		});
	}

	public void ClaimAchievementReward(int achievementId, int chooseOneRewardId = 0)
	{
		SendUtilPacket(613, UtilSystemId.CLIENT, new ClaimAchievementReward
		{
			AchievementId = achievementId,
			ChooseOneRewardItemId = chooseOneRewardId
		});
	}

	public void AckRewardTrackReward(int rewardTrackId, int level, RewardTrackPaidType paidType)
	{
		SendUtilPacket(616, UtilSystemId.CLIENT, new AckRewardTrackReward
		{
			RewardTrackId = rewardTrackId,
			Level = level,
			PaidType = paidType
		});
	}

	public void CheckForRewardTrackSeasonRoll()
	{
		SendUtilPacket(655, UtilSystemId.CLIENT, new CheckForRewardTrackSeasonRoll());
	}

	public void SetActiveEventRewardTrack(int rewardTrackId)
	{
		SendUtilPacket(658, UtilSystemId.CLIENT, new SetActiveEventRewardTrack
		{
			RewardTrackId = rewardTrackId
		});
	}

	public void ClaimRewardTrackReward(int rewardTrackId, int level, RewardTrackPaidType paidType, int chooseOneRewardItemId)
	{
		SendUtilPacket(615, UtilSystemId.CLIENT, new ClaimRewardTrackReward
		{
			RewardTrackId = rewardTrackId,
			Level = level,
			ChooseOneRewardItemId = chooseOneRewardItemId,
			PaidType = paidType
		});
	}

	public void RespondToRedundantNDEReroll(List<long> noticeIds, bool didReroll)
	{
		SendUtilPacket(657, UtilSystemId.CLIENT, new RedundantNDERerollSelectionRequest
		{
			Entries = noticeIds,
			DidReroll = didReroll
		});
	}

	public void AckCardSeen(AckCardSeen ackCardSeenPacket)
	{
		SendUtilPacket(223, UtilSystemId.CLIENT, ackCardSeenPacket);
	}

	public void AckNotice(long noticeId)
	{
		SendUtilPacket(213, UtilSystemId.CLIENT, new AckNotice
		{
			Entry = noticeId
		});
	}

	public void AcknowledgeBanner(int bannerId)
	{
		SendUtilPacket(309, UtilSystemId.CLIENT, new AcknowledgeBanner
		{
			Banner = bannerId
		});
	}

	public void AckWingProgress(int wing, int ackProgress)
	{
		SendUtilPacket(308, UtilSystemId.CLIENT, new AckWingProgress
		{
			Wing = wing,
			Ack = ackProgress
		});
	}

	public void CheckAccountLicenseAchieve(int achieveId)
	{
		SendUtilPacket(297, UtilSystemId.BATTLEPAY, new CheckAccountLicenseAchieve
		{
			Achieve = achieveId
		});
	}

	public void Close()
	{
		dispatcherImpl.Close();
		m_debugConnectionManager.Shutdown();
	}

	public void Concede()
	{
		SendGamePacket(11, new Concede());
	}

	public void ConfirmPurchase()
	{
		SendUtilPacket(273, UtilSystemId.BATTLEPAY, new DoPurchase());
	}

	public void CreateDeck(DeckType deckType, string name, int heroId, bool isHeroOverridden, FormatType formatType, long sortOrder, DeckSourceType sourceType, int? cardBackId, int? cosmeticCoinId, string pastedDeckHash, int brawlLibraryItemId, int? requestId)
	{
		CreateDeck packet = new CreateDeck
		{
			Name = name,
			Hero = heroId,
			HeroOverridden = isHeroOverridden,
			DeckType = deckType,
			TaggedStandard = (formatType == FormatType.FT_STANDARD),
			SortOrder = sortOrder,
			SourceType = sourceType,
			PastedDeckHash = pastedDeckHash,
			BrawlLibraryItemId = brawlLibraryItemId,
			FormatType = formatType
		};
		if (requestId.HasValue)
		{
			packet.RequestId = requestId.Value;
		}
		if (cardBackId.HasValue)
		{
			packet.CardBack = cardBackId.Value;
		}
		if (cosmeticCoinId.HasValue)
		{
			packet.CosmeticCoin = cosmeticCoinId.Value;
		}
		SendUtilPacket(209, UtilSystemId.CLIENT, packet);
	}

	public DeckCreated DeckCreated()
	{
		return UnpackNextUtilPacket<DeckCreated>(217);
	}

	public DeckDeleted DeckDeleted()
	{
		return UnpackNextUtilPacket<DeckDeleted>(218);
	}

	public DeckRenamed DeckRenamed()
	{
		return UnpackNextUtilPacket<DeckRenamed>(219);
	}

	public void DecodeAndProcessPacket(PegasusPacket packet)
	{
		if (dispatcherImpl.DecodePacket(packet) != null)
		{
			dispatcherImpl.NotifyUtilResponseReceived(packet);
		}
	}

	public void DeleteDeck(long deckId, DeckType deckType)
	{
		SendUtilPacket(210, UtilSystemId.CLIENT, new DeleteDeck
		{
			Deck = deckId,
			DeckType = deckType
		});
	}

	public void DisconnectFromGameServer()
	{
		dispatcherImpl.DisconnectFromGameServer();
	}

	public void DoLoginUpdate(string referralSource)
	{
		SendUtilPacket(205, UtilSystemId.CLIENT, new UpdateLogin
		{
			Referral = referralSource
		}, RequestPhase.STARTUP);
	}

	public void DraftAckRewards(long deckId, int slot)
	{
		SendUtilPacket(287, UtilSystemId.CLIENT, new DraftAckRewards
		{
			DeckId = deckId,
			Slot = slot
		});
	}

	public void DraftBegin()
	{
		SendUtilPacket(235, UtilSystemId.CLIENT, new DraftBegin());
	}

	public DraftChosen GetDraftChosen()
	{
		return UnpackNextUtilPacket<DraftChosen>(249);
	}

	public DraftBeginning GetDraftBeginning()
	{
		return UnpackNextUtilPacket<DraftBeginning>(246);
	}

	public DraftChoicesAndContents GetDraftChoicesAndContents()
	{
		return UnpackNextUtilPacket<DraftChoicesAndContents>(248);
	}

	public DraftError DraftGetError()
	{
		return UnpackNextUtilPacket<DraftError>(251);
	}

	public void RequestDraftChoicesAndContents()
	{
		SendUtilPacket(244, UtilSystemId.CLIENT, new DraftGetChoicesAndContents());
	}

	public void SendArenaSessionRequest()
	{
		SendUtilPacket(346, UtilSystemId.CLIENT, new ArenaSessionRequest());
	}

	public ArenaSessionResponse GetArenaSessionResponse()
	{
		return UnpackNextUtilPacket<ArenaSessionResponse>(351);
	}

	public DraftRewardsAcked DraftRewardsAcked()
	{
		return UnpackNextUtilPacket<DraftRewardsAcked>(288);
	}

	public void DraftMakePick(long deckId, int slot, int index, int premium)
	{
		SendUtilPacket(245, UtilSystemId.CLIENT, new DraftMakePick
		{
			DeckId = deckId,
			Slot = slot,
			Index = index,
			Premium = premium
		});
	}

	public void DraftRetire(long deckId, int slot, int seasonId)
	{
		SendUtilPacket(242, UtilSystemId.CLIENT, new DraftRetire
		{
			DeckId = deckId,
			Slot = slot,
			SeasonId = seasonId
		});
	}

	public void DropDebugPacket()
	{
		m_debugConnectionManager.DropPacket();
	}

	public void DropGamePacket()
	{
		dispatcherImpl.DropGamePacket();
	}

	public void DropUtilPacket()
	{
		dispatcherImpl.DropUtilPacket();
	}

	public AccountLicenseAchieveResponse GetAccountLicenseAchieveResponse()
	{
		return UnpackNextUtilPacket<AccountLicenseAchieveResponse>(311);
	}

	public AccountLicensesInfoResponse GetAccountLicensesInfoResponse()
	{
		return UnpackNextUtilPacket<AccountLicensesInfoResponse>(325);
	}

	public AdventureProgressResponse GetAdventureProgressResponse()
	{
		return UnpackNextUtilPacket<AdventureProgressResponse>(306);
	}

	public HeroXP GetHeroXP()
	{
		return UnpackNextUtilPacket<HeroXP>(283);
	}

	public GetAssetResponse GetAssetResponse()
	{
		return UnpackNextUtilPacket<GetAssetResponse>(322);
	}

	public BattlePayConfigResponse GetBattlePayConfigResponse()
	{
		return UnpackNextUtilPacket<BattlePayConfigResponse>(238);
	}

	public BattlePayStatusResponse GetBattlePayStatusResponse()
	{
		return UnpackNextUtilPacket<BattlePayStatusResponse>(265);
	}

	public CancelQuestResponse GetCanceledQuestResponse()
	{
		return UnpackNextUtilPacket<CancelQuestResponse>(282);
	}

	public SetFavoriteCardBackResponse GetSetFavoriteCardBackResponse()
	{
		return UnpackNextUtilPacket<SetFavoriteCardBackResponse>(292);
	}

	public CardBacks GetCardBacks()
	{
		return UnpackNextUtilPacket<CardBacks>(236);
	}

	public BoughtSoldCard GetCardSaleResult()
	{
		return UnpackNextUtilPacket<BoughtSoldCard>(258);
	}

	public CardValues GetCardValues()
	{
		return UnpackNextUtilPacket<CardValues>(260);
	}

	public ClientStaticAssetsResponse GetClientStaticAssetsResponse()
	{
		return UnpackNextUtilPacket<ClientStaticAssetsResponse>(341);
	}

	public CollectionClientStateResponse GetCollectionClientStateResponse()
	{
		return UnpackNextUtilPacket<CollectionClientStateResponse>(713);
	}

	public SecondaryClientStateResponse GetSecondaryClientStateResponse()
	{
		return UnpackNextUtilPacket<SecondaryClientStateResponse>(714);
	}

	public DBAction GetDbAction()
	{
		return UnpackNextUtilPacket<DBAction>(216);
	}

	public Deadend GetDeadendGame()
	{
		return UnpackNextGamePacket<Deadend>(169);
	}

	public DeadendUtil GetDeadendUtil()
	{
		return UnpackNextUtilPacket<DeadendUtil>(167);
	}

	public DebugCommandResponse GetDebugCommandResponse()
	{
		return UnpackNextUtilPacket<DebugCommandResponse>(324);
	}

	public DebugConsoleCommand GetDebugConsoleCommand()
	{
		if (!m_debugConnectionManager.AllowDebugConnections())
		{
			return null;
		}
		return UnpackNextDebugPacket<DebugConsoleCommand>(123);
	}

	public DebugConsoleResponse GetDebugConsoleResponse()
	{
		if (!m_debugConnectionManager.AllowDebugConnections())
		{
			return null;
		}
		return UnpackNextGamePacket<DebugConsoleResponse>(124);
	}

	public GetDeckContentsResponse GetDeckContentsResponse()
	{
		return UnpackNextUtilPacket<GetDeckContentsResponse>(215);
	}

	public FreeDeckChoiceResponse GetFreeDeckChoiceResponse()
	{
		return UnpackNextUtilPacket<FreeDeckChoiceResponse>(334);
	}

	public FreeDeckStateUpdate GetFreeDeckStateUpdate()
	{
		return UnpackNextUtilPacket<FreeDeckStateUpdate>(656);
	}

	public DeckList GetDeckHeaders()
	{
		return UnpackNextUtilPacket<DeckList>(202);
	}

	public DraftRetired GetDraftRetired()
	{
		return UnpackNextUtilPacket<DraftRetired>(247);
	}

	public DraftRemovePremiumsResponse GetDraftDisablePremiumsResponse()
	{
		return UnpackNextUtilPacket<DraftRemovePremiumsResponse>(355);
	}

	public MulliganChooseOneTentativeSelection GetMulliganChooseOneTentativeSelection()
	{
		return UnpackNextGamePacket<MulliganChooseOneTentativeSelection>(44);
	}

	public EntitiesChosen GetEntitiesChosen()
	{
		return UnpackNextGamePacket<EntitiesChosen>(13);
	}

	public EntityChoices GetEntityChoices()
	{
		return UnpackNextGamePacket<EntityChoices>(17);
	}

	public FavoriteHeroesResponse GetFavoriteHeroesResponse()
	{
		return UnpackNextUtilPacket<FavoriteHeroesResponse>(318);
	}

	public GuardianVars GetGuardianVars()
	{
		return UnpackNextUtilPacket<GuardianVars>(264);
	}

	public GameCanceled GetGameCancelInfo()
	{
		return UnpackNextGamePacket<GameCanceled>(12);
	}

	public GameSetup GetGameSetup()
	{
		return UnpackNextGamePacket<GameSetup>(16);
	}

	public GamesInfo GetGamesInfo()
	{
		return UnpackNextUtilPacket<GamesInfo>(208);
	}

	public GameStartState GetGameStartState()
	{
		return dispatcherImpl.GameStartState;
	}

	public void SetGameStartState(GameStartState state)
	{
		dispatcherImpl.GameStartState = state;
	}

	public void GetGameState()
	{
		SendGamePacket(1, new GetGameState());
	}

	public void ResendOptions()
	{
		SendGamePacket(46, new ResendOptions());
	}

	public void UpdateBattlegroundInfo()
	{
		SendGamePacket(53, new UpdateBattlegroundInfo());
	}

	public void UpdateBattlegroundHeroArmorTierList()
	{
		SendGamePacket(54, new GetBattlegroundHeroArmorTierList());
	}

	public void UpdateBattlegroundsPlayerAnomaly()
	{
		SendGamePacket(61, new GetBattlegroundsPlayerAnomaly());
	}

	public void ToggleAIPlayer()
	{
		SendGamePacket(60, new ToggleAIPlayer());
	}

	public void SetBattlegroundHeroBuddyGained(int value, int playerId)
	{
		SendGamePacket(56, new SetBattlegroundHeroBuddyGained
		{
			HeroBuddyGained = value,
			PlayerId = playerId
		});
	}

	public void SetBattlegroundHeroBuddyProgress(int progress, int playerId)
	{
		SendGamePacket(55, new SetBattlegroundHeroBuddyProgress
		{
			HeroBuddyProgress = progress,
			PlayerId = playerId
		});
	}

	public void ReplaceBattlegroundHero(int heroID, int playerId)
	{
		SendGamePacket(57, new ReplaceBattlegroundHero
		{
			HeroId = heroID,
			PlayerId = playerId
		});
	}

	public void RequestReplaceBattlegroundsMulliganHero(int heroID)
	{
		SendGamePacket(64, new RequestReplaceBattlegroundsMulliganHero
		{
			HeroId = heroID
		});
	}

	public void RequestReplaceAllBattlegroundsMulliganHeroExcept(List<int> heroIDsToKeep)
	{
		SendGamePacket(66, new RequestReplaceAllBattlegroundsMulliganHeroExcept
		{
			HeroId = heroIDsToKeep
		});
	}

	public void RequestGameRoundHistory()
	{
		SendGamePacket(32, new GetGameRoundHistory());
	}

	public void RequestRealtimeBattlefieldRaces()
	{
		SendGamePacket(33, new GetGameRealTimeBattlefieldRaces());
	}

	public GenericResponse GetGenericResponse()
	{
		return UnpackNextUtilPacket<GenericResponse>(326);
	}

	public MassDisenchantResponse GetMassDisenchantResponse()
	{
		return UnpackNextUtilPacket<MassDisenchantResponse>(269);
	}

	public MedalInfo GetMedalInfo()
	{
		return UnpackNextUtilPacket<MedalInfo>(232);
	}

	public NAckOption GetNAckOption()
	{
		return UnpackNextGamePacket<NAckOption>(10);
	}

	public ClientStateNotification GetClientStateNotification()
	{
		return UnpackNextUtilPacket<ClientStateNotification>(333);
	}

	public BoosterContent GetOpenedBooster()
	{
		return UnpackNextUtilPacket<BoosterContent>(226);
	}

	public AllOptions GetAllOptions()
	{
		return UnpackNextGamePacket<AllOptions>(14);
	}

	public PlayerQuestStateUpdate GetPlayerQuestStateUpdate()
	{
		return UnpackNextUtilPacket<PlayerQuestStateUpdate>(601);
	}

	public PlayerQuestPoolStateUpdate GetPlayerQuestPoolStateUpdate()
	{
		return UnpackNextUtilPacket<PlayerQuestPoolStateUpdate>(602);
	}

	public PlayerAchievementStateUpdate GetPlayerAchievementStateUpdate()
	{
		return UnpackNextUtilPacket<PlayerAchievementStateUpdate>(603);
	}

	public PlayerRewardTrackStateUpdate GetPlayerRewardTrackStateUpdate()
	{
		return UnpackNextUtilPacket<PlayerRewardTrackStateUpdate>(614);
	}

	public RewardTrackXpNotification GetRewardTrackXpNotification()
	{
		return UnpackNextUtilPacket<RewardTrackXpNotification>(617);
	}

	public RewardTrackUnclaimedNotification GetRewardTrackUnclaimedNotification()
	{
		return UnpackNextUtilPacket<RewardTrackUnclaimedNotification>(619);
	}

	public RerollQuestResponse GetRerollQuestResponse()
	{
		return UnpackNextUtilPacket<RerollQuestResponse>(607);
	}

	public PlayerRecords GetPlayerRecords()
	{
		return UnpackNextUtilPacket<PlayerRecords>(270);
	}

	public PowerHistory GetPowerHistory()
	{
		return UnpackNextGamePacket<PowerHistory>(19);
	}

	public ProfileProgress GetProfileProgress()
	{
		return UnpackNextUtilPacket<ProfileProgress>(233);
	}

	public CancelPurchaseResponse GetCancelPurchaseResponse()
	{
		return UnpackNextUtilPacket<CancelPurchaseResponse>(275);
	}

	public PurchaseMethod GetPurchaseMethodResponse()
	{
		return UnpackNextUtilPacket<PurchaseMethod>(272);
	}

	public PurchaseResponse GetPurchaseResponse()
	{
		return UnpackNextUtilPacket<PurchaseResponse>(256);
	}

	public PurchaseWithGoldResponse GetPurchaseWithGoldResponse()
	{
		return UnpackNextUtilPacket<PurchaseWithGoldResponse>(280);
	}

	public RecruitAFriendDataResponse GetRecruitAFriendDataResponse()
	{
		return UnpackNextUtilPacket<RecruitAFriendDataResponse>(338);
	}

	public RecruitAFriendURLResponse GetRecruitAFriendUrlResponse()
	{
		return UnpackNextUtilPacket<RecruitAFriendURLResponse>(336);
	}

	public RewardProgress GetRewardProgress()
	{
		return UnpackNextUtilPacket<RewardProgress>(271);
	}

	public ServerResult GetServerResult()
	{
		return UnpackNextGamePacket<ServerResult>(23);
	}

	public SetFavoriteHeroResponse GetSetFavoriteHeroResponse()
	{
		return UnpackNextUtilPacket<SetFavoriteHeroResponse>(320);
	}

	public SetProgressResponse GetSetProgressResponse()
	{
		return UnpackNextUtilPacket<SetProgressResponse>(296);
	}

	public SpectatorNotify GetSpectatorNotify()
	{
		return UnpackNextGamePacket<SpectatorNotify>(24);
	}

	public AIDebugInformation GetAIDebugInformation()
	{
		return UnpackNextGamePacket<AIDebugInformation>(6);
	}

	public RopeTimerDebugInformation GetRopeTimerDebugInformation()
	{
		return UnpackNextGamePacket<RopeTimerDebugInformation>(8);
	}

	public ScriptDebugInformation GetScriptDebugInformation()
	{
		return UnpackNextGamePacket<ScriptDebugInformation>(7);
	}

	public GameRoundHistory GetGameRoundHistory()
	{
		return UnpackNextGamePacket<GameRoundHistory>(30);
	}

	public GameRealTimeBattlefieldRaces GetGameRealTimeBattlefieldRaces()
	{
		return UnpackNextGamePacket<GameRealTimeBattlefieldRaces>(31);
	}

	public PlayerRealTimeBattlefieldRaces GetPlayerRealTimeBattlefieldRaces()
	{
		return UnpackNextGamePacket<PlayerRealTimeBattlefieldRaces>(36);
	}

	public BattlegroundsRatingChange GetBattlegroundsRatingChange()
	{
		return UnpackNextGamePacket<BattlegroundsRatingChange>(34);
	}

	public void SendPVPDRSessionInfoRequest()
	{
		SendUtilPacket(376, UtilSystemId.CLIENT, new PVPDRSessionInfoRequest());
	}

	public PVPDRSessionInfoResponse GetPVPDRSessionInfoResponse()
	{
		return UnpackNextUtilPacket<PVPDRSessionInfoResponse>(377);
	}

	public GameGuardianVars GetGameGuardianVars()
	{
		return UnpackNextGamePacket<GameGuardianVars>(35);
	}

	public UpdateBattlegroundInfo GetBattlegroundInfo()
	{
		return UnpackNextGamePacket<UpdateBattlegroundInfo>(53);
	}

	public GetBattlegroundHeroArmorTierList GetBattlegroundHeroArmorTierList()
	{
		return UnpackNextGamePacket<GetBattlegroundHeroArmorTierList>(54);
	}

	public GetBattlegroundsPlayerAnomaly GetBattlegroundsPlayerAnomaly()
	{
		return UnpackNextGamePacket<GetBattlegroundsPlayerAnomaly>(61);
	}

	public TeammatesEntities GetTeammateEntities()
	{
		return UnpackNextGamePacket<TeammatesEntities>(37);
	}

	public TeammatesChooseEntities GetTeammatesChooseEntities()
	{
		return UnpackNextGamePacket<TeammatesChooseEntities>(38);
	}

	public TeammatesEntitiesChosen GetTeammatesEntitiesChosen()
	{
		return UnpackNextGamePacket<TeammatesEntitiesChosen>(39);
	}

	public TeammateConcede GetTeammateConcede()
	{
		return UnpackNextGamePacket<TeammateConcede>(59);
	}

	public EntityPinged GetEntityPinged()
	{
		return UnpackNextGamePacket<EntityPinged>(41);
	}

	public ReplaceBattlegroundMulliganHero ReplaceBattlegroundMulliganHero()
	{
		return UnpackNextGamePacket<ReplaceBattlegroundMulliganHero>(65);
	}

	public DebugMessage GetDebugMessage()
	{
		return UnpackNextGamePacket<DebugMessage>(5);
	}

	public ScriptLogMessage GetScriptLogMessage()
	{
		return UnpackNextGamePacket<ScriptLogMessage>(50);
	}

	public SubscribeResponse GetSubscribeResponse()
	{
		return UnpackNextUtilPacket<SubscribeResponse>(315);
	}

	public TavernBrawlInfo GetTavernBrawlInfo()
	{
		return UnpackNextUtilPacket<TavernBrawlInfo>(316);
	}

	public TavernBrawlPlayerRecordResponse GeTavernBrawlPlayerRecordResponse()
	{
		return UnpackNextUtilPacket<TavernBrawlPlayerRecordResponse>(317);
	}

	public TriggerEventResponse GetTriggerEventResponse()
	{
		return UnpackNextUtilPacket<TriggerEventResponse>(299);
	}

	public PegasusGame.TurnTimer GetTurnTimerInfo()
	{
		return UnpackNextGamePacket<PegasusGame.TurnTimer>(9);
	}

	public UpdateAccountLicensesResponse GetUpdateAccountLicensesResponse()
	{
		return UnpackNextUtilPacket<UpdateAccountLicensesResponse>(331);
	}

	public UserUI GetUserUi()
	{
		return UnpackNextGamePacket<UserUI>(15);
	}

	public ValidateAchieveResponse GetValidateAchieveResponse()
	{
		return UnpackNextUtilPacket<ValidateAchieveResponse>(285);
	}

	public CosmeticCoins GetCoins()
	{
		return UnpackNextUtilPacket<CosmeticCoins>(608);
	}

	public void SetFavoriteCosmeticCoin(int coin, bool isFavorite)
	{
		SendUtilPacket(609, UtilSystemId.CLIENT, new SetFavoriteCosmeticCoin
		{
			Coin = coin,
			IsFavorite = isFavorite
		});
	}

	public AchievementProgress GetAchievementInGameProgress()
	{
		return UnpackNextGamePacket<AchievementProgress>(52);
	}

	public AchievementComplete GetAchievementComplete()
	{
		return UnpackNextUtilPacket<AchievementComplete>(618);
	}

	public SetFavoriteCosmeticCoinResponse GetSetFavoriteCosmeticCoinResponse()
	{
		return UnpackNextUtilPacket<SetFavoriteCosmeticCoinResponse>(610);
	}

	public CoinUpdate GetCoinUpdate()
	{
		return UnpackNextUtilPacket<CoinUpdate>(611);
	}

	public void SendClientScriptGameEvent(ClientScriptGameEventType eventType, int data)
	{
		SendGamePacket(58, new ClientScriptGameEvent
		{
			EventType = (int)eventType,
			Data = data
		});
	}

	public void GotoGameServer(string address, uint port)
	{
		dispatcherImpl.ConnectToGameServer(address, port);
	}

	public void RegisterGameServerConnectEventListener(Action<BattleNetErrors, SocketOperationResult> listener)
	{
		dispatcherImpl.OnGameServerConnectEvent += listener;
	}

	public void RemoveGameServerConnectEventListener(Action<BattleNetErrors, SocketOperationResult> listener)
	{
		dispatcherImpl.OnGameServerConnectEvent -= listener;
	}

	public void RegisterGameServerDisconnectEventListener(Action<BattleNetErrors, SocketOperationResult> listener)
	{
		dispatcherImpl.OnGameServerDisconnectEvent += listener;
	}

	public void RemoveGameServerDisconnectEventListener(Action<BattleNetErrors, SocketOperationResult> listener)
	{
		dispatcherImpl.OnGameServerDisconnectEvent -= listener;
	}

	public void RegisterIPv6ConversionEventListener(Action<string, string> listener)
	{
		dispatcherImpl.OnIPv6ConversionEvent += listener;
	}

	public void RemoveIPv6ConversionEventListener(Action<string, string> listener)
	{
		dispatcherImpl.OnIPv6ConversionEvent -= listener;
	}

	public void SendSpectatorGameHandshake(string version, Platform platform, GameServerInfo info, BnetId bnetId)
	{
		SendGamePacket(22, new SpectatorHandshake
		{
			GameHandle = (int)info.GameHandle,
			Password = info.SpectatorPassword,
			Version = version,
			Platform = platform,
			GameAccountId = bnetId
		});
	}

	public void SendGameHandshake(GameServerInfo info, Platform platform)
	{
		SendGamePacket(168, new Handshake
		{
			Password = info.AuroraPassword,
			GameHandle = (int)info.GameHandle,
			ClientHandle = (int)info.ClientHandle,
			Mission = info.Mission,
			Version = info.Version,
			Platform = platform
		});
	}

	public bool HasErrors()
	{
		return dispatcherImpl.HasUtilErrors();
	}

	public bool HasDebugPackets()
	{
		return m_debugConnectionManager.HaveDebugPackets();
	}

	public bool HasGamePackets()
	{
		return dispatcherImpl.HasGamePackets();
	}

	public bool HasUtilPackets()
	{
		return dispatcherImpl.HasUtilPackets();
	}

	public bool IsConnectedToGameServer()
	{
		return dispatcherImpl.IsConnectedToGameServer();
	}

	public void ProcessUtilPackets()
	{
		dispatcherImpl.ProcessUtilPackets();
	}

	public bool TryConnectDebugConsole()
	{
		return m_debugConnectionManager.TryConnectDebugConsole();
	}

	public void UpdateDebugConsole()
	{
		m_debugConnectionManager.Update();
	}

	public void MassDisenchant()
	{
		SendUtilPacket(268, UtilSystemId.COLLECTION, new MassDisenchantRequest());
	}

	public int NextDebugPacketType()
	{
		return m_debugConnectionManager.NextDebugConsoleType();
	}

	public int NextGamePacketType()
	{
		return dispatcherImpl.NextGameType();
	}

	public int NextUtilPacketType()
	{
		return dispatcherImpl.NextUtilType();
	}

	public void OnLoginComplete()
	{
		dispatcherImpl.OnLoginComplete();
	}

	public void OnLoginStarted()
	{
		m_debugConnectionManager.OnLoginStarted();
	}

	public void OnStartupPacketSequenceComplete()
	{
		dispatcherImpl.OnStartupPacketSequenceComplete();
	}

	public void OpenBooster(int boosterTypeId, int numPacks)
	{
		SendUtilPacket(225, UtilSystemId.COLLECTION, new OpenBooster
		{
			BoosterType = boosterTypeId,
			NumPacks = numPacks
		});
	}

	public void PurchaseViaGold(int quantity, ProductType product, int data)
	{
		SendUtilPacket(279, UtilSystemId.CLIENT, new PurchaseWithGold
		{
			Product = product,
			Quantity = quantity,
			Data = data
		});
	}

	public void RenameDeck(long deckId, string name, DeckType deckType, DeckSourceType sourceType)
	{
		SendUtilPacket(211, UtilSystemId.CLIENT, new RenameDeck
		{
			Deck = deckId,
			Name = name,
			HasDeckType = (deckType != DeckType.UNKNOWN_DECK_TYPE),
			DeckType = deckType,
			HasSourceType = (sourceType != DeckSourceType.DECK_SOURCE_TYPE_UNKNOWN),
			SourceType = sourceType
		});
	}

	public void RequestAccountLicensesUpdate()
	{
		SendUtilPacket(276, UtilSystemId.BATTLEPAY, new UpdateAccountLicenses());
	}

	public void RequestAdventureProgress()
	{
		SendUtilPacket(305, UtilSystemId.CLIENT, new GetAdventureProgress());
	}

	public void RequestCollectionClientState(Platform platform, long cachedCollectionVersion, List<DeckModificationTimes> deckTimes, long cachedCollectionVersionLastModified)
	{
		SendUtilPacket(711, UtilSystemId.CLIENT, new CollectionClientStateRequest
		{
			Platform = platform,
			ClientCollectionVersion = cachedCollectionVersion,
			CachedDeckModificationTimes = deckTimes,
			CollectionVersionLastModified = cachedCollectionVersionLastModified
		}, RequestPhase.STARTUP);
	}

	public void RequestSecondaryClientState(Platform platform, List<DeckModificationTimes> deckTimes)
	{
		SendUtilPacket(712, UtilSystemId.CLIENT, new SecondaryClientStateRequest
		{
			Platform = platform,
			CachedDeckModificationTimes = deckTimes
		}, RequestPhase.STARTUP);
	}

	public void RequestCancelQuest(int questId)
	{
		SendUtilPacket(281, UtilSystemId.CLIENT, new CancelQuest
		{
			QuestId = questId
		});
	}

	public void RequestDeckContents(long[] deckIds)
	{
		GetDeckContents packet = new GetDeckContents();
		packet.DeckId.AddRange(deckIds);
		SendUtilPacket(214, UtilSystemId.CLIENT, packet);
	}

	public void RequestAccountInfoNetCacheObject(GetAccountInfo.Request request)
	{
		SendUtilPacket(201, UtilSystemId.CLIENT, new GetAccountInfo
		{
			Request_ = request
		});
	}

	public void RequestNetCacheObjectList(List<GetAccountInfo.Request> requests, List<GenericRequest> genericRequests)
	{
		GenericRequestList requestList = new GenericRequestList();
		foreach (GetAccountInfo.Request request in requests)
		{
			requestList.Requests.Add(new GenericRequest
			{
				RequestId = 201,
				RequestSubId = (int)request
			});
		}
		if (genericRequests != null)
		{
			foreach (GenericRequest request2 in genericRequests)
			{
				requestList.Requests.Add(request2);
			}
		}
		SendUtilPacket(327, UtilSystemId.CLIENT, requestList);
	}

	public void RequestProcessRecruitAFriend()
	{
		SendUtilPacket(339, UtilSystemId.RECRUIT_A_FRIEND, new ProcessRecruitAFriend());
	}

	public void RequestPurchaseMethod(long? pmtProductId, int quantity, PegasusShared.Currency currency, string deviceId, Platform platform)
	{
		SendUtilPacket(250, UtilSystemId.BATTLEPAY, new GetPurchaseMethod
		{
			PmtProductId = pmtProductId.GetValueOrDefault(),
			Quantity = quantity,
			CurrencyCode = currency.Code,
			DeviceId = deviceId,
			Platform = platform
		});
	}

	public void RequestRecruitAFriendData()
	{
		SendUtilPacket(337, UtilSystemId.RECRUIT_A_FRIEND, new GetRecruitAFriendData());
	}

	public void RequestRecruitAFriendUrl(Platform platform)
	{
		SendUtilPacket(335, UtilSystemId.RECRUIT_A_FRIEND, new GetRecruitAFriendURL
		{
			Platform = platform
		});
	}

	public void CraftingTransaction(CraftingPendingTransaction transaction, PegasusShared.CardDef cardDef, int expectedTransactionCost, int normalOwned, int goldenOwned, int signatureOwned, int diamondOwned)
	{
		CraftingTransaction packet = new CraftingTransaction
		{
			Def = cardDef,
			ExpectedDustDelta = expectedTransactionCost,
			ExpectedNormalOwnedCount = normalOwned,
			ExpectedGoldenOwnedCount = goldenOwned,
			ExpectedSignatureOwnedCount = signatureOwned,
			ExpectedDiamondOwnedCount = diamondOwned,
			NormalDisenchantCount = transaction.NormalDisenchantCount,
			NormalCreateCount = transaction.NormalCreateCount,
			GoldenDisenchantCount = transaction.GoldenDisenchantCount,
			GoldenCreateCount = transaction.GoldenCreateCount,
			GoldenUpgradeFromNormalCount = transaction.GoldenUpgradeFromNormalCount,
			GoldenUpgradeFromNothingCount = transaction.GoldenUpgradeFromNothingCount,
			SignatureDisenchantCount = transaction.SignatureDisenchantCount,
			DiamondDisenchantCount = transaction.DiamondDisenchantCount
		};
		SendUtilPacket(392, UtilSystemId.COLLECTION, packet);
	}

	public void SendAssetRequest(int clientToken, List<AssetKey> requestKeys)
	{
		while (requestKeys.Count > 0)
		{
			int keysToMove = Math.Min(requestKeys.Count, 100);
			List<AssetKey> keysToSend = requestKeys.GetRange(0, keysToMove);
			requestKeys.RemoveRange(0, keysToMove);
			GetAssetRequest packet = new GetAssetRequest
			{
				ClientToken = clientToken,
				Requests = keysToSend
			};
			SendUtilPacket(321, UtilSystemId.CLIENT, packet);
		}
	}

	public void SendMulliganChooseOneTentativeSelect(int entityId, bool isConfirmation)
	{
		SendGamePacket(43, new MulliganChooseOneTentativeSelect
		{
			EntityID = entityId,
			IsConfirmation = isConfirmation
		});
	}

	public void SendChoices(int choicesId, List<int> picks)
	{
		SendGamePacket(3, new ChooseEntities
		{
			Id = choicesId,
			Entities = picks
		});
	}

	public void SendRequestTeammatesGamestate()
	{
		SendGamePacket(42, new RequestTeammatesGamestate());
	}

	public void SendPingTeammateEntity(int entityID, int pingType, bool teammateOwned)
	{
		SendGamePacket(40, new PingTeammateEntity
		{
			EntityID = entityID,
			PingType = pingType,
			TeammateOwned = teammateOwned
		});
	}

	public void SendNotifyTeammateChooseOne(List<int> subCardIDs)
	{
		SendGamePacket(45, new NotifyTeammateChooseOne
		{
			EntityIDs = subCardIDs
		});
	}

	public void SendPreRefreshBGHeroes()
	{
		SendGamePacket(63, new PreRefreshBGHeroes());
	}

	public void SendDebugCommandRequest(DebugCommandRequest packet)
	{
		SendUtilPacket(323, UtilSystemId.CLIENT, packet);
	}

	public void SendDebugConsoleResponse(int responseType, string message)
	{
		if (m_debugConnectionManager.IsActive())
		{
			m_debugConnectionManager.SendDebugConsoleResponse(responseType, message);
		}
	}

	public void SendDeckData(DeckSetData packet)
	{
		SendUtilPacket(222, UtilSystemId.CLIENT, packet);
	}

	public void SendDeckTemplateSource(long deckId, int templateId)
	{
		SendUtilPacket(332, UtilSystemId.CLIENT, new DeckSetTemplateSource
		{
			Deck = deckId,
			TemplateId = templateId
		});
	}

	public void SendFreeDeckChoice(int deckTemplateId, bool bypassTrialPeriod)
	{
		SendUtilPacket(333, UtilSystemId.CLIENT, new FreeDeckChoice
		{
			DeckTemplateId = deckTemplateId,
			BypassTrialPeriod = bypassTrialPeriod
		});
	}

	public void SendSmartDeckRequest(SmartDeckRequest packet)
	{
		SendUtilPacket(369, UtilSystemId.COLLECTION, packet);
	}

	public void SendEmote(int emoteId)
	{
		SendGamePacket(15, new UserUI
		{
			Emote = emoteId
		});
	}

	public void SendBattlegroundsEmote(int emoteId, int battlegroundsEmoteId)
	{
		SendGamePacket(15, new UserUI
		{
			Emote = emoteId,
			BattlegroundsEmoteId = battlegroundsEmoteId
		});
	}

	public void SendSelection(int selectedEntityId)
	{
		SendGamePacket(15, new UserUI
		{
			SelectedEntityId = selectedEntityId
		});
	}

	public bool AllowDebugConnections()
	{
		return m_debugConnectionManager.AllowDebugConnections();
	}

	public void SendDebugConsoleCommand(string command)
	{
		SendGamePacket(123, new DebugConsoleCommand
		{
			Command = command
		});
	}

	public void SendOption(int choiceId, int index, int target, int subOption, int position)
	{
		SendGamePacket(2, new ChooseOption
		{
			Id = choiceId,
			Index = index,
			Target = target,
			SubOption = subOption,
			Position = position
		});
	}

	public void SendPing()
	{
		SendGamePacket(115, new Ping());
	}

	public void SendRemoveSpectators(bool regeneratePassword, List<BnetId> spectators)
	{
		SendGamePacket(26, new RemoveSpectators
		{
			RegenerateSpectatorPassword = regeneratePassword,
			TargetGameaccountIds = spectators
		});
	}

	public void SendUnsubscribeRequest(Unsubscribe packet, UtilSystemId systemChannel)
	{
		SendUtilPacket(329, systemChannel, packet);
	}

	public void SendUserUi(int overCard, int heldCard, int arrowOrigin, int x, int y)
	{
		SendGamePacket(15, new UserUI
		{
			MouseInfo = new MouseInfo
			{
				ArrowOrigin = arrowOrigin,
				OverCard = overCard,
				HeldCard = heldCard,
				X = x,
				Y = y
			}
		});
	}

	public void SendUserUi(int overCard, int heldCard, int arrowOrigin, int x, int y, bool useTeammatesGamestate)
	{
		SendGamePacket(15, new UserUI
		{
			MouseInfo = new MouseInfo
			{
				ArrowOrigin = arrowOrigin,
				OverCard = overCard,
				HeldCard = heldCard,
				X = x,
				Y = y,
				UseTeammatesGamestate = useTeammatesGamestate
			}
		});
	}

	public void SetClientOptions(SetOptions packet)
	{
		SendUtilPacket(239, UtilSystemId.CLIENT, packet);
	}

	public void SetFavoriteCardBack(int cardBack, bool isFavorite)
	{
		SendUtilPacket(291, UtilSystemId.CLIENT, new SetFavoriteCardBack
		{
			CardBack = cardBack,
			IsFavorite = isFavorite
		});
	}

	public void SetDisconnectedFromBattleNet()
	{
		dispatcherImpl.SetDisconnectedFromBattleNet();
	}

	public void SetFavoriteHero(int classId, PegasusShared.CardDef heroCardDef, bool isFavorite)
	{
		SendUtilPacket(319, UtilSystemId.CLIENT, new SetFavoriteHero
		{
			FavoriteHero = new FavoriteHero
			{
				ClassId = classId,
				Hero = heroCardDef
			},
			IsFavorite = isFavorite
		});
	}

	public void SetProgress(long value)
	{
		SendUtilPacket(230, UtilSystemId.CLIENT, new SetProgress
		{
			Value = value
		}, RequestPhase.STARTUP);
	}

	public bool ShouldIgnoreError(BnetErrorInfo errorInfo)
	{
		return dispatcherImpl.ShouldIgnoreError(errorInfo);
	}

	public double GetTimeLastPingReceieved()
	{
		return dispatcherImpl.TimeLastPingReceived;
	}

	public void SetTimeLastPingReceived(double time)
	{
		dispatcherImpl.TimeLastPingReceived = time;
	}

	public double GetTimeLastPingSent()
	{
		return dispatcherImpl.TimeLastPingSent;
	}

	public void SetTimeLastPingSent(double time)
	{
		dispatcherImpl.TimeLastPingSent = time;
	}

	public void SetShouldIgnorePong(bool value)
	{
		dispatcherImpl.ShouldIgnorePong = value;
	}

	public void SetSpoofDisconnected(bool value)
	{
		dispatcherImpl.SpoofDisconnected = value;
	}

	public void SetPingsSinceLastPong(int value)
	{
		dispatcherImpl.PingsSinceLastPong = value;
	}

	public int GetPingsSinceLastPong()
	{
		return dispatcherImpl.PingsSinceLastPong;
	}

	public void ValidateAchieve(int achieveId)
	{
		SendUtilPacket(284, UtilSystemId.CLIENT, new ValidateAchieve
		{
			Achieve = achieveId
		});
	}

	public void RequestTavernBrawlSessionBegin()
	{
		SendUtilPacket(343, UtilSystemId.CLIENT, new TavernBrawlRequestSessionBegin());
	}

	public void TavernBrawlRetire()
	{
		SendUtilPacket(344, UtilSystemId.CLIENT, new TavernBrawlRequestSessionRetire());
	}

	public void AckTavernBrawlSessionRewards()
	{
		SendUtilPacket(345, UtilSystemId.CLIENT, new TavernBrawlAckSessionRewards());
	}

	public TavernBrawlRequestSessionBeginResponse GetTavernBrawlSessionBeginResponse()
	{
		return UnpackNextUtilPacket<TavernBrawlRequestSessionBeginResponse>(347);
	}

	public TavernBrawlRequestSessionRetireResponse GetTavernBrawlSessionRetired()
	{
		return UnpackNextUtilPacket<TavernBrawlRequestSessionRetireResponse>(348);
	}

	public void RequestTavernBrawlInfo(BrawlType brawlType)
	{
		RequestTavernBrawlInfo packet = new RequestTavernBrawlInfo
		{
			BrawlType = brawlType
		};
		SendUtilPacket(352, UtilSystemId.CLIENT, packet);
	}

	public void RequestTavernBrawlPlayerRecord(BrawlType brawlType)
	{
		RequestTavernBrawlPlayerRecord packet = new RequestTavernBrawlPlayerRecord
		{
			BrawlType = brawlType
		};
		SendUtilPacket(353, UtilSystemId.CLIENT, packet);
	}

	public void DraftRequestDisablePremiums()
	{
		SendUtilPacket(354, UtilSystemId.CLIENT, new DraftRequestRemovePremiums());
	}

	public void RequestSkipApprentice()
	{
		SendUtilPacket(659, UtilSystemId.CLIENT, new SkipApprenticeRequest());
	}

	public void UnlockBattlegroundsDuringApprentice()
	{
		SendUtilPacket(710, UtilSystemId.CLIENT, new UnlockBattlegrounds());
	}

	public void GetCustomizedCardHistoryRequest(uint maximumCount)
	{
		SendUtilPacket(707, UtilSystemId.CLIENT, new GetCustomizedCardHistoryRequest
		{
			MaximumCount = maximumCount
		});
	}

	public GetCustomizedCardHistoryResponse CustomizedCardHistoryResponse()
	{
		return UnpackNextUtilPacket<GetCustomizedCardHistoryResponse>(708);
	}

	public void UpdateCustomizedCard(CustomizedCard customizedCard)
	{
		SendUtilPacket(709, UtilSystemId.CLIENT, new UpdateCustomizedCardHistoryRequest
		{
			Card = customizedCard
		});
	}

	public void MercenariesTrainingAddRequest(int mercenaryID)
	{
		SendUtilPacket(10154, UtilSystemId.LETTUCE, new MercenariesTrainingAddRequest
		{
			MercenaryId = mercenaryID
		});
	}

	public void MercenariesTrainingRemoveRequest(int mercenaryID)
	{
		SendUtilPacket(10157, UtilSystemId.LETTUCE, new MercenariesTrainingRemoveRequest
		{
			MercenaryId = mercenaryID
		});
	}

	public void MercenariesTrainingCollectRequest(int mercenaryID)
	{
		SendUtilPacket(10156, UtilSystemId.CLIENT, new MercenariesTrainingCollectRequest
		{
			MercenaryId = mercenaryID
		});
	}

	public SkipApprenticeResponse GetSkipApprenticeResponse()
	{
		return UnpackNextUtilPacket<SkipApprenticeResponse>(660);
	}

	public SmartDeckResponse GetSmartDeckResponse()
	{
		return UnpackNextUtilPacket<SmartDeckResponse>(370);
	}

	public MercenariesPlayerInfoResponse MercenariesPlayerInfoResponse()
	{
		return UnpackNextUtilPacket<MercenariesPlayerInfoResponse>(10102);
	}

	public MercenariesCollectionResponse MercenariesCollectionResponse()
	{
		return UnpackNextUtilPacket<MercenariesCollectionResponse>(10104);
	}

	public MercenariesCollectionUpdate MercenariesCollectionUpdate()
	{
		return UnpackNextUtilPacket<MercenariesCollectionUpdate>(10105);
	}

	public MercenariesCurrencyUpdate MercenariesCurrencyUpdate()
	{
		return UnpackNextUtilPacket<MercenariesCurrencyUpdate>(10106);
	}

	public MercenariesExperienceUpdate MercenariesExperienceUpdate()
	{
		return UnpackNextUtilPacket<MercenariesExperienceUpdate>(10107);
	}

	public MercenariesRewardUpdate MercenariesRewardUpdate()
	{
		return UnpackNextUtilPacket<MercenariesRewardUpdate>(10109);
	}

	public MercenariesTeamListResponse MercenariesTeamListResponse()
	{
		return UnpackNextUtilPacket<MercenariesTeamListResponse>(10111);
	}

	public CreateMercenariesTeamResponse CreateMercenariesTeamResponse()
	{
		return UnpackNextUtilPacket<CreateMercenariesTeamResponse>(10113);
	}

	public UpdateMercenariesTeamResponse UpdateMercenariesTeamResponse()
	{
		return UnpackNextUtilPacket<UpdateMercenariesTeamResponse>(10115);
	}

	public DeleteMercenariesTeamResponse DeleteMercenariesTeamResponse()
	{
		return UnpackNextUtilPacket<DeleteMercenariesTeamResponse>(10117);
	}

	public UpdateMercenariesTeamNameResponse UpdateMercenariesTeamNameResponse()
	{
		return UnpackNextUtilPacket<UpdateMercenariesTeamNameResponse>(10191);
	}

	public UpdateEquippedMercenaryEquipmentResponse UpdateEquippedMercenaryEquipmentResponse()
	{
		return UnpackNextUtilPacket<UpdateEquippedMercenaryEquipmentResponse>(10121);
	}

	public CraftMercenaryResponse CraftMercenaryResponse()
	{
		return UnpackNextUtilPacket<CraftMercenaryResponse>(10123);
	}

	public UpgradeMercenaryAbilityResponse UpgradeMercenaryAbilityResponse()
	{
		return UnpackNextUtilPacket<UpgradeMercenaryAbilityResponse>(10125);
	}

	public CraftMercenaryEquipmentResponse CraftMercenaryEquipmentResponse()
	{
		return UnpackNextUtilPacket<CraftMercenaryEquipmentResponse>(10127);
	}

	public UpgradeMercenaryEquipmentResponse UpgradeMercenaryEquipmentResponse()
	{
		return UnpackNextUtilPacket<UpgradeMercenaryEquipmentResponse>(10129);
	}

	public OpenMercenariesPackResponse OpenMercenariesPackResponse()
	{
		return UnpackNextUtilPacket<OpenMercenariesPackResponse>(10134);
	}

	public LettuceMapResponse GetLettuceMapResponse()
	{
		return UnpackNextUtilPacket<LettuceMapResponse>(10136);
	}

	public LettuceMapChooseNodeResponse GetLettuceMapChooseNodeResponse()
	{
		return UnpackNextUtilPacket<LettuceMapChooseNodeResponse>(10138);
	}

	public LettuceMapRetireResponse GetLettuceMapRetireResponse()
	{
		return UnpackNextUtilPacket<LettuceMapRetireResponse>(10140);
	}

	public MercenariesPvPRatingUpdate MercenariesPvPRatingUpdate()
	{
		return UnpackNextUtilPacket<MercenariesPvPRatingUpdate>(10141);
	}

	public MercenariesMapTreasureSelectionResponse GetMercenariesMapTreasureSelectionResponse()
	{
		return UnpackNextUtilPacket<MercenariesMapTreasureSelectionResponse>(10144);
	}

	public MercenariesMapVisitorSelectionResponse GetMercenariesMapVisitorSelectionResponse()
	{
		return UnpackNextUtilPacket<MercenariesMapVisitorSelectionResponse>(10167);
	}

	public UpdateEquippedMercenaryPortraitResponse GetUpdateEquippedMercenaryPortraitResponse()
	{
		return UnpackNextUtilPacket<UpdateEquippedMercenaryPortraitResponse>(10119);
	}

	public MercenariesPvPWinUpdate MercenariesPvPWinUpdate()
	{
		return UnpackNextUtilPacket<MercenariesPvPWinUpdate>(10142);
	}

	public MercenariesPlayerBountyInfoUpdate MercenariesPlayerBountyInfoUpdate()
	{
		return UnpackNextUtilPacket<MercenariesPlayerBountyInfoUpdate>(10145);
	}

	public MercenariesTeamUpdate MercenariesTeamUpdate()
	{
		return UnpackNextUtilPacket<MercenariesTeamUpdate>(10168);
	}

	public MercenariesTrainingAddResponse MercenariesTrainingAddResponse()
	{
		return UnpackNextUtilPacket<MercenariesTrainingAddResponse>(10155);
	}

	public MercenariesTrainingRemoveResponse MercenariesTrainingRemoveResponse()
	{
		return UnpackNextUtilPacket<MercenariesTrainingRemoveResponse>(10158);
	}

	public MercenariesTrainingCollectResponse MercenariesTrainingCollectResponse()
	{
		return UnpackNextUtilPacket<MercenariesTrainingCollectResponse>(10157);
	}

	public MercenariesMythicTreasureScalarsResponse MercenariesMythicTreasureScalarsResponse()
	{
		return UnpackNextUtilPacket<MercenariesMythicTreasureScalarsResponse>(10182);
	}

	public MercenariesMythicTreasureScalarPurchaseResponse MercenariesMythicTreasureScalarPurchaseResponse()
	{
		return UnpackNextUtilPacket<MercenariesMythicTreasureScalarPurchaseResponse>(10184);
	}

	public MercenariesMythicUpgradeAbilityResponse MercenariesMythicUpgradeAbilityResponse()
	{
		return UnpackNextUtilPacket<MercenariesMythicUpgradeAbilityResponse>(10186);
	}

	public MercenariesMythicUpgradeEquipmentResponse MercenariesMythicUpgradeEquipmentResponse()
	{
		return UnpackNextUtilPacket<MercenariesMythicUpgradeEquipmentResponse>(10188);
	}

	public void RequestGameSaveData(List<long> keys, int clientToken)
	{
		SendUtilPacket(357, UtilSystemId.CLIENT, new GameSaveDataRequest
		{
			KeyIds = keys,
			ClientToken = clientToken
		});
	}

	public GameSaveDataResponse GetGameSaveDataResponse()
	{
		return UnpackNextUtilPacket<GameSaveDataResponse>(358);
	}

	public void SetGameSaveData(List<GameSaveDataUpdate> dataUpdates, int clientToken)
	{
		SendUtilPacket(359, UtilSystemId.CLIENT, new SetGameSaveData
		{
			Data = dataUpdates,
			ClientToken = clientToken
		});
	}

	public SetGameSaveDataResponse GetSetGameSaveDataResponse()
	{
		return UnpackNextUtilPacket<SetGameSaveDataResponse>(360);
	}

	public GameSaveDataStateUpdate GetGameSaveDataStateUpdate()
	{
		return UnpackNextUtilPacket<GameSaveDataStateUpdate>(391);
	}

	public void SendLocateCheatServerRequest()
	{
		SendUtilPacket(361, UtilSystemId.CHEAT, new LocateCheatServerRequest());
	}

	public LocateCheatServerResponse GetLocateCheatServerResponse()
	{
		return UnpackNextUtilPacket<LocateCheatServerResponse>(362);
	}

	public GameToConnectNotification GetGameToConnectNotification()
	{
		return UnpackNextUtilPacket<GameToConnectNotification>(363);
	}

	public void GetServerTimeRequest(long now)
	{
		SendUtilPacket(364, UtilSystemId.CLIENT, new GetServerTimeRequest
		{
			ClientUnixTime = now
		});
	}

	public ResponseWithRequest<GetServerTimeResponse, GetServerTimeRequest> GetServerTimeResponse()
	{
		return UnpackNextUtilPacketWithRequest<GetServerTimeResponse, GetServerTimeRequest>(365);
	}

	public void RequestBaconRatingInfo()
	{
		SendUtilPacket(372, UtilSystemId.CLIENT, new BattlegroundsRatingInfoRequest());
	}

	public ResponseWithRequest<BattlegroundsRatingInfoResponse, BattlegroundsRatingInfoRequest> BattlegroundsRatingInfoResponse()
	{
		return UnpackNextUtilPacketWithRequest<BattlegroundsRatingInfoResponse, BattlegroundsRatingInfoRequest>(373);
	}

	public ResponseWithRequest<PVPDRStatsInfoResponse, PVPDRStatsInfoRequest> PVPDRStatsInfoResponse()
	{
		return UnpackNextUtilPacketWithRequest<PVPDRStatsInfoResponse, PVPDRStatsInfoRequest>(379);
	}

	public BattlegroundsHeroSkinsResponse GetBattlegroundsHeroSkinsResponse()
	{
		return UnpackNextUtilPacket<BattlegroundsHeroSkinsResponse>(621);
	}

	public SetBattlegroundsFavoriteHeroSkinResponse GetSetBattlegroundsFavoriteHeroSkinResponse()
	{
		return UnpackNextUtilPacket<SetBattlegroundsFavoriteHeroSkinResponse>(623);
	}

	public void SetBattlegroundsFavoriteHeroSkin(int baseHeroCardId, int battlegroundsHeroSkinId)
	{
		SendUtilPacket(622, UtilSystemId.CLIENT, new SetBattlegroundsFavoriteHeroSkin
		{
			HeroSkinId = battlegroundsHeroSkinId,
			BaseHeroCardId = baseHeroCardId
		});
	}

	public ClearBattlegroundsFavoriteHeroSkinResponse GetClearBattlegroundsFavoriteHeroSkinResponse()
	{
		return UnpackNextUtilPacket<ClearBattlegroundsFavoriteHeroSkinResponse>(625);
	}

	public void ClearBattlegroundsFavoriteHeroSkin(int baseHeroCardId, int battlegroundsHeroSkinId)
	{
		SendUtilPacket(624, UtilSystemId.CLIENT, new ClearBattlegroundsFavoriteHeroSkin
		{
			HeroSkinId = battlegroundsHeroSkinId,
			BaseHeroCardId = baseHeroCardId
		});
	}

	public BattlegroundsGuideSkinsResponse GetBattlegroundsGuideSkinsResponse()
	{
		return UnpackNextUtilPacket<BattlegroundsGuideSkinsResponse>(626);
	}

	public SetBattlegroundsFavoriteGuideSkinResponse GetSetBattlegroundsFavoriteGuideSkinResponse()
	{
		return UnpackNextUtilPacket<SetBattlegroundsFavoriteGuideSkinResponse>(628);
	}

	public void SetBattlegroundsFavoriteGuideSkin(int favoriteSkinID)
	{
		SendUtilPacket(627, UtilSystemId.CLIENT, new SetBattlegroundsFavoriteGuideSkin
		{
			GuideSkinId = favoriteSkinID
		});
	}

	public ClearBattlegroundsFavoriteGuideSkinResponse GetClearBattlegroundsFavoriteGuideSkinResponse()
	{
		return UnpackNextUtilPacket<ClearBattlegroundsFavoriteGuideSkinResponse>(630);
	}

	public void ClearBattlegroundsFavoriteGuideSkin()
	{
		SendUtilPacket(629, UtilSystemId.CLIENT, new ClearBattlegroundsFavoriteGuideSkin());
	}

	public BattlegroundsBoardSkinsResponse GetBattlegroundsBoardSkinsResponse()
	{
		return UnpackNextUtilPacket<BattlegroundsBoardSkinsResponse>(637);
	}

	public SetBattlegroundsFavoriteBoardSkinResponse GetSetBattlegroundsFavoriteBoardSkinResponse()
	{
		return UnpackNextUtilPacket<SetBattlegroundsFavoriteBoardSkinResponse>(639);
	}

	public void SetBattlegroundsFavoriteBoardSkin(int favoriteSkinID)
	{
		SendUtilPacket(638, UtilSystemId.CLIENT, new SetBattlegroundsFavoriteBoardSkin
		{
			BoardSkinId = favoriteSkinID
		});
	}

	public ClearBattlegroundsFavoriteBoardSkinResponse GetClearBattlegroundsFavoriteBoardSkinResponse()
	{
		return UnpackNextUtilPacket<ClearBattlegroundsFavoriteBoardSkinResponse>(641);
	}

	public void ClearBattlegroundsFavoriteBoardSkin(int bgFavoriteBoardSkinID)
	{
		SendUtilPacket(640, UtilSystemId.CLIENT, new ClearBattlegroundsFavoriteBoardSkin
		{
			BoardSkinId = bgFavoriteBoardSkinID
		});
	}

	public BattlegroundsFinishersResponse GetBattlegroundsFinishersResponse()
	{
		return UnpackNextUtilPacket<BattlegroundsFinishersResponse>(642);
	}

	public SetBattlegroundsFavoriteFinisherResponse GetSetBattlegroundsFavoriteFinisherResponse()
	{
		return UnpackNextUtilPacket<SetBattlegroundsFavoriteFinisherResponse>(644);
	}

	public void SetBattlegroundsFavoriteFinisher(int favoriteFinisherID)
	{
		SendUtilPacket(643, UtilSystemId.CLIENT, new SetBattlegroundsFavoriteFinisher
		{
			FinisherId = favoriteFinisherID
		});
	}

	public ClearBattlegroundsFavoriteFinisherResponse GetClearBattlegroundsFavoriteFinisherResponse()
	{
		return UnpackNextUtilPacket<ClearBattlegroundsFavoriteFinisherResponse>(646);
	}

	public void ClearBattlegroundsFavoriteFinisher(BattlegroundsFinisherId bgFavoriteFinisherID)
	{
		SendUtilPacket(645, UtilSystemId.CLIENT, new ClearBattlegroundsFavoriteFinisher
		{
			FinisherId = bgFavoriteFinisherID.ToValue()
		});
	}

	public BattlegroundsEmotesResponse GetBattlegroundsEmotesResponse()
	{
		return UnpackNextUtilPacket<BattlegroundsEmotesResponse>(649);
	}

	public SetBattlegroundsEmoteLoadoutResponse GetSetBattlegroundsEmoteLoadoutResponse()
	{
		return UnpackNextUtilPacket<SetBattlegroundsEmoteLoadoutResponse>(651);
	}

	public void SetBattlegroundsEmoteLoadout(Hearthstone.BattlegroundsEmoteLoadout loadout)
	{
		SendUtilPacket(650, UtilSystemId.CLIENT, new SetBattlegroundsEmoteLoadout
		{
			Loadout = loadout.ToNetwork()
		});
	}

	public void SendAckBattlegroundSkinsSeenPacket(AckBattlegroundsSkinsSeen packet)
	{
		SendUtilPacket(636, UtilSystemId.CLIENT, packet);
	}

	public void MercenariesPlayerInfoRequest(MercenariesPlayerInfoRequest request)
	{
		SendUtilPacket(10101, UtilSystemId.LETTUCE, request);
	}

	public void MercenariesCollectionRequest(MercenariesCollectionRequest request)
	{
		SendUtilPacket(10103, UtilSystemId.LETTUCE, request);
	}

	public void MercenariesTeamListRequest(MercenariesTeamListRequest request)
	{
		SendUtilPacket(10110, UtilSystemId.LETTUCE, request);
	}

	public void CreateMercenariesTeamRequest(CreateMercenariesTeamRequest request)
	{
		SendUtilPacket(10112, UtilSystemId.LETTUCE, request);
	}

	public void UpdateMercenariesTeamRequest(UpdateMercenariesTeamRequest request)
	{
		SendUtilPacket(10114, UtilSystemId.LETTUCE, request);
	}

	public void DeleteMercenariesTeamRequest(DeleteMercenariesTeamRequest request)
	{
		SendUtilPacket(10116, UtilSystemId.LETTUCE, request);
	}

	public void UpdateMercenariesTeamNameRequest(UpdateMercenariesTeamNameRequest request)
	{
		SendUtilPacket(10190, UtilSystemId.LETTUCE, request);
	}

	public void UpdateEquippedMercenaryEquipmentRequest(UpdateEquippedMercenaryEquipmentRequest request)
	{
		SendUtilPacket(10120, UtilSystemId.LETTUCE, request);
	}

	public void CraftMercenaryRequest(CraftMercenaryRequest request)
	{
		SendUtilPacket(10122, UtilSystemId.LETTUCE, request);
	}

	public void UpgradeMercenaryAbilityRequest(UpgradeMercenaryAbilityRequest request)
	{
		SendUtilPacket(10124, UtilSystemId.LETTUCE, request);
	}

	public void UpgradeMercenaryEquipmentRequest(UpgradeMercenaryEquipmentRequest request)
	{
		SendUtilPacket(10128, UtilSystemId.LETTUCE, request);
	}

	public void OpenMercenariesPackRequest(OpenMercenariesPackRequest request)
	{
		SendUtilPacket(10133, UtilSystemId.LETTUCE, request);
	}

	public void RequestLettuceMap(LettuceMapRequest request)
	{
		SendUtilPacket(10135, UtilSystemId.LETTUCE, request);
	}

	public void ChooseLettuceMapNode(LettuceMapChooseNodeRequest request)
	{
		SendUtilPacket(10137, UtilSystemId.LETTUCE, request);
	}

	public void RetireLettuceMap(LettuceMapRetireRequest request)
	{
		SendUtilPacket(10139, UtilSystemId.LETTUCE, request);
	}

	public void MakeMercenariesMapTreasureSelection(MercenariesMapTreasureSelectionRequest request)
	{
		SendUtilPacket(10143, UtilSystemId.LETTUCE, request);
	}

	public void MakeMercenariesMapVisitorSelection(MercenariesMapVisitorSelectionRequest request)
	{
		SendUtilPacket(10166, UtilSystemId.LETTUCE, request);
	}

	public void UpdateEquippedMercenaryPortraitRequest(UpdateEquippedMercenaryPortraitRequest request)
	{
		SendUtilPacket(10118, UtilSystemId.LETTUCE, request);
	}

	public void RequestMercenaryDebugCommand(MercenariesDebugCommandRequest request)
	{
		SendUtilPacket(10169, UtilSystemId.LETTUCE, request);
	}

	public MercenariesDebugCommandResponse MercenariesDebugCommandResponse()
	{
		return UnpackNextUtilPacket<MercenariesDebugCommandResponse>(10170);
	}

	public void MercenariesMythicTreasureScalarsRequest(MercenariesMythicTreasureScalarsRequest request)
	{
		SendUtilPacket(10181, UtilSystemId.LETTUCE, request);
	}

	public void MercenariesMythicTreasureScalarPurchaseRequest(MercenariesMythicTreasureScalarPurchaseRequest request)
	{
		SendUtilPacket(10183, UtilSystemId.LETTUCE, request);
	}

	public void MercenariesMythicUpgradeAbilityRequest(MercenariesMythicUpgradeAbilityRequest request)
	{
		SendUtilPacket(10185, UtilSystemId.LETTUCE, request);
	}

	public void MercenariesMythicUpgradeEquipmentRequest(MercenariesMythicUpgradeEquipmentRequest request)
	{
		SendUtilPacket(10187, UtilSystemId.LETTUCE, request);
	}

	public void RequestMercenaryVillageStatus(MercenariesGetVillageRequest request)
	{
		SendUtilPacket(10146, UtilSystemId.LETTUCE, request);
	}

	public MercenariesGetVillageResponse GetMercenaryVillageStatusResponse()
	{
		return UnpackNextUtilPacket<MercenariesGetVillageResponse>(10147);
	}

	public void RequestMercenaryVillageRefresh(MercenariesRefreshVillageRequest request)
	{
		SendUtilPacket(10162, UtilSystemId.LETTUCE, request);
	}

	public MercenariesRefreshVillageResponse GetMercenaryVillageRefreshResponse()
	{
		return UnpackNextUtilPacket<MercenariesRefreshVillageResponse>(10163);
	}

	public MercenariesVisitorStateUpdate GetMercenaryVisitorStateUpdate()
	{
		return UnpackNextUtilPacket<MercenariesVisitorStateUpdate>(10148);
	}

	public MercenariesBuildingStateUpdate GetMercenaryBuildingStateUpdate()
	{
		return UnpackNextUtilPacket<MercenariesBuildingStateUpdate>(10164);
	}

	public void RequestMercenaryBuildingUpgrade(MercenariesBuildingUpgradeRequest request)
	{
		SendUtilPacket(10150, UtilSystemId.LETTUCE, request);
	}

	public MercenariesBuildingUpgradeResponse GetMercenaryBuildingUpgradeResponse()
	{
		return UnpackNextUtilPacket<MercenariesBuildingUpgradeResponse>(10151);
	}

	public void RequestMercenaryClaimTask(MercenariesClaimTaskRequest request)
	{
		SendUtilPacket(10152, UtilSystemId.LETTUCE, request);
	}

	public MercenariesClaimTaskResponse GetMercenaryClaimTaskResponse()
	{
		return UnpackNextUtilPacket<MercenariesClaimTaskResponse>(10153);
	}

	public void RequestMercenaryDismissTask(MercenariesDismissTaskRequest request)
	{
		SendUtilPacket(10160, UtilSystemId.LETTUCE, request);
	}

	public MercenariesDismissTaskResponse GetMercenaryDismissTaskResponse()
	{
		return UnpackNextUtilPacket<MercenariesDismissTaskResponse>(10161);
	}

	public void RequestMercenaryBountyAcknowledge(MercenariesBountyAcknowledgeRequest request)
	{
		SendUtilPacket(10171, UtilSystemId.LETTUCE, request);
	}

	public MercenariesBountyAcknowledgeResponse GetMercenaryBountyAcknowledgeResponse()
	{
		return UnpackNextUtilPacket<MercenariesBountyAcknowledgeResponse>(10172);
	}

	public void RequestMercenaryCollectionAcknowledge(MercenariesCollectionAcknowledgeRequest request)
	{
		SendUtilPacket(10173, UtilSystemId.LETTUCE, request);
	}

	public MercenariesCollectionAcknowledgeResponse GetMercenaryCollectionAcknowledgeResponse()
	{
		return UnpackNextUtilPacket<MercenariesCollectionAcknowledgeResponse>(10174);
	}

	public void RequestConvertExcessCoinsToRenown(MercenariesConvertExcessCoinsRequest request)
	{
		SendUtilPacket(10175, UtilSystemId.LETTUCE, request);
	}

	public MercenariesConvertExcessCoinsResponse GetMercenariesConvertExcessCoinsResponse()
	{
		return UnpackNextUtilPacket<MercenariesConvertExcessCoinsResponse>(10176);
	}

	public void DebugRatingInfoRequest(DebugRatingInfoRequest request)
	{
		SendUtilPacket(647, UtilSystemId.CLIENT, request);
	}

	public DebugRatingInfoResponse GetDebugRatingInfoResponse()
	{
		return UnpackNextUtilPacket<DebugRatingInfoResponse>(648);
	}

	public void ReportBlizzardCheckoutStatus(BlizzardCheckoutStatus status, TransactionData data, long now)
	{
		string transId = "";
		string prodId = "";
		string currency = "";
		if (data != null)
		{
			transId = data.TransactionID;
			prodId = data.ProductID.ToString();
			currency = data.CurrencyCode;
		}
		SendUtilPacket(366, UtilSystemId.BATTLEPAY, new ReportBlizzardCheckoutStatus
		{
			Status = status,
			TransactionId = transId,
			ProductId = prodId,
			Currency = currency,
			ClientUnixTime = now
		});
	}

	public void MercenariesTeamReorderRequest(MercenariesTeamReorderRequest request)
	{
		SendUtilPacket(10130, UtilSystemId.LETTUCE, request);
	}

	public void RequestLuckyDrawBoxState(LuckyDrawBoxStateRequest request)
	{
		SendUtilPacket(10200, UtilSystemId.CLIENT, request);
	}

	public LuckyDrawBoxStateResponse GetLuckyDrawBoxStateResponse()
	{
		return UnpackNextUtilPacket<LuckyDrawBoxStateResponse>(10201);
	}

	public void UseLuckyDrawHammer(LuckyDrawUseHammerRequest request)
	{
		SendUtilPacket(10205, UtilSystemId.CLIENT, request);
	}

	public LuckyDrawUseHammerResponse GetUseLuckyDrawHammerResponse()
	{
		return UnpackNextUtilPacket<LuckyDrawUseHammerResponse>(10206);
	}

	public void AcknowledgeLuckyDrawHammers(LuckyDrawAcknowledgeAllHammersRequest request)
	{
		SendUtilPacket(10204, UtilSystemId.CLIENT, request);
	}

	public void AcknowledgeLuckyDrawRewards(LuckyDrawAcknowledgeAllRewardsRequest request)
	{
		SendUtilPacket(10207, UtilSystemId.CLIENT, request);
	}

	public void RequestPurchaseRenownOffer(MercenariesPurchaseRenownOfferRequest request)
	{
		SendUtilPacket(10177, UtilSystemId.LETTUCE, request);
	}

	public MercenariesPurchaseRenownOfferResponse GetPurchaseRenownOfferResponse()
	{
		return UnpackNextUtilPacket<MercenariesPurchaseRenownOfferResponse>(10178);
	}

	public void RequestDismissRenownOffer(MercenariesDismissRenownOfferRequest request)
	{
		SendUtilPacket(10179, UtilSystemId.LETTUCE, request);
	}

	public MercenariesDismissRenownOfferResponse GetDismissRenownOfferResponse()
	{
		return UnpackNextUtilPacket<MercenariesDismissRenownOfferResponse>(10180);
	}

	public MercenariesMythicBountyLevelUpdate GetMercenariesMythicBountyLevelUpdate()
	{
		return UnpackNextUtilPacket<MercenariesMythicBountyLevelUpdate>(10189);
	}

	private void SendGamePacket(int packetId, IProtoBuf body)
	{
		dispatcherImpl.SendGamePacket(packetId, body);
	}

	private void SendUtilPacket(int type, UtilSystemId system, IProtoBuf body, RequestPhase requestPhase = RequestPhase.RUNNING)
	{
		dispatcherImpl.SendUtilPacket(type, system, body, requestPhase);
	}

	private T UnpackNextDebugPacket<T>(int packetId) where T : IProtoBuf
	{
		return Unpack<T>(m_debugConnectionManager.NextDebugPacket(), packetId);
	}

	private T UnpackNextGamePacket<T>(int packetId) where T : IProtoBuf
	{
		return Unpack<T>(dispatcherImpl.NextGamePacket(), packetId);
	}

	private T UnpackNextUtilPacket<T>(int packetId) where T : IProtoBuf
	{
		return Unpack<T>(dispatcherImpl.NextUtilPacket().Response, packetId);
	}

	private ResponseWithRequest<T, U> UnpackNextUtilPacketWithRequest<T, U>(int packetId) where T : IProtoBuf where U : IProtoBuf, new()
	{
		ResponseWithRequest<T, U> packet = new ResponseWithRequest<T, U>();
		ResponseWithRequest responseWithRequest = dispatcherImpl.NextUtilPacket();
		if (responseWithRequest.Request != null && responseWithRequest.Request.Body is byte[])
		{
			packet.Request = (U)ProtobufUtil.ParseFromGeneric<U>((byte[])responseWithRequest.Request.Body);
		}
		packet.Response = Unpack<T>(responseWithRequest.Response, packetId);
		return packet;
	}

	private T Unpack<T>(PegasusPacket p, int packetId) where T : IProtoBuf
	{
		if (p == null || p.Type != packetId || !(p.Body is T))
		{
			return default(T);
		}
		return (T)p.Body;
	}

	public void RequestSteamUserInfo()
	{
		SendUtilPacket(705, UtilSystemId.BATTLEPAY, new GetSteamUserInfoRequest());
	}

	public GetSteamUserInfoResponse GetGetSteamUserInfoResponse()
	{
		return UnpackNextUtilPacket<GetSteamUserInfoResponse>(706);
	}

	public SteamPurchaseResponse GetSteamPurcahseResponse()
	{
		return UnpackNextUtilPacket<SteamPurchaseResponse>(702);
	}

	public ExternalPurchaseUpdate GetExternalPurchaseUpdate()
	{
		return UnpackNextUtilPacket<ExternalPurchaseUpdate>(703);
	}
}
