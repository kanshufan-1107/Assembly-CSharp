using System.Collections.Generic;
using Hearthstone.DataModels;
using Hearthstone.UI;
using Hearthstone.UI.Core;
using UnityEngine;

public class LargeBundleProductPage : ProductPage, IPopupRendering
{
	public GameObject BackgroundSingle;

	public GameObject BackgroundFirst;

	public GameObject BackgroundMiddle;

	public GameObject BackgroundLast;

	public GameObject Divider;

	public GameObject Nameplate;

	public GameObject TraySlider;

	public PlayMakerFSM TurnPagePlayMakerFSM;

	public WidgetTemplate ProductPageWidget;

	public WidgetInstance DetailsFrameWidget;

	public float ItemSpacingX;

	public int ItemsPerPage;

	public float TrayHeight;

	public float TrayOffsetX;

	public float SeamPadding;

	public float FullTrayWidth;

	public Vector3 HeroSkinBasePosition;

	public Vector3 HeroSkinScale;

	public Vector3 BoardSkinBasePosition;

	public Vector3 BoardSkinScale;

	public Vector3 FinisherBasePosition;

	public Vector3 FinisherScale;

	public Vector3 EmoteBasePosition;

	public Vector3 EmoteScale;

	public Vector3 SummaryBasePosition;

	public Vector3 SummaryScale;

	public Vector3 DividerBasePosition;

	public Vector3 DividerScale;

	public Vector3 NameplateBasePosition;

	public Vector3 NameplateScale;

	private static readonly Quaternion s_backgroundRotation = Quaternion.Euler(90f, 0f, 0f);

	private static readonly AssetReference s_rewardItemDisplay = new AssetReference("RewardItemDisplay.prefab:1462b7f022881004c888368b9badc81e");

	private static readonly AssetReference s_summaryFiligree = new AssetReference("BaconStoreLargeBundleTitleFiligree.prefab:802bb3d63f19d064591b07dbfb6b7a3e");

	private static readonly AssetReference s_nameplate = new AssetReference("BaconStoreLargeBundleTextBracket.prefab:c441b65d36fb9454493272658ce3192c");

	private static TriggerEventParameters s_animationStartedEventParameters = new TriggerEventParameters(null, null, noDownwardPropagation: true);

	private bool m_sliderIsAnimating;

	private bool m_pageInfoDataModelBound;

	private PageInfoDataModel m_pageInfoDataModel = new PageInfoDataModel();

	private WidgetInstance m_summary;

	private ShopLargeBundleDetailsDataModel m_summaryDataModel = new ShopLargeBundleDetailsDataModel();

	private GameObject m_singleTray;

	private GameObject m_firstTray;

	private GameObject m_lastTray;

	private List<GameObject> m_middleTrays = new List<GameObject>();

	private List<GameObject> m_trays = new List<GameObject>();

	private List<GameObject> m_dividers = new List<GameObject>();

	private List<WidgetInstance> m_rewardItems = new List<WidgetInstance>();

	private List<WidgetInstance> m_nameplates = new List<WidgetInstance>();

	private BGEmoteAnimationController m_bgEmoteAnimationController;

	private IPopupRoot m_popupRoot;

	private HashSet<IPopupRendering> m_popupRenderingComponents;

	public float SmallTrayWidth => (float)ItemsPerPage * ItemSpacingX;

	private Vector3 BackgroundBasePosition => new Vector3(TrayOffsetX, -0.1f, 0.6f);

	private Vector3 FullTrayScale => new Vector3(FullTrayWidth + SeamPadding, TrayHeight, 1f);

	private Vector3 FullTrayScale_noPadding => new Vector3(FullTrayWidth, TrayHeight, 1f);

	private void PaginationEventListener(string eventName)
	{
		switch (eventName)
		{
		case "PageLeft_code":
			if (m_pageInfoDataModel.PageNumber > 1 && !m_sliderIsAnimating)
			{
				m_sliderIsAnimating = true;
				UpdatePageItemDataModelAndButtonsEnabled(m_pageInfoDataModel.PageNumber - 1, m_pageInfoDataModel.TotalPages);
				TurnPagePlayMakerFSM.SendEvent("PageLeft");
				m_widget.TriggerEvent("AnimationStarted_code", s_animationStartedEventParameters);
			}
			break;
		case "PageRight_code":
			if (m_pageInfoDataModel.PageNumber < m_pageInfoDataModel.TotalPages && !m_sliderIsAnimating)
			{
				m_sliderIsAnimating = true;
				UpdatePageItemDataModelAndButtonsEnabled(m_pageInfoDataModel.PageNumber + 1, m_pageInfoDataModel.TotalPages);
				TurnPagePlayMakerFSM.SendEvent("PageRight");
				m_widget.TriggerEvent("AnimationStarted_code", s_animationStartedEventParameters);
			}
			break;
		case "AnimationFinished_code":
			m_sliderIsAnimating = false;
			break;
		}
	}

	protected override void Start()
	{
		base.Start();
		m_widget.RegisterEventListener(PaginationEventListener);
		m_bgEmoteAnimationController = GetComponent<BGEmoteAnimationController>();
	}

	private void UpdatePageItemDataModelAndButtonsEnabled(int pageNumber, int totalPages)
	{
		if (pageNumber == 1)
		{
			ProductPageWidget.TriggerEvent("ENABLE_BUTTON_LEFT");
			ProductPageWidget.TriggerEvent("DISABLE_BUTTON_LEFT");
		}
		else
		{
			ProductPageWidget.TriggerEvent("ENABLE_BUTTON_LEFT");
		}
		if (pageNumber == totalPages)
		{
			ProductPageWidget.TriggerEvent("DISABLE_BUTTON_RIGHT");
		}
		else
		{
			ProductPageWidget.TriggerEvent("ENABLE_BUTTON_RIGHT");
		}
		m_pageInfoDataModel.PageNumber = pageNumber;
		m_pageInfoDataModel.TotalPages = totalPages;
		m_pageInfoDataModel.InfoText = GameStrings.Format("GLUE_PROGRESSION_REWARD_TRACK_PAGE_NUMBER", pageNumber, totalPages);
		UpdateVisibleAnimatedEmotes(null);
	}

	protected override void OnProductSet()
	{
		base.OnProductSet();
		if (base.Product.RewardList == null)
		{
			Log.Store.PrintWarning("LargeBundleProductPage Product has no RewardList. Cannot create reward items.");
			return;
		}
		m_pageInfoDataModel.ItemsPerPage = ItemsPerPage;
		int effectiveItemCount = base.Product.RewardList.Items.Count + 1;
		foreach (RewardItemDataModel item in base.Product.RewardList.Items)
		{
			if (item.ItemType == RewardItemType.BATTLEGROUNDS_EMOTE_PILE)
			{
				effectiveItemCount += item.BGEmotePile.Count - 1;
			}
		}
		int totalPages = effectiveItemCount / ItemsPerPage + ((effectiveItemCount % ItemsPerPage > 0) ? 1 : 0);
		UpdatePageItemDataModelAndButtonsEnabled(1, totalPages);
		TraySlider.transform.localPosition = new Vector3(0f, TraySlider.transform.localPosition.y, TraySlider.transform.localPosition.z);
		if (!m_pageInfoDataModelBound)
		{
			m_pageInfoDataModelBound = true;
			DetailsFrameWidget.BindDataModel(m_pageInfoDataModel);
		}
		SetTrays();
		SetSummary();
		SetItems();
	}

	private void SetTrays()
	{
		float halfTrayWidthDifference = (FullTrayWidth - SmallTrayWidth) / 2f;
		if (m_pageInfoDataModel.TotalPages == 1)
		{
			SetSingleTray(halfTrayWidthDifference);
		}
		else
		{
			SetMultipleTrays(halfTrayWidthDifference);
		}
	}

	private void SetSingleTray(float halfTrayWidthDifference)
	{
		if (m_firstTray != null)
		{
			m_firstTray.SetActive(value: false);
		}
		foreach (GameObject middleTray in m_middleTrays)
		{
			middleTray.SetActive(value: false);
		}
		if (m_lastTray != null)
		{
			m_lastTray.SetActive(value: false);
		}
		if (m_singleTray == null)
		{
			GameObject obj = Object.Instantiate(BackgroundSingle, TraySlider.transform);
			obj.transform.localPosition = BackgroundBasePosition + new Vector3(halfTrayWidthDifference, 0f, 0f);
			obj.transform.localRotation = s_backgroundRotation;
			obj.transform.localScale = FullTrayScale_noPadding;
		}
		else
		{
			m_singleTray.SetActive(value: true);
		}
	}

	private void SetMultipleTrays(float halfTrayWidthDifference)
	{
		Vector3 smallTrayScale = new Vector3(SmallTrayWidth + SeamPadding, TrayHeight, 1f);
		if (m_singleTray != null)
		{
			m_singleTray.SetActive(value: false);
		}
		if (m_firstTray == null)
		{
			m_firstTray = Object.Instantiate(BackgroundFirst, TraySlider.transform);
			m_firstTray.transform.localPosition = BackgroundBasePosition;
			m_firstTray.transform.localRotation = s_backgroundRotation;
			m_firstTray.transform.localScale = smallTrayScale;
		}
		else
		{
			m_firstTray.SetActive(value: true);
		}
		int i = 0;
		for (int l = m_pageInfoDataModel.TotalPages - 2; i < l; i++)
		{
			if (m_middleTrays.Count > i)
			{
				m_middleTrays[i].SetActive(value: true);
				continue;
			}
			GameObject middleTray = Object.Instantiate(BackgroundMiddle, TraySlider.transform);
			int pageIndex = i + 1;
			float offsetFromPageNumber = SmallTrayWidth * (float)pageIndex;
			middleTray.transform.localPosition = BackgroundBasePosition + new Vector3(offsetFromPageNumber, 0f, 0f);
			middleTray.transform.localRotation = s_backgroundRotation;
			middleTray.transform.localScale = smallTrayScale;
			m_middleTrays.Add(middleTray);
		}
		if (m_lastTray == null)
		{
			m_lastTray = Object.Instantiate(BackgroundLast, TraySlider.transform);
			m_lastTray.transform.localRotation = s_backgroundRotation;
			m_lastTray.transform.localScale = FullTrayScale;
		}
		else
		{
			m_lastTray.SetActive(value: true);
		}
		int lastPageIndex = m_pageInfoDataModel.TotalPages - 1;
		float offsetFromLastPageNumber = SmallTrayWidth * (float)lastPageIndex;
		m_lastTray.transform.localPosition = BackgroundBasePosition + new Vector3(offsetFromLastPageNumber + halfTrayWidthDifference, 0f, 0f);
		for (int j = m_pageInfoDataModel.TotalPages - 2; j < m_middleTrays.Count; j++)
		{
			m_middleTrays[j].SetActive(value: false);
		}
	}

	private void SetSummary()
	{
		m_summaryDataModel.Name = base.Product.Name;
		if (m_summary == null)
		{
			m_summary = WidgetInstance.Create(s_summaryFiligree);
			if (m_summary == null)
			{
				Log.Store.PrintError(string.Format("{0} cannot create an instance of {1}. Cannot create summary.", "LargeBundleProductPage", s_summaryFiligree));
				return;
			}
			m_summary.transform.SetParent(TraySlider.transform);
			m_summary.transform.localPosition = SummaryBasePosition;
			m_summary.transform.localScale = SummaryScale;
			m_summary.transform.localRotation = Quaternion.identity;
			ProductPageWidget.AddNestedInstance(m_summary);
			m_summary.BindDataModel(m_summaryDataModel);
		}
	}

	private void CreateOrReactivateWidgetInstances(int rewardItemIndex, out WidgetInstance rewardItem, out WidgetInstance nameplate)
	{
		if (m_rewardItems.Count > rewardItemIndex)
		{
			rewardItem = m_rewardItems[rewardItemIndex];
			nameplate = m_nameplates[rewardItemIndex];
			rewardItem.gameObject.SetActive(value: true);
			nameplate.gameObject.SetActive(value: true);
			if (rewardItemIndex > 0)
			{
				m_dividers[rewardItemIndex - 1].SetActive(value: true);
			}
			return;
		}
		rewardItem = WidgetInstance.Create(s_rewardItemDisplay);
		if (rewardItem == null)
		{
			Log.Store.PrintError(string.Format("{0} cannot create an instance of {1}. Cannot create summary.", "LargeBundleProductPage", s_rewardItemDisplay));
		}
		else
		{
			rewardItem.transform.SetParent(TraySlider.transform);
			ProductPageWidget.AddNestedInstance(rewardItem);
			m_rewardItems.Add(rewardItem);
		}
		nameplate = WidgetInstance.Create(s_nameplate);
		if (nameplate == null)
		{
			Log.Store.PrintError(string.Format("{0} cannot create an instance of {1}. Cannot create summary.", "LargeBundleProductPage", s_nameplate));
		}
		else
		{
			nameplate.transform.SetParent(TraySlider.transform);
			nameplate.transform.localPosition = NameplateBasePosition + new Vector3(ItemSpacingX * (float)rewardItemIndex, 0f, 0f);
			nameplate.transform.localScale = NameplateScale;
			nameplate.transform.localRotation = Quaternion.identity;
			ProductPageWidget.AddNestedInstance(nameplate);
			m_nameplates.Add(nameplate);
		}
		if (rewardItemIndex > 0)
		{
			GameObject divider = Object.Instantiate(Divider, TraySlider.transform);
			divider.transform.localPosition = DividerBasePosition + new Vector3(ItemSpacingX * ((float)rewardItemIndex - 0.5f), 0f, 0f);
			divider.transform.localScale = DividerScale;
			divider.transform.localRotation = s_backgroundRotation;
			m_dividers.Add(divider);
		}
	}

	private void UpdateRewardItemWidgetTransforms(WidgetInstance rewardItem, RewardItemType itemType, int index)
	{
		Vector3 basePosition;
		Vector3 scale;
		switch (itemType)
		{
		case RewardItemType.BATTLEGROUNDS_BOARD_SKIN:
			basePosition = BoardSkinBasePosition;
			scale = BoardSkinScale;
			break;
		case RewardItemType.BATTLEGROUNDS_HERO_SKIN:
		case RewardItemType.BATTLEGROUNDS_GUIDE_SKIN:
			basePosition = HeroSkinBasePosition;
			scale = HeroSkinScale;
			break;
		case RewardItemType.BATTLEGROUNDS_FINISHER:
			basePosition = FinisherBasePosition;
			scale = FinisherScale;
			break;
		case RewardItemType.BATTLEGROUNDS_EMOTE:
			basePosition = EmoteBasePosition;
			scale = EmoteScale;
			break;
		default:
			basePosition = Vector3.zero;
			scale = Vector3.one;
			break;
		}
		rewardItem.transform.localPosition = basePosition + new Vector3(ItemSpacingX * (float)index, 0f, 0f);
		rewardItem.transform.localScale = scale;
		rewardItem.transform.localRotation = Quaternion.identity;
	}

	private void BindOrReplaceBinding<T>(WidgetInstance widgetInstance, T newDataModel) where T : IDataModel
	{
		IDataModel boundDataModel = widgetInstance.GetDataModel<ShopLargeBundleDetailsNameplateDataModel>();
		if (boundDataModel != null)
		{
			widgetInstance.UnbindDataModel(boundDataModel.DataModelId);
		}
		widgetInstance.BindDataModel(newDataModel);
	}

	private void SetItems()
	{
		DataModelList<RewardItemDataModel> items = base.Product.RewardList.Items;
		if (HasRewardItemLists(items))
		{
			items = ExpandRewardItemLists(items);
		}
		for (int i = 0; i < items.Count; i++)
		{
			RewardItemDataModel rewardItemDataModel = items[i];
			CreateOrReactivateWidgetInstances(i, out var rewardItem, out var nameplate);
			if (rewardItem != null)
			{
				UpdateRewardItemWidgetTransforms(rewardItem, rewardItemDataModel.ItemType, i);
				BindOrReplaceBinding(rewardItem, rewardItemDataModel);
			}
			if (nameplate != null)
			{
				ShopLargeBundleDetailsNameplateDataModel nameplateDataModel = new ShopLargeBundleDetailsNameplateDataModel();
				nameplateDataModel.Name = RewardUtils.GetName(rewardItemDataModel);
				BindOrReplaceBinding(nameplate, nameplateDataModel);
			}
		}
		for (int j = items.Count; j < m_rewardItems.Count; j++)
		{
			m_rewardItems[j].gameObject.SetActive(value: false);
			m_nameplates[j].gameObject.SetActive(value: false);
			m_dividers[j - 1].SetActive(value: false);
		}
		if (m_popupRoot != null)
		{
			m_popupRoot.ApplyPopupRendering(base.transform, m_popupRenderingComponents, overrideLayer: true, base.gameObject.layer);
		}
		base.gameObject.GetComponent<Widget>().RegisterDoneChangingStatesListener(UpdateVisibleAnimatedEmotes, null, callImmediatelyIfSet: true, doOnce: true);
	}

	private void UpdateVisibleAnimatedEmotes(object unused)
	{
		int currentPageIndex = m_pageInfoDataModel.PageNumber - 1;
		int maxItemsOnPage = ((currentPageIndex == 0) ? (ItemsPerPage - 1) : ItemsPerPage);
		int startIndex = ((currentPageIndex != 0) ? (currentPageIndex * ItemsPerPage - 1) : 0);
		List<Animator> bgEmoteAnimators = new List<Animator>();
		for (int i = startIndex; i < startIndex + maxItemsOnPage && i < m_rewardItems.Count; i++)
		{
			WidgetInstance item = m_rewardItems[i];
			if (item.gameObject.activeInHierarchy && item.GetDataModel<RewardItemDataModel>().ItemType == RewardItemType.BATTLEGROUNDS_EMOTE)
			{
				Animator animator = item.GetComponentInChildren<Animator>();
				if (animator == null || animator.runtimeAnimatorController == null)
				{
					Debug.LogError("LargeBundleProductPage: Failed to find animator for emote on " + item.name + ". Animation will not play.");
				}
				else
				{
					bgEmoteAnimators.Add(animator);
				}
			}
		}
		if (m_bgEmoteAnimationController != null)
		{
			m_bgEmoteAnimationController.SetBGEmoteAnimators(bgEmoteAnimators);
			m_bgEmoteAnimationController.StartEmoteAnimation();
		}
	}

	private bool HasRewardItemLists(DataModelList<RewardItemDataModel> items)
	{
		foreach (RewardItemDataModel item in items)
		{
			if (item.ItemType == RewardItemType.BATTLEGROUNDS_EMOTE_PILE)
			{
				return true;
			}
		}
		return false;
	}

	private DataModelList<RewardItemDataModel> ExpandRewardItemLists(DataModelList<RewardItemDataModel> items)
	{
		DataModelList<RewardItemDataModel> expandedList = new DataModelList<RewardItemDataModel>();
		foreach (RewardItemDataModel item in items)
		{
			if (item.ItemType == RewardItemType.BATTLEGROUNDS_EMOTE_PILE)
			{
				foreach (BattlegroundsEmoteDataModel emote in item.BGEmotePile)
				{
					RewardItemDataModel newEmote = new RewardItemDataModel
					{
						ItemType = RewardItemType.BATTLEGROUNDS_EMOTE,
						ItemId = 0,
						BGEmote = emote
					};
					expandedList.Add(newEmote);
				}
			}
			else
			{
				expandedList.Add(item);
			}
		}
		return expandedList;
	}

	private void OnDisable()
	{
		DisablePopupRendering();
	}

	public void EnablePopupRendering(IPopupRoot popupRoot)
	{
		m_popupRoot = popupRoot;
	}

	public void DisablePopupRendering()
	{
		if (m_popupRoot != null)
		{
			if (m_popupRenderingComponents != null)
			{
				m_popupRoot.CleanupPopupRendering(m_popupRenderingComponents);
			}
			m_popupRoot = null;
		}
	}

	public bool HandlesChildPropagation()
	{
		return false;
	}
}
