using Blizzard.T5.Core;
using BobNetProto;
using PegasusFSG;
using PegasusGame;
using PegasusLettuce;
using PegasusLuckyDraw;
using PegasusUtil;

namespace Networking;

public class PacketDecoderManager : IPacketDecoderManager
{
	private readonly Map<int, IPacketDecoder> packetDecoders;

	public PacketDecoderManager(bool registerDebugDecoders)
	{
		packetDecoders = new Map<int, IPacketDecoder>
		{
			{
				116,
				new PongPacketDecoder()
			},
			{
				169,
				new DefaultProtobufPacketDecoder<Deadend>()
			},
			{
				167,
				new DefaultProtobufPacketDecoder<DeadendUtil>()
			},
			{
				14,
				new DefaultProtobufPacketDecoder<AllOptions>()
			},
			{
				5,
				new DefaultProtobufPacketDecoder<DebugMessage>()
			},
			{
				17,
				new DefaultProtobufPacketDecoder<EntityChoices>()
			},
			{
				13,
				new DefaultProtobufPacketDecoder<EntitiesChosen>()
			},
			{
				16,
				new DefaultProtobufPacketDecoder<GameSetup>()
			},
			{
				19,
				new DefaultProtobufPacketDecoder<PowerHistory>()
			},
			{
				15,
				new DefaultProtobufPacketDecoder<UserUI>()
			},
			{
				9,
				new DefaultProtobufPacketDecoder<PegasusGame.TurnTimer>()
			},
			{
				10,
				new DefaultProtobufPacketDecoder<NAckOption>()
			},
			{
				12,
				new DefaultProtobufPacketDecoder<GameCanceled>()
			},
			{
				23,
				new DefaultProtobufPacketDecoder<ServerResult>()
			},
			{
				24,
				new DefaultProtobufPacketDecoder<SpectatorNotify>()
			},
			{
				202,
				new DefaultProtobufPacketDecoder<DeckList>()
			},
			{
				713,
				new DefaultProtobufPacketDecoder<CollectionClientStateResponse>()
			},
			{
				714,
				new DefaultProtobufPacketDecoder<SecondaryClientStateResponse>()
			},
			{
				215,
				new DefaultProtobufPacketDecoder<GetDeckContentsResponse>()
			},
			{
				216,
				new DefaultProtobufPacketDecoder<DBAction>()
			},
			{
				217,
				new DefaultProtobufPacketDecoder<DeckCreated>()
			},
			{
				218,
				new DefaultProtobufPacketDecoder<DeckDeleted>()
			},
			{
				219,
				new DefaultProtobufPacketDecoder<DeckRenamed>()
			},
			{
				226,
				new DefaultProtobufPacketDecoder<BoosterContent>()
			},
			{
				208,
				new DefaultProtobufPacketDecoder<GamesInfo>()
			},
			{
				231,
				new DefaultProtobufPacketDecoder<ProfileDeckLimit>()
			},
			{
				233,
				new DefaultProtobufPacketDecoder<ProfileProgress>()
			},
			{
				270,
				new DefaultProtobufPacketDecoder<PlayerRecords>()
			},
			{
				271,
				new DefaultProtobufPacketDecoder<RewardProgress>()
			},
			{
				232,
				new DefaultProtobufPacketDecoder<MedalInfo>()
			},
			{
				246,
				new DefaultProtobufPacketDecoder<DraftBeginning>()
			},
			{
				247,
				new DefaultProtobufPacketDecoder<DraftRetired>()
			},
			{
				248,
				new DefaultProtobufPacketDecoder<DraftChoicesAndContents>()
			},
			{
				249,
				new DefaultProtobufPacketDecoder<DraftChosen>()
			},
			{
				288,
				new DefaultProtobufPacketDecoder<DraftRewardsAcked>()
			},
			{
				251,
				new DefaultProtobufPacketDecoder<DraftError>()
			},
			{
				285,
				new DefaultProtobufPacketDecoder<ValidateAchieveResponse>()
			},
			{
				282,
				new DefaultProtobufPacketDecoder<CancelQuestResponse>()
			},
			{
				264,
				new DefaultProtobufPacketDecoder<GuardianVars>()
			},
			{
				260,
				new DefaultProtobufPacketDecoder<CardValues>()
			},
			{
				258,
				new DefaultProtobufPacketDecoder<BoughtSoldCard>()
			},
			{
				269,
				new DefaultProtobufPacketDecoder<MassDisenchantResponse>()
			},
			{
				265,
				new DefaultProtobufPacketDecoder<BattlePayStatusResponse>()
			},
			{
				295,
				new DefaultProtobufPacketDecoder<ThirdPartyPurchaseStatusResponse>()
			},
			{
				272,
				new DefaultProtobufPacketDecoder<PurchaseMethod>()
			},
			{
				275,
				new DefaultProtobufPacketDecoder<CancelPurchaseResponse>()
			},
			{
				256,
				new DefaultProtobufPacketDecoder<PurchaseResponse>()
			},
			{
				238,
				new DefaultProtobufPacketDecoder<BattlePayConfigResponse>()
			},
			{
				280,
				new DefaultProtobufPacketDecoder<PurchaseWithGoldResponse>()
			},
			{
				283,
				new DefaultProtobufPacketDecoder<HeroXP>()
			},
			{
				254,
				new NoOpPacketDecoder()
			},
			{
				331,
				new DefaultProtobufPacketDecoder<UpdateAccountLicensesResponse>()
			},
			{
				236,
				new DefaultProtobufPacketDecoder<CardBacks>()
			},
			{
				292,
				new DefaultProtobufPacketDecoder<SetFavoriteCardBackResponse>()
			},
			{
				296,
				new DefaultProtobufPacketDecoder<SetProgressResponse>()
			},
			{
				299,
				new DefaultProtobufPacketDecoder<TriggerEventResponse>()
			},
			{
				306,
				new DefaultProtobufPacketDecoder<AdventureProgressResponse>()
			},
			{
				336,
				new DefaultProtobufPacketDecoder<RecruitAFriendURLResponse>()
			},
			{
				338,
				new DefaultProtobufPacketDecoder<RecruitAFriendDataResponse>()
			},
			{
				307,
				new DefaultProtobufPacketDecoder<UpdateLoginComplete>()
			},
			{
				311,
				new DefaultProtobufPacketDecoder<AccountLicenseAchieveResponse>()
			},
			{
				315,
				new DefaultProtobufPacketDecoder<SubscribeResponse>()
			},
			{
				316,
				new DefaultProtobufPacketDecoder<TavernBrawlInfo>()
			},
			{
				317,
				new DefaultProtobufPacketDecoder<TavernBrawlPlayerRecordResponse>()
			},
			{
				318,
				new DefaultProtobufPacketDecoder<FavoriteHeroesResponse>()
			},
			{
				320,
				new DefaultProtobufPacketDecoder<SetFavoriteHeroResponse>()
			},
			{
				324,
				new DefaultProtobufPacketDecoder<DebugCommandResponse>()
			},
			{
				325,
				new DefaultProtobufPacketDecoder<AccountLicensesInfoResponse>()
			},
			{
				326,
				new DefaultProtobufPacketDecoder<GenericResponse>()
			},
			{
				328,
				new DefaultProtobufPacketDecoder<ClientRequestResponse>()
			},
			{
				322,
				new DefaultProtobufPacketDecoder<GetAssetResponse>()
			},
			{
				341,
				new DefaultProtobufPacketDecoder<ClientStaticAssetsResponse>()
			},
			{
				333,
				new DefaultProtobufPacketDecoder<ClientStateNotification>()
			},
			{
				347,
				new DefaultProtobufPacketDecoder<TavernBrawlRequestSessionBeginResponse>()
			},
			{
				348,
				new DefaultProtobufPacketDecoder<TavernBrawlRequestSessionRetireResponse>()
			},
			{
				349,
				new DefaultProtobufPacketDecoder<TavernBrawlSessionAckRewardsResponse>()
			},
			{
				351,
				new DefaultProtobufPacketDecoder<ArenaSessionResponse>()
			},
			{
				504,
				new DefaultProtobufPacketDecoder<RequestNearbyFSGsResponse>()
			},
			{
				505,
				new DefaultProtobufPacketDecoder<CheckInToFSGResponse>()
			},
			{
				506,
				new DefaultProtobufPacketDecoder<CheckOutOfFSGResponse>()
			},
			{
				508,
				new DefaultProtobufPacketDecoder<InnkeeperSetupGatheringResponse>()
			},
			{
				509,
				new DefaultProtobufPacketDecoder<PatronCheckedInToFSG>()
			},
			{
				510,
				new DefaultProtobufPacketDecoder<PatronCheckedOutOfFSG>()
			},
			{
				511,
				new DefaultProtobufPacketDecoder<FSGFeatureConfig>()
			},
			{
				512,
				new DefaultProtobufPacketDecoder<FSGPatronListUpdate>()
			},
			{
				355,
				new DefaultProtobufPacketDecoder<DraftRemovePremiumsResponse>()
			},
			{
				368,
				new DefaultProtobufPacketDecoder<LeaguePromoteSelfResponse>()
			},
			{
				358,
				new DefaultProtobufPacketDecoder<GameSaveDataResponse>()
			},
			{
				360,
				new DefaultProtobufPacketDecoder<SetGameSaveDataResponse>()
			},
			{
				391,
				new DefaultProtobufPacketDecoder<GameSaveDataStateUpdate>()
			},
			{
				362,
				new DefaultProtobufPacketDecoder<LocateCheatServerResponse>()
			},
			{
				365,
				new DefaultProtobufPacketDecoder<GetServerTimeResponse>()
			},
			{
				363,
				new DefaultProtobufPacketDecoder<GameToConnectNotification>()
			},
			{
				6,
				new DefaultProtobufPacketDecoder<AIDebugInformation>()
			},
			{
				8,
				new DefaultProtobufPacketDecoder<RopeTimerDebugInformation>()
			},
			{
				370,
				new DefaultProtobufPacketDecoder<SmartDeckResponse>()
			},
			{
				373,
				new DefaultProtobufPacketDecoder<BattlegroundsRatingInfoResponse>()
			},
			{
				7,
				new DefaultProtobufPacketDecoder<ScriptDebugInformation>()
			},
			{
				30,
				new DefaultProtobufPacketDecoder<GameRoundHistory>()
			},
			{
				31,
				new DefaultProtobufPacketDecoder<GameRealTimeBattlefieldRaces>()
			},
			{
				36,
				new DefaultProtobufPacketDecoder<PlayerRealTimeBattlefieldRaces>()
			},
			{
				34,
				new DefaultProtobufPacketDecoder<BattlegroundsRatingChange>()
			},
			{
				35,
				new DefaultProtobufPacketDecoder<GameGuardianVars>()
			},
			{
				50,
				new DefaultProtobufPacketDecoder<ScriptLogMessage>()
			},
			{
				334,
				new DefaultProtobufPacketDecoder<FreeDeckChoiceResponse>()
			},
			{
				656,
				new DefaultProtobufPacketDecoder<FreeDeckStateUpdate>()
			},
			{
				390,
				new DefaultProtobufPacketDecoder<UtilLogRelay>()
			},
			{
				52,
				new DefaultProtobufPacketDecoder<AchievementProgress>()
			},
			{
				618,
				new DefaultProtobufPacketDecoder<AchievementComplete>()
			},
			{
				383,
				new DefaultProtobufPacketDecoder<PVPDRSessionStartResponse>()
			},
			{
				389,
				new DefaultProtobufPacketDecoder<PVPDRSessionEndResponse>()
			},
			{
				377,
				new DefaultProtobufPacketDecoder<PVPDRSessionInfoResponse>()
			},
			{
				379,
				new DefaultProtobufPacketDecoder<PVPDRStatsInfoResponse>()
			},
			{
				381,
				new DefaultProtobufPacketDecoder<PVPDRRetireResponse>()
			},
			{
				601,
				new DefaultProtobufPacketDecoder<PlayerQuestStateUpdate>()
			},
			{
				602,
				new DefaultProtobufPacketDecoder<PlayerQuestPoolStateUpdate>()
			},
			{
				603,
				new DefaultProtobufPacketDecoder<PlayerAchievementStateUpdate>()
			},
			{
				614,
				new DefaultProtobufPacketDecoder<PlayerRewardTrackStateUpdate>()
			},
			{
				617,
				new DefaultProtobufPacketDecoder<RewardTrackXpNotification>()
			},
			{
				619,
				new DefaultProtobufPacketDecoder<RewardTrackUnclaimedNotification>()
			},
			{
				607,
				new DefaultProtobufPacketDecoder<RerollQuestResponse>()
			},
			{
				608,
				new DefaultProtobufPacketDecoder<CosmeticCoins>()
			},
			{
				610,
				new DefaultProtobufPacketDecoder<SetFavoriteCosmeticCoinResponse>()
			},
			{
				611,
				new DefaultProtobufPacketDecoder<CoinUpdate>()
			},
			{
				53,
				new DefaultProtobufPacketDecoder<UpdateBattlegroundInfo>()
			},
			{
				54,
				new DefaultProtobufPacketDecoder<GetBattlegroundHeroArmorTierList>()
			},
			{
				61,
				new DefaultProtobufPacketDecoder<GetBattlegroundsPlayerAnomaly>()
			},
			{
				37,
				new DefaultProtobufPacketDecoder<TeammatesEntities>()
			},
			{
				38,
				new DefaultProtobufPacketDecoder<TeammatesChooseEntities>()
			},
			{
				39,
				new DefaultProtobufPacketDecoder<TeammatesEntitiesChosen>()
			},
			{
				59,
				new DefaultProtobufPacketDecoder<TeammateConcede>()
			},
			{
				41,
				new DefaultProtobufPacketDecoder<EntityPinged>()
			},
			{
				621,
				new DefaultProtobufPacketDecoder<BattlegroundsHeroSkinsResponse>()
			},
			{
				623,
				new DefaultProtobufPacketDecoder<SetBattlegroundsFavoriteHeroSkinResponse>()
			},
			{
				625,
				new DefaultProtobufPacketDecoder<ClearBattlegroundsFavoriteHeroSkinResponse>()
			},
			{
				626,
				new DefaultProtobufPacketDecoder<BattlegroundsGuideSkinsResponse>()
			},
			{
				628,
				new DefaultProtobufPacketDecoder<SetBattlegroundsFavoriteGuideSkinResponse>()
			},
			{
				630,
				new DefaultProtobufPacketDecoder<ClearBattlegroundsFavoriteGuideSkinResponse>()
			},
			{
				10102,
				new DefaultProtobufPacketDecoder<MercenariesPlayerInfoResponse>()
			},
			{
				10104,
				new DefaultProtobufPacketDecoder<MercenariesCollectionResponse>()
			},
			{
				10105,
				new DefaultProtobufPacketDecoder<MercenariesCollectionUpdate>()
			},
			{
				10106,
				new DefaultProtobufPacketDecoder<MercenariesCurrencyUpdate>()
			},
			{
				10107,
				new DefaultProtobufPacketDecoder<MercenariesExperienceUpdate>()
			},
			{
				10109,
				new DefaultProtobufPacketDecoder<MercenariesRewardUpdate>()
			},
			{
				10111,
				new DefaultProtobufPacketDecoder<MercenariesTeamListResponse>()
			},
			{
				10113,
				new DefaultProtobufPacketDecoder<CreateMercenariesTeamResponse>()
			},
			{
				10115,
				new DefaultProtobufPacketDecoder<UpdateMercenariesTeamResponse>()
			},
			{
				10117,
				new DefaultProtobufPacketDecoder<DeleteMercenariesTeamResponse>()
			},
			{
				10191,
				new DefaultProtobufPacketDecoder<UpdateMercenariesTeamNameResponse>()
			},
			{
				10123,
				new DefaultProtobufPacketDecoder<CraftMercenaryResponse>()
			},
			{
				10125,
				new DefaultProtobufPacketDecoder<UpgradeMercenaryAbilityResponse>()
			},
			{
				10134,
				new DefaultProtobufPacketDecoder<OpenMercenariesPackResponse>()
			},
			{
				10136,
				new DefaultProtobufPacketDecoder<LettuceMapResponse>()
			},
			{
				10138,
				new DefaultProtobufPacketDecoder<LettuceMapChooseNodeResponse>()
			},
			{
				10140,
				new DefaultProtobufPacketDecoder<LettuceMapRetireResponse>()
			},
			{
				10141,
				new DefaultProtobufPacketDecoder<MercenariesPvPRatingUpdate>()
			},
			{
				10142,
				new DefaultProtobufPacketDecoder<MercenariesPvPWinUpdate>()
			},
			{
				10144,
				new DefaultProtobufPacketDecoder<MercenariesMapTreasureSelectionResponse>()
			},
			{
				10121,
				new DefaultProtobufPacketDecoder<UpdateEquippedMercenaryEquipmentResponse>()
			},
			{
				10127,
				new DefaultProtobufPacketDecoder<CraftMercenaryEquipmentResponse>()
			},
			{
				10129,
				new DefaultProtobufPacketDecoder<UpgradeMercenaryEquipmentResponse>()
			},
			{
				10145,
				new DefaultProtobufPacketDecoder<MercenariesPlayerBountyInfoUpdate>()
			},
			{
				10147,
				new DefaultProtobufPacketDecoder<MercenariesGetVillageResponse>()
			},
			{
				10148,
				new DefaultProtobufPacketDecoder<MercenariesVisitorStateUpdate>()
			},
			{
				10151,
				new DefaultProtobufPacketDecoder<MercenariesBuildingUpgradeResponse>()
			},
			{
				10153,
				new DefaultProtobufPacketDecoder<MercenariesClaimTaskResponse>()
			},
			{
				10161,
				new DefaultProtobufPacketDecoder<MercenariesDismissTaskResponse>()
			},
			{
				10163,
				new DefaultProtobufPacketDecoder<MercenariesRefreshVillageResponse>()
			},
			{
				10164,
				new DefaultProtobufPacketDecoder<MercenariesBuildingStateUpdate>()
			},
			{
				10167,
				new DefaultProtobufPacketDecoder<MercenariesMapVisitorSelectionResponse>()
			},
			{
				10168,
				new DefaultProtobufPacketDecoder<MercenariesTeamUpdate>()
			},
			{
				10119,
				new DefaultProtobufPacketDecoder<UpdateEquippedMercenaryPortraitResponse>()
			},
			{
				648,
				new DefaultProtobufPacketDecoder<DebugRatingInfoResponse>()
			},
			{
				637,
				new DefaultProtobufPacketDecoder<BattlegroundsBoardSkinsResponse>()
			},
			{
				639,
				new DefaultProtobufPacketDecoder<SetBattlegroundsFavoriteBoardSkinResponse>()
			},
			{
				641,
				new DefaultProtobufPacketDecoder<ClearBattlegroundsFavoriteBoardSkinResponse>()
			},
			{
				10155,
				new DefaultProtobufPacketDecoder<MercenariesTrainingAddResponse>()
			},
			{
				10158,
				new DefaultProtobufPacketDecoder<MercenariesTrainingRemoveResponse>()
			},
			{
				10157,
				new DefaultProtobufPacketDecoder<MercenariesTrainingCollectResponse>()
			},
			{
				642,
				new DefaultProtobufPacketDecoder<BattlegroundsFinishersResponse>()
			},
			{
				644,
				new DefaultProtobufPacketDecoder<SetBattlegroundsFavoriteFinisherResponse>()
			},
			{
				646,
				new DefaultProtobufPacketDecoder<ClearBattlegroundsFavoriteFinisherResponse>()
			},
			{
				649,
				new DefaultProtobufPacketDecoder<BattlegroundsEmotesResponse>()
			},
			{
				651,
				new DefaultProtobufPacketDecoder<SetBattlegroundsEmoteLoadoutResponse>()
			},
			{
				10172,
				new DefaultProtobufPacketDecoder<MercenariesBountyAcknowledgeResponse>()
			},
			{
				10174,
				new DefaultProtobufPacketDecoder<MercenariesCollectionAcknowledgeResponse>()
			},
			{
				342,
				new DefaultProtobufPacketDecoder<ProcessRecruitAFriendResponse>()
			},
			{
				10201,
				new DefaultProtobufPacketDecoder<LuckyDrawBoxStateResponse>()
			},
			{
				10206,
				new DefaultProtobufPacketDecoder<LuckyDrawUseHammerResponse>()
			},
			{
				10176,
				new DefaultProtobufPacketDecoder<MercenariesConvertExcessCoinsResponse>()
			},
			{
				10178,
				new DefaultProtobufPacketDecoder<MercenariesPurchaseRenownOfferResponse>()
			},
			{
				10180,
				new DefaultProtobufPacketDecoder<MercenariesDismissRenownOfferResponse>()
			},
			{
				10182,
				new DefaultProtobufPacketDecoder<MercenariesMythicTreasureScalarsResponse>()
			},
			{
				10184,
				new DefaultProtobufPacketDecoder<MercenariesMythicTreasureScalarPurchaseResponse>()
			},
			{
				10186,
				new DefaultProtobufPacketDecoder<MercenariesMythicUpgradeAbilityResponse>()
			},
			{
				10188,
				new DefaultProtobufPacketDecoder<MercenariesMythicUpgradeEquipmentResponse>()
			},
			{
				10189,
				new DefaultProtobufPacketDecoder<MercenariesMythicBountyLevelUpdate>()
			},
			{
				702,
				new DefaultProtobufPacketDecoder<SteamPurchaseResponse>()
			},
			{
				703,
				new DefaultProtobufPacketDecoder<ExternalPurchaseUpdate>()
			},
			{
				706,
				new DefaultProtobufPacketDecoder<GetSteamUserInfoResponse>()
			},
			{
				660,
				new DefaultProtobufPacketDecoder<SkipApprenticeResponse>()
			},
			{
				708,
				new DefaultProtobufPacketDecoder<GetCustomizedCardHistoryResponse>()
			},
			{
				62,
				new DefaultProtobufPacketDecoder<FakeConcede>()
			},
			{
				44,
				new DefaultProtobufPacketDecoder<MulliganChooseOneTentativeSelection>()
			},
			{
				64,
				new DefaultProtobufPacketDecoder<RequestReplaceBattlegroundsMulliganHero>()
			},
			{
				65,
				new DefaultProtobufPacketDecoder<ReplaceBattlegroundMulliganHero>()
			},
			{
				66,
				new DefaultProtobufPacketDecoder<RequestReplaceAllBattlegroundsMulliganHeroExcept>()
			}
		};
		if (registerDebugDecoders)
		{
			packetDecoders.Add(123, new DefaultProtobufPacketDecoder<DebugConsoleCommand>());
			packetDecoders.Add(124, new DefaultProtobufPacketDecoder<DebugConsoleResponse>());
			packetDecoders.Add(10170, new DefaultProtobufPacketDecoder<MercenariesDebugCommandResponse>());
		}
	}

	public bool CanDecodePacket(int packetId)
	{
		return packetDecoders.ContainsKey(packetId);
	}

	public PegasusPacket DecodePacket(PegasusPacket packet)
	{
		if (!packetDecoders.TryGetValue(packet.Type, out var decoder))
		{
			return null;
		}
		return decoder.DecodePacket(packet);
	}

	public PegasusPacket DecodePacket(int packetType, int context, byte[] body)
	{
		if (!packetDecoders.TryGetValue(packetType, out var decoder))
		{
			return null;
		}
		return decoder.DecodePacket(packetType, context, body);
	}
}
