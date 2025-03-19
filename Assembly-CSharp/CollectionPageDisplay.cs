using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.Core;
using Blizzard.T5.MaterialService.Extensions;
using PegasusShared;
using UnityEngine;

public class CollectionPageDisplay : CollectiblePageDisplay
{
	public enum HEADER_CLASS
	{
		INVALID,
		SHAMAN,
		PALADIN,
		MAGE,
		DRUID,
		HUNTER,
		ROGUE,
		WARRIOR,
		PRIEST,
		WARLOCK,
		HEROSKINS,
		CARDBACKS,
		DEMONHUNTER,
		COINS,
		DEATHKNIGHT
	}

	public GameObject m_heroSkinsDecor;

	public GameObject[] m_heroSkinFrames;

	public GameObject m_heroPicker;

	public GameObject m_deckTemplateContainer;

	public GameObject m_noMatchFoundObject;

	public UberText m_noMatchExplanationText;

	public GameObject m_noMatchSetHintObject;

	public GameObject m_noMatchManaHintObject;

	public GameObject m_noMatchCraftingHintObject;

	public GameObject m_noMatchTouristClassHintObject;

	public Material m_deckTemplatePageMaterial;

	public Color m_standardTitleTextColor;

	public Material m_wildHeaderMaterial;

	public Material m_wildPageMaterial;

	public Color m_wildTextColor;

	public Color m_wildTitleTextColor;

	public Material m_classicHeaderMaterial;

	public Material m_classicPageMaterial;

	public Color m_classicTextColor;

	public Color m_classicTitleTextColor;

	public FormatType m_pageFormatType;

	[SerializeField]
	private GameObject m_touristStamp;

	[SerializeField]
	private float m_touristStampToolitpScale;

	private TooltipZone m_tourstStampTooltipZone;

	private bool m_shouldSuppressTouristStampTooltip;

	private MassDisenchant m_massDisenchantVisual;

	public void Awake()
	{
		if (!(m_touristStamp != null))
		{
			return;
		}
		PegUIElement touristStampUIElement = m_touristStamp.GetComponent<PegUIElement>();
		if (touristStampUIElement != null)
		{
			touristStampUIElement.AddEventListener(UIEventType.ROLLOVER, OnTouristStampRollover);
			touristStampUIElement.AddEventListener(UIEventType.ROLLOUT, OnTouristStampRollout);
		}
		m_tourstStampTooltipZone = m_touristStamp.GetComponent<TooltipZone>();
		CollectionManager cm = CollectionManager.Get();
		if (cm == null)
		{
			return;
		}
		CollectionManagerDisplay collectionManagerDisplay = cm.GetCollectibleDisplay() as CollectionManagerDisplay;
		if (collectionManagerDisplay != null)
		{
			CollectionPageManager collectionPageManager = collectionManagerDisplay.GetPageManager() as CollectionPageManager;
			if (collectionPageManager != null)
			{
				collectionPageManager.OnSuppressTouristTooltipChanged += OnSuppressTouristTooltipChanged;
				m_shouldSuppressTouristStampTooltip = collectionPageManager.SuppressTouristTooltip;
			}
		}
	}

	public void OnDestroy()
	{
		CollectionManager cm = CollectionManager.Get();
		if (cm == null)
		{
			return;
		}
		CollectionManagerDisplay collectionManagerDisplay = cm.GetCollectibleDisplay() as CollectionManagerDisplay;
		if (collectionManagerDisplay != null)
		{
			CollectionPageManager collectionPageManager = collectionManagerDisplay.GetPageManager() as CollectionPageManager;
			if (collectionPageManager != null)
			{
				collectionPageManager.OnSuppressTouristTooltipChanged -= OnSuppressTouristTooltipChanged;
			}
		}
	}

	public override void UpdateCollectionItems(List<CollectionCardActors> actorList, List<ICollectible> nonActorCollectibleList, CollectionUtils.ViewMode mode)
	{
		UpdateAllSpecialCaseTransforms();
		base.UpdateCollectionItems(actorList, nonActorCollectibleList, mode);
		foreach (CollectionCardVisual collectionCardVisual in m_collectionCardVisuals)
		{
			collectionCardVisual.SetFavoriteBannerActive(isActive: false);
		}
		DetachAndHideMassDisenchantVisual();
		UpdateFavoriteCardBacks(mode);
		UpdateFavoriteHeroSkins(mode);
		UpdateFavoriteCoin(mode);
		UpdateHeroSkinNames(mode);
		UpdateHeroPicker(mode);
	}

	public void UpdatePageWithMassDisenchant()
	{
		MassDisenchant massDisenchantVisual = GetMassDisenchantVisual();
		if (massDisenchantVisual != null)
		{
			massDisenchantVisual.Show();
		}
	}

	private void DetachAndHideMassDisenchantVisual()
	{
		if (m_massDisenchantVisual != null)
		{
			m_massDisenchantVisual.Hide();
			m_massDisenchantVisual = null;
		}
	}

	public void UpdatePageWithHeroPicker(int[] allHeroCounts, int[] ownedHeroCounts)
	{
		CollectionHeroPickerButtons heroPickerButtons = m_heroPicker.GetComponentInChildren<CollectionHeroPickerButtons>();
		if (heroPickerButtons != null)
		{
			heroPickerButtons.LoadHeroButtonsForFavoriteHeroes();
			heroPickerButtons.Show();
			heroPickerButtons.UpdateHeroClassTotals(allHeroCounts, ownedHeroCounts);
		}
	}

	public void HideHeroPicker()
	{
		CollectionHeroPickerButtons heroPickerButtons = m_heroPicker.GetComponentInChildren<CollectionHeroPickerButtons>();
		if (heroPickerButtons != null && heroPickerButtons.IsReady())
		{
			heroPickerButtons.Hide();
		}
	}

	public void UpdateFavoriteHeroSkins(CollectionUtils.ViewMode mode)
	{
		if (mode != CollectionUtils.ViewMode.HERO_SKINS)
		{
			HideHeroSkinsDecor();
			return;
		}
		if (m_heroSkinsDecor != null && m_heroSkinFrames != null)
		{
			m_heroSkinsDecor.SetActive(value: true);
			HideAllHeroSkinFrames();
		}
		int heroFrameIdx = 0;
		foreach (CollectionCardVisual cardVisual in m_collectionCardVisuals)
		{
			if (cardVisual.IsShown())
			{
				Actor actor = cardVisual.GetActor();
				CollectionHeroSkin heroSkin = actor.GetComponent<CollectionHeroSkin>();
				if (heroSkin == null)
				{
					continue;
				}
				heroSkin.ShowShadow(actor.IsShown());
				EntityDef entityDef = actor.GetEntityDef();
				if (entityDef != null)
				{
					TAG_CLASS heroClass = entityDef.GetClass();
					string heroId = entityDef.GetCardId();
					heroSkin.SetClass(entityDef);
					bool showFavoriteBanner = CollectionManager.Get().GetCountOfOwnedHeroesForClass(heroClass) > 1 && CollectionManager.Get().IsFavoriteHero(heroId);
					heroSkin.ShowFavoriteBanner(showFavoriteBanner);
				}
			}
			if (heroFrameIdx < m_heroSkinFrames.Length)
			{
				m_heroSkinFrames[heroFrameIdx++].SetActive(cardVisual.IsShown());
			}
		}
	}

	public void UpdateHeroSkinNames(CollectionUtils.ViewMode mode)
	{
		if (mode == CollectionUtils.ViewMode.HERO_SKINS)
		{
			StartCoroutine(WaitThenUpdateHeroSkinNames(mode));
		}
	}

	private IEnumerator WaitThenUpdateHeroSkinNames(CollectionUtils.ViewMode mode)
	{
		yield return null;
		foreach (CollectionCardVisual cardVisual in m_collectionCardVisuals)
		{
			if (cardVisual.IsShown())
			{
				CollectionHeroSkin heroSkin = cardVisual.GetActor().GetComponent<CollectionHeroSkin>();
				if (!(heroSkin == null))
				{
					heroSkin.ShowCollectionManagerText();
				}
			}
		}
	}

	public void UpdateHeroPicker(CollectionUtils.ViewMode mode)
	{
		if (mode != CollectionUtils.ViewMode.HERO_PICKER)
		{
			HideHeroPicker();
		}
	}

	public void UpdateFavoriteCardBacks(CollectionUtils.ViewMode mode)
	{
		if (mode != CollectionUtils.ViewMode.CARD_BACKS)
		{
			return;
		}
		HashSet<int> favoriteCardbacks = CardBackManager.Get().GetCardBacks().FavoriteCardBacks;
		foreach (CollectionCardVisual cardVisual in m_collectionCardVisuals)
		{
			if (cardVisual.IsShown())
			{
				CollectionCardBack cardback = cardVisual.GetActor().GetComponent<CollectionCardBack>();
				if (!(cardback == null))
				{
					bool isFavorite = favoriteCardbacks.Contains(cardback.GetCardBackId());
					cardback.ShowFavoriteBanner(isFavorite);
				}
			}
		}
	}

	public void UpdateFavoriteBattlegroundsGuideSkin(CollectionUtils.ViewMode mode)
	{
		_ = 6;
	}

	public void UpdateFavoriteCoin(CollectionUtils.ViewMode mode)
	{
		if (mode != CollectionUtils.ViewMode.COINS)
		{
			return;
		}
		foreach (CollectionCardVisual cardVisual in m_collectionCardVisuals)
		{
			bool isFavorite = cardVisual.IsShown() && CosmeticCoinManager.Get().IsFavoriteCoinCard(cardVisual.CardId);
			cardVisual.SetFavoriteBannerActive(isFavorite);
		}
	}

	public void UpdateDeckTemplateHeader(GameObject deckTemplateHeader, FormatType pageFormatType)
	{
		if (!(deckTemplateHeader == null) && deckTemplateHeader.TryGetComponent<Renderer>(out var deckTempalteHeaderRenderer))
		{
			Material l_headerMaterial = GetHeaderMaterial(pageFormatType, null);
			deckTempalteHeaderRenderer.SetMaterial(l_headerMaterial);
		}
	}

	public void UpdateDeckTemplatePage(Component deckTemplatePicker)
	{
		if (!(deckTemplatePicker != null) || !(m_deckTemplateContainer != null))
		{
			return;
		}
		foreach (CollectionCardVisual collectionCardVisual in m_collectionCardVisuals)
		{
			collectionCardVisual.Hide();
			collectionCardVisual.SetActors(null);
		}
		if (m_basePage != null)
		{
			MeshRenderer basePageRenderer = m_basePage.GetComponent<MeshRenderer>();
			m_basePageMaterial = basePageRenderer.GetMaterial();
			basePageRenderer.SetMaterial(m_deckTemplatePageMaterial);
		}
		GameUtils.SetParent(deckTemplatePicker, m_deckTemplateContainer);
		GameUtils.ResetTransform(deckTemplatePicker);
	}

	public override void ShowNoMatchesFound(bool show, CollectionManager.FindCardsResult findResults = null, bool showHints = true)
	{
		m_noMatchFoundObject.SetActive(show);
		m_noMatchCraftingHintObject.SetActive(value: false);
		m_noMatchSetHintObject.SetActive(value: false);
		m_noMatchManaHintObject.SetActive(value: false);
		if (m_noMatchTouristClassHintObject != null)
		{
			m_noMatchTouristClassHintObject.SetActive(value: false);
		}
		string explanationTextKey = "GLUE_COLLECTION_NO_RESULTS";
		if (show && showHints && findResults != null)
		{
			CollectionManager cm = CollectionManager.Get();
			CollectibleDisplay collectibleDisplay = cm?.GetCollectibleDisplay();
			CollectionPageManager pageManager = ((collectibleDisplay != null) ? (collectibleDisplay.GetPageManager() as CollectionPageManager) : null);
			bool isTouristClass = false;
			bool hasAnyTouristCards = false;
			if (cm != null)
			{
				CollectionDeck currentDeck = cm.GetEditedDeck();
				if (currentDeck != null)
				{
					TAG_CLASS currentClass = pageManager.GetCurrentClassContextClassTag();
					isTouristClass = currentDeck.GetTouristClasses().Contains(currentClass);
					if (isTouristClass && findResults.m_cards.Count((CollectibleCard collectibleCard) => collectibleCard.Class == currentClass && collectibleCard.Set == TAG_CARD_SET.ISLAND_VACATION) == 0)
					{
						hasAnyTouristCards = CollectionManager.Get().GetOwnedCards().Any((CollectibleCard collectibleCard) => collectibleCard.Class == currentClass && collectibleCard.Set == TAG_CARD_SET.ISLAND_VACATION);
					}
				}
			}
			if (collectibleDisplay.GetViewSubmode() == CollectionUtils.ViewSubmode.CARD_ZILLIAX_MODULES)
			{
				explanationTextKey = "GLUE_COLLECTION_NO_SAVED_VERSIONS";
			}
			else if (isTouristClass && !hasAnyTouristCards)
			{
				if (m_noMatchTouristClassHintObject != null)
				{
					m_noMatchTouristClassHintObject.SetActive(value: true);
				}
				explanationTextKey = "GLUE_COLLECTION_NO_CLASS_CARDS";
			}
			else if (findResults.m_resultsWithoutManaFilterExist)
			{
				m_noMatchManaHintObject.SetActive(value: true);
				explanationTextKey = "GLUE_COLLECTION_NO_RESULTS_IN_SELECTED_COST";
			}
			else if (findResults.m_resultsWithoutSetFilterExist)
			{
				m_noMatchSetHintObject.SetActive(value: true);
				explanationTextKey = "GLUE_COLLECTION_NO_RESULTS_IN_CURRENT_SET";
			}
			else if (findResults.m_resultsUnownedExist)
			{
				m_noMatchCraftingHintObject.SetActive(value: true);
				explanationTextKey = "GLUE_COLLECTION_NO_RESULTS_BUT_CRAFTABLE";
			}
			else if (findResults.m_resultsInWildExist)
			{
				explanationTextKey = "GLUE_COLLECTION_NO_RESULTS_IN_STANDARD";
			}
		}
		m_noMatchExplanationText.Text = GameStrings.Get(explanationTextKey);
	}

	public void HideHeroSkinsDecor()
	{
		if (m_heroSkinsDecor != null)
		{
			m_heroSkinsDecor.SetActive(value: false);
		}
		HideAllHeroSkinFrames();
	}

	public void HideAllHeroSkinFrames()
	{
		if (m_heroSkinFrames != null && m_heroSkinFrames.Count() > 0)
		{
			GameObject[] heroSkinFrames = m_heroSkinFrames;
			for (int i = 0; i < heroSkinFrames.Length; i++)
			{
				heroSkinFrames[i].SetActive(value: false);
			}
		}
	}

	public override void UpdateCurrentPageCardLocks(bool playSound = false)
	{
		base.UpdateCurrentPageCardLocks(playSound);
		CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
		if (deck == null || (CollectionDeckTray.Get() != null && CollectionDeckTray.Get().UpdateCurrentPageCardLocks(m_collectionCardVisuals)))
		{
			return;
		}
		foreach (CollectionCardVisual collectionCardVisual in m_collectionCardVisuals)
		{
			if (!collectionCardVisual.IsShown() || collectionCardVisual.GetVisualType() != 0)
			{
				collectionCardVisual.ShowLock(CollectionCardVisual.LockType.NONE);
				continue;
			}
			Actor actor = collectionCardVisual.GetActor();
			if (actor == null)
			{
				continue;
			}
			string cardID = actor.GetEntityDef()?.GetCardId();
			TAG_PREMIUM premium = actor.GetPremium();
			CollectibleCard card = CollectionManager.Get().GetCard(cardID, premium);
			if (card == null)
			{
				continue;
			}
			if (card.OwnedCount <= 0)
			{
				collectionCardVisual.ShowLock(CollectionCardVisual.LockType.NONE);
				continue;
			}
			CollectionCardVisual.LockType lockType = CollectionCardVisual.LockType.NONE;
			DeckRuleset deckRuleset = CollectionManager.Get().GetDeckRuleset();
			if (deckRuleset == null || deckRuleset.CanAddToDeck(actor.GetEntityDef(), premium, deck, out List<RuleInvalidReason> reasons, out List<DeckRule> brokenRules, DeckRule.RuleType.DEATHKNIGHT_RUNE_LIMIT, DeckRule.RuleType.TOURIST_LIMIT))
			{
				collectionCardVisual.ShowLock(CollectionCardVisual.LockType.NONE, null, playSound);
				continue;
			}
			int indexToUse = 0;
			DeckRule deckRule = brokenRules[indexToUse];
			if (deckRule != null && deckRule.Type == DeckRule.RuleType.COUNT_COPIES_OF_EACH_CARD)
			{
				int minCount = reasons[indexToUse].CountParam;
				for (int i = 1; i < reasons.Count; i++)
				{
					if (brokenRules[i].Type == DeckRule.RuleType.COUNT_COPIES_OF_EACH_CARD && reasons[i].CountParam < minCount)
					{
						indexToUse = i;
						minCount = reasons[i].CountParam;
					}
				}
			}
			string displayError = ((reasons[indexToUse] != null) ? reasons[indexToUse].DisplayError : "NULL");
			DeckRule deckRule2 = brokenRules[indexToUse];
			if (deckRule2 != null && deckRule2.Type == DeckRule.RuleType.IS_CARD_PLAYABLE)
			{
				lockType = CollectionCardVisual.LockType.NOT_PLAYABLE;
			}
			else
			{
				DeckRule deckRule3 = brokenRules[indexToUse];
				if (deckRule3 != null && deckRule3.Type == DeckRule.RuleType.PLAYER_OWNS_EACH_COPY)
				{
					lockType = CollectionCardVisual.LockType.NO_MORE_INSTANCES;
				}
				else
				{
					DeckRule deckRule4 = brokenRules[indexToUse];
					lockType = ((deckRule4 != null && deckRule4.Type == DeckRule.RuleType.COUNT_COPIES_OF_EACH_CARD) ? CollectionCardVisual.LockType.MAX_COPIES_IN_DECK : CollectionCardVisual.LockType.BANNED);
				}
			}
			if (brokenRules.Count > 1)
			{
				int indexOwnershipRule = brokenRules.FindIndex((DeckRule r) => r.Type == DeckRule.RuleType.PLAYER_OWNS_EACH_COPY);
				if (indexOwnershipRule >= 0)
				{
					int signatureCount = deck.GetCardCountAllMatchingSlots(cardID, TAG_PREMIUM.SIGNATURE);
					int diamondCount = deck.GetCardCountAllMatchingSlots(cardID, TAG_PREMIUM.DIAMOND);
					int goldenCount = deck.GetCardCountAllMatchingSlots(cardID, TAG_PREMIUM.GOLDEN);
					int normalCount = deck.GetCardCountAllMatchingSlots(cardID, TAG_PREMIUM.NORMAL);
					_ = card.OwnedCount;
					bool showOwnershipError = false;
					switch (premium)
					{
					case TAG_PREMIUM.SIGNATURE:
						if (diamondCount + goldenCount + normalCount > 0)
						{
							showOwnershipError = true;
						}
						break;
					case TAG_PREMIUM.DIAMOND:
						if (signatureCount + goldenCount + normalCount > 0)
						{
							showOwnershipError = true;
						}
						break;
					case TAG_PREMIUM.GOLDEN:
						if (signatureCount + diamondCount + normalCount > 0)
						{
							showOwnershipError = true;
						}
						break;
					case TAG_PREMIUM.NORMAL:
						if (signatureCount + diamondCount + goldenCount > 0)
						{
							showOwnershipError = true;
						}
						break;
					}
					if (showOwnershipError)
					{
						lockType = CollectionCardVisual.LockType.NO_MORE_INSTANCES;
						displayError = ((reasons[indexOwnershipRule] != null) ? reasons[indexOwnershipRule].DisplayError : "NULL");
					}
				}
			}
			collectionCardVisual.ShowLock(lockType, displayError, playSound);
		}
	}

	public override void SetPageType(FormatType inputFormatType)
	{
		if (inputFormatType == m_pageFormatType)
		{
			return;
		}
		m_pageFormatType = inputFormatType;
		if (m_pageFlavorHeader != null)
		{
			Material l_headerMaterial = GetHeaderMaterial(inputFormatType, null);
			if (l_headerMaterial != null)
			{
				m_pageFlavorHeader.GetComponent<Renderer>().SetMaterial(l_headerMaterial);
			}
		}
		if (m_pageCountText != null)
		{
			m_pageCountText.TextColor = GetTextColor(inputFormatType, m_textColor);
		}
		Material l_pageMaterial = GetPageMaterial(inputFormatType, null);
		if (l_pageMaterial != null)
		{
			m_basePageRenderer.SetMaterial(l_pageMaterial);
		}
	}

	public void SetPageTextColor()
	{
		if (m_pageNameText != null)
		{
			m_pageNameText.TextColor = GetTitleTextColor(CollectionManager.Get().GetThemeShowing(), m_textColor);
		}
	}

	public void SetClass(CollectionTabInfo tabInfo, string noClassOverride = "")
	{
		UpdateTouristStampVisibility(m_touristStamp, m_tourstStampTooltipZone, tabInfo.tagClass);
		if (tabInfo.tagClass == TAG_CLASS.INVALID)
		{
			SetPageNameText(noClassOverride);
			if (m_pageFlavorHeader != null)
			{
				if (noClassOverride == "")
				{
					m_pageFlavorHeader.SetActive(value: false);
				}
				else
				{
					SetPageFlavorTextures(m_pageFlavorHeader, TagClassToHeaderClass(TAG_CLASS.NEUTRAL));
				}
			}
		}
		else
		{
			string pageName = GameStrings.GetClassName(tabInfo.tagClass);
			SetPageNameText(pageName);
			SetPageFlavorTextures(m_pageFlavorHeader, TagClassToHeaderClass(tabInfo.tagClass));
		}
	}

	public void SetHeroPicker()
	{
		SetPageNameText(GameStrings.Get("GLUE_COLLECTION_MANAGER_HERO_SKINS_TITLE"));
		SetPageFlavorTextures(m_pageFlavorHeader, HEADER_CLASS.HEROSKINS);
		HideTouristStamp();
	}

	public void SetHeroSkins(TAG_CLASS? classTag)
	{
		if (!classTag.HasValue)
		{
			SetPageNameText(GameStrings.Get("GLUE_COLLECTION_MANAGER_HERO_SKINS_TITLE"));
		}
		else
		{
			SetPageNameText(GameStrings.GetClassName(classTag.Value));
		}
		SetPageFlavorTextures(m_pageFlavorHeader, HEADER_CLASS.HEROSKINS);
		HideTouristStamp();
	}

	public void SetCardBacks()
	{
		SetPageNameText(GameStrings.Get("GLUE_COLLECTION_MANAGER_CARD_BACKS_TITLE"));
		SetPageFlavorTextures(m_pageFlavorHeader, HEADER_CLASS.CARDBACKS);
		HideTouristStamp();
	}

	public void SetCoins()
	{
		SetPageNameText(GameStrings.Get("GLUE_COLLECTION_MANAGER_COIN_TITLE"));
		SetPageFlavorTextures(m_pageFlavorHeader, HEADER_CLASS.COINS);
		HideTouristStamp();
	}

	public void SetDeckTemplates()
	{
		SetPageNameText(string.Empty);
		if (m_pageFlavorHeader != null)
		{
			m_pageFlavorHeader.SetActive(value: false);
		}
		HideTouristStamp();
	}

	public void SetMassDisenchant()
	{
		SetPageNameText(string.Empty);
		if (m_pageFlavorHeader != null)
		{
			m_pageFlavorHeader.SetActive(value: false);
		}
		HideTouristStamp();
	}

	public TAG_CLASS? GetFirstCardClass()
	{
		if (m_collectionCardVisuals.Count == 0)
		{
			return null;
		}
		CollectionCardVisual firstCardVisual = m_collectionCardVisuals[0];
		if (!firstCardVisual.IsShown())
		{
			return null;
		}
		Actor firstCardActor = firstCardVisual.GetActor();
		if (!firstCardActor.IsShown())
		{
			return null;
		}
		return firstCardActor.GetEntityDef()?.GetClass();
	}

	public void HideTouristStamp()
	{
		if (m_touristStamp != null)
		{
			m_touristStamp.SetActive(value: false);
		}
		if (m_tourstStampTooltipZone != null)
		{
			m_tourstStampTooltipZone.HideTooltip();
		}
	}

	private MassDisenchant GetMassDisenchantVisual()
	{
		if (MassDisenchant.Get() == null)
		{
			return null;
		}
		m_massDisenchantVisual = MassDisenchant.Get();
		GameUtils.SetParent(m_massDisenchantVisual, base.gameObject);
		return m_massDisenchantVisual;
	}

	private Material GetHeaderMaterial(FormatType formatType, Material defaultMaterial)
	{
		if (!new Map<FormatType, Material>
		{
			{
				FormatType.FT_STANDARD,
				m_headerMaterial
			},
			{
				FormatType.FT_WILD,
				m_wildHeaderMaterial
			},
			{
				FormatType.FT_CLASSIC,
				m_classicHeaderMaterial
			},
			{
				FormatType.FT_TWIST,
				m_classicHeaderMaterial
			}
		}.TryGetValue(formatType, out var l_result))
		{
			return defaultMaterial;
		}
		return l_result;
	}

	private Material GetPageMaterial(FormatType formatType, Material defaultMaterial)
	{
		if (!new Map<FormatType, Material>
		{
			{
				FormatType.FT_STANDARD,
				m_pageMaterial
			},
			{
				FormatType.FT_WILD,
				m_wildPageMaterial
			},
			{
				FormatType.FT_CLASSIC,
				m_classicPageMaterial
			},
			{
				FormatType.FT_TWIST,
				m_classicPageMaterial
			}
		}.TryGetValue(formatType, out var l_result))
		{
			return defaultMaterial;
		}
		return l_result;
	}

	private Color GetTextColor(FormatType formatType, Color defaultColor)
	{
		if (!new Map<FormatType, Color>
		{
			{
				FormatType.FT_STANDARD,
				m_textColor
			},
			{
				FormatType.FT_WILD,
				m_wildTextColor
			},
			{
				FormatType.FT_CLASSIC,
				m_classicTextColor
			}
		}.TryGetValue(formatType, out var l_result))
		{
			return defaultColor;
		}
		return l_result;
	}

	private Color GetTitleTextColor(FormatType formatType, Color defaultColor)
	{
		if (!new Map<FormatType, Color>
		{
			{
				FormatType.FT_STANDARD,
				m_standardTitleTextColor
			},
			{
				FormatType.FT_WILD,
				m_wildTitleTextColor
			},
			{
				FormatType.FT_CLASSIC,
				m_classicTitleTextColor
			}
		}.TryGetValue(formatType, out var l_result))
		{
			return defaultColor;
		}
		return l_result;
	}

	private void OnSuppressTouristTooltipChanged(bool shouldSuppressTooltips)
	{
		m_shouldSuppressTouristStampTooltip = shouldSuppressTooltips;
		if (shouldSuppressTooltips && m_tourstStampTooltipZone != null)
		{
			m_tourstStampTooltipZone.HideTooltip();
		}
	}

	private void OnTouristStampRollover(UIEvent e)
	{
		if (m_tourstStampTooltipZone != null && !m_shouldSuppressTouristStampTooltip)
		{
			m_tourstStampTooltipZone.ShowTooltip(GameStrings.Get("GLUE_TOURIST_CLASS_TAB_TOOLTIP_HEADER"), GameStrings.Get("GLUE_TOURIST_CLASS_TAB_TOOLTIP_DESCRIPTION"), m_touristStampToolitpScale);
		}
	}

	private void OnTouristStampRollout(UIEvent e)
	{
		if (m_tourstStampTooltipZone != null)
		{
			m_tourstStampTooltipZone.HideTooltip();
		}
	}

	public static HEADER_CLASS TagClassToHeaderClass(TAG_CLASS classTag)
	{
		string tagClass = classTag.ToString();
		if (Enum.IsDefined(typeof(HEADER_CLASS), tagClass))
		{
			return (HEADER_CLASS)Enum.Parse(typeof(HEADER_CLASS), tagClass);
		}
		return HEADER_CLASS.INVALID;
	}

	public static void SetPageFlavorTextures(GameObject header, HEADER_CLASS headerClass)
	{
		if (!(header == null))
		{
			float x = (((float)headerClass < 8f) ? 0f : 0.5f);
			float y = (0f - (float)headerClass) / 8f;
			CollectiblePageDisplay.SetPageFlavorTextures(header, new UnityEngine.Vector2(x, y));
		}
	}

	private static void UpdateTouristStampVisibility(GameObject touristStamp, TooltipZone touristTooltipZone, TAG_CLASS classTag)
	{
		CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
		bool isCurrentlyTouristClass = false;
		if (deck != null && deck.GetTouristClasses().Contains(classTag))
		{
			isCurrentlyTouristClass = true;
		}
		if (touristStamp != null)
		{
			touristStamp.SetActive(isCurrentlyTouristClass);
		}
		if (touristTooltipZone != null)
		{
			touristTooltipZone.HideTooltip();
		}
	}
}
