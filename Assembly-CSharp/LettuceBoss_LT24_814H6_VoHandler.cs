using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT24_814H6_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_Romulo_Male_Human_LETL_Attack_01 = new AssetReference("VO_Romulo_Male_Human_LETL_Attack_01.prefab:c2877026223c8c649b59bed0e1d81e88");

	private static readonly AssetReference VO_Romulo_Male_Human_LETL_Idle_01 = new AssetReference("VO_Romulo_Male_Human_LETL_Idle_01.prefab:102c9a075aa2d7e4a89fcccb441079cf");

	private static readonly AssetReference VO_Romulo_Male_Human_LETL_Idle_02 = new AssetReference("VO_Romulo_Male_Human_LETL_Idle_02.prefab:6540f245af9e44549a506e87b107e692");

	private static readonly AssetReference VO_Julianne_Female_Human_LETL_Attack_01 = new AssetReference("VO_Julianne_Female_Human_LETL_Attack_01.prefab:8ecc86de9b3d4d041af2f6e000fed9d0");

	private static readonly AssetReference VO_Julianne_Female_Human_LETL_Idle_01 = new AssetReference("VO_Julianne_Female_Human_LETL_Idle_01.prefab:1a7272aad4a93334599d43c829256551");

	private static readonly AssetReference VO_Julianne_Female_Human_LETL_Idle_02 = new AssetReference("VO_Julianne_Female_Human_LETL_Idle_02.prefab:15aba187fd4ba8a478b10a2ab9b994d8");

	private List<string> m_IdleLine = new List<string> { VO_Romulo_Male_Human_LETL_Idle_02 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_Romulo_Male_Human_LETL_Attack_01, VO_Romulo_Male_Human_LETL_Idle_01, VO_Romulo_Male_Human_LETL_Idle_02, VO_Julianne_Female_Human_LETL_Attack_01, VO_Julianne_Female_Human_LETL_Idle_01, VO_Julianne_Female_Human_LETL_Idle_02 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_814H6");
		Actor bossActor2 = FindEnemyActorInPlayByDesignCode("LT24_814H7");
		string designCode = playedEntity.GetLettuceAbilityOwner().GetCardId();
		if (designCode == "LT24_814H6")
		{
			if (cardID == "LT24_814P5")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_Romulo_Male_Human_LETL_Attack_01);
				GameState.Get().SetBusy(busy: false);
			}
		}
		else if (designCode == "LT24_814H7" && cardID == "LT24_814P6")
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor2, VO_Julianne_Female_Human_LETL_Attack_01);
			GameState.Get().SetBusy(busy: false);
		}
	}

	public override List<string> GetIdleLines()
	{
		return m_IdleLine;
	}

	public override IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_814H6");
		if (missionEvent == 517)
		{
			yield return MissionPlayVO(bossActor, m_IdleLine);
		}
		else
		{
			yield return base.HandleMissionEventWithTiming(missionEvent);
		}
	}
}
