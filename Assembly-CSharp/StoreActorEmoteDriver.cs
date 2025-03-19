using System;
using System.Collections;
using System.Collections.Generic;
using Hearthstone.UI.Core;
using UnityEngine;

public class StoreActorEmoteDriver : MonoBehaviour
{
	private const float DefaultEmoteWaitTimeSec = 1.5f;

	private readonly Dictionary<EmoteType, EmoteEntry> m_actorEmoteLookup = new Dictionary<EmoteType, EmoteEntry>();

	private Notification m_activeNotification;

	private Actor m_actor;

	[Overridable]
	public Actor Actor
	{
		get
		{
			return m_actor;
		}
		set
		{
			if (!(value == m_actor))
			{
				OnActorLoaded(value);
			}
		}
	}

	public void PlayEmote(Actor owner, EmoteType emote, Action<int> onFinishedCallback = null)
	{
		if (emote != 0)
		{
			EmoteEntry entry;
			if (owner != m_actor)
			{
				Debug.LogError("StoreActorEmoteDriver: Failed to play emote as requesting actor does match loaded emotes actor.");
			}
			else if (!m_actorEmoteLookup.TryGetValue(emote, out entry))
			{
				Debug.LogWarning("StoreActorEmoteDriver: Failed to play emote: " + emote.ToString() + " as it is not support by loaded actor.");
			}
			else
			{
				InternalPlayEmote(entry, onFinishedCallback);
			}
		}
	}

	public void StopEmote()
	{
		if (m_actorEmoteLookup == null || m_actorEmoteLookup.Count == 0)
		{
			return;
		}
		foreach (EmoteEntry value in m_actorEmoteLookup.Values)
		{
			Spell currSpell = value.GetSoundSpell(loadIfNeeded: false);
			if ((bool)currSpell)
			{
				currSpell.Deactivate();
			}
		}
		if (m_activeNotification != null)
		{
			NotificationManager.Get().DestroyNotificationNowWithNoAnim(m_activeNotification);
		}
	}

	private void InternalPlayEmote(EmoteEntry emoteEntry, Action<int> onFinishedCallback = null)
	{
		if (emoteEntry == null)
		{
			Debug.LogError("StoreActorEmoteDriver: Failed to play emote as EmoteEntry was null!");
			return;
		}
		CardSoundSpell emoteSoundSpell = emoteEntry.GetSoundSpell();
		Spell emoteSpell = emoteEntry.GetSpell();
		if (emoteSoundSpell != null)
		{
			emoteSoundSpell.Reactivate();
			if (emoteSoundSpell.IsActive())
			{
				foreach (EmoteEntry entry in m_actorEmoteLookup.Values)
				{
					if (entry != emoteEntry)
					{
						Spell currSpell = entry.GetSoundSpell(loadIfNeeded: false);
						if ((bool)currSpell)
						{
							currSpell.Deactivate();
						}
					}
				}
			}
		}
		string localizedString = null;
		if (emoteSoundSpell != null)
		{
			localizedString = string.Empty;
			if (emoteSoundSpell is CardSpecificVoSpell)
			{
				CardSpecificVoData voData = ((CardSpecificVoSpell)emoteSoundSpell).GetBestVoiceData();
				if (voData != null && !string.IsNullOrEmpty(voData.m_GameStringKey))
				{
					localizedString = GameStrings.Get(voData.m_GameStringKey);
				}
			}
		}
		if (string.IsNullOrEmpty(localizedString) && !string.IsNullOrEmpty(emoteEntry.GetGameStringKey()))
		{
			localizedString = GameStrings.Get(emoteEntry.GetGameStringKey());
		}
		if (m_activeNotification != null)
		{
			NotificationManager.Get().DestroyNotificationNowWithNoAnim(m_activeNotification);
		}
		if (!string.IsNullOrEmpty(localizedString))
		{
			m_activeNotification = NotificationManager.Get().CreateSpeechBubble(new NotificationManager.SpeechBubbleOptions().WithSpeechText(localizedString).WithSpeechBubbleDirection(Notification.SpeechBubbleDirection.BottomLeft).WithActor(m_actor)
				.WithDestroyWhenNewCreated(destroyWhenNewCreated: true)
				.WithParentToActor(parentToActor: true)
				.WithVisualEmoteType(NotificationManager.VisualEmoteType.STORE));
			float waitTime = 1.5f;
			if (emoteSoundSpell != null)
			{
				AudioSource source = emoteSoundSpell.GetActiveAudioSource();
				if ((bool)source && (bool)source.clip && waitTime < source.clip.length)
				{
					waitTime = source.clip.length;
				}
			}
		}
		if (emoteSpell != null)
		{
			VisualEmoteSpell visualEmoteSpell = emoteSpell as VisualEmoteSpell;
			if (visualEmoteSpell != null && visualEmoteSpell.m_PositionOnSpeechBubble && m_activeNotification != null)
			{
				visualEmoteSpell.SetSource(m_activeNotification.gameObject);
				visualEmoteSpell.Reactivate();
			}
			else
			{
				emoteSpell.Reactivate();
			}
		}
		StartCoroutine(WaitForEmoteFinish(emoteSoundSpell, onFinishedCallback));
		m_actor.LegendaryHeroPortrait?.RaiseEmoteAnimationEvent(emoteEntry.GetEmoteType());
	}

	private IEnumerator WaitForEmoteFinish(Spell emote, Action<int> onFinishedCallback = null)
	{
		yield return new WaitUntil(() => emote == null || emote.IsFinished());
		onFinishedCallback?.Invoke(0);
		NotificationManager.Get().DestroyNotification(m_activeNotification, 0f);
	}

	private void OnActorLoaded(Actor actor)
	{
		foreach (EmoteEntry value in m_actorEmoteLookup.Values)
		{
			value.Clear();
		}
		m_actorEmoteLookup.Clear();
		m_actor = actor;
		if (m_actor.EmoteDefs == null || m_actor.EmoteDefs.Count == 0)
		{
			return;
		}
		foreach (EmoteEntryDef emoteDef in m_actor.EmoteDefs)
		{
			m_actorEmoteLookup[emoteDef.m_emoteType] = new EmoteEntry(emoteDef.m_emoteType, emoteDef.m_emoteSpellPath, emoteDef.m_emoteSoundSpellPath, emoteDef.m_emoteGameStringKey, m_actor);
		}
	}
}
