using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneManager : MonoBehaviour
{
	public enum CutsceneAntiAliasingLevel
	{
		OFF = 1,
		TWO = 2,
		FOUR = 4,
		EIGHT = 8
	}

	private static CutsceneManager s_instance;

	[SerializeField]
	[Header("Manager Config")]
	[Min(1f)]
	[Tooltip("Time (seconds) to wait with no active cutscene data before unloading the cutscene (Min 1sec to avoid race conditions)")]
	private float m_managerWaitToDestroyTimerSeconds = 10f;

	[SerializeField]
	private Camera m_RTTCamera;

	[SerializeField]
	private int m_renderResolution = 512;

	[SerializeField]
	private CutsceneAntiAliasingLevel m_antiAliasingLevel = CutsceneAntiAliasingLevel.FOUR;

	[Header("Scene Setup/Core")]
	[SerializeField]
	private Transform m_rootTransform;

	[SerializeField]
	private CutsceneTimeline m_timeline;

	[Header("Scene Setup/Heroes")]
	[SerializeField]
	private CutsceneCardLoader m_friendlyHeroCardLoader;

	[SerializeField]
	private CutsceneCardLoader m_friendlyHeroPowerCardLoader;

	[SerializeField]
	private CutsceneCardLoader m_opponentHeroCardLoader;

	[SerializeField]
	private CutsceneCardLoader m_opponentHeroPowerCardLoader;

	[Header("Alternate Form Hero")]
	[SerializeField]
	private CutsceneCardLoader m_alternateFormHeroCardLoader;

	[Header("Scene Setup/Minions")]
	[SerializeField]
	private List<CutsceneCardLoader> m_friendlyMinionCardLoaders;

	[SerializeField]
	private List<CutsceneCardLoader> m_opponentMinionCardLoaders;

	private RenderTexture m_renderTexture;

	private CutsceneSceneDef m_loadedSceneDef;

	private GameObject m_loadedBoardCache;

	private Coroutine m_managerTimeToLiveCoroutine;

	private CornerSpellReplacementManager m_cornerSpellReplacementManager = new CornerSpellReplacementManager(isInShop: true);

	public static CutsceneManager Get()
	{
		return s_instance;
	}

	private void Awake()
	{
		if (s_instance != null)
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		s_instance = this;
		InitializeRenderTexture();
	}

	private void OnDestroy()
	{
		s_instance = null;
		if (m_RTTCamera != null)
		{
			m_RTTCamera.targetTexture = null;
		}
		if (m_renderTexture != null)
		{
			UnityEngine.Object.Destroy(m_renderTexture);
			m_renderTexture = null;
		}
		if (m_managerTimeToLiveCoroutine != null)
		{
			StopCoroutine(m_managerTimeToLiveCoroutine);
			m_managerTimeToLiveCoroutine = null;
		}
		CleanUp();
	}

	public RenderTexture GetTexture()
	{
		return m_renderTexture;
	}

	public void Play(CutsceneSceneDef sceneDef, bool forceLoad = false)
	{
		if (m_loadedSceneDef != null)
		{
			m_loadedSceneDef.SceneDefDestroyed -= OnLoadedSceneDefDestroyed;
		}
		if (!(sceneDef == null))
		{
			LoadSceneIfNeeded(sceneDef, forceLoad);
			m_timeline.PlayTimeline();
			SafeSetCameraEnable(m_timeline.IsRunning);
		}
	}

	public bool IsTimelineRunning()
	{
		if (m_timeline == null)
		{
			return false;
		}
		return m_timeline.IsRunning;
	}

	public Card GetHeroCard(Player.Side side)
	{
		CutsceneCardLoader loader = ((side == Player.Side.FRIENDLY) ? m_friendlyHeroCardLoader : m_opponentHeroCardLoader);
		if (loader == null || loader.GetActor() == null)
		{
			return null;
		}
		return loader.GetActor().GetCard();
	}

	public void Stop()
	{
		m_timeline.StopTimeline();
		CleanUp();
		SafeSetCameraEnable(isEnabled: false);
	}

	private void OnLoadedSceneDefDestroyed()
	{
		if (m_managerTimeToLiveCoroutine != null)
		{
			StopCoroutine(m_managerTimeToLiveCoroutine);
		}
		m_managerTimeToLiveCoroutine = StartCoroutine(ManagerTimeToLiveRunner(Math.Max(m_managerWaitToDestroyTimerSeconds, 1f)));
	}

	private IEnumerator ManagerTimeToLiveRunner(float waitTimeSec)
	{
		if (waitTimeSec > 0f)
		{
			float elapsedTime = 0f;
			while (elapsedTime < waitTimeSec)
			{
				elapsedTime += Time.deltaTime;
				yield return null;
				if (m_loadedSceneDef != null)
				{
					m_managerTimeToLiveCoroutine = null;
					yield break;
				}
			}
		}
		m_managerTimeToLiveCoroutine = null;
		if (!(m_loadedSceneDef != null))
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void InitializeRenderTexture()
	{
		SafeSetCameraEnable(isEnabled: false);
		if (m_renderTexture != null)
		{
			return;
		}
		int renderResolution = Mathf.NextPowerOfTwo(m_renderResolution);
		int antiAliasingLevel;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			if (m_renderResolution > 512)
			{
				renderResolution = Math.Max(512, renderResolution / 2);
			}
			antiAliasingLevel = 1;
		}
		else
		{
			antiAliasingLevel = (int)m_antiAliasingLevel;
		}
		m_renderTexture = new RenderTexture(renderResolution, renderResolution, 24, RenderTextureFormat.ARGB32)
		{
			antiAliasing = antiAliasingLevel,
			filterMode = FilterMode.Bilinear
		};
		m_RTTCamera.targetTexture = m_renderTexture;
	}

	private void SafeSetCameraEnable(bool isEnabled)
	{
		if (m_RTTCamera != null)
		{
			m_RTTCamera.enabled = isEnabled;
		}
	}

	private void LoadSceneIfNeeded(CutsceneSceneDef sceneDef, bool forceLoad)
	{
		if (sceneDef == null)
		{
			Log.CosmeticPreview.PrintError("[CutsceneManager] Failed to load cutscene as provided cutscene def was null!");
		}
		else if (string.IsNullOrEmpty(sceneDef.SetupData?.Board))
		{
			Log.CosmeticPreview.PrintError("[CutsceneManager] Failed to load cutscene as " + sceneDef.name + " couldn't be initialized!");
		}
		else if (!(sceneDef == m_loadedSceneDef) || forceLoad)
		{
			CleanUp();
			CreateBoard(sceneDef.SetupData.Board);
			UpdateCornerReplacements(sceneDef.SetupData);
			CutsceneLoadedActors sceneActors = LoadBoardElements(sceneDef.SetupData);
			LoadActionsToTimeline(sceneDef.Actions, sceneActors);
			m_loadedSceneDef = sceneDef;
			m_loadedSceneDef.SceneDefDestroyed += OnLoadedSceneDefDestroyed;
		}
	}

	private void CleanUp(bool includeBoard = true)
	{
		if (m_loadedSceneDef != null)
		{
			m_loadedSceneDef.SceneDefDestroyed -= OnLoadedSceneDefDestroyed;
		}
		m_timeline.Dispose();
		if (includeBoard && m_loadedBoardCache != null)
		{
			UnityEngine.Object.Destroy(m_loadedBoardCache);
			m_loadedBoardCache = null;
		}
		m_friendlyHeroCardLoader.Dispose();
		m_alternateFormHeroCardLoader.Dispose();
		m_friendlyHeroPowerCardLoader.Dispose();
		m_opponentHeroCardLoader.Dispose();
		m_opponentHeroPowerCardLoader.Dispose();
		foreach (CutsceneCardLoader friendlyMinionCardLoader in m_friendlyMinionCardLoaders)
		{
			friendlyMinionCardLoader.Dispose();
		}
		foreach (CutsceneCardLoader opponentMinionCardLoader in m_opponentMinionCardLoaders)
		{
			opponentMinionCardLoader.Dispose();
		}
		if (m_cornerSpellReplacementManager != null)
		{
			m_cornerSpellReplacementManager.UpdateCornerReplacements();
		}
		m_loadedSceneDef = null;
	}

	private void CreateBoard(string boardAssetRef)
	{
		GameObject boardObject = AssetLoader.Get().InstantiatePrefab(boardAssetRef);
		if (boardObject == null)
		{
			Log.CosmeticPreview.PrintWarning("[CutsceneManager] Fail to load board asset!");
			return;
		}
		boardObject.transform.SetParent(m_rootTransform, worldPositionStays: false);
		m_loadedBoardCache = boardObject;
	}

	private CutsceneLoadedActors LoadBoardElements(CutsceneSceneDef.CutsceneSetupData setupData)
	{
		CutsceneLoadedActors sceneActors = default(CutsceneLoadedActors);
		if (!string.IsNullOrEmpty(setupData.FriendlyHeroCardId))
		{
			m_friendlyHeroCardLoader.SetAndLoadCard(setupData.FriendlyHeroCardId, CutsceneSceneDef.CardType.HERO, Player.Side.FRIENDLY);
			sceneActors.FriendlyHeroActor = m_friendlyHeroCardLoader.GetActor();
			m_friendlyHeroCardLoader.SetHeroScale(setupData.FriendlyHeroScale);
			m_friendlyHeroPowerCardLoader.SetAndLoadCard(string.Empty, CutsceneSceneDef.CardType.HERO_POWER, Player.Side.FRIENDLY, sceneActors.FriendlyHeroActor);
			sceneActors.FriendlyHeroPowerActor = m_friendlyHeroPowerCardLoader.GetActor();
			if (!setupData.FriendlyHeroPowerEnabled)
			{
				sceneActors.FriendlyHeroPowerActor.Hide();
			}
		}
		if (setupData.HasAlternateForm && !string.IsNullOrEmpty(setupData.AlternateHeroCardId))
		{
			m_alternateFormHeroCardLoader.SetAndLoadCard(setupData.AlternateHeroCardId, CutsceneSceneDef.CardType.ALTERNATE_FORM, Player.Side.FRIENDLY);
			sceneActors.FriendlyAlternateFormHeroActor = m_alternateFormHeroCardLoader.GetActor();
			m_alternateFormHeroCardLoader.SetHeroScale(setupData.FriendlyHeroScale);
			sceneActors.FriendlyAlternateFormHeroActor.SetVisibility(isVisible: false, isInternal: false);
		}
		if (!string.IsNullOrEmpty(setupData.OpponentHeroCardId))
		{
			m_opponentHeroCardLoader.SetAndLoadCard(setupData.OpponentHeroCardId, CutsceneSceneDef.CardType.HERO, Player.Side.OPPOSING);
			sceneActors.OpponentHeroActor = m_opponentHeroCardLoader.GetActor();
			m_opponentHeroPowerCardLoader.SetAndLoadCard(string.Empty, CutsceneSceneDef.CardType.HERO_POWER, Player.Side.OPPOSING, sceneActors.OpponentHeroActor);
			sceneActors.OpponentHeroPowerActor = m_opponentHeroPowerCardLoader.GetActor();
		}
		if (setupData.FriendlyMinionCount > 0)
		{
			sceneActors.FriendlyMinions = CreateMinionsFromRequest(setupData.FriendlyMinionCount, setupData.MinionCardId, m_friendlyMinionCardLoaders, sceneActors.FriendlyHeroActor);
		}
		if (setupData.OpponentMinionCount > 0)
		{
			sceneActors.OpponentMinions = CreateMinionsFromRequest(setupData.OpponentMinionCount, setupData.MinionCardId, m_opponentMinionCardLoaders, sceneActors.OpponentHeroActor);
		}
		return sceneActors;
	}

	private void LoadActionsToTimeline(List<CutsceneSceneDef.CutsceneActionRequest> actionRequests, CutsceneLoadedActors sceneActors)
	{
		if (actionRequests.Count > 0)
		{
			m_timeline.CreateTimeline(actionRequests, in sceneActors);
		}
	}

	private List<Actor> CreateMinionsFromRequest(int numberToCreate, string minionId, List<CutsceneCardLoader> loaders, Actor heroOwnerActor)
	{
		if (loaders == null)
		{
			Log.CosmeticPreview.PrintError("Failed to initialize cutscene minions are card loader list was null!");
			return null;
		}
		if (numberToCreate > loaders.Count)
		{
			Log.CosmeticPreview.PrintWarning(string.Format("Invalid setup for {0}! Requested {1} minions but can only support {2}!", "CutsceneSceneDef", numberToCreate, loaders.Count));
			numberToCreate = loaders.Count;
		}
		if (numberToCreate == 0)
		{
			Log.CosmeticPreview.PrintWarning("Invalid request to create minions for cutscene - requested count was 0.");
			return null;
		}
		List<Actor> createdActors = new List<Actor>();
		for (int i = 0; i < numberToCreate; i++)
		{
			CutsceneCardLoader loader = loaders[i];
			if (loader == null)
			{
				Log.CosmeticPreview.PrintError("Invalid setup for CutsceneManager - minion loader was null!");
				continue;
			}
			loader.SetAndLoadCard(minionId, CutsceneSceneDef.CardType.MINION, Player.Side.FRIENDLY, heroOwnerActor);
			createdActors.Add(loader.GetActor());
		}
		return createdActors;
	}

	public void UpdateCornerReplacement(CornerReplacementSpellType friendlyCornerSpell, CornerReplacementSpellType opposingCornerSpell)
	{
		if (m_cornerSpellReplacementManager != null)
		{
			m_cornerSpellReplacementManager.UpdateCornerReplacements(friendlyCornerSpell, opposingCornerSpell);
			UpdateCornerSpellPositions();
		}
	}

	private void UpdateCornerReplacements(CutsceneSceneDef.CutsceneSetupData cutsceneSetup)
	{
		if (m_cornerSpellReplacementManager != null)
		{
			m_cornerSpellReplacementManager.UpdateCornerReplacements(cutsceneSetup.FriendlyCornerReplacement, cutsceneSetup.OpposingCornerReplacement);
			UpdateCornerSpellPositions();
		}
	}

	private void UpdateCornerSpellPositions()
	{
		if (m_cornerSpellReplacementManager == null)
		{
			return;
		}
		Spell[] cornerSpells = m_cornerSpellReplacementManager.GetCornerSpells();
		if (cornerSpells == null || cornerSpells.Length == 0 || CutsceneBoard.Get() == null)
		{
			return;
		}
		Spell[] array = cornerSpells;
		foreach (Spell spell in array)
		{
			if (!(spell == null))
			{
				spell.transform.SetParent(CutsceneBoard.Get().transform);
				spell.transform.localPosition = default(Vector3);
			}
		}
	}
}
