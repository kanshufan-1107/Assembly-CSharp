using Hearthstone.DataModels;
using UnityEngine;

public class StaticWidgetTaskItemCache : StaticWidgetCache<MercenaryVillageTaskItemDataModel>
{
	private static StaticWidgetTaskItemCache m_instance;

	public static StaticWidgetTaskItemCache Get()
	{
		return m_instance;
	}

	private void Awake()
	{
		if (m_instance == null)
		{
			m_instance = this;
		}
		else
		{
			Object.DestroyImmediate(this);
		}
	}

	public override string GetUniqueIdentifier(MercenaryVillageTaskItemDataModel dataModel)
	{
		return dataModel.MercenaryCard.CardId;
	}
}
