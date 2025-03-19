using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Game.Spells;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Progression;
using HutongGames.PlayMaker;
using PegasusGame;
using UnityEngine;

public class Card : MonoBehaviour
{
	private class PrefabLoadRequest
	{
		public string m_path;

		public PrefabCallback<GameObject> m_loadCallback;
	}

	public delegate void EmotePlayCallback(EmoteType emoteType);

	public enum AnnouncerLineType
	{
		DEFAULT,
		BEFORE_VERSUS,
		AFTER_VERSUS,
		MAX
	}

	private delegate void ActivateGraveyardActorDeathSpellAfterDelayCallback();

	public static readonly Vector3 ABOVE_DECK_OFFSET = new Vector3(-0.3f, 3.6f, 0f);

	public static readonly Vector3 IN_DECK_OFFSET = new Vector3(0f, 0f, 0.1f);

	public static readonly Vector3 IN_DECK_SCALE = new Vector3(0.81f, 0.81f, 0.81f);

	public static readonly Vector3 IN_DECK_ANGLES = new Vector3(-90f, 270f, 0f);

	public static readonly Quaternion IN_DECK_ROTATION = Quaternion.Euler(IN_DECK_ANGLES);

	public static readonly Vector3 IN_DECK_HIDDEN_ANGLES = new Vector3(275f, 90f, 0f);

	public static readonly Quaternion IN_DECK_HIDDEN_ROTATION = Quaternion.Euler(IN_DECK_HIDDEN_ANGLES);

	public static readonly Vector3 EVOLUTION_CARD_OFFSET_RIGHT = new Vector3(2.5f, 0.7f, 0.5f);

	public static readonly Vector3 EVOLUTION_CARD_OFFSET_LEFT = new Vector3(-2.5f, 0.7f, 0.5f);

	public static readonly Vector3 EVOLUTION_CARD_OFFSET_RIGHT_2 = new Vector3(4.9f, 0.7f, 0.5f);

	public static readonly Vector3 EVOLUTION_CARD_OFFSET_LEFT_2 = new Vector3(-4.9f, 0.7f, 0.5f);

	public const float DEFAULT_KEYWORD_DEATH_DELAY_SEC = 0.3f;

	protected Entity m_entity;

	protected DefLoader.DisposableCardDef m_cardDef;

	protected CardEffect m_playEffect;

	protected List<CardEffect> m_additionalPlayEffects;

	protected CardEffect m_attackEffect;

	protected CardEffect m_deathEffect;

	protected CardEffect m_lifetimeEffect;

	protected List<CardEffect> m_subOptionEffects;

	protected List<List<CardEffect>> m_additionalSubOptionEffects;

	protected List<CardEffect> m_triggerEffects;

	protected List<CardEffect> m_resetGameEffects;

	private const string BGQuestDescComponent = "Card_Hand_BG_Quest_Text_Tray_Mesh";

	private const string DefaultDescComponent = "Description_mesh";

	private const string NonBGQuestComponent = "NonQuestObjects";

	public const string BGRewardVFX = "BGRewardVFX";

	protected Map<Network.HistBlockStart, CardEffect> m_proxyEffects;

	protected List<CardEffect> m_allEffects;

	protected CardEffect m_customKeywordEffect;

	protected CardEffect m_customChoiceRevealEffect;

	protected CardEffect m_customChoiceConcealEffect;

	protected Map<SpellType, CardEffect> m_spellTableOverrideEffects = new Map<SpellType, CardEffect>();

	protected CardSound[] m_announcerLine = new CardSound[3];

	protected List<EmoteEntry> m_emotes;

	protected ISpell m_customSummonSpell;

	protected ISpell m_customSpawnSpell;

	protected ISpell m_customSpawnSpellOverride;

	protected ISpell m_customDeathSpell;

	protected ISpell m_customDeathSpellOverride;

	protected ISpell m_customDiscardSpell;

	protected ISpell m_customDiscardSpellOverride;

	private int m_spellLoadCount;

	protected string m_actorPath;

	protected Actor m_actor;

	protected Actor m_actorWaitingToBeReplaced;

	private bool m_actorReady = true;

	private bool m_actorLoading;

	private bool m_transitioningZones;

	private bool m_hasBeenGrabbedByEnemyActionHandler;

	private Zone m_zone;

	private Zone m_prevZone;

	private bool m_goingThroughDeathrattleReturnfromGraveyard;

	private int m_zonePosition;

	private int m_predictedZonePosition;

	public ZonePositionChange m_minionWasMovedFromSrcToDst;

	private bool m_doNotSort;

	private bool m_beingDrawnByOpponent;

	private bool m_cardStandInInteractive = true;

	private ZoneTransitionStyle m_transitionStyle;

	private bool m_doNotWarpToNewZone;

	private float m_transitionDelay;

	protected bool m_shouldShowTooltip;

	protected bool m_alwaysAllowTooltip;

	protected bool m_showTooltip;

	protected bool m_overPlayfield;

	protected MoveMinionHoverTarget m_overMoveMinionTarget;

	protected bool m_mousedOver;

	protected bool m_mousedOverByOpponent;

	protected bool m_mousedOverByTeammate;

	protected bool m_shown = true;

	private bool m_inputEnabled = true;

	protected bool m_attacking;

	protected bool m_moving;

	private int m_activeDeathEffectCount;

	private bool m_ignoreDeath;

	private bool m_suppressDeathEffects;

	private bool m_suppressDeathSounds;

	private bool m_suppressKeywordDeaths;

	private bool m_suppressHandToDeckTransition;

	private float m_keywordDeathDelaySec = 0.3f;

	private bool m_suppressActorTriggerSpell;

	private int m_suppressPlaySoundCount;

	private bool m_isBattleCrySource;

	private bool m_secretTriggered;

	private bool m_secretSheathed;

	private bool m_isBaubleAnimating;

	private ISpell m_activeSpawnSpell;

	private Player.Side? m_playZoneBlockerSide;

	private float m_delayBeforeHideInNullZoneVisuals;

	private DisplayCardsInToolip m_cardsInTooltip;

	private MagneticPlayData m_magneticPlayData;

	private bool m_magneticTarget;

	private ZoneChange m_latestZoneChange;

	private bool m_skipMilling;

	private int m_cardDrawTracker;

	private float? m_drawTimeScale;

	private const int DRAW_FAST_THRESHOLD_START = 3;

	private const int DRAW_FAST_THRESHOLD_MAX = 6;

	private const float NORMAL_DRAW_TIME_SCALE = 1f;

	private const float FAST_DRAW_TIME_SCALE = 0.556f;

	private bool m_disableHeroPowerFlipSoundOnce;

	private int m_lettuceAbilityActionOrder = -1;

	private bool m_lettuceAbilityActionOrderIsTied;

	private Actor m_questRewardActor;

	public Actor m_evolutionCardActor;

	private Actor m_evolutionCardActor2;

	private bool m_questRewardChanged;

	private bool m_alwaysShowCardsInTooltip;

	public bool IsBeingDragged { get; set; }

	public bool HasCardDef => m_cardDef?.CardDef != null;

	public bool HasHiddenCardDef => m_cardDef?.CardDef is HiddenCard;

	public string CustomHeroPhoneManaGem
	{
		get
		{
			if (!HasCardDef)
			{
				return null;
			}
			return m_cardDef.CardDef.m_CustomHeroPhoneManaGem;
		}
	}

	public string CustomHeroTray
	{
		get
		{
			if (!HasCardDef)
			{
				return null;
			}
			return m_cardDef.CardDef.m_CustomHeroTray;
		}
	}

	public string CustomHeroTrayGolden
	{
		get
		{
			if (!HasCardDef)
			{
				return null;
			}
			return m_cardDef.CardDef.m_CustomHeroTrayGolden;
		}
	}

	public string CustomHeroPhoneTray
	{
		get
		{
			if (!HasCardDef)
			{
				return null;
			}
			return m_cardDef.CardDef.m_CustomHeroPhoneTray;
		}
	}

	public bool DisablePremiumHeroTray
	{
		get
		{
			if (!HasCardDef)
			{
				return false;
			}
			return m_cardDef.CardDef.m_DisablePremiumHeroTray;
		}
	}

	public ref string DiamondCustomSpawnSpellPath => ref m_cardDef.CardDef.m_DiamondCustomSpawnSpellPath;

	public ref string GoldenCustomSpawnSpellPath => ref m_cardDef.CardDef.m_GoldenCustomSpawnSpellPath;

	public ref string CustomSpawnSpellPath => ref m_cardDef.CardDef.m_CustomSpawnSpellPath;

	public ref string DiamondCustomSummonSpellPath => ref m_cardDef.CardDef.m_DiamondCustomSummonSpellPath;

	public ref string GoldenCustomSummonSpellPath => ref m_cardDef.CardDef.m_GoldenCustomSummonSpellPath;

	public ref string CustomSummonSpellPath => ref m_cardDef.CardDef.m_CustomSummonSpellPath;

	public BaconLHSConfig LegendaryHeroSkinConfig
	{
		get
		{
			if (!HasCardDef)
			{
				return null;
			}
			return m_cardDef.CardDef.m_LegendaryHeroSkinConfig;
		}
	}

	public List<Board.CustomTraySettings> CustomHeroTraySettings
	{
		get
		{
			if (!HasCardDef)
			{
				return null;
			}
			return m_cardDef.CardDef.m_CustomHeroTraySettings;
		}
	}

	public string HeroFrameFriendlyPath
	{
		get
		{
			if (!HasCardDef)
			{
				return null;
			}
			return m_cardDef.CardDef.m_HeroFrameFriendlyPath;
		}
	}

	public string HeroFrameEnemyPath
	{
		get
		{
			if (!HasCardDef)
			{
				return null;
			}
			return m_cardDef.CardDef.m_HeroFrameEnemyPath;
		}
	}

	public static event Action<Card> HandToPlaySummonOutStart;

	public event EmotePlayCallback OnEmotePlayCallback;

	public override string ToString()
	{
		if (m_entity != null)
		{
			return m_entity.ToString();
		}
		return "UNKNOWN CARD";
	}

	public Entity GetEntity()
	{
		return m_entity;
	}

	public void SetEntity(Entity entity)
	{
		m_entity = entity;
	}

	public void Destroy()
	{
		if (m_actor != null)
		{
			m_actor.Destroy();
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public Player GetController()
	{
		if (m_entity == null)
		{
			return null;
		}
		return m_entity.GetController();
	}

	public Player.Side GetControllerSide()
	{
		if (m_entity == null)
		{
			return Player.Side.NEUTRAL;
		}
		return m_entity.GetControllerSide();
	}

	public Entity GetHero()
	{
		return GetController()?.GetHero();
	}

	public Card GetHeroCard()
	{
		return GetHero()?.GetCard();
	}

	public Entity GetHeroPower()
	{
		return GetController()?.GetHeroPower();
	}

	public Card GetHeroPowerCard()
	{
		return GetHeroPower()?.GetCard();
	}

	public TAG_PREMIUM GetPremium()
	{
		if (m_entity == null)
		{
			return TAG_PREMIUM.NORMAL;
		}
		return m_entity.GetPremiumType();
	}

	public bool IsOverPlayfield()
	{
		return m_overPlayfield;
	}

	public void NotifyOverPlayfield()
	{
		m_overPlayfield = true;
		UpdateActorState();
	}

	public void NotifyLeftPlayfield()
	{
		m_overPlayfield = false;
		UpdateActorState();
	}

	public bool IsOverMoveMinionTarget()
	{
		return m_overMoveMinionTarget != null;
	}

	public void NotifyOverMoveMinionTarget(MoveMinionHoverTarget target)
	{
		m_overMoveMinionTarget = target;
		UpdateActorState();
	}

	public void NotifyLeftMoveMinionTarget()
	{
		m_overMoveMinionTarget = null;
		UpdateActorState();
	}

	public void NotifyOfWeaponPlayed(Entity source)
	{
		this.ActivateLegendaryHeroAnimEvent(LegendaryHeroAnimations.WeaponCardPlayed);
	}

	public void NotifyOfWeaponDestroyed(Entity source)
	{
		this.ActivateLegendaryHeroAnimEvent(LegendaryHeroAnimations.WeaponCardDestroyed);
	}

	public void NotifyOfWeaponSheathed(Entity source)
	{
		this.ActivateLegendaryHeroAnimEvent(LegendaryHeroAnimations.WeaponSheathed);
	}

	public void NotifyOfWeaponUnsheathed(Entity source)
	{
		this.ActivateLegendaryHeroAnimEvent(LegendaryHeroAnimations.WeaponUnsheathed);
	}

	public void NotifyOfSpellPlayed(Entity source, Entity target)
	{
		this.ActivateLegendaryHeroAnimEvent(LegendaryHeroAnimations.SpellCard);
	}

	public void NotifyOfHeroPowerPlayed(Entity source, Entity target)
	{
		if (source.GetTag(GAME_TAG.IS_ALTERNATE_HEROPOWER) == 1)
		{
			this.ActivateLegendaryHeroAnimEvent(LegendaryHeroAnimations.HeroPowerAlt);
		}
		else
		{
			this.ActivateLegendaryHeroAnimEvent(LegendaryHeroAnimations.HeroPower);
		}
	}

	public void NotifyOfVictoryStrikePlayed(Entity source, Entity target)
	{
		this.ActivateLegendaryHeroAnimEvent(LegendaryHeroAnimations.VictoryStrike);
	}

	public void NotifyOfDefeatStrikePlayed(Entity source, Entity target)
	{
		this.ActivateLegendaryHeroAnimEvent(LegendaryHeroAnimations.DefeatStrike);
	}

	public void OnDestroy()
	{
		ReleaseAssets();
		if (m_mousedOver && GameState.Get() != null && !(InputManager.Get() == null))
		{
			InputManager.Get().NotifyCardDestroyed(this);
		}
	}

	private bool SetupEvolutionCards(ref Actor evolutionActor, GAME_TAG evolutionTag, Vector3 localPosLeft, Vector3 localPosRight)
	{
		string cardID = GameUtils.TranslateDbIdToCardId(m_entity.GetTag(evolutionTag));
		if (string.IsNullOrEmpty(cardID))
		{
			return false;
		}
		if (evolutionActor == null || evolutionActor.GetEntityDef().GetCardId() != cardID)
		{
			if (evolutionActor != null)
			{
				evolutionActor.Destroy();
			}
			using DefLoader.DisposableCardDef evolutionCardDef = DefLoader.Get().GetCardDef(cardID);
			if (evolutionCardDef == null)
			{
				Log.Gameplay.PrintError("Card.NotifyMousedOver(): Unable to load CardDef for card ID {0}.", cardID);
				return false;
			}
			EntityDef evolutionCardEntityDef = DefLoader.Get().GetEntityDef(cardID);
			if (evolutionCardEntityDef == null)
			{
				Log.Gameplay.PrintError("Card.NotifyMousedOver(): Unable to load EntityDef for card ID {0}.", cardID);
				return false;
			}
			GameObject evolutionCardActorGO = AssetLoader.Get().InstantiatePrefab(ActorNames.GetHandActor(evolutionCardEntityDef, GetEntity().GetPremiumType()), AssetLoadingOptions.IgnorePrefabPosition);
			if (evolutionCardActorGO == null)
			{
				Log.Gameplay.PrintError("Card.NotifyMousedOver(): Unable to load Hand Actor for entity def {0}.", evolutionCardEntityDef);
				return false;
			}
			LayerUtils.SetLayer(evolutionCardActorGO, base.gameObject.layer, null);
			evolutionCardActorGO.transform.parent = base.gameObject.transform;
			TransformUtil.Identity(evolutionCardActorGO);
			evolutionActor = evolutionCardActorGO.GetComponentInChildren<Actor>();
			evolutionActor.SetEntityDef(evolutionCardEntityDef);
			evolutionActor.SetCardDef(evolutionCardDef);
			evolutionActor.SetPremium(GetEntity().GetPremiumType());
			evolutionActor.SetWatermarkCardSetOverride(GetEntity().GetWatermarkCardSetOverride());
			evolutionActor.UpdateDynamicTextFromQuestEntity(GetEntity());
			if (evolutionActor.UseTechLevelManaGem())
			{
				Spell techLevelSpell = evolutionActor.GetSpell(SpellType.TECH_LEVEL_MANA_GEM);
				if (techLevelSpell != null)
				{
					techLevelSpell.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmInt("TechLevel").Value = evolutionCardEntityDef.GetTechLevel();
					techLevelSpell.ActivateState(SpellStateType.BIRTH);
				}
			}
			else if (evolutionActor.UseCoinManaGem())
			{
				evolutionActor.ActivateSpellBirthState(SpellType.COIN_MANA_GEM);
			}
			if (evolutionCardEntityDef.IsBaconSpell())
			{
				evolutionActor.SetShowCostOverride(-1);
			}
			GameObject EvolutionVFX = GameObjectUtils.FindChildBySubstring(evolutionCardActorGO, "EvolutionVFX");
			if (EvolutionVFX != null)
			{
				EvolutionVFX.SetActive(value: true);
			}
			if (m_actor.IsOnRightSideOfZonePlay())
			{
				evolutionActor.transform.localPosition = localPosLeft;
			}
			else
			{
				evolutionActor.transform.localPosition = localPosRight;
			}
			evolutionActor.UpdateAllComponents();
		}
		else if (evolutionActor != null)
		{
			evolutionActor.gameObject.SetActive(value: true);
		}
		return true;
	}

	public void NotifyMousedOver()
	{
		m_mousedOver = true;
		UpdateActorState();
		UpdateProposedManaUsage();
		if ((bool)RemoteActionHandler.Get() && (bool)TargetReticleManager.Get())
		{
			RemoteActionHandler.Get().NotifyOpponentOfMouseOverEntity(GetEntity().GetCard());
		}
		if (GameState.Get() != null)
		{
			GameState.Get().GetGameEntity().NotifyOfCardMousedOver(GetEntity());
		}
		if (m_zone is ZoneHand)
		{
			Spell spellPowerBurstSpell = GetActorSpell(SpellType.SPELL_POWER_HINT_BURST);
			if (spellPowerBurstSpell != null)
			{
				spellPowerBurstSpell.Deactivate();
			}
			Spell spellPowerIdleSpell = GetActorSpell(SpellType.SPELL_POWER_HINT_IDLE);
			if (spellPowerIdleSpell != null)
			{
				spellPowerIdleSpell.Deactivate();
			}
			Spell healingDamageBurstSpell = GetActorSpell(SpellType.HEALING_DOES_DAMAGE_HINT_BURST);
			if (healingDamageBurstSpell != null)
			{
				healingDamageBurstSpell.Deactivate();
			}
			Spell healingDamageIdleSpell = GetActorSpell(SpellType.HEALING_DOES_DAMAGE_HINT_IDLE);
			if (healingDamageIdleSpell != null)
			{
				healingDamageIdleSpell.Deactivate();
			}
			GetActorSpell(SpellType.LIFESTEAL_DOES_DAMAGE_HINT_IDLE);
			if (healingDamageIdleSpell != null)
			{
				healingDamageIdleSpell.Deactivate();
			}
			if (GameState.Get() != null && GameState.Get().IsMulliganManagerActive())
			{
				SoundManager.Get().LoadAndPlay("collection_manager_card_mouse_over.prefab:0d4e20bc78956bc48b5e2963ec39211c", base.gameObject);
			}
		}
		if (ShouldShowCardsInTooltip())
		{
			m_cardsInTooltip.NotifyMousedOver();
		}
		if (m_entity.IsControlledByFriendlySidePlayer() && (m_entity.IsHero() || m_zone is ZonePlay) && !m_transitioningZones)
		{
			bool hasSpellPower = m_entity.HasSpellPower() || m_entity.HasSpellPowerDouble();
			bool hasHeroPowerDamage = m_entity.HasHeroPowerDamage();
			if (hasSpellPower || hasHeroPowerDamage)
			{
				Spell spell = GetActorSpell(SpellType.SPELL_POWER_HINT_BURST);
				if (spell != null)
				{
					spell.Reactivate();
				}
				if (hasSpellPower)
				{
					ZoneMgr.Get().OnSpellPowerEntityMousedOver(m_entity.GetSpellPowerSchool());
				}
			}
			if (m_entity.HasHealingDoesDamageHint())
			{
				Spell spell2 = GetActorSpell(SpellType.HEALING_DOES_DAMAGE_HINT_BURST);
				if (spell2 != null)
				{
					spell2.Reactivate();
				}
				ZoneMgr.Get().OnHealingDoesDamageEntityMousedOver();
			}
			if (m_entity.HasLifestealDoesDamageHint())
			{
				Spell spell3 = GetActorSpell(SpellType.HEALING_DOES_DAMAGE_HINT_BURST);
				if (spell3 != null)
				{
					spell3.Reactivate();
				}
				ZoneMgr.Get().OnLifestealDoesDamageEntityMousedOver();
			}
		}
		if (m_entity.IsControlledByFriendlySidePlayer() && m_entity.HasTag(GAME_TAG.BACON_DIED_LAST_COMBAT_HINT))
		{
			ZoneMgr.Get().OnDiedLastCombatMousedOver();
		}
		if (m_entity.IsWeapon() && m_entity.IsExhausted() && m_actor != null && m_actor.GetAttackObject() != null)
		{
			m_actor.GetAttackObject().Enlarge(1f);
		}
		if (m_entity.IsQuest() && m_zone is ZoneSecret)
		{
			QuestController questController = m_actor.GetComponent<QuestController>();
			if (questController != null)
			{
				questController.NotifyMousedOver();
			}
		}
		if (m_entity.IsQuestline() && m_zone is ZoneSecret)
		{
			QuestlineController questlineController = m_actor.GetComponent<QuestlineController>();
			if (questlineController != null)
			{
				questlineController.NotifyMousedOver();
			}
		}
		if (m_entity.IsPuzzle() && m_zone is ZoneSecret)
		{
			PuzzleController puzzleController = m_actor.GetComponent<PuzzleController>();
			if (puzzleController != null)
			{
				puzzleController.NotifyMousedOver();
			}
		}
		if (m_entity.IsRulebook() && m_zone is ZoneSecret)
		{
			RulebookController rulebookController = m_actor.GetComponent<RulebookController>();
			if (rulebookController != null)
			{
				rulebookController.NotifyMousedOver();
			}
		}
		if (!(m_zone is ZoneSecret) && !IsAllowedToShowTooltip() && m_entity != null && SetupEvolutionCards(ref m_evolutionCardActor, GAME_TAG.BACON_EVOLUTION_CARD_ID, EVOLUTION_CARD_OFFSET_LEFT, EVOLUTION_CARD_OFFSET_RIGHT))
		{
			SetupEvolutionCards(ref m_evolutionCardActor2, GAME_TAG.BACON_EVOLUTION_CARD_ID_2, EVOLUTION_CARD_OFFSET_LEFT_2, EVOLUTION_CARD_OFFSET_RIGHT_2);
		}
		if (!m_entity.IsLettuceAbility())
		{
			return;
		}
		Entity selectedMinionEntity = ZoneMgr.Get().GetLettuceAbilitiesSourceEntity();
		if (selectedMinionEntity == null)
		{
			return;
		}
		if (GameState.Get().IsValidOption(m_entity, null))
		{
			int fakeMinionDbId = m_entity.GetTag(GAME_TAG.LETTUCE_ABILITY_SUMMONED_MINION);
			if (fakeMinionDbId > 0)
			{
				Spell previewSpell = m_actor.GetSpell(SpellType.LETTUCE_ABILITY_SUMMON_PREVIEW);
				if (previewSpell != null)
				{
					int fakeMinionZonePosition = selectedMinionEntity.GetZonePosition() + 1;
					PlayMakerFSM component = previewSpell.gameObject.GetComponent<PlayMakerFSM>();
					component.FsmVariables.GetFsmInt("FakeMinionZonePosition").Value = fakeMinionZonePosition;
					component.FsmVariables.GetFsmInt("FakeMinionDbId").Value = fakeMinionDbId;
					component.FsmVariables.GetFsmInt("FakeMinionAttack").Value = m_entity.GetATK();
					component.FsmVariables.GetFsmInt("FakeMinionHealth").Value = m_entity.GetHealth();
					component.FsmVariables.GetFsmVector3("FakeMinionScale").Value = selectedMinionEntity.GetCard()?.transform.localScale ?? Vector3.one;
					previewSpell.ActivateState(SpellStateType.BIRTH);
				}
			}
		}
		if (m_actor is LettuceAbilityActor lettuceAbilityActor)
		{
			lettuceAbilityActor.PlayMousedOverSound();
		}
	}

	public void NotifyMousedOut(bool ignoreTooltips = false)
	{
		m_mousedOver = false;
		UpdateActorState();
		UpdateProposedManaUsage();
		if ((bool)RemoteActionHandler.Get())
		{
			RemoteActionHandler.Get().NotifyOpponentOfMouseOut();
		}
		if (!ignoreTooltips && (bool)TooltipPanelManager.Get())
		{
			TooltipPanelManager.Get().HideKeywordHelp();
		}
		if ((bool)CardTypeBanner.Get())
		{
			CardTypeBanner.Get().Hide(this);
		}
		if (GameState.Get() != null)
		{
			GameState.Get().GetGameEntity().NotifyOfCardMousedOff(GetEntity());
		}
		if (m_entity.IsControlledByFriendlySidePlayer() && (m_entity.IsHero() || m_zone is ZonePlay))
		{
			if (m_entity.HasSpellPower())
			{
				ZoneMgr.Get().OnSpellPowerEntityMousedOut(m_entity.GetSpellPowerSchool());
			}
			if (m_entity.HasHealingDoesDamageHint())
			{
				ZoneMgr.Get().OnHealingDoesDamageEntityMousedOut();
			}
			if (m_entity.HasLifestealDoesDamageHint())
			{
				ZoneMgr.Get().OnLifestealDoesDamageEntityMousedOut();
			}
		}
		if (m_entity.IsControlledByFriendlySidePlayer() && m_entity.HasTag(GAME_TAG.BACON_DIED_LAST_COMBAT_HINT))
		{
			ZoneMgr.Get().OnDiedLastCombatMousedOut();
		}
		if (m_entity.IsWeapon() && m_entity.IsExhausted() && m_actor != null && m_actor.GetAttackObject() != null)
		{
			m_actor.GetAttackObject().ScaleToZero();
		}
		if (m_entity.IsQuest() && (m_zone is ZoneSecret || m_prevZone is ZoneSecret))
		{
			QuestController questController = m_actor.GetComponent<QuestController>();
			if (questController != null)
			{
				questController.NotifyMousedOut();
			}
		}
		if (m_entity.IsQuestline() && m_zone is ZoneSecret)
		{
			QuestlineController questlineController = m_actor.GetComponent<QuestlineController>();
			if (questlineController != null)
			{
				questlineController.NotifyMousedOut();
			}
		}
		if (m_entity.IsPuzzle() && m_zone is ZoneSecret && m_actor != null)
		{
			PuzzleController puzzleController = m_actor.GetComponent<PuzzleController>();
			if (puzzleController != null)
			{
				puzzleController.NotifyMousedOut();
			}
		}
		if (m_entity.IsRulebook() && m_zone is ZoneSecret && m_actor != null)
		{
			RulebookController rulebookController = m_actor.GetComponent<RulebookController>();
			if (rulebookController != null)
			{
				rulebookController.NotifyMousedOut();
			}
		}
		if (m_entity.IsLettuceAbility() && m_entity.HasTag(GAME_TAG.LETTUCE_ABILITY_SUMMONED_MINION))
		{
			m_actor.ActivateSpellDeathState(SpellType.LETTUCE_ABILITY_SUMMON_PREVIEW);
		}
		if (m_evolutionCardActor != null)
		{
			m_evolutionCardActor.gameObject.SetActive(value: false);
		}
		if (m_evolutionCardActor2 != null)
		{
			m_evolutionCardActor2.gameObject.SetActive(value: false);
		}
		if (!ignoreTooltips && m_cardsInTooltip != null)
		{
			m_cardsInTooltip.NotifyMousedOut();
		}
	}

	public void ShowWeaknessSplat()
	{
		Spell spell = m_actor?.GetSpell(SpellType.WEAKNESS_SPLAT);
		if (spell != null && spell.GetActiveState() != SpellStateType.BIRTH)
		{
			spell.ActivateState(SpellStateType.BIRTH);
		}
	}

	public void HideWeaknessSplat()
	{
		Spell spell = m_actor?.GetSpellIfLoaded(SpellType.WEAKNESS_SPLAT);
		if (spell != null && spell.GetActiveState() == SpellStateType.BIRTH)
		{
			spell.ActivateState(SpellStateType.DEATH);
		}
	}

	public bool IsMousedOver()
	{
		return m_mousedOver;
	}

	public void NotifyOpponentMousedOverThisCard()
	{
		m_mousedOverByOpponent = true;
		UpdateActorState();
	}

	public void NotifyOpponentMousedOffThisCard()
	{
		m_mousedOverByOpponent = false;
		UpdateActorState();
	}

	public void NotifyTeammateMousedOverThisCard()
	{
		m_mousedOverByTeammate = true;
		UpdateActorState();
	}

	public void NotifyTeammateMousedOffThisCard()
	{
		m_mousedOverByTeammate = false;
		UpdateActorState();
	}

	public void NotifyPickedUp()
	{
		m_transitioningZones = false;
		if (GetZone() is ZoneHand)
		{
			CutoffFriendlyCardDraw();
		}
		if (ShouldShowCardsInTooltip())
		{
			m_cardsInTooltip.NotifyPickedUp();
		}
	}

	public void NotifyTargetingCanceled()
	{
		if (m_entity.IsCharacter() && !IsAttacking())
		{
			ISpell actorAttackSpell = GetActorAttackSpellForInput();
			if (actorAttackSpell != null)
			{
				if (!ShouldShowImmuneVisuals())
				{
					GetActor().ActivateSpellDeathState(SpellType.IMMUNE);
				}
				SpellStateType activeState = actorAttackSpell.GetActiveState();
				if (activeState != 0 && activeState != SpellStateType.CANCEL)
				{
					actorAttackSpell.ActivateState(SpellStateType.CANCEL);
				}
			}
		}
		ActivateHandStateSpells();
	}

	public bool IsInputEnabled()
	{
		if (m_entity != null)
		{
			if (m_entity.HasQueuedChangeEntity() && m_entity.GetCardType() != TAG_CARDTYPE.BATTLEGROUND_QUEST_REWARD)
			{
				return false;
			}
			if (m_entity.IsHeroPower() && m_entity.HasQueuedControllerTagChange())
			{
				return false;
			}
		}
		return m_inputEnabled;
	}

	public void SetInputEnabled(bool enabled)
	{
		m_inputEnabled = enabled;
		UpdateActorState();
	}

	public void SetAlwaysAllowTooptip(bool allow)
	{
		m_alwaysAllowTooltip = allow;
	}

	public bool IsAllowedToShowTooltip()
	{
		if (m_alwaysAllowTooltip)
		{
			return true;
		}
		if (m_zone == null)
		{
			return false;
		}
		if (m_zone.m_ServerTag != TAG_ZONE.PLAY && m_zone.m_ServerTag != TAG_ZONE.SECRET && m_zone.m_ServerTag == TAG_ZONE.HAND && m_zone.m_Side != Player.Side.OPPOSING)
		{
			return false;
		}
		if (GameState.Get() != null && m_entity.IsHero() && m_entity.GetZone() == TAG_ZONE.PLAY && !GameState.Get().GetBooleanGameOption(GameEntityOption.SHOW_HERO_TOOLTIPS))
		{
			return false;
		}
		if (m_entity.IsBobQuest())
		{
			return true;
		}
		if (m_entity.IsQuest() || m_entity.IsQuestline() || m_entity.IsPuzzle() || m_entity.IsRulebook())
		{
			return false;
		}
		return true;
	}

	public bool IsAbleToShowTooltip()
	{
		if (m_entity == null)
		{
			return false;
		}
		if (m_actor == null)
		{
			return false;
		}
		if (BigCard.Get() == null)
		{
			return false;
		}
		return true;
	}

	public bool GetShouldShowTooltip()
	{
		return m_shouldShowTooltip;
	}

	public void SetShouldShowTooltip()
	{
		if (IsAllowedToShowTooltip() && !m_shouldShowTooltip)
		{
			m_shouldShowTooltip = true;
		}
	}

	public void ShowTooltip()
	{
		if (!m_showTooltip)
		{
			m_showTooltip = true;
			UpdateTooltip();
		}
	}

	public void HideTooltip()
	{
		m_shouldShowTooltip = false;
		if (m_showTooltip)
		{
			m_showTooltip = false;
			UpdateTooltip();
		}
	}

	public bool IsShowingTooltip()
	{
		return m_showTooltip;
	}

	private void ShowMouseOverSpell()
	{
		if (m_entity == null || m_actor == null)
		{
			return;
		}
		if (m_entity.HasTag(GAME_TAG.VOODOO_LINK) || m_entity.DoEnchantmentsHaveTag(GAME_TAG.VOODOO_LINK))
		{
			Spell spell = m_actor.GetSpell(SpellType.VOODOO_LINK);
			if ((bool)spell)
			{
				spell.SetSource(base.gameObject);
				spell.Activate();
			}
		}
		string designCode = m_entity.GetCardId();
		if (designCode == MagtheridonLinkToHellfireWardersSpell.MagtheridonId || designCode == MagtheridonLinkToHellfireWardersSpell.HellfireWarderId)
		{
			Spell spell2 = m_actor.GetSpell(SpellType.MAGTHERIDON_LINK);
			if ((bool)spell2)
			{
				spell2.SetSource(base.gameObject);
				spell2.Activate();
			}
		}
		if (m_entity.HasTag(GAME_TAG.FAN_LINK) || m_entity.DoEnchantmentsHaveTag(GAME_TAG.FAN_LINK))
		{
			Spell spell3 = m_actor.GetSpell(SpellType.FAN_LINK);
			if ((bool)spell3)
			{
				spell3.SetSource(base.gameObject);
				spell3.Activate();
			}
		}
	}

	private void HideMouseOverSpell()
	{
		if (!(m_actor == null))
		{
			Spell spell = m_actor.GetSpellIfLoaded(SpellType.VOODOO_LINK);
			if ((bool)spell)
			{
				spell.Deactivate();
			}
			spell = m_actor.GetSpellIfLoaded(SpellType.MAGTHERIDON_LINK);
			if ((bool)spell)
			{
				spell.Deactivate();
			}
			spell = m_actor.GetSpellIfLoaded(SpellType.FAN_LINK);
			if ((bool)spell)
			{
				spell.Deactivate();
			}
		}
	}

	public void UpdateTooltip()
	{
		if (GetShouldShowTooltip() && IsAllowedToShowTooltip() && IsAbleToShowTooltip() && m_showTooltip)
		{
			ShowMouseOverSpell();
			if (BigCard.Get() != null)
			{
				BigCard.Get().Show(this);
			}
			return;
		}
		m_showTooltip = false;
		m_shouldShowTooltip = false;
		HideMouseOverSpell();
		if (BigCard.Get() != null)
		{
			BigCard.Get().Hide(this);
		}
	}

	public bool IsAttacking()
	{
		return m_attacking;
	}

	public void EnableAttacking(bool enable)
	{
		m_attacking = enable;
	}

	public bool IsMoving()
	{
		return m_moving;
	}

	public void EnableMoving(bool enable)
	{
		m_moving = enable;
	}

	public bool WillIgnoreDeath()
	{
		return m_ignoreDeath;
	}

	public void IgnoreDeath(bool ignore)
	{
		m_ignoreDeath = ignore;
	}

	public bool WillSuppressDeathEffects()
	{
		return m_suppressDeathEffects;
	}

	public void SuppressDeathEffects(bool suppress)
	{
		m_suppressDeathEffects = suppress;
	}

	public bool WillSuppressDeathSounds()
	{
		return m_suppressDeathSounds;
	}

	public void SuppressDeathSounds(bool suppress)
	{
		m_suppressDeathSounds = suppress;
	}

	public bool WillSuppressKeywordDeaths()
	{
		return m_suppressKeywordDeaths;
	}

	public void SuppressKeywordDeaths(bool suppress)
	{
		m_suppressKeywordDeaths = suppress;
	}

	public float GetKeywordDeathDelaySec()
	{
		return m_keywordDeathDelaySec;
	}

	public void SetKeywordDeathDelaySec(float sec)
	{
		m_keywordDeathDelaySec = sec;
	}

	public bool WillSuppressActorTriggerSpell()
	{
		return m_suppressActorTriggerSpell;
	}

	public void SuppressActorTriggerSpell(bool suppress)
	{
		m_suppressActorTriggerSpell = suppress;
	}

	public bool WillSuppressPlaySounds()
	{
		if ((GetEntity() != null && GetEntity().HasTag(GAME_TAG.SUPPRESS_ALL_SUMMON_VO)) || GetController().HasTag(GAME_TAG.SUPPRESS_SUMMON_VO_FOR_PLAYER))
		{
			if (GetEntity().GetTag(GAME_TAG.DONT_SUPPRESS_SUMMON_VO) == 1)
			{
				return false;
			}
			return true;
		}
		return m_suppressPlaySoundCount > 0;
	}

	public bool WillSuppressCustomSpells()
	{
		GameState gameState = GameState.Get();
		if ((gameState == null || !gameState.GetGameEntity().HasTag(GAME_TAG.FORCE_NO_CUSTOM_SPELLS)) && !GetController().HasTag(GAME_TAG.FORCE_NO_CUSTOM_SPELLS))
		{
			return GetEntity().HasTag(GAME_TAG.FORCE_NO_CUSTOM_SPELLS);
		}
		return true;
	}

	public bool WillSuppressCustomSummonSpells()
	{
		GameState gameState = GameState.Get();
		if ((gameState == null || !gameState.GetGameEntity().HasTag(GAME_TAG.FORCE_NO_CUSTOM_SUMMON_SPELLS)) && !GetController().HasTag(GAME_TAG.FORCE_NO_CUSTOM_SUMMON_SPELLS))
		{
			return GetEntity().HasTag(GAME_TAG.FORCE_NO_CUSTOM_SUMMON_SPELLS);
		}
		return true;
	}

	public bool WillSuppressCustomLifetimeSpells()
	{
		GameState gameState = GameState.Get();
		if ((gameState == null || !gameState.GetGameEntity().HasTag(GAME_TAG.FORCE_NO_CUSTOM_LIFETIME_SPELLS)) && !GetController().HasTag(GAME_TAG.FORCE_NO_CUSTOM_LIFETIME_SPELLS))
		{
			return GetEntity().HasTag(GAME_TAG.FORCE_NO_CUSTOM_LIFETIME_SPELLS);
		}
		return true;
	}

	public bool WillSuppressCustomKeywordSpells()
	{
		GameState gameState = GameState.Get();
		if ((gameState == null || !gameState.GetGameEntity().HasTag(GAME_TAG.FORCE_NO_CUSTOM_KEYWORD_SPELLS)) && !GetController().HasTag(GAME_TAG.FORCE_NO_CUSTOM_KEYWORD_SPELLS))
		{
			return GetEntity().HasTag(GAME_TAG.FORCE_NO_CUSTOM_KEYWORD_SPELLS);
		}
		return true;
	}

	public void SuppressPlaySounds(bool suppress)
	{
		if (suppress)
		{
			m_suppressPlaySoundCount++;
		}
		else if (--m_suppressPlaySoundCount < 0)
		{
			m_suppressPlaySoundCount = 0;
		}
	}

	public void SuppressHandToDeckTransition()
	{
		m_suppressHandToDeckTransition = true;
	}

	public bool IsShown()
	{
		return m_shown;
	}

	public void ShowCard()
	{
		if (!m_shown)
		{
			m_shown = true;
			ShowImpl();
		}
	}

	private void ShowImpl()
	{
		if (!(m_actor == null))
		{
			m_actor.Show();
			if (m_questRewardActor != null)
			{
				m_questRewardActor.Show();
			}
			RefreshActor();
		}
	}

	public void HideCard()
	{
		if (m_shown && !m_actorLoading)
		{
			m_shown = false;
			HideImpl();
		}
	}

	private void HideImpl()
	{
		if (!(m_actor == null))
		{
			m_actor.Hide();
			if (m_questRewardActor != null)
			{
				m_questRewardActor.Hide();
			}
		}
	}

	public void SetBattleCrySource(bool source)
	{
		m_isBattleCrySource = source;
		if (m_actor != null)
		{
			if (source)
			{
				LayerUtils.SetLayer(m_actor.gameObject, GameLayer.IgnoreFullScreenEffects);
				return;
			}
			LayerUtils.SetLayer(m_actor.gameObject, GameLayer.Default);
			LayerUtils.SetLayer(m_actor.GetMeshRenderer().gameObject, GameLayer.CardRaycast);
		}
	}

	public void DoTauntNotification()
	{
		if ((m_activeSpawnSpell == null || !m_activeSpawnSpell.IsActive()) && m_actor != null)
		{
			Entity entity = m_entity;
			if (entity == null || !entity.IsLaunchpad())
			{
				iTween.PunchScale(m_actor.gameObject, new Vector3(0.2f, 0.2f, 0.2f), 0.5f);
			}
		}
	}

	public void UpdateProposedManaUsage()
	{
		if (GameState.Get() == null || GameState.Get().GetSelectedOption() != -1)
		{
			return;
		}
		Player player = GameState.Get().GetPlayer(GetEntity().GetControllerId());
		if (player == null || !player.IsFriendlySide() || !player.HasTag(GAME_TAG.CURRENT_PLAYER))
		{
			return;
		}
		if (m_mousedOver)
		{
			bool inHand = m_entity.GetZone() == TAG_ZONE.HAND;
			bool isHeroPowerOrGameModeButton = m_entity.IsCardButton();
			if ((inHand || isHeroPowerOrGameModeButton) && GameState.Get().IsValidOption(m_entity, null) && (!m_entity.IsSpell() || !player.HasTag(GAME_TAG.SPELLS_COST_HEALTH)) && m_entity.GetTag<TAG_CARD_ALTERNATE_COST>(GAME_TAG.CARD_ALTERNATE_COST) == TAG_CARD_ALTERNATE_COST.MANA && (inHand || !m_entity.IsLocation()))
			{
				player.ProposeManaCrystalUsage(m_entity);
			}
		}
		else if (!m_entity.IsBattlegroundTrinket())
		{
			player.CancelAllProposedMana(m_entity);
		}
	}

	public void SetMagneticPlayData(MagneticPlayData data)
	{
		if (data != null)
		{
			if (m_magneticPlayData != null)
			{
				Log.Gameplay.PrintError("{0}.SetMagneticPlayData: m_magneticPlayData is already set! {1}", this, m_magneticPlayData);
			}
			m_magneticPlayData = data;
		}
	}

	public MagneticPlayData GetMagneticPlayData()
	{
		return m_magneticPlayData;
	}

	public void SetIsMagneticTarget(bool isTarget)
	{
		m_magneticTarget = isTarget;
	}

	public bool IsMagneticTarget()
	{
		return m_magneticTarget;
	}

	public void DetermineIfOverrideDrawTimeScale()
	{
		if (!m_drawTimeScale.HasValue)
		{
			if (GameState.Get().GetBooleanGameOption(GameEntityOption.ALWAYS_USE_FAST_CARD_DRAW_SCALE))
			{
				m_drawTimeScale = 0.556f;
			}
			else if (m_cardDrawTracker < 3)
			{
				m_drawTimeScale = 1f;
			}
			else if (m_cardDrawTracker <= 6)
			{
				float drawTimeScaleIncreament = -0.111f;
				m_drawTimeScale = 1f + drawTimeScaleIncreament * (float)(m_cardDrawTracker + 1 - 3);
			}
			else
			{
				m_drawTimeScale = 0.556f;
			}
		}
	}

	public void ResetCardDrawTimeScale()
	{
		m_drawTimeScale = null;
	}

	public bool CanPlayHealingDoesDamageHint()
	{
		if (!IsShown())
		{
			return false;
		}
		if (m_entity == null)
		{
			return false;
		}
		if (m_actor == null || !m_actor.IsShown())
		{
			return false;
		}
		if (m_entity.HasTag(GAME_TAG.AFFECTED_BY_HEALING_DOES_DAMAGE))
		{
			return true;
		}
		if (m_entity.HasTag(GAME_TAG.LIFESTEAL))
		{
			return true;
		}
		return m_entity.GetCardTextBuilder().ContainsBonusHealingToken(m_entity);
	}

	public bool CanPlayLifestealDoesDamageHint()
	{
		if (!IsShown())
		{
			return false;
		}
		if (m_entity == null)
		{
			return false;
		}
		if (m_actor == null || !m_actor.IsShown())
		{
			return false;
		}
		return m_entity.HasTag(GAME_TAG.LIFESTEAL);
	}

	public bool CanPlaySpellPowerHint(TAG_SPELL_SCHOOL spellSchool = TAG_SPELL_SCHOOL.NONE)
	{
		if (!IsShown())
		{
			return false;
		}
		if (m_actor == null || !m_actor.IsShown())
		{
			return false;
		}
		if (m_entity == null)
		{
			return false;
		}
		TAG_SPELL_SCHOOL entitySchool = m_entity.GetSpellSchool();
		Player controller = m_entity.GetController();
		if (controller.TotalSpellpower(m_entity, entitySchool) == 0)
		{
			return false;
		}
		if ((m_entity.HasTag(GAME_TAG.SECRET) || m_entity.HasTag(GAME_TAG.SIGIL)) && controller.IsSpellpowerTemporary(entitySchool))
		{
			return false;
		}
		if (spellSchool == TAG_SPELL_SCHOOL.NONE || spellSchool == entitySchool)
		{
			if (!m_entity.IsAffectedBySpellPower())
			{
				return m_entity.GetCardTextBuilder().ContainsBonusDamageToken(m_entity);
			}
			return true;
		}
		return false;
	}

	public DefLoader.DisposableCardDef ShareDisposableCardDef()
	{
		return m_cardDef?.Share();
	}

	public void SetCardDef(DefLoader.DisposableCardDef cardDef, bool updateActor)
	{
		if (!(m_cardDef?.CardDef == cardDef?.CardDef))
		{
			ReleaseCardDef();
			m_cardDef = cardDef.Share();
			InitCardDefAssets();
			if (m_actor != null && !updateActor)
			{
				m_actor.SetCardDef(m_cardDef);
				m_actor.UpdateAllComponents();
			}
		}
	}

	public void PurgeSpells()
	{
		foreach (CardEffect allEffect in m_allEffects)
		{
			allEffect.PurgeSpells();
		}
	}

	private bool ShouldPreloadCardAssets()
	{
		if (HearthstoneApplication.IsPublic())
		{
			return false;
		}
		return Options.Get().GetBool(Option.PRELOAD_CARD_ASSETS, defaultVal: false);
	}

	public void OverrideCustomSpawnSpell(Spell spell)
	{
		if (spell == null)
		{
			Debug.LogErrorFormat("Tried to set OverrideCustomSpawnSpell to null!");
		}
		else
		{
			m_customSpawnSpellOverride = SetupOverrideSpell(m_customSpawnSpellOverride, spell);
		}
	}

	public void OverrideCustomDeathSpell(Spell spell)
	{
		if (spell == null)
		{
			Debug.LogErrorFormat("Tried to set OverrideCustomDeathSpell to null!");
		}
		else
		{
			m_customDeathSpellOverride = SetupOverrideSpell(m_customDeathSpellOverride, spell);
		}
	}

	public void OverrideCustomDiscardSpell(Spell spell)
	{
		if (spell == null)
		{
			Debug.LogErrorFormat("Tried to set OverrideCustomDiscardSpell to null!");
		}
		else
		{
			m_customDiscardSpellOverride = SetupOverrideSpell(m_customDiscardSpellOverride, spell);
		}
	}

	public Texture GetPreferredActorPortraitTexture()
	{
		int preferredPortraitIndex = ((m_cardDef?.CardDef == null) ? 1 : m_cardDef.CardDef.m_PreferredActorPortraitIndex);
		Texture portraitTex = null;
		switch (preferredPortraitIndex)
		{
		case 0:
			portraitTex = GetPortraitTexture();
			break;
		case 1:
			portraitTex = GetGoldenMaterial().mainTexture;
			break;
		}
		return portraitTex;
	}

	public Texture GetPortraitTexture(TAG_PREMIUM premium = TAG_PREMIUM.NORMAL)
	{
		if (!(m_cardDef?.CardDef == null))
		{
			return m_cardDef.CardDef.GetPortraitTexture(premium);
		}
		return null;
	}

	public Material GetGoldenMaterial()
	{
		if (!(m_cardDef?.CardDef == null))
		{
			return m_cardDef.CardDef.GetPremiumPortraitMaterial();
		}
		return null;
	}

	public CardEffect GetPlayEffect(int index)
	{
		if (index > 0)
		{
			if (--index >= m_additionalPlayEffects.Count)
			{
				return null;
			}
			return m_additionalPlayEffects[index];
		}
		return m_playEffect;
	}

	public CardEffect GetOrCreateProxyEffect(Network.HistBlockStart blockStart, CardEffectDef proxyEffectDef)
	{
		if (m_proxyEffects == null)
		{
			m_proxyEffects = new Map<Network.HistBlockStart, CardEffect>();
		}
		if (m_proxyEffects.ContainsKey(blockStart))
		{
			return m_proxyEffects[blockStart];
		}
		CardEffect proxyEffect = new CardEffect(proxyEffectDef, this);
		InitEffect(proxyEffectDef, ref proxyEffect);
		m_proxyEffects.Add(blockStart, proxyEffect);
		return proxyEffect;
	}

	public void DeactivatePlaySpell()
	{
		Entity entity = GetEntity();
		Entity parentEntity = entity.GetParentEntity();
		Spell playSpell;
		if (parentEntity == null)
		{
			playSpell = GetPlaySpell(0, loadIfNeeded: false);
		}
		else
		{
			Card parentCard = parentEntity.GetCard();
			int subOption = parentEntity.GetSubCardIndex(entity);
			playSpell = parentCard.GetSubOptionSpell(subOption, 0, loadIfNeeded: false);
		}
		if (playSpell != null && playSpell.GetActiveState() != 0)
		{
			playSpell.SafeActivateState(SpellStateType.CANCEL);
		}
	}

	public Spell GetPlaySpell(int index, bool loadIfNeeded = true)
	{
		return GetPlayEffect(index)?.GetSpell(loadIfNeeded);
	}

	public List<CardSoundSpell> GetPlaySoundSpells(int index, bool loadIfNeeded = true)
	{
		return GetPlayEffect(index)?.GetSoundSpells(loadIfNeeded);
	}

	public Spell GetAttackSpell(bool loadIfNeeded = true)
	{
		if (m_attackEffect == null)
		{
			return null;
		}
		return m_attackEffect.GetSpell(loadIfNeeded);
	}

	public List<CardSoundSpell> GetAttackSoundSpells(bool loadIfNeeded = true)
	{
		if (m_attackEffect == null)
		{
			return null;
		}
		return m_attackEffect.GetSoundSpells(loadIfNeeded);
	}

	public List<CardSoundSpell> GetDeathSoundSpells(bool loadIfNeeded = true)
	{
		if (m_deathEffect == null)
		{
			return null;
		}
		return m_deathEffect.GetSoundSpells(loadIfNeeded);
	}

	public Spell GetLifetimeSpell(bool loadIfNeeded = true)
	{
		if (m_lifetimeEffect == null)
		{
			return null;
		}
		return m_lifetimeEffect.GetSpell(loadIfNeeded);
	}

	public List<CardSoundSpell> GetLifetimeSoundSpells(bool loadIfNeeded = true)
	{
		if (m_lifetimeEffect == null)
		{
			return null;
		}
		return m_lifetimeEffect.GetSoundSpells(loadIfNeeded);
	}

	public CardEffect GetSubOptionEffect(int suboption, int index)
	{
		if (suboption < 0)
		{
			return null;
		}
		if (index > 0)
		{
			if (m_additionalSubOptionEffects == null)
			{
				return null;
			}
			if (suboption >= m_additionalSubOptionEffects.Count)
			{
				return null;
			}
			List<CardEffect> effectList = m_additionalSubOptionEffects[suboption];
			if (effectList == null)
			{
				return null;
			}
			if (--index >= effectList.Count)
			{
				return null;
			}
			return effectList[index];
		}
		if (m_subOptionEffects == null)
		{
			return null;
		}
		if (suboption >= m_subOptionEffects.Count)
		{
			if (m_entity != null)
			{
				Entity subCardEntity = m_entity.GetSubCard(suboption);
				if (subCardEntity != null)
				{
					return subCardEntity.GetCard().m_playEffect;
				}
			}
			return null;
		}
		return m_subOptionEffects[suboption];
	}

	public Spell GetSubOptionSpell(int suboption, int index, bool loadIfNeeded = true)
	{
		return GetSubOptionEffect(suboption, index)?.GetSpell(loadIfNeeded);
	}

	public List<CardSoundSpell> GetSubOptionSoundSpells(int suboption, int index, bool loadIfNeeded = true)
	{
		return GetSubOptionEffect(suboption, index)?.GetSoundSpells(loadIfNeeded);
	}

	public CardEffect GetTriggerEffect(int index)
	{
		if (m_triggerEffects == null)
		{
			return null;
		}
		if (index < 0)
		{
			return null;
		}
		if (index >= m_triggerEffects.Count)
		{
			return null;
		}
		return m_triggerEffects[index];
	}

	public CardEffect GetResetGameEffect(int index)
	{
		if (m_resetGameEffects == null)
		{
			return null;
		}
		if (index < 0)
		{
			return null;
		}
		if (index >= m_resetGameEffects.Count)
		{
			return null;
		}
		return m_resetGameEffects[index];
	}

	public Spell GetTriggerSpell(int index, bool loadIfNeeded = true)
	{
		return GetTriggerEffect(index)?.GetSpell(loadIfNeeded);
	}

	public List<CardSoundSpell> GetTriggerSoundSpells(int index, bool loadIfNeeded = true)
	{
		return GetTriggerEffect(index)?.GetSoundSpells(loadIfNeeded);
	}

	public ISpell GetCustomKeywordSpell()
	{
		if (m_customKeywordEffect == null)
		{
			return null;
		}
		return m_customKeywordEffect.GetSpell();
	}

	public ISpell GetCustomSummonSpell()
	{
		return m_customSummonSpell;
	}

	public ISpell GetCustomSpawnSpell()
	{
		return m_customSpawnSpell;
	}

	public ISpell GetCustomSpawnSpellOverride()
	{
		return m_customSpawnSpellOverride;
	}

	public ISpell GetCustomDeathSpell()
	{
		return m_customDeathSpell;
	}

	public ISpell GetCustomDeathSpellOverride()
	{
		return m_customDeathSpellOverride;
	}

	public ISpell GetCustomChoiceRevealSpell()
	{
		if (m_customChoiceRevealEffect == null)
		{
			return null;
		}
		Spell spell = m_customChoiceRevealEffect.GetSpell();
		if (spell != null && spell.IsActive())
		{
			spell.AddFinishedCallback(ReleaseCustomChoiceActiveSpell);
			return m_customChoiceRevealEffect.LoadSpell();
		}
		return spell;
	}

	private void ReleaseCustomChoiceActiveSpell(Spell spell, object data)
	{
		SpellManager spellManager = SpellManager.Get();
		if (!(spell == null))
		{
			spellManager?.ReleaseSpell(spell);
		}
	}

	public ISpell GetCustomChoiceConcealSpell()
	{
		if (m_customChoiceConcealEffect == null)
		{
			return null;
		}
		return m_customChoiceConcealEffect.GetSpell();
	}

	public ISpell GetSpellTableOverride(SpellType spellType)
	{
		CardEffect cachedOverrideEffect = null;
		if (m_spellTableOverrideEffects.TryGetValue(spellType, out cachedOverrideEffect))
		{
			return cachedOverrideEffect.GetSpell();
		}
		foreach (SpellTableOverride spellTableOverride in m_cardDef.CardDef.m_SpellTableOverrides)
		{
			if (spellTableOverride.m_Type == spellType)
			{
				if (string.IsNullOrEmpty(spellTableOverride.m_SpellPrefabName))
				{
					break;
				}
				CardEffect overrideEffect = null;
				InitEffect(spellTableOverride.m_SpellPrefabName, ref overrideEffect);
				if (overrideEffect != null)
				{
					m_spellTableOverrideEffects[spellType] = overrideEffect;
					return overrideEffect.GetSpell();
				}
			}
		}
		return null;
	}

	public AudioSource GetAnnouncerLine(AnnouncerLineType type)
	{
		CardSound sound = m_announcerLine[(int)type];
		if (sound == null || sound.GetSound() == null)
		{
			if (m_announcerLine[0] == null)
			{
				string errorMsg = $"Card.GetAnnouncerLine(AnnouncerLineType type) - Failed to load announcer audio source.";
				if (HearthstoneApplication.UseDevWorkarounds())
				{
					Debug.LogError(errorMsg);
				}
				return SoundManager.Get().GetPlaceholderSource();
			}
			sound = m_announcerLine[0];
		}
		return sound.GetSound();
	}

	public EmoteEntry GetEmoteEntry(EmoteType emoteType)
	{
		if (m_emotes == null)
		{
			return null;
		}
		SpecialEventDbfRecord currentSpecialEvent = SpecialEventManager.Get().GetCurrentSpecialEvent();
		EmoteEntry overrideEmote = null;
		if (currentSpecialEvent != null)
		{
			SpecialEvent.SpecialEventType eventType = currentSpecialEvent.SpecialEventType;
			switch (emoteType)
			{
			case EmoteType.GREETINGS:
			case EmoteType.MIRROR_GREETINGS:
				switch (eventType)
				{
				case SpecialEvent.SpecialEventType.LUNAR_NEW_YEAR:
					overrideEmote = GetEmoteEntryExact(EmoteType.HAPPY_NEW_YEAR_LUNAR);
					break;
				case SpecialEvent.SpecialEventType.NOBLEGARDEN:
					overrideEmote = GetEmoteEntryExact(EmoteType.HAPPY_NOBLEGARDEN);
					break;
				case SpecialEvent.SpecialEventType.FIRE_FESTIVAL:
					overrideEmote = GetEmoteEntryExact(EmoteType.FIRE_FESTIVAL);
					break;
				case SpecialEvent.SpecialEventType.WINTER_VEIL:
					overrideEmote = GetEmoteEntryExact(EmoteType.HAPPY_HOLIDAYS);
					break;
				case SpecialEvent.SpecialEventType.FROST_FESTIVAL:
					overrideEmote = GetEmoteEntryExact(EmoteType.FROST_FESTIVAL_FIREWORKS_RANK_THREE);
					break;
				case SpecialEvent.SpecialEventType.NEW_YEAR:
					overrideEmote = GetEmoteEntryExact(EmoteType.HAPPY_NEW_YEAR);
					break;
				case SpecialEvent.SpecialEventType.PIRATE_DAY:
					overrideEmote = GetEmoteEntryExact(EmoteType.PIRATE_DAY);
					break;
				case SpecialEvent.SpecialEventType.HALLOWS_END:
					overrideEmote = GetEmoteEntryExact(EmoteType.HAPPY_HALLOWEEN);
					break;
				}
				break;
			case EmoteType.WOW:
				if (eventType == SpecialEvent.SpecialEventType.FIRE_FESTIVAL)
				{
					overrideEmote = GetEmoteEntryExact(EmoteType.FIRE_FESTIVAL_FIREWORKS_RANK_THREE);
				}
				break;
			}
		}
		return overrideEmote ?? GetEmoteEntryExact(emoteType);
	}

	private EmoteEntry GetEmoteEntryExact(EmoteType emoteType)
	{
		foreach (EmoteEntry emote in m_emotes)
		{
			if (emote.GetEmoteType() == emoteType)
			{
				return emote;
			}
		}
		return null;
	}

	public ISpell GetBestSummonSpell()
	{
		bool standard;
		return GetBestSummonSpell(out standard);
	}

	public ISpell GetBestSummonSpell(out bool standard)
	{
		if (m_customSummonSpell != null && GetMagneticPlayData() == null && GetEntity() != null && !GetEntity().HasTag(GAME_TAG.CARD_DOES_NOTHING) && !WillSuppressCustomSpells() && !WillSuppressCustomSummonSpells())
		{
			standard = false;
			return m_customSummonSpell;
		}
		standard = true;
		if (m_cardDef?.CardDef == null)
		{
			Log.Gameplay.PrintError("Cannot determine best summon spell. Missing CardDef");
			return null;
		}
		bool useFastAnimations = GameState.Get()?.GetGameEntity().HasTag(GAME_TAG.USE_FAST_ACTOR_TRANSITION_ANIMATIONS) ?? false;
		SpellType spellType = m_cardDef.CardDef.DetermineSummonInSpell_HandToPlay(this, useFastAnimations);
		return GetActorSpell(spellType);
	}

	public ISpell GetBestSpawnSpell()
	{
		bool standard;
		return GetBestSpawnSpell(out standard);
	}

	public ISpell GetBestSpawnSpell(out bool standard)
	{
		standard = false;
		if (m_entity.HasTag(GAME_TAG.HAS_BEEN_REBORN))
		{
			Spell spell = GetActorSpell(SpellType.REBORN_SPAWN);
			if (spell != null)
			{
				return spell;
			}
		}
		if (m_customSpawnSpellOverride != null)
		{
			return m_customSpawnSpellOverride;
		}
		if (m_customSpawnSpell != null)
		{
			return m_customSpawnSpell;
		}
		switch (m_entity.GetTag<TAG_ROLE>(GAME_TAG.LETTUCE_ROLE))
		{
		case TAG_ROLE.CASTER:
			return GetActorSpell(SpellType.LETTUCE_COME_IN_PLAY_CASTER);
		case TAG_ROLE.FIGHTER:
			return GetActorSpell(SpellType.LETTUCE_COME_IN_PLAY_FIGHTER);
		case TAG_ROLE.TANK:
			return GetActorSpell(SpellType.LETTUCE_COME_IN_PLAY_PROTECTOR);
		default:
			standard = true;
			if (m_entity.IsControlledByFriendlySidePlayer())
			{
				return GetActorSpell(SpellType.FRIENDLY_SPAWN_MINION_OR_LOCATION);
			}
			return GetActorSpell(SpellType.OPPONENT_SPAWN_MINION_OR_LOCATION);
		}
	}

	public ISpell GetBestDeathSpell()
	{
		bool standard;
		return GetBestDeathSpell(out standard);
	}

	public ISpell GetBestDeathSpell(out bool standard)
	{
		return GetBestDeathSpell(m_actor, out standard);
	}

	private ISpell GetBestDeathSpell(Actor actor)
	{
		bool standard;
		return GetBestDeathSpell(actor, out standard);
	}

	private ISpell GetBestDeathSpell(Actor actor, out bool standard)
	{
		standard = false;
		if (m_prevZone is ZoneHand && m_zone is ZoneGraveyard)
		{
			if (m_customDiscardSpellOverride != null)
			{
				return m_customDiscardSpellOverride;
			}
			if (m_customDiscardSpell != null && !m_entity.IsSilenced())
			{
				return m_customDiscardSpell;
			}
		}
		else
		{
			if (m_customDeathSpellOverride != null)
			{
				return m_customDeathSpellOverride;
			}
			if (m_customDeathSpell != null && !m_entity.IsSilenced())
			{
				return m_customDeathSpell;
			}
		}
		standard = true;
		return actor.GetSpell(SpellType.DEATH);
	}

	public void ActivateCharacterPlayEffects()
	{
		if (!WillSuppressPlaySounds())
		{
			ActivateSoundSpellList(m_playEffect.GetSoundSpells());
		}
		SuppressPlaySounds(suppress: false);
		ActivateLifetimeEffects();
	}

	public void ActivateCharacterTradeEffects()
	{
		if (m_additionalPlayEffects.Count > 0)
		{
			ActivateSoundSpellList(m_additionalPlayEffects[0].GetSoundSpells());
		}
	}

	public void ActivateChooseOneEffects()
	{
		if (!WillSuppressPlaySounds())
		{
			ActivateSoundSpellList(m_playEffect.GetSoundSpells());
		}
		SuppressPlaySounds(suppress: false);
	}

	public void ActivateCharacterAttackEffects()
	{
		ActivateSoundSpellList(m_attackEffect.GetSoundSpells());
	}

	public void ActivateCharacterDeathEffects()
	{
		if (!m_suppressDeathEffects && !IsHeroUsingDeathSpellOverride())
		{
			if (!m_suppressDeathSounds)
			{
				ForceActivateDeathSoundSpells();
			}
			m_suppressDeathSounds = false;
			DeactivateLifetimeEffects();
		}
	}

	public void ForceActivateDeathSoundSpells()
	{
		if (((m_emotes == null) ? (-1) : m_emotes.FindIndex((EmoteEntry e) => e != null && e.GetEmoteType() == EmoteType.DEATH_LINE)) >= 0)
		{
			PlayEmote(EmoteType.DEATH_LINE);
		}
		else
		{
			ActivateSoundSpellList(m_deathEffect.GetSoundSpells());
		}
	}

	private bool IsHeroUsingDeathSpellOverride()
	{
		Entity ent = GetEntity();
		if (ent == null)
		{
			return false;
		}
		if (ent.GetCardType() != TAG_CARDTYPE.HERO)
		{
			return false;
		}
		return ent.GetController().GetTag(GAME_TAG.DEATH_SPELL_OVERRIDE) != 0;
	}

	public void ActivateLifetimeEffects()
	{
		if (m_lifetimeEffect == null || m_entity.IsSilenced() || m_entity.HasTag(GAME_TAG.CARD_DOES_NOTHING) || WillSuppressCustomSpells() || WillSuppressCustomLifetimeSpells())
		{
			return;
		}
		GameEntity gameEntity = GameState.Get()?.GetGameEntity();
		if (gameEntity == null || !gameEntity.HasTag(GAME_TAG.SQUELCH_LIFETIME_EFFECTS))
		{
			Spell lifetimeSpell = m_lifetimeEffect.GetSpell();
			if (lifetimeSpell != null)
			{
				lifetimeSpell.Deactivate();
				lifetimeSpell.ActivateState(SpellStateType.BIRTH);
			}
			if (m_lifetimeEffect.GetSoundSpells() != null)
			{
				ActivateSoundSpellList(m_lifetimeEffect.GetSoundSpells());
			}
		}
	}

	public void DeactivateLifetimeEffects()
	{
		if (m_lifetimeEffect == null)
		{
			return;
		}
		Spell lifetimeSpell = m_lifetimeEffect.GetSpell();
		if (lifetimeSpell != null)
		{
			SpellStateType lifetimeSpellState = lifetimeSpell.GetActiveState();
			if (lifetimeSpellState != 0 && lifetimeSpellState != SpellStateType.DEATH)
			{
				lifetimeSpell.ActivateState(SpellStateType.DEATH);
			}
		}
	}

	public void ActivateCustomKeywordEffect()
	{
		if (m_customKeywordEffect == null || (GetEntity() != null && (GetEntity().HasTag(GAME_TAG.CARD_DOES_NOTHING) || WillSuppressCustomSpells() || WillSuppressCustomKeywordSpells())))
		{
			return;
		}
		Spell keyWordSpell = m_customKeywordEffect.GetSpell();
		if (keyWordSpell == null)
		{
			Debug.LogWarning($"Card.ActivateCustomKeywordEffect() -- failed to load custom keyword spell for card {ToString()}");
			return;
		}
		if (keyWordSpell.DoesBlockServerEvents())
		{
			GameState.Get().AddServerBlockingSpell(keyWordSpell);
		}
		TransformUtil.AttachAndPreserveLocalTransform(keyWordSpell.transform, m_actor.transform);
		keyWordSpell.ActivateState(SpellStateType.BIRTH);
	}

	public void DeactivateCustomKeywordEffect()
	{
		if (m_customKeywordEffect != null)
		{
			Spell keyWordSpell = m_customKeywordEffect.GetSpell(loadIfNeeded: false);
			if (!(keyWordSpell == null) && keyWordSpell.IsActive())
			{
				keyWordSpell.ActivateState(SpellStateType.DEATH);
			}
		}
	}

	public bool ActivateSoundSpellList(List<CardSoundSpell> soundSpells)
	{
		if (soundSpells == null)
		{
			return false;
		}
		if (soundSpells.Count == 0)
		{
			return false;
		}
		bool success = false;
		for (int i = 0; i < soundSpells.Count; i++)
		{
			CardSoundSpell soundSpell = soundSpells[i];
			ActivateSoundSpell(soundSpell);
			success = true;
		}
		return success;
	}

	public bool ActivateSoundSpell(CardSoundSpell soundSpell)
	{
		if (soundSpell == null || GetEntity().HasTag(GAME_TAG.CARD_DOES_NOTHING))
		{
			return false;
		}
		GameState gameState = GameState.Get();
		GameEntity gameEntity = null;
		if (gameState != null)
		{
			gameEntity = gameState.GetGameEntity();
			if (gameEntity == null)
			{
				return false;
			}
		}
		if (gameEntity != null && gameEntity.GetGameOptions().GetBooleanOption(GameEntityOption.DELAY_CARD_SOUND_SPELLS))
		{
			StartCoroutine(WaitThenActivateSoundSpell(soundSpell));
		}
		else
		{
			soundSpell.Reactivate();
		}
		return true;
	}

	public bool HasActiveEmoteSound()
	{
		if (m_emotes == null)
		{
			return false;
		}
		foreach (EmoteEntry emote in m_emotes)
		{
			CardSoundSpell spell = emote.GetSoundSpell(loadIfNeeded: false);
			if (spell != null && spell.IsActive())
			{
				return true;
			}
		}
		return false;
	}

	public EmoteEntry GetActiveEmoteSound()
	{
		if (m_emotes == null)
		{
			return null;
		}
		foreach (EmoteEntry emote in m_emotes)
		{
			CardSoundSpell spell = emote.GetSoundSpell(loadIfNeeded: false);
			if (spell != null && spell.IsActive())
			{
				return emote;
			}
		}
		return null;
	}

	public bool HasUnfinishedEmoteSpell()
	{
		if (m_emotes == null)
		{
			return false;
		}
		foreach (EmoteEntry emote in m_emotes)
		{
			Spell spell = emote.GetSpell(loadIfNeeded: false);
			if (spell != null && !spell.IsFinished())
			{
				return true;
			}
		}
		return false;
	}

	public CardSoundSpell PlayEmote(EmoteType emoteType)
	{
		return PlayEmote(emoteType, Notification.SpeechBubbleDirection.None);
	}

	public CardSoundSpell PlayEmote(EmoteType emoteType, Notification.SpeechBubbleDirection overrideDirection)
	{
		EmoteEntry emoteEntry = GetEmoteEntry(emoteType);
		CardSoundSpell emoteSoundSpell = emoteEntry?.GetSoundSpell();
		Spell emoteSpell = emoteEntry?.GetSpell();
		if (m_actor == null)
		{
			return null;
		}
		if (emoteSoundSpell != null)
		{
			emoteSoundSpell.Reactivate();
			if (emoteSoundSpell.IsActive())
			{
				for (int i = 0; i < m_emotes.Count; i++)
				{
					EmoteEntry currEntry = m_emotes[i];
					if (currEntry != emoteEntry)
					{
						Spell currSpell = currEntry.GetSoundSpell(loadIfNeeded: false);
						if ((bool)currSpell)
						{
							currSpell.Deactivate();
						}
					}
				}
			}
			if (m_entity.IsHero() && !m_entity.IsCutsceneEntity())
			{
				GameState.Get().GetGameEntity().OnEmotePlayed(this, emoteType, emoteSoundSpell);
			}
		}
		Notification.SpeechBubbleDirection direction = Notification.SpeechBubbleDirection.BottomLeft;
		if (GetEntity().IsControlledByOpposingSidePlayer())
		{
			direction = Notification.SpeechBubbleDirection.TopRight;
		}
		if (overrideDirection != 0)
		{
			direction = overrideDirection;
		}
		string localizedString = null;
		if (emoteSoundSpell != null)
		{
			localizedString = string.Empty;
			if (emoteSoundSpell is CardSpecificVoSpell)
			{
				CardSpecificVoData voData = ((CardSpecificVoSpell)emoteSoundSpell).GetBestVoiceData();
				if (voData != null && !string.IsNullOrEmpty(voData.m_GameStringKey))
				{
					localizedString = GameStrings.Get(voData.m_GameStringKey);
				}
			}
		}
		if (string.IsNullOrEmpty(localizedString) && emoteEntry != null && !string.IsNullOrEmpty(emoteEntry.GetGameStringKey()))
		{
			localizedString = GameStrings.Get(emoteEntry.GetGameStringKey());
		}
		StartCoroutine(WaitAndDisplayEmoteSpeechBubble(localizedString, direction, emoteSoundSpell, emoteSpell));
		this.OnEmotePlayCallback?.Invoke(emoteType);
		return emoteSoundSpell;
	}

	protected IEnumerator WaitAndDisplayEmoteSpeechBubble(string speechText, Notification.SpeechBubbleDirection direction, CardSoundSpell emoteSoundSpell, Spell emoteSpell)
	{
		Notification notification = null;
		if (!string.IsNullOrEmpty(speechText))
		{
			notification = NotificationManager.Get().CreateSpeechBubble(speechText, direction, m_actor, bDestroyWhenNewCreated: true);
			float waitTime = 1.5f;
			if ((bool)emoteSoundSpell)
			{
				if (emoteSoundSpell.m_CardSoundData != null)
				{
					float soundDelay = emoteSoundSpell.m_CardSoundData.m_DelaySec;
					if (soundDelay > 0f)
					{
						yield return new WaitForSeconds(soundDelay);
					}
				}
				if ((bool)emoteSoundSpell)
				{
					AudioSource source = emoteSoundSpell.GetActiveAudioSource();
					if ((bool)source && (bool)source.clip && waitTime < source.clip.length)
					{
						waitTime = source.clip.length;
					}
				}
			}
			NotificationManager.Get().DestroyNotification(notification, waitTime);
		}
		if (emoteSpell != null)
		{
			VisualEmoteSpell visualEmoteSpell = emoteSpell as VisualEmoteSpell;
			if (visualEmoteSpell != null && visualEmoteSpell.m_PositionOnSpeechBubble && notification != null)
			{
				visualEmoteSpell.SetSource(notification.gameObject);
				visualEmoteSpell.Reactivate();
			}
			else
			{
				emoteSpell.Reactivate();
			}
		}
	}

	private void InitCardDefAssets()
	{
		InitEffect(m_cardDef.CardDef.m_PlayEffectDef, ref m_playEffect);
		InitEffectList(m_cardDef.CardDef.m_AdditionalPlayEffectDefs, ref m_additionalPlayEffects);
		InitEffect(m_cardDef.CardDef.m_AttackEffectDef, ref m_attackEffect);
		InitEffect(m_cardDef.CardDef.m_DeathEffectDef, ref m_deathEffect);
		InitEffect(m_cardDef.CardDef.m_LifetimeEffectDef, ref m_lifetimeEffect);
		InitEffect(m_cardDef.CardDef.m_CustomKeywordSpellPath, ref m_customKeywordEffect);
		InitEffect(m_cardDef.CardDef.m_CustomChoiceRevealSpellPath, ref m_customChoiceRevealEffect);
		InitEffect(m_cardDef.CardDef.m_CustomChoiceConcealSpellPath, ref m_customChoiceConcealEffect);
		InitEffectList(m_cardDef.CardDef.m_SubOptionEffectDefs, ref m_subOptionEffects);
		InitEffectListList(m_cardDef.CardDef.m_AdditionalSubOptionEffectDefs, ref m_additionalSubOptionEffects);
		InitEffectList(m_cardDef.CardDef.m_TriggerEffectDefs, ref m_triggerEffects);
		InitEffectList(m_cardDef.CardDef.m_ResetGameEffectDefs, ref m_resetGameEffects);
		InitSound(m_cardDef.CardDef.m_AnnouncerLinePath, ref m_announcerLine[0], alwaysValid: true);
		InitSound(m_cardDef.CardDef.m_AnnouncerLineBeforeVersusPath, ref m_announcerLine[1], alwaysValid: false);
		InitSound(m_cardDef.CardDef.m_AnnouncerLineAfterVersusPath, ref m_announcerLine[2], alwaysValid: false);
		InitEmoteList();
		if (m_cardDef.CardDef.m_LegendaryHeroSkinConfig != null)
		{
			if (m_entity.GetController() != null && m_entity.GetController().IsOpposingSide())
			{
				m_cardDef.CardDef.m_LegendaryHeroSkinConfig.InitCombatAssets(this);
			}
			else
			{
				m_cardDef.CardDef.m_LegendaryHeroSkinConfig.InitAllAssets(this);
			}
		}
	}

	private void InitEffect(CardEffectDef effectDef, ref CardEffect effect)
	{
		DestroyCardEffect(ref effect);
		if (effectDef != null)
		{
			effect = new CardEffect(effectDef, this);
			if (m_allEffects == null)
			{
				m_allEffects = new List<CardEffect>();
			}
			m_allEffects.Add(effect);
			if (ShouldPreloadCardAssets())
			{
				effect.LoadAll();
			}
		}
	}

	private void InitEffect(string spellPath, ref CardEffect effect)
	{
		DestroyCardEffect(ref effect);
		if (!string.IsNullOrEmpty(spellPath))
		{
			effect = new CardEffect(spellPath, this);
			if (m_allEffects == null)
			{
				m_allEffects = new List<CardEffect>();
			}
			m_allEffects.Add(effect);
			if (ShouldPreloadCardAssets())
			{
				effect.LoadAll();
			}
		}
	}

	private void InitEffectList(List<CardEffectDef> effectDefs, ref List<CardEffect> effects)
	{
		DestroyCardEffectList(ref effects);
		if (effectDefs == null)
		{
			return;
		}
		effects = new List<CardEffect>();
		for (int i = 0; i < effectDefs.Count; i++)
		{
			CardEffectDef effectDef = effectDefs[i];
			CardEffect effect = null;
			if (effectDef != null)
			{
				effect = new CardEffect(effectDef, this);
				if (m_allEffects == null)
				{
					m_allEffects = new List<CardEffect>();
				}
				m_allEffects.Add(effect);
				if (ShouldPreloadCardAssets())
				{
					effect.LoadAll();
				}
			}
			effects.Add(effect);
		}
	}

	private void InitEffectListList(List<List<CardEffectDef>> effectDefs, ref List<List<CardEffect>> effects)
	{
		if (effects != null)
		{
			for (int i = 0; i < effects.Count; i++)
			{
				List<CardEffect> effectList = effects[i];
				DestroyCardEffectList(ref effectList);
			}
			effects = null;
		}
		if (effectDefs != null)
		{
			effects = new List<List<CardEffect>>();
			for (int j = 0; j < effectDefs.Count; j++)
			{
				List<CardEffect> effectList2 = effects[j];
				InitEffectList(effectDefs[j], ref effectList2);
			}
		}
	}

	private void InitSound(string path, ref CardSound cardSound, bool alwaysValid)
	{
		DestroyCardSound(ref cardSound);
		if (!string.IsNullOrEmpty(path))
		{
			cardSound = new CardSound(path, this, alwaysValid);
			if (ShouldPreloadCardAssets())
			{
				cardSound.GetSound();
			}
		}
	}

	private void InitEmoteList()
	{
		DestroyEmoteList();
		if (m_cardDef.CardDef.m_EmoteDefs == null)
		{
			return;
		}
		m_emotes = new List<EmoteEntry>();
		for (int i = 0; i < m_cardDef.CardDef.m_EmoteDefs.Count; i++)
		{
			EmoteEntryDef emoteDef = m_cardDef.CardDef.m_EmoteDefs[i];
			EmoteEntry emote = new EmoteEntry(emoteDef.m_emoteType, emoteDef.m_emoteSpellPath, emoteDef.m_emoteSoundSpellPath, emoteDef.m_emoteGameStringKey, this);
			if (ShouldPreloadCardAssets())
			{
				emote.GetSoundSpell();
				emote.GetSpell();
			}
			m_emotes.Add(emote);
		}
	}

	private ISpell SetupOverrideSpell(ISpell existingSpell, ISpell spell)
	{
		if (existingSpell != null)
		{
			if (existingSpell.IsActive())
			{
				Log.Gameplay.PrintError("destroying active spell {0} currently in state {1} with source {2}.", existingSpell, existingSpell.GetActiveState(), existingSpell.Source);
			}
			UnityEngine.Object.Destroy(existingSpell.GameObject);
		}
		SpellUtils.SetupSpell(spell, this);
		return spell;
	}

	private void ReleaseAssets()
	{
		ReleaseCardDef();
		DestroyCardDefAssets();
	}

	private void ReleaseCardDef()
	{
		m_cardDef?.Dispose();
		m_cardDef = null;
	}

	private void DestroyCardDefAssets()
	{
		DestroyCardEffect(ref m_playEffect);
		DestroyCardEffect(ref m_attackEffect);
		DestroyCardEffect(ref m_deathEffect);
		DestroyCardEffect(ref m_lifetimeEffect);
		DestroyCardEffectList(ref m_subOptionEffects);
		DestroyCardEffectList(ref m_triggerEffects);
		DestroyCardEffectList(ref m_resetGameEffects);
		foreach (CardEffect value in m_spellTableOverrideEffects.Values)
		{
			value.Clear();
		}
		m_spellTableOverrideEffects.Clear();
		if (m_proxyEffects != null)
		{
			List<CardEffect> proxyEffects = new List<CardEffect>(m_proxyEffects.Values);
			DestroyCardEffectList(ref proxyEffects);
			m_proxyEffects.Clear();
		}
		DestroyCardEffect(ref m_customKeywordEffect);
		DestroyCardEffect(ref m_customChoiceRevealEffect);
		DestroyCardEffect(ref m_customChoiceConcealEffect);
		for (int i = 0; i < m_announcerLine.Count(); i++)
		{
			DestroyCardSound(ref m_announcerLine[i]);
		}
		DestroyEmoteList();
		ReleaseCardSpell(ref m_customSummonSpell);
		ReleaseCardSpell(ref m_customSpawnSpell);
		ReleaseCardSpell(ref m_customSpawnSpellOverride);
		ReleaseCardSpell(ref m_customDeathSpell);
		ReleaseCardSpell(ref m_customDeathSpellOverride);
		ReleaseCardSpell(ref m_customDiscardSpell);
		ReleaseCardSpell(ref m_customDiscardSpellOverride);
	}

	public void DestroyCardDefAssetsOnEntityChanged()
	{
		DeactivateLifetimeEffects();
		ReleaseCardSpell(ref m_customDeathSpell);
		DestroyCardEffect(ref m_lifetimeEffect);
	}

	private void DestroyCardEffect(ref CardEffect effect)
	{
		if (effect != null)
		{
			effect.PurgeSpells();
			effect = null;
		}
	}

	private void DestroyCardSound(ref CardSound cardSound)
	{
		if (cardSound != null)
		{
			cardSound.Clear();
			cardSound = null;
		}
	}

	private void DestroyCardEffectList(ref List<CardEffect> effects)
	{
		if (effects == null)
		{
			return;
		}
		foreach (CardEffect effect in effects)
		{
			effect.PurgeSpells();
		}
		effects = null;
	}

	private void ReleaseCardSpell(ref ISpell asset)
	{
		SpellManager spellManager = SpellManager.Get();
		if (asset != null && spellManager != null && asset is Spell spell)
		{
			spellManager.ReleaseSpell(spell);
			asset = null;
		}
	}

	private void DestroyEmoteList()
	{
		if (m_emotes != null)
		{
			for (int i = 0; i < m_emotes.Count; i++)
			{
				m_emotes[i].Clear();
			}
			m_emotes = null;
		}
	}

	public void CancelActiveSpells()
	{
		SpellUtils.ActivateCancelIfNecessary(GetPlaySpell(0, loadIfNeeded: false));
		if (m_subOptionEffects != null)
		{
			foreach (CardEffect subOptionEffect in m_subOptionEffects)
			{
				SpellUtils.ActivateCancelIfNecessary(subOptionEffect.GetSpell(loadIfNeeded: false));
			}
		}
		if (m_triggerEffects == null)
		{
			return;
		}
		foreach (CardEffect triggerEffect in m_triggerEffects)
		{
			SpellUtils.ActivateCancelIfNecessary(triggerEffect.GetSpell(loadIfNeeded: false));
		}
	}

	public void CancelCustomSpells()
	{
		SpellUtils.ActivateCancelIfNecessary(m_customSummonSpell);
		SpellUtils.ActivateCancelIfNecessary(m_customSpawnSpell);
		SpellUtils.ActivateCancelIfNecessary(m_customSpawnSpellOverride);
		SpellUtils.ActivateCancelIfNecessary(m_customDeathSpell);
		SpellUtils.ActivateCancelIfNecessary(m_customDeathSpellOverride);
		SpellUtils.ActivateCancelIfNecessary(m_customDiscardSpell);
		SpellUtils.ActivateCancelIfNecessary(m_customDiscardSpellOverride);
	}

	private IEnumerator WaitThenActivateSoundSpell(CardSoundSpell soundSpell)
	{
		GameEntity gameEntity = GameState.Get().GetGameEntity();
		while (gameEntity.GetGameOptions().GetBooleanOption(GameEntityOption.DELAY_CARD_SOUND_SPELLS))
		{
			yield return null;
		}
		soundSpell.Reactivate();
	}

	public void OnTagsChanged(TagDeltaList changeList, bool fromShowEntity)
	{
		bool actorDirty = false;
		for (int i = 0; i < changeList.Count; i++)
		{
			TagDelta change = changeList[i];
			switch ((GAME_TAG)change.tag)
			{
			case GAME_TAG.HEALTH:
			case GAME_TAG.ATK:
			case GAME_TAG.COST:
			case GAME_TAG.DURABILITY:
			case GAME_TAG.ARMOR:
			case GAME_TAG.HEALTH_DISPLAY:
			case GAME_TAG.ENABLE_HEALTH_DISPLAY:
			case GAME_TAG.HEALTH_DISPLAY_COLOR:
			case GAME_TAG.LETTUCE_ROLE:
			case GAME_TAG.LETTUCE_COOLDOWN_CONFIG:
			case GAME_TAG.LETTUCE_CURRENT_COOLDOWN:
				actorDirty = true;
				break;
			default:
				OnTagChanged(change, fromShowEntity);
				break;
			}
		}
		if (actorDirty && !m_entity.IsLoadingAssets() && IsActorReady())
		{
			UpdateActorComponents();
		}
	}

	public void OnMetaData(Network.HistMetaData metaData)
	{
		if ((metaData.MetaType != HistoryMeta.Type.DAMAGE && metaData.MetaType != HistoryMeta.Type.HEALING && metaData.MetaType != HistoryMeta.Type.POISONOUS && metaData.MetaType != HistoryMeta.Type.CRITICAL_HIT && metaData.MetaType != HistoryMeta.Type.SPEND_HEALTH && metaData.MetaType != HistoryMeta.Type.SPEND_ARMOR) || !CanShowActorVisuals() || (m_entity.GetZone() != TAG_ZONE.PLAY && !m_entity.HasTag(GAME_TAG.CUTSCENE_CARD_TYPE)))
		{
			return;
		}
		Spell spell = GetActorSpell(SpellType.DAMAGE);
		if (spell == null)
		{
			UpdateActorComponents();
			return;
		}
		spell.AddFinishedCallback(OnSpellFinished_UpdateActorComponents);
		if (m_entity.IsCharacter())
		{
			int damage = ((metaData.MetaType == HistoryMeta.Type.HEALING) ? (-metaData.Data) : metaData.Data);
			DamageSplatSpell damageSpell = (DamageSplatSpell)spell;
			damageSpell.SetDamage(damage);
			if (metaData.MetaType == HistoryMeta.Type.POISONOUS)
			{
				if (damageSpell.IsPoisonous())
				{
					return;
				}
				damageSpell.SetPoisonous(isPoisonous: true);
				damageSpell.SetDamageIsCrit(isCrit: false);
			}
			else if (metaData.MetaType == HistoryMeta.Type.CRITICAL_HIT)
			{
				damageSpell.SetPoisonous(isPoisonous: false);
				damageSpell.SetDamageIsCrit(isCrit: true);
			}
			else if (metaData.MetaType == HistoryMeta.Type.HEALING)
			{
				damageSpell.SetPoisonous(isPoisonous: false);
				damageSpell.SetDamageIsCrit(isCrit: false);
			}
			else
			{
				damageSpell.SetPoisonous(isPoisonous: false);
			}
			if (metaData.MetaType == HistoryMeta.Type.SPEND_HEALTH)
			{
				damageSpell.SetAlternateCostSplat(TAG_CARD_ALTERNATE_COST.HEALTH);
			}
			else if (metaData.MetaType == HistoryMeta.Type.SPEND_ARMOR)
			{
				damageSpell.SetAlternateCostSplat(TAG_CARD_ALTERNATE_COST.ARMOR);
			}
			else
			{
				damageSpell.SetAlternateCostSplat(TAG_CARD_ALTERNATE_COST.MANA);
			}
			spell.ActivateState(SpellStateType.ACTION);
			BoardEvents boardEvents = BoardEvents.Get();
			if (boardEvents != null)
			{
				if (metaData.MetaType == HistoryMeta.Type.HEALING)
				{
					boardEvents.HealEvent(this, -metaData.Data);
				}
				else
				{
					boardEvents.DamageEvent(this, metaData.Data);
				}
			}
		}
		else
		{
			spell.Activate();
		}
	}

	public void HandleCardExhaustedTagChanged(TagDelta change)
	{
		if (m_entity.IsSecret())
		{
			if (!CanShowSecretActorVisuals())
			{
				return;
			}
		}
		else if (!CanShowActorVisuals())
		{
			return;
		}
		if (change.tag != 43)
		{
			return;
		}
		if (m_entity.IsDisabledHeroPower())
		{
			change.newValue = 1;
		}
		if (change.newValue != change.oldValue)
		{
			if (GameState.Get().IsTurnStartManagerActive() && m_entity.IsControlledByFriendlySidePlayer())
			{
				TurnStartManager.Get().NotifyOfExhaustedChange(this, change);
			}
			else
			{
				ShowExhaustedChange(change.newValue);
			}
		}
	}

	public void HandleHeroPowerDisabledTagChanged(TagDelta change)
	{
		if (!m_entity.IsHeroPower())
		{
			return;
		}
		if (m_entity.HasTag(GAME_TAG.EXHAUSTED))
		{
			change.newValue = 1;
		}
		if (change.newValue != change.oldValue)
		{
			if (GameState.Get().IsTurnStartManagerActive() && m_entity.IsControlledByFriendlySidePlayer())
			{
				TurnStartManager.Get().NotifyOfExhaustedChange(this, change);
			}
			else
			{
				ShowExhaustedChange(change.newValue);
			}
		}
	}

	public void OnTagChanged(TagDelta change, bool fromShowEntity)
	{
		if (TagVisualConfiguration.Get() != null)
		{
			TagVisualConfiguration.Get().ProcessTagChange((GAME_TAG)change.tag, this, fromShowEntity, change);
		}
		switch ((GAME_TAG)change.tag)
		{
		case GAME_TAG.DECK_ACTION_COST:
			DoOptionHighlight(GameState.Get());
			break;
		case GAME_TAG.FAKE_ZONE:
		case GAME_TAG.FAKE_ZONE_POSITION:
			SetPredictedZonePosition(0);
			break;
		case GAME_TAG.LETTUCE_ROLE:
			if (!CanShowActorVisuals())
			{
				return;
			}
			if (m_actor != null)
			{
				m_actor.UpdateAllComponents();
			}
			break;
		case GAME_TAG.LETTUCE_IS_COMBAT_ACTION_TAKEN:
		case GAME_TAG.LETTUCE_ABILITY_TILE_VISUAL_SELF_ONLY:
		case GAME_TAG.LETTUCE_ABILITY_TILE_VISUAL_ALL_VISIBLE:
			if (!CanShowActorVisuals())
			{
				return;
			}
			if (GameState.Get()?.GetGameEntity() is LettuceMissionEntity lettuceGameEntity)
			{
				lettuceGameEntity.UpdateAllMercenaryAbilityOrderBubbleText();
			}
			foreach (int entityID in m_entity.GetLettuceAbilityEntityIDs())
			{
				Card abilityCard = GameState.Get()?.GetEntity(entityID)?.GetCard();
				if (abilityCard != null && abilityCard.CanShowActorVisuals() && abilityCard.GetActor() is LettuceAbilityActor actor)
				{
					actor.UpdateCheckMarkObject();
				}
			}
			break;
		case GAME_TAG.TAG_SCRIPT_DATA_NUM_1:
			InputManager.Get()?.ForceRefreshTargetingArrowText();
			break;
		case GAME_TAG.LOCATION_ACTION_COOLDOWN:
			if (m_entity != null && change.oldValue != 0 && change.newValue == 0 && m_entity.GetTag(GAME_TAG.EXHAUSTED) == 1)
			{
				ShowExhaustedChange(2);
			}
			break;
		case GAME_TAG.STEALTH:
			if (m_entity != null && m_entity.HasTaunt() && m_actor != null)
			{
				m_actor.ActivateTaunt();
			}
			break;
		case GAME_TAG.DISPLAY_CARD_ON_MOUSEOVER:
			RefreshCardsInTooltip();
			break;
		case GAME_TAG.FORGE_REVEALED:
			if (m_entity != null && (m_entity.IsControlledByOpposingSidePlayer() || (GameMgr.Get() != null && GameMgr.Get().IsSpectator())))
			{
				StartCoroutine(WaitAndForgeCard());
			}
			break;
		case GAME_TAG.EXHAUSTED:
			if (m_actor != null)
			{
				m_actor.UpdateTitanComponents();
			}
			break;
		case GAME_TAG.BACON_COSTS_HEALTH_TO_BUY:
			if (m_actor != null)
			{
				m_actor.UpdateMeshComponents();
			}
			break;
		case GAME_TAG.BACON_TURNS_LEFT_TO_DISCOVER_TRINKET:
			if (m_actor != null)
			{
				BaconTrinketWidget trinket2 = m_actor.GetComponent<BaconTrinketWidget>();
				if (trinket2 != null)
				{
					trinket2.UpdateTrinketState(turnLeftChanged: true, isPotentialTrinketChanged: false, change.newValue);
				}
			}
			break;
		case GAME_TAG.BACON_IS_POTENTIAL_TRINKET:
			if (m_actor != null)
			{
				BaconTrinketWidget trinket = m_actor.GetComponent<BaconTrinketWidget>();
				if (trinket != null)
				{
					trinket.UpdateTrinketState(turnLeftChanged: false, isPotentialTrinketChanged: true);
					PlayerLeaderboardManager.Get()?.NotifyBattlegroundsTrinketEnabledDirty();
				}
			}
			break;
		case GAME_TAG.BACON_TURNS_TILL_ACTIVE:
			if (m_actor != null)
			{
				AnomalyWidget anomaly = m_actor.GetComponent<AnomalyWidget>();
				if (anomaly != null)
				{
					bool unlocking = change.newValue == 0 && change.oldValue > 0;
					anomaly.UpdateTurnsTillActiveState(unlocking);
				}
			}
			break;
		case GAME_TAG.ELITE:
			if (m_actor != null)
			{
				m_actor.UpdateMeshComponents();
			}
			break;
		case GAME_TAG.LAUNCHPAD:
			if (change.newValue == 0 && change.oldValue > 0)
			{
				InputManager.Get()?.HidePlayerStarshipUI();
			}
			break;
		case GAME_TAG.BACON_NUM_MULLIGAN_REFRESH_USED:
			if (m_actor != null)
			{
				PlayerLeaderboardMainCardActor baconHeroActor2 = m_actor.GetComponent<PlayerLeaderboardMainCardActor>();
				Entity gameEntity2 = GameState.Get().GetGameEntity();
				if (baconHeroActor2 != null && gameEntity2 != null)
				{
					bool hasRerollToken = NetCache.Get().GetBattlegroundsTokenBalance() > 0;
					baconHeroActor2.ClearRerollButtonForcedDisabledState();
					baconHeroActor2.SetShowHeroRerollButton(show: true, hasRerollToken && change.newValue < gameEntity2.GetTag(GAME_TAG.BACON_NUM_MAX_REROLL_PER_HERO));
				}
				if (MulliganManager.Get() != null)
				{
					MulliganManager.Get().UpdateBaconRerollButtons(resetForceDisabledState: true);
				}
			}
			break;
		case GAME_TAG.BACON_LOCKED_MULLIGAN_HERO:
			if (m_actor != null)
			{
				PlayerLeaderboardMainCardActor baconHeroActor = m_actor.GetComponent<PlayerLeaderboardMainCardActor>();
				GameEntity gameEntity = GameState.Get().GetGameEntity();
				if (baconHeroActor != null && gameEntity != null)
				{
					baconHeroActor.ClearRerollButtonForcedDisabledState();
					gameEntity.ConfigureLockedMulliganCardActor(baconHeroActor, change.newValue != 0);
					baconHeroActor.SetShowHeroRerollButton(change.newValue == 0, null);
				}
			}
			break;
		case GAME_TAG.CLASS:
		case GAME_TAG.MULTIPLE_CLASSES:
			if (!m_entity.IsHero())
			{
				break;
			}
			foreach (Card card in GetController().GetHandZone().GetCards())
			{
				if (card.GetEntity().IsMultiClass())
				{
					card.UpdateActorComponents();
				}
			}
			break;
		case GAME_TAG.HERO_POWER_DISABLED:
			if (GetEntity() != null)
			{
				HandleHeroPowerDisabledTagChanged(change);
			}
			break;
		}
		if (m_entity != null)
		{
			m_entity.GetCardTextBuilder()?.OnTagChange(this, change);
		}
		if (m_actor != null)
		{
			m_actor.UpdateDiamondCardArt();
		}
	}

	public void ActivateDormantStateVisual()
	{
		m_actor.ActivateSpellBirthState(SpellType.DORMANT);
		if (m_entity.IsFrozen())
		{
			m_actor.ActivateSpellDeathState(SpellType.FROZEN);
		}
		if (m_entity.IsSilenced())
		{
			m_actor.ActivateSpellDeathState(SpellType.SILENCE);
		}
		DeactivateLifetimeEffects();
	}

	public void DeactivateDormantStateVisual()
	{
		m_actor.ActivateSpellDeathState(SpellType.DORMANT);
		if (m_entity.IsFrozen())
		{
			m_actor.ActivateSpellBirthState(SpellType.FROZEN);
		}
		if (m_entity.IsSilenced())
		{
			m_actor.ActivateSpellBirthState(SpellType.SILENCE);
		}
		ActivateLifetimeEffects();
		ActivateActorSpell(SpellType.AWAKEN_FROM_DORMANT);
		if (m_entity.IsControlledByFriendlySidePlayer())
		{
			if (m_entity.GetRealTimeSpellpower() > 0 || m_entity.GetRealTimeSpellpowerDouble())
			{
				ZoneMgr.Get().OnSpellPowerEntityEnteredPlay(m_entity.GetSpellPowerSchool());
			}
			if (m_entity.GetRealTimeHealingDoeDamageHint())
			{
				ZoneMgr.Get().OnHealingDoesDamageEntityEnteredPlay();
			}
			if (m_entity.GetRealTimeLifestealDoesDamageHint())
			{
				ZoneMgr.Get().OnLifestealDoesDamageEntityEnteredPlay();
			}
		}
		if (m_entity.IsAsleep())
		{
			m_actor.ActivateSpellBirthState(SpellType.Zzz);
		}
	}

	public void UpdateQuestUI()
	{
		if (m_entity != null && m_entity.IsQuest() && !(m_actor == null))
		{
			QuestController questController = m_actor.GetComponent<QuestController>();
			if (questController == null)
			{
				Log.Gameplay.PrintError("Quest card {0} does not have a QuestController component.", this);
			}
			else
			{
				questController.UpdateQuestUI();
			}
		}
	}

	public void UpdateQuestlineUI()
	{
		if (m_entity != null && m_entity.IsQuestline() && !(m_actor == null))
		{
			QuestlineController questlineController = m_actor.GetComponent<QuestlineController>();
			if (questlineController == null)
			{
				Log.Gameplay.PrintError("Questline card {0} does not have a QuestlineController component.", this);
			}
			else
			{
				questlineController.UpdateQuestlineUI();
			}
		}
	}

	public void UpdateSideQuestUI(bool allowQuestComplete)
	{
		if (m_entity != null && m_entity.IsSideQuest() && !(m_actor == null))
		{
			SideQuestController sideQuestController = m_actor.GetComponent<SideQuestController>();
			if (sideQuestController == null)
			{
				Log.Gameplay.PrintError("SideQuest card {0} does not have a SideQuestController component.", this);
			}
			else
			{
				sideQuestController.UpdateQuestUI(allowQuestComplete);
			}
		}
	}

	public void UpdateObjectiveUI()
	{
		if (m_entity != null && m_entity.IsObjective() && !(m_actor == null))
		{
			ObjectiveController objectiveController = m_actor.GetComponent<ObjectiveController>();
			if (objectiveController == null)
			{
				Log.Gameplay.PrintError("Objective card {0} does not have a ObjectiveController component.", this);
			}
			else
			{
				objectiveController.UpdateObjectiveUI();
			}
		}
	}

	public void UpdatePuzzleUI()
	{
		if (m_entity != null && m_entity.IsPuzzle() && !(m_actor == null))
		{
			PuzzleController puzzleController = m_actor.GetComponent<PuzzleController>();
			if (puzzleController == null)
			{
				Log.Gameplay.PrintError("Puzzle card {0} does not have a PuzzleController component.", this);
			}
			else
			{
				puzzleController.UpdatePuzzleUI();
			}
		}
	}

	public bool CanShowActorVisuals()
	{
		if (m_entity.IsLoadingAssets())
		{
			return false;
		}
		if (m_actor == null)
		{
			return false;
		}
		if (!m_actor.IsShown())
		{
			return false;
		}
		return true;
	}

	private bool CanShowSecretActorVisuals()
	{
		if (m_entity.IsLoadingAssets())
		{
			return false;
		}
		if (m_actor == null)
		{
			return false;
		}
		if (m_actorReady && !m_actor.IsShown())
		{
			return false;
		}
		return true;
	}

	public bool ShouldShowImmuneVisuals()
	{
		if (m_entity != null && m_entity.HasTag(GAME_TAG.IMMUNE))
		{
			return !m_entity.HasTag(GAME_TAG.DONT_SHOW_IMMUNE);
		}
		return false;
	}

	public bool CanShowCoinManaGem()
	{
		if (GetZone() is ZoneBattlegroundQuestReward || GetZone() is ZoneBattlegroundTrinket)
		{
			return false;
		}
		if (GetZone() is ZoneTeammatePlay && GetEntity() != null && (GetEntity().IsBattlegroundQuestReward() || GetEntity().IsBattlegroundTrinket()))
		{
			return false;
		}
		Entity entity = GetEntity();
		if (entity != null && entity != null && entity.GetTag<TAG_CARD_ALTERNATE_COST>(GAME_TAG.CARD_ALTERNATE_COST) == TAG_CARD_ALTERNATE_COST.HEALTH)
		{
			return false;
		}
		return true;
	}

	public void ActivateStateSpells(bool forceActivate = false)
	{
		if (m_actor == null)
		{
			return;
		}
		if (m_entity != null && m_entity.IsHeroPower())
		{
			UpdateHeroPowerRelatedVisual();
		}
		TagVisualConfiguration.Get().ActivateStateSpells(this);
		TAG_ZONE zoneTag = ((GetZone() != null) ? GetZone().m_ServerTag : TAG_ZONE.SETASIDE);
		if (zoneTag == TAG_ZONE.HAND)
		{
			ActivateHandStateSpells(forceActivate);
		}
		else if (m_entity != null && (zoneTag == TAG_ZONE.PLAY || zoneTag == TAG_ZONE.SECRET))
		{
			bool isExhausted = m_entity.IsExhausted();
			if (m_entity.IsDisabledHeroPower())
			{
				isExhausted = true;
			}
			ShowExhaustedChange(isExhausted);
		}
		if (zoneTag == TAG_ZONE.PLAY && m_entity.IsLettuceAbility())
		{
			ShowExhaustedChange(exhausted: false);
		}
	}

	public void UpdateHeroPowerRelatedVisual()
	{
		if (!m_entity.IsHeroPower())
		{
			return;
		}
		Player controller = m_entity.GetController();
		if (controller != null)
		{
			if (controller.HasTag(GAME_TAG.STEADY_SHOT_CAN_TARGET) && m_entity.HasClass(TAG_CLASS.HUNTER))
			{
				m_actor.ActivateSpellBirthState(SpellType.STEADY_SHOT_CAN_TARGET);
			}
			else
			{
				m_actor.ActivateSpellDeathState(SpellType.STEADY_SHOT_CAN_TARGET);
			}
			if (controller.HasTag(GAME_TAG.CURRENT_HEROPOWER_DAMAGE_BONUS) && controller.IsHeroPowerAffectedByBonusDamage())
			{
				m_actor.ActivateSpellBirthState(SpellType.CURRENT_HEROPOWER_DAMAGE_BONUS);
			}
			else
			{
				m_actor.ActivateSpellDeathState(SpellType.CURRENT_HEROPOWER_DAMAGE_BONUS);
			}
		}
	}

	public void ActivateHandStateSpells(bool forceActivate = false)
	{
		m_entity.GetController();
		if ((m_entity.IsCardButton() || m_entity.IsSpell()) && m_playEffect != null)
		{
			SpellUtils.ActivateCancelIfNecessary(m_playEffect.GetSpell(loadIfNeeded: false));
		}
		if (m_entity.IsSpell())
		{
			SpellUtils.ActivateCancelIfNecessary(GetActorSpell(SpellType.POWER_UP, loadIfNeeded: false));
		}
		if (TagVisualConfiguration.Get() != null)
		{
			TagVisualConfiguration.Get().ActivateHandStateSpells(this, forceActivate);
		}
	}

	public void DeactivateHandStateSpells(Actor actor = null)
	{
		if (actor == null)
		{
			if (m_actor == null)
			{
				return;
			}
			actor = m_actor;
		}
		if (TagVisualConfiguration.Get() != null)
		{
			TagVisualConfiguration.Get().DeactivateHandStateSpells(this, actor);
		}
		if (actor.UseTechLevelManaGem())
		{
			actor.ReleaseSpell(SpellType.TECH_LEVEL_MANA_GEM);
		}
		if (actor.UseCoinManaGem())
		{
			actor.ReleaseSpell(SpellType.COIN_MANA_GEM);
		}
		if (m_questRewardActor != null && m_questRewardActor.UseCoinManaGem())
		{
			m_questRewardActor.ReleaseSpell(SpellType.COIN_MANA_GEM);
		}
	}

	public void ActivateActorArmsDealingSpell()
	{
		if (CardStandInIsInteractive())
		{
			PowerTaskList curPowerTaskList = GameState.Get().GetPowerProcessor().GetCurrentTaskList();
			if (curPowerTaskList != null && curPowerTaskList.IsBlock())
			{
				StartCoroutine(WaitPowerTaskListAndActivateArmsDealing(curPowerTaskList));
			}
			else
			{
				m_actor.ActivateSpellBirthState(SpellType.ARMS_DEALING);
			}
		}
		else
		{
			Spell spell = m_actor.GetSpell(SpellType.ARMS_DEALING);
			if (spell != null)
			{
				spell.ActivateState(SpellStateType.IDLE);
			}
		}
	}

	private IEnumerator WaitPowerTaskListAndActivateArmsDealing(PowerTaskList curPowerTaskList)
	{
		while (!curPowerTaskList.IsComplete())
		{
			yield return null;
		}
		if (GetZone() is ZoneHand)
		{
			m_actor.ActivateSpellBirthState(SpellType.ARMS_DEALING);
		}
	}

	public void ToggleDeathrattle(bool on)
	{
		if (on)
		{
			m_actor.ActivateSpellBirthState(SpellType.DEATHRATTLE_IDLE);
		}
		else
		{
			m_actor.ActivateSpellDeathState(SpellType.DEATHRATTLE_IDLE);
		}
	}

	public void UpdateBauble()
	{
		if (IsBaubleAnimating())
		{
			return;
		}
		DeactivateBaubles();
		SpellType spellToActivate = m_entity.GetPrioritizedBaubleSpellType();
		if (spellToActivate == SpellType.NONE || !(m_actor != null))
		{
			return;
		}
		Spell baubleSpell = m_actor.GetSpell(spellToActivate);
		if (baubleSpell != null)
		{
			baubleSpell.ClearPositionDirtyFlag();
			baubleSpell.ActivateState(SpellStateType.BIRTH);
			if (spellToActivate == SpellType.AVENGE || spellToActivate == SpellType.TRIGGER_UPBEAT || spellToActivate == SpellType.TRIGGER_XY_STAY)
			{
				baubleSpell.SetSource(base.gameObject);
			}
		}
	}

	public void DeactivateBaubles()
	{
		SpellType spellToActivate = m_entity.GetPrioritizedBaubleSpellType();
		SpellType[] array = new SpellType[16]
		{
			SpellType.TRIGGER,
			SpellType.FAST_TRIGGER,
			SpellType.TRIGGER_XY,
			SpellType.TRIGGER_XY_STAY,
			SpellType.POISONOUS,
			SpellType.POISONOUS_INSTANT,
			SpellType.VENOMOUS,
			SpellType.INSPIRE,
			SpellType.LIFESTEAL,
			SpellType.OVERKILL,
			SpellType.SPELLBURST,
			SpellType.FRENZY,
			SpellType.AVENGE,
			SpellType.HONORABLEKILL,
			SpellType.TRIGGER_AND_SPELLBURST,
			SpellType.LIFESTEAL_AND_SPELLBURST
		};
		foreach (SpellType spellType in array)
		{
			if (spellToActivate != spellType)
			{
				SpellUtils.ActivateDeathIfNecessary(GetActorSpell(spellType, loadIfNeeded: false));
			}
		}
	}

	public bool IsBaubleAnimating()
	{
		return m_isBaubleAnimating;
	}

	public void SetIsBaubleAnimating(bool isAnimating)
	{
		m_isBaubleAnimating = isAnimating;
	}

	public void ShowExhaustedChange(int val)
	{
		if (m_entity.IsLocation())
		{
			if (m_entity.GetCurrentHealth() > 0)
			{
				if (m_entity.GetTag(GAME_TAG.EXHAUSTED) == 1 && m_entity.GetTag(GAME_TAG.LOCATION_ACTION_COOLDOWN) == 0)
				{
					val = 2;
				}
				StartCoroutine(PlayLocationAnimation(val));
			}
		}
		else
		{
			bool exhausted = val == 1;
			ShowExhaustedChange(exhausted);
		}
	}

	public void ShowExhaustedChange(bool exhausted)
	{
		GameState.Get()?.GetGameEntity();
		bool isCoinBasedHeroBuddy = m_entity.IsCoinBasedHeroBuddy();
		if (m_entity.IsHeroPower() || m_entity.IsCoinBasedHeroBuddy())
		{
			StopCoroutine("PlayHeroPowerAnimation");
			StartCoroutine("PlayHeroPowerAnimation", exhausted);
			if (isCoinBasedHeroBuddy)
			{
				GameObject gemCostObject = GameObjectUtils.FindChildBySubstring(base.gameObject, "GemCostObject");
				if (gemCostObject != null)
				{
					gemCostObject.SetActive(!exhausted);
				}
			}
		}
		else if (m_entity.IsWeapon() || m_entity.IsBattlegroundHeroBuddy())
		{
			if (exhausted)
			{
				SheatheWeaponOrHeroBuddy();
			}
			else
			{
				UnSheatheWeaponOrHeroBuddy();
			}
		}
		else if (m_entity.IsSecret())
		{
			StartCoroutine(ShowSecretExhaustedChange(exhausted));
		}
	}

	public void DisableHeroPowerFlipSoundOnce()
	{
		m_disableHeroPowerFlipSoundOnce = true;
	}

	private IEnumerator PlayHeroPowerAnimation(bool exhausted)
	{
		string animationName;
		if (exhausted)
		{
			animationName = (UniversalInputManager.UsePhoneUI ? "HeroPower_Used_phone" : "HeroPower_Used");
			if (m_actor != null && m_actor.UseCoinManaGem())
			{
				Spell spellsCoinManaGem = m_actor.GetSpellIfLoaded(SpellType.COIN_MANA_GEM);
				if (spellsCoinManaGem != null)
				{
					spellsCoinManaGem.Deactivate();
				}
			}
		}
		else
		{
			animationName = (UniversalInputManager.UsePhoneUI ? "HeroPower_Restore_phone" : "HeroPower_Restore");
			if (m_actor != null && m_actor.UseCoinManaGem())
			{
				Spell spellsCoinManaGem2 = m_actor.GetSpellIfLoaded(SpellType.COIN_MANA_GEM);
				if (spellsCoinManaGem2 != null)
				{
					spellsCoinManaGem2.Reactivate();
				}
			}
		}
		SetInputEnabled(enabled: false);
		while (true)
		{
			if (m_actor == null || m_actor.gameObject == null)
			{
				yield break;
			}
			MinionShake shake = m_actor.gameObject.GetComponentInChildren<MinionShake>();
			if (shake == null)
			{
				yield break;
			}
			if (!shake.isShaking())
			{
				break;
			}
			yield return null;
		}
		while (true)
		{
			if (m_actor == null || m_actor.gameObject == null)
			{
				yield break;
			}
			if (m_actor.gameObject.transform.parent == base.transform)
			{
				break;
			}
			yield return null;
		}
		if (m_disableHeroPowerFlipSoundOnce)
		{
			m_disableHeroPowerFlipSoundOnce = false;
		}
		else
		{
			bool canPlaySound = true;
			if (TeammateBoardViewer.Get() != null && !TeammateBoardViewer.Get().IsViewingTeammate())
			{
				canPlaySound = !m_actor.IsTeammateActor();
			}
			if (canPlaySound)
			{
				string flipSoundName = (exhausted ? "hero_power_icon_flip_off.prefab:621ead6ff672f5b4bbfd6578ee217a42" : "hero_power_icon_flip_on.prefab:e1491b367801f6b4395dc63ce0b08f0a");
				if (ServiceManager.TryGet<SoundManager>(out var soundManager))
				{
					soundManager.LoadAndPlay(flipSoundName);
				}
			}
		}
		if (m_actor.GetComponent<Animation>() != null)
		{
			m_actor.GetComponent<Animation>().Play(animationName);
		}
		Spell spell;
		while (true)
		{
			spell = GetPlaySpell(0);
			if (spell == null || spell.GetActiveState() == SpellStateType.NONE)
			{
				break;
			}
			yield return null;
		}
		SetInputEnabled(enabled: true);
		if (exhausted && (GameState.Get()?.IsValidOption(m_entity, null) ?? false) && !m_entity.HasSubCards() && spell != null)
		{
			SetInputEnabled(enabled: false);
		}
	}

	private IEnumerator PlayLocationAnimation(int stateVal)
	{
		string animationName;
		switch (stateVal)
		{
		case 0:
			animationName = "Location_AjarToOpen";
			PlayFSMSoundEvent("Ajar_To_Open");
			break;
		case 1:
			animationName = "Location_OpenToClose";
			PlayFSMSoundEvent("Open_To_Close");
			break;
		default:
			animationName = "Location_ClosedToAjar";
			PlayFSMSoundEvent("Close_To_Ajar");
			break;
		}
		SetInputEnabled(enabled: false);
		if (m_actor == null)
		{
			yield break;
		}
		MinionShake shake = m_actor.gameObject.GetComponentInChildren<MinionShake>();
		if (shake == null)
		{
			yield break;
		}
		while (shake.isShaking())
		{
			yield return null;
		}
		while (m_actor.gameObject.transform.parent != base.transform)
		{
			yield return null;
		}
		if (m_disableHeroPowerFlipSoundOnce)
		{
			m_disableHeroPowerFlipSoundOnce = false;
		}
		if (m_actor.GetComponent<Animation>() != null)
		{
			m_actor.GetComponent<Animation>().Play(animationName);
		}
		Spell spell = GetPlaySpell(0);
		if (spell != null)
		{
			while (spell.GetActiveState() != 0)
			{
				yield return null;
			}
		}
		SetInputEnabled(enabled: true);
	}

	private void PlayFSMSoundEvent(string fsmevent)
	{
		Actor cardActor = GetActor();
		if (cardActor == null)
		{
			return;
		}
		GameObject root = cardActor.GetRootObject();
		if (!(root == null))
		{
			PlayMakerFSM fsm = root.GetComponent<PlayMakerFSM>();
			if (!(fsm == null))
			{
				fsm.SendEvent(fsmevent);
			}
		}
	}

	private void SheatheWeaponOrHeroBuddy()
	{
		if (GetZone() is ZoneWeapon)
		{
			m_actor.GetAttackObject().ScaleToZero();
			ActivateActorSpell(SpellType.SHEATHE);
		}
		else if (GetZone() is ZoneBattlegroundHeroBuddy)
		{
			if (!GameState.Get().IsMulliganManagerActive())
			{
				ActivateActorSpell(SpellType.SHEATHE);
			}
		}
		else if (!(GetZone() == null) && !(GetZone() is ZoneGraveyard))
		{
			Log.Gameplay.PrintError("Failed to process Card.SheatheWeapon() card:{0} zone:{1}", this, GetZone());
		}
		Player controller = GetController();
		if (controller != null)
		{
			Card heroCard = controller.GetHeroCard();
			if (heroCard != null)
			{
				heroCard.NotifyOfWeaponSheathed(m_entity);
			}
		}
	}

	private void UnSheatheWeaponOrHeroBuddy()
	{
		if (GetZone() is ZoneWeapon)
		{
			m_actor.GetAttackObject().Enlarge(1f);
			ActivateActorSpell(SpellType.UNSHEATHE);
		}
		else if (GetZone() is ZoneBattlegroundHeroBuddy)
		{
			if (!GameState.Get().IsMulliganManagerActive())
			{
				ActivateActorSpell(SpellType.UNSHEATHE);
			}
		}
		else if (!(GetZone() == null) && !(GetZone() is ZoneGraveyard))
		{
			Log.Gameplay.PrintError("Failed to process Card.UnSheatheWeapon() card:{0} zone:{1}", this, GetZone());
		}
	}

	private IEnumerator ShowSecretExhaustedChange(bool exhausted)
	{
		while (!m_actorReady)
		{
			yield return null;
		}
		if (m_entity.IsDarkWandererSecret())
		{
			yield break;
		}
		Spell spell = m_actor.GetComponent<Spell>();
		while (spell.GetActiveState() != 0)
		{
			yield return null;
		}
		if (CanShowSecretZoneCard())
		{
			if (exhausted)
			{
				SheatheSecret(spell);
			}
			else
			{
				UnSheatheSecret(spell);
			}
		}
	}

	private void SheatheSecret(Spell spell)
	{
		if (!m_secretSheathed && m_entity.IsExhausted())
		{
			m_secretSheathed = true;
			spell.ActivateState(SpellStateType.IDLE);
		}
	}

	private void UnSheatheSecret(Spell spell)
	{
		if (m_secretSheathed && !m_entity.IsExhausted())
		{
			m_secretSheathed = false;
			spell.ActivateState(SpellStateType.DEATH);
		}
	}

	public void OnEnchantmentAdded(int oldEnchantmentCount, Entity enchantment)
	{
		if (CanShowActorVisuals() && IsActorReady())
		{
			UpdateBauble();
		}
		Spell spell = null;
		if (GameState.Get() != null && GameState.Get().GetGameEntity() != null && GameState.Get().GetBooleanGameOption(GameEntityOption.ALLOW_ENCHANTMENT_SPARKLES))
		{
			switch (enchantment.GetEnchantmentBirthVisual())
			{
			case TAG_ENCHANTMENT_VISUAL.POSITIVE:
				spell = GetActorSpell(SpellType.ENCHANT_POSITIVE);
				break;
			case TAG_ENCHANTMENT_VISUAL.NEGATIVE:
				spell = GetActorSpell(SpellType.ENCHANT_NEGATIVE);
				break;
			case TAG_ENCHANTMENT_VISUAL.NEUTRAL:
				spell = GetActorSpell(SpellType.ENCHANT_NEUTRAL);
				break;
			}
		}
		if (spell == null)
		{
			UpdateEnchantments();
			UpdateTooltip();
		}
		else
		{
			spell.AddStateFinishedCallback(OnEnchantmentSpellStateFinished);
			spell.ActivateState(SpellStateType.BIRTH);
		}
	}

	public void OnEnchantmentRemoved(int oldEnchantmentCount, Entity enchantment)
	{
		if (CanShowActorVisuals())
		{
			UpdateBauble();
		}
		Spell spell = null;
		if (GameState.Get() != null && GameState.Get().GetGameEntity() != null && GameState.Get().GetBooleanGameOption(GameEntityOption.ALLOW_ENCHANTMENT_SPARKLES))
		{
			switch (enchantment.GetEnchantmentBirthVisual())
			{
			case TAG_ENCHANTMENT_VISUAL.POSITIVE:
				spell = GetActorSpell(SpellType.ENCHANT_POSITIVE);
				break;
			case TAG_ENCHANTMENT_VISUAL.NEGATIVE:
				spell = GetActorSpell(SpellType.ENCHANT_NEGATIVE);
				break;
			case TAG_ENCHANTMENT_VISUAL.NEUTRAL:
				spell = GetActorSpell(SpellType.ENCHANT_NEUTRAL);
				break;
			}
		}
		if (spell == null)
		{
			UpdateEnchantments();
			UpdateTooltip();
		}
		else
		{
			spell.AddStateFinishedCallback(OnEnchantmentSpellStateFinished);
			spell.ActivateState(SpellStateType.DEATH);
		}
	}

	private void OnEnchantmentSpellStateFinished(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (prevStateType == SpellStateType.BIRTH || prevStateType == SpellStateType.DEATH)
		{
			spell.RemoveStateFinishedCallback(OnEnchantmentSpellStateFinished);
			UpdateEnchantments();
			UpdateTooltip();
		}
	}

	public void UpdateEnchantments()
	{
		if (GameState.Get() != null && GameState.Get().GetGameEntity() != null && !GameState.Get().GetBooleanGameOption(GameEntityOption.ALLOW_ENCHANTMENT_SPARKLES))
		{
			return;
		}
		List<Entity> enchantments = m_entity.GetEnchantments();
		Spell positiveSpell = GetActorSpell(SpellType.ENCHANT_POSITIVE);
		Spell negativeSpell = GetActorSpell(SpellType.ENCHANT_NEGATIVE);
		Spell neutralSpell = GetActorSpell(SpellType.ENCHANT_NEUTRAL);
		Spell oldIdleSpell = null;
		if (positiveSpell != null && positiveSpell.GetActiveState() == SpellStateType.IDLE)
		{
			oldIdleSpell = positiveSpell;
		}
		else if (negativeSpell != null && negativeSpell.GetActiveState() == SpellStateType.IDLE)
		{
			oldIdleSpell = negativeSpell;
		}
		else if (neutralSpell != null && neutralSpell.GetActiveState() == SpellStateType.IDLE)
		{
			oldIdleSpell = neutralSpell;
		}
		if (enchantments.Count == 0)
		{
			if (oldIdleSpell != null)
			{
				oldIdleSpell.ActivateState(SpellStateType.DEATH);
			}
			return;
		}
		int polarity = 0;
		bool canShowNeutral = false;
		foreach (Entity item in enchantments)
		{
			TAG_ENCHANTMENT_VISUAL enchantVisual = item.GetEnchantmentIdleVisual();
			switch (enchantVisual)
			{
			case TAG_ENCHANTMENT_VISUAL.POSITIVE:
				polarity++;
				break;
			case TAG_ENCHANTMENT_VISUAL.NEGATIVE:
				polarity--;
				break;
			}
			if (enchantVisual != 0)
			{
				canShowNeutral = true;
			}
		}
		Spell newIdleSpell = null;
		if (polarity > 0)
		{
			newIdleSpell = positiveSpell;
		}
		else if (polarity < 0)
		{
			newIdleSpell = negativeSpell;
		}
		else if (canShowNeutral)
		{
			newIdleSpell = neutralSpell;
		}
		if (oldIdleSpell != null && oldIdleSpell != newIdleSpell)
		{
			oldIdleSpell.Deactivate();
		}
		if (newIdleSpell != null)
		{
			newIdleSpell.ActivateState(SpellStateType.BIRTH);
		}
	}

	public Spell GetActorSpell(SpellType spellType, bool loadIfNeeded = true)
	{
		if (m_actor == null)
		{
			return null;
		}
		if (loadIfNeeded)
		{
			return m_actor.GetSpell(spellType);
		}
		return m_actor.GetSpellIfLoaded(spellType);
	}

	public Spell ActivateActorSpell(SpellType spellType)
	{
		return ActivateActorSpell(m_actor, spellType, null, null);
	}

	public Spell ActivateActorSpell(SpellType spellType, ISpellCallbackHandler<Spell>.FinishedCallback finishedCallback)
	{
		return ActivateActorSpell(m_actor, spellType, finishedCallback, null);
	}

	public Spell ActivateActorSpell(SpellType spellType, ISpellCallbackHandler<Spell>.FinishedCallback finishedCallback, ISpellCallbackHandler<Spell>.StateFinishedCallback stateFinishedCallback)
	{
		return ActivateActorSpell(m_actor, spellType, finishedCallback, stateFinishedCallback);
	}

	private Spell ActivateActorSpell(Actor actor, SpellType spellType)
	{
		return ActivateActorSpell(actor, spellType, null, null);
	}

	private Spell ActivateActorSpell(Actor actor, SpellType spellType, ISpellCallbackHandler<Spell>.FinishedCallback finishedCallback)
	{
		return ActivateActorSpell(actor, spellType, finishedCallback, null);
	}

	private Spell ActivateActorSpell(Actor actor, SpellType spellType, ISpellCallbackHandler<Spell>.FinishedCallback finishedCallback, ISpellCallbackHandler<Spell>.StateFinishedCallback stateFinishedCallback)
	{
		if (actor == null)
		{
			Log.Gameplay.Print($"{this}.ActivateActorSpell() - actor IS NULL spellType={spellType}");
			return null;
		}
		Spell spell = actor.GetSpell(spellType);
		if (spell == null)
		{
			Log.Gameplay.Print($"{this}.ActivateActorSpell() - spell IS NULL actor={actor} spellType={spellType}");
			return null;
		}
		ActivateSpell(spell, finishedCallback, stateFinishedCallback);
		return spell;
	}

	private void ActivateSpell(ISpell spell, ISpellCallbackHandler<Spell>.FinishedCallback finishedCallback)
	{
		ActivateSpell(spell, finishedCallback, null, null, null);
	}

	private void ActivateSpell(ISpell spell, ISpellCallbackHandler<Spell>.FinishedCallback finishedCallback, ISpellCallbackHandler<Spell>.StateFinishedCallback stateFinishedCallback)
	{
		ActivateSpell(spell, finishedCallback, null, stateFinishedCallback, null);
	}

	private void ActivateSpell(ISpell spell, ISpellCallbackHandler<Spell>.FinishedCallback finishedCallback, object finishedUserData, ISpellCallbackHandler<Spell>.StateFinishedCallback stateFinishedCallback)
	{
		ActivateSpell(spell, finishedCallback, finishedUserData, stateFinishedCallback, null);
	}

	private void ActivateSpell(ISpell spell, ISpellCallbackHandler<Spell>.FinishedCallback finishedCallback, object finishedUserData, ISpellCallbackHandler<Spell>.StateFinishedCallback stateFinishedCallback, object stateFinishedUserData)
	{
		Spell callbackSpell = spell as Spell;
		if (finishedCallback != null && callbackSpell != null)
		{
			callbackSpell.AddFinishedCallback(finishedCallback, finishedUserData);
		}
		if (stateFinishedCallback != null && callbackSpell != null)
		{
			callbackSpell.AddStateFinishedCallback(stateFinishedCallback, stateFinishedUserData);
		}
		if (spell.GetActiveState() == SpellStateType.NONE)
		{
			spell.Activate();
		}
	}

	public ISpell GetActorAttackSpellForInput()
	{
		if (m_actor == null)
		{
			Log.Gameplay.Print("{0}.GetActorAttackSpellForInput() - m_actor IS NULL", this);
			return null;
		}
		if (m_zone == null)
		{
			Log.Gameplay.Print("{0}.GetActorAttackSpellForInput() - m_zone IS NULL", this);
			return null;
		}
		ISpell spell = m_actor.GetSpell(SpellType.FRIENDLY_ATTACK);
		if (spell == null)
		{
			Log.Gameplay.Print("{0}.GetActorAttackSpellForInput() - {1} spell is null", this, SpellType.FRIENDLY_ATTACK);
			return null;
		}
		return spell;
	}

	public void FakeDeath()
	{
		if (!m_suppressKeywordDeaths)
		{
			StartCoroutine(WaitAndPrepareForDeathAnimation(m_actor));
		}
		ActivateDeathSpell(m_actor);
	}

	private ISpell ActivateDeathSpell(Actor actor)
	{
		bool standard;
		ISpell deathSpell = GetBestDeathSpell(actor, out standard);
		if (deathSpell == null)
		{
			Debug.LogError($"{this}.ActivateDeathSpell() - {SpellType.DEATH} is null");
			return null;
		}
		CleanUpCustomSpell(deathSpell, ref m_customDeathSpell);
		CleanUpCustomSpell(deathSpell, ref m_customDeathSpellOverride);
		m_activeDeathEffectCount++;
		Spell deathSpellAsSpell = deathSpell as Spell;
		if (standard)
		{
			if (m_actor != actor && deathSpellAsSpell != null)
			{
				deathSpellAsSpell.AddStateFinishedCallback(OnSpellStateFinished_DestroyActor);
			}
		}
		else
		{
			deathSpell.SetSource(base.gameObject);
			if (m_actor != actor && deathSpellAsSpell != null)
			{
				deathSpellAsSpell.AddStateFinishedCallback(OnSpellStateFinished_CustomDeath);
			}
			SpellUtils.SetCustomSpellParent(deathSpell, actor);
		}
		if (deathSpellAsSpell != null)
		{
			deathSpellAsSpell.AddFinishedCallback(OnSpellFinished_Death);
		}
		deathSpell.Activate();
		BoardEvents boardEvents = BoardEvents.Get();
		if (boardEvents != null)
		{
			boardEvents.DeathEvent(this);
		}
		return deathSpell;
	}

	public void ActivateHandSpawnSpell()
	{
		if (m_customSpawnSpellOverride == null)
		{
			ActivateDefaultSpawnSpell(OnSpellFinished_DefaultHandSpawn);
			return;
		}
		Entity creator = m_entity.GetCreator();
		Card creatorCard = null;
		if (creator != null && creator.IsMinion())
		{
			creatorCard = creator.GetCard();
		}
		if (creatorCard != null)
		{
			TransformUtil.CopyWorld(base.transform, creatorCard.transform);
		}
		ActivateCustomHandSpawnSpell(m_customSpawnSpellOverride, creatorCard);
	}

	private void ActivatePlaySpawnEffects(bool fallbackToDefault = true)
	{
		ISpell spawnSpell = m_customSpawnSpellOverride;
		if (spawnSpell == null)
		{
			spawnSpell = m_customSpawnSpell;
			if (spawnSpell == null)
			{
				if (fallbackToDefault)
				{
					ActivateDefaultSpawnSpell(OnSpellFinished_DefaultPlaySpawn);
				}
				return;
			}
		}
		if (m_zone is ZoneHeroPower)
		{
			m_actor.Hide();
		}
		ActivateCustomSpawnSpell(spawnSpell);
	}

	private SpellType GetBestReplacementWhenPlayedSpell(Entity spawningActor)
	{
		if (spawningActor == null)
		{
			return SpellType.SUMMON_IN;
		}
		SpellType spawnSpellType = SpellType.SUMMON_IN;
		if (spawningActor.HasTag(GAME_TAG.CREATED_BY_MINIATURIZE))
		{
			spawnSpellType = SpellType.MINIATURIZE_SUMMON_IN;
		}
		else if (spawningActor.HasTag(GAME_TAG.CREATED_BY_GIGANTIFY))
		{
			spawnSpellType = SpellType.GIGANTIFY_SUMMON_IN;
		}
		else if (spawningActor.HasTag(GAME_TAG.CREATED_BY_TWINSPELL))
		{
			spawnSpellType = ((GameState.Get().GetGameEntity().GetTag(GAME_TAG.USE_FAST_ACTOR_TRANSITION_ANIMATIONS) > 0) ? SpellType.TWINSPELL_SUMMON_IN_FAST : SpellType.TWINSPELL_SUMMON_IN);
		}
		else
		{
			Entity creator = GameState.Get().GetEntity(m_entity.GetTag(GAME_TAG.CREATOR));
			if (creator != null && creator.HasReplacementsWhenPlayed() && m_entity.IsSpell())
			{
				List<int> replacementsWhenPlayed = creator.GetReplacementsWhenPlayed();
				bool useFastAnimation = false;
				if (replacementsWhenPlayed != null && replacementsWhenPlayed.Contains(GameUtils.TranslateCardIdToDbId(m_entity.GetCardId())))
				{
					useFastAnimation = GameState.Get().GetGameEntity().GetTag(GAME_TAG.USE_FAST_ACTOR_TRANSITION_ANIMATIONS) > 0;
				}
				spawnSpellType = (useFastAnimation ? SpellType.TWINSPELL_SUMMON_IN_FAST : SpellType.TWINSPELL_SUMMON_IN);
			}
		}
		return spawnSpellType;
	}

	private void ActivateDefaultSpawnSpell(ISpellCallbackHandler<Spell>.FinishedCallback finishedCallback)
	{
		m_inputEnabled = false;
		m_actor.ToggleForceIdle(bOn: true);
		int num = m_entity.GetTag(GAME_TAG.PREMIUM);
		SpellType spellType = SpellType.SUMMON_IN;
		if (num == 2)
		{
			spellType = SpellType.SUMMON_IN_DIAMOND;
		}
		if (m_zone is ZoneHand)
		{
			if (m_entity.HasTag(GAME_TAG.GHOSTLY))
			{
				spellType = SpellType.GHOSTLY_SUMMON_IN;
			}
			if (m_entity.HasTag(GAME_TAG.CREATED_AS_ON_PLAY_REPLACEMENT))
			{
				spellType = GetBestReplacementWhenPlayedSpell(m_entity);
			}
		}
		else if ((m_entity.IsWeapon() && (m_zone is ZoneWeapon || m_zone is ZoneHeroPower)) || (m_entity.IsBattlegroundHeroBuddy() && m_zone is ZoneBattlegroundHeroBuddy) || (m_entity.IsBattlegroundQuestReward() && m_zone is ZoneBattlegroundQuestReward) || (m_entity.IsBattlegroundTrinket() && m_zone is ZoneBattlegroundTrinket) || (m_entity.IsAnomaly() && m_zone is ZoneBattlegroundAnomaly))
		{
			spellType = (m_entity.IsControlledByFriendlySidePlayer() ? SpellType.SUMMON_IN_FRIENDLY : SpellType.SUMMON_IN_OPPONENT);
		}
		if (m_zone is ZoneHand && m_entity.IsMercenary())
		{
			spellType = SpellType.SUMMON_IN_MERCENARY;
		}
		if (m_zone is ZoneHeroPower && m_entity.IsHeroPower() && m_actor.UseCoinManaGem())
		{
			m_actor.ActivateSpellBirthState(SpellType.COIN_MANA_GEM);
		}
		if ((object)ActivateActorSpell(spellType, finishedCallback) == null)
		{
			Debug.LogError($"{this}.ActivateDefaultSpawnSpell() - {spellType} is null");
		}
	}

	private void ActivateCustomSpawnSpell(ISpell spell)
	{
		spell.SetSource(base.gameObject);
		spell.RemoveAllTargets();
		spell.AddTarget(base.gameObject);
		if (spell is Spell s)
		{
			s.AddStateFinishedCallback(OnSpellStateFinished_ReleaseSpell);
			s.AddFinishedCallback(OnSpellFinished_CustomPlaySpawn);
		}
		SpellUtils.SetCustomSpellParent(spell, m_actor);
		spell.Activate();
	}

	private void ActivateCustomHandSpawnSpell(ISpell spell, Card creatorCard)
	{
		GameObject sourceObject = ((creatorCard == null) ? base.gameObject : creatorCard.gameObject);
		spell.SetSource(sourceObject);
		spell.RemoveAllTargets();
		spell.AddTarget(base.gameObject);
		if (spell is Spell s)
		{
			s.AddStateFinishedCallback(OnSpellStateFinished_ReleaseSpell);
			s.AddFinishedCallback(OnSpellFinished_CustomHandSpawn);
		}
		SpellUtils.SetCustomSpellParent(spell, m_actor);
		spell.Activate();
	}

	public void ActivateMinionSpawnEffects()
	{
		Entity creator = m_entity.GetCreator();
		Card creatorCard = null;
		if (creator != null && creator.IsMinion())
		{
			creatorCard = creator.GetCard();
		}
		if (creatorCard != null && !(creatorCard.GetZone() is ZonePlay) && !(creatorCard.GetZone() is ZoneGraveyard))
		{
			creatorCard = null;
		}
		if (creatorCard != null)
		{
			TransformUtil.CopyWorld(base.transform, creatorCard.transform);
		}
		bool standard;
		ISpell spell = GetBestSpawnSpell(out standard);
		if (standard)
		{
			if (creatorCard == null)
			{
				ActivateStandardSpawnMinionSpell();
			}
			else
			{
				StartCoroutine(ActivateCreatorSpawnMinionSpell(creator, creatorCard));
			}
		}
		else
		{
			ActivateCustomSpawnMinionSpell(spell, creatorCard);
		}
	}

	private IEnumerator ActivateCreatorSpawnMinionSpell(Entity creator, Card creatorCard)
	{
		while (creator.IsLoadingAssets() || !creatorCard.IsActorReady())
		{
			yield return 0;
		}
		if (creatorCard.ActivateCreatorSpawnMinionSpell() != null)
		{
			yield return new WaitForSeconds(0.9f);
		}
		ActivateStandardSpawnMinionSpell();
	}

	private ISpell ActivateCreatorSpawnMinionSpell()
	{
		if (m_entity.IsControlledByFriendlySidePlayer())
		{
			return ActivateActorSpell(SpellType.FRIENDLY_SPAWN_MINION_OR_LOCATION);
		}
		return ActivateActorSpell(SpellType.OPPONENT_SPAWN_MINION_OR_LOCATION);
	}

	private void ActivateStandardSpawnMinionSpell()
	{
		if (m_entity.IsControlledByFriendlySidePlayer())
		{
			m_activeSpawnSpell = ActivateActorSpell(SpellType.FRIENDLY_SPAWN_MINION_OR_LOCATION, OnSpellFinished_StandardSpawnCharacter);
		}
		else
		{
			m_activeSpawnSpell = ActivateActorSpell(SpellType.OPPONENT_SPAWN_MINION_OR_LOCATION, OnSpellFinished_StandardSpawnCharacter);
		}
		ActivateCharacterPlayEffects();
	}

	private void ActivateStandardSpawnHeroSpell()
	{
		if (m_entity.IsControlledByFriendlySidePlayer())
		{
			m_activeSpawnSpell = ActivateActorSpell(SpellType.FRIENDLY_SPAWN_HERO, OnSpellFinished_StandardSpawnCharacter);
		}
		else
		{
			m_activeSpawnSpell = ActivateActorSpell(SpellType.OPPONENT_SPAWN_HERO, OnSpellFinished_StandardSpawnCharacter);
		}
	}

	private void ActivateCustomSpawnMinionSpell(ISpell spell, Card creatorCard)
	{
		m_activeSpawnSpell = spell;
		GameObject sourceObject = ((creatorCard == null) ? base.gameObject : creatorCard.gameObject);
		spell.SetSource(sourceObject);
		spell.RemoveAllTargets();
		spell.AddTarget(base.gameObject);
		if (spell is Spell s)
		{
			s.AddStateFinishedCallback(OnSpellStateFinished_ReleaseSpell);
			s.AddFinishedCallback(OnSpellFinished_CustomSpawnMinion);
		}
		SpellUtils.SetCustomSpellParent(spell, m_actor);
		spell.Activate();
	}

	private IEnumerator ActivateReviveSpell()
	{
		while (m_activeDeathEffectCount > 0)
		{
			yield return 0;
		}
		ActivateStandardSpawnMinionSpell();
	}

	private IEnumerator ActivateActorBattlecrySpell()
	{
		if (IsLettuceAbility())
		{
			yield break;
		}
		ISpell battlecrySpell = GetActorSpell(SpellType.BATTLECRY);
		if (battlecrySpell == null || !(m_zone is ZonePlay) || InputManager.Get() == null || InputManager.Get().GetBattlecrySourceCard() != this)
		{
			yield break;
		}
		yield return new WaitForSeconds(0.01f);
		if (!(InputManager.Get() == null) && !(InputManager.Get().GetBattlecrySourceCard() != this))
		{
			if (battlecrySpell.GetActiveState() == SpellStateType.NONE)
			{
				battlecrySpell.ActivateState(SpellStateType.BIRTH);
			}
			Spell playSpell = GetPlaySpell(0);
			if ((bool)playSpell)
			{
				playSpell.ActivateState(SpellStateType.BIRTH);
			}
		}
	}

	private void CleanUpCustomSpell(ISpell chosenSpell, ref ISpell customSpell)
	{
		if (customSpell != null)
		{
			if (chosenSpell == customSpell)
			{
				customSpell = null;
			}
			else
			{
				UnityEngine.Object.Destroy(customSpell.GameObject);
			}
		}
	}

	private void OnSpellFinished_StandardSpawnCharacter(Spell spell, object userData)
	{
		m_actorReady = true;
		m_inputEnabled = true;
		m_actor.Show();
		ActivateStateSpells();
		RefreshActor();
		UpdateActorComponents();
		BoardEvents boardEvents = BoardEvents.Get();
		if (boardEvents != null)
		{
			boardEvents.SummonedEvent(this);
		}
	}

	private void OnSpellFinished_CustomSpawnMinion(Spell spell, object userData)
	{
		OnSpellFinished_StandardSpawnCharacter(spell, userData);
		CleanUpCustomSpell(spell, ref m_customSpawnSpell);
		CleanUpCustomSpell(spell, ref m_customSpawnSpellOverride);
		ActivateCharacterPlayEffects();
	}

	private void OnSpellFinished_DefaultHandSpawn(Spell spell, object userData)
	{
		m_actor.ToggleForceIdle(bOn: false);
		m_inputEnabled = true;
		ActivateStateSpells();
		RefreshActor();
		UpdateActorComponents();
	}

	private void OnSpellFinished_CustomHandSpawn(Spell spell, object userData)
	{
		OnSpellFinished_DefaultHandSpawn(spell, userData);
		CleanUpCustomSpell(spell, ref m_customSpawnSpellOverride);
	}

	private void OnSpellFinished_DefaultPlaySpawn(Spell spell, object userData)
	{
		m_actor.ToggleForceIdle(bOn: false);
		m_inputEnabled = true;
		if (m_zone != null)
		{
			ActivateStateSpells();
		}
		RefreshActor();
		UpdateActorComponents();
	}

	private void OnSpellFinished_CustomPlaySpawn(Spell spell, object userData)
	{
		OnSpellFinished_DefaultPlaySpawn(spell, userData);
		CleanUpCustomSpell(spell, ref m_customSpawnSpell);
		CleanUpCustomSpell(spell, ref m_customSpawnSpellOverride);
	}

	private void OnSpellFinished_StandardCardSummon(Spell spell, object userData)
	{
		m_actorReady = true;
		m_inputEnabled = true;
		ActivateStateSpells();
		RefreshActor();
		UpdateActorComponents();
	}

	private void OnSpellFinished_UpdateActorComponents(Spell spell, object userData)
	{
		UpdateActorComponents();
	}

	private void OnSpellFinished_Death(Spell spell, object userData)
	{
		m_suppressKeywordDeaths = false;
		m_keywordDeathDelaySec = 0.3f;
		m_activeDeathEffectCount--;
		GameState.Get()?.ClearCardBeingDrawn(this);
	}

	private void OnSpellStateFinished_DestroyActor(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (spell.GetActiveState() == SpellStateType.NONE)
		{
			if (m_zone is ZoneGraveyard)
			{
				PurgeSpells();
			}
			Actor actor = GameObjectUtils.FindComponentInThisOrParents<Actor>(spell.gameObject);
			if (actor == null)
			{
				Debug.LogWarning($"Card.OnSpellStateFinished_DestroyActor() - spell {spell} on Card {this} has no Actor ancestor");
			}
			else
			{
				actor.Destroy();
			}
		}
	}

	private void OnSpellStateFinished_ReleaseSpell(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (spell.GetActiveState() == SpellStateType.NONE)
		{
			SpellManager.Get().ReleaseSpell(spell);
		}
	}

	private void OnSpellStateFinished_CustomDeath(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (spell.GetActiveState() == SpellStateType.NONE)
		{
			Actor actor = GameObjectUtils.FindComponentInThisOrParents<Actor>(spell.gameObject);
			if (actor == null)
			{
				Debug.LogWarning($"Card.OnSpellStateFinished_CustomDeath() - spell {spell} on Card {this} has no Actor ancestor");
			}
			else
			{
				actor.Destroy();
			}
		}
	}

	public void UpdateActorState(bool forceHighlightRefresh = false)
	{
		if (m_actor == null || !m_shown || m_entity.IsBusy() || m_zone is ZoneGraveyard)
		{
			return;
		}
		if (!m_inputEnabled || (m_zone != null && !m_zone.IsInputEnabled()))
		{
			m_actor.SetActorState(ActorStateType.CARD_IDLE);
			m_actor.SetDeckActionHighlightState(DeckActionHighlightState.None);
			return;
		}
		if (m_mousedOverByTeammate)
		{
			m_actor.SetActorState(ActorStateType.CARD_MOUSE_OVER);
			return;
		}
		GameState state = GameState.Get();
		if (state != null && state.IsEntityInputEnabled(m_entity))
		{
			if (forceHighlightRefresh)
			{
				m_actor.SetActorState(ActorStateType.CARD_IDLE);
				m_actor.SetDeckActionHighlightState(DeckActionHighlightState.None);
			}
			switch (state.GetResponseMode())
			{
			case GameState.ResponseMode.CHOICE:
				if (DoChoiceHighlight(state))
				{
					return;
				}
				break;
			case GameState.ResponseMode.OPTION:
				if (DoOptionHighlight(state))
				{
					return;
				}
				break;
			case GameState.ResponseMode.SUB_OPTION:
				if (DoSubOptionHighlight(state))
				{
					return;
				}
				break;
			case GameState.ResponseMode.OPTION_TARGET:
				if (DoOptionTargetHighlight(state))
				{
					return;
				}
				break;
			}
		}
		else
		{
			m_actor.SetDeckActionHighlightState(DeckActionHighlightState.None);
		}
		if (m_mousedOver && !(m_zone is ZoneHand))
		{
			if (m_actor.UseBGQuestSiloutte())
			{
				m_actor.SetActorState(ActorStateType.CARD_MOUSE_OVER_BG_QUEST);
			}
			else
			{
				m_actor.SetActorState(ActorStateType.CARD_MOUSE_OVER);
			}
		}
		else if (m_mousedOverByOpponent)
		{
			m_actor.SetActorState(ActorStateType.CARD_OPPONENT_MOUSE_OVER);
		}
		else if (ShouldHighlightSelectedLettuceCharacter(state))
		{
			m_actor.SetActorState(ActorStateType.CARD_MOUSE_OVER);
		}
		else
		{
			m_actor.SetActorState(ActorStateType.CARD_IDLE);
		}
	}

	public void UpdateDeckActionHover(bool show = true)
	{
		if (show)
		{
			if (m_entity.IsTradeable())
			{
				ShowTradeableHover();
			}
			else if (m_entity.IsForgeable())
			{
				ShowForgeableHover();
			}
			else if (m_entity.IsPassable())
			{
				ShowPassableHover();
			}
			ManaCrystalMgr.Get().ProposeManaCrystalUsage(m_entity, fromDeckAction: true);
		}
		else
		{
			if (m_entity.IsTradeable())
			{
				HideTradeableHover();
			}
			else if (m_entity.IsForgeable())
			{
				HideForgeableHover();
			}
			else if (m_entity.IsPassable())
			{
				HidePassableHover();
			}
			ManaCrystalMgr.Get().CancelAllProposedMana(m_entity);
		}
	}

	private bool ShouldHighlightSelectedLettuceCharacter(GameState state)
	{
		if (state == null)
		{
			return false;
		}
		if (state.GetResponseMode() != GameState.ResponseMode.OPTION)
		{
			return false;
		}
		return ZoneMgr.Get().GetLettuceAbilitiesSourceEntity() == m_entity;
	}

	public void UpdateSelectedLettuceCharacterVisual()
	{
		if (m_actor == null)
		{
			return;
		}
		Spell spell = m_actor.GetSpell(SpellType.MERCENARIES_LIFT_UP);
		if (spell != null)
		{
			if (ZoneMgr.Get().GetLettuceAbilitiesSourceEntity() == m_entity)
			{
				spell.ActivateState(SpellStateType.BIRTH);
			}
			else
			{
				spell.ActivateState(SpellStateType.DEATH);
			}
		}
		UpdateActorState();
	}

	private bool DoChoiceHighlight(GameState state)
	{
		m_actor.SetDeckActionHighlightState(DeckActionHighlightState.None);
		if (state.GetChosenEntities().Contains(m_entity))
		{
			if (m_mousedOver)
			{
				m_actor.SetActorState(ActorStateType.CARD_PLAYABLE_MOUSE_OVER);
			}
			else
			{
				m_actor.SetActorState(ActorStateType.CARD_SELECTED);
			}
			return true;
		}
		int entityId = m_entity.GetEntityId();
		Network.EntityChoices choicePacket = state.GetFriendlyEntityChoices();
		if (choicePacket.Entities.Contains(entityId))
		{
			if (GameState.Get().IsMulliganManagerActive())
			{
				if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_HAS_HERO_LOBBY))
				{
					if (m_mousedOver)
					{
						m_actor.SetActorState(GameState.Get().GetGameEntity().GetMulliganChoiceHighlightState());
					}
					else
					{
						m_actor.SetActorState(ActorStateType.CARD_IDLE);
					}
				}
				else
				{
					m_actor.SetActorState(GameState.Get().GetGameEntity().GetMulliganChoiceHighlightState());
				}
			}
			else if (choicePacket.ChoiceType == CHOICE_TYPE.TARGET)
			{
				m_actor.SetActorState(ActorStateType.CARD_VALID_TARGET);
			}
			else if (m_actor.UseBGQuestSiloutte())
			{
				m_actor.SetActorState(ActorStateType.CARD_SELECTABLE_BG_QUEST);
			}
			else if (!m_entity.IsBattlegroundTrinket())
			{
				m_actor.SetActorState(ActorStateType.CARD_SELECTABLE);
			}
			return true;
		}
		return false;
	}

	private bool DoOptionHighlight(GameState state)
	{
		if (m_actor == null)
		{
			return false;
		}
		bool isValidDeckActionOption = GameState.Get().IsValidOption(m_entity, true);
		if (!isValidDeckActionOption)
		{
			m_actor.SetDeckActionHighlightState(DeckActionHighlightState.None);
		}
		else if (!m_overPlayfield)
		{
			m_actor.SetDeckActionHighlightState(DeckActionHighlightState.Green);
		}
		else
		{
			if (IsInDeckActionArea())
			{
				m_actor.SetDeckActionHighlightState(DeckActionHighlightState.Blue);
				m_actor.SetActorState(ActorStateType.CARD_OVER_PLAYFIELD);
				return true;
			}
			m_actor.SetDeckActionHighlightState(DeckActionHighlightState.None);
		}
		if (m_entity.HasTag(GAME_TAG.FORCE_GREEN_GLOW_ACTIVE) && m_entity.IsControlledByFriendlySidePlayer())
		{
			bool showMousedOverHighlight = m_mousedOver || ShouldHighlightSelectedLettuceCharacter(state);
			m_actor.SetActorState(showMousedOverHighlight ? ActorStateType.CARD_PLAYABLE_MOUSE_OVER : ActorStateType.CARD_PLAYABLE);
			return true;
		}
		if (GameState.Get().GetGameEntity().ShouldSuppressOptionHighlight(m_entity))
		{
			return false;
		}
		if (!GameState.Get().IsValidOption(m_entity, false))
		{
			return false;
		}
		if (IsOverMoveMinionTarget())
		{
			m_actor.SetActorState(ActorStateType.CARD_OVER_MOVE_MINION_TARGET);
			return true;
		}
		if (m_overPlayfield)
		{
			if (IsInDeckActionArea() && !isValidDeckActionOption)
			{
				m_actor.SetActorState(ActorStateType.CARD_IDLE);
			}
			else
			{
				m_actor.SetActorState(ActorStateType.CARD_OVER_PLAYFIELD);
			}
			return true;
		}
		bool inPlayOrTransitioningToPlay = m_entity.GetZone() == TAG_ZONE.PLAY || (m_latestZoneChange != null && m_latestZoneChange.GetDestinationZone() != null && m_latestZoneChange.GetDestinationZone().m_ServerTag == TAG_ZONE.PLAY);
		bool inHand = !inPlayOrTransitioningToPlay && m_entity.GetZone() == TAG_ZONE.HAND;
		bool comboActive = m_entity.GetController().IsRealTimeComboActive();
		if ((inHand || m_entity.IsCardButton()) && comboActive && m_entity.HasTag(GAME_TAG.COMBO))
		{
			m_actor.SetActorState(ActorStateType.CARD_COMBO);
			return true;
		}
		bool poweredUp = m_entity.GetRealTimePoweredUp();
		if ((inHand || m_entity.IsCardButton()) && poweredUp)
		{
			m_actor.SetActorState(ActorStateType.CARD_POWERED_UP);
			return true;
		}
		bool movableMinion = state.GetGameEntity().HasTag(GAME_TAG.ALLOW_MOVE_MINION) && m_entity.IsMinion();
		bool movableSpell = state.GetGameEntity().HasTag(GAME_TAG.ALLOW_MOVE_BACON_SPELL) && m_entity.IsBaconSpell();
		if (inPlayOrTransitioningToPlay && (movableMinion || movableSpell))
		{
			if (!GameState.Get().HasEnoughManaForMoveMinionHoverTarget(m_entity))
			{
				if (m_mousedOver)
				{
					m_actor.SetActorState(ActorStateType.CARD_MOUSE_OVER);
				}
				else
				{
					m_actor.SetActorState(ActorStateType.CARD_IDLE);
				}
				return true;
			}
			if (m_mousedOver)
			{
				m_actor.SetActorState(ActorStateType.CARD_MOVEABLE_MOUSE_OVER);
			}
			else
			{
				m_actor.SetActorState(ActorStateType.CARD_MOVEABLE);
			}
			return true;
		}
		if (!inHand)
		{
			int launchCost = GameUtils.StarshipLaunchCost(m_entity.GetController());
			bool canLaunchStarship = m_entity.GetController().GetNumAvailableResources() >= launchCost;
			if (m_mousedOver)
			{
				if (m_entity.GetRealTimeAttackableByRush() && !m_entity.IsLaunchpad())
				{
					m_actor.SetActorState(ActorStateType.CARD_ATTACKABLE_BY_RUSH_MOUSE_OVER);
				}
				else if (m_entity.GetRealTimeTitanAbilityUsable())
				{
					m_actor.SetActorState(ActorStateType.CARD_TITAN_ABILITY_MOUSE_OVER);
				}
				else if (m_entity.IsLaunchpad())
				{
					if (canLaunchStarship)
					{
						m_actor.SetActorState(ActorStateType.CARD_TITAN_ABILITY_MOUSE_OVER);
					}
				}
				else
				{
					m_actor.SetActorState(ActorStateType.CARD_PLAYABLE_MOUSE_OVER);
				}
				return true;
			}
			if (!m_mousedOver)
			{
				if (m_entity.GetRealTimeAttackableByRush() && !m_entity.IsLaunchpad())
				{
					m_actor.SetActorState(ActorStateType.CARD_ATTACKABLE_BY_RUSH);
					return true;
				}
				if (m_entity.GetRealTimeTitanAbilityUsable())
				{
					m_actor.SetActorState(ActorStateType.CARD_TITAN_ABILITY);
					return true;
				}
				if (m_entity.IsLaunchpad())
				{
					if (canLaunchStarship)
					{
						m_actor.SetActorState(ActorStateType.CARD_TITAN_ABILITY);
					}
					return true;
				}
			}
		}
		m_actor.SetActorState(ActorStateType.CARD_PLAYABLE);
		return true;
	}

	private bool DoSubOptionHighlight(GameState state)
	{
		m_actor.SetDeckActionHighlightState(DeckActionHighlightState.None);
		Network.Options.Option selectedNetworkOption = state.GetSelectedNetworkOption();
		int entityId = m_entity.GetEntityId();
		foreach (Network.Options.Option.SubOption subOption in selectedNetworkOption.Subs)
		{
			if (entityId == subOption.ID)
			{
				if (!subOption.PlayErrorInfo.IsValid())
				{
					return false;
				}
				if (m_mousedOver)
				{
					m_actor.SetActorState(ActorStateType.CARD_PLAYABLE_MOUSE_OVER);
				}
				else
				{
					m_actor.SetActorState(ActorStateType.CARD_PLAYABLE);
				}
				return true;
			}
		}
		return false;
	}

	private bool DoOptionTargetHighlight(GameState state)
	{
		m_actor.SetDeckActionHighlightState(DeckActionHighlightState.None);
		Network.Options.Option.SubOption selectedNetworkSubOption = state.GetSelectedNetworkSubOption();
		int entityId = m_entity.GetEntityId();
		if (selectedNetworkSubOption.IsValidTarget(entityId))
		{
			if (m_mousedOver)
			{
				m_actor.SetActorState(ActorStateType.CARD_VALID_TARGET_MOUSE_OVER);
			}
			else
			{
				m_actor.SetActorState(ActorStateType.CARD_VALID_TARGET);
			}
			return true;
		}
		return false;
	}

	public Actor GetActor()
	{
		return m_actor;
	}

	public void SetActor(Actor actor)
	{
		m_actor = actor;
	}

	public Actor GetQuestRewardActor()
	{
		return m_questRewardActor;
	}

	public string GetActorAssetPath()
	{
		return m_actorPath;
	}

	public void SetActorAssetPath(string actorName)
	{
		m_actorPath = actorName;
	}

	public bool IsActorReady()
	{
		return m_actorReady;
	}

	public bool IsActorLoading()
	{
		return m_actorLoading;
	}

	public void UpdateActorComponents()
	{
		if (!(m_actor == null))
		{
			m_actor.UpdateAllComponents();
		}
	}

	public void UpdateLettuceSpeechBubbleText(bool hideUnselectedBubbles)
	{
		Spell speechBubbleSpell = GetActorSpell(SpellType.MERCENARIES_SPEECH_BUBBLE);
		if (speechBubbleSpell == null)
		{
			return;
		}
		Entity preparedLettuceAbilityEntity = GetPreparedLettuceAbilityEntity();
		PlayMakerFSM fsm = speechBubbleSpell.GetComponent<PlayMakerFSM>();
		if (preparedLettuceAbilityEntity != null)
		{
			bool wasHidden = fsm.FsmVariables.GetFsmString("Text").Value == string.Empty;
			if (m_lettuceAbilityActionOrderIsTied)
			{
				fsm.FsmVariables.GetFsmString("Text").Value = GameStrings.Format("GAMEPLAY_LETTUCE_ABILITY_ORDER_TIED_TEXT", GameStrings.GetOrdinalNumber(m_lettuceAbilityActionOrder));
			}
			else
			{
				fsm.FsmVariables.GetFsmString("Text").Value = GameStrings.GetOrdinalNumber(m_lettuceAbilityActionOrder);
			}
			speechBubbleSpell.ActivateState(wasHidden ? SpellStateType.BIRTH : SpellStateType.ACTION);
		}
		else
		{
			fsm.FsmVariables.GetFsmString("Text").Value = string.Empty;
			speechBubbleSpell.ActivateState(hideUnselectedBubbles ? SpellStateType.DEATH : SpellStateType.ACTION);
		}
	}

	public void SetLettuceAbilityActionOrder(int order, bool isTied)
	{
		m_lettuceAbilityActionOrder = order;
		m_lettuceAbilityActionOrderIsTied = isTied;
	}

	public int GetLettuceAbilityActionOrder()
	{
		return m_lettuceAbilityActionOrder;
	}

	public int GetPreparedLettuceAbilitySpeedValue()
	{
		Entity abilityEntity = GetPreparedLettuceAbilityEntity();
		if (abilityEntity != null)
		{
			int speed = abilityEntity.GetCost();
			if (abilityEntity.HasTag(GAME_TAG.HIDE_COST) && abilityEntity.HasSubCards() && m_entity != null)
			{
				int subCardIndex = m_entity.GetTag(GAME_TAG.LETTUCE_SELECTED_SUBCARD_INDEX);
				List<int> subCardEntityIDs = abilityEntity.GetSubCardIDs();
				if (subCardEntityIDs.Count > subCardIndex)
				{
					Entity subCardEntity = GameState.Get().GetEntity(subCardEntityIDs[subCardIndex]);
					if (subCardEntity != null)
					{
						speed = subCardEntity.GetCost();
					}
				}
			}
			return speed;
		}
		return int.MaxValue;
	}

	public Entity GetPreparedLettuceAbilityEntity()
	{
		if (GetEntity() == null)
		{
			return null;
		}
		int abilityEntityID = GetEntity().GetSelectedLettuceAbilityID();
		return GameState.Get().GetEntity(abilityEntityID);
	}

	private Color GetLettuceSpeedTextColor(int defNumber, int currentNumber)
	{
		if (defNumber > currentNumber)
		{
			return Color.green;
		}
		if (defNumber < currentNumber)
		{
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				return new Color(1f, 10f / 51f, 10f / 51f);
			}
			return Color.red;
		}
		return Color.white;
	}

	public void RefreshActor()
	{
		UpdateActorState();
		if (m_entity.IsEnchanted())
		{
			UpdateEnchantments();
		}
		UpdateTooltip();
	}

	public Zone GetZone()
	{
		return m_zone;
	}

	public Zone GetPrevZone()
	{
		return m_prevZone;
	}

	public void SetZone(Zone zone)
	{
		m_zone = zone;
	}

	public int GetZonePosition()
	{
		return m_zonePosition;
	}

	public void SetZonePosition(int pos)
	{
		m_zonePosition = pos;
	}

	public int GetPredictedZonePosition()
	{
		return m_predictedZonePosition;
	}

	public void SetPredictedZonePosition(int pos)
	{
		m_predictedZonePosition = pos;
	}

	public ZoneTransitionStyle GetTransitionStyle()
	{
		return m_transitionStyle;
	}

	public void SetTransitionStyle(ZoneTransitionStyle style)
	{
		m_transitionStyle = style;
	}

	public bool IsTransitioningZones()
	{
		return m_transitioningZones;
	}

	public void EnableTransitioningZones(bool enable)
	{
		m_transitioningZones = enable;
	}

	public bool HasBeenGrabbedByEnemyActionHandler()
	{
		return m_hasBeenGrabbedByEnemyActionHandler;
	}

	public void MarkAsGrabbedByEnemyActionHandler(bool enable)
	{
		Log.FaceDownCard.Print("Card.MarkAsGrabbedByEnemyActionHandler() - card={0} enable={1}", this, enable);
		m_hasBeenGrabbedByEnemyActionHandler = enable;
	}

	public bool IsDoNotSort()
	{
		return m_doNotSort;
	}

	public void SetDoNotSort(bool on)
	{
		if (m_entity.IsControlledByOpposingSidePlayer())
		{
			Log.FaceDownCard.Print("Card.SetDoNotSort() - card={0} on={1}", this, on);
		}
		m_doNotSort = on;
	}

	public bool IsDoNotWarpToNewZone()
	{
		return m_doNotWarpToNewZone;
	}

	public void SetDoNotWarpToNewZone(bool on)
	{
		m_doNotWarpToNewZone = on;
	}

	public float GetTransitionDelay()
	{
		return m_transitionDelay;
	}

	public void SetTransitionDelay(float delay)
	{
		m_transitionDelay = delay;
	}

	public void UpdateZoneFromTags()
	{
		m_zonePosition = m_entity.GetZonePosition();
		Zone zone = ZoneMgr.Get().FindZoneForEntity(m_entity);
		TransitionToZone(zone);
		if (zone != null)
		{
			zone.UpdateLayout();
		}
	}

	public bool ShouldDeactivateHandSpellsWhenTransitioningToNullZone()
	{
		if (m_entity == null)
		{
			return true;
		}
		if (m_entity.HasTag(GAME_TAG.IS_USING_PASS_OPTION))
		{
			return false;
		}
		return true;
	}

	public void TransitionToZone(Zone zone, ZoneChange zoneChange = null)
	{
		m_latestZoneChange = zoneChange;
		if (m_zone == zone)
		{
			Log.Gameplay.Print("Card.TransitionToZone() - card={0} already in target zone", this);
			return;
		}
		if (zone == null)
		{
			if (m_zone.ContainsCard(this))
			{
				m_zone.RemoveCard(this);
			}
			m_prevZone = m_zone;
			m_zone = null;
			DeactivateLifetimeEffects();
			DeactivateCustomKeywordEffect();
			if (m_prevZone is ZoneHand && ShouldDeactivateHandSpellsWhenTransitioningToNullZone())
			{
				DeactivateHandStateSpells();
			}
			if (m_prevZone is ZoneHeroPower)
			{
				foreach (Card card in m_prevZone.GetCards())
				{
					if (!(card == this) && card.GetEntity().GetTag(GAME_TAG.LINKED_ENTITY) == m_entity.GetEntityId() && card.m_customSpawnSpellOverride != null)
					{
						if (m_actor != null)
						{
							m_actor.DeactivateAllSpells();
						}
						return;
					}
				}
			}
			if (m_prevZone is ZoneHero)
			{
				Player owner = m_prevZone.GetController();
				if (owner.GetHero() != null && owner.GetHero().GetCard() != null)
				{
					owner.GetHero().GetCard().ShowCard();
				}
			}
			DoNullZoneVisuals();
			return;
		}
		if (m_zone is ZoneSecret && m_entity != null && (m_entity.IsQuest() || m_entity.IsQuestline()))
		{
			NotifyMousedOut();
		}
		m_prevZone = m_zone;
		m_zone = zone;
		if (m_prevZone is ZoneDeck && m_zone is ZoneHand)
		{
			if (m_zone.m_Side == Player.Side.FRIENDLY)
			{
				m_cardDrawTracker = GameState.Get().GetFriendlyCardDrawCounter();
				GameState.Get().IncrementFriendlyCardDrawCounter();
			}
			else
			{
				m_cardDrawTracker = GameState.Get().GetOpponentCardDrawCounter();
				GameState.Get().IncrementOpponentCardDrawCounter();
			}
		}
		if (m_prevZone != null && m_prevZone.ContainsCard(this))
		{
			m_prevZone.RemoveCard(this);
		}
		m_zone.AddCard(this);
		if ((m_zone is ZonePlay || m_zone is ZoneHero) && m_prevZone is ZoneHand && m_entity.IsHero() && GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_USES_ALTERNATE_ACTORS) && MulliganManager.Get() != null && MulliganManager.Get().IsMulliganActive())
		{
			m_actorReady = true;
			return;
		}
		if (m_zone is ZoneGraveyard && m_actor != null && m_actor.UseCoinManaGem())
		{
			m_actor.ReleaseSpell(SpellType.COIN_MANA_GEM);
		}
		if (m_zone is ZoneGraveyard && GameState.Get().IsBeingDrawn(this))
		{
			m_actorReady = true;
			DiscardCardBeingDrawn();
		}
		else if (m_zone is ZoneGraveyard && m_ignoreDeath)
		{
			m_actorReady = true;
		}
		else if (m_zone is ZoneGraveyard && m_actor != null && m_actorReady && m_entity.IsSpell())
		{
			m_actorReady = false;
			StartCoroutine(LoadActorAndSpellsAfterPowerUpFinishes());
		}
		else if (m_zone is ZoneHeroPower && m_actor != null && m_actorReady && GameMgr.Get().IsBattlegrounds())
		{
			ShowCard();
			bool heroPowerShouldDisplayExhausted = m_entity.IsExhausted() || m_entity.IsDisabledHeroPower();
			ShowExhaustedChange(heroPowerShouldDisplayExhausted);
		}
		else if ((m_zone is ZoneBattlegroundHeroBuddy || m_zone is ZoneBattlegroundTrinket) && m_actor != null && m_actorReady)
		{
			ShowCard();
			ActivateStateSpells();
		}
		else if (m_zone != null && m_zone.m_ServerTag == TAG_ZONE.PLAY && m_actor != null && m_actor.GetEntity() != null && m_actor.GetEntity().IsBattlegroundQuestReward() && m_actorReady)
		{
			ShowCard();
		}
		else
		{
			m_actorReady = false;
			LoadActorAndSpells();
		}
	}

	public void UpdateActor(bool forceIfNullZone = false, string actorPath = null)
	{
		if (!forceIfNullZone && m_zone == null)
		{
			return;
		}
		TAG_ZONE zone = m_entity.GetZone();
		if (actorPath == null)
		{
			actorPath = m_cardDef.CardDef.DetermineActorPathForZone(m_entity, zone);
		}
		if (m_actor != null && m_actorPath == actorPath && !(m_actor is LettuceAbilityActor))
		{
			return;
		}
		GameObject actorGo = AssetLoader.Get().InstantiatePrefab(actorPath, AssetLoadingOptions.IgnorePrefabPosition);
		if (!actorGo)
		{
			Debug.LogWarningFormat("Card.UpdateActor() - FAILED to load actor \"{0}\"", actorPath);
			return;
		}
		Actor actor = actorGo.GetComponent<Actor>();
		if (actor == null)
		{
			Debug.LogWarningFormat("Card.UpdateActor() - ERROR actor \"{0}\" has no Actor component", actorPath);
			return;
		}
		if (m_actor != null)
		{
			StartCoroutine(DeactivateActorSpellsAndDestroy(m_actor));
		}
		m_actor = actor;
		m_actorPath = actorPath;
		m_actor.SetEntity(m_entity);
		m_actor.SetCard(this);
		m_actor.SetCardDef(m_cardDef);
		m_actor.UpdateAllComponents();
		if (m_shown)
		{
			ShowImpl();
		}
		else
		{
			HideImpl();
		}
		RefreshActor();
		m_actorReady = true;
	}

	private IEnumerator DeactivateActorSpellsAndDestroy(Actor actor)
	{
		if (actor == null)
		{
			yield break;
		}
		actor.Hide();
		bool isDivineShieldFinished = true;
		if (actor.IsDivineShieldActive())
		{
			isDivineShieldFinished = false;
			actor.DeactivateDivineShield(delegate
			{
				isDivineShieldFinished = true;
			});
		}
		bool isTauntFinished = true;
		if (m_actor.IsTauntActive())
		{
			isTauntFinished = false;
			actor.DeactivateTaunt(delegate
			{
				isTauntFinished = true;
			});
		}
		while (!isDivineShieldFinished || !isTauntFinished)
		{
			yield return null;
		}
		if (actor != null)
		{
			actor.Destroy();
		}
	}

	private IEnumerator LoadActorAndSpellsAfterPowerUpFinishes()
	{
		m_actorLoading = true;
		Spell spell = m_actor.GetSpell(SpellType.POWER_UP);
		if (spell != null)
		{
			while (spell.GetActiveState() != 0 && spell.GetActiveState() != SpellStateType.IDLE)
			{
				yield return null;
			}
		}
		LoadActorAndSpells();
	}

	private void LoadActorAndSpells()
	{
		m_actorLoading = true;
		List<PrefabLoadRequest> loadRequests = new List<PrefabLoadRequest>();
		if (m_prevZone is ZoneHand && (m_zone is ZonePlay || m_zone is ZoneHero || m_zone is ZoneWeapon))
		{
			PrefabLoadRequest request = MakeCustomSpellLoadRequest(m_cardDef.CardDef.m_CustomSummonSpellPath, m_cardDef.CardDef.m_GoldenCustomSummonSpellPath, OnCustomSummonSpellLoaded);
			if (request != null)
			{
				loadRequests.Add(request);
			}
		}
		if (m_customDeathSpell == null && (m_zone is ZoneHand || m_zone is ZonePlay))
		{
			PrefabLoadRequest request2 = MakeCustomSpellLoadRequest(m_cardDef.CardDef.m_CustomDeathSpellPath, m_cardDef.CardDef.m_GoldenCustomDeathSpellPath, OnCustomDeathSpellLoaded);
			if (request2 != null)
			{
				loadRequests.Add(request2);
			}
		}
		if (m_customDiscardSpell == null && (m_zone is ZoneHand || m_zone is ZoneGraveyard))
		{
			PrefabLoadRequest request3 = MakeCustomSpellLoadRequest(m_cardDef.CardDef.m_CustomDiscardSpellPath, m_cardDef.CardDef.m_GoldenCustomDiscardSpellPath, OnCustomDiscardSpellLoaded);
			if (request3 != null)
			{
				loadRequests.Add(request3);
			}
		}
		if (m_customSpawnSpell == null && (m_zone is ZonePlay || m_zone is ZoneWeapon || m_zone is ZoneSecret || m_zone is ZoneBattlegroundHeroBuddy || m_zone is ZoneBattlegroundQuestReward || m_zone is ZoneBattlegroundTrinket || m_zone is ZoneBattlegroundClickableButton))
		{
			PrefabLoadRequest request4 = MakeCustomSpellLoadRequest(m_cardDef.CardDef.m_CustomSpawnSpellPath, m_cardDef.CardDef.m_GoldenCustomSpawnSpellPath, OnCustomSpawnSpellLoaded);
			if (request4 != null)
			{
				loadRequests.Add(request4);
			}
		}
		m_spellLoadCount = loadRequests.Count;
		if (loadRequests.Count == 0)
		{
			LoadActor();
			return;
		}
		foreach (PrefabLoadRequest request5 in loadRequests)
		{
			if (!AssetLoader.Get().InstantiatePrefab(request5.m_path, request5.m_loadCallback))
			{
				request5.m_loadCallback(request5.m_path, null, null);
			}
		}
	}

	private PrefabLoadRequest MakeCustomSpellLoadRequest(string customPath, string goldenCustomPath, PrefabCallback<GameObject> loadCallback)
	{
		string path = customPath;
		if (m_entity.GetPremiumType() == TAG_PREMIUM.GOLDEN && !string.IsNullOrEmpty(goldenCustomPath))
		{
			path = goldenCustomPath;
		}
		else if (string.IsNullOrEmpty(path))
		{
			return null;
		}
		if (GameUtils.IsOnVFXDenylist(path))
		{
			Error.AddDevWarning("Card", "Spell path '{0}' was on the VFX Denylist", path);
			return null;
		}
		return new PrefabLoadRequest
		{
			m_path = path,
			m_loadCallback = loadCallback
		};
	}

	private void OnCustomSummonSpellLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Error.AddDevFatal("Card.OnCustomSummonSpellLoaded() - FAILED to load \"{0}\" for card {1}", assetRef, this);
			FinishSpellLoad();
			return;
		}
		m_customSummonSpell = go.GetComponent<Spell>();
		if (m_customSummonSpell == null)
		{
			FinishSpellLoad();
			return;
		}
		SpellUtils.SetupSpell(m_customSummonSpell, this);
		FinishSpellLoad();
	}

	private void OnCustomDeathSpellLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Error.AddDevFatal("Card.OnCustomDeathSpellLoaded() - FAILED to load \"{0}\" for card {1}", assetRef, this);
			FinishSpellLoad();
			return;
		}
		m_customDeathSpell = go.GetComponent<Spell>();
		if (m_customDeathSpell == null)
		{
			FinishSpellLoad();
			return;
		}
		SpellUtils.SetupSpell(m_customDeathSpell, this);
		FinishSpellLoad();
	}

	private void OnCustomDiscardSpellLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Error.AddDevFatal("Card.OnCustomDiscardSpellLoaded() - FAILED to load \"{0}\" for card {1}", assetRef, this);
			FinishSpellLoad();
			return;
		}
		m_customDiscardSpell = go.GetComponent<Spell>();
		if (m_customDiscardSpell == null)
		{
			FinishSpellLoad();
			return;
		}
		SpellUtils.SetupSpell(m_customDiscardSpell, this);
		FinishSpellLoad();
	}

	private void OnCustomSpawnSpellLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Error.AddDevFatal("Card.OnCustomSpawnSpellLoaded() - FAILED to load \"{0}\" for card {1}", assetRef, this);
			FinishSpellLoad();
			return;
		}
		m_customSpawnSpell = go.GetComponent<Spell>();
		if (m_customSpawnSpell == null)
		{
			FinishSpellLoad();
			return;
		}
		SpellUtils.SetupSpell(m_customSpawnSpell, this);
		FinishSpellLoad();
	}

	private void FinishSpellLoad()
	{
		m_spellLoadCount--;
		if (m_spellLoadCount <= 0)
		{
			LoadActor();
		}
	}

	private void LoadActor()
	{
		RefreshCardsInTooltip();
		string actorPath = m_cardDef.CardDef.DetermineActorPathForZone(m_entity, m_zone.m_ServerTag);
		if ((m_actorPath == actorPath && m_actor != null) || actorPath == null)
		{
			m_actorPath = actorPath;
			FinishActorLoad(m_actor);
		}
		else
		{
			AssetLoader.Get().InstantiatePrefab(actorPath, OnActorLoaded, null, AssetLoadingOptions.IgnorePrefabPosition);
		}
	}

	public void SetAlwaysShowCardsInTooltip(bool show)
	{
		m_alwaysShowCardsInTooltip = show;
	}

	public void SetCardInTooltipDisplaySide(bool forceLeft = false, bool forceRight = false)
	{
		if (m_cardsInTooltip == null)
		{
			CreateCardsInTooltip();
		}
		if (m_cardsInTooltip != null)
		{
			m_cardsInTooltip.SetTooltipOnLeft(forceLeft);
			m_cardsInTooltip.SetTooltipOnRight(forceRight);
		}
	}

	public bool ShouldShowCardsInTooltip()
	{
		if (m_cardsInTooltip == null)
		{
			return false;
		}
		if (!m_alwaysShowCardsInTooltip)
		{
			if (m_zone is ZoneHand)
			{
				return m_entity.IsControlledByFriendlySidePlayer();
			}
			return false;
		}
		return true;
	}

	public void CreateCardsInTooltip()
	{
		if (m_cardsInTooltip == null)
		{
			m_cardsInTooltip = base.gameObject.AddComponent<DisplayCardsInToolip>();
			m_cardsInTooltip.Setup(this);
		}
		if (m_entity.HasTag(GAME_TAG.HERO_POWER))
		{
			m_cardsInTooltip.AddCardsInTooltip(m_entity.GetTag(GAME_TAG.HERO_POWER));
		}
		if (m_entity.HasTag(GAME_TAG.DISPLAY_CARD_ON_MOUSEOVER))
		{
			m_cardsInTooltip.AddCardsInTooltip(m_entity.GetTag(GAME_TAG.DISPLAY_CARD_ON_MOUSEOVER));
		}
		if (m_entity.IsTitan())
		{
			Entity parent = GetEntity();
			if (parent != null)
			{
				List<int> subCardIDs = parent.GetSubCardIDs();
				for (int i = 0; i < subCardIDs.Count; i++)
				{
					Entity subCardEntity = GameState.Get().GetEntity(subCardIDs[i]);
					if (!subCardEntity.HasTag(GAME_TAG.LITERALLY_UNPLAYABLE))
					{
						int databaseID = GameUtils.TranslateCardIdToDbId(subCardEntity.GetCardId());
						m_cardsInTooltip.AddCardsInTooltip(databaseID);
					}
				}
			}
		}
		if (GameState.Get().BattlegroundAllowBuddies())
		{
			int heroBuddyCardId = m_entity.GetHeroBuddyCardId();
			if (heroBuddyCardId != 0)
			{
				m_cardsInTooltip.AddCardsInTooltip(heroBuddyCardId);
			}
		}
		m_cardsInTooltip.NotifyMousedOut();
	}

	private void DestroyCardsInTooltip()
	{
		if (m_cardsInTooltip != null)
		{
			UnityEngine.Object.Destroy(m_cardsInTooltip);
			m_cardsInTooltip = null;
		}
	}

	public void RefreshTitanCardsInTooltip()
	{
		if (!m_entity.IsTitan())
		{
			return;
		}
		if (m_zone is ZoneHand)
		{
			if (m_cardsInTooltip == null)
			{
				CreateCardsInTooltip();
				m_cardsInTooltip.NotifyMousedOver();
			}
			else if (m_cardsInTooltip.GetActorCardCount() != m_entity.GetUsableTitanAbilityCount())
			{
				DestroyCardsInTooltip();
				CreateCardsInTooltip();
				m_cardsInTooltip.NotifyMousedOver();
			}
		}
		else
		{
			DestroyCardsInTooltip();
		}
	}

	public void RefreshCardsInTooltip()
	{
		DestroyCardsInTooltip();
		if (m_zone is ZoneHand && (m_entity.IsHero() || m_entity.HasTag(GAME_TAG.DISPLAY_CARD_ON_MOUSEOVER)))
		{
			CreateCardsInTooltip();
		}
	}

	private void HideCardsInTooltip()
	{
		if (m_cardsInTooltip != null)
		{
			m_cardsInTooltip.NotifyMousedOut();
		}
	}

	private void OnActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Debug.LogWarning($"Card.OnActorLoaded() - FAILED to load actor \"{assetRef}\"");
			return;
		}
		Actor actor = go.GetComponent<Actor>();
		if (actor == null)
		{
			Debug.LogWarning($"Card.OnActorLoaded() - ERROR actor \"{assetRef}\" has no Actor component");
			return;
		}
		Actor oldActor = m_actor;
		m_actor = actor;
		m_actorPath = assetRef.ToString();
		m_actor.SetEntity(m_entity);
		m_actor.SetCard(this);
		m_actor.SetCardDef(m_cardDef);
		if (GameMgr.Get().IsBattlegrounds() && m_entity.IsBobQuest())
		{
			UseBobQuestComponent();
		}
		m_actor.UpdateAllComponents();
		FinishActorLoad(oldActor);
	}

	private void FinishActorLoad(Actor oldActor)
	{
		m_actorLoading = false;
		OnZoneChanged();
		OnActorChanged(oldActor);
		if (m_isBattleCrySource)
		{
			LayerUtils.SetLayer(m_actor.gameObject, GameLayer.IgnoreFullScreenEffects);
		}
		RefreshActor();
	}

	public void ForceLoadHandActor(bool forceRevealed = false)
	{
		string prefabPath = m_cardDef.CardDef.DetermineActorPathForZone(m_entity, TAG_ZONE.HAND, forceRevealed);
		if (m_actor != null && m_actorPath == prefabPath)
		{
			ShowCard();
			m_actor.Show();
			RefreshActor();
			return;
		}
		GameObject actorObject = AssetLoader.Get().InstantiatePrefab(prefabPath, AssetLoadingOptions.IgnorePrefabPosition);
		if (actorObject == null)
		{
			Debug.LogWarningFormat("Card.ForceLoadHandActor() - FAILED to load actor \"{0}\"", prefabPath);
			return;
		}
		Actor actor = actorObject.GetComponent<Actor>();
		if (actor == null)
		{
			Debug.LogWarningFormat("Card.ForceLoadHandActor() - ERROR actor \"{0}\" has no Actor component", prefabPath);
			return;
		}
		if (m_actor != null)
		{
			m_actor.Destroy();
		}
		m_actor = actor;
		m_actorPath = prefabPath;
		m_actor.SetEntity(m_entity);
		m_actor.SetCard(this);
		m_actor.SetCardDef(m_cardDef);
		if ((GameMgr.Get().IsBattlegrounds() && m_entity.IsQuest()) || m_entity.HasTag(GAME_TAG.IS_NIGHTMARE_BONUS))
		{
			UseBattlegroundQuestComponent();
			UpdateRewardActor();
		}
		m_actor.UpdateAllComponents();
		if (m_shown)
		{
			ShowImpl();
		}
		else
		{
			HideImpl();
		}
		RefreshActor();
	}

	public void UseBattlegroundQuestComponent()
	{
		if (!(m_actor == null))
		{
			GameObjectUtils.FindChildBySubstring(base.gameObject, "Description_mesh")?.SetActive(value: false);
			GameObjectUtils.FindChildBySubstring(base.gameObject, "Card_Hand_BG_Quest_Text_Tray_Mesh")?.SetActive(value: true);
			if (m_actor.m_isDebuggingBattlegroundQuestReward)
			{
				GameObjectUtils.FindChildBySubstring(base.gameObject, "NonQuestObjects")?.SetActive(value: false);
			}
			m_actor.SetUseBGQuestSiloutte(value: true);
		}
	}

	private void UseBobQuestComponent()
	{
		if (!(m_actor == null))
		{
			m_actor.SetUseBGQuestSiloutte(value: true);
		}
	}

	public void UpdateRewardActor()
	{
		if (!(m_questRewardActor == null) && !m_questRewardChanged)
		{
			return;
		}
		if (m_questRewardActor != null)
		{
			m_questRewardActor.Destroy();
		}
		Entity rewardEnt = GameState.Get()?.GetEntity(GetEntity().GetTag(GAME_TAG.TAG_SCRIPT_DATA_ENT_1));
		string questRewardCardID = null;
		if (rewardEnt == null)
		{
			questRewardCardID = QuestController.GetRewardCardIDFromQuestCardID(GetEntity());
			if (string.IsNullOrEmpty(questRewardCardID))
			{
				Debug.LogWarning("[UpdateRewardActor] - rewardEnt is null and no backup QUEST_REWARD_DBID");
				return;
			}
		}
		if (rewardEnt == null)
		{
			UpdateRewardActorUsingDefinition(questRewardCardID);
		}
		else
		{
			UpdateRewardActorUsingEntity(rewardEnt);
		}
	}

	private void UpdateRewardActorUsingDefinition(string defName)
	{
		using DefLoader.DisposableFullDef questRewardDef = DefLoader.Get().GetFullDef(defName);
		if (questRewardDef?.CardDef == null || questRewardDef?.EntityDef == null)
		{
			Log.Spells.PrintError("Card.UpdateRewardActorUsingDefinition(): Unable to load def for card ID {0}.", defName);
			return;
		}
		GameObject fakeQuestRewardActorGO = AssetLoader.Get().InstantiatePrefab(ActorNames.GetHandActor(questRewardDef.EntityDef, TAG_PREMIUM.NORMAL), AssetLoadingOptions.IgnorePrefabPosition);
		if (fakeQuestRewardActorGO == null)
		{
			Log.Spells.PrintError("Card.UpdateRewardActorUsingDefinition(): Unable to load Hand Actor for entity def {0}.", questRewardDef.EntityDef);
			return;
		}
		UpdateRewardActor_Common(fakeQuestRewardActorGO);
		Entity dummyEntity = new Entity();
		dummyEntity.InitCard();
		dummyEntity.ReplaceTags(questRewardDef.EntityDef.GetTags());
		dummyEntity.LoadCard(defName);
		m_questRewardActor.SetEntity(dummyEntity);
		m_questRewardActor.SetCardDef(questRewardDef.DisposableCardDef);
		m_questRewardActor.UpdateAllComponents();
	}

	private void UpdateRewardActorUsingEntity(Entity rewardEnt)
	{
		GameObject rewardCardActorGO = AssetLoader.Get().InstantiatePrefab(ActorNames.GetHandActor(rewardEnt), AssetLoadingOptions.IgnorePrefabPosition);
		if (rewardCardActorGO == null)
		{
			Log.Gameplay.PrintError("[UpdateRewardActor] - Unable to load hand actor for entity {0}.", rewardEnt);
			return;
		}
		UpdateRewardActor_Common(rewardCardActorGO);
		m_questRewardActor.SetEntity(rewardEnt);
		m_questRewardActor.SetCardDefFromEntity(rewardEnt);
		m_questRewardActor.SetPremium(rewardEnt.GetPremiumType());
		m_questRewardActor.SetWatermarkCardSetOverride(rewardEnt.GetWatermarkCardSetOverride());
		m_questRewardActor.UpdateAllComponents();
	}

	private void UpdateRewardActor_Common(GameObject rewardCardActorGO)
	{
		LayerUtils.SetLayer(rewardCardActorGO, m_actor.gameObject.layer, null);
		rewardCardActorGO.transform.parent = m_actor.gameObject.transform;
		TransformUtil.Identity(rewardCardActorGO);
		GameObjectUtils.FindChildBySubstring(rewardCardActorGO, "BGRewardVFX")?.SetActive(value: true);
		m_questRewardActor = rewardCardActorGO.GetComponentInChildren<Actor>();
		LayerUtils.SetLayer(m_questRewardActor, GameLayer.CardRaycast);
		m_questRewardActor.SetCard(this);
		if (m_questRewardActor.UseCoinManaGem())
		{
			m_questRewardActor.m_manaObject.SetActive(value: false);
			m_questRewardActor.m_costTextMesh.gameObject.SetActive(value: false);
			m_questRewardActor.ActivateSpellBirthState(SpellType.COIN_MANA_GEM);
		}
		m_questRewardActor.UpdateAllComponents();
		m_questRewardActor.gameObject.SetActive(value: false);
		if (m_actor != null && m_actor.m_isDebuggingBattlegroundQuestReward)
		{
			Vector3 questCardOffset = new Vector3(0f, 0f, 0.2f);
			iTween.MoveTo(base.gameObject, base.gameObject.transform.position + questCardOffset, 0.2f);
			Vector3 BgRewardOffset = new Vector3(0f, -0.1f, 1.05f);
			Vector3 BgRewardLocalScale = new Vector3(0.9f, 0.9f, 0.9f);
			rewardCardActorGO.transform.localPosition = BgRewardOffset;
			rewardCardActorGO.transform.localScale = BgRewardLocalScale;
			m_questRewardActor.gameObject.SetActive(value: true);
		}
	}

	public void ApplyTeammateViewUpdatesForQuestDiscover(TeammateEntityData entityData)
	{
		m_questRewardActor.gameObject.transform.localPosition = new Vector3(0f, -0.15f, 0.72f);
		m_questRewardActor.gameObject.SetActive(value: true);
		m_actor.ActivateSpellDeathState(SpellType.COIN_MANA_GEM);
		GameObjectUtils.FindChildBySubstring(m_actor.gameObject, "NonQuestObjects")?.SetActive(value: false);
		m_actor.gameObject.transform.localPosition = new Vector3(0f, 0f, -0.28f);
		foreach (Tag tagAndValue in entityData.Tags)
		{
			if (tagAndValue.Name == 2673)
			{
				m_questRewardActor.GetEntity().SetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1, tagAndValue.Value);
			}
			else if (tagAndValue.Name == 2571)
			{
				m_questRewardActor.GetEntity().SetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1, tagAndValue.Value);
			}
		}
		m_questRewardActor.UpdateAllComponents();
	}

	private void OnZoneChanged()
	{
		if (m_prevZone is ZoneHand && m_zone is ZoneGraveyard)
		{
			if (m_mousedOver)
			{
				NotifyMousedOut();
			}
			DoDiscardAnimation();
			HideCardsInTooltip();
		}
		else if (m_prevZone is ZoneHand)
		{
			if (m_mousedOver)
			{
				NotifyMousedOut();
			}
		}
		else if (m_zone is ZoneGraveyard)
		{
			if (m_entity.IsHero())
			{
				m_doNotSort = true;
			}
		}
		else if (m_zone is ZoneHand)
		{
			if (!m_doNotSort)
			{
				ShowCard();
			}
			if (m_prevZone is ZoneGraveyard && m_entity.IsSpell())
			{
				m_actor.Hide();
				if (!GameMgr.Get().IsBattlegrounds() || m_entity == null || m_entity.GetRealTimeZone() == TAG_ZONE.HAND)
				{
					ActivateActorSpell(SpellType.SUMMON_IN, OnSpellFinished_DefaultHandSpawn);
				}
			}
		}
		else if ((m_prevZone is ZoneGraveyard || m_prevZone is ZoneDeck) && m_zone.m_ServerTag == TAG_ZONE.PLAY)
		{
			ShowCard();
		}
		if (!(m_zone is ZonePlay) && m_magneticPlayData != null)
		{
			SpellUtils.ActivateDeathIfNecessary(GetActorSpell(SpellType.MAGNETIC_PLAY_LINKED_RIGHT));
			SpellUtils.ActivateDeathIfNecessary(m_magneticPlayData.m_targetMech.GetActorSpell(SpellType.MAGNETIC_PLAY_LINKED_LEFT));
			SpellUtils.ActivateDeathIfNecessary(m_magneticPlayData.m_beamSpell);
			if (m_magneticPlayData.m_targetMech != null)
			{
				m_magneticPlayData.m_targetMech.SetIsMagneticTarget(isTarget: false);
			}
			m_magneticPlayData = null;
		}
	}

	private bool Check_NullZoneToGraveyard(Actor oldActor, ref bool transitionHandled)
	{
		if (m_prevZone == null && m_zone is ZoneGraveyard)
		{
			if (oldActor != null && oldActor != m_actor)
			{
				oldActor.Destroy();
			}
			if (IsShown())
			{
				HideCard();
			}
			else
			{
				HideImpl();
			}
			DeactivateHandStateSpells();
			transitionHandled = true;
			m_actorReady = true;
			return true;
		}
		return false;
	}

	private bool Check_OldActorIsNull(Actor oldActor, ref bool transitionHandled)
	{
		if (oldActor == null)
		{
			bool creatingGame = GameState.Get().IsGameCreating();
			if (m_zone is ZoneHand && GameState.Get().IsBeginPhase())
			{
				Process_OldActorIsNull_ZoneIsHand_IsBeginPhase(oldActor, ref transitionHandled);
			}
			else if (creatingGame)
			{
				Process_OldActorIsNull_CreatingGame(oldActor, ref transitionHandled);
			}
			else
			{
				Process_OldActorIsNull_WarpStuff(oldActor, ref transitionHandled);
				bool willMulligan = GameState.Get().IsMulliganPhaseNowOrPending();
				if (m_prevZone == null && m_zone is ZonePlay zonePlay)
				{
					Process_OldActorIsNull_NullZoneToPlay(oldActor, ref transitionHandled, zonePlay);
				}
				else if (!willMulligan && (m_zone is ZoneHeroPower || m_zone is ZoneWeapon || m_zone is ZoneBattlegroundHeroBuddy || m_zone is ZoneBattlegroundQuestReward || m_zone is ZoneBattlegroundTrinket || m_zone is ZoneBattlegroundClickableButton))
				{
					Process_OldActorIsNull_WillNotMulligan(oldActor, ref transitionHandled);
				}
				else if (m_prevZone == null && m_zone is ZoneHero)
				{
					Process_OldActorIsNull_NullZoneToHero(oldActor, ref transitionHandled);
				}
			}
			return true;
		}
		return false;
	}

	private void Process_OldActorIsNull_ZoneIsHand_IsBeginPhase(Actor oldActor, ref bool transitionHandled)
	{
		bool num = GameState.Get().IsMulliganPhaseNowOrPending();
		bool isFriendlyCoin = CosmeticCoinManager.Get().IsOwnedCoinCard(m_entity.GetCardId());
		if (num && !GameState.Get().HasTheCoinBeenSpawned())
		{
			if (isFriendlyCoin)
			{
				GameState.Get().NotifyOfCoinSpawn();
				m_actor.TurnOffCollider();
				m_actor.Hide();
				m_actorReady = true;
				transitionHandled = true;
				base.transform.position = Vector3.zero;
				m_doNotWarpToNewZone = true;
				m_doNotSort = true;
			}
			else
			{
				Player controller = m_entity.GetController();
				if (controller.IsOpposingSide() && this == m_zone.GetLastCard() && !controller.HasTag(GAME_TAG.FIRST_PLAYER))
				{
					GameState.Get().NotifyOfCoinSpawn();
					m_actor.TurnOffCollider();
					m_actorReady = true;
					transitionHandled = true;
				}
			}
		}
		if (!isFriendlyCoin)
		{
			ZoneMgr.Get().FindZoneOfType<ZoneDeck>(m_zone.m_Side).SetCardToInDeckState(this);
		}
	}

	private void Process_OldActorIsNull_CreatingGame(Actor oldActor, ref bool transitionHandled)
	{
		TransformUtil.CopyWorld(base.transform, m_zone.transform);
		if (m_zone is ZonePlay || m_zone is ZoneHero || m_zone is ZoneHeroPower || m_zone is ZoneWeapon || m_zone is ZoneBattlegroundHeroBuddy || m_zone is ZoneBattlegroundQuestReward || m_zone is ZoneBattlegroundClickableButton)
		{
			ActivateLifetimeEffects();
		}
	}

	private void Process_OldActorIsNull_WarpStuff(Actor oldActor, ref bool transitionHandled)
	{
		if (!m_doNotWarpToNewZone)
		{
			TransformUtil.CopyWorld(base.transform, m_zone.transform);
		}
		if (!(m_zone is ZoneHand))
		{
			return;
		}
		if (!m_doNotWarpToNewZone)
		{
			ZoneHand zoneHand = (ZoneHand)m_zone;
			base.transform.localScale = zoneHand.GetCardScale();
			base.transform.localEulerAngles = zoneHand.GetCardRotation(this);
			base.transform.position = zoneHand.GetCardPosition(this);
		}
		Entity creatorEntity = GameState.Get().GetEntity(m_entity.GetTag(GAME_TAG.CREATOR));
		bool creatorSpawnsReplacementsWhenPlayed = creatorEntity != null && creatorEntity.HasReplacementsWhenPlayed() && m_entity.HasTag(GAME_TAG.CREATED_AS_ON_PLAY_REPLACEMENT);
		if (creatorEntity != null && creatorEntity.IsMinion())
		{
			if (creatorEntity.HasTag(GAME_TAG.MINIATURIZE) && !m_entity.HasTag(GAME_TAG.CREATED_BY_MINIATURIZE))
			{
				creatorSpawnsReplacementsWhenPlayed = false;
			}
			if (creatorEntity.HasTag(GAME_TAG.GIGANTIFY) && !m_entity.HasTag(GAME_TAG.CREATED_BY_GIGANTIFY))
			{
				creatorSpawnsReplacementsWhenPlayed = false;
			}
		}
		if (creatorSpawnsReplacementsWhenPlayed)
		{
			m_transitionStyle = ZoneTransitionStyle.INSTANT;
			ActivateHandSpawnSpell();
			InputManager.Get().GetFriendlyHand().ActivateCardWithReplacementsDeath();
			InputManager.Get().GetFriendlyHand().ClearReservedCard();
			return;
		}
		m_actorReady = true;
		m_shown = true;
		if (!m_doNotWarpToNewZone)
		{
			m_actor.Hide();
			ActivateHandSpawnSpell();
			transitionHandled = true;
		}
	}

	private void Process_OldActorIsNull_NullZoneToPlay(Actor oldActor, ref bool transitionHandled, ZonePlay zonePlay)
	{
		if (!m_doNotWarpToNewZone)
		{
			base.transform.position = zonePlay.GetCardPosition(this);
		}
		if (m_cardDef.CardDef.m_SuppressPlaySoundsDuringMulligan && GameState.Get().IsMulliganPhaseNowOrPending())
		{
			SuppressPlaySounds(suppress: true);
		}
		if (m_entity.HasTag(GAME_TAG.LINKED_ENTITY))
		{
			if (m_customSpawnSpellOverride != null)
			{
				ActivateMinionSpawnEffects();
			}
			else
			{
				m_transitionStyle = ZoneTransitionStyle.INSTANT;
				Transform offscreen = Board.Get().FindBone("SpawnOffscreen");
				base.transform.position = offscreen.position;
				ActivateCharacterPlayEffects();
				OnSpellFinished_StandardSpawnCharacter(null, null);
			}
		}
		else
		{
			m_actor.Hide();
			ActivateMinionSpawnEffects();
		}
		transitionHandled = true;
	}

	private void Process_OldActorIsNull_WillNotMulligan(Actor oldActor, ref bool transitionHandled)
	{
		if (IsShown())
		{
			ActivatePlaySpawnEffects();
			transitionHandled = true;
			m_actorReady = true;
		}
	}

	private void Process_OldActorIsNull_NullZoneToHero(Actor oldActor, ref bool transitionHandled)
	{
		Entity currentHero = m_entity;
		GameState.Get().GetGameEntity().UpdateNameDisplay();
		if (currentHero.HasTag(GAME_TAG.TREAT_AS_PLAYED_HERO_CARD) && !currentHero.HasTag(GAME_TAG.SUPPRESS_HERO_STANDARD_SUMMON_FX))
		{
			Card oldHero = HeroCustomSummonSpell.GetOldHeroCard(currentHero.GetCard());
			if (oldHero != null)
			{
				currentHero.GetCard().GetActor().Hide();
				HeroCustomSummonSpell.HideStats(oldHero);
				oldHero.SetDelayBeforeHideInNullZoneVisuals(0.8f);
			}
			ActivateStandardSpawnHeroSpell();
			transitionHandled = true;
		}
	}

	private bool Check_NullZoneToVarious(Actor oldActor, ref bool transitionHandled)
	{
		if (m_prevZone == null && (m_zone is ZoneHeroPower || m_zone is ZoneWeapon || m_zone is ZoneBattlegroundHeroBuddy || m_zone is ZoneBattlegroundQuestReward || m_zone is ZoneBattlegroundTrinket || m_zone is ZoneBattlegroundClickableButton))
		{
			oldActor.Destroy();
			TransformUtil.CopyWorld(base.transform, m_zone.transform);
			m_transitionStyle = ZoneTransitionStyle.INSTANT;
			ActivatePlaySpawnEffects();
			transitionHandled = true;
			m_actorReady = true;
			return true;
		}
		if (m_prevZone == null && m_zone is ZoneHand && oldActor == m_actor && !m_goingThroughDeathrattleReturnfromGraveyard)
		{
			ActivateHandStateSpells();
			transitionHandled = true;
			m_actorReady = true;
			return true;
		}
		if (m_prevZone == null && m_zone is ZonePlay && oldActor == m_actor)
		{
			ActivateMinionSpawnEffects();
			ShowCard();
			transitionHandled = true;
			m_actorReady = true;
			return true;
		}
		return false;
	}

	private bool Check_HandToPlayOrHero(Actor oldActor, ref bool transitionHandled)
	{
		if (m_prevZone is ZoneHand && (m_zone is ZonePlay || m_zone is ZoneHero))
		{
			ActivateActorSpells_HandToPlay(oldActor);
			if (m_cardDef.CardDef.m_SuppressPlaySoundsOnSummon || m_entity.HasTag(GAME_TAG.CARD_DOES_NOTHING))
			{
				SuppressPlaySounds(suppress: true);
			}
			ActivateCharacterPlayEffects();
			m_actor.Hide();
			transitionHandled = true;
			if (CardTypeBanner.Get() != null && CardTypeBanner.Get().HasCardDef && CardTypeBanner.Get().HasSameCardDef(m_cardDef.CardDef))
			{
				CardTypeBanner.Get().Hide();
			}
			if (m_entity.IsMinion())
			{
				m_prevZone.GetController().GetHeroCard().ActivateLegendaryHeroAnimEvent(LegendaryHeroAnimations.SummonMinion);
			}
			return true;
		}
		return false;
	}

	private bool Check_HandToWeapon(Actor oldActor, ref bool transitionHandled)
	{
		if (m_prevZone is ZoneHand && m_zone is ZoneWeapon)
		{
			if (ActivateActorSpells_HandToWeapon(oldActor))
			{
				m_actor.Hide();
				transitionHandled = true;
				if (CardTypeBanner.Get() != null && CardTypeBanner.Get().HasCardDef && CardTypeBanner.Get().HasSameCardDef(m_cardDef.CardDef))
				{
					CardTypeBanner.Get().Hide();
				}
			}
			return true;
		}
		return false;
	}

	private bool Check_PlayOrHeroToHand(Actor oldActor, ref bool transitionHandled)
	{
		if ((m_prevZone is ZonePlay || m_prevZone is ZoneHero) && m_zone is ZoneHand)
		{
			DeactivateLifetimeEffects();
			if (m_mousedOver && m_entity.IsControlledByFriendlySidePlayer())
			{
				if (m_entity.HasSpellPower())
				{
					ZoneMgr.Get().OnSpellPowerEntityMousedOut(m_entity.GetSpellPowerSchool());
				}
				if (m_entity.HasHealingDoesDamageHint())
				{
					ZoneMgr.Get().OnHealingDoesDamageEntityMousedOut();
				}
			}
			bool useFastAnimations = GameState.Get().GetGameEntity().GetTag(GAME_TAG.USE_FAST_ACTOR_TRANSITION_ANIMATIONS) > 0;
			if (DoPlayToHandTransition(oldActor, wasInGraveyard: false, useFastAnimations))
			{
				transitionHandled = true;
			}
			return true;
		}
		return false;
	}

	private bool Check_HeroToGraveyard(Actor oldActor, ref bool transitionHandled)
	{
		if (m_prevZone is ZoneHero && m_zone is ZoneGraveyard)
		{
			oldActor.DoCardDeathVisuals();
			DeactivateCustomKeywordEffect();
			transitionHandled = true;
			m_actorReady = true;
			return true;
		}
		return false;
	}

	private bool Check_VariousZonesToGraveyard(Actor oldActor, ref bool transitionHandled)
	{
		if (m_prevZone != null && (m_prevZone is ZonePlay || m_prevZone is ZoneWeapon || m_prevZone is ZoneHeroPower || m_prevZone is ZoneBattlegroundHeroBuddy || m_prevZone is ZoneBattlegroundQuestReward || m_zone is ZoneBattlegroundTrinket || m_zone is ZoneBattlegroundClickableButton) && m_zone is ZoneGraveyard)
		{
			if (m_mousedOver && m_entity.IsControlledByFriendlySidePlayer() && m_prevZone is ZonePlay)
			{
				if (m_entity.HasSpellPower())
				{
					ZoneMgr.Get().OnSpellPowerEntityMousedOut(m_entity.GetSpellPowerSchool());
				}
				if (m_entity.HasHealingDoesDamageHint())
				{
					ZoneMgr.Get().OnHealingDoesDamageEntityMousedOut();
				}
			}
			if (m_entity.HasTag(GAME_TAG.DEATHRATTLE_RETURN_ZONE) && DoesCardReturnFromGraveyard())
			{
				m_playZoneBlockerSide = m_prevZone.m_Side;
				if (!m_entity.IsWeapon())
				{
					m_prevZone.AddLayoutBlocker();
				}
				m_goingThroughDeathrattleReturnfromGraveyard = true;
				TAG_ZONE returnZone = m_entity.GetTag<TAG_ZONE>(GAME_TAG.DEATHRATTLE_RETURN_ZONE);
				int controllerID = GetCardFutureController();
				Zone zone = ZoneMgr.Get().FindZoneForTags(controllerID, returnZone, m_entity.GetCardType(), m_entity);
				if (zone is ZoneDeck)
				{
					zone.AddLayoutBlocker();
				}
				m_actorWaitingToBeReplaced = oldActor;
				m_actor.Hide();
				transitionHandled = true;
				m_actorReady = true;
			}
			else if (HandlePlayActorDeath(oldActor))
			{
				transitionHandled = true;
			}
			return true;
		}
		return false;
	}

	private bool Check_DeckToHand(Actor oldActor, ref bool transitionHandled)
	{
		if (m_prevZone is ZoneDeck && m_zone is ZoneHand)
		{
			if (m_zone.m_Side == Player.Side.FRIENDLY)
			{
				if (GameState.Get().IsPastBeginPhase())
				{
					m_actorWaitingToBeReplaced = oldActor;
					m_cardStandInInteractive = false;
					if (!TurnStartManager.Get().IsCardDrawHandled(this))
					{
						DrawFriendlyCard(m_prevZone as ZoneDeck);
					}
					transitionHandled = true;
				}
				else
				{
					m_actor.TurnOffCollider();
					m_actor.SetActorState(ActorStateType.CARD_IDLE);
				}
			}
			else if (GameState.Get().IsPastBeginPhase())
			{
				if (oldActor != null)
				{
					oldActor.Destroy();
				}
				DrawOpponentCard(m_prevZone as ZoneDeck);
				transitionHandled = true;
			}
			return true;
		}
		return false;
	}

	private bool Check_SecretToGraveyard(Actor oldActor, ref bool transitionHandled)
	{
		if (m_prevZone is ZoneSecret && m_zone is ZoneGraveyard && m_entity.IsSecret())
		{
			transitionHandled = true;
			m_actorReady = true;
			if (CanShowSecretDeath())
			{
				ShowSecretDeath(oldActor);
			}
			m_shown = false;
			m_actor.Hide();
			return true;
		}
		if (m_prevZone is ZoneSecret && m_zone is ZoneGraveyard && m_entity.IsSigil())
		{
			transitionHandled = true;
			m_actorReady = true;
			m_shown = false;
			if (oldActor != null && oldActor != m_actor)
			{
				oldActor.Destroy();
			}
			m_actor.Hide();
			return true;
		}
		if (m_prevZone is ZoneSecret && m_zone is ZoneGraveyard && m_entity.IsObjective())
		{
			transitionHandled = true;
			m_actorReady = true;
			oldActor.GetComponent<Spell>().SafeActivateState(SpellStateType.DEATH);
			return true;
		}
		return false;
	}

	private bool Check_GraveyardToPlay(Actor oldActor, ref bool transitionHandled)
	{
		if (m_prevZone is ZoneGraveyard && m_zone is ZonePlay)
		{
			m_actor.Hide();
			StartCoroutine(ActivateReviveSpell());
			transitionHandled = true;
			return true;
		}
		return false;
	}

	private bool Check_DeckToGraveyard(Actor oldActor, ref bool transitionHandled)
	{
		if (m_prevZone is ZoneDeck && m_zone is ZoneGraveyard)
		{
			MillCard();
			transitionHandled = true;
			return true;
		}
		return false;
	}

	private bool Check_DeckToPlay(Actor oldActor, ref bool transitionHandled)
	{
		bool suppressDeckToPlay = m_entity.IsMinion() && m_entity.HasTag(GAME_TAG.LETTUCE_CONTROLLER);
		if (m_prevZone is ZoneDeck && m_zone is ZonePlay && !suppressDeckToPlay)
		{
			if (oldActor != null)
			{
				oldActor.Destroy();
			}
			AnimateDeckToPlay();
			transitionHandled = true;
			return true;
		}
		return false;
	}

	private bool Check_PlayToDeck(Actor oldActor, ref bool transitionHandled)
	{
		if (m_prevZone is ZonePlay && m_zone is ZoneDeck)
		{
			DeactivateLifetimeEffects();
			m_playZoneBlockerSide = m_prevZone.m_Side;
			m_prevZone.AddLayoutBlocker();
			ZoneMgr.Get().FindZoneOfType<ZoneDeck>(m_zone.m_Side).AddLayoutBlocker();
			DoPlayToDeckTransition(oldActor);
			transitionHandled = true;
			return true;
		}
		return false;
	}

	private bool Check_HandToDeck(Actor oldActor, ref bool transitionHandled)
	{
		if (m_prevZone is ZoneHand && m_zone is ZoneDeck && GameState.Get().IsPastBeginPhase())
		{
			if (!m_suppressHandToDeckTransition)
			{
				StartCoroutine(DoHandToDeckTransition(oldActor));
				if (oldActor.GetEntity() != null && oldActor.GetEntity().HasTag(GAME_TAG.IS_USING_TRADE_OPTION))
				{
					ActivateCharacterTradeEffects();
				}
			}
			else
			{
				oldActor.Destroy();
				m_actorReady = true;
			}
			m_suppressHandToDeckTransition = false;
			transitionHandled = true;
			return true;
		}
		return false;
	}

	private bool Check_DeathrattleReturnFromGraveyardToToDeckOrHand(Actor oldActor, ref bool transitionHandled)
	{
		if (m_goingThroughDeathrattleReturnfromGraveyard && m_zone is ZoneDeck)
		{
			m_goingThroughDeathrattleReturnfromGraveyard = false;
			if (HandleGraveyardToDeck(oldActor))
			{
				transitionHandled = true;
			}
			return true;
		}
		if (m_goingThroughDeathrattleReturnfromGraveyard && m_zone is ZoneHand)
		{
			m_goingThroughDeathrattleReturnfromGraveyard = false;
			if (HandleGraveyardToHand(oldActor))
			{
				transitionHandled = true;
			}
			return true;
		}
		return false;
	}

	private bool Check_IsZoneLettuceAbility(ref bool transitionHandled)
	{
		if (m_zone is ZoneLettuceAbility)
		{
			ActivateStateSpells();
			m_actorReady = true;
			transitionHandled = true;
			return true;
		}
		return false;
	}

	private void Check_TransitionNotHandled(Actor oldActor, ref bool transitionHandled)
	{
		if (!transitionHandled)
		{
			Check_TNH_IsMercenaryInPlayZone(oldActor, ref transitionHandled);
		}
		else if (!transitionHandled)
		{
			Check_TNH_OldActorEqualNewActor(oldActor, ref transitionHandled);
		}
		else if (!transitionHandled)
		{
			Check_TNH_ZoneIsSecret(oldActor, ref transitionHandled);
		}
		else if (!transitionHandled)
		{
			TransitionNotHandled_Final(oldActor);
		}
	}

	private void Check_TNH_IsMercenaryInPlayZone(Actor oldActor, ref bool transitionHandled)
	{
		bool creatingGame = GameState.Get().IsGameCreating();
		if (!m_entity.IsMercenary() || !(m_zone is ZonePlay))
		{
			return;
		}
		if (creatingGame)
		{
			ShowCard();
			ActivateStateSpells();
			m_actorReady = true;
		}
		else if (m_actor == oldActor && m_prevZone != null && m_prevZone.m_Side != m_zone.m_Side)
		{
			m_actorReady = true;
		}
		else
		{
			if (oldActor != null)
			{
				oldActor.Destroy();
			}
			m_actor.Hide();
			m_shown = true;
			ZonePlay obj = (ZonePlay)m_zone;
			SetTransitionStyle(ZoneTransitionStyle.INSTANT);
			obj.UpdateLayout();
			ActivateMinionSpawnEffects();
		}
		transitionHandled = true;
	}

	private void Check_TNH_OldActorEqualNewActor(Actor oldActor, ref bool transitionHandled)
	{
		if (!(oldActor == m_actor))
		{
			return;
		}
		if (m_prevZone != null && m_prevZone.m_Side != m_zone.m_Side)
		{
			if (m_prevZone is ZoneSecret && m_zone is ZoneSecret)
			{
				StartCoroutine(SwitchSecretSides());
				transitionHandled = true;
			}
			else if (m_prevZone is ZonePlay && m_zone is ZonePlay)
			{
				ActivateStateSpells();
				m_actorReady = true;
				transitionHandled = true;
			}
		}
		if (!transitionHandled)
		{
			m_actorReady = true;
		}
		transitionHandled = true;
	}

	private void Check_TNH_ZoneIsSecret(Actor oldActor, ref bool transitionHandled)
	{
		if (!(m_zone is ZoneSecret))
		{
			return;
		}
		m_shown = true;
		if ((bool)oldActor)
		{
			StartCoroutine(HandleSecretZoneTransitionActorCleanup(oldActor));
			if (m_questRewardActor != null)
			{
				m_questRewardActor.Destroy();
			}
		}
		m_transitionStyle = ZoneTransitionStyle.INSTANT;
		m_zone.UpdateLayout();
		ActivatePlaySpawnEffects(fallbackToDefault: false);
		ShowSecretQuestBirth();
		transitionHandled = true;
		m_actorReady = true;
		ActivateStateSpells();
	}

	private void TransitionNotHandled_Final(Actor oldActor)
	{
		if ((bool)oldActor)
		{
			oldActor.Destroy();
		}
		bool shouldCreateStartingCardStateEffects = m_zone.m_ServerTag == TAG_ZONE.PLAY || m_zone.m_ServerTag == TAG_ZONE.SECRET || m_zone.m_ServerTag == TAG_ZONE.HAND;
		if (IsShown() && shouldCreateStartingCardStateEffects)
		{
			ActivateStateSpells();
		}
		m_actorReady = true;
		if (IsShown())
		{
			ShowImpl();
		}
		else
		{
			HideImpl();
		}
	}

	private void OnActorChanged(Actor oldActor)
	{
		HideTooltip();
		bool transitionHandled = false;
		bool creatingGame = GameState.Get().IsGameCreating();
		bool suppressDeckToPlay = m_entity.IsMinion() && m_entity.HasTag(GAME_TAG.LETTUCE_CONTROLLER);
		if (m_prevZone == null && m_zone is ZoneGraveyard)
		{
			if (oldActor != null && oldActor != m_actor)
			{
				oldActor.Destroy();
			}
			if (IsShown())
			{
				HideCard();
			}
			else
			{
				HideImpl();
			}
			DeactivateHandStateSpells();
			transitionHandled = true;
			m_actorReady = true;
		}
		else if (oldActor == null)
		{
			bool willMulligan = GameState.Get().IsMulliganPhaseNowOrPending();
			if (m_zone is ZoneHand && GameState.Get().IsBeginPhase())
			{
				bool isFriendlyCoin = CosmeticCoinManager.Get().IsOwnedCoinCard(m_entity.GetCardId());
				if (willMulligan && !GameState.Get().HasTheCoinBeenSpawned())
				{
					if (isFriendlyCoin)
					{
						GameState.Get().NotifyOfCoinSpawn();
						m_actor.TurnOffCollider();
						m_actor.Hide();
						m_actorReady = true;
						transitionHandled = true;
						base.transform.position = Vector3.zero;
						m_doNotWarpToNewZone = true;
						m_doNotSort = true;
					}
					else
					{
						Player controller = m_entity.GetController();
						if (controller.IsOpposingSide() && this == m_zone.GetLastCard() && !controller.HasTag(GAME_TAG.FIRST_PLAYER))
						{
							GameState.Get().NotifyOfCoinSpawn();
							m_actor.TurnOffCollider();
							m_actorReady = true;
							transitionHandled = true;
						}
					}
				}
				if (!isFriendlyCoin)
				{
					ZoneMgr.Get().FindZoneOfType<ZoneDeck>(m_zone.m_Side).SetCardToInDeckState(this);
				}
			}
			else if (creatingGame)
			{
				TransformUtil.CopyWorld(base.transform, m_zone.transform);
				if (m_zone is ZonePlay || m_zone is ZoneHero || m_zone is ZoneHeroPower || m_zone is ZoneWeapon || m_zone is ZoneBattlegroundHeroBuddy || m_zone is ZoneBattlegroundQuestReward || m_zone is ZoneBattlegroundTrinket || m_zone is ZoneBattlegroundClickableButton)
				{
					ActivateLifetimeEffects();
				}
			}
			else
			{
				if (!m_doNotWarpToNewZone)
				{
					TransformUtil.CopyWorld(base.transform, m_zone.transform);
				}
				if (m_zone is ZoneHand)
				{
					if (!m_doNotWarpToNewZone)
					{
						ZoneHand zoneHand = (ZoneHand)m_zone;
						base.transform.localScale = zoneHand.GetCardScale();
						base.transform.localEulerAngles = zoneHand.GetCardRotation(this);
						base.transform.position = zoneHand.GetCardPosition(this);
					}
					Entity creatorEntity = GameState.Get().GetEntity(m_entity.GetTag(GAME_TAG.CREATOR));
					bool creatorSpawnsReplacementsWhenPlayed = creatorEntity != null && creatorEntity.HasReplacementsWhenPlayed() && m_entity.HasTag(GAME_TAG.CREATED_AS_ON_PLAY_REPLACEMENT);
					if (creatorEntity != null && creatorEntity.IsMinion())
					{
						if (creatorEntity.HasTag(GAME_TAG.MINIATURIZE) && !m_entity.HasTag(GAME_TAG.CREATED_BY_MINIATURIZE))
						{
							creatorSpawnsReplacementsWhenPlayed = false;
						}
						if (creatorEntity.HasTag(GAME_TAG.GIGANTIFY) && !m_entity.HasTag(GAME_TAG.CREATED_BY_GIGANTIFY))
						{
							creatorSpawnsReplacementsWhenPlayed = false;
						}
					}
					if (creatorSpawnsReplacementsWhenPlayed)
					{
						m_transitionStyle = ZoneTransitionStyle.INSTANT;
						ActivateHandSpawnSpell();
						InputManager.Get().GetFriendlyHand().ActivateCardWithReplacementsDeath();
						InputManager.Get().GetFriendlyHand().ClearReservedCard();
					}
					else
					{
						m_actorReady = true;
						m_shown = true;
						if (!m_doNotWarpToNewZone)
						{
							m_actor.Hide();
							ActivateHandSpawnSpell();
							transitionHandled = true;
						}
					}
				}
				if (m_prevZone == null && m_zone is ZonePlay zonePlay)
				{
					if (!m_doNotWarpToNewZone)
					{
						base.transform.position = zonePlay.GetCardPosition(this);
					}
					if (m_cardDef.CardDef.m_SuppressPlaySoundsDuringMulligan && GameState.Get().IsMulliganPhaseNowOrPending())
					{
						SuppressPlaySounds(suppress: true);
					}
					if (m_entity.HasTag(GAME_TAG.LINKED_ENTITY))
					{
						if (m_customSpawnSpellOverride != null)
						{
							ActivateMinionSpawnEffects();
						}
						else
						{
							m_transitionStyle = ZoneTransitionStyle.INSTANT;
							Transform offscreen = Board.Get().FindBone("SpawnOffscreen");
							base.transform.position = offscreen.position;
							ActivateCharacterPlayEffects();
							OnSpellFinished_StandardSpawnCharacter(null, null);
						}
					}
					else
					{
						m_actor.Hide();
						ActivateMinionSpawnEffects();
					}
					transitionHandled = true;
				}
				else if (!willMulligan && (m_zone is ZoneHeroPower || m_zone is ZoneWeapon || m_zone is ZoneBattlegroundHeroBuddy || m_zone is ZoneBattlegroundQuestReward || m_zone is ZoneBattlegroundTrinket || m_zone is ZoneBattlegroundClickableButton))
				{
					if (IsShown())
					{
						ActivatePlaySpawnEffects();
						transitionHandled = true;
						m_actorReady = true;
					}
				}
				else if (m_prevZone == null && m_zone is ZoneHero)
				{
					Entity currentHero = m_entity;
					GameState.Get().GetGameEntity().UpdateNameDisplay();
					if (currentHero.HasTag(GAME_TAG.TREAT_AS_PLAYED_HERO_CARD) && !currentHero.HasTag(GAME_TAG.SUPPRESS_HERO_STANDARD_SUMMON_FX))
					{
						Card oldHero = HeroCustomSummonSpell.GetOldHeroCard(currentHero.GetCard());
						if (oldHero != null)
						{
							currentHero.GetCard().GetActor().Hide();
							HeroCustomSummonSpell.HideStats(oldHero);
							oldHero.SetDelayBeforeHideInNullZoneVisuals(0.8f);
						}
						ActivateStandardSpawnHeroSpell();
						transitionHandled = true;
					}
				}
			}
		}
		else if (m_prevZone == null && (m_zone is ZoneHeroPower || m_zone is ZoneWeapon || m_zone is ZoneBattlegroundHeroBuddy || m_zone is ZoneBattlegroundQuestReward || m_zone is ZoneBattlegroundTrinket || m_zone is ZoneBattlegroundClickableButton))
		{
			oldActor.Destroy();
			TransformUtil.CopyWorld(base.transform, m_zone.transform);
			m_transitionStyle = ZoneTransitionStyle.INSTANT;
			ActivatePlaySpawnEffects();
			transitionHandled = true;
			m_actorReady = true;
		}
		else if (m_prevZone == null && m_zone is ZoneHand && oldActor == m_actor && !m_goingThroughDeathrattleReturnfromGraveyard)
		{
			ActivateHandStateSpells();
			transitionHandled = true;
			m_actorReady = true;
		}
		else if (m_prevZone == null && m_zone is ZonePlay && oldActor == m_actor)
		{
			ActivateMinionSpawnEffects();
			ShowCard();
			transitionHandled = true;
			m_actorReady = true;
		}
		else if (m_prevZone is ZoneHand && (m_zone is ZonePlay || m_zone is ZoneHero))
		{
			ActivateActorSpells_HandToPlay(oldActor);
			if (m_cardDef.CardDef.m_SuppressPlaySoundsOnSummon || m_entity.HasTag(GAME_TAG.CARD_DOES_NOTHING))
			{
				SuppressPlaySounds(suppress: true);
			}
			ActivateCharacterPlayEffects();
			m_actor.Hide();
			transitionHandled = true;
			if (CardTypeBanner.Get() != null && CardTypeBanner.Get().HasCardDef && CardTypeBanner.Get().HasSameCardDef(m_cardDef.CardDef))
			{
				CardTypeBanner.Get().Hide();
			}
			if (m_entity.IsMinion())
			{
				m_prevZone.GetController().GetHeroCard().ActivateLegendaryHeroAnimEvent(LegendaryHeroAnimations.SummonMinion);
			}
		}
		else if (m_prevZone is ZoneHand && m_zone is ZoneWeapon)
		{
			if (ActivateActorSpells_HandToWeapon(oldActor))
			{
				m_actor.Hide();
				transitionHandled = true;
				if (CardTypeBanner.Get() != null && CardTypeBanner.Get().HasCardDef && CardTypeBanner.Get().HasSameCardDef(m_cardDef.CardDef))
				{
					CardTypeBanner.Get().Hide();
				}
			}
		}
		else if ((m_prevZone is ZonePlay || m_prevZone is ZoneHero) && m_zone is ZoneHand)
		{
			DeactivateLifetimeEffects();
			if (m_mousedOver && m_entity.IsControlledByFriendlySidePlayer())
			{
				if (m_entity.HasSpellPower())
				{
					ZoneMgr.Get().OnSpellPowerEntityMousedOut(m_entity.GetSpellPowerSchool());
				}
				if (m_entity.HasHealingDoesDamageHint())
				{
					ZoneMgr.Get().OnHealingDoesDamageEntityMousedOut();
				}
			}
			bool useFastAnimations = GameState.Get().GetGameEntity().GetTag(GAME_TAG.USE_FAST_ACTOR_TRANSITION_ANIMATIONS) > 0;
			if (DoPlayToHandTransition(oldActor, wasInGraveyard: false, useFastAnimations))
			{
				transitionHandled = true;
			}
		}
		else if (m_prevZone is ZoneHero && m_zone is ZoneGraveyard)
		{
			oldActor.DoCardDeathVisuals();
			DeactivateCustomKeywordEffect();
			transitionHandled = true;
			m_actorReady = true;
		}
		else if (m_prevZone != null && (m_prevZone is ZonePlay || m_prevZone is ZoneWeapon || m_prevZone is ZoneHeroPower || m_prevZone is ZoneBattlegroundHeroBuddy || m_prevZone is ZoneBattlegroundQuestReward || m_zone is ZoneBattlegroundTrinket || m_zone is ZoneBattlegroundClickableButton) && m_zone is ZoneGraveyard)
		{
			if (m_mousedOver && m_entity.IsControlledByFriendlySidePlayer() && m_prevZone is ZonePlay)
			{
				if (m_entity.HasSpellPower())
				{
					ZoneMgr.Get().OnSpellPowerEntityMousedOut(m_entity.GetSpellPowerSchool());
				}
				if (m_entity.HasHealingDoesDamageHint())
				{
					ZoneMgr.Get().OnHealingDoesDamageEntityMousedOut();
				}
			}
			if (m_entity.HasTag(GAME_TAG.DEATHRATTLE_RETURN_ZONE) && DoesCardReturnFromGraveyard())
			{
				m_playZoneBlockerSide = m_prevZone.m_Side;
				if (!m_entity.IsWeapon())
				{
					m_prevZone.AddLayoutBlocker();
				}
				m_goingThroughDeathrattleReturnfromGraveyard = true;
				TAG_ZONE returnZone = m_entity.GetTag<TAG_ZONE>(GAME_TAG.DEATHRATTLE_RETURN_ZONE);
				int controllerID = GetCardFutureController();
				Zone zone = ZoneMgr.Get().FindZoneForTags(controllerID, returnZone, m_entity.GetCardType(), m_entity);
				if (zone is ZoneDeck)
				{
					zone.AddLayoutBlocker();
				}
				m_actorWaitingToBeReplaced = oldActor;
				m_actor.Hide();
				transitionHandled = true;
				m_actorReady = true;
			}
			else if (HandlePlayActorDeath(oldActor))
			{
				transitionHandled = true;
			}
		}
		else if (m_prevZone is ZoneDeck && m_zone is ZoneHand)
		{
			if (m_zone.m_Side == Player.Side.FRIENDLY)
			{
				if (GameState.Get().IsPastBeginPhase())
				{
					m_actorWaitingToBeReplaced = oldActor;
					m_cardStandInInteractive = false;
					if (!TurnStartManager.Get().IsCardDrawHandled(this))
					{
						DrawFriendlyCard(m_prevZone as ZoneDeck);
					}
					transitionHandled = true;
				}
				else
				{
					m_actor.TurnOffCollider();
					m_actor.SetActorState(ActorStateType.CARD_IDLE);
				}
			}
			else if (GameState.Get().IsPastBeginPhase())
			{
				if (oldActor != null)
				{
					oldActor.Destroy();
				}
				DrawOpponentCard(m_prevZone as ZoneDeck);
				transitionHandled = true;
			}
		}
		else if (m_prevZone is ZoneSecret && m_zone is ZoneGraveyard && GameState.Get().GetGameEntity().HasTag(GAME_TAG.COIN_MANA_GEM) && m_entity.IsSecretLike())
		{
			transitionHandled = true;
			m_actorReady = true;
			m_shown = false;
			m_actor.Hide();
			if (m_entity.IsObjective())
			{
				oldActor.GetComponent<Spell>().SafeActivateState(SpellStateType.DEATH);
			}
		}
		else if (m_prevZone is ZoneSecret && m_zone is ZoneGraveyard && m_entity.IsSecret())
		{
			transitionHandled = true;
			m_actorReady = true;
			if (CanShowSecretDeath())
			{
				ShowSecretDeath(oldActor);
			}
			m_shown = false;
			m_actor.Hide();
		}
		else if (m_prevZone is ZoneSecret && m_zone is ZoneGraveyard && m_entity.IsSigil())
		{
			transitionHandled = true;
			m_actorReady = true;
			m_shown = false;
			if (oldActor != null && oldActor != m_actor)
			{
				oldActor.Destroy();
			}
			m_actor.Hide();
		}
		else if (m_prevZone is ZoneSecret && m_zone is ZoneGraveyard && m_entity.IsObjective())
		{
			transitionHandled = true;
			m_actorReady = true;
			oldActor.GetComponent<Spell>().SafeActivateState(SpellStateType.DEATH);
		}
		else if (m_prevZone is ZoneGraveyard && m_zone is ZonePlay)
		{
			m_actor.Hide();
			StartCoroutine(ActivateReviveSpell());
			transitionHandled = true;
		}
		else if (m_prevZone is ZoneDeck && m_zone is ZoneGraveyard)
		{
			MillCard();
			transitionHandled = true;
		}
		else if (m_prevZone is ZoneDeck && m_zone is ZonePlay && !suppressDeckToPlay)
		{
			if (oldActor != null)
			{
				oldActor.Destroy();
			}
			AnimateDeckToPlay();
			transitionHandled = true;
		}
		else if (m_prevZone is ZonePlay && m_zone is ZoneDeck)
		{
			DeactivateLifetimeEffects();
			m_playZoneBlockerSide = m_prevZone.m_Side;
			m_prevZone.AddLayoutBlocker();
			ZoneMgr.Get().FindZoneOfType<ZoneDeck>(m_zone.m_Side).AddLayoutBlocker();
			DoPlayToDeckTransition(oldActor);
			transitionHandled = true;
		}
		else if (m_prevZone is ZoneHand && m_zone is ZoneDeck && GameState.Get().IsPastBeginPhase())
		{
			if (!m_suppressHandToDeckTransition)
			{
				StartCoroutine(DoHandToDeckTransition(oldActor));
				if (oldActor.GetEntity() != null && oldActor.GetEntity().HasTag(GAME_TAG.IS_USING_TRADE_OPTION))
				{
					ActivateCharacterTradeEffects();
				}
			}
			else
			{
				oldActor.Destroy();
				m_actorReady = true;
			}
			m_suppressHandToDeckTransition = false;
			transitionHandled = true;
		}
		else if (m_goingThroughDeathrattleReturnfromGraveyard && m_zone is ZoneDeck)
		{
			m_goingThroughDeathrattleReturnfromGraveyard = false;
			if (HandleGraveyardToDeck(oldActor))
			{
				transitionHandled = true;
			}
		}
		else if (m_goingThroughDeathrattleReturnfromGraveyard && m_zone is ZoneHand)
		{
			m_goingThroughDeathrattleReturnfromGraveyard = false;
			if (HandleGraveyardToHand(oldActor))
			{
				transitionHandled = true;
			}
		}
		else if (m_zone is ZoneLettuceAbility)
		{
			ActivateStateSpells();
			m_actorReady = true;
			transitionHandled = true;
		}
		if (!transitionHandled && m_entity.IsMercenary() && m_zone is ZonePlay)
		{
			if (creatingGame)
			{
				ShowCard();
				ActivateStateSpells();
				m_actorReady = true;
				return;
			}
			if (m_actor == oldActor && m_prevZone != null && m_prevZone.m_Side != m_zone.m_Side)
			{
				m_actorReady = true;
				return;
			}
			if (oldActor != null)
			{
				oldActor.Destroy();
			}
			m_actor.Hide();
			m_shown = true;
			ZonePlay obj = (ZonePlay)m_zone;
			SetTransitionStyle(ZoneTransitionStyle.INSTANT);
			obj.UpdateLayout();
			ActivateMinionSpawnEffects();
			return;
		}
		if (!transitionHandled && oldActor == m_actor)
		{
			if (m_prevZone != null && m_prevZone.m_Side != m_zone.m_Side)
			{
				if (m_prevZone is ZoneSecret && m_zone is ZoneSecret)
				{
					StartCoroutine(SwitchSecretSides());
					transitionHandled = true;
				}
				else if (m_prevZone is ZonePlay && m_zone is ZonePlay)
				{
					ActivateStateSpells();
					m_actorReady = true;
					transitionHandled = true;
				}
			}
			if (!transitionHandled)
			{
				m_actorReady = true;
			}
			return;
		}
		if (!transitionHandled && m_zone is ZoneSecret)
		{
			m_shown = true;
			if ((bool)oldActor)
			{
				StartCoroutine(HandleSecretZoneTransitionActorCleanup(oldActor));
				if (m_questRewardActor != null)
				{
					m_questRewardActor.Destroy();
				}
			}
			m_transitionStyle = ZoneTransitionStyle.INSTANT;
			m_zone.UpdateLayout();
			ActivatePlaySpawnEffects(fallbackToDefault: false);
			ShowSecretQuestBirth();
			transitionHandled = true;
			m_actorReady = true;
			ActivateStateSpells();
		}
		if (!transitionHandled)
		{
			if ((bool)oldActor)
			{
				oldActor.Destroy();
			}
			bool shouldCreateStartingCardStateEffects = m_zone.m_ServerTag == TAG_ZONE.PLAY || m_zone.m_ServerTag == TAG_ZONE.SECRET || m_zone.m_ServerTag == TAG_ZONE.HAND;
			if (IsShown() && shouldCreateStartingCardStateEffects)
			{
				ActivateStateSpells();
			}
			m_actorReady = true;
			if (IsShown())
			{
				ShowImpl();
			}
			else
			{
				HideImpl();
			}
		}
	}

	private IEnumerator HandleSecretZoneTransitionActorCleanup(Actor actor)
	{
		if (!(actor == null))
		{
			actor.Hide();
			Spell spell = actor.GetSpellIfLoaded(SpellType.POWER_UP);
			while (spell != null && spell.IsActive() && spell.GetActiveState() != SpellStateType.IDLE)
			{
				yield return null;
			}
			if (actor != null)
			{
				actor.Destroy();
			}
		}
	}

	private bool HandleGraveyardToDeck(Actor oldActor)
	{
		if ((bool)m_actorWaitingToBeReplaced)
		{
			if ((bool)oldActor)
			{
				oldActor.Destroy();
			}
			oldActor = m_actorWaitingToBeReplaced;
			m_actorWaitingToBeReplaced = null;
			DoPlayToDeckTransition(oldActor);
			return true;
		}
		return false;
	}

	private bool HandleGraveyardToHand(Actor oldActor)
	{
		if ((bool)m_actorWaitingToBeReplaced)
		{
			if ((bool)oldActor && oldActor != m_actor)
			{
				oldActor.Destroy();
			}
			oldActor = m_actorWaitingToBeReplaced;
			m_actorWaitingToBeReplaced = null;
			bool useFastAnimations = GameState.Get().GetGameEntity().GetTag(GAME_TAG.USE_FAST_ACTOR_TRANSITION_ANIMATIONS) > 0;
			if (DoPlayToHandTransition(oldActor, wasInGraveyard: true, useFastAnimations))
			{
				return true;
			}
		}
		return false;
	}

	public bool CardStandInIsInteractive()
	{
		return m_cardStandInInteractive;
	}

	private Transform GetPathStartTransform(ZoneDeck srcDeck, bool revealOnDrawn)
	{
		Player.Side side = srcDeck.m_Side;
		string boneName;
		if ((uint)side > 1u && side == Player.Side.OPPOSING)
		{
			if (revealOnDrawn)
			{
				boneName = "OpponentDrawCardAndReveal";
				if ((bool)UniversalInputManager.UsePhoneUI)
				{
					boneName += "_phone";
				}
			}
			else
			{
				boneName = "OpponentDrawCard";
			}
		}
		else
		{
			boneName = "FriendlyDrawCard";
		}
		return Board.Get().FindBone(boneName);
	}

	private void ReadyCardForDraw(ZoneDeck srcDeck = null)
	{
		if (srcDeck == null)
		{
			srcDeck = (m_beingDrawnByOpponent ? GameState.Get().GetOpposingPlayer() : GameState.Get().GetFriendlySidePlayer()).GetDeckZone();
		}
		srcDeck.SetCardToInDeckState(this);
	}

	public void DrawFriendlyCard(ZoneDeck srcDeck = null)
	{
		if (srcDeck == null)
		{
			srcDeck = GameState.Get().GetFriendlySidePlayer().GetDeckZone();
		}
		StartCoroutine(DrawFriendlyCardWithTiming(srcDeck));
	}

	private IEnumerator DrawFriendlyCardWithTiming(ZoneDeck srcDeck)
	{
		m_doNotSort = true;
		m_transitionStyle = ZoneTransitionStyle.SLOW;
		m_actor.Hide();
		while ((bool)GameState.Get().GetFriendlyCardBeingDrawn())
		{
			yield return null;
		}
		srcDeck.NotifyCardAnimationStart();
		GameState.Get().SetFriendlyCardBeingDrawn(this);
		ReadyCardForDraw(srcDeck);
		Actor cardDrawStandIn = Gameplay.Get().GetCardDrawStandIn();
		cardDrawStandIn.transform.parent = m_actor.transform.parent;
		cardDrawStandIn.transform.localPosition = Vector3.zero;
		cardDrawStandIn.transform.localScale = Vector3.one;
		cardDrawStandIn.transform.localEulerAngles = new Vector3(0f, 0f, 180f);
		cardDrawStandIn.Show();
		cardDrawStandIn.GetRootObject().GetComponentInChildren<CardBackDisplay>().SetCardBack(CardBackManager.CardBackSlot.FRIENDLY);
		if (m_actorWaitingToBeReplaced != null)
		{
			m_actorWaitingToBeReplaced.Destroy();
			m_actorWaitingToBeReplaced = null;
		}
		DetermineIfOverrideDrawTimeScale();
		cardDrawStandIn.transform.parent = null;
		cardDrawStandIn.Hide();
		m_actor.Show();
		m_actor.TurnOffCollider();
		bool hasDrawOverride = GetController() != null && GetController().HasTag(GAME_TAG.DRAW_SPELL_OVERRIDE);
		if (srcDeck.GetController().GetSide() == Player.Side.OPPOSING)
		{
			hasDrawOverride = false;
		}
		Spell spell = null;
		if (hasDrawOverride)
		{
			int overrideDrawAnimation = GetController().GetTag(GAME_TAG.DRAW_SPELL_OVERRIDE);
			spell = m_actor.GetSpell((SpellType)overrideDrawAnimation);
			if (spell == null)
			{
				Debug.LogError("No spell");
			}
		}
		hasDrawOverride = hasDrawOverride && spell != null;
		if (hasDrawOverride)
		{
			m_actor.Hide();
			ZoneDeck deck = ZoneMgr.Get().FindZoneOfType<ZoneDeck>(Player.Side.FRIENDLY);
			Vector3 pos = base.gameObject.transform.position;
			pos.x = deck.gameObject.transform.position.x;
			pos.y = 1.2f;
			pos.z = deck.gameObject.transform.position.z;
			base.gameObject.transform.position = pos;
			Vector3 rot = base.gameObject.transform.rotation.eulerAngles;
			rot.x = 0f;
			rot.y = 0f;
			rot.z = 0f;
			base.gameObject.transform.rotation = Quaternion.Euler(rot);
			iTween.Stop(base.gameObject);
			spell.gameObject.transform.parent = null;
			SetDoNotSort(on: true);
			FsmBool fsmBool = spell.GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("isOpponent");
			if (fsmBool != null)
			{
				fsmBool.Value = false;
			}
			spell.SetSource(base.gameObject);
			SpellUtils.ActivateStateIfNecessary(spell, SpellStateType.ACTION);
		}
		else
		{
			Transform finalBone = GetPathStartTransform(srcDeck, revealOnDrawn: true);
			Vector3[] drawPath = new Vector3[3]
			{
				base.gameObject.transform.position,
				base.gameObject.transform.position + ABOVE_DECK_OFFSET,
				finalBone.position
			};
			float moveTime = 1.5f * m_drawTimeScale.Value;
			Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
			moveArgs.Add("path", drawPath);
			moveArgs.Add("time", moveTime);
			moveArgs.Add("easetype", iTween.EaseType.easeInSineOutExpo);
			iTween.MoveTo(base.gameObject, moveArgs);
			base.gameObject.transform.localEulerAngles = new Vector3(270f, 270f, 0f);
			Vector3 targetEulerAngles = new Vector3(0f, 0f, 357f);
			float rotateTime = 1.35f * m_drawTimeScale.Value;
			float rotateDealyTime = 0.15f * m_drawTimeScale.Value;
			Hashtable rotateArgs = iTweenManager.Get().GetTweenHashTable();
			rotateArgs.Add("rotation", targetEulerAngles);
			rotateArgs.Add("time", rotateTime);
			rotateArgs.Add("delay", rotateDealyTime);
			iTween.RotateTo(base.gameObject, rotateArgs);
			float scaleTime = 0.75f * m_drawTimeScale.Value;
			float scaleDealyTime = 0.15f * m_drawTimeScale.Value;
			Hashtable scaleArgs = iTweenManager.Get().GetTweenHashTable();
			scaleArgs.Add("scale", finalBone.localScale);
			scaleArgs.Add("time", scaleTime);
			scaleArgs.Add("delay", scaleDealyTime);
			iTween.ScaleTo(base.gameObject, scaleArgs);
			SoundManager.Get().LoadAndPlay("draw_card_1.prefab:19dd221ebfed9754e85ef1f104e0fddb", base.gameObject);
		}
		srcDeck.UpdateLayout();
		PowerTask cardDrawBlockingTask = GetPowerTaskToBlockCardDraw();
		if (hasDrawOverride)
		{
			while (spell.IsActive())
			{
				yield return null;
			}
			SpellManager.Get().ReleaseSpell(spell);
		}
		else
		{
			while (iTween.Count(base.gameObject) > 0)
			{
				yield return null;
			}
		}
		m_actorReady = true;
		if (ShouldCardDrawWaitForTurnStartSpells())
		{
			yield return StartCoroutine(WaitForCardDrawBlockingTurnStartSpells());
		}
		else if (cardDrawBlockingTask != null)
		{
			while (!cardDrawBlockingTask.IsCompleted())
			{
				yield return null;
			}
		}
		m_doNotSort = false;
		GameState.Get().ClearCardBeingDrawn(this);
		ResetCardDrawTimeScale();
		srcDeck.NotifyCardAnimationFinish();
		if (m_zone != null && m_zone is ZoneHand)
		{
			ZoneHand handZone = (ZoneHand)m_zone;
			SoundManager.Get().LoadAndPlay("add_card_to_hand_1.prefab:bf6b149b859734c4faf9a96356c53646", base.gameObject);
			ActivateStateSpells();
			RefreshActor();
			m_zone.UpdateLayout();
			yield return new WaitForSeconds(0.3f);
			m_cardStandInInteractive = true;
			handZone.MakeStandInInteractive(this);
		}
	}

	public void FocusCardWhilePingwheelIsActive(bool pingWheelActive)
	{
		if (m_zone != null && (m_zone is ZoneHand || m_zone is ZoneTeammateHand))
		{
			ZoneHand handZone = ZoneMgr.Get().FindZonesOfType<ZoneHand>(Player.Side.FRIENDLY).FirstOrDefault();
			if (pingWheelActive)
			{
				handZone.SetOverrideFocusCard(this);
			}
			else
			{
				handZone.ClearOverrideFocusCard();
			}
		}
	}

	public bool IsBeingDrawnByOpponent()
	{
		return m_beingDrawnByOpponent;
	}

	private void DrawOpponentCard(ZoneDeck srcDeck = null)
	{
		if (srcDeck == null)
		{
			srcDeck = GameState.Get().GetFriendlySidePlayer().GetDeckZone();
		}
		StartCoroutine(DrawOpponentCardWithTiming(srcDeck));
	}

	private IEnumerator DrawOpponentCardWithTiming(ZoneDeck srcDeck)
	{
		m_doNotSort = true;
		m_beingDrawnByOpponent = true;
		m_actor.Hide();
		while ((bool)GameState.Get().GetOpponentCardBeingDrawn())
		{
			yield return null;
		}
		if (GetZonePosition() == 0)
		{
			yield return null;
		}
		m_actor.Show();
		GameState.Get().SetOpponentCardBeingDrawn(this);
		ReadyCardForDraw(srcDeck);
		ZoneHand handZone = (ZoneHand)m_zone;
		handZone.UpdateLayout();
		if (m_entity.HasTag(GAME_TAG.REVEALED))
		{
			StartCoroutine(DrawKnownOpponentCard(handZone, srcDeck));
		}
		else
		{
			StartCoroutine(DrawUnknownOpponentCard(handZone, srcDeck));
		}
	}

	private IEnumerator DrawUnknownOpponentCard(ZoneHand handZone, ZoneDeck srcDeck)
	{
		SoundManager.Get().LoadAndPlay("draw_card_and_add_to_hand_opp_1.prefab:5a05fbb2c5833a94182e1b454647d5c8", base.gameObject);
		base.gameObject.transform.rotation = IN_DECK_HIDDEN_ROTATION;
		DetermineIfOverrideDrawTimeScale();
		bool hasDrawOverride = GetController() != null && GetController().HasTag(GAME_TAG.DRAW_SPELL_OVERRIDE);
		if (srcDeck.GetController().GetSide() == Player.Side.FRIENDLY)
		{
			hasDrawOverride = false;
		}
		Spell spell = null;
		if (hasDrawOverride)
		{
			int overrideDrawAnimation = GetController().GetTag(GAME_TAG.DRAW_SPELL_OVERRIDE);
			spell = m_actor.GetSpell((SpellType)overrideDrawAnimation);
			if (spell == null)
			{
				Debug.LogError("No spell");
			}
		}
		hasDrawOverride = hasDrawOverride && spell != null;
		if (hasDrawOverride)
		{
			m_actor.Hide();
			ZoneDeck deck = ZoneMgr.Get().FindZoneOfType<ZoneDeck>(Player.Side.OPPOSING);
			Vector3 pos = base.gameObject.transform.position;
			pos.x = deck.gameObject.transform.position.x;
			pos.y = 1.2f;
			pos.z = deck.gameObject.transform.position.z;
			base.gameObject.transform.position = pos;
			Vector3 rot = base.gameObject.transform.rotation.eulerAngles;
			rot.x = 0f;
			rot.y = 0f;
			rot.z = 0f;
			base.gameObject.transform.rotation = Quaternion.Euler(rot);
			iTween.Stop(base.gameObject);
			spell.gameObject.transform.parent = null;
			SetDoNotSort(on: true);
			FsmBool fsmBool = spell.GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("isOpponent");
			if (fsmBool != null)
			{
				fsmBool.Value = true;
			}
			spell.SetSource(base.gameObject);
			SpellUtils.ActivateStateIfNecessary(spell, SpellStateType.ACTION);
		}
		else
		{
			Transform finalBone = GetPathStartTransform(srcDeck, revealOnDrawn: false);
			Vector3[] drawPath = new Vector3[4]
			{
				base.gameObject.transform.position,
				base.gameObject.transform.position + ABOVE_DECK_OFFSET,
				finalBone.position,
				handZone.GetCardPosition(this)
			};
			float moveTime = 1.75f * m_drawTimeScale.Value;
			Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
			moveArgs.Add("path", drawPath);
			moveArgs.Add("time", moveTime);
			moveArgs.Add("easetype", iTween.EaseType.easeInOutQuart);
			iTween.MoveTo(base.gameObject, moveArgs);
			float rotateTime = 0.7f * m_drawTimeScale.Value;
			float rotateDealyTime = 0.8f * m_drawTimeScale.Value;
			Hashtable rotateArgs = iTweenManager.Get().GetTweenHashTable();
			rotateArgs.Add("rotation", handZone.GetCardRotation(this));
			rotateArgs.Add("time", rotateTime);
			rotateArgs.Add("delay", rotateDealyTime);
			rotateArgs.Add("easetype", iTween.EaseType.easeInOutCubic);
			iTween.RotateTo(base.gameObject, rotateArgs);
			float scaleTime = 0.7f * m_drawTimeScale.Value;
			float scaleDealyTime = 0.8f * m_drawTimeScale.Value;
			Hashtable scaleArgs = iTweenManager.Get().GetTweenHashTable();
			scaleArgs.Add("scale", handZone.GetCardScale());
			scaleArgs.Add("time", scaleTime);
			scaleArgs.Add("delay", scaleDealyTime);
			scaleArgs.Add("easetype", iTween.EaseType.easeInOutQuint);
			iTween.ScaleTo(base.gameObject, scaleArgs);
		}
		srcDeck.UpdateLayout();
		yield return new WaitForSeconds(0.2f);
		m_actorReady = true;
		yield return new WaitForSeconds(0.6f);
		GameState.Get().UpdateOptionHighlights();
		if (hasDrawOverride)
		{
			while (spell.IsActive())
			{
				yield return null;
			}
			SpellManager.Get().ReleaseSpell(spell);
		}
		else
		{
			while (iTween.Count(base.gameObject) > 0)
			{
				yield return null;
			}
		}
		m_doNotSort = false;
		m_beingDrawnByOpponent = false;
		GameState.Get().SetOpponentCardBeingDrawn(null);
		ResetCardDrawTimeScale();
		handZone.UpdateLayout();
	}

	private IEnumerator DrawKnownOpponentCard(ZoneHand handZone, ZoneDeck srcDeck)
	{
		Actor handActor = null;
		bool loadingActor = true;
		PrefabCallback<GameObject> actorLoadedCallback = delegate(AssetReference assetRef, GameObject go, object callbackData)
		{
			loadingActor = false;
			if (go == null)
			{
				Error.AddDevFatal("Card.DrawKnownOpponentCard() - failed to load {0}", assetRef);
			}
			else
			{
				handActor = go.GetComponent<Actor>();
				if (handActor == null)
				{
					Error.AddDevFatal("Card.DrawKnownOpponentCard() - instance of {0} has no Actor component", base.name);
				}
			}
		};
		string actorPath = ActorNames.GetHandActor(m_entity);
		if (!AssetLoader.Get().InstantiatePrefab(actorPath, actorLoadedCallback, null, AssetLoadingOptions.IgnorePrefabPosition))
		{
			actorLoadedCallback(actorPath, null, null);
		}
		while (loadingActor)
		{
			yield return null;
		}
		if ((bool)handActor)
		{
			handActor.SetEntity(m_entity);
			handActor.SetCardDef(m_cardDef);
			handActor.UpdateAllComponents();
			StartCoroutine(RevealDrawnOpponentCard(actorPath, handActor, handZone, srcDeck));
		}
		else
		{
			StartCoroutine(DrawUnknownOpponentCard(handZone, srcDeck));
		}
	}

	private IEnumerator RevealDrawnOpponentCard(string handActorPath, Actor handActor, ZoneHand handZone, ZoneDeck srcDeck)
	{
		SoundManager.Get().LoadAndPlay("draw_card_1.prefab:19dd221ebfed9754e85ef1f104e0fddb", base.gameObject);
		handActor.transform.parent = m_actor.transform.parent;
		TransformUtil.CopyLocal(handActor, m_actor);
		m_actor.Hide();
		DetermineIfOverrideDrawTimeScale();
		bool hasDrawOverride = GetController() != null && GetController().HasTag(GAME_TAG.DRAW_SPELL_OVERRIDE);
		if (srcDeck.GetController().GetSide() == Player.Side.FRIENDLY)
		{
			hasDrawOverride = false;
		}
		Spell spell = null;
		if (hasDrawOverride)
		{
			int overrideDrawAnimation = GetController().GetTag(GAME_TAG.DRAW_SPELL_OVERRIDE);
			spell = m_actor.GetSpell((SpellType)overrideDrawAnimation);
			if (spell == null)
			{
				Debug.LogError("No spell");
			}
		}
		hasDrawOverride = hasDrawOverride && spell != null;
		if (hasDrawOverride)
		{
			m_actor.Hide();
			ZoneDeck deck = ZoneMgr.Get().FindZoneOfType<ZoneDeck>(Player.Side.OPPOSING);
			Vector3 pos = base.gameObject.transform.position;
			pos.x = deck.gameObject.transform.position.x;
			pos.y = 1.2f;
			pos.z = deck.gameObject.transform.position.z;
			base.gameObject.transform.position = pos;
			Vector3 rot = base.gameObject.transform.rotation.eulerAngles;
			rot.x = 0f;
			rot.y = 0f;
			rot.z = 0f;
			base.gameObject.transform.rotation = Quaternion.Euler(rot);
			iTween.Stop(base.gameObject);
			spell.gameObject.transform.parent = null;
			SetDoNotSort(on: true);
			FsmBool fsmBool = spell.GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("isOpponent");
			if (fsmBool != null)
			{
				fsmBool.Value = true;
			}
			spell.SetSource(base.gameObject);
			SpellUtils.ActivateStateIfNecessary(spell, SpellStateType.ACTION);
		}
		else
		{
			base.gameObject.transform.localEulerAngles = new Vector3(270f, 90f, 0f);
			Transform finalBone = GetPathStartTransform(srcDeck, revealOnDrawn: true);
			Vector3[] drawPath = new Vector3[3]
			{
				base.gameObject.transform.position,
				base.gameObject.transform.position + ABOVE_DECK_OFFSET,
				finalBone.position
			};
			float moveTime = 1.75f * m_drawTimeScale.Value;
			Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
			moveArgs.Add("path", drawPath);
			moveArgs.Add("time", moveTime);
			moveArgs.Add("easetype", iTween.EaseType.easeInOutQuart);
			iTween.MoveTo(base.gameObject, moveArgs);
			float rotateTime = 0.7f * m_drawTimeScale.Value;
			float rotateDelayTime = 0.8f * m_drawTimeScale.Value;
			Hashtable rotateArgs = iTweenManager.Get().GetTweenHashTable();
			rotateArgs.Add("rotation", finalBone.eulerAngles);
			rotateArgs.Add("time", rotateTime);
			rotateArgs.Add("delay", rotateDelayTime);
			rotateArgs.Add("easetype", iTween.EaseType.easeInOutCubic);
			iTween.RotateTo(base.gameObject, rotateArgs);
			float scaleTime = 0.7f * m_drawTimeScale.Value;
			float scaleDelayTime = 0.8f * m_drawTimeScale.Value;
			Hashtable scaleArgs = iTweenManager.Get().GetTweenHashTable();
			scaleArgs.Add("scale", finalBone.localScale);
			scaleArgs.Add("time", scaleTime);
			scaleArgs.Add("delay", scaleDelayTime);
			scaleArgs.Add("easetype", iTween.EaseType.easeInOutQuint);
			iTween.ScaleTo(base.gameObject, scaleArgs);
		}
		srcDeck.UpdateLayout();
		yield return new WaitForSeconds(1.75f);
		if (hasDrawOverride)
		{
			while (spell.IsActive())
			{
				yield return null;
			}
			SpellManager.Get().ReleaseSpell(spell);
		}
		m_actorReady = true;
		m_beingDrawnByOpponent = false;
		string actorName = m_actorPath;
		m_actorWaitingToBeReplaced = m_actor;
		m_actorPath = handActorPath;
		m_actor = handActor;
		PowerTask cardDrawBlockingTask = GetPowerTaskToBlockCardDraw();
		if (cardDrawBlockingTask != null)
		{
			while (!cardDrawBlockingTask.IsCompleted())
			{
				yield return null;
			}
			if (handActor == null)
			{
				handActor = m_actor;
			}
		}
		if (m_entity.GetZone() != TAG_ZONE.HAND)
		{
			m_doNotSort = false;
			GameState.Get().ClearCardBeingDrawn(this);
			ResetCardDrawTimeScale();
		}
		else
		{
			m_actor = m_actorWaitingToBeReplaced;
			m_actorPath = actorName;
			m_actorWaitingToBeReplaced = null;
			m_beingDrawnByOpponent = true;
			yield return StartCoroutine(HideRevealedOpponentCard(handActor));
		}
	}

	private IEnumerator HideRevealedOpponentCard(Actor handActor)
	{
		float flipSec = 0.5f;
		float revealSec = 0.525f * flipSec;
		if (!GetController().IsRevealed())
		{
			float handActorRotation = 180f;
			TransformUtil.SetEulerAngleZ(m_actor.gameObject, 0f - handActorRotation);
			if (handActor != null)
			{
				Hashtable handArgs = iTweenManager.Get().GetTweenHashTable();
				handArgs.Add("z", handActorRotation);
				handArgs.Add("time", flipSec);
				handArgs.Add("easetype", iTween.EaseType.easeInOutCubic);
				iTween.RotateAdd(handActor.gameObject, handArgs);
			}
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("z", handActorRotation);
			args.Add("time", flipSec);
			args.Add("easetype", iTween.EaseType.easeInOutCubic);
			iTween.RotateAdd(m_actor.gameObject, args);
		}
		Action<object> changeActorsCallback = delegate
		{
			if (handActor != null)
			{
				UnityEngine.Object.Destroy(handActor.gameObject);
			}
			m_actor.Show();
		};
		Hashtable timerArgs = iTweenManager.Get().GetTweenHashTable();
		timerArgs.Add("time", revealSec);
		timerArgs.Add("oncomplete", changeActorsCallback);
		iTween.Timer(m_actor.gameObject, timerArgs);
		yield return new WaitForSeconds(flipSec);
		m_doNotSort = false;
		m_beingDrawnByOpponent = false;
		GameState.Get().SetOpponentCardBeingDrawn(null);
		ResetCardDrawTimeScale();
		SoundManager.Get().LoadAndPlay("add_card_to_hand_1.prefab:bf6b149b859734c4faf9a96356c53646", base.gameObject);
		ActivateStateSpells();
		RefreshActor();
		m_zone.UpdateLayout();
	}

	private void AnimateDeckToPlay()
	{
		if (m_customSpawnSpellOverride == null)
		{
			((ZonePlay)m_zone).AddLayoutBlocker();
			ZoneDeck deck = ZoneMgr.Get().FindZoneOfType<ZoneDeck>(m_zone.m_Side);
			if (m_latestZoneChange != null && m_latestZoneChange.GetSourceControllerId() != 0 && m_latestZoneChange.GetSourceControllerId() != m_latestZoneChange.GetDestinationControllerId() && m_latestZoneChange.GetSourceZone() is ZoneDeck)
			{
				deck = (ZoneDeck)m_latestZoneChange.GetSourceZone();
			}
			deck.SetCardToInDeckState(this);
			m_doNotSort = true;
			GameObject cardFaceActorObject = AssetLoader.Get().InstantiatePrefab(ActorNames.GetHandActor(m_entity), AssetLoadingOptions.IgnorePrefabPosition);
			Actor cardFaceActor = cardFaceActorObject.GetComponent<Actor>();
			SetupDeckToPlayActor(cardFaceActor, cardFaceActorObject);
			SpellType outSpellType = m_cardDef.CardDef.DetermineSummonOutSpell_HandToPlay(this);
			Spell outSpell = cardFaceActor.GetSpell(outSpellType);
			GameObject hiddenActorObject = AssetLoader.Get().InstantiatePrefab("Card_Hidden.prefab:1a94649d257bc284ca6e2962f634a8b9", AssetLoadingOptions.IgnorePrefabPosition);
			Actor hiddenActor = hiddenActorObject.GetComponent<Actor>();
			SetupDeckToPlayActor(hiddenActor, hiddenActorObject);
			StartCoroutine(AnimateDeckToPlay(cardFaceActor, outSpell, hiddenActor));
		}
		else
		{
			m_actor.Hide();
			ZonePlay obj = (ZonePlay)m_zone;
			SetTransitionStyle(ZoneTransitionStyle.INSTANT);
			obj.UpdateLayout();
			ActivateMinionSpawnEffects();
		}
	}

	private void SetupDeckToPlayActor(Actor actor, GameObject actorObject)
	{
		actor.SetEntity(m_entity);
		actor.SetCardDef(m_cardDef);
		actor.UpdateAllComponents();
		actorObject.transform.parent = base.transform;
		actorObject.transform.localPosition = Vector3.zero;
		actorObject.transform.localScale = Vector3.one;
		actorObject.transform.localRotation = Quaternion.identity;
	}

	private IEnumerator AnimateDeckToPlay(Actor cardFaceActor, Spell outSpell, Actor hiddenActor)
	{
		ZoneDeck zoneDeck = m_prevZone as ZoneDeck;
		zoneDeck?.NotifyCardAnimationStart();
		cardFaceActor.Hide();
		m_actor.Hide();
		hiddenActor.Hide();
		m_inputEnabled = false;
		SoundManager.Get().LoadAndPlay("draw_card_into_play.prefab:52139cc25c53e184fab47b23c72df0d1", base.gameObject);
		base.gameObject.transform.localEulerAngles = new Vector3(270f, 90f, 0f);
		iTween.MoveTo(base.gameObject, base.gameObject.transform.position + ABOVE_DECK_OFFSET, 0.6f);
		Hashtable rotateArgs = iTweenManager.Get().GetTweenHashTable();
		rotateArgs.Add("rotation", new Vector3(0f, 0f, 0f));
		rotateArgs.Add("time", 0.7f);
		rotateArgs.Add("delay", 0.6f);
		rotateArgs.Add("easetype", iTween.EaseType.easeInOutCubic);
		rotateArgs.Add("islocal", true);
		iTween.RotateTo(base.gameObject, rotateArgs);
		hiddenActor.Show();
		yield return new WaitForSeconds(0.4f);
		zoneDeck?.NotifyCardAnimationFinish();
		Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
		moveArgs.Add("position", new Vector3(0f, 3f, 0f));
		moveArgs.Add("time", 1f);
		moveArgs.Add("delay", 0f);
		moveArgs.Add("islocal", true);
		iTween.MoveTo(hiddenActor.gameObject, moveArgs);
		m_doNotSort = false;
		ZonePlay obj = (ZonePlay)m_zone;
		obj.RemoveLayoutBlocker();
		obj.SetTransitionTime(1.6f);
		obj.UpdateLayout();
		yield return new WaitForSeconds(0.2f);
		float cardFlipTime = 0.35f;
		Hashtable flipRotateArgs = iTweenManager.Get().GetTweenHashTable();
		flipRotateArgs.Add("rotation", new Vector3(0f, 0f, -90f));
		flipRotateArgs.Add("time", cardFlipTime);
		flipRotateArgs.Add("delay", 0f);
		flipRotateArgs.Add("easetype", iTween.EaseType.easeInCubic);
		flipRotateArgs.Add("islocal", true);
		iTween.RotateTo(hiddenActor.gameObject, flipRotateArgs);
		yield return new WaitForSeconds(cardFlipTime);
		hiddenActor.Destroy();
		cardFaceActor.Show();
		cardFaceActor.gameObject.transform.localPosition = new Vector3(0f, 3f, 0f);
		cardFaceActor.gameObject.transform.Rotate(new Vector3(0f, 0f, 90f));
		Hashtable faceRotateArgs = iTweenManager.Get().GetTweenHashTable();
		faceRotateArgs.Add("rotation", new Vector3(0f, 0f, 0f));
		faceRotateArgs.Add("time", cardFlipTime);
		faceRotateArgs.Add("delay", 0f);
		faceRotateArgs.Add("easetype", iTween.EaseType.easeOutCubic);
		faceRotateArgs.Add("islocal", true);
		iTween.RotateTo(cardFaceActor.gameObject, faceRotateArgs);
		m_actor.gameObject.transform.localPosition = new Vector3(0f, 2.86f, 0f);
		cardFaceActor.gameObject.transform.localPosition = new Vector3(0f, 2.86f, 0f);
		Hashtable hiddenMoveArgs = iTweenManager.Get().GetTweenHashTable();
		hiddenMoveArgs.Add("position", Vector3.zero);
		hiddenMoveArgs.Add("time", 1f);
		hiddenMoveArgs.Add("delay", 0f);
		hiddenMoveArgs.Add("islocal", true);
		iTween.MoveTo(hiddenActor.gameObject, hiddenMoveArgs);
		ActivateSpell(outSpell, OnSpellFinished_HandToPlay_SummonOut, null, OnSpellStateFinished_DestroyActor);
		ActivateCharacterPlayEffects();
		m_actor.gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
	}

	public void SetSkipMilling(bool skipMilling)
	{
		m_skipMilling = skipMilling;
	}

	private void MillCard()
	{
		if (m_skipMilling)
		{
			m_actor.Hide();
			return;
		}
		if (!m_entity.HasTag(GAME_TAG.IGNORE_SUPPRESS_MILL_ANIMATION) && (m_entity.HasTag(GAME_TAG.SUPPRESS_MILL_ANIMATION) || m_entity.GetController().HasTag(GAME_TAG.SUPPRESS_MILL_ANIMATION)))
		{
			m_actor.Hide();
			return;
		}
		if (m_entity != null)
		{
			m_beingDrawnByOpponent = m_entity.GetController().IsOpposingSide();
		}
		StartCoroutine(MillCardWithTiming());
	}

	private IEnumerator MillCardWithTiming()
	{
		SetDoNotSort(on: true);
		ReadyCardForDraw();
		Player cardOwner = m_entity.GetController();
		bool friendly = cardOwner.IsFriendlySide();
		string boneName;
		if (friendly)
		{
			while ((bool)GameState.Get().GetFriendlyCardBeingDrawn())
			{
				yield return null;
			}
			GameState.Get().SetFriendlyCardBeingDrawn(this);
			boneName = "FriendlyMillCard";
		}
		else
		{
			while ((bool)GameState.Get().GetOpponentCardBeingDrawn())
			{
				yield return null;
			}
			GameState.Get().SetOpponentCardBeingDrawn(this);
			boneName = "OpponentMillCard";
		}
		int turn = GameState.Get().GetTurn();
		if (turn != GameState.Get().GetLastTurnRemindedOfFullHand() && cardOwner.GetHandZone().GetCardCount() >= 10)
		{
			GameState.Get().SetLastTurnRemindedOfFullHand(turn);
			cardOwner.GetHeroCard().PlayEmote(EmoteType.ERROR_HAND_FULL);
		}
		m_actor.Show();
		m_actor.TurnOffCollider();
		bool num = GetController() != null && GetController().HasTag(GAME_TAG.MILL_SPELL_OVERRIDE);
		Spell spell = null;
		if (num)
		{
			int overrideMillAnimation = GetController().GetTag(GAME_TAG.MILL_SPELL_OVERRIDE);
			spell = m_actor.GetSpell((SpellType)overrideMillAnimation);
			if (spell == null)
			{
				Debug.LogError("No spell");
			}
		}
		bool num2 = num && spell != null;
		if (num2)
		{
			m_actor.Hide();
			ZoneDeck deck = (friendly ? ZoneMgr.Get().FindZoneOfType<ZoneDeck>(Player.Side.FRIENDLY) : ZoneMgr.Get().FindZoneOfType<ZoneDeck>(Player.Side.OPPOSING));
			Vector3 pos = base.gameObject.transform.position;
			pos.x = deck.gameObject.transform.position.x;
			pos.y = 1.2f;
			pos.z = deck.gameObject.transform.position.z;
			base.gameObject.transform.position = pos;
			Vector3 rot = base.gameObject.transform.rotation.eulerAngles;
			rot.x = 0f;
			rot.y = 0f;
			rot.z = 0f;
			base.gameObject.transform.rotation = Quaternion.Euler(rot);
			iTween.Stop(base.gameObject);
			spell.gameObject.transform.parent = null;
			SetDoNotSort(on: true);
			FsmBool fsmBool = spell.GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("isOpponent");
			if (fsmBool != null)
			{
				fsmBool.Value = false;
			}
			spell.SetSource(base.gameObject);
			SpellUtils.ActivateStateIfNecessary(spell, SpellStateType.ACTION);
		}
		else
		{
			Transform bone = Board.Get().FindBone(boneName);
			Vector3[] drawPath = new Vector3[3]
			{
				base.gameObject.transform.position,
				base.gameObject.transform.position + ABOVE_DECK_OFFSET,
				bone.position
			};
			Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
			moveArgs.Add("path", drawPath);
			moveArgs.Add("time", 1.5f);
			moveArgs.Add("easetype", iTween.EaseType.easeInSineOutExpo);
			iTween.MoveTo(base.gameObject, moveArgs);
			base.gameObject.transform.localEulerAngles = new Vector3(270f, 270f, 0f);
			Hashtable rotateArgs = iTweenManager.Get().GetTweenHashTable();
			rotateArgs.Add("rotation", new Vector3(0f, 0f, 357f));
			rotateArgs.Add("time", 1.35f);
			rotateArgs.Add("delay", 0.15f);
			iTween.RotateTo(base.gameObject, rotateArgs);
			Hashtable scaleArgs = iTweenManager.Get().GetTweenHashTable();
			scaleArgs.Add("scale", bone.localScale);
			scaleArgs.Add("time", 0.75f);
			scaleArgs.Add("delay", 0.15f);
			iTween.ScaleTo(base.gameObject, scaleArgs);
		}
		if (num2)
		{
			while (spell.IsActive())
			{
				yield return null;
			}
			SpellManager.Get().ReleaseSpell(spell);
		}
		else
		{
			while (iTween.Count(base.gameObject) > 0)
			{
				yield return null;
			}
		}
		m_actorReady = true;
		RefreshActor();
		Spell spell2 = m_actor.GetSpell(SpellType.HANDFULL);
		spell2.AddStateFinishedCallback(OnSpellStateFinished_DestroyActor);
		spell2.Activate();
		GameState.Get().ClearCardBeingDrawn(this);
		SetDoNotSort(on: false);
	}

	public void ActivateActorSpells_HandToPlay(Actor oldActor)
	{
		if (oldActor == null)
		{
			Debug.LogError($"{this}.ActivateActorSpells_HandToPlay() - oldActor=null");
			return;
		}
		if (m_cardDef == null)
		{
			Debug.LogError($"{this}.ActivateActorSpells_HandToPlay() - m_cardDef=null");
			return;
		}
		if (m_actor == null)
		{
			Debug.LogError($"{this}.ActivateActorSpells_HandToPlay() - m_actor=null");
			return;
		}
		DeactivateHandStateSpells(oldActor);
		SpellType outSpellType = m_cardDef.CardDef.DetermineSummonOutSpell_HandToPlay(this);
		ISpell outSpell = oldActor.GetSpell(outSpellType);
		bool standard;
		if (outSpell == null)
		{
			Debug.LogError($"{this}.ActivateActorSpells_HandToPlay() - outSpell=null outSpellType={outSpellType}");
			m_actorReady = true;
		}
		else if (GetBestSummonSpell(out standard) == null)
		{
			Debug.LogError($"{this}.ActivateActorSpells_HandToPlay() - inSpell=null standard={standard}");
		}
		else
		{
			m_inputEnabled = false;
			outSpell.SetSource(base.gameObject);
			ActivateSpell(outSpell, OnSpellFinished_HandToPlay_SummonOut, oldActor, OnSpellStateFinished_DestroyActor);
		}
	}

	private void OnSpellFinished_HandToPlay_SummonOut(Spell spell, object userData)
	{
		Actor oldActor = userData as Actor;
		Card.HandToPlaySummonOutStart?.Invoke(this);
		m_actor.Show();
		if (m_magneticPlayData != null)
		{
			SpellUtils.ActivateDeathIfNecessary(oldActor.GetSpellIfLoaded(SpellType.MAGNETIC_HAND_LINKED_RIGHT));
			ActivateActorSpell(SpellType.MAGNETIC_PLAY_LINKED_RIGHT);
			if (m_cardDef != null && (m_cardDef.CardDef.m_PlayEffectDef == null || string.IsNullOrEmpty(m_cardDef.CardDef.m_PlayEffectDef.m_SpellPath)))
			{
				StartCoroutine(ActivateMagneticPlaySpell());
			}
		}
		bool standard;
		ISpell inSpell = GetBestSummonSpell(out standard);
		if (inSpell == null)
		{
			Debug.LogErrorFormat("{0}.OnSpellFinished_HandToPlay_SummonOut() - inSpell=null standard={1}", this, standard);
			return;
		}
		Spell asSpell = inSpell as Spell;
		if (!standard)
		{
			if (spell != null)
			{
				asSpell.AddStateFinishedCallback(OnSpellStateFinished_ReleaseSpell);
			}
			SpellUtils.SetCustomSpellParent(inSpell, m_actor);
		}
		if (spell != null)
		{
			asSpell.AddFinishedCallback(OnSpellFinished_HandToPlay_SummonIn);
		}
		inSpell.Activate();
	}

	private IEnumerator ActivateMagneticPlaySpell()
	{
		yield return new WaitForSeconds(0.45f);
		if (m_magneticPlayData != null && !(m_magneticPlayData.m_playedCard == null) && !(m_magneticPlayData.m_targetMech == null))
		{
			MagneticPlaySpell magneticPlaySpell = GetActorSpell(SpellType.MAGNETIC_PLAY) as MagneticPlaySpell;
			if (!(magneticPlaySpell == null))
			{
				magneticPlaySpell.SetSource(m_magneticPlayData.m_playedCard.gameObject);
				magneticPlaySpell.AddTarget(m_magneticPlayData.m_targetMech.gameObject);
				SpellUtils.ActivateStateIfNecessary(magneticPlaySpell, SpellStateType.ACTION);
			}
		}
	}

	private void OnSpellFinished_HandToPlay_SummonIn(Spell spell, object userData)
	{
		Actor summonActor = GetActor();
		if (summonActor != null)
		{
			GameObject rootObject = summonActor.GetRootObject();
			if (rootObject != null)
			{
				rootObject.transform.localPosition = Vector3.zero;
				rootObject.transform.localRotation = Quaternion.identity;
				rootObject.transform.localScale = Vector3.one;
			}
			summonActor.Show();
		}
		m_actorReady = true;
		m_inputEnabled = true;
		ActivateStateSpells();
		RefreshActor();
		if (m_entity.IsControlledByFriendlySidePlayer() && !m_entity.GetRealTimeIsDormant())
		{
			if (m_entity.HasSpellPower() || m_entity.HasSpellPowerDouble())
			{
				ZoneMgr.Get().OnSpellPowerEntityEnteredPlay(m_entity.GetSpellPowerSchool());
			}
			if (m_entity.HasHealingDoesDamageHint())
			{
				ZoneMgr.Get().OnHealingDoesDamageEntityEnteredPlay();
			}
			if (m_entity.HasLifestealDoesDamageHint())
			{
				ZoneMgr.Get().OnLifestealDoesDamageEntityEnteredPlay();
			}
		}
		if (m_entity.HasWindfury())
		{
			ActivateActorSpell(SpellType.WINDFURY_BURST);
		}
		StartCoroutine(ActivateActorBattlecrySpell());
		BoardEvents boardEvents = BoardEvents.Get();
		if (boardEvents != null)
		{
			boardEvents.SummonedEvent(this);
		}
	}

	private bool ActivateActorSpells_HandToWeapon(Actor oldActor)
	{
		if (oldActor == null)
		{
			Debug.LogError($"{this}.ActivateActorSpells_HandToWeapon() - oldActor=null");
			return false;
		}
		if (m_actor == null)
		{
			Debug.LogError($"{this}.ActivateActorSpells_HandToWeapon() - m_actor=null");
			return false;
		}
		DeactivateHandStateSpells(oldActor);
		oldActor.SetActorState(ActorStateType.CARD_IDLE);
		SpellType outSpellType = SpellType.SUMMON_OUT_WEAPON;
		Spell outSpell = oldActor.GetSpell(outSpellType);
		if (outSpell == null)
		{
			Debug.LogError($"{this}.ActivateActorSpells_HandToWeapon() - outSpell=null outSpellType={outSpellType}");
			return false;
		}
		ISpell inSpell = m_customSummonSpell;
		if (inSpell == null)
		{
			SpellType inSpellType = (m_entity.IsControlledByFriendlySidePlayer() ? SpellType.SUMMON_IN_FRIENDLY : SpellType.SUMMON_IN_OPPONENT);
			inSpell = GetActorSpell(inSpellType);
			if (inSpell == null)
			{
				Debug.LogError($"{this}.ActivateActorSpells_HandToWeapon() - inSpell=null inSpellType={inSpellType}");
				return false;
			}
		}
		m_inputEnabled = false;
		ActivateSpell(outSpell, OnSpellFinished_HandToWeapon_SummonOut, inSpell, OnSpellStateFinished_DestroyActor);
		return true;
	}

	private void OnSpellFinished_HandToWeapon_SummonOut(Spell spell, object userData)
	{
		m_actor.Show();
		ISpell inSpell = m_customSummonSpell;
		if (inSpell == null)
		{
			inSpell = (ISpell)userData;
		}
		else
		{
			if (inSpell is Spell asSpell)
			{
				asSpell.AddStateFinishedCallback(OnSpellStateFinished_ReleaseSpell);
			}
			SpellUtils.SetCustomSpellParent(inSpell, m_actor);
		}
		ActivateSpell(inSpell, OnSpellFinished_StandardCardSummon);
	}

	private void DiscardCardBeingDrawn()
	{
		if (this == GameState.Get().GetOpponentCardBeingDrawn())
		{
			m_actorWaitingToBeReplaced.Destroy();
			m_actorWaitingToBeReplaced = null;
		}
		if (m_actor.IsShown())
		{
			ActivateDeathSpell(m_actor);
		}
		else
		{
			GameState.Get().ClearCardBeingDrawn(this);
		}
	}

	private void DoDiscardAnimation()
	{
		ZoneHand handZone = m_prevZone as ZoneHand;
		m_actor.SetBlockTextComponentUpdate(block: true);
		m_doNotSort = true;
		iTween.Stop(base.gameObject);
		float slideAmount = 3f;
		if (GetEntity().IsControlledByOpposingSidePlayer())
		{
			slideAmount = 0f - slideAmount;
		}
		iTween.MoveTo(position: new Vector3(base.transform.position.x, base.transform.position.y, base.transform.position.z + slideAmount), target: base.gameObject, time: 3f);
		Vector3 startingScale = base.transform.localScale;
		if (handZone != null)
		{
			startingScale = handZone.GetCardScale();
		}
		iTween.ScaleTo(base.gameObject, startingScale * 1.5f, 3f);
		StartCoroutine(ActivateGraveyardActorDeathSpellAfterDelay(1f, 4f));
	}

	private bool DoPlayToHandTransition(Actor oldActor, bool wasInGraveyard = false, bool useFastAnimations = false)
	{
		bool num = ActivateActorSpells_PlayToHand(oldActor, wasInGraveyard, useFastAnimations);
		if (num)
		{
			m_actor.Hide();
		}
		return num;
	}

	private bool ActivateActorSpells_PlayToHand(Actor oldActor, bool wasInGraveyard, bool useFastAnimations)
	{
		if (oldActor == null)
		{
			Debug.LogError($"{this}.ActivateActorSpells_PlayToHand() - oldActor=null");
			return false;
		}
		if (m_actor == null)
		{
			Debug.LogError($"{this}.ActivateActorSpells_PlayToHand() - m_actor=null");
			return false;
		}
		SpellType outSpellType = (useFastAnimations ? SpellType.BOUNCE_OUT_FAST : SpellType.BOUNCE_OUT);
		ISpell outSpell = oldActor.GetSpell(outSpellType);
		if (outSpell == null)
		{
			Debug.LogError($"{this}.ActivateActorSpells_PlayToHand() - outSpell=null outSpellType={outSpellType}");
			return false;
		}
		SpellType inSpellType = SpellType.BOUNCE_IN;
		if (m_actor.UseTechLevelManaGem())
		{
			inSpellType = SpellType.BOUNCE_IN_TECH_LEVEL;
		}
		else if (useFastAnimations)
		{
			inSpellType = SpellType.BOUNCE_IN_FAST;
		}
		ISpell inSpell = GetActorSpell(inSpellType);
		if (inSpell == null)
		{
			Debug.LogError($"{this}.ActivateActorSpells_PlayToHand() - inSpell=null inSpellType={inSpellType}");
			return false;
		}
		m_inputEnabled = false;
		outSpell.SetSource(base.gameObject);
		if (m_entity.IsControlledByFriendlySidePlayer())
		{
			ISpellCallbackHandler<Spell>.FinishedCallback finishedCallback = (wasInGraveyard ? new ISpellCallbackHandler<Spell>.FinishedCallback(OnSpellFinished_PlayToHand_SummonOut_FromGraveyard) : new ISpellCallbackHandler<Spell>.FinishedCallback(OnSpellFinished_PlayToHand_SummonOut));
			ISpellCallbackHandler<Spell>.StateFinishedCallback funcStartOutSpell = delegate(Spell spell, SpellStateType prevStateType, object userData)
			{
				if (prevStateType == SpellStateType.CANCEL)
				{
					ActivateSpell(outSpell, finishedCallback, inSpell, OnSpellStateFinished_DestroyActor);
				}
			};
			if (!CancelCustomSummonSpell(funcStartOutSpell))
			{
				ActivateSpell(outSpell, finishedCallback, inSpell, OnSpellStateFinished_DestroyActor);
			}
		}
		else
		{
			if (m_entity.IsControlledByOpposingSidePlayer())
			{
				Log.FaceDownCard.Print("Card.ActivateActorSpells_PlayToHand() - {0} - {1} on {2}", this, outSpellType, oldActor);
				Log.FaceDownCard.Print("Card.ActivateActorSpells_PlayToHand() - {0} - {1} on {2}", this, inSpellType, m_actor);
			}
			ISpellCallbackHandler<Spell>.FinishedCallback finishedCallback2 = (wasInGraveyard ? ((ISpellCallbackHandler<Spell>.FinishedCallback)delegate
			{
				ResumeLayoutForPlayZone();
			}) : null);
			ActivateSpell(outSpell, finishedCallback2, null, OnSpellStateFinished_PlayToHand_OldActor_SummonOut);
			ActivateSpell(inSpell, OnSpellFinished_PlayToHand_SummonIn);
		}
		return true;
	}

	private bool CancelCustomSummonSpell(ISpellCallbackHandler<Spell>.StateFinishedCallback callback)
	{
		if (m_customSummonSpell == null)
		{
			return false;
		}
		if (!m_customSummonSpell.HasUsableState(SpellStateType.CANCEL))
		{
			return false;
		}
		if (m_customSummonSpell.GetActiveState() == SpellStateType.NONE)
		{
			return false;
		}
		if (m_customSummonSpell.GetActiveState() == SpellStateType.CANCEL)
		{
			return false;
		}
		if (m_customSummonSpell is Spell spell)
		{
			spell.AddStateFinishedCallback(callback);
		}
		m_customSummonSpell.ActivateState(SpellStateType.CANCEL);
		return true;
	}

	private void OnSpellFinished_PlayToHand_SummonOut(Spell spell, object userData)
	{
		ISpell inSpell = (ISpell)userData;
		ActivateSpell(inSpell, OnSpellFinished_StandardCardSummon);
	}

	private void OnSpellFinished_PlayToHand_SummonOut_FromGraveyard(Spell spell, object userData)
	{
		OnSpellFinished_PlayToHand_SummonOut(spell, userData);
		ResumeLayoutForPlayZone();
	}

	private void ResumeLayoutForPlayZone()
	{
		Player.Side sideToUpdate = (m_playZoneBlockerSide.HasValue ? m_playZoneBlockerSide.Value : m_zone.m_Side);
		m_playZoneBlockerSide = null;
		ZonePlay zonePlay = ZoneMgr.Get().FindZoneOfType<ZonePlay>(sideToUpdate);
		zonePlay.RemoveLayoutBlocker();
		zonePlay.UpdateLayout();
	}

	private void OnSpellStateFinished_PlayToHand_OldActor_SummonOut(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (m_entity.IsControlledByOpposingSidePlayer())
		{
			Log.FaceDownCard.Print("Card.OnSpellStateFinished_PlayToHand_OldActor_SummonOut() - {0} stateType={1}", this, spell.GetActiveState());
		}
		OnSpellStateFinished_DestroyActor(spell, prevStateType, userData);
	}

	private void OnSpellFinished_PlayToHand_SummonIn(Spell spell, object userData)
	{
		if (m_entity.IsControlledByOpposingSidePlayer())
		{
			Log.FaceDownCard.Print("Card.OnSpellFinished_PlayToHand_SummonIn() - {0}", this);
		}
		OnSpellFinished_StandardCardSummon(spell, userData);
	}

	private IEnumerator DoHandToDeckTransition(Actor handActor)
	{
		m_doNotSort = true;
		DeactivateHandStateSpells();
		ZoneDeck deckZone = m_zone as ZoneDeck;
		if (deckZone == null)
		{
			yield break;
		}
		ZoneHand handZone = m_prevZone as ZoneHand;
		if (!(handZone == null))
		{
			int numPreviousHandToDeckTransition = deckZone.GetDefaultHandToDeckAnimationCount();
			deckZone.NotifyCardAnimationStart();
			deckZone.IncrementDefaultHandToDeckAnimationCount();
			deckZone.AddLayoutBlocker();
			if (!m_entity.IsTradeable() || !m_entity.HasTag(GAME_TAG.IS_USING_TRADE_OPTION))
			{
				float slideAmount = (handZone.GetController().IsFriendlySide() ? 3f : (-3f));
				iTween.MoveTo(position: new Vector3(base.transform.position.x, base.transform.position.y, handZone.transform.position.z + slideAmount), target: base.gameObject, time: 1.75f);
				iTween.ScaleTo(base.gameObject, handZone.GetCardScale() * 1.5f, 1.75f);
				float staggerSeconds = 0.3f * (float)numPreviousHandToDeckTransition;
				yield return new WaitForSeconds(1.85f + staggerSeconds);
			}
			else
			{
				yield return new WaitForSeconds(0.1f);
			}
			yield return AnimatePlayToDeck(base.gameObject, deckZone, !handZone.GetController().IsFriendlySide());
			handActor.Destroy();
			m_actorReady = true;
			m_doNotSort = false;
			deckZone.RemoveLayoutBlocker();
			deckZone.UpdateLayout();
			deckZone.DecrementDefaultHandToDeckAnimationCount();
			deckZone.NotifyCardAnimationFinish();
		}
	}

	private void DoPlayToDeckTransition(Actor playActor)
	{
		m_doNotSort = true;
		m_actor.Hide();
		StartCoroutine(AnimatePlayToDeck(playActor));
	}

	private IEnumerator AnimatePlayToDeck(Actor playActor)
	{
		Actor handActor = null;
		bool loadingActor = true;
		PrefabCallback<GameObject> actorLoadedCallback = delegate(AssetReference assetRef, GameObject go, object callbackData)
		{
			loadingActor = false;
			if (go == null)
			{
				Error.AddDevFatal("Card.AnimatePlayToGraveyardToDeck() - failed to load {0}", assetRef);
			}
			else
			{
				handActor = go.GetComponent<Actor>();
				if (handActor == null)
				{
					Error.AddDevFatal("Card.AnimatePlayToGraveyardToDeck() - instance of {0} has no Actor component", base.name);
				}
			}
		};
		AssetReference assetReference = ActorNames.GetHandActor(m_entity);
		if (!AssetLoader.Get().InstantiatePrefab(assetReference, actorLoadedCallback, null, AssetLoadingOptions.IgnorePrefabPosition))
		{
			actorLoadedCallback(assetReference, null, null);
		}
		while (loadingActor)
		{
			yield return null;
		}
		if (handActor == null)
		{
			playActor.Destroy();
			yield break;
		}
		handActor.SetEntity(m_entity);
		handActor.SetCardDef(m_cardDef);
		handActor.UpdateAllComponents();
		handActor.transform.parent = playActor.GetCard().transform;
		TransformUtil.Identity(handActor);
		handActor.Hide();
		SpellType outSpellType = SpellType.SUMMON_OUT;
		Spell outSpell = playActor.GetSpell(outSpellType);
		if (outSpell == null)
		{
			Error.AddDevFatal("{0}.AnimatePlayToGraveyardToDeck() - outSpell=null outSpellType={1}", this, outSpellType);
			yield break;
		}
		SpellType inSpellType = SpellType.SUMMON_IN;
		Spell inSpell = handActor.GetSpell(inSpellType);
		if (inSpell == null)
		{
			Error.AddDevFatal("{0}.AnimatePlayToGraveyardToDeck() - inSpell=null inSpellType={1}", this, inSpellType);
			yield break;
		}
		bool waitForSpells = true;
		ISpellCallbackHandler<Spell>.FinishedCallback inFinishCallback = delegate
		{
			waitForSpells = false;
		};
		ISpellCallbackHandler<Spell>.StateFinishedCallback outStateFinishCallback = delegate(Spell spell, SpellStateType prevStateType, object userData)
		{
			if (spell.GetActiveState() == SpellStateType.NONE)
			{
				playActor.Destroy();
			}
		};
		ISpellCallbackHandler<Spell>.FinishedCallback outFinishCallback = delegate
		{
			inSpell.Activate();
			ResumeLayoutForPlayZone();
		};
		inSpell.AddFinishedCallback(inFinishCallback);
		outSpell.AddFinishedCallback(outFinishCallback);
		outSpell.AddStateFinishedCallback(outStateFinishCallback);
		PrepareForDeathAnimation(playActor);
		outSpell.Activate();
		while (waitForSpells)
		{
			yield return 0;
		}
		ZoneDeck deckZone = (ZoneDeck)m_zone;
		deckZone.NotifyCardAnimationStart();
		yield return StartCoroutine(AnimatePlayToDeck(base.gameObject, deckZone));
		handActor.Destroy();
		m_actorReady = true;
		m_doNotSort = false;
		deckZone.RemoveLayoutBlocker();
		deckZone.UpdateLayout();
		deckZone.NotifyCardAnimationFinish();
	}

	public IEnumerator AnimatePlayToDeck(GameObject mover, ZoneDeck deckZone, bool hideBackSide = false, float timeScale = 1f)
	{
		SoundManager.Get().LoadAndPlay("MinionToDeck_transition.prefab:8063f1b133f28e34aaeade8fcabe250c");
		Vector3 finalPos = deckZone.GetThicknessForLayout().GetMeshRenderer().bounds.center + IN_DECK_OFFSET;
		if (m_entity != null && m_entity.IsMercenary())
		{
			finalPos -= IN_DECK_OFFSET;
		}
		Vector3 intermedPos = finalPos + ABOVE_DECK_OFFSET;
		Vector3 intermedRot = new Vector3(0f, IN_DECK_ANGLES.y, 0f);
		Vector3 finalRot = IN_DECK_ANGLES;
		Vector3 finalScale = IN_DECK_SCALE;
		float finalRotSec = 0.3f;
		if (hideBackSide)
		{
			intermedRot.y = (finalRot.y = 0f - IN_DECK_ANGLES.y);
			finalRotSec = 0.5f;
		}
		float timeFactor = 1f;
		if (timeScale > 0f)
		{
			timeFactor *= 1f / timeScale;
		}
		Actor moverActor = mover.GetComponent<Actor>();
		Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
		moveArgs.Add("position", intermedPos);
		moveArgs.Add("delay", 0f * timeFactor);
		moveArgs.Add("time", 0.7f * timeFactor);
		moveArgs.Add("easetype", iTween.EaseType.easeInOutCubic);
		iTween.MoveTo(mover, moveArgs);
		Hashtable rotateArgs = iTweenManager.Get().GetTweenHashTable();
		rotateArgs.Add("rotation", intermedRot);
		rotateArgs.Add("delay", 0f * timeFactor);
		rotateArgs.Add("time", 0.2f * timeFactor);
		rotateArgs.Add("easetype", iTween.EaseType.easeInOutCubic);
		iTween.RotateTo(mover, rotateArgs);
		moveArgs = iTweenManager.Get().GetTweenHashTable();
		moveArgs.Add("position", finalPos);
		moveArgs.Add("delay", 0.7f * timeFactor);
		moveArgs.Add("time", 0.7f * timeFactor);
		moveArgs.Add("easetype", iTween.EaseType.easeOutCubic);
		iTween.MoveTo(mover, moveArgs);
		Hashtable scaleArgs = iTweenManager.Get().GetTweenHashTable();
		scaleArgs.Add("scale", finalScale);
		scaleArgs.Add("delay", 0.7f * timeFactor);
		scaleArgs.Add("time", 0.6f * timeFactor);
		scaleArgs.Add("easetype", iTween.EaseType.easeInCubic);
		iTween.ScaleTo(mover, scaleArgs);
		if (base.gameObject != null && moverActor != null)
		{
			Hashtable rotateArgs2 = iTweenManager.Get().GetTweenHashTable();
			rotateArgs2.Add("rotation", finalRot);
			rotateArgs2.Add("delay", 0.2f * timeFactor);
			rotateArgs2.Add("time", finalRotSec * timeFactor);
			rotateArgs2.Add("easetype", iTween.EaseType.easeOutCubic);
			rotateArgs2.Add("oncomplete", "OnCardRotateIntoDeckComplete");
			rotateArgs2.Add("oncompleteparams", moverActor);
			rotateArgs2.Add("oncompletetarget", base.gameObject);
			iTween.RotateTo(mover, rotateArgs2);
		}
		else
		{
			Hashtable rotateArgs3 = iTweenManager.Get().GetTweenHashTable();
			rotateArgs3.Add("rotation", finalRot);
			rotateArgs3.Add("delay", 0.2f * timeFactor);
			rotateArgs3.Add("time", finalRotSec * timeFactor);
			rotateArgs3.Add("easetype", iTween.EaseType.easeOutCubic);
			iTween.RotateTo(mover, rotateArgs3);
		}
		while (iTween.HasTween(mover))
		{
			yield return 0;
		}
	}

	private void OnCardRotateIntoDeckComplete(Actor cardActor)
	{
		if (base.gameObject != null && cardActor != null)
		{
			if (cardActor.m_eliteObject != null)
			{
				cardActor.m_eliteObject.SetActive(value: false);
			}
			if (cardActor.m_portraitMesh != null)
			{
				cardActor.m_portraitMesh.SetActive(value: false);
			}
			if (cardActor.m_manaObject != null)
			{
				cardActor.m_manaObject.SetActive(value: false);
			}
			if (cardActor.m_costTextMesh != null)
			{
				cardActor.m_costTextMesh.Hide();
			}
		}
	}

	public void SetSecretTriggered(bool set)
	{
		m_secretTriggered = set;
	}

	public bool WasSecretTriggered()
	{
		return m_secretTriggered;
	}

	public bool CanShowSecretTrigger()
	{
		if (!UniversalInputManager.UsePhoneUI)
		{
			return true;
		}
		if (m_zone.IsOnlyCard(this))
		{
			return true;
		}
		return false;
	}

	public void ShowSecretTrigger()
	{
		m_actor.GetComponent<Spell>().ActivateState(SpellStateType.ACTION);
	}

	private bool CanShowSecretZoneCard()
	{
		if (!UniversalInputManager.UsePhoneUI)
		{
			return true;
		}
		ZoneSecret secretZone = m_zone as ZoneSecret;
		if (secretZone == null)
		{
			return false;
		}
		if (m_entity != null && m_entity.IsQuest())
		{
			return true;
		}
		if (m_entity != null && m_entity.IsQuestline())
		{
			return true;
		}
		if (m_entity != null && m_entity.IsPuzzle())
		{
			return true;
		}
		if (m_entity != null && m_entity.IsRulebook())
		{
			return true;
		}
		if (m_entity != null && m_entity.IsSigil())
		{
			return true;
		}
		if (m_entity != null && m_entity.IsObjective())
		{
			return true;
		}
		if (secretZone.GetSecretCards().IndexOf(this) == 0)
		{
			return true;
		}
		if (secretZone.GetSideQuestCards().IndexOf(this) == 0)
		{
			return true;
		}
		return false;
	}

	public void ShowSecretQuestBirth()
	{
		Spell spell = m_actor.GetComponent<Spell>();
		if (spell == null)
		{
			return;
		}
		if (!CanShowSecretZoneCard())
		{
			ISpellCallbackHandler<Spell>.StateFinishedCallback onStateFinished = delegate(Spell thisSpell, SpellStateType prevStateType, object userData)
			{
				if (thisSpell.GetActiveState() == SpellStateType.NONE && !CanShowSecretZoneCard())
				{
					HideCard();
				}
			};
			spell.AddStateFinishedCallback(onStateFinished);
		}
		spell.ActivateState(SpellStateType.BIRTH);
	}

	public bool CanShowSecretDeath()
	{
		if (!UniversalInputManager.UsePhoneUI)
		{
			return true;
		}
		if (m_prevZone.GetCardCount() == 0)
		{
			return true;
		}
		return false;
	}

	public void ShowSecretDeath(Actor oldActor)
	{
		if (oldActor == null)
		{
			Log.All.PrintError("Card::ShowSecretDeath oldActor is null for card {0}!", this);
			return;
		}
		Spell spell = oldActor.GetComponent<Spell>();
		if (m_secretTriggered)
		{
			m_secretTriggered = false;
			if (spell.GetActiveState() == SpellStateType.NONE)
			{
				oldActor.Destroy();
			}
			else
			{
				spell.AddStateFinishedCallback(OnSpellStateFinished_DestroyActor);
			}
			return;
		}
		spell.AddStateFinishedCallback(OnSpellStateFinished_DestroyActor);
		spell.ActivateState(SpellStateType.ACTION);
		oldActor.transform.parent = null;
		m_doNotSort = true;
		if (!UniversalInputManager.UsePhoneUI)
		{
			iTween.Stop(base.gameObject);
			m_actor.Hide();
			StartCoroutine(WaitAndThenShowDestroyedSecret());
		}
	}

	private IEnumerator WaitAndThenShowDestroyedSecret()
	{
		yield return new WaitForSeconds(0.5f);
		float slideAmount = 2f;
		if (GetEntity().IsControlledByOpposingSidePlayer())
		{
			slideAmount = 0f - slideAmount;
		}
		Vector3 newPos = new Vector3(base.transform.position.x, base.transform.position.y + 1f, base.transform.position.z + slideAmount);
		m_actor.Show();
		iTween.MoveTo(base.gameObject, newPos, 3f);
		base.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
		base.transform.localEulerAngles = new Vector3(0f, 0f, 357f);
		iTween.ScaleTo(base.gameObject, new Vector3(1.25f, 0.2f, 1.25f), 3f);
		StartCoroutine(ActivateGraveyardActorDeathSpellAfterDelay(1f, 4f));
	}

	private IEnumerator SwitchSecretSides()
	{
		m_doNotSort = true;
		Actor newActor = null;
		bool loadingActor = true;
		PrefabCallback<GameObject> actorLoadedCallback = delegate(AssetReference assetRef, GameObject go, object callbackData)
		{
			loadingActor = false;
			if (go == null)
			{
				Error.AddDevFatal("Card.SwitchSecretSides() - failed to load {0}", assetRef);
			}
			else
			{
				newActor = go.GetComponent<Actor>();
				if (newActor == null)
				{
					Error.AddDevFatal("Card.SwitchSecretSides() - instance of {0} has no Actor component", base.name);
				}
			}
		};
		if (!AssetLoader.Get().InstantiatePrefab(m_actorPath, actorLoadedCallback, null, AssetLoadingOptions.IgnorePrefabPosition))
		{
			actorLoadedCallback(m_actorPath, null, null);
		}
		while (loadingActor)
		{
			yield return null;
		}
		if ((bool)newActor)
		{
			Actor oldActor = m_actor;
			m_actor = newActor;
			m_actor.SetEntity(m_entity);
			m_actor.SetCard(this);
			m_actor.SetCardDef(m_cardDef);
			m_actor.UpdateAllComponents();
			m_actor.transform.parent = oldActor.transform.parent;
			TransformUtil.Identity(m_actor);
			m_actor.Hide();
			if (!CanShowSecretDeath())
			{
				oldActor.Destroy();
			}
			else
			{
				oldActor.transform.parent = base.transform.parent;
				m_transitionStyle = ZoneTransitionStyle.INSTANT;
				bool oldActorFinished = false;
				ISpellCallbackHandler<Spell>.FinishedCallback onOldSpellFinished = delegate
				{
					oldActorFinished = true;
				};
				ISpellCallbackHandler<Spell>.StateFinishedCallback onOldSpellStateFinished = delegate(Spell spell, SpellStateType prevStateType, object userData)
				{
					if (spell.GetActiveState() == SpellStateType.NONE)
					{
						oldActor.Destroy();
					}
				};
				Spell component = oldActor.GetComponent<Spell>();
				component.AddFinishedCallback(onOldSpellFinished);
				component.AddStateFinishedCallback(onOldSpellStateFinished);
				component.ActivateState(SpellStateType.ACTION);
				while (!oldActorFinished)
				{
					yield return null;
				}
			}
			m_shown = true;
			m_actor.Show();
			ShowSecretQuestBirth();
		}
		m_actorReady = true;
		m_doNotSort = false;
		m_zone.UpdateLayout();
		ActivateStateSpells();
	}

	private bool ShouldCardDrawWaitForTurnStartSpells()
	{
		SpellController spellController = TurnStartManager.Get().GetSpellController();
		if (spellController == null)
		{
			return false;
		}
		if (spellController.IsSource(this))
		{
			return true;
		}
		if (spellController.IsTarget(this))
		{
			return true;
		}
		return false;
	}

	private IEnumerator WaitForCardDrawBlockingTurnStartSpells()
	{
		while (ShouldCardDrawWaitForTurnStartSpells())
		{
			yield return null;
		}
	}

	private PowerTask GetPowerTaskToBlockCardDraw()
	{
		if (m_latestZoneChange == null)
		{
			return null;
		}
		PowerTaskList cardDrawTaskList = m_latestZoneChange.GetParentList().GetTaskList();
		if (cardDrawTaskList == null)
		{
			return null;
		}
		if (cardDrawTaskList.IsEndOfBlock() && cardDrawTaskList.IsComplete())
		{
			return null;
		}
		PowerTask furthestBlockingTask = null;
		PowerTaskList currentTaskList = GameState.Get().GetPowerProcessor().GetCurrentTaskList();
		if (currentTaskList != null && currentTaskList.IsDescendantOfBlock(cardDrawTaskList))
		{
			DoesTaskListBlockCardDraw(currentTaskList, out furthestBlockingTask);
		}
		foreach (PowerTaskList candidateTaskList in GameState.Get().GetPowerProcessor().GetPowerQueue())
		{
			if (candidateTaskList.IsDescendantOfBlock(cardDrawTaskList) && DoesTaskListBlockCardDraw(candidateTaskList, out var blockingTask))
			{
				if (!CanPowerTaskListBlockCardDraw(candidateTaskList))
				{
					break;
				}
				furthestBlockingTask = blockingTask;
			}
		}
		return furthestBlockingTask;
	}

	private bool CanPowerTaskListBlockCardDraw(PowerTaskList blockingPowerTaskList)
	{
		PowerTaskList currentTaskList = GameState.Get().GetPowerProcessor().GetCurrentTaskList();
		if (currentTaskList != null && (currentTaskList.HasCardDraw() || currentTaskList.HasCardMill() || currentTaskList.HasFatigue()))
		{
			return false;
		}
		foreach (PowerTaskList taskList in GameState.Get().GetPowerProcessor().GetPowerQueue())
		{
			if (taskList == blockingPowerTaskList)
			{
				break;
			}
			if (taskList.HasCardDraw() || taskList.HasCardMill() || taskList.HasFatigue())
			{
				return false;
			}
		}
		return true;
	}

	private bool DoesTaskListBlockCardDraw(PowerTaskList taskList, out PowerTask blockingTask)
	{
		blockingTask = GetPowerTaskBlockingCardDraw(taskList);
		if (blockingTask == null)
		{
			return false;
		}
		foreach (PowerTask task in taskList.GetTaskList())
		{
			if (task == blockingTask)
			{
				break;
			}
			if (task.IsCardDraw() || task.IsCardMill() || task.IsFatigue())
			{
				blockingTask = null;
				return false;
			}
		}
		return true;
	}

	private PowerTask GetPowerTaskBlockingCardDraw(PowerTaskList taskList)
	{
		if (taskList == null)
		{
			return null;
		}
		if (taskList.IsComplete())
		{
			return null;
		}
		Network.HistBlockStart blockStart = taskList.GetBlockStart();
		if (blockStart == null)
		{
			return null;
		}
		if (blockStart.BlockType != HistoryBlock.Type.POWER && blockStart.BlockType != HistoryBlock.Type.TRIGGER)
		{
			return null;
		}
		int entityId = m_entity.GetEntityId();
		List<PowerTask> tasks = taskList.GetTaskList();
		for (int i = 0; i < tasks.Count; i++)
		{
			PowerTask task = tasks[i];
			if (task.IsCompleted())
			{
				continue;
			}
			Network.PowerHistory power = task.GetPower();
			int zoneTag = 0;
			switch (power.Type)
			{
			case Network.PowerType.META_DATA:
			{
				Network.HistMetaData metaData = (Network.HistMetaData)power;
				if (metaData.MetaType == HistoryMeta.Type.HOLD_DRAWN_CARD && metaData.Info.Count == 1 && metaData.Info[0] == entityId)
				{
					return task;
				}
				break;
			}
			case Network.PowerType.SHOW_ENTITY:
			{
				Network.HistShowEntity showEntity = (Network.HistShowEntity)power;
				if (showEntity.Entity.ID == entityId)
				{
					Network.Entity.Tag tag = showEntity.Entity.Tags.Find((Network.Entity.Tag currTag) => currTag.Name == 49);
					if (tag != null)
					{
						zoneTag = tag.Value;
					}
				}
				break;
			}
			case Network.PowerType.HIDE_ENTITY:
			{
				Network.HistHideEntity hideEntity = (Network.HistHideEntity)power;
				if (hideEntity.Entity == entityId)
				{
					zoneTag = hideEntity.Zone;
				}
				break;
			}
			case Network.PowerType.TAG_CHANGE:
			{
				Network.HistTagChange tagChange = (Network.HistTagChange)power;
				if (tagChange.Entity == entityId && tagChange.Tag == 49)
				{
					zoneTag = tagChange.Value;
				}
				break;
			}
			case Network.PowerType.CHANGE_ENTITY:
				if (((Network.HistChangeEntity)power).Entity.ID == entityId)
				{
					return task;
				}
				break;
			}
			if (zoneTag != 0 && zoneTag != 3)
			{
				return task;
			}
		}
		return null;
	}

	private void CutoffFriendlyCardDraw()
	{
		if (!m_actorReady)
		{
			if (m_actorWaitingToBeReplaced != null)
			{
				m_actorWaitingToBeReplaced.Destroy();
				m_actorWaitingToBeReplaced = null;
			}
			m_actor.Show();
			m_actor.TurnOffCollider();
			m_doNotSort = false;
			m_actorReady = true;
			ActivateStateSpells();
			RefreshActor();
			GameState.Get().ClearCardBeingDrawn(this);
			m_zone.UpdateLayout();
		}
	}

	private IEnumerator WaitAndPrepareForDeathAnimation(Actor dyingActor)
	{
		yield return new WaitForSeconds(m_keywordDeathDelaySec);
		PrepareForDeathAnimation(dyingActor);
	}

	private void PrepareForDeathAnimation(Actor dyingActor)
	{
		dyingActor.ToggleCollider(enabled: false);
		dyingActor.ToggleForceIdle(bOn: true);
		dyingActor.SetActorState(ActorStateType.CARD_IDLE);
		dyingActor.DoCardDeathVisuals();
		DeactivateCustomKeywordEffect();
	}

	private IEnumerator ActivateGraveyardActorDeathSpellAfterDelay(float predelay, float postdelay, ActivateGraveyardActorDeathSpellAfterDelayCallback finishedCallback = null)
	{
		m_actor.DoCardDeathVisuals();
		Spell chosenSpell = GetBestDeathSpell() as Spell;
		if (!(chosenSpell == null))
		{
			if (chosenSpell.DoesBlockServerEvents())
			{
				GameState.Get().AddServerBlockingSpell(chosenSpell);
			}
			yield return new WaitForSeconds(predelay);
			ActivateSpell(chosenSpell, null);
			CleanUpCustomSpell(chosenSpell, ref m_customDiscardSpell);
			CleanUpCustomSpell(chosenSpell, ref m_customDiscardSpellOverride);
			yield return new WaitForSeconds(postdelay);
			m_doNotSort = false;
			m_actor.SetBlockTextComponentUpdate(block: false);
			finishedCallback?.Invoke();
		}
	}

	private bool HandlePlayActorDeath(Actor oldActor)
	{
		bool transitionHandled = false;
		if (!m_cardDef.CardDef.m_SuppressDeathrattleDeath && m_entity.HasDeathrattle() && !m_entity.IsDeathrattleDisabled())
		{
			ActivateActorSpell(oldActor, SpellType.DEATHRATTLE_DEATH);
		}
		if (!m_cardDef.CardDef.m_SuppressDeathrattleDeath && m_entity.HasTag(GAME_TAG.REBORN))
		{
			ActivateActorSpell(oldActor, SpellType.REBORN_DEATH);
		}
		if (m_suppressDeathEffects)
		{
			if ((bool)oldActor)
			{
				oldActor.Destroy();
			}
			if (IsShown())
			{
				ShowImpl();
			}
			else
			{
				HideImpl();
			}
			transitionHandled = true;
			m_actorReady = true;
		}
		else
		{
			if (!m_suppressKeywordDeaths)
			{
				StartCoroutine(WaitAndPrepareForDeathAnimation(oldActor));
			}
			if (ActivateDeathSpell(oldActor) != null)
			{
				m_actor.Hide();
				transitionHandled = true;
				m_actorReady = true;
			}
		}
		return transitionHandled;
	}

	private bool DoesCardReturnFromGraveyard()
	{
		foreach (PowerTaskList taskList in GameState.Get().GetPowerProcessor().GetPowerQueue())
		{
			if (DoesTaskListReturnCardFromGraveyard(taskList))
			{
				Log.Gameplay.PrintInfo("Found the task for returning entity {0} from graveyard!", m_entity);
				return true;
			}
		}
		return false;
	}

	private bool DoesTaskListReturnCardFromGraveyard(PowerTaskList taskList)
	{
		if (!taskList.IsTriggerBlock())
		{
			return false;
		}
		foreach (PowerTask task in taskList.GetTaskList())
		{
			Network.PowerHistory power = task.GetPower();
			if (power.Type != Network.PowerType.TAG_CHANGE)
			{
				continue;
			}
			Network.HistTagChange tagChange = power as Network.HistTagChange;
			if (tagChange.Tag == 49 && tagChange.Entity == m_entity.GetEntityId())
			{
				if (tagChange.Value == 6)
				{
					return false;
				}
				return true;
			}
		}
		return false;
	}

	private int GetCardFutureController()
	{
		foreach (PowerTaskList taskList in GameState.Get().GetPowerProcessor().GetPowerQueue())
		{
			int controllerID = GetCardFutureControllerFromTaskList(taskList);
			if (controllerID != m_entity.GetControllerId())
			{
				return controllerID;
			}
		}
		return m_entity.GetControllerId();
	}

	private int GetCardFutureControllerFromTaskList(PowerTaskList taskList)
	{
		foreach (PowerTask task in taskList.GetTaskList())
		{
			Network.PowerHistory power = task.GetPower();
			if (power.Type == Network.PowerType.TAG_CHANGE)
			{
				Network.HistTagChange tagChange = power as Network.HistTagChange;
				if (tagChange.Tag == 50 && tagChange.Entity == m_entity.GetEntityId())
				{
					return tagChange.Value;
				}
			}
		}
		return m_entity.GetControllerId();
	}

	public void SetDelayBeforeHideInNullZoneVisuals(float delay)
	{
		m_delayBeforeHideInNullZoneVisuals = delay;
	}

	private void DoNullZoneVisuals()
	{
		StartCoroutine(DoNullZoneVisualsWithTiming());
	}

	private IEnumerator DoNullZoneVisualsWithTiming()
	{
		if (m_delayBeforeHideInNullZoneVisuals > 0f)
		{
			yield return new WaitForSeconds(m_delayBeforeHideInNullZoneVisuals);
		}
		Spell nullZoneSpell = GetBestNullZoneSpell();
		if (nullZoneSpell != null)
		{
			nullZoneSpell.Activate();
			while (nullZoneSpell.GetActiveState() != 0)
			{
				yield return null;
			}
		}
		if (m_actor != null)
		{
			m_actor.DeactivateAllSpells();
		}
		HideCard();
	}

	public IEnumerator WaitAndForgeCard()
	{
		Actor actor = GetActor();
		if (actor == null)
		{
			yield break;
		}
		Spell spell = actor.GetSpell(SpellType.FORGEABLE_HAMMER_IMPACT);
		if (!(spell == null))
		{
			bool isOpponent = false;
			if (m_entity != null && (m_entity.IsControlledByOpposingSidePlayer() || GameMgr.Get().IsSpectator()))
			{
				ZoneDeck deck = (m_entity.IsControlledByOpposingSidePlayer() ? ZoneMgr.Get().FindZoneOfType<ZoneDeck>(Player.Side.OPPOSING) : ZoneMgr.Get().FindZoneOfType<ZoneDeck>(Player.Side.FRIENDLY));
				Vector3 pos = base.gameObject.transform.position;
				pos.x = deck.gameObject.transform.position.x;
				pos.y = 1.2f;
				pos.z = deck.gameObject.transform.position.z;
				base.gameObject.transform.position = pos;
				Vector3 rot = base.gameObject.transform.rotation.eulerAngles;
				rot.y = 0f;
				rot.z = 0f;
				base.gameObject.transform.rotation = Quaternion.Euler(rot);
				isOpponent = true;
				iTween.Stop(base.gameObject);
			}
			spell.gameObject.transform.parent = null;
			SetDoNotSort(on: true);
			FsmBool fsmBool = spell.GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("isOpponent");
			if (fsmBool != null)
			{
				fsmBool.Value = isOpponent;
			}
			SpellUtils.ActivateStateIfNecessary(spell, SpellStateType.ACTION);
			yield return new WaitForSeconds(1f);
			SetDoNotSort(on: false);
			ZoneHand zone = GetZone() as ZoneHand;
			if (zone != null)
			{
				zone.ClearDeckActionEntity();
				zone.UpdateLayout(null, forced: true);
			}
			yield return new WaitForSeconds(1f);
			SpellManager.Get().ReleaseSpell(spell);
		}
	}

	private Spell GetBestNullZoneSpell()
	{
		if (m_entity.HasTag(GAME_TAG.IS_USING_PASS_OPTION))
		{
			return m_actor.GetSpell(SpellType.ENTER_PORTAL);
		}
		if (m_entity.HasTag(GAME_TAG.GHOSTLY) && GetControllerSide() == Player.Side.FRIENDLY && m_prevZone is ZoneHand && m_actor != null)
		{
			return m_actor.GetSpell(SpellType.GHOSTLY_DEATH);
		}
		if (m_entity.IsSpell() && m_prevZone is ZoneHand && m_actor != null && m_zone is ZoneGraveyard)
		{
			return m_actor.GetSpell(SpellType.POWER_UP);
		}
		return null;
	}

	public void SetDrawTimeScale(float scale)
	{
		m_drawTimeScale = scale;
	}

	public bool IsInDeckActionArea()
	{
		return IsInDeckActionArea(base.gameObject.transform.position);
	}

	public bool IsInPassActionArea(Vector3 checkPosition)
	{
		if (GameState.Get() == null)
		{
			return false;
		}
		GameEntity gameEntity = GameState.Get().GetGameEntity();
		if (gameEntity == null || !(gameEntity is TB_BaconShop))
		{
			return false;
		}
		return ((TB_BaconShop)gameEntity).IsInPassActionArea(checkPosition);
	}

	public bool IsInDeckActionArea(Vector3 checkPosition)
	{
		if (m_entity != null && m_entity.IsPassable())
		{
			return IsInPassActionArea(checkPosition);
		}
		if (ZoneMgr.Get() == null)
		{
			return false;
		}
		Collider deckActionArea = Board.Get().FindCollider("DeckActionArea");
		if (deckActionArea == null)
		{
			return false;
		}
		return deckActionArea.bounds.Contains(checkPosition);
	}

	public bool HasEnoughManaToDeckAction()
	{
		if (m_entity == null || m_entity.GetController() == null)
		{
			return false;
		}
		int num = Math.Max(m_entity.GetTag(GAME_TAG.DECK_ACTION_COST), 0);
		int currentMana = m_entity.GetController().GetNumAvailableResources();
		if (num > currentMana)
		{
			return false;
		}
		return true;
	}

	private void ShowTradeableHover()
	{
		SpellUtils.ActivateBirthIfNecessary(m_actor.GetSpell(SpellType.TRADEABLE_HOVER));
	}

	public void HideTradeableHover()
	{
		Spell spell = m_actor.GetSpell(SpellType.TRADEABLE_HOVER);
		if (!(spell == null) && spell.GetActiveState() != SpellStateType.DEATH)
		{
			SpellUtils.ActivateCancelIfNecessary(spell);
		}
	}

	public void KillTradeableHover()
	{
		if (!(m_actor == null))
		{
			Spell spell = m_actor.GetSpell(SpellType.TRADEABLE_HOVER);
			if (!(spell == null) && spell.GetActiveState() != SpellStateType.CANCEL)
			{
				SpellUtils.ActivateDeathIfNecessary(spell);
			}
		}
	}

	private void ShowForgeableHover()
	{
		SpellUtils.ActivateBirthIfNecessary(m_actor.GetSpell(SpellType.FORGEABLE_HOVER));
	}

	public void HideForgeableHover()
	{
		Spell spell = m_actor.GetSpell(SpellType.FORGEABLE_HOVER);
		if (!(spell == null) && spell.GetActiveState() != SpellStateType.DEATH)
		{
			SpellUtils.ActivateCancelIfNecessary(spell);
		}
	}

	public void KillForgeableHover()
	{
		if (!(m_actor == null))
		{
			Spell spell = m_actor.GetSpell(SpellType.FORGEABLE_HOVER);
			if (!(spell == null) && spell.GetActiveState() != SpellStateType.CANCEL)
			{
				SpellUtils.ActivateDeathIfNecessary(spell);
			}
		}
	}

	private void ShowPassableHover()
	{
		SpellUtils.ActivateBirthIfNecessary(m_actor.GetSpell(SpellType.PASSABLE_HOVER));
	}

	public void HidePassableHover()
	{
		Spell spell = m_actor.GetSpell(SpellType.PASSABLE_HOVER);
		if (!(spell == null) && spell.GetActiveState() != SpellStateType.DEATH)
		{
			SpellUtils.ActivateCancelIfNecessary(spell);
		}
	}

	public void KillPassableHover()
	{
		if (!(m_actor == null))
		{
			Spell spell = m_actor.GetSpell(SpellType.PASSABLE_HOVER);
			if (!(spell == null) && spell.GetActiveState() != SpellStateType.CANCEL)
			{
				SpellUtils.ActivateDeathIfNecessary(spell);
			}
		}
	}

	public bool HasEnoughManaToPlay()
	{
		if (m_entity == null || m_entity.GetController() == null)
		{
			return false;
		}
		int cost = m_entity.GetCost();
		int currentMana = m_entity.GetController().GetNumAvailableResources();
		if (cost > currentMana)
		{
			return false;
		}
		return true;
	}

	public int GetNumberOfMinionsInPlay()
	{
		ZoneMgr zoneMgr = ZoneMgr.Get();
		if (zoneMgr == null)
		{
			return 0;
		}
		ZonePlay zonePlay = zoneMgr.FindZoneOfType<ZonePlay>(Player.Side.FRIENDLY);
		if (zonePlay == null)
		{
			return 0;
		}
		return zonePlay.GetCards().Count((Card c) => !c.IsBeingDragged);
	}

	public bool IsLettuceAbility()
	{
		return GetEntity().IsLettuceAbility();
	}

	public bool HasSameCardDef(CardDef cardDef)
	{
		return m_cardDef?.CardDef == cardDef;
	}

	public T GetCardDefComponent<T>()
	{
		if (!HasCardDef)
		{
			return default(T);
		}
		return m_cardDef.CardDef.GetComponent<T>();
	}
}
