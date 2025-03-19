using System;
using System.Collections;
using Blizzard.T5.Services;
using Hearthstone.Util.Services;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CosmeticPreviewSceneLoader : MonoBehaviour
{
	private const string PREVIEW_SCENE_NAME = "BaconCosmeticPreview";

	public BaconCosmeticPreviewRunnerConfig m_config;

	private bool m_loadedPreviewScene;

	private void Awake()
	{
		BaconCosmeticPreviewTester.StopCosmetic += ReloadScene;
		BaconCosmeticPreviewTester.LoadSceneFromEditor = (Action)Delegate.Combine(BaconCosmeticPreviewTester.LoadSceneFromEditor, new Action(LoadScene));
	}

	private void ReloadScene()
	{
		if (m_loadedPreviewScene)
		{
			StartCoroutine(ReloadSceneRoutine());
		}
	}

	private IEnumerator ReloadSceneRoutine()
	{
		UnloadScene();
		while (SceneManager.GetSceneByName("BaconCosmeticPreview").isLoaded)
		{
			yield return null;
		}
		LoadScene();
	}

	public void LoadScene()
	{
		StartCoroutine(LoadSceneCoroutine());
	}

	public void UnloadScene()
	{
		if (SceneManager.GetSceneByName("BaconCosmeticPreview").isLoaded)
		{
			SceneManager.UnloadSceneAsync("BaconCosmeticPreview");
			m_loadedPreviewScene = false;
		}
	}

	public bool CosmeticsRenderingEnabled()
	{
		return NetCache.Get()?.GetNetObject<NetCache.NetCacheFeatures>()?.Collection?.CosmeticsRenderingEnabled == true;
	}

	private IEnumerator LoadSceneCoroutine()
	{
		ServiceManager.InitializeDynamicServicesIfNeeded(out var serviceDependencies, DynamicServiceSets.UberText());
		ServiceManager.InitializeDynamicServicesIfNeeded(out serviceDependencies, typeof(IAssetLoader), typeof(IAliasedAssetResolver), typeof(SpellManager));
		if (!SceneManager.GetSceneByName("BaconCosmeticPreview").isLoaded)
		{
			AsyncOperation loadOp = SceneManager.LoadSceneAsync("BaconCosmeticPreview", LoadSceneMode.Additive);
			while (!loadOp.isDone)
			{
				yield return null;
			}
			m_loadedPreviewScene = true;
		}
	}

	private void OnDestroy()
	{
		BaconCosmeticPreviewTester.LoadSceneFromEditor = (Action)Delegate.Remove(BaconCosmeticPreviewTester.LoadSceneFromEditor, new Action(LoadScene));
		BaconCosmeticPreviewTester.StopCosmetic -= ReloadScene;
		if (m_loadedPreviewScene)
		{
			UnloadScene();
			m_loadedPreviewScene = false;
		}
	}
}
