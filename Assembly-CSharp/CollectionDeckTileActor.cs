using System;
using System.Collections;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.UI;
using UnityEngine;

public class CollectionDeckTileActor : Actor
{
	[Serializable]
	public class DeckTileFrameColorSet
	{
		public Color m_desatColor = Color.white;

		public float m_desatContrast;

		public float m_desatAmount;

		public Color m_costTextColor = Color.white;

		public Color m_countTextColor = new Color(1f, 0.9f, 0f, 1f);

		public Color m_nameTextColor = Color.white;

		public Color m_sliderColor = new Color(0.62f, 0.62f, 0.62f, 1f);

		public Color m_outlineColor = Color.black;

		public Material m_frameMaterial;

		public Material m_interiorFrameMaterial;

		public Material m_highlightMaterial;

		public Material m_highlightGlowMaterial;

		public Material m_manaGemMaterial;
	}

	public enum GhostedState
	{
		NONE,
		BLUE,
		RED,
		NOT_INCLUDED
	}

	public enum TileIconState
	{
		CARD_COUNT,
		UNIQUE_STAR,
		MULTI_CARD
	}

	public Material m_halfPremiumFrameMaterial;

	public Material m_premiumFrameMaterial;

	public Material m_halfNormalSignatureFrameMaterial;

	public Material m_halfGoldSignatureMaterial;

	public Material m_signatureFrameMaterial;

	public Material m_diamondFrameMaterial;

	public GameObject m_frame;

	public GameObject m_frameInterior;

	public GameObject m_uniqueStar;

	public GameObject m_multiCardIcon;

	public GameObject m_highlight;

	public GameObject m_highlightGlow;

	public GameObject m_attentionGlow;

	public UberText m_countText;

	public MeshRenderer m_manaGem;

	public MeshRenderer m_slider;

	public float m_tooltipScale = 8f;

	public float m_tooltipDelay = 0.85f;

	public WidgetInstance m_sideboardSection;

	private DeckCardBarSideboardWidget m_sideboardSectionWidget;

	[Tooltip("Normal Style Settings")]
	public DeckTileFrameColorSet m_normalColorSet = new DeckTileFrameColorSet();

	[Tooltip("Ghost Style Settings")]
	public DeckTileFrameColorSet m_ghostedColorSet = new DeckTileFrameColorSet();

	[Tooltip("Red Style Settings")]
	public DeckTileFrameColorSet m_redColorSet = new DeckTileFrameColorSet();

	[Tooltip("Not Included Style Settings")]
	public DeckTileFrameColorSet m_notIncludedColorSet = new DeckTileFrameColorSet();

	private const float SLIDER_ANIM_TIME = 0.35f;

	private const string NOT_INCLUDED_TEXT = "!";

	private UberText m_countTextMesh;

	private bool m_sliderIsOpen;

	private Vector3 m_originalSliderLocalPos;

	private Vector3 m_openSliderLocalPos;

	private GhostedState m_ghosted;

	private CollectionDeckSlot m_slot;

	private static readonly Vector3 CardNameTextDefaultPositionPC = new Vector3(1.2083f, 0.2267f, 0.0303f);

	private static readonly Vector3 CardNameTextDeathKnightPositionPC = new Vector3(0.8f, 0.2267f, 0.0303f);

	private const float CardNameTextDefaultWidthPC = 17.43f;

	private const float CardNameTextDeathKnightWidthPC = 16f;

	private static readonly Vector3 CardNameTextDefaultPositionPhone = new Vector3(5.24f, 0.23f, 0.03f);

	private static readonly Vector3 CardNameTextDeathKnightPositionPhone = new Vector3(4f, 0.23f, 0.03f);

	private const float CardNameTextDefaultWidthPhone = 8.42f;

	private const float CardNameTextDeathKnightWidthPhone = 8.42f;

	private Coroutine m_showTooltipCoroutine;

	public static event Action<CollectionDeckTileActor> DeckTileSideboardButtonPressed;

	protected override void OnEnable()
	{
		base.OnEnable();
		AddSideboardSectionListeners();
	}

	private void OnDisable()
	{
		RemoveSideboardSectionListeners();
	}

	public override void Awake()
	{
		base.Awake();
		AssignSlider();
		AssignCardCount();
	}

	public static TileIconState GetCorrectTileIconState(bool isUnique, bool isMulticard)
	{
		if (isMulticard)
		{
			return TileIconState.MULTI_CARD;
		}
		if (isUnique)
		{
			return TileIconState.UNIQUE_STAR;
		}
		return TileIconState.CARD_COUNT;
	}

	public void UpdateDeckCardProperties(bool isUnique, bool isMultiCard, int numCards, bool useSliderAnimations)
	{
		TileIconState iconState = GetCorrectTileIconState(isUnique, isMultiCard);
		UpdateDeckCardProperties(iconState, numCards, useSliderAnimations);
	}

	public void UpdateDeckCardProperties(TileIconState iconState, int numCards, bool useSliderAnimations)
	{
		if (iconState == TileIconState.MULTI_CARD && m_ghosted == GhostedState.NOT_INCLUDED)
		{
			iconState = TileIconState.CARD_COUNT;
		}
		switch (iconState)
		{
		case TileIconState.MULTI_CARD:
			m_uniqueStar.SetActive(value: false);
			m_countTextMesh.gameObject.SetActive(value: false);
			m_multiCardIcon.SetActive(m_shown);
			break;
		case TileIconState.UNIQUE_STAR:
			m_uniqueStar.SetActive(m_shown);
			m_countTextMesh.gameObject.SetActive(value: false);
			m_multiCardIcon.SetActive(value: false);
			break;
		case TileIconState.CARD_COUNT:
			m_uniqueStar.SetActive(value: false);
			m_countTextMesh.gameObject.SetActive(m_shown);
			m_multiCardIcon.SetActive(value: false);
			m_countTextMesh.Text = ((m_ghosted == GhostedState.NOT_INCLUDED) ? "!" : Convert.ToString(numCards));
			break;
		}
		if (iconState == TileIconState.UNIQUE_STAR || numCards > 1)
		{
			OpenSlider(useSliderAnimations);
		}
		else
		{
			CloseSlider(useSliderAnimations);
		}
	}

	public void UpdateMaterial(Material material)
	{
		MeshRenderer portraitMeshRenderer = m_portraitMesh.GetComponent<MeshRenderer>();
		if (material == null)
		{
			Debug.LogErrorFormat("Null portrait material specified for {0}", GetEntityDef().GetCardId());
			Material material2 = portraitMeshRenderer.GetMaterial();
			material2.SetFloat("_OffsetX", 0f);
			material2.SetFloat("_OffsetY", 0f);
		}
		else
		{
			portraitMeshRenderer.SetMaterial(material);
		}
	}

	public void SetGhosted(GhostedState state)
	{
		m_ghosted = state;
	}

	public override void SetPremium(TAG_PREMIUM premium)
	{
		base.SetPremium(premium);
		UpdateFrameMaterial();
	}

	public CollectionDeckSlot GetSlot()
	{
		return m_slot;
	}

	private static void GetCardNameTextPositionAndWidth(bool offsetForRunes, out Vector3 position, out float width)
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			if (offsetForRunes)
			{
				position = CardNameTextDeathKnightPositionPhone;
				width = 8.42f;
			}
			else
			{
				position = CardNameTextDefaultPositionPhone;
				width = 8.42f;
			}
		}
		else if (offsetForRunes)
		{
			position = CardNameTextDeathKnightPositionPC;
			width = 16f;
		}
		else
		{
			position = CardNameTextDefaultPositionPC;
			width = 17.43f;
		}
	}

	public void SetSlot(CollectionDeckSlot slot)
	{
		m_slot = slot;
	}

	public void UpdateGhostTileEffect()
	{
		if (!(m_manaGem == null))
		{
			UpdateFrameMaterial();
			DeckTileFrameColorSet colorSet = GetColorSet(m_ghosted);
			m_manaGem.SetMaterial(colorSet.m_manaGemMaterial);
			m_countText.TextColor = colorSet.m_countTextColor;
			m_nameTextMesh.TextColor = colorSet.m_nameTextColor;
			m_costTextMesh.TextColor = colorSet.m_costTextColor;
			if (m_countText.Outline)
			{
				m_countText.OutlineColor = colorSet.m_outlineColor;
			}
			if (m_nameTextMesh.Outline)
			{
				m_nameTextMesh.OutlineColor = colorSet.m_outlineColor;
			}
			if (m_costTextMesh.Outline)
			{
				m_costTextMesh.OutlineColor = colorSet.m_outlineColor;
			}
			if ((bool)m_highlight && (bool)colorSet.m_highlightMaterial)
			{
				m_highlight.GetComponent<Renderer>().SetMaterial(colorSet.m_highlightMaterial);
			}
			if ((bool)m_highlightGlow && (bool)colorSet.m_highlightGlowMaterial)
			{
				m_highlightGlow.GetComponent<Renderer>().SetMaterial(colorSet.m_highlightGlowMaterial);
			}
			SetDesaturationAmount(GetPortraitMaterial(), colorSet);
			SetDesaturationAmount(m_uniqueStar.GetComponent<MeshRenderer>().GetMaterial(), colorSet);
			SetDesaturationAmount(m_multiCardIcon.GetComponent<MeshRenderer>().GetMaterial(), colorSet);
			if (m_ghosted == GhostedState.NOT_INCLUDED)
			{
				m_cardRuneBanner.SetState(ConvertGhostStateToRuneState(m_ghosted));
			}
		}
	}

	private static RuneState ConvertGhostStateToRuneState(GhostedState state)
	{
		return state switch
		{
			GhostedState.NOT_INCLUDED => RuneState.Disabled, 
			GhostedState.BLUE => RuneState.Blue, 
			GhostedState.RED => RuneState.Red, 
			_ => RuneState.Default, 
		};
	}

	public override void UpdateCardRuneBannerComponent()
	{
		if (!(m_cardRuneBanner == null))
		{
			m_cardRuneBanner.Hide();
			RunePattern runePattern = default(RunePattern);
			if (m_entity != null && m_entity.HasRuneCost)
			{
				runePattern.SetCostsFromEntity(m_entity);
				m_cardRuneBanner.Show(runePattern, ConvertGhostStateToRuneState(m_ghosted));
			}
			else if (m_entityDef != null && m_entityDef.HasRuneCost)
			{
				runePattern.SetCostsFromEntity(m_entityDef);
				m_cardRuneBanner.Show(runePattern, ConvertGhostStateToRuneState(m_ghosted));
			}
		}
	}

	public void UpdateNameTextForRuneBar(bool offsetCardNameForRunes)
	{
		GetCardNameTextPositionAndWidth(offsetCardNameForRunes, out var cardNamePosition, out var cardNameWidth);
		m_nameTextMesh.transform.localPosition = cardNamePosition;
		m_nameTextMesh.Width = cardNameWidth;
	}

	protected override bool IsPremiumPortraitEnabled()
	{
		return false;
	}

	public override void UpdateAllComponents(bool needsGhostUpdate = true)
	{
		base.UpdateAllComponents(needsGhostUpdate);
		if (base.DeckCardBarPortrait != null)
		{
			UpdateMaterial(base.DeckCardBarPortrait);
		}
	}

	private void SetDesaturationAmount(Material material, DeckTileFrameColorSet colorSet)
	{
		material.SetColor("_Color", colorSet.m_desatColor);
		material.SetFloat("_Desaturate", colorSet.m_desatAmount);
		material.SetFloat("_Contrast", colorSet.m_desatContrast);
	}

	private void UpdateFrameMaterial()
	{
		DeckTileFrameColorSet colorSet = GetColorSet(m_ghosted);
		Material frameMaterial = colorSet.m_frameMaterial;
		Material frameInteriorMaterial = colorSet.m_interiorFrameMaterial;
		if (m_ghosted == GhostedState.NONE)
		{
			if (m_slot != null)
			{
				int normalCount = m_slot.GetCount(TAG_PREMIUM.NORMAL);
				int goldenCount = m_slot.GetCount(TAG_PREMIUM.GOLDEN);
				int count = m_slot.GetCount(TAG_PREMIUM.SIGNATURE);
				int diamondCount = m_slot.GetCount(TAG_PREMIUM.DIAMOND);
				if (count > 0)
				{
					frameMaterial = ((goldenCount > 0) ? m_halfGoldSignatureMaterial : ((normalCount <= 0) ? m_signatureFrameMaterial : m_halfNormalSignatureFrameMaterial));
				}
				else if (normalCount > 0 && goldenCount > 0)
				{
					frameMaterial = m_halfPremiumFrameMaterial;
				}
				else if (goldenCount > 0 && normalCount <= 0)
				{
					frameMaterial = m_premiumFrameMaterial;
				}
				else if (diamondCount > 0)
				{
					frameMaterial = m_diamondFrameMaterial;
				}
			}
			else if (m_premiumType == TAG_PREMIUM.GOLDEN)
			{
				frameMaterial = m_premiumFrameMaterial;
			}
			else if (m_premiumType == TAG_PREMIUM.SIGNATURE)
			{
				frameMaterial = m_signatureFrameMaterial;
			}
			else if (m_premiumType == TAG_PREMIUM.DIAMOND)
			{
				frameMaterial = m_diamondFrameMaterial;
			}
		}
		if (frameMaterial != null)
		{
			m_frame.GetComponent<Renderer>().SetMaterial(frameMaterial);
		}
		if (frameInteriorMaterial != null)
		{
			m_frameInterior.GetComponent<Renderer>().SetMaterial(frameInteriorMaterial);
		}
	}

	private void AssignSlider()
	{
		m_originalSliderLocalPos = m_slider.transform.localPosition;
		m_openSliderLocalPos = m_rootObject.transform.Find("OpenSliderPosition").transform.localPosition;
	}

	private void AssignCardCount()
	{
		m_countTextMesh = m_rootObject.transform.Find("CardCountText").GetComponent<UberText>();
	}

	private void OpenSlider(bool useSliderAnimations)
	{
		if (!m_sliderIsOpen)
		{
			m_sliderIsOpen = true;
			iTween.StopByName(m_slider.gameObject, "position");
			if (useSliderAnimations)
			{
				Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
				moveArgs.Add("position", m_openSliderLocalPos);
				moveArgs.Add("islocal", true);
				moveArgs.Add("time", 0.35f);
				moveArgs.Add("easetype", iTween.EaseType.easeOutBounce);
				moveArgs.Add("name", "position");
				iTween.MoveTo(m_slider.gameObject, moveArgs);
			}
			else
			{
				m_slider.transform.localPosition = m_openSliderLocalPos;
			}
		}
	}

	private void CloseSlider(bool useSliderAnimations)
	{
		if (m_sliderIsOpen)
		{
			m_sliderIsOpen = false;
			iTween.StopByName(m_slider.gameObject, "position");
			if (useSliderAnimations)
			{
				Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
				moveArgs.Add("position", m_originalSliderLocalPos);
				moveArgs.Add("islocal", true);
				moveArgs.Add("time", 0.35f);
				moveArgs.Add("easetype", iTween.EaseType.easeOutBounce);
				moveArgs.Add("name", "position");
				iTween.MoveTo(m_slider.gameObject, moveArgs);
			}
			else
			{
				m_slider.transform.localPosition = m_originalSliderLocalPos;
			}
		}
	}

	private DeckTileFrameColorSet GetColorSet(GhostedState state)
	{
		return state switch
		{
			GhostedState.BLUE => m_ghostedColorSet, 
			GhostedState.RED => m_redColorSet, 
			GhostedState.NOT_INCLUDED => m_notIncludedColorSet, 
			_ => m_normalColorSet, 
		};
	}

	public void SetupSideboard(SideboardDeck sideboard)
	{
		if (!m_sideboardSection)
		{
			return;
		}
		bool entityHasSideboard = m_entityDef != null && m_entityDef.HasSideboard;
		if (!m_sideboardSectionWidget)
		{
			m_sideboardSection.RegisterReadyListener(delegate
			{
				m_sideboardSectionWidget = m_sideboardSection.GetComponentInChildren<DeckCardBarSideboardWidget>();
				AddSideboardSectionListeners();
				if (entityHasSideboard)
				{
					ShowSideboardWidget(sideboard);
				}
				else
				{
					HideSideboardWidget();
				}
			});
		}
		else if (entityHasSideboard)
		{
			ShowSideboardWidget(sideboard);
		}
		else
		{
			HideSideboardWidget();
		}
	}

	private void ShowSideboardWidget(SideboardDeck sideboard)
	{
		m_sideboardSectionWidget.Show();
		m_sideboardSectionWidget.Initialize(sideboard);
	}

	private void HideSideboardWidget()
	{
		m_sideboardSectionWidget.Hide();
	}

	private void AddSideboardSectionListeners()
	{
		if (!(m_sideboardSectionWidget == null))
		{
			RemoveSideboardSectionListeners();
			m_sideboardSectionWidget.SideboardButtonPressed += OnSideboardSectionButtonPress;
			m_sideboardSectionWidget.SideboardButtonOver += OnSideboardSectionButtonOver;
			m_sideboardSectionWidget.SideboardButtonOut += OnSideboardSectionButtonOut;
		}
	}

	private void RemoveSideboardSectionListeners()
	{
		if (!(m_sideboardSectionWidget == null))
		{
			m_sideboardSectionWidget.SideboardButtonPressed -= OnSideboardSectionButtonPress;
			m_sideboardSectionWidget.SideboardButtonOver -= OnSideboardSectionButtonOver;
			m_sideboardSectionWidget.SideboardButtonOut -= OnSideboardSectionButtonOut;
		}
	}

	private void OnSideboardSectionButtonPress()
	{
		CollectionDeckTileActor.DeckTileSideboardButtonPressed?.Invoke(this);
	}

	private void OnSideboardSectionButtonOver()
	{
		if (m_entityDef == null)
		{
			return;
		}
		string cardId = m_entityDef.GetCardId();
		CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
		if (deck == null)
		{
			return;
		}
		SideboardDeck sideboardDeck = deck.GetSideboard(cardId);
		if (sideboardDeck != null)
		{
			if (ShouldSideboardShowEmptySideboardTooltip(sideboardDeck))
			{
				ShowSideboardButtonTooltip("GLUE_COLLECTION_EMPTY_SIDEBOARD_HEADER", "GLUE_COLLECTION_EMPTY_SIDEBOARD_DESCRIPTION");
			}
			else if (ShouldSideboardShowInvalidTooltip(sideboardDeck))
			{
				ShowSideboardButtonTooltip("GLUE_COLLECTION_INVALID_SIDEBOARD_CARDS_HEADER", "GLUE_COLLECTION_INVALID_SIDEBOARD_CARDS_DESCRIPTION");
			}
		}
	}

	private void OnSideboardSectionButtonOut()
	{
		HideTooltip();
	}

	private void ShowSideboardButtonTooltip(string header, string description)
	{
		if (m_showTooltipCoroutine != null)
		{
			StopCoroutine(m_showTooltipCoroutine);
		}
		if (!(m_sideboardSectionWidget == null))
		{
			TooltipZone tooltip = m_sideboardSectionWidget.gameObject.GetComponent<TooltipZone>();
			if (!(tooltip == null))
			{
				m_showTooltipCoroutine = StartCoroutine(ShowSideboardButtonTooltip(tooltip, header, description));
			}
		}
	}

	private void HideTooltip()
	{
		if (m_showTooltipCoroutine != null)
		{
			StopCoroutine(m_showTooltipCoroutine);
		}
		if (!(m_sideboardSectionWidget == null))
		{
			TooltipZone tooltipZone = m_sideboardSectionWidget.gameObject.GetComponent<TooltipZone>();
			if (!(tooltipZone == null))
			{
				tooltipZone.HideTooltip();
			}
		}
	}

	private IEnumerator ShowSideboardButtonTooltip(TooltipZone tooltip, string header, string description)
	{
		yield return new WaitForSeconds(m_tooltipDelay);
		string headerText = GameStrings.Get(header);
		string descriptionText = GameStrings.Get(description);
		tooltip.ShowBoxTooltip(headerText, descriptionText);
		tooltip.Scale = m_tooltipScale;
	}

	private bool ShouldSideboardShowEmptySideboardTooltip(SideboardDeck sideboardDeck)
	{
		if (sideboardDeck.SideboardType == TAG_SIDEBOARD_TYPE.ETC)
		{
			return sideboardDeck.GetTotalCardCount() < 1;
		}
		return false;
	}

	private bool ShouldSideboardShowInvalidTooltip(SideboardDeck sideboardDeck)
	{
		if (sideboardDeck.SideboardType == TAG_SIDEBOARD_TYPE.ETC)
		{
			return sideboardDeck.GetTotalInvalidCardCount(null, includeInvalidRuneCards: true) > 0;
		}
		return false;
	}
}
