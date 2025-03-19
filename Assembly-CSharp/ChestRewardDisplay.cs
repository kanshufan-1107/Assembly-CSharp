using System;
using System.Collections.Generic;
using UnityEngine;

public class ChestRewardDisplay : MonoBehaviour
{
	public const string DEFAULT_PREFAB = "RewardChest_Lock.prefab:06ffa33e82036694e8cacb96aa7b48e8";

	public const string MERCENARIES_PREFAB = "RewardChest_Mercenaries.prefab:7ba36254f98c8914e9b9931bbede3c88";

	public const string MERCENARIES_CONSOLATION_PREFAB = "LettuceConsolationPrize.prefab:8c837b1ecf3fe184eadfca1a3d661f6f";

	public const string MERCENARIES_AUTO_RETIRE_PREFAB = "LettuceAutorunPrize.prefab:05f50ccdbe9c5994e9dd5b2d19860822";

	public const string MERCENARY_FULLY_UPGRADED_PREFAB = "MercenariesMaxedOutReward.prefab:57fbf1dc798a43547b597a5d63e18271";

	public PegUIElement m_rewardChest;

	public PlayMakerFSM m_FSM;

	public Transform m_parent;

	public GameObject m_descText;

	public GameObject m_bannerObject;

	public UberText m_bannerUberText;

	public Transform m_rewardBoxBone;

	public Transform m_rewardBoxBonePackOpening;

	[SerializeField]
	private float m_showRewardChestDimAmount = 0.5f;

	private List<RewardData> m_rewards = new List<RewardData>();

	private List<RewardData> m_bonusRewards = new List<RewardData>();

	private List<RewardData> m_rewardsAfterBoxes = new List<RewardData>();

	private List<Reward> m_rewardsAfterBoxesObjects = new List<Reward>();

	private List<Action> m_doneCallbacks = new List<Action>();

	private bool m_fromNotice;

	private long m_noticeID = -1L;

	private int m_wins;

	private int m_leagueId;

	private ScreenEffectsHandle m_screenEffectsHandle;

	public bool ShowRewards_TavernBrawl(int wins, List<RewardData> rewards, Transform rewardBone, bool fromNotice = false, long noticeID = -1L)
	{
		if (rewards == null || rewards.Count < 1)
		{
			Debug.LogErrorFormat("rewards is null!");
			return false;
		}
		m_wins = wins;
		m_rewards = rewards;
		m_fromNotice = fromNotice;
		m_noticeID = noticeID;
		m_descText.SetActive(fromNotice);
		ShowRewardChest_TavernBrawl();
		return true;
	}

	public bool ShowRewards_LeaguePromotion(int leagueId, List<RewardData> rewards, Transform rewardBone, bool fromNotice = false, long noticeID = -1L)
	{
		if (rewards == null || rewards.Count < 1)
		{
			Debug.LogErrorFormat("rewards is null!");
			return false;
		}
		m_leagueId = leagueId;
		m_rewards = rewards;
		m_fromNotice = fromNotice;
		m_noticeID = noticeID;
		ShowRewardChest_LeaguePromotion();
		return true;
	}

	public bool ShowRewards_Quest(List<RewardData> rewards, Transform rewardBone, string title, string desc, bool fromNotice, int noticeId)
	{
		if (rewards == null || rewards.Count < 1)
		{
			Debug.LogErrorFormat("rewards is null!");
			return false;
		}
		m_rewards = rewards;
		m_fromNotice = fromNotice;
		m_noticeID = noticeId;
		m_bannerUberText.Text = title;
		m_descText.SetActive(value: true);
		m_descText.GetComponent<UberText>().Text = desc;
		ShowRewardChest();
		return true;
	}

	public bool ShowRewards_Mercenaries(List<RewardData> rewards, List<RewardData> bonusRewards, bool autoOpenChest, bool fromNotice, int noticeId)
	{
		m_rewards = rewards;
		m_bonusRewards = bonusRewards;
		return ShowRewards_MercenariesShared(autoOpenChest, fromNotice, noticeId);
	}

	private bool ShowRewards_MercenariesShared(bool autoOpenChest, bool fromNotice, int noticeId)
	{
		m_fromNotice = fromNotice;
		m_noticeID = noticeId;
		if (m_bannerObject != null)
		{
			UnityEngine.Object.Destroy(m_bannerObject);
		}
		m_descText?.SetActive(value: false);
		ShowRewardChest();
		if (autoOpenChest)
		{
			ShowRewardBags(null);
		}
		return true;
	}

	public void RegisterDoneCallback(Action action)
	{
		m_doneCallbacks.Add(action);
	}

	private void Awake()
	{
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	private void ShowRewardChest()
	{
		ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignetteDesaturatePerspective;
		screenEffectParameters.Type |= ScreenEffectType.BLENDTOCOLOR;
		screenEffectParameters.BlendToColor.BlendColor = Color.black;
		screenEffectParameters.BlendToColor.Amount = m_showRewardChestDimAmount;
		m_screenEffectsHandle.StartEffect(screenEffectParameters);
		m_FSM.SendEvent("SummonIn");
		LayerUtils.SetLayer(m_rewardChest.gameObject, GameLayer.IgnoreFullScreenEffects);
		m_rewardChest.AddEventListener(UIEventType.RELEASE, ShowRewardBags);
	}

	private void ShowRewardChest_TavernBrawl()
	{
		ShowRewardChest();
		string bannerText = ((m_wins != 0) ? GameStrings.Format("GLUE_BRAWLISEUM_REWARDS_WIN_BANNER_TEXT", m_wins, m_wins) : GameStrings.Get("GLUE_BRAWLISEUM_NO_WINS_REWARD_PACK_TEXT"));
		m_bannerUberText.Text = bannerText;
	}

	private void ShowRewardChest_LeaguePromotion()
	{
		ShowRewardChest();
		GameDbf.LeagueRank.GetRecord((LeagueRankDbfRecord r) => r.LeagueId == m_leagueId && r.StarLevel == 1);
		m_bannerUberText.Text = GameStrings.Get("GLUE_NEW_PLAYER_PROMOTION_CHEST_TITLE");
		m_descText.GetComponent<UberText>().Text = GameStrings.Get("GLUE_NEW_PLAYER_PROMOTION_CHEST_DESC");
	}

	private void OnRollover(UIEvent e)
	{
		m_FSM.SendEvent("Hover");
	}

	private void OnRollout(UIEvent e)
	{
		m_FSM.SendEvent("Idle");
	}

	private void ShowRewardBags(UIEvent e)
	{
		m_rewardChest.RemoveEventListener(UIEventType.RELEASE, ShowRewardBags);
		m_rewardChest.RemoveEventListener(UIEventType.ROLLOVER, OnRollover);
		m_rewardChest.RemoveEventListener(UIEventType.ROLLOUT, OnRollout);
		m_FSM.SendEvent("StartAnim");
	}

	private void OpenRewards()
	{
		if (m_rewards == null || m_rewards.Count == 0)
		{
			OnRewardBoxesDone();
			return;
		}
		PrefabCallback<GameObject> onAssetLoad = delegate(AssetReference assetRef, GameObject go, object callbackData)
		{
			if (SoundManager.Get() != null)
			{
				SoundManager.Get().LoadAndPlay("card_turn_over_legendary.prefab:a8140f686bff601459e954bc23de35e0");
			}
			RewardBoxesDisplay component = go.GetComponent<RewardBoxesDisplay>();
			component.SetRewards(m_rewards, m_bonusRewards);
			component.m_playBoxFlyoutSound = false;
			component.SetLayer(GameLayer.IgnoreFullScreenEffects);
			component.UseDarkeningClickCatcher(value: true);
			component.RegisterDoneCallback(OnRewardBoxesDone);
			if (!UniversalInputManager.UsePhoneUI)
			{
				LayerUtils.SetLayer(m_rewardChest.gameObject, GameLayer.Default);
			}
			Transform rewardBoxBoneForScene = GetRewardBoxBoneForScene();
			component.transform.position = rewardBoxBoneForScene.position;
			component.transform.localRotation = rewardBoxBoneForScene.localRotation;
			component.transform.localScale = rewardBoxBoneForScene.localScale;
			component.AnimateRewards();
		};
		AssetLoader.Get().InstantiatePrefab(RewardBoxesDisplay.GetPrefab(m_rewards), onAssetLoad);
	}

	private void OnRewardBoxesDone()
	{
		if (m_rewardsAfterBoxes.Count == 0)
		{
			OnAllChestRewardsDone();
		}
		else
		{
			DisplayRewardsAfterRewardBoxes();
		}
	}

	private void DisplayRewardsAfterRewardBoxes()
	{
		RewardUtils.LoadAndDisplayRewards(m_rewardsAfterBoxes, OnAllChestRewardsDone);
	}

	private void OnAllChestRewardsDone()
	{
		m_screenEffectsHandle.StopEffect(RewardUtils.MercRewardEndBlurTime);
		m_FSM.SendEvent("SummonOut");
		m_descText.SetActive(value: false);
		if (m_fromNotice)
		{
			Network.Get().AckNotice(m_noticeID);
		}
	}

	public void OnSummonInAnimationDone()
	{
		m_rewardChest.AddEventListener(UIEventType.ROLLOVER, OnRollover);
		m_rewardChest.AddEventListener(UIEventType.ROLLOUT, OnRollout);
	}

	public void OnSummonOutAnimationDone()
	{
		foreach (Action doneCallback2 in m_doneCallbacks)
		{
			doneCallback2?.Invoke();
		}
		UnityEngine.Object.Destroy(m_parent.gameObject);
	}

	private Transform GetRewardBoxBoneForScene()
	{
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.PACKOPENING)
		{
			return m_rewardBoxBonePackOpening;
		}
		return m_rewardBoxBone;
	}
}
