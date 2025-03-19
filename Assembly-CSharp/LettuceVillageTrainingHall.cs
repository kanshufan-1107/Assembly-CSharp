using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Hearthstone;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusLettuce;
using PegasusShared;
using UnityEngine;

public class LettuceVillageTrainingHall : MonoBehaviour
{
	[Flags]
	private enum TrainingHallFlags : long
	{
		SLOT_2_DONE_SHOWING_NEW_DECORATION = 1L
	}

	public delegate void OnCardDroppedCallback();

	private struct TrainingMetrics
	{
		public int numSlotsAvailable;

		public int expPerHour;

		public int maxExpGained;

		public int minTrainingTime;
	}

	public const string SHOW_MERC_PORTRAIT_WHEN_DRAGGING_EVENT = "HOLD_MERC_OVER_TRAINING_code";

	public const string SHOW_MERC_TILE_WHEN_DRAGGING_EVENT = "HOLD_MERC_OVER_TEAM_TRAY_code";

	public const string MERC_DROPPED_IN_SLOT_1_EVENT = "MERC_DROPPED_SLOT_1";

	public const string MERC_DROPPED_IN_SLOT_2_EVENT = "MERC_DROPPED_SLOT_2";

	public const string MERC_DROPPED_IN_LIST_EVENT = "MERC_DROPPED_LIST";

	public const string ENABLE_INPUT_EVENT = "UNBLOCK_SCREEN";

	public const string DISABLE_INPUT_EVENT = "BLOCK_SCREEN";

	public const string MAKE_VISIBLE = "MAKE_VISIBLE";

	public const string MAKE_HIDDEN = "MAKE_HIDDEN";

	public const string MERC_LOADOUT_RELEASED = "MERC_LOADOUT_RELEASED";

	public const string TEAM_MERC_drag_started = "TEAM_MERC_drag_started";

	public const string SLOT_BUTTON_PRESSED = "SLOT_BUTTON_PRESSED";

	public const int FALLBACK_MAX_LEVEL = 30;

	public const float TIME_UNTIL_SERVER_TIMEOUT = 30f;

	public const int MAX_MERCS_TO_LOAD_AT_ONCE = 10;

	public const float TIME_TO_LOAD_NEXT_MERC_BATCH = 0.1f;

	public AsyncReference m_listVCReference;

	public AsyncReference m_draggableReference;

	public AsyncReference m_searchReference;

	public AsyncReference m_trainingWindowReference;

	public AsyncReference m_screenBlockerReference;

	public AsyncReference m_trainingSlot1;

	public AsyncReference m_trainingSlot2;

	[SerializeField]
	private Collider m_dragPlaneCollider;

	[SerializeField]
	private Collider m_dropZoneCollider;

	[SerializeField]
	private GameObject m_expRewardContainer;

	[SerializeField]
	private float m_progressUpdateFrequencyInSeconds = 60f;

	private Widget m_widget;

	private Widget m_trainingWindowWidget;

	private Widget m_screenBlocker;

	private LettuceTrainingHallPopupDataModel m_dataModel;

	private UIBScrollable m_scrollbar;

	private VisualController m_listVC;

	private Listable m_mercListable;

	private Widget m_mercenariesDraggablesWidget;

	private Vector3 m_offScreenPosition;

	public OnCardDroppedCallback m_cardDroppedCallback;

	private GameObject m_dragColliderRoot;

	private LettuceMercenaryDataModel m_draggedMerc;

	private bool m_draggedMercIsMaxLevel;

	private string m_searchText;

	private float m_responseTimeout;

	private float m_timeToUpdateProgress;

	private bool m_processingUpdate;

	private bool m_didInitialLoad;

	private bool m_isShowingMercenariesExperienceRewards;

	private bool m_draggedMercIsOverTrainingArea;

	private TrainingMetrics m_trainingMetrics;

	private int m_lastSlotInteracted;

	private long m_collectedMercInitialExp = -1L;

	private List<long> m_GSDMercenaries = new List<long> { 0L, 0L };

	private Dictionary<int, LettuceMercenaryDataModel> m_mercenaryDMCache = new Dictionary<int, LettuceMercenaryDataModel>();

	private List<int> m_disabledMercenaries;

	private List<LettuceMercenary> m_mercenariesToLoad;

	private void Start()
	{
		m_dragColliderRoot = m_dragPlaneCollider.gameObject;
		m_widget = GetComponent<Widget>();
		m_dragColliderRoot.SetActive(value: false);
		m_widget.RegisterEventListener(HandleEvent);
		ValidateGSDState();
		GenerateTrainingMetrics();
		SetUpDataModel();
		m_listVCReference.RegisterReadyListener<VisualController>(OnListSubwidgetReady);
		m_draggableReference.RegisterReadyListener<Widget>(OnMercenariesDraggablesReady);
		m_searchReference.RegisterReadyListener<CollectionSearch>(OnSearchReady);
		m_trainingWindowReference.RegisterReadyListener<Widget>(OnTrainingWindowReady);
		m_screenBlockerReference.RegisterReadyListener<Widget>(OnScreenBlockerReady);
		m_trainingSlot1.RegisterReadyListener<Widget>(OnTrainingSlot1Ready);
		m_trainingSlot2.RegisterReadyListener<Widget>(OnTrainingSlot2Ready);
		CollectionManager.Get().OnMercenariesTrainingAddResponseReceived += OnMercenariesTrainingAddResponse;
		CollectionManager.Get().OnMercenariesTrainingRemoveResponseReceived += OnMercenariesTrainingRemoveResponse;
		CollectionManager.Get().OnMercenariesTrainingCollectResponseReceived += OnMercenariesTrainingCollectResponse;
	}

	private void HandleEvent(string eventName)
	{
		switch (eventName)
		{
		case "MAKE_VISIBLE":
			OnShown();
			break;
		case "MAKE_HIDDEN":
			OnHidden();
			break;
		case "MERC_LOADOUT_RELEASED":
			MercDropped();
			break;
		case "TEAM_MERC_drag_started":
			OnMercDragStarted();
			break;
		case "SLOT_BUTTON_PRESSED":
		{
			EventDataModel eventDataModel = m_widget.GetDataModel<EventDataModel>();
			if (eventDataModel.Payload is IConvertible)
			{
				int slot = Convert.ToInt32(eventDataModel.Payload);
				OnSlotButtonPressed(slot);
			}
			break;
		}
		}
	}

	private void OnShown()
	{
		m_dataModel.IsPopupVisible = true;
		GenerateTrainingMetrics();
		AddTrainingDataToDataModel();
		UpdateDataModelForSlot(0, progressOnly: false, checkMercExp: false, 0L);
		UpdateDataModelForSlot(1, progressOnly: false, checkMercExp: false, 0L);
		PopulateMercList();
		if (LettuceVillagePopupManager.Get() != null && LettuceVillagePopupManager.Get().GetTutorialGameObject() != null)
		{
			LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_BUILDING_START_TRAINING, LettuceVillagePopupManager.Get().GetTutorialGameObject());
		}
	}

	private void OnHidden()
	{
		m_dataModel.IsPopupVisible = false;
		m_responseTimeout = 0f;
		m_timeToUpdateProgress = 0f;
		if (!m_dataModel.Slot2.Locked)
		{
			SetTrainingHallFlag(TrainingHallFlags.SLOT_2_DONE_SHOWING_NEW_DECORATION, valueToSet: true);
		}
		LettuceVillagePopupManager.Get().OnPopupClosed?.Invoke(LettuceVillagePopupManager.PopupType.TRAININGHALL);
	}

	private void Update()
	{
		if (m_responseTimeout > 0f && Time.time > m_responseTimeout)
		{
			m_responseTimeout = 0f;
			m_processingUpdate = false;
			PopupDisplayManager.SuppressPopupsTemporarily = false;
			m_dataModel.ErrorText = GameStrings.Get("GLUE_LETTUCE_VILLAGE_TIMEOUT_ERROR");
		}
		if (Time.time > m_timeToUpdateProgress && m_timeToUpdateProgress > 0f)
		{
			UpdateDataModelForSlot(0, progressOnly: true, checkMercExp: false, 0L);
			UpdateDataModelForSlot(1, progressOnly: true, checkMercExp: false, 0L);
		}
		LettuceTrainingHallPopupDataModel dataModel = m_dataModel;
		if (dataModel != null && dataModel.IsPlayerDragging)
		{
			UpdateDraggedMerc();
		}
	}

	private void SetUpDataModel()
	{
		m_dataModel = new LettuceTrainingHallPopupDataModel();
		m_dataModel.Slot1 = new LettuceTrainingHallSlotDataModel();
		m_dataModel.Slot2 = new LettuceTrainingHallSlotDataModel();
		AddTrainingDataToDataModel();
		UpdateDataModelForSlot(0, progressOnly: false, checkMercExp: false, 0L);
		UpdateDataModelForSlot(1, progressOnly: false, checkMercExp: false, 0L);
		m_dataModel.IsPopupVisible = false;
		m_widget.BindDataModel(m_dataModel);
	}

	private void UpdateDataModelForSlot(int slot = 0, bool progressOnly = false, bool checkMercExp = false, long xpAwarded = 0L)
	{
		if (m_dataModel == null)
		{
			return;
		}
		LettuceTrainingHallSlotDataModel slotData = ((slot == 0) ? m_dataModel.Slot1 : m_dataModel.Slot2);
		slotData.Locked = m_trainingMetrics.numSlotsAvailable <= slot;
		slotData.SlotIndex = slot;
		slotData.MaxExp = m_trainingMetrics.maxExpGained;
		slotData.PreparationTime = m_trainingMetrics.minTrainingTime;
		if (slot == 1 && !slotData.Locked && !GetTrainingHallFlag(TrainingHallFlags.SLOT_2_DONE_SHOWING_NEW_DECORATION))
		{
			slotData.IsNewlyUnlocked = true;
		}
		else
		{
			slotData.IsNewlyUnlocked = false;
		}
		LettuceMercenary mercInTraining = GetMercenaryInSlot(slot);
		if (mercInTraining == null)
		{
			slotData.SlotIsEmpty = true;
			slotData.ShowAnimatedTraining = false;
			slotData.TrainingIsComplete = false;
			slotData.TotalTimeInTraining = 0;
			slotData.MercIsMaxLevel = false;
			slotData.Mercenary = null;
			slotData.Progress = 0;
			return;
		}
		Date startTime = mercInTraining.m_trainingStartDate;
		slotData.SlotIsEmpty = false;
		if (!progressOnly)
		{
			LettuceMercenaryDataModel mercDataModel = GetDataModelForMerc(mercInTraining);
			if (mercDataModel != null)
			{
				mercDataModel.ChildUpgradeAvailable = false;
				if (checkMercExp && m_collectedMercInitialExp != -1)
				{
					int newExp = Mathf.Max((int)m_collectedMercInitialExp + (int)xpAwarded, (int)mercInTraining.m_experience);
					int newLevel = GameUtils.GetMercenaryLevelFromExperience(newExp);
					CollectionUtils.SetMercenaryStatsByLevel(mercDataModel, mercInTraining.ID, newLevel, mercInTraining.m_isFullyUpgraded);
					mercDataModel.MercenaryLevel = newLevel;
					mercDataModel.ExperienceInitial = newExp;
				}
			}
			slotData.Mercenary = mercDataModel;
		}
		int expGained = (slotData.Progress = LettuceVillageDataUtil.CalculateExpGainedFromTimestamp(startTime, m_trainingMetrics.expPerHour));
		slotData.MercIsMaxLevel = mercInTraining.IsMaxLevel();
		slotData.TrainingIsComplete = expGained >= m_trainingMetrics.maxExpGained;
		slotData.TotalTimeInTraining = LettuceVillageDataUtil.GetTimeTrainingInSeconds(startTime);
		if (!IsMercenaryFinishedPreparing(startTime))
		{
			int secondsLeft = (int)GetTimeLeftPreparing(startTime).TotalSeconds;
			if ((float)secondsLeft > 90f)
			{
				slotData.PreparationText = GameStrings.Format("GLUE_TRAINING_HALL_PREP_TIME_MIN", (secondsLeft + 30) / 60);
			}
			else
			{
				slotData.PreparationText = GameStrings.Format("GLUE_TRAINING_HALL_PREP_TIME_SEC", secondsLeft);
			}
			slotData.ShowAnimatedTraining = false;
		}
		else
		{
			slotData.PreparationText = null;
			slotData.ShowAnimatedTraining = !slotData.TrainingIsComplete && !slotData.MercIsMaxLevel;
		}
		m_timeToUpdateProgress = Time.time + m_progressUpdateFrequencyInSeconds;
	}

	private void OnTrainingWindowReady(Widget widget)
	{
		m_trainingWindowWidget = widget;
		m_trainingWindowWidget.WillLoadSynchronously = true;
	}

	private bool GetTrainingHallFlag(TrainingHallFlags flag)
	{
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARY_TRAINING_GROUND_FLAGS, out long flags);
		return ((ulong)flags & (ulong)flag) == (ulong)flag;
	}

	private void SetTrainingHallFlag(TrainingHallFlags flagsToSet, bool valueToSet)
	{
		GameSaveDataManager gsdMgr = GameSaveDataManager.Get();
		gsdMgr.GetSubkeyValue(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARY_TRAINING_GROUND_FLAGS, out long originalFlags);
		long newFlags = ((!valueToSet) ? (originalFlags & (long)(~flagsToSet)) : (originalFlags | (long)flagsToSet));
		if (newFlags != originalFlags)
		{
			gsdMgr.SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARY_TRAINING_GROUND_FLAGS, newFlags));
		}
	}

	private void OnSlotButtonPressed(int slot = 0)
	{
		LettuceMercenary trainingMerc = GetMercenaryInSlot(slot);
		if (trainingMerc == null)
		{
			Debug.LogWarning("LettuceVillageTrainingHall.OnSlotButtonPressed - button was pressed when no mercenary was present");
			return;
		}
		if (trainingMerc.IsMaxLevel())
		{
			RemoveMercenaryFromTraining(slot);
			return;
		}
		if (IsMercenaryFinishedPreparing(trainingMerc.m_trainingStartDate))
		{
			CollectMercenaryFromTraining(slot);
			return;
		}
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_LETTUCE_VILLAGE_REMOVE_MERCENARY_TRAINING");
		info.m_text = GameStrings.Get("GLUE_LETTUCE_VILLAGE_REMOVE_MERCENARY_TRAINING_DESCRIPTION");
		info.m_showAlertIcon = false;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
		info.m_responseCallback = delegate(AlertPopup.Response response, object userData)
		{
			if (response == AlertPopup.Response.CONFIRM)
			{
				RemoveMercenaryFromTraining(slot);
			}
		};
		DialogManager.Get().ShowPopup(info);
	}

	public void PlaceMercenaryInTraining(int mercId, int slot = 0)
	{
		if (!m_processingUpdate)
		{
			Network.Get().MercenariesTrainingAddRequest(mercId);
			m_lastSlotInteracted = slot;
			m_processingUpdate = true;
			m_responseTimeout = Time.time + 30f;
			LettuceVillagePopupManager.Get().HideHelpPopupsInVillage();
		}
	}

	public void RemoveMercenaryFromTraining(int slot = 0)
	{
		if (!m_processingUpdate)
		{
			LettuceMercenary currentMerc = GetMercenaryInSlot(slot);
			if (currentMerc != null)
			{
				Network.Get().MercenariesTrainingRemoveRequest(currentMerc.ID);
				m_lastSlotInteracted = slot;
				m_processingUpdate = true;
				m_responseTimeout = Time.time + 30f;
			}
		}
	}

	public void CollectMercenaryFromTraining(int slot = 0)
	{
		if (!m_processingUpdate)
		{
			LettuceMercenary currentMerc = GetMercenaryInSlot(slot);
			if (currentMerc != null)
			{
				Network.Get().MercenariesTrainingCollectRequest(currentMerc.ID);
				m_lastSlotInteracted = slot;
				m_collectedMercInitialExp = currentMerc.m_experience;
				PopupDisplayManager.SuppressPopupsTemporarily = true;
				m_processingUpdate = true;
				m_responseTimeout = Time.time + 30f;
			}
		}
	}

	private void ShowSimpleError(string message)
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_COLLECTION_ERROR_HEADER");
		info.m_text = GameStrings.Get(message);
		info.m_showAlertIcon = true;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		DialogManager.Get().ShowPopup(info);
	}

	private void OnMercenariesTrainingAddResponse()
	{
		m_processingUpdate = false;
		m_responseTimeout = 0f;
		MercenariesTrainingAddResponse response = Network.Get().MercenariesTrainingAddResponse();
		if (response != null && response.Success)
		{
			SetMercenaryInSlotGSD(response.MercenaryId, m_lastSlotInteracted);
			if (m_lastSlotInteracted == 1)
			{
				SetTrainingHallFlag(TrainingHallFlags.SLOT_2_DONE_SHOWING_NEW_DECORATION, valueToSet: true);
			}
			UpdateDataModelForSlot(m_lastSlotInteracted, progressOnly: false, checkMercExp: false, 0L);
		}
		else
		{
			ShowSimpleError("GLUE_COLLECTION_GENERIC_ERROR");
		}
		PopulateMercList();
	}

	private void OnMercenariesTrainingRemoveResponse()
	{
		m_processingUpdate = false;
		m_responseTimeout = 0f;
		MercenariesTrainingRemoveResponse response = Network.Get().MercenariesTrainingRemoveResponse();
		if (response == null || !response.Success)
		{
			ShowSimpleError("GLUE_COLLECTION_GENERIC_ERROR");
			return;
		}
		SetMercenaryInSlotGSD(0, m_lastSlotInteracted);
		UpdateDataModelForSlot(m_lastSlotInteracted, progressOnly: false, checkMercExp: false, 0L);
		PopulateMercList();
	}

	private void OnMercenariesTrainingCollectResponse()
	{
		m_processingUpdate = false;
		m_responseTimeout = 0f;
		MercenariesTrainingCollectResponse response = Network.Get().MercenariesTrainingCollectResponse();
		if (response == null || !response.Success)
		{
			ShowSimpleError("GLUE_COLLECTION_GENERIC_ERROR");
			return;
		}
		UpdateDataModelForSlot(m_lastSlotInteracted, progressOnly: false, checkMercExp: true, response.XpAwarded);
		ShowMercenaryExperienceReward(response.MercenaryId, response.XpAwarded);
		m_collectedMercInitialExp = -1L;
		PopulateMercList();
	}

	protected bool ShowMercenaryExperienceReward(int mercenaryId, long expGained)
	{
		if (m_isShowingMercenariesExperienceRewards)
		{
			return true;
		}
		if (m_expRewardContainer == null)
		{
			return false;
		}
		List<MercenaryExpRewardData> mercenariesExperienceRewards = new List<MercenaryExpRewardData>();
		MercenaryExpRewardData rewardData = new MercenaryExpRewardData(mercenaryId, (int)m_collectedMercInitialExp, (int)(m_collectedMercInitialExp + expGained), (int)expGained);
		mercenariesExperienceRewards.Add(rewardData);
		MercenariesTrainingHallExpRewardPopup expReward = m_expRewardContainer.GetComponent<MercenariesTrainingHallExpRewardPopup>();
		if (expReward == null)
		{
			Log.Lettuce.PrintError("MercenariesExperienceTrainingHall game object had no script attached!");
			m_isShowingMercenariesExperienceRewards = false;
			return false;
		}
		m_expRewardContainer.SetActive(value: true);
		expReward.Initialize(mercenariesExperienceRewards, OnMercenariesExperienceRewardReady, OnMercenariesExperienceRewardClosed);
		return true;
	}

	private void OnMercenariesExperienceRewardReady()
	{
		UIContext.GetRoot().ShowPopup(m_expRewardContainer);
		m_isShowingMercenariesExperienceRewards = true;
	}

	private void OnMercenariesExperienceRewardClosed()
	{
		m_isShowingMercenariesExperienceRewards = false;
		UIContext.GetRoot().DismissPopup(m_expRewardContainer);
		m_expRewardContainer.GetComponent<MercenariesTrainingHallExpRewardPopup>();
		m_expRewardContainer.SetActive(value: false);
		ShowNextAbilityRewardUntilAllShown(delegate
		{
			PopupDisplayManager.SuppressPopupsTemporarily = false;
		});
	}

	private void ShowNextAbilityRewardUntilAllShown(Action doneCallback = null)
	{
		NetCache.ProfileNoticeMercenariesAbilityUnlock rewardNotice = PopupDisplayManager.Get().RewardPopups.GetNextMercenariesAbilityUnlockReward();
		if (rewardNotice == null)
		{
			doneCallback?.Invoke();
			return;
		}
		PopupDisplayManager.Get().RewardPopups.ShowNextMercenariesAbilityUnlockReward(rewardNotice, delegate
		{
			ShowNextAbilityRewardUntilAllShown();
		});
	}

	public bool IsProcessingUpdate()
	{
		return m_processingUpdate;
	}

	private void OnTrainingSlot1Ready(Widget slotWidget)
	{
		slotWidget.BindDataModel(m_dataModel.Slot1);
	}

	private void OnTrainingSlot2Ready(Widget slotWidget)
	{
		slotWidget.BindDataModel(m_dataModel.Slot2);
	}

	private void OnScreenBlockerReady(Widget widget)
	{
		m_screenBlocker = widget;
	}

	private void EnableInput(bool value)
	{
		m_screenBlocker.TriggerEvent(value ? "UNBLOCK_SCREEN" : "BLOCK_SCREEN");
	}

	private void AddTrainingDataToDataModel()
	{
		BuildingTierDbfRecord currTrainingHallTier = LettuceVillageDataUtil.GetCurrentTierRecordFromBuilding(MercenaryBuilding.Mercenarybuildingtype.TRAININGHALL);
		if (currTrainingHallTier != null && m_dataModel != null)
		{
			if (m_trainingMetrics.expPerHour > 0)
			{
				m_dataModel.MaxTrainingHours = m_trainingMetrics.maxExpGained / m_trainingMetrics.expPerHour;
			}
			else
			{
				m_dataModel.MaxTrainingHours = 0;
			}
			m_dataModel.TrainingHallLevel = currTrainingHallTier.ID;
		}
	}

	private void GenerateTrainingMetrics()
	{
		BuildingTierDbfRecord currTrainingHallTier = LettuceVillageDataUtil.GetCurrentTierRecordFromBuilding(MercenaryBuilding.Mercenarybuildingtype.TRAININGHALL);
		m_trainingMetrics = default(TrainingMetrics);
		m_trainingMetrics.expPerHour = LettuceVillageDataUtil.GetCurrentTierPropertyForBuilding(MercenaryBuilding.Mercenarybuildingtype.TRAININGHALL, TierProperties.Buildingtierproperty.TRAININGXPPERHOUR, currTrainingHallTier);
		m_trainingMetrics.maxExpGained = LettuceVillageDataUtil.GetCurrentTierPropertyForBuilding(MercenaryBuilding.Mercenarybuildingtype.TRAININGHALL, TierProperties.Buildingtierproperty.TRAININGXPPOOLSIZE, currTrainingHallTier);
		m_trainingMetrics.numSlotsAvailable = LettuceVillageDataUtil.GetCurrentTierPropertyForBuilding(MercenaryBuilding.Mercenarybuildingtype.TRAININGHALL, TierProperties.Buildingtierproperty.TRAININGSLOTS, currTrainingHallTier);
		m_trainingMetrics.minTrainingTime = LettuceVillageDataUtil.GetCurrentTierPropertyForBuilding(MercenaryBuilding.Mercenarybuildingtype.TRAININGHALL, TierProperties.Buildingtierproperty.TRAININGMINSECONDS, currTrainingHallTier);
	}

	private TimeSpan GetTimeLeftPreparing(Date startDate)
	{
		if (startDate == null)
		{
			return default(TimeSpan);
		}
		DateTime dateTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, startDate.Hours, startDate.Min, startDate.Sec).AddSeconds(m_trainingMetrics.minTrainingTime);
		DateTime currentDate = DateTime.UtcNow;
		return dateTime - currentDate;
	}

	private bool IsMercenaryFinishedPreparing(Date startDate)
	{
		if (startDate == null)
		{
			return false;
		}
		DateTime utcNow = DateTime.UtcNow;
		DateTime trainingStartTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, startDate.Hours, startDate.Min, startDate.Sec);
		if ((utcNow - trainingStartTime).TotalSeconds > (double)m_trainingMetrics.minTrainingTime)
		{
			return true;
		}
		return false;
	}

	private LettuceMercenary GetMercenaryInSlot(int slot = 0)
	{
		LettuceMercenary trainingMerc = null;
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARIES_TRAINING_SLOTS, m_GSDMercenaries);
		if (slot < (m_GSDMercenaries?.Count ?? 0) && m_GSDMercenaries[slot] > 0)
		{
			trainingMerc = CollectionManager.Get().GetMercenary(m_GSDMercenaries[slot]);
		}
		return trainingMerc;
	}

	private bool SetMercenaryInSlotGSD(int mercID = 0, int slot = 0)
	{
		if (slot < 0 || slot > 1)
		{
			return false;
		}
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARIES_TRAINING_SLOTS, m_GSDMercenaries);
		m_GSDMercenaries = m_GSDMercenaries ?? new List<long> { 0L, 0L };
		m_GSDMercenaries[slot] = mercID;
		return GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARIES_TRAINING_SLOTS, m_GSDMercenaries.ToArray()));
	}

	private void ValidateGSDState()
	{
		bool anythingChanged = false;
		(LettuceMercenary, LettuceMercenary) mercsInTraining = CollectionManager.Get().GetMercenariesInTraining();
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARIES_TRAINING_SLOTS, m_GSDMercenaries);
		m_GSDMercenaries = m_GSDMercenaries ?? new List<long> { 0L, 0L };
		while (m_GSDMercenaries.Count < 2)
		{
			m_GSDMercenaries.Add(0L);
			anythingChanged = true;
		}
		int trainingMercId0 = mercsInTraining.Item1?.ID ?? (-1);
		int trainingMercId1 = mercsInTraining.Item2?.ID ?? (-1);
		int finalMercSlot0;
		if (m_GSDMercenaries[0] == trainingMercId0)
		{
			finalMercSlot0 = trainingMercId0;
			trainingMercId0 = -1;
		}
		else if (m_GSDMercenaries[0] == trainingMercId1)
		{
			finalMercSlot0 = trainingMercId1;
			trainingMercId1 = -1;
		}
		else
		{
			finalMercSlot0 = 0;
		}
		int finalMercSlot1;
		if (m_GSDMercenaries[1] == trainingMercId0)
		{
			finalMercSlot1 = trainingMercId0;
			trainingMercId0 = -1;
		}
		else if (m_GSDMercenaries[1] == trainingMercId1)
		{
			finalMercSlot1 = trainingMercId1;
			trainingMercId1 = -1;
		}
		else
		{
			finalMercSlot1 = 0;
		}
		if (trainingMercId0 != -1)
		{
			if (finalMercSlot0 == 0)
			{
				finalMercSlot0 = trainingMercId0;
			}
			else
			{
				finalMercSlot1 = trainingMercId0;
			}
		}
		if (trainingMercId1 != -1)
		{
			if (finalMercSlot0 == 0)
			{
				finalMercSlot0 = trainingMercId1;
			}
			else
			{
				finalMercSlot1 = trainingMercId1;
			}
		}
		if (m_GSDMercenaries[0] != finalMercSlot0 || m_GSDMercenaries[1] != finalMercSlot1)
		{
			m_GSDMercenaries[0] = finalMercSlot0;
			m_GSDMercenaries[1] = finalMercSlot1;
			anythingChanged = true;
		}
		if (anythingChanged)
		{
			GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARIES_TRAINING_SLOTS, m_GSDMercenaries.ToArray()));
		}
	}

	private LettuceMercenaryDataModel GetDataModelForMerc(LettuceMercenary merc)
	{
		LettuceMercenaryDataModel target = null;
		if (merc == null)
		{
			return null;
		}
		if (m_mercenaryDMCache.ContainsKey(merc.ID))
		{
			target = m_mercenaryDMCache[merc.ID];
			target.MercenaryLevel = GameUtils.GetMercenaryLevelFromExperience((int)merc.m_experience);
			target.ExperienceInitial = (int)merc.m_experience;
		}
		else
		{
			LettuceMercenaryDataModel mercdm = MercenaryFactory.CreateMercenaryDataModel(merc);
			if (mercdm != null)
			{
				m_mercenaryDMCache.Add(merc.ID, mercdm);
				target = mercdm;
			}
		}
		return target;
	}

	private void OnSearchReady(CollectionSearch obj)
	{
		obj.RegisterActivatedListener(OnSearchActivated);
		obj.RegisterDeactivatedListener(OnSearchDeactivated);
		obj.RegisterClearedListener(OnSearchCleared);
	}

	private void OnSearchActivated()
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			EnableInput(value: false);
		}
	}

	private void OnSearchDeactivated(string oldSearchText, string newSearchText)
	{
		if (!string.Equals(newSearchText, m_searchText, StringComparison.OrdinalIgnoreCase))
		{
			m_searchText = newSearchText;
			PopulateMercList();
		}
		EnableInput(value: true);
	}

	private void OnSearchCleared(bool transitionPage)
	{
		if (!string.IsNullOrEmpty(m_searchText))
		{
			m_searchText = null;
			PopulateMercList();
		}
	}

	private void OnListSubwidgetReady(VisualController obj)
	{
		GameObject vcGameObject = obj.gameObject;
		m_listVC = obj;
		m_scrollbar = vcGameObject.GetComponentInChildren<UIBScrollable>(includeInactive: true);
		m_mercListable = vcGameObject.GetComponentInChildren<Listable>(includeInactive: true);
	}

	private static int CompareMercenaries(LettuceMercenary a, LettuceMercenary b)
	{
		if (a.m_level != b.m_level)
		{
			if (a.m_level <= b.m_level)
			{
				return -1;
			}
			return 1;
		}
		return string.Compare(a.m_mercName, b.m_mercName);
	}

	private void PopulateMercList()
	{
		if (m_disabledMercenaries == null)
		{
			m_disabledMercenaries = LettuceVillageDataUtil.GetDisabledMercenaryList();
		}
		CollectionManager collectionManager = CollectionManager.Get();
		string searchText = m_searchText;
		bool? isOwned = true;
		bool? excludeCraftableFromOwned = true;
		CollectionManager.FindMercenariesResult mercs = collectionManager.FindMercenaries(searchText, isOwned, null, null, excludeCraftableFromOwned, ordered: false, null);
		mercs.m_mercenaries.Sort(CompareMercenaries);
		m_dataModel.MercenaryList.Clear();
		if (!m_didInitialLoad)
		{
			m_didInitialLoad = true;
			m_scrollbar.SetScrollImmediate(0f);
			Debug.Log("populating merc list in batches");
			m_mercenariesToLoad = mercs.m_mercenaries;
			StartCoroutine(BatchLoadRemainingMercenaryTiles());
			return;
		}
		foreach (LettuceMercenary merc in mercs.m_mercenaries)
		{
			if (!LettuceVillageDataUtil.IsMercenaryDisabled(merc.ID, m_disabledMercenaries) && (merc.m_trainingStartDate == null || merc.m_trainingStartDate.Year == 0))
			{
				LettuceMercenaryDataModel mercdm = GetDataModelForMerc(merc);
				mercdm.ShowLevelInList = true;
				mercdm.ChildUpgradeAvailable = false;
				m_dataModel.MercenaryList.Add(mercdm);
			}
		}
		m_scrollbar.UpdateScroll();
	}

	private IEnumerator BatchLoadRemainingMercenaryTiles()
	{
		yield return new WaitForSeconds(0.1f);
		int countCheck = 0;
		while (m_mercenariesToLoad.Count > 0)
		{
			if (countCheck > 10)
			{
				countCheck = 0;
				yield return new WaitForSeconds(0.1f);
			}
			if (m_mercenariesToLoad.Count <= 0)
			{
				break;
			}
			LettuceMercenary merc = m_mercenariesToLoad[0];
			if (!LettuceVillageDataUtil.IsMercenaryDisabled(merc.ID, m_disabledMercenaries) && (merc.m_trainingStartDate == null || merc.m_trainingStartDate.Year == 0))
			{
				LettuceMercenaryDataModel mercdm = GetDataModelForMerc(merc);
				mercdm.ShowLevelInList = true;
				mercdm.ChildUpgradeAvailable = false;
				m_dataModel.MercenaryList.Add(mercdm);
			}
			m_mercenariesToLoad.RemoveAt(0);
			countCheck++;
		}
		m_scrollbar.UpdateScroll();
	}

	private void OnMercenariesDraggablesReady(Widget widget)
	{
		if (widget != null)
		{
			m_offScreenPosition = widget.transform.localPosition;
			m_mercenariesDraggablesWidget = widget;
			widget.Hide();
		}
	}

	private void OnMercDragStarted()
	{
		m_dragColliderRoot.SetActive(value: true);
		LettuceMercenaryDataModel mercData = WidgetUtils.GetEventDataModel(m_listVC).Payload as LettuceMercenaryDataModel;
		if (GrabMerc(mercData))
		{
			m_dataModel.IsPlayerDragging = true;
			m_dataModel.MercenaryList.Remove(mercData);
		}
		else
		{
			m_dragColliderRoot.SetActive(value: false);
		}
	}

	private void DisableDraggableColliders()
	{
		BoxCollider[] colliders = m_mercenariesDraggablesWidget.gameObject.GetComponentsInChildren<BoxCollider>(includeInactive: true);
		if (colliders != null)
		{
			BoxCollider[] array = colliders;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
			}
		}
	}

	private bool GrabMerc(LettuceMercenaryDataModel mercData)
	{
		Ray mouseRay = UniversalInputManager.Get().MousePositionToRay(Box.Get().GetCamera());
		if (!m_dragPlaneCollider.Raycast(mouseRay, out var hit, 1000f))
		{
			return false;
		}
		m_draggedMerc = mercData;
		m_mercenariesDraggablesWidget.Show();
		m_mercenariesDraggablesWidget.BindDataModel(mercData);
		DisableDraggableColliders();
		CheckMercOverDropArea(mouseRay, forceModeEvent: true);
		m_draggedMercIsMaxLevel = CollectionManager.Get().GetMercenary(mercData.MercenaryId, AttemptToGenerate: false, ReportError: false)?.IsMaxLevel() ?? (mercData.MercenaryLevel >= 30);
		m_mercenariesDraggablesWidget.transform.position = hit.point;
		m_dataModel.ErrorText = null;
		return true;
	}

	private void UpdateDraggedMerc()
	{
		Ray mouseRay = UniversalInputManager.Get().MousePositionToRay(Box.Get().GetCamera());
		if (m_dragPlaneCollider.Raycast(mouseRay, out var hit, 1000f))
		{
			Vector3 newPos = hit.point;
			m_mercenariesDraggablesWidget.gameObject.transform.position = newPos;
			CheckMercOverDropArea(mouseRay);
			if (InputCollection.GetMouseButtonUp(0))
			{
				MercDropped();
			}
		}
	}

	private void CheckMercOverDropArea(Ray mouseRay, bool forceModeEvent = false)
	{
		m_draggedMercIsOverTrainingArea = m_dropZoneCollider.Raycast(mouseRay, out var _, 1000f);
		bool isSlotOpen = (m_dataModel.Slot1.SlotIsEmpty && !m_dataModel.Slot1.Locked) || (m_dataModel.Slot2.SlotIsEmpty && !m_dataModel.Slot2.Locked);
		bool okToDropIntoTraining = m_draggedMercIsOverTrainingArea && !m_draggedMercIsMaxLevel && isSlotOpen;
		if (okToDropIntoTraining != m_dataModel.IsMercOverTrainingWindow || forceModeEvent)
		{
			if (okToDropIntoTraining)
			{
				m_mercenariesDraggablesWidget.TriggerEvent("HOLD_MERC_OVER_TRAINING_code");
			}
			else
			{
				m_mercenariesDraggablesWidget.TriggerEvent("HOLD_MERC_OVER_TEAM_TRAY_code");
			}
			m_dataModel.IsMercOverTrainingWindow = okToDropIntoTraining;
		}
	}

	private void MercDropped()
	{
		if (m_dataModel.IsMercOverTrainingWindow)
		{
			int slot = ((!m_dataModel.Slot1.SlotIsEmpty) ? 1 : 0);
			PlaceMercenaryInTraining(m_draggedMerc.MercenaryId, slot);
			m_trainingWindowWidget.TriggerEvent((m_lastSlotInteracted == 0) ? "MERC_DROPPED_SLOT_1" : "MERC_DROPPED_SLOT_2");
		}
		else
		{
			PopulateMercList();
			m_trainingWindowWidget.TriggerEvent("MERC_DROPPED_LIST");
			if (m_draggedMercIsOverTrainingArea)
			{
				m_dataModel.ErrorText = GameStrings.Get(m_draggedMercIsMaxLevel ? "GLUE_TRAINING_HALL_MAX_LEVEL_MSG" : "GLUE_TRAINING_HALL_NO_SLOTS_MSG");
			}
		}
		m_draggedMerc = null;
		m_dataModel.IsPlayerDragging = false;
		m_dataModel.IsMercOverTrainingWindow = false;
		m_draggedMercIsOverTrainingArea = false;
		m_draggedMercIsMaxLevel = false;
		m_mercenariesDraggablesWidget.transform.localPosition = m_offScreenPosition;
		m_mercenariesDraggablesWidget.Hide();
		m_dragColliderRoot.SetActive(value: false);
	}

	public void OnDestroy()
	{
		CollectionManager.Get().OnMercenariesTrainingAddResponseReceived -= OnMercenariesTrainingAddResponse;
		CollectionManager.Get().OnMercenariesTrainingRemoveResponseReceived -= OnMercenariesTrainingRemoveResponse;
		CollectionManager.Get().OnMercenariesTrainingCollectResponseReceived -= OnMercenariesTrainingCollectResponse;
	}
}
