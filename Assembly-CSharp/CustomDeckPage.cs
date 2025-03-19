using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using PegasusShared;
using UnityEngine;

public class CustomDeckPage : MonoBehaviour
{
	public delegate void DeckButtonCallback(CollectionDeckBoxVisual deckbox);

	public enum DeckPageDisplayFormat
	{
		FILL_ALL_DECKSLOTS,
		EMPTY_FIRST_ROW
	}

	public enum PageType
	{
		CUSTOM_DECK_DISPLAY,
		LOANER_DECK_DISPLAY,
		PRECON_DECK_DISPLAY,
		RESERVE
	}

	public Vector3 m_customDeckStart;

	public Vector3 m_customDeckScale;

	public float m_customDeckHorizontalSpacing;

	public float m_customDeckVerticalSpacing;

	public CollectionDeckBoxVisual m_deckboxPrefab;

	public Vector3 m_deckCoverOffset;

	public GameObject m_deckboxCoverPrefab;

	public PlayMakerFSM m_vineGlowBurst;

	public GameObject[] m_customVineGlowToggle;

	public int m_maxCustomDecksToDisplay = 9;

	public const int FIRST_ROW_DECK_COUNT = 3;

	public Material m_multipleDeckSelectionHighlightMaterial;

	protected List<GameObject> m_deckCovers = new List<GameObject>();

	protected List<CollectionDeck> m_collectionDecks;

	protected int m_numCustomDecks;

	protected List<CollectionDeckBoxVisual> m_customDecks = new List<CollectionDeckBoxVisual>();

	protected DeckButtonCallback m_deckButtonCallback;

	private Texture m_customTrayMainTexture;

	private Texture m_customTrayTransitionToTexture;

	private Renderer m_renderer;

	private bool m_initialized;

	private List<GameObject> m_decksCreatedOnPage;

	private bool m_isEnabledAsReserve;

	private DeckPageDisplayFormat m_deckDisplayFormat;

	private PageType m_pageType;

	public const int DEFAULT_MAX_CUSTOM_DECKS_TO_DISPLAY_FIRST_PAGE = 6;

	public const int DEFAULT_MAX_CUSTOM_DECKS_TO_DISPLAY = 9;

	public DeckPageDisplayFormat DeckDisplayFormat
	{
		get
		{
			return m_deckDisplayFormat;
		}
		set
		{
			m_deckDisplayFormat = value;
		}
	}

	public PageType DeckPageType
	{
		get
		{
			return m_pageType;
		}
		set
		{
			m_pageType = value;
		}
	}

	public bool EnabledAsReserve => m_isEnabledAsReserve;

	private void Start()
	{
		m_renderer = GetComponent<Renderer>();
	}

	public void Show()
	{
		m_renderer.enabled = true;
		for (int i = 0; i < m_numCustomDecks; i++)
		{
			if (i < m_customDecks.Count)
			{
				m_customDecks[i].Show();
			}
		}
	}

	public void Hide()
	{
		m_renderer.enabled = false;
		for (int i = 0; i < m_numCustomDecks; i++)
		{
			if (i < m_customDecks.Count)
			{
				m_customDecks[i].Hide();
			}
		}
	}

	public virtual bool PageReady()
	{
		if (m_customTrayMainTexture != null)
		{
			return AreAllCustomDecksReady();
		}
		return false;
	}

	public CollectionDeckBoxVisual GetDeckboxWithDeckID(long deckID)
	{
		if (deckID <= 0)
		{
			return null;
		}
		foreach (CollectionDeckBoxVisual deckBox in m_customDecks)
		{
			if (deckBox.GetDeckID() == deckID)
			{
				return deckBox;
			}
		}
		return null;
	}

	public CollectionDeckBoxVisual GetDeckboxWithDeckTemplateID(long deckTemplateID)
	{
		if (deckTemplateID <= 0)
		{
			return null;
		}
		foreach (CollectionDeckBoxVisual deckBox in m_customDecks)
		{
			if (deckBox.GetDeckTemplateId() == deckTemplateID)
			{
				return deckBox;
			}
		}
		return null;
	}

	public CollectionDeckBoxVisual GetFirstDeckbox()
	{
		if (m_customDecks.Count == 0)
		{
			return null;
		}
		return m_customDecks[0];
	}

	public void UpdateTrayTransitionValue(float transitionValue)
	{
		GetComponent<Renderer>().GetMaterial().SetFloat("_Transistion", transitionValue);
		foreach (GameObject deckCover in m_deckCovers)
		{
			Renderer renderer = deckCover.GetComponentInChildren<Renderer>();
			if (renderer != null)
			{
				renderer.GetMaterial().SetFloat("_Transistion", transitionValue);
			}
		}
	}

	public void PlayVineGlowBurst(bool useFX, bool hasValidStandardDeck)
	{
		if (m_vineGlowBurst != null)
		{
			string eventName = ((!useFX) ? (hasValidStandardDeck ? "GlowVinesNoFX" : "GlowVinesCustomNoFX") : (hasValidStandardDeck ? "GlowVines" : "GlowVinesCustom"));
			if (!string.IsNullOrEmpty(eventName))
			{
				m_vineGlowBurst.SendEvent(eventName);
			}
		}
	}

	public void SetTrayTextures(Texture transitionFromTexture, Texture targetTexture)
	{
		Material material = GetComponent<Renderer>().GetMaterial();
		material.mainTexture = transitionFromTexture;
		material.SetTexture("_MainTex2", targetTexture);
		material.SetFloat("_Transistion", 0f);
		m_customTrayMainTexture = transitionFromTexture;
		m_customTrayTransitionToTexture = targetTexture;
		foreach (GameObject deckCover in m_deckCovers)
		{
			Material material2 = deckCover.GetComponentInChildren<Renderer>().GetMaterial();
			material2.mainTexture = m_customTrayMainTexture;
			material2.SetTexture("_MainTex2", m_customTrayTransitionToTexture);
			material2.SetFloat("_Transistion", 0f);
		}
		if (m_pageType != PageType.LOANER_DECK_DISPLAY)
		{
			UpdateDeckVisuals(m_collectionDecks);
		}
	}

	public void SetDeckButtonCallback(DeckButtonCallback callback)
	{
		m_deckButtonCallback = callback;
	}

	public void EnableDeckButtons(bool enable)
	{
		foreach (CollectionDeckBoxVisual deck in m_customDecks)
		{
			deck.SetEnabled(enable);
			if (!enable)
			{
				deck.SetHighlightState(ActorStateType.HIGHLIGHT_OFF);
			}
		}
	}

	public CollectionDeckBoxVisual FindDeckVisual(CollectionDeck deck)
	{
		int index = 0;
		foreach (CollectionDeck collectionDeck in m_collectionDecks)
		{
			if (collectionDeck == deck)
			{
				return m_customDecks[index];
			}
			index++;
		}
		return null;
	}

	public void TransitionWildDecks()
	{
		int index = 0;
		foreach (CollectionDeck deck in m_collectionDecks)
		{
			if (deck.Type == DeckType.NORMAL_DECK)
			{
				CollectionDeckBoxVisual cd = m_customDecks[index];
				if (deck.FormatType == FormatType.FT_WILD)
				{
					cd.PlayGlowAnim();
				}
				cd.UpdateInvalidCardCountIndicator();
				index++;
			}
		}
	}

	public void UpdateDeckVisuals(List<CollectionDeck> collectionDecks = null)
	{
		int index = 0;
		m_numCustomDecks = 0;
		if (collectionDecks == null)
		{
			if (m_collectionDecks == null)
			{
				Log.DeckTray.Print("m_collectionDecks was null, decks may not be initialized correctly");
				return;
			}
			collectionDecks = m_collectionDecks;
		}
		foreach (CollectionDeck deck in collectionDecks)
		{
			if (deck.Type == DeckType.NORMAL_DECK)
			{
				if (deck.FormatType == FormatType.FT_UNKNOWN && !deck.Locked)
				{
					Debug.LogError("A deck with an unknown format type was detected. Details: " + deck.ToString());
				}
				m_numCustomDecks++;
				CollectionDeckBoxVisual cd = m_customDecks[index];
				cd.SetIsShared(deck.IsShared);
				cd.SetDeckName(deck.Name);
				cd.SetFormatType(deck.FormatType);
				if (deck.IsDeckTemplate)
				{
					cd.SetDeckTemplateId(deck.DeckTemplateId);
					cd.SetUnlockableStatus(deck.DeckTemplate_HasUnownedRequirements(out var _));
				}
				else
				{
					cd.SetDeckID(deck.ID);
				}
				cd.SetHeroCardPremiumOverride(deck.GetDisplayHeroPremiumOverride());
				cd.SetHeroCardID(deck.GetDisplayHeroCardID(!m_initialized), null);
				cd.UpdateInvalidCardCountIndicator();
				cd.UpdateRuneSlotVisual(deck);
				cd.Show();
				if (index < m_deckCovers.Count)
				{
					m_deckCovers[index].SetActive(value: false);
				}
				index++;
				if (index >= m_customDecks.Count)
				{
					break;
				}
			}
		}
		for (; index < m_customDecks.Count; index++)
		{
			m_customDecks[index].Hide();
			if (index < m_deckCovers.Count)
			{
				m_deckCovers[index].SetActive(value: true);
			}
		}
	}

	public bool HasWildDeck()
	{
		foreach (CollectionDeck collectionDeck in m_collectionDecks)
		{
			if (collectionDeck.FormatType == FormatType.FT_WILD)
			{
				return true;
			}
		}
		return false;
	}

	private bool AreAllCustomDecksReady()
	{
		foreach (CollectionDeckBoxVisual customDeck in m_customDecks)
		{
			if (customDeck.IsLoading())
			{
				return false;
			}
		}
		return true;
	}

	public void InitDecks(List<CollectionDeck> decks)
	{
		m_collectionDecks = decks;
		if (!m_initialized)
		{
			m_decksCreatedOnPage = new List<GameObject>();
			int emptyRowDeckCount = 0;
			if (DeckDisplayFormat == DeckPageDisplayFormat.EMPTY_FIRST_ROW)
			{
				emptyRowDeckCount += 3;
			}
			for (int i = 0; i < m_maxCustomDecksToDisplay - emptyRowDeckCount; i++)
			{
				m_decksCreatedOnPage.Add(CreateDeck(i));
			}
			UpdateDeckVisuals(m_collectionDecks);
			m_initialized = true;
		}
	}

	public void ArrangeDeckOnPage(GameObject deckToDisplay, int index)
	{
		if ((bool)deckToDisplay)
		{
			if (DeckDisplayFormat == DeckPageDisplayFormat.EMPTY_FIRST_ROW)
			{
				index += 3;
			}
			float horizontalSpacing = m_customDeckHorizontalSpacing;
			float verticalSpacing = m_customDeckVerticalSpacing;
			if (index == 0)
			{
				deckToDisplay.transform.localPosition = m_customDeckStart;
				return;
			}
			float xPos = m_customDeckStart.x - (float)(index % 3) * horizontalSpacing;
			float zPos = (float)Mathf.CeilToInt(index / 3) * verticalSpacing + m_customDeckStart.z;
			deckToDisplay.transform.localPosition = new Vector3(xPos, m_customDeckStart.y, zPos);
		}
	}

	public int GetAvailableDeckDisplaySlotCount(int decksToShow)
	{
		int maxCustomDeckOnPage = m_maxCustomDecksToDisplay;
		if (DeckDisplayFormat == DeckPageDisplayFormat.EMPTY_FIRST_ROW)
		{
			maxCustomDeckOnPage -= 3;
		}
		return Mathf.Min(decksToShow, maxCustomDeckOnPage);
	}

	public void ClearDeckSlots()
	{
		if (m_collectionDecks != null)
		{
			m_collectionDecks.Clear();
		}
		if (m_decksCreatedOnPage != null)
		{
			for (int i = 0; i < m_decksCreatedOnPage.Count; i++)
			{
				Object.Destroy(m_decksCreatedOnPage[i]);
			}
			m_decksCreatedOnPage.Clear();
		}
		if (m_deckCovers != null)
		{
			for (int j = 0; j < m_deckCovers.Count; j++)
			{
				Object.Destroy(m_deckCovers[j]);
			}
			m_deckCovers.Clear();
		}
		if (m_customDecks != null)
		{
			m_customDecks.Clear();
		}
		m_numCustomDecks = 0;
		m_initialized = false;
	}

	public void SetReservePageStatus(bool status)
	{
		if (m_pageType == PageType.RESERVE)
		{
			m_isEnabledAsReserve = status;
		}
	}

	private GameObject CreateDeck(int index)
	{
		GameObject go = new GameObject();
		go.name = "DeckParent" + index;
		go.transform.parent = base.gameObject.transform;
		ArrangeDeckOnPage(go, index);
		CollectionDeckBoxVisual deckBox = Object.Instantiate(m_deckboxPrefab);
		CollectionDeckBoxVisual collectionDeckBoxVisual = deckBox;
		collectionDeckBoxVisual.name = collectionDeckBoxVisual.name + " - " + index;
		deckBox.transform.parent = go.transform;
		deckBox.transform.localPosition = Vector3.zero;
		deckBox.StoreOriginalButtonPositionAndRotation();
		go.transform.localScale = m_customDeckScale;
		if (m_deckButtonCallback == null)
		{
			Debug.LogError("SetDeckButtonCallback() not called in CustomDeckPage!");
		}
		else
		{
			deckBox.AddEventListener(UIEventType.RELEASE, delegate
			{
				OnSelectCustomDeck(deckBox);
			});
		}
		deckBox.SetEnabled(enabled: true);
		m_customDecks.Add(deckBox);
		CreateDeckCover(go);
		return go;
	}

	private void CreateDeckCover(GameObject go)
	{
		if (m_deckboxCoverPrefab != null)
		{
			GameObject deckCover = Object.Instantiate(m_deckboxCoverPrefab);
			deckCover.transform.parent = base.gameObject.transform;
			deckCover.transform.localScale = m_customDeckScale;
			deckCover.transform.position = go.transform.position + m_deckCoverOffset;
			m_deckCovers.Add(deckCover);
		}
	}

	private void OnSelectCustomDeck(CollectionDeckBoxVisual deckbox)
	{
		m_deckButtonCallback(deckbox);
	}
}
