namespace Hearthstone.UI;

public class HideGameObjectStateAction : StateActionImplementation
{
	public override void Run(bool loadSynchronously = false)
	{
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
		go.SetActive(value: false);
		Complete(success: true);
	}
}
