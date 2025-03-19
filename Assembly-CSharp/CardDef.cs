using System.Collections.Generic;
using Blizzard.T5.AssetManager;
using Blizzard.T5.MaterialService;
using Blizzard.T5.Services;
using UnityEngine;

[CustomEditClass]
public class CardDef : MonoBehaviour
{
	[CustomEditField(Sections = "Portrait", T = EditType.ARTBUNDLE)]
	public Portrait m_Portrait;

	[CustomEditField(Sections = "Portrait", T = EditType.CARD_TEXTURE, HidePredicate = "HideIfPortrait")]
	public string m_PortraitTexturePath;

	[CustomEditField(Sections = "Portrait", T = EditType.MATERIAL, HidePredicate = "HideIfPortrait")]
	public string m_PremiumPortraitMaterialPath;

	[CustomEditField(Sections = "Portrait", T = EditType.UBERANIMATION, HidePredicate = "HideIfPortrait")]
	public string m_PremiumUberShaderAnimationPath;

	[CustomEditField(Sections = "Portrait", T = EditType.CARD_TEXTURE, HidePredicate = "HideIfPortrait")]
	public string m_PremiumPortraitTexturePath;

	[CustomEditField(Sections = "Portrait", T = EditType.CARD_TEXTURE)]
	public string m_SignaturePortraitTexturePath;

	[CustomEditField(Sections = "Portrait", T = EditType.MATERIAL)]
	public string m_SignaturePortraitMaterialPath;

	[CustomEditField(Sections = "Portrait", T = EditType.UBERANIMATION)]
	public string m_SignatureUberShaderAnimationPath;

	[CustomEditField(Sections = "Portrait", T = EditType.MESH, HidePredicate = "HideIfPortrait")]
	public string m_DiamondPlaneRTT_Hand;

	[CustomEditField(Sections = "Portrait", T = EditType.MESH, HidePredicate = "HideIfPortrait")]
	public string m_DiamondPlaneRTT_Play;

	[CustomEditField(Sections = "Portrait", T = EditType.MESH, HidePredicate = "HideIfPortrait")]
	public string m_DiamondBackground_Hand;

	[CustomEditField(Sections = "Portrait", T = EditType.MESH, HidePredicate = "HideIfPortrait")]
	public string m_DiamondBackground_Play;

	[CustomEditField(Sections = "Portrait", HidePredicate = "HideIfPortrait")]
	public Color m_DiamondPlaneRTT_CearColor = Color.clear;

	[CustomEditField(Sections = "Portrait", T = EditType.CARD_TEXTURE, HidePredicate = "HideIfPortrait")]
	public string m_DiamondPortraitTexturePath;

	[CustomEditField(Sections = "Portrait", T = EditType.GAME_OBJECT, HidePredicate = "HideIfPortrait")]
	public string m_DiamondModel;

	[CustomEditField(Sections = "Portrait", T = EditType.GAME_OBJECT, HidePredicate = "HideIfPortrait")]
	public string m_LegendaryModel;

	[CustomEditField(Sections = "Portrait", T = EditType.GAME_OBJECT, HidePredicate = "HideIfPortrait")]
	public string m_MobileLegendaryModel;

	[CustomEditField(Sections = "Portrait")]
	public int m_PreferredActorPortraitIndex;

	[CustomEditField(Sections = "Portrait", HidePredicate = "HideIfPortrait")]
	public Material m_DeckCardBarPortrait;

	[CustomEditField(Sections = "Portrait", HidePredicate = "HideIfPortrait")]
	public Material m_SignatureDeckCardBarPortrait;

	[CustomEditField(Sections = "Portrait", HidePredicate = "HideIfPortrait")]
	public Material m_EnchantmentPortrait;

	[CustomEditField(Sections = "Portrait", HidePredicate = "HideIfPortrait")]
	public Material m_HistoryTileHalfPortrait;

	[CustomEditField(Sections = "Portrait", HidePredicate = "HideIfPortrait")]
	public Material m_HistoryTileFullPortrait;

	[CustomEditField(Sections = "Portrait", HidePredicate = "HideIfPortrait")]
	public Material m_HistoryTileFullSignaturePortrait;

	[CustomEditField(Sections = "Portrait", HidePredicate = "HideIfPortrait")]
	public Material m_HistoryTileHalfSignaturePortrait;

	[CustomEditField(Sections = "Portrait", HidePredicate = "HideIfPortrait")]
	public Material m_LeaderboardTileFullPortrait;

	[CustomEditField(Sections = "Portrait", HidePredicate = "HideIfPortrait")]
	public Material_MobileOverride m_CustomDeckPortrait;

	[CustomEditField(Sections = "Portrait", HidePredicate = "HideIfPortrait")]
	public Material_MobileOverride m_DeckPickerPortrait;

	[CustomEditField(Sections = "Portrait", T = EditType.CARD_TEXTURE, HidePredicate = "HideIfPortrait")]
	public string m_BattlegroundHeroBuddyPortraitTexturePath;

	[CustomEditField(Sections = "Portrait", T = EditType.MATERIAL, HidePredicate = "HideIfPortrait")]
	public Material m_BattlegroundHeroBuddyPortraitMaterial;

	[CustomEditField(Sections = "Portrait", T = EditType.MATERIAL, HidePredicate = "HideIfPortrait")]
	public Material m_BattlegroundsQuestRewardsMaterial;

	[CustomEditField(Sections = "Portrait", T = EditType.MATERIAL, HidePredicate = "HideIfPortrait")]
	public Material m_BattlegroundsTrinketMaterial;

	[CustomEditField(Sections = "Portrait", T = EditType.MATERIAL, HidePredicate = "HideIfPortrait")]
	public Material m_BattlegroundAnomalyMedallionMaterial;

	[CustomEditField(Sections = "Portrait", T = EditType.MATERIAL, HidePredicate = "HideIfPortrait")]
	public Material m_BattlegroundSpellOnBoardMaterial;

	[CustomEditField(Sections = "Portrait", T = EditType.TEXTURE)]
	public string m_CustomRenderDisplayOverride;

	[CustomEditField(Sections = "Portrait")]
	public Material m_LockedClassPortrait;

	[CustomEditField(Sections = "Portrait", HidePredicate = "HideIfPortrait")]
	public Material m_PracticeAIPortrait;

	[CustomEditField(Sections = "Portrait", HidePredicate = "HideIfPortrait")]
	public Material m_DeckBoxPortrait;

	[CustomEditField(Sections = "Portrait", Hide = true, HidePredicate = "HideIfPortrait")]
	public Material m_MercenaryBarPortrait;

	[CustomEditField(Sections = "Portrait", HidePredicate = "HideIfPortrait")]
	public Material m_MercenaryCoinPortrait;

	[CustomEditField(Sections = "Portrait", HidePredicate = "HideIfPortrait")]
	public Material m_MercenaryMapBossCoinPortrait;

	[CustomEditField(Sections = "Portrait", Hide = true, HidePredicate = "HideIfPortrait")]
	public Material m_TeamTray;

	[CustomEditField(Sections = "Portrait", HidePredicate = "HideIfPortrait")]
	public bool m_AlwaysRenderPremiumPortrait;

	[CustomEditField(Sections = "Portrait", HidePredicate = "HideIfPortrait")]
	public CardSilhouetteOverride m_CardSilhouetteOverride;

	[CustomEditField(Sections = "Portrait")]
	public GameObject m_FrameMeshOverride;

	[CustomEditField(Sections = "Portrait")]
	public bool m_IgnoreLegendaryPortraitForDeckCollection;

	[CustomEditField(Sections = "Portrait")]
	public bool m_UseLegendaryPortraitForHistoryTile;

	[CustomEditField(Sections = "Play")]
	public CardEffectDef m_PlayEffectDef;

	[CustomEditField(Sections = "Play")]
	public List<CardEffectDef> m_AdditionalPlayEffectDefs;

	[CustomEditField(Sections = "Attack")]
	public CardEffectDef m_AttackEffectDef;

	[CustomEditField(Sections = "Death")]
	public CardEffectDef m_DeathEffectDef;

	[CustomEditField(Sections = "Lifetime")]
	public CardEffectDef m_LifetimeEffectDef;

	[CustomEditField(Sections = "Trigger")]
	public List<CardEffectDef> m_TriggerEffectDefs;

	[CustomEditField(Sections = "SubOption")]
	public List<CardEffectDef> m_SubOptionEffectDefs;

	[CustomEditField(Sections = "SubOption")]
	public List<List<CardEffectDef>> m_AdditionalSubOptionEffectDefs;

	[CustomEditField(Sections = "ResetGame")]
	public List<CardEffectDef> m_ResetGameEffectDefs;

	[CustomEditField(Sections = "Sub-Spells")]
	public List<CardEffectDef> m_SubSpellEffectDefs;

	[CustomEditField(Sections = "Custom", T = EditType.SPELL)]
	public string m_CustomSummonSpellPath;

	[CustomEditField(Sections = "Custom", T = EditType.SPELL)]
	public string m_GoldenCustomSummonSpellPath;

	[CustomEditField(Sections = "Custom", T = EditType.SPELL)]
	public string m_DiamondCustomSummonSpellPath;

	[CustomEditField(Sections = "Custom", T = EditType.SPELL)]
	public string m_CustomSpawnSpellPath;

	[CustomEditField(Sections = "Custom", T = EditType.SPELL)]
	public string m_GoldenCustomSpawnSpellPath;

	[CustomEditField(Sections = "Custom", T = EditType.SPELL)]
	public string m_DiamondCustomSpawnSpellPath;

	[CustomEditField(Sections = "Custom", T = EditType.SPELL)]
	public string m_CustomDeathSpellPath;

	[CustomEditField(Sections = "Custom", T = EditType.SPELL)]
	public string m_GoldenCustomDeathSpellPath;

	[CustomEditField(Sections = "Custom", T = EditType.SPELL)]
	public string m_DiamondCustomDeathSpellPath;

	[CustomEditField(Sections = "Custom", T = EditType.SPELL)]
	public string m_CustomDiscardSpellPath;

	[CustomEditField(Sections = "Custom", T = EditType.SPELL)]
	public string m_GoldenCustomDiscardSpellPath;

	[CustomEditField(Sections = "Custom", T = EditType.SPELL)]
	public string m_DiamondCustomDiscardSpellPath;

	[CustomEditField(Sections = "Custom", T = EditType.SPELL)]
	public string m_CustomKeywordSpellPath;

	[CustomEditField(Sections = "Custom", T = EditType.SPELL)]
	public string m_CustomChoiceRevealSpellPath;

	[CustomEditField(Sections = "Custom", T = EditType.SPELL)]
	public string m_CustomChoiceConcealSpellPath;

	[CustomEditField(Sections = "Custom", T = EditType.SPELL)]
	public List<SpellTableOverride> m_SpellTableOverrides;

	[CustomEditField(Sections = "Hero", T = EditType.GAME_OBJECT)]
	public string m_CollectionHeroDefPath;

	[CustomEditField(Sections = "Hero", T = EditType.SPELL)]
	public string m_CustomHeroArmorSpell;

	[CustomEditField(Sections = "Hero", T = EditType.SPELL)]
	public string m_SocketInEffectFriendly;

	[CustomEditField(Sections = "Hero", T = EditType.SPELL)]
	public string m_SocketInEffectOpponent;

	[CustomEditField(Sections = "Hero", T = EditType.SPELL)]
	public string m_SocketInEffectFriendlyPhone;

	[CustomEditField(Sections = "Hero", T = EditType.SPELL)]
	public string m_SocketInEffectOpponentPhone;

	[CustomEditField(Sections = "Hero")]
	public bool m_SocketInOverrideHeroAnimation;

	[CustomEditField(Sections = "Hero")]
	public bool m_SocketInParentEffectToHero = true;

	[CustomEditField(Sections = "Hero", T = EditType.TEXTURE)]
	public string m_CustomHeroTray;

	[CustomEditField(Sections = "Hero", T = EditType.TEXTURE)]
	public string m_CustomHeroTrayGolden;

	[CustomEditField(Sections = "Hero")]
	public bool m_DisablePremiumHeroTray;

	[CustomEditField(Sections = "Hero", T = EditType.GAME_OBJECT)]
	public string m_HeroFrameFriendlyPath;

	[CustomEditField(Sections = "Hero", T = EditType.GAME_OBJECT)]
	public string m_HeroFrameEnemyPath;

	[CustomEditField(Sections = "Hero")]
	public List<Board.CustomTraySettings> m_CustomHeroTraySettings;

	[CustomEditField(Sections = "Hero", T = EditType.TEXTURE)]
	public string m_CustomHeroPhoneTray;

	[CustomEditField(Sections = "Hero", T = EditType.TEXTURE)]
	public string m_CustomHeroPhoneManaGem;

	[CustomEditField(Sections = "Hero", T = EditType.SOUND_PREFAB)]
	public string m_AnnouncerLinePath;

	[CustomEditField(Sections = "Hero", T = EditType.SOUND_PREFAB)]
	public string m_AnnouncerLineBeforeVersusPath;

	[CustomEditField(Sections = "Hero", T = EditType.SOUND_PREFAB)]
	public string m_AnnouncerLineAfterVersusPath;

	[CustomEditField(Sections = "Hero", T = EditType.SOUND_PREFAB)]
	public string m_HeroPickerSelectedPrefab;

	[CustomEditField(Sections = "Hero")]
	public List<EmoteEntryDef> m_EmoteDefs;

	[CustomEditField(Sections = "Hero")]
	public BaconLHSConfig m_LegendaryHeroSkinConfig;

	[CustomEditField(Sections = "Misc", T = EditType.GAME_OBJECT)]
	public string m_StoreItemDisplayPath;

	[CustomEditField(Sections = "HeroFrame", T = EditType.GAME_OBJECT)]
	public string m_CustomHeroFramePrefab;

	[CustomEditField(Sections = "HeroFrame", T = EditType.GAME_OBJECT)]
	public string m_CustomHeroInfoFramePrefab;

	[CustomEditField(Sections = "Misc")]
	public bool m_SuppressDeathrattleDeath;

	[CustomEditField(Sections = "Misc")]
	public bool m_SuppressPlaySoundsOnSummon;

	[CustomEditField(Sections = "Misc")]
	public bool m_SuppressPlaySoundsDuringMulligan;

	[CustomEditField(Sections = "Special Events")]
	public List<CardDefSpecialEvent> m_SpecialEvents;

	private static IMaterialService s_materialService;

	private Material m_LoadedPremiumPortraitMaterial;

	private Material m_LoadedPremiumClassMaterial;

	private Material m_LoadedDeckCardBarPortrait;

	private Material m_LoadedSignatureDeckCardBarPortrait;

	private Material m_LoadedEnchantmentPortrait;

	private Material m_LoadedHistoryTileFullPortrait;

	private Material m_LoadedHistoryTileHalfPortrait;

	private Material m_LoadedHistoryTileFullSignaturePortrait;

	private Material m_LoadedHistoryTileHalfSignaturePortrait;

	private Material m_LoadedLeaderboardTileFullPortrait;

	private Material m_LoadedCustomDeckPortrait;

	private Material m_LoadedDeckPickerPortrait;

	private Material m_LoadedPracticeAIPortrait;

	private Material m_LoadedDeckBoxPortrait;

	private Material m_LoadedSignaturePortraitMaterial;

	private Material m_LoadedBattlegroundHeroBuddyPortrait;

	private Material m_LoadedBattlegroundsSpellPortrait;

	private CardPortraitQuality m_portraitQuality = CardPortraitQuality.GetUnloaded();

	private CardDefSpecialEvent m_currentSpecialEvent;

	private AssetHandle<Texture> m_LoadedPortraitTexture;

	private AssetHandle<Texture> m_loadedPremiumPortraitTexture;

	private AssetHandle<Texture> m_loadedSignaturePortraitTexture;

	private AssetHandle<Material> m_premiumMaterialHandle;

	private AssetHandle<Material> m_signatureMaterialHandle;

	private AssetHandle<UberShaderAnimation> m_premiumPortraitAnimation;

	private AssetHandle<UberShaderAnimation> m_signaturePortraitAnimation;

	private AssetHandle<Texture> m_lowQualityPortrait;

	private AssetHandle<Texture> m_LoadedBattlegroundHeroBuddyPortraitTexture;

	private AssetHandle<Texture> m_LoadedBattlegroundsSpellPortraitTexture;

	protected const int LARGE_MINION_COST = 7;

	protected const int MEDIUM_MINION_COST = 4;

	public string PortraitTexturePath
	{
		get
		{
			if (m_Portrait != null)
			{
				return m_Portrait.m_PortraitTexturePath;
			}
			return m_PortraitTexturePath;
		}
	}

	public string GoldenPortraitMaterialPath
	{
		get
		{
			if (m_Portrait != null)
			{
				return m_Portrait.m_PremiumPortraitMaterialPath;
			}
			return m_PremiumPortraitMaterialPath;
		}
	}

	public string SignaturePortraitMaterialPath => m_SignaturePortraitMaterialPath;

	public bool HideIfPortrait()
	{
		return m_Portrait != null;
	}

	public void Awake()
	{
		if (string.IsNullOrEmpty(m_PortraitTexturePath))
		{
			m_portraitQuality.TextureQuality = 3;
			m_portraitQuality.PremiumType = TAG_PREMIUM.GOLDEN;
		}
		else if (string.IsNullOrEmpty(m_PremiumPortraitMaterialPath))
		{
			m_portraitQuality.PremiumType = TAG_PREMIUM.GOLDEN;
		}
	}

	public virtual string DetermineActorPathForZone(Entity entity, TAG_ZONE zoneTag, bool forceRevealed = false)
	{
		return ActorNames.GetZoneActor(entity, zoneTag, forceRevealed);
	}

	public void OnDestroy()
	{
		if (m_Portrait != null)
		{
			Object.Destroy(m_Portrait);
		}
		if ((bool)m_LoadedPremiumPortraitMaterial)
		{
			Object.Destroy(m_LoadedPremiumPortraitMaterial);
		}
		if ((bool)m_LoadedSignaturePortraitMaterial)
		{
			Object.Destroy(m_LoadedSignaturePortraitMaterial);
		}
		if ((bool)m_LoadedPremiumClassMaterial)
		{
			Object.Destroy(m_LoadedPremiumClassMaterial);
		}
		if ((bool)m_LoadedDeckCardBarPortrait)
		{
			Object.Destroy(m_LoadedDeckCardBarPortrait);
		}
		if ((bool)m_LoadedSignatureDeckCardBarPortrait)
		{
			Object.Destroy(m_LoadedSignatureDeckCardBarPortrait);
		}
		if ((bool)m_LoadedEnchantmentPortrait)
		{
			Object.Destroy(m_LoadedEnchantmentPortrait);
		}
		if ((bool)m_LoadedHistoryTileFullPortrait)
		{
			Object.Destroy(m_LoadedHistoryTileFullPortrait);
		}
		if ((bool)m_LoadedHistoryTileHalfPortrait)
		{
			Object.Destroy(m_LoadedHistoryTileHalfPortrait);
		}
		if ((bool)m_LoadedHistoryTileFullSignaturePortrait)
		{
			Object.Destroy(m_LoadedHistoryTileFullSignaturePortrait);
		}
		if ((bool)m_LoadedHistoryTileHalfSignaturePortrait)
		{
			Object.Destroy(m_LoadedHistoryTileHalfSignaturePortrait);
		}
		if ((bool)m_LoadedLeaderboardTileFullPortrait)
		{
			Object.Destroy(m_LoadedLeaderboardTileFullPortrait);
		}
		if ((bool)m_LoadedCustomDeckPortrait)
		{
			Object.Destroy(m_LoadedCustomDeckPortrait);
		}
		if ((bool)m_LoadedDeckPickerPortrait)
		{
			Object.Destroy(m_LoadedDeckPickerPortrait);
		}
		if ((bool)m_LoadedPracticeAIPortrait)
		{
			Object.Destroy(m_LoadedPracticeAIPortrait);
		}
		if ((bool)m_LoadedDeckBoxPortrait)
		{
			Object.Destroy(m_LoadedDeckBoxPortrait);
		}
		if ((bool)m_LoadedBattlegroundHeroBuddyPortrait)
		{
			Object.Destroy(m_LoadedBattlegroundHeroBuddyPortrait);
		}
		if ((bool)m_LoadedBattlegroundsSpellPortrait)
		{
			Object.Destroy(m_LoadedBattlegroundsSpellPortrait);
		}
		AssetHandle.SafeDispose(ref m_LoadedPortraitTexture);
		AssetHandle.SafeDispose(ref m_signaturePortraitAnimation);
		AssetHandle.SafeDispose(ref m_premiumPortraitAnimation);
		AssetHandle.SafeDispose(ref m_premiumMaterialHandle);
		AssetHandle.SafeDispose(ref m_signatureMaterialHandle);
		AssetHandle.SafeDispose(ref m_loadedPremiumPortraitTexture);
		AssetHandle.SafeDispose(ref m_loadedSignaturePortraitTexture);
		AssetHandle.SafeDispose(ref m_lowQualityPortrait);
		AssetHandle.SafeDispose(ref m_LoadedBattlegroundHeroBuddyPortraitTexture);
		AssetHandle.SafeDispose(ref m_LoadedBattlegroundsSpellPortraitTexture);
	}

	public virtual SpellType DetermineSummonInSpell_HandToPlay(Card card, bool useFastAnimations)
	{
		if (card == null)
		{
			Debug.Log($"CardDef.DetermineSummonInSpell_HandToPlay() - card is null for CardDef {this}");
			return SpellType.NONE;
		}
		Entity entity = card.GetEntity();
		if (entity == null)
		{
			Debug.Log($"CardDef.DetermineSummonInSpell_HandToPlay() - entity is null for card {card}");
			return SpellType.NONE;
		}
		if (entity.IsHero())
		{
			return SpellType.SUMMON_IN_HERO;
		}
		switch ((TAG_ROLE)entity.GetTag(GAME_TAG.LETTUCE_ROLE))
		{
		case TAG_ROLE.CASTER:
			return SpellType.LETTUCE_COME_IN_PLAY_CASTER;
		case TAG_ROLE.FIGHTER:
			return SpellType.LETTUCE_COME_IN_PLAY_FIGHTER;
		case TAG_ROLE.TANK:
			return SpellType.LETTUCE_COME_IN_PLAY_PROTECTOR;
		default:
		{
			EntityDef entityDef = entity.GetEntityDef();
			if (entityDef == null)
			{
				Debug.Log($"CardDef.DetermineSummonInSpell_HandToPlay() - entityDef is null for entity {entity}");
				return SpellType.NONE;
			}
			int cost = entityDef.GetCost();
			TAG_PREMIUM premium = entity.GetPremiumType();
			if (entity.GetController() == null)
			{
				Debug.Log($"CardDef.DetermineSummonInSpell_HandToPlay() - controller is null for entity {entity}");
				return SpellType.NONE;
			}
			bool isFriendlySide = entity.GetController().IsFriendlySide();
			if (useFastAnimations)
			{
				switch (premium)
				{
				case TAG_PREMIUM.DIAMOND:
					if (isFriendlySide)
					{
						return SpellType.SUMMON_IN_DIAMOND_FAST;
					}
					return SpellType.SUMMON_IN_OPPONENT_FAST;
				case TAG_PREMIUM.GOLDEN:
					if (isFriendlySide)
					{
						return SpellType.SUMMON_IN_PREMIUM_FAST;
					}
					return SpellType.SUMMON_IN_OPPONENT_FAST;
				case TAG_PREMIUM.NORMAL:
				case TAG_PREMIUM.SIGNATURE:
					break;
				default:
					Debug.LogWarning($"CardDef.DetermineSummonInSpell_HandToPlay() - unexpected premium type {premium}");
					break;
				}
				if (isFriendlySide)
				{
					return SpellType.SUMMON_IN_FAST;
				}
				return SpellType.SUMMON_IN_OPPONENT_FAST;
			}
			if (cost >= 7)
			{
				switch (premium)
				{
				case TAG_PREMIUM.DIAMOND:
					if (isFriendlySide)
					{
						return SpellType.SUMMON_IN_LARGE_DIAMOND;
					}
					return SpellType.SUMMON_IN_OPPONENT_LARGE_DIAMOND;
				case TAG_PREMIUM.GOLDEN:
					if (isFriendlySide)
					{
						return SpellType.SUMMON_IN_LARGE_PREMIUM;
					}
					return SpellType.SUMMON_IN_OPPONENT_LARGE_PREMIUM;
				case TAG_PREMIUM.NORMAL:
				case TAG_PREMIUM.SIGNATURE:
					break;
				default:
					Debug.LogWarning($"CardDef.DetermineSummonInSpell_HandToPlay() - unexpected premium type {premium}");
					break;
				}
				if (isFriendlySide)
				{
					return SpellType.SUMMON_IN_LARGE;
				}
				return SpellType.SUMMON_IN_OPPONENT_LARGE;
			}
			if (cost >= 4)
			{
				switch (premium)
				{
				case TAG_PREMIUM.DIAMOND:
					if (isFriendlySide)
					{
						return SpellType.SUMMON_IN_MEDIUM_DIAMOND;
					}
					return SpellType.SUMMON_IN_OPPONENT_MEDIUM_DIAMOND;
				case TAG_PREMIUM.GOLDEN:
					if (isFriendlySide)
					{
						return SpellType.SUMMON_IN_MEDIUM_PREMIUM;
					}
					return SpellType.SUMMON_IN_OPPONENT_MEDIUM_PREMIUM;
				case TAG_PREMIUM.NORMAL:
				case TAG_PREMIUM.SIGNATURE:
					break;
				default:
					Debug.LogWarning($"CardDef.DetermineSummonInSpell_HandToPlay() - unexpected premium type {premium}");
					break;
				}
				if (isFriendlySide)
				{
					return SpellType.SUMMON_IN_MEDIUM;
				}
				return SpellType.SUMMON_IN_OPPONENT_MEDIUM;
			}
			switch (premium)
			{
			case TAG_PREMIUM.DIAMOND:
				if (isFriendlySide)
				{
					return SpellType.SUMMON_IN_DIAMOND;
				}
				return SpellType.SUMMON_IN_OPPONENT_DIAMOND;
			case TAG_PREMIUM.GOLDEN:
				if (isFriendlySide)
				{
					return SpellType.SUMMON_IN_PREMIUM;
				}
				return SpellType.SUMMON_IN_OPPONENT_PREMIUM;
			case TAG_PREMIUM.NORMAL:
			case TAG_PREMIUM.SIGNATURE:
				break;
			default:
				Debug.LogWarning($"CardDef.DetermineSummonInSpell_HandToPlay() - unexpected premium type {premium}");
				break;
			}
			if (isFriendlySide)
			{
				return SpellType.SUMMON_IN;
			}
			return SpellType.SUMMON_IN_OPPONENT;
		}
		}
	}

	public virtual SpellType DetermineSummonOutSpell_HandToPlay(Card card)
	{
		Entity entity = card.GetEntity();
		if (entity.IsHero())
		{
			return SpellType.SUMMON_OUT_HERO;
		}
		if (entity.IsMercenary())
		{
			return SpellType.SUMMON_OUT_MERCENARY;
		}
		if (!entity.GetController().IsFriendlySide())
		{
			return SpellType.SUMMON_OUT;
		}
		int cost = entity.GetEntityDef().GetCost();
		TAG_PREMIUM premium = entity.GetPremiumType();
		if (card.GetActor() != null && card.GetActor().UseTechLevelManaGem())
		{
			switch (premium)
			{
			case TAG_PREMIUM.GOLDEN:
				return SpellType.SUMMON_OUT_TECH_LEVEL_PREMIUM;
			case TAG_PREMIUM.NORMAL:
			case TAG_PREMIUM.SIGNATURE:
				break;
			default:
				Debug.LogWarning($"CardDef.DetermineSummonOutSpell_HandToPlay(): unexpected premium type {premium}");
				break;
			}
			return SpellType.SUMMON_OUT_TECH_LEVEL;
		}
		if (cost >= 7)
		{
			switch (premium)
			{
			case TAG_PREMIUM.DIAMOND:
				return SpellType.SUMMON_OUT_DIAMOND;
			case TAG_PREMIUM.GOLDEN:
				return SpellType.SUMMON_OUT_PREMIUM;
			case TAG_PREMIUM.NORMAL:
			case TAG_PREMIUM.SIGNATURE:
				break;
			default:
				Debug.LogWarning($"CardDef.DetermineSummonOutSpell_HandToPlay(): unexpected premium type {premium}");
				break;
			}
			return SpellType.SUMMON_OUT_LARGE;
		}
		if (cost >= 4)
		{
			switch (premium)
			{
			case TAG_PREMIUM.DIAMOND:
				return SpellType.SUMMON_OUT_DIAMOND;
			case TAG_PREMIUM.GOLDEN:
				return SpellType.SUMMON_OUT_PREMIUM;
			case TAG_PREMIUM.NORMAL:
			case TAG_PREMIUM.SIGNATURE:
				break;
			default:
				Debug.LogWarning($"CardDef.DetermineSummonOutSpell_HandToPlay(): unexpected premium type {premium}");
				break;
			}
			return SpellType.SUMMON_OUT_MEDIUM;
		}
		switch (premium)
		{
		case TAG_PREMIUM.DIAMOND:
			return SpellType.SUMMON_OUT_DIAMOND;
		case TAG_PREMIUM.GOLDEN:
			return SpellType.SUMMON_OUT_PREMIUM;
		case TAG_PREMIUM.NORMAL:
		case TAG_PREMIUM.SIGNATURE:
			break;
		default:
			Debug.LogWarning($"CardDef.DetermineSummonOutSpell_HandToPlay(): unexpected premium type {premium}");
			break;
		}
		return SpellType.SUMMON_OUT;
	}

	private static void SetTextureIfNotNull(Material baseMat, ref Material targetMat, Texture tex)
	{
		if (!(baseMat == null))
		{
			if (targetMat == null)
			{
				targetMat = Object.Instantiate(baseMat);
				GetMaterialService()?.IgnoreMaterial(targetMat);
			}
			targetMat.mainTexture = tex;
		}
	}

	private static IMaterialService GetMaterialService()
	{
		if (s_materialService == null)
		{
			s_materialService = ServiceManager.Get<IMaterialService>();
		}
		return s_materialService;
	}

	public void OnBattlegroundHeroBuddyPortraitLoaded(AssetHandle<Texture> portrait)
	{
		AssetHandle.Set(ref m_LoadedBattlegroundHeroBuddyPortraitTexture, portrait);
	}

	public void OnBattlegroundsSpellPortraitLoaded(AssetHandle<Texture> portrait)
	{
		AssetHandle.Set(ref m_LoadedBattlegroundsSpellPortraitTexture, portrait);
	}

	public void OnPortraitLoaded(AssetHandle<Texture> portrait, int quality)
	{
		if (m_Portrait != null)
		{
			m_Portrait.OnPortraitLoaded(portrait, quality);
			return;
		}
		if (quality <= m_portraitQuality.TextureQuality)
		{
			Debug.LogWarning($"Loaded texture of quality lower or equal to what was was already available ({quality} <= {m_portraitQuality}), texture={portrait}");
			return;
		}
		m_portraitQuality.TextureQuality = quality;
		if ((bool)m_LoadedPortraitTexture)
		{
			AssetHandle.Set(ref m_lowQualityPortrait, m_LoadedPortraitTexture);
		}
		AssetHandle.Set(ref m_LoadedPortraitTexture, portrait);
		if (m_LoadedSignaturePortraitMaterial != null && string.IsNullOrEmpty(m_SignaturePortraitTexturePath))
		{
			m_LoadedSignaturePortraitMaterial.mainTexture = m_LoadedPortraitTexture;
			m_portraitQuality.PremiumType = TAG_PREMIUM.SIGNATURE;
		}
		else if (m_LoadedPremiumPortraitMaterial != null && string.IsNullOrEmpty(m_PremiumPortraitTexturePath))
		{
			m_LoadedPremiumPortraitMaterial.mainTexture = m_LoadedPortraitTexture;
			m_portraitQuality.PremiumType = TAG_PREMIUM.GOLDEN;
		}
		if (m_LoadedPremiumClassMaterial != null && string.IsNullOrEmpty(m_PremiumPortraitTexturePath))
		{
			m_LoadedPremiumClassMaterial.mainTexture = m_LoadedPortraitTexture;
		}
		SetTextureIfNotNull(m_DeckCardBarPortrait, ref m_LoadedDeckCardBarPortrait, m_LoadedPortraitTexture);
		SetTextureIfNotNull(m_SignatureDeckCardBarPortrait, ref m_LoadedSignatureDeckCardBarPortrait, m_loadedSignaturePortraitTexture);
		SetTextureIfNotNull(m_EnchantmentPortrait, ref m_LoadedEnchantmentPortrait, m_LoadedPortraitTexture);
		SetTextureIfNotNull(m_HistoryTileFullPortrait, ref m_LoadedHistoryTileFullPortrait, m_LoadedPortraitTexture);
		SetTextureIfNotNull(m_HistoryTileHalfPortrait, ref m_LoadedHistoryTileHalfPortrait, m_LoadedPortraitTexture);
		SetTextureIfNotNull(m_HistoryTileFullSignaturePortrait, ref m_LoadedHistoryTileFullSignaturePortrait, m_loadedSignaturePortraitTexture);
		SetTextureIfNotNull(m_HistoryTileHalfSignaturePortrait, ref m_LoadedHistoryTileHalfSignaturePortrait, m_loadedSignaturePortraitTexture);
		SetTextureIfNotNull(m_LeaderboardTileFullPortrait, ref m_LoadedLeaderboardTileFullPortrait, m_LoadedPortraitTexture);
		SetTextureIfNotNull(m_CustomDeckPortrait, ref m_LoadedCustomDeckPortrait, m_LoadedPortraitTexture);
		SetTextureIfNotNull(m_DeckPickerPortrait, ref m_LoadedDeckPickerPortrait, m_LoadedPortraitTexture);
		SetTextureIfNotNull(m_PracticeAIPortrait, ref m_LoadedPracticeAIPortrait, m_LoadedPortraitTexture);
		SetTextureIfNotNull(m_DeckBoxPortrait, ref m_LoadedDeckBoxPortrait, m_LoadedPortraitTexture);
		SetTextureIfNotNull(m_BattlegroundHeroBuddyPortraitMaterial, ref m_LoadedBattlegroundHeroBuddyPortrait, m_LoadedPortraitTexture);
		SetTextureIfNotNull(m_BattlegroundSpellOnBoardMaterial, ref m_LoadedBattlegroundsSpellPortrait, m_LoadedPortraitTexture);
	}

	public void OnPremiumMaterialLoaded(AssetHandle<Material> material, AssetHandle<Texture> portrait, AssetHandle<UberShaderAnimation> portraitAnimation)
	{
		if (m_Portrait != null)
		{
			m_Portrait.OnPremiumMaterialLoaded(material, portrait, portraitAnimation);
			return;
		}
		if (m_LoadedPremiumPortraitMaterial != null)
		{
			if (Application.isPlaying)
			{
				Debug.LogWarning($"Loaded premium material twice: {material}");
			}
			return;
		}
		if ((bool)material)
		{
			AssetHandle.Set(ref m_premiumMaterialHandle, material);
			m_LoadedPremiumPortraitMaterial = (Material)Object.Instantiate((Object)(Material)material);
			m_LoadedPremiumClassMaterial = (Material)Object.Instantiate((Object)(Material)material);
			IMaterialService materialService = GetMaterialService();
			if (materialService != null)
			{
				materialService.IgnoreMaterial(m_LoadedPremiumPortraitMaterial);
				materialService.IgnoreMaterial(m_LoadedPremiumClassMaterial);
			}
		}
		AssetHandle.Set(ref m_premiumPortraitAnimation, portraitAnimation);
		if ((bool)m_LoadedPortraitTexture)
		{
			if (m_LoadedPremiumPortraitMaterial != null)
			{
				m_LoadedPremiumPortraitMaterial.mainTexture = m_LoadedPortraitTexture;
			}
			if (m_LoadedPremiumClassMaterial != null)
			{
				m_LoadedPremiumClassMaterial.mainTexture = m_LoadedPortraitTexture;
			}
			m_portraitQuality.PremiumType = TAG_PREMIUM.GOLDEN;
		}
		if ((bool)portrait)
		{
			AssetHandle.Set(ref m_loadedPremiumPortraitTexture, portrait);
			if (m_LoadedPremiumPortraitMaterial != null)
			{
				m_LoadedPremiumPortraitMaterial.mainTexture = portrait;
			}
			if (m_LoadedPremiumClassMaterial != null)
			{
				m_LoadedPremiumClassMaterial.mainTexture = portrait;
			}
			m_portraitQuality.PremiumType = TAG_PREMIUM.GOLDEN;
		}
	}

	public void OnSignatureMaterialLoaded(AssetHandle<Material> material, AssetHandle<Texture> portrait, AssetHandle<UberShaderAnimation> animation)
	{
		if ((bool)material)
		{
			AssetHandle.Set(ref m_signatureMaterialHandle, material);
			m_LoadedSignaturePortraitMaterial = (Material)Object.Instantiate((Object)(Material)material);
			GetMaterialService()?.IgnoreMaterial(m_LoadedSignaturePortraitMaterial);
		}
		if ((bool)m_loadedSignaturePortraitTexture)
		{
			if (m_LoadedSignaturePortraitMaterial != null)
			{
				m_LoadedSignaturePortraitMaterial.mainTexture = m_loadedSignaturePortraitTexture;
			}
			m_portraitQuality.PremiumType = TAG_PREMIUM.SIGNATURE;
		}
		if ((bool)portrait)
		{
			AssetHandle.Set(ref m_loadedSignaturePortraitTexture, portrait);
			if (m_LoadedSignaturePortraitMaterial != null && m_LoadedSignaturePortraitMaterial.mainTexture == null)
			{
				m_LoadedSignaturePortraitMaterial.mainTexture = portrait;
			}
			if (m_LoadedSignatureDeckCardBarPortrait != null && m_LoadedSignatureDeckCardBarPortrait.mainTexture == null)
			{
				m_LoadedSignatureDeckCardBarPortrait.mainTexture = portrait;
			}
			if (m_LoadedHistoryTileFullSignaturePortrait != null && m_LoadedHistoryTileFullSignaturePortrait.mainTexture == null)
			{
				m_LoadedHistoryTileFullSignaturePortrait.mainTexture = portrait;
			}
			if (m_LoadedHistoryTileHalfSignaturePortrait != null && m_LoadedHistoryTileHalfSignaturePortrait.mainTexture == null)
			{
				m_LoadedHistoryTileHalfSignaturePortrait.mainTexture = portrait;
			}
			m_portraitQuality.PremiumType = TAG_PREMIUM.SIGNATURE;
		}
		if (animation != null)
		{
			AssetHandle.Set(ref m_signaturePortraitAnimation, animation);
		}
	}

	public CardPortraitQuality GetPortraitQuality()
	{
		return m_portraitQuality;
	}

	public Texture GetBattlegroundHeroBuddyTexture()
	{
		return GetBattlegroundHeroBuddyTextureHandle();
	}

	public Texture GetBattlegroundHeroBuddyTextureFromMat()
	{
		return m_BattlegroundHeroBuddyPortraitMaterial?.mainTexture;
	}

	public Material GetBattlegroundHeroBuddyMaterial()
	{
		if (m_LoadedBattlegroundHeroBuddyPortrait != null)
		{
			return m_LoadedBattlegroundHeroBuddyPortrait;
		}
		return m_BattlegroundHeroBuddyPortraitMaterial;
	}

	public Material GetBattlegroundsQuestRewardPortraitMaterial()
	{
		return m_BattlegroundsQuestRewardsMaterial;
	}

	public Material GetBattlegroundsTrinketPortraitMaterial()
	{
		return m_BattlegroundsTrinketMaterial;
	}

	public Material GetBattlegroundsSpellMaterial()
	{
		return m_BattlegroundSpellOnBoardMaterial;
	}

	public Texture GetBattlegroundsSpellTexture()
	{
		return GetBattlegroundsSpellPortraitTextureHandle();
	}

	public Texture GetBattlegroundsSpellTextureFromMat()
	{
		return m_BattlegroundSpellOnBoardMaterial?.mainTexture;
	}

	public Texture GetPortraitTexture(TAG_PREMIUM premium)
	{
		if (premium == TAG_PREMIUM.SIGNATURE)
		{
			Texture signatureTexture = GetSignaturePortraitTextureHandle();
			if (signatureTexture != null)
			{
				return signatureTexture;
			}
		}
		return GetPortraitTextureHandle();
	}

	public bool TryGetPortraitTexture(TAG_PREMIUM premium, out Texture portraitTexture)
	{
		portraitTexture = GetPortraitTexture(premium);
		return portraitTexture != null;
	}

	public AssetHandle<Texture> GetPortraitTextureHandle()
	{
		if (m_Portrait != null)
		{
			return m_Portrait.LoadedPortraitTexture;
		}
		return m_LoadedPortraitTexture;
	}

	public AssetHandle<Texture> GetSignaturePortraitTextureHandle()
	{
		return m_loadedSignaturePortraitTexture;
	}

	public AssetHandle<Texture> GetBattlegroundHeroBuddyTextureHandle()
	{
		return m_LoadedBattlegroundHeroBuddyPortraitTexture;
	}

	public AssetHandle<Texture> GetBattlegroundsSpellPortraitTextureHandle()
	{
		return m_LoadedBattlegroundsSpellPortraitTexture;
	}

	public bool IsPremiumLoaded(TAG_PREMIUM premium)
	{
		switch (premium)
		{
		case TAG_PREMIUM.SIGNATURE:
			return m_LoadedSignaturePortraitMaterial != null;
		case TAG_PREMIUM.GOLDEN:
			return m_LoadedPremiumPortraitMaterial != null;
		case TAG_PREMIUM.NORMAL:
		case TAG_PREMIUM.DIAMOND:
			return m_LoadedPortraitTexture != null;
		default:
			return false;
		}
	}

	public Material GetPremiumPortraitMaterial()
	{
		if (m_Portrait != null)
		{
			return m_Portrait.m_LoadedPremiumPortraitMaterial;
		}
		return m_LoadedPremiumPortraitMaterial;
	}

	public UberShaderAnimation GetPortraitAnimation(TAG_PREMIUM premium)
	{
		if (premium == TAG_PREMIUM.GOLDEN || premium == TAG_PREMIUM.DIAMOND || m_AlwaysRenderPremiumPortrait)
		{
			if (m_Portrait != null)
			{
				return m_Portrait.PremiumPortraitAnimation;
			}
			return m_premiumPortraitAnimation;
		}
		if (premium == TAG_PREMIUM.SIGNATURE)
		{
			return m_signaturePortraitAnimation;
		}
		return null;
	}

	public Material GetSignaturePortraitMaterial()
	{
		return m_LoadedSignaturePortraitMaterial;
	}

	public Material GetPortraitMaterial(TAG_PREMIUM premium)
	{
		switch (premium)
		{
		case TAG_PREMIUM.SIGNATURE:
			return GetSignaturePortraitMaterial();
		case TAG_PREMIUM.GOLDEN:
			return GetPremiumPortraitMaterial();
		default:
			if (m_LoadedBattlegroundsSpellPortrait != null)
			{
				return GetBattlegroundsSpellMaterial();
			}
			if (m_BattlegroundSpellOnBoardMaterial != null)
			{
				Debug.LogError($"Attempting to get portrait material for unexpected premium level {premium}.");
			}
			return null;
		}
	}

	public Material GetDeckCardBarPortrait(TAG_PREMIUM premium)
	{
		if (premium == TAG_PREMIUM.SIGNATURE && m_LoadedSignatureDeckCardBarPortrait != null)
		{
			return m_LoadedSignatureDeckCardBarPortrait;
		}
		if (m_Portrait != null)
		{
			return m_Portrait.m_LoadedDeckCardBarPortrait;
		}
		return m_LoadedDeckCardBarPortrait;
	}

	public bool TryGetEnchantmentPortrait(out Material enchantmentPortraitMat)
	{
		if (m_Portrait != null)
		{
			enchantmentPortraitMat = m_Portrait.m_LoadedEnchantmentPortrait;
		}
		else
		{
			enchantmentPortraitMat = m_LoadedEnchantmentPortrait;
		}
		return enchantmentPortraitMat != null;
	}

	public bool TryGetHistoryTileFullPortrait(TAG_PREMIUM premium, out Material fullHistoryTileMat)
	{
		if (premium == TAG_PREMIUM.SIGNATURE && m_LoadedHistoryTileFullSignaturePortrait != null)
		{
			fullHistoryTileMat = m_LoadedHistoryTileFullSignaturePortrait;
		}
		else if (m_Portrait != null)
		{
			fullHistoryTileMat = m_Portrait.m_LoadedHistoryTileFullPortrait;
		}
		else
		{
			fullHistoryTileMat = m_LoadedHistoryTileFullPortrait;
		}
		return fullHistoryTileMat != null;
	}

	public bool TryGetHistoryTileHalfPortrait(TAG_PREMIUM premium, out Material halfHistoryTileMat)
	{
		if (premium == TAG_PREMIUM.SIGNATURE && m_LoadedHistoryTileHalfSignaturePortrait != null)
		{
			halfHistoryTileMat = m_LoadedHistoryTileHalfSignaturePortrait;
		}
		else if (m_Portrait != null)
		{
			halfHistoryTileMat = m_Portrait.m_LoadedHistoryTileHalfPortrait;
		}
		else
		{
			halfHistoryTileMat = m_LoadedHistoryTileHalfPortrait;
		}
		return halfHistoryTileMat != null;
	}

	public Material GetLeaderboardTileFullPortrait()
	{
		if (m_Portrait != null)
		{
			return m_Portrait.m_LoadedLeaderboardTileFullPortrait;
		}
		return m_LoadedLeaderboardTileFullPortrait;
	}

	public Material GetCustomDeckPortrait()
	{
		if (m_Portrait != null)
		{
			return m_Portrait.m_LoadedCustomDeckPortrait;
		}
		return m_LoadedCustomDeckPortrait;
	}

	public Material GetDeckPickerPortrait()
	{
		if (m_Portrait != null)
		{
			return m_Portrait.m_LoadedDeckPickerPortrait;
		}
		return m_LoadedDeckPickerPortrait;
	}

	public Material GetPracticeAIPortrait()
	{
		if (m_Portrait != null)
		{
			return m_Portrait.m_LoadedPracticeAIPortrait;
		}
		return m_LoadedPracticeAIPortrait;
	}

	public Material GetDeckBoxPortrait()
	{
		if (m_Portrait != null)
		{
			return m_Portrait.m_LoadedDeckBoxPortrait;
		}
		return m_LoadedDeckBoxPortrait;
	}

	public AssetReference GetBattlegroundHeroBuddyPortraitRef()
	{
		return m_BattlegroundHeroBuddyPortraitTexturePath;
	}

	public AssetReference GetPortraitRef()
	{
		if (m_Portrait != null)
		{
			return m_Portrait.GetPortraitRef();
		}
		if (m_currentSpecialEvent != null && !string.IsNullOrEmpty(m_currentSpecialEvent.m_PortraitTextureOverride))
		{
			return m_currentSpecialEvent.m_PortraitTextureOverride;
		}
		return m_PortraitTexturePath;
	}

	public AssetReference GetPremiumMaterialRef()
	{
		if (m_Portrait != null)
		{
			return m_Portrait.GetPremiumMaterialRef();
		}
		if (m_currentSpecialEvent != null && !string.IsNullOrEmpty(m_currentSpecialEvent.m_PremiumPortraitMaterialOverride))
		{
			return m_currentSpecialEvent.m_PremiumPortraitMaterialOverride;
		}
		return m_PremiumPortraitMaterialPath;
	}

	public AssetReference GetPremiumPortraitRef()
	{
		if (m_Portrait != null)
		{
			return m_Portrait.GetPremiumPortraitRef();
		}
		if (m_currentSpecialEvent != null && !string.IsNullOrEmpty(m_currentSpecialEvent.m_PremiumPortraitTextureOverride))
		{
			return m_currentSpecialEvent.m_PremiumPortraitTextureOverride;
		}
		return m_PremiumPortraitTexturePath;
	}

	public AssetReference GetPremiumAnimationRef()
	{
		if (m_Portrait != null)
		{
			return m_Portrait.GetPremiumAnimationRef();
		}
		if (m_currentSpecialEvent != null && !string.IsNullOrEmpty(m_currentSpecialEvent.m_PremiumUberShaderAnimationOverride))
		{
			return m_currentSpecialEvent.m_PremiumUberShaderAnimationOverride;
		}
		return m_PremiumUberShaderAnimationPath;
	}

	public Material GetPremiumClassMaterial()
	{
		if (m_Portrait != null)
		{
			return m_Portrait.m_LoadedPremiumClassMaterial;
		}
		return m_LoadedPremiumClassMaterial;
	}

	public AssetReference GetSignaturePortraitRef()
	{
		return m_SignaturePortraitTexturePath;
	}

	public AssetReference GetSignatureMaterialRef()
	{
		return SignaturePortraitMaterialPath;
	}

	public AssetReference GetSignatureAnimationRef()
	{
		return m_SignatureUberShaderAnimationPath;
	}

	public string GetUberShaderAnimationPath(TAG_PREMIUM premium)
	{
		switch (premium)
		{
		case TAG_PREMIUM.GOLDEN:
			if (m_Portrait != null)
			{
				return m_Portrait.m_PremiumUberShaderAnimationPath;
			}
			return m_PremiumUberShaderAnimationPath;
		case TAG_PREMIUM.SIGNATURE:
			return m_SignatureUberShaderAnimationPath;
		default:
			return null;
		}
	}

	public void UpdateSpecialEvent()
	{
		m_currentSpecialEvent = CardDefSpecialEvent.FindActiveEvent(this);
	}
}
