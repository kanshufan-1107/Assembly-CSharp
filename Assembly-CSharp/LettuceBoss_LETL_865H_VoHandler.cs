using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_865H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_Onyxia_Female_Dragon_Intro_01 = new AssetReference("VO_Onyxia_Female_Dragon_Intro_01.prefab:8214c49a90c32974a8d5cedd34f0b599");

	private static readonly AssetReference VO_Onyxia_Female_Dragon_Fluff_04 = new AssetReference("VO_Onyxia_Female_Dragon_Fluff_04.prefab:ab97e790e3b290b498d19500d3e6cdaa");

	private static readonly AssetReference VO_Onyxia_Female_Dragon_Attack_01 = new AssetReference("VO_Onyxia_Female_Dragon_Attack_01.prefab:4737e827b73ebb84da50b4748b066310");

	private static readonly AssetReference VO_Onyxia_Female_Dragon_PhaseTransition_02 = new AssetReference("VO_Onyxia_Female_Dragon_PhaseTransition_02.prefab:d0cec20304372a3468f28f3051a789a4");

	private static readonly AssetReference Death = new AssetReference("Death.prefab:b6bd9ea0e27442b4b8ee69edb1b18f41");

	private List<string> m_IdleLines = new List<string> { VO_Onyxia_Female_Dragon_PhaseTransition_02 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_Onyxia_Female_Dragon_Intro_01, VO_Onyxia_Female_Dragon_Fluff_04, VO_Onyxia_Female_Dragon_Attack_01, VO_Onyxia_Female_Dragon_PhaseTransition_02, Death };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_Onyxia_Female_Dragon_Intro_01;
		m_deathLine = Death;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_865H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_865H")
		{
			switch (cardID)
			{
			case "LETL_865P1":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVO(bossActor, VO_Onyxia_Female_Dragon_Fluff_04);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LT22_024P1_03":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVO(bossActor, VO_Onyxia_Female_Dragon_Attack_01);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LT22_024P1_05":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVO(bossActor, VO_Onyxia_Female_Dragon_Attack_01);
				GameState.Get().SetBusy(busy: false);
				break;
			}
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_865H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_865H");
		if (entity.GetCardId() == "LETL_865H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_865H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
