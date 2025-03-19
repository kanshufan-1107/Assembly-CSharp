public class InvestigateCardTextBuilder : CardTextBuilder
{
	public override string BuildCardTextInHand(Entity entity)
	{
		GetPowersText(entity, out var power);
		return TextUtils.TransformCardText(entity, power);
	}

	public override string BuildCardName(Entity entity)
	{
		GetNamesText(entity, out var name1);
		return TextUtils.TryFormat("{0}", name1);
	}

	public override string BuildCardTextInHand(EntityDef entityDef)
	{
		return string.Empty;
	}

	public override string BuildCardTextInHistory(Entity entity)
	{
		GetPowersText(entity, out var power);
		CardTextHistoryData cardTextHistoryData = entity.GetCardTextHistoryData();
		if (cardTextHistoryData == null)
		{
			Log.All.Print("TreasureCardTextBuilder.BuildCardTextInHistory: entity {0} does not have a CardTextHistoryData object.", entity.GetEntityId());
			return "";
		}
		return TextUtils.TransformCardText(entity, cardTextHistoryData, power);
	}

	private void GetPowersText(Entity entity, out string power)
	{
		string power2 = CardTextBuilder.GetRawCardTextInHand(entity.GetCardId());
		power2 = power2.Replace("@", ConvertNumber(entity.GetCardId(), entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1)));
		power2 = power2.Replace("*", ConvertNumberTwo(entity.GetCardId(), entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1)));
		string power3 = string.Empty;
		if (entity.HasTag(GAME_TAG.MODULAR_ENTITY_PART_1))
		{
			int databaseId = entity.GetTag(GAME_TAG.MODULAR_ENTITY_PART_1);
			EntityDef entityDef = DefLoader.Get().GetEntityDef(databaseId);
			if (entityDef != null)
			{
				power3 = CardTextBuilder.GetRawCardTextInHand(entityDef.GetCardId());
				power3 = power3.Replace("@", ConvertNumber(entityDef.GetCardId(), entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1)));
				power3 = power3.Replace("*", ConvertNumberTwo(entityDef.GetCardId(), entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1)));
			}
		}
		string power4 = string.Empty;
		if (entity.HasTag(GAME_TAG.MODULAR_ENTITY_PART_2))
		{
			int databaseId2 = entity.GetTag(GAME_TAG.MODULAR_ENTITY_PART_2);
			EntityDef entityDef2 = DefLoader.Get().GetEntityDef(databaseId2);
			if (entityDef2 != null)
			{
				power4 = CardTextBuilder.GetRawCardTextInHand(entityDef2.GetCardId());
				power4 = power4.Replace("@", ConvertNumber(entityDef2.GetCardId(), entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1)));
				power4 = power4.Replace("*", ConvertNumberTwo(entityDef2.GetCardId(), entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1)));
			}
		}
		string sep1 = string.Empty;
		string sep2 = string.Empty;
		if (power2 != string.Empty && power3 != string.Empty)
		{
			sep1 = ", ";
		}
		if ((power2 != string.Empty || power3 != string.Empty) && power4 != string.Empty)
		{
			sep2 = "\n";
		}
		power = $"{power2}{sep1}{power3}{sep2}{power4}";
	}

	private void GetNamesText(Entity entity, out string name)
	{
		string name2 = string.Empty;
		EntityDef edef = entity.GetEntityDef();
		if (edef != null)
		{
			name2 = edef.GetName();
		}
		string name3 = string.Empty;
		if (entity.HasTag(GAME_TAG.MODULAR_ENTITY_PART_1))
		{
			int databaseId = entity.GetTag(GAME_TAG.MODULAR_ENTITY_PART_1);
			EntityDef entityDef = DefLoader.Get().GetEntityDef(databaseId);
			if (entityDef != null)
			{
				name3 = entityDef.GetName();
			}
		}
		string name4 = string.Empty;
		if (entity.HasTag(GAME_TAG.MODULAR_ENTITY_PART_2))
		{
			int databaseId2 = entity.GetTag(GAME_TAG.MODULAR_ENTITY_PART_2);
			EntityDef entityDef2 = DefLoader.Get().GetEntityDef(databaseId2);
			if (entityDef2 != null)
			{
				name4 = entityDef2.GetName();
			}
		}
		string sep1 = string.Empty;
		string sep2 = string.Empty;
		if (name2 != string.Empty && name3 != string.Empty)
		{
			sep1 = " ";
		}
		if ((name2 != string.Empty || name3 != string.Empty) && name4 != string.Empty)
		{
			sep2 = " ";
		}
		name = $"{name2}{sep1}{name3}{sep2}{name4}";
	}

	private string ConvertNumberTwo(string cardID, int size)
	{
		return "";
	}

	private string ConvertNumber(string cardID, int size)
	{
		int finalSize = size;
		switch (cardID)
		{
		case "GIL_097a":
			finalSize = size * 2;
			break;
		case "GIL_097b":
			finalSize = size * 3;
			break;
		case "GIL_097c":
			finalSize = size * 2;
			break;
		case "GIL_097d":
			finalSize = size;
			break;
		case "GIL_097e":
			finalSize = size;
			switch (finalSize)
			{
			case 1:
				return "one";
			case 2:
				return "two";
			case 3:
				return "three";
			}
			break;
		case "GIL_097f":
			finalSize = size;
			break;
		case "GIL_098b":
			finalSize = size;
			break;
		}
		return finalSize.ToString() ?? "";
	}
}
