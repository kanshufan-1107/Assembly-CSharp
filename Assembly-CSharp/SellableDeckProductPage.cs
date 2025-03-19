using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone.DataModels;
using Hearthstone.UI;
using Hearthstone.UI.Core;
using UnityEngine;

public class SellableDeckProductPage : ProductPage, IPopupRendering
{
	private enum VariantStyles
	{
		NoVariant,
		Golden,
		Class
	}

	[SerializeField]
	private Listable m_cardListListable;

	[SerializeField]
	private AsyncReference classSelectorReference;

	[SerializeField]
	private PlayMakerFSM m_turnPagePlaymaker;

	[SerializeField]
	private GameObject m_slidingRoot;

	[SerializeField]
	private UIBScrollable m_scrollbar;

	[SerializeField]
	[Header("Single Deck")]
	private AsyncReference m_singleDeckPouchReference;

	[SerializeField]
	[Header("Deck UI Transforms")]
	private Quaternion m_deckPouchRotation;

	[SerializeField]
	private Quaternion m_deckDescriptionRotation;

	[SerializeField]
	private Vector3 m_startingDeckPouchPos;

	[SerializeField]
	private Vector3 m_deckPouchWithDescriptionPos;

	[SerializeField]
	private Vector3 m_startingDeckDescriptionPos;

	[SerializeField]
	private Vector3 m_deckPouchScale;

	[SerializeField]
	private float m_deckPouchSpacing;

	private const int PAGE_1 = 1;

	private const string PAGE_ANIM_RESET_EVENT = "Reset";

	private const string PAGE_LEFT_PRESSED_EVENT = "PageLeft_code";

	private const string PAGE_RIGHT_PRESSED_EVENT = "PageRight_code";

	private const string ANIMATION_FINISHED_EVENT = "AnimationFinished_code";

	private const string SELLABLE_DECK_BUNDLE = "SELLABLE_DECK_BUNDLE";

	private const string PATH_OF_ARTHAS = "PATH_OF_ARTHAS";

	private static readonly AssetReference s_shopDeckPouch = new AssetReference("ShopDeckPouch.prefab:4e70532f22849244d9a8080e9bad7e9b");

	private static readonly AssetReference s_shopDeckDescription = new AssetReference("DeckDescription.prefab:9f8466429f0b1274f822454005995fb8");

	private ProductDataModel m_firstVariant;

	private WidgetInstance m_singleDeckPouch;

	private ShopDeckPouchDisplay m_singleDeckPouchDisplay;

	private List<WidgetInstance> m_deckPouchWidgets = new List<WidgetInstance>();

	private List<WidgetInstance> m_deckDescriptions = new List<WidgetInstance>();

	private List<ShopDeckPouchDisplay> m_deckPouchDisplays = new List<ShopDeckPouchDisplay>();

	private VariantStyles m_variantStyle;

	private bool m_useMultiDeckInterface;

	private bool m_isAnimating;

	private (ProductDataModel variant, RewardItemDataModel item) m_queuedCardListDataToSetAfterAnimation;

	private List<DeckCardDbfRecord> m_tmpCardList = new List<DeckCardDbfRecord>();

	private HashSet<int> m_tmpCardSet = new HashSet<int>();

	private ShopCardList m_cardList;

	private PageInfoDataModel m_pageInfoDataModel = new PageInfoDataModel();

	private IPopupRoot m_popupRoot;

	private HashSet<IPopupRendering> m_popupRenderingComponents;

	protected override void Start()
	{
		base.Start();
		m_widget.RegisterEventListener(PaginationEventListener);
		m_widget.BindDataModel(m_pageInfoDataModel);
	}

	protected override void OnProductSet()
	{
		base.OnProductSet();
		SetItems();
	}

	public void EnablePopupRendering(IPopupRoot popupRoot)
	{
		m_popupRoot = popupRoot;
	}

	public void DisablePopupRendering()
	{
		if (m_popupRoot != null)
		{
			if (m_popupRenderingComponents != null)
			{
				m_popupRoot.CleanupPopupRendering(m_popupRenderingComponents);
			}
			m_popupRoot = null;
		}
	}

	public bool HandlesChildPropagation()
	{
		return false;
	}

	public override void Open()
	{
		m_cardList = new ShopCardList(m_widget, m_scrollbar);
		base.Open();
		base.OnOpened += InitInput;
		if (m_variantStyle == VariantStyles.Class)
		{
			m_preBuyPopupInfo = new AlertPopup.PopupInfo
			{
				m_headerText = GameStrings.Get("GLUE_SELLABLE_DECK_CONFIRMATION_HEADER"),
				m_text = GameStrings.Get("GLUE_SELLABLE_DECK_CONFIRMATION_BODY"),
				m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
				m_showAlertIcon = true,
				m_alertTextAlignment = UberText.AlignmentOptions.Center,
				m_alertTextAlignmentAnchor = UberText.AnchorOptions.Middle
			};
		}
	}

	public override void Close()
	{
		base.Close();
		m_cardList.RemoveListeners();
	}

	private void InitInput(object sender, EventArgs e)
	{
		base.OnOpened -= InitInput;
		m_cardList.InitInput();
	}

	private void PaginationEventListener(string eventName)
	{
		switch (eventName)
		{
		case "PageLeft_code":
			if (m_pageInfoDataModel.PageNumber > 0 && !m_isAnimating)
			{
				SetNewPageNumber(m_pageInfoDataModel.PageNumber - 1);
				m_turnPagePlaymaker.SendEvent("PageLeft");
			}
			break;
		case "PageRight_code":
			if (m_pageInfoDataModel.PageNumber < m_pageInfoDataModel.TotalPages - 1 && !m_isAnimating)
			{
				SetNewPageNumber(m_pageInfoDataModel.PageNumber + 1);
				m_turnPagePlaymaker.SendEvent("PageRight");
			}
			break;
		case "AnimationFinished_code":
			m_isAnimating = false;
			ShowQueuedCardList();
			break;
		}
	}

	private void SetNewPageNumber(int newPageNumber)
	{
		SetTextAndPageButtonStates(newPageNumber);
		QueueCardListForPageNumber(m_productSelection.Variant, newPageNumber);
		m_isAnimating = true;
		m_pageInfoDataModel.PageNumber = newPageNumber;
	}

	protected override ProductDataModel GetFirstVariantToDisplay(ProductDataModel chosenProduct, ProductDataModel chosenVariant)
	{
		m_variantStyle = VariantStyles.NoVariant;
		ProductDataModel firstSelectedVariant = chosenVariant;
		m_useMultiDeckInterface = false;
		if (chosenProduct == null || chosenProduct.Variants == null || chosenProduct.Variants.Count == 0)
		{
			firstSelectedVariant = ProductFactory.CreateEmptyProductDataModel();
		}
		else if (chosenProduct.Variants.Count > 1)
		{
			foreach (ProductDataModel variant in chosenProduct.Variants)
			{
				if (variant.Tags.Contains("golden"))
				{
					m_variantStyle = VariantStyles.Golden;
					firstSelectedVariant = variant;
					break;
				}
				if (variant.Tags.Contains("show_class_variants"))
				{
					m_variantStyle = VariantStyles.Class;
					break;
				}
			}
			switch (m_variantStyle)
			{
			case VariantStyles.Golden:
				foreach (ProductDataModel variant2 in chosenProduct.Variants)
				{
					variant2.VariantName = (variant2.Tags.Contains("golden") ? GameStrings.Get("GLUE_STORE_PREMIUM_VARIATION_NAME_GOLDEN") : GameStrings.Get("GLUE_STORE_PREMIUM_VARIATION_NAME_NORMAL"));
				}
				break;
			case VariantStyles.Class:
				classSelectorReference?.RegisterReadyListener(delegate(ShopClassVariantSelector selector)
				{
					selector.SetProductPage(this);
					selector.SetProduct(chosenProduct);
				});
				break;
			default:
				Log.Store.PrintWarning("[SellableDeckProductPage.GetOpeningVariant] Product {0} (ID: {1}) has variants but can't find one with a golden or a show_class_variant tag!", chosenProduct.Name, chosenProduct.PmtId);
				break;
			}
		}
		int totalPages = GetNumberOfPages(firstSelectedVariant);
		ResetPageVariables(totalPages);
		m_firstVariant = firstSelectedVariant;
		return firstSelectedVariant;
	}

	private RewardItemDataModel GetDeckItemAt(ProductDataModel product, int index)
	{
		IEnumerable<RewardItemDataModel> decks = GetDecks(product);
		int current = 0;
		foreach (RewardItemDataModel deck in decks)
		{
			if (current >= index)
			{
				return deck;
			}
			current++;
		}
		return null;
	}

	public override void SelectVariant(ProductDataModel variant)
	{
		base.SelectVariant(variant);
		if (m_useMultiDeckInterface)
		{
			SetMultiDeckPouchData(variant);
			QueueCardListForPageNumber(variant, m_pageInfoDataModel.PageNumber);
			ShowQueuedCardList();
		}
		else
		{
			SellableDeckDbfRecord rewardRecord = GameDbf.SellableDeck.GetRecord(GetDeckItemAt(variant, 0).ItemId);
			m_singleDeckPouchDisplay.SetDeckPouchData(m_singleDeckPouch, rewardRecord.DeckTemplateRecord);
			PopulateCardListFromSingleDeckItem(variant, GetDeckItemAt(variant, 0));
		}
		int totalPages = GetNumberOfPages(variant);
		ResetPageVariables(totalPages);
		if (IsBattleReadyDeckBundle(variant))
		{
			m_cardListListable.RegisterDoneChangingStatesListener(OnCardListListableDoneChangingState, null, callImmediatelyIfSet: true, doOnce: true);
		}
		else
		{
			m_cardListListable.RegisterDoneChangingStatesListener(OnCardListListableDoneChangingState, null, callImmediatelyIfSet: false, doOnce: true);
		}
	}

	private void ApplyPopupRendering()
	{
		if (m_popupRoot != null)
		{
			m_popupRoot.ApplyPopupRendering(base.transform, m_popupRenderingComponents, overrideLayer: true, base.gameObject.layer);
		}
		m_turnPagePlaymaker.SendEvent("Reset");
	}

	private void SetMultiDeckPouchData(ProductDataModel variant)
	{
		int pageIndex = ((!IsBattleReadyDeckBundle(variant)) ? 1 : 0);
		int deckIndex = 0;
		foreach (RewardItemDataModel rewardItem in GetDecks(variant))
		{
			if (rewardItem.ItemType == RewardItemType.SELLABLE_DECK)
			{
				SellableDeckDbfRecord rewardRecord = GameDbf.SellableDeck.GetRecord(rewardItem.ItemId);
				if (rewardRecord != null && m_deckPouchDisplays.Count > 0 && deckIndex < m_deckPouchDisplays.Count)
				{
					m_deckPouchDisplays[deckIndex]?.SetDeckPouchData(m_deckPouchWidgets[deckIndex], rewardRecord.DeckTemplateRecord);
					BindDeckDescriptionDataModel(deckIndex);
					UpdateWidgetTransforms(deckIndex, pageIndex);
					deckIndex++;
				}
			}
			pageIndex++;
		}
		BindProductDataModelToDeckPouch(variant);
		ApplyPopupRendering();
	}

	private void OnCardListListableDoneChangingState(object _)
	{
		if (m_productSelection.Variant.Tags.Contains("golden"))
		{
			if (m_container.Variant.Items.Count == 1)
			{
				m_widget.TriggerEvent("GOLDEN_DECK_STATE");
				m_widget.TriggerEvent("GOLD_SINGLE_PACK");
			}
			else if (m_container.Variant.Items.Count > 1)
			{
				m_widget.TriggerEvent("GOLDEN_DECK_STATE");
				m_widget.TriggerEvent(IsBattleReadyDeckBundle(m_container.Variant) ? "MULTI_PACK" : "GOLD_PATH_OF_ARTHAS");
			}
		}
		else if (m_container.Variant.Items.Count == 1)
		{
			m_widget.TriggerEvent("NORMAL_DECK_STATE");
			m_widget.TriggerEvent("SINGLE_PACK");
		}
		else if (m_container.Variant.Items.Count > 1)
		{
			m_widget.TriggerEvent("NORMAL_DECK_STATE");
			m_widget.TriggerEvent(IsBattleReadyDeckBundle(m_container.Variant) ? "MULTI_PACK" : "NORMAL_PATH_OF_ARTHAS");
		}
	}

	private void QueueCardListForPageNumber(ProductDataModel variant, int pageNumber)
	{
		if (IsBattleReadyDeckBundle(variant))
		{
			m_queuedCardListDataToSetAfterAnimation = (variant: variant, item: GetDeckItemAt(variant, pageNumber));
		}
		else if (pageNumber == 0)
		{
			m_queuedCardListDataToSetAfterAnimation = (variant: variant, item: null);
		}
		else
		{
			m_queuedCardListDataToSetAfterAnimation = (variant: variant, item: GetDeckItemAt(variant, pageNumber - 1));
		}
	}

	private void ShowQueuedCardList()
	{
		if (m_queuedCardListDataToSetAfterAnimation.variant != null)
		{
			if (IsBattleReadyDeckBundle(m_queuedCardListDataToSetAfterAnimation.variant) || m_queuedCardListDataToSetAfterAnimation.item != null)
			{
				PopulateCardListFromSingleDeckItem(m_queuedCardListDataToSetAfterAnimation.variant, m_queuedCardListDataToSetAfterAnimation.item);
			}
			else
			{
				PopulateCardListWithUniqueCardsFromAllDecks(m_queuedCardListDataToSetAfterAnimation.variant);
			}
			m_queuedCardListDataToSetAfterAnimation = (variant: null, item: null);
		}
	}

	private IEnumerable<RewardItemDataModel> GetDecks(ProductDataModel product)
	{
		return from x in product.Items
			where x.ItemType == RewardItemType.SELLABLE_DECK
			orderby x.ItemId
			select x;
	}

	private void PopulateCardListWithUniqueCardsFromAllDecks(ProductDataModel variant)
	{
		CollectionManager collectionManager = CollectionManager.Get();
		BoosterDbId boosterId = BoosterDbId.INVALID;
		TAG_PREMIUM premium = (variant.Tags.Contains("golden") ? TAG_PREMIUM.GOLDEN : TAG_PREMIUM.NORMAL);
		foreach (RewardItemDataModel deck in GetDecks(variant))
		{
			SellableDeckDbfRecord rewardRecord = GameDbf.SellableDeck.GetRecord(deck.ItemId);
			if (boosterId == BoosterDbId.INVALID)
			{
				boosterId = GetBoosterId(rewardRecord);
			}
			foreach (DeckCardDbfRecord card in rewardRecord.DeckTemplateRecord.DeckRecord.Cards)
			{
				if (m_tmpCardSet.Contains(card.CardId))
				{
					continue;
				}
				string cardId = GameUtils.TranslateDbIdToCardId(card.CardId);
				CollectibleCard collectibleCard = collectionManager.GetCard(cardId, premium);
				if (collectibleCard.IsCraftable)
				{
					m_tmpCardSet.Add(card.CardId);
					int cardCount = ((collectibleCard.Rarity == TAG_RARITY.LEGENDARY) ? 1 : 2);
					for (int i = 0; i < cardCount; i++)
					{
						m_tmpCardList.Add(card);
					}
				}
			}
		}
		List<CardTileDataModel> cardList = new List<CardTileDataModel>();
		if (variant.Items.Count > 0 && variant.Items[0].ItemType == RewardItemType.CARD)
		{
			cardList.Add(new CardTileDataModel
			{
				CardId = variant.Items[0].Card.CardId,
				Count = 1,
				Premium = variant.Items[0].Card.Premium
			});
		}
		DefLoader loader = DefLoader.Get();
		IEnumerable<CardTileDataModel> cards = from cr in m_tmpCardList
			group cr by cr.CardId into g
			select (loader.GetEntityDef(g.Key), g.Count()) into ed
			orderby ed.Item1.GetRarity() descending, ed.Item1.GetCost()
			select new CardTileDataModel
			{
				CardId = ed.Item1.GetCardId(),
				Count = ed.Item2,
				Premium = premium
			};
		cardList.AddRange(cards);
		m_cardList.SetData(cardList, boosterId);
		m_scrollbar.SetScrollImmediate(0f);
		m_tmpCardList.Clear();
		m_tmpCardSet.Clear();
	}

	private void PopulateCardListFromSingleDeckItem(ProductDataModel variant, RewardItemDataModel item)
	{
		SellableDeckDbfRecord rewardRecord = GameDbf.SellableDeck.GetRecord(item.ItemId);
		BoosterDbId boosterId = GetBoosterId(rewardRecord);
		DeckDbfRecord deck = rewardRecord.DeckTemplateRecord.DeckRecord;
		TAG_PREMIUM premium = (variant.Tags.Contains("golden") ? TAG_PREMIUM.GOLDEN : TAG_PREMIUM.NORMAL);
		m_cardList.SetDataGhostNonCraftableCards(deck.Cards, boosterId, premium);
		m_scrollbar.SetScrollImmediate(0f);
	}

	private BoosterDbId GetBoosterId(SellableDeckDbfRecord rewardRecord)
	{
		BoosterDbId boosterId = BoosterDbId.INVALID;
		if (rewardRecord?.BoosterRecord != null)
		{
			int id = rewardRecord.BoosterRecord.ID;
			if (!Enum.IsDefined(typeof(BoosterDbId), id))
			{
				Log.Store.PrintWarning("[SellableDeckProductPage.GetBoosterId] The DB record {0} for product {1} (ID: {2}) uses an invalid BoosterDbId ({3})!", rewardRecord.ID, base.Product.Name, base.Product.PmtId, id);
			}
			else
			{
				boosterId = (BoosterDbId)rewardRecord.BoosterRecord.ID;
			}
		}
		return boosterId;
	}

	private void SetTextAndPageButtonStates(int pageNumber)
	{
		m_pageInfoDataModel.InfoText = GameStrings.Format("GLUE_PROGRESSION_REWARD_TRACK_PAGE_NUMBER", pageNumber + 1, m_pageInfoDataModel.TotalPages);
		m_widget.TriggerEvent((pageNumber != 0) ? "ENABLE_BUTTON_LEFT" : "DISABLE_BUTTON_LEFT");
		m_widget.TriggerEvent((pageNumber < m_pageInfoDataModel.TotalPages - 1) ? "ENABLE_BUTTON_RIGHT" : "DISABLE_BUTTON_RIGHT");
	}

	private void ResetPageVariables(int totalPages)
	{
		m_queuedCardListDataToSetAfterAnimation = (variant: null, item: null);
		m_turnPagePlaymaker.SendEvent("Reset");
		m_isAnimating = false;
		m_pageInfoDataModel.PageNumber = 0;
		m_pageInfoDataModel.TotalPages = totalPages;
		m_cardListListable.RemoveDoneChangingStatesListener(OnCardListListableDoneChangingState);
		SetTextAndPageButtonStates(0);
	}

	private void SetItems()
	{
		DataModelList<RewardItemDataModel> items = base.Product.RewardList.Items;
		if (m_useMultiDeckInterface)
		{
			int deckIndex = 0;
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i].ItemType != RewardItemType.SELLABLE_DECK)
				{
					continue;
				}
				CreateOrReactivateWidgetInstances(deckIndex, out var deckPouch2, out var _);
				deckIndex++;
				if (deckPouch2 != null)
				{
					deckPouch2.RegisterReadyListener(delegate
					{
						OnMultiDeckWidgetReady(deckPouch2);
					});
				}
			}
			for (int j = items.Count; j < m_deckPouchWidgets.Count; j++)
			{
				m_deckPouchWidgets[j].gameObject.SetActive(value: false);
				m_deckDescriptions[j].gameObject.SetActive(value: false);
			}
		}
		else if (m_singleDeckPouch == null)
		{
			m_singleDeckPouchReference.RegisterReadyListener(delegate(WidgetInstance deckPouch)
			{
				m_singleDeckPouch = deckPouch;
				m_singleDeckPouchDisplay = deckPouch.GetComponentInChildren<ShopDeckPouchDisplay>();
			});
		}
	}

	private void OnMultiDeckWidgetReady(object widget)
	{
		Widget deckPouch = widget as Widget;
		if (deckPouch != null)
		{
			ShopDeckPouchDisplay deckPouchDisplay = deckPouch.GetComponentInChildren<ShopDeckPouchDisplay>();
			m_deckPouchDisplays.Add(deckPouchDisplay);
		}
		if (m_firstVariant != null && m_deckPouchDisplays.Count == m_deckPouchWidgets.Count)
		{
			SetMultiDeckPouchData(m_firstVariant);
			m_firstVariant = null;
		}
	}

	private void CreateOrReactivateWidgetInstances(int deckPouchIndex, out WidgetInstance deckPouch, out WidgetInstance deckDescription)
	{
		if (m_deckPouchWidgets.Count > deckPouchIndex)
		{
			deckPouch = m_deckPouchWidgets[deckPouchIndex];
			deckDescription = m_deckDescriptions[deckPouchIndex];
			deckPouch.gameObject.SetActive(value: true);
			deckDescription.gameObject.SetActive(value: true);
			return;
		}
		deckPouch = WidgetInstance.Create(s_shopDeckPouch);
		if (deckPouch == null)
		{
			Log.Store.PrintError(string.Format("{0} cannot create an instance of {1}.", "SellableDeckProductPage", s_shopDeckPouch));
		}
		else
		{
			m_deckPouchWidgets.Add(deckPouch);
		}
		deckDescription = WidgetInstance.Create(s_shopDeckDescription);
		if (deckDescription == null)
		{
			Log.Store.PrintError(string.Format("{0} cannot create an instance of {1}.", "SellableDeckProductPage", s_shopDeckDescription));
		}
		else
		{
			m_deckDescriptions.Add(deckDescription);
		}
	}

	private void UpdateWidgetTransforms(int deckIndex, int index)
	{
		if (m_useMultiDeckInterface)
		{
			if (deckIndex >= 0 && deckIndex < m_deckPouchWidgets.Count)
			{
				WidgetInstance deckPouch = m_deckPouchWidgets[deckIndex];
				Vector3 startPos = (HasDeckDescription(deckPouch) ? m_deckPouchWithDescriptionPos : m_startingDeckPouchPos);
				deckPouch.transform.SetParent(m_slidingRoot.transform);
				deckPouch.transform.localPosition = startPos + new Vector3(m_deckPouchSpacing * (float)index, 0f, 0f);
				deckPouch.transform.localScale = m_deckPouchScale;
				deckPouch.transform.localRotation = m_deckPouchRotation;
			}
			if (deckIndex >= 0 && deckIndex < m_deckDescriptions.Count)
			{
				WidgetInstance widgetInstance = m_deckDescriptions[deckIndex];
				widgetInstance.transform.SetParent(m_slidingRoot.transform);
				widgetInstance.transform.localPosition = m_startingDeckDescriptionPos + new Vector3(m_deckPouchSpacing * (float)index, 0f, 0f);
				widgetInstance.transform.localScale = new Vector3(1f, 1f, 1f);
				widgetInstance.transform.localRotation = m_deckDescriptionRotation;
			}
		}
	}

	private bool HasDeckDescription(WidgetInstance deckPouch)
	{
		DeckPouchDataModel deckPouchDataModel = deckPouch.GetDataModel<DeckPouchDataModel>();
		if (deckPouchDataModel == null)
		{
			return false;
		}
		return !string.IsNullOrEmpty(deckPouchDataModel.Details.AltDescription);
	}

	private void BindDeckDescriptionDataModel(int deckIndex)
	{
		if (deckIndex < 0 || deckIndex >= m_deckPouchWidgets.Count || deckIndex >= m_deckDescriptions.Count)
		{
			Log.Store.PrintWarning("SellableDeckProductPage cannot bind deck description. " + $"Deck Index: {deckIndex}, Deck Widget Count: {m_deckPouchWidgets.Count}, " + $"Deck Description Count: {m_deckDescriptions.Count}");
			return;
		}
		WidgetInstance deckWidget = m_deckPouchWidgets[deckIndex];
		if (deckWidget == null)
		{
			Log.Store.PrintWarning("SellableDeckProductPage cannot bind deck description. Deck widget does not exist. " + $"Deck Index: {deckIndex}");
			return;
		}
		DeckPouchDataModel deckPouchDataModel = deckWidget.GetDataModel<DeckPouchDataModel>();
		if (deckPouchDataModel == null)
		{
			Log.Store.PrintWarning("SellableDeckProductPage cannot bind deck description. Deck pouch data model does not exist. " + $"Deck Index: {deckIndex}");
			return;
		}
		WidgetInstance deckDescriptionWidget = m_deckDescriptions[deckIndex];
		if (deckDescriptionWidget == null)
		{
			Log.Store.PrintWarning("SellableDeckProductPage cannot bind deck description. Deck description widget does not exist. " + $"Deck Index: {deckIndex}");
			return;
		}
		IDataModel existingDataModel = deckDescriptionWidget.GetDataModel<DeckPouchDataModel>();
		if (existingDataModel != null)
		{
			deckDescriptionWidget.UnbindDataModel(existingDataModel.DataModelId);
		}
		deckDescriptionWidget.BindDataModel(deckPouchDataModel.Details);
	}

	private void BindProductDataModelToDeckPouch(ProductDataModel variant)
	{
		if (variant == null)
		{
			return;
		}
		foreach (WidgetInstance deckPouchWidget in m_deckPouchWidgets)
		{
			deckPouchWidget.BindDataModel(variant);
		}
	}

	private static bool IsBattleReadyDeckBundle(ProductDataModel product)
	{
		if (product != null)
		{
			return !product.Tags.Contains("sellable_deck_bundle");
		}
		return false;
	}

	private int GetNumberOfPages(ProductDataModel variant)
	{
		if (variant == null)
		{
			Log.Store.PrintError("SellableDeckProductPage cannot get number of pages, product does not exist.");
			return 0;
		}
		int totalPages = GetDecks(variant).Count();
		totalPages += ((!IsBattleReadyDeckBundle(variant)) ? 1 : 0);
		m_useMultiDeckInterface = totalPages > 1;
		return totalPages;
	}
}
