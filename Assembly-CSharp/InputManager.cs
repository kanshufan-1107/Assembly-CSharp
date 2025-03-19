using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Game.Spells;
using Blizzard.T5.Services;
using Hearthstone;
using UnityEngine;

public class InputManager : MonoBehaviour
{
	public delegate bool TooltipContentDelegate(ref string headline, ref string description, int index);

	public delegate void OnTooltipShownDelegate(Action<string, string> showRegularTooltip);

	public delegate void OnTooltipHiddenDelegate();

	public class TooltipSettings
	{
		private TooltipContentDelegate m_overrideContentDelegate;

		private OnTooltipShownDelegate m_onTooltipShown;

		private OnTooltipHiddenDelegate m_onTooltipHidden;

		private int m_tooltipCount = 1;

		public bool Allowed { get; private set; }

		public bool IsCustomHandler { get; private set; }

		public TooltipSettings()
		{
		}

		public TooltipSettings(bool allowed)
		{
			Allowed = allowed;
			m_overrideContentDelegate = null;
		}

		public TooltipSettings(bool allowed, TooltipContentDelegate contentDelegate, int tooltipCount = 1)
		{
			Allowed = allowed;
			m_overrideContentDelegate = contentDelegate;
			m_tooltipCount = tooltipCount;
		}

		public static TooltipSettings CreateCustomHandler(OnTooltipShownDelegate onTooltipShown, OnTooltipHiddenDelegate onTooltipHidden)
		{
			return new TooltipSettings
			{
				IsCustomHandler = true,
				m_onTooltipShown = onTooltipShown,
				m_onTooltipHidden = onTooltipHidden
			};
		}

		public void FireOnTooltipShown(Action<string, string> showRegularTooltip)
		{
			m_onTooltipShown?.Invoke(showRegularTooltip);
		}

		public void FireOnTooltipHidden()
		{
			m_onTooltipHidden?.Invoke();
		}

		public int GetTooltipCount()
		{
			return m_tooltipCount;
		}

		public bool GetTooltipOverrideContent(ref string headline, ref string description, int index = 0)
		{
			if (m_overrideContentDelegate != null)
			{
				return m_overrideContentDelegate(ref headline, ref description, index);
			}
			return false;
		}
	}

	public class ZoneTooltipSettings
	{
		public TooltipSettings EnemyHand = new TooltipSettings(allowed: true);

		public TooltipSettings EnemyDeck = new TooltipSettings(allowed: true);

		public TooltipSettings EnemyMana = new TooltipSettings(allowed: true);

		public TooltipSettings FriendlyHand = new TooltipSettings(allowed: true);

		public TooltipSettings FriendlyDeck = new TooltipSettings(allowed: true);

		public TooltipSettings FriendlyMana = new TooltipSettings(allowed: true);

		public bool ShowFriendlyHandWhenHoveringOpponentDeck;
	}

	private class RaycastHitComparer : IComparer<RaycastHit>
	{
		public int Compare(RaycastHit hit1, RaycastHit hit2)
		{
			float y1 = hit1.point.y;
			float y2 = hit2.point.y;
			return y2.CompareTo(y1);
		}
	}

	public delegate void PhoneHandShownCallback(object userData);

	private class PhoneHandShownListener : EventListener<PhoneHandShownCallback>
	{
		public void Fire()
		{
			m_callback(m_userData);
		}
	}

	public delegate void PhoneHandHiddenCallback(object userData);

	private class PhoneHandHiddenListener : EventListener<PhoneHandHiddenCallback>
	{
		public void Fire()
		{
			m_callback(m_userData);
		}
	}

	public float m_MouseOverDelay = 0.4f;

	public float m_TouchHoldDuration = 0.1f;

	public float m_mulliganTouchThreshold = 0.1f;

	public DragRotatorInfo m_DragRotatorInfo = new DragRotatorInfo
	{
		m_PitchInfo = new DragRotatorAxisInfo
		{
			m_ForceMultiplier = 25f,
			m_MinDegrees = -40f,
			m_MaxDegrees = 40f,
			m_RestSeconds = 2f
		},
		m_RollInfo = new DragRotatorAxisInfo
		{
			m_ForceMultiplier = 25f,
			m_MinDegrees = -45f,
			m_MaxDegrees = 45f,
			m_RestSeconds = 2f
		}
	};

	private readonly PlatformDependentValue<float> MIN_GRAB_Y = new PlatformDependentValue<float>(PlatformCategory.Screen)
	{
		Tablet = 80f,
		Phone = 80f
	};

	private const float MOBILE_TARGETTING_Y_OFFSET = 0.8f;

	private const float MOBILE_TARGETTING_XY_SCALE = 1.08f;

	private static InputManager s_instance;

	private UniversalInputManager m_universalInputManager;

	private GameState m_gameState;

	private TargetReticleManager m_targetReticleManager;

	private ZoneHand m_myHandZone;

	private ZonePlay m_myPlayZone;

	private ZoneWeapon m_myWeaponZone;

	private ZoneHand m_enemyHandZone;

	private ZonePlay m_enemyPlayZone;

	private Card m_heldCard;

	private bool m_checkForInput;

	private GameObject m_lastObjectMousedDown;

	private GameObject m_lastObjectRightMousedDown;

	private GameObject m_lastObjectTwoFingerDown;

	private Vector3 m_lastMouseDownPosition;

	private bool m_leftMouseButtonIsDown;

	private bool m_dragging;

	private bool m_lastInputDrag;

	private Card m_mousedOverCard;

	private GameObject m_mousedOverObject;

	private float m_mousedOverTimer;

	private bool m_heldCardWasInDeckActionAreaLastFrame;

	private bool m_hadPendingChoiceTargetLastFrame;

	private ZoneChangeList m_lastZoneChangeList;

	private Card m_battlecrySourceCard;

	private List<Card> m_cancelingBattlecryCards = new List<Card>();

	private bool m_cardWasInsideHandLastFrame;

	private bool m_isInBattleCryEffect;

	private List<Entity> m_entitiesThatPredictedMana = new List<Entity>();

	private IGraphicsManager m_graphicsManager;

	private List<Actor> m_mobileTargettingEffectActors = new List<Actor>();

	private Card m_lastPreviewedCard;

	private bool m_touchDraggingCard;

	private bool m_useHandEnlarge;

	private bool m_hideHandAfterPlayingCard;

	private bool m_targettingHeroPower;

	private bool m_touchedDownOnSmallHand;

	private float m_touchedDownOnSmallHandStartTime;

	private float m_touchedDownOnMulliganCardTime;

	private bool m_enlargeHandAfterDropCard;

	private bool m_handIsEnlarging;

	private int m_telemetryNumDragAttacks;

	private int m_telemetryNumClickAttacks;

	private const int RAYCAST_MAXTOUCHNUMBER = 30;

	private RaycastHit[] m_cachedDustBlockers = new RaycastHit[30];

	private RaycastHitComparer m_hitPointComparer = new RaycastHitComparer();

	private ScreenEffectsHandle m_screenEffectHandle;

	private bool m_targettingPaused;

	private bool m_isShowingStarshipUI;

	private List<PhoneHandShownListener> m_phoneHandShownListener = new List<PhoneHandShownListener>();

	private List<PhoneHandHiddenListener> m_phoneHandHiddenListener = new List<PhoneHandHiddenListener>();

	public bool LeftMouseButtonDown => m_leftMouseButtonIsDown;

	public Vector3 LastMouseDownPosition => m_lastMouseDownPosition;

	private void Awake()
	{
		s_instance = this;
		m_useHandEnlarge = UniversalInputManager.UsePhoneUI;
		m_graphicsManager = ServiceManager.Get<IGraphicsManager>();
		SetDragging(m_dragging);
		UpdateManagers();
		if (m_gameState != null)
		{
			m_gameState.RegisterOptionsReceivedListener(OnOptionsReceived);
			m_gameState.RegisterOptionRejectedListener(OnOptionRejected);
			m_gameState.RegisterTurnTimerUpdateListener(OnTurnTimerUpdate);
			m_gameState.RegisterGameOverListener(OnGameOver);
		}
		FatalErrorMgr.Get().AddErrorListener(OnFatalError);
		m_screenEffectHandle = new ScreenEffectsHandle(this);
	}

	private void OnDestroy()
	{
		UpdateManagers();
		if (m_gameState != null)
		{
			m_gameState.UnregisterOptionsReceivedListener(OnOptionsReceived);
			m_gameState.UnregisterOptionRejectedListener(OnOptionRejected);
			m_gameState.UnregisterTurnTimerUpdateListener(OnTurnTimerUpdate);
			m_gameState.UnregisterGameOverListener(OnGameOver);
		}
		FatalErrorMgr.Get().RemoveErrorListener(OnFatalError);
		s_instance = null;
		m_cachedDustBlockers = null;
		m_hitPointComparer = null;
	}

	private void OnFatalError(FatalErrorMessage message, object userData)
	{
		DisableInput();
	}

	private bool IsInputOverCard(Card wantedCard)
	{
		if (wantedCard == null)
		{
			return false;
		}
		Actor wantedCardActor = wantedCard.GetActor();
		if (wantedCardActor == null)
		{
			return false;
		}
		RaycastHit inputRayInfo;
		if (!wantedCardActor.IsColliderEnabled())
		{
			wantedCardActor.ToggleCollider(enabled: true);
			m_universalInputManager.GetInputHitInfo(GameLayer.CardRaycast, out inputRayInfo);
			wantedCardActor.ToggleCollider(enabled: false);
		}
		else
		{
			m_universalInputManager.GetInputHitInfo(GameLayer.CardRaycast, out inputRayInfo);
		}
		if (inputRayInfo.collider != null)
		{
			Actor actor = GameObjectUtils.FindComponentInParents<Actor>(inputRayInfo.transform);
			if (actor == null)
			{
				return false;
			}
			return actor.GetCard() == wantedCard;
		}
		return false;
	}

	private bool ShouldCancelTargeting(bool hitBattlefieldHitbox)
	{
		bool shouldCancel = false;
		if (!hitBattlefieldHitbox && GetBattlecrySourceCard() == null && ChoiceCardMgr.Get().GetSubOptionParentCard() == null && !HasPendingChoiceTarget())
		{
			shouldCancel = true;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				bool withinPhoneSlushZone = m_universalInputManager.InputHitAnyObject(Camera.main, GameLayer.InvisibleHitBox3);
				if (m_targettingHeroPower || m_gameState.IsSelectedOptionFriendlyHero() || withinPhoneSlushZone)
				{
					shouldCancel = false;
				}
			}
			else if (m_gameState.IsSelectedOptionFriendlyHero())
			{
				Player friendlySidePlayer = m_gameState.GetFriendlySidePlayer();
				Card friendlyHero = friendlySidePlayer.GetHeroCard();
				Card friendlyWeapon = friendlySidePlayer.GetWeaponCard();
				if (IsInputOverCard(friendlyHero) || IsInputOverCard(friendlyWeapon))
				{
					shouldCancel = false;
				}
			}
			else if (m_gameState.IsSelectedOptionFriendlyHeroPower())
			{
				Card friendlyHeroPower = m_gameState.GetFriendlySidePlayer().GetHeroPowerCard();
				if (IsInputOverCard(friendlyHeroPower))
				{
					shouldCancel = false;
				}
			}
			else if (m_gameState.IsSelectedOptionMercenariesAbility())
			{
				shouldCancel = false;
			}
		}
		if (m_heldCard != null)
		{
			Entity heldEntity = m_heldCard.GetEntity();
			if (heldEntity != null && heldEntity.HasDeckAction() && m_universalInputManager.GetInputHitInfo(Camera.main, GameLayer.DragPlane, out var hitInfo) && m_heldCard.IsInDeckActionArea(hitInfo.point))
			{
				shouldCancel = true;
				m_heldCard.transform.position = hitInfo.point;
			}
		}
		return shouldCancel;
	}

	private void UpdateTargetingArrow()
	{
		if ((bool)m_targetReticleManager && m_targetReticleManager.IsActive())
		{
			bool hitBattlefieldHitbox = m_universalInputManager.InputHitAnyObject(Camera.main, GameLayer.InvisibleHitBox2);
			if (ShouldCancelTargeting(hitBattlefieldHitbox))
			{
				bool allowCancel = true;
				if (m_heldCard != null && m_heldCard.GetEntity().HasDeckAction() && m_heldCard.IsInDeckActionArea())
				{
					CancelTargetMode();
					allowCancel = false;
				}
				if (allowCancel && m_targetReticleManager.IsLocalArrow())
				{
					CancelOption();
				}
				if (m_useHandEnlarge)
				{
					m_myHandZone.SetFriendlyHeroTargetingMode(enable: false);
				}
				if (m_heldCard != null)
				{
					PositionHeldCard();
				}
			}
			else
			{
				m_targetReticleManager.UpdateArrowPosition();
				if (m_heldCard != null)
				{
					m_myHandZone.OnCardHeld(m_heldCard);
				}
			}
		}
		else
		{
			if (!m_heldCard)
			{
				return;
			}
			bool hitBattlefieldHitbox2 = m_universalInputManager.InputHitAnyObject(Camera.main, GameLayer.InvisibleHitBox2);
			HandleUpdateWhileHoldingCard(hitBattlefieldHitbox2);
			if (m_heldCard != null)
			{
				bool isInDeckActionArea = m_heldCard.IsInDeckActionArea() && m_gameState.IsValidOption(m_heldCard.GetEntity(), true);
				if (isInDeckActionArea && !m_heldCardWasInDeckActionAreaLastFrame)
				{
					m_heldCard.UpdateActorState(forceHighlightRefresh: true);
					m_heldCard.UpdateDeckActionHover(isInDeckActionArea);
					m_heldCardWasInDeckActionAreaLastFrame = true;
				}
				else if (!isInDeckActionArea && m_heldCardWasInDeckActionAreaLastFrame)
				{
					m_heldCard.UpdateActorState(forceHighlightRefresh: true);
					m_heldCard.UpdateDeckActionHover(isInDeckActionArea);
					m_heldCardWasInDeckActionAreaLastFrame = false;
				}
			}
		}
	}

	private void UpdateChoiceTargeting()
	{
		PowerProcessor processor = GameState.Get().GetPowerProcessor();
		if (processor != null && processor.GetPowerQueue() != null)
		{
			int queueSize = processor.GetPowerQueue().Count;
			bool hasPendingChoiceTarget = HasPendingChoiceTarget() && queueSize <= 0;
			if (hasPendingChoiceTarget && !m_hadPendingChoiceTargetLastFrame)
			{
				m_hadPendingChoiceTargetLastFrame = true;
				StartPendingChoiceTarget();
			}
			else if (!hasPendingChoiceTarget && m_hadPendingChoiceTargetLastFrame)
			{
				FinishPendingChoiceTarget();
				m_hadPendingChoiceTargetLastFrame = false;
			}
		}
	}

	public void ForceRefreshTargetingArrowText()
	{
		if (!HasPendingChoiceTarget())
		{
			return;
		}
		Network.EntityChoices choicePacket = m_gameState.GetFriendlyEntityChoices();
		if (choicePacket != null)
		{
			Entity sourceEntity = m_gameState.GetEntity(choicePacket.Source);
			if ((bool)m_targetReticleManager)
			{
				m_targetReticleManager.RefreshTargetingArrowText(sourceEntity);
			}
		}
	}

	private bool HasPendingChoiceTarget()
	{
		Network.EntityChoices choicePacket = m_gameState.GetFriendlyEntityChoices();
		if (choicePacket != null && choicePacket.ChoiceType == CHOICE_TYPE.TARGET)
		{
			return true;
		}
		return false;
	}

	private void UpdateDeckActionDeckGlow()
	{
		ZoneDeck deck = ZoneMgr.Get().FindZoneOfType<ZoneDeck>(Player.Side.FRIENDLY);
		if (deck == null)
		{
			return;
		}
		bool holdingTradeableCard = false;
		bool holdingForgeableCard = false;
		Entity heldEntity = null;
		bool validOption = false;
		if (m_heldCard != null)
		{
			heldEntity = m_heldCard.GetEntity();
			validOption = heldEntity != null && m_gameState.IsValidOption(heldEntity, true);
			if (heldEntity != null)
			{
				holdingTradeableCard = heldEntity.IsTradeable() && validOption;
				holdingForgeableCard = heldEntity.IsForgeable() && validOption;
			}
		}
		if (holdingTradeableCard)
		{
			deck.ShowTradeableGlow();
		}
		else
		{
			deck.HideTradeableGlow();
		}
		if (holdingForgeableCard)
		{
			deck.ShowForgeableGlow();
		}
		else
		{
			deck.HideForgeableGlow();
		}
		GameEntity gameEntity = m_gameState.GetGameEntity();
		if (!(TeammateBoardViewer.Get() == null) && gameEntity != null && !gameEntity.IsInBattlegroundsCombatPhase())
		{
			if (m_heldCard != null && (m_heldCard.GetZone() is ZoneHand || m_heldCard.IsInDeckActionArea()))
			{
				TeammateBoardViewer.Get().SetPortalGlow(m_heldCard, validOption);
			}
			else
			{
				TeammateBoardViewer.Get().ClearPortalGlow();
			}
		}
	}

	private void UpdateManagers()
	{
		m_universalInputManager = UniversalInputManager.Get();
		m_gameState = GameState.Get();
		m_targetReticleManager = TargetReticleManager.Get();
	}

	private void Update()
	{
		if (!m_checkForInput)
		{
			return;
		}
		UpdateManagers();
		HandleTwoFingerTap();
		if (InputCollection.GetMouseButtonDown(0))
		{
			HandleLeftMouseDown();
		}
		if (InputCollection.GetMouseButtonUp(0))
		{
			m_touchDraggingCard = false;
			HandleLeftMouseUp();
		}
		if (InputCollection.GetMouseButtonDown(1))
		{
			HandleRightMouseDown();
		}
		if (InputCollection.GetMouseButtonUp(1))
		{
			HandleRightMouseUp();
		}
		HandleMouseMove();
		if (m_heldCard == null)
		{
			if (m_leftMouseButtonIsDown)
			{
				HandleUpdateWhileLeftMouseButtonIsDown();
				if (m_universalInputManager.IsTouchMode() && !m_touchDraggingCard)
				{
					HandleUpdateWhileNotHoldingCard();
				}
			}
			else
			{
				HandleUpdateWhileNotHoldingCard();
			}
		}
		else if (m_heldCard.GetEntity() != null && m_heldCard.GetEntity().GetZone() == TAG_ZONE.SETASIDE)
		{
			DropHeldCard(wasCancelled: true);
			m_gameState.ExitMoveMinionMode();
		}
		if (PermitDecisionMakingInput())
		{
			UpdateTargetingArrow();
			UpdateChoiceTargeting();
			UpdateDeckActionDeckGlow();
		}
		EmoteHandler emoteHandler = EmoteHandler.Get();
		if (emoteHandler != null && emoteHandler.AreEmotesActive())
		{
			emoteHandler.HandleInput();
		}
		EnemyEmoteHandler enemyEmoteHandler = EnemyEmoteHandler.Get();
		if (enemyEmoteHandler != null && enemyEmoteHandler.AreEmotesActive())
		{
			enemyEmoteHandler.HandleInput();
		}
		ShowTooltipIfNecessary();
	}

	public static InputManager Get()
	{
		return s_instance;
	}

	public bool HandleKeyboardInput()
	{
		if (HandleUniversalHotkeys())
		{
			return true;
		}
		if (m_gameState != null && m_gameState.IsMulliganManagerActive())
		{
			return HandleMulliganHotkeys();
		}
		return HandleGameHotkeys();
	}

	public Card GetMousedOverCard()
	{
		return m_mousedOverCard;
	}

	public void SetMousedOverCard(Card card)
	{
		if (!(m_mousedOverCard == card))
		{
			if (m_mousedOverCard != null && !(m_mousedOverCard.GetZone() is ZoneHand))
			{
				HandleMouseOffCard();
			}
			if (card.IsInputEnabled())
			{
				m_mousedOverCard = card;
				card.NotifyMousedOver();
			}
		}
	}

	public Card GetBattlecrySourceCard()
	{
		return m_battlecrySourceCard;
	}

	public void StartWatchingForInput()
	{
		if (m_checkForInput)
		{
			return;
		}
		m_checkForInput = true;
		foreach (Zone zone in ZoneMgr.Get().GetZones())
		{
			if (zone.m_Side == Player.Side.FRIENDLY)
			{
				if (zone is ZoneHand)
				{
					m_myHandZone = (ZoneHand)zone;
				}
				else if (zone is ZonePlay)
				{
					m_myPlayZone = (ZonePlay)zone;
				}
				else if (zone is ZoneWeapon)
				{
					m_myWeaponZone = (ZoneWeapon)zone;
				}
			}
			else if (zone is ZonePlay)
			{
				m_enemyPlayZone = (ZonePlay)zone;
			}
			else if (zone is ZoneHand)
			{
				m_enemyHandZone = (ZoneHand)zone;
			}
		}
	}

	public void DisableInput()
	{
		m_checkForInput = false;
		HandleMouseOff();
		m_targetReticleManager?.DestroyFriendlyTargetArrow(isLocallyCanceled: false);
	}

	public bool PermitDecisionMakingInput()
	{
		GameMgr gameMgr = GameMgr.Get();
		if (gameMgr != null && gameMgr.IsSpectator())
		{
			return false;
		}
		if (m_gameState != null)
		{
			Player friendlyPlayer = m_gameState.GetFriendlySidePlayer();
			if (friendlyPlayer != null && friendlyPlayer.HasTag(GAME_TAG.AI_MAKES_DECISIONS_FOR_PLAYER))
			{
				return false;
			}
		}
		return true;
	}

	public Card GetHeldCard()
	{
		return m_heldCard;
	}

	public void EnableInput()
	{
		m_checkForInput = true;
	}

	public void OnMulliganEnded()
	{
		if ((bool)m_mousedOverCard)
		{
			SetShouldShowTooltip();
		}
		AnomalyMedallion.Initialize();
		AnomalyMedallion.Close();
	}

	private void SetShouldShowTooltip()
	{
		m_mousedOverTimer = 0f;
		if (m_mousedOverCard != null)
		{
			m_mousedOverCard.SetShouldShowTooltip();
		}
	}

	private void SetShouldHideTooltip()
	{
		m_mousedOverTimer = 0f;
		if (m_mousedOverCard != null)
		{
			m_mousedOverCard.HideTooltip();
		}
	}

	public ZoneHand GetFriendlyHand()
	{
		return m_myHandZone;
	}

	public ZoneHand GetEnemyHand()
	{
		return m_enemyHandZone;
	}

	public bool UseHandEnlarge()
	{
		return m_useHandEnlarge;
	}

	public void SetHandEnlarge(bool set)
	{
		m_useHandEnlarge = set;
	}

	public bool DoesHideHandAfterPlayingCard()
	{
		return m_hideHandAfterPlayingCard;
	}

	public void SetHideHandAfterPlayingCard(bool set)
	{
		m_hideHandAfterPlayingCard = set;
	}

	public bool DropHeldCard()
	{
		return DropHeldCard(wasCancelled: false);
	}

	private void HandleLeftMouseDown()
	{
		if ((bool)m_lastObjectTwoFingerDown)
		{
			return;
		}
		m_touchedDownOnSmallHand = false;
		bool hitObjectShouldHidePhoneHand = true;
		GameObject hitObject = null;
		if (m_universalInputManager.GetInputHitInfo(GameLayer.CardRaycast, out var leftClickRayInfo))
		{
			hitObject = leftClickRayInfo.collider.gameObject;
			if (hitObject.GetComponent<EndTurnButtonReminder>() != null)
			{
				return;
			}
			CardStandIn standIn = GameObjectUtils.FindComponentInParents<CardStandIn>(leftClickRayInfo.transform);
			if (standIn != null)
			{
				bool viewingTeammate = TeammateBoardViewer.Get() != null && TeammateBoardViewer.Get().IsViewingTeammate();
				if (m_gameState != null && !m_gameState.IsMulliganManagerActive())
				{
					Card linkedCard = standIn.linkedCard;
					if (viewingTeammate)
					{
						TeammateBoardViewer.Get().ClickedTeammatesCardInHand(linkedCard);
					}
					else
					{
						if (IsCancelingBattlecryCard(linkedCard))
						{
							return;
						}
						if (m_myHandZone == null)
						{
							Debug.LogWarning("HandZone not set for CardStandIn " + (standIn.name ?? "Unknown"));
							return;
						}
						if (m_useHandEnlarge && !m_myHandZone.HandEnlarged())
						{
							m_leftMouseButtonIsDown = true;
							m_touchedDownOnSmallHand = true;
							if (!HasPlayFromMiniHandEnabled())
							{
								return;
							}
							m_touchedDownOnSmallHandStartTime = Time.realtimeSinceStartup;
						}
						m_lastObjectMousedDown = standIn.gameObject;
						m_lastMouseDownPosition = InputCollection.GetMousePosition();
						m_leftMouseButtonIsDown = true;
						if (m_universalInputManager.IsTouchMode())
						{
							m_touchDraggingCard = m_myHandZone.TouchReceived();
							m_lastPreviewedCard = standIn.linkedCard;
						}
						if (m_heldCard == null)
						{
							m_myHandZone.HandleInput();
						}
					}
					return;
				}
			}
			if (hitObject.GetComponent<EndTurnButton>() != null && PermitDecisionMakingInput() && !EndTurnButton.Get().IsInputBlocked())
			{
				EndTurnButton.Get().PlayPushDownAnimation();
				m_lastObjectMousedDown = hitObject;
				return;
			}
			if (GameMgr.Get().IsBattlegroundDuoGame() && TeammatePingWheelManager.Get() != null)
			{
				TeammatePingWheelManager.Get().HideAllPingWheels();
				if (hitObject.GetComponent<TeammatePingOptions>() != null)
				{
					hitObject.GetComponent<TeammatePingOptions>().OptionSelected();
					m_lastObjectMousedDown = hitObject;
					return;
				}
			}
			DuosPortal duosPortal = hitObject.GetComponentInParent<DuosPortal>();
			if (duosPortal != null && m_heldCard == null)
			{
				duosPortal.PortalPushed();
				m_lastObjectMousedDown = hitObject;
				return;
			}
			if (hitObject.GetComponent<GameOpenPack>() != null)
			{
				m_lastObjectMousedDown = hitObject;
				return;
			}
			Actor actor = GameObjectUtils.FindComponentInParents<Actor>(leftClickRayInfo.transform);
			if ((actor == null || actor.GetEntity() == null || !actor.GetEntity().IsControlledByFriendlySidePlayer()) && PermitDecisionMakingInput())
			{
				ManuallyDismissMercenariesAbilityTray();
			}
			if (BattlegroundsEmoteHandler.TryGetActiveInstance(out var battlegroundsEmoteHandler) && hitObject != battlegroundsEmoteHandler.gameObject && !hitObject.TryGetComponent<BattlegroundsEmoteOption>(out var _))
			{
				battlegroundsEmoteHandler.HideEmotes();
			}
			if (actor == null)
			{
				return;
			}
			if (TeammateBoardViewer.Get() != null && TeammateBoardViewer.Get().IsViewingTeammate() && !m_universalInputManager.IsTouchMode())
			{
				GameplayErrorManager.Get().DisplayMessage(GameStrings.Get("GAMEPLAY_BACON_CLICKING_ON_TEAMMATES_MINIONS"));
				return;
			}
			Card card = actor.GetCard();
			if (m_universalInputManager.IsTouchMode() && m_battlecrySourceCard != null && card == m_battlecrySourceCard)
			{
				SetDragging(dragging: true);
				m_targetReticleManager.ShowArrow(show: true);
				return;
			}
			if (card != null && (IsCancelingBattlecryCard(card) || m_myHandZone == null || card.GetEntity() == null || (m_useHandEnlarge && m_myHandZone.HandEnlarged() && card.GetEntity().IsHeroPower() && card.GetEntity().IsControlledByLocalUser() && m_myHandZone.GetCardCount() > 1)))
			{
				return;
			}
			if (card != null)
			{
				m_lastObjectMousedDown = card.gameObject;
			}
			else if (actor.GetHistoryCard() != null)
			{
				m_lastObjectMousedDown = actor.transform.parent.gameObject;
			}
			else
			{
				Debug.LogWarning("You clicked on something that is not being handled by InputManager.  Alert The Brode!");
			}
			m_lastMouseDownPosition = InputCollection.GetMousePosition();
			m_leftMouseButtonIsDown = true;
			m_touchedDownOnMulliganCardTime = Time.realtimeSinceStartup;
			hitObjectShouldHidePhoneHand = actor.GetEntity() != null && actor.GetEntity().IsGameModeButton();
		}
		if (m_useHandEnlarge && m_myHandZone != null && m_myHandZone.HandEnlarged() && ChoiceCardMgr.Get().GetSubOptionParentCard() == null && (hitObject == null || hitObjectShouldHidePhoneHand))
		{
			HidePhoneHand();
		}
		if (hitObject == null && PermitDecisionMakingInput())
		{
			ManuallyDismissMercenariesAbilityTray();
		}
		if (hitObject == null && BattlegroundsEmoteHandler.TryGetActiveInstance(out var handler))
		{
			handler.HideEmotes();
		}
		if (GameMgr.Get().IsBattlegroundDuoGame())
		{
			if (TeammatePingWheelManager.Get() != null)
			{
				TeammatePingWheelManager.Get().HideAllPingWheels();
			}
			if (TeammateBoardViewer.Get() != null)
			{
				TeammateBoardViewer.Get().GetTeammateHandViewer().ShrinkHand();
			}
		}
		HandleMemberClick(hitObject);
	}

	private void ShowPhoneHand()
	{
		if (!m_gameState.IsMulliganPhaseNowOrPending() && !m_gameState.IsGameOver() && m_useHandEnlarge && !m_myHandZone.HandEnlarged())
		{
			m_handIsEnlarging = true;
			m_myHandZone.AddUpdateLayoutCompleteCallback(OnHandEnlargeComplete);
			m_myHandZone.SetHandEnlarged(enlarged: true);
			PhoneHandShownListener[] array = m_phoneHandShownListener.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Fire();
			}
		}
	}

	public void HidePhoneHand()
	{
		if (m_useHandEnlarge && m_myHandZone != null && m_myHandZone.HandEnlarged() && !m_handIsEnlarging)
		{
			m_myHandZone.SetHandEnlarged(enlarged: false);
			PhoneHandHiddenListener[] array = m_phoneHandHiddenListener.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Fire();
			}
		}
	}

	private void OnHandEnlargeComplete(Zone zone, object userData)
	{
		zone.RemoveUpdateLayoutCompleteCallback(OnHandEnlargeComplete);
		if (m_leftMouseButtonIsDown && m_universalInputManager.InputHitAnyObject(GameLayer.CardRaycast))
		{
			HandleLeftMouseDown();
		}
		m_handIsEnlarging = false;
	}

	private void HidePhoneHandIfOutOfServerPlays()
	{
		if (!m_gameState.HasHandPlays())
		{
			HidePhoneHand();
		}
	}

	private bool HasLocalHandPlays()
	{
		List<Card> handCards = m_myHandZone.GetCards();
		if (handCards.Count == 0)
		{
			return false;
		}
		int mana = ManaCrystalMgr.Get().GetSpendableManaCrystals();
		foreach (Card item in handCards)
		{
			if (item.GetEntity().GetRealTimeCost() <= mana)
			{
				return true;
			}
		}
		return false;
	}

	private void HandleLeftMouseUp()
	{
		PegCursor.Get().SetMode(PegCursor.Mode.UP);
		m_lastInputDrag = m_dragging;
		SetDragging(dragging: false);
		m_leftMouseButtonIsDown = false;
		m_targettingHeroPower = false;
		GameObject lastCardDown = m_lastObjectMousedDown;
		m_lastObjectMousedDown = null;
		if (m_universalInputManager.WasTouchCanceled())
		{
			CancelOption();
			return;
		}
		if (m_heldCard != null && (m_gameState.GetResponseMode() == GameState.ResponseMode.OPTION || m_gameState.GetResponseMode() == GameState.ResponseMode.NONE))
		{
			DropHeldCard();
			return;
		}
		if (TeammateBoardViewer.Get() != null)
		{
			TeammateBoardViewer.Get().HandleLeftMouseUp();
		}
		if (BattlegroundsEmoteHandler.TryGetActiveInstance(out var battlegroundsEmoteHandler))
		{
			if (battlegroundsEmoteHandler.IsMouseOverEmoteOption)
			{
				battlegroundsEmoteHandler.HandleEmoteClicked();
				return;
			}
			if (m_universalInputManager.IsTouchMode())
			{
				battlegroundsEmoteHandler.HideEmotes();
				return;
			}
		}
		bool cancelOptions = m_universalInputManager.IsTouchMode() && m_gameState.IsInTargetMode();
		ChoiceCardMgr choiceCardMgr = ChoiceCardMgr.Get();
		Card suboptionParentCard = choiceCardMgr.GetSubOptionParentCard();
		bool canCancelSubOption = suboptionParentCard != null;
		if (suboptionParentCard != null && suboptionParentCard.GetEntity().HasTag(GAME_TAG.STARSHIP))
		{
			canCancelSubOption = false;
		}
		if (m_universalInputManager.GetInputHitInfo(GameLayer.CardRaycast, out var leftClickRayInfo))
		{
			GameObject hitObject = leftClickRayInfo.collider.gameObject;
			if (hitObject.GetComponent<EndTurnButtonReminder>() != null)
			{
				return;
			}
			if (hitObject.GetComponent<EndTurnButton>() != null && hitObject == lastCardDown && PermitDecisionMakingInput() && !EndTurnButton.Get().IsInputBlocked())
			{
				EndTurnButton.Get().PlayButtonUpAnimation();
				DoEndTurnButton();
				ManuallyDismissMercenariesAbilityTray();
			}
			else
			{
				GameOpenPack gameOpenPack = hitObject.GetComponent<GameOpenPack>();
				if (gameOpenPack != null && hitObject == lastCardDown)
				{
					gameOpenPack.HandleClick();
				}
				else
				{
					Actor actor = GameObjectUtils.FindComponentInParents<Actor>(leftClickRayInfo.transform);
					if (actor != null)
					{
						Card card = actor.GetCard();
						if (card != null)
						{
							if ((card.gameObject == lastCardDown || m_lastInputDrag) && !IsCancelingBattlecryCard(card))
							{
								HandleClickOnCard(card.gameObject, card.gameObject == lastCardDown);
							}
						}
						else if (actor.GetHistoryCard() != null)
						{
							HistoryManager.Get().HandleClickOnBigCard(actor.GetHistoryCard());
						}
						else if (m_gameState.IsMulliganManagerActive() && ShouldSelectForMulligan())
						{
							MulliganManager.Get().ToggleHoldState(actor);
						}
					}
					CardStandIn standIn = GameObjectUtils.FindComponentInParents<CardStandIn>(leftClickRayInfo.transform);
					if (standIn != null)
					{
						if (HasPlayFromMiniHandEnabled() && m_useHandEnlarge && m_touchedDownOnSmallHand)
						{
							ShowPhoneHand();
						}
						TryHandleClickOnCard(lastCardDown, standIn);
					}
					if (m_universalInputManager.IsTouchMode() && actor != null && choiceCardMgr.GetSubOptionParentCard() != null)
					{
						Card actorCard = actor.GetCard();
						foreach (Card friendlyCard in choiceCardMgr.GetFriendlyCards())
						{
							if (friendlyCard == actorCard)
							{
								canCancelSubOption = false;
								break;
							}
						}
					}
				}
			}
		}
		if (cancelOptions)
		{
			CancelOption();
		}
		if (m_universalInputManager.IsTouchMode() && canCancelSubOption && choiceCardMgr.GetSubOptionParentCard() != null)
		{
			CancelSubOptionMode();
		}
		if (m_universalInputManager.IsTouchMode())
		{
			SetHeldCardValue(null);
			ZoneMgr.Get().FindZoneOfType<ZonePlay>(Player.Side.FRIENDLY).UpdateLayout();
		}
	}

	public bool WaitingForTouchDelay()
	{
		return Time.realtimeSinceStartup - m_touchedDownOnSmallHandStartTime <= m_TouchHoldDuration;
	}

	private bool ShouldSelectForMulligan()
	{
		if (!m_universalInputManager.IsTouchMode())
		{
			return true;
		}
		return Time.realtimeSinceStartup - m_touchedDownOnMulliganCardTime <= m_mulliganTouchThreshold;
	}

	private void TryHandleClickOnCard(GameObject lastCardDown, CardStandIn standIn)
	{
		if (lastCardDown == standIn.gameObject && standIn.linkedCard != null && m_gameState != null && !m_gameState.IsMulliganManagerActive() && !IsCancelingBattlecryCard(standIn.linkedCard))
		{
			HandleClickOnCard(standIn.linkedCard.gameObject, wasMouseDownTarget: true);
		}
	}

	private void HandleRightMouseDown()
	{
		if (!m_universalInputManager.GetInputHitInfo(GameLayer.CardRaycast, out var rightClickRayInfo))
		{
			return;
		}
		GameObject hitObject = rightClickRayInfo.collider.gameObject;
		if (hitObject.GetComponent<EndTurnButtonReminder>() != null || hitObject.GetComponent<EndTurnButton>() != null)
		{
			return;
		}
		Actor parentActor = GameObjectUtils.FindComponentInParents<Actor>(rightClickRayInfo.transform);
		if (parentActor == null)
		{
			CardStandIn standIn = GameObjectUtils.FindComponentInParents<CardStandIn>(rightClickRayInfo.transform);
			if (standIn == null || standIn.linkedCard == null)
			{
				return;
			}
			parentActor = standIn.linkedCard.GetActor();
			if (parentActor == null)
			{
				return;
			}
		}
		Card card = parentActor.GetCard();
		if (card != null)
		{
			m_lastObjectRightMousedDown = card.gameObject;
		}
		else if (parentActor.GetHistoryCard() != null)
		{
			m_lastObjectRightMousedDown = parentActor.transform.parent.gameObject;
		}
		else
		{
			Debug.LogWarning("You clicked on something that is not being handled by InputManager.  Alert The Brode!");
		}
	}

	private void HandleRightMouseUp()
	{
		PegCursor.Get().SetMode(PegCursor.Mode.UP);
		GameObject lastCardDown = m_lastObjectRightMousedDown;
		m_lastObjectRightMousedDown = null;
		m_lastObjectMousedDown = null;
		m_leftMouseButtonIsDown = false;
		SetDragging(dragging: false);
		if (m_universalInputManager.GetInputHitInfo(GameLayer.CardRaycast, out var rightClickRayInfo))
		{
			Actor actor = GameObjectUtils.FindComponentInParents<Actor>(rightClickRayInfo.transform);
			if (actor == null || actor.GetCard() == null)
			{
				CardStandIn standIn = GameObjectUtils.FindComponentInParents<CardStandIn>(rightClickRayInfo.transform);
				if (standIn == null || standIn.linkedCard == null)
				{
					HandleRightClick();
					return;
				}
				actor = standIn.linkedCard.GetActor();
				if (actor == null || actor.GetCard() == null)
				{
					HandleRightClick();
					return;
				}
			}
			if (actor.GetCard().gameObject == lastCardDown)
			{
				HandleRightClickOnCard(actor.GetCard());
			}
			else
			{
				HandleRightClick();
			}
		}
		else
		{
			HandleRightClick();
		}
	}

	private void HandleRightClick()
	{
		HidePlayerStarshipUI();
		if (!HasPendingChoiceTarget() && CancelOption())
		{
			if (m_mousedOverCard != null && m_mousedOverCard.GetZone() is ZonePlay && m_mousedOverCard.GetEntity().IsMinion())
			{
				m_mousedOverCard.SetShouldShowTooltip();
				m_mousedOverCard.ShowTooltip();
			}
			return;
		}
		EmoteHandler emoteHandler = EmoteHandler.Get();
		BattlegroundsEmoteHandler battlegroundsEmoteHandler;
		if (emoteHandler != null)
		{
			if (emoteHandler.AreEmotesActive())
			{
				emoteHandler.HideEmotes();
			}
		}
		else if (BattlegroundsEmoteHandler.TryGetActiveInstance(out battlegroundsEmoteHandler))
		{
			battlegroundsEmoteHandler.HideEmotes();
		}
		EnemyEmoteHandler enemyEmoteHandler = EnemyEmoteHandler.Get();
		if (enemyEmoteHandler != null && enemyEmoteHandler.AreEmotesActive())
		{
			enemyEmoteHandler.HideEmotes();
		}
	}

	private void HandleTwoFingerTap()
	{
		if (!GameMgr.Get().IsBattlegroundDuoGame() || !m_universalInputManager.IsTouchMode())
		{
			return;
		}
		if (Input.GetMouseButtonDown(1) && m_universalInputManager.GetInputHitInfo(GameLayer.CardRaycast, out var rightClickRayInfo))
		{
			Actor parentActor = GameObjectUtils.FindComponentInParents<Actor>(rightClickRayInfo.transform);
			if (parentActor == null)
			{
				CardStandIn standIn = GameObjectUtils.FindComponentInParents<CardStandIn>(rightClickRayInfo.transform);
				if (standIn == null || standIn.linkedCard == null)
				{
					return;
				}
				parentActor = standIn.linkedCard.GetActor();
				if (parentActor == null)
				{
					return;
				}
			}
			Card card = parentActor.GetCard();
			if (card != null)
			{
				m_lastObjectTwoFingerDown = card.gameObject;
			}
		}
		if (!Input.GetMouseButtonUp(1))
		{
			return;
		}
		GameObject lastCardDown = m_lastObjectTwoFingerDown;
		m_lastObjectTwoFingerDown = null;
		if (!m_universalInputManager.GetInputHitInfo(GameLayer.CardRaycast, out var rightClickRayInfo2))
		{
			return;
		}
		Actor actor = GameObjectUtils.FindComponentInParents<Actor>(rightClickRayInfo2.transform);
		if (actor == null || actor.GetCard() == null)
		{
			CardStandIn standIn2 = GameObjectUtils.FindComponentInParents<CardStandIn>(rightClickRayInfo2.transform);
			if (standIn2 == null || standIn2.linkedCard == null)
			{
				return;
			}
			actor = standIn2.linkedCard.GetActor();
			if (actor == null || actor.GetCard() == null)
			{
				return;
			}
		}
		if (actor.GetCard().gameObject == lastCardDown)
		{
			ShowCardPingWheel(actor.GetCard());
		}
	}

	public void HidePlayerStarshipUI()
	{
		if (m_isShowingStarshipUI)
		{
			m_isShowingStarshipUI = false;
			SetShouldShowTooltip();
			ChoiceCardMgr choiceCardMgr = ChoiceCardMgr.Get();
			if (choiceCardMgr != null)
			{
				choiceCardMgr.HideStarshipPiecesForOpposingPlayer();
			}
		}
	}

	private bool CancelOption(bool timeout = false)
	{
		bool optionCanceled = false;
		if (m_gameState.IsInMainOptionMode())
		{
			m_gameState.CancelCurrentOptionMode();
		}
		if (CancelTargetMode())
		{
			optionCanceled = true;
		}
		if (CancelSubOptionMode(timeout))
		{
			optionCanceled = true;
		}
		if (DropHeldCard(wasCancelled: true))
		{
			optionCanceled = true;
		}
		if ((bool)m_mousedOverCard)
		{
			m_mousedOverCard.UpdateProposedManaUsage();
		}
		return optionCanceled;
	}

	public bool IsPaused()
	{
		return m_targettingPaused;
	}

	public bool PauseTargetMode()
	{
		if (!m_gameState.IsInTargetMode() && !m_hadPendingChoiceTargetLastFrame)
		{
			return false;
		}
		m_targettingPaused = true;
		return true;
	}

	public bool CancelTargetMode()
	{
		if (!m_gameState.IsInTargetMode() && !m_hadPendingChoiceTargetLastFrame)
		{
			return false;
		}
		bool playSound = true;
		Network.Options.Option option = m_gameState.GetSelectedNetworkOption();
		if (option != null)
		{
			Entity entity = m_gameState.GetEntity(option.Main.ID);
			if (entity != null && entity.IsLettuceAbility())
			{
				playSound = false;
			}
			HandleLegendaryHeroPowerCancel(entity);
		}
		if (playSound)
		{
			SoundManager.Get().LoadAndPlay("CancelAttack.prefab:9cde7207a78024e46aa5a0a657807845");
		}
		if ((bool)m_mousedOverCard)
		{
			DisableSkullIfNeeded(m_mousedOverCard);
		}
		m_targetReticleManager?.DestroyFriendlyTargetArrow(isLocallyCanceled: true);
		ZoneMgr.Get().DisplayLettuceAbilitiesForPreviouslySelectedCard();
		if (m_battlecrySourceCard != null)
		{
			m_myHandZone.OnCardWithReplacementsDropped(m_battlecrySourceCard);
			m_myHandZone.ClearReservedCard();
		}
		ResetBattlecrySourceCard();
		CancelSubOptions();
		m_gameState.CancelCurrentOptionMode();
		return true;
	}

	public bool CancelSubOptionMode(bool timeout = false)
	{
		if (!m_gameState.IsInSubOptionMode())
		{
			return false;
		}
		if (ChoiceCardMgr.Get().IsWaitingToShowSubOptions())
		{
			if (timeout)
			{
				StartCoroutine(WaitAndCancelSubOptionMode());
			}
			return false;
		}
		CancelSubOptions();
		m_gameState.CancelCurrentOptionMode();
		return true;
	}

	private IEnumerator WaitAndCancelSubOptionMode()
	{
		ChoiceCardMgr choiceCardMgr = ChoiceCardMgr.Get();
		choiceCardMgr.QuenePendingCancelSubOptions();
		while (choiceCardMgr.IsWaitingToShowSubOptions())
		{
			yield return null;
		}
		if (choiceCardMgr.HasPendingCancelSubOptions())
		{
			CancelSubOptions();
			if (m_gameState.IsInSubOptionMode())
			{
				m_gameState.CancelCurrentOptionMode();
			}
		}
		choiceCardMgr.ClearPendingCancelSubOptions();
	}

	private bool AllowMovingMinionAcrossPlayZone()
	{
		return false;
	}

	private bool IsOverFriendlyPlayZone(RaycastHit hitInfo)
	{
		return (double)hitInfo.point.z < -4.0;
	}

	private void PositionHeldCard()
	{
		Card card = m_heldCard;
		Entity entity = card.GetEntity();
		ZonePlay entityPlayZone = GetControllersPlayZone(entity);
		MoveMinionHoverTarget hoverTarget = GetMoveMinionHoverTarget(card);
		RaycastHit battlefieldHitInfo;
		if (hoverTarget != null)
		{
			entityPlayZone.SortWithSpotForHeldCard(-1);
			card.NotifyOverMoveMinionTarget(hoverTarget);
		}
		else if (m_universalInputManager.GetInputHitInfo(Camera.main, GameLayer.InvisibleHitBox2, out battlefieldHitInfo))
		{
			if (!card.IsOverPlayfield())
			{
				if (!m_gameState.HasResponse(entity, null))
				{
					m_leftMouseButtonIsDown = false;
					m_lastObjectMousedDown = null;
					SetDragging(dragging: false);
					DropHeldCard();
					return;
				}
				card.NotifyOverPlayfield();
			}
			if (entity.IsMinion())
			{
				if (AllowMovingMinionAcrossPlayZone())
				{
					ZoneMgr zoneMgr = ZoneMgr.Get();
					ZonePlay friendlyPlayZone = zoneMgr.FindZoneOfType<ZonePlay>(Player.Side.FRIENDLY);
					ZonePlay opposingPlayZone = zoneMgr.FindZoneOfType<ZonePlay>(Player.Side.OPPOSING);
					ZonePlay targetPlayZone = (IsOverFriendlyPlayZone(battlefieldHitInfo) ? friendlyPlayZone : opposingPlayZone);
					ZonePlay sourcePlayZone = ((friendlyPlayZone.GetSlotMousedOver() > -1) ? friendlyPlayZone : opposingPlayZone);
					if (targetPlayZone != sourcePlayZone)
					{
						sourcePlayZone.SortWithSpotForHeldCard(-1);
					}
					int slotWeAreOver = PlayZoneSlotMousedOver(targetPlayZone, card);
					if (slotWeAreOver >= 0 && targetPlayZone.HasMousedOverSlotChanged(slotWeAreOver))
					{
						targetPlayZone.SortWithSpotForHeldCard(slotWeAreOver);
					}
				}
				else
				{
					int slotWeAreOver2 = PlayZoneSlotMousedOver(entityPlayZone, card);
					if (slotWeAreOver2 >= 0 && entityPlayZone.HasMousedOverSlotChanged(slotWeAreOver2))
					{
						entityPlayZone.SortWithSpotForHeldCard(slotWeAreOver2);
					}
				}
			}
			else if (entity.IsLocation())
			{
				int slotWeAreOver3 = PlayZoneSlotMousedOver(entityPlayZone, card);
				if (slotWeAreOver3 >= 0 && entityPlayZone.HasMousedOverSlotChanged(slotWeAreOver3))
				{
					entityPlayZone.SortWithSpotForHeldCard(slotWeAreOver3);
				}
			}
			else if (entity.IsBaconSpell())
			{
				int slotWeAreOver4 = PlayZoneSlotMousedOver(entityPlayZone, card);
				if (slotWeAreOver4 >= 0 && entityPlayZone.HasMousedOverSlotChanged(slotWeAreOver4))
				{
					entityPlayZone.SortWithSpotForHeldCard(slotWeAreOver4);
				}
			}
		}
		else
		{
			bool inPlay = entity.GetZone() == TAG_ZONE.PLAY;
			if (card.IsOverPlayfield() && !inPlay)
			{
				card.NotifyLeftPlayfield();
				entityPlayZone.SortWithSpotForHeldCard(-1);
			}
		}
		if (hoverTarget == null && card.IsOverMoveMinionTarget())
		{
			card.NotifyLeftMoveMinionTarget();
		}
		if (m_universalInputManager.GetInputHitInfo(Camera.main, GameLayer.DragPlane, out var hitInfo))
		{
			card.transform.position = hitInfo.point;
		}
	}

	private int GetNumberOfUsedSlotsInPlay(ZonePlay play)
	{
		return play.GetCards().Count((Card c) => !c.IsBeingDragged);
	}

	public bool IsHeldCardLocation()
	{
		Entity dummyEntity;
		return IsHeldCardLocation(out dummyEntity);
	}

	public bool IsHeldCardLocation(out Entity location)
	{
		Card heldCard = GetHeldCard();
		if (heldCard != null && heldCard.GetEntity() != null)
		{
			Entity heldEntity = heldCard.GetEntity();
			if (heldEntity.IsLocation())
			{
				location = heldEntity;
				return true;
			}
		}
		location = null;
		return false;
	}

	public bool IsHeldCardMinion()
	{
		Entity dummyEntity;
		return IsHeldCardMinion(out dummyEntity);
	}

	public bool IsHeldCardMinion(out Entity minion)
	{
		Card heldCard = GetHeldCard();
		if (heldCard != null && heldCard.GetEntity() != null)
		{
			Entity heldEntity = heldCard.GetEntity();
			if (heldEntity.IsMinion())
			{
				minion = heldEntity;
				return true;
			}
		}
		minion = null;
		return false;
	}

	private int PlayZoneSlotMousedOver(ZonePlay playZone, Card card)
	{
		if (playZone == null)
		{
			return -1;
		}
		int slotWeAreOver = 0;
		if (m_universalInputManager.GetInputHitInfo(Camera.main, GameLayer.InvisibleHitBox2, out var battlefieldHitInfo))
		{
			Entity entity = card.GetEntity();
			int usedSlots = GetNumberOfUsedSlotsInPlay(playZone);
			int maxSlots = m_gameState.GetMaxFriendlySlotsPerPlayer(entity);
			if (usedSlots >= maxSlots && entity != null && !GameState.Get().IsValidOption(entity, false))
			{
				return -1;
			}
			float slotWidth = playZone.GetSlotWidth();
			float leftSideOfZone = playZone.transform.position.x - (float)(usedSlots + 1) * slotWidth / 2f;
			slotWeAreOver = (int)Mathf.Ceil((battlefieldHitInfo.point.x - leftSideOfZone) / slotWidth) - 1;
			if (slotWeAreOver < 0 || slotWeAreOver > usedSlots)
			{
				slotWeAreOver = ((!(card.transform.position.x < playZone.transform.position.x)) ? usedSlots : 0);
			}
		}
		return slotWeAreOver + 1;
	}

	private void HandleUpdateWhileLeftMouseButtonIsDown()
	{
		if (HasPlayFromMiniHandEnabled() && !m_myHandZone.IsCardFocused && !WaitingForTouchDelay())
		{
			m_myHandZone.UpdateLayout(null, forced: true);
		}
		if (m_universalInputManager.IsTouchMode() && m_heldCard == null)
		{
			if (GetBattlecrySourceCard() == null)
			{
				m_myHandZone.HandleInput();
			}
			Card previewedCard = ((m_myHandZone.CurrentStandIn != null) ? m_myHandZone.CurrentStandIn.linkedCard : null);
			if (previewedCard != m_lastPreviewedCard)
			{
				if (previewedCard != null)
				{
					m_lastMouseDownPosition.y = InputCollection.GetMousePosition().y;
				}
				m_lastPreviewedCard = previewedCard;
			}
		}
		if (m_dragging || m_lastObjectMousedDown == null)
		{
			return;
		}
		if ((bool)m_lastObjectMousedDown.GetComponent<HistoryCard>())
		{
			m_lastObjectMousedDown = null;
			m_leftMouseButtonIsDown = false;
			return;
		}
		Vector3 mousePosition = InputCollection.GetMousePosition();
		float yDiff = mousePosition.y - m_lastMouseDownPosition.y;
		float xDiff = mousePosition.x - m_lastMouseDownPosition.x;
		if (xDiff > -20f && xDiff < 20f && yDiff > -20f && yDiff < 20f)
		{
			return;
		}
		bool grabCardAllowed = !m_universalInputManager.IsTouchMode() || yDiff > (float)MIN_GRAB_Y;
		CardStandIn standIn = m_lastObjectMousedDown.GetComponent<CardStandIn>();
		if (standIn != null && m_gameState != null && !m_gameState.IsMulliganManagerActive())
		{
			if (m_universalInputManager.IsTouchMode())
			{
				if (!grabCardAllowed)
				{
					return;
				}
				standIn = m_myHandZone.CurrentStandIn;
				if (standIn == null)
				{
					return;
				}
			}
			if (!ChoiceCardMgr.Get().IsFriendlyShown() && !m_gameState.IsInChoiceMode() && GetBattlecrySourceCard() == null && IsInZone(standIn.linkedCard, TAG_ZONE.HAND))
			{
				SetDragging(dragging: true);
				GrabCard(standIn.linkedCard.gameObject);
			}
		}
		else
		{
			if (m_gameState.IsMulliganManagerActive() || m_gameState.IsInTargetMode())
			{
				return;
			}
			Card card = m_lastObjectMousedDown.GetComponent<Card>();
			Entity entity = card.GetEntity();
			if (entity == null)
			{
				return;
			}
			if (IsInZone(card, TAG_ZONE.HAND))
			{
				if (entity.IsControlledByLocalUser() && grabCardAllowed && (!m_universalInputManager.IsTouchMode() || m_gameState.HasResponse(entity, null)) && (card.GetZone().m_ServerTag == TAG_ZONE.HAND || m_gameState.HasResponse(entity, null)) && !ChoiceCardMgr.Get().IsFriendlyShown() && GetBattlecrySourceCard() == null)
				{
					SetDragging(dragging: true);
					GrabCard(m_lastObjectMousedDown);
				}
			}
			else if (IsInZone(card, TAG_ZONE.PLAY))
			{
				bool isCardButton = entity.IsCardButton();
				if ((!isCardButton && !entity.IsMoveMinionHoverTarget()) || (isCardButton && m_gameState.EntityHasTargets(entity)))
				{
					SetDragging(dragging: true);
					HandleClickOnCardInBattlefield(entity);
				}
			}
		}
	}

	private void HandleUpdateWhileHoldingCard(bool hitBattlefield)
	{
		PegCursor.Get().SetMode(PegCursor.Mode.DRAG);
		Card card = m_heldCard;
		if (!card.IsInputEnabled())
		{
			DropHeldCard();
			return;
		}
		Entity heldEntity = card.GetEntity();
		if (hitBattlefield && (bool)m_targetReticleManager && !m_targetReticleManager.IsActive() && m_gameState.EntityHasTargets(heldEntity) && heldEntity.GetCardType() != TAG_CARDTYPE.MINION && !m_gameState.EntityOnlyTrades(heldEntity) && !m_heldCard.IsInDeckActionArea())
		{
			if (!DoNetworkResponse(heldEntity))
			{
				PositionHeldCard();
				return;
			}
			DragCardSoundEffects cardEffects = card.GetComponent<DragCardSoundEffects>();
			if ((bool)cardEffects)
			{
				cardEffects.Disable();
			}
			RemoteActionHandler remoteActionHandler = RemoteActionHandler.Get();
			remoteActionHandler.NotifyOpponentOfCardPickedUp(card);
			remoteActionHandler.NotifyOpponentOfTargetModeBegin(card);
			bool useHandAsOrigin = heldEntity.GetHero() == null;
			m_targetReticleManager.CreateFriendlyTargetArrow(heldEntity, showDamageIndicatorText: true, showArrow: true, null, useHandAsOrigin);
			ActivatePowerUpSpell(card);
			ActivatePlaySpell(card);
		}
		else
		{
			bool cardWasInsideHandLastFrame = m_cardWasInsideHandLastFrame;
			if (hitBattlefield && m_cardWasInsideHandLastFrame)
			{
				RemoteActionHandler.Get().NotifyOpponentOfCardPickedUp(card);
				m_cardWasInsideHandLastFrame = false;
			}
			else if (!hitBattlefield)
			{
				m_cardWasInsideHandLastFrame = true;
			}
			PositionHeldCard();
			if (hitBattlefield)
			{
				m_myPlayZone.OnMagneticHeld(m_heldCard);
				m_myHandZone.OnCardHeld(m_heldCard);
			}
			else if (cardWasInsideHandLastFrame)
			{
				m_myHandZone.OnCardWithReplacementsDropped(m_heldCard);
				m_myPlayZone.OnMagneticDropped(m_heldCard);
			}
			if (m_gameState.GetResponseMode() == GameState.ResponseMode.SUB_OPTION)
			{
				CancelSubOptionMode();
			}
		}
		if (m_universalInputManager.IsTouchMode() && !hitBattlefield && m_heldCard != null && InputCollection.GetMousePosition().y - m_lastMouseDownPosition.y < (float)MIN_GRAB_Y && !IsInZone(m_heldCard, TAG_ZONE.PLAY))
		{
			m_myHandZone.OnCardWithReplacementsDropped(m_heldCard);
			m_myPlayZone.OnMagneticDropped(m_heldCard);
			PegCursor.Get().SetMode(PegCursor.Mode.STOPDRAG);
			ReturnHeldCardToHand();
		}
	}

	private MoveMinionHoverTarget GetMoveMinionHoverTarget(Card heldCard)
	{
		if (heldCard == null)
		{
			return null;
		}
		if (m_universalInputManager.GetInputHitInfo(GameLayer.CardRaycast, out var hitInfo))
		{
			MoveMinionHoverTarget hoverTarget = hitInfo.transform.gameObject.GetComponentInParent<MoveMinionHoverTarget>();
			if (hoverTarget != null)
			{
				return hoverTarget;
			}
		}
		return null;
	}

	private void ActivatePowerUpSpell(Card card)
	{
		Entity entity = card.GetEntity();
		if (entity.IsSpell() || entity.IsMinion() || entity.IsLettuceAbility())
		{
			Spell powerUpSpell = card.GetActorSpell(SpellType.POWER_UP);
			if (powerUpSpell != null)
			{
				powerUpSpell.ActivateState(SpellStateType.BIRTH);
			}
		}
		card.DeactivateHandStateSpells();
	}

	private void ActivatePlaySpell(Card card)
	{
		Entity entity = card.GetEntity();
		if (!entity.HasTag(GAME_TAG.CARD_DOES_NOTHING))
		{
			Entity parentEntity = entity.GetParentEntity();
			Spell playSpell;
			if (parentEntity == null)
			{
				playSpell = card.GetPlaySpell(0);
			}
			else
			{
				Card parentCard = parentEntity.GetCard();
				int subOption = parentEntity.GetSubCardIndex(entity);
				playSpell = parentCard.GetSubOptionSpell(subOption, 0);
			}
			if (playSpell != null && playSpell.GetActiveState() == SpellStateType.NONE)
			{
				playSpell.ActivateState(SpellStateType.BIRTH);
			}
			HandleLegendaryHeroPowerBirth(entity);
		}
	}

	private void HandleMouseMove()
	{
		if (m_gameState != null && m_gameState.IsInTargetMode())
		{
			HandleUpdateWhileNotHoldingCard();
		}
	}

	private void HandleUpdateWhileNotHoldingCard()
	{
		if (!m_universalInputManager.IsTouchMode() || !m_targetReticleManager.IsLocalArrowActive())
		{
			m_myHandZone.HandleInput();
		}
		if ((!m_universalInputManager.IsTouchMode() || (InputCollection.GetMouseButton(0) && !(m_lastObjectTwoFingerDown != null))) && m_universalInputManager.GetInputHitInfo(GameLayer.CardRaycast, out var hitInfo))
		{
			CardStandIn standIn = null;
			Actor actor = GameObjectUtils.FindComponentInParents<Actor>(hitInfo.transform);
			if (hitInfo.collider.gameObject.GetComponent<TeammatePingOptions>() != null)
			{
				HandleMouseOverObjectWhileNotHoldingCard(hitInfo);
				return;
			}
			if (actor == null)
			{
				standIn = GameObjectUtils.FindComponentInParents<CardStandIn>(hitInfo.transform);
				if (standIn == null)
				{
					HandleMouseOverObjectWhileNotHoldingCard(hitInfo);
					return;
				}
			}
			if (m_mousedOverObject != null)
			{
				HandleMouseOffLastObject();
			}
			Card card = null;
			if (actor != null)
			{
				card = actor.GetCard();
			}
			if (card == null)
			{
				if (m_gameState == null || m_gameState.IsMulliganManagerActive())
				{
					if (m_mousedOverCard != null)
					{
						HandleMouseOffCard();
					}
					return;
				}
				if (standIn == null)
				{
					return;
				}
				card = standIn.linkedCard;
			}
			if (IsCancelingBattlecryCard(card) || (m_useHandEnlarge && m_myHandZone.HandEnlarged() && card.GetEntity().IsCardButton() && !card.GetEntity().IsLocation() && card.GetEntity().IsControlledByLocalUser() && m_myHandZone.GetCardCount() > 1))
			{
				return;
			}
			if (card != m_mousedOverCard && ((card.GetZone() != m_myHandZone && !(card.GetZone() is ZoneTeammateHand)) || m_gameState.IsMulliganManagerActive()))
			{
				if (m_mousedOverCard != null)
				{
					HandleMouseOffCard();
				}
				HandleMouseOverCard(card);
			}
			PegCursor.Get().SetMode(PegCursor.Mode.OVER);
		}
		else
		{
			HandleMouseOff();
		}
	}

	private void HandleMouseOverObjectWhileNotHoldingCard(RaycastHit hitInfo)
	{
		if (m_mousedOverCard != null)
		{
			HandleMouseOffCard();
		}
		if (m_universalInputManager.IsTouchMode() && !InputCollection.GetMouseButton(0))
		{
			if (m_mousedOverObject != null)
			{
				HandleMouseOffLastObject();
			}
			return;
		}
		bool targetRecticlePreventsCardTooltip = m_targetReticleManager != null && m_targetReticleManager.IsLocalArrowActive();
		bool permitDecisionMakingInput = PermitDecisionMakingInput();
		if (!permitDecisionMakingInput)
		{
			targetRecticlePreventsCardTooltip = false;
		}
		GameObject hitObject = hitInfo.collider.gameObject;
		if (hitObject.GetComponent<HistoryManager>() != null && !targetRecticlePreventsCardTooltip)
		{
			m_mousedOverObject = hitObject;
			HistoryManager.Get().NotifyOfInput(hitInfo.point.z);
			return;
		}
		if (hitObject.GetComponent<PlayerLeaderboardManager>() != null && !targetRecticlePreventsCardTooltip)
		{
			m_mousedOverObject = hitObject;
			PlayerLeaderboardManager.Get().NotifyOfInput(hitInfo.point);
			return;
		}
		if (BattlegroundsEmoteHandler.TryGetActiveInstance(out var battlegroundsEmoteHandler))
		{
			if (hitObject.TryGetComponent<BattlegroundsEmoteOption>(out var battlegroundsEmoteOption))
			{
				m_mousedOverObject = hitObject;
				battlegroundsEmoteHandler.HandleMouseOver(battlegroundsEmoteOption);
				return;
			}
			if (hitObject == battlegroundsEmoteHandler.gameObject)
			{
				m_mousedOverObject = hitObject;
				battlegroundsEmoteHandler.HandleMouseOut();
				return;
			}
		}
		if (m_mousedOverObject == hitObject)
		{
			return;
		}
		if (m_mousedOverObject != null)
		{
			HandleMouseOffLastObject();
		}
		if (hitObject.GetComponent<TeammatePingOptions>() != null)
		{
			m_mousedOverObject = hitObject;
			hitObject.GetComponent<TeammatePingOptions>().MousedOver();
			return;
		}
		if ((bool)EndTurnButton.Get() && permitDecisionMakingInput && !EndTurnButton.Get().IsInputBlocked())
		{
			if (hitObject.GetComponent<EndTurnButton>() != null)
			{
				m_mousedOverObject = hitObject;
				EndTurnButton.Get().HandleMouseOver();
			}
			else if (hitObject.GetComponent<DuosPortal>() != null)
			{
				m_mousedOverObject = hitObject;
				hitObject.GetComponent<DuosPortal>().HandleMouseOver();
			}
			else
			{
				EndTurnButtonReminder reminder = hitObject.GetComponent<EndTurnButtonReminder>();
				if (reminder != null && reminder.ShowFriendlySidePlayerTurnReminder())
				{
					m_mousedOverObject = hitObject;
				}
			}
		}
		TooltipZone tooltip = hitObject.GetComponent<TooltipZone>();
		if (tooltip != null)
		{
			m_mousedOverObject = hitObject;
			ShowTooltipZone(hitObject, tooltip);
		}
		GameOpenPack tutorialPack = hitObject.GetComponent<GameOpenPack>();
		if (tutorialPack != null)
		{
			m_mousedOverObject = hitObject;
			tutorialPack.NotifyOfMouseOver();
		}
		if (hitObject.GetComponent<AnomalyMedallion>() != null)
		{
			AnomalyMedallion.Open();
		}
		_ = GetBattlecrySourceCard() != null;
	}

	private void HandleMouseOff()
	{
		if ((bool)m_mousedOverCard)
		{
			Card remoteCard = RemoteActionHandler.Get().GetFriendlyHoverCard();
			if (m_mousedOverCard != remoteCard)
			{
				HandleMouseOffCard();
			}
		}
		if ((bool)m_mousedOverObject)
		{
			HandleMouseOffLastObject();
		}
	}

	private void HandleMouseOffLastObject()
	{
		EndTurnButton endTurnButton = m_mousedOverObject.GetComponent<EndTurnButton>();
		if ((bool)endTurnButton)
		{
			endTurnButton.HandleMouseOut();
			m_lastObjectMousedDown = null;
		}
		else if (m_mousedOverObject.GetComponent<DuosPortal>() != null)
		{
			m_mousedOverObject.GetComponent<DuosPortal>().HandleMouseOut();
			m_lastObjectMousedDown = null;
		}
		else if (m_mousedOverObject.GetComponent<TeammatePingOptions>() != null)
		{
			m_mousedOverObject.GetComponent<TeammatePingOptions>().MousedOut();
			m_lastObjectMousedDown = null;
		}
		else if ((bool)m_mousedOverObject.GetComponent<EndTurnButtonReminder>())
		{
			m_lastObjectMousedDown = null;
		}
		else
		{
			TooltipZone tooltipZone = m_mousedOverObject.GetComponent<TooltipZone>();
			BattlegroundsEmoteHandler handler;
			BattlegroundsEmoteOption component;
			if (tooltipZone != null)
			{
				tooltipZone.HideTooltip();
				m_gameState.GetGameEntity()?.NotifyOfTooltipHide(tooltipZone);
				m_lastObjectMousedDown = null;
			}
			else if (m_mousedOverObject.GetComponent<HistoryManager>() != null)
			{
				HistoryManager.Get().NotifyOfMouseOff();
			}
			else if (m_mousedOverObject.GetComponent<PlayerLeaderboardManager>() != null)
			{
				PlayerLeaderboardManager.Get().NotifyOfMouseOff();
			}
			else if (BattlegroundsEmoteHandler.TryGetActiveInstance(out handler) && (handler.gameObject == m_mousedOverObject || m_mousedOverObject.TryGetComponent<BattlegroundsEmoteOption>(out component)))
			{
				handler.HideEmotes();
				m_lastObjectMousedDown = null;
			}
			else
			{
				GameOpenPack gameOpenPack = m_mousedOverObject.GetComponent<GameOpenPack>();
				if (gameOpenPack != null)
				{
					gameOpenPack.NotifyOfMouseOff();
					m_lastObjectMousedDown = null;
				}
			}
		}
		m_mousedOverObject = null;
	}

	private void SetHeldCardValue(Card newValue)
	{
		if (m_heldCard != null && newValue == null)
		{
			Entity heldEntity = m_heldCard.GetEntity();
			if (heldEntity != null && heldEntity.HasDeckAction() && m_myHandZone.HandEnlarged())
			{
				ManaCrystalMgr.Get().ShowPhoneManaTray();
			}
			DragRotator rotator = m_heldCard.GetComponent<DragRotator>();
			if (rotator != null)
			{
				UnityEngine.Object.Destroy(rotator);
			}
		}
		else if (m_heldCard == null && newValue != null)
		{
			Entity heldEntity2 = newValue.GetEntity();
			if (heldEntity2 != null && heldEntity2.HasDeckAction() && m_myHandZone.HandEnlarged())
			{
				ManaCrystalMgr.Get().HidePhoneManaTray();
			}
		}
		m_heldCard = newValue;
		if (newValue != null && newValue.GetZone() is ZonePlay && GameMgr.Get().IsBattlegroundDuoGame())
		{
			RemoteActionHandler.Get().NotifyOpponentOfCardPickedUp(newValue);
		}
	}

	private void GrabCard(GameObject cardObject)
	{
		if (!PermitDecisionMakingInput())
		{
			return;
		}
		Card card = cardObject.GetComponent<Card>();
		if (!card.IsInputEnabled() || !m_gameState.GetGameEntity().ShouldAllowCardGrab(card.GetEntity()))
		{
			return;
		}
		Zone zone = card.GetZone();
		if (!zone.IsInputEnabled())
		{
			return;
		}
		card.SetDoNotSort(on: true);
		float dragScale = 0.7f;
		if (zone is ZoneHand)
		{
			ZoneHand zoneHand = (ZoneHand)zone;
			if (!m_universalInputManager.IsTouchMode())
			{
				zoneHand.UpdateLayout(null);
			}
			zoneHand.OnCardGrabbed(card);
		}
		else if (zone is ZonePlay)
		{
			ZonePlay obj = (ZonePlay)zone;
			obj.RemoveCard(card);
			obj.UpdateLayout();
			card.HideTooltip();
			dragScale = 0.9f;
		}
		SetHeldCardValue(card);
		card.IsBeingDragged = true;
		SoundManager.Get().LoadAndPlay("FX_MinionSummon01_DrawFromHand_01.prefab:c8adc026a7f5d0a4cb0706627a980c58", cardObject);
		DragCardSoundEffects cardEffects = m_heldCard.GetComponent<DragCardSoundEffects>();
		if ((bool)cardEffects)
		{
			cardEffects.enabled = true;
		}
		else
		{
			cardEffects = cardObject.AddComponent<DragCardSoundEffects>();
		}
		cardEffects.Restart();
		cardObject.AddComponent<DragRotator>().SetInfo(m_DragRotatorInfo);
		ProjectedShadow shadow = card.GetActor().GetComponentInChildren<ProjectedShadow>();
		if (shadow != null)
		{
			shadow.EnableShadow(0.15f);
		}
		iTween.Stop(cardObject);
		iTween.ScaleTo(cardObject, new Vector3(dragScale, dragScale, dragScale), 0.2f);
		TooltipPanelManager.Get().HideKeywordHelp();
		CardTypeBanner.Get()?.Hide();
		card.NotifyPickedUp();
		m_gameState.GetGameEntity().NotifyOfCardGrabbed(card.GetEntity());
		LayerUtils.SetLayer(card, GameLayer.Default);
	}

	private void DropCanceledHeldCard(Card card)
	{
		SetHeldCardValue(null);
		RemoteActionHandler.Get().NotifyOpponentOfCardDropped();
		ZonePlay controllersPlayZone = GetControllersPlayZone(card.GetEntity());
		m_myHandZone.UpdateLayout(null, forced: true);
		controllersPlayZone.SortWithSpotForHeldCard(-1);
		controllersPlayZone.OnMagneticDropped(card);
		m_myHandZone.OnCardWithReplacementsDropped(card);
		SendDragDropCancelPlayTelemetry(card.GetEntity());
		card.IsBeingDragged = false;
	}

	public void ReturnHeldCardToHand()
	{
		if (!(m_heldCard == null))
		{
			Log.Hand.Print("ReturnHeldCardToHand()");
			Card heldCard = m_heldCard;
			heldCard.SetDoNotSort(on: false);
			iTween.Stop(m_heldCard.gameObject);
			Entity entity = heldCard.GetEntity();
			heldCard.NotifyLeftPlayfield();
			m_gameState.GetGameEntity().NotifyOfCardDropped(entity);
			DragCardSoundEffects cardEffects = heldCard.GetComponent<DragCardSoundEffects>();
			if ((bool)cardEffects)
			{
				cardEffects.Disable();
			}
			ProjectedShadow shadow = heldCard.GetActor().GetComponentInChildren<ProjectedShadow>();
			if (shadow != null)
			{
				shadow.DisableShadow();
			}
			RemoteActionHandler.Get().NotifyOpponentOfCardDropped();
			if (m_useHandEnlarge)
			{
				m_myHandZone.SetFriendlyHeroTargetingMode(enable: false);
			}
			m_myHandZone.UpdateLayout(m_myHandZone.GetLastMousedOverCard(), forced: true);
			m_heldCard.IsBeingDragged = false;
			SetDragging(dragging: false);
			SetHeldCardValue(null);
		}
	}

	private bool DropHeldCard(bool wasCancelled)
	{
		Log.Hand.Print("DropHeldCard - cancelled? " + wasCancelled);
		PegCursor.Get().SetMode(PegCursor.Mode.STOPDRAG);
		if (m_enlargeHandAfterDropCard)
		{
			m_enlargeHandAfterDropCard = false;
			ShowPhoneHand();
		}
		if (m_useHandEnlarge)
		{
			m_myHandZone?.SetFriendlyHeroTargetingMode(enable: false);
			if (m_hideHandAfterPlayingCard)
			{
				HidePhoneHand();
			}
			else
			{
				m_myHandZone?.UpdateLayout(null, forced: true);
			}
		}
		if (m_heldCard == null)
		{
			return false;
		}
		Card card = m_heldCard;
		card.SetDoNotSort(on: false);
		iTween.Stop(m_heldCard.gameObject);
		Entity entity = card.GetEntity();
		card.NotifyLeftPlayfield();
		card.NotifyLeftMoveMinionTarget();
		m_gameState?.GetGameEntity()?.NotifyOfCardDropped(entity);
		DragCardSoundEffects cardEffects = card.GetComponent<DragCardSoundEffects>();
		if ((bool)cardEffects)
		{
			cardEffects.Disable();
		}
		SetHeldCardValue(null);
		ProjectedShadow shadow = card.GetActor()?.GetComponentInChildren<ProjectedShadow>();
		if (shadow != null)
		{
			shadow.DisableShadow();
		}
		if (IsInZone(card, TAG_ZONE.PLAY) && card.IsInputEnabled())
		{
			MoveMinionHoverTarget hoverTarget = GetMoveMinionHoverTarget(card);
			if (hoverTarget != null && !wasCancelled)
			{
				hoverTarget.DropCardOnHoverTarget(card);
			}
			else
			{
				AddHeldCardBackToPlayZone(card);
			}
			m_gameState?.ExitMoveMinionMode();
		}
		if (IsInZone(card, TAG_ZONE.PLAY))
		{
			LayerUtils.SetLayer(card, GameLayer.CardRaycast);
		}
		else
		{
			LayerUtils.SetLayer(card, GameLayer.Default);
		}
		if (wasCancelled)
		{
			card.UpdateDeckActionHover(show: false);
			DropCanceledHeldCard(card);
			return true;
		}
		bool notifyEnemyOfTargetArrow = false;
		if (IsInZone(card, TAG_ZONE.HAND))
		{
			bool tradeAction = entity.IsTradeable() && card.IsInDeckActionArea();
			bool forgeAction = entity.IsForgeable() && card.IsInDeckActionArea();
			bool passAction = entity.IsPassable() && card.IsInDeckActionArea();
			if (tradeAction)
			{
				bool cancelDrop = false;
				DropHeldTradeable(entity, ref cancelDrop);
				ZoneDeck deck = ZoneMgr.Get().FindZoneOfType<ZoneDeck>(Player.Side.FRIENDLY);
				if (deck != null)
				{
					deck.HideTradeableGlow(justTraded: true);
				}
				card.KillTradeableHover();
				m_myHandZone.OnCardWithReplacementsDropped(card);
				m_myPlayZone.OnMagneticDropped(card);
				if (cancelDrop)
				{
					DropCanceledHeldCard(card);
					return true;
				}
			}
			else if (forgeAction)
			{
				bool cancelDrop2 = false;
				DropHeldForgeable(entity, ref cancelDrop2);
				ZoneDeck deck2 = ZoneMgr.Get().FindZoneOfType<ZoneDeck>(Player.Side.FRIENDLY);
				if (deck2 != null)
				{
					deck2.HideForgeableGlow(justForged: true);
				}
				card.KillForgeableHover();
				m_myHandZone.OnCardWithReplacementsDropped(card);
				m_myPlayZone.OnMagneticDropped(card);
				if (cancelDrop2)
				{
					DropCanceledHeldCard(card);
					return true;
				}
			}
			else if (passAction)
			{
				bool cancelDrop3 = false;
				DropHeldPassable(entity, ref cancelDrop3);
				m_myPlayZone.OnMagneticDropped(card);
				if (cancelDrop3)
				{
					DropCanceledHeldCard(card);
					return true;
				}
				card.KillPassableHover();
			}
			else if (entity.IsMinion() || entity.IsWeapon() || entity.IsLocation() || entity.IsBattlegroundTrinket())
			{
				DropHeldMinionLikeCard(card, entity, ref notifyEnemyOfTargetArrow);
				if (entity.IsMinion() && card.GetActor() != null && !m_universalInputManager.IsTouchMode())
				{
					card.GetActor().TurnOffCollider();
				}
			}
			else if (entity.IsSpell() || entity.IsHero() || entity.IsLettuceAbility() || entity.IsBattlegroundQuestReward())
			{
				bool cancelDrop4 = false;
				DropHeldSpellLikeCard(card, entity, ref cancelDrop4);
				if (cancelDrop4)
				{
					DropCanceledHeldCard(entity.GetCard());
					return true;
				}
			}
			if (!tradeAction && !passAction)
			{
				m_myHandZone.UpdateLayout(null, forced: true);
			}
			m_myPlayZone.SortWithSpotForHeldCard(-1);
		}
		if (IsInZone(card, TAG_ZONE.PLAY))
		{
			if (entity.IsMinion() || entity.IsBaconSpell())
			{
				DropHeldMinionLikeCard(card, entity, ref notifyEnemyOfTargetArrow);
			}
			GetControllersPlayZone(card.GetEntity()).SortWithSpotForHeldCard(-1);
			if (GameMgr.Get().IsBattlegroundDuoGame() && card.IsInDeckActionArea())
			{
				GameplayErrorManager.Get().DisplayMessage(GameStrings.Format("GAMEPLAY_BACON_DUO_NO_PASS_FROM_BOARD"));
			}
		}
		if (notifyEnemyOfTargetArrow)
		{
			RemoteActionHandler.Get()?.NotifyOpponentOfTargetModeBegin(card);
		}
		else
		{
			GameState gameState = m_gameState;
			if (gameState == null || gameState.GetResponseMode() != GameState.ResponseMode.SUB_OPTION)
			{
				RemoteActionHandler.Get().NotifyOpponentOfCardDropped();
			}
		}
		return true;
	}

	public ZonePlay GetControllersPlayZone(Entity entity)
	{
		if (!entity.IsControlledByFriendlySidePlayer())
		{
			return m_enemyPlayZone;
		}
		return m_myPlayZone;
	}

	public void AddHeldCardBackToPlayZone(Card card)
	{
		GetControllersPlayZone(card.GetEntity()).AddCard(card);
	}

	private void SendDragDropCancelPlayTelemetry(Entity cancelledEntity)
	{
		if (cancelledEntity != null && GameMgr.Get() != null)
		{
			TelemetryManager.Client().SendDragDropCancelPlayCard(GameMgr.Get().GetMissionId(), ((TAG_CARDTYPE)cancelledEntity.GetTag(GAME_TAG.CARDTYPE)/*cast due to .constrained prefix*/).ToString());
		}
	}

	private void DropHeldMinionLikeCard(Card card, Entity entity, ref bool notifyEnemyOfTargetArrow)
	{
		if (card == null || entity == null)
		{
			Debug.LogWarningFormat("DropHeldMinionLikeCard() is called with the invalid card or entity.");
			return;
		}
		ZonePlay playZone = GetControllersPlayZone(card.GetEntity());
		bool isMinionLike = entity.IsMinion() || entity.IsLocation() || entity.IsBaconSpell() || entity.IsBattlegroundTrinket();
		bool isWeapon = entity.IsWeapon();
		if (!isMinionLike && !isWeapon)
		{
			Debug.LogWarningFormat("DropHeldMinionLikeCard() is called with the card: {0}", entity.GetCardId());
			card.IsBeingDragged = false;
			return;
		}
		if (!m_universalInputManager.GetInputHitInfo(Camera.main, GameLayer.InvisibleHitBox2, out var hitInfo))
		{
			playZone.OnMagneticDropped(card);
			SendDragDropCancelPlayTelemetry(entity);
			card.IsBeingDragged = false;
			return;
		}
		Zone zone = ((!isWeapon) ? ((Zone)playZone) : ((Zone)m_myWeaponZone));
		bool canChangeController = AllowMovingMinionAcrossPlayZone();
		if (isMinionLike && canChangeController)
		{
			Player.Side side = (IsOverFriendlyPlayZone(hitInfo) ? Player.Side.FRIENDLY : Player.Side.OPPOSING);
			zone = ZoneMgr.Get().FindZoneOfType<ZonePlay>(side);
		}
		if ((bool)zone)
		{
			int zonePos = 0;
			int optionPos = 0;
			if (isMinionLike)
			{
				zonePos = PlayZoneSlotMousedOver(zone as ZonePlay, card);
				if (zonePos < 0)
				{
					PlayErrors.DisplayPlayError(PlayErrors.ErrorType.REQ_MINION_CAP, null, entity);
					return;
				}
				if (canChangeController)
				{
					optionPos = zonePos;
					m_gameState.SetSelectedOptionPosition(optionPos);
					m_gameState.SetSelectedOptionTarget(zone.GetController().GetEntityId());
				}
				else
				{
					optionPos = ZoneMgr.Get().PredictZonePosition(entity, zone, zonePos);
					m_gameState.SetSelectedOptionPosition(optionPos);
				}
				if (optionPos < 0)
				{
					PlayErrors.DisplayPlayError(PlayErrors.ErrorType.REQ_MINION_CAP, null, entity);
					playZone.OnMagneticDropped(card);
					m_gameState.SetSelectedOptionPosition(0);
					return;
				}
			}
			if (DoNetworkResponse(entity))
			{
				m_myHandZone.OnCardWithReplacementsPlayed(card);
				if (IsInZone(card, TAG_ZONE.HAND))
				{
					m_lastZoneChangeList = ZoneMgr.Get().AddPredictedLocalZoneChange(card, zone, zonePos, optionPos);
					PredictSpentMana(entity);
					playZone.OnMagneticPlay(card, optionPos);
					if (isMinionLike && m_gameState.EntityHasTargets(entity))
					{
						notifyEnemyOfTargetArrow = true;
						bool showArrow = !m_universalInputManager.IsTouchMode();
						m_targetReticleManager?.CreateFriendlyTargetArrow(entity, showDamageIndicatorText: true, showArrow);
						m_battlecrySourceCard = card;
						if (m_universalInputManager.IsTouchMode())
						{
							StartBattleCryEffect(entity);
						}
					}
				}
				else if (IsInZone(card, TAG_ZONE.PLAY) && (card.GetZone() != zone || card.GetZonePosition() != optionPos))
				{
					m_lastZoneChangeList = ZoneMgr.Get().AddPredictedLocalZoneChange(card, zone, zonePos, optionPos);
					card.m_minionWasMovedFromSrcToDst = new ZonePositionChange
					{
						m_sourceZonePosition = card.GetZonePosition(),
						m_destinationZonePosition = zonePos
					};
				}
			}
			else
			{
				m_gameState.SetSelectedOptionPosition(0);
				TooltipPanelManager.Get().ResetCardForTooltips();
			}
		}
		card.IsBeingDragged = false;
	}

	private void DropHeldSpellLikeCard(Card card, Entity entity, ref bool cancelDrop)
	{
		if (card == null || entity == null)
		{
			Debug.LogWarningFormat("DropHeldSpellLikeCard() is called with the invalid card or entity.");
			return;
		}
		if (!entity.IsSpell() && !entity.IsHero() && !entity.IsLettuceAbility() && !entity.IsBattlegroundQuestReward())
		{
			Debug.LogWarningFormat("DropHeldSpellLikeCard() is called with the card: {0}", entity.GetCardId());
			return;
		}
		if (m_gameState.EntityHasTargets(entity) && !entity.HasDeckAction())
		{
			cancelDrop = true;
			return;
		}
		if (!m_universalInputManager.GetInputHitInfo(Camera.main, GameLayer.InvisibleHitBox2, out var _))
		{
			m_myHandZone.OnCardWithReplacementsDropped(card);
			SendDragDropCancelPlayTelemetry(entity);
			return;
		}
		if (!m_gameState.HasResponse(entity, false))
		{
			PlayErrors.DisplayPlayError(m_gameState.GetErrorType(entity), m_gameState.GetErrorParam(entity), entity);
			return;
		}
		m_myHandZone.OnCardWithReplacementsPlayed(card);
		DoNetworkResponse(entity);
		m_lastZoneChangeList = ZoneMgr.Get().AddLocalZoneChange(card, TAG_ZONE.PLAY);
		PredictSpentMana(entity);
		if (entity.IsSpell())
		{
			if (m_gameState.HasSubOptions(entity))
			{
				card.DeactivateHandStateSpells();
				return;
			}
			ActivatePowerUpSpell(card);
			ActivatePlaySpell(card);
		}
	}

	private void DropHeldTradeable(Entity entity, ref bool cancelDrop)
	{
		bool canTrade = DoNetworkResponse(entity, checkValidInput: true, wantWantDeckOption: true);
		if (!canTrade)
		{
			Card card = entity.GetCard();
			if (card != null && !card.HasEnoughManaToDeckAction())
			{
				PlayErrors.DisplayPlayError(PlayErrors.ErrorType.REQ_ENOUGH_MANA, null, entity);
			}
			else
			{
				PlayErrors.DisplayPlayError(PlayErrors.ErrorType.REQ_TRADEABLE, null, entity);
			}
		}
		cancelDrop = !canTrade;
	}

	private void DropHeldForgeable(Entity entity, ref bool cancelDrop)
	{
		bool canForge = DoNetworkResponse(entity, checkValidInput: true, wantWantDeckOption: true);
		if (!canForge)
		{
			Card card = entity.GetCard();
			if (card != null && !card.HasEnoughManaToDeckAction())
			{
				PlayErrors.DisplayPlayError(PlayErrors.ErrorType.REQ_ENOUGH_MANA, null, entity);
			}
			else
			{
				PlayErrors.DisplayPlayError(PlayErrors.ErrorType.REQ_FORGE, null, entity);
			}
		}
		else
		{
			StartCoroutine(entity.GetCard().WaitAndForgeCard());
		}
		cancelDrop = !canForge;
	}

	private void DropHeldPassable(Entity entity, ref bool cancelDrop)
	{
		bool canPass = DoNetworkResponse(entity, checkValidInput: true, wantWantDeckOption: true);
		if (!canPass)
		{
			Card card = entity.GetCard();
			if (card != null && !card.HasEnoughManaToDeckAction())
			{
				PlayErrors.DisplayPlayError(PlayErrors.ErrorType.REQ_ENOUGH_MANA, null, entity, null, "GAMEPLAY_BACON_DUO_NO_GOLD_FOR_PASS");
			}
			else
			{
				PlayErrors.DisplayPlayError(PlayErrors.ErrorType.REQ_BACON_DUO_PASSABLE, null, entity);
			}
		}
		cancelDrop = !canPass;
	}

	private void ShowCardPingWheel(Card card)
	{
		Entity entity = card.GetEntity();
		if (GameMgr.Get().IsBattlegroundDuoGame() && !GameMgr.Get().IsSpectator())
		{
			ZoneHand friendlyHand = GetFriendlyHand();
			bool inSmallHand = false;
			if ((bool)UniversalInputManager.UsePhoneUI && !GameState.Get().IsMulliganManagerActive())
			{
				inSmallHand = ((!TeammateBoardViewer.Get().GetTeammateHandViewer().IsActorInViewer(card.GetActor())) ? (!friendlyHand.HandEnlarged() && entity.GetZone() == TAG_ZONE.HAND) : (!TeammateBoardViewer.Get().GetTeammateHandViewer().IsHandEnlarged()));
			}
			if (TeammatePingWheelManager.Get() != null && !inSmallHand)
			{
				TeammatePingWheelManager.Get().ShowPingWheel(card.GetActor());
			}
		}
	}

	private void HandleRightClickOnCard(Card card)
	{
		if (m_gameState.IsInTargetMode() || m_gameState.IsInSubOptionMode() || m_heldCard != null)
		{
			HandleRightClick();
			return;
		}
		ShowCardPingWheel(card);
		Entity entity = card.GetEntity();
		if (!entity.IsHero())
		{
			return;
		}
		if (entity.IsControlledByLocalUser())
		{
			EmoteHandler emoteHandler = EmoteHandler.Get();
			if (emoteHandler != null)
			{
				if (emoteHandler.AreEmotesActive())
				{
					emoteHandler.HideEmotes();
				}
				else
				{
					emoteHandler.ShowEmotes();
				}
				return;
			}
			BattlegroundsEmoteHandler battlegroundsEmoteHandler = BattlegroundsEmoteHandler.Get();
			if (GameMgr.Get().IsBattlegroundsMatchOrTutorial() && battlegroundsEmoteHandler != null && !GameState.Get().IsMulliganPhase() && (TeammateBoardViewer.Get() == null || !TeammateBoardViewer.Get().IsViewingTeammate()))
			{
				if (battlegroundsEmoteHandler.AreEmotesActive())
				{
					battlegroundsEmoteHandler.HideEmotes();
				}
				else
				{
					battlegroundsEmoteHandler.ShowEmotes();
				}
			}
			return;
		}
		EnemyEmoteHandler enemyEmoteHandler = EnemyEmoteHandler.Get();
		bool canShowOpponentEmoteMenu = enemyEmoteHandler != null;
		if (GameMgr.Get().IsSpectator() && entity.GetControllerSide() != Player.Side.OPPOSING)
		{
			canShowOpponentEmoteMenu = false;
		}
		if (canShowOpponentEmoteMenu)
		{
			if (enemyEmoteHandler.AreEmotesActive())
			{
				enemyEmoteHandler.HideEmotes();
			}
			else
			{
				enemyEmoteHandler.ShowEmotes();
			}
		}
	}

	private void HandleClickOnCard(GameObject upClickedCard, bool wasMouseDownTarget)
	{
		EmoteHandler emoteHandler = EmoteHandler.Get();
		if (emoteHandler != null)
		{
			if (emoteHandler.IsMouseOverEmoteOption())
			{
				return;
			}
			emoteHandler.HideEmotes();
		}
		BattlegroundsEmoteHandler battlegroundsEmoteHandler = BattlegroundsEmoteHandler.Get();
		if (GameMgr.Get().IsBattlegroundsMatchOrTutorial() && battlegroundsEmoteHandler != null)
		{
			if (battlegroundsEmoteHandler.IsMouseOverEmoteOption)
			{
				return;
			}
			battlegroundsEmoteHandler.HideEmotes();
		}
		EnemyEmoteHandler enemyEmoteHandler = EnemyEmoteHandler.Get();
		if (enemyEmoteHandler != null)
		{
			if (enemyEmoteHandler.IsMouseOverEmoteOption())
			{
				return;
			}
			enemyEmoteHandler.HideEmotes();
		}
		Card card = upClickedCard.GetComponent<Card>();
		Entity entity = card.GetEntity();
		Log.Hand.Print("HandleClickOnCard - Card zone: " + card.GetZone());
		if (m_universalInputManager.IsTouchMode() && entity.IsHero() && card.GetZone() is ZoneHero && !m_gameState.IsInTargetMode() && wasMouseDownTarget)
		{
			if (entity.IsControlledByLocalUser())
			{
				if (emoteHandler != null && !m_gameState.IsInChoiceMode())
				{
					emoteHandler.ShowEmotes();
				}
				else if (GameMgr.Get().IsBattlegroundsMatchOrTutorial() && battlegroundsEmoteHandler != null)
				{
					battlegroundsEmoteHandler.ShowEmotes();
				}
				return;
			}
			if (!GameMgr.Get().IsSpectator() && enemyEmoteHandler != null && !m_gameState.IsInChoiceMode())
			{
				enemyEmoteHandler.ShowEmotes();
				return;
			}
		}
		if (card.GetEntity().IsMoveMinionHoverTarget() || (card.GetEntity().IsLaunchpad() && m_isShowingStarshipUI))
		{
			return;
		}
		if (card == ChoiceCardMgr.Get().GetSubOptionParentCard())
		{
			CancelOption();
			return;
		}
		GameState.ResponseMode responseMode = m_gameState.GetResponseMode();
		if (IsInZone(card, TAG_ZONE.HAND))
		{
			if (m_gameState.IsMulliganManagerActive())
			{
				if (PermitDecisionMakingInput() && ShouldSelectForMulligan())
				{
					MulliganManager.Get().ToggleHoldState(card);
				}
			}
			else if (!card.IsAttacking() && !m_gameState.IsInChoiceMode() && !m_gameState.IsInTargetMode() && !m_universalInputManager.IsTouchMode() && card.GetEntity().IsControlledByLocalUser() && !ChoiceCardMgr.Get().IsFriendlyShown() && GetBattlecrySourceCard() == null && (card.GetZone().m_ServerTag == TAG_ZONE.HAND || m_gameState.HasResponse(entity, null)))
			{
				GrabCard(upClickedCard);
			}
			return;
		}
		switch (responseMode)
		{
		case GameState.ResponseMode.SUB_OPTION:
			HandleClickOnSubOption(entity);
			return;
		case GameState.ResponseMode.CHOICE:
			HandleClickOnChoice(entity);
			return;
		}
		if (IsInZone(card, TAG_ZONE.PLAY))
		{
			HandleClickOnCardInBattlefield(entity, wasMouseDownTarget);
		}
	}

	public float GetMouseOverDelay(Entity entity)
	{
		if (m_gameState.GetGameEntity().ShowMouseOverBigCardImmediately(entity))
		{
			return 0f;
		}
		return m_MouseOverDelay;
	}

	private void HandleClickOnCardInBattlefield(Entity clickedEntity, bool wasMouseDownTarget = true)
	{
		if (clickedEntity == null || !PermitDecisionMakingInput() || m_isShowingStarshipUI)
		{
			return;
		}
		PegCursor.Get().SetMode(PegCursor.Mode.STOPDRAG);
		if (m_universalInputManager.IsTouchMode() && clickedEntity.IsCardButton() && !m_gameState.IsInTargetMode() && m_mousedOverTimer > GetMouseOverDelay(clickedEntity))
		{
			return;
		}
		Card clickedCard = clickedEntity.GetCard();
		if (clickedEntity.IsGameModeButton() && clickedCard != null && clickedCard.GetPlaySpell(0) != null && clickedCard.GetPlaySpell(0).GetActiveState() != 0)
		{
			return;
		}
		if ((clickedEntity.IsHeroPower() || clickedEntity.IsGameModeButton()) && clickedCard != null && clickedCard.GetActor() != null)
		{
			clickedCard.GetActor().RemovePingAndNotifyTeammate();
		}
		GameEntity gameEntity = m_gameState.GetGameEntity();
		if (!gameEntity.NotifyOfBattlefieldCardClicked(clickedEntity, m_gameState.IsInTargetMode()))
		{
			return;
		}
		if (m_gameState.IsInTargetMode())
		{
			DisableSkullIfNeeded(clickedCard);
			Network.Options.Option.SubOption subOption = m_gameState.GetSelectedNetworkSubOption();
			if (subOption.ID == clickedEntity.GetEntityId())
			{
				CancelOption();
				return;
			}
			UpdateTelemetryAttackInputCounts(m_gameState.GetEntity(subOption.ID));
			if (DoNetworkResponse(clickedEntity) && m_heldCard != null)
			{
				Card heldCard = m_heldCard;
				m_myHandZone.OnCardWithReplacementsPlayed(heldCard);
				SetHeldCardValue(null);
				heldCard.SetDoNotSort(on: false);
				m_lastZoneChangeList = ZoneMgr.Get().AddLocalZoneChange(heldCard, TAG_ZONE.PLAY);
			}
			return;
		}
		ACTION_STEP_TYPE actionStepType = m_gameState.GetActionStepType();
		bool isMouseUp = InputCollection.GetMouseButtonUp(0);
		if (gameEntity is LettuceMissionEntity && m_gameState.IsActionStep() && actionStepType == ACTION_STEP_TYPE.DEFAULT)
		{
			if (clickedCard != null && !clickedCard.IsInputEnabled())
			{
				return;
			}
			int lettuceController = clickedEntity.GetTag(GAME_TAG.LETTUCE_CONTROLLER);
			if ((isMouseUp || m_dragging) && wasMouseDownTarget && lettuceController != 0 && lettuceController == m_gameState.GetLocalSidePlayer().GetPlayerId())
			{
				if (clickedEntity.IsMinion())
				{
					if (!m_gameState.IsResponsePacketBlocked() && ZoneMgr.Get().GetLettuceAbilitiesSourceEntity() != clickedEntity)
					{
						HandleMouseOffCard();
						ZoneMgr.Get().DisplayLettuceAbilitiesForEntity(clickedEntity);
						RemoteActionHandler.Get().NotifyOpponentOfSelection(clickedEntity.GetEntityId());
					}
					return;
				}
				if (clickedEntity.IsLettuceAbility())
				{
					Entity abilityOwner = clickedEntity.GetLettuceAbilityOwner();
					if (abilityOwner != null && abilityOwner.GetSelectedLettuceAbilityID() == clickedEntity.GetEntityId())
					{
						CancelSelectedLettuceAbilityForEntity(abilityOwner);
						return;
					}
				}
			}
		}
		if (isMouseUp && m_universalInputManager.IsTouchMode() && m_gameState.EntityHasTargets(clickedEntity))
		{
			if (clickedCard != null && !clickedCard.IsShowingTooltip() && m_gameState.IsFriendlySidePlayerTurn())
			{
				PlayErrors.DisplayPlayError(PlayErrors.ErrorType.REQ_DRAG_TO_PLAY, null, clickedEntity);
			}
		}
		else if (clickedEntity.IsWeapon() && clickedEntity.IsControlledByLocalUser() && !m_gameState.IsValidOption(clickedEntity, null))
		{
			HandleClickOnCardInBattlefield(m_gameState.GetFriendlySidePlayer().GetHero());
		}
		else
		{
			if (clickedEntity.IsStarship() && clickedEntity.HasTag(GAME_TAG.LAUNCHPAD) && !isMouseUp && m_dragging)
			{
				return;
			}
			if (clickedEntity.IsStarship() && clickedEntity.IsControlledByOpposingSidePlayer() && clickedEntity.HasTag(GAME_TAG.LAUNCHPAD))
			{
				ChoiceCardMgr choiceCardMgr = ChoiceCardMgr.Get();
				if (choiceCardMgr != null && !m_isShowingStarshipUI)
				{
					choiceCardMgr.ShowStarshipPiecesForOpposingPlayer(clickedEntity);
					m_isShowingStarshipUI = true;
					SetShouldHideTooltip();
				}
			}
			else if ((gameEntity.HasTag(GAME_TAG.ALLOW_MOVE_MINION) && clickedEntity.IsMinion()) || (gameEntity.HasTag(GAME_TAG.ALLOW_MOVE_BACON_SPELL) && clickedEntity.IsBaconSpell()))
			{
				if ((!(clickedCard != null) || clickedCard.IsInputEnabled()) && !clickedEntity.HasTag(GAME_TAG.CANT_MOVE_MINION) && (!m_universalInputManager.IsTouchMode() || (!(m_mousedOverTimer > GetMouseOverDelay(clickedEntity)) && !InputCollection.GetMouseButtonUp(0) && (!(TeammateBoardViewer.Get() != null) || !TeammateBoardViewer.Get().IsViewingTeammate()))))
				{
					if (!AllowMovingMinionAcrossPlayZone() && !clickedEntity.IsControlledByFriendlySidePlayer() && !m_gameState.HasValidHoverTargetForMovedMinion(clickedEntity, out var mainOptionPlayError))
					{
						PlayErrors.DisplayPlayError(mainOptionPlayError, null, clickedEntity);
					}
					else if (clickedCard != null)
					{
						GrabCard(clickedCard.gameObject);
						m_gameState.EnterMoveMinionMode(clickedEntity);
					}
				}
			}
			else
			{
				if ((!clickedEntity.IsTitan() || !clickedEntity.HasUsableTitanAbilities() || isMouseUp || !m_dragging) && !DoNetworkResponse(clickedEntity))
				{
					return;
				}
				if (clickedCard != null && clickedCard.GetActor() is LettuceAbilityActor lettuceAbilityActor)
				{
					lettuceAbilityActor.PlayMouseClickedSound();
				}
				if (!m_gameState.IsInTargetMode())
				{
					if (clickedEntity.IsCardButton())
					{
						if (!clickedEntity.HasSubCards())
						{
							ActivatePlaySpell(clickedCard);
						}
						if (clickedEntity.IsHeroPower() || clickedEntity.IsGameModeButton() || (clickedEntity.IsCoinBasedHeroBuddy() && clickedEntity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_ENT_2) == 0))
						{
							clickedEntity.SetTagAndHandleChange(GAME_TAG.EXHAUSTED, 1);
							PredictSpentMana(clickedEntity);
						}
					}
					return;
				}
				RemoteActionHandler.Get().NotifyOpponentOfTargetModeBegin(clickedCard);
				if ((bool)m_targetReticleManager)
				{
					bool showDamageIndicatorText = false;
					if (clickedEntity.IsLettuceAbility())
					{
						showDamageIndicatorText = true;
						ZoneMgr.Get().TemporarilyDismissMercenariesAbilityTray();
					}
					m_targetReticleManager.CreateFriendlyTargetArrow(clickedEntity, showDamageIndicatorText, showArrow: true, null, useHandAsOrigin: false, isAttackArrow: true);
				}
				if (clickedEntity.IsCardButton())
				{
					m_targettingHeroPower = true;
					ActivatePlaySpell(clickedCard);
				}
				else
				{
					if (!clickedEntity.IsCharacter())
					{
						return;
					}
					if (clickedCard != null)
					{
						clickedCard.ActivateCharacterAttackEffects();
					}
					if (!clickedEntity.HasTag(GAME_TAG.IGNORE_TAUNT))
					{
						m_gameState.ShowEnemyTauntCharacters();
					}
					if (!(clickedCard != null) || clickedCard.IsAttacking())
					{
						return;
					}
					ISpell actorAttackSpell = clickedCard.GetActorAttackSpellForInput();
					if (actorAttackSpell != null)
					{
						if (clickedEntity.GetRealTimeIsImmuneWhileAttacking())
						{
							clickedCard.GetActor().ActivateSpellBirthState(SpellType.IMMUNE);
						}
						actorAttackSpell.ActivateState(SpellStateType.BIRTH);
					}
				}
			}
		}
	}

	public void CancelSelectedLettuceAbilityForEntity(Entity mercenaryEntity)
	{
		if (!m_gameState.IsResponsePacketBlocked() && m_gameState.GetGameEntity() is LettuceMissionEntity lettuceMissionEntity && DoNetworkOptions(mercenaryEntity))
		{
			lettuceMissionEntity.SetEntityThatJustCancelledAbilitySelection(mercenaryEntity);
		}
	}

	private void ManuallyDismissMercenariesAbilityTray()
	{
		if (!m_universalInputManager.IsDialogActive())
		{
			ChoiceCardMgr ccm = ChoiceCardMgr.Get();
			if (!ccm.IsShown() && !ccm.IsWaitingToShowSubOptions() && !m_gameState.GetBooleanGameOption(GameEntityOption.DISABLE_MANUAL_DISMISSAL_OF_MERC_ABILITY_TRAY) && !GameState.Get().IsInTargetMode())
			{
				ZoneMgr.Get().DismissMercenariesAbilityTray();
				RemoteActionHandler.Get().NotifyOpponentOfSelection(0);
			}
		}
	}

	private void UpdateTelemetryAttackInputCounts(Entity sourceEntity)
	{
		if (sourceEntity != null && !(m_battlecrySourceCard != null) && (sourceEntity.IsMinion() || sourceEntity.IsHero()))
		{
			if (m_lastInputDrag)
			{
				m_telemetryNumDragAttacks++;
			}
			else
			{
				m_telemetryNumClickAttacks++;
			}
		}
	}

	public void HandleClickOnSubOption(Entity entity, bool isSimulated = false)
	{
		if (!isSimulated && !PermitDecisionMakingInput())
		{
			return;
		}
		if ((isSimulated || m_gameState.HasResponse(entity, null)) && entity != null)
		{
			bool subEntityHasTargets = false;
			ChoiceCardMgr choiceCardMgr = ChoiceCardMgr.Get();
			Card parentCard = choiceCardMgr.GetSubOptionParentCard();
			if (!isSimulated)
			{
				subEntityHasTargets = m_gameState.SubEntityHasTargets(entity);
				if (subEntityHasTargets)
				{
					RemoteActionHandler.Get().NotifyOpponentOfTargetModeBegin(parentCard);
					string arrowText = entity.GetTargetingArrowText();
					if (string.IsNullOrEmpty(arrowText))
					{
						arrowText = UberText.RemoveMarkupAndCollapseWhitespaces(entity.GetCardTextInHand(), replaceCarriageReturnWithBreakHint: true, preserveBreakHint: true);
					}
					Entity parentEntity = parentCard.GetEntity();
					m_targetReticleManager.CreateFriendlyTargetArrow(parentEntity, showDamageIndicatorText: true, !m_universalInputManager.IsTouchMode(), arrowText);
				}
			}
			Card card = entity.GetCard();
			if (!isSimulated)
			{
				DoNetworkResponse(entity);
			}
			ActivatePowerUpSpell(card);
			if (!isSimulated && !parentCard.IsLettuceAbility())
			{
				ActivatePlaySpell(card);
			}
			if (entity.IsMinion() || entity.IsHero())
			{
				card.HideCard();
			}
			choiceCardMgr.OnSubOptionClicked(entity);
			if (isSimulated)
			{
				choiceCardMgr.ClearSubOptions();
			}
			else if (!subEntityHasTargets)
			{
				FinishSubOptions();
			}
			if (m_universalInputManager.IsTouchMode() && !isSimulated && subEntityHasTargets)
			{
				StartMobileTargetingEffect(m_gameState.GetSelectedNetworkSubOption().Targets);
			}
		}
		else if (!m_isShowingStarshipUI || !(entity.GetCardId() != "GDB_905"))
		{
			PlayErrors.DisplayPlayError(m_gameState.GetErrorType(entity), m_gameState.GetErrorParam(entity), entity);
		}
	}

	private void HandleClickOnChoice(Entity entity)
	{
		if (PermitDecisionMakingInput())
		{
			if (entity.IsBattlegroundTrinket())
			{
				ChoiceCardMgr.Get().ChooseBGTrinket(entity);
			}
			if (DoNetworkResponse(entity))
			{
				SoundManager.Get().LoadAndPlay("HeroDropItem1.prefab:587232e6704b20942af1205d00cfc0f9");
			}
			else
			{
				PlayErrors.DisplayPlayError(m_gameState.GetErrorType(entity), m_gameState.GetErrorParam(entity), entity);
			}
		}
	}

	public void ResetBattlecrySourceCard()
	{
		if (!(m_battlecrySourceCard == null))
		{
			if (m_universalInputManager.IsTouchMode())
			{
				string displayMsg = ((!m_battlecrySourceCard.GetEntity().HasTag(GAME_TAG.BATTLECRY)) ? GameStrings.Get("GAMEPLAY_MOBILE_TARGETING_CANCELED") : GameStrings.Get("GAMEPLAY_MOBILE_BATTLECRY_CANCELED"));
				GameplayErrorManager.Get().DisplayMessage(displayMsg);
			}
			m_cancelingBattlecryCards.Add(m_battlecrySourceCard);
			Entity entity = m_battlecrySourceCard.GetEntity();
			((ISpell)m_battlecrySourceCard.GetActorSpell(SpellType.BATTLECRY))?.ActivateState(SpellStateType.CANCEL);
			((ISpell)m_battlecrySourceCard.GetPlaySpell(0))?.ActivateState(SpellStateType.CANCEL);
			m_battlecrySourceCard.GetCustomSummonSpell()?.ActivateState(SpellStateType.CANCEL);
			ZoneMgr.ChangeCompleteCallback onZoneChangeComplete = delegate(ZoneChangeList changeList, object userData)
			{
				Card item = (Card)userData;
				m_cancelingBattlecryCards.Remove(item);
			};
			ZoneMgr.Get().CancelLocalZoneChange(m_lastZoneChangeList, onZoneChangeComplete, m_battlecrySourceCard);
			m_lastZoneChangeList = null;
			RollbackSpentMana(entity);
			ClearBattlecrySourceCard();
		}
	}

	private bool IsCancelingBattlecryCard(Card card)
	{
		return m_cancelingBattlecryCards.Contains(card);
	}

	public void DoEndTurnButton()
	{
		if (PermitDecisionMakingInput() && !m_isShowingStarshipUI && !m_gameState.IsResponsePacketBlocked() && !EndTurnButton.Get().IsInputBlocked() && !EndTurnButton.Get().IsDisabled)
		{
			DoEndTurnInternal();
		}
	}

	private void DoEndTurnInternal()
	{
		switch (m_gameState.GetResponseMode())
		{
		case GameState.ResponseMode.CHOICE:
			m_gameState.SendChoices();
			break;
		case GameState.ResponseMode.OPTION:
		{
			List<Network.Options.Option> optionPacketList = m_gameState.GetOptionsPacket().List;
			for (int i = 0; i < optionPacketList.Count; i++)
			{
				Network.Options.Option.OptionType optionType = optionPacketList[i].Type;
				if (optionType == Network.Options.Option.OptionType.END_TURN || optionType == Network.Options.Option.OptionType.PASS)
				{
					if (m_gameState.GetGameEntity().NotifyOfEndTurnButtonPushed())
					{
						m_gameState.SetSelectedOption(i);
						m_gameState.SendOption();
						HidePhoneHand();
						DoEndTurnButton_Option_OnEndTurnRequested();
					}
					break;
				}
			}
			break;
		}
		}
	}

	public void DoEndTurn_Cheat()
	{
		DoEndTurnInternal();
	}

	private void DoEndTurnButton_Option_OnEndTurnRequested()
	{
		TurnTimer.Get()?.OnEndTurnRequested();
		EndTurnButton.Get().OnEndTurnRequested();
	}

	public bool DoNetworkResponse(Entity entity, bool checkValidInput = true, bool wantWantDeckOption = false)
	{
		ThinkEmoteManager.Get()?.NotifyOfActivity();
		if (checkValidInput && !m_gameState.IsEntityInputEnabled(entity))
		{
			return false;
		}
		GameState.ResponseMode responseMode = m_gameState.GetResponseMode();
		bool didNetworkStuff = false;
		switch (responseMode)
		{
		case GameState.ResponseMode.CHOICE:
			didNetworkStuff = DoNetworkChoice(entity);
			break;
		case GameState.ResponseMode.OPTION:
			didNetworkStuff = DoNetworkOptions(entity, wantWantDeckOption);
			break;
		case GameState.ResponseMode.SUB_OPTION:
			didNetworkStuff = DoNetworkSubOptions(entity);
			break;
		case GameState.ResponseMode.OPTION_TARGET:
			didNetworkStuff = DoNetworkOptionTarget(entity);
			break;
		}
		if (didNetworkStuff)
		{
			entity.GetCard().UpdateActorState();
		}
		return didNetworkStuff;
	}

	private void OnOptionsReceived(object userData)
	{
		if ((bool)m_mousedOverCard)
		{
			m_mousedOverCard.UpdateProposedManaUsage();
		}
		HidePhoneHandIfOutOfServerPlays();
	}

	private void OnCurrentPlayerChanged(Player player)
	{
		if (player.IsLocalUser())
		{
			m_entitiesThatPredictedMana.Clear();
		}
	}

	private void OnOptionRejected(Network.Options.Option option, object userData)
	{
		if (option.Type == Network.Options.Option.OptionType.POWER)
		{
			Entity entity = m_gameState.GetEntity(option.Main.ID);
			if (entity == null)
			{
				Debug.LogError("OnOptionRejected - Null Entity");
				return;
			}
			entity.GetCard().NotifyTargetingCanceled();
			if (entity.IsHeroPower() || entity.IsGameModeButton())
			{
				entity.SetTagAndHandleChange(GAME_TAG.EXHAUSTED, 0);
			}
			RollbackSpentMana(entity);
			if (entity.HasReplacementsWhenPlayed())
			{
				ZoneHand friendlyHand = GetFriendlyHand();
				friendlyHand.ActivateCardWithReplacementsDeath();
				friendlyHand.ClearReservedCard();
			}
		}
		string displayMsg = GameStrings.Get("GAMEPLAY_ERROR_PLAY_REJECTED");
		GameplayErrorManager.Get().DisplayMessage(displayMsg);
	}

	private void OnTurnTimerUpdate(TurnTimerUpdate update, object userData)
	{
		if (update.GetSecondsRemaining() <= Mathf.Epsilon || GameUtils.IsWaitingForOpponentReconnect())
		{
			CancelOption(timeout: true);
		}
		HidePlayerStarshipUI();
	}

	private void OnGameOver(TAG_PLAYSTATE playState, object userData)
	{
		HidePhoneHand();
		CancelOption();
		SendGameOverTelemetry();
	}

	private void SendGameOverTelemetry()
	{
		int totalAttacks = m_telemetryNumClickAttacks + m_telemetryNumDragAttacks;
		int percentClick = ((totalAttacks != 0) ? ((int)((double)m_telemetryNumClickAttacks * 100.0 / (double)totalAttacks)) : 0);
		int percentDrag = ((totalAttacks != 0) ? ((int)((double)m_telemetryNumDragAttacks * 100.0 / (double)totalAttacks)) : 0);
		TelemetryManager.Client().SendAttackInputMethod(totalAttacks, m_telemetryNumClickAttacks, percentClick, m_telemetryNumDragAttacks, percentDrag);
		m_telemetryNumDragAttacks = 0;
		m_telemetryNumClickAttacks = 0;
	}

	private bool DoNetworkChoice(Entity entity)
	{
		if (!m_gameState.IsChoosableEntity(entity))
		{
			PlayErrors.DisplayPlayError(PlayErrors.ErrorType.INVALID, null, entity);
			return false;
		}
		m_targetReticleManager?.DestroyFriendlyTargetArrow(isLocallyCanceled: false);
		if (m_gameState.RemoveChosenEntity(entity))
		{
			return true;
		}
		m_gameState.AddChosenEntity(entity);
		Network.EntityChoices choicePacket = m_gameState.GetFriendlyEntityChoices();
		Entity sourceEntity = GameState.Get().GetEntity(choicePacket.Source);
		if (choicePacket.IsSingleChoice() && (!m_gameState.GetBooleanGameOption(GameEntityOption.MULLIGAN_IS_CHOOSE_ONE) || MulliganManager.Get() == null || !MulliganManager.Get().IsMulliganActive()) && (sourceEntity == null || !sourceEntity.HasTag(GAME_TAG.BACON_IS_MAGIC_ITEM_DISCOVER)))
		{
			m_gameState.SendChoices();
		}
		return true;
	}

	private bool DoNetworkOptions(Entity entity, bool wantWantDeckOption = false)
	{
		int entityId = entity.GetEntityId();
		List<Network.Options.Option> optionsPacket = m_gameState.GetOptionsPacket().List;
		for (int i = 0; i < optionsPacket.Count; i++)
		{
			Network.Options.Option option = optionsPacket[i];
			if (option.Type != Network.Options.Option.OptionType.POWER)
			{
				continue;
			}
			Network.Options.Option.SubOption main = option.Main;
			if (!main.PlayErrorInfo.IsValid() || main.ID != entityId)
			{
				continue;
			}
			bool isDeckActionOption = main.IsDeckActionOption();
			if (isDeckActionOption != wantWantDeckOption)
			{
				continue;
			}
			m_gameState.SetSelectedOption(i);
			if (!option.HasValidSubOption())
			{
				List<Network.Options.Option.TargetOption> targetOptions = main.Targets;
				if (targetOptions == null || targetOptions.Count == 0)
				{
					m_gameState.SendOption();
				}
				else if (isDeckActionOption)
				{
					m_gameState.SetSelectedOptionTarget(entityId);
					m_gameState.SendOption();
				}
				else if (AllowMovingMinionAcrossPlayZone() && m_gameState.GetSelectedOptionTarget() > 0)
				{
					m_gameState.SendOption();
				}
				else
				{
					EnterOptionTargetMode(entity);
				}
			}
			else
			{
				if (entity.IsLettuceAbility())
				{
					HandleMouseOffCard();
				}
				m_gameState.EnterSubOptionMode();
				Card subOptionCard = entity.GetCard();
				if (entity.IsLaunchpad() && !m_isShowingStarshipUI)
				{
					List<int> subEntityIds = new List<int>();
					for (int j = 0; j < option.Subs.Count; j++)
					{
						subEntityIds.Add(option.Subs[j].ID);
					}
					m_isShowingStarshipUI = true;
					if (!ChoiceCardMgr.Get().ShowSubOptions(subOptionCard, subEntityIds))
					{
						CancelOption();
						m_isShowingStarshipUI = false;
					}
				}
				else if (!ChoiceCardMgr.Get().ShowSubOptions(subOptionCard))
				{
					CancelOption();
				}
			}
			return true;
		}
		if (!m_universalInputManager.IsTouchMode() || !entity.GetCard().IsShowingTooltip())
		{
			PlayErrors.DisplayPlayError(m_gameState.GetErrorType(entity), m_gameState.GetErrorParam(entity), entity);
		}
		return false;
	}

	private bool DoNetworkSubOptions(Entity entity)
	{
		int entityId = entity.GetEntityId();
		GameState state = m_gameState;
		List<Network.Options.Option.SubOption> subs = state.GetSelectedNetworkOption().Subs;
		for (int i = 0; i < subs.Count; i++)
		{
			Network.Options.Option.SubOption subOption = subs[i];
			if (subOption.PlayErrorInfo.IsValid() && subOption.ID == entityId)
			{
				state.SetSelectedSubOption(i);
				List<Network.Options.Option.TargetOption> targetOptions = subOption.Targets;
				if (targetOptions == null || targetOptions.Count == 0)
				{
					state.SendOption();
				}
				else
				{
					EnterOptionTargetMode(entity);
				}
				return true;
			}
		}
		return false;
	}

	private bool DoNetworkOptionTarget(Entity entity)
	{
		int entityId = entity.GetEntityId();
		Network.Options.Option.SubOption subOption = m_gameState.GetSelectedNetworkSubOption();
		Entity sourceEntity = m_gameState.GetEntity(subOption.ID);
		if (!subOption.IsValidTarget(entityId))
		{
			Entity targetEntity = m_gameState.GetEntity(entityId);
			int targetId = targetEntity.GetEntityId();
			PlayErrors.DisplayPlayError(subOption.GetErrorForTarget(targetId), subOption.GetErrorParamForTarget(targetId), sourceEntity, targetEntity);
			return false;
		}
		m_targetReticleManager?.DestroyFriendlyTargetArrow(isLocallyCanceled: false);
		RemoteActionHandler.Get()?.NotifyOpponentOfCardDropped();
		FinishBattlecrySourceCard();
		FinishSubOptions();
		if (sourceEntity.IsHeroPower() || sourceEntity.IsGameModeButton())
		{
			sourceEntity.SetTagAndHandleChange(GAME_TAG.EXHAUSTED, 1);
			PredictSpentMana(sourceEntity);
		}
		m_gameState.SetSelectedOptionTarget(entityId);
		m_gameState.SendOption();
		return true;
	}

	private void EnterOptionTargetMode(Entity entity)
	{
		m_gameState.EnterOptionTargetMode(entity);
		if (m_useHandEnlarge)
		{
			m_myHandZone.SetFriendlyHeroTargetingMode(m_gameState.FriendlyHeroIsTargetable());
			bool handEnlarged = m_myHandZone.HandEnlarged();
			m_enlargeHandAfterDropCard = handEnlarged || ChoiceCardMgr.Get().RestoreEnlargedHandAfterChoice();
			if (handEnlarged)
			{
				HidePhoneHand();
			}
			else
			{
				m_myHandZone.UpdateLayout(null, forced: true);
			}
		}
	}

	private void FinishBattlecrySourceCard()
	{
		if (!(m_battlecrySourceCard == null))
		{
			ClearBattlecrySourceCard();
		}
	}

	private void ClearBattlecrySourceCard()
	{
		if (m_isInBattleCryEffect && m_battlecrySourceCard != null)
		{
			EndBattleCryEffect();
		}
		m_battlecrySourceCard = null;
		RemoteActionHandler.Get().NotifyOpponentOfCardDropped();
		if (m_useHandEnlarge)
		{
			m_myHandZone.SetFriendlyHeroTargetingMode(enable: false);
			m_myHandZone.UpdateLayout(null, forced: true);
		}
	}

	private void CancelSubOptions()
	{
		ChoiceCardMgr choiceCardMgr = ChoiceCardMgr.Get();
		Card subOptionCard = choiceCardMgr.GetSubOptionParentCard();
		if (!(subOptionCard == null))
		{
			choiceCardMgr.CancelSubOptions();
			Entity choiceEntity = subOptionCard.GetEntity();
			if (choiceEntity.HasReplacementsWhenPlayed())
			{
				m_myHandZone.OnCardWithReplacementsDropped(subOptionCard);
				m_myHandZone.ClearReservedCard();
			}
			if (!choiceEntity.IsCardButton() && m_lastZoneChangeList != null && m_lastZoneChangeList.GetLocalTriggerChange().GetEntity() == choiceEntity)
			{
				ZoneMgr.Get().CancelLocalZoneChange(m_lastZoneChangeList);
				m_lastZoneChangeList = null;
			}
			if (!choiceEntity.IsTitan())
			{
				RollbackSpentMana(choiceEntity);
			}
			DropSubOptionParentCard();
		}
	}

	private void FinishSubOptions()
	{
		if (!(ChoiceCardMgr.Get().GetSubOptionParentCard() == null))
		{
			DropSubOptionParentCard();
		}
	}

	public void DropSubOptionParentCard()
	{
		Log.Hand.Print("DropSubOptionParentCard()");
		ChoiceCardMgr.Get().ClearSubOptions();
		RemoteActionHandler.Get().NotifyOpponentOfCardDropped();
		if (m_useHandEnlarge)
		{
			m_myHandZone.SetFriendlyHeroTargetingMode(enable: false);
			m_myHandZone.UpdateLayout(null, forced: true);
		}
		if (m_universalInputManager.IsTouchMode())
		{
			EndMobileTargetingEffect();
		}
	}

	public void StartPendingChoiceTarget()
	{
		Network.EntityChoices choicePacket = m_gameState.GetFriendlyEntityChoices();
		if (choicePacket == null)
		{
			return;
		}
		Entity sourceEntity = m_gameState.GetEntity(choicePacket.Source);
		if ((bool)m_targetReticleManager)
		{
			if (m_universalInputManager.IsTouchMode())
			{
				m_targetReticleManager.CreateFriendlyTargetArrow(sourceEntity, showDamageIndicatorText: true, showArrow: false);
				StartMobileTargetingEffect(choicePacket.Entities);
			}
			else
			{
				m_targetReticleManager.CreateFriendlyTargetArrow(sourceEntity, showDamageIndicatorText: true);
			}
		}
	}

	public void FinishPendingChoiceTarget()
	{
		CancelOption();
		if (m_universalInputManager.IsTouchMode())
		{
			EndMobileTargetingEffect();
		}
	}

	private void StartMobileTargetingEffect(List<Network.Options.Option.TargetOption> targets)
	{
		if (targets == null || targets.Count == 0)
		{
			return;
		}
		List<int> entityIDs = new List<int>();
		foreach (Network.Options.Option.TargetOption targetOption in targets)
		{
			if (targetOption.PlayErrorInfo.IsValid())
			{
				entityIDs.Add(targetOption.ID);
			}
		}
		StartMobileTargetingEffect(entityIDs);
	}

	private void StartMobileTargetingEffect(List<int> entityIDs)
	{
		if (entityIDs == null || entityIDs.Count == 0)
		{
			return;
		}
		m_mobileTargettingEffectActors.Clear();
		foreach (int entityID in entityIDs)
		{
			Card card = m_gameState.GetEntity(entityID).GetCard();
			if (card != null)
			{
				Actor actor = card.GetActor();
				m_mobileTargettingEffectActors.Add(actor);
				ApplyMobileTargettingEffectToActor(actor);
			}
		}
		m_screenEffectHandle.StartEffect(ScreenEffectParameters.DesaturatePerspective);
	}

	private bool IsMobileTargetingEffectActive()
	{
		return m_mobileTargettingEffectActors.Count > 0;
	}

	private void EndMobileTargetingEffect()
	{
		foreach (Actor actor in m_mobileTargettingEffectActors)
		{
			RemoveMobileTargettingEffectFromActor(actor);
		}
		m_mobileTargettingEffectActors.Clear();
		m_screenEffectHandle.StopEffect();
	}

	private void StartBattleCryEffect(Entity entity)
	{
		m_isInBattleCryEffect = true;
		Network.Options.Option option = m_gameState.GetSelectedNetworkOption();
		if (option == null)
		{
			Debug.LogError("No targets for BattleCry.");
			return;
		}
		StartMobileTargetingEffect(option.Main.Targets);
		m_battlecrySourceCard.SetBattleCrySource(source: true);
	}

	private void EndBattleCryEffect()
	{
		m_isInBattleCryEffect = false;
		EndMobileTargetingEffect();
		m_battlecrySourceCard.SetBattleCrySource(source: false);
	}

	private void ApplyMobileTargettingEffectToActor(Actor actor)
	{
		if (!(actor == null))
		{
			GameObject gameObject = actor.gameObject;
			if (!(gameObject == null))
			{
				LayerUtils.SetLayer(gameObject, GameLayer.IgnoreFullScreenEffects);
				Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
				moveArgs.Add("y", 0.8f);
				moveArgs.Add("time", 0.4f);
				moveArgs.Add("easetype", iTween.EaseType.easeOutQuad);
				moveArgs.Add("name", "position");
				moveArgs.Add("islocal", true);
				Hashtable scaleArgs = iTweenManager.Get().GetTweenHashTable();
				scaleArgs.Add("x", 1.08f);
				scaleArgs.Add("z", 1.08f);
				scaleArgs.Add("time", 0.4f);
				scaleArgs.Add("easetype", iTween.EaseType.easeOutQuad);
				scaleArgs.Add("name", "scale");
				iTween.StopByName(gameObject, "position");
				iTween.StopByName(gameObject, "scale");
				iTween.MoveTo(gameObject, moveArgs);
				iTween.ScaleTo(gameObject, scaleArgs);
			}
		}
	}

	private void RemoveMobileTargettingEffectFromActor(Actor actor)
	{
		if (actor == null)
		{
			return;
		}
		GameObject gameObject = actor.gameObject;
		if (!(gameObject == null))
		{
			LayerUtils.SetLayer(gameObject, GameLayer.Default);
			MeshRenderer actoMeshRenderer = actor.GetMeshRenderer();
			if (actoMeshRenderer != null)
			{
				LayerUtils.SetLayer(actoMeshRenderer.gameObject, GameLayer.CardRaycast);
			}
			Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
			moveArgs.Add("x", 0f);
			moveArgs.Add("y", 0f);
			moveArgs.Add("z", 0f);
			moveArgs.Add("time", 0.5f);
			moveArgs.Add("easetype", iTween.EaseType.easeOutQuad);
			moveArgs.Add("name", "position");
			moveArgs.Add("islocal", true);
			Hashtable scaleArgs = iTweenManager.Get().GetTweenHashTable();
			scaleArgs.Add("x", 1f);
			scaleArgs.Add("z", 1f);
			scaleArgs.Add("time", 0.4f);
			scaleArgs.Add("easetype", iTween.EaseType.easeOutQuad);
			scaleArgs.Add("name", "scale");
			iTween.StopByName(gameObject, "position");
			iTween.StopByName(gameObject, "scale");
			iTween.MoveTo(gameObject, moveArgs);
			iTween.ScaleTo(gameObject, scaleArgs);
		}
	}

	private bool HandleMulliganHotkeys()
	{
		MulliganManager mulliganManager = MulliganManager.Get();
		if (mulliganManager == null)
		{
			return false;
		}
		if (GameMgr.Get().IsBattlegrounds())
		{
			return false;
		}
		if (HearthstoneApplication.IsInternal() && InputCollection.GetKeyUp(KeyCode.Escape) && !GameMgr.Get().IsTraditionalTutorial() && !PlatformSettings.IsMobile())
		{
			mulliganManager.SetAllMulliganCardsToHold();
			if (m_gameState != null)
			{
				m_gameState.SendChoices();
			}
			TurnStartManager.Get().BeginListeningForTurnEvents();
			mulliganManager.SkipMulliganForDev();
			return true;
		}
		return false;
	}

	private bool HandleUniversalHotkeys()
	{
		return false;
	}

	private bool HandleGameHotkeys()
	{
		if (m_gameState != null && m_gameState.IsMulliganManagerActive())
		{
			return false;
		}
		if (HasPendingChoiceTarget())
		{
			return false;
		}
		if (InputCollection.GetKeyUp(KeyCode.Escape))
		{
			return CancelOption();
		}
		return false;
	}

	private void ShowBullseyeIfNeeded()
	{
		if (!(m_targetReticleManager == null) && m_targetReticleManager.IsActive())
		{
			bool showBullseye = false;
			if (m_mousedOverCard != null)
			{
				showBullseye = ((!HasPendingChoiceTarget()) ? m_gameState.IsValidOptionTarget(m_mousedOverCard.GetEntity(), checkInputEnabled: false) : m_gameState.GetFriendlyEntityChoices().Entities.Contains(m_mousedOverCard.GetEntity().GetEntityId()));
			}
			m_targetReticleManager.ShowBullseye(showBullseye);
		}
	}

	private bool EntityIsPoisonousForSkullPreview(Entity entity)
	{
		if (entity.GetRealTimeAttack() <= 0)
		{
			return false;
		}
		if (entity.GetRealTimeIsPoisonous() || entity.GetRealTimeIsVenomous())
		{
			return true;
		}
		if (entity.IsHero())
		{
			Card weaponCard = entity.GetWeaponCard();
			Entity weapon = (weaponCard ? weaponCard.GetEntity() : null);
			if (weapon != null && (weapon.GetRealTimeIsPoisonous() || weapon.GetRealTimeIsVenomous()))
			{
				return true;
			}
		}
		return false;
	}

	private void ShowSkullIfNeeded()
	{
		if (GetBattlecrySourceCard() != null)
		{
			return;
		}
		Network.Options.Option.SubOption subOption = m_gameState.GetSelectedNetworkSubOption();
		if (subOption == null)
		{
			return;
		}
		Entity parentEntity = m_gameState.GetEntity(subOption.ID);
		if (parentEntity != null && (parentEntity.IsMinion() || parentEntity.IsHero()))
		{
			Entity mouseOverEntity = m_mousedOverCard.GetEntity();
			if ((mouseOverEntity.IsMinion() || mouseOverEntity.IsHero()) && m_gameState.IsValidOptionTarget(mouseOverEntity, checkInputEnabled: false))
			{
				ShowSkull(parentEntity, mouseOverEntity, m_mousedOverCard);
				ShowSkull(mouseOverEntity, parentEntity, parentEntity.GetCard());
			}
		}
	}

	private void ShowSkull(Entity entity1, Entity entity2, Card card)
	{
		int realTimeAttack = entity1.GetRealTimeAttack();
		if (entity2.HasTag(GAME_TAG.HEAVILY_ARMORED))
		{
			realTimeAttack = Mathf.Min(realTimeAttack, 1);
		}
		if (!entity2.CanBeDamagedRealTime() || (realTimeAttack < entity2.GetRealTimeRemainingHP() && (!EntityIsPoisonousForSkullPreview(entity1) || !entity2.IsMinion())))
		{
			return;
		}
		if (EntityIsPoisonousForSkullPreview(entity1))
		{
			DamageSplatSpell spell = card.ActivateActorSpell(SpellType.DAMAGE) as DamageSplatSpell;
			if (spell != null)
			{
				spell.SetPoisonous(isPoisonous: true);
				spell.ActivateState(SpellStateType.IDLE);
				spell.transform.localScale = Vector3.zero;
				iTween.ScaleTo(spell.gameObject, iTween.Hash("scale", Vector3.one, "time", 0.5f, "easetype", iTween.EaseType.easeOutElastic));
			}
		}
		else
		{
			Spell spell2 = card.ActivateActorSpell(SpellType.SKULL);
			if (spell2 != null)
			{
				spell2.transform.localScale = Vector3.zero;
				iTween.ScaleTo(spell2.gameObject, iTween.Hash("scale", Vector3.one, "time", 0.5f, "easetype", iTween.EaseType.easeOutElastic));
			}
		}
	}

	private void DisableSkullIfNeeded(Card mousedOverCard)
	{
		DisableSkull(mousedOverCard);
		if (m_gameState == null)
		{
			return;
		}
		Network.Options.Option.SubOption subOption = m_gameState.GetSelectedNetworkSubOption();
		if (subOption == null)
		{
			return;
		}
		Entity parentEntity = m_gameState.GetEntity(subOption.ID);
		if (parentEntity != null)
		{
			Card parentCard = parentEntity.GetCard();
			if (!(parentCard == null))
			{
				DisableSkull(parentCard);
			}
		}
	}

	private void DisableSkull(Card card)
	{
		Spell skullSpell = card.GetActorSpell(SpellType.SKULL);
		if (skullSpell != null)
		{
			iTween.Stop(skullSpell.gameObject);
			skullSpell.transform.localScale = Vector3.zero;
			skullSpell.Deactivate();
		}
		Spell damageSpell = card.GetActorSpell(SpellType.DAMAGE);
		if (damageSpell != null && card.GetEntity().IsMinion())
		{
			iTween.Stop(damageSpell.gameObject);
			damageSpell.transform.localScale = Vector3.zero;
			damageSpell.Deactivate();
		}
	}

	private void HandleMouseOverCard(Card card)
	{
		if (!card.IsInputEnabled() || m_gameState.GetGameEntity().ShouldSuppressCardMouseOver(card.GetEntity()))
		{
			return;
		}
		m_mousedOverCard = card;
		bool targetRecticlePreventsCardTooltip = m_gameState.IsFriendlySidePlayerTurn() && (bool)m_targetReticleManager && m_targetReticleManager.ShouldPreventMouseOverBigCard();
		if (!PermitDecisionMakingInput())
		{
			targetRecticlePreventsCardTooltip = false;
		}
		if (m_gameState.IsMainPhase() && m_heldCard == null && !ChoiceCardMgr.Get().HasSubOption() && !targetRecticlePreventsCardTooltip && (!m_universalInputManager.IsTouchMode() || card.gameObject == m_lastObjectMousedDown))
		{
			SetShouldShowTooltip();
		}
		if (!m_gameState.IsMulliganManagerActive() || !card.GetEntity().IsHeroPower())
		{
			card.NotifyMousedOver();
		}
		if (m_gameState.IsMulliganManagerActive())
		{
			if (card.GetEntity().IsControlledByFriendlySidePlayer() && card.GetZone() is ZoneHand && !GameMgr.Get().IsBattlegroundsMatchOrTutorial())
			{
				TooltipPanelManager.Get().UpdateKeywordHelpForMulliganCard(card.GetEntity(), card.GetActor());
			}
		}
		else
		{
			Zone currentCardZone = m_mousedOverCard.GetZone();
			if (!(currentCardZone is ZonePlay) && !(currentCardZone is ZoneHeroPower) && !(currentCardZone is ZoneWeapon) && !(currentCardZone is ZoneHero) && !(currentCardZone is ZoneBattlegroundAnomaly) && !(currentCardZone is ZoneBattlegroundHeroBuddy) && !(currentCardZone is ZoneBattlegroundTrinket) && !(currentCardZone is ZoneLettuceAbility))
			{
				if (m_gameState.GetResponseMode() == GameState.ResponseMode.CHOICE)
				{
					TooltipPanelManager.TooltipBoneSource boneSource = (ShowTooltipOnRight() ? TooltipPanelManager.TooltipBoneSource.TOP_RIGHT : TooltipPanelManager.TooltipBoneSource.TOP_LEFT);
					TooltipPanelManager.Get().UpdateKeywordHelpForDiscover(m_mousedOverCard.GetEntity(), m_mousedOverCard.GetActor(), boneSource);
				}
				else if (m_gameState.GetResponseMode() == GameState.ResponseMode.SUB_OPTION)
				{
					TooltipPanelManager.TooltipBoneSource boneSource2 = (ShowTooltipOnRight() ? TooltipPanelManager.TooltipBoneSource.TOP_RIGHT : TooltipPanelManager.TooltipBoneSource.TOP_LEFT);
					TooltipPanelManager.Get().UpdateKeywordHelpForSubOptions(m_mousedOverCard.GetEntity(), m_mousedOverCard.GetActor(), boneSource2);
				}
				else if (m_isShowingStarshipUI)
				{
					TooltipPanelManager.TooltipBoneSource boneSource3 = TooltipPanelManager.TooltipBoneSource.TOP_RIGHT;
					TooltipPanelManager.Get().UpdateKeywordHelpForSubOptions(m_mousedOverCard.GetEntity(), m_mousedOverCard.GetActor(), boneSource3);
				}
			}
		}
		ShowBullseyeIfNeeded();
		ShowSkullIfNeeded();
	}

	private bool ShowTooltipOnRight()
	{
		bool showOnRight = ChoiceCardMgr.Get() != null && ChoiceCardMgr.Get().CardIsFirstChoice(m_mousedOverCard);
		if (m_mousedOverCard != null && m_mousedOverCard.m_evolutionCardActor != null && m_mousedOverCard.GetActor() != null)
		{
			showOnRight = m_mousedOverCard.GetActor().IsOnRightSideOfZonePlay();
		}
		return showOnRight;
	}

	public void NotifyCardDestroyed(Card destroyedCard)
	{
		if (destroyedCard == m_mousedOverCard)
		{
			HandleMouseOffCard();
		}
	}

	private void HandleMouseOffCard()
	{
		if (!(m_mousedOverCard == null))
		{
			PegCursor.Get().SetMode(PegCursor.Mode.UP);
			Card mousedOverCard = m_mousedOverCard;
			m_mousedOverCard = null;
			mousedOverCard.HideTooltip();
			mousedOverCard.NotifyMousedOut();
			ShowBullseyeIfNeeded();
			DisableSkullIfNeeded(mousedOverCard);
		}
	}

	public void HandleMemberClick(GameObject hitObject)
	{
		if (!(m_mousedOverObject == null))
		{
			return;
		}
		if (m_universalInputManager.GetInputHitInfo(Camera.main, GameLayer.PlayAreaCollision, out var hitInfo))
		{
			if (!(hitObject == null))
			{
				return;
			}
			int hitNumber = UniversalInputManager.Get().GetAllInputHitInfo(-1, ref m_cachedDustBlockers);
			if (hitNumber > 0)
			{
				Array.Sort(m_cachedDustBlockers, 0, hitNumber, m_hitPointComparer);
				for (int i = 0; i < hitNumber; i++)
				{
					GameObject currDustBlocker = m_cachedDustBlockers[i].collider.gameObject;
					if (currDustBlocker.layer == 10)
					{
						break;
					}
					if (currDustBlocker.layer == 8 || currDustBlocker.GetComponent<BoardClickableDustBlocker>() != null)
					{
						return;
					}
				}
			}
			Board.Get().BoardClicked(hitInfo);
		}
		else if (Gameplay.Get() != null && PegUI.Get().FindHitElement() == null)
		{
			SoundManager.Get().LoadAndPlay("UI_MouseClick_01.prefab:fa537702a0db1c3478c989967458788b");
		}
	}

	private void ShowTooltipIfNecessary()
	{
		if (m_mousedOverCard == null || !m_mousedOverCard.GetShouldShowTooltip())
		{
			return;
		}
		if (m_gameState.GetGameEntity().SuppressMousedOverCardTooltip(out var resetTimer))
		{
			if (resetTimer)
			{
				m_mousedOverTimer = 0f;
			}
			return;
		}
		m_mousedOverTimer += Time.unscaledDeltaTime;
		if (m_mousedOverCard.IsActorReady() && !m_isShowingStarshipUI)
		{
			if (m_gameState.GetBooleanGameOption(GameEntityOption.MOUSEOVER_DELAY_OVERRIDDEN))
			{
				m_mousedOverCard.ShowTooltip();
			}
			else if (m_mousedOverCard.GetZone() is ZoneHand)
			{
				m_mousedOverCard.ShowTooltip();
			}
			else if (m_mousedOverTimer >= GetMouseOverDelay(m_mousedOverCard.GetEntity()))
			{
				m_mousedOverCard.ShowTooltip();
			}
		}
	}

	private void ShowTooltipZone(GameObject hitObject, TooltipZone tooltip)
	{
		if (ShowTooltipBattlegroundsQuestHeroSelectBanner(tooltip) || ShowTooltipBattlegroundsBuddyHeroSelectBanner(tooltip) || ShowTooltipBattlegroundTrinketSelectBanner(tooltip) || m_gameState.IsMulliganManagerActive())
		{
			return;
		}
		GameEntity gameEntity = m_gameState.GetGameEntity();
		if (gameEntity != null && !gameEntity.GetGameOptions().GetBooleanOption(GameEntityOption.DISABLE_TOOLTIPS) && !gameEntity.NotifyOfTooltipDisplay(tooltip))
		{
			ZoneTooltipSettings zoneTooltipSettings = gameEntity.GetZoneTooltipSettings();
			if (!ShowTooltipManaCrystalManager(tooltip, zoneTooltipSettings) && !ShowTooltipDeckZone(tooltip, zoneTooltipSettings) && !ShowTooltipOpposingHandZone(tooltip, zoneTooltipSettings) && !ShowTooltipCorpseCounter(tooltip, zoneTooltipSettings))
			{
				ShowTooltipManaCounterZone(tooltip, zoneTooltipSettings);
			}
		}
	}

	private bool ShowTooltipManaCrystalManager(TooltipZone tooltip, ZoneTooltipSettings zoneTooltipSettings)
	{
		ManaCrystalMgr manaCrysalMgr = tooltip.targetObject.GetComponent<ManaCrystalMgr>();
		if (manaCrysalMgr == null)
		{
			return false;
		}
		if (!zoneTooltipSettings.FriendlyMana.Allowed)
		{
			return false;
		}
		string headline = null;
		string description = null;
		if (zoneTooltipSettings.FriendlyMana.GetTooltipOverrideContent(ref headline, ref description))
		{
			ShowTooltipInZone(tooltip, headline, description);
		}
		if (manaCrysalMgr.ShouldShowTooltip(ManaCrystalType.DEFAULT))
		{
			Player friendlySidePlayer = m_gameState.GetFriendlySidePlayer();
			int overloadLockedCrystals = friendlySidePlayer.GetTag(GAME_TAG.OVERLOAD_LOCKED);
			int overloadOwedCrystals = friendlySidePlayer.GetTag(GAME_TAG.OVERLOAD_OWED);
			if (overloadLockedCrystals > 0)
			{
				headline = GameStrings.Format("GAMEPLAY_TOOLTIP_MANA_LOCKED_HEADLINE");
				description = GameStrings.Format("GAMEPLAY_TOOLTIP_MANA_LOCKED_DESCRIPTION", overloadLockedCrystals);
			}
			else if (overloadOwedCrystals > 0)
			{
				headline = GameStrings.Format("GAMEPLAY_TOOLTIP_MANA_OVERLOAD_HEADLINE");
				description = GameStrings.Format("GAMEPLAY_TOOLTIP_MANA_OVERLOAD_DESCRIPTION", overloadOwedCrystals);
			}
			else
			{
				headline = GameStrings.Get("GAMEPLAY_TOOLTIP_MANA_HEADLINE");
				description = GameStrings.Get("GAMEPLAY_TOOLTIP_MANA_DESCRIPTION");
			}
			ShowTooltipInZone(tooltip, headline, description, new Vector3(0f, 0f, 1.17f));
		}
		else if (manaCrysalMgr.ShouldShowTooltip(ManaCrystalType.COIN))
		{
			ShowTooltipInZone(tooltip, GameStrings.Get("GAMEPLAY_TOOLTIP_MANA_COIN_HEADLINE"), GameStrings.Get("GAMEPLAY_TOOLTIP_MANA_COIN_DESCRIPTION"));
		}
		return true;
	}

	private bool ShowTooltipDeckZone(TooltipZone tooltip, ZoneTooltipSettings zoneTooltipSettings)
	{
		ZoneDeck deck = tooltip.targetObject.GetComponent<ZoneDeck>();
		if (deck != null)
		{
			if (deck.m_Side == Player.Side.FRIENDLY)
			{
				ShowTooltipFriendlyDeckZone(tooltip, zoneTooltipSettings, deck);
			}
			else if (deck.m_Side == Player.Side.OPPOSING)
			{
				ShowTooltipOpposingDeckZone(tooltip, zoneTooltipSettings, deck);
			}
			return true;
		}
		return false;
	}

	private void ShowFriendlyHandSizeTooltipWhenHoveringDeck(ZoneTooltipSettings zoneTooltipSettings)
	{
		Player friendlyPlayer = m_gameState.GetFriendlySidePlayer();
		ZoneDeck deck = friendlyPlayer.GetDeckZone();
		if (deck.m_playerHandTooltipZone == null || !zoneTooltipSettings.FriendlyHand.Allowed)
		{
			return;
		}
		int handCount = friendlyPlayer.GetHandZone().GetCards().Count;
		string tooltipDesc = "GAMEPLAY_TOOLTIP_HAND_DESCRIPTION";
		string tooptipDescFulHand = "GAMEPLAY_TOOLTIP_HAND_FULL_DESCRIPTION";
		if (TeammateBoardViewer.Get() != null && TeammateBoardViewer.Get().IsViewingTeammate())
		{
			handCount = TeammateBoardViewer.Get().GetCardsInHand();
			tooltipDesc = "GAMEPLAY_TOOLTIP_TEAMMATE_HAND_DESCRIPTION";
			tooptipDescFulHand = "GAMEPLAY_TOOLTIP_TEAMMATE_HAND_FULL_DESCRIPTION";
		}
		if (handCount < 5 || GameMgr.Get().IsTraditionalTutorial())
		{
			return;
		}
		string headline = null;
		string description = null;
		if (!zoneTooltipSettings.FriendlyHand.GetTooltipOverrideContent(ref headline, ref description))
		{
			headline = GameStrings.Get("GAMEPLAY_TOOLTIP_HAND_HEADLINE");
			description = GameStrings.Format(tooltipDesc, handCount);
			if (handCount >= friendlyPlayer.GetTag(GAME_TAG.MAXHANDSIZE))
			{
				headline = GameStrings.Get("GAMEPLAY_TOOLTIP_HAND_FULL_HEADLINE");
				description = GameStrings.Format(tooptipDescFulHand, handCount);
			}
		}
		ShowTooltipInZone(deck.m_playerHandTooltipZone, headline, description);
	}

	public bool IsDemonPortalActive(bool friendly)
	{
		if (GameState.Get() == null)
		{
			return false;
		}
		Player player = (friendly ? GameState.Get().GetFriendlySidePlayer() : GameState.Get().GetOpposingSidePlayer());
		if (player == null)
		{
			return false;
		}
		return player.GetHero()?.HasTag(GAME_TAG.DEMON_PORTAL_DECK) ?? false;
	}

	private string GetDemonPortalDescription(bool friendly, string defaultString)
	{
		if (GameState.Get() == null)
		{
			return defaultString;
		}
		Player player = (friendly ? GameState.Get().GetFriendlySidePlayer() : GameState.Get().GetOpposingSidePlayer());
		if (player == null)
		{
			return defaultString;
		}
		List<Entity> enchantments = player.GetEnchantments();
		int buffAmount = 0;
		foreach (Entity enchantment in enchantments)
		{
			if (enchantment.GetCardId() == "GDB_145e")
			{
				buffAmount = enchantment.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_2);
				break;
			}
		}
		if (friendly)
		{
			if (buffAmount == 0)
			{
				return GameStrings.Format("GAMEPLAY_TOOLTIP_DECK_DESCRIPTION_DEMON_PORTAL_FRIENDLY");
			}
			return GameStrings.Format("GAMEPLAY_TOOLTIP_DECK_DESCRIPTION_DEMON_PORTAL_FRIENDLY_BUFFED", buffAmount, buffAmount);
		}
		if (buffAmount == 0)
		{
			return GameStrings.Format("GAMEPLAY_TOOLTIP_DECK_DESCRIPTION_DEMON_PORTAL_OPPOSING");
		}
		return GameStrings.Format("GAMEPLAY_TOOLTIP_DECK_DESCRIPTION_DEMON_PORTAL_OPPOSING_BUFFED", buffAmount, buffAmount);
	}

	private void ShowTooltipFriendlyDeckZone(TooltipZone tooltip, ZoneTooltipSettings zoneTooltipSettings, ZoneDeck deck)
	{
		if (zoneTooltipSettings.FriendlyDeck.IsCustomHandler)
		{
			tooltip.RegisterOnTooltipHiddenCallback(delegate
			{
				zoneTooltipSettings.FriendlyDeck.FireOnTooltipHidden();
			});
			zoneTooltipSettings.FriendlyDeck.FireOnTooltipShown(delegate(string headline, string description)
			{
				ShowTooltipInZone(tooltip, headline, description);
			});
			return;
		}
		if (zoneTooltipSettings.FriendlyDeck.Allowed)
		{
			Vector3 localOffset = Vector3.zero;
			string headline2 = null;
			string description2 = null;
			if (!zoneTooltipSettings.FriendlyDeck.GetTooltipOverrideContent(ref headline2, ref description2))
			{
				if (deck.IsFatigued())
				{
					if ((bool)UniversalInputManager.UsePhoneUI)
					{
						localOffset = new Vector3(0f, 0f, 0.562f);
					}
					headline2 = GameStrings.Get("GAMEPLAY_TOOLTIP_FATIGUE_DECK_HEADLINE");
					description2 = GameStrings.Get("GAMEPLAY_TOOLTIP_FATIGUE_DECK_DESCRIPTION");
				}
				else
				{
					headline2 = GameStrings.Format("GAMEPLAY_TOOLTIP_DECK_HEADLINE");
					description2 = GameStrings.Format("GAMEPLAY_TOOLTIP_DECK_DESCRIPTION", deck.GetCards().Count);
					if (IsDemonPortalActive(friendly: true))
					{
						headline2 = GameStrings.Format("GAMEPLAY_TOOLTIP_DECK_HEADLINE_DEMON_PORTAL");
						description2 = GetDemonPortalDescription(friendly: true, description2);
					}
				}
			}
			ShowTooltipInZone(tooltip, headline2, description2, localOffset);
			for (int i = 1; i < zoneTooltipSettings.FriendlyDeck.GetTooltipCount(); i++)
			{
				if (zoneTooltipSettings.FriendlyDeck.GetTooltipOverrideContent(ref headline2, ref description2, i))
				{
					AddTooltipInZone(tooltip, headline2, description2);
				}
			}
		}
		ShowFriendlyHandSizeTooltipWhenHoveringDeck(zoneTooltipSettings);
	}

	private void ShowTooltipOpposingDeckZone(TooltipZone tooltip, ZoneTooltipSettings zoneTooltipSettings, ZoneDeck deck)
	{
		if (zoneTooltipSettings.EnemyDeck.IsCustomHandler)
		{
			tooltip.RegisterOnTooltipHiddenCallback(delegate
			{
				zoneTooltipSettings.EnemyDeck.FireOnTooltipHidden();
			});
			zoneTooltipSettings.EnemyDeck.FireOnTooltipShown(delegate(string headline, string description)
			{
				ShowTooltipInZone(tooltip, headline, description);
			});
			return;
		}
		if (zoneTooltipSettings.EnemyDeck.Allowed)
		{
			string headline2 = null;
			string description2 = null;
			if (!zoneTooltipSettings.EnemyDeck.GetTooltipOverrideContent(ref headline2, ref description2))
			{
				if (deck.IsFatigued())
				{
					headline2 = GameStrings.Get("GAMEPLAY_TOOLTIP_FATIGUE_ENEMYDECK_HEADLINE");
					description2 = GameStrings.Get("GAMEPLAY_TOOLTIP_FATIGUE_ENEMYDECK_DESCRIPTION");
				}
				else
				{
					headline2 = GameStrings.Get("GAMEPLAY_TOOLTIP_ENEMYDECK_HEADLINE");
					description2 = GameStrings.Format("GAMEPLAY_TOOLTIP_ENEMYDECK_DESC", deck.GetCards().Count);
					if (IsDemonPortalActive(friendly: false))
					{
						headline2 = GameStrings.Format("GAMEPLAY_TOOLTIP_DECK_HEADLINE_DEMON_PORTAL");
						description2 = GetDemonPortalDescription(friendly: false, description2);
					}
				}
			}
			ShowTooltipInZone(tooltip, headline2, description2);
			for (int i = 1; i < zoneTooltipSettings.EnemyDeck.GetTooltipCount(); i++)
			{
				if (zoneTooltipSettings.EnemyDeck.GetTooltipOverrideContent(ref headline2, ref description2, i))
				{
					AddTooltipInZone(tooltip, headline2, description2);
				}
			}
		}
		if (zoneTooltipSettings.ShowFriendlyHandWhenHoveringOpponentDeck)
		{
			ShowFriendlyHandSizeTooltipWhenHoveringDeck(zoneTooltipSettings);
		}
		if (deck.m_playerHandTooltipZone != null && zoneTooltipSettings.EnemyHand.Allowed)
		{
			int handCount = m_gameState.GetOpposingSidePlayer().GetHandZone().GetCards()
				.Count;
			if (handCount >= 5 && !GameMgr.Get().IsTraditionalTutorial())
			{
				string headline3 = null;
				string description3 = null;
				if (!zoneTooltipSettings.EnemyHand.GetTooltipOverrideContent(ref headline3, ref description3))
				{
					headline3 = GameStrings.Get("GAMEPLAY_TOOLTIP_ENEMYHAND_HEADLINE");
					description3 = GameStrings.Format("GAMEPLAY_TOOLTIP_ENEMYHAND_DESC", handCount);
				}
				ShowTooltipInZone(deck.m_playerHandTooltipZone, headline3, description3);
			}
		}
		int overloadOwedCrystals = m_gameState.GetOpposingSidePlayer().GetTag(GAME_TAG.OVERLOAD_OWED);
		if (zoneTooltipSettings.EnemyMana.Allowed && overloadOwedCrystals > 0)
		{
			if ((bool)UniversalInputManager.UsePhoneUI && deck.m_playerHandTooltipZone != null)
			{
				string overloadHeadline = GameStrings.Get("GAMEPLAY_TOOLTIP_ENEMYOVERLOAD_HEADLINE");
				string overloadDescription = GameStrings.Format("GAMEPLAY_TOOLTIP_ENEMYOVERLOAD_DESC", overloadOwedCrystals);
				AddTooltipInZone(deck.m_playerHandTooltipZone, overloadHeadline, overloadDescription);
			}
			else if (!UniversalInputManager.UsePhoneUI && deck.m_playerManaTooltipZone != null)
			{
				string overloadHeadline2 = GameStrings.Get("GAMEPLAY_TOOLTIP_ENEMYOVERLOAD_HEADLINE");
				string overloadDescription2 = GameStrings.Format("GAMEPLAY_TOOLTIP_ENEMYOVERLOAD_DESC", overloadOwedCrystals);
				ShowTooltipInZone(deck.m_playerManaTooltipZone, overloadHeadline2, overloadDescription2);
			}
		}
	}

	private bool ShowTooltipOpposingHandZone(TooltipZone tooltip, ZoneTooltipSettings zoneTooltipSettings)
	{
		ZoneHand hand = tooltip.targetObject.GetComponent<ZoneHand>();
		if (hand == null)
		{
			return false;
		}
		if (hand.m_Side != Player.Side.OPPOSING)
		{
			return false;
		}
		if (GameMgr.Get().IsTraditionalTutorial())
		{
			ShowTooltipInZone(tooltip, GameStrings.Get("GAMEPLAY_TOOLTIP_ENEMYHAND_HEADLINE"), GameStrings.Get("GAMEPLAY_TOOLTIP_ENEMYHAND_DESC_TUT"));
		}
		else if (zoneTooltipSettings.EnemyHand.Allowed)
		{
			string headline = null;
			string description = null;
			if (!zoneTooltipSettings.EnemyHand.GetTooltipOverrideContent(ref headline, ref description))
			{
				int numCardsInHand = hand.GetCardCount();
				if (numCardsInHand == 1)
				{
					headline = GameStrings.Get("GAMEPLAY_TOOLTIP_ENEMYHAND_HEADLINE");
					description = GameStrings.Format("GAMEPLAY_TOOLTIP_ENEMYHAND_DESC_SINGLE", numCardsInHand);
				}
				else
				{
					headline = GameStrings.Get("GAMEPLAY_TOOLTIP_ENEMYHAND_HEADLINE");
					description = GameStrings.Format("GAMEPLAY_TOOLTIP_ENEMYHAND_DESC", numCardsInHand);
				}
			}
			ShowTooltipInZone(tooltip, headline, description);
			if ((bool)UniversalInputManager.UsePhoneUI && zoneTooltipSettings.EnemyMana.Allowed)
			{
				int overloadOwedCrystals = m_gameState.GetOpposingSidePlayer().GetTag(GAME_TAG.OVERLOAD_OWED);
				if (overloadOwedCrystals > 0)
				{
					string overloadHeadline = GameStrings.Get("GAMEPLAY_TOOLTIP_ENEMYOVERLOAD_HEADLINE");
					string overloadDescription = GameStrings.Format("GAMEPLAY_TOOLTIP_ENEMYOVERLOAD_DESC", overloadOwedCrystals);
					AddTooltipInZone(tooltip, overloadHeadline, overloadDescription);
				}
			}
		}
		return true;
	}

	private bool ShowTooltipCorpseCounter(TooltipZone tooltip, ZoneTooltipSettings zoneTooltipSettings)
	{
		CorpseCounter corpseCounter = ((tooltip.targetObject != null) ? tooltip.targetObject.GetComponent<CorpseCounter>() : null);
		if (corpseCounter != null && corpseCounter.IsShown())
		{
			int corpseCount = ((corpseCounter.m_side == Player.Side.FRIENDLY) ? m_gameState.GetFriendlySidePlayer() : m_gameState.GetOpposingSidePlayer()).GetNumAvailableCorpses();
			string graveyardHeadline = GameStrings.Get("GAMEPLAY_TOOLTIP_CORPSES");
			string graveyardDescription = GameStrings.Format("GAMEPLAY_TOOLTIP_CORPSES_DESCRIPTION", corpseCount);
			ShowTooltipInZone(tooltip, graveyardHeadline, graveyardDescription);
			return true;
		}
		return false;
	}

	private bool ShowTooltipManaCounterZone(TooltipZone tooltip, ZoneTooltipSettings zoneTooltipSettings)
	{
		ManaCounter manaCounter = tooltip.targetObject.GetComponent<ManaCounter>();
		if (manaCounter != null && manaCounter.m_Side == Player.Side.OPPOSING && zoneTooltipSettings.EnemyMana.Allowed)
		{
			int overloadOwedCrystals = m_gameState.GetOpposingSidePlayer().GetTag(GAME_TAG.OVERLOAD_OWED);
			if (overloadOwedCrystals > 0)
			{
				string overloadHeadline = GameStrings.Get("GAMEPLAY_TOOLTIP_ENEMYOVERLOAD_HEADLINE");
				string overloadDescription = GameStrings.Format("GAMEPLAY_TOOLTIP_ENEMYOVERLOAD_DESC", overloadOwedCrystals);
				ShowTooltipInZone(tooltip, overloadHeadline, overloadDescription);
				return true;
			}
		}
		return false;
	}

	private bool ShowTooltipBattlegroundsQuestHeroSelectBanner(TooltipZone tooltip)
	{
		if (((tooltip.targetObject != null) ? tooltip.targetObject.GetComponent<BaconQuestsMedallion>() : null) != null)
		{
			string questHeadLine = GameStrings.Get("GAMEPLAY_BACON_TOOLTIP_QUEST_HEADER");
			string questDescription = GameStrings.Get("GAMEPLAY_BACON_TOOLTIP_QUEST_BODY");
			ShowTooltipInZone(tooltip, questHeadLine, questDescription);
			return true;
		}
		return false;
	}

	private bool ShowTooltipBattlegroundsBuddyHeroSelectBanner(TooltipZone tooltip)
	{
		if (((tooltip.targetObject != null) ? tooltip.targetObject.GetComponent<BaconBuddiesMedallion>() : null) != null)
		{
			string buddyHeadline = GameStrings.Get("GAMEPLAY_BACON_TOOLTIP_BUDDIES_HEADER");
			string buddyDescription = GameStrings.Get("GAMEPLAY_BACON_TOOLTIP_BUDDIES_BODY");
			ShowTooltipInZone(tooltip, buddyHeadline, buddyDescription);
			return true;
		}
		return false;
	}

	private bool ShowTooltipBattlegroundTrinketSelectBanner(TooltipZone tooltip)
	{
		if (((tooltip.targetObject != null) ? tooltip.targetObject.GetComponent<BaconTrinketsMedallion>() : null) != null)
		{
			string trinketHeadline = GameStrings.Get("GAMEPLAY_BACON_TOOLTIP_TRINKETS_HEADER");
			string trinketDescription = GameStrings.Get("GAMEPLAY_BACON_TOOLTIP_TRINKETS_BODY");
			ShowTooltipInZone(tooltip, trinketHeadline, trinketDescription);
			return true;
		}
		return false;
	}

	private void AddTooltipInZone(TooltipZone tooltip, string headline, string description)
	{
		for (int index = 0; index < 10; index++)
		{
			if (!tooltip.IsShowingTooltip(index))
			{
				ShowTooltipInZone(tooltip, headline, description, Vector3.zero, index);
				return;
			}
		}
		Debug.LogError("You are trying to add too many tooltips. TooltipZone = [" + tooltip.gameObject.name + "] MAX_TOOLTIPS = [" + 10 + "]");
	}

	private void ShowTooltipInZone(TooltipZone tooltip, string headline, string description, int index = 0)
	{
		ShowTooltipInZone(tooltip, headline, description, Vector3.zero, index);
	}

	private void ShowTooltipInZone(TooltipZone tooltip, string headline, string description, Vector3 localOffset, int index = 0)
	{
		m_gameState.GetGameEntity().NotifyOfTooltipZoneMouseOver(tooltip);
		if (m_universalInputManager.IsTouchMode())
		{
			tooltip.ShowGameplayTooltipLarge(headline, description, localOffset, index);
		}
		else
		{
			tooltip.ShowGameplayTooltip(headline, description, localOffset, index);
		}
	}

	private void PredictSpentMana(Entity entity)
	{
		Player friendlyPlayer = m_gameState.GetFriendlySidePlayer();
		if ((!friendlyPlayer.GetRealTimeSpellsCostHealth() || entity.GetRealTimeCardType() != TAG_CARDTYPE.SPELL) && !entity.GetRealTimeCardCostsHealth() && !entity.GetRealTimeCardCostsArmor() && !entity.GetRealTimeCardCostsCorpses())
		{
			int realTimeCost = entity.GetRealTimeCost();
			int realTimeTempMana = friendlyPlayer.GetRealTimeTempMana();
			if (realTimeTempMana > 0)
			{
				int tempManaToBlowUp = Mathf.Clamp(realTimeCost, 0, realTimeTempMana);
				friendlyPlayer.NotifyOfUsedTempMana(tempManaToBlowUp);
				ManaCrystalMgr.Get().DestroyTempManaCrystals(tempManaToBlowUp);
			}
			int manaChangeAmount = realTimeCost - realTimeTempMana;
			if (manaChangeAmount > 0 && !entity.HasTag(GAME_TAG.RED_MANA_CRYSTALS))
			{
				friendlyPlayer.NotifyOfSpentMana(manaChangeAmount);
				ManaCrystalMgr.Get().UpdateSpentMana(manaChangeAmount);
			}
			friendlyPlayer.UpdateManaCounter();
			m_entitiesThatPredictedMana.Add(entity);
		}
	}

	private void RollbackSpentMana(Entity entity)
	{
		int index = m_entitiesThatPredictedMana.IndexOf(entity);
		if (index >= 0)
		{
			m_entitiesThatPredictedMana.RemoveAt(index);
			Player friendlyPlayer = m_gameState.GetFriendlySidePlayer();
			int realTimeCost = entity.GetRealTimeCost();
			int realTimeTempMana = friendlyPlayer.GetRealTimeTempMana();
			if (friendlyPlayer.GetRealTimeTempMana() > 0)
			{
				int tempManaToRestore = Mathf.Clamp(realTimeCost, 0, realTimeTempMana);
				friendlyPlayer.NotifyOfUsedTempMana(-tempManaToRestore);
				ManaCrystalMgr.Get().AddTempManaCrystals(tempManaToRestore);
			}
			int manaChangeAmount = -realTimeCost + realTimeTempMana;
			if (manaChangeAmount < 0)
			{
				friendlyPlayer.NotifyOfSpentMana(manaChangeAmount);
				ManaCrystalMgr.Get().UpdateSpentMana(manaChangeAmount);
			}
			friendlyPlayer.UpdateManaCounter();
		}
	}

	public void OnManaCrystalMgrManaSpent()
	{
		if ((bool)m_mousedOverCard)
		{
			m_mousedOverCard.UpdateProposedManaUsage();
		}
	}

	private bool IsInZone(Entity entity, TAG_ZONE zoneTag)
	{
		return IsInZone(entity.GetCard(), zoneTag);
	}

	private bool IsInZone(Card card, TAG_ZONE zoneTag)
	{
		if (card.GetZone() == null)
		{
			return false;
		}
		Entity entity = card.GetEntity();
		if (entity == null)
		{
			return false;
		}
		TAG_ZONE finalZone = GameUtils.GetFinalZoneForEntity(entity);
		if (finalZone == zoneTag)
		{
			return true;
		}
		GameEntity gameEntity = GameState.Get()?.GetGameEntity();
		if (gameEntity != null && gameEntity.Overwrite_IsInZone_ForInputManager(entity, zoneTag, finalZone, out var isInZone))
		{
			return isInZone;
		}
		return false;
	}

	private void SetDragging(bool dragging)
	{
		m_dragging = dragging;
		m_graphicsManager?.SetDraggingFramerate(dragging);
	}

	private bool GetLegendaryPortraitForHeroPowerEntity(Entity entity, out ILegendaryHeroPortrait portrait)
	{
		portrait = null;
		if (entity == null)
		{
			return false;
		}
		if (!entity.IsHeroPower())
		{
			return false;
		}
		Entity heroEntity = entity.GetHero();
		if (heroEntity == null)
		{
			return false;
		}
		Card card = heroEntity.GetCard();
		if (card == null)
		{
			return false;
		}
		Actor actor = card.GetActor();
		if (actor == null)
		{
			return false;
		}
		portrait = actor.LegendaryHeroPortrait;
		if (portrait == null)
		{
			return false;
		}
		return true;
	}

	private void HandleLegendaryHeroPowerBirth(Entity entity)
	{
		if (GetLegendaryPortraitForHeroPowerEntity(entity, out var portrait))
		{
			portrait.RaiseAnimationEvent(LegendaryHeroAnimations.HeroPowerBirth);
		}
	}

	private void HandleLegendaryHeroPowerCancel(Entity entity)
	{
		if (GetLegendaryPortraitForHeroPowerEntity(entity, out var portrait))
		{
			portrait.RaiseAnimationEvent(LegendaryHeroAnimations.HeroPowerCancel);
		}
	}

	public bool RegisterPhoneHandShownListener(PhoneHandShownCallback callback)
	{
		return RegisterPhoneHandShownListener(callback, null);
	}

	public bool RegisterPhoneHandShownListener(PhoneHandShownCallback callback, object userData)
	{
		PhoneHandShownListener listener = new PhoneHandShownListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_phoneHandShownListener.Contains(listener))
		{
			return false;
		}
		m_phoneHandShownListener.Add(listener);
		return true;
	}

	public bool RemovePhoneHandShownListener(PhoneHandShownCallback callback)
	{
		return RemovePhoneHandShownListener(callback, null);
	}

	public bool RemovePhoneHandShownListener(PhoneHandShownCallback callback, object userData)
	{
		PhoneHandShownListener listener = new PhoneHandShownListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_phoneHandShownListener.Remove(listener);
	}

	public bool RegisterPhoneHandHiddenListener(PhoneHandHiddenCallback callback)
	{
		return RegisterPhoneHandHiddenListener(callback, null);
	}

	public bool RegisterPhoneHandHiddenListener(PhoneHandHiddenCallback callback, object userData)
	{
		PhoneHandHiddenListener listener = new PhoneHandHiddenListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_phoneHandHiddenListener.Contains(listener))
		{
			return false;
		}
		m_phoneHandHiddenListener.Add(listener);
		return true;
	}

	public bool RemovePhoneHandHiddenListener(PhoneHandHiddenCallback callback)
	{
		return RemovePhoneHandHiddenListener(callback, null);
	}

	public bool RemovePhoneHandHiddenListener(PhoneHandHiddenCallback callback, object userData)
	{
		PhoneHandHiddenListener listener = new PhoneHandHiddenListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_phoneHandHiddenListener.Remove(listener);
	}

	public bool HasPlayFromMiniHandEnabled()
	{
		return NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().EnablePlayingFromMiniHand;
	}
}
