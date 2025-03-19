using System;
using System.Linq;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using Blizzard.T5.MaterialService.Extensions;
using Blizzard.T5.Services;
using Hearthstone.Core;
using Hearthstone.DataModels;
using Hearthstone.UI.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Hearthstone.UI;

[WidgetBehaviorDescription(Path = "Hearthstone/Card", UniqueWithinCategory = "asset")]
[AddComponentMenu("")]
public class Card : CustomWidgetBehavior
{
	private enum RenderObject
	{
		Shadow,
		Highlight,
		CustomMaterial
	}

	public delegate void OnCardActorLoadedDelegate(Actor cardActor);

	private enum PremiumTag
	{
		UseDataModel,
		No,
		Yes,
		Diamond,
		Signature
	}

	private struct DesiredDataModelData
	{
		public string DesiredCardId;

		public TAG_PREMIUM DesiredPremium;

		public int DesiredAttack;

		public int DesiredHealth;

		public int DesiredMana;

		public int DesiredCooldown;

		public DataModelList<GameTagValueDataModel> DesiredGameTagOverrides;

		public DataModelList<SpellType> DesiredSpellTypes;
	}

	[Serializable]
	public struct ActorGemObjectVisibility : IEquatable<ActorGemObjectVisibility>
	{
		public bool HideAttackObject;

		public bool HideHealthObject;

		public bool HideManaObject;

		public bool HideLegendaryDragonObject;

		public bool HideMercenaryRoleBannerObject;

		public bool HideMercenaryRoleGems;

		public bool HideMercenaryXp;

		public bool HideMercenaryWatermark;

		public bool HideMercenaryStats;

		public bool ShowTavernTier;

		public bool HideCollisionBox;

		public bool ShowCoinManaGem;

		public bool Equals(ActorGemObjectVisibility other)
		{
			if (HideAttackObject == other.HideAttackObject && HideHealthObject == other.HideHealthObject && HideManaObject == other.HideManaObject && HideLegendaryDragonObject == other.HideLegendaryDragonObject && HideMercenaryRoleBannerObject == other.HideMercenaryRoleBannerObject && HideMercenaryRoleGems == other.HideMercenaryRoleGems && HideMercenaryXp == other.HideMercenaryXp && HideMercenaryWatermark == other.HideMercenaryWatermark && HideMercenaryStats == other.HideMercenaryStats && ShowTavernTier == other.ShowTavernTier && HideCollisionBox == other.HideCollisionBox)
			{
				return ShowCoinManaGem == other.ShowCoinManaGem;
			}
			return false;
		}
	}

	private readonly RenderObject[] RENDER_OBJECT_ORDER = new RenderObject[3]
	{
		RenderObject.Shadow,
		RenderObject.Highlight,
		RenderObject.CustomMaterial
	};

	[SerializeField]
	[Tooltip("This is the ID of the card displayed by default.")]
	private string m_defaultCardId = "BOT_914h";

	[Tooltip("If true, it will use the card ID from the 'card' data model whenever bound.")]
	[SerializeField]
	private bool m_useCardIdFromDataModel = true;

	[Tooltip("If true, it will use the premium tag from the 'card' data model whenever bound.")]
	[SerializeField]
	private PremiumTag m_golden;

	[Tooltip("If true, this will show the shadow object.")]
	[SerializeField]
	protected bool m_useShadow = true;

	[SerializeField]
	[Tooltip("Optional: If supported by selected card, asset loading will override with texture render to quad.")]
	protected GameObject m_displayRenderOverride;

	[Tooltip("Displays the card using the visual treatment it would have in this zone.")]
	[SerializeField]
	protected TAG_ZONE m_zone = TAG_ZONE.HAND;

	[Tooltip("If true, it will use the Base Render Queue to short the render objects such as the custom material plane, highlight, and shadow.")]
	[SerializeField]
	private bool m_overrideCustomMaterialRenderQueue;

	[SerializeField]
	[Tooltip("This is the bas render queue used for the render objects such as the custom material plane, highlight, and shadow.")]
	private int m_baseCustomMaterialRenderQueue = -3;

	[Tooltip("If true, it will use the stat values set in the data model for attack and health. Otherwise, it will use the EntityDef defaults.")]
	[SerializeField]
	private bool m_useStatsFromDataModel;

	[Tooltip("If true, attempt to use the card's short name if it exists")]
	[SerializeField]
	private bool m_useShortName;

	[Tooltip("Bacon Reward's minion type")]
	[SerializeField]
	private int m_baconRewardMinionType;

	[Tooltip("Bacon Quest Reward is Completed")]
	[SerializeField]
	private bool m_baconRewardIsCompleted = true;

	[Tooltip("Bacon Reward card's database ID")]
	[SerializeField]
	private int m_baconRewardCardDatabaseID;

	[Tooltip("Bacon Quest Progress Total")]
	[SerializeField]
	private int m_baconQuestProgressTotal;

	[Tooltip("Bacon Quest Race 1")]
	[SerializeField]
	private int m_baconQuestRace1;

	[SerializeField]
	[Tooltip("Bacon Quest Race 2")]
	private int m_baconQuestRace2;

	[SerializeField]
	[Tooltip("Bacon Hero Buddy ID")]
	private int m_baconHeroBuddyID;

	[Tooltip("Bacon Hero Buddy Cost")]
	[SerializeField]
	private int m_baconHeroBuddyCost;

	[SerializeField]
	[Tooltip("Bacon Number Buddies Gained")]
	private int m_baconNumBuddiesGained = -1;

	[SerializeField]
	[Tooltip("Bacon Trinket minion type")]
	private int m_baconTrinketMinionType;

	[Tooltip("If true, hide the gems, such as mana, attack and health.")]
	public ActorGemObjectVisibility m_actorGemObjectVisibility;

	[Tooltip("If true, it will use the stat values set in the data model for attack and health. Otherwise, it will use the EntityDef defaults.")]
	[SerializeField]
	private bool m_useTagOverridesFromDataModel;

	private bool m_enableHighlight;

	protected string m_displayedCardId;

	private TAG_PREMIUM m_displayedPremiumTag;

	private int m_displayedAttack;

	private int m_displayedHealth;

	private int m_displayedMana;

	private int m_displayedCooldown;

	private DataModelList<GameTagValueDataModel> m_displayedGameTagOverrides;

	private int m_displayedQuantity;

	protected bool m_isShowingShadow;

	protected Actor m_cardActor;

	private bool m_showCustomEffect;

	private Material m_customEffectMaterial;

	private bool m_isShowingCustomEffect;

	private GhostCard.Type m_ghostedEffectType;

	private GhostCard.Type m_shownGhostedEffectType;

	private bool m_isOverriddingRenderQueues;

	private TAG_ZONE m_displayedActorAssetType = TAG_ZONE.HAND;

	private DataModelList<SpellType> m_displayedSpellTypes;

	private ActorGemObjectVisibility m_displayedActorGemObjectVisibility;

	private bool m_mercenaryXPBarCoverActive = true;

	private bool m_portraitFrameActive = true;

	private TAG_ROLE m_displayedMercenaryRole;

	private TAG_ROLE m_desiredMercenaryRole;

	private float m_portraitDesaturation;

	private int m_mercenaryExperienceInitial;

	private int m_mercenaryExperienceFinal;

	private bool m_mercenaryFullyUpgradedFinal;

	private bool m_mercenaryTreasureBannerActive;

	private bool m_updatesPaused;

	private bool m_forceDisplayAbilityText;

	private string m_abilityTextToDisplay;

	[Overridable]
	public TAG_PREMIUM Premium
	{
		get
		{
			return m_displayedPremiumTag;
		}
		set
		{
			m_displayedPremiumTag = value;
		}
	}

	[Overridable]
	public TAG_ZONE Zone
	{
		get
		{
			return m_zone;
		}
		set
		{
			m_zone = value;
		}
	}

	[Overridable]
	public bool HideAttackGemVisibility
	{
		get
		{
			return m_actorGemObjectVisibility.HideAttackObject;
		}
		set
		{
			m_actorGemObjectVisibility.HideAttackObject = value;
		}
	}

	[Overridable]
	public bool HideHealthGemVisibility
	{
		get
		{
			return m_actorGemObjectVisibility.HideHealthObject;
		}
		set
		{
			m_actorGemObjectVisibility.HideHealthObject = value;
		}
	}

	[Overridable]
	public bool HideManaGemVisibility
	{
		get
		{
			return m_actorGemObjectVisibility.HideManaObject;
		}
		set
		{
			m_actorGemObjectVisibility.HideManaObject = value;
		}
	}

	[Overridable]
	public bool HideLegendaryDragonVisibility
	{
		get
		{
			return m_actorGemObjectVisibility.HideLegendaryDragonObject;
		}
		set
		{
			m_actorGemObjectVisibility.HideLegendaryDragonObject = value;
		}
	}

	[Overridable]
	public bool HideMercenaryRoleBannerVisibility
	{
		get
		{
			return m_actorGemObjectVisibility.HideMercenaryRoleBannerObject;
		}
		set
		{
			m_actorGemObjectVisibility.HideMercenaryRoleBannerObject = value;
		}
	}

	[Overridable]
	public bool HideMercenaryRoleGems
	{
		get
		{
			return m_actorGemObjectVisibility.HideMercenaryRoleGems;
		}
		set
		{
			m_actorGemObjectVisibility.HideMercenaryRoleGems = value;
		}
	}

	[Overridable]
	public bool HideMercenaryXp
	{
		get
		{
			return m_actorGemObjectVisibility.HideMercenaryXp;
		}
		set
		{
			m_actorGemObjectVisibility.HideMercenaryXp = value;
		}
	}

	[Overridable]
	public bool HideMercenaryWatermark
	{
		get
		{
			return m_actorGemObjectVisibility.HideMercenaryWatermark;
		}
		set
		{
			m_actorGemObjectVisibility.HideMercenaryWatermark = value;
		}
	}

	[Overridable]
	public bool HideMercenaryStats
	{
		get
		{
			return m_actorGemObjectVisibility.HideMercenaryStats;
		}
		set
		{
			m_actorGemObjectVisibility.HideMercenaryStats = value;
		}
	}

	[Overridable]
	public bool ShowTavernTier
	{
		get
		{
			return m_actorGemObjectVisibility.ShowTavernTier;
		}
		set
		{
			m_actorGemObjectVisibility.ShowTavernTier = value;
		}
	}

	[Overridable]
	public bool ShowCoinManaGem
	{
		get
		{
			return m_actorGemObjectVisibility.ShowCoinManaGem;
		}
		set
		{
			m_actorGemObjectVisibility.ShowCoinManaGem = value;
		}
	}

	[Overridable]
	public bool HideCollisionBox
	{
		get
		{
			return m_actorGemObjectVisibility.HideCollisionBox;
		}
		set
		{
			m_actorGemObjectVisibility.HideCollisionBox = value;
		}
	}

	[Overridable]
	public bool BaconQuestRewardIsCompleted
	{
		get
		{
			return m_baconRewardIsCompleted;
		}
		set
		{
			m_baconRewardIsCompleted = value;
		}
	}

	[Overridable]
	public int BaconRewardCardDatabaseID
	{
		get
		{
			return m_baconRewardCardDatabaseID;
		}
		set
		{
			m_baconRewardCardDatabaseID = value;
		}
	}

	[Overridable]
	public int BaconRewardMinionType
	{
		get
		{
			return m_baconRewardMinionType;
		}
		set
		{
			m_baconRewardMinionType = value;
		}
	}

	[Overridable]
	public int BaconQuestProgressTotal
	{
		get
		{
			return m_baconQuestProgressTotal;
		}
		set
		{
			m_baconQuestProgressTotal = value;
		}
	}

	[Overridable]
	public int BaconQuestRace1
	{
		get
		{
			return m_baconQuestRace1;
		}
		set
		{
			m_baconQuestRace1 = value;
		}
	}

	[Overridable]
	public int BaconQuestRace2
	{
		get
		{
			return m_baconQuestRace2;
		}
		set
		{
			m_baconQuestRace2 = value;
		}
	}

	[Overridable]
	public int BaconNumBuddiesGained
	{
		get
		{
			return m_baconNumBuddiesGained;
		}
		set
		{
			m_baconNumBuddiesGained = value;
		}
	}

	[Overridable]
	public int BaconHeroBuddyID
	{
		get
		{
			return m_baconHeroBuddyID;
		}
		set
		{
			m_baconHeroBuddyID = value;
		}
	}

	[Overridable]
	public int BaconHeroBuddyCost
	{
		get
		{
			return m_baconHeroBuddyCost;
		}
		set
		{
			m_baconHeroBuddyCost = value;
		}
	}

	[Overridable]
	public int BaconTrinketMinionType
	{
		get
		{
			return m_baconTrinketMinionType;
		}
		set
		{
			m_baconTrinketMinionType = value;
		}
	}

	[Overridable]
	public bool EnableHighlight
	{
		set
		{
			m_enableHighlight = value;
			UpdateHighlightState();
		}
	}

	public Actor CardActor => m_cardActor;

	[Overridable]
	public bool PauseUpdates
	{
		get
		{
			return m_updatesPaused;
		}
		set
		{
			if (m_updatesPaused != value)
			{
				m_updatesPaused = value;
				if (!m_updatesPaused)
				{
					UpdateActor();
				}
			}
		}
	}

	[Overridable]
	public bool UseStatsFromDataModel
	{
		get
		{
			return m_useStatsFromDataModel;
		}
		set
		{
			if (m_useStatsFromDataModel != value)
			{
				m_useStatsFromDataModel = value;
				m_useTagOverridesFromDataModel = value;
				UpdateActor();
			}
		}
	}

	[Overridable]
	public bool UseTagOverridesFromDataModel
	{
		get
		{
			return m_useTagOverridesFromDataModel;
		}
		set
		{
			if (m_useTagOverridesFromDataModel != value)
			{
				m_useTagOverridesFromDataModel = value;
				UpdateActor();
			}
		}
	}

	[Overridable]
	public bool ShowShadow
	{
		get
		{
			return m_useShadow;
		}
		set
		{
			if (m_useShadow != value)
			{
				m_useShadow = value;
				UpdateActor();
			}
		}
	}

	[Overridable]
	public bool ShowCustomEffect
	{
		get
		{
			return m_showCustomEffect;
		}
		set
		{
			if (m_showCustomEffect != value)
			{
				m_showCustomEffect = value;
				UpdateActor();
			}
		}
	}

	[Overridable]
	public Material CustomEffectMaterial
	{
		get
		{
			return m_customEffectMaterial;
		}
		set
		{
			if (value != m_customEffectMaterial)
			{
				m_customEffectMaterial = value;
				m_showCustomEffect = m_customEffectMaterial != null;
				if (m_isShowingCustomEffect && m_cardActor != null)
				{
					m_isShowingCustomEffect = false;
					m_cardActor.DisableMissingCardEffect();
				}
				UpdateActor();
			}
		}
	}

	[Overridable]
	public GhostCard.Type GhostedEffectType
	{
		get
		{
			return m_ghostedEffectType;
		}
		set
		{
			if (value != m_ghostedEffectType)
			{
				m_ghostedEffectType = value;
				UpdateActor();
			}
		}
	}

	[Overridable]
	public int AttackValue
	{
		get
		{
			return m_displayedAttack;
		}
		set
		{
			GetCardDataModel().Attack = value;
			m_displayedAttack = value;
			m_cardActor.GetEntityDef().SetTag(GAME_TAG.ATK, m_displayedAttack);
			m_cardActor.UpdateAttackTextMesh(m_cardActor.GetEntityDef());
		}
	}

	[Overridable]
	public int HealthValue
	{
		get
		{
			return m_displayedHealth;
		}
		set
		{
			GetCardDataModel().Health = value;
			m_displayedHealth = value;
			m_cardActor.GetEntityDef().SetTag(GAME_TAG.HEALTH, m_displayedHealth);
			m_cardActor.UpdateHealthTextMesh(m_cardActor.GetEntityDef());
		}
	}

	[Overridable]
	public bool MercenaryXPBarCoverActive
	{
		get
		{
			return m_mercenaryXPBarCoverActive;
		}
		set
		{
			m_mercenaryXPBarCoverActive = value;
			UpdateActor();
		}
	}

	[Overridable]
	public bool MercenaryTreasureBannerActive
	{
		get
		{
			return m_mercenaryTreasureBannerActive;
		}
		set
		{
			m_mercenaryTreasureBannerActive = value;
			UpdateActor();
		}
	}

	[Overridable]
	public bool PortraitFrameActive
	{
		get
		{
			return m_portraitFrameActive;
		}
		set
		{
			if (m_portraitFrameActive != value)
			{
				m_portraitFrameActive = value;
				UpdateActor();
			}
		}
	}

	[Overridable]
	public TAG_ROLE MercenaryRole
	{
		get
		{
			return m_desiredMercenaryRole;
		}
		set
		{
			m_desiredMercenaryRole = value;
		}
	}

	[Overridable]
	public float PortraitDesaturation
	{
		get
		{
			return m_portraitDesaturation;
		}
		set
		{
			if (m_portraitDesaturation != value)
			{
				m_portraitDesaturation = value;
				UpdateActor();
			}
		}
	}

	[Overridable]
	public int MercenaryExperienceInitial
	{
		get
		{
			return m_mercenaryExperienceInitial;
		}
		set
		{
			m_mercenaryExperienceInitial = value;
			m_cardActor?.GetEntityDef()?.SetTag(GAME_TAG.LETTUCE_MERCENARY_EXPERIENCE, value);
			UpdateActor();
		}
	}

	[Overridable]
	public int MercenaryExperienceFinal
	{
		get
		{
			return m_mercenaryExperienceFinal;
		}
		set
		{
			m_mercenaryExperienceFinal = value;
			UpdateActor();
		}
	}

	[Overridable]
	public bool MercenaryFullyUpgradedFinal
	{
		get
		{
			return m_mercenaryFullyUpgradedFinal;
		}
		set
		{
			m_mercenaryFullyUpgradedFinal = value;
			UpdateActor();
		}
	}

	[Overridable]
	public bool ForceDisplayAbilityText
	{
		get
		{
			return m_forceDisplayAbilityText;
		}
		set
		{
			m_forceDisplayAbilityText = value;
			UpdateActor();
		}
	}

	[Overridable]
	public string AbilityTextToDisplay
	{
		get
		{
			return m_abilityTextToDisplay;
		}
		set
		{
			m_abilityTextToDisplay = value;
			UpdateActor();
		}
	}

	private event OnCardActorLoadedDelegate OnCardActorLoaded;

	protected virtual void UpdateActor()
	{
		if (m_updatesPaused)
		{
			return;
		}
		if (m_cardActor == null)
		{
			m_isShowingShadow = m_useShadow;
			m_isShowingCustomEffect = m_showCustomEffect;
			return;
		}
		m_cardActor.ContactShadow(m_useShadow);
		bool enableProjectedShadow = m_useShadow && !m_cardActor.HasContactShadowObject();
		ProjectedShadow projectedShadow = m_cardActor.GetComponent<ProjectedShadow>();
		if (projectedShadow != null)
		{
			if (enableProjectedShadow)
			{
				projectedShadow.m_enabledAlongsideRealtimeShadows = true;
				m_cardActor.GetComponentsInChildren<MeshRenderer>().ForEach(delegate(MeshRenderer r)
				{
					r.shadowCastingMode = ShadowCastingMode.Off;
				});
				projectedShadow.EnableShadow();
			}
			else
			{
				projectedShadow.DisableShadow();
			}
		}
		m_cardActor.SetupUICardText(m_forceDisplayAbilityText, m_abilityTextToDisplay);
		m_isShowingShadow = m_useShadow;
		m_cardActor.SetPortraitDesaturation(m_portraitDesaturation);
		m_cardActor.SetUseShortName(m_useShortName);
		if (m_cardActor.GetAttackObject() != null && m_actorGemObjectVisibility.HideAttackObject)
		{
			m_cardActor.GetAttackObject().gameObject.SetActive(value: false);
			m_cardActor.GetEntityDef()?.SetTag(GAME_TAG.HIDE_ATTACK, 1);
		}
		if (m_cardActor.GetHealthObject() != null && m_actorGemObjectVisibility.HideHealthObject)
		{
			m_cardActor.GetHealthObject().gameObject.SetActive(value: false);
			m_cardActor.GetEntityDef()?.SetTag(GAME_TAG.HIDE_HEALTH, 1);
		}
		if (m_cardActor.m_manaObject != null)
		{
			m_cardActor.m_manaObject.gameObject.SetActive(!m_actorGemObjectVisibility.HideManaObject);
		}
		if (m_cardActor.m_costTextMesh != null)
		{
			m_cardActor.m_costTextMesh.gameObject.SetActive(!m_actorGemObjectVisibility.HideManaObject);
		}
		if (m_actorGemObjectVisibility.ShowTavernTier)
		{
			m_cardActor.ShowTavernTierSpell();
		}
		else
		{
			m_cardActor.HideTavernTierSpell();
		}
		if (m_actorGemObjectVisibility.ShowCoinManaGem)
		{
			m_cardActor.ShowCoinManaGem();
		}
		if (m_cardActor.m_eliteObject != null)
		{
			m_cardActor.m_eliteObject.gameObject.SetActive(!m_actorGemObjectVisibility.HideLegendaryDragonObject);
		}
		if (m_cardActor.m_mercenaryLevelObject != null)
		{
			if (!m_forceDisplayAbilityText)
			{
				m_cardActor.m_mercenaryLevelObject.m_xpBarCover.SetActive(m_mercenaryXPBarCoverActive);
			}
			m_cardActor.m_mercenaryLevelObject.SetLevelInfo(m_mercenaryExperienceInitial, m_mercenaryExperienceFinal, m_mercenaryFullyUpgradedFinal, this);
		}
		if (m_cardActor.m_portraitFrameObject != null)
		{
			m_cardActor.UpdatePortraitFrameVisibility(m_portraitFrameActive);
		}
		if (m_cardActor.m_mercenaryTreasureBannerObject != null)
		{
			m_cardActor.m_mercenaryTreasureBannerObject.SetActive(m_mercenaryTreasureBannerActive);
		}
		if (m_actorGemObjectVisibility.HideCollisionBox)
		{
			GameObject mesh = GameObjectUtils.FindChild(m_cardActor.gameObject, "Medallion_mesh");
			if (mesh == null)
			{
				mesh = GameObjectUtils.FindChild(m_cardActor.gameObject, "Mesh");
			}
			if (mesh != null)
			{
				BoxCollider collider = mesh.GetComponent<BoxCollider>();
				if (collider != null)
				{
					collider.enabled = false;
				}
				MeshCollider meshCollider = mesh.GetComponent<MeshCollider>();
				if (meshCollider != null)
				{
					meshCollider.enabled = false;
				}
			}
		}
		if (m_baconRewardMinionType != 0)
		{
			string text = m_cardActor.GetEntityDef().GetCardTextInHand();
			text = string.Format(text, GameStrings.GetRaceNameBattlegrounds((TAG_RACE)m_baconRewardMinionType));
			m_cardActor.SetCardDefPowerTextOverride(text);
		}
		if (m_baconRewardCardDatabaseID != 0)
		{
			string text2 = m_cardActor.GetEntityDef().GetCardTextInHand();
			CardDbfRecord cardRecord = GameDbf.Card.GetRecord(m_baconRewardCardDatabaseID);
			if (cardRecord != null)
			{
				string name = cardRecord.Name;
				text2 = string.Format(text2, name);
				m_cardActor.SetCardDefPowerTextOverride(text2);
			}
		}
		if (m_baconQuestProgressTotal != 0)
		{
			string text3 = CardTextBuilder.GetRawCardTextInHand(m_cardActor.GetEntityDef().GetCardId());
			string race1 = string.Empty;
			string race2 = string.Empty;
			string rewardName = string.Empty;
			if (m_baconQuestRace1 != 0)
			{
				race1 = BGQuestCardTextBuilder.GetRaceString((TAG_RACE)m_baconQuestRace1, m_baconQuestProgressTotal);
			}
			if (m_baconQuestRace2 != 0)
			{
				race2 = BGQuestCardTextBuilder.GetRaceString((TAG_RACE)m_baconQuestRace2, m_baconQuestProgressTotal);
			}
			text3 = string.Format(text3, m_baconQuestProgressTotal, rewardName, race1, race2);
			m_cardActor.SetCardDefPowerTextOverride(text3);
		}
		if (m_baconHeroBuddyID != 0)
		{
			HeroBuddyWidgetCoinBased heroBuddyWidget = m_cardActor.GetComponent<HeroBuddyWidgetCoinBased>();
			if (heroBuddyWidget != null)
			{
				heroBuddyWidget.SetHeroBuddyIDOverride(m_baconHeroBuddyID);
				switch (m_baconNumBuddiesGained)
				{
				case 0:
					heroBuddyWidget.gameObject.SetActive(value: false);
					break;
				case 1:
					heroBuddyWidget.gameObject.SetActive(value: true);
					break;
				case 2:
					heroBuddyWidget.gameObject.SetActive(value: true);
					heroBuddyWidget.EnterStage2();
					break;
				default:
					Debug.LogWarning("BG Num Buddies gained greater than 2 or less than 0!");
					break;
				}
				if (m_baconHeroBuddyCost != 0)
				{
					EntityDef entityDef = m_cardActor.GetEntityDef();
					entityDef.SetTag(GAME_TAG.HIDE_COST, 0);
					entityDef.SetTag(GAME_TAG.COST, m_baconHeroBuddyCost);
				}
			}
			else
			{
				Debug.LogWarning("Card.UpdateActor - Can't find heroBuddyWidget in hero buddy object");
			}
		}
		BaconTrinketWidget trinketWidget = m_cardActor.GetComponent<BaconTrinketWidget>();
		if (trinketWidget != null)
		{
			trinketWidget.SetIsStatsPageTrinket(isUsedInStatsPage: true);
		}
		if (m_baconTrinketMinionType != 0)
		{
			string text4 = CardTextBuilder.GetRawCardTextInHand(m_cardActor.GetEntityDef().GetCardId());
			text4 = string.Format(text4, GameStrings.GetRaceNameBattlegrounds((TAG_RACE)m_baconTrinketMinionType));
			m_cardActor.SetCardDefPowerTextOverride(text4);
		}
		GameObjectUtils.FindChild(m_cardActor.gameObject, "BGRewardVFX")?.SetActive(!m_baconRewardIsCompleted);
		SetUpCustomEffect();
		SetUpGhostedEffect();
		if (m_cardActor.m_lettuceMinionInPlayFrame != null)
		{
			EntityDef entityDef2 = m_cardActor.GetEntityDef();
			if (entityDef2.IsMinion() && m_zone == TAG_ZONE.PLAY && entityDef2.HasTag(GAME_TAG.LETTUCE_ROLE))
			{
				m_cardActor.m_lettuceMinionInPlayFrame.gameObject.SetActive(!m_actorGemObjectVisibility.HideMercenaryRoleBannerObject);
				GameObject[] attackBaubles = m_cardActor.m_lettuceMinionInPlayFrame.m_attackBaubles;
				for (int i = 0; i < attackBaubles.Length; i++)
				{
					attackBaubles[i].SetActive(!m_actorGemObjectVisibility.HideMercenaryRoleGems);
				}
				attackBaubles = m_cardActor.m_lettuceMinionInPlayFrame.m_healthBaubles;
				for (int i = 0; i < attackBaubles.Length; i++)
				{
					attackBaubles[i].SetActive(!m_actorGemObjectVisibility.HideMercenaryRoleGems);
				}
			}
		}
		UpdateMercenaryXpVisibility();
		UpdateMercenaryWatermarkVisibility();
		UpdateMercenaryStatsVisibility();
	}

	private void UpdateMercenaryXpVisibility()
	{
		if (!(m_cardActor == null) && m_cardActor.m_mercenaryLevelObject != null)
		{
			m_cardActor.m_mercenaryLevelObject.gameObject.SetActive(m_forceDisplayAbilityText || !m_actorGemObjectVisibility.HideMercenaryXp);
		}
	}

	private void UpdateMercenaryWatermarkVisibility()
	{
		if (!(m_cardActor == null) && !m_forceDisplayAbilityText && m_cardActor.m_watermarkMesh != null)
		{
			m_cardActor.m_watermarkMesh.gameObject.SetActive(!m_actorGemObjectVisibility.HideMercenaryWatermark);
		}
	}

	private void UpdateMercenaryStatsVisibility()
	{
		if (!(m_cardActor == null))
		{
			if (m_cardActor.m_attackTextMesh != null)
			{
				m_cardActor.m_attackTextMesh.gameObject.SetActive(!m_actorGemObjectVisibility.HideMercenaryStats);
			}
			if (m_cardActor.m_healthTextMesh != null)
			{
				m_cardActor.m_healthTextMesh.gameObject.SetActive(!m_actorGemObjectVisibility.HideMercenaryStats);
			}
			if (m_cardActor.m_mercenaryLevelObject != null)
			{
				m_cardActor.m_mercenaryLevelObject.m_levelText.gameObject.SetActive(!m_actorGemObjectVisibility.HideMercenaryStats);
			}
		}
	}

	private void SetupDiamondPortraitMode()
	{
		switch (m_zone)
		{
		case TAG_ZONE.HAND:
			m_cardActor.SetDiamondPortraitMode(playMode: false, forced: true);
			break;
		case TAG_ZONE.PLAY:
			m_cardActor.SetDiamondPortraitMode(playMode: true, forced: true);
			break;
		default:
			Debug.LogWarningFormat("CustomWidgetBehavior:Card - Zone {0} not supported.", m_zone);
			break;
		}
	}

	protected void SetUpCustomEffect()
	{
		if (m_cardActor == null || !Application.IsPlaying(this))
		{
			return;
		}
		if (m_showCustomEffect && !m_isShowingCustomEffect)
		{
			if (m_customEffectMaterial != null)
			{
				m_cardActor.SetMissingCardMaterial(m_customEffectMaterial);
			}
			m_isShowingCustomEffect = true;
			m_cardActor.MissingCardEffect(refreshOnFocus: false);
			m_cardActor.MoveShadowToMissingCard(reset: false, GetRenderQueue(RenderObject.Shadow));
		}
		if (!m_showCustomEffect && m_isShowingCustomEffect)
		{
			m_isShowingCustomEffect = false;
			m_cardActor.DisableMissingCardEffect();
			m_cardActor.MoveShadowToMissingCard(reset: true, GetRenderQueue(RenderObject.Shadow));
		}
		if (m_overrideCustomMaterialRenderQueue && m_showCustomEffect && !m_isOverriddingRenderQueues)
		{
			m_isOverriddingRenderQueues = ApplyRenderQueues();
		}
		else if (!m_showCustomEffect)
		{
			m_isOverriddingRenderQueues = false;
			ApplyRenderQueues(reset: true);
		}
	}

	protected void SetUpGhostedEffect()
	{
		if (!(m_cardActor == null) && m_ghostedEffectType != m_shownGhostedEffectType)
		{
			m_shownGhostedEffectType = m_ghostedEffectType;
			m_cardActor.GhostCardEffect(m_ghostedEffectType);
		}
	}

	private bool ApplyRenderQueues(bool reset = false)
	{
		if (!m_overrideCustomMaterialRenderQueue)
		{
			return false;
		}
		m_cardActor.SetMissingCardRenderQueue(reset, GetRenderQueue(RenderObject.CustomMaterial));
		ActorStateMgr actorStateMgr = m_cardActor.GetActorStateMgr();
		if (actorStateMgr != null)
		{
			return actorStateMgr.SetStateRenderQueue(reset, GetRenderQueue(RenderObject.Highlight));
		}
		return false;
	}

	private int GetRenderQueue(RenderObject renderObject)
	{
		return m_baseCustomMaterialRenderQueue + Array.IndexOf(RENDER_OBJECT_ORDER, renderObject);
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		m_displayedCardId = null;
		m_displayedPremiumTag = TAG_PREMIUM.NORMAL;
		m_displayedQuantity = 1;
		ServiceManager.InitializeDynamicServicesIfEditor(out var serviceDependencies, typeof(IAssetLoader), typeof(GameDbf), typeof(WidgetRunner), typeof(IAliasedAssetResolver));
		Processor.QueueJob(HearthstoneJobs.CreateJobFromAction("Card.CreatePreviewableObject", CreatePreviewableObject, JobFlags.StartImmediately, serviceDependencies));
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (m_cardActor == null || base.Owner == null)
		{
			return;
		}
		Widget childWidget = m_cardActor.GetAmountBannerWidget();
		if (!(childWidget == null))
		{
			RewardItemDataModel ownerData = base.Owner.GetDataModel<RewardItemDataModel>();
			if (childWidget.GetDataModel<RewardItemDataModel>() != ownerData)
			{
				childWidget.BindDataModel(ownerData);
			}
			UpdateAmountBanner();
		}
	}

	private void CreatePreviewableObject()
	{
		if (DefLoader.Get().GetAllEntityDefs().Count == 0)
		{
			DefLoader.Get().LoadAllEntityDefs();
		}
		CreatePreviewableObject(CreateObject, ShouldObjectBeRecreated);
	}

	private void UpdateAmountBanner()
	{
		RewardItemDataModel rewardItemDataModel = GetRewardItemDataModel();
		if (rewardItemDataModel != null)
		{
			m_displayedQuantity = rewardItemDataModel.Quantity;
			Widget amountWidget = m_cardActor.GetAmountBannerWidget();
			if (amountWidget != null)
			{
				amountWidget.gameObject.SetActive(ShouldDisplayBanner(m_displayedQuantity));
				amountWidget.BindDataModel(rewardItemDataModel, overrideChildren: true);
			}
		}
	}

	private void CreateObject(IPreviewableObject previewable, Action<GameObject> callback)
	{
		m_isShowingCustomEffect = false;
		m_shownGhostedEffectType = GhostCard.Type.NONE;
		m_isShowingShadow = false;
		DesiredDataModelData desiredData = GetDesiredData();
		m_displayedCardId = desiredData.DesiredCardId;
		m_displayedPremiumTag = desiredData.DesiredPremium;
		m_displayedAttack = desiredData.DesiredAttack;
		m_displayedHealth = desiredData.DesiredHealth;
		m_displayedMana = desiredData.DesiredMana;
		m_displayedCooldown = desiredData.DesiredCooldown;
		m_displayedGameTagOverrides = desiredData.DesiredGameTagOverrides;
		m_displayedMercenaryRole = m_desiredMercenaryRole;
		m_cardActor = null;
		m_displayedActorAssetType = m_zone;
		m_displayedSpellTypes = desiredData.DesiredSpellTypes;
		m_displayedActorGemObjectVisibility = m_actorGemObjectVisibility;
		if (string.IsNullOrEmpty(m_displayedCardId) || GameDbf.GetIndex().GetCardRecord(m_displayedCardId) == null)
		{
			callback(null);
			return;
		}
		EntityDef cardEntityDef = DefLoader.Get().GetEntityDef(m_displayedCardId);
		if (cardEntityDef == null)
		{
			callback(null);
			return;
		}
		if (m_displayRenderOverride != null)
		{
			using DefLoader.DisposableCardDef disposableCardDef = DefLoader.Get().GetCardDef(m_displayedCardId);
			string overrideRenderPath = ((disposableCardDef?.CardDef != null) ? disposableCardDef.CardDef.m_CustomRenderDisplayOverride : string.Empty);
			if (!string.IsNullOrEmpty(overrideRenderPath) && TryLoadRenderOverride(overrideRenderPath))
			{
				m_displayRenderOverride.SetActive(value: true);
				callback(null);
				return;
			}
		}
		GameObject actorGameObject = LoadActorByActorAssetType(cardEntityDef, desiredData.DesiredPremium);
		if (actorGameObject == null)
		{
			callback(null);
			return;
		}
		Actor actor = (m_cardActor = actorGameObject.GetComponent<Actor>());
		UpdateAmountBanner();
		actor.SetIgnoreGameEntity(m_useCardIdFromDataModel);
		EntityDef cardEntityDefClone = cardEntityDef.Clone();
		actor.SetEntityDef(cardEntityDefClone);
		cardEntityDefClone.SetTag(GAME_TAG.ATK, m_displayedAttack);
		cardEntityDefClone.SetTag(GAME_TAG.HEALTH, m_displayedHealth);
		cardEntityDefClone.SetTag(GAME_TAG.COST, m_displayedMana);
		cardEntityDefClone.SetTag(GAME_TAG.LETTUCE_COOLDOWN_CONFIG, m_displayedCooldown);
		actor.SetPremium(m_displayedPremiumTag);
		if (m_displayedGameTagOverrides != null)
		{
			foreach (GameTagValueDataModel tagOverride in m_displayedGameTagOverrides)
			{
				if (tagOverride.IsReferenceValue)
				{
					cardEntityDefClone.SetReferencedTag((int)tagOverride.GameTag, tagOverride.Value);
					if (tagOverride.IsPowerKeywordTag)
					{
						cardEntityDefClone.SetTag(tagOverride.GameTag, tagOverride.Value);
					}
				}
				else
				{
					cardEntityDefClone.SetTag(tagOverride.GameTag, tagOverride.Value);
				}
			}
		}
		if (m_displayedMercenaryRole != 0)
		{
			cardEntityDefClone.SetTag(GAME_TAG.LETTUCE_ROLE, m_displayedMercenaryRole);
		}
		cardEntityDefClone.SetTag(GAME_TAG.LETTUCE_MERCENARY_EXPERIENCE, m_mercenaryExperienceInitial);
		PlaySpellBirths(actor, desiredData.DesiredSpellTypes);
		actor.ToggleCollider(enabled: false);
		if (m_displayedPremiumTag == TAG_PREMIUM.DIAMOND)
		{
			SetupDiamondPortraitMode();
		}
		if (actor.IsLettuceMercenary())
		{
			actor.SetCardBackSlotOverride(CardBackManager.CardBackSlot.DEFAULT);
		}
		actor.UpdateAllComponents();
		UpdateActor();
		UpdateHighlightState();
		actor.SetUnlit();
		actor.DiamondCardArtUpdatedCallback = actor.SetUnlit;
		Transform obj = actor.transform;
		obj.SetParent(base.transform, worldPositionStays: false);
		obj.localPosition = Vector3.zero;
		obj.localRotation = Quaternion.identity;
		obj.localScale = Vector3.one;
		callback(actor.gameObject);
		if (this.OnCardActorLoaded != null)
		{
			this.OnCardActorLoaded(actor);
		}
	}

	private bool TryLoadRenderOverride(string texturePath)
	{
		if (m_displayRenderOverride == null || string.IsNullOrEmpty(texturePath))
		{
			return false;
		}
		return AssetLoader.Get().LoadTexture(texturePath, delegate(AssetReference assetRef, UnityEngine.Object obj, object callbackDat)
		{
			if ((bool)this && !(m_displayRenderOverride == null))
			{
				MeshRenderer componentInChildren = m_displayRenderOverride.GetComponentInChildren<MeshRenderer>(includeInactive: true);
				if (componentInChildren == null)
				{
					Debug.LogError("Failed to override card asset with texture as no renderer was found in override object!");
				}
				else
				{
					Material material = componentInChildren.GetMaterial();
					if (material == null)
					{
						Debug.LogError("Failed to set texture for card asset override as no material was found in override object!");
					}
					else
					{
						material.mainTexture = obj as Texture;
					}
				}
			}
		});
	}

	private bool ShouldObjectBeRecreated(IPreviewableObject previewableObject)
	{
		if (m_updatesPaused)
		{
			return false;
		}
		DesiredDataModelData desiredData = GetDesiredData();
		int desiredQuantity = GetDesiredQuantity();
		bool shouldDisplayCount = ShouldDisplayBanner(desiredQuantity);
		if (!(m_displayedCardId != desiredData.DesiredCardId) && m_displayedPremiumTag == desiredData.DesiredPremium && m_displayedActorAssetType == m_zone && m_displayedAttack == desiredData.DesiredAttack && m_displayedHealth == desiredData.DesiredHealth && m_displayedMana == desiredData.DesiredMana && m_displayedCooldown == desiredData.DesiredCooldown && OverridesEqual(m_displayedGameTagOverrides, desiredData.DesiredGameTagOverrides) && m_displayedMercenaryRole == m_desiredMercenaryRole && m_displayedActorGemObjectVisibility.Equals(m_actorGemObjectVisibility) && ((m_displayedSpellTypes == null && desiredData.DesiredSpellTypes == null) || (m_displayedSpellTypes != null && desiredData.DesiredSpellTypes != null && m_displayedSpellTypes.SequenceEqual(desiredData.DesiredSpellTypes))) && (!(m_cardActor != null) || m_useShadow == m_isShowingShadow))
		{
			if (shouldDisplayCount)
			{
				return desiredQuantity != m_displayedQuantity;
			}
			return false;
		}
		return true;
	}

	private bool OverridesEqual(DataModelList<GameTagValueDataModel> list1, DataModelList<GameTagValueDataModel> list2)
	{
		if (list1 == list2)
		{
			return true;
		}
		if (list1 == null || list2 == null)
		{
			return false;
		}
		if (list1.Count != list2.Count)
		{
			return false;
		}
		for (int i = 0; i < list1.Count; i++)
		{
			if (list1[i].GameTag != list2[i].GameTag || list1[i].Value != list2[i].Value)
			{
				return false;
			}
		}
		return true;
	}

	protected virtual GameObject LoadActorByActorAssetType(EntityDef entityDef, TAG_PREMIUM premium)
	{
		GameObject actor = null;
		switch (m_zone)
		{
		case TAG_ZONE.HAND:
			actor = AssetLoader.Get().InstantiatePrefab(ActorNames.GetHandActor(entityDef, premium), AssetLoadingOptions.IgnorePrefabPosition);
			break;
		case TAG_ZONE.PLAY:
			actor = AssetLoader.Get().InstantiatePrefab(ActorNames.GetPlayActorByTags(entityDef, premium), AssetLoadingOptions.IgnorePrefabPosition);
			break;
		default:
			Debug.LogWarningFormat("CustomWidgetBehavior:Card - Zone {0} not supported.", m_zone);
			break;
		}
		return actor;
	}

	private DesiredDataModelData GetDesiredData()
	{
		DesiredDataModelData data = default(DesiredDataModelData);
		CardDataModel cardDataModel = GetCardDataModel();
		if (!Application.IsPlaying(this) || m_golden != 0)
		{
			switch (m_golden)
			{
			case PremiumTag.Yes:
				data.DesiredPremium = TAG_PREMIUM.GOLDEN;
				break;
			case PremiumTag.Diamond:
				data.DesiredPremium = TAG_PREMIUM.DIAMOND;
				break;
			default:
				data.DesiredPremium = TAG_PREMIUM.NORMAL;
				break;
			}
		}
		if (!Application.IsPlaying(this) || !m_useCardIdFromDataModel)
		{
			data.DesiredCardId = m_defaultCardId;
		}
		if (cardDataModel != null)
		{
			if (m_useCardIdFromDataModel)
			{
				data.DesiredCardId = cardDataModel.CardId;
			}
			if (m_golden == PremiumTag.UseDataModel)
			{
				data.DesiredPremium = cardDataModel.Premium;
			}
			if (m_useStatsFromDataModel)
			{
				data.DesiredAttack = cardDataModel.Attack;
				data.DesiredHealth = cardDataModel.Health;
				data.DesiredMana = cardDataModel.Mana;
				data.DesiredCooldown = cardDataModel.Cooldown;
			}
			if (m_useTagOverridesFromDataModel)
			{
				data.DesiredGameTagOverrides = cardDataModel.GameTagOverrides;
			}
			data.DesiredSpellTypes = cardDataModel.SpellTypes;
		}
		if ((!Application.IsPlaying(this) && cardDataModel == null) || !m_useStatsFromDataModel)
		{
			EntityDef cardEntityDef = DefLoader.Get().GetEntityDef(data.DesiredCardId);
			if (cardEntityDef == null)
			{
				data.DesiredAttack = 0;
				data.DesiredHealth = 0;
				data.DesiredMana = 0;
				data.DesiredCooldown = 0;
				if (!m_useTagOverridesFromDataModel)
				{
					data.DesiredGameTagOverrides = null;
				}
			}
			else
			{
				(bool, int, int) values = cardEntityDef.GetSummonedMinionStats();
				if (values.Item1)
				{
					data.DesiredAttack = values.Item2;
					data.DesiredHealth = values.Item3;
				}
				else
				{
					data.DesiredAttack = cardEntityDef.GetATK();
					data.DesiredHealth = cardEntityDef.GetHealth();
				}
				data.DesiredMana = cardEntityDef.GetCost();
				data.DesiredCooldown = cardEntityDef.GetTag(GAME_TAG.LETTUCE_COOLDOWN_CONFIG);
				if (!m_useTagOverridesFromDataModel)
				{
					data.DesiredGameTagOverrides = null;
				}
			}
		}
		return data;
	}

	private int GetDesiredQuantity()
	{
		return GetRewardItemDataModel()?.Quantity ?? 1;
	}

	private bool ShouldDisplayBanner(int quantity)
	{
		return quantity > 1;
	}

	private void PlaySpellBirths(Actor actor, DataModelList<SpellType> spellTypes)
	{
		if (spellTypes == null)
		{
			return;
		}
		foreach (SpellType spellType in spellTypes)
		{
			actor.ActivateSpellBirthState(spellType);
		}
	}

	public CardDataModel GetCardDataModel()
	{
		IDataModel dataModel = null;
		GetDataModel(27, out dataModel);
		return dataModel as CardDataModel;
	}

	public RewardItemDataModel GetRewardItemDataModel()
	{
		IDataModel dataModel = null;
		GetDataModel(17, out dataModel);
		return dataModel as RewardItemDataModel;
	}

	public void RegisterCardLoadedListener(OnCardActorLoadedDelegate listener)
	{
		OnCardActorLoaded -= listener;
		OnCardActorLoaded += listener;
		if (m_cardActor != null)
		{
			listener(m_cardActor);
		}
	}

	public void UnregisterCardLoadedListener(OnCardActorLoadedDelegate listener)
	{
		OnCardActorLoaded -= listener;
	}

	private void UpdateHighlightState()
	{
		if (m_cardActor != null)
		{
			m_cardActor.SetActorState((!m_enableHighlight) ? ActorStateType.CARD_IDLE : ActorStateType.CARD_MOUSE_OVER);
		}
	}
}
