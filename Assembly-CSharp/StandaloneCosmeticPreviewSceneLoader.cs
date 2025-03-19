using Hearthstone.UI;
using UnityEngine;

public class StandaloneCosmeticPreviewSceneLoader : MonoBehaviour
{
	public AsyncReference m_cosmeticPreviewSceneLoaderRef;

	private static CosmeticPreviewSceneLoader m_cosmeticPreviewSceneLoader;

	private void Awake()
	{
		m_cosmeticPreviewSceneLoaderRef.RegisterReadyListener<CosmeticPreviewSceneLoader>(OnSceneLoaderReady);
	}

	private void OnSceneLoaderReady(CosmeticPreviewSceneLoader sceneLoader)
	{
		if (!(sceneLoader == null))
		{
			m_cosmeticPreviewSceneLoader = sceneLoader;
			m_cosmeticPreviewSceneLoader.LoadScene();
		}
	}

	private void OnDestroy()
	{
		if (!(m_cosmeticPreviewSceneLoader == null))
		{
			m_cosmeticPreviewSceneLoader.UnloadScene();
			m_cosmeticPreviewSceneLoader = null;
		}
	}

	public static bool CosmeticsRenderingEnabled()
	{
		if (m_cosmeticPreviewSceneLoader == null)
		{
			return false;
		}
		return m_cosmeticPreviewSceneLoader.CosmeticsRenderingEnabled();
	}
}
