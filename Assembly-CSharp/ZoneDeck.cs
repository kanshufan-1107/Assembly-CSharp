using System.Collections.Generic;
using UnityEngine;

public class ZoneDeck : Zone
{
	public Actor m_ThicknessFull;

	public Transform m_TicknessFullTopOfDeck;

	public Actor m_Thickness75;

	public Transform m_Tickness75TopOfDeck;

	public Actor m_Thickness50;

	public Transform m_Tickness50TopOfDeck;

	public Actor m_Thickness25;

	public Transform m_Tickness25TopOfDeck;

	public Actor m_Thickness1;

	public Transform m_Tickness1TopOfDeck;

	public Spell m_DeckFatigueGlow;

	public Spell m_DeckTradeableGlow;

	public Spell m_DeckForgeableGlow;

	public DeckCover m_DeckCover;

	public GameObject m_DeckVisualRootObject;

	public TooltipZone m_deckTooltipZone;

	public TooltipZone m_playerHandTooltipZone;

	public TooltipZone m_playerManaTooltipZone;

	public TooltipZone m_friendlyHandTooltipZone;

	private const int MAX_THICKNESS_CARD_COUNT = 26;

	private bool m_suppressEmotes;

	private bool m_warnedAboutLastCard;

	private bool m_warnedAboutNoCards;

	private int m_numCardAnimating;

	private int m_numDefaultHandToDeckAnimation;

	private readonly Dictionary<Actor, Mesh> m_originalDeckMeshes = new Dictionary<Actor, Mesh>();

	private bool m_hasDirtyDeckMeshes;

	public void Awake()
	{
		if (m_deckTooltipZone != null && m_playerHandTooltipZone != null)
		{
			m_deckTooltipZone.SetTooltipChangeCallback(TooltipChanged);
		}
		if (m_DeckCover != null)
		{
			m_DeckCover.SetDeckVisualRootObject(m_DeckVisualRootObject);
		}
		CacheOriginalDeckMeshes();
	}

	public void TooltipChanged(bool shown)
	{
		if (!shown)
		{
			m_playerHandTooltipZone.HideTooltip();
			if (m_playerManaTooltipZone != null)
			{
				m_playerManaTooltipZone.HideTooltip();
			}
			if (m_friendlyHandTooltipZone != null)
			{
				m_friendlyHandTooltipZone.HideTooltip();
			}
		}
	}

	public override bool CanAcceptTags(int controllerId, TAG_ZONE zoneTag, TAG_CARDTYPE cardType, Entity entity)
	{
		GameEntity gameEntity = GameState.Get().GetGameEntity();
		if (gameEntity != null && gameEntity.OverwriteZoneDeckToAcceptEntity(this, controllerId, zoneTag, cardType, entity))
		{
			return true;
		}
		return base.CanAcceptTags(controllerId, zoneTag, cardType, entity);
	}

	public override void Reset()
	{
		base.Reset();
		UpdateLayout();
	}

	public override void UpdateLayout()
	{
		m_updatingLayout++;
		if (IsBlockingLayout())
		{
			UpdateLayoutFinished();
			return;
		}
		UpdateThickness();
		UpdateDeckStateEmotes();
		if (m_DeckCover != null)
		{
			m_DeckCover.UpdateVisual(m_Side);
		}
		for (int i = 0; i < m_cards.Count; i++)
		{
			Card card = m_cards[i];
			if (!card.IsDoNotSort())
			{
				card.HideCard();
				SetCardToInDeckState(card);
			}
		}
		UpdateLayoutFinished();
	}

	public override void OnHealingDoesDamageEntityEnteredPlay()
	{
	}

	public override void OnHealingDoesDamageEntityMousedOut()
	{
	}

	public override void OnHealingDoesDamageEntityMousedOver()
	{
	}

	public override void OnLifestealDoesDamageEntityEnteredPlay()
	{
	}

	public override void OnLifestealDoesDamageEntityMousedOut()
	{
	}

	public override void OnLifestealDoesDamageEntityMousedOver()
	{
	}

	public void SetVisibility(bool visible)
	{
		base.gameObject.SetActive(visible);
	}

	public bool GetVisibility()
	{
		return base.gameObject.activeSelf;
	}

	public void SetCardToInDeckState(Card card)
	{
		Transform topBone = GetTopOfDeckBoneForThickness();
		card.transform.localEulerAngles = new Vector3(275f, 270f, 0f);
		card.transform.position = topBone.position;
		card.transform.localScale = new Vector3(0.88f, 0.88f, 0.88f);
		card.EnableTransitioningZones(enable: false);
	}

	public void DoFatigueGlow()
	{
		if (!(m_DeckFatigueGlow == null) && GameState.Get().GetBooleanGameOption(GameEntityOption.ALLOW_FATIGUE) && m_cards.Count <= 0)
		{
			m_DeckFatigueGlow.ActivateState(SpellStateType.ACTION);
		}
	}

	public void ShowTradeableGlow()
	{
		SpellUtils.ActivateBirthIfNecessary(m_DeckTradeableGlow);
	}

	public void HideTradeableGlow(bool justTraded = false)
	{
		if (!(m_DeckTradeableGlow == null))
		{
			if (justTraded)
			{
				SpellUtils.ActivateDeathIfNecessary(m_DeckTradeableGlow);
			}
			else if (m_DeckTradeableGlow.GetActiveState() != SpellStateType.DEATH)
			{
				SpellUtils.ActivateCancelIfNecessary(m_DeckTradeableGlow);
			}
		}
	}

	public void ShowForgeableGlow()
	{
		SpellUtils.ActivateBirthIfNecessary(m_DeckForgeableGlow);
	}

	public void HideForgeableGlow(bool justForged = false)
	{
		if (!(m_DeckForgeableGlow == null))
		{
			if (justForged)
			{
				SpellUtils.ActivateDeathIfNecessary(m_DeckForgeableGlow);
			}
			else if (m_DeckForgeableGlow.GetActiveState() != SpellStateType.DEATH)
			{
				SpellUtils.ActivateCancelIfNecessary(m_DeckForgeableGlow);
			}
		}
	}

	public void NotifyCardAnimationStart()
	{
		m_numCardAnimating++;
		if (m_DeckCover != null)
		{
			m_DeckCover.OpenDeckCover();
		}
	}

	public void NotifyCardAnimationFinish()
	{
		m_numCardAnimating--;
		if (m_numCardAnimating <= 0 && m_DeckCover != null)
		{
			m_DeckCover.CloseDeckCover();
		}
	}

	public void IncrementDefaultHandToDeckAnimationCount()
	{
		m_numDefaultHandToDeckAnimation++;
	}

	public void DecrementDefaultHandToDeckAnimationCount()
	{
		m_numDefaultHandToDeckAnimation--;
	}

	public int GetDefaultHandToDeckAnimationCount()
	{
		return m_numDefaultHandToDeckAnimation;
	}

	public bool IsFatigued()
	{
		return m_cards.Count == 0;
	}

	public Transform GetTopOfDeckBoneForThickness()
	{
		Actor thickness = GetThicknessForLayout();
		if (thickness == m_ThicknessFull)
		{
			return m_TicknessFullTopOfDeck;
		}
		if (thickness == m_Thickness75)
		{
			return m_Tickness75TopOfDeck;
		}
		if (thickness == m_Thickness50)
		{
			return m_Tickness50TopOfDeck;
		}
		if (thickness == m_Thickness25)
		{
			return m_Tickness25TopOfDeck;
		}
		_ = thickness == m_Thickness1;
		return m_Tickness1TopOfDeck;
	}

	public Actor GetActiveThickness()
	{
		if (m_ThicknessFull.GetMeshRenderer().enabled)
		{
			return m_ThicknessFull;
		}
		if (m_Thickness75.GetMeshRenderer().enabled)
		{
			return m_Thickness75;
		}
		if (m_Thickness50.GetMeshRenderer().enabled)
		{
			return m_Thickness50;
		}
		if (m_Thickness25.GetMeshRenderer().enabled)
		{
			return m_Thickness25;
		}
		if (m_Thickness1.GetMeshRenderer().enabled)
		{
			return m_Thickness1;
		}
		return null;
	}

	public Actor GetThicknessForLayout()
	{
		Actor activeThickness = GetActiveThickness();
		if (activeThickness != null)
		{
			return activeThickness;
		}
		return m_Thickness1;
	}

	public bool AreEmotesSuppressed()
	{
		return m_suppressEmotes;
	}

	public void SetSuppressEmotes(bool suppress)
	{
		m_suppressEmotes = suppress;
	}

	private void UpdateThickness()
	{
		m_ThicknessFull.GetMeshRenderer().enabled = false;
		m_Thickness75.GetMeshRenderer().enabled = false;
		m_Thickness50.GetMeshRenderer().enabled = false;
		m_Thickness25.GetMeshRenderer().enabled = false;
		m_Thickness1.GetMeshRenderer().enabled = false;
		int count = m_cards.Count;
		if (count == 0)
		{
			if (GameState.Get().GetBooleanGameOption(GameEntityOption.ALLOW_FATIGUE))
			{
				m_DeckFatigueGlow.ActivateState(SpellStateType.BIRTH);
			}
			return;
		}
		if (GameState.Get().GetBooleanGameOption(GameEntityOption.ALLOW_FATIGUE) && m_DeckFatigueGlow.GetActiveState() != 0)
		{
			m_DeckFatigueGlow.ActivateState(SpellStateType.DEATH);
		}
		if (count == 1)
		{
			m_Thickness1.GetMeshRenderer().enabled = true;
			return;
		}
		float fullness = (float)count / 26f;
		if (fullness > 0.75f)
		{
			m_ThicknessFull.GetMeshRenderer().enabled = true;
		}
		else if (fullness > 0.5f)
		{
			m_Thickness75.GetMeshRenderer().enabled = true;
		}
		else if (fullness > 0.25f)
		{
			m_Thickness50.GetMeshRenderer().enabled = true;
		}
		else if (fullness > 0f)
		{
			m_Thickness25.GetMeshRenderer().enabled = true;
		}
	}

	public void UpdateToCustomDeckMeshes(CardBack.CustomDeckMeshes customMeshes)
	{
		TryUpdateDeckMesh(m_ThicknessFull, customMeshes.ThicknessFull);
		TryUpdateDeckMesh(m_Thickness75, customMeshes.Thickness75);
		TryUpdateDeckMesh(m_Thickness50, customMeshes.Thickness50);
		TryUpdateDeckMesh(m_Thickness25, customMeshes.Thickness25);
		TryUpdateDeckMesh(m_Thickness1, customMeshes.Thickness1);
		m_hasDirtyDeckMeshes = true;
	}

	public void TryRestoreOriginalDeckMeshes()
	{
		if (!m_hasDirtyDeckMeshes)
		{
			return;
		}
		foreach (KeyValuePair<Actor, Mesh> originalActorToMeshKvp in m_originalDeckMeshes)
		{
			TryUpdateDeckMesh(originalActorToMeshKvp.Key, originalActorToMeshKvp.Value);
		}
		m_hasDirtyDeckMeshes = false;
	}

	private void UpdateDeckStateEmotes()
	{
		if (!GameState.Get().IsPastBeginPhase() || m_suppressEmotes || GameState.Get().GetGameEntity().HasTag(GAME_TAG.HIDE_OUT_OF_CARDS_WARNING))
		{
			return;
		}
		int count = m_cards.Count;
		if (count <= 0 && !m_warnedAboutNoCards)
		{
			m_warnedAboutNoCards = true;
			m_warnedAboutLastCard = true;
			m_controller.GetHeroCard()?.PlayEmote(EmoteType.NOCARDS);
			return;
		}
		if (count == 1 && !m_warnedAboutLastCard)
		{
			if (!(InputManager.Get() != null) || !InputManager.Get().IsDemonPortalActive(m_controller.IsFriendlySide()))
			{
				m_warnedAboutLastCard = true;
				m_controller.GetHeroCard()?.PlayEmote(EmoteType.LOWCARDS);
			}
			return;
		}
		if (m_warnedAboutLastCard && count > 1)
		{
			m_warnedAboutLastCard = false;
		}
		if (m_warnedAboutNoCards && count > 0)
		{
			m_warnedAboutNoCards = false;
		}
	}

	private void CacheOriginalDeckMeshes()
	{
		TryAppendDeckMeshToCache(m_ThicknessFull, m_originalDeckMeshes);
		TryAppendDeckMeshToCache(m_Thickness75, m_originalDeckMeshes);
		TryAppendDeckMeshToCache(m_Thickness50, m_originalDeckMeshes);
		TryAppendDeckMeshToCache(m_Thickness25, m_originalDeckMeshes);
		TryAppendDeckMeshToCache(m_Thickness1, m_originalDeckMeshes);
	}

	private static void TryUpdateDeckMesh(Actor meshActor, Mesh newMesh)
	{
		if (meshActor == null)
		{
			return;
		}
		if (newMesh == null)
		{
			Debug.LogWarning("ZoneDeck failed to update deck mesh as new mesh was null!");
			return;
		}
		MeshFilter filter = meshActor.GetComponentInChildren<MeshFilter>();
		if (filter == null)
		{
			Debug.LogWarning("ZoneDeck failed to update deck mesh for " + meshActor.name + " as it couldn't find original mesh!");
		}
		else
		{
			filter.mesh = newMesh;
		}
	}

	private static void TryAppendDeckMeshToCache(Actor actor, Dictionary<Actor, Mesh> collection)
	{
		MeshFilter filter = actor.GetComponentInChildren<MeshFilter>();
		if (!(filter == null))
		{
			collection[actor] = filter.mesh;
		}
	}
}
