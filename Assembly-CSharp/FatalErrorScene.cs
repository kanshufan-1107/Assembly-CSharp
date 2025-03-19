using Blizzard.T5.Services;
using UnityEngine;

public class FatalErrorScene : PegasusScene
{
	protected override void Awake()
	{
		if (!AssetLoader.Get().InstantiatePrefab("FatalErrorScreen.prefab:b1524cacda5324547ac72995309dad14", OnLoadFatalErrorScreenAttempted))
		{
			OnLoadFatalErrorScreenAttempted("FatalErrorScreen.prefab:b1524cacda5324547ac72995309dad14", null, null);
		}
		base.Awake();
		Navigation.Clear();
		if (ServiceManager.TryGet<Network>(out var network))
		{
			network.AppAbort();
		}
		UserAttentionManager.StartBlocking(UserAttentionBlocker.FATAL_ERROR_SCENE);
		if (DialogManager.Get() != null)
		{
			DialogManager.Get().ClearAllImmediately();
		}
		Camera[] allCameras = Camera.allCameras;
		for (int i = 0; i < allCameras.Length; i++)
		{
			FullScreenEffects fullScreenEffects = allCameras[i].GetComponent<FullScreenEffects>();
			if (!(fullScreenEffects == null))
			{
				fullScreenEffects.Disable();
			}
		}
	}

	private void Start()
	{
		SceneMgr.Get().NotifySceneLoaded();
	}

	private void OnDestroy()
	{
		UserAttentionManager.StopBlocking(UserAttentionBlocker.FATAL_ERROR_SCENE);
	}

	public override void Unload()
	{
		UserAttentionManager.StopBlocking(UserAttentionBlocker.FATAL_ERROR_SCENE);
	}

	private void OnLoadFatalErrorScreenAttempted(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			base.gameObject.AddComponent<FatalErrorDialog>();
		}
	}
}
