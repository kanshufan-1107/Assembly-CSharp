using System.Collections;
using System.Collections.Generic;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

[RequireComponent(typeof(WidgetTemplate))]
public class RankedPlayDisplay : MonoBehaviour
{
	[SerializeField]
	private Transform m_medalBone;

	[SerializeField]
	private VisualController m_rankContainerVisualController;

	[SerializeField]
	private Widget m_rewardsContainerWidget;

	[SerializeField]
	private AsyncReference m_rankedMedalWidgetReference;

	[SerializeField]
	private AsyncReference m_starMultiplierWidgetReference;

	[SerializeField]
	private TooltipZone m_starMultiplierTooltipZone;

	[SerializeField]
	private Vector3 m_rewardListPos;

	[SerializeField]
	private float m_rewardListDeviceScale = 1f;

	[SerializeField]
	private float m_rewardListScaleSmall = 1f;

	[SerializeField]
	private float m_rewardListScaleWide = 1f;

	[SerializeField]
	private float m_rewardListScaleExtraWide = 1f;

	[SerializeField]
	private List<PlayMakerFSM> formatChangeGlowFSMs;

	[SerializeField]
	private List<PlayMakerFSM> newDeckFormatChangeGlowFSMs;

	private bool m_inSetRotationTutorial;

	private VisualsFormatType m_currentVisualsFormatType;

	private RankedPlayDataModel m_rankedChestDataModel;

	private Widget m_starMultiplierWidget;

	private Widget m_rankedMedalWidget;

	private Widget m_widget;

	private WidgetInstance m_rankedRewardListWidget;

	private RankedRewardList m_rankedRewardList;

	private bool m_isShowingRewardsList;

	private bool m_isDesiredHidden;

	private Coroutine m_delayedVisibilityChange;

	private const string MEDAL_BUTTON_CLICKED = "MEDAL_BUTTON_CLICKED";

	private const string SHOW_MEDAL_TOOLTIP = "SHOW_MEDAL_TOOLTIP";

	private const string HIDE_MEDAL_TOOLTIP = "HIDE_MEDAL_TOOLTIP";

	private const string CHEST_BUTTON_CLICKED = "CHEST_BUTTON_CLICKED";

	private const string SHOW_CHEST_TOOLTIP = "SHOW_CHEST_TOOLTIP";

	private const string HIDE_CHEST_TOOLTIP = "HIDE_CHEST_TOOLTIP";

	private const string ENABLE_CLICKABLES = "POPUP_CLOSED_ENABLE_CLICKABLES";

	private const string DISABLE_CLICKABLES = "POPUP_OPEN_DISABLE_CLICKABLES";

	private static readonly Dictionary<FormatType, string> s_TooltipTitleStringMap = new Dictionary<FormatType, string>
	{
		{
			FormatType.FT_STANDARD,
			"GLOBAL_MEDAL_TOOLTIP_BEST_RANK_STANDARD"
		},
		{
			FormatType.FT_WILD,
			"GLOBAL_MEDAL_TOOLTIP_BEST_RANK_WILD"
		},
		{
			FormatType.FT_CLASSIC,
			"GLOBAL_MEDAL_TOOLTIP_BEST_RANK_CLASSIC"
		},
		{
			FormatType.FT_TWIST,
			"GLOBAL_MEDAL_TOOLTIP_BEST_RANK_TWIST"
		}
	};

	private static readonly Dictionary<FormatType, string> s_TooltipBodyStringMap = new Dictionary<FormatType, string>
	{
		{
			FormatType.FT_STANDARD,
			"GLOBAL_MEDAL_TOOLTIP_BODY_STANDARD"
		},
		{
			FormatType.FT_WILD,
			"GLOBAL_MEDAL_TOOLTIP_BODY_WILD"
		},
		{
			FormatType.FT_CLASSIC,
			"GLOBAL_MEDAL_TOOLTIP_BODY_CLASSIC"
		},
		{
			FormatType.FT_TWIST,
			"GLOBAL_MEDAL_TOOLTIP_BODY_TWIST"
		}
	};

	private void Awake()
	{
		m_currentVisualsFormatType = VisualsFormatTypeExtensions.ToVisualsFormatType(Options.GetFormatType(), Options.GetInRankedPlayMode());
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterEventListener(OnRankedPlayDisplayEvent);
	}

	private void Start()
	{
		UpdateRankContainerVisualController();
		m_rewardsContainerWidget.RegisterReadyListener(delegate
		{
			UpdateRewardsContainerWidget();
		});
		m_rankedMedalWidgetReference.RegisterReadyListener<Widget>(OnRankedMedalWidgetReady);
		m_starMultiplierWidgetReference.RegisterReadyListener<Widget>(OnStarMultiplierWidgetReady);
	}

	private void OnDestroy()
	{
		DestroyRankedRewardsList();
	}

	public void UpdateMode(VisualsFormatType newVisualsFormatType)
	{
		DeckPickerTrayDisplay.Get().UpdateRankedClassWinsPlate();
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			DeckPickerTrayDisplay.Get().ToggleRankedDetailsTray(newVisualsFormatType.IsRanked());
		}
		DeckPickerTrayDisplay.Get().SetPlayButtonText(GameStrings.Get("GLOBAL_PLAY"));
	}

	public void StartSetRotationTutorial()
	{
		m_inSetRotationTutorial = true;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			DeckPickerTrayDisplay.Get().ToggleRankedDetailsTray(shown: true);
		}
		m_currentVisualsFormatType = VisualsFormatType.VFT_STANDARD;
		Hide();
		DeckPickerTrayDisplay.Get().SetPlayButtonText(GameStrings.Get("GLOBAL_PLAY"));
		DeckPickerTrayDisplay.Get().SetPlayButtonTextAlpha(0f);
		DeckPickerTrayDisplay.Get().UpdateRankedClassWinsPlate();
		OnMedalChanged(TournamentDisplay.Get().GetCurrentMedalInfo());
	}

	public void OnMedalChanged(NetCache.NetCacheMedalInfo medalInfo)
	{
		MedalInfoTranslator mit = new MedalInfoTranslator(medalInfo);
		bool isTooltipEnabled = false;
		bool hasEarnedCardBack = mit.HasEarnedSeasonCardBack();
		m_rankedChestDataModel = mit.CreateDataModel(m_currentVisualsFormatType.ToFormatType(), RankedMedal.DisplayMode.Chest, isTooltipEnabled, hasEarnedCardBack);
		UpdateRankContainerVisualController();
		UpdateRewardsContainerWidget();
	}

	public void UpdateRankContainerVisualController()
	{
		NetCache.NetCacheMedalInfo medalInfo = TournamentDisplay.Get().GetCurrentMedalInfo();
		if (medalInfo != null)
		{
			MedalInfoTranslator medalInfoTranslator = new MedalInfoTranslator(medalInfo);
			RankedPlayDataModel rankedDataModel = medalInfoTranslator.CreateDataModel(isTooltipEnabled: false, hasEarnedCardBack: medalInfoTranslator.HasEarnedSeasonCardBack(), formatType: m_currentVisualsFormatType.ToFormatType(), mode: RankedMedal.DisplayMode.Stars);
			m_rankContainerVisualController.BindDataModel(rankedDataModel);
			if ((bool)UniversalInputManager.UsePhoneUI && m_rankedMedalWidget != null)
			{
				m_rankedMedalWidget.BindDataModel(rankedDataModel);
			}
		}
	}

	public void UpdateRewardsContainerWidget()
	{
		if (m_rewardsContainerWidget.IsReady && m_rankedChestDataModel != null)
		{
			m_rewardsContainerWidget.BindDataModel(m_rankedChestDataModel);
			if (m_isDesiredHidden)
			{
				Hide();
			}
			else if ((bool)UniversalInputManager.UsePhoneUI)
			{
				m_rewardsContainerWidget.SetLayerOverride(GameLayer.IgnoreFullScreenEffects);
			}
		}
	}

	public void OnSwitchFormat(VisualsFormatType newVisualsFormatType)
	{
		if (!m_inSetRotationTutorial)
		{
			if (m_currentVisualsFormatType != newVisualsFormatType)
			{
				m_currentVisualsFormatType = newVisualsFormatType;
				OnMedalChanged(TournamentDisplay.Get().GetCurrentMedalInfo());
			}
			UpdateMode(newVisualsFormatType);
			if (!Options.GetInRankedPlayMode())
			{
				EventFunctions.TriggerEvent(base.transform, "POPUP_OPEN_DISABLE_CLICKABLES");
			}
			else
			{
				EventFunctions.TriggerEvent(base.transform, "POPUP_CLOSED_ENABLE_CLICKABLES");
			}
		}
	}

	public void Show(float delay = 0f)
	{
		if (m_isDesiredHidden)
		{
			m_isDesiredHidden = false;
			StopAndClearCoroutine(ref m_delayedVisibilityChange);
			if (delay > 0f)
			{
				m_delayedVisibilityChange = StartCoroutine(WaitThenSetVisibility(delay, visible: true));
			}
			else
			{
				SetVisibility(visible: true);
			}
		}
	}

	public void Hide(float delay = 0f)
	{
		if (!m_isDesiredHidden)
		{
			m_isDesiredHidden = true;
			StopAndClearCoroutine(ref m_delayedVisibilityChange);
			if (delay > 0f)
			{
				StartCoroutine(WaitThenSetVisibility(delay, visible: false));
			}
			else
			{
				SetVisibility(visible: false);
			}
		}
	}

	private IEnumerator WaitThenSetVisibility(float delay, bool visible)
	{
		yield return new WaitForSeconds(delay);
		SetVisibility(visible);
	}

	private void SetVisibility(bool visible)
	{
		if (visible)
		{
			m_widget.Show();
			m_rewardsContainerWidget.Show();
			if ((bool)UniversalInputManager.UsePhoneUI && m_rankedMedalWidget != null)
			{
				m_rankedMedalWidget.Show();
			}
		}
		else
		{
			m_rewardsContainerWidget.Hide();
			m_widget.Hide();
		}
	}

	private void StopAndClearCoroutine(ref Coroutine co)
	{
		if (co != null)
		{
			StopCoroutine(co);
			co = null;
		}
	}

	public void PlayTransitionGlowBurstsForNonNewDeckFSMs(string fxEvent)
	{
		foreach (PlayMakerFSM fsm in formatChangeGlowFSMs)
		{
			if (fsm != null)
			{
				fsm.SendEvent(fxEvent);
			}
		}
	}

	public void PlayTransitionGlowBurstsForNewDeckFSMs(string fxEvent)
	{
		if (string.IsNullOrEmpty(fxEvent))
		{
			return;
		}
		foreach (PlayMakerFSM fsm in newDeckFormatChangeGlowFSMs)
		{
			if (fsm != null)
			{
				fsm.SendEvent(fxEvent);
			}
		}
	}

	private void OnRankedMedalWidgetReady(Widget widget)
	{
		m_rankedMedalWidget = widget;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			widget.transform.parent = DeckPickerTrayDisplay.Get().m_medalBone_phone;
			widget.SetLayerOverride(GameLayer.IgnoreFullScreenEffects);
		}
		else
		{
			widget.transform.parent = m_medalBone;
		}
		widget.transform.localScale = Vector3.one;
		widget.transform.localPosition = Vector3.zero;
		OnMedalChanged(TournamentDisplay.Get().GetCurrentMedalInfo());
		if (!Options.GetInRankedPlayMode())
		{
			EventFunctions.TriggerEvent(base.transform, "POPUP_OPEN_DISABLE_CLICKABLES");
		}
		else
		{
			EventFunctions.TriggerEvent(base.transform, "POPUP_CLOSED_ENABLE_CLICKABLES");
		}
	}

	private void OnStarMultiplierWidgetReady(Widget widget)
	{
		m_starMultiplierWidget = widget;
	}

	private void OnRankedPlayDisplayEvent(string eventName)
	{
		switch (eventName)
		{
		case "MEDAL_BUTTON_CLICKED":
			m_widget.TriggerEvent("POPUP_OPEN_DISABLE_CLICKABLES");
			DialogManager.Get().ShowRankedIntroPopUp(delegate
			{
				m_widget.TriggerEvent("POPUP_CLOSED_ENABLE_CLICKABLES");
			});
			break;
		case "SHOW_MEDAL_TOOLTIP":
			ShowMedalTooltip();
			break;
		case "HIDE_MEDAL_TOOLTIP":
			HideMedalTooltip();
			break;
		}
		switch (eventName)
		{
		case "CHEST_BUTTON_CLICKED":
			StartCoroutine(ShowRankedRewardList());
			break;
		case "SHOW_CHEST_TOOLTIP":
			ShowChestTooltip();
			break;
		case "HIDE_CHEST_TOOLTIP":
			HideChestTooltip();
			break;
		}
	}

	private void ShowMedalTooltip()
	{
		FormatType currentFormatType = Options.GetFormatType();
		string bodyGlueString;
		string bodyText = (m_rankedChestDataModel.IsLegend ? GameStrings.Format("GLOBAL_MEDAL_TOOLTIP_BODY_LEGEND") : ((!s_TooltipBodyStringMap.TryGetValue(currentFormatType, out bodyGlueString)) ? ("UNKNOWN FORMAT TYPE " + currentFormatType) : GameStrings.Format(bodyGlueString)));
		string titleGlueString;
		string titleText = ((!s_TooltipTitleStringMap.TryGetValue(currentFormatType, out titleGlueString)) ? ("UNKNOWN FORMAT TYPE " + m_rankedChestDataModel.FormatType) : GameStrings.Format(titleGlueString, m_rankedChestDataModel.RankName));
		TooltipZone tooltipZone = m_rankContainerVisualController.GetComponent<TooltipZone>();
		tooltipZone.ShowTooltip(titleText, bodyText, 5f);
		int starsPerWin = RankMgr.Get().GetLocalPlayerMedalInfo().GetCurrentMedal(currentFormatType)
			.starsPerWin;
		if (starsPerWin > 1)
		{
			string starMultiplierHeadline = GameStrings.Format("GLUE_TOURNAMENT_STAR_MULT_HEAD", starsPerWin);
			string starMultiplierBody = GameStrings.Format("GLUE_TOURNAMENT_STAR_MULT_BODY", starsPerWin);
			if (m_starMultiplierTooltipZone != null)
			{
				m_starMultiplierTooltipZone.ShowTooltip(starMultiplierHeadline, starMultiplierBody, 5f);
				m_starMultiplierTooltipZone.AnchorTooltipTo(tooltipZone.GetTooltipObject(), Anchor.BOTTOM_XZ, Anchor.TOP_XZ);
			}
		}
	}

	private void HideMedalTooltip()
	{
		m_rankContainerVisualController.GetComponent<TooltipZone>().HideTooltip();
		if (m_starMultiplierTooltipZone != null)
		{
			m_starMultiplierTooltipZone.HideTooltip();
		}
	}

	private IEnumerator ShowRankedRewardList()
	{
		if (m_isShowingRewardsList)
		{
			yield break;
		}
		m_widget.TriggerEvent("POPUP_OPEN_DISABLE_CLICKABLES");
		m_isShowingRewardsList = true;
		if (m_rankedRewardListWidget == null)
		{
			m_rankedRewardListWidget = WidgetInstance.Create(RankMgr.RANKED_REWARD_LIST_POPUP);
			CoreRewardsDataModel coreRewardsData = m_widget.GetDataModel<CoreRewardsDataModel>();
			if (!GameUtils.HasEarnedAllVanillaClassCards() && coreRewardsData != null)
			{
				m_rankedRewardListWidget.TriggerEvent("SHOW_CORE_CARD");
				m_rankedRewardListWidget.BindDataModel(coreRewardsData);
			}
			m_rankedRewardListWidget.RegisterReadyListener(delegate
			{
				OnRankedRewardListPopupWidgetReady();
			});
			m_rankedRewardListWidget.WillLoadSynchronously = true;
			m_rankedRewardListWidget.Initialize();
		}
		while (m_rankedRewardList == null || m_rankedRewardListWidget.IsChangingStates)
		{
			yield return null;
		}
		UIContext.GetRoot().ShowPopup(m_rankedRewardListWidget.gameObject);
		m_rankedRewardListWidget.Show();
		m_rankedRewardListWidget.TriggerEvent("SHOW");
		yield return new WaitForSeconds(0.25f);
	}

	private void OnRankedRewardListPopupWidgetReady()
	{
		OverlayUI.Get().AddGameObject(m_rankedRewardListWidget.gameObject);
		m_rankedRewardListWidget.transform.localPosition = m_rewardListPos;
		float rewardListScale = TransformUtil.GetAspectRatioDependentValue(m_rewardListScaleSmall, m_rewardListScaleWide, m_rewardListScaleExtraWide);
		m_rankedRewardListWidget.transform.localScale = Vector3.one * rewardListScale * m_rewardListDeviceScale;
		m_rankedRewardListWidget.RegisterEventListener(WidgetEventListener_RewardsList);
		m_rankedRewardList = m_rankedRewardListWidget.GetComponentInChildren<RankedRewardList>();
		m_rankedRewardListWidget.Hide();
		UpdateRankedRewardList();
	}

	private void WidgetEventListener_RewardsList(string eventName)
	{
		if (eventName.Equals("HIDE"))
		{
			HideRankedRewardsList();
		}
	}

	private void HideRankedRewardsList()
	{
		m_widget.TriggerEvent("POPUP_CLOSED_ENABLE_CLICKABLES");
		UIContext.GetRoot().DismissPopup(m_rankedRewardListWidget.gameObject);
		m_isShowingRewardsList = false;
	}

	private void UpdateRankedRewardList()
	{
		if (m_rankedRewardList != null)
		{
			m_rankedRewardList.Initialize(new MedalInfoTranslator(TournamentDisplay.Get().GetCurrentMedalInfo()));
		}
	}

	private void DestroyRankedRewardsList()
	{
		if (m_rankedRewardListWidget != null)
		{
			Object.Destroy(m_rankedRewardListWidget.gameObject);
		}
		m_isShowingRewardsList = false;
	}

	private void ShowChestTooltip()
	{
		m_rewardsContainerWidget.GetComponent<TooltipZone>().ShowTooltip(GameStrings.Get("GLOBAL_PROGRESSION_RANKED_REWARDS_TOOLTIP_TITLE"), GameStrings.Get("GLOBAL_PROGRESSION_RANKED_REWARDS_TOOLTIP"), 5f);
	}

	private void HideChestTooltip()
	{
		m_rewardsContainerWidget.GetComponent<TooltipZone>().HideTooltip();
	}

	public void SetCoreRewardsDataModel(CoreRewardsDataModel dataModel)
	{
		m_widget.BindDataModel(dataModel);
	}
}
