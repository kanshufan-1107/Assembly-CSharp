using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnomalyMedallion : MonoBehaviour
{
	private static UnityEvent s_initializeEvent;

	private static UnityEvent s_openAnomalyUIEvent;

	private static UnityEvent s_closeAnomalyUIEvent;

	private static bool s_shown;

	public GameObject m_symbol;

	public List<GameObject> m_anomalyBones;

	public List<GameObject> m_anomalyCards = new List<GameObject>();

	public List<GameObject> m_anomalyFSMs = new List<GameObject>();

	private bool m_open;

	private bool m_animating;

	private bool m_initialized;

	private int m_loading;

	private List<GameObject> m_loadedCards = new List<GameObject>();

	private BoxCollider m_colliderComponent;

	private PlayMakerFSM m_FSMComponent;

	private readonly string MEDALLION_SPAWN_SPELL = "TournamentFX_Impact_Anomaly_Spawn.prefab:8ced16a290da15e41a01601c673ad93b";

	static AnomalyMedallion()
	{
		if (s_initializeEvent == null)
		{
			s_initializeEvent = new UnityEvent();
		}
		if (s_openAnomalyUIEvent == null)
		{
			s_openAnomalyUIEvent = new UnityEvent();
		}
		if (s_closeAnomalyUIEvent == null)
		{
			s_closeAnomalyUIEvent = new UnityEvent();
		}
	}

	private void OnEnable()
	{
		m_FSMComponent = GetComponent<PlayMakerFSM>();
		m_colliderComponent = GetComponent<BoxCollider>();
		s_initializeEvent.AddListener(InitializeAnomalyUI);
		s_openAnomalyUIEvent.AddListener(OpenAnomalyUI);
		s_closeAnomalyUIEvent.AddListener(CloseAnomalyUI);
	}

	private void OnDisable()
	{
		m_FSMComponent = null;
		m_colliderComponent = null;
		s_initializeEvent.RemoveListener(InitializeAnomalyUI);
		s_openAnomalyUIEvent.RemoveListener(OpenAnomalyUI);
		s_closeAnomalyUIEvent.RemoveListener(CloseAnomalyUI);
	}

	private void LateUpdate()
	{
		if (m_open)
		{
			if (UniversalInputManager.Get().IsTouchMode() && !InputCollection.GetMouseButton(0))
			{
				Close();
			}
			if (!UniversalInputManager.Get().GetInputHitInfo(GameLayer.CardRaycast.LayerBit(), out var hitInfo))
			{
				Close();
			}
			if (hitInfo.transform != null && hitInfo.transform.GetComponent<AnomalyMedallion>() == null)
			{
				Close();
			}
		}
	}

	public static void Initialize()
	{
		if (s_initializeEvent != null)
		{
			s_initializeEvent.Invoke();
		}
	}

	public static void Open()
	{
		if (s_openAnomalyUIEvent != null)
		{
			s_openAnomalyUIEvent.Invoke();
		}
	}

	public static void Close()
	{
		if (s_closeAnomalyUIEvent != null)
		{
			s_closeAnomalyUIEvent.Invoke();
		}
	}

	public static bool IsShown()
	{
		return s_shown;
	}

	private void InitializeAnomalyUI()
	{
		if (m_initialized)
		{
			return;
		}
		GameState gameState = GameState.Get();
		if (gameState == null)
		{
			return;
		}
		GameEntity gameEntity = gameState.GetGameEntity();
		if (gameEntity != null)
		{
			List<int> entityIDs = new List<int>();
			int anomaly1EntID = gameEntity.GetTag(GAME_TAG.ANOMALY1);
			if (anomaly1EntID != 0)
			{
				entityIDs.Add(anomaly1EntID);
			}
			int anomaly2EntID = gameEntity.GetTag(GAME_TAG.ANOMALY2);
			if (anomaly2EntID != 0)
			{
				entityIDs.Add(anomaly2EntID);
			}
			InitializeAnomalyUI(entityIDs, showSpawnSpell: true);
		}
	}

	public void InitializeAnomalyUI(List<int> anomalyEntIDs, bool showSpawnSpell)
	{
		if (m_initialized)
		{
			return;
		}
		GameState gameState = GameState.Get();
		foreach (int entID in anomalyEntIDs)
		{
			int databaseID = GameUtils.TranslateCardIdToDbId(gameState.GetEntity(entID).GetCardId());
			LoadActor(databaseID);
		}
		s_shown = anomalyEntIDs.Count > 0;
		m_open = true;
		m_initialized = true;
		StartCoroutine(ActivateSpawnSpell(showSpawnSpell));
	}

	private IEnumerator ActivateSpawnSpell(bool showSpawnSpell)
	{
		if (showSpawnSpell && s_shown)
		{
			Spell spell = SpellManager.Get().GetSpell(MEDALLION_SPAWN_SPELL);
			spell.gameObject.transform.parent = base.gameObject.transform;
			spell.gameObject.transform.localPosition = Vector3.zero;
			spell.Activate();
			yield return new WaitForSeconds(1f);
		}
		m_symbol.SetActive(s_shown);
		if (m_colliderComponent != null)
		{
			m_colliderComponent.enabled = s_shown;
		}
	}

	private void OpenAnomalyUI()
	{
		if (!s_shown || m_animating)
		{
			return;
		}
		if (!m_open)
		{
			for (int i = 0; i < m_anomalyCards.Count && i < m_anomalyBones.Count; i++)
			{
				m_anomalyCards[i].SetActive(value: true);
				m_anomalyCards[i].transform.position = m_anomalyBones[i].transform.position;
			}
		}
		m_open = true;
	}

	private void CloseAnomalyUI()
	{
		if (s_shown && m_open)
		{
			for (int i = 0; i < m_anomalyCards.Count && i < m_anomalyBones.Count; i++)
			{
				m_anomalyCards[i].SetActive(value: false);
			}
			m_open = false;
		}
	}

	private void LoadActor(int databaseID)
	{
		m_loading++;
		AssetReference assetRef = ActorNames.GetNameWithPremiumType(ActorNames.ACTOR_ASSET.HAND_SPELL, TAG_PREMIUM.NORMAL);
		AssetLoader.Get().InstantiatePrefab(assetRef, OnActorLoaded, databaseID, AssetLoadingOptions.IgnorePrefabPosition);
	}

	private void OnActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		int databaseID = (int)callbackData;
		Actor actor = go.GetComponent<Actor>();
		using (DefLoader.DisposableFullDef entityfullDef = DefLoader.Get().GetFullDef(databaseID))
		{
			actor.SetFullDef(entityfullDef);
		}
		actor.UpdateAllComponents();
		actor.Show();
		StartCoroutine(DelayThenActivate(m_anomalyFSMs[m_loadedCards.Count]));
		go.transform.parent = m_anomalyCards[m_loadedCards.Count].transform;
		go.transform.localPosition = Vector3.zero;
		LayerUtils.SetLayer(go, GameLayer.Tooltip);
		m_loadedCards.Add(go);
		m_loading--;
	}

	private IEnumerator DelayThenActivate(GameObject anomalyFSM)
	{
		if (anomalyFSM != null)
		{
			anomalyFSM.SetActive(value: true);
			LayerUtils.SetLayer(anomalyFSM, GameLayer.Tooltip);
			yield return new WaitForSeconds(0.5f);
			PlayMakerFSM fsm = anomalyFSM.GetComponent<PlayMakerFSM>();
			if (fsm != null)
			{
				fsm.SendEvent("Tentacle");
			}
		}
	}

	public bool IsLoading()
	{
		return m_loading > 0;
	}

	public void SetAnimating(bool isAnimating)
	{
		m_animating = isAnimating;
	}

	public void SetPositionsFromBones(List<GameObject> bones)
	{
		m_anomalyBones = bones;
		for (int i = 0; i < m_anomalyCards.Count && i < m_anomalyBones.Count; i++)
		{
			m_anomalyCards[i].transform.position = m_anomalyBones[i].transform.position;
		}
	}
}
