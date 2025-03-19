using Blizzard.T5.Core.Utils;
using UnityEngine;

public class InputUtil
{
	public static bool IsMouseOnScreen()
	{
		if (UniversalInputManager.Get() == null)
		{
			return false;
		}
		Vector3 mousePosition = InputCollection.GetMousePosition();
		if (mousePosition.x >= 0f && mousePosition.x <= (float)Screen.width && mousePosition.y >= 0f)
		{
			return mousePosition.y <= (float)Screen.height;
		}
		return false;
	}

	public static bool IsPlayMakerMouseInputAllowed(GameObject go)
	{
		if (UniversalInputManager.Get() == null)
		{
			return false;
		}
		if (ShouldCheckGameplayForPlayMakerMouseInput(go))
		{
			GameState state = GameState.Get();
			if (state != null && state.IsMulliganManagerActive())
			{
				return false;
			}
			TargetReticleManager targetReticle = TargetReticleManager.Get();
			if (targetReticle != null && targetReticle.IsLocalArrowActive())
			{
				return false;
			}
		}
		return true;
	}

	private static bool ShouldCheckGameplayForPlayMakerMouseInput(GameObject go)
	{
		if (SceneMgr.Get() == null)
		{
			return false;
		}
		if (!SceneMgr.Get().IsInGame())
		{
			return false;
		}
		if (LoadingScreen.Get() != null && LoadingScreen.Get().IsPreviousSceneActive() && GameObjectUtils.FindComponentInThisOrParents<LoadingScreen>(go) != null)
		{
			return false;
		}
		if (GameObjectUtils.FindComponentInThisOrParents<BaseUI>(go) != null)
		{
			return false;
		}
		return true;
	}
}
