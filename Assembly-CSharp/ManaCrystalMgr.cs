using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.AssetManager;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

[CustomEditClass]
public class ManaCrystalMgr : MonoBehaviour
{
	private class LoadCrystalCallbackData
	{
		private bool m_isTempCrystal;

		private bool m_isTurnStart;

		public bool IsTempCrystal => m_isTempCrystal;

		public bool IsTurnStart => m_isTurnStart;

		public LoadCrystalCallbackData(bool isTempCrystal, bool isTurnStart)
		{
			m_isTempCrystal = isTempCrystal;
			m_isTurnStart = isTurnStart;
		}
	}

	public Texture redCrystalTexture;

	[CustomEditField(T = EditType.GAME_OBJECT)]
	public String_MobileOverride manaLockPrefab;

	public ManaCrystalEventSpells m_eventSpells;

	public SlidingTray manaTrayPhone;

	public Transform manaGemBone;

	public GameObject friendlyManaCounter;

	public GameObject opposingManaCounter;

	public GameObject teammateManaCounter;

	public List<ManaCrystalAssetPaths> m_ManaCrystalAssetTable = new List<ManaCrystalAssetPaths>();

	public int maxManaCrystalToDisplay = 10;

	private const float SECS_BETW_MANA_SPAWNS = 0.2f;

	private const float SECS_BETW_MANA_READIES = 0.2f;

	private const float SECS_BETW_MANA_SPENDS = 0.2f;

	private const float GEM_FLIP_TEXT_FADE_TIME = 0.1f;

	private readonly string GEM_FLIP_ANIM_NAME = "Resource_Large_phone_Flip";

	private static ManaCrystalMgr s_instance;

	private ManaCrystalType m_manaCrystalType;

	private List<ManaCrystal> m_permanentCrystals;

	private List<ManaCrystal> m_temporaryCrystals;

	private int m_proposedManaSourceEntID = -1;

	private int m_numCrystalsLoading;

	private int m_numQueuedToSpawn;

	private int m_numQueuedToReady;

	private int m_numQueuedToSpend;

	private int m_additionalOverloadedCrystalsOwedNextTurn;

	private int m_additionalOverloadedCrystalsOwedThisTurn;

	private bool m_overloadLocksAreShowing;

	private float m_manaCrystalWidth;

	private GameObject m_friendlyManaGem;

	private UberText m_friendlyManaText;

	private AssetHandle<Texture> m_friendlyManaGemTexture;

	private bool m_disableManaCrystalVisuals;

	private void Awake()
	{
		s_instance = this;
		if (base.gameObject.GetComponent<AudioSource>() == null)
		{
			base.gameObject.AddComponent<AudioSource>();
		}
	}

	private void OnDestroy()
	{
		s_instance = null;
		AssetHandle.SafeDispose(ref m_friendlyManaGemTexture);
	}

	private void Start()
	{
		m_permanentCrystals = new List<ManaCrystal>();
		m_temporaryCrystals = new List<ManaCrystal>();
		InitializePhoneManaGems();
	}

	public static ManaCrystalMgr Get()
	{
		return s_instance;
	}

	public void Reset()
	{
		StopAllCoroutines();
		DestroyManaCrystals(m_permanentCrystals.Count);
		DestroyTempManaCrystals(m_temporaryCrystals.Count);
		UnlockCrystals(m_additionalOverloadedCrystalsOwedThisTurn);
		ReclaimCrystalsOwedForOverload(m_additionalOverloadedCrystalsOwedNextTurn);
		m_manaCrystalType = ManaCrystalType.DEFAULT;
	}

	public void ResetUnresolvedManaToBeReadied()
	{
		if (m_numQueuedToReady < 0)
		{
			m_numQueuedToReady = 0;
		}
	}

	public void SetManaCrystalType(ManaCrystalType type)
	{
		m_manaCrystalType = type;
		InitializePhoneManaGems();
	}

	public Vector3 GetManaCrystalSpawnPosition()
	{
		return base.transform.position;
	}

	public void AddManaCrystals(int numCrystals, bool isTurnStart)
	{
		for (int i = 0; i < numCrystals; i++)
		{
			GameState.Get().GetGameEntity().NotifyOfManaCrystalSpawned();
			if (!m_disableManaCrystalVisuals)
			{
				StartCoroutine(WaitThenAddManaCrystal(isTemp: false, isTurnStart));
			}
		}
	}

	public void AddTempManaCrystals(int numCrystals)
	{
		if (!m_disableManaCrystalVisuals)
		{
			for (int i = 0; i < numCrystals; i++)
			{
				StartCoroutine(WaitThenAddManaCrystal(isTemp: true, isTurnStart: false));
			}
		}
	}

	public void DestroyManaCrystals(int numCrystals)
	{
		StartCoroutine(WaitThenDestroyManaCrystals(isTemp: false, numCrystals));
	}

	public void DestroyTempManaCrystals(int numCrystals)
	{
		StartCoroutine(WaitThenDestroyManaCrystals(isTemp: true, numCrystals));
	}

	public void UpdateSpentMana(int shownChangeAmount)
	{
		if (shownChangeAmount > 0)
		{
			SpendManaCrystals(shownChangeAmount);
		}
		else if (GameState.Get().IsTurnStartManagerActive())
		{
			TurnStartManager.Get().NotifyOfManaCrystalFilled(-shownChangeAmount);
		}
		else
		{
			ReadyManaCrystals(-shownChangeAmount);
		}
	}

	public void SpendManaCrystals(int numCrystals)
	{
		ManaCrystalAssetPaths assetPaths = GetManaCrystalAssetPaths(m_manaCrystalType);
		SoundManager.Get().LoadAndPlay(assetPaths.m_SoundOnSpendPath, base.gameObject);
		for (int i = 0; i < numCrystals; i++)
		{
			SpendManaCrystal();
		}
	}

	public void ReadyManaCrystals(int numCrystals)
	{
		for (int i = 0; i < numCrystals; i++)
		{
			ReadyManaCrystal();
		}
	}

	public int GetSpendableManaCrystals()
	{
		int count = 0;
		for (int i = 0; i < m_temporaryCrystals.Count; i++)
		{
			if (m_temporaryCrystals[i].state == ManaCrystal.State.READY)
			{
				count++;
			}
		}
		for (int j = 0; j < m_permanentCrystals.Count; j++)
		{
			ManaCrystal crystal = m_permanentCrystals[j];
			if (crystal.state == ManaCrystal.State.READY && !crystal.IsOverloaded())
			{
				count++;
			}
		}
		return count;
	}

	public void CancelAllProposedMana(Entity entity)
	{
		if (entity == null || m_proposedManaSourceEntID != entity.GetEntityId())
		{
			return;
		}
		m_proposedManaSourceEntID = -1;
		m_eventSpells.m_proposeUsageSpell.ActivateState(SpellStateType.DEATH);
		for (int i = 0; i < m_temporaryCrystals.Count; i++)
		{
			if (m_temporaryCrystals[i].state == ManaCrystal.State.PROPOSED)
			{
				m_temporaryCrystals[i].state = ManaCrystal.State.READY;
			}
		}
		for (int i2 = m_permanentCrystals.Count - 1; i2 >= 0; i2--)
		{
			if (m_permanentCrystals[i2].state == ManaCrystal.State.PROPOSED)
			{
				m_permanentCrystals[i2].state = ManaCrystal.State.READY;
			}
		}
	}

	public void ProposeManaCrystalUsage(Entity entity, bool fromDeckAction = false)
	{
		if (entity == null)
		{
			return;
		}
		m_proposedManaSourceEntID = entity.GetEntityId();
		int numProposedCrystals = entity.GetCost();
		if (fromDeckAction)
		{
			numProposedCrystals = entity.GetTag(GAME_TAG.DECK_ACTION_COST);
		}
		m_eventSpells.m_proposeUsageSpell.ActivateState(SpellStateType.BIRTH);
		int updatedCrystals = 0;
		for (int i = m_temporaryCrystals.Count - 1; i >= 0; i--)
		{
			if (m_temporaryCrystals[i].state == ManaCrystal.State.USED)
			{
				Log.Gameplay.Print("Found a SPENT temporary mana crystal... this shouldn't happen!");
			}
			else if (updatedCrystals < numProposedCrystals)
			{
				m_temporaryCrystals[i].state = ManaCrystal.State.PROPOSED;
				updatedCrystals++;
			}
			else
			{
				m_temporaryCrystals[i].state = ManaCrystal.State.READY;
			}
		}
		for (int j = 0; j < m_permanentCrystals.Count; j++)
		{
			if (m_permanentCrystals[j].state != ManaCrystal.State.USED && !m_permanentCrystals[j].IsOverloaded())
			{
				if (updatedCrystals < numProposedCrystals)
				{
					m_permanentCrystals[j].state = ManaCrystal.State.PROPOSED;
					updatedCrystals++;
				}
				else
				{
					m_permanentCrystals[j].state = ManaCrystal.State.READY;
				}
			}
		}
	}

	public void HandleSameTurnOverloadChanged(int crystalsChanged)
	{
		if (crystalsChanged > 0)
		{
			MarkCrystalsOwedForOverload(crystalsChanged);
		}
		else if (crystalsChanged < 0)
		{
			ReclaimCrystalsOwedForOverload(-crystalsChanged);
		}
	}

	public void SetCrystalsLockedForOverload(int numCrystals)
	{
		StartCoroutine(WaitForCrystalsToLoadThenLockThem(numCrystals));
	}

	private IEnumerator WaitForCrystalsToLoadThenLockThem(int numCrystals)
	{
		while (m_numCrystalsLoading > 0)
		{
			yield return null;
		}
		for (int crystalIndex = 0; crystalIndex < numCrystals; crystalIndex++)
		{
			if (crystalIndex < m_permanentCrystals.Count)
			{
				m_permanentCrystals[crystalIndex].PayOverload();
			}
		}
	}

	public void MarkCrystalsOwedForOverload(int numCrystals)
	{
		if (numCrystals > 0)
		{
			m_overloadLocksAreShowing = true;
		}
		int numCrystalsMarkedForOverload = 0;
		int crystalIndex = 0;
		while (numCrystals != numCrystalsMarkedForOverload)
		{
			if (crystalIndex == m_permanentCrystals.Count)
			{
				m_additionalOverloadedCrystalsOwedNextTurn += numCrystals - numCrystalsMarkedForOverload;
				break;
			}
			ManaCrystal crystal = m_permanentCrystals[crystalIndex];
			if (!crystal.IsOwedForOverload())
			{
				crystal.MarkAsOwedForOverload();
				numCrystalsMarkedForOverload++;
			}
			crystalIndex++;
		}
	}

	public void ReclaimCrystalsOwedForOverload(int numCrystals)
	{
		int numCrystalsReclaimed = 0;
		int leftMostLockIndex = m_permanentCrystals.FindLastIndex((ManaCrystal crystal) => crystal.IsOwedForOverload());
		for (; numCrystalsReclaimed < numCrystals; numCrystalsReclaimed++)
		{
			if (leftMostLockIndex < 0)
			{
				break;
			}
			m_permanentCrystals[leftMostLockIndex].ReclaimOverload();
			leftMostLockIndex--;
		}
		m_additionalOverloadedCrystalsOwedNextTurn -= numCrystals - numCrystalsReclaimed;
		m_overloadLocksAreShowing = leftMostLockIndex >= 0 || m_additionalOverloadedCrystalsOwedNextTurn > 0;
	}

	public void UnlockCrystals(int numCrystals)
	{
		int numCrystalsUnlocked = 0;
		int leftMostLockIndex = m_permanentCrystals.FindLastIndex((ManaCrystal crystal) => crystal.IsOverloaded());
		for (; numCrystalsUnlocked < numCrystals; numCrystalsUnlocked++)
		{
			if (leftMostLockIndex < 0)
			{
				break;
			}
			m_permanentCrystals[leftMostLockIndex].UnlockOverload();
			leftMostLockIndex--;
		}
		m_additionalOverloadedCrystalsOwedThisTurn -= numCrystals - numCrystalsUnlocked;
		m_overloadLocksAreShowing = leftMostLockIndex >= 0 || m_additionalOverloadedCrystalsOwedThisTurn > 0;
	}

	public void TurnCrystalsRed(int previous, int current)
	{
		for (int i = previous; i < current && i < m_permanentCrystals.Count; i++)
		{
			m_permanentCrystals[i].gem.gameObject.GetComponent<Renderer>().GetMaterial().mainTexture = redCrystalTexture;
		}
	}

	public void OnCurrentPlayerChanged()
	{
		m_additionalOverloadedCrystalsOwedThisTurn = m_additionalOverloadedCrystalsOwedNextTurn;
		m_additionalOverloadedCrystalsOwedNextTurn = 0;
		if (m_additionalOverloadedCrystalsOwedThisTurn > 0)
		{
			m_overloadLocksAreShowing = true;
		}
		else
		{
			m_overloadLocksAreShowing = false;
		}
		for (int crystalIndex = 0; crystalIndex < m_permanentCrystals.Count; crystalIndex++)
		{
			ManaCrystal crystal = m_permanentCrystals[crystalIndex];
			if (crystal.IsOverloaded())
			{
				crystal.UnlockOverload();
			}
			if (crystal.IsOwedForOverload())
			{
				m_overloadLocksAreShowing = true;
				crystal.PayOverload();
			}
			else if (m_additionalOverloadedCrystalsOwedThisTurn > 0)
			{
				crystal.PayOverload();
				m_additionalOverloadedCrystalsOwedThisTurn--;
			}
		}
	}

	public bool ShouldShowTooltip(ManaCrystalType type)
	{
		return m_manaCrystalType == type;
	}

	public bool ShouldShowOverloadTooltip()
	{
		return m_overloadLocksAreShowing;
	}

	public void SetFriendlyManaGemTexture(AssetHandle<Texture> texture)
	{
		AssetHandle.Set(ref m_friendlyManaGemTexture, texture);
		ApplyFriendlyManaGemTexture();
	}

	public void SetFriendlyManaGemTint(Color tint)
	{
		if (!(m_friendlyManaGem == null))
		{
			m_friendlyManaGem.GetComponentInChildren<MeshRenderer>().GetMaterial().SetColor("_TintColor", tint);
		}
	}

	public void ShowPhoneManaTray()
	{
		if (!(manaTrayPhone == null) && !m_disableManaCrystalVisuals)
		{
			Animation component = m_friendlyManaGem.GetComponent<Animation>();
			component[GEM_FLIP_ANIM_NAME].speed = 1f;
			component.Play(GEM_FLIP_ANIM_NAME);
			iTween.ValueTo(base.gameObject, iTween.Hash("from", m_friendlyManaText.TextAlpha, "to", 0f, "time", 0.1f, "onupdate", (Action<object>)delegate(object newVal)
			{
				m_friendlyManaText.TextAlpha = (float)newVal;
			}));
			manaTrayPhone.ToggleTraySlider(show: true);
			CorpseCounter.ShowPhoneManaTray();
		}
	}

	public void HidePhoneManaTray()
	{
		if (!(manaTrayPhone == null))
		{
			Animation friendlyManaGemAnimation = m_friendlyManaGem.GetComponent<Animation>();
			friendlyManaGemAnimation[GEM_FLIP_ANIM_NAME].speed = -1f;
			if (friendlyManaGemAnimation[GEM_FLIP_ANIM_NAME].time == 0f)
			{
				friendlyManaGemAnimation[GEM_FLIP_ANIM_NAME].time = friendlyManaGemAnimation[GEM_FLIP_ANIM_NAME].length;
			}
			friendlyManaGemAnimation.Play(GEM_FLIP_ANIM_NAME);
			iTween.ValueTo(base.gameObject, iTween.Hash("from", m_friendlyManaText.TextAlpha, "to", 1f, "time", 0.1f, "onupdate", (Action<object>)delegate(object newVal)
			{
				m_friendlyManaText.TextAlpha = (float)newVal;
			}));
			manaTrayPhone.ToggleTraySlider(show: false);
			CorpseCounter.HidePhoneManaTray();
		}
	}

	public Material GetTemporaryManaCrystalMaterial()
	{
		return m_ManaCrystalAssetTable[(int)m_manaCrystalType].m_tempManaCrystalMaterial;
	}

	public Material GetTemporaryManaCrystalProposedQuadMaterial()
	{
		return m_ManaCrystalAssetTable[(int)m_manaCrystalType].m_tempManaCrystalProposedQuadMaterial;
	}

	public void SetEnemyManaCounterActive(bool active)
	{
		opposingManaCounter.GetComponent<ManaCounter>().enabled = active;
		if ((bool)UniversalInputManager.UsePhoneUI && !GameMgr.Get().IsBattlegrounds() && !GameMgr.Get().IsBattlegroundsTutorial())
		{
			UberText opposingManaText = opposingManaCounter.GetComponent<UberText>();
			if (opposingManaText != null)
			{
				opposingManaText.TextAlpha = 0f;
				opposingManaText.UpdateNow();
				opposingManaText.enabled = false;
			}
		}
		else
		{
			opposingManaCounter.SetActive(active);
		}
	}

	public void SetFriendlyManaCounterActive(bool active)
	{
		ManaCounter manaCounter = friendlyManaCounter.GetComponent<ManaCounter>();
		if (manaCounter != null)
		{
			manaCounter.enabled = active;
			manaCounter.ToggleManaCrystalTextUpdate(active);
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			UberText friendlyManaText = friendlyManaCounter.GetComponent<UberText>();
			if (friendlyManaText != null)
			{
				friendlyManaText.TextAlpha = 0f;
				friendlyManaText.UpdateNow();
				friendlyManaText.enabled = false;
			}
		}
		else
		{
			friendlyManaCounter.SetActive(active);
		}
	}

	public void DisableManaCountDisplay()
	{
		m_disableManaCrystalVisuals = true;
		if ((bool)UniversalInputManager.UsePhoneUI && manaTrayPhone != null)
		{
			manaTrayPhone.gameObject.SetActive(value: false);
		}
		SetFriendlyManaCounterActive(active: false);
		SetEnemyManaCounterActive(active: false);
	}

	private void UpdateLayout()
	{
		Vector3 newPos = base.transform.position;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			newPos = manaGemBone.transform.position;
		}
		int currManaCrystalCount = 0;
		for (int i = m_permanentCrystals.Count - 1; i >= 0; i--)
		{
			m_permanentCrystals[i].Show();
			if (currManaCrystalCount >= maxManaCrystalToDisplay)
			{
				m_permanentCrystals[i].Hide();
			}
			else
			{
				m_permanentCrystals[i].transform.position = newPos;
				if ((bool)UniversalInputManager.UsePhoneUI)
				{
					newPos.z += m_manaCrystalWidth;
				}
				else
				{
					newPos.x += m_manaCrystalWidth;
				}
				currManaCrystalCount++;
			}
		}
		for (int j = 0; j < m_temporaryCrystals.Count; j++)
		{
			m_temporaryCrystals[j].Show();
			if (m_permanentCrystals.Count + j >= maxManaCrystalToDisplay)
			{
				m_temporaryCrystals[j].Hide();
				continue;
			}
			m_temporaryCrystals[j].transform.position = newPos;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				newPos.z += m_manaCrystalWidth;
			}
			else
			{
				newPos.x += m_manaCrystalWidth;
			}
		}
	}

	private IEnumerator UpdatePermanentCrystalStates()
	{
		while (m_numQueuedToReady > 0 || m_numCrystalsLoading > 0 || m_numQueuedToSpend > 0)
		{
			yield return null;
		}
		int numUsedCrystals = GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.RESOURCES_USED);
		int overloadOwedCrystals = GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.OVERLOAD_OWED);
		int i;
		for (i = 0; i < numUsedCrystals && i != m_permanentCrystals.Count; i++)
		{
			if (m_permanentCrystals[i].state != ManaCrystal.State.USED)
			{
				m_permanentCrystals[i].state = ManaCrystal.State.USED;
			}
		}
		for (int j = i; j < m_permanentCrystals.Count; j++)
		{
			if (m_permanentCrystals[j].state != 0)
			{
				m_permanentCrystals[j].state = ManaCrystal.State.READY;
			}
		}
		for (int k = 0; k < Math.Min(m_permanentCrystals.Count, overloadOwedCrystals); k++)
		{
			if (!m_permanentCrystals[k].IsOwedForOverload())
			{
				m_permanentCrystals[k].MarkAsOwedForOverload();
			}
		}
		if (m_permanentCrystals.Count == 0 && overloadOwedCrystals > 0)
		{
			m_additionalOverloadedCrystalsOwedNextTurn = overloadOwedCrystals;
		}
	}

	private void LoadCrystalCallback(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_numCrystalsLoading--;
		if (m_manaCrystalWidth <= 0f)
		{
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				m_manaCrystalWidth = 0.33f;
			}
			else
			{
				m_manaCrystalWidth = go.transform.Find("Gem_Mana").GetComponent<Renderer>().bounds.size.x;
			}
		}
		LoadCrystalCallbackData loadCrystalCallbackData = callbackData as LoadCrystalCallbackData;
		ManaCrystal crystal = go.GetComponent<ManaCrystal>();
		if (loadCrystalCallbackData.IsTempCrystal)
		{
			crystal.MarkAsTemp();
			m_temporaryCrystals.Add(crystal);
		}
		else
		{
			m_permanentCrystals.Add(crystal);
			if (loadCrystalCallbackData.IsTurnStart)
			{
				if (m_additionalOverloadedCrystalsOwedThisTurn > 0)
				{
					crystal.PayOverload();
					m_additionalOverloadedCrystalsOwedThisTurn--;
				}
			}
			else if (m_additionalOverloadedCrystalsOwedNextTurn > 0)
			{
				crystal.state = ManaCrystal.State.USED;
				crystal.MarkAsOwedForOverload();
				m_additionalOverloadedCrystalsOwedNextTurn--;
			}
			StartCoroutine(UpdatePermanentCrystalStates());
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			crystal.transform.parent = manaGemBone.transform.parent;
			crystal.transform.localRotation = manaGemBone.transform.localRotation;
			crystal.transform.localScale = manaGemBone.transform.localScale;
		}
		else
		{
			crystal.transform.parent = base.transform;
		}
		crystal.transform.localPosition = Vector3.zero;
		crystal.PlayCreateAnimation();
		ManaCrystalAssetPaths assetPaths = GetManaCrystalAssetPaths(m_manaCrystalType);
		SoundManager.Get().LoadAndPlay(assetPaths.m_SoundOnAddPath, base.gameObject);
		UpdateLayout();
	}

	public float GetWidth()
	{
		if (m_permanentCrystals.Count == 0)
		{
			return 0f;
		}
		return m_permanentCrystals[0].transform.Find("Gem_Mana").GetComponent<Renderer>().bounds.size.x * (float)m_permanentCrystals.Count * (float)m_temporaryCrystals.Count;
	}

	public ManaCrystalAssetPaths GetManaCrystalAssetPaths(ManaCrystalType type)
	{
		foreach (ManaCrystalAssetPaths assetPaths in m_ManaCrystalAssetTable)
		{
			if (assetPaths.m_Type == type)
			{
				return assetPaths;
			}
		}
		return m_ManaCrystalAssetTable[0];
	}

	private IEnumerator WaitThenAddManaCrystal(bool isTemp, bool isTurnStart)
	{
		m_numCrystalsLoading++;
		m_numQueuedToSpawn++;
		yield return new WaitForSeconds((float)m_numQueuedToSpawn * 0.2f);
		ManaCrystalAssetPaths assetPaths = GetManaCrystalAssetPaths(m_manaCrystalType);
		LoadCrystalCallbackData callbackData = new LoadCrystalCallbackData(isTemp, isTurnStart);
		AssetLoader.Get().InstantiatePrefab(assetPaths.m_ResourcePath, LoadCrystalCallback, callbackData, AssetLoadingOptions.IgnorePrefabPosition);
		m_numQueuedToSpawn--;
	}

	private IEnumerator WaitThenDestroyManaCrystals(bool isTemp, int numCrystals)
	{
		while (m_numCrystalsLoading > 0)
		{
			yield return null;
		}
		for (int i = 0; i < numCrystals; i++)
		{
			if (isTemp)
			{
				DestroyTempManaCrystal();
			}
			else
			{
				DestroyManaCrystal();
			}
		}
	}

	private IEnumerator WaitThenReadyManaCrystal()
	{
		m_numQueuedToReady++;
		yield return new WaitForSeconds((float)m_numQueuedToReady * 0.2f);
		if (m_numQueuedToReady <= 0)
		{
			yield break;
		}
		if (m_permanentCrystals.Count > 0)
		{
			for (int i = m_permanentCrystals.Count - 1; i >= 0; i--)
			{
				if (m_permanentCrystals[i].state == ManaCrystal.State.USED)
				{
					ManaCrystalAssetPaths assetPaths = GetManaCrystalAssetPaths(m_manaCrystalType);
					SoundManager.Get().LoadAndPlay(assetPaths.m_SoundOnRefreshPath, base.gameObject);
					m_permanentCrystals[i].state = ManaCrystal.State.READY;
					break;
				}
			}
		}
		m_numQueuedToReady--;
	}

	private IEnumerator WaitThenSpendManaCrystal()
	{
		m_numQueuedToSpend++;
		yield return new WaitForSeconds((float)(m_numQueuedToSpend - 1) * 0.2f);
		if (m_numQueuedToSpend <= 0)
		{
			yield break;
		}
		bool successfullySpentManaCrystal = false;
		for (int i = 0; i < m_permanentCrystals.Count; i++)
		{
			if (m_permanentCrystals[i].state != ManaCrystal.State.USED)
			{
				m_permanentCrystals[i].state = ManaCrystal.State.USED;
				successfullySpentManaCrystal = true;
				break;
			}
		}
		if (!successfullySpentManaCrystal)
		{
			m_numQueuedToReady--;
		}
		m_numQueuedToSpend--;
		if (m_numQueuedToSpend <= 0)
		{
			InputManager.Get().OnManaCrystalMgrManaSpent();
		}
	}

	private void DestroyManaCrystal()
	{
		if (m_permanentCrystals.Count > 0)
		{
			int indexToDestroy = 0;
			ManaCrystal manaCrystal = m_permanentCrystals[indexToDestroy];
			m_permanentCrystals.RemoveAt(indexToDestroy);
			manaCrystal.GetComponent<ManaCrystal>().Destroy();
			UpdateLayout();
			StartCoroutine(UpdatePermanentCrystalStates());
		}
	}

	private void DestroyTempManaCrystal()
	{
		if (m_temporaryCrystals.Count > 0)
		{
			int indexToDestroy = m_temporaryCrystals.Count - 1;
			ManaCrystal manaCrystal = m_temporaryCrystals[indexToDestroy];
			m_temporaryCrystals.RemoveAt(indexToDestroy);
			manaCrystal.GetComponent<ManaCrystal>().Destroy();
			UpdateLayout();
		}
	}

	private void SpendManaCrystal()
	{
		StartCoroutine(WaitThenSpendManaCrystal());
	}

	private void ReadyManaCrystal()
	{
		StartCoroutine(WaitThenReadyManaCrystal());
	}

	private void InitializePhoneManaGems()
	{
		if ((bool)UniversalInputManager.UsePhoneUI && !m_disableManaCrystalVisuals)
		{
			m_friendlyManaText = friendlyManaCounter.GetComponent<UberText>();
			ManaCounter manaCounter = friendlyManaCounter.GetComponent<ManaCounter>();
			string resourcePath = m_ManaCrystalAssetTable[(int)m_manaCrystalType].m_phoneLargeResource;
			manaCounter.InitializeLargeResourceGameObject(resourcePath);
			if (opposingManaCounter.activeInHierarchy)
			{
				opposingManaCounter.GetComponent<ManaCounter>().InitializeLargeResourceGameObject(resourcePath);
			}
			m_friendlyManaGem = manaCounter.GetPhoneGem();
			ApplyFriendlyManaGemTexture();
		}
	}

	private void ApplyFriendlyManaGemTexture()
	{
		if (!(m_friendlyManaGem == null) && m_friendlyManaGemTexture != null)
		{
			m_friendlyManaGem.GetComponentInChildren<MeshRenderer>().GetMaterial().mainTexture = m_friendlyManaGemTexture;
		}
	}
}
