using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class CollectionDeck_TwistHeroicDeckRibbonOverride : MonoBehaviour
{
	public MeshRenderer m_TopRibbon;

	public int m_TopRibbonMaterialIndex;

	public MeshRenderer m_BottomRibbon;

	public int m_BottomRibbonMaterialIndex;

	public Texture m_overrideRibbonTexture;

	private void Start()
	{
		if (RankMgr.IsCurrentTwistSeasonUsingHeroicDecks())
		{
			UpdateMainTexture(m_TopRibbon, m_TopRibbonMaterialIndex, m_overrideRibbonTexture);
			UpdateMainTexture(m_BottomRibbon, m_BottomRibbonMaterialIndex, m_overrideRibbonTexture);
		}
	}

	private void UpdateMainTexture(Renderer renderer, int materialIndexed, Texture mainTexture)
	{
		if (!(renderer == null) && !(mainTexture == null))
		{
			renderer.GetSharedMaterial(materialIndexed).mainTexture = mainTexture;
		}
	}
}
