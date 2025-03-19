using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Services;
using Hearthstone.DataModels;
using Hearthstone.Progression;
using Hearthstone.UI;
using JetBrains.Annotations;
using PegasusLettuce;
using PegasusUtil;
using UnityEngine;

namespace Hearthstone;

[RequireComponent(typeof(WidgetTemplate))]
public class LettuceVillageTaskBoardManager : MonoBehaviour
{
	private enum TabType
	{
		None,
		Board,
		Collection
	}

	[SerializeField]
	private AsyncReference m_taskDetailReference;

	private LettuceVillageTaskDetail m_taskDetail;

	private Widget m_taskDetailWidget;

	private bool m_isTaskDetailReady;

	[SerializeField]
	private AsyncReference m_taskBoardReference;

	private LettuceVillageTaskBoard m_taskBoard;

	private bool m_isTaskBoardReady;

	[SerializeField]
	private AsyncReference m_taskCollectionReference;

	private LettuceVillageTaskCollection m_taskCollection;

	private bool m_isTaskCollectionReady;

	[SerializeField]
	private Clickable m_taskBoardTab;

	[SerializeField]
	private Clickable m_taskCollectionTab;

	[SerializeField]
	private GameObject m_taskBoardDetailBonePC;

	[SerializeField]
	private GameObject m_taskBoardDetailBoneMobile;

	[SerializeField]
	private GameObject m_taskCollectionDetailBonePC;

	[SerializeField]
	private GameObject m_taskCollectionDetailBoneMobile;

	[SerializeField]
	private GameObject m_clickBlocker;

	[SerializeField]
	private GameObject m_detailClickBlocker;

	[SerializeField]
	private CollectionSearch m_search;

	[SerializeField]
	private UberText m_helperTip;

	[SerializeField]
	private AsyncReference m_taskCollectionNotificationReference;

	private Widget m_taskCollectionNotification;

	private Widget m_widget;

	private bool m_isReady;

	[CanBeNull]
	private VisitorTaskDbfRecord m_lastClaimedTaskRecord;

	private int m_focusedVisitorId;

	private bool m_showRewardsOnDetailClose;

	private TabType m_currentTab;

	private bool m_isMapReady;

	public const string ON_OPEN = "ON_OPEN";

	public const string ENABLE_ALL_TASK_INPUT = "ENABLE_ALL_TASK_INPUT";

	public const string DISABLE_ALL_TASK_INPUT = "DISABLE_ALL_TASK_INPUT";

	private const string TASK_BOARD_TAB = "TASK_BOARD_OPEN";

	private const string TASK_COLLECTION_TAB = "TASK_COLLECTION_OPEN";

	private const string TASK_DETAIL_OPEN = "TASK_DETAIL_OPEN";

	private const string TASK_DETAIL_CLOSE = "TASK_DETAIL_CLOSE";

	private const string REQUEST_RENOWN_CONVERSION = "TRY_OPEN_RENOWN_CONVERSION";

	private Coroutine m_waitForReadyCoroutine;

	private RewardPresenter m_rewardPresenter = new RewardPresenter();

	private List<RewardData> m_equipmentRewardsToShow = new List<RewardData>();

	[CanBeNull]
	public MercenaryVillageTaskItemDataModel LastAcknowledgedTaskData { get; private set; }

	public bool IsProcessing
	{
		get
		{
			if (!(m_taskDetail != null))
			{
				return false;
			}
			return m_taskDetail.IsProcessing();
		}
	}

	public bool IsShowingRewards { get; private set; }

	private void Awake()
	{
		m_widget = GetComponent<Widget>();
		m_widget.RegisterEventListener(HandleEvent);
		m_search.SetActiveLayer(GameLayer.Reserved29);
		m_search.RegisterActivatedListener(OnSearchActivated);
		m_search.RegisterDeactivatedListener(OnSearchDeactivated);
		m_search.RegisterClearedListener(OnSearchCleared);
	}

	private void OnEnable()
	{
		BnetBar.Get()?.RefreshCurrency();
	}

	private void Start()
	{
		NetCache.Get().RegisterUpdatedListener(typeof(NetCache.NetCacheMercenariesVillageVisitorInfo), OnUpdatedTaskData);
		NetCache.Get().RegisterUpdatedListener(typeof(NetCache.NetCacheMercenariesVillageInfo), OnVillageInfoUpdated);
		Network.Get().RegisterNetHandler(MercenariesClaimTaskResponse.PacketID.ID, OnClaimTaskResponseReceived);
		Network.Get().RegisterNetHandler(MercenariesDismissTaskResponse.PacketID.ID, OnDismissTaskResponseReceived);
		Network.Get().RegisterNetHandler(MercenariesDismissRenownOfferResponse.PacketID.ID, OnDismissRenownOfferResponseReceived);
		Network.Get().RegisterNetHandler(MercenariesPurchaseRenownOfferResponse.PacketID.ID, OnPurchaseRenownOfferResponseReceived);
		if (SceneMgr.Get().GetMode() != SceneMgr.Mode.LETTUCE_MAP || NetCache.Get().IsNetObjectAvailable<NetCache.NetCacheLettuceMap>())
		{
			m_isMapReady = true;
		}
		else
		{
			NetCache.Get().RegisterUpdatedListener(typeof(NetCache.NetCacheLettuceMap), OnUpdatedMapData);
		}
		m_taskDetailReference.RegisterReadyListener<Widget>(OnTaskDetailReady);
		m_taskBoardReference.RegisterReadyListener<Widget>(OnTaskBoardReady);
		m_taskCollectionReference.RegisterReadyListener<Widget>(OnTaskCollectionReady);
		m_taskCollectionNotificationReference.RegisterReadyListener<Widget>(OnTaskCollectionNotificationReady);
		m_taskBoardTab.AddEventListener(UIEventType.RELEASE, OnTaskBoardTabClicked);
		m_taskCollectionTab.AddEventListener(UIEventType.RELEASE, OnTaskCollectionTabClicked);
		m_waitForReadyCoroutine = StartCoroutine(WaitForReady());
	}

	private void OnDestroy()
	{
		if (NetCache.Get() != null)
		{
			NetCache.Get().RemoveUpdatedListener(typeof(NetCache.NetCacheMercenariesVillageVisitorInfo), OnUpdatedTaskData);
			NetCache.Get().RemoveUpdatedListener(typeof(NetCache.NetCacheMercenariesVillageInfo), OnVillageInfoUpdated);
			NetCache.Get().RemoveUpdatedListener(typeof(NetCache.NetCacheLettuceMap), OnUpdatedMapData);
		}
		if (Network.Get() != null)
		{
			Network.Get().RemoveNetHandler(MercenariesClaimTaskResponse.PacketID.ID, OnClaimTaskResponseReceived);
			Network.Get().RemoveNetHandler(MercenariesDismissTaskResponse.PacketID.ID, OnDismissTaskResponseReceived);
			Network.Get().RemoveNetHandler(MercenariesDismissRenownOfferResponse.PacketID.ID, OnDismissRenownOfferResponseReceived);
			Network.Get().RemoveNetHandler(MercenariesPurchaseRenownOfferResponse.PacketID.ID, OnPurchaseRenownOfferResponseReceived);
		}
		if (m_waitForReadyCoroutine != null)
		{
			StopCoroutine(m_waitForReadyCoroutine);
		}
	}

	private IEnumerator WaitForReady()
	{
		while (!m_isTaskDetailReady)
		{
			yield return null;
		}
		while (!m_isTaskBoardReady)
		{
			yield return null;
		}
		while (!m_isTaskCollectionReady)
		{
			yield return null;
		}
		while (!m_isMapReady)
		{
			yield return null;
		}
		OnUpdatedTaskData();
		while (!m_taskBoard.IsDataModelReady())
		{
			yield return null;
		}
		while (!m_taskCollection.IsReady())
		{
			yield return null;
		}
		m_isReady = true;
		m_waitForReadyCoroutine = null;
		if (!TryFocusVisitor(m_focusedVisitorId))
		{
			OnTaskBoardTabClicked(null);
		}
	}

	public bool IsTaskBoardReady()
	{
		return m_isReady;
	}

	private void HandleEvent(string eventName)
	{
		switch (eventName)
		{
		case "ON_OPEN":
			RefreshHelperTip();
			break;
		case "TASK_CLICKED":
			LastAcknowledgedTaskData = m_widget.GetDataModel<EventDataModel>().Payload as MercenaryVillageTaskItemDataModel;
			LettuceVillagePopupManager.Get().HideHelpPopupsInVillage();
			break;
		case "SHRINK_TASK_DETAIL":
			DimissTaskDetail();
			break;
		case "TASK_DETAIL_CLOSE":
			if (m_showRewardsOnDetailClose)
			{
				m_showRewardsOnDetailClose = false;
				ShowTaskRewards();
			}
			LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VIEW_TASKS_POPUP, LettuceVillagePopupManager.Get().GetTutorialGameObject());
			break;
		case "TRY_OPEN_RENOWN_CONVERSION":
			m_taskDetail.SetRequestRenownConversion(isRequested: false);
			LettuceVillagePopupManager.Get().Show(LettuceVillagePopupManager.PopupType.RENOWNCONVERSION);
			break;
		case "TASK_COLLECTION_OPEN":
			LettuceVillagePopupManager.Get().HideHelpPopupsInVillage();
			LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_TASK_COLLECTION_POPUP, base.gameObject);
			LettuceTutorialUtils.ForceCompleteEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VIEW_TASKS_POPUP);
			m_taskCollectionNotification?.Hide();
			break;
		}
	}

	public void ResetPopup()
	{
		m_clickBlocker.SetActive(value: false);
		m_detailClickBlocker.SetActive(value: false);
	}

	public bool TryFocusVisitor(int visitorId)
	{
		m_focusedVisitorId = visitorId;
		if (m_focusedVisitorId <= 0)
		{
			return false;
		}
		if (!m_isReady)
		{
			return false;
		}
		if (m_taskBoard.TryGetDataModel(m_focusedVisitorId, out var data))
		{
			OnTaskBoardTabClicked(null);
		}
		else
		{
			if (!m_taskCollection.TryGetDataModel(m_focusedVisitorId, out data))
			{
				return false;
			}
			OnTaskCollectionTabClicked(null);
		}
		return true;
	}

	public void HandleTutorialPromptsOnOpen()
	{
		GameObject tutorialObject = LettuceVillagePopupManager.Get().GetTutorialGameObject();
		LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_TASK_BOARD_END, tutorialObject);
		if ((bool)m_taskCollectionNotification)
		{
			if (LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VIEW_TASKS_POPUP) && !LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_TASK_COLLECTION_POPUP))
			{
				m_taskCollectionNotification.Show();
			}
			else
			{
				m_taskCollectionNotification.Hide();
			}
		}
	}

	private void OnClaimTaskResponseReceived()
	{
		MercenariesClaimTaskResponse response = Network.Get().ClaimMercenaryTaskResponse();
		m_taskDetail.SetProcessing(isProcessing: false);
		if (response.Success)
		{
			NetCache.NetCacheMercenariesVillageVisitorInfo visitorInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesVillageVisitorInfo>();
			if (visitorInfo != null && visitorInfo.CompletedTasks != null)
			{
				foreach (MercenariesTaskState completedTask in visitorInfo.CompletedTasks)
				{
					if (completedTask.HasTaskId && completedTask.TaskId == response.TaskId)
					{
						visitorInfo.CompletedTasks.Remove(completedTask);
						break;
					}
				}
			}
			if (m_taskDetail.ShouldCloseAfterClaim())
			{
				m_showRewardsOnDetailClose = true;
				DimissTaskDetail();
			}
			int taskId = response.TaskId;
			VisitorTaskDbfRecord taskRecord = (m_lastClaimedTaskRecord = LettuceVillageDataUtil.GetTaskRecordByID(taskId));
			if (taskRecord == null)
			{
				Log.Lettuce.PrintError($"error in OnClaimTaskResponseReceived: task record is null for task id {taskId}");
				return;
			}
			LettuceVillageDataUtil.RecentlyClaimedTaskId = taskRecord.ID;
			MercenaryVisitorDbfRecord visitorRecord = LettuceVillageDataUtil.GetVisitorRecordByID(taskRecord.MercenaryVisitorId);
			if (visitorRecord != null)
			{
				LettuceVillageDataUtil.CurrentTaskContext = LettuceVillageDataUtil.GetMercenaryIdForVisitor(visitorRecord, taskRecord);
			}
			QueueTaskRewards(taskId, response.TaskReward, taskRecord);
			if (!m_showRewardsOnDetailClose)
			{
				ShowTaskRewards();
			}
			LettuceVillageDataUtil.RemoveCompletedOrDismissedTaskDialogue(taskId);
		}
		else
		{
			m_rewardPresenter.Clear();
			ShowError("GLUE_LETTUCE_VILLAGE_CLAIM_TASK_ERROR_DESCRIPTION", GeneralUtils.noOp);
		}
	}

	private void OnDismissTaskResponseReceived()
	{
		MercenariesDismissTaskResponse mercenariesDismissTaskResponse = Network.Get().DismissMercenaryTaskResponse();
		m_taskDetail.SetProcessing(isProcessing: false);
		if (mercenariesDismissTaskResponse.Success)
		{
			DimissTaskDetail();
			return;
		}
		Log.Lettuce.PrintError("Failed to dismiss task after player request");
		ShowError("GLUE_LETTUCE_VILLAGE_DISMISS_TASK_ERROR_DESCRIPTION", delegate
		{
			DimissTaskDetail();
		});
	}

	private void OnDismissRenownOfferResponseReceived()
	{
		MercenariesDismissRenownOfferResponse mercenariesDismissRenownOfferResponse = Network.Get().DismissRenownOfferResponse();
		m_taskDetail.SetProcessing(isProcessing: false);
		if (mercenariesDismissRenownOfferResponse.Success)
		{
			DimissTaskDetail();
			return;
		}
		Log.Lettuce.PrintError("Failed to dismiss renown offer after player request");
		ShowError("GLUE_LETTUCE_VILLAGE_DISMISS_OFFER_ERROR_DESCRIPTION", delegate
		{
			DimissTaskDetail();
		});
	}

	private void OnPurchaseRenownOfferResponseReceived()
	{
		MercenariesPurchaseRenownOfferResponse response = Network.Get().PurchaseRenownOfferResponse();
		m_taskDetail.SetProcessing(isProcessing: false);
		if (response.Success)
		{
			ServiceManager.Get<CurrencyManager>().RefreshWallet();
			m_showRewardsOnDetailClose = true;
			DimissTaskDetail();
			QueueOfferRewards((int)response.RenownOfferId);
		}
		else
		{
			Log.Lettuce.PrintError("Failed to purchase renown offer after player request");
			ShowError("GLUE_LETTUCE_VILLAGE_CLAIM_OFFER_ERROR_DESCRIPTION", delegate
			{
				DimissTaskDetail();
			});
		}
	}

	private void OnVillageInfoUpdated()
	{
		if (m_isReady)
		{
			m_taskBoard.OnVillageInfoUpdated();
		}
	}

	private void OnUpdatedTaskData()
	{
		if (m_isTaskBoardReady && m_isTaskDetailReady && m_isTaskCollectionReady)
		{
			m_taskBoard.Refresh();
			m_taskCollection.Refresh();
			m_taskDetail.PreloadTaskBarkVo(m_taskBoard.GetCurrentVisitors());
		}
	}

	private void OnUpdatedMapData()
	{
		m_isMapReady = true;
	}

	private void ShowTaskDetail(MercenaryVillageTaskItemDataModel taskData = null)
	{
		m_taskDetailWidget.TriggerEvent("OPEN_TASK_DETAIL", new TriggerEventParameters(null, taskData));
	}

	private void DimissTaskDetail()
	{
		m_taskDetailWidget.TriggerEvent("SHRINK_TASK_DETAIL");
	}

	private void OnTaskDetailReady(Widget widget)
	{
		m_isTaskDetailReady = true;
		if (widget == null)
		{
			Log.Lettuce.PrintError("Task detail widget could not be found.");
			return;
		}
		m_taskDetailWidget = widget;
		m_taskDetail = widget.GetComponentInChildren<LettuceVillageTaskDetail>();
		if (m_taskDetail == null)
		{
			Log.Lettuce.PrintError("LettuceVillageTaskDetail component could not be found on the referenced TaskDetail widget.");
			return;
		}
		m_taskDetailWidget.RegisterActivatedListener(delegate
		{
			ShowTaskDetail(LastAcknowledgedTaskData);
		});
	}

	private void OnTaskBoardReady(Widget widget)
	{
		m_isTaskBoardReady = true;
		if (widget == null)
		{
			Log.Lettuce.PrintError("Task board widget could not be found.");
			return;
		}
		m_taskBoard = widget.GetComponentInChildren<LettuceVillageTaskBoard>();
		if (m_taskBoard == null)
		{
			Log.Lettuce.PrintError("LettuceVillageTaskBoard component could not be found on the referenced TaskBoard widget.");
		}
	}

	private void OnTaskBoardTabClicked(UIEvent e)
	{
		if (m_isReady)
		{
			m_currentTab = TabType.Board;
			RefreshHelperTip();
			m_taskCollection.OnClose();
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				m_taskDetail.SetBone(m_taskBoardDetailBoneMobile);
			}
			else
			{
				m_taskDetail.SetBone(m_taskBoardDetailBonePC);
			}
			m_widget.TriggerEvent("TASK_BOARD_OPEN");
		}
	}

	private void OnTaskCollectionReady(Widget widget)
	{
		m_isTaskCollectionReady = true;
		if (widget == null)
		{
			Log.Lettuce.PrintError("Task collection widget could not be found.");
			return;
		}
		m_taskCollection = widget.GetComponentInChildren<LettuceVillageTaskCollection>();
		if (m_taskCollection == null)
		{
			Log.Lettuce.PrintError("LettuceVillageTaskCollection component could not be found on the referenced TaskBoard widget.");
		}
	}

	private void OnTaskCollectionTabClicked(UIEvent e)
	{
		if (m_isReady)
		{
			m_currentTab = TabType.Collection;
			RefreshHelperTip();
			m_taskCollection.OnOpen();
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				m_taskDetail.SetBone(m_taskCollectionDetailBoneMobile);
			}
			else
			{
				m_taskDetail.SetBone(m_taskCollectionDetailBonePC);
			}
			m_widget.TriggerEvent("TASK_COLLECTION_OPEN");
		}
	}

	private void OnTaskCollectionNotificationReady(Widget widget)
	{
		if (widget == null)
		{
			Log.Lettuce.PrintError("The collection notification widget could not be found.");
			return;
		}
		m_taskCollectionNotification = widget;
		m_taskCollectionNotification.Hide();
	}

	private void OnSearchActivated()
	{
		m_taskCollection.OnSearchActivated();
	}

	private void OnSearchDeactivated(string oldSearchText, string newSearchText)
	{
		m_taskCollection.OnSearchDeactivated(oldSearchText, newSearchText);
	}

	private void OnSearchCleared(bool updateVisuals)
	{
		m_taskCollection.OnSearchCleared(updateVisuals);
	}

	private void RefreshHelperTip()
	{
		string helperTipText = "GLUE_LETTUCE_VILLAGE_TASK_BOARD_HELPER_TIP";
		switch (m_currentTab)
		{
		case TabType.Board:
			helperTipText = "GLUE_LETTUCE_VILLAGE_TASK_BOARD_HELPER_TIP";
			break;
		case TabType.Collection:
			helperTipText = (((SceneMgr.Get()?.GetMode() ?? SceneMgr.Mode.INVALID) != SceneMgr.Mode.LETTUCE_MAP) ? "GLUE_LETTUCE_VILLAGE_TASK_COLLECTION_HELPER_TIP" : "GLUE_LETTUCE_VILLAGE_TASK_COLLECTION_MAP_HELPER_TIP");
			break;
		}
		m_helperTip.Text = GameStrings.Get(helperTipText);
	}

	private void ShowError(string descriptionKey, Action callback = null)
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
		{
			m_headerText = GameStrings.Get("GLUE_LETTUCE_VILLAGE_TASK_HEADER"),
			m_text = GameStrings.Get(descriptionKey),
			m_showAlertIcon = true,
			m_responseDisplay = AlertPopup.ResponseDisplay.OK,
			m_responseCallback = delegate
			{
				callback?.Invoke();
			}
		};
		DialogManager.Get().ShowPopup(info);
	}

	public void QueueTaskRewards(int taskId, IEnumerable<RewardItemOutput> TaskRewards, VisitorTaskDbfRecord taskRecord)
	{
		if (taskRecord == null)
		{
			Log.Lettuce.PrintError("Error in OnClaimTaskResponseReceived: task record is null for task id {0}", taskId);
			return;
		}
		m_equipmentRewardsToShow = new List<RewardData>();
		foreach (RewardItemOutput taskReward in TaskRewards)
		{
			RewardItemDbfRecord record = GameDbf.RewardItem.GetRecord(taskReward.RewardItemId);
			if (record == null)
			{
				Log.Lettuce.PrintError("error in OnClaimTaskResponseReceived: no record for reward item id {0}", taskReward.RewardItemId);
			}
			else
			{
				RewardFactory.CreateRewardItemDataModel(record, taskReward.OutputData);
				AddRewardToQueue(taskReward, taskRecord, record);
			}
		}
	}

	public void QueueOfferRewards(int offerId)
	{
		MercenariesRenownOfferData lastPurchaseRequestedOffer = LettuceRenownUtil.LastPurchaseRequestedOffer;
		if (offerId != lastPurchaseRequestedOffer?.RenownOfferId)
		{
			Log.Lettuce.PrintError($"Cannot queue renown offer rewards for {offerId} - Unknown offer");
		}
		else
		{
			AddRewardToQueue(lastPurchaseRequestedOffer);
		}
	}

	public void ShowTaskRewards()
	{
		IsShowingRewards = true;
		if (!m_rewardPresenter.ShowNextReward(ShowTaskRewards))
		{
			if (m_equipmentRewardsToShow.Count > 0)
			{
				RewardUtils.LoadAndDisplayRewards(m_equipmentRewardsToShow, OnTaskRewardsDismissed);
			}
			else
			{
				OnTaskRewardsDismissed();
			}
			m_equipmentRewardsToShow = new List<RewardData>();
		}
	}

	private void AddRewardToQueue(RewardItemOutput taskReward, VisitorTaskDbfRecord taskRecord, RewardItemDbfRecord rewardRecord)
	{
		MercenaryVisitorDbfRecord visitorRecordByID = LettuceVillageDataUtil.GetVisitorRecordByID(taskRecord.MercenaryVisitorId);
		int taskChainIndex = m_taskDetail.CurrentTaskData.TaskChainIndex;
		string rewardTitle = "";
		(taskChainIndex + 1).ToString();
		switch (visitorRecordByID.VisitorType)
		{
		case MercenaryVisitor.VillageVisitorType.EVENT:
		case MercenaryVisitor.VillageVisitorType.SPECIAL:
			rewardTitle = GameStrings.Format("GLUE_LETTUCE_REWARD_MERCENARY_LEGENDARY_TASK_COMPLETE", m_taskDetail.CurrentTaskData.Title);
			break;
		case MercenaryVisitor.VillageVisitorType.STANDARD:
		case MercenaryVisitor.VillageVisitorType.PROCEDURAL:
			rewardTitle = GameStrings.Format("GLUE_LETTUCE_REWARD_MERCENARY_NORMAL_TASK_COMPLETE", m_taskDetail.CurrentTaskData.Title);
			break;
		}
		string rewardDescription = m_taskDetail.CurrentTaskData.Description;
		List<RewardItemDataModel> items = RewardFactory.CreateRewardItemDataModel(rewardRecord, taskReward.OutputData);
		if (items.Count <= 0)
		{
			return;
		}
		RewardItemDataModel item = items[0];
		if (items.Count > 1)
		{
			Debug.LogWarning("LettuceVillageTaskBoard.AddRewardToQueue: RewardFactory generated extra rewards that are not being handled");
		}
		switch (rewardRecord.RewardType)
		{
		case RewardItem.RewardType.MERCENARY_EQUIPMENT:
		{
			LettuceEquipmentDbfRecord equipmentRecord = GameDbf.LettuceEquipment.GetRecord((LettuceEquipmentDbfRecord r) => r.ID == item.MercenaryEquip.AbilityId);
			if (equipmentRecord != null && equipmentRecord.LettuceEquipmentTiers.Count > 0)
			{
				m_equipmentRewardsToShow.Add(new MercenariesEquipmentRewardData(item.Mercenary.MercenaryId, item.MercenaryEquip.AbilityId, equipmentRecord.LettuceEquipmentTiers[0].Tier));
			}
			break;
		}
		case RewardItem.RewardType.MERCENARY_CURRENCY:
			rewardDescription = ((!item.MercenaryCoin.IsRandom) ? GameStrings.Format("GLUE_LETTUCE_REWARD_MERCENARY_TASK_COINS_REWARD", item.MercenaryCoin.Quantity, item.MercenaryCoin.MercenaryName) : GameStrings.Format("GLUE_LETTUCE_REWARD_MERCENARY_TASK_RANDOM_COINS_REWARD", item.MercenaryCoin.Quantity, item.MercenaryCoin.MercenaryName));
			EnqueueScrollReward(rewardTitle, rewardDescription, item);
			break;
		case RewardItem.RewardType.MERCENARY:
			rewardDescription = ((item.ItemType != RewardItemType.MERCENARY_COIN) ? GameStrings.Format("GLUE_LETTUCE_REWARD_MERCENARY_TASK_MERCENARY_REWARD", item.Mercenary.MercenaryName) : GameStrings.Format("GLUE_LETTUCE_REWARD_MERCENARY_TASK_COINS_REWARD", item.MercenaryCoin.Quantity, item.MercenaryCoin.MercenaryName));
			EnqueueScrollReward(rewardTitle, rewardDescription, item);
			break;
		case RewardItem.RewardType.GOLD:
			rewardDescription = GameStrings.Format("GLUE_LETTUCE_REWARD_MERCENARY_TASK_GOLD_REWARD", item.Quantity);
			EnqueueScrollReward(rewardTitle, rewardDescription, item);
			break;
		case RewardItem.RewardType.BOOSTER:
			rewardDescription = GameStrings.Format("GLUE_LETTUCE_REWARD_MERCENARY_TASK_BOOSTER_REWARD", item.Quantity);
			EnqueueScrollReward(rewardTitle, rewardDescription, item);
			break;
		}
	}

	private void AddRewardToQueue(MercenariesRenownOfferData offerData)
	{
		if (CollectionManager.Get().GetMercenary(offerData.MercenaryId, AttemptToGenerate: true) == null)
		{
			Log.Lettuce.PrintError("Unknown mercenary in offer. Will not be displayed");
			return;
		}
		string rewardTitle = GameStrings.Get("GLUE_LETTUCE_REWARD_OFFER_CLAIMED");
		string rewardDescription;
		RewardItemDataModel item = LettuceRenownUtil.CreateRenownOfferRewardDataModel(offerData, out rewardDescription);
		if (item == null)
		{
			Log.Lettuce.PrintError("Unknown reward renown offer type. Will not be displayed");
		}
		else
		{
			EnqueueScrollReward(rewardTitle, rewardDescription, item);
		}
	}

	private void EnqueueScrollReward(string title, string description, RewardItemDataModel item)
	{
		RewardListDataModel rewards = new RewardListDataModel();
		rewards.Items.Add(item);
		RewardScrollDataModel rewardScrollDataModel = new RewardScrollDataModel
		{
			DisplayName = title,
			Description = description,
			RewardList = rewards
		};
		m_rewardPresenter.EnqueueReward(rewardScrollDataModel, GeneralUtils.noOp);
	}

	private void OnTaskRewardsDismissed()
	{
		IsShowingRewards = false;
		if (m_lastClaimedTaskRecord != null)
		{
			NarrativeManager.Get().OnVillageTaskClaimed(m_lastClaimedTaskRecord);
		}
		m_taskDetail.OnTaskRewardsDismissed();
		LettuceVillageDataUtil.CurrentTaskContext = 0;
	}
}
