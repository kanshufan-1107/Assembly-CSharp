using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_822H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_LETL_822H_Male_Quilboar_Attack_01 = new AssetReference("VO_LETL_822H_Male_Quilboar_Attack_01.prefab:5cedb94f231cdc046840154a790830d7");

	private static readonly AssetReference VO_LETL_822H_Male_Quilboar_Attack_02 = new AssetReference("VO_LETL_822H_Male_Quilboar_Attack_02.prefab:c9c546eb243e9b54eb44234e6510a4c9");

	private static readonly AssetReference VO_LETL_822H_Male_Quilboar_Death_01 = new AssetReference("VO_LETL_822H_Male_Quilboar_Death_01.prefab:50fcd304ea21236498127207daab4d73");

	private static readonly AssetReference VO_LETL_822H_Male_Quilboar_Idle_01 = new AssetReference("VO_LETL_822H_Male_Quilboar_Idle_01.prefab:c790d69513d81e547befe33741a891e1");

	private static readonly AssetReference VO_LETL_822H_Male_Quilboar_Intro_01 = new AssetReference("VO_LETL_822H_Male_Quilboar_Intro_01.prefab:78ae619f5af43ef4f975e01328ee46fb");

	private static readonly AssetReference VO_LETL_822H_Male_Quilboar_Intro_02 = new AssetReference("VO_LETL_822H_Male_Quilboar_Intro_02.prefab:0c16f8b62b34fba4ea6c94ae1fedb625");

	private static readonly AssetReference VO_Story_02_Quilboar_Male_Quillboar_Story_Rexxar_Mission4Death_01 = new AssetReference("VO_Story_02_Quilboar_Male_Quillboar_Story_Rexxar_Mission4Death_01.prefab:360502f78bbb40d4b66dffc00d88620f");

	private static readonly AssetReference VO_Story_02_Quilboar_Male_Quillboar_Story_Rexxar_Mission4EmoteResponse_01 = new AssetReference("VO_Story_02_Quilboar_Male_Quillboar_Story_Rexxar_Mission4EmoteResponse_01.prefab:21be207809e8454488283ee3034b15a0");

	private List<string> m_IdleLines = new List<string> { VO_LETL_822H_Male_Quilboar_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_LETL_822H_Male_Quilboar_Attack_01, VO_LETL_822H_Male_Quilboar_Attack_02, VO_LETL_822H_Male_Quilboar_Death_01, VO_LETL_822H_Male_Quilboar_Idle_01, VO_LETL_822H_Male_Quilboar_Intro_01, VO_LETL_822H_Male_Quilboar_Intro_02, VO_Story_02_Quilboar_Male_Quillboar_Story_Rexxar_Mission4Death_01, VO_Story_02_Quilboar_Male_Quillboar_Story_Rexxar_Mission4EmoteResponse_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_LETL_822H_Male_Quilboar_Intro_01;
		m_deathLine = VO_LETL_822H_Male_Quilboar_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_822H");
		FindEnemyActorInPlayByDesignCode("LETL_815H");
		Entity entityThatownsThatAbility = playedEntity.GetLettuceAbilityOwner();
		Actor actorThatCastedAbility = entityThatownsThatAbility.GetCard().GetActor();
		string designCode = entityThatownsThatAbility.GetCardId();
		if (designCode == "LETL_815H")
		{
			switch (cardID)
			{
			case "LETL_815P1_01":
			case "LETL_815P1_02":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVO(actorThatCastedAbility, VO_Story_02_Quilboar_Male_Quillboar_Story_Rexxar_Mission4EmoteResponse_01);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_815P2_01":
			case "LETL_815P2_02":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVO(actorThatCastedAbility, VO_Story_02_Quilboar_Male_Quillboar_Story_Rexxar_Mission4EmoteResponse_01);
				GameState.Get().SetBusy(busy: false);
				break;
			}
		}
		if (!(designCode == "LETL_822H"))
		{
			yield break;
		}
		if (!(cardID == "LETL_822P1_01"))
		{
			if (cardID == "LETL_822P2_01")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LETL_822H_Male_Quilboar_Attack_02);
				GameState.Get().SetBusy(busy: false);
			}
		}
		else
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_LETL_822H_Male_Quilboar_Attack_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_822H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_822H");
		Actor guestbossActor = FindEnemyActorInPlayByDesignCode("LETL_815H");
		string cardID = entity.GetCardId();
		if (!(cardID == "LETL_822H"))
		{
			if (cardID == "LETL_815H")
			{
				yield return MissionPlaySound(guestbossActor, VO_Story_02_Quilboar_Male_Quillboar_Story_Rexxar_Mission4Death_01);
			}
		}
		else
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_822H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
