using System;
using System.Collections.Generic;
using Hearthstone.DataModels;
using Hearthstone.UI;
using Unity.Mathematics;
using UnityEngine;

[CustomEditClass]
public class CardActorAudioDriver : MonoBehaviour
{
	private enum AudioSourceType
	{
		None,
		Hero,
		BG_Hero,
		BG_Bartender
	}

	public enum AudioType
	{
		None,
		Emote,
		CardEffect,
		Bartender
	}

	public enum CardEffectType
	{
		None,
		Play,
		Attack,
		Death,
		Lifetime
	}

	[Serializable]
	public class AudioRequestItem
	{
		public AudioType Type;

		[CustomEditField(HidePredicate = "ShouldHideEmoteTypes")]
		public EmoteType EmoteType;

		[CustomEditField(HidePredicate = "ShouldHideCardEffectTypes")]
		public CardEffectType CardEffectType;

		[CustomEditField(HidePredicate = "ShouldHideBartenderLineType")]
		public BaconGuideConfig.HumanReadableVOLineCategory BartenderLineType;
	}

	private const string WIDGET_PLAY_EVENT = "PLAY_AUDIO";

	[CustomEditField(ListTable = true)]
	public List<AudioRequestItem> m_AudioRequests;

	public Widget m_EventReceivingWidget;

	[Tooltip("Reference to the Widget behaviour Card component that we want to derive emotes from.")]
	public AsyncReference m_AsyncCardParentWidgetReference;

	[Min(0f)]
	public float m_PlayDelaySeconds = 0.5f;

	[Tooltip("Should we play random audio from pool or play in loaded sequence?")]
	public bool m_IsRandom;

	private readonly List<AudioSource> m_audioSources = new List<AudioSource>();

	private readonly List<GameObject> m_cachedInstantiatedObjects = new List<GameObject>();

	private AudioSourceType m_sourceType;

	private Hearthstone.UI.Card m_dataCard;

	private Actor m_actor;

	private string m_cardId;

	private DateTime m_minNextPlayTime;

	private int m_previouslyPlayedAudioIndex = -1;

	private bool m_hasTriedToLoadAudio;

	public bool IsReady()
	{
		return m_actor != null;
	}

	public void Play()
	{
		if (!IsReady() || DateTime.UtcNow < m_minNextPlayTime)
		{
			return;
		}
		if (!m_hasTriedToLoadAudio)
		{
			LoadSoundData();
		}
		if (m_AudioRequests.Count == 0)
		{
			Debug.LogWarning("CardActorAudioDriver has no audio play, requesting ignored...");
			return;
		}
		SoundManager soundMgr = SoundManager.Get();
		if (soundMgr != null)
		{
			if (m_previouslyPlayedAudioIndex >= 0 && m_previouslyPlayedAudioIndex < m_audioSources.Count)
			{
				soundMgr.Stop(m_audioSources[m_previouslyPlayedAudioIndex]);
			}
			int nextIndex;
			if (m_IsRandom && m_audioSources.Count > 2)
			{
				int unclampedIndex = UnityEngine.Random.Range(1, m_audioSources.Count) + m_previouslyPlayedAudioIndex;
				nextIndex = ((unclampedIndex < m_audioSources.Count) ? math.max(unclampedIndex, 0) : math.max(unclampedIndex - m_audioSources.Count, 0));
			}
			else
			{
				nextIndex = ((m_previouslyPlayedAudioIndex < m_audioSources.Count - 1) ? Math.Min(++m_previouslyPlayedAudioIndex, m_audioSources.Count - 1) : 0);
			}
			AudioSource audioSource = m_audioSources[nextIndex];
			if (audioSource == null)
			{
				Debug.LogWarning(string.Format("{0} found null cached {1} at index: {2}... maybe pooled object got cleaned up?", "CardActorAudioDriver", "AudioSource", nextIndex));
				return;
			}
			soundMgr.Play(audioSource);
			m_previouslyPlayedAudioIndex = nextIndex;
			m_minNextPlayTime = DateTime.UtcNow + TimeSpan.FromSeconds(math.max(m_PlayDelaySeconds, 0f));
		}
	}

	private void Start()
	{
		m_AsyncCardParentWidgetReference.RegisterReadyListener<Hearthstone.UI.Card>(OnCardParentWidgetReady);
		m_EventReceivingWidget.RegisterEventListener(OnWidgetEventReceived);
	}

	private void OnDestroy()
	{
		if (m_EventReceivingWidget != null)
		{
			m_EventReceivingWidget.RemoveEventListener(OnWidgetEventReceived);
		}
		if (m_dataCard != null)
		{
			m_dataCard.UnregisterCardLoadedListener(OnDataCardLoaded);
		}
		CleanUpData();
	}

	private void CleanUpData()
	{
		m_actor = null;
		m_hasTriedToLoadAudio = false;
		m_previouslyPlayedAudioIndex = -1;
		m_audioSources.Clear();
		foreach (GameObject cachedInstantiatedObject in m_cachedInstantiatedObjects)
		{
			UnityEngine.Object.Destroy(cachedInstantiatedObject);
		}
		m_cachedInstantiatedObjects.Clear();
	}

	private void OnWidgetEventReceived(string eventName)
	{
		if (base.enabled && base.gameObject.activeInHierarchy && eventName.Equals("PLAY_AUDIO", StringComparison.OrdinalIgnoreCase))
		{
			if (!IsReady())
			{
				Log.Store.PrintWarning("CardActorAudioDriver ignored " + eventName + " event as it was ready...");
			}
			else
			{
				Play();
			}
		}
	}

	private void OnCardParentWidgetReady(Hearthstone.UI.Card dataCard)
	{
		if (dataCard == null)
		{
			Debug.LogError("CardActorAudioDriver: Widget for card parent was null.");
			return;
		}
		CardDataModel cardDataModel = m_EventReceivingWidget.GetDataModel<CardDataModel>();
		if (cardDataModel != null)
		{
			m_cardId = cardDataModel.CardId;
		}
		if (!(dataCard is HeroSkin))
		{
			if (!(dataCard is BaconHeroSkin))
			{
				if (!(dataCard is BaconGuideSkin))
				{
					Debug.LogError(string.Format("{0}: Unhandled data card type: {1} - aborting...", "CardActorAudioDriver", dataCard.GetType()));
					return;
				}
				m_sourceType = AudioSourceType.BG_Bartender;
			}
			else
			{
				m_sourceType = AudioSourceType.BG_Hero;
			}
		}
		else
		{
			m_sourceType = AudioSourceType.Hero;
		}
		m_dataCard = dataCard;
		dataCard.RegisterCardLoadedListener(OnDataCardLoaded);
		if (!(m_dataCard.CardActor == null))
		{
			TryLoadCardActorData(m_dataCard.CardActor);
		}
	}

	private void OnDataCardLoaded(Actor cardActor)
	{
		if (cardActor == null)
		{
			Debug.LogError("CardActorAudioDriver: Failed to find Ui.Card actor.");
		}
		else
		{
			TryLoadCardActorData(cardActor);
		}
	}

	private void TryLoadCardActorData(Actor cardActor)
	{
		CleanUpData();
		if (cardActor == null)
		{
			Debug.LogError("CardActorAudioDriver: Failed to load card actor as passed null.");
			return;
		}
		if (m_sourceType == AudioSourceType.None)
		{
			Debug.LogError("CardActorAudioDriver: Failed to load card actor data due to invalid source type");
			return;
		}
		m_audioSources.Clear();
		m_actor = cardActor;
	}

	private void LoadSoundData()
	{
		if (!IsReady())
		{
			return;
		}
		m_hasTriedToLoadAudio = true;
		if (m_AudioRequests == null)
		{
			return;
		}
		if (string.IsNullOrEmpty(m_cardId))
		{
			CardDataModel cardDataModel = m_EventReceivingWidget.GetDataModel<CardDataModel>();
			if (cardDataModel == null)
			{
				Debug.LogWarning("CardActorAudioDriver failed to find CardId for Actor, unable to load sound data!");
				return;
			}
			m_cardId = cardDataModel.CardId;
		}
		switch (m_sourceType)
		{
		case AudioSourceType.Hero:
		case AudioSourceType.BG_Hero:
			LoadSoundDataForHero();
			break;
		case AudioSourceType.BG_Bartender:
			LoadSoundDataForBattlegroundsGuide();
			break;
		}
	}

	private void LoadSoundDataForHero()
	{
		CardDef cardDef = m_actor.ShareDisposableCardDef().CardDef;
		foreach (AudioRequestItem request in m_AudioRequests)
		{
			if (request.Type == AudioType.Bartender)
			{
				continue;
			}
			if (request.Type == AudioType.Emote)
			{
				if (cardDef.m_EmoteDefs == null)
				{
					continue;
				}
				foreach (EmoteEntryDef emoteDef in cardDef.m_EmoteDefs)
				{
					if (emoteDef.m_emoteType != request.EmoteType)
					{
						continue;
					}
					CardSoundSpell soundSpell = new EmoteEntry(emoteDef.m_emoteType, emoteDef.m_emoteSpellPath, emoteDef.m_emoteSoundSpellPath, emoteDef.m_emoteGameStringKey, m_actor).GetSoundSpell();
					if (soundSpell == null)
					{
						Debug.LogWarning(string.Format("{0} failed to pull sound spell for emote: {1} for card: {2}", "CardActorAudioDriver", request.EmoteType, m_cardId));
						return;
					}
					AudioSource audioSource = soundSpell.DetermineBestAudioSource();
					if (audioSource == null)
					{
						Debug.LogWarning(string.Format("{0} failed to pull audio source for emote: {1} for card: {2}", "CardActorAudioDriver", request.EmoteType, m_cardId));
						return;
					}
					m_audioSources.Add(audioSource);
					break;
				}
			}
			else
			{
				if (request.Type != AudioType.CardEffect)
				{
					continue;
				}
				CardEffectDef cardEffectDef = GetCardEffectDefFromRequestType(cardDef, request.CardEffectType);
				if (cardEffectDef == null)
				{
					continue;
				}
				List<CardSoundSpell> soundSpells = new CardEffect(cardEffectDef, null).GetSoundSpells();
				if (soundSpells == null)
				{
					continue;
				}
				foreach (CardSoundSpell item in soundSpells)
				{
					SpellUtils.SetupSoundSpell(item, m_dataCard);
					AudioSource audioSource2 = item.DetermineBestAudioSource();
					if (!(audioSource2 == null))
					{
						m_audioSources.Add(audioSource2);
					}
				}
			}
		}
	}

	private void LoadSoundDataForBattlegroundsGuide()
	{
		if (m_AudioRequests.Count == 0)
		{
			return;
		}
		BaconGuideConfig config = TB_BaconShop.LoadGuideConfig(m_cardId);
		foreach (AudioRequestItem request in m_AudioRequests)
		{
			List<string> voRefs = config.GetLinesByHumanReadableCategory(request.BartenderLineType);
			if (voRefs == null)
			{
				Debug.LogWarning(string.Format("{0} failed to pull audio source for bartender VO: {1} for card: {2}", "CardActorAudioDriver", request.BartenderLineType, m_cardId));
				continue;
			}
			foreach (string voRef in voRefs)
			{
				if (string.IsNullOrEmpty(voRef))
				{
					continue;
				}
				GameObject go = SoundLoader.LoadSound(voRef);
				if (go == null)
				{
					continue;
				}
				AudioSource audioSource = go.GetComponent<AudioSource>();
				if (audioSource == null)
				{
					UnityEngine.Object.Destroy(go);
					Debug.LogWarning(string.Format("{0} failed to pull audio source for emote: {1} for card: {2}", "CardActorAudioDriver", request.Type, m_cardId));
					continue;
				}
				m_cachedInstantiatedObjects.Add(go);
				m_audioSources.Add(audioSource);
				if (request.BartenderLineType != BaconGuideConfig.HumanReadableVOLineCategory.All)
				{
					continue;
				}
				break;
			}
		}
	}

	private static CardEffectDef GetCardEffectDefFromRequestType(CardDef cardDef, CardEffectType type)
	{
		if (cardDef == null)
		{
			return null;
		}
		switch (type)
		{
		case CardEffectType.Play:
			return cardDef.m_PlayEffectDef;
		case CardEffectType.Attack:
			return cardDef.m_AttackEffectDef;
		case CardEffectType.Death:
			return cardDef.m_DeathEffectDef;
		case CardEffectType.Lifetime:
			return cardDef.m_LifetimeEffectDef;
		default:
			Debug.LogError(string.Format("{0} Unhandled type for card effect def {1}", "CardActorAudioDriver", type));
			return null;
		}
	}
}
