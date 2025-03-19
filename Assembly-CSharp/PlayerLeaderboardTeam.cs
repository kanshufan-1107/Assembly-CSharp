using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class PlayerLeaderboardTeam : MonoBehaviour
{
	protected class PlayerData
	{
		public PlayerLeaderboardCard Entry { get; set; }

		public HistoryTileInitInfo TileInfo { get; set; }
	}

	private const string REVEALED_PLAYMAKER_STATE = "Reveal";

	private const string UNREVEALED_PLAYMAKER_STATE = "Unrevealed";

	private const string POP_OUT_PLAYMAKER_STATE = "PopOut";

	private const string POP_IN_PLAYMAKER_STATE = "PopIn";

	[SerializeField]
	protected uint m_TeamSize;

	public Color m_deadColor;

	public Color m_enemyBorderColor;

	public Color m_selfBorderColor;

	[SerializeField]
	private Actor m_boneContainerActor;

	[SerializeField]
	private Collider m_TeamCollider;

	[Header("Teamwide Items")]
	[SerializeField]
	protected ProgressBar m_HealthBar;

	[SerializeField]
	private GameObject m_ArmorBar;

	[SerializeField]
	private GameObject m_IconSwords;

	[SerializeField]
	private GameObject m_IconSkull;

	[SerializeField]
	protected GameObject m_IconFirst;

	[SerializeField]
	protected GameObject m_IconSecond;

	[SerializeField]
	protected PlayMakerFSM m_teamPlaymaker;

	[SerializeField]
	protected MeshRenderer m_frameMesh;

	[SerializeField]
	[Header("Per Player Items")]
	private List<PlayerLeaderboardCard> m_playerLeaderboardCards;

	private bool m_isShowingOddPlayerFx;

	protected List<PlayerData> m_teamMembers = new List<PlayerData>();

	private int m_teamId;

	private bool m_hasBeenShown;

	public int TeamId
	{
		get
		{
			return m_teamId;
		}
		set
		{
			m_teamId = value;
		}
	}

	public List<PlayerLeaderboardCard> Members => m_teamMembers.Select((PlayerData m) => m.Entry).ToList();

	public bool HasBeenShown()
	{
		return m_hasBeenShown;
	}

	public void MarkAsShown()
	{
		if (m_hasBeenShown)
		{
			return;
		}
		m_hasBeenShown = true;
		foreach (PlayerData teamMember in m_teamMembers)
		{
			teamMember.Entry.MarkAsShown();
		}
	}

	protected virtual void OnMemberAdded(PlayerData newMember)
	{
	}

	public virtual void UpdatePlayerOrder(bool animate = true)
	{
	}

	public Collider GetTileCollider()
	{
		return m_TeamCollider;
	}

	public void SetNextOpponentState(bool active, int primaryPlayerId)
	{
		SetTilePopOutActive(active);
		UpdateOddPlayerOutFx(active, primaryPlayerId);
	}

	public void SetTilePopOutActive(bool active)
	{
		PlayMakerFSM playmaker = m_teamPlaymaker;
		if (!(playmaker == null))
		{
			playmaker.SetState(active ? "PopOut" : "PopIn");
		}
	}

	public void UpdateOddPlayerOutFx(bool isNextOpponent, int primaryPlayerId)
	{
		if (!HasBeenShown())
		{
			return;
		}
		bool showOddPlayerFx = isNextOpponent && GameState.Get().GetFriendlySidePlayer().HasTag(GAME_TAG.BACON_ODD_PLAYER_OUT);
		if (showOddPlayerFx && !m_isShowingOddPlayerFx)
		{
			m_isShowingOddPlayerFx = true;
			{
				foreach (PlayerLeaderboardCard playerLeaderboardCard in Members)
				{
					Card playerHeroCard = playerLeaderboardCard.Entity.GetCard();
					if (!(playerHeroCard == null))
					{
						Spell oddPlayerSpell = playerLeaderboardCard.m_tileActor.GetSpell(SpellType.BACON_ODD_PLAYER);
						if (playerLeaderboardCard.Entity.GetTag(GAME_TAG.PLAYER_ID) != primaryPlayerId)
						{
							oddPlayerSpell = playerLeaderboardCard.m_tileActor.GetSpell(SpellType.BACON_ODD_PLAYER_TEAMMATE);
						}
						oddPlayerSpell.AddTarget(playerHeroCard.gameObject);
						oddPlayerSpell.ChangeState(SpellStateType.BIRTH);
					}
				}
				return;
			}
		}
		if (showOddPlayerFx || !m_isShowingOddPlayerFx)
		{
			return;
		}
		foreach (PlayerLeaderboardCard member in Members)
		{
			member.m_tileActor.ActivateSpellDeathState(SpellType.BACON_ODD_PLAYER);
			member.m_tileActor.ActivateSpellDeathState(SpellType.BACON_ODD_PLAYER_TEAMMATE);
		}
		m_isShowingOddPlayerFx = false;
	}

	public void SetRevealed(bool revealed, bool isNextOpponent)
	{
		PlayMakerFSM playmaker = m_teamPlaymaker;
		if (!(playmaker == null))
		{
			playmaker.FsmVariables.GetFsmInt("IsNextOpponent").Value = (isNextOpponent ? 1 : 0);
			playmaker.SetState(revealed ? "Reveal" : "Unrevealed");
		}
	}

	public void AddMember(HistoryTileInitInfo member)
	{
		PlayerData newMember = new PlayerData
		{
			TileInfo = member
		};
		m_teamMembers.Add(newMember);
		InitPlayerData(newMember, m_teamMembers.Count - 1);
		OnMemberAdded(newMember);
	}

	private void InitPlayerData(PlayerData data, int index)
	{
		if (data.TileInfo != null && data.TileInfo.m_entity != null)
		{
			data.Entry = m_playerLeaderboardCards[index];
			data.Entry.Initialize(data.TileInfo, this);
			RefreshTileVisuals();
			UpdateTeammateOverlays();
		}
	}

	public void UpdateTeammateOverlays()
	{
		if (m_teamMembers.Count == 2)
		{
			m_teamMembers[0].Entry.Overlay.RefreshTeammateActor(m_teamMembers[1].Entry);
			m_teamMembers[1].Entry.Overlay.RefreshTeammateActor(m_teamMembers[0].Entry);
		}
	}

	public int GetBestPlacement()
	{
		int best = 8;
		foreach (PlayerData player in m_teamMembers)
		{
			if (player.Entry.Entity.GetRealTimePlayerLeaderboardPlace() < best)
			{
				best = player.Entry.Entity.GetRealTimePlayerLeaderboardPlace();
			}
		}
		return best;
	}

	public bool IsPlayerOnTeam(int playerId)
	{
		foreach (PlayerData teamMember in m_teamMembers)
		{
			int heroPlayerId = teamMember.Entry.Entity.GetTag(GAME_TAG.PLAYER_ID);
			if (heroPlayerId == playerId || heroPlayerId == 0)
			{
				return true;
			}
		}
		return false;
	}

	public void UpdateTileHealth()
	{
		int maxRemainingHealth = 0;
		int maxOriginalHealth = 1;
		int maxArmor = 0;
		foreach (PlayerData teamMember in m_teamMembers)
		{
			PlayerLeaderboardCard card = teamMember.Entry;
			card.Overlay.ResumeHealthUpdates();
			maxRemainingHealth = Mathf.Max(maxRemainingHealth, card.Entity.GetRealTimeRemainingHP());
			maxOriginalHealth = Mathf.Max(maxOriginalHealth, card.Entity.GetHealth());
			if (Mathf.Clamp01((float)card.Entity.GetRealTimeRemainingHP() / (float)card.Entity.GetHealth()) == 0f)
			{
				card.DarkenDeadHeroPortrait();
			}
			maxArmor = Mathf.Max(maxArmor, card.Entity.GetRealTimeArmor());
		}
		float healthRatio = (float)maxRemainingHealth / (float)maxOriginalHealth;
		healthRatio = Mathf.Clamp01(healthRatio);
		SetCurrentHealth(healthRatio);
		int max = PlayerLeaderboardManager.Get().GetBattlegroundsLeaderboardMaxArmor();
		if (max != 0)
		{
			SetCurrentArmor((float)maxArmor / (float)max);
		}
	}

	public void SetSkullEnabled(bool enabled)
	{
		m_IconSkull.SetActive(enabled);
	}

	public void RefreshTileVisuals()
	{
		if (m_frameMesh != null)
		{
			int friendlyPlayerId = GameState.Get().GetFriendlySidePlayer().GetPlayerId();
			m_frameMesh.GetMaterial().color = ((!IsPlayerOnTeam(friendlyPlayerId)) ? m_enemyBorderColor : m_selfBorderColor);
		}
	}

	public virtual void SetCurrentHealth(float healthPercent)
	{
		SetHealthBarActive(healthPercent > 0f);
		SetSkullIconActive(healthPercent == 0f);
		m_HealthBar.SetProgressBar(healthPercent);
	}

	public void SetCurrentArmor(float armorPercent)
	{
		SetArmorBarActive(armorPercent > 0f);
		m_ArmorBar.GetComponent<Renderer>().GetMaterial().SetFloat("_Percent", armorPercent);
	}

	public void SetHealthBarActive(bool active)
	{
		m_HealthBar.gameObject.SetActive(active);
	}

	public void SetArmorBarActive(bool active)
	{
		m_ArmorBar.gameObject.SetActive(active);
	}

	public void SetSwordsIconActive(bool active, int playerId)
	{
		foreach (PlayerLeaderboardCard member in Members)
		{
			member.SetSwordsIconActive(active && member.Entity.GetTag(GAME_TAG.PLAYER_ID) == playerId);
		}
	}

	public void ActivateDizzyEffectOnPlayerTeammate(int playerId)
	{
		foreach (PlayerLeaderboardCard member in Members)
		{
			if (member.Entity.GetTag(GAME_TAG.PLAYER_ID) == playerId)
			{
				continue;
			}
			Card playerHeroCard = member.Entity.GetCard();
			if (!(playerHeroCard == null))
			{
				Spell spell = member.m_tileActor.GetSpell(SpellType.BACON_PLAYER_DIZZY);
				spell.AddTarget(playerHeroCard.gameObject);
				spell.ChangeState(SpellStateType.BIRTH);
				if (member.m_portraitOverlay != null)
				{
					member.m_portraitOverlay.SetActive(value: true);
				}
			}
		}
	}

	public void DeactivateDizzyEffects()
	{
		foreach (PlayerLeaderboardCard member in Members)
		{
			member.m_tileActor.ActivateSpellDeathState(SpellType.BACON_PLAYER_DIZZY);
			if (member.m_portraitOverlay != null)
			{
				member.m_portraitOverlay.SetActive(value: false);
			}
		}
	}

	public void SetSkullIconActive(bool active)
	{
		m_IconSkull.SetActive(active);
	}

	public virtual void SetPlaceIcon(int currentPlace)
	{
	}

	public void SetTileRevealed(bool revealed, bool isNextOpponent)
	{
		PlayMakerFSM playmaker = m_teamPlaymaker;
		if (!(playmaker == null))
		{
			playmaker.FsmVariables.GetFsmInt("IsNextOpponent").Value = (isNextOpponent ? 1 : 0);
			playmaker.SetState(revealed ? "Reveal" : "Unrevealed");
		}
	}
}
