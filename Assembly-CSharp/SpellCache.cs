using System.Collections.Generic;
using UnityEngine;

internal class SpellCache
{
	private Dictionary<string, SpellTable> m_spellTableCache = new Dictionary<string, SpellTable>();

	private GameObject m_sceneObject;

	private GameObject SceneObject
	{
		get
		{
			if (m_sceneObject == null)
			{
				m_sceneObject = new GameObject("SpellCacheSceneObject", typeof(HSDontDestroyOnLoad));
				m_sceneObject.SetActive(value: false);
			}
			return m_sceneObject;
		}
	}

	public SpellTable GetSpellTable(string prefabPath)
	{
		if (!m_spellTableCache.TryGetValue(prefabPath, out var table))
		{
			return LoadSpellTable(prefabPath);
		}
		return table;
	}

	public void Clear()
	{
		foreach (KeyValuePair<string, SpellTable> item in m_spellTableCache)
		{
			item.Value.ReleaseAllSpells();
		}
		m_spellTableCache.Clear();
	}

	private SpellTable LoadSpellTable(string prefabPath)
	{
		GameObject go = AssetLoader.Get().InstantiatePrefab(prefabPath, AssetLoadingOptions.IgnorePrefabPosition);
		if (go == null)
		{
			Error.AddDevFatal("SpellCache.LoadSpellTable() - SpellCache GameObject failed to load");
			return null;
		}
		SpellTable spellTable = go.GetComponent<SpellTable>();
		if (spellTable == null)
		{
			Error.AddDevFatal("SpellCache.LoadSpellTable() - SpellCache has no SpellTable component ");
			return null;
		}
		spellTable.transform.parent = SceneObject.transform;
		m_spellTableCache.Add(prefabPath, spellTable);
		return spellTable;
	}
}
