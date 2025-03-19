public class MountRewardData : RewardData
{
	public enum MountType
	{
		UNKNOWN,
		WOW_HEARTHSTEED,
		HEROES_MAGIC_CARPET_CARD,
		WOW_SARGE_TALE,
		TEN_YEAR_MOUNT
	}

	public MountType Mount { get; set; }

	public MountRewardData()
		: this(MountType.UNKNOWN)
	{
	}

	public MountRewardData(MountType mount)
		: base(Reward.Type.MOUNT)
	{
		Mount = mount;
	}

	public override string ToString()
	{
		return $"[MountRewardData Mount={Mount} Origin={base.Origin} OriginData={base.OriginData}]";
	}

	protected override string GetAssetPath()
	{
		return Mount switch
		{
			MountType.WOW_HEARTHSTEED => "HearthSteedReward.prefab:fca8aa4ddb6e1304f8382f4091b250a0", 
			MountType.HEROES_MAGIC_CARPET_CARD => "CardMountReward.prefab:9da51e41fcae3ae46b95b4859ea85205", 
			MountType.WOW_SARGE_TALE => "SargeTaleReward.prefab:c1d090b2bc1673848b62bab4cb4e6268", 
			MountType.TEN_YEAR_MOUNT => "FieryHeartReward.prefab:390afbab15c232c45a9a2470af905d43", 
			_ => string.Empty, 
		};
	}
}
