using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT24_818H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_ShadeofAran_Male_Undead_LETL_Attack_01 = new AssetReference("VO_ShadeofAran_Male_Undead_LETL_Attack_01.prefab:b4917909bf4823942995ec35065c8594");

	private static readonly AssetReference VO_ShadeofAran_Male_Undead_LETL_Attack_02 = new AssetReference("VO_ShadeofAran_Male_Undead_LETL_Attack_02.prefab:01eafa730a650cc40a5110ef3972dfa1");

	private static readonly AssetReference VO_ShadeofAran_Male_Undead_LETL_Death_01 = new AssetReference("VO_ShadeofAran_Male_Undead_LETL_Death_01.prefab:0519375b43afaa941b4da6374d3933a8");

	private static readonly AssetReference VO_ShadeofAran_Male_Undead_LETL_Idle_01 = new AssetReference("VO_ShadeofAran_Male_Undead_LETL_Idle_01.prefab:af2aee734a9518c4883fb77c8eaeb48c");

	private static readonly AssetReference VO_ShadeofAran_Male_Undead_LETL_Idle_02 = new AssetReference("VO_ShadeofAran_Male_Undead_LETL_Idle_02.prefab:7db9ff3a4b902254597138b69b70ad81");

	private static readonly AssetReference VO_ShadeofAran_Male_Undead_LETL_Intro_01 = new AssetReference("VO_ShadeofAran_Male_Undead_LETL_Intro_01.prefab:7322048d76f42964bb67a78a01cd5ab7");

	private static readonly AssetReference VO_ShadeofAran_Male_Undead_LETL_Intro_02 = new AssetReference("VO_ShadeofAran_Male_Undead_LETL_Intro_02.prefab:cf36cf4ccbc257742822063a8327f505");

	private List<string> m_IdleLines = new List<string> { VO_ShadeofAran_Male_Undead_LETL_Idle_01, VO_ShadeofAran_Male_Undead_LETL_Idle_02 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_ShadeofAran_Male_Undead_LETL_Intro_01, VO_ShadeofAran_Male_Undead_LETL_Attack_01, VO_ShadeofAran_Male_Undead_LETL_Attack_02, VO_ShadeofAran_Male_Undead_LETL_Idle_01, VO_ShadeofAran_Male_Undead_LETL_Idle_02, VO_ShadeofAran_Male_Undead_LETL_Death_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_ShadeofAran_Male_Undead_LETL_Intro_01;
		m_deathLine = VO_ShadeofAran_Male_Undead_LETL_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_818H");
		if (!(playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT24_818H"))
		{
			yield break;
		}
		if (!(cardID == "LT24_818P1"))
		{
			if (cardID == "LT23_803P2")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_ShadeofAran_Male_Undead_LETL_Attack_02);
				GameState.Get().SetBusy(busy: false);
			}
		}
		else
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_ShadeofAran_Male_Undead_LETL_Attack_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_818H");
		switch (missionEvent)
		{
		case 517:
			yield return MissionPlayVO(bossActor, m_IdleLines);
			break;
		case 514:
			yield return MissionPlayVO(bossActor, m_introLine);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_818H");
		if (entity.GetCardId() == "LT24_818H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_818H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
