using Blizzard.T5.Core.Utils;
using UnityEngine;

namespace Hearthstone.UI;

public class ShowGameObjectStateAction : StateActionImplementation
{
	private bool m_loadSynchronously;

	public override void Run(bool loadSynchronously = false)
	{
		m_loadSynchronously = loadSynchronously;
		GetOverride(0).RegisterReadyListener(HandleReady);
	}

	private void HandleReady(object unused)
	{
		GetOverride(0).RemoveReadyOrInactiveListener(HandleReady);
		if (!GetOverride(0).Resolve(out var go))
		{
			Complete(success: false);
			return;
		}
		go.SetActive(value: true);
		if (m_loadSynchronously)
		{
			GameObjectUtils.WalkSelfAndChildren(go.transform, delegate(Transform t)
			{
				WidgetInstance component = t.GetComponent<WidgetInstance>();
				if (component != null)
				{
					component.Initialize();
					return false;
				}
				return !(t.GetComponent<WidgetTemplate>() != null);
			});
		}
		Complete(success: true);
	}
}
