using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Hearthstone.UI;
using PegasusGame;
using UnityEngine;

public class PlayerLeaderboardPlayerOverlay : MonoBehaviour
{
	private const string HISTORY_MOUSEOVER_AUDIO_PREFAB = "history_event_mouseover.prefab:0bc4f1638257a264a9b02e811c0a61b5";

	private const string CARD_PREFAB = "Card_Hand_Ability.prefab:3c3f5189f0d0b3745a1c1ca21d41efe0";

	private const string RECENT_COMBAT_PANEL_PREFAB_MERGED = "PlayerLeaderBoardRecentActionsPanelWidget.prefab:83cf143fe4ebe7e4eb5c5a79341c280f";

	private const string HERO_POWER_ACTOR_BONE_NAME = "HeroPowerActorBone";

	private const string HERO_POWER_QUEST_REWARD_ACTOR_BONE_NAME = "HeroPowerActorBone";

	private const string CUSTOM_CARD_ACTOR_BONE_NAME = "CustomCardActorBone";

	private const string SECOND_CUSTOM_CARD_ACTOR_BONE_NAME = "SecondCustomCardActorBone";

	private const string HERO_BUDDY_ACTOR_BONE_NAME = "CustomCardActorBone";

	private const string QUEST_REWARD_ACTOR_BONE_NAME = "CustomCardActorBone";

	private const string HEROPOWER_TRINKET_ACTOR_BONE_NAME = "HeroPowerActorBone";

	private const string DUOS_BONE_SUFFIX = "Duos";

	private const string MAIN_ACTOR_BONE_NAME = "MainActorBone";

	private const string HISTORY_ACTOR_BONE_NAME = "HistoryPanelBone";

	private const string HISTORY_ACTOR_WITH_CUSTOM_CARD_BONE_NAME = "HistoryPanelBoneWithCustomCard";

	private const string HERO_NAME_ACTOR_BONE_NAME = "heroNameTextBone";

	private const string ARMOR_ACTOR_BONE_NAME = "ArmorBone";

	private const string HEALTH_ACTOR_BONE_NAME = "HealthBone";

	private const string TEAMMATE_ACTOR_BONE_NAME = "TeammateActorBone";

	private readonly PlatformDependentValue<string> PLATFORM_DEPENDENT_BONE_SUFFIX = new PlatformDependentValue<string>(PlatformCategory.Screen)
	{
		PC = "PC",
		Tablet = "Tablet",
		Phone = "Phone"
	};

	private const string BG_REWARD_VFX = "BGRewardVFX";

	private const string BG_BUDDY_VFX = "BGRewardVFX";

	private const string BG_SECOND_BUDDY = "SecondBuddy";

	private readonly Vector3 BG_SECOND_BUDDY_HIDE_SCALE = new Vector3(0.01f, 0.01f, 0.01f);

	private readonly Vector3 BG_SECOND_BUDDY_SHOW_SCALE = new Vector3(1f, 1f, 1f);

	private const float RECENT_ACTION_PANEL_SCALE = 0.35f;

	[SerializeField]
	private PlayerLeaderboardCard m_owner;

	[SerializeField]
	private Actor m_boneContainerActor;

	private Actor m_heroActor;

	private Actor m_heroPowerActor;

	private Actor m_heroBuddyActor;

	private Actor m_heroPowerQuestRewardActor;

	private Actor m_questRewardActor;

	private Actor m_heroTeammateActor;

	private Actor m_trinketActor1;

	private Actor m_trinketActor2;

	private Actor m_trinketActorHeroPower;

	private VisualController m_recentCombatsPanelController;

	public GameObject m_recentCombatsPanelInUse;

	private PlayerLeaderboardRecentCombatsPanel m_recentCombatsPanel;

	private List<PlayerLeaderboardInformationPanel> m_additionalInfoPanels;

	private bool m_heroNameInitialized;

	private bool m_techLevelDirty = true;

	private bool m_triplesDirty = true;

	private bool m_racesDirty = true;

	private bool m_recentCombatsDirty = true;

	private bool m_heroBuddyEnabledDirty = true;

	private bool m_questRewardDirty = true;

	private bool m_trinketDirty = true;

	private bool m_useDamageCapPanel;

	private bool m_heroActorInitialized;

	private Map<TAG_RACE, int> m_raceCounts = new Map<TAG_RACE, int>();

	private bool MousedOver => m_owner.IsMousedOver();

	private Entity HeroEntity => m_owner.Entity;

	public void SetTechLevelDirty()
	{
		m_techLevelDirty = true;
	}

	public void SetTriplesDirty()
	{
		m_triplesDirty = true;
	}

	public void SetBattlegroundHeroBuddyEnabledDirty()
	{
		m_heroBuddyEnabledDirty = true;
	}

	public void SetBGQuestRewardDirty()
	{
		m_questRewardDirty = true;
	}

	public void SetBGTrinketDirty()
	{
		m_trinketDirty = true;
	}

	public void SetRacesDirty()
	{
		m_racesDirty = true;
	}

	public void SetRecentCombatsDirty()
	{
		m_recentCombatsDirty = true;
	}

	public void HideMainCardActor()
	{
		if (!(m_heroActor == null))
		{
			m_heroActor.SetVisibility(isVisible: false, isInternal: false);
		}
	}

	public void RefreshMainCardActor()
	{
		LoadMainCardActor();
		if (!(m_heroActor == null))
		{
			m_heroActor.SetCardDefFromEntity(HeroEntity);
			m_heroActor.SetPremium(HeroEntity.GetPremiumType());
			m_heroActor.SetWatermarkCardSetOverride(HeroEntity.GetWatermarkCardSetOverride());
			m_heroActor.SetHistoryItem(m_owner, reparent: false);
			m_heroActor.UpdateAllComponents();
			m_heroActor.GetAttackObject().Hide();
		}
	}

	public void RefreshTeammateActor(PlayerLeaderboardCard teammate)
	{
		LoadTeammateHeroActor(teammate);
		if (!(m_heroTeammateActor == null))
		{
			m_heroTeammateActor.SetCardDefFromEntity(teammate.Entity);
			m_heroTeammateActor.SetPremium(teammate.Entity.GetPremiumType());
			m_heroTeammateActor.SetWatermarkCardSetOverride(teammate.Entity.GetWatermarkCardSetOverride());
			m_heroTeammateActor.SetHistoryItem(teammate, reparent: false);
			m_heroTeammateActor.UpdateAllComponents();
			m_heroTeammateActor.GetAttackObject().Hide();
			m_heroTeammateActor.GetHealthObject().Hide();
			m_heroTeammateActor.m_armorSpellBone.SetActive(value: false);
			m_heroTeammateActor.HideAllText();
		}
	}

	public bool UpdateRecentCombats()
	{
		RefreshRecentCombats();
		return true;
	}

	private bool UpdateTechLevel()
	{
		if (m_recentCombatsPanel == null)
		{
			return false;
		}
		int desiredTechLevel = 1;
		int playerId = HeroEntity.GetTag(GAME_TAG.PLAYER_ID);
		if (GameState.Get().GetPlayerInfoMap().ContainsKey(playerId) && GameState.Get().GetPlayerInfoMap()[playerId].GetPlayerHero() != null)
		{
			desiredTechLevel = GameState.Get().GetPlayerInfoMap()[playerId].GetPlayerHero().GetRealTimePlayerTechLevel();
		}
		desiredTechLevel = Mathf.Clamp(desiredTechLevel, 1, 6);
		m_recentCombatsPanel.SetTechLevel(desiredTechLevel);
		return true;
	}

	private bool UpdateTriples()
	{
		if (m_recentCombatsPanel == null)
		{
			return false;
		}
		int triples = 0;
		int playerId = HeroEntity.GetTag(GAME_TAG.PLAYER_ID);
		if (GameState.Get().GetPlayerInfoMap().ContainsKey(playerId) && GameState.Get().GetPlayerInfoMap()[playerId].GetPlayerHero() != null)
		{
			triples = GameState.Get().GetPlayerInfoMap()[playerId].GetPlayerHero().GetTag(GAME_TAG.PLAYER_TRIPLES);
		}
		if (m_recentCombatsPanel.GetTripleCount() == triples)
		{
			return false;
		}
		m_recentCombatsPanel.SetTriples(triples);
		return true;
	}

	private void UpdateHeroPower()
	{
		if (!(m_heroPowerActor == null))
		{
			bool hasHeroPowerQuestReward = HeroEntity.GetTag(GAME_TAG.BACON_HERO_HEROPOWER_QUEST_REWARD_DATABASE_ID) != 0;
			bool heroPowerQuestRewardCompleted = HeroEntity.GetTag(GAME_TAG.BACON_HERO_HEROPOWER_QUEST_REWARD_COMPLETED) != 0;
			bool num = m_trinketActorHeroPower != null;
			bool validHeroPower = m_heroPowerActor.GetEntity() != null && m_heroPowerActor.GetEntity().HasTag(GAME_TAG.BACON_DOUBLE_QUEST_HERO_POWER);
			if (!num && (!(hasHeroPowerQuestReward && heroPowerQuestRewardCompleted) || validHeroPower) && (!hasHeroPowerQuestReward || !HasCustomCardActor()))
			{
				m_heroPowerActor.gameObject.SetActive(value: true);
				m_heroPowerActor.Show();
			}
			else
			{
				m_heroPowerActor.Hide();
				m_heroPowerActor.gameObject.SetActive(value: false);
			}
		}
	}

	private bool UpdateBGQuestRewards()
	{
		if (GameState.Get() == null)
		{
			return false;
		}
		if (HeroEntity == null)
		{
			Debug.LogWarning("UpdateBGQuestRewards - Player Hero Entity is null");
			return false;
		}
		bool hasHeroPowerQuestReward = HeroEntity.GetTag(GAME_TAG.BACON_HERO_HEROPOWER_QUEST_REWARD_DATABASE_ID) != 0;
		bool heroPowerQuestRewardCompleted = HeroEntity.GetTag(GAME_TAG.BACON_HERO_HEROPOWER_QUEST_REWARD_COMPLETED) != 0;
		bool hasQuestReward = HeroEntity.GetTag(GAME_TAG.BACON_HERO_QUEST_REWARD_DATABASE_ID) != 0 && GameState.Get().BattlegroundsAllowQuests();
		bool questRewardCompleted = HeroEntity.GetTag(GAME_TAG.BACON_HERO_QUEST_REWARD_COMPLETED) != 0;
		if (m_heroPowerQuestRewardActor != null)
		{
			m_heroPowerQuestRewardActor.gameObject.SetActive(hasHeroPowerQuestReward);
			m_heroPowerQuestRewardActor.Show();
			GameObjectUtils.FindChild(m_heroPowerQuestRewardActor?.gameObject, "BGRewardVFX")?.gameObject.SetActive(!heroPowerQuestRewardCompleted);
			if (m_heroPowerQuestRewardActor != null && m_heroPowerQuestRewardActor.UseCoinManaGem())
			{
				m_heroPowerQuestRewardActor.ActivateSpellBirthState(SpellType.COIN_MANA_GEM);
			}
		}
		if (m_questRewardActor != null)
		{
			m_questRewardActor.gameObject.SetActive(hasQuestReward);
			m_questRewardActor.Show();
			GameObjectUtils.FindChild(m_questRewardActor?.gameObject, "BGRewardVFX")?.gameObject.SetActive(!questRewardCompleted);
			if (m_questRewardActor != null && m_questRewardActor.UseCoinManaGem())
			{
				m_questRewardActor.ActivateSpellBirthState(SpellType.COIN_MANA_GEM);
			}
		}
		return true;
	}

	private bool UpdateBGTrinkets()
	{
		if (m_trinketActor1 != null)
		{
			m_trinketActor1.gameObject.SetActive(value: true);
			m_trinketActor1.Show();
		}
		if (m_trinketActor2 != null)
		{
			m_trinketActor2.gameObject.SetActive(value: true);
			m_trinketActor2.Show();
		}
		if (m_trinketActorHeroPower != null)
		{
			m_trinketActorHeroPower.gameObject.SetActive(value: true);
			m_trinketActorHeroPower.Show();
		}
		return true;
	}

	private bool UpdateRaces()
	{
		if (m_recentCombatsPanel == null)
		{
			return false;
		}
		return m_recentCombatsPanel.SetRaces(m_raceCounts);
	}

	private string GetHistoryActorBoneName(bool isDuosGame, bool useDuosAffix = true)
	{
		string duosAffix = ((isDuosGame && useDuosAffix) ? "Duos" : "");
		GameState gameState = GameState.Get();
		if (gameState == null || HeroEntity == null)
		{
			return "HistoryPanelBone" + duosAffix;
		}
		if (gameState.BattlegroundAllowBuddies())
		{
			return "HistoryPanelBoneWithCustomCard" + duosAffix;
		}
		bool num = HeroEntity.GetTag(GAME_TAG.BACON_HERO_HEROPOWER_QUEST_REWARD_DATABASE_ID) != 0;
		bool hasQuest = HeroEntity.GetTag(GAME_TAG.BACON_HERO_QUEST_REWARD_DATABASE_ID) != 0;
		if (!num && !hasQuest)
		{
			return "HistoryPanelBone" + duosAffix;
		}
		return "HistoryPanelBoneWithCustomCard" + duosAffix;
	}

	private bool HasCustomCardActor()
	{
		if (!(m_questRewardActor != null) && !(m_heroBuddyActor != null))
		{
			return m_trinketActor1 != null;
		}
		return true;
	}

	private string GetHeroPowerQuestRewardBoneName()
	{
		if (GameState.Get() != null && HeroEntity != null && !HasCustomCardActor())
		{
			return "CustomCardActorBone";
		}
		return "HeroPowerActorBone";
	}

	private string GetHeroPowerActorBoneName(bool isDuosGame, bool useDuosAffix = true)
	{
		if (!(useDuosAffix && isDuosGame))
		{
			return "HeroPowerActorBone";
		}
		return "HeroPowerActorBoneDuos";
	}

	private string GetHeroActorBoneName(bool isDuosGame = false, bool isLeft = true, bool useDuosAffix = true)
	{
		if (!isDuosGame || !useDuosAffix)
		{
			return "MainActorBone";
		}
		if (!isLeft)
		{
			return "MainActorBoneRightDuos";
		}
		return "MainActorBoneDuos";
	}

	private string GetTeammateHeroActorBoneName(bool isLeft = false)
	{
		if (!isLeft)
		{
			return "TeammateActorBone";
		}
		return "TeammateActorBoneLeft";
	}

	private string GetHealthActorBoneName(bool isLeft = true)
	{
		if (!isLeft)
		{
			return "HealthBoneRight";
		}
		return "HealthBone";
	}

	private string GetArmorActorBoneName(bool isLeft = true)
	{
		if (!isLeft)
		{
			return "ArmorBoneRight";
		}
		return "ArmorBone";
	}

	private string GetNameActorBoneName(bool isLeft = true)
	{
		if (!isLeft)
		{
			return "heroNameTextBoneRight";
		}
		return "heroNameTextBone";
	}

	private string GetCustomCardActorBoneName(bool isDuosGame, bool useDuosAffix = true)
	{
		if (!(useDuosAffix && isDuosGame))
		{
			return "CustomCardActorBone";
		}
		return "CustomCardActorBoneDuos";
	}

	private string GetSecondCustomCardActorBoneName(bool isDuosGame, bool useDuosAffix = true)
	{
		if (!(useDuosAffix && isDuosGame))
		{
			return "SecondCustomCardActorBone";
		}
		return "SecondCustomCardActorBoneDuos";
	}

	private bool ShowOnLeft()
	{
		return HeroEntity.GetRealTimePlayerFightsFirst() != 0;
	}

	private void UpdateHoverStatePosition()
	{
		bool isDUOsGame = GameMgr.Get()?.IsBattlegroundDuoGame() ?? false;
		GameObject mainActorBone = m_boneContainerActor.FindBone(GetHeroActorBoneName(isDUOsGame, ShowOnLeft()) + PLATFORM_DEPENDENT_BONE_SUFFIX);
		GameObject heroPowerActorBone = m_boneContainerActor.FindBone(GetHeroPowerActorBoneName(isDUOsGame) + PLATFORM_DEPENDENT_BONE_SUFFIX);
		GameObject heroBuddyActorBone = m_boneContainerActor.FindBone("CustomCardActorBone" + PLATFORM_DEPENDENT_BONE_SUFFIX);
		GameObject historyActorBone = m_boneContainerActor.FindBone(GetHistoryActorBoneName(isDUOsGame) + PLATFORM_DEPENDENT_BONE_SUFFIX);
		GameObject questRewardActorBone = m_boneContainerActor.FindBone("CustomCardActorBone" + PLATFORM_DEPENDENT_BONE_SUFFIX);
		GameObject heroPowerQuestRewardActorBone = m_boneContainerActor.FindBone(GetHeroPowerQuestRewardBoneName() + PLATFORM_DEPENDENT_BONE_SUFFIX);
		GameObject teammateActorBone = m_boneContainerActor.FindBone(GetTeammateHeroActorBoneName(!ShowOnLeft()) + PLATFORM_DEPENDENT_BONE_SUFFIX);
		GameObject trinketActorBone1 = m_boneContainerActor.FindBone(GetCustomCardActorBoneName(isDUOsGame) + PLATFORM_DEPENDENT_BONE_SUFFIX);
		GameObject trinketActorBone2 = m_boneContainerActor.FindBone(GetSecondCustomCardActorBoneName(isDUOsGame) + PLATFORM_DEPENDENT_BONE_SUFFIX);
		GameObject trinketActorBoneHeropower = m_boneContainerActor.FindBone("HeroPowerActorBone" + PLATFORM_DEPENDENT_BONE_SUFFIX);
		m_heroActor.transform.position = new Vector3(base.transform.position.x + mainActorBone.transform.localPosition.x, base.transform.position.y + mainActorBone.transform.localPosition.y, GetZForThisTilesMouseOverCard(base.transform.position.z, mainActorBone.transform.localPosition.z, PlayerLeaderboardManager.Get().HIGHEST_MAIN_ACTOR_Z_WORLD, PlayerLeaderboardManager.Get().LOWEST_MAIN_ACTOR_Z_WORLD));
		m_heroActor.transform.localScale = mainActorBone.transform.localScale;
		m_heroActor.transform.localRotation = mainActorBone.transform.localRotation;
		PlayerLeaderboardMainCardActor playerLeaderboardHeroActor = m_heroActor.GetComponent<PlayerLeaderboardMainCardActor>();
		if (playerLeaderboardHeroActor != null && playerLeaderboardHeroActor.m_playerLeaderboardShadow != null)
		{
			playerLeaderboardHeroActor.m_playerLeaderboardShadow.SetActive(value: true);
		}
		if (isDUOsGame)
		{
			GameObject healthActorBone = m_boneContainerActor.FindBone(GetHealthActorBoneName(ShowOnLeft()) + PLATFORM_DEPENDENT_BONE_SUFFIX);
			GameObject armorActorBone = m_boneContainerActor.FindBone(GetArmorActorBoneName(ShowOnLeft()) + PLATFORM_DEPENDENT_BONE_SUFFIX);
			GameObject nameActorBone = m_boneContainerActor.FindBone(GetNameActorBoneName(ShowOnLeft()) + PLATFORM_DEPENDENT_BONE_SUFFIX);
			if (m_heroActor.GetHealthObject() != null)
			{
				m_heroActor.GetHealthObject().transform.localPosition = healthActorBone.transform.localPosition;
				m_heroActor.GetHealthObject().transform.localRotation = healthActorBone.transform.localRotation;
			}
			if (m_heroActor.m_armorObject != null)
			{
				m_heroActor.m_armorObject.transform.localPosition = armorActorBone.transform.localPosition;
				m_heroActor.m_armorObject.transform.localRotation = armorActorBone.transform.localRotation;
			}
			m_heroActor.GetComponent<PlayerLeaderboardMainCardActor>();
			if (playerLeaderboardHeroActor != null && playerLeaderboardHeroActor.m_alternateNameText != null)
			{
				playerLeaderboardHeroActor.m_alternateNameText.transform.localPosition = nameActorBone.transform.localPosition;
				playerLeaderboardHeroActor.m_alternateNameText.transform.localRotation = nameActorBone.transform.localRotation;
			}
		}
		if (m_heroPowerActor != null && m_heroPowerActor.IsShown())
		{
			m_heroPowerActor.transform.position = new Vector3(base.transform.position.x + heroPowerActorBone.transform.localPosition.x, base.transform.position.y + heroPowerActorBone.transform.localPosition.y, m_heroActor.transform.position.z + heroPowerActorBone.transform.localPosition.z);
			m_heroPowerActor.transform.localScale = heroPowerActorBone.transform.localScale;
			if (m_heroPowerActor.UseCoinManaGem())
			{
				m_heroPowerActor.ActivateSpellBirthState(SpellType.COIN_MANA_GEM);
			}
		}
		if (m_heroBuddyActor != null && m_heroBuddyActor.IsShown())
		{
			m_heroBuddyActor.transform.position = new Vector3(base.transform.position.x + heroBuddyActorBone.transform.localPosition.x, base.transform.position.y + heroBuddyActorBone.transform.localPosition.y, m_heroActor.transform.position.z + heroBuddyActorBone.transform.localPosition.z);
			m_heroBuddyActor.transform.localScale = heroBuddyActorBone.transform.localScale;
			if (m_heroBuddyActor.UseCoinManaGem())
			{
				m_heroBuddyActor.ActivateSpellBirthState(SpellType.COIN_MANA_GEM);
			}
		}
		if (m_heroPowerQuestRewardActor != null && m_heroPowerQuestRewardActor.IsShown())
		{
			m_heroPowerQuestRewardActor.transform.position = new Vector3(base.transform.position.x + heroPowerQuestRewardActorBone.transform.localPosition.x, base.transform.position.y + heroPowerQuestRewardActorBone.transform.localPosition.y, m_heroActor.transform.position.z + heroPowerQuestRewardActorBone.transform.localPosition.z);
			m_heroPowerQuestRewardActor.transform.localScale = heroPowerQuestRewardActorBone.transform.localScale;
		}
		if (m_questRewardActor != null && m_questRewardActor.IsShown())
		{
			m_questRewardActor.transform.position = new Vector3(base.transform.position.x + questRewardActorBone.transform.localPosition.x, base.transform.position.y + questRewardActorBone.transform.localPosition.y, m_heroActor.transform.position.z + questRewardActorBone.transform.localPosition.z);
			m_questRewardActor.transform.localScale = questRewardActorBone.transform.localScale;
		}
		if (m_recentCombatsPanelInUse != null)
		{
			m_recentCombatsPanelInUse.transform.position = new Vector3(base.transform.position.x + historyActorBone.transform.localPosition.x, base.transform.position.y + historyActorBone.transform.localPosition.y, m_heroActor.transform.position.z + historyActorBone.transform.localPosition.z);
			m_recentCombatsPanelInUse.transform.localScale = historyActorBone.transform.localScale;
		}
		if (m_trinketActor1 != null)
		{
			m_trinketActor1.transform.position = new Vector3(base.transform.position.x + trinketActorBone1.transform.localPosition.x, base.transform.position.y + trinketActorBone1.transform.localPosition.y, m_heroActor.transform.position.z + trinketActorBone1.transform.localPosition.z);
			m_trinketActor1.transform.localScale = trinketActorBone1.transform.localScale;
		}
		if (m_trinketActor2 != null)
		{
			m_trinketActor2.transform.position = new Vector3(base.transform.position.x + trinketActorBone2.transform.localPosition.x, base.transform.position.y + trinketActorBone2.transform.localPosition.y, m_heroActor.transform.position.z + trinketActorBone2.transform.localPosition.z);
			m_trinketActor2.transform.localScale = trinketActorBone2.transform.localScale;
		}
		if (m_trinketActorHeroPower != null)
		{
			m_trinketActorHeroPower.transform.position = new Vector3(base.transform.position.x + trinketActorBoneHeropower.transform.localPosition.x, base.transform.position.y + trinketActorBoneHeropower.transform.localPosition.y, m_heroActor.transform.position.z + trinketActorBoneHeropower.transform.localPosition.z);
			m_trinketActorHeroPower.transform.localScale = trinketActorBoneHeropower.transform.localScale;
		}
		if (!(m_heroTeammateActor != null) || !m_heroTeammateActor.IsShown() || !(m_heroActor != null))
		{
			return;
		}
		m_heroTeammateActor.transform.position = new Vector3(base.transform.position.x + teammateActorBone.transform.localPosition.x, base.transform.position.y + teammateActorBone.transform.localPosition.y, m_heroActor.transform.position.z + teammateActorBone.transform.localPosition.z);
		m_heroTeammateActor.transform.localRotation = teammateActorBone.transform.localRotation;
		m_heroTeammateActor.transform.localScale = teammateActorBone.transform.localScale;
		PlayerLeaderboardMainCardActor actor = m_heroTeammateActor.GetComponent<PlayerLeaderboardMainCardActor>();
		if (actor != null)
		{
			if (actor.m_alternateNameText != null)
			{
				actor.m_alternateNameText.gameObject.SetActive(value: false);
			}
			if (actor.m_playerLeaderboardShadow != null)
			{
				actor.m_playerLeaderboardShadow.SetActive(value: true);
			}
		}
	}

	private float GetZForThisTilesMouseOverCard(float tileZPosition, float desiredZOffset, float globalTop, float globalBottom)
	{
		if (tileZPosition + desiredZOffset > globalTop)
		{
			return globalTop;
		}
		if (tileZPosition + desiredZOffset < globalBottom)
		{
			return globalBottom;
		}
		return tileZPosition + desiredZOffset;
	}

	public void ShowOverlay()
	{
		if (PlayerLeaderboardManager.Get().IsNewlyMousedOver())
		{
			SoundManager.Get().LoadAndPlay("history_event_mouseover.prefab:0bc4f1638257a264a9b02e811c0a61b5", m_owner.m_tileActor.gameObject);
		}
		UpdateDamageCapState();
		m_heroActor.Show();
		if (m_heroTeammateActor != null)
		{
			m_heroTeammateActor.Show();
		}
		if (GameState.Get().GetGameEntity() is TB_BaconShop_Tutorial)
		{
			if (m_recentCombatsPanelInUse != null)
			{
				m_recentCombatsPanelInUse.gameObject.SetActive(value: false);
			}
			if (m_heroPowerActor != null)
			{
				m_heroPowerActor.Hide();
				m_heroPowerActor.gameObject.SetActive(value: false);
				m_heroPowerActor.UseCoinManaGem();
			}
			if (m_heroBuddyActor != null)
			{
				m_heroBuddyActor.gameObject.SetActive(value: false);
			}
		}
		else
		{
			UpdateHeroPower();
			if (m_heroBuddyEnabledDirty)
			{
				SetupHeroBuddy();
			}
			if (m_questRewardDirty)
			{
				m_questRewardDirty = !SetupBGQuestRewardCards();
			}
			if (m_trinketDirty)
			{
				m_trinketDirty = !SetupBGTrinket();
			}
			UpdateBGQuestRewards();
			UpdateBGTrinkets();
			if (m_heroBuddyActor != null)
			{
				m_heroBuddyActor.gameObject.SetActive(value: true);
				m_heroBuddyActor.Show();
				GameObject buddyNotEarnedVFX = GameObjectUtils.FindChild(m_heroBuddyActor?.gameObject, "BGRewardVFX");
				int buddyGained = HeroEntity.GetTag(GAME_TAG.BACON_PLAYER_NUM_HERO_BUDDIES_GAINED);
				if (buddyNotEarnedVFX != null)
				{
					buddyNotEarnedVFX.gameObject.SetActive(buddyGained == 0);
				}
				else
				{
					Debug.LogWarning("BG Buddy Reward VFX object is missing from card_hand_ally");
				}
				GameObject secondBuddy = GameObjectUtils.FindChildBySubstring(m_heroBuddyActor.gameObject, "SecondBuddy");
				if (secondBuddy != null)
				{
					secondBuddy.gameObject.transform.localScale = ((buddyGained >= 2) ? BG_SECOND_BUDDY_SHOW_SCALE : BG_SECOND_BUDDY_HIDE_SCALE);
				}
				else
				{
					Debug.LogWarning("BG Second Buddy object is missing from card_hand_ally");
				}
				Spell techLevelSpell = m_heroBuddyActor.GetSpell(SpellType.TECH_LEVEL_MANA_GEM);
				if (techLevelSpell != null)
				{
					techLevelSpell.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmInt("TechLevel").Value = m_heroBuddyActor.GetEntityDef().GetTechLevel();
					techLevelSpell.ActivateState(SpellStateType.BIRTH);
				}
			}
			if (m_recentCombatsPanelInUse != null)
			{
				m_recentCombatsPanelInUse.gameObject.SetActive(value: true);
			}
		}
		InitializeMainCardActor();
		m_heroActor.SetActorState(ActorStateType.CARD_IDLE);
		if (!m_heroNameInitialized)
		{
			m_heroNameInitialized = !RefreshMainCardName();
		}
		UpdateHoverStatePosition();
		if (m_recentCombatsDirty)
		{
			m_recentCombatsDirty = !UpdateRecentCombats();
		}
		if (m_techLevelDirty)
		{
			m_techLevelDirty = !UpdateTechLevel();
		}
		if (m_triplesDirty)
		{
			m_triplesDirty = !UpdateTriples();
		}
		if (m_racesDirty)
		{
			m_racesDirty = !UpdateRaces();
		}
	}

	public void InitializeMainCardActor()
	{
		if (!m_heroActorInitialized)
		{
			m_heroActor.TurnOffCollider();
			m_heroActor.SetActorState(ActorStateType.CARD_HISTORY);
			m_heroActorInitialized = true;
		}
	}

	public void LoadTeammateHeroActor(PlayerLeaderboardCard teammate)
	{
		if (m_heroTeammateActor != null)
		{
			return;
		}
		string actorPath = "Bacon_Leaderboard_Hero.prefab:776977f5238a24647adcd67933f7d4b0";
		GameObject actorObject = AssetLoader.Get().InstantiatePrefab(actorPath, AssetLoadingOptions.IgnorePrefabPosition);
		if (actorObject == null)
		{
			Debug.LogWarningFormat("PlayerLeaderboardPlayerOverlay.LoadTeammateHeroActor() - FAILED to load actor \"{0}\"", actorPath);
			return;
		}
		actorObject.transform.SetParent(base.transform);
		Actor actor = actorObject.GetComponent<Actor>();
		if (actor == null)
		{
			Debug.LogWarningFormat("PlayerLeaderboardPlayerOverlay.LoadTeammateHeroActor() - ERROR actor \"{0}\" has no Actor component", actorPath);
			return;
		}
		m_heroTeammateActor = actor;
		RefreshTeammateActor(teammate);
		LayerUtils.SetLayer(m_heroTeammateActor, GameLayer.Tooltip);
		if (!MousedOver)
		{
			HideOverlay();
		}
	}

	public void LoadMainCardActor()
	{
		if (m_heroActor != null)
		{
			return;
		}
		string actorPath = "Bacon_Leaderboard_Hero.prefab:776977f5238a24647adcd67933f7d4b0";
		string heroPowerActorPath = "History_HeroPower.prefab:e73edf8ccea2b11429093f7a448eef53";
		GameObject actorObject = AssetLoader.Get().InstantiatePrefab(actorPath, AssetLoadingOptions.IgnorePrefabPosition);
		GameObject heroPowerActorObject = AssetLoader.Get().InstantiatePrefab(heroPowerActorPath, AssetLoadingOptions.IgnorePrefabPosition);
		if (actorObject == null)
		{
			Debug.LogWarningFormat("PlayerLeaderboardPlayerOverlay.LoadMainCardActor() - FAILED to load actor \"{0}\"", actorPath);
			return;
		}
		actorObject.transform.SetParent(base.transform);
		heroPowerActorObject.transform.SetParent(base.transform);
		Actor actor = actorObject.GetComponent<Actor>();
		Actor heroPowerActor = heroPowerActorObject.GetComponent<Actor>();
		if (actor == null)
		{
			Debug.LogWarningFormat("PlayerLeaderboardPlayerOverlay.LoadMainCardActor() - ERROR actor \"{0}\" has no Actor component", actorPath);
			return;
		}
		m_heroActor = actor;
		m_heroPowerActor = heroPowerActor;
		RefreshMainCardActor();
		LayerUtils.SetLayer(m_heroActor, GameLayer.Tooltip);
		if (heroPowerActor != null)
		{
			LayerUtils.SetLayer(heroPowerActor, GameLayer.Tooltip);
			heroPowerActor.Hide();
			SetHeroPower(HeroEntity);
		}
		SetupHeroBuddy();
		m_questRewardDirty = !SetupBGQuestRewardCards();
		m_trinketDirty = !SetupBGTrinket();
		if (!MousedOver)
		{
			HideOverlay();
		}
	}

	public IEnumerator WaitForHeroToMoveToPlayZoneAndFinishHeroPowerSetup(Entity hero)
	{
		Player player = hero.GetController();
		while (player.GetHero() != hero)
		{
			yield return null;
		}
		SetHeroPower(hero);
	}

	public IEnumerator WaitForPlayerHeroPowerToExist(Player player)
	{
		while (player.GetHeroPower() == null)
		{
			yield return null;
		}
		Entity heroPower = player.GetHeroPower();
		using DefLoader.DisposableCardDef cardDef = heroPower.ShareDisposableCardDef();
		SetHeroPower(heroPower, cardDef, heroPower.GetEntityDef());
	}

	public void SetHeroPower(Entity hero)
	{
		if (m_heroPowerActor == null)
		{
			return;
		}
		if (hero.GetZone() == TAG_ZONE.HAND)
		{
			StartCoroutine(WaitForHeroToMoveToPlayZoneAndFinishHeroPowerSetup(hero));
			return;
		}
		Player player = hero.GetController();
		if (player.GetHero() == hero && player.GetTag(GAME_TAG.PLAYER_ID) == hero.GetTag(GAME_TAG.PLAYER_ID))
		{
			StartCoroutine(WaitForPlayerHeroPowerToExist(player));
			return;
		}
		Entity heroPower = null;
		if (hero.HasTag(GAME_TAG.HERO_POWER_ENTITY))
		{
			heroPower = GameState.Get().GetEntity(hero.GetTag(GAME_TAG.HERO_POWER_ENTITY));
		}
		if (heroPower != null)
		{
			using (DefLoader.DisposableCardDef cardDef = heroPower.ShareDisposableCardDef())
			{
				SetHeroPower(heroPower, cardDef, heroPower.GetEntityDef());
				return;
			}
		}
		string heroPowerId = GameUtils.GetHeroPowerCardIdFromHero(hero.GetCardId());
		if (hero.HasTag(GAME_TAG.HERO_POWER))
		{
			heroPowerId = GameUtils.TranslateDbIdToCardId(hero.GetTag(GAME_TAG.HERO_POWER));
		}
		if (heroPowerId == null)
		{
			return;
		}
		using DefLoader.DisposableFullDef heroPowerDef = DefLoader.Get().GetFullDef(heroPowerId);
		SetHeroPower(null, heroPowerDef?.DisposableCardDef, heroPowerDef?.EntityDef);
	}

	private void SetupActor(Actor actor, Entity entity, DefLoader.DisposableCardDef cardDef, DefLoader.DisposableFullDef fullDef, EntityDef entityDef, int rewardMinionType = 0, int rewardCardDBID = 0)
	{
		if (actor == null)
		{
			return;
		}
		if (fullDef != null)
		{
			actor.SetFullDef(fullDef);
		}
		if (entity == null)
		{
			actor.SetEntityDef(entityDef);
		}
		else
		{
			actor.SetEntityDef(null);
		}
		actor.SetEntity(entity);
		if (cardDef != null)
		{
			actor.SetCardDef(cardDef);
		}
		if (entity == null)
		{
			actor.SetPremium(HeroEntity.GetPremiumType());
		}
		actor.transform.parent = base.transform;
		TransformUtil.Identity(actor.transform);
		if (rewardMinionType != 0)
		{
			string text = CardTextBuilder.GetDefaultCardTextInHand(fullDef.EntityDef);
			text = string.Format(text, GameStrings.GetRaceNameBattlegrounds((TAG_RACE)rewardMinionType));
			actor.SetCardDefPowerTextOverride(text);
		}
		if (rewardCardDBID != 0)
		{
			string text2 = fullDef.EntityDef.GetCardTextInHand();
			CardDbfRecord cardRecord = GameDbf.Card.GetRecord(rewardCardDBID);
			if (cardRecord != null)
			{
				string name = cardRecord.Name;
				text2 = string.Format(text2, name);
				actor.SetCardDefPowerTextOverride(text2);
			}
		}
		if (actor.UseCoinManaGem())
		{
			actor.ActivateSpellBirthState(SpellType.COIN_MANA_GEM);
		}
		actor.UpdateAllComponents();
		if (MousedOver)
		{
			UpdateHoverStatePosition();
		}
		LayerUtils.SetLayer(actor, GameLayer.Tooltip);
	}

	private bool InitQuestRewardActor(ref Actor actor, int cardId, int rewardMinionType = 0, int rewardCardDBID = 0)
	{
		if (cardId != 0)
		{
			using (DefLoader.DisposableFullDef fullDef = DefLoader.Get().GetFullDef(cardId))
			{
				if (fullDef == null || fullDef.EntityDef == null || fullDef.CardDef == null)
				{
					Log.Spells.PrintError("PlayerLeaderboardCard.LoadMainCardActor(): Unable to load def for card ID {0}.", cardId);
					return false;
				}
				GameObject questRewardActorGO = AssetLoader.Get().InstantiatePrefab("Card_Hand_Ability.prefab:3c3f5189f0d0b3745a1c1ca21d41efe0", AssetLoadingOptions.IgnorePrefabPosition);
				if (questRewardActorGO == null)
				{
					Log.Spells.PrintError("PlayerLeaderboardCard.LoadMainCardActor(): Unable to load Hand Actor for entity def {0}.", fullDef.EntityDef);
					return false;
				}
				actor = questRewardActorGO.GetComponentInChildren<Actor>();
				actor.Hide();
				if (actor != null)
				{
					SetupActor(actor, null, null, fullDef, fullDef.EntityDef, rewardMinionType, rewardCardDBID);
				}
				return true;
			}
		}
		return true;
	}

	private bool SetupBGQuestRewardCards()
	{
		if (m_questRewardActor != null)
		{
			UnityEngine.Object.Destroy(m_questRewardActor.gameObject);
			m_questRewardActor = null;
		}
		if (m_heroPowerQuestRewardActor != null)
		{
			UnityEngine.Object.Destroy(m_heroPowerQuestRewardActor.gameObject);
			m_heroPowerQuestRewardActor = null;
		}
		if (GameState.Get() == null)
		{
			return true;
		}
		if (HeroEntity == null)
		{
			Debug.LogWarning("SetupBGQuestRewardCards - Player Hero Entity is null");
			return false;
		}
		Actor rewardHeroPowerActor = null;
		Actor rewardActor = null;
		int heroPowerQuestRewardCardDatabaseId = HeroEntity.GetTag(GAME_TAG.BACON_HERO_HEROPOWER_QUEST_REWARD_DATABASE_ID);
		int rewardQuestRewardCardDatabaseId = HeroEntity.GetTag(GAME_TAG.BACON_HERO_QUEST_REWARD_DATABASE_ID);
		int rewardMinionType = HeroEntity.GetTag(GAME_TAG.BACON_HERO_REWARD_MINION_TYPE);
		int heroPowerRewardMinionType = HeroEntity.GetTag(GAME_TAG.BACON_HERO_HEROPOWER_REWARD_MINION_TYPE);
		int rewardOfferedRewardCardDatabaseId = HeroEntity.GetTag(GAME_TAG.BACON_HERO_REWARD_CARD_DBID);
		int heroPowerRewardOfferedRewardCardDatabaseId = HeroEntity.GetTag(GAME_TAG.BACON_HERO_HEROPOWER_REWARD_CARD_DBID);
		bool result = InitQuestRewardActor(ref rewardHeroPowerActor, heroPowerQuestRewardCardDatabaseId, heroPowerRewardMinionType, heroPowerRewardOfferedRewardCardDatabaseId) & InitQuestRewardActor(ref rewardActor, rewardQuestRewardCardDatabaseId, rewardMinionType, rewardOfferedRewardCardDatabaseId);
		m_heroPowerQuestRewardActor = rewardHeroPowerActor;
		m_questRewardActor = rewardActor;
		return result;
	}

	private bool SetupBGTrinket()
	{
		if (m_trinketActor1 != null)
		{
			UnityEngine.Object.Destroy(m_trinketActor1.gameObject);
			m_trinketActor1 = null;
		}
		if (m_trinketActor2 != null)
		{
			UnityEngine.Object.Destroy(m_trinketActor2.gameObject);
			m_trinketActor2 = null;
		}
		if (m_trinketActorHeroPower != null)
		{
			UnityEngine.Object.Destroy(m_trinketActorHeroPower.gameObject);
			m_trinketActorHeroPower = null;
		}
		if (GameState.Get() == null)
		{
			return true;
		}
		if (HeroEntity == null)
		{
			Debug.LogWarning("SetupBGTrinket - Player Hero Entity is null");
			return false;
		}
		int firstTrinketDatabaseId = HeroEntity.GetTag(GAME_TAG.BACON_FIRST_TRINKET_DATABASE_ID);
		int secondTrinketDatabaseId = HeroEntity.GetTag(GAME_TAG.BACON_SECOND_TRINKET_DATABASE_ID);
		int heropowerTrinketDatabaseId = HeroEntity.GetTag(GAME_TAG.BACON_HEROPOWER_TRINKET_DATABASE_ID);
		int firstTrinketSDN1 = HeroEntity.GetTag(GAME_TAG.BACON_HERO_FIRST_TRINKET_LEADERBOARD_SDN1);
		int secondTrinketSDN1 = HeroEntity.GetTag(GAME_TAG.BACON_HERO_SECOND_TRINKET_LEADERBOARD_SDN1);
		int heroPowerTrinketSDN1 = HeroEntity.GetTag(GAME_TAG.BACON_HERO_HEROPOWER_TRINKET_LEADERBOARD_SDN1);
		int firstTrinketSDN2 = HeroEntity.GetTag(GAME_TAG.BACON_HERO_FIRST_TRINKET_LEADERBOARD_SDN2);
		int secondTrinketSDN2 = HeroEntity.GetTag(GAME_TAG.BACON_HERO_SECOND_TRINKET_LEADERBOARD_SDN2);
		int heroPowerTrinketSDN2 = HeroEntity.GetTag(GAME_TAG.BACON_HERO_HEROPOWER_TRINKET_LEADERBOARD_SDN2);
		int firstTrinketAltText = HeroEntity.GetTag(GAME_TAG.BACON_HERO_FIRST_TRINKET_LEADERBOARD_ALT_TEXT);
		int secondTrinketAltText = HeroEntity.GetTag(GAME_TAG.BACON_HERO_SECOND_TRINKET_LEADERBOARD_ALT_TEXT);
		int heroPowerTrinketAltText = HeroEntity.GetTag(GAME_TAG.BACON_HERO_HEROPOWER_TRINKET_LEADERBOARD_ALT_TEXT);
		return InitTrinketActor(ref m_trinketActor1, firstTrinketDatabaseId, firstTrinketSDN1, firstTrinketSDN2, firstTrinketAltText) & InitTrinketActor(ref m_trinketActor2, secondTrinketDatabaseId, secondTrinketSDN1, secondTrinketSDN2, secondTrinketAltText) & InitTrinketActor(ref m_trinketActorHeroPower, heropowerTrinketDatabaseId, heroPowerTrinketSDN1, heroPowerTrinketSDN2, heroPowerTrinketAltText);
	}

	public void UpdateTrinketTags()
	{
		if (m_trinketActor1 != null)
		{
			m_trinketActor1.GetEntityDef().SetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1, HeroEntity.GetTag(GAME_TAG.BACON_HERO_FIRST_TRINKET_LEADERBOARD_SDN1));
			m_trinketActor1.GetEntityDef().SetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_2, HeroEntity.GetTag(GAME_TAG.BACON_HERO_FIRST_TRINKET_LEADERBOARD_SDN2));
			m_trinketActor1.GetEntityDef().SetTag(GAME_TAG.USE_ALTERNATE_CARD_TEXT, HeroEntity.GetTag(GAME_TAG.BACON_HERO_FIRST_TRINKET_LEADERBOARD_ALT_TEXT));
		}
		if (m_trinketActor2 != null)
		{
			m_trinketActor2.GetEntityDef().SetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1, HeroEntity.GetTag(GAME_TAG.BACON_HERO_SECOND_TRINKET_LEADERBOARD_SDN1));
			m_trinketActor2.GetEntityDef().SetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_2, HeroEntity.GetTag(GAME_TAG.BACON_HERO_SECOND_TRINKET_LEADERBOARD_SDN2));
			m_trinketActor2.GetEntityDef().SetTag(GAME_TAG.USE_ALTERNATE_CARD_TEXT, HeroEntity.GetTag(GAME_TAG.BACON_HERO_SECOND_TRINKET_LEADERBOARD_ALT_TEXT));
		}
		if (m_trinketActorHeroPower != null)
		{
			m_trinketActorHeroPower.GetEntityDef().SetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1, HeroEntity.GetTag(GAME_TAG.BACON_HERO_HEROPOWER_TRINKET_LEADERBOARD_SDN1));
			m_trinketActorHeroPower.GetEntityDef().SetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_2, HeroEntity.GetTag(GAME_TAG.BACON_HERO_HEROPOWER_TRINKET_LEADERBOARD_SDN2));
			m_trinketActorHeroPower.GetEntityDef().SetTag(GAME_TAG.USE_ALTERNATE_CARD_TEXT, HeroEntity.GetTag(GAME_TAG.BACON_HERO_HEROPOWER_TRINKET_LEADERBOARD_ALT_TEXT));
		}
	}

	private bool InitTrinketActor(ref Actor actor, int cardId, int trinketSDN1, int trinketSDN2, int trinketAltText)
	{
		if (cardId == 0)
		{
			return true;
		}
		using DefLoader.DisposableFullDef fullDef = DefLoader.Get().GetFullDef(cardId);
		if (fullDef == null || fullDef.EntityDef == null || fullDef.CardDef == null)
		{
			Log.Spells.PrintError("PlayerLeaderboardCard.InitTrinketActor(): Unable to load def for card ID {0}.", cardId);
			return false;
		}
		GameObject trinketActorGO = AssetLoader.Get().InstantiatePrefab("Card_Hand_BG_Trinket.prefab:3c79ff39d8cdb154f86343c20ebf9c5a", AssetLoadingOptions.IgnorePrefabPosition);
		if (trinketActorGO == null)
		{
			Log.Spells.PrintError("PlayerLeaderboardCard.InitTrinketActor(): Unable to load Hand Actor for entity def {0}.", fullDef.EntityDef);
			return false;
		}
		actor = trinketActorGO.GetComponentInChildren<Actor>();
		actor.Hide();
		if (actor != null)
		{
			EntityDef dynamicEntityDef = fullDef.EntityDef.Clone();
			dynamicEntityDef.SetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1, trinketSDN1);
			dynamicEntityDef.SetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_2, trinketSDN2);
			dynamicEntityDef.SetTag(GAME_TAG.USE_ALTERNATE_CARD_TEXT, trinketAltText);
			SetupActor(actor, null, null, fullDef, dynamicEntityDef);
		}
		return true;
	}

	public void SetupHeroBuddy()
	{
		if (m_heroBuddyActor != null)
		{
			UnityEngine.Object.Destroy(m_heroBuddyActor.gameObject);
			m_heroBuddyActor = null;
		}
		if (GameState.Get() != null && !GameState.Get().BattlegroundAllowBuddies())
		{
			return;
		}
		Actor heroBuddyActor = null;
		int buddyCardID = HeroEntity.GetHeroBuddyCardId();
		if (buddyCardID != 0)
		{
			using DefLoader.DisposableFullDef cardDef = DefLoader.Get().GetFullDef(buddyCardID);
			if (cardDef?.EntityDef == null || cardDef?.CardDef == null)
			{
				Log.Spells.PrintError("PlayerLeaderboardCard.LoadMainCardActor(): Unable to load def for card ID {0}.", buddyCardID);
				return;
			}
			GameObject heroBuddyActorGO = AssetLoader.Get().InstantiatePrefab(ActorNames.GetHandActor(cardDef.EntityDef, HeroEntity.GetPremiumType()), AssetLoadingOptions.IgnorePrefabPosition);
			if (heroBuddyActorGO == null)
			{
				Log.Spells.PrintError("PlayerLeaderboardCard.LoadMainCardActor(): Unable to load Hand Actor for entity def {0}.", cardDef.EntityDef);
				return;
			}
			heroBuddyActor = heroBuddyActorGO.GetComponentInChildren<Actor>();
			if (heroBuddyActor != null)
			{
				LayerUtils.SetLayer(heroBuddyActor, GameLayer.Tooltip);
				heroBuddyActor.SetFullDef(cardDef);
				heroBuddyActor.SetPremium(HeroEntity.GetPremiumType());
				if (heroBuddyActor.UseCoinManaGem())
				{
					heroBuddyActor.ActivateSpellBirthState(SpellType.COIN_MANA_GEM);
				}
				heroBuddyActor.transform.parent = base.transform;
				TransformUtil.Identity(heroBuddyActor.transform);
				heroBuddyActor.UpdateAllComponents();
				if (MousedOver)
				{
					UpdateHoverStatePosition();
				}
				heroBuddyActor.gameObject.SetActive(value: false);
			}
		}
		m_heroBuddyActor = heroBuddyActor;
		m_heroBuddyEnabledDirty = false;
	}

	private void SetHeroPower(Entity entity, DefLoader.DisposableCardDef cardDef, EntityDef entityDef)
	{
		SetupActor(m_heroPowerActor, entity, cardDef, null, entityDef);
	}

	public void PauseHealthUpdates()
	{
		if (m_heroActor is PlayerLeaderboardMainCardActor)
		{
			((PlayerLeaderboardMainCardActor)m_heroActor).PauseHealthUpdates();
		}
	}

	public void ResumeHealthUpdates()
	{
		if (m_heroActor is PlayerLeaderboardMainCardActor)
		{
			((PlayerLeaderboardMainCardActor)m_heroActor).ResumeHealthUpdates();
		}
	}

	public void HideOverlay()
	{
		if ((bool)m_heroActor)
		{
			m_heroActor.ActivateAllSpellsDeathStates();
			m_heroActor.Hide();
		}
		if ((bool)m_heroPowerActor)
		{
			m_heroPowerActor.ActivateAllSpellsDeathStates();
			m_heroPowerActor.Hide();
			m_heroPowerActor.gameObject.SetActive(value: false);
		}
		if ((bool)m_heroBuddyActor)
		{
			m_heroBuddyActor.gameObject.SetActive(value: false);
		}
		if ((bool)m_questRewardActor)
		{
			m_questRewardActor.gameObject.SetActive(value: false);
		}
		if ((bool)m_heroPowerQuestRewardActor)
		{
			m_heroPowerQuestRewardActor.gameObject.SetActive(value: false);
		}
		if ((bool)m_trinketActor1)
		{
			m_trinketActor1.Hide();
			m_trinketActor1.gameObject.SetActive(value: false);
		}
		if ((bool)m_trinketActor2)
		{
			m_trinketActor2.Hide();
			m_trinketActor2.gameObject.SetActive(value: false);
		}
		if ((bool)m_trinketActorHeroPower)
		{
			m_trinketActorHeroPower.Hide();
			m_trinketActorHeroPower.gameObject.SetActive(value: false);
		}
		if (m_recentCombatsPanelInUse != null)
		{
			m_recentCombatsPanelInUse.gameObject.SetActive(value: false);
		}
		if (m_heroTeammateActor != null)
		{
			m_heroTeammateActor.ActivateAllSpellsDeathStates();
			m_heroTeammateActor.Hide();
		}
	}

	public void LoadRecentCombatsPanel()
	{
		string goPath = "PlayerLeaderBoardRecentActionsPanelWidget.prefab:83cf143fe4ebe7e4eb5c5a79341c280f";
		GameObject recentActionsPanelGameObject = AssetLoader.Get().InstantiatePrefab(goPath);
		if (recentActionsPanelGameObject == null)
		{
			Debug.LogWarningFormat("PlayerLeaderboardCard.LoadRecentCombatsPanel() - FAILED to load GameObject \"{0}\"", goPath);
			return;
		}
		for (int i = 0; i < recentActionsPanelGameObject.transform.childCount; i++)
		{
			if (!(m_recentCombatsPanel == null))
			{
				break;
			}
			m_recentCombatsPanel = recentActionsPanelGameObject.transform.GetChild(i).GetComponent<PlayerLeaderboardRecentCombatsPanel>();
		}
		if (m_recentCombatsPanel == null)
		{
			Debug.Log("PlayerLeaderboardCard - LoadRecentCombatsPanel - recentCombatPanel not loaded");
		}
		m_recentCombatsPanelInUse = recentActionsPanelGameObject;
		m_recentCombatsPanelController = recentActionsPanelGameObject.GetComponent<VisualController>();
		if (m_recentCombatsPanelController == null)
		{
			Debug.LogWarningFormat("PlayerLeaderboardCard.LoadRecentCombatsPanel() - FAILED to find Visual Controller");
		}
		else if (m_useDamageCapPanel)
		{
			m_recentCombatsPanelController.SetState("DAMAGE_CAP");
		}
		else
		{
			m_recentCombatsPanelController.SetState("DEFAULT");
		}
		if (m_recentCombatsPanelInUse == null)
		{
			Debug.LogWarningFormat("PlayerLeaderboardCard.LoadRecentCombatsPanel() - ERROR GameObject \"{0}\" has no PlayerLeaderboardRecentCombatsPanel component", goPath);
			return;
		}
		m_recentCombatsPanelInUse.transform.parent = base.transform;
		TransformUtil.Identity(m_recentCombatsPanelInUse.transform);
		TransformUtil.SetLocalScaleX(m_recentCombatsPanelInUse.gameObject, 0.35f);
		TransformUtil.SetLocalScaleY(m_recentCombatsPanelInUse.gameObject, 0.35f);
		TransformUtil.SetLocalScaleZ(m_recentCombatsPanelInUse.gameObject, 0.35f);
	}

	private void UpdateDamageCapState()
	{
		bool prevPannelState = m_useDamageCapPanel;
		m_useDamageCapPanel = GameState.Get().GetGameEntity().GetRealtimeBaconDamageCapEnabled();
		if (m_recentCombatsPanel == null)
		{
			LoadRecentCombatsPanel();
			LayerUtils.SetLayer(m_recentCombatsPanelInUse, GameLayer.Tooltip);
		}
		if (m_useDamageCapPanel != prevPannelState)
		{
			if (m_recentCombatsPanelController != null)
			{
				m_recentCombatsPanelController.SetState(m_useDamageCapPanel ? "DAMAGE_CAP" : "DEFAULT");
			}
			SetTechLevelDirty();
			SetRacesDirty();
			SetRecentCombatsDirty();
			SetTriplesDirty();
		}
	}

	public bool RefreshMainCardName()
	{
		if (m_heroActor == null)
		{
			return false;
		}
		PlayerLeaderboardMainCardActor heroCard = m_heroActor as PlayerLeaderboardMainCardActor;
		int playerId = HeroEntity.GetTag(GAME_TAG.PLAYER_ID);
		heroCard.UpdatePlayerNameText(GameState.Get().GetGameEntity().GetBestNameForPlayer(playerId));
		if (heroCard.GetEntity() != null && !Options.Get().GetBool(Option.STREAMER_MODE))
		{
			heroCard.UpdateAlternateNameText(heroCard.GetEntity().GetName());
		}
		else
		{
			heroCard.SetAlternateNameTextActive(active: false);
		}
		return true;
	}

	internal void UpdateRacesCount(List<GameRealTimeRaceCount> races)
	{
		m_raceCounts.Clear();
		foreach (GameRealTimeRaceCount raceCount in races)
		{
			TAG_RACE race = (TAG_RACE)raceCount.Race;
			m_raceCounts[race] = raceCount.Count;
		}
		m_racesDirty = true;
	}

	public void RefreshRecentCombats()
	{
		if (m_recentCombatsPanel == null)
		{
			return;
		}
		int playerId = HeroEntity.GetTag(GAME_TAG.PLAYER_ID);
		List<PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo> recentCombats = PlayerLeaderboardManager.Get().GetRecentCombatHistoryForPlayer(playerId);
		m_recentCombatsPanel.ClearRecentCombats();
		if (recentCombats != null)
		{
			int num = (int)Math.Max(0L, recentCombats.Count - (m_recentCombatsPanel.m_maxDisplayItems + 1));
			int endIndex = recentCombats.Count;
			for (int i = num; i < endIndex; i++)
			{
				m_recentCombatsPanel.AddRecentCombat(m_owner, recentCombats[i]);
			}
		}
	}

	public void OnDestroy()
	{
		StopAllCoroutines();
	}
}
