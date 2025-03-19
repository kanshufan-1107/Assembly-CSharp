using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.MaterialService.Extensions;
using Blizzard.T5.Services;
using PegasusShared;
using UnityEngine;

[CustomEditClass]
public class CollectionDeckBoxVisual : PegUIElement, IDraggableCollectionVisual
{
	[Serializable]
	public class FormatElements
	{
		[SerializeField]
		public FormatType formatType;

		[SerializeField]
		public Texture2D highlight;

		[SerializeField]
		public GameObject portraitObject;

		[SerializeField]
		public int portraitMaterialIndex;

		[SerializeField]
		public Material defaultFrameMaterial;

		[SerializeField]
		public int frameMaterialIndex;

		[SerializeField]
		public GameObject classObject;

		[SerializeField]
		public int classIconMaterialIndex;

		[SerializeField]
		public Material defaultClassIconMaterial;

		[SerializeField]
		public int classBannerMaterialIndex;

		[SerializeField]
		public MeshRenderer topBannerRenderer;

		[SerializeField]
		public Material xButtonMaterial;

		[CustomEditField(T = EditType.SOUND_PREFAB)]
		public string deckSelectSound;

		[SerializeField]
		public GameObject disabledMeshObject;

		[SerializeField]
		public Material unlockableFrameMaterial;
	}

	public delegate void DelOnAnimationFinished(object callbackData);

	private class OnPopAnimationFinishedCallbackData
	{
		public string m_animationName;

		public DelOnAnimationFinished m_callback;

		public object m_callbackData;
	}

	private class OnScaleFinishedCallbackData
	{
		public DelOnAnimationFinished m_callback;

		public object m_callbackData;
	}

	public UberText m_deckName;

	public UberText m_deckDesc;

	public GameObject m_labelGradient;

	public PegUIElement m_deleteButton;

	public GameObject m_notificationButton;

	public GameObject m_highlight;

	public List<FormatElements> m_formatElements;

	public GameObject m_invalidCardCountIndicator;

	public UberText m_invalidCardCountIndicatorText;

	public GameObject m_invalidDeckIndicator;

	public GameObject m_unlockableDeckIndicator;

	public Material m_unlockablePortraitMaterial;

	public Material m_unlockableClassIconMaterial;

	public int m_topBannerMaterialIndex;

	public GameObject m_pressedBone;

	public CustomDeckBones m_bones;

	public GameObject m_normalDeckVisuals;

	public GameObject m_lockedDeckVisuals;

	public TooltipZone m_tooltipZone;

	public GameObject m_renameVisuals;

	public bool m_neverUseGoldenPortraits;

	public Material m_defaultPortraitMaterial;

	public PlayMakerFSM m_DeckPortraitChangeFSM;

	public GameObject m_deckRunes;

	public RuneSlotVisual m_runeSlotVisual;

	public static readonly float POPPED_UP_LOCAL_Z = 0f;

	public static readonly Vector3 POPPED_DOWN_LOCAL_POS = new Vector3(0f, -0.8598533f, 0f);

	public const float DECKBOX_SCALE = 0.95f;

	public static readonly Vector3 SCALED_DOWN_LOCAL_SCALE = new Vector3(0.95f, 0.95f, 0.95f);

	public const float SCALED_UP_LOCAL_Y_OFFSET = 3.238702f;

	public const float SCALED_DOWN_LOCAL_Y_OFFSET = 1.273138f;

	private const float BUTTON_POP_SPEED = 6f;

	private const string DECKBOX_POPUP_ANIM_NAME = "Deck_PopUp";

	private const string DECKBOX_POPDOWN_ANIM_NAME = "Deck_PopDown";

	private const string DECKBOX_DESATURATION_ANIM_NAME = "CustomDeck_Desat";

	private Vector3 SCALED_UP_DECK_OFFSET = new Vector3(0f, 0f, 0f);

	private const float SCALE_TIME = 0.2f;

	private const float ADJUST_Y_OFFSET_ANIM_TIME = 0.05f;

	private static readonly Color DECK_DESC_ENABLED_COLOR = new Color(0.97f, 0.82f, 0.22f);

	private static readonly Color DECK_NAME_ENABLED_COLOR = Color.white;

	private static float DEATH_KNIGHT_EDITED_DECK_BOX_COLLIDER_HEIGHT = 2.75f;

	private long m_deckID = -1L;

	private int m_deckTemplateId;

	private bool m_isPoppedUp;

	private bool m_isShown;

	private DefLoader.DisposableFullDef m_fullDef;

	private bool m_isShared;

	private HighlightState m_highlightState;

	private string m_heroCardID = "";

	private TAG_PREMIUM? m_heroCardPremiumOverride;

	private FormatType m_formatType = FormatType.FT_STANDARD;

	private Vector3 m_originalButtonPosition;

	private Quaternion m_originalButtonRotation;

	private bool m_animateButtonPress = true;

	private bool m_wasTouchModeEnabled;

	private int m_positionIndex;

	private bool m_showGlow;

	private bool m_isLocked;

	private bool m_isUnlockable;

	private bool m_isUnlockaleDirty;

	private bool m_forceSingleLineDeckName;

	private bool m_isSelected;

	private float m_wiggleIntensity;

	private bool m_showBanner = true;

	private bool m_isShowingInvalidCardCount;

	private CollectionDeck.CardCountByStatus m_cardCountByStatus;

	private int m_invalidSideboardCardCount;

	private int m_missingSideboardCardCount;

	private ILegendaryHeroPortrait m_legendaryHeroPortrait;

	private Transform m_customDeckTransform;

	private IGraphicsManager m_graphicsManager;

	private BoxCollider m_boxCollider;

	private Vector3 m_originalBoxColliderSize;

	public static Vector3 SCALED_UP_LOCAL_SCALE { get; private set; }

	private GameObject ButtonGameObject => GetActiveFormatElements()?.portraitObject;

	private static DeckTrayDeckListContent DecksContent
	{
		get
		{
			CollectionDeckTray tray = CollectionDeckTray.Get();
			if (!(tray != null))
			{
				return null;
			}
			return tray.GetDecksContent();
		}
	}

	private static DeckTrayTeamListContent TeamsContent
	{
		get
		{
			CollectionDeckTray tray = CollectionDeckTray.Get();
			if (!(tray != null))
			{
				return null;
			}
			return tray.GetTeamsContent();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_graphicsManager = ServiceManager.Get<IGraphicsManager>();
		SetEnabled(enabled: false);
		m_deleteButton.AddEventListener(UIEventType.RELEASE, OnDeleteButtonPressed);
		m_deleteButton.AddEventListener(UIEventType.ROLLOVER, OnDeleteButtonOver);
		m_deleteButton.AddEventListener(UIEventType.ROLLOUT, OnDeleteButtonRollout);
		ShowDeleteButton(show: false);
		ShowNotificationButton(show: false);
		UpdateInvalidCardCountIndicator();
		m_deckName.RichText = false;
		m_deckName.TextColor = DECK_NAME_ENABLED_COLOR;
		m_deckDesc.TextColor = DECK_DESC_ENABLED_COLOR;
		SoundManager.Get().Load("tiny_button_press_1.prefab:44fc68b7418870b4797b85f0ca88a8db");
		SoundManager.Get().Load("tiny_button_mouseover_1.prefab:0ab88a13f5168ed43a3b53275114a842");
		m_customDeckTransform = base.transform.Find("CustomDeck");
		SetHighlightRoot();
		SCALED_UP_LOCAL_SCALE = new Vector3(1.126f, 1.126f, 1.126f);
		if (PlatformSettings.s_screen == ScreenCategory.Phone)
		{
			SCALED_UP_LOCAL_SCALE = new Vector3(1.1f, 1.1f, 1.1f);
			SCALED_UP_DECK_OFFSET = new Vector3(0f, -0.2f, 0f);
		}
		if ((bool)TeamsContent)
		{
			SetFormatType(FormatType.FT_STANDARD);
		}
		m_wasTouchModeEnabled = UniversalInputManager.Get().IsTouchMode();
		m_boxCollider = GetComponent<BoxCollider>();
		m_originalBoxColliderSize = m_boxCollider.size;
	}

	protected override void OnDestroy()
	{
		m_fullDef?.Dispose();
		m_fullDef = null;
		m_legendaryHeroPortrait?.Dispose();
		m_legendaryHeroPortrait = null;
		base.OnDestroy();
	}

	private void Update()
	{
		if (m_wasTouchModeEnabled != UniversalInputManager.Get().IsTouchMode())
		{
			InteractionState state = GetInteractionState();
			if (m_wasTouchModeEnabled)
			{
				switch (state)
				{
				case InteractionState.Down:
					OnPressEvent();
					break;
				case InteractionState.Over:
					OnOverEvent();
					break;
				}
			}
			else
			{
				switch (state)
				{
				case InteractionState.Down:
					OnReleaseEvent();
					break;
				case InteractionState.Over:
					OnOutEvent();
					break;
				}
				ShowDeleteButton(show: false);
			}
			m_wasTouchModeEnabled = UniversalInputManager.Get().IsTouchMode();
		}
		if ((bool)ButtonGameObject)
		{
			float wiggleStartTime = 0f;
			float wiggleStopTime = 0f;
			float wiggleFrequency = 0f;
			float wiggleMaxAmplitude = 0f;
			Vector3 wiggleAxis = Vector3.zero;
			bool shouldWiggle = false;
			if ((bool)DecksContent || (bool)TeamsContent)
			{
				DeckTrayReorderableContent reorderableContent = ((TeamsContent != null) ? ((DeckTrayReorderableContent)TeamsContent) : ((DeckTrayReorderableContent)DecksContent));
				wiggleStartTime = reorderableContent.m_rearrangeStartStopTweenDuration;
				wiggleStopTime = reorderableContent.m_rearrangeStartStopTweenDuration;
				wiggleFrequency = reorderableContent.m_rearrangeWiggleFrequency;
				wiggleMaxAmplitude = reorderableContent.m_rearrangeWiggleAmplitude;
				wiggleAxis = reorderableContent.m_rearrangeWiggleAxis;
				shouldWiggle = reorderableContent.DraggingDeckBox != null && reorderableContent.DraggingDeckBox != this;
			}
			bool num = m_wiggleIntensity > 0f;
			if (shouldWiggle)
			{
				m_wiggleIntensity = Mathf.Clamp01(m_wiggleIntensity + Time.deltaTime / wiggleStartTime);
			}
			else
			{
				m_wiggleIntensity = Mathf.Clamp01(m_wiggleIntensity - Time.deltaTime / wiggleStopTime);
			}
			bool isWiggling = m_wiggleIntensity > 0f;
			if (num || isWiggling)
			{
				float wiggleOffset = wiggleMaxAmplitude * m_wiggleIntensity * Mathf.Cos((float)m_positionIndex + Time.time * wiggleFrequency);
				ButtonGameObject.transform.localRotation = Quaternion.AngleAxis(wiggleOffset, wiggleAxis) * m_originalButtonRotation;
			}
		}
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
		m_isShown = true;
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
		m_isShown = false;
	}

	public bool IsShown()
	{
		return m_isShown;
	}

	public void SetDeckName(string deckName)
	{
		m_deckName.Text = deckName;
	}

	public UberText GetDeckNameText()
	{
		return m_deckName;
	}

	public void HideDeckName()
	{
		m_deckName.gameObject.SetActive(value: false);
	}

	public void ShowDeckName()
	{
		m_deckName.gameObject.SetActive(value: true);
	}

	public void HideRenameVisuals()
	{
		if (m_renameVisuals != null)
		{
			m_renameVisuals.SetActive(value: false);
		}
	}

	public void ShowRenameVisuals()
	{
		if (!CollectionManagerDisplay.IsSpecialOneDeckMode() && m_renameVisuals != null)
		{
			m_renameVisuals.SetActive(value: true);
		}
	}

	public void SetDeckID(long id)
	{
		m_deckID = id;
	}

	public void SetDeckTemplateId(int id)
	{
		m_deckTemplateId = id;
	}

	public int GetDeckTemplateId()
	{
		return m_deckTemplateId;
	}

	public long GetDeckID()
	{
		return m_deckID;
	}

	public CollectionDeck GetCollectionDeck()
	{
		if (IsShared())
		{
			List<CollectionDeck> decks = FriendChallengeMgr.Get().GetSharedDecks();
			if (decks != null)
			{
				return decks.Find((CollectionDeck deck) => deck.ID == m_deckID);
			}
		}
		FreeDeckMgr manager = FreeDeckMgr.Get();
		if (manager != null && manager.Status == FreeDeckMgr.FreeDeckStatus.TRIAL_PERIOD)
		{
			CollectionDeck loanerDeck = manager.GetLoanerDeckFromDeckTemplateId(m_deckTemplateId);
			if (loanerDeck != null)
			{
				return loanerDeck;
			}
		}
		if (m_formatType == FormatType.FT_TWIST)
		{
			RankedPlaySeason season = RankMgr.Get()?.GetCurrentTwistSeason();
			if (season != null && season.UsesPrebuiltDecks)
			{
				return season.GetDeck(m_deckTemplateId);
			}
		}
		return CollectionManager.Get().GetDeck(m_deckID);
	}

	public DefLoader.DisposableFullDef SharedDisposableFullDef()
	{
		return m_fullDef?.Share();
	}

	public bool HasFullDef()
	{
		return m_fullDef != null;
	}

	public string GetHeroCardID()
	{
		return m_heroCardID;
	}

	public bool IsLoading()
	{
		if (GetDeckID() > 0 && m_heroCardID != "None")
		{
			return !HasFullDef();
		}
		return false;
	}

	public bool SetHeroCardID(string heroCardID, TAG_PREMIUM? premiumOverride = null)
	{
		if (string.IsNullOrEmpty(heroCardID) || heroCardID.Equals("None"))
		{
			m_heroCardID = "None";
			return false;
		}
		if (m_heroCardID != heroCardID || premiumOverride != m_heroCardPremiumOverride)
		{
			m_heroCardID = heroCardID;
			m_heroCardPremiumOverride = premiumOverride;
			TAG_PREMIUM premium = GetHeroCardPremium();
			if (m_heroCardPremiumOverride.HasValue)
			{
				premium = m_heroCardPremiumOverride.Value;
			}
			CardPortraitQuality portraitQuality = new CardPortraitQuality(3, premium);
			DefLoader.Get().LoadFullDef(heroCardID, OnHeroFullDefLoaded, null, portraitQuality);
			return true;
		}
		return false;
	}

	public bool SetHeroCardIdFromDeck()
	{
		CollectionDeck deck = GetCollectionDeck();
		if (deck == null)
		{
			return false;
		}
		return SetHeroCardID(deck.GetDisplayHeroCardID(rerollFavoriteHero: true), null);
	}

	public void SetHeroCardPremiumOverride(TAG_PREMIUM? premium)
	{
		m_heroCardPremiumOverride = premium;
	}

	public TAG_PREMIUM GetHeroCardPremium()
	{
		if (m_heroCardPremiumOverride.HasValue)
		{
			return m_heroCardPremiumOverride.Value;
		}
		TAG_CLASS tagClass = GameUtils.GetTagClassFromCardId(m_heroCardID);
		return CollectionManager.Get().GetHeroPremium(tagClass);
	}

	public void SetShowGlow(bool showGlow)
	{
		m_showGlow = showGlow;
		if (m_showGlow)
		{
			SetHighlightState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
		}
	}

	public FormatType GetFormatType()
	{
		return m_formatType;
	}

	public void PlayGlowAnim()
	{
		if (TryGetComponent<Animator>(out var animator))
		{
			animator.enabled = true;
			animator.Play("CustomDeck_GlowOut", 0, 0f);
		}
	}

	public void OnGlowAnimPeak()
	{
		CollectionDeck deck = GetCollectionDeck();
		if (deck == null || deck.FormatType != FormatType.FT_WILD)
		{
			return;
		}
		m_formatType = deck.FormatType;
		ReparentElements(m_formatType);
		FormatElements activeFormatElements = GetActiveFormatElements();
		FormatElements[] inactiveFormatElements = GetInactiveFormatElements();
		m_highlightState.m_StaticSilouetteTexture = activeFormatElements.highlight;
		FormatElements[] array = inactiveFormatElements;
		foreach (FormatElements inactiveElements in array)
		{
			if (inactiveElements.portraitObject != null)
			{
				inactiveElements.portraitObject.SetActive(value: false);
			}
		}
		if (activeFormatElements.portraitObject != null)
		{
			activeFormatElements.portraitObject.SetActive(value: true);
			Animator activeFormatElementsPortraitObjectAnimator = activeFormatElements.portraitObject.GetComponent<Animator>();
			if (!UniversalInputManager.UsePhoneUI)
			{
				activeFormatElementsPortraitObjectAnimator.Play("Wild_RolldownActivate", 0, 1f);
			}
			else
			{
				activeFormatElementsPortraitObjectAnimator.Play("WildActivate", 0, 1f);
			}
		}
	}

	public void SetFormatType(FormatType formatType)
	{
		m_formatType = formatType;
		ReparentElements(formatType);
		UpdateVisualBannerState();
		FormatElements activeFormatElements = GetActiveFormatElements();
		m_deleteButton.GetComponent<Renderer>().SetMaterial(activeFormatElements.xButtonMaterial);
		m_highlightState.m_StaticSilouetteTexture = activeFormatElements.highlight;
		foreach (FormatElements elements in m_formatElements)
		{
			if (elements.portraitObject != null)
			{
				elements.portraitObject.SetActive(elements.formatType == formatType);
			}
		}
	}

	public void SetPositionIndex(int idx)
	{
		m_positionIndex = idx;
	}

	public int GetPositionIndex()
	{
		return m_positionIndex;
	}

	public void UpdateDeckLabel()
	{
		bool showSingleLineOnly = false;
		if (SceneMgr.Get().IsInLettuceMode() || IsShared() || m_heroCardPremiumOverride.HasValue || m_forceSingleLineDeckName || !IsDeckEnabled())
		{
			showSingleLineOnly = true;
		}
		else if (m_isShowingInvalidCardCount)
		{
			if (m_cardCountByStatus.Extra > 0)
			{
				m_deckDesc.Text = GameStrings.FormatPlurals("GLUE_COLLECTION_DECK_EXTRA_CARDS_LABEL", GameStrings.MakePlurals(m_cardCountByStatus.Extra));
			}
			else
			{
				m_deckDesc.Text = GameStrings.FormatPlurals("GLUE_COLLECTION_DECK_MISSING_CARDS_LABEL", GameStrings.MakePlurals(m_cardCountByStatus.MissingPlusInvalid));
			}
		}
		else if (m_invalidSideboardCardCount > 0)
		{
			m_deckDesc.Text = GameStrings.Get("GLUE_COLLECTION_INVALID_SIDEBOARD_CARDS_HEADER");
		}
		else if (m_missingSideboardCardCount > 0)
		{
			m_deckDesc.Text = GameStrings.Get("GLUE_COLLECTION_INVALID_SIDEBOARD_CARDS_HEADER");
		}
		else if (m_fullDef?.EntityDef != null)
		{
			showSingleLineOnly = true;
		}
		if (showSingleLineOnly)
		{
			SetDeckNameAsSingleLine(forceSingleLine: false);
			return;
		}
		m_deckName.transform.position = m_bones.m_deckLabelTwoLine.position;
		m_labelGradient.transform.parent = m_bones.m_gradientTwoLine;
		m_labelGradient.transform.localPosition = Vector3.zero;
		m_labelGradient.transform.localScale = Vector3.one;
		m_deckDesc.gameObject.SetActive(value: true);
	}

	public void SetDeckNameAsSingleLine(bool forceSingleLine)
	{
		if (forceSingleLine)
		{
			m_forceSingleLineDeckName = true;
		}
		if (!(m_deckName == null) && m_bones != null && !(m_labelGradient == null) && !(m_deckDesc?.gameObject == null))
		{
			m_deckName.transform.position = m_bones.m_deckLabelOneLine.position;
			m_labelGradient.transform.parent = m_bones.m_gradientOneLine;
			m_labelGradient.transform.localPosition = Vector3.zero;
			m_labelGradient.transform.localScale = Vector3.one;
			m_deckDesc.gameObject.SetActive(value: false);
		}
	}

	public bool IsDeckEnabled()
	{
		if (DeckPickerTrayDisplay.Get() == null)
		{
			return true;
		}
		CollectionDeck deck = GetCollectionDeck();
		if (deck == null)
		{
			return false;
		}
		FormatType formatType = Options.GetFormatType();
		if (!deck.IsValidForModeAndFormat(SceneMgr.Get().GetMode(), Options.GetInRankedPlayMode(), formatType))
		{
			return false;
		}
		if (deck.IsLoanerDeck && deck.FindInvalidSlot(formatType, ignoreOwnership: true) != null)
		{
			return false;
		}
		return true;
	}

	public bool CanSelectDeck()
	{
		CollectionDeck deck = GetCollectionDeck();
		if (deck == null)
		{
			return false;
		}
		if (!IsDeckEnabled())
		{
			return false;
		}
		if (!deck.IsValidForRuleset)
		{
			return false;
		}
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.TOURNAMENT && Options.GetInRankedPlayMode() && deck.FormatType == FormatType.FT_STANDARD && Options.GetFormatType() == FormatType.FT_WILD && deck.GetTotalInvalidCardCount(FormatType.FT_WILD) > 0)
		{
			return false;
		}
		return true;
	}

	public bool IsDeckUnlockable()
	{
		return m_isUnlockable;
	}

	public bool IsDeckPlayable()
	{
		CollectionDeck deck = GetCollectionDeck();
		if (deck == null)
		{
			return false;
		}
		string requiredCardId;
		bool num = deck.DeckTemplate_HasUnownedRequirements(out requiredCardId);
		bool areAllSideboardsValid = deck.GetMissingSideboardCardCount() == 0 && deck.GetInvalidSideboardCardCount(deck.FormatType) == 0;
		return !num && areAllSideboardsValid;
	}

	private bool CanShowInvalidCardCountIndicator()
	{
		CollectionDeck deck = GetCollectionDeck();
		if (deck == null)
		{
			return false;
		}
		if (!deck.NetworkContentsLoaded())
		{
			return false;
		}
		if (deck.IsBeingEdited())
		{
			return false;
		}
		if (!GameUtils.IsCardGameplayEventActive(deck.HeroCardID))
		{
			return false;
		}
		if (deck.GetRuleset(null).EntityInDeckIgnoresRuleset(deck))
		{
			return false;
		}
		if (!IsDeckEnabled())
		{
			return false;
		}
		return true;
	}

	public FormatType? GetFormatTypeToValidateAgainst()
	{
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.TOURNAMENT && Options.GetInRankedPlayMode())
		{
			return Options.GetFormatType();
		}
		return GetCollectionDeck()?.FormatType;
	}

	public void UpdateInvalidCardCountIndicator()
	{
		m_isShowingInvalidCardCount = false;
		m_cardCountByStatus = null;
		m_invalidSideboardCardCount = 0;
		m_missingSideboardCardCount = 0;
		if (CanShowInvalidCardCountIndicator())
		{
			CollectionDeck deck = GetCollectionDeck();
			FormatType? format = GetFormatTypeToValidateAgainst();
			m_cardCountByStatus = deck.CountCardsByStatus(format);
			m_invalidSideboardCardCount = deck.GetInvalidSideboardCardCount(format);
			m_missingSideboardCardCount = deck.GetMissingSideboardCardCount();
			int currentCountToShow = ((m_cardCountByStatus.Extra <= 0) ? m_cardCountByStatus.Valid : m_cardCountByStatus.Total);
			if (currentCountToShow < m_cardCountByStatus.Min)
			{
				m_isShowingInvalidCardCount = true;
				m_invalidCardCountIndicatorText.Text = GameStrings.Format("GLUE_COLLECTION_DECK_MISSING_CARDS_INDICATOR", currentCountToShow, m_cardCountByStatus.Min);
			}
			if (currentCountToShow > m_cardCountByStatus.Max)
			{
				m_isShowingInvalidCardCount = true;
				m_invalidCardCountIndicatorText.Text = GameStrings.Format("GLUE_COLLECTION_DECK_MISSING_CARDS_INDICATOR", currentCountToShow, m_cardCountByStatus.Max);
			}
		}
		UpdateInvalidIndicators(canShow: true);
		UpdateDeckLabel();
	}

	public void SetUnlockableStatus(bool isUnlockable)
	{
		if (m_isUnlockable != isUnlockable)
		{
			m_isUnlockable = isUnlockable;
			m_isUnlockaleDirty = true;
		}
	}

	public void UpdateUnlockableStatus()
	{
		if (!m_isUnlockaleDirty)
		{
			return;
		}
		FormatElements activeFormatElements = GetActiveFormatElements();
		if (!(activeFormatElements.unlockableFrameMaterial == null))
		{
			m_isUnlockaleDirty = false;
			m_unlockableDeckIndicator.SetActive(m_isUnlockable);
			SetMaterial(activeFormatElements.portraitObject, activeFormatElements.frameMaterialIndex, m_isUnlockable ? activeFormatElements.unlockableFrameMaterial : activeFormatElements.defaultFrameMaterial, ref activeFormatElements.defaultFrameMaterial, preserveOffset: true, preserveScale: true, preserveTexture: false);
			if (m_unlockableClassIconMaterial != null)
			{
				SetMaterial(activeFormatElements.classObject, activeFormatElements.classIconMaterialIndex, m_isUnlockable ? m_unlockableClassIconMaterial : activeFormatElements.defaultClassIconMaterial, ref activeFormatElements.defaultClassIconMaterial, preserveOffset: true, preserveScale: true, preserveTexture: false);
			}
			if (m_unlockablePortraitMaterial != null)
			{
				Material portrait = m_fullDef.CardDef.GetCustomDeckPortrait();
				SetMaterial(activeFormatElements.portraitObject, activeFormatElements.portraitMaterialIndex, m_unlockablePortraitMaterial, ref portrait, preserveOffset: true, preserveScale: true, preserveTexture: true);
			}
		}
	}

	private void SetMaterial(GameObject gameObject, int materialIndex, Material newMaterial, ref Material originalMaterial, bool preserveOffset, bool preserveScale, bool preserveTexture)
	{
		if (gameObject == null || newMaterial == null)
		{
			return;
		}
		Renderer renderer = gameObject.GetComponent<Renderer>();
		if (renderer == null)
		{
			return;
		}
		if (originalMaterial == null)
		{
			originalMaterial = renderer.GetSharedMaterial(materialIndex);
		}
		renderer.SetSharedMaterial(materialIndex, newMaterial);
		if ((bool)originalMaterial)
		{
			Material destMaterial = renderer.GetMaterial(materialIndex);
			if (preserveOffset)
			{
				destMaterial.mainTextureOffset = originalMaterial.mainTextureOffset;
			}
			if (preserveScale)
			{
				destMaterial.mainTextureScale = originalMaterial.mainTextureScale;
			}
			if (preserveTexture)
			{
				destMaterial.mainTexture = originalMaterial.mainTexture;
			}
		}
	}

	public bool IsShared()
	{
		return m_isShared;
	}

	public void SetIsShared(bool isShared)
	{
		if (m_isShared != isShared)
		{
			m_isShared = isShared;
			UpdateDeckLabel();
		}
	}

	public bool IsLocked()
	{
		return m_isLocked;
	}

	public void SetIsLocked(bool isLocked)
	{
		if (m_isLocked != isLocked)
		{
			m_isLocked = isLocked;
			m_normalDeckVisuals.SetActive(!m_isLocked);
			m_lockedDeckVisuals.SetActive(m_isLocked);
			SetHighlightRoot();
		}
	}

	public void SetHighlightRoot()
	{
		if (m_isLocked)
		{
			m_highlightState = m_lockedDeckVisuals.GetComponentInChildren<HighlightState>();
		}
		else
		{
			m_highlightState = m_normalDeckVisuals.GetComponentInChildren<HighlightState>();
		}
	}

	public bool IsSelected()
	{
		return m_isSelected;
	}

	public void SetIsSelected(bool isSelected)
	{
		if (m_isSelected != isSelected)
		{
			m_isSelected = isSelected;
			if (!m_isSelected && m_tooltipZone != null)
			{
				m_tooltipZone.HideTooltip();
			}
		}
	}

	public void EnableButtonAnimation()
	{
		m_animateButtonPress = true;
	}

	public void DisableButtonAnimation()
	{
		m_animateButtonPress = false;
	}

	public void PlayScaleUpAnimation()
	{
		PlayScaleUpAnimation(null);
	}

	public void PlayScaleUpAnimation(DelOnAnimationFinished callback)
	{
		PlayScaleUpAnimation(callback, null);
	}

	public void PlayScaleUpAnimation(DelOnAnimationFinished callback, object callbackData)
	{
		OnScaleFinishedCallbackData readyToScaleUpData = new OnScaleFinishedCallbackData
		{
			m_callback = callback,
			m_callbackData = callbackData
		};
		Vector3 newLocalPos = base.transform.localPosition;
		newLocalPos.y = 3.238702f;
		Hashtable posArgs = iTweenManager.Get().GetTweenHashTable();
		posArgs.Add("position", newLocalPos);
		posArgs.Add("islocal", true);
		posArgs.Add("time", 0.05f);
		posArgs.Add("easetype", iTween.EaseType.linear);
		posArgs.Add("oncomplete", "ScaleUpNow");
		posArgs.Add("oncompletetarget", base.gameObject);
		posArgs.Add("oncompleteparams", readyToScaleUpData);
		iTween.MoveTo(base.gameObject, posArgs);
	}

	private void ScaleUpNow(OnScaleFinishedCallbackData readyToScaleUpData)
	{
		ScaleDeckBox(scaleUp: true, readyToScaleUpData.m_callback, readyToScaleUpData.m_callbackData);
	}

	public void PlayScaleDownAnimation()
	{
		PlayScaleDownAnimation(null);
	}

	public void PlayScaleDownAnimation(DelOnAnimationFinished callback)
	{
		PlayScaleDownAnimation(callback, null);
	}

	public void PlayScaleDownAnimation(DelOnAnimationFinished callback, object callbackData)
	{
		OnScaleFinishedCallbackData onScaledDownData = new OnScaleFinishedCallbackData
		{
			m_callback = callback,
			m_callbackData = callbackData
		};
		ScaleDeckBox(scaleUp: false, OnScaledDown, onScaledDownData);
	}

	private void OnScaledDown(object callbackData)
	{
		OnScaleFinishedCallbackData onScaledDownData = callbackData as OnScaleFinishedCallbackData;
		Vector3 newLocalPos = base.transform.localPosition;
		newLocalPos.y = 1.273138f;
		Hashtable posArgs = iTweenManager.Get().GetTweenHashTable();
		posArgs.Add("position", newLocalPos);
		posArgs.Add("islocal", true);
		posArgs.Add("time", 0.05f);
		posArgs.Add("easetype", iTween.EaseType.linear);
		posArgs.Add("oncomplete", "ScaleDownComplete");
		posArgs.Add("oncompletetarget", base.gameObject);
		posArgs.Add("oncompleteparams", onScaledDownData);
		iTween.MoveTo(base.gameObject, posArgs);
	}

	private void ScaleDownComplete(OnScaleFinishedCallbackData onScaledDownData)
	{
		if (onScaledDownData.m_callback != null)
		{
			onScaledDownData.m_callback(onScaledDownData.m_callbackData);
		}
	}

	public void PlayPopUpAnimation()
	{
		PlayPopUpAnimation(null);
	}

	public void PlayPopUpAnimation(DelOnAnimationFinished callback)
	{
		PlayPopUpAnimation(callback, null);
	}

	public void PlayPopUpAnimation(DelOnAnimationFinished callback, object callbackData)
	{
		if (m_isPoppedUp)
		{
			callback?.Invoke(callbackData);
			return;
		}
		m_isPoppedUp = true;
		if (m_customDeckTransform != null)
		{
			m_customDeckTransform.localPosition += SCALED_UP_DECK_OFFSET;
		}
		Animation component = GetComponent<Animation>();
		component["Deck_PopUp"].time = 0f;
		component["Deck_PopUp"].speed = 6f;
		PlayPopAnimation("Deck_PopUp", callback, callbackData);
	}

	public void PlayDesaturationAnimation()
	{
		Animator animator = GetComponent<Animator>();
		if (animator != null)
		{
			animator.enabled = true;
			animator.Play("CustomDeck_Desat", 0, 0f);
		}
	}

	public void PlayPopDownAnimation()
	{
		PlayPopDownAnimation(null);
	}

	public void PlayPopDownAnimation(DelOnAnimationFinished callback)
	{
		PlayPopDownAnimation(callback, null);
	}

	public void PlayPopDownAnimation(DelOnAnimationFinished callback, object callbackData)
	{
		if (!m_isPoppedUp)
		{
			callback?.Invoke(callbackData);
			return;
		}
		m_isPoppedUp = false;
		if (m_customDeckTransform != null)
		{
			m_customDeckTransform.localPosition -= SCALED_UP_DECK_OFFSET;
		}
		Animation component = GetComponent<Animation>();
		component["Deck_PopDown"].time = 0f;
		component["Deck_PopDown"].speed = 6f;
		PlayPopAnimation("Deck_PopDown", callback, callbackData);
	}

	public void PlayPopDownAnimationImmediately()
	{
		PlayPopDownAnimationImmediately(null);
	}

	public void PlayPopDownAnimationImmediately(DelOnAnimationFinished callback)
	{
		PlayPopDownAnimationImmediately(callback, null);
	}

	public void PlayPopDownAnimationImmediately(DelOnAnimationFinished callback, object callbackData)
	{
		if (!m_isPoppedUp)
		{
			callback?.Invoke(callbackData);
			return;
		}
		m_isPoppedUp = false;
		Animation animation = GetComponent<Animation>();
		animation["Deck_PopDown"].time = animation["Deck_PopDown"].length;
		animation["Deck_PopDown"].speed = 1f;
		PlayPopAnimation("Deck_PopDown", callback, callbackData);
	}

	public void SetHighlightMaterialForState(Material mat, ActorStateType stateType)
	{
		bool stateFound = false;
		foreach (HighlightRenderState renderState in m_highlightState.m_HighlightStates)
		{
			if (renderState.m_StateType == stateType)
			{
				stateFound = true;
				renderState.m_Material = mat;
			}
		}
		if (!stateFound)
		{
			Log.All.PrintWarning("CollectionDeckBoxVisual - Attempting to set new material for state {0}, but no HighlightRenderState object found for that state type!", stateType);
		}
	}

	public void SetHighlightState(ActorStateType stateType)
	{
		if (m_highlightState != null)
		{
			if (!m_highlightState.IsReady())
			{
				StartCoroutine(ChangeHighlightStateWhenReady(stateType));
			}
			else
			{
				m_highlightState.ChangeState(stateType);
			}
		}
	}

	private IEnumerator ChangeHighlightStateWhenReady(ActorStateType stateType)
	{
		while (!m_highlightState.IsReady())
		{
			yield return null;
		}
		m_highlightState.ChangeState(stateType);
	}

	public void ShowDeleteButton(bool show)
	{
		m_deleteButton.gameObject.SetActive(show);
		UpdateInvalidIndicators(!show);
	}

	private void UpdateInvalidIndicators(bool canShow)
	{
		if (m_invalidCardCountIndicator != null)
		{
			m_invalidCardCountIndicator.SetActive(value: false);
		}
		if (m_invalidDeckIndicator != null)
		{
			m_invalidDeckIndicator.SetActive(value: false);
		}
		if (!canShow)
		{
			return;
		}
		if (m_isShowingInvalidCardCount)
		{
			if (m_invalidCardCountIndicator != null)
			{
				m_invalidCardCountIndicator.SetActive(value: true);
			}
		}
		else if (m_invalidSideboardCardCount > 0)
		{
			if (m_invalidDeckIndicator != null)
			{
				m_invalidDeckIndicator.SetActive(value: true);
			}
		}
		else if (m_missingSideboardCardCount > 0 && m_invalidDeckIndicator != null)
		{
			m_invalidDeckIndicator.SetActive(value: true);
		}
	}

	public void ShowNotificationButton(bool show)
	{
		if ((bool)m_notificationButton)
		{
			m_notificationButton.SetActive(show);
		}
		else
		{
			Log.CollectionDeckBox.PrintError("ShowNotificationButton - m_notificationButton is null");
		}
	}

	public void UpdateRuneSlotVisual(CollectionDeck deck)
	{
		if (deck == null || !deck.ShouldShowDeathKnightRunes())
		{
			m_runeSlotVisual.Hide();
		}
		else if (CollectionManager.Get().IsInEditMode())
		{
			m_runeSlotVisual.Hide();
		}
		else
		{
			m_runeSlotVisual.Show(deck.GetRuneOrder());
		}
	}

	public void UpdateColliderHeightForDeathKnight()
	{
		if (CollectionManager.Get().IsEditingDeathKnightDeck() && !(m_boxCollider == null))
		{
			Vector3 newSize = m_boxCollider.size;
			newSize.y = DEATH_KNIGHT_EDITED_DECK_BOX_COLLIDER_HEIGHT;
			m_boxCollider.size = newSize;
		}
	}

	public void ResetColliderHeight()
	{
		m_boxCollider.size = m_originalBoxColliderSize;
	}

	public void StoreOriginalButtonPositionAndRotation()
	{
		if (ButtonGameObject != null)
		{
			m_originalButtonPosition = ButtonGameObject.transform.localPosition;
			m_originalButtonRotation = ButtonGameObject.transform.localRotation;
		}
	}

	public void HideBanner()
	{
		ShowBannerInternal(show: false);
	}

	public void ShowBanner()
	{
		ShowBannerInternal(show: true);
	}

	public void AssignFromCollectionDeck(CollectionDeck deck, bool rerollFavoriteHero)
	{
		if (deck != null)
		{
			SetIsShared(deck.IsShared);
			SetDeckName(deck.Name);
			SetDeckID(deck.ID);
			SetHeroCardPremiumOverride(deck.GetDisplayHeroPremiumOverride());
			SetHeroCardID(deck.GetDisplayHeroCardID(rerollFavoriteHero), null);
			SetShowGlow(ShouldHighlightDeck(deck));
			SetFormatType(CollectionManager.Get().GetThemeShowing(deck));
			UpdateInvalidCardCountIndicator();
			UpdateRuneSlotVisual(deck);
		}
	}

	public void AssignFromMercenariesTeam(LettuceTeam team, bool suppressFX = false)
	{
		if (team != null)
		{
			SetDeckName(team.Name);
			SetDeckID(team.ID);
			LettuceMercenary merc = team.GetLeader();
			string cardId = null;
			TAG_PREMIUM cardPremium = TAG_PREMIUM.NORMAL;
			if (merc != null)
			{
				LettuceMercenary.Loadout teamLoadout = merc.GetTeamLoadout(team);
				cardId = teamLoadout.GetCardId();
				cardPremium = teamLoadout.m_artVariationPremium;
			}
			bool vfxAllowed = !string.IsNullOrEmpty(GetHeroCardID()) && !suppressFX;
			if (SetHeroCardID(cardId, cardPremium) && vfxAllowed && m_DeckPortraitChangeFSM != null)
			{
				m_DeckPortraitChangeFSM.SendEvent("Dissolve");
			}
		}
	}

	private FormatElements GetFormatElements(FormatType formatType)
	{
		if (m_deckID == 0L && formatType == FormatType.FT_UNKNOWN)
		{
			return GetStandardFormatElements();
		}
		FormatElements result = null;
		int i = 0;
		for (int iMax = m_formatElements.Count; i < iMax; i++)
		{
			FormatElements element = m_formatElements[i];
			if (element.formatType == formatType)
			{
				result = element;
				break;
			}
		}
		if (result == null)
		{
			Log.CollectionDeckBox.PrintError("Unsupported format type in CollectionDeckBoxVisual.GetFormatElements: " + formatType.ToString() + ". Will use standard formatting.");
			return GetStandardFormatElements();
		}
		return result;
	}

	private FormatElements GetStandardFormatElements()
	{
		return m_formatElements.Where((FormatElements x) => x.formatType == FormatType.FT_STANDARD).FirstOrDefault();
	}

	private FormatElements GetActiveFormatElements()
	{
		return GetFormatElements(m_formatType);
	}

	private FormatElements[] GetInactiveFormatElements()
	{
		return m_formatElements.Where((FormatElements x) => x.formatType != 0 && x.formatType != m_formatType).ToArray();
	}

	private void ShowBannerInternal(bool show)
	{
		m_showBanner = show;
		UpdateVisualBannerState();
	}

	private void UpdateVisualBannerState()
	{
		FormatElements activeFormatElements = GetActiveFormatElements();
		bool isDeckEnabled = IsDeckEnabled();
		if (activeFormatElements.disabledMeshObject != null)
		{
			activeFormatElements.disabledMeshObject.SetActive(!isDeckEnabled);
		}
		if (activeFormatElements.classObject != null)
		{
			activeFormatElements.classObject.SetActive((isDeckEnabled || (bool)UniversalInputManager.UsePhoneUI) && m_showBanner);
		}
		if (activeFormatElements.topBannerRenderer != null)
		{
			activeFormatElements.topBannerRenderer.gameObject.SetActive(isDeckEnabled && m_showBanner);
		}
	}

	private void OnDeleteButtonRollout(UIEvent e)
	{
		ShowDeleteButton(show: false);
	}

	private void OnDeleteButtonOver(UIEvent e)
	{
		SoundManager.Get().LoadAndPlay("tiny_button_mouseover_1.prefab:0ab88a13f5168ed43a3b53275114a842", base.gameObject);
	}

	private void OnDeleteButtonPressed(UIEvent e)
	{
		bool inLettuceMode = SceneMgr.Get().IsInLettuceMode();
		if ((inLettuceMode || !CollectionDeckTray.Get().IsShowingDeckContents()) && (!inLettuceMode || !CollectionDeckTray.Get().IsShowingTeamContents()))
		{
			SoundManager.Get().LoadAndPlay("tiny_button_press_1.prefab:44fc68b7418870b4797b85f0ca88a8db", base.gameObject);
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_headerText = GameStrings.Get("GLUE_COLLECTION_DELETE_CONFIRM_HEADER");
			info.m_showAlertIcon = false;
			string descriptionString = (inLettuceMode ? "GLUE_COLLECTION_DELETE_TEAM_CONFIRM_DESC" : "GLUE_COLLECTION_DELETE_CONFIRM_DESC");
			info.m_text = GameStrings.Get(descriptionString);
			info.m_alertTextAlignment = UberText.AlignmentOptions.Center;
			info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
			info.m_responseCallback = OnDeleteButtonConfirmationResponse;
			DialogManager.Get().ShowPopup(info);
		}
	}

	private void OnDeleteButtonConfirmationResponse(AlertPopup.Response response, object userData)
	{
		if (response != AlertPopup.Response.CANCEL)
		{
			SetEnabled(enabled: false);
			if (SceneMgr.Get().IsInLettuceMode())
			{
				TeamsContent.DeleteTeam(GetDeckID());
			}
			else
			{
				DecksContent.DeleteDeck(GetDeckID());
			}
		}
	}

	private void PlayPopAnimation(string animationName)
	{
		PlayPopAnimation(animationName, null, null);
	}

	private void PlayPopAnimation(string animationName, DelOnAnimationFinished callback, object callbackData)
	{
		GetComponent<Animation>().Play(animationName);
		OnPopAnimationFinishedCallbackData onAnimationFinishedCallbackData = new OnPopAnimationFinishedCallbackData
		{
			m_callback = callback,
			m_callbackData = callbackData,
			m_animationName = animationName
		};
		StopCoroutine("WaitThenCallAnimationCallback");
		StartCoroutine("WaitThenCallAnimationCallback", onAnimationFinishedCallbackData);
	}

	private IEnumerator WaitThenCallAnimationCallback(OnPopAnimationFinishedCallbackData callbackData)
	{
		Animation animation = GetComponent<Animation>();
		yield return new WaitForSeconds(animation[callbackData.m_animationName].length / animation[callbackData.m_animationName].speed);
		bool enableInput = callbackData.m_animationName.Equals("Deck_PopUp");
		SetEnabled(enableInput);
		if (callbackData.m_callback != null)
		{
			callbackData.m_callback(callbackData.m_callbackData);
		}
	}

	private void ScaleDeckBox(bool scaleUp, DelOnAnimationFinished callback, object callbackData)
	{
		OnScaleFinishedCallbackData onScaleFinishedCallbackData = new OnScaleFinishedCallbackData
		{
			m_callback = callback,
			m_callbackData = callbackData
		};
		Vector3 newScale = (scaleUp ? SCALED_UP_LOCAL_SCALE : SCALED_DOWN_LOCAL_SCALE);
		Hashtable scaleArgHash = iTweenManager.Get().GetTweenHashTable();
		scaleArgHash.Add("scale", newScale);
		scaleArgHash.Add("islocal", true);
		scaleArgHash.Add("time", 0.2f);
		scaleArgHash.Add("easetype", iTween.EaseType.linear);
		scaleArgHash.Add("oncomplete", "OnScaleComplete");
		scaleArgHash.Add("oncompletetarget", base.gameObject);
		scaleArgHash.Add("oncompleteparams", onScaleFinishedCallbackData);
		scaleArgHash.Add("name", "scale");
		iTween.StopByName(base.gameObject, "scale");
		iTween.ScaleTo(base.gameObject, scaleArgHash);
	}

	private void OnScaleComplete(OnScaleFinishedCallbackData callbackData)
	{
		if (callbackData.m_callback != null)
		{
			callbackData.m_callback(callbackData.m_callbackData);
		}
	}

	private void OnHeroFullDefLoaded(string cardID, DefLoader.DisposableFullDef def, object userData)
	{
		Log.CollectionDeckBox.Print("OnHeroFullDefLoaded cardID: {0},  m_heroCardID: {1}", cardID, m_heroCardID);
		m_fullDef?.Dispose();
		m_fullDef = null;
		m_legendaryHeroPortrait?.Dispose();
		m_legendaryHeroPortrait = null;
		if (cardID == null || !cardID.Equals(m_heroCardID))
		{
			SetPortrait(m_defaultPortraitMaterial);
			m_heroCardID = "None";
			return;
		}
		m_fullDef = def;
		Material portraitMat = null;
		if (m_fullDef != null && m_fullDef.CardDef != null)
		{
			portraitMat = m_fullDef.CardDef.GetCustomDeckPortrait();
		}
		if (portraitMat == null && m_defaultPortraitMaterial != null)
		{
			portraitMat = m_defaultPortraitMaterial;
		}
		string legendaryModel = m_fullDef?.CardDef?.m_LegendaryModel;
		if (!string.IsNullOrEmpty(legendaryModel))
		{
			LegendaryHeroRenderToTextureService service = ServiceManager.Get<LegendaryHeroRenderToTextureService>();
			if (service != null)
			{
				m_legendaryHeroPortrait = service.CreatePortrait(legendaryModel, Player.Side.NEUTRAL);
				if (m_legendaryHeroPortrait.PortraitTexture != null && portraitMat != null)
				{
					portraitMat = UnityEngine.Object.Instantiate(portraitMat);
					portraitMat.mainTexture = m_legendaryHeroPortrait.PortraitTexture;
				}
				else
				{
					m_legendaryHeroPortrait.Dispose();
					m_legendaryHeroPortrait = null;
				}
			}
		}
		if (m_legendaryHeroPortrait != null && !m_fullDef.CardDef.m_IgnoreLegendaryPortraitForDeckCollection)
		{
			m_legendaryHeroPortrait.ClearDynamicResolutionControllers();
			foreach (FormatElements elements in m_formatElements)
			{
				GameObject portraitObject = elements.portraitObject;
				if (portraitObject == null)
				{
					Log.CollectionDeckBox.PrintError("OnHeroFullDefLoaded portrait object was INVALID for cardID: {0}", cardID);
					continue;
				}
				LegendarySkinDynamicResController dynamicResController = portraitObject.GetComponent<LegendarySkinDynamicResController>();
				if (!dynamicResController)
				{
					dynamicResController = portraitObject.AddComponent<LegendarySkinDynamicResController>();
				}
				m_legendaryHeroPortrait.ConnectDynamicResolutionController(dynamicResController);
				dynamicResController.CacheMaterialProperties(portraitMat);
				dynamicResController.Renderer = portraitObject.GetComponent<Renderer>();
				dynamicResController.MaterialIdx = elements.portraitMaterialIndex;
			}
		}
		else
		{
			foreach (FormatElements formatElement in m_formatElements)
			{
				GameObject portraitObject2 = formatElement.portraitObject;
				if (portraitObject2 == null)
				{
					Log.CollectionDeckBox.PrintError("OnHeroFullDefLoaded portrait object was INVALID for cardID: {0}", cardID);
					continue;
				}
				LegendarySkinDynamicResController dynamicResController2 = portraitObject2.GetComponent<LegendarySkinDynamicResController>();
				if ((bool)dynamicResController2)
				{
					dynamicResController2.Skin = null;
					dynamicResController2.Renderer = null;
				}
			}
		}
		SetPortrait(portraitMat);
		if (m_legendaryHeroPortrait != null)
		{
			m_legendaryHeroPortrait.UpdateDynamicResolutionControllers();
		}
		if (m_fullDef != null && m_fullDef.EntityDef != null && !m_fullDef.EntityDef.IsLettuceMercenary())
		{
			TAG_CLASS heroClass = m_fullDef.EntityDef.GetClass();
			if (heroClass == TAG_CLASS.INVALID)
			{
				Log.CollectionDeckBox.PrintError("OnHeroFullDefLoaded heroClass was INVALID for cardID: {0},  heroClass: {1}", cardID, heroClass);
			}
			else
			{
				SetClassDisplay(heroClass);
			}
		}
		UpdateDeckLabel();
		UpdateUnlockableStatus();
	}

	private void UpdatePortraitMaterial(GameObject portraitObject, Material portraitMaterial, int portraitMaterialIndex)
	{
		if (portraitMaterial == null)
		{
			Log.CollectionDeckBox.PrintError("Custom Deck Portrait Material is null!");
			return;
		}
		if (portraitObject == null)
		{
			Log.CollectionDeckBox.PrintError("Custom Deck Portrait GameObject is null!");
			return;
		}
		Renderer portraitObjectRenderer = portraitObject.GetComponent<Renderer>();
		if (portraitObjectRenderer == null)
		{
			Log.CollectionDeckBox.PrintError("Custom Deck Portrait GameObject doesnt have a renderer!");
			return;
		}
		portraitObjectRenderer.SetSharedMaterial(portraitMaterialIndex, portraitMaterial);
		if (m_fullDef?.CardDef == null || m_neverUseGoldenPortraits)
		{
			return;
		}
		TAG_PREMIUM premium = GetHeroCardPremium();
		if ((premium != TAG_PREMIUM.GOLDEN && GameUtils.IsVanillaHero(m_fullDef.EntityDef.GetCardId())) || (premium == TAG_PREMIUM.NORMAL && m_fullDef.EntityDef.IsLettuceMercenary()) || m_graphicsManager.isVeryLowQualityDevice())
		{
			return;
		}
		Material premiumPortraitMaterial = m_fullDef?.CardDef.GetPremiumPortraitMaterial();
		if (premiumPortraitMaterial != null)
		{
			Material currMaterial = portraitObjectRenderer.GetMaterial(portraitMaterialIndex);
			Texture shadowTex = null;
			if (currMaterial.HasProperty("_ShadowTex"))
			{
				shadowTex = currMaterial.GetTexture("_ShadowTex");
			}
			portraitObjectRenderer.SetMaterial(portraitMaterialIndex, premiumPortraitMaterial);
			portraitObjectRenderer.GetMaterial(portraitMaterialIndex).SetTexture("_ShadowTex", shadowTex);
			Material material = portraitObjectRenderer.GetMaterial(portraitMaterialIndex);
			material.mainTextureOffset = currMaterial.mainTextureOffset;
			material.mainTextureScale = currMaterial.mainTextureScale;
		}
		UberShaderAnimation premiumPortraitAnimation = m_fullDef?.CardDef.GetPortraitAnimation(premium);
		if (premiumPortraitAnimation != null)
		{
			UberShaderController uberController = portraitObject.GetComponent<UberShaderController>();
			if (uberController == null)
			{
				uberController = portraitObject.AddComponent<UberShaderController>();
			}
			uberController.UberShaderAnimation = UnityEngine.Object.Instantiate(premiumPortraitAnimation);
			uberController.m_MaterialIndex = portraitMaterialIndex;
		}
	}

	private void SetPortrait(Material portraitMaterial)
	{
		foreach (FormatElements elements in m_formatElements)
		{
			UpdatePortraitMaterial(elements.portraitObject, portraitMaterial, elements.portraitMaterialIndex);
		}
	}

	private void SetClassDisplay(TAG_CLASS classTag)
	{
		Color bannerColor = CollectionPageManager.ColorForClass(classTag);
		UnityEngine.Vector2 classTextureOffsets = CollectionPageManager.s_classTextureOffsets[classTag];
		CollectionDeck deck = GetCollectionDeck();
		if (deck != null && deck.IsTwistHeroicDeck)
		{
			bannerColor = new Color(57f / 85f, 57f / 85f, 57f / 85f);
			classTextureOffsets = new UnityEngine.Vector2(0.58f, 0.606f);
		}
		foreach (FormatElements elements in m_formatElements)
		{
			if (elements.classObject == null)
			{
				continue;
			}
			MeshRenderer meshRenderer = elements.classObject.GetComponent<MeshRenderer>();
			if (!(meshRenderer != null))
			{
				continue;
			}
			Material classIconMaterial = meshRenderer.GetMaterial(elements.classIconMaterialIndex);
			Material bannerMaterial = meshRenderer.GetMaterial(elements.classBannerMaterialIndex);
			if (!(classIconMaterial == null) && !(bannerMaterial == null))
			{
				classIconMaterial.mainTextureOffset = classTextureOffsets;
				bannerMaterial.color = bannerColor;
				if (elements.topBannerRenderer != null)
				{
					elements.topBannerRenderer.GetMaterial(m_topBannerMaterialIndex).color = bannerColor;
				}
			}
		}
	}

	private void MarkRewardedDeckAsSeen(long deckId)
	{
		if (RewardUtils.HasNewRewardedDeck(out var rewardedDeckId) && deckId == rewardedDeckId)
		{
			RewardUtils.MarkNewestRewardedDeckAsSeen();
		}
	}

	private void MarkDeckAsSeen()
	{
		SetHighlightState(ActorStateType.HIGHLIGHT_PRIMARY_MOUSE_OVER);
		CollectionDeck deck = GetCollectionDeck();
		if (deck != null && deck.NeedsName)
		{
			Log.CollectionDeckBox.Print($"Sending deck changes for deck {m_deckID}, to clear the NEEDS_NAME flag.");
			deck.SendChanges(CollectionDeck.ChangeSource.MarkDeckAsSeen);
			deck.NeedsName = false;
		}
		MarkRewardedDeckAsSeen(m_deckID);
		m_showGlow = false;
	}

	protected override void OnPress()
	{
		if (m_animateButtonPress && !m_isLocked && !m_isSelected && IsDeckEnabled())
		{
			OnPressEvent();
		}
	}

	protected override void OnHold()
	{
		CollectionDeckTray tray = CollectionDeckTray.Get();
		if (!(tray == null))
		{
			DeckTray.DeckContentTypes deckContentType = tray.GetCurrentContentType();
			if (deckContentType != 0 && deckContentType != DeckTray.DeckContentTypes.Teams)
			{
				_ = 6;
			}
			else
			{
				OnHoldReorderable(tray);
			}
		}
	}

	private void OnHoldReorderable(CollectionDeckTray tray)
	{
		DeckTrayReorderableContent reorderableContent = tray.GetReorderableContent();
		if (!(reorderableContent == null) && !reorderableContent.IsTouchDragging)
		{
			if (m_tooltipZone != null)
			{
				m_tooltipZone.HideTooltip();
			}
			SetHighlightState(ActorStateType.HIGHLIGHT_OFF);
			if (ButtonGameObject != null)
			{
				Hashtable args = iTweenManager.Get().GetTweenHashTable();
				args.Add("scale", reorderableContent.m_rearrangeEnlargeScale * Vector3.one);
				args.Add("islocal", true);
				args.Add("time", reorderableContent.m_rearrangeStartStopTweenDuration);
				args.Add("easetype", iTween.EaseType.linear);
				iTween.ScaleTo(ButtonGameObject, args);
			}
			reorderableContent.StartDragToReorder(this);
		}
	}

	protected override void OnRelease()
	{
		DeckTrayDeckListContent decksContent = DecksContent;
		if (decksContent != null)
		{
			decksContent.StopDragToReorder();
		}
		if (m_isLocked || m_isSelected || !IsDeckEnabled())
		{
			return;
		}
		if (!SceneMgr.Get().IsInTavernBrawlMode() || UniversalInputManager.Get().IsTouchMode())
		{
			string selectDeckSound = GetActiveFormatElements().deckSelectSound;
			if (!string.IsNullOrEmpty(selectDeckSound))
			{
				SoundManager.Get().LoadAndPlay(selectDeckSound, base.gameObject);
			}
		}
		OnReleaseEvent();
	}

	public void OnStopDragToReorder()
	{
		if (!UniversalInputManager.Get().IsTouchMode())
		{
			ShowDeleteButton(show: true);
		}
		if (ButtonGameObject != null)
		{
			float stopEnlargeDuration = 0.1f;
			DeckTrayDeckListContent decksContent = DecksContent;
			if (decksContent != null)
			{
				stopEnlargeDuration = decksContent.m_rearrangeStartStopTweenDuration;
			}
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("scale", Vector3.one);
			args.Add("islocal", true);
			args.Add("time", stopEnlargeDuration);
			args.Add("easetype", iTween.EaseType.linear);
			iTween.ScaleTo(ButtonGameObject, args);
		}
		OnOutEvent();
	}

	protected override void OnOut(InteractionState oldState)
	{
		if (m_tooltipZone != null)
		{
			m_tooltipZone.HideTooltip();
		}
		OnOutEvent();
	}

	protected override void OnOver(InteractionState oldState)
	{
		if (m_tooltipZone != null)
		{
			if (m_isLocked)
			{
				m_tooltipZone.ShowTooltip(GameStrings.Get("GLUE_LOCKED_DECK_HEADER"), GameStrings.Get("GLUE_LOCKED_DECK_DESC"), 4f);
			}
			else if (DeckPickerTrayDisplay.Get() != null)
			{
				CollectionDeck deck = GetCollectionDeck();
				string requiredCardId;
				if (!IsDeckEnabled())
				{
					string disabledDeckHeader = GameStrings.Format("GLUE_DISABLED_DECK_HEADER", GameStrings.GetFormatName(m_formatType));
					string disabledDeckDesc;
					if (deck == null)
					{
						disabledDeckDesc = "";
					}
					else if (!GameUtils.HasUnlockedClass(deck.GetClass()))
					{
						disabledDeckHeader = GameStrings.Get("GLUE_DISABLED_DECK_HEADER_CLASS_LOCKED");
						disabledDeckDesc = GameStrings.Get("GLUE_DISABLED_DECK_CLASS_LOCKED_DESC");
					}
					else if (!deck.IsLoanerDeck)
					{
						disabledDeckDesc = ((!CollectionDeck.DoesModeRequireSpecificFormat(SceneMgr.Get().GetMode(), Options.GetInRankedPlayMode())) ? GameStrings.Get("GLUE_DISABLED_DECK_IN_CURRENT_MODE_DESC") : GameStrings.Format("GLUE_DISABLED_DECK_DESC", GameStrings.GetFormatName(Options.GetFormatType())));
					}
					else
					{
						disabledDeckHeader = GameStrings.Get("GLUE_COLLECTION_DECK_INVALID_LOANER_DECK_TOOLTIP_HEADER");
						disabledDeckDesc = GameStrings.Get("GLUE_COLLECTION_DECK_INVALID_LOANER_DECK_TOOLTIP_BODY");
					}
					m_tooltipZone.ShowTooltip(disabledDeckHeader, disabledDeckDesc, 4f);
				}
				else if (m_isShowingInvalidCardCount)
				{
					if (m_cardCountByStatus.Extra > 0)
					{
						GameStrings.PluralNumber[] plurals = GameStrings.MakePlurals(m_cardCountByStatus.Extra);
						m_tooltipZone.ShowTooltip(GameStrings.FormatPlurals("GLUE_EXTRA_CARDS_DECK_HEADER", plurals), GameStrings.Get("GLUE_EXTRA_CARDS_DECK_DESC"), 4f);
					}
					else
					{
						GameStrings.PluralNumber[] plurals2 = GameStrings.MakePlurals(m_cardCountByStatus.MissingPlusInvalid);
						m_tooltipZone.ShowTooltip(GameStrings.FormatPlurals("GLUE_MISSING_CARDS_DECK_HEADER", plurals2), GameStrings.Get("GLUE_MISSING_CARDS_DECK_DESC"), 4f);
					}
				}
				else if (m_invalidSideboardCardCount > 0)
				{
					m_tooltipZone.ShowTooltip(GameStrings.Get("GLUE_COLLECTION_INVALID_SIDEBOARD_CARDS_HEADER"), GameStrings.Get("GLUE_MISSING_CARDS_DECK_DESC"), 4f);
				}
				else if (m_missingSideboardCardCount > 0)
				{
					m_tooltipZone.ShowTooltip(GameStrings.Get("GLUE_COLLECTION_INVALID_SIDEBOARD_CARDS_HEADER"), GameStrings.Get("GLUE_MISSING_CARDS_DECK_DESC"), 4f);
				}
				else if (deck.DeckTemplate_HasUnownedRequirements(out requiredCardId))
				{
					int cardId = GameUtils.TranslateCardIdToDbId(requiredCardId);
					EntityDef entityDef = DefLoader.Get().GetEntityDef(cardId);
					string missingCardName = "Unknown";
					if (entityDef != null)
					{
						missingCardName = entityDef.GetName();
					}
					m_tooltipZone.ShowTooltip(GameStrings.Get("GLUE_DISABLED_DECK_MISSING_REQUIRED_CARD_HEADER"), GameStrings.Format("GLUE_DISABLED_DECK_MISSING_REQUIRED_CARD_DESC", missingCardName), 4f);
				}
			}
		}
		OnOverEvent();
	}

	public override void SetEnabled(bool enabled, bool isInternal = false)
	{
		base.SetEnabled(enabled, isInternal);
		if (!enabled && m_tooltipZone != null)
		{
			m_tooltipZone.HideTooltip();
		}
	}

	private void OnPressEvent()
	{
		ShowDeleteButton(show: false);
		SceneMgr.Mode currentMode = SceneMgr.Get().GetMode();
		if (currentMode != SceneMgr.Mode.COLLECTIONMANAGER && currentMode != SceneMgr.Mode.LETTUCE_COLLECTION)
		{
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("position", m_pressedBone.transform.localPosition);
			args.Add("islocal", true);
			args.Add("time", 0.1f);
			args.Add("easetype", iTween.EaseType.linear);
			iTween.MoveTo(ButtonGameObject, args);
		}
	}

	private void OnReleaseEvent()
	{
		if (UniversalInputManager.Get().IsTouchMode() && m_showGlow)
		{
			MarkDeckAsSeen();
		}
		SceneMgr.Mode currentMode = SceneMgr.Get().GetMode();
		if (currentMode != SceneMgr.Mode.COLLECTIONMANAGER && currentMode != SceneMgr.Mode.LETTUCE_COLLECTION)
		{
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("position", m_originalButtonPosition);
			args.Add("islocal", true);
			args.Add("time", 0.1f);
			args.Add("easetype", iTween.EaseType.linear);
			iTween.MoveTo(ButtonGameObject, args);
		}
	}

	private void OnOutEvent()
	{
		if (!m_isSelected)
		{
			SetHighlightState(ActorStateType.HIGHLIGHT_OFF);
		}
		SceneMgr.Mode currentMode = SceneMgr.Get().GetMode();
		if (currentMode != SceneMgr.Mode.COLLECTIONMANAGER && currentMode != SceneMgr.Mode.LETTUCE_COLLECTION)
		{
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("position", m_originalButtonPosition);
			args.Add("islocal", true);
			args.Add("time", 0.1f);
			args.Add("easetype", iTween.EaseType.linear);
			iTween.MoveTo(ButtonGameObject, args);
		}
	}

	private void OnOverEvent()
	{
		if (UniversalInputManager.Get().IsTouchMode())
		{
			return;
		}
		DeckTrayDeckListContent decksContent = DecksContent;
		if ((!(decksContent != null) || decksContent.DraggingDeckBox == null) && !m_isSelected)
		{
			SoundManager.Get().LoadAndPlay("collection_manager_hero_mouse_over.prefab:653cc8000b988cd468d2210a209adce6", base.gameObject);
			if (m_showGlow)
			{
				MarkDeckAsSeen();
			}
			else if (IsDeckEnabled())
			{
				SetHighlightState(ActorStateType.HIGHLIGHT_MOUSE_OVER);
			}
		}
	}

	private void ReparentElements(FormatType formatType)
	{
		FormatElements formatElements = GetFormatElements(formatType);
		Transform parent = formatElements.portraitObject.transform;
		m_highlight.transform.parent = parent;
		m_deckName.gameObject.transform.parent = parent;
		m_deckDesc.gameObject.transform.parent = parent;
		m_invalidCardCountIndicator.gameObject.transform.parent = parent;
		if (m_deckRunes != null)
		{
			m_deckRunes.gameObject.transform.parent = parent;
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			formatElements.classObject.transform.parent = parent;
		}
		m_bones.m_gradientOneLine.parent = parent;
		m_bones.m_gradientTwoLine.parent = parent;
	}

	private static bool ShouldHighlightDeck(CollectionDeck deck)
	{
		if (!deck.NeedsName)
		{
			if (RewardUtils.HasNewRewardedDeck(out var rewardedDeckId))
			{
				return deck.ID == rewardedDeckId;
			}
			return false;
		}
		return true;
	}
}
