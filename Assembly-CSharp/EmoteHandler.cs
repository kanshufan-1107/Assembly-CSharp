using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using UnityEngine;

public class EmoteHandler : MonoBehaviour
{
	public List<EmoteOption> m_DefaultEmotes;

	public List<EmoteOption> m_EmoteOverrides;

	public List<EmoteOption> m_HiddenEmotes;

	private List<EmoteOption> m_availableEmotes;

	private const float MIN_TIME_BETWEEN_EMOTES = 4f;

	private const float TIME_WINDOW_TO_BE_CONSIDERED_A_CHAIN = 5f;

	private const float SPAMMER_MIN_TIME_BETWEEN_EMOTES = 15f;

	private const float UBER_SPAMMER_MIN_TIME_BETWEEN_EMOTES = 45f;

	private const int NUM_EMOTES_BEFORE_CONSIDERED_A_SPAMMER = 20;

	private const int NUM_EMOTES_BEFORE_CONSIDERED_UBER_SPAMMER = 25;

	private const int NUM_CHAIN_EMOTES_BEFORE_CONSIDERED_SPAM = 2;

	private const int EMOTE_COUNT = 6;

	private const float MAX_TIME_FOR_EMOTE_RESPONSE = 6f;

	private const float EMOTE_RESPONSE_SERVER_DELAY_SLUSH = 2f;

	private const float DEFAULT_STARTING_TAUNT_DURATION = 2.5f;

	private static EmoteHandler s_instance;

	private bool m_emotesShown;

	private int m_shownAtFrame;

	private EmoteOption m_mousedOverEmote;

	private float m_timeSinceLastEmote = 4f;

	private int m_totalEmotes;

	private int m_chainedEmotes;

	private Map<EmoteType, float> m_timeSinceEmoteFinishedOpposing = new Map<EmoteType, float>();

	private Map<EmoteType, float> m_timeSinceEmoteFinishedFriendly = new Map<EmoteType, float>();

	private Map<EmoteType, float>.KeyCollection m_keyCollectionOpposing;

	private Map<EmoteType, float>.KeyCollection m_keyCollectionFriendly;

	private List<EmoteType> m_keyType = new List<EmoteType>();

	private void Awake()
	{
		s_instance = this;
		GetComponent<Collider>().enabled = false;
	}

	private void Start()
	{
		GameState.Get().RegisterHeroChangedListener(OnHeroChanged);
		DetermineAvailableEmotes();
		m_keyCollectionOpposing = m_timeSinceEmoteFinishedOpposing.Keys;
		m_keyCollectionFriendly = m_timeSinceEmoteFinishedFriendly.Keys;
	}

	private void DetermineAvailableEmotes()
	{
		m_availableEmotes = new List<EmoteOption>();
		if (m_DefaultEmotes == null || m_DefaultEmotes.Count == 0)
		{
			Debug.LogError("EmoteHandler has no default emotes defined.");
			return;
		}
		foreach (EmoteOption emote in m_DefaultEmotes)
		{
			if ((object)emote.gameObject == null)
			{
				Debug.LogError($"EmoteHandler contains a default emote {emote.m_EmoteType} with no gameObject.");
				continue;
			}
			m_availableEmotes.Add(emote);
			emote.gameObject.SetActive(value: true);
		}
		Player friendlySidePlayer = GameState.Get().GetFriendlySidePlayer();
		for (int i = 0; i < 6; i++)
		{
			int overrideValue = friendlySidePlayer.GetTag((GAME_TAG)(740 + i));
			if (overrideValue > 0 && overrideValue < m_EmoteOverrides.Count && (m_EmoteOverrides[overrideValue].ShouldPlayerUseEmoteOverride(friendlySidePlayer) || GameState.Get().GetBooleanGameOption(GameEntityOption.USES_PREMIUM_EMOTES)))
			{
				EmoteOption replacement = m_EmoteOverrides[overrideValue];
				if ((object)replacement == null || (object)replacement.gameObject == null)
				{
					Debug.LogError($"EmoteHandler contains an emote override tag for {m_availableEmotes[i].m_EmoteType} that is missing or has no gameObject.");
					continue;
				}
				m_availableEmotes[i].gameObject.SetActive(value: false);
				m_availableEmotes[i] = replacement;
				TransformUtil.CopyWorld(m_availableEmotes[i], m_DefaultEmotes[i]);
				m_availableEmotes[i].gameObject.SetActive(value: true);
			}
		}
	}

	private void OnDestroy()
	{
		s_instance = null;
	}

	private void Update()
	{
		if (GameState.Get() == null)
		{
			return;
		}
		m_timeSinceLastEmote += Time.unscaledDeltaTime;
		Player opposingPlayer = GameState.Get().GetOpposingSidePlayer();
		if (opposingPlayer != null)
		{
			Card opposingHeroCard = opposingPlayer.GetHeroCard();
			if (!(opposingHeroCard == null))
			{
				UpdateTimeSinceEmoteFinished(opposingHeroCard, m_timeSinceEmoteFinishedOpposing, m_keyCollectionOpposing);
				UpdateTimeSinceEmoteFinished(GameState.Get().GetFriendlySidePlayer().GetHeroCard(), m_timeSinceEmoteFinishedFriendly, m_keyCollectionFriendly);
			}
		}
	}

	public void UpdateTimeSinceEmoteFinished(Card heroCard, Map<EmoteType, float> timeSinceEmoteFinished, Map<EmoteType, float>.KeyCollection keyCollection)
	{
		if (heroCard == null)
		{
			return;
		}
		EmoteEntry currentlyActiveEmote = heroCard.GetActiveEmoteSound();
		if (currentlyActiveEmote != null)
		{
			timeSinceEmoteFinished[currentlyActiveEmote.GetEmoteType()] = 0f;
		}
		m_keyType.Clear();
		m_keyType.AddRange(keyCollection);
		foreach (EmoteType key in m_keyType)
		{
			if (currentlyActiveEmote != null && key == currentlyActiveEmote.GetEmoteType())
			{
				timeSinceEmoteFinished[key] = 0f;
				continue;
			}
			float currentTime = timeSinceEmoteFinished[key];
			currentTime = (timeSinceEmoteFinished[key] = currentTime + Time.unscaledDeltaTime);
			if (currentTime > 8f)
			{
				timeSinceEmoteFinished.Remove(key);
			}
		}
	}

	public static EmoteHandler Get()
	{
		return s_instance;
	}

	public void ChangeAvailableEmotes()
	{
		HideEmotes();
		DetermineAvailableEmotes();
	}

	public void ResetTimeSinceLastEmote()
	{
		if (m_timeSinceLastEmote < 9f)
		{
			m_chainedEmotes++;
		}
		else
		{
			m_chainedEmotes = 0;
		}
		m_timeSinceLastEmote = 0f;
	}

	public bool IsResponseEmote(EmoteType type)
	{
		return type == EmoteType.MIRROR_GREETINGS;
	}

	public bool ShouldUseEmoteResponse(EmoteType desiredEmoteType, Player.Side heroSide)
	{
		Card heroCard = GameState.Get().GetOpposingSidePlayer().GetHeroCard();
		Card friendlyHeroCard = GameState.Get().GetFriendlySidePlayer().GetHeroCard();
		EmoteEntry myEmoteOpposing = heroCard.GetEmoteEntry(desiredEmoteType);
		if (myEmoteOpposing != null && !string.IsNullOrEmpty(myEmoteOpposing.GetGameStringKey()))
		{
			EmoteEntry myEmoteFriendly = friendlyHeroCard.GetEmoteEntry(desiredEmoteType);
			if (myEmoteFriendly != null && myEmoteFriendly.GetGameStringKey() != myEmoteOpposing.GetGameStringKey())
			{
				return false;
			}
		}
		float time = 0f;
		float timeLimit = 6f;
		if (heroSide == Player.Side.FRIENDLY)
		{
			if (!m_timeSinceEmoteFinishedOpposing.TryGetValue(desiredEmoteType, out time))
			{
				return false;
			}
		}
		else
		{
			if (!m_timeSinceEmoteFinishedFriendly.TryGetValue(desiredEmoteType, out time))
			{
				return false;
			}
			timeLimit += 2f;
		}
		return time <= timeLimit;
	}

	public EmoteType GetEmoteResponseType(EmoteType desiredEmoteType)
	{
		if (desiredEmoteType == EmoteType.GREETINGS)
		{
			return EmoteType.MIRROR_GREETINGS;
		}
		return EmoteType.INVALID;
	}

	public EmoteType GetEmoteAntiResponseType(EmoteType desiredEmoteType)
	{
		if (desiredEmoteType == EmoteType.MIRROR_GREETINGS)
		{
			return EmoteType.GREETINGS;
		}
		return EmoteType.INVALID;
	}

	public void ShowEmotes()
	{
		if (m_emotesShown || GameState.Get().IsBusy())
		{
			return;
		}
		m_shownAtFrame = Time.frameCount;
		m_emotesShown = true;
		GetComponent<Collider>().enabled = true;
		foreach (EmoteOption availableEmote in m_availableEmotes)
		{
			availableEmote.Enable();
		}
	}

	public void HideEmotes()
	{
		if (!m_emotesShown)
		{
			return;
		}
		m_mousedOverEmote = null;
		m_emotesShown = false;
		GetComponent<Collider>().enabled = false;
		foreach (EmoteOption availableEmote in m_availableEmotes)
		{
			availableEmote.Disable();
		}
	}

	public bool AreEmotesActive()
	{
		return m_emotesShown;
	}

	public void HandleInput()
	{
		if (!HitTestEmotes(out var hitInfo))
		{
			HideEmotes();
			return;
		}
		if (GameState.Get().IsBusy())
		{
			HideEmotes();
			return;
		}
		EmoteOption emoteOption = hitInfo.transform.gameObject.GetComponent<EmoteOption>();
		if (emoteOption == null)
		{
			if (m_mousedOverEmote != null)
			{
				m_mousedOverEmote.HandleMouseOut();
				m_mousedOverEmote = null;
			}
		}
		else if (m_mousedOverEmote == null)
		{
			m_mousedOverEmote = emoteOption;
			m_mousedOverEmote.HandleMouseOver();
		}
		else if (m_mousedOverEmote != emoteOption)
		{
			m_mousedOverEmote.HandleMouseOut();
			m_mousedOverEmote = emoteOption;
			emoteOption.HandleMouseOver();
		}
		if (!InputCollection.GetMouseButtonUp(0))
		{
			return;
		}
		if (m_mousedOverEmote != null)
		{
			if (EmoteSpamBlocked())
			{
				return;
			}
			m_totalEmotes++;
			if (GameState.Get().GetGameEntity().HasTag(GAME_TAG.ALL_TARGETS_RANDOM))
			{
				List<EmoteOption> useableEmotes = new List<EmoteOption>();
				foreach (EmoteOption emote in m_availableEmotes.Concat(m_HiddenEmotes))
				{
					if (emote.CanPlayerUseEmoteType(GameState.Get().GetFriendlySidePlayer()))
					{
						useableEmotes.Add(emote);
					}
				}
				if (useableEmotes.Count > 0)
				{
					int chosenEmote = Random.Range(0, useableEmotes.Count);
					useableEmotes[chosenEmote].DoClick();
				}
				else
				{
					Log.All.PrintError("EmoteHandler.HandleInput() - No usable emotes exist.");
				}
			}
			else
			{
				m_mousedOverEmote.DoClick();
			}
		}
		else if (UniversalInputManager.Get().IsTouchMode() && Time.frameCount != m_shownAtFrame)
		{
			HideEmotes();
		}
	}

	public bool IsMouseOverEmoteOption()
	{
		if (UniversalInputManager.Get().GetInputHitInfo(GameLayer.Default.LayerBit(), out var hitInfo) && hitInfo.transform.gameObject.GetComponent<EmoteOption>() != null)
		{
			return true;
		}
		return false;
	}

	private bool IsVisualEmoteUnfinished()
	{
		if (GameState.Get() == null)
		{
			return false;
		}
		Player player = GameState.Get().GetFriendlySidePlayer();
		if (player == null)
		{
			return false;
		}
		Card currentHeroCard = player.GetHeroCard();
		if (currentHeroCard != null && currentHeroCard.HasUnfinishedEmoteSpell())
		{
			return true;
		}
		return false;
	}

	public bool EmoteSpamBlocked()
	{
		if (IsVisualEmoteUnfinished())
		{
			return true;
		}
		if (GameMgr.Get().IsFriendly() || GameMgr.Get().IsAI())
		{
			return false;
		}
		if (m_totalEmotes >= 25)
		{
			return m_timeSinceLastEmote < 45f;
		}
		if (m_totalEmotes >= 20)
		{
			return m_timeSinceLastEmote < 15f;
		}
		if (m_chainedEmotes >= 2)
		{
			return m_timeSinceLastEmote < 15f;
		}
		return m_timeSinceLastEmote < 4f;
	}

	public bool IsValidEmoteTypeForOpponent(EmoteType emoteType)
	{
		List<EmoteOption> opponentAvailableEmotes = new List<EmoteOption>();
		foreach (EmoteOption emote in m_DefaultEmotes)
		{
			opponentAvailableEmotes.Add(emote);
		}
		Player opponentPlayer = GameState.Get().GetOpposingSidePlayer();
		for (int i = 0; i < 6; i++)
		{
			int overrideValue = opponentPlayer.GetTag((GAME_TAG)(740 + i));
			if (overrideValue > 0 && overrideValue < m_EmoteOverrides.Count && m_EmoteOverrides[overrideValue].CanPlayerUseEmoteType(opponentPlayer))
			{
				opponentAvailableEmotes[i] = m_EmoteOverrides[overrideValue];
			}
		}
		foreach (EmoteOption item in opponentAvailableEmotes)
		{
			if (item.HasEmoteTypeForPlayer(emoteType, opponentPlayer))
			{
				return true;
			}
		}
		if (GameState.Get().GetGameEntity().HasTag(GAME_TAG.ALL_TARGETS_RANDOM))
		{
			foreach (EmoteOption hiddenEmote in m_HiddenEmotes)
			{
				if (hiddenEmote.HasEmoteTypeForPlayer(emoteType, opponentPlayer))
				{
					return true;
				}
			}
		}
		if (IsResponseEmote(emoteType))
		{
			EmoteType baseType = GetEmoteAntiResponseType(emoteType);
			if (ShouldUseEmoteResponse(baseType, Player.Side.OPPOSING))
			{
				return IsValidEmoteTypeForOpponent(baseType);
			}
		}
		return false;
	}

	private void OnHeroChanged(Player player, object userData)
	{
		if (!player.IsFriendlySide())
		{
			return;
		}
		DetermineAvailableEmotes();
		foreach (EmoteOption availableEmote in m_availableEmotes)
		{
			availableEmote.UpdateEmoteType();
		}
	}

	private bool HitTestEmotes(out RaycastHit hitInfo)
	{
		if (!UniversalInputManager.Get().GetInputHitInfo(GameLayer.CardRaycast.LayerBit(), out hitInfo))
		{
			return false;
		}
		if (IsMousedOverHero(hitInfo))
		{
			return true;
		}
		if (IsMousedOverSelf(hitInfo))
		{
			return true;
		}
		if (IsMousedOverEmote(hitInfo))
		{
			return true;
		}
		return false;
	}

	private bool IsMousedOverHero(RaycastHit cardHitInfo)
	{
		Actor actor = GameObjectUtils.FindComponentInParents<Actor>(cardHitInfo.transform);
		if (actor == null)
		{
			return false;
		}
		Card card = actor.GetCard();
		if (card == null)
		{
			return false;
		}
		if (card.GetEntity().IsHero() && card.GetZone() is ZoneHero)
		{
			return true;
		}
		return false;
	}

	private bool IsMousedOverSelf(RaycastHit cardHitInfo)
	{
		return GetComponent<Collider>() == cardHitInfo.collider;
	}

	private bool IsMousedOverEmote(RaycastHit cardHitInfo)
	{
		foreach (EmoteOption emote in m_availableEmotes)
		{
			if (cardHitInfo.transform == emote.transform)
			{
				return true;
			}
		}
		return false;
	}

	public IEnumerator PlayStartingTaunts(GameObject mulliganGO)
	{
		Card heroCard = GameState.Get().GetOpposingSidePlayer().GetHeroCard();
		Card heroPowerCard = GameState.Get().GetOpposingSidePlayer().GetHeroPowerCard();
		iTween.StopByName(mulliganGO, "HisHeroLightBlend");
		if (heroPowerCard != null)
		{
			while (!heroPowerCard.GetActor().IsShown())
			{
				yield return null;
			}
			GameState.Get().GetGameEntity().FadeInActor(heroPowerCard.GetActor(), 0.4f);
		}
		while (!heroCard.GetActor().IsShown())
		{
			yield return null;
		}
		GameState.Get().GetGameEntity().FadeInHeroActor(heroCard.GetActor());
		EmoteEntry startEmoteEntry = heroCard.GetEmoteEntry(EmoteType.START);
		bool shouldPlayStartEmote = true;
		if (startEmoteEntry != null)
		{
			CardSoundSpell startEmoteSpell = startEmoteEntry.GetSoundSpell();
			if (startEmoteSpell != null && startEmoteSpell.DetermineBestAudioSource() == null)
			{
				shouldPlayStartEmote = false;
			}
		}
		CardSoundSpell emoteSpell = null;
		if (shouldPlayStartEmote)
		{
			emoteSpell = heroCard.PlayEmote(EmoteType.START);
		}
		if (emoteSpell != null)
		{
			while (emoteSpell.GetActiveState() != 0)
			{
				yield return null;
			}
		}
		else
		{
			yield return new WaitForSeconds(2.5f);
		}
		GameState.Get().GetGameEntity().FadeOutHeroActor(heroCard.GetActor());
		if (heroPowerCard != null)
		{
			GameState.Get().GetGameEntity().FadeOutActor(heroPowerCard.GetActor());
		}
		Card myHeroCard = GameState.Get().GetFriendlySidePlayer().GetHeroCard();
		Card myHeroPowerCard = GameState.Get().GetFriendlySidePlayer().GetHeroPowerCard();
		iTween.StopByName(mulliganGO, "MyHeroLightBlend");
		if (myHeroPowerCard != null)
		{
			GameState.Get().GetGameEntity().FadeInActor(myHeroPowerCard.GetActor(), 0.4f);
		}
		EmoteType emoteToPlay = EmoteType.START;
		EmoteEntry myEmoteEntry = myHeroCard.GetEmoteEntry(EmoteType.START);
		if (myEmoteEntry != null && !string.IsNullOrEmpty(myEmoteEntry.GetGameStringKey()))
		{
			EmoteEntry opEmoteEntry = heroCard.GetEmoteEntry(EmoteType.START);
			if (opEmoteEntry != null && myEmoteEntry.GetGameStringKey() == opEmoteEntry.GetGameStringKey())
			{
				emoteToPlay = EmoteType.MIRROR_START;
			}
		}
		while (!myHeroCard.GetActor().IsShown())
		{
			yield return null;
		}
		GameState.Get().GetGameEntity().FadeInHeroActor(myHeroCard.GetActor());
		emoteSpell = myHeroCard.PlayEmote(emoteToPlay, Notification.SpeechBubbleDirection.BottomRight);
		if (emoteSpell != null)
		{
			while (emoteSpell.GetActiveState() != 0)
			{
				yield return null;
			}
		}
		else
		{
			yield return new WaitForSeconds(2.5f);
		}
		GameState.Get().GetGameEntity().FadeOutHeroActor(myHeroCard.GetActor());
		if (myHeroPowerCard != null)
		{
			GameState.Get().GetGameEntity().FadeOutActor(myHeroPowerCard.GetActor());
		}
	}
}
