using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Assets;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core;
using Blizzard.T5.Jobs;
using Blizzard.T5.Logging;
using Blizzard.T5.Services;
using Cysharp.Threading.Tasks;
using Hearthstone.DataModels;
using Hearthstone.Store;
using Hearthstone.UI;
using PegasusLuckyDraw;
using PegasusShared;
using PegasusUtil;

public class LuckyDrawManager : IService
{
	public const int NUM_HOURS_REMAINING_TO_SHOW_TIMER = 72;

	private int m_battlegroundsLuckyDrawID = -1;

	private LuckyDrawDataModel m_battlegroundsLuckyDrawDataModel;

	private LuckyDrawWidget m_luckyDrawWidget;

	private LuckyDrawButtonDataModel m_luckyDrawButtonDataModel;

	private CancellationTokenSource m_requestLuckyDrawBoxCallelationTokenSource;

	private int m_previousHammerCount = -1;

	private bool m_hasNewHammer;

	private UniTask m_RequestDataTask = UniTask.CompletedTask;

	private bool m_isInitialized;

	private ILogger m_logger;

	private event Action m_onDataUpdatedCallbacks;

	private event Action m_onEventExpiredListeners;

	public static LuckyDrawManager Get()
	{
		return ServiceManager.Get<LuckyDrawManager>();
	}

	public int NumUnacknowledgedBonusHammers()
	{
		if (m_battlegroundsLuckyDrawDataModel != null)
		{
			return m_battlegroundsLuckyDrawDataModel.NumUnacknowledgedBonusHammers;
		}
		return 0;
	}

	public int NumUnacknowledgedEarnedHammers()
	{
		if (m_battlegroundsLuckyDrawDataModel != null)
		{
			return m_battlegroundsLuckyDrawDataModel.NumUnacknowledgedEarnedHammers;
		}
		return 0;
	}

	public void SetShowHighlight(bool show)
	{
		GetLuckyDrawButtonDataModel().ShowHighlight = show;
		GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.BACON, GameSaveKeySubkeyId.BACON_SHOW_LUCKY_DRAW_BUTTON_HIGHLIGHT, show ? 1 : 0));
	}

	public bool HasActiveLuckyDrawBox()
	{
		return m_battlegroundsLuckyDrawID != -1;
	}

	private async UniTask CountDownEventTimer()
	{
		await UniTask.Yield();
		while (LuckyDrawUtils.GetLuckyDrawTimeRemaining(m_battlegroundsLuckyDrawID) > TimeSpan.Zero)
		{
			await UniTask.Delay(1000);
		}
		await UniTask.SwitchToMainThread();
		if (m_luckyDrawButtonDataModel != null)
		{
			m_luckyDrawButtonDataModel.IsEventExpired = true;
		}
		if (this.m_onEventExpiredListeners != null)
		{
			this.m_onEventExpiredListeners();
		}
	}

	public void AcknowledgeAllHammers()
	{
		if (m_luckyDrawButtonDataModel != null)
		{
			m_battlegroundsLuckyDrawDataModel.NumUnacknowledgedHammers = 0;
		}
		Network.Get().AcknowledgeLuckyDrawHammers();
	}

	public void AcknowledgeAllRewards()
	{
		Network.Get().AcknowledgeLuckyDrawRewards();
	}

	public void RegisterOnEventEndsListeners(Action action)
	{
		m_onEventExpiredListeners -= action;
		m_onEventExpiredListeners += action;
	}

	public void RemoveOnEventEndsListenders(Action action)
	{
		m_onEventExpiredListeners -= action;
	}

	public void BindLuckyDrawDataModelToWidget(Widget widget)
	{
		BindDataModelToWidget(widget, m_battlegroundsLuckyDrawDataModel);
	}

	public void BindAllLuckyDrawDataModelToWidget(Widget widget)
	{
		BindDataModelToWidget(widget, m_luckyDrawButtonDataModel);
		BindDataModelToWidget(widget, m_battlegroundsLuckyDrawDataModel);
	}

	private void BindDataModelToWidget(Widget widget, IDataModel dataModel)
	{
		if (HasActiveLuckyDrawBox())
		{
			if (widget == null)
			{
				LogError("LuckyDrawManager::BindDataModelToWidget attempted to bind model to null widget.");
			}
			else if (dataModel == null)
			{
				LogError("LuckyDrawManager::BindDataModelToWidget attempted to bind null data model to widget (" + widget.name + ")");
			}
			else
			{
				widget.BindDataModel(dataModel);
			}
		}
	}

	public void UsedFreeHammer(LuckyDrawUseHammerResponse rewardResponse)
	{
		if (rewardResponse == null)
		{
			LogError("Error [LuckyDrawManager] UsedFreeHammer() rewardResponse was null!");
			LuckyDrawUtils.ShowErrorAndReturnToLobby();
		}
		else if (rewardResponse.HasErrorCode && rewardResponse.ErrorCode != 0)
		{
			LogError($"Error [LuckyDrawManager] UsedFreeHammer() rewardResponse had error {rewardResponse.ErrorCode}");
			LuckyDrawUtils.ShowErrorAndReturnToLobby();
		}
		else if (rewardResponse.NumUnusedFreeHammersRemaining < 1)
		{
			MarkFirstHammerClaimed();
		}
	}

	public void DetermineBattlegroundsLuckyDrawBox()
	{
		m_battlegroundsLuckyDrawID = LuckyDrawUtils.GetCurrentLuckyDrawID();
	}

	public void UnregisterOnInitOrUpdateFinishedCallback(Action onFinishedCallback)
	{
		m_onDataUpdatedCallbacks -= onFinishedCallback;
	}

	public void InitializeOrUpdateData(Action onFinishedCallback = null)
	{
		if (onFinishedCallback != null)
		{
			m_onDataUpdatedCallbacks -= onFinishedCallback;
			m_onDataUpdatedCallbacks += onFinishedCallback;
		}
		if (m_RequestDataTask.Status == UniTaskStatus.Succeeded)
		{
			m_RequestDataTask = RequestUpdateDataModel();
		}
	}

	public bool IsIntialized()
	{
		return m_isInitialized;
	}

	public bool IsDataDirty()
	{
		if (m_isInitialized)
		{
			return m_RequestDataTask.Status != UniTaskStatus.Succeeded;
		}
		return true;
	}

	private void SortReponse(ref LuckyDrawBoxStateResponse response)
	{
		if (response != null)
		{
			response.Rewards.Sort((LuckyDrawReward Reward1, LuckyDrawReward Reward2) => Reward1.Id - Reward2.Id);
		}
	}

	private async UniTask RequestUpdateDataModel()
	{
		await UniTask.SwitchToMainThread();
		if (m_requestLuckyDrawBoxCallelationTokenSource != null)
		{
			m_requestLuckyDrawBoxCallelationTokenSource.Cancel();
		}
		DetermineBattlegroundsLuckyDrawBox();
		if (m_battlegroundsLuckyDrawID == -1)
		{
			m_isInitialized = true;
			m_RequestDataTask = UniTask.CompletedTask;
			return;
		}
		CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		Network network = Network.Get();
		LuckyDrawBoxStateResponse luckyDrawResponse = null;
		bool finishedGettingResponse = false;
		Network.NetHandler luckyDrawResponseHandler = delegate
		{
			finishedGettingResponse = true;
			try
			{
				luckyDrawResponse = network.GetLuckyDrawBoxStateResponse();
			}
			catch (Exception ex)
			{
				LogError("Error [LuckyDrawManager] RequestBattlegroundsLuckyDraw() - " + ex.Message);
			}
		};
		network.RegisterNetHandler(LuckyDrawBoxStateResponse.PacketID.ID, luckyDrawResponseHandler);
		network.RequestLuckyDrawBoxState(m_battlegroundsLuckyDrawID);
		m_requestLuckyDrawBoxCallelationTokenSource = cancellationTokenSource;
		while (!finishedGettingResponse)
		{
			await UniTask.Yield();
		}
		await UniTask.SwitchToMainThread();
		network.RemoveNetHandler(LuckyDrawBoxStateResponse.PacketID.ID, luckyDrawResponseHandler);
		if (cancellationTokenSource.Token.IsCancellationRequested)
		{
			if (m_requestLuckyDrawBoxCallelationTokenSource == cancellationTokenSource)
			{
				m_requestLuckyDrawBoxCallelationTokenSource = null;
			}
			cancellationTokenSource.Dispose();
			m_RequestDataTask = UniTask.CompletedTask;
			return;
		}
		if (luckyDrawResponse != null && luckyDrawResponse.ErrorCode == ErrorCode.ERROR_OK)
		{
			SortReponse(ref luckyDrawResponse);
			try
			{
				ConvertResponseToDataModel(luckyDrawResponse);
				foreach (LuckyDrawHammer luckyDrawHammer in luckyDrawResponse.Hammers)
				{
					if (luckyDrawHammer.IsUsed && luckyDrawHammer.GrantMethod_ == LuckyDrawHammer.GrantMethod.GRANT_METHOD_FREE)
					{
						MarkFirstHammerClaimed();
						break;
					}
				}
				m_hasNewHammer = false;
				if (m_previousHammerCount > 0)
				{
					m_hasNewHammer = m_previousHammerCount < m_battlegroundsLuckyDrawDataModel.Hammers;
				}
				m_previousHammerCount = m_battlegroundsLuckyDrawDataModel.Hammers;
			}
			catch (Exception ex2)
			{
				LogError("LuckyDrawManager::RequestUpdateDataModel Failed to parse BattleBash response \"" + ex2.Message + "\"");
				m_RequestDataTask = UniTask.CompletedTask;
				return;
			}
		}
		cancellationTokenSource.Dispose();
		m_requestLuckyDrawBoxCallelationTokenSource = null;
		UpdateLuckyDrawButtonDataModel();
		m_RequestDataTask = UniTask.CompletedTask;
		if (!m_isInitialized)
		{
			m_isInitialized = true;
			if (!m_luckyDrawButtonDataModel.IsEventExpired)
			{
				RegisterOnEventEndsListeners(delegate
				{
					InitializeOrUpdateData();
				});
				CountDownEventTimer().Forget();
			}
		}
		if (this.m_onDataUpdatedCallbacks != null)
		{
			this.m_onDataUpdatedCallbacks();
			this.m_onDataUpdatedCallbacks = null;
		}
	}

	public LuckyDrawDataModel GetBattlegroundsLuckyDrawDataModel()
	{
		return m_battlegroundsLuckyDrawDataModel;
	}

	public LuckyDrawButtonDataModel GetLuckyDrawButtonDataModel()
	{
		if (m_luckyDrawButtonDataModel == null)
		{
			m_luckyDrawButtonDataModel = new LuckyDrawButtonDataModel();
			UpdateLuckyDrawButtonDataModel();
		}
		return m_luckyDrawButtonDataModel;
	}

	public void UpdateLuckyDrawButtonDataModel()
	{
		LuckyDrawButtonDataModel luckyDrawButtonDataModel = GetLuckyDrawButtonDataModel();
		LuckyDrawBoxDbfRecord luckyDrawBoxDbfRecord = GameDbf.LuckyDrawBox.GetRecord(m_battlegroundsLuckyDrawID);
		if (luckyDrawBoxDbfRecord == null)
		{
			luckyDrawButtonDataModel.LuckyDrawEnabled = false;
			return;
		}
		AccountLicenseMgr accountLicenseManager = AccountLicenseMgr.Get();
		GameSaveDataManager gameSaveDataManager = GameSaveDataManager.Get();
		AccountLicenseDbfRecord accountLicenseDbfRecord = GameDbf.AccountLicense.GetRecord(luckyDrawBoxDbfRecord.AccountLicenseId);
		NetCache.NetCacheFeatures features = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		if (accountLicenseManager == null || gameSaveDataManager == null || accountLicenseDbfRecord == null || m_battlegroundsLuckyDrawDataModel == null || features == null)
		{
			luckyDrawButtonDataModel.LuckyDrawEnabled = false;
			return;
		}
		bool isCountryDisabled = GetCountryIsDisabled(features);
		bool battlePassPurchased = accountLicenseManager.OwnsAccountLicense(accountLicenseDbfRecord.LicenseId);
		luckyDrawButtonDataModel.Hammers = m_battlegroundsLuckyDrawDataModel.Hammers;
		luckyDrawButtonDataModel.LuckyDrawEnabled = features.LuckyDrawEnabled && !isCountryDisabled;
		luckyDrawButtonDataModel.BattlePassPurchased = battlePassPurchased;
		luckyDrawButtonDataModel.HasNewHammers = m_hasNewHammer;
		TimeSpan timeRemaining = LuckyDrawUtils.GetLuckyDrawTimeRemaining(m_battlegroundsLuckyDrawID);
		luckyDrawButtonDataModel.IsEventExpired = timeRemaining <= TimeSpan.Zero;
		luckyDrawButtonDataModel.HoursRemaining = 24 * timeRemaining.Days + timeRemaining.Hours;
		luckyDrawButtonDataModel.NumHoursRemainingToShowTimer = 72;
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.BACON, GameSaveKeySubkeyId.BACON_SHOW_LUCKY_DRAW_BUTTON_HIGHLIGHT, out long showLuckyDrawButtonHighlight);
		luckyDrawButtonDataModel.ShowHighlight = showLuckyDrawButtonHighlight > 0;
		LuckyDrawDataModel battlegroundsLuckyDrawDataModel = GetBattlegroundsLuckyDrawDataModel();
		bool ownsAllRewards = true;
		foreach (LuckyDrawRewardDataModel reward in battlegroundsLuckyDrawDataModel.Rewards)
		{
			if (!reward.IsOwned)
			{
				ownsAllRewards = false;
				break;
			}
		}
		luckyDrawButtonDataModel.IsAllRewardsOwned = ownsAllRewards;
	}

	private void MarkFirstHammerClaimed()
	{
		GetLuckyDrawButtonDataModel().ClaimedFirstHammer = true;
	}

	private bool GetCountryIsDisabled(NetCache.NetCacheFeatures features)
	{
		string myCountry = BattleNet.GetAccountCountry().Trim();
		string[] array = features.BattlegroundsLuckyDrawDisabledCountryCode.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Trim().Equals(myCountry))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasUnclamedFree()
	{
		if (m_battlegroundsLuckyDrawDataModel == null)
		{
			return false;
		}
		return !m_luckyDrawButtonDataModel.ClaimedFirstHammer;
	}

	private void ConvertResponseToDataModel(LuckyDrawBoxStateResponse response)
	{
		if (m_battlegroundsLuckyDrawDataModel == null)
		{
			m_battlegroundsLuckyDrawDataModel = new LuckyDrawDataModel();
		}
		if (response.HasErrorCode && response.ErrorCode != 0)
		{
			LogError($"Error: [LuckyDrawManager] ConvertResponseToDataModel() Response had error. Error: {response.ErrorCode}");
			LuckyDrawUtils.ShowErrorAndReturnToLobby();
			return;
		}
		LuckyDrawBoxDbfRecord luckyDrawBoxDBFRecord = GameDbf.LuckyDrawBox.GetRecord(m_battlegroundsLuckyDrawID);
		TimeSpan timeLeft = LuckyDrawUtils.GetLuckyDrawTimeRemaining(m_battlegroundsLuckyDrawID);
		m_battlegroundsLuckyDrawDataModel.Name = luckyDrawBoxDBFRecord.Name;
		m_battlegroundsLuckyDrawDataModel.Theme = luckyDrawBoxDBFRecord.Theme;
		if (string.IsNullOrEmpty(m_battlegroundsLuckyDrawDataModel.Theme))
		{
			m_battlegroundsLuckyDrawDataModel.Theme = "default";
		}
		m_battlegroundsLuckyDrawDataModel.Layout = luckyDrawBoxDBFRecord.Layout;
		m_battlegroundsLuckyDrawDataModel.Hammers = response.Hammers.Count((LuckyDrawHammer Hammer) => !Hammer.IsUsed);
		m_battlegroundsLuckyDrawDataModel.Rewards = GetLuckyDrawRewardDataModels(response.Rewards);
		m_battlegroundsLuckyDrawDataModel.Event = luckyDrawBoxDBFRecord.Event;
		m_battlegroundsLuckyDrawDataModel.NumUnacknowledgedHammers = response.Hammers.Count((LuckyDrawHammer Hammer) => !Hammer.IsAcknowledged);
		m_battlegroundsLuckyDrawDataModel.TimeLeft = BuildLuckyDrawTimeLeftString(timeLeft);
		m_battlegroundsLuckyDrawDataModel.TimeLeftStrPopup = GameStrings.Format("GLUE_BATTLEBASH_EVENT_POPUP_TIME_REM_DAYS", timeLeft.Days);
		m_battlegroundsLuckyDrawDataModel.TimeLeftToolTip = BuildLuckyDrawTimeLeftToolTipString(timeLeft);
		m_battlegroundsLuckyDrawDataModel.IsAllRewardsOwned = response.Rewards.Count((LuckyDrawReward Reward) => !Reward.IsOwned) == 0;
		m_battlegroundsLuckyDrawDataModel.NumUnacknowledgedBonusHammers = response.Hammers.Count((LuckyDrawHammer Hammer) => !Hammer.IsAcknowledged && Hammer.GrantMethod_ == LuckyDrawHammer.GrantMethod.GRANT_METHOD_BONUS);
		m_battlegroundsLuckyDrawDataModel.NumUnacknowledgedEarnedHammers = response.Hammers.Count((LuckyDrawHammer Hammer) => !Hammer.IsAcknowledged && Hammer.GrantMethod_ == LuckyDrawHammer.GrantMethod.GRANT_METHOD_EARNED);
		m_battlegroundsLuckyDrawDataModel.NumUnacknowledgedFreeHammers = response.Hammers.Count((LuckyDrawHammer Hammer) => !Hammer.IsAcknowledged && Hammer.GrantMethod_ == LuckyDrawHammer.GrantMethod.GRANT_METHOD_FREE);
	}

	private string BuildLuckyDrawTimeLeftString(TimeSpan timeLeft)
	{
		if (timeLeft.Days >= 1)
		{
			return GameStrings.Format("GLUE_BATTLEBASH_EVENT_TIME_REM_DAYS", timeLeft.Days);
		}
		if (timeLeft.Hours >= 1)
		{
			return GameStrings.Format("GLUE_BATTLEBASH_EVENT_TIME_REM_HOURS", timeLeft.Hours);
		}
		return GameStrings.Format("GLUE_BATTLEBASH_EVENT_TIME_REM_HOUR_OR_LESS");
	}

	private string BuildLuckyDrawTimeLeftToolTipString(TimeSpan timeLeft)
	{
		if (timeLeft.TotalDays > 1.0)
		{
			int days = (int)Math.Ceiling(timeLeft.TotalDays);
			return GameStrings.Format("GLUE_BATTLEBASH_EVENT_TIME_REM_DAYS_TOOLTIP", days);
		}
		int hours = (int)Math.Ceiling(timeLeft.TotalHours);
		return GameStrings.Format("GLUE_BATTLEBASH_EVENT_TIME_REM_HOURS_TOOLTIP", hours);
	}

	private DataModelList<LuckyDrawRewardDataModel> GetLuckyDrawRewardDataModels(List<LuckyDrawReward> luckyDrawRewards)
	{
		DataModelList<LuckyDrawRewardDataModel> rewards = new DataModelList<LuckyDrawRewardDataModel>();
		foreach (LuckyDrawReward luckyDrawReward in luckyDrawRewards)
		{
			LuckyDrawRewardsDbfRecord luckyDrawRewardsDbfRecord = GameDbf.LuckyDrawRewards.GetRecord(luckyDrawReward.Id);
			LuckyDrawRewardDataModel rewardDataModel = new LuckyDrawRewardDataModel();
			rewardDataModel.Style = GetLuckyDrawStyle(luckyDrawRewardsDbfRecord.Style);
			rewardDataModel.RewardList = RewardUtils.CreateRewardListDataModelFromRewardListId(luckyDrawRewardsDbfRecord.RewardListId);
			rewardDataModel.IsOwned = luckyDrawReward.IsOwned;
			rewardDataModel.RewardID = luckyDrawRewardsDbfRecord.ID;
			rewards.Add(rewardDataModel);
		}
		return rewards;
	}

	private LuckyDrawStyle GetLuckyDrawStyle(LuckyDrawRewards.LuckyDrawStyle style)
	{
		return style switch
		{
			LuckyDrawRewards.LuckyDrawStyle.COMMON => LuckyDrawStyle.COMMON, 
			LuckyDrawRewards.LuckyDrawStyle.LEGENDARY => LuckyDrawStyle.LEGENDARY, 
			_ => LuckyDrawStyle.COMMON, 
		};
	}

	public void UseLuckyDrawHammer(LuckyDrawWidget requestingWidget)
	{
		Network network = Network.Get();
		m_luckyDrawWidget = null;
		m_luckyDrawWidget = requestingWidget;
		network.RegisterNetHandler(LuckyDrawUseHammerResponse.PacketID.ID, OnLuckyDrawRewardReceived);
		network.UseLuckyDrawHammer();
	}

	private void OnLuckyDrawRewardReceived()
	{
		Network network = Network.Get();
		LuckyDrawUseHammerResponse rewardResponse = network.GetUseLuckyDrawHammerResponse();
		network.RemoveNetHandler(LuckyDrawUseHammerResponse.PacketID.ID, OnLuckyDrawRewardReceived);
		if (rewardResponse == null)
		{
			LogError("Error [LuckyDrawManager] OnLuckyDrawRewardReceived() rewardResponse was null!");
			LuckyDrawUtils.ShowErrorAndReturnToLobby();
		}
		else if (rewardResponse.HasErrorCode && rewardResponse.ErrorCode != 0)
		{
			LogError("Error [LuckyDrawManager] OnLuckyDrawRewardReceived() had error {rewardResponse.ErrorCode}");
			LuckyDrawUtils.ShowErrorAndReturnToLobby();
		}
		else if (rewardResponse.LuckyDrawBoxId != m_battlegroundsLuckyDrawID)
		{
			LogWarning($"Warning [LuckyDrawManager] OnLuckyDrawRewardReceived() reward box ID {rewardResponse.LuckyDrawBoxId} does not match current box ID {m_battlegroundsLuckyDrawID}");
		}
		else if (m_battlegroundsLuckyDrawDataModel == null)
		{
			LogError("Error [LuckyDrawManager] OnLuckyDrawRewardReceived() lucky draw data model is null");
		}
		else
		{
			m_battlegroundsLuckyDrawDataModel.Hammers = rewardResponse.NumUnusedHammersRemaining;
			UpdateLuckyDrawButtonDataModel();
			m_luckyDrawWidget.OnRewardResponseReceived(rewardResponse);
		}
	}

	public void OnLuckyDrawHammerAnimationFinished()
	{
		AcknowledgeAllRewards();
	}

	public void UpdateAllRewardsOwnedStatus()
	{
		if (m_battlegroundsLuckyDrawDataModel == null || m_battlegroundsLuckyDrawDataModel.Rewards == null)
		{
			return;
		}
		bool isAllRewardsOwned = true;
		foreach (LuckyDrawRewardDataModel reward in m_battlegroundsLuckyDrawDataModel.Rewards)
		{
			if (reward == null)
			{
				LogError("Error [LuckyDrawManager] UpdateAllRewardsOwnedStatus() LuckyDrawRewardDataModel is null");
			}
			else if (!reward.IsOwned)
			{
				isAllRewardsOwned = false;
				break;
			}
		}
		m_battlegroundsLuckyDrawDataModel.IsAllRewardsOwned = isAllRewardsOwned;
	}

	public bool IsBattleBashEndingSoon()
	{
		return LuckyDrawUtils.GetLuckyDrawTimeRemaining(m_battlegroundsLuckyDrawID).TotalHours <= 72.0;
	}

	private void InitLogger()
	{
		if (m_logger == null)
		{
			LogInfo logInfo = new LogInfo
			{
				m_filePrinting = true,
				m_consolePrinting = true,
				m_screenPrinting = true
			};
			m_logger = LogSystem.Get().CreateLogger("LuckyDraw", logInfo);
		}
	}

	public void LogError(string msg)
	{
		Log(Blizzard.T5.Core.LogLevel.Error, msg);
	}

	public void LogWarning(string msg)
	{
		Log(Blizzard.T5.Core.LogLevel.Warning, msg);
	}

	private void Log(Blizzard.T5.Core.LogLevel logLevel, string msg)
	{
		if (m_logger == null)
		{
			InitLogger();
		}
		m_logger.Log(logLevel, msg);
	}

	public ProductInfo GetProduct()
	{
		StoreManager.Get();
		LuckyDrawBoxDbfRecord luckyDrawBoxDbfRecord = GameDbf.LuckyDrawBox.GetRecord(m_battlegroundsLuckyDrawID);
		if (luckyDrawBoxDbfRecord == null)
		{
			return null;
		}
		if (luckyDrawBoxDbfRecord.AccountLicenseRecord == null)
		{
			return null;
		}
		int accountLicenseID = luckyDrawBoxDbfRecord.AccountLicenseRecord.ID;
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			return null;
		}
		return dataService.EnumerateBundlesForProductType(ProductType.PRODUCT_TYPE_LUCKY_DRAW, requireRealMoneyOption: false, accountLicenseID).FirstOrDefault();
	}

	public int GetActiveLuckyDrawBoxID()
	{
		return m_battlegroundsLuckyDrawID;
	}

	private void OnAccountLicenseUpdate(List<AccountLicenseInfo> changedLicensesInfo, object userData)
	{
		UpdateLuckyDrawButtonDataModel();
	}

	public Type[] GetDependencies()
	{
		return new Type[2]
		{
			typeof(Network),
			typeof(AccountLicenseMgr)
		};
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		AccountLicenseMgr.Get().RegisterAccountLicensesChangedListener(OnAccountLicenseUpdate);
		yield break;
	}

	public void Shutdown()
	{
		AccountLicenseMgr.Get().RemoveAccountLicensesChangedListener(OnAccountLicenseUpdate);
	}
}
