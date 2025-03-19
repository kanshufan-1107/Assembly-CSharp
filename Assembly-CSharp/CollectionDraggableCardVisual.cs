using System.Collections.Generic;
using UnityEngine;

public class CollectionDraggableCardVisual : MonoBehaviour
{
	public enum ActorVisualMode
	{
		DECK_TILE,
		BIG_CARD,
		IN_PLAY_ZILLIAX_SIDEBOARD
	}

	public DragRotatorInfo m_CardDragRotatorInfo = new DragRotatorInfo
	{
		m_PitchInfo = new DragRotatorAxisInfo
		{
			m_ForceMultiplier = 3f,
			m_MinDegrees = -55f,
			m_MaxDegrees = 55f,
			m_RestSeconds = 2f
		},
		m_RollInfo = new DragRotatorAxisInfo
		{
			m_ForceMultiplier = 4.5f,
			m_MinDegrees = -60f,
			m_MaxDegrees = 60f,
			m_RestSeconds = 2f
		}
	};

	private static Vector3 DECK_TILE_LOCAL_SCALE;

	private static Vector3 DECK_TILE_LOCAL_POSITION = new Vector3(2.194931f, 0f, 0f);

	private static Vector3 CARD_ACTOR_LOCAL_SCALE;

	private static Vector3 HERO_SKIN_ACTOR_LOCAL_SCALE;

	private static Vector3 IN_PLAY_ACTOR_LOCAL_SCALE;

	private static Vector3 IN_PLAY_ACTOR_LOCAL_POSITION = new Vector3(2.194931f, 0f, 0f);

	private static Vector3 ACTOR_LOCAL_EULER_ANGLES = new Vector3(0f, 180f, 0f);

	private static bool s_scaleIsSetup = false;

	private CollectionDeckSlot m_slot;

	private DeckTrayDeckTileVisual m_deckTileToRemove;

	private Actor m_cardBackActor;

	private CardBack m_currentCardBack;

	private ActorVisualMode m_actorVisualMode = ActorVisualMode.BIG_CARD;

	private EntityDef m_entityDef;

	private EntityDef m_inPlayEntityDefOverride;

	private TAG_PREMIUM m_premium;

	private DefLoader.DisposableCardDef m_cardDef;

	private DefLoader.DisposableCardDef m_inPlayCardDefOverride;

	private Actor m_activeActor;

	private CollectionDeckTileActor m_deckTile;

	private Actor m_cardActor;

	private Actor m_inPlayActor;

	private HandActorCache m_actorCache = new HandActorCache();

	private bool m_actorCacheInit;

	private CollectionUtils.ViewMode m_visualType;

	private int m_cardBackId;

	private void Awake()
	{
		EnsureScaleConstantsExist();
		base.gameObject.SetActive(value: false);
		LoadDeckTile();
		LoadCardBack();
		if (base.gameObject.GetComponent<AudioSource>() == null)
		{
			base.gameObject.AddComponent<AudioSource>();
		}
	}

	private void OnDestroy()
	{
		m_cardDef?.Dispose();
		m_cardDef = null;
		m_inPlayCardDefOverride?.Dispose();
		m_inPlayCardDefOverride = null;
	}

	private void Update()
	{
		if (m_deckTileToRemove != null)
		{
			m_deckTileToRemove.SetHighlight(highlight: false);
		}
		m_deckTileToRemove = null;
		if (!(m_activeActor != m_deckTile) && CollectionManager.Get().GetEditedDeck() != null && UniversalInputManager.Get().GetInputHitInfo(DeckTrayDeckTileVisual.LAYER.LayerBit(), out var hit))
		{
			DeckTrayDeckTileVisual deckTileVisual = hit.collider.gameObject.GetComponent<DeckTrayDeckTileVisual>();
			if (!(deckTileVisual == null) && !(deckTileVisual == m_deckTileToRemove))
			{
				m_deckTileToRemove = deckTileVisual;
			}
		}
	}

	public void SetCardBackId(int cardBackId)
	{
		m_cardBackId = cardBackId;
	}

	public int GetCardBackId()
	{
		return m_cardBackId;
	}

	public string GetCardID()
	{
		return m_entityDef.GetCardId();
	}

	public TAG_PREMIUM GetPremium()
	{
		return m_premium;
	}

	public EntityDef GetEntityDef()
	{
		return m_entityDef;
	}

	public EntityDef GetCurrentlyShowingEntityDef()
	{
		if (m_actorVisualMode != ActorVisualMode.IN_PLAY_ZILLIAX_SIDEBOARD || m_inPlayEntityDefOverride == null)
		{
			return m_entityDef;
		}
		return m_inPlayEntityDefOverride;
	}

	public CollectionDeckSlot GetSlot()
	{
		return m_slot;
	}

	public void SetSlot(CollectionDeckSlot slot)
	{
		m_slot = slot;
	}

	public CollectionUtils.ViewMode GetVisualType()
	{
		return m_visualType;
	}

	public void InitActorCache()
	{
		if (!m_actorCacheInit)
		{
			m_actorCacheInit = true;
			m_actorCache.AddActorLoadedListener(OnCardActorLoaded);
			m_actorCache.Initialize();
		}
	}

	public bool ChangeActor(Actor actor, CollectionUtils.ViewMode vtype, TAG_PREMIUM premium)
	{
		InitActorCache();
		if (m_actorCache.IsInitializing())
		{
			return false;
		}
		m_visualType = vtype;
		if (m_visualType != CollectionUtils.ViewMode.CARD_BACKS)
		{
			EntityDef entityDef = actor.GetEntityDef();
			bool entityDefChanged = entityDef != m_entityDef;
			bool premiumChanged = premium != m_premium;
			if (!entityDefChanged && !premiumChanged)
			{
				return true;
			}
			m_entityDef = entityDef;
			m_premium = premium;
			m_cardActor = m_actorCache.GetActor(entityDef, premium);
			if (m_cardActor == null)
			{
				return false;
			}
			LoadInPlayActor();
			if (entityDefChanged || premiumChanged)
			{
				DefLoader.Get().LoadCardDef(m_entityDef.GetCardId(), OnCardDefLoaded, new CardPortraitQuality(1, m_premium));
			}
			else
			{
				InitDeckTileActor();
				InitCardActor();
				InitInPlayActor();
			}
			return true;
		}
		if (actor != null)
		{
			m_entityDef = null;
			m_premium = TAG_PREMIUM.NORMAL;
			m_currentCardBack = actor.GetComponentInChildren<CardBack>();
			m_cardActor = m_cardBackActor;
			m_cardBackActor.SetCardbackUpdateIgnore(ignoreUpdate: true);
			return true;
		}
		return false;
	}

	public void UpdateVisual(ActorVisualMode newVisualMode)
	{
		Actor previousActor = m_activeActor;
		SpellType transitionSpellType = SpellType.NONE;
		m_actorVisualMode = newVisualMode;
		if (m_visualType == CollectionUtils.ViewMode.CARDS)
		{
			if (newVisualMode == ActorVisualMode.DECK_TILE && m_entityDef != null && !m_entityDef.IsHeroSkin())
			{
				if (m_deckTile != null && m_entityDef != null)
				{
					bool offsetForRunes = m_entityDef.HasRuneCost;
					m_deckTile.UpdateNameTextForRuneBar(offsetForRunes);
					m_deckTile.UpdateDeckCardProperties(m_entityDef.IsElite(), isMultiCard: false, 1, useSliderAnimations: false);
				}
				m_activeActor = m_deckTile;
				transitionSpellType = SpellType.SUMMON_IN;
			}
			else
			{
				switch (newVisualMode)
				{
				case ActorVisualMode.BIG_CARD:
					m_activeActor = m_cardActor;
					transitionSpellType = SpellType.DEATHREVERSE;
					break;
				case ActorVisualMode.IN_PLAY_ZILLIAX_SIDEBOARD:
					m_activeActor = m_inPlayActor;
					m_activeActor.GetEntityDef().SetTag(GAME_TAG.TRANSFORM, 1);
					transitionSpellType = SpellType.DEATHREVERSE;
					break;
				}
			}
		}
		else
		{
			m_activeActor = m_cardActor;
			transitionSpellType = SpellType.DEATHREVERSE;
			if (m_deckTileToRemove != null)
			{
				m_deckTileToRemove.SetHighlight(highlight: false);
			}
		}
		if (previousActor == m_activeActor)
		{
			return;
		}
		if (previousActor != null)
		{
			previousActor.Hide();
			previousActor.gameObject.SetActive(value: false);
		}
		if (m_activeActor == null)
		{
			return;
		}
		m_activeActor.gameObject.SetActive(value: true);
		m_activeActor.Show();
		if (m_visualType == CollectionUtils.ViewMode.CARD_BACKS && m_currentCardBack != null)
		{
			CardBackManager.Get().UpdateCardBack(m_activeActor, m_currentCardBack);
		}
		if (newVisualMode == ActorVisualMode.IN_PLAY_ZILLIAX_SIDEBOARD)
		{
			m_activeActor.ActivateSpellBirthState(SpellType.EVIL_GLOW);
		}
		else
		{
			Spell transitionSpell = m_activeActor.GetSpell(transitionSpellType);
			if (transitionSpell != null)
			{
				transitionSpell.ActivateState(SpellStateType.BIRTH);
			}
		}
		if (m_entityDef != null && m_entityDef.IsHeroSkin())
		{
			CollectionHeroSkin heroSkin = m_activeActor.gameObject.GetComponent<CollectionHeroSkin>();
			if (heroSkin != null)
			{
				heroSkin.SetClass(m_entityDef);
				heroSkin.ShowSocketFX();
				heroSkin.ShowName = false;
			}
		}
	}

	public bool IsShown()
	{
		if (base.gameObject == null)
		{
			return false;
		}
		return base.gameObject.activeSelf;
	}

	public void Show(bool isOverDeck)
	{
		base.gameObject.SetActive(value: true);
		UpdateVisual((!isOverDeck) ? ActorVisualMode.BIG_CARD : ActorVisualMode.DECK_TILE);
		if (m_deckTile != null && m_entityDef != null)
		{
			m_deckTile.UpdateDeckCardProperties(m_entityDef.IsElite(), isMultiCard: false, 1, useSliderAnimations: false);
		}
	}

	public void Hide()
	{
		if (m_activeActor != null && m_entityDef != null && m_entityDef.IsHeroSkin())
		{
			CollectionHeroSkin heroSkin = m_activeActor.gameObject.GetComponent<CollectionHeroSkin>();
			if (heroSkin != null)
			{
				heroSkin.HideSocketFX();
			}
		}
		base.gameObject.SetActive(value: false);
		if (m_activeActor != null)
		{
			m_activeActor.Hide();
			m_activeActor.gameObject.SetActive(value: false);
			m_activeActor = null;
		}
	}

	public DeckTrayDeckTileVisual GetDeckTileToRemove()
	{
		return m_deckTileToRemove;
	}

	public void SetupInPlayActor()
	{
		if (m_inPlayActor == null)
		{
			LoadInPlayActor();
			InitInPlayActor();
		}
	}

	public void ClearInPlayActor()
	{
		if (m_inPlayActor != null)
		{
			Object.Destroy(m_inPlayActor.gameObject);
			m_inPlayActor = null;
		}
	}

	private void LoadDeckTile()
	{
		GameObject actorObject = AssetLoader.Get().InstantiatePrefab("DeckCardBar.prefab:c2bab6eea6c3a3a4d90dcd7572075291", AssetLoadingOptions.IgnorePrefabPosition);
		if (actorObject == null)
		{
			Debug.LogWarning(string.Format("CollectionDraggableCardVisual.OnDeckTileActorLoaded() - FAILED to load actor \"{0}\"", "DeckCardBar.prefab:c2bab6eea6c3a3a4d90dcd7572075291"));
			return;
		}
		m_deckTile = actorObject.GetComponent<CollectionDeckTileActor>();
		if (m_deckTile == null)
		{
			Debug.LogWarning(string.Format("CollectionDraggableCardVisual.OnDeckTileActorLoaded() - ERROR game object \"{0}\" has no CollectionDeckTileActor component", "DeckCardBar.prefab:c2bab6eea6c3a3a4d90dcd7572075291"));
			return;
		}
		m_deckTile.Hide();
		m_deckTile.transform.parent = base.transform;
		m_deckTile.transform.localPosition = DECK_TILE_LOCAL_POSITION;
		m_deckTile.transform.localScale = DECK_TILE_LOCAL_SCALE;
		m_deckTile.transform.localEulerAngles = ACTOR_LOCAL_EULER_ANGLES;
	}

	private void LoadInPlayActor()
	{
		if (m_entityDef == null)
		{
			return;
		}
		GameObject actorObject = AssetLoader.Get().InstantiatePrefab(ActorNames.GetPlayActor(m_entityDef, m_premium), AssetLoadingOptions.IgnorePrefabPosition);
		if (actorObject == null)
		{
			Debug.LogWarning(string.Format("CollectionDraggableCardVisual.LoadInPlayActor() - FAILED to load actor \"{0}\"", "Card_Play_Ally.prefab:23b7de16184fa8042bf6b734e7ca4d60"));
			return;
		}
		if (m_inPlayActor != null)
		{
			Object.Destroy(m_inPlayActor.gameObject);
			m_inPlayActor = null;
		}
		m_inPlayActor = actorObject.GetComponent<Actor>();
		if (m_inPlayActor == null)
		{
			Debug.LogWarning(string.Format("CollectionDraggableCardVisual.LoadInPlayActor() - ERROR game object \"{0}\" has no Actor component", "DeckCardBar.prefab:c2bab6eea6c3a3a4d90dcd7572075291"));
			return;
		}
		m_inPlayActor.Hide();
		m_inPlayActor.transform.parent = base.transform;
		m_inPlayActor.transform.localPosition = IN_PLAY_ACTOR_LOCAL_POSITION;
		m_inPlayActor.transform.localScale = IN_PLAY_ACTOR_LOCAL_SCALE;
		m_inPlayActor.transform.localEulerAngles = ACTOR_LOCAL_EULER_ANGLES;
	}

	private void LoadCardBack()
	{
		GameObject actorObject = AssetLoader.Get().InstantiatePrefab("Card_Hidden.prefab:1a94649d257bc284ca6e2962f634a8b9", AssetLoadingOptions.IgnorePrefabPosition);
		GameUtils.SetParent(actorObject, this);
		m_cardBackActor = actorObject.GetComponent<Actor>();
		m_cardBackActor.transform.localScale = CARD_ACTOR_LOCAL_SCALE;
		m_cardBackActor.TurnOffCollider();
		m_cardBackActor.Hide();
		actorObject.AddComponent<DragRotator>().SetInfo(m_CardDragRotatorInfo);
	}

	private void OnCardActorLoaded(string assetName, Actor actor, object callbackData)
	{
		if (actor == null)
		{
			Debug.LogWarning($"CollectionDraggableCardVisual.OnCardActorLoaded() - FAILED to load {assetName}");
			return;
		}
		actor.GetType();
		actor.TurnOffCollider();
		actor.Hide();
		if (base.name == "Card_Hero_Skin")
		{
			actor.transform.localScale = HERO_SKIN_ACTOR_LOCAL_SCALE;
		}
		else
		{
			actor.transform.localScale = CARD_ACTOR_LOCAL_SCALE;
		}
		actor.transform.parent = base.transform;
		actor.transform.localPosition = Vector3.zero;
		actor.gameObject.AddComponent<DragRotator>().SetInfo(m_CardDragRotatorInfo);
	}

	private void OnCardDefLoaded(string cardID, DefLoader.DisposableCardDef cardDef, object callbackData)
	{
		if (m_entityDef == null || m_entityDef.GetCardId() != cardID)
		{
			cardDef?.Dispose();
			return;
		}
		m_cardDef?.Dispose();
		m_cardDef = cardDef;
		m_inPlayCardDefOverride?.Dispose();
		m_inPlayCardDefOverride = null;
		InitDeckTileActor();
		InitCardActor();
		InitInPlayActor();
	}

	private void InitDeckTileActor()
	{
		InitActor(m_deckTile);
		m_deckTile.SetSlot(null);
		m_deckTile.SetCardDef(m_cardDef);
		m_deckTile.UpdateAllComponents();
		m_deckTile.UpdateDeckCardProperties(m_entityDef.IsElite(), isMultiCard: false, 1, useSliderAnimations: false);
	}

	private void InitCardActor()
	{
		InitActor(m_cardActor);
		m_cardActor.transform.localRotation = Quaternion.identity;
	}

	private void InitInPlayActor()
	{
		if (m_inPlayActor != null)
		{
			EntityDef entityDefToUse = m_entityDef;
			DefLoader.DisposableCardDef disposableCardDef = m_cardDef;
			m_inPlayCardDefOverride?.Dispose();
			m_inPlayCardDefOverride = null;
			m_inPlayEntityDefOverride = null;
			if (entityDefToUse != null && entityDefToUse.HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_LINKED_COSMETICMOUDLE))
			{
				DefLoader defLoader = DefLoader.Get();
				m_inPlayEntityDefOverride = defLoader.GetEntityDef(entityDefToUse.GetTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_LINKED_COSMETICMOUDLE));
				entityDefToUse = m_inPlayEntityDefOverride;
				m_inPlayCardDefOverride = defLoader.GetCardDef(entityDefToUse.GetCardId(), new CardPortraitQuality(1, m_premium));
				disposableCardDef = m_inPlayCardDefOverride;
			}
			InitActor(m_inPlayActor, entityDefToUse, disposableCardDef);
			Collider[] componentsInChildren = m_inPlayActor.GetComponentsInChildren<Collider>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
			UberText[] componentsInChildren2 = m_inPlayActor.GetComponentsInChildren<UberText>(includeInactive: true);
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].AmbientLightBlend = 0f;
			}
			GemObject attackObject = m_inPlayActor.GetAttackObject();
			if (attackObject != null)
			{
				DisableLightingBlendInGameObject(attackObject.m_gemMesh);
			}
			GemObject healthObject = m_inPlayActor.GetHealthObject();
			if (healthObject != null)
			{
				DisableLightingBlendInGameObject(healthObject.m_gemMesh);
			}
			DisableLightingBlendInGameObject(m_inPlayActor.m_eliteObject);
			m_inPlayActor.transform.localRotation = Quaternion.identity;
		}
	}

	private void DisableLightingBlendInGameObject(GameObject gameObject)
	{
		if (gameObject == null)
		{
			return;
		}
		List<Material> materials = new List<Material>();
		gameObject.GetComponent<Renderer>().GetMaterials(materials);
		foreach (Material item in materials)
		{
			item.SetFloat("_LightingBlend", 0f);
		}
	}

	private void InitActor(Actor actor, EntityDef entityDefToUse = null, DefLoader.DisposableCardDef disposableCardDef = null)
	{
		actor.SetEntityDef(entityDefToUse ?? m_entityDef);
		actor.SetCardDef(disposableCardDef ?? m_cardDef);
		actor.SetPremium(m_premium);
		actor.UpdateAllComponents();
	}

	private void EnsureScaleConstantsExist()
	{
		if (!s_scaleIsSetup)
		{
			s_scaleIsSetup = true;
			DECK_TILE_LOCAL_SCALE = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
			{
				PC = new Vector3(0.6f, 0.6f, 0.6f),
				Phone = new Vector3(0.9f, 0.9f, 0.9f)
			};
			CARD_ACTOR_LOCAL_SCALE = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
			{
				PC = new Vector3(6f, 6f, 6f),
				Phone = new Vector3(6.9f, 6.9f, 6.9f)
			};
			HERO_SKIN_ACTOR_LOCAL_SCALE = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
			{
				PC = new Vector3(7.5f, 7.5f, 7.5f),
				Phone = new Vector3(8.2f, 8.2f, 8.2f)
			};
			IN_PLAY_ACTOR_LOCAL_SCALE = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
			{
				PC = new Vector3(6f, 6f, 6f),
				Phone = new Vector3(6.9f, 6.9f, 6.9f)
			};
		}
	}
}
