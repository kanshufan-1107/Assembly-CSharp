using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteActionHandler : MonoBehaviour
{
	private class CardAndID
	{
		private int m_ID;

		private Entity m_entity;

		private Card m_card;

		public bool useTeammatesGamestate;

		public Card card
		{
			get
			{
				return m_card;
			}
			set
			{
				if (value == m_card)
				{
					return;
				}
				if (value == null)
				{
					Clear();
					return;
				}
				m_card = value;
				m_entity = value.GetEntity();
				if (m_entity == null)
				{
					Debug.LogWarning("RemoteActionHandler--card has no entity");
					Clear();
					return;
				}
				if ((bool)TeammateBoardViewer.Get())
				{
					useTeammatesGamestate = TeammateBoardViewer.Get().IsActorTeammates(m_card.GetActor());
				}
				m_ID = m_entity.GetEntityId();
				if (m_ID < 1)
				{
					Debug.LogWarning("RemoteActionHandler--invalid entity ID");
					Clear();
				}
			}
		}

		public int ID
		{
			get
			{
				return m_ID;
			}
			set
			{
				if (value == m_ID)
				{
					return;
				}
				if (value == 0)
				{
					Clear();
					return;
				}
				m_ID = value;
				if (useTeammatesGamestate)
				{
					if ((bool)TeammateBoardViewer.Get())
					{
						m_entity = TeammateBoardViewer.Get().GetTeammateEntity(value);
					}
				}
				else
				{
					m_entity = GameState.Get().GetEntity(value);
				}
				if (m_entity == null)
				{
					Debug.LogWarning("RemoteActionHandler--no entity found for ID");
					Clear();
					return;
				}
				m_card = m_entity.GetCard();
				if (m_card == null)
				{
					Debug.LogWarning("RemoteActionHandler--entity has no card");
					Clear();
				}
			}
		}

		public Entity entity => m_entity;

		private void Clear()
		{
			m_ID = 0;
			m_entity = null;
			m_card = null;
		}
	}

	private class UserUI
	{
		public CardAndID over = new CardAndID();

		public CardAndID held = new CardAndID();

		public CardAndID origin = new CardAndID();

		private bool m_useTeammateGamestate;

		public bool UseTeammatesGamestate
		{
			get
			{
				return m_useTeammateGamestate;
			}
			set
			{
				m_useTeammateGamestate = value;
				over.useTeammatesGamestate = value;
				held.useTeammatesGamestate = value;
				origin.useTeammatesGamestate = value;
			}
		}

		public bool SameAs(UserUI compare)
		{
			if (held.card != compare.held.card)
			{
				return false;
			}
			if (over.card != compare.over.card)
			{
				return false;
			}
			if (origin.card != compare.origin.card)
			{
				return false;
			}
			if (UseTeammatesGamestate != compare.UseTeammatesGamestate)
			{
				return false;
			}
			return true;
		}

		public void CopyFrom(UserUI source)
		{
			UseTeammatesGamestate = source.UseTeammatesGamestate;
			held.useTeammatesGamestate = source.held.useTeammatesGamestate;
			held.ID = source.held.ID;
			over.useTeammatesGamestate = source.over.useTeammatesGamestate;
			over.ID = source.over.ID;
			origin.useTeammatesGamestate = source.origin.useTeammatesGamestate;
			origin.ID = source.origin.ID;
		}

		public bool IsSourceOrTargetNull()
		{
			if (!(over.card == null))
			{
				return origin.card == null;
			}
			return true;
		}

		public void Clear()
		{
			held.card = null;
			over.card = null;
			origin.card = null;
			UseTeammatesGamestate = false;
		}
	}

	public const string TWEEN_NAME = "RemoteActionHandler";

	private const float DRIFT_TIME = 10f;

	private const float LOW_FREQ_SEND_TIME = 0.35f;

	private const float HIGH_FREQ_SEND_TIME = 0.25f;

	private const float ENEMY_TARGET_ARROW_DESTROY_DELAY = 0.25f;

	private static RemoteActionHandler s_instance;

	private int myCurrentEntitySelection;

	private int myLastEntitySelection;

	private int myLastUnsentEntitySelection = -1;

	private UserUI myCurrentUI = new UserUI();

	private UserUI myLastUI = new UserUI();

	private UserUI myLastUnsentUI = new UserUI();

	private UserUI enemyWantedUI = new UserUI();

	private UserUI enemyActualUI = new UserUI();

	private UserUI friendlyWantedUI = new UserUI();

	private UserUI friendlyActualUI = new UserUI();

	private UserUI teammateWantedUI = new UserUI();

	private UserUI teammateActualUI = new UserUI();

	private float m_lastSendTime;

	private IEnumerator m_destroyEnemyTargetArrowCoroutine;

	private void Awake()
	{
		s_instance = this;
		m_lastSendTime = Time.realtimeSinceStartup;
		if (GameState.Get() == null)
		{
			Debug.LogError($"RemoteActionHandler.Awake() - GameState already Shutdown before RemoteActionHandler was loaded.");
		}
		else
		{
			GameState.Get().RegisterTurnChangedListener(OnTurnChanged);
		}
	}

	private void OnDestroy()
	{
		s_instance = null;
		StopAllCoroutines();
	}

	private void Update()
	{
		if (TargetReticleManager.Get() != null)
		{
			TargetReticleManager.Get().UpdateArrowPosition();
		}
		ProcessUserUI();
		ProcessUserSelection();
	}

	public static RemoteActionHandler Get()
	{
		return s_instance;
	}

	public Card GetTeammateHeldCard()
	{
		return teammateActualUI.held.card;
	}

	public Card GetOpponentHeldCard()
	{
		return enemyActualUI.held.card;
	}

	public Card GetFriendlyHoverCard()
	{
		return friendlyActualUI.over.card;
	}

	public Card GetFriendlyHeldCard()
	{
		return friendlyActualUI.held.card;
	}

	public Card GetActualHeldCard(bool enemy, bool teammate)
	{
		return GetActualUserUI(enemy, teammate).held.card;
	}

	private UserUI GetActualUserUI(bool enemy, bool teammates)
	{
		if (teammates)
		{
			return teammateActualUI;
		}
		if (enemy)
		{
			return enemyActualUI;
		}
		return friendlyActualUI;
	}

	private UserUI GetWantedUserUI(bool enemy, bool teammates)
	{
		if (teammates)
		{
			return teammateWantedUI;
		}
		if (enemy)
		{
			return enemyWantedUI;
		}
		return friendlyWantedUI;
	}

	public void NotifyOpponentOfSelection(int entityID)
	{
		myCurrentEntitySelection = entityID;
	}

	public void NotifyOpponentOfMouseOverEntity(Card card)
	{
		myCurrentUI.over.card = card;
	}

	public void NotifyOpponentOfMouseOut()
	{
		myCurrentUI.over.card = null;
	}

	public void NotifyOpponentOfTargetModeBegin(Card card)
	{
		myCurrentUI.origin.card = card;
	}

	public void NotifyOpponentOfTargetEnd()
	{
		myCurrentUI.origin.card = null;
	}

	public void NotifyOpponentOfCardPickedUp(Card card)
	{
		myCurrentUI.held.card = card;
	}

	public void NotifyOpponentOfCardDropped()
	{
		myCurrentUI.held.card = null;
	}

	public void NotifySwitchingToTeammateView()
	{
		teammateWantedUI = teammateActualUI;
		teammateActualUI = new UserUI();
		UpdateCardHeld();
		MaybeDestroyArrow();
		MaybeCreateArrow();
		UpdateTargetArrow();
	}

	public void HandleAction(Network.UserUI newData)
	{
		bool isFriendlySide = false;
		if (newData.playerId.HasValue)
		{
			Player friendlyPlayer = GameState.Get().GetFriendlySidePlayer();
			if (friendlyPlayer != null)
			{
				isFriendlySide = friendlyPlayer.GetPlayerId() == newData.playerId.Value;
				if (!isFriendlySide)
				{
					Player targetPlayer = GameState.Get().GetPlayer(newData.playerId.Value);
					isFriendlySide = targetPlayer != null && friendlyPlayer.GetTeamId() == targetPlayer.GetTeamId();
				}
			}
		}
		if (newData.mouseInfo != null)
		{
			if (newData.fromTeammate)
			{
				teammateWantedUI.UseTeammatesGamestate = newData.mouseInfo.UseTeammatesGamestate;
				teammateWantedUI.held.ID = newData.mouseInfo.HeldCardID;
				teammateWantedUI.over.ID = newData.mouseInfo.OverCardID;
				teammateWantedUI.origin.ID = newData.mouseInfo.ArrowOriginID;
			}
			else if (isFriendlySide)
			{
				friendlyWantedUI.held.ID = newData.mouseInfo.HeldCardID;
				friendlyWantedUI.over.ID = newData.mouseInfo.OverCardID;
				friendlyWantedUI.origin.ID = newData.mouseInfo.ArrowOriginID;
			}
			else
			{
				enemyWantedUI.held.ID = newData.mouseInfo.HeldCardID;
				enemyWantedUI.over.ID = newData.mouseInfo.OverCardID;
				enemyWantedUI.origin.ID = newData.mouseInfo.ArrowOriginID;
			}
			UpdateCardOver();
			UpdateCardHeld();
			MaybeDestroyArrow();
			MaybeCreateArrow();
			UpdateTargetArrow();
		}
		else if (newData.emoteInfo != null)
		{
			EmoteType emoteType = (EmoteType)newData.emoteInfo.Emote;
			if (isFriendlySide)
			{
				if (GameState.Get().GetBooleanGameOption(GameEntityOption.HAS_ALTERNATE_ENEMY_EMOTE_ACTOR))
				{
					GameState.Get().GetGameEntity().PlayAlternateEnemyEmote(newData.playerId.Value, emoteType, newData.emoteInfo.BattlegroundsEmoteId);
				}
				else
				{
					GameState.Get().GetFriendlySidePlayer().GetHeroCard()
						.PlayEmote(emoteType);
				}
			}
			else if (CanReceiveEnemyEmote(emoteType, newData.playerId.Value))
			{
				if (GameState.Get().GetBooleanGameOption(GameEntityOption.HAS_ALTERNATE_ENEMY_EMOTE_ACTOR))
				{
					GameState.Get().GetGameEntity().PlayAlternateEnemyEmote(newData.playerId.Value, emoteType, newData.emoteInfo.BattlegroundsEmoteId);
				}
				else
				{
					GameState.Get().GetOpposingSidePlayer().GetHeroCard()
						.PlayEmote(emoteType);
				}
			}
		}
		else if (newData.selectionInfo != null && isFriendlySide && GameMgr.Get().IsSpectator())
		{
			Entity entity = GameState.Get().GetEntity(newData.selectionInfo.SelectedEntityID);
			if (entity == null)
			{
				ZoneMgr.Get().DismissMercenariesAbilityTray();
			}
			else
			{
				ZoneMgr.Get().DisplayLettuceAbilitiesForEntity(entity);
			}
		}
	}

	private void ProcessUserUI()
	{
		if (myCurrentUI.SameAs(myLastUI))
		{
			return;
		}
		if (!CanSendUI())
		{
			if (!myCurrentUI.IsSourceOrTargetNull())
			{
				myLastUnsentUI.CopyFrom(myCurrentUI);
			}
			return;
		}
		UserUI sendUI = ((myCurrentUI.SameAs(myLastUnsentUI) || !myCurrentUI.IsSourceOrTargetNull() || myLastUnsentUI.IsSourceOrTargetNull()) ? myCurrentUI : myLastUnsentUI);
		if (GameMgr.Get().IsBattlegroundDuoGame())
		{
			bool viewingTeammate = TeammateBoardViewer.Get() != null && TeammateBoardViewer.Get().IsViewingTeammate();
			Network.Get().SendUserUI(sendUI.over.ID, sendUI.held.ID, sendUI.origin.ID, 0, 0, !viewingTeammate);
		}
		else
		{
			Network.Get().SendUserUI(sendUI.over.ID, sendUI.held.ID, sendUI.origin.ID, 0, 0);
		}
		myLastUI.CopyFrom(myCurrentUI);
		myLastUnsentUI.Clear();
	}

	private void ProcessUserSelection()
	{
		if (myCurrentEntitySelection == myLastEntitySelection)
		{
			return;
		}
		if (!CanSendSelection())
		{
			myLastEntitySelection = myCurrentEntitySelection;
			return;
		}
		if (myLastUnsentEntitySelection >= 0 && myCurrentEntitySelection != myLastUnsentEntitySelection)
		{
			Network.Get().SendSelection(myLastUnsentEntitySelection);
		}
		else
		{
			Network.Get().SendSelection(myCurrentEntitySelection);
		}
		myLastEntitySelection = myCurrentEntitySelection;
		myLastUnsentEntitySelection = -1;
	}

	private bool CanSendUI()
	{
		if (GameMgr.Get() == null)
		{
			return false;
		}
		if (!InputManager.Get().PermitDecisionMakingInput())
		{
			return false;
		}
		if (GameMgr.Get().IsAI() && !SpectatorManager.Get().MyGameHasSpectators())
		{
			return false;
		}
		float now = Time.realtimeSinceStartup;
		float lastSendDeltaSec = now - m_lastSendTime;
		if (IsSendingTargetingArrow() && lastSendDeltaSec > 0.25f)
		{
			m_lastSendTime = now;
			return true;
		}
		if (lastSendDeltaSec < 0.35f)
		{
			return false;
		}
		m_lastSendTime = now;
		return true;
	}

	private bool CanSendSelection()
	{
		if (GameMgr.Get() == null)
		{
			return false;
		}
		if (!InputManager.Get().PermitDecisionMakingInput())
		{
			return false;
		}
		return true;
	}

	private bool IsSendingTargetingArrow()
	{
		if (myCurrentUI.origin.card == null)
		{
			return false;
		}
		if (myCurrentUI.over.card == null)
		{
			return false;
		}
		if (myCurrentUI.over.card == myCurrentUI.origin.card)
		{
			return false;
		}
		if (myCurrentUI.origin.card != myLastUI.origin.card)
		{
			return true;
		}
		if (myCurrentUI.over.card != myLastUI.over.card)
		{
			return true;
		}
		return false;
	}

	private void UpdateCardOver()
	{
		Card oldCard = enemyActualUI.over.card;
		Card card = enemyWantedUI.over.card;
		if (oldCard != card)
		{
			enemyActualUI.over.card = card;
			if (!GameState.Get().GetGameEntity().HasTag(GAME_TAG.REVEAL_CHOICES))
			{
				if (oldCard != null)
				{
					oldCard.NotifyOpponentMousedOffThisCard();
				}
				if (card != null)
				{
					card.NotifyOpponentMousedOverThisCard();
				}
			}
			ZoneMgr.Get().FindZoneOfType<ZoneHand>(Player.Side.OPPOSING).UpdateLayout(card);
		}
		Card teammateOldCard = teammateActualUI.over.card;
		Card teammateCard = teammateWantedUI.over.card;
		if (teammateOldCard != teammateCard)
		{
			teammateActualUI.over.card = teammateCard;
			if (!GameState.Get().GetGameEntity().HasTag(GAME_TAG.REVEAL_CHOICES))
			{
				if (teammateOldCard != null)
				{
					teammateOldCard.NotifyTeammateMousedOffThisCard();
				}
				if (teammateCard != null)
				{
					teammateCard.NotifyTeammateMousedOverThisCard();
				}
			}
		}
		if (!GameMgr.Get().IsSpectator())
		{
			return;
		}
		Card friendlyOldCard = friendlyActualUI.over.card;
		Card friendlyCard = friendlyWantedUI.over.card;
		if (!(friendlyOldCard != friendlyCard))
		{
			return;
		}
		friendlyActualUI.over.card = friendlyCard;
		if (friendlyOldCard != null)
		{
			ZoneHand friendlyHandZone = friendlyOldCard.GetZone() as ZoneHand;
			if (friendlyHandZone != null)
			{
				if (friendlyHandZone.CurrentStandIn == null)
				{
					friendlyHandZone.UpdateLayout(null);
				}
			}
			else
			{
				friendlyOldCard.NotifyMousedOut();
			}
		}
		if (!(friendlyCard != null))
		{
			return;
		}
		ZoneHand friendlyHandZone2 = friendlyCard.GetZone() as ZoneHand;
		if (friendlyHandZone2 != null)
		{
			if (friendlyHandZone2.CurrentStandIn == null)
			{
				friendlyHandZone2.UpdateLayout(friendlyCard);
			}
		}
		else
		{
			friendlyCard.NotifyMousedOver();
		}
	}

	private void UpdateCardHeld()
	{
		Card oldCard = enemyActualUI.held.card;
		Card card = enemyWantedUI.held.card;
		if (oldCard != card)
		{
			enemyActualUI.held.card = card;
			if (oldCard != null)
			{
				oldCard.MarkAsGrabbedByEnemyActionHandler(enable: false);
			}
			if (IsCardInHand(oldCard))
			{
				oldCard.GetZone().UpdateLayout();
			}
			if (CanAnimateHeldCard(card))
			{
				card.MarkAsGrabbedByEnemyActionHandler(enable: true);
				if (SpectatorManager.Get().IsSpectatingOpposingSide())
				{
					StandUpright(isFriendlySide: false);
				}
				Hashtable tweenArgs = iTweenManager.Get().GetTweenHashTable();
				tweenArgs.Add("name", "RemoteActionHandler");
				tweenArgs.Add("position", Board.Get().FindBone("OpponentCardPlayingSpot").position);
				tweenArgs.Add("time", 1f);
				tweenArgs.Add("oncomplete", (Action<object>)delegate
				{
					StartDrift(isFriendlySide: false);
				});
				tweenArgs.Add("oncompletetarget", base.gameObject);
				iTween.MoveTo(card.gameObject, tweenArgs);
			}
		}
		Card teammateOldCard = teammateActualUI.held.card;
		Card teammateCard = teammateWantedUI.held.card;
		if (teammateOldCard != teammateCard)
		{
			bool isViewingTeammate = TeammateBoardViewer.Get() != null && TeammateBoardViewer.Get().IsViewingTeammate();
			teammateActualUI.held.card = teammateCard;
			if (teammateOldCard != null)
			{
				TeammateBoardViewer.Get().NotifyOfDropedCard();
			}
			if (teammateCard != null && isViewingTeammate)
			{
				string boneName = ((teammateCard.GetControllerSide() == Player.Side.FRIENDLY) ? "FriendlyCardPlayingSpot" : "OpponentCardPlayingSpot");
				Vector3 holdPosition = Board.Get().FindBone(boneName).position + Board.Get().FindBone("TeammateBoardPosition").position;
				Hashtable tweenArgs2 = iTweenManager.Get().GetTweenHashTable();
				tweenArgs2.Add("name", "RemoteActionHandler");
				tweenArgs2.Add("position", holdPosition);
				tweenArgs2.Add("time", 1f);
				tweenArgs2.Add("oncomplete", (Action<object>)delegate
				{
					StartDrift(isFriendlySide: false, isTeammate: true);
				});
				tweenArgs2.Add("oncompletetarget", base.gameObject);
				iTween.Stop(teammateCard.gameObject);
				iTween.MoveTo(teammateCard.gameObject, tweenArgs2);
			}
		}
		if (!GameMgr.Get().IsSpectator())
		{
			return;
		}
		Card friendlyOldCard = friendlyActualUI.held.card;
		Card friendlyCard = friendlyWantedUI.held.card;
		if (!(friendlyOldCard != friendlyCard))
		{
			return;
		}
		friendlyActualUI.held.card = friendlyCard;
		if (friendlyOldCard != null)
		{
			friendlyOldCard.MarkAsGrabbedByEnemyActionHandler(enable: false);
		}
		if (IsCardInHand(friendlyOldCard))
		{
			friendlyOldCard.GetZone().UpdateLayout();
		}
		if (!CanAnimateHeldCard(friendlyCard))
		{
			return;
		}
		friendlyCard.MarkAsGrabbedByEnemyActionHandler(enable: true);
		ZoneHand friendlyHandZone = friendlyCard.GetZone() as ZoneHand;
		Hashtable tweenArgs3;
		if (friendlyHandZone != null)
		{
			if (friendlyHandZone.CurrentStandIn == null || friendlyHandZone.CurrentStandIn.linkedCard == friendlyCard)
			{
				friendlyCard.NotifyMousedOut();
			}
			Vector3 newScale = friendlyHandZone.GetCardScale();
			tweenArgs3 = iTweenManager.Get().GetTweenHashTable();
			tweenArgs3.Add("scale", newScale);
			tweenArgs3.Add("time", 0.15f);
			tweenArgs3.Add("easetype", iTween.EaseType.easeOutExpo);
			tweenArgs3.Add("name", "RemoteActionHandler");
			iTween.ScaleTo(friendlyCard.gameObject, tweenArgs3);
		}
		tweenArgs3 = iTweenManager.Get().GetTweenHashTable();
		tweenArgs3.Add("name", "RemoteActionHandler");
		tweenArgs3.Add("position", Board.Get().FindBone("FriendlyCardPlayingSpot").position);
		tweenArgs3.Add("time", 1f);
		tweenArgs3.Add("oncomplete", (Action<object>)delegate
		{
			StartDrift(isFriendlySide: true);
		});
		tweenArgs3.Add("oncompletetarget", base.gameObject);
		iTween.MoveTo(friendlyCard.gameObject, tweenArgs3);
		LayerUtils.SetLayer(friendlyCard, GameLayer.Default);
	}

	private void StartDrift(bool isFriendlySide, bool isTeammate = false)
	{
		if (isFriendlySide || !GameState.Get().GetOpposingSidePlayer().IsRevealed())
		{
			StandUpright(isFriendlySide, isTeammate);
		}
		DriftLeftAndRight(isFriendlySide, isTeammate);
	}

	private void DriftLeftAndRight(bool isFriendlySide, bool isTeammate = false)
	{
		Card card = GetActualHeldCard(!isFriendlySide, isTeammate);
		if (!CanAnimateHeldCard(card))
		{
			return;
		}
		Vector3[] path;
		if (isFriendlySide)
		{
			if (!iTweenPath.paths.TryGetValue(iTweenPath.FixupPathName("driftPath1_friendly"), out var friendlyTweenPath))
			{
				Transform opposingBone = Board.Get().FindBone("OpponentCardPlayingSpot");
				Transform obj = Board.Get().FindBone("FriendlyCardPlayingSpot");
				Vector3 deltaPos = obj.position - opposingBone.position;
				iTweenPath opposingTweenPath = iTweenPath.paths[iTweenPath.FixupPathName("driftPath1")];
				friendlyTweenPath = obj.gameObject.AddComponent<iTweenPath>();
				friendlyTweenPath.pathVisible = true;
				friendlyTweenPath.pathName = "driftPath1_friendly";
				friendlyTweenPath.pathColor = opposingTweenPath.pathColor;
				friendlyTweenPath.nodes = new List<Vector3>(opposingTweenPath.nodes);
				for (int i = 0; i < friendlyTweenPath.nodes.Count; i++)
				{
					friendlyTweenPath.nodes[i] = opposingTweenPath.nodes[i] + deltaPos;
				}
				friendlyTweenPath.enabled = false;
				friendlyTweenPath.enabled = true;
			}
			path = friendlyTweenPath.nodes.ToArray();
		}
		else
		{
			path = iTweenPath.GetPath("driftPath1");
		}
		Hashtable tweenArgs = iTweenManager.Get().GetTweenHashTable();
		tweenArgs.Add("name", "RemoteActionHandler");
		tweenArgs.Add("path", path);
		tweenArgs.Add("time", 10f);
		tweenArgs.Add("easetype", iTween.EaseType.linear);
		tweenArgs.Add("looptype", iTween.LoopType.pingPong);
		iTween.MoveTo(card.gameObject, tweenArgs);
	}

	private void StandUpright(bool isFriendlySide, bool isTeammate = false)
	{
		Card card = GetActualHeldCard(!isFriendlySide, isTeammate);
		if (CanAnimateHeldCard(card))
		{
			float rotateTime = 5f;
			if (!isFriendlySide && GameState.Get().GetOpposingSidePlayer().IsRevealed())
			{
				rotateTime = 0.3f;
			}
			Hashtable tweenArgs = iTweenManager.Get().GetTweenHashTable();
			tweenArgs.Add("name", "RemoteActionHandler");
			tweenArgs.Add("rotation", Vector3.zero);
			tweenArgs.Add("time", rotateTime);
			tweenArgs.Add("easetype", iTween.EaseType.easeInOutSine);
			iTween.RotateTo(card.gameObject, tweenArgs);
		}
	}

	private void MaybeDestroyArrow()
	{
		if (TargetReticleManager.Get() == null || !TargetReticleManager.Get().IsActive())
		{
			return;
		}
		bool isFriendlyTurn = GameState.Get() != null && GameState.Get().IsFriendlySidePlayerTurn();
		bool isViewingTeammate = TeammateBoardViewer.Get() != null && TeammateBoardViewer.Get().IsViewingTeammate();
		UserUI wantedUserUI = GetWantedUserUI(!isFriendlyTurn, isViewingTeammate);
		UserUI actualUI = GetActualUserUI(!isFriendlyTurn, isViewingTeammate);
		if ((object)wantedUserUI.origin.card == actualUI.origin.card)
		{
			return;
		}
		if (actualUI.origin.card != null && actualUI.origin.card.GetActor() != null && !actualUI.origin.card.ShouldShowImmuneVisuals())
		{
			actualUI.origin.card.GetActor().ActivateSpellDeathState(SpellType.IMMUNE);
		}
		bool isLettuceAbility = actualUI.origin.entity != null && actualUI.origin.entity.IsLettuceAbility();
		actualUI.origin.card = null;
		if (isFriendlyTurn)
		{
			if (isLettuceAbility)
			{
				ZoneMgr.Get().DisplayLettuceAbilitiesForPreviouslySelectedCard();
			}
			TargetReticleManager.Get().DestroyFriendlyTargetArrow(isLocallyCanceled: false);
		}
		else
		{
			m_destroyEnemyTargetArrowCoroutine = DestroyEnemyTargetArrow();
			StartCoroutine(m_destroyEnemyTargetArrowCoroutine);
		}
	}

	private void MaybeCreateArrow()
	{
		if (TargetReticleManager.Get() == null || (TargetReticleManager.Get().IsActive() && !TargetReticleManager.Get().IsStaticArrow()))
		{
			return;
		}
		bool isFriendlyTurn = GameState.Get() != null && GameState.Get().IsFriendlySidePlayerTurn();
		bool isViewingTeammate = TeammateBoardViewer.Get() != null && TeammateBoardViewer.Get().IsViewingTeammate();
		UserUI wantedUI = GetWantedUserUI(!isFriendlyTurn, isViewingTeammate);
		UserUI actualUI = GetActualUserUI(!isFriendlyTurn, isViewingTeammate);
		if (wantedUI.origin.card == null || actualUI.over.card == null || actualUI.over.card.GetActor() == null || !actualUI.over.card.GetActor().IsShown() || actualUI.over.card == wantedUI.origin.card)
		{
			return;
		}
		Player currentPlayer = GameState.Get().GetCurrentPlayer();
		if ((currentPlayer == null || currentPlayer.IsLocalUser()) && !isViewingTeammate)
		{
			return;
		}
		actualUI.origin.card = wantedUI.origin.card;
		if (isFriendlyTurn)
		{
			bool showDamageIndicatorText = false;
			if (actualUI.origin.entity != null && actualUI.origin.entity.IsLettuceAbility())
			{
				showDamageIndicatorText = true;
				ZoneMgr.Get().TemporarilyDismissMercenariesAbilityTray();
			}
			TargetReticleManager.Get().CreateFriendlyTargetArrow(actualUI.origin.entity, showDamageIndicatorText);
		}
		else
		{
			if (m_destroyEnemyTargetArrowCoroutine != null)
			{
				StopCoroutine(m_destroyEnemyTargetArrowCoroutine);
			}
			TargetReticleManager.Get().CreateEnemyTargetArrow(actualUI.origin.entity);
		}
		if (actualUI.origin.entity.GetRealTimeIsImmuneWhileAttacking())
		{
			actualUI.origin.card.ActivateActorSpell(SpellType.IMMUNE);
		}
		SetArrowTarget();
	}

	private IEnumerator DestroyEnemyTargetArrow()
	{
		yield return new WaitForSeconds(0.25f);
		TargetReticleManager.Get().DestroyEnemyTargetArrow();
	}

	private void UpdateTargetArrow()
	{
		if (!(TargetReticleManager.Get() == null) && TargetReticleManager.Get().IsActive())
		{
			SetArrowTarget();
		}
	}

	private void SetArrowTarget()
	{
		bool isFriendlyTurn = GameState.Get() != null && GameState.Get().IsFriendlySidePlayerTurn();
		bool isViewingTeammate = TeammateBoardViewer.Get() != null && TeammateBoardViewer.Get().IsViewingTeammate();
		UserUI wantedUI = GetWantedUserUI(!isFriendlyTurn, isViewingTeammate);
		UserUI actualUI = GetActualUserUI(!isFriendlyTurn, isViewingTeammate);
		if (!(actualUI.over.card == null) && !(actualUI.origin.card == null) && !(actualUI.over.card.GetActor() == null) && actualUI.over.card.GetActor().IsShown() && !(actualUI.over.card == wantedUI.origin.card))
		{
			Vector3 cameraPosition = Camera.main.transform.position;
			Vector3 entityPosition = actualUI.over.card.transform.position;
			if (Physics.Raycast(new Ray(cameraPosition, entityPosition - cameraPosition), out var hitInfo, Camera.main.farClipPlane, GameLayer.DragPlane.LayerBit()))
			{
				TargetReticleManager.Get().SetRemotePlayerArrowPosition(hitInfo.point);
			}
		}
	}

	private bool IsCardInHand(Card card)
	{
		if (card == null)
		{
			return false;
		}
		if (!(card.GetZone() is ZoneHand))
		{
			return false;
		}
		if (card.GetEntity().GetZone() != TAG_ZONE.HAND)
		{
			return false;
		}
		return true;
	}

	private bool CanAnimateHeldCard(Card card)
	{
		if (!IsCardInHand(card))
		{
			return false;
		}
		if (card.IsDoNotSort())
		{
			return false;
		}
		string zoneHandTweenLabel = ZoneMgr.Get().GetTweenName<ZoneHand>();
		if (iTween.HasNameNotInList(card.gameObject, "RemoteActionHandler", zoneHandTweenLabel))
		{
			return false;
		}
		return true;
	}

	private void OnTurnChanged(int oldTurn, int newTurn, object userData)
	{
		Player currentPlayer = GameState.Get().GetCurrentPlayer();
		if ((currentPlayer != null && !currentPlayer.IsLocalUser() && !GameMgr.Get().IsSpectator()) || TargetReticleManager.Get() == null)
		{
			return;
		}
		UserUI actualUI;
		if (currentPlayer.IsFriendlySide())
		{
			actualUI = friendlyActualUI;
			if (TargetReticleManager.Get().IsEnemyArrowActive())
			{
				TargetReticleManager.Get().DestroyEnemyTargetArrow();
			}
		}
		else
		{
			actualUI = enemyActualUI;
			if (TargetReticleManager.Get().IsLocalArrowActive())
			{
				TargetReticleManager.Get().DestroyFriendlyTargetArrow(isLocallyCanceled: false);
			}
		}
		if (actualUI.origin != null && actualUI.origin.entity != null && actualUI.origin.card != null && !actualUI.origin.card.ShouldShowImmuneVisuals())
		{
			actualUI.origin.card.GetActor().ActivateSpellDeathState(SpellType.IMMUNE);
		}
	}

	private bool CanReceiveEnemyEmote(EmoteType emoteType, int playerId)
	{
		if (EnemyEmoteHandler.Get() == null && !GameState.Get().GetBooleanGameOption(GameEntityOption.USES_PREMIUM_EMOTES))
		{
			return false;
		}
		if (EnemyEmoteHandler.Get() != null && EnemyEmoteHandler.Get().IsSquelched(playerId))
		{
			return false;
		}
		if (emoteType == EmoteType.COLLECTIBLE_BATTLEGROUNDS_EMOTE && GameMgr.Get().IsBattlegrounds())
		{
			return true;
		}
		if (EmoteHandler.Get() == null)
		{
			return false;
		}
		return EmoteHandler.Get().IsValidEmoteTypeForOpponent(emoteType);
	}
}
