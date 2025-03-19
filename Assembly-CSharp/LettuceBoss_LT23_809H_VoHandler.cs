using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT23_809H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_FishOfNZoth_Male_OldGod_LETL_Attack_01 = new AssetReference("VO_FishOfNZoth_Male_OldGod_LETL_Attack_01.prefab:6f4190c99b082f442b52d0b34ed29251");

	private static readonly AssetReference VO_FishOfNZoth_Male_OldGod_LETL_Attack_02 = new AssetReference("VO_FishOfNZoth_Male_OldGod_LETL_Attack_02.prefab:3abc3951a6d245c4893170a522a285ec");

	private static readonly AssetReference VO_FishOfNZoth_Male_OldGod_LETL_Death_01 = new AssetReference("VO_FishOfNZoth_Male_OldGod_LETL_Death_01.prefab:9036f83f19a239846b91e3792b875b87");

	private static readonly AssetReference VO_FishOfNZoth_Male_OldGod_LETL_Idle_01 = new AssetReference("VO_FishOfNZoth_Male_OldGod_LETL_Idle_01.prefab:05d4dbdab05501a47b441666edaa02b7");

	private static readonly AssetReference VO_FishOfNZoth_Male_OldGod_LETL_Intro_01 = new AssetReference("VO_FishOfNZoth_Male_OldGod_LETL_Intro_01.prefab:abf5302d5ab27294186e9197b35da9ee");

	private List<string> m_IdleLines = new List<string> { VO_FishOfNZoth_Male_OldGod_LETL_Idle_01, VO_FishOfNZoth_Male_OldGod_LETL_Attack_02 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_FishOfNZoth_Male_OldGod_LETL_Death_01, VO_FishOfNZoth_Male_OldGod_LETL_Idle_01, VO_FishOfNZoth_Male_OldGod_LETL_Intro_01, VO_FishOfNZoth_Male_OldGod_LETL_Attack_01, VO_FishOfNZoth_Male_OldGod_LETL_Attack_02 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_FishOfNZoth_Male_OldGod_LETL_Intro_01;
		m_deathLine = VO_FishOfNZoth_Male_OldGod_LETL_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_809H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT23_809H" && cardID == "LT23_809P1")
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_FishOfNZoth_Male_OldGod_LETL_Attack_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_809H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_809H");
		if (entity.GetCardId() == "LT23_809H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_809H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
