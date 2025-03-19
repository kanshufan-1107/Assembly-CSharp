using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_815H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_Story_02_Quilboar_Male_Quillboar_Story_Rexxar_Mission4Death_01 = new AssetReference("VO_Story_02_Quilboar_Male_Quillboar_Story_Rexxar_Mission4Death_01.prefab:360502f78bbb40d4b66dffc00d88620f");

	private static readonly AssetReference VO_Story_02_Quilboar_Male_Quillboar_Story_Rexxar_Mission4EmoteResponse_01 = new AssetReference("VO_Story_02_Quilboar_Male_Quillboar_Story_Rexxar_Mission4EmoteResponse_01.prefab:21be207809e8454488283ee3034b15a0");

	private static readonly AssetReference VO_Story_02_Quilboar_Male_Quillboar_Story_Rexxar_Mission4HeroPower_03 = new AssetReference("VO_Story_02_Quilboar_Male_Quillboar_Story_Rexxar_Mission4HeroPower_03.prefab:80a4e2d96e044b3d9dee1b7b8c261d38");

	private List<string> m_IdleLines = new List<string> { VO_Story_02_Quilboar_Male_Quillboar_Story_Rexxar_Mission4EmoteResponse_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_Story_02_Quilboar_Male_Quillboar_Story_Rexxar_Mission4Death_01, VO_Story_02_Quilboar_Male_Quillboar_Story_Rexxar_Mission4EmoteResponse_01, VO_Story_02_Quilboar_Male_Quillboar_Story_Rexxar_Mission4HeroPower_03 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_Story_02_Quilboar_Male_Quillboar_Story_Rexxar_Mission4HeroPower_03;
		m_deathLine = VO_Story_02_Quilboar_Male_Quillboar_Story_Rexxar_Mission4Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_815H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_815H")
		{
			switch (cardID)
			{
			case "LETL_815P1_01":
			case "LETL_815P1_02":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_Story_02_Quilboar_Male_Quillboar_Story_Rexxar_Mission4EmoteResponse_01);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_815P2_01":
			case "LETL_815P2_02":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_Story_02_Quilboar_Male_Quillboar_Story_Rexxar_Mission4EmoteResponse_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_815H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_815H");
		if (entity.GetCardId() == "LETL_815H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_815H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
