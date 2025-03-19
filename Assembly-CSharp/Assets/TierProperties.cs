using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class TierProperties
{
	public enum Buildingtierproperty
	{
		[Description("Invalid")]
		INVALID = -1,
		[Description("TaskSlots")]
		TASKSLOTS = 1,
		[Description("TrainingSlots")]
		TRAININGSLOTS = 2,
		[Description("TrainingXpPerHour")]
		TRAININGXPPERHOUR = 3,
		[Description("TrainingXpPoolSize")]
		TRAININGXPPOOLSIZE = 4,
		[Description("PveMode")]
		PVEMODE = 5,
		[Description("DeckSlots")]
		DECKSLOTS = 6,
		[Description("TreasureImprovement")]
		TREASUREIMPROVEMENT = 7,
		[Description("UpgradeAbilityCap")]
		UPGRADEABILITYCAP = 8,
		[Description("TrainingMinSeconds")]
		TRAININGMINSECONDS = 9
	}

	public static Buildingtierproperty ParseBuildingtierpropertyValue(string value)
	{
		EnumUtils.TryGetEnum<Buildingtierproperty>(value, out var e);
		return e;
	}
}
