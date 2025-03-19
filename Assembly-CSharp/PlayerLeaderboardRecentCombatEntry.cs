using System;
using System.Collections.Generic;
using Blizzard.T5.AssetManager;
using Blizzard.T5.MaterialService.Extensions;
using Blizzard.T5.Services;
using UnityEngine;

public class PlayerLeaderboardRecentCombatEntry : MonoBehaviour
{
	public enum RecentActionType
	{
		DAMAGE,
		DEATH
	}

	public enum RecentActionTarget
	{
		OWNER,
		OPPONENT,
		TIE
	}

	[Serializable]
	public class TeamEntry
	{
		public List<PlayerDisplay> m_players;

		public GameObject m_iconSkull;
	}

	[Serializable]
	public class PlayerDisplay
	{
		public Actor m_actor;

		public GameObject m_dizzyEffect;
	}

	public TeamEntry m_ownerTeam;

	public TeamEntry m_opponentTeam;

	public TeamEntry m_ownerSolo;

	public TeamEntry m_opponentSolo;

	public GameObject m_iconOwnerSwords;

	public GameObject m_iconOpponentSwords;

	public GameObject m_iconOwnerSplat;

	public GameObject m_iconOpponentSplat;

	public GameObject m_background;

	private RecentActionType m_recentActionType;

	private RecentActionTarget m_recentActionTarget;

	private int m_ownerId;

	private int m_opponentId;

	private int m_ownerTeammateId;

	private int m_opponentTeammateId;

	private int m_splatAmount;

	private PlayerLeaderboardCard m_source;

	private const float TILE_PORTRAIT_MESH_Y_OFFSET = 0.01f;

	private const float TILE_Y_OFFSET = -0.5f;

	private void SetActionTarget(RecentActionTarget target)
	{
		m_recentActionTarget = target;
	}

	private void SetActionType(RecentActionType type)
	{
		m_recentActionType = type;
	}

	private void SetSplatAmount(int splatAmount)
	{
		m_splatAmount = -splatAmount;
	}

	public void Load(PlayerLeaderboardCard source, PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo recentCombatInfo)
	{
		HideAllActors();
		m_source = source;
		m_ownerId = recentCombatInfo.ownerId;
		m_ownerTeammateId = recentCombatInfo.ownerTeammateId;
		m_opponentId = recentCombatInfo.opponentId;
		m_opponentTeammateId = recentCombatInfo.opponentTeammateId;
		SetActionTarget((recentCombatInfo.damageTarget != m_ownerId) ? ((recentCombatInfo.damageTarget == m_opponentId) ? RecentActionTarget.OPPONENT : RecentActionTarget.TIE) : RecentActionTarget.OWNER);
		SetActionType(recentCombatInfo.isDefeated ? RecentActionType.DEATH : RecentActionType.DAMAGE);
		SetSplatAmount(recentCombatInfo.damage);
		int friendlySidePlayerId = GameState.Get().GetFriendlyPlayerId();
		bool isFriendlyOwner = m_ownerId == friendlySidePlayerId || m_ownerTeammateId == friendlySidePlayerId;
		bool isFriendlyOpponent = m_opponentId == friendlySidePlayerId || m_opponentTeammateId == friendlySidePlayerId;
		LoadTileForPlayer(m_ownerId, m_ownerTeammateId, (m_ownerTeammateId == 0) ? m_ownerSolo : m_ownerTeam, recentCombatInfo.ownerIsFirst, recentCombatInfo.friendlyIsDizzy, isFriendlyOwner);
		LoadTileForPlayer(m_opponentId, m_opponentTeammateId, (m_opponentTeammateId == 0) ? m_opponentSolo : m_opponentTeam, recentCombatInfo.opponentIsFirst, recentCombatInfo.enemyIsDizzy, isFriendlyOpponent);
		UpdateDisplay();
		LayerUtils.SetLayer(base.gameObject, GameLayer.Tooltip);
	}

	public void HideAllActors()
	{
		foreach (PlayerDisplay player in m_ownerTeam.m_players)
		{
			player.m_actor.gameObject.SetActive(value: false);
		}
		foreach (PlayerDisplay player2 in m_opponentTeam.m_players)
		{
			player2.m_actor.gameObject.SetActive(value: false);
		}
		foreach (PlayerDisplay player3 in m_opponentSolo.m_players)
		{
			player3.m_actor.gameObject.SetActive(value: false);
		}
		foreach (PlayerDisplay player4 in m_ownerSolo.m_players)
		{
			player4.m_actor.gameObject.SetActive(value: false);
		}
	}

	private void UpdateDisplay()
	{
		bool isSolo = m_opponentTeammateId == 0;
		m_iconOwnerSwords.SetActive(m_recentActionTarget == RecentActionTarget.OPPONENT || m_recentActionTarget == RecentActionTarget.TIE);
		m_iconOpponentSwords.SetActive(m_recentActionTarget == RecentActionTarget.OWNER);
		m_iconOwnerSplat.SetActive(m_recentActionTarget == RecentActionTarget.OPPONENT || m_recentActionTarget == RecentActionTarget.TIE);
		m_iconOpponentSplat.SetActive(m_recentActionTarget == RecentActionTarget.OWNER);
		m_opponentSolo.m_iconSkull.SetActive(isSolo && m_recentActionTarget == RecentActionTarget.OPPONENT && m_recentActionType == RecentActionType.DEATH);
		m_opponentTeam.m_iconSkull.SetActive(!isSolo && m_recentActionTarget == RecentActionTarget.OPPONENT && m_recentActionType == RecentActionType.DEATH);
		m_ownerSolo.m_iconSkull.SetActive(isSolo && m_recentActionTarget == RecentActionTarget.OWNER && m_recentActionType == RecentActionType.DEATH);
		m_ownerTeam.m_iconSkull.SetActive(!isSolo && m_recentActionTarget == RecentActionTarget.OWNER && m_recentActionType == RecentActionType.DEATH);
		GameObject activeSplat = ((m_recentActionTarget == RecentActionTarget.OPPONENT || m_recentActionTarget == RecentActionTarget.TIE) ? m_iconOwnerSplat : m_iconOpponentSplat);
		UpdateSplatSpell(activeSplat);
	}

	private void UpdateSplatSpell(GameObject splatIcon)
	{
		DamageSplatSpell component = splatIcon.GetComponent<DamageSplatSpell>();
		component.SetDamage(-m_splatAmount);
		component.ChangeState(SpellStateType.IDLE);
		component.Show();
	}

	private void LoadTileForPlayer(int playerId, int teammateId, TeamEntry team, bool playerIsFirst, bool firstIsDizzy, bool isFriendly)
	{
		if (!GameState.Get().GetPlayerInfoMap().ContainsKey(playerId))
		{
			Debug.LogWarningFormat("PlayerLeaderboardRecentCombatEntry.LoadTileForPlayer() - FAILED to find playerInfo for playerId \"{0}\"", playerId);
			return;
		}
		Entity playerHeroEntity = GameState.Get().GetPlayerInfoMap()[playerId].GetPlayerHero();
		if (playerHeroEntity == null)
		{
			Debug.LogWarningFormat("PlayerLeaderboardRecentCombatEntry.LoadTileForPlayer() - FAILED to load playerHeroEntity for playerId \"{0}\"", playerId);
			return;
		}
		int index = ((!playerIsFirst) ? 1 : 0);
		LoadCardIntoTileActor(team.m_players[index], playerHeroEntity, isFriendly, playerIsFirst && firstIsDizzy);
		if (teammateId != 0)
		{
			playerHeroEntity = GameState.Get().GetPlayerInfoMap()[teammateId].GetPlayerHero();
			if (playerHeroEntity == null)
			{
				Debug.LogWarningFormat("PlayerLeaderboardRecentCombatEntry.LoadTileForPlayer() - FAILED to load playerHeroEntity for playerId \"{0}\"", teammateId);
			}
			else
			{
				index = (playerIsFirst ? 1 : 0);
				LoadCardIntoTileActor(team.m_players[index], playerHeroEntity, isFriendly, !playerIsFirst && firstIsDizzy);
			}
		}
	}

	private void LoadCardIntoTileActor(PlayerDisplay playerDisplay, Entity hero, bool isFriendly, bool isDizzy)
	{
		DefLoader.DisposableCardDef cardDef = hero.ShareDisposableCardDef();
		if (cardDef == null)
		{
			Debug.LogWarningFormat("PlayerLeaderboardRecentCombatEntry.LoadCardIntoTileActor() - FAILED to load cardDef for playerId \"{0}\"", hero.GetTag(GAME_TAG.PLAYER_ID));
			return;
		}
		Actor targetTile = playerDisplay.m_actor;
		targetTile.gameObject.SetActive(value: true);
		Material[] newMaterials = new Material[2];
		ServiceManager.Get<DisposablesCleaner>()?.Attach(targetTile.gameObject, cardDef);
		newMaterials[0] = targetTile.GetMeshRenderer().GetMaterial();
		TAG_PREMIUM premium = hero.GetPremiumType();
		if (cardDef.CardDef.GetLeaderboardTileFullPortrait() != null)
		{
			newMaterials[1] = cardDef.CardDef.GetLeaderboardTileFullPortrait();
			targetTile.GetMeshRenderer().SetMaterials(newMaterials);
		}
		else if (cardDef.CardDef.TryGetHistoryTileFullPortrait(premium, out newMaterials[1]))
		{
			targetTile.GetMeshRenderer().SetMaterials(newMaterials);
		}
		else
		{
			targetTile.GetMeshRenderer().GetMaterial(1).mainTexture = cardDef.CardDef.GetPortraitTexture(premium);
		}
		Renderer[] componentsInChildren = targetTile.GetMeshRenderer().GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (!(renderer.tag == "FakeShadow"))
			{
				renderer.GetMaterial().color = Board.Get().m_HistoryTileColor;
			}
		}
		targetTile.GetMeshRenderer().GetMaterial(1).color = Board.Get().m_HistoryTileColor;
		if (playerDisplay.m_dizzyEffect != null)
		{
			playerDisplay.m_dizzyEffect.SetActive(isDizzy);
		}
		Color playerColor = (isFriendly ? m_source.Team.m_selfBorderColor : m_source.Team.m_enemyBorderColor);
		SetBorderColor(PlayerIsDead(isFriendly) ? m_source.Team.m_deadColor : playerColor, targetTile);
	}

	private void SetBorderColor(Color color, Actor targetTile)
	{
		targetTile.GetMeshRenderer().GetMaterial().color = color;
	}

	private bool PlayerIsDead(bool isFriendly)
	{
		if (m_recentActionType == RecentActionType.DEATH)
		{
			if (m_recentActionTarget != RecentActionTarget.OPPONENT || isFriendly)
			{
				return m_recentActionTarget == RecentActionTarget.OWNER && isFriendly;
			}
			return true;
		}
		return false;
	}
}
