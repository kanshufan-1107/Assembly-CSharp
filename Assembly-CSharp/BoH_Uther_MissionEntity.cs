using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Blizzard.T5.Core;
using UnityEngine;

public abstract class BoH_Uther_MissionEntity : GenericDungeonMissionEntity
{
	public static class MemberInfoGetting
	{
		public static string GetMemberName<T>(Expression<Func<T>> memberExpression)
		{
			return ((MemberExpression)memberExpression.Body).Member.Name;
		}
	}

	public bool m_MissionDisableAutomaticVO;

	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	public sealed override AdventureDbId GetAdventureID()
	{
		return AdventureDbId.BOH;
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
		if (currentPlayer.IsFriendlySide() && !m_MissionDisableAutomaticVO && !currentPlayer.GetHeroCard().HasActiveEmoteSound())
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
			MusicManager.Get().StartPlaylist(MusicPlaylistType.InGame_Mulligan);
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

	public BoH_Uther_MissionEntity()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}
}
