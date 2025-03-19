using System;
using System.Collections.Generic;
using Blizzard.T5.Core;
using PegasusLettuce;
using PegasusShared;
using UnityEngine;

public class LettuceTeam
{
	public static int DefaultMaxTeamNameCharacters = 24;

	public const long INVALID_TEAM_ID = 0L;

	private string m_name;

	private List<LettuceMercenary> m_lettuceMercs = new List<LettuceMercenary>();

	private Map<LettuceMercenary, LettuceMercenary.Loadout> m_loadouts = new Map<LettuceMercenary, LettuceMercenary.Loadout>();

	private bool m_netContentsLoaded;

	private bool m_isSavingContentChanges;

	private bool m_isSavingNameChanges;

	private bool m_isBeingDeleted;

	private uint m_sortOrder;

	private bool m_sortOrderDirty;

	private bool m_dirty;

	private bool m_nameDirty;

	public long ID;

	public DeckType Type = DeckType.NORMAL_DECK;

	public bool NeedsName;

	public ulong CreateDate;

	public bool Locked;

	public PegasusLettuce.LettuceTeam.Type TeamType;

	public string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			if (value == null)
			{
				Debug.LogError($"LettuceTeam.SetName() - null name given for team {this}");
			}
			else if (!value.Equals(m_name, StringComparison.InvariantCultureIgnoreCase))
			{
				m_dirty = true;
				m_nameDirty = true;
				m_name = value;
			}
		}
	}

	public uint SortOrder
	{
		get
		{
			return m_sortOrder;
		}
		set
		{
			if (m_sortOrder != value)
			{
				m_sortOrder = value;
				m_sortOrderDirty = true;
			}
		}
	}

	public LettuceTeam()
	{
	}

	public LettuceTeam(uint sortOrder)
	{
		m_sortOrder = sortOrder;
	}

	public override string ToString()
	{
		return $"Team [id={ID} name=\"{Name}\" heroCount={GetMercCount()} needsName={NeedsName} sortOrder={SortOrder}]";
	}

	public LettuceMercenary GetLeader()
	{
		if (m_lettuceMercs.Count > 0 && m_lettuceMercs[0] != null)
		{
			return m_lettuceMercs[0];
		}
		return null;
	}

	public void MarkNetworkContentsLoaded()
	{
		m_netContentsLoaded = true;
	}

	public bool NetworkContentsLoaded()
	{
		return m_netContentsLoaded;
	}

	public void MarkBeingDeleted()
	{
		m_isBeingDeleted = true;
	}

	public bool IsBeingDeleted()
	{
		return m_isBeingDeleted;
	}

	public bool IsSavingChanges()
	{
		if (!m_isSavingNameChanges)
		{
			return m_isSavingContentChanges;
		}
		return true;
	}

	public bool IsBeingEdited()
	{
		return this == CollectionManager.Get().GetEditingTeam();
	}

	public List<LettuceMercenary> GetMercs()
	{
		return m_lettuceMercs;
	}

	public int GetMercCount()
	{
		return m_lettuceMercs.Count;
	}

	public int GetTeamMythicLevel()
	{
		if (m_lettuceMercs.Count == 0)
		{
			return 0;
		}
		float totalMythicLevel = 0f;
		int numOfMercs = 0;
		int maxLevel = 0;
		foreach (LettuceMercenary merc in m_lettuceMercs)
		{
			maxLevel = Mathf.Max(merc.m_level, maxLevel);
			if (merc.m_isFullyUpgraded)
			{
				numOfMercs++;
				totalMythicLevel += (float)(merc.m_level + merc.GetMythicModifier());
			}
		}
		if (numOfMercs <= 0)
		{
			return maxLevel;
		}
		return Mathf.RoundToInt(totalMythicLevel / (float)numOfMercs);
	}

	public LettuceMercenary.Loadout GetLoadout(LettuceMercenary merc)
	{
		m_loadouts.TryGetValue(merc, out var loadout);
		return loadout;
	}

	public void ClearContents()
	{
		m_lettuceMercs.Clear();
		m_loadouts.Clear();
	}

	public bool AddMerc(string cardId, int index = -1, LettuceMercenary.Loadout loadout = null)
	{
		if (string.IsNullOrEmpty(cardId))
		{
			return false;
		}
		LettuceMercenary merc = CollectionManager.Get().GetMercenary(cardId);
		if (merc == null)
		{
			Log.Lettuce.PrintError("No mercenary with cardId = {0} in collection!", cardId);
			return false;
		}
		return AddMerc(merc, index, loadout);
	}

	public bool AddMerc(LettuceMercenary merc, int index = -1, LettuceMercenary.Loadout loadout = null)
	{
		if (merc == null)
		{
			Log.Lettuce.PrintError("LettuceTeam.AddMerc - null mercenary passed!");
			return false;
		}
		if (m_lettuceMercs.Find((LettuceMercenary m) => m.ID == merc.ID) != null)
		{
			return false;
		}
		EntityDef entityDef = DefLoader.Get().GetEntityDef(merc.GetCardId());
		if (entityDef != null)
		{
			merc.m_mercName = entityDef.GetName();
		}
		if (index >= 0 && index < m_lettuceMercs.Count)
		{
			m_lettuceMercs.Insert(index, merc);
		}
		else
		{
			m_lettuceMercs.Add(merc);
		}
		m_loadouts.Add(merc, (loadout != null) ? loadout : new LettuceMercenary.Loadout(merc.GetBaseLoadout()));
		m_dirty = true;
		return true;
	}

	public bool RemoveMerc(int mercId)
	{
		LettuceMercenary mercToRemove = CollectionManager.Get().GetMercenary(mercId);
		bool result = m_lettuceMercs.Remove(mercToRemove);
		m_loadouts.Remove(mercToRemove);
		m_dirty = true;
		return result;
	}

	public bool IsMercInTeam(string cardID, bool owned = true)
	{
		foreach (LettuceMercenary merc in m_lettuceMercs)
		{
			if (merc.GetCardId().Equals(cardID) && merc.m_owned == owned)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsMercInTeam(int mercId, bool owned = true)
	{
		foreach (LettuceMercenary merc in m_lettuceMercs)
		{
			if (merc.ID == mercId && merc.m_owned == owned)
			{
				return true;
			}
		}
		return false;
	}

	public bool TryGetMerc(int mercId, out LettuceMercenary result, bool owned = true)
	{
		result = null;
		foreach (LettuceMercenary merc in m_lettuceMercs)
		{
			if (merc.ID == mercId && merc.m_owned == owned)
			{
				result = merc;
				return true;
			}
		}
		return false;
	}

	public bool IsValid()
	{
		return m_lettuceMercs.Count > 0;
	}

	public bool IsDirty()
	{
		if (!m_dirty)
		{
			foreach (LettuceMercenary.Loadout value in m_loadouts.Values)
			{
				if (value.IsDirty())
				{
					return true;
				}
			}
		}
		return m_dirty;
	}

	public void ClearDirty()
	{
		m_dirty = false;
		m_nameDirty = false;
		foreach (LettuceMercenary.Loadout value in m_loadouts.Values)
		{
			value.ClearDirty();
		}
	}

	public bool DoesContainDisabledMerc()
	{
		NetCache.NetCacheMercenariesPlayerInfo playerInfo = NetCache.Get()?.GetNetObject<NetCache.NetCacheMercenariesPlayerInfo>();
		if (playerInfo == null)
		{
			Log.Lettuce.PrintError("DoesContainDisabledMerc - Can't access NetCacheMercenariesPlayerInfo");
			return false;
		}
		List<int> disabledMercList = playerInfo.DisabledMercenaryList;
		if (disabledMercList.Count != 0)
		{
			foreach (LettuceMercenary mercenary in m_lettuceMercs)
			{
				if (disabledMercList.Contains(mercenary.ID))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool SendChanges()
	{
		bool result = false;
		if (IsDirty())
		{
			Network.Get().UpdateMercenariesTeamRequest(this);
			ClearDirty();
			result = true;
		}
		return result;
	}

	public void SendTeamOrderChanges()
	{
		if (m_sortOrderDirty)
		{
			Network.Get().MercenariesTeamReorderRequest(this);
			m_sortOrderDirty = false;
		}
	}

	public void SendTeamRenameChange()
	{
		if (m_nameDirty)
		{
			Network.Get().UpdateMercenariesTeamNameRequest(this);
			m_nameDirty = false;
		}
	}

	public static LettuceTeam Convert(PegasusLettuce.LettuceTeam src, bool initializeWithBase = true, bool checkOwnership = true)
	{
		LettuceTeam result = null;
		if (src == null)
		{
			return result;
		}
		result = new LettuceTeam();
		result.ID = src.TeamId;
		result.Name = src.Name;
		result.SortOrder = src.SortOrder;
		result.TeamType = src.Type_;
		if (!Enum.IsDefined(typeof(PegasusLettuce.LettuceTeam.Type), result.TeamType))
		{
			result.TeamType = PegasusLettuce.LettuceTeam.Type.TYPE_INVALID;
		}
		if (!PopulateTeamMercenaries(src, result, initializeWithBase, checkOwnership))
		{
			result = null;
		}
		result.ClearDirty();
		return result;
	}

	public static bool PopulateTeamMercenaries(PegasusLettuce.LettuceTeam src, LettuceTeam dest, bool initializeWithBase = true, bool checkOwnership = true)
	{
		if (src == null)
		{
			Log.Lettuce.PrintError("PopulateTeamMercenaries - Src team was null");
			return false;
		}
		if (src.HasMercenaryList && src.MercenaryList.Mercenaries != null)
		{
			foreach (LettuceTeamMercenary srcMerc in src.MercenaryList.Mercenaries)
			{
				if (srcMerc == null)
				{
					Log.Lettuce.PrintError($"PopulateTeamMercenaries - null mercenary found for Team {src.TeamId}");
					continue;
				}
				LettuceMercenary mercenary = CollectionManager.Get().GetMercenary(srcMerc.MercenaryId);
				if (mercenary == null)
				{
					Log.Lettuce.PrintError($"PopulateTeamMercenaries - Mercenary{srcMerc.MercenaryId} not found for Team {src.TeamId}");
					continue;
				}
				if (dest.m_loadouts.ContainsKey(mercenary))
				{
					Log.Lettuce.PrintError($"PopulateTeamMercenaries - Duplicate mercenary{srcMerc.MercenaryId} found in Team {src.TeamId}");
					continue;
				}
				dest.m_lettuceMercs.Add(mercenary);
				LettuceMercenary.Loadout baseLoadout = mercenary.GetBaseLoadout();
				LettuceMercenary.Loadout teamLoadout = (initializeWithBase ? new LettuceMercenary.Loadout(baseLoadout) : new LettuceMercenary.Loadout());
				MercenaryArtVariationPremiumDbfRecord portraitRecord = GameDbf.MercenaryArtVariationPremium.GetRecord(srcMerc.SelectedPortraitId);
				int artVariationId = 0;
				TAG_PREMIUM portraitPremium = TAG_PREMIUM.NORMAL;
				if (portraitRecord != null)
				{
					artVariationId = portraitRecord.MercenaryArtVariationId;
					portraitPremium = (TAG_PREMIUM)portraitRecord.Premium;
				}
				if (checkOwnership)
				{
					LettuceMercenary.ArtVariation artVariation = mercenary.GetOwnedArtVariation(artVariationId, portraitPremium);
					teamLoadout.SetArtVariation(artVariation.m_record, artVariation.m_premium);
				}
				else
				{
					teamLoadout.SetArtVariation(GameDbf.MercenaryArtVariation.GetRecord(artVariationId), portraitPremium);
				}
				if (srcMerc.HasSelectedEquipmentId)
				{
					if (CollectionManager.Get().IsLettuceLoaded() && checkOwnership && !mercenary.CanSlotEquipment(srcMerc.SelectedEquipmentId))
					{
						Log.Lettuce.PrintError($"PopulateTeamMercenaries - Could not slot mercenary{srcMerc.MercenaryId} equipment {srcMerc.SelectedEquipmentId}");
					}
					else
					{
						teamLoadout.SetSlottedEquipment(GameDbf.LettuceEquipment.GetRecord(srcMerc.SelectedEquipmentId));
					}
				}
				dest.m_loadouts.Add(mercenary, teamLoadout);
			}
		}
		return true;
	}

	public static PegasusLettuce.LettuceTeam Convert(LettuceTeam src, bool includeDataForRemoteSharing = false)
	{
		PegasusLettuce.LettuceTeam result = null;
		if (src == null)
		{
			return result;
		}
		result = new PegasusLettuce.LettuceTeam();
		result.TeamId = src.ID;
		result.Name = src.Name;
		result.SortOrder = src.SortOrder;
		result.Type_ = src.TeamType;
		result.MercenaryList = new LettuceTeamMercenaryList();
		foreach (LettuceMercenary srcMerc in src.m_lettuceMercs)
		{
			LettuceTeamMercenary resultMerc = new LettuceTeamMercenary();
			resultMerc.MercenaryId = srcMerc.ID;
			LettuceMercenary.Loadout loadout = src.GetLoadout(srcMerc);
			if (loadout != null)
			{
				resultMerc.SelectedPortraitId = LettuceMercenary.GetPortraitIdFromArtVariation(loadout.m_artVariationRecord.ID, loadout.m_artVariationPremium);
				if (loadout.m_equipmentRecord != null)
				{
					resultMerc.SelectedEquipmentId = loadout.m_equipmentRecord.ID;
				}
			}
			if (includeDataForRemoteSharing)
			{
				resultMerc.SharedTeamMercenaryXp = srcMerc.m_experience;
				resultMerc.SharedTeamMercenaryIsFullyUpgraded = srcMerc.m_isFullyUpgraded;
			}
			result.MercenaryList.Mercenaries.Add(resultMerc);
		}
		return result;
	}
}
