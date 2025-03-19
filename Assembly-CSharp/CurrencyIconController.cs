using Hearthstone.UI;
using Hearthstone.UI.Core;
using UnityEngine;

public class CurrencyIconController : MonoBehaviour
{
	public Widget m_Widget;

	[Overridable]
	public CurrencyType CurrencyType
	{
		set
		{
			if (!(m_Widget == null))
			{
				switch (value)
				{
				case CurrencyType.NONE:
				case CurrencyType.REAL_MONEY:
					m_Widget.TriggerEvent("NONE");
					break;
				case CurrencyType.GOLD:
					m_Widget.TriggerEvent("GOLD");
					break;
				case CurrencyType.DUST:
					m_Widget.TriggerEvent("DUST");
					break;
				case CurrencyType.CN_RUNESTONES:
				case CurrencyType.ROW_RUNESTONES:
					m_Widget.TriggerEvent("RUNESTONE_CURRENCY_SMALL");
					break;
				case CurrencyType.CN_ARCANE_ORBS:
					m_Widget.TriggerEvent("ARCANEORBS_CURRENCY_SMALL");
					break;
				case CurrencyType.RENOWN:
					m_Widget.TriggerEvent("RENOWN");
					break;
				}
			}
		}
	}
}
