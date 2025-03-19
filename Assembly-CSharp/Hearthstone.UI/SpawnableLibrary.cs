using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hearthstone.UI;

public class SpawnableLibrary : ScriptableObject
{
	public enum ItemType
	{
		Texture,
		Widget,
		Material,
		Sprite
	}

	[Serializable]
	public class ItemData
	{
		[SerializeField]
		private int m_id = -1;

		[SerializeField]
		private string m_name = string.Empty;

		[SerializeField]
		private ItemType m_itemType;

		[SerializeField]
		private string m_reference = string.Empty;

		[SerializeReference]
		private SpawnableLibraryItemParameters m_parameters;

		public int ID => m_id;

		public string Name => m_name;

		public ItemType ItemType => m_itemType;

		public string Reference => m_reference;

		public SpawnableLibraryItemParameters Parameters => m_parameters;
	}

	[SerializeField]
	private string m_baseMaterial;

	[SerializeField]
	private ItemData m_defaultItemData;

	[SerializeField]
	private List<ItemData> m_itemData = new List<ItemData>();

	public string BaseMaterial => m_baseMaterial;

	public bool HasDefault
	{
		get
		{
			if (m_defaultItemData != null)
			{
				return !string.IsNullOrEmpty(m_defaultItemData.Reference);
			}
			return false;
		}
	}

	public ItemData GetItemDataByID(int id)
	{
		for (int i = 0; i < m_itemData.Count; i++)
		{
			ItemData iconData = m_itemData[i];
			if (iconData.ID == id)
			{
				return iconData;
			}
		}
		return null;
	}

	public ItemData GetItemDataByName(string name)
	{
		for (int i = 0; i < m_itemData.Count; i++)
		{
			ItemData iconData = m_itemData[i];
			if (iconData.Name == name)
			{
				return iconData;
			}
		}
		return null;
	}

	public ItemData GetDefaultItemData()
	{
		if (!HasDefault)
		{
			return null;
		}
		return m_defaultItemData;
	}
}
