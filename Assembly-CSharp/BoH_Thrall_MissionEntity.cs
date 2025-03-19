using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Hearthstone.Progression;
using UnityEngine;

public abstract class BoH_Thrall_MissionEntity : GenericDungeonMissionEntity
{
	public static class MemberInfoGetting
	{
		public static string GetMemberName<T>(Expression<Func<T>> memberExpression)
		{
			return ((MemberExpression)memberExpression.Body).Member.Name;
		}
	}

	public bool m_Mission_EnemyHeroShouldExplodeOnDefeat = true;

	public bool m_Mission_EnemyPlayIdleLines = true;

	public bool m_Mission_EnemyPlayIdleLinesInOrder = true;

	public bool m_Mission_FriendlyHeroShouldExplodeOnDefeat = true;

	public bool m_Mission_FriendlyPlayIdleLines;

	public bool m_Mission_FriendlylayIdleLinesInOrder = true;

	public bool m_MissionDisableAutomaticVO;

	private HashSet<string> m_InOrderPlayedLines = new HashSet<string>();

	public List<string> m_BossVOLines = new List<string>();

	public List<string> m_PlayerVOLines = new List<string>();

	public List<string> m_BossIdleLines;

	public List<string> m_BossIdleLinesCopy;

	public int m_PlayPlayerVOLineIndex;

	public int m_PlayBossVOLineIndex;

	public string m_introLine;

	public string m_deathLine;

	public string m_standardEmoteResponseLine;

	public bool m_DoEmoteDrivenStart;

	public MusicPlaylistType m_OverrideMulliganMusicTrack;

	public MusicPlaylistType m_OverrideMusicTrack;

	public string m_OverrideBossSubtext;

	public string m_OverridePlayerSubtext;

	public bool m_SupressEnemyDeathTextBubble;

	private Spell m_enemyBlowUpSpell;

	private Spell m_friendlyBlowUpSpell;

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string>();
		m_PlayerVOLines = new List<string>(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			PreloadSound(soundFile);
		}
	}

	public virtual List<string> GetBossIdleLines()
	{
		return new List<string>();
	}

	public virtual float GetThinkEmoteBossIdleChancePercentage()
	{
		return 0.25f;
	}

	public virtual float GetThinkIdleChancePercentage()
	{
		return 0.25f;
	}

	public virtual List<string> GetBossHeroPowerRandomLines()
	{
		return new List<string>();
	}

	protected virtual float ChanceToPlayBossHeroPowerVOLine()
	{
		return 1f;
	}

	public void MissionPause(bool pause)
	{
		m_MissionDisableAutomaticVO = pause;
		GameState.Get().SetBusy(pause);
	}

	protected virtual IEnumerator OnBossHeroPowerPlayed(Entity entity)
	{
		float chanceToPlayEnemyBossVO = ChanceToPlayBossHeroPowerVOLine();
		float chanceRoll = UnityEngine.Random.Range(0f, 1f);
		if (m_enemySpeaking || m_MissionDisableAutomaticVO || chanceToPlayEnemyBossVO < chanceRoll)
		{
			yield break;
		}
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		if (enemyActor == null)
		{
			yield break;
		}
		List<string> bossLines = GetBossHeroPowerRandomLines();
		string chosenLine = "";
		while (bossLines.Count > 0)
		{
			int randomChoice = UnityEngine.Random.Range(0, bossLines.Count);
			chosenLine = bossLines[randomChoice];
			bossLines.RemoveAt(randomChoice);
			if (!NotificationManager.Get().HasSoundPlayedThisSession(chosenLine))
			{
				break;
			}
		}
		if (!(chosenLine == ""))
		{
			yield return MissionPlayVO(enemyActor, chosenLine);
		}
	}

	protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
	{
		if (!m_MissionDisableAutomaticVO && !m_enemySpeaking && entity.GetCardType() != 0 && entity.GetCardType() == TAG_CARDTYPE.HERO_POWER && entity.GetControllerSide() == Player.Side.OPPOSING)
		{
			yield return OnBossHeroPowerPlayed(entity);
		}
	}

	protected override void PlayEmoteResponse(EmoteType emoteType, CardSoundSpell emoteSpell)
	{
		if (MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS.Contains(emoteType) && !m_enemySpeaking)
		{
			GameEntity.Coroutines.StartCoroutine(HandleMissionEventWithTiming(515));
		}
	}

	protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
	{
		switch (gameResult)
		{
		case TAG_PLAYSTATE.WON:
			GameState.Get().SetBusy(busy: true);
			GameState.Get().SetBusy(busy: false);
			break;
		case TAG_PLAYSTATE.LOST:
			GameState.Get().SetBusy(busy: true);
			GameState.Get().SetBusy(busy: false);
			break;
		}
		yield break;
	}

	public override bool ShouldShowHeroClassDuringMulligan(Player.Side playerSide)
	{
		return false;
	}

	public override void StartGameplaySoundtracks()
	{
		if (m_OverrideMusicTrack == MusicPlaylistType.Invalid)
		{
			base.StartGameplaySoundtracks();
		}
		else
		{
			MusicManager.Get().StartPlaylist(m_OverrideMusicTrack);
		}
	}

	public override void StartMulliganSoundtracks(bool soft)
	{
		if (!soft)
		{
			if (m_OverrideMulliganMusicTrack == MusicPlaylistType.Invalid)
			{
				base.StartMulliganSoundtracks(soft);
			}
			else
			{
				MusicManager.Get().StartPlaylist(m_OverrideMulliganMusicTrack);
			}
		}
	}

	public override string GetNameBannerSubtextOverride(Player.Side playerSide)
	{
		if (playerSide == Player.Side.OPPOSING && m_OverrideBossSubtext != null)
		{
			return GameStrings.Get(m_OverrideBossSubtext);
		}
		if (playerSide == Player.Side.FRIENDLY && m_OverridePlayerSubtext != null)
		{
			return GameStrings.Get(m_OverridePlayerSubtext);
		}
		return base.GetNameBannerSubtextOverride(playerSide);
	}

	public override void OnPlayThinkEmote()
	{
		Gameplay.Get().StartCoroutine(OnPlayThinkEmoteWithTiming());
	}

	public override IEnumerator OnPlayThinkEmoteWithTiming()
	{
		if (m_enemySpeaking)
		{
			yield break;
		}
		Player currentPlayer = GameState.Get().GetCurrentPlayer();
		if (!currentPlayer.IsFriendlySide() || currentPlayer.GetHeroCard().HasActiveEmoteSound())
		{
			yield break;
		}
		GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCardId();
		if (!m_Mission_FriendlyPlayIdleLines && !m_Mission_EnemyPlayIdleLines)
		{
			yield break;
		}
		float thinkIdleChancePercentage = GetThinkIdleChancePercentage();
		float randomThinkChance = UnityEngine.Random.Range(0f, 1f);
		if (thinkIdleChancePercentage < randomThinkChance)
		{
			yield break;
		}
		float thinkEmoteBossIdleChancePercentage = GetThinkEmoteBossIdleChancePercentage();
		float randomBossThinkChance = UnityEngine.Random.Range(0f, 1f);
		if (thinkEmoteBossIdleChancePercentage < randomBossThinkChance || (!m_Mission_FriendlyPlayIdleLines && m_Mission_EnemyPlayIdleLines))
		{
			Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
				.GetActor();
			string voLine = PopRandomLine(m_BossIdleLinesCopy);
			if (m_BossIdleLinesCopy.Count == 0)
			{
				m_BossIdleLinesCopy = new List<string>(m_BossIdleLines);
			}
			yield return MissionPlayVO(enemyActor, voLine);
		}
		else if (m_Mission_FriendlyPlayIdleLines)
		{
			EmoteType thinkEmote = EmoteType.THINK1;
			switch (UnityEngine.Random.Range(1, 4))
			{
			case 1:
				thinkEmote = EmoteType.THINK1;
				break;
			case 2:
				thinkEmote = EmoteType.THINK2;
				break;
			case 3:
				thinkEmote = EmoteType.THINK3;
				break;
			}
			CardSoundSpell soundSpell = GameState.Get().GetCurrentPlayer().GetHeroCard()
				.PlayEmote(thinkEmote);
			AudioSource activeAudioSource = soundSpell.GetActiveAudioSource();
			yield return GameState.Get().GetCurrentPlayer().GetHeroCard()
				.PlayEmote(thinkEmote);
			yield return new WaitForSeconds(activeAudioSource.clip.length);
		}
	}

	public override void NotifyOfGameOver(TAG_PLAYSTATE playState)
	{
		PegCursor.Get().SetMode(PegCursor.Mode.STOPWAITING);
		MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_EndGameScreen);
		Card enemyHeroCard = GameState.Get().GetOpposingSidePlayer().GetHeroCard();
		Card friendlyHeroCard = GameState.Get().GetFriendlySidePlayer().GetHeroCard();
		Gameplay.Get().SaveOriginalTimeScale();
		AchievementManager.Get()?.PauseToastNotifications();
		if (ShouldPlayHeroBlowUpSpells(playState))
		{
			switch (playState)
			{
			case TAG_PLAYSTATE.WON:
			{
				string audioAsset = GetGameOptions().GetStringOption(GameEntityOption.VICTORY_AUDIO_PATH);
				if (!string.IsNullOrEmpty(audioAsset))
				{
					SoundManager.Get().LoadAndPlay(audioAsset);
				}
				if (m_Mission_EnemyHeroShouldExplodeOnDefeat)
				{
					m_enemyBlowUpSpell = BlowUpHero(enemyHeroCard, SpellType.ENDGAME_LOSE_ENEMY);
				}
				break;
			}
			case TAG_PLAYSTATE.LOST:
			{
				string audioAsset = GetGameOptions().GetStringOption(GameEntityOption.DEFEAT_AUDIO_PATH);
				if (!string.IsNullOrEmpty(audioAsset))
				{
					SoundManager.Get().LoadAndPlay(audioAsset);
				}
				if (m_Mission_FriendlyHeroShouldExplodeOnDefeat)
				{
					m_friendlyBlowUpSpell = BlowUpHero(friendlyHeroCard, SpellType.ENDGAME_LOSE_FRIENDLY);
				}
				break;
			}
			case TAG_PLAYSTATE.TIED:
			{
				string audioAsset = GetGameOptions().GetStringOption(GameEntityOption.DEFEAT_AUDIO_PATH);
				if (!string.IsNullOrEmpty(audioAsset))
				{
					SoundManager.Get().LoadAndPlay(audioAsset);
				}
				if (m_Mission_EnemyHeroShouldExplodeOnDefeat)
				{
					m_enemyBlowUpSpell = BlowUpHero(enemyHeroCard, SpellType.ENDGAME_DRAW);
				}
				if (m_Mission_FriendlyHeroShouldExplodeOnDefeat)
				{
					m_friendlyBlowUpSpell = BlowUpHero(friendlyHeroCard, SpellType.ENDGAME_DRAW);
				}
				break;
			}
			}
		}
		ShowEndGameScreen(playState, m_enemyBlowUpSpell, m_friendlyBlowUpSpell);
		GameEntity.Coroutines.StartCoroutine(HandleGameOverWithTiming(playState));
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
		string randomLine = lines[UnityEngine.Random.Range(0, lines.Count)];
		if (removeLine)
		{
			lines.Remove(randomLine);
		}
		return randomLine;
	}

	public void SetBossVOLines(List<string> VOLines)
	{
		m_BossVOLines = new List<string>(VOLines);
	}

	protected ShouldPlayValue InternalShouldPlayAlways()
	{
		return ShouldPlayValue.Always;
	}

	protected IEnumerator MissionPlayVO(Actor actor, string line, bool bUseBubble, ShouldPlay shouldPlay)
	{
		Notification.SpeechBubbleDirection speakerDirection = GetDirection(actor);
		if (m_forceAlwaysPlayLine)
		{
			yield return GameEntity.Coroutines.StartCoroutine(PlayLine(actor, line, shouldPlay, 2.5f));
		}
		if (shouldPlay() == InternalShouldPlayAlways())
		{
			yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(line, speakerDirection, actor, 2.5f));
		}
		else if (shouldPlay() == InternalShouldPlayOnlyOnce())
		{
			yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeechOnce(line, speakerDirection, actor, 2.5f));
			NotificationManager.Get().ForceAddSoundToPlayedList(line);
		}
	}

	public IEnumerator MissionPlayVO(Actor actor, string line)
	{
		yield return MissionPlayVO(actor, line, bUseBubble: true, InternalShouldPlayAlways);
	}

	public IEnumerator MissionPlaySound(Actor actor, string line)
	{
		float waitTimeScale = 0f;
		bool parentBubbleToActor = true;
		bool delayCardSoundSpells = false;
		yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndWait(line, null, Notification.SpeechBubbleDirection.None, null, waitTimeScale, parentBubbleToActor, delayCardSoundSpells));
	}

	protected IEnumerator MissionPlayVO(string brassRing, string line, bool bUseBubble, ShouldPlay shouldPlay)
	{
		if (m_enemySpeaking)
		{
			yield return null;
		}
		m_enemySpeaking = true;
		if (m_forceAlwaysPlayLine)
		{
			yield return GameEntity.Coroutines.StartCoroutine(PlayBigCharacterQuoteAndWait(brassRing, line));
		}
		if (shouldPlay() == ShouldPlayValue.Always)
		{
			yield return GameEntity.Coroutines.StartCoroutine(PlayBigCharacterQuoteAndWait(brassRing, line));
		}
		else if (shouldPlay() == ShouldPlayValue.Once)
		{
			yield return GameEntity.Coroutines.StartCoroutine(PlayBigCharacterQuoteAndWaitOnce(brassRing, line));
			NotificationManager.Get().ForceAddSoundToPlayedList(line);
		}
		m_enemySpeaking = false;
	}

	public IEnumerator MissionPlayVO(AssetReference brassRing, string line)
	{
		yield return MissionPlayVO(brassRing, line, bUseBubble: true, InternalShouldPlayAlways);
	}
}
