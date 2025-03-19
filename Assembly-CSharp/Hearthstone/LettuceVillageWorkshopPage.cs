using System.Collections.Generic;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone;

public class LettuceVillageWorkshopPage : MonoBehaviour
{
	public Widget[] SlotWidgets;

	private static readonly MercenaryVillageWorkshopItemDataModel s_emptySlotModel = new MercenaryVillageWorkshopItemDataModel
	{
		ShowEmptySlot = true
	};

	public int NumSlots => SlotWidgets.Length;

	public void PrewarmItems()
	{
		Widget[] slotWidgets = SlotWidgets;
		for (int i = 0; i < slotWidgets.Length; i++)
		{
			slotWidgets[i].gameObject.SetActive(value: true);
		}
	}

	public void BindDataModel(List<MercenaryVillageWorkshopItemDataModel> itemList)
	{
		for (int i = 0; i < SlotWidgets.Length; i++)
		{
			SlotWidgets[i].BindDataModel((i < itemList.Count) ? itemList[i] : s_emptySlotModel);
		}
	}
}
