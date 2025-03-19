using System.Runtime.CompilerServices;

public class BonusChallengeUnlockData : RewardData
{
	[CompilerGenerated]
	private string _003CBossCardActorPrefab_003Ek__BackingField;

	public string PrefabToDisplay { get; set; }

	public string BossCardActorPrefab
	{
		[CompilerGenerated]
		set
		{
			_003CBossCardActorPrefab_003Ek__BackingField = value;
		}
	}

	public BonusChallengeUnlockData()
		: this("", "Card_DungeonCrawl_Boss.prefab:c7f700bb034424e46a7c2321e4621965")
	{
	}

	public BonusChallengeUnlockData(string prefabToDisplay, string bossCardActorPrefab)
		: base(Reward.Type.BONUS_CHALLENGE)
	{
		PrefabToDisplay = prefabToDisplay;
		BossCardActorPrefab = bossCardActorPrefab;
	}

	public override string ToString()
	{
		return $"[BonusChallengeUnlockData: PrefabToDisplay={PrefabToDisplay} Origin={base.Origin} OriginData={base.OriginData}]";
	}

	protected override string GetAssetPath()
	{
		return "BonusChallengeUnlocked.prefab:43116842520caa144a9e8b7c9bc418b9";
	}
}
