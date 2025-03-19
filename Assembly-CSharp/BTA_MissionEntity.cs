using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public abstract class BTA_MissionEntity : GenericDungeonMissionEntity
{
	public static class MemberInfoGetting
	{
		public static string GetMemberName<T>(Expression<Func<T>> memberExpression)
		{
			return ((MemberExpression)memberExpression.Body).Member.Name;
		}
	}

	public sealed override AdventureDbId GetAdventureID()
	{
		return AdventureDbId.BTA;
	}

	protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
	{
		if (gameResult == TAG_PLAYSTATE.LOST)
		{
			yield return new WaitForSeconds(5f);
		}
	}

	public override bool ShouldShowHeroClassDuringMulligan(Player.Side playerSide)
	{
		return false;
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
			GameState.Get().GetCurrentPlayer().GetHeroCard()
				.PlayEmote(thinkEmote);
		}
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
		MusicManager.Get().StartPlaylist(MusicPlaylistType.InGame_BT);
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

	protected string PopRandomLine(List<string> lines)
	{
		if (lines == null || lines.Count == 0)
		{
			return null;
		}
		string randomLine = lines[UnityEngine.Random.Range(0, lines.Count)];
		lines.Remove(randomLine);
		return randomLine;
	}

	protected IEnumerator PlayLineAlways(Actor speaker, string line, Notification.SpeechBubbleDirection direction, float duration = 2.5f)
	{
		yield return PlayLine(speaker, line, base.InternalShouldPlayBossLine, duration, direction);
	}

	protected IEnumerator PlayLine(Actor speaker, string line, ShouldPlay shouldPlay, float duration, Notification.SpeechBubbleDirection direction)
	{
		if (m_enemySpeaking)
		{
			yield return null;
		}
		m_enemySpeaking = true;
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
