public class LettuceAbility
{
	public class AbilityTier
	{
		public int ID;

		public int m_tier;

		public string m_cardId;

		public string m_cardName;

		public int m_coinCost;

		public bool m_validTier;
	}

	public int ID;

	public string m_abilityName;

	public const int MAX_ABILITY_TIERS = 5;

	public const int MAX_EQUIPMENT_TIERS = 4;

	public CollectionUtils.MercenariesModeCardType m_cardType;

	public AbilityTier[] m_tierList;

	public int m_tier = 1;

	public int m_mythicModifier;

	public int m_unlockLevel = 1;

	public bool m_isAffectedBySlottedEquipment;

	public bool m_acquireAcknowledged;

	public bool m_upgradeAcknowledged;

	private bool m_owned;

	public bool Owned
	{
		get
		{
			return m_owned;
		}
		set
		{
			if (!m_owned && value && !Options.Get().GetBool(Option.HAS_UNLOCKED_FIRST_EQUIPMENT))
			{
				Options.Get().SetBool(Option.HAS_UNLOCKED_FIRST_EQUIPMENT, val: true);
			}
			m_owned = value;
		}
	}

	public LettuceAbility(CollectionUtils.MercenariesModeCardType cardType)
	{
		m_cardType = cardType;
		int tierSize = ((cardType == CollectionUtils.MercenariesModeCardType.Ability) ? 5 : 4);
		m_tierList = new AbilityTier[tierSize];
		for (int i = 0; i < tierSize; i++)
		{
			m_tierList[i] = new AbilityTier();
		}
	}

	public string GetCardId()
	{
		return m_tierList[m_tier - 1].m_cardId;
	}

	public bool ContainsCardId(string cardId)
	{
		AbilityTier[] tierList = m_tierList;
		foreach (AbilityTier tier in tierList)
		{
			if (tier.m_validTier && tier.m_cardId.Equals(cardId))
			{
				return true;
			}
		}
		return false;
	}

	public string GetCardName()
	{
		return m_tierList[m_tier - 1].m_cardName;
	}

	public int GetNextUpgradeCost()
	{
		for (int i = m_tier; i < m_tierList.Length; i++)
		{
			AbilityTier tier = m_tierList[i];
			if (tier.m_validTier)
			{
				return tier.m_coinCost;
			}
		}
		return 0;
	}

	public int GetBaseTier()
	{
		for (int i = 0; i < m_tierList.Length; i++)
		{
			if (m_tierList[i].m_validTier)
			{
				return i + 1;
			}
		}
		Log.Lettuce.PrintWarning("LettuceAbility.GetBaseTier - Ability ID {0} has no valid base tier!", ID);
		return -1;
	}

	public int GetMaxTier()
	{
		for (int i = m_tierList.Length - 1; i >= 0; i--)
		{
			if (m_tierList[i].m_validTier)
			{
				return i + 1;
			}
		}
		Log.Lettuce.PrintWarning("LettuceAbility.GetMaxTier - Ability ID {0} has no max tier!", ID);
		return -1;
	}

	public int GetNextTier()
	{
		for (int i = m_tier; i < m_tierList.Length; i++)
		{
			if (m_tierList[i].m_validTier)
			{
				return i + 1;
			}
		}
		return m_tier;
	}

	public AbilityTier GetAbilityTier(int tier)
	{
		if (m_tierList.Length >= tier)
		{
			return m_tierList[tier - 1];
		}
		return null;
	}

	public LettuceEquipmentTierDbfRecord GetCurrentEquipmentTierRecord()
	{
		LettuceEquipmentTierDbfRecord equipmentTierRecord = null;
		if (m_cardType == CollectionUtils.MercenariesModeCardType.Equipment)
		{
			AbilityTier abilityTier = GetAbilityTier(m_tier);
			if (abilityTier != null)
			{
				equipmentTierRecord = GameDbf.LettuceEquipmentTier.GetRecord(abilityTier.ID);
			}
		}
		return equipmentTierRecord;
	}

	public bool IsAcknowledged(LettuceMercenary mercenary)
	{
		bool upgradeUnacknowledged = !m_upgradeAcknowledged && mercenary.m_currencyAmount >= GetNextUpgradeCost();
		if (MercenariesDataUtil.IsAbilityorEquipmentAvailableToUse(this, mercenary))
		{
			if (!upgradeUnacknowledged)
			{
				return m_acquireAcknowledged;
			}
			return false;
		}
		return true;
	}
}
