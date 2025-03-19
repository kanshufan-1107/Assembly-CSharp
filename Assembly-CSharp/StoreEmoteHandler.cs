using System;
using System.Collections.Generic;
using Hearthstone.UI;
using Hearthstone.UI.Core;
using UnityEngine;

public class StoreEmoteHandler : MonoBehaviour
{
	private const float MinTimeBetweenEmotesSec = 1f;

	[SerializeField]
	private List<StoreEmoteOption> m_emotes;

	[SerializeField]
	[Tooltip("Reference to the Widget behaviour Card component that we want to derive emotes from.")]
	private AsyncReference m_asyncCardParentWidgetReference;

	[SerializeField]
	private StoreActorEmoteDriver m_emoteDriver;

	[SerializeField]
	private bool m_shouldDefaultToShown;

	[SerializeField]
	[Tooltip("When the player hits an emote bubble, should they hide while that emote is playing?")]
	private bool m_shouldEmotesHideOnPlay;

	[SerializeField]
	private float m_minTimeBetweenEmotesSec = 1f;

	private Hearthstone.UI.Card m_dataCard;

	private Actor m_actor;

	private float m_timeBetweenEmotesSec;

	private float m_emoteCooldownTimeSec;

	private bool m_isShowingEmotes;

	[Overridable]
	public bool ToggleEmotes
	{
		get
		{
			return m_isShowingEmotes;
		}
		set
		{
			if (value != m_isShowingEmotes)
			{
				if (value)
				{
					ShowEmotes();
				}
				else
				{
					HideEmotes();
				}
			}
		}
	}

	private void Awake()
	{
		m_timeBetweenEmotesSec = Math.Max(1f, m_minTimeBetweenEmotesSec);
		if (m_shouldDefaultToShown)
		{
			ShowEmotes();
		}
		else
		{
			HideEmotes();
		}
	}

	private void OnDestroy()
	{
		if (m_dataCard != null)
		{
			m_dataCard.UnregisterCardLoadedListener(OnDataCardLoaded);
			m_dataCard = null;
		}
	}

	private void Start()
	{
		m_asyncCardParentWidgetReference.RegisterReadyListener<Hearthstone.UI.Card>(OnCardParentWidgetReady);
	}

	public void ShowEmotes()
	{
		if (m_isShowingEmotes)
		{
			return;
		}
		m_isShowingEmotes = true;
		if (m_emoteDriver != null)
		{
			m_emoteDriver.StopEmote();
		}
		foreach (StoreEmoteOption emote in m_emotes)
		{
			emote.Enable();
		}
	}

	public void HideEmotes(bool isImmediateHide = true)
	{
		if (!m_isShowingEmotes)
		{
			return;
		}
		m_isShowingEmotes = false;
		if (isImmediateHide && m_emoteDriver != null)
		{
			m_emoteDriver.StopEmote();
		}
		foreach (StoreEmoteOption emote in m_emotes)
		{
			emote.Disable(isImmediateHide);
		}
	}

	public void PlayEmote(EmoteType emoteType)
	{
		if (emoteType == EmoteType.INVALID)
		{
			return;
		}
		if (m_emoteDriver == null)
		{
			Debug.LogError("StoreEmoteHandler: Failed to play emote " + emoteType.ToString() + " as no driver is set.");
		}
		else if (m_actor == null)
		{
			Debug.LogError("StoreEmoteHandler: Failed to play emote as no actor is set to request emote for.");
		}
		else
		{
			if (Time.unscaledTime < m_emoteCooldownTimeSec)
			{
				return;
			}
			m_emoteCooldownTimeSec = Time.unscaledTime + m_timeBetweenEmotesSec;
			m_emoteDriver.PlayEmote(m_actor, emoteType, delegate
			{
				if (m_shouldEmotesHideOnPlay && base.gameObject.activeInHierarchy)
				{
					ShowEmotes();
				}
			});
			if (m_shouldEmotesHideOnPlay)
			{
				HideEmotes(isImmediateHide: false);
			}
		}
	}

	private void TryLoadCardActor(Actor cardActor)
	{
		if (cardActor == null)
		{
			Debug.LogError("StoreEmoteHandler: Failed to load card actor as passed null.");
			return;
		}
		if (m_emoteDriver == null)
		{
			Debug.LogError("StoreEmoteHandler: Failed to load card actor as StoreActorEmoteDriver was null.");
			return;
		}
		m_actor = cardActor;
		m_emoteDriver.Actor = cardActor;
	}

	private void OnCardParentWidgetReady(Hearthstone.UI.Card dataCard)
	{
		if (dataCard == null)
		{
			Debug.LogError("StoreEmoteHandler: Widget for card parent was null.");
			return;
		}
		m_dataCard = dataCard;
		if (m_dataCard.CardActor == null)
		{
			dataCard.RegisterCardLoadedListener(OnDataCardLoaded);
		}
		else
		{
			TryLoadCardActor(m_dataCard.CardActor);
		}
	}

	private void OnDataCardLoaded(Actor cardActor)
	{
		if (cardActor == null)
		{
			Debug.LogError("StoreEmoteHandler: Failed to find Ui.Card actor.");
		}
		else
		{
			TryLoadCardActor(cardActor);
		}
	}
}
