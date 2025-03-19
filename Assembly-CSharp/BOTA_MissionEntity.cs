using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using Hearthstone;
using UnityEngine;

public class BOTA_MissionEntity : GenericDungeonMissionEntity
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static Map<GameEntityOption, string> s_stringOptions = InitStringOptions();

	private static readonly AssetReference PuzzleIntroUI_Mirror = new AssetReference("PuzzleIntroUI_Mirror.prefab:d1c537160881d574f9ec948c60f7053a");

	private static readonly AssetReference PuzzleIntroUI_Lethal = new AssetReference("PuzzleIntroUI_Lethal.prefab:2991b0a18a580eb4dac344255b615563");

	private static readonly AssetReference PuzzleIntroUI_Survival = new AssetReference("PuzzleIntroUI_Survival.prefab:0ffd8ff37cf93e844b58b5babbba9e02");

	private static readonly AssetReference PuzzleIntroUI_Clear = new AssetReference("PuzzleIntroUI_Clear.prefab:47371bd3bd83eda48af01e1f9e4be1ee");

	private static bool s_shownEndTurnReminder = false;

	private Notification m_endTurnReminder;

	private Coroutine m_endTurnReminderCoroutine;

	public bool m_waitingForTurnStartIndicatorAfterReset;

	private PuzzleIntroSpell m_introSpell;

	private NormalButton m_confirmButton;

	private bool m_entranceFinished;

	private bool m_confirmButtonPressed;

	public static string s_introLine = null;

	public static string s_returnLine = null;

	public bool s_returnLineOverride;

	public List<string> s_emoteLines = new List<string>();

	protected List<string> m_randomEmoteLines = new List<string>();

	public List<string> s_idleLines = new List<string>();

	protected List<string> m_randomIdleLines = new List<string>();

	public List<string> s_restartLines = new List<string>();

	protected List<string> m_randomRestartLines = new List<string>();

	public string s_victoryLine_1;

	public string s_victoryLine_2;

	public string s_victoryLine_3;

	public string s_victoryLine_4;

	public string s_victoryLine_5;

	public string s_victoryLine_6;

	public string s_victoryLine_7;

	public string s_victoryLine_8;

	public string s_victoryLine_9;

	public List<string> s_lethalCompleteLines = new List<string>();

	private bool lethalLineUsed;

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.HANDLE_COIN,
			false
		} };
	}

	private static Map<GameEntityOption, string> InitStringOptions()
	{
		return new Map<GameEntityOption, string>();
	}

	public BOTA_MissionEntity()
	{
		m_gameOptions.AddOptions(s_booleanOptions, s_stringOptions);
	}

	public override void OnCreateGame()
	{
		if (!s_shownEndTurnReminder)
		{
			GameState.Get().RegisterOptionsReceivedListener(OnOptionsReceived);
			GameState.Get().RegisterGameOverListener(OnGameOver);
		}
	}

	public override void OnDecommissionGame()
	{
		GameState.Get().UnregisterOptionsReceivedListener(OnOptionsReceived);
		GameState.Get().UnregisterGameOverListener(OnGameOver);
		base.OnDecommissionGame();
	}

	public override float? GetThinkEmoteDelayOverride()
	{
		return 50f + Random.Range(0f, 20f);
	}

	public override void StartGameplaySoundtracks()
	{
		MusicManager.Get().StartPlaylist(MusicPlaylistType.InGame_BOT);
	}

	public override void StartMulliganSoundtracks(bool soft)
	{
		if (!soft)
		{
			MusicManager.Get().StartPlaylist(MusicPlaylistType.InGame_BOTMulligan);
		}
	}

	private IEnumerator ShowEndTurnReminderIfNeeded()
	{
		yield return new WaitForSeconds(1f);
		Network.Options options = GameState.Get().GetOptionsPacket();
		if (options != null && !options.HasValidOption() && !s_shownEndTurnReminder)
		{
			s_shownEndTurnReminder = true;
			GameState.Get().UnregisterOptionsReceivedListener(OnOptionsReceived);
			Vector3 endTurnPos = EndTurnButton.Get().transform.position;
			endTurnPos.x -= 3.1f;
			m_endTurnReminder = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, endTurnPos, TutorialEntity.GetTextScale(), GameStrings.Get("BOTA_PUZZLE_END_TURN_REMINDER"));
			m_endTurnReminder.ShowPopUpArrow(Notification.PopUpArrowDirection.Right);
			m_endTurnReminderCoroutine = null;
		}
	}

	private void OnOptionsReceived(object userData)
	{
		if (!SpectatorManager.Get().IsInSpectatorMode())
		{
			if (m_endTurnReminderCoroutine != null)
			{
				Gameplay.Get().StopCoroutine(m_endTurnReminderCoroutine);
				m_endTurnReminderCoroutine = null;
			}
			Network.Options options = GameState.Get().GetOptionsPacket();
			if (options == null)
			{
				Log.Gameplay.PrintError("BOTA_MissionEntity wants options packet but option packet is null.");
			}
			else if (!s_shownEndTurnReminder && !options.HasValidOption())
			{
				m_endTurnReminderCoroutine = Gameplay.Get().StartCoroutine(ShowEndTurnReminderIfNeeded());
			}
		}
	}

	private void OnGameOver(TAG_PLAYSTATE playState, object userData)
	{
		DestroyEndTurnReminder();
	}

	public override void NotifyOfResetGameStarted()
	{
		base.NotifyOfResetGameStarted();
		DestroyEndTurnReminder();
	}

	public override void NotifyOfResetGameFinished(Entity source, Entity oldGameEntity)
	{
		m_waitingForTurnStartIndicatorAfterReset = true;
		BOTA_MissionEntity oldPuzzleEntity = oldGameEntity as BOTA_MissionEntity;
		s_lethalCompleteLines = oldPuzzleEntity.s_lethalCompleteLines;
		lethalLineUsed = oldPuzzleEntity.lethalLineUsed;
		m_randomEmoteLines = oldPuzzleEntity.m_randomEmoteLines;
		m_randomIdleLines = oldPuzzleEntity.m_randomIdleLines;
		m_randomRestartLines = oldPuzzleEntity.m_randomRestartLines;
		base.NotifyOfResetGameFinished(source, oldGameEntity);
	}

	public override void OnTurnStartManagerFinished()
	{
		if (!m_waitingForTurnStartIndicatorAfterReset || GameState.Get().GetGameEntity().GetTag(GAME_TAG.PREVIOUS_PUZZLE_COMPLETED) != 0)
		{
			Gameplay.Get().StartCoroutine(OnTurnStartManagerFinishedWithTiming());
		}
	}

	public virtual IEnumerator OnTurnStartManagerFinishedWithTiming()
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		yield return RespondToPuzzleStartWithTiming();
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		int puzzleProgress = GameState.Get().GetFriendlySidePlayer().GetSecretZone()
			.GetPuzzleEntity()
			.GetTag(GAME_TAG.PUZZLE_PROGRESS);
		string voiceLine = GetPuzzleVictoryLine(puzzleProgress);
		if (voiceLine != null)
		{
			yield return PlayBossLine(enemyActor, voiceLine);
		}
	}

	protected virtual IEnumerator RespondToPuzzleStartWithTiming()
	{
		yield break;
	}

	private void DestroyEndTurnReminder()
	{
		if (m_endTurnReminderCoroutine != null)
		{
			Gameplay.Get().StopCoroutine(m_endTurnReminderCoroutine);
			m_endTurnReminderCoroutine = null;
		}
		if (m_endTurnReminder != null)
		{
			NotificationManager.Get().DestroyNotification(m_endTurnReminder, 0f);
		}
	}

	public override bool NotifyOfEndTurnButtonPushed()
	{
		DestroyEndTurnReminder();
		return true;
	}

	public override IEnumerator DoGameSpecificPostIntroActions()
	{
		m_entranceFinished = false;
		m_confirmButtonPressed = false;
		int currentPuzzleProgress = 0;
		int totalPuzzleProgress = 0;
		string puzzleName = "";
		string puzzleText = "";
		TAG_PUZZLE_TYPE puzzleType = TAG_PUZZLE_TYPE.INVALID;
		int maxNumAttempts = 2;
		if (HearthstoneApplication.IsPublic())
		{
			maxNumAttempts = 10;
		}
		bool puzzleInfoFound = false;
		for (int i = 0; i < maxNumAttempts; i++)
		{
			puzzleInfoFound = LookUpPuzzleInfoFromFutureTaskLists(out currentPuzzleProgress, out totalPuzzleProgress, out puzzleName, out puzzleText, out puzzleType);
			if (puzzleInfoFound)
			{
				break;
			}
			yield return new WaitForSeconds(1f);
		}
		if (!puzzleInfoFound)
		{
			Log.Spells.PrintError("BOTA_MissionEntity.DoGameSpecificPostIntroActions(): puzzle info could not be found in the task lists - most likely the script for this game entity is not setting up a puzzle entity correctly.");
			if (puzzleType == TAG_PUZZLE_TYPE.INVALID)
			{
				yield break;
			}
		}
		GameObject puzzleIntroUI = LoadIntroUIForPuzzleType(puzzleType);
		PuzzleProgressUI progressUI = puzzleIntroUI.GetComponent<PuzzleProgressUI>();
		if (progressUI == null)
		{
			Log.Spells.PrintError("BOTA_MissionEntity.DoGameSpecificPostIntroActions(): No PuzzleProgressUI found on puzzle intro spell {0}.", puzzleIntroUI.gameObject.name);
			yield break;
		}
		progressUI.UpdateNameAndText(puzzleName, puzzleText);
		progressUI.UpdateProgressValues(currentPuzzleProgress, totalPuzzleProgress);
		m_introSpell = puzzleIntroUI.GetComponent<PuzzleIntroSpell>();
		if (m_introSpell == null)
		{
			Log.Spells.PrintError("BOTA_MissionEntity.DoGameSpecificPostIntroActions(): No PuzzleIntroSpell found on puzzle intro spell {0}.", puzzleIntroUI.gameObject.name);
			yield break;
		}
		if (m_introSpell.GetConfirmButton() == null)
		{
			Log.Spells.PrintError("BOTA_MissionEntity.DoGameSpecificPostIntroActions(): No confirmButton found on puzzle intro spell {0}.", puzzleIntroUI.gameObject.name);
			yield break;
		}
		m_confirmButton = m_introSpell.GetConfirmButton().GetComponentInChildren<NormalButton>();
		if (m_confirmButton == null)
		{
			Log.Spells.PrintError($"BOTA_MissionEntity.DoGameSpecificPostIntroActions() - ERROR \"{m_introSpell.GetConfirmButton()}\" has no {typeof(NormalButton)} component");
			yield break;
		}
		m_introSpell.AddSpellEventCallback(OnSpellEvent);
		m_confirmButton.SetText(GameStrings.Get("GLOBAL_CONFIRM"));
		m_confirmButton.AddEventListener(UIEventType.RELEASE, OnConfirmButtonReleased);
		m_confirmButton.GetComponent<Collider>().enabled = true;
		m_confirmButton.m_button.GetComponent<PlayMakerFSM>().SendEvent("Birth");
		m_introSpell.ActivateState(SpellStateType.BIRTH);
		while (m_introSpell != null && !m_introSpell.IsFinished())
		{
			if (GameState.Get().WasConcedeRequested())
			{
				if (!m_confirmButtonPressed && m_entranceFinished)
				{
					m_confirmButton.SetEnabled(enabled: false);
					ProgressPastConfirmButton();
				}
				yield break;
			}
			yield return null;
		}
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
			.GetActor();
		if (currentPuzzleProgress == 1)
		{
			Gameplay.Get().StartCoroutine(PlayBossLine(enemyActor, s_introLine));
		}
		else if (s_returnLineOverride)
		{
			GameEntity gameEntity = GameState.Get().GetGameEntity();
			gameEntity.SetTag(GAME_TAG.MISSION_EVENT, 77);
			gameEntity.SetTag(GAME_TAG.MISSION_EVENT, 0);
		}
		else
		{
			Gameplay.Get().StartCoroutine(PlayBossLine(enemyActor, s_returnLine));
		}
	}

	private GameObject LoadIntroUIForPuzzleType(TAG_PUZZLE_TYPE puzzleType)
	{
		switch (puzzleType)
		{
		case TAG_PUZZLE_TYPE.INVALID:
			Log.Spells.PrintError($"BOTA_MissionEntity.LoadIntroUIForPuzzleType() - invalid puzzle type");
			return null;
		case TAG_PUZZLE_TYPE.MIRROR:
			return AssetLoader.Get().InstantiatePrefab(PuzzleIntroUI_Mirror);
		case TAG_PUZZLE_TYPE.LETHAL:
			return AssetLoader.Get().InstantiatePrefab(PuzzleIntroUI_Lethal);
		case TAG_PUZZLE_TYPE.SURVIVAL:
			return AssetLoader.Get().InstantiatePrefab(PuzzleIntroUI_Survival);
		case TAG_PUZZLE_TYPE.CLEAR:
			return AssetLoader.Get().InstantiatePrefab(PuzzleIntroUI_Clear);
		default:
			return null;
		}
	}

	private bool LookUpPuzzleInfoFromFutureTaskLists(out int currentPuzzleProgress, out int totalPuzzleProgress, out string puzzleName, out string puzzleText, out TAG_PUZZLE_TYPE puzzleType)
	{
		int currentPuzzleProgressFound = 0;
		int totalPuzzleProgressFound = 0;
		string puzzleNameFound = "";
		string puzzleTextFound = "";
		TAG_PUZZLE_TYPE puzzleTypeFound = TAG_PUZZLE_TYPE.INVALID;
		bool puzzleInfoFound = false;
		GameState.Get().GetPowerProcessor().ForEachTaskList(delegate(int index, PowerTaskList taskList)
		{
			if (currentPuzzleProgressFound != 0 && totalPuzzleProgressFound != 0)
			{
				return;
			}
			foreach (PowerTask task in taskList.GetTaskList())
			{
				Network.PowerHistory power = task.GetPower();
				if (power.Type == Network.PowerType.FULL_ENTITY)
				{
					Network.HistFullEntity histFullEntity = power as Network.HistFullEntity;
					Network.Entity.Tag tag2 = histFullEntity.Entity.Tags.Find((Network.Entity.Tag tag) => tag.Name == 982);
					if (tag2 != null)
					{
						puzzleTypeFound = (TAG_PUZZLE_TYPE)tag2.Value;
						CardDbfRecord cardRecord = GameDbf.GetIndex().GetCardRecord(histFullEntity.Entity.CardID);
						if (cardRecord != null && cardRecord.Name != null && cardRecord.TextInHand != null)
						{
							puzzleNameFound = cardRecord.Name;
							puzzleTextFound = cardRecord.TextInHand;
						}
					}
				}
				if (power.Type == Network.PowerType.TAG_CHANGE)
				{
					Network.HistTagChange histTagChange = power as Network.HistTagChange;
					if (histTagChange.Tag == 980 && histTagChange.Value != 0)
					{
						currentPuzzleProgressFound = histTagChange.Value;
					}
					if (histTagChange.Tag == 981 && histTagChange.Value != 0)
					{
						totalPuzzleProgressFound = histTagChange.Value;
					}
					if (currentPuzzleProgressFound != 0 && totalPuzzleProgressFound != 0)
					{
						puzzleInfoFound = true;
						break;
					}
				}
			}
		});
		currentPuzzleProgress = currentPuzzleProgressFound;
		totalPuzzleProgress = totalPuzzleProgressFound;
		puzzleName = puzzleNameFound;
		puzzleText = puzzleTextFound;
		puzzleType = puzzleTypeFound;
		return puzzleInfoFound;
	}

	private void OnConfirmButtonReleased(UIEvent e)
	{
		if (!GameMgr.Get().IsSpectator())
		{
			((NormalButton)e.GetElement()).SetEnabled(enabled: false);
			m_confirmButtonPressed = true;
			bool concedeRequested = GameState.Get().WasConcedeRequested();
			if (m_entranceFinished || concedeRequested)
			{
				ProgressPastConfirmButton();
			}
		}
	}

	private void ProgressPastConfirmButton()
	{
		m_introSpell.ActivateState(SpellStateType.DEATH);
		m_confirmButton.m_button.GetComponent<PlayMakerFSM>().SendEvent("Death");
	}

	private void OnSpellEvent(string eventName, object eventData, object userData)
	{
		if (eventName == "EntranceFinished")
		{
			bool concedeRequested = GameState.Get().WasConcedeRequested();
			m_entranceFinished = true;
			if (m_confirmButtonPressed || concedeRequested)
			{
				ProgressPastConfirmButton();
			}
		}
	}

	protected virtual List<string> GetBossHeroPowerRandomLines()
	{
		return new List<string>();
	}

	protected virtual string GetBossDeathLine()
	{
		return null;
	}

	protected virtual bool GetShouldSupressDeathTextBubble()
	{
		return false;
	}

	protected virtual float ChanceToPlayBossHeroPowerVOLine()
	{
		return 0.5f;
	}

	protected override float ChanceToPlayRandomVOLine()
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

	public override void NotifyOfGameOver(TAG_PLAYSTATE gameResult)
	{
		base.NotifyOfGameOver(gameResult);
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
			.GetActor();
		string voLine = GetBossDeathLine();
		if (!m_enemySpeaking && !string.IsNullOrEmpty(voLine) && gameResult == TAG_PLAYSTATE.WON)
		{
			if (GetShouldSupressDeathTextBubble())
			{
				Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeech(voLine, Notification.SpeechBubbleDirection.None, enemyActor));
			}
			else
			{
				Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeech(voLine, Notification.SpeechBubbleDirection.TopRight, enemyActor));
			}
		}
	}

	protected override void PlayEmoteResponse(EmoteType emoteType, CardSoundSpell emoteSpell)
	{
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
			.GetActor();
		if (MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS.Contains(emoteType))
		{
			if (m_randomEmoteLines.Count == 0)
			{
				m_randomEmoteLines = new List<string>(s_emoteLines);
			}
			string voiceLine = PopRandomLineWithChance(m_randomEmoteLines);
			if (voiceLine != null)
			{
				Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeech(voiceLine, Notification.SpeechBubbleDirection.TopRight, enemyActor));
			}
		}
	}

	public override void OnPlayThinkEmote()
	{
		if (m_enemySpeaking)
		{
			return;
		}
		Player currentPlayer = GameState.Get().GetCurrentPlayer();
		if (currentPlayer.IsFriendlySide() && !currentPlayer.GetHeroCard().HasActiveEmoteSound())
		{
			Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
				.GetActor();
			if (m_randomIdleLines.Count == 0)
			{
				m_randomIdleLines = new List<string>(s_idleLines);
			}
			string voiceLine = PopRandomLineWithChance(m_randomIdleLines);
			if (voiceLine != null)
			{
				Gameplay.Get().StartCoroutine(PlayBossLine(enemyActor, voiceLine));
			}
		}
	}

	protected override IEnumerator RespondToResetGameFinishedWithTiming(Entity entity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		while (entity.GetCardType() == TAG_CARDTYPE.INVALID)
		{
			yield return null;
		}
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		if (GameState.Get().GetGameEntity().GetTag(GAME_TAG.PREVIOUS_PUZZLE_COMPLETED) == 0)
		{
			if (m_randomRestartLines.Count == 0)
			{
				m_randomRestartLines = new List<string>(s_restartLines);
			}
			string voiceLine = PopRandomLineWithChance(m_randomRestartLines);
			if (voiceLine != null)
			{
				Gameplay.Get().StartCoroutine(PlayBossLine(enemyActor, voiceLine));
			}
		}
	}

	private string GetPuzzleVictoryLine(int puzzleProgress)
	{
		return puzzleProgress switch
		{
			1 => s_victoryLine_1, 
			2 => s_victoryLine_2, 
			3 => s_victoryLine_3, 
			4 => s_victoryLine_4, 
			5 => s_victoryLine_5, 
			6 => s_victoryLine_6, 
			7 => s_victoryLine_7, 
			8 => s_victoryLine_8, 
			9 => s_victoryLine_9, 
			_ => null, 
		};
	}

	protected string GetLethalCompleteLine()
	{
		if (s_lethalCompleteLines.Count == 0)
		{
			return null;
		}
		if (m_enemySpeaking)
		{
			return null;
		}
		if (lethalLineUsed && Random.Range(0, 100) >= 85)
		{
			return null;
		}
		lethalLineUsed = true;
		int index = Random.Range(0, s_lethalCompleteLines.Count);
		string s = s_lethalCompleteLines[index];
		s_lethalCompleteLines.Remove(s);
		return s;
	}
}
