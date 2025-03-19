using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Assets;
using Blizzard.BlizzardErrorMobile;
using Blizzard.T5.AssetManager;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Game.Spells;
using Blizzard.T5.MaterialService.Extensions;
using Blizzard.T5.Services;
using Cysharp.Threading.Tasks;
using Hearthstone.UI;
using HutongGames.PlayMaker;
using PegasusShared;
using UnityEngine;

[CustomEditClass]
public class Actor : MonoBehaviour, IVisibleWidgetComponent
{
	[Serializable]
	public class FactionObject
	{
		public TAG_LETTUCE_FACTION m_faction;

		public GameObject m_banner;
	}

	protected struct ContactShadowData
	{
		public bool IsUnique { get; private set; }

		public GameObject ShadowObject { get; private set; }

		public int InitialRenderQueue { get; private set; }

		public Vector3 InitialPositionRelativeToActor { get; private set; }

		public ContactShadowData(bool isUnique, GameObject shadowObject, int initialRenderQueue, Vector3 initialRelativeToActor)
		{
			IsUnique = isUnique;
			ShadowObject = shadowObject;
			InitialRenderQueue = initialRenderQueue;
			InitialPositionRelativeToActor = initialRelativeToActor;
		}
	}

	public delegate void CustomFrameChangedEventHandler(CustomFrameController customFrameController);

	private enum PortraitMode
	{
		Default,
		ForcedPlayMode,
		ForcedHandMode
	}

	protected readonly UnityEngine.Vector2 GEM_TEXTURE_OFFSET_RARE = new UnityEngine.Vector2(0.5f, 0f);

	protected readonly UnityEngine.Vector2 GEM_TEXTURE_OFFSET_EPIC = new UnityEngine.Vector2(0f, 0.5f);

	protected readonly UnityEngine.Vector2 GEM_TEXTURE_OFFSET_LEGENDARY = new UnityEngine.Vector2(0.5f, 0.5f);

	protected readonly UnityEngine.Vector2 GEM_TEXTURE_OFFSET_COMMON = new UnityEngine.Vector2(0f, 0f);

	protected readonly Color GEM_COLOR_RARE = new Color(0.1529f, 0.498f, 1f);

	protected readonly Color GEM_COLOR_EPIC = new Color(0.596f, 0.1568f, 0.7333f);

	protected readonly Color GEM_COLOR_LEGENDARY = new Color(1f, 0.5333f, 0f);

	protected readonly Color GEM_COLOR_COMMON = new Color(0.549f, 0.549f, 0.549f);

	protected readonly Color CLASS_COLOR_GENERIC = new Color(0.7f, 0.7f, 0.7f);

	protected readonly Color CLASS_COLOR_WARLOCK = new Color(0.33f, 0.2f, 0.4f);

	protected readonly Color CLASS_COLOR_ROGUE = new Color(0.23f, 0.23f, 0.23f);

	protected readonly Color CLASS_COLOR_DRUID = new Color(0.42f, 0.29f, 0.14f);

	protected readonly Color CLASS_COLOR_SHAMAN = new Color(0f, 0.32f, 0.71f);

	protected readonly Color CLASS_COLOR_HUNTER = new Color(0.26f, 0.54f, 0.18f);

	protected readonly Color CLASS_COLOR_MAGE = new Color(0.44f, 0.48f, 0.69f);

	protected readonly Color CLASS_COLOR_PALADIN = new Color(0.71f, 0.49f, 0.2f);

	protected readonly Color CLASS_COLOR_PRIEST = new Color(1f, 1f, 1f);

	protected readonly Color CLASS_COLOR_WARRIOR = new Color(0.43f, 0.14f, 0.14f);

	protected readonly Color CLASS_COLOR_DEATHKNIGHT = new Color(0.0666667f, 0.5294f, 0.5843f);

	protected readonly Color CLASS_COLOR_DEMONHUNTER = new Color(0.0902f, 0.2275f, 0.1961f);

	protected readonly Color CLASS_COLOR_LOCATION_GENERIC = new Color(0.7f, 0.7f, 0.7f);

	protected readonly Color CLASS_COLOR_LOCATION_WARLOCK = new Color(0.3967f, 0.1721f, 0.5f);

	protected readonly Color CLASS_COLOR_LOCATION_ROGUE = new Color(0.1981f, 0.1981f, 0.1981f);

	protected readonly Color CLASS_COLOR_LOCATION_DRUID = new Color(0.3301f, 0.2281f, 0.1105f);

	protected readonly Color CLASS_COLOR_LOCATION_SHAMAN = new Color(0f, 0.2101f, 0.5377f);

	protected readonly Color CLASS_COLOR_LOCATION_HUNTER = new Color(0.1492f, 0.3679f, 0.0885f);

	protected readonly Color CLASS_COLOR_LOCATION_MAGE = new Color(0.3037f, 0.5386f, 0.8584f);

	protected readonly Color CLASS_COLOR_LOCATION_PALADIN = new Color(0.6792f, 0.4239f, 0.0865f);

	protected readonly Color CLASS_COLOR_LOCATION_PRIEST = new Color(0.8207f, 0.8207f, 0.8207f);

	protected readonly Color CLASS_COLOR_LOCATION_WARRIOR = new Color(0.5566f, 0.1128f, 0.1762f);

	protected readonly Color CLASS_COLOR_LOCATION_DEATHKNIGHT = new Color(0.07f, 0.53f, 0.58f);

	protected readonly Color CLASS_COLOR_LOCATION_DEMONHUNTER = new Color(0.1406f, 0.3773f, 0.3247f);

	private readonly Color MISSING_CARD_WILD_GOLDEN_COLOR = new Color(0.518f, 0.361f, 0f, 0.68f);

	private readonly Color MISSING_CARD_STANDARD_GOLDEN_COLOR = new Color(0.867f, 0.675f, 0.22f, 0.53f);

	private readonly string STARSHIP_LAUNCH_COST = "LaunchCost";

	private readonly string STARSHIP_LAUNCH_COST_COLOR = "LaunchCostColor";

	protected readonly Color MISSING_CARD_WILD_DIAMOND_COLOR = new Color(0.4705f, 0.3058f, 0.0117f, 0.6784f);

	protected readonly string MISSING_CARD_WILD_DIAMOND_CONTRAST_KEY = "_Contrast";

	protected readonly float MISSING_CARD_WILD_DIAMOND_CONTRAST = 0.4f;

	protected readonly string MISSING_CARD_WILD_DIAMOND_INTENSITY_KEY = "_Intensity";

	protected readonly float MISSING_CARD_WILD_DIAMOND_INTENSITY = 1.7f;

	protected readonly float WATERMARK_ALPHA_VALUE = 99f / 128f;

	protected readonly List<GAME_TAG> VALID_FACTIONS = new List<GAME_TAG>
	{
		GAME_TAG.GRIMY_GOONS,
		GAME_TAG.KABAL,
		GAME_TAG.JADE_LOTUS,
		GAME_TAG.ZERG,
		GAME_TAG.TERRAN,
		GAME_TAG.PROTOSS
	};

	public GameObject m_cardMesh;

	public int m_cardFrontMatIdx = -1;

	public int m_cardBackMatIdx = -1;

	public int m_premiumRibbon = -1;

	public GameObject m_portraitMesh;

	public GameObject m_portraitMeshRTT;

	public GameObject m_portraitMeshRTT_background;

	public bool m_usePlayPortrait;

	public int m_portraitFrameMatIdx = -1;

	public int m_portraitMatIdx = -1;

	public GameObject m_xpBarRootObject;

	public GameObject m_nameBannerMesh;

	public GameObject m_descriptionMesh;

	public GameObject m_descriptionTrimMesh;

	public GameObject m_baconQuestDescriptionMesh;

	public GameObject m_watermarkMesh;

	public GameObject m_rarityFrameMesh;

	public GameObject m_rarityNoGemMesh;

	public GameObject m_rarityGemMesh;

	public GameObject m_racePlateMesh;

	public Mesh m_spellDescriptionMeshNeutral;

	public Mesh m_spellDescriptionMeshSchool;

	public GameObject m_attackObject;

	public GameObject m_healthObject;

	public GameObject m_armorObject;

	public GameObject m_manaObject;

	public GameObject m_baconCoinObject;

	public CardRuneBanner m_cardRuneBanner;

	public GameObject m_deckRunesContainer;

	public RuneSlotVisual m_deckRuneSlotVisual;

	public GameObject m_speedWingObject;

	public GameObject m_racePlateObject;

	public GameObject m_multiRacePlateObject;

	public GameObject m_cardTypeAnchorObject;

	public GameObject m_eliteObject;

	public GameObject m_classIconObject;

	public GameObject m_classIconPos;

	public GameObject m_diamondPortraitClassIconPos;

	public GameObject m_heroSpotLight;

	public GameObject m_glints;

	public GameObject m_armorSpellBone;

	public GameObject m_decorationRoot;

	public NestedPrefab m_tradeableBannerContainer;

	public NestedPrefab m_forgeBannerContainer;

	public NestedPrefab m_hearthstoneFactionBannerContainer;

	public NestedPrefab m_bannedRibbonContainer;

	public GameObject m_multiclassRibbon;

	public List<MercenaryRoleGemObject> m_mercenaryRoleObjects;

	public MercenaryActorLevelObject m_mercenaryLevelObject;

	public GameObject m_portraitFrameObject;

	public GameObject m_mercenaryTreasureBannerObject;

	public LoanerFlag m_loanerFlag;

	public GameObject m_actorSpecificPortraitOverlay;

	public GameObject m_gemMeshCommon;

	public GameObject m_gemMeshRare;

	public GameObject m_gemMeshEpic;

	public GameObject m_gemMeshLegendary;

	public FactionObject[] m_factionBannerIcons;

	public GameObject m_factionBannerBackground;

	public List<MeshRenderer> m_meshesThatAffectBoundsCalculations;

	public UberText m_costTextMesh;

	public UberText m_attackTextMesh;

	public UberText m_healthTextMesh;

	public UberText m_armorTextMesh;

	public UberText m_nameTextMesh;

	public UberText m_powersTextMesh;

	public UberText m_raceTextMesh;

	public UberText m_multiRaceTextMesh;

	public UberText m_bgQuestPowerTextMesh;

	public UberText m_bgQuestRaceTextMesh;

	public UberText m_secretText;

	public GameObject m_missingCardEffect;

	public GameObject m_ghostCardGameObject;

	public bool m_ghostCardActive;

	public GameObject m_diamondPortraitR2T;

	public LettuceMinionInPlayFrame m_lettuceMinionInPlayFrame;

	public GameObject m_enchantmentBannerAnchorObject;

	public Widget m_amountBannerWidget;

	public bool m_useCardDefMaterial;

	public bool m_isDebuggingBattlegroundQuestReward;

	private bool m_showUICardText;

	private string m_UICardText;

	private Transform m_spellsParent;

	private bool m_skipArmorAnimation;

	private bool m_isTextVisible = true;

	private bool m_healthDisplayRequiresHeroCard = true;

	private int m_showCostOverride;

	[CustomEditField(T = EditType.ACTOR)]
	public string m_spellTablePrefab;

	protected Card m_card;

	protected Entity m_entity;

	protected CardDefHandle m_cardDefHandle = new CardDefHandle();

	protected EntityDef m_entityDef;

	protected TAG_PREMIUM m_premiumType;

	protected ProjectedShadow m_projectedShadow;

	protected bool m_ignoreGameEntity;

	protected bool m_shown = true;

	protected bool m_shadowVisible;

	protected ActorStateMgr m_actorStateMgr;

	protected ActorStateType m_actorState = ActorStateType.CARD_IDLE;

	protected bool forceIdleState;

	protected GameObject m_rootObject;

	protected GameObject m_bones;

	protected MeshRenderer m_meshRenderer;

	protected MeshRenderer m_meshRendererPortrait;

	protected int m_legacyPortraitMaterialIndex = -1;

	protected int m_legacyCardColorMaterialIndex = -1;

	protected Material m_initialPortraitMaterial;

	protected Material m_initialPremiumRibbonMaterial;

	protected Material m_initialCardBackMaterial;

	protected SpellTable m_sharedSpellTable;

	protected bool m_useSharedSpellTable;

	protected Dictionary<SpellType, Spell> m_ownedSpells;

	protected SpellTable m_localSpellTable;

	protected ArmorSpell m_armorSpell;

	protected GameObject m_hiddenCardStandIn;

	protected bool m_shadowform;

	protected GhostCard.Type m_ghostCard;

	protected TAG_PREMIUM m_ghostPremium;

	protected bool m_missingcard;

	protected bool m_armorSpellLoading;

	protected bool m_materialEffectsSeeded;

	protected Player.Side? m_cardBackSideOverride;

	protected CardBackManager.CardBackSlot? m_cardBackSlotOverride;

	private string m_cardDefPowerTextOverride;

	protected bool m_ignoreUpdateCardback;

	protected bool isPortraitMaterialDirty;

	protected Texture m_portraitTextureOverride;

	protected bool m_blockTextComponentUpdate;

	protected bool m_armorSpellDisabledForTransition;

	protected HearthstoneFactionBanner m_hearthstoneFactionBanner;

	protected DeckActionBanner m_deckActionBanner;

	protected UberShaderController m_uberShaderController;

	protected bool m_ignoreHideStats;

	protected TAG_CARD_SET m_watermarkCardSetOverride;

	protected bool m_useShortName;

	protected GameObject m_bannedRibbon;

	protected bool m_useBGQuestSiloutte;

	private TEAMMATE_PING_TYPE m_activePingType;

	private bool m_blockPings;

	private bool m_isTeammateActor;

	private bool m_hasUsedStarshipLaunchAnimationDelay;

	protected List<ContactShadowData> m_contactShadows;

	private bool m_shadowObjectInitialized;

	private int m_initialMissingCardRenderQueue;

	private bool m_isDiamondViewer;

	private GameObject m_diamondModelObject;

	private DiamondRenderToTexture m_diamondRenderToTexture;

	private string m_diamondModelShown;

	private bool m_portraitMeshDirty = true;

	private CustomFrameController m_customFrameController;

	private float m_cachedProjectedShadowAutoDisableHeight;

	private CancellationTokenSource m_updateTokenSource = new CancellationTokenSource();

	private bool m_useAlternateCostTextPos;

	public readonly Vector3 m_alternateCostTextLocalPos = new Vector3(-0.01f, 0.003f, -0.58f);

	private AssetHandle<Texture> m_watermarkTex;

	protected AssetHandle<Texture> m_cardColorTex;

	private IGraphicsManager m_graphicsManager;

	[CustomEditField(Hide = true)]
	public Action DiamondCardArtUpdatedCallback;

	[CustomEditField(Hide = true)]
	public Action OnSetCard;

	[CustomEditField(Hide = true)]
	public Action OnPortraitMaterialUpdated;

	private PortraitMode m_portraitMode;

	private static readonly float descriptionMesh_WithoutRace_TextureOffset = 0.07f;

	private static readonly float descriptionMesh_WithRace_TextureOffset = 0f;

	public ILegendaryHeroPortrait LegendaryHeroPortrait { get; private set; }

	public float ZoneHeroPositionOffset { get; private set; }

	public bool IsDesiredHidden { get; private set; }

	public bool IsDesiredHiddenInHierarchy
	{
		get
		{
			if (IsDesiredHidden)
			{
				return true;
			}
			WidgetTemplate widgetOwner = GetComponentInParent<WidgetTemplate>();
			if (widgetOwner != null && widgetOwner.IsDesiredHiddenInHierarchy)
			{
				return true;
			}
			return false;
		}
	}

	public bool HandlesChildVisibility => true;

	public bool HasCardDef => m_cardDefHandle.Get(m_premiumType) != null;

	public string CardDefName
	{
		get
		{
			if (!HasCardDef)
			{
				return null;
			}
			return m_cardDefHandle.Get(m_premiumType).name;
		}
	}

	public Material DeckCardBarPortrait
	{
		get
		{
			if (!HasCardDef)
			{
				return null;
			}
			return m_cardDefHandle.Get(m_premiumType).GetDeckCardBarPortrait(m_premiumType);
		}
	}

	public Texture PortraitTexture
	{
		get
		{
			if (!HasCardDef)
			{
				return null;
			}
			return m_cardDefHandle.Get(m_premiumType).GetPortraitTexture(m_premiumType);
		}
	}

	public Material PremiumPortraitMaterial
	{
		get
		{
			if (!HasCardDef)
			{
				return null;
			}
			return m_cardDefHandle.Get(m_premiumType).GetPremiumPortraitMaterial();
		}
	}

	public UberShaderAnimation PortraitAnimation
	{
		get
		{
			if (!HasCardDef)
			{
				return null;
			}
			return m_cardDefHandle.Get(m_premiumType).GetPortraitAnimation(m_premiumType);
		}
	}

	public CardPortraitQuality CardPortraitQuality => CardPortraitQuality.GetFromDef(m_cardDefHandle.Get(m_premiumType));

	public CardEffectDef PlayEffectDef
	{
		get
		{
			if (!HasCardDef)
			{
				return null;
			}
			return m_cardDefHandle.Get(m_premiumType).m_PlayEffectDef;
		}
	}

	public bool PremiumAnimationAvailable => CardTextureLoader.PremiumAnimationAvailable(m_cardDefHandle.Get(m_premiumType));

	public string SocketInEffectFriendly
	{
		get
		{
			if (!HasCardDef)
			{
				return null;
			}
			return m_cardDefHandle.Get(m_premiumType).m_SocketInEffectFriendly;
		}
	}

	public string SocketInEffectFriendlyPhone
	{
		get
		{
			if (!HasCardDef)
			{
				return null;
			}
			return m_cardDefHandle.Get(m_premiumType).m_SocketInEffectFriendlyPhone;
		}
	}

	public string SocketInEffectOpponent
	{
		get
		{
			if (!HasCardDef)
			{
				return null;
			}
			return m_cardDefHandle.Get(m_premiumType).m_SocketInEffectOpponent;
		}
	}

	public string SocketInEffectOpponentPhone
	{
		get
		{
			if (!HasCardDef)
			{
				return null;
			}
			return m_cardDefHandle.Get(m_premiumType).m_SocketInEffectOpponentPhone;
		}
	}

	public bool SocketInOverrideHeroAnimation
	{
		get
		{
			if (!HasCardDef)
			{
				return false;
			}
			return m_cardDefHandle.Get(m_premiumType).m_SocketInOverrideHeroAnimation;
		}
	}

	public bool SocketInParentEffectToHero
	{
		get
		{
			if (!HasCardDef)
			{
				return false;
			}
			return m_cardDefHandle.Get(m_premiumType).m_SocketInParentEffectToHero;
		}
	}

	public List<EmoteEntryDef> EmoteDefs
	{
		get
		{
			if (!HasCardDef)
			{
				return null;
			}
			return m_cardDefHandle.Get(m_premiumType).m_EmoteDefs;
		}
	}

	public bool AlwaysRenderPremiumPortrait
	{
		get
		{
			if (m_cardDefHandle != null && m_cardDefHandle.Get(m_premiumType) != null)
			{
				return m_cardDefHandle.Get(m_premiumType).m_AlwaysRenderPremiumPortrait;
			}
			return false;
		}
		set
		{
			if (m_cardDefHandle != null && m_cardDefHandle.Get(m_premiumType) != null)
			{
				m_cardDefHandle.Get(m_premiumType).m_AlwaysRenderPremiumPortrait = value;
			}
		}
	}

	public CardSilhouetteOverride CardSilhouetteOverride
	{
		get
		{
			if (!HasCardDef)
			{
				return CardSilhouetteOverride.None;
			}
			return m_cardDefHandle.Get(m_premiumType).m_CardSilhouetteOverride;
		}
	}

	public BaconLHSConfig LegendaryHeroSkinConfig
	{
		get
		{
			if (!HasCardDef)
			{
				return null;
			}
			return m_cardDefHandle.Get(m_premiumType).m_LegendaryHeroSkinConfig;
		}
	}

	public bool HasUsedStarshipLaunchAnimationDelay
	{
		get
		{
			return m_hasUsedStarshipLaunchAnimationDelay;
		}
		set
		{
			m_hasUsedStarshipLaunchAnimationDelay = value;
		}
	}

	private event CustomFrameChangedEventHandler OnCustomFrameChanged;

	public bool UseBGQuestSiloutte()
	{
		return m_useBGQuestSiloutte;
	}

	public void SetUseBGQuestSiloutte(bool value)
	{
		m_useBGQuestSiloutte = value;
	}

	public virtual void Awake()
	{
		m_graphicsManager = ServiceManager.Get<IGraphicsManager>();
		AssignRootObject();
		AssignBones();
		AssignMeshRenderers();
		AssignSpells();
		CacheInitialMaterials();
	}

	protected virtual void OnEnable()
	{
		if (isPortraitMaterialDirty)
		{
			UpdateAllComponents();
		}
	}

	private void Start()
	{
		Init();
	}

	private void OnDestroy()
	{
		if (CardBackManager.Get() != null)
		{
			CardBackManager.Get().UnregisterUpdateCardbacksListener(UpdateCardBack);
		}
		ReleaseSpells();
		ReleaseCardDef();
		if ((bool)m_diamondPortraitR2T)
		{
			UnityEngine.Object.Destroy(m_diamondPortraitR2T);
		}
		if ((bool)m_uberShaderController)
		{
			m_uberShaderController.UberShaderAnimation = null;
		}
		DestroyCreatedMaterials();
		DestroyLegendaryHeroPortrait();
		DestroyCustomFrame();
		m_updateTokenSource?.Cancel();
		m_updateTokenSource?.Dispose();
		AssetHandle.SafeDispose(ref m_watermarkTex);
		AssetHandle.SafeDispose(ref m_cardColorTex);
	}

	public void Init()
	{
		if (CardBackManager.Get() != null)
		{
			CardBackManager.Get().RegisterUpdateCardbacksListener(UpdateCardBack);
		}
		if (m_rootObject != null)
		{
			TransformUtil.Identity(m_rootObject.transform);
		}
		if (m_actorStateMgr != null)
		{
			m_actorStateMgr.ChangeState(m_actorState);
		}
		m_projectedShadow = GetComponent<ProjectedShadow>();
		if (m_projectedShadow != null)
		{
			m_cachedProjectedShadowAutoDisableHeight = m_projectedShadow.m_AutoDisableHeight;
		}
		if (m_shown)
		{
			ShowImpl(ignoreSpells: false);
		}
		else
		{
			HideImpl(ignoreSpells: false);
		}
	}

	public void Destroy()
	{
		if ((bool)base.gameObject)
		{
			ReleaseSpells();
			ReleaseCardDef();
			DestroyCreatedMaterials();
			DestroyLegendaryHeroPortrait();
			DestroyCustomFrame();
			if (!Application.IsPlaying(this))
			{
				UnityEngine.Object.DestroyImmediate(base.gameObject);
			}
			else
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	private void ReleaseSpells()
	{
		SpellManager spellManager = SpellManager.Get();
		if (spellManager == null)
		{
			return;
		}
		List<Spell> spellsToDeactivate = new List<Spell>();
		if (m_ownedSpells != null)
		{
			foreach (Spell spellToDeactivate in m_ownedSpells.Values)
			{
				if (!(spellToDeactivate == null))
				{
					spellToDeactivate.RemoveSpellReleasedCallback(OnSpellRelease);
					spellsToDeactivate.Add(spellToDeactivate);
				}
			}
			m_ownedSpells.Clear();
		}
		for (int i = spellsToDeactivate.Count - 1; i >= 0; i--)
		{
			spellManager.ReleaseSpell(spellsToDeactivate[i]);
		}
		spellsToDeactivate.Clear();
		if (!(m_localSpellTable != null))
		{
			return;
		}
		for (int i2 = m_localSpellTable.m_Table.Count - 1; i2 >= 0; i2--)
		{
			Spell localSpell = m_localSpellTable.m_Table[i2].m_Spell;
			if (!(localSpell == null))
			{
				spellManager.ReleaseSpell(localSpell);
			}
		}
	}

	private void DestroyCreatedMaterials()
	{
		if (m_initialPremiumRibbonMaterial != null)
		{
			UnityEngine.Object.Destroy(m_initialPremiumRibbonMaterial);
			m_initialPremiumRibbonMaterial = null;
		}
	}

	private void DestroyLegendaryHeroPortrait()
	{
		if (LegendaryHeroPortrait != null)
		{
			LegendaryHeroPortrait.Dispose();
			LegendaryHeroPortrait = null;
		}
	}

	public virtual Actor Clone()
	{
		GameObject obj = UnityEngine.Object.Instantiate(base.gameObject, base.transform.position, base.transform.rotation);
		Actor actor = obj.GetComponent<Actor>();
		actor.SetEntity(m_entity);
		actor.SetEntityDef(m_entityDef);
		actor.SetCard(m_card);
		actor.SetPremium(m_premiumType);
		actor.SetWatermarkCardSetOverride(m_watermarkCardSetOverride);
		obj.transform.localScale = base.gameObject.transform.localScale;
		obj.transform.position = base.gameObject.transform.position;
		actor.SetActorState(m_actorState);
		if (m_shown)
		{
			actor.ShowImpl(ignoreSpells: false);
		}
		else
		{
			actor.HideImpl(ignoreSpells: false);
		}
		return actor;
	}

	public Card GetCard()
	{
		return m_card;
	}

	public void SetCard(Card card)
	{
		if (m_card == card)
		{
			return;
		}
		if (card == null)
		{
			m_card = null;
			base.transform.parent = null;
			OnSetCard?.Invoke();
			return;
		}
		m_card = card;
		OnSetCard?.Invoke();
		base.transform.parent = card.transform;
		TransformUtil.Identity(base.transform);
		if (m_rootObject != null)
		{
			TransformUtil.Identity(m_rootObject.transform);
		}
	}

	public void SetShowCostOverride(int showCost = 1)
	{
		m_showCostOverride = showCost;
	}

	public int ShowCostOverride()
	{
		return m_showCostOverride;
	}

	public DiamondRenderToTexture GetDiamondRenderToTexture()
	{
		return m_diamondRenderToTexture;
	}

	public void SetDiamondRenderToTexture(DiamondRenderToTexture diamondToRenderTexture)
	{
		m_diamondRenderToTexture = diamondToRenderTexture;
	}

	public string GetDiamondModelShown()
	{
		return m_diamondModelShown;
	}

	public void SetDiamondModelShown(string diamondModelShown)
	{
		m_diamondModelShown = diamondModelShown;
	}

	public GameObject GetDiamondModelObject()
	{
		return m_diamondModelObject;
	}

	public void SetDiamondModelObject(GameObject diamondModelObject)
	{
		m_diamondModelObject = diamondModelObject;
	}

	public bool GetPortraitMeshDirty()
	{
		return m_portraitMeshDirty;
	}

	public void SetPortraitMeshDirty(bool portraitMeshDirty)
	{
		m_portraitMeshDirty = portraitMeshDirty;
	}

	public void SetFullDefFromEntity(Entity entity)
	{
		if (entity != null)
		{
			SetEntityDef(entity.GetEntityDef());
			SetCardDefFromEntity(entity);
		}
	}

	public void SetFullDefFromActor(Actor other)
	{
		if (other != null)
		{
			SetCardDefFromActor(other);
			SetEntityDef(other.m_entityDef);
		}
	}

	public void SetFullDef(DefLoader.DisposableFullDef fullDef)
	{
		if (fullDef == null)
		{
			Log.Gameplay.PrintError("Actor.SetFullDef - fullDef is null");
			return;
		}
		SetCardDef(fullDef.DisposableCardDef);
		SetEntityDef(fullDef.EntityDef);
	}

	public DefLoader.DisposableCardDef ShareDisposableCardDef()
	{
		return m_cardDefHandle.Share();
	}

	public void SetCardDefFromEntity(Entity entity)
	{
		if (entity != null)
		{
			using (DefLoader.DisposableCardDef cardDef = entity.ShareDisposableCardDef())
			{
				SetCardDef(cardDef);
			}
		}
	}

	public void SetCardDefFromActor(Actor other)
	{
		if (other != null)
		{
			m_cardDefHandle.Set(other.m_cardDefHandle);
		}
	}

	public void SetCardDefFromCard(Card card)
	{
		if (!(card != null))
		{
			return;
		}
		using DefLoader.DisposableCardDef cardDef = card.ShareDisposableCardDef();
		if (m_cardDefHandle.SetCardDef(cardDef))
		{
			LoadArmorSpell();
		}
	}

	public void SetCardDef(DefLoader.DisposableCardDef cardDef)
	{
		if (m_cardDefHandle.SetCardDef(cardDef))
		{
			LoadArmorSpell();
			TryLoadLegendaryArt(cardDef);
		}
	}

	private bool TryLoadLegendaryArt(DefLoader.DisposableCardDef cardDef)
	{
		if (cardDef == null)
		{
			return false;
		}
		LoadCustomFrame(cardDef.CardDef);
		Player.Side playerSide = m_entity?.GetControllerSide() ?? Player.Side.NEUTRAL;
		UpdateLegendaryCardArt(cardDef.CardDef, playerSide);
		return true;
	}

	public void ReleaseCardDef()
	{
		m_cardDefHandle.ReleaseCardDef();
	}

	public void SetIgnoreHideStats(bool ignore)
	{
		m_ignoreHideStats = ignore;
	}

	public void EnableAlternateCostTextPosition(bool enable)
	{
		m_useAlternateCostTextPos = enable;
	}

	private bool HasHideStats(EntityBase entity)
	{
		if (m_ignoreHideStats)
		{
			return false;
		}
		if (!entity.HasTag(GAME_TAG.HIDE_STATS))
		{
			return entity.IsDormant();
		}
		return true;
	}

	public void SetWatermarkCardSetOverride(TAG_CARD_SET cardSetOverride)
	{
		if (!Enum.IsDefined(typeof(TAG_CARD_SET), cardSetOverride))
		{
			m_watermarkCardSetOverride = TAG_CARD_SET.INVALID;
		}
		else
		{
			m_watermarkCardSetOverride = cardSetOverride;
		}
	}

	public Entity GetEntity()
	{
		return m_entity;
	}

	public void SetEntity(Entity entity)
	{
		m_entity = entity;
		if (m_entity != null)
		{
			SetPremium(m_entity.GetPremiumType());
			SetWatermarkCardSetOverride(m_entity.GetWatermarkCardSetOverride());
		}
	}

	public EntityDef GetEntityDef()
	{
		return m_entityDef;
	}

	public void SetEntityDef(EntityDef entityDef)
	{
		m_entityDef = entityDef;
		if (m_entityDef == null)
		{
			return;
		}
		string entityCardId = m_entityDef.GetCardId();
		m_cardDefHandle.SetCardId(entityCardId);
		using DefLoader.DisposableCardDef entityCardDef = DefLoader.Get()?.GetCardDef(entityCardId);
		SetCardDef(entityCardDef);
	}

	public virtual void SetPremium(TAG_PREMIUM premium)
	{
		m_premiumType = premium;
	}

	public TAG_PREMIUM GetPremium()
	{
		return m_premiumType;
	}

	public TAG_CARD_SET GetCardSet()
	{
		if (m_entityDef == null && m_entity == null)
		{
			return TAG_CARD_SET.NONE;
		}
		if (m_entityDef != null)
		{
			return m_entityDef.GetCardSet();
		}
		return m_entity.GetCardSet();
	}

	public ActorStateType GetActorStateType()
	{
		if (!(m_actorStateMgr == null))
		{
			return m_actorStateMgr.GetActiveStateType();
		}
		return ActorStateType.NONE;
	}

	public void SetActorState(ActorStateType stateType)
	{
		m_actorState = stateType;
		if (!(m_actorStateMgr == null))
		{
			if (forceIdleState)
			{
				m_actorState = ActorStateType.CARD_IDLE;
			}
			ActorStateType prevState = m_actorStateMgr.GetActiveStateType();
			m_actorStateMgr.ChangeState(m_actorState);
			UpdateTitanUsableAbilityGlow(prevState, m_actorState);
		}
	}

	public void ToggleForceIdle(bool bOn)
	{
		forceIdleState = bOn;
	}

	public void TurnOffCollider()
	{
		ToggleCollider(enabled: false);
	}

	public void TurnOnCollider()
	{
		ToggleCollider(enabled: true);
	}

	public void ToggleCollider(bool enabled)
	{
		MeshRenderer meshRenderer = GetMeshRenderer();
		if (!(meshRenderer == null) && meshRenderer.TryGetComponent<Collider>(out var collider))
		{
			collider.enabled = enabled;
		}
	}

	public bool IsColliderEnabled()
	{
		MeshRenderer meshRenderer = GetMeshRenderer();
		if (meshRenderer == null || !meshRenderer.TryGetComponent<Collider>(out var collider))
		{
			return false;
		}
		return collider.enabled;
	}

	public TAG_RARITY GetRarity()
	{
		if (m_entityDef != null)
		{
			return m_entityDef.GetRarity();
		}
		if (m_entity != null)
		{
			return m_entity.GetRarity();
		}
		return TAG_RARITY.FREE;
	}

	public bool IsElite()
	{
		if (IsLettuceMercenary() || IsLettuceAbility())
		{
			return false;
		}
		if (m_entityDef != null)
		{
			return m_entityDef.IsElite();
		}
		if (m_entity != null)
		{
			return m_entity.IsElite();
		}
		if (m_isDiamondViewer)
		{
			return true;
		}
		return false;
	}

	public bool IsLettuceMercenary()
	{
		if (m_entityDef != null)
		{
			return m_entityDef.IsLettuceMercenary();
		}
		if (m_entity != null)
		{
			return m_entity.IsLettuceMercenary();
		}
		return false;
	}

	public bool IsLettuceAbility()
	{
		if (m_entityDef != null)
		{
			return m_entityDef.IsLettuceAbility();
		}
		if (m_entity != null)
		{
			return m_entity.IsLettuceAbility();
		}
		return false;
	}

	public bool IsMultiClass()
	{
		if (m_entityDef != null)
		{
			return m_entityDef.IsMultiClass();
		}
		if (m_entity != null)
		{
			return m_entity.IsMultiClass();
		}
		return false;
	}

	public List<TAG_CLASS> GetClasses()
	{
		List<TAG_CLASS> classes = new List<TAG_CLASS>();
		if (m_entityDef != null)
		{
			m_entityDef.GetClasses(classes);
		}
		else if (m_entity != null)
		{
			m_entity.GetClasses(classes);
		}
		return classes;
	}

	public GAME_TAG GetHearthstoneFaction()
	{
		if (m_entityDef != null)
		{
			foreach (GAME_TAG checkFaciton in VALID_FACTIONS)
			{
				if (m_entityDef.HasTag(checkFaciton))
				{
					return checkFaciton;
				}
			}
		}
		if (m_entity != null)
		{
			foreach (GAME_TAG checkFaciton2 in VALID_FACTIONS)
			{
				if (m_entity.HasTag(checkFaciton2))
				{
					return checkFaciton2;
				}
			}
		}
		return GAME_TAG.TAG_NOT_SET;
	}

	public bool HasHearthstoneFaction()
	{
		return GetHearthstoneFaction() != GAME_TAG.TAG_NOT_SET;
	}

	public bool HasDeckAction()
	{
		if (!IsTradeable())
		{
			return IsForgeable();
		}
		return true;
	}

	public bool IsTradeable()
	{
		if (m_entityDef != null)
		{
			return m_entityDef.IsTradeable();
		}
		if (m_entity != null)
		{
			return m_entity.IsTradeable();
		}
		return false;
	}

	public bool IsForgeable()
	{
		if (m_entityDef != null)
		{
			return m_entityDef.IsForgeable();
		}
		if (m_entity != null)
		{
			return m_entity.IsForgeable();
		}
		return false;
	}

	public bool IsLocation()
	{
		if (m_entityDef != null)
		{
			return m_entityDef.IsLocation();
		}
		if (m_entity != null)
		{
			return m_entity.IsLocation();
		}
		return false;
	}

	public bool HasRuneCost()
	{
		if (m_entityDef != null)
		{
			return m_entityDef.HasRuneCost;
		}
		if (m_entity != null)
		{
			return m_entity.HasRuneCost;
		}
		return false;
	}

	public void SetHiddenStandIn(GameObject standIn)
	{
		m_hiddenCardStandIn = standIn;
	}

	public GameObject GetHiddenStandIn()
	{
		return m_hiddenCardStandIn;
	}

	public void SetShadowform(bool shadowform)
	{
		m_shadowform = shadowform;
	}

	public UberShaderController GetUberShaderController()
	{
		if (m_uberShaderController == null)
		{
			m_uberShaderController = m_portraitMesh.GetComponent<UberShaderController>();
		}
		return m_uberShaderController;
	}

	public void SetIgnoreGameEntity(bool ignore)
	{
		m_ignoreGameEntity = ignore;
	}

	protected GameEntity GetGameEntityIfAllowed()
	{
		if (m_ignoreGameEntity)
		{
			return null;
		}
		return GameState.Get()?.GetGameEntity();
	}

	public void UpdateCustomFrameLightingBlend(float lightingBlend)
	{
		if (m_customFrameController != null)
		{
			m_customFrameController.SetLightingBlendOnSubRenderers(lightingBlend);
		}
	}

	public LoanerFlag GetLoanerFlag()
	{
		return m_loanerFlag;
	}

	public void SetVisibility(bool isVisible, bool isInternal)
	{
		SetVisibility(isVisible, ignoreSpells: false, isInternal);
	}

	protected void SetVisibility(bool isVisible, bool ignoreSpells, bool isInternal)
	{
		if (isVisible != m_shown)
		{
			if (!isInternal)
			{
				IsDesiredHidden = !isVisible;
			}
			m_shown = isVisible;
			if (isVisible)
			{
				ShowImpl(ignoreSpells);
			}
			else
			{
				HideImpl(ignoreSpells);
			}
		}
	}

	public bool IsShown()
	{
		return m_shown;
	}

	public void Show()
	{
		SetVisibility(isVisible: true, ignoreSpells: false, isInternal: false);
	}

	public void Show(bool ignoreSpells)
	{
		SetVisibility(isVisible: true, ignoreSpells, isInternal: false);
	}

	public void ShowSpellTable()
	{
		if (m_ownedSpells != null)
		{
			foreach (Spell spell in m_ownedSpells.Values)
			{
				if (spell != null)
				{
					spell.Show();
				}
			}
		}
		if (m_localSpellTable != null)
		{
			m_localSpellTable.Show();
		}
	}

	public void Hide()
	{
		SetVisibility(isVisible: false, ignoreSpells: false, isInternal: false);
	}

	public void Hide(bool ignoreSpells)
	{
		SetVisibility(isVisible: false, ignoreSpells, isInternal: false);
	}

	public bool IsOnRightSideOfZonePlay()
	{
		GameState gameState = GameState.Get();
		if (gameState == null)
		{
			return false;
		}
		ZonePlay zonePlay = null;
		Player player = gameState.GetFriendlySidePlayer();
		if (player != null)
		{
			zonePlay = player.GetBattlefieldZone();
		}
		else
		{
			player = gameState.GetOpposingSidePlayer();
			if (player != null)
			{
				zonePlay = player.GetBattlefieldZone();
			}
		}
		if (zonePlay == null)
		{
			return false;
		}
		MeshRenderer myRenderer = GetMeshRenderer();
		if (myRenderer == null)
		{
			return false;
		}
		float zoneCenter = zonePlay.GetComponent<BoxCollider>().bounds.center.x;
		return myRenderer.bounds.center.x > zoneCenter + myRenderer.bounds.size.x / 2f;
	}

	public void HideSpellTable()
	{
		if (m_ownedSpells != null)
		{
			foreach (Spell spell in m_ownedSpells.Values)
			{
				if (spell != null && spell.GetSpellType() != 0)
				{
					spell.Hide();
				}
			}
		}
		if (m_localSpellTable != null)
		{
			m_localSpellTable.Hide();
		}
	}

	protected virtual void ShowImpl(bool ignoreSpells)
	{
		if (m_rootObject != null)
		{
			m_rootObject.SetActive(value: true);
		}
		if ((bool)m_diamondRenderToTexture)
		{
			m_diamondRenderToTexture.enabled = true;
		}
		ShowArmorSpell();
		UpdateAllComponents();
		if ((bool)m_projectedShadow)
		{
			m_projectedShadow.enabled = true;
		}
		if (m_actorStateMgr != null)
		{
			m_actorStateMgr.ShowStateMgr();
		}
		if (!ignoreSpells)
		{
			ShowSpellTable();
		}
		if (m_ghostCardGameObject != null)
		{
			m_ghostCardGameObject.SetActive(value: true);
		}
		HighlightState highlightState = GetComponentInChildren<HighlightState>();
		if ((bool)highlightState)
		{
			highlightState.Show();
		}
	}

	protected virtual void HideImpl(bool ignoreSpells)
	{
		if (m_rootObject != null)
		{
			m_rootObject.SetActive(value: false);
		}
		UpdateContactShadow();
		HideArmorSpell();
		if (m_actorStateMgr != null)
		{
			m_actorStateMgr.HideStateMgr();
		}
		if ((bool)m_projectedShadow)
		{
			m_projectedShadow.enabled = false;
		}
		if (m_ghostCardGameObject != null)
		{
			m_ghostCardGameObject.SetActive(value: false);
		}
		if (!ignoreSpells)
		{
			HideSpellTable();
		}
		if (m_missingCardEffect != null)
		{
			UpdateMissingCardArt();
		}
		if ((bool)m_diamondRenderToTexture)
		{
			m_diamondRenderToTexture.enabled = false;
		}
		HighlightState highlightState = GetComponentInChildren<HighlightState>();
		if ((bool)highlightState)
		{
			highlightState.Hide();
		}
		if (m_baconCoinObject != null)
		{
			m_baconCoinObject.SetActive(value: false);
		}
	}

	public ActorStateMgr GetActorStateMgr()
	{
		return m_actorStateMgr;
	}

	public Collider GetCollider()
	{
		if (GetMeshRenderer() == null)
		{
			return null;
		}
		return GetMeshRenderer().gameObject.GetComponent<Collider>();
	}

	public GameObject GetRootObject()
	{
		return m_rootObject;
	}

	public MeshRenderer GetMeshRenderer(bool getPortrait = false)
	{
		if (m_premiumType == TAG_PREMIUM.DIAMOND)
		{
			if (getPortrait)
			{
				return m_meshRendererPortrait;
			}
			return m_meshRenderer;
		}
		return m_meshRenderer;
	}

	public GameObject GetBones()
	{
		return m_bones;
	}

	public UberText GetPowersTextObject()
	{
		return m_powersTextMesh;
	}

	public UberText GetBGQuestPowersText()
	{
		return m_bgQuestPowerTextMesh;
	}

	public UberText GetBGQuestRaceText()
	{
		return m_bgQuestRaceTextMesh;
	}

	public UberText GetRaceText()
	{
		return m_raceTextMesh;
	}

	public UberText GetNameText()
	{
		return m_nameTextMesh;
	}

	public Light GetHeroSpotlight()
	{
		if (m_heroSpotLight == null)
		{
			return null;
		}
		return m_heroSpotLight.GetComponent<Light>();
	}

	public GameObject FindBone(string boneName)
	{
		if (m_bones == null)
		{
			return null;
		}
		return GameObjectUtils.FindChildBySubstring(m_bones, boneName);
	}

	public GameObject GetCardTypeBannerAnchor()
	{
		if (m_cardTypeAnchorObject == null)
		{
			return base.gameObject;
		}
		return m_cardTypeAnchorObject;
	}

	public UberText GetAttackText()
	{
		return m_attackTextMesh;
	}

	public GameObject GetAttackTextObject()
	{
		if (m_attackTextMesh == null)
		{
			return null;
		}
		return m_attackTextMesh.gameObject;
	}

	public GemObject GetAttackObject()
	{
		if (m_attackObject == null)
		{
			return null;
		}
		return m_attackObject.GetComponent<GemObject>();
	}

	public GemObject GetHealthObject()
	{
		if (m_healthObject == null)
		{
			return null;
		}
		return m_healthObject.GetComponent<GemObject>();
	}

	public Widget GetAmountBannerWidget()
	{
		return m_amountBannerWidget;
	}

	public GameObject GetWeaponShields()
	{
		if (m_healthObject != null && m_healthObject.GetComponent<GemObject>() == null)
		{
			return m_healthObject;
		}
		return null;
	}

	public GameObject GetWeaponSwords()
	{
		if (m_attackObject != null && m_attackObject.GetComponent<GemObject>() == null)
		{
			return m_attackObject;
		}
		return null;
	}

	public GemObject GetArmorObject()
	{
		if (m_armorObject == null)
		{
			return null;
		}
		return m_armorObject.GetComponent<GemObject>();
	}

	public UberText GetHealthText()
	{
		return m_healthTextMesh;
	}

	public GameObject GetHealthTextObject()
	{
		if (m_healthTextMesh == null)
		{
			return null;
		}
		return m_healthTextMesh.gameObject;
	}

	public UberText GetCostText()
	{
		if (m_costTextMesh == null)
		{
			return null;
		}
		return m_costTextMesh;
	}

	public GameObject GetCostTextObject()
	{
		if (m_costTextMesh == null)
		{
			return null;
		}
		return m_costTextMesh.gameObject;
	}

	public UberText GetSecretText()
	{
		return m_secretText;
	}

	public virtual void UpdateAllComponents(bool needsGhostUpdate = true)
	{
		if (!m_isDiamondViewer)
		{
			UpdateTextComponents();
			UpdateMaterials();
			UpdateTextures();
			UpdateCardBack();
			UpdateMeshComponents();
			UpdateRootObjectSpellComponents();
			UpdateMissingCardArt();
			if (needsGhostUpdate)
			{
				UpdateGhostCardEffect();
			}
			UpdateDiamondCardArt();
			Player.Side playerSide = m_entity?.GetControllerSide() ?? Player.Side.NEUTRAL;
			UpdateLegendaryCardArt(m_cardDefHandle.Get(m_premiumType), playerSide);
			UpdatePortraitMaterialAnimation();
			UpdateContactShadow();
			UpdateLettuceMinionInPlayFrame();
			UpdateTitanComponents();
		}
		if (PlatformSettings.OS == OSCategory.Mac && (bool)m_nameTextMesh)
		{
			DelayedUpdateNameText(m_updateTokenSource.Token).Forget();
		}
	}

	private async UniTaskVoid DelayedUpdateNameText(CancellationToken token)
	{
		await UniTask.Yield(PlayerLoopTiming.Update, token);
		if ((bool)m_nameTextMesh)
		{
			m_nameTextMesh.UpdateNow();
		}
	}

	public void UpdatePortraitFrameVisibility(bool visible)
	{
		if (m_portraitFrameObject.activeSelf != visible)
		{
			m_portraitFrameObject.SetActive(visible);
			UpdateAllComponents();
		}
	}

	public bool MissingCardEffect(bool refreshOnFocus = true, bool updateComponents = true)
	{
		if ((bool)m_missingCardEffect)
		{
			RenderToTexture r2t = m_missingCardEffect.GetComponent<RenderToTexture>();
			if ((bool)r2t)
			{
				r2t.DontRefreshOnFocus = !refreshOnFocus;
				m_initialMissingCardRenderQueue = r2t.m_RenderQueue;
				m_missingcard = true;
				if (updateComponents)
				{
					UpdateAllComponents();
				}
				return true;
			}
		}
		return false;
	}

	public void DisableMissingCardEffect()
	{
		m_missingcard = false;
		if ((bool)m_missingCardEffect)
		{
			RenderToTexture r2t = m_missingCardEffect.GetComponent<RenderToTexture>();
			if ((bool)r2t)
			{
				r2t.enabled = false;
			}
			UpdateAllComponents();
			MaterialShaderAnimation(animationEnabled: true);
		}
	}

	public void UpdateMissingCardArt()
	{
		if (!m_missingcard || m_missingCardEffect == null)
		{
			return;
		}
		RenderToTexture r2t = m_missingCardEffect.GetComponent<RenderToTexture>();
		if (r2t == null)
		{
			return;
		}
		if (m_rootObject.activeSelf)
		{
			MaterialShaderAnimation(animationEnabled: false);
			if (SceneMgr.Get() != null && SceneMgr.Get().GetMode() == SceneMgr.Mode.COLLECTIONMANAGER)
			{
				TAG_PREMIUM premiumType = GetPremium();
				bool isWildTheme = CollectionManager.Get().GetThemeShowing() == FormatType.FT_WILD;
				if (premiumType == TAG_PREMIUM.GOLDEN)
				{
					if (isWildTheme)
					{
						r2t.m_Material.color = MISSING_CARD_WILD_GOLDEN_COLOR;
					}
					else
					{
						r2t.m_Material.color = MISSING_CARD_STANDARD_GOLDEN_COLOR;
					}
				}
				else if (premiumType == TAG_PREMIUM.DIAMOND && isWildTheme)
				{
					Material material = r2t.m_Material;
					material.color = MISSING_CARD_WILD_DIAMOND_COLOR;
					material.SetFloat(MISSING_CARD_WILD_DIAMOND_CONTRAST_KEY, MISSING_CARD_WILD_DIAMOND_CONTRAST);
					material.SetFloat(MISSING_CARD_WILD_DIAMOND_INTENSITY_KEY, MISSING_CARD_WILD_DIAMOND_INTENSITY);
				}
			}
			r2t.enabled = true;
			r2t.Show(render: true);
		}
		else
		{
			r2t.enabled = false;
			r2t.Hide();
		}
	}

	public void SetMissingCardMaterial(Material missingCardMat)
	{
		if (m_missingCardEffect == null || missingCardMat == null)
		{
			return;
		}
		RenderToTexture r2t = m_missingCardEffect.GetComponent<RenderToTexture>();
		if (r2t == null)
		{
			return;
		}
		r2t.m_Material = missingCardMat;
		if (m_rootObject.activeSelf)
		{
			MaterialShaderAnimation(animationEnabled: false);
			if (r2t.enabled)
			{
				r2t.Render();
			}
		}
	}

	public bool isMissingCard()
	{
		if (m_missingCardEffect == null)
		{
			return false;
		}
		RenderToTexture r2t = m_missingCardEffect.GetComponent<RenderToTexture>();
		if (r2t == null)
		{
			return false;
		}
		return r2t.enabled;
	}

	public void SetMissingCardRenderQueue(bool reset, int renderQueue)
	{
		RenderToTexture r2t = m_missingCardEffect.GetComponent<RenderToTexture>();
		if (!(r2t == null))
		{
			r2t.m_RenderQueue = (reset ? m_initialMissingCardRenderQueue : renderQueue);
		}
	}

	public void GhostCardEffect(GhostCard.Type ghostType, TAG_PREMIUM premium = TAG_PREMIUM.NORMAL, bool update = true)
	{
		if (m_ghostCard != ghostType || m_ghostPremium != premium)
		{
			m_ghostCard = ghostType;
			m_ghostPremium = premium;
			if (update)
			{
				UpdateAllComponents();
			}
		}
	}

	private void UpdateGhostCardEffect(bool RTTUpdateOnly = false)
	{
		if (m_ghostCardGameObject == null)
		{
			return;
		}
		GhostCard ghostCard = m_ghostCardGameObject.GetComponent<GhostCard>();
		if (ghostCard == null)
		{
			return;
		}
		if (m_ghostCard != 0)
		{
			if (RTTUpdateOnly)
			{
				ghostCard.SetRTTDirty();
				return;
			}
			ghostCard.SetGhostType(m_ghostCard);
			ghostCard.SetPremium(m_ghostPremium);
			ghostCard.RenderGhostCard();
		}
		else
		{
			ghostCard.DisableGhost();
		}
	}

	public bool isGhostCard()
	{
		if (m_ghostCard != 0)
		{
			return m_ghostCardGameObject;
		}
		return false;
	}

	public bool DoesDiamondModelExistOnCardDef()
	{
		CardDef cardDefHandle = m_cardDefHandle.Get(m_premiumType);
		if (cardDefHandle == null)
		{
			return false;
		}
		return !string.IsNullOrEmpty(cardDefHandle.m_DiamondModel);
	}

	public bool IsEntityStateBadForDiamondVisuals()
	{
		if (GameState.Get() != null && !GameState.Get().AllowDiamondCards())
		{
			return true;
		}
		GetEntity();
		if (m_entity == null)
		{
			return false;
		}
		bool num = m_entity.HasTag(GAME_TAG.FROZEN);
		bool isReborn = m_entity.HasTag(GAME_TAG.REBORN);
		bool isStealth = m_entity.HasTag(GAME_TAG.STEALTH);
		bool isDormant = m_entity.HasTag(GAME_TAG.DORMANT);
		bool isEnraged = m_entity.HasTag(GAME_TAG.ENRAGED);
		bool isUntargetable = m_entity.HasTag(GAME_TAG.CANT_BE_TARGETED_BY_SPELLS) && m_entity.HasTag(GAME_TAG.CANT_BE_TARGETED_BY_HERO_POWERS);
		bool isMortallyWounded = m_entity.HasTag(GAME_TAG.IS_VAMPIRE);
		Card card = GetCard();
		if (card != null)
		{
			Spell dormantSpell = card.GetActorSpell(SpellType.DORMANT, loadIfNeeded: false);
			if (dormantSpell != null && dormantSpell.GetActiveState() != 0)
			{
				isDormant = true;
			}
		}
		bool isDead = false;
		if (m_card != null && m_card.GetZone() is ZoneGraveyard)
		{
			isDead = true;
		}
		return num || isReborn || isStealth || isDormant || isEnraged || isUntargetable || isDead || isMortallyWounded;
	}

	public bool IsEntityStateBadForHeroDiamondVisuals()
	{
		if (GameState.Get() != null && !GameState.Get().AllowDiamondCards())
		{
			return true;
		}
		if (m_missingcard)
		{
			return true;
		}
		GetEntity();
		if (m_entity == null)
		{
			CustomFrameDiamondPrefab diamondFrame = m_customFrameController.GetDiamondFramePrefab();
			if (diamondFrame != null && diamondFrame.m_loading)
			{
				return true;
			}
			return false;
		}
		bool num = m_entity.HasTag(GAME_TAG.ENRAGED);
		bool isMortallyWounded = m_entity.HasTag(GAME_TAG.IS_VAMPIRE);
		bool isHeavilyArmored = m_entity.HasTag(GAME_TAG.HEAVILY_ARMORED);
		bool isImmune = m_entity.HasTag(GAME_TAG.IMMUNE);
		bool isElusive = m_entity.HasTag(GAME_TAG.ELUSIVE);
		bool isDivineShield = m_entity.HasTag(GAME_TAG.DIVINE_SHIELD);
		bool isStealth = m_entity.HasTag(GAME_TAG.STEALTH);
		bool isFrozen = m_entity.HasTag(GAME_TAG.FROZEN);
		bool isShadowform = m_entity.HasTag(GAME_TAG.SHADOWFORM);
		bool isDormant = m_entity.HasTag(GAME_TAG.DORMANT);
		Card card = GetCard();
		if (card != null)
		{
			Spell dormantSpell = card.GetActorSpell(SpellType.DORMANT, loadIfNeeded: false);
			if (dormantSpell != null && dormantSpell.GetActiveState() != 0)
			{
				isDormant = true;
			}
		}
		bool isDead = false;
		if (m_card != null && m_card.GetZone() is ZoneGraveyard)
		{
			isDead = true;
		}
		return num || isMortallyWounded || isHeavilyArmored || isImmune || isElusive || isDivineShield || isStealth || isFrozen || isShadowform || isDormant || isDead;
	}

	public void LoadDiamondCardMesh(GameObject goMeshRTT, AssetReference planeRef)
	{
		if (planeRef == null)
		{
			return;
		}
		MeshFilter meshFilter = goMeshRTT.GetComponent<MeshFilter>();
		if (!(meshFilter != null) || planeRef == null)
		{
			return;
		}
		using AssetHandle<Mesh> plane = AssetLoader.Get().LoadAsset<Mesh>(planeRef);
		if (plane != null)
		{
			meshFilter.sharedMesh = plane;
		}
	}

	public void UpdateDiamondCardArt()
	{
		UpdateDiamondHeroCardArt();
		if (m_premiumType != TAG_PREMIUM.DIAMOND)
		{
			return;
		}
		if (m_portraitMesh != null && m_portraitMeshRTT != null)
		{
			bool num = IsEntityStateBadForDiamondVisuals();
			bool hasDiamondModel = DoesDiamondModelExistOnCardDef();
			if (num || !hasDiamondModel)
			{
				m_portraitMesh.SetActive(value: true);
				m_portraitMeshRTT.SetActive(value: false);
			}
			else
			{
				m_portraitMesh.SetActive(value: false);
				m_portraitMeshRTT.SetActive(value: true);
			}
		}
		if (m_cardDefHandle.Get(m_premiumType) == null)
		{
			return;
		}
		if (DoesDiamondModelExistOnCardDef() && m_rootObject != null)
		{
			bool hasDiamondModelObject = m_diamondModelObject != null;
			string expectedDiamondModel = m_cardDefHandle.Get(m_premiumType).m_DiamondModel;
			if ((bool)m_diamondPortraitR2T && !m_diamondRenderToTexture)
			{
				m_diamondRenderToTexture = m_diamondPortraitR2T.GetComponent<DiamondRenderToTexture>();
			}
			if (hasDiamondModelObject && expectedDiamondModel != m_diamondModelShown)
			{
				UnityEngine.Object.Destroy(m_diamondModelObject);
				m_diamondModelObject = null;
				hasDiamondModelObject = false;
				if ((bool)m_diamondRenderToTexture)
				{
					m_diamondRenderToTexture.enabled = false;
				}
			}
			if (!hasDiamondModelObject)
			{
				m_diamondModelObject = AssetLoader.Get().InstantiatePrefab(expectedDiamondModel, AssetLoadingOptions.IgnorePrefabPosition);
				m_diamondModelShown = expectedDiamondModel;
				if (m_diamondModelObject != null)
				{
					m_diamondModelObject.transform.parent = m_rootObject.transform;
					if ((bool)m_diamondRenderToTexture)
					{
						m_diamondRenderToTexture.m_ObjectToRender = m_diamondModelObject;
						m_diamondRenderToTexture.m_ClearColor = m_cardDefHandle.Get(m_premiumType).m_DiamondPlaneRTT_CearColor;
					}
					m_portraitMeshDirty = true;
				}
			}
			else if ((bool)m_diamondRenderToTexture)
			{
				m_diamondRenderToTexture.UpdateMaterialBlend(m_usePlayPortrait);
			}
			else
			{
				m_diamondModelObject.SetActive(value: false);
			}
		}
		if (m_portraitMeshDirty && m_portraitMeshRTT != null && m_portraitMeshRTT_background != null)
		{
			LoadDiamondCardMesh(m_portraitMeshRTT, GetDiamondPlaneRef(background: false));
			LoadDiamondCardMesh(m_portraitMeshRTT_background, GetDiamondPlaneRef(background: true));
			AssetReference diamondBackgroundTextureRef = m_cardDefHandle.Get(m_premiumType).m_DiamondPortraitTexturePath;
			Renderer portraitMeshBackgroundRenderer = m_portraitMeshRTT_background.GetComponent<Renderer>();
			if (portraitMeshBackgroundRenderer != null && portraitMeshBackgroundRenderer.GetSharedMaterial().HasProperty("_MainTex") && diamondBackgroundTextureRef != null)
			{
				using AssetHandle<Texture2D> diamondBackgroundTexture = AssetLoader.Get().LoadAsset<Texture2D>(diamondBackgroundTextureRef);
				if (diamondBackgroundTexture != null)
				{
					GetMaterialInstance(portraitMeshBackgroundRenderer).SetTexture("_MainTex", (Texture2D)diamondBackgroundTexture);
				}
			}
			HighlightState highlightState = GetComponentInChildren<HighlightState>();
			if (highlightState != null && highlightState.isActiveAndEnabled)
			{
				highlightState.ContinuousUpdate(0.1f);
			}
			m_portraitMeshDirty = false;
		}
		if ((bool)m_diamondRenderToTexture)
		{
			m_diamondRenderToTexture.enabled = m_shown;
		}
		SetUnlit();
		if (!DoesDiamondModelExistOnCardDef() && m_diamondModelObject != null)
		{
			UnityEngine.Object.Destroy(m_diamondModelObject);
			m_diamondModelObject = null;
		}
		if (m_diamondModelObject == null && m_diamondPortraitR2T != null && (bool)m_diamondRenderToTexture && m_diamondRenderToTexture.enabled)
		{
			m_diamondRenderToTexture.enabled = false;
		}
		DiamondCardArtUpdatedCallback?.Invoke();
	}

	public void UpdateDiamondHeroCardArt()
	{
		if (m_customFrameController != null)
		{
			bool isBadDiamondState = IsEntityStateBadForHeroDiamondVisuals();
			m_customFrameController.ToggleDiamondRTTVisibility(!isBadDiamondState);
			if (m_missingcard && m_customFrameController.MissingDiamondPortraitMat != null)
			{
				Texture portraitTexture = GetPortraitMaterial().mainTexture;
				SetPortraitMaterial(m_customFrameController.MissingDiamondPortraitMat);
				SetPortraitTexture(portraitTexture);
			}
		}
	}

	public void UpdateLegendaryCardArt(CardDef cardDef, Player.Side side)
	{
		if (cardDef == null)
		{
			return;
		}
		string expectedLegendaryModel = cardDef.m_LegendaryModel;
		if (m_missingcard || isGhostCard())
		{
			expectedLegendaryModel = null;
		}
		if (!string.IsNullOrEmpty(expectedLegendaryModel))
		{
			if (LegendaryHeroPortrait != null && !LegendaryHeroPortrait.IsValidForPath(expectedLegendaryModel, side))
			{
				DestroyLegendaryHeroPortrait();
			}
			if (LegendaryHeroPortrait == null)
			{
				LegendaryHeroRenderToTextureService service = ServiceManager.Get<LegendaryHeroRenderToTextureService>();
				if (service != null)
				{
					LegendaryHeroPortrait = service.CreatePortrait(expectedLegendaryModel, side);
					LegendaryHeroPortrait.AttachToActor(this);
					m_portraitMeshDirty = true;
				}
			}
		}
		if (string.IsNullOrEmpty(expectedLegendaryModel) && LegendaryHeroPortrait != null)
		{
			DestroyLegendaryHeroPortrait();
		}
		if (m_portraitMeshDirty && LegendaryHeroPortrait != null)
		{
			UpdateMaterials(cardDef);
			HighlightState highlightState = GetComponentInChildren<HighlightState>();
			if (highlightState != null && highlightState.isActiveAndEnabled)
			{
				highlightState.ContinuousUpdate(0.1f);
			}
			m_portraitMeshDirty = false;
		}
	}

	private AssetReference GetDiamondPlayMeshName(bool background)
	{
		if (background)
		{
			return m_cardDefHandle.Get(m_premiumType).m_DiamondBackground_Play;
		}
		return m_cardDefHandle.Get(m_premiumType).m_DiamondPlaneRTT_Play;
	}

	private AssetReference GetDiamondHandMeshName(bool background)
	{
		if (background)
		{
			return m_cardDefHandle.Get(m_premiumType).m_DiamondBackground_Hand;
		}
		return m_cardDefHandle.Get(m_premiumType).m_DiamondPlaneRTT_Hand;
	}

	private AssetReference GetDiamondPlaneRef(bool background)
	{
		AssetReference diamondPlaneRef = null;
		switch (m_portraitMode)
		{
		case PortraitMode.Default:
			diamondPlaneRef = ((!(m_card == null)) ? (m_usePlayPortrait ? GetDiamondPlayMeshName(background) : GetDiamondHandMeshName(background)) : GetDiamondHandMeshName(background));
			break;
		case PortraitMode.ForcedHandMode:
			diamondPlaneRef = GetDiamondHandMeshName(background);
			break;
		case PortraitMode.ForcedPlayMode:
			diamondPlaneRef = GetDiamondPlayMeshName(background);
			break;
		}
		return diamondPlaneRef;
	}

	public void SetDiamondPortraitMode(bool playMode, bool forced = false)
	{
		m_usePlayPortrait = playMode;
		if (forced)
		{
			m_portraitMode = (playMode ? PortraitMode.ForcedPlayMode : PortraitMode.ForcedHandMode);
		}
		else
		{
			m_portraitMode = PortraitMode.Default;
		}
	}

	public void UpdateMaterials(CardDef cardDef = null)
	{
		if (base.gameObject != null && base.gameObject.activeInHierarchy && m_updateTokenSource != null)
		{
			UpdatePortraitMaterials(m_updateTokenSource.Token, cardDef).Forget();
		}
		else
		{
			isPortraitMaterialDirty = true;
		}
	}

	public void OverrideAllMeshMaterials(Material material)
	{
		if (!(m_rootObject == null))
		{
			RecursivelyReplaceMaterialsList(m_rootObject.transform, material);
		}
	}

	public void SetUnlit()
	{
		SetLightBlend(0f, includeInactive: true);
	}

	public void SetLit()
	{
		SetLightBlend(1f, includeInactive: true);
	}

	public void SetLightBlend(float blendValue, bool includeInactive = false)
	{
		SetLightBlend(base.gameObject, blendValue, includeInactive);
		if (m_diamondPortraitR2T != null)
		{
			DiamondRenderToTexture renderToTextureComp = m_diamondPortraitR2T.GetComponent<DiamondRenderToTexture>();
			if (renderToTextureComp != null)
			{
				renderToTextureComp.UpdateMaterialBlend(blendValue);
			}
		}
	}

	private void SetLightBlend(GameObject go, float blendValue, bool includeInactive = false)
	{
		Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>(includeInactive);
		foreach (Renderer renderer in componentsInChildren)
		{
			if (!renderer.gameObject.activeInHierarchy)
			{
				DeferredEnableHandler.AttachTo(renderer, delegate
				{
					SetRendererLightBlend(renderer, blendValue);
				});
			}
			else
			{
				SetRendererLightBlend(renderer, blendValue);
			}
		}
		UberText[] componentsInChildren2 = go.GetComponentsInChildren<UberText>(includeInactive);
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].AmbientLightBlend = blendValue;
		}
	}

	private void SetRendererLightBlend(Renderer renderer, float blendValue)
	{
		foreach (Material mat in renderer.GetMaterials())
		{
			if (!(mat == null) && mat.HasProperty("_LightingBlend"))
			{
				mat.SetFloat("_LightingBlend", blendValue);
			}
		}
	}

	private void RecursivelyReplaceMaterialsList(Transform transformToRecurse, Material newMaterialPrefab)
	{
		bool canReplaceMeshMaterials = true;
		if (transformToRecurse.GetComponent<MaterialReplacementExclude>() != null)
		{
			canReplaceMeshMaterials = false;
		}
		else if (transformToRecurse.GetComponent<UberText>() != null)
		{
			canReplaceMeshMaterials = false;
		}
		else if (transformToRecurse.GetComponent<Renderer>() == null)
		{
			canReplaceMeshMaterials = false;
		}
		if (canReplaceMeshMaterials)
		{
			ReplaceMaterialsList(transformToRecurse.GetComponent<Renderer>(), newMaterialPrefab);
		}
		foreach (Transform child in transformToRecurse)
		{
			RecursivelyReplaceMaterialsList(child, newMaterialPrefab);
		}
	}

	private void ReplaceMaterialsList(Renderer renderer, Material newMaterialPrefab)
	{
		List<Material> oldMaterials = renderer.GetMaterials();
		int oldMaterialsCount = oldMaterials.Count;
		Material[] newMaterials = new Material[oldMaterialsCount];
		for (int i = 0; i < oldMaterialsCount; i++)
		{
			Material oldMaterial = oldMaterials[i];
			newMaterials[i] = CreateReplacementMaterial(oldMaterial, newMaterialPrefab);
		}
		renderer.SetMaterials(newMaterials);
		if (!(renderer != m_meshRenderer))
		{
			UpdatePortraitTexture();
		}
	}

	private Material CreateReplacementMaterial(Material oldMaterial, Material newMaterialPrefab)
	{
		Material material = UnityEngine.Object.Instantiate(newMaterialPrefab);
		material.mainTexture = oldMaterial.mainTexture;
		return material;
	}

	public void SeedMaterialEffects()
	{
		if (m_materialEffectsSeeded)
		{
			return;
		}
		m_materialEffectsSeeded = true;
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		float seed = UnityEngine.Random.Range(0f, 2f);
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			List<Material> sharedMaterials = renderer.GetSharedMaterials();
			if (sharedMaterials.Count == 1)
			{
				Material material = sharedMaterials[0];
				if (material != null && material.HasProperty("_Seed") && material.GetFloat("_Seed") == 0f)
				{
					GetMaterialInstance(renderer).SetFloat("_Seed", seed);
				}
				continue;
			}
			List<Material> mats = renderer.GetMaterials();
			if (mats == null || mats.Count == 0)
			{
				continue;
			}
			foreach (Material mat in mats)
			{
				if (!(mat == null) && mat.HasProperty("_Seed") && mat.GetFloat("_Seed") == 0f)
				{
					mat.SetFloat("_Seed", seed);
				}
			}
		}
	}

	public void MaterialShaderAnimation(bool animationEnabled)
	{
		if ((bool)m_diamondPortraitR2T)
		{
			return;
		}
		float timescale = 0f;
		if (animationEnabled)
		{
			timescale = 1f;
		}
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			foreach (Material mat in componentsInChildren[i].GetSharedMaterials())
			{
				if (!(mat == null) && mat.HasProperty("_TimeScale"))
				{
					mat.SetFloat("_TimeScale", timescale);
				}
			}
		}
	}

	public CardBackManager.CardBackSlot GetCardBackSlot()
	{
		if (m_cardBackSlotOverride.HasValue)
		{
			return m_cardBackSlotOverride.Value;
		}
		Player.Side playerSide = Player.Side.NEUTRAL;
		if (m_cardBackSideOverride.HasValue)
		{
			playerSide = m_cardBackSideOverride.Value;
		}
		else if (m_entity != null)
		{
			Player controller = m_entity.GetController();
			if (controller != null)
			{
				playerSide = controller.GetSide();
			}
		}
		SceneMgr sceneMgr = SceneMgr.Get();
		CardBackManager.CardBackSlot slot = ((sceneMgr == null || sceneMgr.GetMode() != SceneMgr.Mode.GAMEPLAY) ? CardBackManager.CardBackSlot.FAVORITE : CardBackManager.CardBackSlot.DEFAULT);
		switch (playerSide)
		{
		case Player.Side.FRIENDLY:
			slot = CardBackManager.CardBackSlot.FRIENDLY;
			break;
		case Player.Side.OPPOSING:
			slot = CardBackManager.CardBackSlot.OPPONENT;
			break;
		}
		return slot;
	}

	public void SetCardBackSideOverride(Player.Side? sideOverride)
	{
		m_cardBackSideOverride = sideOverride;
	}

	public void SetCardBackSlotOverride(CardBackManager.CardBackSlot? slotOverride)
	{
		m_cardBackSlotOverride = slotOverride;
	}

	public bool GetCardbackUpdateIgnore()
	{
		return m_ignoreUpdateCardback;
	}

	public void SetCardbackUpdateIgnore(bool ignoreUpdate)
	{
		m_ignoreUpdateCardback = ignoreUpdate;
	}

	public void UpdateCardBack()
	{
		if (m_ignoreUpdateCardback)
		{
			return;
		}
		CardBackManager cardBackMgr = CardBackManager.Get();
		if (cardBackMgr != null)
		{
			CardBackManager.CardBackSlot slot = GetCardBackSlot();
			UpdateCardBackDisplay(slot);
			UpdateCardBackDragEffect();
			if (!(m_cardMesh == null) && m_cardBackMatIdx >= 0 && !(m_initialCardBackMaterial == null))
			{
				Renderer cardMesh = m_cardMesh.GetComponent<Renderer>();
				cardMesh.SetSharedMaterial(m_cardBackMatIdx, m_initialCardBackMaterial);
				cardBackMgr.SetCardBackMaterial(cardMesh, m_cardBackMatIdx, slot);
			}
		}
	}

	public void EnableCardbackShadow(bool enabled)
	{
		CardBackDisplay cbDisplay = GetComponentInChildren<CardBackDisplay>(includeInactive: true);
		if (!(cbDisplay == null))
		{
			cbDisplay.EnableShadow(enabled);
		}
	}

	private void UpdateCardBackDragEffect()
	{
		if (SceneMgr.Get() != null && SceneMgr.Get().GetMode() == SceneMgr.Mode.GAMEPLAY)
		{
			CardBackDragEffect dragEffect = GetComponentInChildren<CardBackDragEffect>();
			if (!(dragEffect == null))
			{
				dragEffect.SetEffect();
			}
		}
	}

	private void UpdateCardBackDisplay(CardBackManager.CardBackSlot slot)
	{
		CardBackDisplay cbDisplay = GetComponentInChildren<CardBackDisplay>();
		if (!(cbDisplay == null))
		{
			cbDisplay.SetCardBack(slot);
		}
	}

	public void UpdateTextures()
	{
		UpdatePortraitTexture();
	}

	public void UpdatePortraitTexture()
	{
		bool useDynamicResolution = false;
		if (m_portraitTextureOverride != null && !UsesLegendaryPortraitForHistoryTile())
		{
			SetPortraitTexture(m_portraitTextureOverride);
		}
		else if (LegendaryHeroPortrait?.PortraitTexture != null)
		{
			SetPortraitTexture(LegendaryHeroPortrait.PortraitTexture);
			useDynamicResolution = true;
		}
		else if (m_cardDefHandle.Get(m_premiumType) != null)
		{
			SetPortraitTexture(m_cardDefHandle.Get(m_premiumType).GetPortraitTexture(m_premiumType));
		}
		if (useDynamicResolution)
		{
			ConnectLegendarySkinToDynamicResolutionController();
		}
		else
		{
			DisconnectLegendarySkinToDynamicResolutionController();
		}
	}

	public void SetPortraitTexture(Texture texture)
	{
		CardDef cardDef = m_cardDefHandle.Get(m_premiumType);
		if (!(cardDef != null) || (m_premiumType < TAG_PREMIUM.GOLDEN && !cardDef.m_AlwaysRenderPremiumPortrait) || ((m_premiumType != TAG_PREMIUM.SIGNATURE || !(cardDef.GetSignaturePortraitMaterial() != null)) && (!IsPremiumPortraitEnabled() || !(cardDef.GetPremiumPortraitMaterial() != null))))
		{
			Material portraitMaterial = GetPortraitMaterial();
			if (!(portraitMaterial == null))
			{
				portraitMaterial.mainTexture = texture;
				UpdateCustomFrameDiamondMaterial();
			}
		}
	}

	public void SetPortraitTextureOverride(Texture portrait)
	{
		m_portraitTextureOverride = portrait;
		UpdatePortraitTexture();
	}

	public Texture GetPortraitTexture()
	{
		Material portraitMaterial = GetPortraitMaterial();
		if (portraitMaterial == null)
		{
			return null;
		}
		return portraitMaterial.mainTexture;
	}

	public Texture GetStaticPortraitTexture()
	{
		return GetStaticPortraitTexture(m_premiumType);
	}

	public Texture GetStaticPortraitTexture(TAG_PREMIUM premiumType)
	{
		if (m_portraitTextureOverride != null)
		{
			return m_portraitTextureOverride;
		}
		return m_cardDefHandle.Get(premiumType).GetPortraitTexture(premiumType);
	}

	private async UniTaskVoid UpdatePortraitMaterials(CancellationToken token, CardDef alternativeCardDef)
	{
		isPortraitMaterialDirty = false;
		if (m_shadowform)
		{
			return;
		}
		CardDef cardDef = alternativeCardDef ?? m_cardDefHandle.Get(m_premiumType);
		if (!cardDef)
		{
			return;
		}
		TAG_PREMIUM portraitPremiumLevel = m_premiumType;
		if (cardDef.m_AlwaysRenderPremiumPortrait)
		{
			portraitPremiumLevel = TAG_PREMIUM.GOLDEN;
		}
		if (portraitPremiumLevel == TAG_PREMIUM.SIGNATURE || (portraitPremiumLevel == TAG_PREMIUM.GOLDEN && IsPremiumPortraitEnabled()) || (portraitPremiumLevel == TAG_PREMIUM.DIAMOND && IsPremiumPortraitEnabled()))
		{
			if (!cardDef.IsPremiumLoaded(m_premiumType))
			{
				CardTextureLoader.Load(cardDef, new CardPortraitQuality(3, m_premiumType));
				await UniTask.Yield(PlayerLoopTiming.Update, token);
				if ((alternativeCardDef ?? m_cardDefHandle.Get(m_premiumType)) != cardDef)
				{
					return;
				}
			}
			Material portraitMaterial = ((portraitPremiumLevel != TAG_PREMIUM.DIAMOND) ? cardDef.GetPortraitMaterial(portraitPremiumLevel) : cardDef.GetPortraitMaterial(TAG_PREMIUM.GOLDEN));
			if (portraitMaterial != null)
			{
				SetPortraitMaterial(portraitMaterial);
			}
			else if (m_initialPortraitMaterial != null)
			{
				SetPortraitMaterial(m_initialPortraitMaterial);
			}
		}
		else
		{
			Material portraitMaterial2 = cardDef.GetPortraitMaterial(portraitPremiumLevel);
			if (m_useCardDefMaterial && portraitMaterial2 != null)
			{
				SetPortraitMaterial(portraitMaterial2);
			}
			else
			{
				SetPortraitMaterial(m_initialPortraitMaterial);
			}
		}
		UpdatePortraitTexture();
		UpdateGhostCardEffect(RTTUpdateOnly: true);
		OnPortraitMaterialUpdated?.Invoke();
	}

	private void UpdatePortraitMaterialAnimation()
	{
		if (m_portraitMesh == null)
		{
			return;
		}
		CardDef cardDef = m_cardDefHandle.Get(m_premiumType);
		if (cardDef == null)
		{
			return;
		}
		UberShaderAnimation portraitAnimation = cardDef.GetPortraitAnimation(m_premiumType);
		if (portraitAnimation == null)
		{
			return;
		}
		m_uberShaderController = m_portraitMesh.GetComponent<UberShaderController>();
		if (m_uberShaderController == null)
		{
			m_uberShaderController = m_portraitMesh.gameObject.AddComponent<UberShaderController>();
			m_uberShaderController.UberShaderAnimation = UnityEngine.Object.Instantiate(portraitAnimation);
		}
		else
		{
			if (m_uberShaderController.UberShaderAnimation != null && m_uberShaderController.UberShaderAnimation.name.Replace("(Clone)", "") == portraitAnimation.name)
			{
				return;
			}
			m_uberShaderController.UberShaderAnimation = UnityEngine.Object.Instantiate(portraitAnimation);
		}
		m_uberShaderController.m_MaterialIndex = m_portraitMatIdx;
		if (isGhostCard() && m_ghostCard != GhostCard.Type.DORMANT)
		{
			m_uberShaderController.enabled = false;
		}
		else
		{
			m_uberShaderController.enabled = true;
		}
	}

	public void UpdateCustomFrameDiamondMaterial()
	{
		m_customFrameController?.UpdateCustomDiamondMaterial();
	}

	public void SetPortraitMaterial(Material material, int portraitMatIndex = -1)
	{
		if (material == null)
		{
			return;
		}
		if (portraitMatIndex == -1)
		{
			portraitMatIndex = m_portraitMatIdx;
		}
		if (m_portraitMesh != null && portraitMatIndex > -1)
		{
			Renderer portraitRenderer;
			try
			{
				portraitRenderer = m_portraitMesh.GetComponent<Renderer>();
			}
			catch (Exception ex)
			{
				ExceptionReporter.Get()?.ReportCaughtException(new Exception("Error setting portraitRenderer. Actor game object:" + base.gameObject.name + ", full path:" + base.gameObject.GetFullPath() + ", Exception:" + ex.Message, ex));
				Log.Gameplay.PrintError("Actor game object:" + base.gameObject.name + ",  full path:" + base.gameObject.GetFullPath() + ", Exception:" + ex.Message, ex.StackTrace);
				return;
			}
			Material currMaterial = portraitRenderer.GetMaterial(portraitMatIndex);
			try
			{
				if (currMaterial.mainTexture == material.mainTexture && currMaterial.shader == material.shader)
				{
					UpdateCustomFrameDiamondMaterial();
					return;
				}
			}
			catch (Exception ex2)
			{
				ExceptionReporter.Get()?.ReportCaughtException(new Exception("Error checking currMaterial.mainTexture. Actor game object:" + base.gameObject.name + ",  full path:" + base.gameObject.GetFullPath() + ", Exception:" + ex2.Message, ex2));
				Log.Gameplay.PrintError("Actor game object:" + base.gameObject.name + ",  full path:" + base.gameObject.GetFullPath() + ", Exception:" + ex2.Message, ex2.StackTrace);
			}
			portraitRenderer.SetMaterial(portraitMatIndex, material);
			UpdateCustomFrameDiamondMaterial();
			float lightBlend = 0f;
			if ((bool)m_card)
			{
				Zone zone = m_card.GetZone();
				if (zone is ZonePlay || zone is ZoneWeapon || zone is ZoneHeroPower || zone is ZoneBattlegroundHeroBuddy || zone is ZoneBattlegroundTrinket)
				{
					lightBlend = 1f;
				}
			}
			{
				foreach (Material mat in portraitRenderer.GetMaterials())
				{
					if (mat.HasProperty("_LightingBlend"))
					{
						mat.SetFloat("_LightingBlend", lightBlend);
					}
					if (mat.HasProperty("_Seed") && mat.GetFloat("_Seed") == 0f)
					{
						mat.SetFloat("_Seed", UnityEngine.Random.Range(0f, 2f));
					}
				}
				return;
			}
		}
		if (m_legacyPortraitMaterialIndex < 0)
		{
			return;
		}
		try
		{
			if (m_meshRenderer.GetMaterial(m_legacyPortraitMaterialIndex) == material)
			{
				return;
			}
		}
		catch (Exception ex3)
		{
			ExceptionReporter.Get()?.ReportCaughtException(new Exception("Error getting currMaterial. Actor game object:" + base.gameObject.name + ",  full path:" + base.gameObject.GetFullPath() + ", Exception:" + ex3.Message, ex3));
			Log.Gameplay.PrintError("Actor game object:" + base.gameObject.name + ", full path:" + base.gameObject.GetFullPath() + ", Exception:" + ex3.Message, ex3.StackTrace);
		}
		m_meshRenderer.SetMaterial(m_legacyPortraitMaterialIndex, material);
	}

	public void SetPortraitDesaturation(float desaturation)
	{
		if (!(m_portraitMesh != null) || m_portraitMatIdx <= -1)
		{
			return;
		}
		foreach (Material mat in m_portraitMesh.GetComponent<Renderer>().GetMaterials())
		{
			if (mat.HasProperty("_Desaturate"))
			{
				mat.SetFloat("_Desaturate", desaturation);
			}
		}
	}

	public GameObject GetPortraitMesh()
	{
		return m_portraitMesh;
	}

	public virtual Material GetPortraitMaterial()
	{
		if (m_portraitMesh != null)
		{
			Renderer renderer = m_portraitMesh.GetComponent<Renderer>();
			if (m_portraitMatIdx >= 0 && m_portraitMatIdx < renderer.GetSharedMaterials().Count)
			{
				if (!Application.isPlaying)
				{
					return renderer.GetSharedMaterial(m_portraitMatIdx);
				}
				return renderer.GetMaterial(m_portraitMatIdx);
			}
		}
		if (m_legacyPortraitMaterialIndex >= 0)
		{
			return m_meshRenderer.GetMaterial(m_legacyPortraitMaterialIndex);
		}
		return null;
	}

	protected virtual bool IsPremiumPortraitEnabled()
	{
		GameEntity gameEntity = GetGameEntityIfAllowed();
		if (gameEntity != null && gameEntity.HasTag(GAME_TAG.DISABLE_NONHERO_GOLDEN_ANIMATIONS) && (m_entityDef == null || !m_entityDef.IsHero()) && (m_entity == null || !m_entity.IsHero()))
		{
			return false;
		}
		CardDef cardDef = m_cardDefHandle.Get(m_premiumType);
		if (cardDef == null || !string.IsNullOrEmpty(cardDef.m_LegendaryModel))
		{
			return false;
		}
		if (m_graphicsManager != null)
		{
			return !m_graphicsManager.isVeryLowQualityDevice();
		}
		return false;
	}

	protected bool UsesLegendaryPortraitForHistoryTile()
	{
		if (m_actorState != ActorStateType.CARD_HISTORY)
		{
			return false;
		}
		CardDef cardDef = m_cardDefHandle.Get(m_premiumType);
		if (cardDef == null || string.IsNullOrEmpty(cardDef.m_LegendaryModel))
		{
			return false;
		}
		return cardDef.m_UseLegendaryPortraitForHistoryTile;
	}

	public void SetBlockTextComponentUpdate(bool block)
	{
		m_blockTextComponentUpdate = block;
	}

	public virtual void UpdateTextComponents()
	{
		if (!m_blockTextComponentUpdate)
		{
			if (m_entityDef != null)
			{
				UpdateTextComponentsDef(m_entityDef);
			}
			else
			{
				UpdateTextComponents(m_entity);
			}
		}
	}

	public virtual void UpdateTextComponentsDef(EntityDef entityDef)
	{
		if (entityDef != null)
		{
			UpdateCostTextMesh(entityDef);
			UpdateAttackTextMesh(entityDef);
			UpdateHealthTextMesh(entityDef);
			UpdateArmorTextMesh(entityDef);
			UpdateNameText();
			UpdatePowersText();
			UpdateRace();
			UpdateSecretAndQuestText();
			UpdateBannedRibbonTextMesh(entityDef);
			UpdateMercenaryLevelTextMesh(entityDef);
			UpdateMercenaryFactionBannerMesh(entityDef);
		}
	}

	private void UpdateCostTextMesh(EntityDef entityDef)
	{
		if (!(m_costTextMesh == null))
		{
			if (HasHideStats(entityDef) || entityDef.HasTag(GAME_TAG.HIDE_COST) || (entityDef.IsCardButton() && entityDef.HasTriggerVisual() && (!GameMgr.Get().IsBattlegrounds() || !entityDef.HasTag(GAME_TAG.HAS_ACTIVATE_POWER))) || m_showCostOverride == -1 || (m_showCostOverride != 1 && ((entityDef.IsBaconSpell() && m_entity != null && m_entity.GetControllerSide() == Player.Side.FRIENDLY) || (!entityDef.IsBaconSpell() && UseTechLevelManaGem()))))
			{
				m_costTextMesh.Text = "";
			}
			else
			{
				m_costTextMesh.Text = Convert.ToString(entityDef.GetTag(GAME_TAG.COST));
			}
		}
	}

	public void UpdateAttackTextMesh(EntityDef entityDef)
	{
		if (m_attackTextMesh == null)
		{
			return;
		}
		if (HasHideStats(entityDef) || entityDef.HasTag(GAME_TAG.HIDE_ATTACK))
		{
			m_attackTextMesh.Text = "";
			m_attackTextMesh.gameObject.SetActive(value: false);
			GemObject gem = GameObjectUtils.FindComponentInThisOrParents<GemObject>(m_attackTextMesh.gameObject);
			if (gem != null)
			{
				gem.Hide();
				gem.SetHideNumberFlag(enable: true);
			}
			return;
		}
		m_attackTextMesh.gameObject.SetActive(m_isTextVisible);
		GemObject gem2 = GameObjectUtils.FindComponentInThisOrParents<GemObject>(m_attackTextMesh.gameObject);
		if (gem2 != null)
		{
			gem2.Show();
			gem2.SetHideNumberFlag(enable: false);
		}
		int attack = entityDef.GetTag(GAME_TAG.ATK);
		if (entityDef.IsHero())
		{
			if (attack == 0)
			{
				if (m_attackObject != null && m_attackObject.activeSelf)
				{
					m_attackObject.SetActive(value: false);
				}
				m_attackTextMesh.Text = "";
				return;
			}
			if (m_attackObject != null && !m_attackObject.activeSelf)
			{
				m_attackObject.SetActive(value: true);
			}
			string attackText = ((!entityDef.HasTag(GAME_TAG.HIDE_ATTACK_NUMBER)) ? Convert.ToString(attack) : string.Empty);
			m_attackTextMesh.Text = attackText;
		}
		else
		{
			string attackText2 = ((!entityDef.HasTag(GAME_TAG.HIDE_ATTACK_NUMBER)) ? Convert.ToString(attack) : string.Empty);
			m_attackTextMesh.Text = attackText2;
		}
	}

	public void UpdateHealthTextMesh(EntityDef entityDef)
	{
		if (m_healthTextMesh == null)
		{
			return;
		}
		GemObject gem = GameObjectUtils.FindComponentInThisOrParents<GemObject>(m_healthTextMesh.gameObject);
		if (HasHideStats(entityDef) || entityDef.HasTag(GAME_TAG.HIDE_HEALTH) || (entityDef.IsHero() && m_healthDisplayRequiresHeroCard && m_card == null))
		{
			m_healthTextMesh.Text = "";
			m_healthTextMesh.gameObject.SetActive(value: false);
			gem = GameObjectUtils.FindComponentInThisOrParents<GemObject>(m_healthTextMesh.gameObject);
			if (gem != null)
			{
				gem.Hide();
				gem.SetHideNumberFlag(enable: true);
			}
			return;
		}
		m_healthTextMesh.gameObject.SetActive(m_isTextVisible);
		if (gem != null)
		{
			gem.Show();
			gem.SetHideNumberFlag(enable: false);
		}
		if (entityDef.HasTag(GAME_TAG.HIDE_HEALTH_NUMBER))
		{
			m_healthTextMesh.Text = string.Empty;
		}
		else if (entityDef.IsWeapon())
		{
			m_healthTextMesh.Text = Convert.ToString(entityDef.GetTag(GAME_TAG.DURABILITY));
		}
		else
		{
			m_healthTextMesh.Text = Convert.ToString(entityDef.GetTag(GAME_TAG.HEALTH));
		}
	}

	private void UpdateArmorTextMesh(EntityDef entityDef)
	{
		if (m_armorTextMesh == null)
		{
			return;
		}
		int armor = entityDef.GetTag(GAME_TAG.ARMOR);
		if (armor == 0 || HasHideStats(entityDef))
		{
			if (m_armorObject != null && m_armorObject.activeSelf)
			{
				m_armorObject.SetActive(value: false);
			}
			m_armorTextMesh.Text = "";
		}
		else
		{
			if (m_armorObject != null && !m_armorObject.activeSelf)
			{
				m_armorObject.SetActive(value: true);
			}
			m_armorTextMesh.Text = Convert.ToString(armor);
		}
	}

	private void UpdateMercenaryLevelTextMesh(EntityDef entityDef)
	{
		if (!(m_mercenaryLevelObject == null) && !(m_mercenaryLevelObject.m_levelText == null))
		{
			if (HasHideStats(entityDef))
			{
				UpdateNumberText(m_mercenaryLevelObject.m_levelText, "", shouldHide: true);
				return;
			}
			int experience = entityDef.GetTag(GAME_TAG.LETTUCE_MERCENARY_EXPERIENCE);
			m_mercenaryLevelObject.SetLevelText(GameUtils.GetMercenaryLevelFromExperience(experience));
		}
	}

	private void UpdateMercenaryFactionBannerMesh(EntityBase entityDef, string name)
	{
		if (m_factionBannerBackground == null || m_factionBannerIcons == null || m_factionBannerIcons.Length == 0)
		{
			return;
		}
		TAG_LETTUCE_FACTION faction = entityDef.GetTag<TAG_LETTUCE_FACTION>(GAME_TAG.LETTUCE_FACTION);
		bool useFactionBanners = faction != TAG_LETTUCE_FACTION.NONE;
		int bannersEnabled = 0;
		m_factionBannerBackground.SetActive(useFactionBanners);
		FactionObject[] factionBannerIcons = m_factionBannerIcons;
		foreach (FactionObject factionObject in factionBannerIcons)
		{
			bool enableThisBanner = factionObject.m_faction == faction && useFactionBanners;
			if (factionObject.m_banner != null)
			{
				factionObject.m_banner.SetActive(enableThisBanner);
				bannersEnabled += (enableThisBanner ? 1 : 0);
			}
		}
		if (useFactionBanners && bannersEnabled != 1)
		{
			Debug.LogError($"Error enabling faction banners on {name}. Expected to enable 1 faction icon, instead got {bannersEnabled}. Requested faction is \"{faction}\".");
		}
	}

	private void UpdateMercenaryFactionBannerMesh(Entity entity)
	{
		UpdateMercenaryFactionBannerMesh(entity, entity.GetName());
	}

	private void UpdateMercenaryFactionBannerMesh(EntityDef entity)
	{
		UpdateMercenaryFactionBannerMesh(entity, entity.GetName());
	}

	private void UpdateBannedRibbonTextMesh(EntityDef entityDef)
	{
		if (!(m_bannedRibbonContainer == null))
		{
			m_bannedRibbonContainer.gameObject.SetActive(value: false);
			if (!(m_bannedRibbon == null) && SceneMgr.Get().GetMode() == SceneMgr.Mode.COLLECTIONMANAGER && !entityDef.IsCustomCoin() && !CraftingManager.GetIsInCraftingMode() && RankMgr.Get().HasLocalPlayerMedalInfo && RankMgr.Get().IsCardLockedInCurrentLeague(entityDef))
			{
				m_bannedRibbonContainer.gameObject.SetActive(value: true);
				m_bannedRibbon.SetActive(value: true);
				RankMgr rankMgr = RankMgr.Get();
				LeagueDbfRecord leagueRecordToUse = rankMgr.GetLeagueRecordForType(GameUtils.HasCompletedApprentice() ? League.LeagueType.NORMAL : League.LeagueType.NEW_PLAYER, rankMgr.GetLocalPlayerMedalInfo().GetCurrentSeasonId());
				m_bannedRibbon.GetComponentInChildren<UberText>().Text = leagueRecordToUse.LockedCardUnplayableText;
			}
		}
	}

	public void UpdateMinionAtkText(int defATK, int entATK, bool allowJiggle = false, bool hideAttackNumber = false)
	{
		if (m_attackTextMesh == null)
		{
			return;
		}
		UpdateTextColorToGreenOrWhite(m_attackTextMesh, defATK, entATK);
		string newAttackString = ((!hideAttackNumber) ? Convert.ToString(entATK) : string.Empty);
		if (allowJiggle && m_attackTextMesh.Text != newAttackString)
		{
			GemObject gem = GameObjectUtils.FindComponentInThisOrParents<GemObject>(m_attackTextMesh.gameObject);
			if (gem != null)
			{
				gem.Jiggle();
			}
		}
		m_attackTextMesh.Text = newAttackString;
	}

	public void UpdateMinionHealthText(int defHealth, int maxHealth, int damage, bool allowJiggle = false, bool hideHealthNumber = false)
	{
		int currHealth = maxHealth - damage;
		if (damage > 0)
		{
			UpdateTextColor(m_healthTextMesh, maxHealth, currHealth);
		}
		else if (maxHealth > defHealth)
		{
			UpdateTextColor(m_healthTextMesh, defHealth, currHealth);
		}
		else
		{
			UpdateTextColor(m_healthTextMesh, currHealth, currHealth);
		}
		string newHealthString = ((!hideHealthNumber) ? Convert.ToString(currHealth) : string.Empty);
		if (allowJiggle && m_healthTextMesh.Text != newHealthString)
		{
			GemObject gem = GameObjectUtils.FindComponentInThisOrParents<GemObject>(m_healthTextMesh.gameObject);
			if (gem != null)
			{
				gem.Jiggle();
			}
		}
		m_healthTextMesh.Text = newHealthString;
	}

	public void UpdateMinionStatsImmediately()
	{
		if (m_entity == null || !m_entity.IsMinion() || HasHideStats(m_entity))
		{
			return;
		}
		if (!m_entity.HasTag(GAME_TAG.HIDE_ATTACK))
		{
			UpdateMinionAtkText(m_entity.GetDefATK(), m_entity.GetATK());
		}
		if (!(m_healthTextMesh != null) || m_entity.HasTag(GAME_TAG.HIDE_HEALTH))
		{
			return;
		}
		int currHealth = 0;
		if (m_entity.HasTag(GAME_TAG.ENABLE_HEALTH_DISPLAY))
		{
			currHealth = m_entity.GetTag(GAME_TAG.HEALTH_DISPLAY);
			if (m_entity.HasTag(GAME_TAG.HEALTH_DISPLAY_NEGATIVE))
			{
				currHealth = -currHealth;
			}
			switch (m_entity.GetTag(GAME_TAG.HEALTH_DISPLAY_COLOR))
			{
			case 0:
				UpdateTextColor(m_healthTextMesh, currHealth, currHealth);
				break;
			case 1:
				UpdateTextColor(m_healthTextMesh, currHealth + 1, currHealth);
				break;
			case 2:
				UpdateTextColor(m_healthTextMesh, currHealth - 1, currHealth);
				break;
			}
		}
		else
		{
			UpdateMinionHealthText(m_entity.GetDefHealth(), m_entity.GetHealth(), m_entity.GetDamage(), allowJiggle: false, m_entity.HasTag(GAME_TAG.HIDE_HEALTH_NUMBER));
		}
	}

	public virtual void UpdateTextComponents(Entity entity)
	{
		if (entity != null)
		{
			UpdateCostTextMesh(entity);
			UpdateAttackTextMesh(entity);
			UpdateHealthTextMesh(entity);
			UpdateArmorTextMesh(entity);
			UpdateNameText();
			UpdatePowersText();
			UpdateRace();
			UpdateSecretAndQuestText();
			UpdateMercenaryLevelTextMesh(entity);
			UpdateMercenaryFactionBannerMesh(entity);
		}
	}

	private int GetSecretCostByClass(TAG_CLASS classType)
	{
		switch (classType)
		{
		case TAG_CLASS.PALADIN:
			return 1;
		case TAG_CLASS.HUNTER:
		case TAG_CLASS.ROGUE:
			return 2;
		case TAG_CLASS.MAGE:
			return 3;
		case TAG_CLASS.WARRIOR:
			return 0;
		default:
			return -1;
		}
	}

	private void UpdateCostTextMesh(Entity entity)
	{
		if (m_costTextMesh == null)
		{
			return;
		}
		bool isBattlegrounds = GameMgr.Get().IsBattlegrounds();
		GameEntity gameEntity = ((GameState.Get() == null) ? null : GameState.Get().GetGameEntity());
		bool enemySideMinionInShoppingPhaseWithCostOverriden = isBattlegrounds && entity.IsMinion() && gameEntity != null && gameEntity.IsInBattlegroundsShopPhase() && gameEntity.HasTag(GAME_TAG.BACON_SHOW_OVERRIDEN_MINION_COST) && entity.GetControllerSide() != Player.Side.FRIENDLY && entity.HasTag(GAME_TAG.BACON_OVERRIDE_BG_COST);
		if (HasHideStats(m_entity) || m_entity.HasTag(GAME_TAG.HIDE_COST) || m_showCostOverride == -1 || (m_showCostOverride != 1 && ((!entity.IsBaconSpell() && !enemySideMinionInShoppingPhaseWithCostOverriden && UseTechLevelManaGem()) || (entity.IsBaconSpell() && entity.GetControllerSide() == Player.Side.FRIENDLY))))
		{
			UpdateNumberText(m_costTextMesh, "", shouldHide: false);
			return;
		}
		if (m_entity.IsHiddenSecret())
		{
			int baseCost = GetSecretCostByClass(entity.GetClass());
			if (baseCost >= 0)
			{
				UpdateTextColor(m_costTextMesh, baseCost, entity.GetCost(), higherIsBetter: true);
			}
			else
			{
				m_costTextMesh.TextColor = Color.white;
			}
		}
		else if (m_entity.IsHiddenForge())
		{
			UpdateTextColor(m_costTextMesh, 0, 0, higherIsBetter: true);
		}
		else if (m_entity.IsBattlegroundTrinket())
		{
			switch (m_entity.GetTag(GAME_TAG.BACON_OVERRIDE_COST_COLOR))
			{
			case 1:
				UpdateTextColor(m_costTextMesh, 1, 0, higherIsBetter: true);
				break;
			case 2:
				UpdateTextColor(m_costTextMesh, 0, 1, higherIsBetter: true);
				break;
			default:
				UpdateTextColor(m_costTextMesh, 0, 0, higherIsBetter: true);
				break;
			}
		}
		else
		{
			UpdateTextColor(m_costTextMesh, entity.GetDefCost(), entity.GetCost(), higherIsBetter: true);
		}
		bool hideCostText = m_entity.IsCardButton() && m_entity.HasTriggerVisual() && (!isBattlegrounds || !m_entity.HasTag(GAME_TAG.HAS_ACTIVATE_POWER));
		int cost = entity.GetCost();
		if (isBattlegrounds && m_entity.IsMinion())
		{
			cost = (m_entity.HasTag(GAME_TAG.BACON_OVERRIDE_BG_COST) ? m_entity.GetTag(GAME_TAG.BACON_OVERRIDE_BG_COST) : 3);
		}
		if (hideCostText)
		{
			UpdateNumberText(m_costTextMesh, "", shouldHide: true);
		}
		else if (m_entity.IsHiddenForge())
		{
			UpdateNumberText(m_costTextMesh, "2");
		}
		else
		{
			UpdateNumberText(m_costTextMesh, Convert.ToString(cost));
		}
	}

	private void UpdateAttackTextMesh(Entity entity)
	{
		if (m_attackTextMesh == null)
		{
			return;
		}
		if (HasHideStats(entity) || entity.HasTag(GAME_TAG.HIDE_ATTACK))
		{
			UpdateNumberText(m_attackTextMesh, "", shouldHide: true);
		}
		else if (entity.IsHero())
		{
			int attack = entity.GetATK();
			if (attack == 0)
			{
				UpdateNumberText(m_attackTextMesh, "", shouldHide: true);
				return;
			}
			Card weapon = entity.GetController().GetWeaponCard();
			int weaponAttack = 0;
			if (weapon != null)
			{
				weaponAttack = weapon.GetEntity().GetATK();
			}
			UpdateTextColorToGreenOrWhite(m_attackTextMesh, weaponAttack, attack);
			UpdateNumberText(m_attackTextMesh, Convert.ToString(attack));
		}
		else
		{
			int entityAtk = entity.GetATK();
			if (entity.IsDormant() && entity.HasCachedTagForDormant(GAME_TAG.ATK))
			{
				entityAtk = entity.GetCachedTagForDormant(GAME_TAG.ATK);
			}
			UpdateTextColorToGreenOrWhite(m_attackTextMesh, entity.GetDefATK(), entityAtk);
			UpdateNumberText(m_attackTextMesh, Convert.ToString(entityAtk));
		}
	}

	private void UpdateHealthTextMesh(Entity entity)
	{
		if (!(m_healthTextMesh != null) || (entity.IsHero() && entity.GetZone() == TAG_ZONE.GRAVEYARD))
		{
			return;
		}
		if (HasHideStats(entity) || entity.HasTag(GAME_TAG.HIDE_HEALTH))
		{
			UpdateNumberText(m_healthTextMesh, "", shouldHide: true);
			return;
		}
		int maxHealth;
		int original;
		if (entity.IsWeapon())
		{
			maxHealth = entity.GetDurability();
			original = entity.GetDefDurability();
		}
		else
		{
			maxHealth = entity.GetHealth();
			original = entity.GetDefHealth();
		}
		int damage = entity.GetDamage();
		if (entity.IsDormant())
		{
			if (entity.HasCachedTagForDormant(GAME_TAG.HEALTH))
			{
				maxHealth = entity.GetCachedTagForDormant(GAME_TAG.HEALTH);
			}
			if (entity.HasCachedTagForDormant(GAME_TAG.DAMAGE))
			{
				damage = entity.GetCachedTagForDormant(GAME_TAG.DAMAGE);
			}
		}
		int currHealth = maxHealth - damage;
		if (m_entity.HasTag(GAME_TAG.ENABLE_HEALTH_DISPLAY))
		{
			currHealth = m_entity.GetTag(GAME_TAG.HEALTH_DISPLAY);
			if (m_entity.HasTag(GAME_TAG.HEALTH_DISPLAY_NEGATIVE))
			{
				currHealth = -currHealth;
			}
			switch (m_entity.GetTag(GAME_TAG.HEALTH_DISPLAY_COLOR))
			{
			case 0:
				UpdateTextColor(m_healthTextMesh, currHealth, currHealth);
				break;
			case 1:
				UpdateTextColor(m_healthTextMesh, currHealth + 1, currHealth);
				break;
			case 2:
				UpdateTextColor(m_healthTextMesh, currHealth - 1, currHealth);
				break;
			}
		}
		else if (entity.GetDamage() > 0)
		{
			UpdateTextColor(m_healthTextMesh, maxHealth, currHealth);
		}
		else if (maxHealth > original)
		{
			UpdateTextColor(m_healthTextMesh, original, currHealth);
		}
		else
		{
			UpdateTextColor(m_healthTextMesh, currHealth, currHealth);
		}
		UpdateNumberText(m_healthTextMesh, Convert.ToString(currHealth));
	}

	private void UpdateArmorTextMesh(Entity entity)
	{
		if (m_armorTextMesh == null)
		{
			return;
		}
		if (HasHideStats(entity))
		{
			UpdateNumberText(m_armorTextMesh, "", shouldHide: true);
			return;
		}
		int armor = entity.GetArmor();
		if (armor == 0)
		{
			UpdateNumberText(m_armorTextMesh, "", shouldHide: true);
		}
		else
		{
			UpdateNumberText(m_armorTextMesh, Convert.ToString(armor));
		}
	}

	private void UpdateMercenaryLevelTextMesh(Entity entity)
	{
		if (!(m_mercenaryLevelObject == null) && !(m_mercenaryLevelObject.m_levelText == null))
		{
			if (HasHideStats(entity))
			{
				UpdateNumberText(m_mercenaryLevelObject.m_levelText, "", shouldHide: true);
				return;
			}
			int experience = entity.GetTag(GAME_TAG.LETTUCE_MERCENARY_EXPERIENCE);
			m_mercenaryLevelObject.SetLevelText(GameUtils.GetMercenaryLevelFromExperience(experience));
		}
	}

	public void SetCardDefPowerTextOverride(string text)
	{
		m_cardDefPowerTextOverride = text;
	}

	public void UpdatePowersText()
	{
		if (BigCard.Get() != null)
		{
			Actor bigCardActor = BigCard.Get().GetBigCardActor();
			if (bigCardActor != null && bigCardActor != this && bigCardActor.GetEntity() == m_entity)
			{
				bigCardActor.UpdatePowersText();
			}
		}
		if (m_powersTextMesh == null && m_bgQuestPowerTextMesh == null)
		{
			return;
		}
		bool isGameplayMercenary = false;
		string text = GetPowersText(out isGameplayMercenary);
		UpdateText(m_powersTextMesh, text);
		UpdateText(m_bgQuestPowerTextMesh, text);
		if ((isGameplayMercenary || m_showUICardText) && m_mercenaryLevelObject != null)
		{
			m_mercenaryLevelObject.gameObject.SetActive(value: true);
			m_mercenaryLevelObject.m_xpBar.gameObject.SetActive(value: false);
			m_mercenaryLevelObject.m_xpBarBacking.SetActive(value: false);
			m_mercenaryLevelObject.m_xpBarCover.SetActive(value: false);
			if (m_watermarkMesh != null)
			{
				m_watermarkMesh.SetActive(string.IsNullOrEmpty(text));
			}
		}
	}

	public string GetPowersText()
	{
		bool isGameplayMercenary;
		return GetPowersText(out isGameplayMercenary);
	}

	protected string GetPowersText(out bool isGameplayMercenary)
	{
		string text = null;
		isGameplayMercenary = false;
		if (IsLettuceMercenary())
		{
			if (GetGameEntityIfAllowed() == null && !m_showUICardText)
			{
				return null;
			}
			Entity entity = m_entity;
			if (entity != null && entity.IsHistoryDupe())
			{
				entity = entity.GetCard().GetEntity();
			}
			if (entity == null && m_card != null)
			{
				entity = m_card.GetEntity();
			}
			if (entity != null)
			{
				isGameplayMercenary = true;
				if (entity.ShouldShowEquipmentTextOnMerc())
				{
					text = entity.GetEquipmentEntity()?.GetCardTextInHand();
					if (m_watermarkMesh != null)
					{
						m_watermarkMesh.SetActive(value: true);
					}
				}
			}
			else if (m_showUICardText)
			{
				text = m_UICardText;
			}
		}
		if (!m_showUICardText && string.IsNullOrEmpty(text))
		{
			if (ShouldUseEntityDefForPowersText())
			{
				text = (string.IsNullOrEmpty(m_cardDefPowerTextOverride) ? m_entityDef.GetCardTextInHand() : m_cardDefPowerTextOverride);
			}
			else
			{
				text = (m_entity.IsHiddenSecret() ? GameStrings.Get("GAMEPLAY_SECRET_DESC") : (m_entity.IsHiddenForge() ? GameStrings.Get("GAMEPLAY_FORGE_DESC") : ((!m_entity.IsHistoryDupe()) ? m_entity.GetCardTextInHand() : m_entity.GetCardTextInHistory())));
				GameEntity gameEntity = GetGameEntityIfAllowed();
				if (gameEntity != null)
				{
					text = gameEntity.UpdateCardText(m_card, this, text);
				}
			}
		}
		return text;
	}

	public void UpdateDynamicTextFromQuestEntity(Entity questEnt)
	{
		if (questEnt == null)
		{
			return;
		}
		if (questEnt.HasTag(GAME_TAG.BACON_MINION_TYPE_REWARD))
		{
			string text = CardTextBuilder.GetDefaultCardTextInHand(m_entityDef);
			text = string.Format(text, GameStrings.GetRaceNameBattlegrounds((TAG_RACE)questEnt.GetTag(GAME_TAG.BACON_MINION_TYPE_REWARD)));
			SetCardDefPowerTextOverride(text);
		}
		if (questEnt.HasTag(GAME_TAG.BACON_CARD_DBID_REWARD))
		{
			string text2 = m_entityDef.GetCardTextInHand();
			CardDbfRecord cardRecord = GameDbf.Card.GetRecord(questEnt.GetTag(GAME_TAG.BACON_CARD_DBID_REWARD));
			if (cardRecord != null)
			{
				string name = cardRecord.Name;
				text2 = string.Format(text2, name);
				SetCardDefPowerTextOverride(text2);
			}
		}
	}

	private bool ShouldUseEntityDefForPowersText()
	{
		if (m_entityDef == null)
		{
			return false;
		}
		if (m_entity == null)
		{
			return true;
		}
		if (m_entity.GetCardTextBuilder().ShouldUseEntityForTextInPlay())
		{
			return false;
		}
		return true;
	}

	private void UpdateNumberText(UberText textMesh, string newText)
	{
		UpdateNumberText(textMesh, newText, shouldHide: false);
	}

	private void UpdateNumberText(UberText textMesh, string newText, bool shouldHide)
	{
		GemObject gem = GameObjectUtils.FindComponentInThisOrParents<GemObject>(textMesh.gameObject);
		if (gem != null)
		{
			if (!gem.IsNumberHidden())
			{
				if (shouldHide)
				{
					textMesh.gameObject.SetActive(value: false);
					if (GetHistoryCard() != null || GetHistoryChildCard() != null)
					{
						gem.Hide();
					}
					else
					{
						gem.ScaleToZero();
					}
				}
				else if (textMesh.Text != newText)
				{
					gem.Jiggle();
				}
			}
			else if (!shouldHide)
			{
				textMesh.gameObject.SetActive(value: true);
				gem.SetToZeroThenEnlarge();
			}
			gem.Initialize();
			gem.SetHideNumberFlag(shouldHide);
		}
		textMesh.Text = newText;
	}

	public void UpdateNameText()
	{
		if (m_nameTextMesh == null)
		{
			return;
		}
		string text = "";
		bool isHiddenSecret = false;
		bool isHiddenForge = false;
		if (m_entity != null)
		{
			if (m_entityDef == null)
			{
				isHiddenSecret = m_entity.IsHiddenSecret();
				isHiddenForge = m_entity.IsHiddenForge();
			}
			text = m_entity.GetName();
		}
		else if (m_entityDef != null)
		{
			string shortName = m_entityDef.GetShortName();
			text = ((m_useShortName && !string.IsNullOrEmpty(shortName)) ? shortName : m_entityDef.GetName());
		}
		if (isHiddenSecret)
		{
			text = ((!GameState.Get().GetBooleanGameOption(GameEntityOption.USE_SECRET_CLASS_NAMES)) ? GameStrings.Get("GAMEPLAY_SECRET_NAME") : (m_entity.GetClass() switch
			{
				TAG_CLASS.PALADIN => GameStrings.Get("GAMEPLAY_SECRET_NAME_PALADIN"), 
				TAG_CLASS.MAGE => GameStrings.Get("GAMEPLAY_SECRET_NAME_MAGE"), 
				TAG_CLASS.HUNTER => GameStrings.Get("GAMEPLAY_SECRET_NAME_HUNTER"), 
				TAG_CLASS.ROGUE => GameStrings.Get("GAMEPLAY_SECRET_NAME_ROGUE"), 
				_ => GameStrings.Get("GAMEPLAY_SECRET_NAME"), 
			}));
		}
		else if (isHiddenForge)
		{
			text = GameStrings.Get("GAMEPLAY_FORGE_NAME");
		}
		UpdateText(m_nameTextMesh, text);
	}

	private void UpdateSecretAndQuestText()
	{
		if (!m_secretText)
		{
			return;
		}
		string text = "?";
		if (m_entity != null)
		{
			if (m_entity.IsQuest() || m_entity.IsSideQuest() || m_entity.IsQuestline())
			{
				text = "!";
			}
			else if (m_entity.IsPuzzle())
			{
				text = "P";
			}
		}
		if (m_entity.HasTag(GAME_TAG.PENDING_TRANSFORM_TO_CARD))
		{
			int cardDBID = m_entity.GetTag(GAME_TAG.PENDING_TRANSFORM_TO_CARD);
			using DefLoader.DisposableFullDef fullDef = DefLoader.Get().GetFullDef(cardDBID);
			if (fullDef.EntityDef != null)
			{
				text = ((!fullDef.EntityDef.IsQuest() && !fullDef.EntityDef.IsSideQuest() && !fullDef.EntityDef.IsQuestline()) ? "?" : "!");
			}
		}
		if ((bool)UniversalInputManager.UsePhoneUI && m_entity != null)
		{
			TransformUtil.SetLocalPosZ(m_secretText, -0.01f);
			Player controller = m_entity.GetController();
			if (controller != null && m_entity.IsSecret())
			{
				ZoneSecret secretZone = controller.GetSecretZone();
				if ((bool)secretZone)
				{
					int count = secretZone.GetSecretCount();
					if (count > 1)
					{
						text = count.ToString();
						TransformUtil.SetLocalPosZ(m_secretText, -0.03f);
					}
				}
			}
			else if (controller != null && m_entity.IsSideQuest())
			{
				TransformUtil.SetLocalPosZ(m_secretText, 0.01f);
				ZoneSecret secretZone2 = controller.GetSecretZone();
				if ((bool)secretZone2)
				{
					int count2 = secretZone2.GetSideQuestCount();
					if (count2 > 1)
					{
						text = count2.ToString();
						TransformUtil.SetLocalPosZ(m_secretText, -0.02f);
					}
				}
			}
			Transform secretMesh = m_secretText.transform.parent.Find("Secret_mesh");
			if (secretMesh != null && secretMesh.gameObject != null)
			{
				SphereCollider secretCollider = secretMesh.gameObject.GetComponent<SphereCollider>();
				if (secretCollider != null)
				{
					secretCollider.radius = 0.5f;
				}
			}
		}
		UpdateText(m_secretText, text);
	}

	public Vector3 GetCustomFrameSecretZoneOffset(int index)
	{
		if (m_customFrameController != null)
		{
			return m_customFrameController.GetCustomFrameSecretZoneOffset(index);
		}
		return Vector3.zero;
	}

	public float GetCustomFrameSecretZoneScale()
	{
		if (m_customFrameController != null)
		{
			return m_customFrameController.GetCustomFrameSecretZoneScale();
		}
		return 1f;
	}

	public bool GetCustomFrameRequiresMetaCalibration()
	{
		if (m_customFrameController != null)
		{
			return m_customFrameController.UseMetaCalibration;
		}
		return false;
	}

	public Vector3 GetCustomFrameMetaRewardCalibrationScalePC()
	{
		if (m_customFrameController != null)
		{
			return m_customFrameController.CardRewardScalePC;
		}
		return Vector3.zero;
	}

	public Vector3 GetCustomFrameMetaRewardCalibrationScalePhone()
	{
		if (m_customFrameController != null)
		{
			return m_customFrameController.CardRewardScalePhone;
		}
		return Vector3.zero;
	}

	public Vector3 GetCustomFrameEndGameXPBarScale()
	{
		if (m_customFrameController != null)
		{
			return m_customFrameController.EndGameXPBarScale;
		}
		return Vector3.zero;
	}

	public Vector3 GetCustomeFrameEndGameVictoryXPBarPosition()
	{
		if (m_customFrameController != null)
		{
			return m_customFrameController.EndGameVictoryBarPosition;
		}
		return Vector3.zero;
	}

	public Vector3 GetHeroLabelOffset(Player.Side side)
	{
		if (m_customFrameController != null)
		{
			return m_customFrameController.GetCustomFrameMulliganHeroNameOffset(side);
		}
		return Vector3.zero;
	}

	public Vector3 GetHeroOffset(Player.Side side)
	{
		if (m_customFrameController != null)
		{
			return m_customFrameController.GetCustomFrameMulliganHeroOffset(side);
		}
		return Vector3.zero;
	}

	private void UpdateText(UberText uberTextMesh, string text)
	{
		if (!(uberTextMesh == null))
		{
			uberTextMesh.Text = text;
		}
	}

	private void UpdateTextColor(UberText originalMesh, int defNumber, int currentNumber)
	{
		UpdateTextColor(originalMesh, defNumber, currentNumber, higherIsBetter: false);
	}

	private void UpdateTextColor(UberText uberTextMesh, int defNumber, int currentNumber, bool higherIsBetter)
	{
		if ((defNumber > currentNumber && higherIsBetter) || (defNumber < currentNumber && !higherIsBetter))
		{
			uberTextMesh.TextColor = Color.green;
		}
		else if ((defNumber < currentNumber && higherIsBetter) || (defNumber > currentNumber && !higherIsBetter))
		{
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				uberTextMesh.TextColor = new Color(1f, 10f / 51f, 10f / 51f);
			}
			else
			{
				uberTextMesh.TextColor = Color.red;
			}
		}
		else if (defNumber == currentNumber)
		{
			uberTextMesh.TextColor = Color.white;
		}
	}

	private void UpdateTextColorToGreenOrWhite(UberText uberTextMesh, int defNumber, int currentNumber)
	{
		if (defNumber < currentNumber)
		{
			uberTextMesh.TextColor = Color.green;
		}
		else
		{
			uberTextMesh.TextColor = Color.white;
		}
	}

	private void DisableTextMesh(UberText mesh)
	{
		if (!(mesh == null))
		{
			mesh.gameObject.SetActive(value: false);
		}
	}

	public void SetUseShortName(bool useShortName)
	{
		m_useShortName = useShortName;
	}

	public void SetupUICardText(bool showText, string textToShow = null)
	{
		bool num = m_showUICardText != showText || (m_showUICardText && !string.Equals(m_UICardText, textToShow, StringComparison.Ordinal));
		m_showUICardText = showText;
		m_UICardText = (m_showUICardText ? textToShow : null);
		if (num)
		{
			UpdatePowersText();
		}
	}

	public void OverrideNameText(UberText newText)
	{
		if (m_nameTextMesh != null)
		{
			m_nameTextMesh.gameObject.SetActive(value: false);
		}
		m_nameTextMesh = newText;
		UpdateNameText();
		if (m_shown && newText != null)
		{
			newText.gameObject.SetActive(m_isTextVisible);
		}
	}

	public void HideAllText()
	{
		ToggleTextVisibility(bOn: false);
	}

	public void ShowAllText()
	{
		ToggleTextVisibility(bOn: true);
	}

	public void HealthDisplayRequiresHeroCard(bool required)
	{
		m_healthDisplayRequiresHeroCard = required;
		ToggleTextVisibility(m_isTextVisible);
	}

	public bool ShouldAlwaysHidePowers()
	{
		if (m_powersTextMesh == null)
		{
			return true;
		}
		if (m_premiumType == TAG_PREMIUM.SIGNATURE && !ActorNames.SignatureFrameHasPowersText(m_entityDef.GetCardId()))
		{
			return true;
		}
		return false;
	}

	private void ToggleTextVisibility(bool bOn)
	{
		m_isTextVisible = bOn;
		if (m_healthTextMesh != null)
		{
			m_healthTextMesh.gameObject.SetActive(bOn);
		}
		if (m_armorTextMesh != null)
		{
			m_armorTextMesh.gameObject.SetActive(bOn);
		}
		if (m_attackTextMesh != null)
		{
			m_attackTextMesh.gameObject.SetActive(bOn);
		}
		if (m_nameTextMesh != null)
		{
			m_nameTextMesh.gameObject.SetActive(bOn);
			if ((bool)m_nameTextMesh.RenderOnObject)
			{
				m_nameTextMesh.RenderOnObject.GetComponent<Renderer>().enabled = bOn;
			}
		}
		if (m_powersTextMesh != null && m_premiumType != TAG_PREMIUM.SIGNATURE)
		{
			m_powersTextMesh.gameObject.SetActive(bOn);
		}
		if (m_bgQuestPowerTextMesh != null)
		{
			m_bgQuestPowerTextMesh.gameObject.SetActive(bOn);
		}
		if (m_costTextMesh != null)
		{
			m_costTextMesh.gameObject.SetActive(bOn);
		}
		if (m_raceTextMesh != null && m_entityDef != null && m_entityDef.GetRaceCount() == 1)
		{
			m_raceTextMesh.gameObject.SetActive(bOn);
		}
		if (m_multiRaceTextMesh != null && m_entityDef != null && m_entityDef.GetRaceCount() > 1)
		{
			m_multiRaceTextMesh.gameObject.SetActive(bOn);
		}
		if (m_bgQuestRaceTextMesh != null)
		{
			m_bgQuestRaceTextMesh.gameObject.SetActive(bOn);
		}
		if ((bool)m_secretText)
		{
			m_secretText.gameObject.SetActive(bOn);
		}
	}

	public void CreateBannedRibbon()
	{
		if (m_bannedRibbonContainer != null)
		{
			m_bannedRibbonContainer.gameObject.SetActive(value: true);
			m_bannedRibbon = m_bannedRibbonContainer.PrefabGameObject(instantiateIfNeeded: true);
			if (m_bannedRibbon != null)
			{
				LayerUtils.SetLayer(m_bannedRibbon, base.gameObject.layer, null);
			}
		}
	}

	public bool IsContactShadowEnabled()
	{
		return m_shadowVisible;
	}

	public bool HasContactShadowObject()
	{
		return m_contactShadows != null;
	}

	public void ContactShadow(bool visible)
	{
		m_shadowVisible = visible;
		if (!m_shadowObjectInitialized)
		{
			CacheShadowObjects();
		}
		UpdateContactShadow();
	}

	public void UpdateContactShadow()
	{
		bool isElite = IsElite();
		if (m_contactShadows == null)
		{
			return;
		}
		foreach (ContactShadowData contactShadow in m_contactShadows)
		{
			Renderer shadowRenderer = contactShadow.ShadowObject.GetComponent<Renderer>();
			if (m_shadowVisible && m_shown)
			{
				shadowRenderer.enabled = (isElite && contactShadow.IsUnique) || (!isElite && !contactShadow.IsUnique);
			}
			else
			{
				shadowRenderer.enabled = false;
			}
		}
	}

	public void MoveShadowToMissingCard(bool reset, int renderQueue = 0)
	{
		Transform parent = null;
		if (reset && m_cardMesh != null)
		{
			parent = m_cardMesh.transform;
		}
		else
		{
			if (reset || !(m_missingCardEffect != null))
			{
				return;
			}
			parent = m_missingCardEffect.transform;
		}
		bool isElite = IsElite();
		if (m_contactShadows == null)
		{
			return;
		}
		foreach (ContactShadowData contactShadow in m_contactShadows)
		{
			if (isElite == contactShadow.IsUnique)
			{
				Renderer renderer = contactShadow.ShadowObject.GetComponent<Renderer>();
				if (!(renderer == null))
				{
					int newRenderQueue = (reset ? contactShadow.InitialRenderQueue : (renderer.GetMaterial().renderQueue + renderQueue));
					renderer.GetMaterial().renderQueue = newRenderQueue;
					contactShadow.ShadowObject.transform.SetParent(base.transform, worldPositionStays: true);
					contactShadow.ShadowObject.transform.localPosition = contactShadow.InitialPositionRelativeToActor;
					contactShadow.ShadowObject.transform.SetParent(parent, worldPositionStays: true);
				}
			}
		}
	}

	public virtual void UpdateMeshComponents()
	{
		UpdateRarityComponent();
		UpdateDescriptionMesh();
		UpdateEliteComponent();
		UpdatePremiumComponents();
		UpdateCardColor();
		UpdateManaGemOffset();
		UpdateManaGemComponent();
		UpdateMercenaryRoleComponents();
		UpdateCardRuneBannerComponent();
		UpdateMulticlassRibbon();
	}

	private void UpdateRarityComponent()
	{
		if (GetPremium() == TAG_PREMIUM.SIGNATURE)
		{
			if (m_card != null)
			{
				if (m_card.GetEntity() != null && ActorNames.GetSignatureFrameId(m_card.GetEntity().GetCardId()) != 1)
				{
					SetRarityGemShape(GetRarity(), m_card.GetEntity().GetCardId());
				}
			}
			else if (m_entityDef != null)
			{
				if (ActorNames.GetSignatureFrameId(m_entityDef.GetCardId()) != 1)
				{
					SetRarityGemShape(GetRarity(), m_entityDef.GetCardId());
				}
			}
			else if (m_entity != null && ActorNames.GetSignatureFrameId(m_entity.GetCardId()) != 1)
			{
				SetRarityGemShape(GetRarity(), m_entity.GetCardId());
			}
		}
		if ((bool)m_rarityGemMesh)
		{
			UnityEngine.Vector2 textureOffset;
			Color tint;
			bool show = GetRarityTextureOffset(out textureOffset, out tint);
			RenderUtils.EnableRenderers(m_rarityGemMesh, show, includeInactive: true);
			if ((bool)m_rarityFrameMesh)
			{
				RenderUtils.EnableRenderers(m_rarityFrameMesh, show, includeInactive: true);
			}
			bool isLocation = false;
			if (m_entity != null)
			{
				isLocation = m_entity.IsLocation();
			}
			else if (m_entityDef != null)
			{
				isLocation = m_entityDef.IsLocation();
			}
			if (isLocation && m_rarityNoGemMesh != null)
			{
				m_rarityNoGemMesh.SetActive(!show);
			}
			if (show)
			{
				Material materialInstance = GetMaterialInstance(m_rarityGemMesh.GetComponent<Renderer>());
				materialInstance.mainTextureOffset = textureOffset;
				materialInstance.SetColor("_tint", tint);
			}
		}
	}

	private void SetRarityGemShape(TAG_RARITY rarity, string cardId)
	{
		if (ActorNames.GetSignatureFrameId(cardId) != 1 && !(m_gemMeshCommon == null) && !(m_gemMeshRare == null) && !(m_gemMeshEpic == null) && !(m_gemMeshLegendary == null))
		{
			m_gemMeshCommon.SetActive(rarity == TAG_RARITY.COMMON || rarity == TAG_RARITY.FREE);
			m_gemMeshRare.SetActive(rarity == TAG_RARITY.RARE);
			m_gemMeshEpic.SetActive(rarity == TAG_RARITY.EPIC);
			m_gemMeshLegendary.SetActive(rarity == TAG_RARITY.LEGENDARY);
			SetRarityGemMesh(rarity);
		}
	}

	private void SetRarityGemMesh(TAG_RARITY rarity)
	{
		switch (rarity)
		{
		case TAG_RARITY.COMMON:
		case TAG_RARITY.FREE:
			m_rarityGemMesh = m_gemMeshCommon;
			break;
		case TAG_RARITY.RARE:
			m_rarityGemMesh = m_gemMeshRare;
			break;
		case TAG_RARITY.EPIC:
			m_rarityGemMesh = m_gemMeshEpic;
			break;
		case TAG_RARITY.LEGENDARY:
			m_rarityGemMesh = m_gemMeshLegendary;
			break;
		}
	}

	private bool GetRarityTextureOffset(out UnityEngine.Vector2 offset, out Color tint)
	{
		offset = GEM_TEXTURE_OFFSET_COMMON;
		tint = GEM_COLOR_COMMON;
		if (m_entityDef == null && m_entity == null)
		{
			return false;
		}
		TAG_CARD_SET cardSet = ((m_entityDef == null) ? m_entity.GetCardSet() : m_entityDef.GetCardSet());
		if (cardSet == TAG_CARD_SET.MISSIONS)
		{
			return false;
		}
		if (m_entityDef != null)
		{
			if (m_premiumType == TAG_PREMIUM.SIGNATURE && ActorNames.GetSignatureFrameId(m_entityDef.GetCardId()) != 1)
			{
				return true;
			}
		}
		else if (m_entity != null && m_premiumType == TAG_PREMIUM.SIGNATURE && ActorNames.GetSignatureFrameId(m_entity.GetCardId()) != 1)
		{
			return true;
		}
		switch (GetRarity())
		{
		case TAG_RARITY.COMMON:
			offset = GEM_TEXTURE_OFFSET_COMMON;
			tint = GEM_COLOR_COMMON;
			break;
		case TAG_RARITY.RARE:
			offset = GEM_TEXTURE_OFFSET_RARE;
			tint = GEM_COLOR_RARE;
			break;
		case TAG_RARITY.EPIC:
			offset = GEM_TEXTURE_OFFSET_EPIC;
			tint = GEM_COLOR_EPIC;
			break;
		case TAG_RARITY.LEGENDARY:
			offset = GEM_TEXTURE_OFFSET_LEGENDARY;
			tint = GEM_COLOR_LEGENDARY;
			break;
		default:
			return false;
		}
		return true;
	}

	private void UpdateDescriptionMesh()
	{
		bool showDescriptionMesh = true;
		if (m_descriptionMesh != null)
		{
			Renderer renderer = m_descriptionMesh.GetComponent<Renderer>();
			if (renderer != null)
			{
				renderer.enabled = showDescriptionMesh;
			}
		}
		if (m_descriptionTrimMesh != null)
		{
			Renderer renderer = m_descriptionTrimMesh.GetComponent<Renderer>();
			if (renderer != null)
			{
				renderer.enabled = showDescriptionMesh;
			}
		}
		bool entityIsQuest = m_entity != null && m_entity.IsQuest();
		bool entityDefIsQuest = m_entityDef != null && m_entityDef.IsQuest();
		bool useBaconQuestMesh = GameMgr.Get() != null && GameMgr.Get().IsBattlegrounds() && (entityIsQuest || entityDefIsQuest);
		if (m_descriptionMesh != null)
		{
			m_descriptionMesh.SetActive(!useBaconQuestMesh);
		}
		if (m_baconQuestDescriptionMesh != null)
		{
			m_baconQuestDescriptionMesh.SetActive(useBaconQuestMesh);
		}
		if (showDescriptionMesh)
		{
			UpdateWatermark();
		}
	}

	private void UpdateWatermark()
	{
		if (m_entityDef == null && m_entity == null)
		{
			return;
		}
		string watermarkIconFile = null;
		EntityDef obj = m_entityDef ?? m_entity.GetEntityDef();
		string perCardWatermarkFile = obj.GetWatermarkTextureOverride();
		TAG_CARD_SET displayedCardSet = GetCardSet();
		if (m_watermarkCardSetOverride != 0)
		{
			displayedCardSet = m_watermarkCardSetOverride;
		}
		else if (!string.IsNullOrEmpty(perCardWatermarkFile))
		{
			watermarkIconFile = perCardWatermarkFile;
		}
		if (watermarkIconFile == null)
		{
			CardSetDbfRecord cardSetRecord = GameDbf.GetIndex().GetCardSet(displayedCardSet);
			if (cardSetRecord != null)
			{
				watermarkIconFile = cardSetRecord.CardWatermarkTexture;
			}
		}
		if (obj.IsCoreCard())
		{
			watermarkIconFile = SetRotationIcon.GetYearIconWatermark();
		}
		float alphaValue = 0f;
		alphaValue = (((m_entityDef == null || !m_entityDef.HasTag(GAME_TAG.HIDE_WATERMARK)) && (m_entity == null || !m_entity.HasTag(GAME_TAG.HIDE_WATERMARK))) ? WATERMARK_ALPHA_VALUE : 0f);
		if (m_descriptionMesh != null && m_descriptionMesh.TryGetComponent<Renderer>(out var descriptionMeshRenderer) && descriptionMeshRenderer.GetSharedMaterial().HasProperty("_SecondTint") && descriptionMeshRenderer.GetSharedMaterial().HasProperty("_SecondTex"))
		{
			if (!string.IsNullOrEmpty(watermarkIconFile))
			{
				AssetLoader.Get().LoadAsset(ref m_watermarkTex, watermarkIconFile);
				GetMaterialInstance(descriptionMeshRenderer).SetTexture("_SecondTex", m_watermarkTex);
			}
			else
			{
				alphaValue = 0f;
			}
			Material materialInstance = GetMaterialInstance(descriptionMeshRenderer);
			Color tintColor = materialInstance.GetColor("_SecondTint");
			tintColor.a = alphaValue;
			materialInstance.SetColor("_SecondTint", tintColor);
		}
		if (m_watermarkMesh != null && m_watermarkMesh.TryGetComponent<Renderer>(out var waterMarkRenderer) && waterMarkRenderer.GetSharedMaterial().HasProperty("_Color") && waterMarkRenderer.GetSharedMaterial().HasProperty("_MainTex"))
		{
			if (!string.IsNullOrEmpty(watermarkIconFile))
			{
				AssetLoader.Get().LoadAsset(ref m_watermarkTex, watermarkIconFile);
				GetMaterialInstance(waterMarkRenderer).SetTexture("_MainTex", m_watermarkTex);
			}
			else
			{
				alphaValue = 0f;
			}
			Material materialInstance2 = GetMaterialInstance(waterMarkRenderer);
			Color tintColor2 = materialInstance2.GetColor("_Color");
			tintColor2.a = alphaValue;
			materialInstance2.SetColor("_Color", tintColor2);
		}
	}

	private void UpdateEliteComponent()
	{
		if (!(m_eliteObject == null))
		{
			bool show = IsElite();
			RenderUtils.EnableRenderers(m_eliteObject, show, includeInactive: true);
		}
	}

	private void UpdateManaGemComponent()
	{
		bool disableNonMercManaGem = false;
		bool disableSpellManaGem = false;
		bool showSpeedWing = false;
		if (GetGameEntityIfAllowed() != null)
		{
			disableNonMercManaGem = GameState.Get().GetBooleanGameOption(GameEntityOption.DISABLE_NONMERC_MANA_GEM);
			disableSpellManaGem = GameState.Get().GetBooleanGameOption(GameEntityOption.DISABLE_SPELL_MANA_GEM);
			showSpeedWing = GameState.Get().GetBooleanGameOption(GameEntityOption.SHOW_SPEED_WING_ON_ACTOR);
		}
		bool forceDisableManaObject = false;
		if (m_entity != null)
		{
			if (disableNonMercManaGem && m_entity.IsMinion() && !m_entity.IsLettuceMercenary())
			{
				forceDisableManaObject = true;
			}
			else if (disableSpellManaGem && m_entity.IsSpell())
			{
				forceDisableManaObject = true;
			}
		}
		else if (m_entityDef != null)
		{
			if (disableNonMercManaGem && m_entityDef.IsMinion() && !m_entityDef.IsLettuceMercenary())
			{
				forceDisableManaObject = true;
			}
			else if (disableSpellManaGem && m_entityDef.IsSpell())
			{
				forceDisableManaObject = true;
			}
		}
		if (UseTechLevelManaGem() || UseCoinManaGem())
		{
			forceDisableManaObject = true;
		}
		if (forceDisableManaObject)
		{
			if (showSpeedWing && m_speedWingObject != null)
			{
				m_speedWingObject.SetActive(value: true);
			}
			if (m_manaObject != null)
			{
				m_manaObject.SetActive(value: false);
			}
		}
		if (m_baconCoinObject != null)
		{
			bool bloodDropletActive = (m_entity != null && m_entity.HasTag(GAME_TAG.BACON_COSTS_HEALTH_TO_BUY)) || (m_entityDef != null && m_entityDef.HasTag(GAME_TAG.BACON_COSTS_HEALTH_TO_BUY));
			bool isBaconSpell = (m_entity != null && m_entity.IsBaconSpell()) || (m_entityDef != null && m_entityDef.IsBaconSpell());
			bool isMinion = (m_entity != null && m_entity.IsMinion()) || (m_entityDef != null && m_entityDef.IsMinion());
			GameEntity gameEntity = GameState.Get()?.GetGameEntity() ?? null;
			bool showMinionOverridenCost = gameEntity != null && !gameEntity.IsInBattlegroundsCombatPhase() && gameEntity.HasTag(GAME_TAG.BACON_SHOW_OVERRIDEN_MINION_COST);
			m_baconCoinObject.SetActive(!bloodDropletActive && (isBaconSpell || (showMinionOverridenCost && isMinion && m_entity != null && m_entity.HasTag(GAME_TAG.BACON_OVERRIDE_BG_COST) && m_entity.GetControllerSide() == Player.Side.OPPOSING)));
			if (isMinion)
			{
				m_costTextMesh.gameObject.SetActive(GameMgr.Get() == null || GameMgr.Get().IsBattlegrounds());
			}
		}
		UpdateLaunchpadComponents();
	}

	public void UpdateLaunchpadComponents()
	{
		if (m_entity == null)
		{
			return;
		}
		Spell launchpadSpell = null;
		if (m_entity.IsStarship() && m_entity.HasTag(GAME_TAG.LAUNCHPAD))
		{
			for (int i = 0; i < m_ownedSpells.Count; i++)
			{
				if (m_ownedSpells.ContainsKey(SpellType.STARSHIP_LAUNCHPAD_NORMAL))
				{
					launchpadSpell = m_ownedSpells[SpellType.STARSHIP_LAUNCHPAD_NORMAL];
					break;
				}
				if (m_ownedSpells.ContainsKey(SpellType.STARSHIP_LAUNCHPAD_GOLDEN))
				{
					launchpadSpell = m_ownedSpells[SpellType.STARSHIP_LAUNCHPAD_GOLDEN];
					break;
				}
				if (m_ownedSpells.ContainsKey(SpellType.STARSHIP_LAUNCHPAD_SIGNATURE))
				{
					launchpadSpell = m_ownedSpells[SpellType.STARSHIP_LAUNCHPAD_SIGNATURE];
					break;
				}
			}
		}
		if (!(launchpadSpell != null))
		{
			return;
		}
		PlayMakerFSM component = launchpadSpell.GetComponent<PlayMakerFSM>();
		FsmFloat launchCost = component.FsmVariables.GetFsmFloat(STARSHIP_LAUNCH_COST);
		FsmColor launchCostColor = component.FsmVariables.GetFsmColor(STARSHIP_LAUNCH_COST_COLOR);
		if (GameState.Get() != null)
		{
			int cost = GameUtils.StarshipLaunchCost(m_entity.GetController());
			launchCost.Value = Mathf.Max(0, cost);
			if (GameUtils.GetCardTagValue(GameUtils.STARSHIP_LAUNCH_CARD_ID, GAME_TAG.COST) > cost)
			{
				launchCostColor.Value = Color.green;
			}
		}
	}

	private void UpdateMercenaryRoleComponents()
	{
		if (m_mercenaryRoleObjects == null)
		{
			return;
		}
		foreach (MercenaryRoleGemObject mercenaryRoleObject in m_mercenaryRoleObjects)
		{
			mercenaryRoleObject.SetRole(GetMercenariesRole());
		}
	}

	private TAG_ROLE GetMercenariesRole()
	{
		TAG_ROLE role = TAG_ROLE.INVALID;
		if (m_entity != null)
		{
			role = m_entity.GetTag<TAG_ROLE>(GAME_TAG.LETTUCE_ROLE);
		}
		else if (m_entityDef != null)
		{
			role = m_entityDef.GetTag<TAG_ROLE>(GAME_TAG.LETTUCE_ROLE);
		}
		return role;
	}

	public void UpdateLettuceMinionInPlayFrame()
	{
		if (!(m_lettuceMinionInPlayFrame == null))
		{
			UpdateEliteComponent();
			TAG_ROLE role = TAG_ROLE.INVALID;
			if (m_entity != null)
			{
				role = m_entity.GetTag<TAG_ROLE>(GAME_TAG.LETTUCE_ROLE);
			}
			else if (m_entityDef != null)
			{
				role = m_entityDef.GetTag<TAG_ROLE>(GAME_TAG.LETTUCE_ROLE);
			}
			m_lettuceMinionInPlayFrame.UpdateFrameType(role);
		}
	}

	public void UpdateTitanComponents()
	{
		UpdateUnplayableChainsOnTitanSubcards();
		UpdateTitanPips();
	}

	public void SetTeammateActor(bool isTeamamteActor)
	{
		m_isTeammateActor = isTeamamteActor;
	}

	public bool IsTeammateActor()
	{
		return m_isTeammateActor;
	}

	private void SetActivePingType(TEAMMATE_PING_TYPE pingType)
	{
		m_activePingType = pingType;
	}

	public TEAMMATE_PING_TYPE GetActivePingType()
	{
		return m_activePingType;
	}

	public void BlockPings(bool block)
	{
		m_blockPings = block;
	}

	public bool ArePingsBlocked()
	{
		return m_blockPings;
	}

	public void RemovePing()
	{
		Spell pingSpell = GetSpell(SpellType.TEAMMATE_PING);
		if (pingSpell != null)
		{
			pingSpell.ActivateState(SpellStateType.DEATH);
			SetActivePingType(TEAMMATE_PING_TYPE.INVALID);
		}
		if (TeammatePingWheelManager.Get() != null)
		{
			TeammatePingWheelManager.Get().RemoveActorHasPing(this);
		}
	}

	public void RemovePingAndNotifyTeammate()
	{
		if (!(TeammateBoardViewer.Get() == null))
		{
			bool cardIsTeammates = IsTeammateActor();
			if (GetActivePingType() != 0)
			{
				Network.Get().SendPingTeammateEntity(GetEntity().GetEntityId(), 0, cardIsTeammates);
			}
			RemovePing();
		}
	}

	public void ActivatePing(TEAMMATE_PING_TYPE pingType)
	{
		Spell pingSpell = GetSpell(SpellType.TEAMMATE_PING);
		if (pingSpell != null)
		{
			pingSpell.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmInt("PingType").Value = (int)pingType;
			pingSpell.ActivateState(SpellStateType.BIRTH);
			SetActivePingType(pingType);
		}
		if (TeammatePingWheelManager.Get() != null)
		{
			TeammatePingWheelManager.Get().AddActorHasPing(this);
		}
	}

	public void PingSelected(TEAMMATE_PING_TYPE pingType)
	{
		if (pingType == TEAMMATE_PING_TYPE.INVALID)
		{
			RemovePing();
		}
		else
		{
			ActivatePing(pingType);
		}
	}

	public void UpdateUnplayableChainsOnTitanSubcards()
	{
		Entity entity = GetEntity();
		if (entity == null)
		{
			return;
		}
		Entity parentEntity = entity.GetParentEntity();
		if (parentEntity == null || !parentEntity.IsTitan() || parentEntity.GetRealTimeZone() != TAG_ZONE.PLAY)
		{
			return;
		}
		if (entity.HasTag(GAME_TAG.LITERALLY_UNPLAYABLE))
		{
			Spell spell = GetSpell(SpellType.UNPLAYABLE_VISUALS);
			if (spell != null)
			{
				SpellStateType state = spell.GetActiveState();
				if (state == SpellStateType.DEATH || state == SpellStateType.NONE)
				{
					spell.ActivateState(SpellStateType.BIRTH);
				}
			}
		}
		else
		{
			Spell spell2 = GetSpellIfLoaded(SpellType.UNPLAYABLE_VISUALS);
			if (spell2 != null && spell2.IsActive())
			{
				spell2.ActivateState(SpellStateType.DEATH);
			}
		}
	}

	public void UpdateTitanUsableAbilityGlow(ActorStateType prevState, ActorStateType newState)
	{
		Card card = GetCard();
		if (card == null)
		{
			return;
		}
		Entity entity = card.GetEntity();
		if (entity != null && entity.GetController() != null && entity.IsTitan())
		{
			Spell spell = GetSpell(SpellType.TITAN);
			if (prevState != ActorStateType.CARD_TITAN_ABILITY && prevState != ActorStateType.CARD_TITAN_ABILITY_MOUSE_OVER && (newState == ActorStateType.CARD_TITAN_ABILITY || newState == ActorStateType.CARD_TITAN_ABILITY_MOUSE_OVER))
			{
				SpellUtils.ActivateBirthIfNecessary(spell);
			}
			if ((prevState == ActorStateType.CARD_TITAN_ABILITY || prevState == ActorStateType.CARD_TITAN_ABILITY_MOUSE_OVER) && newState != ActorStateType.CARD_TITAN_ABILITY && newState != ActorStateType.CARD_TITAN_ABILITY_MOUSE_OVER)
			{
				SpellUtils.ActivateDeathIfNecessary(spell);
			}
		}
	}

	public void UpdateTitanPips(GAME_TAG tagChanged = GAME_TAG.TAG_NOT_SET, int newValue = 1)
	{
		Card card = GetCard();
		if (card == null)
		{
			return;
		}
		Entity entity = card.GetEntity();
		if (entity == null || !entity.IsTitan() || entity.GetZone() != TAG_ZONE.PLAY)
		{
			return;
		}
		Spell spell = GetSpell(SpellType.TITAN_PIPS);
		if (spell == null)
		{
			return;
		}
		if (spell.GetSource() != card.gameObject)
		{
			spell.SetSource(card.gameObject);
		}
		PlayMakerFSM spellFsm = spell.GetComponent<PlayMakerFSM>();
		if (spellFsm == null)
		{
			return;
		}
		if (newValue == 0)
		{
			spellFsm.SendEvent("Birth");
			return;
		}
		switch (tagChanged)
		{
		case GAME_TAG.TITAN_ABILITY_USED_1:
			spellFsm.SendEvent("DeactivateLeftPip");
			break;
		case GAME_TAG.TITAN_ABILITY_USED_2:
			spellFsm.SendEvent("DeactivateCenterPip");
			break;
		case GAME_TAG.TITAN_ABILITY_USED_3:
			spellFsm.SendEvent("DeactivateRightPip");
			break;
		}
	}

	public virtual void UpdateCardRuneBannerComponent()
	{
		if (!(m_cardRuneBanner == null))
		{
			m_cardRuneBanner.Hide();
			RunePattern runePattern = default(RunePattern);
			if (m_entity != null && m_entity.HasRuneCost)
			{
				runePattern.SetCostsFromEntity(m_entity);
				m_cardRuneBanner.Show(runePattern);
			}
			else if (m_entityDef != null && m_entityDef.HasRuneCost)
			{
				runePattern.SetCostsFromEntity(m_entityDef);
				m_cardRuneBanner.Show(runePattern);
			}
		}
	}

	public void UpdateMulticlassRibbon()
	{
		if (m_multiclassRibbon != null)
		{
			List<TAG_CLASS> classes = GetClasses();
			m_multiclassRibbon.SetActive(classes.Count > 2);
		}
	}

	public void UpdateDeckRunesComponent(CollectionDeck deck)
	{
		if (!(m_deckRunesContainer == null))
		{
			if (deck.ShouldShowDeathKnightRunes())
			{
				m_deckRunesContainer.SetActive(value: true);
				m_deckRuneSlotVisual.Show(deck.GetRuneOrder());
			}
			else
			{
				m_deckRunesContainer.SetActive(value: false);
			}
		}
	}

	private void UpdatePremiumComponents()
	{
		if (m_premiumType != 0 && !(m_glints == null))
		{
			m_glints.SetActive(value: true);
			Renderer[] componentsInChildren = m_glints.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = true;
			}
		}
	}

	private static void OffsetDescriptionTexture(GameObject meshObject, string textureId, bool withRace)
	{
		if (!(meshObject != null))
		{
			return;
		}
		Renderer renderer = meshObject.GetComponent<Renderer>();
		if (renderer != null)
		{
			Material material = GetMaterialInstance(renderer);
			if (material != null && material.HasProperty(textureId))
			{
				float currentX = material.GetTextureOffset(textureId).x;
				float newY = (withRace ? descriptionMesh_WithRace_TextureOffset : descriptionMesh_WithoutRace_TextureOffset);
				material.SetTextureOffset(textureId, new UnityEngine.Vector2(currentX, newY));
			}
		}
	}

	private void UpdateRace()
	{
		if (m_entityDef == null && m_entity == null)
		{
			return;
		}
		string raceText = ((m_entity != null) ? m_entity.GetRaceText() : m_entityDef.GetRaceText());
		bool isMinion = ((m_entity != null) ? m_entity.IsMinion() : m_entityDef.IsMinion());
		bool isLocation = ((m_entity != null) ? m_entity.IsLocation() : m_entityDef.IsLocation());
		bool isSpell = ((m_entity == null) ? (m_entityDef.IsSpell() || m_entityDef.IsBaconSpell()) : (m_entity.IsSpell() || m_entity.IsBaconSpell()));
		bool isWeapon = ((m_entity != null) ? m_entity.IsWeapon() : m_entityDef.IsWeapon());
		bool isHero = ((m_entity != null) ? m_entity.IsHero() : m_entityDef.IsHero());
		bool isMercenaryAbility = ((m_entity != null) ? m_entity.IsLettuceAbility() : m_entityDef.IsLettuceAbility());
		bool isBGTrinket = ((m_entity != null) ? m_entity.IsBattlegroundTrinket() : m_entityDef.IsBattlegroundTrinket());
		if (((isMinion || isLocation) && m_racePlateObject == null) || isHero || ((isSpell || isMercenaryAbility) && (m_descriptionMesh == null || m_spellDescriptionMeshNeutral == null || m_spellDescriptionMeshSchool == null)))
		{
			return;
		}
		bool showRace = !string.IsNullOrEmpty(raceText);
		int raceCount = 0;
		if (showRace)
		{
			raceCount = ((isSpell || isMercenaryAbility) ? ((m_entity == null) ? ((m_entityDef.GetSpellSchool() != 0) ? 1 : 0) : (m_entity.HasTag(GAME_TAG.SPELL_SCHOOL) ? 1 : 0)) : ((!isBGTrinket) ? ((m_entity != null) ? m_entity.GetRaceCount() : m_entityDef.GetRaceCount()) : ((m_entity == null) ? ((m_entityDef.GetTag(GAME_TAG.SPELL_SCHOOL) != 0) ? 1 : 0) : (m_entity.HasTag(GAME_TAG.SPELL_SCHOOL) ? 1 : 0))));
		}
		if (isMinion || isLocation)
		{
			if (m_racePlateObject != null)
			{
				bool setSingleRacePlate = showRace && raceCount == 1;
				MeshRenderer[] components = m_racePlateObject.GetComponents<MeshRenderer>();
				for (int i = 0; i < components.Length; i++)
				{
					components[i].enabled = setSingleRacePlate;
				}
			}
			if (m_multiRacePlateObject != null)
			{
				bool setMultiRacePlate = showRace && raceCount > 1;
				m_multiRacePlateObject.SetActive(setMultiRacePlate);
				MeshRenderer[] components = m_multiRacePlateObject.GetComponentsInChildren<MeshRenderer>();
				for (int i = 0; i < components.Length; i++)
				{
					components[i].enabled = setMultiRacePlate;
				}
			}
		}
		else if (isSpell || isMercenaryAbility)
		{
			MeshFilter[] components2 = m_descriptionMesh.GetComponents<MeshFilter>();
			foreach (MeshFilter filter in components2)
			{
				if (showRace)
				{
					filter.sharedMesh = m_spellDescriptionMeshSchool;
				}
				else
				{
					filter.sharedMesh = m_spellDescriptionMeshNeutral;
				}
			}
		}
		bool offsetWithRace = showRace || isWeapon || isLocation;
		OffsetDescriptionTexture(m_descriptionMesh, "_SecondTex", offsetWithRace);
		OffsetDescriptionTexture(m_watermarkMesh, "_MainTex", offsetWithRace);
		if (m_raceTextMesh == null && m_bgQuestRaceTextMesh == null)
		{
			return;
		}
		if (showRace && (Localization.GetLocale() == Locale.thTH || raceCount > 1))
		{
			if (m_raceTextMesh != null)
			{
				m_raceTextMesh.ResizeToFit = false;
				m_raceTextMesh.ResizeToFitAndGrow = false;
			}
			if (m_bgQuestRaceTextMesh != null)
			{
				m_bgQuestRaceTextMesh.ResizeToFit = false;
				m_bgQuestRaceTextMesh.ResizeToFitAndGrow = false;
			}
		}
		if (m_raceTextMesh != null)
		{
			if (raceCount == 1 && showRace)
			{
				m_raceTextMesh.gameObject.SetActive(m_isTextVisible);
				m_raceTextMesh.Text = raceText;
			}
			else
			{
				m_raceTextMesh.gameObject.SetActive(value: false);
			}
		}
		if (m_multiRaceTextMesh != null)
		{
			if (raceCount > 1 && showRace)
			{
				m_multiRaceTextMesh.gameObject.SetActive(m_isTextVisible);
				m_multiRaceTextMesh.Text = raceText;
			}
			else
			{
				m_multiRaceTextMesh.gameObject.SetActive(value: false);
			}
		}
		if (m_bgQuestRaceTextMesh != null)
		{
			m_bgQuestRaceTextMesh.Text = raceText;
		}
	}

	public static Material GetMaterialInstance(Renderer r)
	{
		return r.GetMaterial();
	}

	public HearthstoneFactionBanner GetHearthstoneFactionBanner()
	{
		return m_hearthstoneFactionBanner;
	}

	public CardColorSwitcher.CardColorType GetCardColorTypeForClass(TAG_CLASS classType)
	{
		CardColorSwitcher.CardColorType colorType = CardColorSwitcher.CardColorType.TYPE_GENERIC;
		switch (classType)
		{
		case TAG_CLASS.WARLOCK:
			colorType = CardColorSwitcher.CardColorType.TYPE_WARLOCK;
			break;
		case TAG_CLASS.ROGUE:
			colorType = CardColorSwitcher.CardColorType.TYPE_ROGUE;
			break;
		case TAG_CLASS.DRUID:
			colorType = CardColorSwitcher.CardColorType.TYPE_DRUID;
			break;
		case TAG_CLASS.HUNTER:
			colorType = CardColorSwitcher.CardColorType.TYPE_HUNTER;
			break;
		case TAG_CLASS.MAGE:
			colorType = CardColorSwitcher.CardColorType.TYPE_MAGE;
			break;
		case TAG_CLASS.PALADIN:
			colorType = CardColorSwitcher.CardColorType.TYPE_PALADIN;
			break;
		case TAG_CLASS.PRIEST:
			colorType = CardColorSwitcher.CardColorType.TYPE_PRIEST;
			break;
		case TAG_CLASS.SHAMAN:
			colorType = CardColorSwitcher.CardColorType.TYPE_SHAMAN;
			break;
		case TAG_CLASS.WARRIOR:
			colorType = CardColorSwitcher.CardColorType.TYPE_WARRIOR;
			break;
		case TAG_CLASS.DREAM:
			colorType = CardColorSwitcher.CardColorType.TYPE_HUNTER;
			break;
		case TAG_CLASS.DEATHKNIGHT:
			colorType = CardColorSwitcher.CardColorType.TYPE_DEATHKNIGHT;
			break;
		case TAG_CLASS.DEMONHUNTER:
			colorType = CardColorSwitcher.CardColorType.TYPE_DEMONHUNTER;
			break;
		}
		return colorType;
	}

	private List<TAG_CLASS> GetGameHeroClasses()
	{
		List<TAG_CLASS> result = new List<TAG_CLASS>();
		Entity entity = GetEntity();
		if (entity == null)
		{
			return result;
		}
		Entity hero = entity.GetHero();
		if (hero == null)
		{
			return result;
		}
		hero.GetClasses(result);
		return result;
	}

	private List<TAG_CLASS> GetCollectionManagerClasses()
	{
		List<TAG_CLASS> result = new List<TAG_CLASS>();
		CollectionManager cm = CollectionManager.Get();
		if (cm == null)
		{
			return result;
		}
		CollectionManagerDisplay collectionManagerDisplay = cm.GetCollectibleDisplay() as CollectionManagerDisplay;
		if (collectionManagerDisplay == null)
		{
			return result;
		}
		CollectionPageManager collectionPageManager = collectionManagerDisplay.GetPageManager() as CollectionPageManager;
		if (collectionPageManager == null)
		{
			return result;
		}
		result.Add(collectionPageManager.GetCurrentClassContextClassTag());
		return result;
	}

	private List<TAG_CLASS> GetArenaClasses()
	{
		List<TAG_CLASS> result = new List<TAG_CLASS>();
		DraftDisplay draftDisplay = DraftDisplay.Get();
		if (draftDisplay != null)
		{
			Actor chosenHero = draftDisplay.ChosenHero;
			if (chosenHero != null)
			{
				result.AddRange(chosenHero.GetClasses());
			}
		}
		return result;
	}

	private TAG_CLASS GetClassMatchingHero()
	{
		List<TAG_CLASS> contextClasses = GetGameHeroClasses();
		if (contextClasses.Count <= 0)
		{
			contextClasses = GetCollectionManagerClasses();
		}
		if (contextClasses.Count <= 0)
		{
			contextClasses = GetArenaClasses();
		}
		List<TAG_CLASS> entityClasses = GetClasses();
		foreach (TAG_CLASS heroClass in contextClasses)
		{
			foreach (TAG_CLASS entityClass in entityClasses)
			{
				if (heroClass == entityClass)
				{
					return entityClass;
				}
			}
		}
		return TAG_CLASS.INVALID;
	}

	public void UpdateCardColor()
	{
		if ((m_legacyPortraitMaterialIndex < 0 && m_cardMesh == null) || (GetEntityDef() == null && GetEntity() == null))
		{
			return;
		}
		bool isMercenary = false;
		bool isMinionAbility = false;
		bool isMulticlass = IsMultiClass();
		Player.Side playerSide = ((m_entity == null) ? Player.Side.FRIENDLY : m_entity.GetControllerSide());
		bool isBattlegroundsTrinket = false;
		TAG_CARDTYPE cardType;
		TAG_CLASS classType;
		TAG_ROLE roleType;
		if (m_entityDef != null)
		{
			cardType = m_entityDef.GetCardType();
			classType = m_entityDef.GetClass();
			roleType = (TAG_ROLE)m_entityDef.GetTag(GAME_TAG.LETTUCE_ROLE);
			isMercenary = m_entityDef.IsLettuceMercenary();
			isMinionAbility = m_entityDef.IsLettuceAbilityMinionSummoning();
			m_entityDef.GetCardId();
			isBattlegroundsTrinket = m_entityDef.IsBattlegroundTrinket();
		}
		else if (m_entity != null)
		{
			cardType = m_entity.GetCardType();
			classType = m_entity.GetClass();
			roleType = (TAG_ROLE)m_entity.GetTag(GAME_TAG.LETTUCE_ROLE);
			isMercenary = m_entity.IsLettuceMercenary();
			isMinionAbility = m_entity.IsLettuceAbilityMinionSummoning();
			m_entity.GetCardId();
			isBattlegroundsTrinket = m_entity.IsBattlegroundTrinket();
		}
		else
		{
			cardType = TAG_CARDTYPE.INVALID;
			classType = TAG_CLASS.INVALID;
			roleType = TAG_ROLE.INVALID;
		}
		Color cardColor = Color.magenta;
		CardColorSwitcher.CardColorType colorType = CardColorSwitcher.CardColorType.TYPE_GENERIC;
		if (isMercenary)
		{
			int premiumOffset = (int)m_premiumType;
			switch (roleType)
			{
			case TAG_ROLE.CASTER:
				colorType = (CardColorSwitcher.CardColorType)(22 + premiumOffset);
				break;
			case TAG_ROLE.FIGHTER:
				colorType = (CardColorSwitcher.CardColorType)(25 + premiumOffset);
				break;
			case TAG_ROLE.TANK:
				colorType = (CardColorSwitcher.CardColorType)(28 + premiumOffset);
				break;
			case TAG_ROLE.NEUTRAL:
				colorType = (CardColorSwitcher.CardColorType)(31 + premiumOffset);
				break;
			}
		}
		else if (cardType == TAG_CARDTYPE.LETTUCE_ABILITY)
		{
			switch (roleType)
			{
			case TAG_ROLE.CASTER:
				colorType = (isMinionAbility ? CardColorSwitcher.CardColorType.TYPE_MERCENARIES_NEUTRAL_TIER_2 : CardColorSwitcher.CardColorType.TYPE_MERCENARIES_NEUTRAL_TIER_1);
				break;
			case TAG_ROLE.FIGHTER:
				colorType = (isMinionAbility ? CardColorSwitcher.CardColorType.TYPE_MERCENARIES_ABILITY_FIGHTER_MINION : CardColorSwitcher.CardColorType.TYPE_MERCENARIES_NEUTRAL_TIER_3);
				break;
			case TAG_ROLE.TANK:
				colorType = (isMinionAbility ? CardColorSwitcher.CardColorType.TYPE_MERCENARIES_ABILITY_TANK_MINION : CardColorSwitcher.CardColorType.TYPE_MERCENARIES_ABILITY_TANK_SPELL);
				break;
			case TAG_ROLE.NEUTRAL:
				colorType = (isMinionAbility ? CardColorSwitcher.CardColorType.TYPE_MERCENARIES_ABILITY_NEUTRAL_MINION : CardColorSwitcher.CardColorType.TYPE_MERCENARIES_ABILITY_NEUTRAL_SPELL);
				break;
			case TAG_ROLE.INVALID:
				return;
			}
		}
		else if (isBattlegroundsTrinket)
		{
			colorType = ((playerSide == Player.Side.OPPOSING) ? CardColorSwitcher.CardColorType.TYPE_BATTLEGROUNDS_TRINKET_OPPONENT : CardColorSwitcher.CardColorType.TYPE_BATTLEGROUNDS_TRINKET_FRIENDLY);
		}
		else
		{
			colorType = CardColorSwitcher.GetCardColorTypeForClass(classType);
			cardColor = GetClassColor(colorType);
		}
		if (isMulticlass)
		{
			colorType = CardColorSwitcher.GetCardColorTypeForClasses(GetClasses());
			if (colorType == CardColorSwitcher.CardColorType.TYPE_GENERIC)
			{
				TAG_CLASS matchingClassType = GetClassMatchingHero();
				colorType = GetCardColorTypeForClass(matchingClassType);
			}
			if (colorType == CardColorSwitcher.CardColorType.TYPE_GENERIC)
			{
				colorType = CardColorSwitcher.GetCardColorTypeForClass(GetClassMatchingHero());
			}
			cardColor = GetClassColor(colorType);
		}
		else if (m_premiumRibbon > -1 && m_initialPremiumRibbonMaterial != null)
		{
			Renderer renderer = m_cardMesh.GetComponent<Renderer>();
			if (m_premiumRibbon < renderer.GetMaterials().Count)
			{
				renderer.SetMaterial(m_premiumRibbon, m_initialPremiumRibbonMaterial);
			}
		}
		bool hasHearthstoneFaction = HasHearthstoneFaction();
		bool isTradeable = IsTradeable();
		bool isForgeable = IsForgeable();
		m_hearthstoneFactionBannerContainer?.gameObject.SetActive(hasHearthstoneFaction);
		m_tradeableBannerContainer?.gameObject.SetActive(isTradeable);
		m_forgeBannerContainer?.gameObject.SetActive(isForgeable);
		if (hasHearthstoneFaction && m_hearthstoneFactionBannerContainer != null)
		{
			m_hearthstoneFactionBanner = m_hearthstoneFactionBannerContainer.PrefabGameObject(instantiateIfNeeded: true).GetComponent<HearthstoneFactionBanner>();
			m_hearthstoneFactionBanner.SetFactionType(m_premiumType, CardColorSwitcher.GetFactionColorTypeForTag(GetHearthstoneFaction()));
		}
		if (isTradeable && m_tradeableBannerContainer != null)
		{
			m_deckActionBanner = m_tradeableBannerContainer.PrefabGameObject(instantiateIfNeeded: true).GetComponent<DeckActionBanner>();
		}
		else if (isForgeable && m_forgeBannerContainer != null)
		{
			m_deckActionBanner = m_forgeBannerContainer.PrefabGameObject(instantiateIfNeeded: true).GetComponent<DeckActionBanner>();
		}
		else
		{
			m_deckActionBanner = null;
		}
		if (isMercenary && m_premiumType != 0)
		{
			SetMaterialNormal(cardType, colorType, cardColor);
		}
		SetMaterial(cardType, colorType, cardColor);
	}

	public void UpdateManaGemOffset()
	{
		if (m_manaObject != null && m_costTextMesh != null && m_useAlternateCostTextPos)
		{
			m_costTextMesh.transform.localPosition = m_alternateCostTextLocalPos;
		}
	}

	public void SetDeckActionHighlightState(DeckActionHighlightState state)
	{
		if (m_deckActionBanner != null)
		{
			m_deckActionBanner.SetHighlightState(state);
		}
	}

	private void SetMaterial(TAG_CARDTYPE cardType, CardColorSwitcher.CardColorType colorType, Color cardColor)
	{
		switch (m_premiumType)
		{
		case TAG_PREMIUM.GOLDEN:
		case TAG_PREMIUM.DIAMOND:
		case TAG_PREMIUM.SIGNATURE:
			SetMaterialPremium(cardType, colorType, cardColor);
			break;
		case TAG_PREMIUM.NORMAL:
			SetMaterialNormal(cardType, colorType, cardColor);
			break;
		default:
			Debug.LogWarning($"Actor.SetMaterial(): unexpected premium type {m_premiumType}");
			break;
		}
	}

	private void SetHistoryHeroBannerColor()
	{
		if (m_entity != null && !m_entity.IsControlledByFriendlySidePlayer() && m_entity.IsHistoryDupe())
		{
			Transform heroBanner = GetRootObject().transform.Find("History_Hero_Banner");
			if (!(heroBanner == null))
			{
				GetMaterialInstance(heroBanner.GetComponent<Renderer>()).mainTextureOffset = new UnityEngine.Vector2(0.005f, -0.505f);
			}
		}
	}

	private Color GetClassColor(CardColorSwitcher.CardColorType colorType)
	{
		if (IsLocation())
		{
			return GetLocationClassColor(colorType);
		}
		Color cardColor = CLASS_COLOR_GENERIC;
		switch (colorType)
		{
		case CardColorSwitcher.CardColorType.TYPE_WARLOCK:
			cardColor = CLASS_COLOR_WARLOCK;
			break;
		case CardColorSwitcher.CardColorType.TYPE_ROGUE:
			cardColor = CLASS_COLOR_ROGUE;
			break;
		case CardColorSwitcher.CardColorType.TYPE_DRUID:
			cardColor = CLASS_COLOR_DRUID;
			break;
		case CardColorSwitcher.CardColorType.TYPE_HUNTER:
			cardColor = CLASS_COLOR_HUNTER;
			break;
		case CardColorSwitcher.CardColorType.TYPE_MAGE:
			cardColor = CLASS_COLOR_MAGE;
			break;
		case CardColorSwitcher.CardColorType.TYPE_PALADIN:
			cardColor = CLASS_COLOR_PALADIN;
			break;
		case CardColorSwitcher.CardColorType.TYPE_PRIEST:
			cardColor = CLASS_COLOR_PRIEST;
			break;
		case CardColorSwitcher.CardColorType.TYPE_SHAMAN:
			cardColor = CLASS_COLOR_SHAMAN;
			break;
		case CardColorSwitcher.CardColorType.TYPE_WARRIOR:
			cardColor = CLASS_COLOR_WARRIOR;
			break;
		case CardColorSwitcher.CardColorType.TYPE_DEATHKNIGHT:
			cardColor = CLASS_COLOR_DEATHKNIGHT;
			break;
		case CardColorSwitcher.CardColorType.TYPE_DEMONHUNTER:
			cardColor = CLASS_COLOR_DEMONHUNTER;
			break;
		}
		return cardColor;
	}

	private void GetDualClassColors(CardColorSwitcher.CardColorType dualClassCombo, out Color left, out Color right)
	{
		switch (dualClassCombo)
		{
		case CardColorSwitcher.CardColorType.TYPE_PALADIN_PRIEST:
			left = CLASS_COLOR_PALADIN;
			right = CLASS_COLOR_PRIEST;
			break;
		case CardColorSwitcher.CardColorType.TYPE_WARLOCK_PRIEST:
			left = CLASS_COLOR_PRIEST;
			right = CLASS_COLOR_WARLOCK;
			break;
		case CardColorSwitcher.CardColorType.TYPE_WARLOCK_DEMONHUNTER:
			left = CLASS_COLOR_WARLOCK;
			right = CLASS_COLOR_DEMONHUNTER;
			break;
		case CardColorSwitcher.CardColorType.TYPE_HUNTER_DEMONHUNTER:
			left = CLASS_COLOR_DEMONHUNTER;
			right = CLASS_COLOR_HUNTER;
			break;
		case CardColorSwitcher.CardColorType.TYPE_DRUID_HUNTER:
			left = CLASS_COLOR_HUNTER;
			right = CLASS_COLOR_DRUID;
			break;
		case CardColorSwitcher.CardColorType.TYPE_DRUID_SHAMAN:
			left = CLASS_COLOR_DRUID;
			right = CLASS_COLOR_SHAMAN;
			break;
		case CardColorSwitcher.CardColorType.TYPE_SHAMAN_MAGE:
			left = CLASS_COLOR_SHAMAN;
			right = CLASS_COLOR_MAGE;
			break;
		case CardColorSwitcher.CardColorType.TYPE_MAGE_ROGUE:
			left = CLASS_COLOR_MAGE;
			right = CLASS_COLOR_ROGUE;
			break;
		case CardColorSwitcher.CardColorType.TYPE_WARRIOR_ROGUE:
			left = CLASS_COLOR_ROGUE;
			right = CLASS_COLOR_WARRIOR;
			break;
		case CardColorSwitcher.CardColorType.TYPE_WARRIOR_PALADIN:
			left = CLASS_COLOR_WARRIOR;
			right = CLASS_COLOR_PALADIN;
			break;
		case CardColorSwitcher.CardColorType.TYPE_MAGE_HUNTER:
			left = CLASS_COLOR_MAGE;
			right = CLASS_COLOR_HUNTER;
			break;
		case CardColorSwitcher.CardColorType.TYPE_HUNTER_DEATHKNIGHT:
			left = CLASS_COLOR_HUNTER;
			right = CLASS_COLOR_DEATHKNIGHT;
			break;
		case CardColorSwitcher.CardColorType.TYPE_DEATHKNIGHT_PALADIN:
			left = CLASS_COLOR_DEATHKNIGHT;
			right = CLASS_COLOR_PALADIN;
			break;
		case CardColorSwitcher.CardColorType.TYPE_PALADIN_SHAMAN:
			left = CLASS_COLOR_PALADIN;
			right = CLASS_COLOR_SHAMAN;
			break;
		case CardColorSwitcher.CardColorType.TYPE_SHAMAN_WARRIOR:
			left = CLASS_COLOR_SHAMAN;
			right = CLASS_COLOR_WARRIOR;
			break;
		case CardColorSwitcher.CardColorType.TYPE_WARRIOR_DEMONHUNTER:
			left = CLASS_COLOR_WARRIOR;
			right = CLASS_COLOR_DEMONHUNTER;
			break;
		case CardColorSwitcher.CardColorType.TYPE_DEMONHUNTER_ROGUE:
			left = CLASS_COLOR_DEMONHUNTER;
			right = CLASS_COLOR_ROGUE;
			break;
		case CardColorSwitcher.CardColorType.TYPE_ROGUE_PRIEST:
			left = CLASS_COLOR_ROGUE;
			right = CLASS_COLOR_PRIEST;
			break;
		case CardColorSwitcher.CardColorType.TYPE_PRIEST_DRUID:
			left = CLASS_COLOR_PRIEST;
			right = CLASS_COLOR_DRUID;
			break;
		case CardColorSwitcher.CardColorType.TYPE_DRUID_WARLOCK:
			left = CLASS_COLOR_DRUID;
			right = CLASS_COLOR_WARLOCK;
			break;
		case CardColorSwitcher.CardColorType.TYPE_WARLOCK_MAGE:
			left = CLASS_COLOR_WARLOCK;
			right = CLASS_COLOR_MAGE;
			break;
		case CardColorSwitcher.CardColorType.TYPE_WARRIOR_WARLOCK:
			left = CLASS_COLOR_WARRIOR;
			right = CLASS_COLOR_WARLOCK;
			break;
		default:
			left = (right = Color.magenta);
			break;
		}
	}

	private Color GetLocationClassColor(CardColorSwitcher.CardColorType colorType)
	{
		Color cardColor = CLASS_COLOR_LOCATION_GENERIC;
		switch (colorType)
		{
		case CardColorSwitcher.CardColorType.TYPE_WARLOCK:
			cardColor = CLASS_COLOR_LOCATION_WARLOCK;
			break;
		case CardColorSwitcher.CardColorType.TYPE_ROGUE:
			cardColor = CLASS_COLOR_LOCATION_ROGUE;
			break;
		case CardColorSwitcher.CardColorType.TYPE_DRUID:
			cardColor = CLASS_COLOR_LOCATION_DRUID;
			break;
		case CardColorSwitcher.CardColorType.TYPE_HUNTER:
			cardColor = CLASS_COLOR_LOCATION_HUNTER;
			break;
		case CardColorSwitcher.CardColorType.TYPE_MAGE:
			cardColor = CLASS_COLOR_LOCATION_MAGE;
			break;
		case CardColorSwitcher.CardColorType.TYPE_PALADIN:
			cardColor = CLASS_COLOR_LOCATION_PALADIN;
			break;
		case CardColorSwitcher.CardColorType.TYPE_PRIEST:
			cardColor = CLASS_COLOR_LOCATION_PRIEST;
			break;
		case CardColorSwitcher.CardColorType.TYPE_SHAMAN:
			cardColor = CLASS_COLOR_LOCATION_SHAMAN;
			break;
		case CardColorSwitcher.CardColorType.TYPE_WARRIOR:
			cardColor = CLASS_COLOR_LOCATION_WARRIOR;
			break;
		case CardColorSwitcher.CardColorType.TYPE_DEATHKNIGHT:
			cardColor = CLASS_COLOR_LOCATION_DEATHKNIGHT;
			break;
		case CardColorSwitcher.CardColorType.TYPE_DEMONHUNTER:
			cardColor = CLASS_COLOR_LOCATION_DEMONHUNTER;
			break;
		}
		return cardColor;
	}

	private void SetMaterialPremium(TAG_CARDTYPE cardType, CardColorSwitcher.CardColorType colorType, Color cardColor)
	{
		if (m_cardMesh != null && m_premiumRibbon >= 0)
		{
			Material ribbonMat = m_cardMesh.GetComponent<Renderer>().GetMaterial(m_premiumRibbon);
			if (colorType >= CardColorSwitcher.CardColorType.TYPE_GENERIC && colorType <= CardColorSwitcher.CardColorType.TYPE_DEMONHUNTER)
			{
				ribbonMat.color = cardColor;
				ribbonMat.SetFloat("_EnableDualClass", 0f);
			}
			else
			{
				GetDualClassColors(colorType, out var leftColor, out var rightColor);
				ribbonMat.SetFloat("_EnableDualClass", 1f);
				ribbonMat.SetColor("_Color", leftColor);
				ribbonMat.SetColor("_SecondColor", rightColor);
			}
		}
		if (cardType == TAG_CARDTYPE.HERO)
		{
			SetHistoryHeroBannerColor();
		}
	}

	private void SetMaterialNormal(TAG_CARDTYPE cardType, CardColorSwitcher.CardColorType colorType, Color cardColor)
	{
		switch (cardType)
		{
		case TAG_CARDTYPE.WEAPON:
			SetMaterialWeapon(colorType, cardColor);
			break;
		case TAG_CARDTYPE.HERO:
			SetMaterialHero(colorType);
			break;
		default:
			SetMaterialWithTexture(cardType, colorType);
			break;
		case TAG_CARDTYPE.HERO_POWER:
		case TAG_CARDTYPE.GAME_MODE_BUTTON:
			break;
		}
	}

	protected virtual void SetMaterialWithTexture(TAG_CARDTYPE cardType, CardColorSwitcher.CardColorType colorType)
	{
		if (CardColorSwitcher.Get() == null)
		{
			return;
		}
		AssetReference assetRef = CardColorSwitcher.Get().GetTexture(cardType, colorType);
		if (assetRef == null)
		{
			return;
		}
		AssetLoader.Get().LoadAsset(ref m_cardColorTex, assetRef);
		if (m_cardMesh != null)
		{
			if (m_cardFrontMatIdx > -1)
			{
				m_cardMesh.GetComponent<Renderer>().GetMaterial(m_cardFrontMatIdx).mainTexture = m_cardColorTex;
			}
			if ((cardType == TAG_CARDTYPE.SPELL || cardType == TAG_CARDTYPE.BATTLEGROUND_SPELL || (cardType == TAG_CARDTYPE.WEAPON && colorType == CardColorSwitcher.CardColorType.TYPE_DEATHKNIGHT)) && (bool)m_portraitMesh && m_portraitFrameMatIdx > -1)
			{
				m_portraitMesh.GetComponent<Renderer>().GetMaterial(m_portraitFrameMatIdx).mainTexture = m_cardColorTex;
			}
		}
		else if (m_legacyCardColorMaterialIndex >= 0 && m_meshRenderer != null)
		{
			m_meshRenderer.GetMaterial(m_legacyCardColorMaterialIndex).mainTexture = m_cardColorTex;
		}
		if (cardType == TAG_CARDTYPE.LOCATION && m_rarityNoGemMesh != null)
		{
			m_rarityNoGemMesh.GetComponent<Renderer>().GetMaterial().mainTexture = m_cardColorTex;
		}
	}

	private void SetMaterialHero(CardColorSwitcher.CardColorType colorType)
	{
		SetMaterialWithTexture(TAG_CARDTYPE.HERO, colorType);
		SetHistoryHeroBannerColor();
	}

	private void SetMaterialWeapon(CardColorSwitcher.CardColorType colorType, Color cardColor)
	{
		if ((bool)CardColorSwitcher.Get() && !string.IsNullOrEmpty(CardColorSwitcher.Get().GetTexture(TAG_CARDTYPE.WEAPON, colorType)))
		{
			SetMaterialWithTexture(TAG_CARDTYPE.WEAPON, colorType);
		}
		else if ((bool)m_descriptionTrimMesh)
		{
			GetMaterialInstance(m_descriptionTrimMesh.GetComponent<Renderer>()).SetColor("_Color", cardColor);
		}
	}

	public bool UseTechLevelManaGem()
	{
		GameEntity gameEntity = GetGameEntityIfAllowed();
		if (gameEntity == null || !gameEntity.HasTag(GAME_TAG.TECH_LEVEL_MANA_GEM))
		{
			return false;
		}
		if ((m_entity != null && m_entity.IsMinion()) || (m_entityDef != null && m_entityDef.IsMinion()))
		{
			return true;
		}
		bool isBaconSpellOrSpell = (m_entity != null && (m_entity.IsBaconSpell() || m_entity.IsSpell())) || (m_entityDef != null && (m_entityDef.IsBaconSpell() || m_entityDef.IsSpell()));
		if (((m_entity != null && m_entity.HasTag(GAME_TAG.TECH_LEVEL)) || (m_entityDef != null && m_entityDef.HasTag(GAME_TAG.TECH_LEVEL))) && isBaconSpellOrSpell)
		{
			return true;
		}
		return false;
	}

	public bool UseCoinManaGem()
	{
		return GetGameEntityIfAllowed()?.HasTag(GAME_TAG.COIN_MANA_GEM) ?? false;
	}

	public bool UseCoinManaGemForChoiceCard()
	{
		GameEntity gameEntity = GetGameEntityIfAllowed();
		if (gameEntity != null)
		{
			if (gameEntity.HasTag(GAME_TAG.COIN_MANA_GEM_FOR_CHOICE_CARDS))
			{
				if (GameMgr.Get().IsBattlegrounds() && m_entity != null && m_entity.IsQuest())
				{
					return m_entity.HasTag(GAME_TAG.BACON_SHOW_COST_ON_DISCOVER);
				}
				return true;
			}
			return false;
		}
		return false;
	}

	public HistoryCard GetHistoryCard()
	{
		if (base.transform.parent == null)
		{
			return null;
		}
		return base.transform.parent.gameObject.GetComponent<HistoryCard>();
	}

	public HistoryChildCard GetHistoryChildCard()
	{
		if (base.transform.parent == null)
		{
			return null;
		}
		return base.transform.parent.gameObject.GetComponent<HistoryChildCard>();
	}

	public void ConfigureForHistory(HistoryItem item)
	{
		base.transform.parent = item.transform;
		TransformUtil.Identity(base.transform);
		if (m_rootObject != null)
		{
			TransformUtil.Identity(m_rootObject.transform);
		}
		if (m_localSpellTable != null)
		{
			foreach (SpellTableEntry item2 in m_localSpellTable.m_Table)
			{
				Spell spell = item2.m_Spell;
				if (!(spell == null))
				{
					spell.m_BlockServerEvents = false;
				}
			}
		}
		TurnOffCollider();
		SetActorState(ActorStateType.CARD_HISTORY);
	}

	public void SetHistoryItem(HistoryItem card, bool reparent = true)
	{
		if (card == null)
		{
			if (reparent)
			{
				base.transform.parent = null;
			}
			return;
		}
		if (reparent)
		{
			base.transform.parent = card.transform;
			TransformUtil.Identity(base.transform);
			if (m_rootObject != null)
			{
				TransformUtil.Identity(m_rootObject.transform);
			}
		}
		m_entity = card.GetEntity();
		UpdateTextComponents(m_entity);
		UpdateMeshComponents();
		if (m_premiumType >= TAG_PREMIUM.GOLDEN && card.GetPortraitGoldenMaterial() != null && IsPremiumPortraitEnabled())
		{
			Material goldenMat = card.GetPortraitGoldenMaterial();
			SetPortraitMaterial(goldenMat);
		}
		else
		{
			Texture histTex = card.GetPortraitTexture();
			SetPortraitTextureOverride(histTex);
		}
		if (!(m_localSpellTable != null))
		{
			return;
		}
		foreach (SpellTableEntry item in m_localSpellTable.m_Table)
		{
			Spell spell = item.m_Spell;
			if (!(spell == null))
			{
				spell.m_BlockServerEvents = false;
			}
		}
	}

	public SpellTable GetSpellTable()
	{
		return m_localSpellTable;
	}

	public Spell LoadSpell(SpellType spellType)
	{
		Spell loadedSpell = null;
		if (m_card != null)
		{
			loadedSpell = m_card.GetSpellTableOverride(spellType) as Spell;
		}
		if (loadedSpell == null)
		{
			CardDef cardDef = m_cardDefHandle.Get(m_premiumType);
			if (cardDef != null)
			{
				foreach (SpellTableOverride spellTableOverride in cardDef.m_SpellTableOverrides)
				{
					if (spellTableOverride.m_Type != spellType)
					{
						continue;
					}
					if (!string.IsNullOrEmpty(spellTableOverride.m_SpellPrefabName))
					{
						loadedSpell = SpellManager.Get().GetSpell(spellTableOverride.m_SpellPrefabName);
						if (loadedSpell != null)
						{
							loadedSpell.SetSpellType(spellType);
						}
					}
					break;
				}
			}
		}
		if (loadedSpell == null)
		{
			TAG_CARD_SET cardSet = GetCardSet();
			string overridePrefabName = GameDbf.GetIndex().GetCardSetSpellOverride(cardSet, spellType);
			if (!string.IsNullOrEmpty(overridePrefabName))
			{
				loadedSpell = SpellManager.Get().GetSpell(overridePrefabName);
				if (loadedSpell != null)
				{
					loadedSpell.SetSpellType(spellType);
				}
			}
		}
		if (loadedSpell == null && m_sharedSpellTable != null)
		{
			loadedSpell = m_sharedSpellTable.GetSpellInstance(spellType);
		}
		if (loadedSpell == null)
		{
			return null;
		}
		if (m_ownedSpells.ContainsKey(spellType))
		{
			m_ownedSpells.Remove(spellType);
		}
		m_ownedSpells.Add(spellType, loadedSpell);
		Transform obj = loadedSpell.gameObject.transform;
		TransformUtil.AttachAndPreserveLocalTransform(obj, GetSpellParent());
		obj.localScale.Scale(m_sharedSpellTable.gameObject.transform.localScale);
		LayerUtils.SetLayer(loadedSpell.gameObject, (GameLayer)base.gameObject.layer);
		loadedSpell.AddSpellReleasedCallback(OnSpellRelease);
		loadedSpell.OnLoad();
		if (m_actorStateMgr != null)
		{
			loadedSpell.AddStateStartedCallback(OnSpellStateStarted);
		}
		if (spellType == SpellType.STARSHIP_LAUNCHPAD_NORMAL || spellType == SpellType.STARSHIP_LAUNCHPAD_GOLDEN || spellType == SpellType.STARSHIP_LAUNCHPAD_SIGNATURE)
		{
			UpdateLaunchpadComponents();
		}
		return loadedSpell;
	}

	private void OnSpellRelease(Spell spell)
	{
		m_ownedSpells.Remove(spell.GetSpellType());
		spell.RemoveSpellReleasedCallback(OnSpellRelease);
	}

	public Spell GetLoadedSpell(SpellType spellType)
	{
		Spell loadedSpell = null;
		if (m_ownedSpells != null)
		{
			m_ownedSpells.TryGetValue(spellType, out loadedSpell);
		}
		if (loadedSpell == null)
		{
			loadedSpell = LoadSpell(spellType);
		}
		return loadedSpell;
	}

	public bool IsTauntActive()
	{
		if (!IsSpellActive(SpellType.TAUNT) && !IsSpellActive(SpellType.TAUNT_STEALTH) && !IsSpellActive(SpellType.TAUNT_PREMIUM) && !IsSpellActive(SpellType.TAUNT_PREMIUM_STEALTH) && !IsSpellActive(SpellType.TAUNT_DIAMOND))
		{
			return IsSpellActive(SpellType.TAUNT_DIAMOND_STEALTH);
		}
		return true;
	}

	public bool IsDivineShieldActive()
	{
		return IsSpellActive(SpellType.DIVINE_SHIELD);
	}

	public void ActivateTaunt()
	{
		DeactivateTaunt();
		bool shouldShowTauntStealth = false;
		Entity ent = GetEntity();
		if (ent != null)
		{
			if (ent.IsLaunchpad())
			{
				return;
			}
			if (ent.IsStealthed() && !Options.Get().GetBool(Option.HAS_SEEN_STEALTH_TAUNTER, defaultVal: false))
			{
				NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, GameStrings.Get("VO_INNKEEPER_STEALTH_TAUNT3_22"), "VO_INNKEEPER_STEALTH_TAUNT3_22.prefab:7ec7cc35d1556434ebca64bfe4e770cb");
				Options.Get().SetBool(Option.HAS_SEEN_STEALTH_TAUNTER, val: true);
			}
			shouldShowTauntStealth = ent.IsStealthed() || ent.IsTauntIgnored() || ent.IsImmune();
		}
		switch (m_premiumType)
		{
		case TAG_PREMIUM.GOLDEN:
			if (shouldShowTauntStealth)
			{
				ActivateSpellBirthState(SpellType.TAUNT_PREMIUM_STEALTH);
			}
			else
			{
				ActivateSpellBirthState(SpellType.TAUNT_PREMIUM);
			}
			break;
		case TAG_PREMIUM.DIAMOND:
			if (shouldShowTauntStealth)
			{
				ActivateSpellBirthState(SpellType.TAUNT_DIAMOND_STEALTH);
			}
			else
			{
				ActivateSpellBirthState(SpellType.TAUNT_DIAMOND);
			}
			break;
		default:
			if (shouldShowTauntStealth)
			{
				ActivateSpellBirthState(SpellType.TAUNT_STEALTH);
			}
			else
			{
				ActivateSpellBirthState(SpellType.TAUNT);
			}
			break;
		}
	}

	public void DeactivateTaunt()
	{
		if (IsSpellActive(SpellType.TAUNT))
		{
			ActivateSpellDeathState(SpellType.TAUNT);
		}
		if (IsSpellActive(SpellType.TAUNT_PREMIUM))
		{
			ActivateSpellDeathState(SpellType.TAUNT_PREMIUM);
		}
		if (IsSpellActive(SpellType.TAUNT_PREMIUM_STEALTH))
		{
			ActivateSpellDeathState(SpellType.TAUNT_PREMIUM_STEALTH);
		}
		if (IsSpellActive(SpellType.TAUNT_STEALTH))
		{
			ActivateSpellDeathState(SpellType.TAUNT_STEALTH);
		}
		if (IsSpellActive(SpellType.TAUNT_DIAMOND))
		{
			ActivateSpellDeathState(SpellType.TAUNT_DIAMOND);
		}
		if (IsSpellActive(SpellType.TAUNT_DIAMOND_STEALTH))
		{
			ActivateSpellDeathState(SpellType.TAUNT_DIAMOND_STEALTH);
		}
	}

	public void DeactivateTaunt(ISpellCallbackHandler<Spell>.FinishedCallback finishedCallback)
	{
		SpellType spellType = SpellType.NONE;
		Spell tauntSpell = null;
		if (IsSpellActive(SpellType.TAUNT))
		{
			spellType = SpellType.TAUNT;
			tauntSpell = GetSpell(spellType);
		}
		else if (IsSpellActive(SpellType.TAUNT_PREMIUM))
		{
			spellType = SpellType.TAUNT_PREMIUM;
			tauntSpell = GetSpell(spellType);
		}
		else if (IsSpellActive(SpellType.TAUNT_PREMIUM_STEALTH))
		{
			spellType = SpellType.TAUNT_PREMIUM_STEALTH;
			tauntSpell = GetSpell(spellType);
		}
		else if (IsSpellActive(SpellType.TAUNT_STEALTH))
		{
			spellType = SpellType.TAUNT_STEALTH;
			tauntSpell = GetSpell(spellType);
		}
		else if (IsSpellActive(SpellType.TAUNT_DIAMOND))
		{
			spellType = SpellType.TAUNT_DIAMOND;
			tauntSpell = GetSpell(spellType);
		}
		else if (IsSpellActive(SpellType.TAUNT_DIAMOND_STEALTH))
		{
			spellType = SpellType.TAUNT_DIAMOND_STEALTH;
			tauntSpell = GetSpell(spellType);
		}
		if (tauntSpell != null && finishedCallback != null)
		{
			ActivateSpellDeathState(spellType);
			tauntSpell.AddFinishedCallback(finishedCallback);
		}
	}

	public void DeactivateDivineShield(ISpellCallbackHandler<Spell>.FinishedCallback finishedCallback)
	{
		Spell spell = GetSpell(SpellType.DIVINE_SHIELD);
		if (spell != null)
		{
			ActivateSpellDeathState(SpellType.DIVINE_SHIELD);
			spell.AddFinishedCallback(finishedCallback);
		}
	}

	public Spell ActivateEvilTwinMustache()
	{
		Spell spell = GetSpell(m_premiumType switch
		{
			TAG_PREMIUM.GOLDEN => SpellType.EVIL_TWIN_MUSTACHE_PREMIUM, 
			TAG_PREMIUM.SIGNATURE => SpellType.EVIL_TWIN_MUSTACHE_SIGNATURE, 
			TAG_PREMIUM.DIAMOND => SpellType.EVIL_TWIN_MUSTACHE_DIAMOND, 
			_ => SpellType.EVIL_TWIN_MUSTACHE, 
		});
		if (spell == null)
		{
			return null;
		}
		if (spell.IsActive())
		{
			return spell;
		}
		spell.SetSource(m_card.gameObject);
		spell.ActivateState(SpellStateType.BIRTH);
		return spell;
	}

	public void DeactivateEvilTwinMustache()
	{
		ActivateSpellDeathState(SpellType.EVIL_TWIN_MUSTACHE);
		ActivateSpellDeathState(SpellType.EVIL_TWIN_MUSTACHE_PREMIUM);
		ActivateSpellDeathState(SpellType.EVIL_TWIN_MUSTACHE_SIGNATURE);
		ActivateSpellDeathState(SpellType.EVIL_TWIN_MUSTACHE_DIAMOND);
	}

	public Spell ActivateMinionTypeMask()
	{
		TAG_PREMIUM premiumType = m_premiumType;
		SpellType spellType = ((premiumType != 0 && premiumType == TAG_PREMIUM.GOLDEN) ? SpellType.MINION_TYPE_MASK_PREMIUM : SpellType.MINION_TYPE_MASK);
		Spell spell = GetSpell(spellType);
		if (spell == null)
		{
			return null;
		}
		if (spell.IsActive())
		{
			return spell;
		}
		spell.SetSource(m_card.gameObject);
		spell.ActivateState(SpellStateType.BIRTH);
		return spell;
	}

	public void DeactivateMinionTypeMask()
	{
		ActivateSpellDeathState(SpellType.MINION_TYPE_MASK);
		ActivateSpellDeathState(SpellType.MINION_TYPE_MASK_PREMIUM);
	}

	public void ActivateLaunchpadAnimations()
	{
		Spell spell = GetSpell(m_premiumType switch
		{
			TAG_PREMIUM.GOLDEN => SpellType.STARSHIP_LAUNCHPAD_GOLDEN, 
			TAG_PREMIUM.SIGNATURE => SpellType.STARSHIP_LAUNCHPAD_SIGNATURE, 
			_ => SpellType.STARSHIP_LAUNCHPAD_NORMAL, 
		});
		if (!(spell == null) && !spell.IsActive())
		{
			spell.SetSource(m_card.gameObject);
			spell.ActivateState(SpellStateType.BIRTH);
		}
	}

	public void DeactivateLaunchpadAnimations()
	{
		ActivateSpellDeathState(SpellType.STARSHIP_LAUNCHPAD_NORMAL);
		ActivateSpellDeathState(SpellType.STARSHIP_LAUNCHPAD_GOLDEN);
		ActivateSpellDeathState(SpellType.STARSHIP_LAUNCHPAD_SIGNATURE);
		if (m_entity == null || m_card == null)
		{
			return;
		}
		TagVisualConfiguration tagConfiguration = TagVisualConfiguration.Get();
		if (tagConfiguration == null)
		{
			return;
		}
		TagDelta fakeDelta = new TagDelta();
		fakeDelta.oldValue = 0;
		foreach (KeyValuePair<int, int> tagValue in m_entity.GetTags().GetMap())
		{
			if (tagValue.Value != 0 && tagValue.Key != 3563)
			{
				fakeDelta.tag = tagValue.Key;
				fakeDelta.newValue = tagValue.Value;
				tagConfiguration.ProcessTagChange((GAME_TAG)tagValue.Key, m_card, fromShowEntity: false, fakeDelta);
			}
		}
	}

	public void ActivateAlternateCost()
	{
		ActivateSpellDeathState(SpellType.SPELLS_COST_HEALTH);
		ActivateSpellDeathState(SpellType.COST_ARMOR);
		ActivateSpellDeathState(SpellType.COST_CORPSES);
		switch (GetEntity().GetTag<TAG_CARD_ALTERNATE_COST>(GAME_TAG.CARD_ALTERNATE_COST))
		{
		case TAG_CARD_ALTERNATE_COST.HEALTH:
			ActivateSpellBirthState(SpellType.SPELLS_COST_HEALTH);
			break;
		case TAG_CARD_ALTERNATE_COST.ARMOR:
			ActivateSpellBirthState(SpellType.COST_ARMOR);
			break;
		case TAG_CARD_ALTERNATE_COST.CORPSES:
			ActivateSpellBirthState(SpellType.COST_CORPSES);
			break;
		}
	}

	public void DeactivateAlternateCost()
	{
		ActivateSpellDeathState(SpellType.SPELLS_COST_HEALTH);
		ActivateSpellDeathState(SpellType.COST_ARMOR);
		ActivateSpellDeathState(SpellType.COST_CORPSES);
	}

	public Spell GetSpell(SpellType spellType)
	{
		Spell spell = null;
		if (m_useSharedSpellTable)
		{
			spell = GetLoadedSpell(spellType);
		}
		else if (m_localSpellTable != null)
		{
			spell = m_localSpellTable.GetLocalSpell(spellType);
		}
		return spell;
	}

	public Spell GetSpellIfLoaded(SpellType spellType)
	{
		Spell spell = null;
		if (m_useSharedSpellTable)
		{
			GetSpellIfLoaded(spellType, out spell);
		}
		else if (m_localSpellTable != null)
		{
			spell = m_localSpellTable.GetLocalSpell(spellType);
		}
		return spell;
	}

	public bool GetSpellIfLoaded(SpellType spellType, out Spell result)
	{
		if (m_ownedSpells == null || !m_ownedSpells.ContainsKey(spellType))
		{
			result = null;
			return false;
		}
		result = m_ownedSpells[spellType];
		return result != null;
	}

	public Spell ActivateSpellBirthState(SpellType spellType)
	{
		Spell spell = GetSpell(spellType);
		if (spell == null)
		{
			return null;
		}
		spell.ActivateState(SpellStateType.BIRTH);
		return spell;
	}

	public bool IsSpellActive(SpellType spellType)
	{
		Spell spell = GetSpellIfLoaded(spellType);
		if (spell == null)
		{
			return false;
		}
		return spell.IsActive();
	}

	public void ActivateSpellDeathState(SpellType spellType)
	{
		Spell spell = GetSpellIfLoaded(spellType);
		if (!(spell == null))
		{
			spell.ActivateState(SpellStateType.DEATH);
		}
	}

	public void ActivateSpellCancelState(SpellType spellType)
	{
		Spell spell = GetSpellIfLoaded(spellType);
		if (!(spell == null))
		{
			spell.ActivateState(SpellStateType.CANCEL);
		}
	}

	public void ActivateAllSpellsDeathStates()
	{
		if (m_useSharedSpellTable)
		{
			foreach (Spell spell in m_ownedSpells.Values)
			{
				if (spell != null && spell.IsActive())
				{
					spell.ActivateState(SpellStateType.DEATH);
				}
			}
			return;
		}
		if (!(m_localSpellTable != null))
		{
			return;
		}
		foreach (SpellTableEntry item in m_localSpellTable.m_Table)
		{
			Spell spell2 = item.m_Spell;
			if (!(spell2 == null) && spell2.IsActive())
			{
				spell2.ActivateState(SpellStateType.DEATH);
			}
		}
	}

	public void DoCardDeathVisuals()
	{
		foreach (SpellType type in Enum.GetValues(typeof(SpellType)))
		{
			if (IsSpellActive(type) && type != SpellType.GHOSTLY_DEATH && type != SpellType.DEATH && type != SpellType.DEATHRATTLE_DEATH && type != SpellType.REBORN_DEATH && type != SpellType.DAMAGE)
			{
				ActivateSpellDeathState(type);
			}
		}
	}

	public void DeactivateAllSpells()
	{
		if (m_useSharedSpellTable)
		{
			foreach (SpellType type in new List<SpellType>(m_ownedSpells.Keys))
			{
				Spell spell = m_ownedSpells[type];
				if (spell != null)
				{
					spell.Deactivate();
				}
			}
			return;
		}
		if (!(m_localSpellTable != null))
		{
			return;
		}
		foreach (SpellTableEntry item in m_localSpellTable.m_Table)
		{
			Spell spell2 = item.m_Spell;
			if (!(spell2 == null))
			{
				spell2.Deactivate();
			}
		}
	}

	public void ReleaseSpell(SpellType spellType)
	{
		if (m_useSharedSpellTable)
		{
			if (m_ownedSpells.TryGetValue(spellType, out var spell))
			{
				SpellManager.Get().ReleaseSpell(spell);
				m_ownedSpells.Remove(spellType);
			}
		}
		else
		{
			Debug.LogError($"Actor.DestroySpell() - FAILED to destroy {spellType} because the Actor is not using a shared spell table.");
		}
	}

	public void DisableArmorSpellForTransition()
	{
		m_armorSpellDisabledForTransition = true;
	}

	public void EnableArmorSpellAfterTransition()
	{
		m_armorSpellDisabledForTransition = false;
	}

	public void HideArmorSpell()
	{
		if (m_armorSpell != null)
		{
			m_armorSpell.SetArmor(0);
			m_armorSpell.Deactivate();
			m_armorSpell.gameObject.SetActive(value: false);
		}
	}

	public void ShowArmorSpell()
	{
		if (m_armorSpell != null && !m_armorSpellDisabledForTransition)
		{
			m_armorSpell.gameObject.SetActive(value: true);
			UpdateArmorSpell(m_updateTokenSource.Token);
		}
	}

	public void HideTavernTierSpell()
	{
		ReleaseSpell(SpellType.TECH_LEVEL_MANA_GEM);
	}

	public void HideCoinManaGem()
	{
		ReleaseSpell(SpellType.COIN_MANA_GEM);
	}

	public void ShowTavernTierSpell()
	{
		Spell techLevelSpell = GetSpell(SpellType.TECH_LEVEL_MANA_GEM);
		int techLevel = GetEntityDef().GetTechLevel();
		if (techLevelSpell != null)
		{
			techLevelSpell.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmInt("TechLevel").Value = techLevel;
			techLevelSpell.ActivateState(SpellStateType.BIRTH);
		}
	}

	public void ShowCoinManaGem()
	{
		Spell coinManaGemSpell = GetSpell(SpellType.COIN_MANA_GEM);
		if (coinManaGemSpell != null)
		{
			coinManaGemSpell.ActivateState(SpellStateType.BIRTH);
		}
	}

	private void UpdateRootObjectSpellComponents()
	{
		if (m_entity != null)
		{
			if (m_armorSpellLoading)
			{
				UpdateArmorSpellWhenLoaded(m_updateTokenSource.Token).Forget();
			}
			if (m_armorSpell != null)
			{
				UpdateArmorSpell(m_updateTokenSource.Token);
			}
		}
	}

	private async UniTaskVoid UpdateArmorSpellWhenLoaded(CancellationToken token)
	{
		while (m_armorSpellLoading)
		{
			await UniTask.Yield(PlayerLoopTiming.Update, token);
		}
		UpdateArmorSpell(token);
	}

	private void UpdateArmorSpell(CancellationToken token)
	{
		if (!m_armorSpell.gameObject.activeInHierarchy || m_entity == null)
		{
			return;
		}
		int armor = m_entity.GetArmor();
		int prevArmor = m_armorSpell.GetArmor();
		m_armorSpell.SetArmor(armor);
		if (armor > 0)
		{
			bool wasShown = m_armorSpell.IsShown();
			if (!wasShown)
			{
				m_armorSpell.Show();
			}
			if (prevArmor <= 0)
			{
				ActivateArmorSpell(SpellStateType.BIRTH, armorShouldBeOn: true, token).Forget();
			}
			else if (prevArmor > armor)
			{
				ActivateArmorSpell(SpellStateType.ACTION, armorShouldBeOn: true, token).Forget();
			}
			else if (prevArmor < armor)
			{
				ActivateArmorSpell(SpellStateType.CANCEL, armorShouldBeOn: true, token).Forget();
			}
			else if (!wasShown)
			{
				ActivateArmorSpell(SpellStateType.IDLE, armorShouldBeOn: true, token).Forget();
			}
		}
		else if (prevArmor > 0)
		{
			ActivateArmorSpell(SpellStateType.DEATH, armorShouldBeOn: false, token).Forget();
		}
	}

	public bool IsSkipArmorAnimationActive()
	{
		bool skipTag = false;
		if (m_entity != null)
		{
			skipTag = m_entity.HasTag(GAME_TAG.SKIP_ARMOR_ANIMATION);
		}
		return skipTag | m_skipArmorAnimation;
	}

	public void SetSkipArmorAnimationActive(bool active)
	{
		m_skipArmorAnimation = active;
	}

	private async UniTaskVoid ActivateArmorSpell(SpellStateType stateType, bool armorShouldBeOn, CancellationToken token)
	{
		while (m_armorSpell.GetActiveState() != 0)
		{
			await UniTask.Yield(PlayerLoopTiming.Update, token);
		}
		if (m_armorSpell.GetActiveState() != stateType)
		{
			int armor = m_entity.GetArmor();
			if ((!armorShouldBeOn || armor > 0) && (armorShouldBeOn || armor <= 0))
			{
				m_armorSpell.ActivateState(stateType);
			}
		}
	}

	public void SetArmorSpellState(SpellStateType stateType)
	{
		if (!(m_armorSpell == null))
		{
			m_armorSpell.ActivateState(stateType);
		}
	}

	private void OnSpellStateStarted(Spell spell, SpellStateType prevStateType, object userData)
	{
		spell.AddStateStartedCallback(OnSpellStateStarted);
		m_actorStateMgr.RefreshStateMgr();
		if ((bool)m_projectedShadow)
		{
			m_projectedShadow.UpdateContactShadow();
		}
	}

	private void AssignRootObject()
	{
		m_rootObject = GameObjectUtils.FindChildBySubstring(base.gameObject, "RootObject");
	}

	public GameObject GetRootObjet()
	{
		if (m_rootObject == null)
		{
			AssignRootObject();
		}
		return m_rootObject;
	}

	private void AssignBones()
	{
		m_bones = GameObjectUtils.FindChildBySubstring(base.gameObject, "Bones");
	}

	private void AssignMeshRenderers()
	{
		MeshRenderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			if (meshRenderer.gameObject.name.Equals("Mesh", StringComparison.OrdinalIgnoreCase))
			{
				m_meshRenderer = meshRenderer;
				MeshRenderer[] componentsInChildren2 = meshRenderer.gameObject.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
				foreach (MeshRenderer subMesh in componentsInChildren2)
				{
					AssignMaterials(subMesh);
				}
				break;
			}
		}
		if (m_portraitMesh != null)
		{
			m_meshRendererPortrait = m_portraitMesh.GetComponent<MeshRenderer>();
		}
	}

	private void CacheInitialMaterials()
	{
		if (m_portraitMesh != null && m_portraitMatIdx >= 0)
		{
			m_initialPortraitMaterial = m_portraitMesh.GetComponent<Renderer>().GetSharedMaterial(m_portraitMatIdx);
		}
		else if (m_legacyPortraitMaterialIndex >= 0)
		{
			m_initialPortraitMaterial = m_meshRenderer.GetSharedMaterial(m_legacyPortraitMaterialIndex);
		}
		if (m_cardMesh != null && m_cardBackMatIdx >= 0)
		{
			m_initialCardBackMaterial = m_cardMesh.GetComponent<Renderer>().GetSharedMaterial(m_cardBackMatIdx);
		}
		if (m_premiumRibbon > -1)
		{
			m_initialPremiumRibbonMaterial = m_cardMesh.GetComponent<Renderer>().GetMaterial(m_premiumRibbon);
		}
	}

	private void AssignMaterials(MeshRenderer meshRenderer)
	{
		List<Material> meshRendererMaterials = meshRenderer.GetSharedMaterials();
		for (int i = 0; i < meshRendererMaterials.Count; i++)
		{
			Material material = meshRendererMaterials[i];
			if (!(material == null))
			{
				if (material.name.LastIndexOf("Portrait", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					m_legacyPortraitMaterialIndex = i;
				}
				else if (material.name.IndexOf("Card_Inhand_Ability_Warlock", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					m_legacyCardColorMaterialIndex = i;
				}
				else if (material.name.IndexOf("Card_Inhand_Warlock", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					m_legacyCardColorMaterialIndex = i;
				}
				else if (material.name.IndexOf("Card_Inhand_Weapon_Warlock", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					m_legacyCardColorMaterialIndex = i;
				}
			}
		}
	}

	private void AssignSpells()
	{
		m_localSpellTable = base.gameObject.GetComponentInChildren<SpellTable>();
		m_actorStateMgr = base.gameObject.GetComponentInChildren<ActorStateMgr>();
		if (m_localSpellTable == null)
		{
			if (string.IsNullOrEmpty(m_spellTablePrefab))
			{
				return;
			}
			SpellManager spellManager = SpellManager.Get();
			if (spellManager != null)
			{
				SpellTable cachedSpellTable = spellManager.GetSpellTable(m_spellTablePrefab);
				if (cachedSpellTable != null)
				{
					m_useSharedSpellTable = true;
					m_sharedSpellTable = cachedSpellTable;
					m_ownedSpells = new Dictionary<SpellType, Spell>();
				}
				else
				{
					Debug.LogWarning("failed to load spell table: " + m_spellTablePrefab);
				}
			}
			else
			{
				Debug.LogWarning("Null spell cache: " + m_spellTablePrefab);
			}
		}
		else
		{
			if (!(m_actorStateMgr != null))
			{
				return;
			}
			foreach (SpellTableEntry entry in m_localSpellTable.m_Table)
			{
				if (!(entry.m_Spell == null))
				{
					entry.m_Spell.AddStateStartedCallback(OnSpellStateStarted);
				}
			}
		}
	}

	private Transform GetSpellParent()
	{
		if (m_spellsParent != null)
		{
			return m_spellsParent;
		}
		m_spellsParent = new GameObject("Spells").transform;
		m_spellsParent.parent = base.gameObject.transform;
		m_spellsParent.localPosition = Vector3.zero;
		m_spellsParent.localScale = Vector3.one;
		m_spellsParent.localRotation = Quaternion.identity;
		return m_spellsParent;
	}

	public void SetActorSpecificOverlayActive(bool enabled)
	{
		if (!(m_actorSpecificPortraitOverlay == null))
		{
			m_actorSpecificPortraitOverlay.SetActive(enabled);
		}
	}

	private void CacheShadowObjects()
	{
		List<GameObject> standard = GameObjectUtils.FindChildrenByTag(base.gameObject, "FakeShadow", includeInactive: true);
		List<GameObject> unique = GameObjectUtils.FindChildrenByTag(base.gameObject, "FakeShadowUnique", includeInactive: true);
		AddToContactShadowList(standard, isUnique: false);
		AddToContactShadowList(unique, isUnique: true);
		m_shadowObjectInitialized = true;
	}

	private void AddToContactShadowList(List<GameObject> shadowObjects, bool isUnique)
	{
		if (shadowObjects != null && shadowObjects.Count > 0 && m_contactShadows == null)
		{
			m_contactShadows = new List<ContactShadowData>();
		}
		if (shadowObjects == null)
		{
			return;
		}
		foreach (GameObject shadowObject in shadowObjects)
		{
			Renderer shadowRenderer = shadowObject.GetComponent<Renderer>();
			if (shadowRenderer != null)
			{
				m_contactShadows.Add(new ContactShadowData(isUnique, shadowObject, shadowRenderer.GetMaterial().renderQueue, shadowRenderer.transform.position - base.transform.position));
			}
		}
	}

	private void LoadArmorSpell()
	{
		if (!(m_armorSpellBone == null))
		{
			m_armorSpellLoading = true;
			string heroArmorPrefabPath = "Hero_Armor.prefab:e4d519d1080fe4656967bf5140ca3587";
			CardDef cardDef = m_cardDefHandle.Get(m_premiumType);
			if (cardDef != null && !string.IsNullOrEmpty(cardDef.m_CustomHeroArmorSpell))
			{
				heroArmorPrefabPath = cardDef.m_CustomHeroArmorSpell;
			}
			AssetLoader.Get().InstantiatePrefab(heroArmorPrefabPath, OnArmorSpellLoaded);
		}
	}

	private void OnArmorSpellLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Debug.LogError($"{assetRef} - Actor.OnArmorSpellLoaded() - failed to load Hero_Armor spell! m_armorSpell GameObject = null!");
			return;
		}
		m_armorSpellLoading = false;
		if (m_armorSpell != null)
		{
			bool wasSkipingAnimation = m_skipArmorAnimation;
			m_skipArmorAnimation = true;
			m_skipArmorAnimation = wasSkipingAnimation;
			HideArmorSpell();
			if (GameState.Get() != null)
			{
				GameState.Get().RemoveServerBlockingSpell(m_armorSpell);
			}
		}
		m_armorSpell = go.GetComponent<ArmorSpell>();
		if (m_armorSpell == null)
		{
			Debug.LogError($"{assetRef} - Actor.OnArmorSpellLoaded() - failed to load Hero_Armor spell! m_armorSpell Spell = null!");
			return;
		}
		go.transform.parent = m_armorSpellBone.transform;
		go.transform.localPosition = Vector3.zero;
		go.transform.localRotation = Quaternion.identity;
		go.transform.localScale = Vector3.one;
		ArmorSpellFinishedLoading();
	}

	protected virtual void ArmorSpellFinishedLoading()
	{
	}

	public void LoadCustomFrame(CardDef cardDef)
	{
		if (cardDef != null && !string.IsNullOrEmpty(cardDef.m_CustomHeroFramePrefab))
		{
			AssetReference assetPath = cardDef.m_CustomHeroFramePrefab;
			if (m_customFrameController == null || m_customFrameController.FrameAssetReference != assetPath)
			{
				UnloadCustomFrame();
				IAssetLoader assetLoader = AssetLoader.Get();
				if (assetLoader != null)
				{
					using (AssetHandle<GameObject> handle = assetLoader.GetOrInstantiateSharedPrefab(assetPath))
					{
						OnCustomFrameLoaded(assetPath, handle, null);
					}
				}
			}
			else if (m_customFrameController != null)
			{
				ApplyCustomFrame();
			}
		}
		else
		{
			UnloadCustomFrame();
		}
	}

	private void UnloadCustomFrame()
	{
		if (m_customFrameController != null)
		{
			m_customFrameController.RestoreInitialPortraitMaterial(ref m_initialPortraitMaterial);
			m_customFrameController.RestoreMeshAndMaterials(ref m_portraitMesh, ref m_portraitMatIdx, ref m_portraitFrameMatIdx);
			m_customFrameController.RestoreInitialStatsPositions(ref m_attackObject, ref m_healthObject, ref m_armorSpellBone);
		}
		UpdateMaterials();
		if (m_decorationRoot != null)
		{
			m_decorationRoot.transform.localPosition = Vector3.zero;
		}
		ZoneHeroPositionOffset = 0f;
		if (m_projectedShadow != null)
		{
			m_projectedShadow.m_AutoDisableHeight = m_cachedProjectedShadowAutoDisableHeight;
		}
		this.OnCustomFrameChanged?.Invoke(null);
	}

	private void DestroyCustomFrame()
	{
		UnloadCustomFrame();
		if (m_customFrameController != null)
		{
			((IDisposable)m_customFrameController).Dispose();
			m_customFrameController = null;
		}
	}

	public Material GetInitialPortraitMaterial()
	{
		return m_initialPortraitMaterial;
	}

	private IEnumerator DelayThenUpdateCustomFrameDiamondMaterials()
	{
		yield return new WaitForEndOfFrame();
		UpdateCustomFrameDiamondMaterial();
	}

	private void ApplyCustomFrame()
	{
		if (m_customFrameController != null)
		{
			m_customFrameController.ApplyCustomMeshAndMaterials(out m_portraitMesh);
			m_customFrameController.ApplyStatPositionOffsets(m_attackObject, m_healthObject, m_armorSpellBone);
			m_customFrameController.ApplyMetaFrameOffsets(m_portraitMesh.transform);
			StartCoroutine(DelayThenUpdateCustomFrameDiamondMaterials());
			m_portraitMatIdx = m_customFrameController.PortraitMatIdx;
			m_portraitFrameMatIdx = m_customFrameController.FrameMatIdx;
			if (m_portraitMatIdx >= 0)
			{
				m_initialPortraitMaterial = m_portraitMesh.GetComponent<Renderer>().GetSharedMaterial(m_portraitMatIdx);
			}
			else
			{
				m_initialPortraitMaterial = null;
			}
			if (m_decorationRoot != null)
			{
				m_decorationRoot.transform.localPosition = new Vector3(0f, m_customFrameController.DecorationRootOffset, 0f);
			}
			ZoneHeroPositionOffset = m_customFrameController.HeroZonePositionOffset;
			if (m_projectedShadow != null)
			{
				m_projectedShadow.m_AutoDisableHeight = m_cachedProjectedShadowAutoDisableHeight + ZoneHeroPositionOffset;
			}
			UpdateMaterials();
			SetPortraitMaterial(m_initialPortraitMaterial);
			UpdatePortraitTexture();
			if (isMissingCard())
			{
				MissingCardEffect();
			}
			if ((bool)m_card)
			{
				m_card.GetZone()?.UpdateLayout();
			}
			if (TryGetComponent<CollectibleSkin>(out var skin) && skin.m_nameShadow != null)
			{
				m_customFrameController.ApplyCustomFrameNameShadowTexture(skin.m_nameShadow);
			}
			this.OnCustomFrameChanged?.Invoke(m_customFrameController);
		}
	}

	private void OnCustomFrameLoaded(AssetReference assetRef, AssetHandle<GameObject> go, object callbackData)
	{
		using (go)
		{
			if (go == null || go.Asset == null)
			{
				Debug.LogError($"{assetRef} - Actor.OnCustomFrameLoaded() - failed to load Hero_Armor spell! m_armorSpell GameObject = null!");
				return;
			}
			if (go.Asset.GetComponent<CustomFrameDef>() == null)
			{
				Debug.LogError($"{assetRef} - Actor.OnCustomFrameLoaded() - failed to load Hero_Armor spell! m_armorSpell CustomFrameDef = null!");
				return;
			}
			if (m_customFrameController == null && m_portraitMesh != null)
			{
				m_customFrameController = new CustomFrameController(m_portraitMesh, m_portraitMatIdx, m_portraitFrameMatIdx);
			}
			if (m_customFrameController != null)
			{
				m_customFrameController.SetAssetHandle(assetRef, go);
				m_customFrameController.CacheHighlightState(GetComponentInChildren<HighlightState>());
				m_customFrameController.CacheInitialPortraitMaterial(m_initialPortraitMaterial);
				m_customFrameController.CacheInitialStatsPositions(m_attackObject, m_healthObject, m_armorSpellBone);
				ApplyCustomFrame();
			}
		}
	}

	private void ConnectLegendarySkinToDynamicResolutionController()
	{
		if (m_customFrameController != null)
		{
			LegendarySkinDynamicResController controller = m_customFrameController.DynamicResolutionController;
			if (LegendaryHeroPortrait != null)
			{
				LegendaryHeroPortrait.ConnectDynamicResolutionController(controller);
			}
			else
			{
				controller.Skin = null;
			}
		}
	}

	private void DisconnectLegendarySkinToDynamicResolutionController()
	{
		if (m_customFrameController != null)
		{
			LegendarySkinDynamicResController controller = m_customFrameController.DynamicResolutionController;
			if (controller != null)
			{
				controller.Skin = null;
			}
		}
	}

	public void AddCustomFrameCallback(CustomFrameChangedEventHandler eventHandler)
	{
		eventHandler?.Invoke(m_customFrameController);
		OnCustomFrameChanged += eventHandler;
	}

	public bool HasSameCardDef(CardDef cardDef)
	{
		return m_cardDefHandle.Get(m_premiumType) == cardDef;
	}

	public bool HasSignaturePortraitTexture()
	{
		if (m_cardDefHandle == null)
		{
			return false;
		}
		CardDef cardDef = m_cardDefHandle.Get(m_premiumType);
		if (cardDef == null)
		{
			return false;
		}
		return !string.IsNullOrEmpty(cardDef.m_SignaturePortraitTexturePath);
	}

	public CardRuneBanner GetRuneBanner()
	{
		return m_cardRuneBanner;
	}
}
