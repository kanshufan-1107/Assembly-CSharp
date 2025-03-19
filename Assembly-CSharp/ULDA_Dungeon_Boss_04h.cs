using System.Collections.Generic;

public class ULDA_Dungeon_Boss_04h : ULDA_Dungeon
{
	private static readonly AssetReference VO_ULDA_BOSS_04h_NashTheGreatworm_Death_01 = new AssetReference("VO_ULDA_BOSS_04h_NashTheGreatworm_Death_01.prefab:1bed1cbf5c351ee44bc6fda69cfc9880");

	private static readonly AssetReference VO_ULDA_BOSS_04h_NashTheGreatworm_Defeat_01 = new AssetReference("VO_ULDA_BOSS_04h_NashTheGreatworm_Defeat_01.prefab:4c0cc44b1d322654faf9f293fa7362cb");

	private static readonly AssetReference VO_ULDA_BOSS_04h_NashTheGreatworm_EmoteResponse_01 = new AssetReference("VO_ULDA_BOSS_04h_NashTheGreatworm_EmoteResponse_01.prefab:8a2a0a92b70af624580c315c763a8a0d");

	private static readonly AssetReference VO_ULDA_BOSS_04h_NashTheGreatworm_Intro_01 = new AssetReference("VO_ULDA_BOSS_04h_NashTheGreatworm_Intro_01.prefab:a44e660daa30d704395e929bf58f472f");

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_ULDA_BOSS_04h_NashTheGreatworm_Death_01, VO_ULDA_BOSS_04h_NashTheGreatworm_Defeat_01, VO_ULDA_BOSS_04h_NashTheGreatworm_EmoteResponse_01, VO_ULDA_BOSS_04h_NashTheGreatworm_Intro_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			PreloadSound(soundFile);
		}
	}

	protected override bool GetShouldSuppressDeathTextBubble()
	{
		return true;
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_ULDA_BOSS_04h_NashTheGreatworm_Intro_01;
		m_deathLine = VO_ULDA_BOSS_04h_NashTheGreatworm_Death_01;
		m_standardEmoteResponseLine = VO_ULDA_BOSS_04h_NashTheGreatworm_EmoteResponse_01;
	}
}
