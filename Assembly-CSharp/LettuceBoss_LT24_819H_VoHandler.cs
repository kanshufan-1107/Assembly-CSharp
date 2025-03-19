using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT24_819H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_Netherspite_Male_Dragon_LETL_C03_T09_Dialogue_01 = new AssetReference("VO_Netherspite_Male_Dragon_LETL_C03_T09_Dialogue_01.prefab:9a08d22f3b02d954a8c2be097c18c992");

	private static readonly AssetReference VO_Netherspite_Male_Dragon_LETL_Attack = new AssetReference("VO_Netherspite_Male_Dragon_LETL_Attack.prefab:0b82f9d579e8149469c6bf9b14b0150d");

	private static readonly AssetReference VO_Netherspite_Male_Dragon_LETL_Death = new AssetReference("VO_Netherspite_Male_Dragon_LETL_Death.prefab:1112a8ce589bc2143a49611d3fca61c7");

	private static readonly AssetReference VO_Netherspite_Male_Dragon_LETL_Idle = new AssetReference("VO_Netherspite_Male_Dragon_LETL_Idle.prefab:c8fe009cca27be64ca964e051cbd6bc0");

	private List<string> m_IdleLines = new List<string> { VO_Netherspite_Male_Dragon_LETL_Idle };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_Netherspite_Male_Dragon_LETL_C03_T09_Dialogue_01, VO_Netherspite_Male_Dragon_LETL_Attack, VO_Netherspite_Male_Dragon_LETL_Idle, VO_Netherspite_Male_Dragon_LETL_Death };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_Netherspite_Male_Dragon_LETL_C03_T09_Dialogue_01;
		m_deathLine = VO_Netherspite_Male_Dragon_LETL_Death;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_819H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT24_819H" && cardID == "LT24_819P2")
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_Netherspite_Male_Dragon_LETL_Attack);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_819H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_819H");
		if (entity.GetCardId() == "LT24_819H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_819H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
