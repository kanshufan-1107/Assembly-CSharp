using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LOOT_Dungeon_BOSS_38h : LOOT_Dungeon
{
	private HashSet<string> m_playedLines = new HashSet<string>();

	private List<string> m_DeathrattleLines = new List<string> { "VO_LOOTA_BOSS_38h_Male_Kobold_EventPlayerDeathrattles1_01.prefab:5876e2505aebd13499f22af506e2fe5e", "VO_LOOTA_BOSS_38h_Male_Kobold_EventPlayerDeathrattles3_01.prefab:28b4a262be6ba4a4b93267be10fb3110" };

	private List<string> m_BattlecryLines = new List<string> { "VO_LOOTA_BOSS_38h_Male_Kobold_EventPlayerBattlecries1_01.prefab:7f36a8303b69b7942a3b4b65165bef87", "VO_LOOTA_BOSS_38h_Male_Kobold_EventPlayerBattlecries2_01.prefab:4bdd4e42d0330a74b807778043a0b531", "VO_LOOTA_BOSS_38h_Male_Kobold_EventPlayerBattlecries3_01.prefab:36764c0f7268d0144b813095fb699cdf" };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		foreach (string soundFile in new List<string> { "VO_LOOTA_BOSS_38h_Male_Kobold_Intro_01.prefab:4949580433bcd1e48bd351d8ff68cfba", "VO_LOOTA_BOSS_38h_Male_Kobold_EmoteResponse_01.prefab:c7c2f50bb6da6bd4b89ad39673ae5b86", "VO_LOOTA_BOSS_38h_Male_Kobold_Death_01.prefab:773c7f8ebca84d9458c47a951283da8b", "VO_LOOTA_BOSS_38h_Male_Kobold_EventBrann_01.prefab:8529209f43f99c44dbd9227432602ee1", "VO_LOOTA_BOSS_38h_Male_Kobold_EventRivendare_01.prefab:1555767ca07a51840aada9900666633d", "VO_LOOTA_BOSS_38h_Male_Kobold_EventPlayerBattlecries1_01.prefab:7f36a8303b69b7942a3b4b65165bef87", "VO_LOOTA_BOSS_38h_Male_Kobold_EventPlayerBattlecries2_01.prefab:4bdd4e42d0330a74b807778043a0b531", "VO_LOOTA_BOSS_38h_Male_Kobold_EventPlayerBattlecries3_01.prefab:36764c0f7268d0144b813095fb699cdf", "VO_LOOTA_BOSS_38h_Male_Kobold_EventPlayerDeathrattles1_01.prefab:5876e2505aebd13499f22af506e2fe5e", "VO_LOOTA_BOSS_38h_Male_Kobold_EventPlayerDeathrattles3_01.prefab:28b4a262be6ba4a4b93267be10fb3110" })
		{
			PreloadSound(soundFile);
		}
	}

	protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
	{
		yield return base.RespondToPlayedCardWithTiming(entity);
	}

	protected override List<string> GetBossHeroPowerRandomLines()
	{
		return new List<string>();
	}

	protected override string GetBossDeathLine()
	{
		return "VO_LOOTA_BOSS_38h_Male_Kobold_Death_01.prefab:773c7f8ebca84d9458c47a951283da8b";
	}

	protected override void PlayEmoteResponse(EmoteType emoteType, CardSoundSpell emoteSpell)
	{
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
			.GetActor();
		if (emoteType == EmoteType.START)
		{
			Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeech("VO_LOOTA_BOSS_38h_Male_Kobold_Intro_01.prefab:4949580433bcd1e48bd351d8ff68cfba", Notification.SpeechBubbleDirection.TopRight, enemyActor));
		}
		else if (MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS.Contains(emoteType))
		{
			Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeech("VO_LOOTA_BOSS_38h_Male_Kobold_EmoteResponse_01.prefab:c7c2f50bb6da6bd4b89ad39673ae5b86", Notification.SpeechBubbleDirection.TopRight, enemyActor));
		}
	}

	protected override IEnumerator RespondToFriendlyPlayedCardWithTiming(Entity entity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		while (entity.GetCardType() == TAG_CARDTYPE.INVALID)
		{
			yield return null;
		}
		if (m_playedLines.Contains(entity.GetCardId()))
		{
			yield break;
		}
		yield return WaitForEntitySoundToFinish(entity);
		string cardID = entity.GetCardId();
		m_playedLines.Add(cardID);
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		if (!(cardID == "FP1_031"))
		{
			if (cardID == "LOE_077")
			{
				yield return PlayEasterEggLine(enemyActor, "VO_LOOTA_BOSS_38h_Male_Kobold_EventBrann_01.prefab:8529209f43f99c44dbd9227432602ee1");
			}
		}
		else
		{
			yield return PlayEasterEggLine(enemyActor, "VO_LOOTA_BOSS_38h_Male_Kobold_EventRivendare_01.prefab:1555767ca07a51840aada9900666633d");
		}
		int chanceVO = 50;
		int randomNum = Random.Range(0, 100);
		if (entity.HasTag(GAME_TAG.BATTLECRY) && chanceVO >= randomNum && m_BattlecryLines.Count != 0)
		{
			string randomLine = m_BattlecryLines[Random.Range(0, m_BattlecryLines.Count)];
			yield return PlayLineOnlyOnce(enemyActor, randomLine);
			m_BattlecryLines.Remove(randomLine);
			yield return null;
		}
		if (entity.HasTag(GAME_TAG.DEATHRATTLE) && chanceVO >= randomNum && m_DeathrattleLines.Count != 0)
		{
			string randomLine = m_DeathrattleLines[Random.Range(0, m_DeathrattleLines.Count)];
			yield return PlayLineOnlyOnce(enemyActor, randomLine);
			m_DeathrattleLines.Remove(randomLine);
			yield return null;
		}
	}

	protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		yield return PlayLoyalSideKickBetrayal(missionEvent);
	}
}
