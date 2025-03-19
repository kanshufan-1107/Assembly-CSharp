using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_826H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_LETL_826H_Male_NightElf_Attack_01 = new AssetReference("VO_LETL_826H_Male_NightElf_Attack_01.prefab:5fdbf6b5e00013d4e99fdfb21583736b");

	private static readonly AssetReference VO_LETL_826H_Male_NightElf_Attack_02 = new AssetReference("VO_LETL_826H_Male_NightElf_Attack_02.prefab:814e61f095bc4e44f8d87899ef2dffad");

	private static readonly AssetReference VO_LETL_826H_Male_NightElf_Death_01 = new AssetReference("VO_LETL_826H_Male_NightElf_Death_01.prefab:f6e029176ef306d49b48386a4336e932");

	private static readonly AssetReference VO_LETL_826H_Male_NightElf_Idle_01 = new AssetReference("VO_LETL_826H_Male_NightElf_Idle_01.prefab:37f6eb04fb6eede488f2a557c96ea225");

	private static readonly AssetReference VO_LETL_826H_Male_NightElf_Intro_01 = new AssetReference("VO_LETL_826H_Male_NightElf_Intro_01.prefab:2cad5dc95601ae64d9a405703431d717");

	private static readonly AssetReference VO_LETL_826H2_Male_Tauren_Attack_01 = new AssetReference("VO_LETL_826H2_Male_Tauren_Attack_01.prefab:74d4fb515ee47f649ba82032b4796c84");

	private static readonly AssetReference VO_LETL_826H2_Male_Tauren_Attack_02 = new AssetReference("VO_LETL_826H2_Male_Tauren_Attack_02.prefab:d90c73f52533c0d4a9e2148aba28a056");

	private static readonly AssetReference VO_LETL_826H2_Male_Tauren_Death_01 = new AssetReference("VO_LETL_826H2_Male_Tauren_Death_01.prefab:cd42d0bbdd9ed2c43824f7df0a2a103e");

	private static readonly AssetReference VO_LETL_826H2_Male_Tauren_Idle_01 = new AssetReference("VO_LETL_826H2_Male_Tauren_Idle_01.prefab:fb5bc3ed3a3faa64bbffb2b2c7051aff");

	private static readonly AssetReference VO_LETL_826H2_Male_Tauren_Intro_01 = new AssetReference("VO_LETL_826H2_Male_Tauren_Intro_01.prefab:e24a53542fb86584181dc2cf317c6421");

	private List<string> m_IdleLines = new List<string> { VO_LETL_826H_Male_NightElf_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_LETL_826H_Male_NightElf_Attack_01, VO_LETL_826H_Male_NightElf_Attack_02, VO_LETL_826H_Male_NightElf_Death_01, VO_LETL_826H_Male_NightElf_Idle_01, VO_LETL_826H_Male_NightElf_Intro_01, VO_LETL_826H2_Male_Tauren_Attack_01, VO_LETL_826H2_Male_Tauren_Attack_02, VO_LETL_826H2_Male_Tauren_Death_01, VO_LETL_826H2_Male_Tauren_Idle_01, VO_LETL_826H2_Male_Tauren_Intro_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_LETL_826H_Male_NightElf_Intro_01;
		m_deathLine = VO_LETL_826H_Male_NightElf_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_826H");
		Actor bossguestActor = FindEnemyActorInPlayByDesignCode("LETL_826H2");
		Entity entityThatownsThatAbility = playedEntity.GetLettuceAbilityOwner();
		string designCode = entityThatownsThatAbility.GetCardId();
		string text;
		if (designCode == "LETL_826H")
		{
			text = cardID;
			if (!(text == "LETL_029P6_02"))
			{
				if (text == "LETL_463_02")
				{
					GameState.Get().SetBusy(busy: true);
					yield return MissionPlayVOOnce(bossActor, VO_LETL_826H_Male_NightElf_Attack_02);
					GameState.Get().SetBusy(busy: false);
				}
			}
			else
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LETL_826H_Male_NightElf_Attack_01);
				GameState.Get().SetBusy(busy: false);
			}
		}
		if (!(designCode == "LETL_826H2"))
		{
			yield break;
		}
		text = cardID;
		if (!(text == "LETL_471_02"))
		{
			if (text == "LETL_472_03")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossguestActor, VO_LETL_826H2_Male_Tauren_Attack_02);
				GameState.Get().SetBusy(busy: false);
			}
		}
		else
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossguestActor, VO_LETL_826H2_Male_Tauren_Attack_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_826H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_826H");
		Actor bossguestActor = FindEnemyActorInPlayByDesignCode("LETL_826H2");
		string cardID = entity.GetCardId();
		if (!(cardID == "LETL_826H"))
		{
			if (cardID == "LETL_826H2")
			{
				yield return MissionPlaySound(bossguestActor, VO_LETL_826H2_Male_Tauren_Death_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_826H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
