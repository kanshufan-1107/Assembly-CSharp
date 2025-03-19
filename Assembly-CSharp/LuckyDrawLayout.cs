using System;
using System.Collections.Generic;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class LuckyDrawLayout : MonoBehaviour
{
	private Widget m_widget;

	private PlayMakerFSM m_playmaker;

	public List<WidgetInstance> m_commonRewardsList;

	public List<WidgetInstance> m_legendaryRewardsList;

	private List<WidgetInstance> m_unownedRewardList;

	private int m_rewardTileToAnimate;

	private int m_maxTilesToAnimate;

	private List<WidgetInstance> m_fullRewardList;

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		if (m_widget == null)
		{
			Error.AddDevWarning("Error", "[LuckyDrawLayout] Awake() no Widget Template found for {0}", base.gameObject.name);
			return;
		}
		m_playmaker = GetComponent<PlayMakerFSM>();
		if (m_playmaker == null)
		{
			Error.AddDevWarning("Error", "[LuckyDrawLayout] Awake() no PlaymakerFSM found for {0}", base.gameObject.name);
		}
		else
		{
			m_fullRewardList = new List<WidgetInstance>(m_commonRewardsList);
			m_fullRewardList.AddRange(m_legendaryRewardsList);
			m_unownedRewardList = new List<WidgetInstance>();
		}
	}

	public void InitializeRewardTileWidgets(DataModelList<LuckyDrawRewardDataModel> rewardList)
	{
		int nextCommonTile = 0;
		int nextLegendaryTile = 0;
		foreach (LuckyDrawRewardDataModel reward in rewardList)
		{
			LuckyDrawRewardDataModel nextTileDM;
			if ((nextTileDM = reward).Style == LuckyDrawStyle.COMMON)
			{
				if (m_commonRewardsList.Count <= nextCommonTile)
				{
					Error.AddDevWarning("Error", "[LuckyDrawLayout] InitializeRewardTileWidget() Reward List has more common items than available slots! was the HE2 data setup properly?");
					continue;
				}
				WidgetInstance tileWidget = m_commonRewardsList[nextCommonTile];
				SetupTileWidget(nextTileDM, tileWidget);
				nextCommonTile++;
			}
			else if (m_legendaryRewardsList.Count <= nextLegendaryTile)
			{
				Error.AddDevWarning("Error", "[LuckyDrawLayout] InitializeRewardTileWidget() Reward List has more legendary items than available slots! Was the HE2 data setup properly?");
			}
			else
			{
				WidgetInstance tileWidget2 = m_legendaryRewardsList[nextLegendaryTile];
				SetupTileWidget(nextTileDM, tileWidget2);
				nextLegendaryTile++;
			}
		}
		if (nextCommonTile < m_commonRewardsList.Count)
		{
			Error.AddDevWarning("Error", "[LuckyDrawLayout] InitializeRewardTileWidget() Common reward list not fully filled. The number of common rewards does not match the reward list length and tiles will be empty! Was the HE2 data setup properly?");
		}
		if (nextLegendaryTile < m_legendaryRewardsList.Count)
		{
			Error.AddDevWarning("Error", "[LuckyDrawLayout] InitailizeRewardTileWidget() Legendary reward list not fully filled. The number of legendary rewards does not match the reward list length and tiles will be empty! Was the HE2 data setup properly?");
		}
	}

	public Vector3 GetWorldPositionOfTile(int tileNumber)
	{
		return m_fullRewardList[tileNumber].transform.position;
	}

	private void SetupTileWidget(LuckyDrawRewardDataModel rewardItem, WidgetInstance rewardWidget)
	{
		if (rewardWidget == null)
		{
			Error.AddDevWarning("UI Error", "[LuckyDrawLayout] SetupTileWidget() rewardWidget was null! Cant setup rewardTile.");
			return;
		}
		if (rewardItem == null)
		{
			Error.AddDevWarning("UI Error", "[LuckyDrawLayout] SetupTileWidget() rewardItem was null! No data to setup rewardTile");
			return;
		}
		rewardWidget.BindDataModel(rewardItem);
		rewardWidget.BindDataModel(rewardItem.RewardList);
	}

	public void AnimateTiles()
	{
		if (m_playmaker == null)
		{
			Error.AddDevWarning("UI Error", "[LuckyDrawLayout] AnimateTiles() playmaker not found! Cant do animation!");
			return;
		}
		PopulateUnownedRewardList();
		m_maxTilesToAnimate = m_playmaker.FsmVariables.GetFsmInt("MaxNumberTilesToAnimate").Value;
		float lowRewardSpeedMultiplier = m_playmaker.FsmVariables.GetFsmFloat("LowThresholdAnimationMultiplier").Value;
		float normalUpTiming = m_playmaker.FsmVariables.GetFsmFloat("TileUpTime").Value;
		float normalDownTiming = m_playmaker.FsmVariables.GetFsmFloat("TileDownTime").Value;
		m_playmaker.FsmVariables.GetFsmFloat("LowThresholdTileUpTiming").Value = normalUpTiming * lowRewardSpeedMultiplier;
		m_playmaker.FsmVariables.GetFsmFloat("LowThresholdTileDownTiming").Value = normalDownTiming * lowRewardSpeedMultiplier;
		int lowThresholdValue = m_playmaker.FsmVariables.GetFsmInt("LowBoxThreshold").Value;
		m_playmaker.FsmVariables.GetFsmBool("LowTileThresholdReached").Value = m_unownedRewardList.Count <= lowThresholdValue;
		ShuffleUnownedRewardList();
		m_rewardTileToAnimate = 0;
		AnimateNextTile();
	}

	public void AnimateNextTile()
	{
		if (m_rewardTileToAnimate > m_maxTilesToAnimate || m_rewardTileToAnimate >= m_unownedRewardList.Count)
		{
			m_playmaker.SendEvent("All_Finished");
			return;
		}
		m_playmaker.FsmVariables.GetFsmGameObject("TargetTile").Value = m_unownedRewardList[m_rewardTileToAnimate].gameObject;
		m_playmaker.FsmVariables.GetFsmVector3("TileInitialPosition").Value = m_unownedRewardList[m_rewardTileToAnimate].transform.position;
		m_playmaker.SendEvent("Animate_Tile");
		m_rewardTileToAnimate++;
	}

	public void PlayTileSmashAnim(int tileNumber)
	{
		m_fullRewardList[tileNumber].GetComponentInChildren<PlayMakerFSM>().SendEvent("Code_Box_Smashed");
		SetTileOwned(tileNumber);
	}

	private void PopulateUnownedRewardList()
	{
		m_unownedRewardList.Clear();
		foreach (WidgetInstance tile in m_fullRewardList)
		{
			if (tile.GetDataModel(667, out var datamodel) && !(datamodel as LuckyDrawRewardDataModel).IsOwned)
			{
				m_unownedRewardList.Add(tile);
			}
		}
	}

	private void ShuffleUnownedRewardList()
	{
		System.Random rng = new System.Random();
		for (int i = m_unownedRewardList.Count - 1; i > 0; i--)
		{
			int newPosition = rng.Next(i + 1);
			WidgetInstance targetItem = m_unownedRewardList[newPosition];
			m_unownedRewardList[newPosition] = m_unownedRewardList[i];
			m_unownedRewardList[i] = targetItem;
		}
	}

	public int GetTileFromRewardID(int rewardID)
	{
		for (int i = 0; i < m_fullRewardList.Count; i++)
		{
			LuckyDrawRewardDataModel targetDataModel = m_fullRewardList[i].Widget.GetComponent<LuckyDrawTile>().GetBoundRewardDataModel();
			if (targetDataModel != null && targetDataModel.RewardID == rewardID)
			{
				return i;
			}
		}
		return -1;
	}

	public void SetTileOwned(int tileNumber)
	{
		if (m_fullRewardList[tileNumber].GetDataModel(667, out var dataModel))
		{
			LuckyDrawRewardDataModel rewardData = dataModel as LuckyDrawRewardDataModel;
			rewardData.IsOwned = true;
			RemoveTileFromUnownedTileList(rewardData);
			LuckyDrawManager.Get()?.OnLuckyDrawHammerAnimationFinished();
		}
	}

	private void RemoveTileFromUnownedTileList(LuckyDrawRewardDataModel targetData)
	{
		foreach (WidgetInstance tile in m_unownedRewardList)
		{
			if (tile.GetDataModel(667, out var dataModel) && (dataModel as LuckyDrawRewardDataModel).RewardID == targetData.RewardID)
			{
				m_unownedRewardList.Remove(tile);
				break;
			}
		}
	}
}
