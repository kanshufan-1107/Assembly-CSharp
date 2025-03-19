using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using PegasusGame;
using UnityEngine;

public class PlayerLeaderboardCard : HistoryItem
{
	public PlayerLeaderboardPlayerOverlay m_overlay;

	public GameObject m_IconSwords;

	public MeshRenderer m_portraitMesh;

	public MeshRenderer m_innerFrameMesh;

	public bool m_portraitSharesFrameMesh;

	public GameObject m_portraitOverlay;

	private Entity m_playerHeroEntity;

	public PlayerLeaderboardTeam m_parent;

	private Material m_fullTileMaterial;

	private bool m_mousedOver;

	private bool m_hasBeenShown;

	private bool m_isShowingDiabloPlayerFx;

	private bool m_gameEntityMousedOver;

	private bool m_isNextOpponent;

	public PlayerLeaderboardPlayerOverlay Overlay => m_overlay;

	public Entity Entity => m_playerHeroEntity;

	public PlayerLeaderboardTeam Team => m_parent;

	public void Initialize(HistoryTileInitInfo initInfo, PlayerLeaderboardTeam team)
	{
		m_parent = team;
		m_playerHeroEntity = initInfo.m_entity;
		RefreshTileVisuals(initInfo);
		RefreshOverlay();
	}

	public void ReplaceHero(Entity playerHeroEntity, HistoryTileInitInfo initInfo)
	{
		m_playerHeroEntity = playerHeroEntity;
		RefreshTileVisuals(initInfo);
		RefreshOverlay();
		m_parent.UpdateTeammateOverlays();
	}

	public bool HasBeenShown()
	{
		return m_hasBeenShown;
	}

	public void MarkAsShown()
	{
		if (!m_hasBeenShown)
		{
			m_hasBeenShown = true;
		}
	}

	public void SetTechLevelDirty()
	{
		Overlay.SetTechLevelDirty();
	}

	public void SetTriplesDirty()
	{
		Overlay.SetTriplesDirty();
	}

	public void SetBattlegroundHeroBuddyEnabledDirty()
	{
		Overlay.SetBattlegroundHeroBuddyEnabledDirty();
	}

	public void SetBGQuestRewardDirty()
	{
		Overlay.SetBGQuestRewardDirty();
	}

	public void UpdateTrinketTags()
	{
		Overlay.UpdateTrinketTags();
	}

	public void SetBGTrinketDirty()
	{
		Overlay.SetBGTrinketDirty();
	}

	public void SetRacesDirty()
	{
		Overlay.SetRacesDirty();
	}

	public void SetRecentCombatsDirty()
	{
		Overlay.SetRecentCombatsDirty();
	}

	public void SetHeroPower(Entity hero)
	{
		Overlay.SetHeroPower(hero);
	}

	public void UpdateRacesCount(List<GameRealTimeRaceCount> races)
	{
		Overlay.UpdateRacesCount(races);
	}

	public void RefreshRecentCombats()
	{
		Overlay.RefreshRecentCombats();
	}

	public void RefreshOverlay()
	{
		Overlay.SetupHeroBuddy();
		Overlay.RefreshMainCardActor();
	}

	public void SetSwordsIconActive(bool active)
	{
		if (!(m_IconSwords == null))
		{
			m_IconSwords.SetActive(active);
		}
	}

	public void RefreshTileVisuals(HistoryTileInitInfo info)
	{
		m_entity = info.m_entity;
		m_portraitTexture = info.m_portraitTexture;
		m_portraitGoldenMaterial = info.m_portraitGoldenMaterial;
		m_fullTileMaterial = info.m_fullTileMaterial;
		SetCardDef(info.m_cardDef);
		m_splatAmount = info.m_splatAmount;
		m_dead = info.m_dead;
		RefreshTileVisuals();
	}

	public void PauseHealthUpdates()
	{
		Overlay.PauseHealthUpdates();
	}

	public void UpdateDiabloPlayerFightFX(int oldValue = -1, int newValue = -1)
	{
		if (newValue == -1)
		{
			newValue = GameState.Get().GetGameEntity().GetTag(GAME_TAG.BACON_DIABLO_FIGHT_DIABLO_PLAYER_ID);
		}
		if (oldValue == newValue)
		{
			return;
		}
		int myPlayerID = GameState.Get().GetFriendlyPlayerId();
		bool showDiablePlayerFightFX = false;
		if (Entity.GetCurrentHealth() > 0)
		{
			if (myPlayerID != newValue && newValue == Entity.GetTag(GAME_TAG.PLAYER_ID))
			{
				showDiablePlayerFightFX = true;
			}
			else if (myPlayerID == newValue && newValue != Entity.GetTag(GAME_TAG.PLAYER_ID))
			{
				showDiablePlayerFightFX = true;
			}
		}
		if (showDiablePlayerFightFX)
		{
			Card playerHeroCard = Entity.GetCard();
			if (!(playerHeroCard == null))
			{
				Spell diabloPlayerSpell = m_tileActor.GetSpell(SpellType.BACON_DIABLO_PLAYER);
				diabloPlayerSpell.RemoveAllTargets();
				diabloPlayerSpell.AddTarget(playerHeroCard.gameObject);
				diabloPlayerSpell.SetSource(playerHeroCard.gameObject);
				Player friendlyPlayer = GameState.Get().GetFriendlySidePlayer();
				if (friendlyPlayer != null && friendlyPlayer.GetHero() != null && friendlyPlayer.GetHero().GetCard() != null)
				{
					diabloPlayerSpell.SetSource(friendlyPlayer.GetHero().GetCard().gameObject);
				}
				if (!m_isShowingDiabloPlayerFx)
				{
					diabloPlayerSpell.ChangeState(SpellStateType.BIRTH);
					m_isShowingDiabloPlayerFx = true;
				}
				else
				{
					diabloPlayerSpell.ActivateState(SpellStateType.ACTION);
				}
			}
		}
		else if (!showDiablePlayerFightFX && m_isShowingDiabloPlayerFx)
		{
			m_tileActor.ActivateSpellDeathState(SpellType.BACON_DIABLO_PLAYER);
			m_isShowingDiabloPlayerFx = false;
		}
	}

	public bool GetNextOpponentState()
	{
		return m_isNextOpponent;
	}

	public void SetBorderColor(bool isEnemy)
	{
		if (m_innerFrameMesh != null)
		{
			m_innerFrameMesh.GetMaterial().color = (isEnemy ? Team.m_enemyBorderColor : Team.m_selfBorderColor);
		}
	}

	public void RefreshTileVisuals()
	{
		if (m_portraitMesh == null)
		{
			return;
		}
		int friendlyPlayerId = GameState.Get().GetFriendlySidePlayer().GetPlayerId();
		int teammateId = GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.BACON_DUO_TEAMMATE_PLAYER_ID);
		int tilePlayerId = m_playerHeroEntity.GetTag(GAME_TAG.PLAYER_ID);
		if (tilePlayerId == 0)
		{
			tilePlayerId = m_playerHeroEntity.GetTag(GAME_TAG.CONTROLLER);
		}
		if (m_portraitSharesFrameMesh)
		{
			Material[] newMaterials = new Material[2]
			{
				m_portraitMesh.GetMaterial(),
				null
			};
			if (m_fullTileMaterial != null)
			{
				newMaterials[1] = m_fullTileMaterial;
				m_portraitMesh.SetMaterials(newMaterials);
			}
			else
			{
				m_portraitMesh.GetMaterial(1).mainTexture = m_portraitTexture;
			}
			Renderer[] componentsInChildren = m_portraitMesh.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				if (!renderer.CompareTag(HistoryItem.RENDERER_TAG))
				{
					renderer.GetMaterial().color = Board.Get().m_HistoryTileColor;
				}
			}
			m_portraitMesh.GetMaterial(1).color = Board.Get().m_HistoryTileColor;
		}
		else
		{
			if (m_fullTileMaterial != null)
			{
				RendererExtension.SetMaterials(materials: new Material[1] { m_fullTileMaterial }, renderer: m_portraitMesh);
			}
			else
			{
				m_portraitMesh.GetMaterial().mainTexture = m_portraitTexture;
			}
			if (m_innerFrameMesh != null)
			{
				m_innerFrameMesh.GetMaterial().color = Board.Get().m_HistoryTileColor;
			}
		}
		SetBorderColor(tilePlayerId != friendlyPlayerId && tilePlayerId != teammateId);
	}

	public void DarkenDeadHeroPortrait()
	{
		Material material = null;
		material = ((!m_portraitSharesFrameMesh) ? m_portraitMesh.GetMaterial() : m_portraitMesh.GetMaterial(1));
		if (material != null)
		{
			material.color = Team.m_deadColor;
		}
	}

	public bool IsMousedOver()
	{
		return m_mousedOver;
	}

	public void NotifyMousedOver()
	{
		if (!m_mousedOver)
		{
			m_mousedOver = true;
			Overlay.ShowOverlay();
		}
	}

	public void NotifyMousedOut()
	{
		if (m_mousedOver)
		{
			m_mousedOver = false;
			if (m_gameEntityMousedOver)
			{
				GameState.Get().GetGameEntity().NotifyOfHistoryTokenMousedOut();
				m_gameEntityMousedOver = false;
			}
			Overlay.HideOverlay();
			TooltipPanelManager.Get().HideKeywordHelp();
		}
	}

	public void RefreshMainCardActor()
	{
		Overlay.RefreshMainCardActor();
	}

	public string GetHeroName()
	{
		if (Entity != null)
		{
			return Entity.GetName();
		}
		return null;
	}

	public bool UpdateRecentCombats()
	{
		return Overlay.UpdateRecentCombats();
	}

	public float GetPoppedOutBoneX()
	{
		GameObject bone = m_tileActor.FindBone("PoppedOutBone");
		if (bone == null)
		{
			return 0f;
		}
		return bone.transform.localPosition.x;
	}
}
