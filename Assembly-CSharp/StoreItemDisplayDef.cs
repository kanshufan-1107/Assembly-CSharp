using System;
using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class StoreItemDisplayDef : MonoBehaviour
{
	[Serializable]
	public class StoreVideoDisplay
	{
		[CustomEditField(T = EditType.TEXT_AREA)]
		public string Id;

		[CustomEditField(T = EditType.VIDEO)]
		public string VideoPath;

		[CustomEditField(T = EditType.TEXTURE)]
		public string FallbackTexturePath;

		[CustomEditField(ListTable = true)]
		public List<VideoCaptionKey> VideoCaptions;
	}

	[CustomEditField(Sections = "Video", T = EditType.GAME_OBJECT)]
	public string m_CutsceneSceneDef;

	[CustomEditField(Sections = "Video", T = EditType.GAME_OBJECT)]
	public string m_MultiHeroCutsceneSceneDef;

	[CustomEditField(Sections = "Video", HidePredicate = "ShouldUseCutsceneInsteadOfVideo")]
	public List<StoreVideoDisplay> m_StoreVideoDisplay;

	[CustomEditField(Sections = "Collection Manager Preview - 3D Loaded Assets", T = EditType.GAME_OBJECT)]
	public string m_CustomCMPortraitScene;

	[CustomEditField(Sections = "Collection Manager Preview - 3D Loaded Assets", T = EditType.GAME_OBJECT)]
	public string m_MultiHeroCMPortraitScene;

	[CustomEditField(Sections = "Collection Manager Preview - 2D Materials", T = EditType.MATERIAL)]
	public MaterialReference m_CustomCmPortraitMaterial;

	[CustomEditField(Sections = "Collection Manager Preview - 2D Materials", T = EditType.UBERANIMATION)]
	public string m_materialAnimationPath;

	private bool m_HasInitializedLookup;

	private readonly Dictionary<string, StoreVideoDisplay> m_VideoDisplayLookup = new Dictionary<string, StoreVideoDisplay>(StringComparer.OrdinalIgnoreCase);

	public bool ShouldUseCutsceneInsteadOfVideo()
	{
		return !string.IsNullOrEmpty(m_CutsceneSceneDef);
	}

	private void TryInitLookup()
	{
		if (m_HasInitializedLookup)
		{
			return;
		}
		if (m_StoreVideoDisplay != null)
		{
			foreach (StoreVideoDisplay svd in m_StoreVideoDisplay)
			{
				if (svd != null && !string.IsNullOrEmpty(svd.Id))
				{
					m_VideoDisplayLookup[svd.Id] = svd;
				}
			}
		}
		m_HasInitializedLookup = true;
	}

	public StoreVideoDisplay GetStoreVideoDisplay(string id)
	{
		TryInitLookup();
		if (string.IsNullOrEmpty(id) || !m_VideoDisplayLookup.ContainsKey(id))
		{
			return null;
		}
		return m_VideoDisplayLookup[id];
	}
}
