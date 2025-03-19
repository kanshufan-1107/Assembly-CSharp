using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoH_Jaina_Dungeon : BoH_Jaina_MissionEntity
{
	public List<string> m_BossVOLines = new List<string>();

	public List<string> m_PlayerVOLines = new List<string>();

	public string m_introLine;

	public string m_deathLine;

	public string m_standardEmoteResponseLine;

	public List<string> m_BossIdleLines;

	public List<string> m_BossIdleLinesCopy;

	private int m_PlayPlayerVOLineIndex;

	private int m_PlayBossVOLineIndex;

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

	protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
	{
		if (!m_enemySpeaking && entity.GetCardType() != 0 && entity.GetCardType() == TAG_CARDTYPE.HERO_POWER && entity.GetControllerSide() == Player.Side.OPPOSING)
		{
			OnBossHeroPowerPlayed(entity);
		}
		yield break;
	}

	protected override IEnumerator RespondToFriendlyPlayedCardWithTiming(Entity entity)
	{
		while (entity.GetCardType() == TAG_CARDTYPE.INVALID)
		{
			yield return null;
		}
		while (m_enemySpeaking)
		{
			yield return null;
		}
		yield return WaitForEntitySoundToFinish(entity);
	}

	public override void NotifyOfGameOver(TAG_PLAYSTATE gameResult)
	{
		base.NotifyOfGameOver(gameResult);
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
			.GetActor();
		if (!m_enemySpeaking && !string.IsNullOrEmpty(m_deathLine) && gameResult == TAG_PLAYSTATE.WON)
		{
			if (GetShouldSuppressDeathTextBubble())
			{
				Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeech(m_deathLine, Notification.SpeechBubbleDirection.None, enemyActor));
			}
			else
			{
				Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeech(m_deathLine, Notification.SpeechBubbleDirection.TopRight, enemyActor));
			}
		}
	}

	protected override void PlayEmoteResponse(EmoteType emoteType, CardSoundSpell emoteSpell)
	{
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
			.GetActor();
		if (emoteType == EmoteType.START)
		{
			Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeech(m_introLine, Notification.SpeechBubbleDirection.TopRight, enemyActor));
		}
		else if (MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS.Contains(emoteType))
		{
			Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeech(m_standardEmoteResponseLine, Notification.SpeechBubbleDirection.TopRight, enemyActor));
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = null;
		m_deathLine = null;
		m_standardEmoteResponseLine = null;
		m_BossIdleLines = new List<string>(GetIdleLines());
		m_BossIdleLinesCopy = new List<string>(GetIdleLines());
	}

	protected virtual bool GetShouldSuppressDeathTextBubble()
	{
		return false;
	}

	protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor playerActor = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
			.GetActor();
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
			yield return PlayBossLine(playerActor, m_PlayerVOLines[m_PlayPlayerVOLineIndex]);
			break;
		case 1001:
			GameState.Get().GetGameEntity().SetTag(GAME_TAG.MISSION_EVENT, 0);
			SceneDebugger.Get().AddMessage(m_PlayerVOLines[m_PlayPlayerVOLineIndex]);
			yield return PlayBossLine(playerActor, m_PlayerVOLines[m_PlayPlayerVOLineIndex]);
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
			yield return PlayBossLine(enemyActor, m_BossVOLines[m_PlayBossVOLineIndex]);
			break;
		case 1003:
			GameState.Get().GetGameEntity().SetTag(GAME_TAG.MISSION_EVENT, 0);
			SceneDebugger.Get().AddMessage(m_BossVOLines[m_PlayBossVOLineIndex]);
			yield return PlayBossLine(enemyActor, m_BossVOLines[m_PlayBossVOLineIndex]);
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
		case 58023:
		{
			SceneMgr.Mode nextMode = GameMgr.Get().GetPostGameSceneMode();
			GameMgr.Get().PreparePostGameSceneMode(nextMode);
			SceneMgr.Get().SetNextMode(nextMode);
			break;
		}
		default:
			yield return base.HandleMissionEventWithTiming(missionEvent);
			break;
		}
	}

	public override string GetNameBannerSubtextOverride(Player.Side playerSide)
	{
		_ = 2;
		return base.GetNameBannerSubtextOverride(playerSide);
	}

	public virtual float GetThinkEmoteBossThinkChancePercentage()
	{
		return 0.25f;
	}

	public override void OnPlayThinkEmote()
	{
		if (m_enemySpeaking)
		{
			return;
		}
		Player currentPlayer = GameState.Get().GetCurrentPlayer();
		if (!currentPlayer.IsFriendlySide() || currentPlayer.GetHeroCard().HasActiveEmoteSound())
		{
			return;
		}
		float thinkEmoteBossThinkChancePercentage = GetThinkEmoteBossThinkChancePercentage();
		float randomThink = Random.Range(0f, 1f);
		if (thinkEmoteBossThinkChancePercentage > randomThink && m_BossIdleLines != null && m_BossIdleLines.Count != 0)
		{
			Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
				.GetActor();
			string voLine = PopRandomLine(m_BossIdleLinesCopy);
			if (m_BossIdleLinesCopy.Count == 0)
			{
				m_BossIdleLinesCopy = new List<string>(GetIdleLines());
			}
			Gameplay.Get().StartCoroutine(PlayBossLine(enemyActor, voLine));
			return;
		}
		EmoteType thinkEmote = EmoteType.THINK1;
		switch (Random.Range(1, 4))
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
		GameState.Get().GetCurrentPlayer().GetHeroCard()
			.PlayEmote(thinkEmote);
	}

	public override void StartMulliganSoundtracks(bool soft)
	{
		if (!soft)
		{
			MusicManager.Get().StartPlaylist(MusicPlaylistType.InGame_DHMulligan);
		}
	}

	public override void StartGameplaySoundtracks()
	{
		MusicManager.Get().StartPlaylist(MusicPlaylistType.InGame_DHPrologue);
	}
}
