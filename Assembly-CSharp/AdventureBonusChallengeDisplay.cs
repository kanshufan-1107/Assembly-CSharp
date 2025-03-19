using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.MaterialService.Extensions;
using PegasusShared;
using UnityEngine;

[CustomEditClass]
public class AdventureBonusChallengeDisplay : MonoBehaviour
{
	[CustomEditField(Sections = "Buttons")]
	public PlayButton m_playButton;

	[CustomEditField(Sections = "Buttons")]
	public UIBButton m_backButton;

	[CustomEditField(Sections = "Buttons")]
	public PegUIElement m_rewardChest;

	[CustomEditField(Sections = "Text")]
	public UberText m_bonusChallengeLabel;

	[CustomEditField(Sections = "Text")]
	public UberText m_headerText;

	[CustomEditField(Sections = "Text")]
	public UberText m_footerText;

	[CustomEditField(Sections = "Rewards")]
	public GameObject m_rewardsPreview;

	[CustomEditField(Sections = "Rewards")]
	public GameObject m_rewardContainer;

	[CustomEditField(Sections = "Rewards")]
	public UberText m_rewardsText;

	[CustomEditField(Sections = "Rewards")]
	public Material m_chestOpenMaterial;

	[CustomEditField(Sections = "VO")]
	public float m_delayBeforeEntryVO;

	[CustomEditField(Sections = "VO")]
	public float m_delayBeforeCompleteVO;

	[CustomEditField(Sections = "Phone")]
	public PegUIElement m_rewardOffClickCatcher;

	private string m_headerString;

	private string m_footerString;

	private Vector3 m_rewardsScale;

	private GameObject m_rewardObject;

	private WingDbId m_wingId;

	private void Awake()
	{
		Navigation.PushUnique(OnNavigateBack);
		m_backButton.AddEventListener(UIEventType.RELEASE, OnBackButton);
		m_playButton.AddEventListener(UIEventType.RELEASE, OnPlayButton);
		ScenarioDbfRecord scenarioRecord = GameDbf.Scenario.GetRecord((ScenarioDbfRecord r) => r.AdventureId == (int)AdventureConfig.Get().GetSelectedAdventure() && r.ModeId == (int)AdventureConfig.Get().GetSelectedMode());
		if (scenarioRecord != null)
		{
			AdventureConfig.Get().SetMission((ScenarioDbId)scenarioRecord.ID);
			m_headerString = scenarioRecord.Name;
			m_footerString = (((bool)UniversalInputManager.UsePhoneUI && !string.IsNullOrEmpty(scenarioRecord.ShortDescription)) ? scenarioRecord.ShortDescription : scenarioRecord.Description);
			m_wingId = (WingDbId)scenarioRecord.WingId;
		}
		SetUpUberText();
		InitializeRewardDisplay();
		AdventureSubScene subscene = GetComponent<AdventureSubScene>();
		if (subscene != null)
		{
			subscene.AddSubSceneTransitionFinishedListener(OnSubSceneTransitionComplete);
			subscene.SetIsLoaded(loaded: true);
		}
	}

	private void SetUpUberText()
	{
		if (m_bonusChallengeLabel != null)
		{
			m_bonusChallengeLabel.Text = GameStrings.Get("GLUE_ADVENTURE_BONUS_CHALLENGE_LABEL");
		}
		if (m_headerText != null)
		{
			m_headerText.Text = m_headerString;
		}
		if (m_footerText != null)
		{
			m_footerText.Text = m_footerString;
		}
	}

	private void OnPlayButton(UIEvent e)
	{
		GameMgr.Get().FindGame(GameType.GT_VS_AI, FormatType.FT_WILD, (int)AdventureConfig.Get().GetMission(), 0, 0L, null, null, restoreSavedGameState: false, null, null, 0L);
	}

	private static bool OnNavigateBack()
	{
		AdventureConfig.Get().SubSceneGoBack();
		return true;
	}

	private void OnBackButton(UIEvent e)
	{
		Navigation.GoBack();
	}

	private void OnSubSceneTransitionComplete()
	{
	}

	private void InitializeRewardDisplay()
	{
		int scenarioDbId = (int)AdventureConfig.Get().GetMission();
		if (GetFirstRewardFromScenario(scenarioDbId) == null)
		{
			return;
		}
		if (SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.GAMEPLAY && AchieveManager.Get().GetNewCompletedAchievesToShow().Count > 0)
		{
			m_playButton.SetEnabled(enabled: false);
			LoadingScreen.Get().RegisterFinishedTransitionListener(OnTransitionFromGameplayFinished);
		}
		if (AdventureProgressMgr.Get().HasDefeatedScenario(scenarioDbId))
		{
			m_rewardChest.GetComponent<Renderer>().SetMaterial(m_chestOpenMaterial);
			m_rewardChest.SetEnabled(enabled: false);
			return;
		}
		StartCoroutine(PlayEntryQuoteWithTiming());
		AdventureDataDbfRecord record = GameUtils.GetAdventureDataRecord((int)AdventureConfig.Get().GetSelectedAdventure(), (int)AdventureConfig.Get().GetSelectedMode());
		if (record != null)
		{
			m_rewardsText.Text = record.RewardsDescription;
		}
		if (m_rewardOffClickCatcher != null)
		{
			m_rewardChest.AddEventListener(UIEventType.PRESS, ShowNonSessionRewardPreview);
			m_rewardOffClickCatcher.AddEventListener(UIEventType.PRESS, HideNonSessionRewardPreview);
		}
		else
		{
			m_rewardChest.AddEventListener(UIEventType.ROLLOVER, ShowNonSessionRewardPreview);
			m_rewardChest.AddEventListener(UIEventType.ROLLOUT, HideNonSessionRewardPreview);
		}
		m_rewardsScale = m_rewardsPreview.transform.localScale;
		m_rewardsPreview.transform.localScale = Vector3.one * 0.01f;
	}

	private void OnTransitionFromGameplayFinished(bool cutoff, object userData)
	{
		PopupDisplayManager.Get().ShowAnyOutstandingPopups(delegate
		{
			Navigation.GoBack();
		});
		int scenarioDbId = (int)AdventureConfig.Get().GetMission();
		if (AdventureProgressMgr.Get().HasDefeatedScenario(scenarioDbId))
		{
			StartCoroutine(PlayCompleteQuoteWithTiming());
		}
		LoadingScreen.Get().UnregisterFinishedTransitionListener(OnTransitionFromGameplayFinished);
	}

	private RewardData GetFirstRewardFromScenario(int scenarioDbId)
	{
		HashSet<Achieve.RewardTiming> rewardTimings = new HashSet<Achieve.RewardTiming> { Achieve.RewardTiming.ADVENTURE_CHEST };
		List<RewardData> rewards = AdventureProgressMgr.Get().GetRewardsForDefeatingScenario((int)AdventureConfig.Get().GetMission(), rewardTimings);
		if (rewards == null || rewards.Count == 0)
		{
			return null;
		}
		return rewards[0];
	}

	private void ShowNonSessionRewardPreview(UIEvent e)
	{
		if (AdventureConfig.Get().GetMission() == ScenarioDbId.INVALID)
		{
			return;
		}
		RewardData reward = GetFirstRewardFromScenario((int)AdventureConfig.Get().GetMission());
		if (reward == null)
		{
			return;
		}
		if (reward.RewardType == Reward.Type.CARD_BACK)
		{
			if (m_rewardObject == null)
			{
				int cardBackId = (reward as CardBackRewardData).CardBackID;
				CardBackManager.LoadCardBackData cardBackData = CardBackManager.Get().LoadCardBackByIndex(cardBackId, unlit: false, "Card_Hidden.prefab:1a94649d257bc284ca6e2962f634a8b9", shadowActive: true);
				if (cardBackData == null)
				{
					Debug.LogErrorFormat("AdventureBonusChallengeDisplay.ShowReward() - Could not load cardback ID {0}!", cardBackId);
					return;
				}
				m_rewardObject = cardBackData.m_GameObject;
				GameUtils.SetParent(m_rewardObject, m_rewardContainer);
			}
			m_rewardsPreview.SetActive(value: true);
			iTween.Stop(m_rewardsPreview);
			iTween.ScaleTo(m_rewardsPreview, iTween.Hash("scale", m_rewardsScale, "time", 0.15f));
		}
		else
		{
			Debug.LogErrorFormat("Adventure Bonus Challenge reward type currently not supported! Add type {0} to AdventureBonusChallengeDisplay.ShowReward().", reward.RewardType);
		}
	}

	private void HideNonSessionRewardPreview(UIEvent e)
	{
		iTween.Stop(m_rewardsPreview);
		iTween.ScaleTo(m_rewardsPreview, iTween.Hash("scale", Vector3.one * 0.01f, "time", 0.15f, "oncomplete", (Action<object>)delegate
		{
			m_rewardsPreview.SetActive(value: false);
		}));
	}

	private IEnumerator PlayEntryQuoteWithTiming()
	{
		yield return new WaitForSeconds(m_delayBeforeEntryVO);
		AdventureWingDef wingDef = AdventureScene.Get().GetWingDef(m_wingId);
		if (AdventureUtils.CanPlayWingOpenQuote(wingDef))
		{
			string quoteString = new AssetReference(wingDef.m_OpenQuoteVOLine).GetLegacyAssetName();
			NotificationManager.Get().CreateCharacterQuote(wingDef.m_OpenQuotePrefab, NotificationManager.CHARACTER_POS_ABOVE_QUEST_TOAST, GameStrings.Get(quoteString), wingDef.m_OpenQuoteVOLine, allowRepeatDuringSession: false);
		}
	}

	private IEnumerator PlayCompleteQuoteWithTiming()
	{
		yield return new WaitForSeconds(m_delayBeforeCompleteVO);
		AdventureWingDef wingDef = AdventureScene.Get().GetWingDef(m_wingId);
		if (AdventureUtils.CanPlayWingCompleteQuote(wingDef))
		{
			string quoteString = new AssetReference(wingDef.m_CompleteQuoteVOLine).GetLegacyAssetName();
			NotificationManager.Get().CreateCharacterQuote(wingDef.m_CompleteQuotePrefab, NotificationManager.CHARACTER_POS_ABOVE_QUEST_TOAST, GameStrings.Get(quoteString), wingDef.m_CompleteQuoteVOLine, allowRepeatDuringSession: false);
		}
	}
}
