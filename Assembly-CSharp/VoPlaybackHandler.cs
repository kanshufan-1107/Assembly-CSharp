using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class VoPlaybackHandler
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

	private GameEntity m_GameEntity;

	private MonoBehaviour m_coroutines;

	public const int InGame_BossAttacks = 500;

	public const int InGame_BossAttacksSpecial = 501;

	public const int InGame_BossUsesHeroPower = 510;

	public const int InGame_BossUsesHeroPowerSpecial = 511;

	public const int InGame_BossEquipWeapon = 513;

	public const int InGame_BossDeath = 516;

	public const int InGame_PlayerAttacks = 502;

	public const int InGame_PlayerAttacksSpecial = 503;

	public const int InGame_PlayerUsesHeroPower = 508;

	public const int InGame_PlayerUsesHeroPowerSpecial = 509;

	public const int InGame_PlayerEquipWeapon = 512;

	public const int InGame_PlayerIdle = 518;

	public const int InGame_PlayerDeath = 519;

	public const int InGame_VictoryPreExplosion = 504;

	public const int InGame_VictoryPostExplosion = 505;

	public const int InGame_LossPreExplosion = 506;

	public const int InGame_LossPostExplosion = 507;

	public const int InGame_EmoteResponse = 515;

	public const int TurnOffBossExplodingOnDeath = 600;

	public const int TurnOffPlayerExplodingOnDeath = 601;

	public const int DisableAutomaticVO = 602;

	public const int EnableAutomaticVO = 603;

	public const int TurnOnBossExplodingOnDeath = 610;

	public const int TurnOnPlayerExplodingOnDeath = 611;

	public const int DoEmoteDrivenStart = 612;

	public const int PlayNextPlayerLine = 1000;

	public const int PlayRepeatPlayerLine = 1001;

	public const int PlayNextBossLine = 1002;

	public const int PlayRepeatBossLine = 1003;

	public const int ToggleAlwaysPlayLines = 1010;

	public const int PlayAllVOLines = 1011;

	public const int PlayAllBossVOLines = 1012;

	public const int PlayAllPlayerVOLines = 1013;

	public const int HearthStoneUsed = 58023;

	public int m_PlayPlayerVOLineIndex;

	public int m_PlayBossVOLineIndex;

	public List<string> m_PlayerVOLines = new List<string>();

	public List<string> m_BossVOLines = new List<string>();

	public string m_introLine;

	public string m_deathLine;

	public string m_standardEmoteResponseLine;

	public List<string> m_BossIdleLines;

	public List<string> m_BossIdleLinesCopy;

	public float m_lastVOplayFinshtime;

	public float m_BanterVOSilenceTime = 2f;

	private HashSet<string> m_InOrderPlayedLines = new HashSet<string>();

	public const bool ShowSpeechBubbleTrue = true;

	public const bool ShowSpeechBubbleFalse = false;

	public const bool PlayLinesRandomOrder = true;

	public const bool PlayLinesInOrder = false;

	public const int InGame_Introduction = 514;

	public bool m_MissionDisableAutomaticVO;

	public bool m_forceAlwaysPlayLine;

	public const int InGame_BossIdle = 517;

	private Notification.SpeechBubbleDirection m_LettuceMinionSpeakingDirection = Notification.SpeechBubbleDirection.BottomLeft;

	protected GameEntity GameEntity
	{
		get
		{
			if (m_GameEntity == null)
			{
				m_GameEntity = GameState.Get().GetGameEntity();
			}
			return m_GameEntity;
		}
	}

	public MonoBehaviour Coroutines
	{
		get
		{
			return m_coroutines;
		}
		set
		{
			m_coroutines = value;
		}
	}

	public VoPlaybackHandler()
	{
	}

	public virtual IEnumerator HandleStartOfTurnWithTiming(int turn)
	{
		yield break;
	}

	public virtual IEnumerator RespondToWillPlayCardWithTiming(string cardId, Entity playedEntity)
	{
		yield break;
	}

	public virtual IEnumerator RespondToFriendlyPlayedCardWithTiming(Entity entity)
	{
		yield break;
	}

	public virtual IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult, MissionEntity.OnChangeHandler fallback = null)
	{
		if (fallback != null)
		{
			yield return fallback(gameResult);
		}
	}

	public virtual IEnumerator RespondToResetGameFinishedWithTiming(Entity source)
	{
		yield break;
	}

	public virtual IEnumerator RespondToPlayedCardWithTiming(Entity entity)
	{
		if (!m_enemySpeaking && entity.GetCardType() != 0 && entity.GetCardType() == TAG_CARDTYPE.HERO_POWER && entity.GetControllerSide() == Player.Side.OPPOSING)
		{
			OnBossHeroPowerPlayed(entity);
		}
		yield break;
	}

	public virtual IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		if (missionEvent == 911)
		{
			GameState.Get().SetBusy(busy: true);
			while (m_enemySpeaking)
			{
				yield return null;
			}
			GameState.Get().SetBusy(busy: false);
			yield break;
		}
		Actor friendlyActor = FindRandomEnemyActorInPlay();
		Actor enemyActor = FindRandomFriendlyActorInPlay();
		GameEntity gameEnt = GameState.Get().GetGameEntity();
		gameEnt.GetTag(GAME_TAG.TURN);
		gameEnt.GetTag(GAME_TAG.EXTRA_TURNS_TAKEN_THIS_GAME);
		switch (missionEvent)
		{
		case 1000:
			GameState.Get().GetGameEntity().SetTag(GAME_TAG.MISSION_EVENT, 0);
			if (m_PlayPlayerVOLineIndex + 1 >= m_PlayerVOLines.Count)
			{
				m_PlayPlayerVOLineIndex = 0;
			}
			else
			{
				m_PlayPlayerVOLineIndex++;
			}
			SceneDebugger.Get().AddMessage(m_PlayerVOLines[m_PlayPlayerVOLineIndex]);
			yield return MissionPlayVO(friendlyActor, m_PlayerVOLines[m_PlayPlayerVOLineIndex]);
			break;
		case 1001:
			GameState.Get().GetGameEntity().SetTag(GAME_TAG.MISSION_EVENT, 0);
			SceneDebugger.Get().AddMessage(m_PlayerVOLines[m_PlayPlayerVOLineIndex]);
			yield return MissionPlayVO(friendlyActor, m_PlayerVOLines[m_PlayPlayerVOLineIndex]);
			break;
		case 1002:
			GameState.Get().GetGameEntity().SetTag(GAME_TAG.MISSION_EVENT, 0);
			if (m_PlayBossVOLineIndex + 1 >= m_BossVOLines.Count)
			{
				m_PlayBossVOLineIndex = 0;
			}
			else
			{
				m_PlayBossVOLineIndex++;
			}
			SceneDebugger.Get().AddMessage(m_BossVOLines[m_PlayBossVOLineIndex]);
			yield return MissionPlayVO(enemyActor, m_BossVOLines[m_PlayBossVOLineIndex]);
			break;
		case 1003:
			GameState.Get().GetGameEntity().SetTag(GAME_TAG.MISSION_EVENT, 0);
			SceneDebugger.Get().AddMessage(m_BossVOLines[m_PlayBossVOLineIndex]);
			yield return MissionPlayVO(enemyActor, m_BossVOLines[m_PlayBossVOLineIndex]);
			break;
		case 1011:
			GameState.Get().GetGameEntity().SetTag(GAME_TAG.MISSION_EVENT, 0);
			foreach (string currentLine3 in m_BossVOLines)
			{
				SceneDebugger.Get().AddMessage(currentLine3);
				yield return MissionPlayVO(enemyActor, currentLine3);
			}
			foreach (string currentLine4 in m_PlayerVOLines)
			{
				SceneDebugger.Get().AddMessage(currentLine4);
				yield return MissionPlayVO(enemyActor, currentLine4);
			}
			break;
		case 1012:
			GameState.Get().GetGameEntity().SetTag(GAME_TAG.MISSION_EVENT, 0);
			foreach (string currentLine2 in m_BossVOLines)
			{
				SceneDebugger.Get().AddMessage(currentLine2);
				yield return MissionPlayVO(enemyActor, currentLine2);
			}
			break;
		case 1013:
			GameState.Get().GetGameEntity().SetTag(GAME_TAG.MISSION_EVENT, 0);
			foreach (string currentLine in m_PlayerVOLines)
			{
				SceneDebugger.Get().AddMessage(currentLine);
				yield return MissionPlayVO(enemyActor, currentLine);
			}
			break;
		case 1010:
			if (m_forceAlwaysPlayLine)
			{
				m_forceAlwaysPlayLine = false;
			}
			else
			{
				m_forceAlwaysPlayLine = true;
			}
			break;
		case 603:
			m_MissionDisableAutomaticVO = false;
			break;
		case 602:
			m_MissionDisableAutomaticVO = true;
			break;
		}
	}

	public virtual void NotifyOfEntityAttacked(Entity attacker, Entity defender)
	{
	}

	public virtual void NotifyOfMinionPlayed(Entity minion)
	{
	}

	public virtual void NotifyOfHeroChanged(Entity newHero)
	{
	}

	public virtual void NotifyOfWeaponEquipped(Entity weapon)
	{
	}

	public virtual void NotifyOfSpellPlayed(Entity spell, Entity target)
	{
	}

	public virtual void NotifyOfHeroPowerUsed(Entity heroPower, Entity target)
	{
	}

	public virtual void NotifyOfMinionDied(Entity minion)
	{
	}

	public virtual void NotifyOfHeroDied(Entity hero)
	{
	}

	public virtual void NotifyOfWeaponDestroyed(Entity weapon)
	{
	}

	public virtual void PreloadAssets()
	{
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
		yield return Coroutines.StartCoroutine(PlaySoundAndWait(soundPath, gameString, direction, actor, waitTimeScale, parentBubbleToActor, delayCardSoundSpells, testingDuration, bubbleScale));
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
			yield return Coroutines.StartCoroutine(PlaySoundAndWait(soundPath, gameString, direction, actor, waitTimeScale, parentBubbleToActor, delayCardSoundSpells, testingDuration, bubbleScale));
			m_enemySpeaking = false;
		}
	}

	protected IEnumerator PlaySoundAndWait(string soundPath, string gameString, Notification.SpeechBubbleDirection direction, Actor actor, float waitTimeScale = 1f, bool parentBubbleToActor = true, bool delayCardSoundSpells = false, float testingDuration = 3f, float bubbleScale = 0f)
	{
		AudioSource sound = null;
		bool isJustTesting = false;
		if (string.IsNullOrEmpty(soundPath) || !GameState.Get().GetGameEntity().CheckPreloadedSound(soundPath))
		{
			isJustTesting = true;
		}
		else
		{
			sound = GameState.Get().GetGameEntity().GetPreloadedSound(soundPath);
		}
		if (!isJustTesting && (sound == null || sound.clip == null))
		{
			if (GameEntity.CheckPreloadedSound(soundPath))
			{
				GameEntity.RemovePreloadedSound(soundPath);
				GameEntity.PreloadSound(soundPath);
				while (GameEntity.IsPreloadingAssets())
				{
					yield return null;
				}
				sound = GameEntity.GetPreloadedSound(soundPath);
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
			Coroutines.StartCoroutine(WaitForCardSoundSpellDelay(clipLength));
		}
		if (actor != null && direction != 0)
		{
			m_ActiveSpeechBubble = ShowBubble(gameString, direction, actor, destroyOnNewNotification: false, clipLength, parentBubbleToActor, bubbleScale);
			waitTime += 0.5f;
		}
		yield return new WaitForSeconds(waitTime);
	}

	protected IEnumerator WaitForCardSoundSpellDelay(float sec)
	{
		GameEntity.GetGameOptions().SetBooleanOption(GameEntityOption.DELAY_CARD_SOUND_SPELLS, value: true);
		yield return new WaitForSeconds(sec);
		GameEntity.GetGameOptions().SetBooleanOption(GameEntityOption.DELAY_CARD_SOUND_SPELLS, value: false);
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

	protected ShouldPlayValue InternalShouldPlayOnlyOnce()
	{
		return ShouldPlayValue.Once;
	}

	protected Notification.SpeechBubbleDirection GetDirection(Actor actor)
	{
		if (actor.GetEntity().IsControlledByFriendlySidePlayer())
		{
			return Notification.SpeechBubbleDirection.BottomLeft;
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

	protected IEnumerator PlayLine(Actor speaker, string line, ShouldPlay shouldPlay, float duration)
	{
		if (!m_enemySpeaking)
		{
			m_enemySpeaking = true;
			Notification.SpeechBubbleDirection direction = GetDirection(speaker);
			if (m_forceAlwaysPlayLine)
			{
				yield return Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(line, direction, speaker, duration));
			}
			else if (shouldPlay() == ShouldPlayValue.Always)
			{
				yield return Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(line, direction, speaker, duration));
			}
			else if (shouldPlay() == ShouldPlayValue.Once)
			{
				yield return Coroutines.StartCoroutine(PlaySoundAndBlockSpeechOnce(line, direction, speaker, duration));
			}
			NotificationManager.Get().ForceAddSoundToPlayedList(line);
			m_enemySpeaking = false;
		}
	}

	public IEnumerator HandlePlayerEmoteWithTiming(EmoteType emoteType, CardSoundSpell emoteSpell)
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
		if (emoteType == EmoteType.START)
		{
			Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeech(m_introLine, Notification.SpeechBubbleDirection.TopRight, enemyActor));
		}
		else if (STANDARD_EMOTE_RESPONSE_TRIGGERS.Contains(emoteType))
		{
			Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeech(m_standardEmoteResponseLine, Notification.SpeechBubbleDirection.TopRight, enemyActor));
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

	public virtual List<string> GetIdleLines()
	{
		return new List<string>();
	}

	public void SetBossVOLines(List<string> VOLines)
	{
		m_BossVOLines = new List<string>(VOLines);
	}

	public virtual List<string> GetBossHeroPowerRandomLines()
	{
		return new List<string>();
	}

	protected virtual float ChanceToPlayBossHeroPowerVOLine()
	{
		return 1f;
	}

	protected virtual void OnBossHeroPowerPlayed(Entity entity)
	{
		float chanceToPlay = ChanceToPlayBossHeroPowerVOLine();
		float chanceRoll = Random.Range(0f, 1f);
		if (m_enemySpeaking || chanceToPlay < chanceRoll)
		{
			return;
		}
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		if (enemyActor == null)
		{
			return;
		}
		List<string> bossLines = GetBossHeroPowerRandomLines();
		string chosenLine = "";
		while (bossLines.Count > 0)
		{
			int randomChoice = Random.Range(0, bossLines.Count);
			chosenLine = bossLines[randomChoice];
			bossLines.RemoveAt(randomChoice);
			if (!NotificationManager.Get().HasSoundPlayedThisSession(chosenLine))
			{
				break;
			}
		}
		if (!(chosenLine == ""))
		{
			Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeechOnce(chosenLine, Notification.SpeechBubbleDirection.TopRight, enemyActor));
		}
	}

	public virtual void OnCreateGame()
	{
		m_introLine = null;
		m_deathLine = null;
		m_standardEmoteResponseLine = null;
		m_BossIdleLines = new List<string>(GetIdleLines());
		m_BossIdleLinesCopy = new List<string>(GetIdleLines());
	}

	protected IEnumerator MissionPlayVO(Actor actor, string line, bool bUseBubble, ShouldPlay shouldPlay)
	{
		if (!(actor == null) && line != null)
		{
			Notification.SpeechBubbleDirection speakerDirection = m_LettuceMinionSpeakingDirection;
			if (m_forceAlwaysPlayLine)
			{
				yield return Coroutines.StartCoroutine(PlayLine(actor, line, shouldPlay, 2.5f));
			}
			bool parentToActor = !(actor.GetCard() != null) || actor.GetCard().GetEntity() == null || !actor.GetCard().GetEntity().IsHeroPower();
			if (shouldPlay() == InternalShouldPlayAlways())
			{
				yield return Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(line, speakerDirection, actor, 2.5f, 1f, parentToActor));
			}
			else if (shouldPlay() == InternalShouldPlayOnlyOnce())
			{
				yield return Coroutines.StartCoroutine(PlaySoundAndBlockSpeechOnce(line, speakerDirection, actor, 2.5f, 1f, parentToActor));
				NotificationManager.Get().ForceAddSoundToPlayedList(line);
			}
			m_lastVOplayFinshtime = Time.time;
		}
	}

	public IEnumerator MissionPlayVO(Actor actor, string line)
	{
		yield return MissionPlayVO(actor, line, bUseBubble: true, InternalShouldPlayAlways);
	}

	public IEnumerator MissionPlayVOOnce(Actor actor, string line)
	{
		yield return MissionPlayVO(actor, line, bUseBubble: true, InternalShouldPlayOnce);
	}

	protected IEnumerator MissionPlayVO(Actor speaker, List<string> lines, ShouldPlay shouldPlay, bool bUseBubble = true, bool bPlayOrder = true)
	{
		bool removePickedLine = false;
		if (shouldPlay() == ShouldPlayValue.Once && !m_forceAlwaysPlayLine)
		{
			removePickedLine = true;
		}
		string randomLine = ((!bPlayOrder) ? PopNextLine(lines, removePickedLine) : PopRandomLine(lines, removePickedLine));
		if (randomLine != null)
		{
			yield return MissionPlayVO(speaker, randomLine, bUseBubble, shouldPlay);
		}
	}

	public IEnumerator MissionPlayVO(Actor actor, List<string> lines)
	{
		yield return MissionPlayVO(actor, lines, InternalShouldPlayAlways);
	}

	public IEnumerator MissionPlaySound(string line)
	{
		float waitTimeScale = 0f;
		bool parentBubbleToActor = true;
		bool delayCardSoundSpells = false;
		yield return Coroutines.StartCoroutine(PlaySoundAndWait(line, null, Notification.SpeechBubbleDirection.None, null, waitTimeScale, parentBubbleToActor, delayCardSoundSpells));
	}

	public IEnumerator MissionPlaySound(Actor actor, string line)
	{
		yield return MissionPlaySound(line);
	}

	protected ShouldPlayValue InternalShouldPlayAlways()
	{
		return ShouldPlayValue.Always;
	}

	protected ShouldPlayValue InternalShouldPlayOnce()
	{
		return ShouldPlayValue.Once;
	}

	protected string PopNextLine(List<string> lines, bool removeLine = true)
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
		if (pickedLine == null)
		{
			return null;
		}
		if (removeLine)
		{
			m_InOrderPlayedLines.Add(pickedLine);
		}
		return pickedLine;
	}

	protected string PopRandomLine(List<string> lines, bool removeLine = true)
	{
		if (lines == null)
		{
			return null;
		}
		if (lines.Count == 0)
		{
			return null;
		}
		string randomLine = lines[Random.Range(0, lines.Count)];
		if (removeLine)
		{
			lines.Remove(randomLine);
		}
		return randomLine;
	}

	public IEnumerator DoActionsBeforeDealingBaseMulliganCards()
	{
		MissionPause(pause: true);
		yield return HandleMissionEventWithTiming(514);
		MissionPause(pause: false);
	}

	public void MissionPause(bool pause)
	{
		m_MissionDisableAutomaticVO = pause;
		GameState.Get().SetBusy(pause);
	}

	protected Actor FindRandomEnemyActorInPlay()
	{
		using (List<Card>.Enumerator enumerator = GameState.Get().GetOpposingSidePlayer().GetBattlefieldZone()
			.GetCards()
			.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current.GetActor();
			}
		}
		return null;
	}

	protected Actor FindRandomFriendlyActorInPlay()
	{
		using (List<Card>.Enumerator enumerator = GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone()
			.GetCards()
			.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current.GetActor();
			}
		}
		return null;
	}

	public IEnumerator UnblockSpeechAgainAfterDuration(float durationInSeconds)
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
}
