using Hearthstone.DataModels;

public static class MercenariesDataUtil
{
	public enum MercenariesBountyLockedReason
	{
		INVALID = -1,
		UNLOCKED,
		COMING_SOON,
		EVENT_NOT_STARTED,
		EVENT_NOT_ACTIVE,
		EVENT_ENDED,
		PVE_BUILDING_NEEDS_UPGRADE,
		PREVIOUS_ZONES_INCOMPLETE,
		CURRENT_BOUNTY_UNFINISHED,
		EVENT_NOT_COMPLETE
	}

	public static MercenariesBountyLockedReason GetBountyUnlockStatus(int bountyRecordID)
	{
		return GetBountyUnlockStatus(GameDbf.LettuceBounty.GetRecord(bountyRecordID));
	}

	public static MercenariesBountyLockedReason GetBountyUnlockStatus(LettuceBountyDbfRecord bountyRecord)
	{
		if (bountyRecord == null || !bountyRecord.Enabled)
		{
			return MercenariesBountyLockedReason.INVALID;
		}
		if (bountyRecord.RequiredCompletedBounty > 0 && !IsBountyComplete(bountyRecord.RequiredCompletedBounty))
		{
			return MercenariesBountyLockedReason.PREVIOUS_ZONES_INCOMPLETE;
		}
		EventTimingType duringEvent = bountyRecord.Event;
		bool num = EventTimingManager.Get().IsEventActive(duringEvent);
		EventTimingType afterEvent = bountyRecord.AvailableAfterEvent;
		bool afterEventHasEnded = EventTimingManager.Get().HasEventEnded(afterEvent);
		if (!num && !afterEventHasEnded)
		{
			if (duringEvent != EventTimingType.SPECIAL_EVENT_NEVER && duringEvent != EventTimingType.UNKNOWN)
			{
				return MercenariesBountyLockedReason.EVENT_NOT_ACTIVE;
			}
			return MercenariesBountyLockedReason.EVENT_NOT_COMPLETE;
		}
		return MercenariesBountyLockedReason.UNLOCKED;
	}

	public static MercenariesBountyLockedReason GetBountySetUnlockStatus(LettuceBountySetDbfRecord bountySetRecord)
	{
		if (bountySetRecord == null)
		{
			return MercenariesBountyLockedReason.INVALID;
		}
		if (bountySetRecord.RequiredCompletedBounty > 0 && !IsBountyComplete(bountySetRecord.RequiredCompletedBounty))
		{
			return MercenariesBountyLockedReason.PREVIOUS_ZONES_INCOMPLETE;
		}
		EventTimingType duringEvent = bountySetRecord.Event;
		bool num = EventTimingManager.Get().IsEventActive(duringEvent);
		EventTimingType afterEvent = bountySetRecord.AvailableAfterEvent;
		bool afterEventHasEnded = EventTimingManager.Get().HasEventEnded(afterEvent);
		if (!num && !afterEventHasEnded)
		{
			if (duringEvent != EventTimingType.SPECIAL_EVENT_NEVER && duringEvent != EventTimingType.UNKNOWN)
			{
				return MercenariesBountyLockedReason.EVENT_NOT_ACTIVE;
			}
			return MercenariesBountyLockedReason.EVENT_NOT_COMPLETE;
		}
		return MercenariesBountyLockedReason.UNLOCKED;
	}

	public static bool IsBountyComplete(int bountyId, NetCache.NetCacheMercenariesPlayerInfo mercenariesPlayerInfo = null)
	{
		if (mercenariesPlayerInfo == null)
		{
			mercenariesPlayerInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesPlayerInfo>();
		}
		if (mercenariesPlayerInfo == null || mercenariesPlayerInfo.BountyInfoMap == null)
		{
			return false;
		}
		if (!mercenariesPlayerInfo.BountyInfoMap.ContainsKey(bountyId) || mercenariesPlayerInfo.BountyInfoMap[bountyId] == null)
		{
			return false;
		}
		if (!mercenariesPlayerInfo.BountyInfoMap[bountyId].IsComplete)
		{
			return mercenariesPlayerInfo.BountyInfoMap[bountyId].Completions > 0;
		}
		return true;
	}

	public static bool IsAbilityorEquipmentAvailableToUse(LettuceAbility ability, LettuceMercenary merc)
	{
		if (ability.m_cardType != CollectionUtils.MercenariesModeCardType.Equipment)
		{
			return !merc.IsAbilityLocked(ability);
		}
		return ability.Owned;
	}

	public static void UpdateMercenaryDataModelWithNewData(LettuceMercenaryDataModel mercData, LettuceAbility ability, LettuceMercenary merc)
	{
		if (mercData == null || ability == null || merc == null)
		{
			Log.Lettuce.PrintWarning("MercenaryDetailDisplay.UpdateMercenaryDataModelWithNewData - Invalid null parameter");
			return;
		}
		if (mercData.MercenaryId != merc.ID)
		{
			Log.Lettuce.PrintWarning("MercenaryDetailDisplay.UpdateDataModelsAfterTransaction - " + $"Mercenary display data model merc Id {mercData.MercenaryId} does not match response merc ID {merc.ID}");
			return;
		}
		if (ability.m_cardType == CollectionUtils.MercenariesModeCardType.Equipment && !ability.Owned)
		{
			ability.Owned = true;
			ability.m_tier = ability.GetBaseTier();
		}
		else
		{
			ability.m_tier = ability.GetNextTier();
		}
		foreach (LettuceAbilityDataModel abilityData in (ability.m_cardType == CollectionUtils.MercenariesModeCardType.Ability) ? mercData.AbilityList : mercData.EquipmentList)
		{
			if (abilityData.AbilityId == ability.ID)
			{
				abilityData.CurrentTier = ability.m_tier;
				abilityData.Owned = ability.Owned;
				abilityData.IsNew = !ability.IsAcknowledged(merc);
			}
		}
		CollectionUtils.UpdateReadyForUpgradeStatus(mercData, merc);
		CollectionUtils.SetMercenaryStatsByLevel(mercData, merc.ID, merc.m_level, merc.m_isFullyUpgraded);
		mercData.ChildUpgradeAvailable = false;
		LettuceAbility slottedEquip = merc.GetSlottedEquipment();
		if (slottedEquip != null && slottedEquip.ID == ability.ID)
		{
			CollectionUtils.UpdateAbilityAffectedBySlottedEquipment(mercData, merc);
		}
	}

	public static void UpdateMercenaryDataModelNewStatus(LettuceMercenaryDataModel mercData)
	{
		CollectionManager cm = CollectionManager.Get();
		if (cm == null)
		{
			return;
		}
		LettuceMercenary merc = cm.GetMercenary(mercData.MercenaryId);
		if (merc == null)
		{
			return;
		}
		bool newStatus = cm.DoesMercenaryNeedToBeAcknowledged(merc);
		CollectibleDisplay cd = cm.GetCollectibleDisplay();
		if (cd != null)
		{
			LettuceCollectionPageManager lcpm = cd.GetPageManager() as LettuceCollectionPageManager;
			if (lcpm != null)
			{
				lcpm.UpdateAcknowledgedStatusForPageMercenary(merc.ID, newStatus);
			}
		}
	}
}
