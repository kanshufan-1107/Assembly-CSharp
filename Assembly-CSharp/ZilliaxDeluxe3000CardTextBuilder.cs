using System;
using System.Collections.Generic;
using Hearthstone;
using UnityEngine;

public class ZilliaxDeluxe3000CardTextBuilder : ModularEntityCardTextBuilder
{
	private readonly HashSet<string> g_zilliaxDeluxe3000CosmeticVariants = new HashSet<string> { "TOY_330", "TOY_330t5", "TOY_330t6", "TOY_330t7", "TOY_330t8", "TOY_330t9", "TOY_330t10", "TOY_330t11", "TOY_330t12" };

	private readonly Dictionary<int, int> m_zilliaxDeluxe3000FunctionalModules = new Dictionary<int, int>
	{
		{ 104944, 8 },
		{ 104945, 4 },
		{ 104946, 5 },
		{ 104947, 3 },
		{ 104948, 1 },
		{ 104949, 7 },
		{ 104950, 6 },
		{ 104951, 2 }
	};

	public override string BuildCardTextInHistory(Entity entity)
	{
		string formattedText = BuildFormattedText(entity);
		formattedText = TextUtils.TransformCardText(entity, formattedText);
		return formattedText.Trim(Environment.NewLine.ToCharArray());
	}

	public override string BuildCardTextInHand(EntityDef entityDef)
	{
		return TextUtils.TransformCardText(BuildFormattedText(entityDef)).Trim(Environment.NewLine.ToCharArray());
	}

	protected override string BuildFormattedText(Entity entity)
	{
		if (HearthstoneApplication.IsInternal() && !IsZilliaxDeluxe3000(entity))
		{
			return "This card does not appear to be Zilliax Deluxe 3000 (TOY_330)";
		}
		int firstModuleDBID = entity.GetTag(GAME_TAG.MODULAR_ENTITY_PART_1);
		int secondModuleDBID = entity.GetTag(GAME_TAG.MODULAR_ENTITY_PART_2);
		return CountValidModuleDBIDs(firstModuleDBID, secondModuleDBID) switch
		{
			2 => GetTextOfTwoValidModules(firstModuleDBID, secondModuleDBID), 
			1 => GetTextOfOneValidModule(firstModuleDBID, secondModuleDBID), 
			_ => CardTextBuilder.GetDefaultCardTextInHand(entity.GetEntityDef()), 
		};
	}

	protected string BuildFormattedText(EntityDef entityDef)
	{
		if (HearthstoneApplication.IsInternal() && !IsZilliaxDeluxe3000(entityDef))
		{
			return "This card does not appear to be Zilliax Deluxe 3000 (TOY_330)";
		}
		int firstModuleDBID = entityDef.GetTag(GAME_TAG.MODULAR_ENTITY_PART_1);
		int secondModuleDBID = entityDef.GetTag(GAME_TAG.MODULAR_ENTITY_PART_2);
		return CountValidModuleDBIDs(firstModuleDBID, secondModuleDBID) switch
		{
			2 => GetTextOfTwoValidModules(firstModuleDBID, secondModuleDBID), 
			1 => GetTextOfOneValidModule(firstModuleDBID, secondModuleDBID), 
			_ => CardTextBuilder.GetDefaultCardTextInHand(entityDef), 
		};
	}

	private string GetTextOfTwoValidModules(int firstModuleDBID, int secondModuleDBID)
	{
		if (HearthstoneApplication.IsInternal() && !m_zilliaxDeluxe3000FunctionalModules.ContainsKey(firstModuleDBID) && !m_zilliaxDeluxe3000FunctionalModules.ContainsKey(secondModuleDBID))
		{
			return "ERROR: This appears to be an incomplete Zilliax Deluxe 3000 (TOY_330).";
		}
		int a = m_zilliaxDeluxe3000FunctionalModules[firstModuleDBID];
		int secondKeyOrder = m_zilliaxDeluxe3000FunctionalModules[secondModuleDBID];
		int firstKeySorted = Mathf.Min(a, secondKeyOrder);
		int secondKeySorted = Mathf.Max(a, secondKeyOrder);
		return TextUtils.TransformCardText(GameStrings.Get($"ZILLIAX_DELUXE_COMBINED_MODULE_{firstKeySorted}_{secondKeySorted}"));
	}

	private string GetTextOfOneValidModule(int firstModuleDBID, int secondModuleDBID)
	{
		if (m_zilliaxDeluxe3000FunctionalModules.ContainsKey(firstModuleDBID))
		{
			return LookupTextOfZilliaxModule(firstModuleDBID);
		}
		if (m_zilliaxDeluxe3000FunctionalModules.ContainsKey(secondModuleDBID))
		{
			return LookupTextOfZilliaxModule(secondModuleDBID);
		}
		return "ERROR: This appears to be an incomplete Zilliax Deluxe 3000 (TOY_330).";
	}

	private string LookupTextOfZilliaxModule(int moduleDbid)
	{
		return CardTextBuilder.GetDefaultCardTextInHand(DefLoader.Get().GetEntityDef(moduleDbid));
	}

	private int CountValidModuleDBIDs(int firstModuleDBID, int secondModuleDBID)
	{
		int numValidModules = 0;
		if (m_zilliaxDeluxe3000FunctionalModules.ContainsKey(firstModuleDBID))
		{
			numValidModules++;
		}
		if (m_zilliaxDeluxe3000FunctionalModules.ContainsKey(secondModuleDBID))
		{
			numValidModules++;
		}
		return numValidModules;
	}

	private bool IsZilliaxDeluxe3000(Entity ent)
	{
		if (ent == null)
		{
			return false;
		}
		string cardId = ent.GetCardId();
		return g_zilliaxDeluxe3000CosmeticVariants.Contains(cardId);
	}

	private bool IsZilliaxDeluxe3000(EntityDef entDef)
	{
		if (entDef == null)
		{
			return false;
		}
		string cardId = entDef.GetCardId();
		return g_zilliaxDeluxe3000CosmeticVariants.Contains(cardId);
	}
}
