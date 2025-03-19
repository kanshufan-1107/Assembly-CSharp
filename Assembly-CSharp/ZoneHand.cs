using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Game.Spells;
using UnityEngine;

public class ZoneHand : Zone
{
	private class ReservedSlotInfo
	{
		private List<int> m_slotsReserved = new List<int>(15);

		public bool IsReservingSlots()
		{
			return m_slotsReserved.Count > 0;
		}

		public List<int> GetReservedSlots()
		{
			return m_slotsReserved;
		}

		public void ReserveSlots(params int[] slots)
		{
			ClearReservedSlots();
			if (slots == null)
			{
				return;
			}
			Array.Sort(slots);
			foreach (int i2 in slots)
			{
				if (i2 >= 0)
				{
					m_slotsReserved.Add(i2);
				}
			}
		}

		public void ClearReservedSlots()
		{
			m_slotsReserved.Clear();
		}
	}

	public GameObject m_iPhoneCardPosition;

	public GameObject m_leftArrow;

	public GameObject m_rightArrow;

	public GameObject m_manaGemPosition;

	public ManaCrystalMgr m_manaGemMgr;

	public GameObject m_playCardButton;

	public GameObject m_iPhonePreviewBone;

	public Float_MobileOverride m_SelectCardOffsetZ;

	public Float_MobileOverride m_SelectCardScale;

	public Float_MobileOverride m_TouchDragResistanceFactorY;

	public TwinspellHoldSpell m_CardWithReplacementsHoldSpell;

	public Vector3 m_enlargedHandPosition;

	public Vector3 m_enlargedHandScale;

	public Vector3 m_enlargedHandCardScale;

	public float m_enlargedHandDefaultCardSpacing;

	public float m_enlargedHandCardMinX;

	public float m_enlargedHandCardMaxX;

	public float m_heroWidthInHand;

	public float m_handHidingDistance;

	public GameObject m_heroHitbox;

	public float m_tinyHandMouseOverYOffset = 4.6f;

	public float m_tinyHandMouseOverZOffset = 3.3f;

	public float m_tinyHandMobileHoverDelaySeconds = 0.25f;

	public const float MOUSE_OVER_SCALE = 1.5f;

	public const float HAND_SCALE = 0.62f;

	public const float HAND_SCALE_Y = 0.125f;

	public const float HAND_SCALE_OPPONENT = 0.682f;

	public const float HAND_SCALE_OPPONENT_Y = 0.125f;

	private const float ANGLE_OF_CARDS = 40f;

	private const float DEFAULT_ANIMATE_TIME = 0.35f;

	private const float DRIFT_AMOUNT = 0.08f;

	private const float Z_ROTATION_ON_LEFT = 352.5f;

	private const float Z_ROTATION_ON_RIGHT = 3f;

	private const float RESISTANCE_BASE = 10f;

	private Card m_lastMousedOverCard;

	private float m_maxWidth;

	private bool m_doNotUpdateLayout = true;

	private Vector3 m_centerOfHand;

	private bool m_enemyHand;

	private ZoneTransitionStyle m_transitionStyleOverride;

	private Card m_overrideFocusCard;

	private bool m_handEnlarged;

	private Vector3 m_startingPosition;

	private Vector3 m_startingScale;

	private bool m_handMoving;

	private bool m_targetingMode;

	private int m_touchedSlot;

	private CardStandIn m_hiddenStandIn;

	private TwinspellHoldSpell m_CardWithReplacementsHoldSpellInstance;

	private int m_playingCardWithReplacementsEntityId = -1;

	private int m_deckActionEntityID = -1;

	private ReservedSlotInfo m_reservedSlotInfo = new ReservedSlotInfo();

	private bool m_replacementCardPreviewDirty;

	private List<CardStandIn> standIns;

	public CardStandIn CurrentStandIn
	{
		get
		{
			if (m_lastMousedOverCard == null)
			{
				return null;
			}
			return GetStandIn(m_lastMousedOverCard);
		}
	}

	public bool IsCardFocused { get; private set; }

	public static event Action<bool> MobileHandStateChange;

	private void Awake()
	{
		m_enemyHand = m_Side == Player.Side.OPPOSING;
		m_startingPosition = base.gameObject.transform.localPosition;
		m_startingScale = base.gameObject.transform.localScale;
		UpdateCenterAndWidth();
	}

	private void Start()
	{
		GameState.Get().RegisterCantPlayListener(OnCantPlay);
		GameState.Get().RegisterOptionsSentListener(OnOptionSent);
		GameState.Get().RegisterOptionsReceivedListener(OnOptionReceived);
	}

	public void SetZoneTransitionStyleOverride(ZoneTransitionStyle zoneTransitionStyle)
	{
		m_transitionStyleOverride = zoneTransitionStyle;
	}

	public void SetOverrideFocusCard(Card card)
	{
		m_overrideFocusCard = card;
	}

	public void ClearOverrideFocusCard()
	{
		m_overrideFocusCard = null;
	}

	public Card GetLastMousedOverCard()
	{
		return m_lastMousedOverCard;
	}

	public bool IsHandScrunched(int cardCount)
	{
		return cardCount > 3;
	}

	public void SetDoNotUpdateLayout(bool enable)
	{
		m_doNotUpdateLayout = enable;
	}

	public bool IsDoNotUpdateLayout()
	{
		return m_doNotUpdateLayout;
	}

	public override void OnSpellPowerEntityEnteredPlay(TAG_SPELL_SCHOOL spellSchool = TAG_SPELL_SCHOOL.NONE)
	{
		foreach (Card card in m_cards)
		{
			if (card.CanPlaySpellPowerHint(spellSchool))
			{
				Spell burstSpell = card.GetActorSpell(SpellType.SPELL_POWER_HINT_BURST);
				if (burstSpell != null)
				{
					burstSpell.Reactivate();
				}
			}
		}
	}

	public override void OnSpellPowerEntityMousedOver(TAG_SPELL_SCHOOL spellSchool = TAG_SPELL_SCHOOL.NONE)
	{
		if (TargetReticleManager.Get().IsActive())
		{
			return;
		}
		foreach (Card card in m_cards)
		{
			if (card.CanPlaySpellPowerHint(spellSchool))
			{
				Spell burstSpell = card.GetActorSpell(SpellType.SPELL_POWER_HINT_BURST);
				if (burstSpell != null)
				{
					burstSpell.Reactivate();
				}
				Spell idleSpell = card.GetActorSpell(SpellType.SPELL_POWER_HINT_IDLE);
				if (idleSpell != null)
				{
					idleSpell.ActivateState(SpellStateType.BIRTH);
				}
			}
		}
	}

	public override void OnSpellPowerEntityMousedOut(TAG_SPELL_SCHOOL spellSchool = TAG_SPELL_SCHOOL.NONE)
	{
		foreach (Card card in m_cards)
		{
			Spell spell = card.GetActorSpell(SpellType.SPELL_POWER_HINT_IDLE);
			if (!(spell == null) && spell.IsActive())
			{
				spell.ActivateState(SpellStateType.DEATH);
			}
		}
	}

	public float GetDefaultCardSpacing(bool handEnlarged)
	{
		if ((bool)UniversalInputManager.UsePhoneUI && handEnlarged)
		{
			return m_enlargedHandDefaultCardSpacing;
		}
		return 1.270804f;
	}

	public int GetVisualCardCount()
	{
		int count = m_cards.Count;
		if (m_reservedSlotInfo.IsReservingSlots())
		{
			count += m_reservedSlotInfo.GetReservedSlots().Count;
		}
		return count;
	}

	public void ReserveCardSlot(params int[] slots)
	{
		m_reservedSlotInfo.ReserveSlots(slots);
	}

	public void SortWithSpotForReservedCard(params int[] slots)
	{
		m_reservedSlotInfo.ReserveSlots(slots);
		UpdateLayout();
	}

	public void ClearReservedCard()
	{
		SortWithSpotForReservedCard(-1);
	}

	public override void UpdateLayout()
	{
		if (!GameState.Get().IsMulliganManagerActive() && !m_enemyHand)
		{
			BlowUpOldStandins();
			for (int i = 0; i < m_cards.Count; i++)
			{
				Card currentCard = m_cards[i];
				CreateCardStandIn(currentCard);
			}
		}
		UpdateLayout(null, forced: true, -1);
	}

	public void ForceStandInUpdate()
	{
		BlowUpOldStandins();
		for (int i = 0; i < m_cards.Count; i++)
		{
			Card currentCard = m_cards[i];
			CreateCardStandIn(currentCard);
		}
	}

	public void UpdateLayout(Card cardMousedOver)
	{
		UpdateLayout(cardMousedOver, forced: false, -1);
	}

	public void UpdateLayout(Card cardMousedOver, bool forced)
	{
		UpdateLayout(cardMousedOver, forced, -1);
	}

	public void UpdateLayout(Card cardMousedOver, bool forced, int overrideCardCount)
	{
		m_updatingLayout++;
		if (IsBlockingLayout())
		{
			UpdateLayoutFinished();
			return;
		}
		for (int i = 0; i < m_cards.Count; i++)
		{
			Card card = m_cards[i];
			if (!card.IsDoNotSort() && card.GetTransitionStyle() != ZoneTransitionStyle.VERY_SLOW && !IsCardNotInEnemyHandAnymore(card) && !card.HasBeenGrabbedByEnemyActionHandler())
			{
				ISpell summonSpell = card.GetBestSummonSpell();
				if (summonSpell == null || !summonSpell.IsActive())
				{
					card.ShowCard();
				}
			}
		}
		if (m_doNotUpdateLayout)
		{
			UpdateLayoutFinished();
			return;
		}
		if (cardMousedOver != null && GetCardSlot(cardMousedOver) < 0)
		{
			cardMousedOver = null;
		}
		if (!forced && cardMousedOver == m_lastMousedOverCard)
		{
			m_updatingLayout--;
			UpdateKeywordPanelsPosition(cardMousedOver);
		}
		else
		{
			m_cards.Sort(Zone.CardSortComparison);
			FocusCard(cardMousedOver, overrideCardCount);
		}
	}

	public void HideCards()
	{
		foreach (Card card in m_cards)
		{
			card.GetActor().gameObject.SetActive(value: false);
		}
	}

	public void ShowCards()
	{
		foreach (Card card in m_cards)
		{
			card.GetActor().gameObject.SetActive(value: true);
		}
	}

	public float GetCardWidth()
	{
		int cardCount = GetVisualCardCount();
		if (!m_enemyHand)
		{
			cardCount -= TurnStartManager.Get().GetNumCardsToDraw();
		}
		float worldWidth = GetCardSpacing(cardCount);
		Vector3 worldLeft = m_centerOfHand;
		worldLeft.x -= worldWidth / 2f;
		Vector3 worldRight = m_centerOfHand;
		worldRight.x += worldWidth / 2f;
		Vector3 screenLeft = Camera.main.WorldToScreenPoint(worldLeft);
		return Camera.main.WorldToScreenPoint(worldRight).x - screenLeft.x;
	}

	public bool TouchReceived()
	{
		if (!UniversalInputManager.Get().GetInputHitInfo(GameLayer.CardRaycast.LayerBit(), out var hitInfo))
		{
			m_touchedSlot = -1;
		}
		CardStandIn standIn = GameObjectUtils.FindComponentInParents<CardStandIn>(hitInfo.transform);
		if (standIn != null)
		{
			m_touchedSlot = GetCardSlot(standIn.linkedCard);
			return true;
		}
		m_touchedSlot = -1;
		return false;
	}

	public Card GetMousedOverCard()
	{
		if (m_overrideFocusCard != null)
		{
			return m_overrideFocusCard;
		}
		UniversalInputManager universalInputManager = UniversalInputManager.Get();
		CardStandIn standIn = null;
		if (!universalInputManager.InputHitAnyObject(Camera.main, GameLayer.InvisibleHitBox1) || !universalInputManager.GetInputHitInfo(Camera.main, GameLayer.CardRaycast, out var hitInfo))
		{
			return null;
		}
		standIn = GameObjectUtils.FindComponentInParents<CardStandIn>(hitInfo.transform);
		if (standIn == null)
		{
			return null;
		}
		return standIn.linkedCard;
	}

	public void HandleInput()
	{
		if (TeammateBoardViewer.Get() != null && TeammateBoardViewer.Get().IsViewingTeammate())
		{
			return;
		}
		Card remoteFriendlyHoveringCard = null;
		RemoteActionHandler remoteActionHandler = RemoteActionHandler.Get();
		if (remoteActionHandler != null)
		{
			Card remoteCard = remoteActionHandler.GetFriendlyHoverCard();
			if (remoteCard != null && remoteCard.GetController().IsFriendlySide() && remoteCard.GetZone() is ZoneHand)
			{
				remoteFriendlyHoveringCard = remoteCard;
			}
		}
		if (UniversalInputManager.Get().IsTouchMode())
		{
			InputManager inputManager = InputManager.Get();
			if (!inputManager.LeftMouseButtonDown || m_touchedSlot < 0)
			{
				m_touchedSlot = -1;
				UpdateLayout(remoteFriendlyHoveringCard);
				return;
			}
			Vector3 mousePosition = InputCollection.GetMousePosition();
			Vector3 lastMousePosition = inputManager.LastMouseDownPosition;
			float dx = mousePosition.x - lastMousePosition.x;
			float dy = Mathf.Max(0f, mousePosition.y - lastMousePosition.y);
			int cardSlot = GetCardSlot(m_lastMousedOverCard);
			float cardWidth = GetCardWidth();
			float currentSlotDX = (float)(cardSlot - m_touchedSlot) * cardWidth;
			float resistance = 10f + dy * (float)m_TouchDragResistanceFactorY;
			dx = ((!(dx < currentSlotDX)) ? Mathf.Max(currentSlotDX, dx - resistance) : Mathf.Min(currentSlotDX, dx + resistance));
			int currentSlot = m_touchedSlot + (int)Math.Truncate(dx / cardWidth);
			Card currentCard = null;
			if (currentSlot >= 0 && currentSlot < m_cards.Count)
			{
				currentCard = m_cards[currentSlot];
			}
			UpdateLayout(currentCard);
		}
		else
		{
			Card cardMousedOver = GetMousedOverCard();
			if (cardMousedOver == null && remoteFriendlyHoveringCard == null)
			{
				UpdateLayout(null);
			}
			else if (cardMousedOver == m_lastMousedOverCard)
			{
				UpdateKeywordPanelsPosition(cardMousedOver);
			}
			else if (cardMousedOver == null && remoteFriendlyHoveringCard != null)
			{
				UpdateLayout(remoteFriendlyHoveringCard);
			}
			else
			{
				UpdateLayout(cardMousedOver);
			}
		}
	}

	private void UpdateKeywordPanelsPosition(Card cardMousedOver)
	{
		if (!(cardMousedOver == null) && IsCardFocused && ShouldShowTooltips(cardMousedOver))
		{
			TooltipPanelManager.TooltipBoneSource boneSource = (ShouldShowCardTooltipOnRight(cardMousedOver) ? TooltipPanelManager.TooltipBoneSource.TOP_RIGHT : TooltipPanelManager.TooltipBoneSource.TOP_LEFT);
			TooltipPanelManager.Get().UpdateKeywordPanelsPosition(cardMousedOver, boneSource);
		}
	}

	public bool ShouldShowCardTooltipOnRight(Card card)
	{
		if (GameState.Get().IsMulliganManagerActive())
		{
			int halfCardCount = (int)Mathf.Ceil((float)card.GetZone().GetCardCount() / 2f);
			return card.GetZonePosition() <= halfCardCount;
		}
		if (InputManager.Get().HasPlayFromMiniHandEnabled() && (bool)UniversalInputManager.UsePhoneUI && !m_handEnlarged)
		{
			return false;
		}
		Entity entity = card.GetEntity();
		if (entity.HasTag(GAME_TAG.COLOSSAL))
		{
			return entity.HasTag(GAME_TAG.COLOSSAL_LIMB_ON_LEFT);
		}
		if (card.GetActor() == null || card.GetActor().GetMeshRenderer() == null)
		{
			return false;
		}
		int zonePosition = card.GetZonePosition();
		float middle = (float)(GetCardCount() + 1) / 2f;
		bool tooltipsShouldGoRight = (float)zonePosition <= middle;
		if (entity.HasTag(GAME_TAG.DISPLAY_CARD_ON_MOUSEOVER) || entity.IsHero())
		{
			tooltipsShouldGoRight = !tooltipsShouldGoRight;
		}
		return tooltipsShouldGoRight;
	}

	public void ShowManaGems()
	{
		Vector3 position = m_manaGemPosition.transform.position;
		position.x += -0.5f * m_manaGemMgr.GetWidth();
		m_manaGemMgr.gameObject.transform.position = position;
		m_manaGemMgr.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
	}

	public void HideManaGems()
	{
		m_manaGemMgr.transform.position = new Vector3(0f, 0f, 0f);
	}

	public void SetHandEnlarged(bool enlarged)
	{
		m_handEnlarged = enlarged;
		if (enlarged)
		{
			base.gameObject.transform.localPosition = m_enlargedHandPosition;
			base.gameObject.transform.localScale = m_enlargedHandScale;
			ManaCrystalMgr.Get().ShowPhoneManaTray();
		}
		else
		{
			base.gameObject.transform.localPosition = m_startingPosition;
			base.gameObject.transform.localScale = m_startingScale;
			ManaCrystalMgr.Get().HidePhoneManaTray();
		}
		UpdateCenterAndWidth();
		m_handMoving = true;
		UpdateLayout(null, forced: true);
		m_handMoving = false;
		ZoneHand.MobileHandStateChange?.Invoke(enlarged);
	}

	public bool HandEnlarged()
	{
		return m_handEnlarged;
	}

	public void SetFriendlyHeroTargetingMode(bool enable)
	{
		if (!enable && m_hiddenStandIn != null)
		{
			m_hiddenStandIn.gameObject.SetActive(value: true);
		}
		if (m_targetingMode == enable)
		{
			return;
		}
		m_targetingMode = enable;
		m_heroHitbox.SetActive(enable);
		if (!m_handEnlarged)
		{
			return;
		}
		if (enable)
		{
			m_hiddenStandIn = CurrentStandIn;
			if (m_hiddenStandIn != null)
			{
				m_hiddenStandIn.gameObject.SetActive(value: false);
			}
			Vector3 position = m_enlargedHandPosition;
			position.z -= m_handHidingDistance;
			base.gameObject.transform.localPosition = position;
		}
		else
		{
			base.gameObject.transform.localPosition = m_enlargedHandPosition;
		}
		UpdateCenterAndWidth();
	}

	private bool ShouldShowTooltips(Card card)
	{
		if (card == null)
		{
			return false;
		}
		if (card.GetEntity() == null)
		{
			return true;
		}
		if (card.GetEntity().GetTag<TAG_PREMIUM>(GAME_TAG.PREMIUM) == TAG_PREMIUM.SIGNATURE)
		{
			return true;
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			if (m_handEnlarged)
			{
				if (!card.ShouldShowCardsInTooltip())
				{
					return true;
				}
				int zonePosition = card.GetZonePosition();
				return Mathf.Abs((float)(m_cards.Count + 1) / 2f - (float)zonePosition) <= 0.5f;
			}
			return !card.ShouldShowCardsInTooltip();
		}
		return true;
	}

	private void FocusCard(Card cardMousedOver, int overrideCardCount)
	{
		if (m_lastMousedOverCard != cardMousedOver && m_lastMousedOverCard != null)
		{
			if (!InputManager.Get().HasPlayFromMiniHandEnabled() || IsCardFocused)
			{
				IsCardFocused = false;
				if (CanAnimateCard(m_lastMousedOverCard) && GetCardSlot(m_lastMousedOverCard) >= 0)
				{
					iTween.Stop(m_lastMousedOverCard.gameObject);
					if (!m_enemyHand)
					{
						Vector3 oldPosition = GetMouseOverCardPosition(m_lastMousedOverCard);
						Vector3 newerPosition = GetCardPosition(m_lastMousedOverCard, overrideCardCount);
						m_lastMousedOverCard.transform.position = new Vector3(oldPosition.x, m_centerOfHand.y, newerPosition.z + 0.5f);
						m_lastMousedOverCard.transform.localScale = GetCardScale();
						m_lastMousedOverCard.transform.localEulerAngles = GetCardRotation(m_lastMousedOverCard);
					}
					GameLayer layer = GameLayer.Default;
					if (m_Side == Player.Side.OPPOSING && m_controller.IsRevealed())
					{
						layer = GameLayer.CardRaycast;
					}
					LayerUtils.SetLayer(m_lastMousedOverCard.gameObject, layer);
				}
			}
			m_lastMousedOverCard.NotifyMousedOut(m_enemyHand);
		}
		int cardsAnimated = 0;
		float maxTransitionTime = 0f;
		for (int i = 0; i < m_cards.Count; i++)
		{
			Card currentCard = m_cards[i];
			if (!CanAnimateCard(currentCard))
			{
				continue;
			}
			cardsAnimated++;
			currentCard.transform.rotation = Quaternion.Euler(new Vector3(currentCard.transform.localEulerAngles.x, currentCard.transform.localEulerAngles.y, 352.5f));
			float animateTime = 0.5f;
			if (m_handMoving)
			{
				animateTime = 0.25f;
			}
			if (m_enemyHand)
			{
				animateTime = 1.5f;
			}
			float scaleTime = 0.25f;
			iTween.EaseType easeTypeToUse = iTween.EaseType.easeOutExpo;
			float delayTime = currentCard.GetTransitionDelay();
			currentCard.SetTransitionDelay(0f);
			ZoneTransitionStyle transitionStyle = currentCard.GetTransitionStyle();
			currentCard.SetTransitionStyle(ZoneTransitionStyle.NORMAL);
			switch (transitionStyle)
			{
			case ZoneTransitionStyle.SLOW:
				easeTypeToUse = iTween.EaseType.easeInExpo;
				scaleTime = animateTime;
				goto default;
			case ZoneTransitionStyle.VERY_SLOW:
				easeTypeToUse = iTween.EaseType.easeInOutCubic;
				scaleTime = 1f;
				animateTime = 1f;
				goto default;
			case ZoneTransitionStyle.FAST:
				scaleTime = 0.35f;
				animateTime = 0.35f;
				goto default;
			default:
				currentCard.GetActor().TurnOnCollider();
				break;
			case ZoneTransitionStyle.NORMAL:
				break;
			}
			Vector3 newPosition = GetCardPosition(currentCard, overrideCardCount);
			Vector3 newRotation = GetCardRotation(currentCard, overrideCardCount);
			Vector3 newScale = GetCardScale();
			if (currentCard == cardMousedOver && ShouldCheckTapWhenClickingMiniHand())
			{
				IsCardFocused = true;
				easeTypeToUse = iTween.EaseType.easeOutExpo;
				if (m_enemyHand)
				{
					scaleTime = 0.15f;
					float zOffset = 0.3f;
					newPosition = new Vector3(newPosition.x, newPosition.y, newPosition.z - zOffset);
				}
				else
				{
					float newXScale = m_SelectCardScale;
					float newZScale = m_SelectCardScale;
					newRotation = new Vector3(0f, 0f, 0f);
					newScale = new Vector3(newXScale, newScale.y, newZScale);
					currentCard.transform.localScale = newScale;
					float driftAmount = 0.1f;
					newPosition = GetMouseOverCardPosition(currentCard);
					float prevX = newPosition.x;
					if ((bool)UniversalInputManager.UsePhoneUI)
					{
						newPosition.x = Mathf.Max(newPosition.x, m_enlargedHandCardMinX);
						newPosition.x = Mathf.Min(newPosition.x, m_enlargedHandCardMaxX);
					}
					currentCard.transform.position = new Vector3((prevX != newPosition.x) ? newPosition.x : currentCard.transform.position.x, newPosition.y, newPosition.z - driftAmount);
					currentCard.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
					iTween.Stop(currentCard.gameObject);
					easeTypeToUse = iTween.EaseType.easeOutExpo;
					if ((bool)CardTypeBanner.Get())
					{
						CardTypeBanner.Get().Show(currentCard);
					}
					InputManager.Get().SetMousedOverCard(currentCard);
					if (ShouldShowTooltips(currentCard))
					{
						TooltipPanelManager.TooltipBoneSource boneSource = (ShouldShowCardTooltipOnRight(currentCard) ? TooltipPanelManager.TooltipBoneSource.TOP_RIGHT : TooltipPanelManager.TooltipBoneSource.TOP_LEFT);
						TooltipPanelManager.Get().UpdateKeywordHelp(currentCard, currentCard.GetActor(), boneSource, null, null);
					}
					if ((bool)UniversalInputManager.UsePhoneUI && GameMgr.Get().IsBattlegroundDuoGame() && TeammatePingWheelManager.Get() != null && TeammatePingWheelManager.Get().GetActorWithActivePingWheel() == currentCard.GetActor())
					{
						TeammatePingWheelManager.Get().HidePingOptions(currentCard.GetActor());
					}
					LayerUtils.SetLayer(currentCard.gameObject, GameLayer.Tooltip);
				}
			}
			else if (GetStandIn(currentCard) != null)
			{
				CardStandIn standIn = GetStandIn(currentCard);
				iTween.Stop(standIn.gameObject);
				standIn.transform.position = newPosition;
				standIn.transform.localEulerAngles = newRotation;
				standIn.transform.localScale = newScale;
				if (!currentCard.CardStandInIsInteractive())
				{
					standIn.DisableStandIn();
				}
				else
				{
					standIn.EnableStandIn();
				}
			}
			if (transitionStyle == ZoneTransitionStyle.INSTANT || m_transitionStyleOverride == ZoneTransitionStyle.INSTANT)
			{
				iTween.Stop(currentCard.gameObject, includechildren: true);
				currentCard.EnableTransitioningZones(enable: false);
				currentCard.transform.position = newPosition;
				currentCard.transform.localEulerAngles = newRotation;
				currentCard.transform.localScale = newScale;
				continue;
			}
			currentCard.EnableTransitioningZones(enable: true);
			string tweenLabel = ZoneMgr.Get().GetTweenName<ZoneHand>();
			Hashtable tweenScaleArgs = iTweenManager.Get().GetTweenHashTable();
			tweenScaleArgs.Add("scale", newScale);
			tweenScaleArgs.Add("delay", delayTime);
			tweenScaleArgs.Add("time", scaleTime);
			tweenScaleArgs.Add("easetype", easeTypeToUse);
			tweenScaleArgs.Add("name", tweenLabel);
			iTween.ScaleTo(currentCard.gameObject, tweenScaleArgs);
			Hashtable tweenRotateArgs = iTweenManager.Get().GetTweenHashTable();
			tweenRotateArgs.Add("rotation", newRotation);
			tweenRotateArgs.Add("delay", delayTime);
			tweenRotateArgs.Add("time", scaleTime);
			tweenRotateArgs.Add("easetype", easeTypeToUse);
			tweenRotateArgs.Add("name", tweenLabel);
			iTween.RotateTo(currentCard.gameObject, tweenRotateArgs);
			Hashtable tweenMoveArgs = iTweenManager.Get().GetTweenHashTable();
			tweenMoveArgs.Add("position", newPosition);
			tweenMoveArgs.Add("delay", delayTime);
			tweenMoveArgs.Add("time", animateTime);
			tweenMoveArgs.Add("easetype", easeTypeToUse);
			tweenMoveArgs.Add("name", tweenLabel);
			iTween.MoveTo(currentCard.gameObject, tweenMoveArgs);
			maxTransitionTime = Mathf.Max(maxTransitionTime, delayTime + animateTime, delayTime + scaleTime);
		}
		m_lastMousedOverCard = cardMousedOver;
		if (cardsAnimated > 0)
		{
			StartFinishLayoutTimer(maxTransitionTime);
		}
		else
		{
			UpdateLayoutFinished();
		}
	}

	private bool ShouldCheckTapWhenClickingMiniHand()
	{
		if (!InputManager.Get().HasPlayFromMiniHandEnabled())
		{
			return true;
		}
		if (!UniversalInputManager.UsePhoneUI)
		{
			return true;
		}
		if (m_handEnlarged)
		{
			return true;
		}
		if (!InputManager.Get().WaitingForTouchDelay())
		{
			return true;
		}
		return false;
	}

	private void CreateCardStandIn(Card card)
	{
		Actor actor = card.GetActor();
		if (actor != null && actor.GetMeshRenderer() != null)
		{
			actor.GetMeshRenderer().gameObject.layer = 0;
		}
		GameObject obj = AssetLoader.Get().InstantiatePrefab("Card_Collider_Standin.prefab:06f88b48f6884bf4cafbd6696a28ede4", AssetLoadingOptions.IgnorePrefabPosition);
		obj.transform.localEulerAngles = GetCardRotation(card);
		obj.transform.position = GetCardPosition(card);
		obj.transform.localScale = GetCardScale();
		CardStandIn standIn = obj.GetComponent<CardStandIn>();
		standIn.linkedCard = card;
		standIns.Add(standIn);
		if (!standIn.linkedCard.CardStandInIsInteractive())
		{
			standIn.DisableStandIn();
		}
	}

	public CardStandIn GetStandIn(Card card)
	{
		if (standIns == null)
		{
			return null;
		}
		foreach (CardStandIn standIn in standIns)
		{
			if (!(standIn == null) && standIn.linkedCard == card)
			{
				return standIn;
			}
		}
		return null;
	}

	public void MakeStandInInteractive(Card card)
	{
		if (!(GetStandIn(card) == null))
		{
			GetStandIn(card).EnableStandIn();
		}
	}

	private void BlowUpOldStandins()
	{
		if (standIns == null)
		{
			standIns = new List<CardStandIn>();
			return;
		}
		foreach (CardStandIn standIn in standIns)
		{
			if (!(standIn == null))
			{
				UnityEngine.Object.Destroy(standIn.gameObject);
			}
		}
		standIns = new List<CardStandIn>();
	}

	public int GetCardSlot(Card card)
	{
		int slot = m_cards.IndexOf(card);
		if (m_reservedSlotInfo.IsReservingSlots())
		{
			foreach (int reservedSlot in m_reservedSlotInfo.GetReservedSlots())
			{
				if (reservedSlot <= slot)
				{
					slot++;
				}
			}
		}
		return slot;
	}

	public void CalcCardCountAndScrunch(int overrideCardCount, out int cardCount, out bool isHandScrunched)
	{
		if (overrideCardCount >= 0)
		{
			cardCount = overrideCardCount;
		}
		else
		{
			cardCount = GetVisualCardCount();
		}
		isHandScrunched = IsHandScrunched(cardCount);
		if (!m_enemyHand)
		{
			cardCount -= TurnStartManager.Get().GetNumCardsToDraw();
		}
	}

	public void CalcRotationY(int index, int cardCount, bool isHandScrunched, out float rotationY)
	{
		rotationY = 0f;
		if (m_enemyHand && m_controller.IsRevealed())
		{
			rotationY += 180f;
		}
		if (cardCount > 1 && isHandScrunched)
		{
			float adjustedRotationAngle = 40f;
			if (!m_enemyHand)
			{
				adjustedRotationAngle += (float)(cardCount * 2);
			}
			float rotationDifference = adjustedRotationAngle / (float)(cardCount - 1);
			float rotationOffset = (0f - adjustedRotationAngle) / 2f;
			if (m_enemyHand)
			{
				rotationY -= rotationDifference * (float)index + rotationOffset;
			}
			else
			{
				rotationY += rotationDifference * (float)index + rotationOffset;
			}
		}
	}

	public Vector3 GetCardPosition(Card card)
	{
		return GetCardPosition(card, -1);
	}

	public Vector3 GetCardPosition(Card card, int overrideCardCount)
	{
		return GetCardPosition(GetCardSlot(card), overrideCardCount);
	}

	public Vector3 GetCardPosition(int index, int overrideCardCount)
	{
		int cardCount = 0;
		bool isHandScrunched = false;
		CalcCardCountAndScrunch(overrideCardCount, out cardCount, out isHandScrunched);
		float halfCardSpacing = GetCardSpacing(cardCount) / 2f;
		float newX = m_centerOfHand.x;
		newX += (float)(index * 2 - (cardCount - 1)) * halfCardSpacing;
		if (m_handEnlarged && m_targetingMode)
		{
			if (cardCount % 2 <= 0)
			{
				newX = ((index >= cardCount / 2) ? (newX + m_heroWidthInHand / 2f) : (newX - m_heroWidthInHand / 2f));
			}
			else if (index < (cardCount + 1) / 2)
			{
				newX -= m_heroWidthInHand;
			}
		}
		float newY = m_centerOfHand.y;
		float newZ = m_centerOfHand.z;
		float zCurving = Mathf.Pow((float)index + 0.5f - (float)cardCount / 2f, 2f) / (6f * (float)cardCount);
		if (!isHandScrunched)
		{
			zCurving = 0f;
		}
		float newDegreesY = 0f;
		CalcRotationY(index, cardCount, isHandScrunched, out newDegreesY);
		float zOffset = 0f;
		if ((m_enemyHand && newDegreesY < 0f) || (!m_enemyHand && newDegreesY > 0f))
		{
			zOffset = Mathf.Sin(Mathf.Abs(newDegreesY * ((float)Math.PI / 180f))) * halfCardSpacing;
		}
		newZ = ((!m_enemyHand) ? (newZ - (zCurving + zOffset)) : (newZ + (zCurving + zOffset)));
		if (m_enemyHand && m_controller.IsRevealed())
		{
			newZ -= 0.2f;
		}
		return new Vector3(newX, newY, newZ);
	}

	public Vector3 GetCardRotation(Card card)
	{
		return GetCardRotation(card, -1);
	}

	public Vector3 GetCardRotation(Card card, int overrideCardCount)
	{
		return GetCardRotation(GetCardSlot(card), overrideCardCount);
	}

	public Vector3 GetCardRotation(int index, int overrideCardCount)
	{
		int cardCount = 0;
		bool isHandScrunched = false;
		CalcCardCountAndScrunch(overrideCardCount, out cardCount, out isHandScrunched);
		float newRotationY = 0f;
		CalcRotationY(index, cardCount, isHandScrunched, out newRotationY);
		return new Vector3(0f, newRotationY, 352.5f);
	}

	public Vector3 GetCardScale()
	{
		if (m_enemyHand)
		{
			return new Vector3(0.682f, 0.125f, 0.682f);
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			return m_enlargedHandCardScale;
		}
		return new Vector3(0.62f, 0.125f, 0.62f);
	}

	private Vector3 GetMouseOverCardPosition(Card card)
	{
		Vector3 cardPosition = GetCardPosition(card);
		bool tinyHand = (bool)UniversalInputManager.UsePhoneUI && !m_handEnlarged;
		return new Vector3(cardPosition.x, m_centerOfHand.y + 1f + (tinyHand ? m_tinyHandMouseOverYOffset : 0f), base.transform.Find("MouseOverCardHeight").position.z + (float)m_SelectCardOffsetZ + (tinyHand ? m_tinyHandMouseOverZOffset : 0f));
	}

	private float GetCardSpacing(int cardCount)
	{
		float spacing = GetDefaultCardSpacing(m_handEnlarged);
		float num = spacing * (float)cardCount;
		float maxWidth = MaxHandWidth();
		if (num > maxWidth)
		{
			spacing = maxWidth / (float)cardCount;
		}
		return spacing;
	}

	private float MaxHandWidth()
	{
		float maxWidth = m_maxWidth;
		if (m_handEnlarged && m_targetingMode)
		{
			maxWidth -= m_heroWidthInHand;
		}
		return maxWidth;
	}

	protected bool CanAnimateCard(Card card)
	{
		bool doFaceDownCardLog = m_enemyHand && card.GetPrevZone() is ZonePlay;
		if (card.IsDoNotSort())
		{
			if (doFaceDownCardLog)
			{
				Log.FaceDownCard.Print("ZoneHand.CanAnimateCard() - card={0} FAILED card.IsDoNotSort()", card);
			}
			return false;
		}
		if (!card.IsActorReady())
		{
			if (doFaceDownCardLog)
			{
				Log.FaceDownCard.Print("ZoneHand.CanAnimateCard() - card={0} FAILED !card.IsActorReady()", card);
			}
			return false;
		}
		if (m_controller.IsFriendlySide() && (bool)TurnStartManager.Get() && TurnStartManager.Get().IsCardDrawHandled(card))
		{
			return false;
		}
		if (IsCardNotInEnemyHandAnymore(card))
		{
			if (doFaceDownCardLog)
			{
				Log.FaceDownCard.Print("ZoneHand.CanAnimateCard() - card={0} FAILED IsCardNotInEnemyHandAnymore()", card);
			}
			return false;
		}
		if (card.HasBeenGrabbedByEnemyActionHandler())
		{
			if (doFaceDownCardLog)
			{
				Log.FaceDownCard.Print("ZoneHand.CanAnimateCard() - card={0} FAILED card.HasBeenGrabbedByEnemyActionHandler()", card);
			}
			return false;
		}
		if (m_deckActionEntityID == card.GetEntity().GetEntityId())
		{
			return false;
		}
		return true;
	}

	private bool IsCardNotInEnemyHandAnymore(Card card)
	{
		if (card.GetEntity().GetZone() != TAG_ZONE.HAND)
		{
			return m_enemyHand;
		}
		return false;
	}

	public void UpdateCenterAndWidth()
	{
		Collider zoneHandCollider = GetComponent<Collider>();
		m_centerOfHand = zoneHandCollider.bounds.center;
		m_maxWidth = zoneHandCollider.bounds.size.x;
	}

	public void OnCardGrabbed(Card card)
	{
		Entity entity = card.GetEntity();
		if (entity == null)
		{
			return;
		}
		Player player = entity.GetController();
		if (player == null)
		{
			return;
		}
		if (InputManager.Get().HasPlayFromMiniHandEnabled())
		{
			card.transform.localEulerAngles = Vector3.zero;
		}
		if ((player.HasTag(GAME_TAG.HEALING_DOES_DAMAGE) && card.CanPlayHealingDoesDamageHint()) || (player.HasTag(GAME_TAG.LIFESTEAL_DAMAGES_OPPOSING_HERO) && card.CanPlayLifestealDoesDamageHint()))
		{
			Spell burstSpell = card.GetActorSpell(SpellType.HEALING_DOES_DAMAGE_HINT_BURST);
			if (burstSpell != null)
			{
				burstSpell.Reactivate();
			}
		}
	}

	public void OnCardHeld(Card heldCard)
	{
		if (!(heldCard == null) && heldCard.GetEntity() != null && heldCard.GetEntity().HasReplacementsWhenPlayed())
		{
			OnCardWithReplacementsWhenPlayedHeld(heldCard);
		}
	}

	private void OnCardWithReplacementsWhenPlayedHeld(Card heldCard)
	{
		if (m_CardWithReplacementsHoldSpellInstance == null)
		{
			m_CardWithReplacementsHoldSpellInstance = (TwinspellHoldSpell)SpellManager.Get().GetSpell(m_CardWithReplacementsHoldSpell);
			m_replacementCardPreviewDirty = true;
		}
		if (m_replacementCardPreviewDirty)
		{
			m_CardWithReplacementsHoldSpellInstance.Initialize(heldCard.GetEntity().GetEntityId(), heldCard.GetZonePosition());
			m_replacementCardPreviewDirty = false;
		}
		heldCard.GetActor().ToggleForceIdle(bOn: true);
		heldCard.UpdateActorState();
		SpellUtils.ActivateBirthIfNecessary(m_CardWithReplacementsHoldSpellInstance);
	}

	public void DirtyReplacementsWhenPlayedHintSpell()
	{
		m_replacementCardPreviewDirty = true;
	}

	public void OnCardWithReplacementsPlayed(Card playedCard)
	{
		if (playedCard.GetEntity().HasReplacementsWhenPlayed())
		{
			playedCard.GetActor().ToggleForceIdle(bOn: false);
			playedCard.UpdateActorState();
			ReserveCardSlot(GetCardSlot(playedCard));
			m_playingCardWithReplacementsEntityId = playedCard.GetEntity().GetEntityId();
			if (m_CardWithReplacementsHoldSpellInstance != null)
			{
				m_CardWithReplacementsHoldSpellInstance.ActivateState(SpellStateType.ACTION);
			}
			ActivateCardWithReplacementsDeath();
			m_replacementCardPreviewDirty = true;
		}
	}

	public void OnCardWithReplacementsDropped(Card droppedCard)
	{
		if (droppedCard.GetEntity().HasReplacementsWhenPlayed())
		{
			ActivateCardWithReplacementsDeath();
			droppedCard.GetActor().ToggleForceIdle(bOn: false);
			droppedCard.UpdateActorState();
			m_replacementCardPreviewDirty = true;
		}
	}

	public void ActivateCardWithReplacementsDeath()
	{
		if (m_CardWithReplacementsHoldSpellInstance != null)
		{
			SpellUtils.ActivateDeathIfNecessary(m_CardWithReplacementsHoldSpellInstance);
		}
		m_playingCardWithReplacementsEntityId = -1;
		m_replacementCardPreviewDirty = true;
	}

	public bool IsCardWithReplacementsBeingPlayed(Entity cardWithReplacementsEntity)
	{
		if (cardWithReplacementsEntity == null)
		{
			return false;
		}
		return cardWithReplacementsEntity.GetEntityId() == m_playingCardWithReplacementsEntityId;
	}

	private void OnCantPlay(Entity entity, object userData)
	{
		if (!entity.IsControlledByFriendlySidePlayer())
		{
			return;
		}
		if (entity.HasReplacementsWhenPlayed())
		{
			ActivateCardWithReplacementsDeath();
			ClearReservedCard();
		}
		if (!entity.IsMinion())
		{
			return;
		}
		Card card = entity.GetCard();
		if (card != null)
		{
			MagneticPlayData playData = card.GetMagneticPlayData();
			if (playData != null)
			{
				SpellUtils.ActivateDeathIfNecessary(playData.m_beamSpell);
			}
			ZoneMgr.Get().FindZoneOfType<ZonePlay>(Player.Side.FRIENDLY).OnMagneticDropped(entity.GetCard());
		}
	}

	private void OnOptionSent(Network.Options.Option option, object userData)
	{
		if (option.Main.IsDeckActionOption())
		{
			m_deckActionEntityID = option.Main.Targets[0].ID;
		}
	}

	private void OnOptionReceived(object userData)
	{
		ClearDeckActionEntity();
	}

	public void ClearDeckActionEntity()
	{
		m_deckActionEntityID = -1;
	}

	public override bool AddCard(Card card)
	{
		return base.AddCard(card);
	}
}
