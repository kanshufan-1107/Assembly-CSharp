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
		if (m_loadedPreviewScene && SceneManager.GetSceneByName("BaconCosmeticPreview").isLoaded)
		{
			SceneManager.UnloadSceneAsync("BaconCosmeticPreview");
			StartCoroutine(UnloadSceneCoroutine());
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
		if (m_loadedPreviewScene || !SceneManager.GetSceneByName("BaconCosmeticPreview").isLoaded)
		{
			SceneManager.LoadScene("BaconCosmeticPreview", LoadSceneMode.Additive);
			m_loadedPreviewScene = true;
		}
		yield break;
	}

	private IEnumerator UnloadSceneCoroutine()
	{
		yield return new WaitUntil(() => !SceneManager.GetSceneByName("BaconCosmeticPreview").isLoaded);
		m_loadedPreviewScene = false;
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
