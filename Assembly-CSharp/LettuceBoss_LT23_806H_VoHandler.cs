using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT23_806H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference WC_026_KreshLordofTurtlin_Attack = new AssetReference("WC_026_KreshLordofTurtlin_Attack.prefab:129b5c8bf04306a4c986afb085f30fda");

	private static readonly AssetReference WC_026_KreshLordofTurtlin_Death = new AssetReference("WC_026_KreshLordofTurtlin_Death.prefab:003900f61abaf0849b9960bc6c96caba");

	private static readonly AssetReference WC_026_KreshLordofTurtlin_Play = new AssetReference("WC_026_KreshLordofTurtlin_Play.prefab:f58cd70d59197454f9d2e09bfcc50bda");

	private List<string> m_IdleLines = new List<string> { WC_026_KreshLordofTurtlin_Play };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { WC_026_KreshLordofTurtlin_Play, WC_026_KreshLordofTurtlin_Attack, WC_026_KreshLordofTurtlin_Death };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = WC_026_KreshLordofTurtlin_Play;
		m_deathLine = WC_026_KreshLordofTurtlin_Death;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_806H");
		if (!(playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT23_806H"))
		{
			yield break;
		}
		if (!(cardID == "LT23_806P1"))
		{
			if (cardID == "LT23_806P2")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, WC_026_KreshLordofTurtlin_Attack);
				GameState.Get().SetBusy(busy: false);
			}
		}
		else
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, WC_026_KreshLordofTurtlin_Attack);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_806H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_806H");
		if (entity.GetCardId() == "LT23_806H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_806H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
