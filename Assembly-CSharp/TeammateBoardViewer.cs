using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PegasusGame;
using UnityEngine;

[CustomEditClass]
public class TeammateBoardViewer : MonoBehaviour
{
	private struct SwapableGameObject
	{
		private GameObject m_swapObject;

		private Vector3 m_originalPosition;

		private Vector3 m_teammateBoardPosition;

		public SwapableGameObject(Component component, Vector3 teammateBoardPos)
		{
			GameObject go = (m_swapObject = ((component != null) ? component.gameObject : null));
			m_originalPosition = ((go != null) ? go.transform.position : Vector3.zero);
			m_teammateBoardPosition = m_originalPosition + teammateBoardPos;
		}

		public SwapableGameObject(Component component, Vector3 originalBoardPos, Vector3 teammateBoardPos)
		{
			GameObject go = ((component != null) ? component.gameObject : null);
			m_swapObject = go;
			m_originalPosition = originalBoardPos;
			m_teammateBoardPosition = m_originalPosition + teammateBoardPos;
		}

		public void MoveToTeammatesBoard()
		{
			if (!(m_swapObject == null))
			{
				m_swapObject.transform.position = m_teammateBoardPosition;
			}
		}

		public void MoveToOurBoard()
		{
			if (!(m_swapObject == null))
			{
				m_swapObject.transform.position = m_originalPosition;
			}
		}

		public bool CheckConsistency(bool showTeammate)
		{
			if (m_swapObject == null)
			{
				return false;
			}
			if (showTeammate)
			{
				if (m_swapObject.transform.position != m_teammateBoardPosition)
				{
					m_swapObject.transform.position = m_teammateBoardPosition;
					return true;
				}
			}
			else if (m_swapObject.transform.position != m_originalPosition)
			{
				m_swapObject.transform.position = m_originalPosition;
				return true;
			}
			return false;
		}

		public Vector3 GetOriginalPosition()
		{
			return m_originalPosition;
		}
	}

	[CustomEditField(T = EditType.SPELL)]
	public string m_vignetteSpellPath;

	public float m_transitionToTeammateDelay;

	public float m_transitionToSelfDelay;

	private static TeammateBoardViewer s_instance;

	private DuosPortal m_duosPortal;

	private Spell m_vignetteSpell;

	private Vector3 m_teammateBoardPos;

	private Vector3 m_boardCameraLocalPosition;

	private SwapableGameObject m_boardCamerasSwap;

	private SwapableGameObject m_duosPortalSwap;

	private SwapableGameObject m_endTurnButtonSwap;

	private SwapableGameObject m_turnTimerSwap;

	private SwapableGameObject m_leaderboardSwap;

	private SwapableGameObject m_damageCapFxSwap;

	private SwapableGameObject m_handColliderSwap;

	private SwapableGameObject m_dragPlaneColliderSwap;

	private SwapableGameObject m_enemyEmoteHandlerSwap;

	private SwapableGameObject m_enemyDeckTooltipZone;

	private SwapableGameObject m_playerHandTooltipZone;

	private bool m_initialized;

	private bool m_viewingTeammate;

	private bool m_transitioningToTeammateView;

	private bool m_blockViewTeammate;

	private TagMap m_teammatePlayerTags = new TagMap();

	private TeammateMinionViewer m_teammateMinionViewer;

	private TeammateHandViewer m_teammateHandViewer;

	private TeammateHeroViewer m_teammateHeroViewer;

	private TeammateGameModeButtonViewer m_teammateGameModeButtonViewer;

	private TeammateDiscoverViewer m_teammateDiscoverViewer;

	private TeammateHeroSelectViewer m_teammateHeroSelectViewer;

	private TeammateSecretViewer m_teammateSecretViewer;

	private List<TeammateViewer> m_teammateViewers = new List<TeammateViewer>();

	public void Awake()
	{
		s_instance = this;
		m_teammateMinionViewer = new TeammateMinionViewer();
		m_teammateHandViewer = new TeammateHandViewer();
		m_teammateHeroViewer = new TeammateHeroViewer();
		m_teammateGameModeButtonViewer = new TeammateGameModeButtonViewer();
		m_teammateDiscoverViewer = new TeammateDiscoverViewer();
		m_teammateHeroSelectViewer = new TeammateHeroSelectViewer();
		m_teammateSecretViewer = new TeammateSecretViewer();
		m_teammateViewers.Add(m_teammateMinionViewer);
		m_teammateViewers.Add(m_teammateHandViewer);
		m_teammateViewers.Add(m_teammateHeroViewer);
		m_teammateViewers.Add(m_teammateSecretViewer);
		m_teammateViewers.Add(m_teammateGameModeButtonViewer);
		m_teammateViewers.Add(m_teammateDiscoverViewer);
		m_teammateViewers.Add(m_teammateHeroSelectViewer);
		m_vignetteSpell = SpellUtils.LoadAndSetupSpell(m_vignetteSpellPath, this);
		m_vignetteSpell.gameObject.SetActive(value: true);
		StartCoroutine(m_teammateGameModeButtonViewer.WaitForManaCrystalManagerToBeReadyThenInitTeammateManaCrystals());
		m_teammateViewers.ForEach(delegate(TeammateViewer viewer)
		{
			viewer.SetupViewerSpells(this);
		});
	}

	public void OnDestroy()
	{
		StopAllCoroutines();
		s_instance = null;
	}

	public void Update()
	{
		if (GameMgr.Get().IsBattlegroundDuoGame())
		{
			m_teammateViewers.ForEach(delegate(TeammateViewer viewer)
			{
				viewer.UpdateViewer();
			});
			MaintainConsistency();
		}
	}

	public static TeammateBoardViewer Get()
	{
		return s_instance;
	}

	public bool IsViewingTeammate()
	{
		return m_viewingTeammate;
	}

	public bool IsViewingOrTransitioningToTeammateView()
	{
		if (!IsViewingTeammate())
		{
			return m_transitioningToTeammateView;
		}
		return true;
	}

	public void SetBlockViewingTeammate(bool block)
	{
		m_blockViewTeammate = block;
	}

	public void Initialize()
	{
		m_teammateBoardPos = Board.Get().FindBone("TeammateBoardPosition").position;
		m_boardCameraLocalPosition = BoardCameras.Get().GetCamera().transform.localPosition;
		m_boardCamerasSwap = new SwapableGameObject(BoardCameras.Get(), m_teammateBoardPos);
		m_leaderboardSwap = new SwapableGameObject(PlayerLeaderboardManager.Get(), m_teammateBoardPos);
		m_damageCapFxSwap = new SwapableGameObject(Board.Get().m_leaderboardDamageCapFX, m_teammateBoardPos);
		m_handColliderSwap = new SwapableGameObject(Board.Get().FindCollider("FriendlyHandScrollHitbox"), m_teammateBoardPos);
		m_dragPlaneColliderSwap = new SwapableGameObject(Board.Get().FindCollider("DragPlane"), m_teammateBoardPos);
		ZoneDeck enemyDeck = ZoneMgr.Get().FindZonesOfType<ZoneDeck>(Player.Side.OPPOSING).FirstOrDefault();
		if (enemyDeck != null)
		{
			m_enemyDeckTooltipZone = new SwapableGameObject(enemyDeck.m_deckTooltipZone, m_teammateBoardPos);
			m_playerHandTooltipZone = new SwapableGameObject(enemyDeck.m_friendlyHandTooltipZone, m_teammateBoardPos);
		}
		m_endTurnButtonSwap = new SwapableGameObject(EndTurnButton.Get(), Board.Get().FindBone("EndTurnButton").position, m_teammateBoardPos);
		m_turnTimerSwap = new SwapableGameObject(TurnTimer.Get(), Vector3.zero, m_teammateBoardPos);
		m_teammateViewers.ForEach(delegate(TeammateViewer viewer)
		{
			viewer.InitZones(m_teammateBoardPos);
		});
		StartCoroutine(m_teammateGameModeButtonViewer.KeepTechLevelUpToDateCoroutine());
		Network.Get().RequestTeammatesGamestate();
		m_initialized = true;
	}

	public void InitGameModeButtons()
	{
		m_teammateGameModeButtonViewer.InitGameModeButtons();
	}

	public bool IsActorTeammates(Actor actor)
	{
		foreach (TeammateViewer teammateViewer in m_teammateViewers)
		{
			if (teammateViewer.IsActorInViewer(actor))
			{
				return true;
			}
		}
		return false;
	}

	public Entity GetTeammateEntity(int entityID)
	{
		foreach (TeammateViewer teammateViewer in m_teammateViewers)
		{
			if (teammateViewer.GetTeammateEntity(entityID, out var teammatesEntity))
			{
				return teammatesEntity;
			}
		}
		return null;
	}

	public void NotifyOfDropedCard()
	{
		m_teammateMinionViewer.UpdateZonesLayouts();
		m_teammateHandViewer.UpdateZonesLayouts();
	}

	public Entity GetTeammateHero()
	{
		return m_teammateHeroViewer.GetTeammateHero();
	}

	private void DoShowTeammatesBoard()
	{
		m_transitioningToTeammateView = false;
		if (!m_blockViewTeammate)
		{
			m_viewingTeammate = true;
			m_vignetteSpell.transform.position = m_teammateBoardPos + Board.Get().FindBone("CenterPointBone").position;
			Camera boardCamera = BoardCameras.Get().GetCamera();
			if (CameraShakeMgr.IsShaking(boardCamera))
			{
				CameraShakeMgr.Stop(boardCamera);
			}
			m_boardCamerasSwap.MoveToTeammatesBoard();
			m_duosPortalSwap.MoveToTeammatesBoard();
			m_endTurnButtonSwap.MoveToTeammatesBoard();
			m_turnTimerSwap.MoveToTeammatesBoard();
			m_enemyEmoteHandlerSwap.MoveToTeammatesBoard();
			m_enemyDeckTooltipZone.MoveToTeammatesBoard();
			m_playerHandTooltipZone.MoveToTeammatesBoard();
			m_handColliderSwap.MoveToTeammatesBoard();
			m_dragPlaneColliderSwap.MoveToTeammatesBoard();
			if (!(MulliganManager.Get() != null) || !MulliganManager.Get().IsMulliganActive())
			{
				m_leaderboardSwap.MoveToTeammatesBoard();
				m_damageCapFxSwap.MoveToTeammatesBoard();
				PlayerLeaderboardManager.Get().AnimateTeamsToPositions(animate: false);
			}
			GameState.Get().GetGameEntity().UpdateNameDisplay();
			RemoteActionHandler.Get().NotifySwitchingToTeammateView();
		}
	}

	private IEnumerator WaitThenShowTeammatesBoard()
	{
		yield return new WaitForSeconds(m_transitionToTeammateDelay);
		DoShowTeammatesBoard();
		if (!GameState.Get().GetGameEntity().IsInBattlegroundsCombatPhase())
		{
			m_duosPortal.SetPortalClickable(clickable: true);
		}
	}

	public void ShowTeammateBoard()
	{
		if (!m_blockViewTeammate)
		{
			m_vignetteSpell.ActivateState(SpellStateType.BIRTH);
			m_vignetteSpell.transform.position = Board.Get().FindBone("CenterPointBone").position;
			m_transitioningToTeammateView = true;
			if (m_transitionToTeammateDelay == 0f)
			{
				DoShowTeammatesBoard();
				return;
			}
			m_duosPortal.SetPortalClickable(clickable: false);
			StartCoroutine("WaitThenShowTeammatesBoard");
		}
	}

	private void DoHideTeammatesBoard()
	{
		m_viewingTeammate = false;
		m_vignetteSpell.transform.position = Board.Get().FindBone("CenterPointBone").position;
		Camera boardCamera = BoardCameras.Get().GetCamera();
		if (CameraShakeMgr.IsShaking(boardCamera))
		{
			CameraShakeMgr.Stop(boardCamera);
		}
		m_boardCamerasSwap.MoveToOurBoard();
		m_duosPortalSwap.MoveToOurBoard();
		m_endTurnButtonSwap.MoveToOurBoard();
		m_turnTimerSwap.MoveToOurBoard();
		m_enemyEmoteHandlerSwap.MoveToOurBoard();
		m_enemyDeckTooltipZone.MoveToOurBoard();
		m_playerHandTooltipZone.MoveToOurBoard();
		m_handColliderSwap.MoveToOurBoard();
		m_dragPlaneColliderSwap.MoveToOurBoard();
		if (!(MulliganManager.Get() != null) || !MulliganManager.Get().IsMulliganActive())
		{
			m_leaderboardSwap.MoveToOurBoard();
			m_damageCapFxSwap.MoveToOurBoard();
			PlayerLeaderboardManager.Get().AnimateTeamsToPositions(animate: false);
		}
		GameState.Get().GetGameEntity().UpdateNameDisplay();
		TargetReticleManager.Get().DestroyFriendlyTargetArrow(isLocallyCanceled: false);
	}

	private IEnumerator WaitThenHideTeammatesBoard()
	{
		yield return new WaitForSeconds(m_transitionToSelfDelay);
		DoHideTeammatesBoard();
		if (!GameState.Get().GetGameEntity().IsInBattlegroundsCombatPhase())
		{
			m_duosPortal.SetPortalClickable(clickable: true);
		}
	}

	public void HideTeammateBoard()
	{
		m_vignetteSpell.ActivateState(SpellStateType.DEATH);
		m_vignetteSpell.transform.position = m_teammateBoardPos + Board.Get().FindBone("CenterPointBone").position;
		if (m_transitionToSelfDelay == 0f)
		{
			DoHideTeammatesBoard();
			return;
		}
		m_duosPortal.SetPortalClickable(clickable: false);
		StartCoroutine("WaitThenHideTeammatesBoard");
	}

	private void MaintainConsistency()
	{
		if (!m_initialized)
		{
			return;
		}
		Camera boardCamera = BoardCameras.Get().GetCamera();
		if (CameraShakeMgr.IsShaking(boardCamera))
		{
			return;
		}
		if (m_boardCameraLocalPosition != boardCamera.transform.localPosition)
		{
			boardCamera.transform.localPosition = m_boardCameraLocalPosition;
		}
		bool viewingTeammate = IsViewingTeammate();
		m_duosPortalSwap.CheckConsistency(viewingTeammate);
		m_endTurnButtonSwap.CheckConsistency(viewingTeammate);
		m_turnTimerSwap.CheckConsistency(viewingTeammate);
		m_enemyEmoteHandlerSwap.CheckConsistency(viewingTeammate);
		m_enemyDeckTooltipZone.CheckConsistency(viewingTeammate);
		m_playerHandTooltipZone.CheckConsistency(viewingTeammate);
		m_handColliderSwap.CheckConsistency(viewingTeammate);
		m_dragPlaneColliderSwap.CheckConsistency(viewingTeammate);
		if (!(MulliganManager.Get() != null) || !MulliganManager.Get().IsMulliganActive())
		{
			m_damageCapFxSwap.CheckConsistency(viewingTeammate);
			if (m_leaderboardSwap.CheckConsistency(viewingTeammate))
			{
				PlayerLeaderboardManager.Get().AnimateTeamsToPositions(animate: false);
			}
		}
	}

	private void RemoveActors(TeammatesEntities teammatesEntities)
	{
		m_teammateViewers.ForEach(delegate(TeammateViewer viewer)
		{
			viewer.RemoveActors(teammatesEntities);
		});
	}

	public void UpdateTeammateMulliganHero(ReplaceBattlegroundMulliganHero packet)
	{
		m_teammateHeroSelectViewer.UpdateHeroes(packet);
	}

	public void UpdateTeammateEntities(TeammatesEntities teammatesEntities)
	{
		m_teammatePlayerTags.Clear();
		m_teammatePlayerTags.SetTags(teammatesEntities.PlayerTags);
		m_teammateMinionViewer.TrackZoneTransitions(teammatesEntities, m_teammateHandViewer);
		m_teammateHandViewer.TrackZoneTransitions(teammatesEntities, m_teammateMinionViewer, m_teammateDiscoverViewer);
		RemoveActors(teammatesEntities);
		m_teammateViewers.ForEach(delegate(TeammateViewer viewer)
		{
			viewer.UpdateTeammateEntities(teammatesEntities);
		});
		foreach (TeammateEntityData entity in teammatesEntities.Entities)
		{
			foreach (TeammateViewer viewer2 in m_teammateViewers)
			{
				if (viewer2.ShouldEntityBeInViewer(entity))
				{
					viewer2.AddEntityToViewer(entity);
				}
			}
		}
		m_teammateViewers.ForEach(delegate(TeammateViewer viewer)
		{
			viewer.PostAddingEntitiesToViewer();
		});
	}

	public void UpdateHeroHighlightedState(int entityId, bool isConfirmation)
	{
		m_teammateHeroSelectViewer.UpdateHeroHighlightedState(entityId, isConfirmation);
	}

	public void UpdateTeammatesChooseEntities(TeammatesChooseEntities chooseEntities)
	{
		if (chooseEntities.ChoiceType == 1)
		{
			m_teammateHeroSelectViewer.AddMulliganEntitiesToViewer(chooseEntities);
		}
		else
		{
			m_teammateDiscoverViewer.AddDiscoverEntitiesToViewer(chooseEntities);
		}
	}

	public void UpdateTeammatesEntitiesChosen(TeammatesEntitiesChosen entityChosen)
	{
		Log.Gameplay.PrintDebug("Teammate chose their entity {0}.", entityChosen.EntityID);
		if (entityChosen.ChoiceType == 1)
		{
			m_teammateHeroSelectViewer.ChooseEntitySelected(entityChosen);
			if (m_duosPortal != null)
			{
				m_duosPortal.SetTeammateHeroActor(m_teammateHeroSelectViewer.GetSelectedHero().GetActor());
				if (!IsViewingTeammate())
				{
					m_duosPortal.ApplyTeammateHeroTexture();
				}
			}
		}
		else
		{
			m_teammateDiscoverViewer.ChooseEntitySelected(entityChosen);
		}
	}

	public void DeleteDiscoverEntities(bool includeChosen)
	{
		m_teammateHeroSelectViewer.DeleteEntities(includeChosen);
		m_teammateDiscoverViewer.DeleteEntities(includeChosen);
	}

	public void ClickedTeammatesCardInHand(Card card)
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_teammateHandViewer.ClickedTeamamtesHandPhone(card);
		}
		else
		{
			GameplayErrorManager.Get().DisplayMessage(GameStrings.Get("GAMEPLAY_BACON_CLICKING_ON_TEAMMATES_MINIONS"));
		}
	}

	public void HandleLeftMouseUp()
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_teammateHandViewer.ReleaseTeammatesCardInHand();
		}
	}

	public TeammateHandViewer GetTeammateHandViewer()
	{
		return m_teammateHandViewer;
	}

	public TeammateSecretViewer GetTeammateSecretViewer()
	{
		return m_teammateSecretViewer;
	}

	public int GetCardsInHand()
	{
		if (m_teammateHandViewer != null)
		{
			return m_teammateHandViewer.GetCardsInHand();
		}
		return 0;
	}

	public void SetEnemyEmoteHandler(EnemyEmoteHandler enemyEmoteHandler)
	{
		m_enemyEmoteHandlerSwap = new SwapableGameObject(enemyEmoteHandler, m_teammateBoardPos);
	}

	public void SetDuosPortal(DuosPortal duosPortal)
	{
		m_duosPortal = duosPortal;
		m_duosPortalSwap = new SwapableGameObject(m_duosPortal, m_teammateBoardPos);
		m_teammateViewers.ForEach(delegate(TeammateViewer viewer)
		{
			viewer.SetDuosPortal(duosPortal);
		});
	}

	public void PingPortal(Entity entity, int pingType)
	{
		if (entity != null && !(entity.GetCard() == null))
		{
			PingPortal(entity.GetCard().GetActor(), pingType);
		}
	}

	public void PingPortal(Actor actor, int pingType)
	{
		if (!(m_duosPortal == null) && !IsViewingTeammate())
		{
			m_duosPortal.PingPortal(actor, pingType);
		}
	}

	public void RemovePortalPingIfInteriorIsActor(Actor actor)
	{
		if (m_duosPortal != null)
		{
			m_duosPortal.RemovePingIfInteriorIsActor(actor);
		}
	}

	public Vector3 GetPortalOriginalPosition()
	{
		return m_duosPortalSwap.GetOriginalPosition();
	}

	public Vector3 GetTeammateBoardPosition()
	{
		return m_teammateBoardPos;
	}

	public TagMap GetTeammatePlayerTags()
	{
		return m_teammatePlayerTags;
	}
}
