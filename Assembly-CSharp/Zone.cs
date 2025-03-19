using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone : MonoBehaviour
{
	public delegate void UpdateLayoutCompleteCallback(Zone zone, object userData);

	protected class UpdateLayoutCompleteListener : EventListener<UpdateLayoutCompleteCallback>
	{
		public void Fire(Zone zone)
		{
			m_callback(zone, m_userData);
		}
	}

	public TAG_ZONE m_ServerTag;

	public Player.Side m_Side;

	public const float TRANSITION_SEC = 1f;

	protected Player m_controller;

	protected List<Card> m_cards = new List<Card>();

	protected bool m_layoutDirty = true;

	protected int m_updatingLayout;

	protected List<UpdateLayoutCompleteListener> m_completeListeners = new List<UpdateLayoutCompleteListener>();

	protected int m_inputBlockerCount;

	protected int m_layoutBlockerCount;

	public override string ToString()
	{
		return string.Format("{1} {0}", m_ServerTag, m_Side);
	}

	public Player GetController()
	{
		return m_controller;
	}

	public int GetControllerId()
	{
		if (m_controller != null)
		{
			return m_controller.GetPlayerId();
		}
		return 0;
	}

	public void SetController(Player controller)
	{
		m_controller = controller;
	}

	public List<Card> GetCards()
	{
		return m_cards;
	}

	public int GetCardCount()
	{
		return m_cards.Count;
	}

	public virtual int GetLastSlot()
	{
		return GetCardCount();
	}

	public Card GetFirstCard()
	{
		if (m_cards.Count <= 0)
		{
			return null;
		}
		return m_cards[0];
	}

	public Card GetLastCard()
	{
		if (m_cards.Count <= 0)
		{
			return null;
		}
		return m_cards[m_cards.Count - 1];
	}

	public Card GetCardAtIndex(int index)
	{
		if (index < 0)
		{
			return null;
		}
		if (index >= m_cards.Count)
		{
			return null;
		}
		return m_cards[index];
	}

	public virtual Card GetCardAtSlot(int slot)
	{
		return GetCardAtIndex(slot - 1);
	}

	public int GetLastPos()
	{
		return m_cards.Count + 1;
	}

	public int FindCardPos(Card card)
	{
		return 1 + m_cards.FindIndex((Card currCard) => currCard == card);
	}

	public bool ContainsCard(Card card)
	{
		return FindCardPos(card) > 0;
	}

	public bool IsOnlyCard(Card card)
	{
		if (m_cards.Count != 1)
		{
			return false;
		}
		return m_cards[0] == card;
	}

	public void DirtyLayout()
	{
		m_layoutDirty = true;
	}

	public bool IsLayoutDirty()
	{
		return m_layoutDirty;
	}

	public bool IsUpdatingLayout()
	{
		return m_updatingLayout > 0;
	}

	public bool IsInputEnabled()
	{
		return m_inputBlockerCount <= 0;
	}

	public int GetInputBlockerCount()
	{
		return m_inputBlockerCount;
	}

	public void AddInputBlocker()
	{
		AddInputBlocker(1);
	}

	public void RemoveInputBlocker()
	{
		AddInputBlocker(-1);
	}

	public void BlockInput(bool block)
	{
		int count = (block ? 1 : (-1));
		AddInputBlocker(count);
	}

	public void AddInputBlocker(int count)
	{
		int prevCount = m_inputBlockerCount;
		m_inputBlockerCount += count;
		if (prevCount != m_inputBlockerCount && prevCount * m_inputBlockerCount == 0)
		{
			UpdateInput();
		}
	}

	public bool IsBlockingLayout()
	{
		return m_layoutBlockerCount > 0;
	}

	public int GetLayoutBlockerCount()
	{
		return m_layoutBlockerCount;
	}

	public void AddLayoutBlocker()
	{
		m_layoutBlockerCount++;
	}

	public void RemoveLayoutBlocker()
	{
		m_layoutBlockerCount--;
	}

	public bool AddUpdateLayoutCompleteCallback(UpdateLayoutCompleteCallback callback)
	{
		return AddUpdateLayoutCompleteCallback(callback, null);
	}

	public bool AddUpdateLayoutCompleteCallback(UpdateLayoutCompleteCallback callback, object userData)
	{
		UpdateLayoutCompleteListener listener = new UpdateLayoutCompleteListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_completeListeners.Contains(listener))
		{
			return false;
		}
		m_completeListeners.Add(listener);
		return true;
	}

	public bool RemoveUpdateLayoutCompleteCallback(UpdateLayoutCompleteCallback callback)
	{
		return RemoveUpdateLayoutCompleteCallback(callback, null);
	}

	public bool RemoveUpdateLayoutCompleteCallback(UpdateLayoutCompleteCallback callback, object userData)
	{
		UpdateLayoutCompleteListener listener = new UpdateLayoutCompleteListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_completeListeners.Remove(listener);
	}

	public virtual bool CanAcceptTags(int controllerId, TAG_ZONE zoneTag, TAG_CARDTYPE cardType, Entity entity)
	{
		if (m_ServerTag != zoneTag)
		{
			return false;
		}
		if (m_controller != null && m_controller.GetPlayerId() != controllerId)
		{
			return false;
		}
		if (cardType == TAG_CARDTYPE.ENCHANTMENT)
		{
			return false;
		}
		return true;
	}

	public virtual bool AddCard(Card card)
	{
		m_cards.Add(card);
		CorpseCounter.UpdateTextAll();
		DirtyLayout();
		return true;
	}

	public virtual bool InsertCard(int index, Card card)
	{
		m_cards.Insert(index, card);
		DirtyLayout();
		return true;
	}

	public virtual int RemoveCard(Card card)
	{
		if (card == null)
		{
			return -1;
		}
		for (int i = 0; i < m_cards.Count; i++)
		{
			if (m_cards[i] == card)
			{
				m_cards.RemoveAt(i);
				DirtyLayout();
				return i;
			}
		}
		if (!GameState.Get().EntityRemovedFromGame(card.GetEntity().GetEntityId()))
		{
			Debug.LogWarning($"{this}.RemoveCard() - FAILED: {m_controller} tried to remove {card}");
		}
		return -1;
	}

	public virtual void Reset()
	{
		m_cards.Clear();
		m_inputBlockerCount = 0;
		UpdateInput();
	}

	public virtual Transform GetZoneTransformForCard(Card card)
	{
		return base.transform;
	}

	public virtual void UpdateLayout()
	{
		if (m_cards.Count == 0)
		{
			UpdateLayoutFinished();
			return;
		}
		if (GameState.Get().IsMulliganManagerActive())
		{
			UpdateLayoutFinished();
			return;
		}
		m_updatingLayout++;
		if (IsBlockingLayout())
		{
			UpdateLayoutFinished();
			return;
		}
		for (int i = 0; i < m_cards.Count; i++)
		{
			Card card = m_cards[i];
			if (!card.IsDoNotSort())
			{
				card.ShowCard();
				card.EnableTransitioningZones(enable: true);
				Transform transformForCard = GetZoneTransformForCard(card);
				iTween.MoveTo(card.gameObject, transformForCard.position, 1f);
				iTween.RotateTo(card.gameObject, transformForCard.localEulerAngles, 1f);
				iTween.ScaleTo(card.gameObject, transformForCard.localScale, 1f);
			}
		}
		StartFinishLayoutTimer(1f);
	}

	public static int CardSortComparison(Card card1, Card card2)
	{
		int position1 = card1.GetZonePosition();
		int position2 = card2.GetZonePosition();
		if (position1 != position2)
		{
			return position1 - position2;
		}
		position1 = card1.GetEntity().GetZonePosition();
		position2 = card2.GetEntity().GetZonePosition();
		return position1 - position2;
	}

	public virtual void OnHealingDoesDamageEntityMousedOver()
	{
		if (TargetReticleManager.Get().IsActive())
		{
			return;
		}
		foreach (Card card in m_cards)
		{
			if (card.CanPlayHealingDoesDamageHint())
			{
				Spell burstSpell = card.GetActorSpell(SpellType.HEALING_DOES_DAMAGE_HINT_BURST);
				if (burstSpell != null)
				{
					burstSpell.Reactivate();
				}
				Spell idleSpell = card.GetActorSpell(SpellType.HEALING_DOES_DAMAGE_HINT_IDLE);
				if (idleSpell != null)
				{
					idleSpell.ActivateState(SpellStateType.BIRTH);
				}
			}
		}
	}

	public virtual void OnHealingDoesDamageEntityMousedOut()
	{
		foreach (Card card in m_cards)
		{
			if (!card.GetEntity().HasTag(GAME_TAG.HEALING_DOES_DAMAGE_HINT))
			{
				Spell spell = card.GetActorSpell(SpellType.HEALING_DOES_DAMAGE_HINT_IDLE);
				if (!(spell == null) && spell.IsActive())
				{
					spell.ActivateState(SpellStateType.DEATH);
				}
			}
		}
	}

	public virtual void OnHealingDoesDamageEntityEnteredPlay()
	{
		foreach (Card card in m_cards)
		{
			if (card.CanPlayHealingDoesDamageHint())
			{
				Spell burstSpell = card.GetActorSpell(SpellType.HEALING_DOES_DAMAGE_HINT_BURST);
				if (burstSpell != null)
				{
					burstSpell.Reactivate();
				}
			}
		}
	}

	public virtual void OnLifestealDoesDamageEntityMousedOver()
	{
		if (TargetReticleManager.Get().IsActive())
		{
			return;
		}
		foreach (Card card in m_cards)
		{
			if (card.CanPlayLifestealDoesDamageHint())
			{
				Spell burstSpell = card.GetActorSpell(SpellType.HEALING_DOES_DAMAGE_HINT_BURST);
				if (burstSpell != null)
				{
					burstSpell.Reactivate();
				}
				Spell idleSpell = card.GetActorSpell(SpellType.LIFESTEAL_DOES_DAMAGE_HINT_IDLE);
				if (idleSpell != null)
				{
					idleSpell.ActivateState(SpellStateType.BIRTH);
				}
			}
		}
	}

	public virtual void OnLifestealDoesDamageEntityMousedOut()
	{
		foreach (Card card in m_cards)
		{
			if (!card.GetEntity().HasTag(GAME_TAG.LIFESTEAL_DOES_DAMAGE_HINT))
			{
				Spell spell = card.GetActorSpell(SpellType.LIFESTEAL_DOES_DAMAGE_HINT_IDLE);
				if (!(spell == null) && spell.IsActive())
				{
					spell.ActivateState(SpellStateType.DEATH);
				}
			}
		}
	}

	public virtual void OnLifestealDoesDamageEntityEnteredPlay()
	{
		foreach (Card card in m_cards)
		{
			if (card.CanPlayLifestealDoesDamageHint())
			{
				Spell burstSpell = card.GetActorSpell(SpellType.HEALING_DOES_DAMAGE_HINT_BURST);
				if (burstSpell != null)
				{
					burstSpell.Reactivate();
				}
			}
		}
	}

	public virtual void OnSpellPowerEntityEnteredPlay(TAG_SPELL_SCHOOL spellSchool = TAG_SPELL_SCHOOL.NONE)
	{
	}

	public virtual void OnSpellPowerEntityMousedOver(TAG_SPELL_SCHOOL spellSchool = TAG_SPELL_SCHOOL.NONE)
	{
	}

	public virtual void OnSpellPowerEntityMousedOut(TAG_SPELL_SCHOOL spellSchool = TAG_SPELL_SCHOOL.NONE)
	{
	}

	public virtual void OnDiedLastCombatMousedOver()
	{
	}

	public virtual void OnDiedLastCombatMousedOut()
	{
	}

	protected void UpdateInput()
	{
		bool inputEnabled = IsInputEnabled();
		foreach (Card card in m_cards)
		{
			Actor actor = card.GetActor();
			if (!(actor == null))
			{
				actor.ToggleForceIdle(!inputEnabled);
				actor.ToggleCollider(inputEnabled);
				card.UpdateActorState();
			}
		}
		Card mousedOverCard = InputManager.Get().GetMousedOverCard();
		if (inputEnabled && m_cards.Contains(mousedOverCard))
		{
			mousedOverCard.UpdateProposedManaUsage();
		}
	}

	protected void StartFinishLayoutTimer(float delaySec)
	{
		if (delaySec <= Mathf.Epsilon)
		{
			UpdateLayoutFinished();
			return;
		}
		if (m_cards.Find((Card card) => card.IsTransitioningZones()) == null)
		{
			UpdateLayoutFinished();
			return;
		}
		Hashtable tweenArgs = iTweenManager.Get().GetTweenHashTable();
		tweenArgs.Add("time", delaySec);
		tweenArgs.Add("oncomplete", "UpdateLayoutFinished");
		tweenArgs.Add("oncompletetarget", base.gameObject);
		iTween.Timer(base.gameObject, tweenArgs);
	}

	protected void UpdateLayoutFinished()
	{
		for (int i = 0; i < m_cards.Count; i++)
		{
			m_cards[i].EnableTransitioningZones(enable: false);
		}
		m_updatingLayout--;
		m_layoutDirty = false;
		FireUpdateLayoutCompleteCallbacks();
	}

	protected void FireUpdateLayoutCompleteCallbacks()
	{
		if (m_completeListeners.Count != 0)
		{
			UpdateLayoutCompleteListener[] completeListeners = m_completeListeners.ToArray();
			m_completeListeners.Clear();
			for (int i = 0; i < completeListeners.Length; i++)
			{
				completeListeners[i].Fire(this);
			}
		}
	}
}
