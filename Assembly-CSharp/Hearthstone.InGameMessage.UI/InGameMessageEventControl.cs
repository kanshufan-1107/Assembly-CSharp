using System;

namespace Hearthstone.InGameMessage.UI;

public class InGameMessageEventControl : IInGameMessageEventControl, IDisposable
{
	private Action<PopupEvent> OnTriggerEventCallback { get; set; }

	public void Initialize(Action<PopupEvent> onTriggerEventCallback)
	{
		OnTriggerEventCallback = onTriggerEventCallback;
		SceneMgr.Get().RegisterSceneLoadedEvent(OnSceneLoaded);
	}

	public void Dispose()
	{
		SceneMgr.Get()?.UnregisterSceneLoadedEvent(OnSceneLoaded);
	}

	private void OnSceneLoaded(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
		switch (mode)
		{
		case SceneMgr.Mode.HUB:
			OnTriggerEventCallback?.Invoke(PopupEvent.OnHubScene);
			break;
		case SceneMgr.Mode.TOURNAMENT:
			OnTriggerEventCallback?.Invoke(PopupEvent.OnRankedPlayHub);
			break;
		case SceneMgr.Mode.BACON:
			OnTriggerEventCallback?.Invoke(PopupEvent.OnBGLobby);
			break;
		}
	}
}
