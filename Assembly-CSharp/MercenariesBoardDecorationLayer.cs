using System;
using System.Collections.Generic;
using Blizzard.T5.Core.Utils;
using UnityEngine;

public class MercenariesBoardDecorationLayer : MonoBehaviour
{
	[Serializable]
	public enum DecorationPosition
	{
		INVALID,
		TOP_LEFT,
		TOP_RIGHT,
		BOTTOM_LEFT,
		BOTTOM_RIGHT,
		TOP_CENTER,
		BOTTOM_CENTER
	}

	[Serializable]
	public class WeightedCompatibleDecorationLayer
	{
		public MercenariesBoardDecorationLayer m_compatibleDecoration;

		public int m_weight;
	}

	[Serializable]
	public class DecorationObject
	{
		public DecorationPosition m_decorationPosition;

		public GameObject m_gameObject;
	}

	public List<DecorationObject> m_decorationObjects;

	public List<WeightedCompatibleDecorationLayer> m_compatibleDecorationLayers;

	public void HideAllDecorations()
	{
		foreach (DecorationObject decorationObject in m_decorationObjects)
		{
			decorationObject.m_gameObject.SetActive(value: false);
		}
	}

	public void SetDecorationVisible(DecorationPosition decorationPosition, bool allowStackingCompatibleDecorations = true)
	{
		foreach (DecorationObject decorationObject in m_decorationObjects)
		{
			if (decorationObject.m_decorationPosition != decorationPosition)
			{
				continue;
			}
			decorationObject.m_gameObject.SetActive(value: true);
			if (allowStackingCompatibleDecorations && m_compatibleDecorationLayers != null && m_compatibleDecorationLayers.Count > 0)
			{
				WeightedCompatibleDecorationLayer result = GeneralUtils.RollElementFromWeightedList(m_compatibleDecorationLayers, (WeightedCompatibleDecorationLayer e) => e.m_weight);
				if (result != null && result.m_compatibleDecoration != null)
				{
					result.m_compatibleDecoration.SetDecorationVisible(decorationPosition, allowStackingCompatibleDecorations: false);
				}
			}
		}
	}

	public void HideTopDecorations()
	{
		foreach (DecorationObject decorationObject in m_decorationObjects)
		{
			if (decorationObject.m_decorationPosition == DecorationPosition.TOP_LEFT || decorationObject.m_decorationPosition == DecorationPosition.TOP_RIGHT || decorationObject.m_decorationPosition == DecorationPosition.TOP_CENTER)
			{
				decorationObject.m_gameObject.SetActive(value: false);
			}
		}
	}
}
