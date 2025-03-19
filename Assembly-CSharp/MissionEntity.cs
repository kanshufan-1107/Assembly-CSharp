using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class MissionEntity : GameEntity
{
	protected class EmoteResponse
	{
		public string m_soundName;

		public string m_stringTag;
	}

	protected class EmoteResponseGroup
	{
		public List<EmoteType> m_triggers = new List<EmoteType>();

		public List<EmoteResponse> m_responses = new List<EmoteResponse>();

		public int m_responseIndex;
	}

	protected enum ShouldPlayValue
	{
		Never,
		Once,
		Always
	}

	protected delegate ShouldPlayValue ShouldPlay();

	public delegate IEnumerator OnChangeHandler(TAG_PLAYSTATE gameResult);

	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static Map<GameEntityOption, string> s_stringOptions = InitStringOptions();

	protected const float TIME_TO_WAIT_BEFORE_ENDING_QUOTE = 5f;

	protected const float MINIMUM_DISPLAY_TIME_FOR_BIG_QUOTE = 3f;

	protected const float DEFAULT_VO_DURATION = 2.5f;

	protected static readonly List<EmoteType> STANDARD_EMOTE_RESPONSE_TRIGGERS = new List<EmoteType>
	{
		EmoteType.GREETINGS,
		EmoteType.WELL_PLAYED,
		EmoteType.OOPS,
		EmoteType.SORRY,
		EmoteType.THANKS,
		EmoteType.THREATEN,
		EmoteType.WOW,
		EmoteType.FIRE_FESTIVAL_FIREWORKS_RANK_ONE,
		EmoteType.FIRE_FESTIVAL_FIREWORKS_RANK_TWO,
		EmoteType.FIRE_FESTIVAL_FIREWORKS_RANK_THREE,
		EmoteType.FROST_FESTIVAL_FIREWORKS_RANK_ONE,
		EmoteType.FROST_FESTIVAL_FIREWORKS_RANK_TWO,
		EmoteType.FROST_FESTIVAL_FIREWORKS_RANK_THREE,
		EmoteType.HAPPY_HALLOWEEN,
		EmoteType.HAPPY_NEW_YEAR
	};

	protected bool m_enemySpeaking;

	protected List<EmoteResponseGroup> m_emoteResponseGroups = new List<EmoteResponseGroup>();

	protected Notification m_ActiveSpeechBubble;

	protected VoPlaybackHandler m_voHandler;

	public bool m_forceAlwaysPlayLine;

	private HashSet<string> m_InOrderPlayedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.USE_SECRET_CLASS_NAMES,
			true
		} };
	}

	private static Map<GameEntityOption, string> InitStringOptions()
	{
		return new Map<GameEntityOption, string>();
	}

	public MissionEntity(VoPlaybackHandler voHandler = null)
	{
		m_voHandler = voHandler;
		if (voHandler != null)
		{
			voHandler.Coroutines = GameEntity.Coroutines;
		}
		m_gameOptions.AddOptions(s_booleanOptions, s_stringOptions);
		InitEmoteResponses();
	}

	public override void OnCreate()
	{
		base.OnCreate();
		if (m_voHandler != null)
		{
			m_voHandler.PreloadAssets();
			m_voHandler.OnCreateGame();
		}
	}

	public override void OnTagChanged(TagDelta change)
	{
		switch ((GAME_TAG)change.tag)
		{
		case GAME_TAG.NEXT_STEP:
			if (change.newValue == 6)
			{
				HandleMainReadyStep();
			}
			else if (change.newValue == 10 && (change.oldValue == 9 || change.oldValue == 19) && GameState.Get().IsLocalSidePlayerTurn())
			{
				TurnStartManager.Get().BeginPlayingTurnEvents();
			}
			break;
		case GAME_TAG.STEP:
			if (change.newValue == 4)
			{
				HandleMulliganTagChange();
			}
			else if (change.newValue == 10 && (change.oldValue == 9 || change.oldValue == 19) && !GameState.Get().IsFriendlySidePlayerTurn())
			{
				HandleStartOfTurn(GetTag(GAME_TAG.TURN));
			}
			break;
		case GAME_TAG.MISSION_EVENT:
			HandleMissionEvent(change.newValue);
			break;
		}
		base.OnTagChanged(change);
	}

	public override void NotifyOfStartOfTurnEventsFinished()
	{
		HandleStartOfTurn(GetTag(GAME_TAG.TURN));
	}

	public override void SendCustomEvent(int eventID)
	{
		HandleMissionEvent(eventID);
	}

	public override void NotifyOfOpponentWillPlayCard(string cardId, Entity playedEntity)
	{
		base.NotifyOfOpponentWillPlayCard(cardId, playedEntity);
		if (m_voHandler != null)
		{
			GameEntity.Coroutines.StartCoroutine(m_voHandler.RespondToWillPlayCardWithTiming(cardId, playedEntity));
		}
		else
		{
			GameEntity.Coroutines.StartCoroutine(RespondToWillPlayCardWithTiming(cardId, playedEntity));
		}
	}

	public override void NotifyOfOpponentPlayedCard(Entity entity)
	{
		base.NotifyOfOpponentPlayedCard(entity);
		if (m_voHandler != null)
		{
			GameEntity.Coroutines.StartCoroutine(m_voHandler.RespondToPlayedCardWithTiming(entity));
		}
		else
		{
			GameEntity.Coroutines.StartCoroutine(RespondToPlayedCardWithTiming(entity));
		}
	}

	public override void NotifyOfFriendlyPlayedCard(Entity entity)
	{
		base.NotifyOfFriendlyPlayedCard(entity);
		if (m_voHandler != null)
		{
			GameEntity.Coroutines.StartCoroutine(m_voHandler.RespondToFriendlyPlayedCardWithTiming(entity));
		}
		else
		{
			GameEntity.Coroutines.StartCoroutine(RespondToFriendlyPlayedCardWithTiming(entity));
		}
	}

	public override void NotifyOfGameOver(TAG_PLAYSTATE gameResult)
	{
		base.NotifyOfGameOver(gameResult);
		if (m_voHandler != null)
		{
			GameEntity.Coroutines.StartCoroutine(m_voHandler.HandleGameOverWithTiming(gameResult, HandleGameOverWithTiming));
		}
		else
		{
			GameEntity.Coroutines.StartCoroutine(HandleGameOverWithTiming(gameResult));
		}
	}

	public override void NotifyOfResetGameStarted()
	{
		base.NotifyOfResetGameStarted();
		GameEntity.Coroutines.StopAllCoroutines();
	}

	public override void NotifyOfResetGameFinished(Entity source, Entity oldGameEntity)
	{
		base.NotifyOfResetGameFinished(source, oldGameEntity);
		if (m_voHandler != null)
		{
			GameEntity.Coroutines.StartCoroutine(m_voHandler.RespondToResetGameFinishedWithTiming(source));
		}
		else
		{
			GameEntity.Coroutines.StartCoroutine(RespondToResetGameFinishedWithTiming(source));
		}
	}

	public override void OnEmotePlayed(Card card, EmoteType emoteType, CardSoundSpell emoteSpell)
	{
		if (card.GetEntity().IsControlledByFriendlySidePlayer())
		{
			if (m_voHandler != null)
			{
				GameEntity.Coroutines.StartCoroutine(m_voHandler.HandlePlayerEmoteWithTiming(emoteType, emoteSpell));
			}
			else
			{
				GameEntity.Coroutines.StartCoroutine(HandlePlayerEmoteWithTiming(emoteType, emoteSpell));
			}
		}
	}

	public override bool DoAlternateMulliganIntro()
	{
		if (!ShouldDoAlternateMulliganIntro())
		{
			return false;
		}
		GameEntity.Coroutines.StartCoroutine(SkipStandardMulliganWithTiming());
		return true;
	}

	public bool IsHeroic()
	{
		return GameMgr.Get().IsHeroicMission();
	}

	public bool IsClassChallenge()
	{
		return GameMgr.Get().IsClassChallengeMission();
	}

	public override void NotifyOfEntityAttacked(Entity attacker, Entity defender)
	{
		if (m_voHandler != null)
		{
			m_voHandler.NotifyOfEntityAttacked(attacker, defender);
		}
	}

	public override void NotifyOfMinionPlayed(Entity minion)
	{
		if (m_voHandler != null)
		{
			m_voHandler.NotifyOfMinionPlayed(minion);
		}
	}

	public override void NotifyOfHeroChanged(Entity newHero)
	{
		if (m_voHandler != null)
		{
			m_voHandler.NotifyOfHeroChanged(newHero);
		}
	}

	public override void NotifyOfWeaponEquipped(Entity weapon)
	{
		if (m_voHandler != null)
		{
			m_voHandler.NotifyOfWeaponEquipped(weapon);
		}
	}

	public override void NotifyOfSpellPlayed(Entity spell, Entity target)
	{
		if (m_voHandler != null)
		{
			m_voHandler.NotifyOfSpellPlayed(spell, target);
		}
	}

	public override void NotifyOfHeroPowerUsed(Entity heroPower, Entity target)
	{
		if (m_voHandler != null)
		{
			m_voHandler.NotifyOfHeroPowerUsed(heroPower, target);
		}
	}

	public override void NotifyOfMinionDied(Entity minion)
	{
		if (m_voHandler != null)
		{
			m_voHandler.NotifyOfMinionDied(minion);
		}
	}

	public override void NotifyOfHeroDied(Entity hero)
	{
		if (m_voHandler != null)
		{
			m_voHandler.NotifyOfHeroDied(hero);
		}
	}

	public override void NotifyOfWeaponDestroyed(Entity weapon)
	{
		if (m_voHandler != null)
		{
			m_voHandler.NotifyOfWeaponDestroyed(weapon);
		}
	}

	protected virtual void HandleMainReadyStep()
	{
		if (GameState.Get() == null)
		{
			Log.Gameplay.PrintError("MissionEntity.HandleMainReadyStep(): GameState is null.");
			return;
		}
		GameEntity gameEntity = GameState.Get().GetGameEntity();
		if (gameEntity == null)
		{
			Log.Gameplay.PrintError("MissionEntity.HandleMainReadyStep(): GameEntity is null.");
		}
		else
		{
			if (gameEntity.GetTag(GAME_TAG.TURN) != 1)
			{
				return;
			}
			if (GameState.Get().IsMulliganManagerActive())
			{
				GameState.Get().SetMulliganBusy(busy: true);
			}
			else if (!ShouldDoAlternateMulliganIntro())
			{
				GameState.Get().SetMulliganBusy(busy: true);
				if (MulliganManager.Get() != null)
				{
					MulliganManager.Get().SkipMulligan();
				}
			}
		}
	}

	public void SetBlockVo(bool shouldBlock, float unblockAfterSeconds = 0f)
	{
		if (unblockAfterSeconds < 0f)
		{
			unblockAfterSeconds = 0f;
		}
		if (!shouldBlock)
		{
			m_enemySpeaking = shouldBlock;
		}
		if (shouldBlock)
		{
			if (m_voHandler != null)
			{
				GameEntity.Coroutines.StartCoroutine(m_voHandler.UnblockSpeechAgainAfterDuration(unblockAfterSeconds));
			}
			else
			{
				GameEntity.Coroutines.StartCoroutine(UnblockSpeechAgainAfterDuration(unblockAfterSeconds));
			}
		}
	}

	private IEnumerator UnblockSpeechAgainAfterDuration(float durationInSeconds)
	{
		if (durationInSeconds <= 0f)
		{
			m_enemySpeaking = false;
			yield break;
		}
		while (m_enemySpeaking)
		{
			yield return null;
		}
		m_enemySpeaking = true;
		yield return new WaitForSeconds(durationInSeconds);
		m_enemySpeaking = false;
	}

	protected virtual void HandleMulliganTagChange()
	{
		MulliganManager.Get().BeginMulligan();
	}

	protected void HandleStartOfTurn(int turn)
	{
		if (GameState.Get().GetGameEntity().GetTag(GAME_TAG.IS_CURRENT_TURN_AN_EXTRA_TURN) == 0)
		{
			int turnCountWithoutExtraTurns = turn - GameState.Get().GetGameEntity().GetTag(GAME_TAG.EXTRA_TURNS_TAKEN_THIS_GAME);
			if (m_voHandler != null)
			{
				GameEntity.Coroutines.StartCoroutine(m_voHandler.HandleStartOfTurnWithTiming(turnCountWithoutExtraTurns));
			}
			else
			{
				GameEntity.Coroutines.StartCoroutine(HandleStartOfTurnWithTiming(turnCountWithoutExtraTurns));
			}
		}
	}

	protected virtual IEnumerator HandleStartOfTurnWithTiming(int turn)
	{
		yield break;
	}

	protected void HandleMissionEvent(int missionEvent)
	{
		GameEntity.Coroutines.StartCoroutine(HandleVoThenMissionEventWithTiming(missionEvent));
	}

	protected IEnumerator HandleVoThenMissionEventWithTiming(int missionEvent)
	{
		if (m_voHandler != null)
		{
			yield return m_voHandler.HandleMissionEventWithTiming(missionEvent);
		}
		yield return HandleMissionEventWithTiming(missionEvent);
	}

	protected virtual IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
		yield break;
	}

	protected virtual IEnumerator RespondToWillPlayCardWithTiming(string cardId, Entity playedEntity)
	{
		yield break;
	}

	protected virtual IEnumerator RespondToPlayedCardWithTiming(Entity entity)
	{
		yield break;
	}

	protected virtual IEnumerator RespondToFriendlyPlayedCardWithTiming(Entity entity)
	{
		yield break;
	}

	protected virtual IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
	{
		yield break;
	}

	protected virtual IEnumerator RespondToResetGameFinishedWithTiming(Entity source)
	{
		yield break;
	}

	public override IEnumerator DoActionsBeforeDealingBaseMulliganCards()
	{
		if (m_voHandler != null)
		{
			yield return m_voHandler.DoActionsBeforeDealingBaseMulliganCards();
		}
	}

	protected void PlaySound(string soundPath, float waitTimeScale = 1f, bool parentBubbleToActor = true, bool delayCardSoundSpells = false)
	{
		GameEntity.Coroutines.StartCoroutine(PlaySoundAndWait(soundPath, null, Notification.SpeechBubbleDirection.None, null, waitTimeScale, parentBubbleToActor, delayCardSoundSpells));
	}

	protected IEnumerator PlaySoundAndBlockSpeech(string soundPath, float waitTimeScale = 1f, bool parentBubbleToActor = true, bool delayCardSoundSpells = false)
	{
		m_enemySpeaking = true;
		yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndWait(soundPath, null, Notification.SpeechBubbleDirection.None, null, waitTimeScale, parentBubbleToActor, delayCardSoundSpells));
		m_enemySpeaking = false;
	}

	protected IEnumerator PlaySoundAndBlockSpeechWithCustomGameString(string soundPath, string gameString, Notification.SpeechBubbleDirection direction, Actor actor, float waitTimeScale = 1f, bool parentBubbleToActor = true, bool delayCardSoundSpells = false)
	{
		m_enemySpeaking = true;
		if ((bool)actor && MulliganManager.Get() != null && MulliganManager.Get().IsMulliganActive() && actor.GetEntity() != null && actor.GetEntity().IsHero())
		{
			GameState.Get().GetGameEntity().FadeInHeroActor(actor);
		}
		yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndWait(soundPath, gameString, direction, actor, waitTimeScale, parentBubbleToActor, delayCardSoundSpells));
		if ((bool)actor && MulliganManager.Get() != null && MulliganManager.Get().IsMulliganActive() && actor.GetEntity() != null && actor.GetEntity().IsHero())
		{
			GameState.Get().GetGameEntity().FadeOutHeroActor(actor);
		}
		m_enemySpeaking = false;
	}

	protected IEnumerator PlaySoundAndBlockSpeech(string soundPath, Notification.SpeechBubbleDirection direction, Actor actor, float testingDuration = 3f, float waitTimeScale = 1f, bool parentBubbleToActor = true, bool delayCardSoundSpells = false, float bubbleScale = 0f)
	{
		string gameString = new AssetReference(soundPath).GetLegacyAssetName();
		m_enemySpeaking = true;
		if ((bool)actor && MulliganManager.Get() != null && MulliganManager.Get().IsMulliganActive() && !MulliganManager.Get().IsCustomIntroActive() && actor.GetEntity() != null && actor.GetEntity().IsHero())
		{
			iTween.StopByName(MulliganManager.Get().gameObject, GetMulliganHeroFadeItweenName(actor));
			GameState.Get().GetGameEntity().FadeInHeroActor(actor);
		}
		yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndWait(soundPath, gameString, direction, actor, waitTimeScale, parentBubbleToActor, delayCardSoundSpells, testingDuration, bubbleScale));
		if ((bool)actor && MulliganManager.Get() != null && MulliganManager.Get().IsMulliganActive() && !MulliganManager.Get().IsCustomIntroActive() && actor.GetEntity() != null && actor.GetEntity().IsHero())
		{
			GameState.Get().GetGameEntity().FadeOutHeroActor(actor);
		}
		m_enemySpeaking = false;
	}

	protected IEnumerator PlaySoundAndBlockSpeechOnce(string soundPath, Notification.SpeechBubbleDirection direction, Actor actor, float testingDuration = 3f, float waitTimeScale = 1f, bool parentBubbleToActor = true, bool delayCardSoundSpells = false, float bubbleScale = 0f)
	{
		if (!NotificationManager.Get().HasSoundPlayedThisSession(soundPath))
		{
			NotificationManager.Get().ForceAddSoundToPlayedList(soundPath);
			string gameString = new AssetReference(soundPath).GetLegacyAssetName();
			m_enemySpeaking = true;
			yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndWait(soundPath, gameString, direction, actor, waitTimeScale, parentBubbleToActor, delayCardSoundSpells, testingDuration, bubbleScale));
			m_enemySpeaking = false;
		}
	}

	protected IEnumerator PlaySoundAndWait(string soundPath, string gameString, Notification.SpeechBubbleDirection direction, Actor actor, float waitTimeScale = 1f, bool parentBubbleToActor = true, bool delayCardSoundSpells = false, float testingDuration = 3f, float bubbleScale = 0f)
	{
		AudioSource sound = null;
		bool isJustTesting = false;
		if (string.IsNullOrEmpty(soundPath) || !CheckPreloadedSound(soundPath))
		{
			isJustTesting = true;
		}
		else
		{
			sound = GetPreloadedSound(soundPath);
		}
		if (!isJustTesting && (sound == null || sound.clip == null))
		{
			if (CheckPreloadedSound(soundPath))
			{
				RemovePreloadedSound(soundPath);
				PreloadSound(soundPath);
				while (IsPreloadingAssets())
				{
					yield return null;
				}
				sound = GetPreloadedSound(soundPath);
			}
			if (sound == null || sound.clip == null)
			{
				Log.Sound.PrintDebug("MissionEntity.PlaySoundAndWait() - sound error - " + soundPath);
				yield break;
			}
		}
		float clipLength = testingDuration;
		if (!isJustTesting)
		{
			clipLength = sound.clip.length;
		}
		float waitTime = clipLength * waitTimeScale;
		if (!isJustTesting)
		{
			SoundManager.Get().PlayPreloaded(sound);
		}
		if (delayCardSoundSpells)
		{
			GameEntity.Coroutines.StartCoroutine(WaitForCardSoundSpellDelay(clipLength));
		}
		if (actor != null && direction != 0)
		{
			m_ActiveSpeechBubble = ShowBubble(gameString, direction, actor, destroyOnNewNotification: false, clipLength, parentBubbleToActor, bubbleScale);
			waitTime += 0.5f;
		}
		yield return new WaitForSeconds(waitTime);
	}

	protected IEnumerator PlayCharacterQuoteAndWait(string prefabPath, string soundPath, float testingDuration = 0f, bool allowRepeatDuringSession = true, bool delayCardSoundSpells = false)
	{
		string gameString = new AssetReference(soundPath).GetLegacyAssetName();
		yield return GameEntity.Coroutines.StartCoroutine(PlayCharacterQuoteAndWait(prefabPath, soundPath, gameString, NotificationManager.DEFAULT_CHARACTER_POS, 1f, testingDuration, allowRepeatDuringSession, delayCardSoundSpells));
	}

	protected IEnumerator PlayCharacterQuoteAndWait(string prefabPath, string soundPath, string gameString, Vector3 position, float waitTimeScale = 1f, float testingDuration = 0f, bool allowRepeatDuringSession = true, bool delayCardSoundSpells = false, bool isBig = false, Notification.SpeechBubbleDirection bubbleDir = Notification.SpeechBubbleDirection.None, bool persistCharacter = false, bool skippable = false)
	{
		AudioSource sound = null;
		bool isJustTesting = false;
		if (string.IsNullOrEmpty(soundPath) || !CheckPreloadedSound(soundPath))
		{
			isJustTesting = true;
		}
		else
		{
			sound = GetPreloadedSound(soundPath);
		}
		if (!isJustTesting && (sound == null || sound.clip == null))
		{
			if (CheckPreloadedSound(soundPath))
			{
				RemovePreloadedSound(soundPath);
				PreloadSound(soundPath);
				while (IsPreloadingAssets())
				{
					yield return null;
				}
				sound = GetPreloadedSound(soundPath);
			}
			if (sound == null || sound.clip == null)
			{
				Log.Sound.PrintDebug("MissionEntity.PlaySoundAndWait() - sound error - " + soundPath);
				yield break;
			}
		}
		float clipLength = ((!isJustTesting) ? sound.clip.length : testingDuration);
		if (!persistCharacter)
		{
			clipLength = Mathf.Max(clipLength, 3f);
		}
		float waitTime = clipLength * waitTimeScale;
		Log.Notifications.Print("PlayCharacterQuoteAndWait() - Playing quote with clipLength {0}.  waitTimeScale: {1}  MINIMUM_DISPLAY_TIME_FOR_BIG_QUOTE: {2}", clipLength, waitTimeScale, 3f);
		if (delayCardSoundSpells)
		{
			GameEntity.Coroutines.StartCoroutine(WaitForCardSoundSpellDelay(clipLength));
		}
		Action<int> finishCallback = (skippable ? ((Action<int>)delegate
		{
			waitTime = 0f;
		}) : null);
		Notification notification;
		if (isBig)
		{
			notification = NotificationManager.Get().CreateBigCharacterQuoteWithGameString(prefabPath, position, soundPath, gameString, allowRepeatDuringSession, clipLength, finishCallback, useOverlayUI: false, bubbleDir, persistCharacter);
		}
		else
		{
			if (persistCharacter)
			{
				Log.All.PrintWarning("PersistCharacter is not currently supported for CharacterQuotes that are not big!");
			}
			notification = NotificationManager.Get().CreateCharacterQuote(prefabPath, position, GameStrings.Get(gameString), soundPath, allowRepeatDuringSession, clipLength * 2f, finishCallback);
		}
		if ((bool)notification)
		{
			if (!skippable && notification.clickOff != null)
			{
				notification.clickOff = null;
			}
			else if (skippable && notification.clickOff == null)
			{
				Log.All.PrintWarning("Skippable character quotes require a clickOff reference!");
			}
		}
		else
		{
			Log.All.PrintWarning("PlayCharacterQuoteAndWait: 'notification' is null.");
		}
		waitTime += 0.5f;
		for (; waitTime > 0f; waitTime -= Time.deltaTime)
		{
			yield return null;
		}
		if (!persistCharacter)
		{
			NotificationManager.Get().DestroyActiveQuote(0f);
		}
	}

	protected IEnumerator PlayBigCharacterQuoteAndWait(string prefabPath, string soundPath, Vector3 characterPosition, Notification.SpeechBubbleDirection bubbleDir = Notification.SpeechBubbleDirection.None, float testingDuration = 3f, float waitTimeScale = 1f, bool allowRepeatDuringSession = true, bool delayCardSoundSpells = false, bool persistCharacter = false, bool skippable = false)
	{
		string gameString = new AssetReference(soundPath).GetLegacyAssetName();
		yield return GameEntity.Coroutines.StartCoroutine(PlayCharacterQuoteAndWait(prefabPath, soundPath, gameString, characterPosition, waitTimeScale, testingDuration, allowRepeatDuringSession, delayCardSoundSpells, isBig: true, bubbleDir, persistCharacter, skippable));
	}

	protected IEnumerator PlayBigCharacterQuoteAndWait(string prefabPath, string soundPath, float testingDuration = 3f, float waitTimeScale = 1f, bool allowRepeatDuringSession = true, bool delayCardSoundSpells = false)
	{
		string gameString = new AssetReference(soundPath).GetLegacyAssetName();
		yield return GameEntity.Coroutines.StartCoroutine(PlayCharacterQuoteAndWait(prefabPath, soundPath, gameString, Vector3.zero, waitTimeScale, testingDuration, allowRepeatDuringSession, delayCardSoundSpells, isBig: true));
	}

	protected IEnumerator PlayBigCharacterQuoteAndWait(string prefabPath, string soundPath, string gameString, float testingDuration = 3f, float waitTimeScale = 1f, bool allowRepeatDuringSession = true, bool delayCardSoundSpells = false)
	{
		yield return GameEntity.Coroutines.StartCoroutine(PlayCharacterQuoteAndWait(prefabPath, soundPath, gameString, Vector3.zero, waitTimeScale, testingDuration, allowRepeatDuringSession, delayCardSoundSpells, isBig: true));
	}

	protected IEnumerator PlayBigCharacterQuoteAndWaitOnce(string prefabPath, string soundPath, float testingDuration = 3f, float waitTimeScale = 1f, bool delayCardSoundSpells = false, bool persistCharacter = false, bool skippable = false)
	{
		bool allowRepeat = DemoMgr.Get().IsExpoDemo();
		string gameString = new AssetReference(soundPath).GetLegacyAssetName();
		yield return GameEntity.Coroutines.StartCoroutine(PlayCharacterQuoteAndWait(prefabPath, soundPath, gameString, Vector3.zero, waitTimeScale, testingDuration, allowRepeat, delayCardSoundSpells, isBig: true, Notification.SpeechBubbleDirection.None, persistCharacter, skippable));
	}

	protected IEnumerator WaitForCardSoundSpellDelay(float sec)
	{
		GetGameOptions().SetBooleanOption(GameEntityOption.DELAY_CARD_SOUND_SPELLS, value: true);
		yield return new WaitForSeconds(sec);
		GetGameOptions().SetBooleanOption(GameEntityOption.DELAY_CARD_SOUND_SPELLS, value: false);
	}

	protected Notification ShowBubble(string textKey, Notification.SpeechBubbleDirection direction, Actor speakingActor, bool destroyOnNewNotification, float duration, bool parentToActor, float bubbleScale = 0f)
	{
		if (speakingActor == null)
		{
			return null;
		}
		NotificationManager notificationManager = NotificationManager.Get();
		Notification notification = notificationManager.CreateSpeechBubble(GameStrings.Get(textKey), direction, speakingActor, destroyOnNewNotification, parentToActor, bubbleScale);
		if (duration > 0f)
		{
			notificationManager.DestroyNotification(notification, duration);
		}
		return notification;
	}

	protected ShouldPlayValue InternalShouldPlayOpeningLine()
	{
		return ShouldPlayValue.Always;
	}

	protected ShouldPlayValue InternalShouldPlayBossLine()
	{
		return ShouldPlayValue.Always;
	}

	protected ShouldPlayValue InternalShouldPlayMissionFlavorLine()
	{
		if (IsHeroic())
		{
			return ShouldPlayValue.Once;
		}
		return ShouldPlayValue.Always;
	}

	protected ShouldPlayValue InternalShouldPlayOnlyOnce()
	{
		return ShouldPlayValue.Once;
	}

	protected ShouldPlayValue InternalShouldPlayAdventureFlavorLine()
	{
		if (IsHeroic())
		{
			return ShouldPlayValue.Once;
		}
		if (IsClassChallenge())
		{
			return ShouldPlayValue.Never;
		}
		return ShouldPlayValue.Always;
	}

	protected ShouldPlayValue InternalShouldPlayClosingLine()
	{
		if (IsClassChallenge())
		{
			return ShouldPlayValue.Never;
		}
		return ShouldPlayValue.Always;
	}

	protected ShouldPlayValue InternalShouldPlayEasterEggLine()
	{
		return ShouldPlayValue.Always;
	}

	protected ShouldPlayValue InternalShouldPlayCriticalLine()
	{
		return ShouldPlayValue.Always;
	}

	protected Notification.SpeechBubbleDirection GetDirection(Actor actor)
	{
		if (actor != null && actor.GetEntity() != null)
		{
			if (actor.GetEntity().IsControlledByFriendlySidePlayer())
			{
				return Notification.SpeechBubbleDirection.BottomLeft;
			}
		}
		else
		{
			Log.Gameplay.PrintError("MissionEntity.GetDirection(): actor param is null");
		}
		return Notification.SpeechBubbleDirection.TopRight;
	}

	protected string GetMulliganHeroFadeItweenName(Actor actor)
	{
		if (actor.GetEntity().IsControlledByFriendlySidePlayer())
		{
			return "MyHeroLightBlend";
		}
		return "HisHeroLightBlend";
	}

	protected IEnumerator PlayLittleCharacterLine(string speaker, string line, ShouldPlay shouldPlay, float testingDuration)
	{
		if (shouldPlay() == ShouldPlayValue.Always)
		{
			yield return GameEntity.Coroutines.StartCoroutine(PlayCharacterQuoteAndWait(speaker, line, testingDuration));
		}
	}

	protected IEnumerator PlayLine(string speaker, string line, ShouldPlay shouldPlay, float duration = 2.5f, bool persistCharacter = false, bool skippable = false)
	{
		yield return PlayLine(speaker, line, shouldPlay, Vector3.zero, Notification.SpeechBubbleDirection.None, duration, persistCharacter, skippable);
	}

	protected IEnumerator PlayLine(string speaker, string line, ShouldPlay shouldPlay, Vector3 quotePosition, Notification.SpeechBubbleDirection direction, float duration, bool persistCharacter = false, bool skippable = false)
	{
		if (!m_enemySpeaking)
		{
			m_enemySpeaking = true;
			if (m_forceAlwaysPlayLine)
			{
				yield return GameEntity.Coroutines.StartCoroutine(PlayBigCharacterQuoteAndWait(speaker, line, quotePosition, direction, duration, 1f, allowRepeatDuringSession: true, delayCardSoundSpells: false, persistCharacter, skippable));
			}
			else if (shouldPlay() == ShouldPlayValue.Always)
			{
				yield return GameEntity.Coroutines.StartCoroutine(PlayBigCharacterQuoteAndWait(speaker, line, quotePosition, direction, duration, 1f, allowRepeatDuringSession: true, delayCardSoundSpells: false, persistCharacter, skippable));
			}
			else if (shouldPlay() == ShouldPlayValue.Once)
			{
				yield return GameEntity.Coroutines.StartCoroutine(PlayBigCharacterQuoteAndWaitOnce(speaker, line, duration, 1f, delayCardSoundSpells: false, persistCharacter, skippable));
			}
			NotificationManager.Get().ForceAddSoundToPlayedList(line);
			m_enemySpeaking = false;
		}
	}

	protected IEnumerator PlayLine(Actor speaker, string line, ShouldPlay shouldPlay, float duration)
	{
		if (!m_enemySpeaking)
		{
			m_enemySpeaking = true;
			Notification.SpeechBubbleDirection direction = GetDirection(speaker);
			if (m_forceAlwaysPlayLine)
			{
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(line, direction, speaker, duration));
			}
			else if (shouldPlay() == ShouldPlayValue.Always)
			{
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(line, direction, speaker, duration));
			}
			else if (shouldPlay() == ShouldPlayValue.Once)
			{
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeechOnce(line, direction, speaker, duration));
			}
			NotificationManager.Get().ForceAddSoundToPlayedList(line);
			m_enemySpeaking = false;
		}
	}

	protected bool ShouldPlayLine(string line, ShouldPlay shouldPlay)
	{
		bool returnValue = false;
		switch (shouldPlay())
		{
		case ShouldPlayValue.Always:
			returnValue = true;
			break;
		case ShouldPlayValue.Once:
			if (DemoMgr.Get().IsExpoDemo() || !NotificationManager.Get().HasSoundPlayedThisSession(line))
			{
				returnValue = true;
			}
			break;
		}
		return returnValue;
	}

	protected IEnumerator PlayOpeningLine(string speaker, string line, float duration = 2.5f)
	{
		yield return PlayLine(speaker, line, InternalShouldPlayOpeningLine, duration);
	}

	protected IEnumerator PlayOpeningLine(Actor speaker, string line, float duration = 2.5f)
	{
		yield return PlayLine(speaker, line, InternalShouldPlayOpeningLine, duration);
	}

	protected IEnumerator PlayBossLine(string speaker, string line, float duration = 2.5f)
	{
		yield return PlayLine(speaker, line, InternalShouldPlayBossLine, duration);
	}

	protected IEnumerator PlayBossLine(Actor speaker, string line, float duration = 2.5f)
	{
		yield return PlayLine(speaker, line, InternalShouldPlayBossLine, duration);
	}

	protected IEnumerator PlayLineOnlyOnce(Actor speaker, string line, float duration = 2.5f)
	{
		yield return PlayLine(speaker, line, InternalShouldPlayOnlyOnce, duration);
	}

	protected IEnumerator PlayLineOnlyOnce(string speaker, string line, float duration = 2.5f)
	{
		yield return PlayLine(speaker, line, InternalShouldPlayOnlyOnce, duration);
	}

	protected IEnumerator PlayMissionFlavorLine(string speaker, string line, float duration = 2.5f)
	{
		yield return PlayLine(speaker, line, InternalShouldPlayMissionFlavorLine, duration);
	}

	protected IEnumerator PlayMissionFlavorLine(string speaker, string line, Vector3 quotePosition, Notification.SpeechBubbleDirection direction = Notification.SpeechBubbleDirection.None, float duration = 2.5f, bool persistCharacter = false)
	{
		yield return PlayLine(speaker, line, InternalShouldPlayMissionFlavorLine, quotePosition, direction, duration, persistCharacter);
	}

	protected IEnumerator PlayMissionFlavorLine(Actor speaker, string line, float duration = 2.5f)
	{
		yield return PlayLine(speaker, line, InternalShouldPlayMissionFlavorLine, duration);
	}

	protected IEnumerator PlayAdventureFlavorLine(string speaker, string line, float duration = 2.5f)
	{
		yield return PlayLine(speaker, line, InternalShouldPlayAdventureFlavorLine, duration);
	}

	protected IEnumerator PlayAdventureFlavorLine(Actor speaker, string line, float duration = 2.5f)
	{
		yield return PlayLine(speaker, line, InternalShouldPlayAdventureFlavorLine, duration);
	}

	protected IEnumerator PlayClosingLine(string speaker, string line, float duration = 2.5f)
	{
		yield return PlayLittleCharacterLine(speaker, line, InternalShouldPlayClosingLine, duration);
	}

	protected IEnumerator PlayEasterEggLine(string speaker, string line, float duration = 2.5f)
	{
		yield return PlayLine(speaker, line, InternalShouldPlayEasterEggLine, duration);
	}

	protected IEnumerator PlayEasterEggLine(Actor speaker, string line, float duration = 2.5f)
	{
		yield return PlayLine(speaker, line, InternalShouldPlayEasterEggLine, duration);
	}

	protected IEnumerator PlayCriticalLine(string speaker, string line, float duration = 2.5f)
	{
		yield return PlayLine(speaker, line, InternalShouldPlayCriticalLine, duration);
	}

	protected IEnumerator PlayCriticalLine(Actor speaker, string line, float duration = 2.5f)
	{
		yield return PlayLine(speaker, line, InternalShouldPlayCriticalLine, duration);
	}

	protected bool ShouldPlayCriticalLine(string line)
	{
		return ShouldPlayLine(line, InternalShouldPlayCriticalLine);
	}

	protected bool ShouldPlayMissionFlavorLine(string line)
	{
		return ShouldPlayLine(line, InternalShouldPlayMissionFlavorLine);
	}

	protected bool ShouldPlayBossLine(string line)
	{
		return ShouldPlayLine(line, InternalShouldPlayBossLine);
	}

	protected bool ShouldPlayEasterEggLine(string line)
	{
		return ShouldPlayLine(line, InternalShouldPlayEasterEggLine);
	}

	protected bool ShouldPlayOpeningLine(string line)
	{
		return ShouldPlayLine(line, InternalShouldPlayOpeningLine);
	}

	protected IEnumerator PlayLineAlways(string speaker, string line, float duration = 2.5f, bool persistCharacter = false, bool skippable = false)
	{
		yield return PlayLine(speaker, line, InternalShouldPlayBossLine, duration, persistCharacter, skippable);
	}

	protected IEnumerator PlayLineAlways(Actor speaker, string line, float duration = 2.5f)
	{
		yield return PlayLine(speaker, line, InternalShouldPlayBossLine, duration);
	}

	protected IEnumerator PlayLineAlways(Actor speaker, string backupSpeaker, string line, float duration = 2.5f)
	{
		if (speaker == null)
		{
			yield return PlayLine(backupSpeaker, line, InternalShouldPlayBossLine, duration);
		}
		else
		{
			yield return PlayLine(speaker, line, InternalShouldPlayBossLine, duration);
		}
	}

	public IEnumerator PlayLineInOrderOnce(Actor actor, List<string> lines)
	{
		string pickedLine = null;
		for (int i = 0; i < lines.Count; i++)
		{
			if (!m_InOrderPlayedLines.Contains(lines[i]))
			{
				pickedLine = lines[i];
				break;
			}
		}
		if (pickedLine != null)
		{
			m_InOrderPlayedLines.Add(pickedLine);
			yield return PlayLineAlways(actor, pickedLine);
		}
	}

	public IEnumerator PlayLineInOrderOnce(string actor, List<string> lines)
	{
		string pickedLine = null;
		for (int i = 0; i < lines.Count; i++)
		{
			if (!m_InOrderPlayedLines.Contains(lines[i]))
			{
				pickedLine = lines[i];
				break;
			}
		}
		if (pickedLine != null)
		{
			m_InOrderPlayedLines.Add(pickedLine);
			yield return PlayLineAlways(actor, pickedLine);
		}
	}

	protected virtual void InitEmoteResponses()
	{
	}

	protected IEnumerator HandlePlayerEmoteWithTiming(EmoteType emoteType, CardSoundSpell emoteSpell)
	{
		while (emoteSpell.IsActive())
		{
			yield return null;
		}
		if (!m_enemySpeaking)
		{
			PlayEmoteResponse(emoteType, emoteSpell);
		}
	}

	protected virtual void PlayEmoteResponse(EmoteType emoteType, CardSoundSpell emoteSpell)
	{
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
			.GetActor();
		foreach (EmoteResponseGroup responseGroup in m_emoteResponseGroups)
		{
			if (responseGroup.m_responses.Count != 0 && responseGroup.m_triggers.Contains(emoteType))
			{
				PlayNextEmoteResponse(responseGroup, enemyActor);
				CycleNextResponseGroupIndex(responseGroup);
			}
		}
	}

	protected void PlayNextEmoteResponse(EmoteResponseGroup responseGroup, Actor actor)
	{
		int index = responseGroup.m_responseIndex;
		EmoteResponse response = responseGroup.m_responses[index];
		GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeechWithCustomGameString(response.m_soundName, response.m_stringTag, Notification.SpeechBubbleDirection.TopRight, actor));
	}

	protected virtual void CycleNextResponseGroupIndex(EmoteResponseGroup responseGroup)
	{
		if (responseGroup.m_responseIndex == responseGroup.m_responses.Count - 1)
		{
			responseGroup.m_responseIndex = 0;
		}
		else
		{
			responseGroup.m_responseIndex++;
		}
	}

	protected Actor FindEnemyActorInPlayByDesignCode(string designCode)
	{
		return FindActorInPlayByDesignCode(designCode, Player.Side.OPPOSING);
	}

	protected Actor FindActorInPlayByDesignCode(string designCode, Player.Side side = Player.Side.NEUTRAL)
	{
		if (string.IsNullOrEmpty(designCode))
		{
			return null;
		}
		List<Player> playersToCheck = new List<Player>();
		GameState gs = GameState.Get();
		switch (side)
		{
		case Player.Side.FRIENDLY:
			playersToCheck.Add(gs.GetFriendlySidePlayer());
			break;
		case Player.Side.OPPOSING:
			playersToCheck.Add(gs.GetOpposingSidePlayer());
			break;
		case Player.Side.NEUTRAL:
			playersToCheck.Add(gs.GetFriendlySidePlayer());
			playersToCheck.Add(gs.GetOpposingSidePlayer());
			break;
		}
		foreach (Player player in playersToCheck)
		{
			Zone zone = player.GetBattlefieldZone();
			if (zone != null)
			{
				foreach (Card card in zone.GetCards())
				{
					if (card.GetEntity().GetCardId() == designCode)
					{
						return card.GetActor();
					}
				}
			}
			zone = player.GetHeroZone();
			if (zone != null)
			{
				foreach (Card card2 in zone.GetCards())
				{
					if (card2.GetEntity().GetCardId() == designCode)
					{
						return card2.GetActor();
					}
				}
			}
			Card weapon = player.GetWeaponCard();
			if (weapon != null && weapon.GetEntity().GetCardId() == designCode)
			{
				return weapon.GetActor();
			}
			Card heroPower = player.GetHeroPowerCard();
			if (heroPower != null && heroPower.GetEntity().GetCardId() == designCode)
			{
				return heroPower.GetActor();
			}
		}
		return null;
	}

	protected IEnumerator HideRuneTrayAndLoanerBanner(int linkedID)
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		Entity linkedEnt = GameState.Get().GetEntity(linkedID);
		if (linkedEnt == null)
		{
			yield break;
		}
		Actor linkedActor = linkedEnt.GetCard().GetActor();
		if (linkedActor != null)
		{
			Transform banner = linkedActor.transform.Find("RootObject/LoanerDeckFlag");
			if (banner != null)
			{
				banner.gameObject.SetActive(value: false);
			}
			Transform runeTray = linkedActor.transform.Find("RootObject/DecorationRoot/DeckRunesContainer");
			if (runeTray != null)
			{
				runeTray.gameObject.SetActive(value: false);
			}
		}
	}
}
