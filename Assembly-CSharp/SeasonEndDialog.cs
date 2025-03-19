using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using Blizzard.T5.Services;
using Hearthstone.DataModels;
using Hearthstone.UI;
using HutongGames.PlayMaker;
using PegasusShared;
using UnityEngine;

public class SeasonEndDialog : DialogBase
{
	public class SeasonEndInfo
	{
		public int m_seasonID;

		public int m_leagueId;

		public int m_starLevelAtEndOfSeason;

		public int m_bestStarLevelAtEndOfSeason;

		public int m_legendIndex;

		public List<RewardData> m_rankedRewards;

		public List<long> m_noticesToAck = new List<long>();

		public FormatType m_formatType;

		public bool m_wasLimitedByBestEverStarLevel;
	}

	private enum MODE
	{
		RANK_EARNED,
		CHEST_EARNED,
		SEASON_WELCOME,
		REDUCED_WELCOME,
		REMINDER_CHEST,
		STAR_MULTIPLIER,
		REMINDER_CARDBACK
	}

	public GameObject m_root;

	public UIBButton m_okayButton;

	public GameObject m_boostedMedalBone;

	public GameObject m_boostedMedalLeftFiligreeBone;

	public GameObject m_boostedMedalRightFiligreeBone;

	public GameObject m_rewardChestPage;

	public PegUIElement m_rewardChestLegacy;

	public UberText m_rewardChestHeader;

	public UberText m_rewardChestInstructions;

	public GameObject m_rewardChestLeftFiligreeBone;

	public GameObject m_rewardChestRightFiligreeBone;

	public GameObject m_rewardBoxesBone;

	public AsyncReference m_rankedMedalWidgetReference;

	public AsyncReference m_starMultiplierWidgetReference;

	public AsyncReference m_rankedRewardChestWidgetReference;

	public UberText m_header;

	public UberText m_rankAchieved;

	public UberText m_rankName;

	public GameObject m_ribbon;

	public GameObject m_nameFlourish;

	public GameObject m_boostedFlourish;

	public GameObject m_welcomeItems;

	public GameObject m_leftFiligree;

	public GameObject m_rightFiligree;

	public UberText m_welcomeDetails;

	public UberText m_welcomeTitle;

	public GameObject m_shieldIcon;

	public GameObject m_bonusStarItems;

	public UberText m_bonusStarTitle;

	public UberText m_bonusStarLabel;

	public UberText m_bonusStarFinePrint;

	public GameObject m_bonusStarFlourish;

	public Material m_transparentMaterial;

	public PlayMakerFSM m_medalPlayMaker;

	public GameObject m_seasonFramePage;

	public GameObject m_legendaryGem;

	public List<PegUIElement> m_rewardChests;

	public GameObject m_reminderChestRightFiligreeBone;

	public GameObject m_reminderChestLeftFiligreeBone;

	public GameObject m_reminderRewardsChest;

	public ProgressBar m_progressBar;

	public AsyncReference m_rankedCardBackProgressWidgetReference;

	public UberText m_cardBackReminderDetails;

	public AsyncReference m_rankedIntroPopUpWidgetReference;

	private SeasonEndInfo m_seasonEndInfo;

	private TranslatedMedalInfo m_seasonBestMedalInfo;

	private TranslatedMedalInfo m_seasonEndMedalInfo;

	private TranslatedMedalInfo m_currentMedalInfo;

	private bool m_earnedRewardChest;

	private bool m_wasPrevSeasonLegacy;

	private bool m_isNewSeasonLegacy;

	private MODE m_currentMode;

	private RankedMedalWrapper m_rankedMedal;

	private RankedPlayDataModel m_seasonBestRankedDataModel;

	private RankedPlayDataModel m_currentRankedDataModel;

	private RankedPlayDataModel m_rankedChestDataModel;

	private Widget m_rankedMedalWidget;

	private bool m_showMedal;

	private bool m_chestOpened;

	private Widget m_starMultiplierWidget;

	private Widget m_rankedCardBackProgressWidget;

	private Widget m_rankedIntroPopUpWidget;

	private bool m_skipRankedIntroPopup;

	private Widget m_rankedRewardChestWidget;

	private NetCache.NetCacheRewardProgress m_rewardProgress;

	private bool m_isOkayButtonHidden;

	private const string REWARD_CHEST_NAME_STRING_FORMAT = "GLOBAL_REWARD_CHEST_TIER{0}";

	private const string REWARD_CHEST_EARNED_STRING_FORMAT = "GLOBAL_REWARD_CHEST_TIER{0}_EARNED";

	protected override void Awake()
	{
		base.Awake();
		SceneMgr.Get().RegisterSceneLoadedEvent(OnSceneLoaded);
		if (UniversalInputManager.Get().IsTouchMode())
		{
			m_rewardChestInstructions.Text = GameStrings.Format("GLOBAL_SEASON_END_CHEST_INSTRUCTIONS_TOUCH");
		}
		m_okayButton.SetText(GameStrings.Get("GLOBAL_BUTTON_NEXT"));
		m_okayButton.AddEventListener(UIEventType.RELEASE, OkayButtonReleased);
		NetCache.Get().RegisterUpdatedListener(typeof(NetCache.NetCacheRewardProgress), OnNetCacheRewardProgressUpdated);
		NetCache.Get().ReloadNetObject<NetCache.NetCacheRewardProgress>();
	}

	private void Start()
	{
		m_rankedMedalWidgetReference.RegisterReadyListener<Widget>(OnRankedMedalWidgetReady);
		m_starMultiplierWidgetReference.RegisterReadyListener<Widget>(OnStarMultiplierWidgetReady);
		m_rankedCardBackProgressWidgetReference.RegisterReadyListener<Widget>(OnRankedCardBackProgressWidgetReady);
		m_rankedIntroPopUpWidgetReference.RegisterReadyListener<Widget>(OnRankedIntroPopUpWidgetReady);
		m_rankedRewardChestWidgetReference.RegisterReadyListener<Widget>(OnRankedRewardChestWidgetReady);
	}

	public void Init(SeasonEndInfo info)
	{
		m_seasonEndInfo = info;
		m_header.Text = GameUtils.GetRankedSeasonName(info.m_seasonID);
		m_earnedRewardChest = info.m_rankedRewards != null && info.m_rankedRewards.Count > 0;
		m_seasonEndMedalInfo = MedalInfoTranslator.CreateTranslatedMedalInfo(info.m_formatType, info.m_leagueId, info.m_starLevelAtEndOfSeason, info.m_legendIndex);
		m_seasonBestMedalInfo = MedalInfoTranslator.CreateTranslatedMedalInfo(info.m_formatType, info.m_leagueId, info.m_bestStarLevelAtEndOfSeason, info.m_legendIndex);
		m_currentMedalInfo = RankMgr.Get().GetLocalPlayerMedalInfo().GetCurrentMedal(info.m_formatType);
		m_wasPrevSeasonLegacy = RankMgr.Get().UseLegacyRankedPlay(m_seasonEndInfo.m_leagueId);
		m_isNewSeasonLegacy = RankMgr.Get().UseLegacyRankedPlay(m_currentMedalInfo.leagueId);
		m_seasonBestRankedDataModel = m_seasonBestMedalInfo.CreateDataModel(RankedMedal.DisplayMode.Default);
		m_rankedChestDataModel = m_seasonBestMedalInfo.CreateDataModel(RankedMedal.DisplayMode.Chest);
		m_currentRankedDataModel = m_currentMedalInfo.CreateDataModel(RankedMedal.DisplayMode.Default);
		m_showMedal = true;
		m_rankName.Text = m_seasonBestMedalInfo.GetRankName();
		m_cardBackReminderDetails.Text = GameStrings.Format("GLOBAL_REMINDER_CARDBACK_SEASON_END_DIALOG", RankMgr.Get().GetLocalPlayerMedalInfo().GetSeasonCardBackMinWins());
		foreach (PegUIElement rewardChest in m_rewardChests)
		{
			rewardChest.gameObject.SetActive(value: false);
		}
		if (m_earnedRewardChest && m_wasPrevSeasonLegacy)
		{
			InitLegacyChest();
		}
		m_progressBar.SetProgressBar(0f);
	}

	private void InitLegacyChest()
	{
		int chestIndex = m_seasonBestMedalInfo.RankConfig.RewardChestVisualIndex;
		m_rewardChestLegacy = m_rewardChests[chestIndex];
		m_rewardChestLegacy.gameObject.SetActive(value: true);
		m_rewardChestLegacy.AddEventListener(UIEventType.RELEASE, LegacyChestButtonReleased);
		m_medalPlayMaker.FsmVariables.GetFsmGameObject("RankChest").Value = m_rewardChestLegacy.gameObject;
		UberText[] uberTexts = m_rewardChestLegacy.GetComponentsInChildren<UberText>(includeInactive: true);
		if (uberTexts.Length != 0)
		{
			uberTexts[0].Text = m_seasonBestMedalInfo.GetMedalText();
		}
		m_rewardChestHeader.Text = GetChestEarnedText();
	}

	private void InitNewChest()
	{
		PlayMakerFSM activeChest = m_rankedRewardChestWidget.GetComponentInChildren<PlayMakerFSM>();
		if (activeChest != null)
		{
			m_medalPlayMaker.FsmVariables.GetFsmGameObject("RankChest").Value = activeChest.gameObject;
		}
		m_rewardChestHeader.Text = "GLOBAL_REWARD_CHEST_HEADER";
	}

	protected override void OnDestroy()
	{
		if (ServiceManager.TryGet<SceneMgr>(out var sceneMgr))
		{
			sceneMgr.UnregisterSceneLoadedEvent(OnSceneLoaded);
		}
	}

	public void ShowMedal()
	{
		m_showMedal = true;
		UpdateRankedMedalWidget();
	}

	public void HideMedal()
	{
		m_showMedal = false;
		UpdateRankedMedalWidget();
	}

	private void ShowNewRewardChestWidget()
	{
		if (!m_wasPrevSeasonLegacy)
		{
			m_rankedRewardChestWidget.Show();
		}
	}

	public void ShowRewardChestPage()
	{
		m_rewardChestPage.SetActive(value: true);
		m_leftFiligree.transform.position = m_rewardChestLeftFiligreeBone.transform.position;
		m_rightFiligree.transform.position = m_rewardChestRightFiligreeBone.transform.position;
		iTween.FadeTo(m_leftFiligree.gameObject, 1f, 0.5f);
		iTween.FadeTo(m_rightFiligree.gameObject, 1f, 0.5f);
		if (m_wasPrevSeasonLegacy && m_seasonBestMedalInfo.IsLegendRank())
		{
			m_legendaryGem.SetActive(value: true);
		}
	}

	public void HideRewardChestPage()
	{
		m_rewardChestPage.SetActive(value: false);
		if (!m_wasPrevSeasonLegacy)
		{
			m_rankedRewardChestWidget.Hide();
		}
	}

	private void DisableOkayButton(bool hideButton)
	{
		if (hideButton && !m_isOkayButtonHidden)
		{
			m_okayButton.Flip(faceUp: false);
			m_isOkayButtonHidden = true;
		}
		m_okayButton.SetEnabled(enabled: false);
		m_okayButton.GetComponent<UIBHighlight>().Reset();
	}

	private void EnableOkayButton()
	{
		if (m_isOkayButtonHidden)
		{
			m_okayButton.Flip(faceUp: true);
			m_isOkayButtonHidden = false;
		}
		m_okayButton.SetEnabled(enabled: true);
	}

	public void MedalAnimationFinished()
	{
		if (m_currentMode == MODE.REDUCED_WELCOME)
		{
			if (m_isNewSeasonLegacy)
			{
				GotoChestReminder();
			}
			else
			{
				GoToCardBackReminder();
			}
		}
		else if (m_earnedRewardChest)
		{
			DisableOkayButton(hideButton: true);
			m_currentMode = MODE.CHEST_EARNED;
			m_medalPlayMaker.SendEvent("RevealRewardChest");
			iTween.FadeTo(m_rankAchieved.gameObject, 0f, 0.5f);
		}
		else
		{
			GotoBonusStarsOrWelcome();
		}
	}

	public void GotoBonusStarsOrWelcome()
	{
		string newSeasonName = GameUtils.GetRankedSeasonName(m_rewardProgress.Season);
		m_header.Text = newSeasonName;
		if (m_currentMedalInfo.starsPerWin > 1 && !m_isNewSeasonLegacy)
		{
			GoToStarMultiplier();
		}
		else if (m_currentMedalInfo.starLevel < m_seasonEndMedalInfo.starLevel && m_wasPrevSeasonLegacy)
		{
			GotoReducedMedal();
		}
		else if (!m_earnedRewardChest)
		{
			GotoSeasonWelcome(newSeasonName);
		}
		else if (m_isNewSeasonLegacy)
		{
			GotoChestReminder();
		}
		else
		{
			GoToCardBackReminder();
		}
	}

	public void GoToStarMultiplier()
	{
		m_currentMode = MODE.STAR_MULTIPLIER;
		m_welcomeItems.SetActive(value: false);
		if (m_skipRankedIntroPopup)
		{
			StartCoroutine(DoPageTear());
			return;
		}
		HideRewardChestPage();
		m_bonusStarItems.SetActive(value: true);
		m_bonusStarTitle.Text = GameStrings.Get("GLOBAL_SEASON_END_STAR_MULTIPLIER_TITLE");
		m_bonusStarLabel.Text = GameStrings.Get("GLOBAL_SEASON_END_STAR_MULTIPLIER_LABEL");
		StartCoroutine(FadeWidgetIn(m_starMultiplierWidget, 0f));
		iTween.FadeTo(m_bonusStarItems, 1f, 0f);
		EnableOkayButton();
	}

	public void GotoReducedMedal()
	{
		m_currentMode = MODE.REDUCED_WELCOME;
		StartCoroutine(DoPageTear());
		HideRewardChestPage();
		m_welcomeItems.SetActive(value: false);
		m_bonusStarItems.SetActive(value: true);
		UpdateRankedMedalWidget();
		m_bonusStarLabel.Text = m_currentMedalInfo.GetRankName();
		m_bonusStarTitle.Text = GameStrings.Get("GLOBAL_SEASON_END_BONUS_STAR_TITLE");
		UpdateBonusStarFinePrint();
	}

	public void GotoChestReminder()
	{
		m_currentMode = MODE.REMINDER_CHEST;
		HideRewardChestPage();
		m_welcomeItems.SetActive(value: false);
		m_bonusStarItems.SetActive(value: false);
		StartCoroutine(DoPageTear());
		int minWinsForRewards = RankMgr.Get().GetLeagueRecord(m_seasonEndInfo.m_leagueId).SeasonRollRewardMinWins;
		m_progressBar.SetLabel(GameStrings.Format("GLOBAL_REWARD_PROGRESS", 0, minWinsForRewards));
	}

	public void GoToCardBackReminder()
	{
		m_currentMode = MODE.REMINDER_CARDBACK;
		HideRewardChestPage();
		m_welcomeItems.SetActive(value: false);
		m_bonusStarItems.SetActive(value: false);
		StartCoroutine(DoPageTear());
	}

	public void GoToRankedIntroPopUp()
	{
		iTween.ScaleTo(m_root, new Vector3(0f, 0f, 0f), 0.5f);
		m_rankedIntroPopUpWidget.TriggerEvent("CODE_DIALOGMANAGER_SHOW");
	}

	private void ReminderChestSummonOutFinished()
	{
		Finish();
	}

	public void GotoSeasonWelcome(string newSeasonName)
	{
		m_currentMode = MODE.SEASON_WELCOME;
		StartCoroutine(DoPageTear());
		m_welcomeItems.SetActive(value: true);
		HideRewardChestPage();
		m_bonusStarItems.SetActive(value: false);
		m_welcomeDetails.Text = GameStrings.Format("GLOBAL_SEASON_END_NEW_SEASON", newSeasonName);
	}

	public IEnumerator DoPageTear()
	{
		m_medalPlayMaker.SendEvent("PageTear");
		yield return new WaitForSeconds(0.69f);
		bool deferEnableOkayButton = false;
		if (m_currentMode == MODE.REMINDER_CHEST)
		{
			m_leftFiligree.transform.position = m_reminderChestLeftFiligreeBone.transform.position;
			m_rightFiligree.transform.position = m_reminderChestRightFiligreeBone.transform.position;
			iTween.FadeTo(m_leftFiligree.gameObject, 1f, 0.5f);
			iTween.FadeTo(m_rightFiligree.gameObject, 1f, 0.5f);
			m_reminderRewardsChest.SetActive(value: true);
			m_reminderRewardsChest.GetComponent<PlayMakerFSM>().SendEvent("SummonIn");
			EnableOkayButton();
			m_okayButton.SetText("GLOBAL_DONE");
		}
		else if (m_currentMode == MODE.REDUCED_WELCOME)
		{
			m_leftFiligree.transform.position = m_boostedMedalLeftFiligreeBone.transform.position;
			m_rightFiligree.transform.position = m_boostedMedalRightFiligreeBone.transform.position;
			if (m_seasonBestMedalInfo.IsLegendRank())
			{
				m_medalPlayMaker.SendEvent("JustMedalIn");
			}
			else
			{
				m_medalPlayMaker.SendEvent("MedalBannerIn");
			}
			deferEnableOkayButton = true;
		}
		else if (m_currentMode == MODE.STAR_MULTIPLIER)
		{
			HideRewardChestPage();
			m_bonusStarItems.SetActive(value: true);
			m_bonusStarTitle.Text = GameStrings.Get("GLOBAL_SEASON_END_STAR_MULTIPLIER_TITLE");
			m_bonusStarLabel.Text = GameStrings.Get("GLOBAL_SEASON_END_STAR_MULTIPLIER_LABEL");
			StartCoroutine(FadeWidgetIn(m_starMultiplierWidget, 0.5f));
			iTween.FadeTo(m_bonusStarItems, 1f, 0.5f);
			EnableOkayButton();
		}
		else if (m_currentMode == MODE.REMINDER_CARDBACK)
		{
			m_rankedCardBackProgressWidget.Show();
			m_cardBackReminderDetails.Show();
			m_okayButton.SetText("GLOBAL_DONE");
		}
		if (!deferEnableOkayButton)
		{
			EnableOkayButton();
		}
	}

	public void MedalInFinished()
	{
		EnableOkayButton();
	}

	public override void Show()
	{
		StartCoroutine(ShowWhenReady());
	}

	private IEnumerator ShowWhenReady()
	{
		while (m_rewardProgress == null || m_rankedMedal == null || m_rankedMedalWidget.IsChangingStates || m_starMultiplierWidget == null || m_starMultiplierWidget.IsChangingStates || m_rankedCardBackProgressWidget == null || m_rankedCardBackProgressWidget.IsChangingStates || m_rankedRewardChestWidget == null || m_rankedRewardChestWidget.IsChangingStates)
		{
			yield return null;
		}
		if (m_earnedRewardChest && !m_wasPrevSeasonLegacy)
		{
			InitNewChest();
		}
		FadeEffectsIn();
		base.Show();
		DoShowAnimation();
		UniversalInputManager.Get().SetGameDialogActive(active: true);
		PlayShowSound();
	}

	public override void Hide()
	{
		m_seasonFramePage.SetActive(value: false);
		base.Hide();
		FadeEffectsOut();
		PlayHideSound();
	}

	protected override void OnHideAnimFinished()
	{
		UniversalInputManager.Get().SetGameDialogActive(active: false);
		base.OnHideAnimFinished();
	}

	private void Finish()
	{
		DisableOkayButton(hideButton: false);
		Hide();
		foreach (long noticeID in m_seasonEndInfo.m_noticesToAck)
		{
			Network.Get().AckNotice(noticeID);
		}
	}

	private void OkayButtonReleased(UIEvent e)
	{
		DisableOkayButton(hideButton: false);
		if (m_currentMode == MODE.REMINDER_CHEST)
		{
			m_reminderRewardsChest.GetComponent<PlayMakerFSM>().SendEvent("SummonOut");
		}
		else if (m_currentMode == MODE.SEASON_WELCOME || m_currentMode == MODE.REDUCED_WELCOME)
		{
			m_boostedFlourish.GetComponent<Renderer>().SetMaterial(m_transparentMaterial);
			iTween.FadeTo(m_bonusStarItems.gameObject, 0f, 0.5f);
			iTween.FadeTo(m_boostedFlourish.gameObject, 0f, 0.5f);
			iTween.FadeTo(m_leftFiligree.gameObject, 0f, 0.5f);
			iTween.FadeTo(m_rightFiligree.gameObject, 0f, 0.5f);
			if (m_currentMode == MODE.SEASON_WELCOME)
			{
				m_welcomeItems.SetActive(value: false);
				if (m_isNewSeasonLegacy)
				{
					GotoChestReminder();
				}
				else
				{
					GoToCardBackReminder();
				}
			}
			else
			{
				m_medalPlayMaker.SendEvent("JustMedalNoRibbon");
			}
		}
		else if (m_currentMode == MODE.RANK_EARNED)
		{
			m_ribbon.GetComponent<Renderer>().SetMaterial(m_transparentMaterial);
			m_nameFlourish.GetComponent<Renderer>().SetMaterial(m_transparentMaterial);
			iTween.FadeTo(m_nameFlourish.gameObject, 0f, 0.5f);
			iTween.FadeTo(m_rankName.gameObject, iTween.Hash("alpha", 0f, "time", 0.5f, "oncomplete", "OnRankNameHidden", "oncompletetarget", base.gameObject));
			iTween.FadeTo(m_rankAchieved.gameObject, 0f, 0.5f);
			iTween.FadeTo(m_leftFiligree.gameObject, 0f, 0.5f);
			iTween.FadeTo(m_rightFiligree.gameObject, 0f, 0.5f);
			if (m_seasonBestMedalInfo.IsLegendRank())
			{
				m_medalPlayMaker.SendEvent("JustMedal");
			}
			else
			{
				m_medalPlayMaker.SendEvent("MedalBanner");
			}
		}
		else if (m_currentMode == MODE.STAR_MULTIPLIER)
		{
			StartCoroutine(FadeWidgetOut(m_starMultiplierWidget, 0.5f));
			GoToCardBackReminder();
		}
		else if (m_currentMode == MODE.REMINDER_CARDBACK)
		{
			m_rankedCardBackProgressWidget.Hide();
			Finish();
		}
	}

	private void LegacyChestButtonReleased(UIEvent e)
	{
		if (!m_chestOpened)
		{
			m_chestOpened = true;
			m_rewardChestLegacy.GetComponent<PlayMakerFSM>().SendEvent("StartAnim");
		}
	}

	private void OpenRewards()
	{
		PrefabCallback<GameObject> onAssetLoad = delegate(AssetReference assetRef, GameObject go, object callbackData)
		{
			if (SoundManager.Get() != null)
			{
				SoundManager.Get().LoadAndPlay("card_turn_over_legendary.prefab:a8140f686bff601459e954bc23de35e0");
			}
			RewardBoxesDisplay component = go.GetComponent<RewardBoxesDisplay>();
			component.SetRewards(m_seasonEndInfo.m_rankedRewards);
			component.m_playBoxFlyoutSound = false;
			component.SetLayer(GameLayer.PerspectiveUI);
			component.UseDarkeningClickCatcher(value: true);
			component.RegisterDoneCallback(delegate
			{
				if (m_wasPrevSeasonLegacy)
				{
					m_rewardChestLegacy.GetComponent<PlayMakerFSM>().SendEvent("SummonOut");
				}
				else
				{
					PlayMakerFSM componentInChildren = m_rankedRewardChestWidget.GetComponentInChildren<PlayMakerFSM>();
					FsmGameObject fsmGameObject = componentInChildren.FsmVariables.GetFsmGameObject("OwnerObject");
					if (fsmGameObject != null)
					{
						fsmGameObject.Value = base.gameObject;
					}
					componentInChildren.SendEvent("SummonOut");
				}
			});
			component.transform.localPosition = m_rewardBoxesBone.transform.localPosition;
			component.transform.localRotation = m_rewardBoxesBone.transform.localRotation;
			component.transform.localScale = m_rewardBoxesBone.transform.localScale;
			component.AnimateRewards();
		};
		AssetLoader.Get().InstantiatePrefab(RewardBoxesDisplay.GetPrefab(m_seasonEndInfo.m_rankedRewards), onAssetLoad);
		iTween.FadeTo(m_rewardChestInstructions.gameObject, 0f, 0.5f);
	}

	public static void FadeEffectsIn()
	{
		ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignettePerspective;
		screenEffectParameters.Blur = new BlurParameters(1f, 1f);
		DialogBase.m_screenEffectsHandle.StartEffect(screenEffectParameters);
	}

	public static void FadeEffectsOut()
	{
		DialogBase.m_screenEffectsHandle.StopEffect();
	}

	public static void PlayShowSound()
	{
		SoundManager.Get().LoadAndPlay("rank_window_expand.prefab:9f3f1c260a5d8b34f9705caf4925f5cb");
	}

	public static void PlayHideSound()
	{
		SoundManager.Get().LoadAndPlay("rank_window_shrink.prefab:9c6393a1d207a07439c22f31ef405a7c");
	}

	private void OnSceneLoaded(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
		if (mode != SceneMgr.Mode.HUB)
		{
			Hide();
			Object.Destroy(base.gameObject);
		}
	}

	private void UpdateBonusStarFinePrint()
	{
		if (m_seasonEndInfo.m_wasLimitedByBestEverStarLevel)
		{
			m_bonusStarFinePrint.Text = GameStrings.Format("GLOBAL_SEASON_END_BEST_EVER_ABOVE_MAX", m_currentMedalInfo.GetMedalText());
			m_bonusStarFinePrint.Show();
		}
		else
		{
			m_bonusStarFinePrint.Hide();
		}
	}

	private void OnRankNameHidden()
	{
		m_rankName.gameObject.SetActive(value: false);
	}

	private void OnRankedMedalWidgetReady(Widget widget)
	{
		m_rankedMedalWidget = widget;
		m_rankedMedal = widget.GetComponentInChildren<RankedMedalWrapper>();
		UpdateRankedMedalWidget();
	}

	private void OnStarMultiplierWidgetReady(Widget widget)
	{
		m_starMultiplierWidget = widget;
		if (!m_starMultiplierWidget.GetDataModel(123, out var dataModel))
		{
			dataModel = new RankedPlayDataModel();
			m_starMultiplierWidget.BindDataModel(dataModel);
		}
		if (dataModel is RankedPlayDataModel rankedPlayDataModel)
		{
			rankedPlayDataModel.StarMultiplier = m_currentMedalInfo.starsPerWin;
		}
		StartCoroutine(FadeWidgetOut(m_starMultiplierWidget, 0f));
	}

	private void OnRankedCardBackProgressWidgetReady(Widget widget)
	{
		m_rankedCardBackProgressWidget = widget;
		UpdateRankedCardBackWidget();
		m_cardBackReminderDetails.Hide();
		m_rankedCardBackProgressWidget.Hide();
	}

	private void OnRankedIntroPopUpWidgetReady(Widget widget)
	{
		m_rankedIntroPopUpWidget = widget;
		widget.RegisterEventListener(RankedIntroPopUpEventListener);
	}

	private void RankedIntroPopUpEventListener(string eventName)
	{
		if (eventName.Equals("HIDE"))
		{
			iTween.ScaleTo(m_root, new Vector3(1f, 1f, 1f), 0.5f);
		}
	}

	private void OnRankedRewardChestWidgetReady(Widget widget)
	{
		m_rankedRewardChestWidget = widget;
		if (m_wasPrevSeasonLegacy)
		{
			m_rankedRewardChestWidget.gameObject.SetActive(value: false);
			return;
		}
		m_rankedRewardChestWidget.Hide();
		m_rankedRewardChestWidget.RegisterEventListener(RankedChestEventListener);
		m_rankedRewardChestWidget.BindDataModel(m_rankedChestDataModel);
	}

	private void RankedChestEventListener(string eventName)
	{
		if (eventName.Equals("CLICKED") && !m_chestOpened)
		{
			m_chestOpened = true;
			PlayMakerFSM componentInChildren = m_rankedRewardChestWidget.GetComponentInChildren<PlayMakerFSM>();
			FsmGameObject ownerObjectVar = componentInChildren.FsmVariables.GetFsmGameObject("OwnerObject");
			if (ownerObjectVar != null)
			{
				ownerObjectVar.Value = base.gameObject;
			}
			componentInChildren.SendEvent("StartAnim");
		}
	}

	private void UpdateRankedCardBackWidget()
	{
		if (!(m_rankedCardBackProgressWidget == null) && m_rewardProgress != null)
		{
			if (!m_rankedCardBackProgressWidget.GetDataModel(26, out var dataModel))
			{
				dataModel = new CardBackDataModel();
				m_rankedCardBackProgressWidget.BindDataModel(dataModel);
			}
			if (dataModel is CardBackDataModel cardBackDataModel)
			{
				cardBackDataModel.CardBackId = RankMgr.Get().GetRankedCardBackIdForSeasonId(m_rewardProgress.Season);
			}
			ProgressBar progressBar = m_rankedCardBackProgressWidget.GetComponentInChildren<ProgressBar>();
			if (progressBar != null)
			{
				int minWinsForCardback = RankMgr.Get().GetLocalPlayerMedalInfo().GetSeasonCardBackMinWins();
				progressBar.SetLabel(GameStrings.Format("GLOBAL_REWARD_PROGRESS", 0, minWinsForCardback));
				progressBar.SetProgressBar(0f);
			}
		}
	}

	private void UpdateRankedMedalWidget()
	{
		if (m_rankedMedal == null)
		{
			return;
		}
		if (m_showMedal)
		{
			m_rankedMedal.gameObject.SetActive(value: true);
			RankedPlayDataModel dataModel;
			if (m_currentMode == MODE.REDUCED_WELCOME)
			{
				m_rankedMedal.transform.position = m_boostedMedalBone.transform.position;
				dataModel = m_currentRankedDataModel;
			}
			else
			{
				dataModel = m_seasonBestRankedDataModel;
			}
			m_rankedMedal.BindRankedPlayDataModel(dataModel);
			m_rankedMedal.Show(m_wasPrevSeasonLegacy);
		}
		else
		{
			m_rankedMedal.gameObject.SetActive(value: false);
		}
	}

	private IEnumerator FadeWidgetIn(Widget widget, float time)
	{
		while (!widget.IsReady || widget.IsChangingStates)
		{
			yield return null;
		}
		iTween.FadeTo(widget.gameObject, 1f, time);
	}

	private IEnumerator FadeWidgetOut(Widget widget, float time)
	{
		while (!widget.IsReady || widget.IsChangingStates)
		{
			yield return null;
		}
		iTween.FadeTo(widget.gameObject, 0f, time);
	}

	private void OnNetCacheRewardProgressUpdated()
	{
		m_rewardProgress = NetCache.Get().GetNetObject<NetCache.NetCacheRewardProgress>();
		UpdateRankedCardBackWidget();
	}

	private int GetChestRewardTier()
	{
		int chestIndexMax = RankMgr.Get().GetMaxRewardChestVisualIndex();
		int chestIndex = m_seasonBestMedalInfo.RankConfig.RewardChestVisualIndex;
		return 1 + (chestIndexMax - chestIndex);
	}

	public string GetChestName()
	{
		int rewardTier = GetChestRewardTier();
		return GameStrings.Get($"GLOBAL_REWARD_CHEST_TIER{rewardTier}");
	}

	public string GetChestEarnedText()
	{
		int rewardTier = GetChestRewardTier();
		return GameStrings.Get($"GLOBAL_REWARD_CHEST_TIER{rewardTier}_EARNED");
	}

	protected override void DoShowAnimation()
	{
		m_showAnimState = ShowAnimState.IN_PROGRESS;
		AnimationUtil.ShowWithPunch(base.gameObject, START_SCALE, Vector3.Scale(PUNCH_SCALE, m_originalScale), m_originalScale, "OnShowAnimFinished", noFade: true);
	}
}
