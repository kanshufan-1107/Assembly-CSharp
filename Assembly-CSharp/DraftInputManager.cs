using System.Collections.Generic;
using Hearthstone;
using UnityEngine;

public class DraftInputManager : MonoBehaviour
{
	private static DraftInputManager s_instance;

	private int m_selectedIndex = -1;

	private void Awake()
	{
		s_instance = this;
	}

	private void OnDestroy()
	{
		s_instance = null;
	}

	public static DraftInputManager Get()
	{
		return s_instance;
	}

	public void Unload()
	{
	}

	public bool HandleKeyboardInput()
	{
		DraftDisplay draftDisplay = DraftDisplay.Get();
		if (draftDisplay == null)
		{
			return false;
		}
		bool inHeroSelectMode = draftDisplay.IsInHeroSelectMode();
		if (InputCollection.GetKeyUp(KeyCode.Escape) && inHeroSelectMode)
		{
			draftDisplay.DoHeroCancelAnimation();
			return true;
		}
		CollectionDeck deck = DraftManager.Get().GetDraftDeck();
		if (draftDisplay.GetDraftMode() == DraftDisplay.DraftMode.ACTIVE_DRAFT_DECK && InputCollection.GetKeyDown(KeyCode.C) && (InputCollection.GetKey(KeyCode.LeftMeta) || InputCollection.GetKey(KeyCode.LeftControl)))
		{
			ClipboardUtils.CopyToClipboard(deck.GetShareableDeck().Serialize());
			UIStatus.Get().AddInfo(GameStrings.Get("GLUE_COLLECTION_DECK_COPIED_TOAST"));
		}
		if (!HearthstoneApplication.IsInternal())
		{
			return false;
		}
		List<DraftCardVisual> draftChoices = DraftDisplay.Get().GetCardVisuals();
		if (draftChoices == null)
		{
			return false;
		}
		if (draftChoices.Count == 0)
		{
			return false;
		}
		int cardVisualIndex = -1;
		if (InputCollection.GetKeyUp(KeyCode.Alpha1))
		{
			cardVisualIndex = 0;
		}
		else if (InputCollection.GetKeyUp(KeyCode.Alpha2))
		{
			cardVisualIndex = 1;
		}
		else if (InputCollection.GetKeyUp(KeyCode.Alpha3))
		{
			cardVisualIndex = 2;
		}
		if (cardVisualIndex == -1)
		{
			return false;
		}
		if (draftChoices.Count < cardVisualIndex + 1)
		{
			return false;
		}
		if (inHeroSelectMode && m_selectedIndex == cardVisualIndex)
		{
			draftDisplay.ClickConfirmButton();
			m_selectedIndex = -1;
			return true;
		}
		DraftCardVisual cardVisual = draftChoices[cardVisualIndex];
		if (cardVisual == null)
		{
			return false;
		}
		if (inHeroSelectMode)
		{
			draftDisplay.DoHeroCancelAnimation();
		}
		m_selectedIndex = cardVisualIndex;
		cardVisual.ChooseThisCard();
		return true;
	}
}
