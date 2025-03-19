using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

public class DeckOptionsMenu : MonoBehaviour
{
	[SerializeField]
	[Header("Tray")]
	private GameObject m_root;

	[SerializeField]
	private GameObject m_top;

	[SerializeField]
	private GameObject m_bottom;

	[SerializeField]
	[Header("Buttons")]
	private PegUIElement m_renameButton;

	[SerializeField]
	private PegUIElement m_deleteButton;

	[SerializeField]
	private ConvertFormatButton[] m_convertFormatButtons;

	[SerializeField]
	private PegUIElement m_retireButton;

	[SerializeField]
	private DeckCopyPasteButton m_copyPasteDeckButton;

	[SerializeField]
	private PegUIElement m_deckHelperButton;

	[SerializeField]
	private HighlightState m_highlight;

	[SerializeField]
	[Header("Sound")]
	private WeakAssetReference m_convertToWildSound;

	[SerializeField]
	private WeakAssetReference m_convertToStandardSound;

	[SerializeField]
	private WeakAssetReference m_convertToTwistSound;

	[Header("Bones")]
	[SerializeField]
	private Transform m_showBone;

	[SerializeField]
	private Transform m_hideBone;

	[SerializeField]
	private Transform[] m_buttonPositions;

	[SerializeField]
	private Transform[] m_bottomPositions;

	[SerializeField]
	private float[] m_topScales;

	private int m_buttonCount;

	private bool m_shown;

	private CollectionDeck m_deck;

	private CollectionDeckInfo m_deckInfo;

	private bool m_deleteButtonAlertBeingProcessed;

	public bool IsShown => m_shown;

	public void Awake()
	{
		m_root.SetActive(value: false);
		if (m_renameButton != null)
		{
			m_renameButton.AddEventListener(UIEventType.RELEASE, OnRenameButtonReleased);
		}
		if (m_deleteButton != null)
		{
			m_deleteButton.AddEventListener(UIEventType.RELEASE, OnDeleteButtonReleased);
		}
		if (m_retireButton != null)
		{
			m_retireButton.AddEventListener(UIEventType.RELEASE, OnRetireButtonReleased);
		}
		if (m_copyPasteDeckButton != null)
		{
			m_copyPasteDeckButton.AddEventListener(UIEventType.RELEASE, OnCopyButtonReleased);
		}
		if (m_deckHelperButton != null)
		{
			m_deckHelperButton.AddEventListener(UIEventType.RELEASE, OnDeckHelperButtonReleased);
		}
		if (m_convertFormatButtons.Length != 0)
		{
			InitializeConvertFormatButtons();
		}
	}

	public void Show()
	{
		if (!m_shown)
		{
			iTween.Stop(base.gameObject);
			m_root.SetActive(value: true);
			SetSwitchFormatText(m_deck.FormatType);
			UpdateLayout();
			if (m_buttonCount == 0)
			{
				m_root.SetActive(value: false);
				return;
			}
			Hashtable tweenArgs = iTweenManager.Get().GetTweenHashTable();
			tweenArgs.Add("position", m_showBone.transform.position);
			tweenArgs.Add("time", 0.35f);
			tweenArgs.Add("easetype", iTween.EaseType.easeOutCubic);
			tweenArgs.Add("oncomplete", "FinishShow");
			tweenArgs.Add("oncompletetarget", base.gameObject);
			iTween.MoveTo(m_root, tweenArgs);
			m_shown = true;
		}
	}

	public void Hide(bool animate = true)
	{
		if (m_shown)
		{
			iTween.Stop(base.gameObject);
			if (!animate)
			{
				m_root.SetActive(value: false);
				return;
			}
			m_root.SetActive(value: true);
			Hashtable tweenArgs = iTweenManager.Get().GetTweenHashTable();
			tweenArgs.Add("position", m_hideBone.transform.position);
			tweenArgs.Add("time", 0.35f);
			tweenArgs.Add("easetype", iTween.EaseType.easeOutCubic);
			tweenArgs.Add("oncomplete", "FinishHide");
			tweenArgs.Add("oncompletetarget", base.gameObject);
			iTween.MoveTo(m_root, tweenArgs);
			m_shown = false;
		}
	}

	private void FinishHide()
	{
		if (!m_shown)
		{
			m_root.SetActive(value: false);
		}
	}

	public void SetDeck(CollectionDeck deck)
	{
		m_deck = deck;
	}

	public void SetDeckInfo(CollectionDeckInfo deckInfo)
	{
		m_deckInfo = deckInfo;
	}

	private void OnRenameButtonReleased(UIEvent e)
	{
		m_deckInfo.Hide();
		CollectionDeckTray.Get().GetDecksContent().RenameCurrentlyEditingDeck();
	}

	private void OnDeleteButtonReleased(UIEvent e)
	{
		if (m_deleteButtonAlertBeingProcessed)
		{
			Debug.LogWarning("DeckOptionsMenu:OnDeleteButtonReleased: Called while a Delete button alert was already being processed");
			return;
		}
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_COLLECTION_DELETE_CONFIRM_HEADER");
		info.m_showAlertIcon = false;
		info.m_text = GameStrings.Get("GLUE_COLLECTION_DELETE_CONFIRM_DESC");
		info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
		info.m_responseCallback = OnDeleteButtonConfirmationResponse;
		m_deckInfo.Hide();
		m_deleteButtonAlertBeingProcessed = true;
		DialogManager.Get().ShowPopup(info, OnDeleteButtonAlertPopupProcessed);
	}

	private bool OnDeleteButtonAlertPopupProcessed(DialogBase dialog, object userData)
	{
		m_deleteButtonAlertBeingProcessed = false;
		return true;
	}

	private void OnDeleteButtonConfirmationResponse(AlertPopup.Response response, object userData)
	{
		if (response != AlertPopup.Response.CANCEL)
		{
			CollectionDeckTray.Get().DeleteEditingDeck();
			CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
			if (cmd != null)
			{
				cmd.OnDoneEditingDeck();
			}
		}
	}

	private void OnRetireButtonReleased(UIEvent e)
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_showAlertIcon = false;
		info.m_headerText = GameStrings.Get("GLUE_TAVERN_BRAWL_RETIRE_CONFIRM_HEADER");
		if (TavernBrawlManager.Get().CurrentSeasonBrawlMode == TavernBrawlMode.TB_MODE_HEROIC)
		{
			info.m_text = GameStrings.Get("GLUE_TAVERN_BRAWL_RETIRE_CONFIRM_DESC");
		}
		else
		{
			info.m_text = GameStrings.Get("GLUE_BRAWLISEUM_RETIRE_CONFIRM_DESC");
		}
		info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
		info.m_responseCallback = OnRetireButtonConfirmationResponse;
		m_deckInfo.Hide();
		DialogManager.Get().ShowPopup(info);
	}

	private void OnRetireButtonConfirmationResponse(AlertPopup.Response response, object userData)
	{
		if (response != AlertPopup.Response.CANCEL)
		{
			Network.Get().TavernBrawlRetire();
		}
	}

	private void OnClosePressed(UIEvent e)
	{
		Navigation.GoBack();
	}

	private void OverOffClicker(UIEvent e)
	{
		Debug.Log("OverOffClicker");
		Hide();
	}

	private void SetupConvertDeckButtons()
	{
		List<FormatType> formatsForConvertButtons = ((m_deck == null) ? GetFormatTypesToConvert(CollectionManager.Get().GetEditedDeck().FormatType) : GetFormatTypesToConvert(m_deck.FormatType));
		if (m_convertFormatButtons.Length == formatsForConvertButtons.Count)
		{
			for (int i = 0; i < formatsForConvertButtons.Count; i++)
			{
				FormatType format = formatsForConvertButtons[i];
				ConvertFormatButton obj = m_convertFormatButtons[i];
				obj.Format = format;
				obj.EnableButton(format != FormatType.FT_TWIST || CanConvertCurrentDeckToTwist());
			}
		}
		else
		{
			Debug.LogError("Not enough buttons assigned for deck conversion to other formats");
		}
	}

	private IEnumerator SwitchFormat(FormatType format)
	{
		CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (cmd != null)
		{
			cmd.HideConvertTutorial();
		}
		m_deckInfo.Hide();
		m_highlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
		TraySection editingTraySection = CollectionDeckTray.Get().GetDecksContent().GetEditingTraySection();
		switch (m_deck.FormatType)
		{
		case FormatType.FT_STANDARD:
			editingTraySection.m_deckFX.Play("DeckTraySectionCollectionDeck_StandardGlowOut");
			break;
		case FormatType.FT_WILD:
			editingTraySection.m_deckFX.Play("DeckTraySectionCollectionDeck_WildGlowOut");
			break;
		case FormatType.FT_TWIST:
			editingTraySection.m_deckFX.Play("DeckTraySectionCollectionDeck_WildGlowOut");
			break;
		default:
			Debug.LogError("DeckOptionsMenu.SwitchFormat called switching from an invalid deck format type " + m_deck.FormatType);
			break;
		}
		SetDeckFormat(format);
		switch (format)
		{
		case FormatType.FT_STANDARD:
			if (!string.IsNullOrEmpty(m_convertToStandardSound.AssetString))
			{
				SoundManager.Get().LoadAndPlay(m_convertToStandardSound.AssetString, base.gameObject);
			}
			yield return new WaitForSeconds(0.5f);
			break;
		case FormatType.FT_WILD:
			if (!string.IsNullOrEmpty(m_convertToWildSound.AssetString))
			{
				SoundManager.Get().LoadAndPlay(m_convertToWildSound.AssetString, base.gameObject);
			}
			yield return new WaitForSeconds(0.5f);
			break;
		case FormatType.FT_TWIST:
			if (!string.IsNullOrEmpty(m_convertToTwistSound.AssetString))
			{
				SoundManager.Get().LoadAndPlay(m_convertToTwistSound.AssetString, base.gameObject);
			}
			yield return new WaitForSeconds(0.5f);
			break;
		default:
			Debug.LogError("DeckOptionsMenu.SwitchFormat called switching to an invalid deck format type " + format);
			break;
		}
		if (CollectionManager.Get().GetEditedDeck() != m_deck)
		{
			m_deck.FormatType = format;
		}
	}

	private List<FormatType> GetFormatTypesToConvert(FormatType formatType)
	{
		List<FormatType> result = new List<FormatType>();
		switch (formatType)
		{
		case FormatType.FT_WILD:
			result.Add(FormatType.FT_STANDARD);
			result.Add(FormatType.FT_TWIST);
			return result;
		case FormatType.FT_STANDARD:
			result.Add(FormatType.FT_WILD);
			result.Add(FormatType.FT_TWIST);
			return result;
		case FormatType.FT_TWIST:
			result.Add(FormatType.FT_STANDARD);
			result.Add(FormatType.FT_WILD);
			return result;
		default:
			Debug.LogError("DeckOptionsMenu.SwitchFormat called with invalid deck format type " + formatType);
			return result;
		}
	}

	private void SetDeckFormat(FormatType formatType)
	{
		CollectionDeckTray collectionDeckTray = CollectionDeckTray.Get();
		if (collectionDeckTray == null)
		{
			Debug.LogError("DeckOptionsMenu.SetDeckFormat: CollectionDeckTray.Get() returned null");
			return;
		}
		DeckTrayCardListContent deckTrayCardListContent = collectionDeckTray.GetCardsContent();
		if (deckTrayCardListContent == null)
		{
			Debug.LogError("DeckOptionsMenu.SetDeckFormat: collectionDeckTray.GetCardsContent() returned null");
			return;
		}
		CollectionManager collectionManager = CollectionManager.Get();
		if (collectionManager == null)
		{
			Debug.LogError("DeckOptionsMenu.SetDeckFormat: CollectionManager.Get() returned null");
			return;
		}
		CollectionDeckBoxVisual editingDeckBox = collectionDeckTray.GetEditingDeckBox();
		if (editingDeckBox == null)
		{
			Debug.LogError("DeckOptionsMenu.SetDeckFormat: collectionDeckTray.GetEditingDeckBox() returned null");
			return;
		}
		m_deck.FormatType = formatType;
		editingDeckBox.SetFormatType(formatType);
		collectionManager.SetDeckRuleset(DeckRuleset.GetRuleset(formatType));
		CollectibleDisplay collectibleDisplay = collectionManager.GetCollectibleDisplay();
		bool num = collectibleDisplay != null && (collectibleDisplay.GetViewMode() == CollectionUtils.ViewMode.HERO_SKINS || collectibleDisplay.GetViewMode() == CollectionUtils.ViewMode.CARD_BACKS || collectibleDisplay.GetViewMode() == CollectionUtils.ViewMode.COINS);
		if (collectibleDisplay is CollectionManagerDisplay cmd)
		{
			cmd.GetPageManager().RefreshCurrentPageContents(BookPageManager.PageTransitionType.SINGLE_PAGE_RIGHT);
			cmd.UpdateSetFilters(formatType, editingDeck: true);
			if (formatType != FormatType.FT_WILD && collectionManager.ShouldShowWildToStandardTutorial())
			{
				cmd.ShowStandardInfoTutorial(UserAttentionBlocker.SET_ROTATION_CM_TUTORIALS);
			}
		}
		if (!num)
		{
			deckTrayCardListContent.UpdateCardList();
		}
		deckTrayCardListContent.UpdateTileVisuals();
		DeckTrayCardListContent sideboardTrayContents = collectionDeckTray.GetSideboardCardsContent();
		if (sideboardTrayContents != null)
		{
			sideboardTrayContents.UpdateCardList();
			sideboardTrayContents.UpdateTileVisuals();
		}
	}

	private void SetSwitchFormatText(FormatType currentFormat)
	{
		List<FormatType> formatTypes = GetFormatTypesToConvert(currentFormat);
		Map<FormatType, string> convertGlueStringMap = new Map<FormatType, string>
		{
			{
				FormatType.FT_STANDARD,
				"GLUE_COLLECTION_TO_STANDARD"
			},
			{
				FormatType.FT_WILD,
				"GLUE_COLLECTION_TO_WILD"
			},
			{
				FormatType.FT_TWIST,
				"GLUE_COLLECTION_TO_TWIST"
			}
		};
		if (m_convertFormatButtons.Length == formatTypes.Count)
		{
			for (int i = 0; i < m_convertFormatButtons.Length; i++)
			{
				if (m_convertFormatButtons[i].ButtonText == null)
				{
					Debug.LogError("Button Uber text reference not assigned");
					continue;
				}
				if (convertGlueStringMap.TryGetValue(formatTypes[i], out var convertGlueString))
				{
					m_convertFormatButtons[i].ButtonText.Text = GameStrings.Get(convertGlueString);
					continue;
				}
				Debug.LogError("DeckOptionsMenu.SetSwitchFormatText called with unsupported next format type " + formatTypes[i]);
				m_convertFormatButtons[i].ButtonText.Text = formatTypes[i].ToString();
			}
		}
		else
		{
			Debug.LogError("Not enough buttons assigned for deck conversion buttons");
		}
	}

	private void OnDeckHelperButtonReleased(UIEvent e)
	{
		m_deckInfo.Hide();
		CollectionDeckSlot slotToReplace = CollectionDeckTray.Get().GetCurrentCardListContext().FindInvalidSlot();
		CollectionDeckTray.Get().GetCardsContent().ShowDeckHelper(slotToReplace, replaceSingleSlotOnly: false);
	}

	private void InitializeConvertFormatButtons()
	{
		ConvertFormatButton[] convertFormatButtons = m_convertFormatButtons;
		foreach (ConvertFormatButton convertButton in convertFormatButtons)
		{
			convertButton.AddEventListener(UIEventType.RELEASE, delegate(UIEvent e)
			{
				ConvertFormatButton convertFormatButton = e.GetElement() as ConvertFormatButton;
				if ((bool)convertFormatButton && (convertFormatButton.Format != FormatType.FT_TWIST || CanConvertCurrentDeckToTwist()))
				{
					StartCoroutine(SwitchFormat(convertButton.Format));
				}
			});
		}
	}

	private bool CanConvertCurrentDeckToTwist()
	{
		if (!RankMgr.IsCurrentTwistSeasonActive() || RankMgr.IsCurrentTwistSeasonUsingHeroicDecks())
		{
			return false;
		}
		if (m_deck != null)
		{
			return !RankMgr.IsClassLockedForTwist(m_deck.GetClass());
		}
		return false;
	}

	private void UpdateLayout()
	{
		int buttonCount = GetButtonCount();
		if (buttonCount != m_buttonCount)
		{
			m_buttonCount = buttonCount;
			UpdateBackground();
		}
		UpdateButtons();
	}

	private void UpdateBackground()
	{
		if (m_buttonCount != 0)
		{
			float scale = m_topScales[m_buttonCount - 1];
			m_top.transform.transform.localScale = new Vector3(1f, 1f, scale);
			m_bottom.transform.transform.position = m_bottomPositions[m_buttonCount - 1].position;
		}
	}

	private void UpdateButtons()
	{
		int index = 0;
		bool showConvert = ShowConvertButton();
		bool showRename = ShowRenameButton();
		bool showDelete = ShowDeleteButton();
		bool showCopyPaste = ShowCopyPasteDeckButton();
		bool showRetire = ShowRetireButton();
		bool showDeckHelper = ShowDeckHelperButton();
		ConvertFormatButton[] convertFormatButtons = m_convertFormatButtons;
		for (int i = 0; i < convertFormatButtons.Length; i++)
		{
			convertFormatButtons[i].gameObject.SetActive(showConvert);
		}
		if (showConvert)
		{
			if (m_deck.FormatType == FormatType.FT_WILD && m_highlight != null && CollectionManager.Get().ShouldShowWildToStandardTutorial())
			{
				m_highlight.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
			}
			convertFormatButtons = m_convertFormatButtons;
			for (int i = 0; i < convertFormatButtons.Length; i++)
			{
				convertFormatButtons[i].transform.position = m_buttonPositions[index].position;
				index++;
			}
			SetupConvertDeckButtons();
		}
		m_renameButton.gameObject.SetActive(showRename);
		if (showRename)
		{
			m_renameButton.transform.position = m_buttonPositions[index].position;
			index++;
		}
		m_copyPasteDeckButton.gameObject.SetActive(showCopyPaste);
		if (showCopyPaste)
		{
			m_copyPasteDeckButton.transform.position = m_buttonPositions[index].position;
			index++;
		}
		m_deckHelperButton.gameObject.SetActive(showDeckHelper);
		if (showDeckHelper)
		{
			m_deckHelperButton.transform.position = m_buttonPositions[index].position;
			index++;
		}
		m_deleteButton.gameObject.SetActive(showDelete);
		if (showDelete)
		{
			m_deleteButton.transform.position = m_buttonPositions[index].position;
			index++;
		}
		m_retireButton.gameObject.SetActive(showRetire);
		if (showRetire)
		{
			m_retireButton.transform.position = m_buttonPositions[index].position;
			index++;
		}
	}

	private int GetButtonCount()
	{
		return 0 + (ShowRenameButton() ? 1 : 0) + (ShowDeleteButton() ? 1 : 0) + (ShowConvertButton() ? m_convertFormatButtons.Length : 0) + (ShowCopyPasteDeckButton() ? 1 : 0) + (ShowRetireButton() ? 1 : 0) + (ShowDeckHelperButton() ? 1 : 0);
	}

	private bool ShowCopyPasteDeckButton()
	{
		if (ShowCopyDeckButton())
		{
			SetUpCopyButton();
			return true;
		}
		return false;
	}

	private void SetUpCopyButton()
	{
		m_copyPasteDeckButton.ButtonText.Text = GameStrings.Get("GLUE_COLLECTION_DECK_COPY");
		m_copyPasteDeckButton.TooltipHeaderString = GameStrings.Get("GLUE_COLLECTION_DECK_COPY_TOOLTIP_HEADLINE");
	}

	private void OnCopyButtonReleased(UIEvent e)
	{
		m_deckInfo.Hide();
		CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
		ClipboardUtils.CopyToClipboard(deck.GetShareableDeck().Serialize());
		UIStatus.Get().AddInfo(GameStrings.Get("GLUE_COLLECTION_DECK_COPIED_TOAST"));
		TelemetryManager.Client().SendDeckCopied(deck.ID, deck.GetShareableDeck().Serialize(includeComments: false));
	}

	private bool ShowCopyDeckButton()
	{
		CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
		m_copyPasteDeckButton.TooltipMessage = string.Empty;
		if (deck.GetTotalCardCount() == 0)
		{
			return false;
		}
		bool canCopy = false;
		if (RankMgr.IsTwistDeckWithNoSeason(deck))
		{
			canCopy = false;
			m_copyPasteDeckButton.TooltipMessage = GameStrings.Get("GLUE_COLLECTION_DECK_COPY_TOOLTIP_INCOMPLETE");
		}
		else
		{
			canCopy = deck.CanCopyAsShareableDeck(out var topViolation);
			m_copyPasteDeckButton.TooltipMessage = CollectionDeck.GetUserFriendlyCopyErrorMessageFromDeckRuleViolation(topViolation);
		}
		m_copyPasteDeckButton.EnableButton(canCopy);
		return true;
	}

	private bool ShowRenameButton()
	{
		CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
		if (deck != null && deck.Locked)
		{
			return false;
		}
		if (SceneMgr.Get().IsInTavernBrawlMode())
		{
			return false;
		}
		return UniversalInputManager.Get().IsTouchMode();
	}

	private bool ShowDeleteButton()
	{
		CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
		if (deck != null && deck.Locked)
		{
			return false;
		}
		if (SceneMgr.Get().IsInTavernBrawlMode())
		{
			return UniversalInputManager.UsePhoneUI;
		}
		return UniversalInputManager.Get().IsTouchMode();
	}

	private bool ShowRetireButton()
	{
		if (SceneMgr.Get().IsInTavernBrawlMode() && TavernBrawlManager.Get().IsCurrentSeasonSessionBased && TavernBrawlManager.Get().CurrentSession.DeckLocked)
		{
			TavernBrawlDisplay tavernBrawlDisplay = TavernBrawlDisplay.Get();
			if (tavernBrawlDisplay != null && !tavernBrawlDisplay.IsInDeckEditMode())
			{
				return true;
			}
		}
		return false;
	}

	private bool ShowConvertButton()
	{
		if (SceneMgr.Get().IsInTavernBrawlMode())
		{
			return false;
		}
		if (!CollectionManager.Get().ShouldAccountSeeStandardWild())
		{
			return false;
		}
		if (CollectionManager.Get().GetCollectibleDisplay().GetViewMode() == CollectionUtils.ViewMode.DECK_TEMPLATE)
		{
			return false;
		}
		return true;
	}

	private bool ShowDeckHelperButton()
	{
		CollectionDeckTray collectionDeckTray = CollectionDeckTray.Get();
		if (collectionDeckTray == null)
		{
			return false;
		}
		CollectionDeck deck = collectionDeckTray.GetCurrentDeckContext();
		if (deck != null && deck.Locked)
		{
			return false;
		}
		deck.GetCardCountRange(out var _, out var maxCards);
		if (deck.GetTotalValidCardCount(null) >= maxCards)
		{
			return false;
		}
		if (TavernBrawlDisplay.IsTavernBrawlViewing())
		{
			return false;
		}
		CollectibleDisplay collectibleDisplay = CollectionManager.Get().GetCollectibleDisplay();
		if ((object)collectibleDisplay == null || collectibleDisplay.GetViewMode() != 0)
		{
			return false;
		}
		if (!DeckHelper.HasChoicesToOffer(deck))
		{
			return false;
		}
		return true;
	}
}
