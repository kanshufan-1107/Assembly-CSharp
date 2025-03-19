using System;
using System.Collections;
using System.Collections.Generic;
using PegasusShared;
using UnityEngine;

public class RuneIndicatorVisual : MonoBehaviour
{
	public RuneButton[] runeButtons;

	public RuneButton draggableButton;

	public Transform draggedCardsContainer;

	public int maxDraggedCardsToShow = 5;

	public float tooltipScale = 0.085f;

	public float tooltipDelay = 1f;

	public Transform cardCountContainer;

	public UberText cardCountText;

	public Vector3 draggedTileOffset = new Vector3(0f, 0f, 0.42f);

	private RuneButton m_currentDraggedButton;

	private CollectionDeck m_currentDeck;

	private CollectionDeckTray m_deckTray;

	private Coroutine m_showTooltipCoroutine;

	private List<string> m_cardsToRemove = new List<string>();

	private readonly List<DeckTrayDeckTileVisual> m_draggedTiles = new List<DeckTrayDeckTileVisual>();

	private const int INITIAL_DECK_TILE_POOL_SIZE = 5;

	private Stack<DeckTrayDeckTileVisual> m_deckTilePool = new Stack<DeckTrayDeckTileVisual>();

	public static event Action<RunePattern> RunePatternChanged;

	private void OnEnable()
	{
		runeButtons[0].AddEventListener(UIEventType.RELEASE, OnRuneButtonClicked);
		runeButtons[1].AddEventListener(UIEventType.RELEASE, OnRuneButtonClicked);
		runeButtons[2].AddEventListener(UIEventType.RELEASE, OnRuneButtonClicked);
		runeButtons[0].AddEventListener(UIEventType.DRAG, OnRuneButtonDragged);
		runeButtons[1].AddEventListener(UIEventType.DRAG, OnRuneButtonDragged);
		runeButtons[2].AddEventListener(UIEventType.DRAG, OnRuneButtonDragged);
		runeButtons[0].AddEventListener(UIEventType.ROLLOVER, OnRuneButtonOver);
		runeButtons[1].AddEventListener(UIEventType.ROLLOVER, OnRuneButtonOver);
		runeButtons[2].AddEventListener(UIEventType.ROLLOVER, OnRuneButtonOver);
		runeButtons[0].AddEventListener(UIEventType.ROLLOUT, OnRuneButtonOut);
		runeButtons[1].AddEventListener(UIEventType.ROLLOUT, OnRuneButtonOut);
		runeButtons[2].AddEventListener(UIEventType.ROLLOUT, OnRuneButtonOut);
		CollectionDeckTray.DeckTrayRunesAdded += OnDeckTrayRunesAdded;
	}

	private void OnDisable()
	{
		runeButtons[0].RemoveEventListener(UIEventType.RELEASE, OnRuneButtonClicked);
		runeButtons[1].RemoveEventListener(UIEventType.RELEASE, OnRuneButtonClicked);
		runeButtons[2].RemoveEventListener(UIEventType.RELEASE, OnRuneButtonClicked);
		runeButtons[0].RemoveEventListener(UIEventType.DRAG, OnRuneButtonDragged);
		runeButtons[1].RemoveEventListener(UIEventType.DRAG, OnRuneButtonDragged);
		runeButtons[2].RemoveEventListener(UIEventType.DRAG, OnRuneButtonDragged);
		runeButtons[0].RemoveEventListener(UIEventType.ROLLOVER, OnRuneButtonOver);
		runeButtons[1].RemoveEventListener(UIEventType.ROLLOVER, OnRuneButtonOver);
		runeButtons[2].RemoveEventListener(UIEventType.ROLLOVER, OnRuneButtonOver);
		runeButtons[0].RemoveEventListener(UIEventType.ROLLOUT, OnRuneButtonOut);
		runeButtons[1].RemoveEventListener(UIEventType.ROLLOUT, OnRuneButtonOut);
		runeButtons[2].RemoveEventListener(UIEventType.ROLLOUT, OnRuneButtonOut);
		CollectionDeckTray.DeckTrayRunesAdded -= OnDeckTrayRunesAdded;
	}

	public void Initialize(CollectionDeck deck, CollectionDeckTray deckTray)
	{
		m_currentDeck = deck;
		m_deckTray = deckTray;
		for (int i = 0; i < runeButtons.Length; i++)
		{
			runeButtons[i].Initialize(i, deck.GetRuneAtIndex(i));
		}
	}

	public void InitializeWithTilePool(CollectionDeck deck, CollectionDeckTray deckTray)
	{
		Initialize(deck, deckTray);
		m_deckTilePool.Clear();
		for (int i = 0; i < 5; i++)
		{
			DeckTrayDeckTileVisual tileClone = m_deckTray.GetCardsContent().CreateCardTileVisual("tileClone", draggedCardsContainer);
			tileClone.Hide();
			m_deckTilePool.Push(tileClone);
		}
	}

	private void Update()
	{
		RaycastHit hit;
		if (InputCollection.GetMouseButtonUp(0))
		{
			DropButton();
		}
		else if ((bool)m_currentDraggedButton && UniversalInputManager.Get().GetInputHitInfo(GameLayer.DragPlane.LayerBit(), out hit))
		{
			Vector3 newPos = hit.point;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				newPos.y += InputMgr.PHONE_HEIGHT_OFFSET;
			}
			draggableButton.transform.position = newPos;
		}
	}

	private void OnDeckTrayRunesAdded(CollectionDeck deck, RunePattern cardRunesAdded)
	{
		RunePattern buttonRunes = GetRunesFromButtons();
		RuneType[] validRuneTypes = RunePattern.ValidRuneTypes;
		foreach (RuneType runeType in validRuneTypes)
		{
			int cardRuneCost = cardRunesAdded.GetCost(runeType);
			if (cardRuneCost == 0)
			{
				continue;
			}
			int runeDelta = cardRuneCost - buttonRunes.GetCost(runeType);
			if (runeDelta <= 0)
			{
				continue;
			}
			int runesActuallyAdded = 0;
			RuneButton[] array = runeButtons;
			foreach (RuneButton button in array)
			{
				if (button.RuneType == RuneType.RT_NONE && runesActuallyAdded < runeDelta)
				{
					button.SetRune(runeType, animate: true);
					runesActuallyAdded++;
				}
			}
		}
		foreach (KeyValuePair<string, SideboardDeck> allSideboard in deck.GetAllSideboards())
		{
			allSideboard.Value.UpdateInvalidCardsData();
		}
		if (cardRunesAdded.HasMaxAmountOfOneRuneType)
		{
			TutorialDeathKnightDeckBuilding.ShowTutorial(UIVoiceLinesManager.TriggerType.ADDED_TRIPLE_DEATH_KNIGHT_RUNES);
		}
	}

	private void OnRuneButtonClicked(UIEvent e)
	{
		RuneButton clickedButton = e.GetElement() as RuneButton;
		if (!clickedButton)
		{
			return;
		}
		clickedButton.ShowNextRune();
		m_currentDeck.SetRuneAtIndex(clickedButton.ButtonIndex, clickedButton.RuneType);
		clickedButton.SetHighlighted(highlighted: true);
		RuneIndicatorVisual.RunePatternChanged?.Invoke(m_currentDeck.Runes);
		foreach (KeyValuePair<string, SideboardDeck> allSideboard in m_currentDeck.GetAllSideboards())
		{
			allSideboard.Value.UpdateInvalidCardsData();
		}
		HideTooltip(clickedButton);
	}

	private void OnRuneButtonOver(UIEvent e)
	{
		RuneButton button = e.GetElement() as RuneButton;
		if ((bool)button)
		{
			ShowTooltip(button);
			button.SetHighlighted(highlighted: true);
		}
	}

	private void OnRuneButtonOut(UIEvent e)
	{
		RuneButton draggedButton = e.GetElement() as RuneButton;
		if ((bool)draggedButton)
		{
			draggedButton.SetHighlighted(highlighted: false);
			HideTooltip(draggedButton);
		}
	}

	private void OnRuneButtonDragged(UIEvent e)
	{
		RuneButton draggedButton = e.GetElement() as RuneButton;
		if (!draggedButton || draggedButton.RuneType == RuneType.RT_NONE || (bool)m_currentDraggedButton)
		{
			return;
		}
		int runeCost = m_currentDeck.Runes.GetCost(draggedButton.RuneType);
		List<EntityDef> cardsRemainingInDeck = new List<EntityDef>();
		m_cardsToRemove = m_deckTray.GetCardsContent().GetCardIdsMatchingOrAboveRuneCost(draggedButton.RuneType, runeCost, cardsRemainingInDeck);
		bool shouldShowCardCount = UniversalInputManager.UsePhoneUI;
		int cardCount = 0;
		if (m_cardsToRemove.Count > 0)
		{
			m_draggedTiles.Clear();
			for (int i = 0; i < m_cardsToRemove.Count; i++)
			{
				string cardId = m_cardsToRemove[i];
				DeckTrayDeckTileVisual tileVisual = m_deckTray.GetCardsContent().GetCardTileVisual(cardId);
				cardCount += tileVisual.GetSlot().Count;
				if (shouldShowCardCount && i >= maxDraggedCardsToShow)
				{
					continue;
				}
				StartCoroutine(CreateDraggableDeckTile(m_cardsToRemove[i], i, delegate(DeckTrayDeckTileVisual tile)
				{
					if (tile != null)
					{
						m_draggedTiles.Add(tile);
					}
				}));
			}
		}
		GrabButton(draggedButton);
		UpdateDraggedCardCountText(shouldShowCardCount, cardCount);
		HideTooltip(draggedButton);
	}

	private void ShowTooltip(RuneButton button)
	{
		if (m_showTooltipCoroutine != null)
		{
			StopCoroutine(m_showTooltipCoroutine);
		}
		TooltipZone tooltip = button.gameObject.GetComponent<TooltipZone>();
		if (tooltip != null)
		{
			m_showTooltipCoroutine = StartCoroutine(ShowRuneTooltip(tooltip));
		}
	}

	private void HideTooltip(RuneButton button)
	{
		if (m_showTooltipCoroutine != null)
		{
			StopCoroutine(m_showTooltipCoroutine);
		}
		TooltipZone tooltipZone = button.gameObject.GetComponent<TooltipZone>();
		if (tooltipZone != null)
		{
			tooltipZone.HideTooltip();
		}
	}

	private IEnumerator ShowRuneTooltip(TooltipZone tooltip)
	{
		yield return new WaitForSeconds(tooltipDelay);
		string headline = GameStrings.Get("GLUE_COLLECTION_RUNES_TOOLTIP_HEADER");
		string description = GameStrings.Get("GLUE_COLLECTION_RUNES_TOOLTIP_DESC");
		tooltip.ShowBoxTooltip(headline, description);
		tooltip.Scale = tooltipScale;
	}

	private IEnumerator CreateDraggableDeckTile(string cardId, int index, Action<DeckTrayDeckTileVisual> callback)
	{
		DeckTrayDeckTileVisual tileVisual = m_deckTray.GetCardsContent().GetCardTileVisual(cardId);
		tileVisual.SetPendingRemoval(pendingRemoval: true);
		DeckTrayDeckTileVisual tileClone = GetDraggableClone(tileVisual, index);
		tileClone.Show();
		callback(tileClone);
		yield return new WaitUntil(() => tileClone.isActiveAndEnabled);
		tileClone.GetActor().GetSpell(SpellType.SUMMON_IN).ActivateState(SpellStateType.BIRTH);
	}

	private DeckTrayDeckTileVisual GetDraggableClone(DeckTrayDeckTileVisual tileVisual, int index)
	{
		if (m_deckTilePool.Count > 0)
		{
			DeckTrayDeckTileVisual tileClone = m_deckTilePool.Pop();
			InitializeDraggableClone(tileClone, tileVisual, index);
			return tileClone;
		}
		return CreateDraggableClone(tileVisual, index);
	}

	private void UpdateDraggedCardCountText(bool shouldShowCardCount, int cardCount)
	{
		if (shouldShowCardCount)
		{
			if ((bool)cardCountContainer)
			{
				cardCountContainer.gameObject.SetActive(cardCount > 1);
			}
			if ((bool)cardCountText)
			{
				cardCountText.Text = cardCount.ToString();
			}
		}
	}

	private void GrabButton(RuneButton runeButton)
	{
		m_currentDraggedButton = runeButton;
		draggableButton.gameObject.SetActive(value: true);
		draggableButton.SetRune(runeButton.RuneType, animate: false);
		draggableButton.PlayDragEffect();
		m_currentDeck.SetRuneAtIndex(runeButton.ButtonIndex, RuneType.RT_NONE);
		runeButton.SetRune(RuneType.RT_NONE, animate: false);
		RuneIndicatorVisual.RunePatternChanged?.Invoke(m_currentDeck.Runes);
	}

	private void DropButton()
	{
		if (!m_currentDraggedButton)
		{
			return;
		}
		bool isMouseOverDeck = m_deckTray.MouseIsOver(Box.Get().GetCamera());
		if (isMouseOverDeck)
		{
			m_currentDraggedButton.SetRune(draggableButton.RuneType, animate: true);
			m_currentDeck.SetRuneAtIndex(m_currentDraggedButton.ButtonIndex, m_currentDraggedButton.RuneType);
			RuneIndicatorVisual.RunePatternChanged?.Invoke(m_currentDeck.Runes);
		}
		else
		{
			if (m_currentDeck.Runes.CombinedValue == DeckRule_DeathKnightRuneLimit.MaxRuneSlots - 1)
			{
				TutorialDeathKnightDeckBuilding.ShowTutorial(UIVoiceLinesManager.TriggerType.REMOVED_THIRD_RUNE);
			}
			m_currentDraggedButton.SetRune(RuneType.RT_NONE, animate: true);
			m_currentDeck.SetRuneAtIndex(m_currentDraggedButton.ButtonIndex, RuneType.RT_NONE);
			RuneIndicatorVisual.RunePatternChanged?.Invoke(m_currentDeck.Runes);
		}
		DeckTrayCardListContent cardsContent = m_deckTray.GetCardsContent();
		foreach (string cardId in m_cardsToRemove)
		{
			DeckTrayDeckTileVisual cardTile = cardsContent.GetCardTileVisual(cardId);
			cardTile.SetPendingRemoval(pendingRemoval: false);
		}
		foreach (string cardId2 in m_cardsToRemove)
		{
			DeckTrayDeckTileVisual cardTile = cardsContent.GetCardTileVisual(cardId2);
			if (!isMouseOverDeck)
			{
				m_deckTray.RemoveAllCopiesOfCard(cardTile.GetCardID());
			}
		}
		m_cardsToRemove.Clear();
		m_currentDraggedButton = null;
		draggableButton.StopDragEffect();
		draggableButton.gameObject.SetActive(value: false);
		if (m_draggedTiles.Count > 0)
		{
			for (int i = 0; i < m_draggedTiles.Count; i++)
			{
				DeckTrayDeckTileVisual draggableTile = m_draggedTiles[i];
				draggableTile.Hide();
				m_deckTilePool.Push(draggableTile);
			}
		}
		m_draggedTiles.Clear();
	}

	private RunePattern GetRunesFromButtons()
	{
		RunePattern runes = default(RunePattern);
		RuneButton[] array = runeButtons;
		foreach (RuneButton runeButton in array)
		{
			runes.AddRunes(runeButton.RuneType, 1);
		}
		return runes;
	}

	private DeckTrayDeckTileVisual CreateDraggableClone(DeckTrayDeckTileVisual tileVisual, int index)
	{
		DeckTrayDeckTileVisual tileClone = m_deckTray.GetCardsContent().CreateCardTileVisual(tileVisual.name + " Preview", draggedCardsContainer);
		InitializeDraggableClone(tileClone, tileVisual, index);
		return tileClone;
	}

	private void InitializeDraggableClone(DeckTrayDeckTileVisual tileClone, DeckTrayDeckTileVisual tileVisual, int index)
	{
		bool offsetForRunes = tileVisual.HasRuneCost();
		tileClone.SetSlot(m_currentDeck, tileVisual.GetSlot(), useSliderAnimations: false, offsetForRunes);
		tileClone.transform.rotation = Quaternion.identity;
		float xOffset = (float)index * draggedTileOffset.x;
		float yOffset = (float)index * draggedTileOffset.y;
		float zOffset = (float)index * draggedTileOffset.z;
		tileClone.transform.localPosition = new Vector3(xOffset, yOffset, zOffset);
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void EnableRuneButtons()
	{
		for (int i = 0; i < runeButtons.Length; i++)
		{
			runeButtons[i].SetEnabled(enabled: true);
		}
	}

	public void DisableRuneButtons()
	{
		for (int i = 0; i < runeButtons.Length; i++)
		{
			runeButtons[i].SetEnabled(enabled: false);
		}
	}

	public void ResetRuneButtons()
	{
		if (m_currentDeck != null)
		{
			RuneButton[] array = runeButtons;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetRune(RuneType.RT_NONE, animate: true);
				RuneIndicatorVisual.RunePatternChanged?.Invoke(m_currentDeck.Runes);
			}
		}
	}

	public void HighlightAllRunes(bool highlight)
	{
		RuneButton[] array = runeButtons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetHighlighted(highlight);
		}
	}
}
