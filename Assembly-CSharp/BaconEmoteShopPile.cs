using Assets;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class BaconEmoteShopPile : MonoBehaviour
{
	[SerializeField]
	private Widget m_topEmote;

	[SerializeField]
	private Widget m_widget;

	private void Start()
	{
		m_widget.RegisterEventListener(delegate(string eventName)
		{
			if (eventName == "ORDER_PILE")
			{
				DataModelList<BattlegroundsEmoteDataModel> bGEmotePile = m_widget.GetDataModel<RewardItemDataModel>().BGEmotePile;
				bGEmotePile.Sort(CompareBorders);
				m_topEmote.BindDataModel(bGEmotePile[0]);
				m_topEmote.TriggerEvent("DEFAULT_BOTTOM_LEFT");
				m_widget.TriggerEvent("2_NO_BUBBLE");
				if (bGEmotePile.Count > 1)
				{
					switch (bGEmotePile[1].BorderType)
					{
					case BattlegroundsEmote.Bordertype.PURPLE:
						m_widget.TriggerEvent("2_PURPLE");
						break;
					case BattlegroundsEmote.Bordertype.NONE:
						m_widget.TriggerEvent("2_NO_BORDER");
						break;
					default:
						Debug.LogError("Unimplemented border selected.");
						m_widget.TriggerEvent("2_NO_BORDER");
						break;
					}
				}
				m_widget.TriggerEvent("3_NO_BUBBLE");
				if (bGEmotePile.Count > 2)
				{
					switch (bGEmotePile[2].BorderType)
					{
					case BattlegroundsEmote.Bordertype.PURPLE:
						m_widget.TriggerEvent("3_PURPLE");
						break;
					case BattlegroundsEmote.Bordertype.NONE:
						m_widget.TriggerEvent("3_NO_BORDER");
						break;
					default:
						Debug.LogError("Unimplemented border selected.");
						m_widget.TriggerEvent("3_NO_BORDER");
						break;
					}
				}
			}
		});
	}

	private static int CompareBorders(BattlegroundsEmoteDataModel x, BattlegroundsEmoteDataModel y)
	{
		return y.BorderType.CompareTo(x.BorderType);
	}
}
