using System;
using System.Collections.Generic;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

[CustomEditClass]
public class QuestTile : MonoBehaviour
{
	public enum FsmEvent
	{
		None,
		Birth,
		Death,
		QuestGranted,
		QuestRerolled,
		QuestShownInQuestAlert,
		QuestShownInQuestLog,
		QuestHidden
	}

	[Serializable]
	public class SpecialEventFxEntry
	{
		public EventTimingType m_questActivatedBySpecialEventType;

		[CustomEditField(T = EditType.GAME_OBJECT)]
		public string m_fxPrefab;
	}

	public UberText m_requirement;

	public UberText m_questName;

	public GameObject m_nameLine;

	public GameObject m_progress;

	public UberText m_progressText;

	public NormalButton m_cancelButton;

	public GameObject m_cancelButtonRoot;

	public PlayMakerFSM m_fsmForAutoDestroyQuest;

	public GameObject m_legendaryFX;

	public MeshRenderer m_tileRenderer;

	public Material m_tileNormalMaterial;

	public Material m_tileLegendaryMaterial;

	public GameObject m_rewardIconZone;

	public GameObject m_questTileRewardIconPrefab;

	[CustomEditField(Sections = "Reward Icons")]
	public bool m_rewardIconShrinkToFitEnabled;

	[CustomEditField(Sections = "Reward Icons")]
	public float m_rewardIconPadding;

	[CustomEditField(Sections = "Reward Icons")]
	public float m_rewardIconPaddingPacksOnly = -0.25f;

	[CustomEditField(Sections = "Reward Icons")]
	public float m_rewardIconScaleReductionForEachAdditional;

	[CustomEditField(Sections = "Special Event FX", T = EditType.GAME_OBJECT)]
	public string m_fxPrefabDefault;

	[CustomEditField(Sections = "Special Event FX")]
	public List<SpecialEventFxEntry> m_specialEventFx = new List<SpecialEventFxEntry>();

	private Achievement m_quest;

	private bool m_canShowCancelButton;

	private List<QuestTileRewardIcon> m_rewardIcons = new List<QuestTileRewardIcon>();

	private List<RewardData> m_rewards = new List<RewardData>();

	private PlayMakerFSM m_fsm;

	private bool m_fsmHasBeenSentTerminalEvent;

	private bool m_fsmHasDeathFxFinishedPlaying;

	private bool m_fsmHasPendingQuestRerolledEvent;

	private const float NAME_LINE_PADDING = 0.22f;

	private void Awake()
	{
		SetCanShowCancelButton(canShowCancel: false);
		m_cancelButton.AddEventListener(UIEventType.RELEASE, OnCancelButtonReleased);
	}

	public Achievement GetQuest()
	{
		return m_quest;
	}

	public void SetupTile(Achievement quest, FsmEvent fsmEventToPlay = FsmEvent.None)
	{
		quest.AckCurrentProgressAndRewardNotices(ackIntermediateProgress: true);
		m_quest = quest;
		m_rewards = m_quest.Rewards;
		if (m_quest.MaxProgress > 1)
		{
			m_progressText.Text = m_quest.Progress + "/" + m_quest.MaxProgress;
			m_progress.SetActive(value: true);
		}
		else
		{
			m_progressText.Text = "";
			m_progress.SetActive(value: false);
		}
		if (quest.IsLegendary)
		{
			m_tileRenderer.SetMaterial(m_tileLegendaryMaterial);
			m_legendaryFX.SetActive(value: true);
		}
		else
		{
			m_tileRenderer.SetMaterial(m_tileNormalMaterial);
			m_legendaryFX.SetActive(value: false);
		}
		m_questName.Text = quest.Name;
		RewardUtils.SetQuestTileNameLinePosition(m_nameLine, m_questName, 0.22f);
		m_requirement.Text = quest.Description;
		SetupRewardIcons();
		SetVisible(visible: false);
		LoadFsmAndPlayFX(fsmEventToPlay);
	}

	public void OnDeathFinishedPlaying()
	{
		m_fsmHasDeathFxFinishedPlaying = true;
		if (m_fsmHasPendingQuestRerolledEvent)
		{
			m_fsmHasPendingQuestRerolledEvent = false;
			SendFsmEvent(FsmEvent.QuestRerolled);
		}
	}

	public void SetCanShowCancelButton(bool canShowCancel)
	{
		m_canShowCancelButton = canShowCancel;
		UpdateCancelButtonVisibility();
	}

	public void UpdateCancelButtonVisibility()
	{
		bool showCancelButton = false;
		if (m_canShowCancelButton && m_quest != null)
		{
			showCancelButton = AchieveManager.Get().CanCancelQuest(m_quest.ID);
		}
		m_cancelButtonRoot.gameObject.SetActive(showCancelButton);
	}

	public int GetQuestID()
	{
		if (m_quest == null)
		{
			return 0;
		}
		return m_quest.ID;
	}

	public void OnClose()
	{
		foreach (QuestTileRewardIcon rewardIcon in m_rewardIcons)
		{
			rewardIcon.OnClose();
		}
		SendFsmEvent(FsmEvent.QuestHidden);
	}

	public void CompleteAndAutoDestroyQuest()
	{
		if (m_quest != null && m_quest.AutoDestroy && !(m_fsmForAutoDestroyQuest == null))
		{
			m_fsmForAutoDestroyQuest.SendEvent("Death");
			AchieveManager.Get().CompleteAutoDestroyAchieve(m_quest.ID);
		}
	}

	private void ReplaceAutoDestroyQuest()
	{
		if (m_quest != null && m_quest.AutoDestroy && !(m_fsmForAutoDestroyQuest == null))
		{
			int nextAchieveInChain = m_quest.LinkToId;
			if (nextAchieveInChain != 0)
			{
				OnClose();
				Achievement nextAchievement = AchieveManager.Get().GetAchievement(nextAchieveInChain);
				SetupTile(nextAchievement);
				m_fsmForAutoDestroyQuest.SendEvent("Birth");
			}
		}
	}

	private void SetVisible(bool visible)
	{
		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		if (renderers != null)
		{
			Renderer[] array = renderers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = visible;
			}
		}
		UberText[] uberTexts = GetComponentsInChildren<UberText>();
		if (uberTexts == null)
		{
			return;
		}
		UberText[] array2 = uberTexts;
		foreach (UberText text in array2)
		{
			if (visible)
			{
				text.Show();
			}
			else
			{
				text.Hide();
			}
		}
	}

	private void OnCancelButtonReleased(UIEvent e)
	{
		if (!Network.IsLoggedIn())
		{
			DialogManager.Get().ShowReconnectHelperDialog(delegate
			{
				OnCancelButtonReleased(e);
			});
		}
		else if (m_quest.IsLegendary)
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_headerText = GameStrings.Get("GLUE_LEGENDARY_QUEST_REROLL_TITLE");
			info.m_text = GameStrings.Get("GLUE_LEGENDARY_QUEST_REROLL_BODY");
			info.m_confirmText = GameStrings.Get("GLOBAL_BUTTON_YES");
			info.m_cancelText = GameStrings.Get("GLOBAL_BUTTON_NO");
			info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
			info.m_responseCallback = OnQuestRerolled;
			DialogManager.Get().ShowPopup(info);
		}
		else
		{
			OnQuestRerolled(AlertPopup.Response.CONFIRM, null);
		}
	}

	private void OnQuestRerolled(AlertPopup.Response response, object userData)
	{
		if (response != AlertPopup.Response.CONFIRM || m_quest == null)
		{
			return;
		}
		AchieveManager.Get().CancelQuest(m_quest.ID);
		foreach (QuestTileRewardIcon rewardIcon in m_rewardIcons)
		{
			rewardIcon.OnQuestRerolled();
		}
		SendFsmEvent(FsmEvent.Death);
	}

	private void SendFsmEvent(FsmEvent fsmEvent)
	{
		if (fsmEvent == FsmEvent.None || !(m_fsm != null))
		{
			return;
		}
		m_fsm.SendEvent(fsmEvent.ToString());
		if (fsmEvent == FsmEvent.QuestHidden || fsmEvent == FsmEvent.Death)
		{
			m_fsmHasBeenSentTerminalEvent = true;
			if (fsmEvent == FsmEvent.Death)
			{
				m_fsmHasDeathFxFinishedPlaying = false;
			}
		}
	}

	[ContextMenu("Reset Quest Seen")]
	private void ResetQuestSeen()
	{
		AchieveManager.Get().ResetQuestSeenByPlayerThisSession(m_quest);
	}

	private void LoadFsmAndPlayFX(FsmEvent fsmEventToPlay)
	{
		string fxPrefabPath = m_fxPrefabDefault;
		AchieveRegionDataDbfRecord regionData = m_quest.GetCurrentRegionData();
		if (regionData != null && regionData.ActivateEvent != EventTimingType.UNKNOWN)
		{
			foreach (SpecialEventFxEntry entry in m_specialEventFx)
			{
				if (!string.IsNullOrEmpty(entry.m_fxPrefab) && entry.m_questActivatedBySpecialEventType != EventTimingType.UNKNOWN && entry.m_questActivatedBySpecialEventType == regionData.ActivateEvent)
				{
					fxPrefabPath = entry.m_fxPrefab;
					break;
				}
			}
		}
		AssetLoader.Get().InstantiatePrefab(fxPrefabPath, OnFxPrefabLoaded, fsmEventToPlay, AssetLoadingOptions.IgnorePrefabPosition);
	}

	private void OnFxPrefabLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			return;
		}
		if (base.gameObject == null)
		{
			UnityEngine.Object.Destroy(go);
			return;
		}
		GameUtils.SetParent(go, base.gameObject);
		LayerUtils.SetLayer(go, base.gameObject.layer, null);
		if (m_fsm != null && !m_fsmHasBeenSentTerminalEvent)
		{
			Debug.LogWarning("QuestTile FSM OnFxPrefabLoaded, but existing FSM has not been sent death event!");
			return;
		}
		m_fsmHasBeenSentTerminalEvent = false;
		m_fsm = go.GetComponent<PlayMakerFSM>();
		if (m_fsm == null)
		{
			return;
		}
		SendFsmEvent(FsmEvent.Birth);
		bool isFirstTimeShown = AchieveManager.Get().MarkQuestAsSeenByPlayerThisSession(m_quest);
		m_fsm.FsmVariables.GetFsmBool("IsFirstTimeShown").Value = isFirstTimeShown;
		if (EnumUtils.TryCast<FsmEvent>(callbackData, out var fsmEventToPlay))
		{
			if (fsmEventToPlay == FsmEvent.QuestRerolled && !m_fsmHasDeathFxFinishedPlaying)
			{
				m_fsmHasPendingQuestRerolledEvent = true;
			}
			else
			{
				SendFsmEvent(fsmEventToPlay);
			}
		}
	}

	private void SetupRewardIcons()
	{
		foreach (QuestTileRewardIcon rewardIcon2 in m_rewardIcons)
		{
			UnityEngine.Object.Destroy(rewardIcon2.gameObject);
		}
		m_rewardIcons.Clear();
		RewardChestContentsDbfRecord chestContents = null;
		if (m_quest.DbfRecord.Reward == "generic_reward_chest")
		{
			int rewardChestAssetId = (int)m_quest.DbfRecord.RewardData1;
			int rewardChestLevel = (int)m_quest.DbfRecord.RewardData2;
			chestContents = RewardUtils.GetRewardChestContents(rewardChestAssetId, rewardChestLevel);
			m_rewards = RewardUtils.GetRewardDataFromRewardChestAsset(rewardChestAssetId, rewardChestLevel);
		}
		if (chestContents != null && !string.IsNullOrEmpty(chestContents.IconTexture))
		{
			GameObject obj = UnityEngine.Object.Instantiate(m_questTileRewardIconPrefab, m_rewardIconZone.transform);
			LayerUtils.SetLayer(obj, m_rewardIconZone.gameObject.layer, null);
			QuestTileRewardIcon rewardIcon = obj.GetComponent<QuestTileRewardIcon>();
			AssetReference iconTextureAssetRef = new AssetReference(chestContents.IconTexture);
			Vector2 iconTextureSourceOffset = new Vector2((float)chestContents.IconOffsetX, (float)chestContents.IconOffsetY);
			int renderQueue = 3000;
			rewardIcon.InitWithIconParams(renderQueue, iconTextureAssetRef, iconTextureSourceOffset, null);
			m_rewardIcons.Add(rewardIcon);
		}
		else
		{
			UnravelPackStacks();
			CreateRewardIconsPerReward();
		}
	}

	private void UnravelPackStacks()
	{
		bool unraveled = false;
		bool allPacks = true;
		List<RewardData> unraveledRewards = new List<RewardData>();
		for (int i = 0; i < m_rewards.Count; i++)
		{
			if (m_rewards[i].RewardType == Reward.Type.BOOSTER_PACK)
			{
				unraveled = true;
				BoosterPackRewardData boosterReward = m_rewards[i] as BoosterPackRewardData;
				for (int j = 0; j < boosterReward.Count; j++)
				{
					unraveledRewards.Add(m_rewards[i]);
				}
			}
			else
			{
				allPacks = false;
			}
		}
		if (!allPacks)
		{
			Log.Achievements.PrintWarning("Attempted to display a mixture of packs and other rewards without using a specific Reward Chest icon.");
		}
		if (unraveled && allPacks)
		{
			m_rewards = unraveledRewards;
			m_rewardIconPadding = m_rewardIconPaddingPacksOnly;
			m_rewardIconScaleReductionForEachAdditional = 0f;
			m_rewardIconShrinkToFitEnabled = false;
		}
	}

	private void CreateRewardIconsPerReward()
	{
		bool isDoubleGoldEnabled = m_quest.IsAffectedByDoubleGold && EventTimingManager.Get().IsEventActive(EventTimingType.SPECIAL_EVENT_GOLD_DOUBLED);
		float columnWidth = m_rewardIconZone.GetComponent<BoxCollider>().size.x / (float)m_rewards.Count;
		int renderQueue = 3000 + m_rewards.Count - 1;
		float distanceFromCenter = 0f;
		float offsetDirection = (GeneralUtils.IsEven(m_rewards.Count) ? (-1f) : 1f);
		for (int rewardIndex = 0; rewardIndex < m_rewards.Count; rewardIndex++)
		{
			RewardData rewardData = m_rewards[rewardIndex];
			GameObject go = UnityEngine.Object.Instantiate(m_questTileRewardIconPrefab, m_rewardIconZone.transform);
			LayerUtils.SetLayer(go, m_rewardIconZone.gameObject.layer, null);
			QuestTileRewardIcon rewardIcon = go.GetComponent<QuestTileRewardIcon>();
			rewardIcon.InitWithRewardData(rewardData, isDoubleGoldEnabled, renderQueue);
			m_rewardIcons.Add(rewardIcon);
			if ((GeneralUtils.IsOdd(m_rewards.Count) && GeneralUtils.IsOdd(rewardIndex)) || (GeneralUtils.IsEven(m_rewards.Count) && GeneralUtils.IsEven(rewardIndex)))
			{
				distanceFromCenter += columnWidth / 2f;
				distanceFromCenter += m_rewardIconPadding;
			}
			renderQueue--;
			offsetDirection *= -1f;
			float offsetX = distanceFromCenter * offsetDirection;
			go.transform.localPosition = new Vector3(offsetX, 0f, 0f);
			if (rewardIndex == 0 && m_rewards.Count > 1 && GeneralUtils.IsOdd(m_rewards.Count))
			{
				distanceFromCenter += columnWidth / 2f;
			}
			if (m_rewardIconShrinkToFitEnabled)
			{
				float iconWidth = go.GetComponent<MeshFilter>().mesh.bounds.size.x;
				float iconScaleFactor = Math.Min(1f, columnWidth / iconWidth);
				go.transform.localScale *= iconScaleFactor;
			}
			if (m_rewardIconScaleReductionForEachAdditional > 0f)
			{
				float iconScaleFactor2 = 1f - (float)Math.Max(0, m_rewards.Count - 1) * m_rewardIconScaleReductionForEachAdditional;
				go.transform.localScale *= iconScaleFactor2;
			}
		}
	}
}
