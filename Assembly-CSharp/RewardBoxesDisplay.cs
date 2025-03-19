using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CustomEditClass]
public class RewardBoxesDisplay : MonoBehaviour
{
	[Serializable]
	public class RewardPackageData
	{
		public Transform m_StartBone;

		public Transform m_TargetBone;

		public float m_StartDelay;
	}

	[Serializable]
	public class RewardSet
	{
		public GameObject m_RewardPackage;

		public GameObject m_BonusRewardPackage;

		public float m_AnimationTime = 1f;

		public GameObject m_RewardCard;

		public GameObject m_RewardCardBack;

		public GameObject m_RewardGold;

		public GameObject m_RewardDust;

		public GameObject m_RewardMercenaryCoin;

		public GameObject m_RewardMercenaryExp;

		public GameObject m_RewardRenown;

		public GameObject m_RewardBattlegroundsToken;

		public int m_MaxPackagesPerPage;

		public List<BoxRewardData> m_RewardData;
	}

	[Serializable]
	public class BoxRewardData
	{
		public List<RewardPackageData> m_PackageData;
	}

	public class RewardBoxData
	{
		public GameObject m_GameObject;

		public RewardPackage m_RewardPackage;

		public PlayMakerFSM m_FSM;

		public int m_Index;
	}

	public bool m_playBoxFlyoutSound = true;

	public GameObject m_Root;

	public GameObject m_ClickCatcher;

	[CustomEditField(Sections = "Reward Panel")]
	public NormalButton m_DoneButton;

	public NormalButton m_BonusLootNextButton;

	public RewardSet m_RewardSet;

	private List<Action> m_doneCallbacks;

	private List<GameObject> m_InstancedObjects;

	private GameObject[] m_RewardObjects;

	private List<RewardPackageData> m_RewardPackages;

	private GameLayer m_layer = GameLayer.IgnoreFullScreenEffects;

	private bool m_useDarkeningClickCatcher;

	private bool m_doneButtonFinishedShown;

	private bool m_destroyed;

	private List<RewardData> m_rewards;

	private List<RewardData> m_bonusRewards;

	private int m_currentPageNum;

	private int m_lastPageNum;

	private int m_numRegularRewardPages;

	private bool m_showingBonusRewards;

	private List<GameObject> m_rewardPackageInstances = new List<GameObject>();

	private bool m_hasFadedFullScreenEffectsOut;

	protected const string DEFAULT_PREFAB = "RewardBoxes.prefab:f136fead3d6a148c6801f1e3bd2e8267";

	protected const string MERCENARY_PREFAB = "RewardBoxes_Mercenary.prefab:3c55d213147b7bb4fbcf50b9145857eb";

	private static RewardBoxesDisplay s_Instance;

	private ScreenEffectsHandle m_screenEffectsHandle;

	private bool IsOnLastPage => m_currentPageNum >= m_lastPageNum;

	private bool IsOnLastRegularRewardPage => m_currentPageNum == m_numRegularRewardPages - 1;

	private List<RewardData> CurrentPageRewards
	{
		get
		{
			List<RewardData> currentRewards = m_rewards.Skip(m_RewardSet.m_MaxPackagesPerPage * m_currentPageNum).Take(m_RewardSet.m_MaxPackagesPerPage).ToList();
			m_showingBonusRewards = false;
			if (currentRewards.Count == 0 && m_bonusRewards != null)
			{
				currentRewards = m_bonusRewards.Skip(m_RewardSet.m_MaxPackagesPerPage * (m_currentPageNum - m_numRegularRewardPages)).Take(m_RewardSet.m_MaxPackagesPerPage).ToList();
				m_showingBonusRewards = true;
			}
			return currentRewards;
		}
	}

	public bool IsClosing { get; private set; }

	private void Awake()
	{
		s_Instance = this;
		m_InstancedObjects = new List<GameObject>();
		m_doneCallbacks = new List<Action>();
		RenderUtils.SetAlpha(m_ClickCatcher, 0f);
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	private void Start()
	{
		if (m_RewardSet.m_RewardPackage != null)
		{
			m_RewardSet.m_RewardPackage.SetActive(value: false);
		}
		if (m_RewardSet.m_BonusRewardPackage != null)
		{
			m_RewardSet.m_BonusRewardPackage.SetActive(value: false);
		}
	}

	private void OnDestroy()
	{
		CleanUp();
		m_destroyed = true;
	}

	public static RewardBoxesDisplay Get()
	{
		return s_Instance;
	}

	public static string GetPrefab(List<RewardData> rewards)
	{
		if (rewards != null)
		{
			using List<RewardData>.Enumerator enumerator = rewards.GetEnumerator();
			while (enumerator.MoveNext())
			{
				switch (enumerator.Current.RewardType)
				{
				case Reward.Type.MERCENARY_COIN:
				case Reward.Type.MERCENARY_EXP:
				case Reward.Type.MERCENARY_ABILITY_UNLOCK:
				case Reward.Type.MERCENARY_EQUIPMENT:
				case Reward.Type.MERCENARY_BOOSTER:
				case Reward.Type.MERCENARY_MERCENARY:
				case Reward.Type.MERCENARY_RANDOM_MERCENARY:
				case Reward.Type.MERCENARY_KNOCKOUT:
				case Reward.Type.MERCENARY_RENOWN:
					return "RewardBoxes_Mercenary.prefab:3c55d213147b7bb4fbcf50b9145857eb";
				}
			}
		}
		return "RewardBoxes.prefab:f136fead3d6a148c6801f1e3bd2e8267";
	}

	public void SetRewards(List<RewardData> rewards, List<RewardData> bonusRewards = null)
	{
		m_rewards = rewards;
		int remainder;
		int numPages = Math.DivRem(m_rewards.Count, m_RewardSet.m_MaxPackagesPerPage, out remainder);
		if (remainder > 0)
		{
			numPages++;
		}
		m_numRegularRewardPages = numPages;
		int numBonusPages = 0;
		if (bonusRewards != null)
		{
			m_bonusRewards = bonusRewards;
			remainder = 0;
			numBonusPages = Math.DivRem(m_bonusRewards.Count, m_RewardSet.m_MaxPackagesPerPage, out remainder);
			if (remainder > 0)
			{
				numBonusPages++;
			}
		}
		m_lastPageNum = numPages + numBonusPages - 1;
	}

	public void UseDarkeningClickCatcher(bool value)
	{
		m_useDarkeningClickCatcher = value;
		m_ClickCatcher.layer = 0;
	}

	public void RegisterDoneCallback(Action action)
	{
		m_doneCallbacks.Add(action);
	}

	public List<RewardPackageData> GetPackageData(int rewardCount)
	{
		for (int idx = 0; idx < m_RewardSet.m_RewardData.Count; idx++)
		{
			if (m_RewardSet.m_RewardData[idx].m_PackageData.Count == rewardCount)
			{
				return m_RewardSet.m_RewardData[idx].m_PackageData;
			}
		}
		Debug.LogError("RewardBoxesDisplay: GetPackageData - no package data found with a reward count of " + rewardCount);
		return null;
	}

	public void SetLayer(GameLayer layer)
	{
		m_layer = layer;
		LayerUtils.SetLayer(base.gameObject, m_layer);
	}

	public void ShowAlreadyOpenedRewards()
	{
		List<RewardData> rewardData = CurrentPageRewards;
		m_RewardPackages = GetPackageData(rewardData.Count);
		m_RewardObjects = new GameObject[rewardData.Count];
		FadeFullscreenEffectsIn();
		ShowOpenedRewards(rewardData);
		AllDone();
	}

	public void ShowOpenedRewards(List<RewardData> rewardData)
	{
		for (int idx = 0; idx < m_RewardPackages.Count; idx++)
		{
			RewardPackageData package = m_RewardPackages[idx];
			if (package.m_TargetBone == null)
			{
				Debug.LogWarning("RewardBoxesDisplay: AnimateRewards package target bone is null!");
				break;
			}
			if (idx >= m_RewardObjects.Length || idx >= rewardData.Count)
			{
				Debug.LogWarning("RewardBoxesDisplay: AnimateRewards reward index exceeded!");
				break;
			}
			m_RewardObjects[idx] = CreateRewardInstance(rewardData[idx], idx, package.m_TargetBone.position, activeOnStart: true);
		}
	}

	public void AnimateRewards()
	{
		List<RewardData> rewardData = CurrentPageRewards;
		int numRewards = rewardData.Count;
		m_RewardPackages = GetPackageData(numRewards);
		m_RewardObjects = new GameObject[numRewards];
		for (int idx = 0; idx < m_RewardPackages.Count; idx++)
		{
			RewardPackageData package = m_RewardPackages[idx];
			if (package.m_TargetBone == null)
			{
				Debug.LogWarning("RewardBoxesDisplay: AnimateRewards package target bone is null!");
				return;
			}
			if (idx >= m_RewardObjects.Length || idx >= numRewards)
			{
				Debug.LogWarning("RewardBoxesDisplay: AnimateRewards reward index exceeded!");
				return;
			}
			m_RewardObjects[idx] = CreateRewardInstance(rewardData[idx], idx, package.m_TargetBone.position, activeOnStart: false);
		}
		RewardPackageAnimation();
	}

	public void OpenReward(int rewardIndex, Vector3 rewardPos)
	{
		if (rewardIndex >= m_RewardObjects.Length)
		{
			Debug.LogWarning("RewardBoxesDisplay: OpenReward reward index exceeded!");
			return;
		}
		GameObject rewardObj = m_RewardObjects[rewardIndex];
		if (rewardObj == null)
		{
			Debug.LogWarning("RewardBoxesDisplay: OpenReward object is null!");
			return;
		}
		if (!rewardObj.activeSelf)
		{
			rewardObj.SetActive(value: true);
		}
		if (CheckAllRewardsActive())
		{
			AllDone();
		}
	}

	private void RewardPackageAnimation()
	{
		if (m_showingBonusRewards && m_RewardSet.m_RewardPackage == null)
		{
			Debug.LogWarning("RewardBoxesDisplay: missing Bonus Reward Package!");
			return;
		}
		if (m_RewardSet.m_RewardPackage == null)
		{
			Debug.LogWarning("RewardBoxesDisplay: missing Reward Package!");
			return;
		}
		if (m_currentPageNum == 0)
		{
			FadeFullscreenEffectsIn();
		}
		foreach (GameObject go in m_rewardPackageInstances)
		{
			if (go != null)
			{
				UnityEngine.Object.Destroy(go);
			}
		}
		m_rewardPackageInstances.Clear();
		for (int idx = 0; idx < m_RewardPackages.Count; idx++)
		{
			RewardPackageData package = m_RewardPackages[idx];
			if (package.m_TargetBone == null || package.m_StartBone == null)
			{
				Debug.LogWarning("RewardBoxesDisplay: missing reward target bone!");
				continue;
			}
			GameObject rewardBox = ((!m_showingBonusRewards || !(m_RewardSet.m_BonusRewardPackage != null)) ? UnityEngine.Object.Instantiate(m_RewardSet.m_RewardPackage) : UnityEngine.Object.Instantiate(m_RewardSet.m_BonusRewardPackage));
			TransformUtil.AttachAndPreserveLocalTransform(rewardBox.transform, m_Root.transform);
			rewardBox.transform.position = package.m_StartBone.position;
			rewardBox.SetActive(value: true);
			m_InstancedObjects.Add(rewardBox);
			m_rewardPackageInstances.Add(rewardBox);
			Vector3 boxScale = rewardBox.transform.localScale;
			rewardBox.transform.localScale = Vector3.zero;
			RenderUtils.EnableColliders(rewardBox, enable: false);
			iTween.ScaleTo(rewardBox, iTween.Hash("scale", boxScale, "time", m_RewardSet.m_AnimationTime, "delay", package.m_StartDelay, "easetype", iTween.EaseType.linear));
			PlayMakerFSM fsm = rewardBox.GetComponent<PlayMakerFSM>();
			if (fsm == null)
			{
				Debug.LogWarning("RewardBoxesDisplay: missing reward Playmaker FSM!");
				continue;
			}
			if (!m_playBoxFlyoutSound)
			{
				fsm.FsmVariables.FindFsmBool("PlayFlyoutSound").Value = false;
			}
			RewardPackage arp = rewardBox.GetComponent<RewardPackage>();
			arp.m_RewardIndex = idx;
			RewardBoxData boxData = new RewardBoxData();
			boxData.m_GameObject = rewardBox;
			boxData.m_RewardPackage = arp;
			boxData.m_FSM = fsm;
			boxData.m_Index = idx;
			iTween.MoveTo(rewardBox, iTween.Hash("position", package.m_TargetBone.transform.position, "time", m_RewardSet.m_AnimationTime, "delay", package.m_StartDelay, "easetype", iTween.EaseType.linear, "onstarttarget", base.gameObject, "onstart", "RewardPackageOnStart", "onstartparams", boxData, "oncompletetarget", base.gameObject, "oncomplete", "RewardPackageOnComplete", "oncompleteparams", boxData));
		}
	}

	private void RewardPackageOnStart(RewardBoxData boxData)
	{
		boxData.m_FSM.SendEvent("Birth");
	}

	private void RewardPackageOnComplete(RewardBoxData boxData)
	{
		StartCoroutine(RewardPackageActivate(boxData));
	}

	private IEnumerator RewardPackageActivate(RewardBoxData boxData)
	{
		yield return new WaitForSeconds(0.5f);
		RenderUtils.EnableColliders(boxData.m_GameObject, enable: true);
		boxData.m_RewardPackage.AddEventListener(UIEventType.PRESS, RewardPackagePressed);
	}

	private void RewardPackagePressed(UIEvent e)
	{
		Log.RewardBox.Print("box clicked!");
	}

	private GameObject CreateRewardInstance(RewardData reward, int rewardIndex, Vector3 rewardPos, bool activeOnStart)
	{
		GameObject rewardObj = null;
		switch (reward.RewardType)
		{
		case Reward.Type.ARCANE_DUST:
		{
			rewardObj = UnityEngine.Object.Instantiate(m_RewardSet.m_RewardDust);
			TransformUtil.AttachAndPreserveLocalTransform(rewardObj.transform, m_Root.transform);
			rewardObj.transform.position = rewardPos;
			rewardObj.SetActive(value: true);
			UberText componentInChildren3 = rewardObj.GetComponentInChildren<UberText>();
			ArcaneDustRewardData dustData = (ArcaneDustRewardData)reward;
			componentInChildren3.Text = dustData.Amount.ToString();
			rewardObj.SetActive(activeOnStart);
			break;
		}
		case Reward.Type.BOOSTER_PACK:
		{
			BoosterPackRewardData boosterReward = reward as BoosterPackRewardData;
			int boosterTypeDatabaseID = boosterReward.Id;
			if (boosterTypeDatabaseID == 0)
			{
				boosterTypeDatabaseID = 1;
				Debug.LogWarning("RewardBoxesDisplay - booster reward is not valid. ID = 0");
			}
			Log.RewardBox.Print($"Booster DB ID: {boosterTypeDatabaseID}");
			string assetName = GameDbf.Booster.GetRecord(boosterTypeDatabaseID).ArenaPrefab;
			if (string.IsNullOrEmpty(assetName))
			{
				Debug.LogError($"RewardBoxesDisplay - no prefab found for booster {boosterReward.Id}!");
				break;
			}
			rewardObj = AssetLoader.Get().InstantiatePrefab(assetName);
			if (boosterReward.Count > 1)
			{
				UberText boosterCountText = rewardObj.GetComponentInChildren<UberText>(includeInactive: true);
				if (boosterCountText == null)
				{
					Debug.LogError($"RewardBoxesDisplay - no uber text found for booster {boosterReward.Id}!");
					break;
				}
				boosterCountText.transform.parent.gameObject.SetActive(value: true);
				boosterCountText.Text = boosterReward.Count.ToString();
			}
			TransformUtil.AttachAndPreserveLocalTransform(rewardObj.transform, m_Root.transform);
			rewardObj.transform.position = rewardPos;
			rewardObj.SetActive(activeOnStart);
			break;
		}
		case Reward.Type.GOLD:
		{
			rewardObj = UnityEngine.Object.Instantiate(m_RewardSet.m_RewardGold);
			TransformUtil.AttachAndPreserveLocalTransform(rewardObj.transform, m_Root.transform);
			rewardObj.transform.position = rewardPos;
			rewardObj.SetActive(value: true);
			UberText componentInChildren2 = rewardObj.GetComponentInChildren<UberText>();
			GoldRewardData goldData = (GoldRewardData)reward;
			componentInChildren2.Text = goldData.Amount.ToString();
			rewardObj.SetActive(activeOnStart);
			break;
		}
		case Reward.Type.BATTLEGROUNDS_TOKEN:
		{
			rewardObj = UnityEngine.Object.Instantiate(m_RewardSet.m_RewardBattlegroundsToken);
			TransformUtil.AttachAndPreserveLocalTransform(rewardObj.transform, m_Root.transform);
			rewardObj.transform.position = rewardPos;
			rewardObj.SetActive(value: true);
			UberText componentInChildren = rewardObj.GetComponentInChildren<UberText>();
			BattlegroundsTokenRewardData tokenData = (BattlegroundsTokenRewardData)reward;
			componentInChildren.Text = tokenData.Amount.ToString();
			rewardObj.SetActive(activeOnStart);
			break;
		}
		case Reward.Type.CARD:
		{
			rewardObj = UnityEngine.Object.Instantiate(m_RewardSet.m_RewardCard);
			TransformUtil.AttachAndPreserveLocalTransform(rewardObj.transform, m_Root.transform);
			rewardObj.transform.position = rewardPos;
			rewardObj.SetActive(value: true);
			CardRewardData cardData = (CardRewardData)reward;
			rewardObj.GetComponentInChildren<RewardCard>().LoadCard(cardData, m_layer);
			rewardObj.SetActive(activeOnStart);
			break;
		}
		case Reward.Type.CARD_BACK:
		{
			rewardObj = UnityEngine.Object.Instantiate(m_RewardSet.m_RewardCardBack);
			TransformUtil.AttachAndPreserveLocalTransform(rewardObj.transform, m_Root.transform);
			rewardObj.transform.position = rewardPos;
			rewardObj.SetActive(value: true);
			CardBackRewardData cardbackData = (CardBackRewardData)reward;
			rewardObj.GetComponentInChildren<RewardCardBack>().LoadCardBack(cardbackData, m_layer);
			rewardObj.SetActive(activeOnStart);
			break;
		}
		case Reward.Type.MERCENARY_COIN:
		{
			rewardObj = UnityEngine.Object.Instantiate(m_RewardSet.m_RewardMercenaryCoin);
			TransformUtil.AttachAndPreserveLocalTransform(rewardObj.transform, m_Root.transform);
			rewardObj.transform.position = rewardPos;
			rewardObj.SetActive(value: true);
			MercenaryCoinRewardData mercenaryCoinData = (MercenaryCoinRewardData)reward;
			rewardObj.GetComponentInChildren<RewardMercenaryCoin>().Initialize(mercenaryCoinData);
			rewardObj.SetActive(activeOnStart);
			break;
		}
		case Reward.Type.MERCENARY_EXP:
		{
			rewardObj = UnityEngine.Object.Instantiate(m_RewardSet.m_RewardMercenaryExp);
			TransformUtil.AttachAndPreserveLocalTransform(rewardObj.transform, m_Root.transform);
			rewardObj.transform.position = rewardPos;
			rewardObj.SetActive(value: true);
			MercenaryExpRewardData mercenaryExpData = (MercenaryExpRewardData)reward;
			rewardObj.GetComponentInChildren<RewardMercenaryExp>().Initialize(mercenaryExpData);
			rewardObj.SetActive(activeOnStart);
			break;
		}
		case Reward.Type.MERCENARY_RENOWN:
		{
			rewardObj = UnityEngine.Object.Instantiate(m_RewardSet.m_RewardRenown);
			TransformUtil.AttachAndPreserveLocalTransform(rewardObj.transform, m_Root.transform);
			rewardObj.transform.position = rewardPos;
			rewardObj.SetActive(value: true);
			MercenaryRenownRewardData mercenaryRenownData = (MercenaryRenownRewardData)reward;
			rewardObj.GetComponentInChildren<RewardMercenaryRenown>().Initialize(mercenaryRenownData);
			rewardObj.SetActive(activeOnStart);
			break;
		}
		}
		if (rewardObj == null)
		{
			Debug.LogWarning("RewardBoxesDisplay: Unable to create reward, object null!");
			return null;
		}
		if (rewardIndex >= m_RewardObjects.Length)
		{
			Debug.LogWarning("RewardBoxesDisplay: CreateRewardInstance reward index exceeded!");
			return null;
		}
		LayerUtils.SetLayer(rewardObj, m_layer);
		m_RewardObjects[rewardIndex] = rewardObj;
		m_InstancedObjects.Add(rewardObj);
		return rewardObj;
	}

	private void AllDone()
	{
		Vector3 donePosition = Vector3.zero;
		bool usingBonusLootButton = IsOnLastRegularRewardPage && m_bonusRewards != null && m_bonusRewards.Count > 0 && m_BonusLootNextButton != null;
		NormalButton targetButton;
		if (usingBonusLootButton)
		{
			targetButton = m_BonusLootNextButton;
			m_DoneButton.gameObject.SetActive(value: false);
		}
		else
		{
			targetButton = m_DoneButton;
			if (m_BonusLootNextButton != null)
			{
				m_BonusLootNextButton.gameObject.SetActive(value: false);
			}
		}
		if (m_RewardPackages.Count > 1)
		{
			for (int idx = 0; idx < m_RewardPackages.Count; idx++)
			{
				RewardPackageData package = m_RewardPackages[idx];
				donePosition += package.m_TargetBone.position;
			}
			targetButton.transform.position = donePosition / m_RewardPackages.Count;
		}
		targetButton.gameObject.SetActive(value: true);
		if (usingBonusLootButton)
		{
			targetButton.SetText(GameStrings.Get("GLUE_MERCENARIES_BONUS_LOOT_BUTTON"));
		}
		else if (IsOnLastPage)
		{
			targetButton.SetText(GameStrings.Get("GLOBAL_DONE"));
		}
		else
		{
			targetButton.SetText(GameStrings.Get("GLOBAL_BUTTON_NEXT"));
		}
		Spell doneButtonSpell = targetButton.m_button.GetComponent<Spell>();
		if (usingBonusLootButton)
		{
			doneButtonSpell.AddFinishedCallback(OnBonusLootButtonShown);
		}
		else
		{
			doneButtonSpell.AddFinishedCallback(OnDoneButtonShown);
		}
		doneButtonSpell.ActivateState(SpellStateType.BIRTH);
		if (IsOnLastPage)
		{
			NarrativeManager.Get().OnArenaRewardsShown();
		}
	}

	private void OnDoneButtonShown(Spell spell, object userData)
	{
		m_doneButtonFinishedShown = true;
		RenderUtils.EnableColliders(m_DoneButton.gameObject, enable: true);
		m_DoneButton.AddEventListener(UIEventType.RELEASE, OnDoneButtonPressed);
		m_DoneButton.AddEventListener(UIEventType.ROLLOVER, OnDoneButtonRollover);
		if (IsOnLastPage)
		{
			Navigation.Push(OnNavigateBack);
		}
	}

	private void OnBonusLootButtonShown(Spell spell, object userData)
	{
		m_doneButtonFinishedShown = true;
		RenderUtils.EnableColliders(m_BonusLootNextButton.gameObject, enable: true);
		m_BonusLootNextButton.AddEventListener(UIEventType.RELEASE, OnDoneButtonPressed);
		m_BonusLootNextButton.AddEventListener(UIEventType.ROLLOVER, OnBonusLootButtonRollover);
	}

	private void OnDoneButtonRollover(UIEvent e)
	{
		m_DoneButton.m_button.GetComponent<PlayMakerFSM>().SendEvent("Hover_Wiggle");
	}

	private void OnBonusLootButtonRollover(UIEvent e)
	{
		m_BonusLootNextButton.m_button.GetComponent<PlayMakerFSM>().SendEvent("Hover_Wiggle");
	}

	private void OnDoneButtonPressed(UIEvent e)
	{
		if (IsOnLastPage)
		{
			FadeFullscreenEffectsOut();
			Navigation.GoBack();
			return;
		}
		m_currentPageNum++;
		KillRewardObjects();
		KillDoneButton();
		StartCoroutine(AnimateRewardsWhenReady());
	}

	private IEnumerator AnimateRewardsWhenReady()
	{
		while (CheckAnyRewardActive())
		{
			yield return null;
		}
		AnimateRewards();
	}

	public void Close()
	{
		IsClosing = true;
		if (m_doneButtonFinishedShown)
		{
			OnNavigateBack();
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void KillRewardObjects()
	{
		GameObject[] rewardObjects = m_RewardObjects;
		foreach (GameObject rewardObject in rewardObjects)
		{
			if (!(rewardObject == null))
			{
				PlayMakerFSM rewardFSM = rewardObject.GetComponent<PlayMakerFSM>();
				if (rewardFSM != null)
				{
					rewardFSM.SendEvent("Death");
				}
				UberText[] componentsInChildren = rewardObject.GetComponentsInChildren<UberText>();
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					iTween.FadeTo(componentsInChildren[j].gameObject, iTween.Hash("alpha", 0f, "time", 0.8f, "includechildren", true, "easetype", iTween.EaseType.easeInOutCubic));
				}
				RewardCard rewardCard = rewardObject.GetComponentInChildren<RewardCard>();
				if (rewardCard != null)
				{
					rewardCard.Death();
				}
				UnityEngine.Object.Destroy(rewardObject, 0.8f);
			}
		}
	}

	private void KillDoneButton()
	{
		RenderUtils.EnableColliders(m_DoneButton.gameObject, enable: false);
		m_DoneButton.RemoveEventListener(UIEventType.RELEASE, OnDoneButtonPressed);
		m_DoneButton.RemoveEventListener(UIEventType.ROLLOVER, OnDoneButtonRollover);
		m_DoneButton.m_button.GetComponent<Spell>().ActivateState(SpellStateType.DEATH);
		if (m_BonusLootNextButton != null)
		{
			RenderUtils.EnableColliders(m_BonusLootNextButton.gameObject, enable: false);
			m_BonusLootNextButton.RemoveEventListener(UIEventType.RELEASE, OnDoneButtonPressed);
			m_BonusLootNextButton.RemoveEventListener(UIEventType.ROLLOVER, OnDoneButtonRollover);
			m_BonusLootNextButton.m_button.GetComponent<Spell>().ActivateState(SpellStateType.DEATH);
		}
	}

	private bool OnNavigateBack()
	{
		Debug.Log("navigating back!");
		if (!m_DoneButton.m_button.activeSelf)
		{
			return false;
		}
		KillRewardObjects();
		KillDoneButton();
		return true;
	}

	private void FadeFullscreenEffectsIn()
	{
		if (FullScreenFXMgr.Get() == null)
		{
			Debug.LogWarning("RewardBoxesDisplay: FullScreenFXMgr.Get() returned null!");
			return;
		}
		ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignettePerspective;
		screenEffectParameters.Blur = new BlurParameters(1f, 0.85f);
		m_screenEffectsHandle.StartEffect(screenEffectParameters);
		if (m_useDarkeningClickCatcher)
		{
			iTween.FadeTo(m_ClickCatcher, 0.75f, 0.5f);
		}
	}

	private void FadeFullscreenEffectsOut()
	{
		if (m_hasFadedFullScreenEffectsOut)
		{
			return;
		}
		m_hasFadedFullScreenEffectsOut = true;
		if (FullScreenFXMgr.Get() == null)
		{
			Debug.LogWarning("RewardBoxesDisplay: FullScreenFXMgr.Get() returned null!");
			return;
		}
		m_screenEffectsHandle.StopEffect(2f, iTween.EaseType.easeOutCirc, FadeFullscreenEffectsOutFinished);
		if (m_useDarkeningClickCatcher)
		{
			iTween.FadeTo(m_ClickCatcher, 0f, 0.5f);
		}
	}

	private void FadeVignetteIn()
	{
		m_screenEffectsHandle.StopEffect();
		ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.VignettePerspective;
		screenEffectParameters.Time = 1.5f;
		screenEffectParameters.EaseType = iTween.EaseType.easeOutCirc;
		screenEffectParameters.Vignette.Amount = 1.4f;
		m_screenEffectsHandle.StartEffect(screenEffectParameters);
	}

	private void FadeFullscreenEffectsOutFinished()
	{
		foreach (Action doneCallback2 in m_doneCallbacks)
		{
			doneCallback2?.Invoke();
		}
		m_doneCallbacks.Clear();
		if (!m_destroyed)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private bool CheckAllRewardsActive()
	{
		GameObject[] rewardObjects = m_RewardObjects;
		foreach (GameObject go in rewardObjects)
		{
			if (go == null || !go.activeSelf)
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckAnyRewardActive()
	{
		GameObject[] rewardObjects = m_RewardObjects;
		for (int i = 0; i < rewardObjects.Length; i++)
		{
			if (rewardObjects[i] != null)
			{
				return true;
			}
		}
		return false;
	}

	private void CleanUp()
	{
		foreach (GameObject go in m_InstancedObjects)
		{
			if (go != null)
			{
				UnityEngine.Object.Destroy(go);
			}
		}
		FadeFullscreenEffectsOut();
		s_Instance = null;
	}

	public void DebugLogRewards()
	{
		Debug.Log("BOX REWARDS:");
		List<RewardData> rewardData = CurrentPageRewards;
		for (int i = 0; i < rewardData.Count; i++)
		{
			RewardData reward = rewardData[i];
			Debug.Log($"  reward {i}={reward}");
		}
	}
}
