using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Core;
using Hearthstone.DataModels;
using PegasusShared;
using UnityEngine;

public class ZilliaxSideboardDeck : SideboardDeck
{
	public const int MAXIMUM_COSMETIC_MODULES = 1;

	public const int MAXIMUM_FUNCTIONAL_MODULES = 2;

	private CardTextBuilder m_zilliaxCardTextBuilder = CardTextBuilderFactory.Create(Assets.Card.CardTextBuilderType.ZILLIAX_DELUXE_3000);

	private readonly string[] DEFAULT_FUNCTIONAL_MODULES = new string[2] { "TOY_330t92", "TOY_330t99" };

	private const string DEFAULT_COSMETIC_MODULES = "TOY_330t5";

	public ZilliaxDeckSideboardDataModel ZilliaxDataModel { get; }

	public CardDefHandle DynamicZilliaxCardDefHandle
	{
		get
		{
			CardDefHandle cardDefHandle = new CardDefHandle();
			CollectionDeckSlot cosmeticDeckSlot = GetCosmeticModuleCollectionDeckSlot();
			EntityDef cosmeticModuleEntityDef = null;
			if (cosmeticDeckSlot != null && cosmeticDeckSlot.GetEntityDef() != null)
			{
				cosmeticModuleEntityDef = cosmeticDeckSlot.GetEntityDef();
			}
			string entityCardId = ((cosmeticModuleEntityDef != null) ? cosmeticModuleEntityDef.GetCardId() : "TOY_330");
			cardDefHandle.SetCardId(entityCardId);
			using DefLoader.DisposableCardDef entityCardDef = DefLoader.Get()?.GetCardDef(entityCardId, new CardPortraitQuality(3, ZilliaxDataModel.ZilliaxPreviewCard.Premium));
			cardDefHandle.SetCardDef(entityCardDef);
			return cardDefHandle;
		}
	}

	public EntityDef DynamicZilliaxDef { get; private set; }

	public event Action<ZilliaxSideboardDeck> OnDynamicZilliaxDefUpdated;

	public event Action<ZilliaxSideboardDeck, EntityDef> OnZilliaxSideboardModuleAdded;

	public event Action<ZilliaxSideboardDeck, EntityDef> OnZilliaxSideboardModuleRemoved;

	public event Action OnSavedZilliaxVersionLoaded;

	public ZilliaxSideboardDeck(string ownerCardId, TAG_PREMIUM premium, CollectionDeck mainDeck)
		: base(ownerCardId, premium, mainDeck)
	{
		ZilliaxDataModel = new ZilliaxDeckSideboardDataModel
		{
			FunctionalModuleCardCount = 0,
			CosmeticModuleCardCount = 0,
			IsZilliaxAlreadyCrafted = false,
			ShouldShowZilliaxPreview = false,
			FunctionalModuleMaxCount = 2,
			CosmeticModuleMaxCount = 1,
			ZilliaxPreviewCard = new CardDataModel()
		};
		base.UIDataModels.Add(ZilliaxDataModel);
		DynamicZilliaxDef = DefLoader.Get().GetEntityDef(ownerCardId).Clone();
	}

	public override bool IsValidSlot(CollectionDeckSlot slot, bool ignoreOwnership = false, bool ignoreGameplayEvent = false, bool enforceRemainingDeckRuleset = false, FormatType? formatTypeToValidateAgainst = null)
	{
		EntityDef entityDef = slot.GetEntityDef();
		if (!entityDef.HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_COSMETICMODULE))
		{
			return entityDef.HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_FUNCTIONALMODULE);
		}
		return true;
	}

	public override int GetTotalInvalidCardCount(FormatType? formatTypeToValidateAgainst = null, bool includeInvalidRuneCards = false)
	{
		int count = 0;
		foreach (CollectionDeckSlot slot in GetSlots())
		{
			EntityDef entityDef = slot.GetEntityDef();
			if (!entityDef.HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_COSMETICMODULE) && !entityDef.HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_FUNCTIONALMODULE))
			{
				count++;
			}
		}
		return count;
	}

	public override SlotStatus GetSlotStatus(CollectionDeckSlot slot)
	{
		return SlotStatus.VALID;
	}

	public override bool AddCard(string cardID, TAG_PREMIUM premium, bool allowInvalid = false, EntityDef entityDef = null, params DeckRule.RuleType[] ignoreRules)
	{
		EntityDef cardToAddEntityDef = entityDef ?? DefLoader.Get().GetEntityDef(cardID);
		bool isFunctionalModule = cardToAddEntityDef.HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_FUNCTIONALMODULE);
		bool isCosmeticModule = cardToAddEntityDef.HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_COSMETICMODULE);
		if (GameUtils.IsSavedZilliaxVersion(cardToAddEntityDef))
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_headerText = GameStrings.Get("GLUE_DECK_TRAY_SAVED_ZILLIAX_PROMPT_TITLE");
			info.m_text = GameStrings.Get("GLUE_DECK_TRAY_SAVED_ZILLIAX_PROMPT_DESC");
			info.m_iconSet = AlertPopup.PopupInfo.IconSet.Default;
			info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
			info.m_confirmText = GameStrings.Get("GLUE_COLLECTION_DECK_COMPLETE_POPUP_CONFIRM");
			info.m_cancelText = GameStrings.Get("GLUE_COLLECTION_DECK_COMPLETE_POPUP_CANCEL");
			info.m_responseCallback = delegate(AlertPopup.Response response, object userData)
			{
				if (response == AlertPopup.Response.CONFIRM)
				{
					CollectionDeckTray collectionDeckTray = CollectionDeckTray.Get();
					collectionDeckTray.GetCurrentDeckContext().RemoveAllCards();
					collectionDeckTray.AddCard(DefLoader.Get().GetEntityDef(cardToAddEntityDef.GetTag(GAME_TAG.MODULAR_ENTITY_PART_1)), premium, false, null);
					collectionDeckTray.AddCard(DefLoader.Get().GetEntityDef(cardToAddEntityDef.GetTag(GAME_TAG.MODULAR_ENTITY_PART_2)), premium, false, null);
					collectionDeckTray.AddCard(DefLoader.Get().GetEntityDef(cardToAddEntityDef.GetCardId()), premium, false, null);
					this.OnSavedZilliaxVersionLoaded?.Invoke();
				}
			};
			DialogManager.Get().ShowPopup(info);
			return false;
		}
		if (!isFunctionalModule && !isCosmeticModule)
		{
			Debug.LogWarning("ZilliaxSideboardDeck.AddCard: Failed to add card. $" + cardID + " is missing both the ZILLIAX_CUSTOMIZABLE_FUNCTIONALMODULE tag and the ZILLIAX_CUSTOMIZABLE_COSMETICMODULE tag, all cards added to a ZilliaxSideboardDeck must include one of these tags");
			return false;
		}
		List<CollectionDeckSlot> slots = GetSlots();
		int numFunctionalModules = 0;
		int numCosmeticModules = 0;
		string cosmeticCardID = "";
		foreach (CollectionDeckSlot slot in slots)
		{
			EntityDef slotEntityDef = slot.GetEntityDef();
			if (slotEntityDef.GetCardId() == cardID)
			{
				return false;
			}
			if (slotEntityDef.HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_FUNCTIONALMODULE))
			{
				numFunctionalModules += slot.Count;
			}
			if (slotEntityDef.HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_COSMETICMODULE))
			{
				cosmeticCardID = slotEntityDef.GetCardId();
				numCosmeticModules += slot.Count;
			}
		}
		if (isFunctionalModule && numFunctionalModules >= 2)
		{
			GameplayErrorManager.Get().DisplayMessage(GameStrings.Get("GLUE_ZILLIAX_SIDEBOARD_FULL"));
			return false;
		}
		if (isCosmeticModule && numCosmeticModules >= 1)
		{
			if (cardID == cosmeticCardID)
			{
				return false;
			}
			if (RemoveCard(cosmeticCardID, premium, valid: true, enforceRemainingDeckRuleset: false))
			{
				numCosmeticModules--;
			}
		}
		bool num = base.AddCard(cardID, premium, allowInvalid, entityDef, ignoreRules);
		if (num)
		{
			if (isFunctionalModule)
			{
				numFunctionalModules++;
			}
			else
			{
				numCosmeticModules++;
			}
		}
		UpdateCardCount();
		UpdateZilliaxDataModel(numFunctionalModules, numCosmeticModules);
		if (num)
		{
			Action<ZilliaxSideboardDeck, EntityDef> action = this.OnZilliaxSideboardModuleAdded;
			if (action == null)
			{
				return num;
			}
			action(this, cardToAddEntityDef);
		}
		return num;
	}

	public override bool RemoveCard(string cardID, TAG_PREMIUM premium, bool valid, bool enforceRemainingDeckRuleset)
	{
		bool num = base.RemoveCard(cardID, premium, valid, enforceRemainingDeckRuleset);
		if (num)
		{
			EntityDef cardToRemoveEntityDef = DefLoader.Get().GetEntityDef(cardID);
			bool num2 = cardToRemoveEntityDef.HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_FUNCTIONALMODULE);
			bool isCosmeticModule = cardToRemoveEntityDef.HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_COSMETICMODULE);
			if (num2)
			{
				ZilliaxDeckSideboardDataModel zilliaxDataModel = ZilliaxDataModel;
				int functionalModuleCardCount = zilliaxDataModel.FunctionalModuleCardCount - 1;
				zilliaxDataModel.FunctionalModuleCardCount = functionalModuleCardCount;
			}
			if (isCosmeticModule)
			{
				ZilliaxDeckSideboardDataModel zilliaxDataModel2 = ZilliaxDataModel;
				int functionalModuleCardCount = zilliaxDataModel2.CosmeticModuleCardCount - 1;
				zilliaxDataModel2.CosmeticModuleCardCount = functionalModuleCardCount;
			}
			UpdateZilliaxDataModel(ZilliaxDataModel.FunctionalModuleCardCount, ZilliaxDataModel.CosmeticModuleCardCount);
			Action<ZilliaxSideboardDeck, EntityDef> action = this.OnZilliaxSideboardModuleRemoved;
			if (action == null)
			{
				return num;
			}
			action(this, cardToRemoveEntityDef);
		}
		return num;
	}

	public CollectionDeckSlot GetCosmeticModuleCollectionDeckSlot()
	{
		foreach (CollectionDeckSlot slot in GetSlots())
		{
			if (slot.GetEntityDef().HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_COSMETICMODULE))
			{
				return slot;
			}
		}
		return null;
	}

	public bool IsZilliaxComplete()
	{
		if (ZilliaxDataModel.FunctionalModuleCardCount == 2)
		{
			return ZilliaxDataModel.CosmeticModuleCardCount == 1;
		}
		return false;
	}

	public override void AutoFillSideboard()
	{
		FillDefaultFunctionalModulesIfNeeded();
		FillDefaultCosmeticMoudleIfNeeded();
	}

	public override void AddCardsFrom(CollectionDeck other)
	{
		if (other == null)
		{
			return;
		}
		foreach (CollectionDeckSlot otherSlot in other.GetSlots())
		{
			foreach (TAG_PREMIUM p in Enum.GetValues(typeof(TAG_PREMIUM)))
			{
				for (int i = 0; i < otherSlot.GetCount(p); i++)
				{
					AddCard(otherSlot.CardID, base.DataModel.Premium, false, null);
				}
			}
		}
	}

	private void FillDefaultFunctionalModulesIfNeeded()
	{
		if (ZilliaxDataModel.FunctionalModuleCardCount < 2)
		{
			string[] dEFAULT_FUNCTIONAL_MODULES = DEFAULT_FUNCTIONAL_MODULES;
			foreach (string cardId in dEFAULT_FUNCTIONAL_MODULES)
			{
				AddCard(cardId, base.DataModel.Premium, false, null);
			}
		}
	}

	public void FillDefaultCosmeticMoudleIfNeeded()
	{
		if (ZilliaxDataModel.CosmeticModuleCardCount < 1)
		{
			AddCard("TOY_330t5", base.DataModel.Premium, false, null);
		}
	}

	public List<int> GetFunctionalModules()
	{
		List<CollectionDeckSlot> slots = GetSlots();
		int numSlots = slots.Count;
		List<int> cards = new List<int>(numSlots);
		for (int i = 0; i < numSlots; i++)
		{
			EntityDef currentEntityDef = slots[i].GetEntityDef();
			if (currentEntityDef != null && currentEntityDef.HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_FUNCTIONALMODULE))
			{
				int cardId = GameUtils.TranslateCardIdToDbId(slots[i].CardID);
				cards.Add(cardId);
			}
		}
		return cards;
	}

	private void UpdateZilliaxDataModel(int numFunctionalModules, int numCosmeticModules, bool updateZilliaxPreview = true)
	{
		ZilliaxDataModel.FunctionalModuleCardCount = numFunctionalModules;
		ZilliaxDataModel.CosmeticModuleCardCount = numCosmeticModules;
		ZilliaxDataModel.ShouldShowZilliaxPreview = ZilliaxDataModel.CosmeticModuleCardCount == 1;
		if (updateZilliaxPreview)
		{
			UpdateZilliaxPreview();
		}
	}

	private void UpdateZilliaxPreview()
	{
		List<EntityDef> functionalModuleEntityDefs = new List<EntityDef>(2);
		EntityDef cosmeticModuleEntityDef = null;
		foreach (CollectionDeckSlot slot in GetSlots())
		{
			EntityDef currentEntityDef = slot.GetEntityDef();
			if (currentEntityDef.HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_COSMETICMODULE))
			{
				cosmeticModuleEntityDef = currentEntityDef;
			}
			else if (currentEntityDef.HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_FUNCTIONALMODULE))
			{
				functionalModuleEntityDefs.Add(currentEntityDef);
			}
		}
		if (cosmeticModuleEntityDef == null)
		{
			ZilliaxDataModel.ZilliaxPreviewCard.CardId = "";
			ZilliaxDataModel.ZilliaxPreviewCard.Premium = TAG_PREMIUM.NORMAL;
		}
		else
		{
			ZilliaxDataModel.ZilliaxPreviewCard.CardId = cosmeticModuleEntityDef.GetCardId();
			ZilliaxDataModel.ZilliaxPreviewCard.Premium = base.DataModel.Premium;
		}
		int cost = 0;
		int attack = 0;
		int health = 0;
		foreach (EntityDef functionalModuleEntityDef in functionalModuleEntityDefs)
		{
			cost += functionalModuleEntityDef.GetCost();
			attack += functionalModuleEntityDef.GetATK();
			health += functionalModuleEntityDef.GetHealth();
		}
		ZilliaxDataModel.ZilliaxPreviewCard.Mana = cost;
		ZilliaxDataModel.ZilliaxPreviewCard.Attack = attack;
		ZilliaxDataModel.ZilliaxPreviewCard.Health = health;
		Map<GAME_TAG, int> tagMap = new Map<GAME_TAG, int>();
		tagMap[GAME_TAG.COST] = cost;
		tagMap[GAME_TAG.ATK] = attack;
		tagMap[GAME_TAG.HEALTH] = health;
		tagMap[GAME_TAG.HIDE_STATS] = 0;
		tagMap[GAME_TAG.HIDE_COST] = 0;
		tagMap[GAME_TAG.HIDE_ATTACK_NUMBER] = 0;
		tagMap[GAME_TAG.HIDE_HEALTH_NUMBER] = 0;
		tagMap[GAME_TAG.MODULAR_ENTITY_PART_1] = ((functionalModuleEntityDefs.Count > 0) ? GameUtils.TranslateCardIdToDbId(functionalModuleEntityDefs[0].GetCardId()) : 0);
		tagMap[GAME_TAG.MODULAR_ENTITY_PART_2] = ((functionalModuleEntityDefs.Count > 1) ? GameUtils.TranslateCardIdToDbId(functionalModuleEntityDefs[1].GetCardId()) : 0);
		tagMap[GAME_TAG.CLASS] = 12;
		DynamicZilliaxDef.SetTags(tagMap);
		string zilliaxText = m_zilliaxCardTextBuilder.BuildCardTextInHand(DynamicZilliaxDef);
		ZilliaxDataModel.ZilliaxPreviewCard.CardText = zilliaxText;
		this.OnDynamicZilliaxDefUpdated?.Invoke(this);
	}

	public override void InitCardCount()
	{
		base.InitCardCount();
		List<CollectionDeckSlot> slots = GetSlots();
		int numFunctionalModules = 0;
		int numCosmeticModules = 0;
		foreach (CollectionDeckSlot slot in slots)
		{
			EntityDef entityDef = slot.GetEntityDef();
			if (entityDef.HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_FUNCTIONALMODULE))
			{
				numFunctionalModules += slot.Count;
			}
			if (entityDef.HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_COSMETICMODULE))
			{
				numCosmeticModules += slot.Count;
			}
		}
		UpdateZilliaxDataModel(numFunctionalModules, numCosmeticModules, updateZilliaxPreview: false);
	}
}
