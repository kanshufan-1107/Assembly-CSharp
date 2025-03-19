using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DRGA_Dungeon : DRGA_MissionEntity
{
	public List<string> m_BossVOLines = new List<string>();

	public List<string> m_PlayerVOLines = new List<string>();

	public bool m_Heroic;

	public bool m_Galakrond;

	public static readonly AssetReference BrannBrassRing = new AssetReference("BrannBronzebeard_BrassRing_Quote.prefab:d1f8af47f0917e94289b63f3a42e52f7");

	public static readonly AssetReference EliseBrassRing = new AssetReference("EliseStarseeker_BrassRing_Quote.prefab:7176acaa6d28fa447adbafde663037d3");

	public static readonly AssetReference FinleyBrassRing = new AssetReference("SirFinley_BrassRing_Quote.prefab:5f94953d717142446b348e4d2f3a4ca8");

	public static readonly AssetReference RenoBrassRing = new AssetReference("RenoJackson_BrassRing_Quote.prefab:74a27d2f94ef83744a0a8357dbac2e43");

	public static readonly AssetReference RafaamBrassRing = new AssetReference("Rafaam_BrassRing_Quote.prefab:2d6ab3cc1d153ed4886ff98e47d129c6");

	public string m_introLine;

	public string m_deathLine;

	public string m_standardEmoteResponseLine;

	public List<string> m_BossIdleLines;

	public List<string> m_BossIdleLinesCopy;

	private int m_PlayPlayerVOLineIndex;

	private int m_PlayBossVOLineIndex;

	public int TurnOfPlotTwistLastPlayed;

	public static DRGA_Dungeon InstantiateDRGADungeonMissionEntityForBoss(List<Network.PowerHistory> powerList, Network.HistCreateGame createGame)
	{
		string opposingHeroCardID = GenericDungeonMissionEntity.GetOpposingHeroCardID(powerList, createGame);
		switch (GameMgr.Get().GetMissionId())
		{
		case 0:
			return new DRGA_Evil_Fight_01();
		case 3484:
		case 3594:
			return new DRGA_Evil_Fight_01();
		case 3488:
		case 3595:
			return new DRGA_Evil_Fight_02();
		case 3489:
		case 3596:
			return new DRGA_Evil_Fight_03();
		case 3490:
		case 3597:
			return new DRGA_Evil_Fight_04();
		case 3491:
		case 3598:
			return new DRGA_Evil_Fight_05();
		case 3493:
		case 3599:
			return new DRGA_Evil_Fight_06();
		case 3494:
		case 3600:
			return new DRGA_Evil_Fight_07();
		case 3495:
		case 3601:
			return new DRGA_Evil_Fight_08();
		case 3497:
		case 3602:
			return new DRGA_Evil_Fight_09();
		case 3498:
		case 3603:
			return new DRGA_Evil_Fight_10();
		case 3499:
		case 3604:
			return new DRGA_Evil_Fight_11();
		case 3500:
		case 3605:
			return new DRGA_Evil_Fight_12();
		case 3469:
		case 3556:
			return new DRGA_Good_Fight_01();
		case 3470:
		case 3583:
			return new DRGA_Good_Fight_02();
		case 3471:
		case 3584:
			return new DRGA_Good_Fight_03();
		case 3472:
		case 3585:
			return new DRGA_Good_Fight_04();
		case 3473:
		case 3586:
			return new DRGA_Good_Fight_05();
		case 3475:
		case 3587:
			return new DRGA_Good_Fight_06();
		case 3477:
		case 3588:
			return new DRGA_Good_Fight_07();
		case 3478:
		case 3589:
			return new DRGA_Good_Fight_08();
		case 3479:
		case 3590:
			return new DRGA_Good_Fight_09();
		case 3480:
		case 3591:
			return new DRGA_Good_Fight_10();
		case 3481:
		case 3592:
			return new DRGA_Good_Fight_11();
		case 3483:
		case 3593:
			return new DRGA_Good_Fight_12();
		default:
			Log.All.PrintError("DRGA_Dungeon.InstantiateDRGADungeonMissionEntityForBoss() - Found unsupported enemy Boss {0}.", opposingHeroCardID);
			return new DRGA_Dungeon();
		}
	}

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
		m_Heroic = GetIsHeroic();
	}

	protected virtual bool GetIsHeroic()
	{
		int scenario = GameMgr.Get().GetMissionId();
		if (scenario == 3556 || (uint)(scenario - 3583) <= 22u)
		{
			return true;
		}
		return false;
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

	public IEnumerator PlayAndRemoveRandomLineOnlyOnceWithBrassRing(Actor actor, AssetReference brassRingBackup, List<string> lines)
	{
		if (actor != null)
		{
			yield return PlayAndRemoveRandomLineOnlyOnce(actor, lines);
		}
		else if (brassRingBackup != null)
		{
			yield return PlayAndRemoveRandomLineOnlyOnce(brassRingBackup, lines);
		}
	}

	protected IEnumerator PlayLineAlwaysWithBrassRing(Actor actor, AssetReference brassRingBackup, string line, float duration = 2.5f)
	{
		if (actor != null)
		{
			yield return PlayLineAlways(actor, line);
		}
		else if (brassRingBackup != null)
		{
			yield return PlayLineAlways(brassRingBackup, line);
		}
	}

	public IEnumerator PlayLineInOrderOnceWithBrassRing(Actor actor, AssetReference brassRingBackup, List<string> lines)
	{
		if (actor != null)
		{
			yield return PlayLineInOrderOnce(actor, lines);
		}
		else if (brassRingBackup != null)
		{
			yield return PlayLineInOrderOnce(brassRingBackup, lines);
		}
	}

	public IEnumerator PlayAndRemoveRandomLineOnlyOnce(Actor actor, List<string> lines)
	{
		string randomLine = PopRandomLine(lines);
		if (randomLine != null)
		{
			yield return PlayLineOnlyOnce(actor, randomLine);
		}
	}

	public IEnumerator PlayAndRemoveRandomLineOnlyOnce(string actor, List<string> lines)
	{
		string randomLine = PopRandomLine(lines);
		if (randomLine != null)
		{
			yield return PlayLineOnlyOnce(actor, randomLine);
		}
	}

	public IEnumerator PlayRandomLineAlways(Actor actor, List<string> lines)
	{
		string randomLine = PopRandomLine(lines);
		if (randomLine != null)
		{
			yield return PlayBossLine(actor, randomLine);
		}
	}

	public IEnumerator PlayRandomLineAlways(string actor, List<string> lines)
	{
		string randomLine = PopRandomLine(lines);
		if (randomLine != null)
		{
			yield return PlayBossLine(actor, randomLine);
		}
	}

	protected Actor GetEnemyActorByCardId(string cardId)
	{
		Player player = GameState.Get().GetOpposingSidePlayer();
		foreach (Card card in player.GetBattlefieldZone().GetCards())
		{
			Entity cardEntity = card.GetEntity();
			if (cardEntity.GetControllerId() == player.GetPlayerId() && cardEntity.GetCardId() == cardId)
			{
				return cardEntity.GetCard().GetActor();
			}
		}
		return null;
	}

	protected Actor GetFriendlyActorByCardId(string cardId)
	{
		Player player = GameState.Get().GetFriendlySidePlayer();
		foreach (Card card in player.GetBattlefieldZone().GetCards())
		{
			Entity cardEntity = card.GetEntity();
			if (cardEntity.GetControllerId() == player.GetPlayerId() && cardEntity.GetCardId() == cardId)
			{
				return cardEntity.GetCard().GetActor();
			}
		}
		return null;
	}

	public override void StartMulliganSoundtracks(bool soft)
	{
		if (!soft)
		{
			MusicManager.Get().StartPlaylist(MusicPlaylistType.InGame_DRGMulligan);
		}
	}

	public override void StartGameplaySoundtracks()
	{
		MusicManager.Get().StartPlaylist(MusicPlaylistType.InGame_DRG);
	}
}
