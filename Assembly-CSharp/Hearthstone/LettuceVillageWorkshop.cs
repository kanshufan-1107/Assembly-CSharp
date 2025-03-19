using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Hearthstone.DataModels;
using Hearthstone.Progression;
using Hearthstone.UI;
using PegasusLettuce;
using UnityEngine;

namespace Hearthstone;

public class LettuceVillageWorkshop : MonoBehaviour
{
	private enum ShowPageStyle
	{
		IMMEDIATE,
		NEXT,
		PREVIOUS
	}

	private class CachedBuildingState
	{
		public enum SortBuckets
		{
			BuildNow,
			UpgradeNow,
			InProgress,
			Completed
		}

		public MercenariesBuildingState BuildingState;

		public MercenaryBuildingDbfRecord BuildingRecord;

		public BuildingTierDbfRecord CurrentTier;

		public BuildingTierDbfRecord TierToDisplay;

		public AchievementDataModel Achievement;

		public string BuildingTitle;

		public bool IsAchievementCompleted;

		public bool IsNewBuilding;

		public bool IsFullyUpgraded;

		public SortBuckets SortBucket;
	}

	public float MinimumTimeToShowAuthScreenInSeconds = 1f;

	public AsyncReference ParentControllerReference;

	public AsyncReference Page1Reference;

	public AsyncReference Page2Reference;

	public AsyncReference NextPageButton;

	public AsyncReference PreviousPageButton;

	public Maskable MaskableComponent;

	public MercenaryBuilding.Mercenarybuildingtype[] BuildingsToHideInUI = new MercenaryBuilding.Mercenarybuildingtype[1] { MercenaryBuilding.Mercenarybuildingtype.MAILBOX };

	private Widget m_widget;

	private VisualController m_parentController;

	private VisualController m_previousPageButtonVC;

	private VisualController m_nextPageButtonVC;

	private PurchaseAuthView m_purchaseAuthView = new PurchaseAuthView();

	private MercenaryVillageWorkshopItemDataModel m_buildingBeingUpgraded;

	private float m_timeAuthScreenWasShown;

	private int m_indexOfVisibleWorkshopPage;

	private int m_indexOfVisibleDataPage;

	private int m_buildingsPerPage = -1;

	private bool m_inPrewarmState;

	private List<LettuceVillageWorkshopPage> m_workshopPages = new List<LettuceVillageWorkshopPage>();

	private List<MercenaryVillageWorkshopItemDataModel> m_reusableItemList = new List<MercenaryVillageWorkshopItemDataModel>();

	private List<int> m_buildingIdsToAlwaysHideInUI;

	private LettuceVillagePopupManager m_popupManager;

	private List<CachedBuildingState> m_buildingStatePool = new List<CachedBuildingState>();

	private List<CachedBuildingState> m_buildingStateList = new List<CachedBuildingState>();

	private static readonly string[] c_showPageImmediatelyStateNames = new string[2] { "SHOW_PAGE_1_IMMEDIATELY", "SHOW_PAGE_2_IMMEDIATELY" };

	private static readonly string[] c_showNextPageStateNames = new string[2] { "SHOW_PAGE_1_NEXT", "SHOW_PAGE_2_NEXT" };

	private static readonly string[] c_showPrevPageStateNames = new string[2] { "SHOW_PAGE_1_PREV", "SHOW_PAGE_2_PREV" };

	private LettuceVillageWorkshopPage VisiblePage => m_workshopPages[m_indexOfVisibleWorkshopPage];

	private int VisiblePageIndex => m_indexOfVisibleDataPage;

	private LettuceVillageWorkshopPage HiddenPage => m_workshopPages[HiddenPageIndex];

	private int HiddenPageIndex => m_indexOfVisibleWorkshopPage ^ 1;

	private void Start()
	{
		m_popupManager = GetComponentInParent<LettuceVillagePopupManager>();
		m_purchaseAuthView.OnPurchaseResultAcknowledged += OnSuccessFailureAcknowledged;
		m_purchaseAuthView.OnComponentReady += ShowAuthorizationSpinner;
		ParentControllerReference.RegisterReadyListener<VisualController>(OnParentControllerReady);
		Page1Reference.RegisterReadyListener<LettuceVillageWorkshopPage>(OnPageWidgetReady);
		Page2Reference.RegisterReadyListener<LettuceVillageWorkshopPage>(OnPageWidgetReady);
		NextPageButton.RegisterReadyListener<VisualController>(OnNextPageButtonReady);
		PreviousPageButton.RegisterReadyListener<VisualController>(OnPreviousPageButtonReady);
		Network.Get().RegisterNetHandler(MercenariesBuildingUpgradeResponse.PacketID.ID, OnBuildingUpgradeResponseReceived);
	}

	private void OnDestroy()
	{
		Network.Get()?.RemoveNetHandler(MercenariesBuildingUpgradeResponse.PacketID.ID, OnBuildingUpgradeResponseReceived);
		m_purchaseAuthView.OnPurchaseResultAcknowledged -= OnSuccessFailureAcknowledged;
		m_purchaseAuthView.OnComponentReady -= ShowAuthorizationSpinner;
		m_purchaseAuthView.Unload();
	}

	private void OnParentControllerReady(VisualController obj)
	{
		m_parentController = obj;
		m_parentController.RegisterDoneChangingStatesListener(OnParentControllerDoneChangingStates);
		m_widget = m_parentController.GetComponent<Widget>();
		m_widget.RegisterEventListener(OnWidgetEvent);
	}

	private void OnPageWidgetReady(LettuceVillageWorkshopPage obj)
	{
		m_buildingsPerPage = obj.NumSlots;
		m_workshopPages.Add(obj);
	}

	private void OnPreviousPageButtonReady(VisualController obj)
	{
		m_previousPageButtonVC = obj;
		m_previousPageButtonVC.SetState("HIDDEN");
		obj.GetComponent<Widget>().RegisterEventListener(OnPreviousPageButtonEvent);
	}

	private void OnNextPageButtonReady(VisualController obj)
	{
		m_nextPageButtonVC = obj;
		m_nextPageButtonVC.SetState("HIDDEN");
		obj.GetComponent<Widget>().RegisterEventListener(OnNextPageButtonEvent);
	}

	private int LastValidPageIndexFromListSize(int listSize)
	{
		return (listSize - 1) / m_buildingsPerPage;
	}

	private static int CompareCompleteBuildingStates(CachedBuildingState a, CachedBuildingState b)
	{
		if (a.SortBucket != b.SortBucket)
		{
			if (a.SortBucket <= b.SortBucket)
			{
				return -1;
			}
			return 1;
		}
		if (a.SortBucket != CachedBuildingState.SortBuckets.InProgress)
		{
			return string.Compare(a.BuildingTitle, b.BuildingTitle);
		}
		int aprogress = a.Achievement.Progress * b.Achievement.Quota;
		int bprogress = b.Achievement.Progress * a.Achievement.Quota;
		if (aprogress != bprogress)
		{
			if (aprogress <= bprogress)
			{
				return -1;
			}
			return 1;
		}
		return string.Compare(a.BuildingTitle, b.BuildingTitle);
	}

	private void FillListOfBuildingsToAlwaysHideInUI()
	{
		if (m_buildingIdsToAlwaysHideInUI != null)
		{
			return;
		}
		Dictionary<MercenaryBuilding.Mercenarybuildingtype, bool> buildingsEnabledMap = NetCache.Get()?.GetNetObject<NetCache.NetCacheMercenariesPlayerInfo>()?.BuildingEnabledMap;
		if (buildingsEnabledMap == null)
		{
			return;
		}
		List<MercenariesBuildingState> buildingStates = LettuceVillageDataUtil.BuildingStates;
		m_buildingIdsToAlwaysHideInUI = new List<int>();
		foreach (MercenariesBuildingState bldgState in buildingStates)
		{
			MercenaryBuildingDbfRecord bldgRecord = LettuceVillageDataUtil.GetBuildingRecordByID(bldgState.BuildingId);
			if (buildingsEnabledMap != null && (!buildingsEnabledMap.TryGetValue(bldgRecord.MercenaryBuildingType, out var operational) || !operational))
			{
				m_buildingIdsToAlwaysHideInUI.Add(bldgState.BuildingId);
				continue;
			}
			MercenaryBuilding.Mercenarybuildingtype[] buildingsToHideInUI = BuildingsToHideInUI;
			for (int i = 0; i < buildingsToHideInUI.Length; i++)
			{
				if (buildingsToHideInUI[i] == bldgRecord.MercenaryBuildingType)
				{
					m_buildingIdsToAlwaysHideInUI.Add(bldgState.BuildingId);
					break;
				}
			}
		}
	}

	private List<CachedBuildingState> FillBuildingStateList()
	{
		List<CachedBuildingState> tmp = m_buildingStatePool;
		m_buildingStatePool = m_buildingStateList;
		m_buildingStateList = tmp;
		FillListOfBuildingsToAlwaysHideInUI();
		List<MercenariesBuildingState> inputList = LettuceVillageDataUtil.BuildingStates;
		int poolPosition = 0;
		List<MercenaryBuilding.Mercenarybuildingtype> availableTutorialBuildings = LettuceVillageDataUtil.GetAvailableBuildingsForCurrentFTUEState(isUIContext: true);
		for (int i = 0; i < inputList.Count; i++)
		{
			if (LettuceVillageDataUtil.IsBuildingAvailableInTutorial(inputList[i].BuildingId, availableTutorialBuildings))
			{
				List<int> buildingIdsToAlwaysHideInUI = m_buildingIdsToAlwaysHideInUI;
				if (buildingIdsToAlwaysHideInUI == null || !buildingIdsToAlwaysHideInUI.Contains(inputList[i].BuildingId))
				{
					CachedBuildingState targetState = ((poolPosition >= m_buildingStatePool.Count) ? new CachedBuildingState() : m_buildingStatePool[poolPosition++]);
					CacheBuildingState(targetState, inputList[i]);
					m_buildingStateList.Add(targetState);
				}
			}
		}
		m_buildingStatePool.Clear();
		m_buildingStateList.Sort(CompareCompleteBuildingStates);
		return m_buildingStateList;
	}

	private void CacheBuildingState(CachedBuildingState targetState, MercenariesBuildingState sourceState)
	{
		targetState.BuildingState = sourceState;
		MercenaryBuildingDbfRecord buildingRecord = (targetState.BuildingRecord = LettuceVillageDataUtil.GetBuildingRecordByID(sourceState.BuildingId));
		targetState.BuildingTitle = buildingRecord.Name?.GetString() ?? buildingRecord.MercenaryBuildingType.ToString();
		BuildingTierDbfRecord currentTierRecord = (targetState.CurrentTier = GameDbf.BuildingTier.GetRecord((BuildingTierDbfRecord r) => r.ID == sourceState.CurrentTierId));
		targetState.IsNewBuilding = !LettuceVillageDataUtil.BuildingIsBuilt(sourceState);
		BuildingTierDbfRecord nextTierRecord = LettuceVillageDataUtil.GetNextTierRecord(currentTierRecord);
		if (nextTierRecord == null)
		{
			targetState.IsFullyUpgraded = true;
			targetState.TierToDisplay = currentTierRecord;
			targetState.IsAchievementCompleted = false;
			targetState.Achievement = null;
			targetState.SortBucket = CachedBuildingState.SortBuckets.Completed;
			return;
		}
		targetState.IsFullyUpgraded = false;
		targetState.TierToDisplay = nextTierRecord;
		if (nextTierRecord.UnlockAchievement == 0)
		{
			targetState.IsAchievementCompleted = true;
			targetState.Achievement = null;
		}
		else
		{
			targetState.IsAchievementCompleted = ProgressUtils.IsAchievementComplete((targetState.Achievement = AchievementManager.Get().GetAchievementDataModel(nextTierRecord.UnlockAchievement)).Status);
		}
		if (!targetState.IsAchievementCompleted)
		{
			targetState.SortBucket = CachedBuildingState.SortBuckets.InProgress;
		}
		else
		{
			targetState.SortBucket = ((!targetState.IsNewBuilding) ? CachedBuildingState.SortBuckets.UpgradeNow : CachedBuildingState.SortBuckets.BuildNow);
		}
	}

	private MercenaryVillageWorkshopItemDataModel MakeWorkshopItemDataModel(CachedBuildingState completeState, long goldBalance)
	{
		MercenaryVillageWorkshopItemDataModel buildingModel = new MercenaryVillageWorkshopItemDataModel();
		MercenaryBuildingDbfRecord buildingRecord = completeState.BuildingRecord;
		buildingModel.BuildingID = completeState.BuildingState.BuildingId;
		buildingModel.BuildingType = buildingRecord.MercenaryBuildingType;
		buildingModel.Title = completeState.BuildingTitle;
		buildingModel.Description = buildingRecord.Description?.GetString() ?? "";
		buildingModel.IsNewBuilding = completeState.IsNewBuilding;
		buildingModel.IsFullyUpgraded = completeState.IsFullyUpgraded;
		buildingModel.IsAchievementCompleted = completeState.IsAchievementCompleted;
		BuildingTierDbfRecord tierRecordToDisplay = completeState.TierToDisplay;
		buildingModel.TierDescription = tierRecordToDisplay.Description?.GetString() ?? "";
		buildingModel.CurrentTierId = completeState.CurrentTier.ID;
		buildingModel.NextTierId = completeState.TierToDisplay.ID;
		if (buildingModel.IsFullyUpgraded)
		{
			return buildingModel;
		}
		if (!completeState.IsAchievementCompleted)
		{
			AchievementDataModel achievement = completeState.Achievement;
			buildingModel.AchievementDescription = achievement.Description;
			buildingModel.Progress = achievement.Progress;
			buildingModel.Quota = achievement.Quota;
		}
		else
		{
			buildingModel.Price = new PriceDataModel
			{
				Currency = ((tierRecordToDisplay.UpgradeCost > 0) ? CurrencyType.GOLD : CurrencyType.NONE),
				Amount = tierRecordToDisplay.UpgradeCost,
				DisplayText = tierRecordToDisplay.UpgradeCost.ToString()
			};
			buildingModel.CanAffordUpgrade = goldBalance >= tierRecordToDisplay.UpgradeCost;
		}
		return buildingModel;
	}

	private int PreparePageModel(int pageNumber, List<CachedBuildingState> bldgStateList, List<MercenaryVillageWorkshopItemDataModel> listToFill)
	{
		listToFill.Clear();
		if (bldgStateList.Count == 0)
		{
			return 0;
		}
		int firstStateIndex = pageNumber * m_buildingsPerPage;
		if (firstStateIndex >= bldgStateList.Count)
		{
			pageNumber = LastValidPageIndexFromListSize(bldgStateList.Count);
			firstStateIndex = pageNumber * m_buildingsPerPage;
		}
		long goldBalance = NetCache.Get().GetGoldBalance();
		int lastStateIndex = Mathf.Min(firstStateIndex + m_buildingsPerPage, bldgStateList.Count) - 1;
		for (int i = firstStateIndex; i <= lastStateIndex; i++)
		{
			listToFill.Add(MakeWorkshopItemDataModel(bldgStateList[i], goldBalance));
		}
		return pageNumber;
	}

	private void DismissWorkshop()
	{
		SendEventUpwardStateAction.SendEventUpward(base.gameObject, "HIDE_POPUPS");
	}

	private void SwapWorkshopPages()
	{
		m_indexOfVisibleWorkshopPage = HiddenPageIndex;
	}

	private void UpdatePageButtonVisibility(int pageNumber, int lastValidPageIndex)
	{
		ShowPreviousButton(pageNumber != 0);
		ShowNextButton(pageNumber < lastValidPageIndex);
	}

	private void ShowPreviousButton(bool showIt)
	{
		m_previousPageButtonVC.SetState(showIt ? "LEFT" : "HIDDEN");
	}

	private void ShowNextButton(bool showIt)
	{
		m_nextPageButtonVC.SetState(showIt ? "RIGHT" : "HIDDEN");
	}

	private void OnWidgetEvent(string eventName)
	{
		if (eventName.Equals("UPGRADE_CLICKED", StringComparison.Ordinal))
		{
			EventDataModel eventDataModel = m_widget.GetDataModel<EventDataModel>();
			UpgradeClicked(eventDataModel.Payload as MercenaryVillageWorkshopItemDataModel);
		}
	}

	private void OnParentControllerDoneChangingStates(object obj)
	{
		string state = m_parentController.State;
		if (!(state == "MAKE_VISIBLE"))
		{
			if (state == "PREWARM")
			{
				EnterPrewarmState();
			}
		}
		else
		{
			ExitPrewarmState();
			ShowDataPage(0, ShowPageStyle.IMMEDIATE);
		}
	}

	private void OnPreviousPageButtonEvent(string eventName)
	{
		if (string.Equals("BUTTON_RELEASED", eventName, StringComparison.Ordinal))
		{
			ShowDataPage(m_indexOfVisibleDataPage - 1, ShowPageStyle.PREVIOUS);
		}
	}

	private void OnNextPageButtonEvent(string eventName)
	{
		if (string.Equals("BUTTON_RELEASED", eventName, StringComparison.Ordinal))
		{
			ShowDataPage(m_indexOfVisibleDataPage + 1, ShowPageStyle.NEXT);
		}
	}

	private void EnterPrewarmState()
	{
		Vector3 pos = base.transform.localPosition;
		pos.x += 1000f;
		base.transform.localPosition = pos;
		if (MaskableComponent != null)
		{
			MaskableComponent.enabled = false;
		}
		foreach (LettuceVillageWorkshopPage workshopPage in m_workshopPages)
		{
			workshopPage.PrewarmItems();
		}
		m_inPrewarmState = true;
	}

	private void ExitPrewarmState()
	{
		if (m_inPrewarmState)
		{
			Vector3 pos = base.transform.localPosition;
			pos.x -= 1000f;
			base.transform.localPosition = pos;
			if (MaskableComponent != null)
			{
				MaskableComponent.enabled = true;
			}
			m_inPrewarmState = false;
		}
	}

	private void ShowDataPage(int desiredPageNumber, ShowPageStyle showStyle)
	{
		FillBuildingStateList();
		int actualPageNumber = PreparePageModel(desiredPageNumber, m_buildingStateList, m_reusableItemList);
		switch (showStyle)
		{
		case ShowPageStyle.IMMEDIATE:
			HiddenPage.BindDataModel(m_reusableItemList);
			m_parentController.SetState(c_showPageImmediatelyStateNames[HiddenPageIndex]);
			break;
		case ShowPageStyle.NEXT:
			HiddenPage.BindDataModel(m_reusableItemList);
			m_parentController.SetState(c_showNextPageStateNames[HiddenPageIndex]);
			break;
		case ShowPageStyle.PREVIOUS:
			HiddenPage.BindDataModel(m_reusableItemList);
			m_parentController.SetState(c_showPrevPageStateNames[HiddenPageIndex]);
			break;
		}
		m_indexOfVisibleDataPage = actualPageNumber;
		SwapWorkshopPages();
		UpdatePageButtonVisibility(m_indexOfVisibleDataPage, LastValidPageIndexFromListSize(m_buildingStateList.Count));
	}

	private void UpgradeClicked(MercenaryVillageWorkshopItemDataModel clickedTaskData)
	{
		m_buildingBeingUpgraded = clickedTaskData;
		Network.Get().UpgradeMercenaryBuilding(clickedTaskData.BuildingID, clickedTaskData.NextTierId);
		if (!m_purchaseAuthView.IsLoaded)
		{
			m_purchaseAuthView.Load(AssetLoader.Get());
		}
		else
		{
			ShowAuthorizationSpinner();
		}
	}

	private void ShowAuthorizationSpinner()
	{
		m_purchaseAuthView.Show(null, isZeroCostLicense: false);
		m_timeAuthScreenWasShown = Time.time;
	}

	private void OnBuildingUpgradeResponseReceived()
	{
		if (m_buildingBeingUpgraded != null)
		{
			StartCoroutine(ShowSuccessFailureUI(Network.Get().UpgradeMercenaryBuildingResponse()));
		}
	}

	private IEnumerator ShowSuccessFailureUI(MercenariesBuildingUpgradeResponse result)
	{
		MercenaryVillageWorkshopItemDataModel buildingDataModel = m_buildingBeingUpgraded;
		m_buildingBeingUpgraded = null;
		while (!m_purchaseAuthView.IsShown)
		{
			yield return null;
		}
		float delayTime = Time.time - m_timeAuthScreenWasShown;
		if (delayTime < MinimumTimeToShowAuthScreenInSeconds)
		{
			yield return new WaitForSeconds(MinimumTimeToShowAuthScreenInSeconds - delayTime);
		}
		if (!(result?.Success ?? false))
		{
			m_purchaseAuthView.CompletePurchaseFailure(null, GameStrings.Get("GLUE_CHECKOUT_ERROR_GENERIC_FAILURE"), Network.PurchaseErrorInfo.ErrorType.BP_GENERIC_FAIL);
		}
		else if ((buildingDataModel?.Price?.Amount).GetValueOrDefault() == 0f)
		{
			m_purchaseAuthView.Hide();
			DismissWorkshop();
		}
		else
		{
			m_purchaseAuthView.CompletePurchaseSuccess(null);
		}
	}

	private void OnSuccessFailureAcknowledged(bool succeeded, MoneyOrGTAPPTransaction arg2)
	{
		DismissWorkshop();
	}
}
