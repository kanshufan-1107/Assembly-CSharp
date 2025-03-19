using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZonePlay : Zone
{
	public class PlayZoneSizeOverride
	{
		public float m_scale;

		public float m_slotWidthModifier;
	}

	public int m_MaxSlots = 7;

	public float m_BigCardCenterOffset = 2.5f;

	public MagneticBeamSpell m_MagneticBeamSpell;

	private const float DEFAULT_TRANSITION_TIME = 1f;

	public const float PHONE_CARD_SCALE = 1.15f;

	private float[] PHONE_WIDTH_MODIFIERS = new float[8] { 0.25f, 0.25f, 0.25f, 0.25f, 0.22f, 0.19f, 0.15f, 0.1f };

	private int m_mousedOverSlot = -1;

	private int m_lettuceAbilityReservedSlot = -1;

	private float m_slotWidth;

	private float m_transitionTime = 1f;

	private float m_baseTransitionTime = 1f;

	private ZoneTransitionStyle m_transitionStyleOverride;

	private MagneticBeamSpell m_magneticBeamSpellInstance;

	private Card m_previousHeldCard;

	protected Vector3 m_defaultScale;

	private void Awake()
	{
		m_slotWidth = GetComponent<Collider>().bounds.size.x / (float)m_MaxSlots;
	}

	public float GetTransitionTime()
	{
		return m_transitionTime;
	}

	public void SetTransitionTime(float transitionTime)
	{
		m_transitionTime = transitionTime;
	}

	public void ResetTransitionTime()
	{
		m_transitionTime = m_baseTransitionTime;
	}

	public void OverrideBaseTransitionTime(float newTransitionTime)
	{
		m_baseTransitionTime = newTransitionTime;
	}

	public void SetZoneTransitionStyleOverride(ZoneTransitionStyle zoneTransitionStyle)
	{
		m_transitionStyleOverride = zoneTransitionStyle;
	}

	public void SortWithSpotForHeldCard(int slot)
	{
		m_mousedOverSlot = slot;
		UpdateLayout();
	}

	public override Card GetCardAtSlot(int slot)
	{
		for (int i = 0; i < m_cards.Count; i++)
		{
			if (GetSlotOfCardAtIndex(i) == slot)
			{
				return m_cards[i];
			}
		}
		return null;
	}

	public void SortWithSpotForLettuceAbilityCard(int slot)
	{
		m_lettuceAbilityReservedSlot = slot;
		UpdateLayout();
	}

	public MagneticBeamSpell GetMagneticBeamSpell()
	{
		return m_MagneticBeamSpell;
	}

	public void OnMagneticHeld(Card heldCard)
	{
		if (m_mousedOverSlot < 1 || !heldCard.GetEntity().HasTag(GAME_TAG.MAGNETIC) || heldCard.GetZone() is ZonePlay)
		{
			return;
		}
		if (m_magneticBeamSpellInstance == null)
		{
			m_magneticBeamSpellInstance = (MagneticBeamSpell)SpellManager.Get().GetSpell(m_MagneticBeamSpell);
		}
		Card applicableMechToRight = null;
		List<Card> applicableMechsOnBoard = new List<Card>();
		int zonePos = m_mousedOverSlot;
		int optionPos = ZoneMgr.Get().PredictZonePosition(heldCard.GetEntity(), this, zonePos);
		for (int cardIndex = 0; cardIndex < m_cards.Count; cardIndex++)
		{
			Card card = m_cards[cardIndex];
			Entity entity = card.GetEntity();
			if (entity.HasTag(GAME_TAG.UNTOUCHABLE) || !entity.CanBeMagnitizedBy(heldCard.GetEntity()) || entity.GetRealTimeZone() != TAG_ZONE.PLAY)
			{
				SpellUtils.ActivateDeathIfNecessary(card.GetActorSpell(SpellType.MAGNETIC_PLAY_LINKED_LEFT));
				SpellUtils.ActivateDeathIfNecessary(card.GetActorSpell(SpellType.MAGNETIC_PLAY_UNLINKED_LEFT));
				SpellUtils.ActivateDeathIfNecessary(card.GetActorSpell(SpellType.MAGNETIC_PLAY_UNLINKED_LEFT_DIMMED));
				continue;
			}
			applicableMechsOnBoard.Add(card);
			if (entity.GetRealTimeZonePosition() == optionPos)
			{
				applicableMechToRight = card;
			}
		}
		heldCard.GetActor().ToggleForceIdle(bOn: true);
		heldCard.UpdateActorState();
		foreach (Card card2 in applicableMechsOnBoard)
		{
			card2.GetActor().ToggleForceIdle(bOn: true);
			card2.UpdateActorState();
			if (applicableMechToRight == card2)
			{
				SpellUtils.ActivateBirthIfNecessary(card2.GetActorSpell(SpellType.MAGNETIC_PLAY_LINKED_LEFT));
				SpellUtils.ActivateDeathIfNecessary(card2.GetActorSpell(SpellType.MAGNETIC_PLAY_UNLINKED_LEFT));
				SpellUtils.ActivateDeathIfNecessary(card2.GetActorSpell(SpellType.MAGNETIC_PLAY_UNLINKED_LEFT_DIMMED));
			}
			else if (applicableMechToRight == null)
			{
				SpellUtils.ActivateBirthIfNecessary(card2.GetActorSpell(SpellType.MAGNETIC_PLAY_UNLINKED_LEFT));
				SpellUtils.ActivateDeathIfNecessary(card2.GetActorSpell(SpellType.MAGNETIC_PLAY_LINKED_LEFT));
				SpellUtils.ActivateDeathIfNecessary(card2.GetActorSpell(SpellType.MAGNETIC_PLAY_UNLINKED_LEFT_DIMMED));
			}
			else
			{
				SpellUtils.ActivateBirthIfNecessary(card2.GetActorSpell(SpellType.MAGNETIC_PLAY_UNLINKED_LEFT_DIMMED));
				SpellUtils.ActivateDeathIfNecessary(card2.GetActorSpell(SpellType.MAGNETIC_PLAY_UNLINKED_LEFT));
				SpellUtils.ActivateDeathIfNecessary(card2.GetActorSpell(SpellType.MAGNETIC_PLAY_LINKED_LEFT));
			}
		}
		if (applicableMechsOnBoard.Count > 0)
		{
			if (applicableMechToRight != null)
			{
				m_magneticBeamSpellInstance.SetSource(heldCard.gameObject);
				if (m_magneticBeamSpellInstance.GetTarget() != applicableMechToRight.gameObject)
				{
					m_magneticBeamSpellInstance.RemoveAllTargets();
					m_magneticBeamSpellInstance.AddTarget(applicableMechToRight.gameObject);
				}
				SpellUtils.ActivateBirthIfNecessary(m_magneticBeamSpellInstance);
				SpellUtils.ActivateBirthIfNecessary(heldCard.GetActorSpell(SpellType.MAGNETIC_HAND_LINKED_RIGHT));
				SpellUtils.ActivateDeathIfNecessary(heldCard.GetActorSpell(SpellType.MAGNETIC_HAND_UNLINKED));
			}
			else
			{
				SpellUtils.ActivateDeathIfNecessary(m_magneticBeamSpellInstance);
				SpellUtils.ActivateBirthIfNecessary(heldCard.GetActorSpell(SpellType.MAGNETIC_HAND_UNLINKED));
				SpellUtils.ActivateDeathIfNecessary(heldCard.GetActorSpell(SpellType.MAGNETIC_HAND_LINKED_RIGHT));
			}
		}
		else
		{
			SpellUtils.ActivateDeathIfNecessary(m_magneticBeamSpellInstance);
			SpellUtils.ActivateDeathIfNecessary(heldCard.GetActorSpell(SpellType.MAGNETIC_HAND_LINKED_RIGHT));
			SpellUtils.ActivateDeathIfNecessary(heldCard.GetActorSpell(SpellType.MAGNETIC_HAND_UNLINKED));
		}
	}

	public void OnMagneticPlay(Card playedCard, int zonePos)
	{
		if (!playedCard.GetEntity().HasTag(GAME_TAG.MAGNETIC))
		{
			if (m_magneticBeamSpellInstance != null)
			{
				SpellUtils.ActivateDeathIfNecessary(m_magneticBeamSpellInstance);
			}
			return;
		}
		Card targetMech = null;
		for (int cardIndex = 0; cardIndex < m_cards.Count; cardIndex++)
		{
			Card card = m_cards[cardIndex];
			Entity entity = card.GetEntity();
			if (!entity.HasTag(GAME_TAG.UNTOUCHABLE) && entity.CanBeMagnitizedBy(playedCard.GetEntity()) && entity.GetRealTimeZone() == TAG_ZONE.PLAY)
			{
				if (card.GetEntity().GetRealTimeZonePosition() == zonePos)
				{
					targetMech = card;
					continue;
				}
				card.GetActor().ToggleForceIdle(bOn: false);
				card.UpdateActorState();
				SpellUtils.ActivateDeathIfNecessary(card.GetActorSpell(SpellType.MAGNETIC_PLAY_LINKED_LEFT));
				SpellUtils.ActivateDeathIfNecessary(card.GetActorSpell(SpellType.MAGNETIC_PLAY_UNLINKED_LEFT));
				SpellUtils.ActivateDeathIfNecessary(card.GetActorSpell(SpellType.MAGNETIC_PLAY_UNLINKED_LEFT_DIMMED));
			}
		}
		if (targetMech != null)
		{
			if (m_magneticBeamSpellInstance == null)
			{
				m_magneticBeamSpellInstance = (MagneticBeamSpell)SpellManager.Get().GetSpell(m_MagneticBeamSpell);
			}
			m_magneticBeamSpellInstance.SetSource(playedCard.gameObject);
			m_magneticBeamSpellInstance.RemoveAllTargets();
			m_magneticBeamSpellInstance.AddTarget(targetMech.gameObject);
			MagneticPlayData playData = new MagneticPlayData();
			playData.m_playedCard = playedCard;
			playData.m_targetMech = targetMech;
			playData.m_beamSpell = m_magneticBeamSpellInstance;
			playedCard.SetMagneticPlayData(playData);
			targetMech.SetIsMagneticTarget(isTarget: true);
			playedCard.GetActor().ToggleForceIdle(bOn: true);
			playedCard.UpdateActorState();
			targetMech.GetActor().ToggleForceIdle(bOn: true);
			targetMech.UpdateActorState();
			m_magneticBeamSpellInstance = null;
			SpellUtils.ActivateBirthIfNecessary(playedCard.GetActorSpell(SpellType.MAGNETIC_HAND_LINKED_RIGHT));
			SpellUtils.ActivateBirthIfNecessary(targetMech.GetActorSpell(SpellType.MAGNETIC_PLAY_LINKED_LEFT));
			SpellUtils.ActivateDeathIfNecessary(playedCard.GetActorSpell(SpellType.MAGNETIC_HAND_UNLINKED));
			SpellUtils.ActivateDeathIfNecessary(targetMech.GetActorSpell(SpellType.MAGNETIC_PLAY_UNLINKED_LEFT));
			SpellUtils.ActivateDeathIfNecessary(targetMech.GetActorSpell(SpellType.MAGNETIC_PLAY_UNLINKED_LEFT_DIMMED));
		}
		else
		{
			playedCard.GetActor().ToggleForceIdle(bOn: false);
			playedCard.UpdateActorState();
			SpellUtils.ActivateDeathIfNecessary(m_magneticBeamSpellInstance);
			SpellUtils.ActivateDeathIfNecessary(playedCard.GetActorSpell(SpellType.MAGNETIC_HAND_LINKED_RIGHT));
			SpellUtils.ActivateDeathIfNecessary(playedCard.GetActorSpell(SpellType.MAGNETIC_HAND_UNLINKED));
		}
	}

	public void OnMagneticDropped(Card droppedCard)
	{
		if (!droppedCard.GetEntity().HasTag(GAME_TAG.MAGNETIC))
		{
			return;
		}
		SpellUtils.ActivateDeathIfNecessary(m_magneticBeamSpellInstance);
		SpellUtils.ActivateDeathIfNecessary(droppedCard.GetActorSpell(SpellType.MAGNETIC_HAND_LINKED_RIGHT));
		SpellUtils.ActivateDeathIfNecessary(droppedCard.GetActorSpell(SpellType.MAGNETIC_HAND_UNLINKED));
		droppedCard.GetActor().ToggleForceIdle(bOn: false);
		droppedCard.UpdateActorState();
		for (int cardIndex = 0; cardIndex < m_cards.Count; cardIndex++)
		{
			Card card = m_cards[cardIndex];
			Entity entity = card.GetEntity();
			if (!entity.HasTag(GAME_TAG.UNTOUCHABLE) && entity.CanBeMagnitizedBy(droppedCard.GetEntity()))
			{
				card.GetActor().ToggleForceIdle(bOn: false);
				card.UpdateActorState();
				SpellUtils.ActivateDeathIfNecessary(card.GetActorSpell(SpellType.MAGNETIC_PLAY_LINKED_LEFT));
				SpellUtils.ActivateDeathIfNecessary(card.GetActorSpell(SpellType.MAGNETIC_PLAY_UNLINKED_LEFT));
				SpellUtils.ActivateDeathIfNecessary(card.GetActorSpell(SpellType.MAGNETIC_PLAY_UNLINKED_LEFT_DIMMED));
			}
		}
	}

	public override void OnDiedLastCombatMousedOver()
	{
		foreach (Card card in m_cards)
		{
			Actor actor = card.GetActor();
			if (!(actor == null))
			{
				Entity entity = actor.GetEntity();
				if (entity != null && entity.HasTag(GAME_TAG.BACON_DIED_LAST_COMBAT))
				{
					actor.SetActorState(ActorStateType.CARD_VALID_TARGET);
				}
			}
		}
	}

	public override void OnDiedLastCombatMousedOut()
	{
		foreach (Card card in m_cards)
		{
			Actor actor = card.GetActor();
			if (!(actor == null))
			{
				Entity entity = actor.GetEntity();
				if (entity != null && entity.HasTag(GAME_TAG.BACON_DIED_LAST_COMBAT))
				{
					actor.SetActorState(ActorStateType.CARD_IDLE);
				}
			}
		}
	}

	public int GetSlotMousedOver()
	{
		return m_mousedOverSlot;
	}

	public bool HasMousedOverSlotChanged(int slot)
	{
		return m_mousedOverSlot != slot;
	}

	public float GetSlotWidth(int numberOfCards)
	{
		m_slotWidth = GetComponent<Collider>().bounds.size.x / (float)m_MaxSlots;
		if (m_mousedOverSlot >= 1)
		{
			numberOfCards++;
		}
		if (m_lettuceAbilityReservedSlot >= 0)
		{
			numberOfCards++;
		}
		numberOfCards = Mathf.Clamp(numberOfCards, 0, m_MaxSlots);
		float widthModifier = 1f;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			widthModifier += PHONE_WIDTH_MODIFIERS[numberOfCards];
		}
		PlayZoneSizeOverride sizeOverride = GameState.Get().GetGameEntity().GetPlayZoneSizeOverride();
		if (sizeOverride != null)
		{
			widthModifier = sizeOverride.m_slotWidthModifier;
		}
		return m_slotWidth * widthModifier;
	}

	public float GetSlotWidth()
	{
		return GetSlotWidth(m_cards.Count);
	}

	public void UnhideCardZzzEffects()
	{
		for (int cardIndex = 0; cardIndex < m_cards.Count; cardIndex++)
		{
			Card card = m_cards[cardIndex];
			if (card.GetEntity().IsAsleep())
			{
				SpellUtils.ActivateBirthIfNecessary(card.GetActorSpell(SpellType.Zzz));
			}
		}
	}

	public void HideCardZzzEffects()
	{
		for (int cardIndex = 0; cardIndex < m_cards.Count; cardIndex++)
		{
			SpellUtils.ActivateDeathIfNecessary(m_cards[cardIndex].GetActorSpell(SpellType.Zzz));
		}
	}

	public Vector3 GetCardPosition(Card card)
	{
		int index = m_cards.FindIndex((Card currCard) => currCard == card);
		return GetCardPosition(index);
	}

	public override int GetLastSlot()
	{
		return m_cards.Count;
	}

	public int GetSlotOfCardAtIndex(int index)
	{
		if (index < 0 || index >= m_cards.Count)
		{
			return -1;
		}
		Entity targetEntity = m_cards[index].GetEntity();
		if (targetEntity == null)
		{
			return -1;
		}
		targetEntity.GetEntityId();
		int currentSlot = 1;
		for (int i = 0; i <= index; i++)
		{
			if (i == index)
			{
				return currentSlot;
			}
			if (m_cards[i].GetEntity() != null)
			{
				currentSlot++;
			}
		}
		return -1;
	}

	public Vector3 GetCardPosition(int index)
	{
		if (index < 0)
		{
			return base.transform.position;
		}
		int numberOfSlots = GetLastSlot();
		if (m_mousedOverSlot >= 0)
		{
			numberOfSlots++;
		}
		if (m_lettuceAbilityReservedSlot >= 0)
		{
			numberOfSlots++;
		}
		Vector3 zoneCenter = GetComponent<Collider>().bounds.center;
		float halfOneCardWidth = 0.5f * GetSlotWidth();
		float halfTotalWidth = (float)numberOfSlots * halfOneCardWidth;
		float firstSlotCenterX = zoneCenter.x - halfTotalWidth + halfOneCardWidth;
		float widthsToAdd = 0f;
		int slotOfIndex = GetSlotOfCardAtIndex(index);
		if (m_mousedOverSlot >= 0 && m_mousedOverSlot <= slotOfIndex)
		{
			widthsToAdd += 1f;
		}
		else if (m_lettuceAbilityReservedSlot >= 0 && index >= m_lettuceAbilityReservedSlot)
		{
			widthsToAdd += 1f;
		}
		int slotsCantAnimate = 0;
		int zonePosition = 0;
		for (int i = 0; i < index; i++)
		{
			zonePosition++;
			Card card = m_cards[i];
			if (!CanAnimateCard(card))
			{
				slotsCantAnimate++;
			}
		}
		float slotPosition = (float)zonePosition + widthsToAdd - (float)slotsCantAnimate;
		float zoneCenterX = firstSlotCenterX + slotPosition * GetSlotWidth();
		return new Vector3(zoneCenterX, zoneCenter.y, zoneCenter.z);
	}

	public override bool CanAcceptTags(int controllerId, TAG_ZONE zoneTag, TAG_CARDTYPE cardType, Entity entity)
	{
		if (!base.CanAcceptTags(controllerId, zoneTag, cardType, entity))
		{
			return false;
		}
		return cardType switch
		{
			TAG_CARDTYPE.MINION => true, 
			TAG_CARDTYPE.LOCATION => true, 
			TAG_CARDTYPE.BATTLEGROUND_SPELL => true, 
			_ => false, 
		};
	}

	public override void UpdateLayout()
	{
		m_updatingLayout++;
		if (IsBlockingLayout())
		{
			UpdateLayoutFinished();
			return;
		}
		if (!GameMgr.Get().IsMercenaries())
		{
			UpdatePlayZoneScale();
		}
		if (InputManager.Get() != null && InputManager.Get().GetHeldCard() == null)
		{
			m_mousedOverSlot = -1;
		}
		if (ZoneMgr.Get().GetLettuceAbilitiesSourceEntity() == null)
		{
			m_lettuceAbilityReservedSlot = -1;
		}
		int cardsAnimated = 0;
		m_cards.Sort(Zone.CardSortComparison);
		float maxTransitionTime = 0f;
		for (int i = 0; i < m_cards.Count; i++)
		{
			Card card = m_cards[i];
			if (!(card == null) && CanAnimateCard(card))
			{
				string tweenLabel = ZoneMgr.Get().GetTweenName<ZonePlay>();
				if (m_Side == Player.Side.OPPOSING)
				{
					iTween.StopOthersByName(card.gameObject, tweenLabel);
				}
				Vector3 scale = base.transform.localScale;
				if ((bool)UniversalInputManager.UsePhoneUI)
				{
					scale *= 1.15f;
				}
				PlayZoneSizeOverride sizeOverride = GameState.Get().GetGameEntity().GetPlayZoneSizeOverride();
				if (sizeOverride != null)
				{
					scale = base.transform.localScale * sizeOverride.m_scale;
				}
				Vector3 position = GetCardPosition(i);
				float delayTime = card.GetTransitionDelay();
				card.SetTransitionDelay(0f);
				ZoneTransitionStyle transitionStyle = card.GetTransitionStyle();
				card.SetTransitionStyle(ZoneTransitionStyle.NORMAL);
				if (transitionStyle == ZoneTransitionStyle.INSTANT || m_transitionStyleOverride == ZoneTransitionStyle.INSTANT)
				{
					iTween.Stop(card.gameObject, includechildren: true);
					card.EnableTransitioningZones(enable: false);
					card.transform.position = position;
					card.transform.rotation = base.transform.rotation;
					card.transform.localScale = scale;
					continue;
				}
				card.EnableTransitioningZones(enable: true);
				cardsAnimated++;
				Hashtable scaleArgs = iTweenManager.Get().GetTweenHashTable();
				scaleArgs.Add("scale", scale);
				scaleArgs.Add("delay", delayTime);
				scaleArgs.Add("time", m_transitionTime);
				scaleArgs.Add("name", tweenLabel);
				iTween.ScaleTo(card.gameObject, scaleArgs);
				Hashtable rotateArgs = iTweenManager.Get().GetTweenHashTable();
				rotateArgs.Add("rotation", base.transform.eulerAngles);
				rotateArgs.Add("delay", delayTime);
				rotateArgs.Add("time", m_transitionTime);
				rotateArgs.Add("name", tweenLabel);
				iTween.RotateTo(card.gameObject, rotateArgs);
				Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
				moveArgs.Add("position", position);
				moveArgs.Add("delay", delayTime);
				moveArgs.Add("time", m_transitionTime);
				moveArgs.Add("name", tweenLabel);
				iTween.MoveTo(card.gameObject, moveArgs);
				maxTransitionTime = Mathf.Max(maxTransitionTime, delayTime + m_transitionTime);
			}
		}
		if (cardsAnimated > 0)
		{
			StartFinishLayoutTimer(maxTransitionTime);
		}
		else
		{
			UpdateLayoutFinished();
		}
	}

	private bool DoesCardNeedSpaceOnBoard(Card card)
	{
		if (card != null)
		{
			if (card.GetZone() is ZonePlay)
			{
				return false;
			}
			Entity entity = card.GetEntity();
			if (entity != null && (entity.IsLocation() || entity.IsMinion()) && GameState.Get().IsValidOption(entity, false))
			{
				return true;
			}
		}
		return false;
	}

	private void UpdatePlayZoneScale()
	{
		GameState gameState = GameState.Get();
		if (gameState == null)
		{
			return;
		}
		Player friendlySidePlayer = gameState.GetFriendlySidePlayer();
		Player opposingSidePlayer = gameState.GetOpposingSidePlayer();
		if (friendlySidePlayer == null || opposingSidePlayer == null)
		{
			return;
		}
		ZonePlay friendlyBattlefield = friendlySidePlayer.GetBattlefieldZone();
		ZonePlay enemyBattlefield = opposingSidePlayer.GetBattlefieldZone();
		if (!(friendlyBattlefield == null) && !(enemyBattlefield == null))
		{
			if (friendlyBattlefield.m_defaultScale == Vector3.zero)
			{
				friendlyBattlefield.m_defaultScale = friendlyBattlefield.gameObject.transform.localScale;
			}
			if (enemyBattlefield.m_defaultScale == Vector3.zero)
			{
				enemyBattlefield.m_defaultScale = enemyBattlefield.gameObject.transform.localScale;
			}
			int maxSize = ((gameState.GetGameEntity() != null) ? gameState.GetGameEntity().GetTag(GAME_TAG.MAX_SLOTS_PER_PLAYER_OVERRIDE) : 0);
			if (maxSize == 0)
			{
				maxSize = m_MaxSlots;
			}
			int heldCardSize = 0;
			if (DoesCardNeedSpaceOnBoard(InputManager.Get().GetHeldCard()))
			{
				heldCardSize = 1;
			}
			maxSize = Mathf.Max(friendlyBattlefield.m_cards.Count + heldCardSize, maxSize);
			maxSize = Mathf.Max(enemyBattlefield.m_cards.Count, maxSize);
			maxSize = Mathf.Max(m_MaxSlots, maxSize);
			friendlyBattlefield.transform.localScale = (float)m_MaxSlots / (float)maxSize * m_defaultScale;
			if (!GameMgr.Get().IsBattlegrounds())
			{
				enemyBattlefield.transform.localScale = (float)m_MaxSlots / (float)maxSize * m_defaultScale;
			}
		}
	}

	protected bool CanAnimateCard(Card card)
	{
		if (card.IsDoNotSort())
		{
			return false;
		}
		return true;
	}
}
