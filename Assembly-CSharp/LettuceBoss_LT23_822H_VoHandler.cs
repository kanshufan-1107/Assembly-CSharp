using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT23_822H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_SilasDarkmoon_Male_Gnome_LETL_Attack_03 = new AssetReference("VO_SilasDarkmoon_Male_Gnome_LETL_Attack_03.prefab:7d2fa57d323c0b74c8447f78d9bc692b");

	private static readonly AssetReference VO_SilasDarkmoon_Male_Gnome_LETL_Death_02 = new AssetReference("VO_SilasDarkmoon_Male_Gnome_LETL_Death_02.prefab:f67706d79227de44090b5ec021f7ac6a");

	private static readonly AssetReference VO_SilasDarkmoon_Male_Gnome_LETL_Idle_01 = new AssetReference("VO_SilasDarkmoon_Male_Gnome_LETL_Idle_01.prefab:f2415e39042b2fd4aae7aa45e0d29107");

	private static readonly AssetReference VO_SilasDarkmoon_Male_Gnome_LETL_Intro_01 = new AssetReference("VO_SilasDarkmoon_Male_Gnome_LETL_Intro_01.prefab:86b9436434ae841418442a2470d58999");

	private List<string> m_IdleLines = new List<string> { VO_SilasDarkmoon_Male_Gnome_LETL_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_SilasDarkmoon_Male_Gnome_LETL_Intro_01, VO_SilasDarkmoon_Male_Gnome_LETL_Attack_03, VO_SilasDarkmoon_Male_Gnome_LETL_Idle_01, VO_SilasDarkmoon_Male_Gnome_LETL_Death_02 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_SilasDarkmoon_Male_Gnome_LETL_Intro_01;
		m_deathLine = VO_SilasDarkmoon_Male_Gnome_LETL_Death_02;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_822H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT23_822H" && cardID == "LT23_822P1")
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_SilasDarkmoon_Male_Gnome_LETL_Attack_03);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_822H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_822H");
		if (entity.GetCardId() == "LT23_822H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_822H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
