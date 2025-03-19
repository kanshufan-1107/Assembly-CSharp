using UnityEngine;

public class DeckTrayDeckTileVisual : PegUIElement
{
	public static readonly GameLayer LAYER = GameLayer.CardRaycast;

	private readonly Vector3 BOX_COLLIDER_SIZE = new Vector3(25.34f, 2.14f, 3.68f);

	private readonly Vector3 BOX_COLLIDER_CENTER = new Vector3(-1.4f, 0f, 0f);

	protected const int DEFAULT_PORTRAIT_QUALITY = 1;

	protected CollectionDeck m_deck;

	protected CollectionDeckSlot m_slot;

	protected BoxCollider m_collider;

	protected CollectionDeckTileActor m_actor;

	protected CardDefHandle m_defHandleOverride;

	protected bool m_isInUse;

	protected bool m_useSliderAnimations;

	protected bool m_inArena;

	protected bool m_offsetCardNameForRunes;

	private ZilliaxSideboardDeck m_zilliaxSideboardDeck;

	private bool m_pendingRemoval;

	public CardDefHandle CardDefHandleOverride => m_defHandleOverride;

	public void Initialize(bool useFullScaleDeckTileActor)
	{
		string deckTileName = (useFullScaleDeckTileActor ? "DeckCardBar.prefab:c2bab6eea6c3a3a4d90dcd7572075291" : "DeckCardBar_phone.prefab:bd1c5e767f791984e851553bc5cb3b07");
		GameObject deckTileObj = AssetLoader.Get().InstantiatePrefab(deckTileName, AssetLoadingOptions.IgnorePrefabPosition);
		if (deckTileObj == null)
		{
			Debug.LogWarning($"DeckTrayDeckTileVisual.OnDeckTileActorLoaded() - FAILED to load actor \"{deckTileName}\"");
			return;
		}
		m_actor = deckTileObj.GetComponent<CollectionDeckTileActor>();
		if (m_actor == null)
		{
			Debug.LogWarning($"DeckTrayDeckTileVisual.OnDeckTileActorLoaded() - ERROR game object \"{deckTileName}\" has no CollectionDeckTileActor component");
			return;
		}
		GameUtils.SetParent(m_actor, this);
		m_actor.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
		UIBScrollableItem scrollItem = m_actor.GetComponent<UIBScrollableItem>();
		if (scrollItem != null)
		{
			scrollItem.SetCustomActiveState(IsInUse);
			scrollItem.UpdateScrollableParent();
		}
		SetUpActor();
		if (base.gameObject.GetComponent<BoxCollider>() == null)
		{
			m_collider = base.gameObject.AddComponent<BoxCollider>();
			m_collider.size = BOX_COLLIDER_SIZE;
			m_collider.center = BOX_COLLIDER_CENTER;
		}
		Hide();
		LayerUtils.SetLayer(base.gameObject, LAYER);
		SetDragTolerance(5f);
	}

	protected override void OnDestroy()
	{
		if (m_zilliaxSideboardDeck != null)
		{
			m_zilliaxSideboardDeck.OnDynamicZilliaxDefUpdated -= OnZilliaxSideboardUpdated;
		}
		m_defHandleOverride?.ReleaseCardDef();
		m_defHandleOverride = null;
		base.OnDestroy();
	}

	public string GetCardID()
	{
		return m_actor.GetEntityDef().GetCardId();
	}

	public TAG_PREMIUM GetPremium()
	{
		return m_actor.GetPremium();
	}

	public CollectionDeckSlot GetSlot()
	{
		return m_slot;
	}

	public void SetSlot(CollectionDeck deck, CollectionDeckSlot s, bool useSliderAnimations, bool offsetCardNameForRunes)
	{
		m_deck = deck;
		m_slot = s;
		m_useSliderAnimations = useSliderAnimations;
		m_offsetCardNameForRunes = offsetCardNameForRunes;
		SetUpActor();
	}

	public CollectionDeckTileActor GetActor()
	{
		return m_actor;
	}

	public Bounds GetBounds()
	{
		return m_collider.bounds;
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void ShowAndSetupActor()
	{
		Show();
		SetUpActor();
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void MarkAsUsed()
	{
		m_isInUse = true;
	}

	public void MarkAsUnused()
	{
		m_isInUse = false;
		if (m_zilliaxSideboardDeck != null)
		{
			m_zilliaxSideboardDeck.OnDynamicZilliaxDefUpdated -= OnZilliaxSideboardUpdated;
		}
		if (!(m_actor == null))
		{
			m_actor.UpdateDeckCardProperties(CollectionDeckTileActor.TileIconState.CARD_COUNT, 1, useSliderAnimations: false);
		}
	}

	public bool IsInUse()
	{
		return m_isInUse;
	}

	public void SetInArena(bool inArena)
	{
		m_inArena = inArena;
	}

	public void SetHighlight(bool highlight)
	{
		if (m_actor.m_highlight != null)
		{
			m_actor.m_highlight.SetActive(highlight);
		}
		if (m_actor.m_highlightGlow != null)
		{
			if (GetGhostedState() == CollectionDeckTileActor.GhostedState.RED)
			{
				m_actor.m_highlightGlow.SetActive(highlight);
			}
			else
			{
				m_actor.m_highlightGlow.SetActive(value: false);
			}
		}
	}

	public void UpdateGhostedState()
	{
		m_actor.SetGhosted(GetGhostedState());
		m_actor.UpdateGhostTileEffect();
		m_actor.UpdateCardRuneBannerComponent();
	}

	private CollectionDeckTileActor.GhostedState GetGhostedState()
	{
		CollectionDeckTileActor.GhostedState ghostedState = CollectionDeckTileActor.GhostedState.NONE;
		if (m_deck != null)
		{
			switch (m_deck.GetSlotStatus(m_slot))
			{
			case CollectionDeck.SlotStatus.MISSING:
				ghostedState = CollectionDeckTileActor.GhostedState.BLUE;
				break;
			case CollectionDeck.SlotStatus.NOT_VALID:
				ghostedState = CollectionDeckTileActor.GhostedState.RED;
				break;
			}
			if (m_deck.HasClass(TAG_CLASS.DEATHKNIGHT) && !SceneMgr.Get().IsInArenaDraftMode() && !m_deck.Runes.CanAddRunes(m_slot.GetEntityDef().GetRuneCost(), m_deck.Runes.CombinedValue))
			{
				ghostedState = CollectionDeckTileActor.GhostedState.RED;
			}
			if (m_pendingRemoval)
			{
				ghostedState = CollectionDeckTileActor.GhostedState.BLUE;
			}
		}
		return ghostedState;
	}

	private void SetUpActor()
	{
		if (m_actor == null || m_slot == null || string.IsNullOrEmpty(m_slot.CardID))
		{
			return;
		}
		m_actor.GetEntityDef();
		EntityDef entityDef = m_slot.GetEntityDef();
		m_defHandleOverride?.ReleaseCardDef();
		m_defHandleOverride = null;
		TAG_PREMIUM premium = m_slot.PreferredPremium;
		if (m_inArena && Options.Get().GetBool(Option.HAS_DISABLED_PREMIUMS_THIS_DRAFT))
		{
			premium = TAG_PREMIUM.NORMAL;
		}
		SideboardDeck sideboard = m_deck.GetOrCreateSideboard(entityDef.GetCardId(), premium);
		if (m_deck != null)
		{
			if (m_zilliaxSideboardDeck != null)
			{
				m_zilliaxSideboardDeck.OnDynamicZilliaxDefUpdated -= OnZilliaxSideboardUpdated;
			}
			m_zilliaxSideboardDeck = sideboard as ZilliaxSideboardDeck;
			if (m_zilliaxSideboardDeck != null)
			{
				m_zilliaxSideboardDeck.OnDynamicZilliaxDefUpdated -= OnZilliaxSideboardUpdated;
				m_zilliaxSideboardDeck.OnDynamicZilliaxDefUpdated += OnZilliaxSideboardUpdated;
				if (m_zilliaxSideboardDeck.DynamicZilliaxDef != null)
				{
					entityDef = m_zilliaxSideboardDeck.DynamicZilliaxDef;
					m_defHandleOverride?.ReleaseCardDef();
					m_defHandleOverride = m_zilliaxSideboardDeck.DynamicZilliaxCardDefHandle;
					m_slot.m_entityDefOverride = m_zilliaxSideboardDeck.DynamicZilliaxDef;
				}
			}
		}
		m_actor.SetSlot(m_slot);
		m_actor.SetPremium(premium);
		m_actor.SetEntityDef(entityDef);
		m_actor.SetGhosted(GetGhostedState());
		m_actor.UpdateCardRuneBannerComponent();
		m_actor.UpdateNameTextForRuneBar(m_offsetCardNameForRunes);
		if (m_deck != null)
		{
			m_actor.SetupSideboard(sideboard);
			if (m_deck.ProcessForDynamicallySortingSlot(m_slot))
			{
				m_deck.ForceUpdateSlotPosition(m_slot);
			}
		}
		bool isUnique = entityDef?.IsElite() ?? false;
		if (isUnique && m_inArena && m_slot.Count > 1)
		{
			isUnique = false;
		}
		m_actor.UpdateDeckCardProperties(isUnique, isMultiCard: false, m_slot.Count, m_useSliderAnimations);
		if (m_defHandleOverride == null)
		{
			DefLoader.Get().LoadCardDef(entityDef.GetCardId(), delegate(string cardID, DefLoader.DisposableCardDef cardDef, object data)
			{
				using (cardDef)
				{
					if (!(m_actor == null) && cardID.Equals(m_actor.GetEntityDef().GetCardId()))
					{
						m_actor.SetCardDef(cardDef);
						m_actor.UpdateAllComponents();
						m_actor.UpdateGhostTileEffect();
					}
				}
			}, null, new CardPortraitQuality(1, premium));
			return;
		}
		DefLoader.DisposableCardDef disposableCardDef2 = m_defHandleOverride.Share();
		using (disposableCardDef2)
		{
			m_actor.SetCardDef(disposableCardDef2);
			m_actor.UpdateAllComponents();
			m_actor.UpdateGhostTileEffect();
		}
	}

	private void OnZilliaxSideboardUpdated(ZilliaxSideboardDeck zilliaxSideboardDeck)
	{
		if (zilliaxSideboardDeck != null)
		{
			m_slot.m_entityDefOverride = zilliaxSideboardDeck.DynamicZilliaxDef;
			m_deck.ForceUpdateSlotPosition(m_slot);
			CollectionDeckTray.Get().GetCardsContent().UpdateCardList();
			m_actor.SetEntityDef(zilliaxSideboardDeck.DynamicZilliaxDef);
			CardDefHandle zilliaxCardDefHandle = zilliaxSideboardDeck.DynamicZilliaxCardDefHandle;
			using (DefLoader.DisposableCardDef newCardDef = zilliaxCardDefHandle.Share())
			{
				m_actor.SetCardDef(newCardDef);
			}
			zilliaxCardDefHandle.ReleaseCardDef();
			SetUpActor();
		}
	}

	public void SetPendingRemoval(bool pendingRemoval)
	{
		m_pendingRemoval = pendingRemoval;
		UpdateGhostedState();
	}

	public bool HasRuneCost()
	{
		if (m_actor == null)
		{
			return false;
		}
		return m_actor.GetEntityDef()?.HasRuneCost ?? false;
	}
}
