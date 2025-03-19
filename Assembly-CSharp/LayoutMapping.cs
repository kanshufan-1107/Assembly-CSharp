using System;
using System.Collections.Generic;
using UnityEngine;

public class LayoutMapping
{
	private string m_sectionName;

	private int[,] m_slotMappings;

	private HashSet<int> m_failedToMapSlots = new HashSet<int>();

	public int SingleUnitWidth { get; } = -1;

	public int SingleUnitHeight { get; } = -1;

	public int TotalUnitWidth { get; } = -1;

	public int TotalUnitHeight { get; } = -1;

	public int LayoutCount { get; }

	public int ItemsPerRow { get; private set; }

	public List<LayoutMapEntry> SlotData { get; private set; } = new List<LayoutMapEntry>();

	public LayoutMapping(string name, int unitWidth, int unitHeight, IEnumerable<Tuple<int, int>> slots, int layoutCount)
	{
		m_sectionName = name;
		SingleUnitWidth = Mathf.Max(1, unitWidth);
		SingleUnitHeight = 1;
		LayoutCount = Mathf.Max(1, layoutCount);
		TotalUnitWidth = SingleUnitWidth;
		TotalUnitHeight = SingleUnitHeight * LayoutCount;
		SlotData = new List<LayoutMapEntry>();
		foreach (Tuple<int, int> slot in slots)
		{
			SlotData.Add(new LayoutMapEntry
			{
				UnitWidth = Mathf.Clamp(slot.Item1, 1, SingleUnitWidth),
				UnitHeight = Mathf.Clamp(slot.Item2, 1, SingleUnitHeight)
			});
		}
		m_slotMappings = new int[TotalUnitWidth, TotalUnitHeight];
		RefreshMappings();
		CalculateItemsPerRow();
	}

	public int GetNumberOfSingleSlots()
	{
		return SlotData.Count / LayoutCount;
	}

	public int GetNumberOfTotalSlots()
	{
		return SlotData.Count;
	}

	private void RefreshMappings()
	{
		m_failedToMapSlots.Clear();
		for (int x = 0; x < SingleUnitWidth; x++)
		{
			for (int y = 0; y < SingleUnitHeight; y++)
			{
				m_slotMappings[x, y] = -1;
			}
		}
		for (int slotIndex = 0; slotIndex < SlotData.Count; slotIndex++)
		{
			LayoutMapEntry shopSlotData = SlotData[slotIndex];
			if (!TryAddSlot(shopSlotData, slotIndex, out var _, out var _))
			{
				m_failedToMapSlots.Add(slotIndex);
				Log.Store.PrintWarning("Unable to add slot data of " + shopSlotData.ToString() + " into section " + m_sectionName);
			}
		}
		if (LayoutCount <= 1)
		{
			return;
		}
		int duplicateCount = LayoutCount - 1;
		List<LayoutMapEntry> baseSlotData = new List<LayoutMapEntry>(SlotData);
		for (int duplicateOffset = 0; duplicateOffset < duplicateCount; duplicateOffset++)
		{
			int yOffset = (duplicateOffset + 1) * SingleUnitHeight;
			foreach (LayoutMapEntry item in baseSlotData)
			{
				LayoutMapEntry clone = item.Clone();
				clone.OriginY += yOffset;
				SlotData.Add(clone);
			}
			for (int xBase = 0; xBase < SingleUnitWidth; xBase++)
			{
				for (int yBase = 0; yBase < SingleUnitHeight; yBase++)
				{
					m_slotMappings[xBase, yBase + yOffset] = m_slotMappings[xBase, yBase];
				}
			}
		}
	}

	private void CalculateItemsPerRow()
	{
		foreach (LayoutMapEntry slot in SlotData)
		{
			if (slot.OriginX == 0 && slot.OriginY != 0)
			{
				break;
			}
			ItemsPerRow++;
		}
	}

	private bool TryAddSlot(LayoutMapEntry shopSlotData, int slotIndex, out int originX, out int originY)
	{
		originX = -1;
		originY = -1;
		if (slotIndex < 0)
		{
			return false;
		}
		int mappingXLength = SingleUnitWidth;
		int mappingYLength = SingleUnitHeight;
		for (int mapX = 0; mapX < mappingXLength; mapX++)
		{
			for (int mapY = 0; mapY < mappingYLength; mapY++)
			{
				if (DoesSlotFitAtOrigin(shopSlotData, mapX, mapY))
				{
					originX = mapX;
					originY = mapY;
					break;
				}
			}
			if (originX >= 0 && originY >= 0)
			{
				break;
			}
		}
		if (originX >= 0 && originY >= 0)
		{
			for (int i = 0; i < shopSlotData.UnitWidth; i++)
			{
				for (int j = 0; j < shopSlotData.UnitHeight; j++)
				{
					int xToFill = i + originX;
					int yToFill = j + originY;
					if (xToFill >= mappingXLength || yToFill >= mappingYLength)
					{
						return false;
					}
					m_slotMappings[xToFill, yToFill] = slotIndex;
				}
			}
			shopSlotData.OriginX = originX;
			shopSlotData.OriginY = originY;
			return true;
		}
		return false;
	}

	private bool DoesSlotFitAtOrigin(LayoutMapEntry shopSlotData, int originX, int originY)
	{
		int mappingXLength = SingleUnitWidth;
		int mappingYLength = SingleUnitHeight;
		for (int slotSizeX = 0; slotSizeX < shopSlotData.UnitWidth; slotSizeX++)
		{
			for (int slotSizeY = 0; slotSizeY < shopSlotData.UnitHeight; slotSizeY++)
			{
				int slotCheckX = originX + slotSizeX;
				if (slotCheckX >= mappingXLength)
				{
					return false;
				}
				int slotCheckY = originY + slotSizeY;
				if (slotCheckY >= mappingYLength)
				{
					return false;
				}
				if (m_slotMappings[slotCheckX, slotCheckY] >= 0)
				{
					return false;
				}
			}
		}
		return true;
	}
}
