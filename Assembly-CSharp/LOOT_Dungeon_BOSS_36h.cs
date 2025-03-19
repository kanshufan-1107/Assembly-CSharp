using System.Collections;
using System.Collections.Generic;

public class LOOT_Dungeon_BOSS_36h : LOOT_Dungeon
{
	private static readonly AssetReference VO_LOOT_329_Male_Elemental_Attack_01 = new AssetReference("VO_LOOT_329_Male_Elemental_Attack_01.prefab:c263e413027e1c3419330793de1a9b83");

	private static readonly AssetReference VO_LOOT_329_Male_Elemental_Death_01 = new AssetReference("VO_LOOT_329_Male_Elemental_Death_01.prefab:7fda354ba88198d4992562d4c9b51373");

	private static readonly AssetReference VO_LOOT_329_Male_Elemental_Play_01 = new AssetReference("VO_LOOT_329_Male_Elemental_Play_01.prefab:acccd0bdaf7b3964d8c782e6191599c5");

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		foreach (string soundFile in new List<string> { VO_LOOT_329_Male_Elemental_Attack_01, VO_LOOT_329_Male_Elemental_Death_01, VO_LOOT_329_Male_Elemental_Play_01 })
		{
			PreloadSound(soundFile);
		}
	}

	protected override string GetBossDeathLine()
	{
		return VO_LOOT_329_Male_Elemental_Death_01;
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
			Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeech(VO_LOOT_329_Male_Elemental_Play_01, Notification.SpeechBubbleDirection.TopRight, enemyActor));
		}
		else if (MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS.Contains(emoteType))
		{
			Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeech(VO_LOOT_329_Male_Elemental_Attack_01, Notification.SpeechBubbleDirection.TopRight, enemyActor));
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
