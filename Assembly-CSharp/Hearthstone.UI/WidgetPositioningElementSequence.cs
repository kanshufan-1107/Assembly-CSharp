using System.Collections.Generic;
using Hearthstone.UI.Core;
using UnityEngine;

namespace Hearthstone.UI;

public class WidgetPositioningElementSequence : WidgetPositioningElement
{
	[SerializeField]
	private List<WidgetPositioningElement> m_sequence;

	[Overridable]
	public bool TriggerRunSequence
	{
		get
		{
			return false;
		}
		set
		{
			if (value)
			{
				Refresh();
			}
		}
	}

	protected override void InternalRefresh()
	{
		foreach (WidgetPositioningElement element in m_sequence)
		{
			if (element != null && element != this)
			{
				element.Refresh();
			}
		}
	}
}
