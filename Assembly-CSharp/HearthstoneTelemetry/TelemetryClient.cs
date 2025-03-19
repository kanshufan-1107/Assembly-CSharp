using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.Proto;
using Blizzard.T5.Services;
using Blizzard.Telemetry;
using Blizzard.Telemetry.Standard.Network;
using Blizzard.Telemetry.WTCG.Client;
using Blizzard.Telemetry.WTCG.NGDP;
using Hearthstone;
using UnityEngine;

namespace HearthstoneTelemetry;

public class TelemetryClient : ITelemetryClient
{
	private class DeferredMessage
	{
		public Blizzard.Proto.IProtoBuf ProtoMessage { get; set; }

		public string PackageName { get; set; }

		public string MessageName { get; set; }

		public Type MessageType { get; set; }

		public byte[] Data { get; set; }

		public MessageOptions Options { get; set; }
	}

	private readonly List<DeferredMessage> m_deferredMessages = new List<DeferredMessage>();

	private readonly object m_deferredMessageLock = new object();

	private Service m_telemetryService;

	private bool m_disabled;

	private bool m_initialized;

	private Action m_updateHandler;

	private Blizzard.Telemetry.WTCG.Client.DeviceInfo m_deviceInfo;

	public void SendACSdkResult(string reportId, ACSdkResult.CommandType commandType_, string scriptId, bool returnSetExtraParams, int returnSetupSDK, int returnCallSDK, List<string> messages)
	{
		ACSdkResult message = new ACSdkResult();
		message.ReportId = reportId;
		message.CommandType_ = commandType_;
		message.ScriptId = scriptId;
		message.ReturnSetExtraParams = returnSetExtraParams;
		message.ReturnSetupSDK = returnSetupSDK;
		message.ReturnCallSDK = returnCallSDK;
		message.Messages = messages;
		EnqueueMessage(message);
	}

	public void SendAppInitialized(string testType, float duration, string clientChangelist)
	{
		AppInitialized message = new AppInitialized();
		message.TestType = testType;
		message.Duration = duration;
		message.ClientChangelist = clientChangelist;
		EnqueueMessage(message);
	}

	public void SendAppPaused(bool pauseStatus, float pauseTime)
	{
		AppPaused message = new AppPaused();
		message.PauseStatus = pauseStatus;
		message.PauseTime = pauseTime;
		EnqueueMessage(message);
	}

	public void SendAppStart(string testType, float duration, string clientChangelist)
	{
		AppStart message = new AppStart();
		message.TestType = testType;
		message.Duration = duration;
		message.ClientChangelist = clientChangelist;
		EnqueueMessage(message);
	}

	public void SendAssetLoaderError(string assetGuid, string bundleName, AssetLoaderError.AssetBundleErrorReason reason, string detail)
	{
		AssetLoaderError message = new AssetLoaderError();
		message.DeviceInfo = GetDeviceInfo();
		message.AssetGuid = assetGuid;
		message.BundleName = bundleName;
		message.Reason = reason;
		message.Detail = detail;
		EnqueueMessage(message);
	}

	public void SendAssetNotFound(string assetType, string assetGuid, string filePath, string legacyName)
	{
		AssetNotFound message = new AssetNotFound();
		message.DeviceInfo = GetDeviceInfo();
		message.AssetType = assetType;
		message.AssetGuid = assetGuid;
		message.FilePath = filePath;
		message.LegacyName = legacyName;
		EnqueueMessage(message);
	}

	public void SendAttackInputMethod(long totalNumAttacks, long totalClickAttacks, int percentClickAttacks, long totalDragAttacks, int percentDragAttacks)
	{
		AttackInputMethod message = new AttackInputMethod();
		message.DeviceInfo = GetDeviceInfo();
		message.TotalNumAttacks = totalNumAttacks;
		message.TotalClickAttacks = totalClickAttacks;
		message.PercentClickAttacks = percentClickAttacks;
		message.TotalDragAttacks = totalDragAttacks;
		message.PercentDragAttacks = percentDragAttacks;
		EnqueueMessage(message);
	}

	public void SendBattlegroundsCollectionResult(BattlegroundsCollectionResult.TriggerEvent triggerEvent_, int numberOfOwnedHeroSkins, List<int> favoriteBaseHeroCardIds, List<int> favoriteHeroSkinIds, int numberOfOwnedStrikes, List<int> favoriteStrikeIds, int numberOfOwnedBartenders, List<int> favoriteBartenderSkinIds, int numberOfOwnedBoardSkins, List<int> favoriteBoardSkinIds, int numberOfOwnedEmotes, List<int> equippedEmoteIds)
	{
		BattlegroundsCollectionResult message = new BattlegroundsCollectionResult();
		message.TriggerEvent_ = triggerEvent_;
		message.NumberOfOwnedHeroSkins = numberOfOwnedHeroSkins;
		message.FavoriteBaseHeroCardIds = favoriteBaseHeroCardIds;
		message.FavoriteHeroSkinIds = favoriteHeroSkinIds;
		message.NumberOfOwnedStrikes = numberOfOwnedStrikes;
		message.FavoriteStrikeIds = favoriteStrikeIds;
		message.NumberOfOwnedBartenders = numberOfOwnedBartenders;
		message.FavoriteBartenderSkinIds = favoriteBartenderSkinIds;
		message.NumberOfOwnedBoardSkins = numberOfOwnedBoardSkins;
		message.FavoriteBoardSkinIds = favoriteBoardSkinIds;
		message.NumberOfOwnedEmotes = numberOfOwnedEmotes;
		message.EquippedEmoteIds = equippedEmoteIds;
		EnqueueMessage(message);
	}

	public void SendBenchmarkResult(int cpuAverageFrameTimeMs, int gpuAverageFrameTimeMs, int benchmarkVersion)
	{
		BenchmarkResult message = new BenchmarkResult();
		message.DeviceInfo = GetDeviceInfo();
		message.CpuAverageFrameTimeMs = cpuAverageFrameTimeMs;
		message.GpuAverageFrameTimeMs = gpuAverageFrameTimeMs;
		message.BenchmarkVersion = benchmarkVersion;
		EnqueueMessage(message);
	}

	public void SendBGDownloadResult(float duration, long prevRemainingBytes, long downloadedBytes)
	{
		BGDownloadResult message = new BGDownloadResult();
		message.DeviceInfo = GetDeviceInfo();
		message.Duration = duration;
		message.PrevRemainingBytes = prevRemainingBytes;
		message.DownloadedBytes = downloadedBytes;
		EnqueueMessage(message);
	}

	public void SendBlizzardCheckoutInitializationResult(bool success, string failureReason, string failureDetails)
	{
		BlizzardCheckoutInitializationResult message = new BlizzardCheckoutInitializationResult();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.Success = success;
		message.FailureReason = failureReason;
		message.FailureDetails = failureDetails;
		EnqueueMessage(message);
	}

	public void SendBlizzardCheckoutIsReady(double secondsShown, bool isReady)
	{
		BlizzardCheckoutIsReady message = new BlizzardCheckoutIsReady();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.SecondsShown = secondsShown;
		message.IsReady = isReady;
		EnqueueMessage(message);
	}

	public void SendBlizzardCheckoutPurchaseCancel()
	{
		BlizzardCheckoutPurchaseCancel message = new BlizzardCheckoutPurchaseCancel();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		EnqueueMessage(message);
	}

	public void SendBlizzardCheckoutPurchaseCompletedFailure(string transactionId, string productId, string currency, List<string> errorCodes)
	{
		BlizzardCheckoutPurchaseCompletedFailure message = new BlizzardCheckoutPurchaseCompletedFailure();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.TransactionId = transactionId;
		message.ProductId = productId;
		message.Currency = currency;
		message.ErrorCodes = errorCodes;
		EnqueueMessage(message);
	}

	public void SendBlizzardCheckoutPurchaseCompletedSuccess(string transactionId, string productId, string currency)
	{
		BlizzardCheckoutPurchaseCompletedSuccess message = new BlizzardCheckoutPurchaseCompletedSuccess();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.TransactionId = transactionId;
		message.ProductId = productId;
		message.Currency = currency;
		EnqueueMessage(message);
	}

	public void SendBlizzardCheckoutPurchaseStart(string transactionId, string productId, string currency)
	{
		BlizzardCheckoutPurchaseStart message = new BlizzardCheckoutPurchaseStart();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.TransactionId = transactionId;
		message.ProductId = productId;
		message.Currency = currency;
		EnqueueMessage(message);
	}

	public void SendBoardVisualStateChanged(string fromBoardState, string toBoardState, int timeInSeconds)
	{
		BoardVisualStateChanged message = new BoardVisualStateChanged();
		message.FromBoardState = fromBoardState;
		message.ToBoardState = toBoardState;
		message.TimeInSeconds = timeInSeconds;
		EnqueueMessage(message);
	}

	public void SendBoxInteractable(string testType, float duration, string clientChangelist, int dataVersion)
	{
		BoxInteractable message = new BoxInteractable();
		message.TestType = testType;
		message.Duration = duration;
		message.ClientChangelist = clientChangelist;
		message.DataVersion = dataVersion;
		EnqueueMessage(message);
	}

	public void SendBoxProductBannerClicked(string bannerCampaignName, string bannerImageName, long productId)
	{
		BoxProductBannerClicked message = new BoxProductBannerClicked();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.BannerCampaignName = bannerCampaignName;
		message.BannerImageName = bannerImageName;
		message.ProductId = productId;
		EnqueueMessage(message);
	}

	public void SendBoxProductBannerDisplayed(string bannerCampaignName, string bannerImageName, long productId)
	{
		BoxProductBannerDisplayed message = new BoxProductBannerDisplayed();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.BannerCampaignName = bannerCampaignName;
		message.BannerImageName = bannerImageName;
		message.ProductId = productId;
		EnqueueMessage(message);
	}

	public void SendButtonPressed(string buttonName)
	{
		ButtonPressed message = new ButtonPressed();
		message.ButtonName = buttonName;
		EnqueueMessage(message);
	}

	public void SendCinematic(bool begin, float duration)
	{
		Blizzard.Telemetry.WTCG.Client.Cinematic message = new Blizzard.Telemetry.WTCG.Client.Cinematic();
		message.DeviceInfo = GetDeviceInfo();
		message.Begin = begin;
		message.Duration = duration;
		EnqueueMessage(message);
	}

	public void SendClearInactiveLocales(List<string> locales, bool success, string errors)
	{
		ClearInactiveLocales message = new ClearInactiveLocales();
		message.Locales = locales;
		message.Success = success;
		message.Errors = errors;
		EnqueueMessage(message);
	}

	public void SendClickRecruitAFriend()
	{
		ClickRecruitAFriend message = new ClickRecruitAFriend();
		message.DeviceInfo = GetDeviceInfo();
		EnqueueMessage(message);
	}

	public void SendClientReset(bool forceLogin, bool forceNoAccountTutorial)
	{
		ClientReset message = new ClientReset();
		message.DeviceInfo = GetDeviceInfo();
		message.ForceLogin = forceLogin;
		message.ForceNoAccountTutorial = forceNoAccountTutorial;
		EnqueueMessage(message);
	}

	public void SendCollectionLeftRightClick(CollectionLeftRightClick.Target target_)
	{
		CollectionLeftRightClick message = new CollectionLeftRightClick();
		message.Target_ = target_;
		EnqueueMessage(message);
	}

	public void SendContentConnectFailedToConnect(string url, int httpErrorcode, int serverErrorcode)
	{
		ContentConnectFailedToConnect message = new ContentConnectFailedToConnect();
		message.DeviceInfo = GetDeviceInfo();
		message.Url = url;
		message.HttpErrorcode = httpErrorcode;
		message.ServerErrorcode = serverErrorcode;
		EnqueueMessage(message);
	}

	public void SendContentConnectJsonOpFailed(ContentConnectJsonOpFailed.JsonOp op, string filename, string reason)
	{
		ContentConnectJsonOpFailed message = new ContentConnectJsonOpFailed();
		message.Player = GetPlayer();
		message.Op = op;
		message.Filename = filename;
		message.Reason = reason;
		EnqueueMessage(message);
	}

	public void SendCosmeticsRenderingFallback(string eventMessage)
	{
		CosmeticsRenderingFallback message = new CosmeticsRenderingFallback();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.EventMessage = eventMessage;
		EnqueueMessage(message);
	}

	public void SendDataUpdateFailed(float duration, long realDownloadBytes, long expectedDownloadBytes, int errorCode)
	{
		DataUpdateFailed message = new DataUpdateFailed();
		message.DeviceInfo = GetDeviceInfo();
		message.Duration = duration;
		message.RealDownloadBytes = realDownloadBytes;
		message.ExpectedDownloadBytes = expectedDownloadBytes;
		message.ErrorCode = errorCode;
		EnqueueMessage(message);
	}

	public void SendDataUpdateFinished(float duration, long realDownloadBytes, long expectedDownloadBytes)
	{
		DataUpdateFinished message = new DataUpdateFinished();
		message.DeviceInfo = GetDeviceInfo();
		message.Duration = duration;
		message.RealDownloadBytes = realDownloadBytes;
		message.ExpectedDownloadBytes = expectedDownloadBytes;
		EnqueueMessage(message);
	}

	public void SendDataUpdateProgress(float duration, long realDownloadBytes, long expectedDownloadBytes)
	{
		DataUpdateProgress message = new DataUpdateProgress();
		message.DeviceInfo = GetDeviceInfo();
		message.Duration = duration;
		message.RealDownloadBytes = realDownloadBytes;
		message.ExpectedDownloadBytes = expectedDownloadBytes;
		EnqueueMessage(message);
	}

	public void SendDataUpdateStarted()
	{
		DataUpdateStarted message = new DataUpdateStarted();
		message.DeviceInfo = GetDeviceInfo();
		EnqueueMessage(message);
	}

	public void SendDataUpdateStopped(float duration, long realDownloadBytes, long expectedDownloadBytes, bool byUser)
	{
		DataUpdateStopped message = new DataUpdateStopped();
		message.DeviceInfo = GetDeviceInfo();
		message.Duration = duration;
		message.RealDownloadBytes = realDownloadBytes;
		message.ExpectedDownloadBytes = expectedDownloadBytes;
		message.ByUser = byUser;
		EnqueueMessage(message);
	}

	public void SendDeckCopied(long deckId, string deckHash)
	{
		DeckCopied message = new DeckCopied();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.DeckId = deckId;
		message.DeckHash = deckHash;
		EnqueueMessage(message);
	}

	public void SendDeckPickerToCollection(DeckPickerToCollection.Path path_)
	{
		DeckPickerToCollection message = new DeckPickerToCollection();
		message.DeviceInfo = GetDeviceInfo();
		message.Path_ = path_;
		EnqueueMessage(message);
	}

	public void SendDeckUpdateResponseInfo(float duration)
	{
		DeckUpdateResponseInfo message = new DeckUpdateResponseInfo();
		message.DeviceInfo = GetDeviceInfo();
		message.Duration = duration;
		EnqueueMessage(message);
	}

	public void SendDeepLinkExecuted(string deepLink, string source, bool completed, int questId)
	{
		DeepLinkExecuted message = new DeepLinkExecuted();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.DeepLink = deepLink;
		message.Source = source;
		message.Completed = completed;
		message.QuestId = questId;
		EnqueueMessage(message);
	}

	public void SendDeletedInvalidBundles(List<string> bundles, int counts)
	{
		DeletedInvalidBundles message = new DeletedInvalidBundles();
		message.Bundles = bundles;
		message.Counts = counts;
		EnqueueMessage(message);
	}

	public void SendDeleteModuleData(string moduleName, long deletedSize)
	{
		DeleteModuleData message = new DeleteModuleData();
		message.DeviceInfo = GetDeviceInfo();
		message.ModuleName = moduleName;
		message.DeletedSize = deletedSize;
		EnqueueMessage(message);
	}

	public void SendDeleteOptionalData(long deletedSize)
	{
		DeleteOptionalData message = new DeleteOptionalData();
		message.DeviceInfo = GetDeviceInfo();
		message.DeletedSize = deletedSize;
		EnqueueMessage(message);
	}

	public void SendDeviceMuteChanged(bool muted)
	{
		DeviceMuteChanged message = new DeviceMuteChanged();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.Muted = muted;
		EnqueueMessage(message);
	}

	public void SendDeviceVolumeChanged(float oldVolume, float newVolume)
	{
		DeviceVolumeChanged message = new DeviceVolumeChanged();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.OldVolume = oldVolume;
		message.NewVolume = newVolume;
		EnqueueMessage(message);
	}

	public void SendDragDropCancelPlayCard(long scenarioId, string cardType)
	{
		DragDropCancelPlayCard message = new DragDropCancelPlayCard();
		message.DeviceInfo = GetDeviceInfo();
		message.ScenarioId = scenarioId;
		message.CardType = cardType;
		EnqueueMessage(message);
	}

	public void SendEndGameScreenInit(float elapsedTime, int medalInfoRetryCount, bool medalInfoRetriesTimedOut, bool showRankedReward, bool showCardBackProgress, int otherRewardCount)
	{
		EndGameScreenInit message = new EndGameScreenInit();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.ElapsedTime = elapsedTime;
		message.MedalInfoRetryCount = medalInfoRetryCount;
		message.MedalInfoRetriesTimedOut = medalInfoRetriesTimedOut;
		message.ShowRankedReward = showRankedReward;
		message.ShowCardBackProgress = showCardBackProgress;
		message.OtherRewardCount = otherRewardCount;
		EnqueueMessage(message);
	}

	public void SendExternalAccountLinkingState(ExternalAccountLinkingState.Status state, ulong externalAccountId)
	{
		ExternalAccountLinkingState message = new ExternalAccountLinkingState();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.State = state;
		message.ExternalAccountId = externalAccountId;
		EnqueueMessage(message);
	}

	public void SendFatalBattleNetError(int errorCode, string description)
	{
		FatalBattleNetError message = new FatalBattleNetError();
		message.DeviceInfo = GetDeviceInfo();
		message.ErrorCode = errorCode;
		message.Description = description;
		EnqueueMessage(message);
	}

	public void SendFatalError(string reason, int dataVersion)
	{
		FatalError message = new FatalError();
		message.DeviceInfo = GetDeviceInfo();
		message.Reason = reason;
		message.DataVersion = dataVersion;
		EnqueueMessage(message);
	}

	public void SendFlowPerformance(string uniqueId, Blizzard.Telemetry.WTCG.Client.FlowPerformance.FlowType flowType_, float averageFps, float duration, float fpsWarningsThreshold, int fpsWarningsTotalOccurences, float fpsWarningsTotalTime, float fpsWarningsAverageTime, float fpsWarningsMaxTime)
	{
		Blizzard.Telemetry.WTCG.Client.FlowPerformance message = new Blizzard.Telemetry.WTCG.Client.FlowPerformance();
		message.UniqueId = uniqueId;
		message.DeviceInfo = GetDeviceInfo();
		message.Player = GetPlayer();
		message.FlowType_ = flowType_;
		message.AverageFps = averageFps;
		message.Duration = duration;
		message.FpsWarningsThreshold = fpsWarningsThreshold;
		message.FpsWarningsTotalOccurences = fpsWarningsTotalOccurences;
		message.FpsWarningsTotalTime = fpsWarningsTotalTime;
		message.FpsWarningsAverageTime = fpsWarningsAverageTime;
		message.FpsWarningsMaxTime = fpsWarningsMaxTime;
		EnqueueMessage(message);
	}

	public void SendFlowPerformanceBattlegrounds(string flowId, string gameUuid, int totalRounds)
	{
		Blizzard.Telemetry.WTCG.Client.FlowPerformanceBattlegrounds message = new Blizzard.Telemetry.WTCG.Client.FlowPerformanceBattlegrounds();
		message.FlowId = flowId;
		message.GameUuid = gameUuid;
		message.TotalRounds = totalRounds;
		EnqueueMessage(message);
	}

	public void SendFlowPerformanceGame(string flowId, string uuid, GameType gameType, FormatType formatType, int boardId, int scenarioId)
	{
		Blizzard.Telemetry.WTCG.Client.FlowPerformanceGame message = new Blizzard.Telemetry.WTCG.Client.FlowPerformanceGame();
		message.FlowId = flowId;
		message.DeviceInfo = GetDeviceInfo();
		message.Player = GetPlayer();
		message.Uuid = uuid;
		message.GameType = gameType;
		message.FormatType = formatType;
		message.BoardId = boardId;
		message.ScenarioId = scenarioId;
		EnqueueMessage(message);
	}

	public void SendFlowPerformanceShop(string flowId, Blizzard.Telemetry.WTCG.Client.FlowPerformanceShop.ShopType shopType_)
	{
		Blizzard.Telemetry.WTCG.Client.FlowPerformanceShop message = new Blizzard.Telemetry.WTCG.Client.FlowPerformanceShop();
		message.FlowId = flowId;
		message.DeviceInfo = GetDeviceInfo();
		message.Player = GetPlayer();
		message.ShopType_ = shopType_;
		EnqueueMessage(message);
	}

	public void SendFriendsListView(string currentScene)
	{
		FriendsListView message = new FriendsListView();
		message.DeviceInfo = GetDeviceInfo();
		message.CurrentScene = currentScene;
		EnqueueMessage(message);
	}

	public void SendFTUEButtonClicked(string gameMode)
	{
		FTUEButtonClicked message = new FTUEButtonClicked();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.GameMode = gameMode;
		EnqueueMessage(message);
	}

	public void SendFTUELetsGoButtonClicked(string gameMode)
	{
		FTUELetsGoButtonClicked message = new FTUELetsGoButtonClicked();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.GameMode = gameMode;
		EnqueueMessage(message);
	}

	public void SendFTUEVideoTimeout(string gameMode)
	{
		FTUEVideoTimeout message = new FTUEVideoTimeout();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.GameMode = gameMode;
		EnqueueMessage(message);
	}

	public void SendGameRoundStartAudioSettings(bool deviceMuted, float deviceVolume, float masterVolume, float musicVolume)
	{
		GameRoundStartAudioSettings message = new GameRoundStartAudioSettings();
		message.DeviceInfo = GetDeviceInfo();
		message.Player = GetPlayer();
		message.DeviceMuted = deviceMuted;
		message.DeviceVolume = deviceVolume;
		message.MasterVolume = masterVolume;
		message.MusicVolume = musicVolume;
		EnqueueMessage(message);
	}

	public void SendIgnorableBattleNetError(int errorCode, string description)
	{
		IgnorableBattleNetError message = new IgnorableBattleNetError();
		message.DeviceInfo = GetDeviceInfo();
		message.ErrorCode = errorCode;
		message.Description = description;
		EnqueueMessage(message);
	}

	public void SendIKSClicked(string iksCampaignName, string iksMediaUrl)
	{
		IKSClicked message = new IKSClicked();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.IksCampaignName = iksCampaignName;
		message.IksMediaUrl = iksMediaUrl;
		EnqueueMessage(message);
	}

	public void SendIKSIgnored(string iksCampaignName, string iksMediaUrl)
	{
		IKSIgnored message = new IKSIgnored();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.IksCampaignName = iksCampaignName;
		message.IksMediaUrl = iksMediaUrl;
		EnqueueMessage(message);
	}

	public void SendInGameMessageAction(string messageType, string title, InGameMessageAction.ActionType action, int viewCounts, string uid)
	{
		InGameMessageAction message = new InGameMessageAction();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.MessageType = messageType;
		message.Title = title;
		message.Action = action;
		message.ViewCounts = viewCounts;
		message.Uid = uid;
		EnqueueMessage(message);
	}

	public void SendInGameMessageDataError(string messageType, string title, InGameMessageDataError.ErrorType error, string uid)
	{
		InGameMessageDataError message = new InGameMessageDataError();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.MessageType = messageType;
		message.Title = title;
		message.Error = error;
		message.Uid = uid;
		EnqueueMessage(message);
	}

	public void SendInGameMessageDelayedMessages(string messageType, string eventId)
	{
		InGameMessageDelayedMessages message = new InGameMessageDelayedMessages();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.MessageType = messageType;
		message.EventId = eventId;
		EnqueueMessage(message);
	}

	public void SendInGameMessageDisplayed(string messageType, string uid, string title)
	{
		InGameMessageDisplayed message = new InGameMessageDisplayed();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.MessageType = messageType;
		message.Uid = uid;
		message.Title = title;
		EnqueueMessage(message);
	}

	public void SendInGameMessageQualified(string messageType, List<string> uids)
	{
		InGameMessageQualified message = new InGameMessageQualified();
		message.Player = GetPlayer();
		message.MessageType = messageType;
		message.Uids = uids;
		EnqueueMessage(message);
	}

	public void SendInGameMessageSystemFlow(string messageType, string eventId, int count, InGameMessageSystemFlow.TelemetryMessageType telemetryMessageType_, string uid)
	{
		InGameMessageSystemFlow message = new InGameMessageSystemFlow();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.MessageType = messageType;
		message.EventId = eventId;
		message.Count = count;
		message.TelemetryMessageType_ = telemetryMessageType_;
		message.Uid = uid;
		EnqueueMessage(message);
	}

	public void SendInitialClientStateOutOfOrder(int countNotificationsAchieve, int countNotificationsNotice, int countNotificationsCollection, int countNotificationsCurrency, int countNotificationsBooster, int countNotificationsHeroxp, int countNotificationsPlayerRecord, int countNotificationsArenaSession, int countNotificationsCardBack)
	{
		InitialClientStateOutOfOrder message = new InitialClientStateOutOfOrder();
		message.CountNotificationsAchieve = countNotificationsAchieve;
		message.CountNotificationsNotice = countNotificationsNotice;
		message.CountNotificationsCollection = countNotificationsCollection;
		message.CountNotificationsCurrency = countNotificationsCurrency;
		message.CountNotificationsBooster = countNotificationsBooster;
		message.CountNotificationsHeroxp = countNotificationsHeroxp;
		message.CountNotificationsPlayerRecord = countNotificationsPlayerRecord;
		message.CountNotificationsArenaSession = countNotificationsArenaSession;
		message.CountNotificationsCardBack = countNotificationsCardBack;
		message.DeviceInfo = GetDeviceInfo();
		EnqueueMessage(message);
	}

	public void SendInitialDataRequested(string version, int dataVersion)
	{
		InitialDataRequested message = new InitialDataRequested();
		message.Version = version;
		message.DataVersion = dataVersion;
		EnqueueMessage(message);
	}

	public void SendInitTraceroute(bool success, string initStatus)
	{
		InitTraceroute message = new InitTraceroute();
		message.Success = success;
		message.InitStatus = initStatus;
		EnqueueMessage(message);
	}

	public void SendJobFinishFailure(string jobId, string jobFailureReason, string testType, string clientChangelist, float duration)
	{
		JobFinishFailure message = new JobFinishFailure();
		message.JobId = jobId;
		message.JobFailureReason = jobFailureReason;
		message.TestType = testType;
		message.ClientChangelist = clientChangelist;
		message.Duration = duration;
		EnqueueMessage(message);
	}

	public void SendLanguageChanged(string previousLanguage, string nextLanguage)
	{
		LanguageChanged message = new LanguageChanged();
		message.DeviceInfo = GetDeviceInfo();
		message.PreviousLanguage = previousLanguage;
		message.NextLanguage = nextLanguage;
		EnqueueMessage(message);
	}

	public void SendLiveIssue(string category, string details)
	{
		LiveIssue message = new LiveIssue();
		message.Category = category;
		message.Details = details;
		EnqueueMessage(message);
	}

	public void SendLoadProducts(float timeToLoadProducts, float timeToDeserialize)
	{
		LoadProducts message = new LoadProducts();
		message.DeviceInfo = GetDeviceInfo();
		message.Player = GetPlayer();
		message.TimeToLoadProducts = timeToLoadProducts;
		message.TimeToDeserialize = timeToDeserialize;
		EnqueueMessage(message);
	}

	public void SendLocaleDataUpdateFailed(float duration, long realDownloadBytes, long expectedDownloadBytes, int errorCode)
	{
		LocaleDataUpdateFailed message = new LocaleDataUpdateFailed();
		message.DeviceInfo = GetDeviceInfo();
		message.Duration = duration;
		message.RealDownloadBytes = realDownloadBytes;
		message.ExpectedDownloadBytes = expectedDownloadBytes;
		message.ErrorCode = errorCode;
		EnqueueMessage(message);
	}

	public void SendLocaleDataUpdateFinished(float duration, long realDownloadBytes, long expectedDownloadBytes)
	{
		LocaleDataUpdateFinished message = new LocaleDataUpdateFinished();
		message.DeviceInfo = GetDeviceInfo();
		message.Duration = duration;
		message.RealDownloadBytes = realDownloadBytes;
		message.ExpectedDownloadBytes = expectedDownloadBytes;
		EnqueueMessage(message);
	}

	public void SendLocaleDataUpdateStarted(string locale)
	{
		LocaleDataUpdateStarted message = new LocaleDataUpdateStarted();
		message.DeviceInfo = GetDeviceInfo();
		message.Locale = locale;
		EnqueueMessage(message);
	}

	public void SendLocaleDataUpdateStopped(float duration, long realDownloadBytes, long expectedDownloadBytes, bool byUser)
	{
		LocaleDataUpdateStopped message = new LocaleDataUpdateStopped();
		message.DeviceInfo = GetDeviceInfo();
		message.Duration = duration;
		message.RealDownloadBytes = realDownloadBytes;
		message.ExpectedDownloadBytes = expectedDownloadBytes;
		message.ByUser = byUser;
		EnqueueMessage(message);
	}

	public void SendLocaleModuleDataUpdateFailed(float duration, long realDownloadBytes, long expectedDownloadBytes, int errorCode, string moduleName)
	{
		LocaleModuleDataUpdateFailed message = new LocaleModuleDataUpdateFailed();
		message.DeviceInfo = GetDeviceInfo();
		message.Duration = duration;
		message.RealDownloadBytes = realDownloadBytes;
		message.ExpectedDownloadBytes = expectedDownloadBytes;
		message.ErrorCode = errorCode;
		message.ModuleName = moduleName;
		EnqueueMessage(message);
	}

	public void SendLocaleModuleDataUpdateFinished(float duration, long realDownloadBytes, long expectedDownloadBytes, string moduleName)
	{
		LocaleModuleDataUpdateFinished message = new LocaleModuleDataUpdateFinished();
		message.DeviceInfo = GetDeviceInfo();
		message.Duration = duration;
		message.RealDownloadBytes = realDownloadBytes;
		message.ExpectedDownloadBytes = expectedDownloadBytes;
		message.ModuleName = moduleName;
		EnqueueMessage(message);
	}

	public void SendLocaleModuleDataUpdateStarted(string locale, string moduleName)
	{
		LocaleModuleDataUpdateStarted message = new LocaleModuleDataUpdateStarted();
		message.DeviceInfo = GetDeviceInfo();
		message.Locale = locale;
		message.ModuleName = moduleName;
		EnqueueMessage(message);
	}

	public void SendLocaleModuleDataUpdateStopped(float duration, long realDownloadBytes, long expectedDownloadBytes, bool byUser, string moduleName)
	{
		LocaleModuleDataUpdateStopped message = new LocaleModuleDataUpdateStopped();
		message.DeviceInfo = GetDeviceInfo();
		message.Duration = duration;
		message.RealDownloadBytes = realDownloadBytes;
		message.ExpectedDownloadBytes = expectedDownloadBytes;
		message.ByUser = byUser;
		message.ModuleName = moduleName;
		EnqueueMessage(message);
	}

	public void SendLocaleOptionalDataUpdateFailed(float duration, long realDownloadBytes, long expectedDownloadBytes, int errorCode)
	{
		LocaleOptionalDataUpdateFailed message = new LocaleOptionalDataUpdateFailed();
		message.DeviceInfo = GetDeviceInfo();
		message.Duration = duration;
		message.RealDownloadBytes = realDownloadBytes;
		message.ExpectedDownloadBytes = expectedDownloadBytes;
		message.ErrorCode = errorCode;
		EnqueueMessage(message);
	}

	public void SendLocaleOptionalDataUpdateFinished(float duration, long realDownloadBytes, long expectedDownloadBytes)
	{
		LocaleOptionalDataUpdateFinished message = new LocaleOptionalDataUpdateFinished();
		message.DeviceInfo = GetDeviceInfo();
		message.Duration = duration;
		message.RealDownloadBytes = realDownloadBytes;
		message.ExpectedDownloadBytes = expectedDownloadBytes;
		EnqueueMessage(message);
	}

	public void SendLocaleOptionalDataUpdateStarted(string locale)
	{
		LocaleOptionalDataUpdateStarted message = new LocaleOptionalDataUpdateStarted();
		message.DeviceInfo = GetDeviceInfo();
		message.Locale = locale;
		EnqueueMessage(message);
	}

	public void SendLocaleOptionalDataUpdateStopped(float duration, long realDownloadBytes, long expectedDownloadBytes, bool byUser)
	{
		LocaleOptionalDataUpdateStopped message = new LocaleOptionalDataUpdateStopped();
		message.DeviceInfo = GetDeviceInfo();
		message.Duration = duration;
		message.RealDownloadBytes = realDownloadBytes;
		message.ExpectedDownloadBytes = expectedDownloadBytes;
		message.ByUser = byUser;
		EnqueueMessage(message);
	}

	public void SendLoginTokenFetchResult(LoginTokenFetchResult.TokenFetchResult result)
	{
		LoginTokenFetchResult message = new LoginTokenFetchResult();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.Result = result;
		EnqueueMessage(message);
	}

	public void SendLuckyDrawEventMessage(string eventMessage)
	{
		LuckyDrawEventMessage message = new LuckyDrawEventMessage();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.EventMessage = eventMessage;
		EnqueueMessage(message);
	}

	public void SendManaFilterToggleOff()
	{
		ManaFilterToggleOff message = new ManaFilterToggleOff();
		message.DeviceInfo = GetDeviceInfo();
		EnqueueMessage(message);
	}

	public void SendMASDKAuthResult(MASDKAuthResult.AuthResult result, int errorCode, string source)
	{
		MASDKAuthResult message = new MASDKAuthResult();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.Result = result;
		message.ErrorCode = errorCode;
		message.Source = source;
		EnqueueMessage(message);
	}

	public void SendMASDKImportResult(MASDKImportResult.ImportResult result, MASDKImportResult.ImportType importType_, int errorCode)
	{
		MASDKImportResult message = new MASDKImportResult();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.Result = result;
		message.ImportType_ = importType_;
		message.ErrorCode = errorCode;
		EnqueueMessage(message);
	}

	public void SendMasterVolumeChanged(float oldVolume, float newVolume)
	{
		MasterVolumeChanged message = new MasterVolumeChanged();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.OldVolume = oldVolume;
		message.NewVolume = newVolume;
		EnqueueMessage(message);
	}

	public void SendMercenariesVillageBuildingClicked(int buildingId)
	{
		MercenariesVillageBuildingClicked message = new MercenariesVillageBuildingClicked();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.BuildingId = buildingId;
		EnqueueMessage(message);
	}

	public void SendMercenariesVillageEnter()
	{
		MercenariesVillageEnter message = new MercenariesVillageEnter();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		EnqueueMessage(message);
	}

	public void SendMercenariesVillageExit()
	{
		MercenariesVillageExit message = new MercenariesVillageExit();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		EnqueueMessage(message);
	}

	public void SendMinSpecWarning(bool nextVersion, List<string> warnings)
	{
		MinSpecWarning message = new MinSpecWarning();
		message.DeviceInfo = GetDeviceInfo();
		message.NextVersion = nextVersion;
		message.Warnings = warnings;
		EnqueueMessage(message);
	}

	public void SendMobileFailConnectGameServer(string address, bool moreinfoPressed, bool gotitPressed)
	{
		MobileFailConnectGameServer message = new MobileFailConnectGameServer();
		message.Address = address;
		message.MoreinfoPressed = moreinfoPressed;
		message.GotitPressed = gotitPressed;
		EnqueueMessage(message);
	}

	public void SendModuleDataUpdateFailed(float duration, long realDownloadBytes, long expectedDownloadBytes, int errorCode, string moduleName)
	{
		ModuleDataUpdateFailed message = new ModuleDataUpdateFailed();
		message.DeviceInfo = GetDeviceInfo();
		message.Duration = duration;
		message.RealDownloadBytes = realDownloadBytes;
		message.ExpectedDownloadBytes = expectedDownloadBytes;
		message.ErrorCode = errorCode;
		message.ModuleName = moduleName;
		EnqueueMessage(message);
	}

	public void SendModuleDataUpdateFinished(float duration, long realDownloadBytes, long expectedDownloadBytes, string moduleName)
	{
		ModuleDataUpdateFinished message = new ModuleDataUpdateFinished();
		message.DeviceInfo = GetDeviceInfo();
		message.Duration = duration;
		message.RealDownloadBytes = realDownloadBytes;
		message.ExpectedDownloadBytes = expectedDownloadBytes;
		message.ModuleName = moduleName;
		EnqueueMessage(message);
	}

	public void SendModuleDataUpdateStarted(string moduleName)
	{
		ModuleDataUpdateStarted message = new ModuleDataUpdateStarted();
		message.DeviceInfo = GetDeviceInfo();
		message.ModuleName = moduleName;
		EnqueueMessage(message);
	}

	public void SendModuleDataUpdateStopped(float duration, long realDownloadBytes, long expectedDownloadBytes, bool byUser, string moduleName)
	{
		ModuleDataUpdateStopped message = new ModuleDataUpdateStopped();
		message.DeviceInfo = GetDeviceInfo();
		message.Duration = duration;
		message.RealDownloadBytes = realDownloadBytes;
		message.ExpectedDownloadBytes = expectedDownloadBytes;
		message.ByUser = byUser;
		message.ModuleName = moduleName;
		EnqueueMessage(message);
	}

	public void SendMusicVolumeChanged(float oldVolume, float newVolume)
	{
		MusicVolumeChanged message = new MusicVolumeChanged();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.OldVolume = oldVolume;
		message.NewVolume = newVolume;
		EnqueueMessage(message);
	}

	public void SendNDERerollPopupClicked(bool acceptedReroll, List<long> noticeIds, int cardAsset, List<int> cardPremium, int cardQuantity, int ndePremium, bool isForcedPremium)
	{
		NDERerollPopupClicked message = new NDERerollPopupClicked();
		message.AcceptedReroll = acceptedReroll;
		message.NoticeIds = noticeIds;
		message.CardAsset = cardAsset;
		message.CardPremium = cardPremium;
		message.CardQuantity = cardQuantity;
		message.NdePremium = ndePremium;
		message.IsForcedPremium = isForcedPremium;
		EnqueueMessage(message);
	}

	public void SendNetworkError(NetworkError.ErrorType errorType_, string description, int errorCode)
	{
		NetworkError message = new NetworkError();
		message.DeviceInfo = GetDeviceInfo();
		message.ErrorType_ = errorType_;
		message.Description = description;
		message.ErrorCode = errorCode;
		EnqueueMessage(message);
	}

	public void SendNetworkUnreachableRecovered(int outageSeconds)
	{
		NetworkUnreachableRecovered message = new NetworkUnreachableRecovered();
		message.DeviceInfo = GetDeviceInfo();
		message.OutageSeconds = outageSeconds;
		EnqueueMessage(message);
	}

	public void SendOldVersionInStore(string liveVersion, string updatestat, bool silentGo)
	{
		OldVersionInStore message = new OldVersionInStore();
		message.LiveVersion = liveVersion;
		message.Updatestat = updatestat;
		message.SilentGo = silentGo;
		EnqueueMessage(message);
	}

	public void SendOptionalDataUpdateFailed(float duration, long realDownloadBytes, long expectedDownloadBytes, int errorCode)
	{
		OptionalDataUpdateFailed message = new OptionalDataUpdateFailed();
		message.DeviceInfo = GetDeviceInfo();
		message.Duration = duration;
		message.RealDownloadBytes = realDownloadBytes;
		message.ExpectedDownloadBytes = expectedDownloadBytes;
		message.ErrorCode = errorCode;
		EnqueueMessage(message);
	}

	public void SendOptionalDataUpdateFinished(float duration, long realDownloadBytes, long expectedDownloadBytes)
	{
		OptionalDataUpdateFinished message = new OptionalDataUpdateFinished();
		message.DeviceInfo = GetDeviceInfo();
		message.Duration = duration;
		message.RealDownloadBytes = realDownloadBytes;
		message.ExpectedDownloadBytes = expectedDownloadBytes;
		EnqueueMessage(message);
	}

	public void SendOptionalDataUpdateStarted()
	{
		OptionalDataUpdateStarted message = new OptionalDataUpdateStarted();
		message.DeviceInfo = GetDeviceInfo();
		EnqueueMessage(message);
	}

	public void SendOptionalDataUpdateStopped(float duration, long realDownloadBytes, long expectedDownloadBytes, bool byUser)
	{
		OptionalDataUpdateStopped message = new OptionalDataUpdateStopped();
		message.DeviceInfo = GetDeviceInfo();
		message.Duration = duration;
		message.RealDownloadBytes = realDownloadBytes;
		message.ExpectedDownloadBytes = expectedDownloadBytes;
		message.ByUser = byUser;
		EnqueueMessage(message);
	}

	public void SendPackOpening(float timeToRegisterPackOpening, float timeTillAnimationStart, int packTypeId, int packsOpened)
	{
		Blizzard.Telemetry.WTCG.Client.PackOpening message = new Blizzard.Telemetry.WTCG.Client.PackOpening();
		message.DeviceInfo = GetDeviceInfo();
		message.Player = GetPlayer();
		message.TimeToRegisterPackOpening = timeToRegisterPackOpening;
		message.TimeTillAnimationStart = timeTillAnimationStart;
		message.PackTypeId = packTypeId;
		message.PacksOpened = packsOpened;
		EnqueueMessage(message);
	}

	public void SendPackOpenToStore(PackOpenToStore.Path path_)
	{
		PackOpenToStore message = new PackOpenToStore();
		message.DeviceInfo = GetDeviceInfo();
		message.Path_ = path_;
		EnqueueMessage(message);
	}

	public void SendPersonalizedMessagesResult(bool success, int messageCount, List<string> messageUids)
	{
		PersonalizedMessagesResult message = new PersonalizedMessagesResult();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.Success = success;
		message.MessageCount = messageCount;
		message.MessageUids = messageUids;
		EnqueueMessage(message);
	}

	public void SendPresenceChanged(PresenceStatus newPresenceStatus, PresenceStatus prevPresenceStatus, long millisecondsSincePrev)
	{
		PresenceChanged message = new PresenceChanged();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.NewPresenceStatus = newPresenceStatus;
		message.PrevPresenceStatus = prevPresenceStatus;
		message.MillisecondsSincePrev = millisecondsSincePrev;
		EnqueueMessage(message);
	}

	public void SendPreviousInstanceStatus(int totalCrashCount, int totalExceptionCount, int lowMemoryWarningCount, int crashInARowCount, int sameExceptionCount, bool crashed, string exceptionHash)
	{
		Blizzard.Telemetry.WTCG.Client.PreviousInstanceStatus message = new Blizzard.Telemetry.WTCG.Client.PreviousInstanceStatus();
		message.DeviceInfo = GetDeviceInfo();
		message.TotalCrashCount = totalCrashCount;
		message.TotalExceptionCount = totalExceptionCount;
		message.LowMemoryWarningCount = lowMemoryWarningCount;
		message.CrashInARowCount = crashInARowCount;
		message.SameExceptionCount = sameExceptionCount;
		message.Crashed = crashed;
		message.ExceptionHash = exceptionHash;
		EnqueueMessage(message);
	}

	public void SendPurchaseCancelClicked(long pmtProductId)
	{
		PurchaseCancelClicked message = new PurchaseCancelClicked();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.PmtProductId = pmtProductId;
		EnqueueMessage(message);
	}

	public void SendPurchasePayNowClicked(long pmtProductId)
	{
		PurchasePayNowClicked message = new PurchasePayNowClicked();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.PmtProductId = pmtProductId;
		EnqueueMessage(message);
	}

	public void SendPushNotificationSystemAppInitialized(string appName, string userId)
	{
		PushNotificationSystemAppInitialized message = new PushNotificationSystemAppInitialized();
		message.AppName = appName;
		message.UserId = userId;
		EnqueueMessage(message);
	}

	public void SendPushNotificationSystemDeviceTokenObtained(string appName, string userId, string deviceToken)
	{
		PushNotificationSystemDeviceTokenObtained message = new PushNotificationSystemDeviceTokenObtained();
		message.AppName = appName;
		message.UserId = userId;
		message.DeviceToken = deviceToken;
		EnqueueMessage(message);
	}

	public void SendPushNotificationSystemNotificationDeleted(string appName, string userId)
	{
		PushNotificationSystemNotificationDeleted message = new PushNotificationSystemNotificationDeleted();
		message.AppName = appName;
		message.UserId = userId;
		EnqueueMessage(message);
	}

	public void SendPushNotificationSystemNotificationOpened(string appName, string userId)
	{
		PushNotificationSystemNotificationOpened message = new PushNotificationSystemNotificationOpened();
		message.AppName = appName;
		message.UserId = userId;
		EnqueueMessage(message);
	}

	public void SendPushNotificationSystemNotificationReceived(string appName, string userId)
	{
		PushNotificationSystemNotificationReceived message = new PushNotificationSystemNotificationReceived();
		message.AppName = appName;
		message.UserId = userId;
		EnqueueMessage(message);
	}

	public void SendPushRegistrationFailed(string error)
	{
		PushRegistrationFailed message = new PushRegistrationFailed();
		message.Error = error;
		EnqueueMessage(message);
	}

	public void SendPushRegistrationSucceeded(string deviceToken)
	{
		PushRegistrationSucceeded message = new PushRegistrationSucceeded();
		message.DeviceToken = deviceToken;
		EnqueueMessage(message);
	}

	public void SendQuestTileClick(int questID, DisplayContext displayContext, QuestTileClickType clickType, string deepLinkValue)
	{
		QuestTileClick message = new QuestTileClick();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.QuestID = questID;
		message.DisplayContext = displayContext;
		message.ClickType = clickType;
		message.DeepLinkValue = deepLinkValue;
		EnqueueMessage(message);
	}

	public void SendReconnectSuccess(float disconnectDuration, float reconnectDuration, string reconnectType)
	{
		ReconnectSuccess message = new ReconnectSuccess();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.DisconnectDuration = disconnectDuration;
		message.ReconnectDuration = reconnectDuration;
		message.ReconnectType = reconnectType;
		EnqueueMessage(message);
	}

	public void SendReconnectTimeout(string reconnectType)
	{
		ReconnectTimeout message = new ReconnectTimeout();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.ReconnectType = reconnectType;
		EnqueueMessage(message);
	}

	public void SendRegionSwitched(int currentRegion, int newRegion)
	{
		RegionSwitched message = new RegionSwitched();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.CurrentRegion = currentRegion;
		message.NewRegion = newRegion;
		EnqueueMessage(message);
	}

	public void SendRestartDueToPlayerMigration()
	{
		RestartDueToPlayerMigration message = new RestartDueToPlayerMigration();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		EnqueueMessage(message);
	}

	public void SendRuntimeUpdate(float duration, RuntimeUpdate.Intention intention_)
	{
		RuntimeUpdate message = new RuntimeUpdate();
		message.DeviceInfo = GetDeviceInfo();
		message.Duration = duration;
		message.Intention_ = intention_;
		EnqueueMessage(message);
	}

	public void SendSeamlessReconnectEnd(float disconnectDuration, string disconnectReason, int secSinceLastResume, int secSpentPaused)
	{
		SeamlessReconnectEnd message = new SeamlessReconnectEnd();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.DisconnectDuration = disconnectDuration;
		message.DisconnectReason = disconnectReason;
		message.SecSinceLastResume = secSinceLastResume;
		message.SecSpentPaused = secSpentPaused;
		EnqueueMessage(message);
	}

	public void SendSeamlessReconnectStart(string disconnectReason, int secSinceLastResume, int secSpentPaused)
	{
		SeamlessReconnectStart message = new SeamlessReconnectStart();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.DisconnectReason = disconnectReason;
		message.SecSinceLastResume = secSinceLastResume;
		message.SecSpentPaused = secSpentPaused;
		EnqueueMessage(message);
	}

	public void SendShopBalanceAvailable(List<Balance> balances)
	{
		ShopBalanceAvailable message = new ShopBalanceAvailable();
		message.Player = GetPlayer();
		message.Balances = balances;
		EnqueueMessage(message);
	}

	public void SendShopCardClick(ShopCard shopcard, string storeType, string shopTab, string shopSubTab)
	{
		ShopCardClick message = new ShopCardClick();
		message.Player = GetPlayer();
		message.Shopcard = shopcard;
		message.StoreType = storeType;
		message.ShopTab = shopTab;
		message.ShopSubTab = shopSubTab;
		EnqueueMessage(message);
	}

	public void SendShopProductDetailOpened(long productId, float loadingTime)
	{
		ShopProductDetailOpened message = new ShopProductDetailOpened();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.ProductId = productId;
		message.LoadingTime = loadingTime;
		EnqueueMessage(message);
	}

	public void SendShopPurchaseEvent(Product product, int quantity, string currency, double amount, bool isGift, string storefront, bool purchaseComplete, string storeType, string redirectedProductId, string shopTab, string shopSubTab)
	{
		ShopPurchaseEvent message = new ShopPurchaseEvent();
		message.Player = GetPlayer();
		message.Product = product;
		message.Quantity = quantity;
		message.Currency = currency;
		message.Amount = amount;
		message.IsGift = isGift;
		message.Storefront = storefront;
		message.PurchaseComplete = purchaseComplete;
		message.StoreType = storeType;
		message.RedirectedProductId = redirectedProductId;
		message.ShopTab = shopTab;
		message.ShopSubTab = shopSubTab;
		EnqueueMessage(message);
	}

	public void SendShopStatus(string error, double timeInHubSec, float timeShownClosedSec)
	{
		Blizzard.Telemetry.WTCG.Client.ShopStatus message = new Blizzard.Telemetry.WTCG.Client.ShopStatus();
		message.Player = GetPlayer();
		message.Error = error;
		message.TimeInHubSec = timeInHubSec;
		message.TimeShownClosedSec = timeShownClosedSec;
		EnqueueMessage(message);
	}

	public void SendShopVisit(List<ShopCard> cards, string storeType, string shopTab, string shopSubTab, float loadTimeSeconds)
	{
		ShopVisit message = new ShopVisit();
		message.Player = GetPlayer();
		message.Cards = cards;
		message.StoreType = storeType;
		message.ShopTab = shopTab;
		message.ShopSubTab = shopSubTab;
		message.LoadTimeSeconds = loadTimeSeconds;
		EnqueueMessage(message);
	}

	public void SendSmartDeckCompleteFailed(int requestMessageSize)
	{
		SmartDeckCompleteFailed message = new SmartDeckCompleteFailed();
		message.Player = GetPlayer();
		message.RequestMessageSize = requestMessageSize;
		EnqueueMessage(message);
	}

	public void SendStartupAudioSettings(bool deviceMuted, float deviceVolume, float masterVolume, float musicVolume)
	{
		StartupAudioSettings message = new StartupAudioSettings();
		message.DeviceInfo = GetDeviceInfo();
		message.DeviceMuted = deviceMuted;
		message.DeviceVolume = deviceVolume;
		message.MasterVolume = masterVolume;
		message.MusicVolume = musicVolume;
		EnqueueMessage(message);
	}

	public void SendSystemDetail(UnitySystemInfo info)
	{
		SystemDetail message = new SystemDetail();
		message.Info = info;
		EnqueueMessage(message);
	}

	public void SendTraceroute(string host, List<string> hops, int totalHops, int failHops, int successHops)
	{
		Traceroute message = new Traceroute();
		message.Host = host;
		message.Hops = hops;
		message.TotalHops = totalHops;
		message.FailHops = failHops;
		message.SuccessHops = successHops;
		EnqueueMessage(message);
	}

	public void SendWebLoginError()
	{
		WebLoginError message = new WebLoginError();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		EnqueueMessage(message);
	}

	public void SendWelcomeQuestsAcknowledged(float questAckDuration)
	{
		WelcomeQuestsAcknowledged message = new WelcomeQuestsAcknowledged();
		message.Player = GetPlayer();
		message.DeviceInfo = GetDeviceInfo();
		message.QuestAckDuration = questAckDuration;
		EnqueueMessage(message);
	}

	public void SendWrongVersionContinued(string liveVersion, string updatestat)
	{
		WrongVersionContinued message = new WrongVersionContinued();
		message.LiveVersion = liveVersion;
		message.Updatestat = updatestat;
		EnqueueMessage(message);
	}

	public void SendWrongVersionFixed(string liveVersion, int reties, bool succeeded)
	{
		WrongVersionFixed message = new WrongVersionFixed();
		message.LiveVersion = liveVersion;
		message.Reties = reties;
		message.Succeeded = succeeded;
		EnqueueMessage(message);
	}

	public void SendApkInstallFailure(string updatedVersion, string reason)
	{
		ApkInstallFailure message = new ApkInstallFailure();
		message.UpdatedVersion = updatedVersion;
		message.Reason = reason;
		EnqueueMessage(message);
	}

	public void SendApkInstallSuccess(string updatedVersion, float availableSpaceMB, float elapsedSeconds)
	{
		ApkInstallSuccess message = new ApkInstallSuccess();
		message.UpdatedVersion = updatedVersion;
		message.AvailableSpaceMB = availableSpaceMB;
		message.ElapsedSeconds = elapsedSeconds;
		EnqueueMessage(message);
	}

	public void SendApkUpdate(int installedVersion, int assetVersion, int agentVersion)
	{
		ApkUpdate message = new ApkUpdate();
		message.InstalledVersion = installedVersion;
		message.AssetVersion = assetVersion;
		message.AgentVersion = agentVersion;
		EnqueueMessage(message);
	}

	public void SendNotEnoughSpaceError(ulong availableSpace, ulong expectedOrgBytes, string filesDir, bool initial, bool localeSwitch)
	{
		NotEnoughSpaceError message = new NotEnoughSpaceError();
		message.AvailableSpace = availableSpace;
		message.ExpectedOrgBytes = expectedOrgBytes;
		message.FilesDir = filesDir;
		message.Initial = initial;
		message.LocaleSwitch = localeSwitch;
		EnqueueMessage(message);
	}

	public void SendNoWifi(string updatedVersion, float availableSpaceMB, float elapsedSeconds, bool initial, bool localeSwitch)
	{
		NoWifi message = new NoWifi();
		message.UpdatedVersion = updatedVersion;
		message.AvailableSpaceMB = availableSpaceMB;
		message.ElapsedSeconds = elapsedSeconds;
		message.Initial = initial;
		message.LocaleSwitch = localeSwitch;
		EnqueueMessage(message);
	}

	public void SendOpeningAppStore(string updatedVersion, float availableSpaceMB, float elapsedSeconds, string versionInStore)
	{
		OpeningAppStore message = new OpeningAppStore();
		message.UpdatedVersion = updatedVersion;
		message.AvailableSpaceMB = availableSpaceMB;
		message.ElapsedSeconds = elapsedSeconds;
		message.VersionInStore = versionInStore;
		EnqueueMessage(message);
	}

	public void SendUpdateError(uint errorCode, float elapsedSeconds)
	{
		UpdateError message = new UpdateError();
		message.ErrorCode = errorCode;
		message.ElapsedSeconds = elapsedSeconds;
		EnqueueMessage(message);
	}

	public void SendUpdateFinished(string updatedVersion, float availableSpaceMB, float elapsedSeconds)
	{
		UpdateFinished message = new UpdateFinished();
		message.UpdatedVersion = updatedVersion;
		message.AvailableSpaceMB = availableSpaceMB;
		message.ElapsedSeconds = elapsedSeconds;
		EnqueueMessage(message);
	}

	public void SendUpdateProgress(float duration, long realDownloadBytes, long expectedDownloadBytes)
	{
		UpdateProgress message = new UpdateProgress();
		message.DeviceInfo = GetDeviceInfo();
		message.Duration = duration;
		message.RealDownloadBytes = realDownloadBytes;
		message.ExpectedDownloadBytes = expectedDownloadBytes;
		EnqueueMessage(message);
	}

	public void SendUpdateStarted(string installedVersion, string textureFormat, string dataPath, float availableSpaceMB)
	{
		UpdateStarted message = new UpdateStarted();
		message.InstalledVersion = installedVersion;
		message.TextureFormat = textureFormat;
		message.DataPath = dataPath;
		message.AvailableSpaceMB = availableSpaceMB;
		EnqueueMessage(message);
	}

	public void SendUpdateStopped(string updatedVersion, float availableSpaceMB, float elapsedSeconds, bool byUser)
	{
		UpdateStopped message = new UpdateStopped();
		message.UpdatedVersion = updatedVersion;
		message.AvailableSpaceMB = availableSpaceMB;
		message.ElapsedSeconds = elapsedSeconds;
		message.ByUser = byUser;
		EnqueueMessage(message);
	}

	public void SendUsingCellularData(string updatedVersion, float availableSpaceMB, float elapsedSeconds, bool initial, bool localeSwitch)
	{
		UsingCellularData message = new UsingCellularData();
		message.UpdatedVersion = updatedVersion;
		message.AvailableSpaceMB = availableSpaceMB;
		message.ElapsedSeconds = elapsedSeconds;
		message.Initial = initial;
		message.LocaleSwitch = localeSwitch;
		EnqueueMessage(message);
	}

	public void SendVersionCodeInStore(string versionCode, string countryCode)
	{
		VersionCodeInStore message = new VersionCodeInStore();
		message.VersionCode = versionCode;
		message.CountryCode = countryCode;
		EnqueueMessage(message);
	}

	public void SendVersionError(uint errorCode, uint agentState, string languages, string region, string branch, string additionalTags)
	{
		VersionError message = new VersionError();
		message.ErrorCode = errorCode;
		message.AgentState = agentState;
		message.Languages = languages;
		message.Region = region;
		message.Branch = branch;
		message.AdditionalTags = additionalTags;
		EnqueueMessage(message);
	}

	public void SendVersionFinished(string currentVersion, string liveVersion)
	{
		VersionFinished message = new VersionFinished();
		message.CurrentVersion = currentVersion;
		message.LiveVersion = liveVersion;
		EnqueueMessage(message);
	}

	public void SendVersionStarted(int dummy)
	{
		VersionStarted message = new VersionStarted();
		message.Dummy = dummy;
		EnqueueMessage(message);
	}

	public void Initialize(Service telemetryService)
	{
		if (telemetryService == null)
		{
			throw new ArgumentNullException("telemetryService");
		}
		m_telemetryService = telemetryService;
		m_initialized = true;
		if (m_deferredMessages.Any())
		{
			DeferredMessage[] messages;
			lock (m_deferredMessageLock)
			{
				messages = m_deferredMessages.ToArray();
				m_deferredMessages.Clear();
			}
			DeferredMessage[] array = messages;
			foreach (DeferredMessage message in array)
			{
				EnqueueMessage(message);
			}
		}
	}

	public bool IsInitialized()
	{
		return m_initialized;
	}

	public void Enable()
	{
		m_disabled = false;
	}

	public void Disable()
	{
		m_disabled = true;
	}

	public void OnUpdate()
	{
		if (m_initialized && !m_disabled && m_updateHandler != null)
		{
			m_updateHandler();
		}
	}

	public void Shutdown()
	{
		m_initialized = false;
		if (m_telemetryService is IDisposable disposableService)
		{
			disposableService.Dispose();
		}
		m_telemetryService = null;
	}

	public long EnqueueMessage(string packageName, string messageName, byte[] data, MessageOptions options = null)
	{
		if (m_disabled || data.Length == 0)
		{
			return 0L;
		}
		if (!m_initialized)
		{
			lock (m_deferredMessageLock)
			{
				m_deferredMessages.Add(new DeferredMessage
				{
					PackageName = packageName,
					MessageName = messageName,
					Data = data,
					Options = options
				});
			}
			return 0L;
		}
		return m_telemetryService.Enqueue(packageName, messageName, data, options);
	}

	public void SendConnectSuccess(string name, string host = null, uint? port = null)
	{
		ConnectSuccess message = new ConnectSuccess();
		message.Name = name;
		message.Host = host;
		message.Port = port;
		EnqueueMessage(message);
	}

	public void SendConnectFail(string name, string reason, string host = null, uint? port = null)
	{
		ConnectFail message = new ConnectFail();
		message.Name = name;
		message.Reason = reason;
		message.Host = host;
		message.Port = port;
		EnqueueMessage(message);
	}

	public void SendDisconnect(string name, Disconnect.Reason reason, string description = null, string host = null, uint? port = null)
	{
		Disconnect message = new Disconnect();
		message.Reason_ = reason;
		message.Name = name;
		message.Description = description;
		message.Host = host;
		message.Port = port;
		EnqueueMessage(message);
	}

	public void SendFindGameResult(FindGameResult message)
	{
		message.DeviceInfo = GetDeviceInfo();
		message.Player = GetPlayer();
		EnqueueMessage(message);
	}

	public void SendConnectToGameServer(ConnectToGameServer message)
	{
		message.DeviceInfo = GetDeviceInfo();
		message.Player = GetPlayer();
		EnqueueMessage(message);
	}

	public void SendTcpQualitySample(string address4, uint port, float sampleTimeMs, uint bytesSent, uint bytesReceived, uint messagesSent, uint messagesReceived, TcpQualitySample.Metric timeSincePrevMessageMs, TcpQualitySample.Metric pingMs)
	{
		TcpQualitySample message = new TcpQualitySample();
		message.Address4 = address4;
		message.Port = port;
		message.SampleTimeMs = sampleTimeMs;
		message.BytesSent = bytesSent;
		message.BytesReceived = bytesReceived;
		message.MessagesSent = messagesSent;
		message.MessagesReceived = messagesReceived;
		message.TimeSincePrevMessageMs = timeSincePrevMessageMs;
		message.PingMs = pingMs;
		EnqueueMessage(message);
	}

	public long EnqueueMessage(IProtoBuf message, MessageOptions options = null)
	{
		if (m_disabled || message == null)
		{
			return 0L;
		}
		MemoryStream memoryStream = new MemoryStream();
		message.Serialize(memoryStream);
		return EnqueueMessage(message.GetType(), memoryStream.ToArray(), options);
	}

	private long EnqueueMessage(Blizzard.Proto.IProtoBuf message, MessageOptions options = null)
	{
		if (m_disabled || message == null)
		{
			return 0L;
		}
		if (!m_initialized)
		{
			lock (m_deferredMessageLock)
			{
				m_deferredMessages.Add(new DeferredMessage
				{
					ProtoMessage = message,
					Options = options
				});
			}
			return 0L;
		}
		return m_telemetryService.Enqueue(message, options);
	}

	private long EnqueueMessage(Type messageType, byte[] data, MessageOptions options = null)
	{
		if (m_disabled || data.Length == 0)
		{
			return 0L;
		}
		if (!m_initialized)
		{
			lock (m_deferredMessageLock)
			{
				m_deferredMessages.Add(new DeferredMessage
				{
					MessageType = messageType,
					Data = data,
					Options = options
				});
			}
			return 0L;
		}
		int lastSeparationIndex = messageType.FullName.LastIndexOf('.');
		return m_telemetryService.Enqueue(messageType.FullName.Substring(0, lastSeparationIndex), messageType.FullName.Substring(lastSeparationIndex + 1), data, options);
	}

	private void EnqueueMessage(DeferredMessage message)
	{
		if (message.ProtoMessage != null)
		{
			EnqueueMessage(message.ProtoMessage, message.Options);
		}
		else if (string.IsNullOrEmpty(message.PackageName) || string.IsNullOrEmpty(message.MessageName))
		{
			EnqueueMessage(message.MessageType, message.Data, message.Options);
		}
		else
		{
			EnqueueMessage(message.PackageName, message.MessageName, message.Data, message.Options);
		}
	}

	private Blizzard.Telemetry.WTCG.Client.DeviceInfo GetDeviceInfo()
	{
		if (m_deviceInfo == null)
		{
			int osEnumVal = (int)PlatformSettings.OS;
			int screenCatEnumVal = (int)PlatformSettings.Screen;
			m_deviceInfo = new Blizzard.Telemetry.WTCG.Client.DeviceInfo
			{
				Os = (Blizzard.Telemetry.WTCG.Client.DeviceInfo.OSCategory)osEnumVal,
				OsVersion = SystemInfo.operatingSystem,
				Model = PlatformSettings.DeviceModel,
				Screen = (Blizzard.Telemetry.WTCG.Client.DeviceInfo.ScreenCategory)screenCatEnumVal,
				DroidTextureCompression = ((PlatformSettings.OS == OSCategory.Android) ? AndroidDeviceSettings.Get().InstalledTexture : null)
			};
		}
		m_deviceInfo.ConnectionType_ = GetConnectionType();
		return m_deviceInfo;
	}

	private Blizzard.Telemetry.WTCG.Client.Player GetPlayer()
	{
		return new Blizzard.Telemetry.WTCG.Client.Player
		{
			BattleNetIdLo = (long)BnetUtils.TryGetBnetAccountId().GetValueOrDefault(),
			BnetRegion = (BnetUtils.TryGetBnetRegion() ?? BnetRegion.REGION_UNINITIALIZED).ToString(),
			BnetGameRegion = (Blizzard.Telemetry.WTCG.Client.Player.BnetRegionEnum)(BnetUtils.TryGetGameRegion() ?? BnetRegion.REGION_UNINITIALIZED),
			Locale = Localization.GetLocaleName(),
			GameAccountId = (long)BnetUtils.TryGetGameAccountId().GetValueOrDefault()
		};
	}

	private static Blizzard.Telemetry.WTCG.Client.DeviceInfo.ConnectionType GetConnectionType()
	{
		if (!TryGetInternetReachability(out var reachability))
		{
			return Blizzard.Telemetry.WTCG.Client.DeviceInfo.ConnectionType.UNKNOWN;
		}
		switch (reachability)
		{
		case NetworkReachability.ReachableViaCarrierDataNetwork:
			return Blizzard.Telemetry.WTCG.Client.DeviceInfo.ConnectionType.CELLULAR;
		case NetworkReachability.NotReachable:
			return Blizzard.Telemetry.WTCG.Client.DeviceInfo.ConnectionType.UNKNOWN;
		default:
			if (PlatformSettings.OS != OSCategory.Android && PlatformSettings.OS != OSCategory.iOS)
			{
				return Blizzard.Telemetry.WTCG.Client.DeviceInfo.ConnectionType.WIRED;
			}
			return Blizzard.Telemetry.WTCG.Client.DeviceInfo.ConnectionType.WIFI;
		}
	}

	private static bool TryGetInternetReachability(out NetworkReachability reachability)
	{
		reachability = NetworkReachability.NotReachable;
		if (HearthstoneApplication.IsMainThread)
		{
			reachability = Application.internetReachability;
		}
		else
		{
			if (!ServiceManager.TryGet<NetworkReachabilityManager>(out var reachabilityManager))
			{
				return false;
			}
			reachability = reachabilityManager.CachedReachability;
		}
		return true;
	}
}
