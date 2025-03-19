using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT24_814H4_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_BigBadWolf_Male_Beast_LETL_Attack_01 = new AssetReference("VO_BigBadWolf_Male_Beast_LETL_Attack_01.prefab:b01dcd5a588dd1d4990316c9b85e3ad1");

	private static readonly AssetReference VO_BigBadWolf_Male_Beast_LETL_Attack_02 = new AssetReference("VO_BigBadWolf_Male_Beast_LETL_Attack_02.prefab:2cf0dff6908c98b468979a30288372bb");

	private static readonly AssetReference VO_BigBadWolf_Male_Beast_LETL_Death_01 = new AssetReference("VO_BigBadWolf_Male_Beast_LETL_Death_01.prefab:062f7d663111ed24eae395e3325be12f");

	private static readonly AssetReference VO_BigBadWolf_Male_Beast_LETL_Idle_01 = new AssetReference("VO_BigBadWolf_Male_Beast_LETL_Idle_01.prefab:abbc8f51dcf4eaa48ba5b2427230633b");

	private static readonly AssetReference VO_BigBadWolf_Male_Beast_LETL_Intro_01 = new AssetReference("VO_BigBadWolf_Male_Beast_LETL_Intro_01.prefab:87643dd8f103a3f4e81adace37af1a1f");

	private List<string> m_IdleLines = new List<string> { VO_BigBadWolf_Male_Beast_LETL_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_BigBadWolf_Male_Beast_LETL_Intro_01, VO_BigBadWolf_Male_Beast_LETL_Idle_01, VO_BigBadWolf_Male_Beast_LETL_Attack_01, VO_BigBadWolf_Male_Beast_LETL_Attack_02, VO_BigBadWolf_Male_Beast_LETL_Death_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_BigBadWolf_Male_Beast_LETL_Intro_01;
		m_deathLine = VO_BigBadWolf_Male_Beast_LETL_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_814H4");
		if (!(playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT24_814H4"))
		{
			yield break;
		}
		if (!(cardID == "LT24_814P3"))
		{
			if (cardID == "LT24_814P4")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_BigBadWolf_Male_Beast_LETL_Attack_01);
				GameState.Get().SetBusy(busy: false);
			}
		}
		else
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_BigBadWolf_Male_Beast_LETL_Attack_02);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_814H4");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_814H4");
		if (entity.GetCardId() == "LT24_814H4")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_814H4");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
