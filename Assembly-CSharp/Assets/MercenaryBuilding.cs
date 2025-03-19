using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class MercenaryBuilding
{
	public enum Mercenarybuildingtype
	{
		[Description("Invalid")]
		INVALID = -1,
		[Description("Village")]
		VILLAGE,
		[Description("TaskBoard")]
		TASKBOARD,
		[Description("BuildingManager")]
		BUILDINGMANAGER,
		[Description("TrainingHall")]
		TRAININGHALL,
		[Description("PveZones")]
		PVEZONES,
		[Description("Pvp")]
		PVP,
		[Description("Collection")]
		COLLECTION,
		[Description("Shop")]
		SHOP,
		[Description("Mailbox")]
		MAILBOX
	}

	public static Mercenarybuildingtype ParseMercenarybuildingtypeValue(string value)
	{
		EnumUtils.TryGetEnum<Mercenarybuildingtype>(value, out var e);
		return e;
	}
}
