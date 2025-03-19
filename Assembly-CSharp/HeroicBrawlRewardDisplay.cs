using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroicBrawlRewardDisplay : MonoBehaviour
{
	[Serializable]
	public class RewardVisuals
	{
		public bool DropBox;

		public bool ShatterDialog;

		public int DustPerBottleMin = 50;

		public int DustPerBottleMax = 100;

		public int GoldPerBagMin = 50;

		public int GoldPerBagMax = 100;

		public Transform m_FinalPacksBone;

		public Transform m_FinalGoldBone;

		public Transform m_FinalDustBone;
	}

	[Serializable]
	public struct FireworkRewardZone
	{
		public GameObject ZoneRoot;

		public BoxCollider Collider;

		public GameObject PackGameObject;

		public GameObject GoldGameObject;

		public GameObject DustGameObject;

		public BoosterPackReward packReward;

		public ArcaneDustReward dustReward;

		public GoldReward goldReward;
	}

	[Serializable]
	public struct CardVisuals
	{
		public GameObject[] m_Cards;

		public GameObject[] m_CardTargets;
	}

	public struct RewardCardReceived
	{
		public string CardID;

		public TAG_PREMIUM Premium;

		public EntityDef CardEntityDef;

		public GameObject CardGameObject;
	}

	public struct RewardsReceivedData
	{
		public int PackID;

		public int PackCount;

		public int GoldCount;

		public int DustCount;

		public int CardsCount;

		public List<RewardCardReceived> Cards;
	}

	public GameObject m_Root;

	public float m_FirewarksRewardDelayMin = 0.45f;

	public float m_FirewarksRewardDelayMax = 0.75f;

	public float m_FirewarksRewardHold = 0.7f;

	public float m_FireworksRewardRanRot = 30f;

	public float m_CardRewardDelay = 0.5f;

	public float m_CardRewardBurstDelay = 0.2f;

	public float m_EndScaleAwayTime = 0.3f;

	public float m_CardAnimationTime = 0.5f;

	public PlayMakerFSM m_PackFireworkFSM;

	public PlayMakerFSM m_GoldFireworkFSM;

	public PlayMakerFSM m_DustFireworkFSM;

	public PlayMakerFSM m_CardFireworkFSM;

	public NormalButton m_DoneButton;

	public PlayMakerFSM m_FSM;

	public GameObject m_RewardFireworksRoot;

	public FireworkRewardZone[] m_RewardZones;

	public GameObject m_FinalRewardsRoot;

	public UberText m_BannerUberText;

	public RewardVisuals[] m_RewardVisuals = new RewardVisuals[12];

	public GameObject m_CardsRoot;

	public CardVisuals[] m_CardVisuals = new CardVisuals[3];

	public int m_DebugWins;

	public const string DEFAULT_PREFAB = "HeroicBrawlReward.prefab:8f49f1fcb5ca4485d9b6b22993e1b1ab";

	public PegUIElement m_HeroicRewardChest;

	public GameObject m_DescText;

	private List<RewardData> m_Rewards = new List<RewardData>();

	private RewardsReceivedData m_RewardsReceived;

	private List<Reward> m_finalRewards = new List<Reward>();

	private int m_finalRewardsLoadedCount;

	private int m_lastZone = 1;

	private List<Action> m_doneCallbacks;

	private int m_wins;

	private bool m_fromNotice;

	private long m_noticeID = -1L;

	private static HeroicBrawlRewardDisplay s_instance;

	private ScreenEffectsHandle m_screenEffectsHandle;

	private void Awake()
	{
		s_instance = this;
		m_doneCallbacks = new List<Action>();
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	private void Start()
	{
		Init();
	}

	private void OnDestroy()
	{
		s_instance = null;
	}

	public static HeroicBrawlRewardDisplay Get()
	{
		return s_instance;
	}

	public void ShowRewards(int wins, List<RewardData> rewards, bool fromNotice = false, long noticeID = -1L)
	{
		if (m_FSM == null)
		{
			Debug.LogErrorFormat("FSM is null!");
			return;
		}
		if (rewards == null && rewards.Count < 1)
		{
			Debug.LogErrorFormat("rewards is null!");
			return;
		}
		m_Rewards = rewards;
		m_wins = wins;
		m_fromNotice = fromNotice;
		m_noticeID = noticeID;
		m_DescText.SetActive(fromNotice);
		ShowRewardChest();
	}

	[ContextMenu("Debug Show Rewards")]
	public void DebugShowRewards()
	{
		List<RewardData> rewards = DebugRewards(m_DebugWins);
		ShowRewards(m_DebugWins, rewards, fromNotice: false, -1L);
	}

	public void RegisterDoneCallback(Action action)
	{
		m_doneCallbacks.Add(action);
	}

	private void Init()
	{
		for (int i = 0; i < m_RewardZones.Length; i++)
		{
			m_RewardZones[i].goldReward = m_RewardZones[i].GoldGameObject.GetComponentInChildren<GoldReward>();
			m_RewardZones[i].dustReward = m_RewardZones[i].DustGameObject.GetComponentInChildren<ArcaneDustReward>();
		}
		m_FinalRewardsRoot.SetActive(value: false);
		m_RewardFireworksRoot.SetActive(value: false);
		m_PackFireworkFSM.gameObject.SetActive(value: false);
		m_GoldFireworkFSM.gameObject.SetActive(value: false);
		m_DustFireworkFSM.gameObject.SetActive(value: false);
		m_CardFireworkFSM.gameObject.SetActive(value: false);
	}

	private void ShowRewardChest()
	{
		m_screenEffectsHandle.StartEffect(ScreenEffectParameters.BlurVignetteDesaturatePerspective);
		LoadFinalRewards();
		m_HeroicRewardChest.AddEventListener(UIEventType.RELEASE, ShowRewardsCeremony);
	}

	private void ShowRewardsCeremony(UIEvent e)
	{
		StartCoroutine(AnimateRewardsCeremony());
	}

	private IEnumerator AnimateRewardsCeremony()
	{
		m_HeroicRewardChest.RemoveEventListener(UIEventType.RELEASE, ShowRewardsCeremony);
		RewardVisuals visuals = m_RewardVisuals[m_wins];
		m_DescText.SetActive(value: false);
		if (visuals.DropBox)
		{
			m_FSM.FsmVariables.GetFsmBool("ShatterDialog").Value = visuals.ShatterDialog;
			m_FSM.SendEvent("DropBox");
		}
		else
		{
			m_FSM.SendEvent("OpenBoxOnly");
		}
		while (!m_FSM.FsmVariables.GetFsmBool("isChestAnimationDone").Value)
		{
			yield return null;
		}
		if (visuals.DropBox)
		{
			StartCoroutine(ShowRewardsFireworks(m_wins));
		}
		else
		{
			StartCoroutine(ShowRewardsSimple(m_wins));
		}
	}

	private IEnumerator ShowRewardsSimple(int wins)
	{
		yield return null;
		StartCoroutine(ShowFinalRewards(wins, simpleRewards: true));
	}

	private IEnumerator ShowRewardsFireworks(int wins)
	{
		RewardVisuals rewardvisuals = m_RewardVisuals[wins];
		m_RewardFireworksRoot.SetActive(value: true);
		LoadBoosterReward();
		InitRewardsReceived();
		int remainingPacks = m_RewardsReceived.PackCount;
		int remainingGold = m_RewardsReceived.GoldCount;
		int remainingDust = m_RewardsReceived.DustCount;
		int lastType = 0;
		while (remainingPacks > 0 || remainingGold > 0 || remainingDust > 0)
		{
			int zone = NextRewardZone();
			int rndReward = UnityEngine.Random.Range(0, 4);
			if (rndReward == lastType)
			{
				switch (lastType)
				{
				case 0:
				case 1:
					rndReward = UnityEngine.Random.Range(2, 4);
					break;
				case 2:
					rndReward = UnityEngine.Random.Range(0, 3);
					if (rndReward == 2)
					{
						rndReward = 3;
					}
					break;
				default:
					rndReward = UnityEngine.Random.Range(0, 3);
					break;
				}
			}
			if ((rndReward == 0 || rndReward == 1) && remainingPacks <= 0)
			{
				rndReward = 2;
			}
			if (rndReward == 2 && remainingGold <= 0)
			{
				rndReward = 3;
			}
			if (rndReward == 3 && remainingDust <= 0)
			{
				rndReward = 0;
			}
			if ((rndReward == 0 || rndReward == 1) && remainingPacks <= 0)
			{
				rndReward = 2;
			}
			if (rndReward == 2 && remainingGold <= 0)
			{
				rndReward = 3;
			}
			Vector3 rewardLocalPosition = ZoneRandomLocalPosition(zone);
			if ((rndReward == 0 || rndReward == 1) && remainingPacks > 0)
			{
				lastType = rndReward;
				remainingPacks--;
				StartCoroutine(DisplayFireworkPack(zone, rewardLocalPosition));
				yield return new WaitForSeconds(GetFireworkRewardDelay());
				continue;
			}
			if (rndReward == 2 && remainingGold > 0)
			{
				lastType = rndReward;
				int goldPer = UnityEngine.Random.Range(rewardvisuals.GoldPerBagMin, rewardvisuals.GoldPerBagMax);
				int displayAmount = m_RewardsReceived.GoldCount - goldPer;
				if (displayAmount > goldPer)
				{
					displayAmount = goldPer;
				}
				remainingGold -= displayAmount;
				StartCoroutine(DisplayFireworkGold(zone, rewardLocalPosition, displayAmount));
				yield return new WaitForSeconds(GetFireworkRewardDelay());
				continue;
			}
			if (rndReward == 3 && remainingDust > 0)
			{
				lastType = rndReward;
				int dustPer = UnityEngine.Random.Range(rewardvisuals.DustPerBottleMin, rewardvisuals.DustPerBottleMax);
				int displayAmount2 = m_RewardsReceived.DustCount - dustPer;
				if (displayAmount2 > dustPer)
				{
					displayAmount2 = dustPer;
				}
				remainingDust -= displayAmount2;
				StartCoroutine(DisplayFireworkDust(zone, rewardLocalPosition, displayAmount2));
				yield return new WaitForSeconds(GetFireworkRewardDelay());
				continue;
			}
			Debug.LogWarningFormat("No reward found: Packs: {0}, Gold: {1}, Dust: {2}", remainingPacks, remainingGold, remainingDust);
			break;
		}
		yield return new WaitForSeconds(1f);
		if (m_RewardsReceived.CardsCount > 0)
		{
			StartCoroutine(ShowCards(wins));
			yield return new WaitForSeconds((m_CardRewardDelay + m_CardRewardBurstDelay) * (float)m_RewardsReceived.CardsCount);
		}
		else
		{
			StartCoroutine(ShowFinalRewards(wins));
		}
	}

	private float GetFireworkRewardDelay()
	{
		return UnityEngine.Random.Range(m_FirewarksRewardDelayMin, m_FirewarksRewardDelayMax);
	}

	private float DisplayFirework(PlayMakerFSM fsm, Vector3 targetPosition)
	{
		GameObject obj = UnityEngine.Object.Instantiate(fsm.gameObject);
		obj.transform.parent = base.transform;
		obj.transform.position = fsm.transform.position;
		obj.SetActive(value: true);
		obj.gameObject.layer = fsm.gameObject.layer;
		PlayMakerFSM component = obj.GetComponent<PlayMakerFSM>();
		component.FsmVariables.FindFsmVector3("TargetPosition").Value = targetPosition;
		component.SendEvent("Firework");
		m_FSM.SendEvent("BounceBox");
		return component.FsmVariables.GetFsmFloat("FireworkTime").Value;
	}

	private IEnumerator DisplayFireworkPack(int zone, Vector3 localPosition)
	{
		Vector3 targetPosition = m_RewardZones[zone].ZoneRoot.transform.TransformPoint(localPosition);
		float fireworkWaitTime = DisplayFirework(m_PackFireworkFSM, targetPosition);
		yield return new WaitForSeconds(fireworkWaitTime);
		GameObject packGO = UnityEngine.Object.Instantiate(m_RewardZones[zone].packReward.gameObject);
		packGO.layer = m_RewardZones[zone].PackGameObject.layer;
		packGO.transform.parent = m_RewardZones[zone].ZoneRoot.transform;
		packGO.transform.localPosition = localPosition;
		float randRot = UnityEngine.Random.Range(0f - m_FireworksRewardRanRot, m_FireworksRewardRanRot);
		packGO.transform.localEulerAngles = new Vector3(0f, randRot, 0f);
		packGO.transform.localScale = m_RewardZones[zone].PackGameObject.transform.localScale;
		BoosterPackReward packReward = packGO.GetComponent<BoosterPackReward>();
		BoosterPackRewardData packData = new BoosterPackRewardData();
		packData.Count = 1;
		packData.Id = m_RewardsReceived.PackID;
		packReward.SetData(packData, updateVisuals: true);
		packReward.m_RotateIn = false;
		packReward.m_showBanner = false;
		packReward.m_playSounds = false;
		yield return null;
		packReward.Show(updateCacheValues: true);
		yield return new WaitForSeconds(m_FirewarksRewardHold);
		packReward.HideWithFX();
		m_RewardZones[zone].packReward.Hide(animate: true);
		yield return new WaitForSeconds(3f);
		if (packGO != null)
		{
			UnityEngine.Object.Destroy(packGO);
		}
	}

	private IEnumerator DisplayFireworkGold(int zone, Vector3 localPosition, int amount)
	{
		Vector3 targetPosition = m_RewardZones[zone].ZoneRoot.transform.TransformPoint(localPosition);
		float fireworkWaitTime = DisplayFirework(m_GoldFireworkFSM, targetPosition);
		yield return new WaitForSeconds(fireworkWaitTime);
		GoldRewardData data = new GoldRewardData
		{
			Amount = amount
		};
		GameObject goldGO = UnityEngine.Object.Instantiate(m_RewardZones[zone].goldReward.gameObject);
		goldGO.layer = m_RewardZones[zone].GoldGameObject.layer;
		goldGO.transform.parent = m_RewardZones[zone].ZoneRoot.transform;
		goldGO.transform.localPosition = localPosition;
		float randRot = UnityEngine.Random.Range(0f - m_FireworksRewardRanRot, m_FireworksRewardRanRot);
		goldGO.transform.localEulerAngles = new Vector3(0f, randRot, 0f);
		goldGO.transform.localScale = m_RewardZones[zone].GoldGameObject.transform.localScale;
		GoldReward goldReward = goldGO.GetComponent<GoldReward>();
		yield return null;
		goldReward.SetData(data, updateVisuals: true);
		goldReward.m_RotateIn = false;
		goldReward.m_showBanner = false;
		goldReward.m_playSounds = false;
		goldReward.Show(updateCacheValues: true);
		yield return new WaitForSeconds(m_FirewarksRewardHold);
		goldReward.HideWithFX();
		yield return new WaitForSeconds(3f);
		if (goldGO != null)
		{
			UnityEngine.Object.Destroy(goldGO);
		}
	}

	private IEnumerator DisplayFireworkDust(int zone, Vector3 localPosition, int amount)
	{
		Vector3 targetPosition = m_RewardZones[zone].ZoneRoot.transform.TransformPoint(localPosition);
		float fireworkWaitTime = DisplayFirework(m_DustFireworkFSM, targetPosition);
		yield return new WaitForSeconds(fireworkWaitTime);
		ArcaneDustRewardData data = new ArcaneDustRewardData();
		data.Amount = amount;
		data.MarkAsDummyReward();
		GameObject dustGO = UnityEngine.Object.Instantiate(m_RewardZones[zone].dustReward.gameObject);
		dustGO.layer = m_RewardZones[zone].DustGameObject.layer;
		dustGO.transform.parent = m_RewardZones[zone].ZoneRoot.transform;
		dustGO.transform.localPosition = localPosition;
		float randRot = UnityEngine.Random.Range(0f - m_FireworksRewardRanRot, m_FireworksRewardRanRot);
		dustGO.transform.localEulerAngles = new Vector3(0f, randRot, 0f);
		dustGO.transform.localScale = m_RewardZones[zone].DustGameObject.transform.localScale;
		ArcaneDustReward dustReward = dustGO.GetComponent<ArcaneDustReward>();
		yield return null;
		dustReward.SetData(data, updateVisuals: true);
		dustReward.m_showBanner = false;
		dustReward.m_playSounds = false;
		dustReward.Show(updateCacheValues: true);
		yield return new WaitForSeconds(m_FirewarksRewardHold);
		dustReward.HideWithFX();
		yield return new WaitForSeconds(3f);
		if (dustGO != null)
		{
			UnityEngine.Object.Destroy(dustGO);
		}
	}

	private Vector3 ZoneRandomLocalPosition(int zone)
	{
		Bounds bounds = m_RewardZones[zone].Collider.bounds;
		float x = UnityEngine.Random.Range(0f - bounds.extents.x, bounds.extents.x);
		float z = UnityEngine.Random.Range(0f - bounds.extents.z, bounds.extents.z);
		return new Vector3(x, 0f, z);
	}

	private IEnumerator ShowCards(int wins)
	{
		if (m_RewardsReceived.CardsCount == 0)
		{
			StartCoroutine(ShowFinalRewards(wins));
			yield break;
		}
		int cardNum = 0;
		for (int i = 0; i < m_Rewards.Count; i++)
		{
			if (m_Rewards[i].RewardType == Reward.Type.CARD)
			{
				GameObject cardRoot = m_CardVisuals[m_RewardsReceived.CardsCount - 1].m_Cards[cardNum];
				if (cardRoot == null)
				{
					Debug.LogWarningFormat("ShowCards() m_CardVisuals[{0}].m_Cards[{1}] is null!", m_RewardsReceived.CardsCount, cardNum);
					continue;
				}
				Vector3 targetPosition = cardRoot.transform.position;
				float fireworkWaitTime = DisplayFirework(m_CardFireworkFSM, targetPosition);
				yield return new WaitForSeconds(fireworkWaitTime);
				cardRoot.SetActive(value: true);
				yield return null;
				cardRoot.GetComponentInChildren<CardBurstLegendary>().Activate();
				yield return new WaitForSeconds(m_CardRewardBurstDelay);
				CardReward componentInChildren = cardRoot.GetComponentInChildren<CardReward>();
				componentInChildren.SetData(m_Rewards[i], updateVisuals: true);
				componentInChildren.m_showBanner = false;
				componentInChildren.m_showCardCount = false;
				componentInChildren.m_RotateIn = false;
				componentInChildren.Show(updateCacheValues: false);
				yield return new WaitForSeconds(m_CardRewardDelay);
				cardNum++;
			}
		}
		if (m_RewardsReceived.CardsCount >= 3)
		{
			for (int j = 0; j < m_CardVisuals[m_RewardsReceived.CardsCount - 1].m_Cards.Length; j++)
			{
				UberFloaty floaty = m_CardVisuals[m_RewardsReceived.CardsCount - 1].m_Cards[j].GetComponentInChildren<UberFloaty>();
				if (floaty != null)
				{
					floaty.enabled = false;
				}
				Hashtable posArgs = iTweenManager.Get().GetTweenHashTable();
				posArgs.Add("position", m_CardVisuals[m_RewardsReceived.CardsCount - 1].m_CardTargets[j].transform.position);
				posArgs.Add("time", m_CardAnimationTime);
				posArgs.Add("easetype", iTween.EaseType.easeInOutCubic);
				posArgs.Add("islocal", false);
				iTween.MoveTo(m_CardVisuals[m_RewardsReceived.CardsCount - 1].m_Cards[j], posArgs);
				Hashtable rotArgs = iTweenManager.Get().GetTweenHashTable();
				rotArgs.Add("rotation", m_CardVisuals[m_RewardsReceived.CardsCount - 1].m_CardTargets[j].transform.localEulerAngles);
				rotArgs.Add("time", m_CardAnimationTime);
				rotArgs.Add("easetype", iTween.EaseType.easeInOutCubic);
				rotArgs.Add("islocal", true);
				iTween.RotateTo(m_CardVisuals[m_RewardsReceived.CardsCount - 1].m_Cards[j], rotArgs);
			}
		}
		yield return new WaitForSeconds(m_CardAnimationTime * 0.5f);
		StartCoroutine(ShowFinalRewards(wins));
		if (m_RewardsReceived.CardsCount >= 3)
		{
			yield return new WaitForSeconds(m_CardAnimationTime * 0.5f);
			for (int k = 0; k < m_CardVisuals[m_RewardsReceived.CardsCount - 1].m_Cards.Length; k++)
			{
				UberFloaty floaty2 = m_CardVisuals[m_RewardsReceived.CardsCount - 1].m_Cards[k].GetComponentInChildren<UberFloaty>();
				if (floaty2 != null)
				{
					floaty2.enabled = true;
				}
			}
		}
		PlayMakerFSM fsm = m_CardsRoot.GetComponent<PlayMakerFSM>();
		if (fsm != null)
		{
			fsm.SendEvent("Birth");
		}
	}

	private void InitRewardsReceived()
	{
		m_RewardsReceived.PackID = 1;
		m_RewardsReceived.PackCount = 0;
		m_RewardsReceived.DustCount = 0;
		m_RewardsReceived.GoldCount = 0;
		m_RewardsReceived.CardsCount = 0;
		m_RewardsReceived.Cards = new List<RewardCardReceived>();
		for (int i = 0; i < m_finalRewards.Count; i++)
		{
			Reward reward = m_finalRewards[i];
			switch (reward.RewardType)
			{
			case Reward.Type.BOOSTER_PACK:
			{
				BoosterPackRewardData boosterReward = (BoosterPackRewardData)reward.Data;
				m_RewardsReceived.PackID = boosterReward.Id;
				m_RewardsReceived.PackCount = boosterReward.Count;
				break;
			}
			case Reward.Type.ARCANE_DUST:
			{
				ArcaneDustRewardData arcaneReward = (ArcaneDustRewardData)reward.Data;
				m_RewardsReceived.DustCount = arcaneReward.Amount;
				break;
			}
			case Reward.Type.GOLD:
			{
				GoldRewardData goldReward = (GoldRewardData)reward.Data;
				m_RewardsReceived.GoldCount = (int)goldReward.Amount;
				break;
			}
			case Reward.Type.CARD:
			{
				CardRewardData cardReward = (CardRewardData)reward.Data;
				RewardCardReceived card = default(RewardCardReceived);
				card.CardID = cardReward.CardID;
				card.Premium = cardReward.Premium;
				EntityDef entityDef = DefLoader.Get().GetEntityDef(card.CardID);
				if (entityDef == null)
				{
					Debug.LogWarningFormat("InitRewardsReceived() - entityDef for Card ID {0} is null", card.CardID);
					return;
				}
				card.CardEntityDef = entityDef;
				m_RewardsReceived.Cards.Add(card);
				m_RewardsReceived.CardsCount++;
				break;
			}
			}
		}
	}

	private int NextRewardZone()
	{
		int zoneIdx = UnityEngine.Random.Range(0, m_RewardZones.Length);
		if (zoneIdx == m_lastZone)
		{
			zoneIdx = UnityEngine.Random.Range(0, m_RewardZones.Length);
			if (zoneIdx == m_lastZone)
			{
				zoneIdx++;
				if (zoneIdx >= m_RewardZones.Length)
				{
					zoneIdx = 0;
				}
			}
		}
		m_lastZone = zoneIdx;
		return zoneIdx;
	}

	private IEnumerator ShowFinalRewards(int wins, bool simpleRewards = false)
	{
		while (!IsFinalRewardsLoaded())
		{
			yield return null;
		}
		m_FSM.SendEvent("HideBox");
		m_FinalRewardsRoot.SetActive(value: true);
		string bannerText = ((wins != 0) ? GameStrings.Format("GLUE_HEROIC_BRAWL_REWARDS_WIN_BANNER_TEXT", wins, wins) : GameStrings.Get("GLUE_HEROIC_BRAWL_NO_WINS_REWARD_PACK_TEXT"));
		m_BannerUberText.Text = bannerText;
		RewardVisuals rewardVisual = m_RewardVisuals[wins];
		for (int i = 0; i < m_finalRewards.Count; i++)
		{
			Reward reward = m_finalRewards[i];
			reward.m_playSounds = false;
			reward.m_showBanner = false;
			Transform rewardBone = null;
			switch (reward.RewardType)
			{
			case Reward.Type.BOOSTER_PACK:
				reward.transform.parent = rewardVisual.m_FinalPacksBone;
				reward.Show(updateCacheValues: false);
				rewardBone = rewardVisual.m_FinalPacksBone;
				break;
			case Reward.Type.ARCANE_DUST:
				reward.transform.parent = rewardVisual.m_FinalDustBone;
				reward.Show(updateCacheValues: false);
				rewardBone = rewardVisual.m_FinalDustBone;
				break;
			case Reward.Type.GOLD:
				reward.transform.parent = rewardVisual.m_FinalGoldBone;
				reward.Show(updateCacheValues: false);
				rewardBone = rewardVisual.m_FinalGoldBone;
				break;
			}
			if (simpleRewards && rewardBone != null)
			{
				PlayMakerFSM fsm = rewardBone.GetComponent<PlayMakerFSM>();
				if (fsm != null)
				{
					fsm.SendEvent("Birth");
				}
			}
			if (!simpleRewards)
			{
				PlayMakerFSM fsm2 = m_FinalRewardsRoot.GetComponent<PlayMakerFSM>();
				if (fsm2 != null)
				{
					fsm2.SendEvent("Birth");
				}
			}
			reward.transform.localPosition = Vector3.zero;
			reward.transform.localRotation = Quaternion.identity;
			reward.transform.localScale = Vector3.one;
		}
		AllDone();
	}

	private void LoadFinalRewards()
	{
		m_finalRewardsLoadedCount = 0;
		for (int i = 0; i < m_Rewards.Count; i++)
		{
			m_Rewards[i].LoadRewardObject(FinalRewardLoaded);
		}
	}

	private bool IsFinalRewardsLoaded()
	{
		return m_finalRewardsLoadedCount >= m_Rewards.Count;
	}

	private void FinalRewardLoaded(Reward reward, object callbackData)
	{
		m_finalRewardsLoadedCount++;
		if (reward == null)
		{
			Debug.LogWarningFormat("HeroicBrawlRewardDisplay.FinalRewardLoaded() - FAILED to load reward");
			return;
		}
		if (reward.gameObject == null)
		{
			Debug.LogWarningFormat("HeroicBrawlRewardDisplay.FinalRewardLoaded() - reward GameObject is null");
			return;
		}
		reward.gameObject.layer = base.gameObject.layer;
		m_finalRewards.Add(reward);
	}

	private void AllDone()
	{
		m_DoneButton.gameObject.SetActive(value: true);
		Spell component = m_DoneButton.m_button.GetComponent<Spell>();
		component.AddFinishedCallback(OnDoneButtonShown);
		component.ActivateState(SpellStateType.BIRTH);
	}

	private void OnDoneButtonShown(Spell spell, object userData)
	{
		m_DoneButton.AddEventListener(UIEventType.RELEASE, OnDoneButtonPressed);
	}

	private void OnDoneButtonPressed(UIEvent e)
	{
		m_DoneButton.m_button.GetComponent<Spell>().ActivateState(SpellStateType.DEATH);
		iTween.ScaleTo(m_Root, Vector3.zero, m_EndScaleAwayTime);
		m_screenEffectsHandle.StopEffect();
		if (m_fromNotice)
		{
			Network.Get().AckNotice(m_noticeID);
		}
		foreach (Action doneCallback2 in m_doneCallbacks)
		{
			doneCallback2?.Invoke();
		}
		StartCoroutine(OnDone());
	}

	private IEnumerator OnDone()
	{
		yield return new WaitForSeconds(m_EndScaleAwayTime);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void LoadRewardCards()
	{
		for (int i = 0; i < m_RewardsReceived.Cards.Count; i++)
		{
			RewardCardReceived rewardCard = m_RewardsReceived.Cards[i];
			string actorPath = ActorNames.GetHandActor(rewardCard.CardEntityDef);
			GameObject cardGO = AssetLoader.Get().InstantiatePrefab(actorPath, AssetLoadingOptions.IgnorePrefabPosition);
			RewardCardReceived loadedCard = default(RewardCardReceived);
			loadedCard.CardGameObject = cardGO;
			loadedCard.CardID = rewardCard.CardID;
			loadedCard.CardEntityDef = rewardCard.CardEntityDef;
			loadedCard.Premium = rewardCard.Premium;
			m_RewardsReceived.Cards[i] = loadedCard;
		}
	}

	private void LoadBoosterReward()
	{
		string assetName = "BoosterPackReward.prefab:b3f2b69bf55efe2419ca6d55c46f7fa7";
		for (int i = 0; i < m_RewardZones.Length; i++)
		{
			GameObject obj = AssetLoader.Get().InstantiatePrefab(assetName, AssetLoadingOptions.IgnorePrefabPosition);
			obj.transform.parent = m_RewardZones[i].PackGameObject.transform;
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localRotation = Quaternion.identity;
			obj.transform.localScale = Vector3.one;
			m_RewardZones[i].packReward = m_RewardZones[i].PackGameObject.GetComponentInChildren<BoosterPackReward>();
		}
	}

	private List<RewardData> DebugRewards(int wins)
	{
		int packCount = 0;
		int dustCount = 0;
		int goldCount = 0;
		int legendaryCount = 0;
		switch (wins)
		{
		case 0:
			packCount = 1;
			break;
		case 1:
			packCount = 2;
			break;
		case 2:
			packCount = 3;
			break;
		case 3:
			packCount = 4;
			dustCount = 200;
			goldCount = 200;
			break;
		case 4:
			packCount = 5;
			dustCount = 350;
			goldCount = 350;
			break;
		case 5:
			packCount = 6;
			dustCount = 400;
			goldCount = 400;
			break;
		case 6:
			packCount = 7;
			dustCount = 450;
			goldCount = 450;
			break;
		case 7:
			packCount = 8;
			dustCount = 500;
			goldCount = 500;
			break;
		case 8:
			packCount = 9;
			dustCount = 550;
			goldCount = 550;
			break;
		case 9:
			packCount = 14;
			dustCount = 800;
			goldCount = 800;
			break;
		case 10:
			packCount = 15;
			dustCount = 950;
			goldCount = 950;
			legendaryCount = 1;
			break;
		case 11:
			packCount = 20;
			dustCount = 1000;
			goldCount = 1000;
			legendaryCount = 2;
			break;
		case 12:
			packCount = 50;
			dustCount = 2300;
			goldCount = 2300;
			legendaryCount = 3;
			break;
		}
		List<RewardData> rewards = new List<RewardData>();
		if (packCount > 0)
		{
			BoosterPackRewardData packReward = new BoosterPackRewardData();
			packReward.Count = packCount;
			packReward.Id = 11;
			packReward.MarkAsDummyReward();
			rewards.Add(packReward);
		}
		if (goldCount > 0)
		{
			GoldRewardData goldReward = new GoldRewardData();
			goldReward.Amount = goldCount;
			goldReward.MarkAsDummyReward();
			rewards.Add(goldReward);
		}
		if (dustCount > 0)
		{
			ArcaneDustRewardData dustReward = new ArcaneDustRewardData();
			dustReward.Amount = dustCount;
			dustReward.MarkAsDummyReward();
			rewards.Add(dustReward);
		}
		if (legendaryCount > 0)
		{
			string[] testCards = new string[3] { "NEW1_030", "NEW1_030", "NEW1_030" };
			for (int c = 0; c < legendaryCount; c++)
			{
				CardRewardData cardReward = new CardRewardData();
				cardReward.CardID = testCards[c];
				cardReward.Premium = TAG_PREMIUM.GOLDEN;
				cardReward.MarkAsDummyReward();
				rewards.Add(cardReward);
			}
		}
		return rewards;
	}
}
