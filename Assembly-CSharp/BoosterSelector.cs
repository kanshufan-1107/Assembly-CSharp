using System;
using System.Collections.Generic;
using Assets;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class BoosterSelector : MonoBehaviour
{
	[Serializable]
	public struct AssignedBooster
	{
		public Widget Widget;

		public RewardItem.BoosterSelector BoosterSelector;
	}

	[SerializeField]
	private List<AssignedBooster> m_boosters;

	private void Awake()
	{
		foreach (AssignedBooster booster in m_boosters)
		{
			PackDataModel packData = new PackDataModel();
			packData.Type = GameUtils.GetRewardableBoosterFromSelector(booster.BoosterSelector);
			packData.Quantity = 1;
			booster.Widget.BindDataModel(packData);
		}
	}
}
