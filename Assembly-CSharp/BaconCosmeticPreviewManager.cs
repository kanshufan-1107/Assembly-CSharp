using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.AssetManager;
using Blizzard.T5.Core.Time;
using UnityEngine;

public class BaconCosmeticPreviewManager : MonoBehaviour
{
	private class BoardSkin
	{
		public GameObject m_CombatPrefab;

		public GameObject m_TavernPrefab;

		public AssetHandle<GameObject> m_AssetHandleCombat;

		public AssetHandle<GameObject> m_AssetHandleTavern;

		public BaconBoardSkinBehaviour m_CombatInstance;

		public BaconBoardSkinBehaviour m_TavernInstance;
	}

	public Camera m_mainCamera;

	public Camera[] m_cameras;

	public GameObject m_root;

	public GameObject m_boardBase;

	public GameObject m_boardBasePhone;

	public UberText m_displayText;

	public GameObject m_friendlyHeroHolder;

	public GameObject m_opposingHeroHolder;

	public Actor m_friendlyHeroActor;

	public Actor m_opposingHeroActor;

	private BoardSkin m_boardSkin = new BoardSkin();

	private FinisherGameplaySettings m_strikeSettings;

	private Spell m_currentFinisher;

	private Spell m_currentShatter;

	private int m_pendingLoads;

	private BaconCosmeticPreviewRunnerConfig m_config;

	private int m_currentAction;

	private int m_blockingActionFunctionsInProgress;

	private Coroutine m_loadCosmeticInfoCoroutine;

	private Coroutine m_runPreviewCoroutine;

	private GameObject m_currentCosmeticContainer;

	private readonly List<GameObject> m_instatiatedObjects = new List<GameObject>();

	private SpellHandleValueRange[] m_ImpactDefs;

	private string m_DefaultImpactSpellPrefab;

	private const string ATTACK_SPELL_CONTROLLER_PREFAB_PATH = "AttackSpellController_Battlegrounds_Hero.prefab:922da2c91f4cca1458b5901204d1d26c";

	private const string DEFAULT_SHATTER_SPELL = "Bacon_EndRound_HeroImpact.prefab:34d052b6989dcea4c8b7d22adcb31368";

	private const float STRIKE_PREVIEW_SCALE_FACTOR = 0.88f;

	private Vector3 m_friendlyHeroHolderInitialPosition;

	private Vector3 m_friendlyHeroActorInitialPosition;

	private Vector3 m_opposingHeroHolderInitialPosition;

	private Vector3 m_opposingHeroActorInitialPosition;

	private void Awake()
	{
		m_config = null;
		BaconCosmeticPreviewTester.RunCosmetic += RunLoadCosmeticInfo;
		BaconCosmeticPreviewTester.StopCosmetic += StopCosmeticAnimation;
		m_displayText.Hide();
		SetCamerasEnabled(enabled: false);
		m_friendlyHeroHolderInitialPosition = m_friendlyHeroHolder.transform.localPosition;
		m_friendlyHeroActorInitialPosition = m_friendlyHeroActor.transform.localPosition;
		m_opposingHeroHolderInitialPosition = m_opposingHeroHolder.transform.localPosition;
		m_opposingHeroActorInitialPosition = m_opposingHeroActor.transform.localPosition;
	}

	public void Start()
	{
		AttackSpellController spellController = AssetLoader.Get().InstantiatePrefab("AttackSpellController_Battlegrounds_Hero.prefab:922da2c91f4cca1458b5901204d1d26c").GetComponent<AttackSpellController>();
		m_ImpactDefs = spellController.m_ImpactDefHandles;
		m_DefaultImpactSpellPrefab = spellController.m_DefaultImpactSpellPrefabHandle;
	}

	private void RunLoadCosmeticInfo()
	{
		if (base.gameObject != null)
		{
			SafeStopCoroutine(ref m_loadCosmeticInfoCoroutine);
			SafeStopCoroutine(ref m_runPreviewCoroutine);
			m_loadCosmeticInfoCoroutine = StartCoroutine(LoadCosmeticInfo(startRunningWhenLoaded: true));
		}
	}

	private IEnumerator LoadCosmeticInfo(bool startRunningWhenLoaded)
	{
		if (m_currentCosmeticContainer == null)
		{
			m_currentCosmeticContainer = new GameObject();
			m_currentCosmeticContainer.name = "currentDisplayedCosmetic";
			m_currentCosmeticContainer.transform.parent = m_boardBase.transform;
			m_currentCosmeticContainer.transform.localPosition = Vector3.zero;
		}
		while (BaconCosmeticPreviewLoadInfo.s_runnerConfig == null)
		{
			yield return null;
		}
		BaconCosmeticPreviewRunnerConfig runnerConfig = BaconCosmeticPreviewLoadInfo.s_runnerConfig;
		m_boardSkin = new BoardSkin();
		foreach (GameObject instatiatedObject in m_instatiatedObjects)
		{
			Object.Destroy(instatiatedObject);
		}
		m_instatiatedObjects.Clear();
		m_config = runnerConfig;
		m_pendingLoads = 0;
		LoadStrikeSettings(m_config.strikeId);
		LoadBoardSkin(m_config.boardId);
		LoadHero(m_config.friendlyHeroCardId, Player.Side.FRIENDLY);
		LoadHero(m_config.opposingHeroCardId, Player.Side.OPPOSING);
		if (startRunningWhenLoaded)
		{
			StartRunning();
		}
		m_loadCosmeticInfoCoroutine = null;
	}

	private void LoadHero(string heroId, Player.Side side)
	{
		if (string.IsNullOrEmpty(heroId))
		{
			if (side == Player.Side.FRIENDLY)
			{
				m_friendlyHeroActor.gameObject.SetActive(value: false);
			}
			else
			{
				m_opposingHeroActor.gameObject.SetActive(value: false);
			}
		}
		else
		{
			m_pendingLoads++;
			DefLoader.Get().LoadFullDef(heroId, OnHeroLoaded, side);
		}
	}

	private void LoadStrikeSettings(int id)
	{
		BattlegroundsFinisherDbfRecord strikeRecord = GameDbf.BattlegroundsFinisher.GetRecord(id);
		if (strikeRecord == null)
		{
			return;
		}
		AssetReference strikeReference = AssetReference.CreateFromAssetString(strikeRecord.GameplaySettings);
		if (strikeReference != null)
		{
			m_pendingLoads++;
			if (!AssetLoader.Get().LoadAsset<FinisherGameplaySettings>(strikeReference, OnStrikeLoaded))
			{
				m_pendingLoads--;
			}
		}
	}

	private void LoadBoardSkin(int id)
	{
		BattlegroundsBoardSkinDbfRecord boardRecord = GameDbf.BattlegroundsBoardSkin.GetRecord(id);
		if (boardRecord == null)
		{
			boardRecord = GameDbf.BattlegroundsBoardSkin.GetRecord(1);
		}
		string combatBoardAsset;
		string tavernBoardAsset;
		if (PlatformSettings.Screen == ScreenCategory.Phone)
		{
			combatBoardAsset = boardRecord.FullBoardPrefabPhone;
			tavernBoardAsset = boardRecord.FullTavernBoardPrefabPhone;
		}
		else
		{
			combatBoardAsset = boardRecord.FullBoardPrefab;
			tavernBoardAsset = boardRecord.FullTavernBoardPrefab;
		}
		m_pendingLoads += 2;
		if (!AssetLoader.Get().LoadAsset<GameObject>(combatBoardAsset, OnSkinLoaded, TAG_BOARD_VISUAL_STATE.COMBAT))
		{
			m_pendingLoads--;
		}
		if (!AssetLoader.Get().LoadAsset<GameObject>(tavernBoardAsset, OnSkinLoaded, TAG_BOARD_VISUAL_STATE.SHOP))
		{
			m_pendingLoads--;
		}
	}

	private void OnSkinLoaded(AssetReference assetRef, AssetHandle<GameObject> asset, object callbackData)
	{
		m_pendingLoads--;
		switch ((TAG_BOARD_VISUAL_STATE)callbackData)
		{
		case TAG_BOARD_VISUAL_STATE.COMBAT:
		{
			m_boardSkin.m_AssetHandleCombat = asset;
			m_boardSkin.m_CombatPrefab = asset.Asset;
			GameObject playerInstance2 = Object.Instantiate(m_boardSkin.m_CombatPrefab, m_currentCosmeticContainer.transform);
			m_instatiatedObjects.Add(playerInstance2);
			if (!playerInstance2.TryGetComponent<BaconBoardSkinBehaviour>(out m_boardSkin.m_CombatInstance))
			{
				Log.CosmeticPreview.PrintError("Attempting to get component BaconBoardSkinBehaviour but not found on " + playerInstance2);
			}
			else if (m_config != null)
			{
				m_boardSkin.m_CombatInstance.SetBoardState(m_config.initialState);
			}
			break;
		}
		case TAG_BOARD_VISUAL_STATE.SHOP:
		{
			m_boardSkin.m_AssetHandleTavern = asset;
			m_boardSkin.m_TavernPrefab = asset.Asset;
			GameObject playerInstance = Object.Instantiate(m_boardSkin.m_TavernPrefab, m_currentCosmeticContainer.transform);
			m_instatiatedObjects.Add(playerInstance);
			if (!playerInstance.TryGetComponent<BaconBoardSkinBehaviour>(out m_boardSkin.m_TavernInstance))
			{
				Log.CosmeticPreview.PrintError("Attempting to get component BaconBoardSkinBehaviour but not found on " + playerInstance);
			}
			else if (m_config != null)
			{
				m_boardSkin.m_TavernInstance.SetBoardState(m_config.initialState);
			}
			break;
		}
		}
	}

	private void OnHeroLoaded(string cardId, DefLoader.DisposableFullDef fullDef, object callbackData)
	{
		m_pendingLoads--;
		if ((Player.Side)callbackData == Player.Side.FRIENDLY)
		{
			DisableHeroAttack(m_friendlyHeroActor);
			m_friendlyHeroActor.SetCardDef(fullDef.DisposableCardDef);
			m_friendlyHeroActor.UpdateAllComponents();
		}
		else
		{
			DisableHeroAttack(m_opposingHeroActor);
			m_opposingHeroActor.SetCardDef(fullDef.DisposableCardDef);
			m_opposingHeroActor.UpdateAllComponents();
		}
	}

	private void OnStrikeLoaded(AssetReference assetRef, AssetHandle<FinisherGameplaySettings> asset, object callbackData)
	{
		m_pendingLoads--;
		m_strikeSettings = asset.Asset;
	}

	private void StartRunning()
	{
		if (m_runPreviewCoroutine == null)
		{
			m_runPreviewCoroutine = StartCoroutine(RunPreview());
		}
	}

	private IEnumerator RunPreview()
	{
		while (m_pendingLoads > 0)
		{
			yield return null;
		}
		foreach (GameObject instatiatedObject in m_instatiatedObjects)
		{
			instatiatedObject.SetActive(value: true);
		}
		m_mainCamera.targetTexture = BaconCosmeticPreviewLoadInfo.s_RT;
		SetCamerasEnabled(enabled: true);
		while (true)
		{
			BaconCosmeticPreviewAction action = m_config.actions[m_currentAction];
			if (action.delay > 0f)
			{
				SetHeroAttack(m_friendlyHeroActor, show: false, 0);
				yield return new WaitForSeconds(action.delay);
			}
			m_displayText.Text = GameStrings.Get(action.GetDisplayText());
			m_displayText.Show();
			switch (action.actionType)
			{
			case BaconCosmeticPreviewActionType.SWAP_BOARD_STATE:
				m_boardBase.GetComponentInChildren<BaconBoard>().ChangeBoardVisualStateForPreview(action.boardState, m_boardSkin.m_CombatInstance, m_boardSkin.m_TavernInstance);
				break;
			case BaconCosmeticPreviewActionType.TRIGGER_FSM_EVENT:
				if (action.boardState == TAG_BOARD_VISUAL_STATE.COMBAT)
				{
					m_boardSkin.m_CombatInstance.DebugTriggerFSMState(action.fsmParameter);
				}
				else
				{
					m_boardSkin.m_TavernInstance.DebugTriggerFSMState(action.fsmParameter);
				}
				break;
			case BaconCosmeticPreviewActionType.LAUNCH_STRIKE:
				m_blockingActionFunctionsInProgress++;
				LoadAndLaunchStrike(action);
				break;
			}
			yield return new WaitForSeconds(action.duration);
			ResetSpellParentManipulations();
			m_currentAction = (m_currentAction + 1) % m_config.actions.Count;
		}
	}

	private void LoadAndLaunchStrike(BaconCosmeticPreviewAction action)
	{
		if (action != null && !(m_strikeSettings == null))
		{
			string chosenPrefab = ((action.strikeLethalLevel == KeyboardFinisherSettings.LethalLevel.Lethal && !string.IsNullOrEmpty(m_strikeSettings.LethalPrefab)) ? m_strikeSettings.LethalPrefab : ((action.strikeLethalLevel == KeyboardFinisherSettings.LethalLevel.FirstPlaceVictory && !string.IsNullOrEmpty(m_strikeSettings.FirstPlaceVictoryPrefab)) ? m_strikeSettings.FirstPlaceVictoryPrefab : ((action.strikeDamageLevel != 0) ? m_strikeSettings.LargePrefab : m_strikeSettings.SmallPrefab)));
			if (string.IsNullOrEmpty(chosenPrefab))
			{
				Log.CosmeticPreview.PrintError("Tried to play an empty finisher spell prefab for finisher " + m_strikeSettings.name + ": " + action.strikeDamageLevel.ToString() + ", " + action.strikeLethalLevel);
				m_blockingActionFunctionsInProgress--;
			}
			else
			{
				AssetLoader.Get().LoadAsset<GameObject>(chosenPrefab, OnFinisherSpellLoaded, action);
			}
		}
	}

	private void OnFinisherSpellLoaded(AssetReference assetRef, AssetHandle<GameObject> asset, object callbackData)
	{
		BaconCosmeticPreviewAction action = (BaconCosmeticPreviewAction)callbackData;
		Spell spell = Object.Instantiate(asset.Asset).GetComponent<Spell>();
		spell.SetSource(m_friendlyHeroHolder);
		spell.AddTarget(m_opposingHeroHolder);
		spell.transform.SetParent(m_friendlyHeroActor.transform, worldPositionStays: false);
		m_instatiatedObjects.Add(spell.gameObject);
		spell.Location = SpellLocation.SOURCE;
		spell.AddFinishedCallback(OnFinisherFinished, action);
		SetHeroAttack(m_friendlyHeroActor, show: true, action.strikeImpactDamage);
		if (action.strikeLethalLevel != 0)
		{
			m_blockingActionFunctionsInProgress++;
			spell.AddFinishedCallback(DestroyOpposingPlayerHero, action);
		}
		SuperSpell currentFinisher = spell as SuperSpell;
		if (currentFinisher == null)
		{
			spell.Activate();
		}
		else
		{
			currentFinisher.ActivateFinisher();
		}
		m_currentFinisher = spell;
	}

	private void DestroyOpposingPlayerHero(Spell spell, object callbackData)
	{
		BaconCosmeticPreviewAction action = (BaconCosmeticPreviewAction)callbackData;
		if (!m_strikeSettings.IgnoreDestroyPrefabs)
		{
			string chosenPrefab = ((action.strikeLethalLevel != KeyboardFinisherSettings.LethalLevel.FirstPlaceVictory || string.IsNullOrEmpty(m_strikeSettings.FirstPlaceVictoryDestroyOpponentPrefab)) ? m_strikeSettings.DestroyOpponentPrefab : m_strikeSettings.FirstPlaceVictoryDestroyOpponentPrefab);
			Spell shatterSpell = (string.IsNullOrEmpty(chosenPrefab) ? SpellManager.Get().GetSpell("Bacon_EndRound_HeroImpact.prefab:34d052b6989dcea4c8b7d22adcb31368") : SpellManager.Get().GetSpell(chosenPrefab));
			shatterSpell.AddFinishedCallback(OnFinishedCallback);
			shatterSpell.SetSource(m_opposingHeroHolder);
			shatterSpell.AddTarget(m_opposingHeroHolder);
			shatterSpell.transform.parent = m_opposingHeroHolder.transform;
			shatterSpell.Activate();
			m_currentShatter = shatterSpell;
		}
	}

	private void OnFinisherFinished(Spell spell, object userData)
	{
		ActivateImpactEffects(spell, (BaconCosmeticPreviewAction)userData);
	}

	private void OnFinished(Spell spell)
	{
		m_blockingActionFunctionsInProgress--;
		if (spell != null)
		{
			Object.Destroy(spell, 5f);
		}
		ResetOpponentRenderers();
	}

	private void ResetOpponentRenderers()
	{
		if (!(m_opposingHeroActor == null))
		{
			Actor opponentActor = m_opposingHeroActor;
			if (opponentActor.m_portraitMesh != null)
			{
				opponentActor.m_portraitMesh.GetComponent<Renderer>().enabled = true;
			}
			if (opponentActor.m_healthObject != null)
			{
				opponentActor.m_healthObject.GetComponentInChildren<Renderer>().enabled = true;
			}
			DisableHeroAttack(opponentActor);
			CustomHeroFrameBehaviour heroFrame = opponentActor.GetComponentInChildren<CustomHeroFrameBehaviour>();
			if (heroFrame != null)
			{
				heroFrame.GetFrame().GetComponentInChildren<Renderer>().enabled = true;
			}
		}
	}

	private void DisableHeroAttack(Actor actor)
	{
		GemObject gem = actor.GetAttackObject();
		if (!(gem == null))
		{
			gem.ImmediatelyScaleToZero();
			gem.Hide();
		}
	}

	private void SetHeroAttack(Actor actor, bool show, int attack)
	{
		GemObject gem = actor.GetAttackObject();
		if (gem == null)
		{
			return;
		}
		if (show)
		{
			UberText atkText = gem.m_gemText;
			if (atkText != null)
			{
				atkText.SetText(attack.ToString());
			}
			gem.Show();
			gem.SetToZeroThenEnlarge();
		}
		else
		{
			gem.ScaleToZero();
		}
	}

	private void ResetSpellParentManipulations()
	{
		if (m_currentFinisher == null)
		{
			return;
		}
		PlayMakerFSM spellFSM = m_currentFinisher.GetComponent<PlayMakerFSM>();
		if (!(spellFSM == null))
		{
			GameObject rootObj = spellFSM.FsmVariables.GetFsmGameObject("RootObjectVar").Value;
			GameObject rootParent = spellFSM.FsmVariables.GetFsmGameObject("PrevParent").Value;
			if (!(rootObj == null) && !(rootParent == null))
			{
				rootObj.transform.parent = rootParent.transform;
				rootObj.transform.localPosition = Vector3.zero;
				rootObj.transform.eulerAngles = Vector3.zero;
				rootObj.transform.localScale = Vector3.one;
				m_friendlyHeroHolder.transform.localPosition = m_friendlyHeroHolderInitialPosition;
				m_friendlyHeroActor.transform.localPosition = m_friendlyHeroActorInitialPosition;
				m_opposingHeroHolder.transform.localPosition = m_opposingHeroHolderInitialPosition;
				m_opposingHeroActor.transform.localPosition = m_opposingHeroActorInitialPosition;
			}
		}
	}

	private void OnFinishedCallback(Spell spell, object userData)
	{
		OnFinished(spell);
	}

	private void ActivateImpactEffects(Spell spell, BaconCosmeticPreviewAction action)
	{
		m_blockingActionFunctionsInProgress++;
		if (!m_strikeSettings.ShowImpactEffects)
		{
			OnFinished(spell);
			return;
		}
		string impactSpellPrefab = DetermineImpactSpellPrefab(action.strikeImpactDamage);
		if (string.IsNullOrEmpty(impactSpellPrefab))
		{
			OnFinished(spell);
			return;
		}
		Spell spell2 = SpellManager.Get().GetSpell(impactSpellPrefab);
		spell2.SetSource(m_friendlyHeroHolder);
		spell2.AddTarget(m_opposingHeroHolder);
		Vector3 position = m_opposingHeroHolder.transform.position;
		spell2.SetPosition(position);
		Quaternion rotation = Quaternion.LookRotation(position - m_friendlyHeroHolder.transform.position);
		spell2.SetOrientation(rotation);
		spell2.AddStateFinishedCallback(OnImpactSpellStateFinished, spell);
		spell2.Activate();
	}

	private string DetermineImpactSpellPrefab(int impactDamage)
	{
		SpellHandleValueRange rangeToUse = SpellUtils.GetAppropriateElementAccordingToRanges(m_ImpactDefs, (SpellHandleValueRange x) => x.m_range, impactDamage);
		if (rangeToUse != null && !string.IsNullOrEmpty(rangeToUse.m_spellPrefabName))
		{
			return rangeToUse.m_spellPrefabName;
		}
		return m_DefaultImpactSpellPrefab;
	}

	private void OnImpactSpellStateFinished(Spell spell, SpellStateType prevStateType, object userData)
	{
		OnFinished((Spell)userData);
		OnFinished(spell);
	}

	private void StopCosmeticAnimation()
	{
		SafeStopCoroutine(ref m_runPreviewCoroutine);
		CameraShakeMgr.Stop(Camera.main);
		ResetSpellParentManipulations();
		SetCamerasEnabled(enabled: false);
		m_displayText.Hide();
		foreach (GameObject gameObj in m_instatiatedObjects)
		{
			if (gameObj != null)
			{
				gameObj.SetActive(value: false);
			}
		}
		TimeScaleMgr.Get().SetGameTimeScale(1f);
		if (m_currentShatter != null)
		{
			Object.Destroy(m_currentShatter.gameObject);
		}
		SuperSpell currentFinisherSuperSpell = m_currentFinisher as SuperSpell;
		if (currentFinisherSuperSpell != null)
		{
			currentFinisherSuperSpell.PurgeImmediate();
		}
		Object.Destroy(m_currentCosmeticContainer);
		m_instatiatedObjects.Clear();
		m_currentAction = 0;
		BaconCosmeticPreviewLoadInfo.s_runnerConfig = null;
	}

	private void SetCamerasEnabled(bool enabled)
	{
		if (m_cameras != null)
		{
			Camera[] cameras = m_cameras;
			for (int i = 0; i < cameras.Length; i++)
			{
				cameras[i].enabled = enabled;
			}
		}
	}

	public void OnDestroy()
	{
		BaconCosmeticPreviewTester.RunCosmetic -= RunLoadCosmeticInfo;
		BaconCosmeticPreviewTester.StopCosmetic -= StopCosmeticAnimation;
		SafeStopCoroutine(ref m_runPreviewCoroutine);
	}

	private void SafeStopCoroutine(ref Coroutine coroutine)
	{
		if (coroutine != null)
		{
			StopCoroutine(coroutine);
			coroutine = null;
		}
	}
}
