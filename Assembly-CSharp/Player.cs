using System;
using System.Collections.Generic;
using Blizzard.GameService.SDK.Client.Integration;
using PegasusClient;
using UnityEngine;

public class Player : Entity
{
	public enum Side
	{
		NEUTRAL,
		FRIENDLY,
		OPPOSING,
		TEAMMATE_FRIENDLY,
		TEAMMATE_OPPOSING
	}

	private const string DEFAULT_AI_OPPONENT_NAME = "GAMEPLAY_AI_OPPONENT_NAME";

	private const string POPUP_CONTROLLER_PREFAB = "PopUpProgressBar.prefab:1e74ef51d3388674792ddf7d6233f5d7";

	private BnetGameAccountId m_gameAccountId;

	private bool m_waitingForHeroEntity;

	private string m_name;

	private bool m_local;

	private Side m_side;

	private int m_cardBackId;

	private int m_initialCardBackId;

	private ManaCounter m_manaCounter;

	protected Entity m_hero;

	private Entity m_heroPower;

	private int m_queuedSpentMana;

	private int m_usedTempMana;

	private int m_realtimeTempMana;

	private bool m_realTimeComboActive;

	private bool m_realTimeSpellsCostHealth;

	private MedalInfoTranslator m_medalInfo;

	private uint m_arenaWins;

	private uint m_arenaLoss;

	private uint m_tavernBrawlWins;

	private uint m_tavernBrawlLoss;

	private uint m_duelsWins;

	private uint m_duelsLoss;

	private bool m_concedeEmotePlayed;

	private TAG_PLAYSTATE m_preGameOverPlayState;

	private HashSet<EntityDef> m_seenStartOfGameSpells = new HashSet<EntityDef>();

	private MarkOfEvilCounter m_markOfEvilCounter;

	public static Side GetOppositePlayerSide(Side side)
	{
		return side switch
		{
			Side.FRIENDLY => Side.OPPOSING, 
			Side.OPPOSING => Side.FRIENDLY, 
			_ => side, 
		};
	}

	public void OnShuffleDeck()
	{
		ZoneDeck deck = GetDeckZone();
		if (!(deck == null))
		{
			deck.UpdateLayout();
			Actor deckActor = deck.GetActiveThickness();
			if (!(deckActor == null))
			{
				deckActor.ActivateSpellBirthState(SpellType.SHUFFLE_DECK);
			}
		}
	}

	public void InitPlayer(Network.HistCreateGame.PlayerData netPlayer)
	{
		SetPlayerId(netPlayer.ID);
		SetGameAccountId(netPlayer.GameAccountId);
		SetCardBackId(netPlayer.CardBackID);
		SetTags(netPlayer.Player.Tags);
		InitRealTimeValues(netPlayer.Player.Tags);
		if (IsLocalUser())
		{
			foreach (Network.Entity.Tag tag2 in netPlayer.Player.Tags)
			{
				if (tag2.Name == 1048)
				{
					GameMgr.Get().LastGameData.WhizbangDeckID = tag2.Value;
				}
			}
		}
		if (HasTag(GAME_TAG.CARD_BACK_OVERRIDE))
		{
			SetOverrideCardBackId(GetTag(GAME_TAG.CARD_BACK_OVERRIDE));
		}
		Network.Entity.Tag marksOfEvil = netPlayer.Player.Tags.Find((Network.Entity.Tag tag) => tag.Name == 994);
		if (marksOfEvil != null)
		{
			GetOrCreateMarkOfEvilCounter().OnMarksChanged(marksOfEvil.Value);
		}
		GameState.Get().RegisterTurnChangedListener(OnTurnChanged);
	}

	public override bool HasValidDisplayName()
	{
		return !string.IsNullOrEmpty(m_name);
	}

	public override string GetName()
	{
		return m_name;
	}

	public MedalInfoTranslator GetRank()
	{
		return m_medalInfo;
	}

	public override string GetDebugName()
	{
		if (m_name != null)
		{
			return m_name;
		}
		if (IsAI())
		{
			return GameStrings.Get("GAMEPLAY_AI_OPPONENT_NAME");
		}
		return "UNKNOWN HUMAN PLAYER";
	}

	public void SetGameAccountId(BnetGameAccountId id)
	{
		m_gameAccountId = id;
		UpdateLocal();
		if (IsDisplayable())
		{
			UpdateDisplayInfo();
			return;
		}
		UpdateRank();
		UpdateSessionRecord();
		if (IsBnetPlayer())
		{
			BnetPresenceMgr.Get().AddPlayersChangedListener(OnBnetPlayersChanged);
			if (!BnetFriendMgr.Get().IsFriend(m_gameAccountId))
			{
				GameUtils.RequestPlayerPresence(m_gameAccountId);
			}
		}
	}

	public bool IsLocalUser()
	{
		return m_local;
	}

	public bool IsAI()
	{
		return GameUtils.IsAIPlayer(m_gameAccountId);
	}

	public bool IsHuman()
	{
		return GameUtils.IsHumanPlayer(m_gameAccountId);
	}

	public bool IsBnetPlayer()
	{
		return GameUtils.IsBnetPlayer(m_gameAccountId);
	}

	public Side GetSide()
	{
		return m_side;
	}

	public bool IsFriendlySide()
	{
		return m_side == Side.FRIENDLY;
	}

	public bool IsOpposingSide()
	{
		return m_side == Side.OPPOSING;
	}

	public bool IsSpellpowerTemporary(TAG_SPELL_SCHOOL spellSchool = TAG_SPELL_SCHOOL.NONE)
	{
		int currentSpellpower = GetTag(GAME_TAG.CURRENT_SPELLPOWER_BASE);
		int tempSpellpower = GetTag(GAME_TAG.CURRENT_TEMP_SPELLPOWER_BASE);
		switch (spellSchool)
		{
		case TAG_SPELL_SCHOOL.ARCANE:
			currentSpellpower += GetTag(GAME_TAG.CURRENT_SPELLPOWER_ARCANE);
			tempSpellpower += GetTag(GAME_TAG.CURRENT_TEMP_SPELLPOWER_ARCANE);
			break;
		case TAG_SPELL_SCHOOL.FIRE:
			currentSpellpower += GetTag(GAME_TAG.CURRENT_SPELLPOWER_FIRE);
			tempSpellpower += GetTag(GAME_TAG.CURRENT_TEMP_SPELLPOWER_FIRE);
			break;
		case TAG_SPELL_SCHOOL.FROST:
			currentSpellpower += GetTag(GAME_TAG.CURRENT_SPELLPOWER_FROST);
			tempSpellpower += GetTag(GAME_TAG.CURRENT_TEMP_SPELLPOWER_FROST);
			break;
		case TAG_SPELL_SCHOOL.NATURE:
			currentSpellpower += GetTag(GAME_TAG.CURRENT_SPELLPOWER_NATURE);
			tempSpellpower += GetTag(GAME_TAG.CURRENT_TEMP_SPELLPOWER_NATURE);
			break;
		case TAG_SPELL_SCHOOL.HOLY:
			currentSpellpower += GetTag(GAME_TAG.CURRENT_SPELLPOWER_HOLY);
			tempSpellpower += GetTag(GAME_TAG.CURRENT_TEMP_SPELLPOWER_HOLY);
			break;
		case TAG_SPELL_SCHOOL.SHADOW:
			currentSpellpower += GetTag(GAME_TAG.CURRENT_SPELLPOWER_SHADOW);
			tempSpellpower += GetTag(GAME_TAG.CURRENT_TEMP_SPELLPOWER_SHADOW);
			break;
		case TAG_SPELL_SCHOOL.FEL:
			currentSpellpower += GetTag(GAME_TAG.CURRENT_SPELLPOWER_FEL);
			tempSpellpower += GetTag(GAME_TAG.CURRENT_TEMP_SPELLPOWER_FEL);
			break;
		case TAG_SPELL_SCHOOL.PHYSICAL_COMBAT:
			currentSpellpower += GetTag(GAME_TAG.CURRENT_SPELLPOWER_PHYSICAL);
			tempSpellpower += GetTag(GAME_TAG.CURRENT_TEMP_SPELLPOWER_PHYSICAL);
			break;
		}
		if (currentSpellpower == 0)
		{
			return tempSpellpower > 0;
		}
		return false;
	}

	public int TotalSpellpower(Entity ent, TAG_SPELL_SCHOOL spellSchool = TAG_SPELL_SCHOOL.NONE)
	{
		int includeTempSpellDamage = 1;
		if (ent.HasTag(GAME_TAG.SECRET) || ent.HasTag(GAME_TAG.SIGIL))
		{
			includeTempSpellDamage = 0;
		}
		int spellSchoolBonus = 0;
		switch (spellSchool)
		{
		case TAG_SPELL_SCHOOL.ARCANE:
			spellSchoolBonus += GetTag(GAME_TAG.CURRENT_SPELLPOWER_ARCANE);
			spellSchoolBonus += GetTag(GAME_TAG.CURRENT_TEMP_SPELLPOWER_ARCANE) * includeTempSpellDamage;
			break;
		case TAG_SPELL_SCHOOL.FIRE:
			spellSchoolBonus += GetTag(GAME_TAG.CURRENT_SPELLPOWER_FIRE);
			spellSchoolBonus += GetTag(GAME_TAG.CURRENT_TEMP_SPELLPOWER_FIRE) * includeTempSpellDamage;
			break;
		case TAG_SPELL_SCHOOL.FROST:
			spellSchoolBonus += GetTag(GAME_TAG.CURRENT_SPELLPOWER_FROST);
			spellSchoolBonus += GetTag(GAME_TAG.CURRENT_TEMP_SPELLPOWER_FROST) * includeTempSpellDamage;
			break;
		case TAG_SPELL_SCHOOL.NATURE:
			spellSchoolBonus += GetTag(GAME_TAG.CURRENT_SPELLPOWER_NATURE);
			spellSchoolBonus += GetTag(GAME_TAG.CURRENT_TEMP_SPELLPOWER_NATURE) * includeTempSpellDamage;
			break;
		case TAG_SPELL_SCHOOL.HOLY:
			spellSchoolBonus += GetTag(GAME_TAG.CURRENT_SPELLPOWER_HOLY);
			spellSchoolBonus += GetTag(GAME_TAG.CURRENT_TEMP_SPELLPOWER_HOLY) * includeTempSpellDamage;
			break;
		case TAG_SPELL_SCHOOL.SHADOW:
			spellSchoolBonus += GetTag(GAME_TAG.CURRENT_SPELLPOWER_SHADOW);
			spellSchoolBonus += GetTag(GAME_TAG.CURRENT_TEMP_SPELLPOWER_SHADOW) * includeTempSpellDamage;
			break;
		case TAG_SPELL_SCHOOL.FEL:
			spellSchoolBonus += GetTag(GAME_TAG.CURRENT_SPELLPOWER_FEL);
			spellSchoolBonus += GetTag(GAME_TAG.CURRENT_TEMP_SPELLPOWER_FEL) * includeTempSpellDamage;
			break;
		case TAG_SPELL_SCHOOL.PHYSICAL_COMBAT:
			spellSchoolBonus += GetTag(GAME_TAG.CURRENT_SPELLPOWER_PHYSICAL);
			spellSchoolBonus += GetTag(GAME_TAG.CURRENT_TEMP_SPELLPOWER_PHYSICAL) * includeTempSpellDamage;
			break;
		}
		int tempSpellBonus = GetTag(GAME_TAG.CURRENT_TEMP_SPELLPOWER_BASE) * includeTempSpellDamage;
		int entitySpellDamage = ent.GetTag(GAME_TAG.CURRENT_SPELLPOWER_BASE);
		return GetTag(GAME_TAG.CURRENT_SPELLPOWER_BASE) - GetTag(GAME_TAG.CURRENT_NEGATIVE_SPELLPOWER) + spellSchoolBonus + tempSpellBonus + entitySpellDamage;
	}

	public new bool IsRevealed()
	{
		if (IsFriendlySide())
		{
			return true;
		}
		if (SpectatorManager.Get().IsSpectatingPlayer(m_gameAccountId))
		{
			return true;
		}
		if (HasTag(GAME_TAG.ZONES_REVEALED))
		{
			return true;
		}
		return false;
	}

	public void SetSide(Side side)
	{
		m_side = side;
	}

	public int GetCardBackId()
	{
		return m_cardBackId;
	}

	public int GetOriginalCardBackId()
	{
		return m_initialCardBackId;
	}

	public void SetCardBackId(int id)
	{
		m_cardBackId = id;
		m_initialCardBackId = id;
	}

	public void SetOverrideCardBackId(int id)
	{
		if (id > 0)
		{
			m_cardBackId = id;
		}
		else
		{
			m_cardBackId = m_initialCardBackId;
		}
	}

	public int GetPlayerId()
	{
		return GetTag(GAME_TAG.PLAYER_ID);
	}

	public void SetPlayerId(int playerId)
	{
		SetTag(GAME_TAG.PLAYER_ID, playerId);
	}

	public int GetTeamId()
	{
		return GetTag(GAME_TAG.TEAM_ID);
	}

	public bool IsTeamLeader()
	{
		return GetPlayerId() == GetTeamId();
	}

	public override int BaconFreeRerollLeft()
	{
		if (GameState.Get().GetGameEntity() == null)
		{
			return 0;
		}
		return GetTag(GAME_TAG.BACON_PREMIUM_FREE_REROLLS) - GetTag(GAME_TAG.BACON_NUM_FREE_REROLLS_USED);
	}

	public bool IsCurrentPlayer()
	{
		if (GameState.Get().GetGameEntity().OverwriteCurrentPlayer(this, out var isCurrentPlayer))
		{
			return isCurrentPlayer;
		}
		return HasTag(GAME_TAG.CURRENT_PLAYER);
	}

	public bool IsComboActive()
	{
		return HasTag(GAME_TAG.COMBO_ACTIVE);
	}

	public bool IsRealTimeComboActive()
	{
		return m_realTimeComboActive;
	}

	public void SetRealTimeComboActive(int tagValue)
	{
		SetRealTimeComboActive(tagValue == 1);
	}

	public void SetRealTimeComboActive(bool active)
	{
		m_realTimeComboActive = active;
	}

	public void SetRealTimeSpellsCostHealth(int value)
	{
		m_realTimeSpellsCostHealth = value > 0;
	}

	public bool GetRealTimeSpellsCostHealth()
	{
		return m_realTimeSpellsCostHealth;
	}

	public override void InitRealTimeValues(List<Network.Entity.Tag> tags)
	{
		base.InitRealTimeValues(tags);
		foreach (Network.Entity.Tag tag in tags)
		{
			switch ((GAME_TAG)tag.Name)
			{
			case GAME_TAG.TEMP_RESOURCES:
				SetRealTimeTempMana(tag.Value);
				break;
			case GAME_TAG.COMBO_ACTIVE:
				SetRealTimeComboActive(tag.Value);
				break;
			case GAME_TAG.SPELLS_COST_HEALTH:
				SetRealTimeSpellsCostHealth(tag.Value);
				break;
			}
		}
	}

	public int GetNumAvailableResources()
	{
		int temporaryResources = GetTag(GAME_TAG.TEMP_RESOURCES);
		int tag = GetTag(GAME_TAG.RESOURCES);
		int usedResources = GetTag(GAME_TAG.RESOURCES_USED);
		int availableResources = tag + temporaryResources - usedResources - m_queuedSpentMana - m_usedTempMana;
		if (availableResources >= 0)
		{
			return availableResources;
		}
		return 0;
	}

	public int GetNumAvailableCorpses()
	{
		if (GetHero() == null)
		{
			return 0;
		}
		int tag = GetTag(GAME_TAG.CORPSES);
		int usedCorpses = GetTag(GAME_TAG.CORPSES_SPENT_THIS_GAME);
		int availableCorpses = tag - usedCorpses;
		if (availableCorpses >= 0)
		{
			return availableCorpses;
		}
		return 0;
	}

	public bool HasWeapon()
	{
		foreach (Zone zone in ZoneMgr.Get().GetZones())
		{
			if (zone is ZoneWeapon && zone.m_Side == m_side)
			{
				return zone.GetCards().Count > 0;
			}
		}
		return false;
	}

	public virtual void SetHero(Entity hero)
	{
		if (m_hero != null && hero.HasTag(GAME_TAG.KEEP_HERO_CLASS))
		{
			hero.SetTag(GAME_TAG.CLASS, m_hero.GetTag(GAME_TAG.CLASS));
			hero.SetTag(GAME_TAG.MULTIPLE_CLASSES, m_hero.GetTag(GAME_TAG.MULTIPLE_CLASSES));
		}
		m_hero = hero;
		if (ShouldUseHeroName())
		{
			UpdateDisplayInfo();
		}
		foreach (Card card in GetHandZone().GetCards())
		{
			if (card.GetEntity() != hero && card.GetEntity().IsMultiClass())
			{
				card.UpdateActorComponents();
			}
		}
		if (IsFriendlySide())
		{
			GameState.Get().FireHeroChangedEvent(this);
		}
		CorpseCounter.UpdateTextAll();
	}

	public Entity GetStartingHero()
	{
		Entity hero = GetHero();
		if (hero == null)
		{
			return hero;
		}
		while (hero.HasTag(GAME_TAG.LINKED_ENTITY))
		{
			int linkedHeroID = hero.GetTag(GAME_TAG.LINKED_ENTITY);
			Entity linkedHero = GameState.Get().GetEntity(linkedHeroID);
			if (linkedHero == null || !linkedHero.IsHero())
			{
				Log.Gameplay.PrintError("Player.GetStartingHero() - Hero entity {0} has a LINKED_ENTITY tag value of {1} which corresponds to invalid Entity {2}.", hero, linkedHeroID, linkedHero);
				break;
			}
			hero = linkedHero;
		}
		return hero;
	}

	public override Entity GetHero()
	{
		return m_hero;
	}

	public EntityDef GetHeroEntityDef()
	{
		if (m_hero == null)
		{
			return null;
		}
		EntityDef entityDef = m_hero.GetEntityDef();
		if (entityDef == null)
		{
			return null;
		}
		return entityDef;
	}

	public override Card GetHeroCard()
	{
		if (m_hero == null)
		{
			return null;
		}
		return m_hero.GetCard();
	}

	public void SetHeroPower(Entity heroPower)
	{
		m_heroPower = heroPower;
	}

	public override Entity GetHeroPower()
	{
		return m_heroPower;
	}

	public override Card GetHeroPowerCard()
	{
		if (m_heroPower == null)
		{
			return null;
		}
		return m_heroPower.GetCard();
	}

	public Card GetHeroPowerCardWithIndex(int index)
	{
		foreach (ZoneHeroPower item in ZoneMgr.Get().FindZonesOfType<ZoneHeroPower>(GetSide()))
		{
			Card card = item.GetFirstCard();
			if (!(card == null) && card.GetEntity() != null && card.GetEntity().GetTag(GAME_TAG.ADDITIONAL_HERO_POWER_INDEX) == index)
			{
				return card;
			}
		}
		return null;
	}

	public bool IsHeroPowerAffectedByBonusDamage()
	{
		Card heroPowerCard = GetHeroPowerCard();
		if (heroPowerCard == null)
		{
			return false;
		}
		Entity entity = heroPowerCard.GetEntity();
		if (!entity.IsHeroPower())
		{
			return false;
		}
		return entity.GetCardTextBuilder().ContainsBonusDamageToken(entity);
	}

	public override Card GetWeaponCard()
	{
		return ZoneMgr.Get().FindZoneOfType<ZoneWeapon>(GetSide()).GetFirstCard();
	}

	public override Card GetHeroBuddyCard()
	{
		return ZoneMgr.Get().FindZoneOfType<ZoneBattlegroundHeroBuddy>(GetSide()).GetFirstCard();
	}

	public override Card GetBaconClickableButtonCard()
	{
		return ZoneMgr.Get().FindZoneOfType<ZoneBattlegroundClickableButton>(GetSide()).GetFirstCard();
	}

	public override Card GetQuestRewardFromHeroPowerCard()
	{
		foreach (ZoneBattlegroundQuestReward zone in ZoneMgr.Get().FindZonesOfType<ZoneBattlegroundQuestReward>(GetSide()))
		{
			if (zone.m_isHeroPower)
			{
				return zone.GetFirstCard();
			}
		}
		return null;
	}

	public override Card GetQuestRewardCard()
	{
		foreach (ZoneBattlegroundQuestReward zone in ZoneMgr.Get().FindZonesOfType<ZoneBattlegroundQuestReward>(GetSide()))
		{
			if (!zone.m_isHeroPower)
			{
				return zone.GetFirstCard();
			}
		}
		return null;
	}

	public override List<Card> GetQuestRewardCards()
	{
		List<Card> cards = new List<Card>();
		foreach (ZoneBattlegroundQuestReward zone in ZoneMgr.Get().FindZonesOfType<ZoneBattlegroundQuestReward>(GetSide()))
		{
			if (zone.GetFirstCard() != null)
			{
				cards.Add(zone.GetFirstCard());
			}
		}
		return cards;
	}

	public ZoneHand GetHandZone()
	{
		return ZoneMgr.Get().FindZoneOfType<ZoneHand>(GetSide());
	}

	public ZonePlay GetBattlefieldZone()
	{
		return ZoneMgr.Get().FindZoneOfType<ZonePlay>(GetSide());
	}

	public ZoneDeck GetDeckZone()
	{
		return ZoneMgr.Get().FindZoneOfType<ZoneDeck>(GetSide());
	}

	public ZoneGraveyard GetGraveyardZone()
	{
		return ZoneMgr.Get().FindZoneOfType<ZoneGraveyard>(GetSide());
	}

	public ZoneSecret GetSecretZone()
	{
		return ZoneMgr.Get().FindZoneOfType<ZoneSecret>(GetSide());
	}

	public ZoneHero GetHeroZone()
	{
		return ZoneMgr.Get().FindZoneOfType<ZoneHero>(GetSide());
	}

	public ZoneLettuceAbility GetLettuceAbilityZone()
	{
		return ZoneMgr.Get().FindZoneOfType<ZoneLettuceAbility>(GetSide());
	}

	public bool HasReadyAttackers()
	{
		List<Card> cardsInField = GetBattlefieldZone().GetCards();
		for (int i = 0; i < cardsInField.Count; i++)
		{
			if (GameState.Get().HasResponse(cardsInField[i].GetEntity(), null))
			{
				return true;
			}
		}
		return false;
	}

	public uint GetArenaWins()
	{
		return m_arenaWins;
	}

	public uint GetArenaLosses()
	{
		return m_arenaLoss;
	}

	public uint GetTavernBrawlWins()
	{
		return m_tavernBrawlWins;
	}

	public uint GetTavernBrawlLosses()
	{
		return m_tavernBrawlLoss;
	}

	public uint GetDuelsWins()
	{
		return m_duelsWins;
	}

	public uint GetDuelsLosses()
	{
		return m_duelsLoss;
	}

	public void PlayConcedeEmote()
	{
		if (!m_concedeEmotePlayed)
		{
			Card heroCard = GetHeroCard();
			if (!(heroCard == null))
			{
				heroCard.PlayEmote(EmoteType.CONCEDE);
				m_concedeEmotePlayed = true;
			}
		}
	}

	public BnetGameAccountId GetGameAccountId()
	{
		return m_gameAccountId;
	}

	public BnetPlayer GetBnetPlayer()
	{
		return BnetPresenceMgr.Get().GetPlayer(m_gameAccountId);
	}

	public bool IsDisplayable()
	{
		if (m_gameAccountId == null)
		{
			return false;
		}
		if (!IsBnetPlayer())
		{
			if (ShouldUseHeroName() && GetHeroEntityDef() == null)
			{
				return false;
			}
			return true;
		}
		BnetPlayer player = BnetPresenceMgr.Get().GetPlayer(m_gameAccountId);
		if (player == null)
		{
			return false;
		}
		if (!player.IsDisplayable())
		{
			return false;
		}
		if (GameUtils.IsGameTypeRanked())
		{
			BnetGameAccount hsGameAccount = player.GetHearthstoneGameAccount();
			if (hsGameAccount == null)
			{
				return false;
			}
			if (!hsGameAccount.HasGameField(18u))
			{
				return false;
			}
		}
		return true;
	}

	public void WipeZzzs()
	{
		foreach (Card card in GetBattlefieldZone().GetCards())
		{
			Spell spell = card.GetActorSpell(SpellType.Zzz);
			if (!(spell == null))
			{
				spell.ActivateState(SpellStateType.DEATH);
			}
		}
	}

	public TAG_PLAYSTATE GetPreGameOverPlayState()
	{
		return m_preGameOverPlayState;
	}

	public bool HasSeenStartOfGameSpell(EntityDef entityDef)
	{
		if (GameState.Get().GetTurn() > 2)
		{
			return true;
		}
		return m_seenStartOfGameSpells.Contains(entityDef);
	}

	public void MarkStartOfGameSpellAsSeen(EntityDef entityDef)
	{
		m_seenStartOfGameSpells.Add(entityDef);
	}

	public bool IsEarlyConcedePopupAvailable()
	{
		return HasTag(GAME_TAG.EARLY_CONCEDE_POPUP_AVAILABLE);
	}

	public void AddManaCrystal(int numCrystals, bool isTurnStart)
	{
		if (IsFriendlySide())
		{
			ManaCrystalMgr.Get().AddManaCrystals(numCrystals, isTurnStart);
		}
	}

	public void AddManaCrystal(int numCrystals)
	{
		AddManaCrystal(numCrystals, isTurnStart: false);
	}

	public void DestroyManaCrystal(int numCrystals)
	{
		if (IsFriendlySide())
		{
			ManaCrystalMgr.Get().DestroyManaCrystals(numCrystals);
		}
	}

	public void AddTempManaCrystal(int numCrystals)
	{
		if (IsFriendlySide())
		{
			ManaCrystalMgr.Get().AddTempManaCrystals(numCrystals);
		}
	}

	public void DestroyTempManaCrystal(int numCrystals)
	{
		if (IsFriendlySide())
		{
			ManaCrystalMgr.Get().DestroyTempManaCrystals(numCrystals);
		}
	}

	public void ReadyManaCrystal(int numCrystals)
	{
		if (IsFriendlySide())
		{
			ManaCrystalMgr.Get().ReadyManaCrystals(numCrystals);
		}
	}

	public void HandleSameTurnOverloadChanged(int crystalsChanged)
	{
		if (IsFriendlySide())
		{
			ManaCrystalMgr.Get().HandleSameTurnOverloadChanged(crystalsChanged);
		}
	}

	public void UnlockCrystals(int numCrystals)
	{
		if (IsFriendlySide())
		{
			ManaCrystalMgr.Get().UnlockCrystals(numCrystals);
		}
	}

	public void CancelAllProposedMana(Entity entity)
	{
		if (IsFriendlySide())
		{
			ManaCrystalMgr.Get().CancelAllProposedMana(entity);
		}
	}

	public void ProposeManaCrystalUsage(Entity entity)
	{
		if (IsFriendlySide())
		{
			ManaCrystalMgr.Get().ProposeManaCrystalUsage(entity);
		}
	}

	public void ResetUnresolvedManaToBeReadied()
	{
		if (IsFriendlySide())
		{
			ManaCrystalMgr.Get().ResetUnresolvedManaToBeReadied();
		}
	}

	public void UpdateManaCounter()
	{
		if (!(m_manaCounter == null))
		{
			m_manaCounter.UpdateText();
		}
	}

	public void NotifyOfSpentMana(int spentMana)
	{
		m_queuedSpentMana += spentMana;
	}

	public void NotifyOfUsedTempMana(int usedMana)
	{
		m_usedTempMana += usedMana;
	}

	public void SetRealTimeTempMana(int tempMana)
	{
		m_realtimeTempMana = tempMana;
	}

	public int GetRealTimeTempMana()
	{
		return m_realtimeTempMana;
	}

	public void OnBoardLoaded()
	{
		AssignPlayerBoardObjects();
		GameState.Get().UpdateCornerReplacements();
	}

	public override void OnRealTimeTagChanged(Network.HistTagChange change)
	{
		switch ((GAME_TAG)change.Tag)
		{
		case GAME_TAG.TEMP_RESOURCES:
			SetRealTimeTempMana(change.Value);
			break;
		case GAME_TAG.COMBO_ACTIVE:
			SetRealTimeComboActive(change.Value);
			break;
		case GAME_TAG.PLAYSTATE:
		{
			TAG_PLAYSTATE playState = (TAG_PLAYSTATE)change.Value;
			if (GameUtils.IsPreGameOverPlayState(playState))
			{
				m_preGameOverPlayState = playState;
			}
			break;
		}
		case GAME_TAG.SPELLS_COST_HEALTH:
			SetRealTimeSpellsCostHealth(change.Value);
			break;
		case GAME_TAG.BACON_NUMBER_HERO_REFRESH_AVAILABLE:
			if (IsFriendlySide() && MulliganManager.Get() != null)
			{
				MulliganManager.Get().OnFriendlyPlayerNumberRefreshAvailableChanged(change.Value);
			}
			break;
		}
	}

	public override void OnTagsChanged(TagDeltaList changeList, bool fromShowEntity)
	{
		for (int i = 0; i < changeList.Count; i++)
		{
			TagDelta change = changeList[i];
			OnTagChanged(change);
		}
	}

	public override void OnTagChanged(TagDelta change)
	{
		if (IsFriendlySide())
		{
			OnFriendlyPlayerTagChanged(change);
		}
		else
		{
			OnOpposingPlayerTagChanged(change);
		}
		switch ((GAME_TAG)change.tag)
		{
		case GAME_TAG.CURRENT_PLAYER:
			if (change.newValue == 1 && GameState.Get().IsLocalSidePlayerTurn())
			{
				ManaCrystalMgr.Get().OnCurrentPlayerChanged();
				m_queuedSpentMana = 0;
				if (GameState.Get().IsMainPhase())
				{
					TurnStartManager.Get().BeginListeningForTurnEvents();
				}
			}
			break;
		case GAME_TAG.RESOURCES_USED:
		case GAME_TAG.RESOURCES:
		case GAME_TAG.TEMP_RESOURCES:
			if (!GameState.Get().IsTurnStartManagerActive() || !IsFriendlySide())
			{
				UpdateManaCounter();
			}
			GameState.Get().UpdateOptionHighlights();
			if (StarshipHUDManager.Get() != null)
			{
				StarshipHUDManager.Get().UpdateLaunchButton(this);
			}
			break;
		case GAME_TAG.STARSHIP_LAUNCH_COST_DISCOUNT:
			if (StarshipHUDManager.Get() != null)
			{
				StarshipHUDManager.Get().UpdateLaunchButton(this);
			}
			break;
		case GAME_TAG.COMBO_ACTIVE:
			foreach (Card card2 in GetHandZone().GetCards())
			{
				card2.UpdateActorState();
			}
			GetHeroPower()?.GetCard().UpdateActorState();
			break;
		case GAME_TAG.PLAYSTATE:
			if (change.newValue == 8)
			{
				PlayConcedeEmote();
			}
			break;
		case GAME_TAG.MULLIGAN_STATE:
			if (change.newValue == 4 && MulliganManager.Get() != null)
			{
				MulliganManager.Get().ServerHasDealtReplacementCards(IsFriendlySide());
			}
			break;
		case GAME_TAG.STEADY_SHOT_CAN_TARGET:
		{
			Card heroPowerCard3 = GetHeroPowerCard();
			ToggleActorSpellOnCard(heroPowerCard3, change, SpellType.STEADY_SHOT_CAN_TARGET);
			break;
		}
		case GAME_TAG.CURRENT_HEROPOWER_DAMAGE_BONUS:
			if (IsHeroPowerAffectedByBonusDamage())
			{
				Card heroPowerCard = GetHeroPowerCard();
				ToggleActorSpellOnCard(heroPowerCard, change, SpellType.CURRENT_HEROPOWER_DAMAGE_BONUS);
			}
			break;
		case GAME_TAG.LOCK_AND_LOAD:
		{
			Card heroCard7 = GetHeroCard();
			ToggleActorSpellOnCard(heroCard7, change, SpellType.LOCK_AND_LOAD);
			break;
		}
		case GAME_TAG.DEATH_KNIGHT:
		{
			Card heroCard6 = GetHeroCard();
			ToggleActorSpellOnCard(heroCard6, change, SpellType.DEATH_KNIGHT);
			break;
		}
		case GAME_TAG.SPELLS_COST_HEALTH:
			UpdateSpellsCostHealth(change);
			break;
		case GAME_TAG.CHOOSE_BOTH:
			UpdateChooseBoth();
			break;
		case GAME_TAG.EMBRACE_THE_SHADOW:
		{
			Card heroCard4 = GetHeroCard();
			ToggleActorSpellOnCard(heroCard4, change, SpellType.EMBRACE_THE_SHADOW);
			break;
		}
		case GAME_TAG.STAMPEDE:
		{
			Card heroCard5 = GetHeroCard();
			ToggleActorSpellOnCard(heroCard5, change, SpellType.STAMPEDE);
			break;
		}
		case GAME_TAG.IS_VAMPIRE:
		{
			Card heroCard3 = GetHeroCard();
			ToggleActorSpellOnCard(heroCard3, change, SpellType.IS_VAMPIRE);
			break;
		}
		case GAME_TAG.GLORIOUSGLOOP:
		{
			Card heroCard2 = GetHeroCard();
			ToggleActorSpellOnCard(heroCard2, change, SpellType.GLORIOUSGLOOP);
			break;
		}
		case GAME_TAG.SPELLS_CAST_TWICE:
		{
			Card heroCard = GetHeroCard();
			ToggleActorSpellOnCard(heroCard, change, SpellType.SPELLS_CAST_TWICE);
			break;
		}
		case GAME_TAG.DECK_POWER_UP:
		{
			Card heroCard8 = GetHeroCard();
			Spell powerUpSpell = ToggleActorSpellOnCard(heroCard8, change, SpellType.DECK_POWER_UP);
			if (powerUpSpell != null && GetHeroCard() != null && GetHeroCard().gameObject != null)
			{
				powerUpSpell.SetSource(GetHeroCard().gameObject);
				powerUpSpell.ForceUpdateTransform();
			}
			break;
		}
		case GAME_TAG.PROGRESSBAR_SHOW:
			if (change.newValue == 1)
			{
				AssetLoader.Get().InstantiatePrefab("PopUpProgressBar.prefab:1e74ef51d3388674792ddf7d6233f5d7").GetComponent<PopUpController>()
					.Populate(GetTag(GAME_TAG.PROGRESSBAR_PROGRESS), GetTag(GAME_TAG.PROGRESSBAR_TOTAL), GetTag(GAME_TAG.PROGRESSBAR_CARDID), CollectionManager.Get().GetHeroPremium(m_hero.GetClass()));
			}
			break;
		case GAME_TAG.OVERRIDE_EMOTE_0:
		case GAME_TAG.OVERRIDE_EMOTE_1:
		case GAME_TAG.OVERRIDE_EMOTE_2:
		case GAME_TAG.OVERRIDE_EMOTE_3:
		case GAME_TAG.OVERRIDE_EMOTE_4:
		case GAME_TAG.OVERRIDE_EMOTE_5:
			if (EmoteHandler.Get() != null)
			{
				EmoteHandler.Get().ChangeAvailableEmotes();
			}
			break;
		case GAME_TAG.HERO_POWER_DISABLED:
		{
			Card heroPowerCard2 = GetHeroPowerCard();
			if (heroPowerCard2 != null && heroPowerCard2.GetEntity() != null)
			{
				heroPowerCard2.HandleHeroPowerDisabledTagChanged(change);
			}
			break;
		}
		case GAME_TAG.MARK_OF_EVIL:
			GetOrCreateMarkOfEvilCounter().OnMarksChanged(change.newValue);
			break;
		case GAME_TAG.WHIZBANG_DECK_ID:
			if (IsLocalUser())
			{
				GameMgr.Get().LastGameData.WhizbangDeckID = change.newValue;
			}
			break;
		case GAME_TAG.IGNORE_TAUNT:
		{
			foreach (Card card in GameState.Get().GetFirstOpponentPlayer(GetController()).GetBattlefieldZone()
				.GetCards())
			{
				if (!card.CanShowActorVisuals())
				{
					continue;
				}
				Entity entity = card.GetEntity();
				if (entity != null && entity.HasTaunt())
				{
					Actor actor = card.GetActor();
					if (!(actor == null))
					{
						actor.ActivateTaunt();
					}
				}
			}
			break;
		}
		case GAME_TAG.CARD_BACK_OVERRIDE:
			SetOverrideCardBackId(change.newValue);
			CardBackManager.Get().SetGameCardBackIDs(GameState.Get().GetFriendlySidePlayer().GetCardBackId(), GameState.Get().GetOpposingSidePlayer().GetCardBackId());
			break;
		case GAME_TAG.BACON_CURRENT_COMBAT_PLAYER_ID:
			GameState.Get().GetGameEntity().UpdateNameDisplay();
			PlayerLeaderboardManager.Get().UpdateSwordsDisplay();
			(GameState.Get().GetGameEntity() as TB_BaconShop).SwitchedPlayersInCombat();
			break;
		case GAME_TAG.CORNER_REPLACEMENT_TYPE:
			GameState.Get().UpdateCornerReplacements();
			break;
		case GAME_TAG.BACON_PREMIUM_FREE_REROLLS:
		case GAME_TAG.BACON_NUM_FREE_REROLLS_USED:
		{
			Entity hero = GetHero();
			if (hero != null && hero.GetCard() != null && hero.GetCard().GetActor() != null)
			{
				PlayerLeaderboardMainCardActor baconHeroActor = hero.GetCard().GetActor().GetComponent<PlayerLeaderboardMainCardActor>();
				GameEntity gameEntity = GameState.Get().GetGameEntity();
				if (baconHeroActor != null && gameEntity != null)
				{
					int num = ((change.tag == 3901) ? change.newValue : GetTag(GAME_TAG.BACON_PREMIUM_FREE_REROLLS));
					int numFreeRerollsLeft = ((change.tag == 3904) ? change.newValue : GetTag(GAME_TAG.BACON_NUM_FREE_REROLLS_USED));
					bool hasFreeReroll = num - numFreeRerollsLeft > 0;
					bool hasPaidReroll = NetCache.Get().GetBattlegroundsTokenBalance() > 0;
					baconHeroActor.SetShowHeroRerollButton(baconHeroActor.ShowHeroRerollButton(), hasFreeReroll || hasPaidReroll);
				}
			}
			break;
		}
		}
	}

	public MarkOfEvilCounter GetOrCreateMarkOfEvilCounter()
	{
		if (m_markOfEvilCounter == null)
		{
			GameObject markOfEvilGO = AssetLoader.Get().InstantiatePrefab("MarkOfEvilCounter.prefab:ff08f2e19826b354bb37bb25bf81471d", AssetLoadingOptions.IgnorePrefabPosition);
			m_markOfEvilCounter = markOfEvilGO.GetComponent<MarkOfEvilCounter>();
			string boneName = ((GetSide() == Side.FRIENDLY) ? "MarkOfEvil" : "MarkOfEvil_Opponent");
			Transform bone = Board.Get().FindBone(boneName);
			TransformUtil.CopyWorld(markOfEvilGO, bone);
		}
		return m_markOfEvilCounter;
	}

	private void OnFriendlyPlayerTagChanged(TagDelta change)
	{
		switch ((GAME_TAG)change.tag)
		{
		case GAME_TAG.OVERLOAD_OWED:
			HandleSameTurnOverloadChanged(change.newValue - change.oldValue);
			break;
		case GAME_TAG.OVERLOAD_LOCKED:
			if (change.newValue < change.oldValue && !GameState.Get().IsTurnStartManagerActive())
			{
				UnlockCrystals(change.oldValue - change.newValue);
			}
			break;
		case GAME_TAG.TEMP_RESOURCES:
		{
			int tempOldAmount = change.oldValue - m_usedTempMana;
			int tempChangeAmount = change.newValue - change.oldValue;
			if (tempChangeAmount < 0)
			{
				m_usedTempMana += tempChangeAmount;
			}
			if (m_usedTempMana < 0)
			{
				m_usedTempMana = 0;
			}
			if (tempOldAmount < 0)
			{
				tempOldAmount = 0;
			}
			int tempManaShownChangeAmount = change.newValue - tempOldAmount - m_usedTempMana;
			if (tempManaShownChangeAmount > 0)
			{
				AddTempManaCrystal(tempManaShownChangeAmount);
			}
			else
			{
				DestroyTempManaCrystal(-tempManaShownChangeAmount);
			}
			break;
		}
		case GAME_TAG.RESOURCES:
			if (change.newValue > change.oldValue)
			{
				if (GameState.Get().IsTurnStartManagerActive() && IsFriendlySide())
				{
					TurnStartManager.Get().NotifyOfManaCrystalGained(change.newValue - change.oldValue);
				}
				else
				{
					AddManaCrystal(change.newValue - change.oldValue);
				}
			}
			else
			{
				DestroyManaCrystal(change.oldValue - change.newValue);
			}
			break;
		case GAME_TAG.RESOURCES_USED:
		{
			int oldAmount = change.oldValue + m_queuedSpentMana;
			int changeAmount = change.newValue - change.oldValue;
			if (changeAmount > 0)
			{
				m_queuedSpentMana -= changeAmount;
			}
			if (m_queuedSpentMana < 0)
			{
				m_queuedSpentMana = 0;
			}
			int shownChangeAmount = change.newValue - oldAmount + m_queuedSpentMana;
			ManaCrystalMgr.Get().UpdateSpentMana(shownChangeAmount);
			break;
		}
		case GAME_TAG.CORPSES:
		case GAME_TAG.CORPSES_SPENT_THIS_GAME:
			CorpseCounter.UpdateTextAll();
			break;
		case GAME_TAG.MULLIGAN_STATE:
			if (change.newValue == 4)
			{
				if (!(MulliganManager.Get() == null))
				{
					break;
				}
				{
					foreach (Card card2 in GetHandZone().GetCards())
					{
						card2.GetActor().TurnOnCollider();
					}
					break;
				}
			}
			if (change.newValue == 1 && change.oldValue == 5 && MulliganManager.Get() != null)
			{
				MulliganManager.Get().ServerHasDealtReplacementCards(IsFriendlySide());
			}
			break;
		case GAME_TAG.CURRENT_SPELLPOWER_BASE:
		case GAME_TAG.SPELLPOWER_DOUBLE:
		case GAME_TAG.SPELL_HEALING_DOUBLE:
		case GAME_TAG.JADE_GOLEM:
		case GAME_TAG.CURRENT_NEGATIVE_SPELLPOWER:
		case GAME_TAG.AMOUNT_HEALED_THIS_GAME:
		case GAME_TAG.NUM_HERO_POWER_DAMAGE_THIS_GAME:
		case GAME_TAG.ARMOR_GAINED_THIS_GAME:
		case GAME_TAG.CURRENT_SPELLPOWER_ARCANE:
		case GAME_TAG.CURRENT_SPELLPOWER_FIRE:
		case GAME_TAG.CURRENT_SPELLPOWER_FROST:
		case GAME_TAG.CURRENT_SPELLPOWER_NATURE:
		case GAME_TAG.CURRENT_SPELLPOWER_HOLY:
		case GAME_TAG.CURRENT_SPELLPOWER_SHADOW:
		case GAME_TAG.CURRENT_SPELLPOWER_FEL:
		case GAME_TAG.CURRENT_SPELLPOWER_PHYSICAL:
		case GAME_TAG.CURRENT_TEMP_SPELLPOWER_ARCANE:
		case GAME_TAG.CURRENT_TEMP_SPELLPOWER_FEL:
		case GAME_TAG.CURRENT_TEMP_SPELLPOWER_FIRE:
		case GAME_TAG.CURRENT_TEMP_SPELLPOWER_FROST:
		case GAME_TAG.CURRENT_TEMP_SPELLPOWER_NATURE:
		case GAME_TAG.CURRENT_TEMP_SPELLPOWER_HOLY:
		case GAME_TAG.CURRENT_TEMP_SPELLPOWER_PHYSICAL:
		case GAME_TAG.CURRENT_TEMP_SPELLPOWER_SHADOW:
		case GAME_TAG.CURRENT_TEMP_SPELLPOWER_BASE:
			UpdateHandCardPowersText(onlySpells: false);
			break;
		case GAME_TAG.ALL_HEALING_DOUBLE:
			UpdateHandCardPowersText(onlySpells: false);
			UpdateHeroPowerBigCard();
			break;
		case GAME_TAG.HERO_POWER_DOUBLE:
		case GAME_TAG.CURRENT_HEROPOWER_DAMAGE_BONUS:
		case GAME_TAG.HERO_ATTACK_GIVEN_ADDITIONAL:
		case GAME_TAG.HERO_ARMOR_GIVEN_ADDITIONAL:
			UpdateHeroPowerBigCard();
			break;
		case GAME_TAG.RED_MANA_CRYSTALS:
			ManaCrystalMgr.Get().TurnCrystalsRed(change.oldValue, change.newValue);
			break;
		case GAME_TAG.NUM_TURNS_LEFT:
		{
			TurnStartManager turnMgr = TurnStartManager.Get();
			if (turnMgr != null)
			{
				Spell extraSpell = turnMgr.GetExtraTurnSpell();
				if (change.oldValue >= 2 && change.newValue == 1)
				{
					turnMgr.NotifyOfExtraTurn(extraSpell, isEnding: true);
				}
				if (change.newValue >= 2 && change.newValue > change.oldValue)
				{
					turnMgr.NotifyOfExtraTurn(extraSpell);
				}
			}
			break;
		}
		case GAME_TAG.STARSHIP_LAUNCH_COST_DISCOUNT:
		{
			foreach (Card card in GetBattlefieldZone().GetCards())
			{
				if (card.GetEntity().IsLaunchpad())
				{
					card.GetActor().UpdateMeshComponents();
					card.UpdateActorState();
					break;
				}
			}
			break;
		}
		}
	}

	private void OnOpposingPlayerTagChanged(TagDelta change)
	{
		switch ((GAME_TAG)change.tag)
		{
		case GAME_TAG.PLAYSTATE:
			if (change.newValue == 7)
			{
				NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, GameStrings.Get("VO_ANNOUNCER_DISCONNECT_45"), "VO_ANNOUNCER_DISCONNECT_45.prefab:911a83eb9ad91fc41acf1aca808c5e5a");
			}
			break;
		case GAME_TAG.RESOURCES:
			if (change.newValue > change.oldValue)
			{
				GameState.Get().GetGameEntity().NotifyOfEnemyManaCrystalSpawned();
			}
			break;
		case GAME_TAG.NUM_TURNS_LEFT:
		{
			TurnStartManager turnMgr = TurnStartManager.Get();
			if (turnMgr != null)
			{
				Spell extraSpell = turnMgr.GetExtraTurnSpell(isFriendly: false);
				if (change.oldValue >= 2 && change.newValue == 1)
				{
					TurnStartManager.Get().NotifyOfExtraTurn(extraSpell, isEnding: true, isFriendly: false);
				}
				if (change.newValue >= 2 && change.newValue > change.oldValue)
				{
					TurnStartManager.Get().NotifyOfExtraTurn(extraSpell, isEnding: false, isFriendly: false);
				}
			}
			break;
		}
		case GAME_TAG.CORPSES:
		case GAME_TAG.CORPSES_SPENT_THIS_GAME:
			CorpseCounter.UpdateTextAll();
			break;
		case GAME_TAG.STARSHIP_LAUNCH_COST_DISCOUNT:
		{
			foreach (Card card in GetBattlefieldZone().GetCards())
			{
				if (card.GetEntity().IsLaunchpad())
				{
					card.GetActor().UpdateLaunchpadComponents();
					break;
				}
			}
			break;
		}
		}
	}

	private void UpdateName()
	{
		GameEntity gameEntity = GameState.Get()?.GetGameEntity();
		if (gameEntity != null && gameEntity.ShouldUseAlternateNameForPlayer(GetSide()))
		{
			m_name = gameEntity.GetNameBannerOverride(GetSide());
		}
		else if (ShouldUseHeroName())
		{
			UpdateNameWithHeroName();
		}
		else if (IsAI())
		{
			if (GameUtils.IsMatchmadeGameType(GameMgr.Get().GetGameType(), null))
			{
				m_name = GetRandomName();
			}
			else
			{
				m_name = GameStrings.Get("GAMEPLAY_AI_OPPONENT_NAME");
			}
		}
		else if (IsBnetPlayer())
		{
			BnetPlayer player = BnetPresenceMgr.Get().GetPlayer(m_gameAccountId);
			if (player != null)
			{
				m_name = player.GetBestName();
			}
			if (!string.IsNullOrEmpty(m_name))
			{
				GameMgr.Get().SetLastDisplayedPlayerName(GetPlayerId(), m_name);
			}
		}
		else
		{
			Debug.LogError($"Player.UpdateName() - unable to determine player name");
		}
	}

	private bool ShouldUseHeroName()
	{
		if (IsBnetPlayer())
		{
			return false;
		}
		if (IsAI())
		{
			if (GameMgr.Get().IsPractice())
			{
				return false;
			}
			if (GameUtils.IsMatchmadeGameType(GameMgr.Get().GetGameType(), null))
			{
				return false;
			}
		}
		return true;
	}

	public string GetRandomName(int seed = 0)
	{
		string[] names = ExternalUrlService.Get().GetRandomNamesText().Split(',');
		if (names.Length == 0)
		{
			return GameStrings.Get("GAMEPLAY_AI_OPPONENT_NAME");
		}
		if (seed == 0)
		{
			seed = GameMgr.Get().GetGameHandle();
		}
		System.Random random = new System.Random(seed);
		return names[random.Next(0, names.Length - 1)];
	}

	private void UpdateNameWithHeroName()
	{
		if (m_hero != null)
		{
			EntityDef heroEntityDef = m_hero.GetEntityDef();
			if (heroEntityDef != null)
			{
				m_name = heroEntityDef.GetName();
			}
		}
	}

	private bool ShouldUseBogusRank()
	{
		if (IsBnetPlayer())
		{
			return false;
		}
		return true;
	}

	private void UpdateRank()
	{
		MedalInfoTranslator medalInfo = null;
		if (ShouldUseBogusRank())
		{
			medalInfo = new MedalInfoTranslator();
		}
		else if (m_gameAccountId == BnetPresenceMgr.Get().GetMyGameAccountId())
		{
			medalInfo = RankMgr.Get().GetLocalPlayerMedalInfo();
		}
		if (medalInfo == null)
		{
			BnetPlayer player = BnetPresenceMgr.Get().GetPlayer(m_gameAccountId);
			if (player != null)
			{
				medalInfo = RankMgr.Get().GetRankedMedalFromRankPresenceField(player);
			}
		}
		m_medalInfo = medalInfo;
	}

	public void UpdateDisplayInfo()
	{
		UpdateName();
		UpdateRank();
		UpdateSessionRecord();
		if (IsBnetPlayer() && !IsLocalUser())
		{
			BnetPlayer player = BnetPresenceMgr.Get().GetPlayer(m_gameAccountId);
			if (player != null && BnetFriendMgr.Get().IsFriend(player))
			{
				ChatMgr.Get().AddRecentWhisperPlayerToBottom(player);
			}
		}
	}

	private void UpdateSessionRecord()
	{
		BnetPlayer player = BnetPresenceMgr.Get().GetPlayer(m_gameAccountId);
		if (player == null)
		{
			return;
		}
		BnetGameAccount hsGameAccount = player.GetHearthstoneGameAccount();
		if (hsGameAccount == null)
		{
			return;
		}
		SessionRecord record = hsGameAccount.GetSessionRecord();
		if (record != null)
		{
			if (record.SessionRecordType == SessionRecordType.ARENA)
			{
				m_arenaWins = record.Wins;
				m_arenaLoss = record.Losses;
			}
			else if (record.SessionRecordType == SessionRecordType.TAVERN_BRAWL)
			{
				m_tavernBrawlWins = record.Wins;
				m_tavernBrawlLoss = record.Losses;
			}
			else if (record.SessionRecordType == SessionRecordType.DUELS)
			{
				m_duelsWins = record.Wins;
				m_duelsLoss = record.Losses;
			}
		}
	}

	private void OnBnetPlayersChanged(BnetPlayerChangelist changelist, object userData)
	{
		if (changelist.FindChange(m_gameAccountId) != null && IsDisplayable())
		{
			BnetPresenceMgr.Get().RemovePlayersChangedListener(OnBnetPlayersChanged);
			UpdateDisplayInfo();
		}
	}

	private void UpdateLocal()
	{
		if (GameMgr.Get() != null && SpectatorManager.Get().IsSpectatingOrWatching)
		{
			m_local = false;
		}
		else if (IsBnetPlayer())
		{
			BnetGameAccountId myGameAccountId = BnetPresenceMgr.Get().GetMyGameAccountId();
			m_local = myGameAccountId == m_gameAccountId;
		}
		else
		{
			m_local = m_gameAccountId.Low == 1;
		}
	}

	public void UpdateSide(int friendlySideTeamId)
	{
		if (GetTeamId() == friendlySideTeamId)
		{
			m_side = Side.FRIENDLY;
			GameState.Get().RegisterOptionsReceivedListener(OnFriendlyOptionsReceived);
			GameState.Get().RegisterOptionsSentListener(OnFriendlyOptionsSent);
			GameState.Get().RegisterFriendlyTurnStartedListener(OnFriendlyTurnStarted);
		}
		else
		{
			m_side = Side.OPPOSING;
		}
	}

	private void AssignPlayerBoardObjects()
	{
		if (!IsTeamLeader())
		{
			return;
		}
		ManaCounter[] componentsInChildren = Gameplay.Get().GetBoardLayout().gameObject.GetComponentsInChildren<ManaCounter>(includeInactive: true);
		foreach (ManaCounter counter in componentsInChildren)
		{
			if (counter.m_Side == m_side)
			{
				m_manaCounter = counter;
				m_manaCounter.SetPlayer(this);
				m_manaCounter.UpdateText();
				break;
			}
		}
		InitManaCrystalMgr();
		InitCorpseCounter();
		if (MulliganManager.Get() != null && !MulliganManager.Get().IsMulliganIntroActive())
		{
			AnomalyMedallion.Initialize();
			AnomalyMedallion.Close();
		}
		foreach (Zone zone in ZoneMgr.Get().GetZones())
		{
			if (zone.m_Side == m_side)
			{
				if (IsFriendlySide() && zone.m_ServerTag == TAG_ZONE.HAND)
				{
					zone.SetController(GameState.Get().GetLocalSidePlayer());
				}
				else
				{
					zone.SetController(this);
				}
			}
		}
	}

	private void InitManaCrystalMgr()
	{
		if (IsFriendlySide())
		{
			int temporaryResources = GetTag(GAME_TAG.TEMP_RESOURCES);
			int permanentResources = GetTag(GAME_TAG.RESOURCES);
			int usedResources = GetTag(GAME_TAG.RESOURCES_USED);
			int overloadOwed = GetTag(GAME_TAG.OVERLOAD_OWED);
			int overloadLocked = GetTag(GAME_TAG.OVERLOAD_LOCKED);
			ManaCrystalMgr.Get().AddManaCrystals(permanentResources, isTurnStart: false);
			ManaCrystalMgr.Get().AddTempManaCrystals(temporaryResources);
			ManaCrystalMgr.Get().UpdateSpentMana(usedResources);
			ManaCrystalMgr.Get().MarkCrystalsOwedForOverload(overloadOwed);
			ManaCrystalMgr.Get().SetCrystalsLockedForOverload(overloadLocked);
			ManaCrystalMgr.Get().ResetUnresolvedManaToBeReadied();
		}
	}

	private void InitCorpseCounter()
	{
		if (IsFriendlySide())
		{
			CorpseCounter.InitializeAll();
		}
	}

	private void OnTurnChanged(int oldTurn, int newTurn, object userData)
	{
		WipeZzzs();
		UpdateChooseBoth();
	}

	private void OnFriendlyOptionsReceived(object userData)
	{
		UpdateChooseBoth();
	}

	private void OnFriendlyOptionsSent(Network.Options.Option option, object userData)
	{
		UpdateChooseBoth();
		Entity selectedEntity = GameState.Get().GetEntity(option.Main.ID);
		CancelAllProposedMana(selectedEntity);
	}

	private void OnFriendlyTurnStarted(object userData)
	{
		UpdateChooseBoth();
	}

	private Spell ToggleActorSpellOnCard(Card card, TagDelta change, SpellType spellType)
	{
		if (card == null)
		{
			return null;
		}
		if (!card.CanShowActorVisuals())
		{
			return null;
		}
		Actor actor = card.GetActor();
		if (change.newValue > 0)
		{
			return actor.ActivateSpellBirthState(spellType);
		}
		actor.ActivateSpellDeathState(spellType);
		return null;
	}

	private void UpdateHandCardPowersText(bool onlySpells)
	{
		List<Card> cards = GetHandZone().GetCards();
		for (int i = 0; i < cards.Count; i++)
		{
			Card card = cards[i];
			if (!(card.GetActor() == null) && (!onlySpells || card.GetEntity().IsSpell()))
			{
				card.GetActor().UpdatePowersText();
			}
		}
	}

	private void UpdateHeroPowerBigCard()
	{
		if (BigCard.Get() != null)
		{
			Actor bigCardActor = BigCard.Get().GetBigCardActor();
			if (bigCardActor != null && bigCardActor.GetEntity() == GetHeroPower())
			{
				bigCardActor.UpdatePowersText();
			}
		}
	}

	private void UpdateSpellsCostHealth(TagDelta change)
	{
		if (change.oldValue == change.newValue)
		{
			return;
		}
		if (IsFriendlySide())
		{
			Card mousedOverCard = InputManager.Get().GetMousedOverCard();
			if (mousedOverCard != null)
			{
				Entity mousedOverEntity = mousedOverCard.GetEntity();
				if (mousedOverEntity.IsSpell())
				{
					if (change.newValue > 0)
					{
						ManaCrystalMgr.Get().CancelAllProposedMana(mousedOverEntity);
					}
					else
					{
						ManaCrystalMgr.Get().ProposeManaCrystalUsage(mousedOverEntity);
					}
				}
			}
		}
		List<Card> cards = GetHandZone().GetCards();
		for (int i = 0; i < cards.Count; i++)
		{
			Card card = cards[i];
			if (!card.CanShowActorVisuals())
			{
				continue;
			}
			Entity entity = card.GetEntity();
			if (entity.IsSpell() && entity.GetTag<TAG_CARD_ALTERNATE_COST>(GAME_TAG.CARD_ALTERNATE_COST) != TAG_CARD_ALTERNATE_COST.HEALTH)
			{
				Actor actor = card.GetActor();
				if (change.newValue > 0)
				{
					actor.ActivateSpellBirthState(SpellType.SPELLS_COST_HEALTH);
				}
				else
				{
					actor.ActivateSpellDeathState(SpellType.SPELLS_COST_HEALTH);
				}
			}
		}
	}

	private void UpdateChooseBoth()
	{
		List<Card> cards = GetHandZone().GetCards();
		for (int i = 0; i < cards.Count; i++)
		{
			Card card = cards[i];
			UpdateChooseBoth(card);
		}
		UpdateChooseBoth(GetHeroPowerCard());
	}

	private void UpdateChooseBoth(Card card)
	{
		if (card == null || !card.CanShowActorVisuals())
		{
			return;
		}
		Entity entity = card.GetEntity();
		if (entity.HasTag(GAME_TAG.CHOOSE_ONE))
		{
			Actor actor = card.GetActor();
			SpellType spellType = SpellType.CHOOSE_BOTH;
			if ((entity.HasTag(GAME_TAG.CHOOSE_BOTH) || HasTag(GAME_TAG.CHOOSE_BOTH)) && GameState.Get().IsValidOption(entity, null))
			{
				SpellUtils.ActivateBirthIfNecessary(actor.GetSpell(spellType));
			}
			else
			{
				SpellUtils.ActivateDeathIfNecessary(actor.GetSpellIfLoaded(spellType));
			}
		}
	}
}
