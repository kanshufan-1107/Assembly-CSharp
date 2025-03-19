using System.Collections;
using Hearthstone;
using UnityEngine;

[CustomEditClass]
public class LettuceScene : BasicScene
{
	protected override void Start()
	{
		NetCache.NetCacheFeatures guardianVars = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		if (guardianVars != null && guardianVars.MercenariesEnableVillages)
		{
			if (LettuceVillageDataUtil.Initialized)
			{
				LettuceVillageDataUtil.RefreshDataIfNecessary();
			}
			else
			{
				CollectionManager.Get().StartInitialMercenaryLoadIfRequired();
				StartCoroutine(InitializeDataWhenInitialMercenaryDataIsReady());
			}
		}
		base.Start();
	}

	protected override IEnumerator NotifySceneLoadedWhenReady()
	{
		NetCache.NetCacheFeatures guardianVars = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		if (guardianVars != null && guardianVars.MercenariesEnableVillages)
		{
			while (!LettuceVillageDataUtil.Initialized)
			{
				m_isFinishedLoadingTimer += Time.unscaledDeltaTime;
				if (m_isFinishedLoadingTimer > 15f)
				{
					Error.AddFatal(FatalErrorReason.LOAD_SCENE_NETWORK_TIMEOUT, "GLOBAL_ERROR_NETWORK_DISCONNECT");
					yield break;
				}
				yield return null;
			}
		}
		yield return base.NotifySceneLoadedWhenReady();
	}

	protected IEnumerator InitializeDataWhenInitialMercenaryDataIsReady()
	{
		while (!CollectionManager.Get().IsLettuceLoaded())
		{
			yield return null;
		}
		LettuceVillageDataUtil.InitializeData();
	}

	private void OnDestroy()
	{
		HearthstoneApplication.Get()?.UnloadUnusedAssets();
	}
}
