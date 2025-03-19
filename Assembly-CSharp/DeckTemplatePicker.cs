using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.Core;
using PegasusShared;
using UnityEngine;

public class DeckTemplatePicker : MonoBehaviour
{
	public delegate void OnTemplateDeckChosen();

	public GameObject m_root;

	public GameObject m_pageHeader;

	public UberText m_pageHeaderText;

	public UIBObjectSpacing m_pickerButtonRoot;

	public DeckTemplatePickerButton m_pickerButtonTpl;

	public DeckTemplatePickerButton m_customDeckButton;

	public UberText m_deckTemplateDescription;

	public UberText m_deckTemplatePhoneName;

	public PlayButton m_chooseButton;

	public GameObject m_bottomPanel;

	public Material m_deckArtMaterial;

	public DeckTemplatePhoneTray m_phoneTray;

	public UIBButton m_phoneBackButton;

	public RuneIndicatorVisual m_runeIndicatorVisual;

	public Vector3 m_bottomPanelHideOffset = new Vector3(0f, 0f, 25f);

	public float m_bottomPanelSlideInWaitDelay = 0.25f;

	public float m_bottomPanelAnimateTime = 0.25f;

	public float m_packAnimInTime = 0.25f;

	public float m_packAnimOutTime = 0.2f;

	public Vector3 m_offscreenPackOffset;

	public Transform m_ghostCardTipBone;

	private List<DeckTemplatePickerButton> m_pickerButtons = new List<DeckTemplatePickerButton>();

	private CollectionManager.TemplateDeck m_customDeck = new CollectionManager.TemplateDeck();

	private TAG_CLASS m_currentSelectedClass;

	private FormatType m_currentSelectedFormat;

	private CollectionManager.TemplateDeck m_currentSelectedDeck;

	private List<OnTemplateDeckChosen> m_templateDeckChosenListeners = new List<OnTemplateDeckChosen>();

	private Vector3 m_origBottomPanelPos;

	private bool m_showingBottomPanel;

	private TransformProps m_customDeckInitialPosition;

	private bool m_packsShown;

	public FormatType CurrentSelectedFormat
	{
		get
		{
			return m_currentSelectedFormat;
		}
		set
		{
			m_currentSelectedFormat = value;
		}
	}

	private void Awake()
	{
		m_currentSelectedDeck = m_customDeck;
		for (int i = 0; i < 3; i++)
		{
			int idx = i;
			DeckTemplatePickerButton newSelectorBtn = (DeckTemplatePickerButton)GameUtils.Instantiate(m_pickerButtonTpl, m_pickerButtonRoot.gameObject, withRotation: true);
			Vector3 objectOffset = Vector3.zero;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				objectOffset.x = 0.75f;
			}
			m_pickerButtonRoot.AddObject(newSelectorBtn, objectOffset);
			newSelectorBtn.AddEventListener(UIEventType.RELEASE, delegate
			{
				SelectButtonWithIndex(idx);
			});
			newSelectorBtn.gameObject.SetActive(value: true);
			m_pickerButtons.Add(newSelectorBtn);
		}
		if (m_pickerButtons.Count > 0)
		{
			m_pickerButtons[0].SetIsCoreDeck(isCore: true);
		}
		m_pickerButtonRoot.UpdatePositions();
		m_pickerButtonTpl.gameObject.SetActive(value: false);
		if (m_customDeckButton != null)
		{
			m_customDeckButton.gameObject.SetActive(value: true);
			m_customDeckButton.AddEventListener(UIEventType.RELEASE, delegate
			{
				SelectCustomDeckButton();
			});
		}
		if (m_chooseButton != null)
		{
			m_chooseButton.Disable();
			m_chooseButton.AddEventListener(UIEventType.RELEASE, delegate
			{
				ChooseRecipeAndFillInCards();
			});
		}
		if (m_phoneTray != null)
		{
			m_phoneTray.m_scrollbar.SaveScroll("start");
			m_phoneTray.gameObject.SetActive(value: false);
		}
		if (m_bottomPanel != null)
		{
			m_origBottomPanelPos = m_bottomPanel.transform.localPosition;
		}
		if (m_phoneBackButton != null)
		{
			m_phoneBackButton.AddEventListener(UIEventType.RELEASE, delegate(UIEvent e)
			{
				OnBackButtonPressed(e);
			});
		}
		m_customDeckInitialPosition = TransformUtil.GetLocalTransformProps(m_customDeckButton.transform);
	}

	private void OnBackButtonPressed(UIEvent e)
	{
		Navigation.GoBack();
	}

	private IEnumerator BackOut()
	{
		CollectionManager.Get().GetCollectibleDisplay().EnableInput(enable: false);
		Navigation.RemoveHandler(CollectionDeckTray.Get().OnBackOutOfContainerContents);
		yield return StartCoroutine(ShowPacks(show: false));
		CollectionDeckTray deckTray = CollectionDeckTray.Get();
		deckTray.OnBackOutOfDeckContentsImpl(deleteDeck: true);
		while (!deckTray.m_cardsContent.HasFinishedExiting())
		{
			yield return null;
		}
		CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (cmd != null)
		{
			cmd.EnterSelectNewDeckHeroMode();
			HeroPickerDisplay heroPickerDisplay = cmd.GetHeroPickerDisplay();
			while (heroPickerDisplay != null && !heroPickerDisplay.IsShown())
			{
				yield return null;
			}
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			StartCoroutine(HideTrays());
		}
		CollectionManager.Get().GetCollectibleDisplay().EnableInput(enable: true);
	}

	public bool OnNavigateBack()
	{
		StartCoroutine(BackOut());
		return true;
	}

	public void RegisterOnTemplateDeckChosen(OnTemplateDeckChosen dlg)
	{
		m_templateDeckChosenListeners.Add(dlg);
	}

	public void UnregisterOnTemplateDeckChosen(OnTemplateDeckChosen dlg)
	{
		m_templateDeckChosenListeners.Remove(dlg);
	}

	public bool IsShowingBottomPanel()
	{
		return m_showingBottomPanel;
	}

	public bool IsShowingPacks()
	{
		return m_packsShown;
	}

	public IEnumerator Show(bool show)
	{
		CollectionDeckTray deckTray = CollectionDeckTray.Get();
		CollectionManager collectionManager = CollectionManager.Get();
		DeckTrayCardListContent cardsContent = null;
		CollectionDeck currentDeck = null;
		if (deckTray != null)
		{
			cardsContent = deckTray.GetCardsContent();
		}
		if (cardsContent == null)
		{
			yield break;
		}
		if (collectionManager != null)
		{
			currentDeck = collectionManager.GetEditedDeck();
		}
		if (show)
		{
			m_root.SetActive(value: true);
			m_showingBottomPanel = false;
			m_packsShown = false;
			m_pickerButtonRoot.UpdatePositions();
			TransformUtil.CopyLocal(m_customDeckButton.transform, m_customDeckInitialPosition);
			m_customDeckButton.GetComponentInChildren<UberText>().Text = GameStrings.Get(GameStrings.Get("GLUE_DECK_TEMPLATE_CUSTOM_DECK"));
			if (currentDeck != null)
			{
				SetupTemplateButtons(m_customDeck);
				m_chooseButton.Disable();
				if (m_deckTemplateDescription != null)
				{
					m_deckTemplateDescription.Text = GameStrings.Get("GLUE_COLLECTION_DECK_TEMPLATE_SELECT_A_DECK");
				}
				cardsContent.ResetFakeDeck();
				if (m_phoneTray != null)
				{
					m_phoneTray.m_cardsContent.ResetFakeDeck();
				}
				FillWithCustomDeck();
				if (!UniversalInputManager.UsePhoneUI)
				{
					deckTray.DisableRuneIndicatorVisualButtons();
				}
				m_currentSelectedDeck = m_customDeck;
				if (!UniversalInputManager.UsePhoneUI)
				{
					OnTrayToggled(shown: true);
				}
				Navigation.Push(OnNavigateBack);
				if (!CollectionManager.Get().ShouldShowDeckTemplatePageForClass(m_currentSelectedClass) && !UniversalInputManager.UsePhoneUI)
				{
					SelectCustomDeckButton(preselect: true);
				}
				ShowBottomPanel(show: true);
				yield return StartCoroutine(ShowPacks(show: true));
				while (deckTray == null || deckTray.GetCurrentContentType() != DeckTray.DeckContentTypes.Cards)
				{
					yield return null;
				}
			}
		}
		else if (m_root.activeSelf)
		{
			yield return StartCoroutine(ShowPacks(show: false));
			cardsContent.ResetFakeDeck();
			deckTray.EnableRuneIndicatorVisualButtons();
			ShowBottomPanel(show: true);
			m_root.SetActive(value: false);
		}
	}

	private void SetupTemplateButtons(CollectionManager.TemplateDeck refDeck)
	{
		List<CollectionManager.TemplateDeck> tplDecks = CollectionManager.Get().GetNonStarterTemplateDecks(m_currentSelectedFormat, m_currentSelectedClass);
		if (tplDecks == null)
		{
			Log.Decks.PrintWarning("SetupTemplateButtons with class {0} which had no template decks", m_currentSelectedClass);
			return;
		}
		for (int i = 0; i < m_pickerButtons.Count && i < tplDecks.Count; i++)
		{
			CollectionManager.TemplateDeck tplDeck = tplDecks[i];
			bool num = refDeck == tplDeck;
			if (num)
			{
				m_currentSelectedDeck = tplDeck;
			}
			m_pickerButtons[i].SetSelected(selected: false);
			if (num && m_deckTemplateDescription != null)
			{
				m_deckTemplateDescription.Text = tplDeck.m_description;
			}
			if (num && m_deckTemplatePhoneName != null)
			{
				m_deckTemplatePhoneName.Text = tplDeck.m_title;
			}
			m_pickerButtons[i].transform.localEulerAngles = Vector3.zero;
			m_pickerButtons[i].GetComponent<RandomTransform>().Apply();
			AnimatedLowPolyPack component = m_pickerButtons[i].GetComponent<AnimatedLowPolyPack>();
			component.Init(0, m_pickerButtons[i].transform.localPosition, m_pickerButtons[i].transform.localPosition + m_offscreenPackOffset, ignoreFullscreenEffects: false, changeActivation: false);
			component.SetFlyingLocalRotations(m_pickerButtons[i].transform.localEulerAngles, m_pickerButtons[i].transform.localEulerAngles);
		}
		if (m_customDeckButton != null)
		{
			m_customDeckButton.SetSelected(selected: false);
			m_customDeckButton.transform.localEulerAngles = Vector3.zero;
			AnimatedLowPolyPack component2 = m_customDeckButton.GetComponent<AnimatedLowPolyPack>();
			component2.Init(0, m_customDeckButton.transform.localPosition, m_customDeckButton.transform.localPosition + m_offscreenPackOffset, ignoreFullscreenEffects: false, changeActivation: false);
			component2.SetFlyingLocalRotations(m_customDeckButton.transform.localEulerAngles, m_customDeckButton.transform.localEulerAngles);
		}
	}

	public IEnumerator ShowPacks(bool show)
	{
		float delay = 0f;
		if (show)
		{
			CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
			if (cmd != null)
			{
				HeroPickerDisplay heroPickerDisplay = cmd.GetHeroPickerDisplay();
				while (heroPickerDisplay != null && !heroPickerDisplay.IsHidden())
				{
					yield return new WaitForEndOfFrame();
				}
			}
		}
		DeckTemplatePickerButton[] randomButtons = m_pickerButtons.ToArray();
		GeneralUtils.Shuffle(randomButtons);
		DeckTemplatePickerButton[] array = randomButtons;
		for (int i = 0; i < array.Length; i++)
		{
			AnimatedLowPolyPack animatedLowPolyPack = array[i].GetComponent<AnimatedLowPolyPack>();
			if (show)
			{
				animatedLowPolyPack.FlyIn(m_packAnimInTime, delay);
			}
			else
			{
				animatedLowPolyPack.FlyOut(m_packAnimOutTime, delay);
			}
			yield return new WaitForSeconds(UnityEngine.Random.Range(0.2f * m_packAnimInTime, 0.4f * m_packAnimInTime));
		}
		AnimatedLowPolyPack lowPolyPack = m_customDeckButton.GetComponent<AnimatedLowPolyPack>();
		if (show)
		{
			lowPolyPack.FlyIn(m_packAnimInTime, delay);
			yield return new WaitForSeconds(m_packAnimInTime + delay);
		}
		else
		{
			lowPolyPack.FlyOut(m_packAnimOutTime, delay);
			yield return new WaitForSeconds(m_packAnimOutTime + delay);
		}
		m_packsShown = show;
	}

	public void ShowBottomPanel(bool show)
	{
		if (!(m_bottomPanel != null))
		{
			return;
		}
		Vector3 to = m_origBottomPanelPos;
		Vector3 from = m_origBottomPanelPos;
		float delay = 0f;
		if (show)
		{
			from += m_bottomPanelHideOffset;
			delay = m_bottomPanelSlideInWaitDelay;
			m_showingBottomPanel = true;
		}
		else
		{
			to += m_bottomPanelHideOffset;
			Processor.ScheduleCallback(m_bottomPanelAnimateTime, realTime: false, delegate
			{
				m_showingBottomPanel = show;
			});
		}
		iTween.Stop(m_bottomPanel);
		m_bottomPanel.transform.localPosition = from;
		iTween.MoveTo(m_bottomPanel, iTween.Hash("position", to, "islocal", true, "time", m_bottomPanelAnimateTime, "delay", delay));
	}

	public void OnTrayToggled(bool shown)
	{
		if (shown)
		{
			StartCoroutine(ShowTutorialPopup());
		}
		else
		{
			CollectionManager.Get().GetCollectibleDisplay().SetViewMode(CollectionUtils.ViewMode.CARDS, triggerResponse: true);
		}
	}

	private IEnumerator ShowTutorialPopup()
	{
		yield return new WaitForSeconds(0.5f);
		CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (cmd != null && !Options.Get().GetBool(Option.HAS_SEEN_DECK_TEMPLATE_SCREEN, defaultVal: false) && UserAttentionManager.CanShowAttentionGrabber("DeckTemplatePicker.ShowTutorialPopup:" + Option.HAS_SEEN_DECK_TEMPLATE_SCREEN))
		{
			Transform popupPosition = cmd.m_deckTemplateTutorialWelcomeBone;
			NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, popupPosition.localPosition, GameStrings.Get("GLUE_COLLECTION_TUTORIAL_TEMPLATE_WELCOME"), "VO_INNKEEPER_Male_Dwarf_RECIPE1_01.prefab:0261ef622a5e2b945a8f89e87cbe01a7", 3f);
			Options.Get().SetBool(Option.HAS_SEEN_DECK_TEMPLATE_SCREEN, val: true);
		}
	}

	public void SetDeckFormatAndClass(FormatType deckFormat, TAG_CLASS deckClass)
	{
		m_currentSelectedFormat = deckFormat;
		m_currentSelectedClass = deckClass;
		List<CollectionManager.TemplateDeck> tplDecks = CollectionManager.Get().GetNonStarterTemplateDecks(m_currentSelectedFormat, m_currentSelectedClass);
		int tplDeckCount = tplDecks?.Count ?? 0;
		Color classColor = CollectionPageManager.ColorForClass(deckClass);
		m_pageHeaderText.Text = GameStrings.Format("GLUE_DECK_TEMPLATE_CHOOSE_DECK", GameStrings.GetClassName(deckClass));
		CollectionPageDisplay.SetPageFlavorTextures(m_pageHeader, CollectionPageDisplay.TagClassToHeaderClass(deckClass));
		for (int i = 0; i < m_pickerButtons.Count; i++)
		{
			DeckTemplatePickerButton selectorButton = m_pickerButtons[i];
			bool show = i < tplDeckCount;
			selectorButton.gameObject.SetActive(show);
			if (!show)
			{
				continue;
			}
			CollectionManager.TemplateDeck tplDeck = tplDecks[i];
			selectorButton.SetTitleText(tplDeck.m_title);
			int ownedCardCount = 0;
			int totalCardCount = 0;
			foreach (KeyValuePair<string, int> cardId in tplDeck.m_cardIds)
			{
				CollectionManager.Get().GetOwnedCardCount(cardId.Key, out var standard, out var golden, out var signature, out var diamond);
				int count = Mathf.Min(standard + golden + signature + diamond, cardId.Value);
				ownedCardCount += count;
				totalCardCount += cardId.Value;
			}
			selectorButton.SetCardCountText(ownedCardCount, totalCardCount);
			selectorButton.m_packRibbon.GetMaterial().color = classColor;
			DeckTemplateDbfRecord deckTemplateRecord = GameDbf.DeckTemplate.GetRecord(tplDeck.m_deckTemplateId);
			if (deckTemplateRecord != null && deckTemplateRecord.DisplayCardId != 0)
			{
				selectorButton.SetDeckArtByCardId(deckTemplateRecord.DisplayCardId, m_deckArtMaterial, deckTemplateRecord);
			}
			else
			{
				selectorButton.SetDeckArtByMaterialPath(tplDeck.m_displayTexture, deckTemplateRecord);
			}
		}
		if (m_customDeckButton != null)
		{
			m_customDeckButton.m_deckTexture.GetMaterial().mainTextureOffset = CollectionPageManager.s_classTextureOffsets[deckClass];
			m_customDeckButton.m_packRibbon.GetMaterial().color = classColor;
		}
	}

	private void SelectButtonWithIndex(int index)
	{
		((Action)delegate
		{
			if (m_chooseButton != null)
			{
				m_chooseButton.Enable();
			}
			List<CollectionManager.TemplateDeck> nonStarterTemplateDecks = CollectionManager.Get().GetNonStarterTemplateDecks(m_currentSelectedFormat, m_currentSelectedClass);
			CollectionManager.TemplateDeck templateDeck = m_customDeck;
			if (nonStarterTemplateDecks != null && index < nonStarterTemplateDecks.Count)
			{
				templateDeck = nonStarterTemplateDecks[index];
			}
			for (int i = 0; i < m_pickerButtons.Count; i++)
			{
				m_pickerButtons[i].SetSelected(i == index);
			}
			if (m_deckTemplateDescription != null)
			{
				m_deckTemplateDescription.Text = templateDeck.m_description;
			}
			if (m_deckTemplatePhoneName != null)
			{
				m_deckTemplatePhoneName.Text = templateDeck.m_title;
			}
			if (m_customDeckButton != null)
			{
				m_customDeckButton.SetSelected(selected: false);
			}
			m_currentSelectedDeck = templateDeck;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				SlidingTray component = m_phoneTray.GetComponent<SlidingTray>();
				if (component.TraySliderIsAnimating())
				{
					return;
				}
				m_phoneTray.gameObject.SetActive(value: true);
				component.ShowTray();
				m_phoneTray.m_scrollbar.LoadScroll("start", snap: false);
				m_phoneTray.FlashDeckTemplateHighlight();
			}
			else
			{
				CollectionDeckTray collectionDeckTray = CollectionDeckTray.Get();
				if (collectionDeckTray != null)
				{
					collectionDeckTray.FlashDeckTemplateHighlight();
				}
			}
			FillDeckWithTemplate(m_currentSelectedDeck);
			StartCoroutine(ShowTips());
		})();
	}

	public IEnumerator ShowTips()
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			while (m_phoneTray.GetComponent<SlidingTray>().TraySliderIsAnimating())
			{
				yield return null;
			}
		}
	}

	private void FillDeckWithTemplate(CollectionManager.TemplateDeck tplDeck)
	{
		CollectionDeckTray deckTray = CollectionDeckTray.Get();
		if (deckTray == null)
		{
			Log.ErrorReporter.PrintError("DeckTemplatePicker::FillDeckWithTemplate deckTray is null!");
			return;
		}
		DeckTrayCardListContent cardListContent = deckTray.GetCardsContent();
		if (cardListContent == null)
		{
			Log.ErrorReporter.PrintError("DeckTemplatePicker::FillDeckWithTemplate cardListContent is null!");
			return;
		}
		CollectionDeck currentDeck = cardListContent.GetEditingDeck();
		if (currentDeck == null)
		{
			Log.ErrorReporter.PrintError("DeckTemplatePicker::FillDeckWithTemplate currentDeck is null!");
			return;
		}
		if (tplDeck == null)
		{
			CollectionDeck customDeck = CollectionManager.Get().GetEditedDeck();
			currentDeck.CopyFrom(customDeck);
		}
		else
		{
			currentDeck.FillFromTemplateDeck(tplDeck);
		}
		deckTray.m_cardsContent.UpdateCardList();
		deckTray.m_decksContent.UpdateDeckName(null, shouldValidateDeckName: false);
		deckTray.InitializeRuneIndicatorVisual(currentDeck);
		if (m_phoneTray != null)
		{
			CollectionDeck phoneDeck = m_phoneTray.m_cardsContent.GetEditingDeck();
			if (tplDeck == null)
			{
				CollectionDeck customDeck2 = CollectionManager.Get().GetEditedDeck();
				phoneDeck.CopyFrom(customDeck2);
			}
			else
			{
				phoneDeck.FillFromTemplateDeck(tplDeck);
			}
			if (phoneDeck.HasClass(TAG_CLASS.DEATHKNIGHT))
			{
				m_runeIndicatorVisual.Show();
				m_runeIndicatorVisual.Initialize(phoneDeck, deckTray);
				m_phoneTray.m_cardsContent.SetRuneIndicatorSpacerVisible(visible: true);
				m_runeIndicatorVisual.DisableRuneButtons();
			}
			else
			{
				m_runeIndicatorVisual.Hide();
				m_phoneTray.m_cardsContent.SetRuneIndicatorSpacerVisible(visible: false);
			}
			m_phoneTray.m_cardsContent.UpdateCardList();
			LayerUtils.SetLayer(m_phoneTray, GameLayer.IgnoreFullScreenEffects);
		}
		CollectionManager cm = CollectionManager.Get();
		if (cm != null)
		{
			CollectibleDisplay collectibleDisplay = cm.GetCollectibleDisplay();
			if (collectibleDisplay != null)
			{
				CollectionPageManager collectionPageManager = collectibleDisplay.GetPageManager() as CollectionPageManager;
				if (collectionPageManager != null)
				{
					collectionPageManager.UpdateFiltersForDeck(currentDeck, currentDeck.GetClasses(), skipPageTurn: true);
				}
			}
		}
		deckTray.UpdateRuneIndicatorVisual(currentDeck);
	}

	private void FillWithCustomDeck()
	{
		FillDeckWithTemplate(null);
	}

	private void FireOnTemplateDeckChosenEvent()
	{
		OnTemplateDeckChosen[] arr = m_templateDeckChosenListeners.ToArray();
		for (int i = 0; i < arr.Length; i++)
		{
			arr[i]();
		}
	}

	private IEnumerator HideTrays()
	{
		SlidingTray phoneTray = m_phoneTray.GetComponent<SlidingTray>();
		phoneTray.HideTray();
		while (phoneTray.isActiveAndEnabled && !phoneTray.IsTrayInShownPosition())
		{
			yield return new WaitForEndOfFrame();
		}
		GetComponent<SlidingTray>().HideTray();
	}

	private void ChooseRecipeAndFillInCards()
	{
		CollectionDeckTray deckTray = CollectionDeckTray.Get();
		if (deckTray == null)
		{
			Log.ErrorReporter.PrintError("DeckTemplatePicker::ChooseRecipeAndFillInCards deckTray is null!");
			return;
		}
		DeckTrayCardListContent cardListContent = deckTray.GetCardsContent();
		if (cardListContent == null)
		{
			Log.ErrorReporter.PrintError("DeckTemplatePicker::ChooseRecipeAndFillInCards cardListContent is null!");
			return;
		}
		CollectionManager collectionManager = CollectionManager.Get();
		if (collectionManager == null)
		{
			Log.ErrorReporter.PrintError("DeckTemplatePicker::ChooseRecipeAndFillInCards collectionManager is null!");
			return;
		}
		cardListContent.CommitFakeDeckChanges();
		collectionManager.SetShowDeckTemplatePageForClass(m_currentSelectedClass, m_currentSelectedDeck != m_customDeck);
		FireOnTemplateDeckChosenEvent();
		CollectionDeck editingDeck = collectionManager.GetEditedDeck();
		deckTray.InitializeRuneIndicatorVisual(editingDeck);
		if (m_currentSelectedDeck != m_customDeck)
		{
			editingDeck.SourceType = DeckSourceType.DECK_SOURCE_TYPE_TEMPLATE;
			Network.Get().SetDeckTemplateSource(editingDeck.ID, m_currentSelectedDeck.m_id);
		}
		Navigation.RemoveHandler(OnNavigateBack);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			StartCoroutine(EnterDeckPhone());
		}
		CollectionManagerDisplay cmd = collectionManager.GetCollectibleDisplay() as CollectionManagerDisplay;
		if (cmd != null)
		{
			(cmd.GetPageManager() as CollectionPageManager).UpdateFiltersForDeck(editingDeck, editingDeck.GetClasses(), skipPageTurn: false);
			if (collectionManager.ShouldShowWildToStandardTutorial() && editingDeck.FormatType == FormatType.FT_STANDARD)
			{
				cmd.ShowStandardInfoTutorial(UserAttentionBlocker.SET_ROTATION_CM_TUTORIALS);
			}
		}
		if (editingDeck.HasClass(TAG_CLASS.DEATHKNIGHT))
		{
			TutorialDeathKnightDeckBuilding.ShowTutorial(UIVoiceLinesManager.TriggerType.STARTED_EDITING_DEATH_KNIGHT_DECK);
		}
	}

	private void SelectCustomDeckButton(bool preselect = false)
	{
		CollectionDeckTray cdt = CollectionDeckTray.Get();
		if (cdt != null && !preselect)
		{
			cdt.FlashDeckTemplateHighlight();
		}
		if (m_chooseButton != null)
		{
			m_chooseButton.Enable();
		}
		for (int i = 0; i < m_pickerButtons.Count; i++)
		{
			m_pickerButtons[i].SetSelected(selected: false);
		}
		if (m_customDeckButton != null)
		{
			m_customDeckButton.SetSelected(selected: true);
		}
		if (m_deckTemplateDescription != null)
		{
			m_deckTemplateDescription.Text = GameStrings.Get("GLUE_DECK_TEMPLATE_CUSTOM_DECK_DESCRIPTION");
		}
		FillWithCustomDeck();
		m_currentSelectedDeck = m_customDeck;
		if ((bool)UniversalInputManager.UsePhoneUI && !preselect)
		{
			ChooseRecipeAndFillInCards();
		}
	}

	public IEnumerator EnterDeckPhone()
	{
		yield return StartCoroutine(ShowPacks(show: false));
		yield return StartCoroutine(HideTrays());
	}
}
