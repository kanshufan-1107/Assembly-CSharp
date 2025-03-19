using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public abstract class AdventureDefCacheBase<DefType, DefIDType, DbfRecord> where DefType : Component
{
	private Map<DefIDType, DefType> m_defCache = new Map<DefIDType, DefType>();

	public IEnumerable<DefType> Values => m_defCache.Values;

	public AdventureDefCacheBase(bool preloadRecords)
	{
		if (preloadRecords)
		{
			LoadFromRecords(GetRecords());
		}
	}

	public DefType GetDef(DefIDType defId)
	{
		DefType def = null;
		m_defCache.TryGetValue(defId, out def);
		return def;
	}

	public bool LoadDefForId(DefIDType defId)
	{
		if (m_defCache.ContainsKey(defId))
		{
			Debug.LogWarningFormat("Attempted to load a {0} that was already loaded for id {1}", typeof(DefType).Name, defId);
			return true;
		}
		DbfRecord record = GetRecordForDefId(defId);
		if (record != null && !string.IsNullOrEmpty(GetPrefabFromRecord(record)))
		{
			DefType def = GameUtils.LoadGameObjectWithComponent<DefType>(GetPrefabFromRecord(record));
			AddDef(def, record);
			return true;
		}
		return false;
	}

	private void LoadFromRecords(List<DbfRecord> records)
	{
		if (m_defCache.Count > 0)
		{
			Debug.LogErrorFormat("Attempting to load all {0} when they were already loaded!", typeof(DefType).Name);
		}
		foreach (DbfRecord record in records)
		{
			DefType def = GameUtils.LoadGameObjectWithComponent<DefType>(GetPrefabFromRecord(record));
			if (!(def == null))
			{
				AddDef(def, record);
			}
		}
	}

	private void AddDef(DefType def, DbfRecord record)
	{
		InitalizeDef(def, record);
		m_defCache.Add(GetDefId(def), def);
	}

	protected abstract List<DbfRecord> GetRecords();

	protected abstract string GetPrefabFromRecord(DbfRecord record);

	protected abstract void InitalizeDef(DefType def, DbfRecord record);

	protected abstract DefIDType GetDefId(DefType def);

	protected abstract DbfRecord GetRecordForDefId(DefIDType id);
}
