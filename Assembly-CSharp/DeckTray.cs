using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public abstract class DeckTray : MonoBehaviour
{
	public enum DeckContentTypes
	{
		Decks,
		Cards,
		HeroSkin,
		CardBack,
		Coin,
		Teams,
		Mercs,
		INVALID
	}

	public delegate void ModeSwitched();

	[Serializable]
	public class DeckContentScroll
	{
		public DeckContentTypes m_contentType;

		public GameObject m_scrollObject;

		public bool m_saveScrollPosition;

		private Vector3 m_startPosition;

		private float m_currentScroll;

		public void SaveStartPosition()
		{
			if (m_scrollObject != null)
			{
				m_startPosition = m_scrollObject.transform.localPosition;
			}
		}

		public Vector3 GetStartPosition()
		{
			return m_startPosition;
		}

		public void SaveCurrentScroll(float scroll)
		{
			m_currentScroll = scroll;
		}

		public float GetCurrentScroll()
		{
			return m_currentScroll;
		}
	}

	public DeckTrayCardListContent m_cardsContent;

	public UIBScrollable m_scrollbar;

	public DeckBigCard m_deckBigCard;

	public GameObject m_inputBlocker;

	public GameObject m_topCardPositionBone;

	public List<DeckContentScroll> m_scrollables = new List<DeckContentScroll>();

	protected Map<DeckContentTypes, DeckTrayContent> m_contents = new Map<DeckContentTypes, DeckTrayContent>();

	protected DeckContentTypes m_currentContent = DeckContentTypes.INVALID;

	protected DeckContentTypes m_contentToSet = DeckContentTypes.INVALID;

	protected bool m_settingNewMode;

	protected bool m_updatingTrayMode;

	protected List<ModeSwitched> m_modeSwitchedListeners = new List<ModeSwitched>();

	protected virtual void Start()
	{
		SoundManager.Get().Load("panel_slide_off_deck_creation_screen.prefab:b0d25fc984ec05d4fbea7480b611e5ad");
	}

	public void Initialize()
	{
		DeckContentTypes contentType = DeckContentTypes.INVALID;
		SceneMgr.Mode mode = SceneMgr.Get().GetMode();
		contentType = (((uint)(mode - 19) <= 4u) ? DeckContentTypes.Teams : DeckContentTypes.Decks);
		SetTrayMode(contentType);
	}

	public DeckTrayCardListContent GetCardsContent()
	{
		return m_cardsContent;
	}

	public DeckTrayContent GetCurrentContent()
	{
		m_contents.TryGetValue(m_currentContent, out var content);
		return content;
	}

	public DeckContentTypes GetCurrentContentType()
	{
		return m_currentContent;
	}

	public DeckBigCard GetDeckBigCard()
	{
		return m_deckBigCard;
	}

	public void SetTrayMode(DeckContentTypes contentType)
	{
		m_contentToSet = contentType;
		if (!m_settingNewMode && m_currentContent != contentType)
		{
			StartCoroutine(UpdateTrayMode());
		}
	}

	protected abstract IEnumerator UpdateTrayMode();

	public bool IsUpdatingTrayMode()
	{
		return m_updatingTrayMode;
	}

	public void TryEnableScrollbar()
	{
		if (m_scrollbar == null || GetCurrentContent() == null)
		{
			return;
		}
		DeckContentScroll findScrollObject = m_scrollables.Find((DeckContentScroll type) => GetCurrentContentType() == type.m_contentType);
		if (findScrollObject == null || findScrollObject.m_scrollObject == null)
		{
			Debug.LogWarning("No scrollable object defined.");
			return;
		}
		m_scrollbar.ScrollObject = findScrollObject.m_scrollObject;
		m_scrollbar.ResetScrollStartPosition(findScrollObject.GetStartPosition());
		if (findScrollObject.m_saveScrollPosition)
		{
			m_scrollbar.SetScrollSnap(findScrollObject.GetCurrentScroll());
		}
		m_scrollbar.EnableIfNeeded();
	}

	public void SaveScrollbarPosition(DeckContentTypes contentType)
	{
		DeckContentScroll findScrollObject = m_scrollables.Find((DeckContentScroll type) => contentType == type.m_contentType);
		if (findScrollObject != null && findScrollObject.m_saveScrollPosition)
		{
			findScrollObject.SaveCurrentScroll(m_scrollbar.GetScroll());
		}
	}

	public void ResetDeckTrayScroll()
	{
		if (m_scrollbar == null)
		{
			return;
		}
		m_scrollbar.SetScrollSnap(0f);
		foreach (DeckContentScroll scrollable in m_scrollables)
		{
			scrollable.SaveCurrentScroll(0f);
		}
	}

	protected void TryDisableScrollbar()
	{
		if (!(m_scrollbar == null) && !(m_scrollbar.ScrollObject == null))
		{
			m_scrollbar.Enable(enable: false);
			m_scrollbar.ScrollObject = null;
		}
	}

	public void AllowInput(bool allowed)
	{
		m_inputBlocker.SetActive(!allowed);
	}

	public bool MouseIsOver()
	{
		if (!UniversalInputManager.Get().InputIsOver(base.gameObject))
		{
			return m_cardsContent.MouseIsOverDeckHelperButton(Box.Get().GetCamera());
		}
		return true;
	}

	public bool MouseIsOver(Camera camera)
	{
		if (!UniversalInputManager.Get().ForcedUnblockableInputIsOver(camera, base.gameObject, out var _))
		{
			return m_cardsContent.MouseIsOverDeckHelperButton(camera);
		}
		return true;
	}

	protected abstract void HideUnseenDeckTrays();

	protected void OnTouchScrollStarted()
	{
		if (m_deckBigCard != null)
		{
			m_deckBigCard.ForceHide();
		}
	}

	protected void OnTouchScrollEnded()
	{
	}

	public static void OnDeckTrayTileScrollVisibleAffected(GameObject obj, bool visible)
	{
		DeckTrayDeckTileVisual deckTile = obj.GetComponent<DeckTrayDeckTileVisual>();
		if (!(deckTile == null) && deckTile.IsInUse() && visible != deckTile.gameObject.activeSelf)
		{
			deckTile.gameObject.SetActive(visible);
		}
	}

	protected abstract void ShowDeckBigCard(DeckTrayDeckTileVisual cardTile, float delay = 0f);

	protected abstract void HideDeckBigCard(DeckTrayDeckTileVisual cardTile, bool force = false);

	protected abstract void OnCardTilePress(DeckTrayDeckTileVisual cardTile);

	protected abstract void OnCardTileOver(DeckTrayDeckTileVisual cardTile);

	protected abstract void OnCardTileOut(DeckTrayDeckTileVisual cardTile);

	protected abstract void OnCardTileRelease(DeckTrayDeckTileVisual cardTile);

	public bool IsShowingDeckContents()
	{
		return GetCurrentContentType() != DeckContentTypes.Decks;
	}

	public bool IsShowingTeamContents()
	{
		return GetCurrentContentType() != DeckContentTypes.Teams;
	}

	protected void OnBusyWithDeck(bool busy)
	{
		if (m_inputBlocker == null)
		{
			Log.All.PrintError("If this happens, please notify JMac and copy your stack trace to bug 21743!");
		}
		else
		{
			m_inputBlocker.SetActive(busy);
		}
	}

	protected virtual void OnEditedDeckChanged(CollectionDeck newDeck, CollectionDeck oldDeck, object callbackData)
	{
		bool isNewDeck = callbackData != null && callbackData is bool && (bool)callbackData;
		foreach (KeyValuePair<DeckContentTypes, DeckTrayContent> content in m_contents)
		{
			content.Value.OnEditedDeckChanged(newDeck, oldDeck, isNewDeck);
		}
	}

	protected virtual void OnEditingTeamChanged(LettuceTeam newTeam, LettuceTeam oldTeam, object callbackData)
	{
		bool isNewTeam = callbackData != null && callbackData is bool && (bool)callbackData;
		foreach (KeyValuePair<DeckContentTypes, DeckTrayContent> content in m_contents)
		{
			content.Value.OnEditingTeamChanged(newTeam, oldTeam, isNewTeam);
		}
	}

	public abstract bool OnBackOutOfContainerContents();

	protected void FireModeSwitchedEvent()
	{
		ModeSwitched[] arr = m_modeSwitchedListeners.ToArray();
		for (int i = 0; i < arr.Length; i++)
		{
			arr[i]();
		}
	}

	public void RegisterModeSwitchedListener(ModeSwitched callback)
	{
		m_modeSwitchedListeners.Add(callback);
	}

	public void UnregisterModeSwitchedListener(ModeSwitched callback)
	{
		m_modeSwitchedListeners.Remove(callback);
	}
}
