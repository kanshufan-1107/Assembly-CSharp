using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_848H_Heroic_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_LETL_848H_Male_Dragon_Attack_01 = new AssetReference("VO_LETL_848H_Male_Dragon_Attack_01.prefab:f55ed48d239634e4abd8f4e085ac4454");

	private static readonly AssetReference VO_LETL_848H_Male_Dragon_Attack_02 = new AssetReference("VO_LETL_848H_Male_Dragon_Attack_02.prefab:598ed5033b12a294d9898622a46f27d6");

	private static readonly AssetReference VO_LETL_848H_Male_Dragon_Death_01 = new AssetReference("VO_LETL_848H_Male_Dragon_Death_01.prefab:33dd5718a67c9694992450d43085bce2");

	private static readonly AssetReference VO_LETL_848H_Male_Dragon_Idle_01 = new AssetReference("VO_LETL_848H_Male_Dragon_Idle_01.prefab:f745f9529a6a0b1458dd691a2597b47f");

	private static readonly AssetReference VO_LETL_848H_Male_Dragon_Intro_01 = new AssetReference("VO_LETL_848H_Male_Dragon_Intro_01.prefab:f4a87ff5f174b6642b8bc30f6e582fe4");

	private static readonly AssetReference VO_LETL_848H_Male_Dragon_Intro_02 = new AssetReference("VO_LETL_848H_Male_Dragon_Intro_02.prefab:64af15efdd377204ab98f047f42f2dbb");

	private static readonly AssetReference VO_HERO_03q_Female_BloodElf_MIRROR_START_01 = new AssetReference("VO_HERO_03q_Female_BloodElf_MIRROR_START_01.prefab:31df6829fcf37d145bd546f76f1a029f");

	private List<string> m_IdleLines = new List<string> { VO_LETL_848H_Male_Dragon_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_LETL_848H_Male_Dragon_Attack_01, VO_LETL_848H_Male_Dragon_Attack_02, VO_LETL_848H_Male_Dragon_Death_01, VO_LETL_848H_Male_Dragon_Idle_01, VO_LETL_848H_Male_Dragon_Intro_01, VO_LETL_848H_Male_Dragon_Intro_02, VO_HERO_03q_Female_BloodElf_MIRROR_START_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_LETL_848H_Male_Dragon_Intro_01;
		m_deathLine = VO_LETL_848H_Male_Dragon_Death_01;
		m_standardEmoteResponseLine = VO_HERO_03q_Female_BloodElf_MIRROR_START_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_848H4_Heroic");
		if (!(playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_848H4_Heroic"))
		{
			yield break;
		}
		if (!(cardID == "LETL_848P1_01"))
		{
			if (cardID == "LETL_848P1_03")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LETL_848H_Male_Dragon_Attack_02);
				GameState.Get().SetBusy(busy: false);
			}
		}
		else
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_LETL_848H_Male_Dragon_Attack_01);
			GameState.Get().SetBusy(busy: false);
		}
	}

	public override List<string> GetIdleLines()
	{
		return m_IdleLines;
	}

	public override IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_848H_Heroic");
		Actor valeeraActor = FindActorInPlayByDesignCode("LETL_848H6");
		switch (missionEvent)
		{
		case 517:
			yield return MissionPlayVO(bossActor, m_IdleLines);
			break;
		case 514:
			yield return MissionPlayVO(bossActor, m_introLine);
			yield return MissionPlayVO(valeeraActor, m_standardEmoteResponseLine);
			break;
		default:
			yield return base.HandleMissionEventWithTiming(missionEvent);
			break;
		}
	}

	public override void NotifyOfMinionDied(Entity entity)
	{
		Gameplay.Get().StartCoroutine(NotifyOfMinionDiedWithTiming(entity));
	}

	public IEnumerator NotifyOfMinionDiedWithTiming(Entity entity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_848H_Heroic");
		if (entity.GetCardId() == "LETL_848H_Heroic")
		{
			yield return MissionPlaySound(bossActor, m_deathLine);
		}
	}

	public override IEnumerator HandleStartOfTurnWithTiming(int turn)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_848H_Heroic");
		Actor valeeraActor = FindActorInPlayByDesignCode("LETL_848H6");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
			yield return MissionPlayVOOnce(valeeraActor, m_standardEmoteResponseLine);
		}
	}
}
