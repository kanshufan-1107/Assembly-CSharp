using System.Collections.Generic;
using Blizzard.Telemetry;
using Blizzard.Telemetry.Standard.Network;
using Blizzard.Telemetry.WTCG.Client;

namespace HearthstoneTelemetry;

public interface ITelemetryClient
{
	void SendACSdkResult(string reportId, ACSdkResult.CommandType commandType_, string scriptId, bool returnSetExtraParams, int returnSetupSDK, int returnCallSDK, List<string> messages);

	void SendAppInitialized(string testType, float duration, string clientChangelist);

	void SendAppPaused(bool pauseStatus, float pauseTime);

	void SendAppStart(string testType, float duration, string clientChangelist);

	void SendAssetLoaderError(string assetGuid, string bundleName, AssetLoaderError.AssetBundleErrorReason reason, string detail);

	void SendAssetNotFound(string assetType, string assetGuid, string filePath, string legacyName);

	void SendAttackInputMethod(long totalNumAttacks, long totalClickAttacks, int percentClickAttacks, long totalDragAttacks, int percentDragAttacks);

	void SendBattlegroundsCollectionResult(BattlegroundsCollectionResult.TriggerEvent triggerEvent_, int numberOfOwnedHeroSkins, List<int> favoriteBaseHeroCardIds, List<int> favoriteHeroSkinIds, int numberOfOwnedStrikes, List<int> favoriteStrikeIds, int numberOfOwnedBartenders, List<int> favoriteBartenderSkinIds, int numberOfOwnedBoardSkins, List<int> favoriteBoardSkinIds, int numberOfOwnedEmotes, List<int> equippedEmoteIds);

	void SendBenchmarkResult(int cpuAverageFrameTimeMs, int gpuAverageFrameTimeMs, int benchmarkVersion);

	void SendBGDownloadResult(float duration, long prevRemainingBytes, long downloadedBytes);

	void SendBlizzardCheckoutInitializationResult(bool success, string failureReason, string failureDetails);

	void SendBlizzardCheckoutIsReady(double secondsShown, bool isReady);

	void SendBlizzardCheckoutPurchaseCancel();

	void SendBlizzardCheckoutPurchaseCompletedFailure(string transactionId, string productId, string currency, List<string> errorCodes);

	void SendBlizzardCheckoutPurchaseCompletedSuccess(string transactionId, string productId, string currency);

	void SendBlizzardCheckoutPurchaseStart(string transactionId, string productId, string currency);

	void SendBoardVisualStateChanged(string fromBoardState, string toBoardState, int timeInSeconds);

	void SendBoxInteractable(string testType, float duration, string clientChangelist, int dataVersion);

	void SendBoxProductBannerClicked(string bannerCampaignName, string bannerImageName, long productId);

	void SendBoxProductBannerDisplayed(string bannerCampaignName, string bannerImageName, long productId);

	void SendButtonPressed(string buttonName);

	void SendCinematic(bool begin, float duration);

	void SendClearInactiveLocales(List<string> locales, bool success, string errors);

	void SendClickRecruitAFriend();

	void SendClientReset(bool forceLogin, bool forceNoAccountTutorial);

	void SendCollectionLeftRightClick(CollectionLeftRightClick.Target target_);

	void SendContentConnectFailedToConnect(string url, int httpErrorcode, int serverErrorcode);

	void SendContentConnectJsonOpFailed(ContentConnectJsonOpFailed.JsonOp op, string filename, string reason);

	void SendCosmeticsRenderingFallback(string eventMessage);

	void SendDataUpdateFailed(float duration, long realDownloadBytes, long expectedDownloadBytes, int errorCode);

	void SendDataUpdateFinished(float duration, long realDownloadBytes, long expectedDownloadBytes);

	void SendDataUpdateProgress(float duration, long realDownloadBytes, long expectedDownloadBytes);

	void SendDataUpdateStarted();

	void SendDataUpdateStopped(float duration, long realDownloadBytes, long expectedDownloadBytes, bool byUser);

	void SendDeckCopied(long deckId, string deckHash);

	void SendDeckPickerToCollection(DeckPickerToCollection.Path path_);

	void SendDeckUpdateResponseInfo(float duration);

	void SendDeepLinkExecuted(string deepLink, string source, bool completed, int questId);

	void SendDeletedInvalidBundles(List<string> bundles, int counts);

	void SendDeleteModuleData(string moduleName, long deletedSize);

	void SendDeleteOptionalData(long deletedSize);

	void SendDeviceMuteChanged(bool muted);

	void SendDeviceVolumeChanged(float oldVolume, float newVolume);

	void SendDragDropCancelPlayCard(long scenarioId, string cardType);

	void SendEndGameScreenInit(float elapsedTime, int medalInfoRetryCount, bool medalInfoRetriesTimedOut, bool showRankedReward, bool showCardBackProgress, int otherRewardCount);

	void SendExternalAccountLinkingState(ExternalAccountLinkingState.Status state, ulong externalAccountId);

	void SendFatalBattleNetError(int errorCode, string description);

	void SendFatalError(string reason, int dataVersion);

	void SendFlowPerformance(string uniqueId, Blizzard.Telemetry.WTCG.Client.FlowPerformance.FlowType flowType_, float averageFps, float duration, float fpsWarningsThreshold, int fpsWarningsTotalOccurences, float fpsWarningsTotalTime, float fpsWarningsAverageTime, float fpsWarningsMaxTime);

	void SendFlowPerformanceBattlegrounds(string flowId, string gameUuid, int totalRounds);

	void SendFlowPerformanceGame(string flowId, string uuid, GameType gameType, FormatType formatType, int boardId, int scenarioId);

	void SendFlowPerformanceShop(string flowId, Blizzard.Telemetry.WTCG.Client.FlowPerformanceShop.ShopType shopType_);

	void SendFriendsListView(string currentScene);

	void SendFTUEButtonClicked(string gameMode);

	void SendFTUELetsGoButtonClicked(string gameMode);

	void SendFTUEVideoTimeout(string gameMode);

	void SendGameRoundStartAudioSettings(bool deviceMuted, float deviceVolume, float masterVolume, float musicVolume);

	void SendIgnorableBattleNetError(int errorCode, string description);

	void SendIKSClicked(string iksCampaignName, string iksMediaUrl);

	void SendIKSIgnored(string iksCampaignName, string iksMediaUrl);

	void SendInGameMessageAction(string messageType, string title, InGameMessageAction.ActionType action, int viewCounts, string uid);

	void SendInGameMessageDataError(string messageType, string title, InGameMessageDataError.ErrorType error, string uid);

	void SendInGameMessageDelayedMessages(string messageType, string eventId);

	void SendInGameMessageDisplayed(string messageType, string uid, string title);

	void SendInGameMessageQualified(string messageType, List<string> uids);

	void SendInGameMessageSystemFlow(string messageType, string eventId, int count, InGameMessageSystemFlow.TelemetryMessageType telemetryMessageType_, string uid);

	void SendInitialClientStateOutOfOrder(int countNotificationsAchieve, int countNotificationsNotice, int countNotificationsCollection, int countNotificationsCurrency, int countNotificationsBooster, int countNotificationsHeroxp, int countNotificationsPlayerRecord, int countNotificationsArenaSession, int countNotificationsCardBack);

	void SendInitialDataRequested(string version, int dataVersion);

	void SendInitTraceroute(bool success, string initStatus);

	void SendJobFinishFailure(string jobId, string jobFailureReason, string testType, string clientChangelist, float duration);

	void SendLanguageChanged(string previousLanguage, string nextLanguage);

	void SendLiveIssue(string category, string details);

	void SendLoadProducts(float timeToLoadProducts, float timeToDeserialize);

	void SendLocaleDataUpdateFailed(float duration, long realDownloadBytes, long expectedDownloadBytes, int errorCode);

	void SendLocaleDataUpdateFinished(float duration, long realDownloadBytes, long expectedDownloadBytes);

	void SendLocaleDataUpdateStarted(string locale);

	void SendLocaleDataUpdateStopped(float duration, long realDownloadBytes, long expectedDownloadBytes, bool byUser);

	void SendLocaleModuleDataUpdateFailed(float duration, long realDownloadBytes, long expectedDownloadBytes, int errorCode, string moduleName);

	void SendLocaleModuleDataUpdateFinished(float duration, long realDownloadBytes, long expectedDownloadBytes, string moduleName);

	void SendLocaleModuleDataUpdateStarted(string locale, string moduleName);

	void SendLocaleModuleDataUpdateStopped(float duration, long realDownloadBytes, long expectedDownloadBytes, bool byUser, string moduleName);

	void SendLocaleOptionalDataUpdateFailed(float duration, long realDownloadBytes, long expectedDownloadBytes, int errorCode);

	void SendLocaleOptionalDataUpdateFinished(float duration, long realDownloadBytes, long expectedDownloadBytes);

	void SendLocaleOptionalDataUpdateStarted(string locale);

	void SendLocaleOptionalDataUpdateStopped(float duration, long realDownloadBytes, long expectedDownloadBytes, bool byUser);

	void SendLoginTokenFetchResult(LoginTokenFetchResult.TokenFetchResult result);

	void SendLuckyDrawEventMessage(string eventMessage);

	void SendManaFilterToggleOff();

	void SendMASDKAuthResult(MASDKAuthResult.AuthResult result, int errorCode, string source);

	void SendMASDKImportResult(MASDKImportResult.ImportResult result, MASDKImportResult.ImportType importType_, int errorCode);

	void SendMasterVolumeChanged(float oldVolume, float newVolume);

	void SendMercenariesVillageBuildingClicked(int buildingId);

	void SendMercenariesVillageEnter();

	void SendMercenariesVillageExit();

	void SendMinSpecWarning(bool nextVersion, List<string> warnings);

	void SendMobileFailConnectGameServer(string address, bool moreinfoPressed, bool gotitPressed);

	void SendModuleDataUpdateFailed(float duration, long realDownloadBytes, long expectedDownloadBytes, int errorCode, string moduleName);

	void SendModuleDataUpdateFinished(float duration, long realDownloadBytes, long expectedDownloadBytes, string moduleName);

	void SendModuleDataUpdateStarted(string moduleName);

	void SendModuleDataUpdateStopped(float duration, long realDownloadBytes, long expectedDownloadBytes, bool byUser, string moduleName);

	void SendMusicVolumeChanged(float oldVolume, float newVolume);

	void SendNDERerollPopupClicked(bool acceptedReroll, List<long> noticeIds, int cardAsset, List<int> cardPremium, int cardQuantity, int ndePremium, bool isForcedPremium);

	void SendNetworkError(NetworkError.ErrorType errorType_, string description, int errorCode);

	void SendNetworkUnreachableRecovered(int outageSeconds);

	void SendOldVersionInStore(string liveVersion, string updatestat, bool silentGo);

	void SendOptionalDataUpdateFailed(float duration, long realDownloadBytes, long expectedDownloadBytes, int errorCode);

	void SendOptionalDataUpdateFinished(float duration, long realDownloadBytes, long expectedDownloadBytes);

	void SendOptionalDataUpdateStarted();

	void SendOptionalDataUpdateStopped(float duration, long realDownloadBytes, long expectedDownloadBytes, bool byUser);

	void SendPackOpening(float timeToRegisterPackOpening, float timeTillAnimationStart, int packTypeId, int packsOpened);

	void SendPackOpenToStore(PackOpenToStore.Path path_);

	void SendPersonalizedMessagesResult(bool success, int messageCount, List<string> messageUids);

	void SendPresenceChanged(PresenceStatus newPresenceStatus, PresenceStatus prevPresenceStatus, long millisecondsSincePrev);

	void SendPreviousInstanceStatus(int totalCrashCount, int totalExceptionCount, int lowMemoryWarningCount, int crashInARowCount, int sameExceptionCount, bool crashed, string exceptionHash);

	void SendPurchaseCancelClicked(long pmtProductId);

	void SendPurchasePayNowClicked(long pmtProductId);

	void SendPushNotificationSystemAppInitialized(string appName, string userId);

	void SendPushNotificationSystemDeviceTokenObtained(string appName, string userId, string deviceToken);

	void SendPushNotificationSystemNotificationDeleted(string appName, string userId);

	void SendPushNotificationSystemNotificationOpened(string appName, string userId);

	void SendPushNotificationSystemNotificationReceived(string appName, string userId);

	void SendPushRegistrationFailed(string error);

	void SendPushRegistrationSucceeded(string deviceToken);

	void SendQuestTileClick(int questID, DisplayContext displayContext, QuestTileClickType clickType, string deepLinkValue);

	void SendReconnectSuccess(float disconnectDuration, float reconnectDuration, string reconnectType);

	void SendReconnectTimeout(string reconnectType);

	void SendRegionSwitched(int currentRegion, int newRegion);

	void SendRestartDueToPlayerMigration();

	void SendRuntimeUpdate(float duration, RuntimeUpdate.Intention intention_);

	void SendSeamlessReconnectEnd(float disconnectDuration, string disconnectReason, int secSinceLastResume, int secSpentPaused);

	void SendSeamlessReconnectStart(string disconnectReason, int secSinceLastResume, int secSpentPaused);

	void SendShopBalanceAvailable(List<Balance> balances);

	void SendShopCardClick(ShopCard shopcard, string storeType, string shopTab, string shopSubTab);

	void SendShopProductDetailOpened(long productId, float loadingTime);

	void SendShopPurchaseEvent(Product product, int quantity, string currency, double amount, bool isGift, string storefront, bool purchaseComplete, string storeType, string redirectedProductId, string shopTab, string shopSubTab);

	void SendShopStatus(string error, double timeInHubSec, float timeShownClosedSec);

	void SendShopVisit(List<ShopCard> cards, string storeType, string shopTab, string shopSubTab, float loadTimeSeconds);

	void SendSmartDeckCompleteFailed(int requestMessageSize);

	void SendStartupAudioSettings(bool deviceMuted, float deviceVolume, float masterVolume, float musicVolume);

	void SendSystemDetail(UnitySystemInfo info);

	void SendTraceroute(string host, List<string> hops, int totalHops, int failHops, int successHops);

	void SendWebLoginError();

	void SendWelcomeQuestsAcknowledged(float questAckDuration);

	void SendWrongVersionContinued(string liveVersion, string updatestat);

	void SendWrongVersionFixed(string liveVersion, int reties, bool succeeded);

	void SendApkInstallFailure(string updatedVersion, string reason);

	void SendApkInstallSuccess(string updatedVersion, float availableSpaceMB, float elapsedSeconds);

	void SendApkUpdate(int installedVersion, int assetVersion, int agentVersion);

	void SendNotEnoughSpaceError(ulong availableSpace, ulong expectedOrgBytes, string filesDir, bool initial, bool localeSwitch);

	void SendNoWifi(string updatedVersion, float availableSpaceMB, float elapsedSeconds, bool initial, bool localeSwitch);

	void SendOpeningAppStore(string updatedVersion, float availableSpaceMB, float elapsedSeconds, string versionInStore);

	void SendUpdateError(uint errorCode, float elapsedSeconds);

	void SendUpdateFinished(string updatedVersion, float availableSpaceMB, float elapsedSeconds);

	void SendUpdateProgress(float duration, long realDownloadBytes, long expectedDownloadBytes);

	void SendUpdateStarted(string installedVersion, string textureFormat, string dataPath, float availableSpaceMB);

	void SendUpdateStopped(string updatedVersion, float availableSpaceMB, float elapsedSeconds, bool byUser);

	void SendUsingCellularData(string updatedVersion, float availableSpaceMB, float elapsedSeconds, bool initial, bool localeSwitch);

	void SendVersionCodeInStore(string versionCode, string countryCode);

	void SendVersionError(uint errorCode, uint agentState, string languages, string region, string branch, string additionalTags);

	void SendVersionFinished(string currentVersion, string liveVersion);

	void SendVersionStarted(int dummy);

	void Initialize(Service telemetryService);

	bool IsInitialized();

	void Enable();

	void Disable();

	void OnUpdate();

	void Shutdown();

	long EnqueueMessage(IProtoBuf message, MessageOptions options = null);

	long EnqueueMessage(string packageName, string messageName, byte[] data, MessageOptions options = null);

	void SendConnectSuccess(string name, string host = null, uint? port = null);

	void SendConnectFail(string name, string reason, string host = null, uint? port = null);

	void SendDisconnect(string name, Disconnect.Reason reason, string description = null, string host = null, uint? port = null);

	void SendFindGameResult(FindGameResult message);

	void SendConnectToGameServer(ConnectToGameServer message);

	void SendTcpQualitySample(string address4, uint port, float sampleTimeMs, uint bytesSent, uint bytesReceived, uint messagesSent, uint messagesReceived, TcpQualitySample.Metric timeSincePrevMessageMs, TcpQualitySample.Metric pingMs);
}
