using System;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.Core.Utils;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class LettuceMapCoin : MonoBehaviour
{
	public List<LettuceMapLine> m_ConnectionLines;

	public TooltipZone m_tooltipZone;

	public float m_tooltipScale = 5f;

	public AsyncReference m_RootVisualControllerReference;

	public AsyncReference[] m_CheckMarkContainerReference;

	private LettuceMap m_lettuceMap;

	private List<VisualController> m_checkmarkVisualControllers = new List<VisualController>();

	private int m_nextConnectionLineIndex;

	public int NodeId { get; private set; }

	private void Start()
	{
		LettuceMapCoinDataModel coinDataModel = GetMapCoinDataModel();
		m_lettuceMap = GameObjectUtils.FindComponentInParents<LettuceMap>(base.gameObject);
		if (m_lettuceMap != null)
		{
			m_lettuceMap.RegisterCoin(this, coinDataModel);
		}
		m_checkmarkVisualControllers.Clear();
		AsyncReference[] checkMarkContainerReference = m_CheckMarkContainerReference;
		for (int i = 0; i < checkMarkContainerReference.Length; i++)
		{
			checkMarkContainerReference[i].RegisterReadyListener<VisualController>(OnCheckmarkReadyReady);
		}
		m_RootVisualControllerReference.RegisterReadyListener<VisualController>(OnRootReady);
		m_ConnectionLines = m_ConnectionLines.OrderBy((LettuceMapLine a) => Guid.NewGuid()).ToList();
		if (coinDataModel != null)
		{
			NodeId = coinDataModel.Id;
		}
	}

	public void OnCheckmarkReadyReady(VisualController visualController)
	{
		m_checkmarkVisualControllers.Add(visualController);
	}

	public void OnRootReady(VisualController visualController)
	{
		visualController.Owner.RegisterEventListener(OnMapCoinEvent);
	}

	public void OnMapCoinEvent(string eventName)
	{
		if (!(eventName == "ON_MOUSE_OVER"))
		{
			if (eventName == "ON_MOUSE_OUT")
			{
				HideTooltip();
			}
		}
		else
		{
			ShowTooltip();
		}
	}

	public void DrawLineToObjectOnNextRow(GameObject destination, int currentConnectionIndex, int numConnectionsComingFromLeft, int numConnectionsComingFromRight)
	{
		if (m_nextConnectionLineIndex >= m_ConnectionLines.Count)
		{
			Debug.LogError("LettuceMapCoin.DrawLineToObjectOnNextRow() - Not enough lines! Tried to draw too many!");
			return;
		}
		LettuceMapLine lettuceMapLine = m_ConnectionLines[m_nextConnectionLineIndex];
		m_nextConnectionLineIndex++;
		lettuceMapLine.gameObject.SetActive(value: true);
		lettuceMapLine.m_StartBone = base.transform;
		lettuceMapLine.m_EndBone = destination.transform;
		lettuceMapLine.m_ConnectionType = LettuceMapLine.ConnectionType.NEXT_ROW;
		lettuceMapLine.m_ConnectionIndex = currentConnectionIndex;
		lettuceMapLine.m_NumParentConnectionsComingFromLeft = numConnectionsComingFromLeft;
		lettuceMapLine.m_NumParentConnectionsComingFromRight = numConnectionsComingFromRight;
		lettuceMapLine.RefreshLine();
	}

	public void DrawLineToObjectOnSameRow(GameObject destination)
	{
		if (m_nextConnectionLineIndex >= m_ConnectionLines.Count)
		{
			Debug.LogError("LettuceMapCoin.DrawLineToObjectOnSameRow() - Not enough lines! Tried to draw too many!");
			return;
		}
		LettuceMapLine lettuceMapLine = m_ConnectionLines[m_nextConnectionLineIndex];
		m_nextConnectionLineIndex++;
		lettuceMapLine.gameObject.SetActive(value: true);
		lettuceMapLine.m_StartBone = base.transform;
		lettuceMapLine.m_EndBone = destination.transform;
		lettuceMapLine.m_ConnectionType = LettuceMapLine.ConnectionType.SAME_ROW;
		lettuceMapLine.RefreshLine();
	}

	public void FlashCheckMark()
	{
		foreach (VisualController checkmark in m_checkmarkVisualControllers)
		{
			if (checkmark != null)
			{
				checkmark.SetState("FLASH_CHECK_MARK");
			}
		}
	}

	public LettuceMapCoinDataModel GetMapCoinDataModel()
	{
		Widget coinWidget = GetComponent<Widget>();
		if (coinWidget == null)
		{
			Debug.LogError("GetMapCoinDataModel() - Coin had no widget!");
			return null;
		}
		return coinWidget.GetDataModel<LettuceMapCoinDataModel>();
	}

	private void ShowTooltip()
	{
		LettuceMapCoinDataModel dataModel = GetMapCoinDataModel();
		if (dataModel != null && dataModel.GrantedAnomalyCard == null && m_tooltipZone != null)
		{
			m_tooltipZone.ShowTooltip(dataModel.HoverTooltipHeader, dataModel.HoverTooltipBody, m_tooltipScale);
		}
	}

	private void HideTooltip()
	{
		if (m_tooltipZone != null)
		{
			m_tooltipZone.HideTooltip();
		}
	}
}
