using UnityEngine;

public class CreateButton : CraftingButton
{
	private bool m_textEnlarged;

	[SerializeField]
	private UberText m_labelTextNoDustJar;

	protected override void OnRelease()
	{
		if (!Network.IsLoggedIn())
		{
			CollectionManager.ShowFeatureDisabledWhileOfflinePopup();
		}
		else
		{
			if (CraftingManager.Get().GetPendingServerTransaction() != null || CraftingManager.Get().GetShownActor() == null || CraftingManager.Get().GetShownActor().GetEntityDef() == null)
			{
				return;
			}
			Animation animation = GetComponent<Animation>();
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				animation.Play("CardExchange_ButtonPress2_phone");
			}
			else
			{
				animation.Play("CardExchange_ButtonPress2");
			}
			bool showInvalidCardConfirmation = false;
			bool isInvalidInDeck = false;
			string cardID = CraftingManager.Get().GetShownActor().GetEntityDef()
				.GetCardId();
			DeckRuleset deckRuleset = CollectionManager.Get().GetDeckRuleset();
			DeckRule brokenRule = null;
			if (deckRuleset != null)
			{
				CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
				isInvalidInDeck = !deckRuleset.Filter(DefLoader.Get().GetEntityDef(cardID), deck, out brokenRule);
				showInvalidCardConfirmation = isInvalidInDeck;
			}
			else if (!GameUtils.IsGSDFlagSet(GameSaveKeyId.COLLECTION_MANAGER, GameSaveKeySubkeyId.COLLECTION_MANAGER_SEEN_WILD_CRAFT_ALERT))
			{
				showInvalidCardConfirmation = GameUtils.IsWildCard(cardID);
			}
			if (CraftingManager.Get().GetNumClientTransactions() != 0)
			{
				showInvalidCardConfirmation = false;
			}
			if (showInvalidCardConfirmation)
			{
				TAG_CARD_SET cardSet = GameUtils.GetCardSetFromCardID(cardID);
				string formatTypeString = GameUtils.GetCardSetFormatAsString(cardSet);
				bool num = brokenRule != null && brokenRule.Type == DeckRule.RuleType.IS_CLASS_CARD_OR_NEUTRAL && cardSet == TAG_CARD_SET.ISLAND_VACATION;
				AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
				{
					m_headerText = GameStrings.Get("GLUE_CRAFTING_" + formatTypeString + "_CARD_HEADER"),
					m_cancelText = GameStrings.Get("GLUE_CRAFTING_NONSTANDARD_CARD_WARNING_CANCEL"),
					m_confirmText = GameStrings.Get("GLUE_CRAFTING_NONSTANDARD_CARD_WARNING_CONFIRM"),
					m_showAlertIcon = true,
					m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
					m_responseCallback = OnConfirmCreateResponse
				};
				if (num)
				{
					info.m_headerText = GameStrings.Get("GLUE_CRAFTING_CROSS_CLASS_CARD_HEADER");
					info.m_text = GameStrings.Get("GLUE_CRAFTING_CROSS_CLASS_CARD_DESC");
				}
				else if (SceneMgr.Get().IsInTavernBrawlMode())
				{
					info.m_headerText = GameStrings.Get("GLUE_CRAFTING_INVALID_CARD_TAVERN_BRAWL_HEADER");
					info.m_text = GameStrings.Get("GLUE_CRAFTING_INVALID_CARD_TAVERN_BRAWL_DESC");
				}
				else if (isInvalidInDeck)
				{
					info.m_text = GameStrings.Get("GLUE_CRAFTING_INVALID_CARD_DESC");
				}
				else if (CollectionManager.Get().AccountHasUnlockedWild())
				{
					info.m_text = GameStrings.Get("GLUE_CRAFTING_" + formatTypeString + "_CARD_DESC");
				}
				else
				{
					info.m_text = GameStrings.Get("GLUE_CRAFTING_" + formatTypeString + "_CARD_FIRST_DESC");
				}
				DialogManager.Get().ShowPopup(info);
			}
			else
			{
				DoCreate();
			}
		}
	}

	private void OnConfirmCreateResponse(AlertPopup.Response response, object userData)
	{
		if (response != AlertPopup.Response.CONFIRM)
		{
			return;
		}
		if (GameUtils.IsWildCard(CraftingManager.Get().GetShownActor().GetEntityDef()
			.GetCardId()))
		{
			GameUtils.SetGSDFlag(GameSaveKeyId.COLLECTION_MANAGER, GameSaveKeySubkeyId.COLLECTION_MANAGER_SEEN_WILD_CRAFT_ALERT, enableFlag: true);
			if (!CollectionManager.Get().AccountHasUnlockedWild())
			{
				AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
				{
					m_headerText = GameStrings.Get("GLUE_CRAFTING_WILD_CARD_HEADER"),
					m_text = GameStrings.Get("GLUE_CRAFTING_WILD_CARD_INTRO_DESC"),
					m_showAlertIcon = true,
					m_responseDisplay = AlertPopup.ResponseDisplay.OK,
					m_responseCallback = delegate
					{
						DoCreate();
						Options.Get().SetBool(Option.HAS_SEEN_STANDARD_MODE_TUTORIAL, val: true);
						SetRotationManager.Get().SetRotationIntroProgress();
						Options.Get().SetBool(Option.NEEDS_TO_MAKE_STANDARD_DECK, val: false);
						UserAttentionManager.StopBlocking(UserAttentionBlocker.SET_ROTATION_INTRO);
						Options.Get().SetBool(Option.SHOW_SWITCH_TO_WILD_ON_PLAY_SCREEN, val: true);
						Options.Get().SetBool(Option.SHOW_SWITCH_TO_WILD_ON_CREATE_DECK, val: true);
					}
				};
				DialogManager.Get().ShowPopup(info);
			}
			else
			{
				DoCreate();
			}
		}
		else
		{
			DoCreate();
		}
	}

	public override void EnableButton()
	{
		if (CraftingManager.Get().GetPendingClientTransaction().GetLastTransactionWasDisenchant())
		{
			EnterUndoMode();
			return;
		}
		EntityDef entityDef = CraftingManager.Get().GetShownActor().GetEntityDef();
		string cardID = entityDef.GetCardId();
		TAG_PREMIUM premium = CraftingManager.Get().GetShownActor().GetPremium();
		bool canUpgrade = CraftingManager.Get().CanUpgradeCardToGolden(cardID, premium) == CraftingManager.CanCraftCardResult.CanUpgrade;
		bool canCraft = CraftingManager.Get().CanCraftCardRightNow(entityDef, premium) == CraftingManager.CanCraftCardResult.CanCraft;
		if (canUpgrade && canCraft)
		{
			SetLabelText(GameStrings.Get("GLUE_CRAFTING_CREATE_UPGRADE"));
			SetTextEnlargedForNoDustJarOnPhone(enlarge: true);
			SetCraftingState(CraftingState.CreateUpgrade);
		}
		else if (!canUpgrade && canCraft)
		{
			SetLabelText(GameStrings.Get("GLUE_CRAFTING_CREATE"));
			SetTextEnlargedForNoDustJarOnPhone(enlarge: false);
			SetCraftingState(CraftingState.Create);
		}
		else if (canUpgrade && !canCraft)
		{
			SetLabelText(GameStrings.Get("GLUE_CRAFTING_UPGRADE"));
			SetTextEnlargedForNoDustJarOnPhone(enlarge: false);
			SetCraftingState(CraftingState.Upgrade);
		}
		base.EnableButton();
	}

	public override void EnterUndoMode()
	{
		SetTextEnlargedForNoDustJarOnPhone(enlarge: false);
		base.EnterUndoMode();
	}

	public override void SetLabelText(string text)
	{
		base.SetLabelText(text);
		if (m_labelTextNoDustJar != null)
		{
			m_labelTextNoDustJar.Text = text;
		}
	}

	private void DoCreate()
	{
		CraftingManager.Get().CreateButtonPressed();
	}

	public void SetTextEnlargedForNoDustJarOnPhone(bool enlarge)
	{
		if ((bool)UniversalInputManager.UsePhoneUI && m_labelTextNoDustJar != null)
		{
			if (enlarge && !m_textEnlarged)
			{
				m_textEnlarged = true;
				SetLabelText(labelText.Text);
				labelText.gameObject.SetActive(value: false);
				m_labelTextNoDustJar.gameObject.SetActive(value: true);
			}
			else if (!enlarge && m_textEnlarged)
			{
				m_textEnlarged = false;
				m_labelTextNoDustJar.gameObject.SetActive(value: false);
				labelText.gameObject.SetActive(value: true);
			}
		}
	}
}
