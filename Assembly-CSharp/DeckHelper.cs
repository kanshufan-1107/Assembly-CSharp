using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckHelper : MonoBehaviour
{
	public delegate void DelCompleteCallback(List<EntityDef> chosenCards);

	public UberText m_instructionText;

	public UberText m_replaceText;

	public GameObject m_rootObject;

	public UIBButton m_suggestDoneButton;

	public UIBButton m_replaceDoneButton;

	public PegUIElement m_inputBlocker;

	public Vector3 m_deckCardLocalScale = new Vector3(5.75f, 5.75f, 5.75f);

	public GameObject m_3choiceContainer;

	public GameObject m_replaceContainer;

	public GameObject m_2choiceContainer;

	public Vector3 m_cardSpacing;

	public GameObject m_suggestACardPane;

	public GameObject m_replaceACardPane;

	public string m_replaceACardSound;

	public UIBButton m_innkeeperPopup;

	private static DeckHelper s_instance;

	private Actor m_replaceCardActor;

	private List<Actor> m_choiceActors = new List<Actor>();

	private bool m_shown;

	private DeckTrayDeckTileVisual m_highlightedTile;

	private CollectionDeckSlot m_nextSlotToReplace;

	private bool m_replaceSingleSlotOnly;

	private Vector3 m_innkeeperFullScale;

	private bool m_innkeeperPopupShown;

	private const float INNKEEPER_POPUP_DURATION = 7f;

	private int m_thirdChoiceEntityId;

	private List<EntityDef> m_chosenCards = new List<EntityDef>();

	private DelCompleteCallback m_onCompleteCallback;

	private ScreenEffectsHandle m_screenEffectsHandle;

	private void Awake()
	{
		s_instance = this;
		m_rootObject.SetActive(value: false);
		m_replaceDoneButton.AddEventListener(UIEventType.RELEASE, EndButtonClick);
		m_suggestDoneButton.AddEventListener(UIEventType.RELEASE, EndButtonClick);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			if (m_innkeeperPopup != null)
			{
				m_innkeeperFullScale = m_innkeeperPopup.gameObject.transform.localScale;
				m_innkeeperPopup.AddEventListener(UIEventType.RELEASE, InnkeeperPopupClicked);
			}
		}
		else
		{
			m_inputBlocker.AddEventListener(UIEventType.RELEASE, EndButtonClick);
		}
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	private void OnDestroy()
	{
		s_instance = null;
	}

	private void EndButtonClick(UIEvent e)
	{
		Navigation.GoBack();
	}

	public static DeckHelper Get()
	{
		if (s_instance == null)
		{
			string deckHelperPrefab = (UniversalInputManager.UsePhoneUI ? "DeckHelper_phone.prefab:e2c93e38a85f44eadb1aee945b1c4636" : "DeckHelper.prefab:69e71904d55994cc28b41f5950e6608f");
			s_instance = AssetLoader.Get().InstantiatePrefab(deckHelperPrefab).GetComponent<DeckHelper>();
		}
		return s_instance;
	}

	public bool IsActive()
	{
		return m_shown;
	}

	public void OnCardAdded(CollectionDeck deck)
	{
		if (IsActive())
		{
			HandleDeckChanged(deck);
		}
	}

	public static bool HasChoicesToOffer(CollectionDeck deck)
	{
		return DeckMaker.GetFillCardChoices(deck, null, 1).m_addChoices.Count > 0;
	}

	public void UpdateChoices(CollectionDeckSlot slotToReplace)
	{
		CleanOldChoices();
		if (!IsActive())
		{
			return;
		}
		EntityDef cardToReplace = slotToReplace?.GetEntityDef();
		CollectionDeck currentDeck = CollectionManager.Get().GetEditedDeck();
		DeckMaker.DeckChoiceFill cardsToShow = DeckMaker.GetFillCardChoices(currentDeck, cardToReplace, 3);
		if (cardToReplace == null && cardsToShow.m_removeTemplate != null)
		{
			cardToReplace = cardsToShow.m_removeTemplate;
		}
		string cardReason = cardsToShow.m_reason;
		if (cardsToShow == null || cardsToShow.m_addChoices.Count == 0)
		{
			Debug.LogError("DeckHelper.GetChoices() - Can't find choices!!!!");
			return;
		}
		if (m_instructionText != null)
		{
			bool textChanged = !m_instructionText.Text.Equals(cardReason);
			m_instructionText.Text = cardReason;
			if ((bool)UniversalInputManager.UsePhoneUI && textChanged)
			{
				if (NotificationManager.Get().IsQuotePlaying)
				{
					m_instructionText.Text = "";
				}
				else
				{
					ShowInnkeeperPopup();
				}
			}
		}
		m_replaceACardPane.SetActive(slotToReplace != null);
		m_suggestACardPane.SetActive(slotToReplace == null);
		if (slotToReplace != null && cardToReplace != null)
		{
			GhostCard.Type ghostType = GhostCard.GetGhostTypeFromSlot(currentDeck, slotToReplace);
			m_replaceCardActor = LoadBestCardActor(cardToReplace, TAG_PREMIUM.NORMAL, ghostType);
			if (m_replaceCardActor != null)
			{
				GameUtils.SetParent(m_replaceCardActor, m_replaceContainer);
				if (ghostType == GhostCard.Type.MISSING)
				{
					RenderToTexture ghostRender = m_replaceCardActor.m_ghostCardGameObject.GetComponent<RenderToTexture>();
					BoxCollider boxCollider = m_replaceCardActor.m_ghostCardGameObject.AddComponent<BoxCollider>();
					boxCollider.size = new Vector3(ghostRender.m_Width, 2f, ghostRender.m_Height);
					boxCollider.gameObject.AddComponent<PegUIElement>().AddEventListener(UIEventType.RELEASE, delegate
					{
						OnGhostCardRelease(m_replaceCardActor);
					});
				}
			}
			if (m_replaceText != null)
			{
				switch (ghostType)
				{
				case GhostCard.Type.NOT_VALID:
				{
					bool hasSetReplaceText = false;
					DeckRuleset ruleset = currentDeck.GetRuleset(null);
					if (ruleset != null)
					{
						List<RuleInvalidReason> ruleInvalidReasons = new List<RuleInvalidReason>();
						List<DeckRule> brokenRules = new List<DeckRule>();
						ruleset.CanAddToDeck(cardToReplace, slotToReplace.PreferredPremium, currentDeck, out ruleInvalidReasons, out brokenRules);
						foreach (DeckRule deckRule in brokenRules)
						{
							if (deckRule.Type == DeckRule.RuleType.IS_CLASS_CARD_OR_NEUTRAL)
							{
								m_replaceText.Text = GameStrings.Get("GLUE_COLLECTION_DECK_HELPER_NOT_ALLOWED_IN_DECK");
								hasSetReplaceText = true;
								break;
							}
							if (deckRule.Type == DeckRule.RuleType.TOURIST_LIMIT)
							{
								m_replaceText.Text = GameStrings.Get("GLUE_COLLECTION_DECK_HELPER_TOURIST_LIMIT");
								hasSetReplaceText = true;
								break;
							}
						}
					}
					if (!hasSetReplaceText)
					{
						bool num = GameUtils.IsCardGameplayEventActive(cardToReplace);
						bool isGameplayEventEverActive = GameUtils.IsCardGameplayEventEverActive(cardToReplace);
						if (!num && isGameplayEventEverActive)
						{
							m_replaceText.Text = GameStrings.Get("GLUE_COLLECTION_DECK_HELPER_REPLACE_UNPLAYABLE_CARD");
						}
						else if (CollectionManager.Get().ShouldAccountSeeStandardWild() || !isGameplayEventEverActive)
						{
							m_replaceText.Text = GameStrings.Get("GLUE_COLLECTION_DECK_HELPER_REPLACE_INVALID_CARD");
						}
						else if (GameUtils.IsBanned(currentDeck, cardToReplace))
						{
							m_replaceText.Text = GameStrings.Get("GLUE_COLLECTION_DECK_HELPER_REPLACE_BANNED_CARD");
						}
						else
						{
							m_replaceText.Text = GameStrings.Get("GLUE_COLLECTION_DECK_HELPER_REPLACE_INVALID_CARD_NPR");
						}
					}
					break;
				}
				case GhostCard.Type.MISSING:
					m_replaceText.Text = GameStrings.Get("GLUE_COLLECTION_DECK_HELPER_REPLACE_UNOWNED_CARD");
					break;
				default:
					m_replaceText.Text = GameStrings.Get("GLUE_COLLECTION_DECK_HELPER_REPLACE_CARD");
					break;
				}
			}
			if (slotToReplace.Owned && !Options.Get().GetBool(Option.HAS_SEEN_DECK_TEMPLATE_GHOST_CARD, defaultVal: false))
			{
				Options.Get().SetBool(Option.HAS_SEEN_DECK_TEMPLATE_GHOST_CARD, val: true);
			}
			if (!currentDeck.IsValidSlot(slotToReplace, ignoreOwnership: false, ignoreGameplayEvent: false, enforceRemainingDeckRuleset: false, null) && !Options.Get().GetBool(Option.HAS_SEEN_INVALID_ROTATED_CARD, defaultVal: false))
			{
				Options.Get().SetBool(Option.HAS_SEEN_INVALID_ROTATED_CARD, val: true);
			}
			if (m_replaceACardSound != string.Empty)
			{
				SoundManager.Get().LoadAndPlay(m_replaceACardSound);
			}
		}
		bool num2 = cardToReplace != null;
		int replacementCards = Mathf.Min(num2 ? 2 : 3, cardsToShow.m_addChoices.Count);
		GameObject cardSuggestContainer = (num2 ? m_2choiceContainer : m_3choiceContainer);
		m_thirdChoiceEntityId = 0;
		for (int i = 0; i < replacementCards; i++)
		{
			EntityDef entityDef = cardsToShow.m_addChoices[i];
			if (i == 2)
			{
				m_thirdChoiceEntityId = entityDef.GetEntityId();
			}
			TAG_PREMIUM? premiumToUse = currentDeck.GetPreferredPremiumThatCanBeAdded(entityDef.GetCardId());
			if (!premiumToUse.HasValue)
			{
				continue;
			}
			Actor actor = LoadBestCardActor(entityDef, premiumToUse.Value);
			if (!(actor == null))
			{
				GameUtils.SetParent(actor, cardSuggestContainer);
				PegUIElement pegUIElement = actor.GetCollider().gameObject.AddComponent<PegUIElement>();
				pegUIElement.AddEventListener(UIEventType.RELEASE, delegate
				{
					OnVisualRelease(actor, cardsToShow.m_removeTemplate);
				});
				pegUIElement.AddEventListener(UIEventType.ROLLOVER, delegate
				{
					OnVisualOver(actor);
				});
				pegUIElement.AddEventListener(UIEventType.ROLLOUT, delegate
				{
					OnVisualOut(actor);
				});
				m_choiceActors.Add(actor);
			}
		}
		PositionAndShowChoices(slotToReplace);
	}

	private Actor LoadBestCardActor(EntityDef entityDef, TAG_PREMIUM premiumToUse, GhostCard.Type ghostCard = GhostCard.Type.NONE)
	{
		using DefLoader.DisposableCardDef cardDef = DefLoader.Get().GetCardDef(entityDef.GetCardId(), new CardPortraitQuality(3, premiumToUse));
		GameObject actorObj = AssetLoader.Get().InstantiatePrefab(ActorNames.GetHandActor(entityDef, premiumToUse), AssetLoadingOptions.IgnorePrefabPosition);
		if (actorObj == null)
		{
			Debug.LogWarning($"DeckHelper - FAILED to load actor \"{base.name}\"");
			return null;
		}
		Actor actor = actorObj.GetComponent<Actor>();
		if (actor == null)
		{
			Debug.LogWarning($"DeckHelper - ERROR actor \"{base.name}\" has no Actor component");
			return null;
		}
		actor.transform.parent = base.transform;
		LayerUtils.SetLayer(actor, base.gameObject.layer);
		actor.SetEntityDef(entityDef);
		actor.SetCardDef(cardDef);
		actor.SetPremium(premiumToUse);
		actor.GhostCardEffect(ghostCard, premiumToUse);
		actor.UpdateAllComponents();
		actor.Hide();
		actor.gameObject.name = cardDef.CardDef.name + "_actor";
		return actor;
	}

	private void CleanOldChoices()
	{
		foreach (Actor choiceActor in m_choiceActors)
		{
			Object.Destroy(choiceActor.gameObject);
		}
		m_choiceActors.Clear();
		if (m_replaceCardActor != null)
		{
			Object.Destroy(m_replaceCardActor.gameObject);
			m_replaceCardActor = null;
		}
	}

	private void PositionAndShowChoices(CollectionDeckSlot slotToReplace)
	{
		for (int i = 0; i < m_choiceActors.Count; i++)
		{
			Actor actor = m_choiceActors[i];
			actor.transform.localPosition = m_cardSpacing * i;
			actor.Show();
			CollectionCardVisual.ShowActorShadow(actor, show: true);
		}
		if (m_replaceCardActor != null)
		{
			m_replaceCardActor.Show();
		}
		if (m_highlightedTile != null)
		{
			m_highlightedTile.SetHighlight(highlight: false);
		}
		if (slotToReplace != null)
		{
			DeckTrayCardListContent cardsContent = CollectionDeckTray.Get().GetCurrentCardListContext();
			if (cardsContent != null)
			{
				m_highlightedTile = cardsContent.GetCardTileVisual(slotToReplace.Index);
				if (m_highlightedTile != null)
				{
					m_highlightedTile.SetHighlight(highlight: true);
				}
			}
		}
		StartCoroutine(WaitAndAnimateChoices());
	}

	private IEnumerator WaitAndAnimateChoices()
	{
		yield return new WaitForEndOfFrame();
		for (int i = 0; i < m_choiceActors.Count; i++)
		{
			if (m_choiceActors[i].isActiveAndEnabled)
			{
				m_choiceActors[i].ActivateSpellBirthState(SpellType.SUMMON_IN_FORGE);
			}
		}
		if (m_replaceCardActor != null && m_replaceContainer.activeInHierarchy)
		{
			m_replaceCardActor.ActivateSpellBirthState(SpellType.SUMMON_IN_FORGE);
		}
	}

	public void Show(CollectionDeckSlot slotToReplace, bool replaceSingleSlotOnly, DelCompleteCallback onCompleteCallback)
	{
		if (!m_shown)
		{
			Navigation.PushUnique(OnNavigateBack);
			m_shown = true;
			m_rootObject.SetActive(value: true);
			if (!Options.Get().GetBool(Option.HAS_SEEN_DECK_HELPER, defaultVal: false) && UserAttentionManager.CanShowAttentionGrabber("DeckHelper.Show:" + Option.HAS_SEEN_DECK_HELPER))
			{
				NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, GameStrings.Get("VO_ANNOUNCER_CM_HELP_DECK_50"), "VO_ANNOUNCER_CM_HELP_DECK_50.prefab:450881875d33d094e9a27f6260fb06d9");
				Options.Get().SetBool(Option.HAS_SEEN_DECK_HELPER, val: true);
			}
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignetteDesaturatePerspective;
				screenEffectParameters.Time = 0.1f;
				m_screenEffectsHandle.StartEffect(screenEffectParameters);
			}
			m_replaceSingleSlotOnly = replaceSingleSlotOnly;
			m_onCompleteCallback = onCompleteCallback;
			UpdateChoices(slotToReplace);
			NotificationManager.Get().DestroyNotificationWithText(GameStrings.Get("GLUE_COLLECTION_TUTORIAL_TEMPLATE_REPLACE_1"));
			NotificationManager.Get().DestroyNotificationWithText(GameStrings.Get("GLUE_COLLECTION_TUTORIAL_TEMPLATE_REPLACE_2"));
			NotificationManager.Get().DestroyNotificationWithText(GameStrings.Get("GLUE_COLLECTION_TUTORIAL_REPLACE_WILD_CARDS"));
			NotificationManager.Get().DestroyNotificationWithText(GameStrings.Get("GLUE_COLLECTION_TUTORIAL_REPLACE_WILD_CARDS_NPR"));
		}
	}

	private bool OnNavigateBack()
	{
		Hide(popnavigation: false);
		return true;
	}

	public void Hide(bool popnavigation = true)
	{
		if (m_shown)
		{
			if (popnavigation)
			{
				Navigation.RemoveHandler(OnNavigateBack);
			}
			m_shown = false;
			CleanOldChoices();
			m_rootObject.SetActive(value: false);
			if (m_highlightedTile != null)
			{
				m_highlightedTile.SetHighlight(highlight: false);
			}
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				m_screenEffectsHandle.StopEffect();
			}
			if (m_onCompleteCallback != null)
			{
				m_onCompleteCallback(m_chosenCards);
			}
		}
	}

	private void ShowInnkeeperPopup()
	{
		if (!(m_innkeeperPopup == null))
		{
			m_innkeeperPopup.gameObject.SetActive(value: true);
			m_innkeeperPopupShown = true;
			m_innkeeperPopup.gameObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
			iTween.ScaleTo(m_innkeeperPopup.gameObject, iTween.Hash("scale", m_innkeeperFullScale, "easetype", iTween.EaseType.easeOutElastic, "time", 1f));
			StopCoroutine("WaitThenHidePopup");
			StartCoroutine("WaitThenHidePopup");
		}
	}

	private IEnumerator WaitThenHidePopup()
	{
		yield return new WaitForSeconds(7f);
		HideInnkeeperPopup();
	}

	private void InnkeeperPopupClicked(UIEvent e)
	{
		HideInnkeeperPopup();
	}

	private void HideInnkeeperPopup()
	{
		if (!(m_innkeeperPopup == null) && m_innkeeperPopupShown)
		{
			m_innkeeperPopupShown = false;
			iTween.ScaleTo(m_innkeeperPopup.gameObject, iTween.Hash("scale", new Vector3(0.01f, 0.01f, 0.01f), "easetype", iTween.EaseType.easeInExpo, "time", 0.2f, "oncomplete", "FinishHidePopup", "oncompletetarget", base.gameObject));
		}
	}

	private void FinishHidePopup()
	{
		m_innkeeperPopup.gameObject.SetActive(value: false);
	}

	public void OnVisualRelease(Actor addCardActor, EntityDef cardToReplace)
	{
		TooltipPanelManager.Get().HideKeywordHelp();
		addCardActor.GetSpell(SpellType.DEATHREVERSE).ActivateState(SpellStateType.BIRTH);
		CollectionDeckTray cdt = CollectionDeckTray.Get();
		if (cardToReplace != null)
		{
			string replaceCardId = cardToReplace.GetCardId();
			CollectionDeckSlot slotToRemoveFrom = cdt.GetCurrentCardListContext().GetEditingDeck().FindFirstSlotByCardIdAndValidity(replaceCardId, valid: false, ignoreGameplayEvent: false, enforceRemainingDeckRuleset: true);
			if (slotToRemoveFrom != null)
			{
				if (!cdt.RemoveCard(replaceCardId, slotToRemoveFrom.UnPreferredPremium, valid: false, enforceRemainingDeckRuleset: true))
				{
					return;
				}
				if (slotToRemoveFrom.Count > 0)
				{
					m_nextSlotToReplace = slotToRemoveFrom;
				}
				else
				{
					m_nextSlotToReplace = null;
				}
			}
		}
		if (cdt.AddCard(addCardActor.GetEntityDef(), addCardActor.GetPremium(), false, addCardActor))
		{
			m_chosenCards.Add(addCardActor.GetEntityDef());
		}
	}

	private void OnGhostCardRelease(Actor addCardActor)
	{
		GhostCard ghostCard = addCardActor.m_ghostCardGameObject.GetComponent<GhostCard>();
		MeshRenderer[] componentsInChildren = ghostCard.GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			LayerUtils.SetLayer(base.gameObject, GameLayer.Default);
		}
		CraftingManager.Get().EnterCraftMode(addCardActor, delegate
		{
			if (!(addCardActor == null))
			{
				if ((bool)UniversalInputManager.UsePhoneUI)
				{
					StartCoroutine(WaitThenSetLayer(GameLayer.IgnoreFullScreenEffects));
				}
				ghostCard.ShowRenderers();
			}
		});
	}

	private IEnumerator WaitThenSetLayer(GameLayer layer)
	{
		yield return new WaitForSeconds(0.25f);
		LayerUtils.SetLayer(base.gameObject, layer);
	}

	private void OnVisualOver(Actor actor)
	{
		SoundManager.Get().LoadAndPlay("collection_manager_card_mouse_over.prefab:0d4e20bc78956bc48b5e2963ec39211c");
		actor.SetActorState(ActorStateType.CARD_MOUSE_OVER);
		int cardChoice = 0;
		EntityDef entityDef = actor.GetEntityDef();
		if (entityDef != null)
		{
			cardChoice = ((entityDef.GetEntityId() == 0) ? 3 : 0);
		}
		TooltipPanelManager.Get().UpdateKeywordHelpForDeckHelper(actor.GetEntityDef(), actor, cardChoice);
	}

	private void OnVisualOut(Actor actor)
	{
		actor.SetActorState(ActorStateType.CARD_IDLE);
		TooltipPanelManager.Get().HideKeywordHelp();
	}

	private void HandleDeckChanged(CollectionDeck deck)
	{
		if (m_replaceSingleSlotOnly && m_nextSlotToReplace == null)
		{
			Hide();
			return;
		}
		if (deck.CountCardsByStatus(null).MissingPlusInvalid == 0)
		{
			Hide();
			return;
		}
		CollectionDeckSlot slotToReplace = m_nextSlotToReplace;
		if (slotToReplace == null)
		{
			slotToReplace = deck.FindInvalidSlot(null);
		}
		UpdateChoices(slotToReplace);
	}
}
