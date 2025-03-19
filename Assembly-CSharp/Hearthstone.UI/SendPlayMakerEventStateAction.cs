using UnityEngine;

namespace Hearthstone.UI;

public class SendPlayMakerEventStateAction : StateActionImplementation
{
	public override void Run(bool loadSynchronously = false)
	{
		GetOverride(0).RegisterReadyListener(HandleReady);
	}

	private void HandleReady(object unused)
	{
		GetOverride(0).RemoveReadyOrInactiveListener(HandleReady);
		string eventName = GetString(0);
		bool found = false;
		if (GetOverride(0).Resolve(out var target))
		{
			Component[] components = target.GetComponents<Component>();
			foreach (Component obj in components)
			{
				PlayMakerFSM fsm = obj as PlayMakerFSM;
				if (Application.IsPlaying(obj) && fsm != null)
				{
					found = true;
					fsm.SendEvent(eventName);
				}
			}
		}
		if (!found)
		{
			_ = Application.isPlaying;
		}
		Complete(success: true);
	}
}
