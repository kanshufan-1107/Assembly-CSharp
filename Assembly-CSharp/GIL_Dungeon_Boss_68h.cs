using System.Collections;
using System.Collections.Generic;

public class GIL_Dungeon_Boss_68h : GIL_Dungeon
{
	private HashSet<string> m_playedLines = new HashSet<string>();

	private List<string> m_PoisonMinionLines = new List<string> { "VO_GILA_BOSS_68h_Male_Undead_EventPlayPoison_01.prefab:07927380484b26541be4b49e7a8aad33", "VO_GILA_BOSS_68h_Male_Undead_EventPlayPoison_02.prefab:ef7a4e3aea34db34caa72aa721f6ee45", "VO_GILA_BOSS_68h_Male_Undead_EventPlayPoison_03.prefab:dc750561f86b04b48bdc5e3516c6b41a", "VO_GILA_BOSS_68h_Male_Undead_EventPlayPoison_04.prefab:057102807e5fe1943b8b8780ba1c37a3" };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		foreach (string soundFile in new List<string> { "VO_GILA_BOSS_68h_Male_Undead_Intro_01.prefab:d41182657f7477847966469d4eb6fc08", "VO_GILA_BOSS_68h_Male_Undead_EmoteResponse_01.prefab:e1dfb4ab0b0331a4abcb536c5896186a", "VO_GILA_BOSS_68h_Male_Undead_Death_01.prefab:75f7dabf0922e044aac6a4f8a7315238", "VO_GILA_BOSS_68h_Male_Undead_EventPlayPoison_01.prefab:07927380484b26541be4b49e7a8aad33", "VO_GILA_BOSS_68h_Male_Undead_EventPlayPoison_02.prefab:ef7a4e3aea34db34caa72aa721f6ee45", "VO_GILA_BOSS_68h_Male_Undead_EventPlayPoison_03.prefab:dc750561f86b04b48bdc5e3516c6b41a", "VO_GILA_BOSS_68h_Male_Undead_EventPlayPoison_04.prefab:057102807e5fe1943b8b8780ba1c37a3" })
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
		return "VO_GILA_BOSS_68h_Male_Undead_Death_01.prefab:75f7dabf0922e044aac6a4f8a7315238";
	}

	protected override bool GetShouldSupressDeathTextBubble()
	{
		return true;
	}

	protected override void PlayEmoteResponse(EmoteType emoteType, CardSoundSpell emoteSpell)
	{
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
			.GetActor();
		if (emoteType == EmoteType.START)
		{
			Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeech("VO_GILA_BOSS_68h_Male_Undead_Intro_01.prefab:d41182657f7477847966469d4eb6fc08", Notification.SpeechBubbleDirection.TopRight, enemyActor));
		}
		else if (MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS.Contains(emoteType))
		{
			Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeech("VO_GILA_BOSS_68h_Male_Undead_EmoteResponse_01.prefab:e1dfb4ab0b0331a4abcb536c5896186a", Notification.SpeechBubbleDirection.TopRight, enemyActor));
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
		if (entity.HasTag(GAME_TAG.POISONOUS))
		{
			string randomLine = PopRandomLineWithChance(m_PoisonMinionLines);
			if (randomLine != null)
			{
				yield return PlayLineOnlyOnce(enemyActor, randomLine);
			}
		}
	}
}
